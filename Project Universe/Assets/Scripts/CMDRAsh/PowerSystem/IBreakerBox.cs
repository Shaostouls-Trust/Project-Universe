using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ProjectUniverse.Data.Libraries;
using Unity.Netcode;
using ProjectUniverse.Animation.Controllers;
using ProjectUniverse.Environment.Interactable;
using ProjectUniverse.Serialization;
using System.Linq;

namespace ProjectUniverse.PowerSystem
{
    /*
     * The purpose of this class is to distribute power to large amounts of small IMachines (not unlike IRoutingSubtation, save simpler).
     */
    public sealed class IBreakerBox : NetworkBehaviour//MonoBehavior
    {
        private Guid guid;
        [Header("Power Configuration")]
        [SerializeField] private ISubMachine[] targetSubMachine;
        [SerializeField] private float totalRequiredPower;
        [SerializeField] private float energyBufferMax = 5600.0f;
        [SerializeField] private float bufferCurrent;
        [SerializeField] private float defecitVbreaker;

        [Header("Connection Settings")]
        [SerializeField] private readonly int maxConnections = 30;
        [SerializeField] private GameObject[] occupiedSwitches;
        [SerializeField] private int switchCount;

        [Header("Power Distribution")]
        [SerializeField] private int legsRequired = 3;
        [SerializeField] private int legsReceived;
        [SerializeField] private bool runMachine = true;

        [Header("Audio")]
        [SerializeField] private AudioSource soundsource;

        // Private fields
        private IBreakerBox thisBreaker;
        private LinkedList<ICable> iCableDLL = new LinkedList<ICable>();
        private List<IRoutingSubstation> mySubstations = new List<IRoutingSubstation>();
        private List<Renderer> yellowSwitchRenderers = new List<Renderer>();
        private float lastReceived;

        // Network variables
        private NetworkVariable<float> netTotalRequiredPower = new NetworkVariable<float>();
        private NetworkVariable<float> netEnergyBufferMax = new NetworkVariable<float>();
        private NetworkVariable<float> netBufferCurrent = new NetworkVariable<float>();
        private NetworkVariable<int> netSwitchCount = new NetworkVariable<int>();
        private NetworkVariable<float> netDefecitVBreaker = new NetworkVariable<float>();
        private NetworkVariable<int> netLegsRequired = new NetworkVariable<int>();
        private NetworkVariable<int> netLegsReceived = new NetworkVariable<int>();
        //B2
        [Header("Connection Configuration")]
        [SerializeField] private PowerConnectionPoint connectionPoint = new PowerConnectionPoint();
        [SerializeField] private bool showConnectionPoints = true;

        public PowerConnectionPoint ConnectionPoint
        {
            get { return connectionPoint; }
        }

        public bool RunMachine
        {
            get => runMachine;
            set => runMachine = value;
        }

        public float LastReceived => lastReceived;
        public float BufferCurrent => bufferCurrent;

        void Start()
        {
            occupiedSwitches = new GameObject[switchCount];
            thisBreaker = GetComponent<IBreakerBox>();
            energyBufferMax = 5600.0f;
            bufferCurrent = 0f;
            totalRequiredPower = 0.0f;
            guid = Guid.NewGuid();
            NetworkListeners();
            Debug.Log("Breaker Proxy");
            ProxyStart();

            // Check initial machine states after setup
            CheckInitialMachineStates();

            //B2
            // Initialize connection point
            connectionPoint.ownerComponent = this;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!showConnectionPoints)
            {
                return;
            }

            // Draw connection points
            
            if (connectionPoint == null) return;
            connectionPoint.ownerComponent = this;

            Vector3 worldPos = connectionPoint.GetWorldPosition();

            // Draw sphere at connection point
            Gizmos.color = connectionPoint.connectionType == PowerConnectionPoint.ConnectionType.Input ?
                Color.cyan : Color.green;
            Gizmos.DrawWireSphere(worldPos, 0.3f);

            // Draw connection radius
            Gizmos.color = connectionPoint.connectionType == PowerConnectionPoint.ConnectionType.Input ?
                new Color(0f, 1f, 1f, 0.2f) : new Color(0f, 1f, 0f, 0.2f);
            Gizmos.DrawWireSphere(worldPos, connectionPoint.connectionRadius);

