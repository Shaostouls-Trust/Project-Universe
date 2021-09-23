using UnityEngine;

namespace Impact.Data
{
    /// <summary>
    /// Modes for setting how an object's axes should be aligned with a surface.
    /// </summary>
    public enum AlignmentAxis
    {
        ZDown = 0,
        ZUp = 1,
        YDown = 2,
        YUp = 3
    }

    /// <summary>
    /// Utility class for AlignmentAxis.
    /// </summary>
    public static class AlignmentAxisUtilities
    {
        /// <summary>
        /// Gets a rotation using the given alignment and surface normal.
        /// </summary>
        /// <param name="alignment">The axis alignment to use.</param>
        /// <param name="normal">The surface normal.</param>
        public static Quaternion GetRotationForAlignment(AlignmentAxis alignment, Vector3 normal)
        {
            if (alignment == AlignmentAxis.ZDown)
                return Quaternion.LookRotation(-normal);
            else if (alignment == AlignmentAxis.ZUp)
                return Quaternion.LookRotation(normal);
            else if (alignment == AlignmentAxis.YDown)
            {
                Quaternion q = Quaternion.LookRotation(-normal);
                return q * Quaternion.Euler(90, 0, 0);
            }
            else if (alignment == AlignmentAxis.YUp)
            {
                Quaternion q = Quaternion.LookRotation(normal);
                return q * Quaternion.Euler(90, 0, 0);
            }

            return Quaternion.LookRotation(normal);
        }

        /// <summary>
        /// Gets a rotation using the given alignment, surface normal, and velocity.
        /// </summary>
        /// <param name="alignment">The axis alignment to use.</param>
        /// <param name="normal">The surface normal.</param>
        /// <param name="velocity">The velocity of the interaction.</param>
        public static Quaternion GetVelocityRotation(AlignmentAxis alignment, Vector3 normal, Vector3 velocity)
        {
            if (alignment == AlignmentAxis.ZDown)
                return Quaternion.LookRotation(-normal, velocity);
            else if (alignment == AlignmentAxis.ZUp)
                return Quaternion.LookRotation(normal, velocity);
            else if (alignment == AlignmentAxis.YDown)
            {
                Quaternion q = Quaternion.LookRotation(-normal, velocity);
                return q * Quaternion.Euler(90, 0, 0);
            }
            else if (alignment == AlignmentAxis.YUp)
            {
                Quaternion q = Quaternion.LookRotation(normal, -velocity);
                return q * Quaternion.Euler(90, 0, 0);
            }

            return Quaternion.LookRotation(normal, velocity);
        }
    }
}