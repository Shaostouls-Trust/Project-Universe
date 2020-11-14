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


	public interface IShape
	{

	}





	public class FrozenShape : AX.Generators.Generator2D, IShape
	{
		public override void init_parametricObject()
		{
			base.init_parametricObject();

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, AXParameter.ParameterType.Output, "Output Shape"));
			//parametricObject.addParameter(new AXParameter(AXParameter.DataType.Float, AXParameter.ParameterType.Output, "Area"));

			// handles





		}


        public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool isReplica)
        {

            //P_Input.polyTree = null;

            //AXShape.thickenAndOffset(ref P_Input, P_Input.DependsOn);

            //if (P_Output == null)
            //    return null;

            //P_Output.polyTree = null;
            //AXShape.thickenAndOffset(ref P_Output, P_Input);

            //if (P_Output != null && P_Output.paths != null)
            //    Debug.Log("Frozen Paths: " + P_Output.paths.Count);


           
            deriveStatsETC(P_Output);
            //base.generate(false, initiator_po, isReplica);

            calculateBounds();

            return null;
        }


    }

    public class Shape : AX.Generators.Generator2D, IShape
	{
		public override void init_parametricObject()
		{
			base.init_parametricObject();

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, AXParameter.ParameterType.Output, "Output Spline"));
			//parametricObject.addParameter(new AXParameter(AXParameter.DataType.Float, AXParameter.ParameterType.Output, "Area"));

			// handles

		}


		// SHAPE :: GENERATE
		public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool isReplica)
		{
			//if (Archimatix.doDebug)
			//Debug.Log ("===> [" + parametricObject.Name + "] generate " + isReplica);

			if (!parametricObject.isActive)
			{

				P_Output.paths = null;
				P_Output.polyTree = null;
				return null;
			}

			AXTurtle t = new AXTurtle();

			if (parametricObject.code != null)
			{
				//Debug.Log (parametricObject.code);
				parametricObject.executeCodeBloc(new List<string>(parametricObject.code.Split("\n"[0])), t);
				//Archimatix.axScriptParser.executeCodeBloc(parametricObject, new List<string>(parametricObject.code.Split("\n"[0])), t);
			}



			//			if (t.paths.Count == 1)
			//			{
			//				if (! Clipper.Orientation(t.paths[0]))
			//					t.paths[0].Reverse();
			//
			//			}
			t.createBasePolyTreeFromDescription();

			AXParameter isClosedP = parametricObject.getParameter("isClosed");
			if (isClosedP == null)
				isClosedP = parametricObject.getParameter("isclosed");

			if (isClosedP != null)
				t.s.isClosed = isClosedP.boolval;

			t.s.breakAngle = parametricObject.floatValue("breakAngle");




			parametricObject.transMatrix = localMatrix; //Matrix4x4.TRS(new Vector3(transX, transY, 0), Quaternion.Euler(0, 0, rotZ), new Vector3(1,1,1));

			// could make more efficient
			// by resetting output on deserialize using this?
			AXParameter output = P_Output;

			if (P_Output != null)
			{
				P_Output.spline = t.s;
				P_Output.polyTree = null;

				P_Output.controlPaths = t.paths; // not to be altered in base postprocessing for offset and wallthick






				if (scaleX != 1 || scaleY != 1)
					P_Output.transformedButUnscaledOutputPaths = transformPaths(output.controlPaths, localUnscaledMatrix);
				else
					P_Output.transformedButUnscaledOutputPaths = null;

				if (P_Output.offset != 0)
					P_Output.transformedAndScaledButNotOffsetdOutputPaths = transformPaths(output.controlPaths, localMatrix);
				else
					P_Output.transformedAndScaledButNotOffsetdOutputPaths = null;

				P_Output.transformedControlPaths = transformPaths(output.controlPaths, localMatrix); //output.getTransformedControlPaths();
				P_Output.paths = transformPaths(output.controlPaths, localMatrix); //   output.getTransformedControlPaths(); // may be altered before generate is over...


			}
			//Debug.Log (t.s.toString());

			//Pather.printPaths(P_Output.paths);

			base.generate(false, initiator_po, isReplica);

			//			calculateBounds();
			//			adjustWorldMatrices();



			return null;
		}


	}






	public class ShapeMerger : AX.Generators.Generator2D, IShape
	{

		public override string GeneratorHandlerTypeName { get { return "ShapeMergerHandler"; } }


		public AXShape S_InputShape;

		public override float minNodePaletteWidth
		{
			get { return 200; }
		}


		// INIT
		public override void init_parametricObject()
		{
			base.init_parametricObject();

			// SHAPE
			parametricObject.addShape("Input Shape");
		}


		// POLL INPUTS (only on graph change())
		public override void pollInputParmetersAndSetUpLocalReferences()
		{
			base.pollInputParmetersAndSetUpLocalReferences();

			S_InputShape = parametricObject.getShape("Input Shape");
		}

		public override bool hasOutputsConnected()
		{
			if (S_InputShape != null && S_InputShape.hasOutputConnected())
				return true;

			return false;
		}

		public override bool hasOutputsReady()
		{
			if (S_InputShape != null && S_InputShape.hasInputsConnected() && S_InputShape.hasOutputReady())
				return true;

			return false;
		}

		public void connect(AXParametricObject po)
		{
			AXShape shp = getInputShape();

			if (po.is2D())
			{
				AXParameter out_p = po.generator.getPreferredOutputParameter();
				if (out_p != null)
					shp.addInput().makeDependentOn(out_p);

				po.intValue("Axis", parametricObject.intValue("Axis"));

			}
		}
		public override void connectionBrokenWith(AXParameter p)
		{

			base.connectionBrokenWith(p);

			if (!S_InputShape.hasInputsConnected())
			{
				// clear all outputs!
				S_InputShape.clearAllOutputs();
			}


		}




		public override AXParameter getPreferredOutputParameter()
		{
			/*AXShape inputShape = parametricObject.getShape("Input Shape");
			if (inputShape == null)
				inputShape = parametricObject.getShape("Input Shapes");
			*/
			if (S_InputShape == null)
				return null;

			return S_InputShape.getSelectedOutputParameter();
		}

		// GENERATE SHAPE_MERGER
		public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool isReplica)
		{


			// PRE_GENERATE
			preGenerate();

			S_InputShape.clearAllOutputs();

			if (S_InputShape != null && !S_InputShape.hasInputsConnected())
			{
				// clear all outputs!
				S_InputShape.clearAllOutputs();
				return null;
			}


			if (S_InputShape != null)
			{
				S_InputShape.generate();

				base.generate(false, initiator_po, isReplica);

				calculateBounds();
			}

			return null;



		}





		public override void calculateBounds()
		{
			AXParameter SelectedOutput = S_InputShape.getSelectedOutputParameter();

			Paths paths = (SelectedOutput != null) ? SelectedOutput.getPaths() : null;

			//Debug.Log(parametricObject.Name + ": " + P_Output);

			if (paths == null)
				return;

			IntRect cb = Clipper.GetBounds(SelectedOutput.getPaths());

			Vector3 size = new Vector3(cb.right - cb.left, cb.bottom - cb.top, 0);///Archimatix.IntPointPrecision;
			Vector3 center = new Vector3(cb.left + size.x / 2, cb.top + size.y / 2, 0);


			parametricObject.bounds = new Bounds((center / AXGeometry.Utilities.IntPointPrecision - new Vector3(transX, transY, 0)), size / AXGeometry.Utilities.IntPointPrecision);

		}


	} // END ShapeMerger









	public class ShapeDistributor : AX.Generators.Generator2D, IShape
	{
		//public override string GeneratorHandlerTypeName { get { return "ShapeDistributorHandler"; } }

		// INITIALIZE
		public override void init_parametricObject()
		{
			base.init_parametricObject();

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, AXParameter.ParameterType.Input, "Input Shape"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, AXParameter.ParameterType.Output, "Output Shape"));
		}


		public override void connectionMadeWith(AXParameter to_p, AXParameter from_p)
		{
			base.connectionMadeWith(to_p, from_p);


			AXParameter this_p = (to_p.parametricObject == parametricObject) ? to_p : from_p;
			AXParameter src_p = (to_p.parametricObject == parametricObject) ? from_p : to_p;

			if (this_p.PType == AXParameter.ParameterType.Input)
				P_Output.shapeState = src_p.shapeState;
			else if (this_p.PType == AXParameter.ParameterType.Output)
			{
				src_p.shapeState = this_p.shapeState;
			}



			clearOutput();

			parametricObject.model.setAltered(parametricObject);
		}

		public override void connectionBrokenWith(AXParameter p)
		{

			base.connectionBrokenWith(p);

			//planSrc_po = null;
			clearOutput();

			parametricObject.model.setAltered(parametricObject);


		}
		public void clearOutput()
		{
			P_Output.paths = null;
			P_Output.polyTree = null;


		}


		// SHAPE_DISTRIBUTER :: GENERATE
		public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool isReplica)
		{
			if (P_Input == null || P_Input.DependsOn == null)
				return null;

			P_Input.polyTree = null;

			AXShape.thickenAndOffset(ref P_Input, P_Input.DependsOn);

			if (P_Output == null)
				return null;

			P_Output.polyTree = null;
			AXShape.thickenAndOffset(ref P_Output, P_Input);

			deriveStatsETC(P_Output);
			//base.generate(false, initiator_po, isReplica);

			calculateBounds();

			return null;
		}
	}












	public class ShapeSegmentor : AX.Generators.Generator2D, IShape
	{
		public override void init_parametricObject()
		{
			base.init_parametricObject();

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, AXParameter.ParameterType.Input, "Input Shape"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, AXParameter.ParameterType.Output, "Output Shape"));


			parametricObject.addParameter(AXParameter.DataType.Float, "height", 3f, 0f, 100f);
			parametricObject.addParameter(AXParameter.DataType.Int, "segments", 7f, 0f, 100f);

		}


		// SHAPE :: GENERATE
		public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool isReplica)
		{
			if (inputSrc_p == null)
				return null;

			P_Input.polyTree = null;

			AXShape.thickenAndOffset(ref P_Input, inputSrc_p);


			P_Output.polyTree = null;
			AXShape.thickenAndOffset(ref P_Output, P_Input);


			// now take the output and segment it....

			Paths subjPaths = null;

			if (P_Output.polyTree != null)
			{
				subjPaths = Clipper.PolyTreeToPaths(P_Output.polyTree);
				P_Output.polyTree = null;
			}
			else
			{
				subjPaths = P_Output.getPaths();
			}


			Path src_path = subjPaths[0];
			Paths segmentedPaths = new Paths();


			float y0 = 0;
			float height = parametricObject.floatValue("height");

			int segs = parametricObject.intValue("segments");

			float segHgt = height / segs;


			float zone_y;



			// 
			int zoneCursor = 0;

			// begin the first zone path (or segment)
			Path zonePath = new Path();
			zonePath.Add(src_path[0]);

			// loop through segments in path
			for (int v = 0; v < (src_path.Count - 1); v++)
			{
				Debug.Log("v=" + v);
				IntPoint thisVert = src_path[v];
				IntPoint nextVert = src_path[v + 1];

				float slope = 0;

				if (nextVert.Y - thisVert.Y != 0)
					slope = (0.0f + (nextVert.X - thisVert.X)) / (0.0f + (nextVert.Y - thisVert.Y));

				// do these y's cross over a segment line?
				for (int zone = zoneCursor; zone < segs; zone++)
				{

					zone_y = (y0 + zone * segHgt) * AXGeometry.Utilities.IntPointPrecision;

					Debug.Log("zone=" + zone + ", zone_y=" + zone_y + ",y0= " + y0 + ", segHgt=" + segHgt + ", segs=" + segs);

					if (thisVert.Y < zone_y && nextVert.Y > zone_y)
					{
						// we have a crossover
						float newX = (zone_y - thisVert.Y) * slope + thisVert.X;

						// make a new point
						IntPoint segPt = new IntPoint(newX, zone_y);


						zonePath.Add(segPt);
						segmentedPaths.Add(zonePath);

						zonePath = new Path();
						zonePath.Add(segPt);

						//no need to look at zone less than this again

					}
				}

				// Now add the upper vert...
				zonePath.Add(nextVert);



			}
			segmentedPaths.Add(zonePath);

			Debug.Log("segmentedPaths: " + segmentedPaths.Count);

			P_Output.paths = segmentedPaths;


			return null;

		}
	}

















}