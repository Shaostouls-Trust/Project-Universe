using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.Orion.ProceduralPlanets
{
    public abstract class PostProcessingEffect
    {

        protected Material material;

        public abstract Material GetMaterial();

        public virtual void ReleaseBuffers()
        {

        }
    }
}