using UnityEngine;

using System.Collections;

using AXGeometry;

using AX;

using AXClipperLib;
using Path = System.Collections.Generic.List<AXClipperLib.IntPoint>;


public class AXHalfRoundGenerator {
	
	
	public float uScale = 1f;
	public float vScale = 1f;
	
	public float uShift = 0f;
	public float vShift = 0f;
	

	
	public static Mesh generate(Path sectionPath, int segs, bool topCap, bool botCap ) {
	
		if (sectionPath == null )
		    return new Mesh();

		if (segs < 3) 
			segs = 3;

        
		Vector2[] sectionPoints = Pather.path2Vector2Array(sectionPath);

        


		// Cap Splines
		AXSpline3D planSplineTopCap = new AXSpline3D();
		AXSpline3D planSplineBotCap = new AXSpline3D();
		
		
		int index = 0;


        // INCREMENT BECAUSE WE ARE SPLITTING ONE SEG INTO TWO TO BECOME FIRST AND LAST
        segs++;
        


        float arcDegs 	= 180;
		float deltaAng 	= arcDegs / segs;


        // SCALE THE RIB SO THE MID SEGMENTS ARE AT PROPER RADIUS
		Vector3 scaler 	= new Vector3((1/Mathf.Cos(Mathf.Deg2Rad * deltaAng/2)), 1, 1);




        // -- VERT COUNT
		int sidesVertCount = (segs+3) * sectionPoints.Length;

		int topCapVertCount = (topCap) ? segs + 1 : 0;
		int botCapVertCount = (botCap) ? segs + 1 : 0;

		int totalVertCount		= sidesVertCount + topCapVertCount + botCapVertCount;



        // TRIANGLE COUNT
		int faceCount = (segs+2) * (sectionPoints.Length - 1);

		int facesTriangleCount = 3 * 2*faceCount;

		int topTriangleCount =  (topCap) ? (3 * (topCapVertCount - 2)) : 0;
		int botTriangleCount =  (botCap) ? (3 * (topCapVertCount - 2)) : 0;

		int totalTriangCount	= facesTriangleCount  +  topTriangleCount + botTriangleCount;



        // ALLOCATE ARRAYS ////////////////////////////////////////////////////////////

        Vector3[] vertices		= new Vector3[totalVertCount];
		Vector2[] uv			= new Vector2[totalVertCount];
		int[] triangles			= new int[totalTriangCount];
		
		
		
		
		
		// 1. CREATE VERTICES //////////////////////////////////////////////////////////
		
		
		// FOR EACH VERT IN ARC
		// For arc vert, find the transformation matrix withwhich to transform the section as a "rib"
		// Then traverse the section verts and mutiply them by the matrix.
		// If arc is open (less than 360 degs), then the first and last ribs make the caps.
  

        Vector3 tmpScaler = Vector3.one;

        float angCursor = 0;

        int ribCursor = 0;



        // EACH RIB *************
        for (int i=0; i<=(segs+2); i++) {

			tmpScaler = (i == 0 || i == segs+1) ? Vector3.one : scaler;
   
			ribCursor = i * sectionPoints.Length;

            Matrix4x4 ribI_Matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(new Vector3(0, -angCursor, 0)), tmpScaler);


            // ADD RIB -----------------------------------------------------------------------------
			Vector3 vert = Vector3.zero;
			
			int j;

            // EACH SECTION VERT -- transform by plan location, rotation and scale
			for (j=0; j< sectionPoints.Length; j++) {
		
				/* **** GENERATE TRANSFORMED VERT *** */
				vert = ribI_Matrix.MultiplyPoint( new Vector3(sectionPoints[j].x, sectionPoints[j].y, 0) );
				
                // ** ADD THIS VERT TO MESH VERTICES **
                vertices    [ribCursor + j] = vert;
                uv          [ribCursor + j] = new Vector2((angCursor / 180), sectionPoints[j].y);


                // BUILDUP CAP POLYGON VERTS everytime you start and finish a sec rib.
                //Vector3 thisVert = new Vector3(vert.x, vert.y, vert.z);

                //if (circleIsClosed)
                //{
                //	// circle is closed
                //	if (begCap  &&  j==0) 			
                //		planSplineBegCap.Push(thisVert);

                //	if (endCap  &&  j==(sectionSpline.vertCount-1)) 
                //		planSplineEndCap.Push(thisVert);


                //}
                //else if (sectionSpline.isClosed)
                //{
                //	// circle is open --  end caps
                //	if (begCap  &&  theta_i == begAng) 
                //		secSplineBegCap.Push(thisVert);

                //	if (endCap  &&  theta_i == endAng) 
                //		secSplineEndCap.Push(thisVert);

                //}
 
			}

            //Debug.Log(i + " " + angCursor);
            
            // SET ANGLWEFOR NEXT STEP
            if (i == 0)
                angCursor = deltaAng / 2;
            else if (i == segs)
                angCursor = 180;
            else
                angCursor += deltaAng;


        }








        // 2. CREATE TRIANGLES //////////////////////////////////////////////////////////

        int LRib_L;
		int RRib_L;
		int LRib_U;
		int RRib_U;		
		
		int leftRib 	= 0;
		
		
		// FOREACH PLAN NODE.......
		for (int i=0; i<=segs; i++) {
			
			ribCursor = leftRib * sectionPoints.Length;
		
			// FOREACH SEC POINT.......
			for (int j=1; j< sectionPoints.Length; j++) {
				
                // This rib's points
				LRib_L = ribCursor;
                LRib_U = LRib_L+1;
				
				// Next rib's points
				RRib_L = LRib_L + sectionPoints.Length;
				RRib_U = LRib_U + sectionPoints.Length;
				
				
				triangles[index++] = LRib_L;
				triangles[index++] = RRib_U;
				triangles[index++] = RRib_L;
				
				triangles[index++] = LRib_L;
				triangles[index++] = LRib_U;
				triangles[index++] = RRib_U;
				
				ribCursor++;
			}

            // GO TO NEXT SEGMENT
            leftRib++;
		}
		

		
	
		Mesh mesh = new Mesh();
		
		mesh.vertices = vertices;
		mesh.uv = uv;
		mesh.triangles = triangles;


        // Auto-calculate vertex normals from the mesh
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        Vector3[] norms = mesh.normals;

        Quaternion rotation0 = Quaternion.Euler(0, -deltaAng/4 , 0);
        Quaternion rotationN = Quaternion.Euler(0,  -deltaAng/4 , 0);
        int lastRibV0 = (segs) * sectionPoints.Length;

        for (int jj = 0; jj < sectionPoints.Length; jj++)
        {
            // first rib
            Vector3  n = new Vector3(norms[jj].x, norms[jj].y, 0);
            norms[jj] = n; // rotation0 * norms[jj];

            // last rib
            n = norms[lastRibV0 + jj];
            n.z = 0;

            norms[lastRibV0 + jj] = n;// rotationN * norms[lastRibV0 + jj];
        }



        mesh.normals = norms;




        return mesh;
		
		
	}
	
	
}
