using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.IO;
using UnityEngine;
using System.Linq;
using ProjectUniverse.Data.Libraries.Definitions;
using ProjectUniverse.Data.Libraries;

namespace ProjectUniverse.Data.Libraries
{
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
				int health;
				int priority;
				string RssPath;
				int processed = 0;
				int regened = 0;
				string recMat;
				int recQ;
				MaterialDefinition matDef;
				IngotDefinition ingDef;
				IComponentDefinition comDef;
				//will ensure this only runs once (at Awake()).
				if (!isInitialized)
				{
					isInitialized = true;
					TextAsset _rawText = Resources.Load<TextAsset>("Data/Production/MasterLibraries/ComponentMasterList");
					XDocument xmlDoc = XDocument.Parse(_rawText.text,LoadOptions.PreserveWhitespace);
					Debug.Log("Component Master Found");
					foreach (XElement compDefs in xmlDoc.Descendants("ComponentDefinitions"))
					{
						foreach (XElement comp in compDefs.Elements("Component"))
						{
							componentType = comp.Element("CompData").Attribute("Component_Type").Value;
							quantity = int.Parse(comp.Element("CompData").Attribute("Quantity").Value);
							factory = comp.Element("Factory").Value;
							health = int.Parse(comp.Element("Health").Attribute("Value").Value);
							priority = int.Parse(comp.Element("Priority").Attribute("Value").Value);
							RssPath = comp.Element("ResourcePath").Attribute("Path").Value;
							IComponentDefinition newCompDef = new IComponentDefinition(componentType, quantity, factory, RssPath, health, priority);
							ICL_ComponentDictionary.Add(componentType, newCompDef);

							foreach (XElement recipe in comp.Descendants("Recipe"))
							{
								foreach (XElement part in recipe.Elements("Part"))
								{
									if (part.Attribute("Material") != null)//nullref 
									{
										recMat = part.Attribute("Material").Value;
										recQ = int.Parse(part.Attribute("Quantity").Value);
										if (OreLibrary.MaterialDictionary.TryGetValue(recMat, out matDef))
										{
											newCompDef.AddToRecipe(matDef, recQ);
										}
										else
										{
											Debug.Log("Recipe Requested Non-Existant Material");
											//returnTo.Add(newCompDef);
										}
									}
									if (part.Attribute("Ingot") != null)
									{
										recMat = part.Attribute("Ingot").Value;
										recQ = int.Parse(part.Attribute("Quantity").Value);
										if (IngotLibrary.IngotDictionary.TryGetValue(recMat, out ingDef))
										{
											newCompDef.AddToRecipe(ingDef, recQ);
										}
										else
										{
											Debug.Log("Recipe Requested Non-Existant Ingot");
											//returnTo.Add(newCompDef);
										}
									}
									if (part.Attribute("Component") != null)
									{
										recMat = part.Attribute("Component").Value;
										recQ = int.Parse(part.Attribute("Quantity").Value);
										if (ICL_ComponentDictionary.TryGetValue(recMat, out comDef))
										{
											newCompDef.AddToRecipe(comDef, recQ);
										}
										else
										{
											//a required component def does not exist yet,
											//so we need to return to this compdef later
											if (!returnTo.Contains(recMat))
											{
												returnTo.Add(recMat);
											}
										}
									}
									newCompDef.CalculateBuildTime();
								}
							}
							processed++;
						}
					}
					//loop through returnTo to compile the recipes
					//Runs a max of two times
					//need a safeguard to not dupe a recipe.
					int runs = 0;
					
					while (returnTo.Count > 0 && runs < 1)
					{
						Debug.Log("ReturnTo count: " + returnTo.Count + "; runs: " + runs);
						runs++;
						foreach (XElement compDefs in xmlDoc.Descendants("ComponentDefinitions"))
						{
							foreach (XElement comp in compDefs.Elements("Component"))
							{
								string type = comp.Element("CompData").Attribute("Component_Type").Value;
								foreach (XElement recipe in comp.Descendants("Recipe"))
								{
									foreach (XElement part in recipe.Elements("Part"))
									{
										if (part.Attribute("Component") != null)
										{
											//Debug.Log(type);
											recMat = part.Attribute("Component").Value;
											if (returnTo.Contains(recMat)) { 
												recQ = int.Parse(part.Attribute("Quantity").Value);
												if (ICL_ComponentDictionary.TryGetValue(recMat, out comDef))
												{
													//check if the component is already in the recipe.
													List<(IComponentDefinition, int)> complist = comDef.GetComponentRecipeList();
													if (!complist.Contains((comDef, recQ)))
													{
														regened++;
														//Debug.Log("Missing " + recMat + "... Added");
														ICL_ComponentDictionary.TryGetValue(type, out IComponentDefinition thisCompDef);
														thisCompDef.AddToRecipe(comDef, recQ);
														//returnTo.Remove(type);
														thisCompDef.CalculateBuildTime();
													}
												}
												else
												{
													Debug.Log("Component not found on rerun!");
												}
											}
										}
									}
								}
							}
						}
					}
				}
				Debug.Log("Regenerated " + regened + " Recipes");
				Debug.Log("Processed Components:" + processed);
				Debug.Log("Component Library Construction Finished");
				ComponentDictionary = ICL_ComponentDictionary;
				Resources.UnloadUnusedAssets();
			}
		}
	}
}