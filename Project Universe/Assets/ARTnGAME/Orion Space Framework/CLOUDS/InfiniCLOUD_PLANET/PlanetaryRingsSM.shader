Shader "ORION/PlanetaryRingsSM"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
	    _DensityMap("_DensityMap (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

		_MinimumRenderDistance("Minimum Render Distance", Float) = 10
		_MaximumFadeDistance("Maximum Fade Distance", Float) = 20
		_InnerRingDiameter("Inner Ring Diameter", Range(0, 1)) = 0.5
		_LightWidth("Planet Size", Float) = 0.9
		_LightScale("Light Scale", Float) = 5.0

    }
    SubShader
    {
        Tags { "RenderType" = "Transparent"  "IgnoreProjector" = "true" "Queue" = "Transparent"} //"RenderType"="Opaque" }
        LOD 200
		CULL OFF
			Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        //#pragma surface surf Standard fullforwardshadows
		#pragma surface surf StandardDefaultGI keepalpha addshadow alpha:blend  //fullforwardshadows  alpha:fade

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

		//v0.1
		//#pragma surface surf StandardDefaultGI

		#include "UnityPBSLighting.cginc"		

			inline half4 LightingStandardDefaultGI(SurfaceOutputStandard s, half3 viewDir, UnityGI gi)
			{
				//v0.1
				half4 lighting = LightingStandard(s, viewDir, gi);
				lighting.rgb *= s.Occlusion;
				return lighting;
				//return LightingStandard(s, viewDir, gi);
			}
				inline void LightingStandardDefaultGI_GI(
			SurfaceOutputStandard s,
			UnityGIInput data,
			inout UnityGI gi)
		{
			LightingStandard_GI(s, data, gi);
		}


        sampler2D _MainTex;
		sampler2D _DensityMap;


	


        struct Input
        {
            float2 uv_MainTex;
			float3 worldPos;
        };

       

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

		//v0.1
		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		float _MinimumRenderDistance;
		float _MaximumFadeDistance;
		float _InnerRingDiameter;
		float _LightWidth;
		float _LightScale;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            //fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;

			float distance = length(_WorldSpaceCameraPos - IN.worldPos);
			float2 position = float2((0.5 - IN.uv_MainTex.x) * 2, (0.5 - IN.uv_MainTex.y) * 2);
			float ringDistanceFromCenter = sqrt(position.x * position.x + position.y * position.y);

			clip(ringDistanceFromCenter - _InnerRingDiameter);
			clip(1 - ringDistanceFromCenter);
			clip(distance - _MinimumRenderDistance);

			float opacity = clamp((distance - _MinimumRenderDistance) / (_MaximumFadeDistance - _MinimumRenderDistance), 0 , 1);

			float4 density = tex2D(_DensityMap, float2(clamp((ringDistanceFromCenter - _InnerRingDiameter) / (1 - _InnerRingDiameter), 0, 1), 0.5));
			float3 color = float3(position.x, position.y, density.a);
			o.Albedo = color;///saturate(color);// c.rgb;


            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic * opacity;;
            o.Smoothness = _Glossiness * opacity;
			o.Alpha = opacity * density.a;// c.a;

			//CALC OCCLUSION
			float3 lightToPoint = _WorldSpaceLightPos0.xyz - IN.worldPos;
			float3 lightToObject = _WorldSpaceLightPos0 - float3(0, 0, 0);

			lightToPoint = normalize(lightToPoint);
			lightToObject = normalize(lightToObject);
			o.Occlusion = clamp((-dot(lightToPoint, lightToObject)  + _LightWidth)* _LightScale,0,1);

			//o.Albedo = color * clamp((-dot(lightToPoint, lightToObject) + _LightWidth)* _LightScale, 0, 1);
        }
        ENDCG
    }
	FallBack "Transparent/Cutout/Diffuse" //  "Transparent/VertexLit" //"Diffuse"
}

//FROM TUTORIAL VIDEO - https://www.youtube.com/watch?v=gULMIk3zr4o

