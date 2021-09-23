using UnityEngine;

namespace Impact.Interactions
{
    public static class ImpactInteractionUtilities
    {
        /// <summary>
        /// Gets the adjusted intensity of a collision, taking into account the type of collision and the Collision Normal Influence.
        /// </summary>
        /// <param name="interactionData">The data that contains the velocity and normal of the collision.</param>
        /// <param name="collisionNormalInfluence">A 0 to 1 value representing how much the normal should affect the intensity.</param>
        /// <returns>The adjusted intensity of the collision.</returns>
        public static float GetCollisionIntensity<T>(T interactionData, float collisionNormalInfluence) where T : IInteractionData
        {
            float dotProduct = 0;
            float velocityMagnitude = interactionData.Velocity.magnitude;

            if (interactionData.Normal.sqrMagnitude == 0)
                dotProduct = 1;
            else
            {
                Vector3 normalizedVelocity = velocityMagnitude == 0 ? Vector3.zero : interactionData.Velocity / velocityMagnitude;

                if (interactionData.InteractionType == InteractionData.InteractionTypeCollision)
                    dotProduct = Mathf.Abs(Vector3.Dot(normalizedVelocity, interactionData.Normal));
                else
                    dotProduct = 1 - Mathf.Abs(Vector3.Dot(normalizedVelocity, interactionData.Normal));
            }

            float intensity = (dotProduct + (1 - dotProduct) * (1 - collisionNormalInfluence)) * velocityMagnitude;

            return intensity;
        }

        public static bool GetKeyAndValidate<T>(T interactionData, ImpactInteractionBase interaction, out long key) where T : IInteractionData
        {
            key = 0;

            if (interactionData.InteractionType != InteractionData.InteractionTypeCollision)
            {
                int tagMaskValue = 0;
                if (interactionData.TagMask.HasValue)
                    tagMaskValue = interactionData.TagMask.Value.Value;

                key = cantorPairing(interaction.GetInstanceID(),
                    cantorPairing(tagMaskValue,
                    cantorPairing(interactionData.InteractionType,
                    cantorPairing(interactionData.ThisObject.GetInstanceID(), interactionData.OtherObject.GetInstanceID()))));

                bool containsKey = ImpactManagerInstance.HasActiveContinuousInteractionWithKey(key);

                if (!containsKey && ImpactManagerInstance.HasReachedContinuousInteractionLimit())
                {
                    return false;
                }
            }

            return true;
        }

        private static long cantorPairing(long k1, long k2)
        {
            return (k1 + k2) * (k1 + k2 + 1) / 2 + k2;
        }
    }
}