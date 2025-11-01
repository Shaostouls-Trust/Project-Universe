using System;
using System.Collections.Generic;
using Artngame.CommonTools.WelcomeScreen.PreferenceDefinition;
using UnityEditor;
using UnityEngine;

//v0.1
using Artngame.CommonTools.WelcomeScreen.GuiElements;

namespace Artngame.CommonTools.WelcomeScreen
{
    public abstract class ProductPreferenceBase
    {
        public static readonly Dictionary<string, object> GlobalPrefKeyToValueMap = new Dictionary<string, object>();

        protected static void RenderGuiCommon(List<ProjectEditorPreferenceDefinitionBase> preferenceDefinitions)
        {
            var previousLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 250;

            foreach (var preferenceDefinition in preferenceDefinitions)
            {
                RenderGuiAndPersistInput(preferenceDefinition);
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Reset and Forget All"))
            {

                foreach (var preferenceDefinition in preferenceDefinitions)
                {
                    var oldValue = preferenceDefinition.GetEditorPersistedValueOrDefault();
                    EditorPrefs.DeleteKey(preferenceDefinition.PreferenceKey);
                    GlobalPrefKeyToValueMap[preferenceDefinition.PreferenceKey] = preferenceDefinition.DefaultValue;
                    preferenceDefinition?.HandleOnEditorPersistedValueChange?.Invoke(preferenceDefinition.DefaultValue, oldValue);
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = previousLabelWidth;
        }

        public static void RenderGuiAndPersistInput(ProjectEditorPreferenceDefinitionBase preferenceDefinition)
            => RenderGuiAndPersistInput(preferenceDefinition, null, new GUILayoutOption[0]);

        public static void RenderGuiAndPersistInput(ProjectEditorPreferenceDefinitionBase preferenceDefinition, GUIStyle style, params GUILayoutOption[] layoutOptions)
        {
            EditorGUI.BeginChangeCheck();
            if (!GlobalPrefKeyToValueMap.TryGetValue(preferenceDefinition.PreferenceKey, out var currentValue))
            {
                currentValue = preferenceDefinition.GetEditorPersistedValueOrDefault();
            }
            var newValue = preferenceDefinition.RenderEditorAndCaptureInput(currentValue, style, layoutOptions);
            GlobalPrefKeyToValueMap[preferenceDefinition.PreferenceKey] = newValue;
            if (EditorGUI.EndChangeCheck())
            {
                preferenceDefinition.SetEditorPersistedValue(newValue);
            }
        }

        //v0.1
        public static void RenderURL(string name, string URL)
        {
            EditorGUI.BeginChangeCheck();
            //new OpenUrlButton("Email", $"mailto:artengames@gmail.com");
            if (GUILayout.Button(name))
            {
                Application.OpenURL(URL);
            }
            //if (!GlobalPrefKeyToValueMap.TryGetValue(preferenceDefinition.PreferenceKey, out var currentValue))
            //{
            //    currentValue = preferenceDefinition.GetEditorPersistedValueOrDefault();
            //}
            //var newValue = preferenceDefinition.RenderEditorAndCaptureInput(currentValue, style, layoutOptions);
            //GlobalPrefKeyToValueMap[preferenceDefinition.PreferenceKey] = newValue;
            if (EditorGUI.EndChangeCheck())
            {
               // preferenceDefinition.SetEditorPersistedValue(newValue);
            }
        }

        public static void LoadDefaults(List<ProjectEditorPreferenceDefinitionBase> preferenceDefinitions)
        {
            foreach (var preferenceDefinition in preferenceDefinitions)
            {
                GlobalPrefKeyToValueMap[preferenceDefinition.PreferenceKey] = preferenceDefinition.GetEditorPersistedValueOrDefault();
            }
        }

        protected static SettingsProvider GenerateProvider(string productName, string[] productKeywords, Action renderGuiFn)
        {
            var provider = new SettingsProvider($"Preferences/{productName}", SettingsScope.User)
            {
                guiHandler = (string searchContext) => { renderGuiFn(); },
                keywords = new HashSet<string>(productKeywords),
            };
            return provider;
        }

        public static EnumProjectEditorPreferenceDefinition CreateDefaultShowOptionPreferenceDefinition()
        {
            return new EnumProjectEditorPreferenceDefinition("Show start screen on Unity launch",
                "StartUpShowOption",
                StartupShowMode.OnNewUpdate,
                typeof(StartupShowMode)
            );
        }

        public static TextProjectEditorPreferenceDefinition CreateDefaultUserIdDefinition(string productName)
        {
            return new TextProjectEditorPreferenceDefinition("Unique User Id",
                "UserId",
                Guid.NewGuid().ToString()
            ).AsGlobal<TextProjectEditorPreferenceDefinition>(productName);
        }

        public static TextProjectEditorPreferenceDefinition CreateDefaultLastUpdateText()
        {
            return new TextProjectEditorPreferenceDefinition("Last Update Text",
                "LastUpdateText",
                string.Empty
            );
        }

        public enum StartupShowMode
        {
            Never,
            OnNewUpdate,
            Always
        }
    }
}