using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
//using Artngame.BrunetonsOcean;

namespace Artngame.SKYMASTER
{
    //v0.1 - HDRP volumetric clouds image effect
    public class connectSuntoNebulaCloudsHDRP : MonoBehaviour
    {
        public GameObject volumeObj;
        public bool renderClouds = true;
        [Range(0, 1)]
        public int cloudChoice = 0;
        [Header("------------------------------------------------")]
        [Header("Weather Mask and Fog of War Textures")]
        [Header("------------------------------------------------")]
        ///////// VOLUME CLOUDS v0.1
        public Texture2D WeatherTexture;

        public RenderTexture maskTexture;//v0.1.1
        public RenderTexture maskHistoryTexture;//v0.1.1 - top down camera texture
        public float maskScale = 0f;
        public Camera topDownMaskCamera;
        public Material maskMat;
        public float fogOfWarRadius = 0; //distance to make opque around player
        public float fogOfWarPower = 0;
        public Transform playerPos;

        [Header("------------------------------------------------")]
        [Header("Fly Through Clouds")]
        [Header("------------------------------------------------")]
        public float cameraScale = 80; //use 80 for correct cloud scaling in relation to land //v0.4
        public float extendFarPlaneAboveClouds = 1;
        /////// FULL VOLUMETRIC CLOUDS
        [Tooltip("Fog top Y coordinate")]
        public float height = 1.0f;
        [Tooltip("Distance fog is based on radial distance from camera when checked")]
        public bool useRadialDistance = false;

        //v0.6
        public Vector3 cloudDistanceParams = new Vector3(0, 0, 1);
        //put controlBackAlphaPower = 1, controlCloudAlphaPower = 0.001 if inside or above clouds
        public bool autoRegulateEdgeMode = true; //the new edges mode is only for view below clouds, regulate out of the mode if inside or above
        public float autoRegulateCutoffDistance = 1000;//stop new edge blend mode when near or above clouds
        public float controlBackAlphaPower = 1;//2
        public float controlCloudAlphaPower = 0.001f;//1
        float controlBackAlphaPowerP = 1;
        float controlCloudAlphaPowerP = 0.001f;
        public Vector4 controlCloudEdgeA = new Vector4(1, 1, 1, 1);//0.65, 1.22, 1.14, 1.125
        public Vector4 controlCloudEdgeAP = new Vector4(1, 1, 1, 1);
        public float controlCloudEdgeOffset = 1;
        public float depthDilation = 0;//2204
        float depthDilationP = 0;
        public bool enabledTemporalAA = false;
        public float TemporalResponse = 1;
        public float TemporalGain = 1;

        //v0.7 - VORTEX - SUPERCELLS WIP
        public bool enableVortex = false;
        public bool resetVortex = false;
        public Vector4 vortexPosRadius = new Vector4(0, 0, 0, 280000);
        public Vector4 vortexControlsA = new Vector4(1, 1, 1, 1);
        public Vector4 superCellPosRadius = new Vector4(0, 0, 0, 1000);
        public Vector4 superCellControlsA = new Vector4(1, 1, 1, 1);

        //v0.8 - PLANETARY ATMOSPHERE
        public bool enablePlanetAtmosphere = false;
        public float imageFXBlend = 1;
        public float SCALE = 1000.0f;
        public int TRANSMITTANCE_WIDTH = 256;
        public int TRANSMITTANCE_HEIGHT = 64;
        public int TRANSMITTANCE_CHANNELS = 3;
        public int IRRADIANCE_WIDTH = 64;
        public int IRRADIANCE_HEIGHT = 16;
        public int IRRADIANCE_CHANNELS = 3;
        public int INSCATTER_WIDTH = 256;
        public int INSCATTER_HEIGHT = 128;
        public int INSCATTER_DEPTH = 32;
        public int INSCATTER_CHANNELS = 4;
        //public GameObject m_sun;
        //public RenderTexture m_skyMap;
        public Vector3 m_betaR = new Vector3(0.0058f, 0.0135f, 0.0331f);
        public float m_mieG = 0.75f;
        public float m_sunIntensity = 100.0f;
        //public ComputeShader m_writeData;
        public Vector3 EarthPosition;
        public float RG = 6360.0f, RT = 6420.0f, RL = 6421.0f;
        public float HR = 8;
        public float HM = 1.2f;
        //private CommandBuffer lightingBuffer;
        //private new Camera camera;
        public float MinViewDistance = 3000;
        //public RenderTexture m_transmittance;
        //public RenderTexture m_inscatter;
        //public RenderTexture m_irradiance;
        public Material t_atmosphereImageEffect;
        public ComputeShader m_writeData;

        //v0.5
        public Vector4 YCutHeightDepthScale = new Vector4(0, 1, 0, 1);

        //v0.4
        public bool autoTranspChange = false; //change to behind transparency shader if below certain height and to above transparents if above clouds
        public float transpToggleHeight = 100;
        public bool enableBehindTranspClouds = false; //force clouds behind transparents to appear
        public bool enableAboveTranspClouds = false; //force clouds above transparents to appear

        //v0.3
        public int scatterOn = 1;
        public int sunRaysOn = 1;
        public float zeroCountSteps = 0;
        public int sunShaftSteps = 5;
        public Vector4 raysResolution = new Vector4(1, 1, 1, 1);
        public Vector4 rayShadowing = new Vector4(1, 1, 1, 1);
        //v0.1
        public int renderInFront = 0;
        public enum RandomJitterChoice
        {
            Off,
            Random,
            BlueNoise
        }
        [HeaderAttribute("Debugging")]
        public bool debugNoLowFreqNoise = false;
        public bool debugNoHighFreqNoise = false;
        public bool debugNoCurlNoise = false;
        [HeaderAttribute("Performance")]
        [Range(1, 1024)]
        public int steps = 128;
        public bool adjustDensity = true;
        public AnimationCurve stepDensityAdjustmentCurve = new AnimationCurve(new Keyframe(0.0f, 3.019f), new Keyframe(0.25f, 1.233f), new Keyframe(0.5f, 1.0f), new Keyframe(1.0f, 0.892f));
        public bool allowFlyingInClouds = false;
        [Range(1, 8)]
        public int downSample = 1;
        public Texture2D blueNoiseTexture;
        public RandomJitterChoice randomJitterNoise = RandomJitterChoice.BlueNoise;
        public bool temporalAntiAliasing = true;
        public float spread = 1.0f;
        public float feedback = 0.0f;

