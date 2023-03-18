using ProjectUniverse.Environment.Fluid;
using ProjectUniverse.Environment.Gas;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUniverse.PowerSystem.Nuclear
{
    public class SteamGenerator : MonoBehaviour
    {
        [SerializeField] private NuclearCore core;
        [SerializeField] private float maxPumpRate = 2700000f;//max output
        [Tooltip("If pump should match required coolant flow, or if flow should be manually controlled.")]
        [SerializeField] private bool automaticControl = true;
        [SerializeField] private bool autofail = false;
        [SerializeField] private bool sysfail = false;
        [SerializeField] private bool steamValveState = true;//true is open
        private float currentPumpRate;//current output
        private float requiredPumpRate;//required amount by reactor;
        [SerializeField] private float thresholdPumpRate = 2700000f;//reported output and for limited flow scenarios
        private float coolantHotTemp = 300f;//temp from reactor (373.15 for boil)
        private float coolantLossHot = 300f;
        private float coolantCoolTemp = 300f;//temp to reactor
        [SerializeField] private SteamTurbine turbine;
        [SerializeField] private IFluidPipe waterIn;
        private List<IFluid> storedWater = new List<IFluid>();
        public float waterStored_monitor = 0f;//m^3
        private float waterStoredMax = 10f;//m^3
        //public float steamStored_monitor = 0f;//Kg
        [SerializeField] private IGasPipe steamOutPipe;
        private IGas steamOut;
        [SerializeField] private float primaryCoolantReservoir = 100f;//m3
        private float primaryCoolantMaxCap = 100f;
        [SerializeField] private IFluidPipe coolantOut;//Kg/Hr
        [SerializeField] private IFluidPipe coolantIn;//Kg/Hr
        private IFluid coolantInReservoir;
        [SerializeField] private float outputVelocity = 120f;
        [SerializeField] private float outputPressure = 210.75f;//p should be determined by massflow and vel
        [SerializeField] private float coolantOutputVelocity = 32f;
        //old turbine vars
        private float coolantFlowRateCurrent = 0f;//Kg/hr
        private float steamFlowRateCurrent = 0f;//Kg/hr
        private float waterFlowRateMax = 2870000f;//kg/hr

        // Start is called before the first frame update
        void Start()
        {
            thresholdPumpRate = maxPumpRate;
            steamOut = new IGas("Steam", 851f, 0f, 200f, 72f);
            coolantInReservoir = new IFluid("Coolant", 80.33f, primaryCoolantReservoir, 200f, 100f);
        }

        public bool AutomaticControl { get { return automaticControl; } }

        public float MaxPumpRate
        {
            get { return maxPumpRate; }
        }

        public float ThresholdPumpRate
        {
            get { return thresholdPumpRate; }//Kg/hr
        }

        //Kg/hr
        public float CurrentPumpRate
        {
            get { return currentPumpRate; }
            set { currentPumpRate = value; }
        }
        
        public float RequiredPumpRate//Kg/hr
        {
            get { return requiredPumpRate; }
            set { requiredPumpRate = value; }
        }

        public float CoolantHotTemp
        {
            get { return coolantHotTemp; }
            set { coolantHotTemp = value; }
        }

        public float CoolantCoolTemp
        {
            get { return coolantCoolTemp; }
            set { coolantCoolTemp = value; }
        }
        public float CoolantReservoir
        {
            get { return primaryCoolantReservoir; }
        }
        public float WaterReservoir
        {
            get { return waterStored_monitor; }
        }
        public float WaterReservoirMaxCap
        {
            get { return waterStoredMax; }
        }
        public float CoolantReservoirMaxCap
        {
            get { return primaryCoolantMaxCap; }
        }
        public void AdjustCoolantThreshold(float value)
        {
            thresholdPumpRate += value;
        }
        public bool SteamValveState
        {
            get { return steamValveState; }
        }
        public float SteamFlowRate
        {
            get { return steamFlowRateCurrent; }
        }
        public float CoolantPressure//pressure the coolant is pumped out at
        {
            get { return outputPressure; }
        }
        public bool AutoFail
        {
            get { return autofail; }
        }
        public bool SysFail
        {
            get { return sysfail; }
        }
        private void Update()
        {
            //collect water from pipe
            if ((waterStored_monitor < waterStoredMax) && waterIn != null)
            {
                //grab the incoming water (m^3/sec)
                // but don't overfill!
                float rate = (waterStoredMax - waterStored_monitor);
                List<IFluid> IncommingWater = waterIn.ExtractFluid(rate);
                //float waterInFlowRateCurrent = 0f;
                for (int i = 0; i < IncommingWater.Count; i++)
                {
                    waterStored_monitor += IncommingWater[i].GetConcentration();
                }
                //combine dupes
                for (int i = IncommingWater.Count - 1; i >= 0; i--)
                {
                    for (int j = 0; j < storedWater.Count; j++)
                    {
                        if (IncommingWater[i].GetIDName() == storedWater[j].GetIDName())
                        {
                            float newConc = storedWater[j].GetConcentration() + IncommingWater[i].GetConcentration();
                            storedWater[j].SetConcentration(newConc);
                            IncommingWater.RemoveAt(i);
                        }
                    }
                }
                storedWater.AddRange(IncommingWater);
            }

            if (coolantIn != null)
            {
                //get hot coolant from in pipe
                float coolantHotFlowRateCurrent = 0f;
                List<IFluid> IncommingCoolant = coolantIn.ExtractFluid(-1f);
                for (int i = 0; i < IncommingCoolant.Count; i++)
                {
                    coolantHotFlowRateCurrent += IncommingCoolant[i].GetConcentration();
                    primaryCoolantReservoir += IncommingCoolant[i].GetConcentration();
                    //Debug.Log("Gen In (m3): " + IncommingCoolant[i].GetConcentration());
                }
            }

            //Hot/Cold Coolant cycle
            if (!float.IsNaN(CurrentPumpRate))
            {
                coolantFlowRateCurrent = CurrentPumpRate;
            }
            else
            {
                coolantFlowRateCurrent = 0f;
            }
            
            // calculate the heat energy present in the coolant
            float hotQ;
            if (!float.IsNaN(CoolantHotTemp))
            {
                coolantLossHot = CoolantHotTemp;
                //in some cases (on initialization) cool will be hotter than hot so Q will be -
                hotQ = coolantFlowRateCurrent * 4184f * (CoolantHotTemp-CoolantCoolTemp);//-300f or coolantCool
            }
            else
            {
                coolantLossHot = 300f;
                hotQ = 0f;
            }
            if(hotQ < 0f)
            {
                hotQ = 0f;
            }
            // calculate how much water that energy will bring to 455C (728.15K)
            // this is our required water amount / max steam output (Kg/Hr)
            // Qh2o * latent heat h2o + Qsteam * latent heat steam
            float waterQCoeff = (4164f * 73.15f) + (2.26E6f) + (2010f * 355f);
            steamFlowRateCurrent = hotQ / waterQCoeff;
            float absoluteRequiredWater_Steam = steamFlowRateCurrent;
            if (steamFlowRateCurrent > waterFlowRateMax)
            {
                steamFlowRateCurrent = waterFlowRateMax;
            }
            if (coolantFlowRateCurrent > 0f)
            {
                //remove heat from hot coolant
                //get the energy in the steam
                //float qsteam = steamFlowRateCurrent * waterQCoeff;//=hotQ
                //how much energy was removed (all, unless -300 or cool is applied)
                float dt = (steamFlowRateCurrent * waterQCoeff) / (coolantFlowRateCurrent * 4184f);
                //remove heat energy until hot is at 300K.
                coolantLossHot -= dt - 300f;
                //Debug.Log(coolantLossHot);
            }

            float instFlowRateCurrent = (steamFlowRateCurrent / 1000f / 3600f)*Time.fixedDeltaTime;//Kg/hr to m^3/hr to m^3(instantaneous)
            //Debug.Log(instFlowRateCurrent);
            if (instFlowRateCurrent > waterStored_monitor)
            {
                steamFlowRateCurrent = (waterStored_monitor / Time.deltaTime) * 3600f * 1000f;//m^3[instant] to m^3/s to m^3/hr to Kg/hr
                instFlowRateCurrent = (steamFlowRateCurrent / 1000f / 3600f) * Time.fixedDeltaTime;//Kg/hr to m^3/hr to m^3(instantaneous)
                //water should be exhausted at this point
                for (int i = 0; i < storedWater.Count; i++)
                {
                    storedWater[i].SetConcentration(0f);
                }
                waterStored_monitor = 0f;
            }
            else
            {
                //water/steam outflow
                waterStored_monitor -= instFlowRateCurrent;
                for (int i = 0; i < storedWater.Count; i++)
                {
                    storedWater[i].AddConcentration(-instFlowRateCurrent);
                }
            }

            //Ensure water is not negative
            if (waterStored_monitor < 0f)
            {
                for (int i = 0; i < storedWater.Count; i++)
                {
                    storedWater[i].SetConcentration(0f);
                }
                waterStored_monitor = 0f;
            }
            
            //add steamFlowRateCurrent to steam_monitor and steam gas [no steam stor, send inst rate]
            //steamStored_monitor += instFlowRateCurrent;
            //steamOut.AddConcentration(instFlowRateCurrent);
            steamOut.SetConcentration(instFlowRateCurrent);
            // /\ get the pressure of this gas for steam tank pressure?

            //else
            //change to include coolant flow, not just water flow
            //if required is greater than available
            if (absoluteRequiredWater_Steam > steamFlowRateCurrent || coolantFlowRateCurrent < 1000f)
            {
                
                if (!float.IsNaN(CoolantCoolTemp))
                {
                    float gap = coolantHotTemp - CoolantCoolTemp;//300f coolantHotTemp
                    CoolantCoolTemp += (gap * Time.deltaTime);// * 0.1f
                    if (CoolantCoolTemp > CoolantHotTemp)
                    {
                        CoolantCoolTemp = CoolantHotTemp;
                    }
                }
            }
            //cool down coolant to 300K
            else
            {
                if (!float.IsNaN(CoolantCoolTemp))
                {
                    float gap = CoolantCoolTemp - 300f;
                    CoolantCoolTemp -= (gap * Time.deltaTime * 0.2f);//
                }
                if (coolantCoolTemp < 300f) 
                {
                    coolantCoolTemp = 300f;
                }
            }
            coolantInReservoir.SetTemp(CoolantCoolTemp);

            if (steamOutPipe != null && steamValveState)//steamvalvestate should also make sure steam builds up
            {
                // Send steam to turbine
                if (steamOutPipe.GetConcentration() < steamOutPipe.Volume)
                {
                    steamOutPipe.Receive(false, outputVelocity, outputPressure, steamOut, steamOut.GetTemp());
                    //subtract steam from generator
                    //steamStored_monitor -= instFlowRateCurrent;
                    //steamOut.AddConcentration(-instFlowRateCurrent);
                }
            }

            if (coolantOut != null)
            {
                //send coolant into core
                if (primaryCoolantReservoir > 0)
                {
                    if (RequiredPumpRate > 0f)
                    {
                        IFluid outFlowCoolant = new IFluid(coolantInReservoir);
                        //Kg/hr to m3[inst]
                        //Debug.Log("Req (Kg/hr?): " + RequiredPumpRate);
                        float instRate = ((RequiredPumpRate * Time.fixedDeltaTime) / 1000f) / 3600f;//
                        primaryCoolantReservoir -= instRate;
                        outFlowCoolant.SetConcentration(instRate);//m^3
                                                                  //Debug.Log("Gen Out (m3): " + instRate);
                        if (coolantOut.GetConcentration() < coolantOut.Volume)
                        {
                            coolantOut.Receive(false, coolantOutputVelocity, outputPressure, outFlowCoolant, outFlowCoolant.GetTemp());
                        }
                    }
                }
            }
        }
        public void ExternalInteractFunc(int i)
        {
            if (i == 1)
            {
                //control rods down 10k
                thresholdPumpRate -= 10000f;
            }
            else if (i == 2)
            {
                //down 100k
                thresholdPumpRate -= 100000f;
            }
            else if (i == 3)
            {
                //up 5
                thresholdPumpRate += 10000f;
            }
            else if (i == 4)
            {
                // up 10
                thresholdPumpRate += 100000f;
            }
            if (thresholdPumpRate < 0)
            {
                thresholdPumpRate = 0f;
            }
            else if (thresholdPumpRate > 2700000f)
            {
                thresholdPumpRate = 2700000f;
            }

        }
    }
}