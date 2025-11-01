using ProjectUniverse.Data.Libraries;
using ProjectUniverse.Data.Libraries.Definitions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 Flamability 0-10 : how easily it catches fire
 Combustability 0-10 : How easily it 'splodes when exposed to sparks or fire
 Reactants[] : What it can combust with (proportion gas1 | proportion gas2 | proportion gas3 ||| pn)
 Nuclear: bool
 Toxicity 0-10 : Level of protection needed and dmg amount of exposure
 Temp:
 Density: 
 */

namespace ProjectUniverse.Environment.Gas { 
    public class IGas //: MonoBehaviour
    {
        //Library properties
        private string IDname;
        private int flamability;//does it catch on fire (how well)
        private int combustability;//does it explode (how well)
        private string[] reactants;
        private bool nuclear;
        private float toxicity;//ppm/1,000,000 or (0-100% composition) in some volume.
        private float MolarMass = 31.9988f;//15.9994 is for one O. oxygen is diatomic, so 
        private float specificHeat=920f;//J/kg K
        //Mixed Property
        private float density;//in g/L IE oxygen is 1.427 g/L at 1 atm, 273.15K
        private float stpDensity;
        //Instanced property
        private float temp;
        private float concentration;//amount of the gas in the local volume (m^3)
        private float volume_m3;//size of the local volume in m^3
        private float mass; // in grams
        private float localPressure;//pressure of the gas in it's local volume
        private GasDefinition definition = null;

        override
        public string ToString()
        {
            string compile = "" + IDname + " at " + temp + "F, " + density + "g/L, " + concentration + "m3 in " + volume_m3 + "m3 at " + localPressure + "atm. Mass: " + mass + "g";
            return compile;
        }

        public IGas(string gasID, float mytemp, float myconcentration)
        {
            IDname = gasID;
            temp = mytemp;
            concentration = myconcentration;

            //fill other values from gasID lib
            if (GasLibrary.GasDictionary.TryGetValue(IDname, out definition))
            {
                flamability = definition.Flamability;
                combustability = definition.Combustability;
                nuclear = definition.IsNuclear;
                toxicity = definition.Toxicity;
                MolarMass = definition.MolarMass;
                specificHeat = definition.SpecificHeat;
                stpDensity = (1f * MolarMass) / (0.0821f * 273.15f);
            }

            // Initialize mass based on concentration (assuming STP or default conditions)
            mass = 0f; // Cannot calculate without pressure
        }

        public IGas(IGas otherGas)
        {
            IDname = otherGas.GetIDName();
            temp = otherGas.GetTemp();
            localPressure = otherGas.GetLocalPressure();
            concentration = otherGas.GetConcentration();
            volume_m3 = otherGas.GetLocalVolume();
            mass = otherGas.GetMass(); // NEW

            //gaslib data
            if (GasLibrary.GasDictionary.TryGetValue(IDname, out definition))
            {
                flamability = definition.Flamability;
                combustability = definition.Combustability;
                nuclear = definition.IsNuclear;
                toxicity = definition.Toxicity;
                MolarMass = definition.MolarMass;
                specificHeat = definition.SpecificHeat;
                stpDensity = (1f * MolarMass) / (0.0821f * 273.15f);
            }
        }

        public IGas(string gasID, float mytemp, float myconcentration, float localpressure, float localvolume)
        {
            IDname = gasID;
            temp = mytemp;
            localPressure = localpressure;
            concentration = myconcentration;
            volume_m3 = localvolume;

            //gaslib data
            if (GasLibrary.GasDictionary.TryGetValue(IDname, out definition))
            {
                flamability = definition.Flamability;
                combustability = definition.Combustability;
                nuclear = definition.IsNuclear;
                toxicity = definition.Toxicity;
                MolarMass = definition.MolarMass;
                specificHeat = definition.SpecificHeat;
                stpDensity = (1f * MolarMass) / (0.0821f * 273.15f);
            }

            // Calculate initial mass from ideal gas law: n = PV/RT, mass = n * M
            // Convert temp to Kelvin
            float tempK = ((temp - 32f) * (5f / 9f)) + 273.15f;
            // Convert concentration (m³) to liters
            float volumeL = concentration * 1000f;

            if (tempK > 0f && localPressure > 0f)
            {
                // n = (P * V) / (R * T)
                float moles = (localPressure * volumeL) / (0.0821f * tempK);
                mass = moles * MolarMass; // in grams
            }
            else
            {
                mass = 0f;
            }
        }

