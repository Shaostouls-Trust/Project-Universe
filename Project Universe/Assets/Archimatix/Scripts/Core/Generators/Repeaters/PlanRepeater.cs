using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;


using AXClipperLib;
using Path = System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;

using AXPoly2Tri;
using PolygonPoints = System.Collections.Generic.List<AXPoly2Tri.PolygonPoint>;


using AXGeometry;
using AX;
 
namespace AX.Generators
{ 

	
	// Plan_REPEATER
	
	//public class PlanRepeater : RepeaterBase, IRepeater
	public class PlanRepeater : Mesher, IMeshGenerator
	{
		public override string GeneratorHandlerTypeName { get { return "PlanRepeaterHandler"; } }

		// INPUTS


		public AXParameter 			P_Section;
		public AXParameter 			sectionSrc_p;
		public AXParametricObject 	sectionSrc_po;



		// REPEATER INPUTS -- THESE IN INTERFACE?

		// CORNER
		public AXParameter			P_Corner;
		public AXParameter 			cornerSrc_p;
		public AXParametricObject 	cornerSrc_po;

		// NODE
		public AXParameter			P_Node;
		public AXParameter 			nodeSrc_p;
		public AXParametricObject 	nodeSrc_po;

		// CELL
		public AXParameter			P_Cell;
		public AXParameter 			cellSrc_p;
		public AXParametricObject 	cellSrc_po;

		// REPEATER TOOL
		public AXParameter			P_Repeater;
		public RepeaterTool 		repeaterTool;

		// PROGRESSIVE ROTATION
		public AXParameter 			P_ProgressiveRotation;
		public float 				progressiveRotation;




		// INSET
		public AXParameter			P_Inset;
		public float 				inset;

		// MAX_SEGMENT_LENGTH
		public AXParameter			P_MaxSegmentLength;
		public float 				maxSegmentLength;


		// BAY
		public AXParameter			P_Bay;
		public float 				bay;

		// CORNER BREAK ANGLE
		public AXParameter			P_CornerBreakAngle;
		public float 				cornerBreakAngle;



		public Paths				planPaths;
		public Spline[]				planSplines;


		public AXParameter 	P_CornerAngled;
		public AXParameter 	P_ProgressiveRotationX;
		public AXParameter 	P_ProgressiveRotationY;
		public AXParameter 	P_ProgressiveRotationZ;


		public bool  cornerAngled;
		public float progressiveRotationX;
		public float progressiveRotationY;
		public float progressiveRotationZ;



		public List<SubsplineIndices[]>	paths_SubsplineIndices;


		public List<Matrix4x4> 		spanNodeTransforms;


		public override void initGUIColor ()
		{
			
			GUIColor 		= new Color(.7f, .8f, 1, .7f);
			GUIColorPro 	= new Color(.85f,.85f,1f,1f);
			ThumbnailColor  = new Color(.318f,.31f,.376f);

		}



