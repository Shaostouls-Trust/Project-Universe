//This is only shadow casting shader. It does not actually do anything else
//If you want to change the appearance of lit particles change their shaders 

Shader "Hidden/Ethical Motion/Particles/Lit Alpha Blend Shadow Fallback" {
	Properties {
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Thickness ("Thickness Factor", Range(0.0, 0.2)) = 0.05
		_Cutoff ("Alpha cutoff", Range(0,1) ) = 0.5
	}
	SubShader {
		Tags { "Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="Transparentcutout"}
		Cull Back
		
		CGPROGRAM
		#pragma surface surf Standard alphatest:_Cutoff addshadow novertexlights nolightmap noforwardadd approxview halfasview

		sampler2D _MainTex;
		sampler2D _CameraDepthTexture;
		fixed4 _TintColor;

		struct Input {
			float2 uv_MainTex;
			float4 color : COLOR;
		};

		void surf (Input i, inout SurfaceOutputStandard o) {
			
			half main_tex = tex2D (_MainTex, i.uv_MainTex).a;
			o.Albedo = 0;
			o.Alpha = 2 * main_tex * i.color.a;
		}
		ENDCG
	}
	Fallback "Legacy Shaders/Transparent/Cutout/VertexLit"
}
