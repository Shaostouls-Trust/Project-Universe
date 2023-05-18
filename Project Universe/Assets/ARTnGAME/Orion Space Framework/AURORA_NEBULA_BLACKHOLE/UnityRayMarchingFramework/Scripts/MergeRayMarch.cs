using UnityEngine;
using System.Collections;
namespace Artngame.Orion.RaymarchedNebula
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class MergeRayMarch : MonoBehaviour
    {


        //v0.1 local lights
        public bool useLocalLights = false;
        public Vector4[] localLights;
        public Vector4[] localLightsPosition;
        public bool updateLocalLightsRunTime = false;
        public Vector4 _fogDensities = new Vector4(7, 1, 1, 1);
        public float localLightNoise = 1;
        public float localLightNoiseA = 1;

        public Material material;
        public Texture2D _NoiseTex;
        public Vector4 _LoopNum;


        private Camera myCamera;
        public  Camera cameraA //public new Camera cameraA
        {
            get
            {
                if (myCamera == null)
                {
                    myCamera = GetComponent<Camera>();
                }
                return myCamera;
            }
        }

        private Transform myCameraTransform;
        public Transform cameraTransform
        {
            get
            {
                if (myCameraTransform == null)
                {
                    myCameraTransform = cameraA.transform;
                }

                return myCameraTransform;
            }
        }


        private void Start()
        {
            if (material == null || material.shader == null || !material.shader.isSupported)
            {
                this.enabled = false;
                return;
            }

            //v0.1
            if (useLocalLights)
            {
                NebulaLightSM[] myItems = FindObjectsOfType(typeof(NebulaLightSM)) as NebulaLightSM[];
                if (myItems != null && myItems.Length > 0)
                {
                    localLights = new Vector4[myItems.Length];
                    localLightsPosition = new Vector4[myItems.Length];
                    //localLights
                    for (int i = 0; i < myItems.Length; i++)
                    {
                        localLights[i] = myItems[i].lightDefinition;
                        localLightsPosition[i] = myItems[i].lightPosition;
                    }
                }
            }
        }
        void OnEnable()
        {
            cameraA.depthTextureMode |= DepthTextureMode.Depth;
        }

        [ImageEffectOpaque]
        void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (material != null)
            {
                SetRay();
                material.SetTexture("_NoiseTex", _NoiseTex);
                material.SetVector("_LoopNum", _LoopNum);

                //v0.1
                material.SetVector("_fogDensities", _fogDensities);
                if (useLocalLights)
                {
                    if (updateLocalLightsRunTime)
                    {
                        NebulaLightSM[] myItems = FindObjectsOfType(typeof(NebulaLightSM)) as NebulaLightSM[];
                        if (myItems != null && myItems.Length > 0)
                        {
                            localLights = new Vector4[myItems.Length];
                            localLightsPosition = new Vector4[myItems.Length];
                            //localLights
                            for (int i = 0; i < myItems.Length; i++)
                            {
                                localLights[i] = myItems[i].lightDefinition;
                                localLightsPosition[i] = myItems[i].lightPosition;
                            }
                        }
                    }
                    if (localLights != null && localLights.Length > 0)
                    {
                        material.SetFloat("localLightNoise", localLightNoise);
                        material.SetFloat("localLightNoiseA", localLightNoiseA);
                        material.SetInt("_localLightsCount", localLights.Length);
                        material.SetVectorArray("_localLights", localLights);
                        material.SetVectorArray("_localLightsPositions", localLightsPosition);
                    }
                }

                Graphics.Blit(src, dest, material);
            }
            else
            {
                Graphics.Blit(src, dest);
            }
        }

        private void SetRay()
        {
            Matrix4x4 frustumCorners = Matrix4x4.identity;

            float fov = cameraA.fieldOfView;
            float near = cameraA.nearClipPlane;
            float aspect = cameraA.aspect;

            float halfHeight = near * Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
            Vector3 toRight = cameraTransform.right * halfHeight * aspect;
            Vector3 toTop = cameraTransform.up * halfHeight;

            Vector3 topLeft = cameraTransform.forward * near + toTop - toRight;
            float scale = topLeft.magnitude / near;

            topLeft.Normalize();
            topLeft *= scale;

            Vector3 topRight = cameraTransform.forward * near + toRight + toTop;
            topRight.Normalize();
            topRight *= scale;

            Vector3 bottomLeft = cameraTransform.forward * near - toTop - toRight;
            bottomLeft.Normalize();
            bottomLeft *= scale;

            Vector3 bottomRight = cameraTransform.forward * near + toRight - toTop;
            bottomRight.Normalize();
            bottomRight *= scale;

            frustumCorners.SetRow(0, bottomLeft);
            frustumCorners.SetRow(1, bottomRight);
            frustumCorners.SetRow(2, topRight);
            frustumCorners.SetRow(3, topLeft);

            material.SetMatrix("_FrustumCornersRay", frustumCorners);
            material.SetMatrix("_UnityMatVP", frustumCorners);
        }
    }
}