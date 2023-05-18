using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectUniverse.Environment.Volumes;
using ProjectUniverse.Util;

namespace ProjectUniverse.Environment.Fluid
{
    public class IFluidTank : MonoBehaviour
    {
        [SerializeField] private int capacity_L;
        [SerializeField] private float fluidLevel_L;
        public float flowRate_m3hr;
        private float flowVelocity_ms = 120f;
        [SerializeField] private float maxFlowVelocity_ms = 120f;
        [Tooltip("Pressure level of the outflow pump.")]
        [SerializeField] private float outputPressure = 200f;
        [SerializeField] private bool valveState = false;
        [SerializeField] private bool valveOperable = false;
        [SerializeField] private VolumeAtmosphereController roomVolume;
        [SerializeField] private IFluidPipe inflowPipe;
        [SerializeField] private IFluidPipe outflowPipe;
        //private IFluid fluid;
        private List<IFluid> fluids = new List<IFluid>();
        public bool autofill = false;
        [Tooltip("Allows tank to change output velocity to not cause overpressure in pipes.\n" +
            "Disable automatic control to set output rate via velocity.")]
        [SerializeField] private bool automaticControl;
        private float lastInlet = 0f;
        private float lastOutlet = 0f;
        public bool fixAt85 = false;

        public float FluidLevel
        {
            get { return fluidLevel_L; }
        }
        public float FluidCapacity
        {
            get { return capacity_L; }
        }
        public float InletRate
        {
            get { return lastInlet; }
        }
        public float Outletrate
        {
            get { return lastOutlet; }
        }
        public bool AutomaticMode
        {
            get { return automaticControl; }
        }
        public bool ValveOperable
        {
            get { return valveOperable; }
        }

        // Start is called before the first frame update
        void Start()
        {
            if (autofill)
            {
                IFluid fluid;
                fluid = new IFluid("water", 70, ((capacity_L) / 1000f));
                fluid.SetDensity(1000);
                fluid.SetTemp(60);
                fluid.SetLocalPressure(1f);
                fluid.SetLocalVolume(capacity_L / 1000f);
                fluids.Add(fluid);
                for (int i = 0; i < fluids.Count; i++)
                {
                    fluidLevel_L += fluids[i].GetConcentration() * 1000f;
                }
            }
            
        }

