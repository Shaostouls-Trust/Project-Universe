using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IMachine : MonoBehaviour
{
    //Amount required (requested) to run machine
    [SerializeField]
    private float requiredEnergy;
    //amount to draw when filling the interal buffer
    private float drawToFill;
    //unadjusted amount required to run the machine (see update block)
    private float baseRequirement;
    [SerializeField]
    private float energyBuffer; //Machines shouldn't store more than 10 frames worth of power.
    [SerializeField]
    private float bufferCurrent;
    private ICable cable;
    private IRoutingSubstation[] substations;
    //backend of power cables
    private LinkedList<ICable> iCableDLL = new LinkedList<ICable>();

    //temp stuff
    public Transform transform;

    // Start is called before the first frame update
    void Start()
    {
        drawToFill = requiredEnergy + 25f;
        baseRequirement = requiredEnergy;
        bufferCurrent = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        //reset requiredEnergy and drawToFill
        requiredEnergy = baseRequirement;
        drawToFill = requiredEnergy + 25f;
        //get connected substations
        if (iCableDLL.Count > 1)
        {
            //adjust base values for incomming connections
            requiredEnergy /= iCableDLL.Count;
            drawToFill /= iCableDLL.Count;
        }
        //run the machine
        /* 100% power: Full speed
         * >=75%: Speed reduced to .5 by lerp
         * >=50%: Partial functionality. Slow processing, slow doors, bad lighting, etc.
         * <50%: Non-functional
        */
        //Check internal power amount
        if (bufferCurrent > 0f)
        {

            if (bufferCurrent - baseRequirement < 0.0f)//not enough power to run at full
            {
                //Debug.Log("UNDERVOLT");
                if (bufferCurrent - baseRequirement >= baseRequirement * 0.75f)//75% power
                {
                    transform.Rotate(0.0f, 25.0f * Time.deltaTime, 0.0f);
                }
                else if (bufferCurrent - baseRequirement >= baseRequirement * 0.5f)//no lower than 50%
                {
                    transform.Rotate(0.0f, 15.0f * Time.deltaTime, 0.0f);//should be 0?
                }
                else//lower than 50%
                {
                    
                }
            }
            else
            {
                transform.Rotate(0.0f, 50.0f * Time.deltaTime, 0.0f);
                bufferCurrent -= baseRequirement;
            }
        }
        else//off behavior
        {
            //Debug.Log("!-> UNDERVOLT <-!");
        }
        //temp storage variable for baseRequirement
        float tempBase = baseRequirement / iCableDLL.Count;
        //If the energy buffer is not full //less than 90% capacity
        if (bufferCurrent < (energyBuffer-baseRequirement))//max - constant draw to prevent constant fill draw?
        {
            //Increase draw to fill the energy buffer
            float defecit = energyBuffer - bufferCurrent;
            //Debug.Log("Amount in buffer: "+ bufferCurrent);
            if (defecit >= 25.0f)
            {
                //increase to max draw
                requiredEnergy = drawToFill;
            }
            if(defecit < 25.0f)
            {
                //increase to defecit
                requiredEnergy = (tempBase + defecit);  
            }      
        }
        //full or overfull
        else if(bufferCurrent >= energyBuffer)
        {
            requiredEnergy = 0.0f;
        }
        else
        {
            requiredEnergy = tempBase;
        }
        //Debug.Log(this+" Requesting: " + requiredEnergy);
    }

    public float requestedEnergyAmount(ref int numSuppliers)
    {
        numSuppliers += 1;
        return requiredEnergy;
    }
    public void receiveEnergyAmount(float amount, ref ICable cable)
    {
        //Debug.Log("Received " + amount);
        bufferCurrent += amount;
        if (!iCableDLL.Contains(cable))
        {
            iCableDLL.AddLast(cable);
        }
    }
    //called on cable disconnect
    public void removeCableConnection(ICable cable)
    {
        iCableDLL.Remove(cable);
    }
}
