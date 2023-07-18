using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using ProjectUniverse.Environment.Volumes;
using ProjectUniverse.Util;

namespace ProjectUniverse.Environment.Gas
{
    /// <summary>
    /// Transfer Gases of X types at whatever rate/pressure determined by the pump
    /// Vars:
    /// IGas[] - Array of gases found in pipe
    /// | next - next pipe in linkedlist
    /// | temp - temperature of the pipe. Influenced by internal gas temp and ambient temp.
    /// | tempTol[H,C] - pipe tollerance of hot or cold before bursting
    /// | GlobalPressure - total system/static pressure. Does not factor in temp or volume.
    /// | LocalPressure - interal applied pressure based on temp and volume
    /// | maxP - maxiumum intenal pressure the pipe can handle
    /// | health - pipe's health
    /// | leakrate - % leak from bullet holes into local atmo
    /// | bulletholes - List of all bullet holes in the pipe. Mainly for VFX placemement purposes.
    /// | insulationRating - percent of heat kept in or out. Intended to be higher for superhot pipes, and lower for ducts and water pipes.
    /// 
    /// Transfer gases, pressure, and temp to the 'next' duct(s)
    /// 'next' is set by master node while it traverses the whole duct line.
    /// </summary> 

    //airducts: Gas: Oxygen at 70F, 1.387132g/L, 1.544215m3 in 0.4m3 at 1.047273atm
    public class IGasPipe : MonoBehaviour
    {
        [SerializeField] private List<IGas> gasses = new List<IGas>();
        //private IGasPipe next;
        //private LinkedListNode<IGasPipe> nextLLN;
        [SerializeField] private IGasPipe[] neighbors;
        [SerializeField] private float temp;
        [SerializeField] private float[] tempTol = new float[2];
        [SerializeField] private float maxPressure_bar = 216f;
        [SerializeField] private float globalPressure;
        //[SerializeField] private float appliedPressure;
        [SerializeField] private float volume_m3;//standard duct is .4m3
        [SerializeField] private float health;
        [SerializeField] private float leakRate;
        [SerializeField] private GameObject[] bulletHoles;
        [SerializeField] private GameObject vent;
        [SerializeField] private Volume ductVolume;
        [SerializeField] private float insulationRating = 0.1f;
        [SerializeField] private bool burst;
        [Tooltip("if true, pipes will work outside of room volumes")]
        [SerializeField] private bool ignoreNeighborConstraint = false;
        [SerializeField] private float throughput_m3hr;
        [SerializeField] private float equivalentDiameterInner_m = 0.408f;
        [SerializeField] private float maxVelocity_ms = 120.5f;
        private float flowVelocity_ms = 0f;
        public int gassescount;
        private AudioSource ventSFX;

        public bool IsBurst
        {
            get { return burst; }
            set { burst = true; }
        }

        public List<IGas> Gasses
        {
            get { return gasses; }
            set { gasses = value; }
        }

        public float Temperature
        {
            get { return temp; }
            set { temp = value; }
        }

        public float Volume
        {
            get { return volume_m3; }
        }

