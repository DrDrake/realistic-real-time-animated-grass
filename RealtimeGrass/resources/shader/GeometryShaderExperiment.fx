float4x4 view;
float4x4 proj;
float4x4 world;

struct VS_IN {
	float4 pos : POSITION;
	float4 col : COLOR;
};

struct PS_IN {
	float4 pos : SV_POSITION;
	float4 col : COLOR;
};

PS_IN VS( VS_IN input ) {
	PS_IN output = (PS_IN)0;

	float4x4 worldViewProj = mul(mul(world, view), proj);
	output.pos = mul(input.pos, worldViewProj);
	output.col = input.col;
	
	return output;
}

float4 PS( PS_IN input ) : SV_Target {
	return input.col;
}


//--------------------------------------------------------------------------------------
// GEOMETRY SHADER
//--------------------------------------------------------------------------------------
[maxvertexcount(4)]
void GS( point SPRITE_INPUT sprite[1], inout TriangleStream<PS_INPUT> triStream )
{
	PS_INPUT v;
	v.opacity = sprite[0].opacity;			
	
	//create gras // LOD = 0
	//--------------------------------------------

	//bottom left
	v.p = float4(sprite[0].topLeft[0],sprite[0].topLeft[1]-sprite[0].dimensions[1],0,1);	
	v.t = float2(0,1);	
	triStream.Append(v);
	
	//top left
	v.p = float4(sprite[0].topLeft[0],sprite[0].topLeft[1],0,1);	
	v.t = float2(0,0);
	triStream.Append(v);

	//bottom right
	v.p = float4(sprite[0].topLeft[0]+sprite[0].dimensions[0],sprite[0].topLeft[1]-sprite[0].dimensions[1],0,1);	
	v.t = float2(1,1);
	triStream.Append(v);

	//top right
	v.p = float4(sprite[0].topLeft[0]+sprite[0].dimensions[0],sprite[0].topLeft[1],0,1);	
	v.t = float2(1,0);
	triStream.Append(v);

	//create gras // LOD = 1
	//--------------------------------------------

	//bottom left
	v.p = float4(sprite[0].topLeft[0],sprite[0].topLeft[1],0,1);	
	v.t = float2(0,1);	
	triStream.Append(v);
	
	//top left
	v.p = float4(sprite[0].topLeft[0],sprite[0].topLeft[1]+sprite[0].dimensions[1],0,1);	
	v.t = float2(0,0);
	triStream.Append(v);

	//bottom right
	v.p = float4(sprite[0].topLeft[0]+sprite[0].dimensions[0],sprite[0].topLeft[1],0,1);	
	v.t = float2(1,1);
	triStream.Append(v);

	//top right
	v.p = float4(sprite[0].topLeft[0]+sprite[0].dimensions[0],sprite[0].topLeft[1]+sprite[0].dimensions[1],0,1);	
	v.t = float2(1,0);
	triStream.Append(v);


	//create gras // LOD = 2
	//--------------------------------------------

	//bottom left
	v.p = float4(sprite[0].topLeft[0],sprite[0].topLeft[1]+sprite[0].dimensions[1],0,1);	
	v.t = float2(0,1);	
	triStream.Append(v);
	
	//top left
	v.p = float4(sprite[0].topLeft[0],sprite[0].topLeft[1]+2*(sprite[0].dimensions[1]),0,1);	
	v.t = float2(0,0);
	triStream.Append(v);

	//bottom right
	v.p = float4(sprite[0].topLeft[0]+sprite[0].dimensions[0],sprite[0].topLeft[1]+sprite[0].dimensions[1],0,1);	
	v.t = float2(1,1);
	triStream.Append(v);

	//top right
	v.p = float4(sprite[0].topLeft[0]+sprite[0].dimensions[0],sprite[0].topLeft[1]+2*(sprite[0].dimensions[1]),0,1);	
	v.t = float2(1,0);
	triStream.Append(v);

	//create gras // LOD = 3
	//--------------------------------------------

	//bottom left
	v.p = float4(sprite[0].topLeft[0],sprite[0].topLeft[1]+2*(sprite[0].dimensions[1]),0,1);	
	v.t = float2(0,1);	
	triStream.Append(v);
	
	//top left
	v.p = float4(sprite[0].topLeft[0],sprite[0].topLeft[1]+3*(sprite[0].dimensions[1]),0,1);	
	v.t = float2(0,0);
	triStream.Append(v);

	//bottom right
	v.p = float4(sprite[0].topLeft[0]+sprite[0].dimensions[0],sprite[0].topLeft[1]+2*(sprite[0].dimensions[1]),0,1);	
	v.t = float2(1,1);
	triStream.Append(v);

	//top right
	v.p = float4(sprite[0].topLeft[0]+sprite[0].dimensions[0],sprite[0].topLeft[1]+3*(sprite[0].dimensions[1]),0,1);	
	v.t = float2(1,0);
	triStream.Append(v);

	//create gras // LOD = 4
	//--------------------------------------------

	//bottom left
	v.p = float4(sprite[0].topLeft[0],sprite[0].topLeft[1]+3*(sprite[0].dimensions[1]),0,1);	
	v.t = float2(0,1);	
	triStream.Append(v);
	
	//top left
	v.p = float4(sprite[0].topLeft[0],sprite[0].topLeft[1]+4*(sprite[0].dimensions[1]),0,1);	
	v.t = float2(0,0);
	triStream.Append(v);

	//bottom right
	v.p = float4(sprite[0].topLeft[0]+sprite[0].dimensions[0],sprite[0].topLeft[1]+3*(sprite[0].dimensions[1]),0,1);	
	v.t = float2(1,1);
	triStream.Append(v);

	//top right
	v.p = float4(sprite[0].topLeft[0]+sprite[0].dimensions[0],sprite[0].topLeft[1]+4*(sprite[0].dimensions[1]),0,1);	
	v.t = float2(1,0);
	triStream.Append(v);


}

technique10 Render {
	pass P0 {
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( CompileShader( gs_4_0, GS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS() ) );
	}
}