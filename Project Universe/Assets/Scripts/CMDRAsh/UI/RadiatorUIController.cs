using ProjectUniverse.PowerSystem.Nuclear;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectUniverse.UI
{
    public class RadiatorUIController : MonoBehaviour
    {
        [SerializeField] private RadiatorMain radiatorHead;
        [SerializeField] private RadiatorMain radiatorTail;
        [SerializeField] private TMP_Text steamFlowRate1;
        [SerializeField] private TMP_Text steamTemp1;
        [SerializeField] private TMP_Text outTemp1;
        [SerializeField] private TMP_Text conRad1;
        [SerializeField] private Image automaticImgOn1;
        [SerializeField] private Image automaticImgOff1;
        [Space]
        [SerializeField] private TMP_Text steamFlowRate2;
        [SerializeField] private TMP_Text steamTemp2;
        [SerializeField] private TMP_Text outTemp2;
        [SerializeField] private TMP_Text conRad2;
        [SerializeField] private Image automaticImgOn2;
        [SerializeField] private Image automaticImgOff2;
        [Space]
        [SerializeField] private TMP_Text steamFlowRate3;
        [SerializeField] private TMP_Text steamTemp3;
        [SerializeField] private TMP_Text outTemp3;
        [SerializeField] private TMP_Text conRad3;
        [SerializeField] private Image automaticImgOn3;
        [SerializeField] private Image automaticImgOff3;
        [Space]
        [SerializeField] private TMP_Text steamFlowRate4;
        [SerializeField] private TMP_Text steamTemp4;
        [SerializeField] private TMP_Text outTemp4;
        [SerializeField] private TMP_Text conRad4;
        [SerializeField] private Image automaticImgOn4;
        [SerializeField] private Image automaticImgOff4;
        [Space]
        [SerializeField] private Color32 GREENON = new Color32(0, 170, 50, 255);
        [SerializeField] private Color32 GREENOFF = new Color32(0, 50, 0, 255);
        [SerializeField] private Color32 REDON = new Color32(255, 0, 0, 255);
        [SerializeField] private Color32 REDOFF = new Color32(50, 0, 0, 255);
        private float timer = 0.25f;

        // Update is called once per frame
        void Update()
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                timer = 0.25f;

                if(!radiatorHead.Valve1 || !radiatorTail.Valve1)
                {
                    automaticImgOff1.color = REDON;
                    automaticImgOn1.color = GREENOFF;
                }
                else
                {
                    automaticImgOff1.color = REDOFF;
                    automaticImgOn1.color = GREENON;
                }

                steamFlowRate1.text = Math.Round(radiatorHead.SteamFlow1 / 3600f,2) + " Kg/s";
                steamFlowRate2.text = Math.Round(radiatorHead.SteamFlow2 / 3600f,2) + " Kg/s";
                steamFlowRate3.text = Math.Round(radiatorHead.SteamFlow3 / 3600f,2) + " Kg/s";
                steamFlowRate4.text = Math.Round(radiatorHead.SteamFlow4 / 3600f,2) + " Kg/s";

                steamTemp1.text = Math.Round(radiatorHead.InflowTemp1,1) + "";
                steamTemp2.text = Math.Round(radiatorHead.InflowTemp2,1) + "";
                steamTemp3.text = Math.Round(radiatorHead.InflowTemp3,1) + "";
                steamTemp4.text = Math.Round(radiatorHead.InflowTemp4,1) + "";

                outTemp1.text = Math.Round(radiatorTail.OutflowTemp1,1) + "";
                outTemp2.text = Math.Round(radiatorTail.OutflowTemp2,1) + "";
                outTemp3.text = Math.Round(radiatorTail.OutflowTemp3,1) + "";
                outTemp4.text = Math.Round(radiatorTail.OutflowTemp4,1) + "";

                conRad1.text = radiatorHead.ConnectedRadiators1 + "";
                conRad2.text = radiatorHead.ConnectedRadiators2 + "";
                conRad3.text = radiatorHead.ConnectedRadiators3 + "";
                conRad4.text = radiatorHead.ConnectedRadiators4 + "";
            }
        }
    }
}