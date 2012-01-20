//Old Sampler2D
Texture2D model_texture;

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
	float2 texCoord1		: TEXCOORD1;
	float2 texCoord2		: TEXCOORD2;
	float2 texCoord3		: TEXCOORD3;
	float2 texCoord4		: TEXCOORD4;
	float2 texCoord5		: TEXCOORD5;
	float2 texCoord6		: TEXCOORD6;
	float2 texCoord7		: TEXCOORD7;

	float2 texCoord0a		: TEXCOORD0A;
	float2 texCoord1a		: TEXCOORD1A;
	float2 texCoord2a		: TEXCOORD2A;
	float2 texCoord3a		: TEXCOORD3A;
	float2 texCoord4a		: TEXCOORD4A;
	float2 texCoord5a		: TEXCOORD5A;
	float2 texCoord6a		: TEXCOORD6A;
	float2 texCoord7a		: TEXCOORD7A;

};

//------------------------------------------------------------
//Vertexshader
//------------------------------------------------------------

PS_IN VS( VS_IN input ) 
{
	PS_IN output = (PS_IN)0;

	output.pos = float4(input.pos, 1.0f);
	
	float2 texStep = float2(1/1600, 1/1050);


	output.texCoord0 = input.texCoord + float2(texStep.x* -3.0f, 0.0f);
    output.texCoord1 = input.texCoord + float2(texStep.x * -2.0f, 0.0f);
    output.texCoord2 = input.texCoord + float2(texStep.x * -1.0f, 0.0f);
    output.texCoord3 = input.texCoord + float2(texStep.x *  0.0f, 0.0f);
    output.texCoord4 = input.texCoord + float2(texStep.x *  1.0f, 0.0f);
    output.texCoord5 = input.texCoord + float2(texStep.x *  2.0f, 0.0f);
    output.texCoord6 = input.texCoord + float2(texStep.x *  3.0f, 0.0f);

	output.texCoord0a = input.texCoord + float2(0.0f,texStep.y * -3.0f);
    output.texCoord1a = input.texCoord + float2(0.0f,texStep.y * -2.0f);
    output.texCoord2a = input.texCoord + float2(0.0f,texStep.y * -1.0f);
    output.texCoord3a = input.texCoord + float2(0.0f,texStep.y * 0.0f);
    output.texCoord4a = input.texCoord + float2(0.0f,texStep.y * -1.0f);
    output.texCoord5a = input.texCoord + float2(0.0f,texStep.y * -2.0f);
    output.texCoord6a = input.texCoord + float2(0.0f,texStep.y * -3.0f);
	
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
    normalization = (weight0 + 2.0f * (weight1 + weight2 + weight3));

    // Normalize the weights.
    weight0 = weight0 / normalization;
    weight1 = weight1 / normalization;
    weight2 = weight2 / normalization;
    weight3 = weight3 / normalization;

    // Initialize the color to black.
    color = float4(0.0f, 0.0f, 0.0f, 0.0f);
		
	color = model_texture.Sample(ModelTextureSampler, input.texCoord3);	

/*	color += model_texture.Sample(ModelTextureSampler, input.texCoord3)* weight0;	
	if(input.texCoord3.x > 1)
	{
		color += model_texture.Sample(ModelTextureSampler, input.texCoord0)* weight3; 
		color += model_texture.Sample(ModelTextureSampler, input.texCoord1)* weight2; 
		color += model_texture.Sample(ModelTextureSampler, input.texCoord2)* weight1; 
		color += model_texture.Sample(ModelTextureSampler, input.texCoord4)* weight1; 
		color += model_texture.Sample(ModelTextureSampler, input.texCoord5)* weight2;
		color += model_texture.Sample(ModelTextureSampler, input.texCoord6)* weight3;
		 
		color += model_texture.Sample(ModelTextureSampler, input.texCoord0a)* weight3; 
		color += model_texture.Sample(ModelTextureSampler, input.texCoord1a)* weight2; 
		color += model_texture.Sample(ModelTextureSampler, input.texCoord2a)* weight1; 
		color += model_texture.Sample(ModelTextureSampler, input.texCoord4a)* weight1; 
		color += model_texture.Sample(ModelTextureSampler, input.texCoord5a)* weight2;
		color += model_texture.Sample(ModelTextureSampler, input.texCoord6a)* weight3; 
	}	else {
		
	color = model_texture.Sample(ModelTextureSampler, input.texCoord3);	
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
