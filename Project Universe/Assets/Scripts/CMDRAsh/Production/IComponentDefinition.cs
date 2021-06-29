using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectUniverse.Data.Libraries.Definitions;

namespace ProjectUniverse.Data.Libraries.Definitions
{
	/*
	   <CompData Component_Type="Component_RAM" Quantity = "1"/>
			<Factory/>DevFactory<Factory/>
			<ResourcePath Path="Prefabs\CMDRAsh\" />
			<Recipe>
				<Part Material="Material_Carbon" Quantity = "1">
				<Part Ingot="Ingot_Iron" Quantity = "1">
				<Part Component="Component_" Quantity = "1">
			</Recipe>
	 */
	public class IComponentDefinition
	{
		private string IComponent_Type;
		private int ProductionQuantity;
		private string RequiredFactory;
		private float BuildTime;
		private string ResourcePath;
		private int Health;
		private int Priority;
		private List<(IngotDefinition, float)> IngotRecipe = new List<(IngotDefinition, float)>();
		private List<(MaterialDefinition, float)> MaterialRecipe = new List<(MaterialDefinition, float)>();
		private List<(IComponentDefinition, int)> CompRecipe = new List<(IComponentDefinition, int)>();

		public IComponentDefinition(string strID, int prodQuant, string fact, string rssp, int health, int priority)
		{
			IComponent_Type = strID;
			ProductionQuantity = prodQuant;
			RequiredFactory = fact;
			ResourcePath = rssp;
			BuildTime = 0f;
			Health = health;
			Priority = priority;
			if (Priority <= 0)
			{
				Debug.LogError(this + " has invalid priority");
			}
		}

		public string GetComponentType()
		{
			return IComponent_Type;
		}

		public void AddToRecipe(MaterialDefinition mat, float a)
		{
			MaterialRecipe.Add((mat, a));
		}
		public void AddToRecipe(IngotDefinition mat, int a)
		{
			IngotRecipe.Add((mat, a));
		}
		public void AddToRecipe(IComponentDefinition mat, int a)
		{
			CompRecipe.Add((mat, a));
		}

		public void CalculateBuildTime()
		{
			BuildTime = 0f;
			for (int i = 0; i < IngotRecipe.Count; i++)
			{
				BuildTime++;
			}
			for (int m = 0; m < MaterialRecipe.Count; m++)
			{
				BuildTime++;
			}
			for (int c = 0; c < CompRecipe.Count; c++)
			{
				BuildTime++;
			}
		}
		public float GetBuildTime()
		{
			return BuildTime;
		}

		public List<(IComponentDefinition, int)> GetComponentRecipeList()
		{
			return CompRecipe;
		}
		public List<(IngotDefinition, float)> GetIngotRecipeList()
		{
			return IngotRecipe;
		}
		public List<(MaterialDefinition, float)> GetMaterialRecipeList()
		{
			return MaterialRecipe;
		}

		public int GetHealth()
		{
			return Health;
		}
		public int GetPriority()
		{
			return Priority;
		}
	}
}