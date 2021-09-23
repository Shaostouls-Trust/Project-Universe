using Impact.TagLibrary;
using System;
using UnityEngine;

namespace Impact
{
    /// <summary>
    /// Represents a single tag's value.
    /// Mainly useful if you want to see the tag name in the inspector rather than the integer value.
    /// </summary>
    [Serializable]
    public struct ImpactTag
    {
        [SerializeField]
        private int _value;
        [SerializeField]
        private ImpactTagLibraryBase _tagLibrary;

        /// <summary>
        /// The tag's integer value.
        /// </summary>
        public int Value
        {
            get { return _value; }
            set { _value = value; }
        }

        /// <summary>
        /// Creates a new tag from the given tag value.
        /// </summary>
        /// <param name="tagValue">The original tag value.</param>
        /// <param name="tagLibrary">The tag library that was used to create this tag.</param>
        public ImpactTag(int tagValue, ImpactTagLibraryBase tagLibrary)
        {
            _value = tagValue;
            _tagLibrary = tagLibrary;
        }

        /// <summary>
        /// Creates a new tag mask using only this tag.
        /// </summary>
        /// <returns>A tag mask with only this tag value set.</returns>
        public ImpactTagMask GetTagMask()
        {
            return new ImpactTagMask(1 << Value, _tagLibrary);
        }
    }
}