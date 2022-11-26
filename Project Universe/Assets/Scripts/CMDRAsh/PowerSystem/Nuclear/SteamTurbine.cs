using ProjectUniverse.Environment.Gas;
using ProjectUniverse.Environment.Volumes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


namespace ProjectUniverse.PowerSystem.Nuclear
{
    public class SteamTurbine : MonoBehaviour
    {
        [SerializeField] private SteamGenerator steamGen;
        [SerializeField] private float waterFlowRateMax = 2870000f;//kg/hr
        private float steamFlowRateCurrent = 0f;//Kg/hr
        private float turbinePressure = 200f;
        //private float coolantFlowRateCurrent = 0f;//kg/hr
        //[SerializeField] private float waterRateThreshold = 2870000f;
        private float MWeGenerationPerHour = 0f;
        [SerializeField] private float maxSafeRotationSpeed = 4000f;
        private float currentRotationSpeed = 0f;
        [SerializeField] private float steamFlowToRotation = 181.75f;
        [SerializeField] private float rotationToWatts = 0.2f;
        private float[] pressureRange = new float[] { 100f, 215f };
        private float[] velocityRange = new float[] { 50, 120f };
        [Range(0f,1f)]
        [SerializeField] private float inflowImpingement = 0f;
        //[SerializeField] private float rotationToWattEfficiency = 0f;
        private float steamExpansionRate = 4f;
        //[SerializeField] private RadiatorMain radiatorMainPipe;
        [SerializeField] private IGasPipe steamPipe;
        [SerializeField] private IGasPipe radiatorMainPipe;
        private float loPressureSteamCurrent;
        private float loPressureSteamPressure_bar = 50f;
        private float loPressureSteamTemp = 0f;
        private float rotorHealth = 1200f;
        [SerializeField] private float inputVelocity = 120;
        [SerializeField] private float inputPressure = 210.75f;
        public TMP_Text waterFlow;
        public TMP_Text MWeHr;
        public TMP_Text rpm;
        public TMP_Text inflow;
        private float uiTime = 0.5f;
        [SerializeField] VolumeAtmosphereController vac;
        [SerializeField] private bool automaticControl = false;
        //private float timeScaled;

        public float LoPressureSteamRate
        { 
            get { return loPressureSteamCurrent; }
        }

        // Update is called once per frame
        void Update()
        {
            //timeScaled = Time.deltaTime * 15f;
            uiTime -= Time.deltaTime;

            //Empty the steam pipe
            steamFlowRateCurrent = 0f;
            float inputpressure = steamPipe.GlobalPressure;
            float inputvelocity = steamPipe.FlowVelocity;
            List<IGas> Insteam = steamPipe.ExtractGasses(-1f);//steam generator is pushing out 0 0 dt
            //Debug.Log(Insteam.Count);
            for (int i = 0; i < Insteam.Count; i++)
            {
                //m^3[instant] to m^3/s to m^3/Hr to Kg/Hr
                float steamFlowRate = (Insteam[i].GetConcentration() / Time.fixedDeltaTime) * 3600f * 1000f;
                //Debug.Log(Insteam[i].GetConcentration() + " "+ steamFlowRate + " " +Time.deltaTime);
                steamFlowRateCurrent += steamFlowRate;
            }

            //in order for the turbine to run, input p and v must be in range
            //v will drop to raise p and allow operation.
            /*if(inputpressure < pressureRange[0] && inputpressure > pressureRange[1])
            {

            }
            else
            {
                //10 bar for 7 m/s (out of pipe)
                if (automaticControl)
                {
                    float minusVel = (pressureRange[0] - inputpressure) * (7f/10f);
                    if(inputvelocity - minusVel >= velocityRange[0])
                    {
                        inputpressure += minusVel * (10f / 7f);
                        inputvelocity -= minusVel;
                    }
                    else
                    {
                        inputpressure += (velocityRange[0] - inputvelocity) * (10f / 7f);
                        inputvelocity = velocityRange[0];
                    }
                }
                
            }
            //if the turbine can now run
            if (inputpressure >= pressureRange[0] && inputpressure <= pressureRange[1])
            {

            }*/

            if (rotorHealth > 0f)
            {
                turbinePressure = 200f;//200bar normal
                                       //steamFlowRateCurrent = steamGen.SteamFlowRate; //Bypass the steam pipe
                currentRotationSpeed = (steamFlowRateCurrent / 100000f * steamFlowToRotation);
                if (currentRotationSpeed > maxSafeRotationSpeed)
                {
                    rotorHealth -= (currentRotationSpeed - maxSafeRotationSpeed) * 0.25f * Time.deltaTime;
                    if (rotorHealth <= 0f)
                    {
                        rotorHealth = 0f;
                        //explosion and sound stuff
                        Debug.Log("BOOM!");
                    }
                }

                loPressureSteamCurrent = steamFlowRateCurrent * steamExpansionRate * Time.fixedDeltaTime;//?
                                                                                                    //create steam Igas to send through radiators

                if (loPressureSteamCurrent > 0f)
                {
                    loPressureSteamPressure_bar = inputPressure;// turbinePressure;//200bar normal / steamExpansionRate
                    loPressureSteamTemp = 453.15f;//normal lo P steam temp
                                                  // steam needs to lose: 153.15k
                    float conc = (loPressureSteamCurrent * 1.093f) / 1000f;//Kg to L to m^3
                                                                           //356f is 453.15K and 49.35atm is 50 bar
                    IGas steam = new IGas("Steam", 356f, conc * Time.fixedDeltaTime, loPressureSteamPressure_bar, 12f);
                    steam.CalculateAtmosphericDensity();
                    //push steam into radiator
                    if (radiatorMainPipe != null && radiatorMainPipe.GlobalPressure < 49.35f)
                    {
                        radiatorMainPipe.Receive(false, inputVelocity, loPressureSteamPressure_bar, steam, steam.GetTemp());
                    }
                }
                else
                {
                    loPressureSteamPressure_bar = 0f;
                    loPressureSteamTemp = 0f;
                }
            }
            else
            {
                ///
                /// Steam is vented into room atmosphere. 
                /// No rotation. No low-p steam. Turbine pressure is atmospheric.
                /// 
                if (vac != null)
                {
                    for (int g = 0; g < Insteam.Count; g++)
                    {
                        vac.AddRoomGas(Insteam[g]);
                    }
                    turbinePressure = vac.Pressure;
                }
                else
                {
                    turbinePressure = 0f;
                }
                loPressureSteamCurrent = 0f;
                currentRotationSpeed = 0f;
            }

            MWeGenerationPerHour = currentRotationSpeed * rotationToWatts;
            if (uiTime <= 0f)
            {
                waterFlow.text = steamFlowRateCurrent.ToString("0.0") + " kg/hr";
                MWeHr.text = MWeGenerationPerHour.ToString("0.0") + " MWe/hr";
                inflow.text = steamGen.CurrentPumpRate.ToString("0.0") + " kg/hr";//coolantFlowRateCurrent
                rpm.text = currentRotationSpeed.ToString("0.0");
                uiTime = 0.5f;
            }
        }
    }
}