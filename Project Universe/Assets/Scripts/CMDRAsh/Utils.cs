using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectUniverse.Base;
using System.Reflection;
using System;
using ProjectUniverse.Environment.Gas;
using ProjectUniverse.Environment.Volumes;
using ProjectUniverse.Data.Libraries.Definitions;
using static ProjectUniverse.Environment.Volumes.VolumeConstructionSection;
using ProjectUniverse.Data.Libraries;
using ProjectUniverse.Environment.Fluid;
using ProjectUniverse.Animation.Controllers;

namespace ProjectUniverse.Util
{
    public static class Utils
	{
		public enum ImpactBehaviorType
        {
			NORM = 0,
			AP = 1,
			HE = 2,
			APHE = 3,
			HESH = 4
        }

		public static float LastDeltaTime = 0f;

		/// <summary>
		/// Sort the referenced list by whatever value is reflexively returned by the provided MethodInfo.
		/// The referenced list is directly modified in-place.
		/// </summary>
		/// <param name="list">, the ItemStack to be sorted according to:</param>
		/// <param name="sortBy">, the Method that each TArray item in the Itemstack will Invoke</param>
		/// <param name="highlevelsort">, true sorts list at the ItemStack level, false sorts at an index 0 low level.</param>
		public static void InsertionSort<T>(ref List<ItemStack> list, MethodInfo sortBy, bool highlevelsort) where T:IComparable//List<ItemStack>
		{
			for (int outer = 1; outer < list.Count; outer++)
			{
				int position = outer;
				//get the value to compare to position-1 (this value is the type we are comparing, IE an int/float/string/etc)
				ItemStack tempStack = new ItemStack(list[position]);
				if (highlevelsort)
				{
					var KeyZero = (T)sortBy.Invoke(list[position], null);
					while (position > 0 && KeyZero.CompareTo((T)sortBy.Invoke(list[position - 1], null)) < 0)//keyZero < KeyNext
					{
						list[position] = list[position - 1];
						position--;
					}
				}
				else
				{
					var KeyZero = (T)sortBy.Invoke(list[position].GetItemArray().GetValue(0), null);
					while (position > 0 && KeyZero.CompareTo((T)sortBy.Invoke(list[position - 1].GetItemArray().GetValue(0), null)) < 0)//keyZero < KeyNext
					{
						list[position] = list[position - 1];
						position--;
					}
				}
				list[position] = tempStack;
			}
		}

		/// <summary>
		/// Try to remove X amount from the provided gas. 
		/// Return the removed gas. Out param is the gas from which amount was taken.
		/// </summary>
		/// <param name="amount"></param>
		/// <param name="gasFrom"></param>
		/// <returns></returns>
		public static IGas SubtractGas(float amountPerSecond, IGas gasFrom, out IGas remainder)
		{
			float amount = amountPerSecond * LastDeltaTime;
			IGas returnGas = new IGas(gasFrom.GetIDName(), gasFrom.GetTemp(), 0.0f);
			returnGas.SetLocalPressure(gasFrom.GetLocalPressure());
			returnGas.SetLocalVolume(gasFrom.GetLocalVolume());
			float originalConc = gasFrom.GetConcentration();
			//subtract conc
			float remainingConc = gasFrom.GetConcentration() - amount;
			if (remainingConc > 0)
			{
				gasFrom.SetConcentration(remainingConc);
				returnGas.SetConcentration(amount);
			}
			else
			{
				returnGas.SetConcentration(gasFrom.GetConcentration());
				gasFrom.SetConcentration(0.0f);
			}
			///NOTE: IMPORTANT!
			///this pressure loss calculation is banjaxed, and is only used to remove pressure from the main duct
			///The pressure transfered into the outflow here is 2.5-3.0X below the needed 1.0m3 output to fill the 519m3 test room
			///to 1.0atm while also having +-100% oxygenation. This is likely due to the input volume being 0.4m3, not 1.0m3.
			///
			/// Due to this, pressure for the room is calculated INSIDE the room volume, based on wholesale concentration v room volume.
			///

			float gasFromPressurePercent = gasFrom.GetConcentration() / originalConc;
			gasFrom.SetLocalPressure(gasFrom.GetLocalPressure() * gasFromPressurePercent);
			float returnGasPressurePercent = returnGas.GetConcentration() / originalConc;
			returnGas.SetLocalPressure((returnGas.GetLocalPressure() * returnGasPressurePercent));

			remainder = gasFrom;
			return returnGas;
		}

