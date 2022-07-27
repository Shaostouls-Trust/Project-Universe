#include "EMParticleFunctions.cginc"

struct SurfaceOutputSmoke
{
    fixed3 Albedo;  
    fixed3 Normal;  
    fixed3 Emission;
    half Specular;
    fixed Gloss; 
    fixed Alpha; 
    #ifdef ALPHATRANSMITANCE_ON
    	fixed AlphaMap;
    #endif
};

inline fixed4 LightingSmoke (SurfaceOutputSmoke s, fixed3 lightDir, fixed atten)
{
	fixed diff;
	fixed thickness = 1-_Thickness;

	#ifdef ALPHATRANSMITANCE_ON
		//bump the alpha to get more visible details
		fixed alphaMap = _AlphaInfluence * saturate(1-s.AlphaMap + _AlphaContrast);
		fixed transmission = thickness + alphaMap;
		diff = saturate((dot(s.Normal, lightDir) + transmission) / ((1 + transmission) * (1 + transmission)));
	#else
		diff = saturate((dot(s.Normal, lightDir) + thickness) / ((1 + thickness) * (1 + thickness)));
	#endif
	
	fixed4 c;
	
	c.rgb = s.Albedo * _LightColor0.rgb * diff * atten;
	
	c.a = s.Alpha;
	return c;
}