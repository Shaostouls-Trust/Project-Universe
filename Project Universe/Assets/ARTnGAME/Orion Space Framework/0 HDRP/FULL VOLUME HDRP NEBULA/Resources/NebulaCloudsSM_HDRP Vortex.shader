//https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@7.3/manual/Custom-Post-Process.html?_ga=2.216841366.12121933.1586344315-509234190.1584966518#Shader
//Add the GrayScale effect to the list of custom post - processes that your Project executes.
//To do this, go to Edit > Project Settings > HDRP Default Settings and, at the bottom of the After Post Process list,
//click on the + and select GrayScale.
//Now you can add the GrayScale post - process override to a Volumes in the Scene.To change the effect settings, 
//click the small all text just below the foldout arrow and adjust with the Intensity slider.
Shader "Hidden/Shader/NebulaCloudsSM_HDRP_Vortex"
{
	Properties
	{
		////////// VOLUMETRIC CLOUDS v0.1
		//// CLOUDS
		_MainTex("Base (RGB)", 2D) = "black" {}
	//_CloudTex("Base (RGB)", 2D) = "black" {}
	//_CloudTexP("Base (RGB)", 2D) = "black" {}
	//_SkyTex("Base (RGB)", 2D) = "black" {}//v2.1.15
	_ColorRamp("Colour Palette", 2D) = "gray" {}
	_Close("Close", float) = 0.0
	_Far("Far", float) = 0.0
	v3LightDir("v3LightDir", Vector) = (0,0,0)
	v3LightDirFOG("v3LightDirFOG", Vector) = (0,0,0)
	FogSky("FogSky",float) = 0.0
	_TintColor("Color Tint", Color) = (0,0,0,0)
	ClearSkyFac("Clear Sky Factor",float) = 1.0

		//v3.5 clouds
		_SampleCount0("Sample Count (min)", Float) = 30
		_SampleCount1("Sample Count (max)", Float) = 90
		_SampleCountL("Sample Count (light)", Int) = 16

		[Space]
		_NoiseTex1("Noise Volume", 3D) = ""{}
		_NoiseTex2("Noise Volume", 3D) = ""{}
		_NoiseFreq1("Frequency 1", Float) = 3.1
		_NoiseFreq2("Frequency 2", Float) = 35.1
		_NoiseAmp1("Amplitude 1", Float) = 5
		_NoiseAmp2("Amplitude 2", Float) = 1
		_NoiseBias("Bias", Float) = -0.2

		[Space]
		_Scroll1("Scroll Speed 1", Vector) = (0.01, 0.08, 0.06, 0)
		_Scroll2("Scroll Speed 2", Vector) = (0.01, 0.05, 0.03, 0)

		[Space]
		_Altitude0("Altitude (bottom)", Float) = 1500
		_Altitude1("Altitude (top)", Float) = 3500
		_FarDist("Far Distance", Float) = 30000

		[Space]
		_Scatter("Scattering Coeff", Float) = 0.008
		_HGCoeff("Henyey-Greenstein", Float) = 0.5
		_Extinct("Extinction Coeff", Float) = 0.01

		[Space]
		_SunSize("Sun Size", Range(0,1)) = 0.04
		_AtmosphereThickness("Atmoshpere Thickness", Range(0,5)) = 1.0
		_SkyTint("Sky Tint", Color) = (.5, .5, .5, 1)
		_GroundColor("Ground", Color) = (.369, .349, .341, 1)
		_Exposure("Exposure", Float) = 3
			//v3.5 clouds
			_BackShade("Back shade of cloud top", Float) = 1
			_UndersideCurveFactor("Underside Curve Factor", Float) = 0
			//v3.5.1
			_NearZCutoff("Away from camera Cutoff", Float) = -2
			_HorizonYAdjust("Adjust horizon Height", Float) = 0
			_FadeThreshold("Fade Near", Float) = 0
			//v3.5.3
			_InteractTexture("_Interact Texture", 2D) = "white" {}
			_InteractTexturePos("Interact Texture Pos", Vector) = (1 ,1, 0, 0)
			_InteractTextureAtr("Interact Texture Attributes - 2multi 2offset", Vector) = (1 ,1, 0, 0)
			_InteractTextureOffset("Interact Texture offsets", Vector) = (0 ,0, 0, 0) //v4.0
																					  //v2.1.19
			_fastest("Fastest mode", Int) = 0
			_LocalLightPos("Local Light Pos & Intensity", Vector) = (0 ,0, 0, 0) //local light position (x,y,z) and intensity (w)			 
			_LocalLightColor("Local Light Color & Falloff", Vector) = (0 , 0, 0, 2) //w = _LocalLightFalloff
																					//v2.1.24
			_HorizonZAdjust("Adjust cloud depth", Float) = 1
				//v4.1f
				_mobileFactor("Adjust to 0 to fix Android lighting", Float) = 1
				_alphaFactor("Adjust to 0 to fix Android lighting", Float) = 1
				_invertX("Mirror X", Float) = 0
				_invertRay("Mirror Ray", Float) = 1
				_WorldSpaceCameraPosC("Camera", Vector) = (0 , 0, 0, 1)
				//v4.8
				varianceAltitude1("varianceAltitude1", Float) = 0
				//v4.8.6
				turbidity("turbidity", Float) = 2
				//// END CLOUDS
				//[HideInInspector]_MainTex("Base (RGB)", 2D) = "white" {}
				/*_SunThreshold("sun thres", Color) = (0.87, 0.74, 0.65,1)
				_SunColor("sun color", Color) = (1.87, 1.74, 1.65,1)
				_BlurRadius4("blur", Color) = (0.00325, 0.00325, 0,0)
				_SunPosition("sun pos", Color) = (111, 11,339, 11)*/
				//////////////////// FULL VOLUMETRIC CLOUDS /////////////////////////
				//////////////////// END FULL VOLUMETRIC CLOUDS ////////////////////
				//////////////////// FULL VOLUMETRIC CLOUDS /////////////////////////
				//////////////////// END FULL VOLUMETRIC CLOUDS /////////////////////
				////////// END VOLUMETRIC CLOUDS v0.1




				//////////// SHAFTS
				//[HideInInspector]_MainTexA("Base (RGB)", 2D) = "white" {}
				_SunThreshold("sun thres", Color) = (0.87, 0.74, 0.65,1)
				_SunColor("sun color", Color) = (1.87, 1.74, 1.65,1)
				_BlurRadius4("blur", Color) = (0.00325, 0.00325, 0,0)
				_SunPosition("sun pos", Color) = (111, 11,339, 11)

				//v0.3 - sun rays
				raysResolutionA("Sun Rays resolution", Vector) = (1, 1, 1, 1)
				_rayColor("Ground", Color) = (.969, 0.549, .041, 1)
				rayShadowingA("Ray Shadows", Vector) = (1, 1, 1, 1)
				_underDomeColor("_underDomeColor", Color) = (1, 1, 1, 1)

				//v0.4
				cameraScale("cameraScale", Float) = 1
				extendFarPlaneAboveClouds("extendFarPlaneAboveClouds", Float) = 1

				//v0.5
				YCutHeightDepthScale("YCutHeightDepthScale", Vector) = (0, 1, 0, 1)

				//v0.5
				fogOfWarRadius("fog Of War Radius", Float) = 0
				playerPos("player Pos", Float) = (0,0,0,0)
				//

				//v0.6
				controlCloudEdgeA("control Cloud Edge A", Vector) = (1, 1, 1, 1) //(0.65, 1.22, 1.14, 1.125)
				controlCloudAlphaPower("controlCloudAlphaPower", Float) = 0   //2
				controlBackAlphaPower("controlBackAlphaPower", Float) = 1         //2
				controlCloudEdgeOffset("controlCloudEdgeOffset", Float) = 1		  //1
				depthDilation("depthDilation", Float) = 1						  //1
				_TemporalResponse("Temporal Response", Float) = 1 //1
				_TemporalGain("Temporal Gain", Float) = 1		  //1

				//v0.7a - VORTEX
				vortexPosRadius("vortexPosRadius", Float) = (0, 0, 0, 0)
				vortexControlsA("vortexControlsA", Float) = (0, 0, 0, 0)
				superCellPosRadius("superCellPosRadius", Float) = (0, 0, 0, 0)
				superCellControlsA("superCellControlsA", Float) = (0, 0, 0, 0)
				//v0.7b - SUPER CELLS

			

	}

		HLSLINCLUDE

#pragma target 4.5

#pragma only_renderers d3d11 ps4 xboxone vulkan metal switch

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/TextureXR.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/FXAA.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/RTUpscale.hlsl"

				//v0.1
#include "ClassicNoise3D.hlsl"
//#include "Packages/com.unity.render-pipelines.high-definition/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/EditorShaderVariables.hlsl"
//#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"
////#include "PostProcessing/Shaders/StdLib.hlsl"
//#include "UnityCG.cginc"


////////////////////// FULL VOLUME CLOUDS v0.1 //////////////////////

	//////////////////// FULL VOLUMETRIC CLOUDS /////////////////////////
	//#include "UnityCG.cginc"
#pragma multi_compile __ DEBUG_NO_LOW_FREQ_NOISE
#pragma multi_compile __ DEBUG_NO_HIGH_FREQ_NOISE
#pragma multi_compile __ DEBUG_NO_CURL
#pragma multi_compile __ ALLOW_IN_CLOUDS
#pragma multi_compile __ RANDOM_JITTER_WHITE RANDOM_JITTER_BLUE
#pragma multi_compile __ RANDOM_UNIT_SPHERE
#pragma multi_compile __ SLOW_LIGHTING
#define BIG_STEP 3.0

			//v0.6
			float4 controlCloudEdgeA;// = float4(0.65, 1.22, 1.14, 1.125);
		float controlCloudAlphaPower;// = 2;
		float controlBackAlphaPower;
		float controlCloudEdgeOffset;// = 1;
		float depthDilation;
		float _TemporalResponse;
		float _TemporalGain;

				float4x4 unity_CameraInvProjection;

			uniform int _renderInFront = 0;//v0.1

			int scatterOn = 1;
			int sunRaysOn = 1;
			float zeroCountSteps = 0;
			int sunShaftSteps = 5;

			float4 raysResolution = float4(1, 1, 1, 0);
			float4 rayShadowing = float4(1, 1, 1, 1);
			uniform float4x4 _CameraInvViewMatrix;
			uniform float4x4 _FrustumCornersES;
			//uniform float4 _CameraWS;
			uniform float _FarPlane;
			uniform sampler2D _AltoClouds;
			uniform sampler3D _ShapeTexture;
			uniform sampler3D _DetailTexture;
			uniform sampler2D _WeatherTexture;

			uniform sampler2D _maskTexture;

			uniform sampler2D _CurlNoise;
			uniform sampler2D _BlueNoise;
			uniform float4 _BlueNoise_TexelSize;
			uniform float4 _Randomness;
			uniform float _SampleMultiplier;

			uniform float3 _SunDir;
			uniform float3 _PlanetCenter;
			//uniform float3 _SunColor;
			uniform float3 _CloudBaseColor;
			uniform float3 _CloudTopColor;

			uniform float3 _ZeroPoint;
			uniform float _SphereSize;
			uniform float2 _CloudHeightMinMax;
			uniform float _Thickness;

			uniform float _Coverage;
			uniform float _AmbientLightFactor;
			uniform float _SunLightFactor;
			uniform float _HenyeyGreensteinGForward;
			uniform float _HenyeyGreensteinGBackward;
			uniform float _LightStepLength;
			uniform float _LightConeRadius;
			//uniform float _Density;
			uniform float _Scale;
			uniform float _DetailScale;
			uniform float _WeatherScale;

			float _maskScale;
			float3 _maskPos;
			float fogOfWarRadius;
			float4 playerPos;

			uniform float _CurlDistortScale;
			uniform float _CurlDistortAmount;

			uniform float _WindSpeed;
			uniform float3 _WindDirection;
			uniform float3 _WindOffset;
			uniform float2 _CoverageWindOffset;
			uniform float2 _HighCloudsWindOffset;

			uniform float _CoverageHigh;
			uniform float _CoverageHighScale;
			uniform float _HighCloudsScale;

			uniform float2 _LowFreqMinMax;
			uniform float _HighFreqModifier;

			uniform float4 _Gradient1;
			uniform float4 _Gradient2;
			uniform float4 _Gradient3;

			//v0.8
			float4 vortexPosRadius;
			float4 	vortexControlsA;
			float4 	superCellPosRadius;
			float4 	superCellControlsA;

			uniform int _Steps;
			//////////////////// END FULL VOLUMETRIC CLOUDS /////////////////////
			//TEXTURE2D(_MainTex);
			TEXTURE2D_X(_MainTexB);
			//TEXTURE2D(_ColorBuffer); //v0.1 removed from URP
		//	TEXTURE2D(_Skybox);//v0.1 removed from URP
			SAMPLER(sampler_MainTexB);
			float4 _MainTexB_TexelSize;
			//	SAMPLER(sampler_ColorBuffer);//v0.1 removed from URP
			//	SAMPLER(sampler_Skybox);//v0.1 removed from URP


			SAMPLER(sampler_WeatherTexture);
			SAMPLER(sampler_maskTexture);//v0.1.1

			float farCamDistFactor;
			////////// CLOUDS
			int4 _SceneFogMode;
			float4 _SceneFogParams;
			//v4.8
			float _invertX = 0;
			float _invertRay = 1;
			float3 _WorldSpaceCameraPosC;
			float varianceAltitude1 = 0;

			//v4.1f
			float _mobileFactor;
			float _alphaFactor;

			//v3.5.3
			sampler2D _InteractTexture;
			float4 _InteractTexturePos;
			float4 _InteractTextureAtr;
			float4 _InteractTextureOffset; //v4.0

			float _NearZCutoff;
			float _HorizonYAdjust;
			float _HorizonZAdjust;
			float _FadeThreshold;

			//v3.5 clouds	
			float _BackShade;
			float _UndersideCurveFactor;

			//VFOG
			float4x4 _WorldClip;

			float _SampleCount0 = 2;
			float _SampleCount1 = 3;
			int _SampleCountL = 4;

			sampler3D _NoiseTex1;
			sampler3D _NoiseTex2;
			float _NoiseFreq1 = 3.1;
			float _NoiseFreq2 = 35.1;
			float _NoiseAmp1 = 5;
			float _NoiseAmp2 = 1;
			float _NoiseBias = -0.2;

			float3 _Scroll1 = float3 (0.01, 0.08, 0.06);
			float3 _Scroll2 = float3 (0.01, 0.05, 0.03);

			float _Altitude0 = 1500;
			float _Altitude1 = 3500;
			float _FarDist = 30000;
			float _Scatter = 0.008;
			float _HGCoeff = 0.5;
			float _Extinct = 0.01;

			//float3 _SkyTint;
			float _SunSize;
			float3 _GroundColor; //v4.0
			float _Exposure; //v4.0

			uniform float4 _CloudTex_TexelSize;
			uniform float4 _CloudTexP_TexelSize;//v4.8.2

			TEXTURE2D_X(_CloudTex);
			//TEXTURE2D(_CloudTex);
			SAMPLER(sampler_CloudTex);

			TEXTURE2D_X(_CloudTexP);
			//TEXTURE2D(_CloudTexP);
			SAMPLER(sampler_CloudTexP);

			TEXTURE2D_X(_SkyTex);
			//TEXTURE2D(_SkyTex);
			SAMPLER(sampler_SkyTex);

			float frameFraction = 0;

			//v2.1.19
			int _fastest;
			float4 _LocalLightPos;
			float4 _LocalLightColor;
			///////// END CLOUDS

			//float _Blend;

			//TEXTURE2D(_CameraDepthTexture);
			//SAMPLER(sampler_CameraDepthTexture); //v0.1 FULL VOLUME CLOUDS - REMOVEVD FROM URP
			//half4 _CameraDepthTexture_ST;

			/*half4 _SunThreshold = half4(0.87, 0.74, 0.65, 1);

			half4 _SunColor = half4(0.87, 0.74, 0.65, 1);
			uniform half4 _BlurRadius4 = half4(2.5 / 768, 2.5 / 768, 0.0, 0.0);
			uniform half4 _SunPosition = half4(111, 11, 339, 11);
			uniform half4 _MainTex_TexelSize;

			#define SAMPLES_FLOAT 16.0f
			#define SAMPLES_INT 16*/

			//FOG URP /////////////////////
			// Vertex manipulation
			/*float2 TransformTriangleVertexToUV(float2 vertex)
			{
				float2 uv = (vertex + 1.0) * 0.5;
				return uv;
			}*/

			TEXTURE2D(_NoiseTex);
			SAMPLER(sampler_NoiseTex);
			//TEXTURE2D_SAMPLER2D(_NoiseTex, sampler_NoiseTex);

			//URP v0.1
			// #pragma multi_compile FOG_LINEAR FOG_EXP FOG_EXP2
#pragma multi_compile _ RADIAL_DIST
#pragma multi_compile _ USE_SKYBOX

			float _DistanceOffset;
			float _Density;
			float _LinearGrad;
			float _LinearOffs;
			float _Height;
			float _cameraRoll;
			//WORLD RECONSTRUCT	
			//float4x4 _InverseView;
			//float4x4 _camProjection;	/////TO REMOVE
			// Fog/skybox information
		//	half4 _FogColor; //v0.1 removed from URP
			samplerCUBE _SkyCubemap;
			half4 _SkyCubemap_HDR;
			half4 _SkyTint;
			half _SkyExposure;
			float _SkyRotation;
			float4 _cameraDiff;
			float _cameraTiltSign;
			float _NoiseDensity;
			float _NoiseScale;
			float3 _NoiseSpeed;
			float _NoiseThickness;
			float _OcclusionDrop;
			float _OcclusionExp;
			int noise3D = 0;
			//END FOG URP /////////////////
			/////////////////// MORE FOG URP ///////////////////////////////////////////////////////
			/////////////////// MORE FOG URP ///////////////////////////////////////////////////////
			/////////////////// MORE FOG URP ///////////////////////////////////////////////////////
			// Applies one of standard fog formulas, given fog coordinate (i.e. distance)
			half ComputeFogFactorA(float coord) /// REDFINED, SO CHANGED NAME
			{
				float fog = 0.0;
#if FOG_LINEAR
				// factor = (end-z)/(end-start) = z * (-1/(end-start)) + (end/(end-start))
				fog = coord * _LinearGrad + _LinearOffs;
#elif FOG_EXP
				// factor = exp(-density*z)
				fog = _Density * coord;
				fog = exp2(-fog);
#else // FOG_EXP2
				// factor = exp(-(density*z)^2)
				fog = _Density * coord;
				fog = exp2(-fog * fog);
#endif
				return saturate(fog);
			}

			half ComputeFogFactorB(float coord)
			{
				float fogFac = 0.0;
				if (_SceneFogMode.x == 1) // linear
				{
					// factor = (end-z)/(end-start) = z * (-1/(end-start)) + (end/(end-start))
					fogFac = coord * _SceneFogParams.z + _SceneFogParams.w;
				}
				if (_SceneFogMode.x == 2) // exp
				{
					// factor = exp(-density*z)
					fogFac = _SceneFogParams.y * coord; fogFac = exp2(-fogFac);
				}
				if (_SceneFogMode.x == 3) // exp2
				{
					// factor = exp(-(density*z)^2)
					fogFac = _SceneFogParams.x * coord; fogFac = exp2(-fogFac * fogFac);
				}
				return saturate(fogFac);
			}

			// Distance-based fog
			float ComputeDistance(float3 ray, float depth)
			{
				float dist;
#if RADIAL_DIST
				dist = length(ray * depth);
#else
				dist = depth * _ProjectionParams.z;
#endif
				// Built-in fog starts at near plane, so match that by
				// subtracting the near value. Not a perfect approximation
				// if near plane is very large, but good enough.
				dist -= _ProjectionParams.y;
				return dist;
			}
			////LOCAL LIGHT
			float4 localLightColor;
			float4 localLightPos;
			/////////////////// SCATTER
			bool doDistance;
			bool doHeight;
			// Distance-based fog
			uniform float4 _CameraWS;
			uniform float4x4 _FrustumCornersWS;
			//SM v1.7
			uniform float luminance, Multiplier1, Multiplier2, Multiplier3, bias, lumFac, contrast, turbidity;
			//uniform float mieDirectionalG = 0.7,0.913; 
			float mieDirectionalG;
			float mieCoefficient;//0.054
			float reileigh;
			//SM v1.7
			uniform sampler2D _ColorRamp;
			uniform float _Close;
			uniform float _Far;
			uniform float3 v3LightDir;		// light source
			uniform float3 v3LightDirFOG;
			uniform float FogSky;
			float4 _TintColor; //float3(680E-8, 1550E-8, 3450E-8);
			float4 _TintColorL;
			float4 _TintColorK;
			uniform float ClearSkyFac;
			uniform float4 _HeightParams;
			// x = start distance
			uniform float4 _DistanceParams;

			//int4 _SceneFogMode; // x = fog mode, y = use radial flag
			//float4 _SceneFogParams;
#ifndef UNITY_APPLY_FOG
	//half4 unity_FogColor; //ALREADY DEFINED !!!!!!!!!!
			half4 unity_FogDensity;
#endif	

			uniform float e = 2.71828182845904523536028747135266249775724709369995957;
			uniform float pi = 3.141592653589793238462643383279502884197169;
			uniform float n = 1.0003;
			uniform float N = 2.545E25;
			uniform float pn = 0.035;
			uniform float3 lambda = float3(680E-9, 550E-9, 450E-9);
			uniform float3 K = float3(0.686, 0.678, 0.666);//const vec3 K = vec3(0.686, 0.678, 0.666);
			uniform float v = 4.0;
			uniform float rayleighZenithLength = 8.4E3;
			uniform float mieZenithLength = 1.25E3;
			uniform float EE = 1000.0;
			uniform float sunAngularDiameterCos = 0.999956676946448443553574619906976478926848692873900859324;
			// 66 arc seconds -> degrees, and the cosine of that
			float cutoffAngle = 3.141592653589793238462643383279502884197169 / 1.95;
			float steepness = 1.5;
			// Linear half-space fog, from https://www.terathon.com/lengyel/Lengyel-UnifiedFog.pdf
			float ComputeHalfSpace(float3 wsDir)
			{
				//float4 _HeightParams = float4(1,1,1,1);
				//wsDir.y = wsDir.y - abs(11.2*_cameraDiff.x);// -0.4;// +abs(11.2*_cameraDiff.x);

				float3 wpos = _CameraWS.xyz + wsDir; // _CameraWS + wsDir;
				float FH = _HeightParams.x;
				float3 C = _CameraWS.xyz;
				float3 V = wsDir;
				float3 P = wpos;
				float3 aV = _HeightParams.w * V;
				float FdotC = _HeightParams.y;
				float k = _HeightParams.z;
				float FdotP = P.y - FH;
				float FdotV = wsDir.y;
				float c1 = k * (FdotP + FdotC);
				float c2 = (1 - 2 * k) * FdotP;
				float g = min(c2, 0.0);
				g = -length(aV) * (c1 - g * g / abs(FdotV + 1.0e-5f));
				return g;
			}

			//SM v1.7
			float3 totalRayleigh(float3 lambda) {
				float pi = 3.141592653589793238462643383279502884197169;
				float n = 1.0003; // refraction of air
				float N = 2.545E25; //molecules per air unit volume 								
				float pn = 0.035;
				return (8.0 * pow(pi, 3.0) * pow(pow(n, 2.0) - 1.0, 2.0) * (6.0 + 3.0 * pn)) / (3.0 * N * pow(lambda, float3(4.0, 4.0, 4.0)) * (6.0 - 7.0 * pn));
			}

			float rayleighPhase(float cosTheta)
			{
				return (3.0 / 4.0) * (1.0 + pow(cosTheta, 2.0));
			}

			float3 totalMie(float3 lambda, float3 K, float T)
			{
				float pi = 3.141592653589793238462643383279502884197169;
				float v = 4.0;
				float c = (0.2 * T) * 10E-18;
				return 0.434 * c * pi * pow((2.0 * pi) / lambda, float3(v - 2.0, v - 2.0, v - 2.0)) * K;
			}

			float hgPhase(float cosTheta, float g)
			{
				float pi = 3.141592653589793238462643383279502884197169;
				return (1.0 / (4.0*pi)) * ((1.0 - pow(g, 2.0)) / pow(abs(1.0 - 2.0*g*cosTheta + pow(g, 2.0)), 1.5));
			}

			float sunIntensity(float zenithAngleCos)
			{
				float cutoffAngle = 3.141592653589793238462643383279502884197169 / 1.95;//pi/
				float steepness = 1.5;
				float EE = 1000.0;
				return EE * max(0.0, 1.0 - exp(-((cutoffAngle - acos(zenithAngleCos)) / steepness)));
			}

			float logLuminance(float3 c)
			{
				return log(c.r * 0.2126 + c.g * 0.7152 + c.b * 0.0722);
			}

			float3 tonemap(float3 HDR)
			{
				float Y = logLuminance(HDR);
				float low = exp(((Y*lumFac + (1.0 - lumFac))*luminance) - bias - contrast / 2.0);
				float high = exp(((Y*lumFac + (1.0 - lumFac))*luminance) - bias + contrast / 2.0);
				float3 ldr = (HDR.rgb - low) / (high - low);
				return float3(ldr);
			}
			/////////////////// END SCATTER
			half _Opacity;
			struct Varyings
			{
				//float2 uv        : TEXCOORD0;
				//float4 vertex : SV_POSITION;
				//UNITY_VERTEX_OUTPUT_STEREO

				float4 position : SV_POSITION;// SV_Position;
				float2 texcoord : TEXCOORD0;
				float3 ray : TEXCOORD1;
				float2 uvFOG : TEXCOORD2;
				float4 interpolatedRay : TEXCOORD3;

				float3 FarCam : TEXCOORD4; //URP
										   //float2 uv_depth : TEXCOORD5; //URP

				UNITY_VERTEX_OUTPUT_STEREO
			};
			/////////////////// END MORE FOG URP ////////////////////////////////////////////////////
			/////////////////// END MORE FOG URP ////////////////////////////////////////////////////
			/////////////////// END MORE FOG URP ////////////////////////////////////////////////////
		////////////////////// END FULL VOLUME CLOUDS v0.1 ///////////////////////







		////////////////////////// SHAFTS

		//#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/API/D3D11.hlsl"
		//#include "UnityCG.cginc"
		//#include "Lighting.cginc"
		//#include "AutoLight.cginc" 
		//#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/ScreenSpaceLighting/ShaderVariablesScreenSpaceLighting.hlsl"
		//#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/ScreenSpaceLighting/ShaderVariablesScreenSpaceLighting.cs.hlsl"
		//HDRP 2019.3
		//#define UNITY_DECLARE_TEX2DARRAY(tex) Texture2DArray tex; SamplerState sampler##tex
		//https://forum.unity.com/threads/has-depthpyramidtexture-been-removed-from-hdrp-in-unity-2019-3.851236/#post-5615971
		//UNITY_DECLARE_TEX2DARRAY(_CameraDepthTexture);
		//	TEXTURE2D_X(_DepthTexture);
		//	SAMPLER(sampler_DepthTexture);
		//float4 _DepthPyramidScale;
#define UNITY_SAMPLE_TEX2DARRAY_LOD(tex,coord,lod) tex.SampleLevel (sampler##tex,coord, lod)


	//MASK
	//	sampler2D _SourceTex, _WaterInterfaceTex;
			TEXTURE2D_X(_SourceTex);
			TEXTURE2D_X(_WaterInterfaceTex);

			// List of properties to control the post process effect
			TEXTURE2D_X(_MainTexA);
			TEXTURE2D_X(_ColorBuffer);
			TEXTURE2D_X(_Skybox);
			//TEXTURE2D_X(_CameraDepthTexture);




			uniform half4 _MainTexA_TexelSize;

			//SAMPLER(sampler_MainTexA);
			SAMPLER(sampler_ColorBuffer);
			SAMPLER(sampler_Skybox);
			//SAMPLER(sampler_CameraDepthTexture);
			half4 _CameraDepthTexture_ST;

			float _Blend;
			half4 _SunThreshold = half4(0.87, 0.74, 0.65, 1);
			half4 _SunColor = half4(0.87, 0.74, 0.65, 1);
			uniform half4 _BlurRadius4 = half4(2.5 / 768, 2.5 / 768, 0.0, 0.0);
			uniform half4 _SunPosition = half4(1, 1, 1, 1);
			//uniform half4 _MainTexA_TexelSize;

#define SAMPLES_FLOAT 12.0f
#define SAMPLES_INT 12

/*half4 _SunThreshold = half4(0.87, 0.74, 0.65, 1);
half4 _SunColor = half4(0.87, 0.74, 0.65, 1);
uniform half4 _BlurRadius4 = half4(2.5 / 768, 2.5 / 768, 0.0, 0.0);
uniform half4 _SunPosition = half4(111, 11, 339, 11);
uniform half4 _MainTex_TexelSize;
#define SAMPLES_FLOAT 16.0f
#define SAMPLES_INT 16*/



/////////// REFRACT LINE
//v4.6a
			float _BumpMagnitudeRL, _BumpScaleRL, _BumpLineHeight;
			//v4.6
			float _refractLineWidth;
			float _refractLineXDisp;
			float _refractLineXDispA;
			float4 _refractLineFade;
			//v4.3
			//sampler2D _MainTex, _SourceTex, _WaterInterfaceTex;
			//TEXTURE2D_X(_BumpTex);// sampler2D _BumpTex;
			sampler2D _BumpTex;
			float4 _BumpTex_ST;
			float _BumpMagnitude, _BumpScale;
			float4 _BumpVelocity, _underWaterTint;
			float _underwaterDepthFade;
			//float2 intensity;
			//v4.6
			//half4 _MainTex_TexelSize;
			//half4 _BlurOffsets;
			half3 PerPixelNormal(sampler2D bumpMap, half4 coords, half3 vertexNormal, half bumpStrength)
			{
				half4 bump = tex2D(bumpMap, coords.xy) + tex2D(bumpMap, coords.zw);
				bump.xy = bump.wy - half2(1.0, 1.0);
				half3 worldNormal = vertexNormal + bump.xxy * bumpStrength * half3(1, 0, 1);
				return normalize(worldNormal);
			}
			//half3 PerPixelNormal(sampler2D bumpMap, half4 coords, half3 vertexNormal, half bumpStrength)
			half3 PerPixelNormalUnpacked(sampler2D bumpMap, half4 coords, half bumpStrength)
			{
				half4 bump = tex2D(bumpMap, coords.xy) + tex2D(bumpMap, coords.zw);
				bump = bump * 0.5;
				half3 normal = UnpackNormal(bump);
				normal.xy *= bumpStrength;
				return normalize(normal);
			}
			////////// END REFRACT LINE




			//v0.3 - rays
			float4 raysResolutionA;
			float4 _rayColor;
			float4 rayShadowingA;
			float4 _underDomeColor;

			//v0.4 - fix scaling
			float cameraScale;
			float extendFarPlaneAboveClouds;

			//v0.5
			float4 YCutHeightDepthScale;

			// Vertex manipulation
			float2 TransformTriangleVertexToUV(float2 vertex)
			{
				float2 uv = (vertex + 1.0) * 0.5;
				return uv;
			}

			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				//#if UNITY_UV_STARTS_AT_TOP
				float2 uv1 : TEXCOORD1;
				//#endif		
				float3 FarCam : TEXCOORD2; //URP v0.1
			};


			struct v2f_radial {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 blurVector : TEXCOORD1;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			struct AttributesB
			{
				uint vertexID : SV_VertexID;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VaryingsB
			{
				//float4 positionCS : SV_POSITION;
				//float2 texcoord   : TEXCOORD0;
				float4 pos : SV_POSITION;
				float2 uv   : TEXCOORD0;
				float2 uv1   : TEXCOORD1;
				UNITY_VERTEX_OUTPUT_STEREO
			};
			struct VaryingsA
			{
				//float4 positionCS : SV_POSITION;
				//float2 texcoord   : TEXCOORD0;
				float4 pos : SV_POSITION;
				float2 uv   : TEXCOORD0;
				float2 uv1   : TEXCOORD1;
				float2 bumpUV : TEXCOORD2;
				//v4.6
				half2 taps[4] : TEXCOORD3;

				UNITY_VERTEX_OUTPUT_STEREO
			};

			/*struct Attributes
			{
				float4 positionOS       : POSITION;
				float2 uv               : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct Varyings
			{
				float4 vertex : SV_POSITION;
				float2 uv     : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
			};*/



			float Linear01DepthA(float2 uv)
			{
#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
				return SAMPLE_TEXTURE2D_ARRAY(_CameraDepthTexture, sampler_CameraDepthTexture, uv, unity_StereoEyeIndex).r;
#else
				//return SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, uv);
				//uint2 positionSS = input.texcoord * _ScreenSize.xy;
				uint2 positionSS = uv * _ScreenSize.xy;
				return LOAD_TEXTURE2D_X(_CameraDepthTexture, positionSS);
#endif
			}

			float4 FragGrey(v2f i) : SV_Target
			{
				//float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv.xy);
				//half4 colorB = SAMPLE_TEXTURE2D(_ColorBuffer, sampler_ColorBuffer, i.uv.xy);
				uint2 positionSS = i.uv * _ScreenSize.xy;
				float4 color = LOAD_TEXTURE2D_X(_MainTexA, positionSS);
				half4 colorB = LOAD_TEXTURE2D_X(_ColorBuffer, positionSS);
				return color;
			}

				half4 fragScreen(v2f i) : SV_Target{

					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

			//half4 colorA = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv.xy); 
			uint2 positionSS = i.uv * _ScreenSize.xy;
			uint2 positionSS1 = i.uv1 * _ScreenSize.xy;
			half4 colorA = LOAD_TEXTURE2D_X(_MainTexA, positionSS);

			//ADD REFLECTION in dark parts of water
			//if (colorA.r < 0.05 && colorA.g < 0.05 && colorA.b < 0.05) {//if (tex.g == 0) {
			//	//colorA = float4(0.2, 0.7, 0.9, colorA.a);
			//	//find first color and sample it
			//	[loop]
			//	int counter = 0;
			//	//for (float j = 0; j < 1 * _ScreenSize.y; j = j + _MainTexA_TexelSize.y* _ScreenSize.y) {
			//	for (float j = _ScreenSize.y ; j >= 0; j = j - _MainTexA_TexelSize.y* _ScreenSize.y) {
			//		//if (j > positionSS.y *1) {
			//		if (j < positionSS.y * 1) {
			//			float4 mainTexUP = LOAD_TEXTURE2D_X(_MainTexA, float2(positionSS.x, j));
			//			if (mainTexUP.r < 0.05 && mainTexUP.g < 0.05 && mainTexUP.b < 0.05) {
			//				counter++;
			//			}
			//			else {
			//				//float4 mainTexUPA = LOAD_TEXTURE2D_X(_MainTexA, float2(positionSS.x, j + counter * _MainTexA_TexelSize.y* _ScreenSize.y * 1));
			//				float4 mainTexUPA = LOAD_TEXTURE2D_X(_MainTexA, float2(positionSS.x, j - counter * _MainTexA_TexelSize.y* _ScreenSize.y * 1));
			//				colorA = mainTexUPA;//mainTexUP;
			//				//colorA.a = 0; 
			//				break;
			//			}
			//		}
			//	}
			//}

			#if !UNITY_UV_STARTS_AT_TOP																		
				//half4 colorB = SAMPLE_TEXTURE2D(_ColorBuffer, sampler_ColorBuffer, i.uv1.xy);
				half4 colorB = LOAD_TEXTURE2D_X(_ColorBuffer, positionSS1);
			#else																		
				//half4 colorB = SAMPLE_TEXTURE2D(_ColorBuffer, sampler_ColorBuffer, i.uv.xy);//v1.1
				half4 colorB = LOAD_TEXTURE2D_X(_ColorBuffer, positionSS);//v1.1
			#endif
			half4 depthMask = saturate(colorB * _SunColor);

			if (_SunColor.a == 0.01) {
				//depthMask = saturate(_SunColor);
				//return  1.0f - (1.0f - 0) * (1.0f - depthMask);
				return  (colorA);
			}

			return  1.0f - (1.0f - colorA) * (1.0f - depthMask);//colorA * 5.6;// 1.0f - (1.0f - colorA) * (1.0f - depthMask);
			}

				//v4.6
			half4 _BlurOffsets;
			half4 fragAdd(v2f i) : SV_Target{

				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

				uint2 positionSS = i.uv * _ScreenSize.xy;
				uint2 positionSS1 = i.uv1 * _ScreenSize.xy;
				//half4 colorA = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv.xy);
				half4 colorA = LOAD_TEXTURE2D_X(_MainTexA, positionSS);
				#if !UNITY_UV_STARTS_AT_TOP			
				//half4 colorB = SAMPLE_TEXTURE2D(_ColorBuffer, sampler_ColorBuffer, i.uv1.xy);
				half4 colorB = LOAD_TEXTURE2D_X(_ColorBuffer, positionSS1);
			#else			
				//half4 colorB = SAMPLE_TEXTURE2D(_ColorBuffer, sampler_ColorBuffer, i.uv.xy);
				half4 colorB = LOAD_TEXTURE2D_X(_ColorBuffer, positionSS);
			#endif
			half4 depthMask = saturate(colorB * _SunColor);
			return 1 * colorA + depthMask;
			}







				///////////////////// SHAFTS /////////////////

				VaryingsA Vert(AttributesB input)
			{
				VaryingsA output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
				output.pos = GetFullScreenTriangleVertexPosition(input.vertexID);
				output.uv = GetFullScreenTriangleTexCoord(input.vertexID);
				output.uv1 = GetFullScreenTriangleTexCoord(input.vertexID);

				output.bumpUV = output.uv * _BumpTex_ST.xy + _BumpTex_ST.zw;

				//v4.6
#ifdef UNITY_SINGLE_PASS_STEREO
			// we need to keep texel size correct after the uv adjustment.
				output.taps[0] = UnityStereoScreenSpaceUVAdjust(output.uv + _MainTexA_TexelSize * _BlurOffsets.xy * (1.0f / _MainTexA_ST.xy), _MainTexA_ST);
				output.taps[1] = UnityStereoScreenSpaceUVAdjust(output.uv - _MainTexA_TexelSize * _BlurOffsets.xy * (1.0f / _MainTexA_ST.xy), _MainTexA_ST);
				output.taps[2] = UnityStereoScreenSpaceUVAdjust(output.uv + _MainTexA_TexelSize * _BlurOffsets.xy * half2(1, -1) * (1.0f / _MainTexA_ST.xy), _MainTexA_ST);
				output.taps[3] = UnityStereoScreenSpaceUVAdjust(output.uv - _MainTexA_TexelSize * _BlurOffsets.xy * half2(1, -1) * (1.0f / _MainTexA_ST.xy), _MainTexA_ST);
#else
				output.taps[0] = output.uv + _MainTexA_TexelSize * _BlurOffsets.xy;// *_ScreenSize.xy;// *scalerMask;
				output.taps[1] = output.uv - _MainTexA_TexelSize * _BlurOffsets.xy;// *_ScreenSize.xy;
				output.taps[2] = output.uv + _MainTexA_TexelSize * _BlurOffsets.xy * half2(1, -1);// *_ScreenSize.xy;
				output.taps[3] = output.uv - _MainTexA_TexelSize * _BlurOffsets.xy * half2(1, -1);// *_ScreenSize.xy;
#endif

				return output;
			}

			//v2f vert(Attributes v) {

			//	//v2f o = (v2f)0;
			//	Varyings o;
			//	UNITY_SETUP_INSTANCE_ID(v);
			//	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
			//
			//	//VertexPositionInputs vertexInput = GetVertexPositionInputs(v.positionOS.xyz);		
			//	//o.pos = float4(vertexInput.positionCS.xy, 0.0, 1.0);


			//	o.pos = GetFullScreenTriangleVertexPosition(input.vertexID);
			//	o.uv = GetFullScreenTriangleTexCoord(input.vertexID);


			//	float2 uv = v.uv;
			//			
			//	#if !UNITY_UV_STARTS_AT_TOP
			//			uv = uv * float2(1.0, -1.0) + float2(0.0, 1.0);				
			//	#endif

			//	o.uv = uv;

			//	#if !UNITY_UV_STARTS_AT_TOP
			//			o.uv1 = uv.xy;
			//			if (_MainTex_TexelSize.y < 0)
			//				o.uv1.y = 1 - o.uv1.y;
			//	#endif	
			//	return o;
			//}



			v2f_radial vert_radial(AttributesB input)
			{
				v2f_radial output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
				output.pos = GetFullScreenTriangleVertexPosition(input.vertexID);
				output.uv = GetFullScreenTriangleTexCoord(input.vertexID);
				output.blurVector = (_SunPosition.xy - output.uv.xy) * _BlurRadius4.xy;
				return output;
			}

			//v2f_radial vert_radial(Attributes v) {										

			//	v2f_radial o = (v2f_radial)0;
			//	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
			//	
			//	VertexPositionInputs vertexInput = GetVertexPositionInputs(v.positionOS.xyz);
			//	o.pos = float4(vertexInput.positionCS.xy, 0.0, 1.0);
			//	float2 uv = v.uv;		

			//	#if !UNITY_UV_STARTS_AT_TOP
			//			//uv = uv * float2(1.0, -1.0) + float2(0.0, 1.0);
			//	#endif
			//	o.uv.xy = uv;
			//				
			//	o.blurVector = (_SunPosition.xy - uv.xy) * _BlurRadius4.xy;

			//	return o;
			//}

			half4 frag_radial(v2f_radial i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

				half4 color = half4(0,0,0,0);
				for (int j = 0; j < SAMPLES_INT; j++)
				{
					//half4 tmpColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv.xy);
					//half4 tmpColor = LOAD_TEXTURE2D_X(_MainTexA, i.uv.xy);
					uint2 positionSS = i.uv * _ScreenSize.xy;
					float4 tmpColor = LOAD_TEXTURE2D_X(_MainTexA, positionSS);
					color += tmpColor;
					i.uv.xy += i.blurVector;
				}

				return color / SAMPLES_FLOAT;
			}

				half TransformColor(half4 skyboxValue) {
				return dot(max(skyboxValue.rgb - _SunThreshold.rgb, half3(0, 0, 0)), half3(1, 1, 1)); // threshold and convert to greyscale
			}

			half4 frag_depth(v2f i) : SV_Target{

				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

				uint2 positionSS = i.uv * _ScreenSize.xy;
				uint2 positionSS1 = i.uv1 * _ScreenSize.xy;
				#if !UNITY_UV_STARTS_AT_TOP			
				//float depthSample = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.uv1.xy), _ZBufferParams);
				//float depthSample = Linear01Depth(LOAD_TEXTURE2D_X(_CameraDepthTexture, positionSS1), _ZBufferParams);
			#else			
				//float depthSample = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.uv.xy), _ZBufferParams);
				//float depthSample = Linear01Depth(LOAD_TEXTURE2D_X(_CameraDepthTexture, positionSS), _ZBufferParams);
			#endif

				//HDRP
				//half depthRaw = UNITY_SAMPLE_TEX2DARRAY_LOD(_CameraDepthTexture, float4(positionSS.xy * _DepthPyramidScale / 1, 0, 0), 0).r;//v0.4a
				half depthRaw = UNITY_SAMPLE_TEX2DARRAY_LOD(_CameraDepthTexture, float4(positionSS.xy * float2(1, 0.6667) / 1, 0, 0), 0).r;//v0.4a HDRP 10.2

				//half depth = LOAD_TEXTURE2D_X_LOD(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos).xy, 0).r;
				float depthSample = Linear01Depth(depthRaw, _ZBufferParams);

				//half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv.xy);
				half4 tex = LOAD_TEXTURE2D_X(_MainTexA, positionSS);
				//depthSample = Linear01Depth(depthSample, _ZBufferParams);

				// consider maximum radius
				#if !UNITY_UV_STARTS_AT_TOP
					half2 vec = _SunPosition.xy - i.uv1.xy;
				#else
					half2 vec = _SunPosition.xy - i.uv.xy;
				#endif
				half dist = saturate(_SunPosition.w - length(vec.xy));

				half4 outColor = 0;

				// consider shafts blockers		
				if (depthSample > 1 - 0.018) {
					//outColor = TransformColor(tex) * dist;
			}
			#if !UNITY_UV_STARTS_AT_TOP
				if (depthSample < 0.018) {
					outColor = TransformColor(tex) * dist;
				}
			#else
				if (depthSample > 1 - 0.018) {
					outColor = TransformColor(tex) * dist;
				}
			#endif

				/*if (tex.g == 0) {
					outColor = float4(0.2,0.7,0.9, outColor.a);
				}*/

			return outColor * 1;
			}


				half4 frag_nodepth(v2f i) : SV_Target{

					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

					uint2 positionSS = i.uv * _ScreenSize.xy;
					uint2 positionSS1 = i.uv1 * _ScreenSize.xy;
					#if !UNITY_UV_STARTS_AT_TOP		
					//float4 sky = SAMPLE_TEXTURE2D(_Skybox, sampler_Skybox, i.uv.xy);
					float4 sky = LOAD_TEXTURE2D_X(_Skybox, positionSS);
				#else	
					//float4 sky = SAMPLE_TEXTURE2D(_Skybox, sampler_Skybox, i.uv1.xy);
					float4 sky = LOAD_TEXTURE2D_X(_Skybox, positionSS1);
				#endif

					//half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv.xy);
					half4 tex = LOAD_TEXTURE2D_X(_MainTexA, positionSS);

					/// consider maximum radius
				#if !UNITY_UV_STARTS_AT_TOP
					half2 vec = _SunPosition.xy - i.uv.xy;
				#else
					half2 vec = _SunPosition.xy - i.uv1.xy;
				#endif
				half dist = saturate(_SunPosition.w - length(vec));

				half4 outColor = 0;

				// find unoccluded sky pixels
				// consider pixel values that differ significantly between framebuffer and sky-only buffer as occluded

				if (Luminance(abs(sky.rgb - tex.rgb)) < 0.2) {
					outColor = TransformColor(tex) * dist;
				}

				return outColor * 1;
			}


				/////////////////////////////////////////////////////









			//struct Attributes
			//{
			//	uint vertexID : SV_VertexID;
			//	UNITY_VERTEX_INPUT_INSTANCE_ID
			//};

			//struct Varyings
			//{
			//	float4 positionCS : SV_POSITION;
			//	float2 texcoord   : TEXCOORD0;
			//	UNITY_VERTEX_OUTPUT_STEREO
			//};



			//// List of properties to control your post process effect
			float _Intensity;
			TEXTURE2D_X(_InputTexture);
			float scalerMask = 1;

			float4 CustomPostProcess(VaryingsB input) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

				uint2 positionSS = input.uv * scalerMask * _ScreenSize.xy;
				float4 outColor = LOAD_TEXTURE2D_X(_InputTexture, positionSS);

				return outColor;// float4(lerp(outColor, Luminance(outColor).xxx, _Intensity), 1);
			}


				/////////////////////// WATER MASK ///////////////////////

			float _CamXRot;
			float waterHeight = 0;

			//TEXTURE2D_X(_InputTexture);
			float4 _InputTexture_TexelSize;

			float4 waterMaskFrag(VaryingsB i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

				uint2 positionSS = i.uv * _ScreenSize.xy;
				float4 mainTex = LOAD_TEXTURE2D_X(_InputTexture, positionSS);

				////////////////// MASK		

				//v4.4
				//half4 maskFRONT = tex2D(waterFrontRender, i.uv + float2(0, -0.015));

				//v4.4
				//half4 mainTex = tex2D(_MainTex, i.uv);
				//float4 _MainTex_TexelSize = _InputTexture_TexelSize;

				half4 newTex = float4(0, 0, 0, 0);
				half4 mainTexUP = float4(0, 0, 0, 0);
				bool whiteFound = false;

				[loop]
				for (float j = 0; j < 1 * _ScreenSize.y; j = j + _InputTexture_TexelSize.y* _ScreenSize.y) {
					if (j > positionSS.y) {
						mainTexUP = LOAD_TEXTURE2D_X(_InputTexture, float2(positionSS.x, j)); //SAMPLE PIXELS ABOVE
						if (mainTexUP.r == 0 && mainTexUP.g == 0 && mainTexUP.b == 0) {
							newTex = float4(0, 1, 0, 1);
							whiteFound = true;
							//break;
						}
						//float offsetLR = 8*_InputTexture_TexelSize.x* _ScreenSize.x;
						//mainTexUP = LOAD_TEXTURE2D_X(_InputTexture, float2(positionSS.x + offsetLR, j));
						//if (mainTexUP.r == 0 && mainTexUP.g == 0 && mainTexUP.b == 0) {
						//	newTex = float4(0, 1, 0, 1);
						//	whiteFound = true;
						//	//break;
						//}
						//mainTexUP = LOAD_TEXTURE2D_X(_InputTexture, float2(positionSS.x - offsetLR, j));
						//if (mainTexUP.r == 0 && mainTexUP.g == 0 && mainTexUP.b == 0) {
						//	newTex = float4(0, 1, 0, 1);
						//	whiteFound = true;
						//	//break;
						//}
					}
					if (j < positionSS.y) {
						mainTexUP = LOAD_TEXTURE2D_X(_InputTexture, float2(positionSS.x, j)); //SAMPLE PIXELS ABOVE
						if (mainTexUP.r == 0 && mainTexUP.g == 0 && mainTexUP.b == 0) {
							whiteFound = true;
							//break;
						}
						//float offsetLR = 8* _InputTexture_TexelSize.x* _ScreenSize.x;
						//mainTexUP = LOAD_TEXTURE2D_X(_InputTexture, float2(positionSS.x + offsetLR, j));
						//if (mainTexUP.r == 0 && mainTexUP.g == 0 && mainTexUP.b == 0) {
						//	//newTex = float4(0, 1, 0, 1);
						//	whiteFound = true;
						//	//break;
						//}
						//mainTexUP = LOAD_TEXTURE2D_X(_InputTexture, float2(positionSS.x - offsetLR, j));
						//if (mainTexUP.r == 0 && mainTexUP.g == 0 && mainTexUP.b == 0) {
						//	//newTex = float4(0, 1, 0, 1);
						//	whiteFound = true;
						//	//break;
						//}
					}
				}
				//float3 camPlaneNormal = mul((float3x3)unity_CameraToWorld, float3(0, 0, 1));
				//float3 viewDir = UNITY_MATRIX_IT_MV[2].xyz;

				if (!whiteFound) {
					bool FoundLeftUP = false;
					bool  FoundRightUP = false;
					int foundDOWN = 0;
					[loop]
					for (float j = 0; j < 1 * _ScreenSize.x; j = j + _InputTexture_TexelSize.x* _ScreenSize.x) {
						mainTexUP = LOAD_TEXTURE2D_X(_InputTexture, float2(j, 0.05));	//SAMPLE UPPER LINE	
						if (mainTexUP.r == 0 && mainTexUP.g == 0 && mainTexUP.b == 0) {
							if (j > positionSS.x) {
								FoundRightUP = true; foundDOWN = -1; break;
							}
							if (j < positionSS.x) {
								FoundLeftUP = true; foundDOWN = -1; break;
							}
						}
						mainTexUP = LOAD_TEXTURE2D_X(_InputTexture, float2(j, 0.95 * _ScreenSize.y)); //SAMPLE LOWER LINE		
						if (mainTexUP.r == 0 && mainTexUP.g == 0 && mainTexUP.b == 0) {
							if (j > positionSS.x) {
								FoundRightUP = true; foundDOWN = 1; break;
							}
							if (j < positionSS.x) {
								FoundLeftUP = true; foundDOWN = 1; break;
							}
						}
					}

					if (FoundLeftUP) {
						if (foundDOWN > 0)//if (waveHeightSampler != null && middleDist > 0) //if we are above water, assume white line is below us, so make it all black
						{
							newTex = float4(0, 1, 0, 1);//float4(1, 0, 0, 1);
						}
						else if (foundDOWN < 0) {
							newTex = float4(0, 0, 0, 1);
						}
					}
					if (FoundRightUP) {
						if (foundDOWN > 0)//if (waveHeightSampler != null && middleDist > 0) //if we are above water, assume white line is below us, so make it all black
						{
							newTex = float4(0, 1, 0, 1);//float4(0, 0, 1, 1);
						}
						else if (foundDOWN < 0) {
							newTex = float4(0, 0, 0, 1);
						}
					}

					float _toWaveHeight = _WorldSpaceCameraPos.y - waterHeight;// _Intensity * 20 - 10;

					if (!FoundLeftUP) {
						if (!FoundRightUP) {
							if (_toWaveHeight > 0) {
								//newTex = float4(0, 0, 0, 1);
							}
							if (_toWaveHeight < 0) {//  && _CamXRot > 0) {

								newTex = float4(0,1,0, 1);
							}
						}
					}
				}

				//if (maskFRONT.r > 0) { //if front is white
				///	newTex = float4(0, 0, 0, 1); //make pixel black
				//}

				return half4(newTex.rgb, newTex.a);
				//return half4( mainTex.rgb, newTex.a);
				//return half4(saturate(newTex.rgb) + newTex.rgb * 0.0  + mainTex.rgb, newTex.a);

				///////////////// END MASK

				//return float4(lerp(outColor, Luminance(outColor).xxx, _Intensity), 1);
			}

				/////////////////////// END WATER MASK




				///////////////////// VOLUMETRIC CLOUDS v0.1 /////////////////////////////
				struct Attributes
			{
				float4 positionOS       : POSITION;
				float2 uv               : TEXCOORD0;
			};
			inline half Luminance(half3 rgb)
			{
				//return dot(rgb, unity_ColorSpaceLuminance.rgb);
				return dot(rgb, rgb);
			}
			//v2f vert(Attributes v) {//v2f vert(AttributesDefault v) { //appdata_img v) { //v0.1
			//						//v2f o;
			//	v2f o = (v2f)0;
			//	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

			//	VertexPositionInputs vertexInput = GetVertexPositionInputs(v.positionOS.xyz);

			//	o.pos = float4(vertexInput.positionCS.xyz, 1.0);
			//	float2 uv = v.uv;
			//	o.uv = uv;
			//	o.uv1 = uv.xy;

			//	return o;
			//}

			v2f vert(AttributesB input) {
				v2f output;//v2f_radial output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
				output.pos = GetFullScreenTriangleVertexPosition(input.vertexID);
				output.uv = GetFullScreenTriangleTexCoord(input.vertexID);
				output.uv1 = output.uv;
				return output;
			}

			////////////////// FULL VOLUMETRIC CLOUDS
			struct v2f_FV {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 ray : TEXCOORD1;
			};
			/*struct v2f
			{
			float2 uv : TEXCOORD0;
			float4 pos : SV_POSITION;
			float4 ray : TEXCOORD1;
			};*/

			//		v2f_FV vert_FV(Attributes v) {//v2f vert(AttributesDefault v) { //appdata_img v) {
			//									  //v2f o;
			//			v2f_FV o = (v2f_FV)0;
			//			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
			//
			//			VertexPositionInputs vertexInput = GetVertexPositionInputs(v.positionOS.xyz);
			//
			//			o.pos = float4(vertexInput.positionCS.xyz, 1.0);
			//			//float2 uv = v.uv;
			//			//o.uv = uv;
			//			//o.uv1 = uv.xy;
			//
			//			half index = v.positionOS.z;// v.vertex.z;
			//			v.positionOS.z = 0.1;
			//
			//			//////o.pos = UnityObjectToClipPos(v.vertex);
			//			o.uv = v.uv.xy;
			//
			//#if UNITY_UV_STARTS_AT_TOP
			//			if (_MainTex_TexelSize.y < 0)
			//				o.uv.y = 1 - o.uv.y;
			//#endif
			//
			//			// Get the eyespace view ray (normalized)
			//			o.ray = _FrustumCornersES[(int)index];
			//			// Dividing by z "normalizes" it in the z axis
			//			// Therefore multiplying the ray by some number i gives the viewspace position
			//			// of the point on the ray with [viewspace z]=i
			//			o.ray /= abs(o.ray.z);
			//
			//			// Transform the ray from eyespace to worldspace
			//			o.ray = mul(_CameraInvViewMatrix, o.ray);
			//
			//			return o;
			//		}

			v2f_FV vert_FV(AttributesB input) {
				v2f_FV output;//v2f_radial output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
				output.pos = GetFullScreenTriangleVertexPosition(input.vertexID);
				output.uv = GetFullScreenTriangleTexCoord(input.vertexID);
				//output.uv1 = output.uv;

				half index = output.pos.z; // v.positionOS.z;// v.vertex.z;
				//v.positionOS.z = 0.1;

				//////o.pos = UnityObjectToClipPos(v.vertex);
				//output.uv = v.uv.xy;

				/*#if UNITY_UV_STARTS_AT_TOP
							if (_MainTex_TexelSize.y < 0)
								output.uv.y = 1 - output.uv.y; //v0.1 removed from URP
				#endif*/

				// Get the eyespace view ray (normalized)
				output.ray = _FrustumCornersES[(int)index];
				// Dividing by z "normalizes" it in the z axis
				// Therefore multiplying the ray by some number i gives the viewspace position
				// of the point on the ray with [viewspace z]=i
				output.ray /= abs(output.ray.z);

				// Transform the ray from eyespace to worldspace
				output.ray = mul(_CameraInvViewMatrix, output.ray);

				return output;
			}

			/// FRAG FULL VOLUME CLOUDS
			// http://momentsingraphics.de/?p=127#jittering
			float getRandomRayOffset(float2 uv) // uses blue noise texture to get random ray offset
			{
				float noise = tex2D(_BlueNoise, uv).x;
				noise = mad(noise, 2.0, -1.0);
				return noise;
			}

			// http://byteblacksmith.com/improvements-to-the-canonical-one-liner-glsl-rand-for-opengl-es-2-0/
			float rand(float2 co) {
				float a = 12.9898;
				float b = 78.233;
				float c = 43758.5453;
				float dt = dot(co.xy, float2(a, b));
				float sn = fmod(dt, 3.14);

				return 2.0 * frac(sin(sn) * c) - 1.0;
			}

			float weatherDensity(float3 weatherData) // Gets weather density from weather texture sample and adds 1 to it.
			{
				return weatherData.b + 1.0;
			}

			// from GPU Pro 7 - remaps value from one range to other range
			float remap(float original_value, float original_min, float original_max, float new_min, float new_max)
			{
				return new_min + (((original_value - original_min) / (original_max - original_min)) * (new_max - new_min));
			}

			// returns height fraction [0, 1] for point in cloud
			float getHeightFractionForPoint(float3 pos)
			{

				//v0.7 VORTEX
				//float4 vortexPosRadius = vortexPosRadius;// float4(0, 0, 0, 1100); //v0.8
				float distanceToVortexCenter = length(vortexPosRadius.xz - pos.xz);
				float4 thick = _Thickness;
				if (distanceToVortexCenter < vortexPosRadius.w) { //if (distanceToVortexCenter < 280000) { //v0.8
					//pos.y = pos.y + 1000*distanceToVortexCenter;
////////////					thick = _Thickness * 5;
				}

				return ((distance(pos, _PlanetCenter) - (_SphereSize + _CloudHeightMinMax.x)) / thick);
			}

			// samples the gradient
			float sampleGradient(float4 gradient, float height)
			{
				return smoothstep(gradient.x, gradient.y, height) - smoothstep(gradient.z, gradient.w, height);
			}

			// lerps between cloud type gradients and samples it
			float getDensityHeightGradient(float height, float3 weatherData)
			{
				float type = weatherData.g;
				float4 gradient = lerp(lerp(_Gradient1, _Gradient2, type * 2.0), _Gradient3, saturate((type - 0.5) * 2.0));
				return sampleGradient(gradient, height);
			}

			//v0.7a - VORTEX
			float3x3 rotationMatrix(float3 axis, float angle)
			{
				axis = normalize(axis);
				float s = sin(angle);
				float c = cos(angle);
				float oc = 1.0 - c;

				return float3x3 (oc * axis.x * axis.x + c, oc * axis.x * axis.y - axis.z * s, oc * axis.z * axis.x + axis.y * s,
					oc * axis.x * axis.y + axis.z * s, oc * axis.y * axis.y + c, oc * axis.y * axis.z - axis.x * s,
					oc * axis.z * axis.x - axis.y * s, oc * axis.y * axis.z + axis.x * s, oc * axis.z * axis.z + c);
			}

			// samples weather texture
			float3 sampleWeather(float3 pos) {

				//v0.7
				//float4 vortexPosRadius = float4(0, 0, 0, -1100);
				float distanceToVortexCenter = length(vortexPosRadius.xz - pos.xz);
				//if (distanceToVortexCenter < vortexPosRadius.w) {//if (distanceToVortexCenter < 280000) {
					//pos.y = pos.y + 1000*distanceToVortexCenter;
				//}



				float3 weatherData = tex2Dlod(_WeatherTexture, float4((pos.xz + _CoverageWindOffset) * _WeatherScale, 0, 0)).rgb;



				//v0.7
				float3x3 rotator = rotationMatrix(float3(0, 1, 0), 1 * (_Time.y * vortexControlsA.x));
				float3 posVertex = mul(float3(pos.x, 0, pos.z), rotator);
				float3 weatherData2 = tex2Dlod(_WeatherTexture, float4((posVertex.xz + _CoverageWindOffset) * _WeatherScale, 0, 0)).rgb;
				//if (distanceToVortexCenter > 0) {				
					//weatherData.r = 1;
				//
				if (distanceToVortexCenter > vortexPosRadius.w) {//if (distanceToVortexCenter > 280000) { //280000 /110000 //vortexPosRadius
					//posVertex = pos;
					//weatherData.r = 1;
				}
				else {
					weatherData = lerp(weatherData2, weatherData, 1 - saturate(distanceToVortexCenter / 1000));
					weatherData.r *= 14;
				}
				//v0.7a
				//weatherData.r = weatherData.r + (distanceToVortexCenter / 100000) - 0.2;
				weatherData.r = (weatherData.r - _Coverage);



				float reduceonZero = 1;
				if (_maskScale > 0) {
					//float3 maskData = tex2Dlod(_maskTexture, float4((pos.xz) * 0.00000125  + _maskPos.xz* _maskScale, 0, 0)).rgb;//v0.1.1
					float3 maskData = tex2Dlod(_maskTexture, float4((pos.xz) * 0.00000125* _maskScale + float2(0.5, 0.5), 0, 0)).rgb;
					weatherData.r = weatherData.r * (1 - maskData.r);
				}

				if (fogOfWarRadius > 0) {
					float distAN = length(playerPos.xz * cameraScale - pos.xz);
					if (distAN > fogOfWarRadius) {
						//weatherData.r =  playerPos.w / distAN;// fogOfWarRadius;
						if (weatherData.r < 0.1) {
							reduceonZero = fogOfWarRadius;
						}
						weatherData.r = weatherData.r + 0.000001*(playerPos.w * distAN *(1 - 0.1*(1 - weatherData.r)));// -fogOfWarRadius;
						//weatherData.g = weatherData.b = 1;
						//weatherData.g = 1-weatherData.r;

						weatherData.r = weatherData.r / reduceonZero;
					}
				}

				//v0.2
				float4 texInteract = tex2Dlod(_InteractTexture, 0.0003*float4(
					//_InteractTexturePos.x*pos.x + _InteractTexturePos.z*-_Scroll1.x * _Time.x + _InteractTextureOffset.x,
					//_InteractTexturePos.y*pos.z + _InteractTexturePos.w*-_Scroll1.z * _Time.x + _InteractTextureOffset.y,
					_InteractTexturePos.x*pos.x + _InteractTexturePos.z * _Time.x + _InteractTextureOffset.x,
					_InteractTexturePos.y*pos.z + _InteractTexturePos.w * _Time.x + _InteractTextureOffset.y,
					0, 0));
				float3 _LocalLightPos = float3(0, 0, 0);
				float diffPos = length(_LocalLightPos.xyz - pos);
				texInteract.a = texInteract.a + clamp(_InteractTextureAtr.z * 0.1*(1 - 0.00024*diffPos), -1.5, 0);
				weatherData = weatherData * clamp(texInteract.a*_InteractTextureAtr.w, _InteractTextureAtr.y, 1);
				//  _NoiseAmp2 = _NoiseAmp2*clamp(texInteract.a*_InteractTextureAtr.w,_InteractTextureAtr.y,1);
				//weatherData.r = 1;
				return weatherData;
			}

			// samples cloud density
			float sampleCloudDensity(float3 p, float heightFraction, float3 weatherData, float lod, bool sampleDetail)
			{
				float3 pos = p + _WindOffset; // add wind offset
				pos += heightFraction * _WindDirection * 700.0; // shear at higher altitude

#if defined(DEBUG_NO_LOW_FREQ_NOISE)
				float cloudSample = 0.7;
				cloudSample = remap(cloudSample, _LowFreqMinMax.x, _LowFreqMinMax.y, 0.0, 1.0);
#else
			//float cloudSample = tex3Dlod(_ShapeTexture, float4(pos * _Scale, lod)).r; // sample cloud shape texture


			//v0.7a
				//float4 vortexPosRadius = float4(0, 0, 0, 1000);
				float distanceToVortexCenter = length(vortexPosRadius.xz - pos.xz);
				float3x3 rotator = rotationMatrix(float3(0, 1, 0), 1 * ((_Time.y * 2* vortexControlsA.y) - 700 * (distanceToVortexCenter / 10000000)));
				float3 posVertex = mul(float3(pos.x, 0, pos.z), rotator);
				posVertex.y = pos.y;
				if (distanceToVortexCenter > vortexPosRadius.w) {//if (distanceToVortexCenter > 150000) { //v0.8
					posVertex = pos;
				}
				//posVertex.y = posVertex.y - (_Time.y * 12);
				float cloudSample = tex3Dlod(_ShapeTexture, float4(posVertex * _Scale, lod)).r;



				cloudSample = remap(cloudSample * pow(1.2 - heightFraction, 0.1), _LowFreqMinMax.x, _LowFreqMinMax.y, 0.0, 1.0); // pick certain range from sample texture
#endif

			//v0.7
			//if (pos.y < 130000 || pos.y > 200000 || distanceToVortexCenter < 150000) {
				cloudSample *= getDensityHeightGradient(heightFraction, weatherData); // multiply cloud by its type gradient
			//}

				float cloudCoverage = weatherData.r;
				cloudSample = saturate(remap(cloudSample, saturate(heightFraction / cloudCoverage), 1.0, 0.0, 1.0)); // Change cloud coverage based by height and use remap to reduce clouds outside coverage
				cloudSample *= cloudCoverage; // multiply by cloud coverage to smooth them out, GPU Pro 7

#if defined(DEBUG_NO_HIGH_FREQ_NOISE)
				cloudSample = remap(cloudSample, 0.2, 1.0, 0.0, 1.0);
#else
				if (cloudSample > 0.0 && sampleDetail) // If cloud sample > 0 then erode it with detail noise
				{
#if defined(DEBUG_NO_CURL)
#else
					float3 curlNoise = mad(tex2Dlod(_CurlNoise, float4(p.xz * _CurlDistortScale, 0, 0)).rgb, 2.0, -1.0); // sample Curl noise and transform it from [0, 1] to [-1, 1]
					pos += float3(curlNoise.r, curlNoise.b, curlNoise.g) * heightFraction * _CurlDistortAmount; // distort position with curl noise
#endif
					float detailNoise = tex3Dlod(_DetailTexture, float4(pos * _DetailScale, lod)).r; // Sample detail noise

					float highFreqNoiseModifier = lerp(1.0 - detailNoise, detailNoise, saturate(heightFraction * 10.0)); // At lower cloud levels invert it to produce more wispy shapes and higher billowy

					cloudSample = remap(cloudSample, highFreqNoiseModifier * _HighFreqModifier, 1.0, 0.0, 1.0); // Erode cloud edges
				}
#endif

				return max(cloudSample * _SampleMultiplier, 0.0);
			}




			// GPU Pro 7
			float beerLaw(float density)
			{
				float d = -density * _Density;
				return max(exp(d), exp(d * 0.5)*0.7);
			}

			// GPU Pro 7
			float HenyeyGreensteinPhase(float cosAngle, float g)
			{
				float g2 = g * g;
				return ((1.0 - g2) / pow(1.0 + g2 - 2.0 * g * cosAngle, 1.5)) / 4.0 * 3.1415;
			}

			// GPU Pro 7
			float powderEffect(float density, float cosAngle)
			{
				float powder = 1.0 - exp(-density * 2.0);
				return lerp(1.0f, powder, saturate((-cosAngle * 0.5f) + 0.5f));
			}

			float calculateLightEnergy(float density, float cosAngle, float powderDensity) { // calculates direct light components and multiplies them together
				float beerPowder = 2.0 * beerLaw(density) * powderEffect(powderDensity, cosAngle);
				float HG = max(HenyeyGreensteinPhase(cosAngle, _HenyeyGreensteinGForward), HenyeyGreensteinPhase(cosAngle, _HenyeyGreensteinGBackward)) * 0.07 + 0.8;
				return beerPowder * HG;
			}

			float randSimple(float n) // simple hash function for more random light vectors
			{
				return mad(frac(sin(n) * 43758.5453123), 2.0, -1.0);
			}

			float3 rand3(float3 n) // random vector
			{
				return normalize(float3(randSimple(n.x), randSimple(n.y), randSimple(n.z)));
			}

			float3 sampleConeToLight(float3 pos, float3 lightDir, float cosAngle, float density, float3 initialWeather, float lod)
			{
#if defined(RANDOM_UNIT_SPHERE)
#else
				const float3 RandomUnitSphere[5] = // precalculated random vectors
				{
					{ -0.6, -0.8, -0.2 },
				{ 1.0, -0.3, 0.0 },
				{ -0.7, 0.0, 0.7 },
				{ -0.2, 0.6, -0.8 },
				{ 0.4, 0.3, 0.9 }
				};
#endif
				float heightFraction;
				float densityAlongCone = 0.0;
				const int steps = 5; // light cone step count
				float3 weatherData;
				for (int i = 0; i < steps; i++) {
					pos += lightDir * _LightStepLength; // march forward
#if defined(RANDOM_UNIT_SPHERE) // apply random vector to achive cone shape
					float3 randomOffset = rand3(pos) * _LightStepLength * _LightConeRadius * ((float)(i + 1));
#else
					float3 randomOffset = RandomUnitSphere[i] * _LightStepLength * _LightConeRadius * ((float)(i + 1));
#endif
					float3 p = pos + randomOffset; // light sample point
												   // sample cloud
					heightFraction = getHeightFractionForPoint(p);
					weatherData = sampleWeather(p);
					densityAlongCone += sampleCloudDensity(p, heightFraction, weatherData, lod + ((float)i) * 0.5, true) * weatherDensity(weatherData);
				}

#if defined(SLOW_LIGHTING) // if doing slow lighting then do more samples in straight line
				pos += 24.0 * _LightStepLength * lightDir;
				weatherData = sampleWeather(pos);
				heightFraction = getHeightFractionForPoint(pos);
				densityAlongCone += sampleCloudDensity(pos, heightFraction, weatherData, lod, true) * 2.0;
				int j = 0;
				while (1) {
					if (j > 22) {
						break;
					}
					pos += 4.25 * _LightStepLength * lightDir;
					weatherData = sampleWeather(pos);
					if (weatherData.r > 0.05) {
						heightFraction = getHeightFractionForPoint(pos);
						densityAlongCone += sampleCloudDensity(pos, heightFraction, weatherData, lod, true);
					}

					j++;
				}
#else
				pos += 32.0 * _LightStepLength * lightDir; // light sample from further away
				weatherData = sampleWeather(pos);
				heightFraction = getHeightFractionForPoint(pos);
				densityAlongCone += sampleCloudDensity(pos, heightFraction, weatherData, lod + 2, false) * weatherDensity(weatherData) * 3.0;
#endif

				return calculateLightEnergy(densityAlongCone, cosAngle, density) * _SunColor;
			}

			// raymarches clouds
			// samples cloud density
			float sampleCloudDensity1(float3 p, float heightFraction, float3 weatherData, float lod, bool sampleDetail)
			{
				float3 pos = p;

				//float cloudSample = tex3Dlod(_ShapeTexture, float4(pos * _Scale * 10000, lod)).r; 
				float cloudSample = tex3Dlod(_ShapeTexture, float4(pos * 0.00001, 0)).r;
				float cloudCoverage = 1;// weatherData.r;
				cloudSample = saturate(remap(cloudSample, (heightFraction / cloudCoverage), 1.0, 0.0, 1.0)); // Change cloud coverage based by height and use remap to reduce clouds outside coverage


				return cloudSample * _SampleMultiplier;
			}
			// raymarches clouds
			float4 raymarch1(float3 ro, float3 rd, float steps, float depth, float cosAngle, float2 duv)
			{
				float3 pos = ro;
				float4 res = 0.0; // cloud color
				float lod = 0.0;
				float zeroCount = 0.0; // number of times cloud sample has been 0
				float stepLength = 2; // step length multiplier, 1.0 when doing small steps
									  //[loop]
				for (float i = 0.0; i < 151; i += stepLength)
				{
					float heightFraction = getHeightFractionForPoint(pos);
					float3 weatherData = float3(1, 1, 1); //sampleWeather(pos); // sample weather			
					float cloudDensity = 0 * saturate(sampleCloudDensity(pos, heightFraction, weatherData, lod, true)); // sample the cloud

					if (cloudDensity >= 0.0)
					{
						zeroCount = 0.0; // set zero cloud density counter to 0
						float4 particle = cloudDensity; // construct cloud particle				
						res = particle + res; // use premultiplied alpha blending to acumulate samples	
					}
					//stepLength = zeroCount > 10.0 ? BIG_STEP : 1.0; // check if we need to do big or small steps
					pos += rd * stepLength; // march forward
				}
				//return float4(r, 1);
				return res;
			}


			float3 sampleConeToLightA(float3 pos, float3 lightDir, float cosAngle, float density, float3 initialWeather, float lod)
			{
#if defined(RANDOM_UNIT_SPHERE)
#else
				const float3 RandomUnitSphere[5] = // precalculated random vectors
				{
					{ -0.6, -0.8, -0.2 },
				{ 1.0, -0.3, 0.0 },
				{ -0.7, 0.0, 0.7 },
				{ -0.2, 0.6, -0.8 },
				{ 0.4, 0.3, 0.9 }
				};
#endif
				float heightFraction;
				float densityAlongCone = 0.0;
				const int steps = sunShaftSteps;// 5; // light cone step count
				float3 weatherData;
				//		for (int i = 0; i < steps; i++) {
				//			//pos += lightDir * _LightStepLength; // march forward
				//#if defined(RANDOM_UNIT_SPHERE) // apply random vector to achive cone shape
				//			float3 randomOffset = rand3(pos) * _LightStepLength * _LightConeRadius * ((float)(i + 1));
				//#else
				//			float3 randomOffset = RandomUnitSphere[i] * _LightStepLength * _LightConeRadius * ((float)(i + 1));
				//#endif
				//			float3 p = pos + randomOffset * 10; // light sample point
				//										   // sample cloud
				//			heightFraction = getHeightFractionForPoint(p);
				//			weatherData = sampleWeather(p);
				//			//densityAlongCone += sampleCloudDensity(p, heightFraction, weatherData, lod + ((float)i) * 0.5, true) * weatherDensity(weatherData);
				//			//densityAlongCone += sampleCloudDensity(pos, heightFraction, weatherData, lod + 2, false) * weatherDensity(weatherData) * 3.0;
				//		}
				for (int i = 0; i < steps; i++) {
					pos += 32.0 * _LightStepLength * lightDir * pow(float(i + 1), 2); // light sample from further away
					weatherData = sampleWeather(pos);
					heightFraction = getHeightFractionForPoint(pos);
					densityAlongCone += sampleCloudDensity(pos, heightFraction, weatherData, lod + 2, false) * weatherDensity(weatherData) * 3.0;
					//_LightStepLength = _LightStepLength - 64 * (i + 1);
				}

				//pos += 132.0 * _LightStepLength * lightDir; // light sample from further away
				//weatherData = sampleWeather(pos);
				//heightFraction = getHeightFractionForPoint(pos);
				//densityAlongCone += sampleCloudDensity(pos, heightFraction, weatherData, lod + 2, false) * weatherDensity(weatherData) * 3.0;

				//pos += 432.0 * _LightStepLength * lightDir; // light sample from further away
				//weatherData = sampleWeather(pos);
				//heightFraction = getHeightFractionForPoint(pos);
				//densityAlongCone += sampleCloudDensity(pos, heightFraction, weatherData, lod + 2, false) * weatherDensity(weatherData) * 3.0;

				return calculateLightEnergy(densityAlongCone, cosAngle, density) * _SunColor;
			}


			float BeerPowderA(float depth)
			{
				return exp(-_Extinct * depth) * (1 - exp(-_Extinct * 2 * depth));
			}


			// https://www.scratchapixel.com/lessons/3d-basic-rendering/minimal-ray-tracer-rendering-simple-shapes/ray-sphere-intersection
			float3 findRayStartPos(float3 rayOrigin, float3 rayDirection, float3 sphereCenter, float radius)
			{
				float3 l = rayOrigin - sphereCenter;
				float a = 1.0;
				float b = 2.0 * dot(rayDirection, l);
				float c = dot(l, l) - pow(radius, 2);
				float D = pow(b, 2) - 4.0 * a * c;
				if (D < 0.0)
				{
					return rayOrigin;
				}
				else if (abs(D) - 0.00005 <= 0.0)
				{
					return rayOrigin + rayDirection * (-0.5 * b / a);
				}
				else
				{
					float q = 0.0;
					if (b > 0.0)
					{
						q = -0.5 * (b + sqrt(D));
					}
					else
					{
						q = -0.5 * (b - sqrt(D));
					}
					float h1 = q / a;
					float h2 = c / q;
					float2 t = float2(min(h1, h2), max(h1, h2));
					if (t.x < 0.0) {
						t.x = t.y;
						if (t.x < 0.0) {
							return rayOrigin;
						}
					}
					return rayOrigin + t.x * rayDirection;
				}
			}


			float4 raymarch(float3 ro, float3 rd, float steps, float depth, float cosAngle, float2 duv)
			{
				float3 pos = ro;
				float4 res = 0.0; // cloud color
				float lod = 0.0;
				float zeroCount = 0.0; // number of times cloud sample has been 0
				float stepLength = BIG_STEP; // step length multiplier, 1.0 when doing small steps


				for (float i = 0.0; i < steps; i += stepLength)
				{
					if (distance(_CameraWS, pos) >= depth || res.a >= 0.99) { // check if is behind some geometrical object or that cloud color aplha is almost 1
																			  //break;  // if it is then raymarch ends

																			  //v0.1 - add option to rernder in front of all objects for reflections
						if (_renderInFront == 0) {
							break;
						}
					}
					float heightFraction = getHeightFractionForPoint(pos);
#if defined(ALLOW_IN_CLOUDS) // if it is allowed to fly in the clouds, then we need to check that the sample position is above the ground and in the cloud layer
					if (pos.y < _ZeroPoint.y || heightFraction < 0.0 || heightFraction > 1.0) {
						//break; //v0.1
					}
#endif
					float3 weatherData = sampleWeather(pos); // sample weather
					if (_SunDir.y > 0 && weatherData.r <= 0.1) // if value is low, then continue marching, at some specific weather textures makes it a bit faster. //v0.6a
					{
						pos += rd * stepLength;
						zeroCount += 1.0;
						stepLength = zeroCount > 10.0 ? BIG_STEP : 1.0;
						continue;
					}

					float cloudDensity = saturate(sampleCloudDensity(pos, heightFraction, weatherData, lod, true)); // sample the cloud

					if (cloudDensity > 0.0) // check if cloud density is > 0 //NASOS >=
					{

						//NASOS
						//		float adder1 = 0;
						//		if (cloudDensity == 0.0) {
						//cloudDensity = 0.004*(pow(dot(_SunDir, pos),2))*0.0004 * 0.00005;// *length(_SunDir - pos) * 0.00002;
						//float3 directLightA = sampleConeToLight(pos, _SunDir, cosAngle, cloudDensity, weatherData, lod);
						//			cloudDensity = saturate(0.004*(pow(dot(_SunDir, pos), 1))*0.0002 - 0.0000001*length(_SunDir - pos)) + 0.002;//0.004;

						//	float3 directLightA = sampleConeToLight(pos, _SunDir, cosAngle, cloudDensity, weatherData, lod);
						//cloudDensity =(( cloudDensity * directLightA.r*directLightA.g*directLightA.b))*0.4;
						//cloudDensity = (cloudDensity / (directLightA.r))*1.4;
						//cloudDensity = (cloudDensity *2/(directLightA.b + directLightA.r));

						//	cloudDensity = 0.008;

						//			zeroCount += 1.0;
						//	adder1 = 1;
						//break;
						//			break;
						//		}

						zeroCount = 0.0; // set zero cloud density counter to 0

						if (stepLength > 1.0) // if we did big steps before
						{
							i -= stepLength - 1.0; // then move back, previous 0 density location + one small step
							pos -= rd * (stepLength - 1.0);
							weatherData = sampleWeather(pos); // sample weather
							cloudDensity = saturate(sampleCloudDensity(pos, heightFraction, weatherData, lod, true)); // and cloud again
						}

						float4 particle = cloudDensity; // construct cloud particle
						float3 directLight = sampleConeToLight(pos, _SunDir, cosAngle, cloudDensity, weatherData, lod); // calculate direct light energy and color
						float3 ambientLight = lerp(_CloudBaseColor, _CloudTopColor, heightFraction); // and ambient

						directLight *= _SunLightFactor; // multiply them by their uniform factors


														//if (adder1 == 1) {
														//directLight = directLight * 0.45 + directLight / directLight.r*0.6;
														//particle = particle * directLight.r * directLight.g*2;
														//}


						ambientLight *= _AmbientLightFactor;

						//NASOS - Local Lights - _LocalLightColor
						//float divider = length(pos - float3(0, 3000+1010*cos(_Time.y*5), 0)); //v5.0.1
						float divider = length(pos - _LocalLightPos.xyz); //v5.0.1
																		  //float colorSample = 111111111;
																		  //float colorSample = pow(10, 8);
						float colorSample = pow(10, 8);
						//particle.rgb = directLight + ambientLight + cloudDensity*float3(colorSample, 0, colorSample + pow(10, 8) * cos(_Time.y * 8)) / (pow(divider,2.5));// +10 / (abs(pos.xyz - float3(0, 0, 0))); // add lights up and set cloud particle color
						//v5.0.1
						particle.rgb = directLight + ambientLight + cloudDensity * _LocalLightColor.rgb * colorSample / (pow(divider, _LocalLightColor.w));// +10 / (abs(pos.xyz - float3(0, 0, 0))); // add lights up and set cloud particle color


						particle.rgb *= particle.a; // multiply color by clouds density
						res = (1.0 - res.a) * particle + res; // use premultiplied alpha blending to acumulate samples

															  //NASOS
															  //res = res + (dot(_SunDir, pos))*0.00001;

					}
					else // if cloud sample was 0, then increase zero cloud sample counter
					{

						if (sunRaysOn == 4) {
							//float4 raysResolution = float4(1,1,1,0);
							//float4 rayShadowing = float4(1, 1, 1, 1);
							int _SampleCountL = sunShaftSteps;
							float lightAtten = rayShadowing.z;
							float3 acc = float3(0, 0, 0);
							if (raysResolution.x > 0) {// && abs(lightAtten > 0.5 * rayShadowing.y)) { //else if (raysResolution.x > 0){

													   //float rand = UVRandom(uv + s + 1);
													   //float3 rand = rand3(posA) * _LightStepLength * _LightConeRadius * ((float)(i + 1));
								float divider = 24 / (raysResolution.x);
								float3 light = _SunDir;

								//float strideA = (_CloudHeightMinMax.y - pos.y) / (abs(light.y) * _SampleCountL);    //(_Altitude1 - pos.y) / (abs(light.y) * _SampleCountL);
								//float strideA = (_CloudHeightMinMax.x - pos.y) / (abs(light.y) * _SampleCountL); //v0.6
								float strideA = (_CloudHeightMinMax.x - pos.y) / ((light.y) * _SampleCountL); //v0.6a

								float3 posA = pos;
								posA += light * strideA * divider * 2 * raysResolution.y;
								float AdjustAtten = lightAtten - 0.5 * rayShadowing.y;

								float dist = length(pos.xyz + light * 10000);

								float depth = 0;
								if (raysResolution.w == 0) {
									weatherData = sampleWeather(posA); // sample weather
									heightFraction = getHeightFractionForPoint(posA);
									//cloudDensity = saturate(sampleCloudDensity(posA, heightFraction, weatherData, lod, true));

									float cloudSample = tex3Dlod(_ShapeTexture, float4(pos * 0.00001, 0)).r;
									float cloudCoverage = weatherData.r;
									//cloudDensity = heightFraction / cloudCoverage;// saturate(remap(cloudSample, (heightFraction / cloudCoverage), 1.0, 0.0, 1.0));
									cloudDensity = saturate(remap(cloudSample, (heightFraction / cloudCoverage), 1.0, 0.0, 1.0));

									depth += cloudDensity * strideA * rayShadowing.x * raysResolution.z * 1;//SampleNoise(posA) * strideA * rayShadowing.x * raysResolution.z * 1;
								}
								else if (raysResolution.w == 1) {
									weatherData = sampleWeather(posA); //weatherData = sampleWeather(posA); 
									heightFraction = getHeightFractionForPoint(posA);
									float cloudSample = (pow(weatherData.g, 134) + weatherData.b) * 1; // tex3Dlod(_ShapeTexture, float4(pos * 0.00001, 0)).r;
									float cloudCoverage = cloudSample;
									cloudDensity = cloudSample;// saturate(remap(cloudSample, (heightFraction / cloudCoverage), 1.0, 0.0, 1.0));
									depth += cloudDensity * strideA * rayShadowing.x * raysResolution.z * 1;
								}
								else if (raysResolution.w == 2) {// && dist < 5200000) { //sample weather map exactly where needed
																 //_PlanetCenter, _SphereSize
																 //float sign = 1;
																 //float distanceCameraPlanet = distance(_CameraWS, _PlanetCenter);
																 //if (distanceCameraPlanet < _SphereSize + _CloudHeightMinMax.x - 0) {
																 //}
									float sign = 1;
									float sign2 = 0;
									rayShadowing.x = 2; rayShadowing.y = 1; rayShadowing.z = 5 * rayShadowing.w;
									raysResolution.y = 1; raysResolution.z = 1;
									float distanceCameraPlanet = distance(_CameraWS, _PlanetCenter);
									if (distanceCameraPlanet < _SphereSize + _CloudHeightMinMax.x - 0) {//if inder clouds
										sign = -1;
										sign2 = 1;
										rayShadowing.x = -1.3; rayShadowing.y = 4.3; rayShadowing.z = 11.04;
										raysResolution.y = 15; raysResolution.z = 14;
									}
									//rayShadowing.z = 0;
									rayShadowing.z = 0.56*rayShadowing.z * (pow(dot(pos.xyz / length(pos.xyz), light / length(light)), 1.05) + 0.6) - 2;
									//float3 lightPos = _PlanetCenter + light * 10000;
									//if (length( lightPos) < length( pos - lightPos)) {
									//rayShadowing.z = 0;
									//}
									AdjustAtten = rayShadowing.z - 0.5 * rayShadowing.y;

									//strideA = findRayStartPos(float3 rayOrigin, float3 rayDirection, float3 sphereCenter, float radius);//(_CloudHeightMinMax.y - pos.y) / (abs(light.y) * _SampleCountL); 
									//strideA = length( findRayStartPos(pos, -light, _PlanetCenter, _SphereSize + _CloudHeightMinMax.x) - pos);
									strideA = length(findRayStartPos(pos, sign*light, _PlanetCenter, _SphereSize + _CloudHeightMinMax.x) - pos);
									//strideA = length(findRayStartPos(pos, light, _PlanetCenter,  length(pos - _PlanetCenter) - 100 ) - pos);
									posA = pos + light * strideA * raysResolution.y;
									weatherData = sampleWeather(posA);
									heightFraction = getHeightFractionForPoint(posA);
									float cloudSample = (pow(weatherData.g, 134) + weatherData.b) * 1; // tex3Dlod(_ShapeTexture, float4(pos * 0.00001, 0)).r;
																									   //float cloudCoverage =1 - cloudSample;
									float cloudCoverage = sign * cloudSample + sign2;
									cloudDensity = saturate(remap(cloudSample, (heightFraction / cloudCoverage), 1.0, 0.0, 1.0));

									depth += cloudDensity * strideA * rayShadowing.x * raysResolution.z* _DistanceParams.w;//v0.6


									/////////// 2ond sphere intersection, for upper cloud side ///////////////
									/*strideA = length(findRayStartPos(pos, sign*light, _PlanetCenter, _SphereSize + _CloudHeightMinMax.x + 0.95) - pos);
									posA = pos + light * strideA * raysResolution.y;
									weatherData = sampleWeather(posA);
									heightFraction = getHeightFractionForPoint(posA);
									cloudSample = (pow(weatherData.g, 134) + weatherData.b) * 1;
									cloudCoverage = sign * cloudSample + sign2;
									cloudDensity = saturate(remap(cloudSample, (heightFraction / cloudCoverage), 1.0, 0.0, 1.0));
									depth += cloudDensity * strideA * rayShadowing.x * raysResolution.z;*/
								}
								else {
									//UNITY_LOOP 
									//for (int s = 0; s < _SampleCountL / divider; s++)
									for (int s = 0; s < _SampleCountL; s++)
									{
										weatherData = sampleWeather(posA); // sample weather
										heightFraction = getHeightFractionForPoint(posA);
										cloudDensity = saturate(sampleCloudDensity(posA, heightFraction, weatherData, lod, true));
										depth += cloudDensity * strideA * rayShadowing.x * raysResolution.z; //SampleNoise(posA) * strideA * rayShadowing.x * raysResolution.z;
										posA += light * strideA * divider * 2 * raysResolution.w;
									}
								}

								float depther = BeerPowderA(depth);

								if (depther == 0) {
									depther = 0.04 * 1;
								}

								if (_SunDir.y < 0 || _CloudHeightMinMax.x - pos.y > 0) { //v0.6 //v0.6a
									acc += 10 * _SunColor.w * 0.06 * float3(_SunColor.x, _SunColor.y, _SunColor.z) * depther * pow(AdjustAtten, 2);

									//float dist2 = length(pos.xyz);
									res = saturate((1.0 - res.a) * float4(acc, 1) * 0.04) + res;
									//res = 0.5-saturate((1.0 - res.a) * float4(acc, 1) * 0.04) + 4*res;
								}
							}
							//pos += ray * stride;
						}

						if (sunRaysOn == 3) {
							zeroCount = 0.0; // set zero cloud density counter to 0
							cloudDensity = 0.01; //set a low empty space density !!!!!!!!!!!!!!!!!!!!!!!!!!1
							if (stepLength > 1.0) // if we did big steps before
							{
								i -= stepLength - 1.0; // then move back, previous 0 density location + one small step
								pos -= rd * (stepLength - 1.0);
								weatherData = sampleWeather(pos); // sample weather
								cloudDensity = saturate(sampleCloudDensity(pos, heightFraction, weatherData, lod, true)); // and cloud again
							}

							float4 particle = cloudDensity; // construct cloud particle



															///////////////
							float beerPowder = 7 * beerLaw(cloudDensity) * powderEffect(cloudDensity, cosAngle);
							float HG = max(HenyeyGreensteinPhase(cosAngle, _HenyeyGreensteinGForward), HenyeyGreensteinPhase(cosAngle, _HenyeyGreensteinGBackward)) * 0.07 + 0.8;
							//float3 directLight = (beerPowder)* HG * _SunColor;
							///////////////
							float3 directLight = 0.3*sampleConeToLightA(pos, _SunDir, cosAngle, cloudDensity, weatherData, lod);
							directLight = pow(directLight, 1.3);
							//float3 directLight = 4;// sampleConeToLight(pos, _SunDir, cosAngle, cloudDensity, weatherData, lod); // calculate direct light energy and color
							float3 ambientLight = lerp(_CloudBaseColor, _CloudTopColor, heightFraction); // and ambient

							directLight *= _SunLightFactor;
							ambientLight *= _AmbientLightFactor;
							float divider = length(pos - _LocalLightPos.xyz);
							float colorSample = pow(10, 8);
							particle.rgb = directLight + ambientLight + cloudDensity * _LocalLightColor.rgb * colorSample / (pow(divider, _LocalLightColor.w));// +10 / (abs(pos.xyz - float3(0, 0, 0))); // add lights up and set cloud particle color

							particle.rgb *= particle.a;
							res = (1.0 - res.a) * particle + res;
							//res = (1.0 - res.a) * 0.0008 * 1 * 1 * float4(pow(directLight, 0.06), 1) * float4(1.5, 0.7, 0.5, 1) * 11 + 1 * res * float4(pow(directLight, 0.06), 1);
						}// END 3

						if (sunRaysOn == 2) {
							float adder1 = 0;
							cloudDensity = saturate(0.08*(pow(dot(_SunDir, pos), 1)) - 0.0001)*0.03 + 0.226;//0.004;								
							zeroCount += zeroCountSteps;// 0.0; // set zero cloud density counter to 0

							float4 particle = cloudDensity;// +0.000000005*depth; // construct cloud particle
							float3 directLight = 0;// sampleConeToLight(pos, _SunDir, cosAngle, cloudDensity, weatherData, lod); // calculate direct light energy and color
												   //directLight = 1-sampleConeToLight(pos, _SunDir, cosAngle, cloudDensity, weatherData, lod);					
							float3 posA = pos;
							float3 lightDir = _SunDir;
							float density = cloudDensity;
							float3 initialWeather = weatherData;
#if defined(RANDOM_UNIT_SPHERE)
#else
							const float3 RandomUnitSphere[5] = // precalculated random vectors
							{
								{ -0.6, -0.8, -0.2 },
							{ 1.0, -0.3, 0.0 },
							{ -0.7, 0.0, 0.7 },
							{ -0.2, 0.6, -0.8 },
							{ 0.4, 0.3, 0.9 }
							};
#endif
							float heightFraction;
							float densityAlongCone = -0.16;
							const int steps = sunShaftSteps;// 5; // light cone step count
							float3 weatherData;
							for (int i = 0; i < steps; i = i + 2) {
								posA += (lightDir)* _LightStepLength * 25; // march forward
#if defined(RANDOM_UNIT_SPHERE) // apply random vector to achive cone shape
								float3 randomOffset = rand3(posA) * _LightStepLength * _LightConeRadius * ((float)(i + 1));
#else
								float3 randomOffset = RandomUnitSphere[i] * _LightStepLength * _LightConeRadius * ((float)(i + 1));
#endif
								float3 p = posA + randomOffset; // light sample point
																// sample cloud
								heightFraction = getHeightFractionForPoint(p);
								weatherData = sampleWeather(p);
								densityAlongCone += 221111111 * sampleCloudDensity(p, heightFraction, weatherData, 0, true);
							}
							posA += 32.0 * _LightStepLength * lightDir; // light sample from further away
							weatherData = sampleWeather(posA);
							heightFraction = getHeightFractionForPoint(posA);
							densityAlongCone += sampleCloudDensity(posA, heightFraction, weatherData, lod + 2, false) * weatherDensity(weatherData) * 3.0;

							float densityA = densityAlongCone;
							float powderDensity = density;
							float beerPowder = 1.2 * beerLaw(densityA) * powderEffect(powderDensity, cosAngle);
							float HG = max(HenyeyGreensteinPhase(cosAngle, _HenyeyGreensteinGForward), HenyeyGreensteinPhase(cosAngle, _HenyeyGreensteinGBackward)) * 0.07 + 0.8;

							directLight = (beerPowder)* HG * _SunColor;
							particle.rgb = 0.5*directLight + cloudDensity;// *float3(colorSample, 0, colorSample) / (pow(divider, 2.5));// +10 / (abs(pos.xyz - float3(0, 0, 0))); // add lights up and set cloud particle color

							particle.rgb *= particle.a;
							res = (1.0 - res.a) * 0.0008 * 1 * 1 * float4(pow(directLight, 0.06), 1) * float4(1.5, 0.7, 0.5, 1) * 11 + 1 * res * float4(pow(directLight, 0.06), 1);
						}// END 2

						if (sunRaysOn == 1) {
							//NASOS
							//cloudDensity = saturate(0.004*(pow(dot(_SunDir, pos), 1))*0.0002 - 0.0000001*length(_SunDir - pos)) + 0.002;//0.004;
							//float3 directLight = sampleConeToLight(pos, _SunDir, cosAngle, cloudDensity, weatherData, lod);
							//res = float4(0.22, 0, 0, 0) + (dot(_SunDir,pos))*0.00001; //NASOS
							// res = float4(directLight,0) * 0.1;

							///////////ADD ALL FROM ABOVE
							//NASOS
							float adder1 = 0;
							//if (cloudDensity == 0.0) {
							//cloudDensity = 0.004*(pow(dot(_SunDir, pos),2))*0.0004 * 0.00005;// *length(_SunDir - pos) * 0.00002;
							//float3 directLightA = sampleConeToLight(pos, _SunDir, cosAngle, cloudDensity, weatherData, lod);
							//cloudDensity = saturate(0.004*(pow(dot(_SunDir, pos), 1))*0.0002 - 0.0000001*length(_SunDir - pos)) + 0.006;//0.004;
							cloudDensity = saturate(0.08*(pow(dot(_SunDir, pos), 1)) - 0.0001)*0.03 + 0.226;//0.004;
																											//cloudDensity = cloudDensity * 0.5;
																											//	float3 directLightA = sampleConeToLight(pos, _SunDir, cosAngle, cloudDensity, weatherData, lod);
																											//cloudDensity =(( cloudDensity * directLightA.r*directLightA.g*directLightA.b))*0.4;
																											//cloudDensity = (cloudDensity / (directLightA.r))*1.4;
																											//cloudDensity = (cloudDensity *2/(directLightA.b + directLightA.r));

																											//	cloudDensity = 0.008;

																											//zeroCount += 1.0;
																											//	adder1 = 1;
																											//break;
																											//break;
																											//}

																											//zeroCount = 0;
							zeroCount += zeroCountSteps;// 0.0; // set zero cloud density counter to 0

														//if (stepLength > 1.0) // if we did big steps before
														//{
														//	i -= stepLength - 1.0; // then move back, previous 0 density location + one small step
														//	pos -= rd * (stepLength - 1.0);
														//	weatherData = sampleWeather(pos); // sample weather
														//	cloudDensity = saturate(sampleCloudDensity(pos, heightFraction, weatherData, lod, true)); // and cloud again
														//}

							float4 particle = cloudDensity;// +0.000000005*depth; // construct cloud particle
							float3 directLight = 0;// sampleConeToLight(pos, _SunDir, cosAngle, cloudDensity, weatherData, lod); // calculate direct light energy and color
												   //directLight = 1-sampleConeToLight(pos, _SunDir, cosAngle, cloudDensity, weatherData, lod);


							float3 posA = pos;
							float3 lightDir = _SunDir;
							//float cosAngle, 
							float density = cloudDensity;
							float3 initialWeather = weatherData;
							//	float lod
							//float3 sampleConeToLight(float3 pos, float3 lightDir, float cosAngle, float density, float3 initialWeather, float lod)
							//{
#if defined(RANDOM_UNIT_SPHERE)
#else
							const float3 RandomUnitSphere[5] = // precalculated random vectors
							{
								{ -0.6, -0.8, -0.2 },
							{ 1.0, -0.3, 0.0 },
							{ -0.7, 0.0, 0.7 },
							{ -0.2, 0.6, -0.8 },
							{ 0.4, 0.3, 0.9 }
							};
#endif
							float heightFraction;
							float densityAlongCone = -0.16;
							const int steps = sunShaftSteps;// 5; // light cone step count
							float3 weatherData;
							for (int i = 0; i < steps; i = i + 1) {
								posA += (lightDir)* _LightStepLength * 95; // march forward
#if defined(RANDOM_UNIT_SPHERE) // apply random vector to achive cone shape
								float3 randomOffset = rand3(posA) * _LightStepLength * _LightConeRadius * ((float)(i + 1));
#else
								float3 randomOffset = RandomUnitSphere[i] * _LightStepLength * _LightConeRadius * ((float)(i + 1));
#endif
								float3 p = posA + randomOffset; // light sample point
																// sample cloud
								heightFraction = getHeightFractionForPoint(p);
								weatherData = sampleWeather(p);
								densityAlongCone += 1111111 * sampleCloudDensity(p, heightFraction, weatherData, 0, true);
							}

							//#if defined(SLOW_LIGHTING) // if doing slow lighting then do more samples in straight line
							//						posA += 24.0 * _LightStepLength * lightDir;
							//						weatherData = sampleWeather(posA);
							//						heightFraction = getHeightFractionForPoint(posA);
							//						densityAlongCone += sampleCloudDensity(posA, heightFraction, weatherData, lod, true) * 2.0;
							//						int j = 0;
							//						while (1) {
							//							if (j > 22) {
							//								break;
							//							}
							//							posA += 4.25 * _LightStepLength * lightDir;
							//							weatherData = sampleWeather(posA);
							//							if (weatherData.r > 0.05) {
							//								heightFraction = getHeightFractionForPoint(posA);
							//								densityAlongCone += sampleCloudDensity(posA, heightFraction, weatherData, lod, true);
							//							}
							//
							//							j++;
							//						}
							//#else
							posA += 32.0 * _LightStepLength * lightDir; // light sample from further away
							weatherData = sampleWeather(posA);
							heightFraction = getHeightFractionForPoint(posA);
							densityAlongCone += sampleCloudDensity(posA, heightFraction, weatherData, lod + 2, false) * weatherDensity(weatherData) * 3.0;
							//#endif

							float densityA = densityAlongCone;
							//float cosAngle
							float powderDensity = density;

							//float calculateLightEnergy(float density, float cosAngle, float powderDensity) { // calculates direct light components and multiplies them together
							float beerPowder = 1.2 * beerLaw(densityA) * powderEffect(powderDensity, cosAngle);
							float HG = max(HenyeyGreensteinPhase(cosAngle, _HenyeyGreensteinGForward), HenyeyGreensteinPhase(cosAngle, _HenyeyGreensteinGBackward)) * 0.07 + 0.8;
							//return beerPowder * HG;
							//}

							directLight = (beerPowder)* HG * _SunColor;
							//directLight = (1) * HG * _SunColor * 2;

							//return calculateLightEnergy(densityAlongCone, cosAngle, density) * _SunColor;
							//}


							//////////////END


							//float3 ambientLight = lerp(_CloudBaseColor, _CloudTopColor, heightFraction); // and ambient

							//directLight *= _SunLightFactor; // multiply them by their uniform factors

							//if (adder1 == 1) {
							//directLight = directLight * 0.45 + directLight / directLight.r*0.6;
							//particle = particle * directLight.r * directLight.g*2;
							//}

							//ambientLight *= _AmbientLightFactor;

							//NASOS
							//float divider = length(pos - float3(0, 3000, 0));
							//float colorSample = 111111111;
							//float colorSample = pow(10, 8);
							particle.rgb = 0.5*directLight + cloudDensity;// *float3(colorSample, 0, colorSample) / (pow(divider, 2.5));// +10 / (abs(pos.xyz - float3(0, 0, 0))); // add lights up and set cloud particle color

							particle.rgb *= particle.a; // multiply color by clouds density

														//getRandomRayOffset((duv + _Randomness.xy) * _ScreenParams.xy * _BlueNoise_TexelSize.xy)
														//float rand = getRandomRayOffset((duv + _Randomness.xy) * _ScreenParams.xy * _BlueNoise_TexelSize.xy);
														//	res = (1.0 - res.a) * particle + res; // use premultiplied alpha blending to acumulate samples

														//res = res + 0.001*cos(_Time.x*duv.x*30) + 0.001*cos(_Time.x*duv.y * 130);
														//res = res + length(pos - _SunDir) * 0.00000002 * directLight.r* directLight.g* directLight.b;
														//	res = res + 0.0005 * directLight.r* directLight.g* directLight.b;

														//(1.0 - res.a) * particle
														//res = (1.0 - res.a) * 0.0005 * directLight.r* directLight.g* directLight.b * float4(1.5, 0.7, 0.5, 1)*11 + res ;

							res = (1.0 - res.a) * 0.0008 * 1 * 1 * directLight.b * float4(1.5, 0.7, 0.5, 1) * 11 + res;

						}
						else {
							zeroCount += 1.0;
						}

						//NASOS
						//res = res + (dot(_SunDir, pos))*0.00001;
					}
					stepLength = zeroCount > 10.0 ? BIG_STEP : 1.0; // check if we need to do big or small steps

					pos += rd * stepLength; // march forward
				}

				return res;
			}



			float4 altoClouds(float3 ro, float3 rd, float depth, float cosAngle) { // samples high altitude clouds
				float4 res = 0.0;
				float3 pos = findRayStartPos(ro, rd, _PlanetCenter, _SphereSize + _CloudHeightMinMax.y + 3000.0); // finds sample position
				float dist = distance(ro, pos);
				if (dist < depth && pos.y > _ZeroPoint.y && dist > 0.0) { // chekcs for depth texture, above ground 

					float alto = tex2Dlod(_AltoClouds, float4((pos.xz + _HighCloudsWindOffset) * _HighCloudsScale, 0, 0)).r * 2.0; // samples high altitude cloud texture

					float coverage = tex2Dlod(_WeatherTexture, float4((pos.xz + _HighCloudsWindOffset) * _CoverageHighScale, 0, 0)).r; // same as with volumetric clouds
					coverage = saturate(coverage - _CoverageHigh);

					alto = remap(alto, 1.0 - coverage, 1.0, 0.0, 1.0);
					alto *= coverage;
					float3 directLight = max(HenyeyGreensteinPhase(cosAngle, _HenyeyGreensteinGForward), HenyeyGreensteinPhase(cosAngle, _HenyeyGreensteinGBackward)) * _SunColor; // for high altitude clouds uses HG phase
					directLight *= _SunLightFactor * 0.2;
					float3 ambientLight = _CloudTopColor * _AmbientLightFactor * 1.5; // ambient light is the high cloud layer ambient color
					float4 aLparticle = float4(min(ambientLight + directLight, 0.7), alto);

					aLparticle.rgb *= aLparticle.a;

					res = aLparticle;
				}

				return saturate(res);
			}

			v2f_FV Vertex_FV(uint vertexID : SV_VertexID) //Varyings Vertex(uint vertexID : SV_VertexID) //Varyings Vertex(Attributes v)
			{
				// Render settings
				float far = _ProjectionParams.z * 100000000; //CORRECT
				float2 orthoSize = unity_OrthoParams.xy;
				float isOrtho = _ProjectionParams.w; // 0: perspective, 1: orthographic
													 // Vertex ID -> clip space vertex position
				float x = (vertexID != 1) ? -1 : 3;
				float y = (vertexID == 2) ? -3 : 1;
				float3 vpos = float3(x, y, 1);

				// Perspective: view space vertex position of the far plane
				float3 rayPers = 1 * mul(unity_CameraInvProjection, vpos.xyzz * far).xyz;
				//float3 rayPers = 1 * mul(unity_MatrixInvV, vpos.xyzz * far).xyz; //v0.1
				//rayPers.y = rayPers.y - abs(_cameraDiff.x * 15111); 

				// Orthographic: view space vertex position
				float3 rayOrtho = float3(orthoSize * vpos.xy, 0);

				v2f_FV o;
				o.pos = float4(vpos.x, -vpos.y, 1, 1);
				o.uv = (vpos.xy + 1) / (2);
				//o.ray = float4(rayPers,1);// lerp(rayPers, rayOrtho, isOrtho);

				//NEW
				half index = vpos.z;
				//v.vertex.z = 0.1;
				//vpos.z = 0.1;

				//o.pos = UnityObjectToClipPos(v.vertex);
				//o.uv = v.uv.xy;

	//#if UNITY_UV_STARTS_AT_TOP
	//			if (_MainTex_TexelSize.y < 0) //v0.1 removed from URP
	//				o.uv.y = 1 - o.uv.y;
	//#endif

				// Get the eyespace view ray (normalized)
				o.ray = float4(rayPers, 1);// _FrustumCornersES[(int)index];
										   // Dividing by z "normalizes" it in the z axis
										   // Therefore multiplying the ray by some number i gives the viewspace position
										   // of the point on the ray with [viewspace z]=i
				o.ray /= abs(o.ray.z) * 1;

				// Transform the ray from eyespace to worldspace
				o.ray = mul(_CameraInvViewMatrix, o.ray);

				return o;
			}

			float4x4 _InverseView;
			float3 ComputeViewSpacePosition_FV(v2f_FV input, float z)
			{
				// Render settings
				float near = _ProjectionParams.y;
				float far = _ProjectionParams.z;
				float isOrtho = unity_OrthoParams.w; // 0: perspective, 1: orthographic										
#if !defined(EXCLUDE_FAR_PLANE)
				float mask = 1;
#elif defined(UNITY_REVERSED_Z)
				float mask = z > 0;
#else
				float mask = z < 1;
#endif
				// Perspective: view space position = ray * depth
				float lindepth = Linear01DepthA(input.uv.xy);
				lindepth = Linear01Depth(lindepth, _ZBufferParams);// Linear01Depth(lindepth);
				float3 vposPers = input.ray * lindepth;

				// Orthographic: linear depth (with reverse-Z support)
#if defined(UNITY_REVERSED_Z)
				float depthOrtho = -lerp(far, near, z);
#else
				float depthOrtho = -lerp(near, far, z);
#endif

				// Orthographic: view space position
				float3 vposOrtho = float3(input.ray.xy, depthOrtho);

				// Result: view space position
				return lerp(vposPers, vposOrtho, isOrtho) * mask;
			}

			float4 frag_FV(v2f_FV i) : SV_Target
			{
				//return float4(i.pos);
				//return float4(i.ray);
				//return float4(i.uv, 0, 0);

				//v0.4 - fix scaling
				_CameraWS = _CameraWS * cameraScale;// 80; //v0.2 URP - v0.4 HDRP

				// ray origin (camera position)
				float3 ro = _CameraWS;
				// ray direction
				float3 rd = normalize(i.ray.xyz);


				float zsample = Linear01DepthA(i.uv.xy);
				float depth = Linear01Depth(zsample * (zsample < 1.0), _ZBufferParams);

				//v0.6
				if (depthDilation != 0) {
					float depthOffset = 0.00001 * depthDilation;
					float zsampleA1 = Linear01DepthA(i.uv.xy + float2(depthOffset, depthOffset));
					float depthA1 = Linear01Depth(zsampleA1 * (zsampleA1 < 1.0), _ZBufferParams);
					float zsampleA2 = Linear01DepthA(i.uv.xy + float2(-depthOffset, -depthOffset));
					float depthA2 = Linear01Depth(zsampleA2 * (zsampleA2 < 1.0), _ZBufferParams);
					float zsampleA3 = Linear01DepthA(i.uv.xy + float2(-depthOffset, depthOffset));
					float depthA3 = Linear01Depth(zsampleA3 * (zsampleA3 < 1.0), _ZBufferParams);
					float zsampleA4 = Linear01DepthA(i.uv.xy + float2(depthOffset, -depthOffset));
					float depthA4 = Linear01Depth(zsampleA4 * (zsampleA4 < 1.0), _ZBufferParams);
					depth = (depth + depthA1 + depthA2 + depthA3 + depthA4) / 5;
				}

				//float3 vpos = ComputeViewSpacePosition_FV(i, zsample);
				//float3 wpos = mul(_InverseView, float4(vpos, 1)).xyz;
				float4 wsDir = depth * float4(i.ray);
				float4 wsPos = (_CameraWS)+wsDir;
				//float3 PixelWorld = wpos - _WorldSpaceCameraPosC;		
				//float3 ray = (PixelWorld)*0.0001;
				//rd = normalize(ray);
				//ro = wsPos;

				//return float4(ro, 1);
				//return float4(rd, 1);
				//return float4(depth, depth, depth, 1);
				//return wsDir;
				//return wsPos;

				float2 duv = i.uv;
				//#if UNITY_UV_STARTS_AT_TOP
				//			if (_MainTex_TexelSize.y < 0) //v0.1 removed from URP
				//				duv.y = 1 - duv.y;
				//#endif
							float3 rs;
							float3 re;

							//v0.7
							//float4 vortexPosRadius = float4(0, 0, 0, 1100);
							float distanceToVortexCenter = length(vortexPosRadius.xz - wsPos.xz);
							float3 thick = float3(0, 0, 0);
							if (distanceToVortexCenter < vortexPosRadius.w * 0.93) { //if (distanceToVortexCenter < 280000 * 0.93) { //v0.8
								//pos.y = pos.y + 1000*distanceToVortexCenter;
								thick = float3(0, 80000, 0);
								//ro = ro + thick.y;
								//rd = rd + thick.y;
								//_CloudHeightMinMax.x = _CloudHeightMinMax.x + thick.y;
								_CloudHeightMinMax.y = _CloudHeightMinMax.y + thick.y;
							}

							float steps;
							float stepSize;
							// Ray start pos
				#if defined(ALLOW_IN_CLOUDS) // if in cloud flying is allowed, then figure out if camera is below, above or in the cloud layer and set 
							// starting and end point accordingly.
							bool aboveClouds = false;
							float distanceCameraPlanet = distance(_CameraWS, _PlanetCenter);
							//if (distanceCameraPlanet < _SphereSize + _CloudHeightMinMax.x - _DistanceParams.y) // Below clouds, v0.1 subtract 21000
							//{
							//	if (sunRaysOn == 4) { //v0.6
							//		rs = ro;
							//	}
							//	else {
							//		rs = findRayStartPos(ro, rd, _PlanetCenter, _SphereSize + _CloudHeightMinMax.x);
							//	}
							//	if (rs.y < _ZeroPoint.y + _CameraWS.y - YCutHeightDepthScale.x) // If ray starting position is below horizon //CORRECT + _CameraWS.y - 5000 //v0.5
							//	{
							//		return 0.0;// v0.1
							//	}
							//	re = findRayStartPos(ro, rd, _PlanetCenter, _SphereSize + _CloudHeightMinMax.y);
							//	steps = lerp(_Steps, _Steps * 0.5, rd.y);
							//	stepSize = (distance(re, rs)) / steps;
							//}
							//else if (distanceCameraPlanet > _SphereSize + _CloudHeightMinMax.y) // Above clouds
							//{
							//	rs = findRayStartPos(ro,  rd , _PlanetCenter , _SphereSize + _CloudHeightMinMax.y);//v0.7
							//	re = rs + rd * _FarPlane * extendFarPlaneAboveClouds*1.5; //v0.2    * 6;//v0.1.2

							//	steps = lerp(_Steps, _Steps * 0.5, rd.y);
							//	stepSize = (distance(re, rs)) / steps;
							//	aboveClouds = true;
							//}
							//else // In clouds
							{
								rs = ro;
								re = rs + rd * _FarPlane * extendFarPlaneAboveClouds*1.5; //v0.2    * 6;//CORRECT //v0.1.2

								steps = lerp(_Steps, _Steps * 0.5, rd.y);
								stepSize = (distance(re, rs)) / steps;
							}

				#else
							rs = findRayStartPos(ro, rd, _PlanetCenter, _SphereSize + _CloudHeightMinMax.x);
							if (rs.y < _ZeroPoint.y) // If ray starting position is below horizon
							{
								//return 0.0;
							}
							re = findRayStartPos(ro, rd, _PlanetCenter, _SphereSize + _CloudHeightMinMax.y);
							steps = lerp(_Steps, _Steps * 0.5, rd.y);
							stepSize = (distance(re, rs)) / steps;
				#endif

							// Ray end pos


				#if defined(RANDOM_JITTER_WHITE)
							rs += rd * stepSize * rand(_Time.zw + duv) * BIG_STEP * 0.75;
				#endif
				#if defined(RANDOM_JITTER_BLUE)
							rs += rd * stepSize * BIG_STEP * 0.75 * getRandomRayOffset((duv + _Randomness.xy) * _ScreenParams.xy * _BlueNoise_TexelSize.xy);
				#endif

							// Convert from depth buffer (eye space) to true distance from camera
							// This is done by multiplying the eyespace depth by the length of the "z-normalized"
							// ray (see vert()).  Think of similar triangles: the view-space z-distance between a point
							// and the camera is proportional to the absolute distance.

							//float depth = Linear01Depth(tex2D(_CameraDepthTexture, duv).r);

							//float zsample = Linear01DepthA(i.uv.xy*3);//Linear01DepthA(input.texcoord.xy);
							//float zsample = Linear01DepthA(i.uv.xy);
							//float depth = Linear01Depth(zsample * (zsample < 1.0), _ZBufferParams);

							//depth = 1 - depth;
							//return float4(depth, depth, depth, depth);

							if (depth == 1.0) {
								depth = 100.0;
							}
							depth *= _FarPlane * 100 * YCutHeightDepthScale.y; //v0.5


							float cosAngle = dot(rd, _SunDir);
							//return float4(depth, depth, depth, 0);
							//v0.3
							///////////// SCATTER
							float3 _TintColor = _SkyTint;// float4(1, 1, 1, 1);
														 // towards this screen pixel.
														 //float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv_depth);
														 //float dpth = Linear01Depth(rawDepth);


														 //SM v1.7
							float3 lightDirection = _SunDir;// v3LightDir;// _WorldSpaceLightPos0.xyz;  
							float  cosTheta = dot(normalize(wsDir), lightDirection);

							float3 up = float3(0.0, 0.0, 1.0);
							float3 lambda = float3(680E-8, 550E-8, 450E-8);
							float3 K = float3(0.686, 0.678, 0.666);
							float  rayleighZenithLength = 8.4E3;
							float  mieZenithLength = 1.25E3;
							//float  mieCoefficient = 0.054;
							float  pi = 3.141592653589793238462643383279502884197169;
							float3 betaR = totalRayleigh(lambda) * reileigh * 1000;
							float3 lambda1 = float3(_TintColor.r, _TintColor.g, _TintColor.b)*0.0000001;//  680E-8, 1550E-8, 3450E-8);
							lambda = lambda1;
							float3 betaM = totalMie(lambda1, K, turbidity * Multiplier2) * mieCoefficient;
							float zenithAngle = acos(max(0.0, dot(up, normalize(lightDirection))));
							float sR = rayleighZenithLength / (cos(zenithAngle) + 0.15 * pow(93.885 - ((zenithAngle * 180.0) / pi), -1.253));
							float sM = mieZenithLength / (cos(zenithAngle) + 0.15 * pow(93.885 - ((zenithAngle * 180.0) / pi), -1.253));
							float  rPhase = rayleighPhase(cosTheta*0.5 + 0.5);
							float3 betaRTheta = betaR * rPhase;
							float  mPhase = hgPhase(cosTheta, mieDirectionalG) * Multiplier1;
							float3 betaMTheta = betaM * mPhase;
							float3 Fex = exp(-(betaR * sR + betaM * sM));
							float  sunE = sunIntensity(dot(lightDirection, up));
							float3 Lin = ((betaRTheta + betaMTheta) / (betaR + betaM)) * (1 - Fex) + sunE * Multiplier3*0.0001;
							float  sunsize = 0.0001;
							float3 L0 = 1.5 * Fex + (sunE * 1.0 * Fex)*sunsize;
							float3 FragColor = tonemap(Lin + L0);

							// Compute fog distance
							float g = _DistanceParams.x;
							//if (distance)
							//	g += ComputeDistance(wsDir, depth);
							//if (height)
							g += ComputeHalfSpace(wsDir); //v4.0

														  // Compute fog amount
							half fogFac = ComputeFogFactorA(max(0.0, g));//*1.5; //v0.1 also fix error in URP
																		// Do not fog skybox
																		//if (rawDepth >= 0.999999){
							if (depth > 0) {// rawDepth >= 0.999995) {
								if (1 == 0) {//FogSky <= 0) {
									fogFac = 1.0;
								}
								else {
									//if (distance) {
									//	fogFac = fogFac * ClearSkyFac;
									//}
								}
							}
							//return fogFac; // for debugging
							//half4 sceneColor = tex2D(_MainTex, i.uv);
							//float4 Final_fog_color = lerp(unity_FogColor + float4(FragColor, 1), sceneColor, fogFac);

							//fogFac = float4(fogFac *FragColor.xyz*1,1);
							//float4 fogFac2 = float4(0.5*fogFac +0.5*FragColor.xyz * 1, 1);
							float4 fogFac2 = float4(1 * FragColor.xyz * 1, 1);

							//fogFac = Final_fog_color;
							//////////// END SCATTER


							float4 clouds2D = altoClouds(ro, rd, depth, cosAngle); // sample high altitude clouds
							float4 clouds3D = raymarch(rs, rd * stepSize, steps, depth, cosAngle, duv);// fogFac; // raymarch volumetric clouds
																									   //return clouds2D + clouds3D * 3	;

																									   //return float4(i.ray);
																									   //return float4(rs,0);
																									   //return float4(rd * stepSize, 0);
																									   //return float4(steps, steps, steps, 0);
																									   //return float4(steps, steps, steps, 0);
																									   //return float4(depth, depth, depth, depth);
																									   //return float4(cosAngle, cosAngle, cosAngle, 0);
																									   //return float4(duv, 0, 0);
																									   //float3 weatherData = sampleWeather(rs);
																									   //return float4(weatherData, 0);
																									   //float cloudSample = tex3Dlod(_DetailTexture, float4(rs * _Scale, 1)).r;
																									   //return float4(cloudSample, cloudSample, cloudSample, 0);

							if (scatterOn == 1) {
								clouds3D = float4(clouds3D.rgb*fogFac2.rgb, clouds3D.a);
							}

							//float4 back = SAMPLE_TEXTURE2D(_WeatherTexture, sampler_WeatherTexture, i.uv);
							//float4 weatherData = tex2Dlod(_WeatherTexture, float4((i.uv.xy + 0) * 1, 0, 0));		
							//return weatherData;
							//return float4(depth, depth, depth, 0);
							//return float4(1 * i.uv.y* i.uv.y* i.uv.y, 0, 0, 0);
							//return float4(i.ray);
							//float4 back = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
							//return back;
							//float4 curlNoise = tex2Dlod(_AltoClouds, float4(i.uv.xy * _CurlDistortScale *100000, 0, 0));
							//return float4(curlNoise.rgb,0);//_InteractTexture AltoClouds
							//return float4(depth, depth, depth, 0.5);
							//return float4(i.pos);
							//return float4(i.ray, 0);
							//return clouds2D * (1.0 - clouds3D.a) + clouds3D;
				#if defined(ALLOW_IN_CLOUDS)
							if (aboveClouds) // use premultiplied alpha blending to combine low and high clouds
							{

								return clouds3D * (1.0 - clouds2D.a) + clouds2D;
							}
							else
							{

								return clouds2D * (1.0 - clouds3D.a) + clouds3D;
							}

				#else
							return clouds2D * (1.0 - clouds3D.a) + clouds3D;
				#endif
			}
				////////////////// END FULL VOLUMETRIC CLOUDS

				/////// FOG URP //////////////////////////
				/////// FOG URP //////////////////////////
				/////// FOG URP //////////////////////////
				// Vertex shader that procedurally outputs a full screen triangle
				Varyings Vertex(uint vertexID : SV_VertexID) //Varyings Vertex(Attributes v)
			{
				//Varyings o = (Varyings)0;
				//UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				//VertexPositionInputs vertexInput = GetVertexPositionInputs(v.positionOS.xyz);

				//float4 vpos = float4(vertexInput.positionCS.x, vertexInput.positionCS.y, vertexInput.positionCS.z, 1.0);
				//o.position = vpos;

				////o.position.z = 0.1;

				//float2 uv = v.uv;
				//
				//o.texcoord = float2(uv.x,uv.y);
				////o.texcoord = uv.xy;

				//float far = _ProjectionParams.z ;
				//float3 rayPers = -mul(unity_CameraInvProjection, vpos.xyzz * far).xyz;
				//rayPers.y = rayPers.y + 1* abs(_cameraDiff.x * 15111);

				//o.ray = rayPers;//lerp(rayPers, rayOrtho, isOrtho);
				//o.uvFOG = uv.xy;
				//half index = vpos.z;
				//o.interpolatedRay.xyz = _FrustumCornersWS[(int)index] ;// vpos;  // _FrustumCornersWS[(int)index];
				//o.interpolatedRay.w = index;
				//return o;

				// Render settings
				float far = _ProjectionParams.z;
				float2 orthoSize = unity_OrthoParams.xy;
				float isOrtho = _ProjectionParams.w; // 0: perspective, 1: orthographic
													 // Vertex ID -> clip space vertex position
				float x = (vertexID != 1) ? -1 : 3;
				float y = (vertexID == 2) ? -3 : 1;
				float3 vpos = float3(x, y, 1.0);

				// Perspective: view space vertex position of the far plane
				float3 rayPers = mul(unity_CameraInvProjection, vpos.xyzz * far).xyz;
				//float3 rayPers = mul(unity_MatrixV, vpos.xyzz * far).xyz * 100; //v0.1 //v0.1.1
				//rayPers.y = rayPers.y - abs(_cameraDiff.x * 15111);

				// Orthographic: view space vertex position
				float3 rayOrtho = float3(orthoSize * vpos.xy, 0);

				Varyings o;
				o.position = float4(vpos.x, -vpos.y, 1, 1);
				o.texcoord = (vpos.xy + 1) / 2;
				o.ray = lerp(rayPers, rayOrtho, isOrtho);
				//o.ray = float3(0,0,1); //v0.1.1

				//MINE
				float3 vA = vpos;
				float deg = _cameraRoll;
				float alpha = deg * 3.14 / 180.0;
				float sina, cosa;
				sincos(alpha, sina, cosa);
				float2x2 m = float2x2(cosa, -sina, sina, cosa);

				float3 tmpV = float3(mul(m, vA.xy), vA.z).xyz;
				float2 uvFOG = TransformTriangleVertexToUV(tmpV.xy);
				o.uvFOG = uvFOG.xy;

				half index = vpos.z;
				o.interpolatedRay.xyz = vpos;  // _FrustumCornersWS[(int)index];
				o.interpolatedRay.w = index;

				/// CLOUDS
				//half index = v.vertex.z;
				//v.vertex.z = 0.1;
				//o.vertex = UnityObjectToClipPos(v.vertex);
				//o.uv = v.texcoord.xy;
				//v3.5		
				//		o.uv_depth = MultiplyUV(UNITY_MATRIX_TEXTURE0, o.texcoord);// v.texcoord);
				//#if UNITY_UV_STARTS_AT_TOP
				//		if (_MainTex_TexelSize.y < 0) {
				//			o.uv_depth.y = 1 - o.uv_depth.y;
				//		}
				//#endif	
				float2 XY = vpos.xy / index * _HorizonZAdjust;// o.vertex.xy / o.vertex.w * _HorizonZAdjust; //v2.1.24

															  //XY.y = XY.y + 0.25*_HorizonZAdjust*_cameraDiff.x*_cameraTiltSign; //0.5 - 0.03, 1 - 0.00,
															  //XY.y = XY.y + 0.03*_HorizonZAdjust*_cameraDiff.x*_cameraTiltSign;

				float3 forward = float3(0, 0, 1);// mul((float3x3)unity_CameraToWorld, float3(0, 0, 1)); //v0.1

				//XY.y = XY.y - 1.3*_HorizonZAdjust*forward.y*1;
				//XY.y = XY.y - 1.8*_HorizonZAdjust*forward.y; //0.5 - 1.8, 0.25 - 5.1, 1 - 0
				//		XY.y = XY.y - 3.6*(1-_HorizonZAdjust)*forward.y*_HorizonZAdjust; //URP v1.0
				//		XY.x = XY.x / 2;

				//PERSPECTIVE PROJECTION
				float4 farClip = float4(XY, 1, 1);
				float4 farWorld = mul(_WorldClip, farClip);
				float3 farWorldScaled = farWorld.xyz / farWorld.w * (30000 / farCamDistFactor); //URP v0.1
				o.FarCam = farWorldScaled - _WorldSpaceCameraPosC;//-_CameraWS
																  ////END CLOUDS


				return o;
			}

			float3 ComputeViewSpacePosition(Varyings input, float z)
			{
				// Render settings
				float near = _ProjectionParams.y;
				float far = _ProjectionParams.z;
				float isOrtho = unity_OrthoParams.w; // 0: perspective, 1: orthographic
													 // Z buffer sample
													 // Far plane exclusion
#if !defined(EXCLUDE_FAR_PLANE)
				float mask = 1;
#elif defined(UNITY_REVERSED_Z)
				float mask = z > 0;
#else
				float mask = z < 1;
#endif

				// Perspective: view space position = ray * depth
				float lindepth = Linear01DepthA(input.texcoord.xy);
				lindepth = Linear01Depth(lindepth, _ZBufferParams);// Linear01Depth(lindepth);
				float3 vposPers = input.ray * lindepth / (farCamDistFactor); //v0.1.2

				//if (Linear01DepthA(input.texcoord.xy) ==0) {
				//	vposPers = input.ray;
				//}

				// Orthographic: linear depth (with reverse-Z support)
#if defined(UNITY_REVERSED_Z)
				float depthOrtho = -lerp(far, near, z);
#else
				float depthOrtho = -lerp(near, far, z);
#endif

				// Orthographic: view space position
				float3 vposOrtho = float3(input.ray.xy, depthOrtho);

				// Result: view space position
				return lerp(vposPers, vposOrtho, isOrtho) * mask;
			}

			half4 VisualizePosition(Varyings input, float3 pos)
			{
				const float grid = 5;
				const float width = 3;

				pos *= grid;

				// Detect borders with using derivatives.
				float3 fw = fwidth(pos);
				half3 bc = saturate(width - abs(1 - 2 * frac(pos)) / fw);

				// Frequency filter
				half3 f1 = smoothstep(1 / grid, 2 / grid, fw);
				half3 f2 = smoothstep(2 / grid, 4 / grid, fw);
				bc = lerp(lerp(bc, 0.5, f1), 0, f2);

				// Blend with the source color.			
				half4 c = SAMPLE_TEXTURE2D_X(_MainTexB, sampler_MainTexB, float2(input.texcoord.x, input.texcoord.y / 2));// input.texcoord); //v0.1.1
				c.rgb = SRGBToLinear(lerp(LinearToSRGB(c.rgb), bc, 0.5));

				return c;
			}

			///////////////// FRAGMENT /////////////////////////////////



			float2 WorldToScreenPos(float3 pos) {
				pos = normalize(pos - _WorldSpaceCameraPos)*(_ProjectionParams.y + (_ProjectionParams.z - _ProjectionParams.y)) + _WorldSpaceCameraPos;
				float2 uv = float2(0, 0);
				float4 toCam = float4(pos.xyz, 1);// mul(unity_WorldToCamera, float4(pos.xyz, 1)); //v0.1
				float camPosZ = toCam.z;
				float height = 2 * camPosZ / 1;// unity_CameraProjection._m11; v0.1
				float width = _ScreenParams.x / _ScreenParams.y * height;
				uv.x = (toCam.x + width / 2) / width;
				uv.y = (toCam.y + height / 2) / height;
				return uv;
			}

			float2 raySphereIntersect(float3 r0, float3 rd, float3 s0, float sr) {

				float a = dot(rd, rd);
				float3 s0_r0 = r0 - s0;
				float b = 2.0 * dot(rd, s0_r0);
				float c = dot(s0_r0, s0_r0) - (sr * sr);
				float disc = b * b - 4.0 * a* c;
				if (disc < 0.0) {
					return float2(-1.0, -1.0);
				}
				else {
					return float2(-b - sqrt(disc), -b + sqrt(disc)) / (2.0 * a);
				}
			}


			float4x4 rotationMatrix4(float3 axis, float angle)
			{
				axis = normalize(axis);
				float s = sin(angle);
				float c = cos(angle);
				float oc = 1.0 - c;

				return float4x4 (oc * axis.x * axis.x + c, oc * axis.x * axis.y - axis.z * s, oc * axis.z * axis.x + axis.y * s, 0.0,
					oc * axis.x * axis.y + axis.z * s, oc * axis.y * axis.y + c, oc * axis.y * axis.z - axis.x * s, 0.0,
					oc * axis.z * axis.x - axis.y * s, oc * axis.y * axis.z + axis.x * s, oc * axis.z * axis.z + c, 0.0,
					0.0, 0.0, 0.0, 1.0);

			}

			/////////////// CLOUDS 

			//v3.5
			//v3.5 clouds
			float UVRandom(float2 uv)
			{
				float f = dot(float2(12.9898, 78.233), uv);
				return frac(43758.5453 * sin(f));
			}

			//SUN SHAFTS
			float SampleNoiseA(float3 uvw)
			{
				const float baseFreq = 1e-5;

				float4 uvw1 = float4(uvw * _NoiseFreq1 * baseFreq, 0);
				float4 uvw2 = float4(uvw * _NoiseFreq2 * baseFreq, 0);

				uvw1.xyz += _Scroll1.xyz * _Time.x;
				uvw2.xyz += _Scroll2.xyz * _Time.x;

				float n1 = tex3Dlod(_NoiseTex1, uvw1).a;
				float n2 = tex3Dlod(_NoiseTex2, uvw2).a;
				float n = n1 * _NoiseAmp1 + n2 * _NoiseAmp2;

				n = saturate(n + _NoiseBias);

				float y = uvw.y - _Altitude0;
				float h = _Altitude1 - _Altitude0;
				n *= smoothstep(0, h * 0.1, y);
				n *= smoothstep(0, h * 0.4, h - y);

				return n;
			}

			float SampleNoise(float3 uvw, float _Altitude1, float _NoiseAmp1, float Alpha)//v3.5.3
			{

				float AlphaFactor = clamp(Alpha*_InteractTextureAtr.w, _InteractTextureAtr.x, 1);

				const float baseFreq = 1e-5;

				float4 uvw1 = float4(uvw * _NoiseFreq1 * baseFreq, 0);
				float4 uvw2 = float4(uvw * _NoiseFreq2 * baseFreq, 0);

				uvw1.xyz += _Scroll1.xyz * _Time.x;
				uvw2.xyz += _Scroll2.xyz * _Time.x;

				float n1 = tex3Dlod(_NoiseTex1, uvw1).a;
				float n2 = tex3Dlod(_NoiseTex2, uvw2).a;
				float n = n1 * _NoiseAmp1*AlphaFactor + n2 * _NoiseAmp2;//v3.5.3

				n = saturate(n + _NoiseBias);

				float y = uvw.y - _Altitude0;
				float h = _Altitude1 * 1 - _Altitude0;//v3.5.3
				n *= smoothstep(0, h * (0.1 + _UndersideCurveFactor), y);
				n *= smoothstep(0, h * 0.4, h - y);

				return n;
			}

			float HenyeyGreenstein(float cosine)
			{
				float g2 = _HGCoeff * _HGCoeff;
				return 0.5 * (1 - g2) / pow(abs(1 + g2 - 2 * _HGCoeff * cosine), 1.5);
			}

			float Beer(float depth)
			{
				return exp(-_Extinct * depth * _BackShade);  // return exp(-_Extinct * depth); //_BackShade v3.5
			}

			float BeerPowder(float depth)
			{
				return exp(-_Extinct * depth) * (1 - exp(-_Extinct * 2 * depth));
			}

			float MarchLight(float3 pos, float rand, float _Altitude1, float _NoiseAmp1, float Alpha)
			{
				float3 light = float3(v3LightDir.x, _invertRay * v3LightDir.y, v3LightDir.z);//v3LightDir;// _WorldSpaceLightPos0.xyz; //v4.8
				float stride = (_Altitude1 - pos.y) / (light.y * _SampleCountL);

				//v3.5.2
				if (_invertRay * v3LightDir.y < 0) {//if(_WorldSpaceLightPos0.y < 0){  //v4.8
													//if(_WorldSpaceLightPos0.y > -0.01){         
					stride = (_Altitude0 - pos.y + _WorldSpaceCameraPosC.y) / (light.y * _SampleCountL * 15); //higher helps frame rate A LOT
																											  //}
				}

				pos += light * stride * rand;

				float depth = 0;
				UNITY_LOOP for (int s = 0; s < _SampleCountL; s++)
				{
					depth += SampleNoise(pos, _Altitude1, _NoiseAmp1, Alpha) * stride;
					pos += light * stride;
				}

				return BeerPowder(depth);
			}
			//v3.5 clouds

			////////////// END CLOUDS


			//v0.5a
			half3 DecodeHDR(half4 data, half4 decodeInstructions)
			{
				// Take into account texture alpha if decodeInstructions.w is true(the alpha value affects the RGB channels)
				half alpha = decodeInstructions.w * (data.a - 1.0) + 1.0;

				// If Linear mode is not supported we can skip exponent part
#if defined(UNITY_COLORSPACE_GAMMA)
				return (decodeInstructions.x * alpha) * data.rgb;
#else
#   if defined(UNITY_USE_NATIVE_HDR)
				return decodeInstructions.x * data.rgb; // Multiplier for future HDRI relative to absolute conversion.
#   else
				return (decodeInstructions.x * pow(alpha, decodeInstructions.y)) * data.rgb;
#   endif
#endif
			}


			half4 Fragment(Varyings input) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

			float3 forward = float3(0, 0, 1);// mul((float3x3)(unity_WorldToCamera), float3(0, 0, 1)); //v0.1
			//float zsample = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, input.texcoord);
			//float zsample = Linear01Depth(input.texcoord.xy); //CORRECT WAY

			float zsample = Linear01DepthA(input.texcoord.xy);
			float depth = Linear01Depth(zsample * (zsample < 1.0), _ZBufferParams);// Linear01Depth(lindepth);
																				   //float3 vposPers = input.ray * lindepth;



			float3 vpos = ComputeViewSpacePosition(input, zsample);
			float3 wpos = mul(_InverseView, float4(vpos, 1)).xyz;

			//v0.1.2
			wpos = wpos + float3(0, _HorizonYAdjust,0);

			//return float4(wpos, 1);
			//return float4(depth, depth, depth, 1);

			//float depth = Linear01Depth(zsample * (zsample < 1.0));
			//	float depth =  Linear01Depth(zsample * (zsample < 1.0)); //////// CHANGE 2 URP
			//float depth = Linear01DepthA(input.texcoord.xy);


			//float depthSample = Linear01Depth(input.texcoord.xy); //CORRECT WAY
			//if (depthSample > 0.00000001) { //affect frontal
			//if (depthSample == 0) { 		//affect background
			//if (depthSample < 0.00000001) {	//affect background
			//if (depth > 0.00000001) {
			//return float4(1, 0, 0, 1);
			//}
			//else {
			//return float4(1, 1, 0, 1);
			//}

			//if (depth ==0) {
			//depth = 1; //EXPOSE BACKGROUND AS FORGROUND TO GET SCATTERING
			//}



			//v0.1.1
			float3 PixelWorldA = wpos - _WorldSpaceCameraPosC;
			input.ray = (PixelWorldA)*0.0001; //URP v0.2


			float4 wsDir = depth * float4(input.ray, 1); // input.interpolatedRay;	

														 //_CameraWS = float4(85.8, -102.19,-10,1);
			float4 wsPos = (_CameraWS)+wsDir;// _CameraWS + wsDir; //////// CHANGE 1 URP
											 //return wsPos;


											 //// CLOUDS
			float4 wsDirA = depth * input.interpolatedRay; //dpth * i.interpolatedRay;
			float4 wsPosA = _CameraWS + wsDirA;

			//return ((wsPos) *0.1);

			///// SCATTER
			//float3 lightDirection = float3(-v3LightDir.x - 0 * _cameraDiff.w * forward.x, -v3LightDir.y - 0 * _cameraDiff.w * forward.y, v3LightDir.z);
			//float3 lightDirection = float3(v3LightDir.x - 0 * _cameraDiff.w * forward.x, -v3LightDir.y - 0 * _cameraDiff.w * forward.y, -v3LightDir.z);
			//float3 lightDirection = float3(-v3LightDir.x - 0 * _cameraDiff.w * forward.x, -v3LightDir.y - 0 * _cameraDiff.w * forward.y, v3LightDir.z);
			float3 lightDirection = float3(-v3LightDirFOG.x - 0 * _cameraDiff.w * forward.x, -v3LightDirFOG.y - 0 * _cameraDiff.w * forward.y, v3LightDirFOG.z);


			//int noise3D = 0;
			half4 noise;
			half4 noise1;
			half4 noise2;
			if (noise3D == 0) {
				float fixFactor1 = 0;
				float fixFactor2 = 0;
				float dividerScale = 1; //1
				float scaler1 = 1.00; //0.05
				float scaler2 = 0.8; //0.01
				float scaler3 = 0.3; //0.01
				float signer1 = 0.004 / (dividerScale * 1.0);//0.4 !!!! (0.005 for 1) (0.4 for 0.05) //0.004
				float signer2 = 0.004 / (dividerScale * 1.0);//0.001

				if (_cameraDiff.w < 0) {
					fixFactor1 = -signer1 * 90 * 2 * 2210 / 1 * (dividerScale / 1);//2210
					fixFactor2 = -signer2 * 90 * 2 * 2210 / 1 * (dividerScale / 1);
				}
				float hor1 = -_cameraDiff.w * signer1 *_cameraDiff.y * 2210 / 1 * (dividerScale / 1) - 1.2 * _WorldSpaceCameraPos.x * 10 + fixFactor1;
				float hor2 = -_cameraDiff.w * signer2 *_cameraDiff.y * 2210 / 1 * (dividerScale / 1) - 1.2 * _WorldSpaceCameraPos.x * 10 + fixFactor2;
				float hor3 = -_cameraDiff.w * signer2 *_cameraDiff.y * 1210 / 1 * (dividerScale / 1) - 1.2 * _WorldSpaceCameraPos.x * 2 + fixFactor2;

				float vert1 = _cameraTiltSign * _cameraDiff.x * 0.77 * 1.05 * 160 + 0.0157*_cameraDiff.y * (pow((input.texcoord.x - 0.1), 2)) - 0.3 * _WorldSpaceCameraPos.y * 30
					- 2 * 0.33 * _WorldSpaceCameraPos.z * 2.13 + 50 * abs(cos(_WorldSpaceCameraPos.z * 0.01)) + 35 * abs(sin(_WorldSpaceCameraPos.z * 0.005));

				float vert2 = _cameraTiltSign * _cameraDiff.x * 0.20 * 1.05 * 160 + 0.0157*_cameraDiff.y * (pow((input.texcoord.x - 0.1), 2)) - 0.3 * _WorldSpaceCameraPos.y * 30
					- 1 * 0.33 * _WorldSpaceCameraPos.z * 3.24 + 75 * abs(sin(_WorldSpaceCameraPos.z * 0.02)) + 85 * abs(cos(_WorldSpaceCameraPos.z * 0.01));

				float vert3 = _cameraTiltSign * _cameraDiff.x * 0.10 * 1.05 * 70 + 0.0117*_cameraDiff.y * (pow((input.texcoord.x - 0.1), 2)) - 0.3 * _WorldSpaceCameraPos.y * 30
					- 1 * 1.03 * _WorldSpaceCameraPos.z * 3.24 + 75 * abs(sin(_WorldSpaceCameraPos.z * 0.02)) + 85 * abs(cos(_WorldSpaceCameraPos.z * 0.01));

				noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, 1 * (dividerScale * (float2(input.texcoord.x*scaler1 * 1, input.texcoord.y*scaler1))
					+ (-0.001*float2((0.94)*hor1, vert1)) + 3 * abs(cos(_Time.y *1.22* 0.012)))) * 2 * 9;
				noise1 = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, 1 * (dividerScale * (input.texcoord.xy*scaler2)
					+ (-0.001*float2((0.94)*hor2, vert2) + 3 * abs(cos(_Time.y *1.22* 0.010))))) * 3 * 9;
				noise2 = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, 1 * (dividerScale * (input.texcoord.xy*scaler3)
					+ (-0.001*float2((0.94)*hor3, vert3) + 1 * abs(cos(_Time.y *1.22* 0.006))))) * 3 * 9;
			}
			else {

				/////////// NOISE 3D //////////////
				const float epsilon = 0.0001;

				float2 uv = input.texcoord * 4.0 + float2(0.2, 1) * _Time.y * 0.01;

				/*#if defined(SNOISE_AGRAD) || defined(SNOISE_NGRAD)
				#if defined(THREED)
				float3 o = 0.5;
				#else
				float2 o = 0.5;
				#endif
				#else*/
				float o = 0.5 * 1.5;
				//#endif

				float s = 0.011;

				/*#if defined(SNOISE)
				float w = 0.25;
				#else*/
				float w = 0.02;
				//#endif

				//#ifdef FRACTAL
				for (int i = 0; i < 5; i++)
					//#endif
				{
					float3 coord = wpos + float3(_Time.y * 3 * _NoiseSpeed.x,
						_Time.y * _NoiseSpeed.y,
						_Time.y * _NoiseSpeed.z);
					float3 period = float3(s, s, 1.0) * 1111;



					//#if defined(CNOISE)
					o += cnoise(coord * 0.17 * _NoiseScale) * w;

					float3 pointToCamera = (wpos - _WorldSpaceCameraPos) * 0.47;
					int steps = 2;
					float stepCount = 1;
					float step = length(pointToCamera) / steps;
					for (int j = 0; j < steps; j++) {
						//ray trace noise												
						float3 coordAlongRay = _WorldSpaceCameraPos + normalize(pointToCamera) * step
							+ float3(_Time.y * 6 * _NoiseSpeed.x,
								_Time.y * _NoiseSpeed.y,
								_Time.y * _NoiseSpeed.z);
						o += 1.5*cnoise(coordAlongRay * 0.17 * _NoiseScale) * w * 1;
						//stepCount++;
						if (depth < 0.99999) {
							o += depth * 45 * _NoiseThickness;
						}
						step = step + step;
					}

					s *= 2.0;
					w *= 0.5;
				}
				noise = float4(o, o, o, 1);
				noise1 = float4(o, o, o, 1);
				noise2 = float4(o, o, o, 1);
			}

			float cosTheta = dot(normalize(wsDir.xyz), lightDirection);
			cosTheta = dot(normalize(wsDir.xyz), -lightDirection);

			float lumChange = clamp(luminance * pow(abs(((1 - depth) / (_OcclusionDrop * 0.1 * 2))), _OcclusionExp), luminance, luminance * 2);
			if (depth <= _OcclusionDrop * 0.1 * 1) {
				luminance = lerp(4 * luminance, 1 * luminance, (0.001 * 1) / (_OcclusionDrop * 0.1 - depth + 0.001));
			}

			float3 up = float3(0.0, 1.0, 0.0); //float3(0.0, 0.0, 1.0);			
			float3 lambda = float3(680E-8 + _TintColorL.r * 0.000001, 550E-8 + _TintColorL.g * 0.000001, 450E-8 + _TintColorL.b * 0.000001);
			float3 K = float3(0.686 + _TintColorK.r * 0.1, 0.678 + _TintColorK.g * 0.1, 0.666 + _TintColorK.b * 0.1);
			float  rayleighZenithLength = 8.4E3;
			float  mieZenithLength = 1.25E3;
			float  pi = 3.141592653589793238462643383279502884197169;
			float3 betaR = totalRayleigh(lambda) * reileigh * 1000;
			float3 lambda1 = float3(_TintColor.r, _TintColor.g, _TintColor.b)* 0.0000001;//  680E-8, 1550E-8, 3450E-8); //0.0001//0.00001
			lambda = lambda1;
			float3 betaM = totalMie(lambda1, K, turbidity * Multiplier2) * mieCoefficient;
			float zenithAngle = acos(max(0.0, dot(up, normalize(lightDirection))));
			float sR = rayleighZenithLength / (cos(zenithAngle) + 0.15 * pow(abs(93.885 - ((zenithAngle * 180.0) / pi)), -1.253));
			float sM = mieZenithLength / (cos(zenithAngle) + 0.15 * pow(abs(93.885 - ((zenithAngle * 180.0) / pi)), -1.253));
			float  rPhase = rayleighPhase(cosTheta*0.5 + 0.5);
			float3 betaRTheta = betaR * rPhase;
			float  mPhase = hgPhase(cosTheta, mieDirectionalG) * Multiplier1;
			float3 betaMTheta = betaM * mPhase;
			float3 Fex = exp(-(betaR * sR + betaM * sM));
			float  sunE = sunIntensity(dot(lightDirection, up));
			float3 Lin = ((betaRTheta + betaMTheta) / (betaR + betaM)) * (1 - Fex) + sunE * Multiplier3*0.0001;
			float  sunsize = 0.0001;
			float3 L0 = 1.5 * Fex + (sunE * 1.0 * Fex)*sunsize;
			float3 FragColor = tonemap(Lin + L0);//tonemap(Lin + L0);
												 ///// END SCATTER

												 ///////////////return float4(FragColor,1);

												 //occlusion !!!!
			float4 sceneColor = float4(0, 0, 0, 0);// Multiplier3 * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.texcoord.xy);


												   //return sceneColor+ (float4(FragColor, 1));
												   //return 0 + (float4(FragColor, 1));


			float3 subtractor = saturate(pow(abs(dot(normalize(input.ray), normalize(lightDirection))),36)) - (float3(1, 1, 1)*depth * 1);
			if (depth < _OcclusionDrop * 0.1) {
				FragColor = saturate(FragColor * pow(abs((depth / (_OcclusionDrop * 0.1))), _OcclusionExp));
			}
			else {
				if (depth < 0.9999) {
					FragColor = saturate(FragColor * pow(abs((depth / (_OcclusionDrop * 0.1))), 0.001));
				}
			}


			////return float4(FragColor, 1);


			//SCATTER
			int doHeightA = 1;
			int doDistanceA = 1;
			//float g = ComputeDistance(input.ray, depth) - _DistanceOffset/_WorldSpaceCameraPos.y;
			float g = ComputeDistance(input.ray, depth) - _DistanceOffset;
			if (doDistanceA == 1) {
				//g += ComputeDistance(input.ray, depth) - (_DistanceOffset - 100*_WorldSpaceCameraPos.y);
				g += ComputeDistance(input.ray, depth) - _DistanceOffset;
			}
			if (doHeightA == 1) {
				g += ComputeHalfSpace(wpos);
				//g += ComputeHalfSpace(wpos*0.5) - _DistanceOffset;
				//g += ComputeHalfSpace(wpos*0.05);
				//float4 wsDir1 = depth * input.interpolatedRay;
				//float3 wpos1 = _WorldSpaceCameraPos + (wsDir1);// +wsDir; // _CameraWS + wsDir;
				//float FH = _HeightParams.x;
				//float3 C = _WorldSpaceCameraPos.xyz;
				//float3 V = (wsDir1);
				//float3 P = wpos1;
				//float3 aV = _HeightParams.w * V			*		1;
				//float FdotC = _HeightParams.y;
				//float k = _HeightParams.z;
				//float FdotP = P.y - FH;
				//float FdotV = (wsDir1).y;
				//float c1 = k * (FdotP + FdotC);
				//float c2 = (1 - 2 * k) * FdotP;
				//float g1 = min(c2, 0.0);
				//g1 = -length(aV) * (c1 - g1 * g1 / abs(FdotV + 1.0e-5f));
				//g += g1 * 1;
			}

			g = g * pow(abs((noise.r + 1 * noise1.r + _NoiseDensity * noise2.r * 1)), 1.2)*0.3;

			half fogFac = ComputeFogFactorA(max(0.0, g));
			if (zsample <= 1 - 0.999995) {
				//if (zsample >= 0.999995) {
				if (FogSky <= 0) {
					fogFac = 1.0* ClearSkyFac;
				}
				else {
					if (doDistanceA) {
						fogFac = fogFac * ClearSkyFac;
					}
				}
			}

			//	float4 Final_fog_color = lerp(unity_FogColor + float4(FragColor * 1, 1), sceneColor, fogFac);
				float4 Final_fog_color = lerp(float4(FragColor * 1, 1), sceneColor, fogFac); //v0.1

				//return Final_fog_color;

				float fogHeight = _Height;
				half fog = ComputeFogFactorA(max(0.0, g));

				//local light
				float3 visual = 0;// VisualizePosition(input, wpos);

				//return  VisualizePosition(input, wpos);

				if (1 == 1) {

					float3 light1 = localLightPos.xyz;
					float dist1 = length(light1 - wpos);

					float2 screenPos = WorldToScreenPos(light1);
					float lightRadius = localLightColor.w;

					float dist2 = length(screenPos - float2(input.texcoord.x, input.texcoord.y * 0.62 + 0.23));
					if (
						length(_WorldSpaceCameraPos - wpos) < length(_WorldSpaceCameraPos - light1) - lightRadius
						&&
						dot(normalize(_WorldSpaceCameraPos - wpos), normalize(_WorldSpaceCameraPos - light1)) > 0.95// 0.999
						) { //occlusion
					}
					else {
						float factorOcclusionDist = length(_WorldSpaceCameraPos - wpos) - (length(_WorldSpaceCameraPos - light1) - lightRadius);
						float factorOcclusionDot = dot(normalize(_WorldSpaceCameraPos - wpos), normalize(_WorldSpaceCameraPos - light1));

						Final_fog_color = lerp(Final_fog_color,
							Final_fog_color  * (1 - ((11 - dist2) / 11))
							+ Final_fog_color * float4(2 * localLightColor.x, 2 * localLightColor.y, 2 * localLightColor.z, 1)*(11 - dist2) / 11,
							(localLightPos.w * saturate(1 * 0.1458 / pow(dist2, 0.95))
								+ 0.04*saturate(pow(1 - input.uvFOG.y * (1 - fogHeight), 1.0)) - 0.04)
						);
					}
				}

				//return sceneColor/2 + Final_fog_color/2;

		#if USE_SKYBOX
				// Look up the skybox color.
				half3 skyColor = DecodeHDR(texCUBE(_SkyCubemap, input.ray), _SkyCubemap_HDR);
				skyColor *= _SkyTint.rgb * _SkyExposure * 1;// unity_ColorSpaceDouble; //v0.4
				// Lerp between source color to skybox color with fog amount.
				return lerp(half4(skyColor, 1), sceneColor, fog);
		#else
				// Lerp between source color to fog color with the fog amount.
				half4 skyColor = lerp(_FogColor, sceneColor, saturate(fog));
				float distToWhite = (Final_fog_color.r - 0.99) + (Final_fog_color.g - 0.99) + (Final_fog_color.b - 0.99);



				/////////////////////////////////// CLOUDS
				//ALL PROJECTIONS
				//float depthVOLIN = UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture,i.uv_depth));  //SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,i.uv_depth);
				//float4 PixelWorldW = (i.worldMul*depthVOLIN) + i.worldAdd;
				//float3 PixelWorld = PixelWorldW.xyz/PixelWorldW.w;

				//PERSPECTIVE PROJECTION
				float depthVOLIN = depth; //URP

										  //v2.1.19
				if (_fastest == 0) {
					depthVOLIN = 1;
				}



				//wsPos + _WorldSpaceCameraPosC + float3(0, _HorizonYAdjust, 0);//
				//float3 PixelWorld =  (input.FarCam * depthVOLIN) + _WorldSpaceCameraPosC + float3(0, _HorizonYAdjust, 0); //ORTHO -14000 //v3.5.1
				float3 PixelWorld = wpos - _WorldSpaceCameraPosC; //URP v0.2

																  //float3 PixelWorld = (input.FarCam * depthVOLIN) + 0 + float3(0, _HorizonYAdjust + 1700*_cameraDiff.x * _cameraTiltSign, 0); //ORTHO -14000 //v3.5.1	
																  //*_cameraDiff.x*_cameraTiltSign

				float3 sky = Final_fog_color.xyz;//v3.5a
												 //float3 ray = -(PixelWorld)* 0.00001; //input.ray;// -(PixelWorld)*0.00001;
				float3 ray = (PixelWorld)*0.0001; //URP v0.2

				int samples = lerp(_SampleCount1, _SampleCount0, ray.y);





				//v4.8.2
				float dist0 = _Altitude0 / ray.y;
				float3 pos = ray * (dist0);
				pos = pos + _WorldSpaceCameraPosC;

				_Altitude1 = _Altitude1 + varianceAltitude1 * (cos(pos.x * 0.001) * 1500 + cos(pos.z * 0.001) * 2000 + cos(pos.x * 0.0005) * 300 + cos(pos.z * 0.0003) * 600);
				_Altitude1 = _Altitude1 + varianceAltitude1 * (abs(cos(pos.x * 0.001)) * 1500 + abs(sin(pos.z * 0.001)) * 2000);

				float dist1 = _Altitude1 / ray.y;
				float stride = (dist1 - dist0) / samples;




				//v2.1.19
				float alphaFactor = 1;
				if (_fastest == 0) {
					alphaFactor = _alphaFactor; //4.1f
				}

				//URP v0.1
				if (length(wpos - _WorldSpaceCameraPosC) < _NearZCutoff) {
					return float4(0, 0, 0, 0);
				}

				//if (ray.y < 0.01 || dist0 >= _FarDist || ray.z < _NearZCutoff) return float4(0, 0, 0, alphaFactor); //v4.1
				if (ray.y < 0.01 || dist0 >= _FarDist || length(wpos - _WorldSpaceCameraPosC) < _NearZCutoff) return float4(0, 0, 0, alphaFactor); //v4.1
																																				   //if (ray.y < 0.1 || dist0 >= _FarDist || length(wpos - _WorldSpaceCameraPosC) < _NearZCutoff) return float4(0, 0, 0, alphaFactor); //v4.1 //URP v0.1

				float3 light = v3LightDir;// 4.8
				float hg = HenyeyGreenstein(dot(ray, light));
				float2 uv = input.texcoord + _Time.x;
				float offs = UVRandom(uv) * (dist1 - dist0) / samples;

				//v4.8.2
				pos = pos + ray * (offs);





				//v3.5.1
				float dist = length(wsPosA.xyz - _WorldSpaceCameraPosC.xyz);
				if (dist < _FadeThreshold) {
					return float4(sky, 1);
				}




				float3 acc = float3(0,0,0);

				//v2.1.19
				//float3 intensityMod = _LocalLightPos.w * _LocalLightColor.xyz * pow(10, 7);
				float3 intensityMod = _LocalLightColor.xyz * pow(10, 7); //v0.6

				float depthA = 0;
				float preDevide = samples / _Exposure;
				float3 groundColor1 = _GroundColor.rgb*0.0006;

				float3 _LightColor0 = float3(1, 1, 1);

				float3 light1 = _LightColor0.rgb * _SkyTint.rgb; // URP
				float scatterHG = _Scatter * hg;



				UNITY_LOOP for (int s = 0; s < samples; s++)
				{
					float4 texInteract = tex2Dlod(_InteractTexture, 0.0003*float4(
						_InteractTexturePos.x*pos.x + _InteractTexturePos.z*-_Scroll1.x * _Time.x + _InteractTextureOffset.x,
						_InteractTexturePos.y*pos.z + _InteractTexturePos.w*-_Scroll1.z * _Time.x + _InteractTextureOffset.y,
						0, 0));
					//return float4(texInteract * Final_fog_color);

					float diffPos = length(_LocalLightPos.xyz - pos);
					texInteract.a = texInteract.a + clamp(_InteractTextureAtr.z * 0.1*(1 - 0.00024*diffPos), -1.5, 0);

					_NoiseAmp2 = _NoiseAmp2 * clamp(texInteract.a*_InteractTextureAtr.w, _InteractTextureAtr.y, 1);

					float n = SampleNoise(pos, _Altitude1, _NoiseAmp1, texInteract.a);
					float expose = 0.00001;
					if (s < preDevide) {
						expose = 0;
					}

					//SUN SHAFTS
					float lightAtten = dot(light / length(light), pos / length(pos));

					if (n >= expose) //v4.0 added >= than only >, for better underlight control
					{
						float density = n * stride;
						float rand = UVRandom(uv + s + 1);
						float scatter = density * scatterHG * MarchLight(pos, rand * 0.001, _Altitude1, _NoiseAmp1, texInteract.a); //v4.0

						//float3 beer1 = BeerPowder(depthA) * intensityMod / pow(diffPos, _LocalLightColor.w);
						float3 beer1 = intensityMod / pow(diffPos, _LocalLightColor.w); //v0.6

						float beer2 = 1 - Beer(depthA);
						acc += light1 * scatter * BeerPowder(depthA) + beer2 * groundColor1 + (beer2*0.01*_LightColor0 + scatter) * beer1;//v2.1.19
						depthA += density;
					}
					else if (raysResolutionA.x > 0 && abs(lightAtten > 0.5 * rayShadowingA.y)) { // SUN SHAFTS
						float rand = UVRandom(uv + s + 1);
						float divider = 24 / (raysResolutionA.x);
						float3 light = v3LightDir;// SUN_DIR;
						float strideA = (_Altitude1 - pos.y) / (abs(light.y) * _SampleCountL);
						float3 posA = pos;
						posA += light * strideA * divider * 2 * raysResolutionA.y;
						float AdjustAtten = lightAtten - 0.5 * rayShadowingA.y;
						float depth = 0;
						if (raysResolutionA.w == 0) {
							depth += SampleNoiseA(posA) * strideA * rayShadowingA.x * raysResolutionA.z * 1;
						}
						else {
							UNITY_LOOP for (int s = 0; s < _SampleCountL / divider; s++)
							{
								depth += SampleNoiseA(posA) * strideA * rayShadowingA.x * raysResolutionA.z;
								posA += light * strideA * divider * 2 * raysResolutionA.w;
							}
						}
						float depther = BeerPowder(depth);

						if (depther == 0) {
							depther = 0.02 * 1;
						}
						acc += 10 * _rayColor.w * 0.06 * float3(_rayColor.x, _rayColor.y, _rayColor.z) * depther * pow(AdjustAtten, 2);
					}//END SUN SHAFTS

					pos += ray * stride;

				}

				//return float4(zsample, zsample, zsample, 1);
				//return float4(depth, depth, depth, 1);
				//return float4(dist, dist, dist, 1);
				//return float4(0, 0, 0, 1);
				//return float4(texInteract.rgb, 1);
				//return float4(stride, stride, stride, 1);
				//return float4(ray, 1);
				//return float4(PixelWorld, 1);
				//return float4(wsPos);
				//return float4(acc, 1);

				if (_mobileFactor > 0) { //v4.1f
					acc += Beer(depthA) * sky + FragColor * _SunSize*acc;
					acc = lerp(acc, sky*0.96, saturate(((dist0) / (_FarDist*0.5))) + 0.03);
				}

				float4 finalColor = float4(acc, 1);

				return float4(finalColor.rgb, pow(lerp(Beer(depthA), 0.96, saturate(((dist0) / (_FarDist*0.5))) + 0.03), 2));
				/////////// END CLOUDS

				//return Final_fog_color * _FogColor + float4(visual, 0);		
		#endif	
			}

				//v3.5.2
				float gaussed(float sig, float pos) {
				//from "Fitting a Gaussion Function to binned data" paper
				float numer = pos * pos;
				float denom = sig * sig;
				float ratio = numer / denom;
				return (0.39894 / sig) * exp(-0.5*ratio);
			}

			half4 ComputeFogAddCombine(v2f i) : SV_Target
			{
				//v2.1.19
				float fac1 = _WorldClip[2][3];
			float fac2 = _WorldClip[0][3];
			float fac3 = _WorldClip[1][3];
			half4 cloudColor = SAMPLE_TEXTURE2D_X(_SkyTex, sampler_SkyTex,  float2(i.uv.x, i.uv.y / 2) + float2(1 * 0 * 0.0014 , -1 * 0 * 0.0553));
			//half4 cloudColor = SAMPLE_TEXTURE2D(_SkyTex, sampler_SkyTex, i.uv.xy*fac1 + float2(fac2* 0.002, -fac3* 0.004));
			//half4 cloudColor = SAMPLE_TEXTURE2D(_CloudTex, sampler_CloudTex,  i.uv.xy + float2(1 * 0 * 0.0014 , -1 * 0 * 0.0553));
			//v4.8.3 - interpolate colors																								
			//half4 cloudColorP = SAMPLE_TEXTURE2D(_CloudTexP, sampler_CloudTexP, i.uv +float2(-fac2 * 0.002, fac3 * 0.004));// i.uv + float2(0 * 0.019, 0 * 0.022));
			half4 cloudColorP = SAMPLE_TEXTURE2D_X(_CloudTexP, sampler_CloudTexP, float2(i.uv.x, i.uv.y / 2) + float2(0 * 0.019, 0 * 0.022));
			cloudColor = lerp(cloudColorP, cloudColor, frameFraction);
			return float4(cloudColor.rgb, cloudColor.a);
			}

				half4 ComputeFogAdd(v2f i, bool distance, bool height, bool splitFrames) : SV_Target
			{
				//v2.1.19
				float fac1 = 1;
			float fac2 = 0;
			float fac3 = 0;
			if (splitFrames) {
				fac1 = _WorldClip[2][3];
				fac2 = _WorldClip[0][3];
				fac3 = _WorldClip[1][3];
			}

			float2 iuvs = float2(i.uv.x, i.uv.y / 1); //v0.3 - removed /2
			half4 sceneColor = SAMPLE_TEXTURE2D_X(_MainTexB, sampler_MainTexB, iuvs);// i.uv); //v4.8.3

			float2 usvINNER = iuvs * fac1 + float2(fac2, fac3);
			half4 cloudColor = SAMPLE_TEXTURE2D_X(_CloudTex, sampler_CloudTex, usvINNER); ///URP

																						//v2.1.19
			float2 uvs = iuvs; //v4.8
			if (_invertX == 1) {
				uvs = float2(1 - iuvs.x, iuvs.y);
			}
			half4 skybox = SAMPLE_TEXTURE2D_X(_SkyTex, sampler_SkyTex, uvs*fac1 + half2(fac2, fac3)); //v4.8.3

																									//v2.1.13 - Gauss automated
			if (1 == 1) {
				int rows = 5;
				int iterations = 0.5*(rows - 1);
				float blurStrength = 0.4;
				UNITY_LOOP for (int i1 = -iterations; i1 <= iterations; ++i1) {
					UNITY_LOOP for (int j = -iterations; j <= iterations; ++j) {
						//v2.1.19
						cloudColor += gaussed(3, float(i1))*SAMPLE_TEXTURE2D_X(_CloudTex, sampler_CloudTex, float2(fac2, fac3) + fac1 * float2(iuvs.x + blurStrength * i1*_CloudTex_TexelSize.x, iuvs.y + blurStrength * j*_CloudTex_TexelSize.y));
					}
				}
				cloudColor = 1.2*cloudColor / (rows);
			}

			//v2.1.19
			if (splitFrames) {
				if (iuvs.x < -2 * fac2) {
					float distFromEdge1 = iuvs.x + 2 * fac2;
					cloudColor = SAMPLE_TEXTURE2D_X(_CloudTex, sampler_CloudTex, float2((0 - fac2) + (distFromEdge1 / 2), iuvs.y + fac3));
				}
				if (iuvs.x > 1 - 2 * fac2) {
					float distFromEdge2 = iuvs.x - (1 - 2 * fac2);
					cloudColor = SAMPLE_TEXTURE2D_X(_CloudTex, sampler_CloudTex, float2((1 - (1 * fac2)) + (distFromEdge2 / 2), iuvs.y + fac3));
				}
				if (iuvs.y > 1 - 2 * fac3) {
					//depending on distance 
					float distFromEdge = iuvs.y - (1 - 2 * fac3);
					cloudColor = SAMPLE_TEXTURE2D_X(_CloudTex, sampler_CloudTex, float2(iuvs.x + fac2, (1 - (1 * fac3)) + (distFromEdge / 2)));
				}
				if (iuvs.y < -2 * fac3) {
					float distFromEdge3 = iuvs.y + 2 * fac3;
					cloudColor = SAMPLE_TEXTURE2D_X(_CloudTex, sampler_CloudTex, float2(iuvs.x + fac2,  (0 - fac3) + (distFromEdge3 / 2)));
				}

				//CORNERS - sample from edge
				if (iuvs.y > 1 - 2 * fac3) {
					if (iuvs.x < -2 * fac2) {
						cloudColor = SAMPLE_TEXTURE2D_X(_CloudTex, sampler_CloudTex, float2(0 - 2 * fac2, 1 - 2 * fac3));
					}
				}
				if (iuvs.y > 1 - 2 * fac3) {
					if (iuvs.x > 1 - 2 * fac2) {
						cloudColor = SAMPLE_TEXTURE2D_X(_CloudTex, sampler_CloudTex, float2(1 - 2 * fac2, 1 - 2 * fac3));
					}
				}
				if (iuvs.y < -2 * fac3) {
					if (iuvs.x < -2 * fac2) {
						cloudColor = SAMPLE_TEXTURE2D_X(_CloudTex, sampler_CloudTex, float2(0 - 2 * fac2,  -2 * fac3));
					}
				}
				if (iuvs.y < -2 * fac3) {
					if (iuvs.x > 1 - 2 * fac2) {
						cloudColor = SAMPLE_TEXTURE2D_X(_CloudTex, sampler_CloudTex, float2(1 - 2 * fac2,  -2 * fac3));
					}
				}
			}

			//v2.1.14
			float4 sum = float4 (0,0,0,0);
			float w = 0;
			float weights = 0;
			const float G_WEIGHTS[9] = { 1.0, 0.8, 0.65, 0.5, 0.4, 0.2, 0.1, 0.05, 0.025 };

			float4 sampleA = cloudColor;// SAMPLE_TEXTURE2D(_CloudTex, i.uv.xy); // v2.1.15
			float texelX = _CloudTex_TexelSize.x / 6.0;
			float texelY = _CloudTex_TexelSize.y / 6.0;
			float4 sampleB = SAMPLE_TEXTURE2D_X(_CloudTex, sampler_CloudTex, iuvs.xy + float2(texelX,texelY));
			float4 sampleC = SAMPLE_TEXTURE2D_X(_CloudTex, sampler_CloudTex, iuvs.xy + float2(-texelX,-texelY));
			float4 sampleD = SAMPLE_TEXTURE2D_X(_CloudTex, sampler_CloudTex, iuvs.xy + float2(2 * texelX,2 * texelY));
			float4 sampleE = SAMPLE_TEXTURE2D_X(_CloudTex, sampler_CloudTex, iuvs.xy + float2(-2 * texelX,-2 * texelY));
			float4 sampleF = SAMPLE_TEXTURE2D_X(_CloudTex, sampler_CloudTex, iuvs.xy + float2(3 * texelX,3 * texelY));
			float4 sampleG = SAMPLE_TEXTURE2D_X(_CloudTex, sampler_CloudTex, iuvs.xy + float2(-3 * texelX,-3 * texelY));
			float4 sampleH = SAMPLE_TEXTURE2D_X(_CloudTex, sampler_CloudTex, iuvs.xy + float2(4 * texelX,4 * texelY));
			float4 sampleI = SAMPLE_TEXTURE2D_X(_CloudTex, sampler_CloudTex, iuvs.xy + float2(-4 * texelX,-4 * texelY));
			float4 sampleJ = SAMPLE_TEXTURE2D_X(_CloudTex, sampler_CloudTex, iuvs.xy + float2(5 * texelX,5 * texelY));
			float4 sampleK = SAMPLE_TEXTURE2D_X(_CloudTex, sampler_CloudTex, iuvs.xy + float2(-5 * texelX,-5 * texelY));

			//v2.1.15
			//if pixel is white and has at least one black pixel near, darken it
			if (sampleA.a > 0.5) {//0.5 for first if //0.35 lst if
				if (sampleB.a * sampleC.a * sampleD.a * sampleE.a * sampleF.a * sampleG.a * sampleH.a * sampleI.a < 0.004) {
					sampleA = sampleA / 2;
					sampleB = sampleB / 2;
					sampleC = sampleC / 2;
					sampleD = sampleD / 2;
					sampleE = sampleE / 2;
					sampleF = sampleF / 2;
					sampleG = sampleG / 2;
				}
			}

			w = sampleA.a * G_WEIGHTS[0]; sum += sampleA * w; weights += w;
			w = sampleB.a * G_WEIGHTS[1]; sum += sampleB * w; weights += w;
			w = sampleC.a * G_WEIGHTS[1]; sum += sampleC * w; weights += w;
			w = sampleD.a * G_WEIGHTS[2]; sum += sampleD * w; weights += w;
			w = sampleE.a * G_WEIGHTS[2]; sum += sampleE * w; weights += w;
			w = sampleF.a * G_WEIGHTS[3]; sum += sampleF * w; weights += w;
			w = sampleG.a * G_WEIGHTS[3]; sum += sampleG * w; weights += w;
			w = sampleH.a * G_WEIGHTS[4]; sum += sampleH * w; weights += w;
			w = sampleI.a * G_WEIGHTS[4]; sum += sampleI * w; weights += w;
			w = sampleJ.a * G_WEIGHTS[5]; sum += sampleJ * w; weights += w;
			w = sampleK.a * G_WEIGHTS[5]; sum += sampleK * w; weights += w;
			sum /= weights + 1e-4f;

			//v2.1.19
			if (_fastest == 0) {
				//return float4(sceneColor.a, sceneColor.a, sceneColor.a, 1);
				//return clamp(1.5 * cloudColor*(1 - sceneColor.a), 0, 1) + clamp(1 * sceneColor, 0, 1) + clamp(float4(2 * skybox.rgb*(cloudColor.a)*(1 - sceneColor.a), 0), 0, 1);
				//return cloudColor.a * float4(1, 1, 1, 1);
				return (cloudColor*(1 - cloudColor.a)*(1 - cloudColor.a) + clamp(sceneColor, 0, 1)*(1)*(cloudColor.a)*(cloudColor.a));
				return clamp(1.6 * cloudColor*(1 - sceneColor.a), 0, 1) + clamp(sceneColor, 0, 1) + clamp(float4(2.2 * skybox.rgb*(cloudColor.a)*(1 - sceneColor.a), 0), 0, 1);
			}
			else {
				float4 ds1 = sum + float4((sceneColor.rgb) * (sampleA.a * 2 + sum.a / 2) / 2, 0);  //v2.1.19
				return ds1;
			}
			}
				////// END BLEND

				half4 FragmentTWO(Varyings input) : SV_Target
			{
				///float z = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, input.texcoord.xy);
				float z = SAMPLE_TEXTURE2D_ARRAY(_CameraDepthTexture, sampler_CameraDepthTexture, input.texcoord.xy, unity_StereoEyeIndex).r; //v0.1

				float3 vpos = ComputeViewSpacePosition(input,z);
				//vpos.z = vpos.z +11110;
				float3 wpos = mul(_InverseView, float4(vpos, 1)).xyz;
				return VisualizePosition(input, wpos);
			}

				///// TAA anti alising function
			float3 _TAA_Params;  // xy = offset, z = feedback
			void minmax(in float2 uv, out float4 color_min, out float4 color_max, out float4 color_avg)
			{
				float2 du = float2(_MainTexB_TexelSize.x, 0.0);
				float2 dv = float2(0.0, _MainTexB_TexelSize.y);
				//#if defined(_HIGH_TAA)
				//			float4 ctl = SAMPLE_TEXTURE2D_X(_MainTexB, sampler_MainTexB, uv - dv - du);
				//			float4 ctc = SAMPLE_TEXTURE2D_X(_MainTexB, sampler_MainTexB, uv - dv);
				//			float4 ctr = SAMPLE_TEXTURE2D_X(_MainTexB, sampler_MainTexB, uv - dv + du);
				//			float4 cml = SAMPLE_TEXTURE2D_X(_MainTexB, sampler_MainTexB, uv - du);
				//			float4 cmc = SAMPLE_TEXTURE2D_X(_MainTexB, sampler_MainTexB, uv);
				//			float4 cmr = SAMPLE_TEXTURE2D_X(_MainTexB, sampler_MainTexB, uv + du);
				//			float4 cbl = SAMPLE_TEXTURE2D_X(_MainTexB, sampler_MainTexB, uv + dv - du);
				//			float4 cbc = SAMPLE_TEXTURE2D_X(_MainTexB, sampler_MainTexB, uv + dv);
				//			float4 cbr = SAMPLE_TEXTURE2D_X(_MainTexB, sampler_MainTexB, uv + dv + du);
				//
				//			color_min = min(ctl, min(ctc, min(ctr, min(cml, min(cmc, min(cmr, min(cbl, min(cbc, cbr))))))));
				//			color_max = max(ctl, max(ctc, max(ctr, max(cml, max(cmc, max(cmr, max(cbl, max(cbc, cbr))))))));
				//
				//			color_avg = (ctl + ctc + ctr + cml + cmc + cmr + cbl + cbc + cbr) / 9.0;
				//#elif defined(_MIDDLE_TAA)
				//			float2 ss_offset01 = float2(-_MainTexB_TexelSize.x, _MainTexB_TexelSize.y);
				//			float2 ss_offset11 = float2(_MainTexB_TexelSize.x, _MainTexB_TexelSize.y);
				//			float4 c00 = SAMPLE_TEXTURE2D_X(_MainTexB, sampler_MainTexB, uv - ss_offset11);
				//			float4 c10 = SAMPLE_TEXTURE2D_X(_MainTexB, sampler_MainTexB, uv - ss_offset01);
				//			float4 c01 = SAMPLE_TEXTURE2D_X(_MainTexB, sampler_MainTexB, uv + ss_offset01);
				//			float4 c11 = SAMPLE_TEXTURE2D_X(_MainTexB, sampler_MainTexB, uv + ss_offset11);
				//
				//			color_min = min(c00, min(c10, min(c01, c11)));
				//			color_max = max(c00, max(c10, max(c01, c11)));
				//			color_avg = (c00 + c10 + c01 + c11) / 4.0;
				//#elif defined(_LOW_TAA)
				//			float2 ss_offset11 = float2(_MainTexB_TexelSize.x, _MainTexB_TexelSize.y);
				//			float4 c00 = SAMPLE_TEXTURE2D_X(_MainTexB, sampler_MainTexB, uv - ss_offset11);
				//			float4 c11 = SAMPLE_TEXTURE2D_X(_MainTexB, sampler_MainTexB, uv + ss_offset11);
				//			color_min = min(c00, c11);
				//			color_max = max(c00, c11);
				//			color_avg = (c00 + c11) / 2.0;
				//#endif
				float4 ctl = SAMPLE_TEXTURE2D_X(_MainTexB, sampler_MainTexB, uv - dv - du);
				float4 ctc = SAMPLE_TEXTURE2D_X(_MainTexB, sampler_MainTexB, uv - dv);
				float4 ctr = SAMPLE_TEXTURE2D_X(_MainTexB, sampler_MainTexB, uv - dv + du);
				float4 cml = SAMPLE_TEXTURE2D_X(_MainTexB, sampler_MainTexB, uv - du);
				float4 cmc = SAMPLE_TEXTURE2D_X(_MainTexB, sampler_MainTexB, uv);
				float4 cmr = SAMPLE_TEXTURE2D_X(_MainTexB, sampler_MainTexB, uv + du);
				float4 cbl = SAMPLE_TEXTURE2D_X(_MainTexB, sampler_MainTexB, uv + dv - du);
				float4 cbc = SAMPLE_TEXTURE2D_X(_MainTexB, sampler_MainTexB, uv + dv);
				float4 cbr = SAMPLE_TEXTURE2D_X(_MainTexB, sampler_MainTexB, uv + dv + du);

				color_min = min(ctl, min(ctc, min(ctr, min(cml, min(cmc, min(cmr, min(cbl, min(cbc, cbr))))))));
				color_max = max(ctl, max(ctc, max(ctr, max(cml, max(cmc, max(cmr, max(cbl, max(cbc, cbr))))))));

				color_avg = (ctl + ctc + ctr + cml + cmc + cmr + cbl + cbc + cbr) / 9.0;

			}
			void minmaxCLOUDS(in float2 uv, out float4 color_min, out float4 color_max, out float4 color_avg, float level)
			{
				float2 du = float2(_CloudTex_TexelSize.x * level, 0.0);
				float2 dv = float2(0.0, _CloudTex_TexelSize.y * level);

				float4 ctl = SAMPLE_TEXTURE2D_X(_CloudTex, sampler_CloudTex, uv - dv - du);
				float4 ctc = SAMPLE_TEXTURE2D_X(_CloudTex, sampler_CloudTex, uv - dv);
				float4 ctr = SAMPLE_TEXTURE2D_X(_CloudTex, sampler_CloudTex, uv - dv + du);
				float4 cml = SAMPLE_TEXTURE2D_X(_CloudTex, sampler_CloudTex, uv - du);
				float4 cmc = SAMPLE_TEXTURE2D_X(_CloudTex, sampler_CloudTex, uv);
				float4 cmr = SAMPLE_TEXTURE2D_X(_CloudTex, sampler_CloudTex, uv + du);
				float4 cbl = SAMPLE_TEXTURE2D_X(_CloudTex, sampler_CloudTex, uv + dv - du);
				float4 cbc = SAMPLE_TEXTURE2D_X(_CloudTex, sampler_CloudTex, uv + dv);
				float4 cbr = SAMPLE_TEXTURE2D_X(_CloudTex, sampler_CloudTex, uv + dv + du);

				color_min = min(ctl, min(ctc, min(ctr, min(cml, min(cmc, min(cmr, min(cbl, min(cbc, cbr))))))));
				color_max = max(ctl, max(ctc, max(ctr, max(cml, max(cmc, max(cmr, max(cbl, max(cbc, cbr))))))));

				color_avg = (ctl + ctc + ctr + cml + cmc + cmr + cbl + cbc + cbr) / 9.0;

			}

			/////////////// FULL VOLUMETRIC CLOUDS 
			half4 CombineClouds(v2f i) : SV_Target
			{
				float4 back = SAMPLE_TEXTURE2D_X(_MainTexB, sampler_MainTexB,float2(i.uv.x,i.uv.y*controlCloudEdgeOffset)); //v0.3 removed  /2 //v0.6
				float4 cloud = SAMPLE_TEXTURE2D_X(_CloudTex, sampler_CloudTex, float2(i.uv.x, i.uv.y));


				//void minmax(in float2 uv, out float4 color_min, out float4 color_max, out float4 color_avg)
				//SMOOTH CLOUDS
				if (_Steps % 2 == 0) {//  _Steps < 550) {
					float4 color_min;
					float4 color_max;
					float4 color_avg;
					minmaxCLOUDS(i.uv,  color_min,  color_max,  color_avg, 1);
					cloud.rgb = (cloud.rgb * 1.5 + color_max.rgb * 0.6) / 2;

					//if (abs(length(color_min.rgb) - length(color_max.rgb)) < 0.39999995) {
					//	cloud.rgb = (cloud.rgb * 1.5 + color_max.rgb * 0.6) / 2;
					//}
					//else {
					//	//cloud.rgb = (cloud.rgb * 0.25 + color_max.rgb * 0.75)*0.8;
					//	cloud.rgb = (color_max.rgb * 1.05);
					//}

					//minmaxCLOUDS(i.uv, color_min, color_max, color_avg, 2);
					//cloud.rgb = (cloud.rgb + color_max.rgb * 0.6) / 2;

					//POSTERIZE
					/*if (abs(length(color_min.rgb) - length(color_max.rgb)) < 0.3) {
						cloud.rgb = (cloud.rgb * 1.5 + color_max.rgb * 0.6) / 2;
					}
					else {
						cloud.rgb = (color_max.rgb * 1.6);
					}*/

				}

				//return cloud;
				//return float4(cloud.a, cloud.a, cloud.a, 1);
				//return float4(1,0,0, 1);
				//return float4(cloud.r, cloud.g, cloud.b, 1);
				//return float4(back.rgb * (1.0 - cloud.a) + cloud.rgb, 1.0); // blend them

				//v0.6
				float zsample = Linear01DepthA(i.uv.xy);
				float depth = Linear01Depth(zsample * (zsample < 1.0), _ZBufferParams);
				//return float4(back.rgb * (1.0 - cloud.a) + cloud.rgb, 1.0)*depth + float4((1 - depth) * back.rgb, 0); // blend them
				if (depthDilation == 0) {
					depth = 1;
				}
				return (controlCloudEdgeA.x*float4(back.rgb* (controlCloudEdgeA.y - pow(cloud.a, controlBackAlphaPower)), 1)
					+ controlCloudEdgeA.z*float4(cloud.rgb* pow(cloud.a, controlCloudAlphaPower + 0.001), 1))
					*controlCloudEdgeA.w *depth + float4((1 - depth) * back.rgb, 1);

				//v0.4
				//return float4(back.rgb * (1.0 - clamp(cloud.a + 0.1*cloud.a,0,1)) + cloud.rgb, 1.0); // blend them //v0.6
			}
			half4 fragCombineClouds(v2f i) : SV_Target{ return CombineClouds(i); }
				///////////// END FULL VOLUMETRIC CLOUDS


			half4 frag7(v2f i) : SV_Target{ return ComputeFogAdd(i, false, true,false); }
			half4 frag8(v2f i) : SV_Target{ return ComputeFogAdd(i, false, true,true); }
			half4 frag9(v2f i) : SV_Target{ return ComputeFogAddCombine(i); }
				/////// END FOG URP //////////////////////
				/////// END FOG URP //////////////////////
				/////// END FOG URP //////////////////////

			///////////////////// END VOLUMETRIC CLOUDS v0.1 ///////////////////////////// 1238 line


			/////////////////////////////////////// TEMPORAL AA ///////////////////////////////////