        // Update is called once per frame
        void Update()
        {
            lastInlet = 0f;
            lastOutlet = 0f;
            if (fluidLevel_L < capacity_L)
            {
                //pull in fluid from inflow pipe
                if (inflowPipe != null)
                {
                    //L to M^3. Rate is assumed to be 1 second
                    float rate = (capacity_L - fluidLevel_L)/1000f;
                    List<IFluid> fluds = new List<IFluid>();
                    //Debug.Log("Extract");
                    fluds = inflowPipe.ExtractFluid(rate);
                    for (int inf = 0; inf < fluds.Count; inf++)
                    {
                        fluidLevel_L += fluds[inf].GetConcentration() * 1000f;
                        lastInlet += fluds[inf].GetConcentration() * 1000f;
                    }
                    for(int i = fluds.Count-1; i >= 0; i--)
                    {
                        for (int j = 0; j < fluids.Count; j++)
                        {
                            if (fluds[i].GetIDName() == fluids[j].GetIDName())
                            {
                                float newConc = fluids[j].GetConcentration() + fluds[i].GetConcentration();
                                fluids[j].SetConcentration(newConc);
                                fluds.RemoveAt(i);
                            }
                        }
                    }
                    fluids.AddRange(fluds);
                }
            }

            if (fixAt85)
            {
                if(fluidLevel_L/capacity_L < 0.84f)
                {
                    //fill to 85%
                    fluids[0].SetConcentration(capacity_L * 0.85f);
                    fluidLevel_L = capacity_L * 0.85f;
                }
            }
           
            if (valveState)
            {
                //MAKE SURE FLUID DOESN'T DROP BELOW 0L
                if (fluidLevel_L > 0f)
                {
                    if (outflowPipe != null) 
                    {
                        float maxCapacity = outflowPipe.GetConcentration();
                        if (maxCapacity < outflowPipe.Volume) { 
                            float avgTemp = 0f;
                            //float localPressure = 0f;
                            float totalConc = 0f;
                            for (int a = 0; a < fluids.Count; a++)
                            {
                                avgTemp += fluids[a].GetTemp();
                                //localPressure += fluids[a].GetLocalPressure();
                                totalConc += fluids[a].GetConcentration();
                            }
                            avgTemp /= fluids.Count;
                            //localPressure /= fluids.Count;

                            List<IFluid> outFluid = new List<IFluid>();
                            ///
                            /// Replaced with flow rate calculation
                            /// Calc is in m^3/hr
                            /// Convert to L/[instant]
                            /// 
                            float limVel = flowVelocity_ms;
                            if (automaticControl)
                            {
                                if(limVel >= outflowPipe.MaxVelocity)
                                {
                                    limVel = outflowPipe.MaxVelocity;
                                }
                            }
                            flowRate_m3hr = Utils.CalculateFluidFlowThroughPipe(outflowPipe.InnerDiameter, limVel);
                            float redux = ((flowRate_m3hr) / 3600f) * 1000f;
                            redux *= Time.deltaTime;
                            
                            if (fluidLevel_L - redux < 0)
                            {
                                redux = fluidLevel_L;
                            }
                            if (outflowPipe.GetConcentration() + (redux/1000f) > outflowPipe.Volume)
                            {
                                redux = (outflowPipe.Volume - outflowPipe.GetConcentration())*1000f;
                            }
                            float ratio;
                            for (int b = 0; b < fluids.Count; b++)
                            {
                                ratio = fluids[b].GetConcentration() / totalConc;
                                IFluid outflud = new IFluid(fluids[b]);
                                float concLeft = outflud.GetConcentration() - ((redux / 1000f) * ratio);//l to m^3
                                outflud.SetConcentration((redux / 1000f) * ratio);
                                outFluid.Add(outflud);
                                fluidLevel_L -= redux * ratio;
                                lastOutlet += redux * ratio;
                                fluids[b].SetConcentration(concLeft);
                            }
                            ///
                            /// Flow is lossy (7/1155000L) due to imprecision errors.
                            ///
                            outflowPipe.Receive(false, limVel, outputPressure, outFluid, avgTemp);
                        }
                    }
                    else if (roomVolume != null)
                    {
                        float limVel = flowVelocity_ms;
                        //stop uncapped flows from flooding volumes
                        if (automaticControl)
                        {
                            limVel = 0f;
                        }
                        else
                        {
                            limVel = maxFlowVelocity_ms;
                        }
                        flowRate_m3hr = Utils.CalculateFluidFlowThroughPipe(0.408f, limVel);
                        float redux = ((flowRate_m3hr) / 3600f) * 1000f;
                        redux *= Time.deltaTime;
                        List<IFluid> outFluid = new List<IFluid>();
                        for (int b = 0; b < fluids.Count; b++)
                        {
                            IFluid outflud = new IFluid(fluids[b]);
                            float concLeft = outflud.GetConcentration() - ((redux / 1000f));//l to m^3
                            outflud.SetConcentration((redux / 1000f));
                            outFluid.Add(outflud);
                            fluidLevel_L -= redux;
                            lastOutlet += redux;
                            fluids[b].SetConcentration(concLeft);
                        }
                        roomVolume.AddRoomFluid(outFluid);
                    }
                }
            }
        }

        public void ExternalInteractFunc()
        {
            valveState = !valveState;
        }

        /*public void OnValueActivated(int mode)
        {
            switch (mode)
            {
                case 0:
                    valveState = !valveState;
                    break;
            }
        }*/
    }
}