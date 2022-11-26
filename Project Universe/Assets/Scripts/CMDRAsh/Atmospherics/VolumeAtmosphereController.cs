using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using ProjectUniverse.Environment.Gas;
using ProjectUniverse.Environment.Fluid;
using ProjectUniverse.Animation.Controllers;
using UnityEngine.Profiling;
using ProjectUniverse.Util;

namespace ProjectUniverse.Environment.Volumes
{
    public sealed class VolumeAtmosphereController : MonoBehaviour
    {
        private float roomPressure;
        [SerializeField] private float roomTemp;//rooms cool to -200f over time, without heating
        [SerializeField] private float roomOxygenation;
        [SerializeField] private float roomVolume;
        [SerializeField] private float humidity;
        [SerializeField] private float toxicity;
        /// <summary>
        /// Order roomGases into a defined array for ease of access?
        /// </summary>
        private List<IGas> roomGases = new List<IGas>();
        private List<IGas> gasesToEq = new List<IGas>();
        private List<IFluid> roomFluids = new List<IFluid>();
        [SerializeField] private List<GameObject> roomFluidPlanes = new List<GameObject>();
        [SerializeField] private GameObject[] neighborEmpties;
        //[SerializeField] private GameObject[] roomVolumeDoors;
        private List<GameObject> connectedNeighbors = new List<GameObject>();
        [SerializeField] private int OxygenatedRoom_Priority = 10;
        [SerializeField] private int DeOxygenatedRoom_Priority = 9;
        [SerializeField] public List<PipeSection> volumeGasPipeSections;
        [SerializeField] private bool autoFill;
        public int limiter = 30;
        private float qHeatLossPerSec = -40000f;

        private void Start()
        {
            roomVolume = (gameObject.GetComponent<BoxCollider>().size.x *
                gameObject.GetComponent<BoxCollider>().size.y *
                gameObject.GetComponent<BoxCollider>().size.z);
            
            if (autoFill)
            {
                AddRoomGas(new IGas("Oxygen", 60f, roomVolume, 1.0f, roomVolume));
            }
            
            qHeatLossPerSec = ((gameObject.GetComponent<BoxCollider>().size.x * 2f) +
                (gameObject.GetComponent<BoxCollider>().size.y * 2f) +
                (gameObject.GetComponent<BoxCollider>().size.z * 2f)) * -40000f;//-1240000J per 51300L 19-3-9 rm
        }

        public float Temperature
        {
            get { return roomTemp; }
            set { roomTemp = value; }
        }
        public float Oxygenation
        {
            get { return roomOxygenation; }
            set { roomOxygenation = value; }
        }
        public float Pressure
        {
            get { return roomPressure; }
            set { roomPressure = value; }
        }
        public float Toxicity
        {
            get { return toxicity; }
            set { toxicity = value; }
        }
        public List<IGas> RoomGasses
        {
            get { return roomGases; }
            set { roomGases = value; }
        }

