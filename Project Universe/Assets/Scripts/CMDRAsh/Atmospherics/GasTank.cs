using ProjectUniverse.Environment.Volumes;
using ProjectUniverse.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUniverse.Environment.Gas
{
    public class GasTank : MonoBehaviour
    {
        [SerializeField] private int capacity_L;
        [SerializeField] private float gasLevel_L;
        public float flowRate_m3hr;
        private float flowVelocity_ms = 120f;
        [SerializeField] private float maxFlowVelocity_ms = 120f;
        [Tooltip("Pressure level of the outflow pump.")]
        [SerializeField] private float outputPressure = 200f;
        [SerializeField] private bool valveState;
        [SerializeField] private VolumeAtmosphereController roomVolume;
        [SerializeField] private IGasPipe inflowPipe;
        [SerializeField] private IGasPipe outflowPipe;
        //private IFluid fluid;
        private List<IGas> gasses = new List<IGas>();
        public bool autofill = false;
        [Tooltip("Allows tank to change output velocity to not cause overpressure in pipes.\n" +
            "Disable automatic control to set output rate via velocity.")]
        [SerializeField] private bool automaticControl;

        // Start is called before the first frame update
        void Start()
        {
            if (autofill)
            {
                IGas gas;
                gas = new IGas("Steam", 70, ((capacity_L) / 1000f));
                gas.SetDensity(1000);
                gas.SetTemp(850f);//F, 455C
                gas.SetLocalPressure(200f);
                gas.SetLocalVolume(capacity_L / 1000f);
                gasses.Add(gas);
                for (int i = 0; i < gasses.Count; i++)
                {
                    gasLevel_L += gasses[i].GetConcentration() * 1000f;
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (gasLevel_L < capacity_L)
            {
                //pull in fluid from inflow pipe
                if (inflowPipe != null)
                {
                    //L to M^3. Rate is assumed to be 1 second
                    float rate = (capacity_L - gasLevel_L) / 1000f;
                    List<IGas> fluds = new List<IGas>();
                    //Debug.Log("Extract");
                    fluds = inflowPipe.ExtractGasses(rate);
                    for (int inf = 0; inf < fluds.Count; inf++)
                    {
                        gasLevel_L += fluds[inf].GetConcentration() * 1000f;
                    }
                    for (int i = fluds.Count - 1; i >= 0; i--)
                    {
                        for (int j = 0; j < gasses.Count; j++)
                        {
                            if (fluds[i].GetIDName() == gasses[j].GetIDName())
                            {
                                float newConc = gasses[j].GetConcentration() + fluds[i].GetConcentration();
                                gasses[j].SetConcentration(newConc);
                                fluds.RemoveAt(i);
                            }
                        }
                    }
                    gasses.AddRange(fluds);
                }
            }

            if (valveState)
            {
                if (gasLevel_L > 0f)
                {
                    if (outflowPipe != null)
                    {
                        float maxCapacity = outflowPipe.GetConcentration();
                        if (maxCapacity < outflowPipe.Volume)
                        {
                            float avgTemp = 0f;
                            //float localPressure = 0f;
                            float totalConc = 0f;
                            for (int a = 0; a < gasses.Count; a++)
                            {
                                avgTemp += gasses[a].GetTemp();
                                //localPressure += fluids[a].GetLocalPressure();
                                totalConc += gasses[a].GetConcentration();
                            }
                            avgTemp /= gasses.Count;
                            //localPressure /= fluids.Count;

                            List<IGas> outFluid = new List<IGas>();
                            ///
                            /// Replaced with flow rate calculation
                            /// Calc is in m^3/hr
                            /// Convert to L/[instant]
                            /// 
                            float limVel = flowVelocity_ms;
                            if (automaticControl)
                            {
                                if (limVel >= outflowPipe.MaxVelocity)
                                {
                                    limVel = outflowPipe.MaxVelocity;
                                }
                            }
                            flowRate_m3hr = Utils.CalculateGasFlowThroughPipe(outflowPipe.InnerDiameter, limVel, outputPressure);
                            float redux = ((flowRate_m3hr) / 3600f) * 1000f;
                            redux *= Time.deltaTime;

                            if (gasLevel_L - redux < 0)
                            {
                                redux = gasLevel_L;
                            }
                            if (outflowPipe.GetConcentration() + (redux / 1000f) > outflowPipe.Volume)
                            {
                                redux = (outflowPipe.Volume - outflowPipe.GetConcentration()) * 1000f;
                            }
                            float ratio;
                            for (int b = 0; b < gasses.Count; b++)
                            {
                                ratio = gasses[b].GetConcentration() / totalConc;
                                IGas outflud = new IGas(gasses[b]);
                                float concLeft = outflud.GetConcentration() - ((redux / 1000f) * ratio);//l to m^3
                                outflud.SetConcentration((redux / 1000f) * ratio);
                                outFluid.Add(outflud);
                                gasLevel_L -= redux * ratio;
                                gasses[b].SetConcentration(concLeft);
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
                        ///
                        /// output pressure will need to be affected by the velocity (when at fixed outputs
                        /// or when V is limited and flow needs to be higher)
                        ///
                        flowRate_m3hr = Utils.CalculateGasFlowThroughPipe(0.408f, limVel, outputPressure);
                        float redux = ((flowRate_m3hr) / 3600f) * 1000f;
                        redux *= Time.deltaTime;
                        //List<IGas> outFluid = new List<IGas>();
                        for (int b = 0; b < gasses.Count; b++)
                        {
                            IGas outflud = new IGas(gasses[b]);
                            float concLeft = outflud.GetConcentration() - ((redux / 1000f));//l to m^3
                            outflud.SetConcentration((redux / 1000f));
                            //outFluid.Add(outflud);
                            gasLevel_L -= redux;
                            gasses[b].SetConcentration(concLeft);
                            roomVolume.AddRoomGas(outflud);//outFluid
                        }
                        
                    }
                }
            }
        }
    }
}