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
        [SerializeField] private string IDname;
        [SerializeField] private int flamability;
        [SerializeField] private int combustability;
        [SerializeField] private string[] reactants;
        //[SerializeField] private int volitility;
        [SerializeField] private bool nuclear;
        [SerializeField] private int toxicity;
        [SerializeField] private float MolarMass = 31.9988f;//15.9994 is for one O. oxygen is diatomic, so 
                                                            //Mixed Property
        [SerializeField] private float density;//in g/L IE oxygen is 1.43 g/L
                                               //Instanced property
        [SerializeField] private float temp;
        [SerializeField] private float concentration;//amount of the gas in the local volume
        [SerializeField] private float volume_m3;//amount of gas in m^3
        [SerializeField] private float localPressure;//pressure of the gas in it's local volume

        override
        public string ToString()
        {
            string compile = "" + IDname + " at " + temp + "F, " + density + "g/L, " + concentration + "m3 in " + volume_m3 + "m3 at " + localPressure + "atm";
            return compile;
        }

        public IGas(string gasID, float mytemp, float myconcentration)//, float localpressure)
        {
            IDname = gasID;
            temp = mytemp;
            //localPressure = localpressure;
            concentration = myconcentration;
            //fill other values from gasID lib
        }

        public IGas(IGas otherGas)
        {
            IDname = otherGas.GetIDName();
            temp = otherGas.GetTemp();
            localPressure = otherGas.GetLocalPressure();
            concentration = otherGas.GetConcentration();
            volume_m3 = otherGas.GetLocalVolume();
        }

        public IGas(string gasID, float mytemp, float myconcentration, float localpressure, float localvolume)
        {
            IDname = gasID;
            temp = mytemp;
            localPressure = localpressure;
            concentration = myconcentration;
            volume_m3 = localvolume;
            //fill other values from gasID lib
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
        //public int GetVolitility()
        //{
        //    return volitility;
        //}
        public bool GetNuclear()
        {
            return nuclear;
        }
        public int GetToxicity()
        {
            return toxicity;
        }

        //Temp will affect density
        public float GetTemp()
        {
            return temp;
        }

        //Density value will be at STP.
        public float GetDensity()
        {
            return density;
        }

        //amount per 1 m^3
        public float GetConcentration()
        {
            return concentration;
        }

        public void SetTemp(float newTemp)
        {
            temp = newTemp;
        }
        public void SetDensity(float newDensity)
        {
            density = newDensity;
        }
        public void SetLocalPressure(float pipePressure)
        {
            localPressure = pipePressure;
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
            //P[atm]M[g/mol] / R[atm*L/mol*K]T[K] = d[g/L]
            density = (localPressure * MolarMass) / (0.0821f * tempK);
            return density;
        }

        public void SetConcentration(float newConcentration)
        {
            concentration = newConcentration;
        }
    }
}
