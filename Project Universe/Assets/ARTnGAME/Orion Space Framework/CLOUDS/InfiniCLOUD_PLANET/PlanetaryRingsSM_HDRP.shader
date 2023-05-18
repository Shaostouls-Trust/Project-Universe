Shader "ORION/PlanetaryRingsSM_HDRP"
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

			_SunStrength("Sun Strength", float) = 1
			_SunPower("Sun Power", float) = 1
			ringsPos("rings Position", Vector) = (0,0,0,1)

    }
    SubShader
    {
        Tags { "RenderType" = "Transparent"  "IgnoreProjector" = "true" "Queue" = "Transparent"} //"RenderType"="Opaque" }
        LOD 200
		CULL OFF

			 Pass {
			Blend SrcAlpha OneMinusSrcAlpha

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		//#pragma surface surf Standard fullforwardshadows
		//#pragma surface surf StandardDefaultGI keepalpha addshadow alpha:blend  //fullforwardshadows  alpha:fade
			 #pragma vertex vert
				#pragma fragment frag
		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		//v0.1
		//#pragma surface surf StandardDefaultGI

			#include "UnityCG.cginc"
			////HDRP
			#include "Lighting.cginc"
			#include "AutoLight.cginc"


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
			inout UnityGI gi
		)
		{
			LightingStandard_GI(s, data, gi);
		}


	sampler2D _MainTex;
	sampler2D _DensityMap;

	float _SunStrength;
	float _SunPower;
	float4 ringsPos;
/*

	struct Input
	{
		float2 uv_MainTex;
		float3 worldPos;
	};*/

	struct v2f
	{
		float2 uv : TEXCOORD0;
		fixed4 diff : COLOR0;
		float4 vertex : SV_POSITION;
		//float3 lightdir : TEXCOORD1;
		float3 viewdir : TEXCOORD1;
		float3 normal: NORMAL;
		float3 worldNormal: TEXCOORD2; 
		float3 worldPos: TEXCOORD3;
	};

	v2f vert(appdata_base v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.texcoord;
		half3 worldNormal = UnityObjectToWorldNormal(v.normal);
		half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
		o.diff = nl * _LightColor0;

		// the only difference from previous shader:
		// in addition to the diffuse lighting from the main light,
		// add illumination from ambient or light probes
		// ShadeSH9 function from UnityCG.cginc evaluates it,
		// using world space normal
		o.diff.rgb += ShadeSH9(half4(worldNormal, 1));

		//
		o.normal = v.normal;
		o.worldNormal = UnityObjectToWorldNormal(v.normal);
		float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
		float3 viewDir = normalize(worldPos - _WorldSpaceCameraPos.xyz);
		o.viewdir = viewDir;
		o.worldPos = worldPos;

		return o;
	}

	// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
	// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
	// #pragma instancing_options assumeuniformscaling
	//UNITY_INSTANCING_BUFFER_START(Props)
		// put more per-instance properties here
	//UNITY_INSTANCING_BUFFER_END(Props)

		//v0.1
		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		float _MinimumRenderDistance;
		float _MaximumFadeDistance;
		float _InnerRingDiameter;
		float _LightWidth;
		float _LightScale;

		fixed4 frag(v2f IN) : SV_Target//void surf(Input IN, inout SurfaceOutputStandard o)
		{
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv) * _Color;

			float distance = length(_WorldSpaceCameraPos - IN.worldPos);
			float2 position = float2((0.5 - IN.uv.x) * 2, (0.5 - IN.uv.y) * 2);
			float ringDistanceFromCenter = sqrt(position.x * position.x + position.y * position.y);

			clip(ringDistanceFromCenter - _InnerRingDiameter);
			clip(1 - ringDistanceFromCenter);
			clip(distance - _MinimumRenderDistance);

			float opacity = clamp((distance - _MinimumRenderDistance) / (_MaximumFadeDistance - _MinimumRenderDistance), 0 , 1);

			float4 density = tex2D(_DensityMap, float2(clamp((ringDistanceFromCenter - _InnerRingDiameter) / (1 - _InnerRingDiameter), 0, 1), 0.5));
			float3 color = float3(position.x, position.y, density.a);
	//		o.Albedo = color;///saturate(color);// c.rgb;


			// Metallic and smoothness come from slider variables
	//		o.Metallic = _Metallic * opacity;;
	//		o.Smoothness = _Glossiness * opacity;
	//		o.Alpha = opacity * density.a;// c.a;

			//CALC OCCLUSION
			/*float3 lightToPoint = IN.worldPos - _WorldSpaceLightPos0.xyz  ;
			float3 lightToObject = float3(0, 0, 0) -_WorldSpaceLightPos0 ;*/
			float3 lightToPoint = _WorldSpaceLightPos0.xyz - IN.worldPos;// +ringsPos;
			float3 lightToObject = _WorldSpaceLightPos0 - ringsPos ;

			lightToPoint = normalize(lightToPoint);
			lightToObject = normalize(lightToObject);
		//	o.Occlusion = clamp((-dot(lightToPoint, lightToObject) + _LightWidth)* _LightScale,0,1);



			//LIGHTING
			float3 lightDir = normalize(_WorldSpaceLightPos0);
			float3 diffuse = saturate(dot(IN.worldNormal, -lightDir));
			diffuse = color.rgb * diffuse  * _SunStrength;// *_LightColor0;
			//diffuse = saturate(pow(diffuse, _SunPower))*_SunPower + diffuse * 1;

			// Specular here
			float3 specular = 0;
			if (diffuse.x > 0) {
				float3 reflection = reflect(lightDir, IN.normal);
				float3 viewDir = normalize(IN.viewdir);

				specular = saturate(dot(reflection, -viewDir));
				specular = pow(specular, 20.0f);

				//float4 specularIntensity = tex2D(_SpecularMap, IN.uv);
				//specular *= _LightColor0 * 1;
			}

			//diffuse.rgb *= clamp((-dot(lightToPoint, lightToObject) + _LightWidth)* _LightScale, 0, 1);
			float3 outFinal = color.rgb*c + diffuse + specular;
			outFinal = outFinal*0.5 + outFinal* clamp((-dot(lightToPoint, lightToObject) + _LightWidth)* _LightScale, 0,1);
			//o.Albedo = color * clamp((-dot(lightToPoint, lightToObject) + _LightWidth)* _LightScale, 0, 1);
			//return float4(clamp((-dot(lightToPoint, lightToObject) + _LightWidth)* _LightScale, 0, 1)*c.rgb, opacity * density.a);
			return float4(outFinal, opacity * density.a);
			//return float4(color.rgb*c + diffuse + specular, opacity * density.a);

		}
		ENDCG
			}
    }
	//FallBack "Transparent/Cutout/Diffuse" //  "Transparent/VertexLit" //"Diffuse"
}

//FROM TUTORIAL VIDEO - https://www.youtube.com/watch?v=gULMIk3zr4o

