using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ProjectUniverse.Serialization;
using TMPro;
using System;
using UnityEngine.Rendering;
using UnityEngine.InputSystem;

namespace ProjectUniverse.UI
{
    public class OptionsMenuUIController : MonoBehaviour
    {
        [SerializeField] private VolumeProfile defaultVolumeProfile;
        [Header("Menus")]
        [SerializeField] private Button OptionsMasterButton;
        [SerializeField] private Button Cat_GameplayButton;
        [SerializeField] private Button Cat_VideoButton;
        [SerializeField] private Button Cat_AudioButton;
        [SerializeField] private Button Cat_ControlsButton;
        [SerializeField] private Button Cat_SecretButton;
        [SerializeField] private Button Options_GeneralButton;
        [SerializeField] private Button Options_DetailButton;
        [SerializeField] private Button Options_AOShadowsButton;
        [SerializeField] private Button Options_DOFButton;
        [SerializeField] private Button Options_SSRBloomButton;
        [SerializeField] private Button Options_SSGIButton;
        [SerializeField] private Button Options_FogButton;
        [SerializeField] private Button Options_RaytracingButton;
        [SerializeField] private GameObject Cat_Bar;
        [SerializeField] private GameObject Options_Bar;
        [SerializeField] private GameObject Options_GenPanel;
        [SerializeField] private GameObject Options_DetailPanel;
        [SerializeField] private GameObject Options_AOShadowsPanel;
        [SerializeField] private GameObject Options_DoFPanel;
        [SerializeField] private GameObject Options_SSRBloomPanel;
        [SerializeField] private GameObject Options_SSGIPanel;
        [SerializeField] private GameObject Options_FogPanel;
        [SerializeField] private GameObject Options_Raytracing;
        [SerializeField] private GameObject Controls_Panel;
        [Header("Options - General")]
        [SerializeField] private TMP_Dropdown resolutionDrop;
        [SerializeField] private TMP_Dropdown graphicsDrop;
        [SerializeField] private TMP_Dropdown windowModeDrop;
        [SerializeField] private TMP_Dropdown fxaaDrop;
        [SerializeField] private TMP_Dropdown antiAliasingDrop;
        [SerializeField] private Slider fovSlider;
        [SerializeField] private Slider sensSlider;
        [Header("Options - Detail")]
        [SerializeField] private TMP_Dropdown textureQualityDrop;
        [SerializeField] private TMP_Dropdown gameQualityDrop;
        [Header("Options - AO/Shadows")]
        [SerializeField] private Toggle enableAO;
        [SerializeField] private TMP_Dropdown aoQualityDrop;
        [SerializeField] private Toggle aoFullRes;
        [SerializeField] private Slider aoSteps;
        [SerializeField] private Toggle aotempAccum;
        [SerializeField] private Toggle microShadows;
        [SerializeField] private TMP_Dropdown shadowRes;
        [SerializeField] private Slider maxShadows;
        [Header("Options - DOF")]
        [SerializeField] private TMP_Dropdown dofQualityDrop;
        [SerializeField] private Slider dofNearSamples;
        [SerializeField] private Slider dofNearRadius;
        [SerializeField] private Slider dofFarSamples;
        [SerializeField] private Slider dofFarRadius;
        [Header("Options - SSR/Bloom")]
        [SerializeField] private Toggle enableSSR;
        [SerializeField] private TMP_Dropdown ssrQualityDrop;
        [SerializeField] private Slider ssrMaxRaySteps;
        [SerializeField] private TMP_Dropdown ssrProbeResDrop;
        [SerializeField] private Slider ssrProbeCache;
        [SerializeField] private Toggle enableBloom;
        [SerializeField] private TMP_Dropdown bloomQuality;
        [Header("Options - SSGI")]
        [SerializeField] private Toggle enableSSGI;
        [SerializeField] private TMP_Dropdown ssgiQualityDrop;
        [SerializeField] private Toggle ssgiFullRes;
        [SerializeField] private Slider ssgiRaySteps;
        [SerializeField] private Slider ssgiFilterRadius;
        [Header("Options - Fog")]
        [SerializeField] private Toggle enableVolFog;
        [SerializeField] private TMP_Dropdown vfDenoisingMode;
        [SerializeField] private Slider vfVolDistance;
        [Header("Options - RayTracing")]
        [Header("Controls")]
        [SerializeField] private PlayerControls controlsFile;
        [SerializeField] private TMP_Text[] controlsArray=new TMP_Text[18];
        [Header("Misc")]
        private ColorBlock topMenuButtonDefaults;
        private ColorBlock sideMenuButtonDefaults;

