using UnityEngine;

namespace UnityEditor.VFXToolbox.ImageSequencer
{
    public class RemoveBackgroundSettings : ProcessorSettingsBase
    {
        public Color BackgroundColor;

        public override void Default()
        {
            BackgroundColor = new Color(0.25f,0.25f,0.25f,0.0f);
        }
    }
}
