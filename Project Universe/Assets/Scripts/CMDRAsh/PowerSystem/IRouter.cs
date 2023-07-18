using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ProjectUniverse.PowerSystem.Nuclear;

namespace ProjectUniverse.PowerSystem
{
    public sealed class IRouter : MonoBehaviour
    {
        private Guid guid;
        [SerializeField]
        private int routerLevel;
        [SerializeField]
        private IRoutingSubstation[] subRouters;
        private LinkedList<ICable> iCableDLL = new LinkedList<ICable>();
        [SerializeField]
        private float energyBufferMax;
        [SerializeField]
        private float bufferCurrent;
        private float[] requestedPower;
        private float totalRequiredPower;
        private float deficitVsubMachine;
        private int routerCap;
        private IRouter thisRouter;
        private IGenerator supplyingGenerator;
        private SteamTurbine supplyingTurbine;
        [SerializeField] private PowerOutputController poc;
        private float lastReceived;
        //private float lastOut;

        //power legs update
        private int legsRequired = 3;
        private int legsReceived;
        private int legsOut;//calculate based on machines linked
        [SerializeField]
        private int availibleLegsOut;
        private bool useGeneratorPower = true;

        void Start()
        {
            //create GUID
            guid = Guid.NewGuid();
            //legsOut = subRouters.Length;
            //energyBufferMax = 6000;
            ProxyStart();
        }

        public float BufferCurrent
        {
            get { return bufferCurrent; }
        }

        public float BufferMax
        {
            get { return energyBufferMax; }
        }

        public IGenerator ConnectedGenerator
        {
            get { return supplyingGenerator; }
        }

        public SteamTurbine ConnectedTurbine
        {
            get { return supplyingTurbine; }
        }
        public float LastReceived
        {
            get { return lastReceived; }
        }
        public bool UseGeneratorPower
        {
            get { return useGeneratorPower; }
            set { useGeneratorPower = value; }
        }
        //public float LastOut
        //{
        //    get { return lastOut; }
        //}

