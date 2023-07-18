using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using ProjectUniverse.Environment.Gas;
using ProjectUniverse.Util;

namespace ProjectUniverse.Environment.Fluid
{
    /// <summary>
    /// Transfer Fluids of X types at whatever rate/pressure determined by the pump
    /// Vars:
    /// IFluid[] - Array of fluids found in pipe
    /// | next - next pipe in linkedlist
    /// | temp - temperature of the pipe. Influenced by internal fluid temp and ambient temp.
    /// | tempTol[H,C] - pipe tollerance of hot or cold before bursting
    /// | GlobalPressure - total system/static pressure. Does not factor in temp or volume.
    /// | LocalPressure - interal applied pressure based on temp and volume
    /// | maxP - maxiumum intenal pressure the pipe can handle
    /// | health - pipe's health
    /// | leakrate - % leak from bullet holes into local atmo
    /// | bulletholes - List of all bullet holes in the pipe. Mainly for VFX placemement purposes.
    /// | insulationRating - percent of heat kept in or out. Intended to be higher for superhot pipes, and lower for ducts and water pipes.
    /// 
    /// Transfer fluids, pressure, and temp to the 'next' duct(s)
    /// </summary> 

    public class IFluidPipe : MonoBehaviour
    {
        [SerializeField] private List<IFluid> fluids = new List<IFluid>();
        [SerializeField] private IFluidPipe[] neighbors;
        [SerializeField] private float temp = 70f;
        [SerializeField] private float[] tempTol = new float[2];
        [SerializeField] private float maxPressure_bar = 216f;
        [SerializeField] private float globalPressure=0f;
        [SerializeField] private float volume_m3;//standard pipe is ?m3
        [SerializeField] private float health = 100f;
        [SerializeField] private float leakRate;
        //[SerializeField] private GameObject[] bulletHoles;
        [SerializeField] private float throughput_m3hr;
        [SerializeField] private float equivalentDiameterInner_m = 0.408f;
        [SerializeField] private Volume ductVolume;
        [SerializeField] private float insulationRating = 0.5f;
        [SerializeField] private bool burst;
        [Tooltip("if true, pipes will work outside of room volumes")]
        [SerializeField] private bool ignoreNeighborConstraint = false;
        [SerializeField] private float maxVelocity_ms = 120.5f;
        private float flowVelocity_ms = 0f;

        public bool IsBurst
        {
            get { return burst; }
            set { burst = true; }
        }
        
        public List<IFluid> Fluids
        {
            get { return fluids; }
            set { fluids = value; }
        }

        public float Temperature
        {
            get { return temp; }
            set { temp = value; }
        }
        //public float AppliedPressure
        //{
        //    get { return appliedPressure; }
        //}
        public float GlobalPressure
        {
            get { return globalPressure; }
        }
        public IFluidPipe[] Neighbors
        {
            get { return neighbors; }
            set { neighbors = value; }
        }
        public float Volume
        {
            get { return volume_m3; }
        }
        public float Throughput
        {
            get { return throughput_m3hr; }
        }

        public float InnerDiameter
        {
            get { return equivalentDiameterInner_m; }
        }
        
        public float MaxVelocity
        {
            get { return maxVelocity_ms; }
        }

        public float FlowVelocity
        {
            get { return flowVelocity_ms; }
            set { flowVelocity_ms = value; }
        }

        /// 
        /// Add velocity stuffs. Create new func for fluid injection into pipe?
        ///
        public void TransferTo(IFluidPipe next, float inputVelocity, float inputPressure, List<IFluid> inputFluid, float avgTemp)
        {
            //set temps and pressures
            temp = avgTemp;
            globalPressure = inputPressure;
            fluids = inputFluid;
            flowVelocity_ms = inputVelocity;
            next.Receive(true, inputVelocity, inputPressure, inputFluid, avgTemp);
        }

        public float GetConcentration()
        {
            float conc = 0;
            for (int g = 0; g < fluids.Count; g++)
            {
                conc += fluids[g].GetConcentration();
            }
            return conc;
        }

        /// <summary>
        /// Receive temperature, pressure, and gasses to the pipe/cable.
        /// IE add
        /// Destructive replaces the original gas list with the parameter list
        /*
        public void Receive(bool destructive, params object[] fluidData)
        {
            bool flag = false;
            if (fluidData[2].GetType() == typeof(List<IFluid>) && ((List<IFluid>)fluidData[2]).Count > 0)
            {
                flag = true;
                if (destructive)
                {
                    Fluids = (List<IFluid>)fluidData[2];
                }
                else
                {
                    List<IFluid> addData = (List<IFluid>)fluidData[2];
                    Fluids.AddRange(addData);
                }
            }
            else if (fluidData[2].GetType() == typeof(IFluid) && ((IFluid)fluidData[2]).GetConcentration() > 0)
            {
                flag = true;
                if (destructive)
                {
                    Fluids.Clear();
                    Fluids.Add(new IFluid((IFluid)fluidData[2]));
                }
                else
                {
                    Fluids.Add(new IFluid((IFluid)fluidData[2]));
                }
            }
            if (flag)
            {
                temp = (float)fluidData[0];
                if (destructive)
                {
                    globalPressure = (float)Math.Round((float)fluidData[1], 4);
                }
                else
                {
                    globalPressure = (float)Math.Round((float)fluidData[1], 4);
                }

               
                fluids = CheckFluids(globalPressure);
            }
        }*/

