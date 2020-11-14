using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
 
using AXClipperLib;

using Path 		= System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths 	= System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;

using AXGeometry;


using AX;
using AX.Generators;
using AXEditor;

using Parameters 				= System.Collections.Generic.List<AX.AXParameter>;


public class ParameterSplineGUI  {




	
	public static int OnGUI_Spline(Rect pRect, AXNodeGraphEditorWindow editor, AXParameter p)
	{
		
		
		float cur_x = ArchimatixUtils.cur_x; 

		//float box_w = ArchimatixUtils.paletteRect.width - cur_x - 3*ArchimatixUtils.indent;
		float box_w = pRect.width - cur_x - 1*ArchimatixUtils.indent;

		int cur_y = (int)pRect.y;
		int lineHgt = (int)pRect.height;
		int gap = 5;
		
		int margin = 24;
		
		float wid = pRect.width-margin;
		
		
		
		Color shapeColor = editor.getDataColor (AXParameter.DataType.Spline);
		
		Color oldBackgroundColor = GUI.backgroundColor;
		Color dataColor =  editor.getDataColor(p.Type);
		GUI.backgroundColor = dataColor;
		

		// INPUT
		if ( editor.OutputParameterBeingDragged != null && editor.OutputParameterBeingDragged.Type != AXParameter.DataType.Spline && editor.OutputParameterBeingDragged.Type != AXParameter.DataType.Curve3D)
				GUI.enabled = false;
		if (p.PType != AXParameter.ParameterType.Output)
		{
			if (GUI.Button (new Rect (-3, cur_y, ArchimatixEngine.buttonSize, ArchimatixEngine.buttonSize), "")) {

				if (editor.OutputParameterBeingDragged != null && editor.OutputParameterBeingDragged.Type != p.Type)
					editor.OutputParameterBeingDragged = null;
				else
					editor.inputSocketClicked (p);
			}
		}

		GUI.enabled = true;

		// OUTPUT
		if (editor.InputParameterBeingDragged == null || editor.InputParameterBeingDragged.Type == AXParameter.DataType.Spline)
		{
			if (GUI.Button (new Rect (pRect.width+6, cur_y, ArchimatixEngine.buttonSize, ArchimatixEngine.buttonSize), "")) 
			{
				if (editor.OutputParameterBeingDragged != null && editor.OutputParameterBeingDragged.Type != p.Type)
					editor.OutputParameterBeingDragged = null;
				else
					editor.outputSocketClicked (p);
			}
		}
		
		
		

		// LABEL
		Rect boxRect = new Rect(cur_x + ArchimatixUtils.indent+3, cur_y, box_w, pRect.height);
		Rect lRect = boxRect;
		
		lRect.x += 3;
		lRect.width -= 10;
		
		GUIStyle labelstyle = GUI.skin.GetStyle ("Label");
		labelstyle.alignment = TextAnchor.MiddleLeft;

		if (p.PType == AXParameter.ParameterType.Output)
		{
			labelstyle.alignment = TextAnchor.MiddleRight;
			labelstyle.fixedWidth = lRect.width-13;
		}
		
		GUI.Box(boxRect, " "); GUI.Box(boxRect, " "); GUI.Box(boxRect, " ");GUI.Box(boxRect, " ");


		//Debug.Log(p.Name + " - " + p.ParentNode+ " - " + p.ParentNode.Name);

		string label = p.Name;
		if (p.ParentNode != null && p.ParentNode is AXShape)
		{
			if (p.DependsOn != null)
				if (p.DependsOn.Parent != null )
					label = p.DependsOn.Parent.Name;
		}
		
		
		GUI.Label(lRect, label);







		
		// SOLID/VOID TOGGLE
		
		
		if (p.PType == AXParameter.ParameterType.Input && p.ParentNode != null && (p.ParentNode is AXShape))
		{
			
			// Texture2D solidVoidIconTexture = (Texture2D) AssetDatabase.LoadAssetAtPath(ArchimatixEngine.ArchimatixAssetPath+"/ui/GeneralIcons/"+iconname, typeof(Texture2D));		

			Texture2D solidVoidIconTexture = (p.polyType == PolyType.ptSubject) ? editor.solidIconTexture : editor.voidIconTexture;
			
			Rect svRect = new Rect (boxRect.x + boxRect.width - lineHgt, cur_y, lineHgt, lineHgt);
			
			if(GUI.Button ( svRect, new GUIContent(solidVoidIconTexture, "Solid/Void"), GUIStyle.none))		
				//if(GUI.Button ( bRect, solidVoidIconTexture, GUIStyle.none))
			{
				p.polyType = (p.polyType == PolyType.ptSubject) ? PolyType.ptClip : PolyType.ptSubject;
				p.parametricObject.model.autobuild();


			}
			
			GUI.Label (new Rect (10,40,100,40), GUI.tooltip);
		}
		
		
		
		// OPEN/CLOSED TOGGLE
		else 
		{
			
			Texture2D solidOpenClosedTexture = (p.shapeState == ShapeState.Open) ? editor.shapeOpenIconTexture : editor.shapeClosedIconTexture;
			Rect svRect = new Rect (boxRect.x + boxRect.width - lineHgt-2, cur_y, lineHgt+2, lineHgt+2);

			if(GUI.Button ( svRect, new GUIContent(solidOpenClosedTexture, "Open/Closed"), GUIStyle.none))		
				//if(GUI.Button ( bRect, solidVoidIconTexture, GUIStyle.none))
			{
				p.shapeState = (p.shapeState == ShapeState.Open) ? ShapeState.Closed : ShapeState.Open;
				p.parametricObject.model.autobuild();

			}
			
			GUI.Label (new Rect (10,40,100,40), GUI.tooltip);
		}


		
		// FOLDOUT (isOpen)
		GUI.backgroundColor = new Color(1,1,1,1f);
		p.isOpen = EditorGUI.Foldout (new Rect (cur_x, cur_y, 15, lineHgt), p.isOpen, "");
		GUI.backgroundColor = shapeColor;
		cur_y += lineHgt+gap;
		

		Rect tRect = pRect;
		tRect.x = 30;
		tRect.width = pRect.width-20;
		
		

		
		if (p.isOpen)
		{
			
			ArchimatixUtils.cur_x += ArchimatixUtils.indent;

			lineHgt = ArchimatixUtils.lineHgtSmall;

			p.drawClosed = false;
			
			
			string[] options;
			
			tRect.x += 2;
			tRect.y = cur_y;
			tRect.width -= 11;
			tRect.height = 16;

			Rect cntlRect = tRect; // new Rect(x0, cur_y, wid, 16);

			cntlRect.height = 16;



			// 	EXPOSE AS RUNTIME Interface ------------
				//EditorGUIUtility.labelWidth = wid-36;
				cntlRect = new Rect(tRect.x, cur_y, wid, 16);
				EditorGUI.BeginChangeCheck ();
				p.exposeAsInterface = EditorGUI.Toggle (cntlRect, "Runtime",  p.exposeAsInterface);
				if (EditorGUI.EndChangeCheck ()) {
					Undo.RegisterCompleteObjectUndo (p.parametricObject.model, "Expose Parameter");

					if (p.exposeAsInterface)
					{
						p.parametricObject.model.addExposedParameter(p);
					}
					else
					{
						p.parametricObject.model.removeExposedParameter(p); 
					}
				}
		
				cur_y += lineHgt +gap;
			
				





			// BREAK MINMAXSLIDER
			tRect.y = cur_y;
			GUI.Label(tRect, "Break Geom|Norms");
			cur_y += lineHgt;
			GUI.backgroundColor = Color.white;
			tRect.y = cur_y; 
			EditorGUI.BeginChangeCheck();

			#if UNITY_5_5_OR_NEWER
			EditorGUI.MinMaxSlider( 
				tRect,
				GUIContent.none, 
				ref p.breakGeom, ref p.breakNorm,
				0, 100);

			#else
			EditorGUI.MinMaxSlider( 
				GUIContent.none, 
				tRect,
				ref p.breakGeom, ref p.breakNorm,
				0, 100);
			#endif
			if(EditorGUI.EndChangeCheck())
			{
				Undo.RegisterCompleteObjectUndo (p.parametricObject.model, "Max Min Slider");

				p.parametricObject.model.isAltered(1);
			}
			
			cur_y += lineHgt + gap;
			 
			cntlRect.y = cur_y;


			// REVERSE
			EditorGUI.BeginChangeCheck ();
			p.reverse =  GUI.Toggle (cntlRect, p.reverse, "Reverse");
			if(EditorGUI.EndChangeCheck())
			{
				Undo.RegisterCompleteObjectUndo (p.parametricObject.model, "Reverse");

				p.parametricObject.model.autobuild();
			}

			cur_y += lineHgt + gap;
			
			cntlRect.y = cur_y;


			// SYMMETRY
			EditorGUI.BeginChangeCheck ();
			p.symmetry =  GUI.Toggle (cntlRect, p.symmetry, "Symetry");
			if(EditorGUI.EndChangeCheck())
			{
				Undo.RegisterCompleteObjectUndo (p.parametricObject.model, "Symetry");

				p.parametricObject.model.autobuild();
			}

			cur_y += lineHgt + gap;
			
			cntlRect.y = cur_y;




			// SYMMETRY SEPERATION
			if (p.symmetry)
			{
				EditorGUI.BeginChangeCheck ();
				EditorGUIUtility.labelWidth = .5f*wid;
				GUI.SetNextControlName ("FloatField_SymSeperation_Text_" + p.Guid + "_");
				p.symSeperation = EditorGUI.FloatField (cntlRect, "Seperation", p.symSeperation);
				if(EditorGUI.EndChangeCheck())
				{
					Undo.RegisterCompleteObjectUndo (p.parametricObject.model, "SymetrySeperation");

					p.parametricObject.model.autobuild();
				}

				cur_y += lineHgt + gap;
				
				cntlRect.y = cur_y;
				}






			// FLIP_X

			EditorGUI.BeginChangeCheck ();
			p.flipX =  GUI.Toggle (cntlRect, p.flipX, "FlipX");
			if(EditorGUI.EndChangeCheck())
			{
				Undo.RegisterCompleteObjectUndo (p.parametricObject.model, "Flip X");

				p.parametricObject.model.autobuild();

				p.parametricObject.generator.adjustWorldMatrices();
			}

			cur_y += lineHgt + gap;
			

			
			
			tRect.y = cur_y; 

			tRect.height = 16;
			



			// SHAPE_STATE

			options = ArchimatixUtils.getMenuOptions("ShapeState");
			EditorGUIUtility.labelWidth = .5f*wid;//-50;
			EditorGUI.BeginChangeCheck ();
			p.shapeState = (ShapeState) EditorGUI.Popup(
				tRect,
				"ShapeState",
				(int) p.shapeState, 
				options);
			if (EditorGUI.EndChangeCheck ()) {
				Undo.RegisterCompleteObjectUndo (p.parametricObject.model, "Shape State");

				p.parametricObject.model.autobuild();
			}
			cur_y += lineHgt + gap;
			
			
			
			cntlRect.y = cur_y;
			cntlRect.height = ArchimatixUtils.lineHgtSmall;



			// THICKNESS

			AXEditorUtilities.assertFloatFieldKeyCodeValidity("FloatField_Thickness");

			EditorGUI.BeginChangeCheck ();
			GUI.SetNextControlName("FloatField_Thickness_Text_" + p.Guid + "_" );
			p.thickness = EditorGUI.FloatField(cntlRect, "Thickness",  p.thickness);
			if (EditorGUI.EndChangeCheck ()) {
				Undo.RegisterCompleteObjectUndo (p.Parent.model, "value change for Thickness");
				p.thickness = Mathf.Max(p.thickness, 0);
				p.parametricObject.model.isAltered(2);
			}
			cur_y += ArchimatixUtils.lineHgtSmall + gap;




			if (p.shapeState == ShapeState.Closed) {
				cntlRect.y = cur_y;
				cntlRect.height = ArchimatixUtils.lineHgtSmall;


				// ROUNDNESS 

				AXEditorUtilities.assertFloatFieldKeyCodeValidity("FloatField_Roundness");

				EditorGUI.BeginChangeCheck ();
				GUI.SetNextControlName ("FloatField_Roundness_Text_" + p.Guid + "_");
				p.roundness = EditorGUI.FloatField (cntlRect, "Roundness", p.roundness);
				if (EditorGUI.EndChangeCheck ()) {
					Undo.RegisterCompleteObjectUndo (p.Parent.model, "value change for Roundness");
					p.parametricObject.model.isAltered (3);
				}
				cur_y += ArchimatixUtils.lineHgtSmall + gap;

				cntlRect.y = cur_y;
				cntlRect.height = ArchimatixUtils.lineHgtSmall;




				// OFFSET

				AXEditorUtilities.assertFloatFieldKeyCodeValidity("FloatField_Offset");

				EditorGUI.BeginChangeCheck ();
				GUI.SetNextControlName ("FloatField_Offset_Text_" + p.Guid + "_");
				p.offset = EditorGUI.FloatField (cntlRect, "Offset", p.offset);
				if (EditorGUI.EndChangeCheck ()) {
					Undo.RegisterCompleteObjectUndo (p.Parent.model, "value change for Offset");
					p.parametricObject.model.isAltered (3);
				}
				cur_y += ArchimatixUtils.lineHgtSmall + gap;
			}

			
			if (p.thickness > 0)
			{
				if (p.shapeState == ShapeState.Closed)
				{	// CLOSED
					if(p.thickness > 0)
						p.endType = AXClipperLib.EndType.etClosedLine;
					else 
						p.endType = AXClipperLib.EndType.etClosedPolygon;
				} 
				else 
				{	// OPEN
					switch(p.openEndType)
					{
					case AXParameter.OpenEndType.Butt:
						p.endType = AXClipperLib.EndType.etOpenButt;
						break;
					case AXParameter.OpenEndType.Square:
						p.endType = AXClipperLib.EndType.etOpenSquare;
						break;
					case AXParameter.OpenEndType.Round:
						p.endType = AXClipperLib.EndType.etOpenRound;
						break;
					default:
						p.endType = AXClipperLib.EndType.etOpenSquare;
						break;
						
					}
				}
			}
			
			
			 
			
			
			
			
			if ( p.shapeState == ShapeState.Closed)
			{
				p.drawClosed = true;
			} 
			
			
			if ( (p.thickness > 0) || (p.roundness != 0) || (p.offset !=  0) )
			{
				
				p.drawClosed = true;
				
				tRect.y = cur_y;
				
				options = ArchimatixUtils.getMenuOptions("JoinType");
				EditorGUIUtility.labelWidth = .5f*wid;//-50;
				EditorGUI.BeginChangeCheck ();

				p.joinType = (AXClipperLib.JoinType) EditorGUI.Popup(
					tRect,
					"JoinType",
					(int)p.joinType, 
					options);
				if (EditorGUI.EndChangeCheck ()) {
					Undo.RegisterCompleteObjectUndo (p.Parent.model, "value change for JoinType");
					p.parametricObject.model.autobuild();
				} 
				
				cur_y += lineHgt+gap;
				
				if (p.joinType == AXClipperLib.JoinType.jtRound || p.endType == AXClipperLib.EndType.etOpenRound)
				{
					tRect.y = cur_y;
					
					AXEditorUtilities.assertFloatFieldKeyCodeValidity("FloatField_smoothness");


					if (float.IsNaN(p.arcTolerance))
						p.arcTolerance = 50;
					if (p.arcTolerance < .25f )
						p.arcTolerance = .25f;
					float tmp_arcTol = p.arcTolerance;
					
					float smoothness = (float) (120 / (p.arcTolerance * p.arcTolerance));
					EditorGUI.BeginChangeCheck ();
					GUI.SetNextControlName ("FloatField_smoothness_Text_" + p.Guid + "_");
					smoothness = EditorGUI.FloatField(tRect, "smoothness",  smoothness);

					if (EditorGUI.EndChangeCheck ()) {

						smoothness = Mathf.Clamp(smoothness, .048f, 2.0f);


						float smoothLOD =((smoothness-.048f) *  p.parametricObject.model.segmentReductionFactor)+.048f;

						p.arcTolerance = (float) (Math.Sqrt(120/smoothLOD));
						if (p.arcTolerance != tmp_arcTol && p.Parent.model.readyToRegisterUndo)
						{
							//Debug.Log ("REGISTER UNDO");
							float newval = p.arcTolerance;
							p.arcTolerance = tmp_arcTol;
							Undo.RegisterCompleteObjectUndo (p.Parent.model, "value change for " + p.Name);
							p.arcTolerance = newval;
							
							
							
							p.Parent.model.readyToRegisterUndo = false;
						}
						if (p.arcTolerance < .25f) 
							p.arcTolerance = .25f;
						if (p.arcTolerance > 50)
							p.arcTolerance = 50;
						if (float.IsNaN(p.arcTolerance))
							p.arcTolerance = 50;
						
						p.parametricObject.model.isAltered(4);

					}	
					cur_y += lineHgt+gap;
				}
				
				
			}
			if (p.shapeState == ShapeState.Open && p.thickness > 0)
			{
				// OPEN_END_TYPE
				options = ArchimatixUtils.getMenuOptions("OpenEndType");
				EditorGUIUtility.labelWidth = .5f*wid;//-50;
				tRect.y = cur_y; 		
				EditorGUI.BeginChangeCheck ();
				p.openEndType = (AXParameter.OpenEndType) EditorGUI.Popup(
					tRect,
					"EndType",
					(int) p.openEndType, 
					options);
				if (EditorGUI.EndChangeCheck ()) {
					Undo.RegisterCompleteObjectUndo (p.Parent.model, "value change for EndType");

				}
				cur_y += lineHgt + gap;
				
				
			}
			
			
			
			

			
			cur_y += lineHgt/2 ;


			// SUBDIVISION
			cntlRect.y = cur_y;

			EditorGUI.BeginChangeCheck ();
			GUI.SetNextControlName ("FloatField_Subdivision_Text_" + p.Guid + "_");
			p.subdivision = (float) EditorGUI.IntField (cntlRect, "Subdivision", (int)p.subdivision);
			if (EditorGUI.EndChangeCheck ()) {
				Undo.RegisterCompleteObjectUndo (p.Parent.model, "value change for Subdivision");
				if (p.subdivision < 0)
					p.subdivision = 0;
				p.parametricObject.model.isAltered (3);
			}

			ArchimatixUtils.cur_x -= ArchimatixUtils.indent;
			cur_y += 2*lineHgt ;
			
		}


		GUI.backgroundColor = oldBackgroundColor;

		return cur_y;
		
		
	}
	



}
