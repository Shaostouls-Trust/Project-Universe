using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectUniverse.Data.Libraries;
using ProjectUniverse.Animation.Controllers;
using Unity.Netcode;
using UnityEngine.PlayerLoop;
namespace ProjectUniverse.PowerSystem
{
    public sealed class ISubMachine : NetworkBehaviour
    {
        //Amount required (requested) to run machine
        [SerializeField] private float requestedEnergy;
        //unadjusted amount required to run the machine
        [SerializeField] private float requiredEnergy;
        [SerializeField] private int percentDrawToFill;
        //amount to draw when filling the interal buffer
        private float drawToFill;
        [SerializeField] private float energyBuffer; //Machines shouldn't store more than 10 frames worth of power.
        [SerializeField]
        private float bufferCurrent;
        [SerializeField] private string machineType;
        [SerializeField] private bool runMachine;
        private bool isPowered;
        //private ICable cable;
        private List<IBreakerBox> breakers = new List<IBreakerBox>();
        //backend of power cables
        private LinkedList<ICable> iCableDLL = new LinkedList<ICable>();
        private MeshRenderer renderer;
        [SerializeField] private Light lightComponent;
        private float maxLightIntensity;
        private float maxLightRange;
        //power legs update
        [SerializeField]
        private int legsRequired;
        private int legsReceived;
        //network vars
        private NetworkVariable<float> netRequestedEnergy = new NetworkVariable<float>();//new NetworkVariableSettings { WritePermission = NetworkVariablePermission.Everyone }
        private NetworkVariable<float> netRequiredEnergy = new NetworkVariable<float>();
        private NetworkVariable<float> netEnergyBuffer = new NetworkVariable<float>();
        private NetworkVariable<float> netBufferCurrent = new NetworkVariable<float>();
        private NetworkVariable<bool> netIsPowered = new NetworkVariable<bool>();
        private NetworkVariable<bool> netRunMachine = new NetworkVariable<bool>();
        private NetworkVariable<float> netMaxLightIntensity = new NetworkVariable<float>();
        private NetworkVariable<float> netMaxLightRange = new NetworkVariable<float>();
        private NetworkVariable<int> netLegsRequired = new NetworkVariable<int>();
        private NetworkVariable<int> netLegsReceived = new NetworkVariable<int>();
        private NetworkVariable<bool> netLightEnabled = new NetworkVariable<bool>();

        //B
        private int previousRunState = -1;
        private Material[] powerStateMaterials; // 0=full, 1=75%, 2=50%, 3=low, 4=off
        private int currentMaterialIndex = -1;

        //anti-spaz timer
        private float chillTime = 7f;
        private float lastEnergyReceived = 0f;
        [SerializeField] private SubMachineJobs SMJobs;
        private int runStateFromJob = 4;

        public override void OnNetworkSpawn()
        {
            if (IsServer || IsHost)
            {
                //set starting values
                netRequestedEnergy.Value = requestedEnergy;
                netRequiredEnergy.Value = requiredEnergy;
                netEnergyBuffer.Value = energyBuffer;
                netBufferCurrent.Value = bufferCurrent;
                netIsPowered.Value = isPowered;
                netRunMachine.Value = runMachine;
                netMaxLightIntensity.Value = maxLightIntensity;
                netMaxLightRange.Value = maxLightRange;
                netLegsReceived.Value = legsReceived;
                netLegsRequired.Value = legsRequired;
                if (lightComponent != null)
                {
                    netLightEnabled.Value = lightComponent.enabled;
                }
                base.OnNetworkSpawn();
            }
        }

        private void Awake()
        {
            //subscribe to VA Jobs manager
            if (SMJobs != null)
            {
                SMJobs.AddMachine(this);
            }
        }

        void Start()
        {
            //RunMachine = true;
            bufferCurrent = 0.0f;
            //get light mesh renderer
            renderer = GetComponent<MeshRenderer>();
            if (renderer == null)
            {
                renderer = GetComponentInChildren<MeshRenderer>();
            }
            if(lightComponent == null)
            {
                lightComponent = this.gameObject.GetComponentInChildren<Light>();
            }
            if(lightComponent != null)
            {
                maxLightIntensity = lightComponent.intensity;
                maxLightRange = lightComponent.range;
            }
            //if(isHost){
            //}?
            NetworkListeners();

            //B
            //Pre-bake materials with different emission intensities
            if (powerStateMaterials == null || powerStateMaterials.Length == 0)
            {
                CreatePowerStateMaterials();
            }
        }

