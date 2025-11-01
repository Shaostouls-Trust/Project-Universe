using System;
using Artngame.CommonTools.WelcomeScreen.Utilities;
using UnityEditor;
using UnityEngine;

namespace Artngame.CommonTools.WelcomeScreen
{
    public delegate void AdditionalOnWindowOpened(bool isFirstRun);

    public abstract class WelcomeScreenInitializerBase
    {
        protected static void HandleUnityStartup(Action openWindow, string getUpdatesUrl, AdditionalOnWindowOpened additionalOnWindowOpen = null)
        {
            EditorApplication.CallbackFunction handleEditorUpdate = null;
            handleEditorUpdate = () =>
            {
                HandleEditorUpdate(openWindow, getUpdatesUrl, additionalOnWindowOpen); 
                EditorApplication.update -= handleEditorUpdate;
            };

            EditorApplication.update += handleEditorUpdate;
        }

        private static void HandleEditorUpdate(Action openWindow, string getUpdatesUrl, AdditionalOnWindowOpened additionalOnWindowOpen = null)
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode) 
            {
                var defaultShowAtStartupPreferenceDefinition = ProductPreferenceBase.CreateDefaultShowOptionPreferenceDefinition();
                ProductPreferenceBase.StartupShowMode showWindowOnStartupMode = ProductPreferenceBase.StartupShowMode.Never;
                if (!EditorPrefs.HasKey(defaultShowAtStartupPreferenceDefinition.PreferenceKey)) //first run / import
                {
                    showWindowOnStartupMode = (ProductPreferenceBase.StartupShowMode)defaultShowAtStartupPreferenceDefinition.DefaultValue;
                    defaultShowAtStartupPreferenceDefinition.SetEditorPersistedValue(showWindowOnStartupMode);
                    OpenWindow(openWindow, additionalOnWindowOpen, true);

                    if (showWindowOnStartupMode != ProductPreferenceBase.StartupShowMode.Never)
                    {
                        GetUpdates(openWindow, getUpdatesUrl);
                    }
                }
                else
                {
                    if (Time.realtimeSinceStartup < 10) //static ctor will also be executed on script changes / domain reload, make sure editor is just turned on
                    {
                        showWindowOnStartupMode = (ProductPreferenceBase.StartupShowMode)defaultShowAtStartupPreferenceDefinition.GetEditorPersistedValueOrDefault();
                        if (showWindowOnStartupMode == ProductPreferenceBase.StartupShowMode.Always)
                        {
                            OpenWindow(openWindow, additionalOnWindowOpen, false);
                        }

                        if (showWindowOnStartupMode != ProductPreferenceBase.StartupShowMode.Never)
                        {
                            GetUpdates(openWindow, getUpdatesUrl);
                        }
                    }
                }
            }
        }

        private static void GetUpdates(Action openWindow, string getUpdatesUrl)
        {
            if(string.IsNullOrEmpty(getUpdatesUrl)) return;

            BackgroundTaskRunner.StartBackgroundTask(HttpRequestManager.SendRequest(getUpdatesUrl, (www, textResult) =>
            {
                try
                { 
                    var update = UpdateData.CreateFromJSON(textResult);
                    if (update != null)
                    {
                        ProductPreferenceBase.CreateDefaultLastUpdateText().SetEditorPersistedValue(update.Text);
                        if (!string.IsNullOrWhiteSpace(update.Text))
                        {
                            openWindow();
                        }
                    }
                }
                catch
                {
                    //don't show error
                }
            }));
        }

        private static void OpenWindow(Action openWindow, AdditionalOnWindowOpened additionalOnWindowOpen, bool isFirstRun)
        {
            openWindow();
            additionalOnWindowOpen?.Invoke(isFirstRun);
        }
    }

    [Serializable]
    internal class UpdateData
    {
        public string Text;

        public static UpdateData CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<UpdateData>(jsonString);
        }

        public UpdateData(string text)
        {
            Text = text;
        }
    }
}