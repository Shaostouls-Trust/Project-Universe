using Impact.Interactions;
using Impact.Materials;
using UnityEngine;

namespace Impact.Objects
{
    /// <summary>
    /// Base component for implementing Impact Objects.
    /// </summary>
    public abstract class ImpactObjectBase : MonoBehaviour, IImpactObject
    {
        [SerializeField]
        protected int _priority;

        /// <summary>
        /// How important this object is.
        /// </summary>
        public int Priority
        {
            get { return _priority; }
            set { _priority = value; }
        }

        /// <summary>
        /// The GameObject associated with this object.
        /// </summary>
        public GameObject GameObject
        {
            get { return gameObject; }
        }

        /// <summary>
        /// Gets this object's velocity data at the given point. By default this returns empty data.
        /// </summary>
        /// <param name="point">The world-space point to get data for.</param>
        /// <returns>Velocity data for the point.</returns>
        public virtual VelocityData GetVelocityDataAtPoint(Vector3 point)
        {
            return new VelocityData();
        }

        /// <summary>
        /// Gets the local position of the contact point relative to this object's transform.
        /// </summary>
        /// <param name="point">The world-space point to convert to local space.</param>
        /// <returns>The position of the point in local space.</returns>
        public virtual Vector3 GetContactPointRelativePosition(Vector3 point)
        {
            return transform.InverseTransformPoint(point);
        }

        /// <summary>
        /// Gets the material composition at the given point by filling out the given array.
        /// </summary>
        /// <param name="point">The point to get materials at.</param>
        /// <param name="results">A pre-allocated array to put the results into.</param>
        /// <returns>The number of results.</returns>
        public abstract int GetMaterialCompositionNonAlloc(Vector3 point, ImpactMaterialComposition[] results);
        /// <summary>
        /// Gets the primary material at the given point.
        /// </summary>
        /// <param name="point">The point to get materias at.</param>
        /// <returns>The primary material, if there is one.</returns>
        public abstract IImpactMaterial GetPrimaryMaterial(Vector3 point);
        /// <summary>
        /// Gets the primary material without knowing the contact point.
        /// </summary>
        /// <returns>The primary material of the object.</returns>
        public abstract IImpactMaterial GetPrimaryMaterial();
    }
}