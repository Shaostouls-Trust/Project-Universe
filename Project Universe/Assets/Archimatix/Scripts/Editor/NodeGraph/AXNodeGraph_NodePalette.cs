using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

using System.Text;
using System.Text.RegularExpressions;

using AXGeometry;

using AX;
using AX.Generators;
using AX.GeneratorHandlers;

namespace AXEditor
{

	public class AXNodeGraph_NodePalette  
	{

		
		
		// return the height of this gui area
		public static void OnGUI(int win_id, AXNodeGraphEditorWindow editor, AXParametricObject po) 
		{
			
					
			Event e = Event.current;

			AXModel model = editor.model;






//		if (EditorGUIUtility.isProSkin)
//			GUI.DrawTexture(new Rect(0, 0, maxSize.x, maxSize.y), BGTexturePalette, ScaleMode.StretchToFill);

//
//		if( editor.paletteBoxStyle != null )
//    	{
//				editor.paletteBoxStyle = new GUIStyle( GUI.skin.box );
//				editor.paletteBoxStyle.normal.background = editor.paletteBoxTex;
//    	}
//
//
//
		Color origBG = GUI.backgroundColor;
//		
//		//int po_id = win_id-currentWindowOffset;
//
//
//		//bool mouseIsUpInWindow = false;
		Color currColor;
//
//		if (po_id >= model.parametricObjects.Count)
//			return;
//
//

		
		AXParameter p;



		GeneratorHandler generatorHandler = GeneratorHandler.getGeneratorHandler(po);


		//if (po.Name == "plan1") Debug.Log(po.rect + " - " +  mousePosition + " -- " + po.rect.Contains (mousePosition));

		//Rect localRectForPO = new Rect(0, 0, po.rect.width, po.rect.height);


		ArchimatixUtils.paletteRect = po.rect;

		// DRAW HIGHLIGHT AROUND SELECTED NODE PALETTE
		if (model.isSelected(po))
		{
			float pad = (EditorGUIUtility.isProSkin) ? 1 : 1;
			Rect outline = new Rect(0, 0, po.rect.width-pad, po.rect.height-pad);
			Handles.color = Color.white;

			Handles.DrawSolidRectangleWithOutline(outline, new Color(1, 1, 1, 0f), ArchimatixEngine.AXGUIColors["NodePaletteHighlightRect"]);
			//Handles.DrawSolidRectangleWithOutline(outline, new Color(.1f, .1f, .3f, .05f), new Color(.1f, .1f, .8f, 1f));
		}






		// get serialized property for this po
		// then drill down to parameters for this property



		Rect headerRect = new Rect(0, 0, po.rect.width, po.rect.height);

		if ((e.button == 0) && (e.type == EventType.MouseDown) && headerRect.Contains(e.mousePosition)) {

			if ( po.startRect != po.rect)
				Undo.RegisterCompleteObjectUndo (model, "ParametricObject Drag");

			po.startRect = po.rect;
		}

		Color defaultColor = GUI.color;









		// START LAYOUT OF INNER PALETTE


		// Horizontal layput
		float winMargin = ArchimatixUtils.indent;
		float innerWidth = po.rect.width - 2*winMargin;



		int x1 = 10;
		int x2 = 20;

		// vertical layut
		int cur_y 	= 25;
		int gap 	= ArchimatixUtils.gap;
		int lineHgt = ArchimatixUtils.lineHgt;


		if (EditorGUIUtility.isProSkin)
		{
			GUI.color = po.generator.GUIColorPro;
			GUI.backgroundColor = Color.Lerp(po.generator.GUIColorPro, Color.white, .5f) ;
		}
		else
		{
			GUI.color = po.generator.GUIColor;
			GUI.backgroundColor = Color.Lerp(po.generator.GUIColor, Color.white, .5f) ;

		}


		//GUI.backgroundColor = Color.white;


		if (EditorGUIUtility.isProSkin && editor.paletteBoxStyle != null)
		{
			//Debug.Log(paletteBoxStyle);
				GUI.Box(new Rect(winMargin, lineHgt, innerWidth, po.outputEndHeight), "", editor.paletteBoxStyle);
				GUI.Box(new Rect(winMargin, lineHgt, innerWidth, po.outputEndHeight), "", editor.paletteBoxStyle);

				if (editor.isSelected(po))
			{
					GUI.Box(new Rect(winMargin, lineHgt, innerWidth, po.outputEndHeight), "", editor.paletteBoxStyle);
					GUI.Box(new Rect(winMargin, lineHgt, innerWidth, po.outputEndHeight), "", editor.paletteBoxStyle);
			}
		}
		else
		{
			GUI.Box(new Rect(winMargin, lineHgt, innerWidth, po.outputEndHeight), "");
			GUI.Box(new Rect(winMargin, lineHgt, innerWidth, po.outputEndHeight), "");

				if (editor.isSelected(po))
			{
				GUI.Box(new Rect(winMargin, lineHgt, innerWidth, po.outputEndHeight), "");
				GUI.Box(new Rect(winMargin, lineHgt, innerWidth, po.outputEndHeight), "");
			}
		}



		GUI.color = defaultColor;





		GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);


		buttonStyle.alignment 	= TextAnchor.MiddleLeft;
		buttonStyle.fontSize  	= 12;
		buttonStyle.fixedWidth 	= 0;
		buttonStyle.fixedHeight = 0;

		//style.onHover.textColor = Color.cyan;


		int editButtonWid = (int) (po.rect.width - 2*x1)/2 -6;

		 
		// TITLE
		//Debug.Log ("Adding title " + po.rect);
		//if (poProperty != null) 
		if (true) 
		{
			if (po.isEditing)
			{
				string tname = po.Name.Trim();
				//tname = "bear";
				GUI.SetNextControlName("title_Text_"+po.Guid); 
				tname = EditorGUI.TextField(new Rect(winMargin, cur_y, innerWidth, lineHgt), tname);				

				po.Name = tname.Trim();

				cur_y += lineHgt + gap;
			} 
			else 
			{
				
				if (GUI.Button(new Rect(x1, cur_y, innerWidth-lineHgt*2-6, lineHgt*2), po.Name))
				{				
					po.isEditing = true;
					for (int i=0; i<po.parameters.Count; i++) {
						p =  po.parameters[i];
						p.isEditing = false;
					}
				}


				if ( ArchimatixEngine.nodeIcons.ContainsKey(po.GeneratorTypeString))
						EditorGUI.DrawTextureTransparent(new Rect(x1+innerWidth-lineHgt*2-4, cur_y, lineHgt*2, lineHgt*2), ArchimatixEngine.nodeIcons[po.GeneratorTypeString], ScaleMode.ScaleToFit, 1.0F);


				cur_y += lineHgt + 2*gap;



			}





			if (po.isEditing)
			{
				buttonStyle.fontSize  = 12;


				cur_y += lineHgt + gap + 5;

				// DOCUMENTATION_URL (w/o domain)
				GUI.Label(new Rect(winMargin, cur_y, innerWidth-40, lineHgt), "Documentation URL");
				if (GUI.Button(new Rect(innerWidth-20, cur_y, 40, lineHgt), "Def"))
				{
					po.documentationURL = (po.is2D() ? "2d" : "3d")+"library/"+po.Name.ToLower();
				}
				cur_y += lineHgt + gap + 5; 

				GUI.SetNextControlName("documentationURL_Text_"+po.Guid);
				po.documentationURL = EditorGUI.TextField(new Rect(winMargin, cur_y, innerWidth, lineHgt), po.documentationURL);				


				cur_y += lineHgt + gap + 5; 


				AXEditorUtilities.assertFloatFieldKeyCodeValidity("sortval_" + po.Name);

				EditorGUI.BeginChangeCheck ();
				GUI.SetNextControlName("sortval_Text_" + po.Name);
				po.sortval = EditorGUI.FloatField(new Rect(x1+10, cur_y, 100,lineHgt), po.sortval);
				if(EditorGUI.EndChangeCheck())
				{
					//Debug.Log("changed");
					ArchimatixEngine.library.sortLibraryItems();
				}
				cur_y += lineHgt;


				// DONE
				if (GUI.Button (new Rect(x1, cur_y, innerWidth,lineHgt), "Done" ))
					po.doneEditing();

				cur_y += lineHgt + gap + 5;

			}

		}






