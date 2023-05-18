Shader "ORION/Scattering/Atmospheric Scattering Only Atmos HDRP LAND"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Main Texture", 2D) = "white" {}
		_Glossiness("_Glossiness", Range(0,1)) = 0.5
		_BumpMap("Bump Map", 2D) = "bump" {}
		_GlossyMap("Glossy Map", 2D) = "white" {}

		_CloudsTex("Clouds", 2D) = "black" {}
		_CloudsAlpha("Clouds Transparency", Float) = 0.25
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

			//v0.1
			normalPower("normalPower", float) = 0
			detailCloudsTiling("detailCloudsTiling", float) = 10
			detailNormalTiling("detailNormalTiling", float) = 10
			detailNormalPower("detailNormalPower", float) = 6
			normalPadding("normalPadding", Vector) = (0,0,0,0)
			normalOffsetScale("normalOffsetScale", Vector) = (0,0,1,1)
			normalSize("normalOffsetScale", Vector) = (1,1,0,0)
	}
		SubShader{

			Tags { "RenderType" = "Opaque"}
			LOD 200

		Pass {
	CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag			
			#include "UnityCG.cginc"				
			#include "Lighting.cginc"
			#include "AutoLight.cginc"
			#pragma target 3.0

			sampler2D _MainTex;
			sampler2D _BumpMap;
			sampler2D _GlossyMap;

			sampler2D _CloudsTex;
			fixed _CloudsAlpha;
			float _CloudsSpeed;
			int _CloudsAdditive;
			float _TerrainPower;
			half _Glossiness;
			float _Specular;
			fixed4 _Color;
			sampler2D _NightTex;
			fixed4 _NightColor;
			fixed _NightWrap;
			float _SunStrength;
			float _SunPower;
			float4 planetCenter;
			float4 _LightDirectionFX;

			//v0.1
			float normalPower;
			float detailCloudsTiling;
			float detailNormalTiling;
			float detailNormalPower;
			float4 normalPadding;
			float4 normalOffsetScale;
			float4 normalSize;

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
			};

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
				UNITY_INITIALIZE_OUTPUT(v2f,o);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.centre = mul(unity_ObjectToWorld, half4(0, 0, 0, 1));
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.normal = v.normal;
				o.uv_MainTex = v.texcoord;
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

				if (_passSunExternally == 1) {
					_WorldSpaceLightPos0.xyz = -globalSunPosition.xyz;
				}

				float3 lightDir = worldPos.xyz - _WorldSpaceLightPos0.xyz;
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

			//v0.1
			float2 invLerp2(float2 from, float2 to, float2 value) {
				return (value - from) / (to - from);
			}
			float4 invLerp4(float4 from, float4 to, float4 value) {
				return (value - from) / (to - from);
			}
			//https://www.ronja-tutorials.com/post/047-invlerp_remap/#remap
			float4 remap4(float4 origFrom, float4 origTo, float4 targetFrom, float4 targetTo, float4 value) {
				float4 rel = invLerp4(origFrom, origTo, value);
				return lerp(targetFrom, targetTo, rel);
			}
			float2 remap2(float2 origFrom, float2 origTo, float2 targetFrom, float2 targetTo, float2 value) {
				float2 rel = invLerp2(origFrom, origTo, value);
				return lerp(targetFrom, targetTo, rel);
			}

			//v0.1
			float mod(float x, float y) {
				return x - y * floor(x / y);
			}
		float4 frag(v2f IN) : SV_Target{
			float4 cAlbedo = tex2D(_MainTex, IN.uv_MainTex)* _Color; ;

			//v0.1
			float2 detailCloudsControl = float2(IN.uv_MainTex.xy*detailCloudsTiling);
			float4 cloudsA = tex2D(_CloudsTex, detailCloudsControl + fixed2(_Time.y * _CloudsSpeed, 0))* _CloudsAlpha;

			float4 clouds = tex2D(_CloudsTex, IN.uv_MainTex.xy*detailCloudsTiling + fixed2(-_Time.y * _CloudsSpeed*0.9, 0));
			if (_CloudsAdditive == 1)
			{
				cAlbedo.rgb = saturate(clouds.rgb * _CloudsAlpha + cAlbedo.rgb) + cloudsA;
			}
			else
			{
				cAlbedo.rgb = lerp(cAlbedo.rgb, clouds.rgb, _CloudsAlpha * clouds.a) + cloudsA;
			}
			float3 Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));

			float4 Smoothness = tex2D(_GlossyMap, IN.uv_MainTex) * _Glossiness * (1 - ((clouds).r*0.3 + (clouds).g*0.5901 + (clouds).b*.1102));

			if (_passSunExternally == 1) {
				_WorldSpaceLightPos0.xyz = -globalSunPosition.xyz;
			}

			//v0.1
			float2 startUVs = float2(IN.uv_MainTex.x*normalOffsetScale.z + normalOffsetScale.x, IN.uv_MainTex.y*normalOffsetScale.w + normalOffsetScale.y);
			//startUVs = remap2(float2(0,0), float2(1,1), normalPadding.xy, normalPadding.zw, startUVs);
			/*if (startUVs.y > 0.8) {
				startUVs.y = 0.8;
			}
			if (startUVs.y < 0.2) {
				startUVs.y = 0.2;
			}*/
			//normalPadding.x = normalPadding.x * detailNormalTiling;
			//normalPadding.y = normalPadding.y * detailNormalTiling;
			//normalPadding.z = normalPadding.z * detailNormalTiling;
			//normalPadding.w = normalPadding.w * detailNormalTiling;
			float UVdiff = normalPadding.y - normalPadding.x;
			float UVdiffX = normalPadding.z - normalPadding.w;
			startUVs.x = ((startUVs.x * UVdiffX) + normalPadding.z);
			startUVs.y = ((startUVs.y * UVdiff) + normalPadding.x);
			/*if (startUVs.y > 0.8) {
				startUVs.y = 0.8;
			}
			if (startUVs.y < 0.2) {
				startUVs.y = 0.2;
			}*/
			startUVs = float2(mod(startUVs.x, normalSize.x), mod(startUVs.y, normalSize.y));
			float2 detailNormalControl = startUVs * detailNormalTiling;
			float3 NormalA = UnpackNormal(tex2D(_BumpMap, detailNormalControl));
			NormalA = saturate(NormalA);

			//return float4(float3(NormalA.r*100,0,0), 1);

			IN.worldNormal = IN.worldNormal + normalPower*Normal - detailNormalPower * NormalA*(Normal+1.1);
			float3 N = IN.worldNormal;// WorldNormalVector(IN, Normal);
			float3 L = _WorldSpaceLightPos0.xyz;
			float NdotL = dot(N, L) - _NightWrap;
			float emissionStrength = saturate(-NdotL);
			float3 Emission = tex2D(_NightTex, IN.uv_MainTex).rgb * _NightColor * emissionStrength;

			// Glossiness
			float glossiness = dot(cAlbedo.rgb, 1) / 3 * _Glossiness;
			//glossiness = max(glossiness, snowWeight * _SnowSpecular);

			float3 lightDir = normalize(_WorldSpaceLightPos0);
			float3 diffuse = saturate(dot(IN.worldNormal, lightDir));
			diffuse = cAlbedo.rgb * diffuse  * _SunStrength;
			diffuse = saturate(pow(diffuse, _SunPower))*_SunPower + diffuse * 1;

			float3 specular = 0;
			if (diffuse.x > 0) {
				float3 reflection = reflect(lightDir, Normal);
				float3 viewDir = normalize(-IN.viewdir);
				specular = saturate(dot(reflection, -viewDir));
				specular = pow(specular, 20.0f);
			}

			if (_passSunExternally == 1) {
				diffuse.rbg = diffuse.rbg * globalSunColor.xyz*globalSunColor.w;
			}

			return float4(diffuse * (1 - emissionStrength) * 1 + Emission + specular * 0.5*_Specular + 0.28*Smoothness, cAlbedo.a)*_TerrainPower;

			  }
				ENDCG
			}


			////PASS 2
			Tags{ "RenderType" = "Transparent" 	"Queue" = "Transparent" }
				LOD 200
				Blend One One
				Cull Back

				Pass{
					CGPROGRAM
					#pragma vertex vert
					#pragma fragment frag			
					#include "UnityCG.cginc"				
					#include "Lighting.cginc"
					#include "AutoLight.cginc"
					#pragma target 3.0

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
						UNITY_INITIALIZE_OUTPUT(v2f,o);
						o.vertex = UnityObjectToClipPos(v.vertex);
						const float MetresToUnits = _SphereRadius / _PlanetRadius;
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

						float3 lightDir = worldPos.xyz - _WorldSpaceLightPos0.xyz;
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

					bool lightSampling(float3 PA, float3 SA,out float opticalDepthRay, out float opticalDepthM)
					{
							float C1A;
							float C2A;
							rayInstersection(PA, SA, _PlanetCentre, atmosphereRadius, C1A, C2A);
							opticalDepthRay = 0;
							opticalDepthM = 0;

							float time = 0;
							float3 CA = PA + SA * C2A;
							float lightSampleSize = distance(PA,CA) / (float)(_LightSamples);

							for (int i = 0; i < _LightSamples; i++)
							{
								float3 QA = PA + SA * (time + lightSampleSize * 0.5);
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
					if (!rayInstersection(spacePos, -viewDir, _PlanetCentre, atmosphereRadius, tA, tB)) {
						return fixed4(0, 0, 0, 0);
					}

					float pA, pB;
					if (rayInstersection(spacePos, -viewDir, _PlanetCentre, _PlanetRadius, pA, pB))
					{
						tB = pA;
					}

					float opticalDepthPA = 0;
					float opticalDepthMA = 0;
					float3 totalRayScattering = float3(0,0,0);
					float totalMScattering = 0;

					float time = tA;
					float viewSampleSize = (tB - tA) / (float)(_ViewSamples);
					for (int i = 0; i < _ViewSamples; i++)
					{
						float3 P = spacePos - viewDir * (time + 0.5*viewSampleSize);
						float height = distance(P, _PlanetCentre) - _PlanetRadius;
						float viewopticalDepthPA = exp(-height / _RayScaleHeight) * viewSampleSize;
						float viewopticalDepthMA = exp(-height / _MScaleHeight) * viewSampleSize;
						opticalDepthPA += viewopticalDepthPA;
						opticalDepthMA += viewopticalDepthMA;
						float lightopticalDepthPA = 0;
						float lightopticalDepthMA = 0;

						bool overground = lightSampling(P, lightDir, lightopticalDepthPA, lightopticalDepthMA);
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
					float rayPhase = 3 / (16*PI) * (cos2Theta + 1);
					float mPhase = ((1 - g2) * (cos2Theta + 1)) / (pow(g2 - g*2  * cosTheta + 1, 1.5) * (g2 + 2)) * (3 / (8*PI));

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
					spacePos = UnitsToMetres*(worldPos - worldCentre);
					atmosphereRadius = (_AtmosphereHeight * _AtmosphereModifier) + _PlanetRadius;
					_MScaleHeight *= _AtmosphereModifier;
					_RayScaleHeight *= _AtmosphereModifier;
					_MScatteringCoefficient *= _ScatteringModifier;
					_RayScatteringCoefficient *= _ScatteringModifier;
					
					// Glossiness
					float glossiness = dot(cAlbedo.rgb, 1) / 3 * _Glossiness;
					//glossiness = max(glossiness, snowWeight * _SnowSpecular);

					//URP - HDRP
					if (_passSunExternally == 1) {
						_WorldSpaceLightPos0.xyz = -globalSunPosition.xyz;
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

					return float4(pow(scatter.rgb,1) * 1, cAlbedo.a * scatter.a);
				}
				ENDCG
			}
				  ////END PASS 2
		}
}