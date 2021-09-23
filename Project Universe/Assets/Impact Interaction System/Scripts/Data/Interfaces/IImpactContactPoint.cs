using UnityEngine;

namespace Impact
{
    /// <summary>
    /// An interface for consolidating 3D and 2D collision contact point data.
    /// </summary>
    public interface IImpactContactPoint
    {
        /// <summary>
        /// The contact point of the collision.
        /// </summary>
        Vector3 Point { get; set; }
        /// <summary>
        /// The surface normal at the contact point.
        /// </summary>
        Vector3 Normal { get; set; }

        /// <summary>
        /// The object that recieved the collision event.
        /// </summary>
        GameObject ThisObject { get; set; }
        /// <summary>
        /// The object we are colliding with.
        /// </summary>
        GameObject OtherObject { get; set; }

        /// <summary>
        /// Whether the source contact point data was 3D or 2D.
        /// </summary>
        PhysicsType PhysicsType { get; set; }

        /// <summary>
        /// The instance ID of the Physic Material or Physics 2D Material that was on this object.
        /// </summary>
        int ThisPhysicsMaterialID { get; set; }

        /// <summary>
        /// The instance ID of the Physic Material or Physics 2D Material that was on the other object.
        /// </summary>
        int OtherPhysicsMaterialID { get; set; }
    }
}

