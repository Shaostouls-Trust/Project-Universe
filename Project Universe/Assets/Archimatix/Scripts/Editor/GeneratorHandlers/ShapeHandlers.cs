#pragma warning disable 0618 // SphereHandleCap obsolete - use SphereHandleCap

using UnityEngine;
using UnityEditor;

using System;
using System.Collections; 
using System.Collections.Generic;

using AXClipperLib;

using Path 		= System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths 	= System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;


using AXGeometry;

using AX;
using AX.Generators;
using AXEditor;

using Parameters 				= System.Collections.Generic.List<AX.AXParameter>;



namespace AX.GeneratorHandlers
{
	
	
	public class ShapeHandler : GeneratorHandler2D 
	{

		
	}
	
	
	
	

	
	
	
	
	
	
	
	public class ShapeOffsetterHandler : GeneratorHandler2D 
	{
		

		public void MyPopupCallback(object obj)
		{

		}


		public override void drawControlHandles(ref List<string> visited, Matrix4x4 consumerM, bool beingDrawnFromConsumer)
		{
			if (alreadyVisited(ref visited, "ShapeOffsetterHandler"))
				return;

			//if (generator.P_Output == null)
			//	return;



			base.drawControlHandles(ref visited, consumerM, beingDrawnFromConsumer);



		}


	}


	public class ShapeDistributerHandler : GeneratorHandler2D 
	{
		public override void drawControlHandlesofInputParametricObjects(ref List<string> visited, Matrix4x4 consumerM, bool beingDrawnFromConsumer)
		{

			AXShape inputShape = (parametricObject.generator as ShapeMerger).S_InputShape;

			if (inputShape == null)
				return;
			
			for (int i = 0; i < inputShape.inputs.Count; i++) {
				
				AXParameter src 	= inputShape.inputs[i].DependsOn;
				
				if (src == null)
					continue;
				
				AXParametricObject srcPO = src.Parent;

				GeneratorHandler src_gh = getGeneratorHandler(srcPO);

				if (src_gh != null)
				{
								
					if (srcPO.generator is ShapeMerger)
					{
						src_gh.drawControlHandlesofInputParametricObjects(ref visited, consumerM, true);
					}
					else
					{
						src_gh.drawTransformHandles	(visited, consumerM, false);
						src_gh.drawControlHandles	(ref visited, consumerM, false);

					}
				}


				//else
				//	gh.drawTransformHandles(visited, consumerM, true);
								
			}


		}

	}
	
	public class ShapeMergerHandler : GeneratorHandler2D 
	{

		public override void drawControlHandles(ref List<string> visited, Matrix4x4 consumerM, bool beingDrawnFromConsumer)
		{
			if (alreadyVisited(ref visited, "ShapeMergerHandler"))
				return;

			Matrix4x4 prevHandlesMatrix = Handles.matrix;


			if (parametricObject.model.isSelected(parametricObject))
			{
				base.drawControlHandles(ref visited, consumerM, beingDrawnFromConsumer);

			}

			// MATRIX

			drawControlHandlesofInputParametricObjects(ref visited, generator.parametricObject.worldDisplayMatrix, true);

			// DRAW THE FINAL MERGED SHAPE

			AXShape inputShape = (parametricObject.generator as ShapeMerger).S_InputShape;
			AXParameter out_p = inputShape.getSelectedOutputParameter();


			Handles.matrix = parametricObject.model.transform.localToWorldMatrix * generator.parametricObject.worldDisplayMatrix * generator.localMatrix.inverse;

			Color mergeShapeColor = Color.cyan;
			mergeShapeColor.a *= .75f;
			Handles.color = mergeShapeColor;

			drawPaths(out_p);

			Handles.matrix = prevHandlesMatrix;
		}
			
		
		
