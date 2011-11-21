// with day and night texture


//don't forget to set the Matrizes
float4x4 view;
float4x4 proj;
float4x4 world;

//Old Sampler2D
Texture2D model_texture01;
Texture2D model_texture02;

//Misc
float4 csunWS = float4(10, 20, 10, 1);
float4 camPosWS;
float cTexScal = 3;
float time;


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

	float3 tex = model_texture01.Sample(ModelTextureSampler, input.texCoord * cTexScal)* lightAmount;// * lightAmount;

	float tag = (sin((time%100)/10)+1)/2;
    tex = tex*(tag+0.3)+model_texture02.Sample(ModelTextureSampler, input.texCoord * cTexScal)*(1-tag-0.3);

	return float4(tex, 1.0f);
}

technique10 RenderSolid {
	pass P0 {
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS() ) );
	}
}