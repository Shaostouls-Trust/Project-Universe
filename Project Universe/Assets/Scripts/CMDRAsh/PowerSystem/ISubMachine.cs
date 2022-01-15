using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectUniverse.Data.Libraries;
using ProjectUniverse.Animation.Controllers;
using MLAPI.NetworkVariable;
using MLAPI;
using MLAPI.NetworkVariable.Collections;
using MLAPI.Messaging;

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
        private Light lightComponent;
        public float maxLightIntensity;
        public float maxLightRange;
        //power legs update
        [SerializeField]
        private int legsRequired;
        private int legsReceived;
        //network vars
        private NetworkVariableFloat netRequestedEnergy = new NetworkVariableFloat(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.Everyone });
        private NetworkVariableFloat netRequiredEnergy = new NetworkVariableFloat(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.Everyone });
        private NetworkVariableFloat netEnergyBuffer = new NetworkVariableFloat(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.Everyone });
        private NetworkVariableFloat netBufferCurrent = new NetworkVariableFloat(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.Everyone });
        private NetworkVariableBool netIsPowered = new NetworkVariableBool(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.Everyone });
        private NetworkVariableBool netRunMachine = new NetworkVariableBool(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.Everyone });
        private NetworkVariableFloat netMaxLightIntensity = new NetworkVariableFloat(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.Everyone });
        private NetworkVariableFloat netMaxLightRange = new NetworkVariableFloat(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.Everyone });
        private NetworkVariableInt netLegsRequired = new NetworkVariableInt(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.Everyone });
        private NetworkVariableInt netLegsReceived = new NetworkVariableInt(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.Everyone });
        private NetworkVariableBool netLightEnabled = new NetworkVariableBool(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.Everyone });
        //anti-spaz timer
        private float chillTime = 7f;

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
            lightComponent = this.gameObject.GetComponentInChildren<Light>();
            //if(isHost){
            //}?
            NetworkListeners();
        }

        private void NetworkListeners()
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
            if(lightComponent != null)
            {
                netLightEnabled.Value = lightComponent.enabled;
            }

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

        /// <summary>
        /// We need to lighten this update method as much as possible!
        /// </summary>
        void Update()
        {
            //reset requestedEnergy
            netRequestedEnergy.Value = requiredEnergy;
            //requestedEnergy = requiredEnergy;
            //Recalculate drawToFill based on draw percent
            float floatDrawToFill = (float)percentDrawToFill;
            drawToFill = requiredEnergy + (requiredEnergy * (floatDrawToFill / 100)); //105% or 110% draw
                                                                                      //If the energy buffer is not full
            if (bufferCurrent < energyBuffer)
            {
                //Get the deficit between the energybuffer(max) and the current buffer amount
                float deficit = energyBuffer - bufferCurrent;
                if (deficit >= drawToFill)
                {
                    //send energy request
                    netRequestedEnergy.Value = drawToFill;
                    //requestedEnergy = drawToFill;
                    RequestHelper();

                }
                else if (deficit < drawToFill && deficit > requiredEnergy)
                {
                    netRequestedEnergy.Value = deficit + netRequiredEnergy.Value;
                    //requestedEnergy = deficit + requiredEnergy;
                    //Debug.Log(this.gameObject.name + " Request Helper");
                    RequestHelper();
                }
                else
                {
                    requestedEnergy = requiredEnergy;
                    //Debug.Log(this.gameObject.name + " Request Helper");
                    RequestHelper();
                }
                if(bufferCurrent < 0f)
                {
                    bufferCurrent = 0f;
                }
            }
            else if (bufferCurrent >= energyBuffer)
            {
                //send request
                //netRequestedEnergy.Value = netRequiredEnergy.Value;
                requestedEnergy = requiredEnergy;
                bufferCurrent = energyBuffer;
                //requestedEnergy = 0.0f;
                //Debug.Log(this.gameObject.name + " Request Helper");
                RequestHelper();
            }
            
            /*
            else
            {
                requestedEnergy = requiredEnergy;
                //Debug.Log(this.gameObject.name + " Request Helper");
                RequestHelper();
            }
            */
            if (runMachine)
            {
                //run machines
                //Debug.Log("Running "+this.gameObject.name);
                RunLogic();
            }
            else
            {
                //turn the machine off
                RunMachineSelector(machineType, 5);
            }
        }

        public bool GetRunMachine()
        {
            return netRunMachine.Value;//runMachine;
        }

        public void RequestHelper()
        {
            foreach (IBreakerBox box in breakers)
            {
                //Debug.Log("request from breakers: "+requestedEnergy/breakers.Count);
                box.RequestPowerFromBreaker(requestedEnergy / breakers.Count, this);//this.GetComponent<ISubMachine>()
            }
        }

        public void RunLogic()
        {
            ///////////////////////////////////////
            //Run logic
            ///////////////////////////////////////
            if (runMachine)
            {
                chillTime--;
                if (chillTime < 0f)
                {
                    chillTime = 7f;
                }
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
                RunMachineSelector(machineType, 4);
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

        public float RequestedEnergyAmount() //ref int numSuppliers)
        {
            //numSuppliers += 1;
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
            //receive X legs with X amounts
            for (int i = 0; i < legCount; i++)
            {
                netBufferCurrent.Value += amounts[i];
                //bufferCurrent += amounts[i];
            }
            //Debug.Log(this + " submachine buffer at: " + bufferCurrent);
            //legsReceived = legCount;
            netLegsReceived.Value = legCount;
            //Debug.Log("submachine has "+legsReceived+" legs");
            //bufferCurrent += amount;
            //round buffer current to 3 places to avoid having a psychotic meltdown
            netBufferCurrent.Value = (float)Math.Round(netBufferCurrent.Value, 3);
            //bufferCurrent = (float)Math.Round(bufferCurrent, 3);

            if (!iCableDLL.Contains(cable))
            {
                //netICableDLL.Add(cable);
                iCableDLL.AddLast(cable);
            }
            if (bufferCurrent > energyBuffer)
            {
                //trim off excess power. Buffers cannot overcharge
                netBufferCurrent.Value = energyBuffer;
                //bufferCurrent = energyBuffer;
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
                    this.RunMachinePointLightServerRpc(powerLevel);
                    break;
                case "door":
                    //this.gameObject.GetComponent<DoorAnimator>().runSubMachine(powerLevel);
                    this.gameObject.GetComponent<DoorAnimator>().RunSubmachineServerRpc(powerLevel);//runSubMachine
                    break;
            }//*/
        }

        [ServerRpc(RequireOwnership = false)]
        public void RunMachinePointLightServerRpc(int powerLevel)
        {
            netLightEnabled.Value = true;
            //lightComponent.enabled = true;
            if(chillTime <= 0f)
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
                        lightComponent.range = 0.0f;
                        //set material emissive to 0%
                        renderer.GetPropertyBlock(MPB);
                        MPB.SetFloat("_EmissionIntensity", 0f);//50f is current emissive level for lights
                        renderer.SetPropertyBlock(MPB);
                        break;
                    case 5:
                        netLightEnabled.Value = false;
                        //lightComponent.enabled = false;
                        //set material emissive to 0%
                        renderer.GetPropertyBlock(MPB);
                        MPB.SetFloat("_EmissionIntensity", 0f);//50f is current emissive level for lights
                        renderer.SetPropertyBlock(MPB);
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
                        lightComponent.range = 0.0f;
                        //set material emissive to 0%
                        renderer.GetPropertyBlock(MPB);
                        MPB.SetFloat("_EmissionIntensity", 0f);//50f is current emissive level for lights
                        renderer.SetPropertyBlock(MPB);
                        break;
                    case 5:
                        netLightEnabled.Value = false;
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