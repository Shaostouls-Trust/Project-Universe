using Impact.Materials;

namespace Impact
{
    /// <summary>
    /// Represents the influence of a Impact Material at a single point, such as for Terrains.
    /// </summary>
    public struct ImpactMaterialComposition
    {
        /// <summary>
        /// The Impact Material the composition represents.
        /// </summary>
        public IImpactMaterial Material;
        /// <summary>
        /// The 0 to 1 value representing the influence the material has.
        /// </summary>
        public float CompositionValue;

        /// <summary>
        /// Initialize the material composition data.
        /// </summary>
        /// <param name="material">The Impact Material the composition represents.</param>
        /// <param name="compositionValue">The 0 to 1 value representing the influence the material has.</param>
        public ImpactMaterialComposition(IImpactMaterial material, float compositionValue)
        {
            Material = material;
            CompositionValue = compositionValue;
        }
    }
}