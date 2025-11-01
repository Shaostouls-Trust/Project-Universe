Shader /*ase_name*/ "Hidden/HDRP/Lit" /*end*/
{
	Properties
	{
		/*ase_props*/
		[HideInInspector] _RenderQueueType("Render Queue Type", Float) = 1
		[HideInInspector][ToggleUI] _AddPrecomputedVelocity("Add Precomputed Velocity", Float) = 1
		[HideInInspector][ToggleUI] _SupportDecals("Support Decals", Float) = 1.0
		[HideInInspector] _StencilRef("Stencil Ref", Int) = 0 // StencilUsage.Clear
		[HideInInspector] _StencilWriteMask("Stencil Write Mask", Int) = 3 // StencilUsage.RequiresDeferredLighting | StencilUsage.SubsurfaceScattering
		[HideInInspector] _StencilRefDepth("Stencil Ref Depth", Int) = 0 // Nothing
		[HideInInspector] _StencilWriteMaskDepth("Stencil Write Mask Depth", Int) = 8 // StencilUsage.TraceReflectionRay
		[HideInInspector] _StencilRefMV("Stencil Ref MV", Int) = 32 // StencilUsage.ObjectMotionVector
		[HideInInspector] _StencilWriteMaskMV("Stencil Write Mask MV", Int) = 32 // StencilUsage.ObjectMotionVector
		[HideInInspector] _StencilRefDistortionVec("Stencil Ref Distortion Vec", Int) = 4 				// DEPRECATED
		[HideInInspector] _StencilWriteMaskDistortionVec("Stencil Write Mask Distortion Vec", Int) = 4	// DEPRECATED
		[HideInInspector] _StencilWriteMaskGBuffer("Stencil Write Mask GBuffer", Int) = 3 // StencilUsage.RequiresDeferredLighting | StencilUsage.SubsurfaceScattering
		[HideInInspector] _StencilRefGBuffer("Stencil Ref GBuffer", Int) = 2 // StencilUsage.RequiresDeferredLighting
		[HideInInspector] _ZTestGBuffer("ZTest GBuffer", Int) = 4
		[HideInInspector][ToggleUI] _RequireSplitLighting("Require Split Lighting", Float) = 0
		[HideInInspector][ToggleUI] _ReceivesSSR("Receives SSR", Float) = 1
		[HideInInspector][ToggleUI] _ReceivesSSRTransparent("Receives SSR Transparent", Float) = 0
		[HideInInspector] _SurfaceType("Surface Type", Float) = 0
		[HideInInspector] _BlendMode("Blend Mode", Float) = 0
		[HideInInspector] _SrcBlend("Src Blend", Float) = 1
		[HideInInspector] _DstBlend("Dst Blend", Float) = 0
		[HideInInspector] _DstBlend2("__dst2", Float) = 0
		[HideInInspector] _AlphaSrcBlend("Alpha Src Blend", Float) = 1
		[HideInInspector] _AlphaDstBlend("Alpha Dst Blend", Float) = 0
		[HideInInspector][ToggleUI] _ZWrite("ZWrite", Float) = 1
		[HideInInspector][ToggleUI] _TransparentZWrite("Transparent ZWrite", Float) = 0
		[HideInInspector] _CullMode("Cull Mode", Float) = 2
		[HideInInspector] _TransparentSortPriority("Transparent Sort Priority", Float) = 0
		[HideInInspector][ToggleUI] _EnableFogOnTransparent("Enable Fog", Float) = 1
		[HideInInspector] _CullModeForward("Cull Mode Forward", Float) = 2 // This mode is dedicated to Forward to correctly handle backface then front face rendering thin transparent
		[HideInInspector][Enum(UnityEngine.Rendering.HighDefinition.TransparentCullMode)] _TransparentCullMode("Transparent Cull Mode", Int) = 2 // Back culling by default
		[HideInInspector] _ZTestDepthEqualForOpaque("ZTest Depth Equal For Opaque", Int) = 4 // Less equal
		[HideInInspector][Enum(UnityEngine.Rendering.CompareFunction)] _ZTestTransparent("ZTest Transparent", Int) = 4 // Less equal
		[HideInInspector][ToggleUI] _TransparentBackfaceEnable("Transparent Backface Enable", Float) = 0
		[HideInInspector][ToggleUI] _AlphaCutoffEnable("Alpha Cutoff Enable", Float) = 0
		[HideInInspector][ToggleUI] _UseShadowThreshold("Use Shadow Threshold", Float) = 0
		[HideInInspector][ToggleUI] _DoubleSidedEnable("Double Sided Enable", Float) = 0
		[HideInInspector][Enum(Flip, 0, Mirror, 1, None, 2)] _DoubleSidedNormalMode("Double Sided Normal Mode", Float) = 2
		[HideInInspector] _DoubleSidedConstants("DoubleSidedConstants", Vector) = (1,1,-1,0)

		//_TessPhongStrength( "Tess Phong Strength", Range( 0, 1 ) ) = 0.5
		//_TessValue( "Tess Max Tessellation", Range( 1, 32 ) ) = 16
		//_TessMin( "Tess Min Distance", Float ) = 10
		//_TessMax( "Tess Max Distance", Float ) = 25
		//_TessEdgeLength ( "Tess Edge length", Range( 2, 50 ) ) = 16
		//_TessMaxDisp( "Tess Max Displacement", Float ) = 25

		[HideInInspector][ToggleUI] _TransparentWritingMotionVec("Transparent Writing MotionVec", Float) = 0
		[HideInInspector][ToggleUI] _PerPixelSorting("_PerPixelSorting", Float) = 0.0
		[HideInInspector][Enum(UnityEngine.Rendering.HighDefinition.OpaqueCullMode)] _OpaqueCullMode("Opaque Cull Mode", Int) = 2 // Back culling by default
		[HideInInspector][ToggleUI] _EnableBlendModePreserveSpecularLighting("Enable Blend Mode Preserve Specular Lighting", Float) = 1
		[HideInInspector] _EmissionColor("Color", Color) = (1, 1, 1)

		[HideInInspector][NoScaleOffset] unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset] unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset] unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}

		[HideInInspector][Enum(Auto, 0, On, 1, Off, 2)] _DoubleSidedGIMode("Double sided GI mode", Float) = 0

		[HideInInspector][ToggleUI] _AlphaToMaskInspectorValue("_AlphaToMaskInspectorValue", Float) = 0 // Property used to save the alpha to mask state in the inspector
        [HideInInspector][ToggleUI] _AlphaToMask("__alphaToMask", Float) = 0

		//_Refrac ( "Refraction Model", Float) = 0
        [HideInInspector][ToggleUI]_DepthOffsetEnable("Boolean", Float) = 1
        [HideInInspector][ToggleUI]_ConservativeDepthOffsetEnable("Boolean", Float) = 1
	}

	SubShader
	{
		/*ase_subshader_options:Name=Additional Options
			Port:GBuffer:Bent Normal
				On:SetDefine:ASE_BENT_NORMAL 1
			Port:GBuffer:Coat Mask
				On:SetDefine:_MATERIAL_FEATURE_CLEAR_COAT
				On:SetDefine:pragma shader_feature_local_fragment _ _MATERIAL_FEATURE_CLEAR_COAT
			Port:GBuffer:Occlusion
				On:SetDefine:_AMBIENT_OCCLUSION 1
			Port:GBuffer:Baked GI
				On:SetDefine:ASE_BAKEDGI 1
			Port:GBuffer:Baked Back GI
				On:SetDefine:ASE_BAKEDBACKGI 1
			Port:GBuffer:Vertex Offset
				On:SetDefine:HAVE_MESH_MODIFICATION
			Option:Surface Type:Opaque,Transparent:Opaque
				Opaque:SetShaderProperty:_SurfaceType,0
				Opaque:SetPropertyOnSubShader:RenderType,Opaque
				Opaque:SetPropertyOnSubShader:RenderQueue,Geometry
				Opaque:SetPropertyOnSubShader:ZWrite,On
				Opaque:SetPropertyOnSubShader:BlendRGB,One,Zero
                Opaque:SetOption:    Blending Mode,0
				Opaque:HideOption:    Blending Mode
                Opaque:SetOption:    Blend Preserves Specular,0
				Opaque:HideOption:    Blend Preserves Specular
                Opaque:SetOption:  Rendering Pass,1
				Opaque:HideOption:  Rendering Pass
                Opaque:SetOption:  Refraction Model,0
				Opaque:HideOption:  Refraction Model
				Opaque:HideOption:  Back Then Front Rendering
				Opaque:HideOption:  Transparent Depth Prepass
				Opaque:HideOption:  Transparent Depth Postpass
				Opaque:HideOption:  ZWrite
				Opaque:HideOption:  Z Test
				Opaque:HideOption:Receive SSR Transparent
				Opaque:ShowOption:Receive SSR
				Transparent:SetShaderProperty:_SurfaceType,1
				Transparent:SetPropertyOnSubShader:RenderType,Transparent
				Transparent:SetPropertyOnSubShader:RenderQueue,Transparent
				Transparent:SetPropertyOnSubShader:ZWrite,Off
				Transparent:SetPropertyOnSubShader:BlendRGB,SrcAlpha,OneMinusSrcAlpha
				Transparent:ShowOption:    Blending Mode
				Transparent:ShowOption:    Blend Preserves Specular
				Transparent:ShowOption:  Rendering Pass
				Transparent:ShowOption:  Refraction Model
				Transparent:ShowOption:  Back Then Front Rendering
				Transparent:ShowOption:  Transparent Depth Prepass
				Transparent:ShowOption:  Transparent Depth Postpass
				Transparent:ShowOption:  ZWrite
				Transparent:ShowOption:  Z Test
				Transparent:ShowOption:Receive SSR Transparent
				Transparent:HideOption:Receive SSR
			Option:  Rendering Pass:Before Refraction,Default,Low Resolution:Default
				Before Refraction:SetPropertyOnSubShader:RenderQueue,Transparent,-250
				Before Refraction:SetShaderProperty:_RenderQueueType,4
				Default:SetPropertyOnSubShader:RenderQueue,Transparent,+0
				Default:SetShaderProperty:_RenderQueueType,5
				Low Resolution:SetPropertyOnSubShader:RenderQueue,Transparent,+400
				Low Resolution:SetShaderProperty:_RenderQueueType,6
				disable:SetPropertyOnSubShader:RenderQueue,Geometry,+0
			Option:  Refraction Model:None,Planar,Sphere,Thin:None
				None,disable:HidePort:GBuffer:Refraction Index
				None,disable:HidePort:GBuffer:Refraction Color
				None,disable:HidePort:GBuffer:Refraction Distance
				None,disable:RemoveDefine:_HAS_REFRACTION 1
				None,disable:RemoveDefine:pragma shader_feature_local _ _REFRACTION_PLANE _REFRACTION_SPHERE _REFRACTION_THIN
				Planar,Sphere,Thin:ShowPort:GBuffer:Thickness
				Planar,Sphere,Thin:ShowPort:GBuffer:Refraction Index
				Planar,Sphere,Thin:ShowPort:GBuffer:Refraction Color
				Planar,Sphere,Thin:ShowPort:GBuffer:Refraction Distance
				Planar,Sphere,Thin:SetDefine:_HAS_REFRACTION 1
				Planar,Sphere,Thin:SetDefine:pragma shader_feature_local _ _REFRACTION_PLANE _REFRACTION_SPHERE _REFRACTION_THIN
				None:SetShaderProperty:_Refrac,//[HideInInspector][Enum(None, 0, Planar, 1, Sphere, 2, Thin, 3)]_RefractionModel("Refraction Model", Int) = 0
				Planar:SetShaderProperty:_Refrac,[HideInInspector][Enum(None, 0, Planar, 1, Sphere, 2, Thin, 3)]_RefractionModel("Refraction Model", Int) = 1
				Sphere:SetShaderProperty:_Refrac,[HideInInspector][Enum(None, 0, Planar, 1, Sphere, 2, Thin, 3)]_RefractionModel("Refraction Model", Int) = 2
				Thin:SetShaderProperty:_Refrac,[HideInInspector][Enum(None, 0, Planar, 1, Sphere, 2, Thin, 3)]_RefractionModel("Refraction Model", Int) = 3
			Option:    Blending Mode:Alpha,Premultiply,Additive:Alpha
            	Alpha:SetShaderProperty:_BlendMode,0
				Additive:SetShaderProperty:_BlendMode,1
				Premultiply:SetShaderProperty:_BlendMode,4
			Option:    Blend Preserves Specular:false,true:true
				true:SetDefine:SUPPORT_BLENDMODE_PRESERVE_SPECULAR_LIGHTING
				false,disable:RemoveDefine:SUPPORT_BLENDMODE_PRESERVE_SPECULAR_LIGHTING
			Option:  Back Then Front Rendering:false,true:false
				true:IncludePass:TransparentBackface
				true:SetShaderProperty:_TransparentBackfaceEnable,1
				false,disable:ExcludePass:TransparentBackface
			Option:  Transparent Depth Prepass:false,true:false
				true:IncludePass:TransparentDepthPrepass
				true:ShowPort:GBuffer:Alpha Clip Threshold Depth Prepass
				false,disable:ExcludePass:TransparentDepthPrepass
				false,disable:HidePort:GBuffer:Alpha Clip Threshold Depth Prepass
			Option:  Transparent Depth Postpass:false,true:false
				true:IncludePass:TransparentDepthPostpass
				true:ShowPort:GBuffer:Alpha Clip Threshold Depth Postpass
				false,disable:ExcludePass:TransparentDepthPostpass
				false,disable:HidePort:GBuffer:Alpha Clip Threshold Depth Postpass
			Option:  ZWrite:false,true:false
				true:SetShaderProperty:_ZWrite,1
				true:SetShaderProperty:_TransparentZWrite,1
				false,disable:SetShaderProperty:_ZWrite,0
				false,disable:SetShaderProperty:_TransparentZWrite,0
			Option:  Z Test:Disabled,Never,Less,Equal,Less Equal,Greater,Not Equal,Greater Equal,Always:Less Equal
				Never:SetShaderProperty:_ZTestTransparent,1
				Less:SetShaderProperty:_ZTestTransparent,2
				Equal:SetShaderProperty:_ZTestTransparent,3
				Less Equal:SetShaderProperty:_ZTestTransparent,4
				Greater:SetShaderProperty:_ZTestTransparent,5
				Not Equal:SetShaderProperty:_ZTestTransparent,6
				Greater Equal:SetShaderProperty:_ZTestTransparent,7
				Always:SetShaderProperty:_ZTestTransparent,8
			Option:Double-Sided:Disabled,Enabled,Flipped Normals,Mirrored Normals:Disabled
                Disabled,Enabled,Flipped Normals,Mirrored Normals:SetDefine:ASE_NEED_CULLFACE 1
				Disabled,Enabled,Flipped Normals,Mirrored Normals:SetDefine:pragma shader_feature_local _ _DOUBLESIDED_ON
				Disabled:SetShaderProperty:_DoubleSidedEnable,0
				Disabled:SetShaderProperty:_DoubleSidedNormalMode,2
				Enabled:SetShaderProperty:_DoubleSidedEnable,1
				Enabled:SetShaderProperty:_DoubleSidedNormalMode,2
				Flipped Normals:SetShaderProperty:_DoubleSidedEnable,1
				Flipped Normals:SetShaderProperty:_DoubleSidedNormalMode,0
				Mirrored Normals:SetShaderProperty:_DoubleSidedEnable,1
				Mirrored Normals:SetShaderProperty:_DoubleSidedNormalMode,1
			Option:Alpha Clipping:false,true:false
				true:ShowOption:  Use Shadow Threshold
				true:ShowPort:GBuffer:Alpha Clip Threshold
				true:SetDefine:pragma shader_feature_local _ _ALPHATEST_ON
				false:HideOption:  Use Shadow Threshold
				false:HidePort:GBuffer:Alpha Clip Threshold
				false:RemoveDefine:pragma shader_feature_local _ _ALPHATEST_ON
			Option:  Use Shadow Threshold:false,true:false
				true:SetDefine:_ALPHATEST_SHADOW_ON 1
				true:ShowPort:GBuffer:Alpha Clip Threshold Shadow
				true:SetShaderProperty:_UseShadowThreshold,1
				false,disable:RemoveDefine:_ALPHATEST_SHADOW_ON 1
				false,disable:HidePort:GBuffer:Alpha Clip Threshold Shadow
			Option:Material Type,InvertActionOnDeselection:Standard,Subsurface Scattering,Anisotropy,Iridescence,Specular Color,Translucent:Standard
				Standard:ShowPort:GBuffer:Metallic
				Subsurface Scattering:SetDefine:_MATERIAL_FEATURE_SUBSURFACE_SCATTERING 1
				Subsurface Scattering:SetDefine:pragma shader_feature_local_fragment _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
				Subsurface Scattering:ShowPort:GBuffer:Subsurface Mask
				Subsurface Scattering:ShowPort:GBuffer:Diffusion Profile
				Subsurface Scattering:ShowOption:  Transmission
				Subsurface Scattering:SetShaderProperty:_RequireSplitLighting,1
				Anisotropy:SetDefine:_MATERIAL_FEATURE_ANISOTROPY 1
				Anisotropy:SetDefine:pragma shader_feature_local_fragment _MATERIAL_FEATURE_ANISOTROPY
				Anisotropy:ShowPort:GBuffer:Tangent
				Anisotropy:ShowPort:GBuffer:Anisotropy
				Anisotropy:ShowPort:GBuffer:Metallic
				Iridescence:SetDefine:_MATERIAL_FEATURE_IRIDESCENCE 1
				Iridescence:SetDefine:pragma shader_feature_local_fragment _MATERIAL_FEATURE_IRIDESCENCE
				Iridescence:ShowPort:GBuffer:Iridescence Mask
				Iridescence:ShowPort:GBuffer:Iridescence Thickness
				Iridescence:ShowPort:GBuffer:Metallic
				Specular Color:SetDefine:_MATERIAL_FEATURE_SPECULAR_COLOR 1
				Specular Color:SetDefine:pragma shader_feature_local_fragment _MATERIAL_FEATURE_SPECULAR_COLOR
				Specular Color:ShowPort:GBuffer:Specular
				Specular Color:ShowOption:  Energy Conserving Specular
				Translucent:SetDefine:_MATERIAL_FEATURE_TRANSMISSION 1
				Translucent:SetDefine:pragma shader_feature_local_fragment _MATERIAL_FEATURE_TRANSMISSION
				Translucent:ShowPort:GBuffer:Diffusion Profile
				Translucent:ShowPort:GBuffer:Transmission Mask
				Translucent:ShowPort:GBuffer:Thickness
				Standard,Anisotropy,Iridescence,Specular Color,Translucent:SetShaderProperty:_RequireSplitLighting,0
				Standard,Anisotropy,Iridescence,Specular Color:SetOption:  Transmission,0
			Option:  Energy Conserving Specular:false,true:true
				true:SetDefine:_ENERGY_CONSERVING_SPECULAR 1
				false,disable:RemoveDefine:_ENERGY_CONSERVING_SPECULAR 1
				true:SetDefine:SUPPORT_BLENDMODE_PRESERVE_SPECULAR_LIGHTING
				false:RemoveDefine:SUPPORT_BLENDMODE_PRESERVE_SPECULAR_LIGHTING
			Option:  Transmission,InvertActionOnDeselection:false,true:true
				true:ShowPort:GBuffer:Transmission Mask
				true:ShowPort:GBuffer:Thickness
				true:SetDefine:_MATERIAL_FEATURE_TRANSMISSION 1
				true:SetDefine:pragma shader_feature_local_fragment _MATERIAL_FEATURE_TRANSMISSION
				true:SetDefine:pragma shader_feature_local_fragment _MATERIAL_FEATURE_COLORED_TRANSMISSION
				false,disable:RemoveDefine:_MATERIAL_FEATURE_TRANSMISSION 1
				false,disable:RemoveDefine:pragma shader_feature_local_fragment _MATERIAL_FEATURE_TRANSMISSION
				false,disable:RemoveDefine:pragma shader_feature_local_fragment _MATERIAL_FEATURE_COLORED_TRANSMISSION
			Option:Receive Decals:false,true:true
				true:SetDefine:pragma shader_feature_local_fragment _ _DISABLE_DECALS
				false:RemoveDefine:pragma shader_feature_local_fragment _ _DISABLE_DECALS
			Option:Receive SSR:false,true:true
				true:SetShaderProperty:_ReceivesSSR,1
				true:RemoveDefine:pragma shader_feature_local_fragment _ _DISABLE_SSR
				false:SetShaderProperty:_ReceivesSSR,1
				false:SetDefine:pragma shader_feature_local_fragment _ _DISABLE_SSR
			Option:Receive SSR Transparent:false,true:false
			    true:SetShaderProperty:_ReceivesSSRTransparent,1
			    true:RemoveDefine:pragma shader_feature_local_fragment _ _DISABLE_SSR_TRANSPARENT
				true:SetOption:  Transparent Depth Prepass,1
				true:SetOption:  Rendering Pass,0
				false:SetShaderProperty:_ReceivesSSRTransparent,0
				false:SetDefine:pragma shader_feature_local_fragment _ _DISABLE_SSR_TRANSPARENT
				false:SetOption:  Transparent Depth Prepass,0
			Option:Motion Vectors:false,true:true
				true:SetShaderProperty:_AddPrecomputedVelocity,[HideInInspector][ToggleUI] _AddPrecomputedVelocity("Add Precomputed Velocity", Float) = 1
				false:SetShaderProperty:_AddPrecomputedVelocity,//[HideInInspector][ToggleUI] _AddPrecomputedVelocity("Add Precomputed Velocity", Float) = 1
				true:ShowOption:  Add Precomputed Velocity
				false:HideOption:  Add Precomputed Velocity
				true:IncludePass:MotionVectors
				false:ExcludePass:MotionVectors
				true:SetOption:Tessellation,0
			Option:  Add Precomputed Velocity:false,true:false
				false,disable:RemoveDefine:pragma shader_feature_local _ _ADD_PRECOMPUTED_VELOCITY
				true:SetDefine:pragma shader_feature_local _ _ADD_PRECOMPUTED_VELOCITY
				true:SetShaderProperty:_AddPrecomputedVelocity,1
			Option:Specular AA:false,true:false
				true:SetDefine:shader_feature_local_fragment _ENABLE_GEOMETRIC_SPECULAR_AA
				true:ShowPort:GBuffer:Specular AA Screen Space Variance
				true:ShowPort:GBuffer:Specular AA Threshold
				false:RemoveDefine:shader_feature_local_fragment _ENABLE_GEOMETRIC_SPECULAR_AA
				false:HidePort:GBuffer:Specular AA Screen Space Variance
				false:HidePort:GBuffer:Specular AA Threshold
			Option:Specular Occlusion Mode:Off,From AO,From AO And Bent Normal,Custom:From AO
				Off:RemoveDefine:_SPECULAR_OCCLUSION_FROM_AO 1
				Off:RemoveDefine:_SPECULAR_OCCLUSION_FROM_AO_BENT_NORMAL 1
				Off:RemoveDefine:_SPECULAR_OCCLUSION_CUSTOM 1
				Off:HidePort:Specular Occlusion
				From AO:SetDefine:_SPECULAR_OCCLUSION_FROM_AO 1
				From AO:RemoveDefine:_SPECULAR_OCCLUSION_FROM_AO_BENT_NORMAL 1
				From AO:RemoveDefine:_SPECULAR_OCCLUSION_CUSTOM 1
				From AO:HidePort:Specular Occlusion
				From AO And Bent Normal:RemoveDefine:_SPECULAR_OCCLUSION_FROM_AO 1
				From AO And Bent Normal:SetDefine:_SPECULAR_OCCLUSION_FROM_AO_BENT_NORMAL 1
				From AO And Bent Normal:RemoveDefine:_SPECULAR_OCCLUSION_CUSTOM 1
				From AO And Bent Normal:HidePort:Specular Occlusion
				Custom:RemoveDefine:_SPECULAR_OCCLUSION_FROM_AO 1
				Custom:RemoveDefine:_SPECULAR_OCCLUSION_FROM_AO_BENT_NORMAL 1
				Custom:SetDefine:_SPECULAR_OCCLUSION_CUSTOM 1
				Custom:ShowPort:Specular Occlusion
			Option:Override Baked GI:false,true:false
				true:ShowPort:GBuffer:Baked GI
				true:ShowPort:GBuffer:Baked Back GI
				false:HidePort:GBuffer:Baked GI
				false:HidePort:GBuffer:Baked Back GI
			Option:Depth Offset:false,true:false
				true:SetDefine:_DEPTHOFFSET_ON 1
				true:SetDefine:pragma shader_feature_local_fragment _ _DEPTHOFFSET_ON
				true:ShowPort:GBuffer:Depth Offset
				false:RemoveDefine:_DEPTHOFFSET_ON 1
				false:RemoveDefine:pragma shader_feature_local_fragment _ _DEPTHOFFSET_ON
				false:HidePort:GBuffer:Depth Offset
				true:ShowOption:  Conserative
				false:HideOption:  Conserative
				false:RemoveDefine:pragma shader_feature_local_fragment _ _CONSERVATIVE_DEPTH_OFFSET
			Option:  Conserative:false,true:true
				true:SetDefine:pragma shader_feature_local_fragment _ _CONSERVATIVE_DEPTH_OFFSET
				false:RemoveDefine:pragma shader_feature_local_fragment _ _CONSERVATIVE_DEPTH_OFFSET
			Option:GPU Instancing:false,true:true
				true:SetDefine:pragma multi_compile_instancing
				true:SetDefine:pragma instancing_options renderinglayer
				false:RemoveDefine:pragma multi_compile_instancing
				false:RemoveDefine:pragma instancing_options renderinglayer
			Option:LOD CrossFade:false,true:false
				true:SetDefine:pragma multi_compile _ LOD_FADE_CROSSFADE
				false:RemoveDefine:pragma multi_compile _ LOD_FADE_CROSSFADE
			Option:Tessellation:false,true:false
				true:SetDefine:ASE_TESSELLATION 1
				true:SetDefine:pragma require tessellation tessHW
				true:SetDefine:pragma hull HullFunction
				true:SetDefine:pragma domain DomainFunction
				true:ShowOption:  Phong
				true:ShowOption:  Type
				false,disable:RemoveDefine:ASE_TESSELLATION 1
				false,disable:RemoveDefine:pragma require tessellation tessHW
				false,disable:RemoveDefine:pragma hull HullFunction
				false,disable:RemoveDefine:pragma domain DomainFunction
				false,disable:HideOption:  Phong
				false,disable:HideOption:  Type
				true:SetOption:Motion Vectors,0
			Option:  Phong:false,true:false
				true:SetDefine:ASE_PHONG_TESSELLATION
				false,disable:RemoveDefine:ASE_PHONG_TESSELLATION
				true:ShowOption:  Strength
				false,disable:HideOption:  Strength
			Field:  Strength:Float:0.5:0:1:_TessPhongStrength
				Change:SetMaterialProperty:_TessPhongStrength
				Change:SetShaderProperty:_TessPhongStrength,_TessPhongStrength( "Phong Tess Strength", Range( 0, 1 ) ) = 0.5
				Inline,disable:SetShaderProperty:_TessPhongStrength,//_TessPhongStrength( "Phong Tess Strength", Range( 0, 1 ) ) = 0.5
			Option:  Type:Fixed,Distance Based,Edge Length,Edge Length Cull:Fixed
				Fixed:SetDefine:ASE_FIXED_TESSELLATION
				Fixed,Distance Based:ShowOption:  Tess
				Distance Based:SetDefine:ASE_DISTANCE_TESSELLATION
				Distance Based:ShowOption:  Min
				Distance Based:ShowOption:  Max
				Edge Length:SetDefine:ASE_LENGTH_TESSELLATION
				Edge Length,Edge Length Cull:ShowOption:  Edge Length
				Edge Length Cull:SetDefine:ASE_LENGTH_CULL_TESSELLATION
				Edge Length Cull:ShowOption:  Max Displacement
				disable,Distance Based,Edge Length,Edge Length Cull:RemoveDefine:ASE_FIXED_TESSELLATION
				disable,Fixed,Edge Length,Edge Length Cull:RemoveDefine:ASE_DISTANCE_TESSELLATION
				disable,Fixed,Distance Based,Edge Length Cull:RemoveDefine:ASE_LENGTH_TESSELLATION
				disable,Fixed,Distance Based,Edge Length:RemoveDefine:ASE_LENGTH_CULL_TESSELLATION
				disable,Edge Length,Edge Length Cull:HideOption:  Tess
				disable,Fixed,Edge Length,Edge Length Cull:HideOption:  Min
				disable,Fixed,Edge Length,Edge Length Cull:HideOption:  Max
				disable,Fixed,Distance Based:HideOption:  Edge Length
				disable,Fixed,Distance Based,Edge Length:HideOption:  Max Displacement
			Field:  Tess:Float:16:1:32:_TessValue
				Change:SetMaterialProperty:_TessValue
				Change:SetShaderProperty:_TessValue,_TessValue( "Max Tessellation", Range( 1, 32 ) ) = 16
				Inline,disable:SetShaderProperty:_TessValue,//_TessValue( "Max Tessellation", Range( 1, 32 ) ) = 16
			Field:  Min:Float:10:_TessMin
				Change:SetMaterialProperty:_TessMin
				Change:SetShaderProperty:_TessMin,_TessMin( "Tess Min Distance", Float ) = 10
				Inline,disable:SetShaderProperty:_TessMin,//_TessMin( "Tess Min Distance", Float ) = 10
			Field:  Max:Float:25:_TessMax
				Change:SetMaterialProperty:_TessMax
				Change:SetShaderProperty:_TessMax,_TessMax( "Tess Max Distance", Float ) = 25
				Inline,disable:SetShaderProperty:_TessMax,//_TessMax( "Tess Max Distance", Float ) = 25
			Field:  Edge Length:Float:16:2:50:_TessEdgeLength
				Change:SetMaterialProperty:_TessEdgeLength
				Change:SetShaderProperty:_TessEdgeLength,_TessEdgeLength ( "Edge length", Range( 2, 50 ) ) = 16
				Inline,disable:SetShaderProperty:_TessEdgeLength,//_TessEdgeLength ( "Edge length", Range( 2, 50 ) ) = 16
			Field:  Max Displacement:Float:25:_TessMaxDisp
				Change:SetMaterialProperty:_TessMaxDisp
				Change:SetShaderProperty:_TessMaxDisp,_TessMaxDisp( "Max Displacement", Float ) = 25
				Inline,disable:SetShaderProperty:_TessMaxDisp,//_TessMaxDisp( "Max Displacement", Float ) = 25
			Option:Vertex Position:Absolute,Relative:Relative
				Absolute:SetDefine:ASE_ABSOLUTE_VERTEX_POS 1
				Absolute:SetPortName:GBuffer:11,Vertex Position
				Relative:RemoveDefine:ASE_ABSOLUTE_VERTEX_POS 1
				Relative:SetPortName:GBuffer:11,Vertex Offset
		*/

		Tags
		{
			"RenderPipeline"="HDRenderPipeline"
			"RenderType"="Opaque"
			"Queue"="Geometry+0"
		}

		HLSLINCLUDE
		#pragma target 4.5
		#pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch

		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"

		struct GlobalSurfaceDescription // GBuffer Forward META TransparentBackface
		{
			float3 BaseColor;
			float3 Normal;
			float3 BentNormal;
			float3 Specular;
			float CoatMask;
			float Metallic;
			float3 Emission;
			float Smoothness;
			float Occlusion;
			float Alpha;
			float AlphaClipThreshold;
			float AlphaClipThresholdShadow;
			float AlphaClipThresholdDepthPrepass;
			float AlphaClipThresholdDepthPostpass;
			float SpecularOcclusion;
			float SpecularAAScreenSpaceVariance;
			float SpecularAAThreshold;
			float RefractionIndex;
			float3 RefractionColor;
			float RefractionDistance;
			float DiffusionProfile;
			float TransmissionMask;
			float Thickness;
			float SubsurfaceMask;
			float Anisotropy;
			float3 Tangent;
			float IridescenceMask;
			float IridescenceThickness;
			float3 BakedGI;
			float3 BakedBackGI;
			float DepthOffset;
			float4 VTPackedFeedback;
		};

		struct AlphaSurfaceDescription // ShadowCaster
		{
			float3 Emission;
			float Alpha;
			float AlphaClipThreshold;
			float AlphaClipThresholdShadow;
			float3 BakedGI;
			float3 BakedBackGI;
			float DepthOffset;
			float4 VTPackedFeedback;
		};

		struct SceneSurfaceDescription // SceneSelection
		{
		    float3 Emission;
			float Alpha;
			float AlphaClipThreshold;
			float AlphaClipThresholdShadow;
			float RefractionIndex;
			float3 RefractionColor;
			float RefractionDistance;
			float3 BakedGI;
			float3 BakedBackGI;
			float DepthOffset;
			float4 VTPackedFeedback;
		};

		struct PrePassSurfaceDescription // DepthPrePass
		{
			float3 Normal;
			float3 Emission;
			float Smoothness;
			float Alpha;
			float AlphaClipThreshold;
			float AlphaClipThresholdShadow;
			float AlphaClipThresholdDepthPrepass;
			float3 BakedGI;
			float3 BakedBackGI;
			float DepthOffset;
			float4 VTPackedFeedback;
		};

		struct PostPassSurfaceDescription //DepthPostPass
		{
			float3 Emission;
			float Alpha;
			float AlphaClipThreshold;
			float AlphaClipThresholdShadow;
			float AlphaClipThresholdDepthPostpass;
			float3 BakedGI;
			float3 BakedBackGI;
			float DepthOffset;
			float4 VTPackedFeedback;
		};

		struct SmoothSurfaceDescription // MotionVectors DepthOnly
		{
			float3 Normal;
			float3 Emission;
			float Smoothness;
			float Alpha;
			float AlphaClipThreshold;
			float AlphaClipThresholdShadow;
			float3 BakedGI;
			float3 BakedBackGI;
			float DepthOffset;
			float4 VTPackedFeedback;
		};

        struct PickingSurfaceDescription //Picking
		{
            float3 BentNormal;
			float3 Emission;
			float Alpha;
			float AlphaClipThreshold;
			float AlphaClipThresholdShadow;
			float3 BakedGI;
			float3 BakedBackGI;
			float DepthOffset;
			float4 VTPackedFeedback;

			float3 ObjectSpaceNormal;
			float3 WorldSpaceNormal;
			float3 TangentSpaceNormal;
			float3 ObjectSpaceViewDirection;
			float3 WorldSpaceViewDirection;
			float3 ObjectSpacePosition;
		};

		#ifndef ASE_TESS_FUNCS
		#define ASE_TESS_FUNCS
		float4 FixedTess( float tessValue )
		{
			return tessValue;
		}

		float CalcDistanceTessFactor (float4 vertex, float minDist, float maxDist, float tess, float4x4 o2w, float3 cameraPos )
		{
			float3 wpos = mul(o2w,vertex).xyz;
			float dist = distance (wpos, cameraPos);
			float f = clamp(1.0 - (dist - minDist) / (maxDist - minDist), 0.01, 1.0) * tess;
			return f;
		}

		float4 CalcTriEdgeTessFactors (float3 triVertexFactors)
		{
			float4 tess;
			tess.x = 0.5 * (triVertexFactors.y + triVertexFactors.z);
			tess.y = 0.5 * (triVertexFactors.x + triVertexFactors.z);
			tess.z = 0.5 * (triVertexFactors.x + triVertexFactors.y);
			tess.w = (triVertexFactors.x + triVertexFactors.y + triVertexFactors.z) / 3.0f;
			return tess;
		}

		float CalcEdgeTessFactor (float3 wpos0, float3 wpos1, float edgeLen, float3 cameraPos, float4 scParams )
		{
			float dist = distance (0.5 * (wpos0+wpos1), cameraPos);
			float len = distance(wpos0, wpos1);
			float f = max(len * scParams.y / (edgeLen * dist), 1.0);
			return f;
		}

		float DistanceFromPlaneASE (float3 pos, float4 plane)
		{
			return dot (float4(pos,1.0f), plane);
		}

		bool WorldViewFrustumCull (float3 wpos0, float3 wpos1, float3 wpos2, float cullEps, float4 planes[6] )
		{
			float4 planeTest;
			planeTest.x = (( DistanceFromPlaneASE(wpos0, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlaneASE(wpos1, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlaneASE(wpos2, planes[0]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.y = (( DistanceFromPlaneASE(wpos0, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlaneASE(wpos1, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlaneASE(wpos2, planes[1]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.z = (( DistanceFromPlaneASE(wpos0, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlaneASE(wpos1, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlaneASE(wpos2, planes[2]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.w = (( DistanceFromPlaneASE(wpos0, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlaneASE(wpos1, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlaneASE(wpos2, planes[3]) > -cullEps) ? 1.0f : 0.0f );
			return !all (planeTest);
		}

		float4 DistanceBasedTess( float4 v0, float4 v1, float4 v2, float tess, float minDist, float maxDist, float4x4 o2w, float3 cameraPos )
		{
			float3 f;
			f.x = CalcDistanceTessFactor (v0,minDist,maxDist,tess,o2w,cameraPos);
			f.y = CalcDistanceTessFactor (v1,minDist,maxDist,tess,o2w,cameraPos);
			f.z = CalcDistanceTessFactor (v2,minDist,maxDist,tess,o2w,cameraPos);

			return CalcTriEdgeTessFactors (f);
		}

		float4 EdgeLengthBasedTess( float4 v0, float4 v1, float4 v2, float edgeLength, float4x4 o2w, float3 cameraPos, float4 scParams )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;
			tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
			tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
			tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
			tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			return tess;
		}

		float4 EdgeLengthBasedTessCull( float4 v0, float4 v1, float4 v2, float edgeLength, float maxDisplacement, float4x4 o2w, float3 cameraPos, float4 scParams, float4 planes[6] )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;

			if (WorldViewFrustumCull(pos0, pos1, pos2, maxDisplacement, planes))
			{
				tess = 0.0f;
			}
			else
			{
				tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
				tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
				tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
				tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			}
			return tess;
		}
		#endif //ASE_TESS_FUNCS
		ENDHLSL

		/*ase_pass*/
		Pass
		{
			/*ase_main_pass*/
			Name "GBuffer"
			Tags 
			{ 
				"LightMode" = "GBuffer" 
		    }

			Cull [_CullMode]
			ZTest [_ZTestGBuffer]

			Stencil
			{
				WriteMask [_StencilWriteMaskGBuffer]
				Ref [_StencilRefGBuffer]
		        CompFront Always
		        PassFront Replace
		        CompBack Always
		        PassBack Replace
			}

			HLSLPROGRAM
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #pragma shader_feature _ _SURFACE_TYPE_TRANSPARENT
            #pragma shader_feature_local _ _TRANSPARENT_WRITES_MOTION_VEC _TRANSPARENT_REFRACTIVE_SORT
            #pragma shader_feature_local_fragment _ _ENABLE_FOG_ON_TRANSPARENT

			#pragma multi_compile_fragment _ RENDERING_LAYERS
            #pragma multi_compile_fragment _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ DEBUG_DISPLAY
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile_fragment _ PROBE_VOLUMES_L1 PROBE_VOLUMES_L2
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fragment DECALS_OFF DECALS_3RT DECALS_4RT
            #pragma multi_compile_fragment _ DECAL_SURFACE_GRADIENT
            #pragma multi_compile _ USE_LEGACY_LIGHTMAPS

			#pragma vertex Vert
			#pragma fragment Frag

			#define SHADERPASS SHADERPASS_GBUFFER

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/GeometricTools.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Tessellation.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
            #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"

			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"

            #ifdef RAYTRACING_SHADER_GRAPH_DEFAULT
                #define RAYTRACING_SHADER_GRAPH_HIGH
            #endif

            #ifdef RAYTRACING_SHADER_GRAPH_RAYTRACED
                #define RAYTRACING_SHADER_GRAPH_LOW
            #endif

            #ifndef SHADER_UNLIT
            #if defined(_DOUBLESIDED_ON) && !defined(VARYINGS_NEED_CULLFACE)
                #define VARYINGS_NEED_CULLFACE
            #endif
            #endif

			#if defined(_DOUBLESIDED_ON) && !defined(ASE_NEED_CULLFACE)
			    #define ASE_NEED_CULLFACE 1
			#endif

            #if _MATERIAL_FEATURE_COLORED_TRANSMISSION
            #undef _MATERIAL_FEATURE_CLEAR_COAT
            #endif

		    #if defined(_MATERIAL_FEATURE_SUBSURFACE_SCATTERING) && !defined(_SURFACE_TYPE_TRANSPARENT)
			#define OUTPUT_SPLIT_LIGHTING
		    #endif

		    #if (SHADERPASS == SHADERPASS_PATH_TRACING) && !defined(_DOUBLESIDED_ON) && (defined(_REFRACTION_PLANE) || defined(_REFRACTION_SPHERE))
			#undef  _REFRACTION_PLANE
			#undef  _REFRACTION_SPHERE
			#define _REFRACTION_THIN
		    #endif

            #if SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_PREPASS
            #if !defined(_DISABLE_SSR_TRANSPARENT) && !defined(SHADER_UNLIT)
                #define WRITE_NORMAL_BUFFER
            #endif
            #endif

            #if SHADERPASS == SHADERPASS_MOTION_VECTORS && defined(WRITE_DECAL_BUFFER_AND_RENDERING_LAYER)
                #define WRITE_DECAL_BUFFER
            #endif

            #ifndef DEBUG_DISPLAY
                #if !defined(_SURFACE_TYPE_TRANSPARENT)
                    #if SHADERPASS == SHADERPASS_FORWARD
                    #define SHADERPASS_FORWARD_BYPASS_ALPHA_TEST
                    #elif SHADERPASS == SHADERPASS_GBUFFER
                    #define SHADERPASS_GBUFFER_BYPASS_ALPHA_TEST
                    #endif
                #endif
            #endif

            #if defined(SHADER_LIT) && !defined(_SURFACE_TYPE_TRANSPARENT)
                #define _DEFERRED_CAPABLE_MATERIAL
            #endif
        
            #if (defined(_TRANSPARENT_WRITES_MOTION_VEC) || defined(_TRANSPARENT_REFRACTIVE_SORT)) && defined(_SURFACE_TYPE_TRANSPARENT)
                #define _WRITE_TRANSPARENT_MOTION_VECTOR
            #endif

			CBUFFER_START( UnityPerMaterial )
			float4 _EmissionColor;
			float _AlphaCutoff;
			float _RenderQueueType;
			#ifdef _ADD_PRECOMPUTED_VELOCITY
			    float _AddPrecomputedVelocity;
			#endif
			float _StencilRef;
			float _StencilWriteMask;
			float _StencilRefDepth;
			float _StencilWriteMaskDepth;
			float _StencilRefMV;
			float _StencilWriteMaskMV;
			float _StencilRefDistortionVec;
			float _StencilWriteMaskDistortionVec;
			float _StencilWriteMaskGBuffer;
			float _StencilRefGBuffer;
			float _ZTestGBuffer;
			float _RequireSplitLighting;
			float _ReceivesSSR;
			float _SurfaceType;
			float _BlendMode;
            #ifdef SUPPORT_BLENDMODE_PRESERVE_SPECULAR_LIGHTING
			    float _EnableBlendModePreserveSpecularLighting;
            #endif
			float _SrcBlend;
			float _DstBlend;
			float _DstBlend2;
			float _AlphaSrcBlend;
			float _AlphaDstBlend;
			float _ZWrite;
			float _TransparentZWrite;
			float _CullMode;
			float _TransparentSortPriority;
			float _EnableFogOnTransparent;
			float _CullModeForward;
			float _TransparentCullMode;
			float _ZTestDepthEqualForOpaque;
			float _ZTestTransparent;
			float _TransparentBackfaceEnable;
			float _AlphaCutoffEnable;
			float _UseShadowThreshold;
			float _DoubleSidedEnable;
			float _DoubleSidedNormalMode;
			float4 _DoubleSidedConstants;
			#ifdef ASE_TESSELLATION
			    float _TessPhongStrength;
			    float _TessValue;
			    float _TessMin;
			    float _TessMax;
			    float _TessEdgeLength;
			    float _TessMaxDisp;
			#endif
			UNITY_TEXTURE_STREAMING_DEBUG_VARS;
			CBUFFER_END

            #ifdef SCENEPICKINGPASS
			float4 _SelectionID;
            #endif

            #ifdef SCENESELECTIONPASS
			int _ObjectId;
			int _PassValue;
            #endif

			/*ase_globals*/

            #ifdef DEBUG_DISPLAY
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Debug/DebugDisplay.hlsl"
            #endif

			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/NormalSurfaceGradient.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/BuiltinUtilities.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialUtilities.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphFunctions.hlsl"

            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/DecalUtilities.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitDecalData.hlsl"

			/*ase_pragma*/

			struct AttributesMesh
			{
				float3 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 tangentOS : TANGENT;
				float4 uv1 : TEXCOORD1;
				float4 uv2 : TEXCOORD2;
				/*ase_vdata:p=p;n=n;t=t;uv1=tc1;uv2=tc2*/
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryingsMeshToPS
			{
				SV_POSITION_QUALIFIERS float4 positionCS : SV_Position;
				float4 clipPosV : TEXCOORD0;
				float3 positionRWS : TEXCOORD1;
				float3 normalWS : TEXCOORD2;
				float4 tangentWS : TEXCOORD3;
				float4 uv1 : TEXCOORD4;
				float4 uv2 : TEXCOORD5;
				/*ase_interp(6,):sp=sp.xyzw;rwp=tc1;wn=tc2;wt=tc3;uv1=tc4;uv2=tc5*/
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
				#if defined(SHADER_STAGE_FRAGMENT) && defined(ASE_NEED_CULLFACE)
				FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
				#endif
			};


			/*ase_funcs*/

			void BuildSurfaceData(FragInputs fragInputs, inout GlobalSurfaceDescription surfaceDescription, float3 V, PositionInputs posInput, out SurfaceData surfaceData, out float3 bentNormalWS)
			{
				ZERO_INITIALIZE(SurfaceData, surfaceData);

				surfaceData.specularOcclusion = 1.0;
				surfaceData.thickness = 0.0;

				surfaceData.baseColor =					surfaceDescription.BaseColor;
				surfaceData.perceptualSmoothness =		surfaceDescription.Smoothness;
				surfaceData.ambientOcclusion =			surfaceDescription.Occlusion;
				surfaceData.metallic =					surfaceDescription.Metallic;
				surfaceData.coatMask =					surfaceDescription.CoatMask;

				#ifdef _SPECULAR_OCCLUSION_CUSTOM
				surfaceData.specularOcclusion =			surfaceDescription.SpecularOcclusion;
				#endif

				#ifdef _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
				surfaceData.subsurfaceMask =			surfaceDescription.SubsurfaceMask;
				#endif

				#if defined(_HAS_REFRACTION) || defined(_MATERIAL_FEATURE_TRANSMISSION)
				surfaceData.thickness =					surfaceDescription.Thickness;
				#endif

				#ifdef _MATERIAL_FEATURE_TRANSMISSION
				surfaceData.transmissionMask =			surfaceDescription.TransmissionMask;
				#endif

				#if defined( _MATERIAL_FEATURE_SUBSURFACE_SCATTERING ) || defined( _MATERIAL_FEATURE_TRANSMISSION )
				surfaceData.diffusionProfileHash =		asuint(surfaceDescription.DiffusionProfile);
				#endif

				#ifdef _MATERIAL_FEATURE_SPECULAR_COLOR
				surfaceData.specularColor =				surfaceDescription.Specular;
				#endif

				#ifdef _MATERIAL_FEATURE_ANISOTROPY
				surfaceData.anisotropy =				surfaceDescription.Anisotropy;
				#endif

				#ifdef _MATERIAL_FEATURE_IRIDESCENCE
				surfaceData.iridescenceMask =			surfaceDescription.IridescenceMask;
				surfaceData.iridescenceThickness =		surfaceDescription.IridescenceThickness;
				#endif

				// refraction
                #if defined(_REFRACTION_PLANE) || defined(_REFRACTION_SPHERE) || defined(_REFRACTION_THIN)
                    if (_EnableSSRefraction)
                    {
                        surfaceData.ior =                       surfaceDescription.RefractionIndex;
                        surfaceData.transmittanceColor =        surfaceDescription.RefractionColor;
                        surfaceData.atDistance =                surfaceDescription.RefractionDistance;
        
                        surfaceData.transmittanceMask = (1.0 - surfaceDescription.Alpha);
                        surfaceDescription.Alpha = 1.0;
                    }
                    else
                    {
                        surfaceData.ior = 1.0;
                        surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                        surfaceData.atDistance = 1.0;
                        surfaceData.transmittanceMask = 0.0;
                        surfaceDescription.Alpha = 1.0;
                    }
                #else
                    surfaceData.ior = 1.0;
                    surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                    surfaceData.atDistance = 1.0;
                    surfaceData.transmittanceMask = 0.0;
                #endif

				surfaceData.materialFeatures = MATERIALFEATUREFLAGS_LIT_STANDARD;

				#ifdef _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SUBSURFACE_SCATTERING;
				#endif

				#ifdef _MATERIAL_FEATURE_TRANSMISSION
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_TRANSMISSION;
				#endif

				#ifdef _MATERIAL_FEATURE_COLORED_TRANSMISSION
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_TRANSMISSION;
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_COLORED_TRANSMISSION;
				#endif

                #ifdef _MATERIAL_FEATURE_ANISOTROPY
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_ANISOTROPY;
                    surfaceData.normalWS = float3(0, 1, 0);
                #endif

				#ifdef _MATERIAL_FEATURE_CLEAR_COAT
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_CLEAR_COAT;
				#endif

				#ifdef _MATERIAL_FEATURE_IRIDESCENCE
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_IRIDESCENCE;
				#endif

				#ifdef _MATERIAL_FEATURE_SPECULAR_COLOR
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SPECULAR_COLOR;
				#endif

				#if defined (_MATERIAL_FEATURE_SPECULAR_COLOR) && defined (_ENERGY_CONSERVING_SPECULAR)
                    surfaceData.baseColor *= ( 1.0 - Max3( surfaceData.specularColor.r, surfaceData.specularColor.g, surfaceData.specularColor.b ) );
				#endif

				#ifdef _DOUBLESIDED_ON
                    float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
				#else
                    float3 doubleSidedConstants = float3( 1.0, 1.0, 1.0 );
				#endif

				float3 normalTS = float3(0.0f, 0.0f, 1.0f);
				normalTS = surfaceDescription.Normal;

                #ifdef DECAL_NORMAL_BLENDING
					normalTS = SurfaceGradientFromTangentSpaceNormalAndFromTBN(normalTS, fragInputs.tangentToWorld[0], fragInputs.tangentToWorld[1]);

                    #if HAVE_DECALS
                    if (_EnableDecals)
                    {
                        float alpha = 1.0;
                        alpha = surfaceDescription.Alpha;

                        DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, fragInputs, alpha);
                        ApplyDecalToSurfaceNormal(decalSurfaceData, fragInputs.tangentToWorld[2], normalTS);
                        ApplyDecalToSurfaceDataNoNormal(decalSurfaceData, surfaceData);
                     }
                    #endif

                    GetNormalWS_SG(fragInputs, normalTS, surfaceData.normalWS, doubleSidedConstants);
                #else
					GetNormalWS(fragInputs, normalTS, surfaceData.normalWS, doubleSidedConstants);

                    #if HAVE_DECALS
                    if (_EnableDecals)
                    {
                        float alpha = 1.0;
                        alpha = surfaceDescription.Alpha;
        
                        DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, fragInputs, alpha);
                        ApplyDecalToSurfaceNormal(decalSurfaceData, surfaceData.normalWS.xyz);
                        ApplyDecalToSurfaceDataNoNormal(decalSurfaceData, surfaceData);
                    }
                    #endif
                #endif

				surfaceData.geomNormalWS = fragInputs.tangentToWorld[2];
                surfaceData.tangentWS = normalize(fragInputs.tangentToWorld[0].xyz );
                surfaceData.tangentWS = Orthonormalize(surfaceData.tangentWS, surfaceData.normalWS);

				bentNormalWS = surfaceData.normalWS;

				#ifdef ASE_BENT_NORMAL
                    GetNormalWS( fragInputs, surfaceDescription.BentNormal, bentNormalWS, doubleSidedConstants );
				#endif

				#ifdef _MATERIAL_FEATURE_ANISOTROPY
                    surfaceData.tangentWS = TransformTangentToWorld(surfaceDescription.Tangent, fragInputs.tangentToWorld);
				#endif

				#if defined(DEBUG_DISPLAY)
				    #if !defined(SHADER_STAGE_RAY_TRACING)
				    if (_DebugMipMapMode != DEBUGMIPMAPMODE_NONE)
				    {
					   #ifdef FRAG_INPUTS_USE_TEXCOORD0
                           surfaceData.baseColor = GET_TEXTURE_STREAMING_DEBUG(posInput.positionSS, fragInputs.texCoord0);
                       #else
                           surfaceData.baseColor = GET_TEXTURE_STREAMING_DEBUG_NO_UV(posInput.positionSS);
                       #endif
					   surfaceData.metallic = 0;
				    }
					#endif
				    ApplyDebugToSurfaceData(fragInputs.tangentToWorld, surfaceData); 
				#endif

                #if defined(_SPECULAR_OCCLUSION_CUSTOM)
                #elif defined(_SPECULAR_OCCLUSION_FROM_AO_BENT_NORMAL)
                    surfaceData.specularOcclusion = GetSpecularOcclusionFromBentAO(V, bentNormalWS, surfaceData.normalWS, surfaceData.ambientOcclusion, PerceptualSmoothnessToPerceptualRoughness(surfaceData.perceptualSmoothness));
                #elif defined(_AMBIENT_OCCLUSION) && defined(_SPECULAR_OCCLUSION_FROM_AO)
                    surfaceData.specularOcclusion = GetSpecularOcclusionFromAmbientOcclusion(ClampNdotV(dot(surfaceData.normalWS, V)), surfaceData.ambientOcclusion, PerceptualSmoothnessToRoughness(surfaceData.perceptualSmoothness));
                #endif

                #ifdef _ENABLE_GEOMETRIC_SPECULAR_AA
                    surfaceData.perceptualSmoothness = GeometricNormalFiltering(surfaceData.perceptualSmoothness, fragInputs.tangentToWorld[2], surfaceDescription.SpecularAAScreenSpaceVariance, surfaceDescription.SpecularAAThreshold);
                #endif
			}

			// Get Surface And BuiltinData
			void GetSurfaceAndBuiltinData(GlobalSurfaceDescription surfaceDescription, FragInputs fragInputs, float3 V, inout PositionInputs posInput, out SurfaceData surfaceData, out BuiltinData builtinData)
			{
				#ifdef LOD_FADE_CROSSFADE
                    LODDitheringTransition(ComputeFadeMaskSeed(V, posInput.positionSS), unity_LODFade.x);
				#endif

                #ifdef _DOUBLESIDED_ON
                    float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
                #else
                    float3 doubleSidedConstants = float3(1.0, 1.0, 1.0);
                #endif  
                ApplyDoubleSidedFlipOrMirror(fragInputs, doubleSidedConstants);

				#ifdef _ALPHATEST_ON
                    DoAlphaTest( surfaceDescription.Alpha, surfaceDescription.AlphaClipThreshold );
				#endif

				#ifdef _ALPHATEST_SHADOW_ON
                    DoAlphaTest( surfaceDescription.Alpha, surfaceDescription.AlphaClipThresholdShadow);
				#endif

				#ifdef _DEPTHOFFSET_ON
                    ApplyDepthOffsetPositionInput(V, surfaceDescription.DepthOffset, GetViewForwardDir(), GetWorldToHClipMatrix(), posInput);
				#endif

				float3 bentNormalWS;
                BuildSurfaceData(fragInputs, surfaceDescription, V, posInput, surfaceData, bentNormalWS);
                InitBuiltinData(posInput, surfaceDescription.Alpha, bentNormalWS, -fragInputs.tangentToWorld[2], fragInputs.texCoord1, fragInputs.texCoord2, builtinData);

				#ifdef _DEPTHOFFSET_ON
                    builtinData.depthOffset = surfaceDescription.DepthOffset;
				#endif

                #ifdef _ALPHATEST_ON
                    builtinData.alphaClipTreshold = surfaceDescription.AlphaClipThreshold;
                #endif

                #ifdef UNITY_VIRTUAL_TEXTURING
                    builtinData.vtPackedFeedback = surfaceDescription.VTPackedFeedback;
                #endif

				#ifdef ASE_BAKEDGI
                    builtinData.bakeDiffuseLighting = surfaceDescription.BakedGI;
				#endif

				#ifdef ASE_BAKEDBACKGI
                    builtinData.backBakeDiffuseLighting = surfaceDescription.BakedBackGI;
				#endif

                builtinData.emissiveColor = surfaceDescription.Emission;

				PostInitBuiltinData(V, posInput, surfaceData, builtinData);
			}

			PackedVaryingsMeshToPS VertexFunction(AttributesMesh inputMesh/*ase_vert_input*/ )
			{
				PackedVaryingsMeshToPS output;

				UNITY_SETUP_INSTANCE_ID(inputMesh);
				UNITY_TRANSFER_INSTANCE_ID(inputMesh, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( output );

				/*ase_vert_code:inputMesh=AttributesMesh;output=PackedVaryingsMeshToPS*/

				#ifdef ASE_ABSOLUTE_VERTEX_POS
				float3 defaultVertexValue = inputMesh.positionOS.xyz;
				#else
				float3 defaultVertexValue = float3( 0, 0, 0 );
				#endif
				float3 vertexValue = /*ase_vert_out:Vertex Offset;Float3;11;-1;_VertexOffset*/ defaultVertexValue /*end*/;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
				inputMesh.positionOS.xyz = vertexValue;
				#else
				inputMesh.positionOS.xyz += vertexValue;
				#endif

				inputMesh.normalOS = /*ase_vert_out:Vertex Normal;Float3;12;-1;_VertexNormal*/ inputMesh.normalOS /*end*/;
				inputMesh.tangentOS = /*ase_vert_out:Vertex Tangent;Float4;27;-1;_VertexTangent*/ inputMesh.tangentOS /*end*/;

				float3 positionRWS = TransformObjectToWorld(inputMesh.positionOS);
				float3 normalWS = TransformObjectToWorldNormal(inputMesh.normalOS);
				float4 tangentWS = float4(TransformObjectToWorldDir(inputMesh.tangentOS.xyz), inputMesh.tangentOS.w);

				output.positionCS = TransformWorldToHClip(positionRWS);
				output.clipPosV = output.positionCS;
				output.positionRWS = positionRWS;
				output.normalWS = normalWS;
				output.tangentWS = tangentWS;
				output.uv1 = inputMesh.uv1;
				output.uv2 = inputMesh.uv2;
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float3 positionOS : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				float4 tangentOS : TANGENT;
				float4 uv1 : TEXCOORD1;
				float4 uv2 : TEXCOORD2;
				/*ase_vcontrol*/
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl Vert ( AttributesMesh v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.positionOS = v.positionOS;
				o.normalOS = v.normalOS;
				o.tangentOS = v.tangentOS;
				o.uv1 = v.uv1;
				o.uv2 = v.uv2;
				/*ase_control_code:v=AttributesMesh;o=VertexControl*/
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = /*ase_inline_begin*/_TessValue/*ase_inline_end*/; float tessMin = /*ase_inline_begin*/_TessMin/*ase_inline_end*/; float tessMax = /*ase_inline_begin*/_TessMax/*ase_inline_end*/;
				float edgeLength = /*ase_inline_begin*/_TessEdgeLength/*ase_inline_end*/; float tessMaxDisp = /*ase_inline_begin*/_TessMaxDisp/*ase_inline_end*/;
				#if (SHADEROPTIONS_CAMERA_RELATIVE_RENDERING != 0)
				float3 cameraPos = 0;
				#else
				float3 cameraPos = _WorldSpaceCameraPos;
				#endif
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(float4(v[0].positionOS,1), float4(v[1].positionOS,1), float4(v[2].positionOS,1), tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), cameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(float4(v[0].positionOS,1), float4(v[1].positionOS,1), float4(v[2].positionOS,1), edgeLength, GetObjectToWorldMatrix(), cameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(float4(v[0].positionOS,1), float4(v[1].positionOS,1), float4(v[2].positionOS,1), edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), cameraPos, _ScreenParams, _FrustumPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			PackedVaryingsMeshToPS DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				AttributesMesh o = (AttributesMesh) 0;
				o.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				o.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				o.tangentOS = patch[0].tangentOS * bary.x + patch[1].tangentOS * bary.y + patch[2].tangentOS * bary.z;
				o.uv1 = patch[0].uv1 * bary.x + patch[1].uv1 * bary.y + patch[2].uv1 * bary.z;
				o.uv2 = patch[0].uv2 * bary.x + patch[1].uv2 * bary.y + patch[2].uv2 * bary.z;
				/*ase_domain_code:patch=VertexControl;o=AttributesMesh;bary=SV_DomainLocation*/
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.positionOS.xyz - patch[i].normalOS * (dot(o.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = /*ase_inline_begin*/_TessPhongStrength/*ase_inline_end*/;
				o.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			PackedVaryingsMeshToPS Vert ( AttributesMesh v )
			{
				return VertexFunction( v );
			}
			#endif

			void Frag( PackedVaryingsMeshToPS packedInput,
						OUTPUT_GBUFFER(outGBuffer)
						#ifdef _DEPTHOFFSET_ON
						, out float outputDepth : DEPTH_OFFSET_SEMANTIC
						#endif
						/*ase_frag_input*/
						)
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( packedInput );
				UNITY_SETUP_INSTANCE_ID( packedInput );

				/*ase_local_var:rwp*/float3 PositionRWS = packedInput.positionRWS;
				/*ase_local_var:wvd*/float3 V = GetWorldSpaceNormalizeViewDir( packedInput.positionRWS );
				/*ase_local_var:wn*/float3 NormalWS = packedInput.normalWS;
				/*ase_local_var:wt*/float4 TangentWS = packedInput.tangentWS;
				/*ase_local_var:sp*/float4 ClipPos = packedInput.clipPosV;
				/*ase_local_var:spu*/float4 ScreenPos = ComputeScreenPos( packedInput.clipPosV, _ProjectionParams.x );

				FragInputs input;
				ZERO_INITIALIZE(FragInputs, input);
				input.positionSS = packedInput.positionCS;
				input.positionRWS = PositionRWS;
				input.tangentToWorld = BuildTangentToWorld(TangentWS, NormalWS);
				input.texCoord1 = packedInput.uv1.xyzw;
				input.texCoord2 = packedInput.uv2.xyzw;

				#if _DOUBLESIDED_ON && SHADER_STAGE_FRAGMENT
				input.isFrontFace = IS_FRONT_VFACE( packedInput.cullFace, true, false);
				#elif SHADER_STAGE_FRAGMENT
				#if defined(ASE_NEED_CULLFACE)
				input.isFrontFace = IS_FRONT_VFACE( packedInput.cullFace, true, false );
				#endif
				#endif
				/*ase_local_var:vf*/half IsFrontFace = input.isFrontFace;

				PositionInputs posInput = GetPositionInput(input.positionSS.xy, _ScreenSize.zw, input.positionSS.z, input.positionSS.w, input.positionRWS);
				
				SurfaceData surfaceData;
				BuiltinData builtinData;

				GlobalSurfaceDescription surfaceDescription = (GlobalSurfaceDescription)0;
				/*ase_frag_code:packedInput=PackedVaryingsMeshToPS*/
				surfaceDescription.BaseColor = /*ase_frag_out:Base Color;Float3;0;-1;_BaseColor*/float3( 0.5, 0.5, 0.5 )/*end*/;
				surfaceDescription.Normal = /*ase_frag_out:Normal;Float3;1;-1;_Normal*/float3( 0, 0, 1 )/*end*/;
				surfaceDescription.BentNormal = /*ase_frag_out:Bent Normal;Float3;2;-1;_BentNormal*/float3( 0, 0, 1 )/*end*/;
				surfaceDescription.CoatMask = /*ase_frag_out:Coat Mask;Float;3;-1;_CoatMask*/0/*end*/;
				surfaceDescription.Metallic = /*ase_frag_out:Metallic;Float;4;-1;_Metallic*/0/*end*/;

				#ifdef _MATERIAL_FEATURE_SPECULAR_COLOR
				surfaceDescription.Specular = /*ase_frag_out:Specular;Float3;5;-1;_Specular*/0/*end*/;
				#endif

				surfaceDescription.Emission = /*ase_frag_out:Emission;Float3;6;-1;_Emission*/0/*end*/;
				surfaceDescription.Smoothness = /*ase_frag_out:Smoothness;Float;7;-1;_Smoothness*/0.5/*end*/;
				surfaceDescription.Occlusion = /*ase_frag_out:Occlusion;Float;8;-1;_Occlusion*/1/*end*/;
				surfaceDescription.Alpha = /*ase_frag_out:Alpha;Float;9;-1;_Alpha*/1/*end*/;

				#ifdef _ALPHATEST_ON
				surfaceDescription.AlphaClipThreshold = /*ase_frag_out:Alpha Clip Threshold;Float;10;-1;_AlphaClip*/_AlphaCutoff/*end*/;
				#endif

				#ifdef _ALPHATEST_SHADOW_ON
				surfaceDescription.AlphaClipThresholdShadow = /*ase_frag_out:Alpha Clip Threshold Shadow;Float;30;-1;_AlphaClipShadow*/0.5/*end*/;
				#endif

				surfaceDescription.AlphaClipThresholdDepthPrepass = /*ase_frag_out:Alpha Clip Threshold Depth Prepass;Float;33;-1;_AlphaClipDepthPrepass*/0.5/*end*/;
				surfaceDescription.AlphaClipThresholdDepthPostpass = /*ase_frag_out:Alpha Clip Threshold Depth Postpass;Float;34;-1;_AlphaClipDepthPostpass*/0.5/*end*/;

				#ifdef _ENABLE_GEOMETRIC_SPECULAR_AA
				surfaceDescription.SpecularAAScreenSpaceVariance = /*ase_frag_out:Specular AA Screen Space Variance;Float;13;-1;_SpecularAAScreenSpaceVariance*/0/*end*/;
				surfaceDescription.SpecularAAThreshold = /*ase_frag_out:Specular AA Threshold;Float;14;-1;_SpecularAAThreshold*/0/*end*/;
				#endif

				#ifdef _SPECULAR_OCCLUSION_CUSTOM
				surfaceDescription.SpecularOcclusion = /*ase_frag_out:Specular Occlusion;Float;15;-1;_SpecularOcclusion*/0/*end*/;
				#endif

				#if defined(_HAS_REFRACTION) || defined(_MATERIAL_FEATURE_TRANSMISSION)
				surfaceDescription.Thickness = /*ase_frag_out:Thickness;Float;16;-1;_Thickness*/0/*end*/;
				#endif

				#ifdef _HAS_REFRACTION
				surfaceDescription.RefractionIndex = /*ase_frag_out:Refraction Index;Float;17;-1;_RefractionIndex*/1/*end*/;
				surfaceDescription.RefractionColor = /*ase_frag_out:Refraction Color;Float3;18;-1;_RefractionColor*/float3( 1, 1, 1 )/*end*/;
				surfaceDescription.RefractionDistance = /*ase_frag_out:Refraction Distance;Float;19;-1;_RefractionDistance*/0/*end*/;
				#endif

				#ifdef _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
				surfaceDescription.SubsurfaceMask = /*ase_frag_out:Subsurface Mask;Float;20;-1;_SubsurfaceMask*/1/*end*/;
				#endif

				#ifdef _MATERIAL_FEATURE_TRANSMISSION
				surfaceDescription.TransmissionMask = /*ase_frag_out:Transmission Mask;Float;61;-1;_TransmissionMask*/1/*end*/;
				#endif

				#if defined( _MATERIAL_FEATURE_SUBSURFACE_SCATTERING ) || defined( _MATERIAL_FEATURE_TRANSMISSION )
				surfaceDescription.DiffusionProfile = /*ase_frag_out:Diffusion Profile;Float;62;-1;_DiffusionProfile*/0/*end*/;
				#endif

				#ifdef _MATERIAL_FEATURE_ANISOTROPY
				surfaceDescription.Anisotropy = /*ase_frag_out:Anisotropy;Float;22;-1;_Anisotropy*/1/*end*/;
				surfaceDescription.Tangent = /*ase_frag_out:Tangent;Float3;23;-1;_Tangent*/float3( 1, 0, 0 )/*end*/;
				#endif

				#ifdef _MATERIAL_FEATURE_IRIDESCENCE
				surfaceDescription.IridescenceMask = /*ase_frag_out:Iridescence Mask;Float;24;-1;_IridescenceMask*/0/*end*/;
				surfaceDescription.IridescenceThickness = /*ase_frag_out:Iridescence Thickness;Float;25;-1;_IridescenceThickness*/0/*end*/;
				#endif

				#ifdef ASE_BAKEDGI
				surfaceDescription.BakedGI = /*ase_frag_out:Baked GI;Float3;31;-1;_BakedGI*/0/*end*/;
				#endif
				#ifdef ASE_BAKEDBACKGI
				surfaceDescription.BakedBackGI = /*ase_frag_out:Baked Back GI;Float3;32;-1;_BakedBackGI*/0/*end*/;
				#endif

				#ifdef _DEPTHOFFSET_ON
				surfaceDescription.DepthOffset = /*ase_frag_out:Depth Offset;Float;26;-1;_DepthOffset*/0/*end*/;
				#endif

				#ifdef UNITY_VIRTUAL_TEXTURING
				surfaceDescription.VTPackedFeedback = float4(1.0f,1.0f,1.0f,1.0f);
				#endif

				GetSurfaceAndBuiltinData( surfaceDescription, input, V, posInput, surfaceData, builtinData );
				ENCODE_INTO_GBUFFER( surfaceData, builtinData, posInput.positionSS, outGBuffer );
				#ifdef _DEPTHOFFSET_ON
				outputDepth = posInput.deviceDepth;
				#endif
			}

			ENDHLSL
		}

		/*ase_pass*/
		Pass
		{
			/*ase_hide_pass*/
			Name "META"
			Tags 
			{ 
				"LightMode" = "Meta" 
		    }

			Cull Off

			HLSLPROGRAM
			#pragma shader_feature _ EDITOR_VISUALIZATION
			#pragma multi_compile _ DOTS_INSTANCING_ON

            #pragma shader_feature _ _SURFACE_TYPE_TRANSPARENT
            #pragma shader_feature_local _ _TRANSPARENT_WRITES_MOTION_VEC _TRANSPARENT_REFRACTIVE_SORT
            #pragma shader_feature_local_fragment _ _ENABLE_FOG_ON_TRANSPARENT

			#pragma vertex Vert
			#pragma fragment Frag

            #define SHADERPASS SHADERPASS_LIGHT_TRANSPORT
            #define SCENEPICKINGPASS 1

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/GeometricTools.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Tessellation.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
            #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"

			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/PickingSpaceTransforms.hlsl"

            #ifdef RAYTRACING_SHADER_GRAPH_DEFAULT
                #define RAYTRACING_SHADER_GRAPH_HIGH
            #endif
        
            #ifdef RAYTRACING_SHADER_GRAPH_RAYTRACED
                #define RAYTRACING_SHADER_GRAPH_LOW
            #endif

            #ifndef SHADER_UNLIT
            #if defined(_DOUBLESIDED_ON) && !defined(VARYINGS_NEED_CULLFACE)
                #define VARYINGS_NEED_CULLFACE
            #endif
            #endif

			#if defined(_DOUBLESIDED_ON) && !defined(ASE_NEED_CULLFACE)
			    #define ASE_NEED_CULLFACE 1
			#endif

            #if _MATERIAL_FEATURE_COLORED_TRANSMISSION
            #undef _MATERIAL_FEATURE_CLEAR_COAT
            #endif

		    #if defined(_MATERIAL_FEATURE_SUBSURFACE_SCATTERING) && !defined(_SURFACE_TYPE_TRANSPARENT)
			#define OUTPUT_SPLIT_LIGHTING
		    #endif

            #if (SHADERPASS == SHADERPASS_PATH_TRACING) && !defined(_DOUBLESIDED_ON) && (defined(_REFRACTION_PLANE) || defined(_REFRACTION_SPHERE))
            #undef  _REFRACTION_PLANE
            #undef  _REFRACTION_SPHERE
            #define _REFRACTION_THIN
            #endif

            #if SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_PREPASS
            #if !defined(_DISABLE_SSR_TRANSPARENT) && !defined(SHADER_UNLIT)
                #define WRITE_NORMAL_BUFFER
            #endif
            #endif

            #if SHADERPASS == SHADERPASS_MOTION_VECTORS && defined(WRITE_DECAL_BUFFER_AND_RENDERING_LAYER)
                #define WRITE_DECAL_BUFFER
            #endif

            #ifndef DEBUG_DISPLAY
                #if !defined(_SURFACE_TYPE_TRANSPARENT)
                    #if SHADERPASS == SHADERPASS_FORWARD
                    #define SHADERPASS_FORWARD_BYPASS_ALPHA_TEST
                    #elif SHADERPASS == SHADERPASS_GBUFFER
                    #define SHADERPASS_GBUFFER_BYPASS_ALPHA_TEST
                    #endif
                #endif
            #endif

            #if defined(SHADER_LIT) && !defined(_SURFACE_TYPE_TRANSPARENT)
                #define _DEFERRED_CAPABLE_MATERIAL
            #endif
        
            #if (defined(_TRANSPARENT_WRITES_MOTION_VEC) || defined(_TRANSPARENT_REFRACTIVE_SORT)) && defined(_SURFACE_TYPE_TRANSPARENT)
                #define _WRITE_TRANSPARENT_MOTION_VECTOR
            #endif

			CBUFFER_START( UnityPerMaterial )
			float4 _EmissionColor;
			float _AlphaCutoff;
			float _RenderQueueType;
			#ifdef _ADD_PRECOMPUTED_VELOCITY
			    float _AddPrecomputedVelocity;
			#endif
			float _StencilRef;
			float _StencilWriteMask;
			float _StencilRefDepth;
			float _StencilWriteMaskDepth;
			float _StencilRefMV;
			float _StencilWriteMaskMV;
			float _StencilRefDistortionVec;
			float _StencilWriteMaskDistortionVec;
			float _StencilWriteMaskGBuffer;
			float _StencilRefGBuffer;
			float _ZTestGBuffer;
			float _RequireSplitLighting;
			float _ReceivesSSR;
			float _SurfaceType;
			float _BlendMode;
            #ifdef SUPPORT_BLENDMODE_PRESERVE_SPECULAR_LIGHTING
			    float _EnableBlendModePreserveSpecularLighting;
            #endif
			float _SrcBlend;
			float _DstBlend;
			float _DstBlend2;
			float _AlphaSrcBlend;
			float _AlphaDstBlend;
			float _ZWrite;
			float _TransparentZWrite;
			float _CullMode;
			float _TransparentSortPriority;
			float _EnableFogOnTransparent;
			float _CullModeForward;
			float _TransparentCullMode;
			float _ZTestDepthEqualForOpaque;
			float _ZTestTransparent;
			float _TransparentBackfaceEnable;
			float _AlphaCutoffEnable;
			float _UseShadowThreshold;
			float _DoubleSidedEnable;
			float _DoubleSidedNormalMode;
			float4 _DoubleSidedConstants;
			#ifdef ASE_TESSELLATION
			    float _TessPhongStrength;
			    float _TessValue;
			    float _TessMin;
			    float _TessMax;
			    float _TessEdgeLength;
			    float _TessMaxDisp;
			#endif
			UNITY_TEXTURE_STREAMING_DEBUG_VARS;
			CBUFFER_END

            #ifdef SCENEPICKINGPASS
			float4 _SelectionID;
            #endif

            #ifdef SCENESELECTIONPASS
			int _ObjectId;
			int _PassValue;
            #endif

			/*ase_globals*/

            #ifdef DEBUG_DISPLAY
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Debug/DebugDisplay.hlsl"
            #endif

			#if SHADERPASS == SHADERPASS_LIGHT_TRANSPORT
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/PickingSpaceTransforms.hlsl"
			#endif

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/MetaPass.hlsl"

			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/NormalSurfaceGradient.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/BuiltinUtilities.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialUtilities.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphFunctions.hlsl"

            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/DecalUtilities.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitDecalData.hlsl"

        	#ifdef HAVE_VFX_MODIFICATION
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/VisualEffectVertex.hlsl"
        	#endif

			/*ase_pragma*/

			struct AttributesMesh
			{
				float3 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 tangentOS : TANGENT;
				float4 uv0 : TEXCOORD0;
				float4 uv1 : TEXCOORD1;
				float4 uv2 : TEXCOORD2;
				float4 uv3 : TEXCOORD3;
				/*ase_vdata:p=p;n=n;t=t;uv0=tc0;uv1=tc1;uv2=tc2;uv3=tc3*/
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryingsMeshToPS
			{
				SV_POSITION_QUALIFIERS float4 positionCS : SV_Position;
				#ifdef EDITOR_VISUALIZATION
				float2 VizUV : TEXCOORD0;
				float4 LightCoord : TEXCOORD1;
				#endif
				/*ase_interp(2,):sp=sp.xyzw*/
				UNITY_VERTEX_INPUT_INSTANCE_ID
				#if defined(SHADER_STAGE_FRAGMENT) && defined(ASE_NEED_CULLFACE)
				FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
				#endif
			};

			/*ase_funcs*/

			void BuildSurfaceData(FragInputs fragInputs, inout GlobalSurfaceDescription surfaceDescription, float3 V, PositionInputs posInput, out SurfaceData surfaceData, out float3 bentNormalWS)
			{
				ZERO_INITIALIZE(SurfaceData, surfaceData);

				surfaceData.specularOcclusion = 1.0;
				surfaceData.thickness = 0.0;

				surfaceData.baseColor =					surfaceDescription.BaseColor;
				surfaceData.perceptualSmoothness =		surfaceDescription.Smoothness;
				surfaceData.ambientOcclusion =			surfaceDescription.Occlusion;
				surfaceData.metallic =					surfaceDescription.Metallic;
				surfaceData.coatMask =					surfaceDescription.CoatMask;

				#ifdef _SPECULAR_OCCLUSION_CUSTOM
				surfaceData.specularOcclusion =			surfaceDescription.SpecularOcclusion;
				#endif

				#ifdef _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
				surfaceData.subsurfaceMask =			surfaceDescription.SubsurfaceMask;
				#endif

				#if defined(_HAS_REFRACTION) || defined(_MATERIAL_FEATURE_TRANSMISSION)
				surfaceData.thickness = 				surfaceDescription.Thickness;
				#endif

				#ifdef _MATERIAL_FEATURE_TRANSMISSION
				surfaceData.transmissionMask =			surfaceDescription.TransmissionMask;
				#endif

				#ifdef _MATERIAL_FEATURE_COLORED_TRANSMISSION
				surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_TRANSMISSION;
				surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_COLORED_TRANSMISSION;
				#endif

				#if defined( _MATERIAL_FEATURE_SUBSURFACE_SCATTERING ) || defined( _MATERIAL_FEATURE_TRANSMISSION )
				surfaceData.diffusionProfileHash =		asuint(surfaceDescription.DiffusionProfile);
				#endif

				#ifdef _MATERIAL_FEATURE_SPECULAR_COLOR
				surfaceData.specularColor =				surfaceDescription.Specular;
				#endif

				#ifdef _MATERIAL_FEATURE_ANISOTROPY
				surfaceData.anisotropy =				surfaceDescription.Anisotropy;
				#endif

				#ifdef _MATERIAL_FEATURE_IRIDESCENCE
				surfaceData.iridescenceMask =			surfaceDescription.IridescenceMask;
				surfaceData.iridescenceThickness =		surfaceDescription.IridescenceThickness;
				#endif

				// refraction
                #if defined(_REFRACTION_PLANE) || defined(_REFRACTION_SPHERE) || defined(_REFRACTION_THIN)
                    if (_EnableSSRefraction)
                    {
                        surfaceData.ior =                       surfaceDescription.RefractionIndex;
                        surfaceData.transmittanceColor =        surfaceDescription.RefractionColor;
                        surfaceData.atDistance =                surfaceDescription.RefractionDistance;
        
                        surfaceData.transmittanceMask = (1.0 - surfaceDescription.Alpha);
                        surfaceDescription.Alpha = 1.0;
                    }
                    else
                    {
                        surfaceData.ior = 1.0;
                        surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                        surfaceData.atDistance = 1.0;
                        surfaceData.transmittanceMask = 0.0;
                        surfaceDescription.Alpha = 1.0;
                    }
                #else
                    surfaceData.ior = 1.0;
                    surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                    surfaceData.atDistance = 1.0;
                    surfaceData.transmittanceMask = 0.0;
                #endif

				surfaceData.materialFeatures = MATERIALFEATUREFLAGS_LIT_STANDARD;

                #ifdef _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SUBSURFACE_SCATTERING;
				#endif

				#ifdef _MATERIAL_FEATURE_TRANSMISSION
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_TRANSMISSION;
				#endif

				#ifdef _MATERIAL_FEATURE_COLORED_TRANSMISSION
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_TRANSMISSION;
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_COLORED_TRANSMISSION;
				#endif

                #ifdef _MATERIAL_FEATURE_ANISOTROPY
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_ANISOTROPY;
                    surfaceData.normalWS = float3(0, 1, 0);
                #endif

				#ifdef _MATERIAL_FEATURE_CLEAR_COAT
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_CLEAR_COAT;
				#endif

				#ifdef _MATERIAL_FEATURE_IRIDESCENCE
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_IRIDESCENCE;
				#endif

                #ifdef _MATERIAL_FEATURE_SPECULAR_COLOR
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SPECULAR_COLOR;
                #endif

				#if defined (_MATERIAL_FEATURE_SPECULAR_COLOR) && defined (_ENERGY_CONSERVING_SPECULAR)
                    surfaceData.baseColor *= ( 1.0 - Max3( surfaceData.specularColor.r, surfaceData.specularColor.g, surfaceData.specularColor.b ) );
				#endif

				#ifdef _DOUBLESIDED_ON
				float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
				#else
				float3 doubleSidedConstants = float3( 1.0, 1.0, 1.0 );
				#endif

				float3 normalTS = float3(0.0f, 0.0f, 1.0f);
				normalTS = surfaceDescription.Normal;

                #ifdef DECAL_NORMAL_BLENDING
					normalTS = SurfaceGradientFromTangentSpaceNormalAndFromTBN(normalTS, fragInputs.tangentToWorld[0], fragInputs.tangentToWorld[1]);

                    #if HAVE_DECALS
                    if (_EnableDecals)
                    {
                        float alpha = 1.0;
                        alpha = surfaceDescription.Alpha;

                        DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, fragInputs, alpha);
                        ApplyDecalToSurfaceNormal(decalSurfaceData, fragInputs.tangentToWorld[2], normalTS);
                        ApplyDecalToSurfaceDataNoNormal(decalSurfaceData, surfaceData);
                     }
                    #endif

                    GetNormalWS_SG(fragInputs, normalTS, surfaceData.normalWS, doubleSidedConstants);
                #else
					GetNormalWS(fragInputs, normalTS, surfaceData.normalWS, doubleSidedConstants);

                    #if HAVE_DECALS
                    if (_EnableDecals)
                    {
                        float alpha = 1.0;
                        alpha = surfaceDescription.Alpha;
        
                        DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, fragInputs, alpha);
                        ApplyDecalToSurfaceNormal(decalSurfaceData, surfaceData.normalWS.xyz);
                        ApplyDecalToSurfaceDataNoNormal(decalSurfaceData, surfaceData);
                    }
                    #endif
                #endif

				surfaceData.geomNormalWS = fragInputs.tangentToWorld[2];
                surfaceData.tangentWS = normalize(fragInputs.tangentToWorld[0].xyz );
                surfaceData.tangentWS = Orthonormalize(surfaceData.tangentWS, surfaceData.normalWS);

				bentNormalWS = surfaceData.normalWS;

				#ifdef ASE_BENT_NORMAL
                    GetNormalWS( fragInputs, surfaceDescription.BentNormal, bentNormalWS, doubleSidedConstants );
				#endif

				#ifdef _MATERIAL_FEATURE_ANISOTROPY
                    surfaceData.tangentWS = TransformTangentToWorld(surfaceDescription.Tangent, fragInputs.tangentToWorld);
				#endif

				#if defined(DEBUG_DISPLAY)
				    #if !defined(SHADER_STAGE_RAY_TRACING)
				    if (_DebugMipMapMode != DEBUGMIPMAPMODE_NONE)
				    {
					   #ifdef FRAG_INPUTS_USE_TEXCOORD0
                           surfaceData.baseColor = GET_TEXTURE_STREAMING_DEBUG(posInput.positionSS, fragInputs.texCoord0);
                       #else
                           surfaceData.baseColor = GET_TEXTURE_STREAMING_DEBUG_NO_UV(posInput.positionSS);
                       #endif
					   surfaceData.metallic = 0;
				    }
					#endif
				    ApplyDebugToSurfaceData(fragInputs.tangentToWorld, surfaceData); 
				#endif

                #if defined(_SPECULAR_OCCLUSION_CUSTOM)
                #elif defined(_SPECULAR_OCCLUSION_FROM_AO_BENT_NORMAL)
                    surfaceData.specularOcclusion = GetSpecularOcclusionFromBentAO(V, bentNormalWS, surfaceData.normalWS, surfaceData.ambientOcclusion, PerceptualSmoothnessToPerceptualRoughness(surfaceData.perceptualSmoothness));
                #elif defined(_AMBIENT_OCCLUSION) && defined(_SPECULAR_OCCLUSION_FROM_AO)
                    surfaceData.specularOcclusion = GetSpecularOcclusionFromAmbientOcclusion(ClampNdotV(dot(surfaceData.normalWS, V)), surfaceData.ambientOcclusion, PerceptualSmoothnessToRoughness(surfaceData.perceptualSmoothness));
                #endif

                #ifdef _ENABLE_GEOMETRIC_SPECULAR_AA
                    surfaceData.perceptualSmoothness = GeometricNormalFiltering(surfaceData.perceptualSmoothness, fragInputs.tangentToWorld[2], surfaceDescription.SpecularAAScreenSpaceVariance, surfaceDescription.SpecularAAThreshold);
                #endif
			}

			// Get Surface And BuiltinData
			void GetSurfaceAndBuiltinData(GlobalSurfaceDescription surfaceDescription, FragInputs fragInputs, float3 V, inout PositionInputs posInput, out SurfaceData surfaceData, out BuiltinData builtinData)
			{
				#ifdef LOD_FADE_CROSSFADE
                    LODDitheringTransition(ComputeFadeMaskSeed(V, posInput.positionSS), unity_LODFade.x);
				#endif

                #ifdef _DOUBLESIDED_ON
                    float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
                #else
                    float3 doubleSidedConstants = float3(1.0, 1.0, 1.0);
                #endif
                ApplyDoubleSidedFlipOrMirror(fragInputs, doubleSidedConstants);

				#ifdef _ALPHATEST_ON
                    DoAlphaTest( surfaceDescription.Alpha, surfaceDescription.AlphaClipThreshold );
				#endif

				#ifdef _ALPHATEST_SHADOW_ON
                    DoAlphaTest( surfaceDescription.Alpha, surfaceDescription.AlphaClipThresholdShadow);
				#endif

				#ifdef _DEPTHOFFSET_ON
                    ApplyDepthOffsetPositionInput(V, surfaceDescription.DepthOffset, GetViewForwardDir(), GetWorldToHClipMatrix(), posInput);
				#endif

                float3 bentNormalWS;
                BuildSurfaceData(fragInputs, surfaceDescription, V, posInput, surfaceData, bentNormalWS);
                InitBuiltinData(posInput, surfaceDescription.Alpha, bentNormalWS, -fragInputs.tangentToWorld[2], fragInputs.texCoord1, fragInputs.texCoord2, builtinData);

				#ifdef _DEPTHOFFSET_ON
                    builtinData.depthOffset = surfaceDescription.DepthOffset;
				#endif

                #ifdef _ALPHATEST_ON
                    builtinData.alphaClipTreshold = surfaceDescription.AlphaClipThreshold;
                #endif

                #ifdef UNITY_VIRTUAL_TEXTURING
                    builtinData.vtPackedFeedback = surfaceDescription.VTPackedFeedback;
                #endif

				#ifdef ASE_BAKEDGI
                    builtinData.bakeDiffuseLighting = surfaceDescription.BakedGI;
				#endif

				#ifdef ASE_BAKEDBACKGI
                    builtinData.backBakeDiffuseLighting = surfaceDescription.BakedBackGI;
				#endif

                builtinData.emissiveColor = surfaceDescription.Emission;

                PostInitBuiltinData(V, posInput, surfaceData, builtinData);
			}

			PackedVaryingsMeshToPS VertexFunction(AttributesMesh inputMesh /*ase_vert_input*/ )
			{
				PackedVaryingsMeshToPS output;

				UNITY_SETUP_INSTANCE_ID(inputMesh);
				UNITY_TRANSFER_INSTANCE_ID(inputMesh, output);

				/*ase_vert_code:inputMesh=AttributesMesh;output=PackedVaryingsMeshToPS*/

				#ifdef ASE_ABSOLUTE_VERTEX_POS
				float3 defaultVertexValue = inputMesh.positionOS.xyz;
				#else
				float3 defaultVertexValue = float3( 0, 0, 0 );
				#endif
				float3 vertexValue = /*ase_vert_out:Vertex Offset;Float3;11;-1;_VertexOffset*/ defaultVertexValue /*end*/;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
				inputMesh.positionOS.xyz = vertexValue;
				#else
				inputMesh.positionOS.xyz += vertexValue;
				#endif

				inputMesh.normalOS = /*ase_vert_out:Vertex Normal;Float3;12;-1;_VertexNormal*/ inputMesh.normalOS /*end*/;
				inputMesh.tangentOS = /*ase_vert_out:Vertex Tangent;Float4;26;-1;_VertexTangent*/ inputMesh.tangentOS /*end*/;

				output.positionCS = UnityMetaVertexPosition(inputMesh.positionOS, inputMesh.uv1.xy, inputMesh.uv2.xy, unity_LightmapST, unity_DynamicLightmapST);

				#ifdef EDITOR_VISUALIZATION
					float2 vizUV = 0;
					float4 lightCoord = 0;
					UnityEditorVizData(inputMesh.positionOS.xyz, inputMesh.uv0.xy, inputMesh.uv1.xy, inputMesh.uv2.xy, vizUV, lightCoord);

					output.VizUV.xy = vizUV;
					output.LightCoord = lightCoord;
				#endif

				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float3 positionOS : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				float4 tangentOS : TANGENT;
				float4 uv0 : TEXCOORD0;
				float4 uv1 : TEXCOORD1;
				float4 uv2 : TEXCOORD2;
				float4 uv3 : TEXCOORD3;
				/*ase_vcontrol*/
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl Vert ( AttributesMesh v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.positionOS = v.positionOS;
				o.normalOS = v.normalOS;
				o.tangentOS = v.tangentOS;
				o.uv0 = v.uv0;
				o.uv1 = v.uv1;
				o.uv2 = v.uv2;
				o.uv3 = v.uv3;
				/*ase_control_code:v=AttributesMesh;o=VertexControl*/
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = /*ase_inline_begin*/_TessValue/*ase_inline_end*/; float tessMin = /*ase_inline_begin*/_TessMin/*ase_inline_end*/; float tessMax = /*ase_inline_begin*/_TessMax/*ase_inline_end*/;
				float edgeLength = /*ase_inline_begin*/_TessEdgeLength/*ase_inline_end*/; float tessMaxDisp = /*ase_inline_begin*/_TessMaxDisp/*ase_inline_end*/;
				#if (SHADEROPTIONS_CAMERA_RELATIVE_RENDERING != 0)
				float3 cameraPos = 0;
				#else
				float3 cameraPos = _WorldSpaceCameraPos;
				#endif
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(float4(v[0].positionOS,1), float4(v[1].positionOS,1), float4(v[2].positionOS,1), tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), cameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(float4(v[0].positionOS,1), float4(v[1].positionOS,1), float4(v[2].positionOS,1), edgeLength, GetObjectToWorldMatrix(), cameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(float4(v[0].positionOS,1), float4(v[1].positionOS,1), float4(v[2].positionOS,1), edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), cameraPos, _ScreenParams, _FrustumPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			PackedVaryingsMeshToPS DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				AttributesMesh o = (AttributesMesh) 0;
				o.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				o.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				o.tangentOS = patch[0].tangentOS * bary.x + patch[1].tangentOS * bary.y + patch[2].tangentOS * bary.z;
				o.uv0 = patch[0].uv0 * bary.x + patch[1].uv0 * bary.y + patch[2].uv0 * bary.z;
				o.uv1 = patch[0].uv1 * bary.x + patch[1].uv1 * bary.y + patch[2].uv1 * bary.z;
				o.uv2 = patch[0].uv2 * bary.x + patch[1].uv2 * bary.y + patch[2].uv2 * bary.z;
				o.uv3 = patch[0].uv3 * bary.x + patch[1].uv3 * bary.y + patch[2].uv3 * bary.z;
				/*ase_domain_code:patch=VertexControl;o=AttributesMesh;bary=SV_DomainLocation*/
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.positionOS.xyz - patch[i].normalOS * (dot(o.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = /*ase_inline_begin*/_TessPhongStrength/*ase_inline_end*/;
				o.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			PackedVaryingsMeshToPS Vert ( AttributesMesh v )
			{
				return VertexFunction( v );
			}
			#endif

			float4 Frag(PackedVaryingsMeshToPS packedInput /*ase_frag_input*/ ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( packedInput );
				FragInputs input;
				ZERO_INITIALIZE(FragInputs, input);
				input.tangentToWorld = k_identity3x3;
				input.positionSS = packedInput.positionCS;

				#if _DOUBLESIDED_ON && SHADER_STAGE_FRAGMENT
				input.isFrontFace = IS_FRONT_VFACE( packedInput.cullFace, true, false);
				#elif SHADER_STAGE_FRAGMENT
				#if defined(ASE_NEED_CULLFACE)
				input.isFrontFace = IS_FRONT_VFACE(packedInput.cullFace, true, false);
				#endif
				#endif
				/*ase_local_var:vf*/half isFrontFace = input.isFrontFace;

				PositionInputs posInput = GetPositionInput(input.positionSS.xy, _ScreenSize.zw, input.positionSS.z, input.positionSS.w, input.positionRWS);
				float3 V = float3(1.0, 1.0, 1.0);

				SurfaceData surfaceData;
				BuiltinData builtinData;
				GlobalSurfaceDescription surfaceDescription = (GlobalSurfaceDescription)0;
				/*ase_frag_code:packedInput=PackedVaryingsMeshToPS*/
				surfaceDescription.BaseColor = /*ase_frag_out:Base Color;Float3;0;-1;_BaseColor*/float3( 0.5, 0.5, 0.5 )/*end*/;
				surfaceDescription.Normal = /*ase_frag_out:Normal;Float3;1;-1;_Normal*/float3( 0, 0, 1 )/*end*/;
				surfaceDescription.BentNormal = /*ase_frag_out:Bent Normal;Float3;2;-1;_BentNormal*/float3( 0, 0, 1 )/*end*/;
				surfaceDescription.CoatMask = /*ase_frag_out:Coat Mask;Float;3;-1;_CoatMask*/0/*end*/;
				surfaceDescription.Metallic = /*ase_frag_out:Metallic;Float;4;-1;_Metallic*/0/*end*/;

				#ifdef _MATERIAL_FEATURE_SPECULAR_COLOR
				surfaceDescription.Specular = /*ase_frag_out:Specular;Float3;5;-1;_Specular*/0/*end*/;
				#endif

				surfaceDescription.Emission = /*ase_frag_out:Emission;Float3;6;-1;_Emission*/0/*end*/;
				surfaceDescription.Smoothness = /*ase_frag_out:Smoothness;Float;7;-1;_Smoothness*/0.5/*end*/;
				surfaceDescription.Occlusion = /*ase_frag_out:Occlusion;Float;8;-1;_Occlusion*/1/*end*/;
				surfaceDescription.Alpha = /*ase_frag_out:Alpha;Float;9;-1;_Alpha*/1/*end*/;

				#ifdef _ALPHATEST_ON
				surfaceDescription.AlphaClipThreshold = /*ase_frag_out:Alpha Clip Threshold;Float;10;-1;_AlphaClip*/_AlphaCutoff/*end*/;
				#endif

				#ifdef _ENABLE_GEOMETRIC_SPECULAR_AA
				surfaceDescription.SpecularAAScreenSpaceVariance = /*ase_frag_out:Specular AA Screen Space Variance;Float;13;-1;_SpecularAAScreenSpaceVariance*/0/*end*/;
				surfaceDescription.SpecularAAThreshold = /*ase_frag_out:Specular AA Threshold;Float;14;-1;_SpecularAAThreshold*/0/*end*/;
				#endif

				#ifdef _SPECULAR_OCCLUSION_CUSTOM
				surfaceDescription.SpecularOcclusion = /*ase_frag_out:Specular Occlusion;Float;15;-1;_SpecularOcclusion*/0/*end*/;
				#endif

				#if defined(_HAS_REFRACTION) || defined(_MATERIAL_FEATURE_TRANSMISSION)
				surfaceDescription.Thickness = /*ase_frag_out:Thickness;Float;16;-1;_Thickness*/1/*end*/;
				#endif

				#ifdef _HAS_REFRACTION
				surfaceDescription.RefractionIndex = /*ase_frag_out:Refraction Index;Float;17;-1;_RefractionIndex*/1/*end*/;
				surfaceDescription.RefractionColor = /*ase_frag_out:Refraction Color;Float3;18;-1;_RefractionColor*/float3( 1, 1, 1 )/*end*/;
				surfaceDescription.RefractionDistance = /*ase_frag_out:Refraction Distance;Float;19;-1;_RefractionDistance*/0/*end*/;
				#endif

				#ifdef _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
				surfaceDescription.SubsurfaceMask = /*ase_frag_out:Subsurface Mask;Float;20;-1;_SubsurfaceMask*/1/*end*/;
				#endif

				#ifdef _MATERIAL_FEATURE_TRANSMISSION
				surfaceDescription.TransmissionMask = /*ase_frag_out:Transmission Mask;Float;61;-1;_TransmissionMask*/1/*end*/;
				#endif

				#if defined( _MATERIAL_FEATURE_SUBSURFACE_SCATTERING ) || defined( _MATERIAL_FEATURE_TRANSMISSION )
				surfaceDescription.DiffusionProfile = /*ase_frag_out:Diffusion Profile;Float;62;-1;_DiffusionProfile*/0/*end*/;
				#endif

				#ifdef _MATERIAL_FEATURE_ANISOTROPY
				surfaceDescription.Anisotropy = /*ase_frag_out:Anisotropy;Float;22;-1;_Anisotropy*/1/*end*/;
				surfaceDescription.Tangent = /*ase_frag_out:Tangent;Float3;23;-1;_Tangent*/float3( 1, 0, 0 )/*end*/;
				#endif

				#ifdef _MATERIAL_FEATURE_IRIDESCENCE
				surfaceDescription.IridescenceMask = /*ase_frag_out:Iridescence Mask;Float;24;-1;_IridescenceMask*/0/*end*/;
				surfaceDescription.IridescenceThickness = /*ase_frag_out:Iridescence Thickness;Float;25;-1;_IridescenceThickness*/0/*end*/;
				#endif

				GetSurfaceAndBuiltinData(surfaceDescription,input, V, posInput, surfaceData, builtinData);
				BSDFData bsdfData = ConvertSurfaceDataToBSDFData(input.positionSS.xy, surfaceData);
				LightTransportData lightTransportData = GetLightTransportData(surfaceData, builtinData, bsdfData);

				float4 res = float4( 0.0, 0.0, 0.0, 1.0 );
				UnityMetaInput metaInput;
				metaInput.Albedo = lightTransportData.diffuseColor.rgb;
				metaInput.Emission = lightTransportData.emissiveColor;

			#ifdef EDITOR_VISUALIZATION
				metaInput.VizUV = packedInput.VizUV;
				metaInput.LightCoord = packedInput.LightCoord;
			#endif
				res = UnityMetaFragment(metaInput);

				return res;
			}

			ENDHLSL
		}

		/*ase_pass*/
		Pass
		{
			/*ase_hide_pass*/
			Name "ShadowCaster"
			Tags 
			{ 
				"LightMode" = "ShadowCaster" 
		    }

			Cull [_CullMode]
			ZWrite On
			ZClip [_ZClip]
			ZTest LEqual
			ColorMask 0

			HLSLPROGRAM
			#pragma multi_compile _ DOTS_INSTANCING_ON

            #pragma shader_feature _ _SURFACE_TYPE_TRANSPARENT
            #pragma shader_feature_local _ _TRANSPARENT_WRITES_MOTION_VEC _TRANSPARENT_REFRACTIVE_SORT
            #pragma shader_feature_local_fragment _ _ENABLE_FOG_ON_TRANSPARENT

			#pragma multi_compile_fragment _ SHADOWS_SHADOWMASK

			#pragma vertex Vert
			#pragma fragment Frag

			#define SHADERPASS SHADERPASS_SHADOWS

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
        	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/GeometricTools.hlsl"
        	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Tessellation.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
            #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"

			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"

            #ifdef RAYTRACING_SHADER_GRAPH_DEFAULT
                #define RAYTRACING_SHADER_GRAPH_HIGH
            #endif
        
            #ifdef RAYTRACING_SHADER_GRAPH_RAYTRACED
                #define RAYTRACING_SHADER_GRAPH_LOW
            #endif

            #ifndef SHADER_UNLIT
            #if defined(_DOUBLESIDED_ON) && !defined(VARYINGS_NEED_CULLFACE)
                #define VARYINGS_NEED_CULLFACE
            #endif
            #endif

			#if defined(_DOUBLESIDED_ON) && !defined(ASE_NEED_CULLFACE)
			    #define ASE_NEED_CULLFACE 1
			#endif

            #if _MATERIAL_FEATURE_COLORED_TRANSMISSION
            #undef _MATERIAL_FEATURE_CLEAR_COAT
            #endif

		    #if defined(_MATERIAL_FEATURE_SUBSURFACE_SCATTERING) && !defined(_SURFACE_TYPE_TRANSPARENT)
			#define OUTPUT_SPLIT_LIGHTING
		    #endif

		    #if (SHADERPASS == SHADERPASS_PATH_TRACING) && !defined(_DOUBLESIDED_ON) && (defined(_REFRACTION_PLANE) || defined(_REFRACTION_SPHERE))
			#undef  _REFRACTION_PLANE
			#undef  _REFRACTION_SPHERE
			#define _REFRACTION_THIN
		    #endif

            #if SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_PREPASS
            #if !defined(_DISABLE_SSR_TRANSPARENT) && !defined(SHADER_UNLIT)
                #define WRITE_NORMAL_BUFFER
            #endif
            #endif

            #if SHADERPASS == SHADERPASS_MOTION_VECTORS && defined(WRITE_DECAL_BUFFER_AND_RENDERING_LAYER)
                #define WRITE_DECAL_BUFFER
            #endif

            #ifndef DEBUG_DISPLAY
                #if !defined(_SURFACE_TYPE_TRANSPARENT)
                    #if SHADERPASS == SHADERPASS_FORWARD
                    #define SHADERPASS_FORWARD_BYPASS_ALPHA_TEST
                    #elif SHADERPASS == SHADERPASS_GBUFFER
                    #define SHADERPASS_GBUFFER_BYPASS_ALPHA_TEST
                    #endif
                #endif
            #endif

            #if defined(SHADER_LIT) && !defined(_SURFACE_TYPE_TRANSPARENT)
                #define _DEFERRED_CAPABLE_MATERIAL
            #endif
        
            #if (defined(_TRANSPARENT_WRITES_MOTION_VEC) || defined(_TRANSPARENT_REFRACTIVE_SORT)) && defined(_SURFACE_TYPE_TRANSPARENT)
                #define _WRITE_TRANSPARENT_MOTION_VECTOR
            #endif

			CBUFFER_START( UnityPerMaterial )
			float4 _EmissionColor;
			float _AlphaCutoff;
			float _RenderQueueType;
			#ifdef _ADD_PRECOMPUTED_VELOCITY
			    float _AddPrecomputedVelocity;
			#endif
			float _StencilRef;
			float _StencilWriteMask;
			float _StencilRefDepth;
			float _StencilWriteMaskDepth;
			float _StencilRefMV;
			float _StencilWriteMaskMV;
			float _StencilRefDistortionVec;
			float _StencilWriteMaskDistortionVec;
			float _StencilWriteMaskGBuffer;
			float _StencilRefGBuffer;
			float _ZTestGBuffer;
			float _RequireSplitLighting;
			float _ReceivesSSR;
			float _SurfaceType;
			float _BlendMode;
            #ifdef SUPPORT_BLENDMODE_PRESERVE_SPECULAR_LIGHTING
			    float _EnableBlendModePreserveSpecularLighting;
            #endif
			float _SrcBlend;
			float _DstBlend;
			float _DstBlend2;
			float _AlphaSrcBlend;
			float _AlphaDstBlend;
			float _ZWrite;
			float _TransparentZWrite;
			float _CullMode;
			float _TransparentSortPriority;
			float _EnableFogOnTransparent;
			float _CullModeForward;
			float _TransparentCullMode;
			float _ZTestDepthEqualForOpaque;
			float _ZTestTransparent;
			float _TransparentBackfaceEnable;
			float _AlphaCutoffEnable;
			float _UseShadowThreshold;
			float _DoubleSidedEnable;
			float _DoubleSidedNormalMode;
			float4 _DoubleSidedConstants;
			#ifdef ASE_TESSELLATION
			    float _TessPhongStrength;
			    float _TessValue;
			    float _TessMin;
			    float _TessMax;
			    float _TessEdgeLength;
			    float _TessMaxDisp;
			#endif
			UNITY_TEXTURE_STREAMING_DEBUG_VARS;
			CBUFFER_END

            #ifdef SCENEPICKINGPASS
			float4 _SelectionID;
            #endif

            #ifdef SCENESELECTIONPASS
			int _ObjectId;
			int _PassValue;
            #endif

			/*ase_globals*/

            #ifdef DEBUG_DISPLAY
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Debug/DebugDisplay.hlsl"
            #endif

            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/NormalSurfaceGradient.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/BuiltinUtilities.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialUtilities.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphFunctions.hlsl"

            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/DecalUtilities.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitDecalData.hlsl"

        	#ifdef HAVE_VFX_MODIFICATION
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/VisualEffectVertex.hlsl"
        	#endif

			/*ase_pragma*/

			struct AttributesMesh
			{
				float3 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 tangentOS : TANGENT;
				/*ase_vdata:p=p;n=n;t=t*/
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryingsMeshToPS
			{
				SV_POSITION_QUALIFIERS float4 positionCS : SV_Position;
				float4 clipPosV : TEXCOORD0;
				float3 positionRWS : TEXCOORD1;
				/*ase_interp(2,):sp=sp.xyzw;rwp=tc1*/
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
				#if defined(SHADER_STAGE_FRAGMENT) && defined(ASE_NEED_CULLFACE)
				FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
				#endif
			};

			/*ase_funcs*/

			void BuildSurfaceData(FragInputs fragInputs, inout AlphaSurfaceDescription surfaceDescription, float3 V, PositionInputs posInput, out SurfaceData surfaceData, out float3 bentNormalWS)
			{
				ZERO_INITIALIZE(SurfaceData, surfaceData);

				surfaceData.specularOcclusion = 1.0;
				surfaceData.thickness = 0.0;

				// refraction ShadowCaster
                #if defined(_REFRACTION_PLANE) || defined(_REFRACTION_SPHERE) || defined(_REFRACTION_THIN)
                    if (_EnableSSRefraction)
                    {
        
                        surfaceData.transmittanceMask = (1.0 - surfaceDescription.Alpha);
                        surfaceDescription.Alpha = 1.0;
                    }
                    else
                    {
                        surfaceData.ior = 1.0;
                        surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                        surfaceData.atDistance = 1.0;
                        surfaceData.transmittanceMask = 0.0;
                        surfaceDescription.Alpha = 1.0;
                    }
                    #else
                    surfaceData.ior = 1.0;
                    surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                    surfaceData.atDistance = 1.0;
                    surfaceData.transmittanceMask = 0.0;
                #endif

				surfaceData.materialFeatures = MATERIALFEATUREFLAGS_LIT_STANDARD;

				#ifdef _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SUBSURFACE_SCATTERING;
				#endif

				#ifdef _MATERIAL_FEATURE_TRANSMISSION
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_TRANSMISSION;
				#endif

				#ifdef _MATERIAL_FEATURE_COLORED_TRANSMISSION
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_TRANSMISSION;
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_COLORED_TRANSMISSION;
				#endif

                #ifdef _MATERIAL_FEATURE_ANISOTROPY
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_ANISOTROPY;
                    surfaceData.normalWS = float3(0, 1, 0);
                #endif

				#ifdef _MATERIAL_FEATURE_IRIDESCENCE
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_IRIDESCENCE;
				#endif

				#ifdef _MATERIAL_FEATURE_SPECULAR_COLOR
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SPECULAR_COLOR;
				#endif

				#ifdef _MATERIAL_FEATURE_CLEAR_COAT
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_CLEAR_COAT;
				#endif

				#if defined (_MATERIAL_FEATURE_SPECULAR_COLOR) && defined (_ENERGY_CONSERVING_SPECULAR)
                    surfaceData.baseColor *= ( 1.0 - Max3( surfaceData.specularColor.r, surfaceData.specularColor.g, surfaceData.specularColor.b ) );
				#endif

				#ifdef _DOUBLESIDED_ON
				float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
				#else
				float3 doubleSidedConstants = float3( 1.0, 1.0, 1.0 );
				#endif

				float3 normalTS = float3(0.0f, 0.0f, 1.0f);

                #ifdef DECAL_NORMAL_BLENDING
					normalTS = SurfaceGradientFromTangentSpaceNormalAndFromTBN(normalTS, fragInputs.tangentToWorld[0], fragInputs.tangentToWorld[1]);

                    #if HAVE_DECALS
                    if (_EnableDecals)
                    {
                        float alpha = 1.0;
                        alpha = surfaceDescription.Alpha;

                        DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, fragInputs, alpha);
                        ApplyDecalToSurfaceNormal(decalSurfaceData, fragInputs.tangentToWorld[2], normalTS);
                        ApplyDecalToSurfaceDataNoNormal(decalSurfaceData, surfaceData);
                     }
                    #endif

                    GetNormalWS_SG(fragInputs, normalTS, surfaceData.normalWS, doubleSidedConstants);
                #else
					GetNormalWS(fragInputs, normalTS, surfaceData.normalWS, doubleSidedConstants);

                    #if HAVE_DECALS
                    if (_EnableDecals)
                    {
                        float alpha = 1.0;
                        alpha = surfaceDescription.Alpha;
        
                        DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, fragInputs, alpha);
                        ApplyDecalToSurfaceNormal(decalSurfaceData, surfaceData.normalWS.xyz);
                        ApplyDecalToSurfaceDataNoNormal(decalSurfaceData, surfaceData);
                    }
                    #endif
                #endif

				surfaceData.geomNormalWS = fragInputs.tangentToWorld[2];
                surfaceData.tangentWS = normalize(fragInputs.tangentToWorld[0].xyz );
                surfaceData.tangentWS = Orthonormalize(surfaceData.tangentWS, surfaceData.normalWS);

				bentNormalWS = surfaceData.normalWS;

                #if defined(DEBUG_DISPLAY)
				    #if !defined(SHADER_STAGE_RAY_TRACING)
				    if (_DebugMipMapMode != DEBUGMIPMAPMODE_NONE)
				    {
					   #ifdef FRAG_INPUTS_USE_TEXCOORD0
                           surfaceData.baseColor = GET_TEXTURE_STREAMING_DEBUG(posInput.positionSS, fragInputs.texCoord0);
                       #else
                           surfaceData.baseColor = GET_TEXTURE_STREAMING_DEBUG_NO_UV(posInput.positionSS);
                       #endif
					   surfaceData.metallic = 0;
				    }
					#endif
                    ApplyDebugToSurfaceData(fragInputs.tangentToWorld, surfaceData);
                #endif

                #if defined(_SPECULAR_OCCLUSION_CUSTOM)
                #elif defined(_SPECULAR_OCCLUSION_FROM_AO_BENT_NORMAL)
                    surfaceData.specularOcclusion = GetSpecularOcclusionFromBentAO(V, bentNormalWS, surfaceData.normalWS, surfaceData.ambientOcclusion, PerceptualSmoothnessToPerceptualRoughness(surfaceData.perceptualSmoothness));
                #elif defined(_AMBIENT_OCCLUSION) && defined(_SPECULAR_OCCLUSION_FROM_AO)
                    surfaceData.specularOcclusion = GetSpecularOcclusionFromAmbientOcclusion(ClampNdotV(dot(surfaceData.normalWS, V)), surfaceData.ambientOcclusion, PerceptualSmoothnessToRoughness(surfaceData.perceptualSmoothness));
                #endif

                #ifdef _ENABLE_GEOMETRIC_SPECULAR_AA
                    surfaceData.perceptualSmoothness = GeometricNormalFiltering(surfaceData.perceptualSmoothness, fragInputs.tangentToWorld[2], surfaceDescription.SpecularAAScreenSpaceVariance, surfaceDescription.SpecularAAThreshold);
                #endif
			}

			// Get Surface And BuiltinData
			void GetSurfaceAndBuiltinData(AlphaSurfaceDescription surfaceDescription, FragInputs fragInputs, float3 V, inout PositionInputs posInput, out SurfaceData surfaceData, out BuiltinData builtinData)
			{
				#ifdef LOD_FADE_CROSSFADE
                    LODDitheringTransition(ComputeFadeMaskSeed(V, posInput.positionSS), unity_LODFade.x);
				#endif

                #ifdef _DOUBLESIDED_ON
                    float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
                #else
                    float3 doubleSidedConstants = float3(1.0, 1.0, 1.0);
                #endif
                ApplyDoubleSidedFlipOrMirror(fragInputs, doubleSidedConstants);

				#ifdef _ALPHATEST_ON
                    DoAlphaTest( surfaceDescription.Alpha, surfaceDescription.AlphaClipThreshold );
				#endif

				#ifdef _ALPHATEST_SHADOW_ON
                    DoAlphaTest( surfaceDescription.Alpha, surfaceDescription.AlphaClipThresholdShadow);
				#endif

				#ifdef _DEPTHOFFSET_ON
                    ApplyDepthOffsetPositionInput(V, surfaceDescription.DepthOffset, GetViewForwardDir(), GetWorldToHClipMatrix(), posInput);
				#endif

                float3 bentNormalWS;
                BuildSurfaceData(fragInputs, surfaceDescription, V, posInput, surfaceData, bentNormalWS);
                InitBuiltinData(posInput, surfaceDescription.Alpha, bentNormalWS, -fragInputs.tangentToWorld[2], fragInputs.texCoord1, fragInputs.texCoord2, builtinData);

				#ifdef _DEPTHOFFSET_ON
                    builtinData.depthOffset = surfaceDescription.DepthOffset;
				#endif

                #ifdef _ALPHATEST_ON
                    builtinData.alphaClipTreshold = surfaceDescription.AlphaClipThreshold;
                #endif

                #ifdef UNITY_VIRTUAL_TEXTURING
                    builtinData.vtPackedFeedback = surfaceDescription.VTPackedFeedback;
                #endif

				#ifdef ASE_BAKEDGI
                    builtinData.bakeDiffuseLighting = surfaceDescription.BakedGI;
				#endif

				#ifdef ASE_BAKEDBACKGI
                    builtinData.backBakeDiffuseLighting = surfaceDescription.BakedBackGI;
				#endif

                builtinData.emissiveColor = surfaceDescription.Emission;

                PostInitBuiltinData(V, posInput, surfaceData, builtinData);
			}

			PackedVaryingsMeshToPS VertexFunction(AttributesMesh inputMesh /*ase_vert_input*/)
			{
				PackedVaryingsMeshToPS output;
				UNITY_SETUP_INSTANCE_ID(inputMesh);
				UNITY_TRANSFER_INSTANCE_ID(inputMesh, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( output );

				/*ase_vert_code:inputMesh=AttributesMesh;output=PackedVaryingsMeshToPS*/

				#ifdef ASE_ABSOLUTE_VERTEX_POS
				float3 defaultVertexValue = inputMesh.positionOS.xyz;
				#else
				float3 defaultVertexValue = float3( 0, 0, 0 );
				#endif
				float3 vertexValue = /*ase_vert_out:Vertex Offset;Float3;3;-1;_VertexOffset*/ defaultVertexValue /*end*/;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
				inputMesh.positionOS.xyz = vertexValue;
				#else
				inputMesh.positionOS.xyz += vertexValue;
				#endif

				inputMesh.normalOS = /*ase_vert_out:Vertex Normal;Float3;4;-1;_VertexNormal*/ inputMesh.normalOS /*end*/;
				inputMesh.tangentOS = /*ase_vert_out:Vertex Tangent;Float4;27;-1;_VertexTangent*/inputMesh.tangentOS/*end*/;

				float3 positionRWS = TransformObjectToWorld(inputMesh.positionOS);
				output.positionCS = TransformWorldToHClip(positionRWS);
				output.clipPosV = output.positionCS;
				output.positionRWS = positionRWS;
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float3 positionOS : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				float4 tangentOS : TANGENT;
				/*ase_vcontrol*/
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl Vert ( AttributesMesh v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.positionOS = v.positionOS;
				o.normalOS = v.normalOS;
				o.tangentOS = v.tangentOS;
				/*ase_control_code:v=AttributesMesh;o=VertexControl*/
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = /*ase_inline_begin*/_TessValue/*ase_inline_end*/; float tessMin = /*ase_inline_begin*/_TessMin/*ase_inline_end*/; float tessMax = /*ase_inline_begin*/_TessMax/*ase_inline_end*/;
				float edgeLength = /*ase_inline_begin*/_TessEdgeLength/*ase_inline_end*/; float tessMaxDisp = /*ase_inline_begin*/_TessMaxDisp/*ase_inline_end*/;
				#if (SHADEROPTIONS_CAMERA_RELATIVE_RENDERING != 0)
				float3 cameraPos = 0;
				#else
				float3 cameraPos = _WorldSpaceCameraPos;
				#endif
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(float4(v[0].positionOS,1), float4(v[1].positionOS,1), float4(v[2].positionOS,1), tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), cameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(float4(v[0].positionOS,1), float4(v[1].positionOS,1), float4(v[2].positionOS,1), edgeLength, GetObjectToWorldMatrix(), cameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(float4(v[0].positionOS,1), float4(v[1].positionOS,1), float4(v[2].positionOS,1), edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), cameraPos, _ScreenParams, _FrustumPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			PackedVaryingsMeshToPS DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				AttributesMesh o = (AttributesMesh) 0;
				o.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				o.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				o.tangentOS = patch[0].tangentOS * bary.x + patch[1].tangentOS * bary.y + patch[2].tangentOS * bary.z;
				/*ase_domain_code:patch=VertexControl;o=AttributesMesh;bary=SV_DomainLocation*/
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.positionOS.xyz - patch[i].normalOS * (dot(o.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = /*ase_inline_begin*/_TessPhongStrength/*ase_inline_end*/;
				o.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			PackedVaryingsMeshToPS Vert ( AttributesMesh v )
			{
				return VertexFunction( v );
			}
			#endif

			#if defined(WRITE_NORMAL_BUFFER) && defined(WRITE_MSAA_DEPTH)
			#define SV_TARGET_DECAL SV_Target2
			#elif defined(WRITE_NORMAL_BUFFER) || defined(WRITE_MSAA_DEPTH)
			#define SV_TARGET_DECAL SV_Target1
			#else
			#define SV_TARGET_DECAL SV_Target0
			#endif

			void Frag( PackedVaryingsMeshToPS packedInput
						#if defined(SCENESELECTIONPASS) || defined(SCENEPICKINGPASS)
						, out float4 outColor : SV_Target0
						#else
							#ifdef WRITE_MSAA_DEPTH
							, out float4 depthColor : SV_Target0
								#ifdef WRITE_NORMAL_BUFFER
								, out float4 outNormalBuffer : SV_Target1
								#endif
							#else
								#ifdef WRITE_NORMAL_BUFFER
								, out float4 outNormalBuffer : SV_Target0
								#endif
							#endif

							#if (defined(WRITE_DECAL_BUFFER) && !defined(_DISABLE_DECALS)) || defined(WRITE_RENDERING_LAYER)
							, out float4 outDecalBuffer : SV_TARGET_DECAL
							#endif
						#endif

						#if defined(_DEPTHOFFSET_ON) && !defined(SCENEPICKINGPASS)
						, out float outputDepth : DEPTH_OFFSET_SEMANTIC
						#endif
						/*ase_frag_input*/
					)
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(packedInput);
				UNITY_SETUP_INSTANCE_ID(packedInput);
		
				/*ase_local_var:rwp*/float3 PositionRWS = packedInput.positionRWS;
				/*ase_local_var:wvd*/float3 V = GetWorldSpaceNormalizeViewDir( packedInput.positionRWS );
				/*ase_local_var:sp*/float4 ClipPos = packedInput.clipPosV;
				/*ase_local_var:spu*/float4 ScreenPos = ComputeScreenPos( packedInput.clipPosV, _ProjectionParams.x );

				FragInputs input;
				ZERO_INITIALIZE(FragInputs, input);
				input.tangentToWorld = k_identity3x3;
				input.positionSS = packedInput.positionCS;
				input.positionRWS = PositionRWS;

				#if _DOUBLESIDED_ON && SHADER_STAGE_FRAGMENT
				input.isFrontFace = IS_FRONT_VFACE( packedInput.cullFace, true, false);
				#elif SHADER_STAGE_FRAGMENT
				#if defined(ASE_NEED_CULLFACE)
				input.isFrontFace = IS_FRONT_VFACE( packedInput.cullFace, true, false );
				#endif
				#endif
				/*ase_local_var:vf*/half IsFrontFace = input.isFrontFace;

				PositionInputs posInput = GetPositionInput(input.positionSS.xy, _ScreenSize.zw, input.positionSS.z, input.positionSS.w, input.positionRWS);

				AlphaSurfaceDescription surfaceDescription = (AlphaSurfaceDescription)0;
				/*ase_frag_code:packedInput=PackedVaryingsMeshToPS*/
				surfaceDescription.Alpha = /*ase_frag_out:Alpha;Float;0;-1;_Alpha*/1/*end*/;

				#ifdef _ALPHATEST_ON
				surfaceDescription.AlphaClipThreshold = /*ase_frag_out:Alpha Clip Threshold;Float;1;-1;_AlphaClip*/_AlphaCutoff/*end*/;
				#endif

				#ifdef _ALPHATEST_SHADOW_ON
				surfaceDescription.AlphaClipThresholdShadow = /*ase_frag_out:Alpha Clip Threshold Shadow;Float;2;-1;_AlphaClipShadow*/0.5/*end*/;
				#endif

				#ifdef _DEPTHOFFSET_ON
				surfaceDescription.DepthOffset = /*ase_frag_out:Depth Offset;Float;5;-1;_DepthOffset*/0/*end*/;
				#endif

				SurfaceData surfaceData;
				BuiltinData builtinData;
				GetSurfaceAndBuiltinData(surfaceDescription, input, V, posInput, surfaceData, builtinData);

				#ifdef _DEPTHOFFSET_ON
				outputDepth = posInput.deviceDepth;
				float bias = max(abs(ddx(posInput.deviceDepth)), abs(ddy(posInput.deviceDepth))) * _SlopeScaleDepthBias;
				outputDepth += bias;
				#endif

				#ifdef WRITE_MSAA_DEPTH
					depthColor = packedInput.vmesh.positionCS.z;
					depthColor.a = SharpenAlpha(builtinData.opacity, builtinData.alphaClipTreshold);
				#endif

				#if defined(WRITE_NORMAL_BUFFER)
				EncodeIntoNormalBuffer(ConvertSurfaceDataToNormalData(surfaceData), outNormalBuffer);
				#endif

                #if (defined(WRITE_DECAL_BUFFER) && !defined(_DISABLE_DECALS)) || defined(WRITE_RENDERING_LAYER)
				    DecalPrepassData decalPrepassData;
                    #ifdef _DISABLE_DECALS
				    ZERO_INITIALIZE(DecalPrepassData, decalPrepassData);
                    #else
				    decalPrepassData.geomNormalWS = surfaceData.geomNormalWS;
                    #endif
				    decalPrepassData.renderingLayerMask = GetMeshRenderingLayerMask();
				    EncodeIntoDecalPrepassBuffer(decalPrepassData, outDecalBuffer);
				#endif
			}
			ENDHLSL
		}

		/*ase_pass*/
		Pass
		{
			/*ase_hide_pass*/
			Name "SceneSelectionPass"
			Tags 
			{ 
				"LightMode" = "SceneSelectionPass" 
		    }

			Cull Off

			HLSLPROGRAM
			#pragma editor_sync_compilation
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #pragma shader_feature _ _SURFACE_TYPE_TRANSPARENT
            #pragma shader_feature_local _ _TRANSPARENT_WRITES_MOTION_VEC _TRANSPARENT_REFRACTIVE_SORT
            #pragma shader_feature_local_fragment _ _ENABLE_FOG_ON_TRANSPARENT

			#pragma vertex Vert
			#pragma fragment Frag

			#define SHADERPASS SHADERPASS_DEPTH_ONLY
		    #define SCENESELECTIONPASS 1

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/GeometricTools.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Tessellation.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
            #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"

			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"

            #ifdef RAYTRACING_SHADER_GRAPH_DEFAULT
                #define RAYTRACING_SHADER_GRAPH_HIGH
            #endif

            #ifdef RAYTRACING_SHADER_GRAPH_RAYTRACED
                #define RAYTRACING_SHADER_GRAPH_LOW
            #endif

            #ifndef SHADER_UNLIT
            #if defined(_DOUBLESIDED_ON) && !defined(VARYINGS_NEED_CULLFACE)
                #define VARYINGS_NEED_CULLFACE
            #endif
            #endif

			#if defined(_DOUBLESIDED_ON) && !defined(ASE_NEED_CULLFACE)
			    #define ASE_NEED_CULLFACE 1
			#endif

            #if _MATERIAL_FEATURE_COLORED_TRANSMISSION
            #undef _MATERIAL_FEATURE_CLEAR_COAT
            #endif

		    #if defined(_MATERIAL_FEATURE_SUBSURFACE_SCATTERING) && !defined(_SURFACE_TYPE_TRANSPARENT)
			#define OUTPUT_SPLIT_LIGHTING
		    #endif

		    #if (SHADERPASS == SHADERPASS_PATH_TRACING) && !defined(_DOUBLESIDED_ON) && (defined(_REFRACTION_PLANE) || defined(_REFRACTION_SPHERE))
			#undef  _REFRACTION_PLANE
			#undef  _REFRACTION_SPHERE
			#define _REFRACTION_THIN
		    #endif

            #if SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_PREPASS
            #if !defined(_DISABLE_SSR_TRANSPARENT) && !defined(SHADER_UNLIT)
                #define WRITE_NORMAL_BUFFER
            #endif
            #endif

            #if SHADERPASS == SHADERPASS_MOTION_VECTORS && defined(WRITE_DECAL_BUFFER_AND_RENDERING_LAYER)
                #define WRITE_DECAL_BUFFER
            #endif

            #ifndef DEBUG_DISPLAY
                #if !defined(_SURFACE_TYPE_TRANSPARENT)
                    #if SHADERPASS == SHADERPASS_FORWARD
                    #define SHADERPASS_FORWARD_BYPASS_ALPHA_TEST
                    #elif SHADERPASS == SHADERPASS_GBUFFER
                    #define SHADERPASS_GBUFFER_BYPASS_ALPHA_TEST
                    #endif
                #endif
            #endif

            #if defined(SHADER_LIT) && !defined(_SURFACE_TYPE_TRANSPARENT)
                #define _DEFERRED_CAPABLE_MATERIAL
            #endif

            #if (defined(_TRANSPARENT_WRITES_MOTION_VEC) || defined(_TRANSPARENT_REFRACTIVE_SORT)) && defined(_SURFACE_TYPE_TRANSPARENT)
                #define _WRITE_TRANSPARENT_MOTION_VECTOR
            #endif

			CBUFFER_START( UnityPerMaterial )
			float4 _EmissionColor;
			float _AlphaCutoff;
			float _RenderQueueType;
			#ifdef _ADD_PRECOMPUTED_VELOCITY
			    float _AddPrecomputedVelocity;
			#endif
			float _StencilRef;
			float _StencilWriteMask;
			float _StencilRefDepth;
			float _StencilWriteMaskDepth;
			float _StencilRefMV;
			float _StencilWriteMaskMV;
			float _StencilRefDistortionVec;
			float _StencilWriteMaskDistortionVec;
			float _StencilWriteMaskGBuffer;
			float _StencilRefGBuffer;
			float _ZTestGBuffer;
			float _RequireSplitLighting;
			float _ReceivesSSR;
			float _SurfaceType;
			float _BlendMode;
            #ifdef SUPPORT_BLENDMODE_PRESERVE_SPECULAR_LIGHTING
			    float _EnableBlendModePreserveSpecularLighting;
            #endif
			float _SrcBlend;
			float _DstBlend;
			float _DstBlend2;
			float _AlphaSrcBlend;
			float _AlphaDstBlend;
			float _ZWrite;
			float _TransparentZWrite;
			float _CullMode;
			float _TransparentSortPriority;
			float _EnableFogOnTransparent;
			float _CullModeForward;
			float _TransparentCullMode;
			float _ZTestDepthEqualForOpaque;
			float _ZTestTransparent;
			float _TransparentBackfaceEnable;
			float _AlphaCutoffEnable;
			float _UseShadowThreshold;
			float _DoubleSidedEnable;
			float _DoubleSidedNormalMode;
			float4 _DoubleSidedConstants;
			#ifdef ASE_TESSELLATION
			    float _TessPhongStrength;
			    float _TessValue;
			    float _TessMin;
			    float _TessMax;
			    float _TessEdgeLength;
			    float _TessMaxDisp;
			#endif
			UNITY_TEXTURE_STREAMING_DEBUG_VARS;
			CBUFFER_END

            #ifdef SCENEPICKINGPASS
			float4 _SelectionID;
            #endif

            #ifdef SCENESELECTIONPASS
			int _ObjectId;
			int _PassValue;
            #endif

			/*ase_globals*/

            #ifdef DEBUG_DISPLAY
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Debug/DebugDisplay.hlsl"
            #endif

			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/PickingSpaceTransforms.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/NormalSurfaceGradient.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/BuiltinUtilities.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialUtilities.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphFunctions.hlsl"

            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/DecalUtilities.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitDecalData.hlsl"

        	#ifdef HAVE_VFX_MODIFICATION
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/VisualEffectVertex.hlsl"
        	#endif

			/*ase_pragma*/

			struct AttributesMesh
			{
				float3 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 tangentOS : TANGENT;
				/*ase_vdata:p=p;n=n;t=t*/
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryingsMeshToPS
			{
				SV_POSITION_QUALIFIERS float4 positionCS : SV_Position;
				float3 positionRWS : TEXCOORD0;
				/*ase_interp(1,):sp=sp.xyzw;rwp=tc0*/
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
				#if defined(SHADER_STAGE_FRAGMENT) && defined(ASE_NEED_CULLFACE)
				FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
				#endif
			};

			/*ase_funcs*/

			void BuildSurfaceData(FragInputs fragInputs, inout SceneSurfaceDescription surfaceDescription, float3 V, PositionInputs posInput, out SurfaceData surfaceData, out float3 bentNormalWS)
			{
				ZERO_INITIALIZE(SurfaceData, surfaceData);

				surfaceData.specularOcclusion = 1.0;
				surfaceData.thickness = 0.0;

				//refraction SceneSelectionPass
                #if defined(_REFRACTION_PLANE) || defined(_REFRACTION_SPHERE) || defined(_REFRACTION_THIN)
                    if (_EnableSSRefraction)
                    {
                        surfaceData.ior =                       surfaceDescription.RefractionIndex;
                        surfaceData.transmittanceColor =        surfaceDescription.RefractionColor;
                        surfaceData.atDistance =                surfaceDescription.RefractionDistance;
        
                        surfaceData.transmittanceMask = (1.0 - surfaceDescription.Alpha);
                        surfaceDescription.Alpha = 1.0;
                    }
                    else
                    {
                        surfaceData.ior = 1.0;
                        surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                        surfaceData.atDistance = 1.0;
                        surfaceData.transmittanceMask = 0.0;
                        surfaceDescription.Alpha = 1.0;
                    }
                #else
                    surfaceData.ior = 1.0;
                    surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                    surfaceData.atDistance = 1.0;
                    surfaceData.transmittanceMask = 0.0;
                #endif

				surfaceData.materialFeatures = MATERIALFEATUREFLAGS_LIT_STANDARD;

				#ifdef _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SUBSURFACE_SCATTERING;
				#endif

				#ifdef _MATERIAL_FEATURE_TRANSMISSION
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_TRANSMISSION;
				#endif

				#ifdef _MATERIAL_FEATURE_COLORED_TRANSMISSION
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_TRANSMISSION;
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_COLORED_TRANSMISSION;
				#endif

                #ifdef _MATERIAL_FEATURE_ANISOTROPY
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_ANISOTROPY;
                    surfaceData.normalWS = float3(0, 1, 0);
                #endif

				#ifdef _MATERIAL_FEATURE_CLEAR_COAT
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_CLEAR_COAT;
				#endif

				#ifdef _MATERIAL_FEATURE_IRIDESCENCE
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_IRIDESCENCE;
				#endif

				#ifdef _MATERIAL_FEATURE_SPECULAR_COLOR
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SPECULAR_COLOR;
				#endif

				#if defined (_MATERIAL_FEATURE_SPECULAR_COLOR) && defined (_ENERGY_CONSERVING_SPECULAR)
                    surfaceData.baseColor *= ( 1.0 - Max3( surfaceData.specularColor.r, surfaceData.specularColor.g, surfaceData.specularColor.b ) );
				#endif

				#ifdef _DOUBLESIDED_ON
                    float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
				#else
                    float3 doubleSidedConstants = float3( 1.0, 1.0, 1.0 );
				#endif

				float3 normalTS = float3(0.0f, 0.0f, 1.0f);

                #ifdef DECAL_NORMAL_BLENDING
					normalTS = SurfaceGradientFromTangentSpaceNormalAndFromTBN(normalTS, fragInputs.tangentToWorld[0], fragInputs.tangentToWorld[1]);

                    #if HAVE_DECALS
                    if (_EnableDecals)
                    {
                        float alpha = 1.0;
                        alpha = surfaceDescription.Alpha;

                        DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, fragInputs, alpha);
                        ApplyDecalToSurfaceNormal(decalSurfaceData, fragInputs.tangentToWorld[2], normalTS);
                        ApplyDecalToSurfaceDataNoNormal(decalSurfaceData, surfaceData);
                     }
                    #endif

                    GetNormalWS_SG(fragInputs, normalTS, surfaceData.normalWS, doubleSidedConstants);
                #else
					GetNormalWS(fragInputs, normalTS, surfaceData.normalWS, doubleSidedConstants);

                    #if HAVE_DECALS
                    if (_EnableDecals)
                    {
                        float alpha = 1.0;
                        alpha = surfaceDescription.Alpha;
        
                        DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, fragInputs, alpha);
                        ApplyDecalToSurfaceNormal(decalSurfaceData, surfaceData.normalWS.xyz);
                        ApplyDecalToSurfaceDataNoNormal(decalSurfaceData, surfaceData);
                    }
                    #endif
                #endif

				surfaceData.geomNormalWS = fragInputs.tangentToWorld[2];
                surfaceData.tangentWS = normalize(fragInputs.tangentToWorld[0].xyz );
                surfaceData.tangentWS = Orthonormalize(surfaceData.tangentWS, surfaceData.normalWS);

				bentNormalWS = surfaceData.normalWS;

                #if defined(DEBUG_DISPLAY)
				    #if !defined(SHADER_STAGE_RAY_TRACING)
				    if (_DebugMipMapMode != DEBUGMIPMAPMODE_NONE)
				    {
					   #ifdef FRAG_INPUTS_USE_TEXCOORD0
                           surfaceData.baseColor = GET_TEXTURE_STREAMING_DEBUG(posInput.positionSS, fragInputs.texCoord0);
                       #else
                           surfaceData.baseColor = GET_TEXTURE_STREAMING_DEBUG_NO_UV(posInput.positionSS);
                       #endif
					   surfaceData.metallic = 0;
				    }
					#endif
                     ApplyDebugToSurfaceData(fragInputs.tangentToWorld, surfaceData);
                #endif

                #if defined(_SPECULAR_OCCLUSION_CUSTOM)
                #elif defined(_SPECULAR_OCCLUSION_FROM_AO_BENT_NORMAL)
                    surfaceData.specularOcclusion = GetSpecularOcclusionFromBentAO(V, bentNormalWS, surfaceData.normalWS, surfaceData.ambientOcclusion, PerceptualSmoothnessToPerceptualRoughness(surfaceData.perceptualSmoothness));
                #elif defined(_AMBIENT_OCCLUSION) && defined(_SPECULAR_OCCLUSION_FROM_AO)
                    surfaceData.specularOcclusion = GetSpecularOcclusionFromAmbientOcclusion(ClampNdotV(dot(surfaceData.normalWS, V)), surfaceData.ambientOcclusion, PerceptualSmoothnessToRoughness(surfaceData.perceptualSmoothness));
                #endif

                #ifdef _ENABLE_GEOMETRIC_SPECULAR_AA
                    surfaceData.perceptualSmoothness = GeometricNormalFiltering(surfaceData.perceptualSmoothness, fragInputs.tangentToWorld[2], surfaceDescription.SpecularAAScreenSpaceVariance, surfaceDescription.SpecularAAThreshold);
                #endif
			}

			// Get Surface And BuiltinData
			void GetSurfaceAndBuiltinData(SceneSurfaceDescription surfaceDescription, FragInputs fragInputs, float3 V, inout PositionInputs posInput, out SurfaceData surfaceData, out BuiltinData builtinData)
			{
				#ifdef LOD_FADE_CROSSFADE
                    LODDitheringTransition(ComputeFadeMaskSeed(V, posInput.positionSS), unity_LODFade.x);
				#endif

                #ifdef _DOUBLESIDED_ON
                    float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
                #else
                    float3 doubleSidedConstants = float3(1.0, 1.0, 1.0);
                #endif
                ApplyDoubleSidedFlipOrMirror(fragInputs, doubleSidedConstants);

				#ifdef _ALPHATEST_ON
                    DoAlphaTest( surfaceDescription.Alpha, surfaceDescription.AlphaClipThreshold );
				#endif

				#ifdef _ALPHATEST_SHADOW_ON
                    DoAlphaTest( surfaceDescription.Alpha, surfaceDescription.AlphaClipThresholdShadow);
				#endif

				#ifdef _DEPTHOFFSET_ON
                    ApplyDepthOffsetPositionInput(V, surfaceDescription.DepthOffset, GetViewForwardDir(), GetWorldToHClipMatrix(), posInput);
				#endif

                float3 bentNormalWS;
                BuildSurfaceData(fragInputs, surfaceDescription, V, posInput, surfaceData, bentNormalWS);
                InitBuiltinData(posInput, surfaceDescription.Alpha, bentNormalWS, -fragInputs.tangentToWorld[2], fragInputs.texCoord1, fragInputs.texCoord2, builtinData);

				#ifdef _DEPTHOFFSET_ON
                    builtinData.depthOffset = surfaceDescription.DepthOffset;
				#endif

                #ifdef _ALPHATEST_ON
                    builtinData.alphaClipTreshold = surfaceDescription.AlphaClipThreshold;
                #endif

                #ifdef UNITY_VIRTUAL_TEXTURING
                    builtinData.vtPackedFeedback = surfaceDescription.VTPackedFeedback;
                #endif

				#ifdef ASE_BAKEDGI
                    builtinData.bakeDiffuseLighting = surfaceDescription.BakedGI;
				#endif

				#ifdef ASE_BAKEDBACKGI
                    builtinData.backBakeDiffuseLighting = surfaceDescription.BakedBackGI;
				#endif

                builtinData.emissiveColor = surfaceDescription.Emission;

                PostInitBuiltinData(V, posInput, surfaceData, builtinData);
			}

			PackedVaryingsMeshToPS VertexFunction(AttributesMesh inputMesh /*ase_vert_input*/)
			{
				PackedVaryingsMeshToPS output;
				UNITY_SETUP_INSTANCE_ID(inputMesh);
				UNITY_TRANSFER_INSTANCE_ID(inputMesh, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( output );

				/*ase_vert_code:inputMesh=AttributesMesh;output=PackedVaryingsMeshToPS*/

				#ifdef ASE_ABSOLUTE_VERTEX_POS
				float3 defaultVertexValue = inputMesh.positionOS.xyz;
				#else
				float3 defaultVertexValue = float3( 0, 0, 0 );
				#endif
				float3 vertexValue = /*ase_vert_out:Vertex Offset;Float3;2;-1;_VertexOffset*/ defaultVertexValue /*end*/;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
				inputMesh.positionOS.xyz = vertexValue;
				#else
				inputMesh.positionOS.xyz += vertexValue;
				#endif

				inputMesh.normalOS = /*ase_vert_out:Vertex Normal;Float3;3;-1;_VertexNormal*/ inputMesh.normalOS /*end*/;
				inputMesh.tangentOS = /*ase_vert_out:Vertex Tangent;Float4;26;-1;_VertexTangent*/inputMesh.tangentOS/*end*/;

				float3 positionRWS = TransformObjectToWorld(inputMesh.positionOS);
				output.positionCS = TransformWorldToHClip(positionRWS);
				output.positionRWS = positionRWS;
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float3 positionOS : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				float4 tangentOS : TANGENT;
				/*ase_vcontrol*/
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl Vert ( AttributesMesh v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.positionOS = v.positionOS;
				o.normalOS = v.normalOS;
				o.tangentOS = v.tangentOS;
				/*ase_control_code:v=AttributesMesh;o=VertexControl*/
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = /*ase_inline_begin*/_TessValue/*ase_inline_end*/; float tessMin = /*ase_inline_begin*/_TessMin/*ase_inline_end*/; float tessMax = /*ase_inline_begin*/_TessMax/*ase_inline_end*/;
				float edgeLength = /*ase_inline_begin*/_TessEdgeLength/*ase_inline_end*/; float tessMaxDisp = /*ase_inline_begin*/_TessMaxDisp/*ase_inline_end*/;
				#if (SHADEROPTIONS_CAMERA_RELATIVE_RENDERING != 0)
				float3 cameraPos = 0;
				#else
				float3 cameraPos = _WorldSpaceCameraPos;
				#endif
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(float4(v[0].positionOS,1), float4(v[1].positionOS,1), float4(v[2].positionOS,1), tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), cameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(float4(v[0].positionOS,1), float4(v[1].positionOS,1), float4(v[2].positionOS,1), edgeLength, GetObjectToWorldMatrix(), cameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(float4(v[0].positionOS,1), float4(v[1].positionOS,1), float4(v[2].positionOS,1), edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), cameraPos, _ScreenParams, _FrustumPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			PackedVaryingsMeshToPS DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				AttributesMesh o = (AttributesMesh) 0;
				o.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				o.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				o.tangentOS = patch[0].tangentOS * bary.x + patch[1].tangentOS * bary.y + patch[2].tangentOS * bary.z;
				/*ase_domain_code:patch=VertexControl;o=AttributesMesh;bary=SV_DomainLocation*/
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.positionOS.xyz - patch[i].normalOS * (dot(o.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = /*ase_inline_begin*/_TessPhongStrength/*ase_inline_end*/;
				o.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			PackedVaryingsMeshToPS Vert ( AttributesMesh v )
			{
				return VertexFunction( v );
			}
			#endif

			#if defined(WRITE_NORMAL_BUFFER) && defined(WRITE_MSAA_DEPTH)
			#define SV_TARGET_DECAL SV_Target2
			#elif defined(WRITE_NORMAL_BUFFER) || defined(WRITE_MSAA_DEPTH)
			#define SV_TARGET_DECAL SV_Target1
			#else
			#define SV_TARGET_DECAL SV_Target0
			#endif

			void Frag( PackedVaryingsMeshToPS packedInput
						, out float4 outColor : SV_Target0
						#if defined(_DEPTHOFFSET_ON) && !defined(SCENEPICKINGPASS)
						, out float outputDepth : DEPTH_OFFSET_SEMANTIC
						#endif
						/*ase_frag_input*/
					)
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( packedInput );
				UNITY_SETUP_INSTANCE_ID( packedInput );

				/*ase_local_var:rwp*/float3 PositionRWS = packedInput.positionRWS;
				/*ase_local_var:wvd*/float3 V = GetWorldSpaceNormalizeViewDir( packedInput.positionRWS );

				FragInputs input;
				ZERO_INITIALIZE(FragInputs, input);
				input.tangentToWorld = k_identity3x3;
				input.positionSS = packedInput.positionCS;
				input.positionRWS = PositionRWS;

				#if _DOUBLESIDED_ON && SHADER_STAGE_FRAGMENT
				input.isFrontFace = IS_FRONT_VFACE( packedInput.cullFace, true, false);
				#elif SHADER_STAGE_FRAGMENT
				#if defined(ASE_NEED_CULLFACE)
				input.isFrontFace = IS_FRONT_VFACE( packedInput.cullFace, true, false );
				#endif
				#endif
				/*ase_local_var:vf*/half IsFrontFace = input.isFrontFace;

				PositionInputs posInput = GetPositionInput(input.positionSS.xy, _ScreenSize.zw, input.positionSS.z, input.positionSS.w, input.positionRWS);

				SceneSurfaceDescription surfaceDescription = (SceneSurfaceDescription)0;
				/*ase_frag_code:packedInput=PackedVaryingsMeshToPS*/
				surfaceDescription.Alpha = /*ase_frag_out:Alpha;Float;0;-1;_Alpha*/1/*end*/;

				#ifdef _ALPHATEST_ON
				surfaceDescription.AlphaClipThreshold = /*ase_frag_out:Alpha Clip Threshold;Float;1;-1;_AlphaClip*/_AlphaCutoff/*end*/;
				#endif

				#ifdef _DEPTHOFFSET_ON
				surfaceDescription.DepthOffset = /*ase_frag_out:Depth Offset;Float;4;-1;_DepthOffset*/0/*end*/;
				#endif

				SurfaceData surfaceData;
				BuiltinData builtinData;
				GetSurfaceAndBuiltinData(surfaceDescription, input, V, posInput, surfaceData, builtinData);

				#ifdef _DEPTHOFFSET_ON
				outputDepth = posInput.deviceDepth;
				#endif

				outColor = float4( _ObjectId, _PassValue, 1.0, 1.0 );
			}
			ENDHLSL
		}

		/*ase_pass*/
		Pass
		{
			/*ase_hide_pass*/
			Name "DepthOnly"
			Tags 
			{ 
				"LightMode" = "DepthOnly" 
		    }

			Cull [_CullMode]
			ZWrite On

			Stencil
			{
				WriteMask [_StencilWriteMaskDepth]
				Ref [_StencilRefDepth]
		        CompFront Always
		        PassFront Replace
		        CompBack Always
		        PassBack Replace
			}

			HLSLPROGRAM
			#pragma multi_compile _ DOTS_INSTANCING_ON

            #pragma shader_feature _ _SURFACE_TYPE_TRANSPARENT
            #pragma shader_feature_local _ _TRANSPARENT_WRITES_MOTION_VEC _TRANSPARENT_REFRACTIVE_SORT
            #pragma shader_feature_local_fragment _ _ENABLE_FOG_ON_TRANSPARENT

            #pragma multi_compile _ WRITE_NORMAL_BUFFER
            #pragma multi_compile_fragment _ WRITE_MSAA_DEPTH
            #pragma multi_compile_fragment _ WRITE_DECAL_BUFFER WRITE_RENDERING_LAYER

			#pragma vertex Vert
			#pragma fragment Frag

            #define SHADERPASS SHADERPASS_DEPTH_ONLY

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/GeometricTools.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Tessellation.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
            #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"

			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"

            #ifdef RAYTRACING_SHADER_GRAPH_DEFAULT
                #define RAYTRACING_SHADER_GRAPH_HIGH
            #endif
        
            #ifdef RAYTRACING_SHADER_GRAPH_RAYTRACED
                #define RAYTRACING_SHADER_GRAPH_LOW
            #endif

            #ifndef SHADER_UNLIT
            #if defined(_DOUBLESIDED_ON) && !defined(VARYINGS_NEED_CULLFACE)
                #define VARYINGS_NEED_CULLFACE
            #endif
            #endif

			#if defined(_DOUBLESIDED_ON) && !defined(ASE_NEED_CULLFACE)
			    #define ASE_NEED_CULLFACE 1
			#endif

            #if _MATERIAL_FEATURE_COLORED_TRANSMISSION
            #undef _MATERIAL_FEATURE_CLEAR_COAT
            #endif

		    #if defined(_MATERIAL_FEATURE_SUBSURFACE_SCATTERING) && !defined(_SURFACE_TYPE_TRANSPARENT)
			#define OUTPUT_SPLIT_LIGHTING
		    #endif

		    #if (SHADERPASS == SHADERPASS_PATH_TRACING) && !defined(_DOUBLESIDED_ON) && (defined(_REFRACTION_PLANE) || defined(_REFRACTION_SPHERE))
			#undef  _REFRACTION_PLANE
			#undef  _REFRACTION_SPHERE
			#define _REFRACTION_THIN
		    #endif

            #if SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_PREPASS
            #if !defined(_DISABLE_SSR_TRANSPARENT) && !defined(SHADER_UNLIT)
                #define WRITE_NORMAL_BUFFER
            #endif
            #endif

            #if SHADERPASS == SHADERPASS_MOTION_VECTORS && defined(WRITE_DECAL_BUFFER_AND_RENDERING_LAYER)
                #define WRITE_DECAL_BUFFER
            #endif

            #ifndef DEBUG_DISPLAY
                #if !defined(_SURFACE_TYPE_TRANSPARENT)
                    #if SHADERPASS == SHADERPASS_FORWARD
                    #define SHADERPASS_FORWARD_BYPASS_ALPHA_TEST
                    #elif SHADERPASS == SHADERPASS_GBUFFER
                    #define SHADERPASS_GBUFFER_BYPASS_ALPHA_TEST
                    #endif
                #endif
            #endif

            #if defined(SHADER_LIT) && !defined(_SURFACE_TYPE_TRANSPARENT)
                #define _DEFERRED_CAPABLE_MATERIAL
            #endif

            #if (defined(_TRANSPARENT_WRITES_MOTION_VEC) || defined(_TRANSPARENT_REFRACTIVE_SORT)) && defined(_SURFACE_TYPE_TRANSPARENT)
                #define _WRITE_TRANSPARENT_MOTION_VECTOR
            #endif

			CBUFFER_START( UnityPerMaterial )
			float4 _EmissionColor;
			float _AlphaCutoff;
			float _RenderQueueType;
			#ifdef _ADD_PRECOMPUTED_VELOCITY
			    float _AddPrecomputedVelocity;
			#endif
			float _StencilRef;
			float _StencilWriteMask;
			float _StencilRefDepth;
			float _StencilWriteMaskDepth;
			float _StencilRefMV;
			float _StencilWriteMaskMV;
			float _StencilRefDistortionVec;
			float _StencilWriteMaskDistortionVec;
			float _StencilWriteMaskGBuffer;
			float _StencilRefGBuffer;
			float _ZTestGBuffer;
			float _RequireSplitLighting;
			float _ReceivesSSR;
			float _SurfaceType;
			float _BlendMode;
            #ifdef SUPPORT_BLENDMODE_PRESERVE_SPECULAR_LIGHTING
			    float _EnableBlendModePreserveSpecularLighting;
            #endif
			float _SrcBlend;
			float _DstBlend;
			float _DstBlend2;
			float _AlphaSrcBlend;
			float _AlphaDstBlend;
			float _ZWrite;
			float _TransparentZWrite;
			float _CullMode;
			float _TransparentSortPriority;
			float _EnableFogOnTransparent;
			float _CullModeForward;
			float _TransparentCullMode;
			float _ZTestDepthEqualForOpaque;
			float _ZTestTransparent;
			float _TransparentBackfaceEnable;
			float _AlphaCutoffEnable;
			float _UseShadowThreshold;
			float _DoubleSidedEnable;
			float _DoubleSidedNormalMode;
			float4 _DoubleSidedConstants;
			#ifdef ASE_TESSELLATION
			    float _TessPhongStrength;
			    float _TessValue;
			    float _TessMin;
			    float _TessMax;
			    float _TessEdgeLength;
			    float _TessMaxDisp;
			#endif
			UNITY_TEXTURE_STREAMING_DEBUG_VARS;
			CBUFFER_END

            #ifdef SCENEPICKINGPASS
			float4 _SelectionID;
            #endif

            #ifdef SCENESELECTIONPASS
			int _ObjectId;
			int _PassValue;
            #endif

			/*ase_globals*/

            #ifdef DEBUG_DISPLAY
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Debug/DebugDisplay.hlsl"
            #endif

			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/NormalSurfaceGradient.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/BuiltinUtilities.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialUtilities.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphFunctions.hlsl"

			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/DecalUtilities.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitDecalData.hlsl"

        	#ifdef HAVE_VFX_MODIFICATION
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/VisualEffectVertex.hlsl"
        	#endif

			/*ase_pragma*/

			struct AttributesMesh
			{
				float3 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 tangentOS : TANGENT;
				/*ase_vdata:p=p;n=n;t=t*/
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryingsMeshToPS
			{
				SV_POSITION_QUALIFIERS float4 positionCS : SV_Position;
				float4 clipPosV : TEXCOORD0;
				float3 positionRWS : TEXCOORD1;
				float3 normalWS : TEXCOORD2;
				float4 tangentWS : TEXCOORD3;
				/*ase_interp(4,):sp=sp.xyzw;rwp=tc1;wn=tc2;wt=tc3*/
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
				#if defined(SHADER_STAGE_FRAGMENT) && defined(ASE_NEED_CULLFACE)
				FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
				#endif
			};

			/*ase_funcs*/

			void BuildSurfaceData(FragInputs fragInputs, inout SmoothSurfaceDescription surfaceDescription, float3 V, PositionInputs posInput, out SurfaceData surfaceData, out float3 bentNormalWS)
			{
				ZERO_INITIALIZE(SurfaceData, surfaceData);

				surfaceData.specularOcclusion = 1.0;
				surfaceData.thickness = 0.0;

				surfaceData.perceptualSmoothness =		surfaceDescription.Smoothness;

				// refraction
                #if defined(_REFRACTION_PLANE) || defined(_REFRACTION_SPHERE) || defined(_REFRACTION_THIN)
                    if (_EnableSSRefraction)
                    {
                        surfaceData.ior =                       surfaceDescription.RefractionIndex;
                        surfaceData.transmittanceColor =        surfaceDescription.RefractionColor;
                        surfaceData.atDistance =                surfaceDescription.RefractionDistance;
        
                        surfaceData.transmittanceMask = (1.0 - surfaceDescription.Alpha);
                        surfaceDescription.Alpha = 1.0;
                    }
                    else
                    {
                        surfaceData.ior = 1.0;
                        surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                        surfaceData.atDistance = 1.0;
                        surfaceData.transmittanceMask = 0.0;
                        surfaceDescription.Alpha = 1.0;
                    }
                #else
                    surfaceData.ior = 1.0;
                    surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                    surfaceData.atDistance = 1.0;
                    surfaceData.transmittanceMask = 0.0;
                #endif

				surfaceData.materialFeatures = MATERIALFEATUREFLAGS_LIT_STANDARD;

				#ifdef _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SUBSURFACE_SCATTERING;
				#endif

				#ifdef _MATERIAL_FEATURE_TRANSMISSION
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_TRANSMISSION;
				#endif

				#ifdef _MATERIAL_FEATURE_COLORED_TRANSMISSION
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_TRANSMISSION;
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_COLORED_TRANSMISSION;
				#endif

                #ifdef _MATERIAL_FEATURE_ANISOTROPY
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_ANISOTROPY;
                    surfaceData.normalWS = float3(0, 1, 0);
                #endif

				#ifdef _MATERIAL_FEATURE_CLEAR_COAT
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_CLEAR_COAT;
				#endif

				#ifdef _MATERIAL_FEATURE_IRIDESCENCE
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_IRIDESCENCE;
				#endif

				#ifdef _MATERIAL_FEATURE_SPECULAR_COLOR
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SPECULAR_COLOR;
				#endif

				#if defined (_MATERIAL_FEATURE_SPECULAR_COLOR) && defined (_ENERGY_CONSERVING_SPECULAR)
                    surfaceData.baseColor *= ( 1.0 - Max3( surfaceData.specularColor.r, surfaceData.specularColor.g, surfaceData.specularColor.b ) );
				#endif

				#ifdef _DOUBLESIDED_ON
				    float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
				#else
				    float3 doubleSidedConstants = float3( 1.0, 1.0, 1.0 );
				#endif

				float3 normalTS = float3(0.0f, 0.0f, 1.0f);
				normalTS = surfaceDescription.Normal;

                #ifdef DECAL_NORMAL_BLENDING
					normalTS = SurfaceGradientFromTangentSpaceNormalAndFromTBN(normalTS, fragInputs.tangentToWorld[0], fragInputs.tangentToWorld[1]);

                    #if HAVE_DECALS
                    if (_EnableDecals)
                    {
                        float alpha = 1.0;
                        alpha = surfaceDescription.Alpha;

                        DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, fragInputs, alpha);
                        ApplyDecalToSurfaceNormal(decalSurfaceData, fragInputs.tangentToWorld[2], normalTS);
                        ApplyDecalToSurfaceDataNoNormal(decalSurfaceData, surfaceData);
                     }
                    #endif

                    GetNormalWS_SG(fragInputs, normalTS, surfaceData.normalWS, doubleSidedConstants);
                #else
					GetNormalWS(fragInputs, normalTS, surfaceData.normalWS, doubleSidedConstants);

                    #if HAVE_DECALS
                    if (_EnableDecals)
                    {
                        float alpha = 1.0;
                        alpha = surfaceDescription.Alpha;

                        DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, fragInputs, alpha);
                        ApplyDecalToSurfaceNormal(decalSurfaceData, surfaceData.normalWS.xyz);
                        ApplyDecalToSurfaceDataNoNormal(decalSurfaceData, surfaceData);
                    }
                    #endif
                #endif

				surfaceData.geomNormalWS = fragInputs.tangentToWorld[2];
                surfaceData.tangentWS = normalize(fragInputs.tangentToWorld[0].xyz );
                surfaceData.tangentWS = Orthonormalize(surfaceData.tangentWS, surfaceData.normalWS);

				bentNormalWS = surfaceData.normalWS;

                #if defined(DEBUG_DISPLAY)
				    #if !defined(SHADER_STAGE_RAY_TRACING)
				    if (_DebugMipMapMode != DEBUGMIPMAPMODE_NONE)
				    {
					   #ifdef FRAG_INPUTS_USE_TEXCOORD0
                           surfaceData.baseColor = GET_TEXTURE_STREAMING_DEBUG(posInput.positionSS, fragInputs.texCoord0);
                       #else
                           surfaceData.baseColor = GET_TEXTURE_STREAMING_DEBUG_NO_UV(posInput.positionSS);
                       #endif
					   surfaceData.metallic = 0;
				    }
					#endif
                     ApplyDebugToSurfaceData(fragInputs.tangentToWorld, surfaceData);
                #endif

                #if defined(_SPECULAR_OCCLUSION_CUSTOM)
                #elif defined(_SPECULAR_OCCLUSION_FROM_AO_BENT_NORMAL)
                    surfaceData.specularOcclusion = GetSpecularOcclusionFromBentAO(V, bentNormalWS, surfaceData.normalWS, surfaceData.ambientOcclusion, PerceptualSmoothnessToPerceptualRoughness(surfaceData.perceptualSmoothness));
                #elif defined(_AMBIENT_OCCLUSION) && defined(_SPECULAR_OCCLUSION_FROM_AO)
                    surfaceData.specularOcclusion = GetSpecularOcclusionFromAmbientOcclusion(ClampNdotV(dot(surfaceData.normalWS, V)), surfaceData.ambientOcclusion, PerceptualSmoothnessToRoughness(surfaceData.perceptualSmoothness));
                #endif

                #ifdef _ENABLE_GEOMETRIC_SPECULAR_AA
                    surfaceData.perceptualSmoothness = GeometricNormalFiltering(surfaceData.perceptualSmoothness, fragInputs.tangentToWorld[2], surfaceDescription.SpecularAAScreenSpaceVariance, surfaceDescription.SpecularAAThreshold);
                #endif
			}

			// Get Surface And BuiltinData
			void GetSurfaceAndBuiltinData(SmoothSurfaceDescription surfaceDescription, FragInputs fragInputs, float3 V, inout PositionInputs posInput, out SurfaceData surfaceData, out BuiltinData builtinData)
			{
				#ifdef LOD_FADE_CROSSFADE
                    LODDitheringTransition(ComputeFadeMaskSeed(V, posInput.positionSS), unity_LODFade.x);
				#endif

                #ifdef _DOUBLESIDED_ON
                    float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
                #else
                    float3 doubleSidedConstants = float3(1.0, 1.0, 1.0);
                #endif
                ApplyDoubleSidedFlipOrMirror(fragInputs, doubleSidedConstants);

				#ifdef _ALPHATEST_ON
                    DoAlphaTest( surfaceDescription.Alpha, surfaceDescription.AlphaClipThreshold );
				#endif

				#ifdef _ALPHATEST_SHADOW_ON
                    DoAlphaTest( surfaceDescription.Alpha, surfaceDescription.AlphaClipThresholdShadow);
				#endif

				#ifdef _DEPTHOFFSET_ON
                    ApplyDepthOffsetPositionInput(V, surfaceDescription.DepthOffset, GetViewForwardDir(), GetWorldToHClipMatrix(), posInput);
				#endif

                float3 bentNormalWS;
                BuildSurfaceData(fragInputs, surfaceDescription, V, posInput, surfaceData, bentNormalWS);
                InitBuiltinData(posInput, surfaceDescription.Alpha, bentNormalWS, -fragInputs.tangentToWorld[2], fragInputs.texCoord1, fragInputs.texCoord2, builtinData);

				#ifdef _DEPTHOFFSET_ON
                    builtinData.depthOffset = surfaceDescription.DepthOffset;
				#endif

                #ifdef _ALPHATEST_ON
                    builtinData.alphaClipTreshold = surfaceDescription.AlphaClipThreshold;
                #endif

                #ifdef UNITY_VIRTUAL_TEXTURING
                    builtinData.vtPackedFeedback = surfaceDescription.VTPackedFeedback;
                #endif

				#ifdef ASE_BAKEDGI
                    builtinData.bakeDiffuseLighting = surfaceDescription.BakedGI;
				#endif

				#ifdef ASE_BAKEDBACKGI
                    builtinData.backBakeDiffuseLighting = surfaceDescription.BakedBackGI;
				#endif

                builtinData.emissiveColor = surfaceDescription.Emission;

                PostInitBuiltinData(V, posInput, surfaceData, builtinData);
			}

			#if (defined(WRITE_DECAL_BUFFER) && !defined(_DISABLE_DECALS)) || defined(WRITE_RENDERING_LAYER)
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/DecalPrepassBuffer.hlsl"
			#endif

			PackedVaryingsMeshToPS VertexFunction(AttributesMesh inputMesh /*ase_vert_input*/)
			{
				PackedVaryingsMeshToPS output;

				UNITY_SETUP_INSTANCE_ID(inputMesh);
				UNITY_TRANSFER_INSTANCE_ID(inputMesh, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( output );

				/*ase_vert_code:inputMesh=AttributesMesh;output=PackedVaryingsMeshToPS*/

				#ifdef ASE_ABSOLUTE_VERTEX_POS
				float3 defaultVertexValue = inputMesh.positionOS.xyz;
				#else
				float3 defaultVertexValue = float3( 0, 0, 0 );
				#endif
				float3 vertexValue = /*ase_vert_out:Vertex Offset;Float3;4;-1;_VertexOffset*/ defaultVertexValue /*end*/;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
				inputMesh.positionOS.xyz = vertexValue;
				#else
				inputMesh.positionOS.xyz += vertexValue;
				#endif

				inputMesh.normalOS = /*ase_vert_out:Vertex Normal;Float3;5;-1;_VertexNormal*/ inputMesh.normalOS /*end*/;
				inputMesh.tangentOS = /*ase_vert_out:Vertex Tangent;Float4;7;-1;_VertexTangent*/ inputMesh.tangentOS /*end*/;

				float3 positionRWS = TransformObjectToWorld(inputMesh.positionOS);
				float3 normalWS = TransformObjectToWorldNormal(inputMesh.normalOS);
				float4 tangentWS = float4(TransformObjectToWorldDir(inputMesh.tangentOS.xyz), inputMesh.tangentOS.w);

				output.positionCS = TransformWorldToHClip(positionRWS);
				output.clipPosV = output.positionCS;
				output.positionRWS = positionRWS;
				output.normalWS = normalWS;
				output.tangentWS = tangentWS;
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float3 positionOS : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				float4 tangentOS : TANGENT;
				/*ase_vcontrol*/
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl Vert ( AttributesMesh v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.positionOS = v.positionOS;
				o.normalOS = v.normalOS;
				o.tangentOS = v.tangentOS;
				/*ase_control_code:v=AttributesMesh;o=VertexControl*/
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = /*ase_inline_begin*/_TessValue/*ase_inline_end*/; float tessMin = /*ase_inline_begin*/_TessMin/*ase_inline_end*/; float tessMax = /*ase_inline_begin*/_TessMax/*ase_inline_end*/;
				float edgeLength = /*ase_inline_begin*/_TessEdgeLength/*ase_inline_end*/; float tessMaxDisp = /*ase_inline_begin*/_TessMaxDisp/*ase_inline_end*/;
				#if (SHADEROPTIONS_CAMERA_RELATIVE_RENDERING != 0)
				float3 cameraPos = 0;
				#else
				float3 cameraPos = _WorldSpaceCameraPos;
				#endif
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(float4(v[0].positionOS,1), float4(v[1].positionOS,1), float4(v[2].positionOS,1), tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), cameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(float4(v[0].positionOS,1), float4(v[1].positionOS,1), float4(v[2].positionOS,1), edgeLength, GetObjectToWorldMatrix(), cameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(float4(v[0].positionOS,1), float4(v[1].positionOS,1), float4(v[2].positionOS,1), edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), cameraPos, _ScreenParams, _FrustumPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			PackedVaryingsMeshToPS DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				AttributesMesh o = (AttributesMesh) 0;
				o.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				o.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				o.tangentOS = patch[0].tangentOS * bary.x + patch[1].tangentOS * bary.y + patch[2].tangentOS * bary.z;
				/*ase_domain_code:patch=VertexControl;o=AttributesMesh;bary=SV_DomainLocation*/
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.positionOS.xyz - patch[i].normalOS * (dot(o.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = /*ase_inline_begin*/_TessPhongStrength/*ase_inline_end*/;
				o.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			PackedVaryingsMeshToPS Vert ( AttributesMesh v )
			{
				return VertexFunction( v );
			}
			#endif

			#if defined(WRITE_NORMAL_BUFFER) && defined(WRITE_MSAA_DEPTH)
			#define SV_TARGET_DECAL SV_Target2
			#elif defined(WRITE_NORMAL_BUFFER) || defined(WRITE_MSAA_DEPTH)
			#define SV_TARGET_DECAL SV_Target1
			#else
			#define SV_TARGET_DECAL SV_Target0
			#endif

			void Frag( PackedVaryingsMeshToPS packedInput
						#if defined(SCENESELECTIONPASS) || defined(SCENEPICKINGPASS)
						, out float4 outColor : SV_Target0
						#else
							#ifdef WRITE_MSAA_DEPTH
							, out float4 depthColor : SV_Target0
								#ifdef WRITE_NORMAL_BUFFER
								, out float4 outNormalBuffer : SV_Target1
								#endif
							#else
								#ifdef WRITE_NORMAL_BUFFER
								, out float4 outNormalBuffer : SV_Target0
								#endif
							#endif

							#if (defined(WRITE_DECAL_BUFFER) && !defined(_DISABLE_DECALS)) || defined(WRITE_RENDERING_LAYER)
							, out float4 outDecalBuffer : SV_TARGET_DECAL
							#endif
						#endif

						#if defined(_DEPTHOFFSET_ON) && !defined(SCENEPICKINGPASS)
						, out float outputDepth : DEPTH_OFFSET_SEMANTIC
						#endif
						/*ase_frag_input*/
					)
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(packedInput);
				UNITY_SETUP_INSTANCE_ID(packedInput);

				/*ase_local_var:rwp*/float3 PositionRWS = packedInput.positionRWS;
				/*ase_local_var:wvd*/float3 V = GetWorldSpaceNormalizeViewDir( packedInput.positionRWS );
				/*ase_local_var:wn*/float3 NormalWS = packedInput.normalWS;
				/*ase_local_var:wt*/float4 TangentWS = packedInput.tangentWS;
				/*ase_local_var:sp*/float4 ClipPos = packedInput.clipPosV;
				/*ase_local_var:spu*/float4 ScreenPos = ComputeScreenPos( packedInput.clipPosV, _ProjectionParams.x );
		
				FragInputs input;
				ZERO_INITIALIZE(FragInputs, input);
				input.positionSS = packedInput.positionCS;
				input.positionRWS = PositionRWS;
				input.tangentToWorld = BuildTangentToWorld(TangentWS, NormalWS);

				#if _DOUBLESIDED_ON && SHADER_STAGE_FRAGMENT
				input.isFrontFace = IS_FRONT_VFACE( packedInput.cullFace, true, false);
				#elif SHADER_STAGE_FRAGMENT
				#if defined(ASE_NEED_CULLFACE)
				input.isFrontFace = IS_FRONT_VFACE( packedInput.cullFace, true, false );
				#endif
				#endif
				/*ase_local_var:vf*/half IsFrontFace = input.isFrontFace;

				PositionInputs posInput = GetPositionInput(input.positionSS.xy, _ScreenSize.zw, input.positionSS.z, input.positionSS.w, input.positionRWS);

				SmoothSurfaceDescription surfaceDescription = (SmoothSurfaceDescription)0;
				/*ase_frag_code:packedInput=PackedVaryingsMeshToPS*/
				surfaceDescription.Normal = /*ase_frag_out:Normal;Float3;0;-1;_Normal*/float3( 0, 0, 1 )/*end*/;
				surfaceDescription.Smoothness = /*ase_frag_out:Smoothness;Float;1;-1;_Smoothness*/1/*end*/;
				surfaceDescription.Alpha = /*ase_frag_out:Alpha;Float;2;-1;_Alpha*/1/*end*/;

				#ifdef _ALPHATEST_ON
				surfaceDescription.AlphaClipThreshold = /*ase_frag_out:Alpha Clip Threshold;Float;3;-1;_AlphaClip*/_AlphaCutoff/*end*/;
				#endif

				#ifdef _DEPTHOFFSET_ON
				surfaceDescription.DepthOffset = /*ase_frag_out:Depth Offset;Float;6;-1;_DepthOffset*/0/*end*/;
				#endif

				SurfaceData surfaceData;
				BuiltinData builtinData;
				GetSurfaceAndBuiltinData(surfaceDescription, input, V, posInput, surfaceData, builtinData);

                #if defined(_DEPTHOFFSET_ON) && !defined(SCENEPICKINGPASS)
				outputDepth = posInput.deviceDepth;
				#endif

                #if SHADERPASS == SHADERPASS_SHADOWS
                float bias = max(abs(ddx(posInput.deviceDepth)), abs(ddy(posInput.deviceDepth))) * _SlopeScaleDepthBias;
                outputDepth += bias;
                #endif

				#ifdef SCENESELECTIONPASS
    				outColor = float4(_ObjectId, _PassValue, 1.0, 1.0);
				#elif defined(SCENEPICKINGPASS)
    				outColor = unity_SelectionID;
				#else
    				#ifdef WRITE_MSAA_DEPTH
    				depthColor = packedInput.positionCS.z;
    				depthColor.a = SharpenAlpha(builtinData.opacity, builtinData.alphaClipTreshold);
    				#endif

    				#if defined(WRITE_NORMAL_BUFFER)
    				EncodeIntoNormalBuffer(ConvertSurfaceDataToNormalData(surfaceData), outNormalBuffer);
    				#endif

    				#if (defined(WRITE_DECAL_BUFFER) && !defined(_DISABLE_DECALS)) || defined(WRITE_RENDERING_LAYER)
    				DecalPrepassData decalPrepassData;
                    #ifdef _DISABLE_DECALS
				    ZERO_INITIALIZE(DecalPrepassData, decalPrepassData);
                    #else
    				decalPrepassData.geomNormalWS = surfaceData.geomNormalWS;
                    #endif
    				decalPrepassData.renderingLayerMask = GetMeshRenderingLayerMask();
    				EncodeIntoDecalPrepassBuffer(decalPrepassData, outDecalBuffer);
    				#endif

				#endif // SCENESELECTIONPASS

			}

			ENDHLSL
		}

		/*ase_pass*/
		Pass
		{
			/*ase_hide_pass*/
			Name "MotionVectors"
			Tags 
			{ 
				"LightMode" = "MotionVectors" 
		    }

			Cull [_CullMode]

			ZWrite On

			Stencil
			{
				WriteMask [_StencilWriteMaskMV]
				Ref [_StencilRefMV]
		        CompFront Always
		        PassFront Replace
		        CompBack Always
		        PassBack Replace
			}

			HLSLPROGRAM
			#pragma multi_compile _ DOTS_INSTANCING_ON

            #pragma shader_feature _ _SURFACE_TYPE_TRANSPARENT
            #pragma shader_feature_local _ _TRANSPARENT_WRITES_MOTION_VEC _TRANSPARENT_REFRACTIVE_SORT
            #pragma shader_feature_local_fragment _ _ENABLE_FOG_ON_TRANSPARENT

            #pragma multi_compile _ WRITE_NORMAL_BUFFER
            #pragma multi_compile_fragment _ WRITE_MSAA_DEPTH
            #pragma multi_compile_fragment _ WRITE_DECAL_BUFFER_AND_RENDERING_LAYER

			#ifdef WRITE_DECAL_BUFFER_AND_RENDERING_LAYER
			#define WRITE_DECAL_BUFFER
			#endif

			#pragma vertex Vert
			#pragma fragment Frag

            #define SHADERPASS SHADERPASS_MOTION_VECTORS

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/GeometricTools.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Tessellation.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
            #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"

			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"

            #ifdef RAYTRACING_SHADER_GRAPH_DEFAULT
                #define RAYTRACING_SHADER_GRAPH_HIGH
            #endif
        
            #ifdef RAYTRACING_SHADER_GRAPH_RAYTRACED
               #define RAYTRACING_SHADER_GRAPH_LOW
            #endif

            #ifndef SHADER_UNLIT
            #if defined(_DOUBLESIDED_ON) && !defined(VARYINGS_NEED_CULLFACE)
                #define VARYINGS_NEED_CULLFACE
            #endif
            #endif

			#if defined(_DOUBLESIDED_ON) && !defined(ASE_NEED_CULLFACE)
			     #define ASE_NEED_CULLFACE 1
			#endif

            #if _MATERIAL_FEATURE_COLORED_TRANSMISSION
            #undef _MATERIAL_FEATURE_CLEAR_COAT
            #endif

		    #if defined(_MATERIAL_FEATURE_SUBSURFACE_SCATTERING) && !defined(_SURFACE_TYPE_TRANSPARENT)
			#define OUTPUT_SPLIT_LIGHTING
		    #endif

		    #if (SHADERPASS == SHADERPASS_PATH_TRACING) && !defined(_DOUBLESIDED_ON) && (defined(_REFRACTION_PLANE) || defined(_REFRACTION_SPHERE))
			#undef  _REFRACTION_PLANE
			#undef  _REFRACTION_SPHERE
			#define _REFRACTION_THIN
		    #endif

            #if SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_PREPASS
            #if !defined(_DISABLE_SSR_TRANSPARENT) && !defined(SHADER_UNLIT)
                #define WRITE_NORMAL_BUFFER
            #endif
            #endif

            #if SHADERPASS == SHADERPASS_MOTION_VECTORS && defined(WRITE_DECAL_BUFFER_AND_RENDERING_LAYER)
                #define WRITE_DECAL_BUFFER
            #endif

            #ifndef DEBUG_DISPLAY
                #if !defined(_SURFACE_TYPE_TRANSPARENT)
                    #if SHADERPASS == SHADERPASS_FORWARD
                    #define SHADERPASS_FORWARD_BYPASS_ALPHA_TEST
                    #elif SHADERPASS == SHADERPASS_GBUFFER
                    #define SHADERPASS_GBUFFER_BYPASS_ALPHA_TEST
                    #endif
                #endif
            #endif

            #if defined(SHADER_LIT) && !defined(_SURFACE_TYPE_TRANSPARENT)
                #define _DEFERRED_CAPABLE_MATERIAL
            #endif
        
            #if (defined(_TRANSPARENT_WRITES_MOTION_VEC) || defined(_TRANSPARENT_REFRACTIVE_SORT)) && defined(_SURFACE_TYPE_TRANSPARENT)
                #define _WRITE_TRANSPARENT_MOTION_VECTOR
            #endif

			CBUFFER_START( UnityPerMaterial )
			float4 _EmissionColor;
			float _AlphaCutoff;
			float _RenderQueueType;
			#ifdef _ADD_PRECOMPUTED_VELOCITY
			    float _AddPrecomputedVelocity;
			#endif
			float _StencilRef;
			float _StencilWriteMask;
			float _StencilRefDepth;
			float _StencilWriteMaskDepth;
			float _StencilRefMV;
			float _StencilWriteMaskMV;
			float _StencilRefDistortionVec;
			float _StencilWriteMaskDistortionVec;
			float _StencilWriteMaskGBuffer;
			float _StencilRefGBuffer;
			float _ZTestGBuffer;
			float _RequireSplitLighting;
			float _ReceivesSSR;
			float _SurfaceType;
			float _BlendMode;
            #ifdef SUPPORT_BLENDMODE_PRESERVE_SPECULAR_LIGHTING
			    float _EnableBlendModePreserveSpecularLighting;
            #endif
			float _SrcBlend;
			float _DstBlend;
			float _DstBlend2;
			float _AlphaSrcBlend;
			float _AlphaDstBlend;
			float _ZWrite;
			float _TransparentZWrite;
			float _CullMode;
			float _TransparentSortPriority;
			float _EnableFogOnTransparent;
			float _CullModeForward;
			float _TransparentCullMode;
			float _ZTestDepthEqualForOpaque;
			float _ZTestTransparent;
			float _TransparentBackfaceEnable;
			float _AlphaCutoffEnable;
			float _UseShadowThreshold;
			float _DoubleSidedEnable;
			float _DoubleSidedNormalMode;
			float4 _DoubleSidedConstants;
			#ifdef ASE_TESSELLATION
			    float _TessPhongStrength;
			    float _TessValue;
			    float _TessMin;
			    float _TessMax;
			    float _TessEdgeLength;
			    float _TessMaxDisp;
			#endif
			UNITY_TEXTURE_STREAMING_DEBUG_VARS;
			CBUFFER_END

            #ifdef SCENEPICKINGPASS
			float4 _SelectionID;
            #endif

            #ifdef SCENESELECTIONPASS
			int _ObjectId;
			int _PassValue;
            #endif

			/*ase_globals*/

            #ifdef DEBUG_DISPLAY
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Debug/DebugDisplay.hlsl"
            #endif

			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/NormalSurfaceGradient.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/BuiltinUtilities.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialUtilities.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphFunctions.hlsl"

            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/DecalUtilities.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitDecalData.hlsl"

        	#ifdef HAVE_VFX_MODIFICATION
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/VisualEffectVertex.hlsl"
        	#endif

			/*ase_pragma*/

			struct AttributesMesh
			{
				float3 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 tangentOS : TANGENT;
				float3 previousPositionOS : TEXCOORD4;
				float3 precomputedVelocity : TEXCOORD5;
				/*ase_vdata:p=p;n=n;t=t;uv4=tc4;uv5=tc5*/
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryingsMeshToPS
			{
				SV_POSITION_QUALIFIERS float4 vmeshPositionCS : SV_Position;
				float3 vmeshInterp00 : TEXCOORD0;
				float3 vpassInterpolators0 : TEXCOORD1; //interpolators0
				float3 vpassInterpolators1 : TEXCOORD2; //interpolators1
				/*ase_interp(3,):sp=sp.xyzw*/
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
				#if defined(SHADER_STAGE_FRAGMENT) && defined(ASE_NEED_CULLFACE)
				FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
				#endif
			};

			/*ase_funcs*/

			void BuildSurfaceData(FragInputs fragInputs, inout SmoothSurfaceDescription surfaceDescription, float3 V, PositionInputs posInput, out SurfaceData surfaceData, out float3 bentNormalWS)
			{
				ZERO_INITIALIZE(SurfaceData, surfaceData);

				surfaceData.specularOcclusion = 1.0;
				surfaceData.thickness = 0.0;
				surfaceData.perceptualSmoothness =		surfaceDescription.Smoothness;

				// refraction
                #if defined(_REFRACTION_PLANE) || defined(_REFRACTION_SPHERE) || defined(_REFRACTION_THIN)
                    if (_EnableSSRefraction)
                    {
                        surfaceData.ior =                       surfaceDescription.RefractionIndex;
                        surfaceData.transmittanceColor =        surfaceDescription.RefractionColor;
                        surfaceData.atDistance =                surfaceDescription.RefractionDistance;
        
                        surfaceData.transmittanceMask = (1.0 - surfaceDescription.Alpha);
                        surfaceDescription.Alpha = 1.0;
                    }
                    else
                    {
                        surfaceData.ior = 1.0;
                        surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                        surfaceData.atDistance = 1.0;
                        surfaceData.transmittanceMask = 0.0;
                        surfaceDescription.Alpha = 1.0;
                    }
                #else
                    surfaceData.ior = 1.0;
                    surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                    surfaceData.atDistance = 1.0;
                    surfaceData.transmittanceMask = 0.0;
                #endif

				surfaceData.materialFeatures = MATERIALFEATUREFLAGS_LIT_STANDARD;

				#ifdef _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SUBSURFACE_SCATTERING;
				#endif

				#ifdef _MATERIAL_FEATURE_TRANSMISSION
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_TRANSMISSION;
				#endif

				#ifdef _MATERIAL_FEATURE_COLORED_TRANSMISSION
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_TRANSMISSION;
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_COLORED_TRANSMISSION;
				#endif

                #ifdef _MATERIAL_FEATURE_ANISOTROPY
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_ANISOTROPY;
                    surfaceData.normalWS = float3(0, 1, 0);
                #endif

				#ifdef _MATERIAL_FEATURE_CLEAR_COAT
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_CLEAR_COAT;
				#endif

				#ifdef _MATERIAL_FEATURE_IRIDESCENCE
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_IRIDESCENCE;
				#endif

				#ifdef _MATERIAL_FEATURE_SPECULAR_COLOR
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SPECULAR_COLOR;
				#endif

				#if defined (_MATERIAL_FEATURE_SPECULAR_COLOR) && defined (_ENERGY_CONSERVING_SPECULAR)
                    surfaceData.baseColor *= ( 1.0 - Max3( surfaceData.specularColor.r, surfaceData.specularColor.g, surfaceData.specularColor.b ) );
				#endif

				#ifdef _DOUBLESIDED_ON
                    float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
				#else
                    float3 doubleSidedConstants = float3( 1.0, 1.0, 1.0 );
				#endif

				float3 normalTS = float3(0.0f, 0.0f, 1.0f);
				normalTS = surfaceDescription.Normal;

                #ifdef DECAL_NORMAL_BLENDING
					normalTS = SurfaceGradientFromTangentSpaceNormalAndFromTBN(normalTS, fragInputs.tangentToWorld[0], fragInputs.tangentToWorld[1]);

                    #if HAVE_DECALS
                    if (_EnableDecals)
                    {
                        float alpha = 1.0;
                        alpha = surfaceDescription.Alpha;

                        DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, fragInputs, alpha);
                        ApplyDecalToSurfaceNormal(decalSurfaceData, fragInputs.tangentToWorld[2], normalTS);
                        ApplyDecalToSurfaceDataNoNormal(decalSurfaceData, surfaceData);
                     }
                    #endif

                    GetNormalWS_SG(fragInputs, normalTS, surfaceData.normalWS, doubleSidedConstants);
                #else
					GetNormalWS(fragInputs, normalTS, surfaceData.normalWS, doubleSidedConstants);

                    #if HAVE_DECALS
                    if (_EnableDecals)
                    {
                        float alpha = 1.0;
                        alpha = surfaceDescription.Alpha;

                        DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, fragInputs, alpha);
                        ApplyDecalToSurfaceNormal(decalSurfaceData, surfaceData.normalWS.xyz);
                        ApplyDecalToSurfaceDataNoNormal(decalSurfaceData, surfaceData);
                    }
                    #endif
                #endif

				surfaceData.geomNormalWS = fragInputs.tangentToWorld[2];
                surfaceData.tangentWS = normalize(fragInputs.tangentToWorld[0].xyz );
                surfaceData.tangentWS = Orthonormalize(surfaceData.tangentWS, surfaceData.normalWS);

				bentNormalWS = surfaceData.normalWS;

				#if defined(DEBUG_DISPLAY)
				    #if !defined(SHADER_STAGE_RAY_TRACING)
				    if (_DebugMipMapMode != DEBUGMIPMAPMODE_NONE)
				    {
					   #ifdef FRAG_INPUTS_USE_TEXCOORD0
                           surfaceData.baseColor = GET_TEXTURE_STREAMING_DEBUG(posInput.positionSS, fragInputs.texCoord0);
                       #else
                           surfaceData.baseColor = GET_TEXTURE_STREAMING_DEBUG_NO_UV(posInput.positionSS);
                       #endif
					   surfaceData.metallic = 0;
				    }
					#endif
				    ApplyDebugToSurfaceData(fragInputs.tangentToWorld, surfaceData); 
				#endif

                #if defined(_SPECULAR_OCCLUSION_CUSTOM)
                #elif defined(_SPECULAR_OCCLUSION_FROM_AO_BENT_NORMAL)
                    surfaceData.specularOcclusion = GetSpecularOcclusionFromBentAO(V, bentNormalWS, surfaceData.normalWS, surfaceData.ambientOcclusion, PerceptualSmoothnessToPerceptualRoughness(surfaceData.perceptualSmoothness));
                #elif defined(_AMBIENT_OCCLUSION) && defined(_SPECULAR_OCCLUSION_FROM_AO)
                    surfaceData.specularOcclusion = GetSpecularOcclusionFromAmbientOcclusion(ClampNdotV(dot(surfaceData.normalWS, V)), surfaceData.ambientOcclusion, PerceptualSmoothnessToRoughness(surfaceData.perceptualSmoothness));
                #endif

                #ifdef _ENABLE_GEOMETRIC_SPECULAR_AA
                    surfaceData.perceptualSmoothness = GeometricNormalFiltering(surfaceData.perceptualSmoothness, fragInputs.tangentToWorld[2], surfaceDescription.SpecularAAScreenSpaceVariance, surfaceDescription.SpecularAAThreshold);
                #endif
			}

			// Get Surface And BuiltinData
			void GetSurfaceAndBuiltinData(SmoothSurfaceDescription surfaceDescription, FragInputs fragInputs, float3 V, inout PositionInputs posInput, out SurfaceData surfaceData, out BuiltinData builtinData)
			{
				#ifdef LOD_FADE_CROSSFADE
                    LODDitheringTransition(ComputeFadeMaskSeed(V, posInput.positionSS), unity_LODFade.x);
				#endif

                #ifdef _DOUBLESIDED_ON
                    float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
                #else
                    float3 doubleSidedConstants = float3(1.0, 1.0, 1.0);
                #endif
                ApplyDoubleSidedFlipOrMirror(fragInputs, doubleSidedConstants);

				#ifdef _ALPHATEST_ON
                    DoAlphaTest( surfaceDescription.Alpha, surfaceDescription.AlphaClipThreshold );
				#endif

				#ifdef _ALPHATEST_SHADOW_ON
                    DoAlphaTest( surfaceDescription.Alpha, surfaceDescription.AlphaClipThresholdShadow);
				#endif

				#ifdef _DEPTHOFFSET_ON
                    ApplyDepthOffsetPositionInput(V, surfaceDescription.DepthOffset, GetViewForwardDir(), GetWorldToHClipMatrix(), posInput);
				#endif

                float3 bentNormalWS;
                BuildSurfaceData(fragInputs, surfaceDescription, V, posInput, surfaceData, bentNormalWS);
                InitBuiltinData(posInput, surfaceDescription.Alpha, bentNormalWS, -fragInputs.tangentToWorld[2], fragInputs.texCoord1, fragInputs.texCoord2, builtinData);

				#ifdef _DEPTHOFFSET_ON
                    builtinData.depthOffset = surfaceDescription.DepthOffset;
				#endif

                #ifdef _ALPHATEST_ON
                    builtinData.alphaClipTreshold = surfaceDescription.AlphaClipThreshold;
                #endif

                #ifdef UNITY_VIRTUAL_TEXTURING
                    builtinData.vtPackedFeedback = surfaceDescription.VTPackedFeedback;
                #endif

				#ifdef ASE_BAKEDGI
                    builtinData.bakeDiffuseLighting = surfaceDescription.BakedGI;
				#endif

				#ifdef ASE_BAKEDBACKGI
                    builtinData.backBakeDiffuseLighting = surfaceDescription.BakedBackGI;
				#endif

                builtinData.emissiveColor = surfaceDescription.Emission;

                PostInitBuiltinData(V, posInput, surfaceData, builtinData);
			}

			AttributesMesh ApplyMeshModification(AttributesMesh inputMesh, float3 timeParameters, inout PackedVaryingsMeshToPS output/*ase_vert_input*/ )
			{
				_TimeParameters.xyz = timeParameters;
				/*ase_vert_code:inputMesh=AttributesMesh;output=PackedVaryingsMeshToPS*/

				#ifdef ASE_ABSOLUTE_VERTEX_POS
				float3 defaultVertexValue = inputMesh.positionOS.xyz;
				#else
				float3 defaultVertexValue = float3( 0, 0, 0 );
				#endif
				float3 vertexValue = /*ase_vert_out:Vertex Offset;Float3;4;-1;_VertexOffset*/ defaultVertexValue /*end*/;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
				inputMesh.positionOS.xyz = vertexValue;
				#else
				inputMesh.positionOS.xyz += vertexValue;
				#endif
				inputMesh.normalOS = /*ase_vert_out:Vertex Normal;Float3;5;-1;_VertexNormal*/ inputMesh.normalOS /*end*/;
				inputMesh.tangentOS = /*ase_vert_out:Vertex Tangent;Float4;27;-1;_VertexTangent*/inputMesh.tangentOS/*end*/;
				return inputMesh;
			}

			PackedVaryingsMeshToPS VertexFunction(AttributesMesh inputMesh)
			{
				PackedVaryingsMeshToPS output = (PackedVaryingsMeshToPS)0;
				AttributesMesh defaultMesh = inputMesh;

				UNITY_SETUP_INSTANCE_ID(inputMesh);
				UNITY_TRANSFER_INSTANCE_ID(inputMesh, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( output );

				inputMesh = ApplyMeshModification( inputMesh, _TimeParameters.xyz, output);

				float3 positionRWS = TransformObjectToWorld(inputMesh.positionOS);
				float3 normalWS = TransformObjectToWorldNormal(inputMesh.normalOS);

				float3 VMESHpositionRWS = positionRWS;
				float4 VMESHpositionCS = TransformWorldToHClip(positionRWS);

				float4 VPASSpreviousPositionCS;
				float4 VPASSpositionCS = mul(UNITY_MATRIX_UNJITTERED_VP, float4(VMESHpositionRWS, 1.0));

				bool forceNoMotion = unity_MotionVectorsParams.y == 0.0;
				if (forceNoMotion)
				{
					VPASSpreviousPositionCS = float4(0.0, 0.0, 0.0, 1.0);
				}
				else
				{
					bool hasDeformation = unity_MotionVectorsParams.x > 0.0;
					float3 effectivePositionOS = (hasDeformation ? inputMesh.previousPositionOS : defaultMesh.positionOS);
					#if defined(_ADD_PRECOMPUTED_VELOCITY)
					effectivePositionOS -= inputMesh.precomputedVelocity;
					#endif

					#if defined(HAVE_MESH_MODIFICATION)
						AttributesMesh previousMesh = defaultMesh;
						previousMesh.positionOS = effectivePositionOS ;
						PackedVaryingsMeshToPS test = (PackedVaryingsMeshToPS)0;
						float3 curTime = _TimeParameters.xyz;
						previousMesh = ApplyMeshModification(previousMesh, _LastTimeParameters.xyz, test);
						_TimeParameters.xyz = curTime;
						float3 previousPositionRWS = TransformPreviousObjectToWorld(previousMesh.positionOS);
					#else
						float3 previousPositionRWS = TransformPreviousObjectToWorld(effectivePositionOS);
					#endif

					#ifdef ATTRIBUTES_NEED_NORMAL
						float3 normalWS = TransformPreviousObjectToWorldNormal(defaultMesh.normalOS);
					#else
						float3 normalWS = float3(0.0, 0.0, 0.0);
					#endif

					#if defined(HAVE_VERTEX_MODIFICATION)
						ApplyVertexModification(inputMesh, normalWS, previousPositionRWS, _LastTimeParameters.xyz);
					#endif

					#ifdef _WRITE_TRANSPARENT_MOTION_VECTOR
						if (_TransparentCameraOnlyMotionVectors > 0)
						{
							previousPositionRWS = VMESHpositionRWS.xyz;
						}
					#endif

					VPASSpreviousPositionCS = mul(UNITY_MATRIX_PREV_VP, float4(previousPositionRWS, 1.0));
				}

				output.vmeshPositionCS = VMESHpositionCS;
				output.vmeshInterp00.xyz = VMESHpositionRWS;

				output.vpassInterpolators0 = float3(VPASSpositionCS.xyw);
				output.vpassInterpolators1 = float3(VPASSpreviousPositionCS.xyw);
				return output;
			}

			#if (defined(WRITE_DECAL_BUFFER) && !defined(_DISABLE_DECALS)) || defined(WRITE_RENDERING_LAYER)
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/DecalPrepassBuffer.hlsl"
			#endif

			#if ( 0 ) // TEMPORARY: defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float3 positionOS : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				float4 tangentOS : TANGENT;
				float3 previousPositionOS : TEXCOORD4;
				float3 precomputedVelocity : TEXCOORD5;
				/*ase_vcontrol*/
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl Vert ( AttributesMesh v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.positionOS = v.positionOS;
				o.normalOS = v.normalOS;
				o.tangentOS = v.tangentOS;
				o.previousPositionOS = v.previousPositionOS;
				#if defined (_ADD_PRECOMPUTED_VELOCITY)
				o.precomputedVelocity = v.precomputedVelocity;
				#endif
				/*ase_control_code:v=AttributesMesh;o=VertexControl*/
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = /*ase_inline_begin*/_TessValue/*ase_inline_end*/; float tessMin = /*ase_inline_begin*/_TessMin/*ase_inline_end*/; float tessMax = /*ase_inline_begin*/_TessMax/*ase_inline_end*/;
				float edgeLength = /*ase_inline_begin*/_TessEdgeLength/*ase_inline_end*/; float tessMaxDisp = /*ase_inline_begin*/_TessMaxDisp/*ase_inline_end*/;
				#if (SHADEROPTIONS_CAMERA_RELATIVE_RENDERING != 0)
				float3 cameraPos = 0;
				#else
				float3 cameraPos = _WorldSpaceCameraPos;
				#endif
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(float4(v[0].positionOS,1), float4(v[1].positionOS,1), float4(v[2].positionOS,1), tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), cameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(float4(v[0].positionOS,1), float4(v[1].positionOS,1), float4(v[2].positionOS,1), edgeLength, GetObjectToWorldMatrix(), cameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(float4(v[0].positionOS,1), float4(v[1].positionOS,1), float4(v[2].positionOS,1), edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), cameraPos, _ScreenParams, _FrustumPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			PackedVaryingsMeshToPS DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				AttributesMesh o = (AttributesMesh) 0;
				o.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				o.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				o.tangentOS = patch[0].tangentOS * bary.x + patch[1].tangentOS * bary.y + patch[2].tangentOS * bary.z;
				o.previousPositionOS = patch[0].previousPositionOS * bary.x + patch[1].previousPositionOS * bary.y + patch[2].previousPositionOS * bary.z;
				#if defined (_ADD_PRECOMPUTED_VELOCITY)
					o.precomputedVelocity = patch[0].precomputedVelocity * bary.x + patch[1].precomputedVelocity * bary.y + patch[2].precomputedVelocity * bary.z;
				#endif
				/*ase_domain_code:patch=VertexControl;o=AttributesMesh;bary=SV_DomainLocation*/
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.positionOS.xyz - patch[i].normalOS * (dot(o.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = /*ase_inline_begin*/_TessPhongStrength/*ase_inline_end*/;
				o.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			PackedVaryingsMeshToPS Vert ( AttributesMesh v )
			{
				return VertexFunction( v );
			}
			#endif

			#if defined(WRITE_DECAL_BUFFER) && defined(WRITE_MSAA_DEPTH)
			#define SV_TARGET_NORMAL SV_Target3
			#elif defined(WRITE_DECAL_BUFFER) || defined(WRITE_MSAA_DEPTH)
			#define SV_TARGET_NORMAL SV_Target2
			#else
			#define SV_TARGET_NORMAL SV_Target1
			#endif

			void Frag( PackedVaryingsMeshToPS packedInput
				#ifdef WRITE_MSAA_DEPTH
					, out float4 depthColor : SV_Target0
					, out float4 outMotionVector : SV_Target1
						#ifdef WRITE_DECAL_BUFFER
						, out float4 outDecalBuffer : SV_Target2
						#endif
					#else
					, out float4 outMotionVector : SV_Target0
						#ifdef WRITE_DECAL_BUFFER
						, out float4 outDecalBuffer : SV_Target1
						#endif
					#endif

					#ifdef WRITE_NORMAL_BUFFER
					, out float4 outNormalBuffer : SV_TARGET_NORMAL
					#endif

					#ifdef _DEPTHOFFSET_ON
					, out float outputDepth : DEPTH_OFFSET_SEMANTIC
					#endif
				/*ase_frag_input*/
				)
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( packedInput );
				UNITY_SETUP_INSTANCE_ID( packedInput );
				FragInputs input;
				ZERO_INITIALIZE(FragInputs, input);
				input.tangentToWorld = k_identity3x3;
				input.positionSS = packedInput.vmeshPositionCS;
				input.positionRWS = packedInput.vmeshInterp00.xyz;

				PositionInputs posInput = GetPositionInput(input.positionSS.xy, _ScreenSize.zw, input.positionSS.z, input.positionSS.w, input.positionRWS);

				/*ase_local_var:wvd*/float3 V = GetWorldSpaceNormalizeViewDir(input.positionRWS);

				SurfaceData surfaceData;
				BuiltinData builtinData;

				SmoothSurfaceDescription surfaceDescription = (SmoothSurfaceDescription)0;
				/*ase_frag_code:packedInput=PackedVaryingsMeshToPS*/
				surfaceDescription.Normal = /*ase_frag_out:Normal;Float3;0;-1;_Normal*/float3( 0, 0, 1 )/*end*/;
				surfaceDescription.Smoothness = /*ase_frag_out:Smoothness;Float;1;-1;_Smoothness*/1/*end*/;
				surfaceDescription.Alpha = /*ase_frag_out:Alpha;Float;2;-1;_Alpha*/1/*end*/;

				#ifdef _ALPHATEST_ON
				surfaceDescription.AlphaClipThreshold = /*ase_frag_out:Alpha Clip Threshold;Float;3;-1;_AlphaClip*/_AlphaCutoff/*end*/;
				#endif

				#ifdef _DEPTHOFFSET_ON
				surfaceDescription.DepthOffset = /*ase_frag_out:Depth Offset;Float;6;-1;_DepthOffset*/0/*end*/;
				#endif

				GetSurfaceAndBuiltinData(surfaceDescription, input, V, posInput, surfaceData, builtinData);

				float4 VPASSpositionCS = float4(packedInput.vpassInterpolators0.xy, 0.0, packedInput.vpassInterpolators0.z);
				float4 VPASSpreviousPositionCS = float4(packedInput.vpassInterpolators1.xy, 0.0, packedInput.vpassInterpolators1.z);

				#ifdef _DEPTHOFFSET_ON
				VPASSpositionCS.w += builtinData.depthOffset;
				VPASSpreviousPositionCS.w += builtinData.depthOffset;
				#endif

				float2 motionVector = CalculateMotionVector( VPASSpositionCS, VPASSpreviousPositionCS );
				EncodeMotionVector( motionVector * 0.5, outMotionVector );

				bool forceNoMotion = unity_MotionVectorsParams.y == 0.0;
				if( forceNoMotion )
					outMotionVector = float4( 2.0, 0.0, 0.0, 0.0 );

				#ifdef WRITE_MSAA_DEPTH
					depthColor = packedInput.vmeshPositionCS.z;
					depthColor.a = SharpenAlpha(builtinData.opacity, builtinData.alphaClipTreshold);
				#endif

				#if defined(WRITE_NORMAL_BUFFER)
					EncodeIntoNormalBuffer(ConvertSurfaceDataToNormalData(surfaceData), outNormalBuffer);
				#endif

				#if defined(WRITE_DECAL_BUFFER)
					DecalPrepassData decalPrepassData;
					#ifdef _DISABLE_DECALS
					ZERO_INITIALIZE(DecalPrepassData, decalPrepassData);
					#else
					decalPrepassData.geomNormalWS = surfaceData.geomNormalWS;
					#endif
					decalPrepassData.renderingLayerMask = GetMeshRenderingLayerMask();
					EncodeIntoDecalPrepassBuffer(decalPrepassData, outDecalBuffer);
				#endif

				#ifdef _DEPTHOFFSET_ON
				outputDepth = posInput.deviceDepth;
				#endif
			}

			ENDHLSL
		}

		/*ase_pass*/
		Pass
		{
			/*ase_hide_pass*/
			Name "TransparentBackface"
			Tags 
			{ 
				"LightMode" = "TransparentBackface" 
		    }

			Blend [_SrcBlend] [_DstBlend], [_AlphaSrcBlend] [_AlphaDstBlend]
			Blend 1 One OneMinusSrcAlpha
			Blend 2 One OneMinusSrcAlpha
			Blend 3 One OneMinusSrcAlpha
			Blend 4 One OneMinusSrcAlpha

			Cull Front
			ZTest [_ZTestTransparent]
			ZWrite [_ZWrite]

            ColorMask [_ColorMaskTransparentVelOne] 1
            ColorMask [_ColorMaskTransparentVelTwo] 2

			HLSLPROGRAM
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #pragma shader_feature _ _SURFACE_TYPE_TRANSPARENT
            #pragma shader_feature_local _ _TRANSPARENT_WRITES_MOTION_VEC _TRANSPARENT_REFRACTIVE_SORT
            #pragma shader_feature_local_fragment _ _ENABLE_FOG_ON_TRANSPARENT

			#pragma multi_compile_fragment _ SHADOWS_SHADOWMASK
            #pragma multi_compile_fragment PUNCTUAL_SHADOW_LOW PUNCTUAL_SHADOW_MEDIUM PUNCTUAL_SHADOW_HIGH
            #pragma multi_compile_fragment DIRECTIONAL_SHADOW_LOW DIRECTIONAL_SHADOW_MEDIUM DIRECTIONAL_SHADOW_HIGH
            #pragma multi_compile_fragment AREA_SHADOW_MEDIUM AREA_SHADOW_HIGH
            #pragma multi_compile_fragment _ PROBE_VOLUMES_L1 PROBE_VOLUMES_L2
            #pragma multi_compile_fragment SCREEN_SPACE_SHADOWS_OFF SCREEN_SPACE_SHADOWS_ON

            #pragma multi_compile _ DEBUG_DISPLAY
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fragment DECALS_OFF DECALS_3RT DECALS_4RT
            #pragma multi_compile_fragment _ DECAL_SURFACE_GRADIENT
            #pragma multi_compile _ USE_LEGACY_LIGHTMAPS

			#ifndef SHADER_STAGE_FRAGMENT
			#define SHADOW_LOW
			#define USE_FPTL_LIGHTLIST
			#endif

			#pragma vertex Vert
			#pragma fragment Frag

			#define SHADERPASS SHADERPASS_FORWARD
		    #define HAS_LIGHTLOOP 1
			#define USE_CLUSTERED_LIGHTLIST

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/GeometricTools.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Tessellation.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
            #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"

			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"

            #ifdef RAYTRACING_SHADER_GRAPH_DEFAULT
                #define RAYTRACING_SHADER_GRAPH_HIGH
            #endif

            #ifdef RAYTRACING_SHADER_GRAPH_RAYTRACED
                #define RAYTRACING_SHADER_GRAPH_LOW
            #endif

            #ifndef SHADER_UNLIT
            #if defined(_DOUBLESIDED_ON) && !defined(VARYINGS_NEED_CULLFACE)
                #define VARYINGS_NEED_CULLFACE
            #endif
            #endif

			#if defined(_DOUBLESIDED_ON) && !defined(ASE_NEED_CULLFACE)
			    #define ASE_NEED_CULLFACE 1
			#endif

            #if _MATERIAL_FEATURE_COLORED_TRANSMISSION
            #undef _MATERIAL_FEATURE_CLEAR_COAT
            #endif

		    #if defined(_MATERIAL_FEATURE_SUBSURFACE_SCATTERING) && !defined(_SURFACE_TYPE_TRANSPARENT)
			#define OUTPUT_SPLIT_LIGHTING
		    #endif

		    #if (SHADERPASS == SHADERPASS_PATH_TRACING) && !defined(_DOUBLESIDED_ON) && (defined(_REFRACTION_PLANE) || defined(_REFRACTION_SPHERE))
			#undef  _REFRACTION_PLANE
			#undef  _REFRACTION_SPHERE
			#define _REFRACTION_THIN
		    #endif

            #if SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_PREPASS
            #if !defined(_DISABLE_SSR_TRANSPARENT) && !defined(SHADER_UNLIT)
                #define WRITE_NORMAL_BUFFER
            #endif
            #endif

            #if SHADERPASS == SHADERPASS_MOTION_VECTORS && defined(WRITE_DECAL_BUFFER_AND_RENDERING_LAYER)
                #define WRITE_DECAL_BUFFER
            #endif

            #ifndef DEBUG_DISPLAY
                #if !defined(_SURFACE_TYPE_TRANSPARENT)
                    #if SHADERPASS == SHADERPASS_FORWARD
                    #define SHADERPASS_FORWARD_BYPASS_ALPHA_TEST
                    #elif SHADERPASS == SHADERPASS_GBUFFER
                    #define SHADERPASS_GBUFFER_BYPASS_ALPHA_TEST
                    #endif
                #endif
            #endif

            #if defined(SHADER_LIT) && !defined(_SURFACE_TYPE_TRANSPARENT)
                #define _DEFERRED_CAPABLE_MATERIAL
            #endif
        
            #if (defined(_TRANSPARENT_WRITES_MOTION_VEC) || defined(_TRANSPARENT_REFRACTIVE_SORT)) && defined(_SURFACE_TYPE_TRANSPARENT)
                #define _WRITE_TRANSPARENT_MOTION_VECTOR
            #endif

			CBUFFER_START( UnityPerMaterial )
			float4 _EmissionColor;
			float _AlphaCutoff;
			float _RenderQueueType;
			#ifdef _ADD_PRECOMPUTED_VELOCITY
			    float _AddPrecomputedVelocity;
			#endif
			float _StencilRef;
			float _StencilWriteMask;
			float _StencilRefDepth;
			float _StencilWriteMaskDepth;
			float _StencilRefMV;
			float _StencilWriteMaskMV;
			float _StencilRefDistortionVec;
			float _StencilWriteMaskDistortionVec;
			float _StencilWriteMaskGBuffer;
			float _StencilRefGBuffer;
			float _ZTestGBuffer;
			float _RequireSplitLighting;
			float _ReceivesSSR;
			float _SurfaceType;
			float _BlendMode;
            #ifdef SUPPORT_BLENDMODE_PRESERVE_SPECULAR_LIGHTING
			    float _EnableBlendModePreserveSpecularLighting;
            #endif
			float _SrcBlend;
			float _DstBlend;
			float _DstBlend2;
			float _AlphaSrcBlend;
			float _AlphaDstBlend;
			float _ZWrite;
			float _TransparentZWrite;
			float _CullMode;
			float _TransparentSortPriority;
			float _EnableFogOnTransparent;
			float _CullModeForward;
			float _TransparentCullMode;
			float _ZTestDepthEqualForOpaque;
			float _ZTestTransparent;
			float _TransparentBackfaceEnable;
			float _AlphaCutoffEnable;
			float _UseShadowThreshold;
			float _DoubleSidedEnable;
			float _DoubleSidedNormalMode;
			float4 _DoubleSidedConstants;
			#ifdef ASE_TESSELLATION
			    float _TessPhongStrength;
			    float _TessValue;
			    float _TessMin;
			    float _TessMax;
			    float _TessEdgeLength;
			    float _TessMaxDisp;
			#endif
			CBUFFER_END

            #ifdef SCENEPICKINGPASS
			float4 _SelectionID;
            #endif

            #ifdef SCENESELECTIONPASS
			int _ObjectId;
			int _PassValue;
            #endif

			/*ase_globals*/

            #ifdef DEBUG_DISPLAY
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Debug/DebugDisplay.hlsl"
            #endif

            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/NormalSurfaceGradient.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/LightLoop/LightLoopDef.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/LightLoop/LightLoop.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/BuiltinUtilities.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialUtilities.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphFunctions.hlsl"

            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/DecalUtilities.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitDecalData.hlsl"

        	#ifdef HAVE_VFX_MODIFICATION
        	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/VisualEffectVertex.hlsl"
        	#endif

			/*ase_pragma*/

			struct AttributesMesh
			{
				float3 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 tangentOS : TANGENT;
				float4 uv1 : TEXCOORD1;
				float4 uv2 : TEXCOORD2;
				float3 previousPositionOS : TEXCOORD4;
				float3 precomputedVelocity : TEXCOORD5;
				/*ase_vdata:p=p;n=n;t=t;uv1=tc1;uv2=tc2;uv4=tc4;uv5=tc5*/
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryingsMeshToPS
			{
				SV_POSITION_QUALIFIERS float4 positionCS : SV_Position;
				float4 clipPosV : TEXCOORD0;
				float3 positionRWS : TEXCOORD1;
				float3 normalWS : TEXCOORD2;
				float4 tangentWS : TEXCOORD3;
				float4 uv1 : TEXCOORD4;
				float4 uv2 : TEXCOORD5;
				#ifdef _WRITE_TRANSPARENT_MOTION_VECTOR
					float3 vpassPositionCS : TEXCOORD6;
					float3 vpassPreviousPositionCS : TEXCOORD7;
				#endif
				/*ase_interp(8,):sp=sp.xyzw;rwp=tc1;wn=tc2;wt=tc3;uv1=tc4;uv2=tc5*/
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
				#if defined(SHADER_STAGE_FRAGMENT) && defined(ASE_NEED_CULLFACE)
				FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
				#endif
			};

			/*ase_funcs*/

			void BuildSurfaceData(FragInputs fragInputs, inout GlobalSurfaceDescription surfaceDescription, float3 V, PositionInputs posInput, out SurfaceData surfaceData, out float3 bentNormalWS)
			{
				ZERO_INITIALIZE(SurfaceData, surfaceData);

				surfaceData.specularOcclusion = 1.0;
				surfaceData.thickness = 0.0;

				surfaceData.baseColor =					surfaceDescription.BaseColor;
				surfaceData.perceptualSmoothness =		surfaceDescription.Smoothness;
				surfaceData.ambientOcclusion =			surfaceDescription.Occlusion;
				surfaceData.metallic =					surfaceDescription.Metallic;
				surfaceData.coatMask =					surfaceDescription.CoatMask;

				#ifdef _SPECULAR_OCCLUSION_CUSTOM
				surfaceData.specularOcclusion =			surfaceDescription.SpecularOcclusion;
				#endif

				#ifdef _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
				surfaceData.subsurfaceMask =			surfaceDescription.SubsurfaceMask;
				#endif

				#if defined(_HAS_REFRACTION) || defined(_MATERIAL_FEATURE_TRANSMISSION)
				surfaceData.thickness = 				surfaceDescription.Thickness;
				#endif

				#ifdef _MATERIAL_FEATURE_TRANSMISSION
				surfaceData.transmissionMask =			surfaceDescription.TransmissionMask;
				#endif

				#if defined( _MATERIAL_FEATURE_SUBSURFACE_SCATTERING ) || defined( _MATERIAL_FEATURE_TRANSMISSION )
				surfaceData.diffusionProfileHash =		asuint(surfaceDescription.DiffusionProfile);
				#endif

				#ifdef _MATERIAL_FEATURE_SPECULAR_COLOR
				surfaceData.specularColor =				surfaceDescription.Specular;
				#endif

				#ifdef _MATERIAL_FEATURE_ANISOTROPY
				surfaceData.anisotropy =				surfaceDescription.Anisotropy;
				#endif

				#ifdef _MATERIAL_FEATURE_IRIDESCENCE
				surfaceData.iridescenceMask =			surfaceDescription.IridescenceMask;
				surfaceData.iridescenceThickness =		surfaceDescription.IridescenceThickness;
				#endif

				// refraction
                #if defined(_REFRACTION_PLANE) || defined(_REFRACTION_SPHERE) || defined(_REFRACTION_THIN)
                    if (_EnableSSRefraction)
                    {
                        surfaceData.ior =                       surfaceDescription.RefractionIndex;
                        surfaceData.transmittanceColor =        surfaceDescription.RefractionColor;
                        surfaceData.atDistance =                surfaceDescription.RefractionDistance;
        
                        surfaceData.transmittanceMask = (1.0 - surfaceDescription.Alpha);
                        surfaceDescription.Alpha = 1.0;
                    }
                    else
                    {
                        surfaceData.ior = 1.0;
                        surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                        surfaceData.atDistance = 1.0;
                        surfaceData.transmittanceMask = 0.0;
                        surfaceDescription.Alpha = 1.0;
                    }
                #else
                    surfaceData.ior = 1.0;
                    surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                    surfaceData.atDistance = 1.0;
                    surfaceData.transmittanceMask = 0.0;
                #endif

				surfaceData.materialFeatures = MATERIALFEATUREFLAGS_LIT_STANDARD;

				#ifdef _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SUBSURFACE_SCATTERING;
				#endif

				#ifdef _MATERIAL_FEATURE_TRANSMISSION
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_TRANSMISSION;
				#endif

				#ifdef _MATERIAL_FEATURE_COLORED_TRANSMISSION
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_TRANSMISSION;
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_COLORED_TRANSMISSION;
				#endif

                #ifdef _MATERIAL_FEATURE_ANISOTROPY
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_ANISOTROPY;
                    surfaceData.normalWS = float3(0, 1, 0);
                #endif

                #ifdef _MATERIAL_FEATURE_CLEAR_COAT
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_CLEAR_COAT;
                #endif

                #ifdef _MATERIAL_FEATURE_IRIDESCENCE
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_IRIDESCENCE;
                #endif

                #ifdef _MATERIAL_FEATURE_SPECULAR_COLOR
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SPECULAR_COLOR;
                #endif

                #if defined (_MATERIAL_FEATURE_SPECULAR_COLOR) && defined (_ENERGY_CONSERVING_SPECULAR)
                    surfaceData.baseColor *= ( 1.0 - Max3( surfaceData.specularColor.r, surfaceData.specularColor.g, surfaceData.specularColor.b ) );
                #endif

				#ifdef _DOUBLESIDED_ON
                    float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
				#else
                    float3 doubleSidedConstants = float3( 1.0, 1.0, 1.0 );
				#endif

				float3 normalTS = float3(0.0f, 0.0f, 1.0f);
				normalTS = surfaceDescription.Normal;

                #ifdef DECAL_NORMAL_BLENDING
					normalTS = SurfaceGradientFromTangentSpaceNormalAndFromTBN(normalTS, fragInputs.tangentToWorld[0], fragInputs.tangentToWorld[1]);

                    #if HAVE_DECALS
                    if (_EnableDecals)
                    {
                        float alpha = 1.0;
                        alpha = surfaceDescription.Alpha;

                        DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, fragInputs, alpha);
                        ApplyDecalToSurfaceNormal(decalSurfaceData, fragInputs.tangentToWorld[2], normalTS);
                        ApplyDecalToSurfaceDataNoNormal(decalSurfaceData, surfaceData);
                     }
                    #endif

                    GetNormalWS_SG(fragInputs, normalTS, surfaceData.normalWS, doubleSidedConstants);
                #else
					GetNormalWS(fragInputs, normalTS, surfaceData.normalWS, doubleSidedConstants);

                    #if HAVE_DECALS
                    if (_EnableDecals)
                    {
                        float alpha = 1.0;
                        alpha = surfaceDescription.Alpha;

                        DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, fragInputs, alpha);
                        ApplyDecalToSurfaceNormal(decalSurfaceData, surfaceData.normalWS.xyz);
                        ApplyDecalToSurfaceDataNoNormal(decalSurfaceData, surfaceData);
                    }
                    #endif
                #endif

				surfaceData.geomNormalWS = fragInputs.tangentToWorld[2];
                surfaceData.tangentWS = normalize(fragInputs.tangentToWorld[0].xyz );
                surfaceData.tangentWS = Orthonormalize(surfaceData.tangentWS, surfaceData.normalWS);

				bentNormalWS = surfaceData.normalWS;

				#ifdef ASE_BENT_NORMAL
                    GetNormalWS( fragInputs, surfaceDescription.BentNormal, bentNormalWS, doubleSidedConstants );
				#endif

				#ifdef _MATERIAL_FEATURE_ANISOTROPY
                    surfaceData.tangentWS = TransformTangentToWorld(surfaceDescription.Tangent, fragInputs.tangentToWorld);
				#endif

				#if defined(DEBUG_DISPLAY)
				    #if !defined(SHADER_STAGE_RAY_TRACING)
				    if (_DebugMipMapMode != DEBUGMIPMAPMODE_NONE)
				    {
					   #ifdef FRAG_INPUTS_USE_TEXCOORD0
                           surfaceData.baseColor = GET_TEXTURE_STREAMING_DEBUG(posInput.positionSS, fragInputs.texCoord0);
                       #else
                           surfaceData.baseColor = GET_TEXTURE_STREAMING_DEBUG_NO_UV(posInput.positionSS);
                       #endif
					   surfaceData.metallic = 0;
				    }
					#endif
				    ApplyDebugToSurfaceData(fragInputs.tangentToWorld, surfaceData); 
				#endif

                #if defined(_SPECULAR_OCCLUSION_CUSTOM)
                #elif defined(_SPECULAR_OCCLUSION_FROM_AO_BENT_NORMAL)
                    surfaceData.specularOcclusion = GetSpecularOcclusionFromBentAO(V, bentNormalWS, surfaceData.normalWS, surfaceData.ambientOcclusion, PerceptualSmoothnessToPerceptualRoughness(surfaceData.perceptualSmoothness));
                #elif defined(_AMBIENT_OCCLUSION) && defined(_SPECULAR_OCCLUSION_FROM_AO)
                    surfaceData.specularOcclusion = GetSpecularOcclusionFromAmbientOcclusion(ClampNdotV(dot(surfaceData.normalWS, V)), surfaceData.ambientOcclusion, PerceptualSmoothnessToRoughness(surfaceData.perceptualSmoothness));
                #endif

                #ifdef _ENABLE_GEOMETRIC_SPECULAR_AA
                    surfaceData.perceptualSmoothness = GeometricNormalFiltering(surfaceData.perceptualSmoothness, fragInputs.tangentToWorld[2], surfaceDescription.SpecularAAScreenSpaceVariance, surfaceDescription.SpecularAAThreshold);
                #endif
			}

			// Get Surface And BuiltinData
			void GetSurfaceAndBuiltinData(GlobalSurfaceDescription surfaceDescription, FragInputs fragInputs, float3 V, inout PositionInputs posInput, out SurfaceData surfaceData, out BuiltinData builtinData)
			{
				#ifdef LOD_FADE_CROSSFADE
                    LODDitheringTransition(ComputeFadeMaskSeed(V, posInput.positionSS), unity_LODFade.x);
				#endif

                #ifdef _DOUBLESIDED_ON
                    float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
                #else
                    float3 doubleSidedConstants = float3(1.0, 1.0, 1.0);
                #endif
                ApplyDoubleSidedFlipOrMirror(fragInputs, doubleSidedConstants);

				#ifdef _ALPHATEST_ON
                    DoAlphaTest( surfaceDescription.Alpha, surfaceDescription.AlphaClipThreshold );
				#endif

				#ifdef _ALPHATEST_SHADOW_ON
                    DoAlphaTest( surfaceDescription.Alpha, surfaceDescription.AlphaClipThresholdShadow);
				#endif

				#ifdef _DEPTHOFFSET_ON
                    ApplyDepthOffsetPositionInput(V, surfaceDescription.DepthOffset, GetViewForwardDir(), GetWorldToHClipMatrix(), posInput);
				#endif

                float3 bentNormalWS;
                BuildSurfaceData(fragInputs, surfaceDescription, V, posInput, surfaceData, bentNormalWS);
                InitBuiltinData(posInput, surfaceDescription.Alpha, bentNormalWS, -fragInputs.tangentToWorld[2], fragInputs.texCoord1, fragInputs.texCoord2, builtinData);

				#ifdef _DEPTHOFFSET_ON
                    builtinData.depthOffset = surfaceDescription.DepthOffset;
				#endif

                #ifdef _ALPHATEST_ON
                    builtinData.alphaClipTreshold = surfaceDescription.AlphaClipThreshold;
                #endif

                #ifdef UNITY_VIRTUAL_TEXTURING
                    builtinData.vtPackedFeedback = surfaceDescription.VTPackedFeedback;
                #endif

				#ifdef ASE_BAKEDGI
                    builtinData.bakeDiffuseLighting = surfaceDescription.BakedGI;
				#endif

				#ifdef ASE_BAKEDBACKGI
                    builtinData.backBakeDiffuseLighting = surfaceDescription.BakedBackGI;
				#endif

                builtinData.emissiveColor = surfaceDescription.Emission;

                PostInitBuiltinData(V, posInput, surfaceData, builtinData);
			}

			AttributesMesh ApplyMeshModification(AttributesMesh inputMesh, float3 timeParameters, inout PackedVaryingsMeshToPS output/*ase_vert_input*/ )
			{
				_TimeParameters.xyz = timeParameters;
				/*ase_vert_code:inputMesh=AttributesMesh;output=PackedVaryingsMeshToPS*/

				#ifdef ASE_ABSOLUTE_VERTEX_POS
				float3 defaultVertexValue = inputMesh.positionOS.xyz;
				#else
				float3 defaultVertexValue = float3( 0, 0, 0 );
				#endif
				float3 vertexValue = /*ase_vert_out:Vertex Offset;Float3;11;-1;_VertexOffset*/defaultVertexValue/*end*/;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
				inputMesh.positionOS.xyz = vertexValue;
				#else
				inputMesh.positionOS.xyz += vertexValue;
				#endif
				inputMesh.normalOS = /*ase_vert_out:Vertex Normal;Float3;12;-1;_VertexNormal*/inputMesh.normalOS/*end*/;
				inputMesh.tangentOS = /*ase_vert_out:Vertex Tangent;Float4;27;-1;_VertexTangent*/inputMesh.tangentOS/*end*/;
				return inputMesh;
			}

			PackedVaryingsMeshToPS VertexFunction(AttributesMesh inputMesh)
			{
				PackedVaryingsMeshToPS output = (PackedVaryingsMeshToPS)0;
				AttributesMesh defaultMesh = inputMesh;

				UNITY_SETUP_INSTANCE_ID(inputMesh);
				UNITY_TRANSFER_INSTANCE_ID(inputMesh, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( output );

				inputMesh = ApplyMeshModification( inputMesh, _TimeParameters.xyz, output);

				float3 positionRWS = TransformObjectToWorld(inputMesh.positionOS);
				float3 normalWS = TransformObjectToWorldNormal(inputMesh.normalOS);
				float4 tangentWS = float4(TransformObjectToWorldDir(inputMesh.tangentOS.xyz), inputMesh.tangentOS.w);

				#ifdef _WRITE_TRANSPARENT_MOTION_VECTOR
				float4 VPASSpreviousPositionCS;
				float4 VPASSpositionCS = mul(UNITY_MATRIX_UNJITTERED_VP, float4(positionRWS, 1.0));

				bool forceNoMotion = unity_MotionVectorsParams.y == 0.0;
				if (forceNoMotion)
				{
					VPASSpreviousPositionCS = float4(0.0, 0.0, 0.0, 1.0);
				}
				else
				{
					bool hasDeformation = unity_MotionVectorsParams.x > 0.0;
					float3 effectivePositionOS = (hasDeformation ? inputMesh.previousPositionOS : defaultMesh.positionOS);
					#if defined(_ADD_PRECOMPUTED_VELOCITY)
					effectivePositionOS -= inputMesh.precomputedVelocity;
					#endif

					#if defined(HAVE_MESH_MODIFICATION)
						AttributesMesh previousMesh = defaultMesh;
						previousMesh.positionOS = effectivePositionOS ;
						PackedVaryingsMeshToPS test = (PackedVaryingsMeshToPS)0;
						float3 curTime = _TimeParameters.xyz;
						previousMesh = ApplyMeshModification(previousMesh, _LastTimeParameters.xyz, test);
						_TimeParameters.xyz = curTime;
						float3 previousPositionRWS = TransformPreviousObjectToWorld(previousMesh.positionOS);
					#else
						float3 previousPositionRWS = TransformPreviousObjectToWorld(effectivePositionOS);
					#endif

					#ifdef ATTRIBUTES_NEED_NORMAL
						float3 normalWS = TransformPreviousObjectToWorldNormal(defaultMesh.normalOS);
					#else
						float3 normalWS = float3(0.0, 0.0, 0.0);
					#endif

					#if defined(HAVE_VERTEX_MODIFICATION)
						ApplyVertexModification(inputMesh, normalWS, previousPositionRWS, _LastTimeParameters.xyz);
					#endif

					VPASSpreviousPositionCS = mul(UNITY_MATRIX_PREV_VP, float4(previousPositionRWS, 1.0));
				}
				#endif

				output.positionCS = TransformWorldToHClip(positionRWS);
				output.clipPosV = output.positionCS;
				output.positionRWS = positionRWS;
				output.normalWS = normalWS;
				output.tangentWS = tangentWS;
				output.uv1 = inputMesh.uv1;
				output.uv2 = inputMesh.uv2;

				#ifdef _WRITE_TRANSPARENT_MOTION_VECTOR
					output.vpassPositionCS = float3(VPASSpositionCS.xyw);
					output.vpassPreviousPositionCS = float3(VPASSpreviousPositionCS.xyw);
				#endif
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float3 positionOS : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				float4 tangentOS : TANGENT;
				float4 uv1 : TEXCOORD1;
				float4 uv2 : TEXCOORD2;
				/*ase_vcontrol*/
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl Vert ( AttributesMesh v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.positionOS = v.positionOS;
				o.normalOS = v.normalOS;
				o.tangentOS = v.tangentOS;
				o.uv1 = v.uv1;
				o.uv2 = v.uv2;
				/*ase_control_code:v=AttributesMesh;o=VertexControl*/
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = /*ase_inline_begin*/_TessValue/*ase_inline_end*/; float tessMin = /*ase_inline_begin*/_TessMin/*ase_inline_end*/; float tessMax = /*ase_inline_begin*/_TessMax/*ase_inline_end*/;
				float edgeLength = /*ase_inline_begin*/_TessEdgeLength/*ase_inline_end*/; float tessMaxDisp = /*ase_inline_begin*/_TessMaxDisp/*ase_inline_end*/;
				#if (SHADEROPTIONS_CAMERA_RELATIVE_RENDERING != 0)
				float3 cameraPos = 0;
				#else
				float3 cameraPos = _WorldSpaceCameraPos;
				#endif
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(float4(v[0].positionOS,1), float4(v[1].positionOS,1), float4(v[2].positionOS,1), tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), cameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(float4(v[0].positionOS,1), float4(v[1].positionOS,1), float4(v[2].positionOS,1), edgeLength, GetObjectToWorldMatrix(), cameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(float4(v[0].positionOS,1), float4(v[1].positionOS,1), float4(v[2].positionOS,1), edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), cameraPos, _ScreenParams, _FrustumPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			PackedVaryingsMeshToPS DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				AttributesMesh o = (AttributesMesh) 0;
				o.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				o.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				o.tangentOS = patch[0].tangentOS * bary.x + patch[1].tangentOS * bary.y + patch[2].tangentOS * bary.z;
				o.uv1 = patch[0].uv1 * bary.x + patch[1].uv1 * bary.y + patch[2].uv1 * bary.z;
				o.uv2 = patch[0].uv2 * bary.x + patch[1].uv2 * bary.y + patch[2].uv2 * bary.z;
				/*ase_domain_code:patch=VertexControl;o=AttributesMesh;bary=SV_DomainLocation*/
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.positionOS.xyz - patch[i].normalOS * (dot(o.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = /*ase_inline_begin*/_TessPhongStrength/*ase_inline_end*/;
				o.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			PackedVaryingsMeshToPS Vert ( AttributesMesh v )
			{
				return VertexFunction( v );
			}
			#endif

			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Debug/DebugDisplayMaterial.hlsl"

            #if defined(_TRANSPARENT_REFRACTIVE_SORT) || defined(_ENABLE_FOG_ON_TRANSPARENT)
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Water/Shaders/UnderWaterUtilities.hlsl"
            #endif

            #ifdef UNITY_VIRTUAL_TEXTURING
                #ifdef OUTPUT_SPLIT_LIGHTING
                   #define DIFFUSE_LIGHTING_TARGET SV_Target2
                   #define SSS_BUFFER_TARGET SV_Target3
                #elif defined(_WRITE_TRANSPARENT_MOTION_VECTOR)
                   #define MOTION_VECTOR_TARGET SV_Target2
                    #ifdef _TRANSPARENT_REFRACTIVE_SORT
                        #define BEFORE_REFRACTION_TARGET SV_Target3
                        #define BEFORE_REFRACTION_ALPHA_TARGET SV_Target4
            	#endif
            #endif
            #if defined(SHADER_API_PSSL)
            	#pragma PSSL_target_output_format(target 1 FMT_32_ABGR)
            #endif
            #else
                #ifdef OUTPUT_SPLIT_LIGHTING
                #define DIFFUSE_LIGHTING_TARGET SV_Target1
                #define SSS_BUFFER_TARGET SV_Target2
                #elif defined(_WRITE_TRANSPARENT_MOTION_VECTOR)
                #define MOTION_VECTOR_TARGET SV_Target1
                #ifdef _TRANSPARENT_REFRACTIVE_SORT
                     #define BEFORE_REFRACTION_TARGET SV_Target2
                     #define BEFORE_REFRACTION_ALPHA_TARGET SV_Target3
                #endif
            #endif
            #endif

			void Frag(PackedVaryingsMeshToPS packedInput
				, out float4 outColor:SV_Target0
            #ifdef UNITY_VIRTUAL_TEXTURING
				, out float4 outVTFeedback : SV_Target1
            #endif
            #ifdef OUTPUT_SPLIT_LIGHTING
				, out float4 outDiffuseLighting : DIFFUSE_LIGHTING_TARGET
				, OUTPUT_SSSBUFFER(outSSSBuffer) : SSS_BUFFER_TARGET
            #elif defined(_WRITE_TRANSPARENT_MOTION_VECTOR)
				, out float4 outMotionVec : MOTION_VECTOR_TARGET
                #ifdef _TRANSPARENT_REFRACTIVE_SORT
                , out float4 outBeforeRefractionColor : BEFORE_REFRACTION_TARGET
                , out float4 outBeforeRefractionAlpha : BEFORE_REFRACTION_ALPHA_TARGET
                #endif
            #endif
            #ifdef _DEPTHOFFSET_ON
				, out float outputDepth : DEPTH_OFFSET_SEMANTIC
            #endif
		    /*ase_frag_input*/
				)
			{
                #ifdef _WRITE_TRANSPARENT_MOTION_VECTOR
				   outMotionVec = float4(2.0, 0.0, 0.0, 1.0);
                #endif

				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( packedInput );
				UNITY_SETUP_INSTANCE_ID( packedInput );
		
				/*ase_local_var:rwp*/float3 PositionRWS = packedInput.positionRWS;
				/*ase_local_var:wvd*/float3 V = GetWorldSpaceNormalizeViewDir( packedInput.positionRWS );
				/*ase_local_var:wn*/float3 NormalWS = packedInput.normalWS;
				/*ase_local_var:wt*/float4 TangentWS = packedInput.tangentWS;
				/*ase_local_var:sp*/float4 ClipPos = packedInput.clipPosV;
				/*ase_local_var:spu*/float4 ScreenPos = ComputeScreenPos( packedInput.clipPosV, _ProjectionParams.x );

				FragInputs input;
				ZERO_INITIALIZE(FragInputs, input);
				input.positionSS = packedInput.positionCS;
				input.positionRWS = PositionRWS;
				input.tangentToWorld = BuildTangentToWorld(TangentWS, NormalWS);
				input.texCoord1 = packedInput.uv1.xyzw;
				input.texCoord2 = packedInput.uv2.xyzw;

				#if _DOUBLESIDED_ON && SHADER_STAGE_FRAGMENT
				input.isFrontFace = IS_FRONT_VFACE( packedInput.cullFace, true, false);
				#elif SHADER_STAGE_FRAGMENT
				#if defined(ASE_NEED_CULLFACE)
				input.isFrontFace = IS_FRONT_VFACE(packedInput.cullFace, true, false);
				#endif
				#endif
				/*ase_local_var:vf*/half IsFrontFace = input.isFrontFace;

				AdjustFragInputsToOffScreenRendering(input, _OffScreenRendering > 0, _OffScreenDownsampleFactor);
				uint2 tileIndex = uint2(input.positionSS.xy) / GetTileSize ();

				PositionInputs posInput = GetPositionInput( input.positionSS.xy, _ScreenSize.zw, input.positionSS.z, input.positionSS.w, input.positionRWS.xyz, tileIndex );

				GlobalSurfaceDescription surfaceDescription = (GlobalSurfaceDescription)0;
				/*ase_frag_code:packedInput=PackedVaryingsMeshToPS*/
				surfaceDescription.BaseColor = /*ase_frag_out:Base Color;Float3;0;-1;_BaseColor*/float3( 0.5, 0.5, 0.5 )/*end*/;
				surfaceDescription.Normal = /*ase_frag_out:Normal;Float3;1;-1;_Normal*/float3( 0, 0, 1 )/*end*/;
				surfaceDescription.BentNormal = /*ase_frag_out:Bent Normal;Float3;2;-1;_BentNormal*/float3( 0, 0, 1 )/*end*/;
				surfaceDescription.CoatMask = /*ase_frag_out:Coat Mask;Float;3;-1;_CoatMask*/0/*end*/;
				surfaceDescription.Metallic = /*ase_frag_out:Metallic;Float;4;-1;_Metallic*/0/*end*/;

				#ifdef _MATERIAL_FEATURE_SPECULAR_COLOR
				surfaceDescription.Specular = /*ase_frag_out:Specular;Float3;5;-1;_Specular*/0/*end*/;
				#endif

				surfaceDescription.Emission = /*ase_frag_out:Emission;Float3;6;-1;_Emission*/0/*end*/;
				surfaceDescription.Smoothness = /*ase_frag_out:Smoothness;Float;7;-1;_Smoothness*/0.5/*end*/;
				surfaceDescription.Occlusion = /*ase_frag_out:Occlusion;Float;8;-1;_Occlusion*/1/*end*/;
				surfaceDescription.Alpha = /*ase_frag_out:Alpha;Float;9;-1;_Alpha*/1/*end*/;

				#ifdef _ALPHATEST_ON
				surfaceDescription.AlphaClipThreshold = /*ase_frag_out:Alpha Clip Threshold;Float;10;-1;_AlphaClip*/_AlphaCutoff/*end*/;
				#endif

				#ifdef _ENABLE_GEOMETRIC_SPECULAR_AA
				surfaceDescription.SpecularAAScreenSpaceVariance = /*ase_frag_out:Specular AA Screen Space Variance;Float;13;-1;_SpecularAAScreenSpaceVariance*/0/*end*/;
				surfaceDescription.SpecularAAThreshold = /*ase_frag_out:Specular AA Threshold;Float;14;-1;_SpecularAAThreshold*/0/*end*/;
				#endif

				#ifdef _SPECULAR_OCCLUSION_CUSTOM
				surfaceDescription.SpecularOcclusion = /*ase_frag_out:Specular Occlusion;Float;15;-1;_SpecularOcclusion*/0/*end*/;
				#endif

				#if defined(_HAS_REFRACTION) || defined(_MATERIAL_FEATURE_TRANSMISSION)
				surfaceDescription.Thickness = /*ase_frag_out:Thickness;Float;16;-1;_Thickness*/1/*end*/;
				#endif

				#ifdef _HAS_REFRACTION
				surfaceDescription.RefractionIndex = /*ase_frag_out:Refraction Index;Float;17;-1;_RefractionIndex*/1/*end*/;
				surfaceDescription.RefractionColor = /*ase_frag_out:Refraction Color;Float3;18;-1;_RefractionColor*/float3( 1, 1, 1 )/*end*/;
				surfaceDescription.RefractionDistance = /*ase_frag_out:Refraction Distance;Float;19;-1;_RefractionDistance*/0/*end*/;
				#endif

				#ifdef _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
				surfaceDescription.SubsurfaceMask = /*ase_frag_out:Subsurface Mask;Float;20;-1;_SubsurfaceMask*/1/*end*/;
				#endif

				#ifdef _MATERIAL_FEATURE_TRANSMISSION
				surfaceDescription.TransmissionMask = /*ase_frag_out:Transmission Mask;Float;61;-1;_TransmissionMask*/1/*end*/;
				#endif

				#if defined( _MATERIAL_FEATURE_SUBSURFACE_SCATTERING ) || defined( _MATERIAL_FEATURE_TRANSMISSION )
				surfaceDescription.DiffusionProfile = /*ase_frag_out:Diffusion Profile;Float;62;-1;_DiffusionProfile*/0/*end*/;
				#endif

				#ifdef _MATERIAL_FEATURE_ANISOTROPY
				surfaceDescription.Anisotropy = /*ase_frag_out:Anisotropy;Float;22;-1;_Anisotropy*/1/*end*/;
				surfaceDescription.Tangent = /*ase_frag_out:Tangent;Float3;23;-1;_Tangent*/float3( 1, 0, 0 )/*end*/;
				#endif

				#ifdef _MATERIAL_FEATURE_IRIDESCENCE
				surfaceDescription.IridescenceMask = /*ase_frag_out:Iridescence Mask;Float;24;-1;_IridescenceMask*/0/*end*/;
				surfaceDescription.IridescenceThickness = /*ase_frag_out:Iridescence Thickness;Float;25;-1;_IridescenceThickness*/0/*end*/;
				#endif

				#ifdef ASE_BAKEDGI
				surfaceDescription.BakedGI = /*ase_frag_out:Baked GI;Float3;31;-1;_BakedGI*/0/*end*/;
				#endif
				#ifdef ASE_BAKEDBACKGI
				surfaceDescription.BakedBackGI = /*ase_frag_out:Baked Back GI;Float3;32;-1;_BakedBackGI*/0/*end*/;
				#endif

				#ifdef _DEPTHOFFSET_ON
				surfaceDescription.DepthOffset = /*ase_frag_out:Depth Offset;Float;26;-1;_DepthOffset*/0/*end*/;
				#endif

				#ifdef UNITY_VIRTUAL_TEXTURING
				surfaceDescription.VTPackedFeedback = float4(1.0f,1.0f,1.0f,1.0f);
				#endif

				SurfaceData surfaceData;
				BuiltinData builtinData;
				GetSurfaceAndBuiltinData(surfaceDescription,input, V, posInput, surfaceData, builtinData);

				BSDFData bsdfData = ConvertSurfaceDataToBSDFData(input.positionSS.xy, surfaceData);

				PreLightData preLightData = GetPreLightData(V, posInput, bsdfData);

				outColor = float4(0.0, 0.0, 0.0, 0.0);

				#ifdef DEBUG_DISPLAY
				       #ifdef OUTPUT_SPLIT_LIGHTING
                       outDiffuseLighting = float4(0, 0, 0, 1);
                ENCODE_INTO_SSSBUFFER(surfaceData, posInput.positionSS, outSSSBuffer);
				#endif

			    bool viewMaterial = GetMaterialDebugColor(outColor, input, builtinData, posInput, surfaceData, bsdfData);

				if (!viewMaterial)
				{
					if (_DebugFullScreenMode == FULLSCREENDEBUGMODE_VALIDATE_DIFFUSE_COLOR || _DebugFullScreenMode == FULLSCREENDEBUGMODE_VALIDATE_SPECULAR_COLOR)
					{
						float3 result = float3(0.0, 0.0, 0.0);
						GetPBRValidatorDebug(surfaceData, result);
						outColor = float4(result, 1.0f);
					}
					else if (_DebugFullScreenMode == FULLSCREENDEBUGMODE_TRANSPARENCY_OVERDRAW)
					{
						float4 result = _DebugTransparencyOverdrawWeight * float4(TRANSPARENCY_OVERDRAW_COST, TRANSPARENCY_OVERDRAW_COST, TRANSPARENCY_OVERDRAW_COST, TRANSPARENCY_OVERDRAW_A);
						outColor = result;
					}
					else
                #endif
					{
                #ifdef _SURFACE_TYPE_TRANSPARENT
						uint featureFlags = LIGHT_FEATURE_MASK_FLAGS_TRANSPARENT;
                #else
						uint featureFlags = LIGHT_FEATURE_MASK_FLAGS_OPAQUE;
                #endif
						LightLoopOutput lightLoopOutput;
						LightLoop(V, posInput, preLightData, bsdfData, builtinData, featureFlags, lightLoopOutput);

						// Alias
						float3 diffuseLighting = lightLoopOutput.diffuseLighting;
						float3 specularLighting = lightLoopOutput.specularLighting;

						diffuseLighting *= GetCurrentExposureMultiplier();
						specularLighting *= GetCurrentExposureMultiplier();

                #ifdef OUTPUT_SPLIT_LIGHTING
						if (_EnableSubsurfaceScattering != 0 && ShouldOutputSplitLighting(bsdfData))
						{
							outColor = float4(specularLighting, 1.0);
							outDiffuseLighting = float4(TagLightingForSSS(diffuseLighting), 1.0);
						}
						else
						{
							outColor = float4(diffuseLighting + specularLighting, 1.0);
							outDiffuseLighting = float4(0, 0, 0, 1);
						}
						ENCODE_INTO_SSSBUFFER(surfaceData, posInput.positionSS, outSSSBuffer);
                #else
						outColor = ApplyBlendMode(diffuseLighting, specularLighting, builtinData.opacity);

						#ifdef _ENABLE_FOG_ON_TRANSPARENT
						outColor = EvaluateAtmosphericScattering(posInput, V, outColor);
                        #endif

                        #ifdef _TRANSPARENT_REFRACTIVE_SORT
                        ComputeRefractionSplitColor(posInput, outColor, outBeforeRefractionColor, outBeforeRefractionAlpha);
                        #endif
                #endif

				#ifdef _WRITE_TRANSPARENT_MOTION_VECTOR
						float4 VPASSpositionCS = float4(packedInput.vpassPositionCS.xy, 0.0, packedInput.vpassPositionCS.z);
						float4 VPASSpreviousPositionCS = float4(packedInput.vpassPreviousPositionCS.xy, 0.0, packedInput.vpassPreviousPositionCS.z);
						bool forceNoMotion = any(unity_MotionVectorsParams.yw == 0.0);
                #if defined(HAVE_VFX_MODIFICATION) && !VFX_FEATURE_MOTION_VECTORS
                        forceNoMotion = true;
                #endif
				        if (!forceNoMotion)
						{
							float2 motionVec = CalculateMotionVector(VPASSpositionCS, VPASSpreviousPositionCS);
							EncodeMotionVector(motionVec * 0.5, outMotionVec);
							outMotionVec.zw = 1.0;
						}
				#endif
				}

				#ifdef DEBUG_DISPLAY
				}
				#endif

				#ifdef _DEPTHOFFSET_ON
				outputDepth = posInput.deviceDepth;
				#endif

                #ifdef UNITY_VIRTUAL_TEXTURING
				    float vtAlphaValue = builtinData.opacity;
                    #if defined(HAS_REFRACTION) && HAS_REFRACTION
					vtAlphaValue = 1.0f - bsdfData.transmittanceMask;
                #endif
				outVTFeedback = PackVTFeedbackWithAlpha(builtinData.vtPackedFeedback, input.positionSS.xy, vtAlphaValue);
				outVTFeedback.rgb *= outVTFeedback.a; // premuliplied alpha
                #endif

			}
			ENDHLSL
		}

		/*ase_pass*/
		Pass
		{
			/*ase_hide_pass*/
			Name "TransparentDepthPrepass"
			Tags 
			{ 
				"LightMode" = "TransparentDepthPrepass" 
		    }

			Cull [_CullMode]
            Blend One Zero
            ZWrite On

            Stencil
            {
                WriteMask [_StencilWriteMaskDepth]
                Ref [_StencilRefDepth]
                CompFront Always
                PassFront Replace
                CompBack Always
                PassBack Replace
            }

			HLSLPROGRAM
			#pragma multi_compile _ DOTS_INSTANCING_ON

            #pragma shader_feature _ _SURFACE_TYPE_TRANSPARENT
            #pragma shader_feature_local _ _TRANSPARENT_WRITES_MOTION_VEC _TRANSPARENT_REFRACTIVE_SORT
            #pragma shader_feature_local_fragment _ _ENABLE_FOG_ON_TRANSPARENT

			#pragma vertex Vert
			#pragma fragment Frag

            #define SHADERPASS SHADERPASS_TRANSPARENT_DEPTH_PREPASS
			#define CUTOFF_TRANSPARENT_DEPTH_PREPASS

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/GeometricTools.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Tessellation.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
            #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"

			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"

            #ifdef RAYTRACING_SHADER_GRAPH_DEFAULT
                #define RAYTRACING_SHADER_GRAPH_HIGH
            #endif
        
            #ifdef RAYTRACING_SHADER_GRAPH_RAYTRACED
                #define RAYTRACING_SHADER_GRAPH_LOW
            #endif

            #ifndef SHADER_UNLIT
            #if defined(_DOUBLESIDED_ON) && !defined(VARYINGS_NEED_CULLFACE)
                #define VARYINGS_NEED_CULLFACE
            #endif
            #endif

			#if defined(_DOUBLESIDED_ON) && !defined(ASE_NEED_CULLFACE)
			    #define ASE_NEED_CULLFACE 1
			#endif

            #if _MATERIAL_FEATURE_COLORED_TRANSMISSION
            #undef _MATERIAL_FEATURE_CLEAR_COAT
            #endif

		    #if defined(_MATERIAL_FEATURE_SUBSURFACE_SCATTERING) && !defined(_SURFACE_TYPE_TRANSPARENT)
		    #define OUTPUT_SPLIT_LIGHTING
		    #endif

            #if (SHADERPASS == SHADERPASS_PATH_TRACING) && !defined(_DOUBLESIDED_ON) && (defined(_REFRACTION_PLANE) || defined(_REFRACTION_SPHERE))
            #undef  _REFRACTION_PLANE
            #undef  _REFRACTION_SPHERE
            #define _REFRACTION_THIN
            #endif

            #if SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_PREPASS
            #if !defined(_DISABLE_SSR_TRANSPARENT) && !defined(SHADER_UNLIT)
                #define WRITE_NORMAL_BUFFER
            #endif
            #endif

            #if SHADERPASS == SHADERPASS_MOTION_VECTORS && defined(WRITE_DECAL_BUFFER_AND_RENDERING_LAYER)
                #define WRITE_DECAL_BUFFER
            #endif

            #ifndef DEBUG_DISPLAY
                #if !defined(_SURFACE_TYPE_TRANSPARENT)
                    #if SHADERPASS == SHADERPASS_FORWARD
                    #define SHADERPASS_FORWARD_BYPASS_ALPHA_TEST
                    #elif SHADERPASS == SHADERPASS_GBUFFER
                    #define SHADERPASS_GBUFFER_BYPASS_ALPHA_TEST
                    #endif
                #endif
            #endif

            #if defined(SHADER_LIT) && !defined(_SURFACE_TYPE_TRANSPARENT)
                #define _DEFERRED_CAPABLE_MATERIAL
            #endif

            #if (defined(_TRANSPARENT_WRITES_MOTION_VEC) || defined(_TRANSPARENT_REFRACTIVE_SORT)) && defined(_SURFACE_TYPE_TRANSPARENT)
                #define _WRITE_TRANSPARENT_MOTION_VECTOR
            #endif

			CBUFFER_START( UnityPerMaterial )
			float4 _EmissionColor;
			float _AlphaCutoff;
			float _RenderQueueType;
			#ifdef _ADD_PRECOMPUTED_VELOCITY
			    float _AddPrecomputedVelocity;
			#endif
			float _StencilRef;
			float _StencilWriteMask;
			float _StencilRefDepth;
			float _StencilWriteMaskDepth;
			float _StencilRefMV;
			float _StencilWriteMaskMV;
			float _StencilRefDistortionVec;
			float _StencilWriteMaskDistortionVec;
			float _StencilWriteMaskGBuffer;
			float _StencilRefGBuffer;
			float _ZTestGBuffer;
			float _RequireSplitLighting;
			float _ReceivesSSR;
			float _SurfaceType;
			float _BlendMode;
            #ifdef SUPPORT_BLENDMODE_PRESERVE_SPECULAR_LIGHTING
			    float _EnableBlendModePreserveSpecularLighting;
            #endif
			float _SrcBlend;
			float _DstBlend;
			float _DstBlend2;
			float _AlphaSrcBlend;
			float _AlphaDstBlend;
			float _ZWrite;
			float _TransparentZWrite;
			float _CullMode;
			float _TransparentSortPriority;
			float _EnableFogOnTransparent;
			float _CullModeForward;
			float _TransparentCullMode;
			float _ZTestDepthEqualForOpaque;
			float _ZTestTransparent;
			float _TransparentBackfaceEnable;
			float _AlphaCutoffEnable;
			float _UseShadowThreshold;
			float _DoubleSidedEnable;
			float _DoubleSidedNormalMode;
			float4 _DoubleSidedConstants;
			#ifdef ASE_TESSELLATION
			     float _TessPhongStrength;
			     float _TessValue;
			     float _TessMin;
			     float _TessMax;
			     float _TessEdgeLength;
			     float _TessMaxDisp;
			#endif
			UNITY_TEXTURE_STREAMING_DEBUG_VARS;
			CBUFFER_END

            #ifdef SCENEPICKINGPASS
			float4 _SelectionID;
            #endif

            #ifdef SCENESELECTIONPASS
			int _ObjectId;
			int _PassValue;
            #endif

			/*ase_globals*/

            #ifdef DEBUG_DISPLAY
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Debug/DebugDisplay.hlsl"
            #endif

            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/NormalSurfaceGradient.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/BuiltinUtilities.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialUtilities.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphFunctions.hlsl"

            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/DecalUtilities.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitDecalData.hlsl"

        	#ifdef HAVE_VFX_MODIFICATION
        	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/VisualEffectVertex.hlsl"
        	#endif

			/*ase_pragma*/

			struct AttributesMesh
			{
				float3 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 tangentOS : TANGENT;
				/*ase_vdata:p=p;n=n;t=t*/
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryingsMeshToPS
			{
				SV_POSITION_QUALIFIERS float4 positionCS : SV_Position;
				float4 clipPosV : TEXCOORD0;
				float3 positionRWS : TEXCOORD1;
				float3 normalWS : TEXCOORD2;
				float4 tangentWS : TEXCOORD3;
				/*ase_interp(4,):sp=sp.xyzw;rwp=tc1;wn=tc2;wt=tc3*/
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
				#if defined(SHADER_STAGE_FRAGMENT) && defined(ASE_NEED_CULLFACE)
				FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
				#endif
			};

			/*ase_funcs*/

			void BuildSurfaceData(FragInputs fragInputs, inout PrePassSurfaceDescription surfaceDescription, float3 V, PositionInputs posInput, out SurfaceData surfaceData, out float3 bentNormalWS)
			{
				ZERO_INITIALIZE(SurfaceData, surfaceData);

				surfaceData.specularOcclusion = 1.0;
				surfaceData.thickness = 0.0;
				surfaceData.perceptualSmoothness =		surfaceDescription.Smoothness;

				// refraction TransparentDepthPrepass
                #if defined(_REFRACTION_PLANE) || defined(_REFRACTION_SPHERE) || defined(_REFRACTION_THIN)
                    if (_EnableSSRefraction)
                    {
        
                        surfaceData.transmittanceMask = (1.0 - surfaceDescription.Alpha);
                        surfaceDescription.Alpha = 1.0;
                    }
                    else
                    {
                        surfaceData.ior = 1.0;
                        surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                        surfaceData.atDistance = 1.0;
                        surfaceData.transmittanceMask = 0.0;
                        surfaceDescription.Alpha = 1.0;
                }
                #else
                    surfaceData.ior = 1.0;
                    surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                    surfaceData.atDistance = 1.0;
                    surfaceData.transmittanceMask = 0.0;
                #endif

				surfaceData.materialFeatures = MATERIALFEATUREFLAGS_LIT_STANDARD;

				#ifdef _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SUBSURFACE_SCATTERING;
				#endif

				#ifdef _MATERIAL_FEATURE_TRANSMISSION
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_TRANSMISSION;
				#endif

				#ifdef _MATERIAL_FEATURE_COLORED_TRANSMISSION
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_TRANSMISSION;
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_COLORED_TRANSMISSION;
				#endif

                #ifdef _MATERIAL_FEATURE_ANISOTROPY
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_ANISOTROPY;
                    surfaceData.normalWS = float3(0, 1, 0);
                #endif

				#ifdef _MATERIAL_FEATURE_CLEAR_COAT
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_CLEAR_COAT;
				#endif

				#ifdef _MATERIAL_FEATURE_IRIDESCENCE
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_IRIDESCENCE;
				#endif

				#ifdef _MATERIAL_FEATURE_SPECULAR_COLOR
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SPECULAR_COLOR;
				#endif

				#if defined (_MATERIAL_FEATURE_SPECULAR_COLOR) && defined (_ENERGY_CONSERVING_SPECULAR)
                    surfaceData.baseColor *= ( 1.0 - Max3( surfaceData.specularColor.r, surfaceData.specularColor.g, surfaceData.specularColor.b ) );
				#endif

				#ifdef _DOUBLESIDED_ON
                    float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
				#else
                    float3 doubleSidedConstants = float3( 1.0, 1.0, 1.0 );
				#endif

				float3 normalTS = float3(0.0f, 0.0f, 1.0f);
				normalTS = surfaceDescription.Normal;

                #ifdef DECAL_NORMAL_BLENDING
					normalTS = SurfaceGradientFromTangentSpaceNormalAndFromTBN(normalTS, fragInputs.tangentToWorld[0], fragInputs.tangentToWorld[1]);

                    #if HAVE_DECALS
                    if (_EnableDecals)
                    {
                        float alpha = 1.0;
                        alpha = surfaceDescription.Alpha;

                        DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, fragInputs, alpha);
                        ApplyDecalToSurfaceNormal(decalSurfaceData, fragInputs.tangentToWorld[2], normalTS);
                        ApplyDecalToSurfaceDataNoNormal(decalSurfaceData, surfaceData);
                     }
                    #endif

                    GetNormalWS_SG(fragInputs, normalTS, surfaceData.normalWS, doubleSidedConstants);
                #else
					GetNormalWS(fragInputs, normalTS, surfaceData.normalWS, doubleSidedConstants);

                    #if HAVE_DECALS
                    if (_EnableDecals)
                    {
                        float alpha = 1.0;
                        alpha = surfaceDescription.Alpha;

                        DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, fragInputs, alpha);
                        ApplyDecalToSurfaceNormal(decalSurfaceData, surfaceData.normalWS.xyz);
                        ApplyDecalToSurfaceDataNoNormal(decalSurfaceData, surfaceData);
                    }
                    #endif
                #endif

				surfaceData.geomNormalWS = fragInputs.tangentToWorld[2];
                surfaceData.tangentWS = normalize(fragInputs.tangentToWorld[0].xyz );
                surfaceData.tangentWS = Orthonormalize(surfaceData.tangentWS, surfaceData.normalWS);

				bentNormalWS = surfaceData.normalWS;

				#if defined(DEBUG_DISPLAY)
				    #if !defined(SHADER_STAGE_RAY_TRACING)
				    if (_DebugMipMapMode != DEBUGMIPMAPMODE_NONE)
				    {
					   #ifdef FRAG_INPUTS_USE_TEXCOORD0
                           surfaceData.baseColor = GET_TEXTURE_STREAMING_DEBUG(posInput.positionSS, fragInputs.texCoord0);
                       #else
                           surfaceData.baseColor = GET_TEXTURE_STREAMING_DEBUG_NO_UV(posInput.positionSS);
                       #endif
					   surfaceData.metallic = 0;
				    }
					#endif
				    ApplyDebugToSurfaceData(fragInputs.tangentToWorld, surfaceData); 
				#endif

                #if defined(_SPECULAR_OCCLUSION_CUSTOM)
                #elif defined(_SPECULAR_OCCLUSION_FROM_AO_BENT_NORMAL)
                    surfaceData.specularOcclusion = GetSpecularOcclusionFromBentAO(V, bentNormalWS, surfaceData.normalWS, surfaceData.ambientOcclusion, PerceptualSmoothnessToPerceptualRoughness(surfaceData.perceptualSmoothness));
                #elif defined(_AMBIENT_OCCLUSION) && defined(_SPECULAR_OCCLUSION_FROM_AO)
                    surfaceData.specularOcclusion = GetSpecularOcclusionFromAmbientOcclusion(ClampNdotV(dot(surfaceData.normalWS, V)), surfaceData.ambientOcclusion, PerceptualSmoothnessToRoughness(surfaceData.perceptualSmoothness));
                #endif

                #ifdef _ENABLE_GEOMETRIC_SPECULAR_AA
                    surfaceData.perceptualSmoothness = GeometricNormalFiltering(surfaceData.perceptualSmoothness, fragInputs.tangentToWorld[2], surfaceDescription.SpecularAAScreenSpaceVariance, surfaceDescription.SpecularAAThreshold);
                #endif
			}

			// Get Surface And BuiltinData
			void GetSurfaceAndBuiltinData(PrePassSurfaceDescription surfaceDescription, FragInputs fragInputs, float3 V, inout PositionInputs posInput, out SurfaceData surfaceData, out BuiltinData builtinData)
			{
				#ifdef LOD_FADE_CROSSFADE
                    LODDitheringTransition(ComputeFadeMaskSeed(V, posInput.positionSS), unity_LODFade.x);
				#endif

                #ifdef _DOUBLESIDED_ON
                    float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
                #else
                    float3 doubleSidedConstants = float3(1.0, 1.0, 1.0);
                #endif
                ApplyDoubleSidedFlipOrMirror(fragInputs, doubleSidedConstants);

				#ifdef _ALPHATEST_ON
                    DoAlphaTest( surfaceDescription.Alpha, surfaceDescription.AlphaClipThreshold );
				#endif

				#ifdef _ALPHATEST_SHADOW_ON
                    DoAlphaTest( surfaceDescription.Alpha, surfaceDescription.AlphaClipThresholdShadow);
				#endif

				#ifdef _DEPTHOFFSET_ON
                    ApplyDepthOffsetPositionInput(V, surfaceDescription.DepthOffset, GetViewForwardDir(), GetWorldToHClipMatrix(), posInput);
				#endif

				float3 bentNormalWS;
                BuildSurfaceData(fragInputs, surfaceDescription, V, posInput, surfaceData, bentNormalWS);
                InitBuiltinData(posInput, surfaceDescription.Alpha, bentNormalWS, -fragInputs.tangentToWorld[2], fragInputs.texCoord1, fragInputs.texCoord2, builtinData);

				#ifdef _DEPTHOFFSET_ON
                    builtinData.depthOffset = surfaceDescription.DepthOffset;
				#endif

                #ifdef _ALPHATEST_ON
                    builtinData.alphaClipTreshold = surfaceDescription.AlphaClipThreshold;
                #endif

                #ifdef UNITY_VIRTUAL_TEXTURING
                    builtinData.vtPackedFeedback = surfaceDescription.VTPackedFeedback;
                #endif

				#ifdef ASE_BAKEDGI
                    builtinData.bakeDiffuseLighting = surfaceDescription.BakedGI;
				#endif

				#ifdef ASE_BAKEDBACKGI
                    builtinData.backBakeDiffuseLighting = surfaceDescription.BakedBackGI;
				#endif

                builtinData.emissiveColor = surfaceDescription.Emission;

                PostInitBuiltinData(V, posInput, surfaceData, builtinData);
			}

			PackedVaryingsMeshToPS VertexFunction(AttributesMesh inputMesh /*ase_vert_input*/)
			{
				PackedVaryingsMeshToPS output;
				UNITY_SETUP_INSTANCE_ID(inputMesh);
				UNITY_TRANSFER_INSTANCE_ID(inputMesh, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( output );

				/*ase_vert_code:inputMesh=AttributesMesh;output=PackedVaryingsMeshToPS*/

				#ifdef ASE_ABSOLUTE_VERTEX_POS
				float3 defaultVertexValue = inputMesh.positionOS.xyz;
				#else
				float3 defaultVertexValue = float3( 0, 0, 0 );
				#endif
				float3 vertexValue = /*ase_vert_out:Vertex Offset;Float3;2;-1;_VertexOffset*/ defaultVertexValue /*end*/;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
				inputMesh.positionOS.xyz = vertexValue;
				#else
				inputMesh.positionOS.xyz += vertexValue;
				#endif

				inputMesh.normalOS = /*ase_vert_out:Vertex Normal;Float3;3;-1;_VertexNormal*/ inputMesh.normalOS /*end*/;
				inputMesh.tangentOS = /*ase_vert_out:Vertex Tangent;Float4;7;-1;_VertexTangent*/ inputMesh.tangentOS /*end*/;

				float3 positionRWS = TransformObjectToWorld(inputMesh.positionOS);
				float3 normalWS = TransformObjectToWorldNormal(inputMesh.normalOS);
				float4 tangentWS = float4(TransformObjectToWorldDir(inputMesh.tangentOS.xyz), inputMesh.tangentOS.w);

				output.positionCS = TransformWorldToHClip(positionRWS);
				output.clipPosV = output.positionCS;
				output.positionRWS  = positionRWS;
				output.normalWS = normalWS;
				output.tangentWS = tangentWS;
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float3 positionOS : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				float4 tangentOS : TANGENT;
				/*ase_vcontrol*/
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl Vert ( AttributesMesh v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.positionOS = v.positionOS;
				o.normalOS = v.normalOS;
				o.tangentOS = v.tangentOS;
				/*ase_control_code:v=AttributesMesh;o=VertexControl*/
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = /*ase_inline_begin*/_TessValue/*ase_inline_end*/; float tessMin = /*ase_inline_begin*/_TessMin/*ase_inline_end*/; float tessMax = /*ase_inline_begin*/_TessMax/*ase_inline_end*/;
				float edgeLength = /*ase_inline_begin*/_TessEdgeLength/*ase_inline_end*/; float tessMaxDisp = /*ase_inline_begin*/_TessMaxDisp/*ase_inline_end*/;
				#if (SHADEROPTIONS_CAMERA_RELATIVE_RENDERING != 0)
				float3 cameraPos = 0;
				#else
				float3 cameraPos = _WorldSpaceCameraPos;
				#endif
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(float4(v[0].positionOS,1), float4(v[1].positionOS,1), float4(v[2].positionOS,1), tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), cameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(float4(v[0].positionOS,1), float4(v[1].positionOS,1), float4(v[2].positionOS,1), edgeLength, GetObjectToWorldMatrix(), cameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(float4(v[0].positionOS,1), float4(v[1].positionOS,1), float4(v[2].positionOS,1), edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), cameraPos, _ScreenParams, _FrustumPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			PackedVaryingsMeshToPS DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				AttributesMesh o = (AttributesMesh)0;
				o.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				o.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				o.tangentOS = patch[0].tangentOS * bary.x + patch[1].tangentOS * bary.y + patch[2].tangentOS * bary.z;
				/*ase_domain_code:patch=VertexControl;o=AttributesMesh;bary=SV_DomainLocation*/
            #if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.positionOS.xyz - patch[i].normalOS * (dot(o.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = /*ase_inline_begin*/_TessPhongStrength/*ase_inline_end*/;
				o.positionOS.xyz = phongStrength * (pp[0] * bary.x + pp[1] * bary.y + pp[2] * bary.z) + (1.0f - phongStrength) * o.positionOS.xyz;
            #endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
            #else
			PackedVaryingsMeshToPS Vert(AttributesMesh v)
			{
				return VertexFunction(v);
			}
            #endif

			#if (defined(WRITE_DECAL_BUFFER) && !defined(_DISABLE_DECALS)) || defined(WRITE_RENDERING_LAYER)
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/DecalPrepassBuffer.hlsl"
			#endif

            #if defined(WRITE_NORMAL_BUFFER) && defined(WRITE_MSAA_DEPTH)
            #define SV_TARGET_DECAL SV_Target2
            #elif defined(WRITE_NORMAL_BUFFER) || defined(WRITE_MSAA_DEPTH)
            #define SV_TARGET_DECAL SV_Target1
            #else
            #define SV_TARGET_DECAL SV_Target0
            #endif

			void Frag(PackedVaryingsMeshToPS packedInput
						#if defined(SCENESELECTIONPASS) || defined(SCENEPICKINGPASS)
						, out float4 outColor : SV_Target0
						#else
							#ifdef WRITE_MSAA_DEPTH
							, out float4 depthColor : SV_Target0
								#ifdef WRITE_NORMAL_BUFFER
								, out float4 outNormalBuffer : SV_Target1
								#endif
							#else
								#ifdef WRITE_NORMAL_BUFFER
								, out float4 outNormalBuffer : SV_Target0
								#endif
							#endif

							#if (defined(WRITE_DECAL_BUFFER) && !defined(_DISABLE_DECALS)) || defined(WRITE_RENDERING_LAYER)
							, out float4 outDecalBuffer : SV_TARGET_DECAL
							#endif
						#endif

						#if defined(_DEPTHOFFSET_ON) && !defined(SCENEPICKINGPASS)
						, out float outputDepth : DEPTH_OFFSET_SEMANTIC
						#endif
						/*ase_frag_input*/
					)
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(packedInput);
				UNITY_SETUP_INSTANCE_ID(packedInput);

				/*ase_local_var:rwp*/float3 PositionRWS = packedInput.positionRWS;
				/*ase_local_var:wvd*/float3 V = GetWorldSpaceNormalizeViewDir( packedInput.positionRWS );
				/*ase_local_var:wn*/float3 NormalWS = packedInput.normalWS;
				/*ase_local_var:wt*/float4 TangentWS = packedInput.tangentWS;
				/*ase_local_var:sp*/float4 ClipPos = packedInput.clipPosV;
				/*ase_local_var:spu*/float4 ScreenPos = ComputeScreenPos( packedInput.clipPosV, _ProjectionParams.x );

				FragInputs input;
				ZERO_INITIALIZE(FragInputs, input);
				input.positionSS = packedInput.positionCS;
				input.positionRWS = PositionRWS;
				input.tangentToWorld = BuildTangentToWorld(TangentWS, NormalWS);

				#if _DOUBLESIDED_ON && SHADER_STAGE_FRAGMENT
				input.isFrontFace = IS_FRONT_VFACE(packedInput.cullFace, true, false);
				#elif SHADER_STAGE_FRAGMENT
				#if defined(ASE_NEED_CULLFACE)
				input.isFrontFace = IS_FRONT_VFACE(packedInput.cullFace, true, false);
				#endif
				#endif
				/*ase_local_var:vf*/half IsFrontFace = input.isFrontFace;

				PositionInputs posInput = GetPositionInput(input.positionSS.xy, _ScreenSize.zw, input.positionSS.z, input.positionSS.w, input.positionRWS);

				PrePassSurfaceDescription surfaceDescription = (PrePassSurfaceDescription)0;
				/*ase_frag_code:packedInput=PackedVaryingsMeshToPS*/
				surfaceDescription.Alpha = /*ase_frag_out:Alpha;Float;0;-1;_Alpha*/1/*end*/;

				surfaceDescription.AlphaClipThresholdDepthPrepass = /*ase_frag_out:Alpha Clip Threshold Depth Prepass;Float;1;-1;_AlphaClipDepthPrepass*/0.5/*end*/;
				surfaceDescription.Smoothness = /*ase_frag_out:Smoothness;Float;5;-1;_Smoothness*/1/*end*/;
				surfaceDescription.Normal = /*ase_frag_out:Normal;Float3;6;-1;_Normal*/float3(0, 0, 1)/*end*/;

				#ifdef _DEPTHOFFSET_ON
				surfaceDescription.DepthOffset = /*ase_frag_out:Depth Offset;Float;4;-1;_DepthOffset*/0/*end*/;
				#endif

				SurfaceData surfaceData;
				BuiltinData builtinData;
				GetSurfaceAndBuiltinData(surfaceDescription, input, V, posInput, surfaceData, builtinData);

				#ifdef _DEPTHOFFSET_ON
				outputDepth = posInput.deviceDepth;
				#endif

				#ifdef WRITE_MSAA_DEPTH
					depthColor = packedInput.positionCS.z;
					depthColor.a = SharpenAlpha(builtinData.opacity, builtinData.alphaClipTreshold);
				#endif

				#if defined(WRITE_NORMAL_BUFFER)
				EncodeIntoNormalBuffer(ConvertSurfaceDataToNormalData(surfaceData), outNormalBuffer);
				#endif

				#if (defined(WRITE_DECAL_BUFFER) && !defined(_DISABLE_DECALS)) || defined(WRITE_RENDERING_LAYER)
					DecalPrepassData decalPrepassData;
					#ifdef _DISABLE_DECALS
					ZERO_INITIALIZE(DecalPrepassData, decalPrepassData);
					#else
					decalPrepassData.geomNormalWS = surfaceData.geomNormalWS;
					#endif
					decalPrepassData.renderingLayerMask = GetMeshRenderingLayerMask();
					EncodeIntoDecalPrepassBuffer(decalPrepassData, outDecalBuffer);
				#endif
			}
			ENDHLSL
		}

		/*ase_pass*/
		Pass
		{
			/*ase_hide_pass*/
			Name "TransparentDepthPostpass"
			Tags 
			{ 
				"LightMode" = "TransparentDepthPostpass" 
		    }

			Blend One Zero
			Cull [_CullMode]
			ZWrite On
			ColorMask 0

			HLSLPROGRAM
			#pragma multi_compile _ DOTS_INSTANCING_ON

            #pragma shader_feature _ _SURFACE_TYPE_TRANSPARENT
            #pragma shader_feature_local _ _TRANSPARENT_WRITES_MOTION_VEC _TRANSPARENT_REFRACTIVE_SORT
            #pragma shader_feature_local_fragment _ _ENABLE_FOG_ON_TRANSPARENT

			#pragma vertex Vert
			#pragma fragment Frag

            #define SHADERPASS SHADERPASS_TRANSPARENT_DEPTH_POSTPASS
			#define CUTOFF_TRANSPARENT_DEPTH_POSTPASS

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/GeometricTools.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Tessellation.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
            #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"

			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"

            #ifdef RAYTRACING_SHADER_GRAPH_DEFAULT
                #define RAYTRACING_SHADER_GRAPH_HIGH
            #endif

            #ifdef RAYTRACING_SHADER_GRAPH_RAYTRACED
                #define RAYTRACING_SHADER_GRAPH_LOW
            #endif

            #ifndef SHADER_UNLIT
            #if defined(_DOUBLESIDED_ON) && !defined(VARYINGS_NEED_CULLFACE)
                #define VARYINGS_NEED_CULLFACE
            #endif
            #endif

			#if defined(_DOUBLESIDED_ON) && !defined(ASE_NEED_CULLFACE)
			    #define ASE_NEED_CULLFACE 1
			#endif

            #if _MATERIAL_FEATURE_COLORED_TRANSMISSION
            #undef _MATERIAL_FEATURE_CLEAR_COAT
            #endif

		    #if defined(_MATERIAL_FEATURE_SUBSURFACE_SCATTERING) && !defined(_SURFACE_TYPE_TRANSPARENT)
		    #define OUTPUT_SPLIT_LIGHTING
		    #endif

		    #if (SHADERPASS == SHADERPASS_PATH_TRACING) && !defined(_DOUBLESIDED_ON) && (defined(_REFRACTION_PLANE) || defined(_REFRACTION_SPHERE))
			    #undef  _REFRACTION_PLANE
			    #undef  _REFRACTION_SPHERE
			    #define _REFRACTION_THIN
		    #endif

            #if SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_PREPASS
            #if !defined(_DISABLE_SSR_TRANSPARENT) && !defined(SHADER_UNLIT)
                #define WRITE_NORMAL_BUFFER
            #endif
            #endif

            #if SHADERPASS == SHADERPASS_MOTION_VECTORS && defined(WRITE_DECAL_BUFFER_AND_RENDERING_LAYER)
                #define WRITE_DECAL_BUFFER
            #endif

            #ifndef DEBUG_DISPLAY
                #if !defined(_SURFACE_TYPE_TRANSPARENT)
                    #if SHADERPASS == SHADERPASS_FORWARD
                    #define SHADERPASS_FORWARD_BYPASS_ALPHA_TEST
                    #elif SHADERPASS == SHADERPASS_GBUFFER
                    #define SHADERPASS_GBUFFER_BYPASS_ALPHA_TEST
                    #endif
                #endif
            #endif

            #if defined(SHADER_LIT) && !defined(_SURFACE_TYPE_TRANSPARENT)
                #define _DEFERRED_CAPABLE_MATERIAL
            #endif

            #if (defined(_TRANSPARENT_WRITES_MOTION_VEC) || defined(_TRANSPARENT_REFRACTIVE_SORT)) && defined(_SURFACE_TYPE_TRANSPARENT)
                #define _WRITE_TRANSPARENT_MOTION_VECTOR
            #endif

			CBUFFER_START( UnityPerMaterial )
			float4 _EmissionColor;
			float _AlphaCutoff;
			float _RenderQueueType;
			#ifdef _ADD_PRECOMPUTED_VELOCITY
			    float _AddPrecomputedVelocity;
			#endif
			float _StencilRef;
			float _StencilWriteMask;
			float _StencilRefDepth;
			float _StencilWriteMaskDepth;
			float _StencilRefMV;
			float _StencilWriteMaskMV;
			float _StencilRefDistortionVec;
			float _StencilWriteMaskDistortionVec;
			float _StencilWriteMaskGBuffer;
			float _StencilRefGBuffer;
			float _ZTestGBuffer;
			float _RequireSplitLighting;
			float _ReceivesSSR;
			float _SurfaceType;
			float _BlendMode;
            #ifdef SUPPORT_BLENDMODE_PRESERVE_SPECULAR_LIGHTING
			    float _EnableBlendModePreserveSpecularLighting;
            #endif
			float _SrcBlend;
			float _DstBlend;
			float _DstBlend2;
			float _AlphaSrcBlend;
			float _AlphaDstBlend;
			float _ZWrite;
			float _TransparentZWrite;
			float _CullMode;
			float _TransparentSortPriority;
			float _EnableFogOnTransparent;
			float _CullModeForward;
			float _TransparentCullMode;
			float _ZTestDepthEqualForOpaque;
			float _ZTestTransparent;
			float _TransparentBackfaceEnable;
			float _AlphaCutoffEnable;
			float _UseShadowThreshold;
			float _DoubleSidedEnable;
			float _DoubleSidedNormalMode;
			float4 _DoubleSidedConstants;
			#ifdef ASE_TESSELLATION
			    float _TessPhongStrength;
			    float _TessValue;
			    float _TessMin;
			    float _TessMax;
			    float _TessEdgeLength;
			    float _TessMaxDisp;
			#endif
			CBUFFER_END

			#ifdef SCENEPICKINGPASS
			float4 _SelectionID;
            #endif

            #ifdef SCENESELECTIONPASS
			int _ObjectId;
			int _PassValue;
            #endif

			/*ase_globals*/

            #ifdef DEBUG_DISPLAY
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Debug/DebugDisplay.hlsl"
            #endif

			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/NormalSurfaceGradient.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/BuiltinUtilities.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialUtilities.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphFunctions.hlsl"

            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/DecalUtilities.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitDecalData.hlsl"

			#if (defined(WRITE_DECAL_BUFFER) && !defined(_DISABLE_DECALS)) || defined(WRITE_RENDERING_LAYER)
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/DecalPrepassBuffer.hlsl"
			#endif

        	#ifdef HAVE_VFX_MODIFICATION
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/VisualEffectVertex.hlsl"
        	#endif

			/*ase_pragma*/

			struct AttributesMesh
			{
				float3 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 tangentOS : TANGENT;
				/*ase_vdata:p=p;n=n;t=t*/
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryingsMeshToPS
			{
				SV_POSITION_QUALIFIERS float4 positionCS : SV_Position;
				float4 clipPosV : TEXCOORD0;
				float3 positionRWS : TEXCOORD1;
				/*ase_interp(2,):sp=sp.xyzw;rwp=tc1*/
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
				#if defined(SHADER_STAGE_FRAGMENT) && defined(ASE_NEED_CULLFACE)
				FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
				#endif
			};

			/*ase_funcs*/

			void BuildSurfaceData(FragInputs fragInputs, inout PostPassSurfaceDescription surfaceDescription, float3 V, PositionInputs posInput, out SurfaceData surfaceData, out float3 bentNormalWS)
			{
				ZERO_INITIALIZE(SurfaceData, surfaceData);

				surfaceData.specularOcclusion = 1.0;
				surfaceData.thickness = 0.0;

				// refraction TransparentDepthPostpass
                #if defined(_REFRACTION_PLANE) || defined(_REFRACTION_SPHERE) || defined(_REFRACTION_THIN)
                    if (_EnableSSRefraction)
                    {
        
                        surfaceData.transmittanceMask = (1.0 - surfaceDescription.Alpha);
                        surfaceDescription.Alpha = 1.0;
                    }
                    else
                    {
                        surfaceData.ior = 1.0;
                        surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                        surfaceData.atDistance = 1.0;
                        surfaceData.transmittanceMask = 0.0;
                        surfaceDescription.Alpha = 1.0;
                    }
                #else
                    surfaceData.ior = 1.0;
                    surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                    surfaceData.atDistance = 1.0;
                    surfaceData.transmittanceMask = 0.0;
                #endif

				surfaceData.materialFeatures = MATERIALFEATUREFLAGS_LIT_STANDARD;

				#ifdef _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SUBSURFACE_SCATTERING;
				#endif

				#ifdef _MATERIAL_FEATURE_TRANSMISSION
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_TRANSMISSION;
				#endif

				#ifdef _MATERIAL_FEATURE_COLORED_TRANSMISSION
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_TRANSMISSION;
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_COLORED_TRANSMISSION;
				#endif

                #ifdef _MATERIAL_FEATURE_ANISOTROPY
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_ANISOTROPY;
                    surfaceData.normalWS = float3(0, 1, 0);
                #endif

				#ifdef _MATERIAL_FEATURE_CLEAR_COAT
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_CLEAR_COAT;
				#endif

				#ifdef _MATERIAL_FEATURE_IRIDESCENCE
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_IRIDESCENCE;
				#endif

				#ifdef _MATERIAL_FEATURE_SPECULAR_COLOR
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SPECULAR_COLOR;
				#endif

				#if defined (_MATERIAL_FEATURE_SPECULAR_COLOR) && defined (_ENERGY_CONSERVING_SPECULAR)
                    surfaceData.baseColor *= ( 1.0 - Max3( surfaceData.specularColor.r, surfaceData.specularColor.g, surfaceData.specularColor.b ) );
				#endif

				#ifdef _DOUBLESIDED_ON
                    float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
				#else
                    float3 doubleSidedConstants = float3( 1.0, 1.0, 1.0 );
				#endif

				float3 normalTS = float3(0.0f, 0.0f, 1.0f);

                #ifdef DECAL_NORMAL_BLENDING
					normalTS = SurfaceGradientFromTangentSpaceNormalAndFromTBN(normalTS, fragInputs.tangentToWorld[0], fragInputs.tangentToWorld[1]);

                    #if HAVE_DECALS
                    if (_EnableDecals)
                    {
                        float alpha = 1.0;
                        alpha = surfaceDescription.Alpha;

                        DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, fragInputs, alpha);
                        ApplyDecalToSurfaceNormal(decalSurfaceData, fragInputs.tangentToWorld[2], normalTS);
                        ApplyDecalToSurfaceDataNoNormal(decalSurfaceData, surfaceData);
                     }
                    #endif

                    GetNormalWS_SG(fragInputs, normalTS, surfaceData.normalWS, doubleSidedConstants);
                #else
					GetNormalWS(fragInputs, normalTS, surfaceData.normalWS, doubleSidedConstants);

                    #if HAVE_DECALS
                    if (_EnableDecals)
                    {
                        float alpha = 1.0;
                        alpha = surfaceDescription.Alpha;
 
                        DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, fragInputs, alpha);
                        ApplyDecalToSurfaceNormal(decalSurfaceData, surfaceData.normalWS.xyz);
                        ApplyDecalToSurfaceDataNoNormal(decalSurfaceData, surfaceData);
                    }
                    #endif
                #endif

				surfaceData.geomNormalWS = fragInputs.tangentToWorld[2];
                surfaceData.tangentWS = normalize(fragInputs.tangentToWorld[0].xyz );
                surfaceData.tangentWS = Orthonormalize(surfaceData.tangentWS, surfaceData.normalWS);

				bentNormalWS = surfaceData.normalWS;

				#if defined(DEBUG_DISPLAY)
				    #if !defined(SHADER_STAGE_RAY_TRACING)
				    if (_DebugMipMapMode != DEBUGMIPMAPMODE_NONE)
				    {
					   #ifdef FRAG_INPUTS_USE_TEXCOORD0
                           surfaceData.baseColor = GET_TEXTURE_STREAMING_DEBUG(posInput.positionSS, fragInputs.texCoord0);
                       #else
                           surfaceData.baseColor = GET_TEXTURE_STREAMING_DEBUG_NO_UV(posInput.positionSS);
                       #endif
					   surfaceData.metallic = 0;
				    }
					#endif
				    ApplyDebugToSurfaceData(fragInputs.tangentToWorld, surfaceData); 
				#endif

                #if defined(_SPECULAR_OCCLUSION_CUSTOM)
                #elif defined(_SPECULAR_OCCLUSION_FROM_AO_BENT_NORMAL)
                    surfaceData.specularOcclusion = GetSpecularOcclusionFromBentAO(V, bentNormalWS, surfaceData.normalWS, surfaceData.ambientOcclusion, PerceptualSmoothnessToPerceptualRoughness(surfaceData.perceptualSmoothness));
                #elif defined(_AMBIENT_OCCLUSION) && defined(_SPECULAR_OCCLUSION_FROM_AO)
                    surfaceData.specularOcclusion = GetSpecularOcclusionFromAmbientOcclusion(ClampNdotV(dot(surfaceData.normalWS, V)), surfaceData.ambientOcclusion, PerceptualSmoothnessToRoughness(surfaceData.perceptualSmoothness));
                #endif

                #ifdef _ENABLE_GEOMETRIC_SPECULAR_AA
                    surfaceData.perceptualSmoothness = GeometricNormalFiltering(surfaceData.perceptualSmoothness, fragInputs.tangentToWorld[2], surfaceDescription.SpecularAAScreenSpaceVariance, surfaceDescription.SpecularAAThreshold);
                #endif
			}

			// Get Surface And BuiltinData
			void GetSurfaceAndBuiltinData(PostPassSurfaceDescription surfaceDescription, FragInputs fragInputs, float3 V, inout PositionInputs posInput, out SurfaceData surfaceData, out BuiltinData builtinData)
			{
				#ifdef LOD_FADE_CROSSFADE
                    LODDitheringTransition(ComputeFadeMaskSeed(V, posInput.positionSS), unity_LODFade.x);
				#endif

                #ifdef _DOUBLESIDED_ON
                    float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
                #else
                    float3 doubleSidedConstants = float3(1.0, 1.0, 1.0);
                #endif
                ApplyDoubleSidedFlipOrMirror(fragInputs, doubleSidedConstants);

				#ifdef _ALPHATEST_ON
                    DoAlphaTest( surfaceDescription.Alpha, surfaceDescription.AlphaClipThreshold );
				#endif

				#ifdef _ALPHATEST_SHADOW_ON
                    DoAlphaTest( surfaceDescription.Alpha, surfaceDescription.AlphaClipThresholdShadow);
				#endif

				#ifdef _DEPTHOFFSET_ON
                    ApplyDepthOffsetPositionInput(V, surfaceDescription.DepthOffset, GetViewForwardDir(), GetWorldToHClipMatrix(), posInput);
				#endif

                float3 bentNormalWS;
                BuildSurfaceData(fragInputs, surfaceDescription, V, posInput, surfaceData, bentNormalWS);
                InitBuiltinData(posInput, surfaceDescription.Alpha, bentNormalWS, -fragInputs.tangentToWorld[2], fragInputs.texCoord1, fragInputs.texCoord2, builtinData);

				#ifdef _DEPTHOFFSET_ON
                    builtinData.depthOffset = surfaceDescription.DepthOffset;
				#endif

                #ifdef _ALPHATEST_ON
                    builtinData.alphaClipTreshold = surfaceDescription.AlphaClipThreshold;
                #endif

                #ifdef UNITY_VIRTUAL_TEXTURING
                    builtinData.vtPackedFeedback = surfaceDescription.VTPackedFeedback;
                #endif

				#ifdef ASE_BAKEDGI
                    builtinData.bakeDiffuseLighting = surfaceDescription.BakedGI;
				#endif

				#ifdef ASE_BAKEDBACKGI
                    builtinData.backBakeDiffuseLighting = surfaceDescription.BakedBackGI;
				#endif

                builtinData.emissiveColor = surfaceDescription.Emission;

                PostInitBuiltinData(V, posInput, surfaceData, builtinData);
			}

			PackedVaryingsMeshToPS VertexFunction(AttributesMesh inputMesh /*ase_vert_input*/)
			{
				PackedVaryingsMeshToPS output;
				UNITY_SETUP_INSTANCE_ID(inputMesh);
				UNITY_TRANSFER_INSTANCE_ID(inputMesh, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( output );

				/*ase_vert_code:inputMesh=AttributesMesh;output=PackedVaryingsMeshToPS*/

				#ifdef ASE_ABSOLUTE_VERTEX_POS
				float3 defaultVertexValue = inputMesh.positionOS.xyz;
				#else
				float3 defaultVertexValue = float3( 0, 0, 0 );
				#endif
				float3 vertexValue = /*ase_vert_out:Vertex Offset;Float3;2;-1;_VertexOffset*/ defaultVertexValue /*end*/;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
				inputMesh.positionOS.xyz = vertexValue;
				#else
				inputMesh.positionOS.xyz += vertexValue;
				#endif

				inputMesh.normalOS = /*ase_vert_out:Vertex Normal;Float3;3;-1;_VertexNormal*/ inputMesh.normalOS /*end*/;
				inputMesh.tangentOS = /*ase_vert_out:Vertex Tangent;Float4;27;-1;_VertexTangent*/inputMesh.tangentOS/*end*/;

				float3 positionRWS = TransformObjectToWorld(inputMesh.positionOS);
				output.positionCS = TransformWorldToHClip(positionRWS);
				output.clipPosV = output.positionCS;
				output.positionRWS = positionRWS;
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float3 positionOS : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				float4 tangentOS : TANGENT;
				/*ase_vcontrol*/
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl Vert ( AttributesMesh v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.positionOS = v.positionOS;
				o.normalOS = v.normalOS;
				o.tangentOS = v.tangentOS;
				/*ase_control_code:v=AttributesMesh;o=VertexControl*/
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = /*ase_inline_begin*/_TessValue/*ase_inline_end*/; float tessMin = /*ase_inline_begin*/_TessMin/*ase_inline_end*/; float tessMax = /*ase_inline_begin*/_TessMax/*ase_inline_end*/;
				float edgeLength = /*ase_inline_begin*/_TessEdgeLength/*ase_inline_end*/; float tessMaxDisp = /*ase_inline_begin*/_TessMaxDisp/*ase_inline_end*/;
				#if (SHADEROPTIONS_CAMERA_RELATIVE_RENDERING != 0)
				float3 cameraPos = 0;
				#else
				float3 cameraPos = _WorldSpaceCameraPos;
				#endif
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(float4(v[0].positionOS,1), float4(v[1].positionOS,1), float4(v[2].positionOS,1), tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), cameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(float4(v[0].positionOS,1), float4(v[1].positionOS,1), float4(v[2].positionOS,1), edgeLength, GetObjectToWorldMatrix(), cameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(float4(v[0].positionOS,1), float4(v[1].positionOS,1), float4(v[2].positionOS,1), edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), cameraPos, _ScreenParams, _FrustumPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			PackedVaryingsMeshToPS DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				AttributesMesh o = (AttributesMesh) 0;
				o.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				o.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				o.tangentOS = patch[0].tangentOS * bary.x + patch[1].tangentOS * bary.y + patch[2].tangentOS * bary.z;
				/*ase_domain_code:patch=VertexControl;o=AttributesMesh;bary=SV_DomainLocation*/
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.positionOS.xyz - patch[i].normalOS * (dot(o.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = /*ase_inline_begin*/_TessPhongStrength/*ase_inline_end*/;
				o.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			PackedVaryingsMeshToPS Vert ( AttributesMesh v )
			{
				return VertexFunction( v );
			}
			#endif

			#if defined(WRITE_NORMAL_BUFFER) && defined(WRITE_MSAA_DEPTH)
			#define SV_TARGET_DECAL SV_Target2
			#elif defined(WRITE_NORMAL_BUFFER) || defined(WRITE_MSAA_DEPTH)
			#define SV_TARGET_DECAL SV_Target1
			#else
			#define SV_TARGET_DECAL SV_Target0
			#endif

			void Frag( PackedVaryingsMeshToPS packedInput
						#if defined(SCENESELECTIONPASS) || defined(SCENEPICKINGPASS)
						, out float4 outColor : SV_Target0
						#else
							#ifdef WRITE_MSAA_DEPTH
							, out float4 depthColor : SV_Target0
								#ifdef WRITE_NORMAL_BUFFER
								, out float4 outNormalBuffer : SV_Target1
								#endif
							#else
								#ifdef WRITE_NORMAL_BUFFER
								, out float4 outNormalBuffer : SV_Target0
								#endif
							#endif

							#if (defined(WRITE_DECAL_BUFFER) && !defined(_DISABLE_DECALS)) || defined(WRITE_RENDERING_LAYER)
							, out float4 outDecalBuffer : SV_TARGET_DECAL
							#endif
						#endif

						#if defined(_DEPTHOFFSET_ON) && !defined(SCENEPICKINGPASS)
						, out float outputDepth : DEPTH_OFFSET_SEMANTIC
						#endif
						/*ase_frag_input*/
					)
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(packedInput);
				UNITY_SETUP_INSTANCE_ID(packedInput);
		
				/*ase_local_var:rwp*/float3 PositionRWS = packedInput.positionRWS;
				/*ase_local_var:wvd*/float3 V = GetWorldSpaceNormalizeViewDir( packedInput.positionRWS );
				/*ase_local_var:sp*/float4 ClipPos = packedInput.clipPosV;
				/*ase_local_var:spu*/float4 ScreenPos = ComputeScreenPos( packedInput.clipPosV, _ProjectionParams.x );

				FragInputs input;
				ZERO_INITIALIZE(FragInputs, input);
				input.tangentToWorld = k_identity3x3;
				input.positionSS = packedInput.positionCS;
				input.positionRWS = PositionRWS;

				#if _DOUBLESIDED_ON && SHADER_STAGE_FRAGMENT
				input.isFrontFace = IS_FRONT_VFACE( packedInput.cullFace, true, false);
				#elif SHADER_STAGE_FRAGMENT
				#if defined(ASE_NEED_CULLFACE)
				input.isFrontFace = IS_FRONT_VFACE( packedInput.cullFace, true, false );
				#endif
				#endif
				/*ase_local_var:vf*/half IsFrontFace = input.isFrontFace;

				PositionInputs posInput = GetPositionInput(input.positionSS.xy, _ScreenSize.zw, input.positionSS.z, input.positionSS.w, input.positionRWS);

				PostPassSurfaceDescription surfaceDescription = (PostPassSurfaceDescription)0;
				/*ase_frag_code:packedInput=PackedVaryingsMeshToPS*/
				surfaceDescription.Alpha = /*ase_frag_out:Alpha;Float;0;-1;_Alpha*/1/*end*/;

				surfaceDescription.AlphaClipThresholdDepthPostpass = /*ase_frag_out:Alpha Clip Threshold Depth Postpass;Float;1;-1;_AlphaClipDepthPostpass*/0.5/*end*/;

				#ifdef _DEPTHOFFSET_ON
				surfaceDescription.DepthOffset = /*ase_frag_out:Depth Offset;Float;4;-1;_DepthOffset*/0/*end*/;
				#endif

				SurfaceData surfaceData;
				BuiltinData builtinData;
				GetSurfaceAndBuiltinData(surfaceDescription, input, V, posInput, surfaceData, builtinData);

				#ifdef _DEPTHOFFSET_ON
				outputDepth = posInput.deviceDepth;
				#endif

				#ifdef WRITE_MSAA_DEPTH
					depthColor = packedInput.positionCS.z;
					depthColor.a = SharpenAlpha(builtinData.opacity, builtinData.alphaClipTreshold);
				#endif

				#if defined(WRITE_NORMAL_BUFFER)
					EncodeIntoNormalBuffer(ConvertSurfaceDataToNormalData(surfaceData), outNormalBuffer);
				#endif

				#if (defined(WRITE_DECAL_BUFFER) && !defined(_DISABLE_DECALS)) || defined(WRITE_RENDERING_LAYER)
					DecalPrepassData decalPrepassData;
                    #ifdef _DISABLE_DECALS
				    ZERO_INITIALIZE(DecalPrepassData, decalPrepassData);
                    #else
					decalPrepassData.geomNormalWS = surfaceData.geomNormalWS;
                    #endif
					decalPrepassData.renderingLayerMask = GetMeshRenderingLayerMask();
					EncodeIntoDecalPrepassBuffer(decalPrepassData, outDecalBuffer);
				#endif
			}
			ENDHLSL
		}

		/*ase_pass*/
		Pass
		{
			/*ase_hide_pass*/
			Name "Forward"
			Tags 
			{ 
				"LightMode" = "Forward" 
		    }

			Blend [_SrcBlend] [_DstBlend], [_AlphaSrcBlend] [_AlphaDstBlend]
			Blend 1 One OneMinusSrcAlpha
			Blend 2 One [_DstBlend2]
			Blend 3 One [_DstBlend2]
			Blend 4 One OneMinusSrcAlpha

			Cull [_CullModeForward]
			ZTest [_ZTestDepthEqualForOpaque]
			ZWrite [_ZWrite]

			Stencil
			{
				WriteMask [_StencilWriteMask]
				Ref [_StencilRef]
		        CompFront Always
		        PassFront Replace
		        CompBack Always
		        PassBack Replace
			}

            ColorMask [_ColorMaskTransparentVelOne] 1
            ColorMask [_ColorMaskTransparentVelTwo] 2

			HLSLPROGRAM
			#pragma multi_compile _ DOTS_INSTANCING_ON

            #pragma shader_feature _ _SURFACE_TYPE_TRANSPARENT
            #pragma shader_feature_local _ _TRANSPARENT_WRITES_MOTION_VEC _TRANSPARENT_REFRACTIVE_SORT
            #pragma shader_feature_local_fragment _ _ENABLE_FOG_ON_TRANSPARENT

            #pragma multi_compile_fragment _ SHADOWS_SHADOWMASK
            #pragma multi_compile_fragment PUNCTUAL_SHADOW_LOW PUNCTUAL_SHADOW_MEDIUM PUNCTUAL_SHADOW_HIGH
            #pragma multi_compile_fragment DIRECTIONAL_SHADOW_LOW DIRECTIONAL_SHADOW_MEDIUM DIRECTIONAL_SHADOW_HIGH
            #pragma multi_compile_fragment AREA_SHADOW_MEDIUM AREA_SHADOW_HIGH
            #pragma multi_compile_fragment _ PROBE_VOLUMES_L1 PROBE_VOLUMES_L2
            #pragma multi_compile_fragment SCREEN_SPACE_SHADOWS_OFF SCREEN_SPACE_SHADOWS_ON
            #pragma multi_compile_fragment USE_FPTL_LIGHTLIST USE_CLUSTERED_LIGHTLIST

            #pragma multi_compile _ DEBUG_DISPLAY
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fragment DECALS_OFF DECALS_3RT DECALS_4RT
            #pragma multi_compile_fragment _ DECAL_SURFACE_GRADIENT
            #pragma multi_compile _ USE_LEGACY_LIGHTMAPS

			#ifndef SHADER_STAGE_FRAGMENT
			#define SHADOW_LOW
			#define USE_FPTL_LIGHTLIST
			#endif

			#pragma vertex Vert
			#pragma fragment Frag

			#define SHADERPASS SHADERPASS_FORWARD
		    #define HAS_LIGHTLOOP 1

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/GeometricTools.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Tessellation.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
            #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"

			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"

            #ifdef RAYTRACING_SHADER_GRAPH_DEFAULT
                #define RAYTRACING_SHADER_GRAPH_HIGH
            #endif

            #ifdef RAYTRACING_SHADER_GRAPH_RAYTRACED
                #define RAYTRACING_SHADER_GRAPH_LOW
            #endif

            #ifndef SHADER_UNLIT
            #if defined(_DOUBLESIDED_ON) && !defined(VARYINGS_NEED_CULLFACE)
                #define VARYINGS_NEED_CULLFACE
            #endif
            #endif

			#if defined(_DOUBLESIDED_ON) && !defined(ASE_NEED_CULLFACE)
			    #define ASE_NEED_CULLFACE 1
			#endif

            #if _MATERIAL_FEATURE_COLORED_TRANSMISSION
            #undef _MATERIAL_FEATURE_CLEAR_COAT
            #endif

		    #if defined(_MATERIAL_FEATURE_SUBSURFACE_SCATTERING) && !defined(_SURFACE_TYPE_TRANSPARENT)
			#define OUTPUT_SPLIT_LIGHTING
		    #endif

		    #if (SHADERPASS == SHADERPASS_PATH_TRACING) && !defined(_DOUBLESIDED_ON) && (defined(_REFRACTION_PLANE) || defined(_REFRACTION_SPHERE))
			#undef  _REFRACTION_PLANE
			#undef  _REFRACTION_SPHERE
			#define _REFRACTION_THIN
		    #endif

            #if SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_PREPASS
            #if !defined(_DISABLE_SSR_TRANSPARENT) && !defined(SHADER_UNLIT)
                #define WRITE_NORMAL_BUFFER
            #endif
            #endif

            #if SHADERPASS == SHADERPASS_MOTION_VECTORS && defined(WRITE_DECAL_BUFFER_AND_RENDERING_LAYER)
                #define WRITE_DECAL_BUFFER
            #endif

            #ifndef DEBUG_DISPLAY
                #if !defined(_SURFACE_TYPE_TRANSPARENT)
                    #if SHADERPASS == SHADERPASS_FORWARD
                    #define SHADERPASS_FORWARD_BYPASS_ALPHA_TEST
                    #elif SHADERPASS == SHADERPASS_GBUFFER
                    #define SHADERPASS_GBUFFER_BYPASS_ALPHA_TEST
                    #endif
                #endif
            #endif

            #if defined(SHADER_LIT) && !defined(_SURFACE_TYPE_TRANSPARENT)
                #define _DEFERRED_CAPABLE_MATERIAL
            #endif
        
            #if (defined(_TRANSPARENT_WRITES_MOTION_VEC) || defined(_TRANSPARENT_REFRACTIVE_SORT)) && defined(_SURFACE_TYPE_TRANSPARENT)
                #define _WRITE_TRANSPARENT_MOTION_VECTOR
            #endif

			CBUFFER_START( UnityPerMaterial )
			float4 _EmissionColor;
			float _AlphaCutoff;
			float _RenderQueueType;
			#ifdef _ADD_PRECOMPUTED_VELOCITY
			    float _AddPrecomputedVelocity;
			#endif
			float _StencilRef;
			float _StencilWriteMask;
			float _StencilRefDepth;
			float _StencilWriteMaskDepth;
			float _StencilRefMV;
			float _StencilWriteMaskMV;
			float _StencilRefDistortionVec;
			float _StencilWriteMaskDistortionVec;
			float _StencilWriteMaskGBuffer;
			float _StencilRefGBuffer;
			float _ZTestGBuffer;
			float _RequireSplitLighting;
			float _ReceivesSSR;
			float _SurfaceType;
			float _BlendMode;
            #ifdef SUPPORT_BLENDMODE_PRESERVE_SPECULAR_LIGHTING
			    float _EnableBlendModePreserveSpecularLighting;
            #endif
			float _SrcBlend;
			float _DstBlend;
			float _DstBlend2;
			float _AlphaSrcBlend;
			float _AlphaDstBlend;
			float _ZWrite;
			float _TransparentZWrite;
			float _CullMode;
			float _TransparentSortPriority;
			float _EnableFogOnTransparent;
			float _CullModeForward;
			float _TransparentCullMode;
			float _ZTestDepthEqualForOpaque;
			float _ZTestTransparent;
			float _TransparentBackfaceEnable;
			float _AlphaCutoffEnable;
			float _UseShadowThreshold;
			float _DoubleSidedEnable;
			float _DoubleSidedNormalMode;
			float4 _DoubleSidedConstants;
			#ifdef ASE_TESSELLATION
			    float _TessPhongStrength;
			    float _TessValue;
			    float _TessMin;
			    float _TessMax;
			    float _TessEdgeLength;
			    float _TessMaxDisp;
			#endif
			UNITY_TEXTURE_STREAMING_DEBUG_VARS;
			CBUFFER_END

            #ifdef SCENEPICKINGPASS
			float4 _SelectionID;
            #endif

            #ifdef SCENESELECTIONPASS
			int _ObjectId;
			int _PassValue;
            #endif

			/*ase_globals*/

            #ifdef DEBUG_DISPLAY
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Debug/DebugDisplay.hlsl"
            #endif

            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/NormalSurfaceGradient.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/LightLoop/LightLoopDef.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/LightLoop/LightLoop.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/BuiltinUtilities.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialUtilities.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphFunctions.hlsl"

            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/DecalUtilities.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitDecalData.hlsl"

        	#ifdef HAVE_VFX_MODIFICATION
        	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/VisualEffectVertex.hlsl"
        	#endif

			/*ase_pragma*/

			struct AttributesMesh
			{
				float3 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 tangentOS : TANGENT;
				float4 uv1 : TEXCOORD1;
				float4 uv2 : TEXCOORD2;
				float3 previousPositionOS : TEXCOORD4;
				float3 precomputedVelocity : TEXCOORD5;
				/*ase_vdata:p=p;n=n;t=t;uv1=tc1;uv2=tc2;uv4=tc4;uv5=tc5*/
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryingsMeshToPS
			{
				SV_POSITION_QUALIFIERS float4 positionCS : SV_Position;
				float4 clipPosV : TEXCOORD0;
				float3 positionRWS : TEXCOORD1;
				float3 normalWS : TEXCOORD2;
				float4 tangentWS : TEXCOORD3;
				float4 uv1 : TEXCOORD4;
				float4 uv2 : TEXCOORD5;
				#ifdef _WRITE_TRANSPARENT_MOTION_VECTOR
					float3 vpassPositionCS : TEXCOORD6;
					float3 vpassPreviousPositionCS : TEXCOORD7;
				#endif
				/*ase_interp(8,):sp=sp.xyzw;rwp=tc1;wn=tc2;wt=tc3;uv1=tc4;uv2=tc5*/
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
				#if defined(SHADER_STAGE_FRAGMENT) && defined(ASE_NEED_CULLFACE)
				FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
				#endif
			};

			/*ase_funcs*/

			void BuildSurfaceData(FragInputs fragInputs, inout GlobalSurfaceDescription surfaceDescription, float3 V, PositionInputs posInput, out SurfaceData surfaceData, out float3 bentNormalWS)
			{
				ZERO_INITIALIZE(SurfaceData, surfaceData);
				surfaceData.specularOcclusion = 1.0;
				surfaceData.thickness = 0.0;

				surfaceData.baseColor =                 surfaceDescription.BaseColor;
				surfaceData.perceptualSmoothness =		surfaceDescription.Smoothness;
				surfaceData.ambientOcclusion =			surfaceDescription.Occlusion;
				surfaceData.metallic =					surfaceDescription.Metallic;
				surfaceData.coatMask =					surfaceDescription.CoatMask;

				#ifdef _SPECULAR_OCCLUSION_CUSTOM
				surfaceData.specularOcclusion =			surfaceDescription.SpecularOcclusion;
				#endif

				#ifdef _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
				surfaceData.subsurfaceMask =			surfaceDescription.SubsurfaceMask;
				#endif

				#if defined(_HAS_REFRACTION) || defined(_MATERIAL_FEATURE_TRANSMISSION)
				surfaceData.thickness = 				surfaceDescription.Thickness;
				#endif

				#ifdef _MATERIAL_FEATURE_TRANSMISSION
				surfaceData.transmissionMask =			surfaceDescription.TransmissionMask;
				#endif

				#if defined( _MATERIAL_FEATURE_SUBSURFACE_SCATTERING ) || defined( _MATERIAL_FEATURE_TRANSMISSION )
				surfaceData.diffusionProfileHash =		asuint(surfaceDescription.DiffusionProfile);
				#endif

				#ifdef _MATERIAL_FEATURE_SPECULAR_COLOR
				surfaceData.specularColor =				surfaceDescription.Specular;
				#endif

				#ifdef _MATERIAL_FEATURE_ANISOTROPY
				surfaceData.anisotropy =				surfaceDescription.Anisotropy;
				#endif

				#ifdef _MATERIAL_FEATURE_IRIDESCENCE
				surfaceData.iridescenceMask =			surfaceDescription.IridescenceMask;
				surfaceData.iridescenceThickness =		surfaceDescription.IridescenceThickness;
				#endif

				// refraction
                #if defined(_REFRACTION_PLANE) || defined(_REFRACTION_SPHERE) || defined(_REFRACTION_THIN)
                    if (_EnableSSRefraction)
                    {
                        surfaceData.ior =                       surfaceDescription.RefractionIndex;
                        surfaceData.transmittanceColor =        surfaceDescription.RefractionColor;
                        surfaceData.atDistance =                surfaceDescription.RefractionDistance;
        
                        surfaceData.transmittanceMask = (1.0 - surfaceDescription.Alpha);
                        surfaceDescription.Alpha = 1.0;
                    }
                    else
                    {
                        surfaceData.ior = 1.0;
                        surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                        surfaceData.atDistance = 1.0;
                        surfaceData.transmittanceMask = 0.0;
                        surfaceDescription.Alpha = 1.0;
                    }
                #else
                    surfaceData.ior = 1.0;
                    surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
                    surfaceData.atDistance = 1.0;
                    surfaceData.transmittanceMask = 0.0;
                #endif

				surfaceData.materialFeatures = MATERIALFEATUREFLAGS_LIT_STANDARD;

				#ifdef _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SUBSURFACE_SCATTERING;
				#endif

				#ifdef _MATERIAL_FEATURE_TRANSMISSION
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_TRANSMISSION;
				#endif

				#ifdef _MATERIAL_FEATURE_COLORED_TRANSMISSION
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_TRANSMISSION;
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_COLORED_TRANSMISSION;
				#endif

                #ifdef _MATERIAL_FEATURE_ANISOTROPY
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_ANISOTROPY;
                    surfaceData.normalWS = float3(0, 1, 0);
                #endif

				#ifdef _MATERIAL_FEATURE_CLEAR_COAT
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_CLEAR_COAT;
				#endif

				#ifdef _MATERIAL_FEATURE_IRIDESCENCE
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_IRIDESCENCE;
				#endif

				#ifdef _MATERIAL_FEATURE_SPECULAR_COLOR
                    surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SPECULAR_COLOR;
				#endif

				#if defined (_MATERIAL_FEATURE_SPECULAR_COLOR) && defined (_ENERGY_CONSERVING_SPECULAR)
                    surfaceData.baseColor *= ( 1.0 - Max3( surfaceData.specularColor.r, surfaceData.specularColor.g, surfaceData.specularColor.b ) );
				#endif

				#ifdef _DOUBLESIDED_ON
                    float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
				#else
                    float3 doubleSidedConstants = float3( 1.0, 1.0, 1.0 );
				#endif

				float3 normalTS = float3(0.0f, 0.0f, 1.0f);
				normalTS = surfaceDescription.Normal;

                #ifdef DECAL_NORMAL_BLENDING
					normalTS = SurfaceGradientFromTangentSpaceNormalAndFromTBN(normalTS, fragInputs.tangentToWorld[0], fragInputs.tangentToWorld[1]);

                    #if HAVE_DECALS
                    if (_EnableDecals)
                    {
                        float alpha = 1.0;
                        alpha = surfaceDescription.Alpha;

                        DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, fragInputs, alpha);
                        ApplyDecalToSurfaceNormal(decalSurfaceData, fragInputs.tangentToWorld[2], normalTS);
                        ApplyDecalToSurfaceDataNoNormal(decalSurfaceData, surfaceData);
                     }
                    #endif

                    GetNormalWS_SG(fragInputs, normalTS, surfaceData.normalWS, doubleSidedConstants);
                #else
					GetNormalWS(fragInputs, normalTS, surfaceData.normalWS, doubleSidedConstants);

                    #if HAVE_DECALS
                    if (_EnableDecals)
                    {
                        float alpha = 1.0;
                        alpha = surfaceDescription.Alpha;
        
                        DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, fragInputs, alpha);
                        ApplyDecalToSurfaceNormal(decalSurfaceData, surfaceData.normalWS.xyz);
                        ApplyDecalToSurfaceDataNoNormal(decalSurfaceData, surfaceData);
                    }
                    #endif
                #endif

				surfaceData.geomNormalWS = fragInputs.tangentToWorld[2];
                surfaceData.tangentWS = normalize(fragInputs.tangentToWorld[0].xyz );
                surfaceData.tangentWS = Orthonormalize(surfaceData.tangentWS, surfaceData.normalWS);

				bentNormalWS = surfaceData.normalWS;

				#ifdef ASE_BENT_NORMAL
                    GetNormalWS( fragInputs, surfaceDescription.BentNormal, bentNormalWS, doubleSidedConstants );
				#endif

				#ifdef _MATERIAL_FEATURE_ANISOTROPY
                    surfaceData.tangentWS = TransformTangentToWorld(surfaceDescription.Tangent, fragInputs.tangentToWorld);
				#endif

				#if defined(DEBUG_DISPLAY)
				    #if !defined(SHADER_STAGE_RAY_TRACING)
				    if (_DebugMipMapMode != DEBUGMIPMAPMODE_NONE)
				    {
					   #ifdef FRAG_INPUTS_USE_TEXCOORD0
                           surfaceData.baseColor = GET_TEXTURE_STREAMING_DEBUG(posInput.positionSS, fragInputs.texCoord0);
                       #else
                           surfaceData.baseColor = GET_TEXTURE_STREAMING_DEBUG_NO_UV(posInput.positionSS);
                       #endif
					   surfaceData.metallic = 0;
				    }
					#endif
				    ApplyDebugToSurfaceData(fragInputs.tangentToWorld, surfaceData); 
				#endif

                #if defined(_SPECULAR_OCCLUSION_CUSTOM)
                #elif defined(_SPECULAR_OCCLUSION_FROM_AO_BENT_NORMAL)
                    surfaceData.specularOcclusion = GetSpecularOcclusionFromBentAO(V, bentNormalWS, surfaceData.normalWS, surfaceData.ambientOcclusion, PerceptualSmoothnessToPerceptualRoughness(surfaceData.perceptualSmoothness));
                #elif defined(_AMBIENT_OCCLUSION) && defined(_SPECULAR_OCCLUSION_FROM_AO)
                    surfaceData.specularOcclusion = GetSpecularOcclusionFromAmbientOcclusion(ClampNdotV(dot(surfaceData.normalWS, V)), surfaceData.ambientOcclusion, PerceptualSmoothnessToRoughness(surfaceData.perceptualSmoothness));
                #endif

                #ifdef _ENABLE_GEOMETRIC_SPECULAR_AA
                    surfaceData.perceptualSmoothness = GeometricNormalFiltering(surfaceData.perceptualSmoothness, fragInputs.tangentToWorld[2], surfaceDescription.SpecularAAScreenSpaceVariance, surfaceDescription.SpecularAAThreshold);
                #endif
			}

			// Get Surface And BuiltinData
			void GetSurfaceAndBuiltinData(GlobalSurfaceDescription surfaceDescription, FragInputs fragInputs, float3 V, inout PositionInputs posInput, out SurfaceData surfaceData, out BuiltinData builtinData)
			{
				#ifdef LOD_FADE_CROSSFADE
                    LODDitheringTransition(ComputeFadeMaskSeed(V, posInput.positionSS), unity_LODFade.x);
				#endif

                #ifdef _DOUBLESIDED_ON
                    float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
                #else
                    float3 doubleSidedConstants = float3(1.0, 1.0, 1.0);
                #endif  
                ApplyDoubleSidedFlipOrMirror(fragInputs, doubleSidedConstants);

				#ifdef _ALPHATEST_ON
                    DoAlphaTest( surfaceDescription.Alpha, surfaceDescription.AlphaClipThreshold );
				#endif

				#ifdef _ALPHATEST_SHADOW_ON
                    DoAlphaTest( surfaceDescription.Alpha, surfaceDescription.AlphaClipThresholdShadow);
				#endif

				#ifdef _DEPTHOFFSET_ON
                    ApplyDepthOffsetPositionInput(V, surfaceDescription.DepthOffset, GetViewForwardDir(), GetWorldToHClipMatrix(), posInput);
				#endif

				float3 bentNormalWS;
                BuildSurfaceData(fragInputs, surfaceDescription, V, posInput, surfaceData, bentNormalWS);
                InitBuiltinData(posInput, surfaceDescription.Alpha, bentNormalWS, -fragInputs.tangentToWorld[2], fragInputs.texCoord1, fragInputs.texCoord2, builtinData);

				#ifdef _DEPTHOFFSET_ON
                    builtinData.depthOffset = surfaceDescription.DepthOffset;
				#endif

                #ifdef _ALPHATEST_ON
                    builtinData.alphaClipTreshold = surfaceDescription.AlphaClipThreshold;
                #endif

                #ifdef UNITY_VIRTUAL_TEXTURING
                    builtinData.vtPackedFeedback = surfaceDescription.VTPackedFeedback;
                #endif

				#ifdef ASE_BAKEDGI
                    builtinData.bakeDiffuseLighting = surfaceDescription.BakedGI;
				#endif

				#ifdef ASE_BAKEDBACKGI
                    builtinData.backBakeDiffuseLighting = surfaceDescription.BakedBackGI;
				#endif

                builtinData.emissiveColor = surfaceDescription.Emission;

				PostInitBuiltinData(V, posInput, surfaceData, builtinData);
			}

			AttributesMesh ApplyMeshModification(AttributesMesh inputMesh, float3 timeParameters, inout PackedVaryingsMeshToPS output/*ase_vert_input*/ )
			{
				_TimeParameters.xyz = timeParameters;
				/*ase_vert_code:inputMesh=AttributesMesh;output=PackedVaryingsMeshToPS*/

				#ifdef ASE_ABSOLUTE_VERTEX_POS
				float3 defaultVertexValue = inputMesh.positionOS.xyz;
				#else
				float3 defaultVertexValue = float3( 0, 0, 0 );
				#endif
				float3 vertexValue = /*ase_vert_out:Vertex Offset;Float3;11;-1;_VertexOffset*/defaultVertexValue/*end*/;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
				inputMesh.positionOS.xyz = vertexValue;
				#else
				inputMesh.positionOS.xyz += vertexValue;
				#endif
				inputMesh.normalOS = /*ase_vert_out:Vertex Normal;Float3;12;-1;_VertexNormal*/inputMesh.normalOS/*end*/;
				inputMesh.tangentOS = /*ase_vert_out:Vertex Tangent;Float4;27;-1;_VertexTangent*/inputMesh.tangentOS/*end*/;
				return inputMesh;
			}

			PackedVaryingsMeshToPS VertexFunction(AttributesMesh inputMesh)
			{
				PackedVaryingsMeshToPS output = (PackedVaryingsMeshToPS)0;
				AttributesMesh defaultMesh = inputMesh;

				UNITY_SETUP_INSTANCE_ID(inputMesh);
				UNITY_TRANSFER_INSTANCE_ID(inputMesh, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( output );

				inputMesh = ApplyMeshModification( inputMesh, _TimeParameters.xyz, output);

				float3 positionRWS = TransformObjectToWorld(inputMesh.positionOS);
				float3 normalWS = TransformObjectToWorldNormal(inputMesh.normalOS);
				float4 tangentWS = float4(TransformObjectToWorldDir(inputMesh.tangentOS.xyz), inputMesh.tangentOS.w);

				#ifdef _WRITE_TRANSPARENT_MOTION_VECTOR
				float4 VPASSpreviousPositionCS;
				float4 VPASSpositionCS = mul(UNITY_MATRIX_UNJITTERED_VP, float4(positionRWS, 1.0));

				bool forceNoMotion = unity_MotionVectorsParams.y == 0.0;
				if (forceNoMotion)
				{
					VPASSpreviousPositionCS = float4(0.0, 0.0, 0.0, 1.0);
				}
				else
				{
					bool hasDeformation = unity_MotionVectorsParams.x > 0.0;
					float3 effectivePositionOS = (hasDeformation ? inputMesh.previousPositionOS : defaultMesh.positionOS);
					#if defined(_ADD_PRECOMPUTED_VELOCITY)
					effectivePositionOS -= inputMesh.precomputedVelocity;
					#endif

					#if defined(HAVE_MESH_MODIFICATION)
						AttributesMesh previousMesh = defaultMesh;
						previousMesh.positionOS = effectivePositionOS ;
						PackedVaryingsMeshToPS test = (PackedVaryingsMeshToPS)0;
						float3 curTime = _TimeParameters.xyz;
						previousMesh = ApplyMeshModification(previousMesh, _LastTimeParameters.xyz, test);
						_TimeParameters.xyz = curTime;
						float3 previousPositionRWS = TransformPreviousObjectToWorld(previousMesh.positionOS);
					#else
						float3 previousPositionRWS = TransformPreviousObjectToWorld(effectivePositionOS);
					#endif

					#ifdef ATTRIBUTES_NEED_NORMAL
						float3 normalWS = TransformPreviousObjectToWorldNormal(defaultMesh.normalOS);
					#else
						float3 normalWS = float3(0.0, 0.0, 0.0);
					#endif

					#if defined(HAVE_VERTEX_MODIFICATION)
						ApplyVertexModification(inputMesh, normalWS, previousPositionRWS, _LastTimeParameters.xyz);
					#endif

					VPASSpreviousPositionCS = mul(UNITY_MATRIX_PREV_VP, float4(previousPositionRWS, 1.0));
				}
				#endif

				output.positionCS = TransformWorldToHClip(positionRWS);
				output.clipPosV = output.positionCS;
				output.positionRWS = positionRWS;
				output.normalWS = normalWS;
				output.tangentWS = tangentWS;
				output.uv1 = inputMesh.uv1;
				output.uv2 = inputMesh.uv2;

				#ifdef _WRITE_TRANSPARENT_MOTION_VECTOR
					output.vpassPositionCS = float3(VPASSpositionCS.xyw);
					output.vpassPreviousPositionCS = float3(VPASSpreviousPositionCS.xyw);
				#endif
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float3 positionOS : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				float4 tangentOS : TANGENT;
				float4 uv1 : TEXCOORD1;
				float4 uv2 : TEXCOORD2;
				/*ase_vcontrol*/
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl Vert ( AttributesMesh v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.positionOS = v.positionOS;
				o.normalOS = v.normalOS;
				o.tangentOS = v.tangentOS;
				o.uv1 = v.uv1;
				o.uv2 = v.uv2;
				/*ase_control_code:v=AttributesMesh;o=VertexControl*/
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = /*ase_inline_begin*/_TessValue/*ase_inline_end*/; float tessMin = /*ase_inline_begin*/_TessMin/*ase_inline_end*/; float tessMax = /*ase_inline_begin*/_TessMax/*ase_inline_end*/;
				float edgeLength = /*ase_inline_begin*/_TessEdgeLength/*ase_inline_end*/; float tessMaxDisp = /*ase_inline_begin*/_TessMaxDisp/*ase_inline_end*/;
				#if (SHADEROPTIONS_CAMERA_RELATIVE_RENDERING != 0)
				float3 cameraPos = 0;
				#else
				float3 cameraPos = _WorldSpaceCameraPos;
				#endif
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(float4(v[0].positionOS,1), float4(v[1].positionOS,1), float4(v[2].positionOS,1), tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), cameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(float4(v[0].positionOS,1), float4(v[1].positionOS,1), float4(v[2].positionOS,1), edgeLength, GetObjectToWorldMatrix(), cameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(float4(v[0].positionOS,1), float4(v[1].positionOS,1), float4(v[2].positionOS,1), edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), cameraPos, _ScreenParams, _FrustumPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			PackedVaryingsMeshToPS DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				AttributesMesh o = (AttributesMesh) 0;
				o.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				o.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				o.tangentOS = patch[0].tangentOS * bary.x + patch[1].tangentOS * bary.y + patch[2].tangentOS * bary.z;
				o.uv1 = patch[0].uv1 * bary.x + patch[1].uv1 * bary.y + patch[2].uv1 * bary.z;
				o.uv2 = patch[0].uv2 * bary.x + patch[1].uv2 * bary.y + patch[2].uv2 * bary.z;
				/*ase_domain_code:patch=VertexControl;o=AttributesMesh;bary=SV_DomainLocation*/
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.positionOS.xyz - patch[i].normalOS * (dot(o.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = /*ase_inline_begin*/_TessPhongStrength/*ase_inline_end*/;
				o.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			PackedVaryingsMeshToPS Vert ( AttributesMesh v )
			{
				return VertexFunction( v );
			}
			#endif

			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Debug/DebugDisplayMaterial.hlsl"

            #if defined(_TRANSPARENT_REFRACTIVE_SORT) || defined(_ENABLE_FOG_ON_TRANSPARENT)
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Water/Shaders/UnderWaterUtilities.hlsl"
            #endif

            #ifdef UNITY_VIRTUAL_TEXTURING
                #ifdef OUTPUT_SPLIT_LIGHTING
                   #define DIFFUSE_LIGHTING_TARGET SV_Target2
                   #define SSS_BUFFER_TARGET SV_Target3
                #elif defined(_WRITE_TRANSPARENT_MOTION_VECTOR)
                   #define MOTION_VECTOR_TARGET SV_Target2
                    #ifdef _TRANSPARENT_REFRACTIVE_SORT
                        #define BEFORE_REFRACTION_TARGET SV_Target3
                        #define BEFORE_REFRACTION_ALPHA_TARGET SV_Target4
                #endif
            	#endif
            #if defined(SHADER_API_PSSL)
            	#pragma PSSL_target_output_format(target 1 FMT_32_ABGR)
            #endif
            #else
                #ifdef OUTPUT_SPLIT_LIGHTING
                #define DIFFUSE_LIGHTING_TARGET SV_Target1
                #define SSS_BUFFER_TARGET SV_Target2
                #elif defined(_WRITE_TRANSPARENT_MOTION_VECTOR)
                #define MOTION_VECTOR_TARGET SV_Target1
                #ifdef _TRANSPARENT_REFRACTIVE_SORT
                     #define BEFORE_REFRACTION_TARGET SV_Target2
                     #define BEFORE_REFRACTION_ALPHA_TARGET SV_Target3
                #endif
            #endif
            #endif

			void Frag(PackedVaryingsMeshToPS packedInput
				, out float4 outColor:SV_Target0
            #ifdef UNITY_VIRTUAL_TEXTURING
				, out float4 outVTFeedback : SV_Target1
            #endif
            #ifdef OUTPUT_SPLIT_LIGHTING
				, out float4 outDiffuseLighting : DIFFUSE_LIGHTING_TARGET
				, OUTPUT_SSSBUFFER(outSSSBuffer) : SSS_BUFFER_TARGET
            #elif defined(_WRITE_TRANSPARENT_MOTION_VECTOR)
				, out float4 outMotionVec : MOTION_VECTOR_TARGET
                #ifdef _TRANSPARENT_REFRACTIVE_SORT
                , out float4 outBeforeRefractionColor : BEFORE_REFRACTION_TARGET
                , out float4 outBeforeRefractionAlpha : BEFORE_REFRACTION_ALPHA_TARGET
                #endif
            #endif
            #ifdef _DEPTHOFFSET_ON
				, out float outputDepth : DEPTH_OFFSET_SEMANTIC
            #endif
		    /*ase_frag_input*/
						)
			{
				#ifdef _WRITE_TRANSPARENT_MOTION_VECTOR
					outMotionVec = float4(2.0, 0.0, 0.0, 1.0);
				#endif

				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( packedInput );
				UNITY_SETUP_INSTANCE_ID( packedInput );
		
				/*ase_local_var:rwp*/float3 PositionRWS = packedInput.positionRWS;
				/*ase_local_var:wvd*/float3 V = GetWorldSpaceNormalizeViewDir( packedInput.positionRWS );
				/*ase_local_var:wn*/float3 NormalWS = packedInput.normalWS;
				/*ase_local_var:wt*/float4 TangentWS = packedInput.tangentWS;
				/*ase_local_var:sp*/float4 ClipPos = packedInput.clipPosV;
				/*ase_local_var:spu*/float4 ScreenPos = ComputeScreenPos( packedInput.clipPosV, _ProjectionParams.x );

				FragInputs input;
				ZERO_INITIALIZE(FragInputs, input);
				input.positionSS = packedInput.positionCS;
				input.positionRWS = PositionRWS;
				input.tangentToWorld = BuildTangentToWorld(TangentWS, NormalWS);
				input.texCoord1 = packedInput.uv1.xyzw;
				input.texCoord2 = packedInput.uv2.xyzw;

				#if _DOUBLESIDED_ON && SHADER_STAGE_FRAGMENT
				input.isFrontFace = IS_FRONT_VFACE( packedInput.cullFace, true, false);
				#elif SHADER_STAGE_FRAGMENT
				#if defined(ASE_NEED_CULLFACE)
				input.isFrontFace = IS_FRONT_VFACE(packedInput.cullFace, true, false);
				#endif
				#endif
				/*ase_local_var:vf*/half IsFrontFace = input.isFrontFace;

				AdjustFragInputsToOffScreenRendering(input, _OffScreenRendering > 0, _OffScreenDownsampleFactor);
				uint2 tileIndex = uint2(input.positionSS.xy) / GetTileSize ();

				PositionInputs posInput = GetPositionInput( input.positionSS.xy, _ScreenSize.zw, input.positionSS.z, input.positionSS.w, input.positionRWS.xyz, tileIndex );

				GlobalSurfaceDescription surfaceDescription = (GlobalSurfaceDescription)0;
				/*ase_frag_code:packedInput=PackedVaryingsMeshToPS*/
				surfaceDescription.BaseColor = /*ase_frag_out:Base Color;Float3;0;-1;_BaseColor*/float3( 0.5, 0.5, 0.5 )/*end*/;
				surfaceDescription.Normal = /*ase_frag_out:Normal;Float3;1;-1;_Normal*/float3( 0, 0, 1 )/*end*/;
				surfaceDescription.BentNormal = /*ase_frag_out:Bent Normal;Float3;2;-1;_BentNormal*/float3( 0, 0, 1 )/*end*/;
				surfaceDescription.CoatMask = /*ase_frag_out:Coat Mask;Float;3;-1;_CoatMask*/0/*end*/;
				surfaceDescription.Metallic = /*ase_frag_out:Metallic;Float;4;-1;_Metallic*/0/*end*/;

				#ifdef _MATERIAL_FEATURE_SPECULAR_COLOR
				surfaceDescription.Specular = /*ase_frag_out:Specular;Float3;5;-1;_Specular*/0/*end*/;
				#endif

				surfaceDescription.Emission = /*ase_frag_out:Emission;Float3;6;-1;_Emission*/0/*end*/;
				surfaceDescription.Smoothness = /*ase_frag_out:Smoothness;Float;7;-1;_Smoothness*/0.5/*end*/;
				surfaceDescription.Occlusion = /*ase_frag_out:Occlusion;Float;8;-1;_Occlusion*/1/*end*/;
				surfaceDescription.Alpha = /*ase_frag_out:Alpha;Float;9;-1;_Alpha*/1/*end*/;

				#ifdef _ALPHATEST_ON
				surfaceDescription.AlphaClipThreshold = /*ase_frag_out:Alpha Clip Threshold;Float;10;-1;_AlphaClip*/_AlphaCutoff/*end*/;
				#endif

				#ifdef _SPECULAR_OCCLUSION_CUSTOM
				surfaceDescription.SpecularOcclusion = /*ase_frag_out:Specular Occlusion;Float;15;-1;_SpecularOcclusion*/0/*end*/;
				#endif

				#ifdef _ENABLE_GEOMETRIC_SPECULAR_AA
				surfaceDescription.SpecularAAScreenSpaceVariance = /*ase_frag_out:Specular AA Screen Space Variance;Float;13;-1;_SpecularAAScreenSpaceVariance*/0/*end*/;
				surfaceDescription.SpecularAAThreshold = /*ase_frag_out:Specular AA Threshold;Float;14;-1;_SpecularAAThreshold*/0/*end*/;
				#endif

				#if defined(_HAS_REFRACTION) || defined(_MATERIAL_FEATURE_TRANSMISSION)
				surfaceDescription.Thickness = /*ase_frag_out:Thickness;Float;16;-1;_Thickness*/1/*end*/;
				#endif

				#ifdef _HAS_REFRACTION
				surfaceDescription.RefractionIndex = /*ase_frag_out:Refraction Index;Float;17;-1;_RefractionIndex*/1/*end*/;
				surfaceDescription.RefractionColor = /*ase_frag_out:Refraction Color;Float3;18;-1;_RefractionColor*/float3( 1, 1, 1 )/*end*/;
				surfaceDescription.RefractionDistance = /*ase_frag_out:Refraction Distance;Float;19;-1;_RefractionDistance*/0/*end*/;
				#endif

				#ifdef _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
				surfaceDescription.SubsurfaceMask = /*ase_frag_out:Subsurface Mask;Float;20;-1;_SubsurfaceMask*/1/*end*/;
				#endif

				#ifdef _MATERIAL_FEATURE_TRANSMISSION
				surfaceDescription.TransmissionMask = /*ase_frag_out:Transmission Mask;Float;61;-1;_TransmissionMask*/1/*end*/;
				#endif

				#if defined( _MATERIAL_FEATURE_SUBSURFACE_SCATTERING ) || defined( _MATERIAL_FEATURE_TRANSMISSION )
				surfaceDescription.DiffusionProfile = /*ase_frag_out:Diffusion Profile;Float;62;-1;_DiffusionProfile*/0/*end*/;
				#endif

				#ifdef _MATERIAL_FEATURE_ANISOTROPY
				surfaceDescription.Anisotropy = /*ase_frag_out:Anisotropy;Float;22;-1;_Anisotropy*/1/*end*/;
				surfaceDescription.Tangent = /*ase_frag_out:Tangent;Float3;23;-1;_Tangent*/float3( 1, 0, 0 )/*end*/;
				#endif

				#ifdef _MATERIAL_FEATURE_IRIDESCENCE
				surfaceDescription.IridescenceMask = /*ase_frag_out:Iridescence Mask;Float;24;-1;_IridescenceMask*/0/*end*/;
				surfaceDescription.IridescenceThickness = /*ase_frag_out:Iridescence Thickness;Float;25;-1;_IridescenceThickness*/0/*end*/;
				#endif

				#ifdef ASE_BAKEDGI
				surfaceDescription.BakedGI = /*ase_frag_out:Baked GI;Float3;31;-1;_BakedGI*/0/*end*/;
				#endif

				#ifdef ASE_BAKEDBACKGI
				surfaceDescription.BakedBackGI = /*ase_frag_out:Baked Back GI;Float3;32;-1;_BakedBackGI*/0/*end*/;
				#endif

				#ifdef _DEPTHOFFSET_ON
				surfaceDescription.DepthOffset = /*ase_frag_out:Depth Offset;Float;26;-1;_DepthOffset*/0/*end*/;
				#endif

				#ifdef UNITY_VIRTUAL_TEXTURING
				surfaceDescription.VTPackedFeedback = float4(1.0f,1.0f,1.0f,1.0f);
				#endif

				SurfaceData surfaceData;
				BuiltinData builtinData;
				GetSurfaceAndBuiltinData(surfaceDescription,input, V, posInput, surfaceData, builtinData);

				BSDFData bsdfData = ConvertSurfaceDataToBSDFData(input.positionSS.xy, surfaceData);

				PreLightData preLightData = GetPreLightData(V, posInput, bsdfData);

				outColor = float4(0.0, 0.0, 0.0, 0.0);

				#ifdef DEBUG_DISPLAY
				#ifdef OUTPUT_SPLIT_LIGHTING
					outDiffuseLighting = float4(0, 0, 0, 1);
					ENCODE_INTO_SSSBUFFER(surfaceData, posInput.positionSS, outSSSBuffer);
				#endif

			    bool viewMaterial = GetMaterialDebugColor(outColor, input, builtinData, posInput, surfaceData, bsdfData);

				if (!viewMaterial)
				{
					if (_DebugFullScreenMode == FULLSCREENDEBUGMODE_VALIDATE_DIFFUSE_COLOR || _DebugFullScreenMode == FULLSCREENDEBUGMODE_VALIDATE_SPECULAR_COLOR)
					{
						float3 result = float3(0.0, 0.0, 0.0);
						GetPBRValidatorDebug(surfaceData, result);
						outColor = float4(result, 1.0f);
					}
					else if (_DebugFullScreenMode == FULLSCREENDEBUGMODE_TRANSPARENCY_OVERDRAW)
					{
						float4 result = _DebugTransparencyOverdrawWeight * float4(TRANSPARENCY_OVERDRAW_COST, TRANSPARENCY_OVERDRAW_COST, TRANSPARENCY_OVERDRAW_COST, TRANSPARENCY_OVERDRAW_A);
						outColor = result;
					}
					else
                #endif
					{
                #ifdef _SURFACE_TYPE_TRANSPARENT
						uint featureFlags = LIGHT_FEATURE_MASK_FLAGS_TRANSPARENT;
                #else
						uint featureFlags = LIGHT_FEATURE_MASK_FLAGS_OPAQUE;
                #endif
						LightLoopOutput lightLoopOutput;
						LightLoop(V, posInput, preLightData, bsdfData, builtinData, featureFlags, lightLoopOutput);

						// Alias
						float3 diffuseLighting = lightLoopOutput.diffuseLighting;
						float3 specularLighting = lightLoopOutput.specularLighting;

						diffuseLighting *= GetCurrentExposureMultiplier();
						specularLighting *= GetCurrentExposureMultiplier();

                #ifdef OUTPUT_SPLIT_LIGHTING
						if (_EnableSubsurfaceScattering != 0 && ShouldOutputSplitLighting(bsdfData))
						{
							outColor = float4(specularLighting, 1.0);
							outDiffuseLighting = float4(TagLightingForSSS(diffuseLighting), 1.0);
						}
						else
						{
							outColor = float4(diffuseLighting + specularLighting, 1.0);
							outDiffuseLighting = float4(0, 0, 0, 1);
						}
						ENCODE_INTO_SSSBUFFER(surfaceData, posInput.positionSS, outSSSBuffer);
                #else
						outColor = ApplyBlendMode(diffuseLighting, specularLighting, builtinData.opacity);

						#ifdef _ENABLE_FOG_ON_TRANSPARENT
						outColor = EvaluateAtmosphericScattering(posInput, V, outColor);
                        #endif

                        #ifdef _TRANSPARENT_REFRACTIVE_SORT
                        ComputeRefractionSplitColor(posInput, outColor, outBeforeRefractionColor, outBeforeRefractionAlpha);
                        #endif
                #endif

				#ifdef _WRITE_TRANSPARENT_MOTION_VECTOR
						float4 VPASSpositionCS = float4(packedInput.vpassPositionCS.xy, 0.0, packedInput.vpassPositionCS.z);
						float4 VPASSpreviousPositionCS = float4(packedInput.vpassPreviousPositionCS.xy, 0.0, packedInput.vpassPreviousPositionCS.z);
						bool forceNoMotion = any(unity_MotionVectorsParams.yw == 0.0);
                #if defined(HAVE_VFX_MODIFICATION) && !VFX_FEATURE_MOTION_VECTORS
                        forceNoMotion = true;
                #endif
				        if (!forceNoMotion)
						{
							float2 motionVec = CalculateMotionVector(VPASSpositionCS, VPASSpreviousPositionCS);
							EncodeMotionVector(motionVec * 0.5, outMotionVec);
							outMotionVec.zw = 1.0;
						}
				#endif
				}

				#ifdef DEBUG_DISPLAY
				}
				#endif

				#ifdef _DEPTHOFFSET_ON
				outputDepth = posInput.deviceDepth;
				#endif

                #ifdef UNITY_VIRTUAL_TEXTURING
				    float vtAlphaValue = builtinData.opacity;
                    #if defined(HAS_REFRACTION) && HAS_REFRACTION
					vtAlphaValue = 1.0f - bsdfData.transmittanceMask;
                #endif
				outVTFeedback = PackVTFeedbackWithAlpha(builtinData.vtPackedFeedback, input.positionSS.xy, vtAlphaValue);
				outVTFeedback.rgb *= outVTFeedback.a; // premuliplied alpha
                #endif

			}
			ENDHLSL
		}

		/*ase_pass*/
		Pass
        {
			/*ase_hide_pass*/
            Name "ScenePickingPass"
            Tags 
			{ 
				"LightMode" = "Picking" 
			}

            Cull [_CullMode]

            HLSLPROGRAM
			#pragma editor_sync_compilation
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #pragma shader_feature _ _SURFACE_TYPE_TRANSPARENT
            #pragma shader_feature_local _ _TRANSPARENT_WRITES_MOTION_VEC _TRANSPARENT_REFRACTIVE_SORT
            #pragma shader_feature_local_fragment _ _ENABLE_FOG_ON_TRANSPARENT

			#pragma vertex Vert
			#pragma fragment Frag

			#define SHADERPASS SHADERPASS_DEPTH_ONLY
			#define SCENEPICKINGPASS 1

			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT
			#define VARYINGS_NEED_TANGENT_TO_WORLD

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
        	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/GeometricTools.hlsl"
        	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Tessellation.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
            #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"

			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"

            #ifdef RAYTRACING_SHADER_GRAPH_DEFAULT
                #define RAYTRACING_SHADER_GRAPH_HIGH
            #endif

            #ifdef RAYTRACING_SHADER_GRAPH_RAYTRACED
                #define RAYTRACING_SHADER_GRAPH_LOW
            #endif

            #ifndef SHADER_UNLIT
            #if defined(_DOUBLESIDED_ON) && !defined(VARYINGS_NEED_CULLFACE)
                #define VARYINGS_NEED_CULLFACE
            #endif
            #endif

			#if defined(_DOUBLESIDED_ON) && !defined(ASE_NEED_CULLFACE)
			    #define ASE_NEED_CULLFACE 1
			#endif

            #if _MATERIAL_FEATURE_COLORED_TRANSMISSION
            #undef _MATERIAL_FEATURE_CLEAR_COAT
            #endif

		    #if defined(_MATERIAL_FEATURE_SUBSURFACE_SCATTERING) && !defined(_SURFACE_TYPE_TRANSPARENT)
			#define OUTPUT_SPLIT_LIGHTING
		    #endif

            #if (SHADERPASS == SHADERPASS_PATH_TRACING) && !defined(_DOUBLESIDED_ON) && (defined(_REFRACTION_PLANE) || defined(_REFRACTION_SPHERE))
            #undef  _REFRACTION_PLANE
            #undef  _REFRACTION_SPHERE
            #define _REFRACTION_THIN
            #endif

            #if SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_PREPASS
            #if !defined(_DISABLE_SSR_TRANSPARENT) && !defined(SHADER_UNLIT)
                #define WRITE_NORMAL_BUFFER
            #endif
            #endif

            #if SHADERPASS == SHADERPASS_MOTION_VECTORS && defined(WRITE_DECAL_BUFFER_AND_RENDERING_LAYER)
                #define WRITE_DECAL_BUFFER
            #endif

            #ifndef DEBUG_DISPLAY
                #if !defined(_SURFACE_TYPE_TRANSPARENT)
                    #if SHADERPASS == SHADERPASS_FORWARD
                    #define SHADERPASS_FORWARD_BYPASS_ALPHA_TEST
                    #elif SHADERPASS == SHADERPASS_GBUFFER
                    #define SHADERPASS_GBUFFER_BYPASS_ALPHA_TEST
                    #endif
                #endif
            #endif

            #if defined(SHADER_LIT) && !defined(_SURFACE_TYPE_TRANSPARENT)
                #define _DEFERRED_CAPABLE_MATERIAL
            #endif
        
            #if (defined(_TRANSPARENT_WRITES_MOTION_VEC) || defined(_TRANSPARENT_REFRACTIVE_SORT)) && defined(_SURFACE_TYPE_TRANSPARENT)
                #define _WRITE_TRANSPARENT_MOTION_VECTOR
            #endif
	
            CBUFFER_START( UnityPerMaterial )
			float4 _EmissionColor;
			float _AlphaCutoff;
			float _RenderQueueType;
			#ifdef _ADD_PRECOMPUTED_VELOCITY
			    float _AddPrecomputedVelocity;
			#endif
			float _StencilRef;
			float _StencilWriteMask;
			float _StencilRefDepth;
			float _StencilWriteMaskDepth;
			float _StencilRefMV;
			float _StencilWriteMaskMV;
			float _StencilRefDistortionVec;
			float _StencilWriteMaskDistortionVec;
			float _StencilWriteMaskGBuffer;
			float _StencilRefGBuffer;
			float _ZTestGBuffer;
			float _RequireSplitLighting;
			float _ReceivesSSR;
			float _SurfaceType;
			float _BlendMode;
            #ifdef SUPPORT_BLENDMODE_PRESERVE_SPECULAR_LIGHTING
			    float _EnableBlendModePreserveSpecularLighting;
            #endif
			float _SrcBlend;
			float _DstBlend;
			float _DstBlend2;
			float _AlphaSrcBlend;
			float _AlphaDstBlend;
			float _ZWrite;
			float _TransparentZWrite;
			float _CullMode;
			float _TransparentSortPriority;
			float _EnableFogOnTransparent;
			float _CullModeForward;
			float _TransparentCullMode;
			float _ZTestDepthEqualForOpaque;
			float _ZTestTransparent;
			float _TransparentBackfaceEnable;
			float _AlphaCutoffEnable;
			float _UseShadowThreshold;
			float _DoubleSidedEnable;
			float _DoubleSidedNormalMode;
			float4 _DoubleSidedConstants;
			#ifdef ASE_TESSELLATION
			    float _TessPhongStrength;
			    float _TessValue;
			    float _TessMin;
			    float _TessMax;
			    float _TessEdgeLength;
			    float _TessMaxDisp;
			#endif
			UNITY_TEXTURE_STREAMING_DEBUG_VARS;
			CBUFFER_END

            #ifdef SCENEPICKINGPASS
            float4 _SelectionID;
            #endif

            #ifdef SCENESELECTIONPASS
            int _ObjectId;
            int _PassValue;
            #endif

			/*ase_globals*/

            #ifdef DEBUG_DISPLAY
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Debug/DebugDisplay.hlsl"
            #endif

            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/PickingSpaceTransforms.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/NormalSurfaceGradient.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/BuiltinUtilities.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialUtilities.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphFunctions.hlsl"

			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/DecalUtilities.hlsl"

			/*ase_pragma*/

			struct AttributesMesh
			{
				float3 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 tangentOS : TANGENT;
				float4 uv1 : TEXCOORD1;
				float4 uv2 : TEXCOORD2;
				float3 previousPositionOS : TEXCOORD4;
				float3 precomputedVelocity : TEXCOORD5;
				/*ase_vdata:p=p;n=n;t=t;uv1=tc1;uv2=tc2;uv4=tc4;uv5=tc5*/
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryingsMeshToPS
			{
				SV_POSITION_QUALIFIERS float4 positionCS : SV_Position;
				float3 positionRWS : TEXCOORD0;
				float3 normalWS : TEXCOORD1;
				float4 tangentWS : TEXCOORD2;
				float4 uv1 : TEXCOORD3;
				float4 uv2 : TEXCOORD4;
				#ifdef _WRITE_TRANSPARENT_MOTION_VECTOR
					float3 vpassPositionCS : TEXCOORD5;
					float3 vpassPreviousPositionCS : TEXCOORD6;
				#endif
				/*ase_interp(7,):sp=sp.xyzw;rwp=tc0;wn=tc1;wt=tc2;uv1=tc3;uv2=tc4*/
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
				#if defined(SHADER_STAGE_FRAGMENT) && defined(ASE_NEED_CULLFACE)
				FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
				#endif
			};

			/*ase_funcs*/

			// Get Surface And BuiltinData
			void GetSurfaceAndBuiltinData(PickingSurfaceDescription surfaceDescription, FragInputs fragInputs, float3 V, inout PositionInputs posInput, out SurfaceData surfaceData, out BuiltinData builtinData)
			{
				#ifdef LOD_FADE_CROSSFADE
                    LODDitheringTransition(ComputeFadeMaskSeed(V, posInput.positionSS), unity_LODFade.x);
				#endif

                #ifdef _DOUBLESIDED_ON
                    float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
                #else
                    float3 doubleSidedConstants = float3(1.0, 1.0, 1.0);
                #endif  
                ApplyDoubleSidedFlipOrMirror(fragInputs, doubleSidedConstants);

                #ifdef DEBUG_DISPLAY
                if (_DebugMipMapMode != DEBUGMIPMAPMODE_NONE)
                {
                    surfaceDescription.Alpha = 1.0f;
                }
                #endif

				#ifdef _ALPHATEST_ON
                    DoAlphaTest( surfaceDescription.Alpha, surfaceDescription.AlphaClipThreshold );
				#endif

				#ifdef _ALPHATEST_SHADOW_ON
                    DoAlphaTest( surfaceDescription.Alpha, surfaceDescription.AlphaClipThresholdShadow);
				#endif

				#ifdef _DEPTHOFFSET_ON
                    ApplyDepthOffsetPositionInput(V, surfaceDescription.DepthOffset, GetViewForwardDir(), GetWorldToHClipMatrix(), posInput);
				#endif

				float3 bentNormalWS;
                //BuildSurfaceData(fragInputs, surfaceDescription, V, posInput, surfaceData, bentNormalWS);
                InitBuiltinData(posInput, surfaceDescription.Alpha, bentNormalWS, -fragInputs.tangentToWorld[2], fragInputs.texCoord1, fragInputs.texCoord2, builtinData);

				#ifdef _DEPTHOFFSET_ON
                    builtinData.depthOffset = surfaceDescription.DepthOffset;
				#endif

                #ifdef _ALPHATEST_ON
                    builtinData.alphaClipTreshold = surfaceDescription.AlphaClipThreshold;
                #endif

                #ifdef UNITY_VIRTUAL_TEXTURING
                    builtinData.vtPackedFeedback = surfaceDescription.VTPackedFeedback;
                #endif

				#ifdef ASE_BAKEDGI
                    builtinData.bakeDiffuseLighting = surfaceDescription.BakedGI;
				#endif

				#ifdef ASE_BAKEDBACKGI
                    builtinData.backBakeDiffuseLighting = surfaceDescription.BakedBackGI;
				#endif

                builtinData.emissiveColor = surfaceDescription.Emission;

				PostInitBuiltinData(V, posInput, surfaceData, builtinData);

            }

			AttributesMesh ApplyMeshModification(AttributesMesh inputMesh, float3 timeParameters, inout PackedVaryingsMeshToPS output/*ase_vert_input*/ )
			{
				_TimeParameters.xyz = timeParameters;
				/*ase_vert_code:inputMesh=AttributesMesh;output=PackedVaryingsMeshToPS*/

				#ifdef ASE_ABSOLUTE_VERTEX_POS
				float3 defaultVertexValue = inputMesh.positionOS.xyz;
				#else
				float3 defaultVertexValue = float3( 0, 0, 0 );
				#endif
				float3 vertexValue =  /*ase_vert_out:Vertex Offset;Float3;2;-1;_VertexOffset*/ defaultVertexValue /*end*/;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
				inputMesh.positionOS.xyz = vertexValue;
				#else
				inputMesh.positionOS.xyz += vertexValue;
				#endif
				inputMesh.normalOS = /*ase_vert_out:Vertex Normal;Float3;3;-1;_VertexNormal*/ inputMesh.normalOS /*end*/;
				inputMesh.tangentOS = /*ase_vert_out:Vertex Tangent;Float4;27;-1;_VertexTangent*/inputMesh.tangentOS/*end*/;
				return inputMesh;
			}

			PackedVaryingsMeshToPS VertexFunction(AttributesMesh inputMesh)
			{
				PackedVaryingsMeshToPS output = (PackedVaryingsMeshToPS)0;
				AttributesMesh defaultMesh = inputMesh;

				UNITY_SETUP_INSTANCE_ID(inputMesh);
				UNITY_TRANSFER_INSTANCE_ID(inputMesh, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( output );

				inputMesh = ApplyMeshModification( inputMesh, _TimeParameters.xyz, output);

				float3 positionRWS = TransformObjectToWorld(inputMesh.positionOS);
				float3 normalWS = TransformObjectToWorldNormal(inputMesh.normalOS);
				float4 tangentWS = float4(TransformObjectToWorldDir(inputMesh.tangentOS.xyz), inputMesh.tangentOS.w);

				#ifdef _WRITE_TRANSPARENT_MOTION_VECTOR
				float4 VPASSpreviousPositionCS;
				float4 VPASSpositionCS = mul(UNITY_MATRIX_UNJITTERED_VP, float4(positionRWS, 1.0));

				bool forceNoMotion = unity_MotionVectorsParams.y == 0.0;
				if (forceNoMotion)
				{
					VPASSpreviousPositionCS = float4(0.0, 0.0, 0.0, 1.0);
				}
				else
				{
					bool hasDeformation = unity_MotionVectorsParams.x > 0.0;
					float3 effectivePositionOS = (hasDeformation ? inputMesh.previousPositionOS : defaultMesh.positionOS);
					#if defined(_ADD_PRECOMPUTED_VELOCITY)
					effectivePositionOS -= inputMesh.precomputedVelocity;
					#endif

					#if defined(HAVE_MESH_MODIFICATION)
						AttributesMesh previousMesh = defaultMesh;
						previousMesh.positionOS = effectivePositionOS ;
						PackedVaryingsMeshToPS test = (PackedVaryingsMeshToPS)0;
						float3 curTime = _TimeParameters.xyz;
						previousMesh = ApplyMeshModification(previousMesh, _LastTimeParameters.xyz, test);
						_TimeParameters.xyz = curTime;
						float3 previousPositionRWS = TransformPreviousObjectToWorld(previousMesh.positionOS);
					#else
						float3 previousPositionRWS = TransformPreviousObjectToWorld(effectivePositionOS);
					#endif

					#ifdef ATTRIBUTES_NEED_NORMAL
						float3 normalWS = TransformPreviousObjectToWorldNormal(defaultMesh.normalOS);
					#else
						float3 normalWS = float3(0.0, 0.0, 0.0);
					#endif

					#if defined(HAVE_VERTEX_MODIFICATION)
						ApplyVertexModification(inputMesh, normalWS, previousPositionRWS, _LastTimeParameters.xyz);
					#endif

					VPASSpreviousPositionCS = mul(UNITY_MATRIX_PREV_VP, float4(previousPositionRWS, 1.0));
				}
				#endif

				output.positionCS = TransformWorldToHClip(positionRWS);
				output.positionRWS = positionRWS;
				output.normalWS = normalWS;
				output.tangentWS = tangentWS;
				output.uv1 = inputMesh.uv1;
				output.uv2 = inputMesh.uv2;

				#ifdef _WRITE_TRANSPARENT_MOTION_VECTOR
					output.vpassPositionCS = float3(VPASSpositionCS.xyw);
					output.vpassPreviousPositionCS = float3(VPASSpreviousPositionCS.xyw);
				#endif
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float3 positionOS : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				float4 tangentOS : TANGENT;
				/*ase_vcontrol*/
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl Vert ( AttributesMesh v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.positionOS = v.positionOS;
				o.normalOS = v.normalOS;
				o.tangentOS = v.tangentOS;
				/*ase_control_code:v=AttributesMesh;o=VertexControl*/
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = /*ase_inline_begin*/_TessValue/*ase_inline_end*/; float tessMin = /*ase_inline_begin*/_TessMin/*ase_inline_end*/; float tessMax = /*ase_inline_begin*/_TessMax/*ase_inline_end*/;
				float edgeLength = /*ase_inline_begin*/_TessEdgeLength/*ase_inline_end*/; float tessMaxDisp = /*ase_inline_begin*/_TessMaxDisp/*ase_inline_end*/;
				#if (SHADEROPTIONS_CAMERA_RELATIVE_RENDERING != 0)
				float3 cameraPos = 0;
				#else
				float3 cameraPos = _WorldSpaceCameraPos;
				#endif
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(float4(v[0].positionOS,1), float4(v[1].positionOS,1), float4(v[2].positionOS,1), tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), cameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(float4(v[0].positionOS,1), float4(v[1].positionOS,1), float4(v[2].positionOS,1), edgeLength, GetObjectToWorldMatrix(), cameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(float4(v[0].positionOS,1), float4(v[1].positionOS,1), float4(v[2].positionOS,1), edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), cameraPos, _ScreenParams, _FrustumPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			PackedVaryingsMeshToPS DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				AttributesMesh o = (AttributesMesh) 0;
				o.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				o.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				o.tangentOS = patch[0].tangentOS * bary.x + patch[1].tangentOS * bary.y + patch[2].tangentOS * bary.z;
				/*ase_domain_code:patch=VertexControl;o=AttributesMesh;bary=SV_DomainLocation*/
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.positionOS.xyz - patch[i].normalOS * (dot(o.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = /*ase_inline_begin*/_TessPhongStrength/*ase_inline_end*/;
				o.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			PackedVaryingsMeshToPS Vert ( AttributesMesh v )
			{
				return VertexFunction( v );
			}
			#endif

			#if defined(WRITE_NORMAL_BUFFER) && defined(WRITE_MSAA_DEPTH)
			#define SV_TARGET_DECAL SV_Target2
			#elif defined(WRITE_NORMAL_BUFFER) || defined(WRITE_MSAA_DEPTH)
			#define SV_TARGET_DECAL SV_Target1
			#else
			#define SV_TARGET_DECAL SV_Target0
			#endif

			void Frag( PackedVaryingsMeshToPS packedInput
						#if defined(SCENESELECTIONPASS) || defined(SCENEPICKINGPASS)
						, out float4 outColor : SV_Target0
						#else
							#ifdef WRITE_MSAA_DEPTH
							, out float4 depthColor : SV_Target0
								#ifdef WRITE_NORMAL_BUFFER
								, out float4 outNormalBuffer : SV_Target1
								#endif
							#else
								#ifdef WRITE_NORMAL_BUFFER
								, out float4 outNormalBuffer : SV_Target0
								#endif
							#endif

							#if (defined(WRITE_DECAL_BUFFER) && !defined(_DISABLE_DECALS)) || defined(WRITE_RENDERING_LAYER)
							, out float4 outDecalBuffer : SV_TARGET_DECAL
							#endif
						#endif

						#if defined(_DEPTHOFFSET_ON) && !defined(SCENEPICKINGPASS)
						, out float outputDepth : DEPTH_OFFSET_SEMANTIC
						#endif
						/*ase_frag_input*/
					)
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(packedInput);
				UNITY_SETUP_INSTANCE_ID(packedInput);

				/*ase_local_var:rwp*/float3 positionRWS = packedInput.positionRWS;
				/*ase_local_var:wvd*/float3 V = GetWorldSpaceNormalizeViewDir( packedInput.positionRWS );

				FragInputs input;
				ZERO_INITIALIZE(FragInputs, input);
				input.tangentToWorld = k_identity3x3;
				input.positionSS = packedInput.positionCS;
				input.positionRWS = positionRWS;

				#if _DOUBLESIDED_ON && SHADER_STAGE_FRAGMENT
				input.isFrontFace = IS_FRONT_VFACE( packedInput.cullFace, true, false);
				#elif SHADER_STAGE_FRAGMENT
				#if defined(ASE_NEED_CULLFACE)
				input.isFrontFace = IS_FRONT_VFACE( packedInput.cullFace, true, false );
				#endif
				#endif
				/*ase_local_var:vf*/half isFrontFace = input.isFrontFace;

				PositionInputs posInput = GetPositionInput(input.positionSS.xy, _ScreenSize.zw, input.positionSS.z, input.positionSS.w, input.positionRWS);

				PickingSurfaceDescription surfaceDescription = (PickingSurfaceDescription)0;
				/*ase_frag_code:packedInput=PackedVaryingsMeshToPS*/
				surfaceDescription.Alpha = /*ase_frag_out:Alpha;Float;0;-1;_Alpha*/1/*end*/;

				#ifdef _ALPHATEST_ON
				surfaceDescription.AlphaClipThreshold =  /*ase_frag_out:Alpha Clip Threshold;Float;1;-1;_AlphaClip*/_AlphaCutoff/*end*/;
				#endif

				outColor = _SelectionID;
			}

            ENDHLSL
		}

        Pass
        {

            Name "FullScreenDebug"
            Tags 
			{ 
				"LightMode" = "FullScreenDebug" 
            }

            Cull [_CullMode]
			ZTest LEqual
			ZWrite Off

            HLSLPROGRAM
            #pragma multi_compile _ DOTS_INSTANCING_ON

			#pragma vertex Vert
			#pragma fragment Frag

            #pragma shader_feature _ _SURFACE_TYPE_TRANSPARENT
            #pragma shader_feature_local _ _TRANSPARENT_WRITES_MOTION_VEC _TRANSPARENT_REFRACTIVE_SORT
            #pragma shader_feature_local_fragment _ _ENABLE_FOG_ON_TRANSPARENT

			#define SHADERPASS SHADERPASS_FULL_SCREEN_DEBUG

			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/GeometricTools.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Tessellation.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
            #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"

			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"

			#ifndef SHADER_UNLIT
			#if defined(_DOUBLESIDED_ON) && !defined(VARYINGS_NEED_CULLFACE)
			#define VARYINGS_NEED_CULLFACE
			#endif
			#endif

            #if _MATERIAL_FEATURE_COLORED_TRANSMISSION
            #undef _MATERIAL_FEATURE_CLEAR_COAT
            #endif

		    #if defined(_MATERIAL_FEATURE_SUBSURFACE_SCATTERING) && !defined(_SURFACE_TYPE_TRANSPARENT)
		    #define OUTPUT_SPLIT_LIGHTING
		    #endif

            #if (SHADERPASS == SHADERPASS_PATH_TRACING) && !defined(_DOUBLESIDED_ON) && (defined(_REFRACTION_PLANE) || defined(_REFRACTION_SPHERE))
            #undef  _REFRACTION_PLANE
            #undef  _REFRACTION_SPHERE
            #define _REFRACTION_THIN
            #endif

			#if SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_PREPASS
			#if !defined(_DISABLE_SSR_TRANSPARENT) && !defined(SHADER_UNLIT)
				#define WRITE_NORMAL_BUFFER
			#endif
			#endif

            #if SHADERPASS == SHADERPASS_MOTION_VECTORS && defined(WRITE_DECAL_BUFFER_AND_RENDERING_LAYER)
                #define WRITE_DECAL_BUFFER
            #endif

			#ifndef DEBUG_DISPLAY
				#if !defined(_SURFACE_TYPE_TRANSPARENT)
					#if SHADERPASS == SHADERPASS_FORWARD
					#define SHADERPASS_FORWARD_BYPASS_ALPHA_TEST
					#elif SHADERPASS == SHADERPASS_GBUFFER
					#define SHADERPASS_GBUFFER_BYPASS_ALPHA_TEST
					#endif
				#endif
			#endif

            #if defined(SHADER_LIT) && !defined(_SURFACE_TYPE_TRANSPARENT)
                #define _DEFERRED_CAPABLE_MATERIAL
            #endif
        
            #if (defined(_TRANSPARENT_WRITES_MOTION_VEC) || defined(_TRANSPARENT_REFRACTIVE_SORT)) && defined(_SURFACE_TYPE_TRANSPARENT)
                #define _WRITE_TRANSPARENT_MOTION_VECTOR
            #endif

            #ifdef DEBUG_DISPLAY
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Debug/DebugDisplay.hlsl"
            #endif

			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/NormalSurfaceGradient.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/BuiltinUtilities.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialUtilities.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphFunctions.hlsl"

            struct AttributesMesh
			{
				float3 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 tangentOS : TANGENT;
				#if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
					uint instanceID : INSTANCEID_SEMANTIC;
				#endif
			};

			struct VaryingsMeshToPS
			{
				SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
				#if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
					uint instanceID : CUSTOM_INSTANCE_ID;
				#endif
			};

			struct VertexDescriptionInputs
			{
				 float3 ObjectSpaceNormal;
				 float3 ObjectSpaceTangent;
				 float3 ObjectSpacePosition;
			};

			struct SurfaceDescriptionInputs
			{
				 float3 TangentSpaceNormal;
			};

			struct PackedVaryingsMeshToPS
			{
				SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
				#if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
					uint instanceID : CUSTOM_INSTANCE_ID;
				#endif
			};

            PackedVaryingsMeshToPS PackVaryingsMeshToPS (VaryingsMeshToPS input)
			{
				PackedVaryingsMeshToPS output;
				ZERO_INITIALIZE(PackedVaryingsMeshToPS, output);
				output.positionCS = input.positionCS;
				#if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
				output.instanceID = input.instanceID;
				#endif
				return output;
			}

			VaryingsMeshToPS UnpackVaryingsMeshToPS (PackedVaryingsMeshToPS input)
			{
				VaryingsMeshToPS output;
				output.positionCS = input.positionCS;
				#if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
				output.instanceID = input.instanceID;
				#endif
				return output;
			}

            struct VertexDescription
			{
				float3 Position;
				float3 Normal;
				float3 Tangent;
			};

			VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
			{
				VertexDescription description = (VertexDescription)0;
				description.Position = IN.ObjectSpacePosition;
				description.Normal = IN.ObjectSpaceNormal;
				description.Tangent = IN.ObjectSpaceTangent;
				return description;
			}

            struct SurfaceDescription
			{
				float3 BaseColor;
				float3 Emission;
				float Alpha;
				float3 BentNormal;
				float Smoothness;
				float Occlusion;
				float3 NormalTS;
				float Metallic;
			};

			SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
			{
				SurfaceDescription surface = (SurfaceDescription)0;
				surface.BaseColor = IsGammaSpace() ? float3(0.5, 0.5, 0.5) : SRGBToLinear(float3(0.5, 0.5, 0.5));
				surface.Emission = float3(0, 0, 0);
				surface.Alpha = 1;
				surface.BentNormal = IN.TangentSpaceNormal;
				surface.Smoothness = 0.5;
				surface.Occlusion = 1;
				surface.NormalTS = IN.TangentSpaceNormal;
				surface.Metallic = 0;
				return surface;
			}

			VertexDescriptionInputs AttributesMeshToVertexDescriptionInputs(AttributesMesh input)
			{
				VertexDescriptionInputs output;
				ZERO_INITIALIZE(VertexDescriptionInputs, output);

				output.ObjectSpaceNormal =                          input.normalOS;
				output.ObjectSpaceTangent =                         input.tangentOS.xyz;
				output.ObjectSpacePosition =                        input.positionOS;
				#if UNITY_ANY_INSTANCING_ENABLED
                #else
                #endif

				return output;
			}

			AttributesMesh ApplyMeshModification(AttributesMesh input, float3 timeParameters  )
			{
				VertexDescriptionInputs vertexDescriptionInputs = AttributesMeshToVertexDescriptionInputs(input);

				VertexDescription vertexDescription = VertexDescriptionFunction(vertexDescriptionInputs);

				input.positionOS = vertexDescription.Position;
				input.normalOS = vertexDescription.Normal;
				input.tangentOS.xyz = vertexDescription.Tangent;
				return input;
			}

			FragInputs BuildFragInputs(VaryingsMeshToPS input)
			{
				FragInputs output;
				ZERO_INITIALIZE(FragInputs, output);

				output.tangentToWorld = k_identity3x3;
				output.positionSS = input.positionCS; // input.positionCS is SV_Position
				#if UNITY_ANY_INSTANCING_ENABLED
                #else
                #endif

				return output;
			}

			FragInputs UnpackVaryingsMeshToFragInputs(PackedVaryingsMeshToPS input)
			{
				UNITY_SETUP_INSTANCE_ID(input);
				VaryingsMeshToPS unpacked = UnpackVaryingsMeshToPS(input);
				return BuildFragInputs(unpacked);
			}

			#define DEBUG_DISPLAY
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Debug/DebugDisplay.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Debug/FullScreenDebug.hlsl"

			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/VertMesh.hlsl"

			PackedVaryingsType Vert(AttributesMesh inputMesh)
			{
				VaryingsType varyingsType;
				varyingsType.vmesh = VertMesh(inputMesh);
				return PackVaryingsType(varyingsType);
			}

			#if !defined(_DEPTHOFFSET_ON)
			[earlydepthstencil]
			#endif
			void Frag(PackedVaryingsToPS packedInput)
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(packedInput);
				FragInputs input = UnpackVaryingsToFragInputs(packedInput);

				PositionInputs posInput = GetPositionInput(input.positionSS.xy, _ScreenSize.zw, input.positionSS.z, input.positionSS.w, input.positionRWS.xyz);

			#ifdef PLATFORM_SUPPORTS_PRIMITIVE_ID_IN_PIXEL_SHADER
				if (_DebugFullScreenMode == FULLSCREENDEBUGMODE_QUAD_OVERDRAW)
				{
					IncrementQuadOverdrawCounter(posInput.positionSS.xy, input.primitiveID);
				}
			#endif
			}
            ENDHLSL
        }
		/*ase_pass_end*/
	}
	/*ase_lod*/
	CustomEditor "Rendering.HighDefinition.LightingShaderGraphGUI"
}
