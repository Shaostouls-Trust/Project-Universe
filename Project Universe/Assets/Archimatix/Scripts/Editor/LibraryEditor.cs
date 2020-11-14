using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.IO;

using UnityEditor.AnimatedValues;

using AX.SimpleJSON;



using AXGeometry;

using AX.Generators;
using AXEditor;

namespace AX
{
	// MANAGES ALL THE GENERIC CALLS TO THE LIBRARY IN THE EDITOR
	// JSON MUST BE DONE HERE BECAUS CALLS TOT HE ASSET DATABSE ARE REQUIRED,
	// PARTICULARLY IN po.toJSON()

	public class LibraryEditor  {

		/*
		public static void pasteFromJSONstringToSelectedModel(string json_string)
		{
			if (ArchimatixUtils.lastPastedPO != null)
				Debug.Log ("Archimatix.lastPastedPO="+ArchimatixUtils.lastPastedPO.Name);
			
			//Debug.Log ("PASTE FROM JSON**********************************************************************");
			 
			List<AXParametricObject> poList = new List<AXParametricObject>();
			List<AXRelation>   relationList = new List<AXRelation>();

			AX.SimpleJSON.JSONNode json = AX.SimpleJSON.JSON.Parse(json_string);

			if (json == null || json ["parametric_objects"] == null)
				return;
			
			foreach(AX.SimpleJSON.JSONNode poNode in json["parametric_objects"].AsArray)
				poList.Add (JSONSerializersAX.ParametricObjectFromJSON(poNode));
			
			foreach(AX.SimpleJSON.JSONNode rNode in json["relations"].AsArray)
			{
				//Debug.Log (rNode);
				relationList.Add (AXRelation.fromJSON(rNode));
			}
			//Debug.Log("relationList: " + relationList.Count);
			
			
			
			AXModel model = null;
			
			// make sure there is a game object
			if(ArchimatixEngine.currentModel != null)
				model = ArchimatixEngine.currentModel;
			
			if (model == null)
			{
				GameObject axgo = AXEditorUtilities.createNewModel();
				model = axgo.GetComponent<AXModel>();
				
			}
			
			Undo.RegisterCompleteObjectUndo (model, "Paste Object");
			

			// !!! Actually add the library item (as a list of PO's and Relations) to the model
			//Debug.Log ("ADD AND RIGUP");
			
			model.addAndRigUp(poList, relationList);
			
			poList[0].startStowInputs();
			
			
			
			//model.generate();
			
			
			//Selection.activeGameObject = model.gameObject;
			ArchimatixEngine.currentModel = model; 
			
			model.selectOnlyPO(poList[0]);

			//model.startPanningToRect(poList[0].rect);
			
			AXParametricObject lastPO = ArchimatixUtils.lastPastedPO;
			if (lastPO == null)
				lastPO = ArchimatixUtils.lastCopiedPO;
			
			int pos_x = Screen.width/2-50;
			int pos_y = Screen.height/2-100;
			
			if(lastPO != null)
			{
				pos_x = (int) lastPO.rect.x + 50;
				pos_y = (int) lastPO.rect.y + 50;
			}
			poList[0].rect = new Rect(pos_x, pos_y, 150, 100);
			
			
			// if last po has only one ouput, connect to the same consumer.
			AXParameter op = lastPO.getPreferredOutputSplineParameter();
			//if (op != null && op.Dependents != null && op.Dependents.COunt > 0)
				//Debug.Log (op.Name + " .................... "+ op.Dependents[0].Parent.Name + " ::: " + op.Dependents[0].Parent.generator.isOfType("ShapeMerger"));
				
			if (op != null && op.Dependents != null && op.Dependents.Count == 1 && op.Dependents[0].Parent.generator.isOfType("ShapeMerger"))
				op.Dependents[0].Parent.generator.getInputShape().addInput().makeDependentOn(poList[0].getPreferredOutputSplineParameter());
			
			
			ArchimatixUtils.lastPastedPO = poList[0];

			model.setAllAltered();

			model.autobuild();
			poList[0].generator.adjustWorldMatrices();
			
			ArchimatixEngine.repaintGraphEditorIfExistsAndOpen();

			// SCENEVIEW
			if (SceneView.lastActiveSceneView != null)
				SceneView.lastActiveSceneView.Repaint();
			
			
			
		}
		
		
	
	
		
		public static string poWithSubNodes_2_JSON_OLD(AXParametricObject po, bool withInputSubparts)
		{
			ArchimatixUtils.lastCopiedPO = po;
			ArchimatixUtils.lastPastedPO = null;
			
			// gather relations as you go...
			List<AXRelation> rels = new List<AXRelation>();
			
			// BUILD JSON STRING
			StringBuilder sb = new StringBuilder();
			sb.Append("{");
			sb.Append("\"parametric_objects\":[");
			sb.Append(JSONSerializersAX.ParametricObjectAsJSON(po));
			
			// gather rels
			foreach(AXParameter p in po.parameters)
			{
				foreach (AXRelation rr in p.relations)
					if (! rels.Contains(rr))
						rels.Add(rr);
			}
			
			
			
			if (withInputSubparts)
			{
				po.gatherSubnodes();
				foreach (AXParametricObject spo in po.subnodes)
				{
					sb.Append(',' + JSONSerializersAX.ParametricObjectAsJSON(spo));
					
					// gather rels
					foreach(AXParameter p in spo.parameters)
					{
						foreach (AXRelation rr in p.relations)
							if (! rels.Contains(rr))
								rels.Add(rr);
					}
					
				}
			}
			sb.Append("]");
			
			// add relations to json
			string thecomma;
			// RELATIONS
			if (rels != null && rels.Count > 0)
			{
				sb.Append(", \"relations\": [");		// begin parametric_objects
				
				thecomma = "";
				foreach(AXRelation rr in rels)
				{
					sb.Append(thecomma + rr.asJSON());
					thecomma = ", ";
				}
				sb.Append("]");
			}
			
			
			
			sb.Append("}");
			
			return sb.ToString();
			
		}
		*/



