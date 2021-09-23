using Impact.TagLibrary;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Impact.EditorScripts
{
    [CustomEditor(typeof(ImpactTagLibrary))]
    public class ImpactTagLibraryEditor : Editor
    {
        private readonly Color warningColor = new Color(1, 1, 0.5f);

        private ImpactTagLibrary library;

        private void OnEnable()
        {
            library = target as ImpactTagLibrary;
        }

        public override void OnInspectorGUI()
        {
            drawTagsList();
        }

        private void drawTagsList()
        {
            EditorGUILayout.LabelField("Tags", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Space(5);

            for (int i = 0; i < ImpactTagLibraryConstants.TagCount; i++)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(i + ":", GUILayout.Width(40));
                library[i] = EditorGUILayout.TextField(library[i]);

                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Space(5);
            EditorGUILayout.EndVertical();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(library);
            }
        }
    }
}