float4x4 view;
float4x4 proj;
float4x4 world;

Texture2D model_texture;

struct VS_IN {
	float4 pos		: POSITION;
};

struct PS_IN {
	float4 posPS	: SV_POSITION;
	float3 posWS	: TEXCOORD1;
};

SamplerState ModelTextureSampler {
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

PS_IN VS( VS_IN input ) {
	PS_IN output = (PS_IN)0;

	float4x4 worldViewProj = mul(mul(world, view), proj);

	output.posPS = mul(input.pos, worldViewProj);
	output.posWS = mul(input.pos, world).xyz;
	
	return output;
}

float4 PS( PS_IN input ) : SV_Target {
	return float4(input.posWS, 1.0);
}

technique10 Render {
	pass P0 {
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS() ) );
	}
}