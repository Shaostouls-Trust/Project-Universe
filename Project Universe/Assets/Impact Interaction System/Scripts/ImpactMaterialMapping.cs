using Impact.Materials;
using UnityEngine;

namespace Impact
{
    [System.Serializable]
    public class ImpactPhysicMaterialMapping
    {
        public PhysicMaterial PhysicMaterial;
        public ImpactMaterialBase ImpactMaterial;
    }

    [System.Serializable]
    public class ImpactPhysicsMaterial2DMapping
    {
        public PhysicsMaterial2D PhysicsMaterial2D;
        public ImpactMaterialBase ImpactMaterial;
    }
}
