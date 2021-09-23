using System;
using UnityEngine;

namespace ModelShark
{
    [Serializable]
    public class DynamicImageField
    {
        /// <summary>The user-friendly name of the dynamic image field (Ex: "BodyImage").</summary>
        public string name;
        /// <summary>The placeholder image to replace, required for resetting the image after replacing.</summary>
        public Sprite placeholderSprite;
        /// <summary>The sprite to replace the placeholder with.</summary>
        public Sprite replacementSprite;
    }
}