        [SerializeField] private Color topMenuSelectedColor;
        [SerializeField] private Color sideMenuSelectedColor;
        //
        private int resIndex;
        private bool isfullscreen;
        private int fullscreen;//0 is false, 1 is true
        private Resolution[] resolutions;

        public void Start()
        {
            controlsFile = new PlayerControls();
            topMenuButtonDefaults = Cat_VideoButton.colors;
            sideMenuButtonDefaults = Options_GeneralButton.colors;

            resolutions = Screen.resolutions;
            int currentRes = 0;
            resolutionDrop.ClearOptions();
            List<string> options = new List<string>();
            for (int i = 0; i < resolutions.Length; i++)
            {
                if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
                {
                    currentRes = i;
                }
                string option = resolutions[i].width + " x " + resolutions[i].height;
                options.Add(option);
            }
            resolutionDrop.AddOptions(options);
            resolutionDrop.value = currentRes;
            resolutionDrop.RefreshShownValue();

            Cat_Bar.SetActive(false);

            //load saved settings
            float s = PlayerPrefs.GetFloat("sensitivity", 1f);
            int r = PlayerPrefs.GetInt("resolutionIndex", 0);
            int w = PlayerPrefs.GetInt("windowMode", 0);
            int ssr = PlayerPrefs.GetInt("doSSR", 1);
            if (ssr == 1)
            {
                enableSSR.isOn = true;
            }
            else
            {
                enableSSR.isOn = false;
            }
            windowModeDrop.value = w;
            resolutionDrop.value = resolutions.Length - 1;
            sensSlider.value = s;
            controlsArray[0].text = "" + s;
        }

        public void ShowOptionsMenu()
        {
            Cat_Bar.SetActive(true);
            //load saved settings - again
            float s = PlayerPrefs.GetFloat("sensitivity", 1f);
            int r = PlayerPrefs.GetInt("resolutionIndex", 0);
            int w = PlayerPrefs.GetInt("windowMode", 0);
            int ssr = PlayerPrefs.GetInt("doSSR", 1);
            if (ssr == 1)
            {
                enableSSR.isOn = true;
            }
            else
            {
                enableSSR.isOn = false;
            }
            windowModeDrop.value = w;
            resolutionDrop.value = resolutions.Length - 1;
            sensSlider.value = s;
            controlsArray[0].text = "" + s;
            //
            for(int n = 1; n < controlsArray.Length; n++)
            {
                //controlsArray[n].text = 
            }
        }

