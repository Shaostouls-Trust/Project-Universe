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

	public class AXNodeGraph_NodeMini
	{

		
		
		// return the height of this gui area
		public static void OnGUI (int win_id, AXNodeGraphEditorWindow editor, AXParametricObject po)
		{
			
					
			Event e = Event.current;

			AXModel model = editor.model;


			AXParameter p;

			string buttonLabel;
			Rect buttonRect;

			Color inactiveColor = new Color(.7f,.7f,.7f);


			Color oldBackgroundColor = GUI.backgroundColor;



			// START LAYOUT OF INNER PALETTE


			// Horizontal layput
			float winMargin = ArchimatixUtils.indent;
			float innerWidth = po.rect.width - 2 * winMargin;



			int x1 = 10;
			int x2 = 20;

			// vertical layut
			int cur_y = 25;
			int gap = ArchimatixUtils.gap;
			int lineHgt = ArchimatixUtils.lineHgt;


//		if (EditorGUIUtility.isProSkin)
//		{
//			GUI.color = po.generator.GUIColorPro;
//			GUI.backgroundColor = Color.Lerp(po.generator.GUIColorPro, Color.white, .5f) ;
//		}
//		else
//		{
//			GUI.color = po.generator.GUIColor;
//			GUI.backgroundColor = Color.Lerp(po.generator.GUIColor, Color.white, .5f) ;
//
//		}



			// DRAW HIGHLIGHT AROUND SELECTED NODE PALETTE
			if (model.isSelected (po)) {
				float pad = (EditorGUIUtility.isProSkin) ? 1 : 1;
				Rect outline = new Rect (0, 0, po.rect.width - pad, po.rect.height - pad);
				Handles.color = Color.white;

				Handles.DrawSolidRectangleWithOutline (outline, new Color (1, 1, 1, 0f), ArchimatixEngine.AXGUIColors ["NodePaletteHighlightRect"]);
				//Handles.DrawSolidRectangleWithOutline(outline, new Color(.1f, .1f, .3f, .05f), new Color(.1f, .1f, .8f, 1f));
			}


			// TITLE

			if (GUI.Button (new Rect (x1, cur_y, innerWidth - lineHgt * 2 - 6, lineHgt * 2), po.Name)) {				
//				po.isEditing = true;
//				for (int i = 0; i < po.parameters.Count; i++) {
//					p = po.parameters [i];
//					p.isEditing = false;
//				}
				po.isMini = false;
			}


			if (ArchimatixEngine.nodeIcons.ContainsKey (po.GeneratorTypeString))
				EditorGUI.DrawTextureTransparent (new Rect (x1 + innerWidth - lineHgt * 2 - 4, cur_y, lineHgt * 2, lineHgt * 2), ArchimatixEngine.nodeIcons [po.GeneratorTypeString], ScaleMode.ScaleToFit, 1.0F);


			cur_y += lineHgt + 2 * gap;










			// DO THUMBNAIL / DROP_ZONE


	
			int bottomPadding = 55;
			int splineCanvasSize = (int)(po.rect.width - 60);

			editor.mostRecentThumbnailRect = new Rect (x1, cur_y + lineHgt, innerWidth, innerWidth);

			Rect lowerRect = new Rect (0, cur_y - 50, po.rect.width, po.rect.width);


			if (po.thumbnailState == ThumbnailState.Open) {


				if (po.is2D ()) {


					AXParameter output_p = po.generator.getPreferredOutputParameter (); 
					if (po.generator.hasOutputsReady ()) {

						if (ArchimatixEngine.nodeIcons.ContainsKey ("Blank"))
							EditorGUI.DrawTextureTransparent (editor.mostRecentThumbnailRect, ArchimatixEngine.nodeIcons ["Blank"], ScaleMode.ScaleToFit, 1.0F);

						Color color = po.thumbnailLineColor;

						if (color.Equals (Color.clear))
							color = Color.magenta;

						GUIDrawing.DrawPathsFit (output_p, new Vector2 (po.rect.width / 2, cur_y + po.rect.width / 2), po.rect.width - 60, ArchimatixEngine.AXGUIColors ["ShapeColor"]);

					} else if (ArchimatixEngine.nodeIcons.ContainsKey (po.GeneratorTypeString.ToString ()))
						EditorGUI.DrawTextureTransparent (editor.mostRecentThumbnailRect, ArchimatixEngine.nodeIcons [po.GeneratorTypeString], ScaleMode.ScaleToFit, 1.0F);


				} else {
					//Debug.Log ("po.renTex.IsCreated()="+po.renTex.IsCreated());

					//Debug.Log (po.renTex + " :::::::::::::::::::::::--::




					if (po.generator is PrefabInstancer && po.prefab != null) {
						Texture2D thumber = AssetPreview.GetAssetPreview (po.prefab);
						if (e.type == EventType.Repaint) {
							if (thumber != null)
								EditorGUI.DrawTextureTransparent (editor.mostRecentThumbnailRect, thumber, ScaleMode.ScaleToFit, 1.0F);
							else
								EditorGUI.DrawTextureTransparent (editor.mostRecentThumbnailRect, ArchimatixEngine.nodeIcons [po.GeneratorTypeString], ScaleMode.ScaleToFit, 1.0F);

						}

					} else if (((po.Output != null && po.Output.meshes != null && po.Output.meshes.Count > 0) || po.generator is MaterialTool) && (po.renTex != null || po.thumbnail != null))
 {					//if ( po.thumbnail != null )   
						//Debug.Log("thumb " + po.renTex);
						if (e.type == EventType.Repaint) {
							EditorGUI.DrawTextureTransparent (editor.mostRecentThumbnailRect, po.renTex, ScaleMode.ScaleToFit, 1.0F);


							// DROP ZONE

							if (po.generator is Grouper && editor.editorState == AXNodeGraphEditorWindow.EditorState.DraggingNodePalette && editor.mouseIsDownOnPO != po && po != model.currentWorkingGroupPO) {
								if (editor.mostRecentThumbnailRect.Contains (e.mousePosition)) {
								
									EditorGUI.DrawTextureTransparent(editor.mostRecentThumbnailRect, editor.dropZoneOverTex, ScaleMode.ScaleToFit, 1.0F);
									editor.OverDropZoneOfPO = po;
								} else
									EditorGUI.DrawTextureTransparent(editor.mostRecentThumbnailRect, editor.dropZoneTex, ScaleMode.ScaleToFit, 1.0F);
							}


						}
						//else
						//	GUI.DrawTexture(editor.mostRecentThumbnailRect, po.thumbnail, ScaleMode.ScaleToFit, false, 1.0F);



						if (editor.mostRecentThumbnailRect.Contains (e.mousePosition) || editor.draggingThumbnailOfPO == po) {
							Rect orbitButtonRect = new Rect (x1 + innerWidth - 16 - 3, cur_y + lineHgt + 3, 16, 16);

							if (e.command || e.control)
								EditorGUI.DrawTextureTransparent (orbitButtonRect, editor.dollyIconTex);
							else
								EditorGUI.DrawTextureTransparent (orbitButtonRect, editor.orbitIconTex);


							if (e.type == EventType.MouseDown && orbitButtonRect.Contains (e.mousePosition)) {
								model.selectOnlyPO (po);
								editor.draggingThumbnailOfPO = po;
								e.Use ();
							}

						}	


					} else if (ArchimatixEngine.nodeIcons.ContainsKey (po.GeneratorTypeString.ToString ())) { 
						EditorGUI.DrawTextureTransparent (editor.mostRecentThumbnailRect, ArchimatixEngine.nodeIcons [po.GeneratorTypeString], ScaleMode.ScaleToFit, 1.0F);

						// DROP ZONE

						if (po.generator is Grouper && editor.editorState == AXNodeGraphEditorWindow.EditorState.DraggingNodePalette && editor.mouseIsDownOnPO != po && po != model.currentWorkingGroupPO) {
							if (editor.mostRecentThumbnailRect.Contains (e.mousePosition)) {
								EditorGUI.DrawTextureTransparent (editor.mostRecentThumbnailRect, editor.dropZoneOverTex, ScaleMode.ScaleToFit, 1.0F);
								editor.OverDropZoneOfPO = po;
							} else
								EditorGUI.DrawTextureTransparent (editor.mostRecentThumbnailRect, editor.dropZoneTex, ScaleMode.ScaleToFit, 1.0F);
						}


					}

				}

				cur_y += lineHgt + bottomPadding + splineCanvasSize + gap;

				po.rect.height = cur_y;
				cur_y += 4 * gap;
			} else {
				// no thumbnail
				cur_y += 2 * lineHgt;
				po.rect.height = cur_y;
			}





			// INLETS
			// INPUT ITEMS
			// Parameter Lines

			//for (int i=0; i<po.parameters.Count; i++) {
			if ((po.inputControls != null && po.inputControls.children != null)) {
				for (int i = 0; i < po.inputControls.children.Count; i++) {
					p = (AXParameter)po.inputControls.children [i];


					if (p.PType != AXParameter.ParameterType.Input)
						continue;


					//if ( p.DependsOn != null && !p.DependsOn.Parent.isOpen && ! p.Name.Contains ("External"))
					//	continue;


					//if (parametricObjects_Property != null) 
					if (model.parametricObjects != null) {
						// these points are world, not rlative to the this GUIWindow
						p.inputPoint = new Vector2 (po.rect.x, po.rect.y + 100);
						p.outputPoint = new Vector2 (po.rect.x + po.rect.width, po.rect.y + cur_y + lineHgt / 2);

					}
				}
			}






			// OUTLETS

			if (po.outputsNode != null) {
				for (int i = 0; i < po.outputsNode.children.Count; i++) {

					p = (AXParameter)po.outputsNode.children [i];

					if (p == null)
						continue;

					//if (p.hasInputSocket || ! p.hasOutputSocket)
					if (p.PType != AXParameter.ParameterType.Output)
						continue;


					//if (parametricObjects_Property != null)
					if (model.parametricObjects != null) {
						// these points are world, not relative to the this GUIWindow
						p.inputPoint = new Vector2 (po.rect.x, po.rect.y + cur_y + lineHgt / 2);
						p.outputPoint = new Vector2 (po.rect.x + po.rect.width, po.rect.y + po.rect.width);


						Rect pRect = new Rect (x1, cur_y, innerWidth, lineHgt);


						//if (parameters_Property.arraySize > i)
						if (po.parameters != null && po.parameters.Count > i) {
							int hgt = 0;

							if (po.is2D ()) {

								hgt = ParameterSplineGUI.OnGUI_Spline (pRect, editor, p);
								cur_y = hgt + gap;
							} else {
								//hgt = ParameterGUI.OnGUI (pRect, editor, p);
								//cur_y += hgt + gap;

								Color dataColor =  editor.getDataColor(p.Type);

								// PERSONAL
								if (! EditorGUIUtility.isProSkin)
									dataColor = new Color(dataColor.r, dataColor.b, dataColor.g, .3f);
								
								GUI.color = dataColor;

								buttonLabel = (editor.OutputParameterBeingDragged == p) ? "-" : "";
								buttonRect = new Rect (p.Parent.rect.width - pRect.height - 10, p.Parent.rect.width-10, 2*ArchimatixEngine.buttonSize, 2*ArchimatixEngine.buttonSize);

								// button color
								if (editor.InputParameterBeingDragged != null)
								{
									if (editor.InputParameterBeingDragged.Type != p.Type) 
										GUI.backgroundColor = inactiveColor;
									else if (buttonRect.Contains (Event.current.mousePosition)) 
										GUI.backgroundColor = Color.white;
								} 
								else if (editor.OutputParameterBeingDragged != null)
								{
									if (editor.OutputParameterBeingDragged == p)
										GUI.backgroundColor = Color.white;
									else
										GUI.backgroundColor = inactiveColor;
								}


								if (GUI.Button (buttonRect, buttonLabel))
								{
									if (editor.InputParameterBeingDragged != null && editor.InputParameterBeingDragged.Type != p.Type) 
										editor.InputParameterBeingDragged = null;
									else
										editor.outputSocketClicked(p);
								}
				
	
							}
						}
					}
				}
			}




			// FOOTER //





			// STATS
			if (po.stats_VertCount > 0 || po.generator is MaterialTool) {

				string statsText;

				if (po.generator is MaterialTool) {
					statsText = (po.generator as MaterialTool).texelsPerUnit.ToString ("F0") + " Texels/Unit";
				} else {
					statsText = po.stats_VertCount + " verts";

					if (po.stats_TriangleCount > 0)
						statsText += ", " + po.stats_TriangleCount + " tris";
				}

				GUIStyle statlabelStyle = GUI.skin.GetStyle ("Label");
				TextAnchor prevStatTextAlignment = statlabelStyle.alignment;
				statlabelStyle.alignment = TextAnchor.MiddleLeft;
				EditorGUIUtility.labelWidth = 500;
				GUI.Label (new Rect (10, po.rect.height - x2 + 2, 500, lineHgt), statsText);
				statlabelStyle.alignment = prevStatTextAlignment;
			}





			if (e.type == EventType.MouseDown && lowerRect.Contains (e.mousePosition)) {
				editor.clearFocus ();
				GUI.FocusWindow (po.guiWindowId);
			}

			// WINDOW RESIZE
			buttonRect = new Rect (po.rect.width - 16, po.rect.height - 17, 14, 14);
			if (e.type == EventType.MouseDown && buttonRect.Contains (e.mousePosition)) {
				Undo.RegisterCompleteObjectUndo (model, "GUI Window Resize");

				editor.editorState = AXNodeGraphEditorWindow.EditorState.DragResizingNodePalleteWindow;	
				editor.DraggingParameticObject = po;		

			}
			//GUI.Button ( buttonRect, "∆", GUIStyle.none);
			GUI.Button (buttonRect, editor.resizeCornerTexture, GUIStyle.none);


			if (e.type == EventType.MouseDown && buttonRect.Contains (e.mousePosition)) {
			}

			//cur_y += lineHgt + gap;

			// Window title bar is the dragable area
			//GUI.DragWindow(headerRect);

			if (editor.draggingThumbnailOfPO == null || editor.draggingThumbnailOfPO != po) {
			

				GUI.DragWindow ();

			}

		}

	}
}
