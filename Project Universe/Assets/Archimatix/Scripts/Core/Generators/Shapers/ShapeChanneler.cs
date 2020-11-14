using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;



using AXGeometry;


namespace AX.Generators
{



	/*	GROUPER
	 * 
	 * The Grouper has a list of ParametricObjects under its management.
	 * 
	 * A List of ParamtericObjects
	 */
	public class ShapeChanneler : Generator2D
	{

		//public override string GeneratorHandlerTypeName { get { return "Shape"; } }

	

		public List<AXParameter> inputs;

		public AXParameter P_Channel;

		public int channel = 1;


		// INIT_PARAMETRIC_OBJECT
		public override void init_parametricObject() 
		{
			base.init_parametricObject();

			parametricObject.useSplineInputs = true;
			parametricObject.splineInputs = new List<AXParameter>();

			// PLAN SHAPE
			AXParameter p;

			p = parametricObject.addParameter(new AXParameter(AXParameter.DataType.CustomOption, "Channel"));
			p.optionLabels = new List<string>();
			p.optionLabels.Add("Item 1");
			p.optionLabels.Add("Item 2");
			p.optionLabels.Add("Item 3");
		

			P_Output = parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, AXParameter.ParameterType.Output, "Output Shape"));
			P_Output.hasInputSocket = false;
			P_Output.shapeState = ShapeState.Open;
		}



		public override void pollInputParmetersAndSetUpLocalReferences()
		{
			base.pollInputParmetersAndSetUpLocalReferences();

			P_Channel =  parametricObject.getParameter("Channel");

            if (P_Channel == null)
            {
                foreach (AXParameter p in parametricObject.parameters)
                {
                    if (p.Type == AXParameter.DataType.CustomOption)
                    {
                        P_Channel = p;
                        break;
                    }
                }
            }
           

		}

		// POLL CONTROLS (every model.generate())
		public override void pollControlValuesFromParmeters()
		{

			base.pollControlValuesFromParmeters();

            if (P_Channel == null)
            {
                Debug.LogWarning("The name of the Channel Parameter should not be changed.");
                return;
            }

			channel = (P_Channel  != null) 	? P_Channel.intval 	: 0;


			inputs = parametricObject.getAllInputSplineParameters();
			P_Channel.optionLabels = new List<string>();


			for (int i = 0; i < inputs.Count; i++) {
				AXParameter p = inputs [i];
				if (p.DependsOn != null)
					P_Channel.optionLabels.Add(p.DependsOn.parametricObject.Name);
			}

			//P_Channel.optionLabels.Add("All");


		}

		// GROUPER::GENERATE
		public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool isReplica)
		{
			//if (ArchimatixUtils.doDebug)
			//Debug.Log (parametricObject.Name + " generate +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");


			if (! parametricObject.isActive)
				return null;

			if (parametricObject.code != null) 				
				parametricObject.executeCodeBloc(new List<string>( parametricObject.code.Split("\n"[0])), null);


			preGenerate();


		




			// Reinstate the original functionality of the Grouper as simple combiner in addition to Groupees.
			if (inputs != null && inputs.Count > 0)
			{
				int tmpChannel = channel;
				if (channel > inputs.Count-1 && channel > 0)
					tmpChannel =  inputs.Count-1;

				// ALL
				// Combine all paths
//				if (channel == inputs.Count)
//				{
////					for(int i=0; i<inputs.Count; i++)
////					{
////						if (inputs[i] != null && inputs[i].DependsOn != null)
////						{
////							if (inputs[i].Dependents != null || inputs[i].Dependents.Count == 0)
////							{
////								AXParameter 			src_p  = inputs[i].DependsOn;
////								AXParametricObject 		src_po = inputs[i].DependsOn.parametricObject;
////								//if (! parametricObject.visited_pos.Contains (groupee))
////
////						}
////					}
//				}





				// JUST ONE CHANNEL
				else if (inputs[tmpChannel] != null)
				{


					AXParameter src_p = inputs[tmpChannel].DependsOn;

					if (src_p != null)
					{
						AXParametricObject src_po = src_p.parametricObject;

						if (src_po.is2D())
						{
							if (src_po.Output != null && src_po.Output.polyTree != null)
							{
								P_Output.polyTree = src_po.Output.polyTree;
							}
							else
							{
								P_Output.polyTree = null;
								P_Output.paths = src_po.Output.paths;
							}

							P_Output.parametricObject.bounds = src_po.bounds;


						}

						P_Output.meshes = src_p.meshes;

					}
				}



			}



			return null;

		}

	}
}