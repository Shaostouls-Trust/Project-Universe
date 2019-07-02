using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

namespace LIV.SDK.Unity
{
    [AddComponentMenu("LIV/MixedRealityRender")]
    public class MixedRealityRender : MonoBehaviour
    {
        bool _isSetup;

        LIV _liv;

        Camera _mrCamera;
        Transform _hmd;

        GameObject _clipQuad;
        Material _clipMaterial;
        Material _blitMaterial;

        RenderTexture _compositeTexture;
        RenderTexture _foregroundTexture;
        RenderTexture _backgroundTexture;

        public void Setup(LIV liv)
        {
            _liv = liv;

            _mrCamera = GetComponent<Camera>();
            _mrCamera.rect = new Rect(0, 0, 1, 1);
            _mrCamera.depth = float.MaxValue;
#if UNITY_5_4_OR_NEWER
            _mrCamera.stereoTargetEye = StereoTargetEyeMask.None;
#endif
            _mrCamera.useOcclusionCulling = false;
            _mrCamera.enabled = false;

            _clipMaterial = new Material(Shader.Find("Custom/LIV_ClearBackground"));
            _blitMaterial = new Material(Shader.Find("Custom/LIV_Blit"));

            CreateClipQuad();

            _isSetup = true;
        }

        void CreateClipQuad()
        {
            _clipQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            _clipQuad.name = "ClipQuad";
            Destroy(_clipQuad.GetComponent<MeshCollider>());
            _clipQuad.transform.parent = transform;

            var clipRenderer = _clipQuad.GetComponent<MeshRenderer>();
            clipRenderer.material = _clipMaterial;
            clipRenderer.shadowCastingMode = ShadowCastingMode.Off;
            clipRenderer.receiveShadows = false;
#if UNITY_5_4_OR_NEWER
            clipRenderer.lightProbeUsage = LightProbeUsage.Off;
#endif
            clipRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;

            var clipTransform = _clipQuad.transform;
            clipTransform.localScale = new Vector3(1000.0f, 1000.0f, 1.0f);
            clipTransform.localRotation = Quaternion.identity;

            _clipQuad.SetActive(false);
        }

        void UpdateCamera()
        {
            _mrCamera.fieldOfView = Calibration.FieldOfVision;
            _mrCamera.nearClipPlane = Calibration.NearClip;
            _mrCamera.farClipPlane = Calibration.FarClip;
            _mrCamera.cullingMask = _liv.SpectatorLayerMask;

            _hmd = _liv.HMDCamera.transform;

            transform.localPosition = Calibration.PositionOffset;
            transform.localRotation = Calibration.RotationOffset;
            transform.localScale = Vector3.one;
        }

        void UpdateTextures()
        {
            var width = SharedTextureProtocol.TextureWidth;
            var compositeHeight = SharedTextureProtocol.TextureHeight;
            var height = SharedTextureProtocol.TextureHeight / 2;

            if (
                _compositeTexture == null ||
                _compositeTexture.width != width ||
                _compositeTexture.height != compositeHeight
                )
            {
                _compositeTexture = new RenderTexture(width, compositeHeight, 24, RenderTextureFormat.ARGB32);
                _compositeTexture.antiAliasing = 1;
            }

            if (
                _foregroundTexture == null ||
                _foregroundTexture.width != width ||
                _foregroundTexture.height != height
                )
            {
                _foregroundTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32)
                {
                    antiAliasing = 1,
                    wrapMode = TextureWrapMode.Clamp,
                    useMipMap = false,
                    anisoLevel = 0
                };
            }

            if (
                _backgroundTexture == null ||
                _backgroundTexture.width != width ||
                _backgroundTexture.height != height
            )
            {
                _backgroundTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32)
                {
                    antiAliasing = 1,
                    wrapMode = TextureWrapMode.Clamp,
                    useMipMap = false,
                    anisoLevel = 0
                };
            }
        }

        public float GetDistanceToHMD()
        {
            if (_hmd == null)
                return Calibration.NearClip;

            var offset = _mrCamera.transform;
            var forward = new Vector3(offset.forward.x, 0.0f, offset.forward.z).normalized;
            var hmdPos = _hmd.position + new Vector3(_hmd.forward.x, 0.0f, _hmd.forward.z).normalized *
                         Calibration.HMDOffset;

            var distance = -(new Plane(forward, hmdPos)).GetDistanceToPoint(offset.position);
            return distance;
        }

        void OrientClipQuad()
        {
            float dist = Mathf.Clamp(GetDistanceToHMD() + Calibration.NearOffset, Calibration.NearClip, Calibration.FarClip);
            var clipParent = _clipQuad.transform.parent;

            _clipQuad.transform.position = clipParent.position + clipParent.forward * dist;
            _clipQuad.transform.LookAt(new Vector3(
                _clipQuad.transform.parent.position.x,
                _clipQuad.transform.position.y,
                _clipQuad.transform.parent.position.z
            ));
        }

        void RenderNear()
        {
            // Disable standard assets if required.
            MonoBehaviour[] behaviours = null;
            bool[] wasBehaviourEnabled = null;
            if (_liv.DisableStandardAssets)
            {
                behaviours = _mrCamera.gameObject.GetComponents<MonoBehaviour>();
                wasBehaviourEnabled = new bool[behaviours.Length];
                for (var i = 0; i < behaviours.Length; i++)
                {
                    var behaviour = behaviours[i];
                    if (behaviour.enabled && behaviour.GetType().ToString().StartsWith("UnityStandardAssets."))
                    {
                        behaviour.enabled = false;
                        wasBehaviourEnabled[i] = true;
                    }
                }
            }

            var clearFlags = _mrCamera.clearFlags;
            var bgColor = _mrCamera.backgroundColor;
            _mrCamera.clearFlags = CameraClearFlags.Color;
            _mrCamera.backgroundColor = Color.clear;

            _clipQuad.SetActive(true);

            _mrCamera.targetTexture = _foregroundTexture;
            _foregroundTexture.DiscardContents(true, true);
            _mrCamera.Render();

            _clipQuad.SetActive(false);

            _mrCamera.clearFlags = clearFlags;
            _mrCamera.backgroundColor = bgColor;

            // Restore disabled behaviours.
            if (behaviours != null)
                for (var i = 0; i < behaviours.Length; i++)
                    if (wasBehaviourEnabled[i])
                        behaviours[i].enabled = true;
        }

        void RenderFar()
        {
            _mrCamera.targetTexture = _backgroundTexture;
            _backgroundTexture.DiscardContents(true, true);
            _mrCamera.Render();
        }

        void Composite()
        {
            _compositeTexture.DiscardContents(true, true);

            _blitMaterial.SetTexture("_NearTex", _foregroundTexture);
            _blitMaterial.SetTexture("_FarTex", _backgroundTexture);
            Graphics.Blit(null, _compositeTexture, _blitMaterial);

            SharedTextureProtocol.SetOutputTexture(_compositeTexture);
        }

        void OnEnable()
        {
            StartCoroutine(RenderLoop());
        }
        IEnumerator RenderLoop()
        {
            while (Application.isPlaying && enabled)
            {
                yield return new WaitForEndOfFrame();

                if (_isSetup && SharedTextureProtocol.IsActive)
                {
                    UpdateCamera();
                    UpdateTextures();

                    OrientClipQuad();
                    RenderNear();
                    RenderFar();
                    Composite();
                }
            }
        }
    }
}