using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectUniverse.Data.Libraries.Definitions;

namespace ProjectUniverse.Data.Libraries.Definitions
{
	/*
		NOTE: for ingots, Quantity of: 0.5 is small ingot, 1 is normal, and 1.5 is large
	 */

	/// <summary>
	/// Definition defines what path to go to for the model(s), the cost of building, how many levels the machine has,
	/// cost of upgrading, and the machine's str ID.
	/// </summary>
	public class MachineDefinition
	{
		private string machineID;
		private string RssPath;
		private int baseLevel;
		private List<(IngotDefinition, float)> IngotRecipe = new List<(IngotDefinition, float)>();
		private List<(MaterialDefinition, float)> MaterialRecipe = new List<(MaterialDefinition, float)>();
		private List<(IComponentDefinition, int)> CompRecipe = new List<(IComponentDefinition, int)>();
		private List<int> Levels = new List<int>();
		private List<(IngotDefinition, float)> IngotUpgradeCost = new List<(IngotDefinition, float)>();
		private List<(MaterialDefinition, float)> MaterialUpgradeCost = new List<(MaterialDefinition, float)>();
		private List<(IComponentDefinition, float)> CompUpgradeCost = new List<(IComponentDefinition, float)>();

		public MachineDefinition(string machinename, string resourcePath, int baseLvl)
		{
			machineID = machinename;
			RssPath = resourcePath;
			baseLevel = baseLvl;
		}

		public void AddToRecipe(MaterialDefinition mat, float a)
		{
			MaterialRecipe.Add((mat, a));
		}
		public void AddToRecipe(IngotDefinition ing, float a)
		{
			IngotRecipe.Add((ing, a));
		}
		public void AddToRecipe(IComponentDefinition comp, int a)
		{
			CompRecipe.Add((comp, a));
		}
		public void AddToUpgrade(MaterialDefinition mat, float a)
		{
			MaterialUpgradeCost.Add((mat, a));
		}
		public void AddToUpgrade(IngotDefinition ing, float a)
		{
			IngotUpgradeCost.Add((ing, a));
		}
		public void AddToUpgrade(IComponentDefinition comp, float a)
		{
			CompUpgradeCost.Add((comp, a));
		}
		public void AddLevel(int levelNum)
		{
			Levels.Add(levelNum);
		}

		public List<(IComponentDefinition, int)> GetComponentRecipe()
		{
			return CompRecipe;
		}
		public List<(IComponentDefinition, float)> GetComponentUpgradeCost()
		{
			return CompUpgradeCost;
		}
		public List<int> GetLevels()
		{
			return Levels;
		}
	}
}