        ///check doors
        ///if two doors are open
        ///check the two volumes
        ///go through equalization.
        void FixedUpdate()
        {
            ///
            /// Room temp will slowly drop to -200f over time without the addition of heat through radiators.
            /// Radiators will heat the room according to how open the radiator valve is.
            /// Larger rooms heat and cool more slowly b/c room gasses will must heat and cool as well.
            /// 
            RoomHeatAmbiLoss();

            ///UnityEngine.Profiling.Profiler.BeginSample("Volume Equalization");
            //combine all same gasses in the volume
            //if (roomGases.Count > 1)
            //{
            //    roomGases = CheckGasses(false,0.0f);
            //}
            //check for the surround volumes
            //bool[] doorstates = DoorStates();
            //Debug.Log("-----");
            limiter--;
            if (limiter < 0)
            {
                limiter = 0;
            }
            for (int i = 0; i < neighborEmpties.Length; i++)//roomVolumeDoors.Length
            {
                GameObject door = neighborEmpties[i].GetComponent<VolumeNode>().GetDoor();
                //Debug.Log(door);
                //if the door (in this volume) is open
                if (door.GetComponent<DoorAnimator>().OpenOrOpening())//roomVolumeDoors
                {
                    Vector3 back = door.transform.TransformDirection(Vector3.back);//is Vector3.back for all cases?
                    //raycastAll to check neighbor door
                    //A raycast every frame?
                    RaycastHit[] hits;
                    hits = Physics.RaycastAll(new Vector3(door.transform.position.x,
                        door.transform.position.y + 0.025f, door.transform.position.z), back, 1.0f);
                    foreach(RaycastHit hit in hits)
                    {
                        if(hit.collider.gameObject != door)
                        {
                            //check if it's a door
                            //select the parent object via the DoorAnimator
                            bool clear = false;
                            Component myComponent = hit.collider.GetComponentInParent<DoorAnimator>();
                            Component myComponent2 = hit.collider.GetComponent<DoorAnimator>();
                            Component myComponent3 = hit.collider.GetComponentInChildren<DoorAnimator>();
                            //Debug.Log(hit.collider.gameObject);
                            try
                            {
                                GameObject myDoorGameobject = null;
                                if (myComponent != null)
                                {
                                    myDoorGameobject = myComponent.gameObject;
                                }
                                else if (myComponent2 != null)
                                {
                                    myDoorGameobject = myComponent2.gameObject;
                                }
                                else if (myComponent3 != null)
                                {
                                    myDoorGameobject = myComponent3.gameObject;
                                }
                                clear = myDoorGameobject.GetComponent<DoorAnimator>().OpenOrOpening();
                                if (clear)
                                {
                                    //Debug.Log("---");
                                    //Debug.Log(door);
                                    //Debug.Log(myDoorGameobject);
                                }

                            }
                            catch (Exception e)
                            {
                                //Debug.Log(e);
                                //Debug.Log("Case 1: " + myComponent);
                                //Debug.Log("Case 2: " + myComponent2);
                                //Debug.Log("Case 3: " + myComponent3);
                            }
                            //Debug.Log(clear);
                            if (clear)
                            {
                                //Begin Equalization
                                GameObject localNeighbor = neighborEmpties[i].GetComponent<VolumeNode>().VolumeLink;
                                if (localNeighbor != null)
                                {
                                    if (limiter <= 0)
                                    {
                                        limiter = 30;
                                        VolumeAtmosphereController iNeighborVolume =
                                        neighborEmpties[i].GetComponent<VolumeNode>().VolumeLink.GetComponent<VolumeAtmosphereController>();
                                        Utils.LocalVolumeEqualizer(this, iNeighborVolume);
                                    }

                                }
                                else
                                {
                                    GameObject globalNeighbor = neighborEmpties[i].GetComponent<VolumeNode>().GlobalLink;
                                    if (globalNeighbor)
                                    {
                                        VolumeGlobalAtmosphereController iGlobalNeighbor =
                                            neighborEmpties[i].GetComponent<VolumeNode>().GlobalLink.GetComponent<VolumeGlobalAtmosphereController>();
                                        Utils.GlobalVolumeEqualizer(this, iGlobalNeighbor);
                                    }
                                }
                            }

                        }
                    }
                    //if (Physics.Raycast(
                    //    new Vector3(door.transform.position.x,
                    //    door.transform.position.y + 0.025f,
                    //    door.transform.position.z),
                    //    back, out RaycastHit hit, 1.0f))//roomVolumeDoors[i].transform.position
                    /// old logic here { }
                }
            }
            ///
            /// Volume Gas Pipe Section Updates
            /// 
            for(int i = 0; i < volumeGasPipeSections.Count; i++)
            {
                List<IGasPipe> sectionList = volumeGasPipeSections[i].GasPipesInSection;
                List<IGasPipe> equalizeList = new List<IGasPipe>();
                for (int j = 0; j < sectionList.Count; j++)
                {
                    //check the status of every pipe - if a pipe is burst, do not equalize it
                    // or the pipes after it
                    if (!sectionList[j].IsBurst)
                    {
                        equalizeList.Add(sectionList[j]);
                    }
                    else
                    {
                        equalizeList.Add(sectionList[j]);
                        break;
                    }
                }

                GasPipeSectionEqualization(equalizeList, true);

                ///
                /// Duct has burst. Begin venting contents into volume.
                /// The contents of the duct after venting must be equal to the ambient atmo.
                /// Ambient atmo will be transfered to connected volumes.
                ///
                List<IGasPipe> ventList = new List<IGasPipe>();
                for (int j = 0; j < sectionList.Count; j++)
                {
                    bool compiled = false;
                    IGasPipe burstPipe = sectionList[j];
                    if (burstPipe.GlobalPressure > burstPipe.MaxPressure)
                    {
                        burstPipe.IsBurst = true;
                    }
                    if (burstPipe.IsBurst)
                    {
                        //Dump contents into volume
                        foreach(IGas gas in burstPipe.Gasses)
                        {
                            AddRoomGas(gas);
                        }
                        
                        // Equalize the duct gasses with the volume
                        float volumeratio = burstPipe.Volume / roomVolume;
                        burstPipe.Gasses.Clear();
                        for (int g = 0; g < roomGases.Count; g++)
                        {
                            IGas gas = roomGases[g];
                            gas.SetLocalPressure(roomPressure);
                            gas.SetConcentration(roomGases[g].GetConcentration() * volumeratio);
                            burstPipe.Gasses.Add(gas);
                        }
                        burstPipe.Temperature = roomTemp;

                        //Transfer these contents to the ducts after the breach.
                        // If the duct is in equalizeList then go in the other direction
                        // Compile this list for only the first burst in the section
                        if (!compiled)
                        {
                            compiled = true;
                            int q = 0;
                            for(int p = 0; p < sectionList.Count; p++)
                            {
                                if (sectionList[p] == burstPipe)
                                {
                                    q = p;
                                }
                            }
                            for(; q < sectionList.Count; q++)
                            {
                                ventList.Add(sectionList[q]);
                            }
                        }
                    }
                }
                if(ventList.Count > 0)
                {
                    GasPipeSectionEqualization(ventList, false);
                }

                //if (temp > tempTol[1] || temp < tempTol[0])
                //{
                //melt and explode
                //    throughput_m3 = 0;//temp
                //}

                //if bulletholes
                //yada yada
            }
            ///Profiler.EndSample();
        }

