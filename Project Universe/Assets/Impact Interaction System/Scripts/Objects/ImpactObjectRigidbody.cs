using Impact.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Impact.Objects
{
    /// <summary>
    /// Component for Impact Objects that have a rigidbody attached.
    /// </summary>
    [AddComponentMenu("Impact/Impact Object Rigidbody", 0)]
    public class ImpactObjectRigidbody : ImpactObjectSingleMaterial
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

        private void FixedUpdate()
        {
            //Cache our own rigidbody values because of timing weirdness when getting them directly from OnCollision messages.
            Rigidbody.FixedUpdate();
        }

        public override VelocityData GetVelocityDataAtPoint(Vector3 contactPoint)
        {
            return Rigidbody.GetVelocityData(contactPoint);
        }

        public override Vector3 GetContactPointRelativePosition(Vector3 point)
        {
            return Quaternion.Inverse(transform.rotation) * (point - Rigidbody.WorldCenterOfMass);
        }
    }
}