		cur_y += 2*gap;







		//GUI.DrawTexture(new Rect(x1, cur_y, width, width), model.rt, ScaleMode.ScaleToFit, true, 1.0F);


		Color prevGUIColor = GUI.color;
		Color gcol = GUI.color;


		GUIStyle labelstyle	 	= GUI.skin.GetStyle("Label");
		labelstyle.fixedWidth = po.rect.width-x1-22;
		Rect tooltipRect = new Rect(x1+10,  cur_y+16, po.rect.width-x1-10, 16);





		// DO INFO BUTTON --
		Rect infoRect = new Rect(po.rect.width-x1-38, cur_y, 16, 16);


		// TOOLTIP
		if (infoRect.Contains(Event.current.mousePosition)) 
		{
			gcol.a = 1f;
			GUI.color = gcol;
			labelstyle.alignment = TextAnchor.MiddleRight;
			GUI.Label (tooltipRect, ("About " + System.IO.Path.GetExtension(po.generator.GetType ().ToString ()).Substring(1) ));
		}
		else
			gcol.a = .6f;
		GUI.color = gcol;



			if (GUI.Button ( infoRect, editor.infoIconTexture, GUIStyle.none))
		{
			string typename = System.IO.Path.GetExtension(po.generator.GetType ().ToString ().TrimStart('.'));

			if (! string.IsNullOrEmpty(po.documentationURL))
				Application.OpenURL("http://"+ArchimatixEngine.doucumentationDomain+"/"+po.documentationURL); 
			else
					Application.OpenURL("http://www.archimatix.com/nodes/"+typename.ToLower().Substring(1)); 
		}


		// DO NODE MENU
		Rect poMenuRect = new Rect(po.rect.width-x1-20, cur_y, 16, 16);

		// TOOLTIP
		if (poMenuRect.Contains(Event.current.mousePosition)) // TOOLTIP
		{
			gcol.a = 1f;
			GUI.color = gcol;
			labelstyle.alignment = TextAnchor.MiddleRight;
			GUI.Label (tooltipRect, ("Node Menu"));
		}
		else
			gcol.a = .8f;
		GUI.color = gcol;

		//GUI.Label(poMenuRect, menuIconTexture);
			if (GUI.Button ( poMenuRect, editor.menuIconTexture, GUIStyle.none))
		{

			bool isMac = SystemInfo.operatingSystem.Contains("Mac");

			GenericMenu menu = new GenericMenu ();

			//menu.AddSeparator("Library ...");


			menu.AddItem(new GUIContent("Save to Library             " + ((isMac) ? "⌘L" :  "Ctrl+L") ), false, LibraryEditor.doSave_MenuItem, po);
			menu.AddItem(new GUIContent("Save to Library Folder        "), false, LibraryEditor.doSave_MenuItem_NewFolder, po);

			menu.AddSeparator("");
			//menu.AddItem(new GUIContent("New ShapeDistributor"), false, splineOutputMenuItem, "ShapeDistributor");


			string editLabel = ((po.isEditing) ? "Done Editing                     ":"Edit                               ") + ((isMac) ? "⌘E" : "Ctrl+E");
			menu.AddItem(new GUIContent(editLabel), false,  () => {

				po.isEditing = !po.isEditing; 
			});



			menu.AddItem(new GUIContent("Copy                             " + ((isMac) ? "⌘C" : "Ctrl+C") ), false, () => {
				EditorGUIUtility.systemCopyBuffer = LibraryEditor.poWithSubNodes_2_JSON(po, true); 
				model.autobuild();
			});

			menu.AddItem(new GUIContent("Instance                       " + ((isMac) ? "⌘D" : "Ctrl+D") ), false,  () => {
				AXEditorUtilities.instancePO(po); 
				model.autobuild();
			});

			/*
			menu.AddItem(new GUIContent("Replicate          ⇧⌘D"), false,  () => {
				AXEditorUtilities.replicatePO(po); 
				model.autobuild();
			});
			*/

			//menu.AddItem(new GUIContent("Replicate"), false, () => {
			//	model.autoBuild();
			//});

			menu.AddItem(new GUIContent("Duplicate              " + ((isMac) ? "⌘C-⌘V" : "Ctrl+C-Ctrl+V") ), false, () => {
                //AXEditorUtilities.duplicatePO(po);
                //AXUtilities.duplicatePO(po);

                if (model.selectedPOs.Count > 0)
                {
                    EditorGUIUtility.systemCopyBuffer = LibraryEditor.poWithSubNodes_2_JSON(model.selectedPOs[0], true);
                    Library.pasteParametricObjectFromString(EditorGUIUtility.systemCopyBuffer);


                }


                model.autobuild();
			});

			menu.AddItem(new GUIContent("Close Controls"), false,  () => {
				po.closeParameterSets() ;
			});
			//menu.AddSeparator(" ...");
			menu.AddSeparator("");
			//menu.AddItem(new GUIContent("Lock               ⌘L"), false, () => {

			//});

			//menu.AddItem(new GUIContent("Make Inactive"), false, () => {

			//});

			menu.AddItem(new GUIContent("Delete                      " + ((isMac) ? " ⌘⌫" : "Del") ), false,  () => {
				model.deletePO(po); 
				model.remapMaterialTools();

				model.autobuild();
			});
			//e.mousePosition *= model.zoomScale;

			//Vector2 pos = GUIUtility.GUIToScreenPoint(e.mousePosition);
			//menu.DropDown(new Rect(e.mousePosition.x*model.zoomScale, e.mousePosition.y*model.zoomScale, 200, 500));




			//menu.AddSeparator("Transforms...");
			menu.AddSeparator("");
			menu.AddItem(new GUIContent("Reset Transform           "), false,  () => {
				Undo.RegisterCompleteObjectUndo (model, "Rest Transform");
				po.initiateRipple_setFloatValueFromGUIChange("Trans_X", 0);
				po.initiateRipple_setFloatValueFromGUIChange("Trans_Y", 0);
				po.initiateRipple_setFloatValueFromGUIChange("Trans_Z", 0);
				po.initiateRipple_setFloatValueFromGUIChange("Rot_X", 0);
				po.initiateRipple_setFloatValueFromGUIChange("Rot_Y", 0);
				po.initiateRipple_setFloatValueFromGUIChange("Rot_Z", 0);
				po.initiateRipple_setFloatValueFromGUIChange("Scale_X", 1);
				po.initiateRipple_setFloatValueFromGUIChange("Scale_Y", 1);
				po.initiateRipple_setFloatValueFromGUIChange("Scale_Z", 1);
				model.autobuild();
				po.generator.adjustWorldMatrices();

			});
			menu.AddItem(new GUIContent("Reset Position           "), false,  () => {
				Undo.RegisterCompleteObjectUndo (model, "Rest Position");
				po.initiateRipple_setFloatValueFromGUIChange("Trans_X", 0);
				po.initiateRipple_setFloatValueFromGUIChange("Trans_Y", 0);
				po.initiateRipple_setFloatValueFromGUIChange("Trans_Z", 0);
				model.autobuild();
				po.generator.adjustWorldMatrices();
			});
			menu.AddItem(new GUIContent("Reset Rotation           "), false,  () => {
				Undo.RegisterCompleteObjectUndo (model, "Rest Rotation");
				po.initiateRipple_setFloatValueFromGUIChange("Rot_X", 0);
				po.initiateRipple_setFloatValueFromGUIChange("Rot_Y", 0);
				po.initiateRipple_setFloatValueFromGUIChange("Rot_Z", 0);
				model.autobuild();
				po.generator.adjustWorldMatrices();
			});
			menu.AddItem(new GUIContent("Reset Scale           "), false,  () => {
				Undo.RegisterCompleteObjectUndo (model, "Rest Scale");
				po.initiateRipple_setFloatValueFromGUIChange("Scale_X", 1);
				po.initiateRipple_setFloatValueFromGUIChange("Scale_Y", 1);
				po.initiateRipple_setFloatValueFromGUIChange("Scale_Z", 1);
				model.autobuild();
				po.generator.adjustWorldMatrices();
			});


			menu.ShowAsContext ();

			if(e.type != EventType.Repaint && e.type != EventType.Layout) 
				e.Use();


		}

