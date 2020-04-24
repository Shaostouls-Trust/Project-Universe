using UnityEngine;

namespace UnityEditor.VFXToolbox.ImageSequencer
{
    public class CustomMaterialProcessorSettings : ProcessorSettingsBase
    {
        public Material material;

        public override void Default()
        {
            material = null;
        }
    }
}