        /// <summary>
        /// Receive temperature, pressure, and gasses to the pipe/cable.
        /// IE add
        /// Destructive replaces the original gas list with the parameter list
        /// </summary>
        public void Receive(bool destructive, float inputVelocity, float inputPressure, List<IFluid> inputFluid, float avgTemp)
        {
            bool flag = false;
            if (inputFluid.Count > 0)
            {
                flag = true;
                if (destructive)
                {
                    Fluids = inputFluid;
                }
                else
                {
                    Fluids.AddRange(inputFluid);
                }
            }
            
            if (flag)
            {
                temp = avgTemp;
                globalPressure = inputPressure;
                flowVelocity_ms = inputVelocity;
                fluids = CheckFluids(globalPressure);
            }
            //calc throughput
            throughput_m3hr = Utils.CalculateFluidFlowThroughPipe(InnerDiameter, FlowVelocity);
            
            //string[] nameparts = name.Split('(');
            //string[] nameend = nameparts[1].Split(')');
            //if (string.Compare(nameend[0], "Cond1") != 0)
            //{
                //Debug.Log(name + ": " + InnerDiameter + ", " + FlowVelocity + " -> " + throughput_m3hr);
            //}
            
        }

        /// <summary>
        /// Receive temperature, pressure, and gasses to the pipe/cable.
        /// IE add
        /// Destructive replaces the original gas list with the parameter list
        /// </summary>
        public void Receive(bool destructive, float inputVelocity, float inputPressure, IFluid inputFluid, float avgTemp)
        {
            bool flag = false;
            if (inputFluid.GetConcentration() > 0)
            {
                flag = true;
                if (destructive)
                {
                    Fluids.Clear();
                    Fluids.Add(inputFluid);
                }
                else
                {
                    Fluids.Add(inputFluid);
                }
            }
            if (flag)
            {
                temp = avgTemp;
                globalPressure = inputPressure;
                flowVelocity_ms = inputVelocity;
                fluids = CheckFluids(globalPressure);
            }
            throughput_m3hr = Utils.CalculateFluidFlowThroughPipe(InnerDiameter, FlowVelocity);
            //Debug.Log(throughput_m3hr+" -> dm/dt: "+((throughput_m3hr / 3600)*Time.deltaTime));
            //Debug.Log(name+": "+InnerDiameter + ", " + FlowVelocity + " -> "+throughput_m3hr);
        }

        /// <summary>
        /// Check the gas pipe's internal gas list for duplicates and combine them. totalPressure is the pipe's global pressure
        /// </summary>
        /// <param name="totalPressure"></param>
        /// <returns></returns>
        public List<IFluid> CheckFluids(float totalPressure)
        {
            //Debug.Log(gasses.Count+" gasses in pipe.");
            if (fluids.Count > 1)
            {
                //Debug.Log("Checking fluids");
                List<IFluid> newfluidsList = fluids;//new List<IGas>();
                                                    //combine all same gasses
                //foreach (IFluid gas in newfluidsList)
                //{
                    //Debug.Log(": "+gas);
                //}
                for (int i = 0; i < newfluidsList.Count; i++)
                {
                    for (int j = 0; j < newfluidsList.Count; j++)
                    {
                        if (i != j)
                        {
                            if (newfluidsList[i].GetIDName() == newfluidsList[j].GetIDName())
                            {
                                //Debug.Log(fluids[i] + " " + fluids[j]);                                
                                IFluid EQfluid = Utils.CombineFluids(fluids[i], fluids[j], totalPressure);
                                //Debug.Log(EQfluid);
                                newfluidsList.Remove(fluids[i]);
                                newfluidsList.Remove(fluids[j - 1]);
                                newfluidsList.Add(EQfluid);
                            }
                        }
                    }
                }
                return newfluidsList;
            }
            else
            {
                //gasses is empty or only has one has in it
                return fluids;
            }
        }

        /// <summary>
        /// Empty this pipe into the machine/target gas list
        /// </summary>
        /// <returns></returns>
        public List<IFluid> ExtractFluid(float rate_m3s)
        {
            if (rate_m3s == -1f || rate_m3s > (throughput_m3hr / 3600f))
            {
                //m^3 per second
                rate_m3s = throughput_m3hr / 3600f;
            }
            rate_m3s *= Time.deltaTime;//scale output to one second

            float percent;
            float total = 0f;
            //get total of all gas in this pipe
            for (int i = 0; i < fluids.Count; i++)
            {
                total += fluids[i].GetConcentration();
            }
            percent = rate_m3s / total;
            if (percent > 1f)
            {
                percent = 1f;
            }

            float oldTotal = total;
            List<IFluid> extractedGasses = new List<IFluid>();
            for (int g = fluids.Count - 1; g >= 0 ; g--)
            {
                float amt = fluids[g].GetConcentration();
                IFluid newFluid = new IFluid(fluids[g]);
                newFluid.SetConcentration(amt * percent);
                fluids[g].SetConcentration(amt * (1 - percent));
                total -= amt * percent;
                extractedGasses.Add(newFluid);
                //reset gas and pressures if empty
                if (fluids[g].GetConcentration() == 0)
                {
                    fluids.RemoveAt(g);
                    //new pressure is a ratio of how much gas was removed
                    globalPressure *= total / oldTotal;
                }
            }
            return extractedGasses;
        }