		GUI.color = prevGUIColor;
		labelstyle.alignment = TextAnchor.MiddleLeft;

		cur_y += lineHgt + 2*gap;








		// BASE PARAMETERS
		AXParameter b = null;


		EditorGUI.BeginChangeCheck();
		po.baseControls.isOpen = EditorGUI.Foldout(new Rect(x1, cur_y-20, 30,lineHgt), po.baseControls.isOpen, " ");
		if(EditorGUI.EndChangeCheck())
		{
			editor.doRepaint = true;  
		}

		if (po.baseControls.isOpen)
		{
		// Base (and other) controllers
		if (po.baseControls != null && po.baseControls.children !=null)
		{
			for (int i=0; i<po.baseControls.children.Count; i++) {
				
				b =  po.baseControls.children[i] as AXParameter;

				if (b.PType != AXParameter.ParameterType.None && b.PType != AXParameter.ParameterType.Base)
					continue;
							
				// these points are world, not relative to the this GUIWindow
				b.inputPoint 	= new Vector2( po.rect.x, 					po.rect.y+cur_y+lineHgt/2);
				b.outputPoint 	= new Vector2( po.rect.x+po.rect.width, 	po.rect.y+cur_y+lineHgt/2);
				
				Rect pRect = new Rect(x1, cur_y, innerWidth, lineHgt);

				try {
					int hgt = ParameterGUI.OnGUI(pRect, editor, b);
					cur_y += hgt + gap;
				} catch {
					
				} 
			}
		}

		cur_y += lineHgt + 2*gap;
		}







		// CUSTOM NODE GUI ZONE_1
		if (generatorHandler != null)
			cur_y = generatorHandler.customNodeGUIZone_1(cur_y, editor, po);







		// DO INPUTS
		ArchimatixUtils.cur_x = ArchimatixUtils.indent;

		po.assertInputControls();

		if (po.inputControls != null || (po.useSplineInputs && po.splineInputs != null))
		{



			// INPUT_CONTROL CONNECT BUTTON 
			if (! (po.generator is ShapeDistributor))
			{

				if ( po.inputControls != null && ! po.inputControls.isOpen )
				{
					if (GUI.Button (new Rect (-3, cur_y, lineHgt, lineHgt), "")) {

                           
                        if (editor.OutputParameterBeingDragged != null)
						{

                            if (editor.OutputParameterBeingDragged.parametricObject.is2D())
							{
								if (po.shapes != null && po.shapes.Count > 0)
								{
									// for now, just add to first shape
									AXParameter newp = po.shapes[0].addInput();
									newp.inputPoint = new Vector2( po.rect.x, po.rect.y+cur_y+lineHgt/2);
										newp.makeDependentOn(editor.OutputParameterBeingDragged);
										editor.OutputParameterBeingDragged = null;
									model.autobuild();
								}
								else 
								{
									List<AXParameter> inputs = po.getAllInputShapes();
									if (inputs.Count == 1)
									{
											inputs[0].makeDependentOn(editor.OutputParameterBeingDragged);
										inputs[0].inputPoint = new Vector2( po.rect.x, po.rect.y+cur_y+lineHgt/2);
											editor.OutputParameterBeingDragged = null;
										model.autobuild();
										if (po.geometryControls != null)
											po.geometryControls.isOpen = true;
									}
									else
									{
											editor.inputsInputSocketClicked (po);
									}
								}

							}
								else if (editor.OutputParameterBeingDragged.parametricObject.is3D())
							{
								if (po.generator is Grouper)
								{
									AXParameter new_p = po.addInputMesh();
										new_p.makeDependentOn(editor.OutputParameterBeingDragged);
										editor.OutputParameterBeingDragged = null;
									model.autobuild();
								}
								else 
								{
										editor.inputsInputSocketClicked (po);
								}
							}
						}
						else
								editor.inputsInputSocketClicked (po);

					}
				}
			}



			// INPUT_CONTROLS FOLDOUT TRINGLE


			if (po.generator is ShapeDistributor)
				po.inputControls.isOpen = true;


			if (! (po.generator is ShapeDistributor))
			{
				EditorGUI.BeginChangeCheck();
				po.inputControls.isOpen = EditorGUI.Foldout(new Rect(x1, cur_y, 30,lineHgt), po.inputControls.isOpen, " ");
				if(EditorGUI.EndChangeCheck())
				{
						editor.doRepaint = true;  
				}
			}
			//GUI.color = Color.white;


			// INPUT_CONTROLS FOLDOUT BUTTON
			if (! (po.generator is ShapeDistributor))
			{

				if (GUI.Button(new Rect(x1+10, cur_y, 100,lineHgt), "Inputs"))
				{
					if (! po.inputControls.isOpen)
					{
						//po.closeParameterSets();
						po.inputControls.isOpen = true;
					}
					else
						po.inputControls.isOpen = false;
				}
				po.inputControls.inputPoint 		= new Vector2( po.rect.x, 					po.rect.y+cur_y+lineHgt/2);
				po.inputControls.outputPoint 		= new Vector2( po.rect.x+po.rect.width, 	po.rect.y+cur_y+lineHgt/2);



				if (! po.inputControls.isOpen)
				{
					if (po.shapes != null)
					{

						foreach(AXShape shp in po.shapes)
						{
							foreach(AXParameter sp in  shp.inputs)
								sp.inputPoint 	=  new Vector2( po.rect.x, 					po.rect.y+cur_y+lineHgt/2);
						}
					}


				}

				cur_y += lineHgt + 2*gap;
			}






			if (po.inputControls.isOpen)
			{


				// INPUT SHAPES
				Color backgroundColor = GUI.backgroundColor;
				//Archimatix.indentLevel++;
				if (po.shapes != null)
				{
					// A SHAPE -- move this into AXShape.OnGUI()
					foreach(AXShape shp in po.shapes)
					{
						Rect pRect = new Rect(winMargin+ArchimatixUtils.indent, cur_y, innerWidth, lineHgt);
						cur_y = ShapeGUI.OnGUI(pRect, editor, shp);
					}
				}
				//Archimatix.indentLevel--;
				GUI.backgroundColor = backgroundColor;





				// INPUT ITEMS
				// Parameter Lines

				//for (int i=0; i<po.parameters.Count; i++) {
				if ((po.inputControls != null && po.inputControls.children != null))
				{
					for (int i=0; i<po.inputControls.children.Count; i++) {
						p =  (AXParameter) po.inputControls.children[i];


						if (p.PType != AXParameter.ParameterType.Input)
							continue;


						//if ( p.DependsOn != null && !p.DependsOn.Parent.isOpen && ! p.Name.Contains ("External"))
						//	continue;


						//if (parametricObjects_Property != null) 
						if (model.parametricObjects != null) 
						{
							// these points are world, not rlative to the this GUIWindow
							p.inputPoint 	= new Vector2( po.rect.x, 					po.rect.y+cur_y+lineHgt/2);
							p.outputPoint 	= new Vector2( po.rect.x+po.rect.width, 	po.rect.y+cur_y+lineHgt/2);


							Rect pRect = new Rect(x1, cur_y, innerWidth, 2*lineHgt);

							try {
								if (p.Type == AXParameter.DataType.Spline || p.Type == AXParameter.DataType.Curve3D)
									cur_y = ParameterSplineGUI.OnGUI_Spline(pRect, editor, p);

								else if	(p.Type == AXParameter.DataType.Generic)
								{						
									int hgt = ParameterGUI.OnGUI( pRect, editor, p);
									cur_y += hgt + gap;

								}
								else if (p.Type == AXParameter.DataType.Mesh)
									cur_y = ParameterMeshGUI.OnGUI_Mesh(pRect, editor, p);

								else if (p.Name.Contains("Material") || (p.DependsOn != null && p.DependsOn.parametricObject.generator is MaterialTool))
									cur_y = ParameterToolGUI.display(pRect, editor, p);
								else if (p.Name.Contains("Repeater"))
									cur_y = ParameterToolGUI.display(pRect, editor, p);
                                    else if (p.Name.Contains("Jitter"))
                                        cur_y = ParameterToolGUI.display(pRect, editor, p);
                                    else if (p.Name.Contains("Image"))
                                        cur_y = ParameterToolGUI.display(pRect, editor, p);
                                    else
                                    {
									
									cur_y += ParameterGUI.OnGUI(pRect, editor, p) + gap;
								}
							} 
							catch
							{
								//Debug.Log ("INPUT : " + p.Name + " FAILED");
							}

							if (po.generator is RepeaterBase)
							{

							}


						}
					}
				}






				// SPLINE INPUT LIST

				if (  po.useSplineInputs && po.splineInputs != null)
				{			

					// Empty / New SHAPE PARAMETER

					GUI.backgroundColor = editor.getDataColor(AXParameter.DataType.Spline);

					if (editor.OutputParameterBeingDragged != null && editor.OutputParameterBeingDragged.Type != AXParameter.DataType.Spline)
					{
						GUI.enabled = false;
					}
					if (GUI.Button (new Rect (-3, cur_y, lineHgt, lineHgt), "")) {

                            AXParameter new_p = po.addInputSpline();
							editor.inputSocketClicked (new_p);
							editor.OutputParameterBeingDragged = null;

					}
					GUI.enabled = true;

					Rect boxRect = new Rect(x1+10, cur_y, innerWidth-20,lineHgt);//new Rect (x1 +11, cur_y, width - 38, lineHgt);
					GUI.Box (boxRect, " ");
					GUI.Box (boxRect, " ");
					//boxRect.x += 10;
					GUI.Label (boxRect, "Empty Input");


					cur_y +=  lineHgt + gap;

				}






				// MESH INPUT LIST

				if ( po.useMeshInputs && po.meshInputs != null && ! po.isCurrentGrouper())
				{			

					// Empty / New SHAPE PARAMETER

						GUI.backgroundColor = editor.getDataColor(AXParameter.DataType.Mesh);

						if (editor.OutputParameterBeingDragged != null && editor.OutputParameterBeingDragged.Type != AXParameter.DataType.Mesh)
					{
						GUI.enabled = false;
					}
					if (GUI.Button (new Rect (-3, cur_y, lineHgt, lineHgt), "")) {

						AXParameter new_p = po.addInputMesh();
							editor.inputSocketClicked (new_p);
							editor.OutputParameterBeingDragged = null;

					}
					GUI.enabled = true;

					Rect boxRect = new Rect(x1+10, cur_y, innerWidth-20,lineHgt);//new Rect (x1 +11, cur_y, width - 38, lineHgt);
					GUI.Box (boxRect, " ");
					GUI.Box (boxRect, " ");
					//boxRect.x += 10;
					GUI.Label (boxRect, "Empty Input");


					cur_y +=  lineHgt + gap;

				}

				cur_y +=  lineHgt + gap;
			}

		}