		/// <summary>
		/// Equalize the Temperature and add the concentration of the two passed gasses. Then, recalc density.
		/// </summary>
		/// <param name="gasA"></param>
		/// <param name="gasB"></param>
		/// <returns></returns>
		public static IGas CombineGases(IGas gasA, IGas gasB, float localPressure)
		{
			float gasTemp;
			float gasConc;
			//float gasPressure;

			float gasAt = gasA.GetTemp();
			float gasBt = gasB.GetTemp();
			gasTemp = (gasAt + gasBt) / 2;
			gasA.SetTemp(gasTemp);

			gasConc = gasA.GetConcentration() + gasB.GetConcentration();
			gasA.SetConcentration(gasConc);

			//gas pressure and duct pressure are the same
			gasA.SetLocalPressure((float)Math.Round(localPressure, 4));
			gasA.CalculateAtmosphericDensity();
			return gasA;
		}

		/// <summary>
		/// Equalize the Temperature and add the concentration of the two passed fluids.
		/// </summary>
		/// <param name="gasA"></param>
		/// <param name="gasB"></param>
		/// <returns></returns>
		public static IFluid CombineFluids(IFluid fluidA, IFluid fluidB, float localPressure)
		{
			float gasTemp;
			float gasConc;
			//float gasPressure;

			float fluidAt = fluidA.GetTemp();
			float fluidBt = fluidB.GetTemp();
			gasTemp = (fluidAt + fluidBt) / 2;
			fluidA.SetTemp(gasTemp);

			gasConc = fluidA.GetConcentration() + fluidB.GetConcentration();
			fluidA.SetConcentration(gasConc);

			//gas pressure and duct pressure are the same
			fluidA.SetLocalPressure((float)Math.Round(localPressure, 4));
			//gasA.CalculateAtmosphericDensity();
			return fluidA;
		}

		/// <summary>
		/// Equalize to local volumes to the same relative gas levels
		/// </summary>
		/// <param name="VACa"></param>
		/// <param name="VACb"></param>
		public static void LocalVolumeEqualizer(VolumeAtmosphereController VACa, VolumeAtmosphereController VACb)
		{
			//Debug.Log("-------------- VACa -> "+VACa+"------------------");
			float VACaVolume = VACa.GetVolume();
			float VACbVolume = VACb.GetVolume();
			float totalVolume = VACaVolume + VACbVolume;
			//Debug.Log("Total Volume: "+totalVolume);

			//Equalize the gasses in the two volumes (over time)
			//get the volume ratio between each volume and the total volume
			//Get the total amount of each gas in both volumes
			//for each gas
			//find the eq concentration (concA + concB * ratioA, ratioB)
			//IE multiply the total concentration of each gas by ratioA and B
			List<IGas> jointGasses = new List<IGas>();

			float ratioA = VACaVolume / totalVolume;
			float ratioB = VACbVolume / totalVolume;
			//Debug.Log("Ratio A: " + ratioA);
			//Debug.Log("Ratio B: " + ratioB);
			
			foreach(IGas gas in VACa.RoomGasses)
			{
				jointGasses.Add(gas);
				//Debug.Log("Adding (a) "+gas);
			}
			bool ugh;
			foreach (IGas gas in VACb.RoomGasses)
			{
				ugh = false;
				for(int k = 0; k < jointGasses.Count; k++)
				{
					if (jointGasses[k].GetIDName().Equals(gas.GetIDName()))
					{
						ugh = true;
						jointGasses[k] = CombineGases(gas, jointGasses[k], jointGasses[k].GetLocalPressure());
						//Debug.Log("Combining -> " + jointGasses[k]);
						//break;
					}
				}
				if (!ugh)
				{
					jointGasses.Add(gas);
					//Debug.Log("Adding (b) " + gas);
				}
			}

			bool aGo = false;
			bool bGo = false;
			for(int c = 0; c < jointGasses.Count;c++)
			{
				foreach (IGas gas in VACa.RoomGasses)
				{
					if(gas.GetIDName().Equals(jointGasses[c].GetIDName()))
					{
						aGo = true;
						//Debug.Log(VACaVolume + "Set: [Joint]: " +jointGasses[c]+" TO: "+(jointGasses[c].GetConcentration() * ratioA));
						gas.SetConcentration(jointGasses[c].GetConcentration() * ratioA);
						gas.SetLocalVolume(VACaVolume);
						break;
					}
				}
				if (!aGo)
				{
					//Debug.Log("Adding gas to room a");
					IGas newG = new IGas(jointGasses[c]);
					newG.SetConcentration(jointGasses[c].GetConcentration() * ratioA);
					newG.SetLocalVolume(VACaVolume);
					VACa.AddRoomGas(newG);
				}
				aGo = false;
				foreach (IGas gas in VACb.RoomGasses)
				{
					if (gas.GetIDName().Equals(jointGasses[c].GetIDName()))
					{
						bGo = true;
						//Debug.Log(VACbVolume+ " Set: [Joint]:" + jointGasses[c] + " > " + (jointGasses[c].GetConcentration() * ratioB));
						gas.SetConcentration(jointGasses[c].GetConcentration() * ratioB);
						gas.SetLocalVolume(VACbVolume);
						break;
					}
				}
				if (!bGo)
				{
					//Debug.Log("Adding gas to room b");
					IGas newG = new IGas(jointGasses[c]);
					newG.SetConcentration(jointGasses[c].GetConcentration() * ratioB);
					newG.SetLocalVolume(VACbVolume);
					VACb.AddRoomGas(newG);
				}
				bGo = false;
			}

			float totalGas = VACa.CalculateRoomOxygenation();
			VACa.CalculateRoomPressure(totalGas);
			totalGas = VACb.CalculateRoomOxygenation();
			VACb.CalculateRoomPressure(totalGas);
			VACa.PostProcessVolumeUpdate();
			VACb.PostProcessVolumeUpdate();

		}