        [HeaderAttribute("Cloud modeling")]
        public Gradient gradientLow;
        public Gradient gradientMed;
        public Gradient gradientHigh;
        public Texture2D curlNoise;
        public TextAsset lowFreqNoise;
        public TextAsset highFreqNoise;
        public float startHeight = 1500.0f;
        public float thickness = 4000.0f;
        public float planetSize = 35000.0f;
        public Vector3 planetZeroCoordinate = new Vector3(0.0f, 0.0f, 0.0f);
        //[Range(0.0f, 1.0f)]
        public float scale = 0.3f;
        [Range(0.0f, 32.0f)]
        public float detailScale = 13.9f;
        [Range(0.0f, 1.0f)]
        public float lowFreqMin = 0.366f;
        [Range(0.0f, 1.0f)]
        public float lowFreqMax = 0.8f;
        [Range(0.0f, 1.0f)]
        public float highFreqModifier = 0.21f;
        [Range(0.0f, 10.0f)]
        public float curlDistortScale = 7.44f;
        [Range(0.0f, 1000.0f)]
        public float curlDistortAmount = 407.0f;
        [Range(0.0f, 1.0f)]
        public float weatherScale = 0.1f;



        //[Range(0.0f, 2.0f)]
        public float coverage = 0.92f;
        [Range(0.0f, 2.0f)]
        public float cloudSampleMultiplier = 1.0f;

        [HeaderAttribute("High altitude clouds")]
        public Texture2D cloudsHighTexture;
        [Range(0.0f, 2.0f)]
        public float coverageHigh = 1.0f;
        [Range(0.0f, 2.0f)]
        public float highCoverageScale = 1.0f;
        [Range(0.0f, 1.0f)]
        public float highCloudsScale = 0.5f;

        [HeaderAttribute("Cloud Lighting")]
        public Light sunLight;
        public Color cloudBaseColor = new Color32(199, 220, 255, 255);
        public Color cloudTopColor = new Color32(255, 255, 255, 255);
        [Range(0.0f, 1.0f)]
        public float ambientLightFactor = 0.551f;
        [Range(0.0f, 5f)]//1.5
        public float sunLightFactor = 0.79f;
        public Color highSunColor = new Color32(255, 252, 210, 255);
        public Color lowSunColor = new Color32(255, 174, 0, 255);
        [Range(0.0f, 1.0f)]
        public float henyeyGreensteinGForward = 0.4f;
        [Range(0.0f, 1.0f)]
        public float henyeyGreensteinGBackward = 0.179f;
        [Range(0.0f, 200.0f)]
        public float lightStepLength = 64.0f;
        [Range(0.0f, 1.0f)]
        public float lightConeRadius = 0.4f;
        public bool randomUnitSphere = true;
        [Range(0.0f, 4.0f)]
        public float density = 1.0f;
        public bool aLotMoreLightSamples = false;

        [HeaderAttribute("Animating")]
        public float globalMultiplier = 1.0f;
        public float windSpeed = 15.9f;
        public float windDirection = -22.4f;
        public float coverageWindSpeed = 25.0f;
        public float coverageWindDirection = 5.0f;
        public float highCloudsWindSpeed = 49.2f;
        public float highCloudsWindDirection = 77.8f;

        //v0.3
        public Material shadowMat;

        public Vector3 _windOffset;
        public Vector2 _coverageWindOffset;
        public Vector2 _highCloudsWindOffset;
        public Vector3 _windDirectionVector;
        public float _multipliedWindSpeed;

        //private Texture3D _cloudShapeTexture;
        //private Texture3D _cloudDetailTexture;
        private Vector4 gradientToVector4(Gradient gradient)
        {
            if (gradient.colorKeys.Length != 4)
            {
                return new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            }
            float x = gradient.colorKeys[0].time;
            float y = gradient.colorKeys[1].time;
            float z = gradient.colorKeys[2].time;
            float w = gradient.colorKeys[3].time;
            return new Vector4(x, y, z, w);
        }
        /////// END FULL VOLUMETRIC CLOUDS

        [Header("------------------------------------------------")]
        [Header("Background Clouds")]
        [Header("------------------------------------------------")]
        public Camera reflectCamera;

        public bool enableFog = true;//v0.1 off by default
        public bool allowHDR = false;
        public Transform sun;

        public bool blendBackground = false;
        public Camera backgroundCam;
        public Material backgroundMat;

        public Texture2D testTex;
        public Texture2D testTex2;

