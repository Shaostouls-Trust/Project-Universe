using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUniverse.PowerSystem
{
    public sealed class IMachine : MonoBehaviour
    {
        //Amount required (requested) to run machine
        [SerializeField] private float requestedEnergy;
        //unadjusted amount required to run the machine
        [SerializeField] private float requiredEnergy;
        [SerializeField] private int percentDrawToFill;
        //amount to draw when filling the interal buffer
        private float drawToFill;
        [SerializeField] private float energyBuffer; //Machines shouldn't store more than 10 frames worth of power.
        [SerializeField] private float bufferCurrent;
        [SerializeField] private string machineType;
        [SerializeField] private bool runMachine;
        //private ICable cable;
        private List<IRoutingSubstation> substations = new List<IRoutingSubstation>();
        //backend of power cables
        private LinkedList<ICable> iCableDLL = new LinkedList<ICable>();
        //time control
        private float time = 0.1f; //10 times a second

        //temp stuff
        //public Transform transform;
        public GameObject screenObject;
        //public Light lightComponent;
        //public float maxLightIntensity;
        //public float maxLightRange;

        //power legs update
        [SerializeField]
        private int legsRequired;
        private int legsReceived;

        void Start()
        {
            RunMachine = true;
            bufferCurrent = 0.0f;
        }

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
                    requestedEnergy = drawToFill;
                    //Debug.Log(this.gameObject.name + " Request Helper");
                    RequestHelper();
                }
                else if (deficit == requiredEnergy)
                {
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
            }
            else if (bufferCurrent >= energyBuffer)
            {
                requestedEnergy = requiredEnergy;
                //requestedEnergy = 0.0f;
                //trim off excess power. Buffers cannot overcharge
                //bufferCurrent = energyBuffer;
                //Debug.Log(this.gameObject.name + " Request Helper");
                RequestHelper();
            }
            else
            {
                requestedEnergy = requiredEnergy;
                //Debug.Log(this.gameObject.name + " Request Helper");
                RequestHelper();
            }
            //send power request
            if (runMachine)
            {
                //run machines
                RunLogic();
            }
            else
            {
                if(bufferCurrent > 0f)
                {
                    RunMachineSelector(machineType, 5);
                }
                else
                {
                    RunMachineSelector(machineType, 4);
                }
            }

        }

        public void RequestHelper()
        {
            foreach (IRoutingSubstation subs in substations)
            {
                //Debug.Log(this.gameObject.name + " machine has Requested " + requestedEnergy / substations.Count);
                subs.RequestPowerFromSubstation(requestedEnergy / substations.Count, this.GetComponent<IMachine>());
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
                    if (bufferCurrent > 0f)
                    {
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
                        //'run' at 0 power
                        RunMachineSelector(machineType, 4);
                    }
                }
                else if (legsReceived < legsRequired && legsReceived >= 1)
                {
                    //Shut down machine due to leg requirement
                    RunMachineSelector(machineType, 5);//was 4
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
                RunMachineSelector(machineType, 5);//was 4
            }

        }

        public bool RunMachine
        {
            get { return runMachine; }
            set { runMachine = value; }
        }

        public float RequestedEnergyAmount()//ref int numSuppliers)
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
                //Debug.Log(this.gameObject.name + " machine has Received "+amounts[i]+" X3");
            }
            legsReceived = legCount;
            //Debug.Log(this.gameObject.name+" machine has "+legsReceived+" legs");
            //bufferCurrent += amount;
            //round buffer current to 3 places to avoid having a psychotic meltdown
            bufferCurrent = (float)Math.Round(bufferCurrent, 3);
            if (!iCableDLL.Contains(cable))
            {
                iCableDLL.AddLast(cable);
            }
            if (bufferCurrent > energyBuffer)
            {
                bufferCurrent = energyBuffer;
            }
        }

        //return the amount of legs needed by the machine, so that the substation will know
        //how to divide the power.
        public int GetLegRequirement()
        {
            return legsRequired;
        }

        //called on cable disconnect (NYI)
        public void RemoveCableConnection(ICable cable)
        {
            iCableDLL.Remove(cable);
        }

        //called at the start of the Substation update block
        public bool CheckMachineState(ref IRoutingSubstation substation)
        {
            if (!substations.Contains(substation))
            {
                substations.Add(substation);
            }
            return true;
        }

        /// Could we pass in script to run, so that we don't need to hardcode in all these machine scripts.
        public void RunMachineSelector(string ImachineType, int powerLevel)
        {
            switch (ImachineType)
            {
                case "machine_basic":
                    this.RunMachineNormal(powerLevel);
                    break;
                case "machine_adv":
                    this.RunMachineAdv(powerLevel);
                    break;
                case "kiosk":
                    this.RunMachineKiosk(powerLevel);
                    break;
                    /*
                case "InductionFurnace":
                    this.gameObject.GetComponent<Mach_InductionFurnace>().RunMachine(powerLevel);
                    break;
                case "OxygenGen":
                    this.gameObject.GetComponent<IOxygenGenerator>().RunMachine(powerLevel);
                    break;
                case "DevFactory":
                    this.gameObject.GetComponent<Mach_DevFactory>().RunMachine(powerLevel);
                    break;
                    */
            }
            SendMessage("RunMachine", powerLevel, SendMessageOptions.DontRequireReceiver);
        }

        //public void runMachineSelector<mytype>(string ImachineType, int powerLevel,bool rtw)
        //{
        //    var scriptComponent = this.gameObject.GetComponent(ImachineType) as IOxygenGenerator;
        //    scriptComponent.runMachine(powerLevel);
        //}

        public void RunMachineNormal(int powerLevel)
        {
            switch (powerLevel)
            {
                case 0:
                    this.gameObject.transform.Rotate(0.0f, 50.0f * Time.deltaTime, 0.0f);
                    break;
                case 1:
                    this.gameObject.transform.Rotate(0.0f, 25.0f * Time.deltaTime, 0.0f);//should be 0?
                    break;
                case 2:
                    this.gameObject.transform.Rotate(0.0f, 12.5f * Time.deltaTime, 0.0f);
                    break;
                case 3:
                    this.gameObject.transform.Rotate(0.0f, 6.0f * Time.deltaTime, 0.0f);
                    break;
                case 4:
                    this.gameObject.transform.Rotate(0.0f, 0.0f * Time.deltaTime, 0.0f);
                    break;
                case 5:
                    this.gameObject.transform.Rotate(0.0f, 0.0f * Time.deltaTime, 0.0f);
                    break;
            }
        }

        public void RunMachineAdv(int powerLevel)
        {
            switch (powerLevel)
            {
                case 0:
                    this.gameObject.transform.Rotate(0.0f, 50.0f * Time.deltaTime, 0.0f);
                    break;
                case 1:
                    this.gameObject.transform.Rotate(0.0f, 25.0f * Time.deltaTime, 0.0f);//should be 0?
                    break;
                case 2:
                    this.gameObject.transform.Rotate(0.0f, 12.5f * Time.deltaTime, 0.0f);
                    break;
                case 3:
                    this.gameObject.transform.Rotate(0.0f, 6.0f * Time.deltaTime, 0.0f);
                    break;
                case 4:
                    this.gameObject.transform.Rotate(0.0f, 0.0f * Time.deltaTime, 0.0f);
                    break;
                case 5:
                    this.gameObject.transform.Rotate(0.0f, 0.0f * Time.deltaTime, 0.0f);
                    break;
            }
        }

        public void RunMachineKiosk(int powerlevel)
        {
            //grab the screen's material
            Renderer screenRenderer = screenObject.GetComponent<Renderer>();
            switch (powerlevel)
            {
                case 0:
                    screenRenderer.material.SetFloat("Emiss Intensity", 0.25f);
                    break;
                case 1:
                    break;
                case 2:
                    break;
                case 3:
                    break;
                case 4:
                    screenRenderer.material.SetFloat("Emiss Intensity", 0.0f);
                    break;
                case 5:
                    break;
            }
        }
    }
}