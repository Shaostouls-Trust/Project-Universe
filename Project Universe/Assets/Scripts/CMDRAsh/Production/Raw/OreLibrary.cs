using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.IO;
using UnityEngine;
using ProjectUniverse.Data.Libraries.Definitions;

namespace ProjectUniverse.Data.Libraries
{
	public static class OreLibrary
	{
		public static Dictionary<string, OreDefinition> OreDictionary;
		public static Dictionary<string, MaterialDefinition> MaterialDictionary;
		private static Boolean isOreInitialized;
		private static Boolean isMaterialInitialized;

		public class OreMaterialLibrary
		{
			public Dictionary<string, OreDefinition> OL_OreDictionary;
			public Dictionary<string, MaterialDefinition> OL_MaterialDictionary;
			public OreMaterialLibrary()
			{
				OL_OreDictionary = new Dictionary<string, OreDefinition>();
				OL_MaterialDictionary = new Dictionary<string, MaterialDefinition>();
			}

			public void InitializeOreDictionary()
			{
				Debug.Log("Ore Library Construction Initiated");
				string oreType;
				string oreRSPath;
				string oreIngot;
				string inclusionProfile;

				if (!isOreInitialized)
				{
					isOreInitialized = true;
					TextAsset _rawText = Resources.Load<TextAsset>("Data/Production/MasterLibraries/OreMasterList");
					XDocument xmlDoc = XDocument.Parse(_rawText.text, LoadOptions.PreserveWhitespace);
					Debug.Log("Ore Master Found");
					//XDocument doc = XDocument.Load(fileAndPath);
					foreach (XElement oreDefs in xmlDoc.Descendants("OreDefinitions"))
					{
						foreach (XElement ore in oreDefs.Elements("Ore"))
						{
							oreType = ore.Element("STR_ID").Attribute("Ore_Type").Value;
							//Debug.Log("Ore LibType: " + oreType);
							oreRSPath = ore.Element("ResourcePath").Attribute("Path").Value;
							oreIngot = ore.Element("ProductionID").Attribute("Ingot").Value;
							inclusionProfile = ore.Element("Inclusions").Attribute("Inclusion").Value;
							OreDefinition newOreDef = new OreDefinition(oreType, oreRSPath, oreIngot, inclusionProfile);
							//Add ore def to the OL dictionary
							OL_OreDictionary.Add(oreType, newOreDef);
						}
					}
					Debug.Log("Ore Library Construction Finished");
					OreDictionary = OL_OreDictionary;
					Resources.UnloadUnusedAssets();
				}
			}

			public void InitializeMaterialDictionary()
			{
				Debug.Log("Material Library Construction Initiated");
				string materialType;
				string materialRSPath;
				string inclusionProfile;
				//prevent any other Awake methods from starting the ore dictionary initialization process.
				isMaterialInitialized = true;
				if (isMaterialInitialized)
				{
					TextAsset _rawText = Resources.Load<TextAsset>("Data/Production/MasterLibraries/MaterialMasterList");
					XDocument xmlDoc = XDocument.Parse(_rawText.text, LoadOptions.PreserveWhitespace);
					Debug.Log("Material Master Found");
					foreach (XElement matDefs in xmlDoc.Descendants("MaterialDefinitions"))
					{
						//Debug.Log("MatDef");
						foreach (XElement mat in matDefs.Elements("Material"))
						{
							materialType = mat.Element("STR_ID").Attribute("Material_Type").Value;
							//Debug.Log("Material LibType: " + materialType);
							materialRSPath = mat.Element("ResourcePath").Attribute("Path").Value;
							inclusionProfile = mat.Element("Inclusions").Attribute("Inclusion").Value;
							MaterialDefinition newMatDef = new MaterialDefinition(materialType, materialRSPath, inclusionProfile);
							OL_MaterialDictionary.Add(materialType, newMatDef);
						}
					}
					Debug.Log("Material Library Construction Finished");
					MaterialDictionary = OL_MaterialDictionary;
					Resources.UnloadUnusedAssets();
				}
			}
		}
	}
}