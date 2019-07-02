Shader "Custom/LIV_Blit"
{
	Properties
	{
		_NearTex("Near Texture", 2D) = "white" {}
		_FarTex("Far Texture", 2D) = "white" {}
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _NearTex;
			sampler2D _FarTex;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv.xy = float2(v.uv.x, (v.uv.y - 0.5) * 2.0);
				o.uv.zw = float2(v.uv.x, (v.uv.y) * 2.0);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = tex2D(_NearTex, i.uv.xy) * step(1.0 - i.uv.y, 1.0);
				col += fixed4(tex2D(_FarTex, i.uv.zw).rgb, 1.0) * step(i.uv.y, 0.0);
				return col;
			}

			ENDCG
		}
	}
}
