#pragma warning disable 0618 // SphereHandleCap obsolete - use SphereHandleCap

using UnityEngine;
using UnityEditor;

using System; 
using System.Collections;
using System.Collections.Generic;

using AX;
using AXEditor;
using AX.Generators;

namespace AX.GeneratorHandlers
{
	
	
	public class ToolHandler: GeneratorHandler3D 
	{

	}



	public class RadialRepeaterToolHandler: GeneratorHandler3D 
	{

		public override void drawBoundsHandles(Matrix4x4 consumerM, bool forceDraw=false)
		{

		
			RadialRepeaterTool gener = generator as RadialRepeaterTool;


			Matrix4x4 prevHandlesMatrix = Handles.matrix;
		


			float y = .5f*HandleUtility.GetHandleSize(Vector3.zero);

			Handles.matrix *= Matrix4x4.TRS (new Vector3 (0, 0, 0), Quaternion.identity, Vector3.one);

			Vector3 pos;

			Color lightLineColor = new Color (1, .8f, .6f, .7f);
			Color brightOrange 	 = new Color(1, .5f, 0, .9f);


			// RADIUS //

			Handles.color = brightOrange;

			Handles.DrawLine(new Vector3(gener.radius, y, 0), new Vector3(gener.radius, -y, 0));
			Handles.DrawLine(new Vector3(0, 0, 0), new Vector3(0, -y, 0));

			Handles.DrawLine(new Vector3(0, -y*.66f, 0), new Vector3(gener.radius, -y*.66f, 0));

			// RADIUS LABEL
			GUIStyle labelStyle = new GUIStyle();
			labelStyle.alignment = TextAnchor.MiddleCenter;
			labelStyle.normal.textColor = Color.white;

			Handles.Label(new Vector3(gener.radius/2, -y, 0), "rad="+System.Math.Round(gener.radius, 2), labelStyle);


			// RADIUS HANDLE
			pos = new Vector3(gener.radius, 0, 0);
			EditorGUI.BeginChangeCheck();
			pos = Handles.FreeMoveHandle(
				pos, 
				Quaternion.identity,
				.15f*HandleUtility.GetHandleSize(pos),
				Vector3.zero, 
				Handles.SphereHandleCap
			);
			if(EditorGUI.EndChangeCheck())
			{
				Undo.RegisterCompleteObjectUndo (parametricObject.model, "Radius");
				//.parametricObject.setAltered();
				gener.P_Radius.initiatePARAMETER_Ripple_setFloatValueFromGUIChange( pos.x );
				gener.parametricObject.model.isAltered(23);

				if (generator.P_Output != null && generator.P_Output.Dependents != null)
				{
					for (int i = 0; i < generator.P_Output.Dependents.Count; i++) 
						generator.P_Output.Dependents [i].parametricObject.generator.adjustWorldMatrices ();
				}
			}



			Handles.color = new Color (1, .8f, .6f, .05f);

			Handles.DrawSolidArc(Vector3.zero, 
				Vector3.up, 
				Vector2.right, 
				-gener.size, 
				gener.radius);

			Handles.DrawSolidArc(Vector3.zero, 
				Vector3.up, 
				Vector2.right, 
				-gener.actualBay, 
				gener.radius/2);


			Handles.color = lightLineColor;
			Handles.DrawLine(new Vector3(0, 0, 0), new Vector3(gener.radius, 0, 0));



			Handles.DrawWireArc(Vector3.zero, 
				Vector3.up, 
				Vector2.right, 
				360, 
				gener.radius);
			



			// BAY ANGLE SWEEP
			Handles.color = new Color (0, .5f, 1f, .1f);

			Quaternion rotb = Quaternion.Euler (0, 360-gener.bay, 0);
			EditorGUI.BeginChangeCheck();
			rotb = Handles.Disc(rotb, Vector3.zero, Vector3.up, gener.radius/2, false, 1);

			if (EditorGUI.EndChangeCheck ()) {
				Undo.RegisterCompleteObjectUndo (parametricObject.model, "RadialRepeat Total Angle");

				gener.P_Bay.initiatePARAMETER_Ripple_setFloatValueFromGUIChange( 360-rotb.eulerAngles.y );

				gener.parametricObject.model.isAltered(23);
				for (int i = 0; i < generator.P_Output.Dependents.Count; i++) 
					generator.P_Output.Dependents [i].parametricObject.generator.adjustWorldMatrices ();

			}







			// TOTAL ANGLE SWEEP DISC
			Handles.color = new Color (1, .5f, 0f, 1f);

			Quaternion rot = Quaternion.Euler (0, 360-gener.size, 0);
			EditorGUI.BeginChangeCheck();

			rot = Handles.Disc(rot, Vector3.zero, Vector3.up, gener.radius*.9f, false, 1);

			if (EditorGUI.EndChangeCheck ()) {
				Undo.RegisterCompleteObjectUndo (parametricObject.model, "Lathe SweepAngle");

				gener.P_Size.initiatePARAMETER_Ripple_setFloatValueFromGUIChange( 360-rot.eulerAngles.y );

				gener.parametricObject.model.isAltered(30);
				for (int i = 0; i < generator.P_Output.Dependents.Count; i++) 
					generator.P_Output.Dependents [i].parametricObject.generator.adjustWorldMatrices ();

			}


            Event e = Event.current;

			Matrix4x4 pushM = Handles.matrix;

			Matrix4x4 deltaM = Matrix4x4.TRS (Vector3.zero, Quaternion.Euler (0, 360 - gener.actualBay, 0), Vector3.one);
			for (int i=0; i<gener.cells; i++)
			{
				Handles.matrix *= deltaM;

				if (i == 0)
				{
					Handles.color = lightLineColor;
					Handles.DrawLine(new Vector3(0, 0, 0), new Vector3(gener.radius/2, 0, 0));

					Handles.color = brightOrange;
					Handles.DrawLine(new Vector3(gener.radius/2-.5f, 0, 0), new Vector3(gener.radius/2+.5f, 0, 0));

					Handles.SphereHandleCap(0,
						new Vector3(gener.radius/2, 0, 0),
						Quaternion.identity,
						.15f*HandleUtility.GetHandleSize(pos), e.type);

				}
				Handles.DrawLine(new Vector3(gener.radius-.5f, 0, 0), new Vector3(gener.radius+.5f, 0, 0));

			}



			// SIZE CAP
			Handles.SphereHandleCap(0,
				new Vector3(gener.radius*.9f, 0, 0),
				Quaternion.identity,
				.15f*HandleUtility.GetHandleSize(pos),
                e.type);


			Handles.Label(new Vector3(gener.radius, -y/2, 0), ""+System.Math.Round(gener.size, 0)+" degs", labelStyle);



	



			float handleSizer = .15f*HandleUtility.GetHandleSize(Vector3.zero);





			// CELL_COUNT CLICKERS

			// (-)


			Handles.color = new Color(.7f, 1f, .7f, .9f);
			pos = new Vector3(gener.radius, -y, -handleSizer*2);
			if(Handles.Button(pos, Quaternion.Euler(0,180,0), handleSizer, handleSizer, Handles.ConeHandleCap))
			{ 
				Undo.RegisterCompleteObjectUndo (parametricObject.model, "Cell Count");

				gener.P_Cells.initiateRipple_setIntValueFromGUIChange(gener.cells-1);

				gener.parametricObject.model.autobuild();
				for (int i = 0; i < generator.P_Output.Dependents.Count; i++) 
					generator.P_Output.Dependents [i].parametricObject.generator.adjustWorldMatrices ();
			}

			// [+]
			Handles.color = new Color(.7f, .7f, 1, .9f);
			pos = new Vector3(gener.radius, -y, handleSizer*2);
			if(Handles.Button(pos, Quaternion.Euler(0,0,0), handleSizer, handleSizer, Handles.ConeHandleCap))
			{ 
				Undo.RegisterCompleteObjectUndo (parametricObject.model, "Cell Count");

				gener.P_Cells.initiateRipple_setIntValueFromGUIChange(gener.cells+1);

				gener.parametricObject.model.autobuild();
				for (int i = 0; i < generator.P_Output.Dependents.Count; i++) 
					generator.P_Output.Dependents [i].parametricObject.generator.adjustWorldMatrices ();
			}

			Handles.Label(new Vector3(gener.radius+.2f, 0, 0f), ""+(gener.cells) + " bays", labelStyle);

			Handles.matrix = pushM;


			// FINISH
			Handles.matrix = prevHandlesMatrix;


		}






	}




