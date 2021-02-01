#pragma warning disable 

using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using AX.SimpleJSON;




using AXEditor;

using AX.Generators;
using AXGeometry;

using AX.GeneratorHandlers;
using AX;

 
[CustomEditor (typeof(AXModel))] 
public class AXModelInspector : Editor
{

	public bool mouseJustDown = false;


	Tool LastTool;

	private static GUIStyle richLabelStyle;

	Texture2D infoIconTexture;
	//GUISkin axskin;
	 
	[System.NonSerialized]
	public StopWatch commandStopWatch;


	public static bool doAutobuild;


	public void OnEnable ()
	{
		ArchimatixEngine.establishPaths (); 

		AXEditorUtilities.loadNodeIcons ();
		AXEditorUtilities.loaduiIcons ();

		infoIconTexture = (Texture2D)AssetDatabase.LoadAssetAtPath (ArchimatixEngine.ArchimatixAssetPath + "/ui/GeneralIcons/zz-AXMenuIcons-InfoIcon.png", typeof(Texture2D));

		//axskin = (GUISkin)AssetDatabase.LoadAssetAtPath ("Assets/AXGUISkin.guiskin", typeof(GUISkin));

	}


	public override void OnInspectorGUI ()
	{
		//if (Event.current.type != EventType.Layout)
		//	return;
		AXModel model = (AXModel)target;


		Event e = Event.current;
		//GUI.skin = axskin; 
		//GUI.skin = null; 

		//Debug.Log(evt.type);
		switch (e.type) {
		case EventType.Layout:
			if (doAutobuild) {
				doAutobuild = false;
				//model.autobuild ();
				ArchimatixEngine.scheduleBuild();
			}
			break;

		
		case EventType.MouseDown:
				//Debug.Log("Down");
				
			break;

		case EventType.MouseUp:
			
			//doAutobuild = true;
			break;

		case EventType.KeyUp:
			
			if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
			{
				Undo.RegisterCompleteObjectUndo (model, "Enter");
				doAutobuild = true;
			}
			//Debug.Log("KeyUp");
			//doAutobuild = true;
			break;

		case EventType.DragUpdated:
			//	UnityEngine.Debug.Log("Dragging");
			break;

		case EventType.DragPerform:
				//DragAndDrop.AcceptDrag();
			UnityEngine.Debug.Log("Drag and Drop not supported... yet");
			Undo.RegisterCompleteObjectUndo (model, "Default material scale");

			doAutobuild = true;
			break;
		}

	


		if (richLabelStyle == null) {
			richLabelStyle = new GUIStyle (GUI.skin.label);
			richLabelStyle.richText = true;
			richLabelStyle.wordWrap = true;
		}
		//if (infoIconTexture = null)


		String rubricColor = (EditorGUIUtility.isProSkin) ? "#bbbbff" : "#bbbbff";





		GUIStyle gsTest = new GUIStyle ();
		gsTest.normal.background = ArchimatixEngine.nodeIcons ["Blank"];// //Color.gray;
		gsTest.normal.textColor = Color.white;


		Color textColor = new Color (.82f, .80f, .85f);
		Color textColorSel = new Color (.98f, .95f, 1f);

		GUIStyle rubric = new GUIStyle(GUI.skin.label);
		rubric.normal.textColor =  textColor;


		GUIStyle foldoutStyle = new GUIStyle (EditorStyles.foldout);
		foldoutStyle.normal.textColor = textColor;
		foldoutStyle.active.textColor = textColorSel;
		foldoutStyle.hover.textColor = textColor;
		foldoutStyle.focused.textColor = textColorSel;

		foldoutStyle.onNormal.textColor = textColor;
		foldoutStyle.onActive.textColor = textColorSel;
		foldoutStyle.onHover.textColor = textColor;
		foldoutStyle.onFocused.textColor = textColorSel;






		GUILayout.Space (10);

		GUILayout.BeginVertical (gsTest);



		EditorGUI.indentLevel++;


		EditorGUIUtility.labelWidth = 150;


		model.displayModelDefaults = EditorGUILayout.Foldout (model.displayModelDefaults, "Model Defaults", true, foldoutStyle);

		if (model.displayModelDefaults) {

			// PRECISION LEVEL
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Space(20);
			GUILayout.Label ("Precision Level", rubric);
			GUILayout.FlexibleSpace ();
			model.precisionLevel = (PrecisionLevel)EditorGUILayout.EnumPopup ("", model.precisionLevel);
			EditorGUILayout.EndHorizontal ();


			if (!AXNodeGraphEditorWindow.IsOpen) {
				GUILayout.Space (10);

				if (GUILayout.Button ("Open in Node Graph")) {
					AXNodeGraphEditorWindow.Init ();
				}
			}


			//GUILayout.Space(20);




			// -- RUBRIC - MATERIAL --

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Space(20);
			GUILayout.Label ("Material (Default)", rubric);
			GUILayout.FlexibleSpace ();

			//if (GUILayout.Button ( infoIconTexture, GUIStyle.none))
			if (GUILayout.Button (infoIconTexture, GUIStyle.none, new GUILayoutOption[] {
				GUILayout.Width (16),
				GUILayout.Height (16)
			})) {
				
				Application.OpenURL ("http://www.archimatix.com/manual/materials"); 
			}

			EditorGUILayout.EndHorizontal ();

			// -------- 
			if (model.axMat.mat.name == "AX_GridPurple")
				EditorGUILayout.HelpBox("Set the default Material for this model.", MessageType.Info);

			// Material
			EditorGUI.BeginChangeCheck ();

			model.axMat.mat = (Material)EditorGUILayout.ObjectField (model.axMat.mat, typeof(Material), true);
			if (EditorGUI.EndChangeCheck ()) {
				Undo.RegisterCompleteObjectUndo (model, "Default material for " + model.name);
				model.remapMaterialTools ();
				model.autobuild ();

			}

			GUILayout.Space(10);

			// Texture //
			model.showDefaultMaterial = EditorGUILayout.Foldout (model.showDefaultMaterial, "Texture Scaling", true, foldoutStyle);
			if (model.showDefaultMaterial) {

				EditorGUI.BeginChangeCheck ();
				model.axTex.scaleIsUnified = EditorGUILayout.Toggle ("Unified Scaling", model.axTex.scaleIsUnified);
				if (EditorGUI.EndChangeCheck ()) {
					Undo.RegisterCompleteObjectUndo (model, "Default material scale");
					model.axTex.scale.y = model.axTex.scale.x;
					model.isAltered ();
				}
				 
				if (model.axTex.scaleIsUnified) {
					EditorGUI.BeginChangeCheck ();
					model.axTex.scale.x = EditorGUILayout.FloatField ("Scale", model.axTex.scale.x);
					if (EditorGUI.EndChangeCheck ()) {
						Undo.RegisterCompleteObjectUndo (model, "Default material for " + model.name);

						model.axTex.scale.y = model.axTex.scale.x;
						model.isAltered ();
						ArchimatixEngine.scheduleBuild();

					}
				} else {
					// Scale X
					EditorGUI.BeginChangeCheck ();
					model.axTex.scale.x = EditorGUILayout.FloatField ("Scale X", model.axTex.scale.x);
					if (EditorGUI.EndChangeCheck ()) {
						
						Undo.RegisterCompleteObjectUndo (model, "Default material for " + model.name);
						model.isAltered ();
						ArchimatixEngine.scheduleBuild();

					}

					// Scale Y
					EditorGUI.BeginChangeCheck ();
					model.axTex.scale.y = EditorGUILayout.FloatField ("Scale Y", model.axTex.scale.y);
					if (EditorGUI.EndChangeCheck ()) {
						
						Undo.RegisterCompleteObjectUndo (model, "Default material for " + model.name);
						model.isAltered ();
						ArchimatixEngine.scheduleBuild();


					}

				}

				EditorGUI.BeginChangeCheck ();
				model.axTex.runningU = EditorGUILayout.Toggle ("Running U", model.axTex.runningU);
				if (EditorGUI.EndChangeCheck ()) {
					Undo.RegisterCompleteObjectUndo (model, "Running U");
					model.isAltered ();
					ArchimatixEngine.scheduleBuild();

				}



			}

			GUILayout.Space(10);


			// PhysicMaterial //
			model.axMat.showPhysicsDefaults = EditorGUILayout.Foldout (model.axMat.showPhysicsDefaults, "Physics", true, foldoutStyle);
			if (model.axMat.showPhysicsDefaults) {

                // COLLIDER
                EditorGUI.BeginChangeCheck();
                model.defaultColliderType = (ColliderType)EditorGUILayout.EnumPopup( "Default Collider ", model.defaultColliderType);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RegisterCompleteObjectUndo(model, model.name + ": collidertype");

                }
                model.overrideColliderOnLibraryItem = EditorGUILayout.ToggleLeft("Override Colliders in Library Item", model.overrideColliderOnLibraryItem);

                GUILayout.Space(20);

                // PHYSIC MATERIAL
                EditorGUI.BeginChangeCheck ();
				model.axMat.physMat = (PhysicMaterial)EditorGUILayout.ObjectField (model.axMat.physMat, typeof(PhysicMaterial), true);
				if (EditorGUI.EndChangeCheck ()) {
					Undo.RegisterCompleteObjectUndo (model, "Default PhysicMaterial for " + model.name);
					model.remapMaterialTools ();
					ArchimatixEngine.scheduleBuild();
				}

				// DENSITY
				EditorGUI.BeginChangeCheck ();
				model.axMat.density = EditorGUILayout.FloatField ("Density", model.axMat.density);
				if (EditorGUI.EndChangeCheck ()) {
					Undo.RegisterCompleteObjectUndo (model, "Material Density for " + model.name);
					model.isAltered ();
					ArchimatixEngine.scheduleBuild();
				}
			}


			GUILayout.Space (20);



			model.automaticModelRegeneration = EditorGUILayout.ToggleLeft("Automatic Model Regeneration", model.automaticModelRegeneration);

			GUILayout.Space (20);

			// -- RUBRIC - LIGHTING --

			EditorGUILayout.BeginHorizontal ();


			GUILayout.Space(20);
			GUILayout.Label ("Lighting", rubric);


			//GUILayout.Label ("<color=" + rubricColor + "> <size=13>Lighting</size></color>", richLabelStyle);
			GUILayout.FlexibleSpace ();

			//if (GUILayout.Button ( infoIconTexture, GUIStyle.none))
			if (GUILayout.Button (infoIconTexture, GUIStyle.none, new GUILayoutOption[] {
				GUILayout.Width (16),
				GUILayout.Height (16)
			})) {
				
				Application.OpenURL ("http://www.archimatix.com/manual/lightmapping-with-archimatix"); 
			}

			EditorGUILayout.EndHorizontal ();

			// -------- 


			// LIGHTMAP FLAGS ENABLED
			EditorGUI.BeginChangeCheck ();
			model.staticFlagsEnabled = EditorGUILayout.ToggleLeft ("Lightmap Flags Enabled", model.staticFlagsEnabled);
			if (EditorGUI.EndChangeCheck ()) {
				Undo.RegisterCompleteObjectUndo (model, "Static Masks Enabled change for " + model.name);

				model.staticFlagsJustEnabled = true;

				ArchimatixEngine.scheduleBuild();
			}

			// SECONDARY UVs
			if (model.staticFlagsEnabled) {
				//if (model.buildStatus == AXModel.BuildStatus.Generated)
				EditorGUI.BeginChangeCheck ();
				model.createSecondaryUVs = EditorGUILayout.ToggleLeft ("Create Secondary UVs (for Baked GI)", model.createSecondaryUVs);
				if (EditorGUI.EndChangeCheck ()) {
					//if (model.createSecondaryUVs)
					//	AXEditorUtilities.makeLightMapUVs (model);
					model.createSecondaryUVsJustEnabled = true;


				}
			}







			GUILayout.Space (20);

		} // displayModelDefaults

