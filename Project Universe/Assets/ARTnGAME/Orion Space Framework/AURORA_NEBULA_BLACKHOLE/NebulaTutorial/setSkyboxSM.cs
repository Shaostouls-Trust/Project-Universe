using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Artngame.Orion
{
    [ExecuteInEditMode]
    public class setSkyboxSM : MonoBehaviour
    {
        public Material skyboxMaterial;
        public bool forceSkyboxChange = false;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void LateUpdate()
        {
            if (skyboxMaterial != null && (RenderSettings.skybox == null || forceSkyboxChange))
            {
                RenderSettings.skybox = skyboxMaterial;
            }
        }
    }
}