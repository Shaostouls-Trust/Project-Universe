using UnityEngine;

namespace Impact
{
    /// <summary>
    /// Implementation of the IImpactCollisionWrapper interface. Uses the ImpactContactPoint struct.
    /// </summary>
    public struct ImpactCollisionWrapper : IImpactCollisionWrapper<ImpactContactPoint>
    {
        /// <summary>
        /// The number of contacts in the collision.
        /// </summary>
        public int ContactCount { get; private set; }

        /// <summary>
        /// Whether the source Collision data was 3D or 2D.
        /// </summary>
        public PhysicsType PhysicsType { get; private set; }

        private Collision source3D;
        private Collision2D source2D;

        /// <summary>
        /// Initializes the wrapper for 3D collisions.
        /// </summary>
        /// <param name="collision3D">The source Collision object.</param>
        public ImpactCollisionWrapper(Collision collision3D)
        {
            source3D = collision3D;
            source2D = null;

            ContactCount = collision3D.contactCount;

            PhysicsType = PhysicsType.Physics3D;
        }

        /// <summary>
        /// Initializes the wrapper for 2D collisions.
        /// </summary>
        /// <param name="collision2D">The source Collision2D object.</param>
        public ImpactCollisionWrapper(Collision2D collision2D)
        {
            source2D = collision2D;
            source3D = null;

            ContactCount = collision2D.contactCount;

            PhysicsType = PhysicsType.Physics2D;
        }

        /// <summary>
        /// Returns the contact point at the given index.
        /// </summary>
        /// <param name="index">The index of the contact point to get. Make sure this is less than ContactCount.</param>
        /// <returns>The contact point.</returns>
        public ImpactContactPoint GetContact(int index)
        {
            if (PhysicsType == PhysicsType.Physics3D)
            {
                return new ImpactContactPoint(source3D.GetContact(index));
            }
            else
            {
                return new ImpactContactPoint(source2D.GetContact(index));
            }
        }
    }
}