        public void ProxyStart()
        {
            switch (routerLevel)
            {
                case 1: routerCap = 4; break;
                case 2: routerCap = 6; break;
                case 3: routerCap = 8; break;
                case 4: routerCap = 10; break;
                case 5: routerCap = 12; break;
            }
            if (subRouters.Length > routerCap)
            {
                IRoutingSubstation[] routTemp = new IRoutingSubstation[routerCap];
                for (int i = 0; i < routerCap; i++)
                {
                    routTemp[i] = subRouters[i];
                }
                subRouters = routTemp;
            }
            //set buffer current
            bufferCurrent = 0f;
            thisRouter = this.gameObject.GetComponent<IRouter>();
            //look for router substations based on routerLevel
            for (int i = 0; i < subRouters.Length; i++)
            {
                if (this.subRouters[i] != null)//if the cell is empty
                {
                    //create an ICable node to add to the iCableDLL
                    ICable myIcable = new ICable(this, this.subRouters[i]);
                    //add it to the end of the DLL, if alone, it's first and last.
                    iCableDLL.AddLast(myIcable);
                    Debug.Log("Checking Substation state " + subRouters[i]);
                    subRouters[i].CheckMachineState(ref thisRouter);
                    //create manager buttons
                    if (poc != null)
                    {
                        poc.CreateButton(subRouters[i]);
                    }
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            //lastOut = 0f;
            availibleLegsOut = subRouters.Length * 3;//3 legs per substation
            totalRequiredPower = 0f;
            //get leg states - this will be for when we have levers that close off indiv legs.
            //NYI
            //get requested power amount
            for (int i = 0; i < subRouters.Length; i++)
            {
                if(subRouters[i] != null)
                {
                    totalRequiredPower += subRouters[i].TotalRequiredPower;
                }
            }
            //power request to generator logic
            if (bufferCurrent < energyBufferMax)
            {
                if (supplyingGenerator != null && useGeneratorPower)
                {
                    //request power from generator
                    //Debug.Log(totalRequiredPower);
                    supplyingGenerator.RequestPowerFromGenerator(totalRequiredPower, thisRouter);
                }
            }
            if (bufferCurrent >= energyBufferMax)
            {
                totalRequiredPower = 0.0f;
                bufferCurrent = energyBufferMax;
            }

            //power will be divided equally among linked substations.
            if (totalRequiredPower > bufferCurrent)
            {
                float defecit = totalRequiredPower - bufferCurrent;
                deficitVsubMachine = defecit / totalRequiredPower;
                //Debug.Log(deficitVsubMachine);
            }
            else
            {
                deficitVsubMachine = 0f;
            }
            //Debug.Log("Total Required: " + totalRequiredPower);
            if(bufferCurrent == 0f)
            {
                lastReceived = 0f;
            }
        }

        public bool CheckMachineState(ref IGenerator thisGenerator)
        {
            supplyingGenerator = thisGenerator;
            return true;
        }

        public void RequestPowerFromRouter(float requestedAmount, IRoutingSubstation thisSubstation)
        {
            //send power through to the requesting substation
            foreach (ICable cable in iCableDLL)
            {
                if (cable.subst == thisSubstation)
                {
                    //get subst's leg req
                    int routerLegReq = cable.subst.GetLegRequirement();
                    //if something has happened, and we don't have as many legs as we need.
                    if (routerLegReq > availibleLegsOut)
                    {
                        //we will temporarily change the required leg count to what we can provide
                        routerLegReq = availibleLegsOut;
                    }
                    //apply deficitVsubMachine
                    //Debug.Log("req: "+requestedAmount +" -= "+ (requestedAmount * deficitVsubMachine));
                    requestedAmount -= (requestedAmount * deficitVsubMachine);//breaks low-power-output (buffer/3f)
                    requestedAmount = (float)Math.Round(requestedAmount, 2);
                    //lastOut += requestedAmount;
                    //split power between legs
                    float[] powerAmount = new float[routerLegReq];
                    for (int l = 0; l < routerLegReq; l++)
                    {
                        powerAmount[l] = requestedAmount / routerLegReq;
                    }
                    if (cable.CheckConnection(2))//type is router to substation linkage
                    {
                        if (bufferCurrent - requestedAmount >= 0)
                        {
                            //transfer the uniquely requested amount to the router
                            //cable.transferIn(requestedPower[itteration], 2);
                            cable.TransferIn(routerLegReq, powerAmount, 2);
                            availibleLegsOut -= routerLegReq;
                            bufferCurrent -= requestedAmount;
                        }
                        else if (bufferCurrent - requestedAmount < 0)
                        {
                            float[] tempfloat = new float[] { bufferCurrent / 3f, bufferCurrent / 3f, bufferCurrent / 3f };
                            //or transfer all that remains in the buffer
                            cable.TransferIn(routerLegReq, tempfloat, 2);
                            bufferCurrent = 0f;
                        }
                    }
                }
            }
        }

        public void SetSubstations(IRoutingSubstation[] newSubs)
        {
            subRouters = newSubs;
        }

        public int SubRoutersLength()
        {
            return subRouters.Length;
        }
        public IRoutingSubstation[] SubStations
        {
            get { return subRouters; }
        }
        public int GetLegRequirement()
        {
            return legsRequired;
        }

        public Guid getGUID()
        {
            return guid;
        }

        public void ReceivePowerFromGenerator(int legCount, float[] powerAmounts)
        {
            //Debug.Log("Received power from generator: "+ (powerAmounts[0]*legCount));
            //receive 3 legs of X amount
            lastReceived = 0f;
            for (int i = 0; i < legCount; i++)
            {
                bufferCurrent += powerAmounts[i];
                lastReceived += powerAmounts[i];
            }
            legsReceived = legCount;
        }
        /// <summary>
        /// A simplified input func for turbines
        /// </summary>
        /// <param name="powerAmount"></param>
        public void ReceivePowerFromTurbine(float powerAmount)
        {
            //Debug.Log("Received power from turbine: "+ powerAmount);
            lastReceived = 0f;
            bufferCurrent += powerAmount;
            if(bufferCurrent > BufferMax)
            {
                bufferCurrent = BufferMax;
            }
            lastReceived += powerAmount;
            legsReceived = 4;
        }

        public float getTotalRequiredPower()
        {
            return totalRequiredPower;
        }
    }
}