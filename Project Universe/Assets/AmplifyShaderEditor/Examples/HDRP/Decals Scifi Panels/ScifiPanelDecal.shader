// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ASESampleShaders/Decals Scifi Panels/Scifi Panel Decal"
{
	Properties
	{
		[NoScaleOffset]_Decal_BaseColor("Decal_BaseColor", 2D) = "white" {}
		[NoScaleOffset]_Decal_Emissive("Decal_Emissive", 2D) = "white" {}
		[NoScaleOffset]_Decal_Normal("Decal_Normal", 2D) = "bump" {}
		[HDR]_EmissionTintIntensity("Emission Tint - Intensity", Color) = (0,0,0,0)
		_Tiling("Tiling", Vector) = (0,0,0,0)
		_Offset("Offset", Vector) = (0,0,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _Decal_Normal;
		uniform float2 _Tiling;
		uniform float2 _Offset;
		uniform sampler2D _Decal_BaseColor;
		uniform sampler2D _Decal_Emissive;
		uniform float4 _EmissionTintIntensity;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_TexCoord13 = i.uv_texcoord * _Tiling + _Offset;
			o.Normal = UnpackNormal( tex2D( _Decal_Normal, uv_TexCoord13 ) );
			o.Albedo = tex2D( _Decal_BaseColor, uv_TexCoord13 ).rgb;
			float clampResult29 = clamp( _SinTime.w , 0.2 , 1.0 );
			o.Emission = ( ( tex2D( _Decal_Emissive, uv_TexCoord13 ) * _EmissionTintIntensity ) * clampResult29 ).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18921
300;114.4;862.4;439;1047.182;413.6928;2.321824;False;False
Node;AmplifyShaderEditor.CommentaryNode;53;-1418.008,-198.9024;Inherit;False;590.942;364.8;Tile and Offset adjusted manually for each decal.;3;16;15;13;;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector2Node;16;-1361.315,1.897572;Inherit;False;Property;_Offset;Offset;6;0;Create;True;0;0;0;False;0;False;0,0;1.67,1.33;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;15;-1368.008,-148.9024;Inherit;False;Property;_Tiling;Tiling;5;0;Create;True;0;0;0;False;0;False;0,0;0.33,0.33;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;13;-1069.066,-74.82935;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;11;-613.8002,501.1999;Inherit;False;Property;_EmissionTintIntensity;Emission Tint - Intensity;4;1;[HDR];Create;True;0;0;0;False;0;False;0,0,0,0;120.3367,20.44959,0.5676273,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SinTimeNode;52;-416.2425,719.7072;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;7;-632.5001,269.5001;Inherit;True;Property;_Decal_Emissive;Decal_Emissive;1;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;a973f709b92aa76488eb46bdd48fffe4;a973f709b92aa76488eb46bdd48fffe4;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;29;-189.8269,557.7254;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0.2;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;12;-220.4,311.4999;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;25;89.97389,312.4143;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;9;-606.2,-210.3;Inherit;True;Property;_Decal_Normal;Decal_Normal;3;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;9d098687fb5c01c48bdf5f4cde2f405f;9d098687fb5c01c48bdf5f4cde2f405f;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;8;-617.7997,8.400007;Inherit;True;Property;_Decal_MaskMap;Decal_MaskMap;2;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;6;-602.1999,-456.3;Inherit;True;Property;_Decal_BaseColor;Decal_BaseColor;0;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;a494d020f4020ab4b8ae2aca06660431;a494d020f4020ab4b8ae2aca06660431;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;54;732.1061,-282.2749;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;ASESampleShaders/Decals Scifi Panels/Scifi Panel Decal;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;13;0;15;0
WireConnection;13;1;16;0
WireConnection;7;1;13;0
WireConnection;29;0;52;4
WireConnection;12;0;7;0
WireConnection;12;1;11;0
WireConnection;25;0;12;0
WireConnection;25;1;29;0
WireConnection;9;1;13;0
WireConnection;8;1;13;0
WireConnection;6;1;13;0
WireConnection;54;0;6;0
WireConnection;54;1;9;0
WireConnection;54;2;25;0
ASEEND*/
//CHKSM=6D36E4FE104980AAEE318CEC6394FA66A149EE6A