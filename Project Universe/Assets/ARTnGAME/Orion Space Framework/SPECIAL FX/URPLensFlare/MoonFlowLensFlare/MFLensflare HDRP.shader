Shader"ORION/Lensflare HDRP"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
	// Depth prepass
	   [HideInInspector] _StencilRefDepth("_StencilRefDepth", Int) = 0 // Nothing
	   [HideInInspector] _StencilWriteMaskDepth("_StencilWriteMaskDepth", Int) = 8 // StencilUsage.TraceReflectionRay
			[HideInInspector] _ZTestDepthEqualForOpaque("_ZTestDepthEqualForOpaque", Int) = 4 // Less equal
    }


    SubShader
    {
        
        //Tags
        //{
        //    "RenderType" = "Transparent" 
        //   // "IgnoreProjector" = "True" 
        //  //  "PreviewType" = "Plane" 
        //  //  "PerformanceChecks" = "False" 
        //  //  "RenderPipeline" = "UniversalPipeline"
        //}
		 Tags{ "RenderPipeline" = "HDRenderPipeline" "RenderType" = "HDUnlitShader"  "RenderType" = "Transparent" }
        //LOD 100
		Pass
		{
			Name "DepthForwardOnly"
			Tags{ "LightMode" = "DepthForwardOnly" }

			Stencil
			{
				WriteMask[_StencilWriteMaskDepth]
				Ref[_StencilRefDepth]
				Comp Always
				Pass Replace
			}

			Cull[_CullMode]

			ZWrite On

		// Caution: When using MSAA we have normal and depth buffer bind.
		// Mean unlit object need to not write in it (or write 0) - Disable color mask for this RT
		// This is not a problem in no MSAA mode as there is no buffer bind
		ColorMask 0 0

		HLSLPROGRAM

		#pragma only_renderers d3d11 ps4 xboxone vulkan metal switch

		//enable GPU instancing support
		#pragma multi_compile_instancing

		#pragma multi_compile _ WRITE_MSAA_DEPTH
		// Note we don't need to define WRITE_NORMAL_BUFFER

#pragma shader_feature_local _ALPHATEST_ON
// #pragma shader_feature_local _DOUBLESIDED_ON - We have no lighting, so no need to have this combination for shader, the option will just disable backface culling

#pragma shader_feature_local _EMISSIVE_COLOR_MAP

// Keyword for transparent
#pragma shader_feature _SURFACE_TYPE_TRANSPARENT
#pragma shader_feature_local _ _BLENDMODE_ALPHA _BLENDMODE_ADD _BLENDMODE_PRE_MULTIPLY
#pragma shader_feature_local _ENABLE_FOG_ON_TRANSPARENT

#pragma shader_feature_local _ADD_PRECOMPUTED_VELOCITY

		#define SHADERPASS SHADERPASS_DEPTH_ONLY
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Unlit/UnlitProperties.hlsl"

		#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
		#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Unlit/Unlit.hlsl"
		#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Unlit/ShaderPass/UnlitDepthPass.hlsl"
		#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Unlit/UnlitData.hlsl"
		#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPassDepthOnly.hlsl"

		#pragma vertex Vert
		#pragma fragment Frag

		ENDHLSL
	}
        Pass
        {
		 Tags { "LightMode" = "ForwardOnly" }
			//Tags { "LightMode" = "FirstPass" }
           // Blend One One
            ZWrite Off
            ZTest Off

		  // ZTest[_ZTestDepthEqualForOpaque]

			//ZTest LEqual
			//Blend SrcAlpha OneMinusSrcAlpha // Traditional transparency
			//Blend One OneMinusSrcAlpha // Premultiplied transparency
			Blend One One // Additive
			//Blend OneMinusDstColor One // Soft additive
			//Blend DstColor Zero // Multiplicative
			//Blend DstColor SrcColor // 2x multiplicative
			

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
           // #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/core.hlsl"
			////HDRP
			#pragma shader_feature_local _ALPHATEST_ON
			#pragma shader_feature_local _EMISSIVE_COLOR_MAP
			#pragma shader_feature _SURFACE_TYPE_TRANSPARENT
			#pragma shader_feature_local _ _BLENDMODE_ALPHA _BLENDMODE_ADD _BLENDMODE_PRE_MULTIPLY
			#pragma shader_feature_local _ENABLE_FOG_ON_TRANSPARENT
			  #define SHADERPASS SHADERPASS_FORWARD_UNLIT
			#pragma shader_feature_local _ADD_PRECOMPUTED_VELOCITY
			//#define _SURFACE_TYPE_TRANSPARENT
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"			
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/API/D3D11.hlsl"			
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/TextureXR.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/ScreenSpaceLighting/ShaderVariablesScreenSpaceLighting.hlsl"

		//HDRP
		TEXTURE2D(_CameraOpaqueTexture);
			SAMPLER(sampler_CameraOpaqueTexture);
			float3 SampleSceneColor(float2 uv)
			{
				return SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, UnityStereoTransformScreenSpaceTex(uv)).rgb;
			}
			//	float4 _DepthPyramidScale;
			UNITY_DECLARE_TEX2DARRAY(_ColorPyramidTexture);
			//float4 _ColorPyramidScale;
			SAMPLER(s_trilinear_clamp_sampler);
			float4 _RTHandleScaleHistory;
			void ComputeScreenAndGrabPassPos(float4 pos, out float4 screenPos, out float4 grabPassPos)
			{
				#if UNITY_UV_STARTS_AT_TOP
							float scale = -1.0;
				#else
							float scale = 1.0f;
				#endif
				screenPos = ComputeScreenPos(pos);
				grabPassPos.xy = (float2(pos.x, pos.y*scale) + pos.w) * 0.5;
				grabPassPos.zw = pos.zw;
			}
			float _refractA;
			///END HDRP

			uniform sampler2D_float _CameraDepthTexture;
			float4 _CameraDepthTexture_TexelSize;
			float2 AlignWithGrabTexel(float2 uv) {
#if UNITY_UV_STARTS_AT_TOP
				if (_CameraDepthTexture_TexelSize.y < 0) {
					uv.y = 1 - uv.y;
				}
#endif
				return (floor(uv * _CameraDepthTexture_TexelSize.zw) + 0.5) * abs(_CameraDepthTexture_TexelSize.xy);
			}
			//v0.3
			float _Distortion;
			uniform sampler2D _GrabTexture;
			uniform half4 _GrabTexture_TexelSize;







            struct appdata
            {
               float4 vertex : POSITION;
               float2 uv : TEXCOORD0;
               half4 color : COLOR;
            };

            struct v2f
            {
               float4 vertex : SV_POSITION;
               float2 uv : TEXCOORD0;
               half4 color : TEXCOORD1;

			   //v0.3
			   float4 grabUV : TEXCOORD2;
			   //v0.5
			   float4 screenPos: TEXCOORD3;
            };

			Texture2D _MainTex;
            SamplerState sampler_MainTex;
            float4 _MainTex_ST;

			//UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
           // sampler2D _CameraDepthTexture;//Texture2D _CameraDepthTexture;
            SamplerState sampler_CameraDepthTexture;

            half4 _FlareScreenPos;

			//URP
			float4 TransformObjectToHClip(float3 positionOS)
			{
				//float3 worldPos = mul(unity_ObjectToWorld, positionOS);
				// More efficient than computing M*VP matrix product
				return mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, float4(positionOS, 1.0)));
			}

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);// TransformObjectToHClip(v.vertex);

				//HDRP
				ComputeScreenAndGrabPassPos(o.vertex, o.screenPos, o.grabUV);

                o.uv.xy = v.uv;
                o.color = v.color;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
				//v0.3 - refract
				float3 normal = float3(1,1,1);
				half2 offset = normal.xz  * _GrabTexture_TexelSize.xy * _Distortion * 1000;// worldNormal.xz * _GrabTexture_TexelSize.xy * _Distortion;
				half4 grabUV = half4(offset * i.grabUV.z + i.grabUV.xy, i.grabUV.zw);
				half4 screenUV = half4(offset * i.screenPos.z + i.screenPos.xy, i.screenPos.zw);
				float sceneDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos)));
				float waterDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(grabUV)));// screenUV)));
				float2 samplingPositionNDC2 = float4(i.grabUV.xy / i.grabUV.w, 0, 0).xy;
				float4 cleanRefraction = float4(SAMPLE_TEXTURE2D_LOD(_ColorPyramidTexture, s_trilinear_clamp_sampler, //URP v0.1
				float4(samplingPositionNDC2.x, clamp(samplingPositionNDC2.y, 0, 1), 0, 0) * 1 + float4(0, 0, 0, 0), 0).rgb, 1);


				float depthMask = (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(_FlareScreenPos))).r;
                //half depthMask = tex2D(_CameraDepthTexture, _FlareScreenPos.xy ).r;//SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, _FlareScreenPos.xy).r;
				half depthTex = LinearEyeDepth(depthMask);// , _ZBufferParams);
                half needRender = lerp(saturate(depthTex - _FlareScreenPos.z), 1 - ceil(depthMask), _FlareScreenPos.w);
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex,i.uv.xy);// SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv.xy) * needRender * i.color;
                
				return float4(col) *1 * i.color;
				return float4(1,0,0,1)*2;
				//return col * needRender * i.color;
            }
            ENDHLSL
        }
    }
}
