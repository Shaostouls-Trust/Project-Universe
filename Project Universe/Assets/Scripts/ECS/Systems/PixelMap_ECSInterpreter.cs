using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using ProjectUniverse.Generation;

/// <summary>
/// This class is an attepted rewrite of the pixelmap interpreter using ECS
/// Most of it is coppied from the original class.
/// </summary>
public class PixelMap_ECSInterpreter : MonoBehaviour//, IConvertGameObjectToEntity
{
	public string bitmapName;
	Bitmap levelMap;
	private bool generated;
	public string PixelMapRootPath;
	//private float y3D;
	private float x3D; //rows
	private float z3D; //columns
	//public bool prefabInstantiationComplete;
	private NativeList<Entity> shipEntityArray = new NativeList<Entity>(Allocator.Temp);

	//void Awake and Library Initialization handled in original class

	//public GameObject prefabGameObject;

	/*
	public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
	{
		using (BlobAssetStore blobAssetStore = new BlobAssetStore())
		{
			Entity prefabEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefabGameObject,
				GameObjectConversionSettings.FromWorld(dstManager.World, blobAssetStore));
			//store the prefab entity or instantiate

		}
	}
	*/
	//public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
	//{
		//add prefab to list of prefabs to be converted
		//IDeclareReferencedPrefabs
	//	referencedPrefabs.Add(null);
	//}

	public void BitMapECSInterpreter()
	{
		string path = SolvePath(bitmapName, ".png", PixelMapRootPath);// \\Assets\\Resources\\Maps\\PixelMaps
		try
		{
			levelMap = null;//new Bitmap(path);

		}
		catch (System.Exception e)
		{
			Debug.Log(e);
		}

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
		int heightInPixels = levelMapData.Height;
		int widthInBytes = levelMapData.Width * bytesPerPixel;
		string prefabID;
		Dictionary<int, string> returnedDict;
		string fullpath;
		float rotation;
		int count = 0;//count that increments by one per tile.
		GameObject prefab;
		//y3D = transform.localPosition.y;
		x3D = 0.0f;
		z3D = 0.0f;
		Debug.Log("Attempting to build ECS level...");
		//itterate through the bitmap data
		Debug.Log("Ready for Universe-Ending Headaches?");

		

		for (int y = 0; y < heightInPixels; y++)
		{
			int currentLine = y * levelMapData.Stride;
			for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
			{
				int blue = pixels[currentLine + x];
				int green = pixels[currentLine + x + 1];
				int red = pixels[currentLine + x + 2];
				//image processing
				if (red != 255)//|| red != 254)
				{
					if (PixelMap_IDLibrary.PIXID_TILELIB.TryGetValue(red, out returnedDict))
					{
						if (blue == 0)
						{
							returnedDict.TryGetValue(1, out prefabID);
						}
						else
						{
							returnedDict.TryGetValue(blue, out prefabID);
						}
						PixelMap_IDLibrary.PIXID_PATHPAIRS.TryGetValue(prefabID, out fullpath);
						rotation = green * 3;
						prefab = Resources.Load(fullpath) as GameObject;
						EntityManager myEntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
						//GameObject instanceObject = Instantiate(prefab, new Vector3(transform.position.x + x3D + 0.5f, transform.position.y, transform.position.z + z3D - 0.5f), new Quaternion(0.0f, 0.0f, 0.0f, 1.0f), transform);
						//Entity tileEntity = myEntityManager.Instantiate(prefab); 
						//myEntityManager.SetName(tileEntity, prefabID + " " + count++);
						//set the rotation of the object. No idea if the other components need set or not.
						//myEntityManager.SetComponentData(tileEntity,
						//	new Translation { Value = Quaternion.Euler(0.0f, rotation, 0.0f).eulerAngles });
					
					//instanceObject.transform.localRotation = Quaternion.Euler(0.0f, rotation, 0.0f);
					//
					//NativeArray<Entity> shipArray = new NativeArray<Entity>(0, Allocator.Temp);

					//Entity entity = myEntityManager.CreateEntity();
					//create 100 tiles of archetype 'basictile'. After this we would set the individual component values.
					//myEntityManager.CreateEntity(basicTile, shipArray);
					//Convert the prefab to an Entity
				}
				}
				//update positional parameters
				x3D++;
				//if our x position has hit the bounds of the pixelmap
				if (x3D > levelMapData.Width - 1)//imageWidth is 50 (0-49m)
				{
					//next pixel column, which is the next worldspace z row
					z3D--;
					x3D = 0.0f;
				}
			}
		}
		//unlock bits
		levelMap.UnlockBits(levelMapData);
		//unload image

		//prefabInstantiationComplete = true;
	}

	public NativeArray<Entity> getEntityArray()
	{
		return shipEntityArray;
	}

	public string SolvePath(string name, string extension, string baseDirectory)
	{
		string root = Directory.GetCurrentDirectory();
		string[] filesInDir;
		string[] pathParts;
		string myPath = "";
		string filePath = "";
		filesInDir = Directory.GetFiles(root + baseDirectory, "*" + extension, SearchOption.AllDirectories);
		foreach (string fileAndPath in filesInDir)
		{
			pathParts = fileAndPath.Split('\\');
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
			//begin PIXI process
			BitMapECSInterpreter();
		}
	}
}
