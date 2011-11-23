#include "BlinnPhong.fx"

//--------------------------------------------------------------------------------------
// GLOBAL VARS 
//--------------------------------------------------------------------------------------

//don't forget to set the Matrizes
float4x4 view;
float4x4 proj;
float4x4 world;

//Old Sampler2
Texture2D model_texture;

//Misc
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
	float3 halfway 			: HVECTOR;
};

//------------------------------------------------------------
//Vertexshader
//------------------------------------------------------------

PS_IN VS( VS_IN input ) {
	PS_IN output = (PS_IN)0;
	if (!(input.pos.r == 0.0000)) {
		float shift = 1.6f*sin(time*5);
		input.pos.g = input.pos.g+shift;

		if ((shift) < 0) shift=shift*(-1);

		if (input.pos.r < 0) {
			input.pos.r = input.pos.r+shift/2;
		} else {
			input.pos.r = input.pos.r-shift/2;
		}
	}
	float4x4 worldViewProj = mul(mul(world, view), proj);
	output.pos = mul(float4(input.pos, 1.0), worldViewProj);
	output.normalWS = mul(float4(input.normal, 1.0), world).xyz;
	output.texCoord = input.texCoord;
	float3 viewDir = normalize( eye - (float3) input.pos );
	output.halfway = normalize( -l_dir + viewDir );	
	return output;
	
	}

//--------------------------------------------------------------------------------------
// PER PIXEL LIGHTING 
//--------------------------------------------------------------------------------------

float4 PS( PS_IN input ) : SV_Target
{     	
	//renormalize interpolated vectors
	input.normalWS = normalize( input.normalWS );		
	input.halfway = normalize( input.halfway );

	//calculate lighting	
	float3 I = calcBlinnPhongLighting(input.normalWS, -l_dir, input.halfway, time);
	
	//with texturing
	float4 tex = model_texture.Sample(ModelTextureSampler, input.texCoord);
	tex.xyz = tex.xyz * I;

 if (tex.a < 0.5) 
 		discard; 
	
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