namespace UnityEditor.VFXToolbox.ImageSequencer
{
    public class BreakFilpbookProcessorSettings : ProcessorSettingsBase
    {
        public int FlipbookNumU;
        public int FlipbookNumV;

        public override void Default()
        {
            FlipbookNumU = 5;
            FlipbookNumV = 5;
        }
    }
}

