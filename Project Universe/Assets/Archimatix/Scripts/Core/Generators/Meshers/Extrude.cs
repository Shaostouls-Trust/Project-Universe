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
	
	
	
	
	// EXTRUDE GENERATOR
	public class Extrude : MesherExtruded, IMeshGenerator
	{
        
	 
		public override string GeneratorHandlerTypeName { get { return "ExtrudeHandler"; } }

		// INPUTS

		public AXParameter P_Extrude;
		public AXParameter P_Bevels_Unified;
		public AXParameter P_Bevel_Top;
		public AXParameter P_Bevel_Bottom;
		public AXParameter P_BevelHardEdge;
		public AXParameter P_BevelSegs;
		public AXParameter P_BevelOut;

		public AXParameter P_Taper;

		public AXParameter P_Segs;

		// POLLED MEMBERS

		public float 	extrude 		= 2;
		public bool 	bevelsUnified 	= true;
		public float 	bevelTop 		= .05f;
		public float 	bevelBottom 	= .05f;
		public bool 	bevelHardEdge 	= false;
		public int 		bevelSegs 		= 1;
		public bool 	bevelOut 		= true;
		public float 	taper 			= 0;
		public int 		segs 			= 1;



		// INIT EXTRUDE GENERATOR
		public override void init_parametricObject() 
		{
			

			//parametricObject.addShape("Plan");
			
			
			// PLAN SHAPE
			AXParameter p = parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, AXParameter.ParameterType.Input, "Plan"));
			

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.MaterialTool, AXParameter.ParameterType.Input, "Material"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.MaterialTool, AXParameter.ParameterType.Input, "Top Cap Material"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.MaterialTool, AXParameter.ParameterType.Input, "Bottom Cap Material"));


			// SPECIFIC EXTRUDE CONTROLS
			
			//parametricObject.addParameter(new AXParameter(AXParameter.DataType.Plane, AXParameter.ParameterType.Input, "Elev Plane"));
			

			p = parametricObject.addParameter(AXParameter.DataType.Float, AXParameter.ParameterType.GeometryControl, "Height", 2f, .01f, 1000f);
			p.sizeBindingAxis = Axis.Y;
			parametricObject.addParameter(AXParameter.DataType.Bool,  	"Bevels Unified", true);
			parametricObject.addParameter(AXParameter.DataType.Float,  	"Bevel Top", 0, 0f, 100f );
			parametricObject.addParameter(AXParameter.DataType.Float,  	"Bevel Bottom", 0f, 0f, 100f );
			parametricObject.addParameter(AXParameter.DataType.Bool,  	"Bevel Out", false);
			parametricObject.addParameter(AXParameter.DataType.Bool,  	"Bevel Hard Edge", 1 );
			parametricObject.addParameter(AXParameter.DataType.Int,  	"Bevel Segs", 1, 1, 5 );
			//parametricObject.addParameter(AXParameter.DataType.Float,  	"Taper", 0, -10000, 10000 );

			parametricObject.addParameter(AXParameter.DataType.Float,	"Taper");

			parametricObject.addParameter(AXParameter.DataType.Bool, 	"Top Cap", 		true);
			parametricObject.addParameter(AXParameter.DataType.Bool, 	"Bottom Cap", 	true);


			parametricObject.addParameter(AXParameter.DataType.Float,	"Lip Top", 		0f, 0f, 1000f);
			parametricObject.addParameter(AXParameter.DataType.Float,	"Lip Edge");
			parametricObject.addParameter(AXParameter.DataType.Float,	"Lip Bottom", 	0f, 0f, 1000f);
			parametricObject.addParameter(AXParameter.DataType.Float,	"Lip Edge Bottom");
            parametricObject.addParameter(AXParameter.DataType.Int, "Segs", 1, 1, 36);

            base.init_parametricObject();

		}
		

		// POLL INPUTS (only on graph change())
		public override void pollInputParmetersAndSetUpLocalReferences()
		{
			base.pollInputParmetersAndSetUpLocalReferences();



			P_Extrude 		= parametricObject.getParameter("Height", "Extrude");

			P_Bevels_Unified= parametricObject.getParameter("Bevels Unified");
			P_Bevel_Top 	= parametricObject.getParameter("Bevel Top", "Bevel Radius Top");
			P_Bevel_Bottom 	= parametricObject.getParameter("Bevel Bottom", "Bevel Radius Bottom");
			P_BevelHardEdge = parametricObject.getParameter("Bevel Hard Edge");
			P_BevelSegs 	= parametricObject.getParameter("Bevel Segs");
			P_BevelOut 		= parametricObject.getParameter("Bevel Out");

			P_Taper			= parametricObject.getParameter("Taper");

			P_Segs			= parametricObject.getParameter("Segs");

			if (P_Segs == null)
				P_Segs = parametricObject.addParameter(AXParameter.DataType.Int,	"Segs", 	1, 1, 36);

				

		}

		public override AXParameter getPreferredInputParameter()
		{			
			return P_Plan;
		}
		 


		// POLL CONTROLS (every model.generate())
		public override void pollControlValuesFromParmeters()
		{
			
			base.pollControlValuesFromParmeters();

				 

			extrude 			= (P_Extrude != null)			?	P_Extrude.FloatVal : 1;


			// BEVELS
			bevelsUnified 		= (P_Bevels_Unified != null)  	?	P_Bevels_Unified.boolval : true;


			bevelTop 			= (P_Bevel_Top != null)  			?	Mathf.Clamp(P_Bevel_Top.FloatVal, 0, extrude*.49f) : 0;
			if (P_Bevel_Top != null) 
				P_Bevel_Top.FloatVal = bevelTop;

			bevelBottom 		= (P_Bevel_Bottom != null)  		?	Mathf.Clamp(P_Bevel_Bottom.FloatVal, 0, extrude*.49f) : 0;
			if (P_Bevel_Bottom != null)
				P_Bevel_Bottom.FloatVal = bevelBottom;


			//if (P_Bevel_Top != null && bevelTop != P_Bevel_Top.FloatVal)
			//	P_Bevel_Top.FloatVal = bevelTop;

			bevelHardEdge 		= (P_BevelHardEdge != null)  	?	P_BevelHardEdge.boolval : false;


			bevelSegs 			= (P_BevelSegs != null)  		?	Mathf.Clamp(P_BevelSegs.IntVal, 1, 5) : 1;

			if (P_BevelSegs != null && bevelSegs != P_BevelSegs.IntVal)
				parametricObject.intValue("Bevel Segs", bevelSegs);

			bevelOut 			= (P_BevelOut 	!= null)  	?	P_BevelOut.boolval : false;

			taper 				= (P_Taper 		!= null)	?	P_Taper.FloatVal 	: 0;

			segs 				= (P_Segs		!= null) 	?	P_Segs.IntVal 		: 0;

			reduceValuesForDetailLevel();

		}




		public void reduceValuesForDetailLevel()
		{
			bevelSegs = Mathf.Max( 1, Mathf.FloorToInt( ( (float)bevelSegs * parametricObject.model.segmentReductionFactor ) ) );

			if (parametricObject.model.segmentReductionFactor < .26f)
			{
				bevelTop = 0;
				bevelBottom = 0;
				lipEdge = 0;
			}
			if (parametricObject.model.segmentReductionFactor < .16f)
			{
				lipTop = 0;
				lipBottom = 0;
			}

		}


		public override float validateFloatValue(AXParameter p, float v)
		{

			float value = v;


			switch(p.Name)
			{
			case "Bevel Top":
			case "Bevel Radius Top":
				if (bevelsUnified)
					P_Bevel_Bottom.FloatVal = v;
				break;

			
			case "Bevel Bottom":
			case "Bevel Radius Bottom":
				if (bevelsUnified)
					P_Bevel_Top.FloatVal = v;
				

				break;
			
			}


			return value;
		}





		public override void connectionMadeWith(AXParameter to_p, AXParameter from_p)
		{

			AXParameter this_p = (to_p.parametricObject == parametricObject) ? to_p   : from_p;
			AXParameter src_p  = (to_p.parametricObject == parametricObject) ? from_p : to_p;


			base.connectionMadeWith(to_p, from_p);

			this_p.shapeState 	= src_p.shapeState;
			this_p.breakGeom	= src_p.breakGeom;
			this_p.breakNorm	= src_p.breakNorm;


			AXParametricObject po = from_p.parametricObject;


			//Debug.Log ("HERE!!!! " + po.Name);


			
			if (po.is2D())
			{
				if ((Axis) parametricObject.intValue("Axis") == Axis.NONE)
				{
					parametricObject.intValue("Axis", po.intValue("Axis"));
				}
			} 

			this_p.shapeState = src_p.shapeState;


			if (this_p.Name == "Plan")
			{
				parametricObject.boolValue("Top Cap", 		(this_p.shapeState == ShapeState.Closed) ? true : false);
				parametricObject.boolValue("Bottom Cap", 	(this_p.shapeState == ShapeState.Closed) ? true : false);
			}


		}
		

		

		public void clearOutput()
		{
			P_Output.paths 		= null; 
			P_Output.polyTree 	= null;
			P_Output.meshes 	= null;

		}

		public override void connectionBrokenWith(AXParameter p)
		{
			base.connectionBrokenWith(p);


			switch (p.Name)
			{
				
			case "Plan":
				planSrc_po = null;
				clearOutput();
				break;

			}

		}

		
		




		
		// GENERATE EXTRUDE
		public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool renderToOutputParameter)
		{
//			Debug.Log ("===> [" + parametricObject.Name + "] EXTRUDE generate ... MAKE_GAME_OBJECTS="+makeGameObjects + " renderToOutputParameter="+renderToOutputParameter);
		//Debug.Log(" ******** B0 extrude = " + extrude);

			if (parametricObject == null || ! parametricObject.isActive)
				return null;

			// RESULTING MESHES
			ax_meshes = new List<AXMesh>();


			//Debug.Log(" ******** A ");

			preGenerate();


			// PLAN - 
			// The plan may have multiple paths. Each may generate a separate GO.
			if (P_Plan == null || planSrc_p == null || ! planSrc_p.parametricObject.isActive)
				return null;

			//Debug.Log(" ******** B1 " + planSrc_p.parametricObject.Name + "." + planSrc_p.Name);
			//Debug.Log(" ******** B2 " + planSrc_p.getPaths().Count );



			// Offset is bevel and !bevelOut
			float originalOffset = P_Plan.offset;

			// set back by the max bevel - later consider taper and lip....








			if (! bevelOut)
			{
				/*
				float bevelMax = (bevelTop > bevelBottom) ? bevelTop : bevelBottom;

				if (bevelMax > 0 )
				{
					offsetModified -= bevelMax;
					//P_Plan.joinType = JoinType.jtMiter;
				}
				*/
				 
				P_Plan.offset -= (bevelBottom > bevelTop) ? bevelBottom : bevelTop;
			}	



			// Splitting concave shapes?
			// THis is a pecial case. An ssumption is mad of no holes 
			// so that we can deal only with paths.

			if (parametricObject.splitConcaveShapes)//P_Plan.offset != 0)
			{




			// SPLIT INTO CONCAVES ** ** ** ** ** ** if no holes, and makeConcave,
				Paths paths;
				AXParameter tmpSrc = (! renderToOutputParameter && P_Plan.Dependents != null) ? P_Plan : planSrc_p;

				if (tmpSrc.polyTree != null)
			
					paths = Clipper.PolyTreeToPaths(tmpSrc.polyTree);
				else
					paths = tmpSrc.paths;

				//Pather.printPaths(P_Plan.paths);

				if (paths != null)
				{
					P_Plan.paths = new Paths();

					foreach (Path path in paths)
					{
						Paths splitPaths = Pather.splitIntoConvexPaths(path);
						splitPaths = Pather.offset(splitPaths, P_Plan.offset);
						P_Plan.paths.AddRange(splitPaths);
					}
				}
				P_Plan.polyTree = null;
			
				// SPLIT INTO CONCAVES ** ** ** ** ** ** ** ** if no holes, and makeConcave,



			}
			else
			{
				//Debug.Log("A: "+planSrc_p.paths[0].Count);
				P_Plan.polyTree = null;

				//Debug.Log("EXTRUED " + parametricObject.Name);
	//			Pather.printPaths(P_Plan.paths);

				if (! renderToOutputParameter && P_Plan.Dependents != null)
				{
					// Some Replicant has set the paths of this Plan_P manually
					AXShape.thickenAndOffset(ref P_Plan,  P_Plan);
				}
				else
					AXShape.thickenAndOffset(ref P_Plan,  planSrc_p);
				
			}

			P_Plan.offset = originalOffset;













            //Debug.Log("C: P_Plan=" +P_Plan.getPaths());



            // DEFAULT SECTION -- USING BI_CHAMFER_SIDEflipX




            int reducedBevelSges = Mathf.FloorToInt( parametricObject.model.segmentReductionFactor * bevelSegs);
            if (reducedBevelSges < 1) reducedBevelSges = 1;

            //Debug.Log("segmentReductionFactor="+ parametricObject.model.segmentReductionFactor+", reducedSges=" + reducedBevelSges);
            Path sectionPath = new Path();
			sectionPath = AXTurtle.BiChamferSide(extrude, bevelTop, bevelBottom, reducedBevelSges, true, taper, lipTop, lipBottom, lipEdge, lipEdgeBottom, segs);

			AXParameter sec_p = new AXParameter();
			sec_p.Parent 			= parametricObject;
			sec_p.parametricObject 	= parametricObject;
			sec_p.Type = AXParameter.DataType.Shape;
			sec_p.shapeState = ShapeState.Open;
			sec_p.paths = new Paths();
			sec_p.paths.Add(sectionPath);

			if (parametricObject.boolValue("Bevel Hard Edge"))
			{
				sec_p.breakGeom = 0;
				sec_p.breakNorm = 0;
			}



            //StopWatch sw = new StopWatch();

            //P_Plan.parametricObject.generator.generate (false, null, false);

            //Debug.Log("Extrude ===================== " + P_Plan.paths + " :: " + P_Plan.polyTree + " --- " + P_Plan.hasPolytreeItems );

            GameObject retGO =  generateFirstPass(initiator_po, makeGameObjects, P_Plan, sec_p, Matrix4x4.identity, renderToOutputParameter);
           // Debug.Log("Extrude Done ===================== ax_meshes.Count" + ax_meshes.Count);// + sw.duration());


			// FINISH AX_MESHES

			parametricObject.finishMultiAXMeshAndOutput(ax_meshes, true);//renderToOutputParameter);



            //  TEST LOD ********************

            //if (makeGameObjects)
            //{
            //    float prevSegmentReductionFactor = parametricObject.model.segmentReductionFactor;

            //    GameObject LODGroupGO = ArchimatixUtils.createAXGameObject(parametricObject.Name + "_LODGroup", parametricObject);
            //    LODGroup lodg = LODGroupGO.AddComponent<LODGroup>();
            //    LOD[] lods = new LOD[3];

            //    parametricObject.model.segmentReductionFactor = 1f;

            //    retGO.name = parametricObject.Name + "_LOD0";
            //    retGO.transform.SetParent(LODGroupGO.transform);

            //    Renderer[] renderers0 = new Renderer[1];
            //    renderers0[0] = retGO.GetComponent<Renderer>();
            //    lods[0] = new LOD(1.0f, renderers0);


            //    parametricObject.model.segmentReductionFactor = .5f;
            //    GameObject retGO2 = generateFirstPass(initiator_po, makeGameObjects, P_Plan, sec_p, Matrix4x4.identity, renderToOutputParameter);
            //    retGO2.name = parametricObject.Name + "_LOD1";
            //    retGO2.transform.SetParent(LODGroupGO.transform);
            //    Renderer[] renderers1 = new Renderer[1];
            //    renderers1[0] = retGO2.GetComponent<Renderer>();
            //    lods[1] = new LOD(.5f, renderers1);


            //    parametricObject.model.segmentReductionFactor = .15f;
            //    GameObject retGO3 = generateFirstPass(initiator_po, makeGameObjects, P_Plan, sec_p, Matrix4x4.identity, renderToOutputParameter);
            //    retGO3.name = parametricObject.Name + "_LOD2";
            //    retGO3.transform.SetParent(LODGroupGO.transform);
            //    Renderer[] renderers2 = new Renderer[1];
            //    renderers2[0] = retGO3.GetComponent<Renderer>();
            //    lods[2] = new LOD(.25f, renderers2);


            //    lodg.SetLODs(lods);


            //    parametricObject.model.segmentReductionFactor = prevSegmentReductionFactor;

            //    return LODGroupGO;
            //}

            // FINISH BOUNDING

            setBoundaryFromAXMeshes(ax_meshes);

			//Debug.Log("Extrude: "+ parametricObject.bounds);




			return retGO;

			 
		} 
		  
		 
		public override Matrix4x4 getLocalConsumerMatrixPerInputSocket(AXParametricObject input, AXParameter input_p=null)
		{

			if (input_p != null && input_p.Name == "Plan")
				return Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90, 0, 0), Vector3.one);

			if (planSrc_po != null)
			{
				if (planSrc_po != null && input == planSrc_po)
				{
					if (P_Plan.flipX)
						return Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90, 0, 0), new Vector3(-1,1,1));
					else
						return Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90, 0, 0), Vector3.one);

				}
				return Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(-90, 0, 0), Vector3.one);
			}
			return Matrix4x4.identity;
		}
	}
}