        //B
        private void CreatePowerStateMaterials()
        {
            Material baseMat = renderer.sharedMaterial;
            powerStateMaterials = new Material[5];

            float[] emissionLevels = { 50f, 40f, 25f, 10f, 0f };

            for (int i = 0; i < 5; i++)
            {
                powerStateMaterials[i] = new Material(baseMat);
                powerStateMaterials[i].SetFloat("_EmissionIntensity", emissionLevels[i]);
                //BUG: On light material change, lots of lag??

                // Should do/use?:
                //renderer.GetPropertyBlock(MPB);
                //MPB.SetFloat("_EmissionIntensity", 25f);//50f is current emissive level for lights
                //renderer.SetPropertyBlock(MPB);
            }
        }

        //B
        public void SetPowerState(int state)
        {
            int materialIndex = Mathf.Clamp(state, 0, 4);

            if (materialIndex != currentMaterialIndex)
            {
                try
                {
                    renderer.sharedMaterial = powerStateMaterials[materialIndex];
                }catch(IndexOutOfRangeException e)
                {
                    Debug.Log("Anomalous: "+e);
                }
                currentMaterialIndex = materialIndex;
            }
        }

        private void NetworkListeners()
        {
            //Establish events
            netRequestedEnergy.OnValueChanged += delegate { requestedEnergy = netRequestedEnergy.Value; };
            netRequiredEnergy.OnValueChanged += delegate { requiredEnergy = netRequiredEnergy.Value; };
            netEnergyBuffer.OnValueChanged += delegate { energyBuffer = netEnergyBuffer.Value; };
            netBufferCurrent.OnValueChanged += delegate { bufferCurrent = netBufferCurrent.Value; };
            netIsPowered.OnValueChanged += delegate { isPowered = netIsPowered.Value; };
            netRunMachine.OnValueChanged += delegate { runMachine = netRunMachine.Value; };
            netMaxLightIntensity.OnValueChanged += delegate { maxLightIntensity = netMaxLightIntensity.Value; };
            netMaxLightRange.OnValueChanged += delegate { maxLightRange = netMaxLightRange.Value; };
            netLegsRequired.OnValueChanged += delegate { legsRequired = netLegsRequired.Value; };
            netLegsReceived.OnValueChanged += delegate { legsReceived = netLegsReceived.Value; };
            if (lightComponent != null)
            {
                netLightEnabled.OnValueChanged += delegate { lightComponent.enabled = netLightEnabled.Value; };
            }
        }
        
        public float LastEnergyReceived
        {
            get { return lastEnergyReceived; }
        }

        public float RequiredEnergy
        {
            get { return requiredEnergy; }
        }
        public float BufferCurrent
        {
            get { return bufferCurrent; }
        }
        public float EnergyBuffer
        {
            get { return energyBuffer; }
        } 
        public int PercentDrawToFill
        {
            get { return percentDrawToFill; }
        }
        public float Timer
        {
            get { return chillTime; }
            set { chillTime = value; }
        }
        public int LegsReceived
        {
            get { return legsReceived; }
        }
        public int LegsRequired
        {
            get { return legsRequired; }
        }
        public List<IBreakerBox> Breakers
        {
            get { return breakers; }
        }
        public string MachineType
        {
            get { return machineType; }
        }

        private void Update()
        {
            RunMachineSelector(machineType, runStateFromJob);
        }

        public void SetJobResults(SubMachineJobs.SubMachineResults result)
        {
            // Update state from job
            requestedEnergy = result.netAskEng;
            bufferCurrent = result.newBuffer; // This is the buffer AFTER consumption
            isPowered = result.powered;
            chillTime = result.newTimer;
            runStateFromJob = result.state;
        }

        private void UpdateNetworkVariables()
        {
            if (IsServer)
            {
                netBufferCurrent.Value = bufferCurrent;
                netIsPowered.Value = isPowered;
                netRequestedEnergy.Value = requestedEnergy;
            }
        }

        public bool GetRunMachine()
        {
            return netRunMachine.Value;//runMachine;
        }

