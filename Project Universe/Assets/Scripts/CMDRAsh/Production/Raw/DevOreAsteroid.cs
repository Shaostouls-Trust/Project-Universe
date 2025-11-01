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
        private OreDefinition OreDef;

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
    }
}