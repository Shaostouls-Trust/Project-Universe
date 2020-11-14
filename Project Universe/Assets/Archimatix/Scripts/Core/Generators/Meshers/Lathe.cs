// http://www.archimatix.com

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
	
	
	
	
	// LATHE GENERATOR
	public class Lathe : MesherExtruded, IMeshGenerator
	{
		

		public override string GeneratorHandlerTypeName { get { return "LatheHandler"; } }

		public const float MIN_RAD = 2f;
		public static Matrix4x4 ShiftRadMatrix;

		// INPUTS
		public AXParameter P_Section;



		public AXParameter	P_Radius;
		public AXParameter	P_Segs;
		public AXParameter	P_SweepAngle;
		public AXParameter	P_Faceted;
		public AXParameter	P_ContinuousU;


		// WORKING FIELDS (Polled every Generate)
		public AXParameter 			sectionSrc_p;
		public AXParametricObject 	sectionSrc_po;

		public float 	radius		= 0;
		public int 		segs		= 16;
		public float 	sweepAngle 	= 360;
		public bool 	faceted;
		public bool 	continuousU = true;

		public float snappedSweepAngle;

		// For automatically turning on end caps...
		public float prevSnappedSweepAngle;




		// INIT
		public override void init_parametricObject() 
		{

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, AXParameter.ParameterType.Input, "Section"));

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Input, "End Cap Mesh"));

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.MaterialTool, AXParameter.ParameterType.Input, "Material"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.MaterialTool, AXParameter.ParameterType.Input, "Top Cap Material"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.MaterialTool, AXParameter.ParameterType.Input, "End Cap Material"));

			 			
			// SPECIFIC LATHE CONTROLS
			parametricObject.addParameter(AXParameter.DataType.Float, AXParameter.ParameterType.GeometryControl, 	"Radius", 		0f, 	 0f, 3000f);
			parametricObject.addParameter(AXParameter.DataType.Int , 	"Segs",  		16, 		3, 64);
			parametricObject.addParameter(AXParameter.DataType.Float , 	"SweepAngle", 	  360f, 	1f, 360f);
			parametricObject.addParameter(AXParameter.DataType.Bool, 	"Faceted", 	false);
			parametricObject.addParameter(AXParameter.DataType.Bool, 	"ContinuousU", 	true);

			// GENERAL MESHER PARAMETERS
			base.init_parametricObject();



			parametricObject.addParameter(AXParameter.DataType.Bool, 	"Top Cap", 		true);
			parametricObject.addParameter(AXParameter.DataType.Bool, 	"Bottom Cap", 	true);

			parametricObject.addParameter(AXParameter.DataType.Float,	"Lip Top", 		0f, 0f, 1000f);
			parametricObject.addParameter(AXParameter.DataType.Float,	"Lip Bottom", 	0f, 0f, 1000f);
			parametricObject.addParameter(AXParameter.DataType.Float,	"Lip Edge", 	0f, 0f, 1000f);

			parametricObject.addParameter(AXParameter.DataType.Bool, 	"End Cap A", 	false);
			parametricObject.addParameter(AXParameter.DataType.Bool, 	"End Cap B", 	false);


		}

		// POLL INPUTS (only on graph change())
		public override void pollInputParmetersAndSetUpLocalReferences()
		{
			base.pollInputParmetersAndSetUpLocalReferences();

			P_Section 		= parametricObject.getParameter("Section");
			P_Radius 		= parametricObject.getParameter("Radius", "radius");
			P_Segs 			= parametricObject.getParameter("Segs", "segs");
			P_SweepAngle 	= parametricObject.getParameter("SweepAngle", "end angle");
			P_Faceted 		= parametricObject.getParameter("Faceted");
			P_ContinuousU 		= parametricObject.getParameter("ContinuousU");



		}


		public override AXParameter getPreferredInputParameter()
		{
			return P_Section;
		}
		

		// POLL CONTROLS (every model.generate())
		public override void pollControlValuesFromParmeters()
		{
			base.pollControlValuesFromParmeters();

            autoBevelOpenEnds = true;

            sectionSrc_p	= getUpstreamSourceParameter(P_Section);
			sectionSrc_po 	= (sectionSrc_p != null) ? sectionSrc_p.parametricObject : null;

			radius 	= (P_Radius != null) 	? P_Radius.FloatVal : 0;
			segs 	= (P_Segs != null) 		? P_Segs.IntVal		: 8;


			if (segs < 3)
			{ 	
				segs = 3;
				P_Segs.intval = 3;
			}

			sweepAngle = (P_SweepAngle != null) ? P_SweepAngle.FloatVal : 360;



			snappedSweepAngle =  (sweepAngle > 358 || sweepAngle < 2) ? 360 : sweepAngle;

			faceted 	= (P_Faceted != null) 		? P_Faceted.boolval : false;
			continuousU = (P_ContinuousU != null) 	? P_ContinuousU.boolval : false;


			if (prevSnappedSweepAngle == 360 && snappedSweepAngle < 360)
			{
				// Just opened it up
				P_EndCapA.initiateRipple_setBoolValueFromGUIChange(true);
				P_EndCapB.initiateRipple_setBoolValueFromGUIChange(true);

			} 
			else if (prevSnappedSweepAngle < 360 && snappedSweepAngle == 360)
			{
				// just closed
				P_EndCapA.initiateRipple_setBoolValueFromGUIChange(false);
				P_EndCapB.initiateRipple_setBoolValueFromGUIChange(false);

			}
			prevSnappedSweepAngle = snappedSweepAngle;


			reduceValuesForDetailLevel();

		}

		public void reduceValuesForDetailLevel()
		{
			if (parametricObject.model.segmentReductionFactor < .26f)
			{
				lipEdge = 0;
			}
			if (parametricObject.model.segmentReductionFactor < .16f)
			{
				lipTop = 0;
				lipBottom = 0;
			}

		}




		 /*
		public override float validateFloatValue(AXParameter p, float v)
		{
			Debug.Log("parameterWasModified="+p.Name);

			float value = v;
			switch(p.Name)
			{
				case "SweepAngle":
				if (sweepAngle > 360 || sweepAngle <10)
					value = 360;
				

				break;
			}

			return value;
		}
		*/

		public override void connectionMadeWith(AXParameter to_p, AXParameter from_p)
		{

			base.connectionMadeWith(to_p, from_p);

			if (from_p == null)// || from_p.DependsOn == null)
				return;

			AXParameter this_p = (to_p.parametricObject == parametricObject) ? to_p   : from_p;
			AXParameter src_p  = (to_p.parametricObject == parametricObject) ? from_p : to_p;

			// use this po's bounds to init the radius...
			
			this_p.shapeState 	= src_p.shapeState;
			this_p.breakGeom	= src_p.breakGeom;
			this_p.breakNorm	= src_p.breakNorm;


			pollControlValuesFromParmeters();

			if (sectionSrc_po == null)
				return;
	

			/*
			Paths secPaths = new Paths();
			
			if (sectionSrc_p.polyTree != null)
				secPaths = Clipper.ClosedPathsFromPolyTree(sectionSrc_p.polyTree);
			else if (sectionSrc_p.paths != null)
				secPaths = sectionSrc_p.paths;	
			*/

			///parametricObject.isInitialized = true;

			if (! parametricObject.isInitialized)
			{
				parametricObject.isInitialized = true;
				//IntRect bounds = Clipper.GetBounds(secPaths);

				if (P_Section != null)
				{
					P_Section.breakNorm = (faceted 		?  10 :  135);
					//P_Section.breakGeom = (continuousU && ! faceted	? 100 :   10);

				}



				/*
				float minx =  ((float) bounds.left/AXGeometry.Utilities.IntPointPrecision);

				if (P_Section.shapeState == ShapeState.Closed)
					minx *= 2;
				Debug.Log("minx="+minx);		
				if (minx<0)
					parametricObject.floatValue("radius",-minx+.5f);
				*/
			} 

			//Debug.Log("this_p.shapeState="+this_p.shapeState);
			if ( this_p.Name == "Section" && this_p.shapeState == ShapeState.Closed )
			{
				parametricObject.boolValue("Top Cap", false);
				parametricObject.boolValue("Bottom Cap", false);
			}



		}
		



		public override void connectionBrokenWith(AXParameter p)
		{
			base.connectionBrokenWith(p);


			switch (p.Name)
			{
				
			case "Section":
				sectionSrc_po = null;
				P_Output.meshes = null;
				break;

			}

		}




		
		// GENERATE LATHE

		// The only thing a Lathe Node does is prepare a circular Section. Otherwise it is simple a PlanSweep.

		public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool renderToOutputParameter)
		{

			if (parametricObject == null || ! parametricObject.isActive)
				return null;

			// RESULTING MESHES
			ax_meshes = new List<AXMesh>();



			preGenerate();


			// PLAN - is an arc or circle
			// generate the plane shape. A circle at .5 radius
			AXParameter plan_p = new AXParameter();
			plan_p.Parent = parametricObject;
			plan_p.Type = AXParameter.DataType.Shape;
			plan_p.paths = new Paths();


			float actingRadius = radius+MIN_RAD;  
			//Debug.Log("actingRadius="+actingRadius);


			int msegs = Mathf.Max( 1, Mathf.FloorToInt( ( (float)segs * parametricObject.model.segmentReductionFactor ) ) );

			if (snappedSweepAngle == 360)
			{
				plan_p.paths.Add( AXTurtle.Circle(actingRadius, msegs) );
				plan_p.shapeState = ShapeState.Closed;
			}
			else
			{
                plan_p.paths.Add(AXTurtle.Arc(actingRadius, 0, snappedSweepAngle, msegs));
                //plan_p.paths.Add(AXTurtle.SemiCircS(actingRadius,.31f, msegs));
                plan_p.shapeState = ShapeState.Open;
			}



			//plan_p.breakGeom = 10;
			plan_p.breakNorm = (faceted ? 10 : 135);
			plan_p.breakGeom = (continuousU 	? 100 :   10);




			// In order to conceptualize the section as at a vert rather than the normal case for PlanSweep where it is on a length,
			// shift the section out by: dx = R ( 1 - cos(ß/2))
			// http://www.archimatix.com/geometry/compensating-for-plansweep-in-a-lathe-mesher

			float beta = snappedSweepAngle / segs;
			float dx = actingRadius * (1 - Mathf.Cos( Mathf.Deg2Rad*beta/2));

			//Debug.Log("actingRadius="+actingRadius+", beta="+beta+", dx="+dx);


			ShiftRadMatrix = Matrix4x4.TRS(new Vector3(-MIN_RAD+dx, 0, 0), Quaternion.identity, Vector3.one);

			// SECTION
			
			// The plan may have multiple paths. Each may generate a separate GO.
			
			if (P_Section == null || sectionSrc_p == null || ! sectionSrc_p.parametricObject.isActive)
				return null;

          

			P_Section.polyTree = null;
			P_Section.thickness = 0;
			P_Section.offset = 0;
			
			AXShape.thickenAndOffset(ref P_Section, sectionSrc_p);


            //Debug.Log(parametricObject.Name);
            //Debug.Log(ShiftRadMatrix);
            if (P_Section.polyTree != null)
				AX.Generators.Generator2D.transformPolyTree(P_Section.polyTree, ShiftRadMatrix);
			else
				P_Section.paths = AX.Generators.Generator2D.transformPaths(P_Section.paths, ShiftRadMatrix);


           

            if ((P_Section.paths == null || P_Section.paths.Count == 0) && P_Section.polyTree == null)
                return null;

            

            GameObject retGO = null;

			if (makeGameObjects && P_Section.polyTree == null && P_Section.paths != null && P_Section.paths.Count > 1)
			{
				//Debug.Log("make one for each section");
				retGO = ArchimatixUtils.createAXGameObject(parametricObject.Name, parametricObject);

				for(int i=0; i<P_Section.paths.Count; i++)
				{
					AXParameter tmpSecP = new AXParameter();
					tmpSecP.shapeState = ShapeState.Open;
					tmpSecP.parametricObject = parametricObject;
					Paths tmpPaths = new Paths();
					tmpPaths.Add(P_Section.paths[i]);
					tmpSecP.paths = tmpPaths;
					GameObject tmpObj = generateFirstPass(initiator_po, makeGameObjects, plan_p, tmpSecP, ShiftRadMatrix, renderToOutputParameter);

					tmpObj.transform.parent = retGO.transform;

				}
			}
			else
				retGO =  generateFirstPass(initiator_po, makeGameObjects, plan_p, P_Section, ShiftRadMatrix, renderToOutputParameter);



			// FINISH AX_MESHES

			parametricObject.finishMultiAXMeshAndOutput(ax_meshes, renderToOutputParameter);




			// FINISH BOUNDING

			setBoundaryFromAXMeshes(ax_meshes);


			if (P_Section.polyTree != null)
				AX.Generators.Generator2D.transformPolyTree(P_Section.polyTree, ShiftRadMatrix.inverse);
			//else
			//	P_Section.paths = AX.Generators.Generator2D.transformPaths(P_Section.paths, ShiftRadMatrix);


			return retGO;

		}






		// GET_LOCAL_CONSUMER_MATRIX_PER_INPUT_SOCKET
		public override Matrix4x4 getLocalConsumerMatrixPerInputSocket(AXParametricObject input_po)
		{
			
			// SECTION
			if (input_po == sectionSrc_po)
				return sectionHandleTransform * Matrix4x4.TRS(new Vector3(radius, 0, 0), Quaternion.identity, Vector3.one);// * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90, 0, 0), Vector3.one);// ;

			// ENDCAP
			if (P_EndCapMesh != null && P_EndCapMesh.DependsOn != null && input_po == P_EndCapMesh.DependsOn.parametricObject)
			{
				return   endCapHandleTransform * ShiftRadMatrix;
			}				
			return Matrix4x4.identity ;
		}
	




	}

}