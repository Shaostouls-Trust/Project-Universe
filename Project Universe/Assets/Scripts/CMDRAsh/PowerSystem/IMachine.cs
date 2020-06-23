using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class IMachine : MonoBehaviour
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
    private ICable cable;
    private IRoutingSubstation[] substations;
    //backend of power cables
    private LinkedList<ICable> iCableDLL = new LinkedList<ICable>();
    //time control
    private float time = 0.1f; //10 times a second

    //temp stuff
    public Transform transform;
    public GameObject screenObject;
    public Light lightComponent;

    // Start is called before the first frame update
    void Start()
    {
        bufferCurrent = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        //reset requestedEnergy and drawToFill
        requestedEnergy = requiredEnergy;
        //drawToFill = requiredEnergy;

        //Recalculate drawToFill based on draw percent
        float floatDrawToFill = (float)percentDrawToFill;
        drawToFill = requiredEnergy + (requiredEnergy * (floatDrawToFill / 100)); //105% or 110% draw
        //If the energy buffer is not full
        if (bufferCurrent < energyBuffer)//(energyBuffer-requiredEnergy))
        {
            //Get the deficit between the energybuffer(max) and the current buffer amount
            float deficit = energyBuffer - bufferCurrent;
            if(deficit >= (drawToFill - requiredEnergy))
            {
                requestedEnergy = drawToFill;
            }
            else if(deficit < (drawToFill - requiredEnergy))
            {
                requestedEnergy = deficit + requiredEnergy;
            }
        }
        else if(bufferCurrent >= energyBuffer)
        {
            requestedEnergy = requiredEnergy;
            //trim off excess power. Buffers cannot overcharge
            bufferCurrent = energyBuffer;
        }
        else
        {
            requestedEnergy = requiredEnergy;
        }
        ///////////////////////////////////////
        //Run logic
        ///////////////////////////////////////
        if (bufferCurrent > 0f)
        {
            if (bufferCurrent - requiredEnergy < 0.0f)//not enough power to run at full
            {
                if (bufferCurrent >= requiredEnergy * 0.75f)//75% power
                {
                    runMachineSelector(machineType, 1);
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

    public float requestedEnergyAmount(ref int numSuppliers)
    {
        numSuppliers += 1;
        if(iCableDLL.Count > 1)
        {
            //correct for multiple inputs
            return requestedEnergy / iCableDLL.Count;
        }
        else
        {
            return requestedEnergy;
        }
    }
    
    public void receiveEnergyAmount(float amount, ref ICable cable)
    {
        bufferCurrent += amount;
        //Debug.Log(this + " added " + amount + " to buffer, making buffer " + bufferCurrent);
        //round buffer current to 3 places to avoid having a psychotic meltdown
        bufferCurrent = (float)Math.Round(bufferCurrent, 3);
        if (!iCableDLL.Contains(cable))
        {
            iCableDLL.AddLast(cable);
        }
    }
    //called on cable disconnect (NYI)
    public void removeCableConnection(ICable cable)
    {
        iCableDLL.Remove(cable);
    }

    public void runMachineSelector(string ImachineType, int powerLevel)
    {
        if (machineType == "light_point")
        {
            this.runMachinePointLight(powerLevel);
        }
        else if (machineType == "door")
        {
            this.runMachineDoor(powerLevel);
        }
        else if (machineType == "machine_basic")
        {
            this.runMachineNormal(powerLevel);
        }
        else if (machineType == "machine_adv")
        {
            this.runMachineAdv(powerLevel);
        }
        else if (machineType == "kiosk")
        {
            this.runMachineKiosk(powerLevel);
        }
    }

    public void runMachineDoor(int powerLevel)//reference attached animation controller script
    {
        var powerControl = this.GetComponent<DoorAnimator>();
        var speedControl = this.GetComponent<DoorAnimator>();
        //access the "Door Animator" script and toggle the control variable
        //TODO
        switch (powerLevel)
        {
            case 0:
                powerControl.setPoweredState(true);
                speedControl.setAnimSpeed(1.0f);//NYI
                break;
            case 1:
                powerControl.setPoweredState(true);
                speedControl.setAnimSpeed(0.75f);//NYI
                break;
            case 2:
                powerControl.setPoweredState(true);
                speedControl.setAnimSpeed(0.5f);//NYI
                break;
            case 3:
                powerControl.setPoweredState(true);
                speedControl.setAnimSpeed(0.15f);//NYI
                break;
            case 4:
                powerControl.setPoweredState(false);
                speedControl.setAnimSpeed(0.0f);//NYI  
                break;
        }
    }

    public void runMachineNormal(int powerLevel)
    {
        switch (powerLevel)
        {
            case 0:
                transform.Rotate(0.0f, 50.0f * Time.deltaTime, 0.0f);
                break;
            case 1:
                transform.Rotate(0.0f, 25.0f * Time.deltaTime, 0.0f);//should be 0?
                break;
            case 2:
                transform.Rotate(0.0f, 12.5f * Time.deltaTime, 0.0f);
                break;
            case 3:
                transform.Rotate(0.0f, 6.0f * Time.deltaTime, 0.0f);
                break;
            case 4:
                transform.Rotate(0.0f, 0.0f * Time.deltaTime, 0.0f);
                break;
        }
    }

    public void runMachineAdv(int powerLevel)
    {
        switch (powerLevel)
        {
            case 0:
                transform.Rotate(0.0f, 50.0f * Time.deltaTime, 0.0f);
                break;
            case 1:
                transform.Rotate(0.0f, 25.0f * Time.deltaTime, 0.0f);//should be 0?
                break;
            case 2:
                transform.Rotate(0.0f, 12.5f * Time.deltaTime, 0.0f);
                break;
            case 3:
                transform.Rotate(0.0f, 6.0f * Time.deltaTime, 0.0f);
                break;
            case 4:
                transform.Rotate(0.0f, 0.0f * Time.deltaTime, 0.0f);
                break;
        }
    }

    public void runMachinePointLight(int powerLevel)
    {
        switch (powerLevel)
        {
            case 0:
                lightComponent.intensity = 1.0f;
                lightComponent.range = 10.0f;
                break;
            case 1:
                lightComponent.intensity = 0.5f; //base is 1.0f
                lightComponent.range = 8.0f; //was 10.0f
                break;
            case 2:
                lightComponent.intensity = UnityEngine.Random.Range(0.3f, 0.4f);//0.4f; (.3 to .4)
                lightComponent.range = UnityEngine.Random.Range(5.0f, 6.0f);//6.0f; (5.0 to 6.0)
                break;
            case 3:
                lightComponent.intensity = UnityEngine.Random.Range(0.05f, 0.15f);//0.1f; (.05 to .15)
                lightComponent.range = UnityEngine.Random.Range(3.0f, 1.0f); //4.0f; (3.0 to 4.0f)
                break;
            case 4:
                lightComponent.intensity = 0.0f;
                lightComponent.range = 0.0f;
                break;
        }
    }

    public void runMachineKiosk(int powerlevel)
    {
        //grab the screen's material
        Renderer screenRenderer = screenObject.GetComponent<Renderer>();
        switch (powerlevel)
        {
            case 0:
                screenRenderer.material.SetFloat("Emiss Intensity", 0.25f);
                break;
            case 1:
                break;
            case 2:
                break;
            case 3:
                break;
            case 4:
                screenRenderer.material.SetFloat("Emiss Intensity", 0.0f);
                break;
        }
    }
}