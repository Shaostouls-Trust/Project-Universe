using System.Collections.Generic;
using UnityEngine;
using System.Threading;

namespace Artngame.Orion.PlanetGenerationHEX
{
    public class HexPlanetGenerator : MonoBehaviour
    {
        public PlanetData[] planets;
        static public float G_viewDistance = 10000;
        public GameObject viewer;
        public float viewDistance = 10000;
        public float scaler = 1;
        public void Start()
        {
            foreach (var item in planets)
            {
                // Planet.AddPlanetToQueue(item)
                Planet.AddPlanetToQueue(item.newName, item.position, item.seed, item.generateRandomSeed, item.terrainStyle, item.seaLevel, item.generationData, item.ColorPerLayer,
                    item.population, item.material, item.seaMaterial, item.radius, item.subdivisions, item.chunckSubdivisions, scaler);
                Planet.StartDataQueue();
            }
        }
        private void Update()
        {
            //foreach (var item in planets)
            //{
            G_viewDistance = viewDistance;
            Planet.InstantiateIntoWorld();
            Planet.HideAndShow(viewer.transform.position);

            
            // }
        }
        private void LateUpdate()
        {
            if (Planet.planetListOBJS.Count > 0 && Time.fixedTime > 2 && Planet.planetListOBJS[0].transform.localScale != scaler * Vector3.one)
            {
                Planet.planetListOBJS[0].transform.localScale = scaler * Vector3.one;
            }
        }
    }
}
