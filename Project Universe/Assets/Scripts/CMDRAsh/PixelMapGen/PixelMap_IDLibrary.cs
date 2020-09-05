using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.IO;
using UnityEngine;

public static class PixelMap_IDLibrary
{
	//red 0-254 is tile id
	public static Dictionary<int, Dictionary<int, string>> PIXID_TILELIB;//once PIX_DICT has the xmls read, equate the instance dict with this static dict.
	public static Dictionary<string, string> PIXID_PATHPAIRS;

	/// <summary>
	/// Hardcoded dictionary used for testing purposes.
	/// </summary>
	public static Dictionary<int, Dictionary<int, string>> PIXID_LIBRARY = new Dictionary<int, Dictionary<int, string>>
	{
		{0, new Dictionary<int,string>{{1,"Wall_PanelA" } } },//DevWallStraight
		{50, new Dictionary<int,string>{{1, "Wall_CornerA" } } },//DevWallCorner
		{75, new Dictionary<int,string>{{1,"DevWallFull" } } },
		{100, new Dictionary<int,string>{{1, "Wall_InvertedCornerA" } } },//DevWallCornerInverted
		{125, new Dictionary<int,string>{{1, "ShipFloorA" } } },//DevFloor0
		{150, new Dictionary<int, string>{{1, "ShipCeilingA" } } },
		{160, new Dictionary<int,string>{{1,"1x1_DoorA"},{2,"1x2_DoorA"} } },
		//temp \/
		{175, new Dictionary<int,string>{{1, "EnergyCapacitor_Large" } } },
		{200, new Dictionary<int,string>{{1, "Router_Lvl1" } } },
		{225, new Dictionary<int,string>{{1, "Router_SubstationBox Variant" } } },
		{245, new Dictionary<int,string>{{1, "DummyBreakerBox" } } },

		{254, new Dictionary<int,string>{{255, "DevFloor0" } } },//magenta (IGNORE - No prefab yet)
		{255, new Dictionary<int,string>{{1, "IGNORE" } } },//white
	};

	public class PIX_DICT
	{
		//will ensure this only runs once (at Awake()).
		public Boolean isInitialized = false;
		//pixidlib
		public Dictionary<int, Dictionary<int, string>> PIXID_TileLibrary;
		//tile path pairs
		public Dictionary<string, string> PIXID_PathLibrary;
		public PIX_DICT()
		{
			PIXID_TileLibrary = new Dictionary<int, Dictionary<int, string>>();
			PIXID_PathLibrary = new Dictionary<string, string>();
		}

		/// <summary>
		/// Call this method to initialize the PIXID RGB tile ID dictionary from the XML masters.
		/// </summary>
		public void InitializeTileDictionary()
		{
			//prevent any other Awake methods from starting the tile dictionary initialization process.
			isInitialized = true;
			//find the xmls
			string xmlPath = "\\Assets\\Resources\\Data\\";
			string root = Directory.GetCurrentDirectory();
			string[] filesInDir = Directory.GetFiles(root + xmlPath, "*.xml", SearchOption.AllDirectories);
			string[] pathParts;
			//XML data
			int typeValue;
			int subType;
			string name;
			string rssPath;
			List<string> componentList = new List<string>();

			foreach (string fileAndPath in filesInDir)
			{
				//open xmls
				pathParts = fileAndPath.Split('\\');
				XDocument doc = XDocument.Load(root + xmlPath + pathParts[pathParts.Length - 1]);
				//basic file check
				Debug.Log("Checking XML Format...");
				if (doc.Root.Name != "Definitions")
				{
					Debug.Log("This is not a definition file or is incorrectly formatted!");
				}
				else
				{
					Debug.Log("Format Check Complete");
				}
				//grab tilegroups
				Debug.Log("Attempting parse...");
				foreach (XElement file in doc.Descendants("Definitions"))
				{	
					foreach (XElement groups in file.Elements("TileGroup"))
					{
						//grab attribute TypeValue by accessing the 'ID' element, and it's attribute, 'TypeValue'
						typeValue = int.Parse(groups.Element("ID").Attribute("TypeValue").Value);
						Debug.Log("Tile Type Identified; ID:" + typeValue);
						foreach (XElement tileDefs in groups.Elements("Definition"))
						{
							//get the tile subtype
							subType = int.Parse(tileDefs.Element("SubType").Value);
							Debug.Log("Tile Subtype Identified; SubID:" + subType);
							//get the tile name
							name = tileDefs.Element("Name").Value;
							Debug.Log("Tile Name: " + name);
							//get the resource path
							rssPath = tileDefs.Element("ResourcePath").Attribute("Path").Value;
							Debug.Log("At Path: " + rssPath);
							foreach (XElement component in tileDefs.Elements("Components"))
							{
								componentList.Add(component.Element("Component").Attribute("Name").Value);
							}
							
							//declare a dictionary variable.
							Dictionary<int, string> innerDict;
							//try to return the inner dictionary associated with the typevalue
							//or add the key if it doesn't exist.
							if (!PIXID_TileLibrary.TryGetValue(typeValue, out innerDict)){
								//declare the dictionary as empty.
								innerDict = new Dictionary<int, string>();
								PIXID_TileLibrary.Add(typeValue, innerDict);
							}
							//if the key existed, innerdict is still just a dictionary.
							//Thus we can add the new keyvaluepair without issue.
							innerDict.Add(subType, name);
							//add the name and path to PIXID_PathLibrary (sooo much easier)
							PIXID_PathLibrary.Add(name, rssPath);
							//clear the list for the next tile
							componentList.Clear();
						}
						
					}
				}
			}
			Debug.Log("Library Construction Finished");
			//when the last xml has been red, assign the pathlib and tilelib to the master class statics
			//this way the static dictionaries function as a globally-accessible reference.
			PixelMap_IDLibrary.PIXID_TILELIB = PIXID_TileLibrary;
			PixelMap_IDLibrary.PIXID_PATHPAIRS = PIXID_PathLibrary;
			
			/*
			//print the dict out
			Debug.Log("Keys:");
			foreach (int key in PIXID_TileLibrary.Keys)
			{
				Debug.Log(key);
			}
			foreach (var values in PIXID_TileLibrary.Values)
			{
				Debug.Log("Inner dict:");
				//print out the dicts
				foreach(var keys in values.Keys)
				{
					Debug.Log("Key:");
					Debug.Log(keys);
				}
				foreach (var innerValues in values.Values)
				{
					Debug.Log("InnerValue:");
					Debug.Log(innerValues);
				}
			}
			Debug.Log(PIXID_TileLibrary.Count);
			*/
		}

	}
}
