using Impact.Interactions;

namespace Impact.Materials
{
    /// <summary>
    /// Contains code for processing interaction data and returning interaction results.
    /// </summary>
    public interface IImpactMaterial
    {
        /// <summary>
        /// The tags for this material.
        /// </summary>
        ImpactTagMask MaterialTagsMask { get; }

        /// <summary>
        /// Fills out the results array with the interaction results created from the interaction data.
        /// </summary>
        /// <param name="data">The data to use for generating the interaction results.</param>
        /// <param name="results">Array that will be filled with the interaction results.</param>
        /// <returns>The number of results put into the results array.</returns>
        int GetInteractionResultsNonAlloc<T>(T data, IInteractionResult[] results) where T : IInteractionData;

        /// <summary>
        /// Preloads any needed data and objects.
        /// </summary>
        void Preload();
    }
}

