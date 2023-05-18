Shader "ORION/EarthShader"
{
    Properties
    {
        _PlanetRadius ("Radius of rendered planet (parent coords)", Float) = 1.0
        
        _EarthAlbedo ("Albedo (RGB)", 2D) = "white" {}
        _AlbedoColor ("Albedo Color",Color) = (1,1,1,1)
        _EarthBump ("Bump (RGB)", 2D) = "white" {}
        
        _EarthCloud ("Clouds (RGB)", 2D) = "white" {}
        _CloudColor ("Cloud Color", Color) = (1,1,1,1)
        _CloudNightGlow ("Cloud Night Glow", Color) = (0.02,0.05,0.1,1)
        
        _EarthLight ("Night lights (RGB)", 2D) = "black" {}
        _EmitColor ("Night light color",Color) = (1,1,1,1)
        
        _EarthGlint ("Glintmap (RGB)", 2D) = "black" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "Queue"="Geometry" }
        LOD 300
        
        //Blend SrcAlpha OneMinusSrcAlpha // Ordinary blending
       // ZWrite Off // We're proxy geometry
       // ZTest Always // We need to draw backside
        //Cull Front // Draw back faces only (avoids clipping when camera is inside proxy)
        
        Pass
        {
        CGPROGRAM
        #include "../SphereUtils/RaytraceSphereMath.cginc"
        #include "../SphereUtils/RaytraceDraw.cginc"
        #pragma vertex raytrace_vert
        #pragma fragment raytrace_frag

        sampler2D _EarthAlbedo;
        fixed4 _AlbedoColor;
        sampler2D _EarthBump;
        
        sampler2D _EarthLight;
        fixed4 _EmitColor;
        sampler2D _EarthGlint;
        
        sampler2D _EarthCloud;
        fixed4 _CloudColor;
        fixed4 _CloudNightGlow;
        
        float _Glossiness;

        float4 trace_ray(ray r) {
          // return float4(1,0,0,1);
          
          float3 color=float3(0.0,0.0,0.0);
          
          float radius=1.0;
          float hit_t=intersect_sphere(make_sphere(float3(0.0,0.0,0.0),radius),r);
          if (hit_t<miss_t) {
            float3 hit=ray_at(r,hit_t);  
            float2 uv = sphere_to_UV(hit);
            
            float3 albedo = tex2D (_EarthAlbedo, uv) * _AlbedoColor;
            float3 cloud = tex2D (_EarthCloud, uv) * _CloudColor;
            float3 emit = tex2D (_EarthLight, uv) * _EmitColor;
            float3 glint = tex2D (_EarthGlint, uv) * _Glossiness;
            
            // Metallic and smoothness come from slider variables
            float gloss =  length(glint.rgb);
            
            float3 normal=normalize(hit); // surface normal for sphere is easy (object coords)
            float3 sun_dir = normalize(mul(unity_WorldToObject,float4(+1.0,0.0,0.0,0.0)).xyz);
            
            float lambert = dot(normal,sun_dir);
            if (lambert>0) {
              color += lambert * max(0.8*albedo - 0.6*cloud,float3(0.0,0.0,0.0));
              // fixme: gloss
            }
            color += emit;
          }
          else clip(-1.0); // ray misses actual sphere (proxy geometry is upper bound)
          
          
          return float4(color,1.0)*15;
        }
        
        ENDCG
        }
    }
    FallBack "Diffuse"
}
