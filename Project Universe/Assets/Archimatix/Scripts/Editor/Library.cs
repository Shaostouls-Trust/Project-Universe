#pragma warning disable

using UnityEngine;
using UnityEditor;


using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using UnityEditor.AnimatedValues;


using AX.SimpleJSON;




using AXEditor;

using AXClipperLib;
using Path 	= System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;


using AXGeometry;

namespace AX
{

	[Serializable]
	public class Library {



		// PARAMETRIC_OBJECT

		public static List<AXParametricObject> m_parametricObjects;
		public  List<AXParametricObject>   parametricObjects
		{

			get { 
				return m_parametricObjects; 
			}
			set { m_parametricObjects = value; }
		}



		// LIBRARY_ITEMS


		public List<LibraryItem> libraryItems;

//		[System.NonSerialized]
//		public List<LibraryItem> libraryItems2D;
//
//		[System.NonSerialized]
//		public List<LibraryItem> libraryItems3D;


		[System.NonSerialized]
		public List<LibraryItem> m_filteredResults;
		public List<LibraryItem>   filteredResults
		{
			get { 
				if (m_filteredResults == null || ! isFiltered)
					return libraryItems;
				return m_filteredResults; 
			}
			set { m_filteredResults = value; }
		}

		[System.NonSerialized]
		public bool allLibraryThumbnailsLoaded = false;


		public bool isFiltered;


		[System.NonSerialized]
		public Dictionary<string, List<string>>  categories;

		[System.NonSerialized]
		public static string recentSaveFolder;


		[System.NonSerialized]
		public string searchString;
		List<string> queryTags;






		public Library()
		{


			initDataFile();
			//init ();

			  
			  
		}
		  
		public void saveLibraryMetadata()
		{
			string filename = getLibraryPath() + "axlibrary.dat";

			//Debug.Log(filename);
			BinaryFormatter bf = new BinaryFormatter ();
			 
			FileStream file = File.Create(filename);

			bf.Serialize (file, this);
			file.Dispose();
             file.Close (); 
		}

		public static Library loadLibraryMetadata()
		{
		 
			//Debug.Log("Loading Binary Library");
			string filename = getLibraryPath() + "axlibrary.dat";
			//Debug.Log("Loading Binary Library: "+ filename);

		  	if(File.Exists(filename))
			{



				BinaryFormatter bf = new BinaryFormatter();

					bf.Binder = new AllowAllAssemblyVersionsDeserializationBinder();
				//bf.Binder = new AllowAllAssemblyVersionsDeserializationBinder();
 
//				FileStream file = File.Open(filename, FileMode.Open);
//				Library library = (Library) bf.Deserialize(file);
//				file.Close();


				

					//StreamReader streamReader = new StreamReader(filename);

				System.IO.MemoryStream ms = new System.IO.MemoryStream(File.ReadAllBytes(filename));



					Library library = (Library) bf.Deserialize(ms);

					if (library == null || library.libraryItems == null || library.libraryItems.Count == 0)
					{
						ArchimatixEngine.createLibraryFromJSONFiles();
					}


				if (library == null || library.libraryItems == null || library.libraryItems.Count == 0)
				{
					Debug.Log("reloading library");
					ArchimatixEngine.createLibraryFromJSONFiles();
				}


				// check if the number of axfiles is different than the number of library items
				if (library != null && library.libraryItems != null)
				{
					DirectoryInfo info = new DirectoryInfo(Application.dataPath);
					FileInfo[] files = info.GetFiles("*.axobj", SearchOption.AllDirectories);

					if (files.Length != library.libraryItems.Count)
					{
						// this might be the case after you reinstall AX and the user has her own library items somewhere else in the asset folder.
						ArchimatixEngine.createLibraryFromJSONFiles();

					}

				}

				if (library != null && library.libraryItems != null && library.libraryItems.Count > 0)
				{


					// check to make sure that the libraryItems have the right path
					// if not, then reread from JSON files.
//					if ( File.Exists(library.libraryItems[0].readIntoLibraryFromAbsoluteAXOBJPath))
//						ArchimatixEngine.createLibraryFromJSONFiles();
					
 
					library.cacheLibraryItems();

					library.sortLibraryItems();

					return library;
				}



			}

			// no existing library
			ArchimatixEngine.createLibraryFromJSONFiles();
			return ArchimatixEngine.library;
			


		} 




		// GET_LIBRARY_ITEM

		public LibraryItem getLibraryItem(string itemName)
		{
			return libraryItems.Find(x => x.Name.Equals(itemName.Replace(" ", "")));
		} 




		public bool hasItem(string itemName)
		{
			LibraryItem tmp = getLibraryItem(itemName);

			return ( tmp != null) ;
		}
		 
		public static string getLibraryPath()
		{
			ArchimatixEngine.establishPaths();

			return Application.dataPath + "/"+ArchimatixEngine.ArchimatixAssetPath.Replace("Assets/", "")+"/Library/";
		}
		public static string getRelativeLibraryPath()
		{
			return ArchimatixEngine.ArchimatixAssetPath+"/Library/";
		}
		 


		public void init()
		{
			Debug.Log("LIB INIT");
			categories = new Dictionary<string, List<string>>();

			string filename = getLibraryPath() + "data.json";


		
			if(File.Exists(filename))
			{
				string json_string = File.ReadAllText(filename);
				Debug.Log (json_string);

				AX.SimpleJSON.JSONNode jn = AX.SimpleJSON.JSON.Parse(json_string);

				foreach(AX.SimpleJSON.JSONNode jn_cat in jn.AsArray)
				{
					Debug.Log (jn_cat.GetHashCode());
				}


			}

		}

