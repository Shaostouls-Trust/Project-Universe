using ProjectUniverse.Environment.Volumes;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ProjectUniverse.Environment.Radiation
{
    public class DoscimeterTerminalController : MonoBehaviour
    {
        [SerializeField] private float maxRads = 200f;
        private float detectedRoentgen;
        private float radsAt10;
        [SerializeField] private TMP_Text ppmwTxt;
        [SerializeField] private TMP_Text radTxt;
        [SerializeField] private TMP_Text rateAt10Txt;
        [SerializeField] private TMP_Text dangerTxt;
        [SerializeField] private VolumeAtmosphereController vac;

        public float DetectedRoentgen
        {
            set { detectedRoentgen = value; }
            get { return detectedRoentgen; }
        }

        public float RadiationAt10Meters
        {
            set { radsAt10 = value; }
        }

        private void OnGUI()
        {
            if(detectedRoentgen <= 0f)
            {
                dangerTxt.text = "Dyatlov";
            }
            else if (detectedRoentgen > 0f && detectedRoentgen <= 3.6f)
            {
                dangerTxt.text = "Not Great, Not Terrible.";
            }
            else if (detectedRoentgen > 3.6f && detectedRoentgen <= 25f)
            {
                dangerTxt.text = "Do You Taste Metal?";
            }
            else if (detectedRoentgen > 25f && detectedRoentgen <= 50f)
            {
                dangerTxt.text = "It's Another Faulty Meter.";
            }
            else if (detectedRoentgen > 50 && detectedRoentgen <= 100f)
            {
                dangerTxt.text = "You Didn't See Graphite on the Roof.";
            }
            else if (detectedRoentgen > 100f && detectedRoentgen <= 150f)
            {
                dangerTxt.text = "You Didn't, Because It's NOT THERE!";
            }
            else if (detectedRoentgen > 150f && detectedRoentgen <= 200f)
            {
                dangerTxt.text = "(0,0;)";
            }
            else
            {
                dangerTxt.text = "Dyatlov";
            }

            radTxt.text = Math.Round(detectedRoentgen, 1) + " R/hr";
            rateAt10Txt.text = Math.Round(radsAt10, 1) + " R/hr";
            ppmwTxt.text = vac.Contamination + " ppmw";
        }
    }
}