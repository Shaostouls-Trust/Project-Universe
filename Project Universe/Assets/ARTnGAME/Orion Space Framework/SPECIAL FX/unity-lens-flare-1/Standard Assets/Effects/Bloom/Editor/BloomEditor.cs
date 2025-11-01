using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Artngame.Orion.ImageFX
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Bloom))]
    public class BloomEditor : Editor
    {
        [NonSerialized]
        private List<SerializedProperty> m_Properties = new List<SerializedProperty>();

        BloomGraphDrawer _graph;

        bool CheckHdr(Bloom target)
        {
            var camera = target.GetComponent<Camera>();
            return camera != null && camera.allowHDR;
        }

        void OnEnable()
        {
            //var settings = FieldFinder<Bloom>.GetField(x => x.settings);
            //foreach (var setting in settings.FieldType.GetFields())
            //{
            //    var prop = settings.Name + "." + setting.Name;
            //    m_Properties.Add(serializedObject.FindProperty(prop));
            //}

            //v0.1
            //threshold = 0.9f,
            //            softKnee = 0.5f,
            //            radius = 2.0f,
            //            intensity = 0.7f,
            //            highQuality = true,
            //            antiFlicker = false,
            //            dirtTexture = null,
            //            dirtIntensity = 2.5f
            m_Properties.Add(serializedObject.FindProperty("settings.threshold"));
            m_Properties.Add(serializedObject.FindProperty("settings.softKnee"));
            m_Properties.Add(serializedObject.FindProperty("settings.radius"));
            m_Properties.Add(serializedObject.FindProperty("settings.intensity"));
            m_Properties.Add(serializedObject.FindProperty("settings.highQuality"));
            m_Properties.Add(serializedObject.FindProperty("settings.antiFlicker"));
            m_Properties.Add(serializedObject.FindProperty("settings.dirtTexture"));
            m_Properties.Add(serializedObject.FindProperty("settings.dirtIntensity"));
            _graph = new BloomGraphDrawer();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (!serializedObject.isEditingMultipleObjects)
            {
                EditorGUILayout.Space();
                var bloom = (Bloom)target;
                _graph.Prepare(bloom.settings, CheckHdr(bloom));
                _graph.DrawGraph();
                EditorGUILayout.Space();
            }

            foreach (var property in m_Properties)
                EditorGUILayout.PropertyField(property);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
