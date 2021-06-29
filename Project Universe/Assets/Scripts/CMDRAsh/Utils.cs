using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectUniverse.Base;
using System.Reflection;
using System;

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
		public static void InsertionSort<T>(ref List<ItemStack> list, MethodInfo sortBy) where T:IComparable//List<ItemStack>
		{
			for (int outer = 1; outer < list.Count; outer++)
			{
				int position = outer;
				//get the value to compare to position-1 (this value is the type we are comparing, IE an int/float/string/etc)
				ItemStack tempStack = new ItemStack(list[position]);
				var KeyZero = (T)sortBy.Invoke(list[position].GetItemArray().GetValue(0), null);	
				while(position > 0 && KeyZero.CompareTo((T)sortBy.Invoke(list[position - 1].GetItemArray().GetValue(0), null)) <0)//keyZero < KeyNext
				{
					list[position] = list[position - 1];
					position--;
				}
				list[position] = tempStack;
			}
		}
    }
}