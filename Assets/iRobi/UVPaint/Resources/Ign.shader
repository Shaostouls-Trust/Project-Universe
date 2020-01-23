
Shader "Hidden/iRobi/Ignore Projectors" {
    Properties {
    _MainTex ("Main", 2D) = "white" {}
    }
    SubShader {
    Tags { "RenderType"="Opaque" "IgnoreProjector"="True"}
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
                o.pos = float4(2*v.texcoord.x-5.0f, -2*v.texcoord.y+5.0f, 0, 1.0f);
                return o;
            }
            fixed4 frag(v2f i) : COLOR {
                //clip(- 0.5);
                float3 finalColor = 0;
                return fixed4(finalColor,1);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
