#pragma warning disable 0618 // SphereHandleCap obsolete - use SphereHandleCap

using UnityEngine;
using UnityEditor;

using System;
using System.Collections;
using System.Collections.Generic;

using AX;
using AX.Generators;

namespace AX.GeneratorHandlers
{


	public class ReplicatorHandler: GeneratorHandler3D 
	{


//		public override void drawBoundsHandles(Matrix4x4 consumerM, bool forceDraw=false)
//		{
//			base.drawBoundsHandles(consumerM, forceDraw);
//
//			Replicator replicator = (parametricObject.generator as Replicator);
//
//			if (replicator != null)
//			{
//				GeneratorHandler gh = getGeneratorHandler(replicator.parametricObject);
//
//				if (gh != null)
//				{
//					consumerM *= Matrix4x4.TRS(new Vector3(parametricObject.bounds.extents.x, 0, 0), Quaternion.Euler(0, 0, 90), Vector3.one);
		//					gh.drawBoundsHandles(consumerM *  parametricObject.getAxisRotationMatrix() * generator.getLocalAlignMatrix(), true);
//					List<string> visited = new List<string>();
		//					gh.drawControlHandles(ref visited, consumerM *  parametricObject.getAxisRotationMatrix() * generator.getLocalAlignMatrix(), true);
//				}
//			}
//		}



		public override void drawControlHandlesofInputParametricObjects(ref List<string> visited, Matrix4x4 consumerM)
		{

			Replicator gener = (generator as Replicator);
			// Draw the plan AND the section splines.

			// PLAN first, since the section needsplan information to position itself.
			if (  gener.planSrc_po != null && gener.planSrc_po.generator != null)
			{

				
				GeneratorHandler gh = getGeneratorHandler(gener.planSrc_po);

				if (gh != null)
				{
					//Matrix4x4 localPlanM = parametricObject.model.transform.localToWorldMatrix * generator.parametricObject.worldDisplayMatrix * generator.getLocalConsumerMatrixPerInputSocket(gener.planSrc_po);
					Matrix4x4 localPlanM = Matrix4x4.identity;
					localPlanM = Matrix4x4.TRS(new Vector3(parametricObject.bounds.extents.x, 0, 0), Quaternion.Euler(0, 0, 90), Vector3.one);
					if (gener.planSrc_po.is2D())				
						gh.drawTransformHandles(visited, localPlanM, true);
					gh.drawControlHandles(ref visited, 	 localPlanM, true);
				}
			}
		}

	}
}