		/*
		if (GUILayout.Button("Set All Objects as Lightmap Static"))
		{
			Debug.Log("Set all");
			model.setLightmapStaticForAllPOs();
		}
		*/





		if (ArchimatixEngine.plevel == 3) {

			// RUNTIME //

			string countString = "";
			if (model.exposedParameterAliases != null && model.exposedParameterAliases.Count > 0)
				countString = " ("+model.exposedParameterAliases.Count+")";

			model.displayModelRuntimeParameters = EditorGUILayout.Foldout (model.displayModelRuntimeParameters, "Runtime Parameters"+countString, true, foldoutStyle);

			if (model.displayModelRuntimeParameters) {
				//GUILayout.Label("<color="+rubricColor+"> <size=13>Pro Runtime Features</size></color>", richLabelStyle);


				// EXPOSED PARAMETERS 
				//if (model.cycleSelectedAXGO != null)
				//	GUILayout.Label("Consumer Address: "+model.cycleSelectedAXGO.consumerAddress);

				//GUILayout.Label("Runtime Parameters");

				if (model.exposedParameterAliases != null && model.exposedParameterAliases.Count > 0) {
					EditorGUI.BeginChangeCheck ();
                    for (int i=0; i < model.exposedParameterAliases.Count; i++)
						ParameterAliasGUILayout.OnGUI (model.exposedParameterAliases[i]);
					if (EditorGUI.EndChangeCheck ()) {
						model.isAltered ();
					}
				} else {
//					GUIStyle labelSty = new GUIStyle("Label");
//					labelSty.wordWrap = true;
//					labelSty.richText = true;

					EditorGUILayout.HelpBox("You can add runntime parameters by opening any parameter in the graph and checking \"Enable Runtime\"", MessageType.Info);

					//GUILayout.Label ("<color=\"gray\">You can add runntime parameters by opening any parameter in the graph and checking \"Enable Runtime\"</color>", labelSty);
				}
					

				GUILayout.Space (15);

				if (model.exposedParameterAliases != null && model.exposedParameterAliases.Count > 0) {
					if (GUILayout.Button ("Create Runtime Controller", GUILayout.Width (200))) {
						ArchimatixEngine.createControllerForModel = model;
					}
				}




				// RUNTIME HANDLES 
				//if (model.cycleSelectedAXGO != null)
				//	GUILayout.Label("Consumer Address: "+model.cycleSelectedAXGO.consumerAddress);
				if (model.runtimeHandleAliases != null && model.runtimeHandleAliases.Count > 0) {
					//GUILayout.Label("Runtime Handles");


					foreach (AXHandleRuntimeAlias rth in model.runtimeHandleAliases) {			
						AXRuntimeHandlesGUI.OnGUI (rth);
					}
				}

			

				GUILayout.Space (20);

			}

		} // RUNTIME









