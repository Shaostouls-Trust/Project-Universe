
Shader "iRobi/PaintTwoSides"
{
    Properties
    {
        _Dec ("Decal", 2D) = "gray" { }//TexGen ObjectLinear }
        _Color ("Main Color", Color) = (1,1,1,0.5)
        _UVPosition ("UV Position", Vector) = (0,0,1,1)
//    _MainTex ("Base Texture", 2D) = "white" { }
   // [HideInInspector]
    }
 
    Subshader
    {
        Tags { "RenderType"="Transparent-1" }
        
        
//        Pass {
//            Name "Outline1"
//            Tags {
//            }
//            Cull off
//            
//            CGPROGRAM
//            #pragma vertex vert
//            #pragma fragment frag
//            #include "UnityCG.cginc"
//sampler2D _MainTex;
//            struct VertexInput {
//                float4 vertex : POSITION;
//                float3 normal : NORMAL;
//            };
//            struct VertexOutput {
//                float4 pos : SV_POSITION;
//    float2  uv : TEXCOORD0;
//    float4 scrPos: TEXCOORD1;
//            };
//float4 _MainTex_ST;
//            VertexOutput vert (VertexInput v,appdata_base g) {
//                VertexOutput o;
//                UNITY_INITIALIZE_OUTPUT(VertexOutput, o);
//               if((_ScreenParams.x/_ScreenParams.y)!=1)
//		{o.pos = mul(UNITY_MATRIX_MVP, v.vertex);}
//		else {
//               o.pos =  float4(2*(g.texcoord.x+g.normal.x)-1.0f, -2*(g.texcoord.y+g.normal.z)+1.0f, 0, 1.0f);
//               }
//		o.scrPos = ComputeScreenPos(o.pos);
//                return o;
//            }
//            fixed4 frag(VertexOutput i) : COLOR {
//                float2 wcoord = (i.scrPos.xy/i.scrPos.w);
//    half4 texcol = tex2D (_MainTex, wcoord);
//         
//    return texcol;
//            }
//            ENDCG
//        }
        
        
//        Pass {
//            Name "Outline3"
//            Tags {
//            }
//            Cull off
//            
//            CGPROGRAM
//            #pragma vertex vert
//            #pragma fragment frag
//            #include "UnityCG.cginc"
//sampler2D _MainTex;
//            struct VertexInput {
//                float4 vertex : POSITION;
//                float3 normal : NORMAL;
//            };
//            struct VertexOutput {
//                float4 pos : SV_POSITION;
//    float2  uv : TEXCOORD0;
//    float4 scrPos:TEXCOORD1;
//            };
//float4 _MainTex_ST;
//            VertexOutput vert (VertexInput v,appdata_img g) {
//                VertexOutput o;
//                UNITY_INITIALIZE_OUTPUT(VertexOutput, o);
//               if((_ScreenParams.x/_ScreenParams.y)!=1)
//		{o.pos = mul(UNITY_MATRIX_MVP, v.vertex);}
//		else {
//               o.pos =  float4(2*g.texcoord.x-1.0f, -2*g.texcoord.y+1.0f, 0, 1.0f);
//               float3 norm   = mul ((float3x3)UNITY_MATRIX_IT_MV, v.normal);
//               }
//		o.scrPos = ComputeScreenPos(o.pos);
//                return o;
//            }
//            fixed4 frag(VertexOutput i) : COLOR {
//                float2 wcoord = (i.scrPos.xy/i.scrPos.w);
//    half4 texcol = tex2D (_MainTex, wcoord);
//         
//    return texcol;
//            }
//            ENDCG
//        }
        
        Pass
        {
           ZWrite Off
            Fog { Color (1, 1, 1) }
            AlphaTest Greater 0.05
            ColorMask RGB
             Cull Off
         Blend SrcAlpha OneMinusSrcAlpha
            Offset -1, -1
 
           CGPROGRAM
           #pragma vertex vert
           #pragma fragment frag
           #pragma fragmentoption ARB_fog_exp2
           #pragma fragmentoption ARB_precision_hint_fastest
           #include "UnityCG.cginc"
 
           struct v2f
           {
             float4 pos : SV_POSITION;
             float2 uv_Main : TEXCOORD0;
           };
 
 
           sampler2D _Dec;
           float4 _Color;
           float4x4 _UVPaintShadowProjectionMatrix;
   			sampler2D _FalloffTex;
   			float4 _UVPosition;
   			 
           v2f vert(appdata_tan v)
           {
             v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f, o);
//             if((_ScreenParams.x/_ScreenParams.y)!=1)
//		{
//		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
//		}
//             else {
              float x = v.texcoord.x;
             if(x!=1)
             x = x - floor(x);

             float y = v.texcoord.y;
             if(y!=1)
             y = y - floor(y);
            
             x = 2*x-1;

             #if UNITY_UV_STARTS_AT_TOP
             y = -2*y+1;
             #else
             y = 2*y-1;
             #endif

              o.pos = float4(x, y, 0.0f, 1.0f);
//              }
             o.uv_Main = mul( _UVPaintShadowProjectionMatrix, mul( unity_ObjectToWorld, v.vertex ) );
             return o;
           }
 
           half4 frag (v2f i) : COLOR
           {
    		 half4 falloff = tex2D(_FalloffTex, i.uv_Main);
             half4 tex = tex2D(_Dec, float2( i.uv_Main.x*_UVPosition.z+_UVPosition.x, i.uv_Main.y*_UVPosition.w+_UVPosition.y));
             return half4(tex.rgb*_Color,tex.a*falloff.r);
           }
           ENDCG
 
        }
    
    }
}
 