        public void ShowGamePlayOptions()
        {
            //save all options

            ColorBlock a = Cat_GameplayButton.colors;
            a.normalColor = topMenuSelectedColor;
            Cat_VideoButton.colors = topMenuButtonDefaults;
            Cat_AudioButton.colors = topMenuButtonDefaults;
            Cat_ControlsButton.colors = topMenuButtonDefaults;
            Cat_SecretButton.colors = topMenuButtonDefaults;
        }
        public void ShowVideoOptions()
        {
            //save all options

            ColorBlock a = Cat_VideoButton.colors;
            a.normalColor = topMenuSelectedColor;
            Cat_GameplayButton.colors = topMenuButtonDefaults;
            Cat_AudioButton.colors = topMenuButtonDefaults;
            Cat_ControlsButton.colors = topMenuButtonDefaults;
            Cat_SecretButton.colors = topMenuButtonDefaults;

            Options_Bar.SetActive(true);

        }
        public void ShowAudioOptions()
        {
            //save all options

            ColorBlock a = Cat_AudioButton.colors;
            a.normalColor = topMenuSelectedColor;
            Cat_GameplayButton.colors = topMenuButtonDefaults;
            Cat_VideoButton.colors = topMenuButtonDefaults;
            Cat_ControlsButton.colors = topMenuButtonDefaults;
            Cat_SecretButton.colors = topMenuButtonDefaults;
        }
        public void ShowControlsOptions()
        {
            //save all options

            ColorBlock a = Cat_ControlsButton.colors;
            a.normalColor = topMenuSelectedColor;
            Cat_GameplayButton.colors = topMenuButtonDefaults;
            Cat_VideoButton.colors = topMenuButtonDefaults;
            Cat_AudioButton.colors = topMenuButtonDefaults;
            Cat_SecretButton.colors = topMenuButtonDefaults;

            //show controls options
            Controls_Panel.SetActive(true);
        }
        public void ShowSecretOptions()
        {
            //save all options

            ColorBlock a = Cat_SecretButton.colors;
            a.normalColor = topMenuSelectedColor;
            Cat_GameplayButton.colors = topMenuButtonDefaults;
            Cat_VideoButton.colors = topMenuButtonDefaults;
            Cat_AudioButton.colors = topMenuButtonDefaults;
            Cat_ControlsButton.colors = topMenuButtonDefaults;
        }
        public void ShowGeneralPanel()
        {
            //save all options
            //load all options

            ColorBlock a = Options_GeneralButton.colors;
            a.normalColor = sideMenuSelectedColor;
            Options_DetailButton.colors = sideMenuButtonDefaults;
            Options_AOShadowsButton.colors = sideMenuButtonDefaults;
            Options_DOFButton.colors = sideMenuButtonDefaults;
            Options_SSRBloomButton.colors = sideMenuButtonDefaults;
            Options_SSGIButton.colors = sideMenuButtonDefaults;
            Options_FogButton.colors = sideMenuButtonDefaults;
            Options_RaytracingButton.colors = sideMenuButtonDefaults;

            Options_GenPanel.SetActive(true);
            Options_DetailPanel.SetActive(false);
            Options_AOShadowsPanel.SetActive(false);
            Options_DoFPanel.SetActive(false);
            Options_SSRBloomPanel.SetActive(false);
            Options_SSGIPanel.SetActive(false);
            Options_FogPanel.SetActive(false);
            Options_Raytracing.SetActive(false);
        }
        public void ShowDetailsPanel()
        {
            ColorBlock a = Options_DetailButton.colors;
            a.normalColor = sideMenuSelectedColor;
            Options_GeneralButton.colors = sideMenuButtonDefaults;
            Options_AOShadowsButton.colors = sideMenuButtonDefaults;
            Options_DOFButton.colors = sideMenuButtonDefaults;
            Options_SSRBloomButton.colors = sideMenuButtonDefaults;
            Options_SSGIButton.colors = sideMenuButtonDefaults;
            Options_FogButton.colors = sideMenuButtonDefaults;
            Options_RaytracingButton.colors = sideMenuButtonDefaults;

            Options_GenPanel.SetActive(false);
            Options_DetailPanel.SetActive(true);
            Options_AOShadowsPanel.SetActive(false);
            Options_DoFPanel.SetActive(false);
            Options_SSRBloomPanel.SetActive(false);
            Options_SSGIPanel.SetActive(false);
            Options_FogPanel.SetActive(false);
            Options_Raytracing.SetActive(false);
        }
        public void ShowAOShadowsPanel()
        {
            ColorBlock a = Options_AOShadowsButton.colors;
            a.normalColor = sideMenuSelectedColor;
            Options_GeneralButton.colors = sideMenuButtonDefaults;
            Options_DetailButton.colors = sideMenuButtonDefaults;
            Options_DOFButton.colors = sideMenuButtonDefaults;
            Options_SSRBloomButton.colors = sideMenuButtonDefaults;
            Options_SSGIButton.colors = sideMenuButtonDefaults;
            Options_FogButton.colors = sideMenuButtonDefaults;
            Options_RaytracingButton.colors = sideMenuButtonDefaults;

            Options_GenPanel.SetActive(false);
            Options_DetailPanel.SetActive(false);
            Options_AOShadowsPanel.SetActive(true);
            Options_DoFPanel.SetActive(false);
            Options_SSRBloomPanel.SetActive(false);
            Options_SSGIPanel.SetActive(false);
            Options_FogPanel.SetActive(false);
            Options_Raytracing.SetActive(false);
        }
        public void ShowDOFPanel()
        {
            ColorBlock a = Options_DOFButton.colors;
            a.normalColor = sideMenuSelectedColor;
            Options_GeneralButton.colors = sideMenuButtonDefaults;
            Options_DetailButton.colors = sideMenuButtonDefaults;
            Options_AOShadowsButton.colors = sideMenuButtonDefaults;
            Options_SSRBloomButton.colors = sideMenuButtonDefaults;
            Options_SSGIButton.colors = sideMenuButtonDefaults;
            Options_FogButton.colors = sideMenuButtonDefaults;
            Options_RaytracingButton.colors = sideMenuButtonDefaults;

            Options_GenPanel.SetActive(false);
            Options_DetailPanel.SetActive(false);
            Options_AOShadowsPanel.SetActive(false);
            Options_DoFPanel.SetActive(true);
            Options_SSRBloomPanel.SetActive(false);
            Options_SSGIPanel.SetActive(false);
            Options_FogPanel.SetActive(false);
            Options_Raytracing.SetActive(false);
        }
        public void ShowSSRBloomPanel()
        {
            ColorBlock a = Options_SSRBloomButton.colors;
            a.normalColor = sideMenuSelectedColor;
            Options_GeneralButton.colors = sideMenuButtonDefaults;
            Options_DetailButton.colors = sideMenuButtonDefaults;
            Options_AOShadowsButton.colors = sideMenuButtonDefaults;
            Options_DOFButton.colors = sideMenuButtonDefaults;
            Options_SSGIButton.colors = sideMenuButtonDefaults;
            Options_FogButton.colors = sideMenuButtonDefaults;
            Options_RaytracingButton.colors = sideMenuButtonDefaults;

            Options_GenPanel.SetActive(false);
            Options_DetailPanel.SetActive(false);
            Options_AOShadowsPanel.SetActive(false);
            Options_DoFPanel.SetActive(false);
            Options_SSRBloomPanel.SetActive(true);
            Options_SSGIPanel.SetActive(false);
            Options_FogPanel.SetActive(false);
            Options_Raytracing.SetActive(false);
        }
        public void ShowSSGIPanel()
        {
            ColorBlock a = Options_SSGIButton.colors;
            a.normalColor = sideMenuSelectedColor;
            Options_GeneralButton.colors = sideMenuButtonDefaults;
            Options_DetailButton.colors = sideMenuButtonDefaults;
            Options_AOShadowsButton.colors = sideMenuButtonDefaults;
            Options_DOFButton.colors = sideMenuButtonDefaults;
            Options_SSRBloomButton.colors = sideMenuButtonDefaults;
            Options_FogButton.colors = sideMenuButtonDefaults;
            Options_RaytracingButton.colors = sideMenuButtonDefaults;

            Options_GenPanel.SetActive(false);
            Options_DetailPanel.SetActive(false);
            Options_AOShadowsPanel.SetActive(false);
            Options_DoFPanel.SetActive(false);
            Options_SSRBloomPanel.SetActive(false);
            Options_SSGIPanel.SetActive(true);
            Options_FogPanel.SetActive(false);
            Options_Raytracing.SetActive(false);
        }
        public void ShowFogPanel()
        {
            ColorBlock a = Options_FogButton.colors;
            a.normalColor = sideMenuSelectedColor;
            Options_GeneralButton.colors = sideMenuButtonDefaults;
            Options_DetailButton.colors = sideMenuButtonDefaults;
            Options_AOShadowsButton.colors = sideMenuButtonDefaults;
            Options_DOFButton.colors = sideMenuButtonDefaults;
            Options_SSRBloomButton.colors = sideMenuButtonDefaults;
            Options_SSGIButton.colors = sideMenuButtonDefaults;
            Options_RaytracingButton.colors = sideMenuButtonDefaults;

            Options_GenPanel.SetActive(false);
            Options_DetailPanel.SetActive(false);
            Options_AOShadowsPanel.SetActive(false);
            Options_DoFPanel.SetActive(false);
            Options_SSRBloomPanel.SetActive(false);
            Options_SSGIPanel.SetActive(false);
            Options_FogPanel.SetActive(true);
            Options_Raytracing.SetActive(false);
        }
        public void ShowRTPanel()
        {
            ColorBlock a = Options_RaytracingButton.colors;
            a.normalColor = sideMenuSelectedColor;
            Options_GeneralButton.colors = sideMenuButtonDefaults;
            Options_DetailButton.colors = sideMenuButtonDefaults;
            Options_AOShadowsButton.colors = sideMenuButtonDefaults;
            Options_DOFButton.colors = sideMenuButtonDefaults;
            Options_SSRBloomButton.colors = sideMenuButtonDefaults;
            Options_SSGIButton.colors = sideMenuButtonDefaults;
            Options_FogButton.colors = sideMenuButtonDefaults;

            Options_GenPanel.SetActive(false);
            Options_DetailPanel.SetActive(false);
            Options_AOShadowsPanel.SetActive(false);
            Options_DoFPanel.SetActive(false);
            Options_SSRBloomPanel.SetActive(false);
            Options_SSGIPanel.SetActive(false);
            Options_FogPanel.SetActive(false);
            Options_Raytracing.SetActive(true);
        }