		public void initDataFile()
		{

			string filename = getLibraryPath() + "data.json";

			if (true)// || ! File.Exists(filename))
			{
				// General Menus?
				string[] genres = new string[]
				{
					"Fantasy", 
					"Historic", 
					"Steampunk", 
					"Dieselpunk", 
					"Sci-Fi",
					"Urban",
					"War",
					"Western"
				};
				string[] types = new string[]
				{
					"Architecture", 
					"Furniture", 
					"Machine", 
					"Prop", 
					"Tool",
					"Vehicle",
					"Weapon"
				};

				// Tag Seach?
				string[] functions = new string[]
				{
					"Building",
					"Door",
					"Window",
					"Stair",
					"Floor",
					"Column",
					"Truss",
					"Cornice",
					"Beam",
					"Roof",
					"Tower",
					"Skylight",
					"Balcony",
					"Porch",
					"Shed",
					"Wheel",
					"Gear",
					"Gable",
					"Chair",
					"Table",
					"Bookcase",
					"Bed",
					"Dresser",
					"Cabinet",

					"Prop/Barrel",
					"Prop/Crate"

				};
				string[] periods = new string[]
				{
					"Neolithic", 
					"Ancient", 
					"Medieval", 
					"Renaissance",
					"Revolutionary",
					"Industrial",
					"Jazz Age",
					"Contemporary",
					"Futuristic"
				};
				string[] styles = new string[]
				{
					"International", 
					"Roman", 
					"Greco-Roman", 
					"Greek", 
					"Mayan",
					"Aztec",
					"Incan",
					"Tibetan",
					"Germanic",
					"Viking",
					"Celtic",
					"Native American",
					"Pacific Northwest",
					"Oceania",
					"African",
					"Aboriginal",
					"Japanese",
					"Korean",
					"Chinese"
				};


				// create

				categories = new Dictionary<string, List<string>>();

				StringBuilder sb = new StringBuilder();
				sb.Append("{\"categories\":[");


				// genres
				Array.Sort(genres);
				categories.Add ("Genre", new List<string>(genres));
				sb.Append("{\"Genre\":[");
				for(int i=0; i<genres.Length; i++)
					sb.Append(((i > 0)?",":"") + "\""+genres[i]+"\"");
				sb.Append("]},");



				// types 
				Array.Sort(types);
				categories.Add ("Type", new List<string>(types));
				sb.Append("{\"Type\":[");
				for(int i=0; i<types.Length; i++)
					sb.Append(((i > 0)?",":"") + "\""+types[i]+"\"");
				sb.Append("]},");

				// functions
				Array.Sort(functions);
				categories.Add ("Description", new List<string>(functions));
				sb.Append("{\"Description\":[");
				for(int i=0; i<functions.Length; i++)
					sb.Append(((i > 0)?",":"") + "\""+functions[i]+"\"");
				sb.Append("]},");


				// periods
				Array.Sort(periods);
				categories.Add ("Period", new List<string>(periods));
				sb.Append("{\"Period\":[");
				for(int i=0; i<periods.Length; i++)
					sb.Append(((i > 0)?",":"") + "\""+periods[i]+"\"");
				sb.Append("]},");

				Array.Sort(styles);
				categories.Add ("Style", new List<string>(styles));
				sb.Append("{\"Style\":[");
				for(int i=0; i<styles.Length; i++)
					sb.Append(((i > 0)?",":"") + "\""+styles[i]+"\"");
				sb.Append("]}");

				sb.Append("]"); // categories
				sb.Append("}");

				File.WriteAllText(filename, sb.ToString());
			}
			//AssetDatabase.Refresh();
			//string json_string = File.ReadAllText(f.FullName)
		}

		/* READ_LIBRARY FROM FILES
		 * If the cache is there, use it.
		 * Otherwise...
		 * Parse all the files in the Library data folder
		 * and, using the head object, extract meta data and build indices.
		 */ 
		public void readLibraryFromFiles()
		{ 
			//Debug.Log("read library");
			//StopWatch sw = new StopWatch();

			/* Maintain a live list of all the files found
			 * as head POs
			 * 
			 */
			parametricObjects = new List<AXParametricObject>();
			libraryItems		= new List<LibraryItem>();

			DirectoryInfo info = new DirectoryInfo(Application.dataPath);
			FileInfo[] files = info.GetFiles("*.axobj", SearchOption.AllDirectories);
             
			//Debug.Log ("LIBRARY READ " + files.Length);
			  
			try  
			{
				//foreach (var filepathname in files)
				for (int i = 0; i < files.Length; i++) {

					//Debug.Log ("file: " + files[i].ToString());
					string filepathname = files[i].ToString();

					AX.SimpleJSON.JSONNode jn = AX.SimpleJSON.JSON.Parse(File.ReadAllText(filepathname));


					AXParametricObject head_po = null;

	
					if (jn ["parametric_objects"] != null)
					{
						head_po = JSONSerializersAX.ParametricObjectFromJSON (jn ["parametric_objects"] [0]);
					}
					else
					{
						head_po = JSONSerializersAX.ParametricObjectFromJSON (jn [0]);
					}

										//head_po.readIntoLibraryFromPathname = filepathname;
					head_po.readIntoLibraryFromRelativeAXOBJPath = ArchimatixUtils.getRelativeFilePath(filepathname);




					parametricObjects.Add (head_po);

 
					libraryItems.Add(new LibraryItem(head_po));

				} 
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}



			rebuildTagListsFromExistingPOs();



			cacheLibraryItems();


			//Debug.Log ("LIBRARY READ IN "+sw.stop()+" MILLISECONDS " + parametricObjects.Count + " items.");
		}





