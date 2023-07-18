using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ProjectUniverse.Data.Libraries;
using MLAPI.NetworkVariable;
using MLAPI.NetworkVariable.Collections;
using MLAPI.Messaging;
using MLAPI;
using ProjectUniverse.Animation.Controllers;
using ProjectUniverse.Environment.Interactable;

namespace ProjectUniverse.PowerSystem
{
    /*
     * The purpose of this class is to distribute power to large amounts of small IMachines (not unlike IRoutingSubtation, save simpler).
     */
    public sealed class IBreakerBox : MonoBehaviour//NetworkBehaviour
    {
        private Guid guid;
        //power group or machine this unit provides power to.
        public ISubMachine[] targetSubMachine;
        //private float[] requestedPower;
        [SerializeField] private float totalRequiredPower;
        private IBreakerBox thisBreaker;
        private LinkedList<ICable> iCableDLL = new LinkedList<ICable>();
        private float energyBufferMax;
        [SerializeField] private float bufferCurrent;
        private readonly int maxConnections = 30;
        [SerializeField] private GameObject[] occupiedSwitches;
        public int switchCount;
        private float defecitVbreaker;
        private List<IRoutingSubstation> mySubstations = new List<IRoutingSubstation>();
        private List<Renderer> yellowSwitchRenderers = new List<Renderer>();
        [SerializeField] AudioSource soundsource;
        //power legs update
        private int legsRequired = 3;
        private int legsReceived;
        //Network variable
        private NetworkVariableFloat netTotalRequiredPower = new NetworkVariableFloat(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.Everyone });
        private NetworkVariableFloat netEnergyBufferMax = new NetworkVariableFloat(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.Everyone });
        private NetworkVariableFloat netBufferCurrent = new NetworkVariableFloat(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.Everyone });
        private NetworkVariableInt netSwitchCount = new NetworkVariableInt(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.Everyone });
        private NetworkVariableFloat netDefecitVBreaker = new NetworkVariableFloat(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.Everyone });
        private NetworkVariableInt netLegsRequired = new NetworkVariableInt(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.Everyone });
        private NetworkVariableInt netLegsReceived = new NetworkVariableInt(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.Everyone });
        //private NetworkList<GameObject> netOccupiedSwitches = new NetworkList<GameObject>(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.Everyone });
        [SerializeField] private bool runMachine = true;
        private float lastReceived;

        public bool RunMachine
        {
            get
            {
                return runMachine;
            }
            set
            {
                runMachine = value;
            }
        }

        public float LastReceived
        {
            get
            {
                return lastReceived;
            }
        }

        public float BufferCurrent
        {
            get { return bufferCurrent; }
        }

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
        }

        private void NetworkListeners()
        {
            //set starting values
            netTotalRequiredPower.Value = totalRequiredPower;
            netEnergyBufferMax.Value = energyBufferMax;
            netBufferCurrent.Value = bufferCurrent;
            netSwitchCount.Value = switchCount;
            netDefecitVBreaker.Value = defecitVbreaker;
            netLegsReceived.Value = legsReceived;
            netLegsRequired.Value = legsRequired;
            //for(int i = 0; i < occupiedSwitches.Length; i++)
            //{
            //    netOccupiedSwitches.Add(occupiedSwitches[i]);
           // }
            
            //Establish events
            netTotalRequiredPower.OnValueChanged += delegate { totalRequiredPower = netTotalRequiredPower.Value; };
            netEnergyBufferMax.OnValueChanged += delegate { energyBufferMax = netEnergyBufferMax.Value; };
            netBufferCurrent.OnValueChanged += delegate { bufferCurrent = netBufferCurrent.Value; };
            netSwitchCount.OnValueChanged += delegate { switchCount = netSwitchCount.Value; };
            netDefecitVBreaker.OnValueChanged += delegate { defecitVbreaker = netDefecitVBreaker.Value; };
            netLegsRequired.OnValueChanged += delegate { legsRequired = netLegsRequired.Value; };
            netLegsReceived.OnValueChanged += delegate { legsReceived = netLegsReceived.Value; };
            //netOccupiedSwitches.OnListChanged += delegate { 
                
            //};
        }

        void Update()
        {
            //Debug.Log("===============================");
            netTotalRequiredPower.Value = 0f;
            //if (bufferCurrent < 0.0f)
            //{
           //     netBufferCurrent.Value = 0f;
            //}
            //int numSuppliers = 0;
            //requestedPower = new float[targetSubMachine.Length];
            //Uses the number and type of connections to predict how much power to allocate to each SubMachine.
            for (int i = 0; i < targetSubMachine.Length; i++)
            {
                if (targetSubMachine[i] != null)
                {
                    if (targetSubMachine[i].RunMachine) 
                    { 
                        netTotalRequiredPower.Value += (float)Math.Round(targetSubMachine[i].RequestedEnergyAmount(),2);
                    }
                }
            }
            //Breaker Box power request to IRoutingSubstation
            if (bufferCurrent < energyBufferMax)
            {
                if (RunMachine)
                {
                    //determine amount to request
                    if (bufferCurrent + totalRequiredPower >= energyBufferMax)
                    {
                        netTotalRequiredPower.Value = energyBufferMax - bufferCurrent;//enough to fill the buffer
                    }

                    float requestPerSubstation = (totalRequiredPower / mySubstations.Count);
                    foreach (IRoutingSubstation subs in mySubstations)
                    {
                        subs.RequestPowerFromSubstation(requestPerSubstation, thisBreaker);
                    }
                }
                else
                {
                    lastReceived = 0f;
                }
            }
            else if (bufferCurrent >= energyBufferMax)
            {
                //Debug.Log("Overcap: "+bufferCurrent);
                //bufferCurrent = energyBufferMax;
                netBufferCurrent.Value = netEnergyBufferMax.Value;
            }
            if (bufferCurrent < 0.0f)
            {
                netBufferCurrent.Value = 0f;
            }
            //power will be divided equally among linked machines.
            if (bufferCurrent < totalRequiredPower)
            {
                float defecit = totalRequiredPower - bufferCurrent;
                netDefecitVBreaker.Value = defecit / totalRequiredPower;
                //netDefecitVBreaker.Value = 1.0f;
            }
            else
            {
                //defecitVbreaker = 0.0f;
                netDefecitVBreaker.Value = 0.0f;
            }
            //monitor the yellow power indicator lights
            for (int b = 0; b < yellowSwitchRenderers.Count; b++)
            {
                if (targetSubMachine[b] != null && targetSubMachine[b].enabled)
                {
                    //Debug.Log(targetSubMachine[b] +" pwr: "+targetSubMachine[b].PowerMachine);
                    if (targetSubMachine[b].PowerMachine)
                    {
                        yellowSwitchRenderers[b].material = MaterialLibrary.GetPowerSystemStateMaterials(2);//yellow on
                    }
                    else
                    {
                        yellowSwitchRenderers[b].material = MaterialLibrary.GetPowerSystemStateMaterials(5);
                    }
                }
                else
                {
                    //Debug.Log("AAAAAAAAAAAA");
                    yellowSwitchRenderers[b].material = MaterialLibrary.GetPowerSystemStateMaterials(5);
                    //mySwitchLEDs[0].GetComponent<Renderer>().material = MaterialLibrary.GetPowerSystemStateMaterials(3);//green to off
                    //mySwitchLEDs[1].GetComponent<Renderer>().material = MaterialLibrary.GetPowerSystemStateMaterials(4);//red off
                    //mySwitchLEDs[0].GetComponent<Renderer>().material = MaterialLibrary.GetPowerSystemStateMaterials(5);//yellow off
                }
            }
            //set the powerlevel of each metered submachine
            for (int p = 0; p < targetSubMachine.Length; p++)
            {
                if (targetSubMachine[p] != null && occupiedSwitches[p] != null)
                {
                    BreakerSwitch bs = occupiedSwitches[p].transform.GetChild(2).GetComponent<BreakerSwitch>();
                    bs.SetPowerDisplay(targetSubMachine[p].LastEnergyReceived);
                }
            }

        }

        /// <summary>
        /// Method called by ISubMachines. Transfers power from the breaker buffer to the machine.
        /// 
        /// Allocate Legs as demanded
        /// </summary>
        public void RequestPowerFromBreaker(float requestedAmount, ISubMachine thisSubMachine)
        {
            //find the cable linking the breaker to the calling submachine
            foreach (ICable cable in iCableDLL)
            {
                //Debug.Log("SubMachine tester:"+cable.subMach);
                //Debug.Log("SubMachine testee:" + thisSubMachine);
                if (cable.subMach == thisSubMachine)
                {
                    //Debug.Log(thisSubMachine.gameObject.name + " requests "+requestedAmount);
                    //get machine's leg req
                    int machineLegReq = cable.subMach.GetLegRequirement();
                    //split power between legs
                    float[] powerAmount = new float[machineLegReq];
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
                    requestedAmount = powerAmount[0] * machineLegReq;
                    if (cable.CheckConnection(5))//type is breaker to SubMachine linkage
                    {
                        if (bufferCurrent - requestedAmount >= 0)
                        {
                            //transfer the uniquely requested amount to the machine
                            
                            cable.TransferIn(machineLegReq, powerAmount, 5);
                            //bufferCurrent -= requestedAmount;
                            netBufferCurrent.Value -= requestedAmount;
                            //Debug.Log(netBufferCurrent.Value + ", - " + requestedAmount);
                        }
                        else if (bufferCurrent - requestedAmount < 0)
                        {
                            float[] tempfloat = new float[] { bufferCurrent / 3.0f, bufferCurrent / 3.0f, bufferCurrent / 3.0f };
                            //or transfer all that remains in the buffer
                            
                            cable.TransferIn(machineLegReq, tempfloat, 5);
                            //bufferCurrent = 0f;
                            netBufferCurrent.Value = 0f;
                            //Debug.Log("bufferCurrent = " + bufferCurrent);
                        }
                    }
                    break;
                }
            }
        }

        //called at the start of the substation update block
        public bool CheckMachineState(ref IRoutingSubstation thisSubstation)
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

        [ClientRpc]
        public void SwitchToggleClientRpc(int numID)//, ref GameObject[] mySwitchLEDs)
        {
            //Debug.Log("Attempt to get LEDs from occupiedSwitches");
            //use the numid to get the switchAnimController and therefore access the mySwitchLEDs
            SwitchAnimationController sac = occupiedSwitches[numID].transform.GetChild(4).GetComponent<SwitchAnimationController>();
            GameObject[] mySwitchLEDs = { sac.GreenLED, sac.RedLED, sac.YellowLED };

            //targetSubMachine[numID] causes NullRef on some indexes even though the machine should exist
            //Debug.Log(numID);
            if (targetSubMachine[numID] != null && targetSubMachine[numID].enabled)
            {
                //play switch sound (It should not be set to loop).
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
            /*
            //if the machines are not powered, turn yellow emissive off. If running, on.
            if (targetSubMachine[numID].PowerMachine == true)
            {
                //update switch emissive to on
                mySwitchLEDs[0].GetComponent<Renderer>().material = MaterialLibrary.GetPowerSystemStateMaterials(2);//yellow on?
            }
            else
            {
                //update emissive to off
                mySwitchLEDs[0].GetComponent<Renderer>().material = MaterialLibrary.GetPowerSystemStateMaterials(5);//yellow off
            }*/
        }

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
                //update netOccupiedSwitches
                //netOccupiedSwitches.Clear();
                //for (int i = 0; i < occupiedSwitches.Length; i++)
                //{
                //    netOccupiedSwitches.Add(occupiedSwitches[i]);
                //}
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

        public void ReceivePowerFromSubstation(int legCount, float[] amounts)
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
            //legsReceived = legCount;
            netLegsReceived.Value = legCount;
            if (netBufferCurrent.Value > energyBufferMax)
            {
                netBufferCurrent.Value = energyBufferMax;
            }
            else
            {
                netBufferCurrent.Value = (float)Math.Round(netBufferCurrent.Value, 3);
            }
            //Debug.Log("bufferCurrent: " + netBufferCurrent.Value);
            //bufferCurrent = (float)Math.Round(bufferCurrent, 3);
        }

        public float GetTotalRequiredPower()
        {
            return totalRequiredPower;
        }

        public Guid GetGUID()
        {
            return guid;
        }
    }
}