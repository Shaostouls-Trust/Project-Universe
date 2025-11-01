using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.Orion.ProceduralPlanets
{
    [ExecuteInEditMode]
    public class PlanetTest : MonoBehaviour
    {
        public bool useRadius;
        public float radius;

        public bool spawnPlayer;
        public bool spawnShip;
        public PlayerController playerPrefab;
        public Ship shipPrefab;

        public Vector3 shipOffset;

        public int lodIndex;
        public GameObject sun;
        public Light testLight;
        CelestialBodyGenerator[] bodies;

        void Awake()
        {
            if (Application.isPlaying)
            {
                if (spawnPlayer)
                {
                    Destroy(FindFirstObjectByType<Camera>().gameObject);
                    Instantiate(playerPrefab, Vector3.up * radius * 1.2f, Quaternion.identity);
                }
                if (spawnShip)
                {
                    var ship = Instantiate(shipPrefab, Vector3.up * radius * 1.2f + Vector3.forward * 20 + shipOffset, Quaternion.identity);
                    ship.ToggleHatch();
                }
                if (sun)
                {
                    sun.gameObject.SetActive(true);
                    testLight.gameObject.SetActive(false);
                }
                bodies = FindObjectsByType<CelestialBodyGenerator>(FindObjectsSortMode.None);
            }
        }

        void Update()
        {
            if (Application.isPlaying)
            {
                foreach (var body in bodies)
                {
                    body.SetLOD(0);
                }
            }
        }

        void OnValidate()
        {
            if (!Application.isPlaying)
            {
                sun.SetActive(false);
            }
            var body = GetComponent<CelestialBody>();
            body.radius = radius;
            body.RecalculateMass();

            if (useRadius)
            {
                FindFirstObjectByType<CelestialBodyGenerator>().transform.localScale = Vector3.one * radius;
            }
            else
            {
                FindFirstObjectByType<CelestialBodyGenerator>().transform.localScale = Vector3.one;
            }
        }
    }
}