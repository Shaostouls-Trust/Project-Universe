// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ASESampleShaders/Decals Muddy Ground/GroundDecals"
{
	Properties
	{
		_dirt_decal_BaseColor("dirt_decal_BaseColor", 2D) = "white" {}
		_dirt_decal_mask("dirt_decal_mask", 2D) = "white" {}
		_Dirt_Decal_Normal("Dirt_Decal_Normal", 2D) = "bump" {}
		_SmoothnessMultiplier("Smoothness Multiplier", Range( 0 , 1)) = 0
		_NormalIntensity("Normal Intensity", Float) = 0
		_DecalQuantity("Decal Quantity", Float) = 0
		_DecalType("Decal Type", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#include "UnityStandardUtils.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _Dirt_Decal_Normal;
		uniform float _DecalQuantity;
		uniform float _DecalType;
		uniform float _NormalIntensity;
		uniform sampler2D _dirt_decal_BaseColor;
		uniform sampler2D _dirt_decal_mask;
		uniform float _SmoothnessMultiplier;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
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
			half2 fbuv27 = i.uv_texcoord * fbtiling27 + fboffset27;
			// *** END Flipbook UV Animation vars ***
			o.Normal = UnpackScaleNormal( tex2D( _Dirt_Decal_Normal, fbuv27 ), _NormalIntensity );
			o.Albedo = tex2D( _dirt_decal_BaseColor, fbuv27 ).rgb;
			o.Smoothness = ( tex2D( _dirt_decal_mask, fbuv27 ).a * _SmoothnessMultiplier );
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18921
300;114.4;862.4;439;1257.302;362.9594;1.906489;True;False
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
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;31;-13,-105;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;ASESampleShaders/Decals Muddy Ground/GroundDecals;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
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
WireConnection;31;0;17;0
WireConnection;31;1;19;0
WireConnection;31;4;21;0
ASEEND*/
//CHKSM=F161B701D73CC59A59F017AEB3F24C84895EABFE