		//GUI.backgroundColor = Color.magenta; 







		// CUSTOM NODE GUI ZONE_2
		if (generatorHandler != null)
			cur_y = generatorHandler.customNodeGUIZone_2(cur_y, editor, po);







		// DO TRANSFORMATIONS
		if (! (po.generator is ShapeDistributor))
		{

			GUI.color = Color.white;
			GUI.backgroundColor = Color.white;

			if (po.positionControls != null)
			{
				EditorGUI.BeginChangeCheck();


				po.positionControls.isOpen = EditorGUI.Foldout(new Rect(x1, cur_y, 30,lineHgt), po.positionControls.isOpen, " ");
				if(EditorGUI.EndChangeCheck())
				{
						editor.doRepaint = true;
				}
				//GUI.color = Color.white;





				// TRANSFORMATIONS BUTTON

				GUI.backgroundColor = Color.clear;
				buttonStyle.alignment = TextAnchor.LowerLeft;
				if (GUI.Button(new Rect(x1+10, cur_y, 80,lineHgt+1), "Transform"))
				{
					if (! po.positionControls.isOpen)
					{
						if (po.geometryControls != null)
							po.geometryControls.isOpen = false;

						po.positionControls.isOpen = true;
					}
					else
						po.positionControls.isOpen = false;
				}

				GUI.backgroundColor = Color.white;
				buttonStyle.alignment = TextAnchor.MiddleLeft;



				// SWITCH AXIS BUTTON
				int axisInt =  (int) po.generator.axis;
				//Debug.Log(po.Name + ": " + axisInt + " -- " + po.generator.axis);



				buttonStyle.alignment = TextAnchor.MiddleCenter;
				if (GUI.Button(new Rect(x1+90, cur_y, 50,lineHgt), "" + po.generator.axis))
				{
					int next = (axisInt == 6) ? 0 : axisInt+1;
					po.intValue("Axis", next);
					model.autobuild();
					po.generator.adjustWorldMatrices();
						
				}
				buttonStyle.alignment = TextAnchor.MiddleLeft;

				po.positionControls.inputPoint 		= new Vector2( po.rect.x, 					po.rect.y+cur_y+lineHgt/2);
				po.positionControls.outputPoint 	= new Vector2( po.rect.x+po.rect.width, 	po.rect.y+cur_y+lineHgt/2);




				cur_y +=  lineHgt + gap;

				EditorGUI.BeginChangeCheck();
				if (po.positionControls.isOpen)
				{
					cur_y +=  gap;

					for (int i=0; i<po.positionControls.children.Count; i++) {

						p =  (AXParameter) po.positionControls.children[i];

						//if (po.parameters[i].PType != AXParameter.ParameterType.PositionControl)
						//	continue;

						// these points are world, not rlative to the this GUIWindow
						p.inputPoint 	= new Vector2( po.rect.x, 					po.rect.y+cur_y+lineHgt/2);
						p.outputPoint 	= new Vector2( po.rect.x+po.rect.width, 	po.rect.y+cur_y+lineHgt/2);

						//if (parameters_Property != null && parameters_Property.arraySize > i)
						if (p != null )
						{
							int hgt = ParameterGUI.OnGUI(new Rect(x1, cur_y, innerWidth, lineHgt), editor, p);
							cur_y += hgt + gap;
						}
					}
				}
				if(EditorGUI.EndChangeCheck())
				{

				}

			}

		}


	

		float thumbSize = 16;

		if (po.generator is PrefabInstancer)
		{
			EditorGUI.BeginChangeCheck ();

			po.prefab = (GameObject) EditorGUI.ObjectField(new Rect(x2, cur_y, innerWidth-2*thumbSize, lineHgt), po.prefab, typeof(GameObject), true);
			if (EditorGUI.EndChangeCheck ()) {
				Undo.RegisterCompleteObjectUndo (model, "Prefab");

				model.autobuild();

			}
			cur_y += lineHgt + gap;
		}