		public void addLibraryItemToList(LibraryItem li)
		{
		 
			// if a n item with same name already exists, remove it from the live list.
			foreach (LibraryItem item in libraryItems)
			{
				if (item.Name == li.Name)
				{
					removeLibraryItemFromList(item);
					break;
				}
			}

			libraryItems.Add(li);

			sortLibraryItems();

		}


		public void removeLibraryItemFromList(LibraryItem li)
		{
			if (libraryItems.Contains(li))
				libraryItems.Remove(li);

			if (filteredResults.Contains(li))
				filteredResults.Remove(li);

			sortLibraryItems();

		}


		public void cacheLibraryItems()
		{ 
			//Debug.Log("cacheLibraryItems");

			//libraryItems 	= new List<LibraryItem>();
			if (libraryItems == null)
				return;


			loadThumnails();


			/*
			foreach(LibraryItem li in libraryItems)
			{

				// CACHE THUMBNAIL TEXTURE

				string prefix					= (li.is2D) ? "zz-AX-2DLib-" : "zz-AX-3DLib-";

				string filename 				= System.IO.Path.GetFileNameWithoutExtension(li.readIntoLibraryFromRelativeAXOBJPath);

				string noPrefixThumbnailPath 	= System.IO.Path.ChangeExtension(li.readIntoLibraryFromRelativeAXOBJPath, ".jpg");

				string ThumbnailPathWithPrefix  = noPrefixThumbnailPath.Replace(filename, prefix+filename);




				// Migration - we are adding the prefix "zz-AXLib-" as of v0.8.25 [2016-06-16].
				// check if that thumbnail without prefix is there. If so, then rename it.

//				bool migrating = System.IO.File.Exists(noPrefixThumbnailPath);
//
//				if (migrating)
//				{
//					didMigration = true;
//
//					if (System.IO.File.Exists(ThumbnailPathWithPrefix))
//						System.IO.File.Delete(ThumbnailPathWithPrefix);
//
//					System.IO.File.Move(noPrefixThumbnailPath, ThumbnailPathWithPrefix);
//				}


				// Get the relative path
				string thumbnailRelativePath = ThumbnailPathWithPrefix; //ArchimatixUtils.getRelativeFilePath(ThumbnailPathWithPrefix); ;


				Texture2D tex = (Texture2D) AssetDatabase.LoadAssetAtPath(thumbnailRelativePath, typeof(Texture2D));

				allLibraryThumbnailsLoaded = true;

				if (tex == null) 
				{
					allLibraryThumbnailsLoaded = false;
					//Debug.Log("Not ready: " + thumbnailRelativePath);
				}
				else
					li.icon = tex;

				// Set the import settings for GUI texture
//				if (migrating)
//				{
//					//AssetDatabase.ImportAsset(thumbnailRelativePath); //-- this doesn't seem necessary
//
//					TextureImporter importer 			= AssetImporter.GetAtPath(thumbnailRelativePath) as TextureImporter;
//					if (importer != null)
//					{
//						importer.textureType=TextureImporterType.GUI;
//						importer.maxTextureSize 			= 256;
//
//
//						#if UNITY_5_5_OR_NEWER
//						importer.textureCompression			= TextureImporterCompression.Uncompressed;
//						#else
//						importer.textureFormat				= TextureImporterFormat.AutomaticTruecolor;
//						#endif
//
//						AssetDatabase.WriteImportSettingsIfDirty(thumbnailRelativePath);
//					}
//				}

				 
	
			}
			*/

			sortLibraryItems();

		}

		public void sortLibraryItems()
		{
			libraryItems = libraryItems.OrderBy(libItem => libItem.sortval).ToList();



		}

		public void loadThumnails()
		{
			allLibraryThumbnailsLoaded = true;

			foreach(LibraryItem li in libraryItems)
			{
				if (li.icon != null)
					continue;
					
				string prefix					= (li.is2D) ? "zz-AX-2DLib-" : "zz-AX-3DLib-";

				string filename 				= System.IO.Path.GetFileNameWithoutExtension(li.readIntoLibraryFromRelativeAXOBJPath);

				string noPrefixThumbnailPath 	= System.IO.Path.ChangeExtension(li.readIntoLibraryFromRelativeAXOBJPath, ".jpg");

				string ThumbnailPathWithPrefix  = noPrefixThumbnailPath.Replace(filename, prefix+filename);

				// Get the relative path
				string thumbnailRelativePath = ThumbnailPathWithPrefix; //ArchimatixUtils.getRelativeFilePath(ThumbnailPathWithPrefix); ;


				Texture2D tex = (Texture2D) AssetDatabase.LoadAssetAtPath(thumbnailRelativePath, typeof(Texture2D));

				if (tex == null) 
				{
					allLibraryThumbnailsLoaded = false;
					//Debug.Log(": " + thumbnailRelativePath);
				}
				else
					li.icon = tex;


			}



		}

		public AXParametricObject getHeadParametricObjectWithName(string n)
		{

			return parametricObjects.Find(x => x.Name.Equals(n));


		}
		public void saveParametricObjectMetadata(AXParametricObject head_po)
		{

			//Debug.Log("SAVING head_po.author="+head_po.author);

			AX.SimpleJSON.JSONNode hn = AX.SimpleJSON.JSON.Parse(JSONSerializersAX.ParametricObjectAsJSON(head_po));

			string filename = ArchimatixUtils.getAbsoluteLibraryPath(head_po.readIntoLibraryFromRelativeAXOBJPath);

			AX.SimpleJSON.JSONNode jn = AX.SimpleJSON.JSON.Parse(File.ReadAllText(filename));

			jn["parametric_objects"][0] = hn;

			File.WriteAllText( filename, jn.ToString());

			rebuildTagListsFromExistingPOs();
		} 







