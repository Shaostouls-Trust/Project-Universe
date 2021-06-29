using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.IO;
using UnityEngine;
using ProjectUniverse.Data.Libraries.Definitions;

namespace ProjectUniverse.Data.Libraries
{
	public class IngotLibrary
	{
		public static Dictionary<string, IngotDefinition> IngotDictionary;
		private static Boolean isInitialized;

		public class IngotDefinitionLibrary
		{
			public Dictionary<string, IngotDefinition> IL_IngotDictionary;

			public IngotDefinitionLibrary()
			{
				IL_IngotDictionary = new Dictionary<string, IngotDefinition>();
			}

			public void InitializeIngotDictionary()
			{
				Debug.Log("Ingot Library Construction Initiated");
				string ingotType;
				string ingotRSPath;
				float ingotDensity;

				//will ensure this only runs once (at Awake()).
				if (!isInitialized)
				{
					isInitialized = true;
					//find the xmls
					string xmlPath = "\\Assets\\Resources\\Data\\Production\\MasterLibraries\\";
					string root = Directory.GetCurrentDirectory();
					string[] filesInDir = Directory.GetFiles(root + xmlPath, "*.xml", SearchOption.TopDirectoryOnly);

					foreach (string fileAndPath in filesInDir)
					{
						string[] fpss = fileAndPath.Split('\\');
						if (fpss[fpss.Length - 1] == "IngotMasterList.xml")
						{
							Debug.Log("Ingot Master Found");
							XDocument doc = XDocument.Load(fileAndPath);
							foreach (XElement ingotDefs in doc.Descendants("IngotDefinitions"))
							{
								foreach (XElement ingot in ingotDefs.Elements("Ingot"))
								{
									ingotType = ingot.Element("STR_ID").Attribute("Ingot_Type").Value;
									ingotRSPath = ingot.Element("ResourcePath").Attribute("Path").Value;
									ingotDensity = float.Parse(ingot.Element("Density").Attribute("Density").Value);
									IngotDefinition newingotDef = new IngotDefinition(ingotType, ingotRSPath, ingotDensity);
									IL_IngotDictionary.Add(ingotType, newingotDef);
								}
							}
						}
					}
					Debug.Log("Ingot Library Construction Finished");
					IngotDictionary = IL_IngotDictionary;
				}
			}
		}
	}
}