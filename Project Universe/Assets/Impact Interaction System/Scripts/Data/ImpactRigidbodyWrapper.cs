using Impact.Utility;
using UnityEngine;

namespace Impact
{
    /// <summary>
    /// A wrapper class that consolidates Rigidbody and Rigidbody2D data.
    /// </summary>
    public class ImpactRigidbodyWrapper
    {
        /// <summary>
        /// Whether this rigidbody wrapper contains a 3D or 2D rigidbody.
        /// </summary>
        public PhysicsType PhysicsType { get; private set; }

        /// <summary>
        /// The 3D rigidbody the wrapper contains, if applicable.
        /// </summary>
        public Rigidbody Rigidbody3D { get; private set; }

        /// <summary>
        /// The 2D rigidbody the wrapper contains, if applicable.
        /// </summary>
        public Rigidbody2D Rigidbody2D { get; private set; }

        /// <summary>
        /// The linear velocity of the rigidbody.
        /// This must be manually set, usually in FixedUpdate.
        /// </summary>
        public Vector3 Velocity { get; private set; }

        /// <summary>
        /// The angular velocity of the rigidbody.
        /// This must be manually set, usually in FixedUpdate.
        /// </summary>
        public Vector3 AngularVelocity { get; private set; }

        /// <summary>
        /// The center of mass of the rigidbody in world space.
        /// This must be manually set, usually in FixedUpdate.
        /// </summary>
        public Vector3 WorldCenterOfMass { get; private set; }

        /// <summary>
        /// The actual current velocity of the Rigidbody or Rigidbody2D.
        /// This is retrieved directly from the rigidbody.
        /// </summary>
        public Vector3 CurrentVelocity
        {
            get
            {
                if (PhysicsType == PhysicsType.Physics2D)
                    return Rigidbody2D.velocity;
                else if (PhysicsType == PhysicsType.Physics3D)
                    return Rigidbody3D.velocity;

                Debug.LogError("Enable to get CurrentVelocity in ImpactRigidbodyWrapper. Please ensure that you have Rigidbody or Rigidbody2D components on your Impact Object Rigidbody objects.");
                return Vector3.zero;
            }
        }

        /// <summary>
        /// The actual current angular velocity of the Rigidbody or Rigidbody2D.
        /// This is retrieved directly from the rigidbody.
        /// </summary>
        public Vector3 CurrentAngularVelocity
        {
            get
            {
                if (PhysicsType == PhysicsType.Physics2D)
                    return new Vector3(0, 0, Rigidbody2D.angularVelocity);
                else if (PhysicsType == PhysicsType.Physics3D)
                    return Rigidbody3D.angularVelocity;

                Debug.LogError("Enable to get CurrentAngularVelocity in ImpactRigidbodyWrapper. Please ensure that you have Rigidbody or Rigidbody2D components on your Impact Object Rigidbody objects.");
                return Vector3.zero;
            }
        }

        /// <summary>
        /// The actual current center of mass of the Rigidbody or Rigidbody2D.
        /// This is retrieved directly from the rigidbody.
        /// </summary>
        public Vector3 CurrentWorldCenterOfMass
        {
            get
            {
                if (PhysicsType == PhysicsType.Physics2D)
                    return Rigidbody2D.worldCenterOfMass;
                else if (PhysicsType == PhysicsType.Physics3D)
                    return Rigidbody3D.worldCenterOfMass;

                Debug.LogError("Enable to get CurrentWorldCenterOfMass in ImpactRigidbodyWrapper. Please ensure that you have Rigidbody or Rigidbody2D components on your Impact Object Rigidbody objects.");
                return Vector3.zero;
            }
        }

        /// <summary>
        /// Initializes the wrapper for the given game object.
        /// </summary>
        /// <param name="gameObject">The GameObject to use.</param>
        public ImpactRigidbodyWrapper(GameObject gameObject)
        {
            Rigidbody r3D = gameObject.GetComponentInParent<Rigidbody>();
            if (r3D != null)
            {
                Rigidbody3D = r3D;
                PhysicsType = PhysicsType.Physics3D;
                return;
            }

            Rigidbody2D r2D = gameObject.GetComponentInParent<Rigidbody2D>();
            if (r2D != null)
            {
                Rigidbody2D = r2D;
                PhysicsType = PhysicsType.Physics2D;
                return;
            }

            Debug.LogError("Unable to find Rigidbody or Rigidbody2D component on game object: " + gameObject.name +
                ". Please ensure that you have Rigidbody or Rigidbody2D components on your Impact Object Rigidbody objects.");
        }

        /// <summary>
        /// Initializes the wrapper for a 3D rigidbody.
        /// </summary>
        /// <param name="rigidbody3D">The rigidbody to use.</param>
        public ImpactRigidbodyWrapper(Rigidbody rigidbody3D)
        {
            this.Rigidbody3D = rigidbody3D;
            PhysicsType = PhysicsType.Physics3D;
        }

        /// <summary>
        /// Initializes the wrapper for a 2D rigidbody.
        /// </summary>
        /// <param name="rigidbody2D">The rigidbody to use.</param>
        public ImpactRigidbodyWrapper(Rigidbody2D rigidbody2D)
        {
            this.Rigidbody2D = rigidbody2D;
            PhysicsType = PhysicsType.Physics2D;
        }

        /// <summary>
        /// Update internally cached Center of Mass, Velocity, and Angular Velocity
        /// We need to cache these in FixedUpdate because of timing weirdness when trying to access them from OnCollision messages.
        /// </summary>
        public void FixedUpdate()
        {
            if (PhysicsType == PhysicsType.Physics3D)
            {
                WorldCenterOfMass = Rigidbody3D.worldCenterOfMass;
                Velocity = Rigidbody3D.velocity;
                AngularVelocity = Rigidbody3D.angularVelocity;
            }
            else if (PhysicsType == PhysicsType.Physics2D)
            {
                WorldCenterOfMass = Rigidbody2D.worldCenterOfMass;
                Velocity = Rigidbody2D.velocity;
                AngularVelocity = new Vector3(0, 0, Rigidbody2D.angularVelocity);
            }
        }

        /// <summary>
        /// Gets the velocity of the given point using the cached rigidbody values.
        /// </summary>
        public VelocityData GetVelocityData(Vector3 point)
        {
            return new VelocityData(Velocity, PhysicsUtilities.CalculateTangentialVelocity(point, AngularVelocity, WorldCenterOfMass));
        }

        /// <summary>
        /// Gets the velocity of the given point using values retrieved directly from the rigidbody.
        /// </summary>
        public VelocityData GetCurrentVelocityData(Vector3 point)
        {
            return new VelocityData(CurrentVelocity, PhysicsUtilities.CalculateTangentialVelocity(point, CurrentAngularVelocity, CurrentWorldCenterOfMass));
        }
    }
}

