using UnityEngine;

namespace Impact.TagLibrary
{
    public abstract class ImpactTagLibraryBase : ScriptableObject, IImpactTagLibrary
    {
        public abstract string this[int index] { get; set; }
    }
}