		/// <summary>
		/// Empty the room's gasses into the black and fill it with any local gasses.
		/// </summary>
		/// <param name="VAC"></param>
		/// <param name="VGAC"></param>
		public static void GlobalVolumeEqualizer(VolumeAtmosphereController VAC, VolumeGlobalAtmosphereController VGAC)
		{
			if (VAC.Pressure != VGAC.GetPressure())
			{
				VAC.Pressure = VGAC.GetPressure();
			}
			if (VAC.Oxygenation != VGAC.roomOxygenation)
			{
				VAC.Oxygenation = VGAC.roomOxygenation;
			}
			if (VAC.Temperature != VGAC.roomTemp)
			{
				VAC.Temperature = VGAC.roomTemp;
			}
			if (VAC.Toxicity != VGAC.toxicity)
			{
				VAC.Toxicity = VGAC.toxicity;
			}
			VAC.PostProcessVolumeUpdate();
		}

		/// <summary>
		/// Equalize water levels between two volumes over time.
		/// Water will flow at a rate proportional to the total height of plane 0
		/// Water will equalize such that the heights of the two relative planes are even
		/// Water can completely drain out of a room.
		/// </summary>
		public static void LocalFluidEqualization(VolumeAtmosphereController origin, VolumeAtmosphereController target,
			DoorAnimator originDoor, DoorAnimator targetDoor)
        {
			//Debug.Log("==========");
			//get water levels of each room
			float levelO = origin.WaterLevel(false);
			float levelT = target.WaterLevel(false);

			//if one level is below the door levels, there is no flow, so don't bother with the rest here
			//the world space to visual position is off by ~0.25f
			//Debug.Log((levelO+0.25f) +" > " + originDoor.transform.position.y +" || "+ (levelT + 0.25f) + " > "+ targetDoor.transform.position.y);
			if ((levelO+0.25f) > originDoor.transform.position.y || (levelT + 0.25f) > targetDoor.transform.position.y)
			{

				//Debug.Log(levelO + " & " + levelT);
				//get which doors are open
				int oI = 0;
				int tI = 0;
				for (int a = 0; a < origin.RoomDoorsFluidOrder.Length; a++)
				{
					if (origin.RoomDoorsFluidOrder[a] == originDoor)
					{
						oI = a;
						break;
					}
				}
				for (int b = 0; b < target.RoomDoorsFluidOrder.Length; b++)
				{
					if (target.RoomDoorsFluidOrder[b] == targetDoor)
					{
						tI = b;
						break;
					}
				}
				//zero the world space positions so that the lowest level is 0
				float offset = 0f;
				//Debug.Log(levelO + " " + levelT);

				if (levelO > levelT)
				{
					offset = levelO - levelT;
					levelO -= levelT;
					levelT = 0f;
				}
				else if (levelO < levelT)
				{
					offset = levelT - levelO;
					levelT -= levelO;
					levelO = 0f;
				}
				//if offset == 0, then there is no difference between the water levels.
				if (offset != 0)
				{
					//offset the world space positions by the relative door position
					//if negative, add, else sub (is this correct?)
					if (levelO >= 0f)
					{
						//levelO -= origin.RoomFluidPlaneLevels[oI];
					}
					else
					{
						//levelO += origin.RoomFluidPlaneLevels[oI];
					}
					if (levelT >= 0f)
					{
						//levelT -= target.RoomFluidPlaneLevels[tI];
					}
					else
					{
						//levelT += target.RoomFluidPlaneLevels[tI];
					}
					//Debug.Log(levelO + " & " + levelT);

					//get volume of water over door levels
					float freeVolO = 0f;
					float freeVolT = 0f;
					//float originArea = (origin.GetVolume() / origin.RoomHeight);
					//float targetArea = (target.GetVolume() / target.RoomHeight);
					if (levelO > 0f) //origin.RoomFluidPlaneLevels[oI]
					{
						//get the average area of the room's floor and multiply it by the water above the limit
						freeVolO = Math.Abs(origin.RoomArea * levelO);
					}
					if (levelT > 0f)//= target.RoomFluidPlaneLevels[tI]
					{
						//get the average area of the room's floor and multiply it by the water above the limit
						freeVolT = Math.Abs(target.RoomArea * levelT);
					}
					//add the two free volumes. This is how much fluid can be split between the two.
					float eqVol = freeVolO + freeVolT;
					//Debug.Log(eqVol + " = " + freeVolO + " + " + freeVolT);
					//divide by totalarea to get total eq height
					//Debug.Log(eqVol +" / "+ origin.RoomArea + " + " + target.RoomArea);
					float eqHeight = eqVol / (origin.RoomArea + target.RoomArea);
					//Debug.Log(eqHeight + " = " + eqVol + " / (" + origin.RoomArea + " + " + target.RoomArea + ")");
					float Oheight = 0f;
					float Theight = 0f;
					if (eqHeight > origin.RoomHeight)
					{
						Oheight = eqHeight - origin.RoomHeight;
					}
					if (eqHeight > target.RoomHeight)
					{
						Theight = eqHeight - target.RoomHeight;
					}
					//Debug.Log(eqHeight + " -| " + Oheight + " |- " + Theight);
					//multiply eqheight with volume and remove the volume of the ceiling overflow
					//this is a ratio of the fluid in each room to the total fluid
					float Otrans = ((eqHeight * origin.RoomArea) - (Oheight * origin.RoomArea)) / eqVol;
					float Ttrans = ((eqHeight * target.RoomArea) - (Theight * target.RoomArea)) / eqVol;
					//Debug.Log(Otrans + " -||- " + Ttrans);
					//combine the fluids
					List<IFluid> totalFluids = new List<IFluid>();
					totalFluids.AddRange(origin.RemoveRoomFluid(eqVol));
					totalFluids.AddRange(target.RemoveRoomFluid(eqVol));
					//Debug.Log(totalFluids.Count);
					if (totalFluids.Count > 0)
					{
						//divy the fluid volume to each room.
						for (int f = 0; f < totalFluids.Count; f++)
						{
							IFluid oAdd = new IFluid(totalFluids[f]);
							oAdd.SetConcentration(totalFluids[f].GetConcentration() * Otrans);
							IFluid tAdd = new IFluid(totalFluids[f]);
							tAdd.SetConcentration(totalFluids[f].GetConcentration() * Ttrans);
							origin.AddRoomFluid(oAdd);
							target.AddRoomFluid(tAdd);
						}
						totalFluids.Clear();
					}
				}
			}
		}

