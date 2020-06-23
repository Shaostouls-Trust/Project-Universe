using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/*
 * The purpose of this class is to distribute power to large amounts of small IMachines (not unlike IRoutingSubtation, save simpler).
 */
public class IBreakerBox : MonoBehaviour
{
    private Guid guid;
    //power group or machine this unit provides power to.
    public IMachine[] targetMachine;
    private float[] requestedPower;
    private float totalRequiredPower;
    //private ICable cable;
    private LinkedList<ICable> iCableDLL = new LinkedList<ICable>();
    private float energyBufferMax;
    [SerializeField]
    private float bufferCurrent;

    void Start()
    {
        energyBufferMax = 50.0f;//buffer is the size of 1-update draw?
        bufferCurrent = 0f;
        totalRequiredPower = 0.0f;
        guid = Guid.NewGuid();
        //create a cable between breaker and machine/s
        for (int i = 0; i < targetMachine.Length; i++)
        {
            ICable cable = new ICable(this, targetMachine[i]);
            iCableDLL.AddLast(cable);
        }
    }

    void Update()
    {
        totalRequiredPower = 0f;
        requestedPower = new float[targetMachine.Length];
        int numSuppliers = 0;
        //get connected machines' requested power.
        for (int i = 0; i < targetMachine.Length; i++)
        {
            float uniqueMachineAmount;
            //power required by a machine.
            uniqueMachineAmount = targetMachine[i].requestedEnergyAmount(ref numSuppliers);
            requestedPower[i] = uniqueMachineAmount;
            totalRequiredPower += uniqueMachineAmount;
        }
        
        //power will be divided equally among linked machines. (BUGGED. If values not the same, non-uniform distribution.)
        if (totalRequiredPower > bufferCurrent)
        {
            //Debug.Log("Breaker dividing:" + bufferCurrent);
            float defecit = totalRequiredPower - bufferCurrent;
            float defecitVbreaker = defecit / totalRequiredPower;
            for (int j = 0; j < targetMachine.Length; j++)//loop through once more
            {
                //subtract the amount to reduce (a percent of the requested amount)
                requestedPower[j] -= (requestedPower[j] * defecitVbreaker);
                //Debug.Log("Request " + j + " is " + requestedPower[j]);
                //round to 3 decimal places for sake of not going insane
                requestedPower[j] = (float)Math.Round(requestedPower[j], 3);
                //Debug.Log(this + " power after deficit adjustment " + requestedPower[j]);
            }
        }
        
        //transfer power to the linking cable.
        int itteration = 0;
        foreach (ICable cable in iCableDLL)
        {
            if (cable.checkConnection(5))//type is substation to machine linkage
            {
                if (bufferCurrent - requestedPower[itteration] >= 0)
                {
                    //transfer the uniquely requested amount to the machine
                    cable.transferIn(requestedPower[itteration], 5);
                    bufferCurrent -= requestedPower[itteration];
                }
                else if (bufferCurrent - requestedPower[itteration] < 0)
                {
                    //or transfer all that remains in the buffer
                    cable.transferIn(bufferCurrent, 5);
                    bufferCurrent = 0f;
                }
            }
            itteration++;
        }
    }

    public void receivePowerFromSubstation(float power)
    {
        bufferCurrent += power;
        bufferCurrent = (float)Math.Round(bufferCurrent, 3);
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