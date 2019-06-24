Shader "iRobi/Unlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue"="Geometry"}//"Queue" = "Background" 
		//Tags { "Queue"="Background"}//"Queue" = "Background" 
		Lighting Off
		Cull Back
		//Cull off
		ZWrite Off 
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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                float x = v.uv.x;
				if(x!=1)
				x = x - floor(x);

				float y = v.uv.y;
				if(y!=1)
				y = y - floor(y);
            
				x = 2*x-1;

				#if UNITY_UV_STARTS_AT_TOP
				y = -2*y+1;
				#else
				y = 2*y-1;
				#endif

				o.vertex = float4(x, y, 0, 1);
				//o.vertex = UnityObjectToClipPos(v.vertex);

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