        public void RoomHeatAmbiLoss()
        {
            float massGas = 0f;
            float averageCp = 0f;
            if (roomGases.Count > 0)
            {
                for (int q = 0; q < RoomGasses.Count; q++)
                {
                    IGas gas = RoomGasses[q];
                    massGas += gas.GetConcentration() * 1000f * gas.GetDensity();
                    averageCp += gas.SpecificHeat;
                }
                averageCp /= RoomGasses.Count;
                float dt = (qHeatLossPerSec / (massGas * averageCp))/RoomGasses.Count;
                ///
                /// This is all in F but we are subtracting K.
                /// As a temporary measure, multiple k dt by 5/9
                dt *= (5f / 9f);
                //Debug.Log(dt * Time.deltaTime);
                for (int q = 0; q < RoomGasses.Count; q++)
                {
                    float temper = RoomGasses[q].GetTemp();
                    temper += (dt * Time.deltaTime);
                    //Debug.Log(temper);
                    RoomGasses[q].SetTemp(temper);
                }
            }
            else
            {
                //without gas to hold heat, temp will drop more quickly.
                float massRoom = (roomVolume) * 6836f;//6.836Kg in 1m3
                float dt = qHeatLossPerSec / (massRoom * 532f);//Cp of .8iron .2aluminum (440 and 900)
                dt *= (5f / 9f);
                roomTemp += dt;
            }
            CalculateRoomTemp();
        }