		/// <summary>
		/// There will be 2-way reversible fluid pumps to/from auxilliary reservoirs to drain flooded rooms.
		/// The alternate use of this function is to vent fluids into space.
		/// </summary>
		/// <param name="origin"></param>
		/// <param name="rate"></param> m^3 removed per second
		public static void LocalFluidDrain(VolumeAtmosphereController origin, float rate, IFluidPipe pipe)
        {
			if(rate == -1f)
            {
				//vent into space
				origin.RemoveRoomFluid(6f);
            }
            else
            {
				//remove into some other pipe
				List<IFluid> fluids = origin.RemoveRoomFluid(rate);
				if (fluids != null)
				{
					pipe.Receive(false, 10f, 2f, fluids, fluids[0].GetTemp());
				}
			}
        }

		public static float RefinementMassLoss(int tier, int quality)
		{
			float[,] lossList = {
				{0f, 35f, 40f, 43.75f, 46.25f, 49f },
				{0f, 28f, 32f, 35f, 37f, 39.2f },
				{0f, 21f, 24f, 26.25f, 27.75f, 29.4f },
				{0f, 14f, 16f, 17.5f, 18.5f, 19.6f },
				{0f, 7f, 8f, 8.75f, 9.25f, 9.8f },
				{0f, 3.5f, 4f, 4.375f, 4.625f, 4.9f },
				{0f, 1.75f, 2f, 2.1875f, 2.3125f, 2.45f },
				{0f, 0.7f, 0.8f, 0.875f, 0.925f, 1.0f}
			};

			return (lossList[quality,tier]/100);
		}

