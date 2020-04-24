using UnityEngine;

namespace UnityEditor.VFXToolbox.ImageSequencer
{
    public class ProcessorInfo : ScriptableObject
    {
        public string ProcessorName;
        public bool Enabled;
        public ProcessorSettingsBase Settings;

        public static ProcessorInfo CreateDefault<T>(string name, bool enabled) where T : ProcessorSettingsBase
        {
            ProcessorInfo p = ScriptableObject.CreateInstance<ProcessorInfo>();
            p.ProcessorName = name;
            p.Enabled = enabled;
            p.Settings = ScriptableObject.CreateInstance<T>();
            p.Settings.Default();
            return p;
        }

        public override string ToString()
        {
            return ProcessorName + (Enabled ? "" : "Disabled") ;
        }

    }
}

