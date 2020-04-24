namespace UnityEditor.VFXToolbox.ImageSequencer
{
    public class PremultiplyAlphaProcessorSettings : ProcessorSettingsBase
    {
        public bool RemoveAlpha;
        public float AlphaValue;

        public override void Default()
        {
            RemoveAlpha = false;
            AlphaValue = 1.0f;
        }
    }
}
