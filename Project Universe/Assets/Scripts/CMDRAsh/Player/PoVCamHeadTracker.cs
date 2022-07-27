using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUniverse.Player.PlayerController
{
    public class PoVCamHeadTracker : MonoBehaviour
    {
        [SerializeField] private Transform TransformToTrack;
        [SerializeField] private Transform CamTransformToSet;

        // Update is called once per frame
        void Update()
        {
            CamTransformToSet.position = TransformToTrack.position;
        }

        public void UpdateHeight(float newHeightRaw)
        {
            transform.localPosition = new Vector3(0f, newHeightRaw - 0.14f, 0f);
        }
    }
}