        ////// VOLUME CLOUDS 
        public float farCamDistFactor = 1;
        //Texture2D colourPalette;
        //bool Made_texture = false;
        public Gradient DistGradient = new Gradient();
        public Vector2 GradientBounds = Vector2.zero;
        //v4.8.6
        public bool adjustNightLigting = true;
        public float backShadeNight = 0.5f; //use this at night for more dense clouds
        public float turbidityNight = 2f;
        public float extinctionNight = 0.01f;
        public float shift_dawn = 0; //add shift to when cloud lighting changes vs the TOD of sky master
        public float shift_dusk = 0;
        //v4.8.4
        //public bool adjustNightLigting = true;
        public Vector3 groundColorNight = new Vector3(0.5f, 0.5f, 0.5f);
        public float scatterNight = 0.12f; //use this at night
        public float reflectFogHeight = 1;
        //v2.1.20
        public bool WebGL = false;
        //v2.1.19
        public bool fastest = false;
        public Light localLight;
        public float localLightFalloff = 2;
        public float localLightIntensityA = 1;
        public float currentLocalLightIntensity = 0;
        public Vector3 _SkyTint = new Vector3(0.5f, 0.5f, 0.5f);
        public float _AtmosphereThickness = 1.0f;
        /// <summary>
        /// /////////////////////////////////////////////////
        /// </summary>
        ////////// CLOUDS
        public bool isForReflections = false;
        //v4.8
        public float _invertX = 0;
        public float _invertRay = 1;
        public Vector3 _WorldSpaceCameraPosC;
        public float varianceAltitude1 = 0;
        //v4.1f
        public float _mobileFactor;
        public float _alphaFactor;
        //v3.5.3
        public Texture2D _InteractTexture;
        public Vector4 _InteractTexturePos;
        public Vector4 _InteractTextureAtr;
        public Vector4 _InteractTextureOffset; //v4.0
        public float _NearZCutoff;
        public float _HorizonYAdjust;
        public float _HorizonZAdjust;
        public float _FadeThreshold;
        //v3.5 clouds	
        public float _BackShade;
        public float _UndersideCurveFactor;
        public Matrix4x4 _WorldClip;
        public float _SampleCount0 = 2;
        public float _SampleCount1 = 3;
        public int _SampleCountL = 4;
        public Texture3D _NoiseTex1;
        public Texture3D _NoiseTex2;
        public float _NoiseFreq1 = 3.1f;
        public float _NoiseFreq2 = 35.1f;
        public float _NoiseAmp1 = 5;
        public float _NoiseAmp2 = 1;
        public float _NoiseBias = -0.2f;
        public Vector3 _Scroll1 = new Vector3(0.01f, 0.08f, 0.06f);
        public Vector3 _Scroll2 = new Vector3(0.01f, 0.05f, 0.03f);
        public float _Altitude0 = 1500;
        public float _Altitude1 = 3500;
        public float _FarDist = 30000;
        public float _Scatter = 0.008f;
        public float _HGCoeff = 0.5f;
        public float _Extinct = 0.01f;
        public float _SunSize;
        public Vector3 _GroundColor; //v4.0
        public float _ExposureUnder; //v4.0
        public float frameFraction = 0;
        //v2.1.19
        //public bool _fastest;
        public Vector4 _LocalLightPos;
        public Vector4 _LocalLightColor;
        /// <summary>
        /// /////////////// PERFORMANCE HANDLE
        /// </summary>
        public int splitPerFrames = 1; //v2.1.19
        public bool cameraMotionCompensate = true;//v2.1.19    
        public float updateRate = 0.3f;
        // public int resolution = 256;
        public float downScaleFactor = 1;
        public float lowerRefrReflResFactor = 3; //v0.1
        public bool downScale = false;
        public bool _needsReset = true;
        public bool enableReproject = false;
        public bool autoReproject = false;
        ///////// END CLOUDS
        public Vector4 raysResolutionA = new Vector4(1, 1, 1, 1);
        public Color _rayColor = new Color(.969f, 0.549f, .041f, 1);
        public Vector4 rayShadowingA = new Vector4(1, 1, 1, 1);
        public Color _underDomeColor = new Color(1, 1, 1, 1);
        ////// END VOLUME CLOUDS

        [Header("------------------------------------------------")]
        [Header("Volumetric Fog")]
        [Header("------------------------------------------------")]

        //FOG
        public Light localLightA;
        public float localLightIntensity;
        public float localLightRadius;
        //FOG URP /////////////
        //FOG URP /////////////
        //FOG URP /////////////
        //public float blend =  0.5f;
        public Color _FogColor = Color.white / 2;
        //fog params
        public Texture2D noiseTexture;
        public float _startDistance = 30f;
        public float _fogHeight = 0.75f;
        public float _fogDensity = 1f;
        public float _cameraRoll = 0.0f;
        public Vector4 _cameraDiff = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
        public float _cameraTiltSign = 1;
        public float heightDensity = 1;
        public float noiseDensity = 1;
        public float noiseScale = 1;
        public float noiseThickness = 1;
        public Vector3 noiseSpeed = new Vector4(1f, 1f, 1f);
        public float occlusionDrop = 1f;
        public float occlusionExp = 1f;
        public int noise3D = 1;
        public float startDistance = 1;
        public float luminance = 1;
        public float lumFac = 1;
        public float ScatterFac = 1;
        public float TurbFac = 1;
        public float HorizFac = 1;
        public float turbidity = 1;
        public float reileigh = 1;
        public float mieCoefficient = 1;
        public float mieDirectionalG = 1;
        public float bias = 1;
        public float contrast = 1;
        public Color TintColor = new Color(1, 1, 1, 1);
        public Vector3 TintColorK = new Vector3(0, 0, 0);
        public Vector3 TintColorL = new Vector3(0, 0, 0);
        public Vector4 Sun = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
        public Vector4 SunFOG = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
        public bool FogSky = true;
        public float ClearSkyFac = 1f;
        public Vector4 PointL = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
        public Vector4 PointLParams = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);

        public bool _useRadialDistance = false;
        public bool _fadeToSkybox = true;
        //END FOG URP //////////////////
        //END FOG URP //////////////////
        //END FOG URP //////////////////

        [Header("------------------------------------------------")]
        [Header("Sun Shafts")]
        [Header("------------------------------------------------")]

        //SUN SHAFTS
        //   public BlitFullVolumeCloudsSRP.BlitSettings.ShaftsScreenBlendMode screenBlendMode = BlitFullVolumeCloudsSRP.BlitSettings.ShaftsScreenBlendMode.Screen;
        //public Vector3 sunTransform = new Vector3(0f, 0f, 0f); 
        public int radialBlurIterations = 2;
        public Color sunColor = Color.white;
        public Color sunThreshold = new Color(0.87f, 0.74f, 0.65f);
        public float sunShaftBlurRadius = 2.5f;
        public float sunShaftIntensity = 1.15f;
        public float maxRadius = 0.75f;
        public bool useDepthTexture = true;
        //PostProcessProfile postProfile;
        ////////// END VOLUME CLOUDS v0.1






