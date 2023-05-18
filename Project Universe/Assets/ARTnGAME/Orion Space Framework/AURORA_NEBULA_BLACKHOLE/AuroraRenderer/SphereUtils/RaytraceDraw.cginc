#include "UnityCG.cginc"

struct appdata
{
    float4 vertex : POSITION;
};

struct v2f
{
    float4 vertex : SV_POSITION; // clip position
    float3 vertobj : TEXCOORD0; // object coordinates vertex position
    float3 camobj : TEXCOORD1; // object coordinates camera position
};


v2f raytrace_vert (appdata v)
{
    v2f o;
    o.vertobj = v.vertex.xyz;
    o.camobj = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos,1.0f)).xyz;
    o.vertex = UnityObjectToClipPos(v.vertex);
    return o;
}

float4 trace_ray(ray r);

uniform float _PlanetRadius;
fixed4 raytrace_frag (v2f varyings) : SV_Target
{
    // Set up global coordinate system
    float3 geometry_target = varyings.vertobj/_PlanetRadius;
    float3 camera_start = varyings.camobj/_PlanetRadius; 
    float3 look_dir = normalize(geometry_target-camera_start);
    
    ray r; r.S=camera_start; r.D=look_dir;
    
    return trace_ray(r);
}