        public GameObject Vent
        {
            get { return vent; }
            set { vent = value; }
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

        public void TransferTo(IGasPipe next, float inputVelocity, float inputPressure, List<IGas> inputGas, float avgTemp)
        {
            //set temps and pressures
            temp = avgTemp;
            globalPressure = inputPressure;
            gasses = inputGas;
            flowVelocity_ms = inputVelocity;
            next.Receive(true, inputVelocity, inputPressure, inputGas, avgTemp);
        }
        
        public float GetConcentration()
        {
            float conc = 0;
            for (int g = 0; g < gasses.Count; g++)
            {
                conc += gasses[g].GetConcentration();
            }
            return conc;
        }

        /// <summary>
        /// Receive temperature, pressure, and gasses to the pipe/cable.
        /// IE add
        /// Destructive replaces the original gas list with the parameter list
        /// </summary>
        /// <param name="atmoData"></param>
        /*public void Receive(bool destructive, params object[] atmoData)
        {
            bool flag = false;
            if (atmoData[2].GetType() == typeof(List<IGas>) && ((List<IGas>)atmoData[2]).Count > 0)
            {
                flag = true;
                if (destructive)
                {
                    gasses = (List<IGas>)atmoData[2];
                }
                else
                {
                    gasses.AddRange((List<IGas>)atmoData[2]);
                }
            }
            else if (atmoData[2].GetType() == typeof(IGas) && ((IGas)atmoData[2]).GetConcentration() > 0)
            {
                flag = true;
                if (destructive)
                {
                    gasses.Clear();
                    gasses.Add((IGas)atmoData[2]);
                }
                else
                {
                    gasses.Add((IGas)atmoData[2]);
                }
            }
            //if (((List<IGas>)atmoData[2]).Count > 0)
            //{
            if (flag)
            {
                temp = (float)atmoData[0];
                if (destructive)
                {
                    globalPressure = (float)Math.Round((float)atmoData[1], 4);
                    
                }
                else
                {
                    globalPressure += (float)Math.Round((float)atmoData[1], 4);
                }

                //calculate the change in globalpressure based temp on volume or temp for local pressure calcs
                //float totalPressure = 0.0f;
                //Debug.Log("Pre vol adj: " + globalPressure);
                foreach (IGas gas in gasses)
                {
                    ///P1*V1/T1 = P2*V2/T2///
                    float p1 = gas.GetLocalPressure();
                    float v1 = gas.GetLocalVolume();
                    //convert temp to K
                    float t1 = ((gas.GetTemp() - 32f) * (5f / 9f)) + 273.15f;
                    float p2;
                    float v2 = volume_m3;
                    //convert to K
                    float t2 = ((temp - 32f) * (5f / 9f)) + 273.15f;
                    //Debug.Log("p1: " +p1 + "v1: " +v1+ "t1: " +t1 + "v2: " +v2+ "t2: "+t2);
                    p2 = (p1 * v1 * t2) / (t1 * v2);
                    //add the partial pressure of this gas to the total pressure in the duct
                    //Debug.Log(gameObject+" adj totalPressure is "+totalPressure+" + "+p2);
                    totalPressure += p2;

                    //update the volume params for each gas in this volume in case this volume is not the same as the volume that passed the gas in.
                    gas.SetLocalVolume(volume_m3);
                    //Debug.Log("Current Concentration(s): " + gas.GetConcentration());
                }
                //Debug.Log("Total Pressure after vol adj: " + (float)Math.Round(totalPressure,4));
                //appliedPressure = (float)Math.Round(totalPressure, 4);
                //Debug.Log("Post vol adj: " + globalPressure);
                gasses = CheckGasses(globalPressure);//appliedPressure

                //Debug.Log(gasses[0]);
            }
            //}
        }*/

        public void Receive(bool destructive, float inputVelocity, float inputPressure, List<IGas> inputGas, float avgTemp)
        {
            bool flag = false;
            if (inputGas.Count > 0)
            {
                flag = true;
                if (destructive)
                {
                    Gasses = inputGas;
                }
                else
                {
                    Gasses.AddRange(inputGas);
                }
            }

            if (flag)
            {
                temp = avgTemp;
                globalPressure = inputPressure;
                flowVelocity_ms = inputVelocity;
                Gasses = CheckGasses(globalPressure);
            }
            //calc throughput
            throughput_m3hr = Utils.CalculateGasFlowThroughPipe(InnerDiameter, FlowVelocity, GlobalPressure);
        }

        public void Receive(bool destructive, float inputVelocity, float inputPressure, IGas inputGas, float avgTemp)
        {
            bool flag = false;
            if (inputGas.GetConcentration() > 0)
            {
                flag = true;
                if (destructive)
                {
                    Gasses.Clear();
                    Gasses.Add(inputGas);
                }
                else
                {
                    Gasses.Add(inputGas);
                }
            }
            if (flag)
            {
                temp = avgTemp;
                globalPressure = inputPressure;
                flowVelocity_ms = inputVelocity;
                Gasses = CheckGasses(globalPressure);
            }
            throughput_m3hr = Utils.CalculateGasFlowThroughPipe(InnerDiameter, FlowVelocity, GlobalPressure);
        }

        /// <summary>
        /// Remove qHeat from the gas temps
        /// </summary>
        /// <param name="dt"></param>
        public void RemoveHeat(float qHeat)
        {
            float conc = 0f;
            foreach (IGas gas in gasses)
            {
                conc += (gas.GetConcentration()) / 1.093f;//c*1000f
            }
            float dt = (qHeat / (conc * 2010f));
            foreach (IGas gas in gasses)
            {
                float t = gas.GetTemp();
                gas.SetTemp(t - (dt*(5f/9f)));
            }
            Temperature -= (dt * (5f / 9f));
        }
        
        public float GetGasTempF(bool getAsKelvin)
        {
            float temp = 0f;
            if (Gasses.Count > 0) 
            {
                foreach (IGas gas in gasses)
                {
                    temp += gas.GetTemp();
                }
                temp /= gasses.Count;
            }
            if (getAsKelvin)
            {
                return ((temp - 32f) / 1.8f) + 273.15f;
            }
            else
            {
                return temp;
            }
            
        }

        public float GetKgSteam()
        {
            float conc = 0f;
            foreach (IGas gas in gasses)
            {
                conc += gas.GetConcentration();
            }
            return (conc * 1000f) / 1.093f;
        }

        /// <summary>
        /// Empty this pipe into the machine/target gas list
        /// </summary>
        /// <returns></returns>
        public List<IGas> ExtractGasses(float rate_m3s)
        {
            if (rate_m3s == -1f || rate_m3s > (throughput_m3hr / 3600f))
            {
                rate_m3s = throughput_m3hr / 3600f;
            }
            rate_m3s *= Time.deltaTime;

            float percent;
            float total = 0f;
            //get total of all gas in this pipe
            for (int i = 0; i < Gasses.Count; i++)
            {
                total += gasses[i].GetConcentration();
            }
            percent = rate_m3s / total;
            if (percent > 1f)
            {
                percent = 1f;
            }

            float oldTotal = total;
            List<IGas> extractedGasses = new List<IGas>();
            for (int g = 0; g < gasses.Count; g++)
            {
                float amt = gasses[g].GetConcentration();
                IGas newFluid = new IGas(gasses[g]);
                newFluid.SetConcentration(amt * percent);
                gasses[g].SetConcentration(amt * (1 - percent));
                total -= amt * percent;
                extractedGasses.Add(newFluid);
                //reset gas and pressures if empty
                if (gasses[g].GetConcentration() == 0)
                {
                    gasses.RemoveAt(g);
                    //new pressure is a ratio of how much gas was removed
                    globalPressure *= total / oldTotal;
                }
            }
            return extractedGasses;
        }

        /// <summary>
        /// Check the gas pipe's internal gas list for duplicates and combine them. totalPressure is the pipe's global pressure
        /// </summary>
        /// <param name="totalPressure"></param>
        /// <returns></returns>
        public List<IGas> CheckGasses(float totalPressure)
        {
            //Debug.Log(gasses.Count+" gasses in pipe.");
            if (gasses.Count > 1)
            {

                List<IGas> newGassesList = gasses;//new List<IGas>();
                                                  //combine all same gasses
                for (int i = 0; i < newGassesList.Count; i++)
                {
                    for (int j = 0; j < newGassesList.Count; j++)
                    {
                        if (i != j)
                        {
                            if (newGassesList[i].GetIDName() == newGassesList[j].GetIDName())
                            {
                                IGas EQgas = Utils.CombineGases(gasses[i], gasses[j], totalPressure);
                                newGassesList.Remove(gasses[i]);
                                newGassesList.Remove(gasses[j - 1]);
                                newGassesList.Add(EQgas);
                            }
                        }
                    }
                }
                return newGassesList;
            }
            else
            {
                //gasses is empty or only has one has in it
                return gasses;
            }
        }

        public void TempEQWithDuct()
        {
            //when we intro multiple gasses this will need rewritten. Different gasses will equalize temp over time.
            //all will share uniform pressure, volume, etc. However the differences in temp means that every gas will exert
            //differing amounts of partial pressure on the pipe.
            foreach (IGas gas in gasses)
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

        public void VentToVolume()
        {
            //Play the vent airflow sound from the vent object
            if (ventSFX == null)
            {
                ventSFX = Vent.GetComponent<AudioSource>();
                if (ventSFX == null)
                {
                    ventSFX = Vent.GetComponentInChildren<AudioSource>();
                }
            }
            if(ventSFX != null)
            {
                if (!ventSFX.isPlaying && gasses.Count > 1)
                {
                    ventSFX.Play();
                }
                else if (ventSFX.isPlaying && gasses.Count == 0)
                {
                    ventSFX.Stop();
                }
            }
            else
            {
                Debug.Log("no ventSFX on " + gameObject.transform.parent.parent.name);
            }
            
            //Debug.Log("Vent?");
            VolumeAtmosphereController roomVAC = ductVolume.GetComponent<VolumeAtmosphereController>();
            //float roomVolume = roomVAC.GetVolume();
            IGas remainder;
            //Vents will fill rooms with IGas Oxygen at 1000L/s. 
            //Room oxygenation will be determined by ratio of oxygen(m3) to roomVolume(m3)
            //Room temp will be set according to the avg gas temp

            //1000Ls in total, composition will be based off of the percent concentration in the duct
            //this is so that hazardous or explosive gasses will circulate into every ventilated room.
            if (roomVAC.GetPressure() < 1.0f)
            {
                //Debug.Log("Checking room gasses");
                gasses = CheckGasses(globalPressure);//precautionary check
                if (gasses.Count > 1)
                {
                    //Debug.Log("Vent");
                    float totalConc = 0.0f;
                    //calc total concentration
                    foreach (IGas gas in gasses)
                    {
                        totalConc += gas.GetConcentration();
                    }
                    for (int g = 0; g < gasses.Count; g++)
                    {
                        //determine what percentage of all present gasses this single gas accounts for
                        float ratio = gasses[g].GetConcentration() / totalConc;
                        Debug.Log("transfer ratio: " + ratio);
                        //create the gas that will be sent into the room
                        IGas outflowX = Utils.SubtractGas(1f, gasses[g], out remainder);
                        gasses[g] = remainder;
                        //the change in pressure of the gas needs reflected to the duct.
                        globalPressure = gasses[g].GetLocalPressure();
                        roomVAC.AddRoomGas(outflowX);
                    }
                }
                else
                {
                    //Debug.Log("Vent");
                    IGas outflow = Utils.SubtractGas(1f, gasses[0], out remainder);//gasses[0] will eventually not be oxygen
                    gasses[0] = remainder;
                    //the change in pressure of the gas needs reflected to the duct.
                    globalPressure = gasses[0].GetLocalPressure();
                    //Debug.Log("Outflow: " + outflow.ToString());
                    roomVAC.AddRoomGas(outflow);
                }
            }
        }

        void Update()
        {
            if (Neighbors.Length > 0)
            {
                ignoreNeighborConstraint = true;
            }

            if (gasses.Count > 0)
            {
                //TempEQWithDuct();
                //if (neighbors.Length > 0 && vent != null)
                //{
                //    VentToVolume();
                //}

                // Runs on linked (cross-volume) ducts.
                if (ignoreNeighborConstraint)
                {
                    if ((neighbors.Length > 0) && !burst)
                    {
                        //Debug.Log("TRANSFER");
                        float totalPressures = globalPressure;
                        if (float.IsNaN(totalPressures))
                        {
                            totalPressures = 0f;
                        }
                        float totalVelocity = flowVelocity_ms;
                        float totalConc = 0.0f;
                        float totalTemp = temp;

                        //get total volume, pressure, conc of all gasses in this and neighbors
                        foreach (IGas gass in gasses)
                        {
                            totalConc += gass.GetConcentration();
                        }
                        foreach (IGasPipe pipe in neighbors)
                        {
                            if (!float.IsNaN(pipe.GlobalPressure))
                            {
                                totalPressures += pipe.GlobalPressure;
                            }
                            totalTemp += pipe.temp;
                            totalVelocity += pipe.FlowVelocity;
                            //get total concentration
                            foreach (IGas gas in pipe.gasses)
                            {
                                totalConc += gas.GetConcentration();
                            }
                        }
                        //Global Pressure Eq calc
                        float tEq_global = totalTemp / (neighbors.Length + 1);
                        float pEq_global = totalPressures / (neighbors.Length + 1);
                        float cEq_global = totalConc / (neighbors.Length + 1);
                        float vEq_global = totalVelocity / (neighbors.Length + 1);
                        for (int g = 0; g < neighbors.Length; g++)
                        {
                            List<IGas> newGassesList = new List<IGas>();
                            for (int j = 0; j < gasses.Count; j++)
                            {
                                //this gas is the Eq'd gas.
                                IGas tempGas = new IGas(gasses[j].GetIDName(), tEq_global, cEq_global, pEq_global, volume_m3);
                                tempGas.CalculateAtmosphericDensity();
                                newGassesList.Add(tempGas);
                            }
                            //object[] newAtmoComp = { tEq_global, pEq_global, newGassesList };
                            //This needs to be limitable by throughput, somehow
                            TransferTo(neighbors[g], vEq_global, pEq_global, newGassesList, tEq_global);
                        }
                    }
                }
                if (Vent != null && ductVolume != null)
                {
                    VentToVolume();
                }
            }
            else
            {
                //no gasses
                globalPressure = 0f;
            }
            gassescount = gasses.Count;
        }

        //public float AppliedPressure
        //{
        //    get { return appliedPressure; }
        //}
        public float MaxPressure
        {
            get { return maxPressure_bar; }
        }
        public float GlobalPressure
        {
            get { return globalPressure; }
        }
        public IGasPipe[] Neighbors
        {
            get { return neighbors; }
            set { neighbors = value; }
        }

        /// <summary>
        /// Both neighbor arrays will begin at len 0, this fuction can be expected to be called twice, once by each side.
        /// </summary>
        /// <param name="neighborDuct"></param>
        public void AddNeighbor(IGasPipe neighborDuct)
        {
            //Debug.Log(this.name + " and " + neighborDuct.name);
            //reset the neighbor list??
            neighbors = new IGasPipe[1];
            neighbors[0] = neighborDuct;

            neighborDuct.Neighbors = new IGasPipe[1];
            neighborDuct.Neighbors[0] = this;
            //Debug.Log(Neighbors.Length + " " + neighborDuct.Neighbors.Length);
        }
    }
}