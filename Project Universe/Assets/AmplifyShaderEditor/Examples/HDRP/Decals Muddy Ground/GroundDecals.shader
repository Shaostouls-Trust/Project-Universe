// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ASESampleShaders/Decals Muddy Ground/GroundDecals"
{
	Properties
	{
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		[ASEBegin]_dirt_decal_BaseColor("dirt_decal_BaseColor", 2D) = "white" {}
		_dirt_decal_mask("dirt_decal_mask", 2D) = "white" {}
		_Dirt_Decal_Normal("Dirt_Decal_Normal", 2D) = "bump" {}
		_SmoothnessMultiplier("Smoothness Multiplier", Range( 0 , 1)) = 0
		_NormalIntensity("Normal Intensity", Float) = 0
		_DecalQuantity("Decal Quantity", Float) = 0
		[ASEEnd]_DecalType("Decal Type", Float) = 0

		[HideInInspector]_DrawOrder("Draw Order", Int) = 0
		[HideInInspector]_DecalMeshDepthBias("DecalMesh DepthBias", Float) = 0
	}

	SubShader
	{
		LOD 0

		
		Tags { "RenderPipeline"="HDRenderPipeline" "RenderType"="Opaque" "Queue"="Geometry" }

		HLSLINCLUDE
		#pragma target 4.5
		#pragma only_renderers d3d11 ps4 xboxone vulkan metal switch

		struct SurfaceDescription
		{
			float3 Albedo;
			float AlphaAlbedo;
			float3 Normal;
			float AlphaNormal;
			float Metallic;
			float Occlusion;
			float Smoothness;
			float MAOSOpacity;
			float3 Emission;
		};
		ENDHLSL

		
		Pass
		{
			
			Name "ShaderGraph_DBufferProjector3RT"
            Tags { "LightMode"="ShaderGraph_DBufferProjector3RT" }
        
            Blend 0 SrcAlpha OneMinusSrcAlpha, Zero OneMinusSrcAlpha Blend 1 SrcAlpha OneMinusSrcAlpha, Zero OneMinusSrcAlpha Blend 2 SrcAlpha OneMinusSrcAlpha, Zero OneMinusSrcAlpha
            Cull Front
            ZTest Greater
            ZWrite Off

			Stencil
			{
				Ref 16
				WriteMask 16
				Comp Always
				Pass Replace
				Fail Keep
				ZFail Keep
			}

        
            ColorMask BA 2 ColorMask 0 3

			HLSLPROGRAM
			#define _MATERIAL_AFFECTS_ALBEDO 1
			#define _MATERIAL_AFFECTS_NORMAL 1
			#define _MATERIAL_AFFECTS_METAL 1
			#define _MATERIAL_AFFECTS_AO 1
			#define _MATERIAL_AFFECTS_SMOOTHNESS 1
			#define _MATERIAL_AFFECTS_EMISSION 1
			#define ASE_SRP_VERSION 70301

			#pragma multi_compile_instancing
			
			#if defined(_MATERIAL_AFFECTS_METAL) || defined(_MATERIAL_AFFECTS_AO) || defined(_MATERIAL_AFFECTS_SMOOTHNESS)
			#define _MATERIAL_AFFECTS_MASKMAP 1
			#endif

			#define SHADERPASS SHADERPASS_DBUFFER_PROJECTOR
            #define DECALS_3RT

			#pragma vertex Vert
			#pragma fragment Frag

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"

			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
			#ifdef DEBUG_DISPLAY
				#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Debug/DebugDisplay.hlsl"
			#endif

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialUtilities.hlsl"
			#include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/Decal.hlsl"

			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0


			struct VertexInput
			{
				float3 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 tangentOS : TANGENT;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_Position;
				#if defined(ASE_NEEDS_FRAG_RELATIVE_WORLD_POS)
				float3 positionRWS : TEXCOORD0;
				#endif
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float _DecalQuantity;
			float _DecalType;
			float _NormalIntensity;
			float _SmoothnessMultiplier;
			float _DrawOrder;
			float _DecalMeshDepthBias;
			CBUFFER_END

			sampler2D _dirt_decal_BaseColor;
			sampler2D _Dirt_Decal_Normal;
			sampler2D _dirt_decal_mask;


			
			void GetSurfaceData(SurfaceDescription surfaceDescription, FragInputs fragInputs, float3 V, PositionInputs posInput, out DecalSurfaceData surfaceData)
			{
				#if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR)
					float4x4 normalToWorld = UNITY_ACCESS_INSTANCED_PROP(Decal, _NormalToWorld);
					float fadeFactor = clamp(normalToWorld[0][3], 0.0f, 1.0f);
					float2 scale = float2(normalToWorld[3][0], normalToWorld[3][1]);
					float2 offset = float2(normalToWorld[3][2], normalToWorld[3][3]);
					fragInputs.texCoord0.xy = fragInputs.texCoord0.xy * scale + offset;
					fragInputs.texCoord1.xy = fragInputs.texCoord1.xy * scale + offset;
					fragInputs.texCoord2.xy = fragInputs.texCoord2.xy * scale + offset;
					fragInputs.texCoord3.xy = fragInputs.texCoord3.xy * scale + offset;
				#else
					float fadeFactor = 1.0;
				#endif

				ZERO_INITIALIZE(DecalSurfaceData, surfaceData);

				#if _MATERIAL_AFFECTS_EMISSION
					//surfaceData.emissive.rgb = surfaceDescription.Emission.rgb * fadeFactor;
				#endif

				#if _MATERIAL_AFFECTS_ALBEDO
					surfaceData.baseColor.xyz = surfaceDescription.Albedo;
					surfaceData.baseColor.w = surfaceDescription.AlphaAlbedo * fadeFactor;
					if(surfaceData.baseColor.w > 0)
					{
						surfaceData.HTileMask |= DBUFFERHTILEBIT_DIFFUSE;
					}
				#endif

				#if _MATERIAL_AFFECTS_NORMAL
					#if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR)
						surfaceData.normalWS.xyz = mul((float3x3)normalToWorld, surfaceDescription.Normal);
					#elif (SHADERPASS == SHADERPASS_DBUFFER_MESH) || (SHADERPASS == SHADERPASS_FORWARD_PREVIEW)
						surfaceData.normalWS.xyz = normalize(TransformTangentToWorld(surfaceDescription.Normal, fragInputs.tangentToWorld));
					#endif
						surfaceData.normalWS.w = surfaceDescription.AlphaNormal * fadeFactor;
						if(surfaceData.normalWS.w > 0)
						{
							surfaceData.HTileMask |= DBUFFERHTILEBIT_NORMAL;
						}
				#else
					#if (SHADERPASS == SHADERPASS_FORWARD_PREVIEW)
						surfaceData.normalWS.xyz = normalize(TransformTangentToWorld(float3(0.0, 0.0, 0.1), fragInputs.tangentToWorld));
					#endif
				#endif

				#if _MATERIAL_AFFECTS_MASKMAP
					surfaceData.mask.z = surfaceDescription.Smoothness;
					#ifdef DECALS_4RT
						surfaceData.mask.x = surfaceDescription.Metallic;
						surfaceData.mask.y = surfaceDescription.Occlusion;
					#endif

						surfaceData.mask.w = surfaceDescription.MAOSOpacity * fadeFactor;
					#ifdef DECALS_4RT
						surfaceData.MAOSBlend.x = surfaceDescription.MAOSOpacity * fadeFactor;
						surfaceData.MAOSBlend.y = surfaceDescription.MAOSOpacity * fadeFactor;
					#endif

					if (surfaceData.mask.w > 0)
					{
						surfaceData.HTileMask |= DBUFFERHTILEBIT_MASK;
					}
				#endif
			}

			VertexOutput Vert( VertexInput inputMesh  )
			{
				VertexOutput o;
				UNITY_SETUP_INSTANCE_ID(inputMesh);
				UNITY_TRANSFER_INSTANCE_ID(inputMesh, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				float3 defaultVertexValue = inputMesh.positionOS.xyz;
				#else
				float3 defaultVertexValue = float3( 0, 0, 0 );
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				inputMesh.positionOS.xyz = vertexValue;
				#else
				inputMesh.positionOS.xyz += vertexValue;
				#endif

				inputMesh.normalOS = inputMesh.normalOS;
				inputMesh.tangentOS = inputMesh.tangentOS;

				float3 positionRWS = TransformObjectToWorld(inputMesh.positionOS);
				float3 normalWS = TransformObjectToWorldNormal(inputMesh.normalOS);
				float4 tangentWS = float4(TransformObjectToWorldDir(inputMesh.tangentOS.xyz), inputMesh.tangentOS.w);

				o.positionCS = TransformWorldToHClip(positionRWS);
				#if defined(ASE_NEEDS_FRAG_RELATIVE_WORLD_POS)
				o.positionRWS = positionRWS;
				#endif
				//o.normalWS.xyz = normalWS;
				//o.tangentWS.xyzw = tangentWS;

				#if (SHADERPASS == SHADERPASS_DBUFFER_MESH)
					#if defined(UNITY_REVERSED_Z)
						o.positionCS.z -= _DecalMeshDepthBias;
					#else
						o.positionCS.z += _DecalMeshDepthBias;
					#endif
				#endif
				return o;
			}

			void Frag( VertexOutput packedInput,
				#if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_DBUFFER_MESH)
					OUTPUT_DBUFFER(outDBuffer)
				#else
					out float4 outEmissive : SV_Target0
				#endif
				
				)
			{
				UNITY_SETUP_INSTANCE_ID( packedInput );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( packedInput );
				FragInputs input;
				ZERO_INITIALIZE(FragInputs, input);
				input.tangentToWorld = k_identity3x3;
				#if defined(ASE_NEEDS_FRAG_RELATIVE_WORLD_POS)
				float3 positionRWS = packedInput.positionRWS;
				input.positionRWS = positionRWS;
				#endif

				input.positionSS = packedInput.positionCS;

				float clipValue = 1.0;

				#if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR)
					float depth = LoadCameraDepth(input.positionSS.xy);
					PositionInputs posInput = GetPositionInput(input.positionSS.xy, _ScreenSize.zw, depth, UNITY_MATRIX_I_VP, UNITY_MATRIX_V);

					float3 positionDS = TransformWorldToObject(posInput.positionWS);
					positionDS = positionDS * float3(1.0, -1.0, 1.0) + float3(0.5, 0.5, 0.5);
					if (!(all(positionDS.xyz > 0.0f) && all(1.0f - positionDS.xyz > 0.0f)))
					{
						clipValue = -1.0;
						#ifndef SHADER_API_METAL
						clip(clipValue);
						#endif
					}

					input.texCoord0.xy = positionDS.xz;
					input.texCoord1.xy = positionDS.xz;
					input.texCoord2.xy = positionDS.xz;
					input.texCoord3.xy = positionDS.xz;

					float3 V = GetWorldSpaceNormalizeViewDir(posInput.positionWS);
				#else
					PositionInputs posInput = GetPositionInput(input.positionSS.xy, _ScreenSize.zw, input.positionSS.z, input.positionSS.w, input.positionRWS.xyz, uint2(0, 0));

					#if defined(ASE_NEEDS_FRAG_RELATIVE_WORLD_POS)
					float3 V = GetWorldSpaceNormalizeViewDir(input.positionRWS);
					#else
					float3 V = float3(1.0, 1.0, 1.0);
					#endif
				#endif

				float4 texCoord0 = input.texCoord0;
				float4 texCoord1 = input.texCoord1;
				float4 texCoord2 = input.texCoord2;
				float4 texCoord3 = input.texCoord3;

				SurfaceDescription surfaceDescription = (SurfaceDescription)0;
				float2 texCoord25 = texCoord0.xy * float2( 1,1 ) + float2( 0,0 );
				// *** BEGIN Flipbook UV Animation vars ***
				// Total tiles of Flipbook Texture
				float fbtotaltiles27 = _DecalQuantity * _DecalQuantity;
				// Offsets for cols and rows of Flipbook Texture
				float fbcolsoffset27 = 1.0f / _DecalQuantity;
				float fbrowsoffset27 = 1.0f / _DecalQuantity;
				// Speed of animation
				float fbspeed27 = _Time[ 1 ] * 0.0;
				// UV Tiling (col and row offset)
				float2 fbtiling27 = float2(fbcolsoffset27, fbrowsoffset27);
				// UV Offset - calculate current tile linear index, and convert it to (X * coloffset, Y * rowoffset)
				// Calculate current tile linear index
				float fbcurrenttileindex27 = round( fmod( fbspeed27 + _DecalType, fbtotaltiles27) );
				fbcurrenttileindex27 += ( fbcurrenttileindex27 < 0) ? fbtotaltiles27 : 0;
				// Obtain Offset X coordinate from current tile linear index
				float fblinearindextox27 = round ( fmod ( fbcurrenttileindex27, _DecalQuantity ) );
				// Multiply Offset X by coloffset
				float fboffsetx27 = fblinearindextox27 * fbcolsoffset27;
				// Obtain Offset Y coordinate from current tile linear index
				float fblinearindextoy27 = round( fmod( ( fbcurrenttileindex27 - fblinearindextox27 ) / _DecalQuantity, _DecalQuantity ) );
				// Reverse Y to get tiles from Top to Bottom
				fblinearindextoy27 = (int)(_DecalQuantity-1) - fblinearindextoy27;
				// Multiply Offset Y by rowoffset
				float fboffsety27 = fblinearindextoy27 * fbrowsoffset27;
				// UV Offset
				float2 fboffset27 = float2(fboffsetx27, fboffsety27);
				// Flipbook UV
				half2 fbuv27 = texCoord25 * fbtiling27 + fboffset27;
				// *** END Flipbook UV Animation vars ***
				float4 tex2DNode17 = tex2D( _dirt_decal_BaseColor, fbuv27 );
				
				float3 unpack19 = UnpackNormalScale( tex2D( _Dirt_Decal_Normal, fbuv27 ), _NormalIntensity );
				unpack19.z = lerp( 1, unpack19.z, saturate(_NormalIntensity) );
				
				float4 tex2DNode18 = tex2D( _dirt_decal_mask, fbuv27 );
				
				surfaceDescription.Albedo = tex2DNode17.rgb;
				surfaceDescription.AlphaAlbedo = tex2DNode17.a;
				surfaceDescription.Normal = unpack19;
				surfaceDescription.AlphaNormal = tex2DNode17.a;
				surfaceDescription.Metallic = tex2DNode18.r;
				surfaceDescription.Occlusion = tex2DNode18.g;
				surfaceDescription.Smoothness = ( tex2DNode18.a * _SmoothnessMultiplier );
				surfaceDescription.MAOSOpacity = tex2DNode17.a;
				surfaceDescription.Emission = float3( 0, 0, 0 );

				DecalSurfaceData surfaceData;
				GetSurfaceData(surfaceDescription, input, V, posInput, surfaceData);


				#if ((SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_DBUFFER_MESH)) && defined(PLATFORM_SUPPORTS_BUFFER_ATOMICS_IN_PIXEL_SHADER)
					uint2 htileCoord = input.positionSS.xy / 8;
					int stride = (_ScreenSize.x + 7) / 8;
					uint mask = surfaceData.HTileMask;
					uint tileCoord1d = htileCoord.y * stride + htileCoord.x;
					#ifdef PLATFORM_SUPPORTS_WAVE_INTRINSICS
						uint minTileCoord1d = WaveActiveMin(tileCoord1d);
						while (minTileCoord1d != -1)
						{
							if ((minTileCoord1d == tileCoord1d) && (clipValue > 0.0))
							{
								mask = WaveActiveBitOr(surfaceData.HTileMask);

								if(WaveIsFirstLane())
								{
									if (tileCoord1d != -1)
									{
										tileCoord1d = htileCoord.y * stride + htileCoord.x;
									}
									InterlockedOr(_DecalPropertyMaskBuffer[tileCoord1d], mask);
								}
								tileCoord1d = -1;
							}
							if (tileCoord1d != -1)
							{
								tileCoord1d = htileCoord.y * stride + htileCoord.x;
							}
							minTileCoord1d = WaveActiveMin(tileCoord1d);
						}
					#else
						InterlockedOr(_DecalPropertyMaskBuffer[tileCoord1d], mask);
					#endif
				#endif

				#if ((SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR)) && defined(SHADER_API_METAL)
					clip(clipValue);
				#endif

				#if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_DBUFFER_MESH)
					ENCODE_INTO_DBUFFER(surfaceData, outDBuffer);
				#else
					outEmissive.rgb = surfaceData.emissive * GetCurrentExposureMultiplier();
					outEmissive.a = 1.0;
				#endif
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "ShaderGraph_DBufferProjector4RT"
            Tags { "LightMode"="ShaderGraph_DBufferProjector4RT" }
        
            Blend 0 SrcAlpha OneMinusSrcAlpha, Zero OneMinusSrcAlpha Blend 1 SrcAlpha OneMinusSrcAlpha, Zero OneMinusSrcAlpha Blend 2 SrcAlpha OneMinusSrcAlpha, Zero OneMinusSrcAlpha Blend 3 Zero OneMinusSrcColor
            Cull Front
            ZTest Greater
            ZWrite Off
            
			Stencil
			{
				Ref 16
				WriteMask 16
				Comp Always
				Pass Replace
				Fail Keep
				ZFail Keep
			}

        
            ColorMask RGBA 2 ColorMask RG 3

			HLSLPROGRAM
			#define _MATERIAL_AFFECTS_ALBEDO 1
			#define _MATERIAL_AFFECTS_NORMAL 1
			#define _MATERIAL_AFFECTS_METAL 1
			#define _MATERIAL_AFFECTS_AO 1
			#define _MATERIAL_AFFECTS_SMOOTHNESS 1
			#define _MATERIAL_AFFECTS_EMISSION 1
			#define ASE_SRP_VERSION 70301

			#pragma multi_compile_instancing
			
			#if defined(_MATERIAL_AFFECTS_METAL) || defined(_MATERIAL_AFFECTS_AO) || defined(_MATERIAL_AFFECTS_SMOOTHNESS)
			#define _MATERIAL_AFFECTS_MASKMAP 1
			#endif

			#define SHADERPASS SHADERPASS_DBUFFER_PROJECTOR
            #define DECALS_4RT

			#pragma vertex Vert
			#pragma fragment Frag

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"

			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
			#ifdef DEBUG_DISPLAY
				#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Debug/DebugDisplay.hlsl"
			#endif

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialUtilities.hlsl"
			#include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/Decal.hlsl"

			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0


			struct VertexInput
			{
				float3 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 tangentOS : TANGENT;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_Position;
				#if defined(ASE_NEEDS_FRAG_RELATIVE_WORLD_POS)
				float3 positionRWS : TEXCOORD0;
				#endif
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float _DecalQuantity;
			float _DecalType;
			float _NormalIntensity;
			float _SmoothnessMultiplier;
			float _DrawOrder;
			float _DecalMeshDepthBias;
			CBUFFER_END

			sampler2D _dirt_decal_BaseColor;
			sampler2D _Dirt_Decal_Normal;
			sampler2D _dirt_decal_mask;


			
			void GetSurfaceData(SurfaceDescription surfaceDescription, FragInputs fragInputs, float3 V, PositionInputs posInput, out DecalSurfaceData surfaceData)
			{
				#if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR)
					float4x4 normalToWorld = UNITY_ACCESS_INSTANCED_PROP(Decal, _NormalToWorld);
					float fadeFactor = clamp(normalToWorld[0][3], 0.0f, 1.0f);
					float2 scale = float2(normalToWorld[3][0], normalToWorld[3][1]);
					float2 offset = float2(normalToWorld[3][2], normalToWorld[3][3]);
					fragInputs.texCoord0.xy = fragInputs.texCoord0.xy * scale + offset;
					fragInputs.texCoord1.xy = fragInputs.texCoord1.xy * scale + offset;
					fragInputs.texCoord2.xy = fragInputs.texCoord2.xy * scale + offset;
					fragInputs.texCoord3.xy = fragInputs.texCoord3.xy * scale + offset;
				#else
					float fadeFactor = 1.0;
				#endif

				ZERO_INITIALIZE(DecalSurfaceData, surfaceData);

				#if _MATERIAL_AFFECTS_EMISSION
					//surfaceData.emissive.rgb = surfaceDescription.Emission.rgb * fadeFactor;
				#endif

				#if _MATERIAL_AFFECTS_ALBEDO
					surfaceData.baseColor.xyz = surfaceDescription.Albedo;
					surfaceData.baseColor.w = surfaceDescription.AlphaAlbedo * fadeFactor;
					if(surfaceData.baseColor.w > 0)
					{
						surfaceData.HTileMask |= DBUFFERHTILEBIT_DIFFUSE;
					}
				#endif

				#if _MATERIAL_AFFECTS_NORMAL
					#if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR)
						surfaceData.normalWS.xyz = mul((float3x3)normalToWorld, surfaceDescription.Normal);
					#elif (SHADERPASS == SHADERPASS_DBUFFER_MESH) || (SHADERPASS == SHADERPASS_FORWARD_PREVIEW)
						surfaceData.normalWS.xyz = normalize(TransformTangentToWorld(surfaceDescription.Normal, fragInputs.tangentToWorld));
					#endif
						surfaceData.normalWS.w = surfaceDescription.AlphaNormal * fadeFactor;
						if(surfaceData.normalWS.w > 0)
						{
							surfaceData.HTileMask |= DBUFFERHTILEBIT_NORMAL;
						}
				#else
					#if (SHADERPASS == SHADERPASS_FORWARD_PREVIEW)
						surfaceData.normalWS.xyz = normalize(TransformTangentToWorld(float3(0.0, 0.0, 0.1), fragInputs.tangentToWorld));
					#endif
				#endif

				#if _MATERIAL_AFFECTS_MASKMAP
					surfaceData.mask.z = surfaceDescription.Smoothness;
					#ifdef DECALS_4RT
						surfaceData.mask.x = surfaceDescription.Metallic;
						surfaceData.mask.y = surfaceDescription.Occlusion;
					#endif

						surfaceData.mask.w = surfaceDescription.MAOSOpacity * fadeFactor;
					#ifdef DECALS_4RT
						surfaceData.MAOSBlend.x = surfaceDescription.MAOSOpacity * fadeFactor;
						surfaceData.MAOSBlend.y = surfaceDescription.MAOSOpacity * fadeFactor;
					#endif

					if (surfaceData.mask.w > 0)
					{
						surfaceData.HTileMask |= DBUFFERHTILEBIT_MASK;
					}
				#endif
			}

			VertexOutput Vert( VertexInput inputMesh  )
			{
				VertexOutput o;
				UNITY_SETUP_INSTANCE_ID(inputMesh);
				UNITY_TRANSFER_INSTANCE_ID(inputMesh, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				float3 defaultVertexValue = inputMesh.positionOS.xyz;
				#else
				float3 defaultVertexValue = float3( 0, 0, 0 );
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				inputMesh.positionOS.xyz = vertexValue;
				#else
				inputMesh.positionOS.xyz += vertexValue;
				#endif

				inputMesh.normalOS = inputMesh.normalOS;
				inputMesh.tangentOS = inputMesh.tangentOS;

				float3 positionRWS = TransformObjectToWorld(inputMesh.positionOS);
				float3 normalWS = TransformObjectToWorldNormal(inputMesh.normalOS);
				float4 tangentWS = float4(TransformObjectToWorldDir(inputMesh.tangentOS.xyz), inputMesh.tangentOS.w);

				o.positionCS = TransformWorldToHClip(positionRWS);
				#if defined(ASE_NEEDS_FRAG_RELATIVE_WORLD_POS)
				o.positionRWS = positionRWS;
				#endif
				//o.normalWS.xyz = normalWS;
				//o.tangentWS.xyzw = tangentWS;

				#if (SHADERPASS == SHADERPASS_DBUFFER_MESH)
					#if defined(UNITY_REVERSED_Z)
						o.positionCS.z -= _DecalMeshDepthBias;
					#else
						o.positionCS.z += _DecalMeshDepthBias;
					#endif
				#endif
				return o;
			}

			void Frag( VertexOutput packedInput,
				#if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_DBUFFER_MESH)
					OUTPUT_DBUFFER(outDBuffer)
				#else
					out float4 outEmissive : SV_Target0
				#endif
				
				)
			{
				UNITY_SETUP_INSTANCE_ID( packedInput );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( packedInput );
				FragInputs input;
				ZERO_INITIALIZE(FragInputs, input);
				input.tangentToWorld = k_identity3x3;
				#if defined(ASE_NEEDS_FRAG_RELATIVE_WORLD_POS)
				float3 positionRWS = packedInput.positionRWS;
				input.positionRWS = positionRWS;
				#endif

				input.positionSS = packedInput.positionCS;

				float clipValue = 1.0;

				#if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR)
					float depth = LoadCameraDepth(input.positionSS.xy);
					PositionInputs posInput = GetPositionInput(input.positionSS.xy, _ScreenSize.zw, depth, UNITY_MATRIX_I_VP, UNITY_MATRIX_V);

					float3 positionDS = TransformWorldToObject(posInput.positionWS);
					positionDS = positionDS * float3(1.0, -1.0, 1.0) + float3(0.5, 0.5, 0.5);
					if (!(all(positionDS.xyz > 0.0f) && all(1.0f - positionDS.xyz > 0.0f)))
					{
						clipValue = -1.0;
						#ifndef SHADER_API_METAL
						clip(clipValue);
						#endif
					}

					input.texCoord0.xy = positionDS.xz;
					input.texCoord1.xy = positionDS.xz;
					input.texCoord2.xy = positionDS.xz;
					input.texCoord3.xy = positionDS.xz;

					float3 V = GetWorldSpaceNormalizeViewDir(posInput.positionWS);
				#else
					PositionInputs posInput = GetPositionInput(input.positionSS.xy, _ScreenSize.zw, input.positionSS.z, input.positionSS.w, input.positionRWS.xyz, uint2(0, 0));

					#if defined(ASE_NEEDS_FRAG_RELATIVE_WORLD_POS)
					float3 V = GetWorldSpaceNormalizeViewDir(input.positionRWS);
					#else
					float3 V = float3(1.0, 1.0, 1.0);
					#endif
				#endif

				float4 texCoord0 = input.texCoord0;
				float4 texCoord1 = input.texCoord1;
				float4 texCoord2 = input.texCoord2;
				float4 texCoord3 = input.texCoord3;

				SurfaceDescription surfaceDescription = (SurfaceDescription)0;
				float2 texCoord25 = texCoord0.xy * float2( 1,1 ) + float2( 0,0 );
				// *** BEGIN Flipbook UV Animation vars ***
				// Total tiles of Flipbook Texture
				float fbtotaltiles27 = _DecalQuantity * _DecalQuantity;
				// Offsets for cols and rows of Flipbook Texture
				float fbcolsoffset27 = 1.0f / _DecalQuantity;
				float fbrowsoffset27 = 1.0f / _DecalQuantity;
				// Speed of animation
				float fbspeed27 = _Time[ 1 ] * 0.0;
				// UV Tiling (col and row offset)
				float2 fbtiling27 = float2(fbcolsoffset27, fbrowsoffset27);
				// UV Offset - calculate current tile linear index, and convert it to (X * coloffset, Y * rowoffset)
				// Calculate current tile linear index
				float fbcurrenttileindex27 = round( fmod( fbspeed27 + _DecalType, fbtotaltiles27) );
				fbcurrenttileindex27 += ( fbcurrenttileindex27 < 0) ? fbtotaltiles27 : 0;
				// Obtain Offset X coordinate from current tile linear index
				float fblinearindextox27 = round ( fmod ( fbcurrenttileindex27, _DecalQuantity ) );
				// Multiply Offset X by coloffset
				float fboffsetx27 = fblinearindextox27 * fbcolsoffset27;
				// Obtain Offset Y coordinate from current tile linear index
				float fblinearindextoy27 = round( fmod( ( fbcurrenttileindex27 - fblinearindextox27 ) / _DecalQuantity, _DecalQuantity ) );
				// Reverse Y to get tiles from Top to Bottom
				fblinearindextoy27 = (int)(_DecalQuantity-1) - fblinearindextoy27;
				// Multiply Offset Y by rowoffset
				float fboffsety27 = fblinearindextoy27 * fbrowsoffset27;
				// UV Offset
				float2 fboffset27 = float2(fboffsetx27, fboffsety27);
				// Flipbook UV
				half2 fbuv27 = texCoord25 * fbtiling27 + fboffset27;
				// *** END Flipbook UV Animation vars ***
				float4 tex2DNode17 = tex2D( _dirt_decal_BaseColor, fbuv27 );
				
				float3 unpack19 = UnpackNormalScale( tex2D( _Dirt_Decal_Normal, fbuv27 ), _NormalIntensity );
				unpack19.z = lerp( 1, unpack19.z, saturate(_NormalIntensity) );
				
				float4 tex2DNode18 = tex2D( _dirt_decal_mask, fbuv27 );
				
				surfaceDescription.Albedo = tex2DNode17.rgb;
				surfaceDescription.AlphaAlbedo = tex2DNode17.a;
				surfaceDescription.Normal = unpack19;
				surfaceDescription.AlphaNormal = tex2DNode17.a;
				surfaceDescription.Metallic = tex2DNode18.r;
				surfaceDescription.Occlusion = tex2DNode18.g;
				surfaceDescription.Smoothness = ( tex2DNode18.a * _SmoothnessMultiplier );
				surfaceDescription.MAOSOpacity = tex2DNode17.a;
				surfaceDescription.Emission = float3( 0, 0, 0 );

				DecalSurfaceData surfaceData;
				GetSurfaceData(surfaceDescription, input, V, posInput, surfaceData);


				#if ((SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_DBUFFER_MESH)) && defined(PLATFORM_SUPPORTS_BUFFER_ATOMICS_IN_PIXEL_SHADER)
					uint2 htileCoord = input.positionSS.xy / 8;
					int stride = (_ScreenSize.x + 7) / 8;
					uint mask = surfaceData.HTileMask;
					uint tileCoord1d = htileCoord.y * stride + htileCoord.x;
					#ifdef PLATFORM_SUPPORTS_WAVE_INTRINSICS
						uint minTileCoord1d = WaveActiveMin(tileCoord1d);
						while (minTileCoord1d != -1)
						{
							if ((minTileCoord1d == tileCoord1d) && (clipValue > 0.0))
							{
								mask = WaveActiveBitOr(surfaceData.HTileMask);

								if(WaveIsFirstLane())
								{
									if (tileCoord1d != -1)
									{
										tileCoord1d = htileCoord.y * stride + htileCoord.x;
									}
									InterlockedOr(_DecalPropertyMaskBuffer[tileCoord1d], mask);
								}
								tileCoord1d = -1;
							}
							if (tileCoord1d != -1)
							{
								tileCoord1d = htileCoord.y * stride + htileCoord.x;
							}
							minTileCoord1d = WaveActiveMin(tileCoord1d);
						}
					#else
						InterlockedOr(_DecalPropertyMaskBuffer[tileCoord1d], mask);
					#endif
				#endif

				#if ((SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR)) && defined(SHADER_API_METAL)
					clip(clipValue);
				#endif

				#if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_DBUFFER_MESH)
					ENCODE_INTO_DBUFFER(surfaceData, outDBuffer);
				#else
					outEmissive.rgb = surfaceData.emissive * GetCurrentExposureMultiplier();
					outEmissive.a = 1.0;
				#endif
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "ShaderGraph_ProjectorEmissive"
            Tags { "LightMode"="ShaderGraph_ProjectorEmissive" }
        
            Blend 0 SrcAlpha One
            Cull Front
            ZTest Greater
            ZWrite Off

			HLSLPROGRAM
			#define _MATERIAL_AFFECTS_ALBEDO 1
			#define _MATERIAL_AFFECTS_NORMAL 1
			#define _MATERIAL_AFFECTS_METAL 1
			#define _MATERIAL_AFFECTS_AO 1
			#define _MATERIAL_AFFECTS_SMOOTHNESS 1
			#define _MATERIAL_AFFECTS_EMISSION 1
			#define ASE_SRP_VERSION 70301

			#pragma multi_compile_instancing
			
			#if defined(_MATERIAL_AFFECTS_METAL) || defined(_MATERIAL_AFFECTS_AO) || defined(_MATERIAL_AFFECTS_SMOOTHNESS)
			#define _MATERIAL_AFFECTS_MASKMAP 1
			#endif

			#define SHADERPASS SHADERPASS_FORWARD_EMISSIVE_PROJECTOR

			#pragma vertex Vert
			#pragma fragment Frag

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"

			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
			#ifdef DEBUG_DISPLAY
				#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Debug/DebugDisplay.hlsl"
			#endif

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialUtilities.hlsl"
			#include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/Decal.hlsl"

			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0


			struct VertexInput
			{
				float3 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 tangentOS : TANGENT;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_Position;
				#if defined(ASE_NEEDS_FRAG_RELATIVE_WORLD_POS)
				float3 positionRWS : TEXCOORD0;
				#endif
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float _DecalQuantity;
			float _DecalType;
			float _NormalIntensity;
			float _SmoothnessMultiplier;
			float _DrawOrder;
			float _DecalMeshDepthBias;
			CBUFFER_END

			sampler2D _dirt_decal_BaseColor;
			sampler2D _Dirt_Decal_Normal;
			sampler2D _dirt_decal_mask;


			
			void GetSurfaceData(SurfaceDescription surfaceDescription, FragInputs fragInputs, float3 V, PositionInputs posInput, out DecalSurfaceData surfaceData)
			{
				#if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR)
					float4x4 normalToWorld = UNITY_ACCESS_INSTANCED_PROP(Decal, _NormalToWorld);
					float fadeFactor = clamp(normalToWorld[0][3], 0.0f, 1.0f);
					float2 scale = float2(normalToWorld[3][0], normalToWorld[3][1]);
					float2 offset = float2(normalToWorld[3][2], normalToWorld[3][3]);
					fragInputs.texCoord0.xy = fragInputs.texCoord0.xy * scale + offset;
					fragInputs.texCoord1.xy = fragInputs.texCoord1.xy * scale + offset;
					fragInputs.texCoord2.xy = fragInputs.texCoord2.xy * scale + offset;
					fragInputs.texCoord3.xy = fragInputs.texCoord3.xy * scale + offset;
				#else
					float fadeFactor = 1.0;
				#endif

				ZERO_INITIALIZE(DecalSurfaceData, surfaceData);

				#if _MATERIAL_AFFECTS_EMISSION
					surfaceData.emissive.rgb = surfaceDescription.Emission.rgb * fadeFactor;
				#endif

				#if _MATERIAL_AFFECTS_ALBEDO
					surfaceData.baseColor.xyz = surfaceDescription.Albedo;
					surfaceData.baseColor.w = surfaceDescription.AlphaAlbedo * fadeFactor;
					if(surfaceData.baseColor.w > 0)
					{
						surfaceData.HTileMask |= DBUFFERHTILEBIT_DIFFUSE;
					}
				#endif

				#if _MATERIAL_AFFECTS_NORMAL
					#if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR)
						surfaceData.normalWS.xyz = mul((float3x3)normalToWorld, surfaceDescription.Normal);
					#elif (SHADERPASS == SHADERPASS_DBUFFER_MESH) || (SHADERPASS == SHADERPASS_FORWARD_PREVIEW)
						surfaceData.normalWS.xyz = normalize(TransformTangentToWorld(surfaceDescription.Normal, fragInputs.tangentToWorld));
					#endif
						surfaceData.normalWS.w = surfaceDescription.AlphaNormal * fadeFactor;
						if(surfaceData.normalWS.w > 0)
						{
							surfaceData.HTileMask |= DBUFFERHTILEBIT_NORMAL;
						}
				#else
					#if (SHADERPASS == SHADERPASS_FORWARD_PREVIEW)
						surfaceData.normalWS.xyz = normalize(TransformTangentToWorld(float3(0.0, 0.0, 0.1), fragInputs.tangentToWorld));
					#endif
				#endif

				#if _MATERIAL_AFFECTS_MASKMAP
					surfaceData.mask.z = surfaceDescription.Smoothness;
					#ifdef DECALS_4RT
						surfaceData.mask.x = surfaceDescription.Metallic;
						surfaceData.mask.y = surfaceDescription.Occlusion;
					#endif

						surfaceData.mask.w = surfaceDescription.MAOSOpacity * fadeFactor;
					#ifdef DECALS_4RT
						surfaceData.MAOSBlend.x = surfaceDescription.MAOSOpacity * fadeFactor;
						surfaceData.MAOSBlend.y = surfaceDescription.MAOSOpacity * fadeFactor;
					#endif

					if (surfaceData.mask.w > 0)
					{
						surfaceData.HTileMask |= DBUFFERHTILEBIT_MASK;
					}
				#endif
			}

			VertexOutput Vert( VertexInput inputMesh  )
			{
				VertexOutput o;
				UNITY_SETUP_INSTANCE_ID(inputMesh);
				UNITY_TRANSFER_INSTANCE_ID(inputMesh, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				float3 defaultVertexValue = inputMesh.positionOS.xyz;
				#else
				float3 defaultVertexValue = float3( 0, 0, 0 );
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				inputMesh.positionOS.xyz = vertexValue;
				#else
				inputMesh.positionOS.xyz += vertexValue;
				#endif

				inputMesh.normalOS = inputMesh.normalOS;
				inputMesh.tangentOS = inputMesh.tangentOS;

				float3 positionRWS = TransformObjectToWorld(inputMesh.positionOS);
				float3 normalWS = TransformObjectToWorldNormal(inputMesh.normalOS);
				float4 tangentWS = float4(TransformObjectToWorldDir(inputMesh.tangentOS.xyz), inputMesh.tangentOS.w);

				o.positionCS = TransformWorldToHClip(positionRWS);
				#if defined(ASE_NEEDS_FRAG_RELATIVE_WORLD_POS)
				o.positionRWS = positionRWS;
				#endif
				//o.normalWS.xyz = normalWS;
				//o.tangentWS.xyzw = tangentWS;

				#if (SHADERPASS == SHADERPASS_DBUFFER_MESH)
					#if defined(UNITY_REVERSED_Z)
						o.positionCS.z -= _DecalMeshDepthBias;
					#else
						o.positionCS.z += _DecalMeshDepthBias;
					#endif
				#endif
				return o;
			}

			void Frag( VertexOutput packedInput,
				#if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_DBUFFER_MESH)
					OUTPUT_DBUFFER(outDBuffer)
				#else
					out float4 outEmissive : SV_Target0
				#endif
				
				)
			{
				UNITY_SETUP_INSTANCE_ID( packedInput );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( packedInput );
				FragInputs input;
				ZERO_INITIALIZE(FragInputs, input);
				input.tangentToWorld = k_identity3x3;
				#if defined(ASE_NEEDS_FRAG_RELATIVE_WORLD_POS)
				float3 positionRWS = packedInput.positionRWS;
				input.positionRWS = positionRWS;
				#endif

				input.positionSS = packedInput.positionCS;

				float clipValue = 1.0;

				#if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR)
					float depth = LoadCameraDepth(input.positionSS.xy);
					PositionInputs posInput = GetPositionInput(input.positionSS.xy, _ScreenSize.zw, depth, UNITY_MATRIX_I_VP, UNITY_MATRIX_V);

					float3 positionDS = TransformWorldToObject(posInput.positionWS);
					positionDS = positionDS * float3(1.0, -1.0, 1.0) + float3(0.5, 0.5, 0.5);
					if (!(all(positionDS.xyz > 0.0f) && all(1.0f - positionDS.xyz > 0.0f)))
					{
						clipValue = -1.0;
						#ifndef SHADER_API_METAL
						clip(clipValue);
						#endif
					}

					input.texCoord0.xy = positionDS.xz;
					input.texCoord1.xy = positionDS.xz;
					input.texCoord2.xy = positionDS.xz;
					input.texCoord3.xy = positionDS.xz;

					float3 V = GetWorldSpaceNormalizeViewDir(posInput.positionWS);
				#else
					PositionInputs posInput = GetPositionInput(input.positionSS.xy, _ScreenSize.zw, input.positionSS.z, input.positionSS.w, input.positionRWS.xyz, uint2(0, 0));

					#if defined(ASE_NEEDS_FRAG_RELATIVE_WORLD_POS)
					float3 V = GetWorldSpaceNormalizeViewDir(input.positionRWS);
					#else
					float3 V = float3(1.0, 1.0, 1.0);
					#endif
				#endif

				float4 texCoord0 = input.texCoord0;
				float4 texCoord1 = input.texCoord1;
				float4 texCoord2 = input.texCoord2;
				float4 texCoord3 = input.texCoord3;

				SurfaceDescription surfaceDescription = (SurfaceDescription)0;
				float2 texCoord25 = texCoord0.xy * float2( 1,1 ) + float2( 0,0 );
				// *** BEGIN Flipbook UV Animation vars ***
				// Total tiles of Flipbook Texture
				float fbtotaltiles27 = _DecalQuantity * _DecalQuantity;
				// Offsets for cols and rows of Flipbook Texture
				float fbcolsoffset27 = 1.0f / _DecalQuantity;
				float fbrowsoffset27 = 1.0f / _DecalQuantity;
				// Speed of animation
				float fbspeed27 = _Time[ 1 ] * 0.0;
				// UV Tiling (col and row offset)
				float2 fbtiling27 = float2(fbcolsoffset27, fbrowsoffset27);
				// UV Offset - calculate current tile linear index, and convert it to (X * coloffset, Y * rowoffset)
				// Calculate current tile linear index
				float fbcurrenttileindex27 = round( fmod( fbspeed27 + _DecalType, fbtotaltiles27) );
				fbcurrenttileindex27 += ( fbcurrenttileindex27 < 0) ? fbtotaltiles27 : 0;
				// Obtain Offset X coordinate from current tile linear index
				float fblinearindextox27 = round ( fmod ( fbcurrenttileindex27, _DecalQuantity ) );
				// Multiply Offset X by coloffset
				float fboffsetx27 = fblinearindextox27 * fbcolsoffset27;
				// Obtain Offset Y coordinate from current tile linear index
				float fblinearindextoy27 = round( fmod( ( fbcurrenttileindex27 - fblinearindextox27 ) / _DecalQuantity, _DecalQuantity ) );
				// Reverse Y to get tiles from Top to Bottom
				fblinearindextoy27 = (int)(_DecalQuantity-1) - fblinearindextoy27;
				// Multiply Offset Y by rowoffset
				float fboffsety27 = fblinearindextoy27 * fbrowsoffset27;
				// UV Offset
				float2 fboffset27 = float2(fboffsetx27, fboffsety27);
				// Flipbook UV
				half2 fbuv27 = texCoord25 * fbtiling27 + fboffset27;
				// *** END Flipbook UV Animation vars ***
				float4 tex2DNode17 = tex2D( _dirt_decal_BaseColor, fbuv27 );
				
				float3 unpack19 = UnpackNormalScale( tex2D( _Dirt_Decal_Normal, fbuv27 ), _NormalIntensity );
				unpack19.z = lerp( 1, unpack19.z, saturate(_NormalIntensity) );
				
				float4 tex2DNode18 = tex2D( _dirt_decal_mask, fbuv27 );
				
				surfaceDescription.Albedo = tex2DNode17.rgb;
				surfaceDescription.AlphaAlbedo = tex2DNode17.a;
				surfaceDescription.Normal = unpack19;
				surfaceDescription.AlphaNormal = tex2DNode17.a;
				surfaceDescription.Metallic = tex2DNode18.r;
				surfaceDescription.Occlusion = tex2DNode18.g;
				surfaceDescription.Smoothness = ( tex2DNode18.a * _SmoothnessMultiplier );
				surfaceDescription.MAOSOpacity = tex2DNode17.a;
				surfaceDescription.Emission = float3( 0, 0, 0 );

				DecalSurfaceData surfaceData;
				GetSurfaceData(surfaceDescription, input, V, posInput, surfaceData);


				#if ((SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_DBUFFER_MESH)) && defined(PLATFORM_SUPPORTS_BUFFER_ATOMICS_IN_PIXEL_SHADER)
					uint2 htileCoord = input.positionSS.xy / 8;
					int stride = (_ScreenSize.x + 7) / 8;
					uint mask = surfaceData.HTileMask;
					uint tileCoord1d = htileCoord.y * stride + htileCoord.x;
					#ifdef PLATFORM_SUPPORTS_WAVE_INTRINSICS
						uint minTileCoord1d = WaveActiveMin(tileCoord1d);
						while (minTileCoord1d != -1)
						{
							if ((minTileCoord1d == tileCoord1d) && (clipValue > 0.0))
							{
								mask = WaveActiveBitOr(surfaceData.HTileMask);

								if(WaveIsFirstLane())
								{
									if (tileCoord1d != -1)
									{
										tileCoord1d = htileCoord.y * stride + htileCoord.x;
									}
									InterlockedOr(_DecalPropertyMaskBuffer[tileCoord1d], mask);
								}
								tileCoord1d = -1;
							}
							if (tileCoord1d != -1)
							{
								tileCoord1d = htileCoord.y * stride + htileCoord.x;
							}
							minTileCoord1d = WaveActiveMin(tileCoord1d);
						}
					#else
						InterlockedOr(_DecalPropertyMaskBuffer[tileCoord1d], mask);
					#endif
				#endif

				#if ((SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR)) && defined(SHADER_API_METAL)
					clip(clipValue);
				#endif

				#if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_DBUFFER_MESH)
					ENCODE_INTO_DBUFFER(surfaceData, outDBuffer);
				#else
					outEmissive.rgb = surfaceData.emissive * GetCurrentExposureMultiplier();
					outEmissive.a = 1.0;
				#endif
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "ShaderGraph_DBufferMesh3RT"
            Tags { "LightMode"="ShaderGraph_DBufferMesh3RT" }

			Blend 0 SrcAlpha OneMinusSrcAlpha, Zero OneMinusSrcAlpha Blend 1 SrcAlpha OneMinusSrcAlpha, Zero OneMinusSrcAlpha Blend 2 SrcAlpha OneMinusSrcAlpha, Zero OneMinusSrcAlpha
        
            ZTest LEqual
            ZWrite Off

			Stencil
			{
				Ref 16
				WriteMask 16
				Comp Always
				Pass Replace
				Fail Keep
				ZFail Keep
			}

        
            ColorMask BA 2 ColorMask 0 3

			HLSLPROGRAM
			#define _MATERIAL_AFFECTS_ALBEDO 1
			#define _MATERIAL_AFFECTS_NORMAL 1
			#define _MATERIAL_AFFECTS_METAL 1
			#define _MATERIAL_AFFECTS_AO 1
			#define _MATERIAL_AFFECTS_SMOOTHNESS 1
			#define _MATERIAL_AFFECTS_EMISSION 1
			#define ASE_SRP_VERSION 70301

			#pragma multi_compile_instancing
			
			#if defined(_MATERIAL_AFFECTS_METAL) || defined(_MATERIAL_AFFECTS_AO) || defined(_MATERIAL_AFFECTS_SMOOTHNESS)
			#define _MATERIAL_AFFECTS_MASKMAP 1
			#endif

			#if !defined(ASE_NEEDS_FRAG_RELATIVE_WORLD_POS)
			#define ASE_NEEDS_FRAG_RELATIVE_WORLD_POS
			#endif

			#define SHADERPASS SHADERPASS_DBUFFER_MESH
			#define DECALS_3RT

			#pragma vertex Vert
			#pragma fragment Frag

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"

			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
			#ifdef DEBUG_DISPLAY
				#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Debug/DebugDisplay.hlsl"
			#endif

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialUtilities.hlsl"
			#include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/Decal.hlsl"

			

			struct VertexInput
			{
				float3 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 tangentOS : TANGENT;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_Position;
				#if defined(ASE_NEEDS_FRAG_RELATIVE_WORLD_POS)
				float3 positionRWS : TEXCOORD0;
				#endif
				float3 normalWS : TEXCOORD1;
				float4 tangentWS : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float _DecalQuantity;
			float _DecalType;
			float _NormalIntensity;
			float _SmoothnessMultiplier;
			float _DrawOrder;
			float _DecalMeshDepthBias;
			CBUFFER_END

			sampler2D _dirt_decal_BaseColor;
			sampler2D _Dirt_Decal_Normal;
			sampler2D _dirt_decal_mask;


			
			void GetSurfaceData(SurfaceDescription surfaceDescription, FragInputs fragInputs, float3 V, PositionInputs posInput, out DecalSurfaceData surfaceData)
			{
				#if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR)
					float4x4 normalToWorld = UNITY_ACCESS_INSTANCED_PROP(Decal, _NormalToWorld);
					float fadeFactor = clamp(normalToWorld[0][3], 0.0f, 1.0f);
					float2 scale = float2(normalToWorld[3][0], normalToWorld[3][1]);
					float2 offset = float2(normalToWorld[3][2], normalToWorld[3][3]);
					fragInputs.texCoord0.xy = fragInputs.texCoord0.xy * scale + offset;
					fragInputs.texCoord1.xy = fragInputs.texCoord1.xy * scale + offset;
					fragInputs.texCoord2.xy = fragInputs.texCoord2.xy * scale + offset;
					fragInputs.texCoord3.xy = fragInputs.texCoord3.xy * scale + offset;
				#else
					float fadeFactor = 1.0;
				#endif

				ZERO_INITIALIZE(DecalSurfaceData, surfaceData);

				#if _MATERIAL_AFFECTS_EMISSION
					//surfaceData.emissive.rgb = surfaceDescription.Emission.rgb * fadeFactor;
				#endif

				#if _MATERIAL_AFFECTS_ALBEDO
					surfaceData.baseColor.xyz = surfaceDescription.Albedo;
					surfaceData.baseColor.w = surfaceDescription.AlphaAlbedo * fadeFactor;
					if(surfaceData.baseColor.w > 0)
					{
						surfaceData.HTileMask |= DBUFFERHTILEBIT_DIFFUSE;
					}
				#endif

				#if _MATERIAL_AFFECTS_NORMAL
					#if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR)
						surfaceData.normalWS.xyz = mul((float3x3)normalToWorld, surfaceDescription.Normal);
					#elif (SHADERPASS == SHADERPASS_DBUFFER_MESH) || (SHADERPASS == SHADERPASS_FORWARD_PREVIEW)
						surfaceData.normalWS.xyz = normalize(TransformTangentToWorld(surfaceDescription.Normal, fragInputs.tangentToWorld));
					#endif
						surfaceData.normalWS.w = surfaceDescription.AlphaNormal * fadeFactor;
						if(surfaceData.normalWS.w > 0)
						{
							surfaceData.HTileMask |= DBUFFERHTILEBIT_NORMAL;
						}
				#else
					#if (SHADERPASS == SHADERPASS_FORWARD_PREVIEW)
						surfaceData.normalWS.xyz = normalize(TransformTangentToWorld(float3(0.0, 0.0, 0.1), fragInputs.tangentToWorld));
					#endif
				#endif

				#if _MATERIAL_AFFECTS_MASKMAP
					surfaceData.mask.z = surfaceDescription.Smoothness;
					#ifdef DECALS_4RT
						surfaceData.mask.x = surfaceDescription.Metallic;
						surfaceData.mask.y = surfaceDescription.Occlusion;
					#endif

						surfaceData.mask.w = surfaceDescription.MAOSOpacity * fadeFactor;
					#ifdef DECALS_4RT
						surfaceData.MAOSBlend.x = surfaceDescription.MAOSOpacity * fadeFactor;
						surfaceData.MAOSBlend.y = surfaceDescription.MAOSOpacity * fadeFactor;
					#endif

					if (surfaceData.mask.w > 0)
					{
						surfaceData.HTileMask |= DBUFFERHTILEBIT_MASK;
					}
				#endif
			}

			VertexOutput Vert( VertexInput inputMesh  )
			{
				VertexOutput o;
				UNITY_SETUP_INSTANCE_ID(inputMesh);
				UNITY_TRANSFER_INSTANCE_ID(inputMesh, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				o.ase_texcoord3.xy = inputMesh.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord3.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				float3 defaultVertexValue = inputMesh.positionOS.xyz;
				#else
				float3 defaultVertexValue = float3( 0, 0, 0 );
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				inputMesh.positionOS.xyz = vertexValue;
				#else
				inputMesh.positionOS.xyz += vertexValue;
				#endif

				inputMesh.normalOS = inputMesh.normalOS;
				inputMesh.tangentOS = inputMesh.tangentOS;

				float3 positionRWS = TransformObjectToWorld(inputMesh.positionOS);
				float3 normalWS = TransformObjectToWorldNormal(inputMesh.normalOS);
				float4 tangentWS = float4(TransformObjectToWorldDir(inputMesh.tangentOS.xyz), inputMesh.tangentOS.w);

				o.positionCS = TransformWorldToHClip(positionRWS);
				#if defined(ASE_NEEDS_FRAG_RELATIVE_WORLD_POS)
				o.positionRWS = positionRWS;
				#endif
				o.normalWS = normalWS;
				o.tangentWS = tangentWS;

				#if (SHADERPASS == SHADERPASS_DBUFFER_MESH)
					#if defined(UNITY_REVERSED_Z)
						o.positionCS.z -= _DecalMeshDepthBias;
					#else
						o.positionCS.z += _DecalMeshDepthBias;
					#endif
				#endif
				return o;
			}

			void Frag( VertexOutput packedInput,
				#if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_DBUFFER_MESH)
					OUTPUT_DBUFFER(outDBuffer)
				#else
					out float4 outEmissive : SV_Target0
				#endif
				
				)
			{
				UNITY_SETUP_INSTANCE_ID( packedInput );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( packedInput );
				FragInputs input;
				ZERO_INITIALIZE(FragInputs, input);
				input.tangentToWorld = k_identity3x3;
				#if defined(ASE_NEEDS_FRAG_RELATIVE_WORLD_POS)
				float3 positionRWS = packedInput.positionRWS;
				input.positionRWS = positionRWS;
				#endif
				float3 normalWS = packedInput.normalWS;
				float4 tangentWS = packedInput.tangentWS;
				input.tangentToWorld = BuildTangentToWorld(tangentWS, normalWS);
				input.positionSS = packedInput.positionCS;

				float clipValue = 1.0;

				#if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR)
					float depth = LoadCameraDepth(input.positionSS.xy);
					PositionInputs posInput = GetPositionInput(input.positionSS.xy, _ScreenSize.zw, depth, UNITY_MATRIX_I_VP, UNITY_MATRIX_V);

					float3 positionDS = TransformWorldToObject(posInput.positionWS);
					positionDS = positionDS * float3(1.0, -1.0, 1.0) + float3(0.5, 0.5, 0.5);
					if (!(all(positionDS.xyz > 0.0f) && all(1.0f - positionDS.xyz > 0.0f)))
					{
						clipValue = -1.0;
						#ifndef SHADER_API_METAL
						clip(clipValue);
						#endif
					}

					input.texCoord0.xy = positionDS.xz;
					input.texCoord1.xy = positionDS.xz;
					input.texCoord2.xy = positionDS.xz;
					input.texCoord3.xy = positionDS.xz;

					float3 V = GetWorldSpaceNormalizeViewDir(posInput.positionWS);
				#else
					PositionInputs posInput = GetPositionInput(input.positionSS.xy, _ScreenSize.zw, input.positionSS.z, input.positionSS.w, input.positionRWS.xyz, uint2(0, 0));

					#if defined(ASE_NEEDS_FRAG_RELATIVE_WORLD_POS)
					float3 V = GetWorldSpaceNormalizeViewDir(input.positionRWS);
					#else
					float3 V = float3(1.0, 1.0, 1.0);
					#endif
				#endif

				SurfaceDescription surfaceDescription = (SurfaceDescription)0;
				float2 texCoord25 = packedInput.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
				// *** BEGIN Flipbook UV Animation vars ***
				// Total tiles of Flipbook Texture
				float fbtotaltiles27 = _DecalQuantity * _DecalQuantity;
				// Offsets for cols and rows of Flipbook Texture
				float fbcolsoffset27 = 1.0f / _DecalQuantity;
				float fbrowsoffset27 = 1.0f / _DecalQuantity;
				// Speed of animation
				float fbspeed27 = _Time[ 1 ] * 0.0;
				// UV Tiling (col and row offset)
				float2 fbtiling27 = float2(fbcolsoffset27, fbrowsoffset27);
				// UV Offset - calculate current tile linear index, and convert it to (X * coloffset, Y * rowoffset)
				// Calculate current tile linear index
				float fbcurrenttileindex27 = round( fmod( fbspeed27 + _DecalType, fbtotaltiles27) );
				fbcurrenttileindex27 += ( fbcurrenttileindex27 < 0) ? fbtotaltiles27 : 0;
				// Obtain Offset X coordinate from current tile linear index
				float fblinearindextox27 = round ( fmod ( fbcurrenttileindex27, _DecalQuantity ) );
				// Multiply Offset X by coloffset
				float fboffsetx27 = fblinearindextox27 * fbcolsoffset27;
				// Obtain Offset Y coordinate from current tile linear index
				float fblinearindextoy27 = round( fmod( ( fbcurrenttileindex27 - fblinearindextox27 ) / _DecalQuantity, _DecalQuantity ) );
				// Reverse Y to get tiles from Top to Bottom
				fblinearindextoy27 = (int)(_DecalQuantity-1) - fblinearindextoy27;
				// Multiply Offset Y by rowoffset
				float fboffsety27 = fblinearindextoy27 * fbrowsoffset27;
				// UV Offset
				float2 fboffset27 = float2(fboffsetx27, fboffsety27);
				// Flipbook UV
				half2 fbuv27 = texCoord25 * fbtiling27 + fboffset27;
				// *** END Flipbook UV Animation vars ***
				float4 tex2DNode17 = tex2D( _dirt_decal_BaseColor, fbuv27 );
				
				float3 unpack19 = UnpackNormalScale( tex2D( _Dirt_Decal_Normal, fbuv27 ), _NormalIntensity );
				unpack19.z = lerp( 1, unpack19.z, saturate(_NormalIntensity) );
				
				float4 tex2DNode18 = tex2D( _dirt_decal_mask, fbuv27 );
				
				surfaceDescription.Albedo = tex2DNode17.rgb;
				surfaceDescription.AlphaAlbedo = tex2DNode17.a;
				surfaceDescription.Normal = unpack19;
				surfaceDescription.AlphaNormal = tex2DNode17.a;
				surfaceDescription.Metallic = tex2DNode18.r;
				surfaceDescription.Occlusion = tex2DNode18.g;
				surfaceDescription.Smoothness = ( tex2DNode18.a * _SmoothnessMultiplier );
				surfaceDescription.MAOSOpacity = tex2DNode17.a;
				surfaceDescription.Emission = float3( 0, 0, 0 );

				DecalSurfaceData surfaceData;
				GetSurfaceData(surfaceDescription, input, V, posInput, surfaceData);


				#if ((SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_DBUFFER_MESH)) && defined(PLATFORM_SUPPORTS_BUFFER_ATOMICS_IN_PIXEL_SHADER)
					uint2 htileCoord = input.positionSS.xy / 8;
					int stride = (_ScreenSize.x + 7) / 8;
					uint mask = surfaceData.HTileMask;
					uint tileCoord1d = htileCoord.y * stride + htileCoord.x;
					#ifdef PLATFORM_SUPPORTS_WAVE_INTRINSICS
						uint minTileCoord1d = WaveActiveMin(tileCoord1d);
						while (minTileCoord1d != -1)
						{
							if ((minTileCoord1d == tileCoord1d) && (clipValue > 0.0))
							{
								mask = WaveActiveBitOr(surfaceData.HTileMask);

								if(WaveIsFirstLane())
								{
									if (tileCoord1d != -1)
									{
										tileCoord1d = htileCoord.y * stride + htileCoord.x;
									}
									InterlockedOr(_DecalPropertyMaskBuffer[tileCoord1d], mask);
								}
								tileCoord1d = -1;
							}
							if (tileCoord1d != -1)
							{
								tileCoord1d = htileCoord.y * stride + htileCoord.x;
							}
							minTileCoord1d = WaveActiveMin(tileCoord1d);
						}
					#else
						InterlockedOr(_DecalPropertyMaskBuffer[tileCoord1d], mask);
					#endif
				#endif

				#if ((SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR)) && defined(SHADER_API_METAL)
					clip(clipValue);
				#endif

				#if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_DBUFFER_MESH)
					ENCODE_INTO_DBUFFER(surfaceData, outDBuffer);
				#else
					outEmissive.rgb = surfaceData.emissive * GetCurrentExposureMultiplier();
					outEmissive.a = 1.0;
				#endif
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "ShaderGraph_DBufferMesh4RT"
			Tags { "LightMode"="ShaderGraph_DBufferMesh4RT" }

			Blend 0 SrcAlpha OneMinusSrcAlpha, Zero OneMinusSrcAlpha Blend 1 SrcAlpha OneMinusSrcAlpha, Zero OneMinusSrcAlpha Blend 2 SrcAlpha OneMinusSrcAlpha, Zero OneMinusSrcAlpha Blend 3 Zero OneMinusSrcColor

			ZTest LEqual
			ZWrite Off

			Stencil
			{
				Ref 16
				WriteMask 16
				Comp Always
				Pass Replace
				Fail Keep
				ZFail Keep
			}


			ColorMask RGBA 2 ColorMask RG 3

			HLSLPROGRAM
			#define _MATERIAL_AFFECTS_ALBEDO 1
			#define _MATERIAL_AFFECTS_NORMAL 1
			#define _MATERIAL_AFFECTS_METAL 1
			#define _MATERIAL_AFFECTS_AO 1
			#define _MATERIAL_AFFECTS_SMOOTHNESS 1
			#define _MATERIAL_AFFECTS_EMISSION 1
			#define ASE_SRP_VERSION 70301

			#pragma multi_compile_instancing
			
			#if defined(_MATERIAL_AFFECTS_METAL) || defined(_MATERIAL_AFFECTS_AO) || defined(_MATERIAL_AFFECTS_SMOOTHNESS)
			#define _MATERIAL_AFFECTS_MASKMAP 1
			#endif
			
			#if !defined(ASE_NEEDS_FRAG_RELATIVE_WORLD_POS)
			#define ASE_NEEDS_FRAG_RELATIVE_WORLD_POS
			#endif

			#define SHADERPASS SHADERPASS_DBUFFER_MESH
			#define DECALS_4RT

			#pragma vertex Vert
			#pragma fragment Frag

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"

			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
			#ifdef DEBUG_DISPLAY
				#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Debug/DebugDisplay.hlsl"
			#endif

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialUtilities.hlsl"
			#include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/Decal.hlsl"

			

			struct VertexInput
			{
				float3 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 tangentOS : TANGENT;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_Position;
				#if defined(ASE_NEEDS_FRAG_RELATIVE_WORLD_POS)
				float3 positionRWS : TEXCOORD0;
				#endif
				float3 normalWS : TEXCOORD1;
				float4 tangentWS : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float _DecalQuantity;
			float _DecalType;
			float _NormalIntensity;
			float _SmoothnessMultiplier;
			float _DrawOrder;
			float _DecalMeshDepthBias;
			CBUFFER_END

			sampler2D _dirt_decal_BaseColor;
			sampler2D _Dirt_Decal_Normal;
			sampler2D _dirt_decal_mask;


			
			void GetSurfaceData(SurfaceDescription surfaceDescription, FragInputs fragInputs, float3 V, PositionInputs posInput, out DecalSurfaceData surfaceData)
			{
				#if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR)
					float4x4 normalToWorld = UNITY_ACCESS_INSTANCED_PROP(Decal, _NormalToWorld);
					float fadeFactor = clamp(normalToWorld[0][3], 0.0f, 1.0f);
					float2 scale = float2(normalToWorld[3][0], normalToWorld[3][1]);
					float2 offset = float2(normalToWorld[3][2], normalToWorld[3][3]);
					fragInputs.texCoord0.xy = fragInputs.texCoord0.xy * scale + offset;
					fragInputs.texCoord1.xy = fragInputs.texCoord1.xy * scale + offset;
					fragInputs.texCoord2.xy = fragInputs.texCoord2.xy * scale + offset;
					fragInputs.texCoord3.xy = fragInputs.texCoord3.xy * scale + offset;
				#else
					float fadeFactor = 1.0;
				#endif

				ZERO_INITIALIZE(DecalSurfaceData, surfaceData);

				#if _MATERIAL_AFFECTS_EMISSION
					//surfaceData.emissive.rgb = surfaceDescription.Emission.rgb * fadeFactor;
				#endif

				#if _MATERIAL_AFFECTS_ALBEDO
					surfaceData.baseColor.xyz = surfaceDescription.Albedo;
					surfaceData.baseColor.w = surfaceDescription.AlphaAlbedo * fadeFactor;
					if(surfaceData.baseColor.w > 0)
					{
						surfaceData.HTileMask |= DBUFFERHTILEBIT_DIFFUSE;
					}
				#endif

				#if _MATERIAL_AFFECTS_NORMAL
					#if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR)
						surfaceData.normalWS.xyz = mul((float3x3)normalToWorld, surfaceDescription.Normal);
					#elif (SHADERPASS == SHADERPASS_DBUFFER_MESH) || (SHADERPASS == SHADERPASS_FORWARD_PREVIEW)
						surfaceData.normalWS.xyz = normalize(TransformTangentToWorld(surfaceDescription.Normal, fragInputs.tangentToWorld));
					#endif
						surfaceData.normalWS.w = surfaceDescription.AlphaNormal * fadeFactor;
						if(surfaceData.normalWS.w > 0)
						{
							surfaceData.HTileMask |= DBUFFERHTILEBIT_NORMAL;
						}
				#else
					#if (SHADERPASS == SHADERPASS_FORWARD_PREVIEW)
						surfaceData.normalWS.xyz = normalize(TransformTangentToWorld(float3(0.0, 0.0, 0.1), fragInputs.tangentToWorld));
					#endif
				#endif

				#if _MATERIAL_AFFECTS_MASKMAP
					surfaceData.mask.z = surfaceDescription.Smoothness;
					#ifdef DECALS_4RT
						surfaceData.mask.x = surfaceDescription.Metallic;
						surfaceData.mask.y = surfaceDescription.Occlusion;
					#endif

						surfaceData.mask.w = surfaceDescription.MAOSOpacity * fadeFactor;
					#ifdef DECALS_4RT
						surfaceData.MAOSBlend.x = surfaceDescription.MAOSOpacity * fadeFactor;
						surfaceData.MAOSBlend.y = surfaceDescription.MAOSOpacity * fadeFactor;
					#endif

					if (surfaceData.mask.w > 0)
					{
						surfaceData.HTileMask |= DBUFFERHTILEBIT_MASK;
					}
				#endif
			}

			VertexOutput Vert( VertexInput inputMesh  )
			{
				VertexOutput o;
				UNITY_SETUP_INSTANCE_ID(inputMesh);
				UNITY_TRANSFER_INSTANCE_ID(inputMesh, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				o.ase_texcoord3.xy = inputMesh.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord3.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				float3 defaultVertexValue = inputMesh.positionOS.xyz;
				#else
				float3 defaultVertexValue = float3( 0, 0, 0 );
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				inputMesh.positionOS.xyz = vertexValue;
				#else
				inputMesh.positionOS.xyz += vertexValue;
				#endif

				inputMesh.normalOS = inputMesh.normalOS;
				inputMesh.tangentOS = inputMesh.tangentOS;

				float3 positionRWS = TransformObjectToWorld(inputMesh.positionOS);
				float3 normalWS = TransformObjectToWorldNormal(inputMesh.normalOS);
				float4 tangentWS = float4(TransformObjectToWorldDir(inputMesh.tangentOS.xyz), inputMesh.tangentOS.w);

				o.positionCS = TransformWorldToHClip(positionRWS);
				#if defined(ASE_NEEDS_FRAG_RELATIVE_WORLD_POS)
				o.positionRWS = positionRWS;
				#endif
				o.normalWS = normalWS;
				o.tangentWS = tangentWS;

				#if (SHADERPASS == SHADERPASS_DBUFFER_MESH)
					#if defined(UNITY_REVERSED_Z)
						o.positionCS.z -= _DecalMeshDepthBias;
					#else
						o.positionCS.z += _DecalMeshDepthBias;
					#endif
				#endif
				return o;
			}

			void Frag( VertexOutput packedInput,
				#if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_DBUFFER_MESH)
					OUTPUT_DBUFFER(outDBuffer)
				#else
					out float4 outEmissive : SV_Target0
				#endif
				
				)
			{
				UNITY_SETUP_INSTANCE_ID( packedInput );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( packedInput );
				FragInputs input;
				ZERO_INITIALIZE(FragInputs, input);
				input.tangentToWorld = k_identity3x3;
				#if defined(ASE_NEEDS_FRAG_RELATIVE_WORLD_POS)
				float3 positionRWS = packedInput.positionRWS;
				input.positionRWS = positionRWS;
				#endif
				float3 normalWS = packedInput.normalWS;
				float4 tangentWS = packedInput.tangentWS;
				input.tangentToWorld = BuildTangentToWorld(tangentWS, normalWS);
				input.positionSS = packedInput.positionCS;

				float clipValue = 1.0;

				#if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR)
					float depth = LoadCameraDepth(input.positionSS.xy);
					PositionInputs posInput = GetPositionInput(input.positionSS.xy, _ScreenSize.zw, depth, UNITY_MATRIX_I_VP, UNITY_MATRIX_V);

					float3 positionDS = TransformWorldToObject(posInput.positionWS);
					positionDS = positionDS * float3(1.0, -1.0, 1.0) + float3(0.5, 0.5, 0.5);
					if (!(all(positionDS.xyz > 0.0f) && all(1.0f - positionDS.xyz > 0.0f)))
					{
						clipValue = -1.0;
						#ifndef SHADER_API_METAL
						clip(clipValue);
						#endif
					}

					input.texCoord0.xy = positionDS.xz;
					input.texCoord1.xy = positionDS.xz;
					input.texCoord2.xy = positionDS.xz;
					input.texCoord3.xy = positionDS.xz;

					float3 V = GetWorldSpaceNormalizeViewDir(posInput.positionWS);
				#else
					PositionInputs posInput = GetPositionInput(input.positionSS.xy, _ScreenSize.zw, input.positionSS.z, input.positionSS.w, input.positionRWS.xyz, uint2(0, 0));

					#if defined(ASE_NEEDS_FRAG_RELATIVE_WORLD_POS)
					float3 V = GetWorldSpaceNormalizeViewDir(input.positionRWS);
					#else
					float3 V = float3(1.0, 1.0, 1.0);
					#endif
				#endif

				SurfaceDescription surfaceDescription = (SurfaceDescription)0;
				float2 texCoord25 = packedInput.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
				// *** BEGIN Flipbook UV Animation vars ***
				// Total tiles of Flipbook Texture
				float fbtotaltiles27 = _DecalQuantity * _DecalQuantity;
				// Offsets for cols and rows of Flipbook Texture
				float fbcolsoffset27 = 1.0f / _DecalQuantity;
				float fbrowsoffset27 = 1.0f / _DecalQuantity;
				// Speed of animation
				float fbspeed27 = _Time[ 1 ] * 0.0;
				// UV Tiling (col and row offset)
				float2 fbtiling27 = float2(fbcolsoffset27, fbrowsoffset27);
				// UV Offset - calculate current tile linear index, and convert it to (X * coloffset, Y * rowoffset)
				// Calculate current tile linear index
				float fbcurrenttileindex27 = round( fmod( fbspeed27 + _DecalType, fbtotaltiles27) );
				fbcurrenttileindex27 += ( fbcurrenttileindex27 < 0) ? fbtotaltiles27 : 0;
				// Obtain Offset X coordinate from current tile linear index
				float fblinearindextox27 = round ( fmod ( fbcurrenttileindex27, _DecalQuantity ) );
				// Multiply Offset X by coloffset
				float fboffsetx27 = fblinearindextox27 * fbcolsoffset27;
				// Obtain Offset Y coordinate from current tile linear index
				float fblinearindextoy27 = round( fmod( ( fbcurrenttileindex27 - fblinearindextox27 ) / _DecalQuantity, _DecalQuantity ) );
				// Reverse Y to get tiles from Top to Bottom
				fblinearindextoy27 = (int)(_DecalQuantity-1) - fblinearindextoy27;
				// Multiply Offset Y by rowoffset
				float fboffsety27 = fblinearindextoy27 * fbrowsoffset27;
				// UV Offset
				float2 fboffset27 = float2(fboffsetx27, fboffsety27);
				// Flipbook UV
				half2 fbuv27 = texCoord25 * fbtiling27 + fboffset27;
				// *** END Flipbook UV Animation vars ***
				float4 tex2DNode17 = tex2D( _dirt_decal_BaseColor, fbuv27 );
				
				float3 unpack19 = UnpackNormalScale( tex2D( _Dirt_Decal_Normal, fbuv27 ), _NormalIntensity );
				unpack19.z = lerp( 1, unpack19.z, saturate(_NormalIntensity) );
				
				float4 tex2DNode18 = tex2D( _dirt_decal_mask, fbuv27 );
				
				surfaceDescription.Albedo = tex2DNode17.rgb;
				surfaceDescription.AlphaAlbedo = tex2DNode17.a;
				surfaceDescription.Normal = unpack19;
				surfaceDescription.AlphaNormal = tex2DNode17.a;
				surfaceDescription.Metallic = tex2DNode18.r;
				surfaceDescription.Occlusion = tex2DNode18.g;
				surfaceDescription.Smoothness = ( tex2DNode18.a * _SmoothnessMultiplier );
				surfaceDescription.MAOSOpacity = tex2DNode17.a;
				surfaceDescription.Emission = float3( 0, 0, 0 );

				DecalSurfaceData surfaceData;
				GetSurfaceData(surfaceDescription, input, V, posInput, surfaceData);


				#if ((SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_DBUFFER_MESH)) && defined(PLATFORM_SUPPORTS_BUFFER_ATOMICS_IN_PIXEL_SHADER)
					uint2 htileCoord = input.positionSS.xy / 8;
					int stride = (_ScreenSize.x + 7) / 8;
					uint mask = surfaceData.HTileMask;
					uint tileCoord1d = htileCoord.y * stride + htileCoord.x;
					#ifdef PLATFORM_SUPPORTS_WAVE_INTRINSICS
						uint minTileCoord1d = WaveActiveMin(tileCoord1d);
						while (minTileCoord1d != -1)
						{
							if ((minTileCoord1d == tileCoord1d) && (clipValue > 0.0))
							{
								mask = WaveActiveBitOr(surfaceData.HTileMask);

								if(WaveIsFirstLane())
								{
									if (tileCoord1d != -1)
									{
										tileCoord1d = htileCoord.y * stride + htileCoord.x;
									}
									InterlockedOr(_DecalPropertyMaskBuffer[tileCoord1d], mask);
								}
								tileCoord1d = -1;
							}
							if (tileCoord1d != -1)
							{
								tileCoord1d = htileCoord.y * stride + htileCoord.x;
							}
							minTileCoord1d = WaveActiveMin(tileCoord1d);
						}
					#else
						InterlockedOr(_DecalPropertyMaskBuffer[tileCoord1d], mask);
					#endif
				#endif

				#if ((SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR)) && defined(SHADER_API_METAL)
					clip(clipValue);
				#endif

				#if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_DBUFFER_MESH)
					ENCODE_INTO_DBUFFER(surfaceData, outDBuffer);
				#else
					outEmissive.rgb = surfaceData.emissive * GetCurrentExposureMultiplier();
					outEmissive.a = 1.0;
				#endif
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "ShaderGraph_MeshEmissive"
            Tags { "LightMode"="ShaderGraph_MeshEmissive" }

			Blend 0 SrcAlpha One
            ZTest LEqual
            ZWrite Off

			HLSLPROGRAM
			#define _MATERIAL_AFFECTS_ALBEDO 1
			#define _MATERIAL_AFFECTS_NORMAL 1
			#define _MATERIAL_AFFECTS_METAL 1
			#define _MATERIAL_AFFECTS_AO 1
			#define _MATERIAL_AFFECTS_SMOOTHNESS 1
			#define _MATERIAL_AFFECTS_EMISSION 1
			#define ASE_SRP_VERSION 70301

			#pragma multi_compile_instancing
			
			#if defined(_MATERIAL_AFFECTS_METAL) || defined(_MATERIAL_AFFECTS_AO) || defined(_MATERIAL_AFFECTS_SMOOTHNESS)
			#define _MATERIAL_AFFECTS_MASKMAP 1
			#endif

			#if !defined(ASE_NEEDS_FRAG_RELATIVE_WORLD_POS)
			#define ASE_NEEDS_FRAG_RELATIVE_WORLD_POS
			#endif

			#define SHADERPASS SHADERPASS_FORWARD_EMISSIVE_MESH

			#pragma vertex Vert
			#pragma fragment Frag

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"

			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
			#ifdef DEBUG_DISPLAY
				#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Debug/DebugDisplay.hlsl"
			#endif

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialUtilities.hlsl"
			#include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/Decal.hlsl"

			

			struct VertexInput
			{
				float3 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 tangentOS : TANGENT;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_Position;
				#if defined(ASE_NEEDS_FRAG_RELATIVE_WORLD_POS)
				float3 positionRWS : TEXCOORD0;
				#endif
				float3 normalWS : TEXCOORD1;
				float4 tangentWS : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float _DecalQuantity;
			float _DecalType;
			float _NormalIntensity;
			float _SmoothnessMultiplier;
			float _DrawOrder;
			float _DecalMeshDepthBias;
			CBUFFER_END

			sampler2D _dirt_decal_BaseColor;
			sampler2D _Dirt_Decal_Normal;
			sampler2D _dirt_decal_mask;


			
			void GetSurfaceData(SurfaceDescription surfaceDescription, FragInputs fragInputs, float3 V, PositionInputs posInput, out DecalSurfaceData surfaceData)
			{
				#if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR)
					float4x4 normalToWorld = UNITY_ACCESS_INSTANCED_PROP(Decal, _NormalToWorld);
					float fadeFactor = clamp(normalToWorld[0][3], 0.0f, 1.0f);
					float2 scale = float2(normalToWorld[3][0], normalToWorld[3][1]);
					float2 offset = float2(normalToWorld[3][2], normalToWorld[3][3]);
					fragInputs.texCoord0.xy = fragInputs.texCoord0.xy * scale + offset;
					fragInputs.texCoord1.xy = fragInputs.texCoord1.xy * scale + offset;
					fragInputs.texCoord2.xy = fragInputs.texCoord2.xy * scale + offset;
					fragInputs.texCoord3.xy = fragInputs.texCoord3.xy * scale + offset;
				#else
					float fadeFactor = 1.0;
				#endif

				ZERO_INITIALIZE(DecalSurfaceData, surfaceData);

				#if _MATERIAL_AFFECTS_EMISSION
					surfaceData.emissive.rgb = surfaceDescription.Emission.rgb * fadeFactor;
				#endif

				#if _MATERIAL_AFFECTS_ALBEDO
					surfaceData.baseColor.xyz = surfaceDescription.Albedo;
					surfaceData.baseColor.w = surfaceDescription.AlphaAlbedo * fadeFactor;
					if(surfaceData.baseColor.w > 0)
					{
						surfaceData.HTileMask |= DBUFFERHTILEBIT_DIFFUSE;
					}
				#endif

				#if _MATERIAL_AFFECTS_NORMAL
					#if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR)
						surfaceData.normalWS.xyz = mul((float3x3)normalToWorld, surfaceDescription.Normal);
					#elif (SHADERPASS == SHADERPASS_DBUFFER_MESH) || (SHADERPASS == SHADERPASS_FORWARD_PREVIEW)
						surfaceData.normalWS.xyz = normalize(TransformTangentToWorld(surfaceDescription.Normal, fragInputs.tangentToWorld));
					#endif
						surfaceData.normalWS.w = surfaceDescription.AlphaNormal * fadeFactor;
						if(surfaceData.normalWS.w > 0)
						{
							surfaceData.HTileMask |= DBUFFERHTILEBIT_NORMAL;
						}
				#else
					#if (SHADERPASS == SHADERPASS_FORWARD_PREVIEW)
						surfaceData.normalWS.xyz = normalize(TransformTangentToWorld(float3(0.0, 0.0, 0.1), fragInputs.tangentToWorld));
					#endif
				#endif

				#if _MATERIAL_AFFECTS_MASKMAP
					surfaceData.mask.z = surfaceDescription.Smoothness;
					#ifdef DECALS_4RT
						surfaceData.mask.x = surfaceDescription.Metallic;
						surfaceData.mask.y = surfaceDescription.Occlusion;
					#endif

						surfaceData.mask.w = surfaceDescription.MAOSOpacity * fadeFactor;
					#ifdef DECALS_4RT
						surfaceData.MAOSBlend.x = surfaceDescription.MAOSOpacity * fadeFactor;
						surfaceData.MAOSBlend.y = surfaceDescription.MAOSOpacity * fadeFactor;
					#endif

					if (surfaceData.mask.w > 0)
					{
						surfaceData.HTileMask |= DBUFFERHTILEBIT_MASK;
					}
				#endif
			}

			VertexOutput Vert( VertexInput inputMesh  )
			{
				VertexOutput o;
				UNITY_SETUP_INSTANCE_ID(inputMesh);
				UNITY_TRANSFER_INSTANCE_ID(inputMesh, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				o.ase_texcoord3.xy = inputMesh.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord3.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				float3 defaultVertexValue = inputMesh.positionOS.xyz;
				#else
				float3 defaultVertexValue = float3( 0, 0, 0 );
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				inputMesh.positionOS.xyz = vertexValue;
				#else
				inputMesh.positionOS.xyz += vertexValue;
				#endif

				inputMesh.normalOS = inputMesh.normalOS;
				inputMesh.tangentOS = inputMesh.tangentOS;

				float3 positionRWS = TransformObjectToWorld(inputMesh.positionOS);
				float3 normalWS = TransformObjectToWorldNormal(inputMesh.normalOS);
				float4 tangentWS = float4(TransformObjectToWorldDir(inputMesh.tangentOS.xyz), inputMesh.tangentOS.w);

				o.positionCS = TransformWorldToHClip(positionRWS);
				#if defined(ASE_NEEDS_FRAG_RELATIVE_WORLD_POS)
				o.positionRWS = positionRWS;
				#endif
				o.normalWS = normalWS;
				o.tangentWS = tangentWS;

				#if (SHADERPASS == SHADERPASS_DBUFFER_MESH) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_MESH)
					#if defined(UNITY_REVERSED_Z)
						o.positionCS.z -= _DecalMeshDepthBias;
					#else
						o.positionCS.z += _DecalMeshDepthBias;
					#endif
				#endif
				return o;
			}

			void Frag( VertexOutput packedInput,
				#if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_DBUFFER_MESH)
					OUTPUT_DBUFFER(outDBuffer)
				#else
					out float4 outEmissive : SV_Target0
				#endif
				
				)
			{
				UNITY_SETUP_INSTANCE_ID( packedInput );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( packedInput );
				FragInputs input;
				ZERO_INITIALIZE(FragInputs, input);
				input.tangentToWorld = k_identity3x3;
				#if defined(ASE_NEEDS_FRAG_RELATIVE_WORLD_POS)
				float3 positionRWS = packedInput.positionRWS;
				input.positionRWS = positionRWS;
				#endif
				float3 normalWS = packedInput.normalWS;
				float4 tangentWS = packedInput.tangentWS;
				input.tangentToWorld = BuildTangentToWorld(tangentWS, normalWS);
				input.positionSS = packedInput.positionCS;

				float clipValue = 1.0;

				#if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR)
					float depth = LoadCameraDepth(input.positionSS.xy);
					PositionInputs posInput = GetPositionInput(input.positionSS.xy, _ScreenSize.zw, depth, UNITY_MATRIX_I_VP, UNITY_MATRIX_V);

					float3 positionDS = TransformWorldToObject(posInput.positionWS);
					positionDS = positionDS * float3(1.0, -1.0, 1.0) + float3(0.5, 0.5, 0.5);
					if (!(all(positionDS.xyz > 0.0f) && all(1.0f - positionDS.xyz > 0.0f)))
					{
						clipValue = -1.0;
						#ifndef SHADER_API_METAL
						clip(clipValue);
						#endif
					}

					input.texCoord0.xy = positionDS.xz;
					input.texCoord1.xy = positionDS.xz;
					input.texCoord2.xy = positionDS.xz;
					input.texCoord3.xy = positionDS.xz;

					float3 V = GetWorldSpaceNormalizeViewDir(posInput.positionWS);
				#else
					PositionInputs posInput = GetPositionInput(input.positionSS.xy, _ScreenSize.zw, input.positionSS.z, input.positionSS.w, input.positionRWS.xyz, uint2(0, 0));

					#if defined(ASE_NEEDS_FRAG_RELATIVE_WORLD_POS)
					float3 V = GetWorldSpaceNormalizeViewDir(input.positionRWS);
					#else
					float3 V = float3(1.0, 1.0, 1.0);
					#endif
				#endif

				SurfaceDescription surfaceDescription = (SurfaceDescription)0;
				float2 texCoord25 = packedInput.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
				// *** BEGIN Flipbook UV Animation vars ***
				// Total tiles of Flipbook Texture
				float fbtotaltiles27 = _DecalQuantity * _DecalQuantity;
				// Offsets for cols and rows of Flipbook Texture
				float fbcolsoffset27 = 1.0f / _DecalQuantity;
				float fbrowsoffset27 = 1.0f / _DecalQuantity;
				// Speed of animation
				float fbspeed27 = _Time[ 1 ] * 0.0;
				// UV Tiling (col and row offset)
				float2 fbtiling27 = float2(fbcolsoffset27, fbrowsoffset27);
				// UV Offset - calculate current tile linear index, and convert it to (X * coloffset, Y * rowoffset)
				// Calculate current tile linear index
				float fbcurrenttileindex27 = round( fmod( fbspeed27 + _DecalType, fbtotaltiles27) );
				fbcurrenttileindex27 += ( fbcurrenttileindex27 < 0) ? fbtotaltiles27 : 0;
				// Obtain Offset X coordinate from current tile linear index
				float fblinearindextox27 = round ( fmod ( fbcurrenttileindex27, _DecalQuantity ) );
				// Multiply Offset X by coloffset
				float fboffsetx27 = fblinearindextox27 * fbcolsoffset27;
				// Obtain Offset Y coordinate from current tile linear index
				float fblinearindextoy27 = round( fmod( ( fbcurrenttileindex27 - fblinearindextox27 ) / _DecalQuantity, _DecalQuantity ) );
				// Reverse Y to get tiles from Top to Bottom
				fblinearindextoy27 = (int)(_DecalQuantity-1) - fblinearindextoy27;
				// Multiply Offset Y by rowoffset
				float fboffsety27 = fblinearindextoy27 * fbrowsoffset27;
				// UV Offset
				float2 fboffset27 = float2(fboffsetx27, fboffsety27);
				// Flipbook UV
				half2 fbuv27 = texCoord25 * fbtiling27 + fboffset27;
				// *** END Flipbook UV Animation vars ***
				float4 tex2DNode17 = tex2D( _dirt_decal_BaseColor, fbuv27 );
				
				float3 unpack19 = UnpackNormalScale( tex2D( _Dirt_Decal_Normal, fbuv27 ), _NormalIntensity );
				unpack19.z = lerp( 1, unpack19.z, saturate(_NormalIntensity) );
				
				float4 tex2DNode18 = tex2D( _dirt_decal_mask, fbuv27 );
				
				surfaceDescription.Albedo = tex2DNode17.rgb;
				surfaceDescription.AlphaAlbedo = tex2DNode17.a;
				surfaceDescription.Normal = unpack19;
				surfaceDescription.AlphaNormal = tex2DNode17.a;
				surfaceDescription.Metallic = tex2DNode18.r;
				surfaceDescription.Occlusion = tex2DNode18.g;
				surfaceDescription.Smoothness = ( tex2DNode18.a * _SmoothnessMultiplier );
				surfaceDescription.MAOSOpacity = tex2DNode17.a;
				surfaceDescription.Emission = float3( 0, 0, 0 );

				DecalSurfaceData surfaceData;
				GetSurfaceData(surfaceDescription, input, V, posInput, surfaceData);


				#if ((SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_DBUFFER_MESH)) && defined(PLATFORM_SUPPORTS_BUFFER_ATOMICS_IN_PIXEL_SHADER)
					uint2 htileCoord = input.positionSS.xy / 8;
					int stride = (_ScreenSize.x + 7) / 8;
					uint mask = surfaceData.HTileMask;
					uint tileCoord1d = htileCoord.y * stride + htileCoord.x;
					#ifdef PLATFORM_SUPPORTS_WAVE_INTRINSICS
						uint minTileCoord1d = WaveActiveMin(tileCoord1d);
						while (minTileCoord1d != -1)
						{
							if ((minTileCoord1d == tileCoord1d) && (clipValue > 0.0))
							{
								mask = WaveActiveBitOr(surfaceData.HTileMask);

								if(WaveIsFirstLane())
								{
									if (tileCoord1d != -1)
									{
										tileCoord1d = htileCoord.y * stride + htileCoord.x;
									}
									InterlockedOr(_DecalPropertyMaskBuffer[tileCoord1d], mask);
								}
								tileCoord1d = -1;
							}
							if (tileCoord1d != -1)
							{
								tileCoord1d = htileCoord.y * stride + htileCoord.x;
							}
							minTileCoord1d = WaveActiveMin(tileCoord1d);
						}
					#else
						InterlockedOr(_DecalPropertyMaskBuffer[tileCoord1d], mask);
					#endif
				#endif

				#if ((SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR)) && defined(SHADER_API_METAL)
					clip(clipValue);
				#endif

				#if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_DBUFFER_MESH)
					ENCODE_INTO_DBUFFER(surfaceData, outDBuffer);
				#else
					outEmissive.rgb = surfaceData.emissive * GetCurrentExposureMultiplier();
					outEmissive.a = 1.0;
				#endif
			}
			ENDHLSL
		}
		
	}
	CustomEditor "UnityEditor.Rendering.HighDefinition.DecalGUI"
    FallBack "Hidden/Shader Graph/FallbackError"
	
	
}
/*ASEBEGIN
Version=18710
456;81;1328;935;998.34;405.486;1;True;False
Node;AmplifyShaderEditor.CommentaryNode;30;-1837.536,-303.4446;Inherit;False;673;397;Decal flipbook, put all your decals in a single atlas to simplify their use.;4;25;29;28;27;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;29;-1698.536,-22.44458;Inherit;False;Property;_DecalType;Decal Type;6;0;Create;True;0;0;0;False;0;False;0;4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;25;-1787.536,-253.4446;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;28;-1749.536,-124.4446;Inherit;False;Property;_DecalQuantity;Decal Quantity;5;0;Create;True;0;0;0;False;0;False;0;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCFlipBookUVAnimation;27;-1417.536,-217.4446;Inherit;False;0;0;6;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;24;-930.5359,-122.4446;Inherit;False;Property;_NormalIntensity;Normal Intensity;4;0;Create;True;0;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;18;-719.5359,6.555405;Inherit;True;Property;_dirt_decal_mask;dirt_decal_mask;1;0;Create;True;0;0;0;False;0;False;-1;None;5eb344b0d2df6894484b5d44dbd0fc33;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;20;-730.5641,306.9949;Inherit;False;Property;_SmoothnessMultiplier;Smoothness Multiplier;3;0;Create;True;0;0;0;False;0;False;0;0.541;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;-276.7078,174.2343;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;17;-717.5359,-407.4446;Inherit;True;Property;_dirt_decal_BaseColor;dirt_decal_BaseColor;0;0;Create;True;0;0;0;False;0;False;-1;39445ba53e51aa64db46d5293b29da39;39445ba53e51aa64db46d5293b29da39;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;19;-705.5359,-210.4446;Inherit;True;Property;_Dirt_Decal_Normal;Dirt_Decal_Normal;2;0;Create;True;0;0;0;False;0;False;-1;bca2297e6d257934b865d220d15e2689;bca2297e6d257934b865d220d15e2689;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;11;0,0;Float;False;False;-1;2;UnityEditor.Rendering.HighDefinition.DecalGUI;0;1;New Amplify Shader;d345501910c196f4a81c9eff8a0a5ad7;True;ShaderGraph_DBufferProjector3RT;0;0;ShaderGraph_DBufferProjector3RT;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=HDRenderPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;5;0;True;2;5;False;-1;10;False;-1;1;0;False;-1;10;False;-1;False;True;2;5;False;-1;10;False;-1;1;0;False;-1;10;False;-1;False;True;2;5;False;-1;10;False;-1;1;0;False;-1;10;False;-1;False;False;False;False;True;1;False;-1;False;False;True;False;False;True;True;0;False;-1;True;False;False;False;False;0;False;-1;True;True;16;False;-1;255;False;-1;16;False;-1;7;False;-1;3;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;2;False;-1;True;2;False;-1;False;True;1;LightMode=ShaderGraph_DBufferProjector3RT;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;13;0,0;Float;False;False;-1;2;UnityEditor.Rendering.HighDefinition.DecalGUI;0;1;New Amplify Shader;d345501910c196f4a81c9eff8a0a5ad7;True;ShaderGraph_ProjectorEmissive;0;2;ShaderGraph_ProjectorEmissive;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=HDRenderPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;5;0;True;8;5;False;-1;1;False;-1;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;True;1;False;-1;False;False;False;False;False;True;2;False;-1;True;2;False;-1;False;True;1;LightMode=ShaderGraph_ProjectorEmissive;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;12;0,0;Float;False;False;-1;2;UnityEditor.Rendering.HighDefinition.DecalGUI;0;1;New Amplify Shader;d345501910c196f4a81c9eff8a0a5ad7;True;ShaderGraph_DBufferProjector4RT;0;1;ShaderGraph_DBufferProjector4RT;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=HDRenderPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;5;0;True;2;5;False;-1;10;False;-1;1;0;False;-1;10;False;-1;False;True;2;5;False;-1;10;False;-1;1;0;False;-1;10;False;-1;False;True;2;5;False;-1;10;False;-1;1;0;False;-1;10;False;-1;False;True;1;0;False;-1;6;False;-1;0;1;False;-1;0;False;-1;False;False;True;1;False;-1;False;False;True;True;True;True;True;0;False;-1;True;True;True;False;False;0;False;-1;True;True;16;False;-1;255;False;-1;16;False;-1;7;False;-1;3;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;2;False;-1;True;2;False;-1;False;True;1;LightMode=ShaderGraph_DBufferProjector4RT;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;15;-13,-105;Float;False;True;-1;2;UnityEditor.Rendering.HighDefinition.DecalGUI;0;10;ASESampleShaders/Decals Muddy Ground/GroundDecals;d345501910c196f4a81c9eff8a0a5ad7;True;ShaderGraph_DBufferMesh4RT;0;4;ShaderGraph_DBufferMesh4RT;12;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=HDRenderPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;5;0;True;2;5;False;-1;10;False;-1;1;0;False;-1;10;False;-1;False;True;2;5;False;-1;10;False;-1;1;0;False;-1;10;False;-1;False;True;2;5;False;-1;10;False;-1;1;0;False;-1;10;False;-1;False;True;1;0;False;-1;6;False;-1;0;1;False;-1;0;False;-1;False;False;False;False;False;True;True;True;True;True;0;False;-1;True;True;True;False;False;0;False;-1;True;True;16;False;-1;255;False;-1;16;False;-1;7;False;-1;3;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;2;False;-1;True;3;False;-1;False;True;1;LightMode=ShaderGraph_DBufferMesh4RT;False;0;;0;0;Standard;7;Affect BaseColor;1;Affect Normal;1;Affect Metal;1;Affect AO;1;Affect Smoothness;1;Affect Emission;1;Vertex Position,InvertActionOnDeselection;1;0;6;True;True;True;True;True;True;False;;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;16;0,0;Float;False;False;-1;2;UnityEditor.Rendering.HighDefinition.DecalGUI;0;1;New Amplify Shader;d345501910c196f4a81c9eff8a0a5ad7;True;ShaderGraph_MeshEmissive;0;5;ShaderGraph_MeshEmissive;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=HDRenderPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;5;0;True;8;5;False;-1;1;False;-1;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;-1;True;3;False;-1;False;True;1;LightMode=ShaderGraph_MeshEmissive;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;14;0,0;Float;False;False;-1;2;UnityEditor.Rendering.HighDefinition.DecalGUI;0;1;New Amplify Shader;d345501910c196f4a81c9eff8a0a5ad7;True;ShaderGraph_DBufferMesh3RT;0;3;ShaderGraph_DBufferMesh3RT;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=HDRenderPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;5;0;True;2;5;False;-1;10;False;-1;1;0;False;-1;10;False;-1;False;True;2;5;False;-1;10;False;-1;1;0;False;-1;10;False;-1;False;True;2;5;False;-1;10;False;-1;1;0;False;-1;10;False;-1;False;False;False;False;False;False;False;True;False;False;True;True;0;False;-1;True;False;False;False;False;0;False;-1;True;True;16;False;-1;255;False;-1;16;False;-1;7;False;-1;3;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;2;False;-1;True;3;False;-1;False;True;1;LightMode=ShaderGraph_DBufferMesh3RT;False;0;;0;0;Standard;0;False;0
WireConnection;27;0;25;0
WireConnection;27;1;28;0
WireConnection;27;2;28;0
WireConnection;27;4;29;0
WireConnection;18;1;27;0
WireConnection;21;0;18;4
WireConnection;21;1;20;0
WireConnection;17;1;27;0
WireConnection;19;1;27;0
WireConnection;19;5;24;0
WireConnection;15;0;17;0
WireConnection;15;1;17;4
WireConnection;15;2;19;0
WireConnection;15;3;17;4
WireConnection;15;4;18;1
WireConnection;15;5;18;2
WireConnection;15;6;21;0
WireConnection;15;7;17;4
ASEEND*/
//CHKSM=DA252995E0FFB530E9EFBDAA12383316C61D6F6F