        public void RequestHelper()
        {
            if (RunMachine)
            {
                foreach (IBreakerBox box in breakers)
                {
                    //Debug.Log("request from breakers: "+requestedEnergy/breakers.Count);
                    box.RequestPowerFromBreaker(requestedEnergy / breakers.Count, this);//this.GetComponent<ISubMachine>()
                }
            }
            else
            {
                netRequestedEnergy.Value = 0f;
            }
        }

        public void RunLogic()
        {
            ///////////////////////////////////////
            //Run logic
            ///////////////////////////////////////
            chillTime--;
            if (chillTime < 0f)
            {
                chillTime = 7f;
            }
            if (runMachine)
            {
                if (legsReceived == legsRequired)
                {
                    //Debug.Log("Legs received");
                    if (bufferCurrent > 0f)
                    {
                        netIsPowered.Value = true;
                        //isPowered = true;
                        if (bufferCurrent - requiredEnergy < 0.0f)//not enough power to run at full
                        {
                            if (bufferCurrent >= requiredEnergy * 0.75f)//75% power
                            {
                                ///return type and int from job to handle this in main thread
                                RunMachineSelector(machineType, 1); //any slower locks emiss to blinking yellow.
                            }
                            else if (bufferCurrent >= requiredEnergy * 0.5f)//no lower than 50%
                            {
                                RunMachineSelector(machineType, 2);
                            }
                            else//lower than 50%
                            {
                                RunMachineSelector(machineType, 3);
                            }
                            //no matter what, the buffer is emptied
                            netBufferCurrent.Value = 0.0f;
                            //bufferCurrent = 0.0f;
                        }
                        else
                        {
                            //run full power
                            RunMachineSelector(machineType, 0);
                            //bufferCurrent -= requiredEnergy;
                            netBufferCurrent.Value -= requiredEnergy;
                        }
                    }
                    else
                    {
                        netIsPowered.Value = false;
                        //isPowered = false;
                        //'run' at 0 power
                        RunMachineSelector(machineType, 4);
                    }
                }
                else if (legsReceived < legsRequired && legsReceived >= 1)
                {
                    //Shut down machine due to leg requirement
                    RunMachineSelector(machineType, 4);
                    //electrical damage (if the buffer is not empty)
                    //if (bufferCurrent > 0)
                    //{
                    //NYI
                    //}
                }
                else
                {
                    //Shut down machine due to leg requirement
                    RunMachineSelector(machineType, 4);
                    //NO electrical damage, because no legs attached.
                }
            }
            else
            {
                RunMachineSelector(machineType, 5);
            }

        }

        public int GetLegRequirement()
        {
            return legsRequired;
        }

        public bool RunMachine
        {
            get { return runMachine; }
            set { netRunMachine.Value = value; }
        }

        public bool PowerMachine
        {
            get { return isPowered; }
        }

        public float RequestedEnergyAmount()
        {
            if (iCableDLL.Count > 1)
            {
                //correct for multiple inputs
                return requestedEnergy / iCableDLL.Count;
            }
            else
            {
                return requestedEnergy;
            }
        }

        public void ReceiveEnergyAmount(int legCount, float[] amounts, ref ICable cable)
        {
            /// net updates removed for multithreading - need to flag for update
            //receive X legs with X amounts
            for (int i = 0; i < legCount; i++)
            {
                //netBufferCurrent.Value += amounts[i];
                bufferCurrent += amounts[i];
            }
            lastEnergyReceived = amounts[0] * legCount;
            //Debug.Log(this + " submachine buffer at: " + bufferCurrent);
            //netLegsReceived.Value = legCount;
            legsReceived = legCount;
            //Debug.Log("submachine has "+legsReceived+" legs");
            //round buffer current to 3 places to avoid having a psychotic meltdown
            //netBufferCurrent.Value = (float)Math.Round(netBufferCurrent.Value, 3);
            bufferCurrent = (float)Math.Round(bufferCurrent, 3);
            if (!iCableDLL.Contains(cable))
            {
                //netICableDLL.Add(cable);
                iCableDLL.AddLast(cable);
            }
            if (bufferCurrent > energyBuffer)
            {
                //trim off excess power. Buffers cannot overcharge
                //netBufferCurrent.Value = energyBuffer;
                bufferCurrent = energyBuffer;
            }
        }
        /*
        [ServerRpc(RequireOwnership = false)]
        public void RemoveCableConnectionServerRpc(ICable cable)
        {
            RemoveCableConnectionClientRpc(cable);
        }
        //called on cable disconnect (NYI)
        [ClientRpc]
        public void RemoveCableConnectionClientRpc(ICable cable)
        {
            //netICableDLL.Remove(cable);
            iCableDLL.Remove(cable);
        }*/

