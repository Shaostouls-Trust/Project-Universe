using ProjectUniverse.PowerSystem.Nuclear;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectUniverse.UI
{
    public class SteamTurbineUIController : MonoBehaviour
    {
        [SerializeField] private SteamTurbine steamTurb;
        [SerializeField] private TMP_Text steamFlowRate;
        [SerializeField] private TMP_Text steamTemp;
        [SerializeField] private TMP_Text steamPres;
        [SerializeField] private TMP_Text outTemp;
        [SerializeField] private TMP_Text outPres;
        [SerializeField] private TMP_Text turbRPM;
        [SerializeField] private TMP_Text mwe;
        [SerializeField] private Image automaticImgOn;
        [SerializeField] private Image automaticImgOff;
        [Space]
        [SerializeField] private Color32 GREENON = new Color32(0, 170, 50, 255);
        [SerializeField] private Color32 GREENOFF = new Color32(0, 50, 0, 255);
        [SerializeField] private Color32 REDON = new Color32(255, 0, 0, 255);
        [SerializeField] private Color32 REDOFF = new Color32(50, 0, 0, 255);
        [SerializeField] private Color32 YELLOWON = new Color32(255, 225, 0, 255);
        [SerializeField] private Color32 YELLOWOFF = new Color32(51, 43, 0, 255);
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
                steamFlowRate.text = Math.Round((steamTurb.SteamFlowRate / 3600f),2) + " Kg/s";
                steamTemp.text = steamTurb.InflowTemp + "K";
                steamPres.text = steamTurb.TurbinePressure + " bar";//steamTurb.InflowPressure
                outTemp.text = steamTurb.OutflowTemp + "K";
                outPres.text = steamTurb.OutflowPressure + " bar";
                turbRPM.text = Math.Round(steamTurb.RPM,2) + "";
                mwe.text = Math.Round(steamTurb.PowerOutput,2) + " MWe";
            }

            ///Warnings
            ///A - SysFail: The turbine has exploded or smth and can no longer circulate coolant.
            ///B - Integrity Warning: The turbine is close to failure and should be shut down at once.
            ///C - Outlet Blocked: Low pressure steam cannot exit the turbine.
            ///D - Over Pres: The pressure in the turbine is too high and about to blow the system
            ///E - Steam Leak: Steam is leaking from the turbine, most likely high-pressure, high-temp steam.
            ///F - Valve Fail: The release valve at the turbine exit is non-operable.
            ///G - Over Speed: Tubine RPMs are too high and may be damaging the shaft.
            ///H - Inlet Bypass Open: Steam is no longer flowing through the turbine.
            ///I - Outlet Valve Closed: Outlet release valve is closed and steam may be accumulating.
            ///J - Inlet Pressure Low: The pressure of the incoming steam is too low for normal operations.
            ///K - Inflow Velocity Low: The velocity of the incoming steam is too low for normal operations.
            ///L - Manual Mode: The inlet regulator is under manual control. Check pressure settings.
            ///

            if (steamTurb.RotorHealth <= 0f)
            {
                warningA.color = REDON;
            }
            else
            {
                warningA.color = REDOFF;
            }

            if(steamTurb.RotorHealth <= 200f)
            {
                warningB.color = REDON;
            }
            else
            {
                warningB.color = REDOFF;
            }

            if (!steamTurb.OutletValve && !steamTurb.OutletValveOperable)
            {
                warningC.color = REDON;
            }
            else
            {
                warningC.color = REDOFF;
            }

            if(steamTurb.TurbinePressure > 210f)
            {
                warningD.color = REDON;
            }
            else
            {
                warningD.color = REDOFF;
            }

            //steam leak by damage
            warningE.color = REDOFF;

            if (!steamTurb.OutletValveOperable || !steamTurb.BypassValveOperable)
            {
                warningF.color = REDON;
            }
            else
            {
                warningF.color = REDOFF;
            }

            if (steamTurb.RPM > steamTurb.RPMSafeSpeed)
            {
                warningG.color = YELLOWON;
            }
            else
            {
                warningG.color = YELLOWOFF;
            }

            if (steamTurb.BypassValve)//open
            {
                warningH.color = YELLOWON;
            }
            else
            {
                warningH.color = YELLOWOFF;
            }

            if (!steamTurb.OutletValve)//closed
            {
                warningI.color = YELLOWON;
            }
            else
            {
                warningI.color = YELLOWOFF;
            }

            if (steamTurb.LowPressureInflow)
            {
                warningJ.color = YELLOWON;
            }
            else
            {
                warningJ.color = YELLOWOFF;
            }

            if (steamTurb.LowVelocityInflow)
            {
                warningK.color = YELLOWON;
            }
            else
            {
                warningK.color = YELLOWOFF;
            }

            if (!steamTurb.AutomaticMode)
            {
                warningL.color = YELLOWON;
                automaticImgOn.color = GREENOFF;
                automaticImgOff.color = REDON;
            }
            else
            {
                warningL.color = YELLOWOFF;
                automaticImgOn.color = GREENON;
                automaticImgOff.color = REDOFF;
            }
        }
    }
}