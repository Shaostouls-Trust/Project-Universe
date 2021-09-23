using UnityEngine;

namespace Impact.Interactions
{
    /// <summary>
    /// The type of interval for interactions like particles and decals when displaying for interactions like sliding and rolling.
    /// </summary>
    public enum InteractionIntervalType
    {
        /// <summary>
        /// Interval is in seconds.
        /// </summary>
        Time = 0,
        /// <summary>
        /// Interval is in units/meters.
        /// </summary>
        Distance = 1
    }

    /// <summary>
    /// Base ScriptableObject implementation of the IImpactMaterialInteraction interface.
    /// </summary>
    public abstract class ImpactInteractionBase : ScriptableObject, IImpactInteraction
    {
        /// <summary>
        /// Process the given interaction data and return an interaction result.
        /// </summary>
        /// <param name="interactionData">The data to process.</param>
        /// <returns>An interaction result.</returns>
        public abstract IInteractionResult GetInteractionResult<T>(T interactionData) where T : IInteractionData;

        /// <summary>
        /// Preload any needed data or objects, such as object pools.
        /// </summary>
        public abstract void Preload();
    }
}