        //[ServerRpc(RequireOwnership = false)]
       // public void CheckMachineStateServerRpc(ref IBreakerBox myBreaker)
       // {
      //      CheckMachineStateClientRpc(ref myBreaker);
      //  }

        //called at the start of the breaker update block
        //[ClientRpc]
        public bool CheckMachineState(ref IBreakerBox myBreaker)//ClientRpc
        {
            if (!breakers.Contains(myBreaker))
            {
                //Debug.Log("breaker added");
                //netBreakers.Add(myBreaker);
                breakers.Add(myBreaker);//NEED TO BE ABLE TO SYNC CHANGES HERE
            }
            return true;
        }

        

        public void RunMachineSelector(string ImachineType, int powerLevel)
        {
            //SendMessage("runSubMachine", powerLevel, SendMessageOptions.DontRequireReceiver);
          //  /*
            switch (ImachineType)
            {
                case "light_point":
                    //this.RunMachinePointLightServerRpc(powerLevel);
                    this.RunMachinePointLightStandAlone(powerLevel);
                    break;
                case "door":
                    //this.gameObject.GetComponent<DoorAnimator>().runSubMachine(powerLevel);
                    this.gameObject.GetComponent<DoorAnimator>().runSubMachine(powerLevel);
                        //RunSubmachineServerRpc(powerLevel);//runSubMachine
                    break;
            }//*/
        }

        [ServerRpc(RequireOwnership = false)]
        public void RunMachinePointLightServerRpc(int powerLevel)
        {
            if(chillTime <= 0f)
            {
                netLightEnabled.Value = true;
                MaterialPropertyBlock MPB = MaterialLibrary.GetMaterialPropertyBlockForCommonLights();
                switch (powerLevel)
                {
                    //base is 100.0f
                    //base is 5.0f
                    case 0:
                        lightComponent.intensity = maxLightIntensity;
                        lightComponent.range = maxLightRange;
                        //set material emissive to default
                        //MaterialPropertyBlock to manage the emissive material values for all our common lights
                        renderer.GetPropertyBlock(MPB);
                        MPB.SetFloat("_EmissionIntensity", 50f);//50f is current emissive level for lights
                        renderer.SetPropertyBlock(MPB);

                        //netBufferCurrent.Value -= netRequiredEnergy.Value;
                        //if(bufferCurrent <= 0f)
                        //{
                        //    bufferCurrent = 0f;
                        //}
                        break;
                    case 1:
                        lightComponent.intensity = maxLightIntensity * 0.5f; //50
                        lightComponent.range = maxLightRange * 0.75f; //3.75
                                                                      //set material emissive to 50%
                        renderer.GetPropertyBlock(MPB);
                        MPB.SetFloat("_EmissionIntensity", 40f);//50f is current emissive level for lights
                        renderer.SetPropertyBlock(MPB);

                        //netBufferCurrent.Value = 0f;
                        break;
                    case 2:
                        lightComponent.intensity = maxLightIntensity * UnityEngine.Random.Range(0.35f, 0.25f);//35 - 25
                        lightComponent.range = maxLightRange * UnityEngine.Random.Range(0.5f, 0.6f);//6.0f; (2.5 to 3)
                                                                                                    //set material emissive to 35%
                        renderer.GetPropertyBlock(MPB);
                        MPB.SetFloat("_EmissionIntensity", 25f);//50f is current emissive level for lights
                        renderer.SetPropertyBlock(MPB);

                        //netBufferCurrent.Value = 0f;
                        break;
                    case 3:
                        lightComponent.intensity = maxLightIntensity * UnityEngine.Random.Range(0.05f, 0.1f);//5 - 10
                        lightComponent.range = maxLightRange * UnityEngine.Random.Range(0.2f, 0.30f); //4.0f; (1 to 1.5)
                                                                                                      //set material emissive to 10%
                        renderer.GetPropertyBlock(MPB);
                        MPB.SetFloat("_EmissionIntensity", 10f);//50f is current emissive level for lights
                        renderer.SetPropertyBlock(MPB);

                        //netBufferCurrent.Value = 0f;
                        break;
                    case 4:
                        lightComponent.intensity = 0.0f;
                        //set material emissive to 0%
                        renderer.GetPropertyBlock(MPB);
                        MPB.SetFloat("_EmissionIntensity", 0f);//50f is current emissive level for lights
                        renderer.SetPropertyBlock(MPB);
                        break;
                    case 5:
                        netLightEnabled.Value = false;
                        //lightComponent.intensity = 0.0f;
                        //set material emissive to 0%
                        renderer.GetPropertyBlock(MPB);
                        MPB.SetFloat("_EmissionIntensity", 0f);//50f is current emissive level for lights
                        renderer.SetPropertyBlock(MPB);
                        break;
                }
            }
            
        }

