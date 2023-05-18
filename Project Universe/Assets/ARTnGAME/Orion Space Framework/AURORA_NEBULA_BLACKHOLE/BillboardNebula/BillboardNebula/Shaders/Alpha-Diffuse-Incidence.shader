Shader "ORION/Transparent/Diffuse Incidence" {
Properties {
	_TintColor ("Main Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
}

SubShader {
	Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
	LOD 200

	/*
	Pass
	{
		Cull Front
		Blend One One
		ZWrite On
		ZTest Greater
	}*/

	/*
    // extra pass that renders to depth buffer only
    Pass
	{
        ZWrite On
        ColorMask 0
    }
	*/

	// paste in forward rendering passes from Transparent/Diffuse
    //UsePass "Transparent/Diffuse/FORWARD"

	CGPROGRAM
	#pragma surface surf Lambert alpha:fade noshadow

	sampler2D _MainTex;
	fixed4 _TintColor;

	struct Input {
		float4 color : COLOR;
		float4 screenPos;
		float2 uv_MainTex;
		float3 viewDir;
	};

	void surf (Input IN, inout SurfaceOutput o) {
	fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _TintColor * IN.color;
	
	o.Albedo = c.rgb * IN.color;
	o.Emission = c.rgb * 0.1;
	//o.Emission = c.rgb * IN.color.rgb;
	
	// Front facing only sided
	float rim = saturate(dot (normalize(IN.viewDir), o.Normal));	

	// Double sided
	//float rim = saturate(dot (normalize(-IN.viewDir), o.Normal)) + saturate(dot (normalize(IN.viewDir), o.Normal));
	
	o.Alpha = rim * rim * c.a;
	//o.Alpha = c.a;
}
ENDCG
}

Fallback "Legacy Shaders/Transparent/VertexLit"
}
