// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Ethical Motion/Particles/Lit MultiLight" {
	Properties {
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_HDRMultiplier ("Emissive HDR multiplier", float) = 1.0
		_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0

		_Thickness ("Thickness Factor", Range(0.01, 1)) = 0.05
		_AlphaInfluence ("Alpha channel influence", Range(0, 2)) = 0.5
		_AlphaContrast ("Alpha channel contrast", Range(0,1)) = 0.5

		_Cutoff ("Alpha cutoff", Range(0,1) ) = 0.5
		_FadeStart ("Distance fade start", float) = 2.0
		_FadeEnd ("Distance fade end", float) = 10.0
		
		[HideInInspector] _AlphaMode ("_AlphaMode", float) = 0.0
		[HideInInspector] _LightingMode("_LightingMode", float) = 0.0
		[HideInInspector] _LightCount("_LightCount", float) = 0.0
		[HideInInspector] _BlendMode ("_BlendMode", float) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Transparent" "IgnoreProjector"="True" "Queue"="Transparent" }
		Cull Back
		Zwrite Off
		
		CGPROGRAM
		#pragma surface surf Smoke vertex:vert addshadow alpha:fade nodynlightmap nodirlightmap 
		#pragma target 3.0
		#include "EMParticleLighting.cginc"
		#pragma shader_feature SOFTPARTICLE_ON __
		#pragma shader_feature DISTANCEFADE_ON __
		#pragma shader_feature ALPHAEROSION_ON __
		#pragma shader_feature EMISSION_ON __	
		#pragma shader_feature ALPHATRANSMITANCE_ON __

		sampler2D _MainTex;
		fixed4 _TintColor;
		
		#ifdef SOFTPARTICLE_ON
			sampler2D _CameraDepthTexture;
			fixed _InvFade;
		#endif

		#ifdef EMISSION_ON
			float _HDRMultiplier;
		#endif
			
		struct Input {
			float4 vertex : SV_POSITION;
			float2 uv_MainTex;
			float4 color : COLOR;
			
			#ifdef SOFTPARTICLE_ON
				float4 projPos : TEXCOORD0;
			#endif
			
			#ifdef DISTANCEFADE_ON
				fixed distanceFade : TEXCOORD1;
			#endif

			#ifdef EMISSION_ON
				float _HDRMultiplier;
			#endif
		};
		
		void vert(inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.vertex = UnityObjectToClipPos(v.vertex);
			
			#ifdef SOFTPARTICLE_ON
				o.projPos = ComputeScreenPos(o.vertex);
				COMPUTE_EYEDEPTH(o.projPos.z);
			#endif
			
			#ifdef DISTANCEFADE_ON
				//_FadeEnd and _FadeStart are defined in EMParticleVariables.cginc
				o.distanceFade = DistanceFade(_FadeEnd, _FadeStart, mul(unity_ObjectToWorld, v.vertex).xyz);
			#endif
		}

		void surf (Input i, inout SurfaceOutputSmoke o) {
			fixed4 color = tex2D(_MainTex, i.uv_MainTex);

			#ifdef ALPHATRANSMITANCE_ON
				o.AlphaMap = color.a * i.color.a * _TintColor.a;
			#endif

			color.rgb *= _TintColor.rgb;
			
			#ifdef ALPHAEROSION_ON
				color.a = saturate(color.a - (1 - i.color.a));
				#else
					color.a *= i.color.a;
			#endif
			
			color.a *= _TintColor.a;

			if (color.a < 0.003) discard;

			#ifdef SOFTPARTICLE_ON
				float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
				float partZ = i.projPos.z;
				float fade = saturate (_InvFade * (sceneZ-partZ));
				color.a *= fade;
			#endif
			
			#ifdef DISTANCEFADE_ON
				color.a *= i.distanceFade;
			#endif
			
			#ifdef EMISSION_ON
				o.Emission = i.color.rgb * _HDRMultiplier;
				#else
					color.rgb *= i.color.rgb;
			#endif
			o.Albedo = color.rgb;
			o.Alpha = color.a;
		}
		ENDCG
	} 
	Fallback "Hidden/Ethical Motion/Particles/Lit Alpha Blend Shadow Fallback"
	CustomEditor "EMMaterialInspector"
}
