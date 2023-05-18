// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld' //v3.4.6

//TUTORIAL
//https://stevencraeynest.wordpress.com/2013/03/29/easy-volumetric-explosion-in-unity3d/

Shader "ORION/SkyMaster/PYRO_PLANET_SM_HDRP" 
	{
		Properties
		{
			_RampTex("Color Ramp", 2D) = "white" {}
			_DispTex("Displacement Texture", 2D) = "gray" {}
			_Displacement("Displacement", Range(0, 1.0)) = 0.1
			_ChannelFactor("ChannelFactor (r,g,b)", Vector) = (1,0,0)
			_Range("Range (min,max)", Vector) = (0,0.5,0)
			_ClipRange("ClipRange [0,1]", float) = 0.8

				bodyScale("bodyScale", Float) = 1
				_FresnelStrengthNear("_FresnelStrengthNear", Float) = 1
				_FresnelStrengthFar("_FresnelStrengthFar", Float) = 1
				_FresnelPow("_FresnelPow", Float) = 1
				_GlowSpeed("_GlowSpeed", Float) =0
				dispScaleOffset("dispScaleOffset", Vector) = (1,1,0,0)
		}

			SubShader
			{
				Tags { "RenderType" = "Opaque" }
				Cull Off
				LOD 300
					Pass {
				CGPROGRAM
				//#pragma surface surf Lambert vertex:disp nolightmap addshadow //alpha:fade
				 #pragma vertex vert
				#pragma fragment frag
				#pragma target 3.0
				//#pragma glsl

				#include "UnityCG.cginc"
			////HDRP
			#include "Lighting.cginc"
			#include "AutoLight.cginc"

				sampler2D _DispTex;
				float _Displacement;
				float3 _ChannelFactor;
				float2 _Range;
				float _ClipRange;


				//M
				float bodyScale;
				float _FresnelStrengthNear;
				float _FresnelStrengthFar;
				float _FresnelPow;
				float _GlowSpeed;
				float4 dispScaleOffset;

				struct Input
				{
					float2 uv_DispTex;
				};

				void disp(inout appdata_full v)
				{
					float3 dcolor = tex2Dlod(_DispTex, float4(v.texcoord.xy * dispScaleOffset.xy,0,0) + float4(dispScaleOffset.zw,0,0) + float4(_Time.y, _Time.y,0,0)*0.01*_GlowSpeed);
					float d = (dcolor.r*_ChannelFactor.r + dcolor.g*_ChannelFactor.g + dcolor.b*_ChannelFactor.b);
					//d = d * abs(pow(cos(_Time.y*2)+1.5,2)*0.5);
					d = d * abs(pow(0.4*(cos(_Time.y * 2) + 1.5), 2)*0.65)+0.5*d;
					v.vertex.xyz += v.normal * d * _Displacement;
					v.normal.xyz += v.normal * d * _Displacement;
				}

				sampler2D _RampTex;


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
					//INTERNAL_DATA
				};

				v2f vert(appdata_full v)
				{
					v2f o;
					UNITY_INITIALIZE_OUTPUT(v2f, o);

					disp(v);
					

					o.vertex = UnityObjectToClipPos(v.vertex);
					o.terrainData = v.texcoord;
					o.normal = v.normal;
					o.tangent = v.tangent;
					//
					//float craterDst = v.texcoord.y;
					//float2 craterUV = 0.5 + float2(cos(v.texcoord.x), sin(v.texcoord.x)) * craterDst;
					//o.craterUV = float4(craterUV.xy, craterDst, 0);

					// Fresnel (fade out when close to body)
					float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
					float3 bodyWorldCentre = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
					float camRadiiFromSurface = (length(bodyWorldCentre - _WorldSpaceCameraPos.xyz) - bodyScale) / bodyScale;
					float fresnelT = smoothstep(0, 1, camRadiiFromSurface);
					float3 viewDir = normalize(worldPos - _WorldSpaceCameraPos.xyz);
					float3 normWorld = normalize(mul(unity_ObjectToWorld, float4(v.normal, 0)));
					float fresStrength = lerp(_FresnelStrengthNear, _FresnelStrengthFar, fresnelT);
					o.fresnel = saturate(fresStrength * pow(1 + dot(viewDir, normWorld), _FresnelPow));
					o.worldNormal = UnityObjectToWorldNormal(v.normal);// normalize(mul(float4(v.normal, 0.0), unity_ObjectToWorld).xyz);


					//MINE	
					float3 lightDir = worldPos.xyz - _WorldSpaceLightPos0.xyz;
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

				float4 frag(v2f IN) : SV_Target
				{
					
					float3 dcolor = tex2D(_DispTex, IN.terrainData + float2(_Time.y, _Time.y)*0.1*_GlowSpeed);// uv_DispTex);
					float d = (dcolor.r*_ChannelFactor.r + dcolor.g*_ChannelFactor.g + dcolor.b*_ChannelFactor.b) * (_Range.y - _Range.x) + _Range.x;
					clip(_ClipRange - d);
					half4 c = tex2D(_RampTex, float2(d, 0.5) + float2(_Time.y, _Time.y)*0.1*_GlowSpeed);
					//	o.Albedo = compositeCol;

					// Set metallic and smoothness
					//	o.Metallic = lerp(_MetallicA, _MetallicB, biomeWeight);;
					//float smoothness = lerp(_SmoothnessA, _SmoothnessB, biomeWeight);
					//smoothness = lerp(smoothness, _SmoothnessEjecta, saturate(ejecta * 1.5) * craterAlpha);
					//	o.Smoothness = smoothness;

					//fixed4 col = tex2D(_MainTex, IN.terrainData);
					//col *= i.diff;

					//MINE
					float3 lightDir = normalize(IN.lightdir);
					float3 diffuse = saturate(dot(IN.normal, -lightDir));
					diffuse = 1 * c.rgb * diffuse;

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

					return float4(pow(diffuse,3)*4 + 0*pow(specular-0.33,3) , 1)*5;
				}


				/*void surf(Input IN, inout SurfaceOutput o)
				{
					float3 dcolor = tex2D(_DispTex, IN.uv_DispTex);
					float d = (dcolor.r*_ChannelFactor.r + dcolor.g*_ChannelFactor.g + dcolor.b*_ChannelFactor.b) * (_Range.y - _Range.x) + _Range.x;
					clip(_ClipRange - d);
					half4 c = tex2D(_RampTex, float2(d,0.5));
					o.Albedo = c.rgb;
					o.Emission = c.rgb*c.a;
				}*/
				ENDCG
					}
			}
				//FallBack "Diffuse"
	}