	public class RepeaterToolHandler: GeneratorHandler3D 
	{


		public override void drawBoundsHandles(Matrix4x4 consumerM, bool forceDraw=false)
		{
			//base.drawBoundsHandles(consumerM, forceDraw);



			RepeaterTool repeaterToolU = generator as RepeaterTool;



			Matrix4x4 prevHandlesMatrix = Handles.matrix;

			float width = repeaterToolU.bay/3;
			float thick = .1f;
			float alpha = .3f;

			//Handles.matrix = consumerM; // * consumerM * parametricObject.getAxisRotationMatrix() * generator.getLocalAlignMatrix() ;

			Color prevColor = Handles.color;
			Handles.color = new Color(1, .5f, 0);
			Handles.DrawLine(new Vector3(-repeaterToolU.size/2, 0, 0), new Vector3(repeaterToolU.size/2, 0, 0));

			Handles.color = new Color(1, .5f, 0, alpha);
			Handles.DrawLine(new Vector3(-repeaterToolU.size/2, 0, thick), new Vector3(repeaterToolU.size/2, 0, thick));
			Handles.DrawLine(new Vector3(-repeaterToolU.size/2, 0, -thick), new Vector3(repeaterToolU.size/2, 0, -thick));


			float cursor = -repeaterToolU.size/2;

			for (int i=0; i<=repeaterToolU.cells; i++)
			{
				Handles.color = new Color(1, .5f, 0);
				Handles.DrawLine(new Vector3(cursor, 0, -width), new Vector3(cursor, 0, width));

				Handles.color = new Color(1, .5f, 0, alpha);
				Handles.DrawLine(new Vector3(cursor+thick, 0, -width), new Vector3(cursor+thick, 0, width));
				Handles.DrawLine(new Vector3(cursor-thick, 0, -width), new Vector3(cursor-thick, 0, width));

				cursor += repeaterToolU.actualBay;
			}

			bool isEven = (repeaterToolU.cells % 2 == 0);

			isEven = false;

			GUIStyle labelStyle = new GUIStyle();
        	labelStyle.alignment = TextAnchor.MiddleCenter;
        	labelStyle.normal.textColor = Color.white;



			// Dimension Lines

			float handleSize = .15f*HandleUtility.GetHandleSize(Vector3.zero);

			float hgtSpan 	= handleSize*8f;

			hgtSpan = Mathf.Clamp(hgtSpan, .25f, 100);

			float hgtBay 	= hgtSpan*.5f;

			float lhgtBay 	= hgtBay*.75f;
			float lhgtSpan 	= hgtSpan*.75f;

			float hgt = hgtBay;
			float lhgt = lhgtBay;



			float left  = (isEven) ?  0 					  : -repeaterToolU.actualBay/2;
			float right = (isEven) ?  repeaterToolU.actualBay :  repeaterToolU.actualBay/2 ;
			float len = .05f;

			Vector3 pos = new Vector3(right, -lhgt, 0);



			Handles.color = new Color(1, 1f, .5f, alpha);
			Handles.DrawLine(new Vector3(left, 0, 0), new Vector3(left, -hgt, 0));
			Handles.DrawLine(new Vector3(right, 0, 0), new Vector3(right, -hgt, 0));
			Handles.DrawLine(new Vector3(left, -lhgt, 0), new Vector3(right, -lhgt, 0));
			Handles.DrawLine(new Vector3(right+len, -lhgt-len, 0), new Vector3(right-len, -lhgt+len, 0));
			Handles.DrawLine(new Vector3(left+len, -lhgt-len, 0), new Vector3(left-len, -lhgt+len, 0));

			Handles.Label(new Vector3((isEven) ? -.25f*repeaterToolU.bay : .25f*repeaterToolU.bay, -lhgt*1.1f, 0), ""+System.Math.Round(repeaterToolU.actualBay, 2), labelStyle);

			//EditorGUI.



			// BAY_SIZE
			Handles.color = new Color(1, .5f, .5f, .3f);
			EditorGUI.BeginChangeCheck();
			pos = Handles.FreeMoveHandle(
				pos, 
				Quaternion.identity,
				handleSize,
				Vector3.zero, 
				Handles.SphereHandleCap
				);
			if(EditorGUI.EndChangeCheck())
			{
				Undo.RegisterCompleteObjectUndo (parametricObject.model, "Bay Size");

				repeaterToolU.P_Bay.initiatePARAMETER_Ripple_setFloatValueFromGUIChange( ((isEven) ? pos.x : pos.x+repeaterToolU.bay/2 ) );
				repeaterToolU.parametricObject.model.isAltered(22);

				if (generator.P_Output != null && generator.P_Output.Dependents != null)
				{
					for (int i = 0; i < generator.P_Output.Dependents.Count; i++) 
						generator.P_Output.Dependents [i].parametricObject.generator.adjustWorldMatrices ();
				}
			}





			// SIZE DIMENSION LINE

			hgt 	=  hgtSpan;
			lhgt 	=  lhgtSpan;
			left  	= -repeaterToolU.size/2;
			right 	=  repeaterToolU.size/2 ;

			Handles.color = new Color(1, 1f, .5f, alpha);
			Handles.DrawLine(new Vector3(left, 0, 0), new Vector3(left, -hgt, 0));
			Handles.DrawLine(new Vector3(right, 0, 0), new Vector3(right, -hgt, 0));
			Handles.DrawLine(new Vector3(left, -lhgt, 0), new Vector3(right, -lhgt, 0));

			Handles.DrawLine(new Vector3(right+len, -lhgt-len, 0), new Vector3(right-len, -lhgt+len, 0));
			Handles.DrawLine(new Vector3(left+len, -lhgt-len, 0), new Vector3(left-len, -lhgt+len, 0));


			Handles.Label(new Vector3((isEven) ? -.25f*repeaterToolU.size : -.25f*repeaterToolU.size, -lhgt*1.1f, 0), ""+System.Math.Round(repeaterToolU.size, 2), labelStyle);

			// SIZE (RIGHT)

			pos = new Vector3(right, -lhgt, 0);
			Handles.color = new Color(1, .5f, 0, .9f);
			EditorGUI.BeginChangeCheck();
			pos = Handles.FreeMoveHandle(
				pos, 
				Quaternion.identity,
				.15f*HandleUtility.GetHandleSize(pos),
				Vector3.zero, 
				Handles.SphereHandleCap
				);
			if(EditorGUI.EndChangeCheck())
			{
				Undo.RegisterCompleteObjectUndo (parametricObject.model, "Size");
				//.parametricObject.setAltered();
				repeaterToolU.parametricObject.getParameter("Size").initiatePARAMETER_Ripple_setFloatValueFromGUIChange( pos.x*2 );
				repeaterToolU.parametricObject.model.isAltered(23);
				for (int i = 0; i < generator.P_Output.Dependents.Count; i++) 
					generator.P_Output.Dependents [i].parametricObject.generator.adjustWorldMatrices ();
			}


			// SIZE (LEFT)
			pos = new Vector3(left, -lhgt, 0);
			Handles.color = new Color(1, .5f, 0, .9f);
			EditorGUI.BeginChangeCheck();
			pos = Handles.FreeMoveHandle(
				pos, 
				Quaternion.identity,
				.15f*HandleUtility.GetHandleSize(pos),
				Vector3.zero, 
				Handles.SphereHandleCap
				);
			if(EditorGUI.EndChangeCheck())
			{
				Undo.RegisterCompleteObjectUndo (parametricObject.model, "Size");
				//repeaterToolU.parametricObject.setAltered();
				repeaterToolU.parametricObject.getParameter("Size").initiatePARAMETER_Ripple_setFloatValueFromGUIChange( -pos.x*2 );
				repeaterToolU.parametricObject.model.isAltered(24);
				for (int i = 0; i < generator.P_Output.Dependents.Count; i++) 
					generator.P_Output.Dependents [i].parametricObject.generator.adjustWorldMatrices ();
			}

			 


			// CELL_COUNT CLICKERS

			// (-)
			Handles.color = new Color(.7f, .7f, 1, .9f);
			pos = new Vector3(left-handleSize*1f, -lhgtBay, 0);
			if(Handles.Button(pos, Quaternion.Euler(0,-90,0), .15f*HandleUtility.GetHandleSize(pos), .15f*HandleUtility.GetHandleSize(pos), Handles.ConeHandleCap))
			{ 
				Undo.RegisterCompleteObjectUndo (parametricObject.model, "Cell Count");
				repeaterToolU.parametricObject.getParameter("Cells").initiateRipple_setIntValueFromGUIChange(repeaterToolU.parametricObject.intValue("Cells")-1);
				repeaterToolU.parametricObject.model.autobuild();
				for (int i = 0; i < generator.P_Output.Dependents.Count; i++) 
					generator.P_Output.Dependents [i].parametricObject.generator.adjustWorldMatrices ();
			}

			// [+]
			Handles.color = new Color(.7f, 1f, .7f, .9f);
			pos = new Vector3(left+handleSize*1f, -lhgtBay, 0);
			if(Handles.Button(pos, Quaternion.Euler(0,90,0), .15f*HandleUtility.GetHandleSize(pos), .15f*HandleUtility.GetHandleSize(pos), Handles.ConeHandleCap))
			{ 
				Undo.RegisterCompleteObjectUndo (parametricObject.model, "Cell Count");
				repeaterToolU.parametricObject.getParameter("Cells").initiateRipple_setIntValueFromGUIChange(repeaterToolU.parametricObject.intValue("Cells")+1);
				repeaterToolU.parametricObject.model.autobuild();
				for (int i = 0; i < generator.P_Output.Dependents.Count; i++) 
					generator.P_Output.Dependents [i].parametricObject.generator.adjustWorldMatrices ();
			}

			Handles.Label(new Vector3(left, -lhgtBay, 0), ""+(repeaterToolU.cells), labelStyle);




			// EDGE_COUNT CLICKERS

			// (-)
			if (repeaterToolU.edgeCount < 90)
			{
				Handles.color = new Color(.7f, .7f, 1, .6f);
				pos = new Vector3(left-handleSize*1f, -lhgtBay/2, 0);
				if(Handles.Button(pos, Quaternion.Euler(0,-90,0), .13f*HandleUtility.GetHandleSize(pos), .13f*HandleUtility.GetHandleSize(pos), Handles.ConeHandleCap))
				{ 
					Undo.RegisterCompleteObjectUndo (parametricObject.model, "Edge Count");
					repeaterToolU.parametricObject.getParameter("Edge_Count").initiateRipple_setIntValueFromGUIChange(repeaterToolU.parametricObject.intValue("Edge_Count")-1);
					repeaterToolU.parametricObject.model.autobuild();
					for (int i = 0; i < generator.P_Output.Dependents.Count; i++) 
						generator.P_Output.Dependents [i].parametricObject.generator.adjustWorldMatrices ();
				}

				// [+]
				Handles.color = new Color(.7f, 1f, .7f, .6f);
				pos = new Vector3(left+handleSize*1f, -lhgtBay/2, 0);
				if(Handles.Button(pos, Quaternion.Euler(0,90,0), .13f*HandleUtility.GetHandleSize(pos), .13f*HandleUtility.GetHandleSize(pos), Handles.ConeHandleCap))
				{ 
					Undo.RegisterCompleteObjectUndo (parametricObject.model, "Edge Count");
					repeaterToolU.parametricObject.getParameter("Edge_Count").initiateRipple_setIntValueFromGUIChange(repeaterToolU.parametricObject.intValue("Edge_Count")+1);
					repeaterToolU.parametricObject.model.autobuild();
					for (int i = 0; i < generator.P_Output.Dependents.Count; i++) 
						generator.P_Output.Dependents [i].parametricObject.generator.adjustWorldMatrices ();
				}

				Handles.Label(new Vector3(left, -lhgtBay/2, 0), ""+(repeaterToolU.edgeCount), labelStyle);
			}





			Handles.matrix = prevHandlesMatrix;
			Handles.color = prevColor;

		}
		
		
	}





