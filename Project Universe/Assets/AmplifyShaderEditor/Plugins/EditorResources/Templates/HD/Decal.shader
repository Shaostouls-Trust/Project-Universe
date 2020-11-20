Shader /*ase_name*/ "Hidden/HD/Decal" /*end*/
{
	Properties
	{
		/*ase_props*/
		[HideInInspector]_DrawOrder("Draw Order", Int) = 0
		[HideInInspector]_DecalMeshDepthBias("DecalMesh DepthBias", Float) = 0
	}

	SubShader
	{
		/*ase_subshader_options:Name=Additional Options
			Option:Affect BaseColor:false,true:true
				true:SetDefine:_MATERIAL_AFFECTS_ALBEDO 1
				false:RemoveDefine:_MATERIAL_AFFECTS_ALBEDO 1
			Option:Affect Normal:false,true:true
				true:SetDefine:_MATERIAL_AFFECTS_NORMAL 1
				false:RemoveDefine:_MATERIAL_AFFECTS_NORMAL 1
			Option:Affect Metal:false,true:true
				true:SetDefine:_MATERIAL_AFFECTS_METAL 1
				true:SetPropertyOnPass:ShaderGraph_DBufferMesh4RT:ColorMask2,true,,,
				true:SetPropertyOnPass:ShaderGraph_DBufferMesh4RT:ColorMask3,true,,,
				true:SetPropertyOnPass:ShaderGraph_DBufferProjector4RT:ColorMask2,true,,,
				true:SetPropertyOnPass:ShaderGraph_DBufferProjector4RT:ColorMask3,true,,,
				false:RemoveDefine:_MATERIAL_AFFECTS_METAL 1
				false:SetPropertyOnPass:ShaderGraph_DBufferMesh4RT:ColorMask2,false,,,
				false:SetPropertyOnPass:ShaderGraph_DBufferMesh4RT:ColorMask3,false,,,
				false:SetPropertyOnPass:ShaderGraph_DBufferProjector4RT:ColorMask2,false,,,
				false:SetPropertyOnPass:ShaderGraph_DBufferProjector4RT:ColorMask3,false,,,
			Option:Affect AO:false,true:true
				true:SetDefine:_MATERIAL_AFFECTS_AO 1
				true:SetPropertyOnPass:ShaderGraph_DBufferMesh4RT:ColorMask2,,true,,
				true:SetPropertyOnPass:ShaderGraph_DBufferMesh4RT:ColorMask3,,true,,
				true:SetPropertyOnPass:ShaderGraph_DBufferProjector4RT:ColorMask2,,true,,
				true:SetPropertyOnPass:ShaderGraph_DBufferProjector4RT:ColorMask3,,true,,
				false:RemoveDefine:_MATERIAL_AFFECTS_AO 1
				false:SetPropertyOnPass:ShaderGraph_DBufferMesh4RT:ColorMask2,,false,,
				false:SetPropertyOnPass:ShaderGraph_DBufferMesh4RT:ColorMask3,,false,,
				false:SetPropertyOnPass:ShaderGraph_DBufferProjector4RT:ColorMask2,,false,,
				false:SetPropertyOnPass:ShaderGraph_DBufferProjector4RT:ColorMask3,,false,,
			Option:Affect Smoothness:false,true:true
				true:SetDefine:_MATERIAL_AFFECTS_SMOOTHNESS 1
				true:SetPropertyOnPass:ShaderGraph_DBufferMesh4RT:ColorMask2,,,true,true
				true:SetPropertyOnPass:ShaderGraph_DBufferProjector4RT:ColorMask2,,,true,true
				false:RemoveDefine:_MATERIAL_AFFECTS_SMOOTHNESS 1
				false:SetPropertyOnPass:ShaderGraph_DBufferMesh4RT:ColorMask2,,,false,false
				false:SetPropertyOnPass:ShaderGraph_DBufferProjector4RT:ColorMask2,,,false,false
			Option:Affect Emission:false,true:true
				true:SetDefine:_MATERIAL_AFFECTS_EMISSION 1
				true:IncludePass:ShaderGraph_ProjectorEmissive
				true:IncludePass:ShaderGraph_MeshEmissive
				false:RemoveDefine:_MATERIAL_AFFECTS_EMISSION 1
				false:ExcludePass:ShaderGraph_ProjectorEmissive
				false:ExcludePass:ShaderGraph_MeshEmissive
			Option:Vertex Position,InvertActionOnDeselection:Absolute,Relative:Relative
				Absolute:SetDefine:ASE_ABSOLUTE_VERTEX_POS 1
				Absolute:SetPortName:ShaderGraph_DBufferMesh4RT:9,Vertex Position
				Relative:SetPortName:ShaderGraph_DBufferMesh4RT:9,Vertex Offset
		*/
		Tags
		{
			"RenderPipeline"="HDRenderPipeline"
			"RenderType"="Opaque"
			"Queue" = "Geometry+0"
		}

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

		/*ase_pass*/
		Pass
		{
			/*ase_hide_pass*/
			Name "ShaderGraph_DBufferProjector3RT"
            Tags { "LightMode" = "ShaderGraph_DBufferProjector3RT" }
        
            Blend 0 SrcAlpha OneMinusSrcAlpha, Zero OneMinusSrcAlpha Blend 1 SrcAlpha OneMinusSrcAlpha, Zero OneMinusSrcAlpha Blend 2 SrcAlpha OneMinusSrcAlpha, Zero OneMinusSrcAlpha
            Cull Front
            ZTest Greater
            ZWrite Off

			Stencil
			{
				WriteMask 16
				Ref  16
				Comp Always
				Pass Replace
			}
        
            ColorMask BA 2 ColorMask 0 3

			HLSLPROGRAM
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

			/*ase_pragma*/

			struct VertexInput
			{
				float3 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 tangentOS : TANGENT;
				/*ase_vdata:p=p;n=n;t=t*/
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_Position;
				#if defined(ASE_NEEDS_FRAG_RELATIVE_WORLD_POS)
				float3 positionRWS : TEXCOORD0;
				#endif
				/*ase_interp(1,):sp=sp.xyzw;rwp=tc0*/
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float _DrawOrder;
			float _DecalMeshDepthBias;
			CBUFFER_END

			/*ase_globals*/

			/*ase_funcs*/

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

			VertexOutput Vert( VertexInput inputMesh /*ase_vert_input*/ )
			{
				VertexOutput o;
				UNITY_SETUP_INSTANCE_ID(inputMesh);
				UNITY_TRANSFER_INSTANCE_ID(inputMesh, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				/*ase_vert_code:inputMesh=VertexInput;o=VertexOutput*/
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				float3 defaultVertexValue = inputMesh.positionOS.xyz;
				#else
				float3 defaultVertexValue = float3( 0, 0, 0 );
				#endif
				float3 vertexValue = /*ase_vert_out:Vertex Offset;Float3;9;-1;_VertexOffset*/defaultVertexValue/*end*/;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				inputMesh.positionOS.xyz = vertexValue;
				#else
				inputMesh.positionOS.xyz += vertexValue;
				#endif

				inputMesh.normalOS = /*ase_vert_out:Vertex Normal;Float3;10;-1;_VertexNormal*/inputMesh.normalOS/*end*/;
				inputMesh.tangentOS = /*ase_vert_out:Vertex Tangent;Float4;11;-1;_VertexTangent*/inputMesh.tangentOS/*end*/;

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
				/*ase_frag_input*/
				)
			{
				UNITY_SETUP_INSTANCE_ID( packedInput );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( packedInput );
				FragInputs input;
				ZERO_INITIALIZE(FragInputs, input);
				input.tangentToWorld = k_identity3x3;
				#if defined(ASE_NEEDS_FRAG_RELATIVE_WORLD_POS)
				/*ase_local_var:rwp*/float3 positionRWS = packedInput.positionRWS;
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

				/*ase_local_var:uv0*/float4 texCoord0 = input.texCoord0;
				/*ase_local_var:uv1*/float4 texCoord1 = input.texCoord1;
				/*ase_local_var:uv2*/float4 texCoord2 = input.texCoord2;
				/*ase_local_var:uv3*/float4 texCoord3 = input.texCoord3;

				SurfaceDescription surfaceDescription = (SurfaceDescription)0;
				/*ase_frag_code:packedInput=VertexOutput*/
				surfaceDescription.Albedo = /*ase_frag_out:Albedo;Float3;0;-1;_Albedo*/float3( 0.7353569, 0.7353569, 0.7353569 )/*end*/;
				surfaceDescription.AlphaAlbedo = /*ase_frag_out:Alpha Albedo;Float;1;-1;_AlphaAlbedo*/1/*end*/;
				surfaceDescription.Normal = /*ase_frag_out:Normal;Float3;2;-1;_Normal*/float3( 0, 0, 1 )/*end*/;
				surfaceDescription.AlphaNormal = /*ase_frag_out:Alpha Normal;Float;3;-1;_AlphaNormal*/1/*end*/;
				surfaceDescription.Metallic = /*ase_frag_out:Metallic;Float;4;-1;_Metallic*/0/*end*/;
				surfaceDescription.Occlusion = /*ase_frag_out:Occlusion;Float;5;-1;_Occlusion*/1/*end*/;
				surfaceDescription.Smoothness = /*ase_frag_out:Smoothness;Float;6;-1;_Smoothness*/0.5/*end*/;
				surfaceDescription.MAOSOpacity = /*ase_frag_out:MAOSOpacity;Float;7;-1;_MAOSOpacity*/1/*end*/;
				surfaceDescription.Emission = /*ase_frag_out:Emission;Float3;8;-1;_Emission*/float3( 0, 0, 0 )/*end*/;

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

		/*ase_pass*/
		Pass
		{
			/*ase_hide_pass*/
			Name "ShaderGraph_DBufferProjector4RT"
            Tags { "LightMode" = "ShaderGraph_DBufferProjector4RT" }
        
            Blend 0 SrcAlpha OneMinusSrcAlpha, Zero OneMinusSrcAlpha Blend 1 SrcAlpha OneMinusSrcAlpha, Zero OneMinusSrcAlpha Blend 2 SrcAlpha OneMinusSrcAlpha, Zero OneMinusSrcAlpha Blend 3 Zero OneMinusSrcColor
            Cull Front
            ZTest Greater
            ZWrite Off
            
			Stencil
			{
				WriteMask 16
				Ref  16
				Comp Always
				Pass Replace
			}
        
            ColorMask RGBA 2 ColorMask RG 3

			HLSLPROGRAM
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

			/*ase_pragma*/

			struct VertexInput
			{
				float3 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 tangentOS : TANGENT;
				/*ase_vdata:p=p;n=n;t=t*/
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_Position;
				#if defined(ASE_NEEDS_FRAG_RELATIVE_WORLD_POS)
				float3 positionRWS : TEXCOORD0;
				#endif
				/*ase_interp(1,):sp=sp.xyzw;rwp=tc0*/
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float _DrawOrder;
			float _DecalMeshDepthBias;
			CBUFFER_END

			/*ase_globals*/

			/*ase_funcs*/

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

			VertexOutput Vert( VertexInput inputMesh /*ase_vert_input*/ )
			{
				VertexOutput o;
				UNITY_SETUP_INSTANCE_ID(inputMesh);
				UNITY_TRANSFER_INSTANCE_ID(inputMesh, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				/*ase_vert_code:inputMesh=VertexInput;o=VertexOutput*/
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				float3 defaultVertexValue = inputMesh.positionOS.xyz;
				#else
				float3 defaultVertexValue = float3( 0, 0, 0 );
				#endif
				float3 vertexValue = /*ase_vert_out:Vertex Offset;Float3;9;-1;_VertexOffset*/defaultVertexValue/*end*/;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				inputMesh.positionOS.xyz = vertexValue;
				#else
				inputMesh.positionOS.xyz += vertexValue;
				#endif

				inputMesh.normalOS = /*ase_vert_out:Vertex Normal;Float3;10;-1;_VertexNormal*/inputMesh.normalOS/*end*/;
				inputMesh.tangentOS = /*ase_vert_out:Vertex Tangent;Float4;11;-1;_VertexTangent*/inputMesh.tangentOS/*end*/;

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
				/*ase_frag_input*/
				)
			{
				UNITY_SETUP_INSTANCE_ID( packedInput );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( packedInput );
				FragInputs input;
				ZERO_INITIALIZE(FragInputs, input);
				input.tangentToWorld = k_identity3x3;
				#if defined(ASE_NEEDS_FRAG_RELATIVE_WORLD_POS)
				/*ase_local_var:rwp*/float3 positionRWS = packedInput.positionRWS;
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

				/*ase_local_var:uv0*/float4 texCoord0 = input.texCoord0;
				/*ase_local_var:uv1*/float4 texCoord1 = input.texCoord1;
				/*ase_local_var:uv2*/float4 texCoord2 = input.texCoord2;
				/*ase_local_var:uv3*/float4 texCoord3 = input.texCoord3;

				SurfaceDescription surfaceDescription = (SurfaceDescription)0;
				/*ase_frag_code:packedInput=VertexOutput*/
				surfaceDescription.Albedo = /*ase_frag_out:Albedo;Float3;0;-1;_Albedo*/float3( 0.7353569, 0.7353569, 0.7353569 )/*end*/;
				surfaceDescription.AlphaAlbedo = /*ase_frag_out:Alpha Albedo;Float;1;-1;_AlphaAlbedo*/1/*end*/;
				surfaceDescription.Normal = /*ase_frag_out:Normal;Float3;2;-1;_Normal*/float3( 0, 0, 1 )/*end*/;
				surfaceDescription.AlphaNormal = /*ase_frag_out:Alpha Normal;Float;3;-1;_AlphaNormal*/1/*end*/;
				surfaceDescription.Metallic = /*ase_frag_out:Metallic;Float;4;-1;_Metallic*/0/*end*/;
				surfaceDescription.Occlusion = /*ase_frag_out:Occlusion;Float;5;-1;_Occlusion*/1/*end*/;
				surfaceDescription.Smoothness = /*ase_frag_out:Smoothness;Float;6;-1;_Smoothness*/0.5/*end*/;
				surfaceDescription.MAOSOpacity = /*ase_frag_out:MAOSOpacity;Float;7;-1;_MAOSOpacity*/1/*end*/;
				surfaceDescription.Emission = /*ase_frag_out:Emission;Float3;8;-1;_Emission*/float3( 0, 0, 0 )/*end*/;

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

		/*ase_pass*/
		Pass
		{
			/*ase_hide_pass*/
			Name "ShaderGraph_ProjectorEmissive"
            Tags { "LightMode" = "ShaderGraph_ProjectorEmissive" }
        
            Blend 0 SrcAlpha One
            Cull Front
            ZTest Greater
            ZWrite Off

			HLSLPROGRAM
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

			/*ase_pragma*/

			struct VertexInput
			{
				float3 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 tangentOS : TANGENT;
				/*ase_vdata:p=p;n=n;t=t*/
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_Position;
				#if defined(ASE_NEEDS_FRAG_RELATIVE_WORLD_POS)
				float3 positionRWS : TEXCOORD0;
				#endif
				/*ase_interp(1,):sp=sp.xyzw;rwp=tc0*/
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float _DrawOrder;
			float _DecalMeshDepthBias;
			CBUFFER_END

			/*ase_globals*/

			/*ase_funcs*/

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

			VertexOutput Vert( VertexInput inputMesh /*ase_vert_input*/ )
			{
				VertexOutput o;
				UNITY_SETUP_INSTANCE_ID(inputMesh);
				UNITY_TRANSFER_INSTANCE_ID(inputMesh, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				/*ase_vert_code:inputMesh=VertexInput;o=VertexOutput*/
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				float3 defaultVertexValue = inputMesh.positionOS.xyz;
				#else
				float3 defaultVertexValue = float3( 0, 0, 0 );
				#endif
				float3 vertexValue = /*ase_vert_out:Vertex Offset;Float3;9;-1;_VertexOffset*/defaultVertexValue/*end*/;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				inputMesh.positionOS.xyz = vertexValue;
				#else
				inputMesh.positionOS.xyz += vertexValue;
				#endif

				inputMesh.normalOS = /*ase_vert_out:Vertex Normal;Float3;10;-1;_VertexNormal*/inputMesh.normalOS/*end*/;
				inputMesh.tangentOS = /*ase_vert_out:Vertex Tangent;Float4;11;-1;_VertexTangent*/inputMesh.tangentOS/*end*/;

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
				/*ase_frag_input*/
				)
			{
				UNITY_SETUP_INSTANCE_ID( packedInput );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( packedInput );
				FragInputs input;
				ZERO_INITIALIZE(FragInputs, input);
				input.tangentToWorld = k_identity3x3;
				#if defined(ASE_NEEDS_FRAG_RELATIVE_WORLD_POS)
				/*ase_local_var:rwp*/float3 positionRWS = packedInput.positionRWS;
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

				/*ase_local_var:uv0*/float4 texCoord0 = input.texCoord0;
				/*ase_local_var:uv1*/float4 texCoord1 = input.texCoord1;
				/*ase_local_var:uv2*/float4 texCoord2 = input.texCoord2;
				/*ase_local_var:uv3*/float4 texCoord3 = input.texCoord3;

				SurfaceDescription surfaceDescription = (SurfaceDescription)0;
				/*ase_frag_code:packedInput=VertexOutput*/
				surfaceDescription.Albedo = /*ase_frag_out:Albedo;Float3;0;-1;_Albedo*/float3( 0.7353569, 0.7353569, 0.7353569 )/*end*/;
				surfaceDescription.AlphaAlbedo = /*ase_frag_out:Alpha Albedo;Float;1;-1;_AlphaAlbedo*/1/*end*/;
				surfaceDescription.Normal = /*ase_frag_out:Normal;Float3;2;-1;_Normal*/float3( 0, 0, 1 )/*end*/;
				surfaceDescription.AlphaNormal = /*ase_frag_out:Alpha Normal;Float;3;-1;_AlphaNormal*/1/*end*/;
				surfaceDescription.Metallic = /*ase_frag_out:Metallic;Float;4;-1;_Metallic*/0/*end*/;
				surfaceDescription.Occlusion = /*ase_frag_out:Occlusion;Float;5;-1;_Occlusion*/1/*end*/;
				surfaceDescription.Smoothness = /*ase_frag_out:Smoothness;Float;6;-1;_Smoothness*/0.5/*end*/;
				surfaceDescription.MAOSOpacity = /*ase_frag_out:MAOSOpacity;Float;7;-1;_MAOSOpacity*/1/*end*/;
				surfaceDescription.Emission = /*ase_frag_out:Emission;Float3;8;-1;_Emission*/float3( 0, 0, 0 )/*end*/;

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

		/*ase_pass*/
		Pass
		{
			/*ase_hide_pass*/
			Name "ShaderGraph_DBufferMesh3RT"
            Tags { "LightMode" = "ShaderGraph_DBufferMesh3RT" }

			Blend 0 SrcAlpha OneMinusSrcAlpha, Zero OneMinusSrcAlpha Blend 1 SrcAlpha OneMinusSrcAlpha, Zero OneMinusSrcAlpha Blend 2 SrcAlpha OneMinusSrcAlpha, Zero OneMinusSrcAlpha
        
            ZTest LEqual
            ZWrite Off

			Stencil
			{
				WriteMask 16
				Ref  16
				Comp Always
				Pass Replace
			}
        
            ColorMask BA 2 ColorMask 0 3

			HLSLPROGRAM
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

			/*ase_pragma*/

			struct VertexInput
			{
				float3 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 tangentOS : TANGENT;
				/*ase_vdata:p=p;n=n;t=t*/
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
				/*ase_interp(3,):sp=sp.xyzw;rwp=tc0;wn=tc1;wt=tc2*/
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float _DrawOrder;
			float _DecalMeshDepthBias;
			CBUFFER_END

			/*ase_globals*/

			/*ase_funcs*/

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

			VertexOutput Vert( VertexInput inputMesh /*ase_vert_input*/ )
			{
				VertexOutput o;
				UNITY_SETUP_INSTANCE_ID(inputMesh);
				UNITY_TRANSFER_INSTANCE_ID(inputMesh, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				/*ase_vert_code:inputMesh=VertexInput;o=VertexOutput*/
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				float3 defaultVertexValue = inputMesh.positionOS.xyz;
				#else
				float3 defaultVertexValue = float3( 0, 0, 0 );
				#endif
				float3 vertexValue = /*ase_vert_out:Vertex Offset;Float3;9;-1;_VertexOffset*/defaultVertexValue/*end*/;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				inputMesh.positionOS.xyz = vertexValue;
				#else
				inputMesh.positionOS.xyz += vertexValue;
				#endif

				inputMesh.normalOS = /*ase_vert_out:Vertex Normal;Float3;10;-1;_VertexNormal*/inputMesh.normalOS/*end*/;
				inputMesh.tangentOS = /*ase_vert_out:Vertex Tangent;Float4;11;-1;_VertexTangent*/inputMesh.tangentOS/*end*/;

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
				/*ase_frag_input*/
				)
			{
				UNITY_SETUP_INSTANCE_ID( packedInput );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( packedInput );
				FragInputs input;
				ZERO_INITIALIZE(FragInputs, input);
				input.tangentToWorld = k_identity3x3;
				#if defined(ASE_NEEDS_FRAG_RELATIVE_WORLD_POS)
				/*ase_local_var:rwp*/float3 positionRWS = packedInput.positionRWS;
				input.positionRWS = positionRWS;
				#endif
				/*ase_local_var:wn*/float3 normalWS = packedInput.normalWS;
				/*ase_local_var:wt*/float4 tangentWS = packedInput.tangentWS;
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
				/*ase_frag_code:packedInput=VertexOutput*/
				surfaceDescription.Albedo = /*ase_frag_out:Albedo;Float3;0;-1;_Albedo*/float3( 0.7353569, 0.7353569, 0.7353569 )/*end*/;
				surfaceDescription.AlphaAlbedo = /*ase_frag_out:Alpha Albedo;Float;1;-1;_AlphaAlbedo*/1/*end*/;
				surfaceDescription.Normal = /*ase_frag_out:Normal;Float3;2;-1;_Normal*/float3( 0, 0, 1 )/*end*/;
				surfaceDescription.AlphaNormal = /*ase_frag_out:Alpha Normal;Float;3;-1;_AlphaNormal*/1/*end*/;
				surfaceDescription.Metallic = /*ase_frag_out:Metallic;Float;4;-1;_Metallic*/0/*end*/;
				surfaceDescription.Occlusion = /*ase_frag_out:Occlusion;Float;5;-1;_Occlusion*/1/*end*/;
				surfaceDescription.Smoothness = /*ase_frag_out:Smoothness;Float;6;-1;_Smoothness*/0.5/*end*/;
				surfaceDescription.MAOSOpacity = /*ase_frag_out:MAOSOpacity;Float;7;-1;_MAOSOpacity*/1/*end*/;
				surfaceDescription.Emission = /*ase_frag_out:Emission;Float3;8;-1;_Emission*/float3( 0, 0, 0 )/*end*/;

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

		/*ase_pass*/
		Pass
		{
			/*ase_main_pass*/
			Name "ShaderGraph_DBufferMesh4RT"
			Tags { "LightMode" = "ShaderGraph_DBufferMesh4RT" }

			Blend 0 SrcAlpha OneMinusSrcAlpha, Zero OneMinusSrcAlpha Blend 1 SrcAlpha OneMinusSrcAlpha, Zero OneMinusSrcAlpha Blend 2 SrcAlpha OneMinusSrcAlpha, Zero OneMinusSrcAlpha Blend 3 Zero OneMinusSrcColor

			ZTest LEqual
			ZWrite Off

			Stencil
			{
				WriteMask 16
				Ref  16
				Comp Always
				Pass Replace
			}

			ColorMask RGBA 2 ColorMask RG 3

			HLSLPROGRAM
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

			/*ase_pragma*/

			struct VertexInput
			{
				float3 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 tangentOS : TANGENT;
				/*ase_vdata:p=p;n=n;t=t*/
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
				/*ase_interp(3,):sp=sp.xyzw;rwp=tc0;wn=tc1;wt=tc2*/
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float _DrawOrder;
			float _DecalMeshDepthBias;
			CBUFFER_END

			/*ase_globals*/

			/*ase_funcs*/

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

			VertexOutput Vert( VertexInput inputMesh /*ase_vert_input*/ )
			{
				VertexOutput o;
				UNITY_SETUP_INSTANCE_ID(inputMesh);
				UNITY_TRANSFER_INSTANCE_ID(inputMesh, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				/*ase_vert_code:inputMesh=VertexInput;o=VertexOutput*/
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				float3 defaultVertexValue = inputMesh.positionOS.xyz;
				#else
				float3 defaultVertexValue = float3( 0, 0, 0 );
				#endif
				float3 vertexValue = /*ase_vert_out:Vertex Offset;Float3;9;-1;_VertexOffset*/defaultVertexValue/*end*/;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				inputMesh.positionOS.xyz = vertexValue;
				#else
				inputMesh.positionOS.xyz += vertexValue;
				#endif

				inputMesh.normalOS = /*ase_vert_out:Vertex Normal;Float3;10;-1;_VertexNormal*/inputMesh.normalOS/*end*/;
				inputMesh.tangentOS = /*ase_vert_out:Vertex Tangent;Float4;11;-1;_VertexTangent*/inputMesh.tangentOS/*end*/;

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
				/*ase_frag_input*/
				)
			{
				UNITY_SETUP_INSTANCE_ID( packedInput );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( packedInput );
				FragInputs input;
				ZERO_INITIALIZE(FragInputs, input);
				input.tangentToWorld = k_identity3x3;
				#if defined(ASE_NEEDS_FRAG_RELATIVE_WORLD_POS)
				/*ase_local_var:rwp*/float3 positionRWS = packedInput.positionRWS;
				input.positionRWS = positionRWS;
				#endif
				/*ase_local_var:wn*/float3 normalWS = packedInput.normalWS;
				/*ase_local_var:wt*/float4 tangentWS = packedInput.tangentWS;
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
				/*ase_frag_code:packedInput=VertexOutput*/
				surfaceDescription.Albedo = /*ase_frag_out:Albedo;Float3;0;-1;_Albedo*/float3( 0.7353569, 0.7353569, 0.7353569 )/*end*/;
				surfaceDescription.AlphaAlbedo = /*ase_frag_out:Alpha Albedo;Float;1;-1;_AlphaAlbedo*/1/*end*/;
				surfaceDescription.Normal = /*ase_frag_out:Normal;Float3;2;-1;_Normal*/float3( 0, 0, 1 )/*end*/;
				surfaceDescription.AlphaNormal = /*ase_frag_out:Alpha Normal;Float;3;-1;_AlphaNormal*/1/*end*/;
				surfaceDescription.Metallic = /*ase_frag_out:Metallic;Float;4;-1;_Metallic*/0/*end*/;
				surfaceDescription.Occlusion = /*ase_frag_out:Occlusion;Float;5;-1;_Occlusion*/1/*end*/;
				surfaceDescription.Smoothness = /*ase_frag_out:Smoothness;Float;6;-1;_Smoothness*/0.5/*end*/;
				surfaceDescription.MAOSOpacity = /*ase_frag_out:MAOSOpacity;Float;7;-1;_MAOSOpacity*/1/*end*/;
				surfaceDescription.Emission = /*ase_frag_out:Emission;Float3;8;-1;_Emission*/float3( 0, 0, 0 )/*end*/;

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

		/*ase_pass*/
		Pass
		{
			/*ase_hide_pass*/
			Name "ShaderGraph_MeshEmissive"
            Tags { "LightMode" = "ShaderGraph_MeshEmissive" }

			Blend 0 SrcAlpha One
            ZTest LEqual
            ZWrite Off

			HLSLPROGRAM
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

			/*ase_pragma*/

			struct VertexInput
			{
				float3 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 tangentOS : TANGENT;
				/*ase_vdata:p=p;n=n;t=t*/
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
				/*ase_interp(3,):sp=sp.xyzw;rwp=tc0;wn=tc1;wt=tc2*/
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float _DrawOrder;
			float _DecalMeshDepthBias;
			CBUFFER_END

			/*ase_globals*/

			/*ase_funcs*/

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

			VertexOutput Vert( VertexInput inputMesh /*ase_vert_input*/ )
			{
				VertexOutput o;
				UNITY_SETUP_INSTANCE_ID(inputMesh);
				UNITY_TRANSFER_INSTANCE_ID(inputMesh, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				/*ase_vert_code:inputMesh=VertexInput;o=VertexOutput*/
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				float3 defaultVertexValue = inputMesh.positionOS.xyz;
				#else
				float3 defaultVertexValue = float3( 0, 0, 0 );
				#endif
				float3 vertexValue = /*ase_vert_out:Vertex Offset;Float3;9;-1;_VertexOffset*/defaultVertexValue/*end*/;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				inputMesh.positionOS.xyz = vertexValue;
				#else
				inputMesh.positionOS.xyz += vertexValue;
				#endif

				inputMesh.normalOS = /*ase_vert_out:Vertex Normal;Float3;10;-1;_VertexNormal*/inputMesh.normalOS/*end*/;
				inputMesh.tangentOS = /*ase_vert_out:Vertex Tangent;Float4;11;-1;_VertexTangent*/inputMesh.tangentOS/*end*/;

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
				/*ase_frag_input*/
				)
			{
				UNITY_SETUP_INSTANCE_ID( packedInput );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( packedInput );
				FragInputs input;
				ZERO_INITIALIZE(FragInputs, input);
				input.tangentToWorld = k_identity3x3;
				#if defined(ASE_NEEDS_FRAG_RELATIVE_WORLD_POS)
				/*ase_local_var:rwp*/float3 positionRWS = packedInput.positionRWS;
				input.positionRWS = positionRWS;
				#endif
				/*ase_local_var:wn*/float3 normalWS = packedInput.normalWS;
				/*ase_local_var:wt*/float4 tangentWS = packedInput.tangentWS;
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
				/*ase_frag_code:packedInput=VertexOutput*/
				surfaceDescription.Albedo = /*ase_frag_out:Albedo;Float3;0;-1;_Albedo*/float3( 0.7353569, 0.7353569, 0.7353569 )/*end*/;
				surfaceDescription.AlphaAlbedo = /*ase_frag_out:Alpha Albedo;Float;1;-1;_AlphaAlbedo*/1/*end*/;
				surfaceDescription.Normal = /*ase_frag_out:Normal;Float3;2;-1;_Normal*/float3( 0, 0, 1 )/*end*/;
				surfaceDescription.AlphaNormal = /*ase_frag_out:Alpha Normal;Float;3;-1;_AlphaNormal*/1/*end*/;
				surfaceDescription.Metallic = /*ase_frag_out:Metallic;Float;4;-1;_Metallic*/0/*end*/;
				surfaceDescription.Occlusion = /*ase_frag_out:Occlusion;Float;5;-1;_Occlusion*/1/*end*/;
				surfaceDescription.Smoothness = /*ase_frag_out:Smoothness;Float;6;-1;_Smoothness*/0.5/*end*/;
				surfaceDescription.MAOSOpacity = /*ase_frag_out:MAOSOpacity;Float;7;-1;_MAOSOpacity*/1/*end*/;
				surfaceDescription.Emission = /*ase_frag_out:Emission;Float3;8;-1;_Emission*/float3( 0, 0, 0 )/*end*/;

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
		/*ase_pass_end*/
	}
	CustomEditor "UnityEditor.Rendering.HighDefinition.DecalGUI"
    FallBack "Hidden/Shader Graph/FallbackError"
}
