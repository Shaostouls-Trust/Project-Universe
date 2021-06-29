using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectUniverse.Data.Libraries;
using ProjectUniverse.Animation.Controllers;

namespace ProjectUniverse.PowerSystem
{
    public sealed class ISubMachine : MonoBehaviour
    {
        //Amount required (requested) to run machine
        [SerializeField]
        private float requestedEnergy;
        //unadjusted amount required to run the machine
        [SerializeField]
        private float requiredEnergy;
        [SerializeField]
        private int percentDrawToFill;
        //amount to draw when filling the interal buffer
        private float drawToFill;
        [SerializeField]
        private float energyBuffer; //Machines shouldn't store more than 10 frames worth of power.
        [SerializeField]
        private float bufferCurrent;
        [SerializeField]
        private string machineType;
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
        }

        /// <summary>
        /// We need to lighten this update method as much as possible!
        /// </summary>
        void Update()
        {
            //reset requestedEnergy
            requestedEnergy = requiredEnergy;
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
                    requestedEnergy = drawToFill;
                    //Debug.Log(this.gameObject.name+" Request Helper");
                    RequestHelper();

                }
                else if (deficit == requiredEnergy)
                {
                    //send energy request
                    requestedEnergy = requiredEnergy;
                    //Debug.Log(this.gameObject.name + " Request Helper");
                    RequestHelper();
                }
                else if (deficit < drawToFill && deficit > requiredEnergy)
                {
                    requestedEnergy = deficit + requiredEnergy;
                    //Debug.Log(this.gameObject.name + " Request Helper");
                    RequestHelper();
                }
                else
                {
                    requestedEnergy = requiredEnergy;
                    //Debug.Log(this.gameObject.name + " Request Helper");
                    RequestHelper();
                }
            }
            else if (bufferCurrent >= energyBuffer)
            {
                //send request
                requestedEnergy = requiredEnergy;
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
            return runMachine;
        }

        public void RequestHelper()
        {
            foreach (IBreakerBox box in breakers)
            {
                //Debug.Log("request/breakCount: "+requestedEnergy/breakers.Count);
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
                if (legsReceived == legsRequired)
                {
                    //Debug.Log("Legs received");
                    if (bufferCurrent > 0f)
                    {
                        isPowered = true;
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
                            bufferCurrent = 0.0f;
                        }
                        else
                        {
                            //run full power
                            RunMachineSelector(machineType, 0);
                            bufferCurrent -= requiredEnergy;
                        }
                    }
                    else
                    {
                        isPowered = false;
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
            set { runMachine = value; }
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
                bufferCurrent += amounts[i];
            }
            //Debug.Log(this + " submachine buffer at: " + bufferCurrent);
            legsReceived = legCount;
            //Debug.Log("submachine has "+legsReceived+" legs");
            //bufferCurrent += amount;
            //round buffer current to 3 places to avoid having a psychotic meltdown
            bufferCurrent = (float)Math.Round(bufferCurrent, 3);

            if (!iCableDLL.Contains(cable))
            {
                iCableDLL.AddLast(cable);
            }
            if (bufferCurrent > energyBuffer)
            {
                //trim off excess power. Buffers cannot overcharge
                bufferCurrent = energyBuffer;
            }
        }

        //called on cable disconnect (NYI)
        public void RemoveCableConnection(ICable cable)
        {
            iCableDLL.Remove(cable);
        }

        //called at the start of the breaker update block
        public bool CheckMachineState(ref IBreakerBox myBreaker)
        {
            if (!breakers.Contains(myBreaker))
            {
                //Debug.Log("breaker added");
                breakers.Add(myBreaker);
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
                    this.RunMachinePointLight(powerLevel);
                    break;
                case "door":
                    this.gameObject.GetComponent<DoorAnimator>().runSubMachine(powerLevel);
                    break;
            }//*/
        }

        public void RunMachinePointLight(int powerLevel)
        {
            lightComponent = this.gameObject.GetComponentInChildren<Light>();
            lightComponent.enabled = true;
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
                    lightComponent.enabled = false;
                    //set material emissive to 0%
                    renderer.GetPropertyBlock(MPB);
                    MPB.SetFloat("_EmissionIntensity", 0f);//50f is current emissive level for lights
                    renderer.SetPropertyBlock(MPB);
                    break;
            }
        }
    }
}