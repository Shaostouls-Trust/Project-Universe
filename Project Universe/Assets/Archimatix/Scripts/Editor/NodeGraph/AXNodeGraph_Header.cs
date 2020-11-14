using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

using AXGeometry;

using AX;
namespace AXEditor
{

	public class AXNodeGraph_Header  {

		
		
		// return the height of this gui area
		public static void OnGUI(Rect headerRect, AXNodeGraphEditorWindow editor) 
		{
			
					
			Event e = Event.current;

			AXModel model = editor.model;

			// DO HEADER MENU BAR -- MODEL MENU
				 
		  
			GUILayout.BeginArea(headerRect);
			GUILayout.BeginHorizontal();

			GUIStyle labelstyle = new GUIStyle(GUI.skin.label);
			labelstyle.alignment = TextAnchor.LowerLeft;

			labelstyle.fixedWidth = 150;


			GUILayout.Space(5);


			if(GUILayout.Button ("2D"))
			{
				ArchimatixEngine.openLibrary2D();
			}
			if(GUILayout.Button ("3D"))
			{
				ArchimatixEngine.openLibrary3D();
			}

			 

			// NEW MODEL
			GUILayout.Space(10);



			if(GUILayout.Button ("▼"))
			{

				AXModel[] allModels = ArchimatixUtils.getAllModels();

					
				GenericMenu menu = new GenericMenu ();

				//menu.AddSeparator("Library ...");


				if (allModels != null && allModels.Length > 0)
				{
					for (int i = 0; i < allModels.Length; i++) {
						AXModel m = allModels [i];
						menu.AddItem (new GUIContent (m.gameObject.name), false, () =>  {
							
							Selection.activeGameObject = m.gameObject;
							ArchimatixEngine.currentModel = m;


						});
					}
				}


				menu.AddSeparator("");

				menu.AddItem(new GUIContent("New Model"), false, () => {
					AXEditorUtilities.createNewModel();
				});

				menu.ShowAsContext ();
				

			}




			Color bgc = GUI.backgroundColor;
			GUI.backgroundColor = Color.clear;







			if (model != null)			
			{
				// SELECTED MODEL
				if(GUILayout.Button (model.name))
				{
					//model.currentWorkingGroupPO = null;
					model.clearCurrentWorkingGroup();

					model.selectAllVisibleInGroup(null);

					float framePadding = 50;

					Rect allRect = AXUtilities.getBoundaryRectFromPOs(model.selectedPOs);
					allRect.x 		-= framePadding;
					allRect.y 		-= framePadding;
					allRect.width 	+= framePadding*2;
					allRect.height 	+= framePadding*2;



					AXNodeGraphEditorWindow.zoomToRectIfOpen(allRect);
				}



				if (model.currentWorkingGroupPO != null)
				{
					if (model.currentWorkingGroupPO.grouper != null)
					{
						// Make breadcrumb trail
						List<AXParametricObject> crumbs = new List<AXParametricObject>();
						AXParametricObject cursor = model.currentWorkingGroupPO;


						while (cursor.grouper != null)
						{						
							crumbs.Add(cursor.grouper);
							cursor = cursor.grouper;
						}
						crumbs.Reverse();

						// model button frames 



						for(int i=0; i<crumbs.Count; i++)
						{
							if (GUILayout.Button("> " + crumbs[i].Name))
							{
								model.currentWorkingGroupPO = crumbs[i];
								Rect grouperCanvasRect = model.currentWorkingGroupPO.getBoundsRect();
								editor.zoomToRect(grouperCanvasRect);

							}
						}	
					}
					GUILayout.Button("> "+model.currentWorkingGroupPO.Name);
				}
			}
			GUILayout.FlexibleSpace();




			if (model != null)
			{
				Color buildBG = Color.Lerp(Color.cyan, Color.white, .8f);
				string buildLabel = "Rebuild";



				if (model.buildStatus == AXModel.BuildStatus.Generated)
				{
					buildBG		= Color.red;
					buildLabel	= "Build";
				}


		
				GUI.backgroundColor = buildBG;
				if (GUILayout.Button( buildLabel))
					model.build ();

				GUI.backgroundColor = Color.cyan;


				if (model.generatedGameObjects != null && model.generatedGameObjects.transform.childCount == 0)
				{	
					GUI.enabled = false;
				}

				if (GUILayout.Button( "Stamp"))
				{
					model.stamp ();

				}

				if (GUILayout.Button("Prefab"))
				{
					string startDir = Application.dataPath;

					string path = EditorUtility.SaveFilePanel(
						"Save Prefab",
						startDir,
						(""+model.name),
						"prefab");

					if (! string.IsNullOrEmpty(path))
					{

						AXPrefabWindow.makePrefab(model, path, editor);

					}
				}
				GUI.enabled = true;

				GUILayout.Space(4);

			}


			GUILayout.EndHorizontal();
			GUILayout.EndArea();

			GUI.backgroundColor = bgc;



		}

	}

}