            // Draw direction arrow
            Gizmos.color = connectionPoint.connectionType == PowerConnectionPoint.ConnectionType.Input ?
                Color.cyan : Color.green;
            Vector3 endPos = worldPos + connectionPoint.GetWorldDirection() * 1f;
            Gizmos.DrawLine(worldPos, endPos);

            // Draw cone for output, inverted cone for input
            if (connectionPoint.connectionType == PowerConnectionPoint.ConnectionType.Output)
            {
                GizmosExtensions.DrawCone(endPos, connectionPoint.GetWorldDirection(), 0.2f, 0.3f);
            }
            else
            {
                GizmosExtensions.DrawCone(endPos, connectionPoint.GetWorldDirection(), 0.2f, 0.3f);
            }
            
        }

        private void OnDrawGizmosSelected()
        {
            if (!showConnectionPoints)
            {
                return;
            }
            if (connectionPoint == null) return;

            Vector3 worldPos = connectionPoint.GetWorldPosition();
            UnityEditor.Handles.Label(worldPos + Vector3.up * 0.5f,
                $"{connectionPoint.name} ({connectionPoint.connectionType})");
            
        }
#endif

        private void CheckInitialMachineStates()
        {
            // Check and set initial LED states for all machines
            for (int i = 0; i < targetSubMachine.Length && i < occupiedSwitches.Length; i++)
            {
                if (occupiedSwitches[i] != null)
                {
                    var sac = occupiedSwitches[i].transform.GetChild(4).GetComponent<SwitchAnimationController>();
                    if (sac != null)
                    {
                        GameObject[] mySwitchLEDs = { sac.GreenLED, sac.RedLED, sac.YellowLED };

                        if (targetSubMachine[i] != null && targetSubMachine[i].gameObject.activeInHierarchy)
                        {
                            // Set initial LED states based on machine state
                            if (targetSubMachine[i].RunMachine)
                            {
                                mySwitchLEDs[0].GetComponent<Renderer>().material = MaterialLibrary.GetPowerSystemStateMaterials(0); // green on
                                mySwitchLEDs[1].GetComponent<Renderer>().material = MaterialLibrary.GetPowerSystemStateMaterials(4); // red off
                            }
                            else
                            {
                                mySwitchLEDs[0].GetComponent<Renderer>().material = MaterialLibrary.GetPowerSystemStateMaterials(3); // green off
                                mySwitchLEDs[1].GetComponent<Renderer>().material = MaterialLibrary.GetPowerSystemStateMaterials(1); // red on
                            }
                        }
                        else
                        {
                            // Machine not active - all LEDs off
                            mySwitchLEDs[0].GetComponent<Renderer>().material = MaterialLibrary.GetPowerSystemStateMaterials(3); // green off
                            mySwitchLEDs[1].GetComponent<Renderer>().material = MaterialLibrary.GetPowerSystemStateMaterials(4); // red off
                            mySwitchLEDs[2].GetComponent<Renderer>().material = MaterialLibrary.GetPowerSystemStateMaterials(5); // yellow off
                        }
                    }
                }
            }
        }

        private void NetworkListeners()
        {
            //var obj = Instantiate(prefab)
            //var t = obj.GetComponent
            //t.netTotalRequiredPower.Initialize(t);
            //t.netTotalRequiredPower.Value = 
            //SpawnWithOwnership
            netTotalRequiredPower.OnValueChanged += delegate { totalRequiredPower = netTotalRequiredPower.Value; };
            netEnergyBufferMax.OnValueChanged += delegate { energyBufferMax = netEnergyBufferMax.Value; };
            netBufferCurrent.OnValueChanged += delegate { bufferCurrent = netBufferCurrent.Value; };
            netSwitchCount.OnValueChanged += delegate { switchCount = netSwitchCount.Value; };
            netDefecitVBreaker.OnValueChanged += delegate { defecitVbreaker = netDefecitVBreaker.Value; };
            netLegsRequired.OnValueChanged += delegate { legsRequired = netLegsRequired.Value; };
            netLegsReceived.OnValueChanged += delegate { legsReceived = netLegsReceived.Value; };

        }

