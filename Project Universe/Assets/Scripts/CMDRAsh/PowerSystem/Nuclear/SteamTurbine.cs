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
        private float steamInTemp;
        private float turbinePressure = 200f;
        private float MWeGeneration = 0f;
        [SerializeField] private float maxSafeRotationSpeed = 5200f;
        private float currentRotationSpeed = 0f;
        [SerializeField] private float steamFlowToRotation = 250f;//181.75
        [SerializeField] private float rotationToWatts = 0.2f;//.155
        private float[] pressureRange = new float[] { 100f, 215f };
        private float[] velocityRange = new float[] { 50, 120f };
        [Range(0f,1f)]
        [SerializeField] private float inflowImpingement = 0f;
        //private float steamExpansionRate = 4f;
        [SerializeField] private IGasPipe steamPipe;
        [SerializeField] private IGasPipe radiatorMainPipe;
        private float loPressureSteamCurrent;
        private float loPressureSteamPressure_bar = 15f;
        private float loPressureSteamTemp = 493f;//k
        private float rotorHealth = 1200f;
        [SerializeField] private float inputVelocity = 120;
        [SerializeField] private float inputPressure = 210.75f;
        [SerializeField] VolumeAtmosphereController vac;
        [SerializeField] private bool automaticControl = false;
        [SerializeField] private bool outletValve = true;//true is open
        [SerializeField] private bool outletValveOperable = true;//true is usable
        [SerializeField] private bool bypassValve = false;//true is open
        [SerializeField] private bool bypassValveOperable = true;//true is usable
        private bool lowPressure = false;
        private bool lowVel = false;
        [SerializeField] private IRouter outputRouter;
        public bool outputToRouter = false;
        //private float timeScaled;
        [SerializeField] private AudioSource src;

        public float LoPressureSteamRate
        { 
            get { return loPressureSteamCurrent; }
        }
        public float SteamFlowRate
        {
            get { return steamFlowRateCurrent; }
        }
        public float InflowTemp
        {
            get { return steamInTemp; }
        }
        public float InflowPressure
        {
            get { return inputPressure; }
        }
        public float TurbinePressure
        {
            get { return turbinePressure; }
        }
        public float OutflowTemp
        {
            get { return loPressureSteamTemp; }
        }
        public float OutflowPressure
        {
            get { return loPressureSteamPressure_bar; }
        }
        public float RPM
        {
            get { return currentRotationSpeed; }
        }
        public float PowerOutput
        {
            get { return MWeGeneration; }
        }
        public float RotorHealth
        {
            get { return rotorHealth; }
        }
        public bool OutletValve
        {
            get { return outletValve; }
        }
        public bool OutletValveOperable
        {
            get { return outletValveOperable; }
        }
        public bool BypassValve
        {
            get { return bypassValve; }
        }
        public bool BypassValveOperable
        {
            get { return bypassValveOperable; }
        }
        public float RPMSafeSpeed
        {
            get { return maxSafeRotationSpeed; }
        }
        public bool LowPressureInflow
        {
            get { return lowPressure; }
        }
        public bool LowVelocityInflow
        {
            get { return lowVel; }
        }
        public bool AutomaticMode
        {
            get { return automaticControl; }
        }

        // Update is called once per frame
        void Update()
        {
            //timeScaled = Time.deltaTime * 15f;

            //Empty the steam pipe
            steamFlowRateCurrent = 0f;
            float inputpressure = 0f;
            float inputvelocity = 0f;
            List<IGas> Insteam = new List<IGas>();
            steamInTemp = 0f;
            if (steamPipe != null)
            {
                inputpressure = steamPipe.GlobalPressure;
                inputvelocity = steamPipe.FlowVelocity;
                Insteam = steamPipe.ExtractGasses(-1f);//steam generator is pushing out 0 0 dt
                //Debug.Log(Insteam.Count);
                for (int i = 0; i < Insteam.Count; i++)
                {
                    //m^3[instant] to m^3/s to m^3/Hr to Kg/Hr
                    float steamFlowRate = (Insteam[i].GetConcentration() / Time.fixedDeltaTime) * 3600f * 1000f;
                    //Debug.Log(Insteam[i].GetConcentration() + " "+ steamFlowRate + " " +Time.deltaTime);
                    steamFlowRateCurrent += steamFlowRate;
                    steamInTemp = Insteam[i].GetTemp();
                }
            }

            if(steamFlowRateCurrent == 0f || inputpressure == 0f)
            {
                turbinePressure = 0f;
            }
            else
            {
                turbinePressure = 200f;
            }

            //in order for the turbine to run, input p and v must be in range
            //v will drop to raise p and allow operation.
            if(inputpressure < pressureRange[0])// || inputpressure > pressureRange[1]
            {
                lowPressure = true;
            }
            else
            {
                lowPressure = true;
            }
            if(inputvelocity < velocityRange[0])// || inputvelocity > velocityRange[1]
            {
                lowVel = true;
            }
            else
            {
                lowVel = false;
            }
            /*else
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
                        src.Play();
                        //Debug.Log("BOOM!");
                    }
                }

                
                loPressureSteamCurrent = steamFlowRateCurrent;
                // * Time.fixedDeltaTime;//* steamExpansionRate

                //loPressureSteamCurrent is Kg/hr
                if (loPressureSteamCurrent > 0f)
                {
                    loPressureSteamPressure_bar = 15f;
                    loPressureSteamTemp = 493f;//normal lo P steam temp
                                               // steam needs to lose: 193k
                                               //Kg/Hr to Kg/s to m^3/s
                    float conc = (loPressureSteamCurrent / 3600f / 1000f);
                    //Debug.Log("Conc: " + (loPressureSteamCurrent / 3600f)+ " -> "+(conc) +" -> "+(conc * Time.fixedDeltaTime));
                        //(loPressureSteamCurrent * 1.093f) / 1000f;//Kg to L to m^3
                                                                           //356f is 453.15K and 49.35atm is 50 bar
                    IGas steam = new IGas("Steam", 356f, (conc * Time.fixedDeltaTime), loPressureSteamPressure_bar, 12f);
                    steam.CalculateAtmosphericDensity();
                    //push steam into radiator
                    if (radiatorMainPipe != null && radiatorMainPipe.GlobalPressure < 14.9f)
                    {
                        radiatorMainPipe.Receive(false, inputVelocity, loPressureSteamPressure_bar, steam, steam.GetTemp());
                        //Debug.Log(radiatorMainPipe.GlobalPressure);//working.
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

            MWeGeneration = currentRotationSpeed * rotationToWatts;
            if(outputRouter != null && outputToRouter)
            {
                outputRouter.ReceivePowerFromTurbine(MWeGeneration*1000f);
            }
        }
    }
}