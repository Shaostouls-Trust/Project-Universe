using Impact.TagLibrary;
using System;
using UnityEngine;

namespace Impact
{
    /// <summary>
    /// Represents a tag mask as a 32-bit integer bitmask.
    /// Adding this property to your scripts will show a custom editor in the inspector.
    /// </summary>
    [Serializable]
    public struct ImpactTagMask
    {
        [SerializeField]
        private int _value;
        [SerializeField]
        private ImpactTagLibraryBase _tagLibrary;

        /// <summary>
        /// The raw integer value of the bitmask.
        /// </summary>
        public int Value
        {
            get { return _value; }
            set { _value = value; }
        }

        /// <summary>
        /// Creates a new tag mask from the given bitmask.
        /// </summary>
        /// <param name="maskValue">The original bitmask.</param>
        /// <param name="tagLibrary">The tag library that was used to create this mask.</param>
        public ImpactTagMask(int maskValue, ImpactTagLibraryBase tagLibrary)
        {
            _value = maskValue;
            _tagLibrary = tagLibrary;
        }
    }
}