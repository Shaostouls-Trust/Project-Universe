using ProjectUniverse.Base;
using ProjectUniverse.Environment.Fluid;
using ProjectUniverse.Environment.Gas;
using ProjectUniverse.Environment.Volumes;
using ProjectUniverse.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

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
        [SerializeField] private bool waterValve = true;
        [SerializeField] private bool coolantValve = true;
        [SerializeField] private bool tankLeak = false;
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
        [SerializeField] private float steamChamberVolume = 278f;//m^3
        private float maxChamberStress = 900f;//MPa (can withstand 231.5bar at 0.0129 t/d)
        private float thicknessToDiamRatio = 0.0129f;//2.75in thick / 5.43m diam
        private float chamberPressure = 200f;//bar (20MPA)
        [SerializeField] private VolumeAtmosphereController vac;
        private bool tankBlown;
        [SerializeField] private AudioSource src;
        [SerializeField] private ScriptedExplosion scrExp;

        //old turbine vars
        private float coolantFlowRateCurrent = 0f;//Kg/hr
        private float steamFlowRateCurrent = 0f;//Kg/hr
        private float waterFlowRateMax = 2870000f;//kg/hr

        // Start is called before the first frame update
        void Start()
        {
            thresholdPumpRate = maxPumpRate;
            steamOut = new IGas("Steam", 851f, 0f, 200f, 72f);
            coolantInReservoir = new IFluid("Coolant", 80.33f, primaryCoolantReservoir, 200f);
        }

        public bool AutomaticControl { get { return automaticControl; } set { automaticControl = value; } }

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
        public float ChamberPressure
        {
            get { return chamberPressure; }
        }
        public bool TankLeak
        {
            get { return tankLeak; }
        }
        public bool SteamValveState
        {
            get { return steamValveState; }
            set { steamValveState = value; }
        }
        public bool WaterValveState
        {
            get { return waterValve; }
            set { waterValve = value; }
        }
        public bool CoolantValveState
        {
            get { return coolantValve; }
            set { coolantValve = value; }
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

        public void SetTankPressure(int hash, float pressure)
        {
            if(hash == 85432657)
            {
                chamberPressure = pressure;
            }
        }

        private void Update()
        {
            //collect water from pipe
            /// SAMPLE WATER LOGIC
            //Profiler.BeginSample("Water");
            if (waterValve)
            {
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
            }
            //Profiler.EndSample();

            /// SAMPLE COOLANT INPUT LOGIC
            //Profiler.BeginSample("CoolantIn");
            if (coolantIn != null && coolantValve)
            {
                if (CoolantReservoir < CoolantReservoirMaxCap)
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
            }
            //Profiler.EndSample();

            /// SAMPLE EXCHANGE LOGIC
            //Profiler.BeginSample("Exchange");
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
            //Profiler.EndSample();

            /// SAMPLE STEAM LOGIC
            //Profiler.BeginSample("Steam");
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
            
            //add steamFlowRateCurrent to steam gas
            steamOut.AddConcentration(instFlowRateCurrent);
            //Profiler.EndSample();

            /// SAMPLE CHAMBER PRESSURE LOGIC
            //Profiler.BeginSample("Chamber Pressure A");
            //steamOut.SetConcentration(instFlowRateCurrent);
            //the tank volume is 525m^3. 0.0054*1000. 1/18.02 g/mol * g steam. Steam is(?) 455C. p=nrt/v. 200 is base pressure.
            chamberPressure = 200f + (8.3145f*0.0555f*steamOut.GetConcentration()*108f*728.15f) / 525f;
            if(steamOut.GetConcentration() <= 0f)
            {
                //default (shutdown) pressure
                chamberPressure = 1.01f;
            }
            if (chamberPressure >= 219f)
            {
                steamOut.SetLocalPressure(219f);
            }
            else
            {
                steamOut.SetLocalPressure(chamberPressure);
            }
            //Profiler.EndSample();

            Profiler.BeginSample("Chamber Pressure");
            if (ChamberPressure > 231.5f || tankBlown)
            {
                //blow;
                tankLeak = true;
                if (!tankBlown)//on first run will be false
                {
                    scrExp.ExplodeEffect();
                    //src.Play();
                }
                tankBlown = true;
                //sfx and material stuff duh
                //steam and water leak into room
                if (vac != null)
                {
                    Profiler.BeginSample("Chamber Pressure Sub A");
                    //don't add empty steam and water
                    if(steamOut.GetConcentration() > 0f)
                    {
                        vac.AddRoomGas(steamOut);
                    }
                    chamberPressure = vac.Pressure;
                    Profiler.EndSample();
                    Profiler.BeginSample("Chamber Pressure Sub B");
                    if(WaterReservoir > 0f)
                    {
                        vac.AddRoomFluid(new IFluid("water", 300f, WaterReservoir, vac.Pressure));
                        waterStored_monitor = 0f;
                    }
                    steamOut.SetConcentration(0f);
                    Profiler.EndSample();
                }
                else
                {
                    chamberPressure = 1.01f;
                    waterStored_monitor = 0f;
                }
            }
            Profiler.EndSample();

            //else
            //change to include coolant flow, not just water flow
            //if required is greater than available
            /// SAMPLE STEAM OUT LOGIC
            //Profiler.BeginSample("Steam Out");
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

            if (steamOutPipe != null && steamValveState)
            {
                // Send steam to turbine
                if (steamOutPipe.GetConcentration() < steamOutPipe.Volume)
                {
                    //calc max flow rate for pipe
                    float throughput_m3 = Utils.CalculateGasFlowThroughPipe(
                        steamOutPipe.InnerDiameter, outputVelocity, outputPressure)/180000000f;// /3600f and 50000f
                    //Debug.Log(throughput_m3);
                    if (steamOut.GetConcentration() < throughput_m3)
                    {
                        throughput_m3 = steamOut.GetConcentration();
                    }
                    IGas outputSteamInst = new IGas(steamOut);
                    outputSteamInst.SetConcentration(throughput_m3);

                    steamOutPipe.Receive(false, outputVelocity, outputPressure, outputSteamInst, outputSteamInst.GetTemp());
                    //subtract steam from generator
                    steamOut.AddConcentration(-throughput_m3);
                }
            }
            //Profiler.EndSample();

            /// SAMPLE COOLANT OUT LOGIC
            //Profiler.BeginSample("Coolant Out");
            if (coolantOut != null && coolantValve)
            {
                //send coolant into core
                if (primaryCoolantReservoir > 0)
                {
                    if (RequiredPumpRate > 0f && !SysFail)
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
                else
                {
                    primaryCoolantReservoir = 0f;
                }
            }
            //Profiler.EndSample();
        }

        public void IncrementThresholdValue()
        {
            thresholdPumpRate += 108000f;
            if(thresholdPumpRate > maxPumpRate)
            {
                thresholdPumpRate = maxPumpRate;
            }
        }
        public void DecrementThresholdValue()
        {
            thresholdPumpRate -= 108000f;
            if (thresholdPumpRate < 0f)
            {
                thresholdPumpRate = 0f;
            }
        }

        public void CheckPressureImmediate()
        {
            if (ChamberPressure > 231.5f || tankBlown)
            {
                //blow;
                tankLeak = true;
                if (!tankBlown)//on first run will be false
                {
                    scrExp.ExplodeEffect();
                    //src.Play();
                }
                tankBlown = true;
                //sfx and material stuff duh
                //steam and water leak into room
                if (vac != null)
                {
                    vac.AddRoomGas(steamOut);
                    chamberPressure = vac.Pressure;
                    vac.AddRoomFluid(new IFluid("water", 300f, WaterReservoir, vac.Pressure));
                    waterStored_monitor = 0f;
                    steamOut.SetConcentration(0f);
                }
                else
                {
                    chamberPressure = 1.01f;
                    waterStored_monitor = 0f;
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