		public static float OreToIngotBaseLoss(int quality)
		{
			float[] baseloss =
			{
				25f,22.5f,20f,20f,20f,15f,15f,10f
			};
			return (baseloss[quality]/100);
		}

		public static string[] AlloyRecipeMatrix(int a)
		{
			string[,] alloyRIng =
			{
				{"Ingot_Iron","Material_Carbon"},
				{"Ingot_Steel","Ingot_Chromium"},
				{"Ingot_Copper","Ingot_Zinc"},
				{"Ingot_Iron","Ingot_Nickel"}
			};

			string[] rtn = { alloyRIng[a, 0], alloyRIng[a, 1] };
			return rtn;
		}

		/// <summary>
		/// Calculate with reasonable accuracy the amount of radiation a person is being exposed to. Units: R/Hr.
		/// </summary>
		/// <param name="activity_TBq"></param>
		/// <param name="distance"></param>
		/// <returns></returns>
		public static float RadiationExposureRate(float activity_TBq, float distance)
        {
			//Debug.Log(activity_TBq+" at "+(distance/100f));
			//U-235 exposure constant: 36756.76 R*cm^2/TBq*hr (1.36 R*cm^2/mCi*hr)
			// Exposure Rate = fluence factor (5.263x10^-7) * A (Bq) * TABULATED_SUM_MeV / distance(cm)^2
			//return (0.0000005263f * ((activity_TBq * Mathf.Pow(10, 12f))) * .1576555265f) / (distance * distance);
			return ((activity_TBq * 36756.76f) / (distance * distance));
        }

		/// <summary>
		/// Calculate distance to an exposure rate of X based on actitivty of Y Bqs.
		/// </summary>
		/// <param name="activity_TBq"></param>
		/// <param name="exposureRate"></param>
		/// <returns></returns>
		public static float MaxRadiationExposureRange(float activity_TBq, float exposureRate)
        {
			//float rng = ((float)Math.Sqrt(0.0000005263f * ((activity_TBq * Mathf.Pow(10, 12f))) * .1576555265f) /(exposureRate)) / 100f;
			float rng = ((float)Math.Sqrt((activity_TBq * 36756.76) / (exposureRate))) / 100f;
			//Debug.Log("range: "+rng);
			return rng;

		}

		/// <summary>
		/// Return the dose absorbed by the player when behind a shield of X cm. Units: rads/Hr
		/// </summary>
		/// <returns></returns>
		public static float RadiationDoseRate(float doseRateAir, float shield_cm, float attenuation)
        {
			// Ib*e^-u*shield
			float dose = doseRateAir * (Mathf.Pow((float)Math.E, (-1f * attenuation * shield_cm)));
			//Debug.Log(dose);
			return dose;
        }

