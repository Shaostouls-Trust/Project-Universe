using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Type class from which all ingots will obtain their base parameters and such and such
/// </summary>
public class IngotDefinition
{
	private string ingot_Type;
	private string resourcePath;
	private float metalDensity;
	//The inclusions will already have been calculated, so they need added to this IngotDef.
	//private List<OreDefinition> oreIncs;
	//private List<MaterialDefinition> matIncs;

	public IngotDefinition(string type,string rssPath, float density)
	{
		ingot_Type = type;
		resourcePath = rssPath;
		metalDensity = density;
	}

	public string GetIngotType()
	{
		return ingot_Type;
	}
}
