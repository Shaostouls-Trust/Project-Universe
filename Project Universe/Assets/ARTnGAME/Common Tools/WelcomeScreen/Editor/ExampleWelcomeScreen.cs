using System;
using System.Collections.Generic;
using System.Linq;
using Artngame.CommonTools.WelcomeScreen;
using Artngame.CommonTools.WelcomeScreen.GuiElements;
using Artngame.CommonTools.WelcomeScreen.PreferenceDefinition;
using Artngame.CommonTools.WelcomeScreen.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
namespace Artngame.CommonTools.WelcomeScreen
{
    public class WelcomeScreen : ProductWelcomeScreenBase
    {
        #region CustomisablePerProduct

        // Analytics
        // To enable analytics please go to https://app.immersiveVRtools.com
        // You can register there and get AnalyticsVerificationToken, website will also allow you to view data

        //v0.1
        //public Texture2D banners;

        //General
        public static bool IsUsageAnalyticsAndCommunicationsEnabled = false;
        public static readonly string AnalyticsVerificationToken = ""; //Only add if analytics enabled. Check website.
        public const string ProjectId = "";
        public static string VersionId = "1.0";

        public static string ProductName = "";
        private const string StartWindowMenuItemPath = "Window/ARTnGAME/Welcome Screen";
        public static string[] ProductKeywords = new[] { "start", "vr", "tools" };
        private static readonly string ProjectIconName = "ProductImage64";

        private static readonly string GIProxyIconName = "GIProxyIcon"; 
        private static readonly string SkyMasterIconName = "SkyMasterIcon"; 
        private static readonly string InfiniGRASSIconName = "InfiniGRASSIcon";
        private static readonly string OrionIconName = "OrionIcon";
        private static readonly string EtherealIconName = "EtherealIcon";

        private static readonly string InfiniCLOUDIconName = "InfiniCLOUDIcon";
        private static readonly string InfiniRIVERIconName = "InfiniRIVERIcon";
        private static readonly string InfiniTREEIconName = "InfiniTREEIcon";
        private static readonly string IvySTUDIOName = "ivyStudioIcon";
        private static readonly string PDMIconName = "ParticleDynamicMagicIcon";
        private static readonly string OCEANISIconName = "OceanisIcon";

        private static readonly string GIBLIIconName = "GIBLIIcon";
        private static readonly string PANGAEAIconName = "PANGAEAIcon";

        private static readonly string LUMINAIconName = "LUMINAIcon";
        private static readonly string LIQUAIconName = "LIQUAIcon";

        private static readonly string InfiniSPLATIconName = "InfiniSPLATIcon";
        private static readonly string EnvironmentBuildingBundleIconName = "EnvironmentBuildingBundleIcon";
        private static readonly string TEMIconName = "TEMIcon";

        private static readonly string InfiniSNOWIconName = "InfiniSNOWIcon";
        private static readonly string InfiniBATCHIconName = "InfiniBATCHIcon";

        private static readonly string InfiniTUNESIconName = "InfiniTUNESIcon";

        private static readonly string GLAMORIconName = "GLAMORIcon";

        //Window Layout
        private static Vector2 _WindowSizePx = new Vector2(650, 650);
        private static readonly int LeftColumnWidth = 175;

