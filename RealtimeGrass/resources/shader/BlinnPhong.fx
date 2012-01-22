//Material
float mat_ambient;
float mat_diffuse;
float mat_specular;
float mat_shininess;

//--------------------------------------------------------------------------------------
//LIGHTING VARIABLES
//--------------------------------------------------------------------------------------
//DirectionalLight
float3 l_color = float3 (1.0f, 1.0f, 1.0f);
float3 l_dirWS = float3 (-1.0f, -1.0f, 1.0f);

//lighting vars
float3 ambientLight= float3(1.0f, 1.0f, 1.0f);
float3 halfwayWS;

//--------------------------------------------------------------------------------------
// Blinn-Phong Lighting Reflection Model
//--------------------------------------------------------------------------------------
float3 calcBlinnPhongLighting(float3 NormalWS, float time)
{	
	float3 color, color2;

    float t = time/40;
	float pi = 3.14159265358979323846f;
	switch(t % 4)
	{
	 case 0: // Morning
 		color = l_color * (float3(0.5f,1.0f,1.0f));
 		color2 = l_color * (float3(0.2f,0.2f,0.4f));
		break;
	 case 1: // Noon
 		color = l_color * (float3(1.0f,1.0f,1.0f));
 		color2 = l_color * (float3(0.5f,1.0f,1.0f));
		break;
	 case 2: // Evening
 		color = l_color * (float3(1.0f,0.5f,0.5f));
 		color2 = l_color * (float3(1.0f,1.0f,1.0f));
		break;
	 case 3: // Night
 		color = l_color * (float3(0.2f,0.2f,0.4f));
 		color2 = l_color * (float3(1.0f,0.5f,0.5f));
		break;
	}

    float blend = sin((t-floor(t)) * pi/2);
	float3 finalcolor = color * blend + color2 * (1-blend);
	float3 halfwayWS2 = halfwayWS; 

	halfwayWS2 = -l_dirWS; // Faken von Highlights


	float3 Ia = mat_ambient * ambientLight;
	float3 Id = mat_diffuse * saturate( dot(NormalWS, -l_dirWS) );
	float3 Is = mat_specular * pow( saturate(dot(NormalWS, halfwayWS2)), mat_shininess )* finalcolor;
	
   return saturate((Ia + Id + Is) * finalcolor);
}