	public class FloorRepeaterToolHandler: GeneratorHandler3D 
	{
		

		public override void drawBoundsHandles(Matrix4x4 consumerM, bool forceDraw=false)
		{
			base.drawBoundsHandles(consumerM, forceDraw);



			RepeaterTool repeaterTool = generator as RepeaterTool;


			Matrix4x4 prevHandlesMatrix = Handles.matrix;

			float width = repeaterTool.bay/3;
			float thick = .5f;
			float alpha = .3f;

			Handles.matrix = consumerM; // * consumerM * parametricObject.getAxisRotationMatrix() * generator.getLocalAlignMatrix() ;

			Color prevColor = Handles.color;
			Handles.color = new Color(1, .5f, 0);
			Handles.DrawLine(new Vector3(-repeaterTool.size/2, 0, 0), new Vector3(repeaterTool.size/2, 0, 0));

			Handles.color = new Color(1, .5f, 0, alpha);
			Handles.DrawLine(new Vector3(-repeaterTool.size/2, 0, thick), new Vector3(repeaterTool.size/2, 0, thick));
			Handles.DrawLine(new Vector3(-repeaterTool.size/2, 0, -thick), new Vector3(repeaterTool.size/2, 0, -thick));


			float cursor = -repeaterTool.size/2;

			for (int i=0; i<=repeaterTool.cells; i++)
			{
				Handles.color = new Color(1, .5f, 0);
				Handles.DrawLine(new Vector3(cursor, 0, -width), new Vector3(cursor, 0, width));

				Handles.color = new Color(1, .5f, 0, alpha);
				Handles.DrawLine(new Vector3(cursor+thick, 0, -width), new Vector3(cursor+thick, 0, width));
				Handles.DrawLine(new Vector3(cursor-thick, 0, -width), new Vector3(cursor-thick, 0, width));

				cursor += repeaterTool.actualBay;
			}

			bool isEven = (repeaterTool.cells % 2 == 0);

			isEven = false;




			// Dimension Lines

			float handleSize = .15f*HandleUtility.GetHandleSize(Vector3.zero);

			float hgtSpan 	= handleSize*8f;

			hgtSpan = Mathf.Clamp(hgtSpan, .25f, 100);

			float hgtBay 	= hgtSpan*.5f;

			float lhgtBay 	= hgtBay*.75f;
			float lhgtSpan 	= hgtSpan*.75f;

			float hgt = hgtBay;
			float lhgt = lhgtBay;



			float left  = (isEven) ?  0 					  : -repeaterTool.actualBay/2;
			float right = (isEven) ?  repeaterTool.actualBay :  repeaterTool.actualBay/2 ;
			float len = .05f;

			Vector3 pos = new Vector3(right, -lhgt, 0);



			Handles.color = new Color(1, 1f, .5f, alpha);
			Handles.DrawLine(new Vector3(left, 0, 0), new Vector3(left, -hgt, 0));
			Handles.DrawLine(new Vector3(right, 0, 0), new Vector3(right, -hgt, 0));
			Handles.DrawLine(new Vector3(left, -lhgt, 0), new Vector3(right, -lhgt, 0));
			Handles.DrawLine(new Vector3(right+len, -lhgt-len, 0), new Vector3(right-len, -lhgt+len, 0));
			Handles.DrawLine(new Vector3(left+len, -lhgt-len, 0), new Vector3(left-len, -lhgt+len, 0));

			Handles.Label(new Vector3((isEven) ? repeaterTool.bay/2 : 0, -lhgt*1.1f, 0), ""+System.Math.Round(repeaterTool.actualBay, 2), ArchimatixEngine.sceneViewLabelStyle);

			//EditorGUI.



			Handles.color = new Color(1, .5f, 0, .9f);
			EditorGUI.BeginChangeCheck();
			pos = Handles.FreeMoveHandle(
				pos, 
				Quaternion.identity,
				handleSize,
				Vector3.zero, 
				Handles.SphereHandleCap
				);
			if(EditorGUI.EndChangeCheck())
			{
				Undo.RegisterCompleteObjectUndo (parametricObject.model, "Bay Size");

				repeaterTool.P_Bay.initiatePARAMETER_Ripple_setFloatValueFromGUIChange( ((isEven) ? pos.x : pos.x+repeaterTool.bay/2 ) );
				repeaterTool.parametricObject.model.isAltered(25);

			}





			// SIZE DIMENSION LINE

			hgt = hgtSpan;
			lhgt = lhgtSpan;
			left  = -repeaterTool.size/2;
			right =  repeaterTool.size/2 ;

			Handles.color = new Color(1, 1f, .5f, alpha);
			Handles.DrawLine(new Vector3(left, 0, 0), new Vector3(left, -hgt, 0));
			Handles.DrawLine(new Vector3(right, 0, 0), new Vector3(right, -hgt, 0));
			Handles.DrawLine(new Vector3(left, -lhgt, 0), new Vector3(right, -lhgt, 0));

			Handles.DrawLine(new Vector3(right+len, -lhgt-len, 0), new Vector3(right-len, -lhgt+len, 0));
			Handles.DrawLine(new Vector3(left+len, -lhgt-len, 0), new Vector3(left-len, -lhgt+len, 0));


			Handles.Label(new Vector3((isEven) ? repeaterTool.bay/2 : 0, -lhgt*1.1f, 0), ""+System.Math.Round(repeaterTool.size, 2), ArchimatixEngine.sceneViewLabelStyle);


			pos = new Vector3(right, -lhgt, 0);
			Handles.color = new Color(1, .5f, 0, .9f);
			EditorGUI.BeginChangeCheck();
			pos = Handles.FreeMoveHandle(
				pos, 
				Quaternion.identity,
				.15f*HandleUtility.GetHandleSize(pos),
				Vector3.zero, 
				Handles.SphereHandleCap
				);
			if(EditorGUI.EndChangeCheck())
			{
				Undo.RegisterCompleteObjectUndo (parametricObject.model, "Size");

				repeaterTool.parametricObject.getParameter("Size").initiatePARAMETER_Ripple_setFloatValueFromGUIChange( pos.x*2 );
				repeaterTool.parametricObject.model.isAltered(26);

			}


			pos = new Vector3(right-handleSize*1f, -lhgtBay, 0);
			if(Handles.Button(pos, Quaternion.Euler(0,-90,0), .15f*HandleUtility.GetHandleSize(pos), .15f*HandleUtility.GetHandleSize(pos), Handles.ConeHandleCap))
			{ 
				Undo.RegisterCompleteObjectUndo (parametricObject.model, "Cell Count");
				repeaterTool.parametricObject.getParameter("Cells").initiateRipple_setIntValueFromGUIChange(repeaterTool.parametricObject.intValue("Cells")-1);
				repeaterTool.parametricObject.model.autobuild();
			}

			pos = new Vector3(left+handleSize*1f, -lhgtBay, 0);
			if(Handles.Button(pos, Quaternion.Euler(0,90,0), .15f*HandleUtility.GetHandleSize(pos), .15f*HandleUtility.GetHandleSize(pos), Handles.ConeHandleCap))
			{ 
				Undo.RegisterCompleteObjectUndo (parametricObject.model, "Cell Count");
				repeaterTool.parametricObject.getParameter("Cells").initiateRipple_setIntValueFromGUIChange(repeaterTool.parametricObject.intValue("Cells")+1);
				repeaterTool.parametricObject.model.autobuild();
			}







			Handles.matrix = prevHandlesMatrix;
			Handles.color = prevColor;

		}
		
		
	}





	public class MaterialToolHandler: GeneratorHandler3D 
	{
		

	
	}




	public class PlaneToolHandler: GeneratorHandler3D 
	{
		

		public override void drawBoundsHandles(Matrix4x4 consumerM, bool forceDraw=false)
		{

			
			//Matrix4x4 prevHandlesMatrix = Handles.matrix;
			
			/*
			Handles.matrix = matrix; // * consumerM * parametricObject.getAxisRotationMatrix() * generator.getLocalAlignMatrix() ;
			
			Handles.color = new Color(1, .5f, 0);
			
			Handles.DrawLine(new Vector3(-10, 0, 0), new Vector3(10, 0, 0));
			Handles.DrawLine(new Vector3(0, 0, -10), new Vector3(0, 0, 10));
			
			
			Handles.matrix = prevHandlesMatrix;
			
			*/
		}
		
		
	}
	
}
