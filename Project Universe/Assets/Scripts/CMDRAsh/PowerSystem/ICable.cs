using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

public class ICable
{
	public IGenerator gen;
	public IRouter route;
	public IRoutingSubstation subst;
	public IBreakerBox breaker;
	public IMachine mach;

	private float requestedEnergy;
	private float maximumThroughput;

	private float maxHeatCap;
	private float heatAmount;

	private int activeLegs; //number of legs transmitting
	private int maxActiveLegs; //number of legs in cable
	//public ILeg[] cableLegs; //leg array

	//carry power from generator to a single router
	public ICable(IGenerator generator, IRouter router)
	{
		route = router;
		gen = generator;
		maximumThroughput = 4320;//EHV max
		maxHeatCap = 14400f;
		maxActiveLegs = 3;
	}

	//carry power from router to a substation
	public ICable(IRouter router, IRoutingSubstation substation)
    {
		subst = substation;
		route = router;
		maximumThroughput = 1080;//MVmin + 360 (enough for 6 at 120)
		maxHeatCap = 3600f;
		maxActiveLegs = 3;
	}

	//power from substation to machine
	public ICable(IRoutingSubstation substation, IMachine machine)
    {
		mach = machine;
		subst = substation;
		maximumThroughput = 360;//LVmax
		maxHeatCap = 1200f;
		maxActiveLegs = 3;
	}

	//power from substation to breaker box
	public ICable(IRoutingSubstation substation, IBreakerBox brBox)
    {
		subst = substation;
		breaker = brBox;
		maximumThroughput = 120;//LV low
		maxHeatCap = 400f;
		maxActiveLegs = 1;
	}

	//power from breaker box to machine
	public ICable(IBreakerBox brBox, IMachine machine)
    {
		breaker = brBox;
		mach = machine;
		maximumThroughput = 5;//EVL low
		maxHeatCap = 15f;
		maxActiveLegs = 1;
	}

	//get power passed in
	public void transferIn(int legCount, float[] powerinPerLeg, int type)
    {
		float powerinTotal = 0f;
		foreach(float amt in powerinPerLeg)
        {
			powerinTotal += amt;
        }
		//Debug.Log("power total: "+powerinTotal);
        //check capacity.
		//if over throughput limit, begin generating heat
        if (powerinTotal > maximumThroughput)
        {
			float overcap = powerinTotal - maximumThroughput;
			heatAmount += overcap * .01f;//100 seconds at 10+, 60 fps, 1200f max (*.02f).
			heatAmount = (float)Math.Round(heatAmount, 3);
			//overcap will cause sparks
			//Debug.Log("heat: "+heatAmount+"/"+maxHeatCap);
        }
		else if(powerinTotal <= maximumThroughput)
        {
			heatAmount -= 0.1f * Time.deltaTime; //cooldown in 200 seconds from 1200f max, 60fps
        }
		if (heatAmount > maxHeatCap)
		{
			//cable melts, later this will interact with surrounding objects
			//for now, just set energy transfer to 0
			transferOut(0, new float[]{}, type);
        }
        else
        {
			transferOut(legCount, powerinPerLeg, type);
        }
    }

	//pass power to the machine/router/etc requesting said power.
	private void transferOut(int legCount, float[] powerOutPerLeg, int type)
    {
		ICable cable = this;
		switch (type)
        {
			case 1:
				//Debug.Log(powerOutPerLeg[0]+" "+powerOutPerLeg[1]+" "+powerOutPerLeg[1]+" to a router.");
				//transfer to a router
				route.receivePowerFromGenerator(legCount, powerOutPerLeg);
				break;
			case 2:
				//Debug.Log(powerOutPerLeg[0] + " " + powerOutPerLeg[1] + " " + powerOutPerLeg[2] + " to a substation.");
				//transfering to a substation
				subst.receivePowerFromRouter(legCount, powerOutPerLeg);
				break;
			case 3:
				//Debug.Log(powerOutPerLeg[0] + " " + powerOutPerLeg[1] + " " + powerOutPerLeg[2] + " to a machine.");
				//transfering to a machine
				cable = this;
				mach.receiveEnergyAmount(legCount, powerOutPerLeg, ref cable);
				break;
			case 4:
				//Debug.Log(powerOutPerLeg[0] + " " + powerOutPerLeg[1] + " to a breaker.");
				//transfer to breakerBox
				breaker.receivePowerFromSubstation(legCount, powerOutPerLeg);
				break;
			case 5:
				//Debug.Log(powerOutPerLeg[0] + " to a machine.");
				//transfering to a machine
				cable = this;
				mach.receiveEnergyAmount(legCount, powerOutPerLeg, ref cable);
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

	//type 1 = generator to router; 2 = router to substation; 3 = subst to machine; 4 = subst to breakear; 5 = breaker to machine
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
			case 4:
				if (subst != null && breaker != null)
                {
					return true;
                }
				break;
			case 5:
				if (breaker != null && mach != null)
                {
					return true;
                }
				break;
        }
		return false;
	}

}