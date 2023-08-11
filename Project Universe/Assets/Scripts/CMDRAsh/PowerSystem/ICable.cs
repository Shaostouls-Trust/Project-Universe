using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using ProjectUniverse.Util;

namespace ProjectUniverse.PowerSystem
{
	public class ICable
	{
		public IGenerator gen;
		public IRouter route;
		public IRoutingSubstation subst;
		public IBreakerBox breaker;
		public IMachine mach;
		public ISubMachine subMach;

		private float requestedEnergy;
		private float maximumThroughput;

		private float maxHeatCap;
		private float heatAmount;

		private int activeLegs; //number of legs transmitting
		private int maxActiveLegs; //number of legs in cable
								   //public ILeg[] cableLegs; //leg array

		//carry power from generator to a single router -- 500mm cable
		public ICable(IGenerator generator, IRouter router)
		{
			route = router;
			gen = generator;
			maximumThroughput = 181200f;//EHV max
			maxHeatCap = 288000f;
			maxActiveLegs = 3;
		}

		//carry power from router to a substation -- 250mm cable
		public ICable(IRouter router, IRoutingSubstation substation)
		{
			subst = substation;
			route = router;
			maximumThroughput = 95000f;//MVmin + 360 (enough for 6 at 120)
			maxHeatCap = 72000f;
			maxActiveLegs = 3;
		}

		//power from substation to machine -- 150mm cable
		public ICable(IRoutingSubstation substation, IMachine machine)
		{
			mach = machine;
			subst = substation;
			maximumThroughput = 12000f;//LVmax (one heavy machine)360 + 5%ish
			maxHeatCap = 24000f;
			maxActiveLegs = 3;
		}

		//power from substation to breaker box -- 100mm cable
		public ICable(IRoutingSubstation substation, IBreakerBox brBox)
		{
			subst = substation;
			breaker = brBox;
			maximumThroughput = 5400f;//LV low + 60 (30 connections. 30 lights is 60. 12 doors is 180 (actual is 11 b/c of drawToCharge)
									//Ideal is 9 doors (135) + 15 lights (30)).
			maxHeatCap = 12000f;
			maxActiveLegs = 3;
		}

		//power from breaker box to machine -- 50mm cable
		public ICable(IBreakerBox brBox, ISubMachine submachine)
		{
			breaker = brBox;
			subMach = submachine;
			maximumThroughput = 750f;//ELV med (5 + 20)
			maxHeatCap = 2000f;
			maxActiveLegs = 2;
		}

		//get power passed in
		public void TransferIn(int legCount, float[] powerinPerLeg, int type)
		{
			float powerinTotal = 0f;
			foreach (float amt in powerinPerLeg)
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
				//JobLogger.Log("heat: "+heatAmount+"/"+maxHeatCap);
			}
			else if (powerinTotal <= maximumThroughput)
			{
				heatAmount -= 0.01f * Utils.LastDeltaTime;//Time.deltaTime; //cooldown in 200 seconds from 1200f max, 60fps
				if (heatAmount <= 0f)
                {
					heatAmount = 0f;
                }
			}
			if (heatAmount > maxHeatCap)
			{
				//cable melts, later this will interact with surrounding objects
				//for now, just set energy transfer to 0
				TransferOut(0, new float[] { }, type);
			}
			else
			{
				TransferOut(legCount, powerinPerLeg, type);
			}
		}

		//pass power to the machine/router/etc requesting said power.
		private void TransferOut(int legCount, float[] powerOutPerLeg, int type)
		{
			ICable cable = this;
			switch (type)
			{
				case 1:
					//Debug.Log(powerOutPerLeg[0]+" "+powerOutPerLeg[1]+" "+powerOutPerLeg[1]+" to a router.");
					//transfer to a router
					route.ReceivePowerFromGenerator(legCount, powerOutPerLeg);
					break;
				case 2:
					//Debug.Log(powerOutPerLeg[0] + " " + powerOutPerLeg[1] + " " + powerOutPerLeg[2] + " to a substation.");
					//transfering to a substation
					subst.ReceivePowerFromRouter(legCount, powerOutPerLeg);
					break;
				case 3:
					//Debug.Log(powerOutPerLeg[0] + " " + powerOutPerLeg[1] + " " + powerOutPerLeg[2] + " to a machine.");
					//transfering to a machine
					cable = this;
					mach.ReceiveEnergyAmount(legCount, powerOutPerLeg, ref cable);
					break;
				case 4:
					//Debug.Log(powerOutPerLeg[0] + " " + powerOutPerLeg[1] + " to a breaker.");
					//transfer to breakerBox
					breaker.ReceivePowerFromSubstation(legCount, powerOutPerLeg);
					break;
				case 5:
					//Debug.Log(powerOutPerLeg[0] + " to a submachine.");
					//transfering to a machine
					cable = this;
					subMach.ReceiveEnergyAmount(legCount, powerOutPerLeg, ref cable);
					break;
			}

		}

		public void SetRequestedEnergy(float request)
		{
			requestedEnergy = request;
		}
		public float GetRequestedEnergy()
		{
			return requestedEnergy;
		}

		//type 1 = generator to router; 2 = router to substation; 3 = subst to machine; 4 = subst to breakear; 5 = breaker to machine
		public Boolean CheckConnection(int type)
		{
			switch (type)
			{
				case 1:
					if (gen != null && route != null)
					{
						return true;
					}
					break;
				case 2:
					if (route != null && subst != null)
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
					if (breaker != null && subMach != null)
					{
						return true;
					}
					break;
			}
			return false;
		}

	}
}