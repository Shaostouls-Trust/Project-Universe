using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.Orion.ProceduralPlanets
{
    public class SunShadowCaster : MonoBehaviour
    {
        Transform track;

        void Start()
        {
            track = Camera.main?.transform;
        }

        void LateUpdate()
        {
            if (track)
            {
                transform.LookAt(track.position);
            }
        }
    }
}