        /// <summary>
        /// ///// SHAFTS
        /// </summary>
        public Transform SunLight;
        public float refractLineWidth = -1.5f;
        public float waterBlackLineWidth = 19;

        public float waterHeight;

        [Tooltip("start underwater effects below this distance to water.")]
        public float cutoffOffset = 1; //start underwater effects below this distance to water

        public bool enableShafts = false;
        public bool enableClouds = true; //v0.1

        //ACCESS MASK
        //WaterAirMaskSM waterAirMask;

        //PASS MASK
        NebulaCloudsSM_HDRP shafts;

        //VOLUME CLOUDS
        // FullVolumeCloudsSM_HDRP volumeClouds;

        //FOG
        Fog fog;

        //v0.1
        void Awake()
        {
            _needsReset = true;
            //postProfile = GetComponent<PostProcessVolume>().profile;


            //pass to shafts
            //Volume volume = volumeObj.GetComponent<Volume>();
            //FullVolumeCloudsSM_HDRP tempShafts;
            //if (volume.profile.TryGet<FullVolumeCloudsSM_HDRP>(out tempShafts))
            //{
            //    shafts = tempShafts;
            //    // Debug.Log("Got SHAFTS script");
            //}

            //if (shafts != null)
            //{
            //    passParamsToClouds();
            //}
        }

