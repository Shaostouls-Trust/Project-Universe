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
	

	public class PlanRepeater2DHandler : GeneratorHandler2D 
	{


		public override void drawControlHandles(ref List<string> visited, Matrix4x4 consumerM, bool beingDrawnFromConsumer)
		{
			if (alreadyVisited(ref visited, "PlanRepeater2DHandler"))
				return;


			Matrix4x4 prevHandlesMatrix = Handles.matrix;

			base.drawControlHandles(ref visited, consumerM, beingDrawnFromConsumer);


		




			// MATRIX

			drawControlHandlesofInputParametricObjects(ref visited, consumerM * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(-90, 0, 0), Vector3.one), true);

			Handles.matrix = prevHandlesMatrix;

		}




		// SHAPE_REPEATER_2D :: DRAW_CONTROL_HANDLES_OF_INPUT_PARAMETRIC_OBJECTS
		public override void drawControlHandlesofInputParametricObjects(ref List<string> visited, Matrix4x4 consumerM)
		{
			
			PlanRepeater2D gener2D = (generator as PlanRepeater2D);
			// Draw the plan AND the section splines.
			 


			// Corner 
			if (  gener2D.cornerSrc_p != null && gener2D.cornerSrc_po.generator != null)
			{

				GeneratorHandler gh = getGeneratorHandler(gener2D.cornerSrc_po);
				
				if (gh != null)
				{
					
					Matrix4x4 localPlanM = consumerM * generator.getLocalConsumerMatrixPerInputSocket(gener2D.cornerSrc_po);
									
					gh.drawTransformHandles(visited, localPlanM, true);
					gh.drawControlHandles(ref visited, 	 localPlanM, true);
				}
			}




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
	} // \RadialRepeater2DHandler


}
