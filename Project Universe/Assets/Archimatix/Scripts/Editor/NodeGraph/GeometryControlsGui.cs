using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

using AXClipperLib;

using AXGeometry;

using AX;
using AXEditor;
using AX.Generators;


public class GeometryControlsGui {




	public static int GeometryControlsOnGUI(int cur_y, AXNodeGraphEditorWindow editor, AXParametricObject po)
	{

		int gap 	= ArchimatixUtils.gap;
		int lineHgt = ArchimatixUtils.lineHgt;

		float x1 = 10;
		float x2 = 20;
		float winMargin = ArchimatixUtils.indent;
		float innerWidth = po.rect.width - 2*winMargin;



		AXParameter p = null;

		// Geometry (and other) controllers
		if (po.geometryControls != null && po.geometryControls.children !=null)
		{
			for (int i=0; i<po.geometryControls.children.Count; i++) {
				
				p =  po.geometryControls.children[i] as AXParameter;
				
				if (p.PType != AXParameter.ParameterType.None && p.PType != AXParameter.ParameterType.GeometryControl)
					continue;
							
				// these points are world, not relative to the this GUIWindow
				p.inputPoint 	= new Vector2( po.rect.x, 					po.rect.y+cur_y+lineHgt/2);
				p.outputPoint 	= new Vector2( po.rect.x+po.rect.width, 	po.rect.y+cur_y+lineHgt/2);
				
				Rect pRect = new Rect(x1, cur_y, innerWidth, lineHgt);
				
				try {
					int hgt = ParameterGUI.OnGUI(pRect, editor, p);
					cur_y += hgt + gap;
				} catch {
					
				}
			}
		}
					
//		if (true || po.is2D() || po.generator is Grouper)
//		{
			if (GUI.Button (new Rect(x2, cur_y, lineHgt*1.25f,lineHgt), new GUIContent("+", "Create a new Control Parameter")))
			{
				Undo.RegisterCompleteObjectUndo (po.model, "New AXParameter");
				AXParameter tmpP = po.addParameter (new AXParameter());

				foreach(AXParameter pop in po.parameters)
					if (pop != p)
						pop.isOpen = false;

				po.model.indexedParameters.Add (tmpP.Guid, tmpP);
				
				po.doneEditing();

				tmpP.isOpen 	= true;
				tmpP.isEditing 	= false;
				tmpP.shouldFocus = true;
				//po.isEditing 	= true;

				po.model.cleanGraph();

				 
				AXNodeGraphEditorWindow.repaintIfOpen();
			} 

			/*
			if (GUI.Button (new Rect(x1+editButtonWid+6, cur_y, editButtonWid,lineHgt), "Done" ))
					po.doneEditing();
			else
				if (GUI.Button (new Rect(x1+editButtonWid+6, cur_y, editButtonWid,lineHgt), "Edit Controls" ))
					po.isEditing = true;
				*/
			cur_y += lineHgt + gap + 5;
//		}


		if (po.generator is MaterialTool)
		{
			MaterialTool materialTool = (po.generator as MaterialTool);

			GUIStyle labelstyle = GUI.skin.GetStyle ("Label");
			labelstyle.alignment = TextAnchor.MiddleLeft;
			GUI.Label(new Rect(10, cur_y, 250, 32), "   Texels/Unit: " + materialTool.texelsPerUnit.ToString("F0"));
			cur_y += 32;

		}
		else if (po.generator is ShapeOffsetter)
		{

			ShapeOffsetter offsetter = (po.generator as ShapeOffsetter);

			AXParameter output_p = offsetter.P_Output;

			if (output_p == null)
				output_p = po.getParameter("Output Shape");

			if (output_p == null)
				return cur_y;


			Rect tRect = new Rect(25, cur_y, 150, 16);

			GUIStyle labelstyle = GUI.skin.GetStyle ("Label");
			labelstyle.alignment = TextAnchor.MiddleLeft;
			/*
		if (p.PType == AXParameter.ParameterType.Output)
		{
			labelstyle.alignment = TextAnchor.MiddleRight;
			labelstyle.fixedWidth = lRect.width+5;
		}
		*/

 			// JOIN_TYPE: Square, Round, Miter
			if (offsetter.offset != 0)
			{
				string[] options = ArchimatixUtils.getMenuOptions("JoinType");
				EditorGUIUtility.labelWidth = 75;//-50;

				EditorGUI.BeginChangeCheck ();
				output_p.joinType = (JoinType) EditorGUI.Popup(tRect, "JoinType", (int) output_p.joinType, options);
				if (EditorGUI.EndChangeCheck ()) {
					Undo.RegisterCompleteObjectUndo (po.model, "value change for JoinType");
					po.model.autobuild();
				} 
				cur_y += ArchimatixUtils.lineHgt+ArchimatixUtils.gap;
			}

			// ARC_TOLERANCE (for JoinType.Round)
			if (output_p.joinType == AXClipperLib.JoinType.jtRound || output_p.endType == AXClipperLib.EndType.etOpenRound)
			{
				tRect.y = cur_y;
				
					
				if (float.IsNaN(output_p.arcTolerance))
					output_p.arcTolerance = 50;
				if (output_p.arcTolerance < .25f )
					output_p.arcTolerance = .25f;

				float smooth = (float) (120 / (output_p.arcTolerance * output_p.arcTolerance));
				
				AXEditorUtilities.assertFloatFieldKeyCodeValidity("shapehandler_Text_smoothness_" +output_p.Guid +"_" + output_p.Name);

				EditorGUI.BeginChangeCheck ();
				GUI.SetNextControlName("shapehandler" + output_p.Name);
				smooth = EditorGUI.FloatField(tRect, "smoothness",  smooth);
				if (EditorGUI.EndChangeCheck ()) {

					output_p.arcTolerance = (float) (Mathf.Sqrt(120/smooth));
					Undo.RegisterCompleteObjectUndo (po.model, "value change for " + output_p.Name);
					if (output_p.arcTolerance < .25f) 
						output_p.arcTolerance = .25f;
					if (output_p.arcTolerance > 50)
						output_p.arcTolerance = 50;
					if (float.IsNaN(output_p.arcTolerance))
						output_p.arcTolerance = 50;
					po.model.isAltered(20);
				}	
				cur_y += ArchimatixUtils.lineHgt+ArchimatixUtils.gap;
			}




		}

		return cur_y; 
	}


}
