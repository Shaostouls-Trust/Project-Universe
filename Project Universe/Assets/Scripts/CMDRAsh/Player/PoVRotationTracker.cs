using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUniverse.Player.PlayerController
{
    /// <summary>
    /// Rotate the player's arms and hand/head equipment according to the camera angle
    /// </summary>
    public class PoVRotationTracker : MonoBehaviour
    {
        [SerializeField] private Transform camTransformToTrack;
        [SerializeField] private Transform TransformToSet;
        [SerializeField] private Vector3 rotationAdjustment;

        public Transform CamTransformToTrack
        {
            get { return camTransformToTrack; }
            set { camTransformToTrack = value; }
        }

        void Update()
        {
            TransformToSet.rotation = Quaternion.Euler(CamTransformToTrack.rotation.eulerAngles.x + rotationAdjustment.x,
                CamTransformToTrack.rotation.eulerAngles.y+rotationAdjustment.y, CamTransformToTrack.rotation.eulerAngles.z+rotationAdjustment.z);
        }
    }
}