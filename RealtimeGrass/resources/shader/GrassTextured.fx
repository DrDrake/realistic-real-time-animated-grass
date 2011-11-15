//don't forget to set the Matrizes
float4x4 view;
float4x4 proj;
float4x4 world;

//Old Sampler2
Texture2D grass_diffuse;
Texture2D grass_alpha;
Texture2D grass_noise;

//Misc
float cTexScal = 1;
float time;

//Texture Filtering
SamplerState ModelTextureSampler {
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Mirror;
    AddressV = Mirror;
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

	float alphar = grass_alpha.Sample(ModelTextureSampler, input.texCoord).r;

	float3 tex = grass_diffuse.Sample(ModelTextureSampler, input.texCoord).rgb;

	return float4(tex, alphar);
}

//--------------------------------------------------------------------------------------
// GEOMETRY SHADER
//--------------------------------------------------------------------------------------
[maxvertexcount(4*5)]
void GS(point VS_IN s[1],  inout TriangleStream<PS_IN> triStream)
{
    GS_WORKING bl;
	GS_WORKING tl;
	GS_WORKING br;
	GS_WORKING tr;

	int dimension_x = 2;
	int dimension_y = 6;

	float2 texCoord = float2(s[0].pos.x, s[0].pos.y);

	float4 random = (grass_noise.SampleLevel(ModelTextureSampler, texCoord, 0) - 0.5) * 2 * 5;

	float offsetX = sin(time) * random.r;
	float offsetZ = sin(time) * random.g;

	//create gras // LOD = 0
	//--------------------------------------------

	//bottom left
	bl.pos = float3(s[0].pos.x - dimension_x/2, s[0].pos.y, s[0].pos.z);	
	bl.texCoord = float2(0, 0);
	
	//top left
	tl.pos = float3(s[0].pos.x - dimension_x/2 + offsetX*0.2, s[0].pos.y+dimension_y -random.g, s[0].pos.z + offsetZ*0.2);	
	tl.texCoord = float2(0.2, 0);

	//bottom right
	br.pos = float3(s[0].pos.x + dimension_x/2, s[0].pos.y, s[0].pos.z);	
	br.texCoord = float2(0, 1);

	//top right
	tr.pos = float3(s[0].pos.x + dimension_x/2 + offsetX*0.2, s[0].pos.y + dimension_y-random.g, s[0].pos.z + offsetZ*0.2);	
	tr.texCoord = float2(0.2, 1);

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

	//create gras // LOD = 1
	//--------------------------------------------

	//bottom left
	bl.pos = tl.pos;	
	bl.texCoord = tl.texCoord;
	
	//top left
	tl.pos = float3(s[0].pos.x - dimension_x/2 + offsetX*0.4, s[0].pos.y+2*dimension_y-random.g, s[0].pos.z + offsetZ*0.4);	
	tl.texCoord = float2(0.4, 0);

	//bottom right
	br.pos = tr.pos;	
	br.texCoord = tr.texCoord;

	//top right
	tr.pos = float3(s[0].pos.x + dimension_x/2 + offsetX*0.4, s[0].pos.y + 2*dimension_y-random.g, s[0].pos.z + offsetZ*0.4);	
	tr.texCoord = float2(0.4, 1);

	//Normals bl2tl = bottomleft to topleft (Distance)
	a = br.pos - bl.pos;
	b = tl.pos - bl.pos;

	bl.normal = cross( a, b);
	tl.normal = cross( a,-b);
	br.normal = cross(-a, b);
	tr.normal = cross(-a,-b);
	
	//Append
	triStream.Append(VSreal(bl));
	triStream.Append(VSreal(tl));
	triStream.Append(VSreal(br));
	triStream.Append(VSreal(tr));

	//create gras // LOD = 2
	//--------------------------------------------

	//bottom left
	bl.pos = tl.pos;	
	bl.texCoord = tl.texCoord;
	
	//top left
	tl.pos = float3(s[0].pos.x - dimension_x/2 + offsetX*0.6, s[0].pos.y+3*dimension_y-random.g, s[0].pos.z + offsetZ*0.6);	
	tl.texCoord = float2(0.6, 0);

	//bottom right
	br.pos = tr.pos;	
	br.texCoord = tr.texCoord;

	//top right
	tr.pos = float3(s[0].pos.x + dimension_x/2 + offsetX*0.6, s[0].pos.y + 3*dimension_y-random.g, s[0].pos.z + offsetZ*0.6);	
	tr.texCoord = float2(0.6, 1);

	//Normals bl2tl = bottomleft to topleft (Distance)
	a = br.pos - bl.pos;
	b = tl.pos - bl.pos;

	bl.normal = cross( a, b);
	tl.normal = cross( a,-b);
	br.normal = cross(-a, b);
	tr.normal = cross(-a,-b);
	
	//Append
	triStream.Append(VSreal(bl));
	triStream.Append(VSreal(tl));
	triStream.Append(VSreal(br));
	triStream.Append(VSreal(tr));

	//create gras // LOD = 3
	//--------------------------------------------

	//bottom left
	bl.pos = tl.pos;	
	bl.texCoord = tl.texCoord;
	
	//top left
	tl.pos = float3(s[0].pos.x - dimension_x/2 + offsetX*0.8, s[0].pos.y+4*dimension_y-random.g, s[0].pos.z + offsetZ*0.8);	
	tl.texCoord = float2(0.8, 0);

	//bottom right
	br.pos = tr.pos;	
	br.texCoord = tr.texCoord;

	//top right
	tr.pos = float3(s[0].pos.x + dimension_x/2 + offsetX*0.8, s[0].pos.y + 4*dimension_y-random.g, s[0].pos.z + offsetZ*0.8);	
	tr.texCoord = float2(0.8, 1);

	//Normals bl2tl = bottomleft to topleft (Distance)
	a = br.pos - bl.pos;
	b = tl.pos - bl.pos;

	bl.normal = cross( a, b);
	tl.normal = cross( a,-b);
	br.normal = cross(-a, b);
	tr.normal = cross(-a,-b);
	
	//Append
	triStream.Append(VSreal(bl));
	triStream.Append(VSreal(tl));
	triStream.Append(VSreal(br));
	triStream.Append(VSreal(tr));

	//create gras // LOD = 4
	//--------------------------------------------

	//bottom left
	bl.pos = tl.pos;	
	bl.texCoord = tl.texCoord;
	
	//top left
	tl.pos = float3(s[0].pos.x - dimension_x/2 + offsetX*1, s[0].pos.y+5*dimension_y-random.g, s[0].pos.z + offsetZ*1);	
	tl.texCoord = float2(1, 0);

	//bottom right
	br.pos = tr.pos;	
	br.texCoord = tr.texCoord;

	//top right
	tr.pos = float3(s[0].pos.x + dimension_x/2 + offsetX*1, s[0].pos.y + 5*dimension_y-random.g, s[0].pos.z + offsetZ*1);	
	tr.texCoord = float2(1, 1);

	//Normals bl2tl = bottomleft to topleft (Distance)
	a = br.pos - bl.pos;
	b = tl.pos - bl.pos;

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