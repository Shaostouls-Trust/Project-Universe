using UnityEngine;

namespace UnityEditor.VFXToolbox.ImageSequencer
{
    public class RemapColorProcessorSettings : ProcessorSettingsBase
    {
        public enum RemapColorSource
        {
            sRGBLuminance,
            LinearRGBLuminance,
            Alpha
        }

        public Gradient Gradient;
        public RemapColorSource ColorSource;

        public override void Default()
        {
            ColorSource = RemapColorSource.sRGBLuminance;
            DefaultGradient();
        }

        public void DefaultGradient()
        {
            Gradient = new Gradient();
            GradientColorKey[] colors = new GradientColorKey[2] { new GradientColorKey(Color.black, 0),new GradientColorKey(Color.white, 1) };
            GradientAlphaKey[] alpha = new GradientAlphaKey[2] { new GradientAlphaKey(0,0), new GradientAlphaKey(1,1) };
            Gradient.SetKeys(colors, alpha);
        }
    }
}
