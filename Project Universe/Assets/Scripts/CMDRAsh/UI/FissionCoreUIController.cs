using ProjectUniverse.PowerSystem.Nuclear;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectUniverse.UI
{
    public class FissionCoreUIController : MonoBehaviour
    {
        public GameObject activityUIPrefab;
        [SerializeField] private NuclearCore core;
        [SerializeField] private GameObject coreRodUI;//
        [SerializeField] private TMP_Text controlRodGlobalText;//
        [SerializeField] private TMP_Text autoModeText;
        [SerializeField] private TMP_Text mintempText;
        [SerializeField] private TMP_Text avgtempText;
        [SerializeField] private TMP_Text maxtempText;
        [SerializeField] private TMP_Text minactivityText;
        [SerializeField] private TMP_Text avgactivityText;
        [SerializeField] private TMP_Text maxactivityText;
        [SerializeField] private TMP_Text coolantFlowText;
        [SerializeField] private TMP_Text mweText;
        [SerializeField] private TMP_Text vesselPresText;
        [SerializeField] private TMP_Text vesselTempText;
        [SerializeField] private TMP_Text coreStateText;
        [SerializeField] private Image critTempImg;
        [SerializeField] private Image critPresImg;
        [SerializeField] private Image radsImg;
        [SerializeField] private Image coolImg;
        [SerializeField] private Image coolImg2;
        [SerializeField] private Image critTempImg2;
        [SerializeField] private Image critPresImg2;
        [SerializeField] private Image radsImg2;
        [SerializeField] private Image vesselTempImg;
        [SerializeField] private Image vesselPresTmg;
        [SerializeField] private Image coreStateTmg;
        [SerializeField] private Image catastrophyImg;
        [SerializeField] private Image contaminationImg;
        [SerializeField] private Color32 BLUE;
        [SerializeField] private Color32 GREEN;
        [SerializeField] private Color32 RED;
        [SerializeField] private Color32 YELLOW;
        [SerializeField] private Color32 REDON;
        [SerializeField] private Color32 REDOFF;
        [SerializeField] private Color32 YELLOWON;
        [SerializeField] private Color32 YELLOWOFF;
        private float uiTimer = 0.25f;
        [SerializeField] private AudioSource[] radAlarms;
        private bool alarmOverride;
        [SerializeField] private FissionCoreUIController[] otherStations;

        // Update is called once per frame
        void Update()
        {
            uiTimer -= Time.deltaTime;
            if (uiTimer <= 0f)
            {
                uiTimer = 0.5f;
                controlRodGlobalText.text = Math.Round((core.GlobalControlRodInsertion*100f),1) + "%";
                float minTemp = 9999f;
                float minAct = 9999f;
                float avgTemp = 0f;
                float avgAct = 0f;
                float maxTemp = 0f;
                float maxAct = 0f;
                float MWeT = 0f;
                float coolant = 0f;

                //get core information
                NuclearFuelRod[,] matrix = core.NFRMatrix;
                float numCores = core.RodsReal;
                float temp;
                float act;
                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    for (int j = 0; j < matrix.GetLength(1); j++)
                    {
                        if (matrix[i, j] != null)
                        {
                            temp = matrix[i, j].RodCoreTemp;
                            act = matrix[i, j].PositiveActivity;
                            if (temp < minTemp)
                            {
                                minTemp = temp;
                            }
                            else if (temp > maxTemp)
                            {
                                maxTemp = temp;
                            }
                            if (act < minAct)
                            {
                                minAct = act;
                            }
                            else if (act > maxAct)
                            {
                                maxAct = act;
                            }
                            avgTemp += temp;
                            avgAct += act;
                            MWeT += matrix[i, j].MegaWattsThermal;
                            //coolant += matrix[i, j].CoolantMDot;//required
                        }
                    }
                }
                avgTemp /= numCores;
                avgAct /= numCores;
                mintempText.text = "" + minTemp;
                avgtempText.text = "" + avgTemp;
                maxtempText.text = "" + maxTemp;
                minactivityText.text = "" + minAct;
                avgactivityText.text = "" + avgAct;
                maxactivityText.text = "" + maxAct;
                mweText.text = "" + MWeT;
                for (int g = 0; g < core.SteamGenerators.Length; g++){
                    coolant += core.SteamGenerators[g].CurrentPumpRate;
                }
                coolantFlowText.text = "" + coolant;
                vesselPresText.text = Math.Round(core.VesselPres,1) + " / "+ Math.Round(core.MaxVesselPres,1) +" bar";
                vesselTempText.text = Math.Round(core.VesselTemp,1) + " / 600 K";

                if(maxTemp > 1300f)//100 below melting point uran
                {
                    //sound buzzer
                    //activate img
                    if (critTempImg != null)
                    {
                        critTempImg.color = REDON;
                    }
                    critTempImg2.color = REDON;
                }
                else
                {
                    if (critTempImg != null)
                    {
                        critTempImg.color = REDOFF;
                    }
                    critTempImg2.color = REDOFF;
                }
            }

            float vesTemp = core.VesselTemp;
            float vesPres = core.VesselPres;
            float vesPresMax = core.MaxVesselPres;
            //temp and pressure checks
            if(vesTemp >= 600)//600 is melting point lead. 600K is rad leak.
            {
                //sound buzzer
                //activate img
                vesselTempImg.color = REDON;
                coreStateText.text = "Unstable";
                coreStateTmg.color = REDON;
                vesselTempText.color = REDON;
            }
            else if(vesTemp < 600f && vesTemp >= 500f)
            {
                vesselTempImg.color = YELLOW;
                coreStateTmg.color = YELLOWON;
                vesselTempText.color = YELLOWON;
            }
            else if (vesTemp < 500f)
            {
                vesselTempImg.color = GREEN;
                coreStateText.text = "Stable";
                coreStateTmg.color = GREEN;
                vesselTempText.color = GREEN;
            }

            if (vesPres >= vesPresMax - 10f)//vesPresMax is burst
            {
                vesselPresTmg.color = REDON;
                if (critPresImg != null)
                {
                    critPresImg.color = REDON;
                }
                critPresImg2.color = REDON;
                coreStateText.text = "Danger";
                coreStateTmg.color = REDON;
                coreStateText.color = REDON;
                //alarm
            }
            else if (vesPres < (vesPresMax - 10f) && vesPres >= 240)//210 is  pump pressure
            {
                vesselPresTmg.color = YELLOW;
                if (critPresImg)
                {
                    critPresImg.color = REDOFF;
                }
                critPresImg2.color = REDOFF;
                coreStateText.text = "Caution";
                coreStateTmg.color = YELLOWON;
                vesselPresText.color = YELLOWON;
            }
            else
            {
                vesselPresTmg.color = GREEN;
                vesselPresText.color = GREEN;
                if (critPresImg)
                {
                    critPresImg.color = REDOFF;
                }
                critPresImg2.color = REDOFF;
                if (vesTemp < 500f)
                {
                    coreStateText.text = "Stable";
                    coreStateTmg.color = GREEN;
                    coreStateText.color = GREEN;
                }
            }

            if(core.DetectedRads > 0f)
            {
                if (radsImg)
                {
                    radsImg.color = YELLOWON;
                }
                radsImg2.color = YELLOWON;
                //play alarms
                for(int i = 0; i < radAlarms.Length; i++)
                {
                    if (!radAlarms[i].isPlaying && !alarmOverride)
                    {
                        radAlarms[i].Play();
                    }
                }
            }
            else
            {
                if (radsImg)
                {
                    radsImg.color = YELLOWOFF;
                }
                radsImg2.color = YELLOWOFF;
                for (int i = 0; i < radAlarms.Length; i++)
                {
                    if (radAlarms[i].isPlaying)
                    {
                        radAlarms[i].Stop();
                    }
                }
            }

            if (core.HasCoreExploded())
            {
                catastrophyImg.color = REDON;
            }
            else
            {
                catastrophyImg.color = REDOFF;
            }
            if (core.IsCoreContaminated())
            {
                contaminationImg.color = YELLOWON;
            }
            else
            {
                contaminationImg.color = YELLOWOFF;
            }

            //if coolant level in core is low
            for(int i = 0; i < core.SteamGenerators.Length; i++)
            {
                if (core.SteamGenerators[i].CoolantReservoir < (core.SteamGenerators[i].CoolantReservoirMaxCap) * 0.25f)
                {
                    if (coolImg)
                    {
                        coolImg.color = REDON;
                    }
                    coolImg2.color = REDON;
                    break;
                }
                else
                {
                    if (coolImg)
                    {
                        coolImg.color = REDOFF;
                    }
                    coolImg2.color = REDOFF;
                }
            }
        }

        public void ExternalInteractFunc(int i)
        {
            if(i == 1)
            {
                //rad alarm state
                for (int b = 0; b < radAlarms.Length; b++)
                {
                    if (radAlarms[b].isPlaying)
                    {
                        alarmOverride = true;
                        radAlarms[b].Stop();
                        for(int c = 0; c < otherStations.Length; c++)
                        {
                            otherStations[c].alarmOverride = true;
                        }
                    }
                    else
                    {
                        alarmOverride = false;
                        for (int c = 0; c < otherStations.Length; c++)
                        {
                            otherStations[c].alarmOverride = false;
                        }
                    }
                }
                
            }
        }
    }
}