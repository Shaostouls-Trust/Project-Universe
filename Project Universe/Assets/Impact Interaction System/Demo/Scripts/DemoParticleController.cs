using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Impact.Demo
{
    public class DemoParticleController : MonoBehaviour
    {
        [SerializeField]
        private ParticleSystem particles;

        public void ToggleParticles()
        {
            if (particles.isEmitting)
                particles.Stop();
            else
                particles.Play();
        }
    }
}

