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
    private float outputCurrent;//duh
    [SerializeField]
    private IRouter[] routers;
    private Guid guid;
    private LinkedList<ICable> iCableDLL = new LinkedList<ICable>();
    public float[] requestedRouterPower;

    // Start is called before the first frame update
    void Start()
    {
        outputCurrent = 0f;
        //create GUID
        guid = Guid.NewGuid();
        //look for routers based on generatorLevel
        for(int i = 0; i < generatorLevel; i++)
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
        outputCurrent = 0f;
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
            if (itteration < generatorLevel)
            {
                if (outputCurrent + requestedRouterPower[itteration] <= outputMax)
                {
                    //the first line (level number) will get power priority
                    //transfer as much power as is needed, up until capacity is met.
                    cable.transferIn(requestedRouterPower[itteration], 1);
                    outputCurrent += requestedRouterPower[itteration];
                }
                else
                {
                    Debug.Log("Output MAX!");
                    //Debug.Log("Request is "+requestedRouterPower[itteration]+" and current output is "+outputCurrent);
                    cable.transferIn((outputMax - outputCurrent), 1);
                }
            }
            else if((routers.Length - generatorLevel) <= 0)
            {
                //remainging, unallocated output
                //remaining power is divided between other connections
                float average = (outputCurrent - outputMax)/(routers.Length - generatorLevel);
                cable.transferIn(average, 1);
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