		// ShapeMerger::drawControlHandlesofInputParametricObjects
		public override void drawControlHandlesofInputParametricObjects(ref List<string> visited, Matrix4x4 consumerM, bool beingDrawnFromConsumer)
		{

			AXShape inputShape = (parametricObject.generator as ShapeMerger).S_InputShape;

			if (inputShape == null)
				return;
			
			for (int i = 0; i < inputShape.inputs.Count; i++) {
				
				AXParameter src 	= inputShape.inputs[i].DependsOn;
				
				if (src == null)
					continue;
				
				AXParametricObject srcPO = src.Parent;

				GeneratorHandler src_gh = getGeneratorHandler(srcPO);

				if (src_gh != null)
				{
								
					if (srcPO.generator is ShapeMerger)
					{
						src_gh.drawControlHandlesofInputParametricObjects(ref visited, consumerM, true);
					}
					else
					{
						src_gh.drawTransformHandles	(visited, consumerM, false);
						src_gh.drawControlHandles	(ref visited, consumerM, false);

					}
				}


				//else
				//	gh.drawTransformHandles(visited, consumerM, true);
								
			}


		}
	}




	public class GridRepeater2DHandler : GeneratorHandler2D 
	{


		public override void drawControlHandles(ref List<string> visited, Matrix4x4 consumerM, bool beingDrawnFromConsumer)
		{
			if (alreadyVisited(ref visited, "GridRepeater2DHandler"))
				return;

			Matrix4x4 prevHandlesMatrix = Handles.matrix;

			base.drawControlHandles(ref visited, consumerM, beingDrawnFromConsumer);


			RepeaterTool repeaterToolU = (parametricObject.generator as GridRepeater2D).repeaterToolU;
			RepeaterTool repeaterToolV = (parametricObject.generator as GridRepeater2D).repeaterToolV;



			if (repeaterToolU != null && repeaterToolV != null)
			{
				// DRAW GRID
				Handles.color = new Color(1, .5f, 0, .4f);


				if (! beingDrawnFromConsumer) 
				{
					consumerM *= generator.parametricObject.getLocalMatrix().inverse;
					consumerM *= parametricObject.getAxisRotationMatrix();
				}

				consumerM *= Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(-90, 0, 0), Vector3.one);

				//consumerM *= Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90, 0, 0), Vector3.one);


				Matrix4x4 context = parametricObject.model.transform.localToWorldMatrix * generator.parametricObject.worldDisplayMatrix * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(-90, 0, 0), Vector3.one);

				Handles.matrix = context;

				for (int i = 0; i<=repeaterToolU.cells; i++)
					Handles.DrawLine(new Vector3(-repeaterToolU.size/2+i*repeaterToolU.actualBay, 0, -repeaterToolV.size/2), new Vector3(-repeaterToolU.size/2+i*repeaterToolU.actualBay, 0, repeaterToolV.size/2));

				for (int k = 0; k<=repeaterToolV.cells; k++)
					Handles.DrawLine(new Vector3(-repeaterToolU.size/2, 0, -repeaterToolV.size/2+k*repeaterToolV.actualBay), new Vector3(repeaterToolU.size/2, 0, -repeaterToolV.size/2+k*repeaterToolV.actualBay));

				
				// ASK RepeaterTools to DRAW
				GeneratorHandler gh = getGeneratorHandler(repeaterToolU.parametricObject);

				if (gh != null)
				{
					Handles.matrix = context *  Matrix4x4.TRS(new Vector3(0, 0, -1.3f*repeaterToolV.size/2), Quaternion.Euler(90, 0, 0), Vector3.one);

					gh.drawBoundsHandles(context , true);
					gh.drawControlHandles(ref visited, consumerM , true);
				}
				gh = getGeneratorHandler(repeaterToolV.parametricObject);
				if (gh != null)
				{
					Handles.matrix = context *  Matrix4x4.TRS(new Vector3(1.3f*repeaterToolU.size/2, 0, 0), Quaternion.Euler(90, -90, 0), Vector3.one);
					gh.drawBoundsHandles(context, true);
					gh.drawControlHandles(ref visited, consumerM, true);
				}
			}


			// MATRIX
			//Matrix4x4 inputsM = consumerM;

			//if (beingDrawnFromConsumer)
			//	inputsM *= generator.parametricObject.getLocalMatrix();

