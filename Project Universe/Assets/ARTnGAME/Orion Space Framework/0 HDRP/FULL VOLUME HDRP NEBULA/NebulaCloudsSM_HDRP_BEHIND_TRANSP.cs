using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;

namespace Artngame.SKYMASTER
{
    [Serializable, VolumeComponentMenu("Post-processing/Custom/NebulaCloudsSM_HDRP_BEHIND_TRANSP")]
    public sealed class NebulaCloudsSM_HDRP_BEHIND_TRANSP : CustomPostProcessVolumeComponent, IPostProcessComponent
    {

        //v0.6
        public Vector3 cloudDistanceParams = new Vector3(0, 0, 1);
        public float controlBackAlphaPower = 1;
        public float controlCloudAlphaPower = 0.001f;
        public Vector4 controlCloudEdgeA = new Vector4(1, 1, 1, 1);
        public float controlCloudEdgeOffset = 1;
        public float depthDilation = 1;
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

        //v0.6 - fix issue with AA in clouds rthandles

        public bool renderClouds = true;

        /////////////////// FULL VOLUMETRIC CLOUDS v0.1 ///////////////////

        public Vector4 raysResolution = new Vector4(1, 1, 1, 1);
        public Vector4 rayShadowing = new Vector4(1, 1, 1, 1);

        public Texture2D WeatherTexture;
        public RenderTexture maskTexture;//v0.1.1
        public float cameraScale = 1; //use 80 for correct cloud scaling in relation to land //v0.4
        public float extendFarPlaneAboveClouds = 1;

        public int cloudChoice = 0;
        /////// FULL VOLUMETRIC CLOUDS
        [Tooltip("Fog top Y coordinate")]
        public float height = 1.0f;
        [Tooltip("Distance fog is based on radial distance from camera when checked")]
        public bool useRadialDistance = false;

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

        //v0.1
        public int renderInFront = 0;

        public enum RandomJitter
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
        [Range(1, 256)]
        public int steps = 128;
        public bool adjustDensity = true;
        public AnimationCurve stepDensityAdjustmentCurve = new AnimationCurve(new Keyframe(0.0f, 3.019f), new Keyframe(0.25f, 1.233f), new Keyframe(0.5f, 1.0f), new Keyframe(1.0f, 0.892f));
        public bool allowFlyingInClouds = false;
        [Range(1, 8)]
        public int downSample = 1;
        public Texture2D blueNoiseTexture;
        public RandomJitter randomJitterNoise = RandomJitter.BlueNoise;
        public bool temporalAntiAliasing = true;

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
        [Range(0.0f, 1.0f)]
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

        public float maskScale = 0f;//v0.1.1
        public Camera topDownMaskCamera;
        public float fogOfWarRadius = 0; //distance to make opque around player
        public Transform playerPos;
        public float fogOfWarPower = 0;

        [Range(0.0f, 2.0f)]
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

        public Vector3 _windOffset;
        public Vector2 _coverageWindOffset;
        public Vector2 _highCloudsWindOffset;
        public Vector3 _windDirectionVector;
        public float _multipliedWindSpeed;

        private Texture3D _cloudShapeTexture;
        private Texture3D _cloudDetailTexture;
        //private CloudTemporalAntiAliasing _temporalAntiAliasing;
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







        public float farCamDistFactor = 1;
        ////// VOLUME CLOUDS 
        public Texture2D colourPalette;
        public bool Made_texture = false;
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
        public float localLightIntensity = 1;
        public float localLightIntensityA = 1;
        public float currentLocalLightIntensity = 0;
        public Vector3 _SkyTint = new Vector3(0.5f, 0.5f, 0.5f);
        public float _AtmosphereThickness = 1.0f;
        /// <summary>
        /// /////////////////////////////////////////////////
        /// </summary>
        ////////// CLOUDS
        //public bool useRadialDistance = false;
        public bool isForReflections = false;
        //v4.8
        public float _invertX = 0;
        public float _invertRay = 1;
        public Vector3 _WorldSpaceCameraPosC;
        public float varianceAltitude1 = 0;
        //v4.1f
        public float _mobileFactor = 1;
        public float _alphaFactor = 0.96f;
        //v3.5.3
        public Texture2D _InteractTexture;
        public Vector4 _InteractTexturePos;
        public Vector4 _InteractTextureAtr;
        public Vector4 _InteractTextureOffset; //v4.0
        public float _NearZCutoff = -2;
        public float _HorizonYAdjust = 0;
        public float _HorizonZAdjust = 0;
        public float _FadeThreshold = 0;
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
        public float _SunSize = 0.04f;
        public Vector3 _GroundColor = new Vector3(0.8f, 0.8f, 0.8f); //v4.0
        public float _ExposureUnder = 3; //v4.0
        public float frameFraction = 0;
        //v2.1.19
        // public bool _fastest = false;
        public Vector4 _LocalLightPos;
        public Vector4 _LocalLightColor;
        ///////// END CLOUDS
        ////// END VOLUME CLOUDS

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
        public bool allowHDR = false;
        //END FOG URP //////////////////
        //END FOG URP //////////////////
        //END FOG URP //////////////////


        //SUN SHAFTS  
        public Vector4 raysResolutionA = new Vector4(1, 1, 1, 1);
        public Color _rayColor = new Color(.969f, 0.549f, .041f, 1);
        public Vector4 rayShadowingA = new Vector4(1, 1, 1, 1);
        public Color _underDomeColor = new Color(1, 1, 1, 1);
        public bool enableShafts = false;

        //public Vector3 sunTransform = new Vector3(0f, 0f, 0f); // Transform sunTransform;
        //public int radialBlurIterations = 2;
        //public Color sunColor = Color.white;
        //public Color sunThreshold = new Color(0.87f, 0.74f, 0.65f);
        //public float sunShaftBlurRadius = 2.5f;
        //public float sunShaftIntensity = 1.15f;
        //public float maxRadius = 0.75f;
        //public bool useDepthTexture = true;
        public float blend = 0.5f;

        public enum RenderTarget
        {
            Color,
            RenderTexture,
        }
        public bool inheritFromController = true;

        public bool enableFog = true;//v0.1 off by default

        //public Material blitMaterial = null;
        //public Material blitMaterialFOG = null;
        public int blitShaderPassIndex = 0;
        //public FilterMode filterMode { get; set; }   

        string m_ProfilerTag;

        //SUN SHAFTS
        //RenderTexture lrColorB;

        //private RenderTargetIdentifier source { get; set; }
        ////public BlitFullVolumeCloudsSRP.BlitSettings.SunShaftsResolution resolution = BlitFullVolumeCloudsSRP.BlitSettings.SunShaftsResolution.Normal;
        //// public BlitFullVolumeCloudsSRP.BlitSettings.ShaftsScreenBlendMode screenBlendMode = BlitFullVolumeCloudsSRP.BlitSettings.ShaftsScreenBlendMode.Screen;
        //private UnityEngine.Rendering.Universal.RenderTargetHandle destination { get; set; }
        //UnityEngine.Rendering.Universal.RenderTargetHandle m_TemporaryColorTexture;
        //UnityEngine.Rendering.Universal.RenderTargetHandle lrDepthBuffer;

        Camera currentCamera;
        float prevDownscaleFactor;//v0.1
        float prevlowerRefrReflResFactor;

        /////////////////// END FULL VOLUMETRIC CLOUDS v0.1 ///////////////////












        /// <summary>
        /// //////////////////////////// SHAFTS ////////////////////////////////
        /// </summary>

        [Tooltip("Controls the intensity of the effect.")]
        public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);

        public BoolParameter shaftsFollowCamera = new BoolParameter(false);

        //SHAFTS
        public enum SunShaftsResolution
        {
            Low = 0,
            Normal = 1,
            High = 2,
        }
        public enum ShaftsScreenBlendMode
        {
            Screen = 0,
            Add = 1,
        }
        public SunShaftsResolution resolution = SunShaftsResolution.Normal;
        public ShaftsScreenBlendMode screenBlendMode = ShaftsScreenBlendMode.Screen;
        public Vector3Parameter sunTransform = new Vector3Parameter(new Vector3(0f, 0f, 0f));  //public Vector3 sunTransform = new Vector3(0f, 0f, 0f); 
        public IntParameter radialBlurIterations = new IntParameter(2);  //public int radialBlurIterations = 2;
        public ColorParameter sunColor = new ColorParameter(Color.white);
        public ColorParameter sunThreshold = new ColorParameter(new Color(0.87f, 0.74f, 0.65f));
        public FloatParameter sunShaftBlurRadius = new FloatParameter(2.5f);
        public FloatParameter sunShaftIntensity = new FloatParameter(1.15f);
        public FloatParameter maxRadius = new FloatParameter(0.75f);
        public BoolParameter useDepthTexture = new BoolParameter(true);

        //MASK
        public FloatParameter waterHeight = new FloatParameter(0);


        /////REFRACT LINE
        public TextureParameter BumpMap = new TextureParameter(null);
        public FloatParameter underwaterDepthFade = new FloatParameter(1.1f); //put above 1 to fade faster towards the surface - TO DO: Regulate based on player distance to surface as well
        public FloatParameter BumpIntensity = new FloatParameter(0.02f);
        public Vector4Parameter BumpVelocity = new Vector4Parameter(new Vector4(0.15f, 0.10f, 0.2f, 0.3f));
        public FloatParameter BumpScale = new FloatParameter(0.1f);
        //v4.6
        public ColorParameter underWaterTint = new ColorParameter(Color.cyan);
        public FloatParameter BumpIntensityRL = new FloatParameter(0.17f);
        public FloatParameter BumpScaleRL = new FloatParameter(0.4f);
        public FloatParameter BumpLineHeight = new FloatParameter(0.1f);
        public FloatParameter refractLineWidth = new FloatParameter(1); //v4.6
        public FloatParameter refractLineXDisp = new FloatParameter(1); //v4.6
        public FloatParameter refractLineXDispA = new FloatParameter(1); //v4.6
        public Vector4Parameter refractLineFade = new Vector4Parameter(new Vector4(1, 1, 1, 1));
        /////END REFRACT LINE

        Material sheetSHAFTS;
        public bool IsActive() => sheetSHAFTS != null && intensity.value > 0f;
        //public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.BeforePostProcess;// AfterPostProcess;
        public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterOpaqueAndSky;