		// RELATIONS

		if (model.selectedRelationInGraph != null) {
			GUILayout.Space (20);

			GUILayout.Label ("<color=" + rubricColor + "> <size=13>Selected Relation</size></color>", richLabelStyle);
		

			AXRelation r = model.selectedRelationInGraph;
			RelationEditorGUI.OnGUI (r);




		}


		//GUILayout.Space(20);


		model.displayModelSelectedNodes = EditorGUILayout.Foldout (model.displayModelSelectedNodes, "Selected Node Controls", true, foldoutStyle);

		if (model.displayModelSelectedNodes) {

			// -- RUBRIC - SELECTED NODES --

			EditorGUILayout.BeginHorizontal ();

			//GUILayout.Label("<color="+rubricColor+"> <size=13>Selected Nodes</size></color>", richLabelStyle);
			GUILayout.FlexibleSpace ();

			//if (GUILayout.Button ( infoIconTexture, GUIStyle.none))
			if (GUILayout.Button (infoIconTexture, GUIStyle.none, new GUILayoutOption[] {
				GUILayout.Width (16),
				GUILayout.Height (16)
			})) {
				Application.OpenURL ("http://www.archimatix.com/manual/node-selection"); 
			}

			EditorGUILayout.EndHorizontal ();

			// -------- 



			//GUILayout.Space(10);
			
			if (model.selectedPOs != null && model.selectedPOs.Count > 0) {
				for (int i = 0; i < model.selectedPOs.Count; i++) {
					//Debug.Log(i);
					AXParametricObject po = model.selectedPOs [i];

					//Debug.Log(i+" ------------------------ po.Name="+po.Name+ " -- " + po.generator.AllInput_Ps.Count);


					doPO (po);

					// for subnodes...

					if (po.generator.AllInput_Ps != null) {
						for (int j = po.generator.AllInput_Ps.Count - 1; j >= 0; j--) {

							AXParameter p = po.generator.AllInput_Ps [j];
							
							if (p.DependsOn != null) {
								AXParametricObject spo = p.DependsOn.parametricObject;
								doPO (spo);

								// sub-sub nodes...
								for (int k = spo.generator.AllInput_Ps.Count - 1; k >= 0; k--) {

									if (spo.generator.AllInput_Ps [k].DependsOn != null)
										doPO (spo.generator.AllInput_Ps [k].DependsOn.parametricObject);
								}
							}


						}
					}


				}
			} else
				GUILayout.Label ("<color=\"gray\">...no nodes selected</color>", richLabelStyle);

			GUILayout.Space (50);



		}

