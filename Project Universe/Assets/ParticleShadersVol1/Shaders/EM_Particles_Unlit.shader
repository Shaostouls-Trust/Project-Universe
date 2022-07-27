// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Ethical Motion/Particles/Unlit" {
Properties {
	[HDR]_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
	_MainTex ("Particle Texture", 2D) = "white" {}
	_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
	_FadeStart ("Distance fade start", float) = 2
	_FadeEnd ("Distance fade end", float) = 10
	
	[HideInInspector] _AlphaMode ("_AlphaMode", float) = 0.0
	[HideInInspector] _LightingMode("_LightingMode", float) = 2.0
	[HideInInspector] _BlendMode ("_BlendMode", float) = 0.0
	[HideInInspector] _BlendSrc ("Source blend mode", float) = 1.0
	[HideInInspector] _BlendDst ("Destination blend mode", float) = 1.0
}

Category {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
	Blend [_BlendSrc] [_BlendDst]
	Cull Back Lighting Off ZWrite Off

	SubShader {
		Pass {
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0

			#pragma multi_compile_particles
			#pragma multi_compile_fog

			#pragma shader_feature SOFTPARTICLE_ON __
			#pragma shader_feature DISTANCEFADE_ON __
			#pragma shader_feature ALPHAEROSION_ON __
			#pragma shader_feature ADDITIVE_ON __
			
			#include "EMParticleFunctions.cginc"
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _TintColor;
			
			#ifdef SOFTPARTICLE_ON
				sampler2D_float _CameraDepthTexture;
				float _InvFade;
			#endif
			
			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;

				float4 texcoordBlendFrame : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;

				float2 texcoord2 : TEXCOORD1;
				fixed blend : TEXCOORD2;
				UNITY_FOG_COORDS(3)
				
				#ifdef SOFTPARTICLE_ON
					float4 projPos : TEXCOORD4;
				#endif
				
				#ifdef DISTANCEFADE_ON
					fixed distanceFade : TEXCOORD5;
				#endif

				UNITY_VERTEX_OUTPUT_STEREO
			};
			
			v2f vert (appdata_t v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); 

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color;

				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				o.texcoord2 = TRANSFORM_TEX(v.texcoordBlendFrame.xy,_MainTex);
				o.blend = v.texcoordBlendFrame.z;
				UNITY_TRANSFER_FOG(o,o.vertex);
				
				#ifdef SOFTPARTICLE_ON
					o.projPos = ComputeScreenPos (o.vertex);
					COMPUTE_EYEDEPTH(o.projPos.z);
				#endif
				
				#ifdef DISTANCEFADE_ON
					//_FadeEnd and _FadeStart are defined in EMParticleVariables.cginc
					o.distanceFade = DistanceFade(_FadeEnd, _FadeStart, mul(unity_ObjectToWorld, v.vertex).xyz);
				#endif
				
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				//float4 color = tex2D(_MainTex, i.texcoord);
				//color.rgb *= i.color.rgb * _TintColor.rgb;

				fixed4 colA = tex2D(_MainTex, i.texcoord);
				fixed4 colB = tex2D(_MainTex, i.texcoord2);
				fixed4 color = lerp(colA, colB, i.blend);
				color.rgb *= i.color.rgb * _TintColor.rgb;

				#ifdef ADDITIVE_ON
					#ifdef ALPHAEROSION_ON
						color.a = saturate(color.a - (1 - i.color.a));
					#else
						color.a *= i.color.a;
					#endif
					
					color.rgb *= color.a;
					color.rgb *= _TintColor.a;
				#else
					#ifdef ALPHAEROSION_ON
						color.a -= saturate(1 - i.color.a);
						color.a *= _TintColor.a;
					#else
						color.a *= i.color.a;
						color.a *= _TintColor.a;
					#endif

				#endif

				#ifdef SOFTPARTICLE_ON
					float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
					float partZ = i.projPos.z;
					float fade = saturate (_InvFade * (sceneZ-partZ));
					#ifdef ADDITIVE_ON
						color.rgb *= fade;
					#else
						color.a *= fade;
					#endif
				#endif

				#ifdef DISTANCEFADE_ON
					#ifdef ADDITIVE_ON
						color.rgb *= i.distanceFade;
					#else
						color.a *= i.distanceFade;
					#endif
				#endif

				UNITY_APPLY_FOG(i.fogCoord, color);

				return color;
			}
			ENDCG 
		}
		
	}	
	CustomEditor "EMMaterialInspector"
}
}
