
using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
//using System.Diagnostics;

using AXClipperLib;
using Path 			= System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths 		= System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;

using AXPoly2Tri;
using PolygonPoints = System.Collections.Generic.List<AXPoly2Tri.PolygonPoint>;



namespace AXGeometry
{
	public class PlanSweeper  {
	 
		public List<Vector3> debugPoints;
		public List<Vector3> debugVectors;

		public Spline topCapSpline;
		public Spline botCapSpline;



        public bool autoBevelOpenEnds = true;


        bool blendToXY = false;


        // GENERATE
        /*
		public void generate (ref Mesh mesh, Path planPath, Path sectionPath, AXTex tex)
		{
			Spline planSpline = new Spline(planPath);
			Spline sectionSpline = new Spline(sectionPath);

			generate (ref mesh, planSpline, sectionSpline, tex);
		}
	*/

        public void generate (ref Mesh mesh, Spline planSpline, Spline sectionSpline, AXTexCoords tex)
		{
			/*
			 	When generating a PlanSweep, we need to know the breaking angles of each plan layer.
			 	This is because the original plan node might be convex at a certain section offset, it is concave, depending on how far out that section goes. 
			 	
			 	To do this:
			 	1. for each section node, do an offset using clipper - or use bevelAngles of the orginnal plan and make a new spline, whichever is more efficient
			 	2. store these offset plans as AX.Splines in a list.
			 	3. use these Splines for the isSharp and isBlend conditionals
			 
			 */

			if ( planSpline == null  ||   planSpline.controlVertices == null 	||    planSpline.controlVertices.Count == 0)
				return;

			if (sectionSpline == null || sectionSpline.controlVertices == null 	|| sectionSpline.controlVertices.Count == 0)
				return;

			if (tex == null)
				return;
            //Debug.Log("PlanSweeper::generate");

			//Debug.Log(sectionSpline.controlVertices.Count);

			//AXGeometry.Utilities.printPath(AXGeometry.Utilities.Spline2Path(planSpline));

			StopWatch sw = new StopWatch();


			// Combine the plan and sections to make the fabric
			float uShift = tex.shift.x;
			float vShift = tex.shift.y;
			float uScale = tex.scale.x;
			float vScale = tex.scale.y;

		


			// *** VERTICES 	***

			// MAKE ALL VERTICES - Weave the Fabric
			// *** VERTICES
			int vertSize 					= planSpline.derivedVertices.Count * sectionSpline.derivedVertices.Count ;

			// VERTICES
			Vector3[] 		vertices 		= new Vector3[vertSize];
			int 			verticesCt 		= 0;


			// UV			*** 
			Vector2[] 		uv 				= new Vector2[vertSize];
			int				uvCt			= 0;

			// COLORS
			//Color[] colors 					= new Color[vertSize];



			// *** TRIANGLES	***

			int planSubber 		= (planSpline.shapeState 	== ShapeState.Open ? 1 : 0);
			int sectionSubber 	= (sectionSpline.shapeState == ShapeState.Open ? 1 : 0);

			//int 		trianglesSizeWah 	=  3*planSpline.derivedVertices.Count  * ( sectionSpline.derivedVertices.Count - 1 );
			int 		trianglesSize		= 3 * 2 * ((planSpline.controlVertices.Count-planSubber) * (sectionSpline.controlVertices.Count-sectionSubber));
			int[]			triangles 		= new int[trianglesSize];
			int 			triCt			= 0;


			 

			float  plan_u = 0;

			//float plan_u_l = 0;

			// left of the current control point
			float plan_u_L = uShift + planSpline.length / uScale;

			// right of the current contol point
			float plan_u_R = 0;

			float  v = 0;


			//float uScalerAfter  	= 1f;

			float bevelU = 0;
			float bevelMultiplier = 0;

			/*
				In this, we step through the section and add each vert of the spline transformed for that joint in the plan.
				
				We calculate the u,v for both left and right sides of the spline joint, 
				but use one or the other when copying the spline rib into the 
				final lists. 
			
			 */


			int planCount 	= (planSpline.shapeState == ShapeState.Closed ? planSpline.controlVertices.Count : planSpline.controlVertices.Count-1);
			int planlen 	= planSpline.derivedVertices.Count;

			int secCount  	= (sectionSpline.isClosed ? sectionSpline.controlVertices.Count : sectionSpline.controlVertices.Count-1);
			int seclen  	= sectionSpline.derivedVertices.Count;


			topCapSpline = new Spline();
			botCapSpline = new Spline();

			float samePointTolerence = .001f;



			sw.milestone("setup");









			// ***** EACH PLAN NODE *****



			//List<Vector3> 	quadNormals 		= new List<Vector3>();

			// We need this outside of the loop.
			Vector3[] 	ribVertices0 		= null;

			int terminIndexSubtractor = 0;
			if (Vector2.Distance(planSpline.controlVertices[0], planSpline.controlVertices[planSpline.controlVertices.Count-1]) < samePointTolerence)
				terminIndexSubtractor = 1;

			//Vector3 prevPlanPoint;

			for (int i=0; i<planSpline.controlVertices.Count-terminIndexSubtractor; i++)
			{
				
				
				// RIB_VERTICES
				Vector3[] 	ribVertices 		= new Vector3[sectionSpline.derivedVertices.Count]; //new List<Vector3>();
				int 		ribVerticesCt		= 0;

				List<Vector2>  rib_UV_Left 		= new List<Vector2>();
				List<Vector2>  rib_UV_Right 		= new List<Vector2>();


				Vector3 vert0 = Vector3.zero;


				// Remeber that plan_u_L and plan_u_R are athe same control point

				// RUNNING_U
				if (tex.runningU)
				{
					// The plan_u is the u value running around the spline at the spline.
					// This is used as the base for the beveled textures.
					plan_u = uShift + planSpline.running_distances[i] / uScale;
					plan_u_L = plan_u;
					plan_u_R = plan_u;
				}

				// NOT RUNNING_U
				else
				{
					// LEFT OF CONTROL POINT
					int prev_i = (i==0) ? planSpline.controlVertices.Count-1 : i-1;
					plan_u_L = uShift + planSpline.edgeLengths[prev_i] / uScale;

					// RIGHT OF CONTROL POINT (ALWAYS RESETS)
					plan_u_R = uShift;
				}





				//Debug.Log ( " >> ["+i+"] plan_u="+plan_u+ ", bevelAngle="+planSpline.bevelAngles[i]+", Tan="+Mathf.Tan( planSpline.bevelAngles[i] * Mathf.Deg2Rad   ) +", " + planSpline.jointIsAcute(i));


				// ** BEVEL_MULTIPLIER
				bevelMultiplier = Mathf.Tan( planSpline.bevelAngles[i] * Mathf.Deg2Rad );


				/*
				for (int j = 0; j < planSpline.bevelAngles.Count; j++) {
					float ang = planSpline.bevelAngles [j];
					//Debug.Log ("bevelAngle["+j+"] = " + ang);
				}
				*/

				Matrix4x4 bevelTransform = planSpline.nodeTransforms[i];

				if (planSpline.shapeState == ShapeState.Open)
				{
					if (i==0)
						bevelTransform = planSpline.begTransform;
					else if (i==planSpline.controlVertices.Count-1)
						bevelTransform = planSpline.endTransform;
				}	


				//float tmp = .5f;


				{	// **** EACH SECTION NODE *****

					// Calculate a rib, an instance of the section uniquely adjusted by joint transform, as well as its possible UVs
					// The vert is transforms, as is a u value to be either added or subtracted from the plan_u
					for (int j=0; j<sectionSpline.controlVertices.Count; j++)
					{

						//float offset = sectionSpline.controlVertices[j].x;


						//Debug.Log (i+","+j);

						// Transform plan vert
						Vector3 vert = bevelTransform.MultiplyPoint( new Vector3(sectionSpline.controlVertices[j].x,  0, sectionSpline.controlVertices[j].y) ) ;

//						if (i == 0)
//							Debug.Log (vert.x.ToString("F4") + ", " + vert.y.ToString("F4"));


						if (j == 0)
						{
							//Debug.Log ("adding vert: " + new Vector2(vert.x, vert.z));
							botCapSpline.controlVertices.Add(new Vector2(vert.x, vert.z));
						}
						if (j == sectionSpline.controlVertices.Count-1)
						{
							//Debug.Log ("adding vert: " + new Vector2(vert.x, vert.z));
							topCapSpline.controlVertices.Add(new Vector2(vert.x, vert.z));
						}

						if (j == 0)
							vert0 = vert;

						// Determine UV coordinates
						bevelU = 0;

						if ( planSpline.bevelJointIsAcute(i) )
							bevelU =  sectionSpline.controlVertices[j].x * bevelMultiplier / uScale;

						//Debug.Log ("i,j="+i+","+j+", bevelU="+bevelU);

						v = vShift + sectionSpline.running_distances[j] / vScale;


						ribVertices[ribVerticesCt++] = vert; // ribVertices.Add(vert);
						rib_UV_Left.Add  (new Vector2(plan_u_L+bevelU, v));
						rib_UV_Right.Add (new Vector2(plan_u_R-bevelU, v));

						// sharp edge in section, add another vert
						//Debug.Log ("breaking="+sectionSpline.breakingAngles[j]+", isSharp? "+sectionSpline.isSharp(j));
						if ( sectionSpline.isSharp(j)     &&    ( (j > 0 && j < (sectionSpline.controlVertices.Count-1)) || (j==(sectionSpline.controlVertices.Count-1) && sectionSpline.shapeState == ShapeState.Closed) ) ) // ADD ANOTHER DUPLICATE RIB
						{
							ribVertices[ribVerticesCt++] = vert; // ribVertices.Add(vert);
							rib_UV_Left.Add  (new Vector2(plan_u_L+bevelU, v));
							rib_UV_Right.Add (new Vector2(plan_u_R-bevelU, v));
						}	
					}



					// LAST 
					if (sectionSpline.isClosed)
					{
						bevelU = 0;

						bevelU =  sectionSpline.controlVertices[0].x * bevelMultiplier / uScale;

						v = vShift + sectionSpline.length / vScale;


						ribVertices[ribVerticesCt++] = vert0; // ribVertices.Add(vert0);
						if (i>0 && i<(planSpline.controlVertices.Count-1))
						{
							rib_UV_Left.Add  (new Vector2(plan_u_L+bevelU, v));
							rib_UV_Right.Add (new Vector2(plan_u_R-bevelU, v));
						}
						else 
						{ 
							rib_UV_Left.Add  (new Vector2(plan_u_L+bevelU, v));
							rib_UV_Right.Add (new Vector2(plan_u_R-bevelU, v));

						}

					}


				}


                // ADD RIB to vertices and uvs
				for (int k = 0; k < ribVertices.Length; k++) 
				{
					vertices[verticesCt++] = ribVertices [k];

					if (i > 0)
						uv[uvCt++] = rib_UV_Left[k];// uv.Add(rib_UV_Left[k]);
					else
                    {
                        
                            uv[uvCt++] = rib_UV_Right[k];// uv.Add(rib_UV_Right[k]);
                        

                    }
						
				}	

				// ADD RIB AGAIN if sharp edge in plan, or if this is last plan joint and plan is closed

				if ( planSpline.isSharp(i)     &&    ( (i > 0 && i < (planSpline.controlVertices.Count-1)) || (i==(planSpline.controlVertices.Count-1) && planSpline.shapeState == ShapeState.Closed) ) ) // ADD ANOTHER DUPLICATE RIB
				{
					for (int k = 0; k < ribVertices.Length; k++) 
					{
						vertices[verticesCt++] = ribVertices [k];
						uv[uvCt++] = rib_UV_Right[k]; // uv.Add(rib_UV_Right[k]);
					}
				}

				if (i==0)
					ribVertices0 = ribVertices;




				sw.milestone("-- each plan vert");
			} // \ EACH PLAN CONTROL VERT



			for( int i=0; i<vertices.Length; i++)
			{
				if (float.IsNaN(vertices[i].x) || float.IsNaN(vertices[i].y) || float.IsNaN(vertices[i].z))
					return;;
			}





			bevelMultiplier = Mathf.Tan( planSpline.bevelAngles[0] * Mathf.Deg2Rad );



			// LAST RIB
			if (planSpline.shapeState == ShapeState.Closed)
			{
				// add one more rib that mirrors first rib but with new UV
				// add rib to vertices

				bevelU = 0;

				for (int k = 0; k < ribVertices0.Length; k++) 
				{
					if (k < ribVertices0.Length && (vertices.Length-1) >= verticesCt)
						vertices[verticesCt++] = ribVertices0 [k];//vertices.Add (ribVertices0 [k]);
				}

				// UVS FOR LAST RIB AT LOCATION 0
				if (tex.runningU)
					plan_u_L = uShift + planSpline.length / uScale;

				//plan_u_L = uShift + planSpline.edgeLengths[planSpline.controlVertices.Count-1] / uScale;


				/*
				foreach(Vector2 d in planSpline.controlVertices)
					Debug.Log (d);
				Debug.Log ("-------");
				foreach(float d in planSpline.edgeLengths)
					Debug.Log (d);
				*/

				// Make a new rib	


				for (int j=0; j<sectionSpline.controlVertices.Count; j++)
				{
					if ( planSpline.bevelJointIsAcute(0) )
						bevelU =  sectionSpline.controlVertices[j].x * bevelMultiplier / uScale;
					v = vShift + sectionSpline.running_distances[j] / vScale;

					uv[uvCt++] = new Vector2(plan_u_L + bevelU, v); // uv.Add(new Vector2(plan_u_L + bevelU, v));

					if ( sectionSpline.isSharp(j)     &&    ( (j > 0 && j < (sectionSpline.controlVertices.Count-1)) || (j==(sectionSpline.controlVertices.Count-1) && sectionSpline.isClosed) ) ) // ADD ANOTHER DUPLICATE RIB
					{
						uv[uvCt++] = new Vector2(plan_u_L + bevelU, v); // uv.Add(new Vector2(plan_u_L + bevelU, v));

					}
				}
				if (sectionSpline.isClosed)
				{
					if ( planSpline.bevelJointIsAcute(0) )
						bevelU =  sectionSpline.controlVertices[0].x * bevelMultiplier / uScale;
					v = vShift + sectionSpline.length / vScale;

					uv[uvCt++] = new Vector2(plan_u_L + bevelU, v); // uv.Add(new Vector2(plan_u_L + bevelU, v));
				}

			}

			sw.milestone(" last rib");

			// All vertices are now in the list.
			//for (int i=0; i<vertices.Count; i++)
			//	Debug.Log ("["+i+"] " + vertices[i] + ", uv="+uv[i]);


			// QUADS / TRIANGLES

			int slen = sectionSpline.derivedVertices.Count;
			//Debug.Log (" * * slen="+slen);


			//Debug.Log ("slen="+slen);

			debugPoints = new List<Vector3>();
			debugVectors = new List<Vector3>();

			int p_cur = 0;
			int s_cur = 0;

			int splineLoopCount = sectionSpline.controlVertices.Count-1;
			if(sectionSpline.isClosed)
				splineLoopCount++;
			for (int i=0; i<planCount; i++)
			{

				s_cur = 0;
				for (int j=0; j<splineLoopCount; j++)
				{

					// process this quad

					int LL = p_cur * slen + s_cur;
					int UL = LL + 1;
					int LR = LL + slen;
					int UR = LR + 1;

					// QUAD TRIANGLES: Two triangles per quad...

					triangles[triCt++] = LL;
					triangles[triCt++] = UR;
					triangles[triCt++] = LR;

					triangles[triCt++] = LL;
					triangles[triCt++] = UL;
					triangles[triCt++] = UR;



					// QUAD NORMAL: use quad to calculate its normal (lefthand rule in unity)

					/*
					Vector3 Tv = vertices[UL]-vertices[LL];

					Vector3 Tu = vertices[LR]-vertices[LL];

					// addressing will be i*sectionSpline.controlVerticies.Count + j
					//quadNormals.Add (Vector3.Cross(Tv, Tu).normalized);
					quadTangentsV.Add (Tv);
					quadTangentsU.Add (Tu);
					//Debug.Log ("["+quadCount++ + "] " + LL + ", " + UL + ", " + LR + ", " + UR);
					//Debug.Log (vertices[LL] + ", " + vertices[UL] + ", " + vertices[LR] + ", " + vertices[UR]);
					*/

					// section cursor incrementing
					s_cur++;
					int next_j = (j<sectionSpline.controlVertices.Count-1) ? j+1 : 0;
					if (sectionSpline.isSharp(next_j))
						s_cur++;

					//if (i==0)
					//	Debug.Log ("tri ["+j+"] "+sectionSpline.isSharp(j) + ", s_cur="+s_cur);


				}

				// plan cursor incrementing
				p_cur++;
				int next_i = (i<planSpline.controlVertices.Count-1) ? i+1 : 0;
				//Debug.Log("tris "+i+ ": "+ planSpline.isSharp(i));
				if (planSpline.isSharp(next_i))
					p_cur++;

			}

			//Debug.Log ("triangles.Count="+triangles.Count);

			sw.milestone("triangles set");



			if (tex.rotateSidesTex)
			{
				float tmpval_x;
				for (int i = 0; i < uv.Length; i++)
				{ 
				
					tmpval_x = uv[i].x;
					uv[i].x = uv[i].y;
					uv[i].y =tmpval_x;
				}

				sw.milestone("rotate sides uvs");
			}




			//Mesh mesh = new Mesh();

			// clean verts

			for( int i=0; i<vertices.Length; i++)
			{
				
				if (float.IsNaN(vertices[i].x) || float.IsNaN(vertices[i].y) || float.IsNaN(vertices[i].z))
					return;;
			}

			mesh.vertices 	= vertices;
			mesh.uv 		= uv;
			mesh.triangles 	= triangles;


			sw.milestone("mesh done");


			mesh.RecalculateNormals();

			sw.milestone("recalc normals");















	
			bool doNormals =  ( (planSpline.isBlend(0) && planSpline.shapeState == ShapeState.Closed)
                || planSpline.breakAngleGeom < planSpline.breakAngleNormals || sectionSpline.breakAngleGeom < sectionSpline.breakAngleNormals);

            //Debug.Log("planSpline.isBlend(0)=" + planSpline.isBlend(0) + " " + planSpline.shapeState );
            //Debug.Log(" -- " + planSpline.breakAngleGeom + " < " + planSpline.breakAngleNormals);
            //Debug.Log(" -- " + sectionSpline.breakAngleGeom + " < " + sectionSpline.breakAngleNormals);
            //Debug.Log("doNormals="+ doNormals);


            if (doNormals)
			{

				

				// Adjust normals at the seam if plan is closed and close joint is smooth


				//Debug.Log (" ========= planSpline.isClosed"+planSpline.isClosed+", planSpline.isSharp(0)="+planSpline.isSharp(0) + ", slen="+slen);
				// Find quads for each side of the section seam and 
				// determine if 2 or 4 quads are needed to get the normal for the two points.

				// for each point ObjectNames the seam, eg, section controlPoint,
				// there are either two or four vertices associated with it.

				// If section is smooth at that joint,
				// then use for neighboring quads to get the normal for all four points.

				// If section break, then use lower 2 quads to get lower two points
				// and upper two quads to get upper two points.
				s_cur = 0;
				//Vector3[] normals = new Vector3[mesh.vertices.Length];
				Vector3[] normals = mesh.normals;

                //Debug.Log("blendToXY=" + planSpline.controlVertices[0].y + " " + planSpline.controlVertices[planSpline.controlVertices.Count - 1].y);
                //for(int i=0; i< planSpline.controlVertices.Count; i++)
                //{
                //    Debug.Log(planSpline.controlVertices[i]);
                //}

                // ADDED 4/27/2020
                // BLEND_TO_XY is for shells that meet at XY plane such as a shampoo bottle with different front and back for projections
                if (planSpline.shapeState == ShapeState.Open && Mathf.Approximately(0, planSpline.controlVertices[0].y) && Mathf.Approximately(0, planSpline.controlVertices[planSpline.controlVertices.Count - 1].y))
                    blendToXY = true;
                else
                    blendToXY = false;

                //Debug.Log("blendToXY=" + blendToXY);


                // *** PLAN **********
                p_cur = 0;
				for (int i=0; i<planSpline.controlVertices.Count; i++)
				{
					//int prev_i = (i+planSpline.controlVertices.Count-1) % (planSpline.controlVertices.Count);

					//Debug.Log ("plan: "+prev_i + " ==> [" + i + "] ++"+planSpline.controlVertices[i]+"++======= isSharp="+planSpline.isSharp(i) +", isSeam="+planSpline.isSeam(i) +", isBlend="+planSpline.isBlend(i)+" =====================[" + i + "]");



					int pi = (i == 0) ? planSpline.controlVertices.Count-1: i-1;

					Quaternion planBevelRot =  Quaternion.AngleAxis((planSpline.nodeRotations[i]),  	Vector3.up);
					Quaternion planRightRot =  Quaternion.AngleAxis((planSpline.edgeRotations[i]+90+0),	Vector3.up);
					Quaternion planLeftRot 	=  Quaternion.AngleAxis((planSpline.edgeRotations[pi]+90-0),	Vector3.up);



					// *** SECTION ***********
					s_cur = 0;
					for (int j=0; j<sectionSpline.controlVertices.Count; j++)
					{


						//int prev_j = (j+sectionSpline.controlVertices.Count-1) % (sectionSpline.controlVertices.Count);

						//if (i==1)
						//	Debug.Log (" :::: sec: (" +prev_j +") ==> ["+j+"]++"+sectionSpline.controlVertices[j]+"++, ::"+Math.Round(sectionSpline.breakingAngles[j])+"::, isSharp="+sectionSpline.isSharp(j) +", isSeam="+sectionSpline.isSeam(j) +", isBlend="+sectionSpline.isBlend(j));


						// NEIGHBORING QUAD NORMALS

						Vector3 normalUR		= Vector3.zero;
						Vector3 normalLR		= Vector3.zero;
						Vector3 normalRight		= Vector3.zero;

						Vector3 normalLL		= Vector3.zero;
						Vector3 normalUL		= Vector3.zero;
						Vector3 normalLeft		= Vector3.zero;

						Vector3 normalUpper		= Vector3.zero;
						Vector3 normalLower		= Vector3.zero;

						Vector3 normalN			= Vector3.zero;



						//  NORMALS AT BEVEL
						if (planSpline.shapeState == ShapeState.Closed || ( i>0 && i<planCount) )
						{
							normalUpper = (planBevelRot * sectionSpline.edgeNormals[j]).normalized;

							int pj = (j==0) ? sectionSpline.controlVertices.Count-1 : j-1;
							normalLower = (planBevelRot * sectionSpline.edgeNormals[pj]).normalized;

							normalN = (normalUpper + normalLower).normalized;		
							//Debug.Log("A :  " + normalN);
							//normalN = new Vector3(1,0,0);//Vecto3Quaternion.AngleAxis(30, Vector3.up) * normalN ;	
							//Debug.Log("B :  " + normalN);									
						}


						// Right Side
						if (planSpline.shapeState == ShapeState.Closed || i<planCount )
						{
							if (sectionSpline.isClosed || j<secCount)
								normalUR = (planRightRot * sectionSpline.edgeNormals[j]).normalized;

							int pj = (j==0) ? sectionSpline.controlVertices.Count-1 : j-1;
							if (sectionSpline.isClosed || j>0)
								normalLR = (planRightRot * sectionSpline.edgeNormals[pj]).normalized;

							if (sectionSpline.isClosed || (j>0 && j<secCount))
								normalRight = (normalLR + normalUR).normalized;
						}


						// Left Side
						if (planSpline.shapeState == ShapeState.Closed || i>0)
						{
							if (sectionSpline.isClosed || j<secCount)
								normalUL = (planLeftRot * sectionSpline.edgeNormals[j]).normalized;

							int pj = (j==0) ? sectionSpline.controlVertices.Count-1 : j-1;
							if (sectionSpline.isClosed || j>0)
								normalLL = (planLeftRot * sectionSpline.edgeNormals[pj]).normalized;

							if (sectionSpline.isClosed || (j>0 && j<secCount))
								normalLeft  = (normalLL + normalUL).normalized;
						}



						// DETERMINE VERTICES AT THIS NODE 1,2, or 4?
						// BASED ON POLYGONAL BREAKS AS SPECIFIED IN THE SPLINES



						int thisRight = p_cur * seclen + s_cur;

						//Debug.Log ("planlen="+planlen + ", seclen="+seclen+", s_cur="+s_cur);

						int thisLeft  = (i==0) ? (planlen-1) * seclen + s_cur : thisRight-seclen; //( prev_i * sectionSpline.derivedVertices.Count) + s_cur;



						int prevRight = (j==0) ? (p_cur * seclen)+seclen-1 : thisRight-1;

						int prevLeft = 0;
						if (i==0 && j==0)
							prevLeft = vertices.Length-1;
						else if (j==0)
							prevLeft  =  (p_cur-1) * seclen + seclen - 1;
						else 
							prevLeft = thisLeft-1;




                        

                        // FIRST POINT
                        if (planSpline.shapeState == ShapeState.Open  && i == 0)
						{
							if (sectionSpline.isBlend(j))
							{
								if (sectionSpline.isClosed || j < secCount)
                                {
                                    normals[thisRight] = normalRight;
                                    if(blendToXY)
                                        normals[thisRight].z = 0;
                                }
									
								//if (sectionSpline.isSeam(j))
								if (sectionSpline.isClosed || j > 0)
                                {
                                    normals[prevRight] = normalRight;
                                    if (blendToXY)
                                        normals[prevRight].z = 0;
                                }
									
							}
							else
							{
								if (sectionSpline.isClosed || j < secCount)
                                {
                                    normals[thisRight] = normalUR;
                                    if (blendToXY)
                                        normals[thisRight].z = 0;
                                }
									
								if (sectionSpline.isClosed || j > 0)
                                {
                                    normals[prevRight] = normalLR;
                                    if (blendToXY)
                                        normals[prevRight].z = 0;
                                }
									
							}
                            
                            
                            

                        }

                        //LAST POINT
                        else if (planSpline.shapeState == ShapeState.Open && i == planCount)
						{
							if (sectionSpline.isBlend(j))
							{
								if (sectionSpline.isClosed || j<secCount)
                                {
                                    normals[thisLeft] = normalLeft;
                                    if (blendToXY)
                                        normals[thisLeft].z = 0;
                                }
									
                                if (sectionSpline.isSeam(j))
                                {
                                    normals[prevLeft] = normalLeft;
                                    if (blendToXY)
                                        normals[prevLeft].z = 0;
                                }
									
							}
							else
							{
								if (sectionSpline.isClosed || j < secCount)
                                {
                                    normals[thisLeft] = normalUL;
                                    if (blendToXY)
                                        normals[thisLeft].z = 0;
                                }
									
								if (sectionSpline.isClosed || j > 0)
                                {
                                    normals[prevLeft] = normalLL;
                                    if (blendToXY)
                                        normals[prevLeft].z = 0;
                                }
									
							}
                            
                            

                        }
                        else if ( planSpline.isSeam(i) && sectionSpline.isSeam(j) )
						{
							// 4 vertices
							if ( planSpline.isBlend(i) && sectionSpline.isBlend(j) )
							{

								if (sectionSpline.isClosed || j<secCount)
								{
									normals[thisRight] = normalN;
									normals[thisLeft]  = normalN;
								}

								if (sectionSpline.isClosed || j>0)
								{
									normals[prevRight] = normalN;
									normals[prevLeft]  = normalN;
								}
							}
							else if (planSpline.isBlend(i))
							{

								if (sectionSpline.isClosed || j<secCount)
								{
									normals[thisRight] = normalUpper;
									normals[thisLeft]  = normalUpper;
								}
								if (sectionSpline.isClosed || j>0)
								{
									normals[prevRight] = normalLower;
									normals[prevLeft]  = normalLower;
								}
							}
							else if (sectionSpline.isBlend(j))
							{

								if (sectionSpline.isClosed || j<secCount)
								{
									normals[thisRight] = normalRight;
									normals[thisLeft]  = normalLeft;
								}
								if (sectionSpline.isClosed || j>0)
								{
									normals[prevRight] = normalRight;
									normals[prevLeft]  = normalLeft;
								}
							}
						}
						else if (planSpline.isSeam(i) && ! sectionSpline.isSeam(j))
						{
							// 2 vertics (U)
							if (planSpline.isBlend(i))
							{
								if (sectionSpline.isClosed || j<secCount)
								{
									normals[thisRight] = normalN;
									normals[thisLeft]  = normalN;
								}
							}
							else
							{
								if (sectionSpline.isClosed || j<secCount)
								{
									normals[thisRight] = normalRight;
									normals[thisLeft]  = normalLeft;
								}
							}							
						}
						else if (! planSpline.isSeam(i) && sectionSpline.isSeam(j) )
						{
							// 2 vertics (V)
							if (sectionSpline.isBlend(j))
							{	
								if (sectionSpline.isClosed || j<secCount)
									normals[thisRight] = normalN;

								if (sectionSpline.isClosed || j>0)
									normals[prevRight] = normalN;
							}
							else
							{
								if (sectionSpline.isClosed || j<secCount)
									normals[thisRight] = normalUpper;
								if (sectionSpline.isClosed || j>0)
									normals[prevRight] = normalLower; 
							}
						}
						else
						{
							// 1 vertex, full blending
							normals[thisRight] = normalN;
						}



						// section cursor incrementing
						s_cur++;
						int next_j = (j<sectionSpline.controlVertices.Count-1) ? j+1 : 0;
						if (sectionSpline.isSharp(next_j))
							s_cur++;


					}

					// plan cursor incrementing
					p_cur++;
					int next_i = (i<planSpline.controlVertices.Count-1) ? i+1 : 0;
					//Debug.Log("tris "+i+ ": "+ planSpline.isSharp(i));
					if (planSpline.isSharp(next_i))
						p_cur++;


				}

				mesh.normals = normals;
			}


			//Debug.Log(mesh.vertices.Length);

			//mesh.RecalculateBounds();

			mesh.RecalculateTangents();
			//sw.pDuration();

			sw.milestone("normals");

			//sw.dump();


			sw.stop();


			//Debug.Log("milli="+sw.);


			//botCapSpline.Reverse();




		}







	}
}