        public void AddRoomHeat(float heat)
        {
            float massGas = 0f;
            float averageCp = 0f;
            if (roomGases.Count > 0)
            {
                for (int q = 0; q < RoomGasses.Count; q++)
                {
                    IGas gas = RoomGasses[q];
                    massGas += gas.GetConcentration() * 1000f * gas.GetDensity();
                    averageCp += gas.SpecificHeat;
                }
                averageCp /= RoomGasses.Count;
                float dt = (heat / (massGas * averageCp)) / RoomGasses.Count;
                ///
                /// This is all in F but we are subtracting K.
                /// As a temporary measure, multiple k dt by 5/9
                dt *= (5f / 9f);
                //Debug.Log(dt * Time.deltaTime);
                for (int q = 0; q < RoomGasses.Count; q++)
                {
                    float temper = RoomGasses[q].GetTemp();
                    temper += (dt * Time.deltaTime);
                    //Debug.Log(temper);
                    RoomGasses[q].SetTemp(temper);
                }
            }
            CalculateRoomTemp();
        }
        
        private void GasPipeSectionEqualization(List<IGasPipe> equalizeList, bool ventAndTempEq)
        {
            float totalPressures = equalizeList[0].GlobalPressure;
            float totalConc = 0.0f;
            float totalTemp = equalizeList[0].Temperature;
            float totalVelocity = equalizeList[0].FlowVelocity;

            //get total volume, pressure, conc of all gasses in this and neighbors
            foreach (IGas gass in equalizeList[0].Gasses)
            {
                totalConc += gass.GetConcentration();
            }

            // Skip the first duct
            for (int j = 1; j < equalizeList.Count; j++)
            {
                IGasPipe pipe = equalizeList[j];
                if (ventAndTempEq) { 
                    pipe.TempEQWithDuct();
                    if (pipe.Vent != null && pipe.Gasses.Count > 0)
                    {
                        ///
                        /// Maybe make vents (that havn't been breached) one-way? IE air can only flow out into the room. Then, airvents that have
                        /// been kicked or busted out will Eq both ways w/out a throttle (1000L/s or whatev)
                        ///
                        pipe.VentToVolume();
                    }
                }
                totalVelocity += pipe.FlowVelocity;
                totalPressures += pipe.GlobalPressure;
                totalTemp += pipe.Temperature;
                //get total concentration
                foreach (IGas gas in pipe.Gasses)
                {
                    totalConc += gas.GetConcentration();
                }
            }
            //Global Pressure Eq calc
            float tEq_global = totalTemp / (equalizeList.Count);
            float pEq_global = totalPressures / (equalizeList.Count);
            float cEq_global = totalConc / (equalizeList.Count);
            float vEq_global = totalVelocity / (equalizeList.Count);

            // Skip the first duct
            for (int j = 1; j < equalizeList.Count; j++)
            {
                List<IGas> newGassesList = new List<IGas>();
                for (int u = 0; u < equalizeList[0].Gasses.Count; u++)
                {
                    //this gas is the Eq'd gas.
                    IGas tempGas = new IGas(equalizeList[0].Gasses[u].GetIDName(),
                        tEq_global, cEq_global, pEq_global, equalizeList[0].Volume);
                    tempGas.CalculateAtmosphericDensity();
                    newGassesList.Add(tempGas);
                }
                object[] newAtmoComp = { tEq_global, pEq_global, newGassesList };
                //This needs to be limitable by throughput, somehow?
                //first duct TransferTo(other ducts, newAtmoComp)
                equalizeList[0].TransferTo(equalizeList[j], vEq_global, pEq_global, newGassesList, tEq_global);
            }
        }

        /// <summary>
        /// This method is responsable for adjusting the weight of the Deoxygenated VP and the Oxygenated VP. This will be Lerped between 0 and 1 using roomPressure as value.
        /// </summary>
        public void PostProcessVolumeUpdate()
        {
            if (float.IsNaN(roomPressure))
            {
                roomPressure = 0f;
                RoomGasses = new List<IGas>();
                roomTemp = 0f;
                roomOxygenation = 0f;
            }
            Volume[] PPEVS = GetComponents<Volume>();
            for (int i = 0; i < PPEVS.Length; i++)
            {
                if (PPEVS[i].priority == OxygenatedRoom_Priority)
                {
                    float rp = roomPressure;
                    rp = Mathf.Clamp(rp, 0f, 1f);
                    //Debug.Log("ox: "+rp);
                    PPEVS[i].weight = rp;//asserting that roomPressure is between 0 and 1
                }
                else if (PPEVS[i].priority == DeOxygenatedRoom_Priority)
                {
                    float rp = 1 - roomPressure;
                    rp = Mathf.Clamp(rp, 0f, 1f);
                    //Debug.Log("de: " + rp);
                    PPEVS[i].weight = rp;//assume that deoxy is inverse of room pressure
                }
            }
        }

