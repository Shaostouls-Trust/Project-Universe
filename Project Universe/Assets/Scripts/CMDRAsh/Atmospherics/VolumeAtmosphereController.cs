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
using ProjectUniverse.Ship;
using ProjectUniverse.PowerSystem;

namespace ProjectUniverse.Environment.Volumes
{
    //[RequireComponent(typeof(VolumeComponent))]
    public sealed class VolumeAtmosphereController : MonoBehaviour
    {
        private float roomPressure;
        [SerializeField] private float roomTemp;//rooms cool to -200f over time, without heating
        [SerializeField] private float roomOxygenation;
        [SerializeField] private float roomVolume;
        [SerializeField] private float humidity;
        [SerializeField] private float toxicity;//gasses and stuff
        [SerializeField] private float contamination;//radioactive particles
        /// <summary>
        /// Order roomGases into a defined array for ease of access?
        /// </summary>
        private List<IGas> roomGases = new List<IGas>();
        private List<IGas> gasesToEq = new List<IGas>();
        private List<IFluid> roomFluids = new List<IFluid>();
        [Tooltip("Fluid planes in order of lowest to highest.")]
        [SerializeField] private List<GameObject> roomFluidPlanes = new List<GameObject>();
        [Tooltip("The y level at which the next fluid plane will begin to rise.")]
        [SerializeField] private float[] roomFluidPlaneLevelLimits;
        [Tooltip("The y level after which fluid can pass through this door.")]
        [SerializeField] private float[] roomFluidPlaneDoorLevels;
        [Tooltip("All doors in volume in order of their above plane level.")]
        [SerializeField] private DoorAnimator[] roomDoorsFluidOrder;
        [SerializeField] private GameObject[] neighborEmpties;
        //All volumes that define the shape of the room.
        [SerializeField] private List<BoxCollider> roomVolumeSections;
        //[SerializeField] private GameObject[] roomVolumeDoors;
        private List<GameObject> connectedNeighbors = new List<GameObject>();
        [SerializeField] private int OxygenatedRoom_Priority = 10;
        [SerializeField] private int DeOxygenatedRoom_Priority = 9;
        [SerializeField] public List<PipeSection> volumeGasPipeSections;
        [SerializeField] private bool autoFill;
        [SerializeField] private bool flood;
        public int limiter = 30;
        private float qHeatLossPerSec = 0f;//-40000
        public bool doRenderStateLogic = true;
        //[SerializeField] private bool renderstate = false;
        private RenderStateManager rsm;
        private bool rendersOn = true;
        private MeshRenderer[] volumeRenders;
        private GameObject[] lightGOs;
        private List<GameObject> GOculls;
        [SerializeField] private GameObject[] lightGroups;
        [Tooltip("Volumes/objects that should render while in this volume. Useful for windows or large doors.")]
        [SerializeField] private GameObject[] additionalRenderVolumes;
        private FrustrumState[] frustrumStates;
        private GameObject[] pipes;
        private float maxFluidFillHeight;
        private float roomFloorArea;
        [SerializeField] private VolumeAtmosphereJobs VAJobs;

        private void Start()
        {
            if (autoFill)
            {
                AddRoomGas(new IGas("Oxygen", 60f, roomVolume, 1.0f, roomVolume));
            }
            if(roomFluids.Count == 0)
            {
                for(int j = 0; j < roomFluidPlanes.Count; j++)
                {
                    roomFluidPlanes[j].SetActive(false);
                }
            }
        }


