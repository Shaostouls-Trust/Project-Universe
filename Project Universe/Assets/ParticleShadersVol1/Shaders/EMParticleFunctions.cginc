#include "EMParticleVariables.cginc"
inline float DistanceFade(float fadeDistEnd, float fadeDistStart, float3 wsParticlePos)
{
	//_WorldSpaceCameraPos comes from Unity defined variables
	float3 wsCameraPos = _WorldSpaceCameraPos; 

	float fadeDistance = distance(wsCameraPos, wsParticlePos) - fadeDistEnd;
	fadeDistance = saturate(fadeDistance / (fadeDistStart - fadeDistEnd));
	return fadeDistance;
}