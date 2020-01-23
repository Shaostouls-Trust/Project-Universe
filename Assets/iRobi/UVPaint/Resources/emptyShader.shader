

Shader "iRobi/Empty-Shader" {
    Properties {
    }
    SubShader {
        Pass {
            Name "ForwardBase"
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct v2f {
    float4  pos : SV_POSITION;
};
            v2f vert (appdata_base v) {
                v2f o;
                o.pos = float4(50.0f, 50.0f, 0, 1.0f);
                return o;
            }
            fixed4 frag(v2f i) : COLOR {
                //clip(- 0.5);
                float3 finalColor = 0;
                return fixed4(finalColor,0);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