#pragma multi_compile _LOW_TAA _MIDDLE_TAA _HIGH_TAA
//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
//#include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"
//float3 _TAA_Params;  // xy = offset, z = feedback
//TEXTURE2D_X(_MainTex);
//uniform float4 _MainTex_TexelSize;

			TEXTURE2D_X(_TAA_Pretexture);
			//TEXTURE2D_X_FLOAT(_CameraDepthTexture);
			float4x4 _PrevViewProjM_TAA;
			float4x4 _I_P_Current_jittered;
			float4x4 _I_V_Current_jittered;
			struct AttributesTAA
			{
				float4 positionOS   : POSITION;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VaryingsTAA
			{
				half4  positionCS   : SV_POSITION;
				half4  uv           : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
			};
			struct AttributesBTAA
			{
				uint vertexID : SV_VertexID;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			VaryingsTAA VertexTAA(AttributesBTAA input)//VaryingsTAA VertexTAA(AttributesTAA input)
			{
				VaryingsTAA output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				//output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
				output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
				output.uv.xy = GetFullScreenTriangleTexCoord(input.vertexID);
				output.uv.y = output.uv.y;// v0.3 - removed / 1.8;

				float4 projPos = output.positionCS * 0.5;
				projPos.xy = projPos.xy + projPos.w;

				//output.uv.xy = input.texcoord;// UnityStereoTransformScreenSpaceTex(input.texcoord);
				output.uv.zw = projPos.xy;

				return output;
			}

			//float4 sample_color(Texture2D<float4> tex, float2 uv)
			//{
			//	return SAMPLE_TEXTURE2D_X(_MainTexB, sampler_MainTexB, uv);// sampler_LinearClamp, uv);
			//}

			float2 historyPostion(float2 un_jitted_uv)
			{
				float depth = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, un_jitted_uv).r;// sampler_PointClamp, un_jitted_uv).r;
#if UNITY_REVERSED_Z
				depth = 1.0 - depth;
#endif
				depth = 2.0 * depth - 1.0;
#if UNITY_UV_STARTS_AT_TOP
				un_jitted_uv.y = 1.0f - un_jitted_uv.y;
#endif
				float3 viewPos = ComputeViewSpacePosition(un_jitted_uv, depth, _I_P_Current_jittered);
				//float4 worldPos = float4(mul(unity_CameraToWorld, float4(viewPos, 1.0)).xyz, 1.0);
				//float3 vpos = ComputeViewSpacePosition(input, zsample);
				float4 worldPos = float4(mul(_InverseView, float4(viewPos, 1)).xyz, 1.0);

				float4 historyNDC = mul(_PrevViewProjM_TAA, worldPos);
				historyNDC /= historyNDC.w;
				historyNDC.xy = historyNDC.xy * 0.5f + 0.5f;
				return historyNDC.xy;
			}

			float4 clip_aabb(float3 aabb_min, float3 aabb_max, float4 avg, float4 input_texel)
			{
				float3 p_clip = 0.5 * (aabb_max + aabb_min);
				float3 e_clip = 0.5 * (aabb_max - aabb_min) + FLT_EPS;
				float4 v_clip = input_texel - float4(p_clip, avg.w);
				float3 v_unit = v_clip.xyz / e_clip;
				float3 a_unit = abs(v_unit);
				float ma_unit = max(a_unit.x, max(a_unit.y, a_unit.z));

				if (ma_unit > 1.0)
					return float4(p_clip, avg.w) + v_clip / ma_unit;
				else
					return input_texel;
			}



			float4 FragTAA(VaryingsTAA input) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
				float2 uv_jitted = input.uv;// UnityStereoTransformScreenSpaceTex(input.uv);
				float2 un_jitted_uv = uv_jitted - _TAA_Params.xy;
				float4 color = SAMPLE_TEXTURE2D_X(_MainTexB, sampler_MainTexB, un_jitted_uv);//sampler_LinearClamp, un_jitted_uv);
				float4 color_min, color_max, color_avg;
				minmax(un_jitted_uv, color_min, color_max, color_avg);
				float2 previousTC = historyPostion(un_jitted_uv);
				float4 prev_color = SAMPLE_TEXTURE2D_X(_TAA_Pretexture, sampler_MainTexB, previousTC); // sampler_LinearClamp, previousTC);
				prev_color = clip_aabb(color_min, color_max, color_avg, prev_color);
				float4 result_color = lerp(color, prev_color, _TAA_Params.z);
				//return float4(1,0,0,1);// result_color;
				//return (prev_color + color)/2;
				return result_color;
			}
				/////////////////////////////////////// END TEMPORAL AA ///////////////////////////////

				ENDHLSL

				SubShader
			{

				//Pass
				//{
				//	Name "SunShaftsSM_HDRP"
				//	ZWrite Off
				//	ZTest Always
				//	Blend Off
				//	Cull Off

				//	HLSLPROGRAM
				//		#pragma fragment frag_radial
				//		#pragma vertex vert_radial
				//	ENDHLSL
				//}

				//	//pass 1
				//	Pass
				//	{
				//		Name "SunShaftsSM_HDRP"
				//		ZWrite Off
				//		ZTest Always
				//		Blend Off
				//		Cull Off

				//		HLSLPROGRAM
				//		#pragma fragment CustomPostProcess
				//		#pragma vertex vert_radial
				//		ENDHLSL
				//	}

				//////////////////////////
				Pass{ // .0
					ZTest Always Cull Off ZWrite Off

					HLSLPROGRAM

					#pragma vertex Vert
					#pragma fragment fragScreen

					ENDHLSL
				}

					Pass{ // .1
						ZTest Always Cull Off ZWrite Off

						HLSLPROGRAM

						#pragma vertex vert_radial
						#pragma fragment frag_radial

						ENDHLSL
				}

					Pass{ // .2
						ZTest Always Cull Off ZWrite Off

						HLSLPROGRAM

						#pragma vertex Vert
						#pragma fragment frag_depth

						ENDHLSL
				}

					Pass{ // .3
						ZTest Always Cull Off ZWrite Off

						HLSLPROGRAM

						#pragma vertex Vert
						#pragma fragment frag_nodepth

						ENDHLSL
				}

					Pass{ // .4
						ZTest Always Cull Off ZWrite Off

						HLSLPROGRAM

						#pragma vertex Vert
						#pragma fragment fragAdd

						ENDHLSL
				}

					//PASS  // .5
					Pass{
						ZTest Always Cull Off ZWrite Off

						HLSLPROGRAM

						#pragma vertex Vert
						#pragma fragment FragGrey

						ENDHLSL
				}
					//PASS6  // .6
					Pass{
						ZTest Always Cull Off ZWrite Off

						HLSLPROGRAM

						#pragma vertex Vert
						#pragma fragment CustomPostProcess

						ENDHLSL
				}
					//PASS6  // .7 - WATER MASK
					Pass{
						ZTest Always Cull Off ZWrite Off

						HLSLPROGRAM

						#pragma vertex Vert
						#pragma fragment waterMaskFrag

						ENDHLSL
				}
					//////////////////////////
					//v4.3
					Pass{ // 8 - blend with water interface mask
						HLSLPROGRAM//CGPROGRAM
						#pragma vertex Vert
						#pragma fragment FragmentProgram

							half4 FragmentProgram(VaryingsA i) : SV_Target{

					//half4 c = tex2D(_SourceTex, i.uv);
					//half4 mask = tex2D(_WaterInterfaceTex, i.uv);
					//half4 effe = tex2D(_MainTexA, i.uv);

					uint2 positionSS = i.uv * _ScreenSize.xy * 1;
					uint2 positionSSS = i.uv * _ScreenSize.xy * scalerMask;
					float4 c = LOAD_TEXTURE2D_X(_SourceTex, positionSS);
					float4 mask = LOAD_TEXTURE2D_X(_WaterInterfaceTex, positionSSS);
					//float4 col = LOAD_TEXTURE2D_X(_MainTexA, positionSS);

					//v0.3 - remove water effect
					float4 colA = LOAD_TEXTURE2D_X(_MainTexA, positionSS);
					return half4(colA.rgb,  c.a);


					float enchanceBorder = 0;





					//BLUR
					//float blurWidthA = 0.002;					
					//half4 maskUP1A = LOAD_TEXTURE2D_X(_WaterInterfaceTex, positionSS + float2(0, blurWidthA)* _ScreenSize.y);
					//half4 maskUP2A = LOAD_TEXTURE2D_X(_WaterInterfaceTex, positionSS + float2(0, -blurWidthA)* _ScreenSize.y);
					//if (mask.g == 0) {
					//	//	//if white, check for even one black around in searchDist distance
					//	if (maskUP1A.g == 1 || maskUP2A.g == 1) {
					//		if (distortion.g > 0.01) {
					//			mask.g = 1;
					//		}
					//		result = c.rgb* (0.5) * distortion.g
					//			+ colDISTORT * (0.5)* (lerp(_underWaterTint * 2, float4(1, 1, 1, 1), uv.y * _underwaterDepthFade))* distortion.g
					//			+ 0.5*float3(0.2, 0.5, 0.8)* (0.5)* distortion.g;
					//	}

					//	float sampleWidth = 0.001 * 30;
					//	half4 maskDOWN = LOAD_TEXTURE2D_X(_WaterInterfaceTex, positionSSS + float2(0, -sampleWidth)* _ScreenSize.xy * scalerMask);
					//	half4 maskLEFT = LOAD_TEXTURE2D_X(_WaterInterfaceTex, positionSSS + float2(-sampleWidth, 0)* _ScreenSize.xy * scalerMask);
					//	half4 maskRIGHT = LOAD_TEXTURE2D_X(_WaterInterfaceTex, positionSSS + float2(sampleWidth, 0)* _ScreenSize.xy * scalerMask);
					//	//if white, check for even one black around in searchDist distance
					//	if (maskDOWN.g == 1 && (maskLEFT.g == 1 || maskRIGHT.g == 1)) {
					//		result = c.rgb* (0.5) * distortion.g
					//			+ colDISTORT * (0.5)* (lerp(_underWaterTint * 2, float4(1, 1, 1, 1), uv.y * _underwaterDepthFade))* distortion.g
					//			+ 0.5*float3(0.2, 0.5, 0.8)* (0.5)* distortion.g;
					//	}
					//}
					//if (mask.g == 1) {
					//	//	//if white, check for even one black around in searchDist distance
					//	if (maskUP1A.g == 0 || maskUP2A.g == 0) {
					//		if (distortion.g > 0.01) {
					//			mask.g = 1;
					//		}
					//		result = c.rgb* (mask.g) * distortion.g
					//			+ colDISTORT * (mask.g)* (lerp(_underWaterTint * 2, float4(1, 1, 1, 1), uv.y * _underwaterDepthFade))* distortion.g
					//			+ 0.5*float3(0.2, 0.5, 0.8)* (mask.g)* distortion.g;
					//	}
					//}

					float2 blurUV = positionSSS; //taps0
					if (mask.g == 0) {
					half4 colorM = LOAD_TEXTURE2D_X(_WaterInterfaceTex, blurUV); //v4.6													
																				 //colorM *= 1.5;

					float distM = 0.003 * 1;
					half4 color1M = LOAD_TEXTURE2D_X(_WaterInterfaceTex, blurUV + float2(distM, -distM)* _ScreenSize.xy);
					half4 color2M = LOAD_TEXTURE2D_X(_WaterInterfaceTex, blurUV + float2(-distM, -distM)* _ScreenSize.xy);
					half4 color3M = LOAD_TEXTURE2D_X(_WaterInterfaceTex, blurUV + float2(distM, distM)* _ScreenSize.xy);
					half4 color4M = LOAD_TEXTURE2D_X(_WaterInterfaceTex, blurUV + float2(-distM, distM)* _ScreenSize.xy);
					half4 color1aM = LOAD_TEXTURE2D_X(_WaterInterfaceTex, blurUV + float2(0, distM)* _ScreenSize.xy);
					half4 color2aM = LOAD_TEXTURE2D_X(_WaterInterfaceTex, blurUV + float2(0, -distM)* _ScreenSize.xy);
					half4 color3aM = LOAD_TEXTURE2D_X(_WaterInterfaceTex, blurUV + float2(distM, 0)* _ScreenSize.xy);
					half4 color4aM = LOAD_TEXTURE2D_X(_WaterInterfaceTex, blurUV + float2(-distM, 0)* _ScreenSize.xy);

						colorM += color1M + color2M + color3M + color4M;
						colorM /= 4;
						colorM += color1aM + color2aM + color3aM + color4aM;
						colorM /= 4;


					distM = 0.001 * 1;
					color1M = LOAD_TEXTURE2D_X(_WaterInterfaceTex, blurUV + float2(distM, -distM)* _ScreenSize.xy);
					color2M = LOAD_TEXTURE2D_X(_WaterInterfaceTex, blurUV + float2(-distM, -distM)* _ScreenSize.xy);
					color3M = LOAD_TEXTURE2D_X(_WaterInterfaceTex, blurUV + float2(distM, distM)* _ScreenSize.xy);
					color4M = LOAD_TEXTURE2D_X(_WaterInterfaceTex, blurUV + float2(-distM, distM)* _ScreenSize.xy);
					color1aM = LOAD_TEXTURE2D_X(_WaterInterfaceTex, blurUV + float2(0, distM)* _ScreenSize.xy);
					color2aM = LOAD_TEXTURE2D_X(_WaterInterfaceTex, blurUV + float2(0, -distM)* _ScreenSize.xy);
					color3aM = LOAD_TEXTURE2D_X(_WaterInterfaceTex, blurUV + float2(distM, 0)* _ScreenSize.xy);
					color4aM = LOAD_TEXTURE2D_X(_WaterInterfaceTex, blurUV + float2(-distM, 0)* _ScreenSize.xy);
					colorM += color1M + color2M + color3M + color4M;
					colorM /= 4;
					colorM += color1aM + color2aM + color3aM + color4aM;
					colorM /= 4;

					mask.g = colorM.g;
					}







					//v4.6
					float blurWidth = 0.002;
					//half4 maskUP1 = tex2D(_WaterInterfaceTex, i.uv + float2(0,  blurWidth));
					//half4 maskUP2 = tex2D(_WaterInterfaceTex, i.uv + float2(0, -blurWidth));//*_WaterInterfaceTex_TexelSize.y));
					half4 maskUP1 = LOAD_TEXTURE2D_X(_WaterInterfaceTex, positionSSS + float2(0, blurWidth)* _ScreenSize.y * scalerMask);
					half4 maskUP2 = LOAD_TEXTURE2D_X(_WaterInterfaceTex, positionSSS + float2(0, -blurWidth)* _ScreenSize.y * scalerMask);
					//if (mask.g == 1) {
					//	//if white, check for even one black around in searchDist distance
					//	if (maskUP1.g == 0 || maskUP2.g == 0) {
					//		//blur by sampling all around pixels
					//		//effe  = float4(11, 11, 11, 1); 

					//		//effe = effe * float4(1.4, 1, 1, 1); //v4.6
					//		mask += LOAD_TEXTURE2D_X(_WaterInterfaceTex, i.uv * _ScreenSize.xy * scalerMask);
					//		mask += LOAD_TEXTURE2D_X(_WaterInterfaceTex, i.taps[0] * _ScreenSize.xy * scalerMask);
					//		mask += LOAD_TEXTURE2D_X(_WaterInterfaceTex, i.taps[1] * _ScreenSize.xy * scalerMask);
					//		mask += LOAD_TEXTURE2D_X(_WaterInterfaceTex, i.taps[2] * _ScreenSize.xy * scalerMask);
					//		mask += LOAD_TEXTURE2D_X(_WaterInterfaceTex, i.taps[3] * _ScreenSize.xy * scalerMask);

					//		//effe += tex2D(_MainTex, i.taps[1]);
					//		//effe += tex2D(_MainTex, i.taps[2]);
					//		//effe += tex2D(_MainTex, i.taps[3]);
					//		//mask += enchanceBorder;
					//	}
					//}
					//if (mask.g == 0) {
					//	//if white, check for even one black around in searchDist distance
					//	if (maskUP1.g == 1 || maskUP2.g == 1) {
					//		//blur by sampling all around pixels			 
					//		//c.rgb = float4(11, 11, 11, 1);

					//		//c.rgb = c.rgb * float4(5, 5, 1, 1);
					//		mask += LOAD_TEXTURE2D_X(_WaterInterfaceTex, i.uv * _ScreenSize.xy * scalerMask);
					//		mask += LOAD_TEXTURE2D_X(_WaterInterfaceTex, i.taps[0] * _ScreenSize.xy * scalerMask);
					//		mask += LOAD_TEXTURE2D_X(_WaterInterfaceTex, i.taps[1] * _ScreenSize.xy * scalerMask);
					//		mask += LOAD_TEXTURE2D_X(_WaterInterfaceTex, i.taps[2] * _ScreenSize.xy * scalerMask);
					//		mask += LOAD_TEXTURE2D_X(_WaterInterfaceTex, i.taps[3] * _ScreenSize.xy * scalerMask);
					//		mask += enchanceBorder;
					//		//c.rgb += tex2D(_SourceTex, i.taps[1]);
					//		//c.rgb += tex2D(_SourceTex, i.taps[2]);
					//		//c.rgb += tex2D(_SourceTex, i.taps[3]);
					//	}
					//}

					//FILTER AA MASK
					//if (mask.g == 0) {		
					//	float sampleWidth = 0.001 * 30;
					//	half4 maskDOWN = LOAD_TEXTURE2D_X(_WaterInterfaceTex, positionSSS + float2(0, -sampleWidth)* _ScreenSize.xy * scalerMask);
					//	half4 maskLEFT = LOAD_TEXTURE2D_X(_WaterInterfaceTex, positionSSS + float2( -sampleWidth,0)* _ScreenSize.xy * scalerMask);
					//	half4 maskRIGHT = LOAD_TEXTURE2D_X(_WaterInterfaceTex, positionSSS + float2(sampleWidth,0)* _ScreenSize.xy * scalerMask);
					//	//if white, check for even one black around in searchDist distance
					//	if (maskDOWN.g == 1 && (maskLEFT.g == 1 || maskRIGHT.g == 1) ) {							
					//		mask.g = 1;							
					//	}
					//}


					float2 uv = i.uv * _ScreenSize.xy * 1;
					/////////////// REFRACTION LINE
					float2 addRefract = float2(_refractLineXDisp, _refractLineWidth) * 0.025; //v4.6b
					float rightBreakPoint = 0.84;
					if (uv.x > rightBreakPoint) {
						float dist = uv.x - rightBreakPoint;
						addRefract.x = addRefract.x * (1 - ((dist) / (1 - rightBreakPoint)));
					}
					addRefract.x = addRefract.x * 0.45;
					float upperBreakPoint = 0.75;
					if (uv.y > upperBreakPoint) {
						float dist = uv.y - upperBreakPoint;
						addRefract.y = addRefract.y * (1 - ((dist) / (1 - upperBreakPoint)));
					}
					//v4.6
					float2 taps0 = (i.taps[0] * _ScreenSize.xy + addRefract - float2(_refractLineXDispA,0)* _ScreenSize.x) * 1;
					float2 taps1 = (i.taps[1] * _ScreenSize.xy + addRefract) * 1;
					float2 taps2 = (i.taps[2] * _ScreenSize.xy + addRefract) * 1;
					float2 taps3 = (i.taps[3] * _ScreenSize.xy + addRefract) * 1;

					//FIX BLACK WHEN GO ABOVE TEXTURE in refraction
					if (taps0.y >= 0.93 * _ScreenSize.y) {
						taps0.y = 0.93 * _ScreenSize.y;
					}

					float4 col = LOAD_TEXTURE2D_X(_MainTexA, taps0);// positionSS);

					//float4 col = LOAD_TEXTURE2D_X(_MainTexA, i.taps[0] * _ScreenSize.xy);
					//float4 col1 = LOAD_TEXTURE2D_X(_MainTexA, uv.y * 1);
					//return col1;

					//half4 color = LOAD_TEXTURE2D_X(_MainTexA, taps0); //v4.6													
					//color *= 1.5;				

					//float dist = 0.006* 1;
					//half4 color1 = LOAD_TEXTURE2D_X(_MainTexA, taps0 + float2(dist, -dist)* _ScreenSize.xy);
					//half4 color2 = LOAD_TEXTURE2D_X(_MainTexA, taps0 + float2(-dist, -dist)* _ScreenSize.xy);
					//half4 color3 = LOAD_TEXTURE2D_X(_MainTexA, taps0 + float2(dist, dist)* _ScreenSize.xy);
					//half4 color4 = LOAD_TEXTURE2D_X(_MainTexA, taps0 + float2(-dist, dist)* _ScreenSize.xy);
					//half4 color1a = LOAD_TEXTURE2D_X(_MainTexA, taps0 + float2(0, dist)* _ScreenSize.xy);
					//half4 color2a = LOAD_TEXTURE2D_X(_MainTexA, taps0 + float2(0, -dist)* _ScreenSize.xy);
					//half4 color3a = LOAD_TEXTURE2D_X(_MainTexA, taps0 + float2(dist, 0)* _ScreenSize.xy);
					//half4 color4a = LOAD_TEXTURE2D_X(_MainTexA, taps0 + float2(-dist, 0)* _ScreenSize.xy);

					//color += color1 + color2 + color3 + color4;
					//color /= 4;
					//color += color1a + color2a + color3a + color4a;
					//color /= 4;

					//dist = 0.003* 1;
					//color1 = LOAD_TEXTURE2D_X(_MainTexA, taps0 + float2(dist, -dist)* _ScreenSize.xy);
					//color2 = LOAD_TEXTURE2D_X(_MainTexA, taps0 + float2(-dist, -dist)* _ScreenSize.xy);
					//color3 = LOAD_TEXTURE2D_X(_MainTexA, taps0 + float2(dist, dist)* _ScreenSize.xy);
					//color4 = LOAD_TEXTURE2D_X(_MainTexA, taps0 + float2(-dist, dist)* _ScreenSize.xy);
					//color1a = LOAD_TEXTURE2D_X(_MainTexA, taps0 + float2(0, dist)* _ScreenSize.xy);
					//color2a = LOAD_TEXTURE2D_X(_MainTexA, taps0 + float2(0, -dist)* _ScreenSize.xy);
					//color3a = LOAD_TEXTURE2D_X(_MainTexA, taps0 + float2(dist, 0)* _ScreenSize.xy);
					//color4a = LOAD_TEXTURE2D_X(_MainTexA, taps0 + float2(-dist, 0)* _ScreenSize.xy);
					//color += color1 + color2 + color3 + color4;
					//color /= 4;
					//color += color1a + color2a + color3a + color4a;
					//color /= 4;

					//half3 resultA = color.rgb *(mask.g)*1 + LOAD_TEXTURE2D_X(_MainTexA, uv).rgb * (1 - mask.g);
					////////////// END REFRACTION LINE

					half4 maskDISP = LOAD_TEXTURE2D_X(_WaterInterfaceTex, uv + float2(0, _BumpLineHeight) * _ScreenSize.xy);
					if (maskDISP.g == 0) {  //if (maskDISP.r == 0) { 
						_BumpMagnitude = _BumpMagnitudeRL;
						_BumpScale = _BumpScaleRL;
						//v4.6b - gradient refract 
						float gradDiff = uv.y - (mask.g) + _BumpLineHeight;
						float _BumpGradIntensity = 1;
						_BumpMagnitude = _BumpMagnitude * pow(gradDiff, _BumpGradIntensity) * 0.11 * 115;
					}
					//add bump map
					//half4 bump = LOAD_TEXTURE2D_X(_BumpTex, i.uv * _BumpScale* 1 + float2(_BumpVelocity.x*abs((_Time.y*_BumpVelocity.z + 2.4)), _BumpVelocity.y*abs((_Time.y*_BumpVelocity.w + 1.4))) * _ScreenSize.xy );
					//half4 bump = LOAD_TEXTURE2D_X(_BumpTex, uv);
					//half3 worldNormal = PerPixelNormal(_BumpTex, i.uv, float3(0,1,0), 1);

					//half3 PerPixelNormal(sampler2D bumpMap, half4 coords, half3 vertexNormal, half bumpStrength)
					//half3 PerPixelNormalUnpacked(sampler2D bumpMap, half4 coords, half bumpStrength)
					float2 bumpUVs = i.uv * _BumpScale
					+ float2(_BumpVelocity.x*abs((_Time.y*_BumpVelocity.z + 2.4)), _BumpVelocity.y*abs((_Time.y*_BumpVelocity.w + 1.4))) * _ScreenSize.xy;
					//half2 distortion = 1;
					//half2 distortion = PerPixelNormal(_BumpTex, i.uv, half4(0,1,0), 1).rg;
					half3 bump = PerPixelNormal(_BumpTex, float4(bumpUVs.x, bumpUVs.y, 0, 0), half3(0, 1, 0), 1);
					half2 distortion = UnpackNormal(float4(bump,1)).rg;
					//uv.xy += distortion * _BumpMagnitude *0.4;
					taps0.xy += distortion * _BumpMagnitude *0.4;

					//if (taps0.y >= 0.9 * _ScreenSize.y) {
					//	taps0.y = 0.9 * _ScreenSize.y;
					//}

					half4 colDISTORT = LOAD_TEXTURE2D_X(_MainTexA, taps0);// uv);
					half3 resultB = c.rgb *(1 - mask.g) + colDISTORT.rgb * mask.g * (lerp(_underWaterTint * 2, float4(1, 1, 1, 1), uv.y * _underwaterDepthFade));
					//resultB = saturate(float3(distortion.r, distortion.g, distortion.r)  *1) + float3(1,1,1)*0.01;
					///////////// END REFRACT LINE 2


					/*if (distortion.g > 0.5) {
						mask.g = 1;
					}*/

					//half3 result =  c.rgb* (1 - mask.g) + col * (mask.g) + float3(0.2,0.5,0.8)* (mask.g);
					half3 result = c.rgb* (1 - mask.g)
						+ colDISTORT * (mask.g)* (lerp(_underWaterTint * 2, float4(1, 1, 1, 1), uv.y * _underwaterDepthFade) + 0.5)
						;// +0.5*float3(0.2, 0.5, 0.8)* (mask.g);


					half4 color = half4(result.rgb,1);//LOAD_TEXTURE2D_X(_MainTexA, taps0); //v4.6													
					//color *= 1.5;
					//if (mask.g > 0.8) 
					//{
					//	float dist = 0.006 * 1;
					//	half4 color1 = LOAD_TEXTURE2D_X(_MainTexA, taps0 + float2(dist, -dist)* _ScreenSize.xy);
					//	half4 color2 = LOAD_TEXTURE2D_X(_MainTexA, taps0 + float2(-dist, -dist)* _ScreenSize.xy);
					//	half4 color3 = LOAD_TEXTURE2D_X(_MainTexA, taps0 + float2(dist, dist)* _ScreenSize.xy);
					//	half4 color4 = LOAD_TEXTURE2D_X(_MainTexA, taps0 + float2(-dist, dist)* _ScreenSize.xy);
					//	half4 color1a = LOAD_TEXTURE2D_X(_MainTexA, taps0 + float2(0, dist)* _ScreenSize.xy);
					//	half4 color2a = LOAD_TEXTURE2D_X(_MainTexA, taps0 + float2(0, -dist)* _ScreenSize.xy);
					//	half4 color3a = LOAD_TEXTURE2D_X(_MainTexA, taps0 + float2(dist, 0)* _ScreenSize.xy);
					//	half4 color4a = LOAD_TEXTURE2D_X(_MainTexA, taps0 + float2(-dist, 0)* _ScreenSize.xy);

					//	color += color1 + color2 + color3 + color4;
					//	color /= 4;
					//	color += color1a + color2a + color3a + color4a;
					//	color /= 4;

					//	dist = 0.003 * 1;
					//	color1 = LOAD_TEXTURE2D_X(_MainTexA, taps0 + float2(dist, -dist)* _ScreenSize.xy);
					//	color2 = LOAD_TEXTURE2D_X(_MainTexA, taps0 + float2(-dist, -dist)* _ScreenSize.xy);
					//	color3 = LOAD_TEXTURE2D_X(_MainTexA, taps0 + float2(dist, dist)* _ScreenSize.xy);
					//	color4 = LOAD_TEXTURE2D_X(_MainTexA, taps0 + float2(-dist, dist)* _ScreenSize.xy);
					//	color1a = LOAD_TEXTURE2D_X(_MainTexA, taps0 + float2(0, dist)* _ScreenSize.xy);
					//	color2a = LOAD_TEXTURE2D_X(_MainTexA, taps0 + float2(0, -dist)* _ScreenSize.xy);
					//	color3a = LOAD_TEXTURE2D_X(_MainTexA, taps0 + float2(dist, 0)* _ScreenSize.xy);
					//	color4a = LOAD_TEXTURE2D_X(_MainTexA, taps0 + float2(-dist, 0)* _ScreenSize.xy);
					//	color += color1 + color2 + color3 + color4;
					//	color /= 4;
					//	color += color1a + color2a + color3a + color4a;
					//	color /= 4;
					//}
					//result = color;// +color * (mask.g - 0.85);

					//mask.g = mask.g * pow(1-i.uv.y,6);
					//return float4(mask.rgb, c.a);

					return half4(result, c.a);
			}
			ENDHLSL//ENDCG
				}//END PASS 8
				////////////////////////////







				//////////////////// FULL VOLUMETRIC CLOUDS v0.1 //////////////////////

				///////////// PASS FOG
				Pass{/// PASS 9   // Previously 6 in URP ////////////////////////////////// PASS 9 
					ZTest Always Cull Off ZWrite Off
					HLSLPROGRAM

					#pragma vertex Vertex
					#pragma fragment Fragment// FragmentTWO

					ENDHLSL
			}
				/// 3: combine // /// PASS 10   // Previously 7 in URP ////////////////////////////////// PASS 10
				Pass
			{
				Cull Off //v0.1 - solve issue with GL.invertculling !!!
				HLSLPROGRAM
				#pragma vertex vert
				#pragma fragment frag7
				#pragma target 3.0					
				ENDHLSL
			}
				// 4: combine split frames // /// PASS 11  // Previously 8 in URP ////////////////////////////////// PASS 11 
				Pass
			{
				HLSLPROGRAM
				#pragma vertex vert
				#pragma fragment frag8
				#pragma target 3.0				
				ENDHLSL
			}
				// 5: combine previous and current frames // /// PASS 12   // Previously 9 in URP ////////////////////////////////// PASS 12 
				Pass
			{
				HLSLPROGRAM
				#pragma vertex vert//vert
				#pragma fragment frag9
				#pragma target 3.0					
				ENDHLSL
			}
				///////////// END PASS FOG

				///////////// PASS FULL VOLUMETRIC CLOUDS
				Pass{ // /// PASS 13   // Previously 10 in URP ////////////////////////////////// PASS 13 
				ZTest Always Cull Off ZWrite Off
				HLSLPROGRAM

				#pragma vertex Vertex_FV//vert_FV
				#pragma fragment frag_FV

				ENDHLSL
			}
				//fragCombineClouds
				Pass{ // /// PASS 14   // Previously 11 in URP ////////////////////////////////// PASS 14 
					ZTest Always Cull Off ZWrite Off
					HLSLPROGRAM

					#pragma vertex vert_FV
					#pragma fragment fragCombineClouds

					ENDHLSL
			}
				///////////// END PASS FULL VOLUMETRIC CLOUDS

			///  PASS 15 
				Pass
			{
				ZTest Always ZWrite Off Cull Off
				Name "Stop NaN"
				HLSLPROGRAM
				#pragma vertex VertexTAA
				#pragma fragment FragTAA
				ENDHLSL
			}

					////////////// TEMPORAL - https://github.com/cdrinmatane/SSRT - MIT LICENSE
									//https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@13.1/manual/renderer-features/how-to-fullscreen-blit-in-xr-spi.html
					Pass // 16
				{
					Name "TemporalReproj"
					HLSLPROGRAM
					#pragma vertex VertexTAA
					#pragma fragment frag

					struct AttributesA
					{
						float4 positionHCS   : POSITION;
						float2 uv           : TEXCOORD0;
						UNITY_VERTEX_INPUT_INSTANCE_ID
					};

					//struct VaryingsAA
					//{
					//	float4  positionCS  : SV_POSITION;
					//	float2  uv          : TEXCOORD0;
					//	UNITY_VERTEX_OUTPUT_STEREO
					//};

					//VaryingsAA vert(AttributesA input)
					//{
					//	VaryingsAA output;
					//	UNITY_SETUP_INSTANCE_ID(input);
					//	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

					//	// Note: The pass is setup with a mesh already in clip
					//	// space, that's why, it's enough to just output vertex
					//	// positions
					//	output.positionCS = float4(input.positionHCS.xyz, 1.0);

					//	#if UNITY_UV_STARTS_AT_TOP
					//	output.positionCS.y *= -1;
					//	#endif

					//	output.uv = input.uv;
					//	return output;
					//}

					//sampler2D _PreviousColor;
					//sampler2D _PreviousDepth;
					TEXTURE2D_X(_PreviousColor);
					SAMPLER(sampler_PreviousColor);
					TEXTURE2D_X(_PreviousDepth);
					SAMPLER(sampler_PreviousDepth);
					//float4 sampler_PreviousColor;

					// Transformation matrices
					float4x4 _CameraToWorldMatrix;
					float4x4 _InverseProjectionMatrix;
					float4x4 _LastFrameViewProjectionMatrix;
					float4x4 _InverseViewProjectionMatrix;
					float4x4 _LastFrameInverseViewProjectionMatrix;
					//float _TemporalResponse;
					/*struct v2f
					{
						float4 pos : SV_POSITION;
						float4 uv : TEXCOORD0;
					};*/
					float LinearEyeDepthB(float z)
					{
						return 1.0 / (_ZBufferParams.z * z + _ZBufferParams.w);
					}
					float4 frag(VaryingsTAA  input) : SV_Target
					{
						//UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
						//float4 color = SAMPLE_TEXTURE2D(_CloudTex, sampler_CloudTex, input.uv);
						//return color*1;

						float2 uv = input.uv.xy;
						float2 oneOverResolution = (1.0 / _ScreenParams.xy);

			//			float4 gi = SAMPLE_TEXTURE2D(_CloudTex, sampler_CloudTex, input.uv.xy);// tex2D(_MainTex, input.uv.xy);
						float4 gi = SAMPLE_TEXTURE2D_X(_CloudTex, sampler_CloudTex, input.uv.xy);// tex2D(_MainTex, input.uv.xy);

						//DEBUG
						//float4 giP = SAMPLE_TEXTURE2D(_PreviousColor, sampler_PreviousColor, input.uv.xy);
						//return giP;//

						//float4 depthIMG = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, input.uv.xy);
						float4 depthIMG = LOAD_TEXTURE2D_X(_CameraDepthTexture, input.uv.xy * _ScreenSize.xy);


						float depth = LinearEyeDepthB(depthIMG.x);// depthIMG.x; //LinearEyeDepth(depthIMG.x);
						float4 currentPos = float4(input.uv.x * 2.0 - 1.0, input.uv.y * 2.0 - 1.0, depth * 2.0 - 1.0, 1.0);

						float4 fragpos = mul(_InverseViewProjectionMatrix, float4(float3(uv * 2 - 1, depth), 1));
						fragpos.xyz /= fragpos.w;
						float4 thisWorldPosition = fragpos;

						float2 motionVectors = float2(0,0);// tex2Dlod(_CameraMotionVectorsTexture, float4(input.uv.xy, 0.0, 0.0)).xy;
						float2 reprojCoord = input.uv.xy - motionVectors.xy;

						//float prevDepth = SAMPLE_DEPTH_TEXTURE(_PreviousDepth, sampler_PreviousDepth, float2(reprojCoord + oneOverResolution * 0.0)).x;
						float prevDepth = LOAD_TEXTURE2D_X(_PreviousDepth, float2(reprojCoord + oneOverResolution * 0.0)* _ScreenSize.xy).x;

						//tex2Dlod(_PreviousDepth, sampler_PreviousDepth, float4(reprojCoord + oneOverResolution * 0.0, 0.0, 0.0)).x;//LinearEyeDepth(tex2Dlod(_PreviousDepth, float4(reprojCoord + oneOverResolution * 0.0, 0.0, 0.0)).x);
					prevDepth = LinearEyeDepthB(prevDepth);

					float4 previousWorldPosition = mul(_LastFrameInverseViewProjectionMatrix, float4(reprojCoord.xy * 2.0 - 1.0, prevDepth, 1.0)); //prevDepth * 2.0 - 1.0, 1.0));
					previousWorldPosition /= previousWorldPosition.w;

					float blendWeight = 0.15 * _TemporalResponse;

					float posSimilarity = saturate(1.0 - distance(previousWorldPosition.xyz, thisWorldPosition.xyz) * 1.0);
					blendWeight = lerp(1.0, blendWeight, posSimilarity);

					float4 minPrev = float4(10000, 10000, 10000, 10000);
					float4 maxPrev = float4(0, 0, 0, 0);

					float4 s0 = SAMPLE_TEXTURE2D_X(_CloudTex, sampler_CloudTex, float2(input.uv.xy + oneOverResolution * float2(0.5, 0.5)));// tex2Dlod(_MainTex, float4(input.uv.xy + oneOverResolution * float2(0.5, 0.5), 0, 0));
					minPrev = s0;
					maxPrev = s0;
					s0 = SAMPLE_TEXTURE2D_X(_CloudTex, sampler_CloudTex, float2(input.uv.xy + oneOverResolution * float2(0.5, -0.5)));
					// tex2Dlod(_MainTex, float4(input.uv.xy + oneOverResolution * float2(0.5, -0.5), 0, 0));
					minPrev = min(minPrev, s0);
					maxPrev = max(maxPrev, s0);
					s0 = SAMPLE_TEXTURE2D_X(_CloudTex, sampler_CloudTex, float2(input.uv.xy + oneOverResolution * float2(-0.5, 0.5)));
					//  tex2Dlod(_MainTex, float4(input.uv.xy + oneOverResolution * float2(-0.5, 0.5), 0, 0));
					minPrev = min(minPrev, s0);
					maxPrev = max(maxPrev, s0);
					s0 = SAMPLE_TEXTURE2D_X(_CloudTex, sampler_CloudTex, float2(input.uv.xy + oneOverResolution * float2(-0.5, -0.5)));
					//  tex2Dlod(_MainTex, float4(input.uv.xy + oneOverResolution * float2(-0.5, -0.5), 0, 0));
					minPrev = min(minPrev, s0);
					maxPrev = max(maxPrev, s0);

					float4 prevGI = SAMPLE_TEXTURE2D_X(_PreviousColor, sampler_PreviousColor, float2(reprojCoord.xy));
					// //tex2Dlod(_PreviousColor, float4(reprojCoord, 0.0, 0.0));
					prevGI = lerp(prevGI, clamp(prevGI, minPrev, maxPrev), 0.25);

					gi = lerp(prevGI, gi, float4(blendWeight, blendWeight, blendWeight, blendWeight)*0.65 * _TemporalGain);

					return gi;// (gi + prevGI) / 2;
				}
					ENDHLSL
				}

				Pass //17
				{
					Name"GetDepth"
					HLSLPROGRAM//CGPROGRAM
					#pragma vertex vert
					#pragma fragment frag

					struct AttributesA
					{
						float4 positionHCS   : POSITION;
						float2 uv           : TEXCOORD0;
						UNITY_VERTEX_INPUT_INSTANCE_ID
					};

					struct VaryingsAA
					{
						float4  positionCS  : SV_POSITION;
						float2  uv          : TEXCOORD0;
						UNITY_VERTEX_OUTPUT_STEREO
					};

					VaryingsAA vert(AttributesA input)
					{
						VaryingsAA output;
						UNITY_SETUP_INSTANCE_ID(input);
						UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

						// Note: The pass is setup with a mesh already in clip
						// space, that's why, it's enough to just output vertex
						// positions
						output.positionCS = float4(input.positionHCS.xyz, 1.0);

						#if UNITY_UV_STARTS_AT_TOP
						output.positionCS.y *= -1;
						#endif

						output.uv = input.uv;
						return output;
					}

					/*struct v2f
					{
						float4 pos : SV_POSITION;
						float4 uv : TEXCOORD0;
					};*/
					float4 frag(VaryingsAA input) : COLOR0
					{
						float2 coord = input.uv.xy + (1.0 / _ScreenParams.xy) * 0.5;
						//float4 tex = tex2D(_CameraDepthTexture, coord);

						//uint2 positionSS = coord * _ScreenSize.xy;
						float4 tex = LOAD_TEXTURE2D_X(_CameraDepthTexture, coord * _ScreenSize.xy);

						//float4 tex = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, coord);
						//float4 tex = UNITY_SAMPLE_TEX2DARRAY_LOD(_CameraDepthTexture, float4(positionSS.xy * float2(1, 0.6667) / 1, 0, 0), 0);
						//half depthRaw = UNITY_SAMPLE_TEX2DARRAY_LOD(_CameraDepthTexture, float4(positionSS.xy * float2(1, 0.6667) / 1, 0, 0), 0).r;//v0.4a HDRP 10.2
						//float zsample = Linear01DepthA(i.uv.xy);
						//float depth = Linear01Depth(zsample * (zsample < 1.0), _ZBufferParams);

						return tex;
					}
					ENDHLSL
				}


				//////////////////// END FULL VOLUMETRIC CLOUDS v0.1 //////////////////////




			}
			Fallback Off
}