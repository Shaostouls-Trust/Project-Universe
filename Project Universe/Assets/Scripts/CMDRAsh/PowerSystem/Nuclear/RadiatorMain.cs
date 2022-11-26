using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is accessed by turbines, condensors, heat engines, and radiators to input, cool, and consume, 
/// low pressure steam.
/// Turbines feed steam into pipes that lead to the radiator main block.
/// Condensors are fed from the radiator main.
/// Heat Engines use remaining heat before the condensor.
/// Radiators cool steam from pipes before radiator main.
/// </summary>
namespace ProjectUniverse.PowerSystem.Nuclear
{
    public class RadiatorMain : MonoBehaviour
    {
        // Start is called before the first frame update
        [SerializeField] private float steamTemp;
        [SerializeField] private float steamPressure;
        [SerializeField] private float steamRate;
        [SerializeField] private float steamAmount;
        [SerializeField] private SteamCondenser condenser;

        public float SteamTemp { get => steamTemp; set => steamTemp = value; }
        public float SteamPressure { get => steamPressure; set => steamPressure = value; }
        public float SteamRate { get => steamRate; set => steamRate = value; }
        public float SteamAmount { get => steamAmount; set => steamAmount = value; }

        /*public void RemoveHeatAmount(float qHeat)
        {
            //float totQ = SteamAmount * 2010f * SteamTemp;
            SteamTemp -= (qHeat / (SteamAmount * 2010f));
        }

        private void Update()
        {
            //set steam into condenser
            condenser.SteamAmount += steamRate * Time.deltaTime;
            steamAmount -= steamRate * Time.deltaTime;
            if(steamAmount <= 0f)
            {
                //steam is gone
                steamAmount = 0f;
            }
        }*/

    }
}