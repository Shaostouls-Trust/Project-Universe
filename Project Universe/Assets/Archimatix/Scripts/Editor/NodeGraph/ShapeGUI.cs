using UnityEngine;
using UnityEditor;
using System.Collections;

using AXGeometry;

using AX;

public class ShapeGUI  {


	
	public static int OnGUI(Rect pRect, AXNodeGraphEditorWindow editor, AXShape shp) 
	{
		
		Color shapeColor = editor.getDataColor(AXParameter.DataType.Spline);

		Color origBG = GUI.backgroundColor;

		
		//Rect pRect = new Rect(x1+12, cur_y, width-20,lineHgt);
		float cur_x = ArchimatixUtils.cur_x; 
		//float box_x = cur_x + ArchimatixUtils.indent;
		float box_w = ArchimatixUtils.paletteRect.width - cur_x - 3*ArchimatixUtils.indent;
		
		
		
		int x1 			= (int) pRect.x - ArchimatixUtils.indent ;
		int cur_y 		= (int) pRect.y;
		int width 		= (int) pRect.width;
		int lineHgt 	= ArchimatixUtils.lineHgt;
		int gap 		= 5;
		
		Rect boxRect = pRect;
		
		boxRect = new Rect(cur_x + ArchimatixUtils.indent, cur_y, box_w, ArchimatixUtils.lineHgt);
		
		Rect lRect = new Rect(x1+11, cur_y, 50 ,lineHgt);
		
		if (! shp.isOpen && shp.inputs != null)
		{
			foreach(AXParameter sp in  shp.inputs)
			{
				sp.inputPoint 	= new Vector2( ArchimatixUtils.paletteRect.x, 								ArchimatixUtils.paletteRect.y + cur_y + lineHgt/2);
				sp.outputPoint  = new Vector2( ArchimatixUtils.paletteRect.x + ArchimatixUtils.paletteRect.width, ArchimatixUtils.paletteRect.y + cur_y + lineHgt/2);
			}
		}
		
		

		
		// Header
		//EditorGUI.BeginChangeCheck();
		
		//GUI.Button(pRect, Name);
		GUI.color = Color.white;
		
		GUI.backgroundColor = shapeColor;
		

		
		

		// 
		shp.isOpen = true;
		
		if (shp.isOpen)
		{
			
			/*  INPUTS  
					 */
			
			
			GUIStyle labelstyle = GUI.skin.GetStyle ("Label");
			labelstyle.alignment = TextAnchor.MiddleLeft;
			labelstyle.fixedWidth = 100;
			labelstyle.fontSize = 12;
			
			//Rect boxRect = new Rect (x1 + 22, cur_y, width - 38, lineHgt);
			//Rect editRect = new Rect (x1 + 22, cur_y, width - 38, lineHgt);
			

			//Archimatix.cur_x += Archimatix.indent;
			// INPUT SHAPE PARAMETERS
			for (int i = 0; i < shp.inputs.Count; i++) {
				
				AXParameter sp = shp.inputs [i];
				
				sp.inputPoint 	= new Vector2 (ArchimatixUtils.paletteRect.x, 								ArchimatixUtils.paletteRect.y + cur_y + lineHgt / 2);
				sp.outputPoint 	= new Vector2 (ArchimatixUtils.paletteRect.x + ArchimatixUtils.paletteRect.width, ArchimatixUtils.paletteRect.y + cur_y + lineHgt / 2);
				
				cur_y = ParameterSplineGUI.OnGUI_Spline(new Rect(x1, cur_y, width,lineHgt), editor, sp);
				
			}
			//Archimatix.cur_x -= Archimatix.indent;
			
			// Empty / New SHAPE PARAMETER

			if ( editor.OutputParameterBeingDragged != null && editor.OutputParameterBeingDragged.Type != AXParameter.DataType.Spline)
				GUI.enabled = false;

			if (GUI.Button (new Rect (-3, cur_y, lineHgt, lineHgt), "")) {
				
				//Debug.Log ("make shape 2");
				AXParameter new_p = shp.addInput();
				editor.inputSocketClicked (new_p);
				editor.OutputParameterBeingDragged = null;
			}

			GUI.enabled = true;
			
			boxRect = new Rect (x1 +11, cur_y, width - 38, lineHgt);
			GUI.Box (boxRect, " ");
			GUI.Box (boxRect, " ");
			//boxRect.x += 10;
			GUI.Label (boxRect, "Empty Shape");
			
			
			cur_y += lineHgt+gap;
			
			
			//cur_y += 2*gap;
			
			
			
			
			//Rect bRect = boxRect;
			
			lRect.y = cur_y;
			
			
			/*  SPECIFIC OUTPUT  PARAMETERS
					 */
			//lRect.y = cur_y;
			//GUI.Label(lRect, "Output Combinations"); 
			cur_y += gap;
			
			//Archimatix.cur_x += Archimatix.indent;
			
			GUI.color = Color.white;
			GUI.backgroundColor = Color.white;
			
			//Rect foldRect = new Rect(ArchimatixUtils.indent*2, cur_y, 30,lineHgt);
			
			
			//outputParametersOpen = EditorGUI.Foldout(foldRect, outputParametersOpen, "Merge Results");
			
			  
			
			/*
			if (false && shp.outputParametersOpen)
			{
				cur_y += lineHgt;
				ArchimatixUtils.cur_x += ArchimatixUtils.indent;
				
				bool tmp_boolval;
				bRect.x = ArchimatixUtils.indent*3+4;//width-lineHgt;
				
				
				// difference
				shp.difference.inputPoint 	= new Vector2 (ArchimatixUtils.paletteRect.x, 									ArchimatixUtils.paletteRect.y + cur_y + lineHgt / 2);
				shp.difference.outputPoint 	= new Vector2 (ArchimatixUtils.paletteRect.x + ArchimatixUtils.paletteRect.width, 	ArchimatixUtils.paletteRect.y + cur_y + lineHgt / 2);
				bRect.y = cur_y-2;
				lRect.y = cur_y;
				
				cur_y = ParameterSplineGUI.OnGUI_Spline(new Rect(x1, cur_y, width,lineHgt), editor, shp.difference);
				
				tmp_boolval = (shp.combineType == AXShape.CombineType.Difference);
				EditorGUI.BeginChangeCheck ();
				tmp_boolval = EditorGUI.Toggle (bRect, "",  tmp_boolval);
				if (EditorGUI.EndChangeCheck ()) {
					Undo.RegisterCompleteObjectUndo (shp.Parent.model, "value change for combineType" );
					shp.combineType = AXShape.CombineType.Difference;
					shp.parametricObject.model.autobuild();
				}
				
				// difference
				shp.differenceRail.inputPoint 	= new Vector2 (ArchimatixUtils.paletteRect.x, 									ArchimatixUtils.paletteRect.y + cur_y + lineHgt / 2);
				shp.differenceRail.outputPoint 	= new Vector2 (ArchimatixUtils.paletteRect.x + ArchimatixUtils.paletteRect.width, 	ArchimatixUtils.paletteRect.y + cur_y + lineHgt / 2);
				
				bRect.y = cur_y-2;
				
				cur_y = ParameterSplineGUI.OnGUI_Spline(new Rect(x1, cur_y, width, lineHgt), editor, shp.differenceRail);
				
				tmp_boolval = (shp.combineType == AXShape.CombineType.DifferenceRail);
				EditorGUI.BeginChangeCheck ();
				tmp_boolval = EditorGUI.Toggle (bRect, "",  tmp_boolval); 
				if (EditorGUI.EndChangeCheck ()) {
					Undo.RegisterCompleteObjectUndo (shp.Parent.model, "value change for combineType" );
					shp.combineType = AXShape.CombineType.DifferenceRail;
					shp.parametricObject.model.autobuild();
				}
				
				cur_y += gap;
				
				
				
				// INTERSECTION
				shp.intersection.inputPoint 	= new Vector2 (ArchimatixUtils.paletteRect.x, 									ArchimatixUtils.paletteRect.y + cur_y + lineHgt / 2);
				shp.intersection.outputPoint 	= new Vector2 (ArchimatixUtils.paletteRect.x + ArchimatixUtils.paletteRect.width, 	ArchimatixUtils.paletteRect.y + cur_y + lineHgt / 2);
				bRect.y = cur_y-2;
				
				cur_y = ParameterSplineGUI.OnGUI_Spline(new Rect(x1, cur_y, width, lineHgt), editor, shp.intersection);
				
				tmp_boolval = (shp.combineType == AXShape.CombineType.Intersection);
				EditorGUI.BeginChangeCheck ();
				tmp_boolval = EditorGUI.Toggle (bRect, "",  tmp_boolval);
				if (EditorGUI.EndChangeCheck ()) {
					Undo.RegisterCompleteObjectUndo (shp.Parent.model, "value change for combineType" );
					Debug.Log("YA");
					shp.combineType = AXShape.CombineType.Intersection;
					shp.parametricObject.model.autobuild();
				}
				
				
				// INTERSECTION_RAIL
				shp.intersectionRail.inputPoint 	= new Vector2 (ArchimatixUtils.paletteRect.x, 									ArchimatixUtils.paletteRect.y + cur_y + lineHgt / 2);
				shp.intersectionRail.outputPoint 	= new Vector2 (ArchimatixUtils.paletteRect.x + ArchimatixUtils.paletteRect.width, 	ArchimatixUtils.paletteRect.y + cur_y + lineHgt / 2);
				bRect.y = cur_y-2;
				
				cur_y = ParameterSplineGUI.OnGUI_Spline(new Rect(x1, cur_y, width, lineHgt), editor, shp.intersectionRail);
				
				tmp_boolval = (shp.combineType == AXShape.CombineType.IntersectionRail);
				EditorGUI.BeginChangeCheck ();
				tmp_boolval = EditorGUI.Toggle (bRect, "",  tmp_boolval);
				if (EditorGUI.EndChangeCheck ()) {
					Undo.RegisterCompleteObjectUndo (shp.Parent.model, "value change for combineType" );
					shp.combineType = AXShape.CombineType.IntersectionRail;
					shp.parametricObject.model.autobuild();
				}
				
				cur_y += gap;
				
				
				
				// union
				shp.union.inputPoint 	= new Vector2 (ArchimatixUtils.paletteRect.x, 									ArchimatixUtils.paletteRect.y + cur_y + lineHgt / 2);
				shp.union.outputPoint 	= new Vector2 (ArchimatixUtils.paletteRect.x + ArchimatixUtils.paletteRect.width, 	ArchimatixUtils.paletteRect.y + cur_y + lineHgt / 2);
				bRect.y = cur_y-2;
				
				cur_y = ParameterSplineGUI.OnGUI_Spline(new Rect(x1, cur_y, width, lineHgt), editor, shp.union);
				
				tmp_boolval = (shp.combineType == AXShape.CombineType.Union);
				EditorGUI.BeginChangeCheck ();
				tmp_boolval = EditorGUI.Toggle (bRect, "",  tmp_boolval);
				if (EditorGUI.EndChangeCheck ()) {
					Undo.RegisterCompleteObjectUndo (shp.Parent.model, "value change for combineType" );
					shp.combineType = AXShape.CombineType.Union;
					shp.parametricObject.model.autobuild();
				}
				
				cur_y += gap;
				
				
				
				// grouped
				if (shp.grouped == null || shp.grouped.Type == AXParameter.DataType.Float)
					shp.grouped 			= shp.createSplineParameter(AXParameter.ParameterType.Output, "Grouped");
				
				shp.grouped.inputPoint 	= new Vector2 (ArchimatixUtils.paletteRect.x, 									ArchimatixUtils.paletteRect.y + cur_y + lineHgt / 2);
				shp.grouped.outputPoint 	= new Vector2 (ArchimatixUtils.paletteRect.x + ArchimatixUtils.paletteRect.width, 	ArchimatixUtils.paletteRect.y + cur_y + lineHgt / 2);
				bRect.y = cur_y-2;
				
				cur_y = ParameterSplineGUI.OnGUI_Spline(new Rect(x1, cur_y, width, lineHgt), editor, shp.grouped);
				
				tmp_boolval = (shp.combineType == AXShape.CombineType.Grouped);
				EditorGUI.BeginChangeCheck ();
				tmp_boolval = EditorGUI.Toggle (bRect, "",  tmp_boolval);
				if (EditorGUI.EndChangeCheck ()) {
					Undo.RegisterCompleteObjectUndo (shp.Parent.model, "value change for combineType" );
					shp.combineType = AXShape.CombineType.Grouped;
					shp.parametricObject.model.autobuild();
				}
				
				
				
				
				ArchimatixUtils.cur_x -= ArchimatixUtils.indent;
				
				
			}
			*/
			//Archimatix.cur_x -= Archimatix.indent;
			//cur_y += lineHgt;
			
		}	


		GUI.backgroundColor = origBG;

		return cur_y;
		
		
		
	} // OnGUI






