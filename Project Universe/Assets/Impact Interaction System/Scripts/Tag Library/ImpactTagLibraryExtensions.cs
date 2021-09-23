using UnityEngine;

namespace Impact.TagLibrary
{
    public static class ImpactTagLibraryExtensions
    {
        public static int IndexOf(this IImpactTagLibrary tagLibrary, string tagName)
        {
            for (int i = 0; i < ImpactTagLibraryConstants.TagCount; i++)
            {
                if (tagLibrary[i].Equals(tagName))
                    return i;
            }

            return -1;
        }
    }
}
