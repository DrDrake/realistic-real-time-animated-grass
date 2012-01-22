#include "BlinnPhong.fx"

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
Texture2D grass_shift;

//Misc
float3 cam_Pos;
float cTexScal = 1;
float time;
float windPW=8;  // Value between 0-12
float3 winddir= float3 (0.5,1,0.5); // y always 1

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
};

//--------------------------------------------------------------------------------------
// VERTEX SHADER 
//--------------------------------------------------------------------------------------

VS_IN VS(VS_IN input) {
	float distance2Cam = length(cam_Pos - input.pos);
	if (distance2Cam > 2500) 
	{
		input.pos.x=0;
		input.pos.y=0;
		input.pos.z=0;
	}
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
	

	if (s[0].pos.y > 0) { 
		
		GS_WORKING bl;
		GS_WORKING tl;
		GS_WORKING br;
		GS_WORKING tr;

		float2 texCoord = float2(s[0].pos.x, s[0].pos.y);

		float4 random = (grass_noise.SampleLevel(ModelTextureSampler, texCoord, 0));

			bl.random = random;
			tl.random = random;
			br.random = random;
			tr.random = random;

		random.r=random.r-0.5;
		if (random.r < 0) {
		random.r = random.r*(-1);

	random.r=random.r*2;

	bl.random = random;
	tl.random = random;
	br.random = random;
	tr.random = random;

	float dimension_x = (2+1*(1-(0.5*random.g)))/2;
	float dimension_y = 15+35*(1-(0.5*random.g));

	texCoord = float2((s[0].pos.x%100)/100, (s[0].pos.y%100)/100);
	float4 random2 = (grass_noise.SampleLevel(ModelTextureSampler, texCoord, 0));

	float4 shift = (grass_shift.SampleLevel(ModelTextureSampler, texCoord, 0));

	float turn = 2*(random2.b*random2.r*random2.g);
	s[0].pos.x = s[0].pos.x+4*random2.r;
	s[0].pos.z = s[0].pos.z-4*random2.r;


	//------------------------------------------------
	//change LOD depending on vertex 2 camera distance
	float distance2Cam = length(cam_Pos - s[0].pos);
	int LOD = 0;

	if (distance2Cam < 400 && distance2Cam >= 100)
	{
		LOD = 1;
	}
	else if(distance2Cam < 1500 && distance2Cam >= 400)
	{
		LOD = 2;
	}
	else if(distance2Cam >= 1500)
	{
		LOD = 3;
	}


	//------------------------------------------------


		float size = exp(-pow(distance2Cam/1200,2))+0.1;
	
			if (s[0].pos.y < 20) { 
			size=size*(s[0].pos.y/40 + 0.5);
			}


			float windpower = 0.0f;
			float offsetX = 1.0f;
			float offsetY = 0.0f;
			float offsetZ = 1.0f;

			dimension_y = dimension_y*size;
			dimension_x = dimension_x*size;
			

			if (LOD < 3)
			{
				float sinus = sin(time + random.r + shift.r * 3);

				// Motion added with x^2 influence (between 0-1)
				windpower = windPW; // * (((sinus) +1) /2) +1;
				offsetX = winddir.x * windpower * (0.5+random2.r)*sinus;
				offsetY = -windpower*(0.5+random2.g)*sinus;

				if (offsetY > 0) 
				{
					offsetY = offsetY*(-1);
				}
				offsetZ = winddir.y*windpower*(0.5+random2.b)*sinus;
			} 

			offsetX = offsetX*size;
			offsetY = offsetY*size;
			offsetZ = offsetZ*size;
	if (LOD > 1) {

	//create gras // LOD = 2 / 3
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

	} else if (LOD == 1) {

	dimension_y = dimension_y/3;

	//create gras // LOD = 1 Seg = 1
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

	//create gras // LOD = 0 Seg = 0
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

	//create gras // LOD = 0 Seg = 1
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

	//create gras // LOD = 0 Seg = 2
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

	//create gras // LOD = 0 Seg = 3
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

	//create gras // LOD = 0 Seg = 4
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

	//calculate lighting	
	float3 I = calcBlinnPhongLighting(input.normalWS, time);
	float3 IBack = calcBlinnPhongLighting(-input.normalWS, time);
	
	//with texturing
	float alpha = grass_alpha.Sample(ModelTextureSampler, input.texCoord).r;

    clip( alpha < 0.3f ? -1:1 );

	float3 tex = grass_diffuse01.Sample(ModelTextureSampler, input.texCoord).rgb* input.random.r + grass_diffuse02.Sample(ModelTextureSampler, input.texCoord).rgb * (1-input.random.r);
	tex = tex * I;
	//tex = tex* (input.texCoord.x+1)/2;

//	return float4(tex, alpha);
	return float4(tex, 1.0f);

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
