using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;



using AXGeometry;


using AXClipperLib;
using Path = System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;

using Curve		= System.Collections.Generic.List<AXGeometry.CurveControlPoint2D>;


using LibNoise.Unity;
using LibNoise.Unity.Generator;

using Perlin = LibNoise.Unity.Generator.Perlin;



namespace AX.Generators
{
	public class ShapeSlicer  : AX.Generators.Generator2D, IShape, ICustomNode
	{

		public AXParameter	P_Y1;
		public AXParameter	P_Y2;
		public AXParameter	P_Segs;





        // WORKING FIELDS (Updated every Generate)
        // As a best practice, each parameter value could have a local variable
        // That is set before the generate funtion is called.
        // This will allow Handles to acces the parameter values more efficiently.

        float y1;
        float y2;
        int segs;
		


		// INITIALIZE
		public override void init_parametricObject() 
		{
			base.init_parametricObject();

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, AXParameter.ParameterType.Input, "Input Shape"));


			// GEOMETRY_CONTROLS
			parametricObject.addParameter(AXParameter.DataType.Float, 	"Y1", 1f);
			parametricObject.addParameter(AXParameter.DataType.Float, 	"Y2", 1f);
			parametricObject.addParameter(AXParameter.DataType.Int, 	"Segs", 3);


			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, AXParameter.ParameterType.Output, "Output Shape"));


		}



		// POLL INPUTS (only on graph change())
		public override void pollInputParmetersAndSetUpLocalReferences()
		{
			//Debug.Log("Repeater::pollInputParmetersAndSetUpLocalReferences " + parametricObject.Name + " parametersHaveBeenPolled="+parametersHaveBeenPolled);

			base.pollInputParmetersAndSetUpLocalReferences();

			P_Input 				= parametricObject.getParameter("Input Shape");

			P_Y1 					= parametricObject.getParameter("Y1");
			P_Y2 					= parametricObject.getParameter("Y2");
			P_Segs 					= parametricObject.getParameter("Segs");
			


			pollControlValuesFromParmeters();
			 
		}


		// POLL CONTROLS (every model.generate())
		// It is helpful to set the values for parameter variables before generate().
		// These values will be available not only to generate() but also the Handle functions.
		public override void pollControlValuesFromParmeters()
		{
			base.pollControlValuesFromParmeters();



			y1 	    = (P_Y1  	!= null) ? P_Y1.FloatVal	: 1.0f;
			y2 	    = (P_Y2  	!= null) ? P_Y2.FloatVal	: 1.0f;
            segs    = (P_Segs   != null) ? P_Segs.IntVal    : 0;


        }



        public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool isReplica)
		{
			//if (ArchimatixUtils.doDebug)
			//Debug.Log (parametricObject.Name + " generate +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");

			//Debug.Log("P_Output="+P_Output);

            preGenerate();

            if (! parametricObject.isActive)
				return null;

			if (P_Input == null ||  inputSrc_p == null )
				return null;

			if (P_Output == null )
				return null;




           // P_Input.polyTree = null;

            //Pather.printPaths(inputSrc_p.getPaths());


            Paths paths = inputSrc_p.getPaths();

           // Debug.Log("paths.Count = " + paths.Count);
            if (paths == null || paths.Count == 0)
                return null;

            Path path = paths[0];

            if (segs > 0)
            {
                path = Pather.YDivide(paths[0], y1);

                float seglen = (y2 - y1) / segs;

                for (int i = 1; i < segs; i++)
                {
                    path = Pather.YDivide(path, y1+i*seglen);
                }

            }
            path = Pather.YDivide(path, y2);


            //P_Output.polyTree = null;
            P_Output.paths = new Paths();

            P_Output.paths.Add(path);


            //bool planIsClosed = (P_Output.hasThickness || P_Output.shapeState == ShapeState.Closed) ? true : false;

            //if (P_Output.paths != null && (P_Output.subdivision > 0 || P_Input.subdivision > 0))
            //{
            //    int seglen = Pather.getSegLenBasedOnSubdivision(P_Output.paths, (int)((P_Input.subdivision > 0) ? P_Input.subdivision : P_Output.subdivision));
            //    //Debug.Log ("seglen=" + seglen);
            //    P_Output.paths = Pather.segmentPaths(P_Output.paths, seglen, planIsClosed);
            //}



            //for (int i = 0; i < P_Output.paths.Count; i++)
            //{
            //    Path path = P_Output.paths[i];

            //    for (int j = 0; j < path.Count; j++)
            //    {
            //        IntPoint ip = path[j];

            //        Vector2 vert = Pather.IP2Vector2WithPrecision(ip);

            //        float pval = (float)perlin.GetValue(vert);

            //        Vector2 ray = (vert - center);

            //        vert = vert + (ray.normalized * pval * amount * .1f);

            //        path[j] = Pather.Vector2IPWithPrecision(vert);
            //    }
            //}



            calculateBounds();


			return null;
		}


	}

}