		// INIT_PARAMETRIC_OBJECT
		public override void init_parametricObject() 
		{
			//SHAPES
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, 		AXParameter.ParameterType.Input, "Plan"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, 		AXParameter.ParameterType.Input, "Section"));

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, 		AXParameter.ParameterType.Input, "Corner Mesh"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, 		AXParameter.ParameterType.Input, "Node Mesh"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, 		AXParameter.ParameterType.Input, "Cell Mesh"));

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.MaterialTool, AXParameter.ParameterType.Input, "Material"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.MaterialTool, AXParameter.ParameterType.Input, "End Cap Material"));

			// JITTER
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.JitterTool,   AXParameter.ParameterType.Input, "Jitter Translation"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.JitterTool,   AXParameter.ParameterType.Input, "Jitter Rotation"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.JitterTool,   AXParameter.ParameterType.Input, "Jitter Scale"));



			base.init_parametricObject();

			parametricObject.addParameter(AXParameter.DataType.Float, 	"Inset", 		1, 	 0f, 100f);
			parametricObject.addParameter(AXParameter.DataType.Float, 	"Max Segment Length", 		25, 	 1f, 1000f);
			parametricObject.addParameter(AXParameter.DataType.Float, 	"Bay Width", 			2,	.01f, 100f);
			parametricObject.addParameter(AXParameter.DataType.Float, 	"Corner Break Angle", 			30,	.01f, 180f);



			parametricObject.addParameter(AXParameter.DataType.Bool, 	"CornerAngled", 	true);
			P_ProgressiveRotationX = parametricObject.addParameter(AXParameter.DataType.Float, 	"IncrRotX", 0f); 
			P_ProgressiveRotationY = parametricObject.addParameter(AXParameter.DataType.Float, 	"IncrRotY", 0f); 
			P_ProgressiveRotationZ = parametricObject.addParameter(AXParameter.DataType.Float, 	"IncrRotZ", 0f); 
			parametricObject.addParameter(AXParameter.DataType.Bool, 	"End Cap A", 	false);
			parametricObject.addParameter(AXParameter.DataType.Bool, 	"End Cap B", 	false);


			// OUTPUT
			//parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Output,"Output Mesh"));


		} 

		/*
		public override void initSpanParameters() 
		{
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Input, "Bay SpanU"));
		}

		public override void initRepeaterTools() 
		{
			Debug.Log(parametricObject.Name + " **** initRepeaterTools");
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.RepeaterTool, AXParameter.ParameterType.Input, "RepeaterU"));
			AXParametricObject repeaterTool =  parametricObject.model.createNode("RepeaterTool");
			repeaterTool.rect.x = parametricObject.rect.x-200;
			repeaterTool.isOpen = false;
			repeaterTool.intValue("Edge_Count", 100);
			parametricObject.getParameter("RepeaterU").makeDependentOn(repeaterTool.getParameter("Output"));
		}
		*/
		 
		// POLL INPUTS (only on graph change())
		public override void pollInputParmetersAndSetUpLocalReferences()
		{
			base.pollInputParmetersAndSetUpLocalReferences();

			P_Section 				= parametricObject.getParameter("Section");

			P_Inset 				= parametricObject.getParameter("Inset");
			P_MaxSegmentLength 		= parametricObject.getParameter("Max Segment Length");
			P_Bay 					= parametricObject.getParameter("Bay Width", "Bay");
			P_CornerBreakAngle 		= parametricObject.getParameter("Corner Break Angle");


			// REPEATER
			P_Corner 				= parametricObject.getParameter("Corner Mesh");
			P_Node 					= parametricObject.getParameter("Node Mesh");
			P_Cell 					= parametricObject.getParameter("Cell Mesh");

			P_Repeater 				=  parametricObject.getParameter("Repeater");


			P_CornerAngled			= parametricObject.getParameter("CornerAngled");
			P_ProgressiveRotationX 	= parametricObject.getParameter("IncrRotX");
			P_ProgressiveRotationY 	= parametricObject.getParameter("IncrRotY");
			P_ProgressiveRotationZ 	= parametricObject.getParameter("IncrRotZ");

		}

		

		public override AXParameter getPreferredInputParameter()
		{			
			return P_Plan;
		}

		public override void connectionMadeWith(AXParameter to_p, AXParameter from_p)
		{

			base.connectionMadeWith(to_p, from_p);

			if (P_Input != null)
			{
				P_Input.polyTree = null;
				P_Input.paths = null;
			}

		}
		public override void connectionBrokenWith(AXParameter p)
		{
			base.connectionBrokenWith(p);

			if (P_Input != null)
			{
				P_Input.polyTree = null;
				P_Input.paths = null;
			}

			switch (p.Name)
			{
				
			case "P_Corner":
				cornerSrc_po = null;
				P_Output.meshes = null;
				break;

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





		// POLL CONTROLS (every model.generate())
		public override void pollControlValuesFromParmeters()
		{
			base.pollControlValuesFromParmeters();






			cornerSrc_p 	= (P_Corner 	!= null) 	? getUpstreamSourceParameter(P_Corner)		: null;
			cornerSrc_po 	= (cornerSrc_p 	!= null) 	? cornerSrc_p.parametricObject				: null;

			nodeSrc_p 		= (P_Node 		!= null) 	? getUpstreamSourceParameter(P_Node)		: null;
			nodeSrc_po 		= (nodeSrc_p	!= null) 	? nodeSrc_p.parametricObject				: null;

				cellSrc_p 	= (P_Cell		!= null) 	? getUpstreamSourceParameter(P_Cell)		: null;
			cellSrc_po 		= (cellSrc_p	!= null) 	? cellSrc_p.parametricObject				: null;








			inset 				= (P_Inset != null) 			? P_Inset.FloatVal : 0;
			maxSegmentLength 	= (P_MaxSegmentLength != null) 	? P_MaxSegmentLength.FloatVal : 0;
			bay 				= (P_Bay != null) 				? P_Bay.FloatVal : 2f;

			cornerBreakAngle 		= (P_CornerBreakAngle != null) 	? P_CornerBreakAngle.FloatVal : 0;


			cornerAngled = (P_CornerAngled != null) ? P_CornerAngled.boolval : false;
			progressiveRotationX 	= (P_ProgressiveRotationX != null) ? P_ProgressiveRotationX.FloatVal : 0;
			progressiveRotationY 	= (P_ProgressiveRotationY != null) ? P_ProgressiveRotationY.FloatVal : 0;
			progressiveRotationZ 	= (P_ProgressiveRotationZ != null) ? P_ProgressiveRotationZ.FloatVal : 0;


		}










		// GENERATE PLAN_REPEATER
		public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool renderToOutputParameter)
		{
			if (parametricObject == null || ! parametricObject.isActive)
				return null;
			//Debug.Log("yo ******* makeGameObjects="+makeGameObjects);


			// RESULTING MESHES
			ax_meshes = new List<AXMesh>();

			paths_SubsplineIndices = new List<SubsplineIndices[]>();

			preGenerate();


						  
			planSrc_p		= P_Plan.DependsOn;// getUpstreamSourceParameter(P_Plan);
			planSrc_po 		= (planSrc_p != null) 								? planSrc_p.parametricObject 	: null;

			//Debug.Log("planSrc_po = " + planSrc_po.Name+"."+planSrc_p.Name + " ... " + planSrc_p.DependsOn.PType + " ..... " + planSrc_p.getPaths());

			P_Plan.polyTree = null;
			AXShape.thickenAndOffset(ref P_Plan, planSrc_p);
			if (P_Plan.reverse)
				P_Plan.doReverse();


			planPaths = P_Plan.getPaths();

			if (planPaths == null || planPaths.Count == 0)
				return null;
			 
			


			// ** CREATE PLAN_SPLINES **

			if (planPaths != null && planPaths.Count > 0)
			{
				planSplines = new Spline[planPaths.Count];


				if (planSplines != null)
				{
					for (int i=0; i<planSplines.Length; i++)
					{
						planSplines[i] = new Spline(planPaths[i], (P_Plan.shapeState == ShapeState.Closed) ? true : false);
					}
				}
			}






			//Debug.Log("controlVertices="+ planSplines[0].controlVertices.Count);









			// CORNER_MESH
			GameObject cornerPlugGO 				= null;
			if (cornerSrc_p != null)
			{
				cornerSrc_po 		= cornerSrc_p.parametricObject;
				if (makeGameObjects && ! parametricObject.combineMeshes)
					cornerPlugGO 	= cornerSrc_po.generator.generate(true,  initiator_po,  renderToOutputParameter);
			}	




			// NODE_MESH

			GameObject nodePlugGO 					= null;
			if (nodeSrc_p != null)
			{
				nodeSrc_po 			= nodeSrc_p.parametricObject;
				//Debug.Log("yo makeGameObjects="+makeGameObjects+", parametricObject.combineMeshes="+parametricObject.combineMeshes);
				if (makeGameObjects && ! parametricObject.combineMeshes)
				{
					nodePlugGO 		= nodeSrc_po.generator.generate(true,  initiator_po,  renderToOutputParameter);
				}
			}	

			


			// CELL_MESH

			GameObject cellPlugGO 					= null;
			if (cellSrc_p != null)
			{
				cellSrc_po 			= cellSrc_p.parametricObject;
				if (makeGameObjects && ! parametricObject.combineMeshes)
					cellPlugGO 		= cellSrc_po.generator.generate(true,  initiator_po,  renderToOutputParameter);
			}	
		

			//  INSET PLANSWEEPS
			GameObject plansweepGO = null;

			
						
			
			

			// Plan
			// This is the main spline used for the iteration
				
						


			//AXParameter plan_p = ip.DependsOn;

				
	
			// The section spline is used to provide the connective tissue around corners

			// SECTION

			// The plan may have multiple paths. Each may generate a separate GO.

			sectionSrc_p	= P_Section.DependsOn;//getUpstreamSourceParameter(P_Section);
			sectionSrc_po 	= (sectionSrc_p != null ) ? sectionSrc_p.parametricObject : null;

			if (sectionSrc_po != null )
			{
				P_Section.polyTree = null;
				AXShape.thickenAndOffset(ref P_Section, sectionSrc_p);
				if (P_Section.reverse)
					P_Section.doReverse();

			}

			   

			float bay_U 		= parametricObject.floatValue("bay_U");
			float bay_V 		= parametricObject.floatValue("bay_V");
			
			if (bay_U == 0) 
				bay_U = .1f;
			if (bay_V == 0) 
				bay_V = 10f;
			
			float margin_U 		= parametricObject.floatValue("margin_U");
			


			Vector2 prevV 		= new Vector2();

			//Vector3 scaler1 	= Vector3.one;
			

			//float margin = 2.5f;

			Vector2 firstPM 	= new Vector2();
			Vector2 prevVM 		= new Vector2();
			

			
			AXSpline secSpline = null ;
			AXSplineExtrudeGenerator tmpEXSP = null;
			
			if (sectionSrc_p != null && sectionSrc_p.spline != null && sectionSrc_p.spline.vertCount > 0)
				secSpline = sectionSrc_p.spline;
			

			
			
			tmpEXSP = new AXSplineExtrudeGenerator();
			
			Material tmp_mat 	= parametricObject.axMat.mat;
			if (parametricObject.axTex != null)
			{
				tmpEXSP.uScale		=  parametricObject.axTex.scale.x;
				tmpEXSP.vScale		=  parametricObject.axTex.scale.y;
				tmpEXSP.uShift 		= -parametricObject.axTex.shift.x;
				tmpEXSP.vShift 		=  parametricObject.axTex.shift.y;
			}
		
				
			
			
			
			
			
			
			float runningU = 0;

			 

			//ip.spline.getAnglesArray();

			//Spline sectionSpline = null;
			//	= new Spline(sectionPath, sectionIsClosed,  sec_p.breakGeom, sec_p.breakNorm);



			GameObject go 			= null;
			GameObject planGO 		= null;
			if (makeGameObjects && ! parametricObject.combineMeshes)
				go = ArchimatixUtils.createAXGameObject(parametricObject.Name, parametricObject);


	


			// BOUNDING

			List<AXMesh> boundingMeshes = new List<AXMesh>();






			// FOR EACH PATH
			for (int path_i=0; path_i<planPaths.Count; path_i++)
			{

				if (makeGameObjects && ! parametricObject.combineMeshes)
					planGO = ArchimatixUtils.createAXGameObject(parametricObject.Name+"_"+path_i, parametricObject);



				// **** PREPARE EACH SPLINE *** //

				Spline planSpline = planSplines[path_i];

				planSpline.breakAngleCorners = cornerBreakAngle;
				planSpline.shapeState = P_Plan.shapeState;








				// Create subsplines between each break point
				planSpline.getSmoothSubsplineIndicies(0, maxSegmentLength);


				// ....... DEBUG
				//Pather.printPaths(planPaths);


				planSpline.createGroupedCornerNodes_NearBreakAngleVertices(inset*2);

				if (planSpline.groupedCornerNodes == null || planSpline.groupedCornerNodes.Count == 0)
					inset = 0;



				// **** PREPARE EACH SPLINE *** //



				Matrix4x4 localPlacement_mx =  Matrix4x4.identity;




				//  INSET PLANSWEEPS


				if (sectionSrc_p != null && inset > 0 && planSpline.insetSplines != null)
				{
					
						// convert planSpline.insetSplines to Paths
						Paths insetPaths = AXGeometry.Utilities.Splines2Paths(planSpline.insetSplines);

						AXParameter tmpPlanP 		= new AXParameter();
						tmpPlanP.parametricObject 	= parametricObject;
						tmpPlanP.paths 				= insetPaths;
						tmpPlanP.Type 				= AXParameter.DataType.Shape;
						tmpPlanP.shapeState 		= ShapeState.Open;
						tmpPlanP.breakGeom			= 0;

						if (planSpline.insetSplines.Count == 1)
							tmpPlanP.shapeState 		= planSpline.insetSplines[0].shapeState;

						topCap = false;
						botCap = false;

					plansweepGO = generateFirstPass(initiator_po, makeGameObjects, tmpPlanP, P_Section, Matrix4x4.identity, renderToOutputParameter, false);




						if (makeGameObjects && ! parametricObject.combineMeshes && plansweepGO != null)
							plansweepGO.transform.parent = go.transform;
				}



				AXMesh tmpMesh 				= null;








				if (planSpline.insetSpanSplines != null && planSpline.insetSpanSplines.Count > 0)
				{
					//Debug.Log(".....????>> spanSpline.subsplines.Count="+ planSpline.insetSpanSplines.Count + " --- " + planSpline.subsplines.Count );
					for (int si=0; si < planSpline.insetSpanSplines.Count; si++)
					{
						planSpline.insetSpanSplines[si].setRepeaterTransforms(si, inset, bay);
					}

					
					// SPAN NODES - MESHES ALONG SUBSPLINES
					if (nodeSrc_p != null && nodeSrc_p.meshes != null)
					{
						
						//int endCount = (P_Plan != null && P_Plan.shapeState == ShapeState.Open) ?  planSpline.subsplines.Count-1 : planSpline.subsplines.Count;
						int endCount = (P_Plan != null && P_Plan.shapeState == ShapeState.Open) ?  planSpline.insetSpanSplines.Count-1 : planSpline.insetSpanSplines.Count;

						for (int i=0; i<endCount; i++)
						{
							//SubsplineIndices rsi = planSpline.subsplines[i];
							Spline spanSpline = planSpline.insetSpanSplines[i];


							//Debug.Log(i + "||||||||||> " + spanSpline.toString());

							List<Matrix4x4> nodeMatrices = spanSpline.repeaterNodeTransforms;


							// on each of these nodes, place a nodePlug instance.
							bool spanNodesAtBreakCorners = true;

							int starter = (inset > 0 || spanNodesAtBreakCorners) ? 0 : 1;
							//Debug.Log("starter="+starter);

							int ender 	= (inset > 0 || spanSpline.shapeState == ShapeState.Open || planSpline.subsplines.Count == 1) ? nodeMatrices.Count : nodeMatrices.Count-1; 


							//Debug.Log("starter="+starter +", ender="+ender + ", nodeMatrices.Count=" + nodeMatrices.Count);

							//Debug.Log("(inset > 0 && spanNodesAtBreakCorners)="+(inset > 0 && spanNodesAtBreakCorners)+", nodeMatrices.Length="+nodeMatrices.Length+", starter="+starter+", ender="+ender);

							string this_address = "";

							if (nodeMatrices != null) 
							{
								for(int ii=starter; ii<ender; ii++)
								{
									this_address = "node_"+path_i+"_"+i+"_"+ii;

									//Debug.Log("this_address="+this_address);
									// LOCAL_PLACEMENT

									localPlacement_mx = localMatrixFromAddress(RepeaterItem.Node, path_i, i, ii);


									// AX_MESHES

									for (int mi = 0; mi < nodeSrc_p.meshes.Count; mi++) {
										AXMesh dep_amesh = nodeSrc_p.meshes [mi];
										tmpMesh = dep_amesh.Clone (localPlacement_mx * dep_amesh.transMatrix);
										tmpMesh.subItemAddress = this_address;
										ax_meshes.Add (tmpMesh);
									}



									// BOUNDING
									boundingMeshes.Add(new AXMesh(nodeSrc_po.boundsMesh, localPlacement_mx * nodeSrc_po.generator.localMatrix));

									//Debug.Log("boundingMeshes: " + boundingMeshes.Count);

									// GAME_OBJECTS

									if (nodePlugGO != null  && makeGameObjects && ! parametricObject.combineMeshes)
									{
										
										Matrix4x4  mx 		=   localPlacement_mx * nodeSrc_po.generator.localMatrixWithAxisRotationAndAlignment;
										GameObject copyGO 	= (GameObject) GameObject.Instantiate(nodePlugGO, AXUtilities.GetPosition(mx), AXUtilities.QuaternionFromMatrix(mx));


										Vector3 newJitterScale = jitterScale + Vector3.one;
										copyGO.transform.localScale= new Vector3(copyGO.transform.localScale.x*newJitterScale.x, copyGO.transform.localScale.y*newJitterScale.y, copyGO.transform.localScale.z*newJitterScale.z);
										copyGO.transform.localScale += jitterScale;

										AXGameObject axgo = copyGO.GetComponent<AXGameObject>();


										if (axgo != null)
										{
											axgo.consumerAddress = this_address;
											//Debug.Log("ADD ADDRESS: " + this_address);
										}
										copyGO.name = copyGO.name+"_" + this_address;

										//Debug.Log("copyGO.name = "+copyGO.name);

										copyGO.transform.parent = planGO.transform;
									}
								}
							}
						}
					} // \NODE MESHES



					// CELL NODES - MESHES ALONG SUBSPLINES
					if (cellSrc_p != null && cellSrc_p.meshes != null)
					{
						//int endCount = (P_Plan != null && P_Plan.shapeState == ShapeState.Open) ?  planSpline.subsplines.Count-1 : planSpline.subsplines.Count;
						int endCount = (P_Plan != null && P_Plan.shapeState == ShapeState.Open) ?  planSpline.insetSpanSplines.Count-1 : planSpline.insetSpanSplines.Count;

						for (int i=0; i<endCount; i++)
						{
							//SubsplineIndices rsi = planSpline.subsplines[i];
							Spline spanSpline = planSpline.insetSpanSplines[i];

							spanSpline.setRepeaterTransforms(i, inset, bay);


							List<Matrix4x4> cellMatrices = spanSpline.repeaterCellTransforms;



							// on each of these cell, place a nodePlug instance.
							//bool spanNodesAtBreakCorners = true;

							int starter = 0;//spanNodesAtBreakCorners ? 0 : 1;


							//int ender 	= (inset > 0 || spanNodesAtBreakCorners) ? cellMatrices.Count : cellMatrices.Count-1; 
							int ender 	= cellMatrices.Count; 

							//Debug.Log("(inset > 0 && spanNodesAtBreakCorners)="+(inset > 0 && spanNodesAtBreakCorners)+", nodeMatrices.Length="+nodeMatrices.Length+", starter="+starter+", ender="+ender);

							string this_address = "";

							if (cellMatrices != null) 
							{
								for(int ii=starter; ii<ender; ii++)
								{

									// ADDRESS

									this_address = "cell_"+path_i+"_"+i+"_"+ii;

									
									// LOCAL_PLACEMENT

									localPlacement_mx = localMatrixFromAddress(RepeaterItem.Cell, path_i, i, ii);



									// AX_MESHES
	
									for (int mi = 0; mi < cellSrc_p.meshes.Count; mi++) {
										AXMesh dep_amesh = cellSrc_p.meshes [mi];
										tmpMesh = dep_amesh.Clone (localPlacement_mx * dep_amesh.transMatrix);
										tmpMesh.subItemAddress = this_address;
										ax_meshes.Add (tmpMesh);
									}



									// BOUNDING
									boundingMeshes.Add(new AXMesh(cellSrc_po.boundsMesh, localPlacement_mx * cellSrc_po.generator.localMatrix));


									// GAME_OBJECTS

									if (cellPlugGO != null  && makeGameObjects && ! parametricObject.combineMeshes)
									{
										Matrix4x4  mx 		=   localPlacement_mx * cellSrc_po.generator.localMatrixWithAxisRotationAndAlignment;
										GameObject copyGO 	= (GameObject) GameObject.Instantiate(cellPlugGO, AXUtilities.GetPosition(mx), AXUtilities.QuaternionFromMatrix(mx));


										Vector3 newJitterScale = jitterScale + Vector3.one;
										copyGO.transform.localScale= new Vector3(copyGO.transform.localScale.x*newJitterScale.x, copyGO.transform.localScale.y*newJitterScale.y, copyGO.transform.localScale.z*newJitterScale.z);
										copyGO.transform.localScale += jitterScale;

										AXGameObject axgo = copyGO.GetComponent<AXGameObject>();


										if (axgo != null)
										{
											axgo.consumerAddress = this_address;
											//Debug.Log("ADD ADDRESS: " + this_address);
										}
										copyGO.name = copyGO.name+"_";// + this_address;

										//Debug.Log("copyGO.name = "+copyGO.name);

										copyGO.transform.parent = planGO.transform;
									}
								}
							}
						}
					} // \CELL MESHES


				} // \each spanSpline






				// BREAK CORNER MESHES

				if (cornerSrc_p != null && cornerSrc_p.meshes != null)
				{

					for(int bi=0; bi<planSpline.breakIndices.Count; bi++) {

						// ADDRESS

						string this_address = "corner_"+path_i+"_"+planSpline.breakIndices[bi];

						// LOCAL_PLACEMENT

						localPlacement_mx = localMatrixFromAddress(RepeaterItem.Corner, path_i, planSpline.breakIndices[bi]);


						// AX_MESHES

						for (int mi = 0; mi < cornerSrc_p.meshes.Count; mi++) {
							AXMesh dep_amesh = cornerSrc_p.meshes [mi];
							tmpMesh = dep_amesh.Clone (localPlacement_mx * dep_amesh.transMatrix);
							tmpMesh.subItemAddress = this_address;
							ax_meshes.Add (tmpMesh);
						}
						


						// BOUNDING
						boundingMeshes.Add(new AXMesh(cornerSrc_po.boundsMesh, localPlacement_mx * cornerSrc_po.generator.localMatrix));




						// GAME_OBJECTS

						if (cornerPlugGO != null  && makeGameObjects && ! parametricObject.combineMeshes)
						{
							Matrix4x4  mx 		=   localPlacement_mx * cornerSrc_po.generator.localMatrixWithAxisRotationAndAlignment;
							GameObject copyGO 	= (GameObject) GameObject.Instantiate(cornerPlugGO, AXUtilities.GetPosition(mx), AXUtilities.QuaternionFromMatrix(mx));


							Vector3 newJitterScale = jitterScale + Vector3.one;
							copyGO.transform.localScale= new Vector3(copyGO.transform.localScale.x*newJitterScale.x, copyGO.transform.localScale.y*newJitterScale.y, copyGO.transform.localScale.z*newJitterScale.z);
							copyGO.transform.localScale += jitterScale;

							AXGameObject axgo = copyGO.GetComponent<AXGameObject>();
							if (axgo != null)
								axgo.consumerAddress = this_address;

							copyGO.name = copyGO.name+"_" + this_address;
							copyGO.transform.parent = planGO.transform;
						}
					}
					

				}









					



					/*
					int cells = Mathf.CeilToInt( md/bay_U );
					int nby = Mathf.CeilToInt( height/bay_V );
					
					//Debug.Log ("d="+d+", md="+md);
					
					float actual_bayx = md/cells;
					float actual_bayy = height/nby;

					// CREATE VERSIONS OF INPUT BAY_CENTER MESH(ES) USING BOUNDING BOX
					// ******************************************************************************
					// someting similar shoud be added to griditerator, stepiterator, and where else?
					
					// instead of prev, would be better to create a dictionary of bounds and AXMeshes?
					if (bay_span_p != null && (prevActuralBayx != actual_bayx || prevActuralBayx != actual_bayy))
					{
						// regnerate the input meshes using actual_bayx
						bay_span_p.Parent.setParameterValueByName("uScale", parametricObject.floatValue("uScale"));
						bay_span_p.Parent.setParameterValueByName("vScale", parametricObject.floatValue("vScale"));
						bay_span_p.Parent.setParameterValueByName("uShift", parametricObject.floatValue("uShift"));
						bay_span_p.Parent.setParameterValueByName("vShift", parametricObject.floatValue("vShift"));
						
						//bay_span_p.Parent.generateOutputNow (makeGameObjects, parametricObject, true);//, new Vector3(actual_bayx, actual_bayy, margin_th));
						
					}
					// ******************************************************************************
					
					
					// CREATE VERSIONS OF INPUT BAY_CENTER MESH(ES) USING BOUNDING BOX
					// ******************************************************************************
					// someting similar shoud be added to griditerator, stepiterator, and where else?
					
					// instead of prev, would be better to create a dictionary of bounds and AXMeshes?
					if (node_p != null && (prevActuralBayx != actual_bayx || prevActuralBayx != actual_bayy))
					{
						// regnerate the input meshes using actual_bayx
						node_p.Parent.setParameterValueByName("uScale", parametricObject.floatValue("uScale"));
						node_p.Parent.setParameterValueByName("vScale", parametricObject.floatValue("vScale"));
						node_p.Parent.setParameterValueByName("uShift", parametricObject.floatValue("uShift"));
						node_p.Parent.setParameterValueByName("vShift", parametricObject.floatValue("vShift"));
						
						//node_p.Parent.generateOutputNow (makeGameObjects, parametricObject, true);//, new Vector3(actual_bayx, actual_bayy, margin_th));
						
					}
					// ******************************************************************************
					*/
					


					
					
					



				
				
				//margin_U = .5f;
				if (margin_U > 0 && planSpline.isClosed)
				{
					AXSpline connSpline2 = new AXSpline();
					connSpline2.Push(prevVM);
					connSpline2.Push(prevV);
					connSpline2.Push(firstPM);
					connSpline2.isClosed = false;
					
					tmpEXSP.uShift = parametricObject.floatValue("uShift") +(runningU)/tmpEXSP.uScale;
					
					Mesh mesh = tmpEXSP.generate(connSpline2, secSpline);
					ax_meshes.Add (new AXMesh( mesh, Matrix4x4.identity, tmp_mat));
				}
				
				
				if (makeGameObjects && ! parametricObject.combineMeshes)
						planGO.transform.parent = go.transform;

			}// end paths




			//GameObject.DestroyImmediate(bay_spanPlugGO);
			GameObject.DestroyImmediate(cornerPlugGO);
			GameObject.DestroyImmediate(nodePlugGO);
			GameObject.DestroyImmediate(cellPlugGO);

			if (makeGameObjects &&  parametricObject.combineMeshes)
				GameObject.DestroyImmediate(plansweepGO);

			

			
			

			
			// FINISH AX_MESHES
			//Debug.Log("finish " + ax_meshes.Count);
			parametricObject.finishMultiAXMeshAndOutput(ax_meshes, renderToOutputParameter);


			// FINISH BOUNDS

			CombineInstance[] boundsCombinator = new CombineInstance[boundingMeshes.Count];
			for(int bb=0; bb<boundsCombinator.Length; bb++)
			{
				boundsCombinator[bb].mesh 		= boundingMeshes[bb].mesh;
				boundsCombinator[bb].transform 	= boundingMeshes[bb].transMatrix;
			}
			setBoundsWithCombinator(boundsCombinator);




			// FINISH GAME_OBJECTS

			if (makeGameObjects)
			{
				if (parametricObject.combineMeshes)
				{
					go = parametricObject.makeGameObjectsFromAXMeshes(ax_meshes, true);
				}

				Matrix4x4 tmx = parametricObject.getLocalMatrix();

				go.transform.rotation 		= AXUtilities.QuaternionFromMatrix(tmx);
				go.transform.position 		= AXUtilities.GetPosition(tmx);
				go.transform.localScale 	= parametricObject.getLocalScaleAxisRotated();//AXUtilities.GetScale(tmx);

				if (P_Plan.reverse)
					P_Plan.doReverse();

				if (P_Section != null && P_Section.reverse)
					P_Section.doReverse();

				return go;
			}
			else
			{
				// Submit AXMeshes for realtime rendering
				//parametricObject.finishMultiAXMeshAndOutput(ax_meshes, renderToOutputParameter);
				//setBoundaryFromMeshes(ax_meshes);
			}			
		


			if (P_Plan.reverse)
				P_Plan.doReverse();

			if (P_Section != null && P_Section.reverse)
				P_Section.doReverse();

			return null;
		}





	
	/*
		public override Matrix4x4 getLocalConsumerMatrixPerInputSocket(AXParametricObject input, AXParameter input_p=null)
		{
			Debug.Log("planSrc_po="+planSrc_po);
			if (planSrc_po != null)
			{
				//Debug.Log(input.Name + " -- " + planSrc_po.Name);
				if (planSrc_po!= null && input == planSrc_po)
					return Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90, 0, 0), Vector3.one);
				
				//return Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(-90, 0, 0), Vector3.one);
			}
			return Matrix4x4.identity;
		}
	
	
		// GET_LOCAL_CONSUMER_MATRIX_PER_INPUT_SOCKET
		public override Matrix4x4 getLocalConsumerMatrixPerInputSocket(AXParametricObject input_po)
		{
			//Debug.Log ("input_po.Name=" + input_po.Name);// + " -- " + endCapHandleTransform);
			// PLAN
			if (input_po == planSrc_po)
				return  Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90, 0, 0), Vector3.one);

			
			// SECTION
			if (input_po == sectionSrc_po)
				return sectionHandleTransform * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90, 0, 0), Vector3.one);

			// ENDCAP
			if (P_EndCapMesh != null && P_EndCapMesh.DependsOn != null && input_po == P_EndCapMesh.DependsOn.Parent)
				return endCapHandleTransform;
							
			return Matrix4x4.identity ;
		}
	*/

		public override Matrix4x4 getLocalConsumerMatrixPerInputSocket(AXParametricObject input_po, AXParameter input_P)
		{
			// use shift too
			// use caller address
			if (input_po == null)
				return Matrix4x4.identity;


            bool addressIsNull = (input_po.selectedConsumer == null || input_po.selectedConsumer != this.parametricObject || String.IsNullOrEmpty(input_po.selectedConsumerAddress));


			if (input_P != null)
			{
				if (input_P.Name == "Plan")
                {
                     return Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90, 0, 0), Vector3.one);
                }

                if (input_P.Name == "Section")
				{
					//Debug.Log("the section      ****");
					//Debug.Log(sectionHandleTransform);
					return sectionHandleTransform * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90, 0, 0), Vector3.one);
				}
			}


			// PLAN
			if (input_po == planSrc_po)
				return  Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90, 0, 0), Vector3.one);

			// SECTION
			if (input_po == sectionSrc_po)
			{
				
				return sectionHandleTransform * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90, 0, 0), Vector3.one);
			}

			// PARSE ADDRESS
			Matrix4x4 optionalLocalM = Matrix4x4.identity;

			if (addressIsNull)
			{
                

                // set default address
                if (nodeSrc_po != null && input_po == nodeSrc_po)
					input_po.selectedConsumerAddress = "node_0_0_0";
				else if (cellSrc_po != null && input_po == cellSrc_po)
					input_po.selectedConsumerAddress = "cell_0_0_0";
				else if (cornerSrc_po != null && input_po == cornerSrc_po)
					input_po.selectedConsumerAddress = "corner_0_0";

				optionalLocalM = input_po.getLocalMatrix() * input_po.getAxisRotationMatrix().inverse;
			}


			addressIsNull = (input_po.selectedConsumer == null || input_po.selectedConsumer != this.parametricObject || String.IsNullOrEmpty(input_po.selectedConsumerAddress));

			if (! addressIsNull)
			{
                
				string[] address = input_po.selectedConsumerAddress.Split('_');

				if (address.Length < 3)
					return Matrix4x4.identity;

				string meshInput = address[0]; // e.g., "node", "cell", "spanU", "spanV", "corner"


				// Find out which input this caller is fed into Node Mesh, Bay Span or Cell Mesh?
				//Debug.Log(input_po.selectedConsumerAddress);

				// CORNER
				if (meshInput == "corner" && cornerSrc_po != null && input_po == cornerSrc_po)
				{
					int path_i = int.Parse(address[1]);
					int vert_i = int.Parse(address[2]);
					return localMatrixFromAddress(RepeaterItem.Corner, path_i, vert_i) * optionalLocalM;
				}

				if (meshInput == "node" && nodeSrc_po != null && input_po == nodeSrc_po && address.Length == 4)
				{
					int path_i 		= int.Parse(address[1]);
					int subspline_i = int.Parse(address[2]);
					int node_i 		= int.Parse(address[3]);

					return localMatrixFromAddress(RepeaterItem.Node, path_i, subspline_i, node_i) * optionalLocalM;

				}

				if (meshInput == "cell" && cellSrc_po != null && input_po == cellSrc_po && address.Length == 4)
				{
					int path_i 		= int.Parse(address[1]);
					int subspline_i = int.Parse(address[2]);
					int node_i 		= int.Parse(address[3]);

					return localMatrixFromAddress(RepeaterItem.Cell, path_i, subspline_i, node_i) * optionalLocalM;

				}
			}

           
            return Matrix4x4.identity;
		} 

		 
		  
		public virtual Matrix4x4 localMatrixFromAddress(RepeaterItem ri, int i=0, int j=0, int k=0)
		{

			Matrix4x4 retMatrix = Matrix4x4.identity;

			if (planSplines == null || i > planSplines.Length-1)
				return retMatrix;

			Spline planSpline = planSplines[i];

			if (planSpline == null || planSpline.controlVertices == null || j > planSpline.controlVertices.Count-1)
				return retMatrix;

			float x = planSpline.controlVertices[j].x;
			float y = planSpline.controlVertices[j].y;

			//Quaternion bisectorRot = Quaternion.identity;

			float rot_angX = 0;
			float rot_angY = 0;
			float rot_angZ = 0;


			switch (ri)
			{
			case RepeaterItem.Corner:
				if (planSpline.controlVertices.Count > j)
				{
						

					x = planSpline.controlVertices[j].x;
					y = planSpline.controlVertices[j].y;

					rot_angY = 0;

					if (cornerAngled)
					{

						if (j == 0 && ! planSpline.isClosed && planSpline.insetSpanSplines != null && planSpline.insetSpanSplines.Count > j && planSpline.insetSpanSplines[j] != null && planSpline.insetSpanSplines[j].repeaterNodeRotations != null && planSpline.insetSpanSplines[j].repeaterNodeRotations.Count > 0)
						{
							rot_angY = planSpline.insetSpanSplines[j].repeaterNodeRotations[0];
						}
						else if (planSpline.insetSpanSplines != null 
                        && j == planSpline.insetSpanSplines.Count-1
                        && (j-1) >= 0
                        && planSpline.insetSpanSplines[j - 1] != null
                         
                        && planSpline.insetSpanSplines[j - 1].repeaterNodeRotations != null
                        && planSpline.insetSpanSplines[j - 1].repeaterNodeRotations.Count > 0
                            && ! planSpline.isClosed)
						{
							rot_angY = planSpline.insetSpanSplines[j-1].repeaterNodeRotations[0];
						}
						else
						{
							Vector2 v = planSpline.controlVertices[j];
							
							Vector2 pv = planSpline.previousPoint(j);
							Vector2 nv = planSpline.nextPoint(j);

							Vector2 pseg = v-pv;
							Vector2 nseg = nv-v;

							Vector2 bisector = pseg.normalized + nseg.normalized;
							rot_angY = -Mathf.Atan2(bisector.y, bisector.x)*Mathf.Rad2Deg;
						}
					}

					//bisectorRot =     Quaternion.Euler(	0, planSpline.bevelAngles[vert_i], 0 ); 
				}
				break;



			case RepeaterItem.Node:

				if (planSplines != null && planSplines[i] != null && planSplines[i].insetSpanSplines != null && planSplines[i].insetSpanSplines.Count > j && planSplines[i].insetSpanSplines.Count > j && planSplines[i].insetSpanSplines[j].repeaterNodeTransforms.Count>k)
				{
					Vector2 pos = planSplines[i].insetSpanSplines[j].repeaterNodePositions[k];

					x = pos.x;
					y = pos.y;

					rot_angX = k * progressiveRotationX;
					rot_angY = planSplines[i].insetSpanSplines[j].repeaterNodeRotations[k] + k * progressiveRotationY;
					rot_angZ = k * progressiveRotationZ;

				}


				break;

			case RepeaterItem.Cell:
				if (planSplines != null && planSplines[i] != null && planSplines[i].insetSpanSplines != null && planSplines[i].insetSpanSplines.Count > j && planSplines[i].insetSpanSplines[j].repeaterCellPositions.Count>k)
				{
					Vector2 cellpos = planSplines[i].insetSpanSplines[j].repeaterCellPositions[k];
					x = cellpos.x;
					y = cellpos.y;

					rot_angY = planSplines[i].insetSpanSplines[j].repeaterCellRotations[k];
				}
				//if (planSplines[i].spanSplines[j].repeaterCellTransforms.Count>k)
				//	return planSplines[i].spanSplines[j].repeaterCellTransforms[k];


				break;

			}


			//bisectorRot =     Quaternion.Euler(	0, rot_ang, 0 ); 


			float perlinTranslation 	= 1;
			float perlinRot 			= 1;
			float perlinScale 			= 1;



			//shiftU = -repeaterToolU.cells * actualBayU / 2;
			//shiftV = -repeaterToolV.cells * actualBayV / 2;








			if (jitterTranslationTool != null)
				perlinTranslation 	=  Mathf.PerlinNoise( (x+jitterTranslationTool.offset) * jitterTranslationTool.perlinScale,  	y * jitterTranslationTool.perlinScale);

			if (jitterRotationTool != null)
				perlinRot 			=  Mathf.PerlinNoise( (x+jitterRotationTool.offset) * jitterRotationTool.perlinScale,  		y * jitterRotationTool.perlinScale);

			if (jitterScaleTool != null)
				perlinScale 		=  Mathf.PerlinNoise( (x+jitterScaleTool.offset) * jitterScaleTool.perlinScale,  		y * jitterScaleTool.perlinScale);
				 


			// TRANSLATION 	*********
			Vector3 translate = new Vector3(x, 0, y);

			Terrain terrain = parametricObject.terrain;
			if (terrain != null)
				translate.y += terrain.SampleHeight(parametricObject.model.gameObject.transform.TransformPoint(parametricObject.localMatrix.MultiplyPoint(translate)));

			if (jitterTranslationTool != null)
				translate += new Vector3( perlinTranslation*jitterTranslationTool.x-jitterTranslationTool.x/2 , perlinTranslation*jitterTranslationTool.y-jitterTranslationTool.y/2, perlinTranslation*jitterTranslationTool.z-jitterTranslationTool.z/2);

		
			// ROTATION		*********
			Quaternion 	rotation = Quaternion.Euler (rot_angX, rot_angY, rot_angZ);
			if (jitterRotationTool != null)
				rotation = Quaternion.Euler ((perlinRot*jitterRotationTool.x - jitterRotationTool.x/2)*1.5f, (perlinRot*jitterRotationTool.y - jitterRotationTool.y/2), (perlinRot*jitterRotationTool.z - jitterRotationTool.z/2)) * rotation;


			// SCALE		**********
			jitterScale = Vector3.zero;
			if (jitterScaleTool != null)
				jitterScale = new Vector3( perlinScale*jitterScaleTool.x-jitterScaleTool.x/2 , perlinScale*jitterScaleTool.y-jitterScaleTool.y/2, 0);
			

			// ******** USE A FUNCTION HERE TO GET FOR HANDLES AS WELL
			return   Matrix4x4.TRS(translate, rotation, Vector3.one+jitterScale);

		}


		 		 		 		 
	}

}