        public List<GameObject> GetConnectedNeighbors()
        {
            return connectedNeighbors;
        }

        public void SetConnectedNeighbors(List<GameObject> newNeighbors)
        {
            connectedNeighbors = newNeighbors;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("_VolumeNode"))
            {
                //Add to list to compare. Whatever exists in VAC is removed from VGAC
                if (!connectedNeighbors.Contains(other.gameObject))
                {
                    //Debug.Log(this.name + " detected VolumeNode: " + other.gameObject.name);
                    connectedNeighbors.Add(other.gameObject);
                    other.GetComponent<VolumeNode>().VolumeLink = this.gameObject;
                }
            }
            if (other.gameObject.CompareTag("Player"))
            {
                PlayerVolumeController player = other.GetComponent<PlayerVolumeController>();
                player.OnVolumeEnter(roomPressure, roomTemp, roomOxygenation);
                player.SetPlayerVolume(this.GetComponents<Volume>());
                player.SetPlayerVolumeController(this);
            }
            else if (other.gameObject.CompareTag("Drone"))
            {
                DroneVolumeController player = other.GetComponent<DroneVolumeController>();
                player.OnVolumeEnter(roomPressure, roomTemp, roomOxygenation);
                player.SetPlayerVolume(this.GetComponents<Volume>());
                player.SetPlayerVolumeController(this);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                other.GetComponent<PlayerVolumeController>().ResetPlayerVolumeController(this);
            }
            else if (other.gameObject.CompareTag("Drone"))
            {
                other.GetComponent<DroneVolumeController>().ResetPlayerVolumeController(this);
            }
        }

        public List<IGas> CheckGasses(bool setToLocalPressure, float localPressure)
        {
            if (roomGases.Count > 1)
            {
                //Debug.Log(roomGases.Count + " gasses in pipe.");
                List<IGas> newGassesList = roomGases;//new List<IGas>();
                                                     //combine all same gasses
                for (int i = 0; i < newGassesList.Count; i++)
                {
                    for (int j = 0; j < newGassesList.Count; j++)
                    {
                        if (i != j)
                        {
                            if (newGassesList[i].GetIDName() == newGassesList[j].GetIDName())
                            {
                                IGas EQgas = CombineGases(roomGases[i], roomGases[j], localPressure, setToLocalPressure);
                                newGassesList.Remove(roomGases[i]);
                                newGassesList.Remove(roomGases[j - 1]);
                                newGassesList.Add(EQgas);
                            }
                        }
                    }
                }
                foreach (IGas gas in newGassesList)
                {
                    //Debug.Log("EQM result: "+gas.ToString());
                }
                return newGassesList;
            }
            else
            {
                //gasses is empty or only has one has in it
                return roomGases;
            }
        }
        public List<IFluid> CheckFluids(bool setToLocalPressure, float localPressure)
        {
            if (roomFluids.Count > 1)
            {
                //Debug.Log(roomGases.Count + " gasses in pipe.");
                List<IFluid> newFluidsList = roomFluids;//new List<IGas>();
                                                        //foreach(IFluid fluid in roomFluids)
                                                        //{
                                                        //    Debug.Log(fluid);
                                                        //}
                                                        //combine all same gasses
                for (int i = 0; i < newFluidsList.Count; i++)
                {
                    for (int j = 0; j < newFluidsList.Count; j++)
                    {
                        if (i != j)
                        {
                            if (newFluidsList[i].GetIDName() == newFluidsList[j].GetIDName())
                            {
                                //Debug.Log("i "+newFluidsList[i]);
                                //Debug.Log("j "+newFluidsList[j]);
                                IFluid EQFluid = CombineFluids(roomFluids[i], roomFluids[j], localPressure, setToLocalPressure);
                                Debug.Log("EQFluid " + EQFluid);
                                newFluidsList.Remove(roomFluids[i]);
                                newFluidsList.Remove(roomFluids[j - 1]);//?
                                newFluidsList.Add(EQFluid);
                            }
                        }
                    }
                }
                return newFluidsList;
            }
            else
            {
                //gasses is empty or only has one has in it
                return roomFluids;
            }
        }
        public IGas CombineGases(IGas gasA, IGas gasB, float localPressure, bool setToLocalPressure)
        {
            float gasTemp;
            float gasConc;
            float gasPressure;

            float gasAt = gasA.GetTemp();
            float gasBt = gasB.GetTemp();
            gasTemp = (gasAt + gasBt) / 2;
            gasA.SetTemp(gasTemp);

            gasConc = gasA.GetConcentration() + gasB.GetConcentration();
            gasA.SetConcentration(gasConc);

            if (setToLocalPressure)
            {
                gasA.SetLocalPressure(localPressure);
            }
            else
            {
                float gasAp = gasA.GetLocalPressure();
                float gasBp = gasB.GetLocalPressure();
                gasPressure = (gasAp + gasBp);
                gasA.SetLocalPressure(gasPressure);
            }

            gasA.CalculateAtmosphericDensity();
            //Debug.Log("Volume Gas Combiner: "+gasPressure);
            return gasA;
        }