        public void BackToTopBar()
        {
            Options_GenPanel.SetActive(false);
            Options_DetailPanel.SetActive(false);
            Options_AOShadowsPanel.SetActive(false);
            Options_DoFPanel.SetActive(false);
            Options_SSRBloomPanel.SetActive(false);
            Options_SSGIPanel.SetActive(false);
            Options_FogPanel.SetActive(false);
            Options_Raytracing.SetActive(false);
            Options_Bar.SetActive(false);
            //save all settings
        }

        public void BackToMainMenuBar()
        {
            //save all settings - again
            if (GlobalSettings.EnableSSR)
            {
                PlayerPrefs.SetInt("doSSR", 1);
            }
            else
            {
                PlayerPrefs.SetInt("doSSR", 0);
            }
            PlayerPrefs.SetFloat("sensitivity",GlobalSettings.Sensitivity);
            PlayerPrefs.GetInt("resolutionIndex", GlobalSettings.ScreenResolution);
            PlayerPrefs.GetInt("windowMode", GlobalSettings.WindowMode);

            Cat_Bar.SetActive(false);
        }

        ///
        /// Options Methods
        ///

        public void SetResolution(int index)
        {
            GlobalSettings.ScreenResolution = resolutionDrop.value;
            Debug.Log(GlobalSettings.ScreenResolution);
            try
            {
                Resolution res = resolutions[index];
                Screen.SetResolution(res.width, res.height, Screen.fullScreen);
                resIndex = index;
            }
            catch (Exception e)
            {
                Debug.Log("Failed To Set Resolution: " + e.Message);
                Debug.Log(e.StackTrace);
            }
            PlayerPrefs.SetInt("resolutionIndex", GlobalSettings.ScreenResolution);
        }
        public void SetGraphicsGeneral()
        {
            GlobalSettings.GraphicsGeneral = graphicsDrop.options[graphicsDrop.value].text;
            Debug.Log(GlobalSettings.GraphicsGeneral);
        }
        public void SetWindowMode()
        {
            GlobalSettings.WindowMode = windowModeDrop.value;
            Debug.Log(GlobalSettings.WindowMode);
            if (GlobalSettings.WindowMode == 0)
            {
                Screen.fullScreen = true;
            }
            else
            {
                Screen.fullScreen = false;
            }
            PlayerPrefs.SetInt("windowMode", GlobalSettings.WindowMode);
        }
        public void SetBrightness(float brightness)
        {
            GlobalSettings.Brightness = brightness;
            Debug.Log(GlobalSettings.Brightness);
        }

