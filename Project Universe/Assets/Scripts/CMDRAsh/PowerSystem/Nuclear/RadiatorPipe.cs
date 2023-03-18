using ProjectUniverse.Environment.Gas;
using ProjectUniverse.Environment.Volumes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUniverse.PowerSystem.Nuclear
{
    //one radiator main reqs abt 114 radiators at 4960000
    public class RadiatorPipe : MonoBehaviour
    {
        //[SerializeField] private RadiatorMain radiatorMain;//FROM PIPE
        [SerializeField] private IGasPipe radiatorPipe;
        [SerializeField] private VolumeAtmosphereController vac;
        private float steamTemp = 300f;
        [SerializeField] private float steamPressure = 0f;
        private float steamAmount = 0f;
        public float deltaHeatSteam =0f;
        /// <summary>
        /// 0.5 is to maintain room temp
        /// </summary>
        [Range(0,1)]
        [SerializeField] private float valveAmount = 0.5f;
        [SerializeField] private float qTransferRateMax = 4960000f;
        public float qtransRate =0f;

        // Update is called once per frame
        void Update()
        {
            steamTemp = ((radiatorPipe.GetGasTempF(false) - 32f)/1.8f)+273.15f;//f to k
            steamPressure = radiatorPipe.GlobalPressure;
            steamAmount = (radiatorPipe.GetConcentration() * 1000f) / 1.093f;//m^3 to L to Kg
            float steamQ = steamAmount * 2010f * (steamTemp-300f);            
            float heatLossMax = qTransferRateMax * valveAmount * Time.fixedDeltaTime;
            qtransRate = steamQ;
            if (steamQ > 0f)
            {
                if (steamQ > heatLossMax)
                {
                    steamQ = heatLossMax;
                }
                if (steamPressure >= 1f)
                {
                    if (steamTemp > ((vac.Temperature - 32f) / 1.8f) + 273.15f)
                    {
                        vac.AddRoomHeat(steamQ);
                        deltaHeatSteam = (steamQ / (radiatorPipe.GetKgSteam() * 2010f));
                        radiatorPipe.RemoveHeat(steamQ);
                    }
                }
                //if (steamPressure < 1f)
                //{
                    //Debug.Log(steamPressure);
                //}
            }
            
        }
    }
}