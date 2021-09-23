using Impact.Utility;
using UnityEngine;

namespace Impact
{
    /// <summary>
    /// Stores velocity data related to a point on a rigidbody.
    /// </summary>
    public struct VelocityData
    {
        /// <summary>
        /// The linear velocity of the entire Rigidbody.
        /// </summary>
        public Vector3 LinearVelocity;

        /// <summary>
        /// The velocity of the point derived from the angular velocity of the Rigidbody.
        /// </summary>
        public Vector3 TangentialVelocity;

        /// <summary>
        /// The sum of LinearVelocity and TangentialVelocity. This represents the actual velocity of the point.
        /// </summary>
        public Vector3 TotalPointVelocity
        {
            get { return LinearVelocity + TangentialVelocity; }
        }

        public VelocityData(Vector3 linearVelocity, Vector3 tangentialVelocity)
        {
            LinearVelocity = linearVelocity;
            TangentialVelocity = tangentialVelocity;
        }

        public override string ToString()
        {
            return "[Linear Velocity=" + LinearVelocity + "] [TangentialVelocity=" + TangentialVelocity + "] [Total=" + TotalPointVelocity + "]";
        }
    }
}
