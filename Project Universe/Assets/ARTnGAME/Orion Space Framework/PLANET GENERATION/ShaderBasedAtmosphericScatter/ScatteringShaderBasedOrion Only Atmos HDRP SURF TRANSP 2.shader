Shader "ORION/Scattering/Atmospheric Scattering Only Atmos HDRP SURF TRANSP 2"
//Shader "ORION/Scattering/Atmospheric Scattering Only Atmos HDRP SURF TRANSP 1"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Main Texture", 2D) = "white" {}
		_Glossiness("_Glossiness", Range(0,1)) = 0.5
		_BumpMap("Bump Map", 2D) = "bump" {}
		_GlossyMap("Glossy Map", 2D) = "white" {}

		_CloudsTex("Clouds", 2D) = "black" {}
		_CloudsAlpha("Clouds Transparency", Range(0,1)) = 0.25
		_CloudsSpeed("Clouds Speed", Range(-10,10)) = 1
		[Toggle] _CloudsAdditive("Additive clouds", Int) = 1

		[Space]
		_NightTex("Night Time Map", 2D) = "black" {}
		[HDR] _NightColor("Night Color", Color) = (1,1,1,0.5)
		_NightWrap("Night Wrap", Range(0,1)) = 0.5

		_AtmosphereModifier("Atmosphere Modifier", Float) = 1
		_ScatteringModifier("Scattering Modifier", Float) = 1
		_AtmosphereColor("Atmosphere Color", Color) = (1,1,1,1)

		_PlanetRadius("Planet Radius", Float) = 6372000
		_AtmosphereHeight("Atmosphere Height", Float) = 60500
		_SphereRadius("Sphere Radius", Float) = 6.371

		_RayScatteringCoefficient("br", Vector) = (0.000005804542996261094, 0.000013562911419845636, 0.00003026590629238532, 0)
		_RayScaleHeight("H0", Float) = 8050

		_MScatteringCoefficient("bm", Float) = 0.002111
		_MAnisotropy("gi", Range(-1,1)) = 0.75821
		_MScaleHeight("H0", Float) = 1205

		_SunIntensity("Sun intensity", Range(0,100)) = 23
		_ViewSamples("View ray steps", Range(0,256)) = 16
		_LightSamples("Light ray steps", Range(0,256)) = 8

		_Specular("_Specular", float) = 0.5
		_SunStrength("Sun Strength", float) = 1
		_SunPower("Sun Power", float) = 1
		_LightDirectionFX("Light Direction FX", Vector) = (-1,0,0,0)
		_TerrainPower("_TerrainPower", float) = 1
		_passSunExternally("Give sun pos-color with globalSunPosition-SunColor", float) = 0
			flipSun("flipSun", float) = 1

			contrast("contrast", float) = 1
			contrastAtmo("contrast Atmosphere", float) = 0

			CongtrolVars("CongtrolVars", Vector) = (1,1,1,1)
	}
		SubShader{

			////PASS 2
			Tags{ "RenderType" = "Transparent" 	"Queue" = "Transparent" }
				LOD 200
					Blend SrcAlpha OneMinusSrcAlpha//Blend One One
				Cull Front

			//v0.1
			//ZTest LEqual
			//Zwrite Off


				Pass{
					CGPROGRAM
					#pragma vertex vert
					#pragma fragment frag			
					#include "UnityCG.cginc"				
					#include "Lighting.cginc"
					#include "AutoLight.cginc"
					#pragma target 3.0

					float4 CongtrolVars;

					float contrast;
					float contrastAtmo;

					float flipSun;

					sampler2D _MainTex;
					float4 planetCenter;
					float _Glossiness;
					float _SunStrength;
					float _SunPower;
					float4 _LightDirectionFX;
					//GLOBAL
					float4 globalSunPosition;
					float4 globalSunColor;
					float _passSunExternally;

					struct v2f
					{
						float4 vertex: SV_POSITION;
						float3 normal: NORMAL;
						float4 uv_MainTex: TEXCOORD0;
						float3 worldNormal: TEXCOORD1;
						float4 tangent: TANGET;
						float centre : TEXCOORD2;
						float3 lightdir : TEXCOORD3;
						float3 viewdir : TEXCOORD4;
						float3 worldTangent : TEXCOORD5;
						float3 worldBinormal : TEXCOORD6;
						float3 worldPos: TEXCOORD7;
						float3 vertPos: TEXCOORD8;
						UNITY_VERTEX_OUTPUT_STEREO //v0.1
					};
					fixed4 _Color;
					#define PI 3.1415926535897911

					float _SphereRadius;
					float _PlanetRadius;
					float _AtmosphereHeight;
					float atmosphereRadius;
					float _AtmosphereModifier;
					float _ScatteringModifier;
					fixed4 _AtmosphereColor;
					float UnitsToMetres;
					float3 worldCentre;
					float3 worldPos;
					float3 _PlanetCentre;
					float3 spacePos;
					float3 _RayScatteringCoefficient;
					float _RayScaleHeight;
					float _MScatteringCoefficient;
					float _MScaleHeight;
					float _MAnisotropy;
					float _SunIntensity;
					int _ViewSamples;
					int _LightSamples;

					v2f vert(appdata_full v) {
						v2f o;

						UNITY_SETUP_INSTANCE_ID(v);//v0.1
						UNITY_INITIALIZE_OUTPUT(v2f, o);
						UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);//v0.1

						o.vertex = UnityObjectToClipPos(v.vertex);
						const float MetresToUnits =  _SphereRadius / _PlanetRadius;
						v.vertex.xyz += MetresToUnits * ((_AtmosphereModifier*_AtmosphereHeight)) * v.normal.xyz;
						o.centre = mul(unity_ObjectToWorld, half4(0, 0, 0, 1));
						o.vertex = UnityObjectToClipPos(v.vertex);
						o.normal = v.normal;
						o.uv_MainTex = v.texcoord;////
						o.tangent = v.tangent;
						o.vertPos = v.vertex;
						// Fresnel (fade out when close to body)
						float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
						/*float3 bodyWorldCentre = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
						float camRadiiFromSurface = (length(bodyWorldCentre - _WorldSpaceCameraPos.xyz) - bodyScale) / bodyScale;
						float fresnelT = smoothstep(0, 1, camRadiiFromSurface);
						float3 viewDir = normalize(worldPos - _WorldSpaceCameraPos.xyz);
						float3 normWorld = normalize(mul(unity_ObjectToWorld, float4(v.normal, 0)));
						float fresStrength = lerp(_FresnelStrengthNear, _FresnelStrengthFar, fresnelT);*/
						//o.fresnel = saturate(fresStrength * pow(1 + dot(viewDir, normWorld), _FresnelPow));
						float3 viewDir = normalize(worldPos - _WorldSpaceCameraPos.xyz);

						o.worldPos = worldPos;
						o.worldNormal = UnityObjectToWorldNormal(v.normal);

						//URP - HDRP
						if (_passSunExternally == 1) {
							_WorldSpaceLightPos0.xyz = -globalSunPosition.xyz;
						}

						float3 lightDir =(worldPos.xyz - _WorldSpaceLightPos0.xyz);
						o.lightdir = normalize(lightDir);
						o.viewdir = viewDir;
						float3 worldNormal = mul((float3x3)unity_ObjectToWorld, v.normal);
						float3 worldTangent = mul((float3x3)unity_ObjectToWorld, v.tangent);
						float3 binormal = cross(v.normal, v.tangent.xyz);
						float3 worldBinormal = mul((float3x3)unity_ObjectToWorld, binormal);
						o.worldTangent = normalize(worldTangent);
						o.worldBinormal = normalize(worldBinormal);
						return o;
					}

					//v0.2
					bool solveQuadratic(float a, float b, float c, out float x0, out float x1)
					{
						float discr = b * b - 4 * a * c;
						if (discr < 0) return false;
						else if (discr == 0) x0 = x1 = -0.5 * b / a;
						else {
							float q = (b > 0) ?
								-0.5 * (b + sqrt(discr)) :
								-0.5 * (b - sqrt(discr));
							x0 = q / a;
							x1 = c / q;
						}
						//if (x0 > x1) std::swap(x0, x1);
						if (x0 > x1) {
							float temp = x0;
							x0 = x1;
							x1 = temp;
						}
						return true;
					}

					//v0.2
					float absorb;
					//https://stackoverflow.com/questions/40157187/sphere-ray-intersection-on-glsl-different-behaviours-depending-on-device
					//http://www.scratchapixel.com/lessons/3d-basic-rendering/minimal-ray-tracer-rendering-simple-shapes/ray-sphere-intersection
					//float2 intersectionsWithSphere(float3 o, float3 d, float3 shift, float r, out float AO, out float BO) {//origin, direction, radius
					bool intersectionsWithSphere(float3 origin, float3 dir, float3 shift, float radius, out float AO, out float BO) {//origin, direction, radius


						// float a = dot(d, d);
						// float b = 2.0 * dot(shift - o, d);
						// float c = dot(shift - o, shift - o) - r * r;// pow(r, 2.0);

						// float q = b * b - 4.0 * a * c;// pow(b, 2.0) - 4.0 * a * c;
						//	if (q < 0.0) {
						//		return false;
						//		//return float2(-1.0, -1.0);
						//	}

						// float sq = sqrt(q);
						// float t1 = (-b - sq) / (2.0*a);
						// float t2 = (-b + sq) / (2.0*a);

						//if (t1 > t2) {
						//	float a = t2;
						//	t2 = t1;
						//	t1 = a;
						//}
						//AO = t2;
						//BO = t1;
						////return float2(t1, t2);

						//http://www.scratchapixel.com/lessons/3d-basic-rendering/minimal-ray-tracer-rendering-simple-shapes/ray-sphere-intersection
						float t0, t1;
						float radius2 = radius * radius;
						if (1 == 0) {
							// geometric solution
							float3 L = shift - origin;
							float tca = dot(L, dir);
							// if (tca < 0) return false;
							float d2 = dot(L, L) - tca * tca;
							if (d2 > radius2) return false;
							float thc = sqrt(radius2 - d2);
							t0 = tca - thc;
							t1 = tca + thc;
						}
						else {
							// analytic solution
							float3 L = origin -shift;
							float a = CongtrolVars.z  *dot(dir, dir);
							float b = 2 * dot(L, dir);
							float c = dot(L, L) - 1*radius2;
							if (!solveQuadratic(a, b, c, t0, t1)) return false;
						}
						if (t0 > t1) {
							float temp = t0;
							t0 = t1;
							t1 = temp;
						}
						//if (t0 < 0) {
						//    t0 = t1; // if t0 is negative, let's use t1 instead 
						//    if (t0 < 0) return false; // both t0 and t1 are negative 
						//}
						AO = t0;
						BO = t1;
						return true;
					}

					bool rayInstersection(float3 O,float3 D,float3 C,float R,out float AO,out float BO)
					{
						float DT = dot(D, C - O);
						float R2 = pow(R, 2);
						float CT2 = dot(C - O, C - O) - pow(DT,2);
						if (CT2 > R2)
						{
							return false;
						}
						float AT = sqrt(R2 - CT2);
						float BT = AT;
						AO = DT - AT;
						BO = DT + BT;
						return true;
					}

					bool lightSampling(float3 PA, float3 SA, out float opticalDepthRay, out float opticalDepthM)
					{
						float C1A;
						float C2A;
						intersectionsWithSphere(PA, SA, _PlanetCentre, atmosphereRadius, C1A, C2A);//rayInstersection(PA, SA, _PlanetCentre, atmosphereRadius, C1A, C2A);

					/*	if (C1A < 0) {
							return false;
						}*/


						opticalDepthRay = 0;
						opticalDepthM = 0;

						float time = 0;
						float3 CA = PA + SA * C2A;
						float lightSampleSize = distance(PA, CA) / (float)(_LightSamples);

						for (int i = 0; i < _LightSamples; i++)
						{
							float3 QA = PA +  SA * (time + lightSampleSize * 0.5);
							float height = distance(_PlanetCentre, QA) - _PlanetRadius;
							if (height < 0)
								return false;

							opticalDepthRay += exp(-height / _RayScaleHeight) * lightSampleSize;
							opticalDepthM += exp(-height / _MScaleHeight) * lightSampleSize;
							time += lightSampleSize;
						}
						return true;
					}

					float4 LightingScattering(float3 normal, float3 viewDir, float3 lightDir)
					{

						float tA;
						float tB;
						if (!intersectionsWithSphere(spacePos, -viewDir, _PlanetCentre, atmosphereRadius, tA, tB)) {
							//if (!rayInstersection(spacePos, -viewDir, _PlanetCentre, atmosphereRadius, tA, tB)) {
							return fixed4(0, 0, 0, 0);
						}

						float pA, pB;
						if (intersectionsWithSphere(spacePos * 1, -viewDir, _PlanetCentre, _PlanetRadius, pA, pB))
							//if (rayInstersection(spacePos, -viewDir, _PlanetCentre, _PlanetRadius, pA, pB))
						{
							if (pA < 0) { // if poijt behind camera
								pA = pB; //return 0 ;
							}
							tB = pA;
						}

						float opticalDepthPA = 0;
						float opticalDepthMA = 0;
						float3 totalRayScattering = float3(0, 0, 0);
						float totalMScattering = 0;

						float time = tA;
						float viewSampleSize = (tB - tA) / (float)(_ViewSamples); //float step_size_i = (ray_length.y - ray_length.x) / float(steps_i);
						for (int i = 0; i < _ViewSamples; i++)
						{
							//https://www.shadertoy.com/view/wlBXWK
							//https://github.com/Dimev/atmosphere-shader
							//https://github.com/Dimev/atmosphere-shader/blob/main/shaders/Image.glsl
							//vec3 pos_i = start + dir * ray_pos_i; //float ray_pos_i = ray_length.x + step_size_i * 0.5;
							//how high we are above the surface
							//float height_i = length(pos_i) - planet_radius;
							//now calculate the density of the particles (both for rayleigh and mie)
							//vec3 density = vec3(exp(-height_i / scale_height), 0.0);
							// and the absorption density. this is for ozone, which scales together with the rayleigh, 
							// but absorbs the most at a specific height, so use the sech function for a nice curve falloff for this height
							// clamp it to avoid it going out of bounds. This prevents weird black spheres on the night side
							//float denom = (height_absorption - height_i) / absorption_falloff;
							//density.z = (1.0 / (denom * denom + 1.0)) * density.x;

							float3 P = spacePos - CongtrolVars.w  * viewDir * (time + 0.5*viewSampleSize);
							float height = CongtrolVars.x *distance(P, _PlanetCentre) - CongtrolVars.y *_PlanetRadius;
							float viewopticalDepthPA = exp(-height / _RayScaleHeight) * viewSampleSize; //density *= step_size_i;
							float viewopticalDepthMA = exp(-height / _MScaleHeight) * viewSampleSize; //vec2 scale_height = vec2(height_ray, height_mie);
							opticalDepthPA += viewopticalDepthPA;
							opticalDepthMA += viewopticalDepthMA;
							float lightopticalDepthPA = 0;
							float lightopticalDepthMA = 0;

							//v0.2
							float height_absorption = 1e3;
							float absorption_falloff = 1e2;
							float denom = (height_absorption - height) / absorption_falloff;
							opticalDepthMA = (1 - absorb)* opticalDepthMA + (absorb)*(1.0 / (denom * denom + 1.0)) * opticalDepthPA;


							bool overground = lightSampling(P, lightDir, lightopticalDepthPA, lightopticalDepthMA);///v0.1
							if (overground)
							{
								float3 attenuation = exp(-(_RayScatteringCoefficient * (opticalDepthPA + lightopticalDepthPA) +
									_MScatteringCoefficient * (opticalDepthMA + lightopticalDepthMA)));

								totalRayScattering += viewopticalDepthPA * attenuation;
								totalMScattering += viewopticalDepthMA * attenuation;
							}
							time += viewSampleSize;
						}

						float cosTheta = dot(viewDir, lightDir);
						float cos2Theta = pow(cosTheta, 2);
						float g = _MAnisotropy;
						float g2 = pow(g, 2);
						float rayPhase = 3 / (16 * PI) * (cos2Theta + 1);
						float mPhase = ((1 - g2) * (cos2Theta + 1)) / (pow(g2 - g * 2 * cosTheta + 1, 1.5) * (g2 + 2)) * (3 / (8 * PI));

						float3 scattering = _SunIntensity * ((rayPhase * _RayScatteringCoefficient) * totalRayScattering +
							(mPhase * _MScatteringCoefficient) * totalMScattering);

						fixed4 col = _AtmosphereColor;
						col.rgb *= scattering * col.a;
						col.rgb = min(col.rgb, 1);
						return col;
					}


				float4 frag(v2f IN) : SV_Target{

					float4 cAlbedo = tex2D(_MainTex, IN.uv_MainTex);
					planetCenter = mul(unity_ObjectToWorld, half4(0, 0, 0, 1));
					worldCentre = -float3(planetCenter.xyz) * (_SphereRadius - 1);//float3(0, 0, 0);
					worldPos = IN.worldPos;
					UnitsToMetres = _PlanetRadius / _SphereRadius;
					planetCenter = _PlanetRadius * planetCenter;
					_PlanetCentre = float3(0, 0, 0) + planetCenter.xyz;
					spacePos = UnitsToMetres * (worldPos - worldCentre);
					atmosphereRadius = (_AtmosphereHeight * _AtmosphereModifier) + _PlanetRadius;

					//v0.1
					float distA = length(_WorldSpaceCameraPos - planetCenter);
					float distB = length(IN.worldPos - planetCenter);
					if (distA > distB + 111110.2) {//if (distA > distB +0.1) {
						//discard;
					}

					//		spacePos = UnitsToMetres * (_WorldSpaceCameraPos )+float3(0, 1, 0);
					//		spacePos = _WorldSpaceCameraPos;	
							//atmosphereRadius = length(_WorldSpaceCameraPos - planetCenter* UnitsToMetres)*0.000000001;

							_MScaleHeight *= _AtmosphereModifier;
							_RayScaleHeight *= _AtmosphereModifier;
							_MScatteringCoefficient *= _ScatteringModifier;
							_RayScatteringCoefficient *= _ScatteringModifier;

							// Glossiness
							float glossiness = dot(cAlbedo.rgb, 1) / 3 * _Glossiness;
							//glossiness = max(glossiness, snowWeight * _SnowSpecular);

							//URP - HDRP
							if (_passSunExternally == 1) {
								_WorldSpaceLightPos0.xyz = -1 * globalSunPosition.xyz;
							}

							float3 lightDir = normalize(_WorldSpaceLightPos0);
							float3 diffuse = saturate(dot(IN.worldNormal, -lightDir));
							diffuse = cAlbedo.rgb * diffuse  * _SunStrength;
							diffuse = saturate(pow(diffuse, _SunPower))*_SunPower + diffuse * 1;

							float3 specular = 0;
							if (diffuse.x > 0) {
								float3 reflection = reflect(lightDir, IN.normal);
								float3 viewDir = normalize(IN.viewdir);
								specular = saturate(dot(reflection, -viewDir));
								specular = pow(specular, 20.0f);
							}

							float4 scatter = LightingScattering(IN.normal, _LightDirectionFX.x * IN.viewdir + _LightDirectionFX.yzw, _WorldSpaceLightPos0);

							if (_passSunExternally == 1) {
								scatter.rbg = scatter.rbg * globalSunColor.xyz*globalSunColor.w;
							}
							//float distC = length(IN.worldPos - planetCenter) + 0.1;
							float distC = length(spacePos - _WorldSpaceCameraPos) + 0.1;
							//distC = length(IN.worldPos * UnitsToMetres - atmosphereRadius * UnitsToMetres);
							//return float4(pow(scatter.rgb,1) * 1, (cAlbedo.a * scatter.a -0.3* (0.3-scatter.r)) * (2111111111110.5/pow(distB,1.64))   );
							//return float4(pow(scatter.rgb, 1) *	1, (cAlbedo.a * scatter.a - 0.3* (0.3 - scatter.r))*  1 );
							//return float4(1, 0, 0, 1);
							//return float4(pow(scatter.rgb, 1) * 1, (cAlbedo.a * scatter.a - 0.3* (0.3 - scatter.r))*  1110.5 * (1/(distC))  );
							//return float4(scatter.rgb, saturate(1- (cAlbedo.a * scatter.a - 0.3* (0.3 - scatter.r))*  111.5 * (1 / (distC)))    );
							float4 outFinal = float4(scatter.rgb, saturate((cAlbedo.a * scatter.a - 0.3* (0.3 - scatter.r)) * 2111111111 * (1 / pow(distC,1.2))));

							outFinal.rgb = pow(outFinal.rgb, contrast) + contrastAtmo * 4 * scatter.rgb;
							outFinal.a += outFinal.a * contrastAtmo*1.8 * outFinal.a* outFinal.a;
							//outFinal.r = outFinal.a*outFinal.a*outFinal.a*(outFinal.r * outFinal.r + 0.9);
							//outFinal.g = outFinal.a*outFinal.a*outFinal.a*(outFinal.g * outFinal.g + 0.9);
							//outFinal.b = outFinal.a*outFinal.a*outFinal.a*(outFinal.b * outFinal.b + 0.9);

							return outFinal;
						}
						ENDCG
					}
			////END PASS 2
		}
}
//Shader "ORION/Scattering/Atmospheric Scattering Only Atmos HDRP SURF TRANSP 2"
//{
//	Properties
//	{
//		_LightIntensity("Light Intensity", Float) = 30
//		_LightColor("Light Color", Color) = (1,1,1)
//		_LightDirection("Light Direction", Vector) = (0,0,1)
//		_PlanetRadius("Planet Radius", Float) = 47
//		_AtmosphereRadius("Atmosphere Radius", Float) = 50
//		_Steps("Steps", Int) = 20
//		_LightSteps("Light Steps", Int) = 12
//		_RayleighScattering("Rayleigh Scattering", Vector) = (0.08,0.2,0.51,0.64)
//		_MieScattering("Mie Scattering", Vector) = (0.01, 0.9, 0, 0.8)
//		_ClipThreshold("Clip Threshold", Range(0.0,1.0)) = 0.73
//	}
//		SubShader
//	{
//		//Blend OneMinusDstColor One
//		Blend SrcAlpha OneMinusSrcAlpha
//		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
//		LOD 100
//
//		ZWrite Off
//		Cull Off
//
//		Pass
//		{
//			CGPROGRAM
//			#pragma vertex vert
//			#pragma fragment frag
//
//			#include "UnityCG.cginc"
//			#define PI 3.14159265
//
//			struct appdata
//			{
//				float4 vertex : POSITION;
//				float4 normal : NORMAL;
//			};
//
//			struct v2f
//			{
//				float4 vertex : SV_POSITION;
//				float4 normal : TEXCOORD2;
//				float3 viewDir : TEXCOORD1;
//				float3 startPos : TEXCOORD3;
//				float3 wPos : TEXCOORD4;
//			};
//
//			float _LightIntensity;
//			float3 _LightColor;
//			float3 _LightDirection;
//			float _PlanetRadius;
//			float3 _PlanetCenter;
//			float _AtmosphereRadius;
//			float _Steps;
//			float _LightSteps;
//			float4 _RayleighScattering;
//			float4 _MieScattering;
//			float _ClipThreshold;
//
//			float sqrLength(float3 v) {
//				return (v.x * v.x + v.y * v.y + v.z * v.z);
//			}
//
//			bool SphereIntersect(float3 ro, float3 rd, out float t0, out float t1, bool isPlanet) {
//				float t = dot(_PlanetCenter - ro, rd);
//				float3 pM = ro + rd * t;
//				float height = sqrLength(pM - _PlanetCenter);
//				if (height > _AtmosphereRadius * _AtmosphereRadius) return false;
//				float x = sqrt(_AtmosphereRadius * _AtmosphereRadius - height);
//				t0 = (t - x < 0) ? 0 : t - x;
//				if (isPlanet && height < _PlanetRadius * _PlanetRadius && t > 0) {
//					float x = sqrt(_PlanetRadius * _PlanetRadius - height);
//					t1 = t - x;
//				}
//				else {
//					t1 = t + x;
//				}
//				return true;
//			}
//
//bool LightMarch(float3 p1, float3 rd, float l, out float2 lightDepth) {
//	float ds = l / _LightSteps;
//	float time = 0;
//	lightDepth = float2(0, 0);
//	for (int i = 0; i < _LightSteps; i++)
//	{
//		float3 p = p1 + rd * (time + ds * 0.5);
//		float height = length(p - _PlanetCenter) - _PlanetRadius;
//
//		if (height < 0) return false;
//
//		lightDepth.x += exp(-height / _RayleighScattering.w) * ds;
//		lightDepth.y += exp(-height / _MieScattering.w) * ds;
//
//		time += ds;
//	}
//	return true;
//}
//
//v2f vert(appdata v)
//{
//	v2f o;
//	o.vertex = UnityObjectToClipPos(v.vertex);
//	o.startPos = mul(unity_ObjectToWorld, v.vertex).xyz;
//	o.wPos = o.startPos;
//	o.viewDir = normalize(o.startPos - _WorldSpaceCameraPos.xyz);
//	v.normal *= -1;
//	o.normal = v.normal;
//	_PlanetCenter = mul(unity_ObjectToWorld, float4(0, 0, 0, 1));
//	return o;
//}
//
//fixed4 frag(v2f i) : SV_Target
//{
//	//if (_PlanetRadius > _AtmosphereRadius) _PlanetRadius = _AtmosphereRadius - 2;
//	if (_AtmosphereRadius < 0) _AtmosphereRadius = 1;
//
//	float3 rsRGB = float3(_RayleighScattering.xyz);
//	float msRGB = _MieScattering.x;
//	float rSH = _RayleighScattering.w;
//	float mSH = _MieScattering.w;
//
//	i.viewDir = normalize(i.startPos - _WorldSpaceCameraPos.xyz);
//	i.startPos = _WorldSpaceCameraPos;
//
//	float t0,t1;
//	if (!SphereIntersect(i.startPos, i.viewDir, t0, t1, true)) discard;
//
//	float mu = dot(i.viewDir, normalize(-_LightDirection));
//	float g = _MieScattering.y;
//	float phaseR = 3.0 / (16.0 * PI) * (1 + mu * mu);
//	float phaseM = 3.0 / (8.0 * PI) * ((1.f - g * g) * (1.f + mu * mu)) / ((2.f + g * g) * pow(1.f + g * g - 2.f * g * mu, 1.5f));
//
//	float3 sumR, sumM;
//	float2 opticalDepth;
//
//	float3 p1 = i.startPos + i.viewDir * t0;
//	float l = t1 - t0;
//	float ds = l / _Steps;
//	float time = 0;
//	for (int e = 0; e < _Steps; e++)
//	{
//		float3 p = p1 + i.viewDir * (time + ds * 0.5);
//		float3 lrd = normalize(-_LightDirection);
//
//		float lt0, lt1;
//		SphereIntersect(p, lrd, lt0, lt1, false);
//		float2 opticallightDepth;
//		float3 lp1 = p + lrd * lt0;
//		if (LightMarch(lp1, lrd, lt1 - lt0, opticallightDepth)) {
//			float height = length(p - _PlanetCenter) - _PlanetRadius;
//
//			float hr = exp(-height / rSH) * ds;
//			float hm = exp(-height / mSH) * ds;
//
//			opticalDepth.x += hr;
//			opticalDepth.y += hm;
//
//			float3 tau = rsRGB * (opticalDepth.x + opticallightDepth.x) + msRGB * 1.1 * (opticalDepth.y + opticallightDepth.y);
//			float3 attenuation = float3 (exp(-tau.x), exp(-tau.y), exp(-tau.z));
//
//			sumR += attenuation * hr;
//			sumM += attenuation * hm;
//		}
//
//		time += ds;
//	}
//
//	float3 color = (sumR * rsRGB * phaseR + sumM * msRGB * phaseM) * _LightIntensity * _LightColor;
//
//	float a = pow(saturate(sqrLength(_WorldSpaceCameraPos.xyz - i.wPos) - 0.4),2);
//	float a1 = (color.x + color.y + color.z) / 3;
//	if (a1 < _ClipThreshold)
//		a1 = lerp(0,a1, a1 / _ClipThreshold);
//
//	return fixed4(color.xyz, min(a,a1));
//}
//ENDCG
//}
//	}
//}