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
    //public IRouter sourceRouter;
    //power group or machine this unit provides power to.
    public IMachine[] targetMachine;
    public IBreakerBox[] targetBreakers;
    private float[] requestedPower;
    private float totalRequiredPower;
    //private ICable cable;
    private LinkedList<ICable> iCableDLL = new LinkedList<ICable>();
    private float energyBufferMax;
    private float bufferCurrent;

    // Start is called before the first frame update
    void Start()
    {
        energyBufferMax = 150f;
        bufferCurrent = 0f;
        totalRequiredPower = 0.0f;
        guid = Guid.NewGuid();
        //create a cable between substation and machine/s
        for(int i = 0; i < targetMachine.Length; i++)
        {
            if (targetMachine[i] != null)
            {
                ICable cable = new ICable(this, targetMachine[i]);
                iCableDLL.AddLast(cable);
            }
        }
        for(int i = 0; i < targetBreakers.Length; i++)
        {
            if (targetBreakers[i] != null)
            {
                ICable cable = new ICable(this, targetBreakers[i]);
                iCableDLL.AddLast(cable);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        totalRequiredPower = 0f;
        float totalMachineReq = 0f;
        float totalBreakerReq = 0f;
        requestedPower = new float[targetMachine.Length + targetBreakers.Length];
        //totalRequiredPower = 0f;
        //float totalAmount = 0f;
        int numSuppliers = 0;
        //get connected machines' requested power.
        for(int i = 0; i < targetMachine.Length; i++)
        {
            if (targetMachine[i] != null)
            {
                float uniqueMachineAmount;
                //power required by a machine.
                uniqueMachineAmount = targetMachine[i].requestedEnergyAmount(ref numSuppliers);
                //Power is tracked per machine
                requestedPower[i] = uniqueMachineAmount;
                totalRequiredPower += uniqueMachineAmount;
                totalMachineReq += uniqueMachineAmount;
            }
        }
        //repeat above for breakers
        //start at the index targetMachine stopped at
        int target = targetMachine.Length + targetBreakers.Length;
        for(int i = targetMachine.Length; i < target; i++)
        {
            if (targetBreakers[i - targetMachine.Length] != null)
            {
                float uniqueMachineAmount;
                uniqueMachineAmount = targetBreakers[i - targetMachine.Length].getTotalRequiredPower();
                requestedPower[i] = uniqueMachineAmount;
                totalRequiredPower += uniqueMachineAmount;
                totalBreakerReq += uniqueMachineAmount;
            }
        }
        //power will be divided equally among linked machines, sacrificing breaker power by as much as 75% in case of defecit.
        //ignore this division block if the substation buffer is empty
        if (bufferCurrent > 0) {
            if (totalBreakerReq + totalMachineReq > bufferCurrent)
            {
                //difference between required and available power.
                float deficit = totalRequiredPower - bufferCurrent;
                //determine the percent difference between defecit and the power required by the breakers
                float defecitVbreaker = deficit / totalBreakerReq;
                //if the deficit is 3/4 or less of the breaker requirement.
                if (defecitVbreaker <= 0.75f)
                {
                    for (int j = targetMachine.Length; j < target; j++)
                    {
                        if (targetMachine[j] != null)
                        {
                            //subtract the amount to reduce (a percent of the requested amount)
                            requestedPower[j] -= (requestedPower[j] * defecitVbreaker);
                            requestedPower[j] = (float)Math.Round(requestedPower[j], 3);
                            //Debug.Log(this + " breaker power after deficit adjustment " + requestedPower[j]);
                        }
                    }
                }
                else//assume defecit is greater than 75% the breaker requirement
                {
                    for (int j = targetMachine.Length; j < target; j++)
                    {
                        if (targetMachine[j] != null)
                        {
                            //we've cut off 75% of the breaker draw. Hence only 25% required
                            requestedPower[j] *= 0.25f;
                            requestedPower[j] = (float)Math.Round(requestedPower[j], 3);
                            //Debug.Log(this + " breaker power after deficit adjustment " + requestedPower[j]);
                        }
                    }
                    totalBreakerReq *= 0.25f;
                    //get the new power requirement.
                    float newRequiredPower = totalMachineReq + totalBreakerReq;
                    //recompute deficit based on reduced breaker draw.
                    deficit = newRequiredPower - bufferCurrent;
                    //calculate the percent that we need to trim off of the machine requirement to settle deficit
                    float machinePercentCut = deficit / newRequiredPower;//this includes the breaker's 25%
                    for (int j = 0; j < targetMachine.Length; j++)
                    {
                        if (targetMachine[j] != null)
                        {
                            requestedPower[j] -= (requestedPower[j] * machinePercentCut);
                            requestedPower[j] = (float)Math.Round(requestedPower[j], 3);
                            //Debug.Log(this + " power after deficit adjustment " + requestedPower[j]);
                        }
                    }
                }
            }
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
                    //or transfer all that remains in the buffer
                    cable.transferIn(bufferCurrent, 3);
                    bufferCurrent = 0f;
                }
            }
            else if (cable.checkConnection(4))//breaker to machine
            {
                if (bufferCurrent - requestedPower[itteration] >= 0)
                {
                    //transfer the uniquely requested amount to the machine
                    cable.transferIn(requestedPower[itteration], 4);
                    bufferCurrent -= requestedPower[itteration];
                }
                else if (bufferCurrent - requestedPower[itteration] < 0)
                {
                    //or transfer all that remains in the buffer
                    cable.transferIn(bufferCurrent, 4);
                    bufferCurrent = 0f;
                }
            }
            itteration++;
        }
    }

    public void receivePowerFromRouter(float power)
    {
        bufferCurrent += power;
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