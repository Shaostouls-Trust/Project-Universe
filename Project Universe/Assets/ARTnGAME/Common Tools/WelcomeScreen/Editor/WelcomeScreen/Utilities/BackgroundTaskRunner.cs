using System;
using System.Collections;
using UnityEditor;

namespace Artngame.CommonTools.WelcomeScreen.Utilities
{
    public class BackgroundTaskRunner
    {
        public static void StartBackgroundTask(IEnumerator update, Action end = null)
        {
            EditorApplication.CallbackFunction closureCallback = null;

            closureCallback = () =>
            {
                try
                {
                    if (update.MoveNext() == false)
                    {
                        if (end != null)
                            end();
                        EditorApplication.update -= closureCallback;
                    }
                }
                catch (Exception ex)
                {
                    if (end != null)
                        end();
                    UnityEngine.Debug.LogException(ex);
                    EditorApplication.update -= closureCallback;
                }
            };

            EditorApplication.update += closureCallback;
        }
    }
}
