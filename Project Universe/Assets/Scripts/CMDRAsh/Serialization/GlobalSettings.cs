using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUniverse.Serialization
{
    [Serializable]
    public static class GlobalSettings
    {
        /// 
        /// Gameplay
        /// 

        /// 
        /// Video
        /// 
        //General
        [SerializeField] private static string _screenResolution;
        [SerializeField] private static string _graphicsGeneral;
        [SerializeField] private static string _windowMode;
        [SerializeField] private static float _brightness;
        [SerializeField] private static float _gama;
        [SerializeField] private static float _minExposure;
        [SerializeField] private static float _maxExposure;
        [SerializeField] private static float _hudBorderX;
        [SerializeField] private static float _hudBorderY;
        [SerializeField] private static float _hudScale;
        [SerializeField] private static int _fov;
        [SerializeField] private static bool _vsync;
        [SerializeField] private static string _fxaa;
        [SerializeField] private static string _antiAliasing;
        // Video - Detail
        [SerializeField] private static string _textureQuality;
        [SerializeField] private static string _gameQuality;
        [SerializeField] private static float _maxDecals;
        [SerializeField] private static float _maxDistance;
        [SerializeField] private static float _clutterDistance;
        [SerializeField] private static float _viewDistance;
        // Video - AO/Shadows
        [SerializeField] private static bool _enableAO;
        [SerializeField] private static float _aoIntensity;//
        [SerializeField] private static float _aoDirectLight;//
        [SerializeField] private static float _aoRadius;
        [SerializeField] private static string _aoQuality;
        [SerializeField] private static bool _aoFullRes;
        [SerializeField] private static int _aoSteps;
        [SerializeField] private static bool _aoTemporalAccumulation;
        [SerializeField] private static bool _microShadows;
        [SerializeField] private static string _shadowsResolution;
        [SerializeField] private static float _maxShadowsDistance;
        [SerializeField] private static int _maxShadowCount;
        // Video - DOF
        [SerializeField] private static string _dofQuality;
        [SerializeField] private static int _dofNearSamples;
        [SerializeField] private static int _dofNearRadius;
        [SerializeField] private static int _dofFarSamples;
        [SerializeField] private static int _dofFarRadius;
        // Video - SSR/Bloom
        [SerializeField] private static bool _enableSSR;
        [SerializeField] private static string _ssrQuality;
        [SerializeField] private static float _ssrEdgeFadeDistance;
        [SerializeField] private static float _ssrObjectThickness;
        [SerializeField] private static int _ssrMaxRaySteps;
        [SerializeField] private static string _ssrProbeResolution;
        [SerializeField] private static int _ssrProbeCache;
        [SerializeField] private static bool _enableBloom;
        [SerializeField] private static string _bloomQuality;
        [SerializeField] private static float _bloomThreshold;
        [SerializeField] private static float _bloomIntensity;
        [SerializeField] private static float _bloomScatter;
        // Video - SSGI
        [SerializeField] private static bool _enableSSGI;
        [SerializeField] private static string _ssgiQuality;
        [SerializeField] private static bool _ssgiFullRes;
        [SerializeField] private static int _ssgiRaySteps;
        [SerializeField] private static int _ssgiFilterRadius;
        // Video - Fog
        [SerializeField] private static bool _enableVolumetricFog;
        [SerializeField] private static string _vfDenoisingMode;
        [SerializeField] private static int _vfVolumetricDistance;
        // Video - RayTracing

        /// 
        /// Audio
        /// 

        /// 
        /// Controls
        /// 

        /// 
        /// Secret
        /// 

        ///Video - General
        public static string ScreenResolution
        {
            get { return _screenResolution; }
            set { _screenResolution = value; }
        }
        public static string GraphicsGeneral
        {
            get { return _graphicsGeneral; }
            set { _graphicsGeneral = value; }
        }
        public static string WindowMode
        {
            get { return _windowMode; }
            set { _windowMode = value; }
        }
        public static float Brightness
        {
            get { return _brightness; }
            set { _brightness = value; }
        }
        public static float Gama
        {
            get { return _gama; }
            set { _gama = value; }
        }
        public static float MinExposure
        {
            get { return _minExposure; }
            set { _minExposure = value; }
        }
        public static float MaxExposure
        {
            get { return _maxExposure; }
            set { _maxExposure = value; }
        }
        public static float HUDBorderX
        {
            get { return _hudBorderX; }
            set { _hudBorderX = value; }
        }
        public static float HUDBorderY
        {
            get { return _hudBorderY; }
            set { _hudBorderY = value; }
        }
        public static float HUDScale
        {
            get { return _hudScale; }
            set { _hudScale = value; }
        }
        public static int FOV
        {
            get { return _fov; }
            set { _fov = value; }
        }
        public static bool VSync
        {
            get { return _vsync; }
            set { _vsync = value; }
        }
        public static string FXAA
        {
            get { return _fxaa; }
            set { _fxaa = value; }
        }
        public static string AntiAliasing
        {
            get { return _antiAliasing; }
            set { _antiAliasing = value; }
        }
        /// Video - Detail
        public static string TextureQuality
        {
            get { return _textureQuality; }
            set { _textureQuality = value; }
        }
        public static string GameQuality
        {
            get { return _gameQuality; }
            set { _gameQuality = value; }
        }
        public static float MaxDecals
        {
            get { return _maxDecals; }
            set { _maxDecals = value; }
        }
        public static float MaxDistance
        {
            get { return _maxDistance; }
            set { _maxDistance = value; }
        }
        public static float ClutterDistance
        {
            get { return _clutterDistance; }
            set { _clutterDistance = value; }
        }
        public static float ViewDistance
        {
            get { return _viewDistance; }
            set { _viewDistance = value; }
        }
        /// Video - AO/Shadows
        public static bool EnableAO
        {
            get { return _enableAO; }
            set { _enableAO = value; }
        }
        public static float AOIntensity
        {
            get { return _aoIntensity; }
            set { _aoIntensity = value; }
        }
        public static float AODirectLight
        {
            get { return _aoDirectLight; }
            set { _aoDirectLight = value; }
        }
        public static float AORadius
        {
            get { return _aoRadius; }
            set { _aoRadius = value; }
        }
        public static string AOQuality
        {
            get { return _aoQuality; }
            set { _aoQuality = value; }
        }
        public static bool AOFullRes
        {
            get { return _aoFullRes; }
            set { _aoFullRes = value; }
        }
        public static int AOSteps
        {
            get { return _aoSteps; }
            set { _aoSteps = value; }
        }
        public static bool AOTemporalAccumulation
        {
            get { return _aoTemporalAccumulation; }
            set { _aoTemporalAccumulation = value; }
        }
        public static bool MicroShadows
        {
            get { return _microShadows; }
            set { _microShadows = value; }
        }
        public static string ShadowsResolution
        {
            get { return _shadowsResolution; }
            set { _shadowsResolution = value; }
        }
        public static float MaxShadowDistance
        {
            get { return _maxShadowsDistance; }
            set { _maxShadowsDistance = value; }
        }
        public static int MaxShadowCount
        {
            get { return _maxShadowCount; }
            set { _maxShadowCount = value; }
        }
        /// Video - DOF
        public static string DOFQuality
        {
            get { return _dofQuality; }
            set { _dofQuality = value; }
        }
        public static int DOFNearSamples
        {
            get { return _dofNearSamples; }
            set { _dofNearSamples = value; }
        }
        public static int DOFNearRadius
        {
            get { return _dofNearRadius; }
            set { _dofNearRadius = value; }
        }
        public static int DOFFarSamples
        {
            get { return _dofFarSamples; }
            set { _dofFarSamples = value; }
        }
        public static int DOFFarRadius
        {
            get { return _dofFarRadius; }
            set { _dofFarRadius = value; }
        }
        /// Video - SSR/Bloom
        public static bool EnableSSR
        {
            get { return _enableSSR; }
            set { _enableSSR = value; }
        }
        public static string SSRQuality
        {
            get { return _ssrQuality; }
            set { _ssrQuality = value; }
        }
        public static float SSREdgeFadeDistance
        {
            get { return _ssrEdgeFadeDistance; }
            set { _ssrEdgeFadeDistance = value; }
        }
        public static float SSRObjectThickness
        {
            get { return _ssrObjectThickness; }
            set { _ssrObjectThickness = value; }
        }
        public static int SSRMaxRaySteps
        {
            get { return _ssrMaxRaySteps; }
            set { _ssrMaxRaySteps = value; }
        }
        public static string SSRProbeResolution
        {
            get { return _ssrProbeResolution; }
            set { _ssrProbeResolution = value; }
        }
        public static int SSRProbeCache
        {
            get { return _ssrProbeCache; }
            set { _ssrProbeCache = value; }
        }
        public static bool EnableBloom
        {
            get { return _enableBloom; }
            set { _enableBloom = value; }
        }
        public static string BloomQuality
        {
            get { return _bloomQuality; }
            set { _bloomQuality = value; }
        }
        public static float BloomThreshold
        {
            get { return _bloomThreshold; }
            set { _bloomThreshold = value; }
        }
        public static float BloomIntensity
        {
            get { return _bloomIntensity; }
            set { _bloomIntensity = value; }
        }
        public static float BloomScatter
        {
            get { return _bloomScatter; }
            set { _bloomScatter = value; }
        }
        /// Video - SSGI
        public static bool EnableSSGI
        {
            get { return _enableSSGI; }
            set { _enableSSGI = value; }
        }
        public static string SSGIQuality
        {
            get { return _ssgiQuality; }
            set { _ssgiQuality = value; }
        }
        public static bool SSGIFullRes
        {
            get { return _ssgiFullRes; }
            set { _ssgiFullRes = value; }
        }
        public static int SSGIRaySteps
        {
            get { return _ssgiRaySteps; }
            set { _ssgiRaySteps = value; }
        }
        public static int SSGIFilterRadius
        {
            get { return _ssgiFilterRadius; }
            set { _ssgiFilterRadius = value; }
        }
        /// Video - Fog
        public static bool EnableVolumetricFog
        {
            get { return _enableVolumetricFog; }
            set { _enableVolumetricFog = value; }
        }
        public static string VFDenoisingMode
        {
            get { return _vfDenoisingMode; }
            set { _vfDenoisingMode = value; }
        }
        public static int VFVolumetricDistance
        {
            get { return _vfVolumetricDistance; }
            set { _vfVolumetricDistance = value; }
        }
        /// Video - RayTracing
    }
}