        public void passParamsToClouds()
        {
            if (shafts != null)
            {
                //FullVolumeCloudsSM_HDRP connector = shafts;

                Debug.Log("Passing params");

                connectSuntoNebulaCloudsHDRP connector = this;

                shafts.enableFog = connector.enableFog;

                shafts.cloudChoice = connector.cloudChoice;
                //////////////// FULL VOLUMETRIC CLOUDS

                shafts.WeatherTexture = connector.WeatherTexture;
                shafts.maskTexture = connector.maskTexture;//v0.1.1

                shafts.height = connector.height;
                shafts.useRadialDistance = connector.useRadialDistance;

                //v0.4
                shafts.autoTranspChange = connector.autoTranspChange;
                shafts.transpToggleHeight = connector.transpToggleHeight;
                shafts.enableBehindTranspClouds = connector.enableBehindTranspClouds;
                shafts.enableAboveTranspClouds = connector.enableAboveTranspClouds;

                //v0.3
                shafts.scatterOn = connector.scatterOn;
                shafts.sunRaysOn = connector.sunRaysOn;
                shafts.zeroCountSteps = connector.zeroCountSteps;
                shafts.sunShaftSteps = connector.sunShaftSteps;

                //v0.1
                shafts.renderInFront = connector.renderInFront;


                shafts.debugNoLowFreqNoise = connector.debugNoLowFreqNoise;
                shafts.debugNoHighFreqNoise = connector.debugNoHighFreqNoise;
                shafts.debugNoCurlNoise = connector.debugNoCurlNoise;


                shafts.steps = connector.steps;
                shafts.adjustDensity = connector.adjustDensity;
                shafts.stepDensityAdjustmentCurve = connector.stepDensityAdjustmentCurve;
                shafts.allowFlyingInClouds = connector.allowFlyingInClouds;

                shafts.downSample = connector.downSample;
                shafts.blueNoiseTexture = connector.blueNoiseTexture;

                if (connector.randomJitterNoise == RandomJitterChoice.BlueNoise)
                {
                    shafts.randomJitterNoise = NebulaCloudsSM_HDRP.RandomJitter.BlueNoise;
                }
                if (connector.randomJitterNoise == RandomJitterChoice.Off)
                {
                    shafts.randomJitterNoise = NebulaCloudsSM_HDRP.RandomJitter.Off;
                }
                if (connector.randomJitterNoise == RandomJitterChoice.Random)
                {
                    shafts.randomJitterNoise = NebulaCloudsSM_HDRP.RandomJitter.Random;
                }
                //randomJitterNoise = connector.randomJitterNoise ;
                //shafts.randomJitterNoise = connector.randomJitterNoise;

                shafts.temporalAntiAliasing = connector.temporalAntiAliasing;
                shafts.spread = connector.spread;
                shafts.feedback = connector.feedback;

                shafts.gradientLow = connector.gradientLow;
                shafts.gradientMed = connector.gradientMed;
                shafts.gradientHigh = connector.gradientHigh;
                shafts.curlNoise = connector.curlNoise;
                shafts.lowFreqNoise = connector.lowFreqNoise;
                shafts.highFreqNoise = connector.highFreqNoise;
                shafts.startHeight = connector.startHeight;
                shafts.thickness = connector.thickness;
                shafts.planetSize = connector.planetSize;
                shafts.planetZeroCoordinate = connector.planetZeroCoordinate;

                shafts.scale = connector.scale;

                shafts.detailScale = connector.detailScale;

                shafts.lowFreqMin = connector.lowFreqMin;

                shafts.lowFreqMax = connector.lowFreqMax;

                shafts.highFreqModifier = connector.highFreqModifier;

                shafts.curlDistortScale = connector.curlDistortScale;

                shafts.curlDistortScale = connector.curlDistortScale;

                shafts.weatherScale = connector.weatherScale;
                shafts.maskScale = connector.maskScale;//v0.1.1
                shafts.topDownMaskCamera = connector.topDownMaskCamera;
                shafts.fogOfWarRadius = connector.fogOfWarRadius;

                shafts.coverage = connector.coverage;

                shafts.cloudSampleMultiplier = connector.cloudSampleMultiplier;

                shafts.cloudsHighTexture = connector.cloudsHighTexture;

                shafts.coverageHigh = connector.coverageHigh;

                shafts.highCoverageScale = connector.highCoverageScale;

                shafts.highCloudsScale = connector.highCloudsScale;

                shafts.sunLight = connector.sunLight;
                shafts.cloudBaseColor = connector.cloudBaseColor;
                shafts.cloudTopColor = connector.cloudTopColor;

                shafts.ambientLightFactor = connector.ambientLightFactor;

                shafts.sunLightFactor = connector.sunLightFactor;
                shafts.highSunColor = connector.highSunColor;
                shafts.lowSunColor = connector.lowSunColor;

                shafts.henyeyGreensteinGForward = connector.henyeyGreensteinGForward;

                shafts.henyeyGreensteinGBackward = connector.henyeyGreensteinGBackward;

                shafts.lightStepLength = connector.lightStepLength;

                shafts.lightConeRadius = connector.lightConeRadius;
                shafts.randomUnitSphere = connector.randomUnitSphere;

                shafts.density = connector.density;
                shafts.aLotMoreLightSamples = connector.aLotMoreLightSamples;

                shafts.globalMultiplier = connector.globalMultiplier;
                shafts.windSpeed = connector.windSpeed;
                shafts.windDirection = connector.windDirection;
                shafts.coverageWindSpeed = connector.coverageWindSpeed;
                shafts.coverageWindDirection = connector.coverageWindDirection;
                shafts.highCloudsWindSpeed = connector.highCloudsWindSpeed;
                shafts.highCloudsWindDirection = connector.highCloudsWindDirection;

                shafts._windOffset = connector._windOffset;
                shafts._coverageWindOffset = connector._coverageWindOffset;
                shafts._highCloudsWindOffset = connector._highCloudsWindOffset;
                shafts._windDirectionVector = connector._windDirectionVector;
                shafts._multipliedWindSpeed = connector._multipliedWindSpeed;

                shafts.raysResolution = connector.raysResolution;
                shafts.rayShadowing = connector.rayShadowing;
                /////////////// END FULL VOLUMETRIC CLOUDS


                shafts.sunTransform.value = new Vector3(connector.Sun.x, connector.Sun.y, connector.Sun.z);// connector.sun.transform.position;
                                                                                                           //         shafts.screenBlendMode = connector.screenBlendMode;
                                                                                                           //public Vector3 sunTransform = new Vector3(0f, 0f, 0f); 
                shafts.radialBlurIterations.value = connector.radialBlurIterations;
                shafts.sunColor.value = connector.sunColor;
                shafts.sunThreshold.value = connector.sunThreshold;
                shafts.sunShaftBlurRadius.value = connector.sunShaftBlurRadius;
                shafts.sunShaftIntensity.value = connector.sunShaftIntensity;
                shafts.maxRadius.value = connector.maxRadius;
                shafts.useDepthTexture.value = connector.useDepthTexture;

                ////// VOLUME FOG URP /////////////////
                //FOG URP /////////////
                //FOG URP /////////////
                //FOG URP /////////////
                //this.blend =  0.5f;
                shafts._FogColor = connector._FogColor;
                //fog params
                shafts.noiseTexture = connector.noiseTexture;
                shafts._startDistance = connector._startDistance;

                shafts._fogHeight = connector._fogHeight;
                shafts._fogDensity = connector._fogDensity;
                shafts._cameraRoll = connector._cameraRoll;
                shafts._cameraDiff = connector._cameraDiff;
                shafts._cameraTiltSign = connector._cameraTiltSign;
                shafts.heightDensity = connector.heightDensity;
                shafts.noiseDensity = connector.noiseDensity;
                shafts.noiseScale = connector.noiseScale;
                shafts.noiseThickness = connector.noiseThickness;
                shafts.noiseSpeed = connector.noiseSpeed;
                shafts.occlusionDrop = connector.occlusionDrop;
                shafts.occlusionExp = connector.occlusionExp;
                shafts.noise3D = connector.noise3D;
                shafts.startDistance = connector.startDistance;
                shafts.luminance = connector.luminance;
                shafts.lumFac = connector.lumFac;
                shafts.ScatterFac = connector.ScatterFac;
                shafts.TurbFac = connector.TurbFac;
                shafts.HorizFac = connector.HorizFac;
                shafts.turbidity = connector.turbidity;
                shafts.reileigh = connector.reileigh;
                shafts.mieCoefficient = connector.mieCoefficient;
                shafts.mieDirectionalG = connector.mieDirectionalG;
                shafts.bias = connector.bias;
                shafts.contrast = connector.contrast;
                shafts.TintColor = connector.TintColor;
                shafts.TintColorK = connector.TintColorK;
                shafts.TintColorL = connector.TintColorL;
                shafts.Sun = connector.Sun;

                shafts.SunFOG = connector.SunFOG;

                shafts.FogSky = connector.FogSky;
                shafts.ClearSkyFac = connector.ClearSkyFac;
                shafts.PointL = connector.PointL;
                shafts.PointLParams = connector.PointLParams;
                shafts._useRadialDistance = connector._useRadialDistance;
                shafts._fadeToSkybox = connector._fadeToSkybox;
                shafts.allowHDR = connector.allowHDR;
                //END FOG URP //////////////////
                //END FOG URP //////////////////
                //END FOG URP //////////////////
                ////// END VOLUME FOG URP /////////////////

                shafts.farCamDistFactor = connector.farCamDistFactor;

                ////// VOLUME CLOUDS
                shafts.blendBackground = connector.blendBackground;
                shafts.backgroundCam = connector.backgroundCam;
                shafts.backgroundMat = connector.backgroundMat;

                shafts.DistGradient = connector.DistGradient;
                shafts.GradientBounds = connector.GradientBounds;
                //v4.8.6
                shafts.adjustNightLigting = connector.adjustNightLigting;
                shafts.backShadeNight = connector.backShadeNight; //use this at night for more dense clouds
                shafts.turbidityNight = connector.turbidityNight;
                shafts.extinctionNight = connector.extinctionNight;
                shafts.shift_dawn = connector.shift_dawn; //add shift to when cloud lighting changes vs the TOD of sky master
                shafts.shift_dusk = connector.shift_dusk;
                //v4.8.4
                //public bool adjustNightLigting = true;
                shafts.groundColorNight = connector.groundColorNight;
                shafts.scatterNight = connector.scatterNight; //use this at night
                shafts.reflectFogHeight = connector.reflectFogHeight;
                //v2.1.20
                shafts.WebGL = connector.WebGL;
                //v2.1.19
                shafts.fastest = connector.fastest; //Debug.Log(fastest);
                shafts.localLight = connector.localLight;
                shafts.localLightFalloff = connector.localLightFalloff;
                //public float localLightIntensity = 1;
                shafts.currentLocalLightIntensity = connector.currentLocalLightIntensity;
                shafts.localLightIntensity = connector.localLightIntensity;
                shafts.localLightIntensityA = connector.localLightIntensityA;
                shafts._SkyTint = connector._SkyTint;
                shafts._AtmosphereThickness = connector._AtmosphereThickness;
                ////////// CLOUDS
                //           isForReflections = connector.isForReflections;
                //v4.8
                shafts._invertX = connector._invertX;
                shafts._invertRay = connector._invertRay;
                shafts._WorldSpaceCameraPosC = connector._WorldSpaceCameraPosC;
                shafts.varianceAltitude1 = connector.varianceAltitude1;
                //v4.1f
                shafts._mobileFactor = connector._mobileFactor;
                shafts._alphaFactor = connector._alphaFactor;
                //v3.5.3
                shafts._InteractTexture = connector._InteractTexture;
                shafts._InteractTexturePos = connector._InteractTexturePos;
                shafts._InteractTextureAtr = connector._InteractTextureAtr;
                shafts._InteractTextureOffset = connector._InteractTextureOffset; //v4.0
                shafts._NearZCutoff = connector._NearZCutoff;
                shafts._HorizonYAdjust = connector._HorizonYAdjust;
                shafts._HorizonZAdjust = connector._HorizonZAdjust;
                shafts._FadeThreshold = connector._FadeThreshold;
                //v3.5 clouds	
                shafts._BackShade = connector._BackShade;
                shafts._UndersideCurveFactor = connector._UndersideCurveFactor;
                shafts._WorldClip = connector._WorldClip;
                shafts._SampleCount0 = connector._SampleCount0;
                shafts._SampleCount1 = connector._SampleCount1;
                shafts._SampleCountL = connector._SampleCountL;
                shafts._NoiseTex1 = connector._NoiseTex1;
                shafts._NoiseTex2 = connector._NoiseTex2;
                shafts._NoiseFreq1 = connector._NoiseFreq1;
                shafts._NoiseFreq2 = connector._NoiseFreq2;
                shafts._NoiseAmp1 = connector._NoiseAmp1;
                shafts._NoiseAmp2 = connector._NoiseAmp2;
                shafts._NoiseBias = connector._NoiseBias;
                shafts._Scroll1 = connector._Scroll1;
                shafts._Scroll2 = connector._Scroll2;
                shafts._Altitude0 = connector._Altitude0;
                shafts._Altitude1 = connector._Altitude1;
                shafts._FarDist = connector._FarDist;
                shafts._Scatter = connector._Scatter;
                shafts._HGCoeff = connector._HGCoeff;
                shafts._Extinct = connector._Extinct;
                shafts._SunSize = connector._SunSize;
                shafts._GroundColor = connector._GroundColor; //v4.0
                shafts._ExposureUnder = connector._ExposureUnder; //v4.0
                                                                  //frameFraction = connector.frameFraction;
                                                                  //v2.1.19
                                                                  //_fastest= connector._fastest;
                shafts._LocalLightPos = connector._LocalLightPos;
                shafts._LocalLightColor = connector._LocalLightColor;
                ///////// END CLOUDS
                shafts.splitPerFrames = connector.splitPerFrames; //v2.1.19
                shafts.cameraMotionCompensate = connector.cameraMotionCompensate;//v2.1.19    
                shafts.updateRate = connector.updateRate;
                // public int resolution = 256;
                shafts.downScaleFactor = connector.downScaleFactor;
                shafts.downScale = connector.downScale;
                shafts._needsReset = connector._needsReset;
                if (!connector.autoReproject)
                {
                    shafts.enableReproject = connector.enableReproject;
                }
                shafts.autoReproject = connector.autoReproject;

                //v0.1
                shafts.lowerRefrReflResFactor = connector.lowerRefrReflResFactor;
                ////// END VOLUME CLOUDS

                shafts.renderClouds = connector.renderClouds;
                Debug.Log("shafts.renderClouds = " + shafts.renderClouds);
            }
        }

