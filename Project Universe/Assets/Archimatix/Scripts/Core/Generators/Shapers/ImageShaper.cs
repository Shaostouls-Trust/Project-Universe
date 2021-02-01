#pragma warning disable

using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using AXGeometry;

using AXClipperLib;
using Path = System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;

using Curve = System.Collections.Generic.List<AXGeometry.CurveControlPoint2D>;


using LibNoise.Unity;
using LibNoise.Unity.Generator;

using Perlin = LibNoise.Unity.Generator.Perlin;



using CsPotrace;



namespace AX.Generators
{

	public class ImageShaper : AX.Generators.Generator2D, IShape
	{
        public AXParameter P_ImageInput;


        public AXParameter P_AlphaCutoff;
        public AXParameter P_DecimationAngle;

        public float alphaCutoff = .99f;
        public float decimationAngle = 7.5f;


        // INIT_PARAMETRIC_OBJECT
        public override void init_parametricObject()
		{
			base.init_parametricObject();

            P_ImageInput = parametricObject.addParameter(new AXParameter(AXParameter.DataType.Texture2D, AXParameter.ParameterType.Input, "Input Image"));

            // GEOMETRY_CONTROLS
            P_AlphaCutoff = parametricObject.addParameter(AXParameter.DataType.Float, "AlphaCutoff", .1f);
            P_DecimationAngle = parametricObject.addParameter(AXParameter.DataType.Float, "DecimationAngle", 7.5f);

            parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, AXParameter.ParameterType.Output, "Output Shape"));
		}



        // POLL INPUTS (only on graph change())
        public override void pollInputParmetersAndSetUpLocalReferences()
        {
            //Debug.Log("Repeater::pollInputParmetersAndSetUpLocalReferences " + parametricObject.Name + " parametersHaveBeenPolled="+parametersHaveBeenPolled);

            base.pollInputParmetersAndSetUpLocalReferences();

            P_ImageInput = parametricObject.getParameter("Input Image");

            P_AlphaCutoff = parametricObject.getParameter("AlphaCutoff");
            P_DecimationAngle = parametricObject.getParameter("DecimationAngle");
        }


        // POLL CONTROLS (every model.generate())
        // It is helpful to set the values for parameter variables before generate().
        // These values will be available not only to generate() but also the Handle functions.
        public override void pollControlValuesFromParmeters()
        {
            base.pollControlValuesFromParmeters();

            alphaCutoff = (P_AlphaCutoff != null) ? P_AlphaCutoff.FloatVal : .1f;
            decimationAngle = (P_DecimationAngle != null) ? P_DecimationAngle.FloatVal : 7.5f;
        }
        public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool isReplica)
        {
            //if (ArchimatixUtils.doDebug)
            //Debug.Log (parametricObject.Name + " generate +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");

            //Debug.Log("ImageShaper");

            if (P_ImageInput.DependsOn == null)
                return null;

            Texture2D imageData = P_ImageInput.DependsOn.parametricObject.imageData;

           

            float ratio = (float) imageData.height / (float) imageData.width;

            float curveWidth = .75f; // meters
            float curveHeight = curveWidth * ratio; // meters

            if (imageData == null)
                return null;
            //Debug.Log("HAVE IMAGE");

            if (imageData != null)

            {
                // TRACE IMAGE AND SAVE TO OUTPUT
                /// TRACE ------------------------------------------------
                Paths tracedPaths = Pather.TraceImageToPaths(imageData, alphaCutoff);

                if (tracedPaths != null)
                {

                
                    for (int pi = 0; pi < tracedPaths.Count; pi++)
                    {
                        tracedPaths[pi] = Clipper.CleanPolygon(tracedPaths[pi]);
                        //Debug.Log("decimationAngle=" + decimationAngle);
                        // DECIMATE -----------------------------------------------
                        tracedPaths[pi] = Pather.decimatePath(tracedPaths[pi], decimationAngle);
                    }

                    // SCALE and cut path --------------------------------------

                    IntRect ir = Pather.getBounds(tracedPaths);
                    IntPoint WH = new IntPoint(ir.right - ir.left, ir.bottom - ir.top);

                    IntPoint wh = Pather.Vector2IPWithPrecision(new Vector2(curveWidth, curveHeight));

                    long shiftx = WH.X/2;
                    float sx = (float)wh.X / (float)(WH.X);
                    float sy = (float)wh.Y / (float)WH.Y;

                    // SCALE AND SHIFT ALL PATHS
                    // Paths scaledPaths = new Paths();

                    for (int pi = 0; pi < tracedPaths.Count; pi++)
                    {
                        Path _path = tracedPaths[pi];

                        for (int k = 0; k < _path.Count; k++)
                        {
                            _path[k] = new IntPoint(Mathf.FloorToInt(sx * (float)(_path[k].X - shiftx)), Mathf.FloorToInt((sy * (float)_path[k].Y)));
                        }
                    }

                    P_Output.paths = tracedPaths;
                    //Pather.printPath(tracedPaths[0]);
                }
            }

            return null;
        }

    }
}
