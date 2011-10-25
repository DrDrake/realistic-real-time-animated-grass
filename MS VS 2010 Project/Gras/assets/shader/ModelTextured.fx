//don't forget to set the Matrizes
float4x4 view;
float4x4 proj;
float4x4 world;

//Old Sampler2D
Texture2D model_texture;

//Misc
float4 csunWS = float4(0, 100, 0, 1);
float4 camPosWS;



//Texture Filtering
SamplerState ModelTextureSampler {
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

//Vertexshader Input from Pipeline
struct VS_IN {
	float4 pos				: POSITION;
	float3 normal			: NORMAL;
	float2 texCoord			: TEXCOORD;
};

//Vertexshader Output & Pixelshader Input
struct PS_IN {
	float4 pos				: SV_POSITION;
	float3 normalWS			: NORMAL;
	float2 texCoord			: TEXCOORD;

	float4 positionWS		: TEXCOORD1;
	float4 lightDirWS		: TEXCOORD2;
	float4 vert2CamWS		: TEXCOORD3;
};

PS_IN VS( VS_IN input ) {
	PS_IN output = (PS_IN)0;
	
	float4x4 worldViewProj = mul(mul(world, view), proj);
	output.pos = mul(input.pos, worldViewProj);

	//For Lighting
	output.positionWS = mul(input.pos, world);

	// Calculate light to object vector
	output.lightDirWS = output.positionWS - csunWS;

	// Calculate object to camera vector
	output.vert2CamWS = camPosWS - output.positionWS;

	output.normalWS = mul(float4(input.normal, 1.0), world).xyz;

	output.texCoord = input.texCoord;
	
	return output;
}

float4 PS( PS_IN input ) : SV_Target {
	float lightAmount = dot(normalize(float4(input.normalWS, 1.0)),normalize(csunWS));


	return model_texture.Sample(ModelTextureSampler, input.texCoord);// * lightAmount;
}

technique10 RenderSolid {
	pass P0 {
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS() ) );
	}
}