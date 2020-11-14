#pragma warning disable

using UnityEngine;

using AXGeometry;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;


using AXClipperLib;
using Path = System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;

using Curve = System.Collections.Generic.List<AXGeometry.CurveControlPoint2D>;



namespace AX.Generators
{



	public class Instance2D : AX.Generators.Generator2D, IShape
	{

		public override string GeneratorHandlerTypeName { get { return "GeneratorHandler2D"; } }
		//public override string GeneratorHandlerTypeName { get { return "ShapeMergerHandler"; } }


		// POLLED MEMBERS

		public float thickness;
		public float offset;



		//AXClipperLib.JoinType joinType;

		public override void init_parametricObject()
		{
			base.init_parametricObject();

			// parameters

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, AXParameter.ParameterType.Input, "Input Shape"));


			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, AXParameter.ParameterType.Output, "Output Shape"));


		}




		// POLL CONTROLS (every model.generate())
		public override void pollControlValuesFromParmeters()
		{
			if (parametersHaveBeenPolled)
				return;

			base.pollControlValuesFromParmeters();

			thickness = parametricObject.floatValue("Thickness");
			offset = parametricObject.floatValue("Offset");

		}



		// SHAPE_OFFSETTER :: GENERATE
		public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool isReplica)
		{

			P_Input.clearAllPaths();

			P_Output.clearAllPaths();

			if (P_Input == null || inputSrc_p == null)
				return null;



			if (inputSrc_p.pathsAreNull())
				return null;



			if ((inputSrc_p.paths == null || inputSrc_p.paths.Count == 0) && inputSrc_p.polyTree == null)
			{
				return null;
			}


			//Debug.Log(P_Input.path)
			// PRE_GENERATE
			preGenerate();





			AXShape.thickenAndOffset(ref P_Input, inputSrc_p);



			if (P_Output == null)
				return null;


			P_Output.polyTree = null;

			//Debug.Log("thickness="+thickness+", offset=" + offset);
			AXShape.thickenAndOffset(ref P_Output, P_Input);

			//if (P_Output.polyTree != null)
			//Debug.Log(" P_Output.polyTree 1="+ Clipper.PolyTreeToPaths(P_Output.polyTree).Count);



			if (P_Output.polyTree != null)
			{
				transformPolyTree(P_Output.polyTree, localMatrix);
			}
			else if (P_Output.paths != null)
			{
				P_Output.paths = transformPaths(P_Output.paths, localMatrix);

                P_Output.paths = Clipper.CleanPolygons(P_Output.paths);

                P_Output.transformedControlPaths = P_Output.paths;
			}

			calculateBounds();

			return null;

		}

	} // \ShapeOffsetter 

}