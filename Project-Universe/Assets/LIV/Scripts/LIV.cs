#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
#if UNITY_2017_2_OR_NEWER
using UnityEngine.XR;
#endif

namespace LIV.SDK.Unity
{
    [AddComponentMenu("LIV/LIV")]
    public class LIV : MonoBehaviour
    {
#if UNITY_EDITOR
        [Help(
            "The origin of tracked space.\n" +
            "This is the object that you use to move the player around."
        )]
#endif
        [Tooltip("If unpopulated, we'll use the parent transform.")]
        public Transform TrackedSpaceOrigin;

        [Space]
#if UNITY_EDITOR
        [Help(
            "Set this to the camera used to render the HMD's point of view."
        )]
#endif
        public Camera HMDCamera;

        [Space]
#if UNITY_EDITOR
        [Help(
            "If you're using Unity's standard effects and they're interfering with MR rendering, check this box.",
            MessageType.Warning
        )]
#endif
        public bool DisableStandardAssets;

        [Space]
#if UNITY_EDITOR
        [Help(
            "By default, we'll show everything on the spectator camera. If you want to disable certain objects from showing, update this mask property."
        )]
#endif
        public LayerMask SpectatorLayerMask = ~0;

        public Transform Origin
        {
            get
            {
                return TrackedSpaceOrigin == null ? transform.parent : TrackedSpaceOrigin;
            }
        }

        protected bool WasActive;

        SharedTextureProtocol _sharedTextureProtocol;
        ExternalCamera _externalCamera;
        MixedRealityRender _mixedRealityRender;

        bool _wasSteamVRExternalCameraActive;

        void OnEnable()
        {
            // XR Settings

#if UNITY_2017_2_OR_NEWER

            if (!XRSettings.enabled)
            {
                Debug.LogWarning("LIV: Project is not in XR mode! Disabling.");
                enabled = false;
                return;
            }

            if (XRSettings.loadedDeviceName != "OpenVR")
            {
                Debug.LogWarningFormat(
                    "LIV: Unity is currently using {0} for XR support. Please switch to OpenVR to enable the LIV SDK. Disabling.",
                    XRSettings.loadedDeviceName
                );
                enabled = false;
                return;
            }

            Debug.Log("LIV: Unity is using OpenVR, setting up...");

            if (SteamVRCompatibility.IsAvailable)
                Debug.Log("LIV: SteamVR asset found!");

#else

            if (!SteamVRCompatibility.IsAvailable) {
                Debug.LogError("LIV: SteamVR asset not found. In Unity versions earlier than 2017.2, SteamVR is required.");
                enabled = false;
                return;
            }

#endif

            // Configuration checks

            if (HMDCamera == null)
            {
                Debug.LogError("LIV: HMD Camera is a required parameter!");
                enabled = false;
                return;
            }

            if (SpectatorLayerMask == 0)
                Debug.LogWarning("LIV: The spectator layer mask is set to not show anything. Is this right?");

            Debug.Log("LIV: Ready! Waiting for compositor.");
        }

        void OnDisable()
        {
            Debug.Log("LIV: Disabled, cleaning up.");

            if (WasActive)
                OnCompositorDeactivated();
        }

        void Update()
        {
            if (SharedTextureProtocol.IsActive && !WasActive)
                OnCompositorActivated();

            if (!SharedTextureProtocol.IsActive && WasActive)
                OnCompositorDeactivated();
        }

        void OnCompositorActivated()
        {
            WasActive = true;
            Debug.Log("LIV: Compositor connected, setting up MR!");

            CreateSharedTextureProtocol();
            CreateExternalCamera();
            CreateMixedRealityRender();

            if (SteamVRCompatibility.IsAvailable)
            {
                var steamVRExternalCamera = GetComponent(SteamVRCompatibility.SteamVRExternalCamera);

                if (steamVRExternalCamera != null)
                {
                    _wasSteamVRExternalCameraActive = steamVRExternalCamera.gameObject.activeInHierarchy;
                    steamVRExternalCamera.gameObject.SetActive(false);
                }
            }
        }

        void OnCompositorDeactivated()
        {
            WasActive = false;
            Debug.Log("LIV: Compositor disconnected, cleaning up.");

            DestroySharedTextureProtocol();
            DestroyMixedRealityRender();
            DestroyExternalCamera();

            if (SteamVRCompatibility.IsAvailable)
            {
                var steamVRExternalCamera = GetComponent(SteamVRCompatibility.SteamVRExternalCamera);

                if (steamVRExternalCamera != null)
                    steamVRExternalCamera.gameObject.SetActive(_wasSteamVRExternalCameraActive);
            }
        }

        void CreateSharedTextureProtocol()
        {
            _sharedTextureProtocol = gameObject.AddComponent<SharedTextureProtocol>();
        }
        void DestroySharedTextureProtocol()
        {
            if (_sharedTextureProtocol != null)
            {
                Destroy(_sharedTextureProtocol);

                _sharedTextureProtocol = null;
            }
        }

        void CreateExternalCamera()
        {
            var externalCameraObject = new GameObject("LIV Camera Reference");
            externalCameraObject.transform.parent = Origin;
            _externalCamera = externalCameraObject.AddComponent<ExternalCamera>();
        }
        void DestroyExternalCamera()
        {
            if (_externalCamera != null)
            {
                Destroy(_externalCamera.gameObject);

                _externalCamera = null;
            }
        }

        void CreateMixedRealityRender()
        {
            HMDCamera.enabled = false;
            HMDCamera.gameObject.SetActive(false);
            var hmdCameraClone = Instantiate(HMDCamera.gameObject);
            HMDCamera.gameObject.SetActive(true);
            HMDCamera.enabled = true;

            hmdCameraClone.name = "LIV Camera";

            // Remove all children from camera clone.
            while (hmdCameraClone.transform.childCount > 0)
                DestroyImmediate(hmdCameraClone.transform.GetChild(0).gameObject);

            DestroyImmediate(hmdCameraClone.GetComponent("AudioListener"));
            DestroyImmediate(hmdCameraClone.GetComponent("MeshCollider"));

            if (SteamVRCompatibility.IsAvailable)
            {
                DestroyImmediate(hmdCameraClone.GetComponent(SteamVRCompatibility.SteamVRCamera));
                DestroyImmediate(hmdCameraClone.GetComponent(SteamVRCompatibility.SteamVRFade));
            }

            _mixedRealityRender = hmdCameraClone.AddComponent<MixedRealityRender>();
            hmdCameraClone.transform.parent = _externalCamera.transform;

            hmdCameraClone.SetActive(true);

            _mixedRealityRender.Setup(this);
        }
        void DestroyMixedRealityRender()
        {
            if (_mixedRealityRender != null)
            {
                Destroy(_mixedRealityRender.gameObject);

                _mixedRealityRender = null;
            }
        }
    }
}
