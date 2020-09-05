using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

//[ExecuteInEditMode]
public class PixelMap_Interpreter : MonoBehaviour
{
	public string bitmapName;
	Bitmap levelMap;
	public string PixelMapRootPath;
	public string PrefabsRootPath;
	public bool generated;
	//private float y3D;
	private float x3D; //rows
	private float z3D; //columns
	//public PixelMap_IDLibrary library = new PixelMap_IDLibrary();
	//public Dictionary<string, string> tilePathPairs = new Dictionary<string, string>();
	/*tile placement with respect to world axis.
	* Z is North/South
	* X is East/West
	* Reads +X, -Z, so invert Z (subtract)
	* PixelMap X is E/W
	*/
	//Red - Tile ID
	//Green - rotation
	//Blue - rotation 2 (for 256 deg and up)
	//change to:
	//Green - rot / 3 (*3 for rotation)
	//blue - subset (IE door_A, door_B, ceilingwall_A, ceilingwall_B)

	void Awake()
	{
		PixelMap_IDLibrary.PIX_DICT internalDictConstructor = new PixelMap_IDLibrary.PIX_DICT();
		if (!internalDictConstructor.isInitialized)
		{
			internalDictConstructor.InitializeTileDictionary();
		}
	}

	public void BitMapInterpreter()
	{
		//Load the parameter bitmap
		string path = SolvePath(bitmapName, ".png", PixelMapRootPath);// "\\Assets\\Resources\\Maps\\PixelMaps");
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
		int heightInPixels = levelMapData.Height;
		int widthInBytes = levelMapData.Width * bytesPerPixel;
		//declare the variables we need inside the loop
		string prefabID;
		Dictionary<int, string> returnedDict;
		string fullpath;
		float rotation;
		GameObject prefab;
		//establish the 3D (y) axis
		//y3D = transform.localPosition.y;
		//establish our build plane
		x3D = 0.0f;
		z3D = 0.0f;
		Debug.Log("Attempting to build level...");
		//itterate through the bitmap data
		for (int y = 0; y < heightInPixels; y++)
		{
			int currentLine = y * levelMapData.Stride;
			for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
			{
				int blue = pixels[currentLine + x];
				int green = pixels[currentLine + x + 1];
				int red = pixels[currentLine + x + 2];
				//image processing
				if (red != 255 )//|| red != 254)
				{
					//check the R value against the dictionary of tile monochannel IDs
					if (PixelMap_IDLibrary.PIXID_TILELIB.TryGetValue(red, out returnedDict))//PIXID_LIBRARY
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
						PixelMap_IDLibrary.PIXID_PATHPAIRS.TryGetValue(prefabID, out fullpath);
						//pull rotation from G and multiply by 3
						rotation = green * 3;
						//load the prefab into memory via the Resource system
						prefab = Resources.Load(fullpath) as GameObject;
						//instanciate a prefab object at the coords relative from the pixelmap
						//Vector3 newRotation = new Vector3(0.0f, rotation, 0.0f);
						GameObject instanceObject = Instantiate(prefab, new Vector3(transform.position.x + x3D + 0.5f, transform.position.y, transform.position.z + z3D - 0.5f), new Quaternion(0.0f, 0.0f, 0.0f,1.0f), transform);
						//set the objects rotation (not using a quaternion for sake of euler precision)
						instanceObject.transform.localRotation = Quaternion.Euler(0.0f,rotation,0.0f);
						//set name to whatever is next (IE 1,2,3) so that not everything has the same name
						//if it's a floor
						if(red == 125)
						{
							//place a ceiling over it
							prefab = Resources.Load("Prefabs\\CMDRAsh\\ShipCeilingA") as GameObject;
							GameObject instanceObjectRoof = Instantiate(prefab, new Vector3(transform.position.x + x3D + 0.5f, transform.position.y + 3, transform.position.z + z3D - 0.5f), new Quaternion(0.0f, 0.0f, 0.0f, 1.0f), transform);
							instanceObjectRoof.transform.localRotation = Quaternion.Euler(0.0f,0.0f,180.0f);
						}
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
	}

	//attempt to path from the basedirectory to the file described by the 'name' and 'extension' params
	public string SolvePath(string name, string extension, string baseDirectory)
	{
		string root = Directory.GetCurrentDirectory();
		//move from root to base prefab directory
		//parse through prefabs and find the path for the prefab name
		string[] filesInDir;
		string[] pathParts;
		string myPath = "";
		string filePath = "";
		//get files in directory
		filesInDir = Directory.GetFiles(root + baseDirectory,"*"+extension,SearchOption.AllDirectories);
		//search for name
		foreach(string fileAndPath in filesInDir)
		{
			//split off the name of the tile and compare to var 'name'
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
			BitMapInterpreter();
		}
	}
}