        public void SetSensitivity(float sens)
        {
            GlobalSettings.Sensitivity = (float)Math.Round(sens,1);
            //Debug.Log(GlobalSettings.Sensitivity);
            PlayerPrefs.SetFloat("sensitivity", GlobalSettings.Sensitivity);
        }

        public void SetGama(float gama)
        {
            GlobalSettings.Gama = gama;
            Debug.Log(GlobalSettings.Gama);
        }
        public void MinExposure(float minExp)
        {
            GlobalSettings.MinExposure = minExp;
            Debug.Log(GlobalSettings.MinExposure);
        }
        public void MaxExposure(float maxExp)
        {
            GlobalSettings.MaxExposure = maxExp;
            Debug.Log(GlobalSettings.MaxExposure);
        }
        public void HUDBorderX(float bdX)
        {
            GlobalSettings.HUDBorderX = bdX;
            Debug.Log(GlobalSettings.HUDBorderX);
        }
        public void HUDBorderY(float bdY)
        {
            GlobalSettings.HUDBorderY = bdY;
            Debug.Log(GlobalSettings.HUDBorderY);
        }
        public void HUDScale(float scale)
        {
            GlobalSettings.HUDScale = scale;
            Debug.Log(GlobalSettings.HUDScale);
        }
        public void SetFOV()
        {
            GlobalSettings.FOV = (int)fovSlider.value;
            Debug.Log(GlobalSettings.FOV);
        }
        public void SetVsync(bool vsync)
        {
            GlobalSettings.VSync = vsync;
            Debug.Log(GlobalSettings.VSync);
        }
        public void SetFXAA()
        {
            GlobalSettings.FXAA = fxaaDrop.options[fxaaDrop.value].text;
            Debug.Log(GlobalSettings.FXAA);
        }
        public void SetAntiAliasing()
        {
            GlobalSettings.AntiAliasing = antiAliasingDrop.options[antiAliasingDrop.value].text;
            Debug.Log(GlobalSettings.AntiAliasing);
        }
        //Detail
        public void SetTextureQuality()
        {
            GlobalSettings.TextureQuality = textureQualityDrop.options[textureQualityDrop.value].text;
            Debug.Log(GlobalSettings.TextureQuality);
        }
        public void SetGameQuality()
        {
            GlobalSettings.GameQuality = gameQualityDrop.options[gameQualityDrop.value].text;
            Debug.Log(GlobalSettings.GameQuality);
        }
        public void SetMaxDecals(float value)
        {
            GlobalSettings.MaxDecals = value;
            Debug.Log(GlobalSettings.MaxDecals);
        }
        public void SetMaxDistnace(float value)
        {
            GlobalSettings.MaxDistance = value;
            Debug.Log(GlobalSettings.MaxDistance);
        }
        public void SetClutterDistnace(float value)
        {
            GlobalSettings.ClutterDistance = value;
            Debug.Log(GlobalSettings.ClutterDistance);
        }
        public void SetViewDistnace(float value)
        {
            GlobalSettings.ViewDistance = value;
            Debug.Log(GlobalSettings.ViewDistance);
        }
        //AO Shadows
        public void SetAOEnabled()
        {
            GlobalSettings.EnableAO = enableAO.isOn;
            Debug.Log(GlobalSettings.EnableAO);
        }
        public void SetAOIntensity(float value)
        {
            GlobalSettings.AOIntensity = value;
            Debug.Log(GlobalSettings.AOIntensity);
        }
        public void SetAODirectLight(float value)
        {
            GlobalSettings.AODirectLight = value;
            Debug.Log(GlobalSettings.AODirectLight);
        }
        public void SetAORadius(float value)
        {
            GlobalSettings.AORadius = value;
            Debug.Log(GlobalSettings.AORadius);
        }
        public void SetAOQuality()
        {
            GlobalSettings.AOQuality = aoQualityDrop.options[aoQualityDrop.value].text;
            Debug.Log(GlobalSettings.AOQuality);
        }
        public void SetAOFullRes()
        {
            GlobalSettings.AOFullRes = aoFullRes.isOn;
            Debug.Log(GlobalSettings.AOFullRes);
        }
        public void SetAOSteps()
        {
            GlobalSettings.AOSteps = (int)aoSteps.value;
            Debug.Log(GlobalSettings.AOSteps);
        }
        public void SetTemporalAccumulation()
        {
            GlobalSettings.AOTemporalAccumulation = aotempAccum.isOn;
            Debug.Log(GlobalSettings.AOTemporalAccumulation);
        }
        public void SetMicroShadowsEnabled()
        {
            GlobalSettings.MicroShadows = microShadows.isOn;
            Debug.Log(GlobalSettings.MicroShadows);
        }
        public void SetShadowResolution()
        {
            GlobalSettings.ShadowsResolution = shadowRes.options[shadowRes.value].text;
            Debug.Log(GlobalSettings.ShadowsResolution);
        }
        public void SetMaxShadowDistance(float value)
        {
            GlobalSettings.MaxShadowDistance = value;
            Debug.Log(GlobalSettings.MaxShadowDistance);
        }
        public void SetShadowCount()
        {
            GlobalSettings.MaxShadowCount = (int)maxShadows.value;
            Debug.Log(GlobalSettings.MaxShadowCount);
        }
        public void SetDOFQuality()
        {
            GlobalSettings.DOFQuality = dofQualityDrop.options[dofQualityDrop.value].text;
            Debug.Log(GlobalSettings.DOFQuality);
        }
        public void SetDOFNearSamples()
        {
            GlobalSettings.DOFNearSamples = (int)dofNearSamples.value;
            Debug.Log(GlobalSettings.DOFNearSamples);
        }
        public void SetDOFNearRadius()
        {
            GlobalSettings.DOFNearRadius = (int)dofNearRadius.value;
            Debug.Log(GlobalSettings.DOFNearRadius);
        }
        public void SetDOFFarSamples()
        {
            GlobalSettings.DOFFarSamples = (int)dofFarSamples.value;
            Debug.Log(GlobalSettings.DOFFarSamples);
        }
        public void SetDOFFarRadius()
        {
            GlobalSettings.DOFFarRadius = (int)dofFarRadius.value;
            Debug.Log(GlobalSettings.DOFFarRadius);
        }
        public void SetSSREnabled()
        {
            GlobalSettings.EnableSSR = enableSSR.isOn;
            Debug.Log(GlobalSettings.EnableSSR);
            if(defaultVolumeProfile.TryGet<ScreenSpaceReflection>(out var ssr))
            {
                ssr.enabled.overrideState = true;
                ssr.enabled.value = GlobalSettings.EnableSSR;
            }
            if (GlobalSettings.EnableSSR)
            {
                PlayerPrefs.SetInt("doSSR", 1);
            }
            else
            {
                PlayerPrefs.SetInt("doSSR", 0);
            }
            
        }
        public void SetSSRQuality()
        {
            GlobalSettings.SSRQuality = ssrQualityDrop.options[ssrQualityDrop.value].text;
            Debug.Log(GlobalSettings.SSRQuality);
        }
        public void SetSSREdgeFadeDistance(float value)
        {
            GlobalSettings.SSREdgeFadeDistance= value;
            Debug.Log(GlobalSettings.SSREdgeFadeDistance);
        }
        public void SetSSRObjectThickness(float value)
        {
            GlobalSettings.SSRObjectThickness = value;
            Debug.Log(GlobalSettings.SSRObjectThickness);
        }
        public void SetSSRMaxRaySteps()
        {
            GlobalSettings.SSRMaxRaySteps = (int)ssrMaxRaySteps.value;
            Debug.Log(GlobalSettings.SSRMaxRaySteps);
        }
        public void SetSSRProbeResolution()
        {
            GlobalSettings.SSRProbeResolution = ssrProbeResDrop.options[ssrProbeResDrop.value].text;
            Debug.Log(GlobalSettings.SSRProbeResolution);
        }
        public void SetSSRProbeCache()
        {
            GlobalSettings.SSRProbeCache = (int)ssrProbeCache.value;
            Debug.Log(GlobalSettings.SSRProbeCache);
        }
        public void SetBloomEnabled()
        {
            GlobalSettings.EnableBloom = enableBloom.isOn;
            Debug.Log(GlobalSettings.EnableBloom);
        }
        public void SetBloomQuality()
        {
            GlobalSettings.BloomQuality = bloomQuality.options[bloomQuality.value].text;
            Debug.Log(GlobalSettings.BloomQuality);
        }
        public void SetBloomThreshold(float value)
        {
            GlobalSettings.BloomThreshold = value;
            Debug.Log(GlobalSettings.BloomThreshold);
        }
        public void SetBloomIntensity(float value)
        {
            GlobalSettings.BloomIntensity = value;
            Debug.Log(GlobalSettings.BloomIntensity);
        }
        public void SetBloomScatter(float value)
        {
            GlobalSettings.BloomScatter = value;
            Debug.Log(GlobalSettings.BloomScatter);
        }
        public void SetSSGIEnabled()
        {
            GlobalSettings.EnableSSGI = enableSSGI.isOn;
            Debug.Log(GlobalSettings.EnableSSGI);
        }
        public void SetSSGIQuality()
        {
            GlobalSettings.SSGIQuality = ssgiQualityDrop.options[ssgiQualityDrop.value].text;
            Debug.Log(GlobalSettings.SSGIQuality);
        }
        public void SetSSGIFullRes()
        {
            GlobalSettings.SSGIFullRes = ssgiFullRes.isOn;
            Debug.Log(GlobalSettings.SSGIFullRes);
        }
        public void SetSSGIRaySteps()
        {
            GlobalSettings.SSGIRaySteps = (int)ssgiRaySteps.value;
            Debug.Log(GlobalSettings.SSGIRaySteps);
        }
        public void SetSSGIFilterRadius()
        {
            GlobalSettings.SSGIFilterRadius = (int)ssgiFilterRadius.value;
            Debug.Log(GlobalSettings.SSGIFilterRadius);
        }
        public void SetVolumetricFogEnabled()
        {
            GlobalSettings.EnableVolumetricFog = enableVolFog.isOn;
            Debug.Log(GlobalSettings.EnableVolumetricFog);
        }
        public void SetVFDenoisingMode()
        {
            GlobalSettings.VFDenoisingMode = vfDenoisingMode.options[vfDenoisingMode.value].text;
            Debug.Log(GlobalSettings.VFDenoisingMode);
        }
        public void SetVolumetricDistance()
        {
            GlobalSettings.VFVolumetricDistance = (int)vfVolDistance.value;
            Debug.Log(GlobalSettings.VFVolumetricDistance);
        }

