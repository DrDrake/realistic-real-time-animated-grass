//Old Sampler2D
Texture2D model_texture;
float2 texStep = float2(1/1600, 1/1050);
//--------------------------------------------------------------------------------------
//RASTERIZER STATES
//--------------------------------------------------------------------------------------
RasterizerState rsSolid
{
	  FillMode = Solid;
	  CullMode = None;
	  FrontCounterClockwise = false;
};

//Texture Filtering
SamplerState ModelTextureSampler 
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

//Vertexshader Input from Pipeline
struct VS_IN 
{
	float3 pos				: POSITION;
	float3 normal			: NORMAL;
	float2 texCoord			: TEXCOORD0;
};

//Vertexshader Output & Pixelshader Input
struct PS_IN 
{
	float4 pos				: SV_POSITION;
	float2 texCoord0		: TEXCOORD0;
};

//------------------------------------------------------------
//Vertexshader
//------------------------------------------------------------

PS_IN VS( VS_IN input ) 
{
	PS_IN output = (PS_IN)0;

	output.pos = float4(input.pos, 1.0f);

    output.texCoord0 = input.texCoord;

	return output;
}

float4 PS( PS_IN input ) : SV_Target 
{
	float weight0, weight1, weight2, weight3;
    float normalization;
	float4 color = float4(0.0f, 0.0f, 0.0f, 0.0f);
	
	weight0 = 1.0f;
    weight1 = 0.9f;
    weight2 = 0.55f;
    weight3 = 0.18f;

    // Create a normalized value to average the weights out a bit.
    normalization = (weight0 + 2.0f * (weight1 + weight2 + weight3)); // 4.0f*

    // Normalize the weights.
    weight0 = weight0 / normalization;
    weight1 = weight1 / normalization;
    weight2 = weight2 / normalization;
    weight3 = weight3 / normalization;

    // Initialize the color to black.
    color = float4(0.0f, 0.0f, 0.0f, 0.0f);
		
	color += model_texture.Sample(ModelTextureSampler, input.texCoord0)* weight0;	
	//Vergleich aus
/*	if(input.texCoord0.x > 0.501)
	{*/
		color += model_texture.Sample(ModelTextureSampler, float2(input.texCoord0.x+0.0009,input.texCoord0.y))* weight3; 
		color += model_texture.Sample(ModelTextureSampler, float2(input.texCoord0.x+0.0005,input.texCoord0.y))* weight2; 
		color += model_texture.Sample(ModelTextureSampler, float2(input.texCoord0.x+0.0003,input.texCoord0.y))* weight1; 
		color += model_texture.Sample(ModelTextureSampler, float2(input.texCoord0.x-0.0003,input.texCoord0.y))* weight1; 
		color += model_texture.Sample(ModelTextureSampler, float2(input.texCoord0.x-0.0005,input.texCoord0.y))* weight2;
		color += model_texture.Sample(ModelTextureSampler, float2(input.texCoord0.x-0.0009,input.texCoord0.y))* weight3;	
/*
		color += model_texture.Sample(ModelTextureSampler, float2(input.texCoord0.x,input.texCoord0.y+0.0009))* weight3; 
		color += model_texture.Sample(ModelTextureSampler, float2(input.texCoord0.x,input.texCoord0.y+0.0005))* weight2; 
		color += model_texture.Sample(ModelTextureSampler, float2(input.texCoord0.x,input.texCoord0.y+0.0003))* weight1; 
		color += model_texture.Sample(ModelTextureSampler, float2(input.texCoord0.x,input.texCoord0.y-0.0003))* weight1; 
		color += model_texture.Sample(ModelTextureSampler, float2(input.texCoord0.x,input.texCoord0.y-0.0005))* weight2;
		color += model_texture.Sample(ModelTextureSampler, float2(input.texCoord0.x,input.texCoord0.y-0.0009))* weight3;	 
*/		
/*	}	else
	
	{ 
	if(input.texCoord0.x > 0.499) {
    color = float4(0.0f, 0.0f, 0.0f, 0.0f);
	
	} else {
		
	color = model_texture.Sample(ModelTextureSampler, input.texCoord0);	
	}
	}																    
*/	
																    
	color.a = 1.0f;														    

	return color;
}

technique10 RenderSolid 
{
	pass P0 
	{
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( 0 );
		SetPixelShader( CompileShader( ps_4_0, PS() ) );
		SetRasterizerState( rsSolid );
	}
}
