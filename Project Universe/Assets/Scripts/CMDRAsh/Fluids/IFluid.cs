using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 Flamability 0-10 : how easily it catches fire
 Combustability 0-10 : How easily it 'splodes when exposed to sparks or fire
 Reactants[] : What it can combust with (proportion fluid1 | proportion fluid2 | proportion fluid3 ||| pn)
 Volitility 0-10 : how quickly it turns to gas
 Nuclear: bool
 Toxicity 0-10 : Level of protection needed and dmg amount of exposure
 Irradiation: How irradiated the fluid is, a function of exposure over time
 Temp:
 Density: 
 */

namespace ProjectUniverse.Environment.Fluid { 

    public class IFluid //: MonoBehaviour
    {
        private string IDname;
        private int flamability;
        private int combustability;
        private string[] reactants;
        private int volitility;
        private bool nuclear;
        private int toxicity;
        private float irradiation;

        private float MolarMass = 18.02f;//water MM
        private float density;//in g/L IE water is 1g/cm3 so 1000g/L
        private float temp;
        private float concentration;//amount of the fluid in the local volume
        //private float volume_m3;//amount of fluid in m^3
        private float localPressure;//pressure of the fluid in it's local volume

        override
        public string ToString()
        {
            string compile = "" + IDname + " at " + temp + "F, " + density + "g/L, " + concentration + "m3 at " + localPressure + "atm";// volume_m3 + "m3 at " 
            return compile;
        }

        public IFluid(string gasID, float mytemp, float myconcentration)//, float localpressure)
        {
            IDname = gasID;
            temp = mytemp;
            //localPressure = localpressure;
            concentration = myconcentration;
            //fill other values from gasID lib
        }

        public IFluid(string gasID, float mytemp, float myconcentration, float localpressure)//, float localvolume)
        {
            IDname = gasID;
            temp = mytemp;
            localPressure = localpressure;
            concentration = myconcentration;
            //volume_m3 = localvolume;
            //fill other values from gasID lib
        }

        public IFluid(IFluid otherFluid)
        {
            IDname = otherFluid.GetIDName();
            temp = otherFluid.GetTemp();
            localPressure = otherFluid.GetLocalPressure();
            concentration = otherFluid.GetConcentration();
            //volume_m3 = otherFluid.GetLocalVolume();
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
        public int GetVolitility()
        {
            return volitility;
        }
        public bool GetNuclear()
        {
            return nuclear;
        }
        public int GetToxicity()
        {
            return toxicity;
        }
        public float GetTemp()
        {
            return temp;
        }
        public float GetDensity()
        {
            return density;
        }
        public float GetConcentration()
        {
            return concentration;
        }
        public float GetLocalPressure()
        {
            return localPressure;
        }
        //public float GetLocalVolume()
        //{
        //    return volume_m3;
        //}
        public float GetFilledVolume()
        {
            return 1000f * concentration;//1000L in 1 m^3 water
        }
        public float GetMolarMass()
        {
            return MolarMass;
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
        //public void SetLocalVolume(float localVolume)
        //{
        //    volume_m3 = localVolume;
        //}
        public void SetConcentration(float newConcentration)
        {
            concentration = newConcentration;
        }
        public void AddConcentration(float additional)
        {
            concentration += additional;
        }
    }
}