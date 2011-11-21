//--------------------------------------------------------------------------------------
// GLOBAL VARS 
//--------------------------------------------------------------------------------------

//don't forget to set the Matrizes
float4x4 view;
float4x4 proj;
float4x4 world;

//Old Sampler2
Texture2D grass_diffuse01;
Texture2D grass_diffuse02;
Texture2D grass_diffuse03;
Texture2D grass_alpha;
Texture2D grass_noise;
Texture2D grass_shift;

//Misc
float cTexScal = 1;
float time;
float windPW=10;  // Value between 0-12
float3 winddir= float3 (0.5,1,0.5); // y always 1

//--------------------------------------------------------------------------------------
//LIGHTING VARIABLES
//--------------------------------------------------------------------------------------
//DirectionalLight
float4 l_color = float4 (1.0f,1.0f,1.0f,1.0f);
float4 l_dir = float3 (-1,-1,1,0);

//Material
	float mat_Ka, mat_Kd, mat_Ks, mat_A;

//lighting vars
float4 ambientLight= float4(1.0f,1.0f,1.0f,1.0f);
float3 eye;

//--------------------------------------------------------------------------------------
//RASTERIZER STATES
//--------------------------------------------------------------------------------------
RasterizerState rsSolid
{
	  FillMode = Solid;
	  CullMode = None;
	  FrontCounterClockwise = false;

};


//--------------------------------------------------------------------------------------
// FUNCTIONS
//--------------------------------------------------------------------------------------

//Texture Filtering
SamplerState ModelTextureSampler {
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Mirror;
    AddressV = Mirror;
};

//--------------------------------------------------------------------------------------
// Blinn-Phong Lighting Reflection Model
//--------------------------------------------------------------------------------------
float4 calcBlinnPhongLighting(float M_Ka, float M_Kd, float M_Ks, float M_A, float4 LColor, float3 N, float3 L, float3 H )
{	
	float4 Ia = M_Ka * ambientLight;
	float4 Id = M_Kd * saturate( dot(N,L) );
	float4 Is = M_Ks * pow( saturate(dot(N,H)), M_A );
	
	return Ia + (Id + Is) * LColor;
}

//--------------------------------------------------------------------------------------
// STRCUCTS
//--------------------------------------------------------------------------------------

//Vertexshader Input from Pipeline
struct VS_IN {
	float3 pos				: POSITION;
};

//For working in the Geometryshader
struct GS_WORKING {
	float3 pos;	
	float3 normal;	
	float2 texCoord;
	float4 random;
};

//Vertexshader Output & Pixelshader Input
struct PS_IN {
	float4 pos				: SV_POSITION;		
	float3 normalWS			: NORMAL;
	float2 texCoord			: TEXCOORD;
	float4 random			: RANDOM;
	float3 h 				: HVECTOR;
};

//--------------------------------------------------------------------------------------
// VERTEX SHADER 
//--------------------------------------------------------------------------------------

VS_IN VS(VS_IN input) {
	return input;
}

//--------------------------------------------------------------------------------------
// VERTEX SHADER called from Geometryshader as a normal function
//--------------------------------------------------------------------------------------

PS_IN VSreal( GS_WORKING input ) {
	PS_IN output = (PS_IN)0;
	
	float4x4 worldViewProj = mul(mul(world, view), proj);
	output.pos = mul(float4(input.pos, 1.0), worldViewProj);
	output.normalWS = mul(float4(input.normal, 1.0), world).xyz;
	output.texCoord = input.texCoord;
	output.random = input.random;	
	float3 V = normalize( eye - (float3) input.pos );
	output.h = normalize( -l_dir + V );	
	return output;
}

//--------------------------------------------------------------------------------------
// GEOMETRY SHADER
//--------------------------------------------------------------------------------------

