using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class IRoutingSubstation : MonoBehaviour
{
    private Guid guid;
    /*
     * The purpose of this class is to manage a distribution station (PathBreaker)
     * This is mainly via levers and event-driven variables.
     */
    //the parent router supplying energy
    public IRouter sourceRouter;
    //power group or machine this unit provides power to.
    public IMachine[] targetMachine;
    private float[] requestedPower;
    private float totalRequiredPower;
    //private ICable cable;
    private LinkedList<ICable> iCableDLL = new LinkedList<ICable>();
    private float energyBufferMax;
    [SerializeField]
    private float bufferCurrent;

    // Start is called before the first frame update
    void Start()
    {
        energyBufferMax = 150f;
        bufferCurrent = 0f;
        guid = Guid.NewGuid();
        //create a cable between substation and machine/s
        for(int i = 0; i < targetMachine.Length; i++)
        {
            ICable cable = new ICable(this, targetMachine[i]);
            iCableDLL.AddLast(cable);
        }
    }

    // Update is called once per frame
    void Update()
    {
        totalRequiredPower = 0f;
        requestedPower = new float[targetMachine.Length];
        //totalRequiredPower = 0f;
        //float totalAmount = 0f;
        int numSuppliers = 0;
        //get connected machines' requested power.
        for(int i = 0; i < targetMachine.Length; i++)
        {
            float uniqueMachineAmount;
            //power required by a machine.
            uniqueMachineAmount = targetMachine[i].requestedEnergyAmount(ref numSuppliers);
            //For the case in which machines have different power draws, or otherwise do not require uniform amounts of power.
            //Power is tracked differently per machine
            requestedPower[i] = uniqueMachineAmount;
            totalRequiredPower += uniqueMachineAmount;
        }
        //transfer power to the linking cable.
        int itteration = 0;
        foreach (ICable cable in iCableDLL)
        {
            if (cable.checkConnection(3))//type is substation to machine linkage
            {
                if (bufferCurrent - requestedPower[itteration] >= 0)
                {
                    //transfer the uniquely requested amount to the machine
                    cable.transferIn(requestedPower[itteration], 3);
                    bufferCurrent -= requestedPower[itteration];
                }
                else if(bufferCurrent - requestedPower[itteration] < 0)
                {
                    Debug.Log("Power Shortage In Substation!");
                    //or transfer all that remains in the buffer
                    cable.transferIn(bufferCurrent, 3);
                    bufferCurrent = 0f;
                }
            }
            itteration++;
        }
    }

    public void receivePowerFromRouter(float power)
    {
        bufferCurrent += power;
        //Debug.Log("SubRouter received " + power);
    }

    public float getTotalRequiredPower()
    {
        return totalRequiredPower;
    }

    public Guid getGUID()
    {
        return guid;
    }
}
