using System;

namespace LIV.SDK.Unity
{
    static class SteamVRCompatibility
    {
        public static bool IsAvailable;

        public static Type SteamVRCamera;
        public static Type SteamVRExternalCamera;
        public static Type SteamVRFade;

        static SteamVRCompatibility()
        {
            IsAvailable = FindSteamVRAsset();
        }

        static bool FindSteamVRAsset()
        {
            if (SteamVRCamera == null) SteamVRCamera = Type.GetType("SteamVR_Camera", false);
            if (SteamVRCamera == null) SteamVRCamera = Type.GetType("Valve.VR.SteamVR_Camera", false);

            if (SteamVRExternalCamera == null) SteamVRExternalCamera = Type.GetType("SteamVR_ExternalCamera", false);
            if (SteamVRExternalCamera == null) SteamVRExternalCamera = Type.GetType("Valve.VR.SteamVR_ExternalCamera", false);

            if (SteamVRFade == null) SteamVRFade = Type.GetType("SteamVR_Fade", false);
            if (SteamVRFade == null) SteamVRFade = Type.GetType("Valve.VR.SteamVR_Fade", false);

            return SteamVRCamera != null &&
                   SteamVRExternalCamera != null &&
                   SteamVRFade != null;
        }
    }
}