        if (po.generator is TextureTool)
        {
            EditorGUI.BeginChangeCheck();

            
            po.imageData = (Texture2D)EditorGUI.ObjectField(new Rect(x2, cur_y, innerWidth - 2 * thumbSize, lineHgt), po.imageData, typeof(Texture2D), true);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(model, "TextureSelection");

                model.autobuild();

            }
            cur_y += lineHgt + gap;
        }








            // TEXTURES

            // MATERIAL


            //if ((po.is3D() && ! po.generator.isOfInterface("IReplica")) || po.generator is MaterialTool)
            // MATERIAL SELECTION
            if (po.generator is MaterialTool)
		{
			

			if (po.axMat == null) 
				po.axMat = new AXMaterial();

			if (po.axMat.mat == null && po.Mat != null)
				po.axMat.mat = po.Mat;

			// MATERIAL OBJECT FIELD
			EditorGUI.BeginChangeCheck ();
			po.axMat.mat = (Material) EditorGUI.ObjectField(new Rect(x2+5, cur_y, innerWidth-2*thumbSize, lineHgt), po.axMat.mat, typeof(Material), true);
			if (EditorGUI.EndChangeCheck ()) {
				Undo.RegisterCompleteObjectUndo (model, "Material");
				model.remapMaterialTools();
				model.autobuild();  
			}
				if(po.axMat.mat != null && po.axMat.mat.HasProperty("_MainTex") && po.axMat.mat.mainTexture != null)
					EditorGUI.DrawTextureTransparent(new Rect(x2+(innerWidth-2*thumbSize), cur_y, thumbSize, thumbSize),   po.axMat.mat.mainTexture, ScaleMode.ScaleToFit, 1.0F);
			
			cur_y += lineHgt + gap;


                 
			// PHYSICS MATERIAL
			po.axMat.showPhysicsDefaults = EditorGUI.Foldout(new Rect(x1, cur_y, 30,lineHgt), po.axMat.showPhysicsDefaults, "Physics", true);
			cur_y += lineHgt + gap;

			if (po.axMat.showPhysicsDefaults)
			{

				po.axMat.physMat = (PhysicMaterial) EditorGUI.ObjectField(new Rect(x2+5, cur_y, innerWidth-2*thumbSize, lineHgt), po.axMat.physMat, typeof(PhysicMaterial), true);
				cur_y += lineHgt + gap;


				EditorGUIUtility.labelWidth = 50;
				//labelstyle.fixedWidth = 100;
				//labelstyle.alignment = 	TextAnchor.MiddleLeft;
										

					
				EditorGUI.BeginChangeCheck ();
				po.axMat.density = EditorGUI.FloatField(new Rect(x2+5, cur_y, 100,lineHgt), "Density",  po.axMat.density);

				if (EditorGUI.EndChangeCheck ()) {
				Undo.RegisterCompleteObjectUndo (model, "Material Density");
					model.autobuild();  
				}
				cur_y += lineHgt + gap;

			}
		}



		// TERRAIN INPUT
		if (po.generator is RepeaterBase || po.generator is PlanRepeater || po.generator is TerrainDeformer || po.generator is ITerrainer)
		{
			EditorGUI.BeginChangeCheck ();
		
			po.terrain = (Terrain) EditorGUI.ObjectField(new Rect(x2, cur_y, innerWidth-2*thumbSize, lineHgt), po.terrain, typeof(Terrain), true);
			if (EditorGUI.EndChangeCheck ()) {
				Undo.RegisterCompleteObjectUndo (model, "Terrain");

				model.autobuild();

			}
			cur_y += lineHgt + gap;

			if (po.generator is Lander)
			{

				if (GUI.Button(new Rect(x2, cur_y, innerWidth-2*thumbSize, lineHgt), "Memorize"))
				{
					Debug.Log("Memorize");

					((Lander) po.generator).memorizeTerrain();
				}
				cur_y +=  lineHgt + gap;
			}

		}

		cur_y +=  gap;






		// CUSTOM NODE GUI ZONE_3
		if (generatorHandler != null)
			cur_y = generatorHandler.customNodeGUIZone_3(cur_y, editor, po);




