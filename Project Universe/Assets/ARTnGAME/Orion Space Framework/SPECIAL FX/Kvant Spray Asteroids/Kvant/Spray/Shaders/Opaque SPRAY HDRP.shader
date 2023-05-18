//
// Opaque surface shader for Spray
//
// Vertex format:
// position.xyz = vertex position
// texcoord0.xy = uv for texturing
// texcoord1.xy = uv for position/rotation buffer
//
// Position buffer format:
// .xyz = particle position
// .w   = life (+0.5 -> -0.5)
//
// Rotation buffer format:
// .xyzw = particle rotation
//
Shader "ORION/Kvant/Spray/Opaque PBR HDRP"
{
    Properties
    {
        _PositionBuffer ("-", 2D) = "black"{}
        _RotationBuffer ("-", 2D) = "red"{}

        [KeywordEnum(Single, Animate, Random)]
        _ColorMode ("-", Float) = 0
        _Color     ("-", Color) = (1, 1, 1, 1)
        _Color2    ("-", Color) = (0.5, 0.5, 0.5, 1)

        _Metallic   ("-", Range(0,1)) = 0.5
        _Smoothness ("-", Range(0,1)) = 0.5

        _MainTex      ("-", 2D) = "white"{}
        _NormalMap    ("-", 2D) = "bump"{}
        _NormalScale  ("-", Range(0,2)) = 1
        _OcclusionMap ("-", 2D) = "white"{}
        _OcclusionStr ("-", Range(0,1)) = 1

        [HDR] _Emission ("-", Color) = (0, 0, 0)

        _ScaleMin ("-", Float) = 1
        _ScaleMax ("-", Float) = 1

        _RandomSeed ("-", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
		 Pass {
		CGPROGRAM

		//#pragma surface surf Standard vertex:vert nolightmap addshadow
		#pragma shader_feature _COLORMODE_RANDOM
		#pragma shader_feature _ALBEDOMAP
		#pragma shader_feature _NORMALMAP
		#pragma shader_feature _OCCLUSIONMAP
		#pragma shader_feature _EMISSION
		 #pragma vertex vert
				#pragma fragment frag
		// #pragma surface surf Standard vertex:vert nolightmap addshadow
		 #pragma target 3.0

		#include "UnityCG.cginc"
		////HDRP
		#include "Lighting.cginc"
		#include "AutoLight.cginc"

		#include "Common.cginc"

		half _Metallic;
		half _Smoothness;

		sampler2D _MainTex;
		sampler2D _NormalMap;
		half _NormalScale;
		sampler2D _OcclusionMap;
		half _OcclusionStr;
		half3 _Emission;

		/*struct Input
		{
			float2 uv_MainTex;
			half4 color : COLOR;
		};*/
		struct v2f
		{
			float4 vertex : SV_POSITION;
			float2 uv : TEXCOORD0;
			//fixed4 diff : COLOR0;			
			float3 localNormal: TEXCOORD1;
			//float3 uv_NormalMap: TEXCOORD2;
			float4 color : COLOR;
			float3 tangent: TANGENT;
			float3 normal : NORMAL;			
		};

		v2f vert(appdata_full v)//(inout appdata_full v)
		{
			v2f o;
			float4 uv = float4(v.texcoord1.xy + _BufferOffset, 0, 0);

			float4 p = tex2Dlod(_PositionBuffer, uv);
			float4 r = tex2Dlod(_RotationBuffer, uv);

			float l = p.w + 0.5;
			float s = calc_scale(uv, l);

			v.vertex.xyz = rotate_vector(v.vertex.xyz, r) * s + p.xyz;
			v.normal = rotate_vector(v.normal, r);
		#if _NORMALMAP
			v.tangent.xyz = rotate_vector(v.tangent.xyz, r);
		#endif
			v.color = calc_color(uv, l);

			//
			o.color = v.color;
			o.uv = v.texcoord.xy;
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.normal = v.normal;
			o.tangent = v.tangent;
			return o;
		}

		float4 frag(v2f IN) : SV_Target//void surf(Input IN, inout SurfaceOutputStandard o)
		{
	//	#if _ALBEDOMAP
			half4 c = tex2D(_MainTex, IN.uv);
	//		o.Albedo = IN.color.rgb * c.rgb;
	//	#else
	//		o.Albedo = IN.color.rgb;
	//	#endif

	//	#if _NORMALMAP
			half4 n = tex2D(_NormalMap, IN.uv);
	//		o.Normal = UnpackScaleNormal(n, _NormalScale);
	//	#endif

	//	#if _OCCLUSIONMAP
			half4 occ = tex2D(_OcclusionMap, IN.uv);
	//		o.Occlusion = lerp((half4)1, occ, _OcclusionStr);
	//	#endif

	//	#if _EMISSION
	//		o.Emission = _Emission;
	//	#endif

			//o.Metallic = _Metallic;
			//o.Smoothness = _Smoothness;

			return float4(IN.color.rgb * c.rgb * c.rgb,1);
		}

		ENDCG
			}
    }
    CustomEditor "Kvant.SpraySurfaceMaterialEditor"
}
