// create by JiepengTan 2018-04-13  
// email: jiepengtan@gmail.com
Shader "ORION/FishManShaderTutorial/FogNEBULA_Lights" {
    Properties{
        _MainTex("Base (RGB)", 2D) = "white" {}
		_LoopNum("_LoopNum", Vector) = (40.,128., 1, 1)
		_FogSpd("_FogSpd", Vector) = (1.,0.,0.,0.5)
		_FogHighRange("_FogHighRange", Vector) = (-5,10,0.,0.5)
		_FogCol("_FogCol", COLOR) = (.025, .2, .125,0.)
		_fogDensities("_fogDensities", Vector) = (7,1,1,1)
	}
		SubShader{
			Pass {
				ZTest Always Cull Off ZWrite Off
				CGPROGRAM
			// Upgrade NOTE: excluded shader from DX11, OpenGL ES 2.0 because it uses unsized arrays
			//	#pragma exclude_renderers d3d11 gles
						// Upgrade NOTE: excluded shader from DX11, OpenGL ES 2.0 because it uses unsized arrays
						//#pragma exclude_renderers d3d11 gles

			//v0.1
			const static int localLightsCount = 64;
			float4 _localLights[localLightsCount];
			float4 _localLightsPositions[localLightsCount];
			float4 _fogDensities;
			int _localLightsCount;
			float localLightNoise;
			float localLightNoiseA;

            float4 _LoopNum = float4(40.,128.,0.,0.);
            float3 _FogSpd ;
			float2 _FogHighRange;
			fixed3 _FogCol;
			 
			#pragma vertex vert  
			#pragma fragment frag  
			#include "ShaderLibs/Framework3D.cginc" 
			#define ITR 100 
			#define FAR 11150.
			

			fixed3 Normal(in fixed3 p)
			{  
				return float3(0.,1.0,0.);
			}

			fixed RayCast(in fixed3 ro, in fixed3 rd)
			{
				if (rd.y>=0.0) {
					return 100000;
				}
				float d = -(ro.y - 0.)/rd.y;
				d = min(100000.0, d);
				return d;
			}

			//MINE
			fixed3 FogA(in fixed3 bgCol, in fixed3 ro, in fixed3 rd, in fixed maxT,
				float3 fogCol, float3 spd, float2 heightRange)
			{
				float d = .4;
				float d1 = .4;
				float3 col = bgCol;
				for (int i = 0; i < (int)_fogDensities.x; i++)//for (int i = 0; i < 7; i++)
				{
					float3  p = ro + rd * d ;

					//float3 localLihtColor;
					//for (int j = 0; j < 64; j++)//for (int i = 0; i < 7; i++)
					//{
					//	//_WorldSpaceCameraPos
					//	float3 localLightPos = (_localLightsPositions[j].xyz) ;
					//	localLihtColor += _localLights[j].w*_localLights[j].xyz *(1 / pow(length(localLightPos.xyz - p.xyz), _localLightsPositions[j].w));// _localLightsPositions
					//}

					// add some movement at some dir
					p += spd * ftime;
					p.z += sin(p.x*.5);

					// get height desity 
					float hDen = (1. - smoothstep(heightRange.x, heightRange.y, p.y));
					// get final  density
					fixed den = TNoise(p*2.2 / (d + 20.), ftime, 0.2)* hDen;
					fixed3 col2 = fogCol * (den *0.5 + 0.5);

					//col2 = col2 + localLihtColor;
					//col = col + localLihtColor;

					col = lerp(col, col2, clamp(den*smoothstep(d - 0.4, d + 2. + d * .75, maxT), 0., 1.));
					d *= 1.5 - _fogDensities.y -5 + 0.3;//d *= 1.5 + 0.3;	
					//d *= 15.5 + 0.3;

					float3  p1 = ro + rd * d1;
					float3 localLihtColor = float3(0, 0, 0);
					for (int j = 0; j < _localLightsCount; j = j + 1)//for (int i = 0; i < 7; i++)
					{
						//_WorldSpaceCameraPos
						float localLightPos = length(_localLightsPositions[j].xyz - p1.xyz);// (_localLightsPositions[j].xyz);
						localLihtColor += _localLights[j].w*_localLights[j].xyz *(1 / pow(localLightPos, _localLightsPositions[j].w));// _localLightsPositions
					}
					// add some movement at some dir
					if (length(localLihtColor) > 0)// && length(col) < 0.5) 
					{
						p1 += spd * ftime;
						p1.z += sin(p1.x*.5);
						// get height desity 
						float hDen1 = (1. - smoothstep(heightRange.x, heightRange.y, p1.y));
						// get final  density
						float den1 = TNoise((p1 - _WorldSpaceCameraPos) *2.2 / (d1 + 20.), ftime, 0.2)* hDen1;
						//float3 col22 = fogCol * (den1 *0.5 + 0.5);

						localLihtColor = (1 - localLightNoise)*localLihtColor + (localLightNoise)*localLihtColor*clamp(den1*smoothstep(d1 - 0.4, d1 + 2. + d1 * .75, maxT), 0., 1.);

						d1 += 1.5 + _fogDensities.z*1.5 - 5 + 0.3;
						col = col + localLightNoiseA*0.6*col*pow(saturate(localLihtColor * (den1 *0.5 + 0.5)),1);
					}

					if (d1 > maxT && d > maxT)break;
				}


				////v0.1
				//d = .4;
				//for (int k = 0; k < (int)_fogDensities.w; k=k+1)//for (int i = 0; i < 7; i++)
				//{
				//	float3  p = ro + rd * d;
				//	float3 localLihtColor = float3(0,0,0);
				//	for (int j = 0; j < _localLightsCount; j=j+1)//for (int i = 0; i < 7; i++)
				//	{
				//		//_WorldSpaceCameraPos
				//		float localLightPos = length(_localLightsPositions[j].xyz - p.xyz);// (_localLightsPositions[j].xyz);
				//		localLihtColor += _localLights[j].w*_localLights[j].xyz *(1 / pow(localLightPos, _localLightsPositions[j].w));// _localLightsPositions
				//	}
				//	// add some movement at some dir
				//	if (length(localLihtColor) > 0) {
				//		p += spd * ftime;
				//		p.z += sin(p.x*.5);
				//		// get height desity 
				//		float hDen = (1. - smoothstep(heightRange.x, heightRange.y, p.y));
				//		// get final  density
				//		fixed den = TNoise( (p - _WorldSpaceCameraPos) *2.2 / (d + 20.), ftime, 0.2)* hDen;
				//		//fixed3 col2 = fogCol * (den *0.5 + 0.5);

				//		localLihtColor = (1-localLightNoise)*localLihtColor + (localLightNoise)*localLihtColor*clamp(den*smoothstep(d - 0.4, d + 2. + d * .75, maxT), 0., 1.);

				//		d += 1.5 + _fogDensities.z*1.5 - 5 + 0.3;
				//		col += localLihtColor * (den *0.5 + 0.5);

				//		
				//	}

				//	if (d > maxT)break;
				//}


				return col+col*col;
			}


            float4 ProcessRayMarch(float2 uv,float3 ro,float3 rd,inout float sceneDep,float4 sceneCol){ 

				float4 sceneColA = sceneCol;

				fixed3 ligt = normalize( fixed3(.5, .05, -.2) );
				fixed3 ligt2 = normalize( fixed3(.5, -.1, -.2) );
    
				fixed rz = RayCast(ro,rd);
	
				fixed3 fogb = lerp(fixed3(.7,.8,.8	)*0.3, fixed3(1.,1.,.77)*.95, pow(dot(rd,ligt2)+1.2, 2.5)*.25);
				fogb *= clamp(rd.y*.5+.6, 0., 1.);
				fixed3 col = fogb;
				
				if ( rz < FAR )
				{
					fixed3 pos = ro+rz*rd;
					fixed3 nor= Normal( pos );
					fixed dif = clamp( dot( nor, ligt ), 0.0, 1.0 );
					fixed spe = pow(clamp( dot( reflect(rd,nor), ligt ), 0.0, 1.0 ),50.);
					col = lerp(fixed3(0.1,0.2,1),fixed3(.3,.5,1),pos.y*.5)*0.2+.1;
					col = col*dif + col*spe*.5 ;
				}
				 
				MergeRayMarchingIntoUnityA(rz,col,sceneDep * 1,sceneCol);  
			
				col = lerp(col, fogb, smoothstep(FAR-7.,FAR,rz)); 
				//then volumetric fog 
				col = FogA(col, ro, rd, rz,_FogCol,_FogSpd,_FogHighRange);
				//post
				col = pow(col,float3(0.8,0.8,0.8));
                sceneCol.xyz = col;
				return sceneCol;// +sceneCol * sceneColA;
            }
            ENDCG
        }//end pass 
    }//end SubShader
    FallBack Off
}



