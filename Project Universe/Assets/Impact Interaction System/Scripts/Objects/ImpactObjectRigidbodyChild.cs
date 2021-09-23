using Impact.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Impact.Objects
{
    /// <summary>
    /// Special Component for Impact Objects that are a child of a rigidbody.
    /// This component has somewhat lesser overhead than ImpactObjectRigidbody because it does not use FixedUpdate.
    /// </summary>
    [AddComponentMenu("Impact/Impact Object Rigidbody Child", 0)]
    public class ImpactObjectRigidbodyChild : ImpactObjectSingleMaterial
    {
        /// <summary>
        /// The parent ImpactObjectRigidbody associated with this child.
        /// </summary>
        public ImpactObjectRigidbody Parent
        {
            get { return parent; }
            set
            {
                parent = value;
                hasParent = parent != null;
            }
        }

        private ImpactObjectRigidbody parent;
        private bool hasParent = false;

        protected override void Awake()
        {
            base.Awake();

            RefreshParent();
        }

        /// <summary>
        /// Gets the ImpactObjectRigidbody component in this object's parent.
        /// </summary>
        public void RefreshParent()
        {
            Parent = GetComponentInParent<ImpactObjectRigidbody>();
        }

        public override VelocityData GetVelocityDataAtPoint(Vector3 contactPoint)
        {
            if (hasParent)
            {
                return Parent.GetVelocityDataAtPoint(contactPoint);
            }
            else
            {
                Debug.LogError($"ImpactObjectRigidbodyChild {gameObject.name} has no ImpactObjectRigidbody parent.");
                return new VelocityData();
            }
        }

        public override Vector3 GetContactPointRelativePosition(Vector3 point)
        {
            if (hasParent)
            {
                return Parent.GetContactPointRelativePosition(point);
            }
            else
            {
                Debug.LogError($"ImpactObjectRigidbodyChild {gameObject.name} has no ImpactObjectRigidbody parent.");
                return point;
            }
        }
    }
}

