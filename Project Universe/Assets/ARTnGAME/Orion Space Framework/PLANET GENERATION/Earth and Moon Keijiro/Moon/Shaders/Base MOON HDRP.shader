Shader "ORION/Moon/Base HDRP"
{
    Properties
    {
        _BaseMap ("Base Map", CUBE) = ""{}
        _Saturation ("Saturation", Range(0,2)) = 1

        _NormalMap ("Normal Map", 2D) = ""{}
        _NormalScale ("Normal Scale", Range(0,2)) = 0.5

        _Glossiness ("Smoothness", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

		 Pass {
			CGPROGRAM
				 #pragma vertex vert
				#pragma fragment frag
			// #pragma surface surf Standard vertex:vert nolightmap addshadow
			 #pragma target 3.0

			#include "UnityCG.cginc"
			////HDRP
			#include "Lighting.cginc"
			#include "AutoLight.cginc"

		samplerCUBE _BaseMap;
		half _Saturation;

		sampler2D _NormalMap;
		half _NormalScale;

		half _Glossiness;

		/*struct Input
		{
			float2 uv_NormalMap;
			float3 localNormal;
		};*/

		struct v2f
		{
			float2 uv : TEXCOORD0;
			fixed4 diff : COLOR0;
			float4 vertex : SV_POSITION;
			float3 localNormal: TEXCOORD1;
			//float3 uv_NormalMap: TEXCOORD2;
			float3 normal : NORMAL;
		};

		v2f vert(appdata_base v)//void vert(inout appdata_full v, out Input o)
		{
			v2f o;
			UNITY_INITIALIZE_OUTPUT(v2f, o);
			//o.localNormal = v.normal * float3(-1, 1, 1);
			
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
			o.localNormal = v.normal * float3(-1, 1, 1);
			o.normal = v.normal;
			return o;
		}

		fixed4 frag(v2f IN) : SV_Target//void surf(Input IN, inout SurfaceOutputStandard o)
		{
			half4 base = texCUBE(_BaseMap, IN.normal);
			half4 normal = tex2D(_NormalMap, IN.uv);

			float3 outCol = lerp((float3)Luminance(base), base.rgb, _Saturation);

			/*o.Albedo = lerp((float3)Luminance(base), base.rgb, _Saturation);
			o.Normal = UnpackScaleNormal(normal, _NormalScale);
			o.Smoothness = _Glossiness;

			o.Metallic = 0;
			o.Alpha = 1;*/

			return float4(outCol, 1) * 2 * _NormalScale;

		}

		ENDCG
			}
    }
   // FallBack "Diffuse"
}
