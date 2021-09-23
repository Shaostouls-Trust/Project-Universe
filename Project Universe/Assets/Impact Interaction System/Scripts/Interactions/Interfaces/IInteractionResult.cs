using Impact.Objects;
using System;

namespace Impact.Interactions
{
    /// <summary>
    /// Holds all data and code needed to perform an interaction.
    /// </summary>
    public interface IInteractionResult : IDisposable
    {
        /// <summary>
        /// Does the result contain any useful data?
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// Perform the necessary tasks to display the results of the interaction, such as playing audio or emitting particle effects.
        /// </summary>
        /// <param name="parent">The Impact Object the data originated from.</param>
        void Process(IImpactObject parent);
    }

    /// <summary>
    /// Holds all data and code needed to perform an interaction that may persist over a length of time, such as sliding and rolling.
    /// </summary>
    public interface IContinuousInteractionResult : IInteractionResult
    {
        /// <summary>
        /// A unique key used to identify this result.
        /// </summary>
        long Key { get; set; }
        /// <summary>
        /// Is this result still alive and should continue being updated?
        /// </summary>
        bool IsAlive { get; }

        /// <summary>
        /// Update anything that needs to be updated.
        /// </summary>
        void FixedUpdate();
        /// <summary>
        /// Update the result and ensure it stays alive.
        /// </summary>
        /// <param name="newResult">The updated result.</param>
        void KeepAlive(IInteractionResult newResult);
    }

    public static class InteractionResultExtensions
    {
        /// <summary>
        /// Gets the priority, either from the Interaction Data Priority Override or from the Impact Object.
        /// </summary>
        /// <param name="collisionResult">The collision result to get the priority for.</param>
        /// <param name="obj">The impact object to get the priority from.</param>
        /// <returns>The priority, either from the Interaction Data Priority Override or from the Impact Object.</returns>
        public static int GetPriority(int? priorityOverride, IImpactObject obj)
        {
            if (priorityOverride.HasValue)
                return priorityOverride.Value;
            else if (obj != null)
                return obj.Priority;

            return 0;
        }
    }
}