        public IFluid CombineFluids(IFluid FluidA, IFluid FluidB, float localPressure, bool setToLocalPressure)
        {
            float FluidTemp;
            float FluidConc;
            float FluidPressure;

            float FluidAt = FluidA.GetTemp();
            float FluidBt = FluidB.GetTemp();
            FluidTemp = (FluidAt + FluidBt) / 2;
            FluidA.SetTemp(FluidTemp);

            FluidConc = FluidA.GetConcentration() + FluidB.GetConcentration();
            //Debug.Log("Fluid conc:" + FluidA.GetConcentration() +" " + FluidB.GetConcentration()+": "+FluidConc);
            FluidA.SetConcentration(FluidConc);

            if (setToLocalPressure)
            {
                FluidA.SetLocalPressure(localPressure);
            }
            else
            {
                float gasAp = FluidA.GetLocalPressure();
                float gasBp = FluidB.GetLocalPressure();
                FluidPressure = (gasAp + gasBp);
                FluidA.SetLocalPressure(FluidPressure);
            }

            //FluidA.CalculateAtmosphericDensity();
            //Debug.Log("Volume Gas Combiner: "+gasPressure);
            return FluidA;
        }
        
        public void RemoveRoomGas(IGas gasToRemove)
        {
            IGas gas = new IGas(gasToRemove);
            if (roomGases.Count > 0)
            {
                for (int j = 0; j < roomGases.Count; j++)
                {
                    if (roomGases[j].GetIDName() == gas.GetIDName())
                    {
                        float nVal = roomGases[j].GetConcentration() - gas.GetConcentration();
                        if (nVal > 0f)
                        {
                            roomGases[j].SetConcentration(nVal);
                        }
                        else
                        {
                            //may not work?
                            roomGases.Remove(gas);
                        }
                    }
                }
            }
            //update Volume Atmosphere
            float totalGas = CalculateRoomOxygenation();
            CalculateRoomTemp();
            CalculateRoomPressure(totalGas);
        }

        public void RemoveRoomGas(float pressureToRemove)
        {
            //convert pressure to concentration
            float runConc = 0f;
            foreach(IGas gas in roomGases)
            {
                runConc += gas.GetConcentration();
            }
            
            float conc = ((roomPressure - pressureToRemove) * runConc) / roomPressure;
            float remove = runConc - conc;
            Debug.Log("remove gas conc: " + remove);
            
            //remove that conc from the first available gas
            for (int j = 0; j < roomGases.Count; j++)
            {
                if(remove > 0f)
                {
                    if (remove - roomGases[j].GetConcentration() > 0f)
                    {
                        remove -= roomGases[j].GetConcentration();
                        roomGases[j].SetConcentration(0f);
                    }
                    else
                    {
                        roomGases[j].SetConcentration(roomGases[j].GetConcentration() - remove);
                        remove = 0f;
                    }
                }
                else
                {
                    break;
                } 
            }
            
            //update Volume Atmosphere
            float totalGas = CalculateRoomOxygenation();
            CalculateRoomTemp();
            CalculateRoomPressure(totalGas);
        }

