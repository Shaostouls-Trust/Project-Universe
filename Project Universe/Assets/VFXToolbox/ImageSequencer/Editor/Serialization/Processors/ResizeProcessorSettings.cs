namespace UnityEditor.VFXToolbox.ImageSequencer
{
    public class ResizeProcessorSettings : ProcessorSettingsBase
    {
        public ushort Width;
        public ushort Height;

        public override void Default()
        {
            Width = 256;
            Height = 256;
        }

    }
}
