using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Impact.TagLibrary;
using System.Linq;
using Impact.Utility;
using System.Text;
using UnityEditor.IMGUI.Controls;

namespace Impact.EditorScripts
{
    public static class ImpactEditorUtilities
    {
        public static void Separator()
        {
            EditorGUILayout.Separator();
            GUILayout.Box("", GUILayout.MaxWidth(Screen.width - 25f), GUILayout.Height(2));
        }

        public static void TagMaskPropertyEditor(SerializedProperty tagMaskProperty, GUIContent label, ImpactTagNameList tagNames)
        {
            SerializedProperty tagMaskValueProperty = tagMaskProperty.FindPropertyRelative("_value");

            //Fallback for if no tags are given
            if (tagNames.ActiveTagCount == 0)
            {
                EditorGUILayout.PropertyField(tagMaskValueProperty, label);
                tagMaskProperty.serializedObject.ApplyModifiedProperties();
            }
            else
            {
                Rect totalRect = GUILayoutUtility.GetRect(new GUIContent(), EditorStyles.layerMaskField);
                Rect controlRect = EditorGUI.PrefixLabel(totalRect, label);

                TagMaskDropdown(tagMaskProperty, tagMaskValueProperty, tagNames, controlRect);
            }
        }

        public static void TagMaskDropdown(SerializedProperty tagMaskProperty, SerializedProperty tagMaskValueProperty, ImpactTagNameList tagNames, Rect controlRect)
        {
            string selectedTags = GetSelectedTags(tagMaskValueProperty.intValue, tagNames, false);
            string selectedTagsTooltip = GetSelectedTags(tagMaskValueProperty.intValue, tagNames, true);

            if (GUI.Button(controlRect, new GUIContent(selectedTags, selectedTagsTooltip), EditorStyles.layerMaskField))
            {
                ImpactTagSelectionDropdown tagMaskPopup = ScriptableObject.CreateInstance<ImpactTagSelectionDropdown>();

                tagMaskPopup.Initialize(tagMaskValueProperty, tagNames, true, (int pos, bool selected) =>
                {
                    if (selected)
                        tagMaskValueProperty.intValue = tagMaskValueProperty.intValue.SetBit(pos);
                    else
                        tagMaskValueProperty.intValue = tagMaskValueProperty.intValue.UnsetBit(pos);

                    tagMaskProperty.serializedObject.ApplyModifiedProperties();
                });

                Rect buttonRect = controlRect;
                Vector2 adjustedPosition = EditorGUIUtility.GUIToScreenPoint(buttonRect.position);
                buttonRect.position = adjustedPosition;

                tagMaskPopup.ShowAsDropDown(buttonRect, tagMaskPopup.GetWindowSize(controlRect));
            }
        }

        public static void TagDropdown(SerializedProperty tagProperty, SerializedProperty tagValueProperty, ImpactTagNameList tagNames, Rect controlRect)
        {
            string selectedTagName = "";
            int tagValue = tagValueProperty.intValue;

            if (tagValue >= 0 && tagValue < tagNames.Length)
                selectedTagName = tagNames[tagValue];

            if (GUI.Button(controlRect, selectedTagName, EditorStyles.layerMaskField))
            {
                ImpactTagSelectionDropdown tagMaskPopup = ScriptableObject.CreateInstance<ImpactTagSelectionDropdown>();

                tagMaskPopup.Initialize(tagValueProperty, tagNames, false, (int pos, bool selected) =>
                {
                    tagValueProperty.intValue = pos;
                    tagProperty.serializedObject.ApplyModifiedProperties();
                });

                Rect buttonRect = controlRect;
                Vector2 adjustedPosition = EditorGUIUtility.GUIToScreenPoint(buttonRect.position);
                buttonRect.position = adjustedPosition;

                tagMaskPopup.ShowAsDropDown(buttonRect, tagMaskPopup.GetWindowSize(controlRect));
            }
        }

        public static Range RangeEditor(Range range, GUIContent label)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(220));

            range.Min = EditorGUILayout.FloatField(range.Min);
            range.Max = EditorGUILayout.FloatField(range.Max);

            EditorGUILayout.EndHorizontal();

