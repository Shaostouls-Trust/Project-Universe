#pragma warning disable

using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Threading;



using AX;
using AXEditor;

//namespace AXEditor
//{
public class AXPrefabWindow : EditorWindow
{



	GUIStyle backgroundStyle;
	GUIStyle richLabelStyle;
	GUIStyle richButtonStyle;
	GUIStyle iconButtonStyle;
	GUIStyle textLabelStyle;

	public  AXModel model;

	 string prefabPath;
	// string prefabName;

	// string relativePrefabPath;
	 string folderPath;
	 string relativeFolderPath;

	// bool isSaving;
	 int doPrefab;
	int frame_index;


	//int framesPerSecond = 24;

	//float win_width = 530;
	//float win_height = 370;
	//float margin = 3;


	//int meshCount = 0;

	Thread thread;

//	AXNodeGraphEditorWindow editor;

	void OnGUI()
	{
	/*
		
		GUI.DrawTexture( new Rect(margin,margin, this.position.width-margin, this.position.height-margin), EditorGUIUtility.whiteTexture );
		Color prevBackgroundColor = GUI.backgroundColor;

		Color prevGuiColor = GUI.color;
		Color guiColor = GUI.color;
		guiColor.a = .8f;
		GUI.color = guiColor;

		GUI.DrawTexture( new Rect(margin,margin, this.position.width-margin, this.position.height-margin),  editor.prefabWindowBackground);

		GUI.color = prevGuiColor;

		GUI.contentColor = Color.white;

						//if (doPrefab > 0)
		//frame_index = (counter  % frames.Length);
		//counter++;


		frame_index =  Mathf.RoundToInt((float) EditorApplication.timeSinceStartup * framesPerSecond) % editor.prefabWindowFrames.Length ;

		if (richLabelStyle == null)
		{
			
			richLabelStyle = new GUIStyle(GUI.skin.label);
			richLabelStyle.richText = true;
			richLabelStyle.alignment = TextAnchor.LowerLeft;
			richLabelStyle.fontSize = 28;
			richLabelStyle.fixedWidth = win_width;
			richLabelStyle.fixedHeight = 60;
			richLabelStyle.wordWrap = true;

			richButtonStyle = new GUIStyle(GUI.skin.button);
			richButtonStyle.richText = true;
			
			textLabelStyle = new GUIStyle(GUI.skin.label);
			textLabelStyle.richText = true;
			textLabelStyle.wordWrap = true;
			textLabelStyle.alignment = TextAnchor.UpperLeft;
			textLabelStyle.fixedWidth = win_width;
			textLabelStyle.fontSize = 18;

			iconButtonStyle = new GUIStyle(GUI.skin.button);
			iconButtonStyle.normal.background = null;
			iconButtonStyle.imagePosition = ImagePosition.ImageOnly;
			iconButtonStyle.fixedWidth = 128;
			iconButtonStyle.fixedHeight = 128;
		}



		// NAME
		GUILayout.BeginHorizontal();
		GUILayout.Label("<b> "+prefabName+"</b>", richLabelStyle);
		GUILayout.EndHorizontal();

		HR(0, 5);

		GUILayout.BeginHorizontal();
		GUILayout.Label("  to:/"+relativeFolderPath+"", textLabelStyle);
		GUILayout.EndHorizontal();

		GUILayout.Space(100);


		GUI.DrawTexture(new Rect (margin, 100, 128, 128), editor.prefabWindowFrames[frame_index]);

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();

		if (isSaving)
		{
			GUILayout.Label("Creating Prefab...", richLabelStyle);

			GUILayout.Label("Meshes processed: "+meshCount, textLabelStyle);
		}
		else
		{
			if (GUILayout.Button("Create   "+prefabName+".prefab", richButtonStyle, GUILayout.MaxWidth(300), GUILayout.Height(36)))
			{
				isSaving = true;
				doPrefab = 1;

				//ThreadStart threadStart = new ThreadStart(updater);
				//thread = new Thread(threadStart);
				//thread.Start();


			}

			if (GUILayout.Button("Cancel", richButtonStyle, GUILayout.MaxWidth(100), GUILayout.Height(36)))
				this.Close();
		}


		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();



		Repaint();

		GUI.backgroundColor = prevBackgroundColor;
		*/
	}	