        //was all in Start
        private void Awake()
        {
            //subscribe to VA Jobs manager
            if (VAJobs != null)
            {
                VAJobs.AddVolume(this);
            }
            else
            {
                Debug.LogError("Volume Atmosphere Not Subscribed to Update System!");
            }
            //room volume and fill height
            float maxy = 0;
            for (int i = 0; i < roomVolumeSections.Count; i++)
            {
                roomVolume += (roomVolumeSections[i].size.x * roomVolumeSections[i].size.y * roomVolumeSections[i].size.z);
                roomFloorArea += (roomVolumeSections[i].size.x * roomVolumeSections[i].size.z);
                if (roomVolumeSections[i].size.y > maxy)
                {
                    maxy = roomVolumeSections[i].size.y;
                }
            }
            maxFluidFillHeight = maxy;

            ///
            /// This does not ensure that we are only losing heat from the outer sides of the volume,
            /// but from all sides of each section of the volume, which is not a valid assumption.
            ///
            //qHeatLossPerSec = ((gameObject.GetComponent<BoxCollider>().size.x * 2f) +
            //    (gameObject.GetComponent<BoxCollider>().size.y * 2f) +
            //    (gameObject.GetComponent<BoxCollider>().size.z * 2f)) * -40000f;//-1240000J per 51300L 19-3-9 rm
            for (int j = 0; j < roomVolumeSections.Count; j++)
            {
                //rooomVolume += (etc)
                qHeatLossPerSec += ((roomVolumeSections[j].size.x * 2f) + (roomVolumeSections[j].size.y * 2f) +
                (roomVolumeSections[j].size.z * 2f)) * -400f;//40000f
            }

            if (doRenderStateLogic)
            {
                //Debug.Log("- - - - -");
                rsm = GetComponentInParent<RenderStateManager>();
                //Debug.Log("Room RSM: " + rsm);
                ///Get all top-level renderers and light objects in this volume
                ///
                List<GameObject> lightList = new List<GameObject>();
                for (int i = 0; i < lightGroups.Length; i++)
                {
                    Light[] lights = lightGroups[i].GetComponentsInChildren<Light>(false);
                    for (int l = 0; l < lights.Length; l++)
                    {
                        //Debug.Log(lights[l]);
                        lightList.Add(lights[l].gameObject);
                        //set the intensity to 0 to preempt lighting bugs when there is no power on first volume activation.
                        //if (lights[l].transform.parent.TryGetComponent(out ISubMachine _))
                        //{
                        //    lights[l].intensity = 0f;
                        //    lights[l].enabled = false;
                        //}

                    }
                }

                List<MeshRenderer> renderlist = new List<MeshRenderer>();
                List<Transform> carryOver = new List<Transform>();
                lightGOs = lightList.ToArray();
                //get all 1st level children
                GOculls = new List<GameObject>();
                List<FrustrumState> fsList = new List<FrustrumState>();
                foreach (Transform child in transform)
                {
                    //Debug.Log(child);
                    if (child.gameObject.activeInHierarchy)
                    {
                        if (child.TryGetComponent(out MeshRenderer render))
                        {
                            renderlist.Add(render);
                            if (render.gameObject.TryGetComponent(out FrustrumState fs))
                            {
                                fsList.Add(fs);
                            }
                            //There are gameobjects hidden under the combined stacks that were kept separate
                            //from the rest of the merged tiles that are not LODs. The following is intended
                            //to grab them and add them to the LOD/GO cull stack.
                            foreach(Transform obj in child)
                            {
                                if (!obj.TryGetComponent(out MeshRenderer r))
                                {
                                    GOculls.Add(obj.gameObject);
                                }
                            }
                        }
                        else if(child.gameObject.tag == "IBreaker")
                        {
                            foreach (Transform obj in child.transform)
                            {
                                if(obj.tag == "_root")
                                {
                                    //Debug.Log(obj.gameObject);
                                    GOculls.Add(obj.gameObject);
                                }
                            }
                        }
                        else
                        {
                            //Debug.Log(">> " + child.name);
                            carryOver.Add(child);
                        }
                    }
                }
                frustrumStates = fsList.ToArray();
                //Debug.Log(gameObject.name + " A has " + renderlist.Count);
                //Get all child renderers of the carryover stack
                foreach (Transform carry in carryOver)
                {
                    if (carry.gameObject.tag != "GUI" && carry.gameObject.tag != "Pipe")
                    {
                        MeshRenderer[] renderchilds = carry.GetComponentsInChildren<MeshRenderer>(false);
                        //Debug.Log(renderchilds.Length);
                        for (int r = 0; r < renderchilds.Length; r++)
                        {
                            if (renderchilds[r].gameObject.tag != "GUI" && renderchilds[r].gameObject.tag != "Button3D")
                            {
                                renderlist.Add(renderchilds[r]);
                                //Debug.Log("+ " + renderchilds[r]);
                            }
                        }
                    }
                }
                volumeRenders = renderlist.ToArray();
                LODGroup[] lodObjs = transform.GetComponentsInChildren<LODGroup>();
                Canvas[] canvasObj = transform.GetComponentsInChildren<Canvas>();
                List<GameObject> pipesGOs = new List<GameObject>();
                for(int j = 0; j < lodObjs.Length; j++)
                {
                    //if not breaker, router, subst, gen, or anything else
                    //that has/might have a script or lod in the future
                    if (lodObjs[j].tag == "Untagged")
                    {
                        GOculls.Add(lodObjs[j].gameObject);
                    }
                    else if (lodObjs[j].tag == "Pipe")
                    {
                        pipesGOs.Add(lodObjs[j].gameObject);
                    }
                }
                for(int c = 0; c < canvasObj.Length; c++)
                {
                    GOculls.Add(canvasObj[c].gameObject);
                }
                pipes = pipesGOs.ToArray();
            }
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
        public float Contamination
        {
            get { return contamination; }
            set { contamination = value; }
        }
        public List<IGas> RoomGasses
        {
            get { return roomGases; }
            set { roomGases = value; }
        }
        public bool RenderEnabled
        {
            get { return rendersOn; }
            set { rendersOn = value; }
        }
        public GameObject[] GetNeighborEmpties
        {
            get { return neighborEmpties; }
        }

        public FrustrumState[] GetFrustrumStates
        {
            get { return frustrumStates; }
        }

        public GameObject[] AdditionalRenderVolumes
        {
            get { return additionalRenderVolumes; }
        }

        public RenderStateManager RSM
        {
            get { return rsm; }
        }

        public GameObject[] LightGameObjects
        {
            get { return lightGOs; }
        }

        public GameObject[] NeighborEmpties
        {
            get { return neighborEmpties; }
        }

        public List<PipeSection> VolumeGasPipeSections
        {
            get { return volumeGasPipeSections; }
        }

        public bool Flood
        {
            get { return flood; }
        }

        public float RoomVolume
        {
            get { return roomVolume; }
        }

        public float WaterLevel(bool useLocal)
        {
            if (useLocal)
            {
                return roomFluidPlanes[0].transform.localPosition.y;
            }
            else
            {
                return roomFluidPlanes[0].transform.position.y;        
            }
        }

        public DoorAnimator[] RoomDoorsFluidOrder
        {
            get { return roomDoorsFluidOrder; }
        }
        public float[] RoomFluidPlaneLevels
        {
            get { return roomFluidPlaneDoorLevels; }
        }
        public float RoomHeight
        {
            get { return maxFluidFillHeight; }
        }
        public List<IFluid> RoomFluids
        {
            get { return roomFluids; }
        }
        public float RoomArea
        {
            get { return roomFloorArea; }
        }

        ///<summary>
        /// check doors
        /// if two doors are open
        ///check the two volumes
        ///go through equalization. 
        /// </summary>
        /// 
        /// Use FIXEDUPDATE to spin up a worker thread to handle the update logic so that the main thread
        /// can continue on?


        void FixedUpdate()
        {
            ///
            /// Room temp will slowly drop to -200f over time without the addition of heat through radiators.
            /// Radiators will heat the room according to how open the radiator valve is.
            /// Larger rooms heat and cool more slowly b/c room gasses will must heat and cool as well.
            /// 
            //RoomHeatAmbiLoss();
            //there is no point running this until reactor radiators are set up for the ship

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
            //intended to be temp
            /*if (flood)
            {
                IFluid tWat = new IFluid("water", 80f, 0.2f);
                AddRoomFluid(tWat);
                //render control is not showing/hiding water plane
                //hall to control is not hiding water
            }
            */
            for (int i = 0; i < neighborEmpties.Length; i++)//roomVolumeDoors.Length
            {
                GameObject door = neighborEmpties[i].GetComponent<VolumeNode>().GetDoor();
                //Debug.Log(door);
                //if the door (in this volume) is open
                if (door.GetComponent<DoorAnimator>().OpenOrOpening())//roomVolumeDoors
                {
                    Vector3 back = door.transform.TransformDirection(Vector3.back);//is Vector3.back for all cases?
                    //raycastAll to check neighbor door. Might need 3, not two indicies.
                    RaycastHit[] hits = new RaycastHit[2];
                    Physics.RaycastNonAlloc(new Vector3(door.transform.position.x,
                        door.transform.position.y + 0.025f, door.transform.position.z), back, hits, 1.0f);
                    //hits = Physics.RaycastAll(new Vector3(door.transform.position.x,
                    //    door.transform.position.y + 0.025f, door.transform.position.z), back, 1.0f);
                    GameObject myDoorGameobject;
                    foreach (RaycastHit hit in hits)
                    {
                        myDoorGameobject = null;
                        if (hit.collider.gameObject != door)
                        {
                            //check if it's a door
                            //select the parent object via the DoorAnimator
                            bool clear = false;
                            Component myComponent = hit.collider.GetComponentInParent<DoorAnimator>();
                            Component myComponent2 = hit.collider.GetComponent<DoorAnimator>();
                            Component myComponent3 = hit.collider.GetComponentInChildren<DoorAnimator>();
                            //Debug.Log(hit.collider.gameObject);
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

                            if (myDoorGameobject != null)
                            {
                                clear = myDoorGameobject.GetComponent<DoorAnimator>().OpenOrOpening();
                            }
                            else
                            {
                                clear = true;//no door on other side, it is what it is when this door opens.
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
                                        //Debug.Log("Eq "+ this + ""+ iNeighborVolume);
                                        Utils.LocalVolumeEqualizer(this, iNeighborVolume);
                                        //Fluid Equalization
                                        if (myDoorGameobject != null)
                                        {
                                            Utils.LocalFluidEqualization(this, iNeighborVolume,
                                                door.GetComponent<DoorAnimator>(), myDoorGameobject.GetComponent<DoorAnimator>());
                                        }
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
                                        //Fluid Drain into void
                                        Utils.LocalFluidDrain(this, -1f, null);
                                    }
                                }

                            }

                        }
                    }
                }
            }
            /*
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
        */
        }

