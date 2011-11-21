//--------------------------------------------------------------------------------------
// GLOBAL VARS 
//--------------------------------------------------------------------------------------

//don't forget to set the Matrizes
float4x4 view;
float4x4 proj;
float4x4 world;

//Old Sampler2
Texture2D model_texture01;

//Misc
float cTexScal = 3;
float time;

//--------------------------------------------------------------------------------------
//LIGHTING VARIABLES
//--------------------------------------------------------------------------------------
//DirectionalLight
float4 l_color = float4 (1.0f,1.0f,1.0f,1.0f);
float3 l_dir = float3 (-1,-1,1);

//Material
	float mat_Ka, mat_Kd, mat_Ks, mat_A;

//lighting vars
float4 ambientLight= float4(1.0f,1.0f,1.0f,1.0f);
float3 eye;

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
// Blinn-Phong Lighting Reflection Model
//--------------------------------------------------------------------------------------
float4 calcBlinnPhongLighting(float M_Ka, float M_Kd, float M_Ks, float M_A, float4 LColor, float3 N, float3 L, float3 H )
{	
	float4 Ia = M_Ka * ambientLight;
	float4 Id = M_Kd * saturate( dot(N,L) );
	float4 Is = M_Ks * pow( saturate(dot(N,H)), M_A );
	
	return Ia + (Id + Is) * LColor;
}

//--------------------------------------------------------------------------------------
// STRCUCTS
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
	float3 h 				: HVECTOR;
};

//------------------------------------------------------------
//Vertexshader
//------------------------------------------------------------

PS_IN VS( VS_IN input ) {
	PS_IN output = (PS_IN)0;
	
	float4x4 worldViewProj = mul(mul(world, view), proj);
	output.pos = mul(float4(input.pos, 1.0), worldViewProj);
	output.normalWS = mul(float4(input.normal, 1.0), world).xyz;
	output.texCoord = input.texCoord;
	float3 V = normalize( eye - (float3) input.pos );
	output.h = normalize( -l_dir + V );	
	return output;
	
	}

//--------------------------------------------------------------------------------------
// PER PIXEL LIGHTING 
//--------------------------------------------------------------------------------------

float4 PS_PIXEL_LIGHTING_BLINNPHONG( PS_IN input ) : SV_Target
{     	
	//renormalize interpolated vectors
	input.normalWS = normalize( input.normalWS );		
	input.h = normalize( input.h );

	float4 color, color2;

    float t = time/40;
	float pi = 3.14159265358979323846f;
	switch(t%4)
{
 case 0:
 	color = l_color*(float4(0.5f,1.0f,1.0f,1.0f));
 	color2 = l_color*(float4(0.1f,0.1f,0.3f,1.0f));
	break;
 case 1:
 	color = l_color*(float4(1.0f,1.0f,1.0f,1.0f));
 	color2 = l_color*(float4(0.5f,1.0f,1.0f,1.0f));
	break;
 case 2:
 	color = l_color*(float4(1.0f,0.6f,0.6f,1.0f));
 	color2 = l_color*(float4(1.0f,1.0f,1.0f,1.0f));
	break;
 case 3:
 	color = l_color*(float4(0.1f,0.1f,0.3f,1.0f));
 	color2 = l_color*(float4(1.0f,0.6f,0.6f,1.0f));
	break;
}

    float blend = sin((t-floor(t))*pi/2);
	float4 color3 = color*blend+color2*(1-blend);

	//calculate lighting	
	float4 I = calcBlinnPhongLighting( mat_Ka, mat_Kd, mat_Ks, mat_A, color3, input.normalWS, -l_dir, input.h );
	
	//with texturing
	float3 tex = model_texture01.Sample(ModelTextureSampler, input.texCoord);

	tex = tex*I;

	return float4(tex,1.0f);	
	
}




technique10 RenderSolid {
	pass p0 {
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( 0 );
		//SetPixelShader( CompileShader( ps_4_0, PS() ) );
        SetPixelShader( CompileShader( ps_4_0, PS_PIXEL_LIGHTING_BLINNPHONG() ) );
        SetRasterizerState( rsSolid );
	}
}