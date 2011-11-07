float4x4 view;
float4x4 proj;
float4x4 world;

Texture2D model_texture;
sampler TextureSampler : register(s0);

struct VS_IN {
	float4 pos : POSITION;
	float4 col : COLOR;
	float2 texCoord	: TEXCOORD;
};

struct PS_IN {
	float4 pos : SV_POSITION;
	float4 col : COLOR;
	float2 texCoord	: TEXCOORD;
};

SamplerState ModelTextureSampler {
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

PS_IN VS( VS_IN input ) {
	PS_IN output = (PS_IN)0;

	float4x4 worldViewProj = mul(mul(world, view), proj);
	output.pos = mul(input.pos, worldViewProj);
	output.col = input.col;
	output.texCoord = input.texCoord;
	
	return output;
}

float4 PS( PS_IN input ) : SV_Target {
	//return input.col;
	return model_texture.Sample(ModelTextureSampler, input.texCoord); //* input.diffuse
}

technique10 Render {
	pass P0 {
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS() ) );
	}
}