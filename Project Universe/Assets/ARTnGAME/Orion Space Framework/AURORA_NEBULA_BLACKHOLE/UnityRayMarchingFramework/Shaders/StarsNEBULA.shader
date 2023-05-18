// create by JiepengTan 2018-04-13  email: jiepengtan@gmail.com
Shader "ORION/FishManShaderTutorial/StarsNEBULA" {
    Properties{
        _MainTex("Base (RGB)", 2D) = "white" {}
		depthCutoff("depth Cutoff Distance", float) = 50000
	}
		SubShader{
			Pass {
				ZTest Always Cull Off ZWrite Off
				CGPROGRAM

	#pragma vertex vert  
	#pragma fragment frag  
	#include "ShaderLibs/Feature.cginc"
	#include "ShaderLibs/Framework3D.cginc"

			float depthCutoff;

		float3 StarsA(in float3 rd,float den,float tileNum)
{
	float3 c = float3(0.,0.,0.);
	float3 p = rd;
	float SIZE = 0.5;
	for (float i = 0.; i < 12.; i++)
	{
		float3 q = frac(p*tileNum) - 0.5;
		float3 id = floor(p*tileNum);
		float2 rn = Hash33(id).xy;

		float size = (Hash13(id)*0.2 + 0.8)*SIZE;
		float demp = pow(1. - size / SIZE,.8)*0.45;
		float val = (sin(_Time.y*31.*size)*demp + 1. - demp) * size;
		float c2 = 1. - smoothstep(0.,val,length(q));
		c2 *= step(rn.x,(.0005 + i * i*0.001)*den);
		c += c2 * (lerp(float3(1.0,0.49,0.1),float3(0.75,0.9,1.),rn.y)*0.25 + 0.75);
		p *= 1.4;
	}
	return c * c*.7;
}

            float4 ProcessRayMarch(float2 uv,float3 ro,float3 rd,inout float sceneDep,float4 sceneCol)  {

				if (sceneDep > depthCutoff) {//if (length(ro + rd) < sceneDep) {//if (sceneDep > 21111){// (length(ro + rd) < sceneDep) {
					sceneCol.xyz = sceneCol + StarsA(rd,3.,50.);//sceneCol.xyz =  StarsA(rd,3.,50.);
				}

                return sceneCol;
            } 
            ENDCG
        }//end pass
    }//end SubShader
    FallBack Off
}