		///
		/// Determine the construction cost of volume sections
		/// Base cost for each section
		/// Additional cost for each 'level'
		///
		public static List<(IComponentDefinition,int)> ComputeVolumeSectionCost(SectionType sType, int stage)
        {
			IComponentLibrary.ComponentDictionary.TryGetValue("Component_SteelComponents", out IComponentDefinition steelCompsDef);
			IComponentLibrary.ComponentDictionary.TryGetValue("Component_IronComponents", out IComponentDefinition ironCompsDef);
			IComponentLibrary.ComponentDictionary.TryGetValue("Component_AluminumComponents", out IComponentDefinition alumCompsDef);
			IComponentLibrary.ComponentDictionary.TryGetValue("Component_TinComponents", out IComponentDefinition tinCompsDef);
			IComponentLibrary.ComponentDictionary.TryGetValue("Component_NickelComponents", out IComponentDefinition nickCompsDef);
			IComponentLibrary.ComponentDictionary.TryGetValue("Component_CopperComponents", out IComponentDefinition copperCompsDef);
			//List<(IComponentDefinition, float)> cost = new List<(IComponentDefinition, float)>();
			List<(IComponentDefinition, int)> baseCost = new List<(IComponentDefinition, int)>();
			//Cost to build to stage 0
			if(stage == 0)
            {
				switch (sType)
				{
					case SectionType.Ceiling:
						baseCost.Add((steelCompsDef, 1));//4
						break;
					case SectionType.Floor:
						baseCost.Add((steelCompsDef, 1));//4
						break;
					case SectionType.Wall:
						baseCost.Add((steelCompsDef, 1));//2
						break;
					case SectionType.Door:
						baseCost.Add((alumCompsDef, 2));
						break;
					case SectionType.Breakers:
						baseCost.Add((alumCompsDef, 1));
						break;
					case SectionType.Lights:
						baseCost.Add((alumCompsDef, 1));
						break;
					case SectionType.Ducts:
						baseCost.Add((alumCompsDef, 1));
						break;
					case SectionType.Pipes:
						baseCost.Add((ironCompsDef, 1));
						break;
					case SectionType.Overhead:
						baseCost.Add((ironCompsDef, 1));
						break;
				}
			}
			//cost to build from stage0 to stage1
			else if(stage == 1)
            {
				switch (sType)
				{
					case SectionType.Ceiling:
						baseCost.Add((steelCompsDef, 1));//4
						break;
					case SectionType.Floor:
						baseCost.Add((steelCompsDef, 1));//4
						break;
					case SectionType.Wall:
						baseCost.Add((steelCompsDef, 1));//2
						break;
				}
			}
			// complete cost (build cost for sub/machines)
			else if (stage == 2)
			{
				switch (sType)
				{
					case SectionType.Ceiling:
						baseCost.Add((steelCompsDef, 1));//4
						break;
					case SectionType.Floor:
						baseCost.Add((steelCompsDef, 1));//4
						break;
					case SectionType.Wall:
						baseCost.Add((steelCompsDef, 1));//2
						break;
					case SectionType.Pipes:
						baseCost.Add((steelCompsDef, 1));//2
						//baseCost.Add((nickCompsDef, 1));
						break;
					case SectionType.Ducts:
						baseCost.Add((tinCompsDef, 1));//3
						break;
					case SectionType.Overhead:
						baseCost.Add((tinCompsDef, 1));//2
						break;
				}
			}
			

			return baseCost;
        }

		// raycast along local axes and determine the directions of the obstructing objects
		public static void RayCastCheckSide(Transform transform, Vector3 offset, float distance,
			out bool forward,out bool back, out bool left, out bool right, bool forOnly)
		{
			// set all bools to false unless an object is detected
			forward = false;
			back = false;
			left = false;
			right = false;

			Physics.queriesHitTriggers = false;
			//forward
			Debug.DrawRay(offset, (transform.forward * distance), Color.red, .1f);
			if (Physics.Raycast(offset, transform.forward, out RaycastHit hitf, distance))
			{
				if (!hitf.transform.CompareTag("Player"))
				{
					forward = true;
				}
			}
			if (!forOnly)
			{
				//back
				if (Physics.Raycast(offset, -transform.forward, out RaycastHit hitb, distance))
				{
					if (!hitb.transform.CompareTag("Player"))
					{
						back = true;
					}
				}

				//left
				if (Physics.Raycast(offset, transform.right, out RaycastHit hitr, distance))
				{
					if (!hitr.transform.CompareTag("Player"))
					{
						right = true;
					}
				}

				//right
				if (Physics.Raycast(offset, -transform.right, out RaycastHit hitl, distance))
				{
					if (!hitl.transform.CompareTag("Player"))
					{
						left = true;
					}
				}
			}
		}

		/// <summary>
		/// Use raycasts to test for ledges and cavities. Returns the minimum distance.
		/// </summary>
		/// <returns></returns>
		public static float GetNearestEdge(Vector3 start, Vector3 end, Vector3 direction, int density, float distance)
        {
			//float startval = start.y;
			float running = 0f;
			Vector3 origin = start;
			// determine the number and distance of rays to be attempted
			//Debug.Log(end.y+" - "+ start.y);
			float spacing = (end.y - start.y)/density;
			float runs = 0;
			
			while(runs <= density)
			{
				runs++;
				//Debug.DrawRay(origin, direction * distance, Color.red,20f);
				if (Physics.Raycast(origin, direction, distance))
				{
					running += spacing;
					origin.y += spacing;
                }
                else
                {
					break;
                }
            }

			// return the height
			return running;
        }
        
        public static float BreakerBoxB_DegToDial(float pwr)
        {
			return -.0075f * (float)Math.Pow(pwr, 3f) + .343f * (float)Math.Pow(pwr, 2f) + .277f * pwr - 49.3f;

		}

        /// <summary>
		/// Calculated the flow rate of a liquid through a pipe of x diameter at y velocity.
		/// Returns m^3/Hr.
		/// </summary>
		/// <param name="diamInner_m"></param>
		/// <param name="flowVelocity_ms"></param>
		/// <returns></returns>
		public static float CalculateFluidFlowThroughPipe(float diamInner_m, float flowVelocity_ms)
        {
            return (3600f * (float)Math.PI) * (float)Math.Pow(diamInner_m/2f,2) * flowVelocity_ms;
        }