		// DO GEOMETRY CONTROLS
		if ( ! (po.generator is Instance))
		{

			if (po.geometryControls != null)
			{
				// FOLDOUT 

				if (po.geometryControls.children.Count == 1)
					po.geometryControls.isOpen = true;
				else
				{
					EditorGUI.BeginChangeCheck();
						po.geometryControls.isOpen = EditorGUI.Foldout(new Rect(x1, cur_y, 30,lineHgt), po.geometryControls.isOpen, " ");
					if(EditorGUI.EndChangeCheck())
					{
							editor.doRepaint = true;
					}
				//GUI.color = Color.white;



					// GEOMETRY BUTTON

					GUI.backgroundColor = Color.clear;
					buttonStyle.alignment = TextAnchor.LowerLeft;
					if (GUI.Button(new Rect(x1+10, cur_y, 100, lineHgt+1), ((po.is2D()) ? "Geometry":"Controls")))
					{
						if (! po.geometryControls.isOpen)
						{
							if (po.positionControls != null)
								po.positionControls.isOpen = false;
							po.geometryControls.isOpen = true;
						}
						else
							po.geometryControls.isOpen = false;
					}
					GUI.backgroundColor = Color.white;
					buttonStyle.alignment = TextAnchor.MiddleLeft;


					po.geometryControls.inputPoint 		= new Vector2( po.rect.x, 					po.rect.y+cur_y+lineHgt/2);
					po.geometryControls.outputPoint 	= new Vector2( po.rect.x+po.rect.width, 	po.rect.y+cur_y+lineHgt/2);

					cur_y +=  lineHgt+2*gap;
				}


				// CONTROL PARAMETERS
				if ( (po.geometryControls != null && po.geometryControls.isOpen)   || po.revealControls)
				{



					// GEOMETRY_CONTROLS_ON_GUI

					//cur_y = gh.GeometryControlsOnGUI(cur_y, editor);
					cur_y = GeometryControlsGui.GeometryControlsOnGUI(cur_y, editor, po);

                      


                    // DO HANDLES
                        if (po.is2D() || po.generator is Grouper || po.generator is IHandles)
					{
						EditorGUI.BeginChangeCheck();
							currColor = GUI.color;
						GUI.color = new Color(1,1,1,.6f);
						po.showHandles = EditorGUI.Foldout(new Rect(x1, cur_y, 100,lineHgt), po.showHandles, "Handles");
						GUI.color = currColor;
						if(EditorGUI.EndChangeCheck()) 
								editor.doRepaint = true;

						cur_y +=  lineHgt;
						AXHandle han;
						if (po.showHandles)
						{
							//po.showSubpartHandles = EditorGUI.Toggle(new Rect(x2, cur_y, po.rect.width*1.3f, lineHgt), new GUIContent("Show handles:", "Allow the direct editing of subparts."), po.showSubpartHandles);

							cur_y += gap*2;

							if (po.handles == null)
								po.handles = new List<AXHandle>();

							for (int i=0; i<po.handles.Count; i++) {
								han =  po.handles[i];
								if (han != null)
								{
									Rect pRect = new Rect(x2, cur_y, innerWidth,lineHgt);

									// HANDLE
									EditorGUI.BeginChangeCheck();
									int hgt = AXHandleGUI.OnGUI(han, pRect, editor);
									if(EditorGUI.EndChangeCheck())
									{
										Undo.RegisterCompleteObjectUndo (model, "Handle Edit");

										Event.current.Use ();
										if (po.rect.width < 250)
											po.rect.width = 250;

									}
									cur_y += hgt + gap;
								}
							}

							if (GUI.Button(new Rect(x2, cur_y, lineHgt*1.25f, lineHgt), new GUIContent("+", "Create a new Handle")))
							{
								Undo.RegisterCompleteObjectUndo (model, "New AXHandle");
								//Debug.Log ("ADDING A HANDLE");
								AXHandle tmpH = po.addHandle ();

								tmpH.isEditing = true;
								po.model.cleanGraph();
								tmpH.Name = "";
							}
							if (po.isEditing)
							{
								if (GUI.Button (new Rect(x1+editButtonWid+6, cur_y, editButtonWid,lineHgt), "Done" ))
									po.doneEditing();



							}
							cur_y += lineHgt + gap + 5;

						}
					}

					/*
					if (EditorGUIUtility.isProSkin)
					{
						GUI.color = Color.Lerp(po.generator.GUIColorPro, Color.white, .7f);//po.generator.GUIColorPro;
						GUI.backgroundColor = Color.Lerp(po.generator.GUIColorPro, Color.white, .7f) ;
					}
					else
					{
						GUI.color = Color.Lerp(po.generator.GUIColor, Color.white, .8f);
						GUI.backgroundColor = Color.Lerp(po.generator.GUIColor, Color.white, .6f) ;

					}
					*/

				}

			}





			// LOGIC WINDOW
			GUI.backgroundColor = origBG;
			//GUI.skin.settings.selectionColor = new Color(.7f,.7f,.7f);
			if (!  (po.generator is FreeCurve) &&    ((po.geometryControls != null && po.geometryControls.isOpen )  || po.revealControls || (po.generator is ILogic)))
			{
				

				// DO LOGIC
				if (po.is2D() || (po.generator is Channeler))
				{
					

					GUIStyle codestyle = EditorStyles.textField;
					codestyle.fontSize  = 12;
					int codeLineHgt = 14;

					if ((Event.current.type ==  EventType.KeyDown) && GUI.GetNameOfFocusedControl().Contains("logicTextArea_") && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter))
					{
						po.codeScrollPosition.y += codeLineHgt;

					}





					EditorGUI.BeginChangeCheck();
					po.showLogic = EditorGUI.Foldout(new Rect(x1, cur_y, 100,lineHgt), po.showLogic, "Logic");
					if(EditorGUI.EndChangeCheck())
					{
							editor.doRepaint = true;
						if (po.showLogic && po.rect.width < 250)
							po.rect.width = 250;

					}

					if(po.showLogic)
					{


						// REFRESH BUTTON
	//					if (po.showLogic && po.codeIsDirty == true) 
	//					{
							GUI.SetNextControlName("RefreshButton_" + po.Guid);
							if (GUI.Button(new Rect(innerWidth-100, cur_y, 100, lineHgt), "Refresh")  )  
							{	

								po.codeIsDirty = false;
								model.autobuild();
							}

						//}


						 
						cur_y += lineHgt + gap;
						//Rect codeRect = GUILayoutUtility.GetLastRect();


						if (! string.IsNullOrEmpty(po.codeWarning))
						{
							//Debug.Log ("CODE WARNING: "+po.Name + " ... " + po.codeWarning);
							GUI.Label(new Rect(x1, cur_y, po.rect.width,lineHgt), po.codeWarning);
							cur_y += lineHgt + gap;
						}

						//Rect logicRect;

						//TEXTAREA

						if (po.codeWindowHeight < 100 && po.showLogic) 
							po.codeWindowHeight = 100;

						int codeLineCount = po.codeLineCount();
						int codeHgt = (codeLineCount > 6) ? codeLineCount*codeLineHgt : 6*codeLineHgt;

						//if (codeHgt < po.codeWindowHeight) 
						//	codeHgt = po.codeWindowHeight;

						//if (po.codeWindowHeight  < codeHgt) 
						//	po.codeWindowHeight = codeHgt;

						//Debug.Log("po.codeWindowHeight="+po.codeWindowHeight);
						Rect codeScrollWindowRect = new Rect(po.rect.x+x1, po.rect.y+cur_y, innerWidth, po.codeWindowHeight);

						// SCROLLVIEW
						//GUI.BeginGroup(new Rect(x1, cur_y, innerWidth, po.codeWindowHeight));

						editor.mostRecentLogicRectLocal = new Rect(x1, cur_y, innerWidth, po.codeWindowHeight);
						po.codeWindowRectLocal = new Rect(x1, cur_y, innerWidth, po.codeWindowHeight);

						GUI.SetNextControlName("logicTextArea_Text_" + po.Guid);
						po.codeScrollPosition = GUI.BeginScrollView(new Rect(x1, cur_y, innerWidth, po.codeWindowHeight), po.codeScrollPosition, new Rect(0, 0, innerWidth-20, codeHgt));
						//po.codeScrollPosition = EditorGUILayout.BeginScrollView(po.codeScrollPosition);

						EditorGUI.BeginChangeCheck();
						GUI.SetNextControlName("logicTextArea_Text_Scrollview_" + po.Guid);

						// TEXTAREA
						po.code = EditorGUI.TextArea(new Rect(0, 5, innerWidth, Mathf.Max(po.codeWindowHeight-3, codeHgt)), po.code );

						//po.code = EditorGUILayout.TextArea(po.code,  GUILayout.Width(innerWidth) );
						if(EditorGUI.EndChangeCheck())  
						{
							Undo.RegisterCompleteObjectUndo (model, "Logic Text");

							// ** [complimets of S.Darkwell to fix copy and paste issue 2016.06.01]
							po.code = Regex.Replace(po.code, "[\r]","");
							// ** **
//
//
//							//Debug.Log("TEXT CHANGE");
//							// set this text area as hot rect not to generate onmouseup
//
//
								editor.mostRecentLogicRect = codeScrollWindowRect;
//							//Debug.Log (mostRecentLogicRect);
//							codeChanged = true;
							po.codeIsDirty = true;
						} 
						else
								editor.codeChanged = false;

						GUI.EndScrollView(); 
						//GUI.EndGroup();

						//logicRect = new Rect(x1, cur_y, innerWidth, lineHgt*20);


//						TextEditor te = GUIUtility.GetStateObject((TextEditor), GUIUtility.keyboardControl);
//						te.SelectCurrentParagraph();

						cur_y += po.codeWindowHeight + gap;


						//cur_y += lineHgt + gap;
					}
					// LOGIC


				}
			}
			// LOGIC AREA: WINDOW RESIZE BUTTON
			Rect codeResizeRect = new Rect(po.rect.width-x1-11, cur_y, x1, x1);
			if (e.type == EventType.MouseDown && codeResizeRect.Contains(e.mousePosition))
			{
				Undo.RegisterCompleteObjectUndo (model, "GUI LOGI Window Resize");
					editor.editorState = AXNodeGraphEditorWindow.EditorState.DragResizingLogicWindow;	
					editor.DraggingParameticObject = po;	

			}
				GUI.Button ( codeResizeRect, editor.resizeCornerTexture, GUIStyle.none);

