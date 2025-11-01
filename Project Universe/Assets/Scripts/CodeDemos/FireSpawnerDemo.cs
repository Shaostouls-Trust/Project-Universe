using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace ProjectUniverse.PowerSystem.CollisionDemo
{
    public class FireSpawnerDemo : MonoBehaviour
    {
        public GameObject firePrefab;
        public EnvironmentalThreatManager envManager;

        [ContextMenu("Spawn Fire Here")]
        void SpawnFire()
        {
            GameObject fireObj = new GameObject("TestFire");
            fireObj.transform.position = transform.position;
            fireObj.AddComponent<DemoFire>();
            //if (firePrefab)
            //{
            //    Instantiate(firePrefab, transform.position, Quaternion.identity);
            //}
        }

        void Update()
        {
            // Spawn fire on mouse click for testing
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (firePrefab)
                    {
                        Instantiate(firePrefab, hit.point, Quaternion.identity);
                    }
                }
            }
        }
    }
}