		public static AXParametricObject instantiateParametricObject(string readIntoLibraryFromRelativeAXOBJPath)
		{


			AXModel model = null;

			// 1. First choice is to use a selected model....
			if(ArchimatixEngine.currentModel != null)
				model = ArchimatixEngine.currentModel;



			bool doInstantiation = true;

			if (model == null)
			{ 
				// ok, are there other models in the scene that the user doesn't realize arene't selected?
				AXModel[] axModels =  GameObject.FindObjectsOfType(typeof(AXModel)) as AXModel[];

				if (axModels != null)
				{
					if (axModels.Length == 1)
					{	
						// Offer this model as a button,  but also give opportunity to create a new one... 
						int option = EditorUtility.DisplayDialogComplex("Instantiating Library Item", "There is no AX model currently selected",  "Cancel",  "Add to "+axModels[0],  "Create a new Model"); 


						switch (option) {
						// Save Scene
						case 0:
							doInstantiation = false;
							break;
							// Save and Quit.
						case 1:
							model = axModels[0];
							break;

						case 2:
							GameObject axgo = AXEditorUtilities.createNewModel();
							model = axgo.GetComponent<AXModel>();
							doInstantiation = false;
							break;

						default:
							doInstantiation = false;
							break;

						}
					}

					else if (axModels.Length > 1)
					{
						// Throw up a dialog to see if use wants to go select one of the existing models or create a new one.

						if (EditorUtility.DisplayDialog("Instantiating Library Item", "There is no AX model currently selected",   "Select and Existing Model",  "Create a new Model")) 
						{ 
							return null; 
						}

					}
				}

			}


			if (! doInstantiation)
				return null;



			if (model == null)
			{	// Well, then, we tried everything....
				GameObject axgo = AXEditorUtilities.createNewModel();
				model = axgo.GetComponent<AXModel>();
			}

			AXParametricObject mostRecentPO = model.recentlySelectedPO;//getPOSelectedBefore(head_po);

			if (mostRecentPO != null)
				mostRecentPO = model.mostRecentlyInstantiatedPO;


			//Library.lastInstantiation = po;

			// This file path is where the metadata
			// for this head_po was found. Go back to this file 
			// and get the rest of the data...
			string filepath = ArchimatixUtils.getAbsoluteLibraryPath(readIntoLibraryFromRelativeAXOBJPath);


			List<AXParametricObject> poList = new List<AXParametricObject>();
			List<AXRelation> relationList = new List<AXRelation>();

			AX.SimpleJSON.JSONNode json = AX.SimpleJSON.JSON.Parse(File.ReadAllText(filepath));

			foreach(AX.SimpleJSON.JSONNode poNode in json["parametric_objects"].AsArray)
				poList.Add (JSONSerializersAX.ParametricObjectFromJSON(poNode));

			foreach(AX.SimpleJSON.JSONNode rNode in json["relations"].AsArray)
				relationList.Add (AXRelation.fromJSON(rNode));

			//Debug.Log (rNode);





			// make sure there is a game object ready to instantiate this on



			if (model == null)
			{
				// check if the user really wants to create a new model...

				GameObject axgo = AXEditorUtilities.createNewModel();
				model = axgo.GetComponent<AXModel>();
			}

			Undo.RegisterCompleteObjectUndo (model, "Add Library Object");



			// ADD AND RIGUP 
			// !!! Actually add the library item (as a list of PO's and Relations) to the model
			model.addAndRigUp(poList, relationList);



			AXParametricObject head_po = poList[0];

			head_po.startStowInputs();
			//if (poList[0].is2D() && ArchimatixEngine.workingAxis != (int) Axis.NONE)
			//	poList[0].intValue ("Axis", ArchimatixEngine.workingAxis);


			model.setRenderMode( AXModel.RenderMode.GameObjects);
			model.generate();

			ArchimatixEngine.currentModel = model; 
			Selection.activeGameObject = model.gameObject;


			model.deselectAll();
			model.selectOnlyPO(head_po);
			head_po.focusMe = true;


			//AXParametricObject mostRecentPO = model.recentlySelectedPO;



			//Debug.Log ("model.focusPointInGraphEditor="+model.focusPointInGraphEditor);

			float pos_x = (model.focusPointInGraphEditor.x)+100;// + UnityEngine.Random.Range(-100, 300);
			float pos_y = (model.focusPointInGraphEditor.y - 250) + UnityEngine.Random.Range(-10, 0);

			if(mostRecentPO != null)
			{
				pos_x =  mostRecentPO.rect.x + 200;
				pos_y =  mostRecentPO.rect.y; 
			}

			//Debug.Log (new Rect(pos_x, pos_y, 150, 100));
			head_po.rect = new Rect(pos_x, pos_y, 150, 500); 


			// 2D ORIENT TO DEFAULT AXIS
			if (head_po.is2D() && SceneView.lastActiveSceneView != null )
			{
				head_po.setAxis(AXEditorUtilities.getDefaultAxisBasedBasedOnSceneViewOrientation(SceneView.lastActiveSceneView));
			}


			//Debug.Log ("OK "+po.Name+".focusMe = " + po.focusMe);
			// make sure it is in view in the editor window....

			//model.startPanningToRect(head_po.rect);

			 
			//model.cleanGraph();
			model.remapMaterialTools();
			model.autobuild();
			head_po.generator.adjustWorldMatrices();
			model.build();

			model.selectOnlyPO(head_po);

			//model.beginPanningToRect(head_po.rect);
			//AXNodeGraphEditorWindow.zoomToRectIfOpen(head_po.rect);

			//Debug.Log(" 1 model.selectedPOs.Count="+model.selectedPOs.Count + " " + model.selectedPOs[0].Name );
			// EDITOR WINDOW
			ArchimatixEngine.repaintGraphEditorIfExistsAndOpen();





			// SCENEVIEW
			if (SceneView.lastActiveSceneView != null)
			{

				// IF THIS IS A 2D SHAPE and IN 2D SCENVIEW MODE, THEN ASSERT AXIS NZ

	
				//Debug.Log("REPAINT");
				SceneView.lastActiveSceneView.Repaint();

			}

			//Debug.Log("--> ArchimatixEngine.currentModel=" + ArchimatixEngine.currentModel);

			model.mostRecentlyInstantiatedPO = head_po;
			model.mostRecentlySelectedPO	 = head_po;

			return head_po;





		}



