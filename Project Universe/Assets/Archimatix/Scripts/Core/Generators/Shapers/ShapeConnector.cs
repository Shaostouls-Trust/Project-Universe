using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using AXClipperLib;
using Path = System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;

using Curve		= System.Collections.Generic.List<AXGeometry.CurveControlPoint2D>;


using AXGeometry;


namespace AX.Generators
{



	/*	GROUPER
	 * 
	 * The Grouper has a list of ParametricObjects under its management.
	 * 
	 * A List of ParamtericObjects
	 */
	public class ShapeConnector : Generator2D
	{

		//public override string GeneratorHandlerTypeName { get { return "Generator2D"; } }

		public List<AXParameter> inputs;


		// INIT_PARAMETRIC_OBJECT
		public override void init_parametricObject() 
		{
			base.init_parametricObject();

			parametricObject.useSplineInputs = true;
			parametricObject.splineInputs = new List<AXParameter>();


			P_Output = parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, AXParameter.ParameterType.Output, "Output Shape"));
			P_Output.hasInputSocket = false;
			P_Output.shapeState = ShapeState.Open;
		}



		// POLL CONTROLS (every model.generate())
		public override void pollControlValuesFromParmeters()
		{

			base.pollControlValuesFromParmeters();



			inputs = parametricObject.getAllInputSplineParameters();


		}





		// GROUPER::GENERATE
		public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool isReplica)
		{
			//if (ArchimatixUtils.doDebug)
			//Debug.Log (parametricObject.Name + " generate +++++++++++++++++++++++++++++++++++++++++++++++++ " + inputs.Count);


			if (! parametricObject.isActive)
				return null;

			

			preGenerate();

			Path path = new Path();



			if (inputs != null && inputs.Count > 0)
			{

				for(int i=0; i<inputs.Count; i++)
				{
					AXParameter input = inputs[i];

					if (input != null && input.DependsOn != null)
					{
						if ( input.DependsOn.parametricObject.generator is FreeCurve)
						{
							FreeCurve fc = (FreeCurve) input.DependsOn.parametricObject.generator;

							Path output = AX.Generators.Generator2D.transformPath(fc.getPathFromCurve(), fc.localMatrix);

							path.AddRange(output);
						}
						else
						{
							Paths tmpPaths = input.DependsOn.getPaths();
							if (tmpPaths != null)
							{
								for (int j=0; j<tmpPaths.Count; j++)
								{
									path.AddRange(tmpPaths[j]);
								}
							}
						}

					}
				}

				path = Clipper.CleanPolygon(path);

				P_Output.paths = new Paths();

				P_Output.paths.Add(path);




			}

			return null;


		}


	}

}
