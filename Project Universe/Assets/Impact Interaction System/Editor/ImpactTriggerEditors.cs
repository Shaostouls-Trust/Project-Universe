using Impact.Triggers;
using UnityEditor;
using UnityEngine;

namespace Impact.EditorScripts
{
    public class ImpactTriggerBaseEditor : Editor
    {
        protected SerializedProperty targetProp;
        protected SerializedProperty useMaterialCompositionProp;
        protected SerializedProperty contactModeProp;
        protected SerializedProperty highPriorityProp;
        protected SerializedProperty enabledProp;

        protected virtual void OnEnable()
        {
            targetProp = serializedObject.FindProperty("_target");
            useMaterialCompositionProp = serializedObject.FindProperty("_useMaterialComposition");
            contactModeProp = serializedObject.FindProperty("_contactMode");
            highPriorityProp = serializedObject.FindProperty("_highPriority");
            enabledProp = serializedObject.FindProperty("_enabled");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            inspectorGUICore();

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void inspectorGUICore()
        {
            drawEnabledProperty();

            EditorGUILayout.Separator();

            drawTargetProperty();

            EditorGUILayout.Separator();

            drawContactModeProperty();
            drawMaterialCompositionProperty();
            drawHighPriorityProperty();
        }

        protected void drawEnabledProperty()
        {
            EditorGUILayout.PropertyField(enabledProp, new GUIContent("Enabled", "Should this trigger process collisions? You should use this instead of the normal enabled property because collision messages are still sent to disabled components."));
        }

        protected void drawTargetProperty()
        {
            EditorGUILayout.ObjectField(targetProp, new GUIContent("Impact Object", "The object to send interaction data to. This can be null if there are children objects with different materials."));

            if (targetProp.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("If no Impact Object is assigned, this trigger will attempt to find one using the collider from the collision data.", MessageType.Info);
            }
        }

        protected void drawContactModeProperty()
        {
            EditorGUILayout.PropertyField(contactModeProp, new GUIContent("Contacts Mode", "How collision contacts should be processed."));
        }

        protected void drawMaterialCompositionProperty()
        {
            EditorGUILayout.PropertyField(useMaterialCompositionProp, new GUIContent("Use Material Composition", "Should multiple interactions be played for each material at the contact point?"));
        }

        protected void drawHighPriorityProperty()
        {
            EditorGUILayout.PropertyField(highPriorityProp, new GUIContent("High Priority", "Should this trigger ignore the Physics Interactions Limit set in the Impact Manager?"));
        }
    }

    public class ImpactSlideAndRollTriggerBaseEditor : ImpactTriggerBaseEditor
    {
        private SerializedProperty slideModeProp;
        private SerializedProperty rollModeProp;

        protected override void OnEnable()
        {
            base.OnEnable();

            slideModeProp = serializedObject.FindProperty("_slideMode");
            rollModeProp = serializedObject.FindProperty("_rollMode");
        }

        protected override void inspectorGUICore()
        {
            base.inspectorGUICore();

            EditorGUILayout.Separator();

            EditorGUILayout.PropertyField(slideModeProp, new GUIContent("Slide Mode", "How should the trigger play sounds for Sliding?"));
            EditorGUILayout.PropertyField(rollModeProp, new GUIContent("Roll Mode", "How should the trigger play sounds for Rolling?"));

            EditorGUILayout.Separator();
        }
    }

    [CustomEditor(typeof(ImpactSlideAndRollTrigger3D))]
    [CanEditMultipleObjects]
    public class ImpactSlideAndRollTrigger3DEditor : ImpactSlideAndRollTriggerBaseEditor { }

    [CustomEditor(typeof(ImpactSlideAndRollTrigger2D))]
    [CanEditMultipleObjects]
    public class ImpactSlideAndRollTrigger2DEditor : ImpactSlideAndRollTriggerBaseEditor { }

    [CustomEditor(typeof(ImpactCollisionTrigger3D))]
    [CanEditMultipleObjects]
    public class ImpactCollisionTrigger3DEditor : ImpactTriggerBaseEditor { }

    [CustomEditor(typeof(ImpactCollisionTrigger2D))]
    [CanEditMultipleObjects]
    public class ImpactCollisionTrigger2DEditor : ImpactTriggerBaseEditor { }

    public class ImpactSimpleCollisionTriggerBaseEditor : ImpactTriggerBaseEditor
    {
        protected override void inspectorGUICore()
        {
            drawEnabledProperty();

            EditorGUILayout.Separator();

            drawTargetProperty();

            EditorGUILayout.Separator();
        }
    }

