using UnityEngine;
using UnityEditor;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using AX.SimpleJSON;



using AXGeometry;

using AX;
using AX.Generators;

using AX.GeneratorHandlers;
using AXEditor;

using Axis = AXGeometry.Axis;



    public class AXEditorUtilities {

    	public static string 	lastCommand;
    	public static long 		lastCopyCommandTime;
    	public static long 		lastPasteCommandTime;
    	
    	public static Event lastEvent;

    	 

    	public static Texture2D textureFromSprite(Sprite sprite)
         {
             if(sprite.rect.width != sprite.texture.width){
    			Texture2D newText = new Texture2D((int)sprite.textureRect.width,(int)sprite.textureRect.height);
                 Color[] newColors = sprite.texture.GetPixels((int)sprite.textureRect.x, 
                                                              (int)sprite.textureRect.y, 
                                                              (int)sprite.textureRect.width, 
                                                              (int)sprite.textureRect.height );
                 newText.SetPixels(newColors);
                 newText.Apply();
                 return newText;
             } else
                 return sprite.texture;
         }
    	 
    	public static void loaduiIcons()
    	{
    //		if (ArchimatixEngine.uiIcons == null )
    //			return;

    		ArchimatixEngine.uiIcons = new Dictionary<string, Texture2D>();

    		// load CurvePointIcons
    		UnityEngine.Object[] spriteData = AssetDatabase.LoadAllAssetRepresentationsAtPath(ArchimatixEngine.ArchimatixAssetPath+"/ui/GeneralIcons/zz-AXIcons-CurvePointEditing.png");
    	
    		ArchimatixEngine.uiIcons.Add("AddPoint", textureFromSprite((Sprite) spriteData[0]));
    		ArchimatixEngine.uiIcons.Add("AddPointOff", textureFromSprite((Sprite) spriteData[1]));
    		ArchimatixEngine.uiIcons.Add("DeletePoint", textureFromSprite((Sprite) spriteData[2]));

    		ArchimatixEngine.uiIcons.Add("PointOn", textureFromSprite((Sprite) spriteData[4]));
    		ArchimatixEngine.uiIcons.Add("PointOff", textureFromSprite((Sprite) spriteData[5]));


    		ArchimatixEngine.uiIcons.Add("BezierPointOn", textureFromSprite((Sprite) spriteData[6]));
    		ArchimatixEngine.uiIcons.Add("BezierPointOff", textureFromSprite((Sprite) spriteData[7]));


    		ArchimatixEngine.uiIcons.Add("BrokenBezierPointOn", textureFromSprite((Sprite) spriteData[8]));
    		ArchimatixEngine.uiIcons.Add("BrokenBezierPointOff", textureFromSprite((Sprite) spriteData[9]));

    		ArchimatixEngine.uiIcons.Add("GridSnapOff", textureFromSprite((Sprite) spriteData[10]));
    		ArchimatixEngine.uiIcons.Add("GridSnapOn", textureFromSprite((Sprite) spriteData[11]));

    	}
    	public static void loadNodeIcons()
    	{
    		if (ArchimatixEngine.nodeIcons != null)
    			return;

            if (Application.isPlaying)
                return;

    		ArchimatixEngine.nodeIcons = new Dictionary<string, Texture2D>();


    		// The ? wildcard is used here to make sure that .jpg.meta files are not being returned
    		String[] nodeIconPaths = Directory.GetFiles(ArchimatixEngine.ArchimatixAssetPath+"/ui/NodeIcons/", "*.jp?");

    		string filename;
    		string nodeIconName;
    		 
    		for(int i=0; i<nodeIconPaths.Length; i++)
    		{
    			//Debug.Log(nodeIconPaths[i]);
    			filename = System.IO.Path.GetFileName(nodeIconPaths[i]);
            
            if (! string.IsNullOrEmpty(filename) && filename.Length > 10)
    			{
    				// strip extension
    				nodeIconName =  System.IO.Path.GetFileNameWithoutExtension(nodeIconPaths[i]);

    				if (! string.IsNullOrEmpty(filename) && nodeIconName.Length > 10)
    				{
    					//strip prefix of "zz-AXNode-"
    					nodeIconName 	= nodeIconName.Substring(10);
                   // Debug.Log(nodeIconName);
    					ArchimatixEngine.nodeIcons.Add(nodeIconName, (Texture2D) AssetDatabase.LoadAssetAtPath(ArchimatixEngine.ArchimatixAssetPath+"/ui/NodeIcons/"+filename, typeof(Texture2D)));
    				}
    			}
    		}
    		  






    //		ArchimatixEngine.nodeIcons.Add("Point", (Texture2D) AssetDatabase.LoadAssetAtPath(ArchimatixEngine.ArchimatixAssetPath+"/ui/GeneralIcons/zz-AXIcons-CurvePointEditing.png", typeof(Texture2D)));
    //		Debug.Log(ArchimatixEngine.nodeIcons.Count);
    //		Debug.Log(ArchimatixEngine.nodeIcons["Point"]);
    //		//ArchimatixEngine.nodeIcons.Add("Point", (Texture2D) AssetDatabase.LoadAssetAtPath(ArchimatixEngine.ArchimatixAssetPath+"/ui/GeneralIcons/zz-AXIcons-CurvePointEditing_2", typeof(Texture2D)));
    		 
    		// custom nodes with name of form zz_AXNode-typename
    		Archimatix.discoverThirdPartyNodes();

    		foreach (String name in Archimatix.customNodeNames)   
    		{
    			string typeName = name;

    			DirectoryInfo info = new DirectoryInfo(Application.dataPath);

    			FileInfo[] files = info.GetFiles("zz_AXNode-"+ typeName + ".jpg", SearchOption.AllDirectories);

    			if (files != null && files.Length > 0)
    			{
    				string iconRelPath = ArchimatixUtils.getRelativeFilePath(files[0].ToString());
    				//Debug.Log("path: " + typeName + " --- " + iconRelPath);

    				if (! ArchimatixEngine.nodeIcons.ContainsKey(typeName))
    					ArchimatixEngine.nodeIcons.Add(typeName, (Texture2D) AssetDatabase.LoadAssetAtPath(iconRelPath, typeof(Texture2D)));
    			}
    		}
    	}



    	public static void clearFocus()
    	{
    		GUI.FocusControl("dummy_label");
    	}





    	public static void processEventCommand(Event e, AXModel model)
    	{


    		if (e.type == EventType.ValidateCommand)
    			e.Use ();
    		
    		//Debug.Log("-> processEventCommand 1: PROCESS COMMAND "+e.commandName);
    		
    		
    		var view = SceneView.lastActiveSceneView;
    		
    		string focusedControlName = GUI.GetNameOfFocusedControl();

    		if (model != null)
    		{
    			// intercept this command and use for po's
    			switch (e.commandName)
    			{
    				
    			case "UndoRedoPerformed":
    				//Debug.Log ("UndoRedoPerformed");
    				model.cleanGraph();
    				model.autobuild();

    				for (int i=0; i<model.selectedPOs.Count; i++)
    					model.selectedPOs[i].generator.adjustWorldMatrices();

    				// SCENEVIEW
    				if (SceneView.lastActiveSceneView != null)
    					SceneView.lastActiveSceneView.Repaint();


    				//model.setRenderMode( AXModel.RenderMode.GameObjects );
    				model.renderThumbnails("AXGeometry.Utilities::processEventCommand::UndoRedoPerformed");


    				if(e.type != EventType.Repaint && e.type != EventType.Layout) 
    					e.Use(); 
    				break;
    			

    			case "SelectAll":

    				Debug.Log("SelectAll");
    				model.selectAll();


    				e.Use();
    				break; 

    			case "Copy": 
    				//Debug.Log ("COPY ..."+ GUI.GetNameOfFocusedControl());

    				//Debug.Log ("buf: " + EditorGUIUtility.systemCopyBuffer);

    				//Debug.Log("-"+focusedControlName+"-");
    				//Debug.Log((string.IsNullOrEmpty(focusedControlName) || !focusedControlName.Contains("_Text_")));
    				if (string.IsNullOrEmpty(focusedControlName) || !focusedControlName.Contains("_Text_"))
    				{
    					if (model.selectedPOs.Count > 0)
    					{  
    						EditorGUIUtility.systemCopyBuffer = LibraryEditor.poWithSubNodes_2_JSON(model.selectedPOs[0], true);
    					} 
    					if(e.type != EventType.Repaint && e.type != EventType.Layout) 
    						e.Use();
    				} 

    				break;
    				 
    			case "Paste":
    				//Debug.Log ("PASTE");
    				//Debug.Log(GUI.GetNameOfFocusedControl());

    				//if (string.IsNullOrEmpty(GUI.GetNameOfFocusedControl()))
    				//{
    				Undo.RegisterCompleteObjectUndo (model, "Paste");
    				string focusedControlNameBeforePaste = GUI.GetNameOfFocusedControl();
    				if (string.IsNullOrEmpty(focusedControlNameBeforePaste) || !focusedControlNameBeforePaste.Contains("_Text_"))
    				{

    					

    					//model.deselectAll();

    					Library.pasteParametricObjectFromString(EditorGUIUtility.systemCopyBuffer);
    					model.autobuild();

    					if(e.type != EventType.Repaint && e.type != EventType.Layout) 
    						e.Use();
    								
    				}
    				model.autobuild();

    				break;  
    				
    			case "Duplicate":
    				Undo.RegisterCompleteObjectUndo (model, "Duplicate");
    				
    				//Debug.Log ("Duplicate Command");
    				if (model.selectedPOs.Count > 0)
    				{
    					AXParametricObject selectedPO = model.selectedPOs[0];

    						instancePO(selectedPO);
    				}
    				
    				model.autobuild();

    				if(e.type != EventType.Repaint && e.type != EventType.Layout) 
    					e.Use();
    				break;
    				
    			case "Cut":
    				Undo.RegisterCompleteObjectUndo (model, "Cut");
    				//EditorGUIUtility.systemCopyBuffer = JSONSerializersAX.allSelectedPOsAsJson(model);


    				if (string.IsNullOrEmpty(focusedControlName) || !focusedControlName.Contains("_Text_"))
    				{

    					if (model.selectedPOs.Count > 0)
    						EditorGUIUtility.systemCopyBuffer = LibraryEditor.poWithSubNodes_2_JSON(model.selectedPOs[0], true);


    					if (e.shift)
    						model.deleteSelectedPOsAndInputs();
    					else
    						model.deleteSelectedPOs();

    					model.autobuild();
    					if(e.type != EventType.Repaint && e.type != EventType.Layout) 
    					e.Use();				

    				}




    				break;
    				
    			case "SoftDelete":
    			case "Delete":
    				
    				//Debug.Log("DELETE");
    				// see if it is a selected point on a curve
    				//if (ArchimatixEngine

    				/*
    				FreeCurve selectedFreeCurve = null;

    				if ( model.selectedPOs != null && model.selectedPOs.Count == 1 &&  model.selectedPOs[0] != null && model.selectedPOs[0].generator != null &&  model.selectedPOs[0].generator is FreeCurve)
    				{
    					selectedFreeCurve = (FreeCurve) model.selectedPOs[0].generator;
    				}

    				if (selectedFreeCurve != null && selectedFreeCurve.selectedIndices != null  &&  selectedFreeCurve.selectedIndices.Count > 0)
    				{
    					// delete points
    					selectedFreeCurve.deleteSelected();
    				}
    				*/

    				// SELECTED POINTS TO DELETE?
    				//Debug.Log("focusedControlName="+focusedControlName);
    				if (string.IsNullOrEmpty(focusedControlName) || !focusedControlName.Contains("_Text_"))
    				{

    					bool foundSelectedPoints = false;

    					if (model.activeFreeCurves.Count > 0)
    					{
    						for (int i=0; i<model.activeFreeCurves.Count; i++)
    						{
    							FreeCurve gener = (FreeCurve) model.activeFreeCurves[i].generator;


    							//Debug.Log("gener.hasSelectedPoints()="+gener.hasSelectedPoints());
    							if (gener.hasSelectedPoints())
    							{
    								foundSelectedPoints = true;
    								gener.DeleteSelected();
    							}
    						}
    					}
    					//Debug.Log("foundSelectedPoints="+foundSelectedPoints);

    					if (foundSelectedPoints)
    					{
    						ArchimatixEngine.mouseIsDownOnHandle = false;
    					}
    					else if (e.shift) {
    						Undo.RegisterCompleteObjectUndo (model, "Delete Nodes");
    						model.deleteSelectedPOsAndInputs ();

    					} 
    					// [S.Darkwell: changed else to this else if to fix bug: "Fix pressing Delete key without Node selected still registers undo event" https://archimatixbeta.slack.com/files/s.darkwell/F1DJRQ3LL/fix_pressing_delete_key_without_node_selected_still_registers_undo_event.cs  - 2016.06.02]
    					else if (model.selectedPOs.Count > 0)
    					{

    						Undo.RegisterCompleteObjectUndo (model, "Delete Node");
    						model.deleteSelectedPOs ();

    					}
    					else if (model.selectedParameterInputRelation != null)
    					{
    						Undo.RegisterCompleteObjectUndo (model, "Delete Dependancy");
    						model.selectedParameterInputRelation.makeIndependent();
    						model.selectedParameterInputRelation = null;
    					} else if (model.selectedRelationInGraph != null)
    					{
    						Undo.RegisterCompleteObjectUndo (model, "Delete Relation");
    						model.unrelate(model.selectedRelationInGraph);
    						model.selectedRelationInGraph= null;
    					}

    					//Debug.Log("*********************************** DELETE");
    					model.remapMaterialTools();

    					model.autobuild();

    					//Debug.Log("caching here G");
    					model.renderThumbnails();
    					e.Use();

    				}



    				break;
    				
    				
    				
    			case "FrameSelected":
    				
    				if(view != null)
    				{
    					float framePadding = 400;


    					if (model.selectedPOs == null || model.selectedPOs.Count == 0)
    					{
    						//model.selectAll();
    						model.selectAllVisibleInGroup(model.currentWorkingGroupPO);


    						Rect allRect = AXUtilities.getBoundaryRectFromPOs(model.selectedPOs);
    						allRect.x 		-= framePadding;
    						allRect.y 		-= framePadding;
    						allRect.width 	+= framePadding*2;
    						allRect.height 	+= framePadding*2;

    						AXNodeGraphEditorWindow.zoomToRectIfOpen(allRect);
    						model.deselectAll();
    					}
    					else if (model.selectedPOs.Count > 1)
    					{
    						Rect allRect = AXUtilities.getBoundaryRectFromPOs(model.selectedPOs);
    						allRect.x 		-= framePadding;
    						allRect.y 		-= framePadding;
    						allRect.width 	+= framePadding*2;
    						allRect.height 	+= framePadding*2;

    						AXNodeGraphEditorWindow.zoomToRectIfOpen(allRect);
    					}
    					else
    					{
    						//frame first po
    						AXParametricObject currPO = model.cycleSelectedPO;// model.selectedPOs[0];

    						if (currPO == null)
    							currPO = model.mostRecentlySelectedPO;

    						if (currPO == null && model.selectedPOs != null && model.selectedPOs.Count > 0)
    							currPO = model.selectedPOs[0];

    						if (currPO == null)
    							currPO = model.selectFirstHeadPO(); 

    						if (currPO != null)
    						{
    							Matrix4x4 m = model.transform.localToWorldMatrix * currPO.worldDisplayMatrix;// * currPO.getLocalMatrix();

    							if (m.isIdentity)
    								m = currPO.generator.localMatrix;

    							Vector3 position = m.MultiplyPoint(currPO.bounds.center);


    							if (currPO.bounds.size.magnitude > .005 && currPO.bounds.size.magnitude < 10000)
    								view.LookAt(position, view.camera.transform.rotation, currPO.bounds.size.magnitude*1.01f);
    							else {
    								//Debug.Log("FrameSelected - select ParametricObjectObject bounds not good: "+currPO.bounds.size.magnitude);
    							}

    							//if (currPO.grouper != null )
    							//{
    								AXNodeGraphEditorWindow.displayGroupIfOpen(currPO.grouper);
    							//}
    							//model.beginPanningToPoint(currPO.rect.center);
    							AXNodeGraphEditorWindow.zoomToRectIfOpen(currPO.rect);

    						}
    					}
    					if(e.type != EventType.Repaint && e.type != EventType.Layout) 
    						e.Use();
    				}
    				
    				break;
    				
    			}
    			

    		}

    		// EDITOR WINDOW

    		ArchimatixEngine.repaintGraphEditorIfExistsAndOpen();

    		// SCENEVIEW
    		if (view != null)
    			view.Repaint();
    		
    		
    		lastCommand = e.commandName;
    		
    		
    	}




    	// MODEL STATIC FLAGS


    	public static void makeBatchingStatic(AXModel model)
    	{
    		Debug.Log ("makeBatchingStatic");
    		if (model.generatedGameObjects != null)
    		{
    			Transform[] transforms = model.generatedGameObjects.GetComponentsInChildren<Transform> ();
    			foreach (Transform transform in transforms) 
    			{
    				if (transform.GetComponent<Rigidbody> () == null)
    					GameObjectUtility.SetStaticEditorFlags (transform.gameObject, StaticEditorFlags.BatchingStatic);
    			}				
    		}

    	} 


    	// RECRUSIVE SET ALL PO FLAGS
    	public static void setFlagOfPO_toUpstream(AXParametricObject po, AXStaticEditorFlags flag)
    	{
    		po.axStaticEditorFlags = po.axStaticEditorFlags | flag;

    		foreach(AXParameter input in po.getAllInputMeshParameters())
    		{
    			if (input.DependsOn != null && input.DependsOn.parametricObject.is3D())
    				setFlagOfPO_toUpstream(input.DependsOn.parametricObject, flag);

    		}

    	}

    	public static void makeLightmapStatic(AXModel model)
    	{
    		if (model.generatedGameObjects != null)
    		{
    			Transform[] transforms = model.generatedGameObjects.GetComponentsInChildren<Transform> ();
    			foreach (Transform transform in transforms) 
    			{	
    				if (transform.GetComponent<Rigidbody> () == null)
                    {
#if UNITY_2019_2_OR_NEWER
                    GameObjectUtility.SetStaticEditorFlags(transform.gameObject, StaticEditorFlags.ContributeGI);

#else
                     GameObjectUtility.SetStaticEditorFlags(transform.gameObject, StaticEditorFlags.LightmapStatic);
                    
#endif
                }

            }
    			model.buildStatus = AXModel.BuildStatus.Lightmapped;
    		}

    	}

    	public static void makeLightMapUVs(AXModel model)
    	{

    	//Debug.Log("Making secondary");
    		if (model.generatedGameObjects != null)
    		{
    			List<Mesh> meshesProcessed = new List<Mesh> ();




    			Transform[] transforms = model.generatedGameObjects.GetComponentsInChildren<Transform> ();
    			foreach(Transform transform in transforms)
    			{
    				if (transform.GetComponent<Rigidbody> () == null) {

    					AXGameObject axgo = transform.gameObject.GetComponent<AXGameObject>();

    					if (axgo != null && axgo.parametricObject != null && ( (axgo.parametricObject.axStaticEditorFlags & AXStaticEditorFlags.LightmapStatic) ==  AXStaticEditorFlags.LightmapStatic))
    					{
    						//Debug.Log(transform.gameObject.name + " gets secondary");
    						MeshFilter meshFilter = transform.gameObject.GetComponent<MeshFilter> ();

    						if (meshFilter != null) {
    							Mesh mesh = meshFilter.sharedMesh;

    							if (!meshesProcessed.Contains (mesh)) {
    								Debug.Log("doin' it!");


    								Unwrapping.GenerateSecondaryUVSet (mesh);
    								//Mesh tmpMesh = mesh;

    								//tmpMesh.uv2 = mesh.uv2;

    								meshesProcessed.Add (mesh);

    							}
    							//GameObjectUtility.SetStaticEditorFlags (transform.gameObject, StaticEditorFlags.LightmapStatic);

    						}

    					}

    				}
    			}
    			model.buildStatus = AXModel.BuildStatus.lightmapUVs;
    		}


    	}



    	public static Axis getDefaultAxisBasedBasedOnSceneViewOrientation(SceneView sceneView)
    	{


    		Axis axis = Axis.Y;


    		if (sceneView == null)
    			return axis;

    		if (sceneView.in2DMode)
    			axis = Axis.NZ;

    		else 
    		{

    			// find closest axis
    			float angle = 0.0F;
    			Vector3 angAxis = Vector3.zero;

    			sceneView.rotation.ToAngleAxis(out angle, out angAxis);

    //			Debug.Log(angle + " *** " + angAxis );
    //			Debug.Log(sceneView.rotation.eulerAngles);


    			if (sceneView.orthographic)
    			{
    				if (angAxis == new Vector3(1,0,0))
    				{
    					if (Mathf.Approximately(angle, 270f))
    						axis = Axis.NY;
    					else if (Mathf.Approximately(angle, 180f))
    						axis = Axis.Z;
    					else if (Mathf.Approximately(angle, 90f))
    						axis = Axis.Y;
    					else if (Mathf.Approximately(angle, 0f))
    						axis = Axis.NZ;



    				}
    				else if (angAxis == new Vector3(-1,0,0))
    				{
    					if (Mathf.Approximately(angle, 270f))
    						axis = Axis.Y;
    					else if (Mathf.Approximately(angle, 180f))
    						axis = Axis.Z;
    					else if (Mathf.Approximately(angle, 90f))
    						axis = Axis.NY;
    					else if (Mathf.Approximately(angle, 0f))
    						axis = Axis.NZ;

    				}

    				else if (angAxis == new Vector3(0,-1,0))
    				{
    					if (Mathf.Approximately(angle, 270f))
    						axis = Axis.NX;
    					else if (Mathf.Approximately(angle, 180f))
    						axis = Axis.Z;
    					else if (Mathf.Approximately(angle, 90f))
    						axis = Axis.X;
    					else if (Mathf.Approximately(angle, 0f))
    						axis = Axis.NZ;
    				} 

    				else if (angAxis == new Vector3(0,1,0))
    				{
    					if (Mathf.Approximately(angle, 270f))
    						axis = Axis.X;
    					else if (Mathf.Approximately(angle, 180f))
    						axis = Axis.Z;
    					else if (Mathf.Approximately(angle, 90f))
    						axis = Axis.NX;
    					else if (Mathf.Approximately(angle, 0f))
    						axis = Axis.NZ;

    				}
    			}

    			else //
    			{


    				if (angAxis.y <=.9f)
    				{
    					axis = Axis.Y;
    				}
    				else
    				{
    					axis = Axis.X;
    				}

    				//					float angleX = Quaternion.Angle(sceneView.rotation, Quaternion.Euler(Vector3.forward));// Vector3.Angle(Vector3.zero, normal);
    				//					float angleY = Quaternion.Angle(sceneView.rotation, Quaternion.Euler(Vector3.up));
    				//					float angleZ = Quaternion.Angle (sceneView.rotation, Quaternion.Euler (Vector3.left));
    				//
    				//					
    				//					Debug.Log(Quaternion.Euler(0, 90, 90));
    				//					Debug.Log(Quaternion.Euler(Vector3.up));
    				//					Debug.Log(Quaternion.Euler(Vector3.left));
    				//
    				//					Debug.Log(angleX + " " +angleY + " " +angleZ );
    				//
    				//
    				//					if (sceneView.rotation == Quaternion.Euler(90,0,0))
    				//						head_po.setAxis(Axis.Y);


    				////					else if if (sceneView.rotation == Quaternion.Euler(90,0,0))
    				//						head_po.setAxis(Axis.Y);
    				//					Debug.Log(sceneView.rotation);
    				//					Debug.Log(Quaternion.Euler(90,90,-90));
    			}
    		}


    		return axis;

    	}





    	
    	public static AXParametricObject duplicatePO(AXParametricObject po)
    	{
    		// copy
    		EditorGUIUtility.systemCopyBuffer = LibraryEditor.poWithSubNodes_2_JSON(po, true);
    		
    		// paste
    		//LibraryEditor.pasteFromJSONstringToSelectedModel(EditorGUIUtility.systemCopyBuffer);

    		return Library.pasteParametricObjectFromString(EditorGUIUtility.systemCopyBuffer);
    	}






    	// INSTANCE
    	public static AXParametricObject instancePO(AXParametricObject po)
    	{
    		// make an instance

    		AXParametricObject src_po = po;


    		if (po.is2D())
    		{

    			AXParameter output_p = po.generator.P_Output;
    			if (output_p == null && po.generator is ShapeMerger)
    			{

    				ShapeMerger gener = (ShapeMerger) po.generator;



    				output_p = gener.S_InputShape.getSelectedOutputParameter();
    			}

    			if (output_p != null)
    			{
    				
    				AXParametricObject instance_po = AXEditorUtilities.addNodeToCurrentModel("Instance2D", true, po);

    				AXParameter in_p = po.getParameter ("Input Shape");

    				if (in_p == null)
    					src_po = po;
    				else if (in_p.DependsOn != null)   
    					src_po = in_p.DependsOn.parametricObject;

    				AXParameter out_p = src_po.getParameter ("Output Shape");

    				AXParameter inst_inP = instance_po.getParameter("Input Shape");

    				inst_inP.makeDependentOn(out_p);

    				inst_inP.shapeState = out_p.shapeState;

    				AXParameter inst_outP = instance_po.getParameter("Output Shape");
    				inst_outP.shapeState = out_p.shapeState;

    				return instance_po;
    			}

    		}
    		else
    		{
    			if (src_po.generator is IReplica)
    			{
    				// get next PO from downstream
    				AXParameter in_p = po.getParameter ("Input Mesh");

    				if (in_p.DependsOn != null)   
    					src_po = in_p.DependsOn.parametricObject;

    				
    				
    			}
    			
    			AXParameter out_p = src_po.getParameter ("Output Mesh", "Output");
    			
    			if (out_p != null)
    			{
    				AXParametricObject instance_po = AXEditorUtilities.addNodeToCurrentModel("Instance", true, src_po);

    					
    				instance_po.getParameter("Input Mesh").makeDependentOn(out_p);



    				return instance_po;
    			}


    		}
    		return null;

    	}


    	// REPLICATE
    	public static void replicatePO(AXParametricObject po)
    	{
    		// make an instance
    		AXParametricObject src_po = po;
    		
    		if (src_po.generator is IReplica)
    		{
    			// get next PO from downstream
    			AXParameter in_p = po.getParameter ("Input Mesh");
    			if (in_p.DependsOn != null)   
    				src_po = in_p.DependsOn.parametricObject;
    		}
    		
    		AXParameter out_p = src_po.getParameter ("Output Mesh");
    		
    		if (out_p != null)
    			AXEditorUtilities.addNodeToCurrentModel("Replicant").getParameter("Input Mesh").makeDependentOn(out_p);
    	}
    	
    	
    	public static void copySelectedPO(AXModel model)
    	{
    	
    	
    	}
    	
    	public static void processEventCommandKeyDown(Event e, AXModel model)
    	{
    		

    		switch(e.keyCode)
    		{
    		case KeyCode.L:
    			if (model.selectedPOs != null && model.selectedPOs.Count == 1)
    			{
    				LibraryEditor.doSave_MenuItem(model.selectedPOs[0]);

    				if(e.type != EventType.Repaint && e.type != EventType.Layout) 
    					e.Use();
    			}
    			break;
    		case KeyCode.V:
    			// this paste waste uncaught by EventType.ExecuteCommand or EventType.ValidateCommand
    			// beacuse alt or command were held down
    			Undo.RegisterCompleteObjectUndo (model, "Paste");
    			
    			if (e.shift)
    			{
    				if (e.alt)
    					model.duplicateSelectedPOs();
    				else
    					model.replicateSelectedPOs();
    				
    				model.isAltered(6);


    				if(e.type != EventType.Repaint && e.type != EventType.Layout) 
    					e.Use();
    			}
    			
    			break;
    			
    		case KeyCode.D:
    			Undo.RegisterCompleteObjectUndo (model, "Duplicate");
    			//Debug.Log ("DUPLICATE.....KeyCode.D");
    			if (model.selectedPOs.Count > 0)
    			{
    				AXParametricObject selectedPO = model.selectedPOs[0];
    				if (e.shift)
    					replicatePO(selectedPO);
    				else if (e.alt)
    					duplicatePO(selectedPO);
    				else
    				{
    					//Debug.Log ("instancePO");
    					instancePO(selectedPO);
    				}
    			}
    			
    			// "Duplicate" is caught by the validateCommand and processed here in processEventCommand()
    			
    			model.isAltered(7);

    			if(e.type != EventType.Repaint && e.type != EventType.Layout) 
    				e.Use();
    			break;
    			
    		case KeyCode.Backspace:

    			string focusedControlName = GUI.GetNameOfFocusedControl();


    			if (string.IsNullOrEmpty(focusedControlName) || !focusedControlName.Contains("_Text_"))
    			{
    			 
    				Undo.RegisterCompleteObjectUndo (model, "Delete");
    				
    				if (e.shift)
    					model.deleteSelectedPOsAndInputs();
    				else
    					model.deleteSelectedPOs();
    				
    				model.isAltered(8);
    			e.Use();

    			}

    			break;


    		}

    		// Update EditorWindow
    		ArchimatixEngine.repaintGraphEditorIfExistsAndOpen();

    		
    		
    	}
    	
    	public static bool isSelected(GameObject modelGO)
    	{
    		return (ArchimatixEngine.currentModel == modelGO);
    	}
    	
    	

    	public static GameObject createNewModel(string m_name = null)
    	{



    		GameObject 	modelGO = new GameObject();
    		AXModel 	model 	= modelGO.AddComponent<AXModel>();

    		ArchimatixEngine.currentModel = model;



    		if (String.IsNullOrEmpty(m_name))
    		{

    			// for auto naming, get highest count...
    			// (can't use static variable for this, since it is cleared when scene reloads).
    			int highestnum = 0;
    			AXModel[] axModels =  GameObject.FindObjectsOfType(typeof(AXModel)) as AXModel[];
    			foreach (AXModel m in axModels)
    			{
    				
    				String[] numbers = Regex.Split(m.name, @"\D+"); 
    				if (numbers != null && numbers.Length>0)
    				{
    					string numString = numbers[numbers.Length-1];
    					if (!string.IsNullOrEmpty(numString))
    					{
    						int num = int.Parse(numString);
    						
    						if (num > highestnum)
    							highestnum = num;
    					}
    				}
    				
    			}
    			highestnum++;
    			
    			modelGO.name = "AXModel_" + highestnum;
    		}
    		else
    			modelGO.name = m_name;

    		Selection.activeGameObject = modelGO;


    		model.axMat = new AXMaterial();

    		model.axMat.mat 		= (Material) 		AssetDatabase.LoadAssetAtPath(ArchimatixEngine.ArchimatixAssetPath+"/Materials/AX_GridPurple.mat", 						typeof(Material));
    		model.axMat.physMat 	= (PhysicMaterial) 	AssetDatabase.LoadAssetAtPath(ArchimatixEngine.ArchimatixAssetPath+"/Materials/AX_DefaultPhysicMaterial.physicMaterial", 	typeof(PhysicMaterial));
    		model.axMat.density = 1;

    		//if (ArchimatixEngine.graphEditorIsOpen())
    		if (ArchimatixEngine.useKyle)
    			ArchimatixEngine.createScalingFigure();


    		return modelGO;
    	}


    	public static AXModel getSelectedModel()
    	{
    		AXModel model;

    		AXModel[] axModels =  GameObject.FindObjectsOfType(typeof(AXModel)) as AXModel[];

    		//Log("** getSelecteModel::" + axModels.Length);

    		if (axModels != null)
    		{
    			if (axModels.Length == 1)
    			{
    				return axModels[0];
    			}
    			else if (axModels.Length > 1)
    			{
    				if (Selection.activeGameObject != null)
    				{
    					model = Selection.activeGameObject.GetComponent<AXModel>();
    					if (model != null)
    						return model;
    				}
    				else if (ArchimatixEngine.currentModel != null)
    					return ArchimatixEngine.currentModel;

    				return axModels[axModels.Length-1];
    			}
    		}
    		//GameObject axgo = AXEditorUtilities.createNewModel();
    		return null;// axgo.GetComponent<AXModel>();


    	}

    	public static AXModel getOrMakeSelectedModel()
    	{
    		AXModel model;

    		AXModel[] axModels =  GameObject.FindObjectsOfType(typeof(AXModel)) as AXModel[];

    		//Log("** getOrMakeSelecteModel::" + axModels.Length);
    		 
    		if (axModels != null)
    		{
    			if (axModels.Length == 1)
    			{
    				return axModels[0];
    			}
    			else if (axModels.Length > 1)
    			{
    				if (Selection.activeGameObject != null)
    				{
    					model = Selection.activeGameObject.GetComponent<AXModel>();
    					if (model != null)
    						return model;
    				}
    				else if (ArchimatixEngine.currentModel != null)
    					return ArchimatixEngine.currentModel;

    				return axModels[axModels.Length-1];
    			}
    		}
    		GameObject axgo = AXEditorUtilities.createNewModel();
    		return axgo.GetComponent<AXModel>();
    		
    		
    	}




    	public static void generateModel(AXModel model, string caller = "")
    	{
    	
    		model.generate (caller);
    		EditorUtility.SetDirty (model.gameObject);
    	}



    	public static void DrawGrid3D(float size = 50, float bayw = 10)		
    	{		
    		
    		float 	baycount = size/bayw;
    		float 	edge = size/2;
    		
    		for(int i=0; i<=baycount; i++) 
    		{		
    			Handles.DrawLine(new Vector3(-edge+i*bayw, -edge, 0), new Vector3(-edge+i*bayw, edge, 0));
    			Handles.DrawLine(new Vector3(-edge, -edge+i*bayw, 0), new Vector3(edge, -edge+i*bayw,0));
    		}
    	}
    	

    	public static void drawCrosshair(Vector3 cen)
    	{
    		
    		float size = .15f * HandleUtility.GetHandleSize(cen);
    		
    		Vector3 p1 = new Vector3 (cen.x-size, 	cen.y,		cen.z);
    		Vector3 p2 = new Vector3 (cen.x+size, 	cen.y,		cen.z);
    		Vector3 p3 = new Vector3 (cen.x, 		cen.y+size, cen.z);
    		Vector3 p4 = new Vector3 (cen.x, 		cen.y-size, cen.z);
    		Vector3 p5 = new Vector3 (cen.x, 		cen.y, 		cen.z+size);
    		Vector3 p6 = new Vector3 (cen.x, 		cen.y, 		cen.z-size);
    		
    		Handles.DrawLine(p1, p2);
    		Handles.DrawLine(p3, p4);
    		Handles.DrawLine(p5, p6);
    		
    		
    	}
    	
    	
    	public static void displayModelOfSelectedGameObject() 
    	{
    		/* 
    		 * When the selection has changed, 
    		 * 1. select the AXModel in the scene
    		 * 2. within the model select the PO
    		 * 3. if the PO is the same as last time, try to select PO's consumer.
    		 * 4. If it has no consumer, select an input PO upstream.
    		 */
    		
    		
    		if (ArchimatixEngine.currentModel == null)
    			return;
    		
    		//Debug.Log("SELECTION CHANGED::"+ Selection.activeGameObject.name);
    		
    		// IS it an AX gameObject? That is, a sub gameobject of the model root?
    		AXModel model = (AXModel) Selection.activeGameObject.GetComponent<AXModel>();
    		
    		if (model != null)
    		{
    			model.selectFirstHeadPO();			
    		}
    		//Repaint ();
    	}

    	
    	

    	
    	
    	
    	public static AXParametricObject addNodeToCurrentModel(string nodeName, bool panTo = true, AXParametricObject basedOnPO = null)
    	{
    		AXModel model = getOrMakeSelectedModel(); 


    		Undo.RegisterCompleteObjectUndo (model, "Add Generator named "+nodeName);
    		 

    		AXParametricObject npo = model.createNode(nodeName, panTo, basedOnPO);

    		if (npo == null)
    			return null;

    		if (basedOnPO != null)
    		{
    			//Debug.Log("basedOnPO.Name="+basedOnPO.Name);
    			basedOnPO.copyTransformTo(npo);

    		}




    		ArchimatixEngine.repaintGraphEditorIfExistsAndOpen();


    		bool isInstanceofCurrentGrouper = ( (npo.generator is IReplica) && model.currentWorkingGroupPO != null && basedOnPO == model.currentWorkingGroupPO);


    		if (model.currentWorkingGroupPO != null  )
    		{
    			List<AXParametricObject> npos = new List<AXParametricObject>();
    			npos.Add(npo);

    		
    			if (! isInstanceofCurrentGrouper)
    			{	


    				model.currentWorkingGroupPO.addGroupees(npos);
    			}
    		}

    		if (isInstanceofCurrentGrouper)
    			model.currentWorkingGroupPO = model.currentWorkingGroupPO.grouper;

    		return npo;
    		 
    		
    	}





    		


    	public static void assertFloatFieldKeyCodeValidity(string controlName)
    	{
    		
    		if (Event.current.type == EventType.KeyDown)
    		{
    			if(Event.current.keyCode != KeyCode.None && !AXEditorUtilities.isValidNumericFieldKeyCode(Event.current.keyCode) && GUI.GetNameOfFocusedControl() == controlName)
    				clearFocus();// GUI.FocusControl("dummy_label");
    		}
    	}

    	public static bool isValidNumericFieldKeyCode(KeyCode keyCode)
    	{
    	 	bool ret = true;
    		//Debug.Log("CHECK: " + keyCode + " :: " + (keyCode == KeyCode.Alpha2));


    		switch(keyCode)
    		{
    		case KeyCode.A:
    		case KeyCode.B:
    		case KeyCode.C:
    		case KeyCode.D:
    		case KeyCode.E:
    		case KeyCode.F:
    		case KeyCode.G:
    		case KeyCode.H:
    		case KeyCode.I:
    		case KeyCode.J:
    		case KeyCode.K:
    		case KeyCode.L:
    		case KeyCode.M:
    		case KeyCode.N:
    		case KeyCode.O:
    		case KeyCode.P:
    		case KeyCode.Q:
    		case KeyCode.R:
    		case KeyCode.S:
    		case KeyCode.T:
    		case KeyCode.U:
    		case KeyCode.V:
    		case KeyCode.W:
    		case KeyCode.X:
    		case KeyCode.Y:
    		case KeyCode.Z:
    			ret = false;

    			break;

    		}

    		return ret;

    	}


    	public static void assertIntFieldKeyCodeValidity(string controlName)
    	{

    		if (Event.current.type == EventType.KeyDown)
    		{
    			if(Event.current.keyCode != KeyCode.None && !AXEditorUtilities.isValidNumericFieldKeyCode(Event.current.keyCode) && GUI.GetNameOfFocusedControl() == controlName)
    				clearFocus();//GUI.FocusControl("dummy_label");
    		}
    	}



    	public static float getHandlesMatrixScaleAdjuster()
    	{
    		return 1.442f/AXUtilities.GetScale(Handles.matrix).magnitude;
    	}




    }



    public class AXMenuItem
    {
    	
    	public AXParameter p;
    	public string poName;
    	public string inputName;
    	
    	public AXMenuItem (AXParameter outp, string pon, string inp="Input" )
    	{
    		poName 		= pon;
    		inputName 	= inp;
    		p 			= outp;
    		
    	} 
    	
    	
    }




    // Editor Window Scaling Utility



    /// A simple class providing static access to functions that will provide a 
    /// zoomable area similar to Unity's built in BeginVertical and BeginArea
    /// Systems.
    // From:
    // http://www.arsentuf.me/2015/01/27/unity-3d-editor-gui-and-scaling-matricies/
     //https://github.com/Honeybunch/VNKit/blob/master/VNKit/Assets/VNKit/Editor/EditorZoomArea.cs
    public static class RectExtensions
    {
    	/// <summary>
    	/// Scales a rect by a given amount around its center point
    	/// </summary>
    	/// <param name="rect">The given rect</param>
    	/// <param name="scale">The scale factor</param>
    	/// <returns>The given rect scaled around its center</returns>
    	public static Rect ScaleSizeBy(this Rect rect, float scale) { return rect.ScaleSizeBy(scale, rect.center); }
    	
    	/// <summary>
    	/// Scales a rect by a given amount and around a given point
    	/// </summary>
    	/// <param name="rect">The rect to size</param>
    	/// <param name="scale">The scale factor</param>
    	/// <param name="pivotPoint">The point to scale around</param>
    	/// <returns>The rect, scaled around the given pivot point</returns>
    	public static Rect ScaleSizeBy(this Rect rect, float scale, Vector2 pivotPoint) 
    	{
    		Rect result = rect;
    		
    		//"translate" the top left to something like an origin
    		result.x -= pivotPoint.x;
    		result.y -= pivotPoint.y;
    		
    		//Scale the rect
    		result.xMin *= scale;
    		result.yMin *= scale;
    		result.xMax *= scale;
    		result.yMax *= scale;
    		
    		//"translate" the top left back to its original position
    		result.x += pivotPoint.x;
    		result.y += pivotPoint.y;
    		
    		return result;
    	}
    }


