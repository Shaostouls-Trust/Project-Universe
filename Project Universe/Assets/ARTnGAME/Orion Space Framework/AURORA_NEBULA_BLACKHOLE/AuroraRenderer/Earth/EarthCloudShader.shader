Shader "ORION/EarthCloudShader"
{
    Properties
    {
        _PlanetRadius ("Radius of rendered planet (parent coords)", Float) = 1.0
        
        _EarthCloud ("Clouds (RGB)", 2D) = "white" {}
        _CloudColor ("Cloud Color", Color) = (1,1,1,1)
        _CloudNightGlow ("Cloud Night Glow", Color) = (0.02,0.05,0.1,1)
    }
    SubShader
    {
        //Tags { "Queue"="Geometry+1" }
        LOD 300
        
        
        Blend One OneMinusSrcAlpha // Ordinary blending
        ZWrite Off // We're transparent
        ZTest Always // We need to draw backside
        //Cull Front // Draw back faces only (avoids clipping when camera is inside proxy)
        
        Pass
        {
        CGPROGRAM
        #include "../SphereUtils/RaytraceSphereMath.cginc"
        #include "../SphereUtils/RaytraceDraw.cginc"
        #pragma vertex raytrace_vert
        #pragma fragment raytrace_frag

        sampler2D _EarthCloud;
        fixed4 _CloudColor;
        fixed4 _CloudNightGlow;

        float4 trace_ray(ray r) {
          float3 color=float3(0.0,0.0,0.0);
          float3 cloud=fixed3(0.0,0.0,0.0);
          float alpha=0.0;
          
          float3 planet_center=float3(0.0,0.0,0.0);
          float planet_radius=0.999;
          float cloud_radius=1.0;
          float hit_t=intersect_sphere(make_sphere(planet_center,cloud_radius),r);
          if (hit_t<miss_t) {
            float3 hit=ray_at(r,hit_t);  
            float2 uv = sphere_to_UV(hit);
            
            cloud = tex2D (_EarthCloud, uv) * _CloudColor;
            alpha = clamp(length(cloud),0.0,1.0);
            
            float3 normal=normalize(hit); // surface normal for sphere is easy (object coords)
            float3 sun_dir = normalize(mul(unity_WorldToObject,float4(+1.0,0.0,0.0,0.0)).xyz);
            
            float lambert = clamp(dot(normal,sun_dir),0.0,1.0)+0.05;
            color += lambert * cloud.rgb;
            color += 0.2 * cloud.rgb * _CloudNightGlow;
            
            // Check if we're the only part of the planet still visible:
            if (intersect_sphere(make_sphere(planet_center,planet_radius),r)>=miss_t) 
            { // clouds are sticking up over edge of planet--set alpha to opaque, so stars extinct properly
              alpha=1.0;
            }
          }
          //else clip(-1.0); // ray misses actual sphere (proxy geometry is upper bound)
          
          
          return float4(color*11,alpha);
        }
        
        ENDCG
        }
    }
    FallBack "Diffuse"
}
