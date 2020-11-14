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

	// PLAN_SEC GENERATOR
	public class PlanSweep : MesherExtruded, IMeshGenerator
	{
		public override string GeneratorHandlerTypeName { get { return "PlanSweepHandler"; } }

		// INPUTS


		public AXParameter P_Section;
		public AXParameter sectionSrc_p;
		public AXParametricObject sectionSrc_po;




		// INIT
		public override void init_parametricObject()
		{

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, AXParameter.ParameterType.Input, "Plan"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, AXParameter.ParameterType.Input, "Section"));

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Input, "End Cap Mesh"));

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.MaterialTool, AXParameter.ParameterType.Input, "Material"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.MaterialTool, AXParameter.ParameterType.Input, "Top Cap Material"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.MaterialTool, AXParameter.ParameterType.Input, "End Cap Material"));



			parametricObject.addParameter(AXParameter.DataType.Bool, "Top Cap", true);
			parametricObject.addParameter(AXParameter.DataType.Bool, "Bottom Cap", true);

			parametricObject.addParameter(AXParameter.DataType.Float, "Lip Top", 0f, 0f, 1000f);
			parametricObject.addParameter(AXParameter.DataType.Float, "Lip Bottom", 0f, 0f, 1000f);
			parametricObject.addParameter(AXParameter.DataType.Float, "Lip Edge", 0f, 0f, 1000f);

			parametricObject.addParameter(AXParameter.DataType.Bool, "End Cap A", false);
			parametricObject.addParameter(AXParameter.DataType.Bool, "End Cap B", false);

			parametricObject.addParameter(AXParameter.DataType.Bool, "Auto Bevel Ends", true);


			base.init_parametricObject();

			// HANDLES
			parametricObject.addHandle("depth", AXHandle.HandleType.Point, "0", "depth", "0", "depth=han_y");

			// Code
		}

		// POLL INPUTS (only on graph change())
		public override void pollInputParmetersAndSetUpLocalReferences()
		{
			base.pollInputParmetersAndSetUpLocalReferences();

			P_Plan = parametricObject.getParameter("Plan");
			P_Section = parametricObject.getParameter("Section");



		}


		// POLL CONTROLS (every model.generate())
		public override void pollControlValuesFromParmeters()
		{
			base.pollControlValuesFromParmeters();


			reduceValuesForDetailLevel();

		}

		public void reduceValuesForDetailLevel()
		{
			if (parametricObject.model.segmentReductionFactor < .3f)
			{
				lipEdge = 0;
			}
			if (parametricObject.model.segmentReductionFactor < .2f)
			{
				lipTop = 0;
				lipBottom = 0;
			}

		}




		public override AXParameter getPreferredInputParameter()
		{
			return P_Section;
		}





		public override void connectionMadeWith(AXParameter to_p, AXParameter from_p)
		{

			base.connectionMadeWith(to_p, from_p);

			AXParameter this_p = (to_p.parametricObject == parametricObject) ? to_p : from_p;
			AXParameter src_p = (to_p.parametricObject == parametricObject) ? from_p : to_p;


			this_p.shapeState = src_p.shapeState;
			this_p.breakGeom = src_p.breakGeom;
			this_p.breakNorm = src_p.breakNorm;

			if ((this_p.Name == "Plan" && sectionSrc_p != null) || (this_p.Name == "Section" && planSrc_p != null))
			{
				// THEN THIS IS THE FINAL HOOKUP
				parametricObject.boolValue("Bottom Cap", false);

				if (P_Plan.shapeState == ShapeState.Closed && P_Section.shapeState == ShapeState.Closed)
				{
					parametricObject.boolValue("Top Cap", false);
					parametricObject.boolValue("End Cap A", false);
					parametricObject.boolValue("End Eap B", false);
				}
				else if (P_Plan.shapeState == ShapeState.Open && P_Section.shapeState == ShapeState.Closed)
				{
					parametricObject.boolValue("Top Cap", false);
					parametricObject.boolValue("End Cap A", true);
					parametricObject.boolValue("End Cap B", true);
				}
				else if (P_Plan.shapeState == ShapeState.Closed && P_Section.shapeState == ShapeState.Open)
				{
					parametricObject.boolValue("Top Cap", true);
					parametricObject.boolValue("End Cap A", false);
					parametricObject.boolValue("End Cap B", false);
				}
				else
				{   // BOTH OPEN
					parametricObject.boolValue("Top Cap", false);
					parametricObject.boolValue("End Cap A", false);
					parametricObject.boolValue("End Cap B", false);
				}
			}



		}



		public override void connectionBrokenWith(AXParameter p)
		{
			base.connectionBrokenWith(p);


			switch (p.Name)
			{

				case "Plan":
					planSrc_po = null;
					P_Output.meshes = null;
					break;


				case "Section":
					sectionSrc_po = null;
					P_Output.meshes = null;
					break;

			}



		}




		// GENERATE PLAN_SWEEP
		public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool renderToOutputParameter)
		{
			if (parametricObject == null || !parametricObject.isActive)
				return null;

			P_Output.meshes = null;
			P_Output.mesh = null;


            //Debug.Log("PlanSweep::generate()");

            //Debug.Log("PlanSweep: " + parametricObject.Name);

            // RESULTING MESHES
            ax_meshes = new List<AXMesh>();


			preGenerate();




			// PLAN
			// The plan may have multiple paths. Each may generate a separate GO.

			if (P_Plan == null)
				return null;



			if (planSrc_p == null || !planSrc_p.parametricObject.isActive)
				return null;

			P_Plan.polyTree = null;





			AXShape.thickenAndOffset(ref P_Plan, planSrc_p);





			// SECTION

			// The plan may have multiple paths. Each may generate a separate GO.

			if (P_Section == null)
			{
				P_Output.meshes = null;
				return null;
			}



			sectionSrc_p = getUpstreamSourceParameter(P_Section);
			sectionSrc_po = (sectionSrc_p != null) ? sectionSrc_p.parametricObject : null;

			if (sectionSrc_p == null || !sectionSrc_p.parametricObject.isActive)
			{
				P_Output.meshes = null;
				return null;
			}

			if ((sectionSrc_p.paths == null || sectionSrc_p.paths.Count == 0) && sectionSrc_p.polyTree == null)
				return null;



			P_Section.polyTree = null;

			if (lipTop > 0 && P_Section.shapeState == ShapeState.Open && sectionSrc_p.paths != null && sectionSrc_p.paths.Count == 1)
			{
				P_Section.paths = new Paths();
				Path path = new Path();


				foreach (IntPoint ip in sectionSrc_p.paths[0])
					path.Add(new IntPoint(ip.X, ip.Y));



				if (lipBottom > 0)
				{
					path.Insert(0, new IntPoint((path[0].X - lipBottom * AXGeometry.Utilities.IntPointPrecision), path[0].Y));

					if (lipEdge > 0)
						path.Insert(0, new IntPoint(path[0].X, (path[0].Y + lipEdge * AXGeometry.Utilities.IntPointPrecision)));

				}

				if (lipTop > 0)
				{
					path.Add(new IntPoint((path[path.Count - 1].X - lipTop * AXGeometry.Utilities.IntPointPrecision), path[path.Count - 1].Y));

					if (lipEdge > 0)
						path.Add(new IntPoint((path[path.Count - 1].X), (path[path.Count - 1].Y - lipEdge * AXGeometry.Utilities.IntPointPrecision)));
				}

				P_Section.paths.Add(path);
			}
			else
				AXShape.thickenAndOffset(ref P_Section, sectionSrc_p);





			//StopWatch sw = new StopWatch();



			GameObject retGO = generateFirstPass(initiator_po, makeGameObjects, P_Plan, P_Section, Matrix4x4.identity, renderToOutputParameter);
			//Debug.Log("PlanSweep ============= Done: " + sw.duration());



			// FINISH AX_MESHES

			parametricObject.finishMultiAXMeshAndOutput(ax_meshes, renderToOutputParameter);



			// FINISH BOUNDING

			setBoundaryFromAXMeshes(ax_meshes);


			//adjustWorldMatrices();

			return retGO;
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
			// SECTION
			if (input_po == sectionSrc_po)
			{
				//Debug.Log("??? " + sectionHandleTransform);
				if (P_Section.flipX)
				{
					return sectionHandleTransform * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90, 0, 0), new Vector3(-1, 1, 1));
				}
				else
				{
					if (P_Plan.shapeState == ShapeState.Open)
						return endCapHandleTransform;
					else
						return sectionHandleTransform * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90, 0, 0), Vector3.one);
				}
			}
			// ENDCAP
			if (P_EndCapMesh != null && P_EndCapMesh.DependsOn != null && input_po == P_EndCapMesh.DependsOn.Parent)
				return endCapHandleTransform;

			return Matrix4x4.identity;
		}
	}
}
