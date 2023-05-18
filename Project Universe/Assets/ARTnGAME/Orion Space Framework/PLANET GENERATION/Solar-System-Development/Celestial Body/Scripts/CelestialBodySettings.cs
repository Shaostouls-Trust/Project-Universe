using UnityEngine;
namespace Artngame.Orion.ProceduralPlanets
{
    [CreateAssetMenu(menuName = "Celestial Body/Settings Holder")]
    public class CelestialBodySettings : ScriptableObject
    {
        public CelestialBodyShape shape;
        public CelestialBodyShading shading;
    }
}