		private static Dictionary<float, float> pressureToSpecificVolumeTableA = new Dictionary<float, float>()
        {
			{0f,1.6735f },{1f,0.8804f },{2f,0.6034f },{3f,0.4610f },{4f,0.3739f },{5f,0.3150f },{6f,0.2723f },{7f,0.2400f },
			{8f,0.2146f },{9f,0.1941f },{10f,0.1773f },{11f,0.1631f },{12f,0.1510f },{13f,0.1407f },{14f,0.1316f },
			{15f,0.1236f },{16f,0.1166f },{17f,0.1103f },{18f,0.1046f },{19f,0.0995f },{20f,0.0949f },{21f,0.0907f },
			{22f,0.0868f },{23f,0.0832f },{24f,0.0799f },{25f,0.07687f },{26f,7.4038e-02f},{27f,7.1404e-02f},{28f,6.8944e-02f},
			{ 29f,6.6643e-02f},{ 30f,6.4485e-02f},{ 31f,6.2456e-02f},{ 32f,6.0547e-02f},{ 33f,5.8745e-02f},{ 34f,5.7043e-02f},
			{ 35f,5.5432e-02f},{ 36f,5.3905e-02f},{ 37f,5.2456e-02f},{ 38f,5.1077e-02f},{ 39f,4.9766e-02f},{ 40f,4.8516e-02f},
			{ 41f,4.7323e-02f},{ 42f,4.6184e-02f},{ 43f,4.5094e-02f},{ 44f,4.4051e-02f},{ 45f,4.3052e-02f},{ 46f,4.2094e-02f},
			{ 47f,4.1174e-02f},{ 48f,4.0290e-02f},{ 49f,3.9440e-02f},{ 50f,3.8622e-02f},{ 51f,3.7835e-02f},{ 52f,3.7076e-02f},
			{ 53f,3.6344e-02f},{ 54f,3.5637e-02f},{ 55f,3.4955e-02f},{ 56f,3.4296e-02f},{ 57f,3.3658e-02f},{ 58f,3.3042e-02f},
			{ 59f,3.3042e-02f},{ 60f,3.1867e-02f},{ 61f,3.1306e-02f},{ 62f,3.0763e-02f},{ 63f,3.0236e-02f},{ 64f,2.9725e-02f},
			{ 65f,2.9228e-02f},{ 66f,2.8745e-02f},{ 67f,2.8277e-02f},{ 68f,2.7821e-02f},{ 69f,2.7377e-02f},{ 70f,2.6946e-02f},
			{ 71f,2.6525e-02f},{ 72f,2.6116e-02f},{ 73f,2.5718e-02f},{ 74f,2.5329e-02f},{ 75f,2.4951e-02f},{ 76f,2.4581e-02f},
			{ 77f,2.4221e-02f},{ 78f,2.3869e-02f},{ 79f,2.3526e-02f},{ 80f,2.3191e-02f},{ 81f,2.2863e-02f},{ 82f,2.2543e-02f},
			{ 83f,2.2230e-02f},{ 84f,2.1924e-02f},{ 85f,2.1625e-02f},{ 86f,2.1333e-02f},{ 87f,2.1046e-02f},{ 88f,2.0766e-02f},
			{ 89f,2.0492e-02f},{ 90f,2.0223e-02f},{ 91f,1.9960e-02f},{ 92f,1.9702e-02f},{ 93f,1.9449e-02f},{ 94f,1.9202e-02f},
			{ 95f,1.8959e-02f},{ 96f,1.8721e-02f},{ 97f,1.8487e-02f},{ 98f,1.8258e-02f},{ 99f,1.8033e-02f},{100f,1.7812e-02f},
			{101f,1.7595e-02f},{102f,1.7382e-02f},{103f,1.7173e-02f},{104f,1.6968e-02f},{105f,1.6766e-02f},{106f,1.6568e-02f},
			{107f,1.6373e-02f},{108f,1.6182e-02f},{109f,1.5993e-02f},{110f,1.5808e-02f},{111f,1.5626e-02f},{112f,1.5447e-02f},
			{113f,1.5270e-02f},{114f,1.5097e-02f},{115f,1.4926e-02f},{116f,1.4758e-02f},{117f,1.4592e-02f},{118f,1.4429e-02f},
			{119f,1.4269e-02f},{120f,1.4110e-02f},{121f,1.3955e-02f},{122f,1.3801e-02f},{123f,1.3650e-02f},{124f,1.3500e-02f},
			{125f,1.3353e-02f},{126f,1.3208e-02f},{127f,1.3065e-02f},{128f,1.2924e-02f},{129f,1.2785e-02f},{130f,1.2648e-02f},
			{131f,1.2512e-02f},{132f,1.2378e-02f},{133f,1.2246e-02f},{134f,1.2116e-02f},{135f,1.1987e-02f},{136f,1.1860e-02f},
			{137f,1.1735e-02f},{138f,1.1611e-02f},{139f,1.1489e-02f},{140f,1.1368e-02f},{141f,1.1248e-02f},{142f,1.1130e-02f},
			{143f,1.1013e-02f},{144f,1.0898e-02f},{145f,1.0784e-02f},{146f,1.0671e-02f},{147f,1.0559e-02f},{148f,1.0559e-02f},
			{149f,1.0340e-02f},{150f,1.0232e-02f},{151f,1.0125e-02f},{152f,1.0019e-02f},{153f,9.9146e-03f},{154f,9.8110e-03f},
			{155f,9.7084e-03f},{156f,9.6068e-03f},{157f,9.5063e-03f},{158f,9.4066e-03f},{159f,9.3080e-03f},{160f,9.2102e-03f},
			{161f,9.1134e-03f},{162f,9.0175e-03f},{163f,8.9224e-03f},{164f,8.8281e-03f},{165f,8.7347e-03f},{166f,8.6421e-03f},
			{167f,8.5502e-03f},{168f,8.4592e-03f},{169f,8.3689e-03f},{170f,8.2793e-03f},{171f,8.1905e-03f},{172f,8.1023e-03f},
			{173f,8.0149e-03f},{174f,7.9281e-03f},{175f,7.8420e-03f},{176f,7.7565e-03f},{177f,7.6716e-03f},{178f,7.5874e-03f},
			{179f,7.5037e-03f},{180f,7.4207e-03f},{181f,7.3382e-03f},{182f,7.2562e-03f},{183f,7.1748e-03f},{184f,7.0940e-03f},
			{185f,7.0136e-03f},{186f,6.9338e-03f},{187f,6.8545e-03f},{188f,6.7756e-03f},{189f,6.6972e-03f},{190f,6.6193e-03f},
			{191f,6.5418e-03f},{192f,6.4648e-03f},{193f,6.3881e-03f},{194f,6.3119e-03f},{195f,6.2361e-03f},{196f,6.1607e-03f},
			{197f,6.0856e-03f},{198f,6.0109e-03f},{199f,5.9366e-03f},{200f,5.8626e-03f},{201f,5.7890e-03f},{202f,5.7156e-03f},
			{203f,5.6426e-03f},{204f,5.5698e-03f},{205f,5.4974e-03f},{206f,5.4251e-03f},{207f,5.3532e-03f},{208f,5.2815e-03f},
			{209f,5.2099e-03f},{210f,5.1386e-03f},{211f,5.0674e-03f},{212f,4.9964e-03f},{213f,4.9254e-03f},{214f,4.8546e-03f},
			{215f,4.7837e-03f},{216f,4.7128e-03f},{217f,4.6417e-03f},{218f,4.5704e-03f},{219f,4.4987e-03f}
		};

