Shader "ORION/AuroraShader"
{
// Aurora borealis / austrialis shader
// Additive blending, so it needs to be the last pass: set Material's Render Queue to 3000.

    Properties
    {
        _PlanetRadius ("Radius of rendered planet (parent coords)", Float) = 1.0
        
        _AuroraPostColor ("Aurora Color Scaling (RGB)", Color) = (1,1,1,1)
        _AuroraPostGamma ("Final output gamma correction", Range(0.8,3.0)) = 2.2
        
        _AtmosphereColor ("Atmosphere Color (RGB)", Color) = (0.03,0.04,0.06,1)
        _AtmosphereAlpha ("Atmosphere Alpha", Range(0.0,5.0)) = 1.0
        
        _AuroraCurtains ("Aurora Curtain Footprint (RGB)", 2D) = "black" {}
        _AuroraDistance ("Aurora Curtain Distance Field (Grayscale)", 2D) = "white" {}
        _AuroraDeposition ("Aurora Deposition vs Height (RGB)", 2D) = "green" {}
    }
    SubShader
    {
        Tags { "Queue"="Geometry-5" } // need to render after skybox and clouds
        LOD 100
        
        Blend One OneMinusSrcAlpha // Additive blending
        ZWrite Off // We're transparent
       // ZTest Always // We need to draw backside
        //Cull Front // Draw back faces only (avoids clipping when camera is inside proxy)
        
        Pass
        {
            CGPROGRAM
// GPUs without variable loops can't handle the central volume rendering step
#pragma exclude_renderers d3d11_9x
#pragma exclude_renderers d3d9


/*
 HLSL fragment shader: raytracer with planetary atmosphere.
 
 The coordinate system here uses units of 1.0 earth radius, so you 
 normally draw the planet as a sphere with diameter 2.0.
 
 Dr. Orion Sky Lawlor, lawlor@alaska.edu, 2019-01-09 (Public Domain)
*/

sampler2D _AuroraCurtains; // actual curtain footprints
sampler2D _AuroraDistance; // distance from curtains (0==far away, 1==close)
sampler2D _AuroraDeposition; // deposition function, x=intensity, y=height

float _PlanetRadius;
float4 _AtmosphereColor;
float _AtmosphereAlpha;
float4 _AuroraPostColor;
float _AuroraPostGamma;

static const float proxy_size = 1.1; // <- size of the proxy geometry, in Earth radii

static const float km=1.0/6371.0; // convert kilometers to render units (planet radii)
static const float dt=1.0/6371.0; /* sampling rate for aurora, in kilometer steps: fine sampling gets *SLOW* */

#include "../SphereUtils/RaytraceSphereMath.cginc"

/************** Aurora Mechanics *****************/
float sqr(float x) {return x*x;}


/* Return the amount of auroral energy deposited at this height,
   measured in planetary radii. */
float3 deposition_function(float height)
{
	// Fixed intensity version
	//   return float3(1.0); 
	
	height-=1.0; /* convert to altitude (subtract off planet's radius) */

	// Texture lookup version
	float max_height=300.0*km;
	float3 tex=tex2D(_AuroraDeposition, float2(0.4, 1.0-height/max_height)).rgb;
	return tex*tex; // HDR storage
	//return exp(-10.0*tex); // reconstruct from decibels (old way, fine but slow!)
	
/*
// Analytic atmosphere model.  Because the thermosphere is not a uniform temperature or composition, this isn't the same as the low-atmosphere version in the texture.
	float scaleheight=7.3*km; // "scale height," where atmosphere reaches 1/e thickness (planetary radius units)
	float extra_scale=(height-100.0*km)*(7.0*km/(300.0*km));
	// HACK: adjust k to deal with atmospheric nonlinearity, 
	//    and mix of energies.  
	//	The effect of this is to make the final curtains higher
	scaleheight+=max(extra_scale,0.0); 
	
	float k=1.0/scaleheight; // atmosphere density = refDen*exp(-(height-refHt)*k) 
	float refDen=1033.0*(k); // atmosphere density at surface, in grams/square cm (1000*psi) 
	
	float rho=refDen*exp(-(height)*k); // atmosphere density (approx 1/100)
	float R=rho/(k); // weight of atmosphere above this point (==integral rho)
	
	// For monodirectional incoming radiation:
	float C1=2.11685, C2=2.97035, C3=2.09710, C4=0.74054, C5=0.58795, C6=1.72746, C7=1.37459, C8=0.93296;

	float E_max=10.0; // keV
	float y = pow((R/4.5e-6),1.0/1.65)/E_max;
	float constant = 0.1; // scale to rendering units (flux rate, etc)
	float E_z = constant* rho / (2.0*R) * ( C1*pow(y,C2)*exp(-C3*pow(y,C4)) + C5*pow(y,C6)*exp(-C7*pow(y,C8)));

	return float3(E_z);
*/
}

// Apply nonlinear tone mapping to final summed output color
float3 tonemap(float3 color) {
	float len=length(color)+0.000001;
	return color*pow(len,1.0/_AuroraPostGamma-1.0); /* apply gamma */
}

/* Convert a 3D location to a 2D aurora map index (polar stereographic) */
float2 downtomap(float3 worldloc) {
	float3 onplanet=normalize(worldloc);
	float2 mapcoords=onplanet.xy*0.5+float2(0.5,0.5); // on 2D polar stereo map
	return mapcoords;
}

/* Sample the aurora's color at this 3D point */
float3 sample_aurora(float3 loc) {
		/* project sample point to surface of planet, and look up in texture */
		float r=length(loc);
		float3 deposition=deposition_function(r);
		float curtain=tex2D(_AuroraCurtains,downtomap(loc)).g;
		return deposition*curtain;
}

/* Sample the aurora's color along this ray, and return the summed color */
float3 sample_aurora(ray r,span s) {
	if (s.h<0.0) return float3(0.0,0.0,0.0); /* whole span is behind our head */
	if (s.l<0.0) s.l=0.0; /* start sampling at observer's head */
	
	float pathlength=s.h-s.l; // ray span length
	//float3 auroraglow=0.00001*float3(0,1,0); // generalized glow (overall in layer)
	
	float aurorascale=dt/(30.0*km); /* scale factor: samples at dt -> screen color */
	
	/* Sum up aurora light along ray span */
	float3 sum=float3(0.0,0.0,0.0);
	float3 loc=ray_at(r,s.l); /* start point along ray */
	float counthit=0.0;
	float t=s.l;

	[loop] while (t<s.h) {
		float3 loc=ray_at(r,t);
		sum+=sample_aurora(loc); // real curtains
		float dist=(0.997-tex2D(_AuroraDistance,downtomap(loc)).r)*0.10;
		if (dist<dt) dist=dt;
		t+=dist;
		counthit++;
	}
	
	//return 0.01*float3(0.0,counthit,0.0); // hitcolors
	return sum*aurorascale; //  + auroraglow*counthit; // full curtain
}


/************** Atmosphere Integral Approximation **************/
/**
  Decent little Wikipedia/Winitzki 2003 approximation to erf.
  Supposedly accurate to within 0.035% relative error.
*/
static const float a=8.0*(M_PI-3.0)/(3.0*M_PI*(4.0-M_PI));
float erf_guts(float x) {
   float x2=x*x;
   return exp(-x2 * (4.0/M_PI + a*x2) / (1.0+a*x2));
}
// "error function": integral of exp(-x*x)
float win_erf(float x) {
   float sign=1.0;
   if (x<0.0) sign=-1.0;
   return sign*sqrt(1.0-erf_guts(x));
}
// erfc = 1.0-erf, but with less roundoff
float win_erfc(float x) {
   if (x>3.0) { //<- hits zero sig. digits around x==3.9
      // x is big -> erf(x) is very close to +1.0
      // erfc(x)=1-erf(x)=1-sqrt(1-e)=approx +e/2
      return 0.5*erf_guts(x);
   } else {
      return 1.0-win_erf(x);
   }
}


/**
   Compute the atmosphere's integrated thickness along this ray.
   The planet is assumed to be centered at origin, with unit radius.
   This is an exponential approximation:
*/
float atmosphere_thickness(float3 start,float3 dir,float tstart,float tend) {
	float scaleheight=8.0*km; /* "scale height," where atmosphere reaches 1/e thickness (planetary radius units) */
	float k=1.0/scaleheight; /* atmosphere density = refDen*exp(-(height-refHt)*k) */
	float refHt=1.0; /* height where density==refDen */
	float refDen=100.0; /* atmosphere opacity per planetary radius */
	/* density=refDen*exp(-(height-refHt)*k) */
	float norm=sqrt(M_PI)/2.0; /* normalization constant */
	
// Step 1: planarize problem from 3D to 2D
	// integral is along ray: tstart to tend along start + t*dir
	float a=dot(dir,dir),b=2.0*dot(dir,start),c=dot(start,start);
	float tc=-b/(2.0*a); //t value at ray/origin closest approach
	float y=sqrt(tc*tc*a+tc*b+c);
	float xL=tstart-tc;
	float xR=tend-tc;
	// integral is along line: from xL to xR at given y
	// x==0 is point of closest approach

// Step 2: Find first matching radius r1-- smallest used radius
	float ySqr=y*y, xLSqr=xL*xL, xRSqr=xR*xR;
	float r1Sqr,r1;
	float isCross=0.0;
	if (xL*xR<0.0) //Span crosses origin-- use radius of closest approach
	{
		r1Sqr=ySqr;
		r1=y;
		isCross=1.0;
	}
	else
	{ //Use either left or right endpoint-- whichever is closer to surface
		r1Sqr=xLSqr+ySqr;
		if (r1Sqr>xRSqr+ySqr) r1Sqr=xRSqr+ySqr;
		r1=sqrt(r1Sqr);
	}
	
// Step 3: Find second matching radius r2
	float del=2.0/k;//This distance is 80% of atmosphere (at any height)
	float r2=r1+del; 
	float r2Sqr=r2*r2;
	//float r3=r1+2.0*del; // <- third radius not needed if we assume B==0
	//float r3Sqr=r3*r3;
	
// Step 4: Find parameters for parabolic approximation to true hyperbolic distance
	// r(x)=sqrt(y^2+x^2), r'(x)=A+Cx^2; r1=r1', r2=r2'
	float x1Sqr=r1Sqr-ySqr; // rSqr = xSqr + ySqr, so xSqr = rSqr - ySqr
	float x2Sqr=r2Sqr-ySqr;
	
	float C=(r1-r2)/(x1Sqr-x2Sqr);
	float A=r1-x1Sqr*C-refHt;
	
// Step 5: Compute the integral of exp(-k*(A+Cx^2)) from x==xL to x==xR
	float sqrtKC=sqrt(k*C); // variable change: z=sqrt(k*C)*x; exp(-z^2)
	float erfDel;
	if (isCross>0.0) { //xL and xR have opposite signs-- use erf normally
		erfDel=win_erf(sqrtKC*xR)-win_erf(sqrtKC*xL);
	} else { //xL and xR have same sign-- flip to positive half and use erfc
		if (xL<0.0) {xL=-xL; xR=-xR;}
		erfDel=win_erfc(sqrtKC*xR)-win_erfc(sqrtKC*xL);
		//if (xL>0.0) {xL=-xL; xR=-xR;} // flip to negative half (more roundoff)
		//erfDel=win_erf(sqrtKC*xR)-win_erf(sqrtKC*xL);
	}
	if (abs(erfDel)>1.0e-10) /* parabolic approximation has acceptable roundoff */
	{
		float eScl=exp(-k*A); // from constant term of integral
		return refDen*norm*eScl/sqrtKC*abs(erfDel);
	} 
	else { /* erfDel==0.0 -> Roundoff!  Switch to a linear approximation:
		a.) Create linear approximation r(x) = M*x+B
		b.) integrate exp(-k*(M*x+B-1.0)) dx, from xL to xR
		   integral = (1.0/(-k*M)) * exp(-k*(M*x+B-1.0))
		*/
		float x1=sqrt(x1Sqr), x2=sqrt(x2Sqr); 
		float M=(r2-r1)/(x2-x1); /* linear fit at (x1,r1) and (x2,r2) */
		float B=r1-M*x1-1.0;
		
		float t1=exp(-k*(M*xL+B));
		float t2=exp(-k*(M*xR+B));
		return abs(refDen*(t2-t1)/(k*M));
	}
}


/*******************************************************************
  Final overall raytrace and composite for aurora and atmosphere
*/
float4 aurora_raytrace(ray r) 
{

	// Compute intersection ranges with all our geometry
	span auroraL=span_sphere(make_sphere(float3(0.0,0.0,0.0),85.0*km+1.0),r);
	span auroraH=span_sphere(make_sphere(float3(0.0,0.0,0.0),300.0*km+1.0),r);

	// Planet itself
	float planet_t=intersect_sphere(make_sphere(float3(0.0,0.0,0.0),1.0),r);
	float3 planet=float3(0.0,0.0,0.0);

	// Atmosphere
	span airspan=span_sphere(make_sphere(float3(0.0,0.0,0.0),75.0*km+1.0),r);
	if (airspan.h>planet_t) {airspan.h=planet_t;} // looking down at planet
	if (airspan.l<0.0) {airspan.l=0.0;} // looking up
	float3 airColor = _AtmosphereColor.xyz;
	float airMass=0.0;
	if (airspan.h<miss_t && airspan.h > 0.0) {
		airMass=atmosphere_thickness(r.S,r.D,airspan.l,airspan.h);
		// airMass=2.0*(airspan.h-airspan.l); // silly fixed-density model
	}
	float airTransmit=exp(-_AtmosphereAlpha*airMass); // fraction of light penetrating atmosphere
	float airInscatter=1.0-airTransmit; // fraction added by atmosphere
	
	// add: thin-shell of airglow
	
	/* Aurora: just getting the sampling ranges right is tricky!
	
		SKIM only hits upper aurora shell:
		   H.l  (aurora) H.h
	
		BOTH enters aurora layer twice.  Far aurora may need to be air-attenuated.
		   H.l  (post-air aurora)  L.l    (planet+air)    L.h (pre-air aurora) H.h
	
		MAIN is a typical planet-hitting ray:
		   H.l  (aurora)  L.l    (air+planet)
	*/
	float3 aurora=float3(0.0,0.0,0.0);
	if (auroraL.l>=miss_t) 
	{ // SKIM: we miss the lower boundary
		aurora=sample_aurora(r,make_span(auroraH.l,auroraH.h)); // upper shell
	}
	else if (planet_t>=miss_t) 
	{ // BOTH: second stretch of pre-air aurora on far side of atmosphere
		planet+=sample_aurora(r,make_span(auroraL.h,auroraH.h)); // pre atmosphere
		aurora=sample_aurora(r,make_span(auroraH.l,auroraL.l)); // post atmosphere
	} 
	else 
	{ // MAIN: common planet-hitting case
		aurora=sample_aurora(r,make_span(auroraH.l,auroraL.l)); // post atmosphere
	}
	
	float3 total=aurora + airInscatter*airColor + airTransmit*planet;
  
  return float4(tonemap(total),1.0-airTransmit);
}

/* Normal Unity Unlit shader here */
            #include "UnityCG.cginc"
            #pragma vertex aurora_vert
            #pragma fragment aurora_frag
            
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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _GlowColor;
            
            /* Convert raw proxy geometry from object coordinates to raytracer coords. */
            float3 import_proxy_geometry(float3 prox) {
              prox=prox*proxy_size;
              
              // There is only one up axis, and its name is Z
              return float3(prox.x, -prox.z, prox.y);
            }
            
            v2f aurora_vert (appdata v)
            {
                v2f o;
                o.vertobj = import_proxy_geometry(v.vertex.xyz);
                o.camobj = import_proxy_geometry(mul(unity_WorldToObject, float4(_WorldSpaceCameraPos,1.0f)).xyz);
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 aurora_frag (v2f varyings) : SV_Target
            {
                // Set up global coordinate system
                float3 geometry_target = varyings.vertobj/_PlanetRadius;
                float3 camera_start = varyings.camobj/_PlanetRadius; 
                float3 look_dir = normalize(geometry_target-camera_start);
                
                ray r; r.S=camera_start; r.D=look_dir;
                
                return _AuroraPostColor * aurora_raytrace(r) * 10;
            }
            ENDCG
        }
    }
}
