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

namespace ProjectUniverse.Util
{
    public static class Utils
	{
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
			float amount = amountPerSecond * Time.deltaTime;
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
			// Exposure Rate = fluence factor (5.263x10^-7) * A (Bq) * TABULATED_SUM_MeV / distance(cm)^2
			return (0.0000005263f * ((activity_TBq * Mathf.Pow(10, 12f))) * .1576555265f) / (distance * distance);
        }

		/// <summary>
		/// Calculate distance to an exposure rate of X based on actitivty of Y Bqs.
		/// </summary>
		/// <param name="activity_TBq"></param>
		/// <param name="exposureRate"></param>
		/// <returns></returns>
		public static float MaxRadiationExposureRange(float activity_TBq, float exposureRate)
        {
			float rng = ((float)Math.Sqrt(0.0000005263f * ((activity_TBq * Mathf.Pow(10, 12f))) * .1576555265f) /(exposureRate)) / 100f;
			Debug.Log("range: "+rng);
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

	}
}