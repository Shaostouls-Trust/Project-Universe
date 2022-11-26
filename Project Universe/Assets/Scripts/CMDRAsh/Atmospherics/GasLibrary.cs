using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.IO;
using UnityEngine;
using ProjectUniverse.Data.Libraries.Definitions;

namespace ProjectUniverse.Data.Libraries
{
    public class GasLibrary : MonoBehaviour
    {
		public static Dictionary<string, GasDefinition> GasDictionary;
		private static Boolean isInitialized;

		public class GasDefinitionLibrary
		{
			public Dictionary<string, GasDefinition> GL_GasDictionary;

			public GasDefinitionLibrary()
			{
				GL_GasDictionary = new Dictionary<string, GasDefinition>();
			}

			public void InitializeGasDictionary()
			{
				Debug.Log("Gas Library Construction Initiated");
				string gasType;
				int flamability;
				int combustability;
                float molarMass;
                bool isNuclear;
				float toxicity;
				float specificHeat;

                //will ensure this only runs once (at Awake()).
                if (!isInitialized)
				{
					isInitialized = true;
					TextAsset _rawText = Resources.Load<TextAsset>("Data/Production/MasterLibraries/GasMasterList");
					XDocument xmlDoc = XDocument.Parse(_rawText.text, LoadOptions.PreserveWhitespace);
					Debug.Log("Gas Master Found");
					foreach (XElement ingotDefs in xmlDoc.Descendants("GasDefinitions"))
					{
						foreach (XElement ingot in ingotDefs.Elements("Gas"))
						{
							gasType = ingot.Element("Gas_Type").Attribute("STR_ID").Value;
                            flamability = int.Parse(ingot.Element("Flamability").Attribute("Value").Value);
                            combustability = int.Parse(ingot.Element("Combustability").Attribute("Value").Value);
							molarMass = float.Parse(ingot.Element("MolarMass").Attribute("Value").Value);
							isNuclear = bool.Parse(ingot.Element("IsNuclear").Attribute("BoolValue").Value);
                            toxicity = float.Parse(ingot.Element("Toxicity").Attribute("Value").Value);
                            specificHeat = float.Parse(ingot.Element("SpecificHeat").Attribute("Value").Value);
                            GasDefinition newingotDef = new GasDefinition(gasType,flamability,
								combustability,molarMass,isNuclear,toxicity,specificHeat);
                            GL_GasDictionary.Add(gasType, newingotDef);
                        }
					}
					Debug.Log("Gas Library Construction Finished");
					GasDictionary = GL_GasDictionary;
					Resources.UnloadUnusedAssets();
				}
			}
		}
	}
}
