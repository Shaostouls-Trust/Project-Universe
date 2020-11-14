using UnityEngine;

using System.Collections;

using AXGeometry;

public class AXLatheGenerator {
	
	
	bool topCap 	= true;
	bool botCap 	= true;
	

	public bool begCap 	= true;
	public bool endCap 	= true;
	
	bool isCapped 	= true;
	
	
	public float uScale = 1f;
	public float vScale = 1f;
	
	public float uShift = 0f;
	public float vShift = 0f;
	

	public float breakAngle = 60;




	// Constructors
	
	
	public Mesh generate(AXSpline _sectionSpline, float radius, int segs ) {
		return generate(_sectionSpline, radius, segs, 0, 360  );
	}


	public Mesh generate(AXSpline _sectionSpline, float radius, int segs, float begAng, float endAng  ) {

		if (_sectionSpline == null || _sectionSpline.verts == null || _sectionSpline.verts.Length == 0 || float.IsNaN(_sectionSpline.verts[0].x) )
		    return new Mesh();

		if (radius <= 0)
			radius = .01f;

		//Debug.Log (float.IsNaN(_sectionSpline.verts[0].x));
		//return new Mesh();

		if (segs < 3) 
			segs = 3;

		// section spline
		AXSpline sectionSpline;
		sectionSpline 				= _sectionSpline.turnRight();
		sectionSpline.isClosed 		= _sectionSpline.isClosed;
		sectionSpline.breakAngle 	= _sectionSpline.breakAngle;
		
		// Cap Splines
		AXSpline3D planSplineBegCap = new AXSpline3D();
		AXSpline3D planSplineEndCap = new AXSpline3D();
		
		AXSpline3D secSplineBegCap = new AXSpline3D();
		AXSpline3D secSplineEndCap = new AXSpline3D();
		
		
		int index = 0;
		

		float arcDegs 	= endAng-begAng;
		float deltaAng 	= arcDegs / segs;


		Vector3 scaler 	= new Vector3((1/Mathf.Cos(Mathf.Deg2Rad * deltaAng/2)), 1, 1);

		bool circleIsClosed 		= false;
		int circleAdjustedVertCount = segs+1;

		if (arcDegs == 360)
			circleIsClosed 			= true;

		bool circleIsAccute 		= false;
		if (deltaAng > breakAngle)
		{
			circleIsAccute 			= true;
			circleAdjustedVertCount += (circleAdjustedVertCount-2);

		}


		float segLength = 2 * radius * Mathf.Sin( Mathf.Deg2Rad * deltaAng/2);
		//Debug.Log ("segLength = " + segLength);

		// ALLOCATE ARRAYS ////////////////////////////////////////////////////////////
		
		//float[] sec_Angles 		 		 	= sectionSpline.getAnglesArray();
		int sec_AdjustedVertsCount 	 		= sectionSpline.getAllocateVertsCt();
		
		
		int fabricVertCount					= circleAdjustedVertCount * sec_AdjustedVertsCount;
		//Debug.Log ("fabricVertCount="+fabricVertCount);
		int begCapVertCount = 0;
		int endCapVertCount = 0;


		if (begCap) {
			if (circleIsClosed) {
				//if(circleIsAccute) fabricVertCount += circleAdjustedVertCount;
				begCapVertCount = segs;
			} else if (sectionSpline.isClosed) {
				//if(sectionSpline.closeJointIsAcute()) fabricVertCount += sec_AdjustedVertsCount;
				begCapVertCount = sectionSpline.vertCount;
			}
		}
		if (endCap) {
			if (circleIsClosed) {
				//if(circleIsAccute) fabricVertCount += circleAdjustedVertCount;
				endCapVertCount = segs;
			} else if (sectionSpline.isClosed) {
				//if(sectionSpline.closeJointIsAcute()) fabricVertCount += sec_AdjustedVertsCount;
				endCapVertCount = sectionSpline.vertCount;
			}
		}

		int totalVertCount		= fabricVertCount +   begCapVertCount +     endCapVertCount;

		int faceCount = segs*(sectionSpline.vertCount-1);

		int facesTriangleCount = 3 * 2*faceCount ;

		int   begTriangleCount =  (begCap) ? (3 * (begCapVertCount-2)) : 0;
		int   endTriangleCount =  (endCap) ? (3 * (endCapVertCount-2)) : 0;


		int totalTriangCount	= facesTriangleCount  +  begTriangleCount + endTriangleCount;

		Vector3[] vertices		= new Vector3[totalVertCount];
		Vector2[] uv			= new Vector2[totalVertCount];
		int[] triangles			= new int[totalTriangCount];
		
		float u  				= 0.0f;
		float v 				= 0.0f;
		
		
		
		
		
		
		
		
		
		
		
		
		
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
		
		int ribCounter		= 0;
		int vertCursor		= 0; 
		int vertCursor0		= 0; 
		
		if (arcDegs == 360) 
		{
			//endAng = 360-deltaAng;
		}

		// FOR EACH VERT IN ARC
		// For arc vert, find the transformation matrix withwhich to transform the section as a "rib"
		// Then traverse the section verts and mutiply them by the matrix.
		// If arc is open (less than 360 degs), then the first and last ribs make the caps.

		float theta_i = 0;

		for (int i=0; i<=segs; i++) {

			theta_i = i*deltaAng + begAng;

			u = uShift + i * segLength / uScale;

			vertCursor = ribCounter*sec_AdjustedVertsCount;

			Vector2 arc_vert = new Vector2( radius * Mathf.Cos(Mathf.Deg2Rad * theta_i), radius *  Mathf.Sin(Mathf.Deg2Rad * theta_i) );

			Matrix4x4 transMatrix 		= Matrix4x4.identity;
			Vector3 	translation 	= new Vector3(arc_vert.x, 0, arc_vert.y);
			Quaternion 	rotation 		= Quaternion.Euler(new Vector3(0, -theta_i, 0));;

			transMatrix.SetTRS(translation, rotation, scaler);
			
			
			
			
			// ADD RIB -----------------------------------------------------------------------------
			
			// FOR EACH POINT IN SECTION SPLINE (sectionSpline)
			
			int thisRibVertCursor = 0;

			Vector3 vert0 		= Vector3.zero;
			Vector3 vert	 	= Vector3.zero;
			

			//u = 0.0f;
			v = 0.0f;
			
			int j;
			
			// **  A RIB  **
			// **  each spline vert
			// ** 		- transform by plan location, rotation and scale
			for (j=0; j< sectionSpline.vertCount; j++) {
				
				
				/* **** VERT IS CREATED *** */
				vert = transMatrix.MultiplyPoint( new Vector3(-sectionSpline.verts[j].y, sectionSpline.verts[j].x, 0) );
				
				if (j==0) vert0 = vert;	
				//if (j>0)
					v = vShift + sectionSpline.curve_distances[j] / vScale;
				

				// BUILDUP CAP POLYGON VERTS everytime you start and finish a sec rib.
				Vector3 thisVert = new Vector3(vert.x, vert.y, vert.z);

				if (circleIsClosed)
				{
					// circle is closed
					if (begCap  &&  j==0) 			
						planSplineBegCap.Push(thisVert);
					
					if (endCap  &&  j==(sectionSpline.vertCount-1)) 
						planSplineEndCap.Push(thisVert);
				

				}
				else if (sectionSpline.isClosed)
				{
					// circle is open --  end caps
					if (begCap  &&  theta_i == begAng) 
						secSplineBegCap.Push(thisVert);
					
					if (endCap  &&  theta_i == endAng) 
						secSplineEndCap.Push(thisVert);

				}



				
				// ** ADD THIS VERT TO MESH VERTICES **
				vertices[vertCursor+thisRibVertCursor] 					= vert;				  
				uv[vertCursor+thisRibVertCursor] 						= new Vector2 (u, v);
				
				thisRibVertCursor++;
				
				if (sectionSpline.jointIsAcute(j)) {
					
					// ** ADD THIS VERT TO MESH VERTICES (AGAIN) **    (and restart the u?)
					vertices[vertCursor+thisRibVertCursor] 				= vert;				  
					uv[vertCursor+thisRibVertCursor] 					= new Vector2 (u, v);
					
					thisRibVertCursor++;
				}
			}
			
			if (sectionSpline.isClosed) {
				if (sectionSpline.closeJointIsAcute()) {
					//float d = Vector3.Distance(preVert, vert0);
					//Debug.Log (d);
					//u += d/100.0f;
					//Debug.Log ("u="+u);
					vertices[vertCursor+thisRibVertCursor] 				= vert0;				  
					uv[vertCursor+thisRibVertCursor] 					= new Vector2 (u, v);
				}
			}
			
			
			
			// GET READY FOR NEXT PLAN VERT, ie, RIB
			ribCounter++;
			
			if (i > 0 && i <segs && circleIsAccute) {
				// ADD RIB -----------------------------------------------------------------------------
				for(var n=0; n<sec_AdjustedVertsCount; n++) {
					vertices[vertCursor+n + sec_AdjustedVertsCount] 	= vertices[vertCursor+n];				  
					uv[vertCursor+n + sec_AdjustedVertsCount] 			= uv[vertCursor+n];
				}
				u = 0.0f;
				
				ribCounter++;
			}

		} 
		
		
		
		vertCursor = ribCounter*sec_AdjustedVertsCount;
		
		//int tmp = vertCursor-1;

		/*
		if (i == segs && circleIsClosed && circleIsAccute) {
			// ADD RIB ------------------------------------- DUPLICATE FIRST RIB --------------------
			for(int n=0; n<sec_AdjustedVertsCount; n++) {
				vertices[vertCursor+n] 									= vertices[n];				  
				uv[vertCursor+n] 										= new Vector2(uScale, uv[n].y);
			}
			
		}
		*/

		
		

		

		
		
		
		
		
		
		
		// 2. CREATE TRIANGLES //////////////////////////////////////////////////////////
		
		int LRib_L;
		int RRib_L;
		int LRib_U;
		int RRib_U;		
		
		int leftRib 	= 0;
		
		
		// FOREACH PLAN NODE.......
		for (int i=0; i<segs; i++) {
			



			vertCursor = leftRib*sec_AdjustedVertsCount;
			vertCursor0 = vertCursor;
			
			// FOREACH SEC NODE.......
			for (int j=1; j<sectionSpline.vertCount; j++) {
				
				//if (j == sectionSpline.vertCount-1  && ! sectionSpline.isClosed) 
				//	break;
				
				LRib_L = vertCursor;
				
				if (j == sectionSpline.vertCount && sectionSpline.isClosed && ! sectionSpline.closeJointIsAcute()) {
					Debug.Log ("GOING BACK TO SEC ORIGN...");
					LRib_U = vertCursor0;
				} else {
					LRib_U = LRib_L+1;
				}
				
				if (circleIsClosed  && i == segs  &&  ! circleIsAccute) {
					// use rib0
					RRib_L = LRib_L - fabricVertCount + sec_AdjustedVertsCount;
					RRib_U = LRib_U - fabricVertCount + sec_AdjustedVertsCount;;
				} else {
					// use next rib's points
					RRib_L = LRib_L + sec_AdjustedVertsCount;
					RRib_U = LRib_U + sec_AdjustedVertsCount;
				}
				
				triangles[index++] = LRib_L;
				triangles[index++] = RRib_U;
				triangles[index++] = RRib_L;
				
				triangles[index++] = LRib_L;
				triangles[index++] = LRib_U;
				triangles[index++] = RRib_U;
				
				vertCursor++;
				if (j<sectionSpline.vertCount && sectionSpline.jointIsAcute(j)) {
					vertCursor++; 	
				}
			}
			
			
			// GO TO NEXT SEGMENT
			leftRib++;
			if (i < (segs+1) && circleIsAccute) {
				leftRib++; 	
			}
		}
		
		
		
		
		
		
		
		
		
		
		
		
		
		//for (int l = 0; l<vertices.Length; l++)
		//	Debug.Log (l+": " + vertices[l]);

		
		
		

		

		
		
		// CAPS ////////////////////////

		if (isCapped) {
			
			
			
			//int vertCount = fabricVertCount;
			
			
			//int[] indices;
			//Vector2[] vertices2D;
			
			


			if ((botCap || topCap)  && circleIsClosed && !sectionSpline.isClosed) {
				// planSpline CAPS

				// BOT_CAP

				/*
				if (botCap) {
					// BOT_CAP VERTS
					
					for (int i=0; i<planSplineBegCap.vertCount; i++) {
						vertices[vertCount+i] = new Vector3(planSplineBegCap.verts[i].x, planSplineBegCap.verts[i].y, planSplineBegCap.verts[i].z);
						uv		[vertCount+i] = new Vector2 (uShift+planSplineBegCap.verts[i].x/uScale, vShift+planSplineBegCap.verts[i].z/vScale);	
					}
					
					// INDICES
					
					AXSpline begCapSlpine = new AXSpline(planSplineBegCap.getVertsAsVector2s());
					
					
					vertices2D = begCapSlpine.getVertsAsVector2s();


					tr = new Triangulator(vertices2D);
					indices = tr.Triangulate();

					// flip
					int[] indicesFlipped = new int[indices.Length];
					int c = 0;
					for(int i=(indices.Length-1); i >= 0; i--) 
						indicesFlipped[c++] = indices[i];


					for(int i=0; i<indicesFlipped.Length; i++) 
						triangles[index++] = indicesFlipped[i] + vertCount;

					
					vertCount += planSplineBegCap.vertCount;
					
				}
				
				
				
				// TOP CAP
				
				if (topCap) {
					// TOP CAP VERTS

					for (int i=0; i<planSplineEndCap.vertCount; i++) {
						vertices[vertCount+i] = new Vector3(planSplineEndCap.verts[i].x, planSplineEndCap.verts[i].y, planSplineEndCap.verts[i].z);
						uv		[vertCount+i] = new Vector2 (planSplineEndCap.verts[i].x/uScale, planSplineEndCap.verts[i].z/vScale);	
					}
					
					// INDICES
					AXSpline endCapSlpine = new AXSpline(planSplineEndCap.getVertsAsVector2s());
					vertices2D = endCapSlpine.getVertsAsVector2s();

					tr = new Triangulator(vertices2D);
					indices = tr.Triangulate();
					for(int i=0; i<indices.Length; i++) 
					{
						triangles[index++] = indices[i] + vertCount;
					}
					vertCount += planSplineEndCap.vertCount;
				}
				
				
			

				*/




			} else if( ! circleIsClosed && sectionSpline.isClosed) {
				// SS CAPS
				// SEC BEG CAP VERTS
				
				// The indices are independent of where the sec points are in space
				//vertices2D = sectionSpline.getVertsAsVector2s();

				/*
				tr = new Triangulator(vertices2D);
				indices = tr.Triangulate();



				// SEC BEG CAP
				if (begCap)
				{
					for (int i=0; i<secSplineBegCap.vertCount; i++) {
						vertices[vertCount+i] = new Vector3(secSplineBegCap.verts[i].x, secSplineBegCap.verts[i].y, secSplineBegCap.verts[i].z);
						uv		[vertCount+i] = new Vector2 (secSplineBegCap.verts[i].x/uScale, secSplineBegCap.verts[i].y/vScale);	
					}				
					for(int i=0; i<indices.Length; i++) 
						triangles[index++] = indices[i] + vertCount;
					
					vertCount += secSplineBegCap.vertCount;
				}


				// SEC END CAP
				if (endCap)
				{
					for (int i=0; i<secSplineEndCap.vertCount; i++) {
						vertices[vertCount+i] = new Vector3(secSplineEndCap.verts[i].x, secSplineEndCap.verts[i].y, secSplineEndCap.verts[i].z);
						uv		[vertCount+i] = new Vector2 (secSplineEndCap.verts[i].x/uScale, secSplineEndCap.verts[i].y/vScale);	
					}
					for(int i=indices.Length-1; i>=0; i--) 
						triangles[index++] = indices[i] + vertCount;
					
					vertCount += secSplineBegCap.vertCount;
				}
			
				*/
			}
		}

		
	
		Mesh mesh = new Mesh();
		
		mesh.vertices = vertices;
		mesh.uv = uv;
		mesh.triangles = triangles;
		
		// Auto-calculate vertex normals from the mesh
		mesh.RecalculateNormals();




		
		// adjust normals for first and last vert
		if (! circleIsAccute)
		{
			//Debug.Log ("adjust normals");
			Vector3[] norms = mesh.normals;


			Quaternion rotation0 = Quaternion.Euler(0, deltaAng/2, 0);
			Quaternion rotationN = Quaternion.Euler(0, -deltaAng/2, 0);
			int lastRibV0 =  (circleAdjustedVertCount-1) * sec_AdjustedVertsCount;

			for (int jj=0; jj< sec_AdjustedVertsCount; jj++) 
			{
				// first rib
				norms[jj] 				= rotation0 * norms[jj];

				// last rib
				norms[lastRibV0+jj] = rotationN * norms[lastRibV0+jj];
			}



			mesh.normals = norms;
		}





		
		return mesh;
		
		
	}
	
	
	
	
	
	
	
}