        /// <summary>
        /// Add the parameter gas to the room's gas array. Update Volume Atmosphere.
        /// </summary>
        /// <param name="gasToAdd"></param>
        public void AddRoomGas(IGas gasToAdd)
        {
            bool add = true;
            IGas gas = new IGas(gasToAdd);
            gasToAdd.SetLocalVolume(roomVolume);
            if (roomGases.Count > 0)
            {
                for (int j = 0; j < roomGases.Count; j++)
                {
                    if (roomGases[j].GetIDName() == gas.GetIDName())
                    {
                        roomGases[j] = CombineGases(roomGases[j], gas, Pressure, false);
                        add = false;
                    }
                }
                if (add)
                {
                    //Debug.Log("Add");
                    roomGases.Add(gas);
                }
            }
            else
            {
                //Debug.Log("Add");
                roomGases.Add(gas);
            }
            //update Volume Atmosphere
            float totalGas = CalculateRoomOxygenation();
            CalculateRoomTemp();
            CalculateRoomPressure(totalGas);
        }

        public void AddRoomFluid(IFluid fluidToAdd)
        {
            bool add = false;
            IFluid fluid = new IFluid(fluidToAdd);
            fluidToAdd.SetLocalVolume(roomVolume);
            //If the roomFluids list is empty or does not contain the passed fluid
            if (roomFluids.Count > 0)
            {
                for (int j = 0; j < roomFluids.Count; j++)
                {
                    if (roomFluids[j].GetIDName() == fluid.GetIDName())
                    {
                        add = false;
                    }
                    else
                    {
                        add = true;
                    }
                }
                if (add)
                {
                    roomFluids.Add(fluid);
                }
            }
            else
            {
                roomFluids.Add(fluid);
            }
            //Combine the passed fluid with fluids already in volume
            for (int j = 0; j < roomFluids.Count; j++)
            {
                if (roomFluids[j].GetIDName() == fluid.GetIDName())
                {
                    IFluid EQFluid = CombineFluids(roomFluids[j], fluid, roomPressure, true);
                    //Debug.Log("EQFluid " + EQFluid);
                    roomFluids.Remove(roomFluids[j]);
                    roomFluids.Add(EQFluid);
                }
            }
            UpdateRoomFluidLevel();
        }

        public void AddRoomFluid(List<IFluid> fluidToAdd)
        {
            bool add = false;
            for(int f = 0; f < fluidToAdd.Count; f++)
            {
                IFluid fluid = new IFluid(fluidToAdd[f]);
                fluidToAdd[f].SetLocalVolume(roomVolume);
                //If the roomFluids list is empty or does not contain the passed fluid
                if (roomFluids.Count > 0)
                {
                    //Combine the passed fluid with fluids already in volume
                    for (int j = 0; j < roomFluids.Count; j++)
                    {
                        if (roomFluids[j].GetIDName() == fluid.GetIDName())
                        {
                            IFluid EQFluid = CombineFluids(roomFluids[j], fluid, roomPressure, true);
                            //Debug.Log("EQFluid " + EQFluid);
                            roomFluids.Remove(roomFluids[j]);
                            roomFluids.Add(EQFluid);
                        }
                    }
                }
                else
                {
                    roomFluids.Add(fluid);
                }
            }
            UpdateRoomFluidLevel();
        }

