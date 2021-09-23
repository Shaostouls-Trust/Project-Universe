using Impact.Utility.ObjectPool;
using UnityEngine;

namespace Impact.Interactions.Decals
{
    /// <summary>
    /// Base Component for decals placed by decal interactions
    /// This can be inherited from if special decal functionality is needed.
    /// This type is used by the ImpactDecalPool.
    /// </summary>
    public abstract class ImpactDecalBase : PooledObject
    {
        /// <summary>
        /// The size of the object pool to be created for this decal.
        /// </summary>
        public abstract int PoolSize { get; set; }

        /// <summary>
        /// Defines behavior of the object pool when there is no available object to retrieve.
        /// </summary>
        public abstract ObjectPoolFallbackMode PoolFallbackMode { get; set; }

        /// <summary>
        /// Places the decal at the given point.
        /// </summary>
        /// <param name="interactionResult">The interaction result this decal is being created for.</param>
        /// <param name="point">The point at which to place the decal.</param>
        /// <param name="normal">The surface normal used to set the decal rotation.</param>
        public abstract void SetupDecal(DecalInteractionResult interactionResult, Vector3 point, Vector3 normal);
    }
}
