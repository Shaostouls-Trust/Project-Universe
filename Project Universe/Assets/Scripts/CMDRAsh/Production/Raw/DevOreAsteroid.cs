using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectUniverse.Data.Libraries.Definitions;

namespace ProjectUniverse.Environment.World
{
    public class DevOreAsteroid : MonoBehaviour
    {
        [SerializeField] private string AsteroidName;
        [SerializeField] private int[] OreQualities;
        [SerializeField] private int[] OreZones;
        [SerializeField] private string[] OreTypes;
        [SerializeField] private int[] OreMasses;
        public MyAsteroidOrbiter orbitPath;
        private float distanceFromCenter;
        private OreDefinition OreDef;

        private void Start()
        {
            Vector3 centerPos = orbitPath.gameObject.transform.localPosition;
            float deltaX = centerPos.x - transform.localPosition.x;
            float deltaY = transform.localPosition.y - centerPos.y;
            float deltaZ = centerPos.z - transform.localPosition.z;
            distanceFromCenter = (float)Mathf.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
        }

        public int[] GetOreQualities()
        {
            return OreQualities;
        }
        public int[] GetOreZones()
        {
            return OreZones;
        }
        public string[] GetOreTypes()
        {
            return OreTypes;
        }
        public int[] GetOreMasses()
        {
            return OreMasses;
        }
        public void SetOreMass(int index, int mass)
        {
            OreMasses[index] = mass;
        }

        public string GetAsteroidName()
        {
            return AsteroidName;
        }

        // Update is called once per frame
        void Update()
        {
            /*
            float linearVelocity = orbitPath.GetAngularVelocity() * distanceFromCenter;//orbitPath.GetSolarRadius();
            //this.gameObject.GetComponent<Rigidbody>().AddForce(new Vector3(-1, -1, -1), ForceMode.Impulse);
            float T = (2 * Mathf.PI * distanceFromCenter)/linearVelocity;
            float w = 2 * Mathf.PI / T;
            //r(t) = Acos(wT) + Asin(wT)
            float xPos = (distanceFromCenter) * Mathf.Cos(w * Time.time);
            float zPos = (distanceFromCenter) * Mathf.Sin(w * Time.time);
            //sets all asteroids to same x and z pos
            this.GetComponent<Rigidbody>().MovePosition(new Vector3(xPos, transform.localPosition.y, zPos));
            */
        }
    }
}