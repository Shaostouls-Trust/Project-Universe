namespace UnityEditor.VFXToolbox.ImageSequencer
{
    public class DecimateProcessorSettings : ProcessorSettingsBase
    {
        public ushort DecimateBy;

        public override void Default()
        {
            DecimateBy = 3;
        }
    }
}


