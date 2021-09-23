using UnityEngine;
using UnityEngine.UI;

namespace ModelShark
{
    public class DynamicImage : MonoBehaviour
    {
        public string placeholderName;

        [HideInInspector]
        public Image image;

        /// <summary>The user-friendly name of the placeholder field, without the delimiters.</summary>
        public string Name { get; set; }

        /// <summary>Used during Editor time to retrieve the image. Not used during runtime for performance reasons.</summary>
        public Image PlaceholderImage
        {
            get { return GetComponent<Image>(); }
        }
    }
}