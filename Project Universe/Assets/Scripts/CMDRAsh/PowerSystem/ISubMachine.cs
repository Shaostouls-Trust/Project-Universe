using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ISubMachine : MonoBehaviour
{
    //Amount required (requested) to run machine
    [SerializeField]
    private float requestedEnergy;
    //unadjusted amount required to run the machine
    [SerializeField]
    private float requiredEnergy;
    [SerializeField]
    private int percentDrawToFill;
    //amount to draw when filling the interal buffer
    private float drawToFill;
    [SerializeField]
    private float energyBuffer; //Machines shouldn't store more than 10 frames worth of power.
    [SerializeField]
    private float bufferCurrent;
    [SerializeField]
    private string machineType;
    [SerializeField]
    private bool runMachine;
    //private ICable cable;
    private List<IBreakerBox> breakers = new List<IBreakerBox>();
    //backend of power cables
    private LinkedList<ICable> iCableDLL = new LinkedList<ICable>();
    
    private Light lightComponent;
    public float maxLightIntensity;
    public float maxLightRange;

    //power legs update
    [SerializeField]
    private int legsRequired;
    private int legsReceived;
    void Start()
    {
        RunMachine = true;
        bufferCurrent = 0.0f;
    }

    void Update()
    {
        //reset requestedEnergy
        requestedEnergy = requiredEnergy;
        //Recalculate drawToFill based on draw percent
        float floatDrawToFill = (float)percentDrawToFill;
        drawToFill = requiredEnergy + (requiredEnergy * (floatDrawToFill / 100)); //105% or 110% draw
                                                                                  //If the energy buffer is not full
        if (bufferCurrent < energyBuffer)
        {
            //Get the deficit between the energybuffer(max) and the current buffer amount
            float deficit = energyBuffer - bufferCurrent;
            if (deficit >= (drawToFill - requiredEnergy))
            {
                //send energy request
                requestedEnergy = drawToFill;
                //Debug.Log(this.gameObject.name+" Request Helper");
                requestHelper();

            }
            else if (deficit < (drawToFill - requiredEnergy))
            {
                //send energy request
                requestedEnergy = deficit + requiredEnergy;
                //Debug.Log(this.gameObject.name + " Request Helper");
                requestHelper();
            }
        }
        else if (bufferCurrent >= energyBuffer)
        {
            //send request
            requestedEnergy = requiredEnergy;
            //requestedEnergy = 0.0f;
            //Debug.Log(this.gameObject.name + " Request Helper");
            requestHelper();
        }
        else
        {
            requestedEnergy = requiredEnergy;
            //Debug.Log(this.gameObject.name + " Request Helper");
            requestHelper();
        }
        if (runMachine)
        {
            //run machines
            //Debug.Log("Running "+this.gameObject.name);
            runLogic();
        }
        else
        {
            //turn the machine off
            runMachineSelector(machineType, 5);
        }
    }

    public void requestHelper()
    {
        foreach (IBreakerBox box in breakers)
        {
            //Debug.Log("request/breakCount: "+requestedEnergy/breakers.Count);
            box.requestPowerFromBreaker(requestedEnergy/breakers.Count, this);//this.GetComponent<ISubMachine>()
        }
    }

    public void runLogic()
    {
        ///////////////////////////////////////
        //Run logic
        ///////////////////////////////////////
        if (runMachine)
        {
            if (legsReceived == legsRequired)
            {
                //Debug.Log("Legs received");
                if (bufferCurrent > 0f)
                {
                    if (bufferCurrent - requiredEnergy < 0.0f)//not enough power to run at full
                    {
                        if (bufferCurrent >= requiredEnergy * 0.75f)//75% power
                        {
                            runMachineSelector(machineType, 1); //any slower locks emiss to blinking yellow.
                        }
                        else if (bufferCurrent >= requiredEnergy * 0.5f)//no lower than 50%
                        {
                            runMachineSelector(machineType, 2);
                        }
                        else//lower than 50%
                        {
                            runMachineSelector(machineType, 3);
                        }
                        //no matter what, the buffer is emptied
                        bufferCurrent = 0.0f;
                    }
                    else
                    {
                        //run full power
                        runMachineSelector(machineType, 0);
                        bufferCurrent -= requiredEnergy;
                    }
                }
                else
                {
                    //'run' at 0 power
                    runMachineSelector(machineType, 4);
                }
            }
            else if (legsReceived < legsRequired && legsReceived >= 1)
            {
                //Shut down machine due to leg requirement
                runMachineSelector(machineType, 4);
                //electrical damage (if the buffer is not empty)
                //if (bufferCurrent > 0)
                //{
                //NYI
                //}
            }
            else
            {
                //Shut down machine due to leg requirement
                runMachineSelector(machineType, 4);
                //NO electrical damage, because no legs attached.
            }
        }
        else
        {
            runMachineSelector(machineType, 4);
        }

    }

    public int getLegRequirement()
    {
        return legsRequired;
    }

    public bool RunMachine
    {
        get { return runMachine; }
        set { runMachine = value; }
    }

    public float requestedEnergyAmount() //ref int numSuppliers)
    {
        //numSuppliers += 1;
        if (iCableDLL.Count > 1)
        {
            //correct for multiple inputs
            return requestedEnergy / iCableDLL.Count;
        }
        else
        {
            return requestedEnergy;
        }
    }

    public void receiveEnergyAmount(int legCount, float[] amounts, ref ICable cable)
    {
        //receive X legs with X amounts
        for (int i = 0; i < legCount; i++)
        {
            bufferCurrent += amounts[i];
        }
        legsReceived = legCount;
        //Debug.Log("machine has "+legsReceived+" legs");
        //bufferCurrent += amount;
        //round buffer current to 3 places to avoid having a psychotic meltdown
        bufferCurrent = (float)Math.Round(bufferCurrent, 3);
        if (!iCableDLL.Contains(cable))
        {
            iCableDLL.AddLast(cable);
        }
        if(bufferCurrent > energyBuffer)
        {
            //trim off excess power. Buffers cannot overcharge
            bufferCurrent = energyBuffer;
        }
    }

    //called on cable disconnect (NYI)
    public void removeCableConnection(ICable cable)
    {
        iCableDLL.Remove(cable);
    }

    //called at the start of the breaker update block
    public bool checkMachineState(ref IBreakerBox myBreaker)
    {
        if (!breakers.Contains(myBreaker))
        {
            //Debug.Log("breaker added");
            breakers.Add(myBreaker);
        }
        return true;
    }

    public void runMachineSelector(string ImachineType, int powerLevel)
    {
        switch (ImachineType)
        {
            case "light_point":
                this.runMachinePointLight(powerLevel);
                break;
            case "door":
                this.runMachineDoor(powerLevel);
                break;
        }
    }

    public void runMachineDoor(int powerLevel)
    {
        var control = this.GetComponent<DoorAnimator>();
        switch (powerLevel)
        {
            case 0:
                control.setPoweredState(true);
                control.setRunningState(true);
                control.setAnimSpeed(1.0f);
                break;
            case 1:
                control.setPoweredState(true);
                control.setRunningState(true);
                control.setAnimSpeed(0.75f);
                break;
            case 2:
                control.setPoweredState(true);
                control.setRunningState(true);
                control.setAnimSpeed(0.5f);
                break;
            case 3:
                control.setPoweredState(true);
                control.setRunningState(true);
                control.setAnimSpeed(0.15f);
                break;
            case 4:
                control.setPoweredState(false);
                control.setRunningState(true);
                control.setAnimSpeed(0.0f);
                break;
            case 5:
                control.setRunningState(false);
                break;
        }
        //temp override for ship construction sake
        control.setPoweredState(true);
        control.setAnimSpeed(1.0f);
    }

    public void runMachinePointLight(int powerLevel)
    {
        lightComponent = this.gameObject.GetComponentInChildren<Light>();
        lightComponent.enabled = true;
        switch (powerLevel)
        {
            case 0:
                lightComponent.intensity = maxLightIntensity;
                lightComponent.range = maxLightRange;
                break;
            case 1:
                lightComponent.intensity = maxLightIntensity * 0.25f; //base is 500.0f
                lightComponent.range = maxLightRange * 0.75f; //base is 15.0f
                break;
            case 2:
                lightComponent.intensity = maxLightIntensity * UnityEngine.Random.Range(0.03f, 0.04f);//0.4f; (.3 to .4)
                lightComponent.range = maxLightRange * UnityEngine.Random.Range(.50f, .60f);//6.0f; (5.0 to 6.0)
                break;
            case 3:
                lightComponent.intensity = maxLightIntensity * UnityEngine.Random.Range(0.005f, 0.015f);//0.1f; (.05 to .15)
                lightComponent.range = maxLightRange * UnityEngine.Random.Range(.30f, .10f); //4.0f; (3.0 to 4.0f)
                break;
            case 4:
                lightComponent.intensity = 0.0f;
                lightComponent.range = 0.0f;
                break;
            case 5:
                lightComponent.enabled = false;
                break;
        }
    }
}
