using UnityEngine;
using UnityEditor;

using System;
using System.Collections;
using System.Collections.Generic;

using AX;
using AX.Generators;

namespace AX.GeneratorHandlers
{
	
	
	public class OrganizerHandler: GeneratorHandler3D 
	{
	}

	public class GrouperHandler: GeneratorHandler3D 
	{
		
		
		//public override void drawControlHandlesofInputParametricObjects(ref List<string> visited, Matrix4x4 consumerM)
		public override void drawControlHandlesofInputParametricObjects(ref List<string> visited, Matrix4x4 consumerM)
		{


			List<AXParameter> inputPs = parametricObject.generator.AllInput_Ps;



			foreach(AXParameter input_p in inputPs)
			{
				
				
				if (input_p != null  &&  input_p.DependsOn != null )
				{
					AXParametricObject src_po = input_p.DependsOn.parametricObject;
				
					GeneratorHandler gh = getGeneratorHandler(src_po);
					
					if (gh != null)
					{
						//Debug.Log(src_po.Name);
						//Debug.Log(src_po.generator.parametricObject.worldDisplayMatrix);

						gh.drawControlHandles(ref visited, consumerM, false);
						//gh.drawTransformHandles(visited, consumerM);
					}
				}
			}

			 
		}
		
		
	}


	public class ChannelerHandler: GeneratorHandler3D 
	{
		
		
		//public override void drawControlHandlesofInputParametricObjects(ref List<string> visited, Matrix4x4 consumerM)
		public override void drawControlHandlesofInputParametricObjects(ref List<string> visited, Matrix4x4 consumerM)
		{
			// only show the inputs for the current channel

			Channeler gener = (generator as Channeler);


			if (gener.inputs != null && gener.inputs.Count > gener.channel)
			{

				AXParameter src_p = gener.inputs[gener.channel].DependsOn;


				if (src_p != null)
				{
					AXParametricObject src_po = src_p.parametricObject;

					GeneratorHandler gh = getGeneratorHandler(src_po);
					if (gh != null)
					{
						gh.drawControlHandles(ref visited, consumerM, false);
					}

				
				}

			}

			 
		}
		
		
	}




//
//	public class CSGCombinerHandler: GeneratorHandler3D 
//	{
//		
//		
//		//public override void drawControlHandlesofInputParametricObjects(ref List<string> visited, Matrix4x4 consumerM)
//		public override void drawControlHandlesofInputParametricObjects(ref List<string> visited, Matrix4x4 consumerM)
//		{
//			// only show the inputs for the current channel
//
//			CSGCombiner gener = (generator as CSGCombiner);
//
//
//			if (gener.inputs != null && gener.inputs.Count > gener.channel)
//			{
//
//				AXParameter src_p = gener.inputs[gener.channel].DependsOn;
//
//
//				if (src_p != null)
//				{
//					AXParametricObject src_po = src_p.parametricObject;
//
//					GeneratorHandler gh = getGeneratorHandler(src_po);
//					if (gh != null)
//					{
//						gh.drawControlHandles(ref visited, consumerM, false);
//					}
//
//				
//				}
//
//			}
//
//			 
//		}
//		
//		
//	}

}
		