        public override void Setup()
        {
            connectSuntoNebulaCloudsHDRP connector = Camera.main.GetComponent<connectSuntoNebulaCloudsHDRP>();
            if (connector != null)
            {
                passParamsToClouds(connector);
            }
            else
            {
                return;
            }

            //if (Shader.Find("Hidden/Shader/SunShaftsSM_HDRP") != null)
            //{
            //    sheetSHAFTS = new Material(Shader.Find("Hidden/Shader/SunShaftsSM_HDRP"));
            //}

            if (Shader.Find("Hidden/Shader/NebulaCloudsSM_HDRP_Vortex") != null)
            {
                sheetSHAFTS = new Material(Shader.Find("Hidden/Shader/NebulaCloudsSM_HDRP_Vortex"));
            }

            var hdrpAsset = (GraphicsSettings.renderPipelineAsset as HDRenderPipelineAsset);
            var colorBufferFormat = hdrpAsset.currentPlatformRenderPipelineSettings.colorBufferFormat;

            if (lrDepthBuffer == null)
            {
                lrDepthBuffer = RTHandles.Alloc(
                    Vector2.one, TextureXR.slices, dimension: TextureXR.dimension,
                    colorFormat: (UnityEngine.Experimental.Rendering.GraphicsFormat)colorBufferFormat, // We don't need alpha in the blur
                    useDynamicScale: true, name: "lrDepthBuffer"
                );
            }
            if (m_TemporaryColorTexture == null)
            {
                m_TemporaryColorTexture = RTHandles.Alloc(
                    Vector2.one, TextureXR.slices, dimension: TextureXR.dimension,
                    colorFormat: (UnityEngine.Experimental.Rendering.GraphicsFormat)colorBufferFormat, // We don't need alpha in the blur
                    useDynamicScale: true, name: "m_TemporaryColorTexture"
                );
            }

            //v0.3
            if (m_TemporaryColorTextureTMP == null)
            {
                m_TemporaryColorTextureTMP = RTHandles.Alloc(
                    Vector2.one, TextureXR.slices, dimension: TextureXR.dimension,
                    colorFormat: (UnityEngine.Experimental.Rendering.GraphicsFormat)colorBufferFormat, // We don't need alpha in the blur
                    useDynamicScale: true, name: "m_TemporaryColorTextureTMP"
                );
            }

            if (m_TemporaryColorTextureScaled == null)
            {
                m_TemporaryColorTextureScaled = RTHandles.Alloc(
                    Vector2.one * scalerMask.value, TextureXR.slices, dimension: TextureXR.dimension, filterMode: FilterMode.Trilinear,
                    colorFormat: (UnityEngine.Experimental.Rendering.GraphicsFormat)colorBufferFormat, // We don't need alpha in the blur
                    useDynamicScale: true, name: "m_TemporaryColorTextureScaled"
                );
            }

            if (lrColorA == null)
            {
                lrColorA = RTHandles.Alloc(
                    Vector2.one, TextureXR.slices, dimension: TextureXR.dimension,
                    colorFormat: (UnityEngine.Experimental.Rendering.GraphicsFormat)colorBufferFormat, // We don't need alpha in the blur
                    useDynamicScale: true, name: "lrColorA"
                );
            }

            //Debug.Log((UnityEngine.Experimental.Rendering.GraphicsFormat)colorBufferFormat);

            float lowerRefrRefl = 1; //v0.1                                
            if (isForReflections)
            {
                lowerRefrRefl = lowerRefrReflResFactor; //(downScaleFactor * lowerRefrRefl) 
            }

            if (new1 == null)
            {
                new1 = RTHandles.Alloc(
                    // 1280 / (int)downScaleFactor, 720 / (int)downScaleFactor, TextureXR.slices, dimension: TextureXR.dimension,
                    Vector2.one / (float)downScaleFactor, TextureXR.slices, dimension: TextureXR.dimension,
                    colorFormat: UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_SFloat, // We don't need alpha in the blur
                    useDynamicScale: true, name: "new1"
                );
            }

            if (_cloudBufferP1 == null)
            {
                _cloudBufferP1 = RTHandles.Alloc(
                   Vector2.one / (float)downScaleFactor, TextureXR.slices, dimension: TextureXR.dimension,
                    colorFormat: UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_SFloat, // We don't need alpha in the blur
                    useDynamicScale: true, name: "_cloudBufferP1"
                );
            }
            if (_cloudBufferP == null)
            {
                _cloudBufferP = RTHandles.Alloc(
                    Vector2.one / (float)downScaleFactor, TextureXR.slices, dimension: TextureXR.dimension,
                    colorFormat: UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_SFloat, // We don't need alpha in the blur
                    useDynamicScale: true, name: "_cloudBufferP"
                );
            }
            if (_cloudBuffer == null)
            {
                _cloudBuffer = RTHandles.Alloc(
                   Vector2.one / (float)downScaleFactor, TextureXR.slices, dimension: TextureXR.dimension,
                    colorFormat: UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_SFloat, // We don't need alpha in the blur
                    useDynamicScale: true, name: "_cloudBuffer"
                );
            }

            if (tmpBuffer == null)
            {
                tmpBuffer = RTHandles.Alloc(
                    Vector2.one / (float)downScaleFactor, TextureXR.slices, dimension: TextureXR.dimension,
                    colorFormat: UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_SFloat, // We don't need alpha in the blur
                    useDynamicScale: true, name: "tmpBuffer"
                );
            }
            if (tmpBuffer1 == null)
            {
                tmpBuffer1 = RTHandles.Alloc(
                    Vector2.one / (float)downScaleFactor, TextureXR.slices, dimension: TextureXR.dimension,
                    colorFormat: UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_SFloat, // We don't need alpha in the blur
                    useDynamicScale: true, name: "tmpBuffer1"
                );
            }

            // if (tmpBuffer2 == null)
            //{
            //     tmpBuffer2 = RTHandles.Alloc( //v0.6
            //                (int)(Screen.width / (float)downScaleFactor), (int)(Screen.height / (float)downScaleFactor), TextureXR.slices, dimension: TextureXR.dimension, //Vector2.one / (float)downScaleFactor, TextureXR.slices, dimension: TextureXR.dimension,
            //                colorFormat: UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_SFloat, // We don't need alpha in the blur
            //                useDynamicScale: true, name: "tmpBuffer2", filterMode: FilterMode.Trilinear, autoGenerateMips: true, msaaSamples: MSAASamples.MSAA8x// v0.6 enableMSAA: true
            //            );
            //}

            ////Debug.Log(tmpBuffer2.rt.width + " , " + tmpBuffer2.rt.height + ", " + downScaleFactor);
            //if (tmpBufferAA == null)
            //{
            //    tmpBufferAA = RTHandles.Alloc(
            //               (int)(Screen.width / (float)downScaleFactor), (int)(Screen.height / (float)downScaleFactor), TextureXR.slices, dimension: TextureXR.dimension, // 1 *Vector2.one / (int)1, TextureXR.slices, dimension: TextureXR.dimension,
            //               colorFormat: UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_SFloat, // We don't need alpha in the blur
            //               useDynamicScale: true, name: "tmpBufferAA", filterMode: FilterMode.Trilinear, autoGenerateMips: true, msaaSamples: MSAASamples.MSAA8x// v0.6  enableMSAA: true
            //           );
            //}
            //if (tmpBufferAA2 == null)
            //{
            //    tmpBufferAA2 = RTHandles.Alloc(
            //               (int)(Screen.width / (float)downScaleFactor), (int)(Screen.height / (float)downScaleFactor), TextureXR.slices, dimension: TextureXR.dimension, //1 * Vector2.one / (int)1, TextureXR.slices, dimension: TextureXR.dimension,
            //               colorFormat: UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_SFloat, // We don't need alpha in the blur
            //               useDynamicScale: true, name: "tmpBufferAA2", filterMode: FilterMode.Trilinear, autoGenerateMips: true, msaaSamples: MSAASamples.MSAA8x// v0.6  enableMSAA: true
            //           );
            //}

            if (tmpBuffer2 == null)
            {
                tmpBuffer2 = RTHandles.Alloc( //v0.6
                           Vector2.one / (float)downScaleFactor, TextureXR.slices, dimension: TextureXR.dimension,
                           colorFormat: UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_SFloat, // We don't need alpha in the blur
                           useDynamicScale: true, name: "tmpBuffer2", filterMode: FilterMode.Trilinear, autoGenerateMips: true//, enableMSAA: true //HDRP 12 - 2021
                       );
            }

            //v0.6
            if (tmpBuffer3 == null)
            {
                tmpBuffer3 = RTHandles.Alloc(
                           Vector2.one / (float)downScaleFactor, TextureXR.slices, dimension: TextureXR.dimension,
                           colorFormat: UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_SFloat, // We don't need alpha in the blur
                           useDynamicScale: true, name: "tmpBuffer3", filterMode: FilterMode.Trilinear, autoGenerateMips: true//, enableMSAA: true //HDRP 12 - 2021
                       );
            }
            //previousDepthTexture
            if (previousDepthTexture == null)
            {
                previousDepthTexture = RTHandles.Alloc(
                           Vector2.one / (float)downScaleFactor, TextureXR.slices, dimension: TextureXR.dimension,
                           colorFormat: UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_SFloat, // We don't need alpha in the blur
                           useDynamicScale: true, name: "previousDepthTexture", filterMode: FilterMode.Trilinear, autoGenerateMips: true//, enableMSAA: true //HDRP 12 - 2021
                       );
            }
            if (previousFrameTexture == null)
            {
                previousFrameTexture = RTHandles.Alloc(
                               Vector2.one / (float)downScaleFactor, TextureXR.slices, dimension: TextureXR.dimension,
                               colorFormat: UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_SFloat, // We don't need alpha in the blur
                               useDynamicScale: true, name: "previousFrameTexture", filterMode: FilterMode.Trilinear, autoGenerateMips: true//, enableMSAA: true //HDRP 12 - 2021
                           );
            }

            //Debug.Log(tmpBuffer2.rt.width + " , " + tmpBuffer2.rt.height + ", " + downScaleFactor);
            if (tmpBufferAA == null)
            {
                tmpBufferAA = RTHandles.Alloc(
                            1 * Vector2.one / (int)1, TextureXR.slices, dimension: TextureXR.dimension,
                           colorFormat: UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_SFloat, // We don't need alpha in the blur
                           useDynamicScale: true, name: "tmpBufferAA", filterMode: FilterMode.Trilinear, autoGenerateMips: true//, enableMSAA: true
                       );
            }
            if (tmpBufferAA2 == null)
            {
                tmpBufferAA2 = RTHandles.Alloc(
                           1 * Vector2.one / (int)1, TextureXR.slices, dimension: TextureXR.dimension,
                           colorFormat: UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_SFloat, // We don't need alpha in the blur
                           useDynamicScale: true, name: "tmpBufferAA2", filterMode: FilterMode.Trilinear, autoGenerateMips: true//, enableMSAA: true
                       );
            }

        }


        //SHAFTS
        //UnityEngine.Rendering.HighDefinition.rendert lrDepthBuffer;
        //UnityEngine.Rendering.Universal.RenderTargetHandle lrDepthBuffer;
        //RenderTexture lrDepthBuffer;
        // RenderTargetIdentifier lrDepthBuffer;
        // RenderTexture lrColorA;
        //rendertarg lrDepthBuffer;

        RTHandle lrDepthBuffer;
        RTHandle m_TemporaryColorTexture;
        RTHandle m_TemporaryColorTextureTMP;
        RTHandle m_TemporaryColorTextureScaled;
        RTHandle lrColorA;

        public void passParamsToClouds(connectSuntoNebulaCloudsHDRP connector)
        {
            NebulaCloudsSM_HDRP_BEHIND_TRANSP shafts = this;

            if (1 == 1)
            {
                //FullVolumeCloudsSM_HDRP connector = shafts;

                //Debug.Log("Passing params");

                //connectSuntoNebulaCloudsHDRP connector = this;

                shafts.enableFog = connector.enableFog;

                shafts.cloudChoice = connector.cloudChoice;
                //////////////// FULL VOLUMETRIC CLOUDS

                shafts.WeatherTexture = connector.WeatherTexture;
                shafts.maskTexture = connector.maskTexture;//v0.1.1

                //v0.5
                YCutHeightDepthScale = connector.YCutHeightDepthScale;
                extendFarPlaneAboveClouds = connector.extendFarPlaneAboveClouds;//v0.4
                cameraScale = connector.cameraScale;//v0.4

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

                if (connector.randomJitterNoise == connectSuntoNebulaCloudsHDRP.RandomJitterChoice.BlueNoise)
                {
                    shafts.randomJitterNoise = RandomJitter.BlueNoise;
                }
                if (connector.randomJitterNoise == connectSuntoNebulaCloudsHDRP.RandomJitterChoice.Off)
                {
                    shafts.randomJitterNoise = RandomJitter.Off;
                }
                if (connector.randomJitterNoise == connectSuntoNebulaCloudsHDRP.RandomJitterChoice.Random)
                {
                    shafts.randomJitterNoise = RandomJitter.Random;
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
                shafts.maskScale = connector.maskScale;
                shafts.topDownMaskCamera = connector.topDownMaskCamera;
                shafts.fogOfWarRadius = connector.fogOfWarRadius;
                shafts.playerPos = connector.playerPos;
                shafts.fogOfWarPower = connector.fogOfWarPower;

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

                //v0.3
                shafts.sunTransform.value = connector.sunLight.transform.position;
                // shafts.sunTransform.value = new Vector3(connector.Sun.x, connector.Sun.y, connector.Sun.z);// connector.sun.transform.position;
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
                //Debug.Log("shafts.renderClouds = " + shafts.renderClouds);

                //SUN SHAFTS
                shafts.raysResolutionA = connector.raysResolutionA;
                shafts._rayColor = connector._rayColor;
                shafts.rayShadowingA = connector.rayShadowingA;
                shafts._underDomeColor = connector._underDomeColor;

                shafts.enableShafts = connector.enableShafts; //v0.3

                //v0.6
                cloudDistanceParams = connector.cloudDistanceParams;
                controlBackAlphaPower = connector.controlBackAlphaPower;
                controlCloudAlphaPower = connector.controlCloudAlphaPower;
                controlCloudEdgeA = connector.controlCloudEdgeA;
                controlCloudEdgeOffset = connector.controlCloudEdgeOffset;
                depthDilation = connector.depthDilation;
                enabledTemporalAA = connector.enabledTemporalAA;
                TemporalResponse = connector.TemporalResponse;
                TemporalGain = connector.TemporalGain;

                //v0.7 - VORTEX - SUPERCELLS WIP
                shafts.enableVortex = connector.enableVortex;
                shafts.resetVortex = connector.resetVortex;
                shafts.vortexPosRadius = connector.vortexPosRadius;
                shafts.vortexControlsA = connector.vortexControlsA;
                shafts.superCellPosRadius = connector.superCellPosRadius;
                shafts.superCellControlsA = connector.superCellControlsA;
            }
        }

        //ACCESS MASK
        //WaterAirMaskSM waterAirMask;
        public FloatParameter scalerMask = new FloatParameter(0.2f);

        public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
        {
            if (sheetSHAFTS == null)
                return;

            connectSuntoNebulaCloudsHDRP connector = Camera.main.GetComponent<connectSuntoNebulaCloudsHDRP>();
            if (connector != null)
            {
                passParamsToClouds(connector);
                // connector.passParamsToClouds();
            }
            //renderClouds = connector.renderClouds;

            //RenderShafts(cmd, camera, source, destination);
            //Debug.Log("renderClouds = " + renderClouds);

            //v0.4
            if (enableBehindTranspClouds)
            {
                renderClouds = true;
            }
            if (autoTranspChange)
            {
                if (Camera.main.transform.position.y < transpToggleHeight)
                {
                    renderClouds = true;
                }
                else
                {
                    renderClouds = false;
                }
            }

            if (renderClouds)
            {
                if (cloudChoice == 0)
                {
                    if (enableShafts) //v0.3
                    {
                        RenderFog(cmd, source, m_TemporaryColorTextureTMP);
                        RenderShafts(cmd, camera, m_TemporaryColorTextureTMP, destination);
                    }
                    else
                    {
                        RenderFog(cmd, source, destination);
                    }
                }
                if (cloudChoice == 1)
                {
                    //RenderFullVolumetricClouds(cmd, source, destination);

                    if (enableShafts) //v0.3
                    {
                        //RenderShafts(cmd, camera, source, source);
                        //var format = RenderTextureFormat.DefaultHDR;//camera.hdr ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
                        //RenderTexture tempBUFF = RenderTexture.GetTemporary(source.rt.width, source.rt.height, 0, format);
                        //cmd.GetTemporaryRT(lrDepthBuffer.id, opaqueDesc, filterMode);
                        RenderFullVolumetricClouds(cmd, source, m_TemporaryColorTextureTMP, camera); //v0.4a
                        RenderShafts(cmd, camera, m_TemporaryColorTextureTMP, destination);
                    }
                    else
                    {
                        RenderFullVolumetricClouds(cmd, source, destination, camera); //v0.4a
                    }
                }
            }
            else if (enableShafts)
            {
                RenderShafts(cmd, camera, source, destination);
            }
        }

        public void RenderShafts(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
        {
            //if (sheetSHAFTS == null)
            // return;

            sheetSHAFTS.SetFloat("_Intensity", intensity.value);
            sheetSHAFTS.SetTexture("_InputTexture", source);

            //ACCESS MASK
            //if (1 == 0) //v0.5a
            //{
            //    if (Camera.main != null)
            //    {
            //        float camXRot = Camera.main.transform.eulerAngles.x;
            //        if (camXRot > 180)
            //        {
            //            camXRot = -(360 - camXRot);
            //        }
            //        sheetSHAFTS.SetFloat("_CamXRot", camXRot);
            //    }
            //    var downsampleProperties = new MaterialPropertyBlock();
            //    downsampleProperties.SetTexture("_InputTexture", source);
            //    downsampleProperties.SetFloat("waterHeight", waterHeight.value);
            //    HDUtils.DrawFullScreen(cmd, sheetSHAFTS, m_TemporaryColorTextureScaled, downsampleProperties, 7);

            //   // var UPsampleProperties = new MaterialPropertyBlock();
            //    //UPsampleProperties.SetTexture("_InputTexture", m_TemporaryColorTextureScaled);
            //    //UPsampleProperties.SetFloat("scalerMask", scalerMask.value); //SCALE RESULT
            //    //HDUtils.DrawFullScreen(cmd, sheetSHAFTS, destination, UPsampleProperties, 6);
            //    //return;
            //}

            //Volume volume = this.m_TemporaryColorTexture;// GetComponent<Volume>();
            //DepthOfField tempDof;

            //if (volume.profile.TryGet<DepthOfField>(out tempDof))
            //{
            //    depthOfField = tempDof;
            //}

            //depthOfField.focusDistance.value = 42f;
            //if (this.m_TemporaryColorTexture != null)
            //{
            //    Debug.Log(this.m_TemporaryColorTexture.rt.width);
            //}

            //sheetSHAFTS.SetTexture("_MainTexA", source);

            //SHAFTS        
            //opaqueDesc.depthBufferBits = 0;

            // Material sheetSHAFTS = blitMaterial;
            //sheetSHAFTS.SetFloat("_Blend", blend);

            Camera cameraA = Camera.main;
            // we actually need to check this every frame
            if (useDepthTexture.value)
            {
                cameraA.depthTextureMode |= DepthTextureMode.Depth;
            }

            Vector3 v = Vector3.one * 0.5f;
            if (sunTransform != Vector3.zero)
            {
                v = Camera.main.WorldToViewportPoint(sunTransform.value);
            }
            else
            {
                v = new Vector3(0.5f, 0.5f, 0.0f);
            }


            /////// UNDERWATER ONLY, always face camera
            if (shaftsFollowCamera.value == true)
            {
                v = Camera.main.WorldToViewportPoint(Camera.main.transform.position
                    + (new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z) * 1000 + Vector3.up * 500));
            }

            //v0.1
            int rtW = source.referenceSize.x; // opaqueDesc.width;
            int rtH = source.referenceSize.y; // opaqueDesc.height;
                                              // Debug.Log(rtW + ", " + rtH);

            //cmd.GetTemporaryRT(lrDepthBuffer.id, opaqueDesc, filterMode);
            //int lrDepthBufferID = Shader.PropertyToID("lrDepthBuffer");
            //cmd.GetTemporaryRT(lrDepthBufferID, rtW, rtH, 0, FilterMode.Bilinear);

            var format = cameraA.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default; //v3.4.9
                                                                                                          //RenderTextureFormat format = RenderTextureFormat.ARGB32;
                                                                                                          //RenderTexture lrDepthBuffer = RenderTexture.GetTemporary(rtW, rtH, 0, format);
                                                                                                          //RenderTexture lrColorA = RenderTexture.GetTemporary(rtW, rtH, 0, format);
                                                                                                          //RenderTexture m_TemporaryColorTexture = RenderTexture.GetTemporary(rtW, rtH, 0, format);
                                                                                                          //int lrDepthBufferID = Shader.PropertyToID("lrDepthBuffer");
                                                                                                          //cmd.GetTemporaryRT(lrDepthBufferID, rtW, rtH, 0, FilterMode.Bilinear);
                                                                                                          //RenderTargetIdentifier lrDepthBuffer;
                                                                                                          //RTHandle lrDepthBufferIDA =  RenderTexture.GetTemporary(rtW, rtH, 0, format).GetInstanceID;
                                                                                                          //lrDepthBufferIDA.rt = lrDepthBuffer;       



            // mask out everything except the skybox     
            sheetSHAFTS.SetVector("_BlurRadius4", new Vector4(1.0f, 1.0f, 0.0f, 0.0f) * sunShaftBlurRadius.value);
            sheetSHAFTS.SetVector("_SunThreshold", sunThreshold.value);

            if (!useDepthTexture.value)
            {
                RenderTexture tmpBuffer = RenderTexture.GetTemporary(rtW, rtH, 0, format);
                RenderTexture.active = tmpBuffer;
                GL.ClearWithSkybox(false, cameraA);

                var UPsampleProperties01 = new MaterialPropertyBlock();


                UPsampleProperties01.SetTexture("_Skybox", tmpBuffer);
                //UnityEngine.Rendering.HighDefinition.
                //cmd.Blit(source, lrDepthBufferID, sheetSHAFTS, 3);
                UPsampleProperties01.SetTexture("_MainTexA", source);
                HDUtils.DrawFullScreen(cmd, sheetSHAFTS, lrDepthBuffer, UPsampleProperties01, 3);
                //Blit(cmd, source, lrDepthBufferID, sheetSHAFTS, 3);//Blit(cmd, source, lrDepthBuffer.Identifier(), sheetSHAFTS, 3);
                RenderTexture.ReleaseTemporary(tmpBuffer);
                // Debug.Log("pass 3");
            }
            else
            {
                var UPsampleProperties02 = new MaterialPropertyBlock();
                UPsampleProperties02.SetTexture("_MainTexA", source);
                HDUtils.DrawFullScreen(cmd, sheetSHAFTS, lrDepthBuffer, UPsampleProperties02, 2);
                //cmd.Blit(source, lrDepthBufferID, sheetSHAFTS, 2);
                //Blit(cmd, source, lrDepthBuffer.Identifier(), sheetSHAFTS, 2);
                //Debug.Log("pass 2");
            }

            // radial blur:

            //int m_TemporaryColorTextureID = Shader.PropertyToID("m_TemporaryColorTexture");
            //cmd.Blit(source, m_TemporaryColorTextureID);
            // Blit(cmd, source, m_TemporaryColorTexture.Identifier()); //KEEP BACKGROUND
            var UPsampleProperties03 = new MaterialPropertyBlock();
            UPsampleProperties03.SetTexture("_MainTexA", source);
            UPsampleProperties03.SetFloat("scalerMask", 1);
            HDUtils.DrawFullScreen(cmd, sheetSHAFTS, m_TemporaryColorTexture, UPsampleProperties03, 6);

            radialBlurIterations.value = Mathf.Clamp(radialBlurIterations.value, 1, 4);

            float ofs = sunShaftBlurRadius.value * (1.0f / 768.0f);

            sheetSHAFTS.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0.0f, 0.0f));

