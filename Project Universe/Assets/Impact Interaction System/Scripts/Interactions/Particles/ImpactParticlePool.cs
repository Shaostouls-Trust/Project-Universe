using Impact.Utility.ObjectPool;
using UnityEngine;

namespace Impact.Interactions.Particles
{
    /// <summary>
    /// Object pool for creating particles from ParticleInteractionResults.
    /// </summary>
    public class ImpactParticlePool : ObjectPool<ImpactParticlesBase>
    {
        private static ObjectPoolGroup<ImpactParticlePool, ImpactParticlesBase> poolGroup = new ObjectPoolGroup<ImpactParticlePool, ImpactParticlesBase>();

        /// <summary>
        /// Creates an object pool for the given particles template.
        /// </summary>
        /// <param name="template">The prefab to create an object pool for.</param>
        public static void PreloadPoolForParticle(ImpactParticlesBase template)
        {
            poolGroup.GetOrCreatePool(template, template.PoolSize, template.PoolFallbackMode, out ImpactParticlePool pool);
        }

        /// <summary>
        /// Emits particles for a ParticleInteractionResult.
        /// </summary>
        /// <param name="interactionResult">The interaction result to use data from.</param>
        /// <param name="point">The world position to emit the particles at.</param>
        /// <param name="normal">The surface normal used to set the particle's rotation.</param>
        /// <param name="priority">The priority of the particles. Particles with a priority less than this will be "stolen" and used for this result.</param>
        /// <returns>The ImpactParticles that will be used to emit the particles. Will be null if no particles are available.</returns>
        public static ImpactParticlesBase EmitParticles(ParticleInteractionResult interactionResult, Vector3 point, Vector3 normal, int priority)
        {
            if (interactionResult.ParticlesTemplate == null)
                return null;

            ImpactParticlePool pool;

            if (poolGroup.GetOrCreatePool(interactionResult.ParticlesTemplate, interactionResult.ParticlesTemplate.PoolSize, interactionResult.ParticlesTemplate.PoolFallbackMode, out pool))
            {
                ImpactParticlesBase a;

                if (pool.GetObject(priority, out a))
                {
                    a.EmitParticles(interactionResult, point, normal);
                    return a;
                }
            }

            return null;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            poolGroup.Remove(this);
        }
    }
}