			drawControlHandlesofInputParametricObjects(ref visited, consumerM * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90, 0, 0), Vector3.one), true);


			/*
			// DRAW THE FINAL MERGED SHAPE

			if (! beingDrawnFromConsumer) 
				consumerM *= generator.parametricObject.getLocalMatrix().inverse;

			AXShape inputShape = (parametricObject.generator as ShapeMerger).InputShape;
			AXParameter out_p = inputShape.getSelectedOutputParameter();


			if(! inputShape.hasOutputConnected())
				consumerM *= generator.parametricObject.getAxisRotationMatrix();

			Handles.matrix = generator.parametricObject.worldDisplayMatrix;;

			Color mergeShapeColor = Color.cyan;
			mergeShapeColor.a *= .75f;
			Handles.color = mergeShapeColor;

			drawPaths(out_p);
			*/
			Handles.matrix = prevHandlesMatrix;

		}




		// SHAPE_REPEATER_2D :: DRAW_CONTROL_HANDLES_OF_INPUT_PARAMETRIC_OBJECTS
		public override void drawControlHandlesofInputParametricObjects(ref List<string> visited, Matrix4x4 consumerM)
		{
			
			GridRepeater2D gener2D = (generator as GridRepeater2D);
			// Draw the plan AND the section splines.


			// NODE 
			if (  gener2D.nodeSrc_po != null && gener2D.nodeSrc_po.generator != null)
			{
				GeneratorHandler gh = getGeneratorHandler(gener2D.nodeSrc_po);
				
				if (gh != null)
				{
					Matrix4x4 localPlanM = consumerM * generator.getLocalConsumerMatrixPerInputSocket(gener2D.nodeSrc_po);
									
					gh.drawTransformHandles(visited, localPlanM, true);
					gh.drawControlHandles(ref visited, 	 localPlanM, true);
				}
			}

			// SECTION
			if (  gener2D.cellSrc_po != null && gener2D.cellSrc_po.generator != null)
			{
				GeneratorHandler gh = getGeneratorHandler(gener2D.cellSrc_po);
				
				if (gh != null)
				{
					Matrix4x4 localSecM = consumerM * generator.getLocalConsumerMatrixPerInputSocket(gener2D.cellSrc_po);

					gh.drawTransformHandles(visited, localSecM, true);
					gh.drawControlHandles(ref visited, localSecM, true);
				
				}
			}
			
		
			
			
			
		}
	

			
	}












	public class PairRepeater2DHandler : GeneratorHandler2D 
	{
		//public override void drawBoundsHandles(Matrix4x4 consumerM, bool forceDraw=false)
		public override void drawControlHandles(ref List<string> visited, Matrix4x4 consumerM, bool beingDrawnFromConsumer)
		{
			if (alreadyVisited(ref visited, "PairRepeater2DHandler"))
				return;

			//base.drawBoundsHandles(consumerM, forceDraw);



			PairRepeater2D gener = (parametricObject.generator as PairRepeater2D);


			bool zAxis = parametricObject.boolValue("zAxis");
			float separation = gener.P_Separation.FloatVal;


			// RADIUS HANDLE



			Matrix4x4 prevHandlesMatrix = Handles.matrix;

			Handles.matrix = parametricObject.model.transform.localToWorldMatrix * generator.parametricObject.worldDisplayMatrix;


			Handles.matrix *= Matrix4x4.TRS(new Vector3(0, -parametricObject.getBoundsAdjustedForAxis().size.y/2, 0), Quaternion.identity, Vector3.one);
			if (zAxis)
				Handles.matrix *= Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0,90,0), Vector3.one);



			float left 	= -separation/2;
			float right = separation/2;

			float hgt = .75f*HandleUtility.GetHandleSize(Vector3.zero);
			float lhgt = .8f*hgt;



			float len;

			Handles.color = new Color(1, 1f, .5f, .5f);

			Handles.DrawLine(new Vector3(left, 0, 0), new Vector3(left, -hgt, 0));
			Handles.DrawLine(new Vector3(right, 0, 0), new Vector3(right, -hgt, 0));


			Handles.DrawLine(new Vector3(left, -lhgt, 0), new Vector3(right, -lhgt, 0));

			// hatches
			len = .9f * .15f*HandleUtility.GetHandleSize(new Vector3(right, -lhgt, 0));
			Handles.DrawLine(new Vector3(right+len, -lhgt-len, 0), new Vector3(right-len, -lhgt+len, 0));

			len = .9f * .15f*HandleUtility.GetHandleSize(new Vector3(left, -lhgt, 0));
			Handles.DrawLine(new Vector3(left+len, -lhgt-len, 0), new Vector3(left-len, -lhgt+len, 0));

			GUIStyle labelStyle = new GUIStyle();
			labelStyle.alignment = TextAnchor.MiddleCenter;
			labelStyle.normal.textColor = Color.white;

			Handles.Label(new Vector3(0, -lhgt*1.1f, 0), ""+System.Math.Round(separation, 2), labelStyle);


			Handles.color = new Color(1, .5f, .5f, .9f);

			Vector3 pos;

			//right
			pos  = new Vector3(separation/2, 0, 0);
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
				Undo.RegisterCompleteObjectUndo (parametricObject.model, "Separation");
				gener.P_Separation.initiatePARAMETER_Ripple_setFloatValueFromGUIChange( Mathf.Max(2*pos.x, .1f) );
				parametricObject.model.isAltered(13);
			}

			pos  = new Vector3(separation/2, -lhgt, 0);
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
				Undo.RegisterCompleteObjectUndo (parametricObject.model, "Separation");
				gener.P_Separation.initiatePARAMETER_Ripple_setFloatValueFromGUIChange( Mathf.Max(2*pos.x, .1f) );
				parametricObject.model.isAltered(14);
			}


			pos = new Vector3(-separation/2, 0, 0);
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
				Undo.RegisterCompleteObjectUndo (parametricObject.model, "Separation");
				gener.P_Separation.initiatePARAMETER_Ripple_setFloatValueFromGUIChange( Mathf.Max(-2*pos.x, -.1f) );
				parametricObject.model.isAltered(15);
			}

			pos = new Vector3(-separation/2, -lhgt, 0);
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
				Undo.RegisterCompleteObjectUndo (parametricObject.model, "Separation");
				gener.P_Separation.initiatePARAMETER_Ripple_setFloatValueFromGUIChange( Mathf.Max(-2*pos.x, -.1f) );
				parametricObject.model.isAltered(16);
			}




			drawControlHandlesofInputParametricObjects(ref visited, generator.parametricObject.worldDisplayMatrix, true);



			Handles.matrix = prevHandlesMatrix;
		}	



		// SHAPE_REPEATER_2D :: DRAW_CONTROL_HANDLES_OF_INPUT_PARAMETRIC_OBJECTS
		public override void drawControlHandlesofInputParametricObjects(ref List<string> visited, Matrix4x4 consumerM)
		{
			Debug.Log("BOOO");
			PairRepeater2D gener2D = (generator as PairRepeater2D);
			// Draw the plan AND the section splines.


			// NODE 
			if (  gener2D.nodeSrc_po != null && gener2D.nodeSrc_po.generator != null)
			{
				GeneratorHandler gh = getGeneratorHandler(gener2D.nodeSrc_po);

				if (gh != null)
				{
					Matrix4x4 localPlanM = consumerM * generator.getLocalConsumerMatrixPerInputSocket(gener2D.nodeSrc_po);

					gh.drawTransformHandles(visited, localPlanM, true);
					gh.drawControlHandles(ref visited, 	 localPlanM, true);
				}
			}

			// SECTION
			if (  gener2D.cellSrc_po != null && gener2D.cellSrc_po.generator != null)
			{
				GeneratorHandler gh = getGeneratorHandler(gener2D.cellSrc_po);

				if (gh != null)
				{
					Matrix4x4 localSecM = consumerM * generator.getLocalConsumerMatrixPerInputSocket(gener2D.cellSrc_po);

					gh.drawTransformHandles(visited, localSecM, true);
					gh.drawControlHandles(ref visited, localSecM, true);

				}
			}




		}


	}














	 
	
}
