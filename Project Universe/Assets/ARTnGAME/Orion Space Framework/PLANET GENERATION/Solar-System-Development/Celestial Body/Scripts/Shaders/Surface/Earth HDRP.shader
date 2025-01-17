﻿Shader "ORION/Celestial/Earth HDRP"
{
	Properties
	{
		[Header(Flat Terrain)]
		_ShoreLow("Shore Low", Color) = (0,0,0,1)
		_ShoreHigh("Shore High", Color) = (0,0,0,1)
		_FlatLowA("Flat Low A", Color) = (0,0,0,1)
		_FlatHighA("Flat High A", Color) = (0,0,0,1)

		_FlatLowB("Flat Low B", Color) = (0,0,0,1)
		_FlatHighB("Flat High B", Color) = (0,0,0,1)

		_FlatColBlend("Colour Blend", Range(0,3)) = 1.5
		_FlatColBlendNoise("Blend Noise", Range(0,1)) = 0.3
		_ShoreHeight("Shore Height", Range(0,0.2)) = 0.05
		_ShoreBlend("Shore Blend", Range(0,0.2)) = 0.03
		_MaxFlatHeight("Max Flat Height", Range(0,1)) = 0.5

		[Header(Steep Terrain)]
		_SteepLow("Steep Colour Low", Color) = (0,0,0,1)
		_SteepHigh("Steep Colour High", Color) = (0,0,0,1)
		_SteepBands("Steep Bands", Range(1, 20)) = 8
		_SteepBandStrength("Band Strength", Range(-1,1)) = 0.5

		[Header(Flat to Steep Transition)]
		_SteepnessThreshold("Steep Threshold", Range(0,1)) = 0.5
		_FlatToSteepBlend("Flat to Steep Blend", Range(0,0.3)) = 0.1
		_FlatToSteepNoise("Flat to Steep Noise", Range(0,0.2)) = 0.1

		[Header(Snowy Poles)]
		[Toggle()]
	  _UseSnowyPoles("Use Poles", float) = 0
		_SnowCol("Snow Colour", Color) = (1,1,1,1)
		_SnowLongitude("Snow Longitude", Range(0,1)) = 0.8
		_SnowBlend("Snow Blend", Range(0, 0.2)) = 0.1
		_SnowSpecular("Snow Specular", Range(0,1)) = 1
		_SnowHighlight("Snow Highlight", Range(1,2)) = 1.2
		_SnowNoiseA("Snow Noise A", Range(0,10)) = 5
		_SnowNoiseB("Snow Noise B", Range(0,10)) = 4

		[Header(Noise)]
		[NoScaleOffset] _NoiseTex("Noise Texture", 2D) = "white" {}
		_NoiseScale("Noise Scale", Float) = 1
		_NoiseScale2("Noise Scale2", Float) = 1

		[Header(Other)]
		_FresnelCol("Fresnel Colour", Color) = (1,1,1,1)
		_FresnelStrengthNear("Fresnel Strength Min", float) = 2
		_FresnelStrengthFar("Fresnel Strength Max", float) = 5
		_FresnelPow("Fresnel Power", float) = 2
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0

		_TestParams("Test Params", Vector) = (0,0,0,0)
			_SunStrength("Sun Strength", float) = 1
			_SunPower("Sun Power", float) = 1
	}
		SubShader
		{
			Tags { "RenderType" = "Opaque" }
			LOD 200
		   Pass {
			CGPROGRAM

			// Physically based Standard lighting model, and enable shadows on all light types
			//#pragma surface surf Standard fullforwardshadows vertex:vert
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.5
					#include "UnityCG.cginc"
				////HDRP
				#include "Lighting.cginc"
				#include "AutoLight.cginc"
			#include "../Includes/Triplanar.cginc"
			#include "../Includes/Math.cginc"

			float4 _TestParams;
			float4 _FresnelCol;
			float _FresnelStrengthNear;
			float _FresnelStrengthFar;
			float _FresnelPow;
			float bodyScale;

			/*struct Input
			{
				float2 uv_MainTex;
				float3 worldPos;
				float4 terrainData;
				float3 vertPos;
				float3 normal;
				float4 tangent;
				float fresnel;
			};*/
			struct v2f
			{
				//float2 uv : TEXCOORD0;
				//fixed4 diff : COLOR0;
				//float4 vertex : SV_POSITION;

				float4 vertex: SV_POSITION;
				float3 normal: NORMAL;
				float4 terrainData: TEXCOORD0;
				float3 worldNormal: TEXCOORD1;
				float4 tangent: TANGET;
				//float4 craterUV: TEXCOORD2;
				float fresnel : TEXCOORD2;

				float3 lightdir : TEXCOORD3;
				float3 viewdir : TEXCOORD4;
				float3 worldTangent : TEXCOORD5;
				float3 worldBinormal : TEXCOORD6;
				float3 worldPos: TEXCOORD7;
				float3 vertPos: TEXCOORD8;
				//INTERNAL_DATA
			};

			v2f vert(appdata_full v)//void vert (inout appdata_full v, out Input o)
			{
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.normal = v.normal;
				o.terrainData = v.texcoord;
				o.tangent = v.tangent;
				o.vertPos = v.vertex;
				// Fresnel (fade out when close to body)
				float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				float3 bodyWorldCentre = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
				float camRadiiFromSurface = (length(bodyWorldCentre - _WorldSpaceCameraPos.xyz) - bodyScale) / bodyScale;
				float fresnelT = smoothstep(0,1,camRadiiFromSurface);
				float3 viewDir = normalize(worldPos - _WorldSpaceCameraPos.xyz);
				float3 normWorld = normalize(mul(unity_ObjectToWorld, float4(v.normal,0)));
				float fresStrength = lerp(_FresnelStrengthNear, _FresnelStrengthFar, fresnelT);
				o.fresnel = saturate(fresStrength * pow(1 + dot(viewDir, normWorld), _FresnelPow));

				o.worldPos = worldPos;
				o.worldNormal = UnityObjectToWorldNormal(v.normal);// normalize(mul(float4(v.normal, 0.0), unity_ObjectToWorld).xyz);


				//MINE	
				float3 lightDir = worldPos.xyz - _WorldSpaceLightPos0.xyz   - _WorldSpaceCameraPos.xyz;//HDRP add camera
				o.lightdir = normalize(lightDir);
				o.viewdir = viewDir;
				float3 worldNormal = mul((float3x3)unity_ObjectToWorld, v.normal);
				float3 worldTangent = mul((float3x3)unity_ObjectToWorld, v.tangent);
				float3 binormal = cross(v.normal, v.tangent.xyz); // *input.tangent.w;
				float3 worldBinormal = mul((float3x3)unity_ObjectToWorld, binormal);
				o.worldTangent = normalize(worldTangent);
				o.worldBinormal = normalize(worldBinormal);
				return o;
			}

			// Flat terrain:
			float4 _ShoreLow;
			float4 _ShoreHigh;

			float4 _FlatLowA;
			float4 _FlatHighA;
			float4 _FlatLowB;
			float4 _FlatHighB;

			float _FlatColBlend;
			float _FlatColBlendNoise;
			float _ShoreHeight;
			float _ShoreBlend;
			float _MaxFlatHeight;

			// Steep terrain
			float4 _SteepLow;
			float4 _SteepHigh;
			float _SteepBands;
			float _SteepBandStrength;

			// Flat to steep transition
			float _SteepnessThreshold;
			float _FlatToSteepBlend;
			float _FlatToSteepNoise;

			// Snowy poles
			float _UseSnowyPoles;
			float3 _SnowCol;
			float _SnowLongitude;
			float _SnowBlend;
			float _SnowSpecular;
			float _SnowHighlight;
			float _SnowNoiseA;
			float _SnowNoiseB;

			// Other:
			float _Glossiness;
			float _Metallic;

			sampler2D _NoiseTex;
			sampler2D _SnowNormal;
			float _NoiseScale;
			float _NoiseScale2;


			// Height data:
			float2 heightMinMax;
			float oceanLevel;
			float _SunStrength;
			float _SunPower;

			float4 frag(v2f IN) : SV_Target //void surf (Input IN, inout SurfaceOutputStandard o)
			{

				// Calculate steepness: 0 where totally flat, 1 at max steepness
				float3 sphereNormal = normalize(IN.vertPos);
				float steepness = 1 - dot(sphereNormal, IN.worldNormal);
				steepness = remap01(steepness, 0, 0.65);

				// Calculate heights
				float terrainHeight = length(IN.vertPos);
				float shoreHeight = lerp(heightMinMax.x, 1, oceanLevel);
				float aboveShoreHeight01 = remap01(terrainHeight, shoreHeight, heightMinMax.y);
				float flatHeight01 = remap01(aboveShoreHeight01, 0, _MaxFlatHeight);

				// Sample noise texture at two different scales
				float4 texNoise = triplanar(IN.vertPos, IN.normal, _NoiseScale, _NoiseTex);
				float4 texNoise2 = triplanar(IN.vertPos, IN.normal, _NoiseScale2, _NoiseTex);

				// Flat terrain colour A and B
				float flatColBlendWeight = Blend(0, _FlatColBlend, (flatHeight01 - .5) + (texNoise.b - 0.5) * _FlatColBlendNoise);
				float3 flatTerrainColA = lerp(_FlatLowA, _FlatHighA, flatColBlendWeight);
				flatTerrainColA = lerp(flatTerrainColA, (_FlatLowA + _FlatHighA) / 2, texNoise.a);
				float3 flatTerrainColB = lerp(_FlatLowB, _FlatHighB, flatColBlendWeight);
				flatTerrainColB = lerp(flatTerrainColB, (_FlatLowB + _FlatHighB) / 2, texNoise.a);

				// Biome
				float biomeWeight = Blend(_TestParams.x, _TestParams.y,IN.terrainData.x);
				biomeWeight = Blend(0, _TestParams.z, IN.vertPos.x + IN.terrainData.x * _TestParams.x + IN.terrainData.y * _TestParams.y);
				float3 flatTerrainCol = lerp(flatTerrainColA, flatTerrainColB, biomeWeight);

				// Shore
				float shoreBlendWeight = 1 - Blend(_ShoreHeight, _ShoreBlend, flatHeight01);
				float4 shoreCol = lerp(_ShoreLow, _ShoreHigh, remap01(aboveShoreHeight01, 0, _ShoreHeight));
				shoreCol = lerp(shoreCol, (_ShoreLow + _ShoreHigh) / 2, texNoise.g);
				flatTerrainCol = lerp(flatTerrainCol, shoreCol, shoreBlendWeight);

				// Steep terrain colour
				float3 sphereTangent = normalize(float3(-sphereNormal.z, 0, sphereNormal.x));
				float3 normalTangent = normalize(IN.normal - sphereNormal * dot(IN.normal, sphereNormal));
				float banding = dot(sphereTangent, normalTangent) * .5 + .5;
				banding = (int)(banding * (_SteepBands + 1)) / _SteepBands;
				banding = (abs(banding - 0.5) * 2 - 0.5) * _SteepBandStrength;
				float3 steepTerrainCol = lerp(_SteepLow, _SteepHigh, aboveShoreHeight01 + banding);

				// Flat to steep colour transition
				float flatBlendNoise = (texNoise2.r - 0.5) * _FlatToSteepNoise;
				float flatStrength = 1 - Blend(_SteepnessThreshold + flatBlendNoise, _FlatToSteepBlend, steepness);
				float flatHeightFalloff = 1 - Blend(_MaxFlatHeight + flatBlendNoise, _FlatToSteepBlend, aboveShoreHeight01);
				flatStrength *= flatHeightFalloff;

				// Snowy poles
				float3 snowCol = 0;
				float snowWeight = 0;
				float snowLineNoise = IN.terrainData.y * _SnowNoiseA * 0.01 + (texNoise.b - 0.5) * _SnowNoiseB * 0.01;
				snowWeight = Blend(_SnowLongitude, _SnowBlend, abs(IN.vertPos.y + snowLineNoise)) * _UseSnowyPoles;
				float snowSpeckle = 1 - texNoise2.g * 0.5 * 0.1;
				snowCol = _SnowCol * lerp(1, _SnowHighlight, aboveShoreHeight01 + banding) * snowSpeckle;

				// Set surface colour
				float3 compositeCol = lerp(steepTerrainCol, flatTerrainCol, flatStrength);
				compositeCol = lerp(compositeCol, snowCol, snowWeight);
				compositeCol = lerp(compositeCol, _FresnelCol, IN.fresnel);
				//		o.Albedo = compositeCol;

						// Glossiness
						float glossiness = dot(compositeCol, 1) / 3 * _Glossiness;
						glossiness = max(glossiness, snowWeight * _SnowSpecular);
						//		o.Smoothness = glossiness;
						//		o.Metallic = _Metallic;


								//MINE
								float3 lightDir = normalize(_WorldSpaceLightPos0);
								float3 diffuse = saturate(dot(IN.worldNormal, -lightDir));
								diffuse = compositeCol.rgb * diffuse  * _SunStrength;// *_LightColor0;
								diffuse = saturate(pow(diffuse, _SunPower))*_SunPower + diffuse*1;

								// Specular here
								float3 specular = 0;
								if (diffuse.x > 0) {
									float3 reflection = reflect(lightDir, IN.normal);
									float3 viewDir = normalize(IN.viewdir);

									specular = saturate(dot(reflection, -viewDir));
									specular = pow(specular, 20.0f);

									//float4 specularIntensity = tex2D(_SpecularMap, IN.uv);
									//specular *= _LightColor0 * 1;
								}

								return float4(diffuse + glossiness + specular, 1)*1.5;

				}
				ENDCG
			}
		}
			//FallBack "Diffuse"
}