[maxvertexcount(40)]
void GS(point VS_IN s[1],  inout TriangleStream<PS_IN> triStream)
{
if (s[0].pos.y > 0) { 
    GS_WORKING bl;
	GS_WORKING tl;
	GS_WORKING br;
	GS_WORKING tr;

	float2 texCoord = float2(s[0].pos.x, s[0].pos.y);

	float4 random = (grass_noise.SampleLevel(ModelTextureSampler, texCoord, 0));
	random.r=random.r-0.5;
	if (random.r < 0) {
	random.r = random.r*(-1);
	}
	random.r=random.r*2;

	bl.random = random;
	tl.random = random;
	br.random = random;
	tr.random = random;

	float dimension_x = (2+1*(random.g-0.5))/2;
	float dimension_y = 15+35*(1-(0.5*random.g));

	texCoord = float2((s[0].pos.x%100)/100, (s[0].pos.y%100)/100);
	float4 random2 = (grass_noise.SampleLevel(ModelTextureSampler, texCoord, 0));
	float4 shift = (grass_shift.SampleLevel(ModelTextureSampler, texCoord, 0));
	// Motion added with x^2 influence (between 0-1)
	float windpower = windPW*(((sin((time+random.r)+shift.rgb*3)+1)/2)+1);

	float turn = 2*(random2.b*random2.r*random2.g);
	s[0].pos.x = s[0].pos.x+4*random2.r;
	s[0].pos.z = s[0].pos.z-4*random2.r;

    float offsetX = winddir.x*windpower*(0.5+random2.r)*sin((time+random.r)+shift.rgb*3);

	float offsetY = -windpower*(0.5+random2.g)*sin((time+random.r)+shift.rgb*3);
	if (offsetY > 0) {
	offsetY = offsetY*(-1);
	}

	float offsetZ = winddir.y*windpower*(0.5+random2.b)*sin((time+random.r)+shift.rgb*3);

	int LOD = 2;

	if (LOD == 0) {

	//create gras // LOD = 0
	//--------------------------------------------

	//bottom left
	bl.pos = float3(s[0].pos.x - dimension_x/2, s[0].pos.y, s[0].pos.z);	
	bl.texCoord = float2(0, 0);
	
	//top left
	tl.pos = float3(s[0].pos.x - dimension_x/2 + offsetX, s[0].pos.y+dimension_y+ offsetY, s[0].pos.z + offsetZ);	
	tl.texCoord = float2(1, 0);

	//bottom right
	br.pos = float3(s[0].pos.x + dimension_x/2+turn, s[0].pos.y, s[0].pos.z+turn);	
	br.texCoord = float2(0, 1);

	//top right
	tr.pos = float3(s[0].pos.x + dimension_x/2+turn + offsetX, s[0].pos.y + dimension_y+ offsetY, s[0].pos.z + offsetZ+turn);	
	tr.texCoord = float2(1, 1);

	//Normals bl2tl = bottomleft to topleft (Distance)
	float3 a = br.pos - bl.pos;
	float3 b = tl.pos - bl.pos;

	bl.normal = -cross( a, b);
	tl.normal = cross( a,-b);
	br.normal = cross(-a, b);
	tr.normal = -cross(-a,-b);
	
	//Append
	triStream.Append(VSreal(bl));
	triStream.Append(VSreal(br));
	triStream.Append(VSreal(tl));
	triStream.Append(VSreal(tr));

	} else {
	if (LOD == 1) {

	dimension_y = dimension_y/3;

	//create gras // LOD = 1 Seg = 0
	//--------------------------------------------

	//bottom left
	bl.pos = float3(s[0].pos.x - dimension_x/2, s[0].pos.y, s[0].pos.z);	
	bl.texCoord = float2(0, 0);
	
	//top left
	tl.pos = float3(s[0].pos.x - dimension_x/2 + offsetX*0.11, s[0].pos.y+dimension_y+ offsetY*0.11, s[0].pos.z + offsetZ*0.11);	
	tl.texCoord = float2(0.33, 0);

	//bottom right
	br.pos = float3(s[0].pos.x + dimension_x/2+turn, s[0].pos.y, s[0].pos.z+turn);	
	br.texCoord = float2(0, 1);

	//top right
	tr.pos = float3(s[0].pos.x + dimension_x/2+turn + offsetX*0.11, s[0].pos.y + dimension_y+ offsetY*0.11, s[0].pos.z + offsetZ*0.11+turn);	
	tr.texCoord = float2(0.33, 1);

	//Normals bl2tl = bottomleft to topleft (Distance)
	float3 a = br.pos - bl.pos;
	float3 b = tl.pos - bl.pos;

	bl.normal = -cross( a, b);
	tl.normal = cross( a,-b);
	br.normal = cross(-a, b);
	tr.normal = -cross(-a,-b);
	
	//Append
	triStream.Append(VSreal(bl));
	triStream.Append(VSreal(br));
	triStream.Append(VSreal(tl));
	triStream.Append(VSreal(tr));

	//create gras // LOD = 1 Seg = 1
	//--------------------------------------------

	//bottom left
	bl.pos = tl.pos;	
	bl.texCoord = tl.texCoord;
	
	//top left
	tl.pos = float3(s[0].pos.x - dimension_x/2 + offsetX*0.44, s[0].pos.y+2*dimension_y+ offsetY*0.44, s[0].pos.z + offsetZ*0.44);	
	tl.texCoord = float2(0.66, 0);

	//bottom right
	br.pos = tr.pos;	
	br.texCoord = tr.texCoord;

	//top right
	tr.pos = float3(s[0].pos.x + dimension_x/2+turn + offsetX*0.44, s[0].pos.y +2*dimension_y+ offsetY*0.44, s[0].pos.z + offsetZ*0.44+turn);	
	tr.texCoord = float2(0.66, 1);

	//Normals bl2tl = bottomleft to topleft (Distance)
	a = br.pos - bl.pos;
	b = tl.pos - bl.pos;

	tl.normal = cross( a,-b);
	tr.normal = -cross(-a,-b);
	
	//Append
	triStream.Append(VSreal(tl));
	triStream.Append(VSreal(tr));

	//create gras // LOD = 1 Seg = 2
	//--------------------------------------------

	//bottom left
	bl.pos = tl.pos;	
	bl.texCoord = tl.texCoord;
	
	//top left
	tl.pos = float3(s[0].pos.x - dimension_x/2 + offsetX*1, s[0].pos.y+3*dimension_y+ offsetY*1, s[0].pos.z + offsetZ*1);	
	tl.texCoord = float2(1, 0);

	//bottom right
	br.pos = tr.pos;	
	br.texCoord = tr.texCoord;

	//top right
	tr.pos = float3(s[0].pos.x + dimension_x/2 +turn+ offsetX*1, s[0].pos.y + 3*dimension_y+ offsetY*1, s[0].pos.z + offsetZ*1+turn);	
	tr.texCoord = float2(1, 1);

	//Normals bl2tl = bottomleft to topleft (Distance)
	a = br.pos - bl.pos;
	b = tl.pos - bl.pos;

	tl.normal = cross( a,-b);
	tr.normal = -cross(-a,-b);
	
	//Append
	triStream.Append(VSreal(tl));
	triStream.Append(VSreal(tr));

	} else {

	dimension_y = dimension_y/5;

	//create gras // LOD = 2 Seg = 0
	//--------------------------------------------

	//bottom left
	bl.pos = float3(s[0].pos.x - dimension_x/2, s[0].pos.y, s[0].pos.z);	
	bl.texCoord = float2(0, 0);
	
	//top left
	tl.pos = float3(s[0].pos.x - dimension_x/2 + offsetX*0.04, s[0].pos.y+dimension_y+ offsetY*0.04, s[0].pos.z + offsetZ*0.04);	
	tl.texCoord = float2(0.2, 0);

	//bottom right
	br.pos = float3(s[0].pos.x + dimension_x/2+turn , s[0].pos.y, s[0].pos.z+turn);	
	br.texCoord = float2(0, 1);

	//top right
	tr.pos = float3(s[0].pos.x + dimension_x/2 + offsetX*0.04+turn, s[0].pos.y + dimension_y+ offsetY*0.04, s[0].pos.z + offsetZ*0.04+turn);	
	tr.texCoord = float2(0.2, 1);

	//Normals bl2tl = bottomleft to topleft (Distance)
	float3 a = br.pos - bl.pos;
	float3 b = tl.pos - bl.pos;

	bl.normal = -cross( a, b);
	tl.normal = cross( a,-b);
	br.normal = cross(-a, b);
	tr.normal = -cross(-a,-b);
	
	//Append
	triStream.Append(VSreal(bl));
	triStream.Append(VSreal(br));
	triStream.Append(VSreal(tl));
	triStream.Append(VSreal(tr));

	//create gras // LOD = 2 Seg = 1
	//--------------------------------------------

	//bottom left
	bl.pos = tl.pos;	
	bl.texCoord = tl.texCoord;
	
	//top left
	tl.pos = float3(s[0].pos.x - dimension_x/2 + offsetX*0.16, s[0].pos.y+2*dimension_y+ offsetY*0.16, s[0].pos.z + offsetZ*0.16);	
	tl.texCoord = float2(0.4, 0);

	//bottom right
	br.pos = tr.pos;	
	br.texCoord = tr.texCoord;

	//top right
	tr.pos = float3(s[0].pos.x + dimension_x/2+turn + offsetX*0.16, s[0].pos.y + 2*dimension_y+ offsetY*0.16, s[0].pos.z + offsetZ*0.16+turn);	
	tr.texCoord = float2(0.4, 1);

	//Normals bl2tl = bottomleft to topleft (Distance)
	a = br.pos - bl.pos;
	b = tl.pos - bl.pos;

	tl.normal = cross( a,-b);
	tr.normal = -cross(-a,-b);
	
	//Append
	triStream.Append(VSreal(tl));
	triStream.Append(VSreal(tr));

	//create gras // LOD = 2 Seg = 2
	//--------------------------------------------

	//bottom left
	bl.pos = tl.pos;	
	bl.texCoord = tl.texCoord;
	
	//top left
	tl.pos = float3(s[0].pos.x - dimension_x/2 + offsetX*0.36, s[0].pos.y+3*dimension_y+ offsetY*0.36, s[0].pos.z + offsetZ*0.36);	
	tl.texCoord = float2(0.6, 0);

	//bottom right
	br.pos = tr.pos;	
	br.texCoord = tr.texCoord;

	//top right
	tr.pos = float3(s[0].pos.x + dimension_x/2 +turn+ offsetX*0.36, s[0].pos.y + 3*dimension_y+ offsetY*0.36, s[0].pos.z + offsetZ*0.36+turn);	
	tr.texCoord = float2(0.6, 1);

	//Normals bl2tl = bottomleft to topleft (Distance)
	a = br.pos - bl.pos;
	b = tl.pos - bl.pos;


	tl.normal = cross( a,-b);
	tr.normal = -cross(-a,-b);
	
	//Append
	triStream.Append(VSreal(tl));
	triStream.Append(VSreal(tr));

	//create gras // LOD = 2 Seg = 3
	//--------------------------------------------

	//bottom left
	bl.pos = tl.pos;	
	bl.texCoord = tl.texCoord;
	
	//top left
	tl.pos = float3(s[0].pos.x - dimension_x/2 + offsetX*0.64, s[0].pos.y+4*dimension_y+ offsetY*0.64, s[0].pos.z + offsetZ*0.64);	
	tl.texCoord = float2(0.8, 0);

	//bottom right
	br.pos = tr.pos;	
	br.texCoord = tr.texCoord;

	//top right
	tr.pos = float3(s[0].pos.x + dimension_x/2+turn + offsetX*0.64, s[0].pos.y + 4*dimension_y+ offsetY*0.64, s[0].pos.z + offsetZ*0.64+turn);	
	tr.texCoord = float2(0.8, 1);

	//Normals bl2tl = bottomleft to topleft (Distance)
	a = br.pos - bl.pos;
	b = tl.pos - bl.pos;

	tl.normal = cross( a,-b);
	tr.normal = -cross(-a,-b);
	
	//Append
	triStream.Append(VSreal(tl));
	triStream.Append(VSreal(tr));

	//create gras // LOD = 2 Seg = 4
	//--------------------------------------------

	//bottom left
	bl.pos = tl.pos;	
	bl.texCoord = tl.texCoord;
	
	//top left
	tl.pos = float3(s[0].pos.x - dimension_x/2 + offsetX*1, s[0].pos.y+5*dimension_y+ offsetY*1, s[0].pos.z + offsetZ*1);	
	tl.texCoord = float2(1, 0);

	//bottom right
	br.pos = tr.pos;	
	br.texCoord = tr.texCoord;

	//top right
	tr.pos = float3(s[0].pos.x + dimension_x/2+turn + offsetX*1, s[0].pos.y + 5*dimension_y+ offsetY*1, s[0].pos.z + offsetZ*1+turn);	
	tr.texCoord = float2(1, 1);

	//Normals bl2tl = bottomleft to topleft (Distance)
	a = br.pos - bl.pos;
	b = tl.pos - bl.pos;

	tl.normal = cross( a,-b);
	tr.normal = -cross(-a,-b);
	
	//Append
	triStream.Append(VSreal(tl));
	triStream.Append(VSreal(tr));
	}
	}
	}

}