        //B
        void Update()
        {
            netTotalRequiredPower.Value = 0f;

            // Calculate total required power
            float ttrp = 0f;
            for (int i = 0; i < targetSubMachine.Length; i++)
            {
                if (targetSubMachine[i] != null && targetSubMachine[i].gameObject.activeInHierarchy)
                {
                    if (targetSubMachine[i].RunMachine)
                    {
                        ttrp += (float)Math.Round(targetSubMachine[i].RequestedEnergyAmount(), 2);
                    }
                }
            }
            netTotalRequiredPower.Value = ttrp;

            // Breaker Box power request to IRoutingSubstation
            if (bufferCurrent < energyBufferMax && RunMachine && mySubstations.Count > 0)
            {
                // Calculate request amount with proper clamping
                float requestAmount = Math.Min(totalRequiredPower, energyBufferMax - bufferCurrent);

                if (requestAmount > 0)
                {
                    float requestPerSubstation = requestAmount / mySubstations.Count;
                    foreach (IRoutingSubstation subs in mySubstations)
                    {
                        subs.RequestPowerFromSubstation(requestPerSubstation, thisBreaker);
                    }
                }
            }
            else if (!RunMachine)
            {
                lastReceived = 0f;
            }

            // Clamp buffer values
            if (bufferCurrent >= energyBufferMax)
            {
                netBufferCurrent.Value = energyBufferMax;
            }
            else if (bufferCurrent < 0.0f)
            {
                netBufferCurrent.Value = 0f;
            }

            // Calculate power deficit
            if (bufferCurrent < totalRequiredPower)
            {
                float deficit = totalRequiredPower - bufferCurrent;
                netDefecitVBreaker.Value = deficit / totalRequiredPower;
            }
            else
            {
                netDefecitVBreaker.Value = 0.0f;
            }

            // Monitor yellow power indicator lights
            for (int b = 0; b < yellowSwitchRenderers.Count; b++)
            {
                if (b < targetSubMachine.Length && targetSubMachine[b] != null && targetSubMachine[b].enabled)
                {
                    if (targetSubMachine[b].PowerMachine)
                    {
                        yellowSwitchRenderers[b].material = MaterialLibrary.GetPowerSystemStateMaterials(2); // yellow on
                    }
                    else
                    {
                        yellowSwitchRenderers[b].material = MaterialLibrary.GetPowerSystemStateMaterials(5); // yellow off
                    }
                }
                else
                {
                    yellowSwitchRenderers[b].material = MaterialLibrary.GetPowerSystemStateMaterials(5); // yellow off
                }
            }

            // Set the power level display of each metered submachine
            for (int p = 0; p < targetSubMachine.Length; p++)
            {
                if (targetSubMachine[p] != null && p < occupiedSwitches.Length && occupiedSwitches[p] != null)
                {
                    Transform scriptChild = occupiedSwitches[p].transform.GetChild(2);
                    if (scriptChild != null)
                    {
                        BreakerSwitch bs = scriptChild.GetComponent<BreakerSwitch>();
                        if (bs != null)
                        {
                            bs.SetPowerDisplay(targetSubMachine[p].LastEnergyReceived);
                        }
                    }
                }
            }
        }

