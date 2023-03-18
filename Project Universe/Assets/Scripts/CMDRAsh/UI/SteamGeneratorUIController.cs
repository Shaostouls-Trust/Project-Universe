using ProjectUniverse.PowerSystem.Nuclear;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectUniverse.UI
{

    public class SteamGeneratorUIController : MonoBehaviour
    {
        [SerializeField] private SteamGenerator steamGen;
        [SerializeField] private TMP_Text maxPumpRate;
        [SerializeField] private TMP_Text currentFlowRate;
        [SerializeField] private TMP_Text hottemp;
        [SerializeField] private TMP_Text cooltemp;
        [SerializeField] private TMP_Text coolantPres;
        [SerializeField] private TMP_Text coolantRes;
        [SerializeField] private TMP_Text waterRes;
        [SerializeField] private Image automaticImgOn;
        [SerializeField] private Image automaticImgOff;
        [SerializeField] private Color32 GREENON = new Color32(0,170,50,255);
        [SerializeField] private Color32 GREENOFF = new Color32(0, 50, 0, 255);
        [SerializeField] private Color32 REDON = new Color32(255, 0, 0, 255);
        [SerializeField] private Color32 REDOFF = new Color32(50, 0, 0, 255);
        [SerializeField] private Color32 YELLOWON = new Color32(255, 225, 0, 255);
        [SerializeField] private Color32 YELLOWOFF = new Color32(51, 43, 0, 255);
        [Space]
        [SerializeField] private RectTransform coolTankScale;
        [SerializeField] private TMP_Text coolantRes2;
        [Space]
        [SerializeField] private Image warningA;
        [SerializeField] private Image warningB;
        [SerializeField] private Image warningC;
        [SerializeField] private Image warningD;
        [SerializeField] private Image warningE;
        [SerializeField] private Image warningF;
        [SerializeField] private Image warningG;
        [SerializeField] private Image warningH;
        [SerializeField] private Image warningI;
        [SerializeField] private Image warningJ;
        [SerializeField] private Image warningK;
        [SerializeField] private Image warningL;
        private float timer = 0.25f;

        // Update is called once per frame
        void Update()
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                timer = 0.25f;
                maxPumpRate.text = ""+(steamGen.ThresholdPumpRate/3600f) + " Kg/s";
                currentFlowRate.text = ""+Mathf.Round(steamGen.CurrentPumpRate/3600f) + " Kg/s";
                if (steamGen.AutomaticControl)
                {
                    automaticImgOn.color = GREENON;
                    automaticImgOff.color = REDOFF;
                }
                else
                {
                    automaticImgOn.color = GREENOFF;
                    automaticImgOff.color = REDON;
                }
                hottemp.text = "" + Mathf.Round(steamGen.CoolantHotTemp)+ "K";
                cooltemp.text = "" + Mathf.Round(steamGen.CoolantCoolTemp) + "K";
                float reservoirPer = steamGen.CoolantReservoir / steamGen.CoolantReservoirMaxCap;
                coolantRes.text = Mathf.Round(reservoirPer*100f) + "%";
                coolantRes2.text = Mathf.Round(reservoirPer*100f) + "%";
                coolTankScale.localScale = new Vector3(1f, reservoirPer, 1f);
                waterRes.text = "" + Mathf.Round(steamGen.WaterReservoir) + "%";
                coolantPres.text = "" + steamGen.CoolantPressure + " bar";
            }
            ///Warnings
            ///A - SysFail: The Generator has exploded or smth and can no longer circulate coolant.
            ///B - No Coolant: The primary reservoir is empty.
            ///C - No Water: The water reservoir is empty.
            ///D - Crit Pres: The pressure in the coolant or water reservoir is too high and about to rupture the tank(s).
            ///E - Crit Temp: The coolant (or steam) in the reservoir is too hot and will soon damage the pumps.
            ///F - Auto Fail: Automatic pump control has failed and the flow must be manually regulated. In cases of core instability,
            ///this can be a path to meltdown.
            ///G - Tank Leak: The coolant reservoir is leaking.
            ///H - Coolant Low: primary reservoir is below 25%
            ///I - Water Low: water res is below 25%
            ///J - Pres High: The pressure in the coolant or water reservoir is higher than during normal operations
            ///K - Coolant Temp High: The cold side of the primary coolant is getting too hot to effectively remove heat from the core.
            ///L - Outflow Blocked: The steam valve is closed and steam is mostly likely rapidly accumulating in the generator.

            if (steamGen.SysFail)
            {
                warningA.color = REDON;
            }
            else
            {
                warningA.color = REDOFF;
            }

            if(steamGen.CoolantReservoir <= 0f)
            {
                warningB.color = REDON;
            }
            else
            {
                warningB.color = REDOFF;
            }

            if (steamGen.WaterReservoir <= 0f)
            {
                warningC.color = REDON;
            }
            else
            {
                warningC.color = REDOFF;
            }

            if(steamGen.CoolantPressure >= 215f)
            {
                warningD.color = REDON;
            }
            else
            {
                warningD.color = REDOFF;
            }

            if(steamGen.CoolantHotTemp > 1100f || steamGen.CoolantCoolTemp > 373f)
            {
                warningE.color = REDON;
            }
            else
            {
                warningE.color = REDOFF;
            }

            if (steamGen.AutoFail)
            {
                warningF.color = REDON;
            }
            else
            {
                warningF.color = REDOFF;
            }

            //Tank leak implementation? Rely on damage to tank detection?
            warningG.color = YELLOWOFF;

            if(steamGen.CoolantReservoir <= (steamGen.CoolantReservoirMaxCap * 0.25f))
            {
                warningH.color = YELLOWON;
            }
            else
            {
                warningH.color = YELLOWOFF;
            }

            if (steamGen.WaterReservoir <= (steamGen.WaterReservoirMaxCap * 0.25f))
            {
                warningI.color = YELLOWON;
            }
            else
            {
                warningI.color = YELLOWOFF;
            }

            //should also include steam pressure
            if (steamGen.CoolantPressure >= 211f)
            {
                warningJ.color = YELLOWON;
            }
            else
            {
                warningJ.color = YELLOWOFF;
            }

            if (steamGen.CoolantHotTemp > 950f || steamGen.CoolantCoolTemp > 343f)
            {
                warningK.color = YELLOWON;
            }
            else
            {
                warningK.color = YELLOWOFF;
            }

            if (!steamGen.SteamValveState)
            {
                warningL.color = YELLOWON;
            }
            else
            {
                warningL.color = YELLOWOFF;
            }
        }
    }
}