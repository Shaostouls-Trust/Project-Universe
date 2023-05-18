Shader "ORION/Scattering/Atmospheric Scattering Only Atmos HDRP LAND ONLY"
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
		_SphereRadius("Sphere Radius", Range(0.1,25)) = 6.371

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

	}
		SubShader{

			Tags { "RenderType" = "Opaque" "Queue" = "Geometry"}
			//LOD 200
			//ZWrite LEqual
			Cull Off
			ZWrite On
		Pass {
	CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag			
			#include "UnityCG.cginc"				
			#include "Lighting.cginc"
			#include "AutoLight.cginc"
			#pragma target 3.0
		#pragma multi_compile_fwdbase  multi_compile_shadowcaster

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

			//GLOBAL
			float4 globalSunPosition;
			float4 globalSunColor;
			float _passSunExternally;

			struct v2f
			{
				float4 pos: SV_POSITION;
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
				LIGHTING_COORDS(9, 10)
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
				o.pos = UnityObjectToClipPos(v.vertex);
				o.centre = mul(unity_ObjectToWorld, half4(0, 0, 0, 1));
				o.pos = UnityObjectToClipPos(v.vertex);
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

				TRANSFER_VERTEX_TO_FRAGMENT(o);
				return o;
			}

		float4 frag(v2f IN) : SV_Target{
			float4 cAlbedo = tex2D(_MainTex, IN.uv_MainTex)* _Color; ;

			float4 clouds = tex2D(_CloudsTex, IN.uv_MainTex + fixed2(_Time.y * _CloudsSpeed, 0));
			if (_CloudsAdditive == 1)
			{
				cAlbedo.rgb = saturate(clouds.rgb * _CloudsAlpha + cAlbedo.rgb);
			}
			else
			{
				cAlbedo.rgb = lerp(cAlbedo.rgb, clouds.rgb, _CloudsAlpha * clouds.a);
			}
			float3 Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));

			float4 Smoothness = tex2D(_GlossyMap, IN.uv_MainTex) * _Glossiness * (1 - ((clouds).r*0.3 + (clouds).g*0.5901 + (clouds).b*.1102));

			if (_passSunExternally == 1) {
				_WorldSpaceLightPos0.xyz = -globalSunPosition.xyz;
			}

			float3 N = IN.worldNormal + 0;// WorldNormalVector(IN, Normal);
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

			return float4(diffuse * (1 - emissionStrength) * 1 + Emission + specular * 0.5*_Specular + 0.28*Smoothness, 1)*_TerrainPower;

			  }
				ENDCG
			}
			//UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
			 // shadow caster rendering pass, implemented manually
		// using macros from UnityCG.cginc
			Pass
			{
				Tags {"LightMode" = "ShadowCaster"}

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_shadowcaster
				#include "UnityCG.cginc"

				struct v2f {
					V2F_SHADOW_CASTER;
				};

				v2f vert(appdata_base v)
				{
					v2f o;
					TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
					return o;
				}

				float4 frag(v2f i) : SV_Target
				{
					SHADOW_CASTER_FRAGMENT(i)
				}
				ENDCG
			}
			
		}
		Fallback "Diffuse"
}