		public static void removeLibraryItem(LibraryItem li)
		{
			string filepath = ArchimatixUtils.getAbsoluteLibraryPath(li.readIntoLibraryFromRelativeAXOBJPath);


			// First remove the item from the live library. THat way when the file is deleted, AX will not try to rebuild the library.
			//string nodeName = System.IO.Path.GetFileNameWithoutExtension(filepath);

			//LibraryItem li = ArchimatixEngine.library.getLibraryItem(nodeName);

			ArchimatixEngine.library.removeLibraryItemFromList(li);

			//string localFilepath = filepath.Substring(filepath.IndexOf("Assets/"));
			string axobjRelativePath = ArchimatixUtils.getRelativeFilePath(filepath); ;



			// [Darkwell: change "undoable" to "cannot be undone" - 2016.06.02]
			if (EditorUtility.DisplayDialog("Delete Library Object?",
				"Are you sure you want to delete " + li.Name
				+ "? This cannot be undone!", "Really Delete", "Cancel")) 
			{
			 
				AssetDatabase.DeleteAsset(axobjRelativePath);

				string prefix = (li.is2D) ? "zz-AX-2DLib-" : "zz-AX-3DLib-";
				string filename 				= System.IO.Path.GetFileNameWithoutExtension(li.readIntoLibraryFromRelativeAXOBJPath);
				string noPrefixThumbnailPath 	= System.IO.Path.ChangeExtension(li.readIntoLibraryFromRelativeAXOBJPath, ".jpg");
				string ThumbnailPathWithPrefix  = noPrefixThumbnailPath.Replace(filename, prefix+filename);

				AssetDatabase.DeleteAsset(ThumbnailPathWithPrefix);

				//Debug.Log(thumbnailRelativePath + " deleted");

				// Refresh the AssetDatabase after all the changes
				AssetDatabase.Refresh();


				//library.readLibraryFromFiles();

			}
		}

