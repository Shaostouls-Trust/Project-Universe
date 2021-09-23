using Impact.Utility.ObjectPool;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Impact.Interactions.Decals
{
    /// <summary>
    /// An object pool used for placing decals from DecalInteractionResults.
    /// </summary>
    public class ImpactDecalPool : ObjectPool<ImpactDecalBase>
    {
        private static ObjectPoolGroup<ImpactDecalPool, ImpactDecalBase> poolGroup = new ObjectPoolGroup<ImpactDecalPool, ImpactDecalBase>();

        /// <summary>
        /// Create a pool for the given decal template.
        /// </summary>
        /// <param name="template">The decal prefab to create a pool for.</param>
        public static void PreloadPoolForDecal(ImpactDecalBase template)
        {
            poolGroup.GetOrCreatePool(template, template.PoolSize, template.PoolFallbackMode, out ImpactDecalPool pool);
        }

        /// <summary>
        /// Retrieve a decal from the pool.
        /// This will ALWAYS return a decal. If all decals are unavailable, the oldest active decal will be used.
        /// </summary>
        /// <param name="collisionResult">The result to create the decal for.</param>
        /// <param name="point">The point at which to place the decal.</param>
        /// <param name="normal">The surface normal for the decal rotation.</param>
        /// <returns>An ImpactDecal instance.</returns>
        public static ImpactDecalBase CreateDecal(DecalInteractionResult collisionResult, Vector3 point, Vector3 normal)
        {
            if (collisionResult.DecalTemplate == null)
                return null;

            ImpactDecalPool pool;

            if (poolGroup.GetOrCreatePool(collisionResult.DecalTemplate, collisionResult.DecalTemplate.PoolSize, collisionResult.DecalTemplate.PoolFallbackMode, out pool))
            {
                ImpactDecalBase a;
                if (pool.GetObject(0, out a))
                    a.SetupDecal(collisionResult, point, normal);

                return a;
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