		public static string poWithSubNodes_2_JSON(AXParametricObject po, bool withInputSubparts)
		{

			string origGrouperKey = po.grouperKey;

			po.grouperKey = null;


			
			// gather relations as you go...
			List<AXRelation> rels = new List<AXRelation>();

			// BUILD JSON STRING
			StringBuilder sb = new StringBuilder();
			sb.Append("{");
			sb.Append("\"parametric_objects\":[");
			sb.Append(JSONSerializersAX.ParametricObjectAsJSON(po));

			// gather rels
			foreach(AXParameter p in po.parameters)
			{
				foreach (AXRelation rr in p.relations)
					if (! rels.Contains(rr))
						rels.Add(rr);
			}




			// SUB NODES

			if (withInputSubparts)
			{
				// GATHER SUB_NODES
				//Debug.Log("Start gather");
				po.gatherSubnodes();
				//Debug.Log("End gather");

				foreach (AXParametricObject spo in po.subnodes)
				{
					//Debug.Log("-- " + spo.Name);
					sb.Append(',' + JSONSerializersAX.ParametricObjectAsJSON(spo));

					// gather rels
					foreach(AXParameter p in spo.parameters)
					{
						foreach (AXRelation rr in p.relations)
							if (! rels.Contains(rr))
								rels.Add(rr);
					}

				}
			}
			sb.Append("]");






			// add relations to json
			string thecomma;
			// RELATIONS
			if (rels != null && rels.Count > 0)
			{
				sb.Append(", \"relations\": [");		// begin parametric_objects

				thecomma = "";
				foreach(AXRelation rr in rels)
				{
					sb.Append(thecomma + rr.asJSON() );
					thecomma = ", ";
				} 
				sb.Append("]");
			}



			sb.Append("}"); 



			// SAVE AS ASSET

			po.grouperKey = origGrouperKey;

			return sb.ToString();




		}



		// ** SAVING PO TO LIBRARY ** //

		// Try to save quickly the currently preferred library folder.

		public static string assertDefaultLibraryFolder()
		{
			string absoluteDefaultFolderPath =  System.IO.Path.Combine(Application.dataPath, "ArchimatixLibrary"); // of the form "Assets/folder1/folder2..."

			if (! System.IO.Directory.Exists(absoluteDefaultFolderPath))
				AssetDatabase.CreateFolder("Assets", "ArchimatixLibrary");

			return absoluteDefaultFolderPath;
		}


		// SAVE WITHOUT DIALOG POPUP IF POSSIBLE

