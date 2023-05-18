Shader "ORION/Unlit/AuroraShader"
{
// Aurora borealis / austrialis shader
// Additive blending, so it needs to be the last pass: set Material's Render Queue to 3000.

    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        
        _GlowColor("Glow Color", Color) = (0,1,0,1)
        
        
    }
    SubShader
    {
        Tags { "Queue"="Transparent+100" } // need to render after skybox and clouds
        LOD 100
        
        Blend One OneMinusSrcAlpha // Additive blending
        ZWrite Off // We're transparent
        ZTest Always // We need to draw backside
        Cull Front // Draw back faces only (avoids clipping when camera is inside object)
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

/*
 HLSL fragment shader: raytracer with planetary atmosphere.
 
 The coordinate system here uses units of 1.0 earth radius, so you 
 usually draw the planet with diameter 2.0.
 
 Dr. Orion Sky Lawlor, lawlor@alaska.edu, 2019-01-09 (Public Domain)
*/
static const float M_PI=3.1415926535;

static const float proxy_size = 2.5; // <- size of the proxy geometry, in Earth radii

static const float km=1.0/6371.0; // convert kilometers to render units (planet radii)
static const float dt=1.0/6371.0; /* sampling rate for aurora, in kilometer steps: fine sampling gets *SLOW* */

static const float miss_t=100.0; // t value for a miss
static const float min_t=0.000001; // minimum acceptable t value

/* A 3D ray shooting through space */
struct ray {
	float3 S, D; /* start location (camera / ray origin), and direction (unit length) */
};
float3 ray_at(ray r,float t) { return r.S+r.D*t; }

/* A span of ray t values */
struct span {
	float l,h; /* lowest, and highest t value */
};

/* Everything about a ray-object hit point */
struct ray_intersection {
	float t; // t along the ray
	float3 P; // intersection point, in world coordinates
	float3 N; // surface normal at intersection point
};

struct sphere {
	float3 center;
	float r;
};
sphere make_sphere(float3 c,float r) {
  sphere s;
  s.center=c;
  s.r=r;
  return s;
}

/* Return t value at first intersection of ray and sphere */
float intersect_sphere(sphere s, ray r) {
	float b=2.0*dot(r.S-s.center,r.D), 
		c=dot(r.S-s.center,r.S-s.center)-s.r*s.r;
	float det = b*b-4.0*c;
	if (det<0.0) {return miss_t;} /* total miss */
	float t = (-b - sqrt(det)) *0.5;
	if (t<min_t) {return miss_t;} /* behind head: miss! */
	return t;
}

/* Return span of t values at intersection region of ray and sphere */
span span_sphere(sphere s, ray r) {
	float b=2.0*dot(r.S-s.center,r.D), 
		c=dot(r.S-s.center,r.S-s.center)-s.r*s.r;
	float det = b*b-4.0*c;
	if (det<0.0) {span sp; sp.l=sp.h=miss_t; return sp;} /* total miss */
	float sd=sqrt(det);
	float tL = (-b - sd) *0.5;
	float tH = (-b + sd) *0.5;
	
	span sp;
	sp.l=tL;
	sp.h=tH;
	return sp;
}

float sqr(float x) {return x*x;}


            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION; // clip position
                float3 vertobj : TEXCOORD1; // object coordinates vertex position
                float3 camobj : TEXCOORD2; // object coordinates camera position
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _GlowColor;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertobj = v.vertex.xyz * proxy_size;
                o.camobj = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos,1.0f)).xyz * proxy_size;
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f varyings) : SV_Target
            {
                // Set up global coordinate system
                float3 geometry_target = varyings.vertobj;
                float3 camera_start = varyings.camobj; 
                float3 look_dir = normalize(geometry_target-camera_start);
                
                ray r; r.S=camera_start; r.D=look_dir;
                float planet_t=intersect_sphere(make_sphere(float3(0.0,0.0,0.0),1.0),r);
                
                
                float3 col=float3(0,0,1);
                
                if (planet_t<miss_t) col=float3(frac(ray_at(r,planet_t)));
               
                
                
                // col = float3(frac(10.0*planet_t),frac(planet_t),0); // frac(view), 1.0f); // float4(varyings.uv,0.0f,1.0f);
                //col = frac(start);
                
                
                return float4(col,0.0);
            }
            ENDCG
        }
    }
}