		public static AXParametricObject pasteParametricObjectFromString(String json_string)
		{
			
			if (string.IsNullOrEmpty(json_string))
				json_string = EditorGUIUtility.systemCopyBuffer;

			if (string.IsNullOrEmpty(json_string))
				return null;
			
			AXModel model = null;
			 
			// 1. First choice is to use a selected model....
			if(ArchimatixEngine.currentModel != null)
				model = ArchimatixEngine.currentModel;

			bool doInstantiation = true;

			if (model == null)
			{ 
				// ok, are there other models in the scene that the user doesn't realize arene't selected?
				AXModel[] axModels =  GameObject.FindObjectsOfType(typeof(AXModel)) as AXModel[];

				if (axModels != null)
				{ 
					if (axModels.Length == 1)
					{	
						// Offer this model as a button,  but also give opportunity to create a new one... 
						int option = EditorUtility.DisplayDialogComplex("Instantiating Library Item", "There is no AX model currently selected",  "Cancel",  "Add to "+axModels[0],  "Create a new Model"); 


						switch (option) {
						// Save Scene
						case 0:
							doInstantiation = false;
							break;
							// Save and Quit.
						case 1:
							model = axModels[0];
							break;

						case 2:
							GameObject axgo = AXEditorUtilities.createNewModel();
							model = axgo.GetComponent<AXModel>();
							doInstantiation = false;
							break;

						default:
							doInstantiation = false;
							break;

						}
					}

					else if (axModels.Length > 1)
					{
						// Throw up a dialog to see if use wants to go select one of the existing models or create a new one.

						if (EditorUtility.DisplayDialog("Instantiating Library Item", "There is no AX model currently selected",   "Select and Existing Model",  "Create a new Model")) 
						{ 
							return null; 
						}

					}
				}

			}


			if (! doInstantiation)
				return null;



			if (model == null)
			{	// Well, then, we tried everything....
				GameObject axgo = AXEditorUtilities.createNewModel();
				model = axgo.GetComponent<AXModel>();
			}



			


			List<AXParametricObject> poList = new List<AXParametricObject>();
			List<AXRelation> relationList = new List<AXRelation>();
			AX.SimpleJSON.JSONNode json = null;
			try
			{
				json = AX.SimpleJSON.JSON.Parse(json_string);
			}
			catch
			{
				return null;
			}
			if (json == null)
				return null;

			if (json["parametric_objects"] != null)
				foreach(AX.SimpleJSON.JSONNode poNode in json["parametric_objects"].AsArray)
				poList.Add (JSONSerializersAX.ParametricObjectFromJSON(poNode));

			foreach(AX.SimpleJSON.JSONNode rNode in json["relations"].AsArray)
				relationList.Add (AXRelation.fromJSON(rNode));

			//Debug.Log (rNode);





			// make sure there is a game object ready to instantiate this on



			if (model == null)
			{
				// check if the user really wants to create a new model...

				GameObject axgo = AXEditorUtilities.createNewModel();
				model = axgo.GetComponent<AXModel>();
			}

			Undo.RegisterCompleteObjectUndo (model, "Add Library Object");

			AXParametricObject head_po = poList[0];



			if (model.currentWorkingGroupPO != null && model.currentWorkingGroupPO.Name != head_po.Name)
			{
				head_po.grouperKey = model.currentWorkingGroupPO.Guid;
			}
			else
				head_po.grouperKey = null;



			// ADD AND RIGUP 
			// !!! Actually add the library item (as a list of PO's and Relations) to the model
			model.addAndRigUp(poList, relationList);





			head_po.startStowInputs();
			//if (poList[0].is2D() && ArchimatixEngine.workingAxis != (int) Axis.NONE)
			//	poList[0].intValue ("Axis", ArchimatixEngine.workingAxis);


			model.setRenderMode( AXModel.RenderMode.GameObjects);
			model.generate();

			ArchimatixEngine.currentModel = model; 
			Selection.activeGameObject = model.gameObject;


			AXParametricObject mostRecentPO = model.recentlySelectedPO;//getPOSelectedBefore(head_po);


			model.selectOnlyPO(head_po);
			head_po.focusMe = true;


			//AXParametricObject mostRecentPO = model.recentlySelectedPO;
			//AXParametricObject mostRecentPO = model.recentlySelectedPO;//getPOSelectedBefore(head_po);



			//Debug.Log ("model.focusPointInGraphEditor="+model.focusPointInGraphEditor);

			float pos_x = (model.focusPointInGraphEditor.x)+100;// + UnityEngine.Random.Range(-100, 300);
			float pos_y = (model.focusPointInGraphEditor.y - 250) + UnityEngine.Random.Range(-10, 0);

			if(mostRecentPO != null)
			{
				pos_x =  mostRecentPO.rect.x + 200;
				pos_y =  mostRecentPO.rect.y;
			}

			//Debug.Log (new Rect(pos_x, pos_y, 150, 100));
			head_po.rect = new Rect(pos_x, pos_y, 150, 500);



			// IF THIS IS A 2D SHAPE and IN 2D SCENVIEW MODE, THEN ASSERT AXIS NZ

			if (head_po.is2D() && SceneView.lastActiveSceneView != null && SceneView.lastActiveSceneView.in2DMode)
			{
				head_po.setAxis(Axis.NZ);
			}

			 
			//Debug.Log ("OK "+po.Name+".focusMe = " + po.focusMe);
			// make sure it is in view in the editor window....

			model.beginPanningToRect(head_po.rect);


			//model.cleanGraph();
			model.autobuild();
			head_po.generator.adjustWorldMatrices();


			model.selectOnlyPO(head_po);



			// Paste inside grouper?
			if (model.currentWorkingGroupPO != null && model.currentWorkingGroupPO.Name != head_po.Name)
			{
				model.currentWorkingGroupPO.addGroupee(head_po);

			}  


			if (model.currentWorkingGroupPO != null && model.currentWorkingGroupPO.Name == head_po.Name)
			{
				model.currentWorkingGroupPO = model.currentWorkingGroupPO.grouper;
			}





			//model.beginPanningToRect(head_po.rect);
			//AXNodeGraphEditorWindow.zoomToRectIfOpen(head_po.rect);

			//Debug.Log(" 1 model.selectedPOs.Count="+model.selectedPOs.Count + " " + model.selectedPOs[0].Name );
			// EDITOR WINDOW
			ArchimatixEngine.repaintGraphEditorIfExistsAndOpen();





			// SCENEVIEW
			if (SceneView.lastActiveSceneView != null)
			{
				//Debug.Log("REPAINT");
				SceneView.lastActiveSceneView.Repaint();

			}

			//Debug.Log("--> ArchimatixEngine.currentModel=" + ArchimatixEngine.currentModel);

			model.mostRecentlyInstantiatedPO = head_po;
			model.mostRecentlySelectedPO	 = head_po;

			return head_po;

		}











		public string get2DMenu()
		{
			if (filteredResults == null)
				return "";

			string menuItemString = "";
			string delimeter = "";
			foreach (LibraryItem li in filteredResults) 
			{
				if (li.is2D) 
				{
					menuItemString += (delimeter + li.Name);
					delimeter = "|";
				}
			}
			return menuItemString;
		}


		public string get3DMenu()
		{
			if (filteredResults == null)
				return "";

			string menuItemString = "";
			string delimeter = "";
			foreach (LibraryItem li  in filteredResults) 
			{
				if (! li.is2D)
				{
					menuItemString += (delimeter + li.Name);
					delimeter = "|";
				}
			}
			return menuItemString;
		}








		public AXParametricObject getFirstItemByName(string name)
		{
			foreach(AXParametricObject po in m_parametricObjects)
			{
				if (po.Name == name)
				{
					return po;

				}

			}
			return null;

		}


