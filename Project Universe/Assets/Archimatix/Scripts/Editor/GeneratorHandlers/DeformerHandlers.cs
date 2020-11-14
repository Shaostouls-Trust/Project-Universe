using UnityEngine;
using UnityEditor;

using System;
using System.Collections;
using System.Collections.Generic;

using AX;
using AX.Generators;

namespace AX.GeneratorHandlers
{
	
	
	public class PerlinDeformer : GeneratorHandler3D {

	}









	public class PlanDeformerHandler: GeneratorHandler3D 
	{


		public override void drawControlHandlesofInputParametricObjects(ref List<string> visited, Matrix4x4 consumerM)
		{

			PlanDeformer gener = (generator as PlanDeformer);
			// Draw the plan AND the section splines.

			// PLAN first, since the section needsplan information to position itself.
			if (  gener.planSrc_po != null && gener.planSrc_po.generator != null)
			{


				GeneratorHandler gh = getGeneratorHandler(gener.planSrc_po);

				if (gh != null)
				{
					//Matrix4x4 localPlanM = parametricObject.model.transform.localToWorldMatrix * generator.parametricObject.worldDisplayMatrix * generator.getLocalConsumerMatrixPerInputSocket(gener.planSrc_po);
					Matrix4x4 localPlanM = Matrix4x4.identity;
					if (gener.planSrc_po.is2D())				
						gh.drawTransformHandles(visited, localPlanM, true);
					gh.drawControlHandles(ref visited, 	 localPlanM, true);
				}
			}





		}

	}

}





