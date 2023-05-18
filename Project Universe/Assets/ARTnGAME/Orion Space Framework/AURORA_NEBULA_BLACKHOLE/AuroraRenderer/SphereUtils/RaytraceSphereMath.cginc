
/********* Raytracer Utilities ************/
static const float M_PI=3.1415926535;
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
span make_span(float L,float H) {
  span sp;
  sp.l=L; sp.h=H;
  return sp;
}

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

/* Get u,v texture coordinates from sphere hit point */
float2 sphere_to_UV(float3 hit) {
  float radius2D=length(float2(hit.x,hit.z));
  float latitude=atan2(hit.y,radius2D)*(1.0/M_PI)+0.5; 
  float longitude=frac(atan2(-hit.x,hit.z)*(0.5/M_PI)+0.75);
  float2 uv = float2(longitude,latitude);
  return uv;
}


