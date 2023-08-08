using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace ProjectUniverse.Util
{
    public class AutoDepthOfField : MonoBehaviour
    {
        [SerializeField] private Camera cam;
        //[SerializeField] private VolumeProfile globalVolumeProfile;
        private float oldDistance = 0f;
        //[SerializeField] private float focusTime = 0.5f;
        [SerializeField] private float maxFocusDistance = 30f;
        [SerializeField] private float precision = 0.005f;
        //private DepthOfField depthOfField;

       /* private void Start()
        {
            if (globalVolumeProfile.TryGet<DepthOfField>(out var dof))
            {
                dof.focusDistance.overrideState = true;
                depthOfField = dof;
            }
        }*/

        /// <summary>
        /// Dof will grab the distance to the nearest object in the center of the screen and set the focal length
        /// to that value, if it is not within 0.1f (or round to 0.1f?)
        /// Do not set the focal if the value is the same
        /// </summary>
        void FixedUpdate()
        {
            RaycastHit hit;
            if(Physics.Raycast(cam.transform.position, cam.transform.TransformDirection(0f, 0f, 1f), out hit, maxFocusDistance))//cam.transform.forward
            {
                if(Mathf.Abs(hit.distance - oldDistance) >= precision)
                {
                    //adjust .01 per camera degree up to 90
                    float adjust = cam.transform.localEulerAngles.x / 100f;
                    if(0f <= cam.transform.localEulerAngles.x && 180f > cam.transform.localEulerAngles.x)
                    {
                        //positive angles
                        adjust -= 2f;
                    }
                    else if(180 <= cam.transform.localEulerAngles.x && 360 > cam.transform.localEulerAngles.x)
                    {
                        //negative
                        adjust = (cam.transform.localEulerAngles.x-360f) / 100f;
                    }
                    //Debug.Log(adjust);
                    cam.focusDistance = (hit.distance+adjust);
                    oldDistance = hit.distance;
                }
            }
            else
            {
                cam.focusDistance = (hit.distance);
                //depthOfField.focusDistance.Override(maxFocusDistance);
                oldDistance = maxFocusDistance;
            }
            
        }
    }
}