//Material
float mat_ambient;
float mat_diffuse;
float mat_specular;
float mat_shininess;

//--------------------------------------------------------------------------------------
//LIGHTING VARIABLES
//--------------------------------------------------------------------------------------
//DirectionalLight
float4 l_color = float4 (1.0f,1.0f,1.0f,1.0f);
float3 l_dir = float3 (-1,-1,1);

//lighting vars
float4 ambientLight= float4(1.0f, 1.0f, 1.0f, 1.0f);
float3 eye;

//--------------------------------------------------------------------------------------
// Blinn-Phong Lighting Reflection Model
//--------------------------------------------------------------------------------------
float4 calcBlinnPhongLighting(float3 N, float3 L, float3 H , float time)
{	
	float4 color, color2;

    float t = time/40;
	float pi = 3.14159265358979323846f;
	switch(t % 4)
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


	float4 Ia = mat_ambient * ambientLight;
	float4 Id = mat_diffuse * saturate( dot(N,L) );
	float4 Is = mat_specular * pow( saturate(dot(N,H)), mat_shininess );
	
	return Ia + (Id + Is) * color3;
}