	public static int displayOutput(Rect pRect, AXNodeGraphEditorWindow editor, AXShape shp)
	{


		float cur_x = ArchimatixUtils.cur_x; 
		//float box_x = cur_x + ArchimatixUtils.indent;
		float box_w = ArchimatixUtils.paletteRect.width - cur_x - 3*ArchimatixUtils.indent;

		int x1 			= (int) pRect.x - ArchimatixUtils.indent ;
		int cur_y 		= (int) pRect.y;
		int width 		= (int) pRect.width;
		int lineHgt 	= ArchimatixUtils.lineHgt;
		int gap 		= 5;

		Rect boxRect = pRect;
		
		boxRect = new Rect(cur_x + ArchimatixUtils.indent, cur_y, box_w, ArchimatixUtils.lineHgt);
		
		Rect lRect = new Rect(x1+11, cur_y, 50 ,lineHgt);


		Rect bRect = boxRect;
			
		lRect.y = cur_y;


		//cur_y += lineHgt;
		ArchimatixUtils.cur_x += ArchimatixUtils.indent;
		
		bool tmp_boolval;
		bRect.x = ArchimatixUtils.indent*3+4;//width-lineHgt;
		
		
		// difference
		shp.difference.inputPoint 	= new Vector2 (ArchimatixUtils.paletteRect.x, 									ArchimatixUtils.paletteRect.y + cur_y + lineHgt / 2);
		shp.difference.outputPoint 	= new Vector2 (ArchimatixUtils.paletteRect.x + ArchimatixUtils.paletteRect.width, 	ArchimatixUtils.paletteRect.y + cur_y + lineHgt / 2);
		bRect.y = cur_y-1;
		lRect.y = cur_y;
		
		cur_y = ParameterSplineGUI.OnGUI_Spline(new Rect(x1, cur_y, width,lineHgt), editor, shp.difference);
		
		tmp_boolval = (shp.combineType == AXShape.CombineType.Difference);
		EditorGUI.BeginChangeCheck ();
		tmp_boolval = EditorGUI.Toggle (bRect, "",  tmp_boolval);
		if (EditorGUI.EndChangeCheck ()) {
			Undo.RegisterCompleteObjectUndo (shp.Parent.model, "value change for combineType" );
			shp.combineType = AXShape.CombineType.Difference;
			shp.parametricObject.model.generate();
		}
		
		// difference
		shp.differenceRail.inputPoint 	= new Vector2 (ArchimatixUtils.paletteRect.x, 									ArchimatixUtils.paletteRect.y + cur_y + lineHgt / 2);
		shp.differenceRail.outputPoint 	= new Vector2 (ArchimatixUtils.paletteRect.x + ArchimatixUtils.paletteRect.width, 	ArchimatixUtils.paletteRect.y + cur_y + lineHgt / 2);
		
		bRect.y = cur_y-1;
		
		cur_y = ParameterSplineGUI.OnGUI_Spline(new Rect(x1, cur_y, width, lineHgt), editor, shp.differenceRail);
		
		tmp_boolval = (shp.combineType == AXShape.CombineType.DifferenceRail);
		EditorGUI.BeginChangeCheck ();
		tmp_boolval = EditorGUI.Toggle (bRect, "",  tmp_boolval); 
		if (EditorGUI.EndChangeCheck ()) {
			Undo.RegisterCompleteObjectUndo (shp.Parent.model, "value change for combineType" );
			shp.combineType = AXShape.CombineType.DifferenceRail;
			shp.parametricObject.model.generate();
		}
		
		cur_y += gap;
		
		
		
		// INTERSECTION
		shp.intersection.inputPoint 	= new Vector2 (ArchimatixUtils.paletteRect.x, 									ArchimatixUtils.paletteRect.y + cur_y + lineHgt / 2);
		shp.intersection.outputPoint 	= new Vector2 (ArchimatixUtils.paletteRect.x + ArchimatixUtils.paletteRect.width, 	ArchimatixUtils.paletteRect.y + cur_y + lineHgt / 2);
		bRect.y = cur_y-1;
		
		cur_y = ParameterSplineGUI.OnGUI_Spline(new Rect(x1, cur_y, width, lineHgt), editor, shp.intersection);
		
		tmp_boolval = (shp.combineType == AXShape.CombineType.Intersection);
		EditorGUI.BeginChangeCheck ();
		tmp_boolval = EditorGUI.Toggle (bRect, "",  tmp_boolval);
		if (EditorGUI.EndChangeCheck ()) {
			Undo.RegisterCompleteObjectUndo (shp.Parent.model, "value change for combineType" );
			shp.combineType = AXShape.CombineType.Intersection;
			shp.parametricObject.model.generate();
		}
		
		
		// INTERSECTION_RAIL
		shp.intersectionRail.inputPoint 	= new Vector2 (ArchimatixUtils.paletteRect.x, 									ArchimatixUtils.paletteRect.y + cur_y + lineHgt / 2);
		shp.intersectionRail.outputPoint 	= new Vector2 (ArchimatixUtils.paletteRect.x + ArchimatixUtils.paletteRect.width, 	ArchimatixUtils.paletteRect.y + cur_y + lineHgt / 2);
		bRect.y = cur_y-1;
		
		cur_y = ParameterSplineGUI.OnGUI_Spline(new Rect(x1, cur_y, width, lineHgt), editor, shp.intersectionRail);
		
		tmp_boolval = (shp.combineType == AXShape.CombineType.IntersectionRail);
		EditorGUI.BeginChangeCheck ();
		tmp_boolval = EditorGUI.Toggle (bRect, "",  tmp_boolval);
		if (EditorGUI.EndChangeCheck ()) {
			Undo.RegisterCompleteObjectUndo (shp.Parent.model, "value change for combineType" );
			shp.combineType = AXShape.CombineType.IntersectionRail;
			shp.parametricObject.model.generate();
		}
		
		cur_y += gap;
		
		
		
		// union
		shp.union.inputPoint 	= new Vector2 (ArchimatixUtils.paletteRect.x, 									ArchimatixUtils.paletteRect.y + cur_y + lineHgt / 2);
		shp.union.outputPoint 	= new Vector2 (ArchimatixUtils.paletteRect.x + ArchimatixUtils.paletteRect.width, 	ArchimatixUtils.paletteRect.y + cur_y + lineHgt / 2);
		bRect.y = cur_y-1;
		
		cur_y = ParameterSplineGUI.OnGUI_Spline(new Rect(x1, cur_y, width, lineHgt), editor, shp.union);
		
		tmp_boolval = (shp.combineType == AXShape.CombineType.Union);
		EditorGUI.BeginChangeCheck ();
		tmp_boolval = EditorGUI.Toggle (bRect, "",  tmp_boolval);
		if (EditorGUI.EndChangeCheck ()) {
			Undo.RegisterCompleteObjectUndo (shp.Parent.model, "value change for combineType" );
			shp.combineType = AXShape.CombineType.Union;
			shp.parametricObject.model.generate();
		}
		
		cur_y += gap;
		
		
		
		// grouped
		if (shp.grouped == null || shp.grouped.Type == AXParameter.DataType.Float)
			shp.grouped 			= shp.createSplineParameter(AXParameter.ParameterType.Output, "Grouped");
		
		shp.grouped.inputPoint 	= new Vector2 (ArchimatixUtils.paletteRect.x, 									ArchimatixUtils.paletteRect.y + cur_y + lineHgt / 2);
		shp.grouped.outputPoint 	= new Vector2 (ArchimatixUtils.paletteRect.x + ArchimatixUtils.paletteRect.width, 	ArchimatixUtils.paletteRect.y + cur_y + lineHgt / 2);
		bRect.y = cur_y-1;
		
		cur_y = ParameterSplineGUI.OnGUI_Spline(new Rect(x1, cur_y, width, lineHgt), editor, shp.grouped);
		
		tmp_boolval = (shp.combineType == AXShape.CombineType.Grouped);
		EditorGUI.BeginChangeCheck ();
		tmp_boolval = EditorGUI.Toggle (bRect, "",  tmp_boolval);
		if (EditorGUI.EndChangeCheck ()) {
			Undo.RegisterCompleteObjectUndo (shp.Parent.model, "value change for combineType" );
			shp.combineType = AXShape.CombineType.Grouped;
			shp.parametricObject.model.generate();
		}
		
		
		
		
		ArchimatixUtils.cur_x -= ArchimatixUtils.indent;
		
		
	
		//Archimatix.cur_x -= Archimatix.indent;
		cur_y += lineHgt*2;

		return cur_y;


	}
	




}
