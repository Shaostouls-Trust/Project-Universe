using System.Linq;
using System.Text;
using UnityEngine;
using Valve.VR;

namespace LIV.SDK.Unity
{
    [AddComponentMenu("LIV/ExternalCamera")]
    public class ExternalCamera : MonoBehaviour
    {
        public bool IsValid
        {
            get { return OpenVRTrackedDeviceId != OpenVR.k_unTrackedDeviceIndexInvalid; }
        }

        public uint OpenVRTrackedDeviceId { get; protected set; }

        TrackedDevicePose_t _trackedDevicePose;

        TrackedDevicePose_t[] _devices = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
        TrackedDevicePose_t[] _gameDevices = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];

        void OnEnable()
        {
            InvokeRepeating("UpdateOpenVRDevice", 0.5f, 0.5f);
            UpdateOpenVRDevice();
        }
        void OnDisable()
        {
            CancelInvoke("UpdateOpenVRDevice");
        }

        void LateUpdate()
        {
            UpdateCamera();
        }
        void OnPreCull()
        {
            UpdateCamera();
        }

        void UpdateCamera()
        {
            if (OpenVRTrackedDeviceId == OpenVR.k_unTrackedDeviceIndexInvalid) return;

            UpdateOpenVRPose();

            if (!_trackedDevicePose.bDeviceIsConnected)
            {
                UpdateOpenVRDevice();
                if (OpenVRTrackedDeviceId == OpenVR.k_unTrackedDeviceIndexInvalid) return;

                UpdateOpenVRPose();
            }

            UpdateTransform(_trackedDevicePose.mDeviceToAbsoluteTracking);
        }

        void UpdateOpenVRPose()
        {
            var error = OpenVR.Compositor.GetLastPoses(_devices, _gameDevices);
            if (error != EVRCompositorError.None)
                return;

            _trackedDevicePose = _devices[OpenVRTrackedDeviceId];
        }

        void UpdateTransform(HmdMatrix34_t deviceToAbsolute)
        {
            var m = Matrix4x4.identity;

            m[0, 0] = deviceToAbsolute.m0;
            m[0, 1] = deviceToAbsolute.m1;
            m[0, 2] = -deviceToAbsolute.m2;
            m[0, 3] = deviceToAbsolute.m3;

            m[1, 0] = deviceToAbsolute.m4;
            m[1, 1] = deviceToAbsolute.m5;
            m[1, 2] = -deviceToAbsolute.m6;
            m[1, 3] = deviceToAbsolute.m7;

            m[2, 0] = -deviceToAbsolute.m8;
            m[2, 1] = -deviceToAbsolute.m9;
            m[2, 2] = deviceToAbsolute.m10;
            m[2, 3] = -deviceToAbsolute.m11;

            transform.localPosition = m.GetPosition();
            transform.localRotation = m.GetRotation();
        }

        /// <summary>
        /// Warning: Method name referred to by string from code in this class.
        /// </summary>
        void UpdateOpenVRDevice()
        {
            OpenVRTrackedDeviceId = IdentifyExternalCameraDevice();
        }

        uint IdentifyExternalCameraDevice()
        {
            var error = OpenVR.Compositor.GetLastPoses(_devices, _gameDevices);
            if (error != EVRCompositorError.None)
            {
                Debug.Log("Encountered an error whilst looking for the external camera: " + error);
                return OpenVR.k_unTrackedDeviceIndexInvalid;
            }

            var candidates = _devices
                .Select((pose, index) => new { pose, index = (uint)index })
                .Where(candidate => candidate.pose.bDeviceIsConnected)
                .Select(candidate => new
                {
                    candidate.pose,
                    candidate.index,
                    deviceClass = OpenVR.System.GetTrackedDeviceClass(candidate.index)
                })
                .Where(candidate => candidate.deviceClass == ETrackedDeviceClass.Controller || candidate.deviceClass == ETrackedDeviceClass.GenericTracker)
                .Select(candidate => new
                {
                    candidate.pose,
                    candidate.index,
                    candidate.deviceClass,
                    deviceRole = OpenVR.System.GetControllerRoleForTrackedDeviceIndex(candidate.index),
                    modelNumber = GetStringTrackedDeviceProperty(candidate.index, ETrackedDeviceProperty.Prop_ModelNumber_String),
                    renderModel = GetStringTrackedDeviceProperty(candidate.index, ETrackedDeviceProperty.Prop_RenderModelName_String)
                })
                .OrderByDescending(candidate =>
                {
                    if (candidate.modelNumber == "LIV Virtual Camera") return 10; // Always use the Virtual Camera if possible.
                    if (candidate.modelNumber == "Virtual Controller Device") return 9; // Always use the Virtual Controller if possible.

                    if (candidate.deviceClass == ETrackedDeviceClass.GenericTracker) return 5; // Weight all GenericTrackers the same.

                    if (candidate.deviceClass == ETrackedDeviceClass.Controller)
                    {
                        if (candidate.renderModel == "{htc}vr_tracker_vive_1_0") return 8; // Dead giveaway that the user is trying to do MR.
                        if (candidate.deviceRole == ETrackedControllerRole.OptOut) return 7; // OptOut is a new role, indicative of a special case.
                        if (candidate.deviceRole == ETrackedControllerRole.Invalid) return 6; // User probably has 3 controllers, and this is the camera.

                        return 1; // Better than nothing!
                    }

                    return 0;
                });

            var externalCamera = candidates.FirstOrDefault();

            return externalCamera != null ? externalCamera.index : OpenVR.k_unTrackedDeviceIndexInvalid;
        }

        static string GetStringTrackedDeviceProperty(uint device, ETrackedDeviceProperty property)
        {
            const int bufferLength = 1024;
            var buffer = new StringBuilder(bufferLength);

            var error = ETrackedPropertyError.TrackedProp_Success;
            OpenVR.System.GetStringTrackedDeviceProperty(device, property, buffer, bufferLength, ref error);

            switch (error)
            {
                case ETrackedPropertyError.TrackedProp_Success:
                    return buffer.ToString();
                case ETrackedPropertyError.TrackedProp_UnknownProperty:
                    return "";

                default:
                    Debug.Log("Encountered an error whilst reading a tracked device property: " + error);
                    return null;
            }
        }
    }
}
