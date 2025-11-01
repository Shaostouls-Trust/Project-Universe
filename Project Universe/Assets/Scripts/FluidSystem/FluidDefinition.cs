using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUniverse.Data.Libraries.Definitions
{
    public class FluidDefinition
    {
        private string fluidID;
        private int flammability;
        private int combustibility;
        private float molarMass;
        private bool isNuclear;
        private float toxicity;

        // New thermodynamic properties
        private float boilingPoint; // K at 1 atm
        private float specificHeatLiquid; // J/(kg·K)
        private float specificHeatGas; // J/(kg·K)
        private float enthalpyVaporization; // J/kg
        private float liquidDensity; // kg/m³ at STP

        public FluidDefinition(string id, int flam, int comb, float molar, bool nuclear, float tox,
            float boilPoint, float cpLiquid, float cpGas, float hVap, float densityLiquid)
        {
            fluidID = id;
            flammability = flam;
            combustibility = comb;
            molarMass = molar;
            isNuclear = nuclear;
            toxicity = tox;
            boilingPoint = boilPoint;
            specificHeatLiquid = cpLiquid;
            specificHeatGas = cpGas;
            enthalpyVaporization = hVap;
            liquidDensity = densityLiquid;
        }

        public string FluidID
        {
            get { return fluidID; }
        }

        public int Flammability
        {
            get { return flammability; }
        }

        public int Combustibility
        {
            get { return combustibility; }
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

        public float BoilingPoint
        {
            get { return boilingPoint; }
        }

        public float SpecificHeatLiquid
        {
            get { return specificHeatLiquid; }
        }

        public float SpecificHeatGas
        {
            get { return specificHeatGas; }
        }

        public float EnthalpyVaporization
        {
            get { return enthalpyVaporization; }
        }

        public float LiquidDensity
        {
            get { return liquidDensity; }
        }
    }
}