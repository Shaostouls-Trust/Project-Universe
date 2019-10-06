Shader "Dissolving" {
    Properties {
      _MainTex ("Texture (RGB)", 2D) = "white" {}
      _SliceGuide ("Slice Guide (RGB)", 2D) = "white" {}


 _BurnSize ("Burn Size", Range(0.0, 1.0)) = 0.15
 _BurnRamp ("Burn Ramp (RGB)", 2D) = "white" {}
    }
    SubShader {
      Tags { "RenderType" = "Opaque" }
      CGPROGRAM
      #pragma surface surf Lambert
      struct Input {
          float2 uv_MainTex;
          float2 uv_SliceGuide;
      };


      sampler2D _MainTex;
      sampler2D _SliceGuide;
 sampler2D _BurnRamp;
 float _BurnSize;


      void surf (Input IN, inout SurfaceOutput o) {
//      float time = lerp(0.1,0.2,sin(_Time.g));
      float time = sin(_Time.b*1.5)/2-1;
      float3 ttt = tex2D (_SliceGuide, IN.uv_MainTex).rgb/1.2;
          clip( 1-(ttt - time*ttt) );
          o.Albedo = tex2D (_MainTex, IN.uv_MainTex).rgb;

 ttt = tex2D (_SliceGuide, IN.uv_MainTex).rgb/2;
 half test = 0.5-(ttt - time*ttt) ;//1-ttt - time*ttt.r
 if(test < _BurnSize ){//&& time > 0 && time < 1
    o.Emission = ttt;
// o.Albedo *= o.Emission;
 }
      }
      ENDCG
    } 
    Fallback "Diffuse"
  }