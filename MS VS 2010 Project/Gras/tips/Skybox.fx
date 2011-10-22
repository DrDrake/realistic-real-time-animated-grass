float4x4 WorldViewProj : WorldViewProjection;
float4x4 World: World;
float4x4 ViewInverse : ViewInverse;

struct VS_INPUT{
	float3 Position: POSITION;
};

struct VS_OUTPUT{
	float4 Position: POSITION;
	float3 WorldPosition: TEXCOORD0;
};


textureCUBE BaseTexture: TEXTURE0;

samplerCUBE BaseSampler = sampler_state
{
    Texture = (BaseTexture);
    MIPFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MINFILTER = LINEAR;
};


VS_OUTPUT mainVS(VS_INPUT input){
    VS_OUTPUT output;
    output.Position=mul(float4(input.Position, 1.0), WorldViewProj);
    output.WorldPosition =  mul(float4(input.Position, 1.0), World);

    return output;
}



float4 mainPS(VS_OUTPUT IN) : COLOR {
	float3 DirectionToCamera = normalize(ViewInverse[3].xyz - IN.WorldPosition);
	return texCUBE(BaseSampler,-DirectionToCamera);
}

technique Skybox {
	pass p0 {
		ZEnable=false;
		CullMode = CW;
		VertexShader = compile vs_3_0 mainVS();
		PixelShader = compile ps_3_0 mainPS();		
	}
}
