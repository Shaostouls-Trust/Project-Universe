using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectUniverse.Base;
using System.Reflection;
using System;
using ProjectUniverse.Environment.Gas;
using ProjectUniverse.Environment.Volumes;

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

	}
}