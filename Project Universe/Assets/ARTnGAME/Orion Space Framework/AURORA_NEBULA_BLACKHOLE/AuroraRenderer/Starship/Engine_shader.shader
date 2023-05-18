Shader "ORION/Custom/Engine_shader"
{
  Properties
  {
    _Color ("Color", Color) = (1,1,1,1)
    _MainTex ("Albedo (RGB)", 2D) = "white" {}
    _Glossiness ("Smoothness", Range(0,1)) = 0.5
    _Metallic ("Metallic", Range(0,1)) = 0.0
  }
  SubShader
  {
    Tags { "RenderType"="Opaque" }
    LOD 200

    CGPROGRAM
    // Physically based Standard lighting model, and enable shadows on all light types
    #pragma surface surf Standard fullforwardshadows

    // Use shader model 3.0 target, to get nicer looking lighting
    #pragma target 3.0

    sampler2D _MainTex;

    struct Input
    {
      float2 uv_MainTex;
      float3 worldPos;
      float3 worldNormal;
    };

    half _Glossiness;
    half _Metallic;
    fixed4 _Color;
    
    // Global thrust level: 0.0 = engines off.  1.0 = all engines on full.
    uniform float g_Thrust_1;
    uniform float g_Thrust_3;
    uniform float g_Thrust_7;
      
      // Afterglow is the engine bells glowing after running for a bit.
    uniform float g_AfterGlow_1;
    uniform float g_AfterGlow_3;
    uniform float g_AfterGlow_7;

    // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
    // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
    // #pragma instancing_options assumeuniformscaling
    UNITY_INSTANCING_BUFFER_START(Props)
        // put more per-instance properties here
    UNITY_INSTANCING_BUFFER_END(Props)

    // see https://docs.unity3d.com/Manual/SL-SurfaceShaders.html
    void surf (Input IN, inout SurfaceOutputStandard o)
    {
      // Albedo comes from a texture tinted by color
      fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
      o.Albedo = c.rgb;
      // Metallic and smoothness come from slider variables
      o.Metallic = _Metallic;
      o.Smoothness = _Glossiness;
      o.Alpha = c.a;
      
      float4 world=float4(IN.worldPos,1.0f);
      float4 obj=mul(unity_WorldToObject,world);
      obj.z+=15+1.5; // shift Z up to engine bell edge
      float3 worldN=IN.worldNormal;
      float3 objN=normalize(mul((float3x3)unity_WorldToObject,worldN));
      
      float burn=0.0f; // blue-white burning
      float glow=0.0f; // orange-red afterglow
      float raptor_r=2.1f/2.0f; // radius of one raptor (in the model)
      if (abs(obj.x)<raptor_r) { // middle three engines
        if (abs(obj.y)<raptor_r) { // middlemost engine
          glow=g_AfterGlow_1;
          burn=g_Thrust_1;
        } else { // outer two
          glow=g_AfterGlow_3;
          burn=g_Thrust_3;
        }
      }
      else {
        glow=g_AfterGlow_7;
        burn=g_Thrust_7;
      }
      
      glow=clamp(2.0f*glow,0.0f,1.9f);
      glow*=clamp(obj.z/2.0f,0.0f,1.0f); // drop off glow near edge
      float3 emit=float3(0.7f*clamp(glow,0.0f,1.0f), 0.1f*glow,0.05f*glow); // (0.1f+clamp(obj.z/2.0f-1.0f,0.0f,0.5f))*glow, 0.05f*glow);
      
      burn=clamp(burn,0.0f,0.9f);
      if (objN.z>=0.0f) burn=0.0f; // burning only visible on inside surface of bell
      float face=clamp(-objN.z,0.0f,1.0f);
      emit += float3(face*burn,face*burn,burn); //frac(1.0f/2.1f*obj.xyz);
      
      o.Emission = emit;
    }
    ENDCG
  }
  FallBack "Diffuse"
}
