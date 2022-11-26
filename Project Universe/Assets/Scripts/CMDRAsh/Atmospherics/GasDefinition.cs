using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUniverse.Data.Libraries.Definitions
{
    public class GasDefinition
    {
        private string gasID;
        private int flamability;
        private int combustability;
        private float molarMass;
        private bool isNuclear;
        private float toxicity;
        private float specificHeat;

        public GasDefinition(string id, int flam, int comb, float molar, bool nuclear, float tox, float cp)
        {
            gasID = id;
            flamability = flam;
            combustability = comb;
            molarMass = molar;
            isNuclear = nuclear;
            toxicity = tox;
            specificHeat = cp;
        }
        
        public string GasID
        {
            get { return gasID; }
        }

        public int Flamability
        {
            get { return flamability; }
        }

        public int Combustability
        {
            get { return combustability; }
        }
        
        public float MolarMass
        {
            get { return molarMass; }
        }
        
        public bool IsNuclear
        {
            get { return isNuclear; }
        }
        
        public float Toxicity
        {
            get { return toxicity; }
        }

        public float SpecificHeat
        {
            get { return specificHeat; }
        }
    }
}