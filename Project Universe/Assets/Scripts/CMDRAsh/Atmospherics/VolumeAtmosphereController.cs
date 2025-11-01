using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using ProjectUniverse.Environment.Gas;
using ProjectUniverse.Environment.Fluids;
using ProjectUniverse.Animation.Controllers;
using UnityEngine.Profiling;
using ProjectUniverse.Util;
using ProjectUniverse.Ship;
using ProjectUniverse.PowerSystem;
using ProjectUniverse.Environment.Chemistry;
using System.Linq;
using static UnityEngine.Rendering.LineRendering;
using static UnityEngine.Rendering.DebugUI;

namespace ProjectUniverse.Environment.Volumes
{
    //[RequireComponent(typeof(VolumeComponent))]
    public sealed class VolumeAtmosphereController : MonoBehaviour, IGasContainer
    {
        private float roomPressure;
        [SerializeField] private float roomTemp;//rooms cool to -200f over time, without heating
        [SerializeField] private float roomOxygenation;
        [SerializeField] private float roomVolume;
        [SerializeField] private float humidity;
        [SerializeField] private float toxicity;//gasses and stuff
        [SerializeField] private float contamination;//radioactive particles
        private List<IGas> roomIGases = new List<IGas>();

        //private List<IGas> gasesToEq = new List<IGas>();
        private List<IFluid> roomIFluids = new List<IFluid>();

        //A
        private List<Fluid> roomFluids = new();

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

        [Header("Explosion Propagation")]
        [SerializeField] private bool hasActiveExplosion = false;
        private float explosionPressureWave;
        private float explosionHeatWave;
        [SerializeField] private List<ParticulateConcentration> particulates = new List<ParticulateConcentration>();

