#include "BlinnPhong.fx"

//--------------------------------------------------------------------------------------
// GLOBAL VARS 
//--------------------------------------------------------------------------------------

//don't forget to set the Matrizes
float4x4 view;
float4x4 proj;
float4x4 world;

//Old Sampler2
Texture2D model_texture_high;
Texture2D model_texture_low;
//Misc
float3 cam_Pos;
float cTexScal = 1;
float time;

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
// STRUCTS
//--------------------------------------------------------------------------------------

//Vertexshader Input from Pipeline
struct VS_IN {
	float3 pos				: POSITION;
	float3 normal			: NORMAL;
	float2 texCoord			: TEXCOORD;
};

//Vertexshader Output & Pixelshader Input
struct PS_IN {
	float4 pos				: SV_POSITION;		
	float3 normalWS			: NORMAL;
	float2 texCoord			: TEXCOORD;
	float distance2Cam		: DISTANCE;
};

//------------------------------------------------------------
//Vertexshader
//------------------------------------------------------------

PS_IN VS( VS_IN input ) {
	PS_IN output = (PS_IN)0;
	
	float4x4 worldViewProj = mul(mul(world, view), proj);
	output.pos = mul(float4(input.pos, 1.0), worldViewProj);
	output.normalWS = mul(float4(input.normal, 1.0), world).xyz;
	if (input.pos.y < 0) 
	{
		output.distance2Cam = 0;
	} 
	else {
		output.distance2Cam = length(cam_Pos - input.pos);
	}
	output.texCoord = input.texCoord;
	return output;
}

//--------------------------------------------------------------------------------------
// PER PIXEL LIGHTING 
//--------------------------------------------------------------------------------------

float4 PS( PS_IN input ) : SV_Target
{     	
	//renormalize interpolated vectors
	input.normalWS = normalize( input.normalWS );

	//calculate lighting	
	float3 I = calcBlinnPhongLighting(input.normalWS, time);

	//with texturing and LOD

	float4 tex = model_texture_low.Sample(ModelTextureSampler, input.texCoord * cTexScal);

		if(input.distance2Cam < 300)
	{
	tex = model_texture_high.Sample(ModelTextureSampler, input.texCoord * cTexScal);

	}
	else if(input.distance2Cam < 600 && input.distance2Cam > 300)
	{
	 tex = model_texture_high.Sample(ModelTextureSampler, input.texCoord * cTexScal)*(1-((input.distance2Cam-300)/300)) + model_texture_low.Sample(ModelTextureSampler, input.texCoord * cTexScal)*((input.distance2Cam-300)/300);
	}

	tex.xyz = tex.xyz * I;

    clip( tex.a < 0.1f ? -1:1 );

	return tex;	
}

technique10 RenderSolid {
	pass p0 {
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( 0 );
		SetPixelShader( CompileShader( ps_4_0, PS() ) );
        SetRasterizerState( rsSolid );
	}
}