
using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

using AXClipperLib;
using Path = System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;

using AXPoly2Tri;
using PolygonPoints = System.Collections.Generic.List<AXPoly2Tri.PolygonPoint>;

using AXGeometry;
 

namespace AX.Generators
{
	
	

	
	// WINWALL GENERATOR
	public class WinWall : Generator3D, IMeshGenerator, ICustomNode
	{

		public List<AXParameter> inputs;

		public override string GeneratorHandlerTypeName { get { return "WinWallHandler"; } }

		// INPUTS
		public AXParameter 			P_Plan;
		public AXParameter 			planSrc_p;
		public AXParametricObject 	planSrc_po;

		public AXParameter P_Height;


		// POLLED MEMBERS

		public float 	height 		= 3;

		public bool planIsClosed = false;

		public List<AXMesh> ax_meshes = null;



		// INIT_PARAMETRIC_OBJECT
		public override void init_parametricObject() 
		{
			base.init_parametricObject();


			// PLAN SHAPE
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, AXParameter.ParameterType.Input, "Plan"));
			

			parametricObject.useSplineInputs = true;
			parametricObject.splineInputs = new List<AXParameter>();

			P_Height = parametricObject.addParameter(AXParameter.DataType.Float, AXParameter.ParameterType.GeometryControl, "Height", 2f, .01f, 1000f);
			P_Height.sizeBindingAxis = Axis.Y;


			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Output, "Output Mesh"));

		}



		// POLL INPUTS (only on graph change())
		public override void pollInputParmetersAndSetUpLocalReferences()
		{
			base.pollInputParmetersAndSetUpLocalReferences();

			P_Plan 			= parametricObject.getParameter("Plan");


			P_Height 		= parametricObject.getParameter("Height");

		}



		// POLL CONTROLS (every model.generate())
		public override void pollControlValuesFromParmeters()
		{
			
			base.pollControlValuesFromParmeters();

			planSrc_p		= P_Plan.DependsOn; //getUpstreamSourceParameter(P_Plan);
			planSrc_po 		= (planSrc_p != null) 								? planSrc_p.parametricObject 	: null;

            height = (P_Height != null) ? P_Height.FloatVal : 3;

        }





		// GENERATE WINWALL
		public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool renderToOutputParameter)
		{
			if (parametricObject == null || ! parametricObject.isActive)
				return null;


			//Debug.Log("PlanSweep::generate()");

			// RESULTING MESHES
			ax_meshes = new List<AXMesh>();


			preGenerate();


			
			// PLAN
			// The plan may have multiple paths. Each may generate a separate GO.
			
			if (P_Plan == null)
				return null;


			planSrc_p		= getUpstreamSourceParameter(P_Plan);
			planSrc_po 		= (planSrc_p != null) 								? planSrc_p.parametricObject 	: null;

			if (planSrc_p == null || ! planSrc_p.parametricObject.isActive)
				return null;


			planIsClosed 		= (P_Plan.hasThickness || P_Plan.shapeState == ShapeState.Closed) ? true : false;

			
			P_Plan.polyTree = null;

			Paths planPaths = planSrc_p.getPaths();

            if (planPaths == null)
                return null;

			Path planPath = planPaths[0];

			Spline planSpline 		= new Spline(planPath, planIsClosed, P_Plan.breakGeom, P_Plan.breakNorm);



            float wallThick = .5f;

			Paths offsetPaths = Pather.wallOffsets(planSpline, wallThick/2, -wallThick/2);

			//Pather.printPaths(offsetPaths);


			// each path, step through and mak a rectangle 
			// segment wide and height and then subtract windows.

			//Then make poly and add to combiner


            //Debug.Log("==========");
            //Pather.printPath(window);




            Pather rightPather = new Pather(offsetPaths[0]);
            Pather leftPather = new Pather(offsetPaths[1]);


            //Pather.printPath(offsetPaths[0]);
            //Pather.printPath(offsetPaths[1]);

            int[] rightLengths = rightPather.segment_lengths;
            int[] leftLengths = leftPather.segment_lengths;

            float winWidth = 2f; 
            float winHeight = 2f;
            float margin = .75f;
            float gap = 1f;

            int numFloors = 1;
            float winActualHgt = winHeight / numFloors;



            AXMesh tmpAXMesh = null;

            int loopTo = rightLengths.Length;

            if (!planIsClosed)
                loopTo--;

            for (int i=0; i< loopTo; i++)
			{
				//Debug.Log(rightLengths[i]);

				int next_i = (i == rightLengths.Length-1) ? 0 : i+1;

                float rightWallLen = rightLengths[next_i] / 10000f;
                //float leftWallLen = leftLengths[next_i] / 10000f;


                Path rightWallRect = AXTurtle.Rectangle(rightLengths[next_i] / 10000f, height, false);
                Path leftWallRect = AXTurtle.Rectangle((leftLengths[next_i]) / 10000f, height, false);



                // WINDOWS
                Paths windows = new Paths();

               
                int numWins = Mathf.FloorToInt((rightWallLen - 2 * margin) / (winWidth + gap));

                float windowSetLength = rightWallLen - 2 * margin - numWins*gap;

                float winActualWid = windowSetLength / numWins;
               

                for (int j=0; j< numWins; j++)
                {
                    float xpos = margin + gap / 2 + j * (winActualWid + gap);

                    for (int k=0; k<numFloors; k++)
                    {
                        Path window = AXTurtle.Rectangle(winWidth, winActualHgt, false);

                        float ypos = margin + gap / 2 + k * (winActualHgt + gap);


                        Pather.shiftPath(window, new IntPoint(xpos * 10000, ypos * 10000));

                        windows.Add(window);

                    }

                }


                // RIGHT WALL (SOLID)
                Clipper c = new Clipper(Clipper.ioPreserveCollinear);
                c.AddPath(rightWallRect, PolyType.ptSubject, true);

                // fenestration
                c.AddPaths(windows, PolyType.ptClip, true);

                AXClipperLib.PolyTree rightPolytree = new AXClipperLib.PolyTree();
                c.Execute(ClipType.ctDifference, rightPolytree, PolyFillType.pftNonZero, PolyFillType.pftNonZero);

                //Paths pathResult = Clipper.PolyTreeToPaths(rightPolytree);

                //Pather.printPaths(pathResult);

                Mesh mesh = AXPolygon.triangulate(rightPolytree, new AXTexCoords());

                Matrix4x4 wallm = Matrix4x4.TRS(new Vector3(offsetPaths[0][i].X / 10000f, 0, offsetPaths[0][i].Y / 10000f), Quaternion.Euler(-90, planSpline.edgeRotations[i], 0), Vector3.one);

                tmpAXMesh = new AXMesh(mesh, wallm);
                tmpAXMesh.makerPO = parametricObject;
                ax_meshes.Add(tmpAXMesh);




                // EXTERIOR WINDOW FRAMES

                Path sectionPath = AXTurtle.Rectangle(.1f,.75f, true);
                Spline sectionSpline = new Spline(sectionPath, true, 60, 60);
                //sectionSpline.shift(-radius, 0);


                Path windowPlan = AXTurtle.Rectangle(winWidth, 2, false);
                Spline windowSpline = new Spline(windowPlan, true, 60, 60);
                windowSpline.isClosed = true;

                Mesh frameMesh = new Mesh();

                PlanSweeper frameSweeper = new PlanSweeper();
                frameSweeper.generate(ref frameMesh, windowSpline, sectionSpline, new AXTexCoords());

                for (int j = 0; j < numWins; j++)
                {

                    float xpos = margin + gap / 2 + j * (winActualWid + gap);
                    //Pather.shiftPath(window, new IntPoint(xpos * 10000, 10000));


                    Matrix4x4 m = wallm * Matrix4x4.TRS(new Vector3(xpos, -.2f, 1.25f), Quaternion.identity, Vector3.one);

                    tmpAXMesh = new AXMesh(frameMesh, m);
                    tmpAXMesh.makerPO = parametricObject;
                    ax_meshes.Add(tmpAXMesh);

                   
                    

                }


               






                // LEFT WALL (SOLID)
                c = new Clipper(Clipper.ioPreserveCollinear);
                c.AddPath(leftWallRect, PolyType.ptSubject, true);

                // fenestration
                float leftFenestrationShifter = -Mathf.CeilToInt(wallThick * 10000f);
                //Debug.Log(planSpline.bevelAngles[i] +" roter: " + (planSpline.edgeRotations[i]));
                if (planSpline.bevelAngles[i] < 0)
                    leftFenestrationShifter = -leftFenestrationShifter;

                Pather.shiftPaths(    windows,    new IntPoint(leftFenestrationShifter, 0)      );
                c.AddPaths(windows, PolyType.ptClip, true);

                //Debug.Log(Mathf.CeilToInt(wallThick / 10000f));

                AXClipperLib.PolyTree leftPolytree = new AXClipperLib.PolyTree();
                c.Execute(ClipType.ctDifference, leftPolytree, PolyFillType.pftNonZero, PolyFillType.pftNonZero);

                //pathResult = Clipper.PolyTreeToPaths(leftPolytree);

                //Pather.printPaths(pathResult);

                Mesh left_mesh = AXPolygon.triangulate(leftPolytree, new AXTexCoords());

                Matrix4x4 left_wallm = Matrix4x4.TRS(new Vector3(offsetPaths[1][i].X / 10000f, 0, offsetPaths[1][i].Y / 10000f), Quaternion.Euler(-90, planSpline.edgeRotations[i], 0), Vector3.one);

                int[] leftTris = left_mesh.triangles;
                //leftTris.Reverse();

                //Debug.Log(left_mesh.triangles[0]);
                left_mesh.triangles = leftTris.Reverse().ToArray();
                left_mesh.RecalculateNormals();

                //Debug.Log(left_mesh.triangles[0]);

                tmpAXMesh = new AXMesh(left_mesh, left_wallm);
                tmpAXMesh.makerPO = parametricObject;
                ax_meshes.Add(tmpAXMesh);

               






            }

            parametricObject.finishMultiAXMeshAndOutput(ax_meshes, true); ;// renderToOutputParameter);


			// FINISH BOUNDING

			setBoundaryFromAXMeshes(ax_meshes);


			if (makeGameObjects)
				return parametricObject.makeGameObjectsFromAXMeshes (ax_meshes);


			return null;
		}




        // GET_LOCAL_CONSUMER_MATRIX_PER_INPUT_SOCKET
        public override Matrix4x4 getLocalConsumerMatrixPerInputSocket(AXParametricObject input_po)
        {
            //Debug.Log ("input_po.Name=" + input_po.Name + " :: planSrc_po.Name=" + planSrc_po.Name);// + " -- " + endCapHandleTransform);
            // PLAN
            if (input_po == planSrc_po)
            {
                if (P_Plan.flipX)
                    return Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90, 0, 0), new Vector3(-1, 1, 1));
                else
                    return Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90, 0, 0), Vector3.one);


            }
            return Matrix4x4.identity;

        }


    }
}
