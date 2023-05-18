using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.Orion.PyroPlanet
{
    public class PyroEffectControllerSM : MonoBehaviour
    {
        public Material pyroMat;
        public float speed = 1f;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (pyroMat)
            {
                Vector4 vex4 = pyroMat.GetVector("_Range");
                pyroMat.SetVector("_Range", new Vector4(vex4.x, (0.1f * Mathf.Cos(Time.fixedTime * 1.5f * speed)) + 0.45f, vex4.z, vex4.w));

                pyroMat.SetFloat("_Displacement", (0.45f * Mathf.Cos(Time.fixedTime * 1.5f * speed)) + 0.55f);
            }
        }
    }
}