using Impact.Interactions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Impact.Materials
{
    /// <summary>
    /// The standard implementation of an Impact Material, applicable for most uses.
    /// </summary>
    [CreateAssetMenu(fileName = "New Impact Material", menuName = "Impact/Impact Material", order = 1)]
    public class ImpactMaterial : ImpactMaterialBase
    {
        [SerializeField]
        private List<ImpactMaterialInteractionSet> _interactionSets = new List<ImpactMaterialInteractionSet>();
        [SerializeField]
        private ImpactTagMask _fallbackTagMask;

        /// <summary>
        /// The tag mask to use if the input interaction data does not have one.
        /// </summary>
        public ImpactTagMask FallbackTagMask
        {
            get { return _fallbackTagMask; }
            set { _fallbackTagMask = value; }
        }

        /// <summary>
        /// Fills the results array with interaction results based on the given interaction data.
        /// </summary>
        /// <param name="interactionData">The interaction data to create results for.</param>
        /// <param name="results">An array to fill with interaction results.</param>
        /// <returns>The number of results.</returns>
        public override int GetInteractionResultsNonAlloc<T>(T interactionData, IInteractionResult[] results)
        {
            int count = 0;

            ImpactTagMask tagMask = interactionData.TagMask ?? FallbackTagMask;

            for (int i = 0; i < InteractionSetCount || count >= results.Length; i++)
            {
                ImpactMaterialInteractionSet interactionSet = _interactionSets[i];
                populateCollisionResultsForInteractionSet(interactionData, interactionSet, tagMask, results, ref count);
            }

            return count;
        }

        private void populateCollisionResultsForInteractionSet<T>(T interactionData, ImpactMaterialInteractionSet interactionSet, ImpactTagMask tagMask, IInteractionResult[] results, ref int count) where T : IInteractionData
        {
            if (interactionSet.CompareTagMaskFilters(tagMask))
            {
                for (int j = 0; j < interactionSet.InteractionCount || count >= results.Length; j++)
                {
                    IInteractionResult c = interactionSet[j].GetInteractionResult(interactionData);
                    if (c == null)
                        continue;

                    if (c.IsValid)
                    {
                        results[count] = c;
                        count++;
                    }
                    else
                        c.Dispose();
                }

            }
        }

        /// <summary>
        /// Preloads data for all interaction sets.
        /// </summary>
        public override void Preload()
        {
            foreach (ImpactMaterialInteractionSet interactionSet in _interactionSets)
            {
                interactionSet.Preload();
            }
        }

        #region IList Implementation

        public int InteractionSetCount
        {
            get { return _interactionSets.Count; }
        }

        public ImpactMaterialInteractionSet this[int index]
        {
            get { return _interactionSets[index]; }
            set { _interactionSets[index] = value; }
        }

        public void AddInteractionSet(ImpactMaterialInteractionSet item)
        {
            _interactionSets.Add(item);
        }

        public void RemoveInteractionSet(int index)
        {
            _interactionSets.RemoveAt(index);
        }

        public void ClearInteractionSets()
        {
            _interactionSets.Clear();
        }

        #endregion
    }
}