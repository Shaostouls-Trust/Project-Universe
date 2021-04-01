using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Type class from which all ores will obtain their base parameters and such and such
/// </summary>
public class OreDefinition
{
	private string ore_Type;
	private string resourcePath;
	private string productionID;//what this ore is smelted into
	//ore inclusions ex: Ore_Nickel,0.05 (5%)
	private string oreIncType;//FROM_ZONE or RARE
							  //Dictionary<OreDefinition, float> OreInclusions;
							  //Dictionary<MaterialDefinition, float> MaterialInclusions;
	private int processingTime_Sec;

	public OreDefinition(string type, string prefPath, string prodID, string incType)
	{
		ore_Type = type;
		resourcePath = prefPath;
		productionID = prodID;
		oreIncType = incType;//NYI
	}

	public string GetOreType()
	{
		return ore_Type;
	}
	public string GetResourcePath()
	{
		return resourcePath;
	}
	public string GetProductionID()
	{
		return productionID;
	}
	public string GetInclusionSet()
	{
		return oreIncType;
	}
	public int GetProcessingTimePerUnit()
	{
		return 1;
	}
}
