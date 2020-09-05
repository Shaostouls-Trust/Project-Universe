using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class IGenerator : MonoBehaviour
{
    [SerializeField]
    private int outputMax;//duh
    [SerializeField]
    private int generatorLevel;
    public string powerGrid;//grid that this generator is part of
    [SerializeField]
    private float outputCurrent;//duh
    [SerializeField]
    private int maxRouters; //level * 4
    [SerializeField]
    private IRouter[] routers;
    private Guid guid;
    private LinkedList<ICable> iCableDLL = new LinkedList<ICable>();
    private float[] requestedRouterPower;

    //leg update
    private int legsOut;//calculate based on machines linked
    [SerializeField]
    private int availibleLegsOut;

    // Start is called before the first frame update
    void Start()
    {
        if (routers.Length > maxRouters)
        {
            routers = new IRouter[4];
        }
        outputCurrent = 0f;
        //create GUID
        guid = Guid.NewGuid();
        //look for routers based on max amount (level * 4)
        for(int i = 0; i < routers.Length; i++)
        {
            if (this.routers[i] != null)
            {
                //create an ICable node to add to the iCableDLL
                ICable myIcable = new ICable(this, this.routers[i]);
                //add it to the end of the DLL, if alone, it's first and last.
                iCableDLL.AddLast(myIcable);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        availibleLegsOut = routers.Length * 3;
        outputCurrent = 0f;
        //get leg states - this will be for when we have levers that close off indiv legs.
        //NYI
        //get requested power amount
        requestedRouterPower = new float[routers.Length];
        //float totalAmount = 0f;
        for (int i = 0; i < routers.Length; i++)
        {
            float uniqueRouterAmount = 0f;
            //power required by the router;
            uniqueRouterAmount = routers[i].getTotalRequiredPower();
            //Power requirement is tracked per router, duh.
            requestedRouterPower[i] = uniqueRouterAmount;
            //totalAmount += uniqueRouterAmount;
        }
        //send it through to routing stations
        int itteration = 0;
        //transfer power
        foreach (ICable cable in iCableDLL)
        {
            //get subst's leg req
            int routerLegReq = cable.route.getLegRequirement();
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
                powerAmount[l] = requestedRouterPower[itteration] / routerLegReq;
            }
            //Debug.Log("Request is " + requestedRouterPower[itteration]);
            if (outputCurrent + requestedRouterPower[itteration] <= outputMax)
            {
                //transfer as much power as is needed, up until capacity is met.
                //cable.transferIn(requestedRouterPower[itteration], 1);
                cable.transferIn(routerLegReq, powerAmount, 1);
                availibleLegsOut -= routerLegReq;
                outputCurrent += requestedRouterPower[itteration];
            }
            else
            {
                //Debug.Log("Output MAX!");
                //Debug.Log("Request is "+requestedRouterPower[itteration]+" and current output is "+outputCurrent);
                float[] tempfloat = new float[] { outputMax - outputCurrent/3, outputMax - outputCurrent/ 3, outputMax - outputCurrent/ 3 };
                cable.transferIn(routerLegReq, tempfloat, 1);
                outputCurrent = outputMax;
            }
            itteration++;
        }
    }

    public Boolean checkConnection()
    {
        return true;
    }

    public Guid getGUID()
    {
        return guid;
    }
}