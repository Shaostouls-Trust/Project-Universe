using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using ProjectUniverse.Environment.Volumes;

namespace ProjectUniverse.UI
{
    public class RadiationUIController : MonoBehaviour
    {
        public TMP_Text peakRateDis;
        public TMP_Text StayTimeDis;//seconds since entered rad zone, but displayed in hr,min,sec
        public Image rateBarParent;
        public Image doseMeter;
        public TMP_Text rateDis;
        public TMP_Text doseDis;
        public GameObject[] BodyHealthDis;
        /// 0 - Head
        /// 1 - Chest
        /// 2,3 - LegL, LegR
        /// 4,5 - ArmL, ArmR
        private float peakRate = 0f;
        private float stayTime;
        private float rate;
        private float dose;
        //private float doscimeterMax;
        [SerializeField] private PlayerVolumeController PVC;
        private Color32 GREEN = new Color32(0, 200, 57, 255);
        private Color32 YELLOW = new Color32(255, 200, 75, 255);
        private Color32 RED = new Color32(237, 0, 0, 255);
        private Color32 DARKRED = new Color32(55, 0, 0, 255);

        // Update is called once per frame
        void Update()
        {
            //Rate
            rate = PVC.GetRadiationExposureRate();
            if (rate <= PVC.GetMaxRadsDetectable())
            {
                rateDis.text = Math.Round(rate, 1) + " R";
            }
            else if (rate <= PVC.GetMaxRadsDetectable() + 1000) { rateDis.text = PVC.GetMaxRadsDetectable() + " R"; }
            else { rateDis.text = "ERR"; }
            //rateDis.text = Math.Round(rate,1)+" R";
            
            Image rateBar = rateBarParent.transform.GetChild(0).GetComponent<Image>();
            /*
            if (rate <= 300) { rateDis.color = GREEN; rateBar.color = GREEN; }
            else if (rate < 600) { rateDis.color = YELLOW; rateBar.color = YELLOW; }
            else if (rate < 2000) { rateDis.color = RED; rateBar.color = RED; }
            else if (rate < 5000) { rateDis.color = DARKRED; rateBar.color = DARKRED; }
            else if (rate >= 5000) { rateDis.color = DARKRED; rateBar.color = DARKRED; }//5000
            */
            float barmax = rateBarParent.rectTransform.rect.height;
            float y = 0;
            if (rate < 5000)
            {
                if (rate >= PVC.GetMaxRadsDetectable())
                {
                    y = barmax;
                }
                else 
                { 
                    y = (rate / PVC.GetMaxRadsDetectable()) * barmax; 
                }
            }
            else { y = barmax; }
            rateBar.rectTransform.transform.localScale = new Vector3(1, y, 1);
            //absorbed - meter and text
            dose = PVC.AbsorbedDose;
            doseDis.text = Math.Round(dose, 2) + " rads";
            Image doseBar = doseMeter.transform.GetChild(0).GetComponent<Image>();
            /*
            if (dose <= 300) { doseDis.color = GREEN; doseBar.color = GREEN; }
            else if (dose <= 2000) { doseDis.color = YELLOW; doseBar.color = YELLOW; }
            else if (dose > 2000) { doseDis.color = RED; doseBar.color = RED; }
            */
            float dosebar = rateBarParent.rectTransform.rect.height;
            float y2 = 0;
            if (dose < 5000f)
            {
                y2 = (dose / 5000f) * dosebar;
            }
            else { y2 = dosebar; }
            doseBar.rectTransform.transform.localScale = new Vector3(1, y2, 1);
            //absorbed - player ref
            Color32[] colors;
            if (dose <= 300)
            {
                colors = new[] { GREEN, GREEN, GREEN, GREEN, GREEN, GREEN };
            }
            else if (dose <= 600)
            {
                colors = new[] { GREEN, GREEN, YELLOW, YELLOW, YELLOW, YELLOW };
            }
            else if (dose <= 1000)
            {
                colors = new[] { YELLOW, YELLOW, YELLOW, YELLOW, YELLOW, YELLOW };
            }
            else if (dose <= 2000)
            {
                colors = new[] { YELLOW, YELLOW, RED, RED, RED, RED };
            }
            else if (dose > 2000 && dose < 5000)
            {
                colors = new[] { RED, RED, RED, RED, RED, RED };
            }
            else
            {
                colors = new[] { DARKRED, DARKRED, DARKRED, DARKRED, DARKRED, DARKRED };
            }
            SetBodyColor(colors);
            //time
            stayTime = PVC.ExposureTime;
            StayTimeDis.text = TimeSpan.FromSeconds(stayTime).ToString(@"hh\:mm\:ss");
            //highest dose rate
            if (rate > peakRate)
            {
                peakRate = rate;
            }
            peakRateDis.text = Math.Round(peakRate, 2) + "";
        }

        private void SetBodyColor(Color32[] colors)
        {
            for (int i = 0; i < BodyHealthDis.Length; i++)
            {
                BodyHealthDis[i].GetComponent<Image>().color = colors[i];
            }
        }
    }
}