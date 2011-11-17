float4x4 view;
float4x4 proj;
float4x4 world;

float time;

TextureCube model_texture01;
TextureCube model_texture02;

struct VS_IN {
	float3 pos		: POSITION;
	float3 normal	: NORMAL;
	float2 texCoord	: TEXCOORD;
};

struct PS_IN {
	float4 posPS	: SV_POSITION;
	float4 normalWS	: NORMAL;
	float3 texCoord	: TEXCOORD;
};

SamplerState ModelTextureSampler {
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

PS_IN VS( VS_IN input ) {
	PS_IN output = (PS_IN)0;

	float4x4 worldViewProj = mul(mul(world, view), proj);

	output.posPS = mul(float4(input.pos, 1.0), worldViewProj).xyww;
	output.normalWS = mul(float4(input.normal, 1.0), world);
	output.texCoord = input.pos;
	
	return output;
}

float4 PS( PS_IN input ) : SV_Target {
	float tag = (sin((time%100)/10)+1)/2;
    float4 tex = model_texture01.Sample(ModelTextureSampler, input.texCoord)*tag+model_texture02.Sample(ModelTextureSampler, input.texCoord)*(1-tag);
	return tex;
}

RasterizerState NoCulling
{
	CullMode = None;
};

DepthStencilState LessEqualDSS
{
	DepthFunc = LESS_EQUAL;
};

technique10 Render {
	pass P0 {
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS() ) );

		SetRasterizerState(NoCulling);
		SetDepthStencilState(LessEqualDSS, 0);
	}
}