		EditorGUI.indentLevel--;

		GUILayout.EndVertical ();
//		Editor currentTransformEditor = Editor.CreateEditor(model.gameObject.);
//		if (currentTransformEditor != null) {
//            currentTransformEditor.OnInspectorGUI ();
//        }






		//model.controls[0].val = EditorGUILayout.Slider(model.controls[0].val, 0, 100);

		 
		/*
		switch (e.type)
		{
		case EventType.KeyUp:
		case EventType.MouseUp: 

			model.autobuild();
			//e.Use ();

			//return;
			break;

		case EventType.MouseDown: 

			//model.autobuild();
			//e.Use ();
			break;

		}
		*/

	
		//DrawDefaultInspector ();
	}




















	public static bool showTitle (AXParametricObject po)
	{
		if (po.generator is MaterialTool)
			return false;

		return true;

	}


	public void doPO (AXParametricObject po)
	{

		
		Color guiColorOrig = GUI.color;
		Color guiContentColorOrig = GUI.contentColor;
		GUI.color = Color.white;
		GUI.contentColor = Color.white;

		Color textColor = new Color (.82f, .80f, .85f);
		Color textColorSel = new Color (.98f, .95f, 1f);


		GUIStyle labelstyle = new GUIStyle (GUI.skin.label);
		//int fontSize = labelstyle.fontSize;
		labelstyle.fixedHeight = 30;
		labelstyle.alignment = TextAnchor.UpperLeft;

		labelstyle.normal.textColor = textColorSel;

		labelstyle.fontSize = 20;

//		GUIStyle labelstyleTmp = GUI.skin.GetStyle("Label"); 
//		labelstyleTmp.normal.textColor = Color.red;

		Color bgcolorOrig = GUI.backgroundColor;
		//GUI.backgroundColor = Color.cyan;



		GUIStyle gsTest = new GUIStyle ();
		gsTest.normal.background = ArchimatixEngine.nodeIcons ["Blank"];// //Color.gray;
		gsTest.normal.textColor = Color.white;

		GUIStyle foldoutStyle = new GUIStyle (EditorStyles.foldout);
		foldoutStyle.normal.textColor = textColor;
		foldoutStyle.active.textColor = textColorSel;
		foldoutStyle.hover.textColor = textColor;
		foldoutStyle.focused.textColor = textColorSel;

		foldoutStyle.onNormal.textColor = textColor;
		foldoutStyle.onActive.textColor = textColorSel;
		foldoutStyle.onHover.textColor = textColor;
		foldoutStyle.onFocused.textColor = textColorSel;

		GUIStyle smallFoldoutStyle = new GUIStyle (EditorStyles.foldout);

		smallFoldoutStyle.fixedWidth = 0;




		GUILayout.BeginVertical (gsTest);


		GUILayout.BeginHorizontal (gsTest);
		GUILayout.Space (40);
		Rect rect = GUILayoutUtility.GetLastRect ();

		//GUI.DrawTexture(new Rect(position.x, rect.y, EditorGUIUtility.currentViewWidth, 100), ArchimatixEngine.nodeIcons["Blank"], ScaleMode.ScaleToFit, true, 1.0F);

		GUILayout.Space (10);

		// TITLE
		GUILayout.Label (po.Name, labelstyle);


		labelstyle.fontSize = 12;
		GUILayout.EndHorizontal ();







		if (po.is2D () && po.generator.hasOutputsReady ()) {
			AXParameter output_p = po.generator.getPreferredOutputParameter (); 
			GUIDrawing.DrawPathsFit (output_p, new Vector2 (42, rect.y + 15), 28, ArchimatixEngine.AXGUIColors ["ShapeColor"]);
		} else if (ArchimatixEngine.nodeIcons != null && ArchimatixEngine.nodeIcons.ContainsKey (po.GeneratorTypeString)) {
			//Rect thumbRect = new Rect (28, rect.y - 0, 36, 36);
			//GUI.DrawTexture(thumbRect,    po.renTex, ScaleMode.ScaleToFit, true, 1.0F);
			EditorGUI.DrawTextureTransparent (new Rect (28, rect.y - 0, 36, 36), ArchimatixEngine.nodeIcons [po.GeneratorTypeString], ScaleMode.ScaleToFit, 1.0F);
		}
		Rect rectthumb2 = GUILayoutUtility.GetLastRect ();


		if (po.is3D () && po.renTex != null) {
			GUIStyle thumbLgStyle = new GUIStyle ();
			float thumbLgSize = 64;

			thumbLgStyle.fixedHeight = thumbLgSize;
			GUILayout.BeginHorizontal (thumbLgStyle);
			GUILayout.Space (thumbLgSize);

			Rect thumbRectLG = new Rect (40, rectthumb2.y + 35, thumbLgSize, thumbLgSize);
			EditorGUI.DrawTextureTransparent (thumbRectLG, po.renTex, ScaleMode.ScaleToFit, 1.0F);

			GUILayout.EndHorizontal ();
		}




		EditorGUI.indentLevel++;


		//GUILayout.Space(20);

		GUILayout.BeginHorizontal ();

		EditorGUIUtility.labelWidth = 35;

		EditorGUI.BeginChangeCheck ();
		//EditorGUIUtility.labelWidth = 65;

		po.isActive = EditorGUILayout.ToggleLeft ("Enabled", po.isActive );
		if (EditorGUI.EndChangeCheck ()) {
			


			Undo.RegisterCompleteObjectUndo (po.model, "isActive value change for " + po.Name);
			po.model.autobuild ();
			po.generator.adjustWorldMatrices ();
		}

		GUILayout.FlexibleSpace ();

		if (GUILayout.Button ("Select in Node Graph")) {
			po.model.selectAndPanToPO (po);
		}

		GUILayout.EndHorizontal ();




		// FLAGS, TAGS & LAYERS
		if (po.is3D ())
			po.displayFlagsTagsLayers = EditorGUILayout.Foldout (po.displayFlagsTagsLayers, "Name, Flags, Tags & Layers", true, foldoutStyle);
		else
			po.displayFlagsTagsLayers = EditorGUILayout.Foldout (po.displayFlagsTagsLayers, "Name", true, foldoutStyle);


		EditorGUIUtility.labelWidth = 135;
		
		if (po.displayFlagsTagsLayers) {
			//EDIT TITLE
//			if (showTitle(po))
//			{
			//GUILayout.BeginHorizontal();

					


			GUILayout.BeginHorizontal ();
			GUILayout.Space (35);
			GUILayout.Label ("Name: ");


			po.Name = GUILayout.TextField (po.Name);
			GUILayout.EndHorizontal ();
				 


			if (po.is3D ()) {

				//EditorGUIUtility.labelWidth = 75;

				EditorGUI.BeginChangeCheck ();
				#if UNITY_2018_1_OR_NEWER
				po.axStaticEditorFlags = (AXStaticEditorFlags)EditorGUILayout.EnumFlagsField ("Static: ", po.axStaticEditorFlags);
				#else
				po.axStaticEditorFlags = (AXStaticEditorFlags)EditorGUILayout.EnumMaskField ("Static: ", po.axStaticEditorFlags);
				#endif
				if (EditorGUI.EndChangeCheck ()) {
					Undo.RegisterCompleteObjectUndo (po.model, "Static value change for " + po.Name);

					po.setUpstreamersToYourFlags ();
					// post dialog to change all children...
					ArchimatixEngine.scheduleBuild();
												
				}


				GUI.backgroundColor = bgcolorOrig;



				//EditorGUIUtility.labelWidth = 75;




				// TAGS
				EditorGUI.BeginChangeCheck ();
				po.tag = EditorGUILayout.TagField ("Tag:", po.tag);
				if (EditorGUI.EndChangeCheck ()) {
					Undo.RegisterCompleteObjectUndo (po.model, "Tag value change for " + po.Name);
					ArchimatixEngine.scheduleBuild();
				}



				// LAYERS
				EditorGUI.BeginChangeCheck ();
				int intval = EditorGUILayout.LayerField ("Layer:", po.layer);

				if (EditorGUI.EndChangeCheck ()) {
					Undo.RegisterCompleteObjectUndo (po.model, "Layer value change for " + po.Name);
					po.layer = intval;
					ArchimatixEngine.scheduleBuild();
				}


				


			} else if (po.generator is MaterialTool) {
				EditorGUI.BeginChangeCheck ();

				//float thumbSize = 16;

				po.axMat.mat = (Material)EditorGUILayout.ObjectField (po.axMat.mat, typeof(Material), true);
				if (EditorGUI.EndChangeCheck ()) {
					
					Undo.RegisterCompleteObjectUndo (po.model, "Material");

					po.model.remapMaterialTools ();
					po.model.autobuild ();
					 
				}

			}

		} // FLAGS< TAGS & LAYERS



		po.displayMeshRenderOptions = EditorGUILayout.Foldout (po.displayMeshRenderOptions, "MeshRenderer", true, foldoutStyle);

		if (po.displayMeshRenderOptions)
		{
			bool hasMeshRenderer = !po.noMeshRenderer;
				EditorGUI.BeginChangeCheck ();
				hasMeshRenderer = EditorGUILayout.ToggleLeft ("Mesh Renderer", hasMeshRenderer);
				if (EditorGUI.EndChangeCheck ()) {
					Undo.RegisterCompleteObjectUndo (po.model, "hasMeshRenderer");
					po.noMeshRenderer = !hasMeshRenderer;
					ArchimatixEngine.scheduleBuild();
				}
				if (hasMeshRenderer) {
					//EditorGUI.BeginChangeCheck ();
					EditorGUI.BeginChangeCheck ();
					po.lightProbeUsage = (LightProbeUsage) EditorGUILayout.EnumPopup("Light Probes", po.lightProbeUsage);
					po.reflectionProbeUsage = (ReflectionProbeUsage) EditorGUILayout.EnumPopup("Reflection Probes", po.reflectionProbeUsage);
					po.shadowCastingMode = (ShadowCastingMode) EditorGUILayout.EnumPopup("Cast Shadows", po.shadowCastingMode);
					po.receiveShadows = EditorGUILayout.ToggleLeft ("Receive Shadows", po.receiveShadows);

					po.motionVectorGenerationMode = (MotionVectorGenerationMode) EditorGUILayout.EnumPopup("Motion Vectors", po.motionVectorGenerationMode);

					if (EditorGUI.EndChangeCheck ()) {
					po.model.autobuild ();
					}
				}
            po.isTrigger = EditorGUILayout.ToggleLeft("isTrigger", po.isTrigger);


        }
			 



		// POSITION CONTROLS

		if (po.positionControls != null && po.positionControls.children != null) {
			po.positionControls.isOpenInInspector = EditorGUILayout.Foldout (po.positionControls.isOpenInInspector, "Transform", true, foldoutStyle);

			if (po.positionControls.isOpenInInspector) {
				InspectorParameterGUI.OnGUI (po.positionControls.children);
			}
		}



		// GEOMETRY CONTROLS

		if (po.geometryControls != null && po.geometryControls.children != null) {
			po.geometryControls.isOpenInInspector = EditorGUILayout.Foldout (po.geometryControls.isOpenInInspector, "Geometry Controls", true, foldoutStyle);
			if (po.geometryControls.isOpenInInspector) {
				InspectorParameterGUI.OnGUI (po.geometryControls.children);
			}
		}


		// PROTOTYPES

		if (po.is3D ()) {
			po.displayPrototypes = EditorGUILayout.Foldout (po.displayPrototypes, "Prototypes", true, foldoutStyle);

			if (po.displayPrototypes) {
				EditorGUI.BeginChangeCheck ();
				po.prototypeGameObject = (GameObject)EditorGUILayout.ObjectField (po.prototypeGameObject, typeof(GameObject), true);
				if (EditorGUI.EndChangeCheck ()) {
					Undo.RegisterCompleteObjectUndo (po.model, "Prototype GameObject set for " + po.model.name);
					if (po.prototypeGameObject != null) {
						AXPrototype proto = (AXPrototype)po.prototypeGameObject.GetComponent ("AXPrototype");
						if (proto == null) {
							proto = po.prototypeGameObject.AddComponent<AXPrototype> ();
						}
						if (!proto.parametricObjects.Contains (po))
							proto.parametricObjects.Add (po);
					}
					po.model.autobuild ();


				}
			}

		}


		if (po.selectedAXGO != null) {

			GUILayout.Label (po.selectedAXGO.consumerAddress);
		}



//		if (po.is3D() && po.renTex != null)
//		{
//		GUIStyle thumbLgStyle = new GUIStyle();
//			float thumbLgSize = 194;
//
//			thumbLgStyle.fixedHeight = thumbLgSize;
//			GUILayout.BeginHorizontal(thumbLgStyle);
//			GUILayout.Space(40);
//			Rect rectthumb = GUILayoutUtility.GetLastRect();
//			Rect thumbRectLG = new Rect(28, rectthumb.y-0, thumbLgSize, thumbLgSize);
//		GUI.DrawTexture(thumbRectLG,    po.renTex, ScaleMode.ScaleToFit, true, 1.0F);
//
//		GUILayout.EndHorizontal();
//		}
//


		EditorGUI.indentLevel--;




		GUILayout.EndVertical ();
		GUILayout.Space (30);


		GUI.color = guiColorOrig;
		GUI.contentColor	= guiContentColorOrig;


	}

	
	

}