        private void Start()
        {
            if (autoFill)
            {
                // Create oxygen as a Fluid in gas state
                AddRoomGas(new Fluid("Oxygen", roomVolume * 1.293f, 288.15f, roomVolume, 1.0f)); // ~20°C, 1 atm
            }
            if (roomFluids.Count == 0)
            {
                for (int j = 0; j < roomFluidPlanes.Count; j++)
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
                        else if(child.gameObject.CompareTag("IBreaker"))
                        {
                            foreach (Transform obj in child.transform)
                            {
                                if(obj.CompareTag("_root"))
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
                    if (!carry.gameObject.CompareTag("GUI") && !carry.gameObject.CompareTag("Pipe"))
                    {
                        MeshRenderer[] renderchilds = carry.GetComponentsInChildren<MeshRenderer>(false);
                        //Debug.Log(renderchilds.Length);
                        for (int r = 0; r < renderchilds.Length; r++)
                        {
                            if (!renderchilds[r].gameObject.CompareTag("GUI") && !renderchilds[r].gameObject.CompareTag("Button3D"))
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
                    if (lodObjs[j].CompareTag("Untagged"))
                    {
                        GOculls.Add(lodObjs[j].gameObject);
                    }
                    else if (lodObjs[j].CompareTag("Pipe"))
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

            // Set door vac references
            foreach(DoorAnimator door in roomDoorsFluidOrder)
            {
                door.VACRef = this;
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
            set {
                roomPressure = value;
                foreach (IGas gas in roomIGases)//LEGACY
                {
                    gas.SetLocalPressure(value); 
                }
            }
        }
        public float RoomPressure
        {
            get { return roomPressure; }
            set
            {
                roomPressure = value;
                foreach (Fluid fluid in roomFluids)
                {
                    fluid.SetPressure(value);
                }
            }
        }
        /// <summary>
        /// Add pressure directly to the room (for explosions)
        /// </summary>
        public void AddPressure(float pressureIncrease)
        {
            roomPressure += pressureIncrease;

            // Update all gas pressures
            foreach (Fluid gas in roomFluids)
            {
                gas.SetPressure(roomPressure);
            }
        }

        public float Toxicity
        {
            get { return toxicity; }
            set { toxicity = value; }
        }

        
        public float Contamination
        {
            get
            {
                float total = 0f;
                foreach (var particulate in particulates)
                {
                    total += particulate.ConcentrationPPMW;
                }
                return total;
            }
        }
        public List<BoxCollider> RoomVolumeSections
        {
            get { return roomVolumeSections; }
        }

        public List<IGas> RoomGassesLegacy
        {
            get { return Gases; }
            set { Gases = value; }
        }
        public List<IGas> Gases // For interface compatability
        {
            get { return roomIGases; }
            set { roomIGases = value; }
        }
        public List<IFluid> RoomFluidsLegacy
        {
            get { return roomIFluids; }
        }
        //A
        public List<Fluid> RoomFluids
        {
            get { return roomFluids; }
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
            for (int i = 0; i < neighborEmpties.Length; i++)
            {
                GameObject door = neighborEmpties[i].GetComponent<VolumeNode>().GetDoor();
                //if the door (in this volume) is open
                DoorAnimator da = door.GetComponent<DoorAnimator>();
                if (da.OpenOrOpening() || da.IsRuptured)//roomVolumeDoors
                {
                    Vector3 back = door.transform.TransformDirection(Vector3.back);//is Vector3.back for all cases?
                    RaycastHit[] hits = new RaycastHit[2];
                    Physics.RaycastNonAlloc(new Vector3(door.transform.position.x,
                        door.transform.position.y + 0.025f, door.transform.position.z), back, hits, 1.0f);
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
                                clear = myDoorGameobject.GetComponent<DoorAnimator>().OpenOrOpening() || 
                                    myDoorGameobject.GetComponent<DoorAnimator>().IsRuptured;
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
                float enthalpy = 0f;
                if (roomFluids.Count > 0)
                {
                    for (int q = 0; q < roomFluids.Count; q++)
                    {
                        enthalpy += roomFluids[q].GetEnthalpy();
                    }

                    if (enthalpy > 0f)
                    {
                        float heatLossPerSec = qHeatLossPerSec * Time.deltaTime;

                        foreach (var gas in roomFluids)
                        {
                            // Each gas loses heat proportional to its enthalpy content
                            float gasRatio = gas.GetEnthalpy() / enthalpy;
                            float gasHeatLoss = heatLossPerSec * gasRatio;

                            // Use the Fluid class's RemoveEnergy method
                            gas.RemoveEnergy(gasHeatLoss);
                        }
                    }
                }
                else
                {
                    //without gas to hold heat, temp will drop more quickly.
                    float massRoom = (roomVolume) * 6836f;//6.836Kg in 1m3
                    float dt = qHeatLossPerSec / (massRoom * 532f);//Cp of .8iron .2aluminum (440 and 900)
                    //Debug.Log("+="+dt);
                    dt *= (9f / 5f); // K to F conversion for temperature difference
                    //roomTemp += dt * Time.deltaTime;
                }

                CalculateRoomTemp();
            }
        }

        public void AddRoomHeat(float heat, bool dump=false)
        {
            float enthalpy = 0f;
            if (roomFluids.Count > 0)
            {
                for (int q = 0; q < roomFluids.Count; q++)
                {
                    enthalpy += roomFluids[q].GetEnthalpy();
                }
                if (enthalpy > 0f)
                {
                    float heatToAdd = dump ? heat : heat * Time.deltaTime;
                    foreach (var gas in roomFluids)
                    {
                        // Each gas receives heat proportional to its enthalpy content
                        float gasRatio = gas.GetEnthalpy() / enthalpy;
                        float gasHeatGain = heatToAdd * gasRatio;

                        // Use the Fluid class's AddEnergy method
                        gas.AddEnergy(gasHeatGain);
                    }
                }
            }
            else
            {
                float massRoom = roomVolume * 6836f; // 6.836Kg/m³ for ship structure
                float dt = heat / (massRoom * 532f); // Cp of .8iron .2aluminum (440 and 900)

                // Convert to Fahrenheit delta
                dt *= (9f / 5f); // K to F conversion for temperature difference
                roomTemp += dt * Time.deltaTime;
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
                roomFluids = new List<Fluid>();
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

        public List<Fluid> CheckGasses(bool setToLocalPressure, float localPressure)
        {
            if (roomFluids.Count <= 1)
                return roomFluids;

            // Use the new CombineFluids method from Utils
            roomFluids = Utils.CombineFluids(roomFluids);

            if (setToLocalPressure)
            {
                foreach (var gas in roomFluids)
                {
                    // The pressure should likely be calculated by the Fluid and set back to room.
                    gas.SetPressure(localPressure);
                }
            }

            return roomFluids;
        }
        public List<Fluid> CheckFluids(bool setToLocalPressure, float localPressure)
        {
            if (roomFluids.Count <= 1)
                return roomFluids;

            // Use the new CombineFluids method from Utils
            roomFluids = Utils.CombineFluids(roomFluids);

            if (setToLocalPressure)
            {
                foreach (var fluid in roomFluids)
                {
                    fluid.SetPressure(localPressure);
                }
            }

            return roomFluids;
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
            if (roomIGases.Count == 0) return;

            IGas gas = new IGas(gasToRemove);
            if (roomIGases.Count > 0)
            {
                for (int j = 0; j < roomIGases.Count; j++)
                {
                    if (roomIGases[j].GetIDName() == gas.GetIDName())
                    {
                        float nVal = roomIGases[j].GetConcentration() - gas.GetConcentration();
                        if (nVal > 0f)
                        {
                            roomIGases[j].SetConcentration(nVal);
                        }
                        else
                        {
                            //may not work?
                            roomIGases.Remove(gas);
                        }
                    }
                }
            }
            //update Volume Atmosphere
            float totalGas = CalculateRoomOxygenation();
            CalculateRoomTemp();
            CalculateRoomPressure(totalGas);
        }


        public void RemoveRoomGas(Fluid gasToRemove)
        {
            if (roomFluids.Count == 0) return;

            for (int j = 0; j < roomFluids.Count; j++)
            {
                if (roomFluids[j].GetIDName() == gasToRemove.GetIDName())
                {
                    float remainingMass = roomFluids[j].GetMass() - gasToRemove.GetMass();
                    if (remainingMass > 0.001f)
                    {
                        roomFluids[j].SetMass(remainingMass);
                    }
                    else
                    {
                        roomFluids.RemoveAt(j);
                    }
                    break;
                }
            }

            //update Volume Atmosphere
            float totalGas = CalculateRoomOxygenation();
            CalculateRoomTemp();
            CalculateRoomPressure(totalGas);
        }

        public void RemoveRoomGas(float pressureToRemove)
        {
            if (roomFluids.Count == 0 || roomPressure <= 0) return;

            // Calculate total mass to remove based on pressure
            float totalVolume = 0f;
            foreach (var gas in roomFluids)
            {
                totalVolume += gas.GetVolume();
            }

            float pressureRatio = pressureToRemove / roomPressure;
            List<Fluid> removedGases = Utils.ExtractMass(roomFluids, totalVolume * pressureRatio * 1.293f); // Approximate density

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
            if (roomIGases.Count > 0)
            {
                for (int j = 0; j < roomIGases.Count; j++)
                {
                    if (roomIGases[j].GetIDName() == gas.GetIDName())
                    {
                        roomIGases[j] = CombineGases(roomIGases[j], gas, Pressure, false);
                        add = false;
                    }
                }
                if (add)
                {
                    //Debug.Log("Add");
                    roomIGases.Add(gas);
                }
            }
            else
            {
                //Debug.Log("Add");
                roomIGases.Add(gas);
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
        public void AddRoomGas(Fluid gasToAdd)
        {
            bool found = false;

            for (int j = 0; j < roomFluids.Count; j++)
            {
                if (roomFluids[j].GetIDName() == gasToAdd.GetIDName())
                {
                    roomFluids[j] = Fluid.Mix(roomFluids[j], gasToAdd);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                roomFluids.Add(new Fluid(gasToAdd));
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
            roomIFluids.Add(fluid);
            //roomIFluids = CheckFluids(true, Pressure);
            UpdateRoomFluidLevel();
        }


        public void AddRoomFluid(Fluid fluidToAdd)
        {
            roomFluids.Add(new Fluid(fluidToAdd));
            roomFluids = Utils.CombineFluids(roomFluids);
            UpdateRoomFluidLevel();
        }

        public List<Fluid> RemoveRoomFluid(float volumeToRemove)
        {
            if (roomFluids.Count == 0)
                return new List<Fluid>();

            float totalVolume = 0f;
            foreach (var fluid in roomFluids)
            {
                totalVolume += fluid.GetVolume();
            }

            if (totalVolume == 0f)
                return new List<Fluid>();

            // Extract proportionally by volume
            float extractRatio = Mathf.Min(volumeToRemove / totalVolume, 1f);
            List<Fluid> extracted = new List<Fluid>();

            for (int i = roomFluids.Count - 1; i >= 0; i--)
            {
                float fluidVolume = roomFluids[i].GetVolume();
                float extractVolume = fluidVolume * extractRatio;

                // Convert volume to mass for extraction
                float density = roomFluids[i].GetMass() / fluidVolume;
                float extractMass = extractVolume * density;

                Fluid extractedFluid = roomFluids[i].Split(extractMass);
                extracted.Add(extractedFluid);

                if (roomFluids[i].GetMass() < 0.001f)
                {
                    roomFluids.RemoveAt(i);
                }
            }

            UpdateRoomFluidLevel();
            return extracted;
        }

        public void AddRoomFluid(List<Fluid> fluidsToAdd)
        {
            foreach (var fluid in fluidsToAdd)
            {
                AddRoomFluid(fluid);
            }
        }

        /// <summary>
        /// recalc room oxygenation. It is the ratio of oxygen (in m3) to room volume (m3).
        /// Gasses that are not oxygen do not count towards oxygenation.
        /// Returns: The total amount of gasses in the room in m3
        /// </summary>
        public float CalculateRoomOxygenation()
        {
            float oxygenVolume = 0.0f;
            float totalGasVolume = 0.0f;

            foreach (var gas in roomFluids)
            {
                if (gas.GetState() == FluidState.Gas || gas.GetState() == FluidState.Mixed)
                {
                    float gasVolume = gas.GetVolume();
                    if (gas.GetState() == FluidState.Mixed)
                    {
                        gasVolume *= gas.GetQuality(); // Only count vapor fraction
                    }

                    totalGasVolume += gasVolume;

                    if (gas.GetIDName() == "Oxygen")
                    {
                        oxygenVolume += gasVolume;
                    }
                }
            }

            roomOxygenation = (oxygenVolume / roomVolume) * 100f;
            return totalGasVolume;
        }

        /// <summary>
        /// Calculate room temp based on the gasses present in the room, and it's ambient heat.
        /// Returns the room temp before temp Eq.
        /// </summary>
        public void CalculateRoomTemp()
        {
            if (roomFluids.Count == 0)
                return;

            float totalEnthalpy = 0f;
            float totalMass = 0f;

            foreach (var gas in roomFluids)
            {
                totalEnthalpy += gas.GetEnthalpy();
                totalMass += gas.GetMass();
            }

            // Weight temperature by mass
            float weightedTemp = 0f;
            foreach (var gas in roomFluids)
            {
                weightedTemp += gas.GetTemperature() * (gas.GetMass() / totalMass);
            }

            roomTemp = (weightedTemp - 273.15f) * 1.8f + 32f; // Convert K to F
        }

        /// <summary>
        /// Recalc room pressure based on the concentration of gas present in the volume, and it's temperature.
        /// </summary>
        public void CalculateRoomPressure(float totalRoomGasses_m3)
        {
            if (roomFluids.Count == 0)
            {
                roomPressure = 0f;
                return;
            }
            else
            {
                float totalPressure = 0f;
                foreach (var gas in roomFluids)
                {
                    totalPressure = gas.GetPressure();
                }
                roomPressure = totalPressure / RoomFluids.Count;
            }

            // Calculate pressure based on gas volume in room
            // Now done by Fluid class
            /*
            float totalPressure = 0f;

            foreach (var gas in roomFluids)
            {
                if (gas.GetState() == FluidState.Gas || gas.GetState() == FluidState.Mixed)
                {
                    // For gases, use ideal gas law contribution
                    float gasVolume = gas.GetVolume();
                    if (gas.GetState() == FluidState.Mixed)
                    {
                        gasVolume *= gas.GetQuality();
                    }

                    // Partial pressure contribution
                    totalPressure += gas.GetPressure() * (gasVolume / roomVolume);
                }
            }

            roomPressure = totalPressure;

            // Update all gas pressures
            foreach (var gas in roomFluids)
            {
                gas.SetPressure(roomPressure);
            }*/
        }

        public void AddParticulate(string particulateTypeID, float ppmw)
        {
            var existing = particulates.Find(p => p.ParticulateTypeID == particulateTypeID);
            if (existing != null)
            {
                existing.ConcentrationPPMW += ppmw;
            }
            else
            {
                particulates.Add(new ParticulateConcentration(particulateTypeID, ppmw));
            }
        }

        public void RemoveParticulate(string particulateTypeID, float ppmw)
        {
            var existing = particulates.Find(p => p.ParticulateTypeID == particulateTypeID);
            if (existing != null)
            {
                existing.ConcentrationPPMW = Mathf.Max(0f, existing.ConcentrationPPMW - ppmw);
                if (existing.ConcentrationPPMW <= 0.001f)
                {
                    particulates.Remove(existing);
                }
            }
        }

        public float GetParticulateConcentration(string particulateTypeID)
        {
            var existing = particulates.Find(p => p.ParticulateTypeID == particulateTypeID);
            return existing?.ConcentrationPPMW ?? 0f;
        }

        public List<ParticulateConcentration> GetAllParticulates()
        {
            return new List<ParticulateConcentration>(particulates);
        }

        public float GetTotalHealthRisk()
        {
            float risk = 0f;
            foreach (var particulate in particulates)
            {
                var type = ParticulateDatabase.GetParticulate(particulate.ParticulateTypeID);
                if (type != null)
                {
                    risk += type.HealthRisk * (particulate.ConcentrationPPMW / 1000f); // Normalized
                }
            }
            return risk;
        }

        public float GetTotalRadioactivity()
        {
            float activity = 0f;
            foreach (var particulate in particulates)
            {
                var type = ParticulateDatabase.GetParticulate(particulate.ParticulateTypeID);
                if (type != null)
                {
                    activity += type.Radioactivity * (particulate.ConcentrationPPMW / 1000000f);
                }
            }
            return activity;
        }

        public bool HasCombustibleParticulates()
        {
            foreach (var particulate in particulates)
            {
                var type = ParticulateDatabase.GetParticulate(particulate.ParticulateTypeID);
                if (type != null && type.Combustibility > 0f && particulate.ConcentrationPPMW >= type.Combustibility)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Check if any doors are ruptured and propagate explosion to connected rooms
        /// </summary>
        public void CheckExplosionPropagation(float explosionIntensity, float heatReleased)
        {
            if (roomDoorsFluidOrder == null || roomDoorsFluidOrder.Length == 0)
                return;

            hasActiveExplosion = true;
            explosionPressureWave = explosionIntensity;
            explosionHeatWave = heatReleased;

            for (int i = 0; i < roomDoorsFluidOrder.Length; i++)
            {
                DoorAnimator door = roomDoorsFluidOrder[i];
                if (door != null && door.IsRuptured)
                {
                    PropagateExplosionThroughDoor(i, explosionIntensity, heatReleased);
                }
            }
        }

        /// <summary>
        /// Propagate explosion through a specific ruptured door
        /// </summary>
        private void PropagateExplosionThroughDoor(int doorIndex, float explosionIntensity, float heatReleased)
        {
            if (doorIndex >= neighborEmpties.Count())
                return;

            GameObject neighborEmpty = neighborEmpties[doorIndex];
            if (neighborEmpty == null)
                return;

            VolumeNode volumeNode = neighborEmpty.GetComponent<VolumeNode>();
            if (volumeNode == null)
                return;

            // Check local neighbor (another room)
            GameObject localNeighbor = volumeNode.VolumeLink;
            if (localNeighbor != null)
            {
                VolumeAtmosphereController neighborAtmosphere =
                    localNeighbor.GetComponent<VolumeAtmosphereController>();

                if (neighborAtmosphere != null && !neighborAtmosphere.hasActiveExplosion)
                {
                    // Transfer pressure wave (attenuated)
                    float pressureTransfer = explosionIntensity * 0.6f; // 60% of original intensity
                    neighborAtmosphere.AddPressure(pressureTransfer);

                    // Transfer heat wave (attenuated)
                    float heatTransfer = heatReleased * 0.5f; // 50% of original heat
                    neighborAtmosphere.AddRoomHeat(heatTransfer);

                    // Trigger ignition in connected room
                    RoomReactionManager neighborReactions = localNeighbor.GetComponent<RoomReactionManager>();
                    if (neighborReactions != null)
                    {
                        neighborReactions.TriggerIgnition(2f); // 2 second ignition burst
                        Debug.Log($"Explosion propagated from {gameObject.name} to {localNeighbor.name}");
                    }

                    // Add explosion particulates to neighbor
                    float particulateSpread = explosionIntensity * 5f;
                    neighborAtmosphere.AddParticulate("ash", particulateSpread * 0.4f);
                    neighborAtmosphere.AddParticulate("carbon_black", particulateSpread * 0.3f);
                    neighborAtmosphere.AddParticulate("soot", particulateSpread * 0.3f);

                    // Recursively check if neighbor has ruptured doors
                    neighborAtmosphere.CheckExplosionPropagation(
                        explosionIntensity * 0.5f,
                        heatReleased * 0.3f
                    );
                }
            }
            // If it's a global neighbor (space/exterior)
            else
            {
                GameObject globalNeighbor = volumeNode.GlobalLink;
                if (globalNeighbor != null)
                {
                    // Explosion vents to space/exterior - rapid pressure loss
                    //Pressure *= 0.3f; // Lose 70% of pressure rapidly
                    //Temperature *= 0.7f; // Lose heat to space

                    Debug.Log($"Explosion vented from {gameObject.name} to exterior");
                }
            }
        }

        /// <summary>
        /// Check if pressure is high enough to rupture doors
        /// This is also being checked by every door, so this block is a backup
        /// </summary>
        public void CheckPressureRupture()
        {
            foreach (GameObject obj in neighborEmpties)
            {
                VolumeNode vn = obj.GetComponent<VolumeNode>();
                float headpres = vn.VolumeLink.GetComponent<VolumeAtmosphereController>().Pressure;
                DoorAnimator dr = vn.GetDoor().GetComponent<DoorAnimator>();
                float maxPres = dr.MaxPressure;

                if (Pressure - headpres >= maxPres)
                {
                    dr.IsRuptured = true;
                    Debug.LogWarning($"Door {dr.gameObject.name} ruptured due to pressure: {Pressure:F2} atm");
                }
            }
        }

        /// <summary>
        /// Reset explosion state after propagation completes
        /// </summary>
        public void ResetExplosionState()
        {
            hasActiveExplosion = false;
            explosionPressureWave = 0f;
            explosionHeatWave = 0f;
        }

        public void UpdateRoomFluidLevel()
        {
            float totalFluidVolume = 0.0f;

            foreach (var fluid in roomFluids)
            {
                // Only count liquid volume
                if (fluid.GetState() == FluidState.Liquid)
                {
                    totalFluidVolume += fluid.GetVolume();
                }
                else if (fluid.GetState() == FluidState.Mixed)
                {
                    // For mixed state, only count liquid fraction
                    totalFluidVolume += fluid.GetVolume() * (1f - fluid.GetQuality());
                }
            }

            if (totalFluidVolume <= 0.0001f)
            {
                //set all planes false, as the room has emptied
                for (int f = 0; f < roomFluidPlanes.Count; f++)
                {
                    roomFluidPlanes[f].SetActive(false);
                }
                return;
            }

            float volumeRatio = totalFluidVolume / roomVolume;
            float translatedFill = (float)Math.Round((volumeRatio * maxFluidFillHeight), 3);

            // Update fluid plane positions (rest of method unchanged)
            for (int p = 0; p < roomFluidPlanes.Count; p++)
            {
                if (roomFluidPlanes[p] != null)
                {
                    if ((p - 1) < 0)
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

        /// <summary>
        /// Get total fluid volume in the room
        /// </summary>
        public float GetTotalFluidVolume()
        {
            float total = 0f;
            foreach (var fluid in roomFluids)
            {
                total += fluid.GetVolume();
            }
            return total;
        }

        /// <summary>
        /// Check if fluid level allows passage through a specific door
        /// </summary>
        public bool CanFluidPassThroughDoor(int doorIndex)
        {
            if (doorIndex < 0 || doorIndex >= roomFluidPlaneDoorLevels.Length)
                return false;

            float totalFluidVolume = GetTotalFluidVolume();
            if (totalFluidVolume <= 0.001f)
                return false;

            float fillHeight = totalFluidVolume / roomFloorArea;
            return fillHeight >= roomFluidPlaneDoorLevels[doorIndex];
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
    }
}