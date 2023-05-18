// Don't draw a skybox 
//  No matter what queue you set, it erases everything drawn pre-geometry.

Shader "ORION/LawlorCode/NOP Skybox" {
   Properties {
   }
   SubShader {
      Tags { "RenderType" = "Background"   "Queue" = "Background" }
      
      Pass {   
         ZWrite Off
         ZTest Equal

         CGPROGRAM
 
         #pragma vertex vert  
         #pragma fragment frag 
 
         #include "UnityCG.cginc"
 
         struct vertexInput {
            float4 vertex : POSITION;
            float3 texcoord : TEXCOORD0;
         };
         struct vertexOutput {
            float4 vertex : SV_POSITION;
            float3 texcoord : TEXCOORD0;
         };
         vertexOutput vert(vertexInput input) 
         {
            vertexOutput output;
            output.vertex = UnityObjectToClipPos(input.vertex);
            output.texcoord = input.texcoord;
            return output;
         }
 
         float4 frag(vertexOutput input) : COLOR
         {
            return float4(1.0,0.0,0.0,0.0); // no alpha
         }
 
         ENDCG
      }
   }
}