        private void RunMachinePointLightStandAlone(int powerLevel)
        {
            if (chillTime > 0f || lightComponent == null) return;

            //B
            SetPowerState(powerLevel);

            switch (powerLevel)
            {
                case 0:
                    SetLightProperties(maxLightIntensity, maxLightRange, true);
                    break;
                case 1:
                    SetLightProperties(maxLightIntensity * 0.5f, maxLightRange * 0.75f, true);
                    break;
                case 2:
                    SetLightProperties(
                        maxLightIntensity * UnityEngine.Random.Range(0.25f, 0.35f),
                        maxLightRange * UnityEngine.Random.Range(0.5f, 0.6f),
                        true);
                    break;
                case 3:
                    SetLightProperties(
                        maxLightIntensity * UnityEngine.Random.Range(0.05f, 0.1f),
                        maxLightRange * UnityEngine.Random.Range(0.2f, 0.3f),
                        true);
                    break;
                case 4:
                    SetLightProperties(0f, maxLightRange, true);
                    break;
                case 5:
                    SetLightProperties(0f, maxLightRange, false);
                    break;
            }
        }

        private void SetLightProperties(float intensity, float range, bool enabled)
        {
            lightComponent.intensity = intensity;
            lightComponent.range = range;
            if (IsServer) netLightEnabled.Value = enabled;
        }

        private void RunMachinePointLightStandAlone_(int powerLevel)
        {
            if (chillTime <= 0f)
            {
                netLightEnabled.Value = true;
                MaterialPropertyBlock MPB = MaterialLibrary.GetMaterialPropertyBlockForCommonLights();
                switch (powerLevel)
                {
                    //base is 100.0f
                    //base is 5.0f
                    case 0:
                        lightComponent.intensity = maxLightIntensity;
                        lightComponent.range = maxLightRange;
                        //set material emissive to default
                        //MaterialPropertyBlock to manage the emissive material values for all our common lights
                        //renderer.GetPropertyBlock(MPB);
                        //MPB.SetFloat("_EmissionIntensity", 50f);//50f is current emissive level for lights
                        //renderer.SetPropertyBlock(MPB);
                        break;
                    case 1:
                        lightComponent.intensity = maxLightIntensity * 0.5f; //50
                        lightComponent.range = maxLightRange * 0.75f; //3.75
                                                                      //set material emissive to 50%
                        //renderer.GetPropertyBlock(MPB);
                        //MPB.SetFloat("_EmissionIntensity", 40f);//50f is current emissive level for lights
                        //renderer.SetPropertyBlock(MPB);
                        break;
                    case 2:
                        lightComponent.intensity = maxLightIntensity * UnityEngine.Random.Range(0.35f, 0.25f);//35 - 25
                        lightComponent.range = maxLightRange * UnityEngine.Random.Range(0.5f, 0.6f);//6.0f; (2.5 to 3)
                                                                  //set material emissive to 35%
                        //renderer.GetPropertyBlock(MPB);
                        //MPB.SetFloat("_EmissionIntensity", 25f);//50f is current emissive level for lights
                        //renderer.SetPropertyBlock(MPB);
                        break;
                    case 3:
                        lightComponent.intensity = maxLightIntensity * UnityEngine.Random.Range(0.05f, 0.1f);//5 - 10
                        lightComponent.range = maxLightRange * UnityEngine.Random.Range(0.2f, 0.30f); //4.0f; (1 to 1.5)
                                                                                                      //set material emissive to 10%
                        //renderer.GetPropertyBlock(MPB);
                        //MPB.SetFloat("_EmissionIntensity", 10f);//50f is current emissive level for lights
                        //renderer.SetPropertyBlock(MPB);
                        break;
                    case 4:
                        lightComponent.intensity = 0.0f;
                        //set material emissive to 0%
                        //renderer.GetPropertyBlock(MPB);
                        //MPB.SetFloat("_EmissionIntensity", 0f);//50f is current emissive level for lights
                        //renderer.SetPropertyBlock(MPB);
                        break;
                    case 5:
                        netLightEnabled.Value = false;
                        //set material emissive to 0%
                        //renderer.GetPropertyBlock(MPB);
                        //MPB.SetFloat("_EmissionIntensity", 0f);//50f is current emissive level for lights
                        //renderer.SetPropertyBlock(MPB);
                        break;
                }
            }
        }

