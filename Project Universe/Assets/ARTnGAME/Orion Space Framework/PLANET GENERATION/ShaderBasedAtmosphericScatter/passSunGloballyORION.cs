using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Artngame.Orion
{

    [ExecuteInEditMode]
    public class passSunGloballyORION : MonoBehaviour
    {
        public Light sun;

        // Start is called before the first frame update
        void Start()
        {
            if (sun == null)
            {
                sun = GetComponent<Light>();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (sun != null)
            {
                Shader.SetGlobalVector("globalSunPosition", sun.transform.forward);
                Shader.SetGlobalVector("globalSunColor", new Vector4(sun.color.r, sun.color.g, sun.color.b, sun.intensity));
            }
        }
    }
}