			cur_y += lineHgt + gap;

		}

		cur_y += gap;





		// CUSTOM NODE GUI ZONE_4
		if (generatorHandler != null)
			cur_y = generatorHandler.customNodeGUIZone_4(cur_y, editor, po, x1, innerWidth );









		// DO OUPUT GENERATED

		// Parameter Line



		// SHAPE MERGER SHAPES

		//Archimatix.indentLevel++;
		if (po.shapes != null)
		{
			//Rect lRect = new Rect(x1+11, cur_y, 50 , lineHgt);

			// A SHAPE -- move this into AXShape.OnGUI()
			foreach(AXShape shp in po.shapes)
			{
				Rect pRect = new Rect(winMargin+ArchimatixUtils.indent, cur_y, innerWidth, lineHgt);
				cur_y = ShapeGUI.displayOutput(pRect, editor, shp);
			}
		}
		//Archimatix.indentLevel--;
		//GUI.backgroundColor = backgroundColor;


		//for (int i=0; i<po.parameters.Count; i++) {



		if (po.outputsNode != null)
		{
			for (int i=0; i<po.outputsNode.children.Count; i++) {

				p =  (AXParameter) po.outputsNode.children[i];

				if (p == null)
					continue;

				//if (p.hasInputSocket || ! p.hasOutputSocket)
				if (p.PType != AXParameter.ParameterType.Output)
					continue;


				//if (parametricObjects_Property != null)
				if (model.parametricObjects != null)
				{
					// these points are world, not relative to the this GUIWindow
					p.inputPoint 	= new Vector2( po.rect.x, 					po.rect.y+cur_y+lineHgt/2);
					p.outputPoint 	= new Vector2( po.rect.x+po.rect.width, 	po.rect.y+cur_y+lineHgt/2);

					Rect pRect = new Rect(x1, cur_y, innerWidth, lineHgt);

					//if (parameters_Property.arraySize > i)
					if (po.parameters != null && po.parameters.Count > i)
					{
						int hgt = 0;

						if (po.is2D())
						{

							hgt = ParameterSplineGUI.OnGUI_Spline(pRect, editor, p);
							cur_y = hgt + gap;
						}
						else
						{
							hgt = ParameterGUI.OnGUI( pRect, editor, p);
							cur_y += hgt + gap;




							// PHYSICS

							if (p.Name == "Output Mesh" && ! (po.generator is IReplica))
							{
								float labelWidth = EditorGUIUtility.labelWidth;

								EditorGUIUtility.labelWidth = innerWidth-40;


								GUIStyle labelStyle = GUI.skin.GetStyle ("Label");
								TextAnchor prevTextAlignment = labelStyle.alignment;
								labelStyle.alignment = TextAnchor.MiddleLeft;


								// COMBINE MESHES
								Rect cntlRect = new Rect(x1+16, cur_y, innerWidth, lineHgt);
								EditorGUI.BeginChangeCheck ();
								po.combineMeshes = EditorGUI.Toggle (cntlRect, "Combine Meshes",  po.combineMeshes);
								if (EditorGUI.EndChangeCheck ()) {
									Undo.RegisterCompleteObjectUndo (model, "value change for " + po.Name);
									model.autobuild();
								} 
								labelStyle.alignment = prevTextAlignment;
								EditorGUIUtility.labelWidth = labelWidth;


								cur_y += hgt + gap;

								// SPRITE
								//cntlRect = new Rect(x1+16, cur_y, innerWidth, lineHgt);
								//EditorGUI.BeginChangeCheck ();
								//po.isSpriteGenerator = EditorGUI.Toggle (cntlRect, "Sprite Generator",  po.isSpriteGenerator);
								//if (EditorGUI.EndChangeCheck ()) {
								//	Undo.RegisterCompleteObjectUndo (model, "value change for " + po.Name);

								//	if (po.isSpriteGenerator)
								//		po.model.assertSpriteSupport();

								//	model.autobuild();
								//} 
								labelStyle.alignment = prevTextAlignment;
								EditorGUIUtility.labelWidth = labelWidth;


								cur_y += hgt + gap;





								po.showPhysics = EditorGUI.Foldout(new Rect(x1, cur_y, 30,lineHgt), po.showPhysics, " ");

								if (GUI.Button(new Rect(x1+10, cur_y, 100,lineHgt), "Physics"))
								{
									po.showPhysics = ! po.showPhysics;
								}


								if ( po.showPhysics)
								{

									cur_y += hgt + gap;


									cntlRect = new Rect(x1+16, cur_y, innerWidth-24, lineHgt);

									EditorGUIUtility.labelWidth = 50;
									EditorGUI.BeginChangeCheck ();
									po.colliderType = (ColliderType) EditorGUI.EnumPopup(cntlRect, "Collider ", po.colliderType);
									if (EditorGUI.EndChangeCheck ()) 
									{
										Undo.RegisterCompleteObjectUndo (model, "value change for " + po.Name + ":collidertype");
										model.autobuild();
									}

									cur_y += hgt + gap;





									// isRigidbody

									EditorGUIUtility.labelWidth = 70;

									cntlRect = new Rect(x1+16, cur_y, innerWidth-24, lineHgt);

									EditorGUI.BeginChangeCheck ();
									cntlRect = new Rect(x1+16, cur_y, innerWidth, lineHgt);
									po.isRigidbody = EditorGUI.Toggle (cntlRect, "Rigidbody",  po.isRigidbody);
									if (EditorGUI.EndChangeCheck ()) {
										Undo.RegisterCompleteObjectUndo (model, "value change for " + po.Name);
										if (po.isRigidbody && po.colliderType == ColliderType.Mesh)
											po.colliderType = ColliderType.ConvexMesh;
										model.autobuild();
									} 

									cur_y += hgt + gap;

									if (po.isRigidbody)
									{
									 

										/*
										cntlRect = new Rect(x1+16, cur_y, innerWidth-24, lineHgt);

										EditorGUI.BeginChangeCheck ();
										po.axMat.physMat = (PhysicMaterial) EditorGUI.ObjectField(cntlRect, po.axMat.physMat, typeof(PhysicMaterial), true);
										if (EditorGUI.EndChangeCheck ()) {
											Undo.RegisterCompleteObjectUndo (model, "PhysicMaterial");
											model.autobuild();
										}

										cur_y += hgt + gap;
										*/

										cntlRect = new Rect(x1+16, cur_y, innerWidth, lineHgt);

										EditorGUI.BeginChangeCheck ();
										po.isKinematic = EditorGUI.Toggle (cntlRect, "isKinematic",  po.isKinematic);

										if (EditorGUI.EndChangeCheck ()) {
											Undo.RegisterCompleteObjectUndo (model, "isKinematic");

											model.autobuild();
										}

										cur_y += hgt + gap;

										cntlRect = new Rect(x1+16, cur_y, innerWidth-24, lineHgt);

										EditorGUI.BeginChangeCheck ();
										cntlRect = new Rect(x1+16, cur_y, innerWidth, lineHgt);
										po.splitConcaveShapes = EditorGUI.Toggle (cntlRect, "Split Concave SHapes",  po.splitConcaveShapes);
										if (EditorGUI.EndChangeCheck ()) {
											Undo.RegisterCompleteObjectUndo (model, "Split concave shapes " + po.Name);
											model.autobuild();
										} 



									}

								}
							}
							cur_y += hgt + gap;



						}

					}
				}

			}
		}

		if (po.isEditing)
		{

			//GUILayout.FlexibleSpace();
			cur_y += lineHgt + gap;
			if (GUI.Button (new Rect(x1+3, cur_y, editButtonWid*2,lineHgt), "Delete Object"))
			{
				Undo.RegisterCompleteObjectUndo (model, "ParametricObject Delete");
				model.removeParametricObject(po);
			}
			cur_y += lineHgt + gap;

		}
		po.outputEndHeight = cur_y-gap;








		// DO THUMBNAIL / DROP_ZONE


	
		int bottomPadding = 55;
		int splineCanvasSize = (int)(po.rect.width-60);

		editor.mostRecentThumbnailRect = new Rect(x1, cur_y+lineHgt, innerWidth, innerWidth);

		Rect lowerRect = new Rect(0, cur_y-50, po.rect.width, po.rect.width);


		if (po.thumbnailState == ThumbnailState.Open)
		{


			if (po.is2D()) 
			{


				AXParameter output_p = po.generator.getPreferredOutputParameter(); 
				if  ( po.generator.hasOutputsReady() )
				{

					if (ArchimatixEngine.nodeIcons.ContainsKey("Blank"))
							EditorGUI.DrawTextureTransparent(editor.mostRecentThumbnailRect, ArchimatixEngine.nodeIcons["Blank"], ScaleMode.ScaleToFit, 1.0F);

					Color color = po.thumbnailLineColor;

					if (color.Equals(Color.clear))
						color = Color.magenta;

					
					GUIDrawing.DrawPathsFit(output_p, new Vector2(po.rect.width/2, cur_y+po.rect.width/2 ), po.rect.width-60, ArchimatixEngine.AXGUIColors["ShapeColor"]);

				}
				else if (ArchimatixEngine.nodeIcons.ContainsKey(po.GeneratorTypeString.ToString()))
						EditorGUI.DrawTextureTransparent(editor.mostRecentThumbnailRect, ArchimatixEngine.nodeIcons[po.GeneratorTypeString], ScaleMode.ScaleToFit, 1.0F);


			} 

			else
			{
						
					//Debug.Log (po.Name + " po.renTex.IsCreated()="+po.renTex.IsCreated());

				//Debug.Log (po.renTex + " :::::::::::::::::::::::--::




				if (po.generator is PrefabInstancer && po.prefab != null)
				{
					Texture2D thumber = AssetPreview.GetAssetPreview(po.prefab);
					if (e.type == EventType.Repaint)
					{
						if (thumber != null)
								EditorGUI.DrawTextureTransparent(editor.mostRecentThumbnailRect,    thumber, ScaleMode.ScaleToFit, 1.0F);
						else
								EditorGUI.DrawTextureTransparent(editor.mostRecentThumbnailRect, ArchimatixEngine.nodeIcons[po.GeneratorTypeString], ScaleMode.ScaleToFit, 1.0F);

					}

				}
                if (po.generator is TextureTool && po.imageData != null)
                {
                    Texture2D thumber = po.imageData;
                    if (e.type == EventType.Repaint)
                    {
                        if (thumber != null)
                            EditorGUI.DrawTextureTransparent(editor.mostRecentThumbnailRect, thumber, ScaleMode.ScaleToFit, 1.0F);
                        else
                            EditorGUI.DrawTextureTransparent(editor.mostRecentThumbnailRect, ArchimatixEngine.nodeIcons[po.GeneratorTypeString], ScaleMode.ScaleToFit, 1.0F);

                    }

                }
                else if ( ((po.Output != null && po.Output.meshes != null && po.Output.meshes.Count > 0) || po.generator is MaterialTool) && (po.renTex != null || po.thumbnail != null)   )
				//if ( po.thumbnail != null )   
				{
					//Debug.Log(po.Name + " thumb " + po.renTex);
					if (e.type == EventType.Repaint)
					{
						//GUI.DrawTexture(editor.mostRecentThumbnailRect,    po.renTex, ScaleMode.ScaleToFit, true, 1.0F);
						EditorGUI.DrawTextureTransparent(editor.mostRecentThumbnailRect,    po.renTex, ScaleMode.ScaleToFit, 1.0F);

						// DROP ZONE

						if (po.generator is Grouper && editor.editorState == AXNodeGraphEditorWindow.EditorState.DraggingNodePalette && editor.mouseIsDownOnPO != po && po != model.currentWorkingGroupPO)
						{
								if (editor.mostRecentThumbnailRect.Contains(e.mousePosition))
							{
								
									GUI.DrawTexture(editor.mostRecentThumbnailRect,    editor.dropZoneOverTex, ScaleMode.ScaleToFit, true, 1.0F);
									editor.OverDropZoneOfPO = po;
							}
							else
									GUI.DrawTexture(editor.mostRecentThumbnailRect,    editor.dropZoneTex, ScaleMode.ScaleToFit, true, 1.0F);
						}


					}
					//else
					//	GUI.DrawTexture(editor.mostRecentThumbnailRect, po.thumbnail, ScaleMode.ScaleToFit, false, 1.0F);



					if (editor.mostRecentThumbnailRect.Contains(e.mousePosition) || editor.draggingThumbnailOfPO == po)
					{
						Rect orbitButtonRect = new Rect(x1+innerWidth-16-3, cur_y+lineHgt+3, 16, 16);

						if (e.command || e.control)
								GUI.DrawTexture(orbitButtonRect, editor.dollyIconTex);
						else
								GUI.DrawTexture(orbitButtonRect, editor.orbitIconTex);


						if (e.type == EventType.MouseDown && orbitButtonRect.Contains(e.mousePosition))
						{
							model.selectOnlyPO(po);
								editor.draggingThumbnailOfPO = po;
							e.Use();
						}

					}	


				}
				else if (ArchimatixEngine.nodeIcons.ContainsKey(po.GeneratorTypeString.ToString()))
				{ 
						EditorGUI.DrawTextureTransparent(editor.mostRecentThumbnailRect, ArchimatixEngine.nodeIcons[po.GeneratorTypeString], ScaleMode.ScaleToFit, 1.0F);

					// DROP ZONE

						if (po.generator is Grouper && editor.editorState == AXNodeGraphEditorWindow.EditorState.DraggingNodePalette && editor.mouseIsDownOnPO != po && po != model.currentWorkingGroupPO)
					{
							if (editor.mostRecentThumbnailRect.Contains(e.mousePosition))
						{
								EditorGUI.DrawTextureTransparent(editor.mostRecentThumbnailRect,    editor.dropZoneOverTex, ScaleMode.ScaleToFit, 1.0F);
							editor.OverDropZoneOfPO = po;
						}
						else
								EditorGUI.DrawTextureTransparent(editor.mostRecentThumbnailRect,    editor.dropZoneTex, ScaleMode.ScaleToFit, 1.0F);
					}


				}

			}

			cur_y += lineHgt + bottomPadding + splineCanvasSize+  gap;

			po.rect.height = cur_y ;
			cur_y += 4*gap;
		}
		else
		{
			// no thumbnail
			cur_y += 2*lineHgt;
			po.rect.height = cur_y ;
		}










		// FOOTER //





		// STATS
		if (po.stats_VertCount > 0 || po.generator is MaterialTool)
		{

			string statsText;

			if (po.generator is MaterialTool)
			{
				statsText = (po.generator as MaterialTool).texelsPerUnit.ToString("F0") + " Texels/Unit";
			}
			else
			{
				statsText = po.stats_VertCount + " verts";

				if (po.stats_TriangleCount > 0)
					statsText += ", " + po.stats_TriangleCount + " tris";
			}

			GUIStyle statlabelStyle = GUI.skin.GetStyle ("Label");
			TextAnchor prevStatTextAlignment = statlabelStyle.alignment;
			statlabelStyle.alignment = TextAnchor.MiddleLeft;
			EditorGUIUtility.labelWidth = 500;
			GUI.Label(new Rect(10, po.rect.height-x2+2, 500, lineHgt), statsText);
			statlabelStyle.alignment = prevStatTextAlignment;
		}





		if (e.type == EventType.MouseDown &&  lowerRect.Contains(e.mousePosition) )  
		{
				editor.clearFocus();
			GUI.FocusWindow(po.guiWindowId);
		}

		// WINDOW RESIZE
		Rect buttonRect = new Rect(po.rect.width-16, po.rect.height-17, 14, 14);
		if (e.type == EventType.MouseDown && buttonRect.Contains(e.mousePosition))
		{
			Undo.RegisterCompleteObjectUndo (model, "GUI Window Resize");

				editor.editorState = AXNodeGraphEditorWindow.EditorState.DragResizingNodePalleteWindow;	
				editor.DraggingParameticObject = po;		

		}
		//GUI.Button ( buttonRect, "∆", GUIStyle.none);
			GUI.Button ( buttonRect, editor.resizeCornerTexture, GUIStyle.none);


		if (e.type == EventType.MouseDown && buttonRect.Contains(e.mousePosition))
		{
		}

		//cur_y += lineHgt + gap;

		// Window title bar is the dragable area
		//GUI.DragWindow(headerRect);

			if (editor.draggingThumbnailOfPO == null || editor.draggingThumbnailOfPO != po)
		{
			

			GUI.DragWindow();

		}

		GUI.backgroundColor = origBG;


	}


	}

}

