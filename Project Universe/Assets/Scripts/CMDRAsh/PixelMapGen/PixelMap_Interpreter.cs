using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ProjectUniverse.Generation
{
	public class PixelMap_Interpreter : MonoBehaviour
	{
		public string bitmapName;
		Bitmap levelMap;
		public string PixelMapRootPath;
		//public string PrefabsRootPath;
		private bool generated;
		private bool tilesGenerated;
		private float y3D;
		private float x3D; //rows
		private float z3D; //columns

		private ArrayList tileArrayList = new ArrayList();
		private int tileIttCount = 0;
		//run config
		public bool ignoreR254;
		public bool triscalar;
		public bool multipleLevels;
		private int ceiling;
		public int staticCeiling;
		private List<int> tilelist6m = new List<int> { 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65 };
		private List<int> tilelist5m = new List<int> { };
		/*tile placement with respect to world axis.
		* Z is North/South
		* X is East/West
		* Reads +X, -Z, so invert Z (subtract)
		* PixelMap X is E/W
		*/
		//Red - Tile ID
		//Green - rot / 3 (*3 for rotation)
		//blue - subset (IE door_A, door_B, ceilingwall_A, ceilingwall_B)

		void Awake()
		{
			PixelMap_IDLibrary.PIX_DICT internalDictConstructor = new PixelMap_IDLibrary.PIX_DICT();
			if (!internalDictConstructor.GetInitialization())
			{
				internalDictConstructor.InitializeTileDictionary();
			}
		}

		public void MultiLevelIntegration(string baseMapName, Transform gameobjectTransform, bool myIgnoreR254, bool triscalar)
		{
			//search directory for the base map name
			string root = Directory.GetCurrentDirectory();
			string[] filesInDir;
			string[] pathParts;
			string[] nameAndNumber;
			List<string> maps = new List<string>(1);
			int count = 1;
			string myName;
			//string filePath = "";
			filesInDir = Directory.GetFiles(root + GetPixelMapRootPath(), "*" + ".png", SearchOption.AllDirectories);
			//search for name
			foreach (string fileAndPath in filesInDir)
			{
				//split off the name from the path
				pathParts = fileAndPath.Split('\\');
				//Split name and the level number
				//Debug.Log(pathParts[pathParts.Length - 1]);
				nameAndNumber = pathParts[pathParts.Length - 1].Split('_');
				//strip extension
				string[] temp = nameAndNumber[nameAndNumber.Length - 1].Split('.');
				nameAndNumber[nameAndNumber.Length - 1] = temp[0];
				if (nameAndNumber.Length > 1)
				{
					//Idea here is that any map that is not part of a multi-level structure will fail the conversion of the last argument 
					//(which should always be an integer) because non-level maps will end on a non-parsable string
					try
					{
						int.Parse(nameAndNumber[nameAndNumber.Length - 1]);
					}
					catch (Exception e)
					{
						continue;
					}
					//extract base name
					string[] temp2 = new string[nameAndNumber.Length - 1];
					for (int j = 0; j < nameAndNumber.Length - 1; j++)
					{
						temp2[j] = nameAndNumber[j];
					}
					string checkName = String.Join("_", temp2);
					//count = nameAndNumber[pathParts.Length - 1];
					//nameAndNumber.SetValue("", nameAndNumber.Length - 1);
					myName = String.Join("_", nameAndNumber);
					//check name
					Debug.Log("map: " + myName + "; " + checkName);
					if (checkName == baseMapName)
					{
						maps.Add(myName);
					}
				}
			}
			//Debug.Log(maps);
			foreach (string level in maps)
			{
				BitMapInterpreter(level, gameobjectTransform, myIgnoreR254, triscalar, count++);
			}
		}

		public void BitMapInterpreter(string mapName, Transform gameobjectTransform, bool myIgnoreR254, bool triscalar, int levelNumber)
		{


			//Load the parameter bitmap
			string path = SolvePath(mapName, ".png", GetPixelMapRootPath());// "\\Assets\\Resources\\Maps\\PixelMaps");
			try
			{
				//open image
				levelMap = new Bitmap(path);

			}
			catch (System.Exception e)
			{
				Debug.Log(e);
				//load an error image
			}
			//create a rectangle using the bounds of the image
			Rectangle rect = new Rectangle(0, 0, levelMap.Width, levelMap.Height);
			//lock bits into memory
			BitmapData levelMapData = levelMap.LockBits(rect, ImageLockMode.ReadOnly, levelMap.PixelFormat);
			//get number of bytes by dividing size. So 24bits is 3 bytes per pixel.
			int bytesPerPixel = Bitmap.GetPixelFormatSize(levelMap.PixelFormat) / 8;
			int byteCount = levelMapData.Stride * levelMap.Height;
			byte[] pixels = new byte[byteCount];
			//find memory address of the bitmap
			System.IntPtr ptrFirstPixel = levelMapData.Scan0;
			//copy the pixels from memory to the array we created above
			Marshal.Copy(ptrFirstPixel, pixels, 0, pixels.Length);
			//we have the map data, so unlock the bits and close the image
			levelMap.UnlockBits(levelMapData);
			levelMap.Dispose();
			int heightInPixels = levelMapData.Height;
			int widthInBytes = levelMapData.Width * bytesPerPixel;
			//declare the variables we need inside the loop
			string prefabID;
			string fullpath;
			float rotation;
			float levelMapWidth = levelMapData.Width;
			if (triscalar)
			{
				levelMapWidth *= 3;
			}
			//establish the 3D (y) axis
			y3D = gameobjectTransform.position.y + (levelNumber * 3);//for now assume each level is 3m
			x3D = 0.0f;
			z3D = 0.0f;
			float xOffset = 0.5f;
			float zOffset = -0.5f;
			Debug.Log("Attempting to build level...");
			//itterate through the bitmap data
			for (int y = 0; y < heightInPixels; y++)
			{
				int currentLine = y * levelMapData.Stride;
				for (int x = 0; x < widthInBytes; x += bytesPerPixel)
				{
					int blue = pixels[currentLine + x];
					int green = pixels[currentLine + x + 1];
					int red = pixels[currentLine + x + 2];
					//image processing
					if (red != 255)
					{
						//TODO clean this empty statement up
						if (red == 254 && myIgnoreR254) { }
						else
						{
							//check the R value against the dictionary of tile monochannel IDs
							if (PixelMap_IDLibrary.PIXID_TILELIB.TryGetValue(red, out Dictionary<int, string> returnedDict))
							{
								//access returned dict for subtype (blue)
								//If there are no subtypes blue == 0. This is out of consideration to map-making, where B=1 is incredibly hard (impossible) to see,
								//so the less it is done, the less likely there are to be mistakes in the map.
								if (blue == 0)
								{
									returnedDict.TryGetValue(1, out prefabID);
								}
								else
								{   //get the subtype
									returnedDict.TryGetValue(blue, out prefabID);
								}
								//get the path from the static PATHPAIRS dictionary
								try
								{
									PixelMap_IDLibrary.PIXID_PATHPAIRS.TryGetValue(prefabID, out fullpath);
								}
								catch (System.Exception e)
								{
									Debug.Log(e);
									Debug.Log(red + " " + blue + " " + green);
									PixelMap_IDLibrary.PIXID_TILELIB.TryGetValue(254, out returnedDict);
									returnedDict.TryGetValue(255, out prefabID);
									PixelMap_IDLibrary.PIXID_PATHPAIRS.TryGetValue(prefabID, out fullpath);
								}

								//pull rotation from G and multiply by 3
								rotation = green * 3;

								if (red == 245)
								{
									///check X and Z component of name for the size-based offsets
									string[] XYdims = prefabID.Split('_')[0].Split('x');
									//Debug.Log(XYdims[0]+" "+XYdims[1]);
									int x0 = int.Parse(XYdims[0]);
									int y0 = int.Parse(XYdims[1]);
									switch (x0)
									{
										case 3:
											xOffset = 0.5f;
											break;
										case 5:
											xOffset = 1.0f;
											break;
										case 6:
											if (rotation == 0)
											{
												xOffset = 1.5f;
											}
											else if (rotation == 90)
											{
												xOffset = 2.5f;
											}
											else if (rotation == 180)
											{
												xOffset = 1.5f;
											}
											else if (rotation == 270)
											{
												xOffset = 2.5f;
											}
											//xOffset = 1.5f;
											break;
										case 9:
											xOffset = 0.5f;
											break;
									}
									switch (y0)
									{
										case 3:
											zOffset = -0.5f;
											break;
										case 5:
											zOffset = 0.0f;
											break;
										case 6:
											zOffset = 0.0f;
											break;
										case 9:
											zOffset = -0.5f;
											break;
									}
									//if (tilelist6m.Contains(blue))
									//{
									//	xOffset = 1.5f;
									//	zOffset = 0.0f;
									//}
									ArrayList tempArray = new ArrayList() { fullpath,
							new Vector3(gameobjectTransform.position.x + x3D + xOffset, y3D, gameobjectTransform.position.z + z3D + zOffset),// - 0.5f
							rotation };
									tileArrayList.Add(tempArray);
								}
								else
								{
									xOffset = 0.5f;
									zOffset = -0.5f;
									ArrayList tempArray = new ArrayList() { fullpath,
							new Vector3(gameobjectTransform.position.x + x3D + 0.5f, y3D, gameobjectTransform.position.z + z3D - 0.5f ),
							rotation };
									tileArrayList.Add(tempArray);
								}
							}
						}
					}
					//update positional parameters
					if (triscalar)
					{
						x3D += 3;
					}
					else
					{
						x3D++;
					}
					//if our x position has hit the bounds of the pixelmap
					if (x3D > levelMapWidth - 1)//imageWidth is 50 (0-49m)
					{
						//next pixel column, which is the next worldspace z row
						if (triscalar)
						{
							z3D -= 3;
						}
						else
						{
							z3D--;
						}
						x3D = 0.0f;
					}
				}
			}

		}

		void Update()
		{
			if (tileArrayList.Count > 0 && !tilesGenerated)
			{
				//Debug.Log("...");
				if (tileIttCount + staticCeiling > tileArrayList.Count)
				{
					ceiling += tileArrayList.Count - tileIttCount;
				}
				else
				{
					ceiling += staticCeiling;
				}
				for (; tileIttCount < ceiling; tileIttCount++)
				{
					//catch prefabbed rooms
					//Might not be necessary?
					ArrayList unpacked = (ArrayList)tileArrayList[tileIttCount];
					string fullpath = (string)unpacked[0];
					Vector3 position = (Vector3)unpacked[1];
					float rotation = (float)unpacked[2];
					GameObject prefab = Resources.Load(fullpath) as GameObject;
					GameObject instanceObject = Instantiate(prefab, new Vector3(position.x, position.y, position.z), new Quaternion(0.0f, 0.0f, 0.0f, 1.0f), transform);
					//set the objects rotation (not using a quaternion for sake of euler precision)
					instanceObject.transform.localRotation = Quaternion.Euler(0.0f, rotation, 0.0f);
					instanceObject.name = prefab.name + " " + tileIttCount;
					//if it's a floor
					//if(red == 125)
					//{
					//place a ceiling over it
					//	prefab = Resources.Load("Prefabs\\CMDRAsh\\ShipCeilingA") as GameObject;
					//	GameObject instanceObjectRoof = Instantiate(prefab, new Vector3(transform.position.x + x3D + 0.5f, transform.position.y + 3, transform.position.z + z3D - 0.5f), new Quaternion(0.0f, 0.0f, 0.0f, 1.0f), transform);
					//	instanceObjectRoof.transform.localRotation = Quaternion.Euler(0.0f,0.0f,180.0f);
					//}
				}
				if (tileIttCount >= tileArrayList.Count - 1)
				{
					tilesGenerated = true;
					Debug.Log("Generated");
				}
			}
		}

		public void SetPixelMapRootPath(string pixelMapRoot)
		{
			PixelMapRootPath = pixelMapRoot;
		}
		public string GetPixelMapRootPath()
		{
			return PixelMapRootPath;
		}
		public ArrayList GetTileArrayList()
		{
			return tileArrayList;
		}

		//attempt to path from the basedirectory to the file described by the 'name' and 'extension' params
		public string SolvePath(string name, string extension, string baseDirectory)
		{
			string root = Directory.GetCurrentDirectory();
			string[] filesInDir;
			string[] pathParts;
			string myPath;
			string filePath = "";
			filesInDir = Directory.GetFiles(root + baseDirectory, "*" + extension, SearchOption.AllDirectories);
			//search for name
			foreach (string fileAndPath in filesInDir)
			{
				//split off the name of the tile and compare to var 'name'
				pathParts = fileAndPath.Split('\\');
				//split at _ and recombine all but the last segment

				//check name
				if (pathParts[pathParts.Length - 1] == name + extension)
				{
					pathParts.SetValue("", pathParts.Length - 1);
					myPath = Path.Combine(pathParts);
					filePath = fileAndPath;
					break;
				}
			}
			return filePath;
		}

		public void ButtonResponse()
		{
			if (!generated)
			{
				generated = true;
				tilesGenerated = false;
				if (multipleLevels)
				{
					MultiLevelIntegration(bitmapName, transform, ignoreR254, triscalar);
				}
				else
				{
					//begin PIXI process
					BitMapInterpreter(bitmapName, transform, ignoreR254, triscalar, 1);
				}

			}
		}
	}
}