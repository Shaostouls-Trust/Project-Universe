using Impact.Interactions;
using Impact.TagLibrary;
using UnityEngine;

namespace Impact.Materials
{
    /// <summary>
    /// Base ScriptableObject implementation of IImpactMaterial.
    /// </summary>
    public abstract class ImpactMaterialBase : ScriptableObject, IImpactMaterial
    {
        [SerializeField]
        private ImpactTagLibraryBase _tagLibrary;
        [SerializeField]
        private ImpactTagMask _materialTagsMask;

        /// <summary>
        /// The tag library used for this material (only used in the editor).
        /// </summary>
        public ImpactTagLibraryBase TagLibrary
        {
            get { return _tagLibrary; }
            set { _tagLibrary = value; }
        }

        /// <summary>
        /// The tags defined for this material.
        /// </summary>
        public ImpactTagMask MaterialTagsMask
        {
            get { return _materialTagsMask; }
            set { _materialTagsMask = value; }
        }

        /// <summary>
        /// Gets a single-element material composition array for this material, used for impact objects that only have a single material.
        /// </summary>
        /// <returns>A single-element material composition array for this material.</returns>
        public ImpactMaterialComposition[] GetSingleMaterialComposition()
        {
            ImpactMaterialComposition[] materialTypeComposition = new ImpactMaterialComposition[1];
            materialTypeComposition[0] = new ImpactMaterialComposition(this, 1);

            return materialTypeComposition;
        }

        /// <summary>
        /// Fills out the results array with the interaction results created from the interaction data.
        /// </summary>
        /// <param name="interactionData">The data to use for generating the interaction results.</param>
        /// <param name="results">Array that will be filled with the interaction results.</param>
        /// <returns>The number of results put into the results array.</returns>
        public abstract int GetInteractionResultsNonAlloc<T>(T interactionData, IInteractionResult[] results) where T : IInteractionData;
        /// <summary>
        /// Preloads any needed data and objects.
        /// </summary>
        public abstract void Preload();
    }
}