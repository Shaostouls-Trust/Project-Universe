
Shader "iRobi/RestorePC"
{
    Properties
    {
        _Dec ("Decal", 2D) = "gray" { }
        _Color ("Main Color", Color) = (1,1,1,0.5)
        _DeepAngle ("Deep Angle", Range(0,1)) =0.5
        _UVPosition ("UV Position", Vector) = (0,0,1,1)
//   _FalloffTex ("FallOff", 2D) = "gray" { TexGen ObjectLinear   }
    _MainTex ("Base Texture", 2D) = "white" { }
	_OrigTex ("Original Texture", 2D) = "white" { }
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
//               o.pos =  float4(2*(g.texcoord.x+v.normal.x)-1.0f, -2*(g.texcoord.y+v.normal.z)+1.0f, 0, 1.0f);
//               }
//		o.scrPos = ComputeScreenPos(o.pos);
//		o.pos.xyz -= v.normal*0.03f;
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
//               o.pos = float4(2*g.texcoord.x-1.0f, -2*g.texcoord.y+1.0f, 0.0f, 1.0f);
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

   Lighting Off

   Cull Off

   ZWrite Off
   ZTest Off
   Offset -1, -1

   

   Tags { "Queue" = "Transparent" "RenderType"="Opaque" }//"Queue" = "Transparent"
//   AlphaTest Greater 0.05
//            ColorMask RGB
//   Blend SrcAlpha OneMinusSrcAlpha
//   AlphaToMask True

   CGPROGRAM

   #pragma vertex vert

   #pragma fragment frag

   #pragma fragmentoption ARB_fog_exp2

   #pragma fragmentoption ARB_precision_hint_fastest

//   #pragma glsl_no_auto_normalization

   #include "UnityCG.cginc"
   struct Input

   {
    float4 vertex : POSITION;
    float3 normal : NORMAL;
   };
   struct v2f
   {
    float4 pos : SV_POSITION;
    float2 uv_Main  : TEXCOORD0;
    //float3 normalDir : TEXCOORD1;
			float dpth : TEXCOORD1;
			float4 sh : TEXCOORD2;  
    //float3 viewD : TEXCOORD4;
    float3 DD : TEXCOORD3;
	float2 scrPos: TEXCOORD4;
   };
   sampler2D _Dec;
   float4 _Color;
   //float4x4 unity_Projector;
   float d;
        float _DeepAngle;
   float4 _UVPosition;
   sampler2D _FalloffTex;
   sampler2D _MainTex;
   sampler2D _OrigTex;

		float4x4 _UVPaintShadowProjectionMatrix;

		sampler2D _ShadowMapTex;

   v2f vert(appdata_base g,Input i)
   {
   v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f, o);
//    if((_ScreenParams.x/_ScreenParams.y)!=1)
//		{
//		o.pos = mul(UNITY_MATRIX_MVP, i.vertex);
//		}
//             else {
             float x = g.texcoord.x;
             if(x!=1)
             x = x - floor(x);

             float y = g.texcoord.y;
             if(y!=1)
             y = y - floor(y);
            
             x = 2*x-1;

             #if UNITY_UV_STARTS_AT_TOP
             y = -2*y+1;
             #else
             y = 2*y-1;
             #endif

              o.pos = float4(x, y, 0, 1);

//              #if UNITY_UV_STARTS_AT_TOP
//			o.proj1.y = (o.position.w - o.position.y) * 0.5;
//			#endif

//              }
              //o.normalDir = UnityObjectToWorldNormal(i.normal);
             // i.vertex.xyz-=g.normal*0.1f;
    //o.uv_Main = mul (unity_Projector, i.vertex);
    o.sh = mul( unity_ObjectToWorld, i.vertex );
			o.sh = mul( _UVPaintShadowProjectionMatrix, o.sh );
			o.uv_Main = o.sh;
			o.dpth = ( o.sh.z / o.sh.w );
			//o.viewD= normalize(ObjSpaceViewDir(i.vertex));
			//o.viewD=normalize(-float3(_UVPaintShadowProjectionMatrix[2][0],_UVPaintShadowProjectionMatrix[2][1], _UVPaintShadowProjectionMatrix[2][2]));
			o.DD = clamp(clamp(dot(UnityObjectToWorldNormal(i.normal), normalize(-float3(_UVPaintShadowProjectionMatrix[2][0],_UVPaintShadowProjectionMatrix[2][1], _UVPaintShadowProjectionMatrix[2][2])))+_DeepAngle,-999,1),0,100);
			o.DD=o.DD*o.DD*o.DD;
			o.scrPos = ComputeScreenPos(o.pos);

    return o;
   }


   half sampleShadowmap( float2 uv, float depth )
		{
			half4 enc = tex2D (_ShadowMapTex, uv);
			return 1 - step(enc.r, depth);
		}

   half4 frag (v2f i) : COLOR
   {
    float Depth = sampleShadowmap( i.sh.xy, 1-i.dpth*0.97 );
   //float3 normView = normalize(-float3(unity_Projector[1][0],unity_Projector[1][1], unity_Projector[1][2]));
	//float3 normView = normalize(-float3(_UVPaintShadowProjectionMatrix[2][0],_UVPaintShadowProjectionMatrix[2][1], _UVPaintShadowProjectionMatrix[2][2]));
    half4 falloff = tex2D(_FalloffTex, i.uv_Main);
    //d = clamp(dot(i.normalDir, i.viewD)+0.5,0,100);
	//d=d*d*d;
	d = i.DD;
//    d = sampleShadowmap( i.sh.xy, i.dpth*1.001 );
	float2 wcoord = i.scrPos;
    half4 texcol0 = tex2D (_MainTex, wcoord);
	half4 texcol = tex2D (_OrigTex, wcoord);
    half4 comp = tex2D(_Dec, float2( i.uv_Main.x*_UVPosition.z+_UVPosition.x, i.uv_Main.y*_UVPosition.w+_UVPosition.y))*_Color;
//    return fixed4(comp.rgb,comp.a*min(d + _DeepAngle,1)*falloff.r);
    //return fixed4(comp.rgb,comp.a*(1-Depth)*min(d + _DeepAngle,1)*falloff.r);
    //return fixed4(texcol0.rgb,comp.a*(1-Depth)*(d)*falloff.r);
	//fixed alpha = (1-comp.a*(1-Depth)*(d)*falloff.r);
	fixed alpha = comp.a*(1-Depth)*falloff.r;
	return fixed4(lerp(texcol0.rgb,texcol.rgb,alpha),lerp(texcol0.a,texcol.a,alpha));
    //return fixed4(texcol0.rgb,texcol0.a);
    //return fixed4(comp.rgb,comp.a*falloff.r);
   }

   ENDCG

  }
    
    }
}
 

