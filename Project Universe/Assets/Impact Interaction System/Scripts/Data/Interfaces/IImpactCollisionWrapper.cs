using UnityEngine;

namespace Impact
{
    /// <summary>
    /// A wrapper interface for consolidating 3D and 2D collision data.
    /// </summary>
    public interface IImpactCollisionWrapper<T> where T : IImpactContactPoint
    {
        /// <summary>
        /// The number of contacts in the collision.
        /// </summary>
        int ContactCount { get; }

        /// <summary>
        /// Whether the source Collision data was 3D or 2D.
        /// </summary>
        PhysicsType PhysicsType { get; }

        /// <summary>
        /// Returns the contact point at the given index.
        /// </summary>
        /// <param name="index">The index of the contact point to get. Make sure this is less than ContactCount.</param>
        /// <returns>The contact point.</returns>
        T GetContact(int index);

    }
}