        public void SetKeyBind(int i)
        {
            //use i to get targetted binding
            switch (i)
            {
                case 1:
                    
                    break;
                case 2: break;
                case 3: break;
                case 4: break;
                case 5:
                    //set button text to "Bind To: ..."
                    controlsArray[i].text = "Bind To: ...";
                    //wait for keyboard press
                    InputActionReference m_Action = InputActionReference.Create(controlsFile.FindAction("Fire"));
                    InputActionRebindingExtensions.RebindingOperation op = InputActionRebindingExtensions.PerformInteractiveRebinding(m_Action.action);
                    op.OnComplete(_ => UpdateKeybindings(5, m_Action));
                    op.Start();
                    break;
                case 6: break;
                case 7: break;
                case 8: break;
                case 9: break;
                case 10: break;
                case 11: break;
                case 12: break;
                case 13: break;
                case 14: break;
                case 15: break;
                case 16: break;
                case 17: break;
                case 18: break;
            }
            
           
            //assign key to bind
        }

        private void UpdateKeybindings(int i, InputActionReference m_Action)
        {
            controlsArray[i].text = "?";
            Debug.Log(m_Action.action.bindings.ToString());
        }

        public void LoadBindings()
        {

        }
        public void SaveBindings()
        {
            
        }
    }
}