        void Start()
        {

            //v0.6
            controlBackAlphaPowerP = controlBackAlphaPower;
            controlCloudAlphaPowerP = controlCloudAlphaPower;
            depthDilationP = depthDilation;
            controlCloudEdgeAP = controlCloudEdgeA;

            ////////// VOLUME CLOUDS v0.1
            _needsReset = true;
            //postProfile = GetComponent<PostProcessVolume>().profile;

            //// FULL VOLUMETRIC CLOUDS
            _windOffset = new Vector3(0.0f, 0.0f, 0.0f);
            _coverageWindOffset = new Vector3(0.5f / (weatherScale * 0.00025f), 0.5f / (weatherScale * 0.00025f));
            _highCloudsWindOffset = new Vector3(1500.0f, -900.0f);
            //// END FULL VOLUMETRIC CLOUDS
            ////////// END VOLUME CLOUDS v0.1




            ///////////// SHAFTS ///////////
            if (volumeObj != null)
            {
                Volume volume = volumeObj.GetComponent<Volume>();
                //WaterAirMaskSM tempMask;

                //if (volume.profile.TryGet<WaterAirMaskSM>(out tempMask))
                //{
                //    waterAirMask = tempMask;
                //    //Debug.Log("Got script");
                //}
                //if (waterAirMask != null && waterAirMask.m_TemporaryColorTexture != null)
                //{
                //    Debug.Log(waterAirMask.m_TemporaryColorTexture.name);
                //}
                //depthOfField.focusDistance.value = 42f;

                Fog tempFog;
                if (volume.profile.TryGet<Fog>(out tempFog))
                {
                    fog = tempFog;
                    //Debug.Log("Got FOG script");
                }
                NebulaCloudsSM_HDRP tempShafts;
                if (volume.profile.TryGet<NebulaCloudsSM_HDRP>(out tempShafts))
                {
                    shafts = tempShafts;
                    // Debug.Log("Got SHAFTS script");
                }
            }
            //v0.1
            //if (shafts != null)
            //{
            //    passParamsToClouds();
            //}

        }