	public static void makePrefab(AXModel model, string prefabPath, AXNodeGraphEditorWindow _editor)
	{
		//if (model == null)
		//	return null;

		if (model.generatedGameObjects != null)
		{


			// extrat folder path from path

			//string filename = System.IO.Path.GetFileName (filepath);
			string prefabName = System.IO.Path.GetFileNameWithoutExtension (prefabPath);

			string relativePrefabPath = ArchimatixUtils.getRelativeFilePath (prefabPath);

			string folderPath = System.IO.Path.GetDirectoryName (prefabPath);
			string relativeFolderPath = ArchimatixUtils.getRelativeFilePath (folderPath);

			// if no directory selected in dialog, then relativeFolderPath is empty
			if (String.IsNullOrEmpty (relativeFolderPath))
				relativeFolderPath = "Assets";

			Debug.Log (folderPath + " :: " + relativeFolderPath + " :: " + prefabName);

			model.build();

			GameObject stampedGO = model.stamp ();

			// Only save unique, sharedMeshes to the AssetsDatabase
			List<Mesh> meshesToSave = new List<Mesh> ();
			int meshCount = 0;

			Transform[] transforms = stampedGO.GetComponentsInChildren<Transform> ();
			foreach(Transform transform in transforms)
			{
				MeshFilter meshFilter = transform.gameObject.GetComponent<MeshFilter> ();

				if (meshFilter != null)
				{
					Mesh mesh = meshFilter.sharedMesh;

					if (mesh != null && ! meshesToSave.Contains (mesh)) {

						// There was a crash with SkinnedMeshRenderer from PhysicsRope.
						// For now punt and don't generate secondaries if no regular MeshRenderer
						if (transform.gameObject.GetComponent<MeshRenderer>() != null)
							Unwrapping.GenerateSecondaryUVSet (mesh);

						meshesToSave.Add (mesh);

						meshCount++;
					}
					GameObjectUtility.SetStaticEditorFlags(transform.gameObject, StaticEditorFlags.LightmapStatic);
				}
			}

			AssetDatabase.DeleteAsset (relativePrefabPath);


			// [not sure why this is needed here to correct the filepath, but not in the Library Save dalog...]
			relativePrefabPath = relativePrefabPath.Replace('\\', '/');



			var prefab = PrefabUtility.CreateEmptyPrefab(relativePrefabPath);

			int i = 0;
			foreach (Mesh mesh in meshesToSave)
			/*
			{
				if (string.IsNullOrEmpty(mesh.name))
					mesh.name = prefabName+"_"+(i++);
				AssetDatabase.DeleteAsset (relativePrefabPath+"/"+mesh.name);
				AssetDatabase.AddObjectToAsset(mesh, relativePrefabPath);
			}
			*/
			{ // Compliments of Enoch 
                if (string.IsNullOrEmpty(mesh.name))
                    mesh.name = prefabName+"_"+(i++);
                var path = AssetDatabase.GetAssetPath(mesh);
               
               if (string.IsNullOrEmpty(path))
               {
                   AssetDatabase.AddObjectToAsset(mesh, relativePrefabPath);
               }
            }



			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			PrefabUtility.ReplacePrefab(stampedGO, prefab, ReplacePrefabOptions.ConnectToPrefab);

			stampedGO.name = prefabName;
			//Selection.activeGameObject = stampedGO;

			//buildStatus = BuildStatus.Generated;



			//Close();


		}


	}




	void HR(int prevSpace, int nextSpace)
	{
		GUILayout.Space(prevSpace);
		Rect r = GUILayoutUtility.GetRect(Screen.width, 2);
		Color og = GUI.backgroundColor;
		GUI.backgroundColor = new Color(.7f, .5f, .6f, .3f);
		GUI.Box(r, "");
		GUI.backgroundColor = og;
		GUILayout.Space(nextSpace);
	}


}

//}