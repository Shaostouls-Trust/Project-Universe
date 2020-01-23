Shader "iRobi/CustomShader"
{
    Properties
    {
        _MainTex ("Texture (RGB)", 2D) = "white" {}
      _SliceGuide ("Slice Guide (RGB)", 2D) = "white" {}


 _BurnSize ("Burn Size", Range(0.0, 1.0)) = 0.15
 _BurnRamp ("Burn Ramp (RGB)", 2D) = "white" {}
    }
    SubShader
    {
        Pass
        {
            // indicate that our pass is the "base" pass in forward
            // rendering pipeline. It gets ambient and main directional
            // light data set up; light direction in _WorldSpaceLightPos0
            // and color in _LightColor0
            Tags {"RenderType"="Opaque" }//"LightMode"="ForwardBase"
        
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc" // for UnityObjectToWorldNormal
            #include "UnityLightingCommon.cginc" // for _LightColor0

            struct v2f
            {
                float2 uv : TEXCOORD0;
                fixed4 diff : COLOR0; // diffuse lighting color
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata_base v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                // get vertex normal in world space
                half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                // dot product between normal and light direction for
                // standard diffuse (Lambert) lighting
                half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
                // factor in the light color
                o.diff = nl * _LightColor0;
				o.diff.rgb += ShadeSH9(half4(worldNormal,1))+0.25;
                return o;
            }
            
            sampler2D _MainTex;
			sampler2D _SliceGuide;
			sampler2D _BurnRamp;
			float _BurnSize;

            fixed4 frag (v2f i) : SV_Target
            {
			float time = sin(_Time.b*1.5)/2-1;
      float3 ttt = tex2D (_SliceGuide, i.uv).rgb/1.2;
          clip( 1-(ttt - time*ttt) );
                // sample texture
                fixed4 col = tex2D(_MainTex, i.uv);

				ttt = tex2D (_SliceGuide, i.uv).rgb/2;
                // multiply by lighting
                col *= i.diff;
				 half test = 0.5-(ttt - time*ttt) ;//1-ttt - time*ttt.r
				 if(test < _BurnSize ){//&& time > 0 && time < 1
				    col.rgb+= ttt.rgb*3;
				 }

                return col;
            }
            ENDCG
        }
    }
}