        //public ProjectedGrid projectedGrid;
        public bool shaftsFollowCamera = false;
        public bool smoothFadeToAir = false;

        // Update is called once per frame
        void Update()
        {
            //v0.6
            //put controlBackAlphaPower = 1, controlCloudAlphaPower = 0.001 if inside or above clouds
            if (autoRegulateEdgeMode && Application.isPlaying)
            { //the new edges mode is only for view below clouds, regulate out of the mode if inside or above
                if (Camera.main.transform.position.y > autoRegulateCutoffDistance)//stop new edge blend mode when near or above clouds
                {
                    controlBackAlphaPower = 1;
                    controlCloudAlphaPower = 0.001f;
                    depthDilation = 0;
                    controlCloudEdgeA = Vector4.Lerp(controlCloudEdgeA, new Vector4(1, 1, 1, 1), Time.deltaTime * 0.4f);
                }
                else
                {
                    controlBackAlphaPower = controlBackAlphaPowerP;
                    controlCloudAlphaPower = controlCloudAlphaPowerP;
                    depthDilation = depthDilationP;
                    controlCloudEdgeA = Vector4.Lerp(controlCloudEdgeA, controlCloudEdgeAP, Time.deltaTime * 0.4f);
                }
            }

            //v0.1.1
            if (maskHistoryTexture != null && maskMat != null)
            {
                //generate mask
                if (maskTexture == null)
                {
                    maskTexture = new RenderTexture(maskHistoryTexture);
                }
                // maskHistoryTexture
                RenderTexture temp = RenderTexture.GetTemporary(maskHistoryTexture.width, maskHistoryTexture.height);
                Graphics.Blit(maskTexture, temp);
                maskMat.SetTexture("prevTex", temp);
                Graphics.Blit(maskHistoryTexture, maskTexture, maskMat, 0);
                temp.Release();
            }


            if (enableShafts)
            {
                shafts.shaftsFollowCamera.value = shaftsFollowCamera;

                if (SunLight != null)
                {
                    shafts.sunTransform.value = SunLight.position;
                }

                //if (waterAirMask != null && waterAirMask.m_TemporaryColorTexture != null)
                //{
                //    Debug.Log(waterAirMask.m_TemporaryColorTexture.rt.width);
                //}

                float diff = Camera.main.transform.position.y - waterHeight;

                if (smoothFadeToAir)
                {
                    float diffA = Camera.main.transform.position.y - shafts.waterHeight.value + 3;

                    //BumpScaleRL
                    //0.3 to 0.003 as go up
                    //0.003 to 0.3 as go down

                    //Sun shafts Intensity
                    //from 0 to 1 as go down
                    //and 1 to 0 as go up

                    shafts.BumpScaleRL.value = (0.3f + Mathf.Clamp(-diffA, -6, 6) * 0.297f / 6.0f) - 0.16f;
                    shafts.sunShaftIntensity.value = Mathf.Clamp(1f + Mathf.Clamp(-diffA, -6, 6) * 1.0f / 6.0f, 0, 1);
                }



                //if (Camera.main != null && projectedGrid != null)
                //{
                //    if (diff < -4)
                //    {
                //        if (projectedGrid.heightOffset > 0)
                //        {
                //            projectedGrid.heightOffset = -projectedGrid.heightOffset;
                //        }
                //        projectedGrid.heightOffset = 10;// -17;
                //    }
                //    else
                //    {
                //        if (projectedGrid.heightOffset < 0)
                //        {
                //            projectedGrid.heightOffset = -projectedGrid.heightOffset;
                //        }
                //        projectedGrid.heightOffset = 27;
                //    }
                //}

                if (waterBlackLineWidth > 0)
                {
                    if (Camera.main != null && diff < -10)
                    {
                        //from - 10 in (-1.5)
                        //to - 20   1.5

                        if (shafts != null && shafts.active)
                        {
                            if (diff > -20)
                            {
                                shafts.refractLineWidth.value = -1.5f - (diff + 10) / 3.33f;
                            }
                            else
                            {
                                shafts.refractLineWidth.value = 1.5f;
                            }
                        }
                    }
                    else
                    {
                        shafts.refractLineWidth.value = refractLineWidth;// - 1.5f;
                    }
                }

                //if (Camera.main != null && diff > cutoffOffset && projectedGrid != null)
                //{
                //    //disable effects
                //    if (shafts != null && shafts.active)
                //    {
                //        shafts.active = false;
                //    }
                //    if (fog != null && fog.active)
                //    {
                //        fog.active = false;
                //    }
                //}
                //else if (projectedGrid != null)
                //{
                //    //enable effects
                //    if (shafts != null && !shafts.active)
                //    {
                //        shafts.active = true;
                //    }
                //    if (fog != null && !fog.active)
                //    {
                //        fog.active = true;
                //    }
                //}
            }//END SHAFTS

            if (enableClouds)
            {
                if (sun != null)
                {
                    UpdateFOG();
                    //URP v0.1
                    if (Time.fixedTime < 1)
                    {
                        _needsReset = true;
                    }
                }

                ////////// FULL VOLUMETRIC CLOUDS
                // updates wind offsets
                _multipliedWindSpeed = windSpeed * globalMultiplier;
                float angleWind = windDirection * Mathf.Deg2Rad;
                _windDirectionVector = new Vector3(Mathf.Cos(angleWind), -0.25f, Mathf.Sin(angleWind));
                _windOffset += _multipliedWindSpeed * _windDirectionVector * Time.deltaTime;

                float angleCoverage = coverageWindDirection * Mathf.Deg2Rad;
                Vector2 coverageDirecton = new Vector2(Mathf.Cos(angleCoverage), Mathf.Sin(angleCoverage));
                _coverageWindOffset += coverageWindSpeed * globalMultiplier * coverageDirecton * Time.deltaTime;

                float angleHighClodus = highCloudsWindDirection * Mathf.Deg2Rad;
                Vector2 highCloudsDirection = new Vector2(Mathf.Cos(angleHighClodus), Mathf.Sin(angleHighClodus));
                _highCloudsWindOffset += highCloudsWindSpeed * globalMultiplier * highCloudsDirection * Time.deltaTime;
                ////////// END FULL VOLUMETRIC CLOUDS

                //v0.3
                // Material shadowMat;
                if (shadowMat != null)
                {
                    shadowMat.SetFloat("_WindSpeed", _multipliedWindSpeed);
                    shadowMat.SetVector("_WindDirection", _windDirectionVector);
                    shadowMat.SetVector("_WindOffset", _windOffset);
                    shadowMat.SetVector("_CoverageWindOffset", _coverageWindOffset);

                }

            }//END VOLUMETRIC CLOUDS v0.1

        }//END UPDATE