		public void filterResultsUsingSearch(List<string> queryTags)
		{
			if (queryTags == null || queryTags.Count == 0)
			{
				filteredResults = null;
				return;
			}

			//Debug.Log ("OK, Filter");
			List<LibraryItem> tmp_filteredResults = new List<LibraryItem>();

			string namelower;
			string po_tags_lower;
			string qtaglower;


			foreach(LibraryItem li  in libraryItems)
			{
				namelower 		= li.Name.ToLower();
				po_tags_lower 	= (! string.IsNullOrEmpty(li.tags)) ? li.tags.ToLower() : "";

				foreach(string qtag in queryTags)
				{
					qtaglower = qtag.ToLower();

					if (namelower.Contains(qtaglower) || po_tags_lower.Contains(qtaglower))
					{
						tmp_filteredResults.Add (li);
					}
				}
			}

			if (tmp_filteredResults.Count > 0)
			{
				isFiltered = true;
				filteredResults = tmp_filteredResults;
			}
			else
			{
				isFiltered = true;
				filteredResults = tmp_filteredResults;
			}



			sortLibraryItems();

		}










		public void filterResultsUsing(List<string> queryTags)
		{
			if (queryTags == null || queryTags.Count == 0)
			{
				filteredResults = null;
				return;
			}


			filteredResults = new List<LibraryItem>();

			foreach(LibraryItem li  in libraryItems)
			{
				foreach(string tag in queryTags)
				{
					if (li.tags.Contains(tag) && ! filteredResults.Contains(li))
						filteredResults.Add (li);
				}

			}
		}


		public void rebuildTagListsFromExistingPOs()
		{
			string[] tags;
			string[] parts;


			//string category;
			//string item;

			if (parametricObjects == null)
				return;

			foreach(AXParametricObject po in parametricObjects)
			{
				if (! String.IsNullOrEmpty(po.tags))
				{
					po.tags = po.tags.Trim( new Char[] { ' ', ',' } );
					tags = po.tags.Split(',');

					foreach(string tag in tags)
					{
						// SEE IF THERE IS A HEIRARCHY
						parts = tag.Split('/');


						if (parts.Length == 3)
						{
							//category = parts[0];
							//item = parts[1];

							//if (! categories.ContainsKey(category))
							//categories.Add (category);





						}
						else
						{
							// check if it is in any list
							// else add it to Description
							if (! categories["Description"].Contains(tag))
								categories["Description"].Add(tag);
						}
					}

					categories["Description"].Sort ();

				}

			}

		}




		public static void cache2DThumbnail(AXParametricObject po)
		{
			//if (po.thumbnail == null)

			int texSize 	= 256;
			int imageSize = 200;

			po.thumbnail = new Texture2D(texSize, texSize);

			// draw to this thumnail...
			//po.generateOutputNow();


			// PREPARE THE TEXTURE
			int y = 0;
			//Color fillcolor = new Color (.2f,.1f,.2f,.1f);
			Color fillcolor = new Color(.318f,.31f,.376f);
			//Color fillcolor = new Color(.30f,.29f,.31f);
			//Color fillcolor = new Color(.258f,.25f,.316f);

			//Debug.Log("FILLCOLOR");
			while (y < po.thumbnail.height) {
				int x = 0;
				while (x < po.thumbnail.width) {
					po.thumbnail.SetPixel(x, y, fillcolor);
					x++;
				}
				y++;
			}



			//AXSpline os = output_p.spline.clone ();
			//os.rotate (po.floatValue("Rot_Z"));

			Library.DrawOutputPaths(po.thumbnail, po, texSize, imageSize);


		}

		public static void DrawOutputPaths(Texture2D canvas, AXParametricObject po, float texSize, float imageSize) 
		{

			if (po == null) 
				return;

			// GET OUTPUT PARAMETER
			AXParameter p = po.getPreferredOutputSplineParameter();

			if (p == null)
				return;

			bool isClosed = (p.shapeState == ShapeState.Closed);

			Paths paths = null;
			if(p.polyTree != null)
				paths = Clipper.PolyTreeToPaths(p.polyTree);
			else if (p.paths != null)
				paths = p.paths;		



			Rect 	bounds = AXGeometry.Utilities.getClipperBounds(paths);
			float 	maxdim = (bounds.width > bounds.height) ? bounds.width : bounds.height;
			float 	scale = imageSize / maxdim;


			long[] shifter = AXGeometry.Utilities.getShifterToPathsCenter(paths);
			Paths centeredPaths = AXGeometry.Utilities.shiftPathsAsGroup(paths, shifter);



			//Archimatix.printPaths(centeredPaths);

			Vector2 offset = new Vector2(texSize/2, texSize/2);

			float rot = po.floatValue("Rot_Z");




			Color lineColor = Color.magenta;
			lineColor = new Color(1, .5f, 1);
			foreach(Path path in centeredPaths)
				Library.DrawPathsOnTexture(canvas, AXGeometry.Utilities.path2Vec2s(path, rot, scale), offset, isClosed, lineColor);

			// AXES
			long s =  (shifter[2]/18);

			Path xaxis = new Path();
			xaxis.Add(new IntPoint(-s-shifter[0], -shifter[1]));
			xaxis.Add(new IntPoint( s-shifter[0], -shifter[1]));
			Library.DrawPathsOnTexture(canvas, AXGeometry.Utilities.path2Vec2s(xaxis, rot, scale), offset, false, Color.white);

			Path yaxis = new Path();
			yaxis.Add(new IntPoint(-shifter[0], -s-shifter[1]));
			yaxis.Add(new IntPoint(-shifter[0],  s-shifter[1]));
			Library.DrawPathsOnTexture(canvas, AXGeometry.Utilities.path2Vec2s(yaxis, rot, scale), offset, false, Color.white);



		}

