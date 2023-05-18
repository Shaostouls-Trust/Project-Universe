Shader "ORION/BumpedFluidPlanetShader" {
	Properties{

		_ColorA("Color", Color) = (1,1,1,1)
		_ColorB("Color", Color) = (1,1,1,1)
		_ColorC("Color", Color) = (1,1,1,1)
		_ColorD("Color", Color) = (1,1,1,1)

		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Velo("_Velo", 2D) = "black" {}

		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0

		_passSunExternally("Give sun pos-color with globalSunPosition-SunColor", float) = 0

		//BUMP		
		_GlossyMap("Glossy Map", 2D) = "white" {}
		_BumpMap("Bump Map", 2D) = "bump" {}
		_BumpMap2("Bump Map 2", 2D) = "bump" {}
		bumpIntensity("bumpIntensity", Vector) = (1,4,1,1)
			bumpPower("bumpPower", float) =1
	}

		// Note 
		// This shader turns the densitity & velocity information into
		// an visual Texture which then is applied to the planet

		SubShader{

		Pass
		{
			Tags { "RenderType" = "Opaque" }
			LOD 200

			CGPROGRAM
			// Upgrade NOTE: excluded shader from DX11 because it uses wrong array syntax (type[size] name)
			//#pragma exclude_renderers d3d11

			//#pragma surface surf Standard fullforwardshadows
			//#pragma target 3.0

			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

		//BUMP
		sampler2D _BumpMap;
		sampler2D _BumpMap2;
		sampler2D _GlossyMap;	
		uniform fixed4 bumpIntensity;
		uniform float bumpPower;

		sampler2D _MainTex;

		//GLOBAL
		float4 globalSunPosition;
		float4 globalSunColor;
		float _passSunExternally;
		
		struct appdata
		{
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
			float3 normal : NORMAL;
		};

		struct v2f
		{
			float4 vertex : SV_POSITION;
			float2 uv : TEXCOORD0;
			//float4 screenPos : TEXCOORD1;
			//float4 grabPos : TEXCOORD2;
			float3 normal : NORMAL;
			//float3 viewDir : TEXCOORD3;
			float3 diff : COLOR0;
			SHADOW_COORDS(1)
		};

		half _Glossiness;
		half _Metallic;

		float4 _ColorA;
		float4 _ColorB;
		float4 _ColorC;
		float4 _ColorD;
		float4 _ColorD_1;

		sampler2D _Velo;

		v2f vert(appdata v)
		{
			v2f o;

			//float4 offset = tex2Dlod(_NoiseTex, float4(v.uv - _Time.xy * _DistortTimeFactor, 0, 0));
			//v.vertex.xy -= 2.01*offset.xy * _DistortStrength;
			//v.vertex.x -= 2.01*offset.x * _DistortStrength;

			o.vertex = UnityObjectToClipPos(v.vertex);

			//o.grabPos = ComputeGrabScreenPos(o.vertex);

			o.uv = v.uv;// TRANSFORM_TEX(v.uv, _NoiseTex);

			//o.screenPos = ComputeScreenPos(o.vertex);

			//COMPUTE_EYEDEPTH(o.screenPos.z);

			o.normal = UnityObjectToWorldNormal(v.normal);

			//o.viewDir = normalize(UnityWorldSpaceViewDir(mul(unity_ObjectToWorld, v.vertex)));

			if (_passSunExternally == 1) {
				_WorldSpaceLightPos0.xyz = -globalSunPosition.xyz;
				_LightColor0.rgb = globalSunColor.xyz*globalSunColor.w;
			}

			half3 worldNormal = UnityObjectToWorldNormal(v.normal);
			half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
			o.diff = nl * _LightColor0.rgb;
			TRANSFER_SHADOW(o)
			return o;
		}

		/*void surf (Input IN, inout SurfaceOutputStandard o) {

		   float4 c = tex2D (_MainTex, IN.uv_MainTex);
		   float4 cf = c/ (c+1);

		   float4 v = tex2D (_Velo, IN.uv_MainTex);
		   float fakelight=pow(max(0,dot(v.xy, normalize(float2(-1,-1)))+0.22),2)*0.5+0.5;

		   o.Albedo  =lerp(lerp(lerp(_ColorA,_ColorB, cf.r),_ColorC, cf.g), _ColorD, cf.b).rgb;
		   o.Albedo *=(length(v.xy)*0.5+0.5)*2*fakelight;

		   o.Metallic = _Metallic;
		   o.Smoothness = _Glossiness;
		   o.Alpha = 1;

		}*/
		float4 frag(v2f i) : SV_Target
		{
			if (_passSunExternally == 1) {
				_WorldSpaceLightPos0.xyz = -globalSunPosition.xyz;
				_LightColor0.rgb = globalSunColor.xyz*globalSunColor.w;
			}


			//BUMP
			float3 Normal = UnpackNormal(tex2D(_BumpMap, i.uv))*1;
			float3 Normal2 = UnpackNormal(tex2D(_BumpMap2, i.uv))*1;
			float3 N = bumpIntensity.y*(Normal - 0.55*Normal2) + i.normal ;// IN.worldNormal + 0;// WorldNormalVector(IN, Normal);
			float3 L = bumpIntensity.w*_WorldSpaceLightPos0.xyz;
			float NdotL = dot(N, L);
			//float emissionStrength = saturate(-NdotL);
			

		   float4 c = tex2D(_MainTex, i.uv);
		   float4 cf = c / (c + 1);

		   float4 v = tex2D(_Velo, i.uv);
		   float fakelight = pow(max(0,dot(v.xy, normalize(float2(-1,-1))) + 0.22),2)*0.5 + 0.5;

		   float3 Albedo = lerp(lerp(lerp(_ColorA,_ColorB, cf.r),_ColorC, cf.g), _ColorD, cf.b).rgb;
		   Albedo *= (length(v.xy)*0.5 + 0.5) * 2 * fakelight;

		   float shadow = SHADOW_ATTENUATION(i);
		   // darken light's illumination with shadow, keep ambient intact
		   float3 lighting = i.diff * shadow * bumpIntensity.z - bumpIntensity.x*pow(NdotL, 2)*float3(1,1,1);// +i.ambient;
		  
		   if (_LightColor0.r > 0) {
			   Albedo.rgb *= lighting;
			   return float4(Albedo* _LightColor0.rgb*1.5, 1);
		   }
		   else {
			   return float4(Albedo * 6 * shadow, 1);
		   }
		}

		ENDCG
			}//END PASS 1
		}
			//FallBack "Diffuse"
}
