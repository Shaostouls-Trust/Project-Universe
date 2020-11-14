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

	public class PlanRepeater2D : Repeater2D, IShape
	{

		public override string GeneratorHandlerTypeName { get { return "PlanRepeater2DHandler"; } }



		// INPUTS
		public AXParameter 			P_Plan;
		public AXParameter 			planSrc_p;
		public AXParametricObject 	planSrc_po;



		// WORKING FIELDS (Updated every Generate)
		// CORNER
		public AXParameter			P_Corner;
		public AXParameter 			cornerSrc_p;
		public AXParametricObject 	cornerSrc_po;


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


		public List<SubsplineIndices[]>	paths_SubsplineIndices;


		public List<Matrix4x4> 		spanNodeTransforms;




		// INITIALIZE
		public override void init_parametricObject() 
		{
			base.init_parametricObject();

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, 		AXParameter.ParameterType.Input, "Plan"));

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, AXParameter.ParameterType.Input, "Corner Shape"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, AXParameter.ParameterType.Input, "Node Shape"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, AXParameter.ParameterType.Input, "Cell Shape"));

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.JitterTool,   AXParameter.ParameterType.Input, "Jitter Translation"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.JitterTool,   AXParameter.ParameterType.Input, "Jitter Rotation"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.JitterTool,   AXParameter.ParameterType.Input, "Jitter Scale"));


			parametricObject.addParameter(AXParameter.DataType.Float, 	"Inset", 		1, 	 0f, 100f);
			parametricObject.addParameter(AXParameter.DataType.Float, 	"Max Segment Length", 		25, 	 1f, 1000f);
			parametricObject.addParameter(AXParameter.DataType.Float, 	"Bay Width", 			2,	.01f, 100f);
			parametricObject.addParameter(AXParameter.DataType.Float, 	"Corner Break Angle", 			10,	.01f, 180f);



			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, AXParameter.ParameterType.Output, "Output Shape"));

			parametricObject.addParameter(AXParameter.DataType.Float, "Progressive Rotation", 0f); 
		}




		// POLL INPUTS (only on graph change())
		public override void pollInputParmetersAndSetUpLocalReferences()
		{
			//Debug.Log("Repeater::pollInputParmetersAndSetUpLocalReferences " + parametricObject.Name + " parametersHaveBeenPolled="+parametersHaveBeenPolled);

			base.pollInputParmetersAndSetUpLocalReferences();

			P_Plan 					= parametricObject.getParameter("Plan");


			P_Corner 				= parametricObject.getParameter("Corner Shape");
			P_Node 					= parametricObject.getParameter("Node Shape");
			P_Cell 					= parametricObject.getParameter("Cell Shape");


			P_Inset 				= parametricObject.getParameter("Inset");
			P_MaxSegmentLength 		= parametricObject.getParameter("Max Segment Length");
			P_Bay 					= parametricObject.getParameter("Bay Width", "Bay");
			P_CornerBreakAngle 		= parametricObject.getParameter("Corner Break Angle");


			pollControlValuesFromParmeters();
			 
		}



		// POLL CONTROLS (every model.generate())
		public override void pollControlValuesFromParmeters() 
		{
			base.pollControlValuesFromParmeters();

			cornerSrc_p 	= (P_Corner 	!= null) 	? getUpstreamSourceParameter(P_Corner)		: null;
			cornerSrc_po 	= (cornerSrc_p 	!= null) 	? cornerSrc_p.parametricObject				: null;

			nodeSrc_p 		= (P_Node 		!= null) 	? getUpstreamSourceParameter(P_Node)		: null;
			nodeSrc_po 		= (nodeSrc_p	!= null) 	? nodeSrc_p.parametricObject				: null;

			cellSrc_p 		= (P_Cell		!= null) 	? getUpstreamSourceParameter(P_Cell)		: null;
			cellSrc_po 		= (cellSrc_p	!= null) 	? cellSrc_p.parametricObject				: null;


			inset 				= (P_Inset != null) 			? P_Inset.FloatVal : 0;
			maxSegmentLength 	= (P_MaxSegmentLength != null) 	? P_MaxSegmentLength.FloatVal : 0;
			bay 				= (P_Bay != null) 				? P_Bay.FloatVal : 2f;

			cornerBreakAngle 		= (P_CornerBreakAngle != null) 	? P_CornerBreakAngle.FloatVal : 0;

		}







		public override void connectionMadeWith(AXParameter to_p, AXParameter from_p)
		{

			if (from_p == null)// || from_p.DependsOn == null)
				return;


			AXParameter this_p = (to_p.parametricObject == parametricObject) ? to_p   : from_p;
			AXParameter src_p  = (to_p.parametricObject == parametricObject) ? from_p : to_p;

			base.connectionMadeWith(to_p, from_p);


			switch(this_p.Name) { 
			case "Node Shape":
				nodeSrc_p = src_p;
				if (! parametricObject.isInitialized)
					initializeBays(this_p.Name);
				break;
			case "Cell Shape":
				cellSrc_p = src_p;
				if (! parametricObject.isInitialized)
					initializeBays(this_p.Name);
				break;

			case "Jitter Translation":
				jitterTranslationTool = src_p.parametricObject.generator as JitterTool;
				break;

			case "Jitter Rotation":
				jitterRotationTool = src_p.parametricObject.generator as JitterTool;
				break;

			case "Jitter Scaling":
				jitterScaleTool = src_p.parametricObject.generator as JitterTool;
				break;

			
			}
		}

		/*
		public override void initializeBays(string pName)
		{
			if (parametricObject.isInitialized)
				return;

			parametricObject.isInitialized = true;

			RadialRepeaterTool gener = repeaterToolU as RadialRepeaterTool;

			switch(pName)
			{
			case "Node Shape":
				if (repeaterToolU != null)
					gener.radius = 3.5f * nodeSrc_p.parametricObject.bounds.size.x ;
				break;
			case "Cell Shape":
				if (repeaterToolU != null)
					gener.radius = 2.5f * cellSrc_p.parametricObject.bounds.size.x ;
				break;
			}
		}
		*/





		// SHAPE_REPEATER_2D :: GENERATE
		public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool isReplica)
		{

			if (parametricObject == null || ! parametricObject.isActive)
				return null;

			if ( (P_Corner == null ||  cornerSrc_p == null) &&  (P_Node == null ||  inputSrc_p == null) &&  (P_Cell == null ||  cellSrc_p == null))
			{

				if (P_Output != null)
				{
					P_Output.paths = null;
					P_Output.polyTree = null;
				}
				return null;
			}


			// PRE_GENERATE
			preGenerate();


			planSrc_p		= P_Plan.DependsOn;// getUpstreamSourceParameter(P_Plan);
			planSrc_po 		= (planSrc_p != null) 								? planSrc_p.parametricObject 	: null;


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








			AXParameter P_cornerOutput = new AXParameter ();
			P_cornerOutput.parametricObject = parametricObject;

			AXParameter P_nodeOutput = new AXParameter ();
			P_nodeOutput.parametricObject = parametricObject;


			AXParameter P_cellOutput = new AXParameter ();
			P_cellOutput.parametricObject = parametricObject;
				



			// PROCESS SHAPE INPUTS

			if (cornerSrc_p != null)
			{
				P_Corner.polyTree = null;
				AXShape.thickenAndOffset(ref P_Corner, cornerSrc_p);
			}

			if (inputSrc_p != null)
			{
				P_Node.polyTree = null;
				AXShape.thickenAndOffset(ref P_Node, inputSrc_p);
			}


			if (cellSrc_p != null)
			{
				P_Cell.polyTree = null;
				AXShape.thickenAndOffset(ref P_Cell, cellSrc_p);
			}
			 
			



			bool doPolyTreeNodes = false;
			//bool doPolyTreeCells = false;


			// NODE
			//Matrix4x4 tm = Matrix4x4.identity;




			//  NODE SHAPES
			Paths nodeSourcePaths = null;
			//Clipper nodeClipper = null;

			if (P_Node != null)
			{
				if (P_Node.polyTree != null)
				{
					nodeSourcePaths = AXGeometry.Utilities.cleanPaths(Clipper.PolyTreeToPaths(P_Node.polyTree));
					doPolyTreeNodes = true;
					//nodeClipper =  new Clipper(Clipper.ioPreserveCollinear);
				} 
				else
				{
					nodeSourcePaths = P_Node.getClonePaths();
					P_cornerOutput.paths = new Paths();
				}
			}


			// CELL SHAPES
			Paths cellSourcePaths = null;
			//Clipper cellClipper = null;

			if (P_Cell != null)
			{
				if (P_Cell.polyTree != null)
				{
					cellSourcePaths = AXGeometry.Utilities.cleanPaths(Clipper.PolyTreeToPaths(P_Cell.polyTree));
					doPolyTreeNodes = true;
					//cellClipper =  new Clipper(Clipper.ioPreserveCollinear);
				}
				else
				{
					cellSourcePaths = P_Cell.getClonePaths();
					P_cornerOutput.paths = new Paths();
				}
			}




			// BREAK CORNER SHAPES
			Paths cornerSourcePaths = null;
			Clipper theClipper = new Clipper(Clipper.ioPreserveCollinear);

			if (P_Corner != null)
			{
				if (P_Corner.polyTree != null)
				{
					cornerSourcePaths = AXGeometry.Utilities.cleanPaths(Clipper.PolyTreeToPaths(P_Corner.polyTree));
					doPolyTreeNodes = true;
				}
				else
				{
					cornerSourcePaths = P_Corner.getClonePaths();
					P_cornerOutput.paths = new Paths();
				}
			}




	

				// FOR EACH PATH

				for (int path_i=0; path_i<planPaths.Count; path_i++)
				{



					// **** PREPARE EACH SPLINE *** //

					Spline planSpline = planSplines[path_i];

					planSpline.breakAngleCorners = cornerBreakAngle;
					planSpline.shapeState = P_Plan.shapeState;

					// Create subsplines between each break point
					planSpline.getSmoothSubsplineIndicies(0, maxSegmentLength);

//					Debug.Log("planSpline.subsplines.Count="+ planSpline.subsplines.Count );
//					foreach(SubsplineIndices si in planSpline.subsplines)
//						si.print();

					planSpline.createGroupedCornerNodes_NearBreakAngleVertices(inset*2);

					// **** PREPARE EACH SPLINE *** //



					Matrix4x4 localPlacement_mx =  Matrix4x4.identity;





					if (planSpline.insetSpanSplines != null && planSpline.insetSpanSplines.Count > 0)
					{
						for (int si=0; si<planSpline.insetSpanSplines.Count; si++)
							planSpline.insetSpanSplines[si].setRepeaterTransforms(si, inset, bay);



						// SPAN NODES - SHAPES ALONG SUBSPLINES
						if (nodeSrc_p != null && nodeSrc_p.meshes != null)
						{

							int endCount = (P_Plan != null && P_Plan.shapeState == ShapeState.Open) ?  planSpline.insetSpanSplines.Count-1 : planSpline.insetSpanSplines.Count;

							for (int i=0; i<endCount; i++)
							{
								//SubsplineIndices rsi = planSpline.subsplines[i];
								Spline spanSpline = planSpline.insetSpanSplines[i];
								List<Matrix4x4> nodeMatrices = spanSpline.repeaterNodeTransforms;


								// on each of these nodes, place a nodePlug instance.
								bool spanNodesAtBreakCorners = true;

								int starter = spanNodesAtBreakCorners ? 0 : 1;
								int ender 	= (inset > 0 || spanNodesAtBreakCorners) ? nodeMatrices.Count : nodeMatrices.Count-1; 

								//string this_address = "";


								if (nodeMatrices != null) 
								{
									for(int ii=starter; ii<ender; ii++)
									{
										//this_address = "node_"+path_i+"_"+i+"_"+ii;

										//Debug.Log("this_address="+this_address);
										// LOCAL_PLACEMENT

										localPlacement_mx = localMatrixFromAddress(RepeaterItem.Node, path_i, i, ii);

										if (doPolyTreeNodes)
										{
											theClipper.AddPaths ( AX.Generators.Generator2D.transformPaths(nodeSourcePaths, localPlacement_mx), PolyType.ptSubject, true);
							 			}
										else 
										{
											Paths tmp = AX.Generators.Generator2D.transformPaths(nodeSourcePaths, localPlacement_mx);
											P_cornerOutput.paths.AddRange(tmp);
										} 
									}
								}
							}
						}



						// CELL NODES - SHAPES ALONG SUBSPLINES
						if (cellSrc_p != null && cellSrc_p.meshes != null)
						{

							int endCount = (P_Plan != null && P_Plan.shapeState == ShapeState.Open) ?  planSpline.insetSpanSplines.Count-1 : planSpline.insetSpanSplines.Count;

							for (int i=0; i<endCount; i++)
							{
								//SubsplineIndices rsi = planSpline.subsplines[i];
								Spline spanSpline 				= planSpline.insetSpanSplines[i];
								List<Matrix4x4> cellMatrices 	= spanSpline.repeaterCellTransforms;


								// on each of these nodes, place a nodePlug instance.
								bool spanNodesAtBreakCorners = true;

								int starter = spanNodesAtBreakCorners ? 0 : 1;
								int ender 	= (inset > 0 || spanNodesAtBreakCorners) ? cellMatrices.Count : cellMatrices.Count-1; 

								//string this_address = "";


								if (cellMatrices != null) 
								{
									for(int ii=starter; ii<ender; ii++)
									{
										//this_address = "cell_"+path_i+"_"+i+"_"+ii;

										//Debug.Log("this_address="+this_address);
										// LOCAL_PLACEMENT

										localPlacement_mx = localMatrixFromAddress(RepeaterItem.Cell, path_i, i, ii);

										if (doPolyTreeNodes)
										{
											theClipper.AddPaths ( AX.Generators.Generator2D.transformPaths(cellSourcePaths, localPlacement_mx), PolyType.ptSubject, true);
							 			}
										else 
										{
											Paths tmp = AX.Generators.Generator2D.transformPaths(nodeSourcePaths, localPlacement_mx);
											P_cornerOutput.paths.AddRange(tmp);
										} 
									}
								}
							}
						}





					}




				if (cornerSourcePaths != null && cornerSourcePaths.Count > 0)
				{

                    // CORNERS
                    //Debug.Log(planSpline.breakIndices.Count);
					for(int bi=0; bi<planSpline.breakIndices.Count; bi++) 
					{
                        //Debug.Log("- " + bi);

						//tm = Matrix4x4.TRS(new Vector3(2*i-2, 2*j-2, 0), Quaternion.identity, Vector3.one);
						localPlacement_mx = localMatrixFromAddress(RepeaterItem.Corner, path_i, planSpline.breakIndices[bi]);

                       


                        if (doPolyTreeNodes)
						{
							theClipper.AddPaths ( AX.Generators.Generator2D.transformPaths(cornerSourcePaths, localPlacement_mx), PolyType.ptSubject, true);
			 			}
						else 
						{
							Paths tmp = AX.Generators.Generator2D.transformPaths(cornerSourcePaths, localPlacement_mx);
							P_cornerOutput.paths.AddRange(tmp);
						} 
					}

				}




				if (doPolyTreeNodes)
				{	
					P_cornerOutput.polyTree = new AXClipperLib.PolyTree();
					theClipper.Execute(ClipType.ctDifference, 	P_cornerOutput.polyTree, 	PolyFillType.pftNonZero, PolyFillType.pftNonZero);
				}


			}





			P_Output.polyTree = null;

			if (cornerSrc_p != null  ||  cellSrc_p != null ||  nodeSrc_p != null  )
			{

				// JUST NODES
				AXShape.thickenAndOffset(ref P_Output, P_cornerOutput);
			}
			/*
			else if (nodeSrc_p != null && (cellSrc_p == null || (P_cellOutput.paths == null && P_cellOutput.polyTree == null)))
			{
				// JUST NODES
				AXShape.thickenAndOffset(ref P_Output, P_nodeOutput);
			}

			else if (nodeSrc_p == null && cellSrc_p != null)
			{
				// JUST CELLS
				AXShape.thickenAndOffset(ref P_Output, P_cellOutput);
			}
			else
			{
				// BOTH TO COMBINE
				clipper =  new Clipper(Clipper.ioPreserveCollinear);

				if (P_nodeOutput.polyTree == null)
					clipper.AddPaths(P_nodeOutput.paths, PolyType.ptSubject, true);
				else
					clipper.AddPaths(AXGeometry.Utilities.cleanPaths(Clipper.PolyTreeToPaths(P_nodeOutput.polyTree)), PolyType.ptSubject, true);
				
				if (P_cellOutput.polyTree == null)
					clipper.AddPaths(P_cellOutput.paths, PolyType.ptSubject, true);
				else
					clipper.AddPaths(AXGeometry.Utilities.cleanPaths(Clipper.PolyTreeToPaths(P_cellOutput.polyTree)), PolyType.ptSubject, true);


				P_Output.polyTree = new AXClipperLib.PolyTree();
				clipper.Execute(ClipType.ctUnion, 	P_Output.polyTree, 	PolyFillType.pftNonZero, PolyFillType.pftNonZero);

				AXShape.thickenAndOffset(ref P_Output, P_Output);
			}
			*/

			if ( P_Output.polyTree != null)
				transformPolyTree(P_Output.polyTree, localMatrix);
			else if (P_nodeOutput.paths != null)
			{
				P_Output.paths 						= transformPaths(P_nodeOutput.paths, localMatrix);
				P_Output.transformedControlPaths 	= P_nodeOutput.paths;
			}
			//base.generate(false, initiator_po, isReplica);
			calculateBounds();

			return null;


		} // \generate






		public override Matrix4x4 getLocalConsumerMatrixPerInputSocket(AXParametricObject input)
		{

			if (input == cornerSrc_po)
				return localMatrixFromAddress(RepeaterItem.Corner, 0, 0);
			
			if (input == nodeSrc_po)
				return localMatrixFromAddress(RepeaterItem.Node, 0, 0);
			
			else if (input == cellSrc_po)
				return localMatrixFromAddress(RepeaterItem.Cell, 0, 0);

			return Matrix4x4.identity;

		}




		public virtual Matrix4x4 localMatrixFromAddress(RepeaterItem ri, int i=0, int j=0, int k=0)
		{
            // i = path index
            // j = break index on path[i]


			Matrix4x4 retMatrix = Matrix4x4.identity;

			if (planSplines == null || i > planSplines.Length-1)
				return retMatrix;

			Spline planSpline = planSplines[i];

			if (planSpline == null || planSpline.controlVertices == null || j > planSpline.controlVertices.Count-1)
				return retMatrix;

			float x = planSpline.controlVertices[j].x;
			float y = planSpline.controlVertices[j].y;

			//Quaternion bisectorRot = Quaternion.identity;

			float rot_ang = 0;


			switch (ri)
			{
			case RepeaterItem.Corner:
				if (planSpline.controlVertices.Count > j)
				{
						

					x = planSpline.controlVertices[j].x;
					y = planSpline.controlVertices[j].y;


					Vector2 v = planSpline.controlVertices[j];
					
					Vector2 pv = planSpline.previousPoint(j);
					Vector2 nv = planSpline.nextPoint(j);

					Vector2 pseg = v-pv;
					Vector2 nseg = nv-v;

					Vector2 bisector = pseg.normalized + nseg.normalized;
					rot_ang = Mathf.Atan2(bisector.y, bisector.x)*Mathf.Rad2Deg;


                        //bisectorRot =     Quaternion.Euler(	0, planSpline.bevelAngles[vert_i], 0 ); 

                        //Debug.Log(j + " of " + planSpline.derivedVertices.Count);
                        if (! planSpline.isClosed)
                        {

                        
                        if (j == 0)
                        {
                            //Debug.Log("FIRST " + planSpline.perpAngleFirst + " --- " + planSpline.nodeRotations[0]);
                            rot_ang = planSpline.perpAngleFirst;

                        }
                        if (j == planSpline.derivedVertices.Count - 1)
                        {
                           //Debug.Log("LAST  " + planSpline.perpAngleLast + " --- " + planSpline.nodeRotations[planSpline.nodeRotations.Count - 1]);
                            rot_ang = planSpline.perpAngleLast;

                        }
                        }
                        //Debug.Log("rot_ang="+rot_ang);

                    }
                    break;




			case RepeaterItem.Node:

				if (planSplines != null && planSplines[i] != null && planSplines[i].insetSpanSplines != null && planSplines[i].insetSpanSplines.Count > j && planSplines[i].insetSpanSplines[j].repeaterNodeTransforms.Count>k)
				{
					Vector2 pos = planSplines[i].insetSpanSplines[j].repeaterNodePositions[k];

					x = pos.x;
					y = pos.y;

					rot_ang = -planSplines[i].insetSpanSplines[j].repeaterNodeRotations[k];

				}


				break;

			case RepeaterItem.Cell:
				if (planSplines != null && planSplines[i] != null && planSplines[i].insetSpanSplines != null && planSplines[i].insetSpanSplines.Count > j && planSplines[i].insetSpanSplines[j].repeaterCellPositions.Count>k)
				{
					Vector2 cellpos = planSplines[i].insetSpanSplines[j].repeaterCellPositions[k];
					x = cellpos.x;
					y = cellpos.y;

					rot_ang = -planSplines[i].insetSpanSplines[j].repeaterCellRotations[k];
				}
				//if (planSplines[i].spanSplines[j].repeaterCellTransforms.Count>k)
				//	return planSplines[i].spanSplines[j].repeaterCellTransforms[k];


				break;

			}



			//bisectorRot =     Quaternion.Euler(	0, rot_ang, 0 ); 


			float perlinTranslation 	= 1;
			float perlinRot 			= 1;
			float perlinScale 			= 1;



			if (jitterTranslationTool != null)
				perlinTranslation 	=  Mathf.PerlinNoise( x * jitterTranslationTool.perlinScale,  	y * jitterTranslationTool.perlinScale);

			if (jitterRotationTool != null)
				perlinRot 			=  Mathf.PerlinNoise( x * jitterRotationTool.perlinScale,  		y * jitterRotationTool.perlinScale);

			if (jitterScaleTool != null)
				perlinScale 		=  Mathf.PerlinNoise( x * jitterScaleTool.perlinScale,  		y * jitterScaleTool.perlinScale);



			// TRANSLATION 	*********
			Vector3 translate = new Vector3(x, y, 0);
			if (jitterTranslationTool != null)
				translate += new Vector3( perlinTranslation*jitterTranslationTool.x-jitterTranslationTool.x/2 , perlinTranslation*jitterTranslationTool.y-jitterTranslationTool.y/2, perlinTranslation*jitterTranslationTool.z-jitterTranslationTool.z/2);

			// ROTATION		*********
			Quaternion 	rotation = Quaternion.Euler (0, 0, rot_ang);
			if (jitterRotationTool != null)
				rotation = Quaternion.Euler ((perlinRot*jitterRotationTool.x - jitterRotationTool.x/2)*1.5f, (perlinRot*jitterRotationTool.y - jitterRotationTool.y/2), (perlinRot*jitterRotationTool.z - jitterRotationTool.z/2)) * rotation;


			// SCALE		**********
			jitterScale = Vector3.zero;
			if (jitterScaleTool != null)
				jitterScale = new Vector3( perlinScale*jitterScaleTool.x-jitterScaleTool.x/2 , perlinScale*jitterScaleTool.y-jitterScaleTool.y/2, 0);
			
			// ******** USE A FUNCTION HERE TO GET FOR HANDLES AS WELL

			return   Matrix4x4.TRS(translate, rotation, Vector3.one+jitterScale) * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, (j) * progressiveRotation), Vector3.one);

		}

	}

}