using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace LIV.SDK.Unity
{
    [AddComponentMenu("LIV/SharedTextureProtocol")]
    public class SharedTextureProtocol : MonoBehaviour
    {
        #region Interop

        [DllImport("LIV_MR")]
        static extern IntPtr GetRenderEventFunc();

        [DllImport("LIV_MR", EntryPoint = "LivCaptureIsActive")]
        [return: MarshalAs(UnmanagedType.U1)]
        static extern bool GetIsCaptureActive();

        [DllImport("LIV_MR", EntryPoint = "LivCaptureWidth")]
        static extern int GetTextureWidth();

        [DllImport("LIV_MR", EntryPoint = "LivCaptureHeight")]
        static extern int GetTextureHeight();

        [DllImport("LIV_MR", EntryPoint = "LivCaptureSetTextureFromUnity")]
        static extern void SetTexture(IntPtr texture);

        #endregion

        public static bool IsActive
        {
            get { return GetIsCaptureActive(); }
        }

        public static int TextureWidth
        {
            get { return GetTextureWidth(); }
        }

        public static int TextureHeight
        {
            get { return GetTextureHeight(); }
        }

        public static void SetOutputTexture(Texture texture)
        {
            if (IsActive)
            {
                SetTexture(texture.GetNativeTexturePtr());
            }
        }

        void OnEnable()
        {
            StartCoroutine(CallPluginAtEndOfFrames());
        }
        IEnumerator CallPluginAtEndOfFrames()
        {
            while (enabled)
            {
                yield return new WaitForEndOfFrame();
                GL.IssuePluginEvent(GetRenderEventFunc(), 1);
            }
        }
        void OnDisable()
        {
            SetTexture(IntPtr.Zero);
        }
    }
}
