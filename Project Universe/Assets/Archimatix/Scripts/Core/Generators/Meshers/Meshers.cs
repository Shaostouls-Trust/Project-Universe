using UnityEngine;
#if UNITY_EDITOR  
using UnityEditor;
#endif

 
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

	public interface IMeshGenerator
	{
		
	}

	public class Mesher : AX.Generators.Generator3D, IMeshGenerator
	{


        // INPUTS

        public AXParameter P_Plan;
        public AXParameter planSrc_p;
        public AXParametricObject planSrc_po;

        public AXParameter 	P_TopCapMaterial;
		public AXParameter 	P_BottomCapMaterial;
		public AXParameter 	P_EndCapMaterial;
		public AXParameter 	P_EndCapMesh;


		public AXParameter 	P_TopCap;
        public AXParameter P_BottomCap;
        public AXParameter P_AutoBevelOpenEnds;


        public AXParameter P_Subdivisions;
        public AXParameter P_SubdivisionsGrid;



        // CONTROLS

        public AXTexCoords	po_axTex;
		public Material		po_mat;

		public bool 		topCapHasOwnMat = false;
		public AXTexCoords 		topCapTex;
		public Material 	topCap_mat 		= null;

		public bool 		botCapHasOwnMat = false;
		public AXTexCoords 		botCapTex;
		public Material 	botCap_mat 		= null;

		public bool topCap 					= true;
		public bool botCap 					= true;


		public bool planIsClosed = false;
		public bool sectionIsClosed = false;

		public bool 		endCapHasOwnMat = false;
		public AXTexCoords 		endCapTex;
		public Material 	endCap_mat 		= null;

		public bool   		texLockToTransY = true;
		
		public bool hasSides 				= true;
		public bool hasBackfaces 				= false;
		public bool capA 					= true;
		public bool capB 					= true;

        public bool autoBevelOpenEnds = true;


        public bool hasEndCapSourceMeshes  	= false;

		public Matrix4x4 sectionHandleTransform = Matrix4x4.identity;
		public Matrix4x4 endCapHandleTransform = Matrix4x4.identity;


        public int subdivisions = 1;
        public bool useSubdivisionGrid = false;

		public List<AXMesh> ax_meshes = null;


       



        bool planIsCCW 	= true;
		//bool secIsCCW 	= true;


		public override void init_parametricObject() 
		{

            P_Subdivisions = parametricObject.addParameter(AXParameter.DataType.Int, "Subdivisions", 1, 1, 36);
            P_SubdivisionsGrid = parametricObject.addParameter(AXParameter.DataType.Bool, "SubdivisionGrid", false);

            base.init_parametricObject();



			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Output, "Output Mesh"));

		}


		
		public override void initGUIColor ()
		{
			GUIColor 		= new Color(.63f,.75f,.63f,.8f);
			GUIColorPro 	= new Color(.77f, 1f, .77f, .8f);
			ThumbnailColor  = new Color(.318f,.31f,.376f);
		}


		// POLL INPUTS (only on graph change())
		public override void pollInputParmetersAndSetUpLocalReferences()
		{
			base.pollInputParmetersAndSetUpLocalReferences();

            P_Plan = parametricObject.getParameter("Plan");

            P_TopCapMaterial = parametricObject.getParameter("Top Cap Material");
			P_BottomCapMaterial = parametricObject.getParameter("Bottom Cap Material");
			P_EndCapMaterial = parametricObject.getParameter("End Cap Material");

			P_TopCap 	= parametricObject.getParameter("Top Cap", "top cap",  			"topCap");
            P_BottomCap = parametricObject.getParameter("Bottom Cap", "bottom cap", "botCap");

            P_AutoBevelOpenEnds = parametricObject.getParameter("Auto Bevel Ends");

            P_Subdivisions = parametricObject.getParameter("Subdivisions");
            P_SubdivisionsGrid = parametricObject.getParameter("SubdivisionGrid");
        }


		// POLL CONTROLS (every model.generate())
		public override void pollControlValuesFromParmeters()
		{
			//if (parametersHaveBeenPolled)
			//	return;

			base.pollControlValuesFromParmeters();

            planSrc_p = getUpstreamSourceParameter(P_Plan);
            planSrc_po = (planSrc_p != null) ? planSrc_p.parametricObject : null;





            // TEXTURE

            po_axTex = parametricObject.axTex;
			po_mat 		= parametricObject.axMat.mat; 



			if (po_mat == null) 
				po_mat = parametricObject.model.axMat.mat;


			// TOP CAP MATERIAL 

			topCapTex 	= po_axTex; 
			topCap_mat 	= po_mat;
			if (P_TopCapMaterial != null && P_TopCapMaterial.DependsOn != null)
			{
				if (P_TopCapMaterial.DependsOn.parametricObject.axMat != null && P_TopCapMaterial.DependsOn.parametricObject.axMat.mat != null)
					topCapHasOwnMat = true;
				topCapTex 		= P_TopCapMaterial.DependsOn.parametricObject.axTex;
			}	

			// BOTTOM CAP MATERIAL

			botCapTex 	= topCapTex; 
			botCap_mat 	= topCap_mat;
			if (P_BottomCapMaterial != null && P_BottomCapMaterial.DependsOn != null)
			{
				if (P_BottomCapMaterial.DependsOn.parametricObject.axMat != null && P_BottomCapMaterial.DependsOn.parametricObject.axMat.mat != null)
					botCapHasOwnMat = true;
				botCapTex 		= P_BottomCapMaterial.DependsOn.parametricObject.axTex;
			}	



			// END CAP TEXTURE
			endCapTex 	= po_axTex; 
			endCap_mat 	= po_mat;

			if (P_EndCapMaterial != null && P_EndCapMaterial.DependsOn != null)
			{
				if (P_EndCapMaterial.DependsOn.parametricObject.axMat != null && P_EndCapMaterial.DependsOn.parametricObject.axMat.mat != null)
					endCapHasOwnMat = true;
				endCapTex 		= P_EndCapMaterial.DependsOn.parametricObject.axTex;
			}	



			// CAP BOOLS

			topCap 		= (P_TopCap != null) 	? P_TopCap.boolval 		: false;
			botCap 		= (P_BottomCap != null) ? P_BottomCap.boolval	: false;



            autoBevelOpenEnds =  (P_AutoBevelOpenEnds != null) ? P_AutoBevelOpenEnds.boolval : true;

            subdivisions =  (P_Subdivisions != null) ? P_Subdivisions.IntVal : 1;
            
            useSubdivisionGrid = (P_SubdivisionsGrid != null) ? P_SubdivisionsGrid.boolval : false;

        }






        // MESHER GENERATOR - FIRST PASS
        public GameObject generateFirstPass(AXParametricObject initiator_po, bool makeGameObjects, AXParameter plan_p, AXParameter sec_p, Matrix4x4 endCapShiftM, bool renderToOutputParameter=true, bool addLocalMatrix = true)
		{

			
			//int seglen = (int) (plan_p.seglen * AXGeometry.Utilities.IntPointPrecision);



			bool colorVerts = false;


			//Debug.Log("first pass: makeGameObjects="+ makeGameObjects);

			//parametricObject.setLocalMatrix();
			if (ax_meshes == null)
				ax_meshes = new List<AXMesh>();


			bool makeIndividualGameObjects = (makeGameObjects && ! parametricObject.combineMeshes);	



			if (topCapHasOwnMat && P_TopCapMaterial != null && P_TopCapMaterial.DependsOn != null)
				topCap_mat 	= P_TopCapMaterial.DependsOn.Parent.axMat.mat;
			
			if (botCapHasOwnMat && P_BottomCapMaterial != null && P_BottomCapMaterial.DependsOn != null)
				botCap_mat 	= P_BottomCapMaterial.DependsOn.Parent.axMat.mat;
			else 
				botCap_mat = topCap_mat;
			
			if (endCapHasOwnMat && P_EndCapMaterial != null && P_EndCapMaterial.DependsOn != null)
				endCap_mat 	= P_EndCapMaterial.DependsOn.Parent.axMat.mat;





			//Debug.Log("  @@@@@@@@@@@@@@@@ @@@@@@@@@@@@@@@@@ @@@@@@@@@@@@@@@@@ @@@@@@@@@@@Setting Plan: "+plan_p.Name);
			setSectionHandleTransform(plan_p);
			//Debug.Log(sectionHandleTransform);


			Paths secPaths = (sec_p != null) ? sec_p.getPaths() : null;

			if (secPaths == null)
			{
				//return null;

			}


			 
			float height = (secPaths != null) ? (Clipper.GetBounds (secPaths).top/AXGeometry.Utilities.IntPointPrecision) : 0;
																												
			
			 
			
			AXTexCoords texy = new AXTexCoords(po_axTex);
			if (texLockToTransY)
				texy.shift.y += transY/texy.scale.y;

			
			

			int count = 0;
			
			 

			
			// SWEEPER SETUP
			PlanSweeper	 planSweeper = new PlanSweeper();						
			
			AXMesh tmpAXmesh = null;

			List<Mesh> sideMeshes 	= new List<Mesh>();
			List<Mesh> insideMeshes = new List<Mesh>();

			Mesh sidesMesh 			= null;
			Mesh backfacesMesh 		= null;
			Mesh topMesh 			= null;
			Mesh botMesh 			= null;
			

			planIsClosed 		= (plan_p.hasThickness || plan_p.shapeState == ShapeState.Closed) ? true : false;



			/*
			if (plan_p != null && plan_p.reverse)
			{
				plan_p.doReverse();
				planIsCCW = plan_p.isCCW();
			}

			if (sec_p != null && sec_p.reverse)
			{	
				sec_p.doReverse();
				//secIsCCW = sec_p.isCCW();
			}
			*/
			planIsCCW = plan_p.isCCW();
			

		



			if (sec_p != null)
				sectionIsClosed 	= (sec_p.hasThickness || sec_p.shapeState == ShapeState.Closed) ? true : false;
			

			
			GameObject 	go = null;			
			
			Matrix4x4 tmx = parametricObject.generator.localMatrixWithAxisRotationAndAlignment;
			

			// DEFAULT SECTION CAP
			Mesh endCapMeshDefault = null;
			if (sec_p != null && (capA || capB))
			{
				// Make a default polygon cap with same material
				if (sec_p.polyTree != null)
					endCapMeshDefault = AXPolygon.triangulate(sec_p.polyTree, endCapTex);
				else if (sec_p.paths != null)
					endCapMeshDefault = AXPolygon.triangulate(sec_p.paths, endCapTex);
			}	




			// GET SEGLEN FROM SUBDIVISION
			int seglen = 9999999;
			Paths planPaths = null;

			if (plan_p.hasPolytreeItems && plan_p.polyTree != null)
				planPaths = Clipper.PolyTreeToPaths(plan_p.polyTree);
			else
				planPaths = plan_p.paths;


            //Debug.Log("planPaths="+planPaths);
            //         if (planPaths != null && plan_p.subdivision > 0)
            //{
            //}

            if (planPaths != null)
            {
                if (subdivisions > 1)
                {
                    seglen = Pather.getSegLenBasedOnSubdivision(planPaths, subdivisions);
                }
                else if (plan_p.subdivision > 0)
                {
                    seglen = Pather.getSegLenBasedOnSubdivision(planPaths, (int)plan_p.subdivision);
                   
                }
                planPaths = Pather.segmentPaths(planPaths, seglen, planIsClosed);


            }



            bool doProcessPaths1 	= true;
			Paths processedPaths 	= null;
			if (doProcessPaths1 && planPaths != null)
			{
				processedPaths = new Paths();
				for (int i = 0; i < planPaths.Count; i++) 
				{
					Path planPath = planPaths [i];

					//Debug.Log(" ++ " + Pather.isConvex(planPath));
					//Debug.Log("planPath.Count = " + planPath.Count);
					processedPaths.Add(planPath);
				}
			}
			else
				processedPaths = plan_p.paths;




			


			if (plan_p.hasPolytreeItems)
			{
			    //Debug.Log ("First Pass - hasPolytreeItems: Do Plan AXClipperLib.PolyTree: " + parametricObject.Name);


				// BOUNDS FOR ASSEMBLY
				Rect plan_bnds 	=  AXGeometry.Utilities.getClipperBounds(Clipper.PolyTreeToPaths(plan_p.polyTree));
				Rect sec_bnds = new Rect();

				 if (sec_p != null)
					sec_bnds= ( sec_p.polyTree != null ) ? AXGeometry.Utilities.getClipperBounds(Clipper.PolyTreeToPaths(sec_p.polyTree)) : AXGeometry.Utilities.getClipperBounds(sec_p.paths);

				height = sec_bnds.yMax;

				  


				Bounds b = new Bounds();
				b.size = new Vector3(plan_bnds.width, height, plan_bnds.height);
				b.extents = b.size/2;
				b.center = new Vector3(plan_bnds.center.x, height/2, plan_bnds.center.y);
				parametricObject.bounds = b;

				//Debug.Log(plan_p.parametricObject.Name + " makeIndividualGameObjects="+makeIndividualGameObjects);

				if (makeIndividualGameObjects)
				{
					//Debug.Log("here");
					AXClipperLib.PolyTree secPolytree = (sec_p != null) ? sec_p.polyTree : null; 
					// step through the polytree structure recursively...




					go = generateGameObjectsFromPlanAndSecPolytrees(initiator_po, plan_p, plan_p.polyTree, sec_p, secPolytree);
					go.transform.rotation 		= AXUtilities.QuaternionFromMatrix(tmx);
					go.transform.position 		= AXUtilities.GetPosition(tmx);
					go.transform.localScale 	= parametricObject.getLocalScale(); 

					parametricObject.lastGameObjectCreated = go;
					return go;
				}
				
				else // COMBINED (MakeGO's or not)
				{
					// Debug.Log("Polytree - Combined");

					// JUST MAKE THE AX_MESHES AND SUBMIT 
					
					// No need to step through the polytree structure because 
					// it solids and holes are equal in the eyes of the triangulation which only needs
					// clockwise and counterclockwise to make its decision.
                     

					// EACH SECTION * * * * * * * * * * * * * * * * * * * * * * * * * * * ..

					if (secPaths != null)
					{
						foreach (Path sectionPath in secPaths)
						{					
							Spline sectionSpline 	= new Spline(sectionPath, sectionIsClosed,  sec_p.breakGeom, sec_p.breakNorm);
						    

                            //Spline sectionSplineRev = new Spline(sectionPath, sectionIsClosed,  sec_p.breakGeom, sec_p.breakNorm);
							//sectionSplineRev.Reverse();

							if (sectionSpline.controlVertices.Count < 2)
								continue;
                             
							float topx;
							float topy;
							float botx;
							float boty;

							float y1 = sectionSpline.controlVertices.First().y;
							float y2 = sectionSpline.controlVertices.Last().y;

							if (y1 < y2)
							{
								boty = y1;
								botx = sectionSpline.controlVertices.First().x;

								topy = y2;
								topx = sectionSpline.controlVertices.Last().x;
							}
							else
							{
								boty = y2;
								botx = sectionSpline.controlVertices.Last().x;

								topy = y1;
								topx = sectionSpline.controlVertices.First().x;
							}


							Matrix4x4 botCapTransform 		= Matrix4x4.TRS (new Vector3(0, boty, 0), Quaternion.identity, Vector3.one);
							//Matrix4x4 topCapTransform 		= Matrix4x4.TRS (new Vector3(0, topy, 0), Quaternion.identity, Vector3.one);

							Matrix4x4 turnu 				= Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90, 0, 0), Vector3.one);




							//seint seglen = getSegLenBasedOnSubdivision(glen = (int) (plan_p.seglen * AXGeometry.Utilities.IntPointPrecision);

							// SIDES -------------------
							if (sec_p != null && (hasSides || hasBackfaces))
							{



								// EACH PLAN * * * * * * * * * * * * * * * * * * * * * * * * * * *  //

								foreach(Path planPath in planPaths)
								{
									// PLAN



									Spline planSpline 		= new Spline(planPath, planIsClosed, plan_p.breakGeom, plan_p.breakNorm);




									// SIDES
									if (hasSides)
									{

                                        sidesMesh = new Mesh();
										try{
											planSweeper.generate(ref sidesMesh, planSpline, sectionSpline, texy);
										}
										catch
										{
										 Debug.Log("Too many vertices");
										}
                                         
										sidesMesh.RecalculateBounds();
										tmpAXmesh 	= new AXMesh( sidesMesh, Matrix4x4.identity, po_mat);
										tmpAXmesh.name 		= parametricObject.Name+"_sides";
										tmpAXmesh.makerPO 	= parametricObject;					
										ax_meshes.Add (tmpAXmesh);
									}
									
									if (hasBackfaces)
									{
                                      
										sidesMesh = new Mesh();
										try {
											planSweeper.generate(ref sidesMesh, planSpline, sectionSpline.Reverse(), texy);
										} catch  {
                                            Debug.Log("Too many vertices");
                                        }
										sidesMesh.RecalculateBounds();
										tmpAXmesh 	= new AXMesh( sidesMesh, Matrix4x4.identity, po_mat);
										tmpAXmesh.name 		= parametricObject.Name+"_backFaces";
										tmpAXmesh.makerPO 	= parametricObject;					
										ax_meshes.Add (tmpAXmesh);
									}



									// @ --- CAP_A
									if (capA && endCapMeshDefault != null)
									{
										Matrix4x4 capATransform = planSpline.begTransform;//	Archimatix.getEndTransformA(planPath);

										tmpAXmesh = new AXMesh( endCapMeshDefault, capATransform, endCap_mat);
										tmpAXmesh.name 		= parametricObject.Name+"_endCapA";
                                        tmpAXmesh.makerPO = parametricObject;
                                        tmpAXmesh.getsCollider = parametricObject.isRigidbody ? false : true;

										ax_meshes.Add (tmpAXmesh);

										if (hasEndCapSourceMeshes &&  P_EndCapMaterial.DependsOn != null && P_EndCapMaterial.DependsOn.meshes != null)
										{
											for (int mi = 0; mi < P_EndCapMaterial.DependsOn.meshes.Count; mi++) {
												AXMesh dep_amesh = P_EndCapMaterial.DependsOn.meshes [mi];
												AXMesh clone = dep_amesh.Clone (capATransform * turnu * dep_amesh.transMatrix);
												ax_meshes.Add (clone);
											}
										}

									}

									// @ --- CAP_B
									if (capB && endCapMeshDefault != null)
									{
										Matrix4x4 capBTransform = AXGeometry.Utilities.getEndTransformB(planPath);
											
										// because drawmesh cannot do negative scaling
										// the workaround is to bake the scaling in...
										AXMesh tmpAXmesh2 = new AXMesh(AXMesh.freezeWithMatrix(endCapMeshDefault, capBTransform), Matrix4x4.identity, endCap_mat);
                                        tmpAXmesh2.makerPO = parametricObject;
                                        ax_meshes.Add (tmpAXmesh2);
										
										if (hasEndCapSourceMeshes  &&  P_EndCapMaterial.DependsOn != null && P_EndCapMaterial.DependsOn.meshes != null)
										{
											for (int mi = 0; mi < P_EndCapMaterial.DependsOn.meshes.Count; mi++) {
												AXMesh dep_amesh = P_EndCapMaterial.DependsOn.meshes [mi];
												tmpAXmesh2 = dep_amesh.Clone ();
												tmpAXmesh2.mesh = AXMesh.freezeWithMatrix (dep_amesh.mesh, dep_amesh.transMatrix.inverse * capBTransform * turnu * dep_amesh.transMatrix);
												ax_meshes.Add (tmpAXmesh2);
											}
										}
									}

									
								}
							}
							
							// TOP & BOTTOM CAPS ------------------
							if (topCap)
							{

								AXClipperLib.PolyTree resPolytree = plan_p.polyTree;
								
								if (topx != 0)
								{
									ClipperOffset 	co  = new ClipperOffset ();
									co.AddPaths (AXGeometry.Utilities.cleanPaths(planPaths), AXClipperLib.JoinType.jtMiter, AXClipperLib.EndType.etClosedPolygon); //JoinType.jtSquare, AXClipperLib.EndType.etClosedPolygon);
									resPolytree = new AXClipperLib.PolyTree();
									co.Execute (ref resPolytree, (double)(topx * AXGeometry.Utilities.IntPointPrecision));
								}

								topMesh = AXPolygon.triangulate(resPolytree, topCapTex, seglen, useSubdivisionGrid);

                                if (topMesh == null)
                                    return null;

								if (! planIsCCW || hasBackfaces)
									topMesh.triangles = topMesh.triangles.Reverse().ToArray();

								topMesh.RecalculateNormals();
								topMesh.RecalculateBounds();

								// Raise up
								Vector3[] verts = new Vector3[topMesh.vertices.Length];
								for (int ii=0; ii<topMesh.vertices.Length; ii++)
									verts[ii] = new Vector3(topMesh.vertices[ii].x, topMesh.vertices[ii].y + topy, topMesh.vertices[ii].z);
								topMesh.vertices = verts;

								tmpAXmesh = new AXMesh( topMesh, Matrix4x4.identity, topCap_mat);
								tmpAXmesh.name 		= parametricObject.Name+"_topCap";
                                tmpAXmesh.makerPO = parametricObject;
                                tmpAXmesh.getsCollider = parametricObject.isRigidbody ? false : true;
								ax_meshes.Add (tmpAXmesh);
							}
							if (botCap) 
							{
								AXClipperLib.PolyTree resPolytree = plan_p.polyTree;
								
								if (botx != 0)
								{
									ClipperOffset 	co  = new ClipperOffset ();
									co.AddPaths (AXGeometry.Utilities.cleanPaths(planPaths), AXClipperLib.JoinType.jtMiter, AXClipperLib.EndType.etClosedPolygon); //JoinType.jtSquare, AXClipperLib.EndType.etClosedPolygon);
									resPolytree = new AXClipperLib.PolyTree();
									co.Execute (ref resPolytree, (double)(botx * AXGeometry.Utilities.IntPointPrecision));
								}
								botMesh = AXPolygon.triangulate(resPolytree, botCapTex, seglen, useSubdivisionGrid);
								if (planIsCCW && ! hasBackfaces)
									botMesh.triangles = botMesh.triangles.Reverse().ToArray();
								botMesh.RecalculateNormals();
								tmpAXmesh = new AXMesh( botMesh, botCapTransform, botCap_mat);
								tmpAXmesh.name 		= parametricObject.Name+"_botCap";
                                tmpAXmesh.makerPO = parametricObject;
                                tmpAXmesh.getsCollider = parametricObject.isRigidbody ? false : true;
								ax_meshes.Add (tmpAXmesh);
							}



						}

                       
					} 
					else
					{

						// NO SECTION PATH - must be flat poly

						
							// TOP & BOTTOM CAPS ------------------
							if (topCap)
							{

								AXClipperLib.PolyTree resPolytree = plan_p.polyTree;
								
								topMesh = AXPolygon.triangulate(resPolytree, topCapTex, seglen, useSubdivisionGrid);

							
								if (! planIsCCW || hasBackfaces)
									topMesh.triangles = topMesh.triangles.Reverse().ToArray();

								topMesh.RecalculateBounds();
								topMesh.RecalculateNormals();


								tmpAXmesh = new AXMesh( topMesh, Matrix4x4.identity, topCap_mat);
								tmpAXmesh.name 		= parametricObject.Name+"_topCap";
                                tmpAXmesh.makerPO = parametricObject;
                                tmpAXmesh.getsCollider = parametricObject.isRigidbody ? false : true;
								ax_meshes.Add (tmpAXmesh);
							}
							if (botCap)
							{
								AXClipperLib.PolyTree resPolytree = plan_p.polyTree;
								
								botMesh = AXPolygon.triangulate(resPolytree, topCapTex, seglen, useSubdivisionGrid);
							if (planIsCCW && ! hasBackfaces)
									botMesh.triangles = botMesh.triangles.Reverse().ToArray();
								botMesh.RecalculateNormals();
								tmpAXmesh = new AXMesh( botMesh, Matrix4x4.identity, botCap_mat);
								tmpAXmesh.name 		= parametricObject.Name+"_botCap";
                                tmpAXmesh.makerPO = parametricObject;
                                tmpAXmesh.getsCollider = parametricObject.isRigidbody ? false : true;
								ax_meshes.Add (tmpAXmesh);
							}
					}



                   

                    // Turn ax_meshes into GameObjects?
                    if (makeGameObjects) // 
					{

                        //Debug.Log("MakeGOs");

						go =	parametricObject.makeGameObjectsFromAXMeshes(ax_meshes, false);

						go.transform.rotation 		= AXUtilities.QuaternionFromMatrix(tmx);
						go.transform.position 		= AXUtilities.GetPosition(tmx);
						go.transform.localScale 	= parametricObject.getLocalScale();

						if (plan_p != null && plan_p.reverse)
							plan_p.doReverse();


						if (sec_p != null && sec_p.reverse)
							sec_p.doReverse();
						
						parametricObject.lastGameObjectCreated = go;
                        //Debug.Log("Done");
						return go;
					}
					else
					{
						// Submit AXMeshes for realtime rendering
						//parametricObject.finishMultiAXMeshAndOutput(ax_meshes, renderToOutputParameter);
					}

					if (plan_p != null && plan_p.reverse)
						plan_p.doReverse();


					

					if (sec_p != null && sec_p.reverse)
							sec_p.doReverse();


                   // Debug.Log("returning null");
					return null;
														
				}	
																					
			} // END POLYTREE
			
			
			
			
			
			
			
			// *********************************************************** PATHS
			
			else if (plan_p.paths != null)
			{
			
				//Debug.Log ("First Pass - Paths: " + parametricObject.Name);
				

				if (makeIndividualGameObjects)
				{
					//Debug.Log("plan_p.paths.Count="+plan_p.paths.Count+", " + plan_p.paths[0].Count);
					if (plan_p.paths.Count > 1 )
					{
						//go = new GameObject(parametricObject.Name);
						go = ArchimatixUtils.createAXGameObject(parametricObject.Name, parametricObject);

	
					}
				}


				// PREPROCESS PLAN_PATHS TO BREAK APART CONCAVES AND DESIGNATE THEM FOR FIXEDJOINT COMBINATIONS
				bool doProcessPaths 	= false;
				Paths processedPaths2 	= null;
				if (doProcessPaths)
				{
					processedPaths2 = new Paths();
					for (int i = 0; i < plan_p.paths.Count; i++) 
					{
						Path planPath = plan_p.paths [i];

						Debug.Log(" ++ " + Pather.isConvex(planPath));
						//Debug.Log("planPath.Count = " + planPath.Count);
						processedPaths2.Add(planPath);
					}
				}
				else
					processedPaths2 = plan_p.paths;

				// EACH PLAN * * * * * * * * * * * * * * * * * * * * * * * * * * * ..
				// Create meshes and add them to axmeshes
				// OBJECT will be made automatically from ax_meshes 




				for (int i = 0; i < processedPaths2.Count; i++) {
					Path planPath = processedPaths2 [i];





					if (planPath == null || planPath.Count == 0)
						continue;

					
					if (seglen > 0)
						planPath = Pather.segmentPath(planPath,  seglen, planIsClosed);


					// AREA FOR VOLUME CALCULATION
					float planArea =  AXGeometry.Utilities.pathArea(planPath);

					 



					Spline planSpline = new Spline (planPath, planIsClosed, plan_p.breakGeom, plan_p.breakNorm, true, autoBevelOpenEnds);
                    

					//StopWatch sw = new StopWatch();


					List<AXMesh> planPathMeshes = new List<AXMesh> ();
					float topx = 0;
					float topy = 0;
					float botx = 0;
					float boty = 0;


					// EACH SECTION * * * * * * * * * * * * * * * * * * * * * * * * * * * ..
					if (secPaths != null && secPaths.Count > 0) {

						Spline sectionSpline0 = null;


						for (int j = 0; j < secPaths.Count; j++) {
							Path 	sectionPath = secPaths [j];
							Spline 	sectionSpline = new Spline (sectionPath, sectionIsClosed, sec_p.breakGeom, sec_p.breakNorm);

							float sectionArea = AXGeometry.Utilities.pathArea(sectionPath);


							if (sectionSpline.controlVertices.Count < 1)
								continue;




							// VOLUME CALCULATION //
							// Most useful for readout of volume used without making GameObjects and Rigidbodies

							// calculate volume and add this to only one of the meshes (typically the sides mesh)
							float vol;
							if (sectionIsClosed)
							{
								vol = sectionSpline.length * sectionArea;
							} 
							else
							{
								// for most accurate, offset plan to left edge of section and take cap to cap height * area + section area * planLength
								// For now, approximate
								Paths tmpPaths = new Paths();
								tmpPaths.Add(sectionPath);
								IntRect pbounds = Clipper.GetBounds(tmpPaths);

								float appxVolumeHgt = Mathf.Abs(((float)(pbounds.top-pbounds.bottom)/((float)AXGeometry.Utilities.IntPointPrecision)));

								vol = appxVolumeHgt * planArea;
							}

							// Debug.Log("vol = " + vol);

							 
							
							if (j == 0)
								sectionSpline0 = sectionSpline;

							// GENERATE MESHES
							// @ ---- SIDES
							if (hasSides) {
								sidesMesh = new Mesh ();



								planSweeper.generate (ref sidesMesh, planSpline, sectionSpline, texy);

								sidesMesh.RecalculateBounds ();
								sidesMesh.name = parametricObject.Name + "_sides";
								 
								
								// verts color -- DANGER! THIS IS COMPUTATIONALLY INTENSIVE!
								if (colorVerts)
								{
									Color[] colors = new Color[sidesMesh.vertices.Length];
									for (int ii = 0; ii < sidesMesh.vertices.Length; ii++) {
										if (sidesMesh.vertices [ii].y > 1)
											colors [ii] = Color.Lerp (Color.red, Color.green, .8f);
										// colors[i] = Color.Lerp(Color.red, Color.green, topMesh.vertices[i].y);
										else
											colors [ii] = Color.red;
										//Color.Lerp(Color.red, Color.green, topMesh.vertices[i].y);
									}
									sidesMesh.colors = colors;
								}
								  

								sideMeshes.Add (sidesMesh);
								// Eventually: add running total up...
								//parametricObject.bounds = sidesMesh.bounds;
								tmpAXmesh = new AXMesh (sidesMesh, Matrix4x4.identity, po_mat);
								tmpAXmesh.name = parametricObject.Name + "_" + count++;
								tmpAXmesh.makerPO = parametricObject;
								tmpAXmesh.volume = vol;	// preferred 
								ax_meshes.Add (tmpAXmesh);
								planPathMeshes.Add (tmpAXmesh);
								//Debug.Log("hasSides " + sw.duration());
							}
							// @ ---- BACK_FACES
							if (hasBackfaces) {
								backfacesMesh = new Mesh ();
								planSweeper.generate (ref backfacesMesh, planSpline, sectionSpline.Reverse (), texy);
								//mesh.RecalculateNormals();
								backfacesMesh.RecalculateBounds ();
								backfacesMesh.name = parametricObject.Name + "_backfaces";
								insideMeshes.Add (backfacesMesh);
								// Eventually: add running total up...
								tmpAXmesh = new AXMesh (backfacesMesh, Matrix4x4.identity, po_mat);
								tmpAXmesh.name = parametricObject.Name + "_" + count++;
								tmpAXmesh.makerPO = parametricObject;
								if (! hasSides)
									tmpAXmesh.volume = vol;
								ax_meshes.Add (tmpAXmesh);
								planPathMeshes.Add (tmpAXmesh);

							}
						}
						//Spline sectionSpline0 = new Spline (secPaths [0], sectionIsClosed, sec_p.breakGeom, sec_p.breakNorm);

						if (sectionSpline0 == null) // ||  sectionSpline0.controlVertices == null || sectionSpline0.controlVertices.First () == null)
							continue;
						 
						float y1 = sectionSpline0.controlVertices.First ().y;
						float y2 = sectionSpline0.controlVertices.Last ().y;
						if (y1 < y2) {
							boty = y1;
							botx = sectionSpline0.controlVertices.First ().x;
							topy = y2;
							topx = sectionSpline0.controlVertices.Last ().x;
						}
						else {
							boty = y2;
							botx = sectionSpline0.controlVertices.Last ().x;
							topy = y1;
							topx = sectionSpline0.controlVertices.First ().x;
						}
					}




					// @ --- TOP CAP
					if (topCap) {
						

						//Matrix4x4 topCapTransform = Matrix4x4.TRS (new Vector3 (0, topy, 0), Quaternion.identity, Vector3.one);

						// If there is a section or taper, the plan needs to be offset. 
						// This is different from the offset of the source shape.
						AXClipperLib.PolyTree resPolytree = plan_p.polyTree;
						if (topx != 0) {
							AXClipperLib.ClipperOffset co = new ClipperOffset ();
							co.AddPath (planPath, AXClipperLib.JoinType.jtMiter, AXClipperLib.EndType.etClosedPolygon);
							//JoinType.jtSquare, AXClipperLib.EndType.etClosedPolygon);
							resPolytree = new AXClipperLib.PolyTree ();
							co.Execute (ref resPolytree, (double)(topx * AXGeometry.Utilities.IntPointPrecision));
							topMesh = AXPolygon.triangulate (resPolytree, topCapTex);
						}
						else {
							topMesh = AXPolygon.triangulate (planPath, topCapTex);
						}


						if (topMesh != null) {


							// verts color
//							if (! planIsCCW || hasBackfaces)
//								topMesh.triangles = topMesh.triangles.Reverse ().ToArray ();

							topMesh.RecalculateNormals();
							if (colorVerts)
							{
								Color[] colors = new Color[topMesh.vertices.Length];
								for (int ii = 0; ii < topMesh.vertices.Length; ii++)
									colors [ii] = Color.green;
								//Color.Lerp(Color.red, Color.green, topMesh.vertices[i].y);
								topMesh.colors = colors;
							}
							//topMesh.RecalculateBounds ();


							// Raise up
							Vector3[] verts = new Vector3[topMesh.vertices.Length];
							for (int ii=0; ii<topMesh.vertices.Length; ii++)
								verts[ii] = new Vector3(topMesh.vertices[ii].x, topMesh.vertices[ii].y + topy, topMesh.vertices[ii].z);
							topMesh.vertices = verts;


							tmpAXmesh = new AXMesh (topMesh, Matrix4x4.identity, topCap_mat);
							tmpAXmesh.name = parametricObject.Name + "_topCap";
                            tmpAXmesh.makerPO = parametricObject;
                            tmpAXmesh.getsCollider = parametricObject.isRigidbody ? false : true;
							ax_meshes.Add (tmpAXmesh);
							planPathMeshes.Add (tmpAXmesh);
						}
					}
					// @ --- BOTTOM CAP
					if (botCap) {
						AXClipperLib.PolyTree resPolytree = plan_p.polyTree;
						Matrix4x4 botCapTransform = Matrix4x4.TRS (new Vector3 (0, boty, 0), Quaternion.identity, Vector3.one);
						if (botx != 0) {
							AXClipperLib.ClipperOffset co = new ClipperOffset ();
							co.AddPath (planPath, AXClipperLib.JoinType.jtMiter, AXClipperLib.EndType.etClosedPolygon);
							//JoinType.jtSquare, AXClipperLib.EndType.etClosedPolygon);
							resPolytree = new AXClipperLib.PolyTree ();
							co.Execute (ref resPolytree, (double)(botx * AXGeometry.Utilities.IntPointPrecision));
							botMesh = AXPolygon.triangulate (resPolytree, botCapTex);
						}
						else
							botMesh = AXPolygon.triangulate (planPath, botCapTex);

						if (botMesh != null) {
							// verts color
							if (colorVerts)
							{
								Color[] colors = new Color[botMesh.vertices.Length];
								for (int ii = 0; ii < botMesh.vertices.Length; ii++)
									colors [ii] = Color.blue;
								//Color.Lerp(Color.red, Color.green, topMesh.vertices[i].y);
								botMesh.colors = colors;
							} 

							if (planIsCCW && ! hasBackfaces)
								botMesh.triangles = botMesh.triangles.Reverse ().ToArray ();
							botMesh.RecalculateNormals ();

							tmpAXmesh = new AXMesh (botMesh, botCapTransform, botCap_mat);
							tmpAXmesh.name = parametricObject.Name + "_botCap";
                            tmpAXmesh.makerPO = parametricObject;
                            tmpAXmesh.getsCollider = parametricObject.isRigidbody ? false : true;
							ax_meshes.Add (tmpAXmesh);
							planPathMeshes.Add (tmpAXmesh);
						}
					}
					Matrix4x4 turnu = Matrix4x4.TRS (Vector3.zero, Quaternion.Euler (90, 0, 0), Vector3.one);
					// @ --- CAP_A
					Matrix4x4 capATransform = planSpline.begTransform;
					//Archimatix.getEndTransformA(planPath);
					endCapHandleTransform = capATransform * turnu;
					if (capA && endCapMeshDefault != null) {
						tmpAXmesh = new AXMesh (endCapMeshDefault, capATransform, endCap_mat);
						tmpAXmesh.name = parametricObject.Name + "_endCapA";
                        tmpAXmesh.makerPO = parametricObject;
                        tmpAXmesh.getsCollider = parametricObject.isRigidbody ? false : true;
						ax_meshes.Add (tmpAXmesh);
						planPathMeshes.Add (tmpAXmesh);
					}
					if (hasEndCapSourceMeshes) {
						for (int mi = 0; mi < P_EndCapMesh.DependsOn.meshes.Count; mi++) {
							AXMesh dep_amesh = P_EndCapMesh.DependsOn.meshes [mi];
							AXMesh clone = dep_amesh.Clone (endCapHandleTransform *  endCapShiftM *  dep_amesh.transMatrix);
							clone.getsCollider = parametricObject.isRigidbody ? false : true;
							ax_meshes.Add (clone);
							planPathMeshes.Add (clone);
						}
					}
					// @ --- CAP_B
					Matrix4x4 capBTransform = AXGeometry.Utilities.getEndTransformB (planPath, planSpline.isBeveledB);
					if (capB && endCapMeshDefault != null) {
						//Matrix4x4 capBTransform = planSpline.endTransform; //Archimatix.getEndTransformB(planPath);
						if (makeIndividualGameObjects) {
							tmpAXmesh = new AXMesh (endCapMeshDefault, capBTransform, endCap_mat);
							tmpAXmesh.name = parametricObject.Name + "_endCapB";
                            tmpAXmesh.makerPO = parametricObject;
                            tmpAXmesh.getsCollider = parametricObject.isRigidbody ? false : true;
							planPathMeshes.Add (tmpAXmesh);
						}
						else {
							// because drawmesh cannot do negative scaling
							// the workaround is to bake the scaling in...
							AXMesh tmpAXmesh2 = new AXMesh (AXMesh.freezeWithMatrix (endCapMeshDefault, capBTransform), Matrix4x4.identity, endCap_mat);
                            tmpAXmesh.makerPO = parametricObject;
                            ax_meshes.Add (tmpAXmesh2);
						}
					}
					if (hasEndCapSourceMeshes) {
						capBTransform = capBTransform * turnu;
						for (int mi = 0; mi < P_EndCapMesh.DependsOn.meshes.Count; mi++) {
							AXMesh dep_amesh = P_EndCapMesh.DependsOn.meshes [mi];
							if (makeIndividualGameObjects) {
								AXMesh clone = dep_amesh.Clone (capBTransform * endCapShiftM * dep_amesh.transMatrix);
								clone.getsCollider = parametricObject.isRigidbody ? false : true;
								planPathMeshes.Add (clone);
							}
							else {
								AXMesh tmpAXmesh2 = dep_amesh.Clone ();
								tmpAXmesh2.mesh = AXMesh.freezeWithMatrix (dep_amesh.mesh, dep_amesh.transMatrix.inverse * capBTransform  * endCapShiftM * dep_amesh.transMatrix);
								tmpAXmesh2.getsCollider = parametricObject.isRigidbody ? false : true;
								ax_meshes.Add (tmpAXmesh2);
							}
						}
					}
					if (makeIndividualGameObjects) {

						// Combine the meshes for THIS planPath (planPathMeshes) into a GameObject (and alternatively its caps children defined by tmpAXmesh.getsCollider = false;
						GameObject planPathObject = parametricObject.makeGameObjectsFromAXMeshes (planPathMeshes);
						//if (! parametricObject.isRigidbody)
						//	planPathObject.isStatic = true;
						if (plan_p.paths.Count == 1)
							go = planPathObject;
						else
							planPathObject.transform.parent = go.transform;
					}
				} // END PLAN_PATH LOOP



				//Debug.Log("here makeIndividualGameObjects="+makeIndividualGameObjects);
				if (makeIndividualGameObjects)
				{
					
					//tmx = parametricObject.generator.localMatrix;
					if (go != null)
					{
						if (addLocalMatrix)
						{
							go.transform.rotation 		= AXUtilities.QuaternionFromMatrix(tmx);
							go.transform.position 		= AXUtilities.GetPosition(tmx);
							//Debug.Log(parametricObject.getLocalScale());
							go.transform.localScale 	= parametricObject.getLocalScale();
					   	}

						if (plan_p != null && plan_p.reverse)
							plan_p.doReverse();

						if (sec_p != null && sec_p.reverse)
							sec_p.doReverse();

						parametricObject.lastGameObjectCreated = go;
						return go;
					}
					//else
					//	return null;
				}

			} // END PLAN_PATHS


				
			//parametricObject.finishMultiAXMeshAndOutput(ax_meshes, renderToOutputParameter);
			//setBoundaryFromMeshes(ax_meshes);

			// Turn ax_meshes into GameObjects
			if (makeGameObjects && parametricObject.combineMeshes)
			{
			//Debug.Log("COCO");
				go =	parametricObject.makeGameObjectsFromAXMeshes(ax_meshes, false);

				go.transform.rotation 		= AXUtilities.QuaternionFromMatrix(tmx);
				go.transform.position 		= AXUtilities.GetPosition(tmx);
				go.transform.localScale 	= parametricObject.getLocalScale(); 


				if (plan_p != null && plan_p.reverse)
					plan_p.doReverse();

				if (sec_p != null && sec_p.reverse)
					sec_p.doReverse();

				parametricObject.lastGameObjectCreated = go;
				return go;
			}
			if (plan_p != null && plan_p.reverse)
				plan_p.doReverse();

			if (sec_p != null && sec_p.reverse)
				sec_p.doReverse();

			return null;

		}
		







		//public GameObject generateGameObjectFromAXMeshes(AXMesh








		
		
		public  GameObject generateGameObjectsFromPlanAndSecPolytrees(AXParametricObject initiator_po,  AXParameter plan_p, PolyNode planPolyTree, AXParameter sec_p, PolyNode secPolyTree, int gen=0)
		{
			//Debug.Log ("generateGameObjectsFromPlanPolytree:: gen=" + gen );




			if (gen > 10)
				return null;
				
			if (planPolyTree.Childs == null || planPolyTree.Childs.Count == 0)
				return null;
			

			// GET SEGLEN FROM SUBDIVISION
			int seglen = 999999;
			Paths planPaths =Clipper.PolyTreeToPaths(plan_p.polyTree);



			//if (planPaths != null && plan_p.subdivision > 0)
			//{
			//	seglen = Pather.getSegLenBasedOnSubdivision(planPaths, (int) plan_p.subdivision);
			//	planPaths = Pather.segmentPaths(planPaths,  seglen, planIsClosed);
			//}


            //Debug.Log("planPaths="+planPaths);
            //         if (planPaths != null && plan_p.subdivision > 0)
            //{
            //}

            if (planPaths != null)
            {
                if (subdivisions > 1)
                {
                    seglen = Pather.getSegLenBasedOnSubdivision(planPaths, subdivisions);
                }
                else if (plan_p.subdivision > 0)
                {
                    seglen = Pather.getSegLenBasedOnSubdivision(planPaths, (int)plan_p.subdivision);

                }
                planPaths = Pather.segmentPaths(planPaths, seglen, planIsClosed);


            }







            AXTexCoords texy = new AXTexCoords(po_axTex);
			if (texLockToTransY)
				texy.shift.y += transY/texy.scale.y;


				
			
			
			
			
			
			Mesh sidesMesh 			= null;
			Mesh backsidesMesh 		= null;
			Mesh topMesh 			= null;
			Mesh botMesh 			= null;


			
			PlanSweeper planSweeper = new PlanSweeper();						
			
			int itemNo = 0;

			
			AXMesh tmpAXmesh;
			
			
			
			
			
			
			

			
			//Matrix4x4 tmx = Matrix4x4.identity;//parametricObject.getTransMatrix();
			
			
			GameObject 	returnGO = null;
			
			if (planPolyTree.Childs.Count > 1)
			{
				//returnGO = new GameObject(parametricObject.Name);
				returnGO = ArchimatixUtils.createAXGameObject(parametricObject.Name, parametricObject);
			}
			/*
			returnGO.transform.localScale 	= Vector3.one; //GetScale(mx);
			returnGO.transform.rotation 	= AXUtilities.QuaternionFromMatrix(tmx);
			returnGO.transform.position 	= AXUtilities.GetPosition(tmx);
			*/

			// DEFAULT SECTION CAP
			Mesh endCapMeshDefault = null;
			if (sec_p != null && (capA || capB))
			{
				// Make a default polygon cap with same material
				if (sec_p.polyTree != null)
					endCapMeshDefault = AXPolygon.triangulate(sec_p.polyTree, endCapTex);
				else
					endCapMeshDefault = AXPolygon.triangulate(sec_p.paths, endCapTex);
			}	

			
			//Debug.Log("GET 1 secPaths...");
			Paths secPaths = (sec_p != null) ? sec_p.getPaths() : null;
			//Debug.Log("GET 2 secPaths = " + secPaths);
			
			foreach(PolyNode node in planPolyTree.Childs)
			{


				//Debug.Log ("polytree");
				// Here, the Contour is a solid, say for example, a brick rectangle in a brickwall
				itemNo++;

				List<AXMesh> ax_meshesTmp 		= new List<AXMesh>();
				sidesMesh 		= null;
				backsidesMesh 	= null;
				topMesh 		= null;
				botMesh 		= null;



				// Use CONTOUR to create this planSpline
				Path p = (seglen > 0) ? Pather.segmentPath(node.Contour,  seglen, planIsClosed) : node.Contour;





		
			



 









				Spline planSpline 		= new Spline(p, planIsClosed, plan_p.breakGeom, plan_p.breakNorm);

				if (secPaths != null)
				{
					foreach (Path sectionPath in secPaths)
					{				
						//Path sp = (seglen > 0) ? Pather.segmentPath(sectionPath,  seglen/2) : sectionPath;
						Path sp = sectionPath;
						Spline sectionSpline 	= new Spline(sp, sectionIsClosed,  sec_p.breakGeom, sec_p.breakNorm);

						if (sectionSpline.controlVertices.Count < 2) 
							continue;

						float topx;
						float topy;
						float botx;
						float boty;

						float y1 = sectionSpline.controlVertices.First().y;
						float y2 = sectionSpline.controlVertices.Last().y;

						if (y1 < y2)
						{
							boty = y1;
							botx = sectionSpline.controlVertices.First().x;

							topy = y2;
							topx = sectionSpline.controlVertices.Last().x;
						}
						else
						{
							boty = y2;
							botx = sectionSpline.controlVertices.Last().x;

							topy = y1;
							topx = sectionSpline.controlVertices.First().x;
						}




						Matrix4x4 botCapTransform 		= Matrix4x4.TRS (new Vector3(0, boty, 0), Quaternion.identity, Vector3.one);
						//Matrix4x4 topCapTransform 		= Matrix4x4.TRS (new Vector3(0, topy, 0), Quaternion.identity, Vector3.one);

						// @ ---- SIDES
						if (hasSides)
						{
							sidesMesh = new Mesh();
							try
							{
								planSweeper.generate(ref sidesMesh, planSpline, sectionSpline, texy);
							} catch {
								Debug.Log("Too many vertices. Try reducing the size of your mesh.");
							}
							sidesMesh.RecalculateBounds();
							sidesMesh.name 		= parametricObject.Name+"_sides";


							tmpAXmesh 	= new AXMesh( sidesMesh, Matrix4x4.identity, po_mat);
							tmpAXmesh.name 		= parametricObject.Name+"_sides_"+itemNo;
							tmpAXmesh.makerPO 	= parametricObject;					
							ax_meshesTmp.Add (tmpAXmesh);
							
							
							// ADD HOLES TO POLYGON
							//Debug.Log("SUBNODERS>>>>>"+node.Childs.Count);
							
							foreach(PolyNode subnode in node.Childs)
							{						
								Spline subplanSpline 		= new Spline(subnode.Contour, planIsClosed, plan_p.breakGeom, plan_p.breakNorm);

								Mesh holeMesh = new Mesh();
								try {
									planSweeper.generate(ref holeMesh, subplanSpline, sectionSpline, texy);
								} catch {
									Debug.Log("Too many vertices. Try reducing the size of your mesh.");
								}


								holeMesh.RecalculateBounds();
								holeMesh.name 		= parametricObject.Name+"_hole";

								tmpAXmesh 	= new AXMesh( holeMesh, Matrix4x4.identity, po_mat);
								tmpAXmesh.name 		= parametricObject.Name+"_sides_"+itemNo;
								tmpAXmesh.makerPO 	= parametricObject;					
								ax_meshesTmp.Add (tmpAXmesh);
								
							}
							
						}


						// @ ---- BACK_FACES
						if (hasBackfaces)
						{
							backsidesMesh = new Mesh();
							planSweeper.generate(ref backsidesMesh, planSpline, sectionSpline.Reverse(), texy);
							backsidesMesh.RecalculateBounds();

							tmpAXmesh 	= new AXMesh( backsidesMesh, Matrix4x4.identity, po_mat);
							tmpAXmesh.name 		= parametricObject.Name+"_backfaces_"+itemNo;
							tmpAXmesh.makerPO 	= parametricObject;					
							ax_meshesTmp.Add (tmpAXmesh);

							foreach(PolyNode subnode in node.Childs)
							{			
											
								Spline subplanSpline 		= new Spline(subnode.Contour, planIsClosed, plan_p.breakGeom, plan_p.breakNorm);

								Mesh holeMesh = new Mesh();
								planSweeper.generate(ref holeMesh, subplanSpline, sectionSpline, texy);
								holeMesh.RecalculateBounds();

								tmpAXmesh 	= new AXMesh( holeMesh, Matrix4x4.identity, po_mat);
								tmpAXmesh.name 		= parametricObject.Name+"_faces_"+itemNo;
								tmpAXmesh.makerPO 	= parametricObject;					
								ax_meshesTmp.Add (tmpAXmesh);
								
							}

						}



						// @ ---- TOP_CAP
						if (topCap)
						{
							if (topx != 0)
							{
								
								Paths paths = new Paths();
								paths.Add(node.Contour);
								foreach(PolyNode subnode in node.Childs)
									paths.Add(subnode.Contour);
								
								AXClipperLib.PolyTree resPolytree = new AXClipperLib.PolyTree();

								ClipperOffset 	co  = new ClipperOffset ();

								co.AddPaths (AXGeometry.Utilities.cleanPaths(paths) , AXClipperLib.JoinType.jtMiter, AXClipperLib.EndType.etClosedPolygon); //JoinType.jtSquare, AXClipperLib.EndType.etClosedPolygon);
								co.Execute (ref resPolytree, (double)(topx * AXGeometry.Utilities.IntPointPrecision));
								if (resPolytree != null && resPolytree.Childs != null && resPolytree.Childs.Count > 0)
									topMesh = AXPolygon.triangulatePolyNode(resPolytree.Childs[0], topCapTex, seglen, useSubdivisionGrid);
							}
							else 
							{
								
								topMesh = AXPolygon.triangulatePolyNode(node, topCapTex, seglen, useSubdivisionGrid);
							}
							//topMesh = AXPolygon.triangulatePolyNode(node, endCapTex);
							
							if (topMesh != null)
							{
								
								if (! planIsCCW  || hasBackfaces)
									topMesh.triangles = topMesh.triangles.Reverse ().ToArray ();
								topMesh.RecalculateNormals();


								// Raise up

								Vector3[] verts = new Vector3[topMesh.vertices.Length];
								for (int ii=0; ii<topMesh.vertices.Length; ii++)
								{
									verts[ii] = new Vector3(topMesh.vertices[ii].x, topMesh.vertices[ii].y + topy, topMesh.vertices[ii].z);
								}
								topMesh.vertices = verts;

								tmpAXmesh = new AXMesh( topMesh, Matrix4x4.identity, topCap_mat);
								tmpAXmesh.name 		= parametricObject.Name+"_topCap_"+itemNo;
                                tmpAXmesh.makerPO = parametricObject;
                                tmpAXmesh.getsCollider = parametricObject.isRigidbody ? false : true;
								ax_meshesTmp.Add (tmpAXmesh);
							}
						}

						// @ ---- BOTTOM_CAP
						if (botCap)
						{
							//AXClipperLib.PolyTree resPolytree = node;
							if (botx != 0)
							{
								//Debug.Log("here 2 a");
								Paths paths = new Paths();
								paths.Add(node.Contour);
								foreach(PolyNode subnode in node.Childs)
									paths.Add(subnode.Contour);
								
								AXClipperLib.PolyTree resPolytree = new AXClipperLib.PolyTree();
								ClipperOffset 	co  = new ClipperOffset ();
								co.AddPaths (AXGeometry.Utilities.cleanPaths(paths), AXClipperLib.JoinType.jtMiter, AXClipperLib.EndType.etClosedPolygon); //JoinType.jtSquare, AXClipperLib.EndType.etClosedPolygon);
								co.Execute (ref resPolytree, (double)(botx * AXGeometry.Utilities.IntPointPrecision));
								if (resPolytree != null && resPolytree.Childs != null && resPolytree.Childs.Count > 0)
									botMesh = AXPolygon.triangulatePolyNode(resPolytree.Childs[0], botCapTex, seglen, useSubdivisionGrid);
							}
							else 
								botMesh = AXPolygon.triangulatePolyNode(node, botCapTex, seglen, useSubdivisionGrid);

							
							if (botMesh != null)
							{
								if (planIsCCW && ! hasBackfaces)
									botMesh.triangles = botMesh.triangles.Reverse().ToArray();
								botMesh.RecalculateNormals();
								
								tmpAXmesh = new AXMesh( botMesh, botCapTransform, botCap_mat);
								tmpAXmesh.name 		= parametricObject.Name+"_botCap_"+itemNo;
                                tmpAXmesh.makerPO = parametricObject;
                                tmpAXmesh.getsCollider = parametricObject.isRigidbody ? false : true;
								ax_meshesTmp.Add (tmpAXmesh);
							}
						}


						// @ --- CAP_A
						if (capA && endCapMeshDefault != null)
						{
							Matrix4x4 capATransform = AXGeometry.Utilities.getEndTransformA(node.Contour);

							tmpAXmesh = new AXMesh( endCapMeshDefault, capATransform, endCap_mat);
							tmpAXmesh.name 		= parametricObject.Name+"_endCapA";
                            tmpAXmesh.makerPO = parametricObject;
                            tmpAXmesh.getsCollider = parametricObject.isRigidbody ? false : true;

							ax_meshesTmp.Add (tmpAXmesh);

							 
							if (hasEndCapSourceMeshes &&  P_EndCapMaterial.DependsOn != null && P_EndCapMaterial.DependsOn.meshes != null)
							{
								for (int mi = 0; mi < P_EndCapMaterial.DependsOn.meshes.Count; mi++) {
									AXMesh dep_amesh = P_EndCapMaterial.DependsOn.meshes [mi];
									AXMesh clone = dep_amesh.Clone (capATransform * dep_amesh.transMatrix);
									ax_meshesTmp.Add (clone);
								}
							}
						}

						// @ --- CAP_B
						if (capB && endCapMeshDefault != null)
						{
							Matrix4x4 capBTransform = AXGeometry.Utilities.getEndTransformB(node.Contour);
								
							// because drawmesh cannot do negative scaling
							// the workaround is to bake the scaling in...
							AXMesh tmpAXmesh2 = new AXMesh(AXMesh.freezeWithMatrix(endCapMeshDefault, capBTransform), Matrix4x4.identity, endCap_mat);
                            tmpAXmesh2.makerPO = parametricObject;
                            ax_meshesTmp.Add (tmpAXmesh2);
							
							if (hasEndCapSourceMeshes  &&  P_EndCapMaterial.DependsOn != null && P_EndCapMaterial.DependsOn.meshes != null)
							{
								for (int mi = 0; mi < P_EndCapMaterial.DependsOn.meshes.Count; mi++) {
									AXMesh dep_amesh = P_EndCapMaterial.DependsOn.meshes [mi];
									tmpAXmesh2 = dep_amesh.Clone ();
									tmpAXmesh2.mesh = AXMesh.freezeWithMatrix (dep_amesh.mesh, dep_amesh.transMatrix.inverse * capBTransform * dep_amesh.transMatrix);
									ax_meshesTmp.Add (tmpAXmesh2);
								}
							}
						}
					}
				}
				else
				{
					// just top or bottom caps


						// @ ---- TOP_CAP
						if (topCap)
						{

						//Debug.Log("Topcap B");
							topMesh = AXPolygon.triangulatePolyNode(node, topCapTex, seglen, useSubdivisionGrid);
							if (! planIsCCW  || hasBackfaces)
								topMesh.triangles = topMesh.triangles.Reverse().ToArray();

							topMesh.RecalculateNormals();
							topMesh.RecalculateBounds();

							//topMesh = AXPolygon.triangulatePolyNode(node, endCapTex);
							
							if (topMesh != null)
							{

								tmpAXmesh = new AXMesh( topMesh, Matrix4x4.identity, topCap_mat);
								tmpAXmesh.name 		= parametricObject.Name+"_topCap_"+itemNo;
                            tmpAXmesh.makerPO = parametricObject;
                            tmpAXmesh.getsCollider = parametricObject.isRigidbody ? false : true;

							ax_meshesTmp.Add (tmpAXmesh);
							}
						}

						// @ ---- BOTTOM_CAP
						if (botCap)
						{
							//AXClipperLib.PolyTree resPolytree = node;
						
							botMesh = AXPolygon.triangulatePolyNode(node, topCapTex, seglen, useSubdivisionGrid);
							
							if (botMesh != null)
							{
								if (planIsCCW && ! hasBackfaces)
									botMesh.triangles = botMesh.triangles.Reverse().ToArray();
								botMesh.RecalculateNormals();
								
								tmpAXmesh = new AXMesh( botMesh, Matrix4x4.identity, botCap_mat);
								tmpAXmesh.name 		= parametricObject.Name+"_botCap_"+itemNo;
                            tmpAXmesh.makerPO = parametricObject;
                            tmpAXmesh.getsCollider = parametricObject.isRigidbody ? false : true;
								ax_meshesTmp.Add (tmpAXmesh);
							}
						}
				}


				// MAKE GAMEOBJECT



				GameObject  obj = parametricObject.makeGameObjectsFromAXMeshes(ax_meshesTmp, false);
				ax_meshes.AddRange(ax_meshesTmp);
				//if (! parametricObject.isRigidbody)
				//	obj.isStatic = true;


				//parametricObject.addCollider (obj);

				/*
				tmx = parametricObject.generator.localMatrix;// * parametricObject.getAxisRotationMatrix().inverse;
				obj.transform.rotation 		= AXUtilities.QuaternionFromMatrix(tmx);
				obj.transform.position 		= AXUtilities.GetPosition(tmx);
				obj.transform.localScale 	= parametricObject.getLocalScale(); 
				*/

				//f (planPolyTree.Childs.Count > 1)
				//	obj.transform.parent = returnGO.transform;
				
				if (planPolyTree.Childs.Count == 1)
					returnGO = obj;
				else 
					obj.transform.parent = returnGO.transform;

				
				
				// Continue on down the polytree structure
				if (node != null && node.Childs != null && node.Childs.Count > 0)
				{
					for (int i = 0; i < node.Childs.Count; i++) {
						PolyNode subnode = node.Childs [i];

						//AXClipperLib.PolyTree secPolyTree = (sec_p != null) ? sec_p.polyTree : null;
						GameObject childObj = generateGameObjectsFromPlanAndSecPolytrees (initiator_po, plan_p, subnode, sec_p, secPolyTree, ++gen);
						if (childObj != null) {
							//childObj.transform.localScale 	= new Vector3(1, 1, 1); //GetScale(mx);
							//childObj.transform.rotation 		= AXUtilities.QuaternionFromMatrix(tmx);
							//childObj.transform.position 		= AXUtilities.GetPosition(tmx);
							childObj.transform.parent = obj.transform;
						}
					}
				}
			} // \EACH POLYNODE
			
			return returnGO;
			
		}








		public void setSectionHandleTransform(AXParameter plan_p)
		{
			// BEG SECTION HANDLE TRANSFORM ===================================================================


			Paths planPaths = plan_p.getPaths ();

			if (planPaths != null && planPaths.Count > 0 && planPaths [0] != null) {
				Path firstPlanPath = planPaths [0];

				if (firstPlanPath.Count > 1) 
				{
					Vector3 p0 = AXGeometry.Utilities.IntPt2Vec3 (firstPlanPath [0]);
					Vector3 p1 = AXGeometry.Utilities.IntPt2Vec3 (firstPlanPath [1]);
					Vector3 secLocalOrigin = Vector3.Lerp (p1, p0, .5f);
					
					
					Vector3 v = p0 - p1;
					Quaternion rot = Quaternion.identity;
					//rot.SetFromToRotation(v, Vector3.down);
					rot.SetLookRotation (Vector3.up, v);
					
					//Debug.Log ("HANDLE: " + p0 + " :: " + p1 + ", " + v + ", rot = " + rot.eulerAngles.y);
					
					sectionHandleTransform = Matrix4x4.TRS (secLocalOrigin, rot, Vector3.one);

				}
				
			}



			// END SECTION HANDLE TRANSFORM ===================================================================
		}

		/*
		public override Matrix4x4 getLocalConsumerMatrixPerInputSocket(AXParametricObject input, AXParameter input_p=null)
		{
			
			if (parametricObject.hasInputSplineReady("Plan"))
			{
				AXParameter plan  = parametricObject.getParameter("Plan");
				
				
				if (input == plan.DependsOn.Parent)
					return Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90, 0, 0), Vector3.one);
				
				return Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(-90, 0, 0), Vector3.one);
				
			}
			
			return Matrix4x4.identity;
		}
		*/


	}




	// EXTRUDE GENERATOR
	public class MesherExtruded : Mesher, IMeshGenerator
	{

		// INPUTS
		public AXParameter 	P_Sides;
		public AXParameter 	P_Backfaces;

		public AXParameter 	P_EndCapA;
		public AXParameter 	P_EndCapB;

		public AXParameter P_LipTop;
		public AXParameter P_LipBottom;
		public AXParameter P_LipEdge;
		public AXParameter P_LipEdgeBottom;

		public float 	lipTop 			= 0;
		public float 	lipBottom 		= 0;
		public float 	lipEdge 		= 0;
		public float 	lipEdgeBottom 	= 0;



		public override void init_parametricObject() 
		{


			P_Sides 	= parametricObject.addParameter(AXParameter.DataType.Bool, 	"Sides", 		true);
			P_Backfaces 	= parametricObject.addParameter(AXParameter.DataType.Bool, 	"Backfaces", 		false);

			base.init_parametricObject();


		}

		// POLL INPUTS (only on graph change())
		public override void pollInputParmetersAndSetUpLocalReferences()
		{
			base.pollInputParmetersAndSetUpLocalReferences();

			P_TopCapMaterial = parametricObject.getParameter("Top Cap Material");

			P_EndCapMaterial = parametricObject.getParameter("End Cap Material");

			P_EndCapMesh = parametricObject.getParameter("End Cap Mesh");

			P_Sides = parametricObject.getParameter("Sides");
			P_Backfaces = parametricObject.getParameter("Backfaces", "Insides");

			P_EndCapA = parametricObject.getParameter("End Cap A", "EndCap A", "Cap A", "endCap");
			P_EndCapB = parametricObject.getParameter("End Cap B", "EndCap B", "Cap B", "begCap");


			P_LipTop		= parametricObject.getParameter("Lip Top");
			P_LipBottom		= parametricObject.getParameter("Lip Bottom");
			P_LipEdge		= parametricObject.getParameter("Lip Edge");
			P_LipEdgeBottom	= parametricObject.getParameter("Lip Edge Bottom");

		}


		public override void pollControlValuesFromParmeters()
		{
			if (parametersHaveBeenPolled)
				return;

			base.pollControlValuesFromParmeters();

			//po_tex 	= parametricObject.getAXTex();

			//Debug.Log("tex="+tex);




			// END CAP MESH


			hasEndCapSourceMeshes = (P_EndCapMesh != null && P_EndCapMesh.DependsOn != null && P_EndCapMesh.DependsOn.meshes != null && P_EndCapMesh.DependsOn.meshes.Count > 0) ? true : false;
				
							
			
			//Debug.Log("1 ****** endCapHasOwnMat="+endCapHasOwnMat+", topCapHasOwnMat="+topCapHasOwnMat);

			

			
			
			hasSides 	= (P_Sides != null) ? P_Sides.boolval : true;
			hasBackfaces  = (P_Backfaces != null) ? P_Backfaces.boolval : false;


			capA 		= (P_EndCapA != null) ? P_EndCapA.boolval : false;
			capB 		= (P_EndCapB != null) ? P_EndCapB.boolval : false;

			lipTop 				= (P_LipTop 	!= null)	?	P_LipTop.FloatVal : 0;
			lipBottom 			= (P_LipBottom 	!= null)	?	P_LipBottom.FloatVal : 0;
			lipEdge 			= (P_LipEdge 	!= null)	?	P_LipEdge.FloatVal : 0;
			lipEdgeBottom 		= (P_LipEdgeBottom 	!= null)	?	P_LipEdgeBottom.FloatVal : 0;


		}







	}





		



	/*

	// WINWALL GENERATOR

	public class WinWall : Mesher, IMeshGenerator
	{

		// INIT
		public override void init_parametricObject() 
		{

			base.init_parametricObject();

			// Parameters
			Debug.Log ("INIT WINWALL");
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, "Header Spline"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, "Sill Spline"));

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, "Output Mesh"));


			// Handles

			// Code
		}
		
		
		// WIN_WALL GENERATE
		public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool isReplica)
		{
			if (parametricObject == null || ! parametricObject.isActive)
				return null;
			
			preGenerate();

			List<AXMesh> ax_meshes = new List<AXMesh>();

			AXWinWall ww = new AXWinWall();
			
			AXParameter sec = parametricObject.getParameter("Header Spline");
			if (sec != null && sec.DependsOn != null)
			{
				ww.headerPO	 = sec.DependsOn.Parent;
				ww.headerSec = sec.DependsOn.spline;
			}
			AXParameter sill_sec = parametricObject.getParameter("Sill Spline");
			if (sill_sec != null && sill_sec.DependsOn != null)
			{
				ww.sillPO	 = sill_sec.DependsOn.Parent;
				ww.sillSec 	 = sill_sec.DependsOn.spline;
			}
			
			ww.mat = po_mat;
			ww.tex = po_tex;
			
			ww.innerDepth = parametricObject.floatValue("innerDepth");
			ww.outerDepth = parametricObject.floatValue("outerDepth");
			
			ww.xlen = parametricObject.floatValue("xlen");
			ww.ylen = parametricObject.floatValue("ylen");
			
			ww.bay_x = parametricObject.floatValue("bay_X");
			ww.bay_y = parametricObject.floatValue("bay_Y");
			
			ww.win_x = parametricObject.floatValue("win_X");
			ww.win_y = parametricObject.floatValue("win_Y");
			
			ax_meshes = ww.generate();
			
			
			parametricObject.finishMultiAXMeshAndOutput(ax_meshes, isReplica);

			return null;
		}
	}
	*/


}

