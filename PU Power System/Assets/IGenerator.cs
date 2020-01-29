using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IGenerator : MonoBehaviour
{
    //base params that will be overritten by inheritors
    public float totalVoltage = 120f;
    public float currentVoltage = 0f;
    public int legs = 1;
    public int routers = 1;
    //connection params
    public IRouter[] routerConnections = new IRouter[]{};

    public IGenerator(float totalV, int legCnt, int routeCnt)
    {
        totalVoltage = totalV;
        legs = legCnt;
        routers = routeCnt;
        
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {

    }

    // Update is called once per frame
    protected virtual void Update()
    {

    }

    /*
     * Return the current voltage for the whole system
     */
    public float GetVoltage()
    {

    }

    /*
     * Return the current voltage for the specified leg
     */
     public float GetVoltage(ILeg leg)
     {

     }

    /*
     * Return the current voltage for the legs
     */
     public float[] GetVoltage(ILeg[] legs)
     {

     }

     /*
     * return the number of legs
     */
     public int GetLegCount()
    {
        return legs;
    }

    /*
     * Return the number of possible connections to routers
     */
     public int GetRouterCount()
    {
        return routers;
    }

    /*
     * Return the number of routers that are connected and fuctional
     */
     public int GetActiveRouters()
    {

    }

    /*
     * Try to connect to a router that is present on the same ship. Called on 10th update
     * Fails When: Only Hull, non-piped, or non-defined non-powered are between the two stations (and the router is not on the other end of the ship, etc.
     * Succedes When: Piped, powered non-defined spaces are between the two points.
     */
    public Boolean TryConnection()
    {

    }

    /*
     * Connect to a pathable router that has passed the TryConnection method.
     * This connection is a pointer, not an actual path or anything.
     */
    public void ConnectTo()
    {

    }

    /*
     * Get the info of a connected router
     */
    public void GetConnection()//should return a Router object (or just a descriptor object like RouterInfo? That would be a smaller thing to pass
    {

    }

    /*
     * Change the voltage for the system
     */
    public float ModVoltage(float decrVoltageBy)
    {

    }

    /*
     * Change the voltage for a specific leg
     */
    public float ModVoltage(float decrVoltageBy, ILeg leg)
    {

    }
}
