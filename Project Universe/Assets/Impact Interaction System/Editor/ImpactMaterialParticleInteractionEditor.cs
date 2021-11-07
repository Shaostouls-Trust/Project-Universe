using Impact.Interactions;
using Impact.Interactions.Particles;
using Impact.Utility;
using UnityEditor;
using UnityEngine;

namespace Impact.EditorScripts
{
    [CustomEditor(typeof(ImpactParticleInteraction))]
    public class ImpactMaterialParticleInteractionEditor : Editor
    {
        private readonly Color warningColor = new Color(1, 1, 0.5f);

        private ImpactParticleInteraction interaction;

        private void OnEnable()
        {
            interaction = target as ImpactParticleInteraction;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField("Particle Properties", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Particle Prefab", "The particle prefab to use."), GUILayout.Width(180));
            interaction.ParticlePrefab = EditorGUILayout.ObjectField(interaction.ParticlePrefab, typeof(ImpactParticlesBase), false) as ImpactParticlesBase;
            EditorGUILayout.EndHorizontal();

            if (interaction.ParticlePrefab == null)
            {
                EditorGUILayout.HelpBox("You must assign an Particle Prefab for this interaction.", MessageType.Error);
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Is Particle Looped", "Is the particle prefab looped?"), GUILayout.Width(180));
            interaction.IsParticleLooped = EditorGUILayout.Toggle(interaction.IsParticleLooped);
            EditorGUILayout.EndHorizontal();

            ImpactEditorUtilities.Separator();

            EditorGUILayout.LabelField("Interaction Properties", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Minimum Velocity", "The minimum velocity magnitude required to show particles."), GUILayout.Width(180));
            interaction.MinimumVelocity = EditorGUILayout.FloatField(interaction.MinimumVelocity);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Collision Normal Influence", "How much the collision normal should influence the calculated intensity."), GUILayout.Width(180));
            interaction.CollisionNormalInfluence = EditorGUILayout.Slider(interaction.CollisionNormalInfluence, 0, 1);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            interaction.EmitOnCollision = EditorGUILayout.ToggleLeft(new GUIContent("Emit On Collision", "Should particles be emitted on single collisions?"), interaction.EmitOnCollision);
            interaction.EmitOnSlide = EditorGUILayout.ToggleLeft(new GUIContent("Emit On Slide", "Should particles be emitted when sliding?"), interaction.EmitOnSlide);
            interaction.EmitOnRoll = EditorGUILayout.ToggleLeft(new GUIContent("Emit On Roll", "Should particles be emitted when rolling?"), interaction.EmitOnRoll);

            GUI.enabled = !interaction.IsParticleLooped && (interaction.EmitOnSlide || interaction.EmitOnRoll);

            interaction.EmissionInterval = ImpactEditorUtilities.RangeEditor(interaction.EmissionInterval, new GUIContent("Emission Interval (Min/Max)", "The interval at which particles should be emitted when sliding or rolling."));
            interaction.EmissionIntervalType = (InteractionIntervalType)EditorGUILayout.EnumPopup(new GUIContent("Interval Type", "Whether the Emission Interval is defined in Time (seconds) or Distance."), interaction.EmissionIntervalType);

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(interaction);
            }
        }
    }

    [CustomEditor(typeof(ImpactParticles))]
    [CanEditMultipleObjects]
    public class ImpactParticlesEditor : Editor
    {
        private SerializedProperty rotationModeProp;
        private SerializedProperty axisProp;
        private SerializedProperty poolSizeProp;
        private SerializedProperty poolFallbackModeProp;

        private ImpactParticles impactParticles;

        private void OnEnable()
        {
            rotationModeProp = serializedObject.FindProperty("_rotationMode");
            axisProp = serializedObject.FindProperty("_axis");
            poolSizeProp = serializedObject.FindProperty("_poolSize");
            poolFallbackModeProp = serializedObject.FindProperty("_poolFallbackMode");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.Separator();

            serializedObject.Update();

            impactParticles = target as ImpactParticles;

            EditorGUILayout.PropertyField(rotationModeProp, new GUIContent("Rotation Mode", "How should the particles be rotated?"));

            if (impactParticles.RotationMode != ImpactParticles.ParticleRotationMode.NoRotation)
                EditorGUILayout.PropertyField(axisProp, new GUIContent("Axis", "How should the object's axes be aligned to the surface?"));

            EditorGUILayout.Separator();

            EditorGUILayout.PropertyField(poolSizeProp, new GUIContent("Pool Size", "The size of the object pool that will be created for these particles."));
            EditorGUILayout.PropertyField(poolFallbackModeProp, new GUIContent("Pool Fallback Mode", "Defines behavior of the object pool when there is no available object to retrieve."));

            serializedObject.ApplyModifiedProperties();
        }
    }
}