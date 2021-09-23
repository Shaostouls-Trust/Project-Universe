using System;
using UnityEngine;

namespace Impact.TagLibrary
{
    [CreateAssetMenu(fileName = "Impact Tag Library", menuName = "Impact/Impact Tag Library", order = 0)]
    public class ImpactTagLibrary : ImpactTagLibraryBase
    {
        [SerializeField]
        private string[] _tagNames = new string[32];

        public override string this[int index]
        {
            get { return _tagNames[index]; }
            set { _tagNames[index] = value; }
        }
    }
}