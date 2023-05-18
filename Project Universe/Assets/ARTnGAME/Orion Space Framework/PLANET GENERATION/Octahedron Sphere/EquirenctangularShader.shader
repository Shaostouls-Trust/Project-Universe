﻿// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'
//https://forum.unity.com/threads/what-is-wrong-with-unitys-spherical-mapping-how-to-fix-it.321205/
//https://answers.unity.com/questions/755222/how-do-i-fix-texture-seam-from-uv-spherical-mappin.html	
Shader "ORION/Custom/Equirectangular" {
	Properties{
		_Color("Main Color", Color) = (1,1,1,1)
		_MainTex("Diffuse (RGB) Alpha (A)", 2D) = "gray" {}
		_UVs("UVs", Float) = (1,1,0,0)
	}

		SubShader{
			Pass {
				Tags {"LightMode" = "Always"}

				CGPROGRAM
					#pragma vertex vert
					#pragma fragment frag
					#pragma fragmentoption ARB_precision_hint_fastest
					#pragma glsl
					#pragma target 3.0

					#include "UnityCG.cginc"

					float4 _UVs;

					struct appdata {
					   float4 vertex : POSITION;
					   float3 normal : NORMAL;
					};

					struct v2f
					{
						float4    pos : SV_POSITION;
						float3    normal : TEXCOORD0;
					};

					v2f vert(appdata v)
					{
						v2f o;
						o.pos = UnityObjectToClipPos(v.vertex);
						o.normal = v.normal;
						return o;
					}

					sampler2D _MainTex;

					#define PI 3.141592653589793

					inline float2 RadialCoords(float3 a_coords)
					{
						float3 a_coords_n = normalize(a_coords);
						float lon = atan2(a_coords_n.z, a_coords_n.x);
						float lat = acos(a_coords_n.y);
						float2 sphereCoords = float2(lon, lat) * (1.0 / PI);
						return float2(sphereCoords.x * 0.5 + 0.5, 1 - sphereCoords.y);
					}

					float4 frag(v2f IN) : COLOR
					{
						float2 equiUV = RadialCoords(IN.normal);
						equiUV.x = 1 - equiUV.x;
						//return tex2D(_MainTex, equiUV * _UVs.xy + _UVs.zw);
						return tex2Dlod(_MainTex, float4(_UVs.x * equiUV.x + _UVs.z, _UVs.y * equiUV.y + _UVs.w, _UVs.z , _UVs.w)  );
					}
				ENDCG
			}
	}
		FallBack "VertexLit"
}