		public static float CalculateGasFlowThroughPipe(float diamInner_m, float flowVelocity_ms, float flowPressure_bar)
        {
			//This may not be able to find the pressure if it's not 1 whole number
			float sv = PressureToSpecificVolume(flowPressure_bar);
			return (3600f * (float)Math.PI) * (float)Math.Pow(diamInner_m / 2f, 2) * (flowVelocity_ms/sv);
        }

		private static float PressureToSpecificVolume(float flowPressure_bar)
        {
			//find the closest value
			int rounded = (int)Mathf.RoundToInt(flowPressure_bar);
			//we need the next closest value (forwards or backwards)
			float dif = flowPressure_bar - rounded;
			int valueB = rounded;
			if(dif < 0f)
            {
				valueB = rounded - 1;
			}
			else if(dif > 0f)
            {
				valueB = rounded + 1;
			}
			//get the two tabulated values that bookend the flowPressure_bar
			pressureToSpecificVolumeTableA.TryGetValue(rounded, out float roundedSV);
			pressureToSpecificVolumeTableA.TryGetValue(valueB, out float valueBSV);
			//linearly interpolate the aproximate sv value of flowPressure_bar
			return Mathf.Lerp(roundedSV, valueBSV, dif);
        }

		public static float StrengthVersusTemperature_Titanium(float tempK)
        {
			//lose 1MPa for every degree K
			return 950f - (tempK);//- 273f
		}
	}
}