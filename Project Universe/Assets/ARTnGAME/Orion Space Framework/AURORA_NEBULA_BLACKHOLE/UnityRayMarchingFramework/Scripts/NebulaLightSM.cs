using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Artngame.Orion.RaymarchedNebula
{
    [ExecuteInEditMode]
    public class NebulaLightSM : MonoBehaviour
    {

        public Vector4 lightDefinition;
        public Vector4 lightPosition;
        Light localLight;

        // Start is called before the first frame update
        void Start()
        {
            localLight = GetComponent<Light>();
        }

        // Update is called once per frame
        void Update()
        {
            lightDefinition.x = localLight.color.r;
            lightDefinition.y = localLight.color.g;
            lightDefinition.z = localLight.color.b;
            lightDefinition.w = localLight.intensity;

            lightPosition.x = localLight.transform.position.x;
            lightPosition.y = localLight.transform.position.y;
            lightPosition.z = localLight.transform.position.z;
            lightPosition.w = localLight.range;
        }
    }
}