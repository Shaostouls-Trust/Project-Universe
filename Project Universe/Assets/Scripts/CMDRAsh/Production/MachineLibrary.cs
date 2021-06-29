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
						if (fpss[fpss.Length - 1] == "MachineMasterList.xml")
						{
							Debug.Log("Machine Master Found");
							XDocument doc = XDocument.Load(fileAndPath);
							foreach (XElement machDefs in doc.Descendants("MachineDefinitions"))
							{
								//Debug.Log("MachineDef Descendants:");
								foreach (XElement mach in machDefs.Elements("Machine"))
								{
									//Debug.Log("Machine " + (processed+1));
									machineType = mach.Element("MachData").Attribute("Machine_Type").Value;
									RssPath = mach.Element("ResourcePath").Attribute("Path").Value;
									//The location of this Element in the XML v in code shouldn't matter, right?
									baseLevel = int.Parse(mach.Element("BaseLevel").Attribute("Level").Value);
									MachineDefinition newMachDef = new MachineDefinition(machineType, RssPath, baseLevel);
									MDL_MachineDictionary.Add(machineType, newMachDef);
									//Debug.Log("Machine Created");
									//get build costs
									foreach (XElement recipe in mach.Descendants("Recipe"))
									{
										foreach (XElement part in recipe.Elements("Part"))
										{
											//try {
											if (part.Attribute("Component") != null)//Element("Part").
											{
												recMat = part.Attribute("Component").Value;//Element("Part").
												recQ = float.Parse(part.Attribute("Quantity").Value);//Element("Part").
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
											//}catch(NullReferenceException a) { }
											//try
											//{
											if (part.Attribute("Ingot") != null)//Element("Part").
											{
												recMat = part.Attribute("Ingot").Value;//Element("Part").
												recQ = float.Parse(part.Attribute("Quantity").Value);//.Element("Part")
												if (IngotLibrary.IngotDictionary.TryGetValue(recMat, out ingDef))
												{
													//Debug.Log("AddToMachineRecipe");
													newMachDef.AddToRecipe(ingDef, recQ);
												}
												else
												{
													Debug.Log("Recipe Requested Non-Existant Ingot (" + recMat + ")");
													//returnTo.Add(newCompDef);
												}
											}
											//}
											//catch (NullReferenceException b) { }
											//try
											//{
											if (part.Attribute("Material") != null)//Element("Part").
											{
												recMat = part.Attribute("Material").Value;//Element("Part").
												recQ = float.Parse(part.Attribute("Quantity").Value);//Element("Part").
												if (OreLibrary.MaterialDictionary.TryGetValue(recMat, out matDef))
												{
													//Debug.Log("AddToMachineRecipe");
													newMachDef.AddToRecipe(matDef, recQ);
												}
												else
												{
													Debug.Log("Recipe Requested Non-Existant Material (" + recMat + ")");
													//returnTo.Add(newCompDef);
												}
											}
											//}
											//catch (NullReferenceException c) { }
										}

										foreach (XElement upg in mach.Descendants("Upgrades"))
										{
											//get upgrade costs
											foreach (XElement cost in mach.Descendants("Cost"))
											{
												foreach (XElement part in cost.Elements("Part"))
												{
													//try
													//{
													if (part.Attribute("Component") != null)//Element("Part").
													{
														recMat = part.Attribute("Component").Value;//Element("Part").
														recQ = float.Parse(part.Attribute("Quantity").Value);//Element("Part").
														if (IComponentLibrary.ComponentDictionary.TryGetValue(recMat, out compDef))
														{
															//Debug.Log("AddToMachineCost");
															newMachDef.AddToUpgrade(compDef, recQ);
														}
														else
														{
															Debug.Log("Upgrade Requested Non-Existant Component");
															//returnTo.Add(recMat);
														}
													}
													//}
													//catch (NullReferenceException a) { }
													//try
													//{
													if (part.Attribute("Ingot") != null)//Element("Part").
													{
														recMat = part.Attribute("Ingot").Value;//.Element("Part")
														recQ = float.Parse(part.Attribute("Quantity").Value);//Element("Part").
														if (IngotLibrary.IngotDictionary.TryGetValue(recMat, out ingDef))
														{
															//Debug.Log("AddToMachineCost");
															newMachDef.AddToUpgrade(ingDef, recQ);
														}
														else
														{
															Debug.Log("Upgrade Requested Non-Existant Ingot");
															//returnTo.Add(newCompDef);
														}
													}
													//}
													//catch (NullReferenceException b) { }
													//try
													//{
													if (part.Attribute("Material") != null)//Element("Part").
													{
														recMat = part.Attribute("Material").Value;//Element("Part").
														recQ = float.Parse(part.Attribute("Quantity").Value);//Element("Part").
														if (OreLibrary.MaterialDictionary.TryGetValue(recMat, out matDef))
														{
															//Debug.Log("AddToMachineCost");
															newMachDef.AddToUpgrade(matDef, recQ);
														}
														else
														{
															Debug.Log("Upgrade Requested Non-Existant Material");
															//returnTo.Add(newCompDef);
														}
													}
												}
												//}
												//catch (NullReferenceException c) { }
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
						}
					}
					Debug.Log("Processed Machines: " + processed);
					Debug.Log("Machine Library Construction Finished");
					MachineDictionary = MDL_MachineDictionary;
				}
			}


		}
	}
}