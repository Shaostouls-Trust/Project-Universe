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
	

	public class LinearRepeater2DHandler : GeneratorHandler2D 
	{


		public override void drawControlHandles(ref List<string> visited, Matrix4x4 consumerM, bool beingDrawnFromConsumer)
		{
			if (alreadyVisited(ref visited, "LinearRepeater2DHandler"))
				return;

			Matrix4x4 prevHandlesMatrix = Handles.matrix;

			base.drawControlHandles(ref visited, consumerM, beingDrawnFromConsumer);


			RepeaterTool repeaterToolU = (parametricObject.generator as LinearRepeater2D).repeaterToolU;



			if (repeaterToolU != null )
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
					Handles.DrawLine(new Vector3(-repeaterToolU.size/2+i*repeaterToolU.actualBay, 0, 0), new Vector3(-repeaterToolU.size/2+i*repeaterToolU.actualBay, 0, 0));


				
				// ASK RepeaterTools to DRAW
				GeneratorHandler gh = getGeneratorHandler(repeaterToolU.parametricObject);

				if (gh != null)
				{
					Handles.matrix = context *  Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.Euler(90, 0, 0), Vector3.one);

					gh.drawBoundsHandles(context , true);

					//List<string> _visited = new List<string>();
					gh.drawControlHandles(ref visited, consumerM , true);
				}

			}


			// MATRIX

			drawControlHandlesofInputParametricObjects(ref visited, consumerM * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(-90, 0, 0), Vector3.one), true);

			Handles.matrix = prevHandlesMatrix;

		}




		// SHAPE_REPEATER_2D :: DRAW_CONTROL_HANDLES_OF_INPUT_PARAMETRIC_OBJECTS
		public override void drawControlHandlesofInputParametricObjects(ref List<string> visited, Matrix4x4 consumerM, bool beingDrawnFromConsumer)
		{

			LinearRepeater2D gener2D = (generator as LinearRepeater2D);
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

			// CELL
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
	} // \LinearRepeater2DHandler



}
