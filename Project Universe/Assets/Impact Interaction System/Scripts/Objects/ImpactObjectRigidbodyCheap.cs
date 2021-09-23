using UnityEngine;

namespace Impact.Objects
{
    /// <summary>
    /// Component for Impact Objects that have a rigidbody attached.
    /// This is the "cheap" version which does not use a FixedUpdate call.
    /// There may be slight inaccuracies on some objects, particularly regarding rolling.
    /// Best used for objects where precision isn't needed.
    /// </summary>
    [AddComponentMenu("Impact/Impact Object Rigidbody (Cheap)", 0)]
    public class ImpactObjectRigidbodyCheap : ImpactObjectSingleMaterial
    {
        /// <summary>
        /// The rigidbody (either 3D or 2D) associated with this object.
        /// </summary>
        public ImpactRigidbodyWrapper Rigidbody { get; set; }

        protected override void Awake()
        {
            base.Awake();

            Rigidbody = new ImpactRigidbodyWrapper(gameObject);
        }

        public override VelocityData GetVelocityDataAtPoint(Vector3 contactPoint)
        {
            return Rigidbody.GetCurrentVelocityData(contactPoint);
        }

        public override Vector3 GetContactPointRelativePosition(Vector3 point)
        {
            return Quaternion.Inverse(transform.rotation) * (point - Rigidbody.CurrentWorldCenterOfMass);
        }
    }
}

