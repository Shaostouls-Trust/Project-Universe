#pragma warning disable 0618 // SphereHandleCap obsolete - use SphereHandleCap

using UnityEngine;
using UnityEditor;

using System;
using System.Collections;
using System.Collections.Generic;

using AXGeometry;

using AX;
using AXEditor;
using AX.Generators;

namespace AX.GeneratorHandlers
{

	public class LofterHandler : GeneratorHandler3D 
	{


		public override void drawControlHandles(ref List<string> visited, Matrix4x4 consumerM, bool beingDrawnFromConsumer)
		{	

			if (alreadyVisited(ref visited, "LofterHandler"))
				return;

			Matrix4x4 prevHandlesMatrix = Handles.matrix;

			base.drawControlHandles(ref visited, consumerM, beingDrawnFromConsumer);


			drawControlHandlesofInputParametricObjects(ref visited, consumerM * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(-90, 0, 0), Vector3.one), true);

			Handles.matrix = prevHandlesMatrix;


		}
		
		// L :: DRAW_CONTROL_HANDLES_OF_INPUT_PARAMETRIC_OBJECTS
		public override void drawControlHandlesofInputParametricObjects(ref List<string> visited, Matrix4x4 consumerM)
		{
			
			Lofter gener = (generator as Lofter);


			//if (alreadyVisited(ref visited, "LofterHandler"))
			//	return;

			Matrix4x4 prevHandlesMatrix = Handles.matrix;


			foreach (AXParameter input in gener.inputs)
			{
				
				if (input.DependsOn != null)
				{
					GeneratorHandler gh = getGeneratorHandler(input.DependsOn.parametricObject);

					//Debug.Log(input.DependsOn.parametricObject.Name);

					Matrix4x4 context = parametricObject.model.transform.localToWorldMatrix * generator.parametricObject.worldDisplayMatrix * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, 0), Vector3.one);

					if (gh != null)
					{
						Handles.matrix = context *  Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0), Vector3.one);

						gh.drawBoundsHandles(context , true);

						//List<string> _visited = new List<string>();
						gh.drawControlHandles(ref visited, consumerM , true);
					}

				}

			}




		}
	}
}