    [CustomEditor(typeof(ImpactSimpleCollisionTrigger3D))]
    [CanEditMultipleObjects]
    public class ImpactSimpleCollisionTrigger3DEditor : ImpactSimpleCollisionTriggerBaseEditor { }

    [CustomEditor(typeof(ImpactSimpleCollisionTrigger2D))]
    [CanEditMultipleObjects]
    public class ImpactSimpleCollisionTrigger2DEditor : ImpactSimpleCollisionTriggerBaseEditor { }

    public class ImpactSpeculativeCollisionTriggerBaseEditor : ImpactTriggerBaseEditor
    {
        private SerializedProperty maxCollisionsProp;
        private SerializedProperty contactPointThresholdProp;
        private SerializedProperty contactPointLifetimeProp;

        protected override void OnEnable()
        {
            base.OnEnable();

            maxCollisionsProp = serializedObject.FindProperty("_maxCollisionsPerFrame");
            contactPointThresholdProp = serializedObject.FindProperty("_contactPointComparisonThreshold");
            contactPointLifetimeProp = serializedObject.FindProperty("_contactPointLifetime");
        }

        protected override void inspectorGUICore()
        {
            drawEnabledProperty();

            EditorGUILayout.Separator();

            drawTargetProperty();

            EditorGUILayout.Separator();

            drawMaterialCompositionProperty();
            drawHighPriorityProperty();

            EditorGUILayout.Separator();

            EditorGUILayout.PropertyField(maxCollisionsProp, new GUIContent("Max Collisions Per Frame", "The maximum number of collisions that can be created per frame."));
            EditorGUILayout.PropertyField(contactPointThresholdProp, new GUIContent("Contact Point Comparison", "Value used for comparing contact point relative positions. Contact points will be considered identical if the sqrMagnitude of their difference is less than this value."));
            EditorGUILayout.PropertyField(contactPointLifetimeProp, new GUIContent("Contact Point Lifetime", "How many frames should a contact point be alive for before it is removed from the list of active contacts? Increasing this value can reduce the likelyhood of interactions happening in quick succession for the same contact point."));

            EditorGUILayout.Separator();
        }
    }

    [CustomEditor(typeof(ImpactSpeculativeCollisionTrigger3D))]
    [CanEditMultipleObjects]
    public class ImpactSpeculativeCollisionTrigger3DEditor : ImpactSpeculativeCollisionTriggerBaseEditor { }

    [CustomEditor(typeof(ImpactSpeculativeCollisionTrigger2D))]
    [CanEditMultipleObjects]
    public class ImpactSpeculativeCollisionTrigger2DEditor : ImpactSpeculativeCollisionTriggerBaseEditor { }

    [CustomEditor(typeof(ImpactParticleCollisionTrigger), false)]
    [CanEditMultipleObjects]
    public class ImpactParticleCollisionTriggerEditor : ImpactTriggerBaseEditor
    {
        private SerializedProperty particlesProp;

        protected override void OnEnable()
        {
            base.OnEnable();

            particlesProp = serializedObject.FindProperty("_particles");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            drawEnabledProperty();

            EditorGUILayout.Separator();

            drawTargetProperty();

            EditorGUILayout.Separator();

            drawMaterialCompositionProperty();
            drawHighPriorityProperty();

            EditorGUILayout.Separator();

            EditorGUILayout.PropertyField(particlesProp, new GUIContent("Particles", "The particles to use for collisions. If not set, this object will recieve collisions from particles."));

            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(ImpactVelocityCollisionTrigger3D))]
    [CanEditMultipleObjects]
    public class ImpactVelocityCollisionTrigger3DEditor : ImpactVelocityCollisionTriggerBaseEditor { }

    [CustomEditor(typeof(ImpactVelocityCollisionTrigger2D))]
    [CanEditMultipleObjects]
    public class ImpactVelocityCollisionTrigger2DEditor : ImpactVelocityCollisionTriggerBaseEditor { }

    public class ImpactVelocityCollisionTriggerBaseEditor : ImpactTriggerBaseEditor
    {
        private SerializedProperty influenceProperty;

        protected override void OnEnable()
        {
            base.OnEnable();

            influenceProperty = serializedObject.FindProperty("_velocityChangeInfluence");
        }

        protected override void inspectorGUICore()
        {
            base.inspectorGUICore();

            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(influenceProperty, new GUIContent("Velocity Change Influence", "How much the velocity change affects the resulting collision velocity. 1 means full influence. 0 behaves just like the normal Impact Collision Trigger."));

        }
    }
}
