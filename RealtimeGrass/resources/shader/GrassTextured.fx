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
Texture2D grass_alpha;
Texture2D grass_noise;

//Misc
float cTexScal = 1;
float time;

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
	float random;
};

//Vertexshader Output & Pixelshader Input
struct PS_IN {
	float4 pos				: SV_POSITION;		
	float3 normalWS			: NORMAL;
	float2 texCoord			: TEXCOORD;
	float random			: RANDOM;
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
	return output;
}

//--------------------------------------------------------------------------------------
// GEOMETRY SHADER
//--------------------------------------------------------------------------------------

[maxvertexcount(40)]
void GS(point VS_IN s[1],  inout TriangleStream<PS_IN> triStream)
{
    int LOD = 2;
    GS_WORKING bl;
	GS_WORKING tl;
	GS_WORKING br;
	GS_WORKING tr;

	int dimension_x = 2;
	int dimension_y = 30;

	float2 texCoord = float2(s[0].pos.x, s[0].pos.y);

	float4 random = (grass_noise.SampleLevel(ModelTextureSampler, texCoord, 0));
	random.r=random.r-0.5;
	if (random.r < 0) {
	random.r = random.r*(-1);
	}

	bl.random = random.b;
	tl.random = random.b;
	br.random = random.b;
	tr.random = random.b;


	// Motion added with x^2 influence (between 0-1)
	float windpower = 15;

	float turn = (random.b-0.5);
    float offsetX = windpower/20*sin(time+random.r);

	float offsetY = -windpower/2*sin(time+random.r);
	if (offsetY > 0) {
	offsetY = offsetY*(-1);
	}

	float offsetZ = windpower*sin(time+random.r);

	if (LOD == 0) {

	//create gras // LOD = 0
	//--------------------------------------------

	//bottom left
	bl.pos = float3(s[0].pos.x - dimension_x/2, s[0].pos.y, s[0].pos.z);	
	bl.texCoord = float2(0, 0);
	
	//top left
	tl.pos = float3(s[0].pos.x - dimension_x/2 + offsetX, s[0].pos.y+dimension_y -random.g+ offsetY, s[0].pos.z + offsetZ);	
	tl.texCoord = float2(1, 0);

	//bottom right
	br.pos = float3(s[0].pos.x + dimension_x/2+turn, s[0].pos.y, s[0].pos.z+turn);	
	br.texCoord = float2(0, 1);

	//top right
	tr.pos = float3(s[0].pos.x + dimension_x/2+turn + offsetX, s[0].pos.y + dimension_y-random.g+ offsetY, s[0].pos.z + offsetZ+turn);	
	tr.texCoord = float2(1, 1);

	//Normals bl2tl = bottomleft to topleft (Distance)
	float3 a = br.pos - bl.pos;
	float3 b = tl.pos - bl.pos;

	bl.normal = cross( a, b);
	tl.normal = cross( a,-b);
	br.normal = cross(-a, b);
	tr.normal = cross(-a,-b);
	
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
	tl.pos = float3(s[0].pos.x - dimension_x/2 + offsetX*0.11, s[0].pos.y+dimension_y -random.g+ offsetY*0.11, s[0].pos.z + offsetZ*0.11);	
	tl.texCoord = float2(0.33, 0);

	//bottom right
	br.pos = float3(s[0].pos.x + dimension_x/2+turn, s[0].pos.y, s[0].pos.z+turn);	
	br.texCoord = float2(0, 1);

	//top right
	tr.pos = float3(s[0].pos.x + dimension_x/2+turn + offsetX*0.11, s[0].pos.y + dimension_y-random.g+ offsetY*0.11, s[0].pos.z + offsetZ*0.11+turn);	
	tr.texCoord = float2(0.33, 1);

	//Normals bl2tl = bottomleft to topleft (Distance)
	float3 a = br.pos - bl.pos;
	float3 b = tl.pos - bl.pos;

	bl.normal = cross( a, b);
	tl.normal = cross( a,-b);
	br.normal = cross(-a, b);
	tr.normal = cross(-a,-b);
	
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
	tl.pos = float3(s[0].pos.x - dimension_x/2 + offsetX*0.44, s[0].pos.y+2*dimension_y-random.g+ offsetY*0.44, s[0].pos.z + offsetZ*0.44);	
	tl.texCoord = float2(0.66, 0);

	//bottom right
	br.pos = tr.pos;	
	br.texCoord = tr.texCoord;

	//top right
	tr.pos = float3(s[0].pos.x + dimension_x/2+turn + offsetX*0.44, s[0].pos.y +2*dimension_y-random.g+ offsetY*0.44, s[0].pos.z + offsetZ*0.44+turn);	
	tr.texCoord = float2(0.66, 1);

	//Normals bl2tl = bottomleft to topleft (Distance)
	a = br.pos - bl.pos;
	b = tl.pos - bl.pos;

	tl.normal = cross( a,-b);
	tr.normal = cross(-a,-b);
	
	//Append
	triStream.Append(VSreal(tl));
	triStream.Append(VSreal(tr));

	//create gras // LOD = 1 Seg = 2
	//--------------------------------------------

	//bottom left
	bl.pos = tl.pos;	
	bl.texCoord = tl.texCoord;
	
	//top left
	tl.pos = float3(s[0].pos.x - dimension_x/2 + offsetX*1, s[0].pos.y+3*dimension_y-random.g+ offsetY*1, s[0].pos.z + offsetZ*1);	
	tl.texCoord = float2(1, 0);

	//bottom right
	br.pos = tr.pos;	
	br.texCoord = tr.texCoord;

	//top right
	tr.pos = float3(s[0].pos.x + dimension_x/2 +turn+ offsetX*1, s[0].pos.y + 3*dimension_y-random.g+ offsetY*1, s[0].pos.z + offsetZ*1+turn);	
	tr.texCoord = float2(1, 1);

	//Normals bl2tl = bottomleft to topleft (Distance)
	a = br.pos - bl.pos;
	b = tl.pos - bl.pos;


	tl.normal = cross( a,-b);
	tr.normal = cross(-a,-b);
	
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
	tl.pos = float3(s[0].pos.x - dimension_x/2 + offsetX*0.04, s[0].pos.y+dimension_y -random.g+ offsetY*0.04, s[0].pos.z + offsetZ*0.04);	
	tl.texCoord = float2(0.2, 0);

	//bottom right
	br.pos = float3(s[0].pos.x + dimension_x/2, s[0].pos.y, s[0].pos.z+turn);	
	br.texCoord = float2(0, 1);

	//top right
	tr.pos = float3(s[0].pos.x + dimension_x/2 + offsetX*0.04, s[0].pos.y + dimension_y-random.g+ offsetY*0.04, s[0].pos.z + offsetZ*0.04+turn);	
	tr.texCoord = float2(0.2, 1);

	//Normals bl2tl = bottomleft to topleft (Distance)
	float3 a = br.pos - bl.pos;
	float3 b = tl.pos - bl.pos;

	bl.normal = cross( a, b);
	tl.normal = cross( a,-b);
	br.normal = cross(-a, b);
	tr.normal = cross(-a,-b);
	
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
	tl.pos = float3(s[0].pos.x - dimension_x/2 + offsetX*0.16, s[0].pos.y+2*dimension_y-random.g+ offsetY*0.16, s[0].pos.z + offsetZ*0.16);	
	tl.texCoord = float2(0.4, 0);

	//bottom right
	br.pos = tr.pos;	
	br.texCoord = tr.texCoord;

	//top right
	tr.pos = float3(s[0].pos.x + dimension_x/2+turn + offsetX*0.16, s[0].pos.y + 2*dimension_y-random.g+ offsetY*0.16, s[0].pos.z + offsetZ*0.16+turn);	
	tr.texCoord = float2(0.4, 1);

	//Normals bl2tl = bottomleft to topleft (Distance)
	a = br.pos - bl.pos;
	b = tl.pos - bl.pos;

	tl.normal = cross( a,-b);
	tr.normal = cross(-a,-b);
	
	//Append
	triStream.Append(VSreal(tl));
	triStream.Append(VSreal(tr));

	//create gras // LOD = 2 Seg = 2
	//--------------------------------------------

	//bottom left
	bl.pos = tl.pos;	
	bl.texCoord = tl.texCoord;
	
	//top left
	tl.pos = float3(s[0].pos.x - dimension_x/2 + offsetX*0.36, s[0].pos.y+3*dimension_y-random.g+ offsetY*0.36, s[0].pos.z + offsetZ*0.36);	
	tl.texCoord = float2(0.6, 0);

	//bottom right
	br.pos = tr.pos;	
	br.texCoord = tr.texCoord;

	//top right
	tr.pos = float3(s[0].pos.x + dimension_x/2 +turn+ offsetX*0.36, s[0].pos.y + 3*dimension_y-random.g+ offsetY*0.36, s[0].pos.z + offsetZ*0.36+turn);	
	tr.texCoord = float2(0.6, 1);

	//Normals bl2tl = bottomleft to topleft (Distance)
	a = br.pos - bl.pos;
	b = tl.pos - bl.pos;


	tl.normal = cross( a,-b);
	tr.normal = cross(-a,-b);
	
	//Append
	triStream.Append(VSreal(tl));
	triStream.Append(VSreal(tr));

	//create gras // LOD = 2 Seg = 3
	//--------------------------------------------

	//bottom left
	bl.pos = tl.pos;	
	bl.texCoord = tl.texCoord;
	
	//top left
	tl.pos = float3(s[0].pos.x - dimension_x/2 + offsetX*0.64, s[0].pos.y+4*dimension_y-random.g+ offsetY*0.64, s[0].pos.z + offsetZ*0.64);	
	tl.texCoord = float2(0.8, 0);

	//bottom right
	br.pos = tr.pos;	
	br.texCoord = tr.texCoord;

	//top right
	tr.pos = float3(s[0].pos.x + dimension_x/2+turn + offsetX*0.64, s[0].pos.y + 4*dimension_y-random.g+ offsetY*0.64, s[0].pos.z + offsetZ*0.64+turn);	
	tr.texCoord = float2(0.8, 1);

	//Normals bl2tl = bottomleft to topleft (Distance)
	a = br.pos - bl.pos;
	b = tl.pos - bl.pos;

	tl.normal = cross( a,-b);
	tr.normal = cross(-a,-b);
	
	//Append
	triStream.Append(VSreal(tl));
	triStream.Append(VSreal(tr));

	//create gras // LOD = 2 Seg = 4
	//--------------------------------------------

	//bottom left
	bl.pos = tl.pos;	
	bl.texCoord = tl.texCoord;
	
	//top left
	tl.pos = float3(s[0].pos.x - dimension_x/2 + offsetX*1, s[0].pos.y+5*dimension_y-random.g+ offsetY*1, s[0].pos.z + offsetZ*1);	
	tl.texCoord = float2(1, 0);

	//bottom right
	br.pos = tr.pos;	
	br.texCoord = tr.texCoord;

	//top right
	tr.pos = float3(s[0].pos.x + dimension_x/2+turn + offsetX*1, s[0].pos.y + 5*dimension_y-random.g+ offsetY*1, s[0].pos.z + offsetZ*1+turn);	
	tr.texCoord = float2(1, 1);

	//Normals bl2tl = bottomleft to topleft (Distance)
	a = br.pos - bl.pos;
	b = tl.pos - bl.pos;

	tl.normal = cross( a,-b);
	tr.normal = cross(-a,-b);
	
	//Append
	triStream.Append(VSreal(tl));
	triStream.Append(VSreal(tr));
	}
	}

}

//--------------------------------------------------------------------------------------
// PIXEL SHADER
//--------------------------------------------------------------------------------------

float4 PS( PS_IN input ) : SV_Target { 
	
	float alphar = grass_alpha.Sample(ModelTextureSampler, input.texCoord).r;

	float3 tex = grass_diffuse01.Sample(ModelTextureSampler, input.texCoord).rgb*input.random+grass_diffuse02.Sample(ModelTextureSampler, input.texCoord).rgb*(1-input.random);
	
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
		SetPixelShader( CompileShader( ps_4_0, PS() ) );
	}
}