            return range;
        }

        public static void GetTagNames(IImpactTagLibrary tagLibrary, ref ImpactTagNameList tagNames)
        {
            tagNames.ActiveTagCount = 0;

            for (int i = 0; i < ImpactTagLibraryConstants.TagCount; i++)
            {
                if (tagLibrary != null && !string.IsNullOrEmpty(tagLibrary[i]))
                {
                    tagNames.ActiveTagCount++;
                    tagNames[i] = i + ": " + tagLibrary[i];
                }
                else
                    tagNames[i] = "";
            }
        }

        public static string GetSelectedTags(int tagMaskValue, ImpactTagNameList tagNames, bool newline)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < tagNames.Length; i++)
            {
                if (tagMaskValue.IsBitSet(i))
                {
                    sb.Append(tagNames[i]);

                    if (newline)
                        sb.Append("\n");
                    else
                        sb.Append(", ");
                }
            }

            if (sb.Length == 0)
                sb.Append("Nothing");
            else
                sb.Remove(sb.Length - (newline ? 1 : 2), newline ? 1 : 2);


            return sb.ToString();
        }

        public static bool DragAndDropArea(Rect dropArea, out string[] paths)
        {
            Event evt = Event.current;

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!dropArea.Contains(evt.mousePosition))
                    {
                        paths = null;
                        return false;
                    }

                    UnityEditor.DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        UnityEditor.DragAndDrop.AcceptDrag();

                        paths = UnityEditor.DragAndDrop.paths;
                        return true;
                    }

                    paths = null;
                    return true;
            }

            paths = null;
            return false;
        }
    }

    [CustomPropertyDrawer(typeof(ImpactTagMask))]
    public class ImpactTagMaskDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            SerializedProperty valueProperty = property.FindPropertyRelative("_value");
            SerializedProperty tagLibraryProperty = property.FindPropertyRelative("_tagLibrary");

            position.height = 16;
            tagLibraryProperty.objectReferenceValue = EditorGUI.ObjectField(position, tagLibraryProperty.objectReferenceValue, typeof(ImpactTagLibraryBase), false);

            position.y += 18;
            position.height = 16;

            ImpactTagLibraryBase tagLibrary = tagLibraryProperty.objectReferenceValue as ImpactTagLibraryBase;
            if (tagLibrary != null)
            {
                ImpactTagNameList tagNames = new ImpactTagNameList(ImpactTagLibraryConstants.TagCount);
                ImpactEditorUtilities.GetTagNames(tagLibrary, ref tagNames);

                if (tagNames.ActiveTagCount > 0)
                    ImpactEditorUtilities.TagMaskDropdown(property, valueProperty, tagNames, position);
                else
                    valueProperty.intValue = EditorGUI.IntField(position, valueProperty.intValue);
            }
            else
            {
                valueProperty.intValue = EditorGUI.IntField(position, valueProperty.intValue);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 36;
        }
    }

    [CustomPropertyDrawer(typeof(ImpactTag))]
    public class ImpactTagDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            SerializedProperty valueProperty = property.FindPropertyRelative("_value");
            SerializedProperty tagLibraryProperty = property.FindPropertyRelative("_tagLibrary");

            position.height = 16;
            tagLibraryProperty.objectReferenceValue = EditorGUI.ObjectField(position, tagLibraryProperty.objectReferenceValue, typeof(ImpactTagLibraryBase), false);

            position.y += 18;
            position.height = 16;

            ImpactTagLibraryBase tagLibrary = tagLibraryProperty.objectReferenceValue as ImpactTagLibraryBase;
            if (tagLibrary != null)
            {
                ImpactTagNameList tagNames = new ImpactTagNameList(ImpactTagLibraryConstants.TagCount);
                ImpactEditorUtilities.GetTagNames(tagLibrary, ref tagNames);

                ImpactEditorUtilities.TagDropdown(property, valueProperty, tagNames, position);
            }
            else
            {
                valueProperty.intValue = EditorGUI.IntField(position, valueProperty.intValue);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 36;
        }
    }

    public class ImpactTagSelectionDropdown : EditorWindow
    {
        private const int maxTagEntries = 10;

        private ImpactTagNameList tagNames;
        private SerializedProperty tagValueProperty;
        private bool isMask;

        private string search;
        private SearchField searchField;
        private Vector2 scrollPos;

        private System.Action<int, bool> tagSelectedChangedCallback;

        public void Initialize(SerializedProperty tagMaskValueProperty, ImpactTagNameList tagNames, bool isMask, System.Action<int, bool> tagSelectedChangedCallback)
        {
            this.isMask = isMask;
            this.tagValueProperty = tagMaskValueProperty;
            this.tagNames = tagNames;
            this.tagSelectedChangedCallback = tagSelectedChangedCallback;

            search = "";
            searchField = new SearchField();
        }

        public Vector2 GetWindowSize(Rect controlRect)
        {
            int h = Mathf.Min(tagNames.ActiveTagCount, maxTagEntries);

            float height = 18 * h + 38;
            return new Vector2(controlRect.width, height);
        }

        public void OnGUI()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            search = searchField.OnGUI(search);
            string searchLower = search.ToLower();

            EditorGUILayout.Separator();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            int tagValue = tagValueProperty.intValue;

            for (int i = 0; i < tagNames.Length; i++)
            {
                string tagName = tagNames[i];
                bool isTagNameDefined = tagNames.IsTagDefined(i);
                string tagNameLower = tagName.ToLower();

                bool boolValue = isMask ? tagValue.IsBitSet(i) : tagValue == i;

                bool show = isTagNameDefined || boolValue;

                if (search.Length > 0)
                    show &= searchLower.Contains(tagNameLower) || tagNameLower.Contains(searchLower);
                if (show)
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.BeginHorizontal();

                    bool toggleValue = GUILayout.Toggle(boolValue, new GUIContent(tagName));

                    if (!isTagNameDefined)
                    {
                        GUILayout.FlexibleSpace();
                        GUIContent warning = EditorGUIUtility.IconContent("console.warnicon.sml");
                        warning.tooltip = "Undefined tag.";
                        GUILayout.Label(warning, GUILayout.Width(20));
                    }

                    EditorGUILayout.EndHorizontal();

                    if (EditorGUI.EndChangeCheck())
                        tagSelectedChangedCallback.Invoke(i, toggleValue);
                }
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }
    }

    public struct ImpactTagNameList
    {
        public int ActiveTagCount;
        private string[] tagNames;

        public int Length
        {
            get { return tagNames.Length; }
        }

        public string this[int index]
        {
            get
            {
                if (string.IsNullOrEmpty(tagNames[index]))
                    return index + ": undefined";
                return tagNames[index];
            }
            set { tagNames[index] = value; }
        }

        public ImpactTagNameList(int tagNameCount)
        {
            tagNames = new string[tagNameCount];
            ActiveTagCount = 0;
        }

        public bool IsTagDefined(int index)
        {
            return !string.IsNullOrEmpty(tagNames[index]);
        }
    }
}