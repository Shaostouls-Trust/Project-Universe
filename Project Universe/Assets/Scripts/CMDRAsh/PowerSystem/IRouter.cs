using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class IRouter : MonoBehaviour
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

    //power legs update
    private int legsRequired = 3;
    private int legsReceived;
    private int legsOut;//calculate based on machines linked
    [SerializeField]
    private int availibleLegsOut;

    // Start is called before the first frame update
    void Start()
    {
        //create GUID
        guid = Guid.NewGuid();
        //legsOut = subRouters.Length;
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
            subRouters = new IRoutingSubstation[4];
        }
        //set buffer current
        bufferCurrent = 0f;
        //look for router substations based on routerLevel
        for (int i = 0; i < subRouters.Length; i++)
        {
            if (this.subRouters[i] != null)//if the cell is empty
            {
                //create an ICable node to add to the iCableDLL
                ICable myIcable = new ICable(this, this.subRouters[i]);
                //add it to the end of the DLL, if alone, it's first and last.
                iCableDLL.AddLast(myIcable);
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
        //power will be divided equally among linked substations.
        if (totalRequiredPower > bufferCurrent)
        {
            float defecit = totalRequiredPower - bufferCurrent;
            float defecitVbreaker = defecit / totalRequiredPower;
            for (int j = 0; j < subRouters.Length; j++)
            {
                //subtract the amount to reduce (a percent of the requested amount)
                requestedPower[j] -= (requestedPower[j] * defecitVbreaker);
                requestedPower[j] = (float)Math.Round(requestedPower[j], 3);
            }
        }
        //Debug.Log("Total Required: " + totalRequiredPower);
        //send it through to substations
        int itteration = 0;
        foreach (ICable cable in iCableDLL)
        {
            //get subst's leg req
            int routerLegReq = cable.subst.getLegRequirement();
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
                powerAmount[l] = requestedPower[itteration] / routerLegReq;
            }
            if (cable.checkConnection(2))//type is router to substation linkage
            {
                if (bufferCurrent - requestedPower[itteration] >= 0)
                {
                    //transfer the uniquely requested amount to the router
                    //cable.transferIn(requestedPower[itteration], 2);
                    cable.transferIn(routerLegReq, powerAmount, 2);
                    availibleLegsOut -= routerLegReq;
                    bufferCurrent -= requestedPower[itteration];
                }
                else if (bufferCurrent - requestedPower[itteration] < 0)
                {
                    float[] tempfloat = new float[] { bufferCurrent / 3, bufferCurrent / 3, bufferCurrent / 3 };
                    //or transfer all that remains in the buffer
                    cable.transferIn(routerLegReq, tempfloat, 2);
                    //cable.transferIn(bufferCurrent, 3);
                    bufferCurrent = 0f;
                }
            }
            itteration++;
        }   
    }

    public int getLegRequirement()
    {
        return legsRequired;
    }

    public Guid getGUID()
    {
        return guid;
    }

    public Boolean bufferNotEmpty()
    {
        if(bufferCurrent > 0f)
        {
            return true;
        }
    return false;
    }

    public void receivePowerFromGenerator(int legCount, float[] powerAmounts)
    {
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