using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.Orion.ProceduralPlanets
{
    [ExecuteInEditMode]
    public class SunTest : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            Light light = FindFirstObjectByType<Light>();
            light.transform.forward = -transform.position.normalized;
            light.transform.position = transform.position;
        }
    }
}