        public float MassToMoles(float massInGrams)
        {
            if (MolarMass <= 0f) return 0f;
            return massInGrams / MolarMass;
        }

        // NEW METHOD: Remove mass from the gas
        public void RemoveMass(float massInGrams)
        {
            Debug.Log("less: "+massInGrams);
            mass = Mathf.Max(0f, mass - massInGrams);
            Debug.Log("new mass: " + mass);
            UpdateConcentrationFromMass();
        }

        // NEW METHOD: Add mass to the gas
        public void AddMass(float massInGrams)
        {
            mass += massInGrams;
            UpdateConcentrationFromMass();
        }

        // NEW METHOD: Get current mass
        public float GetMass()
        {
            return mass;
        }

        // NEW HELPER: Update concentration based on current mass
        private void UpdateConcentrationFromMass()
        {
            // Convert mass to moles
            float moles = MassToMoles(mass);

            // Convert temp to Kelvin
            float tempK = ((temp - 32f) * (5f / 9f)) + 273.15f;

            if (tempK <= 0f || localPressure <= 0f || moles <= 0f)
            {
                concentration = 0f;
                return;
            }

            // Calculate volume from ideal gas law: V = nRT/P
            // Result in liters
            float volumeL = (moles * 0.0821f * tempK) / localPressure;

            // Convert to m³
            concentration = volumeL / 1000f;
        }

        public string GetIDName()
        {
            return IDname;
        }
        public int GetFlamabitity()
        {
            return flamability;
        }
        public int GetCombustability()
        {
            return combustability;
        }
        public string[] GetReactants()
        {
            return reactants;
        }
        public bool GetNuclear()
        {
            return nuclear;
        }
        public float GetToxicity()
        {
            return toxicity;
        }

        public float SpecificHeat
        {
            get { return specificHeat; }
        }

        //Temp will affect density
        public float GetTemp()
        {
            return temp;
        }

        //public float TempKelvin
        //{
        //    get { return (5f/9f) * (temp - 32f) + 273.15f; }
        //}

        //Density value may not be at STP.
        public float GetDensity()
        {
            return density;
        }
        //STP value for density
        public float GetSTPDensity()
        {
            return stpDensity;
        }

        //amount per 1 m^3
        public float GetConcentration()
        {
            return concentration;
        }

        public void AddConcentration(float amount)
        {
            concentration += amount;
        }

        public void SetTemp(float newTemp)
        {
            temp = newTemp;
            UpdateConcentrationFromMass(); // Recalculate concentration when temp changes
        }

        //public void SetTempKelvin(float kelvin)
        //{
        //    temp = ((temp - 273.15f) * (9f / 5f)) + 32f;
        //}

        public void SetDensity(float newDensity)
        {
            density = newDensity;
        }
        public void SetLocalPressure(float pipePressure)
        {
            localPressure = pipePressure;
            UpdateConcentrationFromMass(); // Recalculate concentration when pressure changes
        }
        public float GetLocalPressure()
        {
            return localPressure;
        }
        public void SetLocalVolume(float localVolume)
        {
            volume_m3 = localVolume;
        }
        public float GetLocalVolume()
        {
            return volume_m3;
        }

        public float GetMolarMass()
        {
            return MolarMass;
        }

        /// <summary>
        /// Calculate the density of the gas based on it's temp, pressure
        /// </summary>
        public float CalculateAtmosphericDensity()
        {
            //convert temp(F) to K
            //(32°F − 32) × 5/9 + 273.15
            float tempK = ((temp - 32f) * (5f / 9f)) + 273.15f;
            //P[atm]M[g/mol] / R[atm*L/mol*K]T[K] = d[kg/L]
            density = (localPressure * MolarMass) / (0.0821f * tempK);
            return density;
        }

        public void SetConcentration(float newConcentration)
        {
            concentration = newConcentration;
        }
    }
}
