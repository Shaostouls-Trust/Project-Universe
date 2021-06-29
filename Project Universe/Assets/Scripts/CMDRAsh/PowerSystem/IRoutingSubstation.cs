using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ProjectUniverse.Production.Machines;

namespace ProjectUniverse.PowerSystem
{
    public sealed class IRoutingSubstation : MonoBehaviour
    {
        private Guid guid;
        public IMachine[] targetMachine;
        public IBreakerBox[] targetBreakers;
        //private float[] requestedPower;
        private float totalRequiredPower;
        private IRoutingSubstation thisSubstation;
        private LinkedList<ICable> iCableDLL = new LinkedList<ICable>();
        [SerializeField] private float energyBufferMax;
        private float energyBufferMaxResetValue;
        private Mach_RoutingSubstation M_Substation;
        [SerializeField]
        private float bufferCurrent;
        private float defecitVbreaker = 1.0f;
        private float defecitVmachine = 1.0f;
        private List<IRouter> myRouters = new List<IRouter>();

        //buildstate
        private bool buildState;

        //power legs update
        private int legsRequired = 3;//leg shortage willonly cut distributable power by 1/2/3 3rds
        private int legsReceived; //if lose a leg, increase demand through remaining.
        private int legsOut;//calculate based on machines linked and component health
        [SerializeField]
        private int availibleLegsOut;

        //damage update
        private int _legRedux;
        private float _bufferCurrentRedux;
        private float _bufferMaxRedux;
        private float _requestRedux;

        // Start is called before the first frame update
        void Start()
        {
            M_Substation = GetComponent<Mach_RoutingSubstation>();
            thisSubstation = GetComponent<IRoutingSubstation>();
            energyBufferMax = 1080f;
            energyBufferMaxResetValue = energyBufferMax;
            bufferCurrent = 0f;
            totalRequiredPower = 0.0f;
            guid = Guid.NewGuid();
            //create a cable between substation and machine/s
            ProxyStart(2);
            ProxyStart(1);
        }