        [ClientRpc]
        public void RunMachinePointLightClientRpc(int powerLevel)
        {
            netLightEnabled.Value = true;
            //lightComponent.enabled = true;
            if (chillTime <= 0f)
            {
                MaterialPropertyBlock MPB = MaterialLibrary.GetMaterialPropertyBlockForCommonLights();
                switch (powerLevel)
                {
                    //base is 100.0f
                    //base is 5.0f
                    case 0:
                        lightComponent.intensity = maxLightIntensity;
                        lightComponent.range = maxLightRange;
                        //set material emissive to default
                        //MaterialPropertyBlock to manage the emissive material values for all our common lights
                        renderer.GetPropertyBlock(MPB);
                        MPB.SetFloat("_EmissionIntensity", 50f);//50f is current emissive level for lights
                        renderer.SetPropertyBlock(MPB);
                        break;
                    case 1:
                        lightComponent.intensity = maxLightIntensity * 0.5f; //50
                        lightComponent.range = maxLightRange * 0.75f; //3.75
                                                                      //set material emissive to 50%
                        renderer.GetPropertyBlock(MPB);
                        MPB.SetFloat("_EmissionIntensity", 40f);//50f is current emissive level for lights
                        renderer.SetPropertyBlock(MPB);
                        break;
                    case 2:
                        lightComponent.intensity = maxLightIntensity * UnityEngine.Random.Range(0.35f, 0.25f);//35 - 25
                        lightComponent.range = maxLightRange * UnityEngine.Random.Range(0.5f, 0.6f);//6.0f; (2.5 to 3)
                                                                                                    //set material emissive to 35%
                        renderer.GetPropertyBlock(MPB);
                        MPB.SetFloat("_EmissionIntensity", 25f);//50f is current emissive level for lights
                        renderer.SetPropertyBlock(MPB);
                        break;
                    case 3:
                        lightComponent.intensity = maxLightIntensity * UnityEngine.Random.Range(0.05f, 0.1f);//5 - 10
                        lightComponent.range = maxLightRange * UnityEngine.Random.Range(0.2f, 0.30f); //4.0f; (1 to 1.5)
                                                                                                      //set material emissive to 10%
                        renderer.GetPropertyBlock(MPB);
                        MPB.SetFloat("_EmissionIntensity", 10f);//50f is current emissive level for lights
                        renderer.SetPropertyBlock(MPB);
                        break;
                    case 4:
                        lightComponent.intensity = 0.0f;
                        //set material emissive to 0%
                        renderer.GetPropertyBlock(MPB);
                        MPB.SetFloat("_EmissionIntensity", 0f);//50f is current emissive level for lights
                        renderer.SetPropertyBlock(MPB);
                        break;
                    case 5:
                        netLightEnabled.Value = false;
                        lightComponent.intensity = 0.0f;
                        //lightComponent.enabled = false;
                        //set material emissive to 0%
                        renderer.GetPropertyBlock(MPB);
                        MPB.SetFloat("_EmissionIntensity", 0f);//50f is current emissive level for lights
                        renderer.SetPropertyBlock(MPB);
                        break;
                }
            }
                
        }
    }
}