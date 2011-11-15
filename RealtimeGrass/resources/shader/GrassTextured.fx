//don't forget to set the Matrizes
float4x4 view;
float4x4 proj;
float4x4 world;

//Old Sampler2
Texture2D grass_texture;
Texture2D grass_alpha;

//Misc
float cTexScal = 1;

//Texture Filtering
SamplerState ModelTextureSampler {
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

//Vertexshader Input from Pipeline
struct VS_IN {
	float3 pos				: POSITION;
};

//For working in the Geometryshader
struct GS_WORKING {
	float3 pos;	
	float3 normal;	
	float2 texCoord;
};

//Vertexshader Output & Pixelshader Input
struct PS_IN {
	float4 pos				: SV_POSITION;
	float3 normalWS			: NORMAL;
	float2 texCoord			: TEXCOORD;
};

//--------------------------------------------------------------------------------------
// VERTEX SHADER 
//--------------------------------------------------------------------------------------
VS_IN VS(VS_IN input) {
	return input;
}

//--------------------------------------------------------------------------------------
// VERTEX SHADER called from Geometryshader as a normal function
//--------------------------------------------------------------------------------------

PS_IN VSreal( GS_WORKING input ) {
	PS_IN output = (PS_IN)0;
	
	float4x4 worldViewProj = mul(mul(world, view), proj);
	output.pos = mul(float4(input.pos, 1.0), worldViewProj);

	output.normalWS = mul(float4(input.normal, 1.0), world).xyz;

	output.texCoord = input.texCoord;
	
	return output;
}

//--------------------------------------------------------------------------------------
// PIXEL SHADER
//--------------------------------------------------------------------------------------

float4 PS( PS_IN input ) : SV_Target {

	float3 tex = grass_texture.Sample(ModelTextureSampler, input.texCoord).xyz;
	return float4(tex, 1.0f);
}

//--------------------------------------------------------------------------------------
// GEOMETRY SHADER
//--------------------------------------------------------------------------------------
[maxvertexcount(4)]
void GS(point VS_IN s[1],  inout TriangleStream<PS_IN> triStream)
{
    GS_WORKING bl;
	GS_WORKING tl;
	GS_WORKING br;
	GS_WORKING tr;

	int dimension_x = 2;
	int dimension_y = 3;

	//create gras // LOD = 0
	//--------------------------------------------

	//bottom left
	bl.pos = float3(s[0].pos.x, s[0].pos.y, s[0].pos.z);	
	bl.texCoord = float2(0,1);
	
	//top left
	tl.pos = float3(s[0].pos.x, s[0].pos.y+dimension_y, s[0].pos.z);	
	tl.texCoord = float2(0,0);

	//bottom right
	br.pos = float3(s[0].pos.x + dimension_x, s[0].pos.y, s[0].pos.z);	
	br.texCoord = float2(1,1);

	//top right
	tr.pos = float3(s[0].pos.x + dimension_x, s[0].pos.y + dimension_y, s[0].pos.z);	
	tr.texCoord = float2(1,0);

	//Normals bl2tl = bottomleft to topleft (Distance)
	float3 a = br.pos - bl.pos;
	float3 b = tl.pos - bl.pos;

	bl.normal = cross( a, b);
	tl.normal = cross( a,-b);
	br.normal = cross(-a, b);
	tr.normal = cross(-a,-b);
	
	//Append
	triStream.Append(VSreal(bl));
	triStream.Append(VSreal(tl));
	triStream.Append(VSreal(br));
	triStream.Append(VSreal(tr));
}

technique10 Render {
	pass P0 {
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( CompileShader( gs_4_0, GS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS() ) );
	}
}