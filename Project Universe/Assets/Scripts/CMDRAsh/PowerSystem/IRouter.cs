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

    // Start is called before the first frame update
    void Start()
    {
        //create GUID
        guid = Guid.NewGuid();
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
        totalRequiredPower = 0f;
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
        //Debug.Log("Total Required: " + totalRequiredPower);
        //send it through to substations
        int itteration = 0;
        foreach (ICable cable in iCableDLL)
        {
            if (cable.checkConnection(2))//type is router to substation linkage
            {
                if (bufferCurrent - requestedPower[itteration] >= 0)
                {
                    //transfer the uniquely requested amount to the machine
                    cable.transferIn(requestedPower[itteration], 2);
                    bufferCurrent -= requestedPower[itteration];
                }
                else if (bufferCurrent - requestedPower[itteration] < 0)
                {
                    //Debug.Log("Power Shortage In Router!");
                    //or transfer all that remains in the buffer
                    cable.transferIn(bufferCurrent, 2);
                    bufferCurrent = 0f;
                }
            }
            itteration++;
        }   
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

    public void receivePowerFromGenerator(float amount)
    {
        bufferCurrent += amount;
    }

    public float getTotalRequiredPower()
    {
        return totalRequiredPower;
    }
}
