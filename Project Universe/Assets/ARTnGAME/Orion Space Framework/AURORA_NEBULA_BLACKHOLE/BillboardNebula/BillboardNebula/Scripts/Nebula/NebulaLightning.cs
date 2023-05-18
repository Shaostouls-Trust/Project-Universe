using UnityEngine;
namespace Artngame.Orion.BillboardNebula
{
    [RequireComponent(typeof(Nebula))]
    public class NebulaLightning : MonoBehaviour
    {
        public Color color;
        public bool flash = false;

        Nebula parentNeb;

        void Awake()
        {
            parentNeb = GetComponent<Nebula>();
        }

        void Update()
        {
            if (flash)
            {
                RenderSettings.ambientLight = color;
            }

            else
            {
                RenderSettings.ambientLight = parentNeb.ambientLight;
            }
        }
    }
}