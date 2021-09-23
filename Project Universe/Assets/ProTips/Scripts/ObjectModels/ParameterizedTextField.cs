using System;

namespace ModelShark
{
    [Serializable]
    public class ParameterizedTextField
    {
        /// <summary>The user-friendly name of the parameterized text field (Ex: "BodyText").</summary>
        public string name;
        /// <summary>The raw placeholder text, required for resetting the field when hidden (Ex: "%BodyText%").</summary>
        public string placeholder;
        /// <summary>The value to replace the placeholder text with (Ex: "This is some sample body text").</summary>
        public string value;
    }
}
