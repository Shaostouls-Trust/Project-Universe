namespace Impact.Interactions
{
    /// <summary>
    /// Handles processing interaction data and returning an interaction result.
    /// </summary>
    public interface IImpactInteraction
    {
        /// <summary>
        /// Process the given interaction data and return an interaction result.
        /// </summary>
        /// <param name="interactionData">The data to process.</param>
        /// <returns>An interaction result.</returns>
        IInteractionResult GetInteractionResult<T>(T interactionData) where T : IInteractionData;

        /// <summary>
        /// Preload any needed data or objects, such as object pools.
        /// </summary>
        void Preload();
    }
}