        /// <summary>
        /// recalc room oxygenation. It is the ratio of oxygen (in m3) to room volume (m3).
        /// Gasses that are not oxygen do not count towards oxygenation.
        /// Returns: The total amount of gasses in the room in m3
        /// </summary>
        public float CalculateRoomOxygenation()
        {
            float oxygenation = 0.0f;
            float totalGasses = 0.0f;
            for (int i = 0; i < roomGases.Count; i++)//gasesToEq
            {
                totalGasses += roomGases[i].GetConcentration();//gasesToEq
                if (roomGases[i].GetIDName() == "Oxygen")
                {
                    oxygenation += roomGases[i].GetConcentration();//gasesToEq
                }
            }
            //Debug.Log("Gasses: "+ totalGasses + " in " + this.name);
            //the above calcs are full oxygenation at 1.0, not 100.0f, so mult by 100
            roomOxygenation = (oxygenation /= roomVolume) * 100f;
            //float oxygenTemp = (oxygenation /= roomVolume)*100f;
            //roomOxygenation += oxygenTemp;
            return totalGasses;
        }

        /// <summary>
        /// Calculate room temp based on the gasses present in the room, and it's ambient heat.
        /// Returns the room temp before temp Eq.
        /// </summary>
        public void CalculateRoomTemp()
        {
            if (roomGases.Count > 0f)
            {
                float temperature = 0.0f;
                for (int j = 0; j < roomGases.Count; j++)
                {
                    temperature += roomGases[j].GetTemp();
                }
                //Equalized temp of the gases in the room
                roomTemp = temperature / (roomGases.Count);
            }
        }

        /// <summary>
        /// Recalc room pressure based on the concentration of gas present in the volume, and it's temperature.
        /// </summary>
        public void CalculateRoomPressure(float totalRoomGasses_m3)
        {
            ///calculate pressure in the room according to the gas vars.
            ///IE The amount of gasses in the room where >x is 1.x atm and less than x is 0.x atm
            ///THEN adjust for temp

            float concPressure = totalRoomGasses_m3 / roomVolume;
            //t1 = originalTemp; t2 = roomTemp
            //p1 = roomPressure; p2 = X
            //v1 = roomVolume; v2 = roomVolume
            float p2 = 0.0f;
            foreach (IGas gas in roomGases)
            {
                float p1 = concPressure;
                float v1 = gas.GetLocalVolume();
                float t1 = ((gas.GetTemp() - 32f) * (5f / 9f)) + 273.15f;
                float v2 = gas.GetLocalVolume();
                float t2 = ((roomTemp - 32f) * (5f / 9f)) + 273.15f;
                //Debug.Log("p1: " +p1 + " v1: " +v1+ " t1: " +t1 + " v2: " +v2+ " t2: "+t2);
                //temp-adjusted pressure for one gas in the room
                p2 += (p1 * v1 * t2) / (t1 * v2);
            }
            roomPressure = p2 / roomGases.Count;
            //Debug.Log("Pressure recal: " + roomPressure);
            foreach (IGas setGases in roomGases)
            {
                setGases.SetLocalPressure(roomPressure);
                setGases.SetTemp(roomTemp);
                setGases.CalculateAtmosphericDensity();
            }
            roomGases = CheckGasses(true, roomPressure);
        }

        public void UpdateRoomFluidLevel()
        {
            //EXTREMELY temporary
            //float fluidLevel = 0.0f;
            float fluidConc = 0.0f;
            //Debug.Log(roomFluids.Count);
            for (int i = 0; i < roomFluids.Count; i++)
            {
                float volumeRatio = (roomFluids[i].GetConcentration()) / roomVolume;//1000L in 1 m^3 conc/1000
                fluidConc += roomFluids[i].GetConcentration();
                //Debug.Log(roomFluids[i].GetConcentration() + "/" + roomVolume);
                float translatedFill = volumeRatio * gameObject.GetComponent<BoxCollider>().size.y;
                roomFluidPlanes[0].transform.localPosition = new Vector3(
                    roomFluidPlanes[0].transform.localPosition.x,
                    translatedFill,// + fluidLevel,
                    roomFluidPlanes[0].transform.localPosition.z);
                //fluidLevel += translatedFill;
            }
            //Debug.Log(fluidConc + "/" + roomVolume);
        }

        public float GetPressure()
        {
            return roomPressure;
        }
        public void SetPressure(float value)
        {
            roomPressure = value;
        }
        public float GetVolume()
        {
            return roomVolume;
        }
    }
}