        //Section Definitions
        private static readonly List<GuiSection> LeftSections = new List<GuiSection>() {
        new GuiSection("", new List<ClickableElement>
        {
            //Standard communication screen which is used when user has any message that you passed on to them
            new LastUpdateButton("New Update!", (screen) => LastUpdateSection.RenderMainScrollViewSection(screen)),
            //Standard welcome screen, that's visible unless there's new update
            new ChangeMainViewButton("Welcome", (screen) => MainContentSection.RenderMainScrollViewSection(screen)),
        }),
        new GuiSection("ARTnGAME", new List<ClickableElement>
        {
            //new ChangeMainViewButton("VR Integrations", (screen) =>
            //{
            //    GUILayout.Label(
            //        @"XR Toolkit will require some dependencies to run, please have a look in documentation, it should only take a few moments to set up.",
            //        screen.TextStyle
            //    );

            //    using (LayoutHelper.LabelWidth(200))
            //    {
            //        ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.EnableXrToolkitIntegrationPreferenceDefinition);
            //    }

            //    const int sectionBreakHeight = 15;
            //    GUILayout.Space(sectionBreakHeight);

            //    GUILayout.Label(
            //        @"VRTK require some dependencies to run, please have a look in documentation, it should only take a few moments to set up.",
            //        screen.TextStyle
            //    );

            //    using (LayoutHelper.LabelWidth(200))
            //    {
            //        ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.EnableVrtkIntegrationPreferenceDefinition);
            //    }
            //    GUILayout.Space(sectionBreakHeight);

            //}),
            //new ChangeMainViewButton("Shaders", (screen) =>
            //{
            //    GUILayout.Label(
            //        @"By default package uses HDRP shaders, you can change those to standard surface shaders from dropdown below",
            //        screen.TextStyle
            //    );

            //    using (LayoutHelper.LabelWidth(200))
            //    {
            //        ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
            //    }
            //})

             //if (base._productIconGIProxy != null) {
             //   GUILayout.Label(_productIconGIProxy);
             //   }

           
            //v0.2
            new ChangeMainViewButton("Environment Building Bundle", (screen) =>
            {

                //v0.1a
                GUILayout.Label(screen._productIconEnviro, GUILayout.Width(425),GUILayout.Height(255));//  screen.banners,GUILayout.Width(120),GUILayout.Height(120));

                GUILayout.Label(
                    @"Environment Building Bundle is the new ARTnGAME asset collection bundle containing most of ARTnGAME major assets, focused on environment creation and weather effects.
The system will be updated with bonus assets as new environment related assets are released. InfiniSPLAT is the next major bonus planned addition.
The Bundle is planned to be more than the collection of the separate assets, the plan is to later offer demos that will showcase the combination of more than one of ARTnGAME assets used together.",
                    screen.TextStyle
                );

                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Environment Building Bundle combined assets Video", "https://www.youtube.com/watch?v=oC41-XrP0lU"
                     );
                }

                  GUILayout.Label(
                    @"Sky Master ULTIMATE users can upgrade to the Environment Building Bundle at a very big discount.",
                    screen.TextStyle
                );
                
                 using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Environment Building Bundle Asset Store Page", "https://assetstore.unity.com/packages/tools/particles-effects/environment-building-bundle-238238"
                     );
                }

            }),
            //v0.1
            new ChangeMainViewButton("Global Illumination Proxy", (screen) =>
            {

                 //v0.1a
             GUILayout.Label(screen._productIconGIProxy, GUILayout.Width(450),GUILayout.Height(85));//  screen.banners,GUILayout.Width(120),GUILayout.Height(120));

                GUILayout.Label(
                    @"Global Illumination Proxy is a light manipulation framework that allows the emulation of indirect global illumination in Unity, with support for all Unity versions & platforms and optimizations for great performance on Mobile.


By default package uses Standard Pipeline shaders, you can change those to HDRP or URP by taking advantage of Unity material conversion tools.",
                    screen.TextStyle
                );

                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("GI Proxy Tutorial Videos Playlist", "https://www.youtube.com/watch?v=d4B7gj5Myqw&list=PLJQvoQM6t9GfLLM7XDSIyzKApVXRKmHtq&index=7"
                     );
                }

                  GUILayout.Label(
                    @"Global Illumination Proxy can also be used with Sky Master ULTIMATE and is included in all Sky Master ULTIMATE versions.",
                    screen.TextStyle
                );

                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("GI Proxy used with Sky Master ULTIMATE Video", "https://www.youtube.com/watch?v=G4T54SI0Cdk&list=PLJQvoQM6t9GfLLM7XDSIyzKApVXRKmHtq&index=6"
                     );
                }
                 using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("GI PROXY Asset Store Page", "https://assetstore.unity.com/packages/tools/level-design/global-illumination-proxy-21197"
                     );
                }

            }),
            //v0.1a
            new ChangeMainViewButton("Sky Master ULTIMATE", (screen) =>
            {

                 //v0.1a
             GUILayout.Label(screen._productIconSkyMaster, GUILayout.Width(450),GUILayout.Height(85));//  screen.banners,GUILayout.Width(120),GUILayout.Height(120));

                GUILayout.Label(
                    @"Sky Master ULTIMATE is a Dynamic Sky, Weather, Volumetric Cloud - Lighting, Dynamic GI & Ocean solution for Unity.",
                    screen.TextStyle
                );

                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Sky Master ULTIMATE Tutorial Videos Playlist", "https://www.youtube.com/watch?list=PLJQvoQM6t9Ge2ehO4N1kNq3jvHmVst_el&v=XXCMXiuM9VA"
                     );
                }

                  GUILayout.Label(
                    @"The Beta versions of the URP and HDRP complete remakes of the Sky Master ULTIMATE system are available to all users on PM request in email or Sky Master Unity forum thread.",
                    screen.TextStyle
                );

                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Sky Master URP - HDRP Tutorial Videos Playlist", "https://www.youtube.com/watch?list=PLJQvoQM6t9Gead80gg5MSyN-gsEzmO0LW&v=w6useVWMeMM"
                     );
                }
                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Sky Master URP Ethereal system Tutorial Videos Playlist", "https://www.youtube.com/watch?list=PLJQvoQM6t9GdvknDg470wVngbVAo_WEGo&v=vJIka5aX94o"
                     );
                }

                GUILayout.BeginHorizontal();
                 using (LayoutHelper.LabelWidth(100))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Sky Master Unity Forum Thread", "https://forum.unity.com/threads/78-off-offer-sky-master-hdrp-urp-mobile-sky-ocean-volume-clouds-lighting-weather-realgi.280612/"
                     );
                }
                  using (LayoutHelper.LabelWidth(100))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Sky Master Asset Store Page", "https://assetstore.unity.com/packages/tools/particles-effects/sky-master-ultimate-volumetric-skies-weather-25357"
                     );
                }
                  GUILayout.EndHorizontal();

            }),
            //v0.1a
            new ChangeMainViewButton("InfiniGRASS", (screen) =>
            {

                 //v0.1a
                GUILayout.Label(screen._productIconInfiniGRASS, GUILayout.Width(450),GUILayout.Height(85));//  screen.banners,GUILayout.Width(120),GUILayout.Height(120));

                GUILayout.Label(
                    @"InfiniGRASS is a robust grass and prefab painting and optimization system, which allows very detailed next gen grass, trees and foliage to be placed in mass quantities on any surface.",
                    screen.TextStyle
                );

                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniGRASS Tutorial Videos Playlist", "https://www.youtube.com/watch?list=PLJQvoQM6t9Gddp8uags3YC4D9zUQ4CbYt&v=l-apm8ZcZuc"
                     );
                }

                  GUILayout.Label(
                    @"InfiniGRASS can also be used with Sky Master ULTIMATE for various effects including snow build up on foliage depending on weather conditions.",
                    screen.TextStyle
                );

                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniGRASS used with Sky Master ULTIMATE Video", "https://www.youtube.com/watch?list=PLJQvoQM6t9Gf8C5CxuX52Srzxd7BMyk-5&v=eERpEqGUbpc"
                     );
                }

                //InfiniGRASS STUDIO
                GUILayout.Label(
                    @"InfiniGRASS STUDIO is the upcoming new asset that includes the next generation of the system that uses the InfiniGRASS as base. The STUDIO version is in closed Beta for InfiniGRASS and Sky Master ULTIMATE users.",
                    screen.TextStyle
                );
                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniGRASS STUDIO Tutorial and showcase Videos", "https://www.youtube.com/watch?v=grJBt5VfU8U&list=PLJQvoQM6t9Gd8knVRnO0FWLoG7QMzIcpk&index=1"
                     );
                }                

                GUILayout.BeginHorizontal();
                 using (LayoutHelper.LabelWidth(100))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniGRASS Unity Forum Thread", "https://forum.unity.com/threads/50-off-offer-infinigrass-gpu-optimized-interactive-grass-trees-meshes-work-on-mobile-hdrp-urp.351694/"
                     );
                }
                using (LayoutHelper.LabelWidth(100))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniGRASS Asset Store Page", "https://assetstore.unity.com/packages/tools/particles-effects/infini-grass-gpu-vegetation-45188"
                     );
                }
                GUILayout.EndHorizontal();

            }),
            //v0.1a
            new ChangeMainViewButton("Ethereal URP Volume Lighting", (screen) =>
            {

                 //v0.1a
                GUILayout.Label(screen._productIconETHEREAL, GUILayout.Width(450),GUILayout.Height(85));//  screen.banners,GUILayout.Width(120),GUILayout.Height(120));

               GUILayout.Label(
                    @"Ethereal URP Volumetric lighting and Fog is an asset dedicated to the creation of atmospheric fog and volumetric lighting effects in the Universal Pipeline, by simulating the illumination of the small particles present in the atmosphere.",
                    screen.TextStyle
                );

                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Ethereal Tutorial Videos Playlist", "https://www.youtube.com/watch?list=PLJQvoQM6t9GdvknDg470wVngbVAo_WEGo&v=vJIka5aX94o"
                     );
                }
                 using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Ethereal v1.5 Tutorial", "https://www.youtube.com/watch?list=PLJQvoQM6t9GdvknDg470wVngbVAo_WEGo&v=UkZGJlOBueg"
                     );
                }

                  GUILayout.Label(
                    @"Ethereal is included in Sky Master ULTIMATE URP Version.",
                    screen.TextStyle
                );

                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Ethereal used in Sky Master ULTIMATE URP Video", "https://www.youtube.com/watch?list=PLJQvoQM6t9GemRdXnNGgPVbO1SqIGsdpA&v=_wPenCtIjsM"
                     );
                }

                GUILayout.BeginHorizontal();
                 using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Ethereal Unity Forum Thread", "https://forum.unity.com/threads/71-off-offer-ethereal-urp-volumetric-lighting-atmosphere-and-fog-system-for-the-urp.1031233/"
                     );
                }
                  using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Ethereal Asset Store Page", "https://assetstore.unity.com/packages/tools/particles-effects/ethereal-urp-volumetric-lighting-fog-187323"
                     );
                }
                  GUILayout.EndHorizontal();
            }),


            //v0.1b
            new ChangeMainViewButton("LUMINA Global Illumination", (screen) =>
            {
                //v0.1a
                GUILayout.Label(screen._productIconLUMINA, GUILayout.Width(450),GUILayout.Height(85));//  screen.banners,GUILayout.Width(120),GUILayout.Height(120));
                GUILayout.Label(
                    @"LUMINA Real-Time Global Illumination is a true indirect lighting calculation system using scene voxelization, embedded ambient occlusion & mesh based light sources for any light shape. Ideally combined with Sky Master ULTIMATE Ethereal volumetric lighting asset for cutting edge scene lighting.",
                    screen.TextStyle
                );               
                using (LayoutHelper.LabelWidth(200))
                {
                    ProductPreferenceBase.RenderURL("LUMINA URP Tutorial video", "https://www.youtube.com/watch?list=PLJQvoQM6t9GeExHKPgNJ7fZM-dK4sTZ7p&v=fHEsjMaIVMc"
                     );
                }
                GUILayout.BeginHorizontal();
                 using (LayoutHelper.LabelWidth(100))
                {
                    ProductPreferenceBase.RenderURL("LUMINA URP with Sky Master ULTIMATE Ethereal", "https://www.youtube.com/watch?list=PLJQvoQM6t9GeExHKPgNJ7fZM-dK4sTZ7p&v=6KxhpFNrZq4"
                     );
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                 using (LayoutHelper.LabelWidth(200))
                {
                    ProductPreferenceBase.RenderURL("LUMINA Unity Forum Thread", "https://forum.unity.com/threads/released-50-off-lumina-gi-real-time-true-voxel-based-global-illumination-for-urp-hdpr-in-wip.1307583/"
                     );
                }
                  using (LayoutHelper.LabelWidth(200))
                {                   
                    ProductPreferenceBase.RenderURL("LUMINA Asset Store Page", "https://assetstore.unity.com/packages/tools/particles-effects/lumina-gi-real-time-voxel-global-illumination-226660"
                     );
                }
                  GUILayout.EndHorizontal();

            }), //END LUMINA


            //v0.1a
            new ChangeMainViewButton("ORION Space Scene Creation", (screen) =>
            {

                 //v0.1a
                GUILayout.Label(screen._productIconORION, GUILayout.Width(450),GUILayout.Height(85));//  screen.banners,GUILayout.Width(120),GUILayout.Height(120));

                GUILayout.Label(
                    @"ORION Space Scene Generation framework is a system that covers all space scene generation aspects, from procedural planets & spaceships to any relevant special effects, with support for all pipelines.",
                    screen.TextStyle
                );

                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("ORION Tutorial Videos Playlist", "https://www.youtube.com/watch?list=PLJQvoQM6t9Ge296gQqQdO56Q28la5btOj&v=YhinFIat1s0"
                     );
                }

                  GUILayout.Label(
                    @"ORION can also be used with Sky Master ULTIMATE for planetary volumetric clouds.",
                    screen.TextStyle
                );

                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("ORION used with Sky Master ULTIMATE Video", "https://www.youtube.com/watch?list=PLJQvoQM6t9GemRdXnNGgPVbO1SqIGsdpA&v=SPofagIFiWw"
                     );
                }

                GUILayout.BeginHorizontal();
                 using (LayoutHelper.LabelWidth(100))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("ORION Unity Forum Thread", "https://forum.unity.com/threads/50-off-at-refresh-sale-orion-space-scene-generation-framework.1192270/"
                     );
                }
                    using (LayoutHelper.LabelWidth(100))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("ORION Asset Store Page", "https://assetstore.unity.com/packages/tools/level-design/orion-space-scene-generation-framework-206113"
                     );
                }
                GUILayout.EndHorizontal();


            }),

             //v0.1
            new ChangeMainViewButton("GLAMOR Image Fx Framework", (screen) =>
            {
                //v0.1a
                GUILayout.Label(screen._productIconGLAMOR, GUILayout.Width(450),GUILayout.Height(85));//  screen.banners,GUILayout.Width(120),GUILayout.Height(120));

                GUILayout.Label(
                    @"GLAMOR Image Effects framework is a collection of advanced layer selective post effects for Unity, including Eye Adaptation, Painterly effects, Bloom and Haze, Impostor volumetric lights, Lens Flares and a very extensive list of effects that will be constantly expanded.",
                    screen.TextStyle
                );

                  using (LayoutHelper.LabelWidth(200))
                {
                    ProductPreferenceBase.RenderURL("GLAMOR Image effects Tutorial Video", "https://www.youtube.com/watch?v=KFBezxr1xxE"
                     );
                }
                    using (LayoutHelper.LabelWidth(200))
                {
                    ProductPreferenceBase.RenderURL("GLAMOR Image effects demo showcase", "https://www.youtube.com/watch?v=7Au9lgQAdYY"
                     );
                }
                  using (LayoutHelper.LabelWidth(200))
                {
                    ProductPreferenceBase.RenderURL("GLAMOR Image effects with Sky Master and ORION", "https://www.youtube.com/watch?v=HXgwQYbw3_k"
                     );
                }
                    
            }),

            //v0.1b
            new ChangeMainViewButton("InfiniRIVER Fluid Dynamics", (screen) =>
            {
                 //v0.1a
                GUILayout.Label(screen._productIconInfiniRIVER, GUILayout.Width(450),GUILayout.Height(85));//  screen.banners,GUILayout.Width(120),GUILayout.Height(120));

                GUILayout.Label(
                    @"InfiniRIVER is a Fluid based water simulator system that can emulate the interaction of water with scene objects in real time using a fluid solver in a global or local way for versatility.",
                    screen.TextStyle
                );

                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniRIVER Tutorial Videos Playlist", "https://www.youtube.com/watch?list=PLJQvoQM6t9GddDxIVi0jNHbKuLcuWSjH1&v=LlFBoo4OMBU"
                     );
                }

                  GUILayout.Label(
                    @"InfiniRIVER can be used with two main fluid solutions, one terrain wide for big dynamic bodies of water and one local for detailed vortexes.",
                    screen.TextStyle
                );

                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniRIVER Fluid solvers.", "https://www.youtube.com/watch?v=hXD8ObIxBIg"
                     );
                }

                GUILayout.BeginHorizontal();
                 using (LayoutHelper.LabelWidth(100))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniRIVER Unity Forum Thread", "https://forum.unity.com/threads/discount-offers-infiniriver-real-time-fluid-water-lava-simulator-river-generation.1186711/"
                     );
                }
                    using (LayoutHelper.LabelWidth(100))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniRIVER Asset Store Page", "https://assetstore.unity.com/packages/tools/terrain/infiniriver-fluid-based-water-simulator-205568"
                     );
                }
                GUILayout.EndHorizontal();
            }),
            //v0.1b
            new ChangeMainViewButton("InfiniCLOUD", (screen) =>
            {
                 //v0.1a
                GUILayout.Label(screen._productIconInfniCLOUD, GUILayout.Width(450),GUILayout.Height(85));//  screen.banners,GUILayout.Width(120),GUILayout.Height(120));

                GUILayout.Label(
                    @"InfiniCLOUD HDRP-URP is a volumetric clouds system for the new Scriptable Render Pipelines in Unity, with focus on optimization with spectacular graphics.",
                    screen.TextStyle
                );

                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniCLOUD Tutorial Video", "https://www.youtube.com/watch?v=pr0r60lpN3c"
                     );
                }
                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniCLOUD Tutorial Video used in Sky Master ULTIMATE", "https://www.youtube.com/watch?v=XXCMXiuM9VA"
                     );
                }
                  GUILayout.Label(
                    @"InfiniCLOUD is included in Sky Master ULTIMATE in all pipelines.",
                    screen.TextStyle
                );

                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniCLOUD vortex system.", "https://www.youtube.com/watch?v=WZyCFdCXUvs"
                     );
                }

                GUILayout.BeginHorizontal();
                 using (LayoutHelper.LabelWidth(100))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniCLOUD Unity Forum Thread", "https://forum.unity.com/threads/new-offers-infinicloud-hdrp-urp-volumetric-clouds-for-the-new-pipelines-mobile.745415/page-3"
                     );
                }
                    using (LayoutHelper.LabelWidth(100))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniCLOUD Asset Store Page", "https://assetstore.unity.com/packages/tools/particles-effects/infinicloud-hdrp-urp-volumetric-clouds-particles-154133"
                     );
                }
                GUILayout.EndHorizontal();
            }),
            //v0.1b
            new ChangeMainViewButton("InfiniTREE", (screen) =>
            {
                 //v0.1a
                GUILayout.Label(screen._productIconInfiniTREE, GUILayout.Width(450),GUILayout.Height(85));//  screen.banners,GUILayout.Width(120),GUILayout.Height(120));

                GUILayout.Label(
                    @"InfiniTREE is a procedural tree generation and optimization system for Unity for all pipelines. It supports dynamic LOD management, ultimate ease of customization, using any prefab or Unity Tree Creator to define tree parts and interactive trees.",
                    screen.TextStyle
                );

                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniTREE HDRP", "https://www.youtube.com/watch?v=-0g_wYUITqk"
                     );
                }
                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniTREE tree chopping", "https://vimeo.com/122318534?embedded=true&source=video_title&owner=31557072"
                     );
                }
                  
                
                GUILayout.BeginHorizontal();
                 using (LayoutHelper.LabelWidth(100))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniTREE Unity Forum Thread", "https://forum.unity.com/threads/new-big-offers-infinitree-procedural-ltree-generation-growth-dynamics.287512/page-4"
                     );
                }
                    using (LayoutHelper.LabelWidth(100))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniTREE Asset Store Page", "https://assetstore.unity.com/packages/tools/level-design/infinitree-procedural-forest-creation-optimization-27457"
                     );
                }
                GUILayout.EndHorizontal();
            }),
            //v0.1b
            new ChangeMainViewButton("Ivy Studio", (screen) =>
            {
                 //v0.1a
                GUILayout.Label(screen._productIconIvyStudio, GUILayout.Width(450),GUILayout.Height(85));//  screen.banners,GUILayout.Width(120),GUILayout.Height(120));

                GUILayout.Label(
                    @"Ivy Studio is a procedural Ivy and climbing plants generator that focuses on the creation of realistic environment adaptive vegetation. Generate Ivy in editor or grow it gradually at runtime and interact with it in various ways.",
                    screen.TextStyle
                );

                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Ivy Studio Tutorials Playlist", "https://www.youtube.com/watch?v=mNpAjVFhlA0&list=PLJQvoQM6t9GfscpJJBgGEUd6TI-1YUmhm&index=1"
                     );
                }
                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Ivy Studio interactive vines", "https://www.youtube.com/watch?v=i192PVLQjxs"
                     );
                }


                GUILayout.BeginHorizontal();
                 using (LayoutHelper.LabelWidth(100))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Ivy Studio Unity Forum Thread", "https://forum.unity.com/threads/50-off-initial-release-ivy-studio-next-gen-of-ivy-creation-optimize-in-editor-run-time.1260143/"
                     );
                }
                    using (LayoutHelper.LabelWidth(100))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Ivy Studio Asset Store Page", "https://assetstore.unity.com/packages/tools/modeling/ivy-studio-procedural-vine-generation-217205"
                     );
                }
                GUILayout.EndHorizontal();
            }),
             //v0.1b
            new ChangeMainViewButton("Particle Dynamic Magic", (screen) =>
            {
                 //v0.1a
                GUILayout.Label(screen._productIconParticleDynamicMagic, GUILayout.Width(450),GUILayout.Height(85));//  screen.banners,GUILayout.Width(120),GUILayout.Height(120));

                GUILayout.Label(
                    @"Particle Dynamic Magic is a dynamic decal, particle and spline creation & manipulation framework that allows creative, performant and unique control of particles & gameobjects in Unity.",
                    screen.TextStyle
                );

                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Particle Dynamic Magic Showcase Videos Playlist", "https://www.youtube.com/watch?v=Qcu7ky0h5l8&list=PLJQvoQM6t9GdjWxtAIS9A6o3vBwg1WTYB&index=1"
                     );
                }
                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Particle Dynamic Magic 2.0 Video", "https://vimeo.com/129664655?embedded=true&source=video_title&owner=31557072"
                     );
                }
                
                GUILayout.BeginHorizontal();
                 using (LayoutHelper.LabelWidth(100))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Particle Dynamic Magic Unity Forum Thread", "https://forum.unity.com/threads/new-offers-particle-dynamic-magic-2-advanced-fx-framework-decal-particle-spline-ai.239305/page-11"
                     );
                }
                    using (LayoutHelper.LabelWidth(100))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Asset Store Page", "https://assetstore.unity.com/packages/tools/level-design/particle-dynamic-magic-2-decal-spline-ai-particles-dynamics-16175"
                     );
                }
                GUILayout.EndHorizontal();
            }),
            //v0.1b
            new ChangeMainViewButton("GIBLION Anime Scene Maker", (screen) =>
            {
                 //v0.1a
                GUILayout.Label(screen._productIconGIBLI, GUILayout.Width(450),GUILayout.Height(85));//  screen.banners,GUILayout.Width(120),GUILayout.Height(120));

                GUILayout.Label(
                    @"GIBLION - Anime scene generation framework is a next gen Anime scene creation tool for Unity URP. The system packs multiple toon, outlining and non photoreal shading techniques, blob based creation of dynamic polygon clouds and meshes, particle based foliage generation, dynamic toon grass, dynamic wires system, toon water and much more.",
                    screen.TextStyle
                );

                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("GIBLION Showcase Videos Playlist", "https://www.youtube.com/watch?v=9tjj9KGu9Cs&list=PLJQvoQM6t9GfjoOi_RIx5c_taEo6J1F1_&index=1"
                     );
                }


                GUILayout.BeginHorizontal();
                 using (LayoutHelper.LabelWidth(100))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("GIBLION Unity Forum Thread", "https://forum.unity.com/threads/gibli-next-generation-anime-cartoon-scene-generation-framework.1235929/"
                     );
                }
                  using (LayoutHelper.LabelWidth(100))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Asset Store Page", "https://assetstore.unity.com/packages/vfx/giblion-anime-scene-generation-framework-215911"
                     );
                }
                GUILayout.EndHorizontal();

                  using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("GIBLION Tutorial Video", "https://www.youtube.com/watch?v=lVAzRLqBqBg"
                     );
                }
                //    using (LayoutHelper.LabelWidth(100))
                //{
                //    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                //    ProductPreferenceBase.RenderURL("Oceanis Asset Store Page", "Coming Soon"
                //     );
                //}
               // GUILayout.EndHorizontal();
            }),
             //v0.1b
            new ChangeMainViewButton("Oceanis Water system", (screen) =>
            {
                 //v0.1a
                GUILayout.Label(screen._productIconOceanis, GUILayout.Width(450),GUILayout.Height(85));//  screen.banners,GUILayout.Width(120),GUILayout.Height(120));

                GUILayout.Label(
                    @"Oceanis is a new ocean and water system for Unity HDRP and URP pipelines. The system has been released for the URP pipeline. Also a base version of Oceanis HDRP is included in Sky Master ULTIMATE HDRP version.",
                    screen.TextStyle
                );
                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Oceanis URP Tutorial Video", "https://www.youtube.com/watch?list=PLJQvoQM6t9GejhKWf4NgDl3EKlERRcSU3&v=uRD08e8e-HU"
                     );
                }
                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Oceanis HDRP Beta Tutorial Video A", "https://www.youtube.com/watch?v=GNM02VZmOzQ&list=PLJQvoQM6t9Gead80gg5MSyN-gsEzmO0LW&index=6"
                     );
                }
                 using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Oceanis HDRP Beta Tutorial Video B", "https://www.youtube.com/watch?v=0bIq0WF_Mm4&list=PLJQvoQM6t9GemRdXnNGgPVbO1SqIGsdpA&index=5"
                     );
                }
                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Oceanis Underwater rendering", "https://www.youtube.com/watch?v=9hD10SzP_18"
                     );
                }

                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Oceanis Massive number of Floaters", "https://www.youtube.com/watch?v=Pf00LuBCrLM"
                     );
                }

                GUILayout.BeginHorizontal();
                 using (LayoutHelper.LabelWidth(100))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Oceanis Unity Forum Thread", "https://forum.unity.com/threads/oceanis-hdrp-urp-water-ocean-system.767168/"
                     );
                }                  
                  using (LayoutHelper.LabelWidth(200))
                {
                    ProductPreferenceBase.RenderURL("Oceanis Asset Store Page", "https://assetstore.unity.com/packages/tools/particles-effects/oceanis-urp-pro-water-framework-224933"
                     );
                }

                //    using (LayoutHelper.LabelWidth(100))
                //{
                //    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                //    ProductPreferenceBase.RenderURL("Oceanis Asset Store Page", "Coming Soon"
                //     );
                //}
                GUILayout.EndHorizontal();
            }),            
            
            //v0.1
            new ChangeMainViewButton("InfiniTUNES Medieval Music", (screen) =>
            {
                 //v0.1a
             GUILayout.Label(screen._productIconInfiniTUNES, GUILayout.Width(450),GUILayout.Height(85));//  screen.banners,GUILayout.Width(120),GUILayout.Height(120));

                GUILayout.Label(
                    @"InfiniTUNES Medieval Music is a collection of medieval themed real instrument songs, 
that also include each song's individual instrument tracks. The tracks can be recombined 
using the included InfiniTUNE system for unique track combinations and song variations.",
                    screen.TextStyle
                );

                  using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniTUNES Collection A Soundcloud", "https://soundcloud.com/artengames/sets/medieval-collection-a-samples"
                     );
                }
                    using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniTUNES Collection B Soundcloud", "https://soundcloud.com/artengames/sets/medieval-collection-b-samples"
                     );
                }
                      using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniTUNES Collection C Soundcloud", "https://soundcloud.com/artengames/sets/medieval-collection-c-samples"
                     );
                }
                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniTUNES video with InfiniGRASS STUDIO", "https://www.youtube.com/watch?v=qgycxO-HMJc"
                     );
                }
                 using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniTUNES Real Instrument Music - Dynamic music controller showcase", "https://www.youtube.com/watch?v=ga-G7kb2LF4"
                     );
                }
                 using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniTUNES Forum", "https://forum.unity.com/threads/released-infinitunes-music-with-real-instruments-and-tracks-combination-system.1453270/"
                     );
                }
            }),
            
            //v0.1b - LIQUA
            new ChangeMainViewButton("LIQUA Volumetric Fluids", (screen) =>
            {
                //v0.1a
                GUILayout.Label(screen._productIconLIQUA, GUILayout.Width(450),GUILayout.Height(85));//  screen.banners,GUILayout.Width(120),GUILayout.Height(120));
                GUILayout.Label(
                    @"LIQUA Volumetric Fluids is a system to render fluids inside containers with various methods and covering various use cases. The system has shader and full 3D simulation based fluids.",
                    screen.TextStyle
                );
                using (LayoutHelper.LabelWidth(200))
                {
                    ProductPreferenceBase.RenderURL("LIQUA Raytraced Liquid Surfaces", "https://www.youtube.com/watch?v=HMBjTwNAfhQ"
                     );
                }
                
                GUILayout.BeginHorizontal();
                 using (LayoutHelper.LabelWidth(200))
                {
                    ProductPreferenceBase.RenderURL("LIQUA Unity Forum Thread", "https://forum.unity.com/threads/coming-soon-liqua-volumetric-fluids.1369425/"
                     );
                }
                  using (LayoutHelper.LabelWidth(200))
                {
                    ProductPreferenceBase.RenderURL("LIQUA Asset Store Page", "https://assetstore.unity.com/packages/vfx/shaders/liqua-volumetric-fluids-240987"
                     );
                }
                  GUILayout.EndHorizontal();

            }), //END LIQUA
            
             //v0.1
            new ChangeMainViewButton("Toon Effects Maker", (screen) =>
            {

                 //v0.1a
             GUILayout.Label(screen._productIconTem, GUILayout.Width(450),GUILayout.Height(85));//  screen.banners,GUILayout.Width(120),GUILayout.Height(120));

                GUILayout.Label(
                    @"Toon Effects Maker is a special FX creation framework, with focus on cartoon style and an extensive library of ready to use modules for creating composite effects through the system.",
                    screen.TextStyle
                );

                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Toon Effects Maker Videos Playlist", "https://www.youtube.com/watch?v=ArtHoMEtxl4&list=PLJQvoQM6t9Gd3vYhxVU8UUAMaucNLQWm5&index=2"
                     );
                }  
                 using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Toon Effects Maker Unity Forum Thread", "https://forum.unity.com/threads/new-offers-toon-effects-maker-effect-library-toon-ocean-sprite-sheet-creator-path-record.299776/"
                     );
                }  
                 using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Toon Effects Maker SRP Asset Store Page", "https://assetstore.unity.com/packages/tools/level-design/toon-effects-maker-special-fx-creation-control-30849"
                     );
                }
                  using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("Toon Effects Maker URP ULTIMATE Asset Store Page", "https://u3d.as/328f"
                     );
                }
            }),

            //v0.1
            new ChangeMainViewButton("InfiniBATCH (Coming Soon)", (screen) =>
            {
                 //v0.1a
             GUILayout.Label(screen._productIconInfiniBATCH, GUILayout.Width(450),GUILayout.Height(85));//  screen.banners,GUILayout.Width(120),GUILayout.Height(120));

                GUILayout.Label(
                    @"InfiniBATCH is a massive optimization framework, using 3D imposters of objects and batching them for optimization so millions of those can be visible on camera even on older hardware.
The  sytem will be available as a discounted upgrade (more than 50%) from the Environment Building Bundle.",
                    screen.TextStyle
                );

                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniBATCH video", "https://www.youtube.com/watch?v=DTgFGQBnBP0"
                     );
                }
                 using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniBATCH used in the upcoming InfiniGRASS STUDIO Pro", "https://youtu.be/MfyXXbvsVm0"
                     );
                }
                 using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniBATCH Forum", "https://forum.unity.com/threads/infinibatch-batched-3d-impostors-for-massive-number-of-far-objects-with-minor-performance-impact.1467317/"
                     );
                }                 
            }),

            //v0.1b
            new ChangeMainViewButton("InfiniSNOW (Coming Soon)", (screen) =>
            {
                 //v0.1a
                GUILayout.Label(screen._productIconInfiniSNOW, GUILayout.Width(450),GUILayout.Height(85));//  screen.banners,GUILayout.Width(120),GUILayout.Height(120));

                GUILayout.Label(
                    @"InfiniSNOW Global Snow & Rain is a new system for applying snow and rain effects on surfaces as a global image effect, to objects in scene in selected layers, without
changes in object materials. The system also supports screen space rain, besides rain running on objects and rain drop ripples.",
                    screen.TextStyle
                );

                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniSNOW Screen Rain with Sky Master ULTIMATE", "https://www.youtube.com/watch?v=1YmnDI3j4Ao"
                     );
                }
                //  GUILayout.Label(
                //    @"InfiniRIVER can be used with two main fluid solutions, one terrain wide for big dynamic bodies of water and one local for detailed vortexes.",
                //    screen.TextStyle
                //);

                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniSNOW on city scape with Sky Master ULTIMATE clouds", "https://www.youtube.com/watch?v=Bkt2azNL7b8"
                     );
                }
                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniSNOW advanced parallax effect", "https://www.youtube.com/watch?v=UkkkvvOAofA"
                     );
                }
                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniSNOW rain occlusion", "https://www.youtube.com/watch?v=GVscJgdWukQ"
                     );
                }
                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniSNOW select objects by layer", "https://www.youtube.com/watch?v=MWvOgzs9QjE"
                     );
                }
                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniSNOW rain running on objects", "https://www.youtube.com/watch?v=cEVKumqu69A"
                     );
                }
                GUILayout.BeginHorizontal();
                 using (LayoutHelper.LabelWidth(100))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("InfiniSNOW Unity Forum Thread", "https://forum.unity.com/threads/infinisnow-global-screen-space-snow-and-rain-parallax-rocks-ice-and-much-more.1433782/"
                     );
                }
                //    using (LayoutHelper.LabelWidth(100))
                //{
                //    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                //    ProductPreferenceBase.RenderURL("InfiniRIVER Asset Store Page", "https://assetstore.unity.com/packages/tools/terrain/infiniriver-fluid-based-water-simulator-205568"
                //     );
                //}
                GUILayout.EndHorizontal();
            }),
            //v0.1b
            new ChangeMainViewButton("InfiniSPLAT (Coming Soon)", (screen) =>
            {
                GUILayout.Label(screen._productIconInfiniSPLAT, GUILayout.Width(450),GUILayout.Height(85));//  screen.banners,GUILayout.Width(120),GUILayout.Height(120));

                GUILayout.Label(
                    @"InfiniSPLAT is the new ARTnGAME asset for advanced Terrain splat map shading and generation. The system allows splat map in a single pass and uses the Shader Graph for easy extension and control of the various modules.",
                    screen.TextStyle
                );

                using (LayoutHelper.LabelWidth(200))
                {
                    ProductPreferenceBase.RenderURL("InfiniSPLAT Showcase Videos Playlist", "https://www.youtube.com/watch?v=fhjlH46PQPk&list=PLJQvoQM6t9Gf4A2eob5n-ok9AfLMkIH20&index=1"
                     );
                }                

                 GUILayout.Label(
                    @"InfiniSPLAT will soon be at closed Beta for Sky Master ULTIMATE and Environment Building Bundle users.",
                    screen.TextStyle
                );

                GUILayout.BeginHorizontal();
                 using (LayoutHelper.LabelWidth(100))
                {ProductPreferenceBase.RenderURL("InfiniSPLAT Unity Forum Thread", "https://forum.unity.com/threads/infinisplat-shader-graph-based-advanced-terrain-splat-mapping-and-shading.1386933/"
                     );
                }
                GUILayout.EndHorizontal();
            }),

            //v0.1b
            new ChangeMainViewButton("PANGAEA (Coming Soon)", (screen) =>
            {
                 //v0.1a
                GUILayout.Label(screen._productIconPANGAEA, GUILayout.Width(450),GUILayout.Height(85));//  screen.banners,GUILayout.Width(120),GUILayout.Height(120));

                GUILayout.Label(
                    @"PANGEA is the new ARTnGAME asset for Terrain generation based on a GPU enabled fluid simulator. The module presents a new way of sculpting of terrains using a real time fluid solution fully controllable by the user. The system allows for real time manipulation and generation of effects like erosion and land sculpting, and is working directly on GPU for maximum performance. A spline system is used to curve roads and rivers on the terrain, or place mountains and define terrain regions for editing.",
                    screen.TextStyle
                );

                using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("PANGAEA Tutorial and Showcase Videos Playlist", "https://www.youtube.com/watch?v=-P6GE5t3LZA&list=PLJQvoQM6t9GcQCSJtNtgP560BH_-dKy3O&index=1"
                     );
                }

                  GUILayout.Label(
                    @"PANGAEA supports multi mode editing, which includes a Non Destructive method of terrain generation.",
                    screen.TextStyle
                );

                  using (LayoutHelper.LabelWidth(200))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("PANGAEA Non Destructive workflow Video", "https://www.youtube.com/watch?v=B_9_MoRzPfg"
                     );
                }

                 GUILayout.Label(
                    @"PANGAEA is in closed Beta for InfiniGRASS and Sky Master ULTIMATE users.",
                    screen.TextStyle
                );

                GUILayout.BeginHorizontal();
                 using (LayoutHelper.LabelWidth(100))
                {
                    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                    ProductPreferenceBase.RenderURL("PANGAEA Unity Forum Thread", "https://forum.unity.com/threads/pangaea-next-gen-dynamic-fast-terrain-creation-using-gpu-fluid-simulation-and-spline-systems.1027885/"
                     );
                }
                //    using (LayoutHelper.LabelWidth(100))
                //{
                //    //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                //    ProductPreferenceBase.RenderURL("Oceanis Asset Store Page", "Coming Soon"
                //     );
                //}
                GUILayout.EndHorizontal();
            }),
            ////// end list items
        }),
        //new GuiSection("Launch Demo", new List<ClickableElement>
        //{
        //    new LaunchSceneButton("XR Toolkit", (s) => GetXrToolkingDemoScenePath())
        //})
    };

        private static readonly GuiSection TopSection = new GuiSection("Support", new List<ClickableElement>
        {
            //new OpenUrlButton("Documentation", $"{BaseUrl}/{WebSafeProjectId}/documentation"),
            new OpenUrlButton("Unity Forum", $"https://forum.unity.com/threads/67-off-offer-sky-master-hdrp-urp-mobile-sky-ocean-volume-clouds-lighting-weather-realgi.280612/"),
            new OpenUrlButton("Email", $"mailto:artengames@gmail.com"),
            new OpenUrlButton("Discord", $"https://discord.gg/X6fX6J5")
        }
        );

        private static readonly ScrollViewGuiSection MainContentSection = new ScrollViewGuiSection(
            "", (screen) =>
            {
                GenerateCommonWelcomeText(ProductName, screen);

                GUILayout.Label("Latest News and Offers:", screen.LabelStyle);
                GUILayout.Label(
                   @"
Sky Master ULTIMATE can be upgraded to all other major ARTnGAME 
assets with big (more than 50%) discount.

Sky Master ULTIMATE UPR Beta 25 and HDRP Beta 16 are available to 
all Sky Master ULTIMATE assets (Standard Pipeline system) users for 
download in Google Drive on PM request. The Beta versions are being 
developed on Unity 2021.3 LTS. InfiniGRASS URP Beta 2.0 and Oceanis 
Standard Pipeline Beta v0.3f1 are also available.

Please provide the invoice or order number in a personal message 
to be eligible for the Beta phase of the new URP and HDRP upcoming 
assets. Also must have downloaded Sky Master ULTIMATE or InfiniGRASS 
store versions, it is not needed to download fully of install them.");
                            
                using (LayoutHelper.LabelWidth(200))
                {
                //ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
                ProductPreferenceBase.RenderURL("ARTnGAME Asset Store", "https://assetstore.unity.com/publishers/6503"
                     );
                }

            //GUILayout.Label("https://assetstore.unity.com/publishers/6503", screen.LabelStyle);
            //using (LayoutHelper.LabelWidth(220))
            //{
            //    ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.EnableXrToolkitIntegrationPreferenceDefinition);
            //    ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.EnableVrtkIntegrationPreferenceDefinition);
            //    ProductPreferenceBase.RenderGuiAndPersistInput(WelcomeScreenPreferences.ShaderModePreferenceDefinition);
            //}
        }
        );

        private static readonly GuiSection BottomSection = new GuiSection(
            "Can I ask for a quick favour?",
            $"I'd be great help if you could spend few minutes to leave a review on:",
            new List<ClickableElement>
            {
            new OpenUrlButton(" ARTnGAME Unity Asset Store", $"https://assetstore.unity.com/publishers/6503"),
            }
        );

        private static readonly ScrollViewGuiSection LastUpdateSection = new ScrollViewGuiSection(
            "New Update", (screen) =>
            {
                GUILayout.Label(screen.LastUpdateText, screen.BoldTextStyle, GUILayout.ExpandHeight(true));
            }
        );

        #endregion
        private static string GetXrToolkingDemoScenePath()
        {
            var demoScene = AssetDatabase.FindAssets("t:Scene XRToolkitDemoScene").FirstOrDefault();
            return demoScene != null ? AssetDatabase.GUIDToAssetPath(demoScene) : null;
        }

        //Following code is required, please do not remove or amend
        #region RequiredSetupCode

        private static string WebSafeProjectId => Uri.EscapeDataString(ProjectId);
        public const string BaseUrl = "https://www.artengame.com";
        public static string GenerateGetUpdatesUrl(string userId, string versionId)
        {
            if (!IsUsageAnalyticsAndCommunicationsEnabled) return string.Empty;
            return $"{BaseUrl}/updates/{AnalyticsVerificationToken}/{WebSafeProjectId}/{versionId}/{userId}";
        }
        private static int RightColumnWidth => (int)_WindowSizePx.x - LeftSections.First().WidthPx - 15;
        public override string WindowTitle { get; } = ProductName;
        public override Vector2 WindowSizePx { get; } = _WindowSizePx;

        static WelcomeScreen()
        {
            foreach (var section in LeftSections)
                section.InitializeWidthPx(LeftColumnWidth);

            TopSection.InitializeWidthPx(RightColumnWidth);
            BottomSection.InitializeWidthPx(RightColumnWidth);
        }

        [MenuItem(StartWindowMenuItemPath, false, 1999)]
        public static void Init()
        {
            OpenWindow<WelcomeScreen>(ProductName, _WindowSizePx + new Vector2(10,0));
        }

        public void OnEnable()
        {


            OnEnableCommon(ProjectIconName);

            OnEnableCommonENVIRO(EnvironmentBuildingBundleIconName);

            OnEnableCommonGIPROXY(GIProxyIconName);
            OnEnableCommonSKYMASTER(SkyMasterIconName);
            OnEnableCommonINFINIGRASS(InfiniGRASSIconName);
            OnEnableCommonORION(OrionIconName);
            OnEnableCommonETHEREAL(EtherealIconName);

            OnEnableCommonINFINITREE(InfiniTREEIconName);
            OnEnableCommonINFINICLOUD(InfiniCLOUDIconName);
            OnEnableCommonINFINIRIVER(InfiniRIVERIconName);
            OnEnableCommonIvySTUDIO(IvySTUDIOName);
            OnEnableCommonOCEANIS(OCEANISIconName);
            OnEnableCommonPDM(PDMIconName);

            OnEnableCommonGIBLI(GIBLIIconName);
            OnEnableCommonPANGAEA(PANGAEAIconName);

            OnEnableCommonLUMINA(LUMINAIconName);
            OnEnableCommonLIQUA(LIQUAIconName);

            OnEnableCommonINFINISPLAT(InfiniSPLATIconName);

            OnEnableCommonTEM(TEMIconName);

            OnEnableCommonInfiniSNOW(InfiniSNOWIconName);
            OnEnableCommonInfiniBATCH(InfiniBATCHIconName);

            OnEnableCommonInfiniTUNES(InfiniTUNESIconName);

            OnEnableCommonGLAMOR(GLAMORIconName);
        }
        protected void OnEnableCommonGLAMOR(string productIconName)
        {
            if (!string.IsNullOrEmpty(productIconName) && base._productIconGLAMOR == null)
            {
                var iconGuid = AssetDatabase.FindAssets(productIconName).FirstOrDefault();
                if (!string.IsNullOrEmpty(iconGuid))
                {
                    _productIconGLAMOR = new GUIContent(AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(iconGuid)));
                }
            }
        }
        protected void OnEnableCommonInfiniTUNES(string productIconName)
        {
            if (!string.IsNullOrEmpty(productIconName) && base._productIconInfiniTUNES == null)
            {
                var iconGuid = AssetDatabase.FindAssets(productIconName).FirstOrDefault();
                if (!string.IsNullOrEmpty(iconGuid))
                {
                    _productIconInfiniTUNES = new GUIContent(AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(iconGuid)));
                }
            }
        }
        protected void OnEnableCommonInfiniBATCH(string productIconName)
        {
            if (!string.IsNullOrEmpty(productIconName) && base._productIconInfiniBATCH == null)
            {
                var iconGuid = AssetDatabase.FindAssets(productIconName).FirstOrDefault();
                if (!string.IsNullOrEmpty(iconGuid))
                {
                    _productIconInfiniBATCH = new GUIContent(AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(iconGuid)));
                }
            }
        }
        protected void OnEnableCommonInfiniSNOW(string productIconName)
        {
            if (!string.IsNullOrEmpty(productIconName) && base._productIconInfiniSNOW == null)
            {
                var iconGuid = AssetDatabase.FindAssets(productIconName).FirstOrDefault();
                if (!string.IsNullOrEmpty(iconGuid))
                {
                    _productIconInfiniSNOW = new GUIContent(AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(iconGuid)));
                }
            }
        }
        protected void OnEnableCommonTEM(string productIconName)
        {
            if (!string.IsNullOrEmpty(productIconName) && base._productIconTem == null)
            {
                var iconGuid = AssetDatabase.FindAssets(productIconName).FirstOrDefault();
                if (!string.IsNullOrEmpty(iconGuid))
                {
                    _productIconTem = new GUIContent(AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(iconGuid)));
                }
            }
        }
        protected void OnEnableCommonENVIRO(string productIconName)
        {
            if (!string.IsNullOrEmpty(productIconName) && base._productIconEnviro == null)
            {
                var iconGuid = AssetDatabase.FindAssets(productIconName).FirstOrDefault();
                if (!string.IsNullOrEmpty(iconGuid))
                {
                    _productIconEnviro = new GUIContent(AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(iconGuid)));
                }
            }
        }

        protected void OnEnableCommonINFINISPLAT(string productIconName)
        {
            if (!string.IsNullOrEmpty(productIconName) && base._productIconInfiniSPLAT == null)
            {
                var iconGuid = AssetDatabase.FindAssets(productIconName).FirstOrDefault();
                if (!string.IsNullOrEmpty(iconGuid))
                {
                    _productIconInfiniSPLAT = new GUIContent(AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(iconGuid)));
                }
            }
        }
        protected void OnEnableCommonETHEREAL(string productIconName)
        {
            if (!string.IsNullOrEmpty(productIconName) && base._productIconETHEREAL == null)
            {
                var iconGuid = AssetDatabase.FindAssets(productIconName).FirstOrDefault();
                if (!string.IsNullOrEmpty(iconGuid))
                {
                    _productIconETHEREAL = new GUIContent(AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(iconGuid)));
                }
            }
        }
        protected void OnEnableCommonORION(string productIconName)
        {
            if (!string.IsNullOrEmpty(productIconName) && base._productIconORION == null)
            {
                var iconGuid = AssetDatabase.FindAssets(productIconName).FirstOrDefault();
                if (!string.IsNullOrEmpty(iconGuid))
                {
                    _productIconORION = new GUIContent(AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(iconGuid)));
                }
            }
        }
        protected void OnEnableCommonGIPROXY(string productIconName)
        {
            if (!string.IsNullOrEmpty(productIconName) && base._productIconGIProxy == null)
            {
                var iconGuid = AssetDatabase.FindAssets(productIconName).FirstOrDefault();
                if (!string.IsNullOrEmpty(iconGuid))
                {
                    _productIconGIProxy = new GUIContent(AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(iconGuid)));
                }
            }
        }
        protected void OnEnableCommonSKYMASTER(string productIconName)
        {
            if (!string.IsNullOrEmpty(productIconName) && base._productIconSkyMaster == null)
            {
                var iconGuid = AssetDatabase.FindAssets(productIconName).FirstOrDefault();
                if (!string.IsNullOrEmpty(iconGuid))
                {
                    _productIconSkyMaster = new GUIContent(AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(iconGuid)));
                }
            }
        }
        protected void OnEnableCommonINFINIGRASS(string productIconName)
        {
            if (!string.IsNullOrEmpty(productIconName) && base._productIconInfiniGRASS == null)
            {
                var iconGuid = AssetDatabase.FindAssets(productIconName).FirstOrDefault();
                if (!string.IsNullOrEmpty(iconGuid))
                {
                    _productIconInfiniGRASS = new GUIContent(AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(iconGuid)));
                }
            }
        }

        //NEW1
        protected void OnEnableCommonINFINITREE(string productIconName)
        {
            if (!string.IsNullOrEmpty(productIconName) && base._productIconInfiniTREE == null)
            {
                var iconGuid = AssetDatabase.FindAssets(productIconName).FirstOrDefault();
                if (!string.IsNullOrEmpty(iconGuid))
                {
                    _productIconInfiniTREE = new GUIContent(AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(iconGuid)));
                }
            }
        }
        protected void OnEnableCommonINFINICLOUD(string productIconName)
        {
            if (!string.IsNullOrEmpty(productIconName) && base._productIconInfniCLOUD == null)
            {
                var iconGuid = AssetDatabase.FindAssets(productIconName).FirstOrDefault();
                if (!string.IsNullOrEmpty(iconGuid))
                {
                    _productIconInfniCLOUD = new GUIContent(AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(iconGuid)));
                }
            }
        }
        protected void OnEnableCommonINFINIRIVER(string productIconName)
        {
            if (!string.IsNullOrEmpty(productIconName) && base._productIconInfiniRIVER == null)
            {
                var iconGuid = AssetDatabase.FindAssets(productIconName).FirstOrDefault();
                if (!string.IsNullOrEmpty(iconGuid))
                {
                    _productIconInfiniRIVER = new GUIContent(AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(iconGuid)));
                }
            }
        }
        protected void OnEnableCommonIvySTUDIO(string productIconName)
        {
            if (!string.IsNullOrEmpty(productIconName) && base._productIconIvyStudio == null)
            {
                var iconGuid = AssetDatabase.FindAssets(productIconName).FirstOrDefault();
                if (!string.IsNullOrEmpty(iconGuid))
                {
                    _productIconIvyStudio = new GUIContent(AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(iconGuid)));
                }
            }
        }
        protected void OnEnableCommonOCEANIS(string productIconName)
        {
            if (!string.IsNullOrEmpty(productIconName) && base._productIconOceanis == null)
            {
                var iconGuid = AssetDatabase.FindAssets(productIconName).FirstOrDefault();
                if (!string.IsNullOrEmpty(iconGuid))
                {
                    _productIconOceanis = new GUIContent(AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(iconGuid)));
                }
            }
        }
        protected void OnEnableCommonPDM(string productIconName)
        {
            if (!string.IsNullOrEmpty(productIconName) && base._productIconParticleDynamicMagic == null)
            {
                var iconGuid = AssetDatabase.FindAssets(productIconName).FirstOrDefault();
                if (!string.IsNullOrEmpty(iconGuid))
                {
                    _productIconParticleDynamicMagic = new GUIContent(AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(iconGuid)));
                }
            }
        }
        protected void OnEnableCommonLUMINA(string productIconName)
        {
            if (!string.IsNullOrEmpty(productIconName) && base._productIconLUMINA == null)
            {
                var iconGuid = AssetDatabase.FindAssets(productIconName).FirstOrDefault();
                if (!string.IsNullOrEmpty(iconGuid))
                {
                    _productIconLUMINA = new GUIContent(AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(iconGuid)));
                }
            }
        }
        protected void OnEnableCommonLIQUA(string productIconName)
        {
            if (!string.IsNullOrEmpty(productIconName) && base._productIconLIQUA == null)
            {
                var iconGuid = AssetDatabase.FindAssets(productIconName).FirstOrDefault();
                if (!string.IsNullOrEmpty(iconGuid))
                {
                    _productIconLIQUA = new GUIContent(AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(iconGuid)));
                }
            }
        }
        protected void OnEnableCommonPANGAEA(string productIconName)
        {
            if (!string.IsNullOrEmpty(productIconName) && base._productIconPANGAEA == null)
            {
                var iconGuid = AssetDatabase.FindAssets(productIconName).FirstOrDefault();
                if (!string.IsNullOrEmpty(iconGuid))
                {
                    _productIconPANGAEA = new GUIContent(AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(iconGuid)));
                }
            }
        }
        protected void OnEnableCommonGIBLI(string productIconName)
        {
            if (!string.IsNullOrEmpty(productIconName) && base._productIconGIBLI == null)
            {
                var iconGuid = AssetDatabase.FindAssets(productIconName).FirstOrDefault();
                if (!string.IsNullOrEmpty(iconGuid))
                {
                    _productIconGIBLI = new GUIContent(AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(iconGuid)));
                }
            }
        }


        public void OnGUI()
        {
            RenderGUI(LeftSections, TopSection, BottomSection, MainContentSection);
        }

        #endregion
    }

    public class WelcomeScreenPreferences : ProductPreferenceBase
    {
        #region CustomisablePerProduct

        public static string BuildSymbol_EnableXrToolkit = "INTEGRATIONS_XRTOOLKIT";
        public static string BuildSymbol_EnableVRTK = "INTEGRATIONS_VRTK";


        public static readonly ToggleProjectEditorPreferenceDefinition EnableXrToolkitIntegrationPreferenceDefinition = new ToggleProjectEditorPreferenceDefinition(
            "Enable Unity XR Toolkit integration", "XRToolkitIntegrationEnabled", true,
            (newValue, oldValue) =>
            {
            //BuildDefineSymbolManager.SetBuildDefineSymbolState(BuildSymbol_EnableXrToolkit, (bool)newValue);
        });

        public static readonly ToggleProjectEditorPreferenceDefinition EnableVrtkIntegrationPreferenceDefinition = new ToggleProjectEditorPreferenceDefinition(
            "Enable VRTK integration", "VRTKIntegrationEnabled", false,
            (newValue, oldValue) =>
            {
            //BuildDefineSymbolManager.SetBuildDefineSymbolState(BuildSymbol_EnableVRTK, (bool)newValue);
        });

        public static readonly EnumProjectEditorPreferenceDefinition ShaderModePreferenceDefinition = new EnumProjectEditorPreferenceDefinition("Shaders",
            "ShadersMode",
            ShadersMode.HDRP,
            typeof(ShadersMode),
            (newValue, oldValue) =>
            {
                if (oldValue == null) oldValue = default(ShadersMode);

                var newShaderModeValue = (ShadersMode)newValue;
                var oldShaderModeValue = (ShadersMode)oldValue;

                if (newShaderModeValue != oldShaderModeValue)
                {
                //SetCommonMaterialsShader(newShaderModeValue);
            }
            }
        );

        public static void SetCommonMaterialsShader(ShadersMode newShaderModeValue)
        {
            var rootToolFolder = AssetPathResolver.GetAssetFolderPathRelativeToScript(ScriptableObject.CreateInstance(typeof(WelcomeScreen)), 1);
            var assets = AssetDatabase.FindAssets("t:Material", new[] { rootToolFolder });

            try
            {
                Shader shaderToUse = null;
                switch (newShaderModeValue)
                {
                    case ShadersMode.HDRP: shaderToUse = Shader.Find("HDRP/Lit"); break;
                    case ShadersMode.URP: shaderToUse = Shader.Find("Universal Render Pipeline/Lit"); break;
                    case ShadersMode.Surface: shaderToUse = Shader.Find("Standard"); break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                foreach (var guid in assets)
                {
                    var material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
                    if (material.shader.name != shaderToUse.name)
                    {
                        material.shader = shaderToUse;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Shader does not exist: {ex.Message}");
            }
        }

        public enum ShadersMode
        {
            HDRP,
            URP,
            Surface
        }

        //Add all preference options that you'd like to be persisted for project
        public static List<ProjectEditorPreferenceDefinitionBase> PreferenceDefinitions = new List<ProjectEditorPreferenceDefinitionBase>()
    {
        CreateDefaultShowOptionPreferenceDefinition(),
        EnableXrToolkitIntegrationPreferenceDefinition,
        EnableVrtkIntegrationPreferenceDefinition,
        ShaderModePreferenceDefinition
    };

        #endregion


        #region RequiredSetupCode
        private static bool PrefsLoaded = false;


#if UNITY_2019_1_OR_NEWER
        [SettingsProvider]
        public static SettingsProvider ImpostorsSettings()
        {
            return GenerateProvider(WelcomeScreen.ProductName, WelcomeScreen.ProductKeywords, PreferencesGUI);
        }

#else
	[PreferenceItem(ProductName)]
#endif
        public static void PreferencesGUI()
        {
            if (!PrefsLoaded)
            {
                LoadDefaults(PreferenceDefinitions);
                PrefsLoaded = true;
            }

            RenderGuiCommon(PreferenceDefinitions);
        }

        #endregion
    }

    [InitializeOnLoad]
    public class WelcomeScreenInitializer : WelcomeScreenInitializerBase
    {
        #region CustomisablePerProduct

        public static void RunOnWindowOpened(bool isFirstRun)
        {
            AutoDetectAndSetShaderMode();
        }

        private static void AutoDetectAndSetShaderMode()
        {
            //var usedShaderMode = WelcomeScreenPreferences.ShadersMode.Surface;
            //var renderPipelineAsset = GraphicsSettings.renderPipelineAsset;
            //if (renderPipelineAsset == null)
            //{
            //    usedShaderMode = WelcomeScreenPreferences.ShadersMode.Surface;
            //}
            //else if (renderPipelineAsset.GetType().Name.Contains("HDRenderPipelineAsset"))
            //{
            //    usedShaderMode = WelcomeScreenPreferences.ShadersMode.HDRP;
            //}
            //else if (renderPipelineAsset.GetType().Name.Contains("UniversalRenderPipelineAsset"))
            //{
            //    usedShaderMode = WelcomeScreenPreferences.ShadersMode.URP;
            //}

            //WelcomeScreenPreferences.ShaderModePreferenceDefinition.SetEditorPersistedValue(usedShaderMode);
            //WelcomeScreenPreferences.SetCommonMaterialsShader(usedShaderMode);
        }

        #endregion


        #region RequiredSetupCode

        static WelcomeScreenInitializer()
        {
            var userId = ProductPreferenceBase.CreateDefaultUserIdDefinition(WelcomeScreen.ProjectId).GetEditorPersistedValueOrDefault().ToString();

            HandleUnityStartup(WelcomeScreen.Init, WelcomeScreen.GenerateGetUpdatesUrl(userId, WelcomeScreen.VersionId), RunOnWindowOpened);
        }

        #endregion
    }
}