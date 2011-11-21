//don't forget to set the Matrizes
float4x4 view;
float4x4 proj;
float4x4 world;

//Old Sampler2D
Texture2D model_texture;

//Misc
float4 csunWS = float4(0,500, 0, 1);
float4 camPosWS;
float cTexScal = 4;



//Texture Filtering
SamplerState ModelTextureSampler {
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

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

	float4 positionWS		: TEXCOORD1;
	float4 lightDirWS		: TEXCOORD2;
	float4 vert2CamWS		: TEXCOORD3;
};

//------------------------------------------------------------
//Vertexshader
//------------------------------------------------------------

PS_IN VS( VS_IN input ) {
	PS_IN output = (PS_IN)0;
	
	float4x4 worldViewProj = mul(mul(world, view), proj);
	output.pos = mul(float4(input.pos, 1.0), worldViewProj);

	//For Lighting
	output.positionWS = mul(float4(input.pos, 1.0), world);

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

	float3 tex = model_texture.Sample(ModelTextureSampler, input.texCoord * cTexScal);// * lightAmount;
	return float4(tex, 0.3f);
}

technique10 RenderSolid {
	pass P0 {
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( 0 );
		SetPixelShader( CompileShader( ps_4_0, PS() ) );
	}
}