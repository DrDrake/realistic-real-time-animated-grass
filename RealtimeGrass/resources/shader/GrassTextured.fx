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
int kernelSize = 13;

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
	int dimension_y = 30;

	int x = s[0].pos.x;
	int y = s[0].pos.y;
	int z = s[0].pos.z;

	float2 texCoord = float2(x, y);

	//float4 random = (grass_noise.SampleLevel(ModelTextureSampler, texCoord, 0) - 0.5) * 2 * 5;

	// Motion added with x^2 influence (between 0-1)
	float windpower = 15;

    float offsetX = windpower/20 * sin(time);

	float offsetY = -windpower/2 * sin(time);

	if (offsetY > 0) 
	{
		offsetY = offsetY*(-1);
	}

	float4 random = 0;
	float offsetZ = windpower * sin(time + random.r);

	int LOD = 0;

	if (LOD == 0) {
		//create gras // LOD = 0
		//--------------------------------------------

		//bottom left
		bl.pos = float3(x - dimension_x/2, y, z);	
		bl.texCoord = float2(0, 0);
	
		//top left
		tl.pos = float3(x - dimension_x/2 + offsetX, y+dimension_y -random.g + offsetY, z + offsetZ);	
		tl.texCoord = float2(1, 0);

		//bottom right
		br.pos = float3(x + dimension_x/2, y, z);	
		br.texCoord = float2(0, 1);

		//top right
		tr.pos = float3(x + dimension_x/2 + offsetX, y + dimension_y-random.g + offsetY, z + offsetZ);	
		tr.texCoord = float2(1, 1);

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

	} else if (LOD == 1) {

		dimension_y = dimension_y/3;

		//create gras // LOD = 1 Seg = 0
		//--------------------------------------------

		//bottom left
		bl.pos = float3(x - dimension_x/2, y, z);	
		bl.texCoord = float2(0, 0);
	
		//top left
		tl.pos = float3(x - dimension_x/2 + offsetX*0.11, y+dimension_y -random.g+ offsetY*0.11, z + offsetZ*0.11);	
		tl.texCoord = float2(0.33, 0);

		//bottom right
		br.pos = float3(x + dimension_x/2, y, z);	
		br.texCoord = float2(0, 1);

		//top right
		tr.pos = float3(x + dimension_x/2 + offsetX*0.11, y + dimension_y-random.g+ offsetY*0.11, z + offsetZ*0.11);	
		tr.texCoord = float2(0.33, 1);

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

		//create gras // LOD = 1 Seg = 1
		//--------------------------------------------

		//bottom left
		bl.pos = tl.pos;	
		bl.texCoord = tl.texCoord;
	
		//top left
		tl.pos = float3(x - dimension_x/2 + offsetX*0.44, y+2*dimension_y-random.g+ offsetY*0.44, z + offsetZ*0.44);	
		tl.texCoord = float2(0.66, 0);

		//bottom right
		br.pos = tr.pos;	
		br.texCoord = tr.texCoord;

		//top right
		tr.pos = float3(x + dimension_x/2 + offsetX*0.44, y + 2*dimension_y-random.g+ offsetY*0.44, z + offsetZ*0.44);	
		tr.texCoord = float2(0.66, 1);

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

		//create gras // LOD = 1 Seg = 2
		//--------------------------------------------

		//bottom left
		bl.pos = tl.pos;	
		bl.texCoord = tl.texCoord;
	
		//top left
		tl.pos = float3(x - dimension_x/2 + offsetX*1, y+3*dimension_y-random.g+ offsetY*1, z + offsetZ*1);	
		tl.texCoord = float2(1, 0);

		//bottom right
		br.pos = tr.pos;	
		br.texCoord = tr.texCoord;

		//top right
		tr.pos = float3(x + dimension_x/2 + offsetX*1, y + 3*dimension_y-random.g+ offsetY*1, z + offsetZ*1);	
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

	} else if (LOD == 2){

		dimension_y = dimension_y/5;

		//create gras // LOD = 2 Seg = 0
		//--------------------------------------------

		//bottom left
		bl.pos = float3(x - dimension_x/2, y, z);	
		bl.texCoord = float2(0, 0);
	
		//top left
		tl.pos = float3(x - dimension_x/2 + offsetX*0.04, y+dimension_y -random.g+ offsetY*0.04, z + offsetZ*0.04);	
		tl.texCoord = float2(0.2, 0);

		//bottom right
		br.pos = float3(x + dimension_x/2, y, z);	
		br.texCoord = float2(0, 1);

		//top right
		tr.pos = float3(x + dimension_x/2 + offsetX*0.04, y + dimension_y-random.g+ offsetY*0.04, z + offsetZ*0.04);	
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

		//create gras // LOD = 2 Seg = 1
		//--------------------------------------------

		//bottom left
		bl.pos = tl.pos;	
		bl.texCoord = tl.texCoord;
	
		//top left
		tl.pos = float3(x - dimension_x/2 + offsetX*0.16, y+2*dimension_y-random.g+ offsetY*0.16, z + offsetZ*0.16);	
		tl.texCoord = float2(0.4, 0);

		//bottom right
		br.pos = tr.pos;	
		br.texCoord = tr.texCoord;

		//top right
		tr.pos = float3(x + dimension_x/2 + offsetX*0.16, y + 2*dimension_y-random.g+ offsetY*0.16, z + offsetZ*0.16);	
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

		//create gras // LOD = 2 Seg = 2
		//--------------------------------------------

		//bottom left
		bl.pos = tl.pos;	
		bl.texCoord = tl.texCoord;
	
		//top left
		tl.pos = float3(x - dimension_x/2 + offsetX*0.36, y+3*dimension_y-random.g+ offsetY*0.36, z + offsetZ*0.36);	
		tl.texCoord = float2(0.6, 0);

		//bottom right
		br.pos = tr.pos;	
		br.texCoord = tr.texCoord;

		//top right
		tr.pos = float3(x + dimension_x/2 + offsetX*0.36, y + 3*dimension_y-random.g+ offsetY*0.36, z + offsetZ*0.36);	
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

		//create gras // LOD = 2 Seg = 3
		//--------------------------------------------

		//bottom left
		bl.pos = tl.pos;	
		bl.texCoord = tl.texCoord;
	
		//top left
		tl.pos = float3(x - dimension_x/2 + offsetX*0.64, y+4*dimension_y-random.g+ offsetY*0.64, z + offsetZ*0.64);	
		tl.texCoord = float2(0.8, 0);

		//bottom right
		br.pos = tr.pos;	
		br.texCoord = tr.texCoord;

		//top right
		tr.pos = float3(x + dimension_x/2 + offsetX*0.64, y + 4*dimension_y-random.g+ offsetY*0.64, z + offsetZ*0.64);	
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

		//create gras // LOD = 2 Seg = 4
		//--------------------------------------------

		//bottom left
		bl.pos = tl.pos;	
		bl.texCoord = tl.texCoord;
	
		//top left
		tl.pos = float3(x - dimension_x/2 + offsetX*1, y+5*dimension_y-random.g+ offsetY*1, z + offsetZ*1);	
		tl.texCoord = float2(1, 0);

		//bottom right
		br.pos = tr.pos;	
		br.texCoord = tr.texCoord;

		//top right
		tr.pos = float3(x + dimension_x/2 + offsetX*1, y + 5*dimension_y-random.g+ offsetY*1, z + offsetZ*1);	
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
// TECHNIQUE
//--------------------------------------------------------------------------------------

technique10 RenderSolid 
{
	pass p0 {
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( CompileShader( gs_4_0, GS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS() ) );
	}
}