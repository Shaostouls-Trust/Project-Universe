using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
        private int routerCap;
        private IRouter thisRouter;
        private IGenerator supplyingGenerator;

        //power legs update
        private int legsRequired = 3;
        private int legsReceived;
        private int legsOut;//calculate based on machines linked
        [SerializeField]
        private int availibleLegsOut;

        void Start()
        {
            //create GUID
            guid = Guid.NewGuid();
            //legsOut = subRouters.Length;
            energyBufferMax = 4320.0f;
            ProxyStart();
        }

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
                //Array.Copy(subRouters, routTemp,subRouters.Length-routerCap);
                //subRouters.CopyTo(routTemp, 0); //Destination array not long enough
                //routTemp = subRouters;// = new IRouter[4];
                //subRouters = new IRoutingSubstation[routerCap];
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
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            availibleLegsOut = subRouters.Length * 3;//3 legs per substation
            totalRequiredPower = 0f;
            //get leg states - this will be for when we have levers that close off indiv legs.
            //NYI
            //get requested power amount
            for (int i = 0; i < subRouters.Length; i++)
            {
                totalRequiredPower += subRouters[i].GetTotalRequiredPower();
            }
            //power request to generator logic
            if (bufferCurrent < energyBufferMax)
            {
                if (supplyingGenerator != null)
                {
                    //request power from generator
                    supplyingGenerator.RequestPowerFromGenerator(totalRequiredPower, thisRouter);
                }
            }
            if (bufferCurrent >= energyBufferMax)
            {
                totalRequiredPower = 0.0f;
                bufferCurrent = energyBufferMax;
            }

            /*
            requestedPower = new float[iCableDLL.Count];
            for (int i = 0; i < iCableDLL.Count; i++)
            {
                float uniqueRouterAmount;
                //power required by the subrouter;
                uniqueRouterAmount = subRouters[i].getTotalRequiredPower();
                //For the case in which machines have different power draws, or otherwise do not require uniform amounts of power.
                //Power is tracked differently per machine
                requestedPower[i] = uniqueRouterAmount;
                totalRequiredPower += uniqueRouterAmount;
            }
            */
            //power will be divided equally among linked substations.
            if (totalRequiredPower > bufferCurrent)
            {
                float defecit = totalRequiredPower - bufferCurrent;
                float defecitVsubs = defecit / totalRequiredPower;
                //  Debug.Log(defecitVsubs);
                //  for (int j = 0; j < subRouters.Length; j++)
                // {
                //subtract the amount to reduce (a percent of the requested amount)
                //requestedPower[j] -= (requestedPower[j] * defecitVsubs);
                //requestedPower[j] = (float)Math.Round(requestedPower[j], 3);
                // }
            }
            //Debug.Log("Total Required: " + totalRequiredPower);
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
                            float[] tempfloat = new float[] { bufferCurrent / 3, bufferCurrent / 3, bufferCurrent / 3 };
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
            //Debug.Log("Received power from generator: "+ powerAmounts[0]+" X3");
            //receive 3 legs of X amount
            for (int i = 0; i < legCount; i++)
            {
                bufferCurrent += powerAmounts[i];
            }
            legsReceived = legCount;
        }

        public float getTotalRequiredPower()
        {
            return totalRequiredPower;
        }
    }
}