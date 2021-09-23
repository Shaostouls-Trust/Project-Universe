using Impact.Objects;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Impact.EditorScripts
{
    public class ImpactObjectBaseEditor : Editor
    {
        private SerializedProperty priorityProp;

        private bool advancedFoldout;

        protected virtual void OnEnable()
        {
            priorityProp = serializedObject.FindProperty("_priority");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            inspectorGUICore();

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void inspectorGUICore()
        {
            EditorGUILayout.PropertyField(priorityProp, new GUIContent("Priority", "How important this object is as it relates to object pooling. Higher priorities will \"steal\" pooled resources from lower priorities."));
        }
    }

    [CustomEditor(typeof(ImpactObjectSingleMaterial))]
    [CanEditMultipleObjects]
    public class ImpactObjectSingleMaterialEditor : ImpactObjectBaseEditor
    {
        private SerializedProperty materialProp;

        protected override void OnEnable()
        {
            base.OnEnable();
            materialProp = serializedObject.FindProperty("_material");
        }

        protected override void inspectorGUICore()
        {
            EditorGUILayout.Separator();

            EditorGUILayout.ObjectField(materialProp, new GUIContent("Material", "The material associated with this object."));
            base.inspectorGUICore();
        }
    }

    [CustomEditor(typeof(ImpactObjectRigidbody))]
    [CanEditMultipleObjects]
    public class ImpactObjectRigidbodyEditor : ImpactObjectSingleMaterialEditor
    {
        private ImpactObjectRigidbody obj;
        private ImpactObjectRigidbodyChild[] children;

        private bool childrenFoldout;

        protected override void OnEnable()
        {
            base.OnEnable();

            obj = serializedObject.targetObject as ImpactObjectRigidbody;
            children = obj.GetComponentsInChildren<ImpactObjectRigidbodyChild>();
        }

        protected override void inspectorGUICore()
        {
            base.inspectorGUICore();

            if (children.Length > 0)
            {
                EditorGUILayout.Separator();

                EditorGUILayout.HelpBox("This object has Impact Object Rigidbody Child components.", MessageType.Info);
                childrenFoldout = EditorGUILayout.Foldout(childrenFoldout, new GUIContent("Children", "The children Impact Objects of this object."));

                if (childrenFoldout)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.Space(2);

                    for (int i = 0; i < children.Length; i++)
                    {
                        GUI.enabled = false;
                        EditorGUILayout.ObjectField(children[i], typeof(ImpactObjectRigidbodyChild), true);
                        GUI.enabled = true;
                    }

                    GUILayout.Space(2);
                    EditorGUILayout.EndVertical();
                }

            }
        }
    }

    [CustomEditor(typeof(ImpactObjectRigidbodyChild))]
    [CanEditMultipleObjects]
    public class ImpactObjectRigidbodyChildEditor : ImpactObjectSingleMaterialEditor
    {
        private ImpactObjectRigidbodyChild obj;
        private ImpactObjectRigidbody parent;

        protected override void OnEnable()
        {
            base.OnEnable();

            obj = serializedObject.targetObject as ImpactObjectRigidbodyChild;
            parent = obj.GetComponentInParent<ImpactObjectRigidbody>();
        }

        protected override void inspectorGUICore()
        {
            base.inspectorGUICore();

            EditorGUILayout.Separator();

            GUI.enabled = false;
            EditorGUILayout.ObjectField(new GUIContent("Parent"), parent, typeof(ImpactObjectRigidbody), true);
            GUI.enabled = true;

            if (parent == null)
            {
                EditorGUILayout.HelpBox("This child object has no parent Impact Object Rigidbody. Ensure that there is an Impact Object Rigidbody component added to the game object with a Rigidbody.", MessageType.Error);
            }
        }
    }
}