        public void RequestPowerFromBreaker(float requestedAmount, ISubMachine thisSubMachine)
        {
            // Find the cable linking the breaker to the calling submachine
            foreach (ICable cable in iCableDLL)
            {
                if (cable.subMach == thisSubMachine)
                {
                    // Get machine's leg requirement
                    float machineLegReq = (float)thisSubMachine.GetLegRequirement();

                    // Split power between legs
                    float[] powerAmount = new float[((int)machineLegReq)];

                    for (int l = 0; l < machineLegReq; l++)
                    {
                        powerAmount[l] = requestedAmount / machineLegReq;
                        // Apply deficit reduction
                        powerAmount[l] -= (powerAmount[l] * defecitVbreaker);
                        powerAmount[l] = (float)Math.Round(powerAmount[l], 3);
                    }

                    // Recalculate the requested amount
                    requestedAmount = powerAmount[0] * machineLegReq;

                    if (cable.CheckConnection(5)) // Type is breaker to SubMachine linkage
                    {
                        if (bufferCurrent - requestedAmount >= 0)
                        {
                            cable.TransferIn((int)machineLegReq, powerAmount, 5);
                            bufferCurrent -= requestedAmount;
                        }
                        else if (bufferCurrent > 0)
                        {
                            // Transfer what remains in the buffer
                            float remainingPerLeg = bufferCurrent / machineLegReq;
                            float[] tempfloat = new float[(int)machineLegReq];
                            for (int i = 0; i < tempfloat.Length; i++)
                            {
                                tempfloat[i] = remainingPerLeg;
                            }

                            cable.TransferIn((int)machineLegReq, tempfloat, 5);
                            bufferCurrent = 0f;
                        }
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Method called by ISubMachines. Transfers power from the breaker buffer to the machine.
        /// 
        /// Allocate Legs as demanded
        /// </summary>
        public void RequestPowerFromBreaker_(float requestedAmount, ISubMachine thisSubMachine)
        {
            //find the cable linking the breaker to the calling submachine
            foreach (ICable cable in iCableDLL)
            {
                //Debug.Log("SubMachine tester:"+cable.subMach);
                //Debug.Log("SubMachine testee:" + thisSubMachine);
                if (cable.subMach == thisSubMachine)
                {
                    //JobLogger.Log("Request: "+requestedAmount);
                    //get machine's leg req
                    float machineLegReq = (float)thisSubMachine.GetLegRequirement();//cable.subMach
                    //split power between legs
                    float[] powerAmount = new float[((int)machineLegReq)];
                    //JobLogger.Log(machineLegReq);
                    //Debug.Log("dVb this update: "+defecitVbreaker);
                    for (int l = 0; l < machineLegReq; l++)
                    {
                        powerAmount[l] = requestedAmount / machineLegReq;
                        //remove the resultant multiplicant from defecitVbreaker from the amount of power to be sent, then round off to three places
                        powerAmount[l] -= (powerAmount[l] * defecitVbreaker);
                        //powerAmount[l] *= defecitVbreaker;
                        //Debug.Log("powerAmount after dVb: "+powerAmount[l]+" for "+powerAmount[l]* machineLegReq);
                        powerAmount[l] = (float)Math.Round(powerAmount[l], 3);
                    }
                    // Debug.Log("defecitVbreakerbox:" + defecitVbreaker);
                    //recalculate the requested about - all powerAmount[] indicies should be equvalent.
                    // BUG: purpose??
                    requestedAmount = powerAmount[0] * machineLegReq;
                    if (cable.CheckConnection(5))//type is breaker to SubMachine linkage
                    {
                        if (bufferCurrent >= requestedAmount)
                        {
                            //transfer the uniquely requested amount to the machine
                            //JobLogger.Log( powerAmount[0] * machineLegReq);
                            cable.TransferIn((int)machineLegReq, powerAmount, 5);
                            bufferCurrent -= requestedAmount;
                            //net was removed to allow for multithreading - need to flag for update
                            //netBufferCurrent.Value -= requestedAmount;
                            //JobLogger.Log(bufferCurrent + ", - " + requestedAmount);
                        }
                        else if (bufferCurrent > 0)
                        {
                            // BUG: should be based on leg req, not static
                            float[] tempfloat = new float[] { bufferCurrent / 3.0f, bufferCurrent / 3.0f, bufferCurrent / 3.0f };
                            //or transfer all that remains in the buffer
                            
                            cable.TransferIn((int)machineLegReq, tempfloat, 5);
                            bufferCurrent = 0f;
                            //net was removed to allow for multithreading - need to flag for update
                            //netBufferCurrent.Value = 0f;
                            //JobLogger.Log("bufferCurrent = " + 0f);
                        }
                    }
                    break;
                }
            }
        }

        //B
        public bool CheckMachineState(ref IRoutingSubstation thisSubstation)
        {
            if (!mySubstations.Contains(thisSubstation))
            {
                mySubstations.Add(thisSubstation);
            }
            return true;
        }

        //called at the start of the substation update block
        public bool CheckMachineState_(ref IRoutingSubstation thisSubstation)
        {
            if (!mySubstations.Contains(thisSubstation))
            {
                //Debug.Log("breaker added");
                mySubstations.Add(thisSubstation);
            }
            return true;
        }

        [ServerRpc(RequireOwnership = false)]
        public void SwitchToggleServerRpc(int numID)//, ref GameObject[] mySwitchLEDs)
        {
            SwitchToggleClientRpc(numID);//, ref mySwitchLEDs);
        }

        //B
        [ClientRpc]
        public void SwitchToggleClientRpc(int numID)
        {
            if (numID >= occupiedSwitches.Length || occupiedSwitches[numID] == null)
                return;

            // Get the switch animation controller
            SwitchAnimationController sac = occupiedSwitches[numID].transform.GetChild(4).GetComponent<SwitchAnimationController>();
            if (sac == null)
                return;

            GameObject[] mySwitchLEDs = { sac.GreenLED, sac.RedLED, sac.YellowLED };

            if (targetSubMachine[numID] != null && targetSubMachine[numID].gameObject.activeInHierarchy)
            {
                // Play switch sound
                if (soundsource != null)
                {
                    soundsource.volume = GlobalSettings.MasterVolume * GlobalSettings.SFXVolume;
                    soundsource.Play();
                }

                // Toggle machine state
                if (targetSubMachine[numID].RunMachine)
                {
                    targetSubMachine[numID].RunMachine = false;
                    // Update switch emissive to red
                    mySwitchLEDs[0].GetComponent<Renderer>().material = MaterialLibrary.GetPowerSystemStateMaterials(3); // green off
                    mySwitchLEDs[1].GetComponent<Renderer>().material = MaterialLibrary.GetPowerSystemStateMaterials(1); // red on
                }
                else
                {
                    targetSubMachine[numID].RunMachine = true;
                    // Update emissive to green
                    mySwitchLEDs[0].GetComponent<Renderer>().material = MaterialLibrary.GetPowerSystemStateMaterials(0); // green on
                    mySwitchLEDs[1].GetComponent<Renderer>().material = MaterialLibrary.GetPowerSystemStateMaterials(4); // red off
                }
            }
            else
            {
                Debug.Log("SubMachine Not Active In Scene");
                mySwitchLEDs[0].GetComponent<Renderer>().material = MaterialLibrary.GetPowerSystemStateMaterials(3); // green off
                mySwitchLEDs[1].GetComponent<Renderer>().material = MaterialLibrary.GetPowerSystemStateMaterials(4); // red off
                mySwitchLEDs[2].GetComponent<Renderer>().material = MaterialLibrary.GetPowerSystemStateMaterials(5); // yellow off
            }
        }

        [ClientRpc]
        public void SwitchToggle_ClientRpc(int numID)//, ref GameObject[] mySwitchLEDs)
        {
            //use the numid to get the switchAnimController and therefore access the mySwitchLEDs
            SwitchAnimationController sac = occupiedSwitches[numID].transform.GetChild(4).GetComponent<SwitchAnimationController>();
            GameObject[] mySwitchLEDs = { sac.GreenLED, sac.RedLED, sac.YellowLED };

            //targetSubMachine[numID] causes NullRef on some indexes even though the machine should exist
            //Debug.Log(numID);
            if (targetSubMachine[numID] != null && targetSubMachine[numID].enabled)
            {
                //play switch sound (It should not be set to loop).
                soundsource.volume *= GlobalSettings.MasterVolume * GlobalSettings.SFXVolume;
                soundsource.Play();
                //if the machines are not running, turn red emissive on. If running, green.
                if (targetSubMachine[numID].RunMachine)
                {
                    targetSubMachine[numID].RunMachine = false;
                    //update switch emissive to red
                    mySwitchLEDs[0].GetComponent<Renderer>().material = MaterialLibrary.GetPowerSystemStateMaterials(3);//green to off
                    mySwitchLEDs[1].GetComponent<Renderer>().material = MaterialLibrary.GetPowerSystemStateMaterials(1);//red to on
                }
                else
                {
                    targetSubMachine[numID].RunMachine = true;
                    //update emissive to green
                    mySwitchLEDs[0].GetComponent<Renderer>().material = MaterialLibrary.GetPowerSystemStateMaterials(0);//green on
                    mySwitchLEDs[1].GetComponent<Renderer>().material = MaterialLibrary.GetPowerSystemStateMaterials(4);//red off
                }
            }
            else
            {
                Debug.Log("SubMachine Not Active In Scene");
                mySwitchLEDs[0].GetComponent<Renderer>().material = MaterialLibrary.GetPowerSystemStateMaterials(3);//green to off
                mySwitchLEDs[1].GetComponent<Renderer>().material = MaterialLibrary.GetPowerSystemStateMaterials(4);//red off
                mySwitchLEDs[0].GetComponent<Renderer>().material = MaterialLibrary.GetPowerSystemStateMaterials(5);//yellow off
            }
        }

        //B
        public void ProxyStart()
        {
            if (targetSubMachine.Length > maxConnections)
            {
                ISubMachine[] temp = new ISubMachine[maxConnections];
                for (int i = 0; i < maxConnections; i++)
                {
                    temp[i] = targetSubMachine[i];
                }
                targetSubMachine = temp;
            }

            Debug.Log("SubMachine Array Length: " + targetSubMachine.Length);

            if (targetSubMachine.Length > 0)
            {
                // Create cables between breaker and machines
                for (int i = 0; i < targetSubMachine.Length; i++)
                {
                    if (targetSubMachine[i] != null)
                    {
                        ICable cable = new ICable(this, targetSubMachine[i]);
                        iCableDLL.AddLast(cable);
                        targetSubMachine[i].CheckMachineState(ref thisBreaker);
                        occupiedSwitches = new GameObject[i + 1];
                    }
                }

                // Button emissive logic
                int count = 0;
                GameObject[] allswitches = new GameObject[switchCount];

                foreach (Transform child in transform)
                {
                    if (child.gameObject.name.Contains("BreakerBox_Switch"))
                    {
                        allswitches[count] = child.gameObject;
                        if (count < occupiedSwitches.Length && count < 30)
                        {
                            occupiedSwitches[count] = child.gameObject;
                        }
                        count++;
                    }
                }

                // Disable all buttons after targetmachine.length
                for (int i = targetSubMachine.Length; i < allswitches.Length; i++)
                {
                    if (allswitches[i] != null)
                    {
                        foreach (Transform child in allswitches[i].transform)
                        {
                            if (child.gameObject.name == "BreakerBox_Yellow")
                            {
                                child.GetComponent<Renderer>().material = MaterialLibrary.GetPowerSystemStateMaterials(5); // yellow off
                            }
                            else if (child.gameObject.name == "BreakerBox_Red")
                            {
                                child.GetComponent<Renderer>().material = MaterialLibrary.GetPowerSystemStateMaterials(4); // red off
                            }
                            else if (child.gameObject.name == "BreakerBox_Green")
                            {
                                child.GetComponent<Renderer>().material = MaterialLibrary.GetPowerSystemStateMaterials(3); // green off
                            }
                            else if (child.gameObject.name == "BreakerSwitchScript")
                            {
                                child.gameObject.SetActive(false);
                            }
                        }
                    }
                }

                // For every machine, enable the yellow emissive
                for (int j = 0; j < targetSubMachine.Length; j++)
                {
                    if (targetSubMachine[j] != null && j < occupiedSwitches.Length && occupiedSwitches[j] != null)
                    {
                        foreach (Transform child in occupiedSwitches[j].transform)
                        {
                            if (child.gameObject.name == "BreakerBox_Yellow")
                            {
                                child.GetComponent<Renderer>().material = MaterialLibrary.GetPowerSystemStateMaterials(2);
                                yellowSwitchRenderers.Add(child.GetComponent<Renderer>());
                            }
                            else if (child.gameObject.name == "BreakerBox_Red")
                            {
                                child.GetComponent<Renderer>().material = MaterialLibrary.GetPowerSystemStateMaterials(4); // red off
                            }
                            else if (child.gameObject.name == "WSButton3")
                            {
                                child.gameObject.SetActive(true);
                            }
                        }
                    }
                }
            }
        }

        public void ProxyStart_()
        {
            if (targetSubMachine.Length > maxConnections)
            {
                ISubMachine[] temp = new ISubMachine[maxConnections];
                for (int i = 0; i < maxConnections; i++)
                {
                    temp[i] = targetSubMachine[i];
                }
                targetSubMachine = temp;
            }
            Debug.Log("SubMachine Array Length: " + targetSubMachine.Length);
            if (targetSubMachine.Length > 0)
            {
                //create a cable between breaker and machine/s
                for (int i = 0; i < targetSubMachine.Length; i++)
                {
                    if (targetSubMachine[i] != null)
                    {
                        ICable cable = new ICable(this, targetSubMachine[i]);
                        iCableDLL.AddLast(cable);
                        //Debug.Log("Checking Submachine State " + i);
                        targetSubMachine[i].CheckMachineState(ref thisBreaker);//ServerRpc
                        //add one cell to occupiedSwitches
                        occupiedSwitches = new GameObject[i + 1];
                        //Debug.Log("New occSch: " + occupiedSwitches.Length);
                    }
                }
                //button emiss logic
                int count = 0;
                GameObject[] allswitches = new GameObject[switchCount]; ;
                foreach (Transform child in transform)
                {
                    if (child.gameObject.name.Contains("BreakerBox_Switch"))
                    {
                        //Debug.Log("Adding index to all switches. Index: "+count);
                        allswitches[count] = child.gameObject;
                        //Debug.Log(child.gameObject.name + " "+ count);
                        if (count < occupiedSwitches.Length && count < 30)
                        {
                            occupiedSwitches[count] = child.gameObject;
                            //netOccupiedSwitches[count] = child.gameObject;//probs won't work
                        }
                        count++;
                    }
                }

                //disable all button after targetmachine.length
                for (int i = targetSubMachine.Length; i < allswitches.Length; i++)//occupied switches OR switchcount
                {
                    //Debug.Log(i);
                    foreach (Transform child in allswitches[i].transform)//nullRef
                    {
                        //get yellow mesh
                        if (child.gameObject.name == "BreakerBox_Yellow")
                        {
                            child.GetComponent<Renderer>().material = MaterialLibrary.GetPowerSystemStateMaterials(5);//yellow off
                        }
                        //turn off r and g
                        else if (child.gameObject.name == "BreakerBox_Red")
                        {
                            child.GetComponent<Renderer>().material = MaterialLibrary.GetPowerSystemStateMaterials(4);//red off
                        }
                        else if (child.gameObject.name == "BreakerBox_Green")
                        {
                            //Debug.Log(child.GetComponent<Renderer>().material.ToString());
                            child.GetComponent<Renderer>().material = MaterialLibrary.GetPowerSystemStateMaterials(3);//green off
                        }
                        //disable wsbutton3
                        else if (child.gameObject.name == "BreakerSwitchScript")
                        {
                            child.gameObject.SetActive(false);
                        }
                    }
                }

                //for every machine, enable the yellow emissive and enable WSButton3?
                for (int j = 0; j < targetSubMachine.Length; j++)
                {
                    if (targetSubMachine[j] != null)
                    {
                        foreach (Transform child in occupiedSwitches[j].transform)
                        {
                            //get yellow mesh
                            if (child.gameObject.name == "BreakerBox_Yellow")
                            {
                                child.GetComponent<Renderer>().material = MaterialLibrary.GetPowerSystemStateMaterials(2);
                                yellowSwitchRenderers.Add(child.GetComponent<Renderer>());
                            }
                            else if (child.gameObject.name == "BreakerBox_Red")
                            {
                                child.GetComponent<Renderer>().material = MaterialLibrary.GetPowerSystemStateMaterials(4);//red off
                            }
                            //enable wsbutton3
                            else if (child.gameObject.name == "WSButton3")
                            {
                                child.gameObject.SetActive(true);
                            }
                        }
                    }
                }
            }
        }

        public void SetMachines(ISubMachine[] newSubMachines)
        {
            targetSubMachine = newSubMachines;
        }

        public int GetLegRequirement()
        {
            return legsRequired;
        }

        //B
        public void ReceivePowerFromSubstation(int legCount, float[] amounts)
        {
            if (amounts == null || amounts.Length == 0)
                return;

            float totalReceived = 0f;
            for (int i = 0; i < legCount && i < amounts.Length; i++)
            {
                totalReceived += amounts[i];
            }

            netBufferCurrent.Value += totalReceived;

            if (netBufferCurrent.Value > energyBufferMax)
            {
                netBufferCurrent.Value = energyBufferMax;
            }
            else
            {
                netBufferCurrent.Value = (float)Math.Round(netBufferCurrent.Value, 3);
            }

            lastReceived = totalReceived;
            netLegsReceived.Value = legCount;
        }

        public void ReceivePowerFromSubstation_(int legCount, float[] amounts)
        {
            //Debug.Log("Breaker Received Power");
            //receive X legs with X amounts
            //Debug.Log(netBufferCurrent.Value + " += " + amounts[0] * legCount);
            for (int i = 0; i < legCount; i++)
            {
                //Debug.Log(legCount);
                //bufferCurrent += amounts[i];
                netBufferCurrent.Value += amounts[i];
            }
            if (amounts.Length != 0)
            {
                lastReceived = amounts[0] * legCount;
            }
            netLegsReceived.Value = legCount;
            if (netBufferCurrent.Value > energyBufferMax)
            {
                netBufferCurrent.Value = energyBufferMax;
            }
            else
            {
                netBufferCurrent.Value = (float)Math.Round(netBufferCurrent.Value, 3);
            }
        }

        public float GetTotalRequiredPower()
        {
            return totalRequiredPower;
        }

        public Guid GetGUID()
        {
            return guid;
        }


        public override void OnDestroy()
        {
            Resources.UnloadUnusedAssets();
            base.OnDestroy();
        }
    }
}