//--------------------------------------------------------------------------------------
// PER PIXEL LIGHTING 
//--------------------------------------------------------------------------------------

float4 PS_PIXEL_LIGHTING_BLINNPHONG( PS_IN input ) : SV_Target
{     	
	//renormalize interpolated vectors
	input.normalWS = normalize( input.normalWS );		
	input.h = normalize( input.h );
	
	//calculate lighting	
	float4 I = calcBlinnPhongLighting( mat_Ka, mat_Kd, mat_Ks, mat_A, l_color, input.normalWS, -l_dir, input.h );
	
	//with texturing
	float alphar = grass_alpha.Sample(ModelTextureSampler, input.texCoord).r;

	float3 tex = grass_diffuse01.Sample(ModelTextureSampler, input.texCoord).rgb*input.random.b+grass_diffuse02.Sample(ModelTextureSampler, input.texCoord).rgb*(1-input.random.b);

	float tag = (sin((time % 180)/10)+1)/2;
    tex = tex * (tag + 0.3) + grass_diffuse03.Sample(ModelTextureSampler, input.texCoord)*(1-tag-0.3);
	
	tex = tex * I;

	if (alphar < 0.5) 
		discard; 
	
	return float4(tex, alphar);	
}

//--------------------------------------------------------------------------------------
// PIXEL SHADER
//--------------------------------------------------------------------------------------


float4 PS( PS_IN input ) : SV_Target { 

	float alphar = grass_alpha.Sample(ModelTextureSampler, input.texCoord).r;

	float3 tex = grass_diffuse01.Sample(ModelTextureSampler, input.texCoord)*input.random.b+grass_diffuse02.Sample(ModelTextureSampler, input.texCoord)*(1-input.random.b);

	float tag = (sin((time%100)/10)+1)/2;
    tex = tex*(tag+0.3)+grass_diffuse03.Sample(ModelTextureSampler, input.texCoord)*(1-tag-0.3);

	return float4(tex, alphar);	


}


//--------------------------------------------------------------------------------------
// TECHNIQUE
//--------------------------------------------------------------------------------------

technique10 RenderSolid 
{
	pass p0 {
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( CompileShader( gs_4_0, GS() ) );
		//SetPixelShader( CompileShader( ps_4_0, PS() ) );
        SetPixelShader( CompileShader( ps_4_0, PS_PIXEL_LIGHTING_BLINNPHONG() ) );
        SetRasterizerState( rsSolid );
	}
}
