using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUniverse.Data.Libraries.Definitions
{
	/// <summary>
	/// Type class from which all materials will obtain their base parameters and such and such
	/// </summary>
	public class MaterialDefinition
	{
		string ore_Type;
		string resourcePath;
		string inclusions;//STANDARD
						  //Dictionary<MaterialDefinition, float> MaterialInclusions;

		public MaterialDefinition(string type, string prefPath, string incType)
		{
			ore_Type = type;
			resourcePath = prefPath;
			inclusions = incType;//NYI
		}

		public string GetMaterialType()
		{
			return ore_Type;
		}

	}
}