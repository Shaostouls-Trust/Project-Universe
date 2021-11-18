using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace ModelShark
{
    public static class ExtensionMethods
    {
        /// <summary>Fills a list with string names of parameterized text fields found within an array of Text UI fields.</summary>
        /// <param name="textFields">An array of Text UI fields to scan for the existence of parameterized fields within its text.</param>
        /// <param name="parameterizedTextFields">The list of parameterized text fields to fill. Passing by ref and filling is done for performance, to avoid GC Allocation.</param>
        /// <param name="delimiter">The delimiter to use for finding parameterized fields. Ex: "Hello, %FirstName%!" would have "%" as the delimeter for the FirstName field.</param>
        public static void FillParameterizedTextFields(this List<string> textFields, ref List<ParameterizedTextField> parameterizedTextFields, string delimiter)
        {
            List<string> fieldNames = new List<string>();
            foreach (string textField in textFields)
            {
                string searchPattern = String.Format("{0}\\w*{0}", delimiter);
                MatchCollection matches = Regex.Matches(textField, searchPattern,
                    RegexOptions.Multiline | RegexOptions.IgnoreCase);

                // Add fields that aren't already in the list.
                foreach (Match match in matches)
                {
                    // Trim the field name and add it to the list of names.
                    string fieldName = match.Value.Trim('%');
                    if (!fieldNames.Contains(fieldName))
                        fieldNames.Add(fieldName);

                    // Check to see if this field name is already found in the list of fields passed in.
                    bool fieldFound = false;
                    foreach (ParameterizedTextField field in parameterizedTextFields)
                    {
                        if (fieldName == field.name)
                        {
                            field.placeholder = match.Value;
                            fieldFound = true;
                        }
                    }
                    // If not found, add it.
                    if (!fieldFound)
                        parameterizedTextFields.Add(new ParameterizedTextField() {name = fieldName, placeholder = match.Value, value = string.Empty});
                }
            }

            // Remove any parameterized fields that no longer exist in the associated text fields.
            parameterizedTextFields.RemoveAll(x => !fieldNames.Contains(x.name));
        }

        /// <summary>Fills a list with dynamic image fields found within an array of Image UI fields.</summary>
        /// <param name="imageFields">An array of dynamic image fields to replace.</param>
        /// <param name="dynamicImageFields">The list of dynamic image fields to fill. Passing by ref and filling is done for performance, to avoid GC Allocation.</param>
        /// <param name="delimiter">The delimiter to use for the image placeholder value (this just gets trimmed off, it's just there for visual consistency with the parameterized text fields).</param>
        public static void FillDynamicImageFields(this DynamicImage[] imageFields, ref List<DynamicImageField> dynamicImageFields, string delimiter)
        {
            List<string> fieldNames = new List<string>();

            // Add fields that aren't already in the list.
            foreach (DynamicImage imageField in imageFields)
            {
                // Trim the field name and add it to the list of names.
                string fieldName = imageField.placeholderName.Trim('%');
                if (!fieldNames.Contains(fieldName))
                    fieldNames.Add(fieldName);

                // Get the Image component on this object (all DynamicImage objects should have an associated Image UI component).
                Image placeholderImage = imageField.PlaceholderImage;

                // Check to see if this field name is already found in the list of fields passed in.
                bool fieldFound = false;
                foreach (DynamicImageField field in dynamicImageFields)
                {
                    if (fieldName == field.name)
                        fieldFound = true;
                }
                // If not found, add it.
                if (!fieldFound)
                    dynamicImageFields.Add(new DynamicImageField() { name = fieldName, placeholderSprite = placeholderImage.sprite, replacementSprite = null });
            }

            // Remove any parameterized fields that no longer exist in the associated text fields.
            dynamicImageFields.RemoveAll(x => !fieldNames.Contains(x.name));
        }

        public static void FillDynamicSectionFields(this DynamicSection[] sectionFields, ref List<DynamicSectionField> dynamicSectionFields, string delimiter)
        {
            List<string> fieldNames = new List<string>();

            // Add fields that aren't already in the list.
            foreach (DynamicSection sectionField in sectionFields)
            {
                // Trim the field name and add it to the list of names.
                string fieldName = sectionField.placeholderName.Trim('%');
                if (!fieldNames.Contains(fieldName))
                    fieldNames.Add(fieldName);

                // Get the GameObject reference for this object (this is what we will activate/deactivate dynamically).
                GameObject go = sectionField.gameObject;

                // Check to see if this field name is already found in the list of fields passed in.
                bool fieldFound = false;
                foreach (DynamicSectionField field in dynamicSectionFields)
                {
                    if (fieldName == field.name)
                        fieldFound = true;
                }
                // If not found, add it.
                if (!fieldFound)
                    dynamicSectionFields.Add(new DynamicSectionField() { name = fieldName,  isOn = go.activeSelf });
            }

            // Remove any parameterized fields that no longer exist in the associated text fields.
            dynamicSectionFields.RemoveAll(x => !fieldNames.Contains(x.name));
        }
    }
}
