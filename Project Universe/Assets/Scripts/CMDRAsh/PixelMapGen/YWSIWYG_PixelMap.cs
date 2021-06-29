using System.Collections;
using System.Collections.Generic;
using System.Drawing;
//using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using ProjectUniverse.PowerSystem;

namespace ProjectUniverse.Generation
{
	[ExecuteInEditMode]
	public class YWSIWYG_PixelMap : MonoBehaviour
	{
		public string bitmapName;

		public bool instantiateLibrary = true;
		public bool updateTileArray;
		public bool run;
		public string pixelMapRootPath;
		private int tileIttCount = 0;
		//private int ceiling = 10;
		private ArrayList tileList = new ArrayList();
		//run config
		public bool ignoreR254;
		public bool multipleLevels;
		public bool triscalar;
		public bool RUNAFTER_updatePowerPointers;
		private int ceiling;
		public int staticCeiling;

		void Update()
		{
			if (instantiateLibrary)
			{
				//staticCeiling = ceiling;
				instantiateLibrary = false;
				PixelMap_IDLibrary.PIX_DICT internalDictConstructor = new PixelMap_IDLibrary.PIX_DICT();
				internalDictConstructor.InitializeTileDictionary();
			}
			if (updateTileArray)
			{

				ceiling = staticCeiling;
				updateTileArray = false;
				//read in the pixelmap specified (via the BitMapInterpreter)
				PixelMap_Interpreter pixMapInt = new PixelMap_Interpreter();
				pixMapInt.SetPixelMapRootPath(pixelMapRootPath);
				if (multipleLevels)
				{
					pixMapInt.MultiLevelIntegration(bitmapName, transform, ignoreR254, triscalar);
				}
				else
				{
					pixMapInt.BitMapInterpreter(bitmapName, transform, ignoreR254, triscalar, 1);
				}
				tileList = pixMapInt.GetTileArrayList();
				Debug.Log(tileList.Count);
				//reset the ship
				tileIttCount = 0;
				//ceiling = staticCeiling;
				while (transform.childCount > 0)
				{
					foreach (Transform child in transform)
					{
						GameObject.DestroyImmediate(child.gameObject);
					}
				}
			}
			if (run)
			{
				//Declare cutsom handling event for arraylist
				//we need a way to determine what needs updated, and what doesn't
				if (tileList.Count > 0)
				{
					if (tileIttCount + staticCeiling > tileList.Count)
					{
						ceiling += tileList.Count - tileIttCount;
					}
					else
					{
						ceiling += staticCeiling;
					}
					for (; tileIttCount < ceiling; tileIttCount++)
					{
						ArrayList unpacked = (ArrayList)tileList[tileIttCount];
						string fullpath = (string)unpacked[0];
						Vector3 position = (Vector3)unpacked[1];
						float rotation = (float)unpacked[2];
						GameObject prefab = Resources.Load(fullpath) as GameObject;
						GameObject instanceObject = Instantiate(prefab, new Vector3(position.x, position.y, position.z), new Quaternion(0.0f, 0.0f, 0.0f, 1.0f), transform);
						//set the objects rotation (not using a quaternion for sake of euler precision)
						instanceObject.transform.localRotation = Quaternion.Euler(0.0f, rotation, 0.0f);
						instanceObject.name = prefab.name + " " + tileIttCount;
					}
				}
			}
			if (RUNAFTER_updatePowerPointers)
			{
				RUNAFTER_updatePowerPointers = false;
				ILinkOnCreation linker = new ILinkOnCreation(gameObject);
			}
		}
	}
}