using System;

namespace ModelShark
{
    [Serializable]
    public class DynamicSectionField
    {
        /// <summary>The user-friendly name of the dynamic section field (Ex: "EquippedItemSection").</summary>
        public string name;

        /// <summary>Whether the dynamic section is turned on or off.</summary>
        public bool isOn;
    }
}