        public void ProxyStart(int mode)
        {
            if (mode == 1)
            {
                for (int i = 0; i < targetBreakers.Length; i++)
                {
                    if (targetBreakers[i] != null)
                    {
                        ICable cable = new ICable(this, targetBreakers[i]);
                        iCableDLL.AddLast(cable);
                        legsOut += targetBreakers[i].GetLegRequirement();
                        availibleLegsOut = legsOut;
                        Debug.Log("Checking Breaker State " + targetBreakers[i].gameObject.name);
                        targetBreakers[i].CheckMachineState(ref thisSubstation);
                    }
                }
            }
            else if (mode == 2)
            {
                for (int i = 0; i < targetMachine.Length; i++)
                {
                    if (targetMachine[i] != null)
                    {
                        ICable cable = new ICable(this, targetMachine[i]);
                        iCableDLL.AddLast(cable);
                        legsOut += targetMachine[i].GetLegRequirement();
                        Debug.Log("Checking Machine State " + targetMachine[i].gameObject.name);
                        targetMachine[i].CheckMachineState(ref thisSubstation);
                    }
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            //get the Mach_ build state
            //buildState = M_Substation.GetBuildState();
            //if (buildState)
            //{
            availibleLegsOut = legsOut;
            totalRequiredPower = 0f;
            energyBufferMax = energyBufferMaxResetValue;
            //First apply effects of damaged components
            DamageEffectStack();
            //get leg states - this will be for when we have levers that close off indiv legs
            //(For subst input. Output to machines will be a display).
            //NYI
            float totalMachineReq = 0f;
            float totalBreakerReq = 0f;
            //requestedPower = new float[targetMachine.Length + targetBreakers.Length];
            //int numSuppliers = 0;

            for (int i = 0; i < targetMachine.Length; i++)
            {
                if (targetMachine[i] != null)
                {
                    totalMachineReq += targetMachine[i].RequestedEnergyAmount();
                    totalRequiredPower += targetMachine[i].RequestedEnergyAmount();
                }
            }
            //Debug.Log("Machine requirement: "+ totalMachineReq);
            for (int k = 0; k < targetBreakers.Length; k++)
            {
                if (targetBreakers[k] != null)
                {
                    totalBreakerReq += targetBreakers[k].GetTotalRequiredPower();
                    totalRequiredPower += targetBreakers[k].GetTotalRequiredPower();
                }
            }
            //Debug.Log("Breaker requirement: " + totalBreakerReq);
            //request the required energy from the router
            if (bufferCurrent < energyBufferMax)
            {
                //request energy from Router
                float requestPerRouter = totalRequiredPower / myRouters.Count;
                foreach (IRouter rout in myRouters)
                {
                    rout.RequestPowerFromRouter(requestPerRouter, thisSubstation);
                }
            }
            else if (bufferCurrent >= energyBufferMax)
            {
                totalRequiredPower = 0;
                bufferCurrent = energyBufferMax;
            }
            //power will be divided equally among linked machines, sacrificing breaker power by as much as 75% in case of defecit.
            //ignore this division block if the substation buffer is empty
            if (totalBreakerReq + totalMachineReq > bufferCurrent)
            {
                //Debug.Log("Requested exceeds buffer:"+bufferCurrent);
                //difference between required and available power.
                float deficit = totalRequiredPower - bufferCurrent;
                if (targetBreakers.Length > 0)//no need to run the code below if there are no breakers.
                {
                    //defecitVbreaker = 1.0f;
                    //determine the percent difference between defecit and the power required by the breakers
                    if (totalBreakerReq > 0)//div by zero precaution
                    {
                        //defecitVbreaker = totalBreakerReq / deficit; //backwards?
                        defecitVbreaker = deficit / totalBreakerReq;
                    }
                    else { defecitVbreaker = 0.0f; }
                }
                else { defecitVbreaker = 0.0f; }
                //multiply by the ammount of power we are retaining for the breakers
                totalBreakerReq *= defecitVbreaker;//-= (totalBreakerReq * defecitVbreaker);//0.25f;
                                                    //get the new power requirement.
                float newRequiredPower = totalMachineReq + totalBreakerReq;
                //recompute deficit based on reduced breaker draw.
                deficit = newRequiredPower - bufferCurrent;
                //calculate the percent that we need to trim off of the machine requirement to settle deficit
                //IE how much is the defecit when compared to the buffer (in %)
                defecitVmachine = deficit / totalMachineReq;
                //Debug.Log("Defecit v machine: " + defecitVmachine);
                //Debug.Log("Defecit v breaker: " + defecitVbreaker);
            }
            else
            {
                defecitVbreaker = 0.0f;
                defecitVmachine = 0.0f;
            }

            //}
        }

        //called at start of router update
        public bool CheckMachineState(ref IRouter thisRouter)
        {
            if (!myRouters.Contains(thisRouter))
            {
                Debug.Log("router added");
                myRouters.Add(thisRouter);
            }
            return true;
        }

        public void RequestPowerFromSubstation(float requestedAmount, IBreakerBox thisBreaker)
        {
            //transfer power to the linking cable.
            foreach (ICable cable in iCableDLL)
            {
                //if this is a machine
                if (cable.breaker == thisBreaker)
                {
                    //get machine's leg req (a default value)
                    int breakerLegReq = cable.breaker.GetLegRequirement();
                    //if something has happened, and we don't have as many legs as we need.
                    if (breakerLegReq > availibleLegsOut)
                    {
                        //we will temporarily change the required leg count to what we can provide
                        breakerLegReq = availibleLegsOut;
                    }
                    //adjust total power need by the breaker defecit
                    requestedAmount -= (requestedAmount * defecitVbreaker);//new, was in for below
                                                                           //split power between legs
                    float[] powerAmount = new float[breakerLegReq];
                    for (int l = 0; l < breakerLegReq; l++)
                    {
                        powerAmount[l] = requestedAmount / breakerLegReq;
                        //Debug.Log("power amount " + l + " : " + powerAmount[l]);
                        //apply the effects of damage to internal components
                        powerAmount[l] -= (powerAmount[l] * _requestRedux);
                        powerAmount[l] = (float)Math.Round(powerAmount[l], 3);
                    }
                    //Debug.Log("defecitVbreaker:" + defecitVbreaker);
                    //requestedAmount = powerAmount[0] * breakerLegReq; //was included
                    if (cable.CheckConnection(4))//transfer to breaker
                    {
                        //Debug.Log("buffer amount: "+bufferCurrent);
                        if (bufferCurrent - requestedAmount >= 0)
                        {
                            //transfer the uniquely requested amount to the machine
                            //Debug.Log("Transfer power amount: "+(powerAmount[0]*breakerLegReq));
                            cable.TransferIn(breakerLegReq, powerAmount, 4);
                            availibleLegsOut -= breakerLegReq;
                            bufferCurrent -= requestedAmount;
                        }
                        else if (bufferCurrent - requestedAmount < 0)
                        {
                            //Debug.Log("Transfer remaining ("+bufferCurrent+")");
                            float[] tempfloat = new float[] { bufferCurrent / 3.0f, bufferCurrent / 3.0f, bufferCurrent / 3.0f };
                            //or transfer all that remains in the buffer
                            cable.TransferIn(breakerLegReq, tempfloat, 4);
                            availibleLegsOut -= breakerLegReq;
                            bufferCurrent = 0f;
                        }
                    }
                }
            }
        }

        public void RequestPowerFromSubstation(float requestedAmount, IMachine thisMachine)
        {
            //transfer power to the linking cable.
            foreach (ICable cable in iCableDLL)
            {
                //if this is a machine
                if (cable.mach == thisMachine)
                {
                    //get machine's leg req
                    int machineLegReq = cable.mach.GetLegRequirement();
                    //if something has happened, and we don't have as many legs as we need.
                    if (machineLegReq > availibleLegsOut)
                    {
                        //we will temporarily change the required leg count to what we can provide
                        machineLegReq = availibleLegsOut;
                        //Debug.Log("leg shortage ("+machineLegReq+")");
                    }
                    //
                    requestedAmount -= (requestedAmount * defecitVmachine);
                    //split power between legs
                    float[] powerAmount = new float[machineLegReq];
                    //Debug.Log("splitting power between legs");
                    for (int l = 0; l < machineLegReq; l++)
                    {
                        powerAmount[l] = requestedAmount / machineLegReq;
                        //Debug.Log("power amount " + l + " : " + powerAmount[l]);
                        //apply the effects of damage to internal components
                        powerAmount[l] -= (powerAmount[l] * _requestRedux);
                        powerAmount[l] = (float)Math.Round(powerAmount[l], 3);
                    }
                    if(powerAmount.Length != 0)
                    {
                        requestedAmount = powerAmount[0] * machineLegReq;
                    }
                    else { requestedAmount = 0f; }
                    if (cable.CheckConnection(3))//type is substation to machine linkage
                    {
                        //Debug.Log("Cable exists");
                        if (bufferCurrent - requestedAmount >= 0)
                        {
                            //transfer the uniquely requested amount to the machine
                            // Debug.Log("Sufficient Power");
                            cable.TransferIn(machineLegReq, powerAmount, 3);
                            availibleLegsOut -= machineLegReq;
                            bufferCurrent -= requestedAmount;
                        }
                        else if (bufferCurrent - requestedAmount < 0)
                        {
                            //  Debug.Log("Power Defecit");
                            float[] tempfloat = new float[] { bufferCurrent / 3, bufferCurrent / 3, bufferCurrent / 3 };
                            //or transfer all that remains in the buffer
                            cable.TransferIn(machineLegReq, tempfloat, 3);
                            availibleLegsOut -= machineLegReq;
                            bufferCurrent = 0f;
                        }
                    }
                }
            }
        }

        public void SetMachines(IMachine[] newMachines)
        {
            targetMachine = newMachines;
        }

        public void SetBreakers(IBreakerBox[] newBreakers)
        {
            targetBreakers = newBreakers; ;
        }

        public int GetLegRequirement()
        {
            return legsRequired;
        }

        public void ReceivePowerFromRouter(int legCount, float[] powerAmounts)
        {
            //Debug.Log("Receiving ");
            //receive 3 legs of X amount
            for (int i = 0; i < legCount; i++)
            {
                bufferCurrent += powerAmounts[i];
            }
            legsReceived = legCount;
            bufferCurrent = (float)Math.Round(bufferCurrent, 3);
        }

        public float GetTotalRequiredPower()
        {
            return totalRequiredPower;
        }

        public Guid getGUID()
        {
            return guid;
        }

        public void DamageEffectStack()
        {
            //Debug.Log("legs b4 "+availibleLegsOut);
            availibleLegsOut -= _legRedux;
            if(availibleLegsOut < 0)
            {
                availibleLegsOut = 0;
            }
            //Debug.Log("legs af " + availibleLegsOut);

            bufferCurrent -= (bufferCurrent * _bufferCurrentRedux);

            energyBufferMax -= (energyBufferMax * _bufferMaxRedux);
        }

        public int LegsRedux
        {
            set { _legRedux = value; }
            get { return _legRedux; }
        }

        public float RequestRedux
        {
            get { return _requestRedux; }
            set { _requestRedux = value; }
        }
        public float EnergyBufferRedux
        {
            get { return _bufferMaxRedux; }
            set { _bufferMaxRedux = value; }
        }
        public float BufferCurrentRedux
        {
            get { return _bufferCurrentRedux; }
            set { _bufferCurrentRedux = value; }
        }
    }
}