        public void TempEQWithDuct()
        {
            //when we intro multiple gasses this will need rewritten. Different gasses will equalize temp over time.
            //all will share uniform pressure, volume, etc. However the differences in temp means that every gas will exert
            //differing amounts of partial pressure on the pipe.
            foreach (IFluid gas in fluids)
            {
                //Debug.Log("Gas / Pipe Eq, density, and pressure calcs");
                //calculate the new temp for duct and gas
                float ductTemp;
                float gasTemp;
                if (temp < gas.GetTemp())//the gas is hotter than the duct
                {
                    ductTemp = ((gas.GetTemp() + temp) / 2) + (temp * insulationRating); //heat bleeds from the gas into the duct.
                    gasTemp = ((gas.GetTemp() + temp) / 2) - (temp * insulationRating); //heat bleeds from the gas to the duct.
                }
                else if (temp > gas.GetTemp())//the gas is cooler than the duct
                {
                    ductTemp = ((gas.GetTemp() + temp) / 2) - (temp * insulationRating); //insulation to keep heat from bleeding into the pipe.
                    gasTemp = ((gas.GetTemp() + temp) / 2) + (temp * insulationRating); //heating the pipe will not affect the bulk of gas as much.
                }
                else//gas and duct are equal temps
                {
                    ductTemp = ((gas.GetTemp() + temp) / 2);
                    gasTemp = ((gas.GetTemp() + temp) / 2);
                }
                //set the new temp
                gas.SetTemp(gasTemp);
                temp = ductTemp;
            }
        }

        void Update()
        {
            //Debug.Log(GetConcentration()+"/"+Volume);
            if (fluids.Count > 0)
            {
                //TempEQWithDuct();
                if(flowVelocity_ms > maxVelocity_ms)
                {
                    Debug.Log("Burst");
                    burst = true;
                    //do burst things
                    // fluid goes into volume
                }
                if (!burst)
                {
                    // Runs on linked (cross-volume) ducts.
                    if ((neighbors.Length > 0 || ignoreNeighborConstraint) && !burst)
                    {
                        //Debug.Log("TRANSFER");
                        float totalPressures = globalPressure;
                        float totalVelocity = flowVelocity_ms;
                        float totalConc = 0.0f;
                        float totalTemp = temp;

                        //get total volume, pressure, conc of all gasses in this and neighbors
                        foreach (IFluid fluid in fluids)
                        {
                            totalConc += fluid.GetConcentration();
                        }
                        foreach (IFluidPipe pipe in neighbors)
                        {

                            totalPressures += pipe.GlobalPressure;
                            totalTemp += pipe.Temperature;
                            //totalVelocity += pipe.FlowVelocity;
                            //get total concentration
                            foreach (IFluid fluid in pipe.fluids)
                            {
                                totalConc += fluid.GetConcentration();
                            }
                        }
                        //Global Pressure Eq calc
                        float tEq_global = totalTemp / (neighbors.Length + 1);
                        float pEq_global = totalPressures / (neighbors.Length + 1);
                        float cEq_global = totalConc / (neighbors.Length + 1);
                        float vEq_global = totalVelocity;// / (neighbors.Length + 1);
                        ///
                        /// USE VEQ_GLOBAL TO CALC NEW PRESSURE FOR NEXT PIPE?
                        ///
                        for (int g = 0; g < neighbors.Length; g++)
                        {
                            List<IFluid> newGassesList = new List<IFluid>();
                            for (int j = 0; j < fluids.Count; j++)
                            {
                                //this gas is the Eq'd gas.
                                IFluid tempGas = new IFluid(fluids[j].GetIDName(), tEq_global, cEq_global, pEq_global);
                                newGassesList.Add(tempGas);
                            }
                            object[] newAtmoComp = { tEq_global, pEq_global, newGassesList };
                            //This needs to be limitable by throughput, somehow
                            TransferTo(neighbors[g], vEq_global, pEq_global, newGassesList, tEq_global);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Both neighbor arrays will begin at len 0, this fuction can be expected to be called twice, once by each side.
        /// </summary>
        /// <param name="neighborDuct"></param>
        public void AddNeighbor(IFluidPipe neighborDuct)
        {
            //Debug.Log(this.name + " and " + neighborDuct.name);
            neighbors = new IFluidPipe[1];
            neighbors[0] = neighborDuct;

            neighborDuct.Neighbors = new IFluidPipe[1];
            neighborDuct.Neighbors[0] = this;
            //Debug.Log(Neighbors.Length + " " + neighborDuct.Neighbors.Length);
        }
    }
}