            /// <summary>
            /// cools extremely quickly to -330 once down near zero.
            /// Function of density?
            /// </summary>
            public void RoomHeatAmbiLoss()
        {
            if (roomTemp > -330f)
            {
                float massGas = 0f;
                float averageCp = 0f;
                if (roomGases.Count > 0)
                {
                    for (int q = 0; q < RoomGasses.Count; q++)
                    {
                        IGas gas = RoomGasses[q];
                        massGas += gas.GetConcentration() * gas.GetSTPDensity();
                        averageCp += gas.SpecificHeat;
                    }
                    averageCp /= RoomGasses.Count;
                    float dt = (qHeatLossPerSec / (massGas * averageCp)) / RoomGasses.Count;
                    ///
                    /// This is all in F but we are subtracting K.
                    /// As a temporary measure, multiple k dt by 5/9
                    dt *= (5f / 9f);
                    //Debug.Log(dt * Time.deltaTime);
                    for (int q = 0; q < RoomGasses.Count; q++)
                    {
                        float temper = RoomGasses[q].GetTemp();//temper does not cange over time wit te volume temp?
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
                    //Debug.Log("+="+dt);
                    //roomTemp += dt;
                }

                CalculateRoomTemp();
            }
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
                    massGas += gas.GetConcentration() * gas.GetDensity();
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
        
        public void GasPipeSectionEqualization(List<IGasPipe> equalizeList, bool ventAndTempEq)
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
            if (ventAndTempEq)
            {
                equalizeList[0].TempEQWithDuct();
                //GameObject null checks are main thread only *sigh*
                if (equalizeList[0].HasVent && equalizeList[0].Gasses.Count > 0)
                {
                    equalizeList[0].VentToVolume();
                }
            }

            // Skip the first duct
            for (int j = 1; j < equalizeList.Count; j++)
            {
                IGasPipe pipe = equalizeList[j];
                if (ventAndTempEq) { 
                    pipe.TempEQWithDuct();
                    if (pipe.HasVent && pipe.Gasses.Count > 0)
                    {
                        ///
                        /// Maybe make vents (that havn't been breached) one-way? 
                        /// IE air can only flow out into the room. Then, airvents that have
                        /// been kicked or busted out will Eq both ways w/out a throttle (1000L/s or whatev)
                        ///
                        pipe.VentToVolume();
                    }
                }
                totalVelocity += pipe.FlowVelocity;
                if (!float.IsNaN(pipe.GlobalPressure))
                {
                     totalPressures += pipe.GlobalPressure;
                }
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
            //if(totalPressures != 0)
            //{
            //    Debug.Log("total: "+totalPressures);
            //}
            //if (equalizeList[0].GlobalPressure != 0)
            //{ 
            //    Debug.Log("global: "+equalizeList[0].GlobalPressure + " over " + equalizeList.Count);
            //}
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
                //object[] newAtmoComp = { tEq_global, pEq_global, newGassesList };
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
                //toggle renderstates
                if(rsm != null)
                {
                    rsm.CurrentRoom = this.gameObject;
                }
                if (!rendersOn)
                {
                    Debug.Log("Render " + gameObject);
                    ShowRenderVolume();
                }
                //renderstate = true;
            }
            else if (other.gameObject.CompareTag("Drone"))
            {
                //Debug.Log("entered "+this.name);
                DroneVolumeController player = other.GetComponent<DroneVolumeController>();
                player.OnVolumeEnter(roomPressure, roomTemp, roomOxygenation);
                player.SetPlayerVolume(this.GetComponents<Volume>());
                player.SetPlayerVolumeController(this);
                //toggle renderstates (only render the immediate volume, but two volume's worth
                //of ducts?)
                //renderstate = true;
                if (rsm != null)
                {
                    rsm.CurrentRoom = this.gameObject;
                }
                if (!rendersOn)
                {
                    ShowRenderVolume();
                }
            }
        }

        /// <summary>
        /// This is the source of the rsm/atmo bug, most likely. When the player exits a volume, if they haven't
        /// entered another (or even if they have), it's setting to null for a sec.
        /// Need a better determination method for exiting a volume into the global atmosphere control space.
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerExit(Collider other)
        {
            /**/
            StartCoroutine(WaitForVolumeUpdate(other));
        }

        public IEnumerator WaitForVolumeUpdate(Collider other)
        {
            yield return null;
            if(other.TryGetComponent(out PlayerVolumeController player))
            {
                //if the player's volume is still this after 'exited', it's likely we've left the ship.              
                player.ResetPlayerVolumeController(this);
            }
            else if (other.TryGetComponent(out DroneVolumeController drone))
            {
                drone.ResetPlayerVolumeController(this);
            }
        }

        /// <summary>
        /// Render State Manager sends a list of the render state collision planes that were determined to be visible.
        /// Use the Frustrum States attached to the VAC to disable or enable the appropriate mesh renderers.
        /// </summary>
        /// <param name="rsmp"></param>
        public void ReceiveActiveFrustrumPlanes(List<MeshCollider> rsmp)
        {
            //disable all renderstate renderers in preparation for the below checks.
            for (int q = 0; q < frustrumStates.Length; q++)
            {
                frustrumStates[q].gameObject.GetComponent<MeshRenderer>().enabled = false;
                frustrumStates[q].HidByOccluder = true;
                frustrumStates[q].visibleInFrustrum = true;//keep checking this section
            }
            //enable all visible renderers
            for (int i = 0; i < frustrumStates.Length; i++)
            {
                for(int j = 0; j < frustrumStates[i].RenderStatePlanes.Length; j++)
                {
                    //if the active collider list contains the state plane, then the gameobject
                    //attached to the frustrum state is visible (and should be rendered)
                    if (rsmp.Contains(frustrumStates[i].RenderStatePlanes[j]))
                    {
                        frustrumStates[i].gameObject.GetComponent<MeshRenderer>().enabled = true;
                        frustrumStates[i].HidByOccluder = false;
                        frustrumStates[i].visibleInFrustrum = true;//implied
                    }
                }
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
                //foreach (IGas gas in newGassesList)
                //{
                    //Debug.Log("EQM result: "+gas.ToString());
                //}
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
                List<IFluid> newFluidsList = roomFluids;
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
                                //Debug.Log("EQFluid " + EQFluid);
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

            ///Use Cp?
            float gasAt = gasA.GetTemp();
            float gasBt = gasB.GetTemp();
            gasTemp = (gasAt + gasBt) / 2;
            //Debug.Log("set temp");
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
                FluidPressure = (gasAp + gasBp)/2f;// /2?
                FluidA.SetLocalPressure(FluidPressure);
            }

            float dens = FluidA.GetDensity();
            dens += FluidB.GetDensity();
            FluidA.SetDensity(dens / 2f);
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
            //bool add = false;
            IFluid fluid = new IFluid(fluidToAdd);
            roomFluids.Add(fluid);
            roomFluids = CheckFluids(true, Pressure);
            UpdateRoomFluidLevel();
        }

        public List<IFluid> RemoveRoomFluid(float amount)
        {
            if (roomFluids.Count > 0)
            {
                List<IFluid> fluidsRemoved = new List<IFluid>();
                float per = amount / roomFluids.Count;
                for (int j = 0; j < roomFluids.Count; j++)
                {
                    fluidsRemoved.Add(new IFluid(roomFluids[j]));

                    if (roomFluids[j].GetConcentration() < per)
                    {
                        fluidsRemoved[j].SetConcentration(roomFluids[j].GetConcentration());
                        roomFluids[j].SetConcentration(0f);
                    }
                    else
                    {
                        fluidsRemoved[j].SetConcentration(per);
                        roomFluids[j].AddConcentration(-per);
                    }
                }
                return fluidsRemoved;
            }
            else
            {
                //protect against ArgNullException
                return new List<IFluid>();
            }

        }

        public void AddRoomFluid(List<IFluid> fluidToAdd)
        {
            bool add = false;
            for(int f = 0; f < fluidToAdd.Count; f++)
            {
                IFluid fluid = new IFluid(fluidToAdd[f]);
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
            //Debug.Log("Recalc");
            if (roomGases.Count > 0f)
            {                
                float temperature = 0.0f;
                float totalconc = 0f;
                //scale by volume
                for (int j = 0; j < roomGases.Count; j++)
                {
                    totalconc += roomGases[j].GetConcentration();
                }
                for (int j = 0; j < roomGases.Count; j++)
                {
                    //Debug.Log(roomGases[j].GetConcentration() / totalconc);
                    temperature += (roomGases[j].GetTemp() * (roomGases[j].GetConcentration()/totalconc));
                    //Debug.Log(temperature);
                }
                //Equalized temp of the gases in the room
                //Debug.Log(roomTemp+"=>"+ (temperature / (roomGases.Count)));
                roomTemp = temperature;// / (roomGases.Count);
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
                //Debug.Log("Set");
                setGases.SetTemp(roomTemp);
                setGases.CalculateAtmosphericDensity();
            }
            roomGases = CheckGasses(true, roomPressure);
        }

        public void UpdateRoomFluidLevel()
        {
            float fluidConc = 0.0f;
            //Debug.Log(roomFluids.Count);  
            for (int i = 0; i < roomFluids.Count; i++)
            {
                fluidConc += roomFluids[i].GetConcentration();
            }
            if(fluidConc <= 0.1f)
            {
                //set all planes false, as the room has emptied
                for (int f = 0; f < roomFluidPlanes.Count; f++)
                {
                    roomFluidPlanes[f].SetActive(false);
                }
            }

            float volumeRatio = fluidConc / roomVolume;
            //volumeRatio * gameObject.GetComponent<BoxCollider>().size.y)
            float translatedFill = (float)Math.Round((volumeRatio * maxFluidFillHeight),3);
            for(int p = 0; p < roomFluidPlanes.Count; p++)
            {
                // Main thread API problem?
                if (roomFluidPlanes[p] != null)
                {
                    if ((p-1) < 0)
                    {
                        roomFluidPlanes[p].SetActive(true);
                        roomFluidPlanes[p].transform.localPosition = new Vector3(
                        roomFluidPlanes[p].transform.localPosition.x, translatedFill,
                        roomFluidPlanes[p].transform.localPosition.z);
                    }
                    else
                    {
                        float tfnot = translatedFill - roomFluidPlaneLevelLimits[p - 1];
                        if (tfnot <= 0f)
                        {
                            roomFluidPlanes[p].SetActive(false);
                        }
                        else
                        {
                            roomFluidPlanes[p].SetActive(true);
                            roomFluidPlanes[p].transform.localPosition = new Vector3(
                            roomFluidPlanes[p].transform.localPosition.x, tfnot,
                            roomFluidPlanes[p].transform.localPosition.z);
                        }
                    }
                    
                }
            }
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

        //Hide renderers and lights
        public void HideRenderVolume()
        {
            Debug.Log("Hide " + gameObject);
            rendersOn = false;
            if (volumeRenders != null && lightGOs != null)
            {
                for (int r = 0; r < volumeRenders.Length; r++)
                {
                    volumeRenders[r].enabled = false;
                }
                for (int l = 0; l < lightGOs.Length; l++)
                {
                    lightGOs[l].SetActive(false);
                }
            }
            if(GOculls != null)
            {
                for (int r = 0; r < GOculls.Count; r++)
                {
                    GOculls[r].SetActive(false);
                }
            }
            if (additionalRenderVolumes != null)
            {
                for (int a = 0; a < additionalRenderVolumes.Length; a++)
                {
                    if (additionalRenderVolumes[a].TryGetComponent(out VolumeAtmosphereController vac2))
                    {
                        if (vac2.rendersOn)
                        {
                            vac2.HideRenderVolume();
                        }
                    }
                    else
                    {
                        additionalRenderVolumes[a].SetActive(false);
                    }
                }
            }
            if (pipes != null)
            {
                for (int p = 0; p < pipes.Length; p++)
                {
                    //disable LODGroup
                    //disable subrenderers
                    pipes[p].GetComponent<LODGroup>().enabled = false;
                    foreach(Transform lod in pipes[p].transform)
                    {
                        lod.GetComponent<MeshRenderer>().enabled = false;
                    }
                }
            }
        }

        //show render groups, then next frame reactivate lights
        public void ShowRenderVolume()
        {
            Debug.Log("Render " + gameObject);
            rendersOn = true;
            StartCoroutine(ShowRenders());
            if (additionalRenderVolumes != null)
            {
                for (int a = 0; a < additionalRenderVolumes.Length; a++)
                {
                    if (additionalRenderVolumes[a].TryGetComponent(out VolumeAtmosphereController vac2))
                    {
                        if (!vac2.rendersOn)
                        {
                            vac2.ShowRenders();
                        }
                    }
                    else
                    {
                        additionalRenderVolumes[a].SetActive(true);
                    }
                }
            }
        }

        private IEnumerator ShowRenders()
        {
            //foreach (BoxCollider box in roomVolumeSections)
            //{
                //box.isTrigger = true;
                //box.enabled = true;
            //}
            //yield return null;
            for (int r = 0; r < volumeRenders.Length; r++)
            {
                volumeRenders[r].enabled = true;
            }
            yield return null;
            if (GOculls != null)
            {
                for (int r = 0; r < GOculls.Count; r++)
                {
                    GOculls[r].SetActive(true);
                }
            }
            yield return null;
            if (pipes != null)
            {
                for (int p = 0; p < pipes.Length; p++)
                {
                    //disable LODGroup
                    //disable subrenderers
                    pipes[p].GetComponent<LODGroup>().enabled = true;
                    foreach (Transform lod in pipes[p].transform)
                    {
                        lod.GetComponent<MeshRenderer>().enabled = true;
                    }
                }
            }
            yield return null;
            for (int l = 0; l < lightGOs.Length; l++)
            {
                lightGOs[l].SetActive(true);
            }
        }

        //public bool RenderState
        //{
        //    get { return renderstate; }
        //    set { renderstate = value; }
        //}

        
        public void RenderPlaneSetup(GameObject[] doors, GameObject planePrefab, Transform planeParent)
        {
            Debug.Log("RPS");
            //get all top-level trigger colliders, including from doors
            /*foreach (BoxCollider col in roomVolumeSections)
            {
                if (col.isTrigger)
                {
                    //prefab COLplanes to form a cube
                    GameObject planeTo = Instantiate(planePrefab, planeParent);
                    GameObject planeBo = Instantiate(planePrefab, planeParent);
                    GameObject planeL = Instantiate(planePrefab, planeParent);
                    GameObject planeR = Instantiate(planePrefab, planeParent);
                    GameObject planeFr = Instantiate(planePrefab, planeParent);
                    GameObject planeBk = Instantiate(planePrefab, planeParent);
                    //center and side offsets for cube
                    float xOff = col.size.x / 2f;
                    float yOff = col.size.y / 2f;
                    float zOff = col.size.z / 2f;
                    Vector3 center = col.center;
                    Debug.Log("local center: "+center);
                    Debug.Log("local x offset: " + xOff);
                    Debug.Log("local y offset: " + yOff);
                    Debug.Log("local z offset: " + zOff);
                    //position - coordinate system shifts by axis
                    planeTo.transform.localPosition = new Vector3(center.x, center.y + yOff, center.z);
                    planeBo.transform.localPosition = new Vector3(center.x, center.y - yOff, center.z);
                    planeL.transform.localPosition = new Vector3(center.x - xOff, center.y, center.z);
                    planeR.transform.localPosition = new Vector3(center.x + xOff, center.y, center.z);
                    planeFr.transform.localPosition = new Vector3(center.x, center.y, center.z + zOff);
                    planeBk.transform.localPosition = new Vector3(center.x, center.y, center.z - zOff);
                    //scaling
                    //To,Bo - ZX
                    //L,R - ZY
                    //Fr,Bk - XY
                    planeTo.transform.localScale = new Vector3(xOff, 1f, zOff);
                    planeBo.transform.localScale = new Vector3(xOff, 1f, zOff);
                    planeL.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                    planeR.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                    planeL.transform.localScale = new Vector3(yOff, 1f, zOff);
                    planeR.transform.localScale = new Vector3(yOff, 1f, zOff);
                    planeFr.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                    planeBk.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                    planeFr.transform.localScale = new Vector3(xOff, 1f, yOff);
                    planeBk.transform.localScale = new Vector3(xOff, 1f, yOff);
                }
            }

            //generate a portal collider at each door
            foreach(GameObject door in doors)
            {
                GameObject planeDoom = Instantiate(planePrefab, planeParent);
                Vector3 bias = Vector3.zero;
                if(door.transform.localRotation.eulerAngles == new Vector3(0f, 90f, 0f))
                {
                    //x+1, y +1, z - 0.5
                    bias = new Vector3(-0.5f, 1f, 0.5f);
                    planeDoom.transform.localRotation = Quaternion.Euler(90f, 0f, 90f);
                    planeDoom.tag = "Door";
                }
                else if (door.transform.localRotation.eulerAngles == new Vector3(0f, 180f, 0f))
                {
                    //x+1, y +1, z - 0.5
                    bias = new Vector3(+0.5f, 1f, 0.5f);
                    planeDoom.transform.localRotation = Quaternion.Euler(0f, 90f, 90f);
                    planeDoom.tag = "Door";
                }
                else if (door.transform.localRotation.eulerAngles == new Vector3(0f, 0f, 0f))
                {
                    //x+1, y +1, z - 0.5
                    bias = new Vector3(-0.5f, 1f, -0.5f);
                    planeDoom.transform.localRotation = Quaternion.Euler(90f, 90f, 90f);
                    planeDoom.tag = "Door";
                }
                else if (door.transform.localRotation.eulerAngles == new Vector3(0f, 270f, 0f))
                {
                    //x+1, y +1, z - 0.5
                    bias = new Vector3(0.5f, 1f, -0.5f);
                    planeDoom.transform.localRotation = Quaternion.Euler(90f, 0f, 90f);
                    planeDoom.tag = "Door";
                }
                planeDoom.transform.position = door.transform.position;
                planeDoom.transform.localPosition += bias;
                planeDoom.transform.localScale = new Vector3(1f, 1f, 1f);
            }
            */
            //List<FrustrumState> fsList = new List<FrustrumState>();
            foreach (Transform child in transform)
            {
                if (child.gameObject.activeInHierarchy)
                {
                    if (child.TryGetComponent(out MeshRenderer render))
                    {
                        //place a FrustrumState script in obj
                        //render.gameObject.AddComponent<FrustrumState>();
                    }
                }
            }
            //now raycast from all instancedColliders' centers and find what walls or whatever they are flush to.
            RaycastHit[] hits;
            foreach (Transform obj in planeParent)
            {
                if (obj.gameObject.activeInHierarchy)
                {
                    //raycast front / back. if no valid collisions, cast L, R for 1.25m to catch edges of doors.
                    //hits = Physics.SphereCastAll(obj.position, 0.1f, transform.forward, 0.25f);
                    bool neg = true;
                    hits = Physics.RaycastAll(obj.position, transform.forward, 0.25f);
                    for (int k = 0; k < hits.Length; k++)
                    {
                        FrustrumState fs = hits[k].transform.GetComponentInParent<FrustrumState>();
                        if (fs != null)
                        {
                            Debug.Log(hits[k].transform.name + ": " + fs.name);
                            fs.AddStatePlate(obj.GetComponent<MeshCollider>());
                            neg = false;
                            //deactivate the object fir debug purposes
                            obj.gameObject.SetActive(false);
                        }
                    }
                    if (neg)
                    {
                        hits = Physics.RaycastAll(obj.position, -transform.forward, 0.25f);
                        for (int k = 0; k < hits.Length; k++)
                        {
                            FrustrumState fs = hits[k].transform.GetComponentInParent<FrustrumState>();
                            if (fs != null)
                            {
                                Debug.Log(hits[k].transform.name + ": " + fs.name);
                                fs.AddStatePlate(obj.GetComponent<MeshCollider>());
                                neg = false;
                                //deactivate the object fir debug purposes
                                obj.gameObject.SetActive(false);
                            }
                        }
                        if (neg)
                        {
                            hits = Physics.RaycastAll(obj.position, transform.right, 1.5f);
                            for (int k = 0; k < hits.Length; k++)
                            {
                                FrustrumState fs = hits[k].transform.GetComponentInParent<FrustrumState>();
                                if (fs != null)
                                {
                                    Debug.Log(hits[k].transform.name + ": " + fs.name);
                                    fs.AddStatePlate(obj.GetComponent<MeshCollider>());
                                    neg = false;
                                    //deactivate the object fir debug purposes
                                    obj.gameObject.SetActive(false);
                                }
                            }
                            if (neg)
                            {
                                hits = Physics.RaycastAll(obj.position, -transform.right, 1.5f);
                                for (int k = 0; k < hits.Length; k++)
                                {
                                    FrustrumState fs = hits[k].transform.GetComponentInParent<FrustrumState>();
                                    if (fs != null)
                                    {
                                        Debug.Log(hits[k].transform.name + ": " + fs.name);
                                        fs.AddStatePlate(obj.GetComponent<MeshCollider>());
                                        neg = false;
                                        //deactivate the object fir debug purposes
                                        obj.gameObject.SetActive(false);
                                    }
                                }
                                if (neg)
                                {
                                    hits = Physics.RaycastAll(obj.position, transform.up, 0.5f);
                                    for (int k = 0; k < hits.Length; k++)
                                    {
                                        FrustrumState fs = hits[k].transform.GetComponentInParent<FrustrumState>();
                                        if (fs != null)
                                        {
                                            Debug.Log(hits[k].transform.name + ": " + fs.name);
                                            fs.AddStatePlate(obj.GetComponent<MeshCollider>());
                                            neg = false;
                                            //deactivate the object fir debug purposes
                                            obj.gameObject.SetActive(false);
                                        }
                                    }
                                    if (neg)
                                    {
                                        hits = Physics.RaycastAll(obj.position, -transform.up, 0.5f);
                                        for (int k = 0; k < hits.Length; k++)
                                        {
                                            FrustrumState fs = hits[k].transform.GetComponentInParent<FrustrumState>();
                                            if (fs != null)
                                            {
                                                Debug.Log(hits[k].transform.name + ": " + fs.name);
                                                fs.AddStatePlate(obj.GetComponent<MeshCollider>());
                                                //deactivate the object fir debug purposes
                                                obj.gameObject.SetActive(false);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}