        void UpdateFOG()
        {
            var volFog = this;// shafts;//this; //The custom forward renderer will read variables from this script

            //var volFog = postProfile.GetSetting<VolumeFogSM_SRP>();
            if (volFog != null)
            {
                if (localLightA != null)
                {

                    //volFog.sunTransform.value = sun.transform.position;
                }
                Camera cam = Camera.current;
                if (cam == null)
                {
                    cam = Camera.main;
                }
                volFog._cameraRoll = cam.transform.eulerAngles.z;

                volFog._cameraDiff = cam.transform.eulerAngles;// - prevRot;

                if (cam.transform.eulerAngles.y > 360)
                {
                    volFog._cameraDiff.y = cam.transform.eulerAngles.y % 360;
                }
                if (cam.transform.eulerAngles.y > 180)
                {
                    volFog._cameraDiff.y = -(360 - volFog._cameraDiff.y);
                }

                //slipt in 90 degs, 90 to 180 mapped to 90 to zero
                //volFog._cameraDiff.value.w = 1;
                if (volFog._cameraDiff.y > 90 && volFog._cameraDiff.y < 180)
                {
                    volFog._cameraDiff.y = 180 - volFog._cameraDiff.y;
                    volFog._cameraDiff.w = -1;
                    //volFog._cameraDiff.value.w = Mathf.Lerp(volFog._cameraDiff.value.w ,- 1, Time.deltaTime * 20);
                }
                else if (volFog._cameraDiff.y < -90 && volFog._cameraDiff.y > -180)
                {
                    volFog._cameraDiff.y = -180 - volFog._cameraDiff.y;
                    volFog._cameraDiff.w = -1;
                    //volFog._cameraDiff.value.w = Mathf.Lerp(volFog._cameraDiff.value.w, -1, Time.deltaTime * 20);
                    //Debug.Log("dde");
                }
                else
                {
                    //volFog._cameraDiff.value.w = Mathf.Lerp(volFog._cameraDiff.value.w, 1, Time.deltaTime * 20);
                    volFog._cameraDiff.w = 1;
                }

                //vertical fix
                if (cam.transform.eulerAngles.x > 360)
                {
                    volFog._cameraDiff.x = cam.transform.eulerAngles.x % 360;
                }
                if (cam.transform.eulerAngles.x > 180)
                {
                    volFog._cameraDiff.x = 360 - volFog._cameraDiff.x;
                }
                //Debug.Log(cam.transform.eulerAngles.x);
                if (cam.transform.eulerAngles.x > 0 && cam.transform.eulerAngles.x < 180)
                {
                    volFog._cameraTiltSign = 1;
                }
                else
                {
                    // Debug.Log(cam.transform.eulerAngles.x);
                    volFog._cameraTiltSign = -1;
                }
                if (sun != null)
                {
                    Vector3 sunDir = -sun.transform.forward;// -sun.transform.forward;
                                                            //sunDir = Quaternion.AngleAxis(-cam.transform.eulerAngles.y, Vector3.up) * -sunDir;
                                                            //sunDir = Quaternion.AngleAxis(cam.transform.eulerAngles.x, Vector3.left) * sunDir;
                                                            //sunDir = Quaternion.AngleAxis(-cam.transform.eulerAngles.z, Vector3.forward) * sunDir;
                                                            // volFog.Sun.value = -new Vector4(sunDir.x, sunDir.y, sunDir.z, 1);
                    volFog.Sun = new Vector4(sunDir.x, sunDir.y, sunDir.z, 1);
                    //volFog.sun.position = new Vector3(sunDir.x, sunDir.y, sunDir.z);////// vector 4 to vector3

                    //FOG SUN
                    Vector3 sunDiFOGr = sun.transform.forward;// -sun.transform.forward;
                    sunDiFOGr = Quaternion.AngleAxis(-cam.transform.eulerAngles.y, Vector3.up) * -sunDiFOGr;
                    sunDiFOGr = Quaternion.AngleAxis(cam.transform.eulerAngles.x, Vector3.left) * sunDiFOGr;
                    sunDiFOGr = Quaternion.AngleAxis(-cam.transform.eulerAngles.z, Vector3.forward) * sunDiFOGr;
                    // volFog.Sun.value = -new Vector4(sunDir.x, sunDir.y, sunDir.z, 1);
                    volFog.SunFOG = new Vector4(sunDiFOGr.x, sunDiFOGr.y, sunDiFOGr.z, 1);
                }
                else
                {
                    volFog.Sun = new Vector4(15, 0, 1, 1);
                }
                if (localLightA != null)
                {
                    volFog.PointL = new Vector4(localLightA.transform.position.x, localLightA.transform.position.y, localLightA.transform.position.z, localLightIntensity);
                    volFog.PointLParams = new Vector4(localLightA.color.r, localLightA.color.g, localLightA.color.b, localLightRadius);
                }
                //Debug.Log(volFog._cameraDiff.value);
                //prevRot = cam.transform.eulerAngles;
            }
        }
        //END FOG
    }
}