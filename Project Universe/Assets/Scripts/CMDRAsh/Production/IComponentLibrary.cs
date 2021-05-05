using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.IO;
using UnityEngine;
using System.Linq;

public class IComponentLibrary : MonoBehaviour
{
    public static Dictionary<string, IComponentDefinition> ComponentDictionary;
    private static Boolean isInitialized;

	public class ComponentDefinitionLibrary
	{
		public Dictionary<string, IComponentDefinition> ICL_ComponentDictionary;
		private List<string> returnTo;

		public ComponentDefinitionLibrary()
		{
			ICL_ComponentDictionary = new Dictionary<string, IComponentDefinition>();
			returnTo = new List<string>();
		}

		public void InitializeComponentDictionary()
		{
			Debug.Log("Component Library Construction Initiated");
			string componentType;
			int quantity;
			string factory;
			string RssPath;
			int processed = 0;
			string recMat;
			int recQ;
			MaterialDefinition matDef;
			IngotDefinition ingDef;
			IComponentDefinition comDef;
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
					if (fpss[fpss.Length - 1] == "ComponentMasterList.xml")
					{
						Debug.Log("Component Master Found");
						XDocument doc = XDocument.Load(fileAndPath);
						foreach (XElement compDefs in doc.Descendants("ComponentDefinitions"))
						{
							foreach (XElement comp in compDefs.Elements("Component"))
							{
								componentType = comp.Element("CompData").Attribute("Component_Type").Value;
								//Debug.Log(componentType);
								quantity = int.Parse(comp.Element("CompData").Attribute("Quantity").Value);
								factory = comp.Element("Factory").Value;
								RssPath = comp.Element("ResourcePath").Attribute("Path").Value;
								IComponentDefinition newCompDef = new IComponentDefinition(componentType, quantity, factory, RssPath);
								ICL_ComponentDictionary.Add(componentType, newCompDef);

								foreach (XElement recipe in comp.Descendants("Recipe"))
								{
									foreach(XElement part in recipe.Elements("Part"))
									{
										//only looping through this once
										//Debug.Log("------------------LOOP--------------------");
										//try
										//{
										if (part.Attribute("Material") != null)//nullref //Element("Part").
										{
											//Debug.Log("ADDING MATERIAL");
											recMat = part.Attribute("Material").Value;//Element("Part").
											recQ = int.Parse(part.Attribute("Quantity").Value);//Element("Part").
											if (OreLibrary.MaterialDictionary.TryGetValue(recMat, out matDef))
											{
												//Debug.Log("AddToMaterial Price");
												newCompDef.AddToRecipe(matDef, recQ);
											}
											else
											{
												Debug.Log("Recipe Requested Non-Existant Material");
												//returnTo.Add(newCompDef);
											}

										}
										//}
										//catch (NullReferenceException a) { Debug.Log("FAILED"); }
										//try
										//{
										if (part.Attribute("Ingot") != null)//.Element("Part")
											{
											//Debug.Log("ADDING INGOT");
											recMat = part.Attribute("Ingot").Value;//Element("Part").
											recQ = int.Parse(part.Attribute("Quantity").Value);//Element("Part").
											if (IngotLibrary.IngotDictionary.TryGetValue(recMat, out ingDef))
											{
												//Debug.Log("AddToIngot Price");
												newCompDef.AddToRecipe(ingDef, recQ);
											}
											else
											{
												Debug.Log("Recipe Requested Non-Existant Ingot");
												//returnTo.Add(newCompDef);
											}
										}
										//}
										//catch (NullReferenceException b) { Debug.Log("FAILED"); }
										//try
										//{
										if (part.Attribute("Component") != null)//.Element("Part")
											{
											//Debug.Log("ADDING COMPONENT");
											recMat = part.Attribute("Component").Value;//Element("Part").
											recQ = int.Parse(part.Attribute("Quantity").Value);//Element("Part").
											if (ICL_ComponentDictionary.TryGetValue(recMat, out comDef))
											{
												//Debug.Log("Add " + comDef.GetComponentType() + " to " + newCompDef.GetComponentType());
												newCompDef.AddToRecipe(comDef, recQ);
											}
											else
											{
												//Debug.Log("Add To ReturnTo");
												//a required component def does not exist yet,
												//so we need to return to this compdef later
												returnTo.Add(recMat);
											}
										}
										//}
										//catch (NullReferenceException c) { Debug.Log("FAILED"); }
									}
								}
								processed++;
							}
						}
					}
				}
				//loop through returnTo as many times as it takes to compile the recipes
				//Runs a max of two times
				//need a safeguard to not and dupes to recipe.
				int runs = 0;
				while (returnTo.Count > 0 && runs < 2)
				{
					Debug.Log("ReturnTo count: "+returnTo.Count+"; runs: "+runs);
					runs++;
					foreach (string fileAndPath in filesInDir)
					{
						string[] fpss = fileAndPath.Split('\\');
						if (fpss[fpss.Length - 1] == "ComponentMasterList.xml")
						{
							XDocument doc = XDocument.Load(fileAndPath);
							foreach (XElement compDefs in doc.Descendants("ComponentDefinitions"))
							{
								foreach (XElement comp in compDefs.Elements("Component"))
								{
									string type = comp.Element("CompData").Attribute("Component_Type").Value;
									if (returnTo.Contains(type))
									{
										foreach (XElement recipe in comp.Descendants("Recipe"))
										{
											foreach (XElement part in recipe.Elements("Part"))
											{
												//try
												//{
												if (part.Attribute("Component") != null)//Element("Part").
												{
													recMat = part.Attribute("Component").Value;//Element("Part").
													recQ = int.Parse(part.Attribute("Quantity").Value);//Element("Part").
													if (ICL_ComponentDictionary.TryGetValue(recMat, out comDef))
													{
														//check if the component is already in the recipe.
														List<(IComponentDefinition, int)> complist = comDef.GetComponentRecipeList();
														if (!complist.Contains((comDef, recQ)))
														{
															Debug.Log("Missing Component Found");
															ICL_ComponentDictionary.TryGetValue(type, out IComponentDefinition thisCompDef);
															thisCompDef.AddToRecipe(comDef, recQ);
															returnTo.Remove(type);
														}
													}
													else
													{
														Debug.Log("Component not found on rerun!");
													}
												}
											}
											//}
											//catch (NullReferenceException e) { }
										}
									}
								}
							}
						}
					}
				}
				Debug.Log("Processed Components:" + processed);
				Debug.Log("Component Library Construction Finished");
				ComponentDictionary = ICL_ComponentDictionary;
			}
		}
	}
}
