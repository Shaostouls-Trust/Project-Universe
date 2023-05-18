using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Artngame.Orion
{
    public class followCameraSM : MonoBehaviour
    {
        public Camera cameraToFollow;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void LateUpdate()
        {
            if (cameraToFollow != null)
            {
                this.transform.position = cameraToFollow.transform.position;
            }
        }
    }
}