            float adjustX = 0.5f;
            if (v.x < 0.5f)
            {
                float diff = 0.5f - v.x;
                adjustX = adjustX - 0.5f * diff;
            }
            float adjustY = 0.5f;
            if (v.y > 1.25f)
            {
                float diff2 = v.y - 1.25f;
                adjustY = adjustY - 0.3f * diff2;
            }
            if (v.y > 1.8f)
            {
                v.y = 1.8f;
                float diff3 = v.y - 1.25f;
                adjustY = 0.5f - 0.3f * diff3;
            }

            sheetSHAFTS.SetVector("_SunPosition", new Vector4(v.x * 0.5f + adjustX, v.y * 0.5f + adjustY, v.z, maxRadius.value));

            for (int it2 = 0; it2 < radialBlurIterations.value; it2++)
            {
                // each iteration takes 2 * 6 samples
                // we update _BlurRadius each time to cheaply get a very smooth look
                //lrColorA = RenderTexture.GetTemporary(rtW, rtH, 0);
                //cmd.Blit(lrDepthBufferID, lrColorA, sheetSHAFTS, 1);

                var UPsampleProperties04 = new MaterialPropertyBlock();
                UPsampleProperties04.SetTexture("_MainTexA", lrDepthBuffer);
                HDUtils.DrawFullScreen(cmd, sheetSHAFTS, lrColorA, UPsampleProperties04, 1);

                //Blit(cmd, lrDepthBuffer.Identifier(), lrColorA, sheetSHAFTS, 1);
                //cmd.ReleaseTemporaryRT(lrDepthBuffer); //cmd.ReleaseTemporaryRT(lrDepthBuffer.id);
                ofs = sunShaftBlurRadius.value * (((it2 * 2.0f + 1.0f) * 6.0f)) / 768.0f;
                sheetSHAFTS.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0.0f, 0.0f));

                //cmd.GetTemporaryRT(lrDepthBuffer.id, opaqueDesc, filterMode);
                //Blit(cmd, lrColorA, lrDepthBuffer.Identifier(), sheetSHAFTS, 1);

                //cmd.GetTemporaryRT(lrDepthBufferID, rtW, rtH, 0, FilterMode.Bilinear);
                // cmd.Blit(lrColorA, lrDepthBufferID, sheetSHAFTS, 1);

                UPsampleProperties04.SetTexture("_MainTexA", lrColorA);
                HDUtils.DrawFullScreen(cmd, sheetSHAFTS, lrDepthBuffer, UPsampleProperties04, 1);

                //RenderTexture.ReleaseTemporary(lrColorA);
                ofs = sunShaftBlurRadius.value * (((it2 * 2.0f + 2.0f) * 6.0f)) / 768.0f;
                sheetSHAFTS.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0.0f, 0.0f));
            }

            // put together:
            if (v.z >= 0.0f)
            {
                sheetSHAFTS.SetVector("_SunColor", new Vector4(sunColor.value.r, sunColor.value.g, sunColor.value.b, sunColor.value.a) * sunShaftIntensity.value);
            }
            else
            {
                //sheetSHAFTS.SetVector("_SunColor", Vector4.zero); // no backprojection !
                sheetSHAFTS.SetVector("_SunColor", new Vector4(sunColor.value.r * sunShaftIntensity.value, sunColor.value.g * sunShaftIntensity.value, sunColor.value.b * sunShaftIntensity.value, 0.01f));
            }

            //cmd.SetGlobalTexture("_ColorBuffer", lrDepthBuffer.Identifier());
            cmd.SetGlobalTexture("_ColorBuffer", lrDepthBuffer);

            // Blit(cmd, m_TemporaryColorTexture.Identifier(), source, sheetSHAFTS, (screenBlendMode == BlitSunShaftsSRP.BlitSettings.ShaftsScreenBlendMode.Screen) ? 0 : 4);
            //cmd.Blit(m_TemporaryColorTextureID, source, sheetSHAFTS, (screenBlendMode == ShaftsScreenBlendMode.Screen) ? 0 : 4);

            //sheetSHAFTS.SetTexture("_MainTexA", m_TemporaryColorTexture);
            //HDUtils.DrawFullScreen(cmd, sheetSHAFTS, destination, null, (screenBlendMode == ShaftsScreenBlendMode.Screen) ? 0 : 4);
            // return;

            //MASK RESULT
            var UPsampleProperties05 = new MaterialPropertyBlock();
            UPsampleProperties05.SetTexture("_MainTexA", m_TemporaryColorTexture);

            //v0.5a
            //HDUtils.DrawFullScreen(cmd, sheetSHAFTS, lrColorA, UPsampleProperties05, (screenBlendMode == ShaftsScreenBlendMode.Screen) ? 0 : 4);
            HDUtils.DrawFullScreen(cmd, sheetSHAFTS, destination, UPsampleProperties05, (screenBlendMode == ShaftsScreenBlendMode.Screen) ? 0 : 4);

            //var UPsampleProperties1 = new MaterialPropertyBlock();
            //UPsampleProperties1.SetTexture("_InputTexture", lrColorA);
            //UPsampleProperties1.SetFloat("scalerMask", 1.0f);
            //HDUtils.DrawFullScreen(cmd, sheetSHAFTS, destination, UPsampleProperties1, 6);
            return;//v0.5a

            //var UPsampleProperties1 = new MaterialPropertyBlock();
            ////UPsampleProperties.SetTexture("_InputTexture", m_TemporaryColorTextureScaled);
            //UPsampleProperties1.SetTexture("_WaterInterfaceTex", m_TemporaryColorTextureScaled);
            //UPsampleProperties1.SetTexture("_MainTexA", lrColorA.rt);
            //UPsampleProperties1.SetTexture("_SourceTex", source);
            //UPsampleProperties1.SetFloat("scalerMask", scalerMask.value);

            //////////////////// REFRACT LINE
            //if (BumpMap.value != null)
            //{
            //    UPsampleProperties1.SetTexture("_BumpTex", BumpMap.value);
            //    //Debug.Log(BumpMap.value.width);
            //}
            //UPsampleProperties1.SetFloat("_BumpMagnitude", BumpIntensity.value);
            //UPsampleProperties1.SetFloat("_BumpScale", BumpScale.value);
            ////v4.6
            //UPsampleProperties1.SetFloat("_BumpMagnitudeRL", BumpIntensityRL.value);
            //UPsampleProperties1.SetFloat("_BumpScaleRL", BumpScaleRL.value);
            //UPsampleProperties1.SetFloat("_BumpLineHeight", BumpLineHeight.value);
            //UPsampleProperties1.SetVector("_BumpVelocity", BumpVelocity.value);
            //UPsampleProperties1.SetVector("_underWaterTint", underWaterTint.value);
            //UPsampleProperties1.SetFloat("_underwaterDepthFade", underwaterDepthFade.value);
            //UPsampleProperties1.SetFloat("_refractLineWidth", refractLineWidth.value); //v4.6
            //UPsampleProperties1.SetFloat("_refractLineXDisp", refractLineXDisp.value); //v4.6
            //UPsampleProperties1.SetFloat("_refractLineXDispA", refractLineXDispA.value); //v4.6
            //UPsampleProperties1.SetVector("_refractLineFade", refractLineFade.value); //v4.6
            //////////////////// END REFRACT LINE

            //HDUtils.DrawFullScreen(cmd, sheetSHAFTS, destination, UPsampleProperties1, 8);






            //Graphics.Blit(currentDestination, destination, sunShaftsMaterial, 8);
            //sheetSHAFTS.SetTexture("_MainTexA", lrColorA);
            //sheetSHAFTS.SetTexture("_MainTexA", m_TemporaryColorTexture);
            //HDUtils.DrawFullScreen(cmd, sheetSHAFTS, destination, null, (screenBlendMode == ShaftsScreenBlendMode.Screen) ? 0 : 4);

            // cmd.ReleaseTemporaryRT(lrDepthBuffer.id);
            // cmd.ReleaseTemporaryRT(m_TemporaryColorTexture.id);
            // cmd.ReleaseTemporaryRT(lrDepthBufferID);
            // cmd.ReleaseTemporaryRT(m_TemporaryColorTextureID);

            //execute
            //sheetSHAFTS.SetTexture("_MainTexA", source);
            //HDUtils.DrawFullScreen(cmd, sheetSHAFTS, destination, null, 6);
            //context.ExecuteCommandBuffer(cmd);
            // CommandBufferPool.Release(cmd);
            //RenderTexture.ReleaseTemporary(lrColorA);
            //RenderTexture.ReleaseTemporary(lrDepthBuffer);
            //RenderTexture.ReleaseTemporary(m_TemporaryColorTexture);


            //  HDUtils.DrawFullScreen(cmd, sheetSHAFTS, destination, null, 0);
        }
        //public override void Cleanup() => CoreUtils.Destroy(sheetSHAFTS);


        public override void Cleanup()//protected override void Cleanup()
        {
            CoreUtils.Destroy(sheetSHAFTS);
            if (m_TemporaryColorTexture != null)
            {
                m_TemporaryColorTexture.Release();
                m_TemporaryColorTextureTMP.Release(); //v0.3
                lrDepthBuffer.Release();
                lrColorA.Release();
                m_TemporaryColorTextureScaled.Release();

                _cloudBuffer.Release();
                _cloudBufferP.Release();
                _cloudBufferP1.Release();
                tmpBuffer.Release();
                tmpBuffer1.Release();
                tmpBuffer2.Release();
                tmpBuffer3.Release();//v0.6
                previousDepthTexture.Release();//v0.6
                previousFrameTexture.Release();//v0.6
                new1.Release();

                tmpBufferAA.Release();
                tmpBufferAA2.Release();
            }
        }

        ///////////////// VOLUME CLOUDS v0.1 //////////////////////////


        //RenderTexture tmpBuffer;//v2.1.15  
        RTHandle tmpBuffer;

        public bool blendBackground = false;
        public Camera backgroundCam;
        public Material backgroundMat;

        //OPTIMIZE
        int toggleCounter = 0;
        public int splitPerFrames = 1; //v2.1.19
        public bool cameraMotionCompensate = true;//v2.1.19
                                                  // RenderTexture _cloudBuffer;
                                                  //RenderTexture _cloudBufferP;
                                                  // RenderTexture _cloudBufferP1;


        RTHandle _cloudBuffer;
        RTHandle _cloudBufferP;
        RTHandle _cloudBufferP1;
        RTHandle tmpBuffer1;
        RTHandle tmpBuffer2;
        RTHandle tmpBuffer3; //v0.6
        RTHandle previousDepthTexture;//v0.6
        RTHandle previousFrameTexture;//v0.6

        RTHandle tmpBufferAA;
        RTHandle tmpBufferAA2;

        public float updateRate = 0.3f;
        // public int resolution = 256;
        public float downScaleFactor = 2; //v0.1 lower by default
        public bool downScale = false;
        //RenderTexture _prevcloudBuffer;
        public bool _needsReset = true;
        Vector3 prevCameraRot;
        Vector3 prevCameraRotP;//v4.8.3 previous position when frame grabbed
                               //RenderTexture new1; //v4.5
        RTHandle new1;
        //v4.8.3
        int countCameraSteady = 0;
        //v4.8
        public bool enableReproject = false;
        public bool autoReproject = false;
        public float lowerRefrReflResFactor = 3;
        RenderTexture CreateBuffer()
        {
            //var width = (_columns + 1) * 2;
            //var height = _totalRows + 1;
            float lowerRefrRefl = 1; //v0.1
                                     //if (currentCamera.name.Contains("refract") || currentCamera.name.Contains("reflect") || currentCamera.name.Contains("Refl")
                                     //    || currentCamera.name.Contains("Refract"))

            if (isForReflections)
            {
                lowerRefrRefl = lowerRefrReflResFactor; //(downScaleFactor * lowerRefrRefl) 
            }

            //if(currentCamera.gameObject.name == Camera.main.gameObject.name)
            // {
            //     lowerRefrRefl = 2;
            //}
            //Debug.Log(downScaleFactor);
            RenderTexture buffer = new RenderTexture((int)(1280 / (downScaleFactor * lowerRefrRefl)), (int)(720 / (downScaleFactor * lowerRefrRefl)), 0, RenderTextureFormat.ARGB32); //SM v4.0
                                                                                                                                                                                      //buffer.hideFlags = HideFlags.DontSave;
                                                                                                                                                                                      //buffer.hideFlags = HideFlags.None;
            buffer.filterMode = FilterMode.Bilinear; //FilterMode.Point; //v4.8.8
            buffer.wrapMode = TextureWrapMode.Repeat;
            return buffer;
        }

        //public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
        void CustomGraphicsBlitOpt(
            RTHandle source, RTHandle dest,
            RenderTexture skysource,
            Material fxMaterial, int passNr, Gradient DistGradient,
            Vector2 GradientBounds, Texture2D colourPalette, Camera cam, bool toggle, bool splitPerFrame, bool WebGL, CommandBuffer cmd)
        // void CustomGraphicsBlitOpt(ScriptableRenderContext context, RenderTargetIdentifier source, UnityEngine.Rendering.Universal.RenderTargetHandle dest, RenderTexture skysource, Material fxMaterial, int passNr, Gradient DistGradient,
        //     Vector2 GradientBounds, Texture2D colourPalette, Camera cam, bool toggle, bool splitPerFrame, bool WebGL, CommandBuffer cmd, RenderTextureDescriptor opaqueDesc)
        {
            //URP
            //int rtW = opaqueDesc.width;
            //int rtH = opaqueDesc.height;
            var format = allowHDR ? RenderTextureFormat.Default : RenderTextureFormat.DefaultHDR; //v3.4.9 //v LWRP

            if (!fastest)
            {
                fxMaterial.SetInt("_fastest", 0);
            }
            else
            {
                fxMaterial.SetInt("_fastest", 1);
            }

            if (toggle)
            {
                //v4.8.3                
                if (enableReproject || countCameraSteady > 1)
                {
                    Graphics.Blit(new1, _cloudBufferP);
                }

                //v2.1.15
                if (!fastest)
                {
                    fxMaterial.SetTexture("_SkyTex", skysource);
                }

                //fxMaterial.SetTexture("_CloudTex", _cloudBuffer);

                //v4.5
                //         if (new1 == null)
                //          {
                //             new1 = CreateBuffer();
                //         }
                //RenderTexture.active = new1;

                //fxMaterial.SetTexture ("_MainTex", source);
                //fxMaterial.SetTexture("_MainTexB", source);

                //v2.1.20
                int signer = 1;
                if (WebGL)
                {
                    signer = -1;
                }
                //v4.0
                Camera cam1 = cam;
                if (currentCamera != null) //Camera.main != null)
                {
                    cam1 = currentCamera;// Camera.main;
                }
                //Matrix4x4 scaler = cam.cameraToWorldMatrix * Matrix4x4.Scale(new Vector3(1, 1 * signer, -1)) * cam1.projectionMatrix.inverse * Matrix4x4.Scale(new Vector3(-1, 1, 1));//v2.1.19
                Matrix4x4 scaler = cam.cameraToWorldMatrix * Matrix4x4.Scale(new Vector3(1, 1 * signer, 1)) * cam1.projectionMatrix.inverse;//v2.1.19 //URP v0.2                                                                                                                                                                 //scaler[0,3] = scaler[0,3] + 0.1f;

                fxMaterial.SetMatrix("_WorldClip", scaler);

                fxMaterial.SetTexture("_ColorRamp", colourPalette);

                if (GradientBounds != Vector2.zero)
                {
                    fxMaterial.SetFloat("_Close", GradientBounds.x);
                    fxMaterial.SetFloat("_Far", GradientBounds.y);
                }

                // Blit(cmd, source, new1, fxMaterial, 6);
                var UPsampleProperties1 = new MaterialPropertyBlock();
                //UPsampleProperties1.SetTexture("_MainTexB", source);
                HDUtils.DrawFullScreen(cmd, fxMaterial, _cloudBuffer, UPsampleProperties1, 9); //6

                // Graphics.Blit(new1, _cloudBuffer); //v4.8.3
                // Blit(cmd, new1, _cloudBuffer); //URP
                //UPsampleProperties1.SetTexture("_MainTex", new1);
                //HDUtils.DrawFullScreen(cmd, fxMaterial, _cloudBuffer, UPsampleProperties1);


                //Graphics.Blit(new1, _cloudBuffer);
                //UPsampleProperties1.SetTexture("_MainTexB", source);
                //UPsampleProperties1.SetTexture("_CloudTex", _cloudBuffer);
                // HDUtils.DrawFullScreen(cmd, fxMaterial, dest, UPsampleProperties1, 9);//14

                //UPsampleProperties1.SetTexture("_MainTexB", _cloudBuffer);
                //UPsampleProperties1.SetTexture("_CloudTex", _cloudBuffer);
                //HDUtils.DrawFullScreen(cmd, fxMaterial, dest, UPsampleProperties1, 14);

                //release
                prevCameraRotP = cam.transform.eulerAngles;

                //context.ExecuteCommandBuffer(cmd);
                //CommandBufferPool.Release(cmd);
            }
            else
            {
                //URP
                //RenderTexture tmpBuffer1 = RenderTexture.GetTemporary(rtW, rtH, 0, format); //v0.1
                // RTHandle tmpBuffer11;
                //tmpBuffer11.rt = tmpBuffer1;

                //RenderTexture.active = tmpBuffer1; 
                //GL.ClearWithSkybox(false, cam);

                //Blit(cmd, source, tmpBuffer1);
                //var UPsampleProperties1 = new MaterialPropertyBlock();
                //UPsampleProperties1.SetTexture("_MainTex", source);
                //HDUtils.DrawFullScreen(cmd, fxMaterial, tmpBuffer1, UPsampleProperties1);
                // Graphics.Blit(source, tmpBuffer1);

                fxMaterial.SetTexture("_MainTexB", source);

                //fxMaterial.SetTexture("_MainTex", source);
                if (!fastest)
                {
                    fxMaterial.SetTexture("_SkyTex", skysource);//v2.1.15
                }

                if (splitPerFrames > 0 && enableReproject && Application.isPlaying)
                {
                    if (splitPerFrame)
                    {
                        fxMaterial.SetTexture("_CloudTex", _cloudBufferP1);
                    }
                    else
                    {
                        //v4.8.3
                        if (autoReproject)
                        {
                            if (autoReproject && enableReproject && countCameraSteady > 2)
                            {
                                fxMaterial.SetTexture("_CloudTex", _cloudBufferP1);
                            }
                            else
                            {
                                fxMaterial.SetTexture("_CloudTex", _cloudBuffer);
                            }
                        }
                        else
                        {
                            fxMaterial.SetTexture("_CloudTex", _cloudBufferP1);
                        }
                    }
                }
                else
                {
                    fxMaterial.SetTexture("_CloudTex", _cloudBuffer); //v4.8.3
                }

                //URP                
                //RenderTexture tmpBuffer2 = RenderTexture.GetTemporary(rtW, rtH, 0, format);

                //v2.1.20
                int signer = 1;
                if (WebGL)
                {
                    signer = -1;
                }
                //v4.0
                Camera cam1 = cam;
                if (currentCamera != null) //Camera.main != null)
                {
                    cam1 = currentCamera;// Camera.main;
                }

                Matrix4x4 scaler = cam.cameraToWorldMatrix * Matrix4x4.Scale(new Vector3(1, 1 * signer, 1)) * cam1.projectionMatrix.inverse;//v2.1.19 //URP v0.2

                if (splitPerFrame)
                {
                    float Xdisp = cam.transform.eulerAngles.y - prevCameraRot.y;
                    float Ydisp = -(cam.transform.eulerAngles.x - prevCameraRot.x);

                    //v4.8.3
                    if (autoReproject)
                    {
                        //Debug.Log(Xdisp + " :" + Ydisp);
                        if (Mathf.Abs(Xdisp) == 0.000000f && Mathf.Abs(Ydisp) == 0.000000f)
                        {
                            if (countCameraSteady > 8)
                            {
                                enableReproject = true; //connector.enableReproject = true;
                            }
                            else { countCameraSteady++; }
                        }
                        else
                        {
                            //Debug.Log(toggleCounter + " :::" + countCameraSteady);
                            if (toggleCounter == 1 && countCameraSteady > 0)
                            {
                                enableReproject = false; //connector.enableReproject = false;
                                countCameraSteady = 0;

                                //Debug.Log(Xdisp + " :" + Ydisp);
                            }
                        }
                    }

                    if (Xdisp > 122f || Xdisp < -122f)
                    {
                        Xdisp = 0;
                    }
                    if (Ydisp > 122f || Ydisp < -122f)
                    {
                        Ydisp = 0;
                    }

                    scaler[0, 3] = 0.009f * Xdisp;//0.005f * Xdisp;
                    scaler[1, 3] = 0.012f * Ydisp;
                    scaler[2, 3] = 1;// 1f - 0.005f * (Xdisp+Ydisp); // 0.95f+ 0.005f*(splitPerFrames - toggleCounter); //image scaler
                }
                else
                {
                    prevCameraRot = cam.transform.eulerAngles;
                }
                fxMaterial.SetMatrix("_WorldClip", scaler);

                //GL.PushMatrix();
                //GL.LoadOrtho();

                //v0.1
                var UPsampleProperties1 = new MaterialPropertyBlock();
                //UPsampleProperties1.SetTexture("_MainTexB", source);
                UPsampleProperties1.SetMatrix("_WorldClip", scaler);
                UPsampleProperties1.SetTexture("_MainTex", _cloudBuffer);
                UPsampleProperties1.SetTexture("_MainTexB", source);
                UPsampleProperties1.SetTexture("_CloudTex", _cloudBuffer);
                UPsampleProperties1.SetTexture("_SkyTex", skysource);

                if (splitPerFrame && cameraMotionCompensate)
                {
                    // fxMaterial.SetPass(8); //4
                    //Blit(cmd, source, tmpBuffer2, fxMaterial, 8);
                    HDUtils.DrawFullScreen(cmd, fxMaterial, dest, UPsampleProperties1, 11);//8
                }
                else
                {
                    // fxMaterial.SetPass(7);//3
                    //Blit(cmd, source, tmpBuffer2, fxMaterial, 7);
                    HDUtils.DrawFullScreen(cmd, fxMaterial, dest, UPsampleProperties1, 10); //7
                }

                //Blit(cmd, tmpBuffer2, source);

                //Graphics.Blit(tmpBuffer2, source);
                //Graphics.Blit(tmpBuffer2, dest);
                //var UPsampleProperties2 = new MaterialPropertyBlock();
                // UPsampleProperties2.SetMatrix("_WorldClip", scaler);
                //UPsampleProperties2.SetTexture("_MainTexB", source);
                //UPsampleProperties2.SetTexture("_MainTexA", tmpBuffer2);
                //UPsampleProperties2.SetTexture("_MainTex", tmpBuffer2);
                //HDUtils.DrawFullScreen(cmd, fxMaterial, dest, UPsampleProperties1, 5);

                //UPsampleProperties1.SetTexture("_MainTexB", source);
                //UPsampleProperties1.SetTexture("_CloudTex", source);
                //UPsampleProperties2.SetTexture("_CloudTex", tmpBuffer2);
                //HDUtils.DrawFullScreen(cmd, fxMaterial, dest, UPsampleProperties2, 14);

                //          context.ExecuteCommandBuffer(cmd);
                //          CommandBufferPool.Release(cmd);

                //          RenderTexture.ReleaseTemporary(tmpBuffer1);
                //          RenderTexture.ReleaseTemporary(tmpBuffer2);
                return;
            }
        }

        /////////////// Render background clouds -----------------------------
        /////////////////////// VOLUME FOG SRP /////////////////////////////////////
        //public void RenderFog(ScriptableRenderContext context, UnityEngine.Rendering.Universal.RenderingData renderingData, CommandBuffer cmd, RenderTextureDescriptor opaqueDesc)
        public void RenderFog(CommandBuffer cmd, RTHandle source, RTHandle destination)
        //public override void Render(PostProcessRenderContext context)
        {
            //var _material = context.propertySheets.Get(Shader.Find("Hidden/InverseProjectVFogLWRP"));
            Material _material = sheetSHAFTS;// blitMaterial;
            _material.SetFloat("_DistanceOffset", _startDistance);
            _material.SetFloat("_Height", _fogHeight); //v0.1                                                                      
            _material.SetFloat("_cameraRoll", _cameraRoll);
            _material.SetVector("_cameraDiff", _cameraDiff);
            _material.SetFloat("_cameraTiltSign", _cameraTiltSign);

            var mode = RenderSettings.fogMode;
            if (mode == FogMode.Linear)
            {
                var start = RenderSettings.fogStartDistance;//RenderSettings.RenderfogStartDistance;
                var end = RenderSettings.fogEndDistance;
                var invDiff = 1.0f / Mathf.Max(end - start, 1.0e-6f);
                _material.SetFloat("_LinearGrad", -invDiff);
                _material.SetFloat("_LinearOffs", end * invDiff);
                _material.DisableKeyword("FOG_EXP");
                _material.DisableKeyword("FOG_EXP2");
            }
            else if (mode == FogMode.Exponential)
            {
                const float coeff = 1.4426950408f; // 1/ln(2)
                var density = RenderSettings.fogDensity;// RenderfogDensity;
                _material.SetFloat("_Density", coeff * density * _fogDensity);
                _material.EnableKeyword("FOG_EXP");
                _material.DisableKeyword("FOG_EXP2");
            }
            else // FogMode.ExponentialSquared
            {
                const float coeff = 1.2011224087f; // 1/sqrt(ln(2))
                var density = RenderSettings.fogDensity;//RenderfogDensity;
                _material.SetFloat("_Density", coeff * density * _fogDensity);
                _material.DisableKeyword("FOG_EXP");
                _material.EnableKeyword("FOG_EXP2");
            }
            if (_useRadialDistance)
                _material.EnableKeyword("RADIAL_DIST");
            else
                _material.DisableKeyword("RADIAL_DIST");

            if (_fadeToSkybox)
            {
                _material.DisableKeyword("USE_SKYBOX");
                _material.SetColor("_FogColor", _FogColor);// RenderfogColor);//v0.1            
            }
            else
            {
                _material.DisableKeyword("USE_SKYBOX");
                _material.SetColor("_FogColor", _FogColor);// RenderfogColor);
            }

            //v0.1  //URP v0.1       
            if (_material != null && noiseTexture != null)
            {
                _material.SetTexture("_NoiseTex", noiseTexture);
            }

            // Calculate vectors towards frustum corners.
            Camera camera = Camera.main;// currentCamera; //Camera.main; //v0.1.1
            var cam = camera;// GetComponent<Camera>();
            var camtr = cam.transform;

            ////////// SCATTER
            var camPos = camtr.position;
            float FdotC = camPos.y - _fogHeight;
            float paramK = (FdotC <= 0.0f ? 1.0f : 0.0f);

            _material.SetFloat("farCamDistFactor", farCamDistFactor);
            _material.SetVector("_CameraWS", camPos);
            //Debug.Log("_HeightParams="+ new Vector4(_fogHeight, FdotC, paramK, heightDensity * 0.5f));

            _material.SetVector("_HeightParams", new Vector4(_fogHeight, FdotC, paramK, heightDensity * 0.5f));
            _material.SetVector("_DistanceParams", new Vector4(-Mathf.Max(startDistance, 0.0f), 0, 0, 0));
            _material.SetFloat("_NoiseDensity", noiseDensity);
            _material.SetFloat("_NoiseScale", noiseScale);
            _material.SetFloat("_NoiseThickness", noiseThickness);
            _material.SetVector("_NoiseSpeed", noiseSpeed);
            _material.SetFloat("_OcclusionDrop", occlusionDrop);
            _material.SetFloat("_OcclusionExp", occlusionExp);
            _material.SetInt("noise3D", noise3D);
            //SM v1.7
            _material.SetFloat("luminance", luminance);
            _material.SetFloat("lumFac", lumFac);
            _material.SetFloat("Multiplier1", ScatterFac);
            _material.SetFloat("Multiplier2", TurbFac);
            _material.SetFloat("Multiplier3", HorizFac);
            _material.SetFloat("turbidity", turbidity);
            _material.SetFloat("reileigh", reileigh);
            _material.SetFloat("mieCoefficient", mieCoefficient);
            _material.SetFloat("mieDirectionalG", mieDirectionalG);
            _material.SetFloat("bias", bias);
            _material.SetFloat("contrast", contrast);
            _material.SetVector("v3LightDir", Sun);//new Vector3(1, 1, 1));// Sun);//.forward);
            _material.SetVector("v3LightDirFOG", SunFOG);
            _material.SetVector("_TintColor", new Vector4(TintColor.r, TintColor.g, TintColor.b, 1));//68, 155, 345
            _material.SetVector("_TintColorK", new Vector4(TintColorK.x, TintColorK.y, TintColorK.z, 1));
            _material.SetVector("_TintColorL", new Vector4(TintColorL.x, TintColorL.y, TintColorL.z, 1));

            float Foggy = 0;
            if (FogSky) //ClearSkyFac
            {
                Foggy = 1;
            }
            _material.SetFloat("FogSky", Foggy);
            _material.SetFloat("ClearSkyFac", ClearSkyFac);
            //////// END SCATTER

            //LOCAL LIGHT
            _material.SetVector("localLightPos", new Vector4(PointL.x, PointL.y, PointL.z, PointL.w));//68, 155, 345
            _material.SetVector("localLightColor", new Vector4(PointLParams.x, PointLParams.y, PointLParams.z, PointLParams.w));//68, 155, 345                                       
                                                                                                                                //END LOCAL LIGHT
                                                                                                                                //////////// CLOUDS
                                                                                                                                //v3.5.3
                                                                                                                                //if (!useFluidTexture)
                                                                                                                                //{
            _material.SetTexture("_InteractTexture", _InteractTexture);
            //}
            //else
            //{
            //    _material.SetTexture("_InteractTexture", fluidFlow.GetTexture("_MainTex")); //v4.0
            //}
            _material.SetVector("_InteractTexturePos", _InteractTexturePos);
            _material.SetVector("_InteractTextureAtr", _InteractTextureAtr);
            _material.SetVector("_InteractTextureOffset", _InteractTextureOffset); //v4.0
                                                                                   //v3.5.1
            _material.SetFloat("_NearZCutoff", _NearZCutoff);
            _material.SetFloat("_HorizonYAdjust", _HorizonYAdjust);
            _material.SetFloat("_HorizonZAdjust", _HorizonZAdjust);//v2.1.24
            _material.SetFloat("_FadeThreshold", _FadeThreshold);
            //v4.1f
            _material.SetFloat("_mobileFactor", _mobileFactor); //v4.1f
            _material.SetFloat("_alphaFactor", _alphaFactor);
            //v3.5
            _material.SetFloat("_SampleCount0", _SampleCount0);
            _material.SetFloat("_SampleCount1", _SampleCount1);
            _material.SetInt("_SampleCountL", _SampleCountL);
            _material.SetFloat("_NoiseFreq1", _NoiseFreq1);
            _material.SetFloat("_NoiseFreq2", _NoiseFreq2);
            _material.SetFloat("_NoiseAmp1", _NoiseAmp1);
            _material.SetFloat("_NoiseAmp2", _NoiseAmp2);
            _material.SetFloat("_NoiseBias", _NoiseBias);
            //v4.8.6
            if (Application.isPlaying)
            {
                _material.SetVector("_Scroll1", _Scroll1);
                _material.SetVector("_Scroll2", _Scroll2);
            }
            else
            {
                _material.SetVector("_Scroll1", Vector4.zero);
                _material.SetVector("_Scroll2", Vector4.zero);
            }
            _material.SetFloat("_Altitude0", _Altitude0);
            _material.SetFloat("_Altitude1", _Altitude1);
            _material.SetFloat("_FarDist", _FarDist);
            _material.SetFloat("_HGCoeff", _HGCoeff);
            _material.SetFloat("_Exposure", _ExposureUnder); //v4.0
                                                             //v4.8.4
            if (!adjustNightLigting)
            {
                _material.SetFloat("_Scatter", _Scatter);
                _material.SetVector("_GroundColor", _GroundColor);//
                _material.SetFloat("_BackShade", _BackShade);
                _material.SetFloat("turbidity", turbidity);
                _material.SetFloat("_Extinct", _Extinct);
            }
            _material.SetFloat("_SunSize", _SunSize);
            _material.SetVector("_SkyTint", _SkyTint);
            _material.SetFloat("_AtmosphereThickness", _AtmosphereThickness);
            //v3.5
            //_material.SetFloat("_BackShade",_BackShade); //v4.8.6 moved up for night time change
            _material.SetFloat("_UndersideCurveFactor", _UndersideCurveFactor);
            //v2.1.19
            if (localLight != null)
            {
                Vector3 localLightPos = localLight.transform.position;
                //float intensity = Mathf.Pow(10, 3 + (localLightFalloff - 3) * 3);
                currentLocalLightIntensity = Mathf.Pow(10, 3 + (localLightFalloff - 3) * 3);
                //_material.SetVector ("_LocalLightPos", new Vector4 (localLightPos.x, localLightPos.y, localLightPos.z, localLight.intensity * localLightIntensity * intensity));
                _material.SetVector("_LocalLightPos", new Vector4(localLightPos.x, localLightPos.y, localLightPos.z, localLight.intensity * localLightIntensity * currentLocalLightIntensity)); //v0.4 //* localLightIntensity * currentLocalLightIntensity));
                _material.SetVector("_LocalLightColor", new Vector4(localLight.color.r, localLight.color.g, localLight.color.b, localLightFalloff));
            }
            else
            {
                if (currentLocalLightIntensity > 0)
                {
                    currentLocalLightIntensity = 0;
                    //_material.SetVector ("_LocalLightPos", new Vector4 (localLightPos.x, localLightPos.y, localLightPos.z, localLight.intensity * localLightIntensity * intensity));
                    _material.SetVector("_LocalLightColor", Vector4.zero);
                }
            }
            //SM v1.7
            _material.SetFloat("luminance", luminance);
            _material.SetFloat("lumFac", lumFac);
            _material.SetFloat("Multiplier1", ScatterFac);
            _material.SetFloat("Multiplier2", TurbFac);
            _material.SetFloat("Multiplier3", HorizFac);
            //_material.SetFloat("turbidity",turbidity); //v4.8.6
            _material.SetFloat("reileigh", reileigh);
            _material.SetFloat("mieCoefficient", mieCoefficient);
            _material.SetFloat("mieDirectionalG", mieDirectionalG);
            _material.SetFloat("bias", bias);
            _material.SetFloat("contrast", contrast);
            //v4.8
            _material.SetFloat("varianceAltitude1", varianceAltitude1);

            var sceneMode = RenderSettings.fogMode;
            var sceneDensity = 0.01f; //RenderSettings.fogDensity;//v3.0
            var sceneStart = RenderSettings.fogStartDistance;
            var sceneEnd = RenderSettings.fogEndDistance;
            Vector4 sceneParams;
            bool linear = (sceneMode == FogMode.Linear);
            float diff = linear ? sceneEnd - sceneStart : 0.0f;
            float invDiffA = Mathf.Abs(diff) > 0.0001f ? 1.0f / diff : 0.0f;
            sceneParams.x = sceneDensity * 1.2011224087f; // density / sqrt(ln(2)), used by Exp2 fog mode
            sceneParams.y = sceneDensity * 1.4426950408f; // density / ln(2), used by Exp fog mode
            sceneParams.z = linear ? -invDiffA : 0.0f;
            sceneParams.w = linear ? sceneEnd * invDiffA : 0.0f;
            _material.SetVector("_SceneFogParams", sceneParams);
            _material.SetVector("_SceneFogMode", new Vector4((int)sceneMode, _useRadialDistance ? 1 : 0, 0, 0));
            //int pass = 0;
            //if (distanceFog && heightFog)
            //    pass = 0; // distance + height
            //else if (distanceFog)
            //    pass = 1; // distance only
            //else
            //    pass = 2; // height only
            //v4.8
            if (isForReflections)
            {
                _material.SetFloat("_invertX", 1);
            }
            else
            {
                _material.SetFloat("_invertX", 0);
            }
            if (isForReflections)
            {
                _material.SetFloat("_invertRay", -1);
            }
            else
            {
                _material.SetFloat("_invertRay", 1);
            }
            if (isForReflections)
            {
                _material.SetVector("_WorldSpaceCameraPosC", camPos);
            }
            else
            {
                _material.SetVector("_WorldSpaceCameraPosC", camPos);
            }
            _material.SetTexture("_ColorRamp", colourPalette);
            if (GradientBounds != Vector2.zero)
            {
                _material.SetFloat("_Close", GradientBounds.x);
                _material.SetFloat("_Far", GradientBounds.y);
            }
            //v4.8.3
            //_material.SetTexture("_CloudTexP", _cloudBuffer);
            //_material.SetTexture("_CloudTex", _cloudBufferP);
            //_material.SetFloat("frameFraction", frameFraction);
            //v3.5
            _material.SetTexture("_NoiseTex1", _NoiseTex1);
            _material.SetTexture("_NoiseTex2", _NoiseTex2);

            //WORLD RECONSTRUCT        
            Matrix4x4 camToWorld = cam.cameraToWorldMatrix;
            _material.SetMatrix("_InverseView", camToWorld);

            //int rtW = opaqueDesc.width;
            // int rtH = opaqueDesc.height;

            //SUN SHAFTS
            _material.SetVector("raysResolutionA", raysResolutionA);
            _material.SetColor("_rayColor", _rayColor);
            _material.SetVector("rayShadowingA", rayShadowingA);
            _material.SetColor("_underDomeColor", _underDomeColor);

            if (!downScale)
            {
                //renderMe(cam, _material, opaqueDesc, context, cmd);
            }
            else
            {
                //v2.1.15
                if (!fastest)
                {
                    if (tmpBuffer == null)
                    {
                        var format = cam.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
                        //                 tmpBuffer = RenderTexture.GetTemporary(rtW + 125, rtH + 125, 0, format);//v2.1.19 - add extra pixels to cover for frame displacement

                        RenderTexture.active = tmpBuffer;
                        GL.ClearWithSkybox(false, cam);
                        if (blendBackground)
                        { //v2.1.20
                          //v4.8
                            if (isForReflections)
                            {
                                backgroundCam.transform.rotation = cam.transform.rotation;
                                backgroundCam.transform.position = cam.transform.position;
                            }
                            else
                            {
                                backgroundCam.transform.rotation = cam.transform.rotation;
                                backgroundCam.transform.position = cam.transform.position;
                            }

                            //v4.1f                             
                            if (backgroundCam.targetTexture == null || backgroundCam.targetTexture.width != Screen.width || backgroundCam.targetTexture.height != Screen.height)
                            {
                                backgroundCam.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
                            }

                            //v4.8
                            if (isForReflections)
                            {
                                backgroundCam.transform.eulerAngles = new Vector3(backgroundCam.transform.eulerAngles.x, backgroundCam.transform.eulerAngles.y, 180);
                                backgroundCam.Render();
                                //tmpBuffer.DiscardContents();//v4.1f
                                //Graphics.Blit(backgroundCam.targetTexture, tmpBuffer);
                                //Blit(cmd, backgroundCam.targetTexture, tmpBuffer); //URP
                                Graphics.Blit(backgroundCam.targetTexture, tmpBuffer);
                            }
                            else
                            {
                                backgroundCam.Render(); //TO FIX
                                                        //Graphics.Blit(backgroundCam.targetTexture, tmpBuffer, backgroundMat);
                                                        //Blit(cmd, backgroundCam.targetTexture, tmpBuffer, backgroundMat); //URP
                                var UPsampleProperties1 = new MaterialPropertyBlock();
                                UPsampleProperties1.SetTexture("_MainTex", backgroundCam.targetTexture);
                                HDUtils.DrawFullScreen(cmd, backgroundMat, tmpBuffer, UPsampleProperties1);
                            }
                        }
                        //CustomGraphicsBlitOpt(source, destination, tmpBuffer, fogMaterial, pass, DistGradient, GradientBounds, colourPalette, texture3Dnoise1, texture3Dnoise2, cam, true, false, WebGL);
                        //CustomGraphicsBlitOpt(source, destination, tmpBuffer, fogMaterial, pass, DistGradient, GradientBounds, colourPalette, texture3Dnoise1, texture3Dnoise2, cam, false, false, WebGL);
                        // Debug.Log("tmpBuffer null");
                        CustomGraphicsBlitOpt(source, destination, tmpBuffer, _material, 6, DistGradient, GradientBounds, colourPalette, cam, true, false, WebGL, cmd);
                        CustomGraphicsBlitOpt(source, destination, tmpBuffer, _material, 6, DistGradient, GradientBounds, colourPalette, cam, false, false, WebGL, cmd);
                    }
                    else
                    {
                        //v2.1.19
                        if (toggleCounter != 0 && Application.isPlaying && splitPerFrames > 0)
                        {
                            //Debug.Log("tmpBuffer not null 1");
                            //v4.8.3
                            //if (splitPerFrames > 0 && toggleCounter != splitPerFrames) {
                            if (enableReproject)
                            {
                                int signer = 1;
                                if (WebGL)
                                {
                                    signer = -1;
                                }
                                //v4.0
                                Camera cam1 = cam;
                                if (currentCamera != null) //Camera.main != null)
                                {
                                    cam1 = currentCamera;// Camera.main;
                                }
                                Matrix4x4 scaler = cam.cameraToWorldMatrix * Matrix4x4.Scale(new Vector3(2, 1 * signer, -1)) * cam1.projectionMatrix.inverse * Matrix4x4.Scale(new Vector3(-1, 1, 1));//v2.1.19

                                float frameFraction = (float)toggleCounter / (float)splitPerFrames;
                                //Debug.Log("frameFraction"+ frameFraction + "_toggleCounter="+ toggleCounter);

                                float Xdisp = cam.transform.eulerAngles.y - prevCameraRotP.y;
                                float Ydisp = -(cam.transform.eulerAngles.x - prevCameraRotP.x);

                                if (Xdisp > 122f || Xdisp < -122f)
                                {
                                    Xdisp = 0;
                                }
                                if (Ydisp > 122f || Ydisp < -122f)
                                {
                                    Ydisp = 0;
                                }

                                scaler[0, 3] = 1 * Xdisp;//0.0075f * Xdisp * 1;//0.005f * Xdisp;//0.009f * Xdisp;
                                scaler[1, 3] = 1 * Ydisp;//0.0062f * Ydisp * 1;// 0.0062f * Ydisp * 1;//0.012f * Ydisp;
                                scaler[2, 3] = 1;// 1f - 0.005f * (Xdisp+Ydisp); // 0.95f+ 0.005f*(splitPerFrames - toggleCounter); //image scaler
                                _material.SetMatrix("_WorldClip", scaler);

                                //v4.8.3
                                _material.SetTexture("_CloudTexP", _cloudBuffer);
                                _material.SetTexture("_CloudTex", _cloudBufferP);
                                _material.SetFloat("frameFraction", frameFraction);
                                //Debug.Log(frameFraction);

                                //connector.testTex = toTexture2D(_cloudBufferP);
                                //connector.testTex2 = toTexture2D(_cloudBuffer);

                                //Blit(cmd,_cloudBufferP, _cloudBufferP1, _material, 9);// 5);//v4.8.3       //URP 
                                _material.SetTexture("_SkyTex", _cloudBufferP);
                                //Blit(cmd, _cloudBuffer, _cloudBufferP1, _material, 9);

                                Graphics.Blit(_cloudBuffer, _cloudBufferP1, _material, 9);

                                //var format = allowHDR ? RenderTextureFormat.Default : RenderTextureFormat.DefaultHDR;
                                //RenderTexture tmpBuffer1A = RenderTexture.GetTemporary(rtW, rtH, 0, format);
                                //Blit(cmd, source, tmpBuffer1A, _material, 9);

                                //connector.testTex2 = toTexture2D(_cloudBufferP1);
                            }

                            toggleCounter--;
                            //CustomGraphicsBlitOpt(source, destination, tmpBuffer, fogMaterial, pass, DistGradient, GradientBounds, colourPalette, texture3Dnoise1, texture3Dnoise2, cam, false, true, WebGL);
                            CustomGraphicsBlitOpt(source, destination, tmpBuffer, _material, 6, DistGradient, GradientBounds, colourPalette, cam, false, true, WebGL, cmd);

                        }
                        else
                        {
                            //Debug.Log("tmpBuffer not null 2");

                            toggleCounter = splitPerFrames;
                            RenderTexture.active = tmpBuffer;
                            GL.ClearWithSkybox(false, cam);
                            if (blendBackground)
                            { //v2.1.20
                              //v4.8
                                if (isForReflections)
                                {
                                    backgroundCam.transform.rotation = cam.transform.rotation;
                                    backgroundCam.transform.position = cam.transform.position;
                                }
                                else
                                {
                                    backgroundCam.transform.rotation = cam.transform.rotation;
                                    backgroundCam.transform.position = cam.transform.position;
                                }

                                //v2.1.23                               
                                //v4.1f                                
                                if (backgroundCam.targetTexture == null || backgroundCam.targetTexture.width != Screen.width || backgroundCam.targetTexture.height != Screen.height)
                                {
                                    backgroundCam.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
                                }

                                //v4.8
                                if (isForReflections)
                                {
                                    backgroundCam.transform.eulerAngles = new Vector3(backgroundCam.transform.eulerAngles.x, backgroundCam.transform.eulerAngles.y, 180);
                                    backgroundCam.Render();
                                    // tmpBuffer.DiscardContents();//v4.1f
                                    Graphics.Blit(backgroundCam.targetTexture, tmpBuffer);
                                    // Blit(cmd, backgroundCam.targetTexture, tmpBuffer);
                                }
                                else
                                {
                                    backgroundCam.Render();
                                    //  tmpBuffer.DiscardContents();//v4.1f
                                    //Graphics.Blit(backgroundCam.targetTexture, tmpBuffer, backgroundMat);
                                    // Blit(cmd, backgroundCam.targetTexture, tmpBuffer, backgroundMat);
                                    var UPsampleProperties1 = new MaterialPropertyBlock();
                                    UPsampleProperties1.SetTexture("_MainTex", backgroundCam.targetTexture);
                                    HDUtils.DrawFullScreen(cmd, backgroundMat, tmpBuffer, UPsampleProperties1);
                                }
                            }

                            //CustomGraphicsBlitOpt(source, destination, tmpBuffer, fogMaterial, pass, DistGradient, GradientBounds, colourPalette, texture3Dnoise1, texture3Dnoise2, cam, true, false, WebGL);
                            CustomGraphicsBlitOpt(source, destination, tmpBuffer, _material, 6, DistGradient, GradientBounds, colourPalette, cam, true, false, WebGL, cmd);

                            //Debug.Log("f");
                            //destination.DiscardContents();//v4.1f
                            ////tmpBuffer.DiscardContents();//v4.1f                           
                            //CustomGraphicsBlitOpt(source, destination, tmpBuffer, fogMaterial, pass, DistGradient, GradientBounds, colourPalette, texture3Dnoise1, texture3Dnoise2, cam, false, false, WebGL);
                            CustomGraphicsBlitOpt(source, destination, tmpBuffer, _material, 6, DistGradient, GradientBounds, colourPalette, cam, false, false, WebGL, cmd);

                        }
                    }
                }
                else
                {
                    //renderMe(cam, _material, opaqueDesc, context, cmd);
                    //CustomGraphicsBlitOpt(source, destination,null, fogMaterial, pass, DistGradient, GradientBounds, colourPalette, texture3Dnoise1, texture3Dnoise2, cam, true, false, WebGL);
                    //CustomGraphicsBlitOpt(source, destination, null, fogMaterial, pass, DistGradient, GradientBounds, colourPalette, texture3Dnoise1, texture3Dnoise2, cam, false, false, WebGL);
                    CustomGraphicsBlitOpt(source, destination, null, _material, 6, DistGradient, GradientBounds, colourPalette, cam, true, false, WebGL, cmd);
                    CustomGraphicsBlitOpt(source, destination, null, _material, 6, DistGradient, GradientBounds, colourPalette, cam, false, false, WebGL, cmd);
                }
            }
            ////END RENDERING
        }
        /////////////// END Render background clouds--------------------------

        /////////////////////// END VOLUME FOG SRP ///////////////////////////////// 

        ///////////////// END VOLUME CLOUDS v0.1 //////////////////////////





        ///////////////////////////////// FULL VOLUME PLANET CLOUDS v0.1.2 ///////////////////////////////////////
        //public void RenderFullVolumetricClouds(ScriptableRenderContext context, UnityEngine.Rendering.Universal.RenderingData renderingData, CommandBuffer cmd, RenderTextureDescriptor opaqueDesc)
        public void RenderFullVolumetricClouds(CommandBuffer cmd, RTHandle source, RTHandle destination, HDCamera cameraHD) //v0.4a
                                                                                                                            //public override void Render(PostProcessRenderContext context)
        {
            Material CloudMaterial = sheetSHAFTS;// blitMaterial;

            if (_cloudShapeTexture == null) // if shape texture is missing load it in
            {
                _cloudShapeTexture = TGALoader.load3DFromTGASlices(lowFreqNoise);
            }

            if (_cloudDetailTexture == null) // if detail texture is missing load it in
            {
                _cloudDetailTexture = TGALoader.load3DFromTGASlices(highFreqNoise);
            }

            //v0.4a
            Camera CurrentCamera = cameraHD.camera;//  Camera.main;// currentCamera; //v0.4a HDRP reflections
            if (CurrentCamera == null)
            {
                CurrentCamera = Camera.main;
            }
            //else
            //{
            //    if (cameraHD.camera != null)
            //    {
            //        Debug.Log(cameraHD.camera.name);
            //    }
            //}
            //if (cameraHD.camera != null)
            //{
            //    Debug.Log(cameraHD.camera.name);
            //}



            Vector3 cameraPos = CurrentCamera.transform.position;
            // sunLight.rotation.x 364 -> 339, 175 -> 201

            float sunLightFactorUpdated = sunLightFactor;
            float ambientLightFactorUpdated = ambientLightFactor;
            float sunAngle = sunLight.transform.eulerAngles.x;
            Color sunColor = highSunColor;
            float henyeyGreensteinGBackwardLerp = henyeyGreensteinGBackward;

            float noiseScale = 0.00001f + scale * 0.0004f;

            if (sunAngle > 170.0f) // change sunlight color based on sun's height.
            {
                float gradient = Mathf.Max(0.0f, (sunAngle - 330.0f) / 30.0f);
                float gradient2 = gradient * gradient;
                sunLightFactorUpdated *= gradient;
                ambientLightFactorUpdated *= gradient;
                henyeyGreensteinGBackwardLerp *= gradient2 * gradient;
                ambientLightFactorUpdated = Mathf.Max(0.02f, ambientLightFactorUpdated);
                sunColor = Color.Lerp(lowSunColor, highSunColor, gradient2);
            }

            updateMaterialKeyword(debugNoLowFreqNoise, "DEBUG_NO_LOW_FREQ_NOISE", CloudMaterial);
            updateMaterialKeyword(debugNoHighFreqNoise, "DEBUG_NO_HIGH_FREQ_NOISE", CloudMaterial);
            updateMaterialKeyword(debugNoCurlNoise, "DEBUG_NO_CURL", CloudMaterial);
            updateMaterialKeyword(allowFlyingInClouds, "ALLOW_IN_CLOUDS", CloudMaterial);
            updateMaterialKeyword(randomUnitSphere, "RANDOM_UNIT_SPHERE", CloudMaterial);
            updateMaterialKeyword(aLotMoreLightSamples, "SLOW_LIGHTING", CloudMaterial);

            switch (randomJitterNoise)
            {
                case RandomJitter.Off:
                    updateMaterialKeyword(false, "RANDOM_JITTER_WHITE", CloudMaterial);
                    updateMaterialKeyword(false, "RANDOM_JITTER_BLUE", CloudMaterial);
                    break;
                case RandomJitter.Random:
                    updateMaterialKeyword(true, "RANDOM_JITTER_WHITE", CloudMaterial);
                    updateMaterialKeyword(false, "RANDOM_JITTER_BLUE", CloudMaterial);
                    break;
                case RandomJitter.BlueNoise:
                    updateMaterialKeyword(false, "RANDOM_JITTER_WHITE", CloudMaterial);
                    updateMaterialKeyword(true, "RANDOM_JITTER_BLUE", CloudMaterial);
                    break;
            }

            // send uniforms to shader
            CloudMaterial.SetVector("_SunDir", sunLight.transform ? (-sunLight.transform.forward).normalized : Vector3.up);
            //Debug.Log("_SunDir:" + (-sunLight.transform.forward).normalized);
            CloudMaterial.SetVector("_PlanetCenter", planetZeroCoordinate - new Vector3(0, planetSize, 0));
            CloudMaterial.SetVector("_ZeroPoint", planetZeroCoordinate);
            CloudMaterial.SetColor("_SunColor", sunColor);
            //CloudMaterial.SetColor("_SunColor", sunLight.color);

            CloudMaterial.SetTexture("_WeatherTexture", WeatherTexture);
            if (maskTexture != null)
            {
                CloudMaterial.SetTexture("_maskTexture", maskTexture);//v0.1.1
            }

            //v0.5
            CloudMaterial.SetVector("YCutHeightDepthScale", YCutHeightDepthScale);
            CloudMaterial.SetFloat("extendFarPlaneAboveClouds", extendFarPlaneAboveClouds);//v0.4
            CloudMaterial.SetFloat("cameraScale", cameraScale);//v0.4

            CloudMaterial.SetVector("raysResolution", raysResolution);
            CloudMaterial.SetVector("rayShadowing", rayShadowing);

            CloudMaterial.SetColor("_CloudBaseColor", cloudBaseColor);
            CloudMaterial.SetColor("_CloudTopColor", cloudTopColor);
            CloudMaterial.SetFloat("_AmbientLightFactor", ambientLightFactorUpdated);
            CloudMaterial.SetFloat("_SunLightFactor", sunLightFactorUpdated);
            //CloudMaterial.SetFloat("_AmbientLightFactor", sunLight.intensity * ambientLightFactor * 0.3f);
            //CloudMaterial.SetFloat("_SunLightFactor", sunLight.intensity * sunLightFactor);

            CloudMaterial.SetTexture("_ShapeTexture", _cloudShapeTexture);
            CloudMaterial.SetTexture("_DetailTexture", _cloudDetailTexture);
            CloudMaterial.SetTexture("_CurlNoise", curlNoise);
            CloudMaterial.SetTexture("_BlueNoise", blueNoiseTexture);
            CloudMaterial.SetVector("_Randomness", new Vector4(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value));
            CloudMaterial.SetTexture("_AltoClouds", cloudsHighTexture);

            CloudMaterial.SetFloat("_CoverageHigh", 1.0f - coverageHigh);
            CloudMaterial.SetFloat("_CoverageHighScale", highCoverageScale * weatherScale * 0.001f);
            CloudMaterial.SetFloat("_HighCloudsScale", highCloudsScale * 0.002f);

            CloudMaterial.SetFloat("_CurlDistortAmount", 150.0f + curlDistortAmount);
            CloudMaterial.SetFloat("_CurlDistortScale", curlDistortScale * noiseScale);

            CloudMaterial.SetFloat("_LightConeRadius", lightConeRadius);
            CloudMaterial.SetFloat("_LightStepLength", lightStepLength);
            CloudMaterial.SetFloat("_SphereSize", planetSize);
            CloudMaterial.SetVector("_CloudHeightMinMax", new Vector2(startHeight, startHeight + thickness));
            CloudMaterial.SetFloat("_Thickness", thickness);
            CloudMaterial.SetFloat("_Scale", noiseScale);
            CloudMaterial.SetFloat("_DetailScale", detailScale * noiseScale);
            CloudMaterial.SetVector("_LowFreqMinMax", new Vector4(lowFreqMin, lowFreqMax));
            CloudMaterial.SetFloat("_HighFreqModifier", highFreqModifier);
            CloudMaterial.SetFloat("_WeatherScale", weatherScale * 0.00025f);

            CloudMaterial.SetFloat("_maskScale", maskScale);//v0.1.1
            CloudMaterial.SetFloat("fogOfWarRadius", fogOfWarRadius);//v0.1.1
            if (playerPos != null)
            {
                CloudMaterial.SetVector("playerPos", new Vector4(playerPos.position.x, playerPos.position.y, playerPos.position.z, fogOfWarPower));//v0.1.1
            }
            if (topDownMaskCamera != null)
            {
                CloudMaterial.SetVector("_maskPos", topDownMaskCamera.transform.position);
            }

            CloudMaterial.SetFloat("_Coverage", 1.0f - coverage);
            CloudMaterial.SetFloat("_HenyeyGreensteinGForward", henyeyGreensteinGForward);
            CloudMaterial.SetFloat("_HenyeyGreensteinGBackward", -henyeyGreensteinGBackwardLerp);
            if (adjustDensity)
            {
                CloudMaterial.SetFloat("_SampleMultiplier", cloudSampleMultiplier * stepDensityAdjustmentCurve.Evaluate(steps / 256.0f));
            }
            else
            {
                CloudMaterial.SetFloat("_SampleMultiplier", cloudSampleMultiplier);
            }

            CloudMaterial.SetFloat("_Density", density);

            CloudMaterial.SetFloat("_WindSpeed", _multipliedWindSpeed);
            CloudMaterial.SetVector("_WindDirection", _windDirectionVector);
            CloudMaterial.SetVector("_WindOffset", _windOffset);
            CloudMaterial.SetVector("_CoverageWindOffset", _coverageWindOffset);
            CloudMaterial.SetVector("_HighCloudsWindOffset", _highCloudsWindOffset);

            CloudMaterial.SetVector("_Gradient1", gradientToVector4(gradientLow));
            CloudMaterial.SetVector("_Gradient2", gradientToVector4(gradientMed));
            CloudMaterial.SetVector("_Gradient3", gradientToVector4(gradientHigh));

            CloudMaterial.SetInt("_Steps", steps);
            CloudMaterial.SetInt("_renderInFront", renderInFront);//v0.1 choose to render in front of objects for reflections

            CloudMaterial.SetMatrix("_FrustumCornersES", GetFrustumCorners(CurrentCamera)); //v0.4a
            CloudMaterial.SetMatrix("_CameraInvViewMatrix", CurrentCamera.cameraToWorldMatrix); //v0.4a
            CloudMaterial.SetVector("_CameraWS", cameraPos); //Debug.Log("cameraPos:" + cameraPos);
            CloudMaterial.SetFloat("_FarPlane", CurrentCamera.farClipPlane * 1);// 0.016f );////CurrentCamera.farClipPlane); //v0.4a

            //v0.2
            //v3.5.3			
            CloudMaterial.SetTexture("_InteractTexture", _InteractTexture);
            CloudMaterial.SetVector("_InteractTexturePos", _InteractTexturePos);
            CloudMaterial.SetVector("_InteractTextureAtr", _InteractTextureAtr);
            CloudMaterial.SetVector("_InteractTextureOffset", _InteractTextureOffset); //v4.0

            //////// SCATTER  
            CloudMaterial.SetInt("scatterOn", scatterOn);//v0.3
            CloudMaterial.SetInt("sunRaysOn", sunRaysOn);//v0.3
            CloudMaterial.SetFloat("zeroCountSteps", zeroCountSteps);//v0.3
            CloudMaterial.SetInt("sunShaftSteps", sunShaftSteps);//v0.3

            float FdotC = CurrentCamera.transform.position.y - height;
            float paramK = (FdotC <= 0.0f ? 1.0f : 0.0f);

            CloudMaterial.SetVector("_HeightParams", new Vector4(height, FdotC, paramK, heightDensity * 0.5f));
            CloudMaterial.SetVector("_DistanceParams", new Vector4(-Mathf.Max(startDistance, 0.0f), cloudDistanceParams.x, cloudDistanceParams.y, cloudDistanceParams.z));//v0.5

            CloudMaterial.SetFloat("_Scatter", _Scatter);
            CloudMaterial.SetFloat("_HGCoeff", _HGCoeff);
            CloudMaterial.SetFloat("_Extinct", _Extinct);


            CloudMaterial.SetVector("_SkyTint", _SkyTint);

            //v3.5
            CloudMaterial.SetFloat("_BackShade", _BackShade);


            //v2.1.19
            if (localLight != null)
            {
                Vector3 localLightPos = localLight.transform.position * cameraScale;//v0.4
                                                                                    //float intensity = Mathf.Pow(10, 3 + (localLightFalloff - 3) * 3);
                currentLocalLightIntensity = Mathf.Pow(10, 3 + (localLightFalloff - 3) * 3);
                //fogMaterial.SetVector ("_LocalLightPos", new Vector4 (localLightPos.x, localLightPos.y, localLightPos.z, localLight.intensity * localLightIntensity * intensity));
                //CloudMaterial.SetVector("_LocalLightPos", new Vector4(localLightPos.x, localLightPos.y, localLightPos.z, localLight.intensity * localLightIntensity * currentLocalLightIntensity));

                CloudMaterial.SetVector("_LocalLightPos", new Vector4(localLightPos.x, localLightPos.y, localLightPos.z,
                    localLight.intensity * localLightIntensityA * currentLocalLightIntensity));//v0.4

                CloudMaterial.SetVector("_LocalLightColor", new Vector4(localLight.color.r, localLight.color.g, localLight.color.b, localLightFalloff));
            }
            else
            {
                if (currentLocalLightIntensity > 0)
                {
                    currentLocalLightIntensity = 0;
                    //fogMaterial.SetVector ("_LocalLightPos", new Vector4 (localLightPos.x, localLightPos.y, localLightPos.z, localLight.intensity * localLightIntensity * intensity));
                    CloudMaterial.SetVector("_LocalLightColor", Vector4.zero);
                }
            }

            //SM v1.7
            CloudMaterial.SetFloat("luminance", luminance);
            CloudMaterial.SetFloat("lumFac", lumFac);
            CloudMaterial.SetFloat("Multiplier1", ScatterFac);
            CloudMaterial.SetFloat("Multiplier2", TurbFac);
            CloudMaterial.SetFloat("Multiplier3", HorizFac);
            CloudMaterial.SetFloat("turbidity", turbidity);
            CloudMaterial.SetFloat("reileigh", reileigh);
            CloudMaterial.SetFloat("mieCoefficient", mieCoefficient);
            CloudMaterial.SetFloat("mieDirectionalG", mieDirectionalG);
            CloudMaterial.SetFloat("bias", bias);
            CloudMaterial.SetFloat("contrast", contrast);
            CloudMaterial.SetVector("v3LightDir", -sunLight.transform.forward); //Debug.Log("v3LightDir:" + sunLight.transform.forward);
                                                                                //CloudMaterial.SetVector("_TintColor", new Vector4(TintColor.x, TintColor.y, TintColor.z, 1));//68, 155, 345
            CloudMaterial.SetVector("_TintColor", new Vector4(TintColor.r, TintColor.g, TintColor.b, 1));//68, 155, 345

            float Foggy = 0;
            if (FogSky)
            {
                Foggy = 1;
            }
            CloudMaterial.SetFloat("FogSky", Foggy);
            CloudMaterial.SetFloat("ClearSkyFac", ClearSkyFac);

            var sceneMode = RenderSettings.fogMode;
            var sceneDensity = 0.01f; //RenderSettings.fogDensity;//v3.0
            var sceneStart = RenderSettings.fogStartDistance;
            var sceneEnd = RenderSettings.fogEndDistance;
            Vector4 sceneParams;
            bool linear = (sceneMode == FogMode.Linear);
            float diff = linear ? sceneEnd - sceneStart : 0.0f;
            float invDiff = Mathf.Abs(diff) > 0.0001f ? 1.0f / diff : 0.0f;
            sceneParams.x = sceneDensity * 1.2011224087f; // density / sqrt(ln(2)), used by Exp2 fog mode
            sceneParams.y = sceneDensity * 1.4426950408f; // density / ln(2), used by Exp fog mode
            sceneParams.z = linear ? -invDiff : 0.0f;
            sceneParams.w = linear ? sceneEnd * invDiff : 0.0f;
            CloudMaterial.SetVector("_SceneFogParams", sceneParams);
            CloudMaterial.SetVector("_SceneFogMode", new Vector4((int)sceneMode, useRadialDistance ? 1 : 0, 0, 0));


            ////////// END SCATTER

            //v0.6
            CloudMaterial.SetFloat("controlBackAlphaPower", controlBackAlphaPower);
            CloudMaterial.SetFloat("controlCloudAlphaPower", controlCloudAlphaPower);
            CloudMaterial.SetVector("controlCloudEdgeA", controlCloudEdgeA);
            CloudMaterial.SetFloat("controlCloudEdgeOffset", controlCloudEdgeOffset);
            CloudMaterial.SetFloat("depthDilation", depthDilation);
            CloudMaterial.SetFloat("_TemporalResponse", TemporalResponse);
            CloudMaterial.SetFloat("_TemporalGain", TemporalGain);

            //v0.7 - Vortex and supercell
            if (enableVortex)
            {
                CloudMaterial.SetVector("vortexPosRadius", vortexPosRadius);
                CloudMaterial.SetVector("vortexControlsA", vortexControlsA);
                CloudMaterial.SetVector("superCellPosRadius", superCellPosRadius);
                CloudMaterial.SetVector("superCellControlsA", superCellControlsA);
            }

            // get cloud render texture and render clouds to it

            int rtW = source.rt.width;// opaqueDesc.width;
            int rtH = source.rt.height;// opaqueDesc.height;

            var format = CurrentCamera.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
            //   //  //  //    RenderTexture tmpBuffer1 = RenderTexture.GetTemporary(rtW, rtH, 0, format);


            //var UPsampleProperties04 = new MaterialPropertyBlock();
            //UPsampleProperties04.SetTexture("_MainTexA", lrDepthBuffer);
            //HDUtils.DrawFullScreen(cmd, sheetSHAFTS, lrColorA, UPsampleProperties04, 1);
            //Blit(cmd, source, tmpBuffer1);


            // RenderTexture rtClouds = RenderTexture.GetTemporary((int)(rtW / ((float)downSample)), (int)(rtH / ((float)downSample)), 0, 
            //     source.format, RenderTextureReadWrite.Default, source.antiAliasing);

            //       RenderTexture rtClouds = RenderTexture.GetTemporary((int)(rtW / ((float)downSample)), (int)(rtH / ((float)downSample)), 0, format);
            ////         RenderTexture rtClouds = RenderTexture.GetTemporary((int)(source.width / ((float)downSample)), (int)(source.height / ((float)downSample)), 0, source.format, RenderTextureReadWrite.Default, source.antiAliasing);
            ///       CustomGraphicsBlit(source, rtClouds, CloudMaterial, 0);
            //CustomGraphicsBlit(null, rtClouds, CloudMaterial, 10);
            //CloudMaterial.SetTexture("_MainTex", tmpBuffer1);
            //CloudMaterial.SetTexture("_Clouds", tmpBuffer1);

            //CloudMaterial.SetTexture("_MainTex", tmpBuffer1);
            //Blit(cmd, destination.id, rtClouds, CloudMaterial, 10);

            var UPsampleProperties04 = new MaterialPropertyBlock();
            UPsampleProperties04.SetTexture("_MainTexB", source);
            HDUtils.DrawFullScreen(cmd, CloudMaterial, tmpBuffer2, UPsampleProperties04, 13); //was pass 10

            //Debug.Log("W="+tmpBuffer2.rt.width + ", H="+ tmpBuffer2.rt.width);

            //if (temporalAntiAliasing) // if TAA is enabled, then apply it to cloud render texture
            //{
            //    RenderTexture rtTemporal = RenderTexture.GetTemporary(rtClouds.width, rtClouds.height, 0, rtClouds.format, RenderTextureReadWrite.Default, source.antiAliasing);
            //    _temporalAntiAliasing.TemporalAntiAliasing(rtClouds, rtTemporal);
            //    UpscaleMaterial.SetTexture("_Clouds", rtTemporal);
            //    RenderTexture.ReleaseTemporary(rtTemporal);
            //}
            //else
            //{
            /////        ////////UpscaleMaterial.SetTexture("_Clouds", rtClouds);
            //CloudMaterial.SetTexture("_Clouds", rtClouds);
            //}
            // Apply clouds to background
            //Graphics.Blit(source, destination, UpscaleMaterial, 0);
            ////        Graphics.Blit(source, destination, CloudMaterial, 11);
            //       Blit(cmd, backgroundCam.targetTexture, tmpBuffer, CloudMaterial, 11);

            //Debug.Log(destination.id);

            //      CloudMaterial.SetTexture("_MainTex", tmpBuffer1);
            //       CloudMaterial.SetTexture("_CloudTex", rtClouds);
            // Blit(cmd, tmpBuffer2, source);
            // Blit(cmd, source, destination.Identifier(), CloudMaterial, 11);
            ////      Blit(cmd, tmpBuffer1, source, CloudMaterial, 11);
            //      context.ExecuteCommandBuffer(cmd);


            //v0.6
            // if (previousFrameTexture != null) { previousFrameTexture.Release(); }
            // if (previousDepthTexture != null) { previousDepthTexture.Release(); }
            //if (previousFrameTexture == null)
            //{
            //    previousFrameTexture = new RenderTexture((int)(rtW / ((float)downSample)), (int)(rtH / ((float)downSample)), 0, RenderTextureFormat.DefaultHDR);
            //    previousFrameTexture.filterMode = FilterMode.Point;
            //    previousFrameTexture.Create();
            //}
            //if (previousDepthTexture == null)
            //{
            //    previousDepthTexture = new RenderTexture((int)(rtW / ((float)downSample)), (int)(rtH / ((float)downSample)), 0, RenderTextureFormat.RFloat);
            //    previousDepthTexture.filterMode = FilterMode.Point;
            //    previousDepthTexture.Create();
            //}
            //////RenderTexture tmpBuffer3 = RenderTexture.GetTemporary((int)(rtW / ((float)downSample)), (int)(rtH / ((float)downSample)), 0, format);
            if (enabledTemporalAA && Time.fixedTime > 0.05f)
            {
                //Debug.Log("AA Enabled");
                var UPsampleProperties01 = new MaterialPropertyBlock();
                var worldToCameraMatrix = Camera.main.worldToCameraMatrix;
                var projectionMatrix = GL.GetGPUProjectionMatrix(Camera.main.projectionMatrix, false);
                UPsampleProperties01.SetMatrix("_InverseProjectionMatrix", projectionMatrix.inverse);
                viewProjectionMatrix = projectionMatrix * worldToCameraMatrix;
                UPsampleProperties01.SetMatrix("_InverseViewProjectionMatrix", viewProjectionMatrix.inverse);
                UPsampleProperties01.SetMatrix("_LastFrameViewProjectionMatrix", lastFrameViewProjectionMatrix);
                UPsampleProperties01.SetMatrix("_LastFrameInverseViewProjectionMatrix", lastFrameInverseViewProjectionMatrix);
                UPsampleProperties01.SetTexture("_CloudTex", tmpBuffer2);//CloudMaterial.SetTexture("_CloudTex", rtClouds);
                UPsampleProperties01.SetTexture("_PreviousColor", previousFrameTexture);
                UPsampleProperties01.SetTexture("_PreviousDepth", previousDepthTexture);

                cmd.SetRenderTarget(tmpBuffer3);
                if (mesh == null)
                {
                    Awake();
                }
                //HDUtils.DrawFullScreen(cmd, CloudMaterial, destination, UPsampleProperties05, 14);
                //cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, CloudMaterial, 0, 12);// (int)RenderPass.TemporalReproj);
                HDUtils.DrawFullScreen(cmd, CloudMaterial, tmpBuffer3, UPsampleProperties01, 16);

                //cmd.blit(context.source, context.destination, _material, 0);
                //cmd.Blit(tmpBuffer3, previousFrameTexture);
                //cmd.Blit(tmpBuffer3, tmpBuffer2);
                HDUtils.BlitCameraTexture(cmd, tmpBuffer3, previousFrameTexture);
                HDUtils.BlitCameraTexture(cmd, tmpBuffer3, tmpBuffer2);

                cmd.SetRenderTarget(previousDepthTexture);

                //HDUtils.DrawFullScreen(cmd, CloudMaterial, destination, UPsampleProperties05, 14);
                //cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, CloudMaterial, 0, 13);//(int)RenderPass.GetDepth);
                HDUtils.DrawFullScreen(cmd, CloudMaterial, previousDepthTexture, UPsampleProperties01, 17);

                //DEBUG TAA
                //cmd.Blit(previousDepthTexture, rtClouds);
                //Blit(cmd, previousDepthTexture, source); //Blit(cmd, rtClouds, source);
                //context.ExecuteCommandBuffer(cmd);
                //CommandBufferPool.Release(cmd);
                //RenderTexture.ReleaseTemporary(rtClouds);
                //RenderTexture.ReleaseTemporary(tmpBuffer1);
                //return;
                //END DEBUG TAA
            }



            var UPsampleProperties05 = new MaterialPropertyBlock();
            UPsampleProperties05.SetTexture("_MainTexB", source);
            UPsampleProperties05.SetTexture("_CloudTex", tmpBuffer2);
            //HDUtils.DrawFullScreen(cmd, CloudMaterial, destination, UPsampleProperties05, 14);

            connectSuntoNebulaCloudsHDRP connector = Camera.main.GetComponent<connectSuntoNebulaCloudsHDRP>();
            if (connector != null)
            {
                passParamsToClouds(connector);
                //Debug.Log("Found connector");
            }

            //TEMPORAL
            if (temporalAntiAliasing)
            {
                HDUtils.DrawFullScreen(cmd, CloudMaterial, tmpBuffer1, UPsampleProperties05, 14);
                //colorTextureIdentifier  ("_CameraColorTexture"); = tmpBuffer1

                //EnsureArray(ref historyBuffer, 2);
                //EnsureRenderTarget(ref historyBuffer[0], tmpBuffer1.rt.width, tmpBuffer1.rt.height, tmpBuffer1.rt.format, FilterMode.Bilinear);
                //EnsureRenderTarget(ref historyBuffer[1], tmpBuffer1.rt.width, tmpBuffer1.rt.height, tmpBuffer1.rt.format, FilterMode.Bilinear);


                int indexRead = indexWrite;
                indexWrite = (++indexWrite) % 2;

                //Debug.Log("spread.value" + spread);
                //Debug.Log("feedback.value" + feedback);

                //set vars
                Camera camera = CurrentCamera;// Camera.main;// renderingData.cameraData.camera; //v0.4a
                Vector2 additionalSample = GenerateRandomOffset() * spread;
                m_TaaData = new TAAData();
                m_TaaData.sampleOffset = additionalSample;
                m_TaaData.porjPreview = previewProj;
                m_TaaData.viewPreview = previewView;
                m_TaaData.projOverride = camera.orthographic
                           ? GetJitteredOrthographicProjectionMatrix(camera, m_TaaData.sampleOffset)
                           : GetJitteredPerspectiveProjectionMatrix(camera, m_TaaData.sampleOffset);
                m_TaaData.sampleOffset = new Vector2(m_TaaData.sampleOffset.x / camera.scaledPixelWidth, m_TaaData.sampleOffset.y / camera.scaledPixelHeight);
                previewView = camera.worldToCameraMatrix;
                previewProj = camera.projectionMatrix;

                var UPsampleProperties06 = new MaterialPropertyBlock();
                Matrix4x4 inv_p_jitterd = Matrix4x4.Inverse(m_TaaData.projOverride);
                Matrix4x4 inv_v_jitterd = Matrix4x4.Inverse(CurrentCamera.worldToCameraMatrix); //v0.4a
                Matrix4x4 previous_vp = m_TaaData.porjPreview * m_TaaData.viewPreview;
                UPsampleProperties06.SetMatrix(ShaderConstants._TAA_CurInvView, inv_v_jitterd);
                UPsampleProperties06.SetMatrix(ShaderConstants._TAA_CurInvProj, inv_p_jitterd);
                UPsampleProperties06.SetMatrix(ShaderConstants._TAA_PrevViewProjM, previous_vp);
                UPsampleProperties06.SetVector(ShaderConstants._TAA_Params, new Vector3(m_TaaData.sampleOffset.x, m_TaaData.sampleOffset.y, feedback));
                // CloudMaterial.SetTexture(ShaderConstants._TAA_pre_texture, historyBuffer[indexRead]);
                if (indexRead == 0)
                {
                    UPsampleProperties06.SetTexture(ShaderConstants._TAA_pre_texture, tmpBufferAA);
                }
                if (indexRead == 1)
                {
                    UPsampleProperties06.SetTexture(ShaderConstants._TAA_pre_texture, tmpBufferAA2);
                }
                CoreUtils.SetKeyword(cmd, ShaderKeywordStrings.HighTAAQuality, true);
                CoreUtils.SetKeyword(cmd, ShaderKeywordStrings.MiddleTAAQuality, false);
                CoreUtils.SetKeyword(cmd, ShaderKeywordStrings.LOWTAAQuality, false);
                //cmd.Blit(tmpBuffer1, historyBuffer[indexWrite], m_Material);
                //cmd.Blit(historyBuffer[indexWrite], destination);
                if (indexRead == 0)
                {
                    UPsampleProperties06.SetTexture("_MainTexB", tmpBuffer1);
                    HDUtils.DrawFullScreen(cmd, CloudMaterial, tmpBufferAA2, UPsampleProperties06, 15);
                    //cmd.Blit(historyBuffer[indexWrite], destination);
                    UPsampleProperties06.SetTexture("_MainTexB", tmpBufferAA2);
                    UPsampleProperties06.SetTexture("_CloudTex", tmpBufferAA2);
                    HDUtils.DrawFullScreen(cmd, CloudMaterial, destination, UPsampleProperties06, 14);
                }
                if (indexRead == 1)
                {
                    //var UPsampleProperties06 = new MaterialPropertyBlock();
                    UPsampleProperties06.SetTexture("_MainTexB", tmpBuffer1);
                    HDUtils.DrawFullScreen(cmd, CloudMaterial, tmpBufferAA, UPsampleProperties06, 15);
                    //cmd.Blit(historyBuffer[indexWrite], destination);
                    UPsampleProperties06.SetTexture("_MainTexB", tmpBufferAA);
                    UPsampleProperties06.SetTexture("_CloudTex", tmpBufferAA);
                    HDUtils.DrawFullScreen(cmd, CloudMaterial, destination, UPsampleProperties06, 14);
                }
            }
            else
            {
                HDUtils.DrawFullScreen(cmd, CloudMaterial, destination, UPsampleProperties05, 14);
            }


            //CommandBufferPool.Release(cmd);

            //RenderTexture.ReleaseTemporary(rtClouds);
            // // //RenderTexture.ReleaseTemporary(tmpBuffer1);

            //v0.6
            lastFrameViewProjectionMatrix = viewProjectionMatrix;
            lastFrameInverseViewProjectionMatrix = viewProjectionMatrix.inverse;
        }

        //v0.6
        Matrix4x4 lastFrameViewProjectionMatrix;
        Matrix4x4 viewProjectionMatrix;
        Matrix4x4 lastFrameInverseViewProjectionMatrix;
        protected override void OnDestroy()
        {
            //if (previousFrameTexture != null)
            //{
            //    previousFrameTexture.Release();
            //    previousFrameTexture = null;
            //}

            //if (previousDepthTexture != null)
            //{
            //    previousDepthTexture.Release();
            //    previousDepthTexture = null;
            //}

            Cleanup();
        }
        protected override void OnDisable()
        {
            //if (previousFrameTexture != null)
            //{
            //    previousFrameTexture.Release();
            //    previousFrameTexture = null;
            //}

            //if (previousDepthTexture != null)
            //{
            //    previousDepthTexture.Release();
            //    previousDepthTexture = null;
            //}

            Cleanup();
        }
        //RenderTexture previousFrameTexture;
        //RenderTexture previousDepthTexture;
        Mesh mesh;
        void Awake()
        {
            mesh = new Mesh();
            mesh.vertices = new Vector3[]
            {
            new Vector3(-1, -1, 1),
            new Vector3(-1, 1, 1),
            new Vector3(1, 1, 1),
            new Vector3(1, -1, 1)
            };
            mesh.uv = new Vector2[]
            {
            new Vector2(0, 1),
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1)
            };
            mesh.SetIndices(new int[] { 0, 1, 2, 3 }, MeshTopology.Quads, 0);
        }

        //////// TEMPORAL AA /////////-------------------------------
        // [Tooltip("The quality of AntiAliasing")]
        // public MotionBlurQualityParameter quality = new MotionBlurQualityParameter(MotionBlurQuality.Low);
        [Tooltip("Sampling Distance")]
        //public ClampedFloatParameter spread = new ClampedFloatParameter(1.0f, 0f, 11f);
        public float spread = 0;
        [Tooltip("Feedback")]
        //public ClampedFloatParameter feedback = new ClampedFloatParameter(0.0f, 0f, 11f);
        public float feedback = 1;
        internal static class ShaderKeywordStrings
        {
            internal static readonly string HighTAAQuality = "_HIGH_TAA";
            internal static readonly string MiddleTAAQuality = "_MIDDLE_TAA";
            internal static readonly string LOWTAAQuality = "_LOW_TAA";
        }
        Matrix4x4 previewView;
        Matrix4x4 previewProj;
        internal static class ShaderConstants
        {
            public static readonly int _TAA_Params = Shader.PropertyToID("_TAA_Params");
            public static readonly int _TAA_pre_texture = Shader.PropertyToID("_TAA_Pretexture");
            public static readonly int _TAA_pre_vp = Shader.PropertyToID("_TAA_Pretexture");
            public static readonly int _TAA_PrevViewProjM = Shader.PropertyToID("_PrevViewProjM_TAA");
            public static readonly int _TAA_CurInvView = Shader.PropertyToID("_I_V_Current_jittered");
            public static readonly int _TAA_CurInvProj = Shader.PropertyToID("_I_P_Current_jittered");
        }
        RenderTexture[] historyBuffer;
        static int indexWrite = 0;
        TAAData m_TaaData;
        internal enum AntialiasingQuality
        {
            Low,
            Medium,
            High
        }
        internal sealed class TAAData
        {
            #region Fields
            internal Vector2 sampleOffset;
            internal Matrix4x4 projOverride;
            internal Matrix4x4 porjPreview;
            internal Matrix4x4 viewPreview;
            #endregion
            #region Constructors
            internal TAAData()
            {
                projOverride = Matrix4x4.identity;
                porjPreview = Matrix4x4.identity;
                viewPreview = Matrix4x4.identity;
            }
            #endregion
        }
        void EnsureArray<T>(ref T[] array, int size, T initialValue = default(T))
        {
            if (array == null || array.Length != size)
            {
                array = new T[size];
                for (int i = 0; i != size; i++)
                    array[i] = initialValue;
            }
        }
        bool EnsureRenderTarget(ref RenderTexture rt, int width, int height, RenderTextureFormat format, FilterMode filterMode, int depthBits = 0, int antiAliasing = 1)
        {
            if (rt != null && (rt.width != width || rt.height != height || rt.format != format || rt.filterMode != filterMode || rt.antiAliasing != antiAliasing))
            {
                RenderTexture.ReleaseTemporary(rt);
                rt = null;
            }
            if (rt == null)
            {
                rt = RenderTexture.GetTemporary(width, height, depthBits, format, RenderTextureReadWrite.Default, antiAliasing);
                rt.filterMode = filterMode;
                rt.wrapMode = TextureWrapMode.Clamp;
                return true;// new target
            }
            return false;// same target
        }
        const int k_SampleCount = 8;
        public static int sampleIndex { get; private set; }
        public static Vector2 GenerateRandomOffset()
        {
            // The variance between 0 and the actual halton sequence values reveals noticeable instability
            // in Unity's shadow maps, so we avoid index 0.
            var offset = new Vector2(
                    GetHaltonSeq((sampleIndex & 1023) + 1, 2) - 0.5f,
                    GetHaltonSeq((sampleIndex & 1023) + 1, 3) - 0.5f
                );

            if (++sampleIndex >= k_SampleCount)
                sampleIndex = 0;

            return offset;
        }
        public static float GetHaltonSeq(int index, int radix)
        {
            float result = 0f;
            float fraction = 1f / (float)radix;

            while (index > 0)
            {
                result += (float)(index % radix) * fraction;

                index /= radix;
                fraction /= (float)radix;
            }

            return result;
        }
        /// <summary>
        /// Gets a jittered orthographic projection matrix for a given camera.
        /// </summary>
        /// <param name="camera">The camera to build the orthographic matrix for</param>
        /// <param name="offset">The jitter offset</param>
        /// <returns>A jittered projection matrix</returns>
        public static Matrix4x4 GetJitteredOrthographicProjectionMatrix(Camera camera, Vector2 offset)
        {
            float vertical = camera.orthographicSize;
            float horizontal = vertical * camera.aspect;

            offset.x *= horizontal / (0.5f * camera.pixelWidth);
            offset.y *= vertical / (0.5f * camera.pixelHeight);
            float left = offset.x - horizontal;
            float right = offset.x + horizontal;
            float top = offset.y + vertical;
            float bottom = offset.y - vertical;

            return Matrix4x4.Ortho(left, right, bottom, top, camera.nearClipPlane, camera.farClipPlane);
        }
        /// <summary>
        /// Gets a jittered perspective projection matrix for a given camera.
        /// </summary>
        /// <param name="camera">The camera to build the projection matrix for</param>
        /// <param name="offset">The jitter offset</param>
        /// <returns>A jittered projection matrix</returns>
        public static Matrix4x4 GetJitteredPerspectiveProjectionMatrix(Camera camera, Vector2 offset)
        {
            float near = camera.nearClipPlane;
            float far = camera.farClipPlane;

            float vertical = Mathf.Tan(0.5f * Mathf.Deg2Rad * camera.fieldOfView) * near;
            float horizontal = vertical * camera.aspect;

            offset.x *= horizontal / (0.5f * camera.pixelWidth);
            offset.y *= vertical / (0.5f * camera.pixelHeight);

            var matrix = camera.projectionMatrix;

            matrix[0, 2] += offset.x / horizontal;
            matrix[1, 2] += offset.y / vertical;

            return matrix;
        }
        //////// END TEMPORAL AA /////////------------------------

        private void updateMaterialKeyword(bool b, string keyword, Material CloudMaterial)
        {
            if (b != CloudMaterial.IsKeywordEnabled(keyword))
            {
                if (b)
                {
                    CloudMaterial.EnableKeyword(keyword);
                }
                else
                {
                    CloudMaterial.DisableKeyword(keyword);
                }
            }
        }
        /// \brief Stores the normalized rays representing the camera frustum in a 4x4 matrix.  Each row is a vector.
        /// 
        /// The following rays are stored in each row (in eyespace, not worldspace):
        /// Top Left corner:     row=0
        /// Top Right corner:    row=1
        /// Bottom Right corner: row=2
        /// Bottom Left corner:  row=3
        private Matrix4x4 GetFrustumCorners(Camera cam)
        {
            float camFov = cam.fieldOfView;
            float camAspect = cam.aspect;

            Matrix4x4 frustumCorners = Matrix4x4.identity;

            float fovWHalf = camFov * 0.5f;

            float tan_fov = Mathf.Tan(fovWHalf * Mathf.Deg2Rad);

            Vector3 toRight = Vector3.right * tan_fov * camAspect;
            Vector3 toTop = Vector3.up * tan_fov;

            Vector3 topLeft = (-Vector3.forward - toRight + toTop);
            Vector3 topRight = (-Vector3.forward + toRight + toTop);
            Vector3 bottomRight = (-Vector3.forward + toRight - toTop);
            Vector3 bottomLeft = (-Vector3.forward - toRight - toTop);

            frustumCorners.SetRow(0, topLeft);
            frustumCorners.SetRow(1, topRight);
            frustumCorners.SetRow(2, bottomRight);
            frustumCorners.SetRow(3, bottomLeft);

            return frustumCorners;
        }

        ///////////////////////////////// END FULL VOLUME PLANET CLOUDS v0.1.2 ///////////////////////////////////////
    }
}