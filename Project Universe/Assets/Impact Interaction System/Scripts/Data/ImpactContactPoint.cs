using UnityEngine;

namespace Impact
{
    /// <summary>
    /// Implementation of the IImpactContactPoint interface.
    /// </summary>
    public struct ImpactContactPoint : IImpactContactPoint
    {
        /// <summary>
        /// The contact point of the collision.
        /// </summary>
        public Vector3 Point { get; set; }
        /// <summary>
        /// The surface normal at the contact point.
        /// </summary>
        public Vector3 Normal { get; set; }

        /// <summary>
        /// The object that recieved the collision event.
        /// </summary>
        public GameObject ThisObject { get; set; }
        /// <summary>
        /// The object we are colliding with.
        /// </summary>
        public GameObject OtherObject { get; set; }

        /// <summary>
        /// Whether the source contact point was 3D or 2D.
        /// </summary>
        public PhysicsType PhysicsType { get; set; }

        /// <summary>
        /// The instance ID of the Physic Material or Physics 2D Material that was on this object.
        /// </summary>
        public int ThisPhysicsMaterialID { get; set; }

        /// <summary>
        /// The instance ID of the Physic Material or Physics 2D Material that was on the collider.
        /// </summary>
        public int OtherPhysicsMaterialID { get; set; }

        public ImpactContactPoint(IImpactContactPoint original)
        {
            Point = original.Point;
            Normal = original.Normal;

            ThisObject = original.ThisObject;
            OtherObject = original.OtherObject;

            PhysicsType = PhysicsType.Physics3D;

            ThisPhysicsMaterialID = original.ThisPhysicsMaterialID;
            OtherPhysicsMaterialID = original.OtherPhysicsMaterialID;
        }

        /// <summary>
        /// Initializes the contact point with data from a ContactPoint. 
        /// </summary>
        /// <param name="contact3D">The source ContactPoint data.</param>
        public ImpactContactPoint(ContactPoint contact3D)
        {
            Point = contact3D.point;
            Normal = contact3D.normal;

            ThisObject = contact3D.thisCollider.gameObject;
            OtherObject = contact3D.otherCollider.gameObject;

            PhysicsType = PhysicsType.Physics3D;

            ThisPhysicsMaterialID = 0;
            OtherPhysicsMaterialID = 0;

            if (contact3D.thisCollider.sharedMaterial != null)
                ThisPhysicsMaterialID = contact3D.thisCollider.sharedMaterial.GetInstanceID();
            if (contact3D.otherCollider.sharedMaterial != null)
                OtherPhysicsMaterialID = contact3D.otherCollider.sharedMaterial.GetInstanceID();
        }

        /// <summary>
        /// Initializes the contact point with data from a ContactPoint2D. 
        /// </summary>
        /// <param name="contact2D">The source ContactPoint2D data.</param>
        public ImpactContactPoint(ContactPoint2D contact2D)
        {
            Point = contact2D.point;
            Normal = contact2D.normal;

            ThisObject = contact2D.otherCollider.gameObject;
            OtherObject = contact2D.collider.gameObject;

            PhysicsType = PhysicsType.Physics2D;

            ThisPhysicsMaterialID = 0;
            OtherPhysicsMaterialID = 0;

            if (contact2D.otherCollider.sharedMaterial != null)
                ThisPhysicsMaterialID = contact2D.otherCollider.sharedMaterial.GetInstanceID();
            if (contact2D.collider.sharedMaterial != null)
                OtherPhysicsMaterialID = contact2D.collider.sharedMaterial.GetInstanceID();
        }
    }
}
