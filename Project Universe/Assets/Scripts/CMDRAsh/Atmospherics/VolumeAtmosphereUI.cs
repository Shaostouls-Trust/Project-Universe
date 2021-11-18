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

        // Update is called once per frame
        void OnGUI()
        {
            float pressure = (float)Math.Round(vac.Pressure, 2);
            presTxt.text = ""+pressure;
            if (pressure <= 0.5f && pressure > 0.25f)
            {
                presTxt.color = YELLOW;
            }
            else if(pressure <= 0.25f)
            {
                presTxt.color = RED;
            }
            else
            {
                presTxt.color = GREEN;
            }

            float temp = (float)Math.Round(vac.Temperature, 2);
            tempTxt.text = ""+ temp;
            if(temp <= 32f && temp > 0f)
            {
                tempTxt.color = YELLOW;
            }
            else if(temp <= 0f)
            {
                tempTxt.color = RED;
            }
            else
            {
                tempTxt.color = GREEN;
            }

            float oxy = (float)Math.Round(vac.Oxygenation, 2);
            oxyTxt.text = ""+ oxy;
            if (oxy <= 0.6f && oxy > 0.3f)
            {
                oxyTxt.color = YELLOW;
            }
            else if (oxy <= 0.3f)
            {
                oxyTxt.color = RED;
            }
            else
            {
                oxyTxt.color = GREEN;
            }

            float tox = (float)Math.Round(vac.Toxicity, 2);
            toxTxt.text = "" + tox;
            if (oxy > 0.0f && oxy <= 0.25f)
            {
                oxyTxt.color = YELLOW;
            }
            else if (oxy > 0.25f)
            {
                oxyTxt.color = RED;
            }
            else
            {
                oxyTxt.color = GREEN;
            }
        }

    } 
}
