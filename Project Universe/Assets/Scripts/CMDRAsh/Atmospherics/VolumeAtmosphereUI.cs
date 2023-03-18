using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

namespace ProjectUniverse.Environment.Volumes
{
    public class VolumeAtmosphereUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text presTxt;
        [SerializeField] private TMP_Text tempTxt;
        [SerializeField] private TMP_Text oxyTxt;
        [SerializeField] private TMP_Text toxTxt;
        [SerializeField] private Color32 GREEN;
        [SerializeField] private Color32 YELLOW;
        [SerializeField] private Color32 RED;
        [SerializeField] private VolumeAtmosphereController vac;
        private float timer = 1.0f;

        // Update is called once per frame
        void OnGUI()
        {
            timer -= Time.deltaTime;
            if(timer <= 0f)
            {
                timer = 1f;
                float pressure = (float)Math.Round(vac.Pressure, 2);
                presTxt.text = "" + pressure+" atm";
                if (pressure <= 0.5f && pressure > 0.25f)
                {
                    presTxt.color = YELLOW;
                }
                else if (pressure <= 0.25f)
                {
                    presTxt.color = RED;
                }
                else
                {
                    presTxt.color = GREEN;
                }

                float temp = (float)Math.Round(vac.Temperature, 2);
                tempTxt.text = "" + temp+ " F";
                if (temp <= 32f && temp > 0f)
                {
                    tempTxt.color = YELLOW;
                }
                else if (temp <= 0f)
                {
                    tempTxt.color = RED;
                }
                else
                {
                    tempTxt.color = GREEN;
                }

                float oxy = (float)Math.Round(vac.Oxygenation, 2);
                oxyTxt.text = "" + (oxy)+"%";
                if (oxy <= 60.0f && oxy > 30.0f)
                {
                    oxyTxt.color = YELLOW;
                }
                else if (oxy <= 30.0f)
                {
                    oxyTxt.color = RED;
                }
                else
                {
                    oxyTxt.color = GREEN;
                }

                float tox = (float)Math.Round(vac.Toxicity, 2);
                toxTxt.text = "" + tox;
                if (tox > 0.0f && tox <= 0.10f)
                {
                    toxTxt.color = YELLOW;
                }
                else if (tox > 0.10f)
                {
                    toxTxt.color = RED;
                }
                else
                {
                    toxTxt.color = GREEN;
                }
            }
        }
    } 
}
