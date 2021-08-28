using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.IO;
using UnityEngine;
using ProjectUniverse.Data.Libraries.Definitions;
using ProjectUniverse.Data.Libraries;

namespace ProjectUniverse.Data.Libraries
{
	public class MachineLibrary : MonoBehaviour
	{
		public static Dictionary<string, MachineDefinition> MachineDictionary;
		private static Boolean isInitialized;

		public class MachineDefinitionLibrary
		{
			public Dictionary<string, MachineDefinition> MDL_MachineDictionary;
			//private List<string> returnTo = new List<string>();

			public MachineDefinitionLibrary()
			{
				MDL_MachineDictionary = new Dictionary<string, MachineDefinition>();
			}

			public void InitializeMachineDictionary()
			{
				Debug.Log("Machine Library Construction Initiated");
				string machineType;
				int baseLevel;
				string RssPath;
				int processed = 0;
				string recMat;
				float recQ;
				MaterialDefinition matDef;
				IngotDefinition ingDef;
				IComponentDefinition compDef;
				if (!isInitialized)
				{
					isInitialized = true;

					TextAsset _rawText = Resources.Load<TextAsset>("Data/Production/MasterLibraries/MachineMasterList");
					XDocument xmlDoc = XDocument.Parse(_rawText.text, LoadOptions.PreserveWhitespace);
					Debug.Log("Machine Master Found");
					foreach (XElement machDefs in xmlDoc.Descendants("MachineDefinitions"))
					{
						foreach (XElement mach in machDefs.Elements("Machine"))
						{
							machineType = mach.Element("MachData").Attribute("Machine_Type").Value;
							RssPath = mach.Element("ResourcePath").Attribute("Path").Value;
							baseLevel = int.Parse(mach.Element("BaseLevel").Attribute("Level").Value);
							MachineDefinition newMachDef = new MachineDefinition(machineType, RssPath, baseLevel);
							MDL_MachineDictionary.Add(machineType, newMachDef);
							//get build costs
							foreach (XElement recipe in mach.Descendants("Recipe"))
							{
								foreach (XElement part in recipe.Elements("Part"))
								{
											
									if (part.Attribute("Component") != null)
									{
										recMat = part.Attribute("Component").Value;
										recQ = float.Parse(part.Attribute("Quantity").Value);
										if (IComponentLibrary.ComponentDictionary.TryGetValue(recMat, out compDef))
										{
											//Debug.Log("AddToMachineRecipe");
											newMachDef.AddToRecipe(compDef, (int)recQ);
										}
										else
										{
											Debug.Log("Recipe Requested Non-Existant Component (" + recMat + ")");
											//returnTo.Add(recMat);
										}
									}
											
									if (part.Attribute("Ingot") != null)
									{
										recMat = part.Attribute("Ingot").Value;
										recQ = float.Parse(part.Attribute("Quantity").Value);
										if (IngotLibrary.IngotDictionary.TryGetValue(recMat, out ingDef))
										{
											newMachDef.AddToRecipe(ingDef, recQ);
										}
										else
										{
											Debug.Log("Recipe Requested Non-Existant Ingot (" + recMat + ")");
											//returnTo.Add(newCompDef);
										}
									}
											
									if (part.Attribute("Material") != null)
									{
										recMat = part.Attribute("Material").Value;
										recQ = float.Parse(part.Attribute("Quantity").Value);
										if (OreLibrary.MaterialDictionary.TryGetValue(recMat, out matDef))
										{
											newMachDef.AddToRecipe(matDef, recQ);
										}
										else
										{
											Debug.Log("Recipe Requested Non-Existant Material (" + recMat + ")");
											//returnTo.Add(newCompDef);
										}
									}
								}

								foreach (XElement upg in mach.Descendants("Upgrades"))
								{
									//get upgrade costs
									foreach (XElement cost in mach.Descendants("Cost"))
									{
										foreach (XElement part in cost.Elements("Part"))
										{
											if (part.Attribute("Component") != null)
											{
												recMat = part.Attribute("Component").Value;
												recQ = float.Parse(part.Attribute("Quantity").Value);
												if (IComponentLibrary.ComponentDictionary.TryGetValue(recMat, out compDef))
												{
													newMachDef.AddToUpgrade(compDef, recQ);
												}
												else
												{
													Debug.Log("Upgrade Requested Non-Existant Component");
													//returnTo.Add(recMat);
												}
											}
											if (part.Attribute("Ingot") != null)
											{
												recMat = part.Attribute("Ingot").Value;
												recQ = float.Parse(part.Attribute("Quantity").Value);
												if (IngotLibrary.IngotDictionary.TryGetValue(recMat, out ingDef))
												{
													newMachDef.AddToUpgrade(ingDef, recQ);
												}
												else
												{
													Debug.Log("Upgrade Requested Non-Existant Ingot");
													//returnTo.Add(newCompDef);
												}
											}
											if (part.Attribute("Material") != null)
											{
												recMat = part.Attribute("Material").Value;
												recQ = float.Parse(part.Attribute("Quantity").Value);
												if (OreLibrary.MaterialDictionary.TryGetValue(recMat, out matDef))
												{
													newMachDef.AddToUpgrade(matDef, recQ);
												}
												else
												{
													Debug.Log("Upgrade Requested Non-Existant Material");
													//returnTo.Add(newCompDef);
												}
											}
										}
									}
									//Get levels and add to levels temp
									try
									{
										newMachDef.AddLevel(int.Parse(upg.Element("Level").Attribute("Level").Value));//null ref
									}
									catch (NullReferenceException e) { }
								}
							}
							processed++;
						}
					}
					Debug.Log("Processed Machines: " + processed);
					Debug.Log("Machine Library Construction Finished");
					MachineDictionary = MDL_MachineDictionary;
					Resources.UnloadUnusedAssets();
				}
			}
		}
	}
}