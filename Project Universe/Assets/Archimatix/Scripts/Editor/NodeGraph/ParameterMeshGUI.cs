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


public class ParameterMeshGUI  {





	public static int OnGUI_Mesh(Rect pRect, AXNodeGraphEditorWindow editor, AXParameter p)
	{


		float cur_x = ArchimatixUtils.cur_x; 

		//float box_w = ArchimatixUtils.paletteRect.width - cur_x - 3*ArchimatixUtils.indent;
		float box_w = pRect.width - cur_x - 1*ArchimatixUtils.indent;

		int cur_y = (int)pRect.y;
		int lineHgt = (int)pRect.height;
		int gap = 5;





		Color oldBackgroundColor = GUI.backgroundColor;
		Color dataColor =  editor.getDataColor(p.Type);
		GUI.backgroundColor = dataColor;


		// INPUT
		if (! p.parametricObject.isCurrentGrouper())
		{
			if (editor.OutputParameterBeingDragged == null|| editor.OutputParameterBeingDragged.Type == AXParameter.DataType.Mesh)
			{
				if (p.PType != AXParameter.ParameterType.Output)
				{
					if (GUI.Button (new Rect (-3, cur_y, ArchimatixEngine.buttonSize, ArchimatixEngine.buttonSize), "")) {
						if (editor.OutputParameterBeingDragged != null && editor.OutputParameterBeingDragged.Type != p.Type)
							editor.OutputParameterBeingDragged = null;
						else
							editor.inputSocketClicked (p);
					}
				}
			}
		}


		// OUTPUT
		if (editor.InputParameterBeingDragged == null || editor.InputParameterBeingDragged.Type == AXParameter.DataType.Mesh)
		{
			if (GUI.Button (new Rect (pRect.width+6, cur_y, ArchimatixEngine.buttonSize, ArchimatixEngine.buttonSize), "")) 
			{
				if (editor.OutputParameterBeingDragged != null && editor.OutputParameterBeingDragged.Type != p.Type)
					editor.OutputParameterBeingDragged = null;
				else
					editor.outputSocketClicked (p);
			}
		}

		/*
		// IF EDITABLE
		EditorGUI.BeginChangeCheck ();
		p.isOpen = EditorGUI.Foldout (new Rect (pRect.x , cur_y, 20, lineHgt), p.isOpen, "");
		if(EditorGUI.EndChangeCheck())
		{
			if (p.isOpen )
			{
				foreach(AXParameter pop in p.parametricObject.parameters)
					if (pop != p)
						pop.isOpen = false;
			}
		}
		*/

		// LABEL
		Rect boxRect = new Rect(cur_x + ArchimatixUtils.indent, cur_y, box_w, pRect.height);
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


		GUI.Label(lRect, label);



		GUI.Label (new Rect (10,40,100,40), GUI.tooltip);

		cur_y += lineHgt+gap;








		GUI.backgroundColor = oldBackgroundColor;

		return cur_y;


	}




}
