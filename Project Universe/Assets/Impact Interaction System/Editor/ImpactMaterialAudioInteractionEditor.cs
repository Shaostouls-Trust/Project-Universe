using Impact.Interactions.Audio;
using Impact.Utility;
using UnityEditor;
using UnityEngine;

namespace Impact.EditorScripts
{
    [CustomEditor(typeof(ImpactAudioInteraction))]
    public class ImpactMaterialAudioInteractionEditor : Editor
    {
        private readonly Color warningColor = new Color(1, 1, 0.5f);

        private ImpactAudioInteraction interaction;

        private void OnEnable()
        {
            interaction = target as ImpactAudioInteraction;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            drawAudioProperties();

            ImpactEditorUtilities.Separator();

            drawInteractionProperties();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(interaction);
            }
        }

        private void drawInteractionProperties()
        {
            EditorGUILayout.LabelField("Interaction Properties", EditorStyles.boldLabel);

            interaction.VelocityRange = ImpactEditorUtilities.RangeEditor(interaction.VelocityRange, new GUIContent("Velocity Range (Min/Max)", "The velocity magnitude range to use when calculating collision intensity."));

            EditorGUILayout.Separator();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Collision Normal Influence", "How much the normal should affect the intensity."), GUILayout.Width(180));
            interaction.CollisionNormalInfluence = EditorGUILayout.Slider("", interaction.CollisionNormalInfluence, 0, 1);
            EditorGUILayout.EndHorizontal();

            interaction.ScaleVolumeWithVelocity = EditorGUILayout.ToggleLeft(new GUIContent("Scale Volume With Velocity", "Should volume be scaled based on the velocity?"), interaction.ScaleVolumeWithVelocity);

            if (interaction.ScaleVolumeWithVelocity)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Volume Scale Curve", ""), GUILayout.Width(150));
                interaction.VelocityVolumeScaleCurve = EditorGUILayout.CurveField("", interaction.VelocityVolumeScaleCurve);
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Separator();

            interaction.RandomPitchRange = ImpactEditorUtilities.RangeEditor(interaction.RandomPitchRange, new GUIContent("Pitch Randomness (Min/Max)", "Random multiplier for the pitch."));
            interaction.RandomVolumeRange = ImpactEditorUtilities.RangeEditor(interaction.RandomVolumeRange, new GUIContent("Volume Randomness (Min/Max)", "Random multiplier for the volume."));

            EditorGUILayout.Separator();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Slide Velocity Pitch Modifier", "How much to increase the pitch as sliding and rolling velocity increases."), GUILayout.Width(180));
            interaction.SlideVelocityPitchMultiplier = EditorGUILayout.Slider("", interaction.SlideVelocityPitchMultiplier, 0, 1);
            EditorGUILayout.EndHorizontal();
        }

        private void drawAudioProperties()
        {
            EditorGUILayout.LabelField("Audio Clips", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (interaction.CollisionAudioClips.Count == 0)
            {
                EditorGUILayout.Separator();

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("No Collision Audio Clips have been added.", GUILayout.Width(250));
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Separator();
            }
            else
            {
                GUILayout.Space(2);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Audio Clip Selection Mode", "Should audio clips be chosen based on Velocity or be chosen Randomly?"));
                interaction.CollisionAudioSelectionMode = (ImpactAudioInteraction.CollisionAudioClipSelectionMode)EditorGUILayout.EnumPopup(interaction.CollisionAudioSelectionMode);
                EditorGUILayout.EndHorizontal();

                for (int i = 0; i < interaction.CollisionAudioClips.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();

                    if (interaction.CollisionAudioSelectionMode == ImpactAudioInteraction.CollisionAudioClipSelectionMode.Velocity)
                        EditorGUILayout.LabelField((i + 1) + ")", GUILayout.Width(20));
                    else
                        GUILayout.Space(5);

                    interaction.CollisionAudioClips[i] = EditorGUILayout.ObjectField(interaction.CollisionAudioClips[i], typeof(AudioClip), false) as AudioClip;

                    Color originalColor = GUI.color;
                    GUI.color = warningColor;
                    if (GUILayout.Button(new GUIContent("X", "Remove"), GUILayout.Width(18), GUILayout.Height(15)))
                    {
                        interaction.CollisionAudioClips.RemoveAt(i);
                        i--;
                    }
                    GUI.color = originalColor;

                    EditorGUILayout.EndHorizontal();
                }

                GUILayout.Space(2);
            }

            EditorGUILayout.HelpBox("You can drag-and-drop Audio Clips here to add them.", MessageType.Info);

            EditorGUILayout.EndVertical();

            //Drag and drop
            Rect listRect = GUILayoutUtility.GetLastRect();
            string[] paths;
            bool drop = ImpactEditorUtilities.DragAndDropArea(listRect, out paths);

            if (drop && paths != null)
            {
                for (int i = 0; i < paths.Length; i++)
                {
                    AudioClip a = AssetDatabase.LoadAssetAtPath<AudioClip>(paths[i]);
                    if (a != null)
                        interaction.CollisionAudioClips.Add(a);
                }
            }

            EditorGUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Add Collision Audio Clip"))
            {
                interaction.CollisionAudioClips.Add(null);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            interaction.SlideAudioClip = EditorGUILayout.ObjectField(new GUIContent("Slide Audio", "The AudioClip to play when sliding."), interaction.SlideAudioClip, typeof(AudioClip), false) as AudioClip;
            interaction.RollAudioClip = EditorGUILayout.ObjectField(new GUIContent("Roll Audio", "The AudioClip to play when rolling."), interaction.RollAudioClip, typeof(AudioClip), false) as AudioClip;

            EditorGUILayout.Separator();

            interaction.AudioSourceTemplate = EditorGUILayout.ObjectField(new GUIContent("Audio Source Template", "The audio source whose properties will be used when playing sounds from this interaction."), interaction.AudioSourceTemplate, typeof(ImpactAudioSourceBase), false) as ImpactAudioSourceBase;

            if (interaction.AudioSourceTemplate == null)
            {
                EditorGUILayout.HelpBox("You must assign an Audio Source Template for sounds to play for this interaction.", MessageType.Error);
            }
        }
    }

    [CustomEditor(typeof(ImpactAudioSource))]
    [CanEditMultipleObjects]
    public class ImpactAudioSourceEditor : Editor
    {
        private SerializedProperty audioSourceProp;

        private SerializedProperty poolSizeProp;
        private SerializedProperty poolFallbackModeProp;

        private void OnEnable()
        {
            poolSizeProp = serializedObject.FindProperty("_poolSize");
            poolFallbackModeProp = serializedObject.FindProperty("_poolFallbackMode");
            audioSourceProp = serializedObject.FindProperty("_audioSource");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.Separator();

            serializedObject.Update();

            EditorGUILayout.PropertyField(audioSourceProp, new GUIContent("Audio Source", "The audio source to use for playing sounds."));

            EditorGUILayout.Separator();

            EditorGUILayout.PropertyField(poolSizeProp, new GUIContent("Pool Size", "The size of the object pool that will be created for these particles."));
            EditorGUILayout.PropertyField(poolFallbackModeProp, new GUIContent("Pool Fallback Mode", "Defines behavior of the object pool when there is no available object to retrieve."));

            serializedObject.ApplyModifiedProperties();
        }
    }
}