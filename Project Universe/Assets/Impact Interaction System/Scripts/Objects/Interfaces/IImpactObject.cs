using Impact.Materials;
using UnityEngine;

namespace Impact.Objects
{
    /// <summary>
    /// Interface for implementing Impact Objects.
    /// </summary>
    public interface IImpactObject
    {
        /// <summary>
        /// How important this object is.
        /// </summary>
        int Priority { get; }
        /// <summary>
        /// The GameObject associated with this object.
        /// </summary>
        GameObject GameObject { get; }

        /// <summary>
        /// Gets the material composition at the given point by filling out the given array.
        /// </summary>
        /// <param name="point">The point to get materials at.</param>
        /// <param name="results">A pre-allocated array to put the results into.</param>
        /// <returns>The number of results.</returns>
        int GetMaterialCompositionNonAlloc(Vector3 point, ImpactMaterialComposition[] results);
        /// <summary>
        /// Gets the primary material at the given point.
        /// </summary>
        /// <param name="point">The point to get materias at.</param>
        /// <returns>The primary material, if there is one.</returns>
        IImpactMaterial GetPrimaryMaterial(Vector3 point);
        /// <summary>
        /// Gets the primary material without knowing the contact point.
        /// </summary>
        /// <returns>The primary material of the object.</returns>
        IImpactMaterial GetPrimaryMaterial();

        /// <summary>
        /// Gets this object's velocity data at the given point.
        /// </summary>
        /// <param name="contactPoint">The world-space point to get data for.</param>
        /// <returns>Velocity data for the point.</returns>
        VelocityData GetVelocityDataAtPoint(Vector3 contactPoint);
        /// <summary>
        /// Gets the local position of the contact point relative to this object's transform.
        /// </summary>
        /// <param name="point">The world-space point to convert to local space.</param>
        /// <returns>The position of the point in local space.</returns>
        Vector3 GetContactPointRelativePosition(Vector3 point);
    }
}