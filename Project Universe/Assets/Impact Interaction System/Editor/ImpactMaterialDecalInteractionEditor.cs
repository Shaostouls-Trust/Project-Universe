using Impact.Interactions;
using Impact.Interactions.Decals;
using Impact.Utility;
using UnityEditor;
using UnityEngine;

namespace Impact.EditorScripts
{
    [CustomEditor(typeof(ImpactDecalInteraction))]
    public class ImpactMaterialDecalInteractionEditor : Editor
    {
        private readonly Color warningColor = new Color(1, 1, 0.5f);

        private ImpactDecalInteraction interaction;

        private void OnEnable()
        {
            interaction = target as ImpactDecalInteraction;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField("Decal Properties", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Decal Prefab", "The decal prefab to use."), GUILayout.Width(180));
            interaction.DecalPrefab = EditorGUILayout.ObjectField(interaction.DecalPrefab, typeof(ImpactDecalBase), false) as ImpactDecalBase;
            EditorGUILayout.EndHorizontal();

            if (interaction.DecalPrefab == null)
            {
                EditorGUILayout.HelpBox("You must assign an Decal Prefab for this interaction.", MessageType.Error);
            }

            ImpactEditorUtilities.Separator();

            EditorGUILayout.LabelField("Interaction Properties", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Minimum Velocity", "The minimum velocity magnitude required to place a decal."), GUILayout.Width(180));
            interaction.MinimumVelocity = EditorGUILayout.FloatField(interaction.MinimumVelocity);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Collision Normal Influence", "How much the collision normal should influence the calculated intensity."), GUILayout.Width(180));
            interaction.CollisionNormalInfluence = EditorGUILayout.Slider(interaction.CollisionNormalInfluence, 0, 1);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            interaction.CreateOnCollision = EditorGUILayout.ToggleLeft(new GUIContent("Create On Collision", "Should decals be placed on single collisions?"), interaction.CreateOnCollision);
            interaction.CreateOnSlide = EditorGUILayout.ToggleLeft(new GUIContent("Create On Slide", "Should decals be placed when sliding?"), interaction.CreateOnSlide);
            interaction.CreateOnRoll = EditorGUILayout.ToggleLeft(new GUIContent("Create On Roll", "Should decals be placed when rolling?"), interaction.CreateOnRoll);

            GUI.enabled = interaction.CreateOnSlide || interaction.CreateOnRoll;

            interaction.CreationInterval = ImpactEditorUtilities.RangeEditor(interaction.CreationInterval, new GUIContent("Creation Interval (Min/Max)", "The interval at which decals should be placed when sliding or rolling."));
            interaction.CreationIntervalType = (InteractionIntervalType)EditorGUILayout.EnumPopup(new GUIContent("Interval Type", "Whether the Creation Interval is defined in Time (seconds) or Distance."), interaction.CreationIntervalType);

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(interaction);
            }
        }
    }

    [CustomEditor(typeof(ImpactDecal))]
    [CanEditMultipleObjects]
    public class ImpactDecalEditor : Editor
    {
        private SerializedProperty poolSizeProp;
        private SerializedProperty poolFallbackModeProp;
        private SerializedProperty decalDistanceProp;
        private SerializedProperty parentToObjectProp;
        private SerializedProperty rotationModeProp;
        private SerializedProperty axisProp;

        private void OnEnable()
        {
            poolSizeProp = serializedObject.FindProperty("_poolSize");
            poolFallbackModeProp = serializedObject.FindProperty("_poolFallbackMode");

            decalDistanceProp = serializedObject.FindProperty("_decalDistance");
            parentToObjectProp = serializedObject.FindProperty("_parentToObject");
            rotationModeProp = serializedObject.FindProperty("_rotationMode");
            axisProp = serializedObject.FindProperty("_axis");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.Separator();

            serializedObject.Update();

            EditorGUILayout.PropertyField(decalDistanceProp, new GUIContent("Decal Distance", "How far the pivot of the decal should be placed from the surface."));
            EditorGUILayout.PropertyField(rotationModeProp, new GUIContent("Rotation Mode", "How should the decal be rotated?"));
            EditorGUILayout.PropertyField(axisProp, new GUIContent("Axis", "Which axis should be pointed towards the surface?"));

            EditorGUILayout.Separator();

            EditorGUILayout.PropertyField(parentToObjectProp, new GUIContent("Parent to Object", "Should the decal be parented to the object it is placed on?"));

            EditorGUILayout.Separator();

            EditorGUILayout.PropertyField(poolSizeProp, new GUIContent("Pool Size", "The size of the object pool that will be created for this decal."));
            EditorGUILayout.PropertyField(poolFallbackModeProp, new GUIContent("Pool Fallback Mode", "Defines behavior of the object pool when there is no available object to retrieve."));

            serializedObject.ApplyModifiedProperties();
        }
    }
}