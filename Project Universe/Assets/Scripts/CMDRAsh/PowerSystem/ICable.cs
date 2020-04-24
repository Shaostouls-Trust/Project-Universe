using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

public class ICable
{
	public IGenerator gen;
	public IRouter route;
	public IRoutingSubstation subst;
	public IMachine mach;

	private float requestedEnergy;
	private float maximumThroughput;

	private float maxHeatCap;
	private float heatAmount;

	//carry power from generator to router
	public ICable(IGenerator generator, IRouter router)
	{
		route = router;
		gen = generator;
		maximumThroughput = 1080;//HV max
		maxHeatCap = 5400f;
	}

	//carry power from router to a substation
	public ICable(IRouter router, IRoutingSubstation substation)
    {
		subst = substation;
		route = router;
		maximumThroughput = 720;//MVmin + 360 (enough for 6 at 120)
		maxHeatCap = 2400f;
	}

	//power from router to machine
	public ICable(IRoutingSubstation substation, IMachine machine)
    {
		mach = machine;
		subst = substation;
		maximumThroughput = 360;//LVmax
		maxHeatCap = 1200f;
	}

	//get power passed in
	public void transferIn(float powerin, int type)
    {
        //check capacity, loss over distance, etc.
		//if over throughput limit, begin generating heat
        if (powerin > maximumThroughput)
        {
			float overcap = powerin - maximumThroughput;
			heatAmount += overcap * .01f;//100 seconds at 10+, 60 fps, 1200f max (*.02f).
			Debug.Log("heat: "+heatAmount+"/"+maxHeatCap);
        }
		else if(powerin <= maximumThroughput * Time.deltaTime)
        {
			heatAmount -= 0.1f * Time.deltaTime; //cooldown in 200 seconds from 1200f max, 60fps
        }
		if(heatAmount > maxHeatCap)
        {
			//cable melts, later this will interact with surrounding objects
			//for now, just set energy transfer to 0
			transferOut(0f, type);
        }
        else
        {
			transferOut(powerin, type);
        }
    }

	//pass power to the machine/router/etc requesting said power.
	private void transferOut(float powerOut, int type)
    {
        switch (type)
        {
			case 1:
				//transfer to a router
				route.receivePowerFromGenerator(powerOut);
				break;
			case 2:
				//transfering to a substation
				subst.receivePowerFromRouter(powerOut);
				break;
			case 3:
				//transfering to a machine
				ICable cable = this;
				mach.receiveEnergyAmount(powerOut, ref cable);
				break;
        }

    }

	public void setRequestedEnergy(float request)
    {
		requestedEnergy = request;
    }
	public float getRequestedEnergy()
    {
		return requestedEnergy;
    }

	//type 1 = generator to router; 2 = router to substation; 3 = subst to machine
	public Boolean checkConnection(int type)
    {
        switch (type)
        {
			case 1:
				if(gen != null && route != null)
                {
					return true;
                }
				break;
			case 2:
				if(route != null && subst != null)
                {
					return true;
				}
				break;
			case 3:
				if (subst != null && mach != null)
                {
					return true;
				}
				break;
        }
		return false;
	}

}
