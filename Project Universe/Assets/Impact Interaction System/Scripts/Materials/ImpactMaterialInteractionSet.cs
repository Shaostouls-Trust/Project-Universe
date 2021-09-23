using Impact.Interactions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Impact.Materials
{
    /// <summary>
    /// Contains a list of interactions and filters for using the interactions.
    /// </summary>
    [System.Serializable]
    public class ImpactMaterialInteractionSet
    {
        /// <summary>
        /// Filter for comparing tag masks.
        /// </summary>
        [System.Serializable]
        public struct TagMaskFilter
        {
            /// <summary>
            /// The tag mask to filter by.
            /// </summary>
            public ImpactTagMask TagMask;

            /// <summary>
            /// Should tag masks being compared exactly match the filter?
            /// </summary>
            public bool ExactMatch;

            public bool CompareTagMask(ImpactTagMask tagMask)
            {
                if (ExactMatch)
                    return tagMask.Value == TagMask.Value;
                else
                    return (tagMask.Value & TagMask.Value) != 0;
            }
        }

        [SerializeField]
        private string _name;

        [SerializeField]
        private TagMaskFilter _includeTagsFilter;
        [SerializeField]
        private TagMaskFilter _excludeTagsFilter;

        [SerializeField]
        private List<ImpactInteractionBase> _interactions = new List<ImpactInteractionBase>();

        /// <summary>
        /// User-friendly name for this interaction set.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Tags that should be matched for this interaction set to be used.
        /// </summary>
        public TagMaskFilter IncludeTagsFilter
        {
            get { return _includeTagsFilter; }
            set { _includeTagsFilter = value; }
        }

        /// <summary>
        /// Tags that, if matched, will cause this interaction set to be ignored.
        /// </summary>
        public TagMaskFilter ExcludeTagsFilter
        {
            get { return _excludeTagsFilter; }
            set { _excludeTagsFilter = value; }
        }

        /// <summary>
        /// The number of interactions this set contains.
        /// </summary>
        public int InteractionCount
        {
            get { return _interactions.Count; }
        }

        public ImpactInteractionBase this[int index]
        {
            get { return _interactions[index]; }
            set { _interactions[index] = value; }
        }

        /// <summary>
        /// Compares the given tag mask to the include and exclude tag filters.
        /// </summary>
        /// <param name="tagMask">The tag mask to compare.</param>
        /// <returns>True if the tag mask passes the filters, false otherwise.</returns>
        public bool CompareTagMaskFilters(ImpactTagMask tagMask)
        {
            return _includeTagsFilter.CompareTagMask(tagMask) && !_excludeTagsFilter.CompareTagMask(tagMask);
        }

        /// <summary>
        /// Preload any needed data or objects.
        /// </summary>
        public void Preload()
        {
            foreach (ImpactInteractionBase interaction in _interactions)
            {
                interaction.Preload();
            }
        }

        /// <summary>
        /// Adds a new interaction.
        /// </summary>
        /// <param name="index"></param>
        public void AddInteraction(ImpactInteractionBase impactInteraction)
        {
            _interactions.Add(impactInteraction);
        }

        /// <summary>
        /// Removes the interaction at the given index.
        /// </summary>
        /// <param name="index"></param>
        public void RemoveInteraction(int index)
        {
            _interactions.RemoveAt(index);
        }
    }
}