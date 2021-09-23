using Impact.Materials;
using UnityEngine;

namespace Impact.Objects
{
    /// <summary>
    /// Component for Impact Objects that have only a single material.
    /// </summary>
    [AddComponentMenu("Impact/Impact Object Single Material", 0)]
    public class ImpactObjectSingleMaterial : ImpactObjectBase
    {
        [SerializeField]
        private ImpactMaterialBase _material;

        /// <summary>
        /// The ImpactMaterialBase this object uses.
        /// </summary>
        public ImpactMaterialBase Material
        {
            get { return _material; }
            set
            {
                _material = value;
                hasMaterial = _material != null;

                if (hasMaterial)
                    cachedMaterialTypeComposition = _material.GetSingleMaterialComposition();
            }
        }

        private ImpactMaterialComposition[] cachedMaterialTypeComposition;
        private bool hasMaterial;

        protected virtual void Awake()
        {
            hasMaterial = Material != null;

            if (hasMaterial)
                cachedMaterialTypeComposition = Material.GetSingleMaterialComposition();
        }

        public override int GetMaterialCompositionNonAlloc(Vector3 point, ImpactMaterialComposition[] results)
        {
            if (!hasMaterial)
            {
                Debug.LogError($"Cannot get material composition for ImpactObjectSingleMaterial {gameObject.name} because it has no Material.");
                return 0;
            }

            int l = Mathf.Min(results.Length, cachedMaterialTypeComposition.Length);

            for (int i = 0; i < l; i++)
            {
                results[i] = cachedMaterialTypeComposition[i];
            }

            return cachedMaterialTypeComposition.Length;
        }

        public override IImpactMaterial GetPrimaryMaterial(Vector3 point)
        {
            return GetPrimaryMaterial();
        }

        public override IImpactMaterial GetPrimaryMaterial()
        {
            if (!hasMaterial)
                Debug.LogError($"ImpactObjectSingleMaterial {gameObject.name} has no Material.");

            return _material;
        }
    }
}