		public static void doSave_MenuItem(object obj)
		{

			//Debug.Log("LastLibraryPath="+EditorPrefs.GetString("LastLibraryPath"));
			//EditorPrefs.SetString("LastLibraryPath", "");
			// 1. Is there a folder pref?
			string absoluteFolderPath = EditorPrefs.GetString("LastLibraryPath"); // of the form "Assets/folder1/folder2..."



			if (string.IsNullOrEmpty(absoluteFolderPath) || ! absoluteFolderPath.Contains(Application.dataPath))
			{
				// 1. THERE IS NO PREFERENCE FOR A FOLDER OR THE PREFERENCE IS FROM A DIFFERENT PROJECT -- USE DEFAULT

				absoluteFolderPath = System.IO.Path.Combine(Application.dataPath, "ArchimatixLibrary");

				//Debug.Log("CASE #1: " + absoluteFolderPath);
				doSavePO_ToSpecifiedFolder(assertDefaultLibraryFolder(), obj);

			} 
			else if (! System.IO.Directory.Exists(absoluteFolderPath))
			{
				// 2. THERE WAS A PREFERENCE, BUT IT DOES NOT EXIST -- Has the project name changed?
				string relativeFolderPath =  ArchimatixUtils.getRelativeFilePath(absoluteFolderPath);

				string tryThisAbsolutePath = System.IO.Path.Combine(Application.dataPath, relativeFolderPath);

				//Debug.Log("CASE #2: " + tryThisAbsolutePath);


				if (System.IO.Directory.Exists(tryThisAbsolutePath))
				{
					// 2a. There is an analagous folder in this project. go to file save panel with this as a suggestion
					doSavePO_ToSpecifiedFolder(tryThisAbsolutePath, obj);

				}
				else
				{
					// 2b. Go to file panel with default folder

					AXParametricObject po 	= (AXParametricObject) obj;
					doSave_SaveFilePanel(System.IO.Path.Combine(Application.dataPath, "ArchimatixLibrary"), po.Name, obj);

				}


			}
			else
			{
				//Debug.Log("CASE #3: " + absoluteFolderPath);
				// 3. The pref folder checks out, silently save to it...
				doSavePO_ToSpecifiedFolder(absoluteFolderPath, obj);
			}

		}




		public static void doSave_MenuItem_NewFolder(object obj)
		{
			AXParametricObject po 	= (AXParametricObject) obj;

			string absoluteFolderPath = Application.dataPath;


			doSave_SaveFilePanel(absoluteFolderPath, po.Name, obj);





		}








		public static void doSavePO_ToSpecifiedFolder(string absoluteFolderPath, object obj, string filename="")
		{
			AXParametricObject po 	= (AXParametricObject) obj;

			if(string.IsNullOrEmpty(filename))
				filename = po.Name+".axobj";

			string absoluteFilePath = System.IO.Path.Combine(absoluteFolderPath, filename);


			if (System.IO.Directory.Exists(absoluteFolderPath))
			{
				// file exists, ask to  overwrite or rename
				if (System.IO.File.Exists(absoluteFilePath))
					doSave_SaveFilePanel(absoluteFolderPath, filename, obj);

				else
					LibraryEditor.saveParametricObject(po, true, absoluteFilePath);
			}
			else
			{
				doSave_MenuItem_NewFolder(obj);
			}



		}





		public static void doSave_SaveFilePanel(string absoluteFolderPath, string filename, object obj)
		{


			//Debug.Log(absoluteFolderPath);

			string path = EditorUtility.SaveFilePanel(
				"Save parametric object",
				absoluteFolderPath,
				filename,
				"axobj");


			//Debug.Log(path);

			if(! String.IsNullOrEmpty(path)) {
				// Assumption here is that you can go ahead and overwrite at this point.

				// However, if the name has changed, then changed the name in the head.

				string save_Name = System.IO.Path.GetFileNameWithoutExtension(path);

				AXParametricObject po = (AXParametricObject) obj;

				string orig_Name = po.Name;

				po.Name = save_Name;

				// Save the folder selection to prefs

				// NOW SAVE!
				LibraryEditor.saveParametricObject(po, true, path);

				// revert name
				po.Name = orig_Name;

			}


		}






