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

using Curve		= System.Collections.Generic.List<AXGeometry.CurveControlPoint2D>;



namespace AX.Generators
{



	public class ShapeOffsetter : AX.Generators.Generator2D, IShape
	{
		
		public override string GeneratorHandlerTypeName { get { return "ShapeOffsetterHandler"; } }
		//public override string GeneratorHandlerTypeName { get { return "ShapeMergerHandler"; } }

		AXParameter P_Thickness;
		AXParameter P_Roundness;
		AXParameter P_Offset;
		AXParameter P_Smoothness;

		// POLLED MEMBERS

		public float thickness;
		public float roundness;
		public float offset;
		public float smoothness;



		AXClipperLib.JoinType joinType;

		public override void init_parametricObject() 
		{
			base.init_parametricObject();

			// parameters

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, AXParameter.ParameterType.Input, "Input Shape"));


			parametricObject.addParameter(AXParameter.DataType.Float, AXParameter.ParameterType.GeometryControl, 		"Thickness", 				0f, 0, 1000);
			parametricObject.addParameter(AXParameter.DataType.Float, AXParameter.ParameterType.GeometryControl, 		"Offset", 					0f, -1000, 1000);

			//parametricObject.addParameter(AXParameter.DataType.Float, AXParameter.ParameterType.GeometryControl, 		"Roundness", 				0f, 0, 1000);
			//parametricObject.addParameter(AXParameter.DataType.Float, AXParameter.ParameterType.GeometryControl, 		"Smoothness", 				0f, -1000, 1000);

			parametricObject.addParameter(AXParameter.DataType.Int, AXParameter.ParameterType.Hidden, 					"Join_Type", 		0f, 0, 10);
							
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, AXParameter.ParameterType.Output, "Output Shape"));


		}


		// POLL INPUTS (only on graph change())
		public override void pollInputParmetersAndSetUpLocalReferences()
		{
			base.pollInputParmetersAndSetUpLocalReferences();

			P_Thickness 		= parametricObject.getParameter("Thickness");
			P_Offset 			= parametricObject.getParameter("Offset");

			//P_Roundness 		= parametricObject.getParameter("Roundness");
			//P_Smoothness 		= parametricObject.getParameter("Smoothness");

		}

		// POLL CONTROLS (every model.generate())
		public override void pollControlValuesFromParmeters()
		{
			if (parametersHaveBeenPolled)
				return;

			base.pollControlValuesFromParmeters();

			thickness 	= (P_Thickness  != null) 	? P_Thickness.FloatVal  : 0;
			offset 		= (P_Offset 	!= null) 	? P_Offset.FloatVal 	: 0;

			//roundness 	= (P_Roundness  != null) 	? P_Roundness.FloatVal  : 0;
			//smoothness 	= (P_Smoothness != null) 	? P_Thickness.FloatVal  : 0;


		}



		// SHAPE_OFFSETTER :: GENERATE
		public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool isReplica)
		{		

			if (P_Input == null ||  inputSrc_p == null )
				return null;

			// PRE_GENERATE
			preGenerate();


			P_Input.polyTree = null;
			AXShape.thickenAndOffset(ref P_Input, inputSrc_p);

			//if (P_Input.polyTree != null)
				//Debug.Log(" P_Input.polyTree 1="+ Clipper.PolyTreeToPaths(P_Input.polyTree).Count);

			//base.generate(ref visited, initiator_po, isReplica);
			
				 		
			if (P_Output == null )
				return null;


			if (thickness > 0)
			{
				P_Output.thickness = thickness;
				P_Output.endType = AXClipperLib.EndType.etClosedLine;
			}
			else 
				P_Output.endType = AXClipperLib.EndType.etClosedPolygon;
 

			if (P_Output.shapeState == ShapeState.Closed)
			{	// CLOSED
				P_Output.offset = offset;
				//P_Output.roundness = roundness;


			} 
			else 
			{	// OPEN 
				switch(P_Output.openEndType)
				{
				case AXParameter.OpenEndType.Butt:
					P_Output.endType = AXClipperLib.EndType.etOpenButt;
					break;
				case AXParameter.OpenEndType.Square:
					P_Output.endType = AXClipperLib.EndType.etOpenSquare;
					break;
				case AXParameter.OpenEndType.Round:
					P_Output.endType = AXClipperLib.EndType.etOpenRound;
					break;
				default:
					P_Output.endType = AXClipperLib.EndType.etOpenSquare;
					break;
					
				}  
			}
			     
			   
			P_Output.polyTree = null;

			AXShape.thickenAndOffset(ref P_Output, P_Input);

			//if (P_Output.polyTree != null)
			//Debug.Log(" P_Output.polyTree 1="+ Clipper.PolyTreeToPaths(P_Output.polyTree).Count);

			if ( P_Output.polyTree != null)
			{
				
				transformPolyTree(P_Output.polyTree, localMatrix);
			}
			else if (P_Output.paths != null)
			{
				
				P_Output.paths = transformPaths(P_Output.paths, localMatrix);
				P_Output.transformedControlPaths = P_Output.paths;
			}






			//Debug.Log("stats");
			AX.Generators.Generator2D.deriveStatsETC(P_Output);

			//P_Output.transformedControlPaths 							= transformPaths(P_Output.controlPaths, localMatrix); //output.getTransformedControlPaths();
			//P_Output.paths												= transformPaths(P_Output.controlPaths, localMatrix); //   output.getTransformedControlPaths(); // may be altered before generate is over...
			 
			//Debug.Log(" P_Output.path="+ P_Output.paths);
			//Debug.Log(" P_Output.transformedControlPaths="+ P_Output.transformedControlPaths);
			//if (P_Output.polyTree != null)
			//Debug.Log(" P_Output.polyTree 2="+ Clipper.PolyTreeToPaths(P_Output.polyTree).Count);


			calculateBounds();

			return null;
	
		}

	} // \ShapeOffsetter 

}