		public static void DrawPathsOnTexture(Texture2D canvas, Vector2[] verts, Vector2 offset, bool isClosed, Color color) 
		{
			//Debug.Log ("========== DRAW =========");
			//Debug.Log ("offset="+offset);
			//for (int i=0; i<verts.Length; i++)
			//Debug.Log (i + ": " +verts[i]);

			if (verts.Length > 1) 
			{
				int x0, y0, x1, y1;


				if (verts != null)
				{
					for (int i=1; i<verts.Length; i++) {
						//Debug.Log(verts[i]);
						if (verts[i-1].x >10000 || verts[i].x >10000)
							continue;
						x0 = (int)  (verts[i-1].x+offset.x);
						y0 = (int)  (verts[i-1].y+offset.y);
						x1 = (int)  (  verts[i].x+offset.x);
						y1 = (int)  (  verts[i].y+offset.y);

						TextureDrawLine.DrawLine(canvas, x0, y0, x1, y1, color);
					}

					color.a = .2f;
					if(isClosed)
					{
						x0 = (int) (verts[verts.Length-1].x+offset.x);
						y0 = (int) (verts[verts.Length-1].y+offset.y);
						x1 = (int) (			 verts[0].x+offset.x);
						y1 = (int) (			 verts[0].y+offset.y);
						TextureDrawLine.DrawLine(canvas, x0, y0, x1, y1, color);
					}
				}


			}



		}





	}



	[Serializable]
	public class LibraryItem
	{
		[NonSerialized]
		public AXParametricObject po;

		[NonSerialized]
		public Texture2D icon;

		[NonSerialized]
		public bool isEditing;

		public bool includeInSidebarMenu = true;

		public string 	Name;
		public string 	guid;

		public string 	description;

		public bool 	is2D;
		public float 	sortval = 10000;

		public string 	tags = "";
		public string 	documentationURL = "";

		public string 	author = "";
		public DateTime createdate; 

		public string readIntoLibraryFromRelativeAXOBJPath;

		[NonSerialized]
		LibraryItem cache; 

		public LibraryItem(AXParametricObject po = null)
		{
			if (po == null)
				return;

		

			Name 			= po.Name;
			guid			= po.Guid;

			description 	= po.description;

			is2D			= po.is2D();
			sortval 		= po.sortval;

			tags 			= po.tags;

			author  		= po.author;

			documentationURL = po.documentationURL;

			readIntoLibraryFromRelativeAXOBJPath = po.readIntoLibraryFromRelativeAXOBJPath;

			includeInSidebarMenu = po.includeInSidebarMenu;

		}

		public void cacheSelf()
		{
			cache = new LibraryItem();

			cache.Name 				= Name;
			cache.description 		= description;
			cache.tags 				= tags;
			cache.author 			= author;
			cache.documentationURL 	= documentationURL;
			cache.includeInSidebarMenu = includeInSidebarMenu;
		}
		public void revertFromCache()
		{
			if (cache == null)
				return;

			Name 					= cache.Name;
			description 			= cache.description;
			author 					= cache.author;
			tags 					= cache.tags;
			documentationURL 		= cache.documentationURL;
			includeInSidebarMenu 	= cache.includeInSidebarMenu;
		}

		public void saveToFile()
		{
			if (! string.IsNullOrEmpty(readIntoLibraryFromRelativeAXOBJPath))
			{
				string filepath = ArchimatixUtils.getAbsoluteLibraryPath(readIntoLibraryFromRelativeAXOBJPath);

				if (File.Exists(filepath))
				{
                    // DECODE
					AX.SimpleJSON.JSONNode jn = AX.SimpleJSON.JSON.Parse(File.ReadAllText(filepath));

					AX.SimpleJSON.JSONNode pon = jn[0];

					if (jn ["parametric_objects"] != null)
						pon = jn ["parametric_objects"] [0];

					pon["name"]					= Name;
					pon["includeInSidebarMenu"] = includeInSidebarMenu.ToString();
					pon["description"] 			= description;

					if (! string.IsNullOrEmpty(author))
						pon["author"]				= author;

					if (! string.IsNullOrEmpty(tags))
						pon["tags"]					= tags;

					if (! string.IsNullOrEmpty(documentationURL))
						pon["documentationURL"]		= documentationURL;

					if (! string.IsNullOrEmpty(readIntoLibraryFromRelativeAXOBJPath))
						pon["readIntoLibraryFromRelativeAXOBJPath"]	= readIntoLibraryFromRelativeAXOBJPath;

                    // resave
                     
                   
                    // ENCODEe
					File.WriteAllText( filepath, jn.ToString().Replace("\\","").Replace("turnl", "burnl_").Replace("turnr", "burnr_").Replace("tan", "ban_").Replace(";ttt", ";          ").Replace(";tt", ";        ").Replace(";t", "; ").Replace("burnl_", "turnl").Replace("burnr_", "turnr").Replace("ban_", "tan"));

					Debug.Log(jn.ToString().Replace("\\",""));
				}

			}

		}




	}



	//http://spazzarama.com/2009/06/25/binary-deserialize-unable-to-find-assembly/
	sealed class AllowAllAssemblyVersionsDeserializationBinder : System.Runtime.Serialization.SerializationBinder
{
    public override Type BindToType(string assemblyName, string typeName)
    {
        Type typeToDeserialize = null;
 
        String currentAssembly = Assembly.GetExecutingAssembly().FullName;
 
        // In this case we are always using the current assembly
        assemblyName = currentAssembly;
 
        // Get the type using the typeName and assemblyName
        typeToDeserialize = Type.GetType(String.Format("{0}, {1}",
            typeName, assemblyName));
 
        return typeToDeserialize;
    }
}
}
