using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;


using AXGeometry;

namespace AX
{
	
	public class AXSplineExtrudeGenerator {
	
	
		public bool topCap 	= true;
		public bool botCap 	= true;
		
		public bool isCapped 	= true;
		
		public bool begCap 	= true;
		public bool endCap 	= true;
	
		public float uScale = 20f;
		public float vScale = 20f;
		public float uShift = 0f;
		public float vShift = 0f;
	
		public Axis axis = Axis.Y;
	
	
		
		
		
		
		
	
		// Constructors
		public Mesh generate(AXSpline _planSpline, AXSpline _sectionSpline ) 
		{
			return generate(_planSpline, _sectionSpline, 0f, 0f );
		}
	
		public Mesh generate(AXSpline _planSpline, AXSpline _sectionSpline, float uShiftNow, float vShiftNow ) {
	
	
	
			_planSpline.removeLastVertIfIdenticalToFirst();
			//Debug.Log ("topCap="+topCap+",  planSpline.isClosed="+_planSpline.isClosed + " : sectionSpline.isClosed=" + _sectionSpline.isClosed);
	
			if(uShiftNow != 0)
				uShift = uShiftNow;
	
			if(vShiftNow != 0)
				vShift = vShiftNow;
	
			if (_planSpline == null || _planSpline.vertCount == 0 || _sectionSpline == null || _sectionSpline.vertCount == 0)
				return null;
	
			// plan spline
			AXSpline planSpline = _planSpline;
	
			// LATER: calc this on whether vert 0 and n are the same.
			planSpline.isClosed = false;
	
			planSpline.calcStats();
			planSpline.getUValues();
	
		
			// section spline
			AXSpline sectionSpline;
			sectionSpline 				= _sectionSpline.turnRight();
			sectionSpline.isClosed 		= true; //_sectionSpline.isClosed;
			sectionSpline.breakAngle 	= _sectionSpline.breakAngle;
	
			
	
			int index = 0;
	
	
	
			if (!planSpline.isClosed && !sectionSpline.isClosed)
			{
				begCap = false;
				endCap = false;
			}
	
			// ALLOCATE ARRAYS ////////////////////////////////////////////////////////////
			
			// SECTION ALLOCATION
			int sec_AdjustedVertsCount 	 		= sectionSpline.getAllocateVertsCt();
			//if (sectionSpline.isClosed && ! sectionSpline.closeJointIsAcute()) // add a close rib
			//	sec_AdjustedVertsCount++;
	
			// PLAN ALLOCATION
			int plan_AdjustedVertsCount 		= planSpline.getAllocateVertsCt();
			if (planSpline.isClosed && ! planSpline.closeJointIsAcute()) // add a close rib
				plan_AdjustedVertsCount++;
	
	
			int fabricVertCount					= plan_AdjustedVertsCount * sec_AdjustedVertsCount + 2;
	
			//Debug.Log
	
			// use the count of sides to determine the quads count in the extrusion
			int planSides = planSpline.vertCount;
			if (! planSpline.isClosed)
				planSides--;
	
			int sectionSides = sectionSpline.vertCount;
			if (! sectionSpline.isClosed)
				sectionSides--;
	
			int quadCount = planSides * sectionSides;
	
			int totalVertCount		=  fabricVertCount ;
			int totalTriangCount	= 3 * 2*quadCount;
			
			Vector3[] vertices		= new Vector3[totalVertCount];
			Vector2[] uv			= new Vector2[totalVertCount];
			int[] triangles			= new int[totalTriangCount];
			
	
			
			
	
			
			
			
			//Debug.Log("plan_AdjustedVertsCount="+plan_AdjustedVertsCount+", sec_AdjustedVertsCount="+sec_AdjustedVertsCount+", fabricVertCount = "+ fabricVertCount);
			
			
			
			// 1. CREATE VERTICES //////////////////////////////////////////////////////////
			
			
			// i is current vert:
			//
			//					  (i-1)
			//						|
			//						|
			//						| edgeAfter
			//						|
			//						|
			//				       (i)
			//					   /
			//				      /
			//				     / edgeBefore
			//				    /
			//				   /
			//    		    (i-1)
	
			int ribCount		= 0;
			int planRib_VertCursor		= 0; 
			int vertCursor0		= 0; 
	
			Vector2 edgeBefore 				= Vector2.zero;
			Vector2 edgeBeforePerp 			= Vector2.zero;
			Vector2 edgeBeforePerpN 		= Vector2.zero;
			
			Vector2 edgeAfter 				= Vector2.zero;
			Vector2 edgeAfterPerp 			= Vector2.zero;
			Vector2 edgeAfterPerpN 			= Vector2.zero;
	
	
			float uScalerAfter  	= 1f;
	
			float plan_u = 0f;
			
			// FOR EACH PLAN  VERT
			for (int i = 0; i<planSpline.vertCount; i++) {
	
		
				//Debug.Log ("New PLAN POINT: " + i + " :: ribCount=" + ribCount + ", sec_AdjustedVertsCount="+sec_AdjustedVertsCount);
	
				planRib_VertCursor = ribCount * sec_AdjustedVertsCount ;
				Debug.Log ("planRib_VertCursor["+i+"] = " + planRib_VertCursor);
	
				plan_u = planSpline.curve_distances[i] / uScale;
	
				//Debug.Log (planSpline.angles[i]);
				
				// EACH Plan node
				
				// ASSUME: If spline is open then the first and last are perpendicular to the end segments.
	
				// For each plan point, find the transformation matrix withwhich to transform the section as a "rib"
				// Then traverse the section verts and mutiply them by the plan point matrix.
	
				Matrix4x4 transMatrix 		= Matrix4x4.identity;
	
				Vector3 	translation = new Vector3(planSpline.verts[i].x, 0.0f, planSpline.verts[i].y);
				Quaternion 	rotation 	= Quaternion.identity;
				Vector3 	scaler 		= Vector3.one;
	
	
	
				if ( ! planSpline.isClosed && ( i == 0 || i == planSpline.vertCount-1 )) 
				{
					// open plan has special treatment of first and last verts
					if (i == 0)
					{
						edgeAfter 		= planSpline.verts[i+1] - planSpline.verts[i];
						edgeAfterPerp 	= new Vector2(edgeAfter.y, -edgeAfter.x);
						edgeAfterPerpN  = edgeAfterPerp.normalized;
						rotation  		= Quaternion.FromToRotation(Vector3.right, new Vector3(edgeAfterPerp.x, 0, edgeAfterPerp.y));
					}
					else 
					{
						edgeBefore 		= planSpline.verts[i] - planSpline.verts[i-1];
						edgeBeforePerp 	= new Vector2(edgeAfter.y, -edgeAfter.x);
						edgeBeforePerpN = edgeBeforePerp.normalized;
						rotation  		= Quaternion.FromToRotation(Vector3.right, new Vector3(edgeBeforePerp.x, 0, edgeBeforePerp.y));
					}
	
					// scaler remains one
				}
				else
				{
	
					if (i == 0)
						edgeBefore 		= planSpline.verts[i] - planSpline.verts[planSpline.vertCount-1];
					else
						edgeBefore 		= planSpline.verts[i] - planSpline.verts[i-1];
					
					edgeBeforePerp 	= new Vector2(edgeBefore.y, -edgeBefore.x);
					edgeBeforePerpN = edgeBeforePerp.normalized;
	
	
	
	
					if (i == planSpline.vertCount-1)
						edgeAfter 		= planSpline.verts[0] - planSpline.verts[i];
					else 
						edgeAfter 		= planSpline.verts[i+1] - planSpline.verts[i];
	
					edgeAfterPerp 	= new Vector2(edgeAfter.y, -edgeAfter.x);
					edgeAfterPerpN  = edgeAfterPerp.normalized;
	
	
	
					// the addition of the normalized perpendicular vectors leads to a bisector
					Vector2 bisector = edgeAfterPerpN + edgeBeforePerpN ;
					rotation  		= Quaternion.FromToRotation(Vector3.right, new Vector3(bisector.x, 0, bisector.y));
	
					float biAngle = Vector2.Angle(bisector, edgeAfter);
	
					// we can get the scaler from the dot product
					float scalerVal = 1 / Vector2.Dot( edgeBeforePerpN, bisector.normalized);
					scaler 		= new Vector3(scalerVal, 1, 1);
	
					uScalerAfter 	= scalerVal*Mathf.Cos (Mathf.Deg2Rad*biAngle);
	
					//Debug.Log ("biAngle="+biAngle + " :: " + Mathf.Cos (Mathf.Deg2Rad*biAngle) + ", scalerVal="+scalerVal + " -- " + scalerVal*Mathf.Cos (Mathf.Deg2Rad*biAngle));
	
					
				}
	
				//Debug.Log("["+i+"] " + translation + " :: " + rotation + " :: " + scaler);
				transMatrix.SetTRS(translation, rotation, scaler);
	
	
				
				// ADD RIB -----------------------------------------------------------------------------
				
				// FOR EACH POINT IN SECTION SPLINE (sectionSpline)
				
				int section_VertCursor = 0;
				Vector3 vert	 	= Vector3.zero;
	
	
				int j;
	
				float  u = 0;
				float nu = 0;
				float  v = 0;
	
				int ribEnd_vertIndex = 0;
				int ribBeg_vertIndex = 0;
	
				// **  A RIB  **
				// **  each spline vert
				// ** 		- transform by plan location, rotation and scale
				for (j=0; j<=sectionSpline.vertCount; j++) {
	
					if ( !sectionSpline.isClosed && j==sectionSpline.vertCount)
						break;
	
					/* **** VERT IS CREATED *** */
					if (j==sectionSpline.vertCount) // return to first vert
						vert = transMatrix.MultiplyPoint( new Vector3(-sectionSpline.verts[0].y, sectionSpline.verts[0].x, 0) );
					else
						vert = transMatrix.MultiplyPoint( new Vector3(-sectionSpline.verts[j].y, sectionSpline.verts[j].x, 0) );
	
	
	
				
	
	
	
	
					// THIS IS THE MAIN SECTION OF CODE //
	
	
					// Determine UV coordinates
	
	
					// THIS_U
					if (i == 0 && !planSpline.closeJointIsAcute())
						u 	= uShift + plan_u;
					else
						u 	= uShift + plan_u - (uScalerAfter * sectionSpline.verts[j].y / uScale);
	
					
					// NEXT_U (AFTER BREAK BUT AT SAME LOCATION)
					if ( planSpline.jointIsAcute(i) || (i==0 && planSpline.closeJointIsAcute()))
					{
						if (i == 0 && planSpline.isClosed)
							nu 	= uShift + (planSpline.getLength() / uScale) + (uScalerAfter * sectionSpline.verts[j].y / uScale);
						else
							nu 	= uShift + plan_u + (uScalerAfter * sectionSpline.verts[j].y / uScale);
					}
					else
					{
						if (i == 0 && planSpline.isClosed)
							nu 	= uShift + (planSpline.getLength() / uScale);
						else
							nu = uShift + plan_u;
					}
					
					// THIS_V
					if (j == sectionSpline.vertCount)
						v = vShift + sectionSpline.getLength() / vScale;
					else
						v = vShift + sectionSpline.curve_distances[j] / vScale;
	
	
					// determine vertices address
					if (i == 0 && planSpline.isClosed )
					{
						ribEnd_vertIndex = (fabricVertCount - sec_AdjustedVertsCount) + section_VertCursor;
						ribBeg_vertIndex = planRib_VertCursor +	section_VertCursor;
					}
					else
					{
						ribEnd_vertIndex = planRib_VertCursor +	section_VertCursor;
						ribBeg_vertIndex = planRib_VertCursor +	section_VertCursor + sec_AdjustedVertsCount;
					}
	
	
					// ** ADD THIS VERT TO MESH VERTICES **
					vertices[ribEnd_vertIndex] 					= vert;	
					uv[      ribEnd_vertIndex] 					= new Vector2 (nu, v);
	
					//Debug.Log ("Add ribEnd_vertIndex="+ribEnd_vertIndex + "    -- "+vertices[ribEnd_vertIndex]);
	
					
					
					if (  (i>0 && planSpline.jointIsAcute(i)) || (i == 0 && planSpline.isClosed) )  
					{
						// add an additional vert in the next rib (in same location
						vertices[ribBeg_vertIndex] = vert;				  
						uv[      ribBeg_vertIndex] = new Vector2 (u, v);
						//Debug.Log ("Add ribBeg_vertIndex="+ribBeg_vertIndex + "    -- "+vertices[ribBeg_vertIndex]);
	
					}
	
	
					section_VertCursor++;
	
					// SECTION BREAK?
					if (j<sectionSpline.vertCount  && sectionSpline.jointIsAcute(j)) {
						// ** ADD THIS VERT TO MESH VERTICES (AGAIN) **    (and restart the u?)
						// add an additional section point in the same location
						if (i == 0 && planSpline.isClosed)
						{
							ribEnd_vertIndex = (fabricVertCount - sec_AdjustedVertsCount) + section_VertCursor;
							ribBeg_vertIndex = planRib_VertCursor +	section_VertCursor;
						}
						else
						{
							ribEnd_vertIndex = planRib_VertCursor +	section_VertCursor;
							ribBeg_vertIndex = planRib_VertCursor +	section_VertCursor + sec_AdjustedVertsCount;
						}
						//Debug.Log ("ribEnd_vertIndex="+ribEnd_vertIndex+", ribBeg_vertIndex="+ribBeg_vertIndex + "   -- "+vertices[ribBeg_vertIndex]);
	
						vertices[ribEnd_vertIndex] 				= vert;				  
						uv[		 ribEnd_vertIndex] 				= new Vector2 (  nu, v);
						//Debug.Log ("Add ribEnd_vertIndex="+ribEnd_vertIndex + "    -- "+vertices[ribEnd_vertIndex]);
	
						if (  (i>0 && planSpline.jointIsAcute(i)) || (i == 0 && planSpline.isClosed) )  
						{
							// add an additional vert in the next rib (in same location
							vertices[ribBeg_vertIndex] = vert;				  
							uv[      ribBeg_vertIndex] = new Vector2 (u, v);
							//Debug.Log ("Add ribBeg_vertIndex="+ribBeg_vertIndex + "    -- "+vertices[ribBeg_vertIndex]);
						}
	
						section_VertCursor++;
					}
				}
	
	
	
				ribCount++;							// COUNT THIS RIB, ie, RIB
				if (planSpline.jointIsAcute(i)) 	
					ribCount++;						// ADD ADDITIONAL RIB COUNT 
	
				
			} 
	
	
			
	
	
	
	
	
	
			
	
	
	
	
	
			
	
			
			// 2. CREATE TRIANGLES //////////////////////////////////////////////////////////
			
			int LRib_L;
			int RRib_L;
			int LRib_U;
			int RRib_U;		
			
			int leftRib 	= 0;
	
	
			// FOREACH PLAN NODE.......
			for (int i=1; i<=planSpline.vertCount;i++) {
				
				if (i == planSpline.vertCount  &&  ! planSpline.isClosed) 
					break;
				// from left to right
				
				planRib_VertCursor = leftRib*sec_AdjustedVertsCount ;
	
	
				vertCursor0 = planRib_VertCursor;
				
				// FOREACH SEC NODE.......
				for (int j=1; j<=sectionSpline.vertCount; j++) {
					
					if (j == sectionSpline.vertCount  && ! sectionSpline.isClosed) 
						break;
					
					LRib_L = planRib_VertCursor;
	
					if (j == sectionSpline.vertCount && sectionSpline.isClosed && ! sectionSpline.closeJointIsAcute()) {
						//Debug.Log ("GOING BACK TO SEC ORIGN...");
						LRib_U = vertCursor0;
					} else {
						LRib_U = LRib_L+1;
					}
	
	
	
					// use next rib's points ( Never loop back to use rib0)
					RRib_L = LRib_L + sec_AdjustedVertsCount;
					RRib_U = LRib_U + sec_AdjustedVertsCount;
	
					
					
					triangles[index++] = LRib_L;
					triangles[index++] = RRib_U;
					triangles[index++] = RRib_L;
					
					triangles[index++] = LRib_L;
					triangles[index++] = LRib_U;
					triangles[index++] = RRib_U;
					
					planRib_VertCursor++;
					if (j<sectionSpline.vertCount && sectionSpline.jointIsAcute(j)) {
						planRib_VertCursor++; 	
					}
				}
				
				
				// GO TO NEXT SEGMENT
				leftRib++;
				if ( i<planSpline.vertCount  && planSpline.jointIsAcute(i) ) {
					leftRib++; 	
				}
			}
			
			
			
			
			
			
			
			
			
			
			
			
			
			
			
			
			
			
			
			
			
	
	
			Mesh mesh = new Mesh();
			mesh.vertices 	= vertices;
			mesh.uv 		= uv;
			mesh.triangles 	= triangles;
	
	
	
	
			// Auto-calculate vertex normals from the mesh
			mesh.RecalculateNormals();
	
			return mesh;
			
	
		}
		
	
	
	
	
	
	
	}
}