		// DO THE ACTUAL SAVE
		public static void saveParametricObject (AXParametricObject po, bool withInputSubparts, string filepathname)
		{
			//Debug.Log(filepathname);
			//EditorUtility.DisplayDialog("Archimatix Library", "Saving to Library: This may take a few moments.", "Ok");

			po.readIntoLibraryFromRelativeAXOBJPath = ArchimatixUtils.getRelativeFilePath(filepathname);

			Library.recentSaveFolder = System.IO.Path.GetDirectoryName(filepathname);



			// If this head po has a grouperKey, lose it! 
			// This is because when it is later instantiated, 
			// that grouper may not exist or may not be the desired place for this.

			po.grouperKey = null;



			// gather relations as you go...
			List<AXRelation> rels = new List<AXRelation>();

			// BUILD JSON STRING
			StringBuilder sb = new StringBuilder();
			sb.Append("{");
			sb.Append("\"parametric_objects\":[");
			sb.Append(JSONSerializersAX.ParametricObjectAsJSON(po));

			// gather rels
			foreach(AXParameter p in po.parameters)
			{
				foreach (AXRelation rr in p.relations)
					if (! rels.Contains(rr))
						rels.Add(rr);
			}




			// SUB NODES

			if (withInputSubparts)
			{
				// GATHER SUB_NODES
				//Debug.Log("Start gather");
				po.gatherSubnodes();
				//Debug.Log("End gather");

				foreach (AXParametricObject spo in po.subnodes)
				{
					//Debug.Log("-- " + spo.Name);
					sb.Append(',' + JSONSerializersAX.ParametricObjectAsJSON(spo));

					// gather rels
					foreach(AXParameter p in spo.parameters)
					{
						foreach (AXRelation rr in p.relations)
							if (! rels.Contains(rr))
								rels.Add(rr);
					}

				}
			}
			sb.Append("]");






			// add relations to json
			string thecomma;
			// RELATIONS
			if (rels != null && rels.Count > 0)
			{
				sb.Append(", \"relations\": [");		// begin parametric_objects

				thecomma = "";
				foreach(AXRelation rr in rels)
				{
					sb.Append(thecomma + rr.asJSON());
					thecomma = ", ";
				}
				sb.Append("]");
			}



			sb.Append("}");






			// *** SAVE AS ASSET ***



			// Since we have added the newItem to the live library, 
			// generating this new file will not force a recreation of the library from the raw JSON file.
			File.WriteAllText( filepathname, sb.ToString());







			// THUMBNAIL TO PNG


				
			if (po.is2D())
				Library.cache2DThumbnail(po);
			else
			{
				Thumbnail.BeginRender();
				Thumbnail.renderThumbnail(po, true);
				Thumbnail.EndRender(); 


			}
			//string thumb_filename = System.IO.Path.ChangeExtension(filepathname, ".png");
			string filename 				= System.IO.Path.GetFileNameWithoutExtension(filepathname);

			string prefix = (po.is2D()) ? "zz-AX-2DLib-" : "zz-AX-3DLib-";
			string thumb_filename = System.IO.Path.ChangeExtension(filepathname, ".jpg");
			thumb_filename = thumb_filename.Replace(filename, prefix+filename);



			byte[] bytes = po.thumbnail.EncodeToJPG();
			//File.WriteAllBytes(libraryFolder + "data/"+ po.Name + ".png", bytes);
			File.WriteAllBytes(thumb_filename, bytes);

			//AssetDatabase.Refresh();

			string thumbnailRelativePath = ArchimatixUtils.getRelativeFilePath(thumb_filename);

			//Debug.Log(path); 
			AssetDatabase.ImportAsset(thumbnailRelativePath);
			TextureImporter importer 			= AssetImporter.GetAtPath(thumbnailRelativePath) as TextureImporter;

			if (importer != null)
			{
				importer.textureType=TextureImporterType.GUI;
				importer.maxTextureSize 			= 256;



				#if UNITY_5_5_OR_NEWER
				importer.textureCompression			= TextureImporterCompression.Uncompressed;
				#else
				importer.textureFormat				= TextureImporterFormat.AutomaticTruecolor;
				#endif
			}

			AssetDatabase.WriteImportSettingsIfDirty(thumbnailRelativePath);


			// Save the recent path to prefs
			EditorPrefs.SetString("LastLibraryPath", System.IO.Path.GetDirectoryName(filepathname));


			EditorUtility.DisplayDialog("Archimatix Library", "ParametricObject saved to " + System.IO.Path.GetDirectoryName(thumbnailRelativePath) + " as " + filename, "Great, thanks!");

			Texture2D tex = (Texture2D) AssetDatabase.LoadAssetAtPath(thumbnailRelativePath, typeof(Texture2D));




			LibraryItem newItem = new LibraryItem(po);
			newItem.icon = tex;
			ArchimatixEngine.library.addLibraryItemToList(newItem);
			ArchimatixEngine.library.sortLibraryItems();


			ArchimatixEngine.saveLibrary();




			AssetDatabase.Refresh();

			//Debug.Log("yo 2");
			//ArchimatixEngine.library.readLibraryFromFiles();
			

		}


	}


	 





}
