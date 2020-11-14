using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

using AXGeometry;

using AX;

namespace AXEditor
{

	public class AXNodeGraph_Footer  {

		
		// return the height of this gui area
		public static void OnGUI(Rect footerRect, AXNodeGraphEditorWindow editor) 
		{
			Event e = Event.current;

			AXModel model = editor.model;



			float statusBarY = footerRect.y;

			GUI.Box (footerRect, GUIContent.none);

			float bSize = 32;

			Rect vButtonRect	= new Rect (4, statusBarY-3, 	bSize, bSize);


			GUIStyle s = new GUIStyle(EditorStyles.label);
			Color oldColor = s.normal.textColor;

			s.alignment = TextAnchor.MiddleLeft;
			s.normal.textColor = ArchimatixEngine.AXGUIColors["GrayText"];
			s.fixedWidth = 120;

			Color vcolor = Color.white;
			vcolor.a = .5f;

			GUI.color = vcolor;
			GUI.Label (vButtonRect, "AX v" + ArchimatixEngine.version, s);
			GUI.color = Color.white;

			 

			Rect mButtonRect	= new Rect (90, statusBarY, 	bSize, bSize);
			Rect tooltipRect 	= new Rect (mButtonRect.x-10, statusBarY-25, 	100, bSize);

			GUIStyle labelstyle = GUI.skin.GetStyle ("Label");
			labelstyle.alignment = TextAnchor.MiddleLeft;

			Color prevGUIColor = GUI.color;

			Color gcol = GUI.color;
			GUI.backgroundColor = Color.gray;

			tooltipRect.x 	= mButtonRect.x-10;
			// BUTTON: Close All Controls
			if (mButtonRect.Contains(Event.current.mousePosition)) // TOOLTIP
			{
				gcol.a = .8f;
				GUI.Label (tooltipRect, "Close All Controls");
			}
			else
				gcol.a = .5f;
			GUI.color = gcol;
			if (GUI.Button ( mButtonRect, editor.CloseAllControlsIcon))
			{ 
				editor.closeAllControls ();
			}


			// BUTTON: Close All Tools
			mButtonRect.x 	+= bSize + 3;
			tooltipRect.x 	= mButtonRect.x-10;

			if (mButtonRect.Contains(Event.current.mousePosition))// TOOLTIP
			{
				gcol.a = .8f;
				GUI.color = gcol;
				GUI.Label (tooltipRect, "Close All Tools");
			}
			else
				gcol.a = .5f;
			GUI.color = gcol;
			if (GUI.Button (mButtonRect, editor.CloseAllToolsIcon ))
			{ 
				editor.closeTools ();
			}


			// BUTTON: Show All Nodes
			mButtonRect.x 	+= bSize + 3;
			tooltipRect.x 	= mButtonRect.x-10;

			if (mButtonRect.Contains(Event.current.mousePosition))// TOOLTIP
			{
				gcol.a = .8f;
				GUI.color = gcol;
				GUI.Label (tooltipRect, "Show All Nodes");
			}
			else
				gcol.a = .5f;
			GUI.color = gcol;
			if (GUI.Button (mButtonRect, editor.ShowAllNodesIcon))
			{
				foreach(AXParametricObject po in model.parametricObjects)
					po.isOpen = true;

			}





            // BUTTON: CLEAN CURRENT GROUPER
            if (editor.model.currentWorkingGroupPO != null)
            {
                mButtonRect.x += bSize + 3;
                tooltipRect.x = mButtonRect.x - 10;

                if (mButtonRect.Contains(Event.current.mousePosition))// TOOLTIP
                {
                    gcol.a = .8f;
                    GUI.color = gcol;
                    GUI.Label(tooltipRect, "Clean up Grouper");
                }
                else
                    gcol.a = .5f;
                GUI.color = gcol;
                if (GUI.Button(mButtonRect, editor.CleanCurrentGrouperIcon))
                {
                    editor.model.currentWorkingGroupPO.cleanupGrouper();
                    

                }
            }





            // zoomScale

            mButtonRect.x 	+= bSize + 3;
			tooltipRect.x 	= mButtonRect.x-10;
			mButtonRect.width = 45;
			if (mButtonRect.Contains(Event.current.mousePosition))// TOOLTIP
			{
				gcol.a = .8f;
				GUI.color = gcol;
				GUI.Label (tooltipRect, "Zoom Scale");
			}
			else
				gcol.a = .5f;
			GUI.color = gcol;
			if (GUI.Button (mButtonRect, (""+(model.zoomScale*100))+"%"))
			{

				AXNodeGraphEditorWindow.zoomScale = 1;
				model.zoomScale = 1;
				editor.Repaint();
			}





			GUI.color = prevGUIColor;


			//GUI.Label (new Rect (position.width / 2, statusBarY + 10, 100, 20), "Archimatix v " + ArchimatixEngine.version);



			if (model != null)
			{

				//Debug.Log("model.stats_TriangleCount="+model.stats_TriangleCount);
				EditorGUI.LabelField(new Rect(editor.position.width-335, statusBarY+7, 100, 20), "Vertices: " + model.stats_VertCount,s);
				EditorGUI.LabelField(new Rect(editor.position.width-230, statusBarY+7, 100, 20), "Triangles: " + model.stats_TriangleCount, s);



				EditorGUI.BeginChangeCheck();
				EditorGUIUtility.labelWidth = 70;
				//model.segmentReductionFactor = EditorGUI.Slider( new Rect(position.width-120, statusBarY+7, 115, 20), "Detail Level", model.segmentReductionFactor, 0, 1);
				model.segmentReductionFactor = EditorGUI.Slider( new Rect(editor.position.width-120, statusBarY+7, 115, 20), "Detail Level", model.segmentReductionFactor, 0, 1);
				if (EditorGUI.EndChangeCheck())
				{
					Undo.RegisterCompleteObjectUndo (model, "Segment Reduction");
					model.isAltered();
				}

			}

			Handles.BeginGUI( );
			Handles.color = Color.gray;
			Handles.DrawLine( 
				new Vector3(0, statusBarY, 0),
				new Vector3(editor.position.width, statusBarY, 0));
			Handles.EndGUI ();


			s.normal.textColor = oldColor;



		}

	}
}