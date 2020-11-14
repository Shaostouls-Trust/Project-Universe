using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using AXClipperLib;
using Path 			= System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths 		= System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;

using AXPoly2Tri;
using PolygonPoints = System.Collections.Generic.List<AXPoly2Tri.PolygonPoint>;




namespace AXGeometry
{



	/* SPLINE
	 * 
	 * Manage a set of 2D points.
	 *
	 *
	 */

	[System.Serializable]
	public class Spline 
	{

		public List<Vector2>	controlVertices 		= new List<Vector2>();

		// Each controlVErtex can have a Color. This can be set in turtle script, or set on FreeCurve points and extrapolated.
		public List<Vector2>	controlColors 			= new List<Vector2>();


		public List<Vector2>	derivedVertices 		= new List<Vector2>();
		

		public List<float>		breakingAngles			= new List<float>();
		
		public List<float>		edgeLengths    			= new List<float>();
		public List<float>		edgeRotations    		= new List<float>();
		
		public List<Vector3>	edgeNormals    			= new List<Vector3>();
		public List<Vector3>	nodeNormals    			= new List<Vector3>();
		
		
		public List<float>		nodeRotations			= new List<float>(); // used for transforms at joints
		public List<Matrix4x4>  nodeTransforms			= new List<Matrix4x4>();
		
		public Matrix4x4  		begTransform			= Matrix4x4.identity;
		public Matrix4x4  		capATransform			= Matrix4x4.identity;

		public Matrix4x4  		endTransform			= Matrix4x4.identity;
		public Matrix4x4  		capBTransform			= Matrix4x4.identity;


		public List<Vector2>	repeaterNodePositions;
		public List<Vector2>	repeaterCellPositions;

		public List<float>		repeaterNodeRotations;
		public List<float>		repeaterCellRotations;

		public List<Matrix4x4>	repeaterNodeTransforms;		
		public List<Matrix4x4>	repeaterCellTransforms;

        public float perpAngleFirst = 0;
        public float perpAngleLast = 0;

        public List<float>		bevelAngles			  	= new List<float>(); // used for textures
		public float[]			bevelAnglesActual;
		float 					bevelAngle0 			= 0;

		public List<float> 		segment_lengths;
		public List<float>		running_distances    	= new List<float>();
		public float 			length					= 0;
		
		public List<float>		normalizedUvalues    	= new List<float>();
		
		public float 			breakTolerance			= 0.1f;
		public float 			breakAngleGeom 			= 60.0f; // determines how many vertices will be derived from control points
		public float 			breakAngleNormals 		= 60.0f; // used for determining the direction of normals

		public float 			breakAngleCorners		= 60; //The angle at which the vertex is considered a corner. Used when making RepeterSplineSegemnets.
		
		public bool 			isBeveledA;
		public bool 			isBeveledB;
		
		
		public int 				sharpVertsInside_Count 	= 0;
		public int 				allocatedVerticesCount	= 0;
		public bool 			isClosed 				= false;
		public ShapeState 		shapeState 				= ShapeState.Closed;

		public float 			samePointTolerence 		= 0.001f;

		// Segments between break angles defined by start and end indices.
		// These are essentionally consecutive odes of a curve.
		//public SubsplineIndices[] 	subsplineIndices;
		public List<SubsplineIndices> subsplines;

		// Consecutive breakAngle nodes that are closer together than minimum length
		public int[] 			groupedIndicies;
		public int 				countOfGroupedNodes;
		public float 			breakAngleGrouping 		= 22.5f;
		public float			min_segmentLength		= 2;
		public float			max_segmentLength		= 12;


		// These are splines defined by inset
		public List<int> 		breakIndices;
		public List<List<int>>	groupedCornerNodes;

		public List<Spline> 	insetSplines;
		public List<Spline> 	insetSpanSplines;



		public bool allSharp;

		public bool trueSideSection = true;


        public bool autoBevelOpenEnds = true;



        /*************************
	 	* CONSTRUCTORS
	 	*************************/

        public Spline()
		{
			controlVertices = new List<Vector2>();
			isClosed = false;
		}
		
		public Spline(Path path)
		{
			isClosed = false;
			init (path);
		}
		
		public Spline(Path path, bool _isClosed)
		{
			isClosed = _isClosed;
			init (path);
		}
		public Spline(Path path, bool _isClosed, float breakP, float breakN, bool _trueSideSection = true, bool _autoBevelOpenEnds = true)
		{
			isClosed 			= _isClosed;
			shapeState = isClosed ? ShapeState.Closed : ShapeState.Open;

			breakAngleGeom = breakP;
			breakAngleNormals 	= breakN;

			trueSideSection = _trueSideSection;

            autoBevelOpenEnds = _autoBevelOpenEnds;

            init (path);
		}
		
		public Spline(List<Vector2> verts,  bool _isClosed)
		{
			controlVertices 	= verts;
			isClosed 			= _isClosed;
			shapeState = isClosed ? ShapeState.Closed : ShapeState.Open;

			initializeCurveMetrics();
		}
		public Spline(List<Vector2> verts,  bool _isClosed, float breakP, float breakN, bool _trueSideSection = true)
		{
			controlVertices 	= verts;
			breakAngleGeom = breakP;
			breakAngleNormals 	= breakN;
			isClosed 			= _isClosed;
			shapeState = isClosed ? ShapeState.Closed : ShapeState.Open;

			trueSideSection = _trueSideSection;

			initializeCurveMetrics();
		}
		
		private void init(Path path)
		{ 	
			// if the path first and last points are the same, remove and close this spline
			// If they match override the isClosed passed in a constructor...
			if (path.Count > 0 && path[0] == path[path.Count-1])
			{
				path.RemoveAt(path.Count-1);
				//isClosed = true;
			}
			
			controlVertices = new List<Vector2>();
			
			//Debug.Log ("init spline");
			
			if (path != null && path.Count > 0)
			{
				// Migrate path points to Vector2's
				foreach (IntPoint ip in path)
					controlVertices.Add (AXGeometry.Utilities.IntPt2Vec2(ip));
				
				
				initializeCurveMetrics();
			}
		}


		public void setIsClosed(bool _isClosed)
		{
			if (isClosed == _isClosed)
				return;
			
			isClosed = _isClosed;
			initializeCurveMetrics();	
		}
		
		
		public Spline Reverse()
		{
			controlVertices.Reverse();
			initializeCurveMetrics();
			return this;
		}
		





		/*************************
	 	 * GET_VERT_DISTANCES LIST
	 	 * This makes arrays of useful metrix per control vertex
	 	 * such as running distance along the curve, normalized U, angle 
	 	 * and edge lengths
	 	 *************************/
		public void initializeCurveMetrics(Boolean allowClosingPoint = false) {

			allSharp = true;

			if (controlVertices == null || controlVertices.Count == 0)
				return;

			derivedVertices     = new List<Vector2>();
			
			edgeLengths 		= new List<float>();
			edgeRotations    	= new List<float>();
			edgeNormals			= new List<Vector3>();
			nodeNormals			= new List<Vector3>();
			nodeRotations		= new List<float>();
			nodeTransforms		= new List<Matrix4x4>();
			
			// the angle of the bisector between two segments.
			// A negative bevel angle is a concave angle
			bevelAngles			= new List<float>();
			bevelAnglesActual	= new float[controlVertices.Count];

			// The angle from one segment to the next.
			breakingAngles		= new List<float>();

			segment_lengths		= new List<float>();
			running_distances 	= new List<float>();
			normalizedUvalues 	= new List<float>();
			
			
			// temporary variables
			float running_distance = 0.0f;			
						
			// By definition, the first running distance is 0:
			running_distances.Add (running_distance);
						
			// remove end point if it is too near the first point
			if (! allowClosingPoint && Vector2.Distance(controlVertices[0], controlVertices[controlVertices.Count-1]) < samePointTolerence && controlVertices.Count >1)
			{
				controlVertices.RemoveAt(controlVertices.Count-1);
				shapeState = ShapeState.Closed;

				//isClosed = true;
			}
			
			// check if first vert is smooth. If another vert is sharp, roll to that position.
			// .... would need to calc break angles first
			

			// start with vert 0
			allocatedVerticesCount = 1;
			derivedVertices.Add( controlVertices[0] );
			
			// Loop to find angles, edge lengths, and running distances.
			//   	Start at 1, since running_distances[0] was already added.
			// 		Continue to one past the end to account for the round trip to the 
			// 		first vert again.
			// 		Thus  curve_distances[controlVertices.Count] is the full length of the spline.


			Vector3 firstPosition = Vector3.zero;
			Vector3 lastPosition = Vector3.zero;



			for (int i=1; i<=controlVertices.Count; i++) {
				

				Vector2 pp = previousPoint(i);
				Vector2  p = thisPoint(i);
				Vector2 np = nextPoint(i);


				Vector2 v1 	=  p - pp;
				Vector2 v2 	= np - p;
				
				if (i == 1)
					edgeNormals.Add((new Vector2(v1.y, -v1.x)).normalized);
					
				edgeNormals.Add((new Vector2(v2.y, -v2.x)).normalized);
				
				if (i < controlVertices.Count)
					derivedVertices.Add( controlVertices[i] );

				// ANGLE
				float breakAng = Vector2.Angle(v2, v1);
				
				breakingAngles.Add (breakAng);
				//Debug.Log ("["+i+"] breakAng="+breakAng + ", breakingpolyAng="+ breakAnglePolygonal + ", isSharp="+isSharp (i-1));



				if (isSharp (i-1) && ( (i < controlVertices.Count-1) || (i==controlVertices.Count-1 && isClosed) ) ) // ACUTE ANGLE -- MAKE BREAK
					derivedVertices.Add( controlVertices[i] );
				
				
				// EDGE LENGTHS
				float edgeLen = v1.magnitude;
				edgeLengths.Add(edgeLen);
				
				// EDGE ROTATIONS
				float tmp_edge_ang = -Mathf.Atan2(v1.y, v1.x)*Mathf.Rad2Deg;
				if (tmp_edge_ang < 0)
					tmp_edge_ang += 360;
				edgeRotations.Add(tmp_edge_ang);
				
				// NODE ROTATION & TRANSFORM
				Vector2 v1PN = (new Vector2(v1.y, -v1.x)).normalized;
				Vector2 v2PN = (new Vector2(v2.y, -v2.x)).normalized;
				
				// -- BISECTOR: the addition of the normalized perpendicular vectors leads to a bisector
				Vector2 bisector = v1PN + v2PN ;
				
				nodeNormals.Add (bisector);
				
				
				float tmp_ang = -Mathf.Atan2(bisector.y, bisector.x)*Mathf.Rad2Deg;
				if (tmp_ang < 0)
					tmp_ang += 360;

				
				nodeRotations.Add(tmp_ang);

				
				// BEVEL ANGLE
				float bevelAng = Vector2.Angle(bisector, v2) - 90;

				//Debug.Log("["+i+"] bevelAng="+bevelAng);

				if (i == controlVertices.Count)
					bevelAngle0 = bevelAng;

				//if ((i == controlVertices.Count || i == controlVertices.Count-1) && shapeState == ShapeState.Open)
				//	bevelAng = 0;


				bevelAngles.Add(bevelAng);


				bevelAnglesActual[ ((i==controlVertices.Count)? 0 : i) ] = bevelAng;

                //Debug.Log("["+i+"] pp="+pp+", p="+p+", np="+np+", breakAng="+breakAng+", bevelAng="+ bevelAng+", tmp_ang="+ tmp_ang + ", " + thisPoint(i));

                // -- SCALE: we can get the scaler from the dot product
                float scalerVal = 1 / Vector2.Dot(v1PN, bisector.normalized);
                //float scalerVal = 5; //bisector.magnitude / Vector2.Dot(v1PN, bisector);

                float scaleX = (trueSideSection) ?  scalerVal : 1;

                //scaleX = scalerVal;
                //float x = (trueSideSection) ? p.x : (p.x / scalerVal) ;

                //Debug.Log("%%%% " + scaleX);

				Matrix4x4 m  = Matrix4x4.TRS(new Vector3 (p.x, 0, p.y), Quaternion.Euler(-90, 0, tmp_ang), new Vector3(scaleX, 1, 1));
				//Debug.Log("m["+i+"]="+m);
				nodeTransforms.Add( m);
				
				if (i==1)
				{
					firstPosition = new Vector3 (pp.x, 0, pp.y);
					perpAngleFirst = Mathf.Atan2(v1PN.y, v1PN.x) * Mathf.Rad2Deg;

                   

                }
				if (i==controlVertices.Count-1)
				{
					lastPosition = new Vector3 (p.x, 0, p.y);
					perpAngleLast = Mathf.Atan2(v1PN.y, v1PN.x) * Mathf.Rad2Deg;
				}																
				// RUNNING DISTANCES
				if (i<controlVertices.Count)
					segment_lengths.Add(edgeLen);
				else
					segment_lengths.Insert(0, edgeLen);

				
				running_distance += edgeLen;


				running_distances.Add (running_distance);
			}

			 
			 
			// end bevels?
			float minAng = 20;

			isBeveledA = false;
			isBeveledB = false;

            
            // END A
            float rotA =  -perpAngleFirst;

			if (shapeState == ShapeState.Open)
			{
				if(edgeLengths.Count >= 2)
				{
					if ( autoBevelOpenEnds && Mathf.Abs(bevelAngles[0]) < minAng  && Mathf.Abs(bevelAngles[1]) < minAng  && edgeLengths[0] <= edgeLengths[1]*1.1f)
					{
						isBeveledA = true;
						rotA += bevelAngles[1]; 
						bevelAngles[bevelAngles.Count-1] = bevelAngles[1];
					}
					else if (shapeState == ShapeState.Open)
					{
						bevelAngles[bevelAngles.Count-1] = 0;
					}
				}
			}

			begTransform  =  Matrix4x4.TRS(firstPosition, Quaternion.Euler(-90,  0, rotA), new Vector3(1, 1, 1));
			capATransform =  Matrix4x4.TRS(firstPosition, Quaternion.Euler(  0,  0, rotA), new Vector3(1, 1, 1));

			// END B
			float rotB =  -perpAngleLast;

			if (shapeState == ShapeState.Open)
			{
				if(autoBevelOpenEnds && edgeLengths.Count > 3) 
				{
					if (Mathf.Abs(bevelAngles[bevelAngles.Count-3]) < minAng  && (edgeLengths[edgeLengths.Count-2] < (edgeLengths[edgeLengths.Count-3]*1.1f)))
					{
						isBeveledB = true;
						rotB -= bevelAngles[bevelAngles.Count-3];
						bevelAngles[bevelAngles.Count-2] = bevelAngles[bevelAngles.Count-3];

					}
					else 
					{
						bevelAngles[bevelAngles.Count-2] = 0;
					}
				}
				else if (edgeLengths.Count > 1)
				{
					bevelAngles[bevelAngles.Count-2] = 0;
				}

			}

            


            endTransform =  Matrix4x4.TRS(lastPosition, Quaternion.Euler(-90,  0, rotB), new Vector3(1, 1, 1));
			capBTransform =  Matrix4x4.TRS(lastPosition, Quaternion.Euler(  0,  0, rotB), new Vector3(1, 1, 1));



			// add one to account for last point at the location of the first point
			if (shapeState == ShapeState.Closed)
				allocatedVerticesCount++;
				
			// shift the angles so last is now first
			breakingAngles.Insert(0, breakingAngles[breakingAngles.Count-1]);
			breakingAngles.RemoveAt(breakingAngles.Count-1);
			
			bevelAngles.Insert(0, bevelAngles[bevelAngles.Count-1]);
			bevelAngles.RemoveAt(bevelAngles.Count-1);

			nodeNormals.Insert(0, nodeNormals[nodeNormals.Count-1]);
			nodeNormals.RemoveAt(nodeNormals.Count-1);
			
			nodeRotations.Insert(0, nodeRotations[nodeRotations.Count-1]);
			nodeRotations.RemoveAt(nodeRotations.Count-1);
			
			nodeTransforms.Insert(0, nodeTransforms[nodeRotations.Count-1]);
			nodeTransforms.RemoveAt(nodeTransforms.Count-1);
			
			//edgeLengths.Insert(0, edgeLengths[edgeLengths.Count-1]);
			//edgeLengths.RemoveAt(edgeLengths.Count-1);
			

			if (shapeState == ShapeState.Open)
			{ 	// OPEN
				
			 	// set the end rotations to be perpendicular with the first and last edges
				nodeRotations[0] 	=  edgeRotations[0];
				
				//Debug.Log ("nodeRotations[0]="+nodeRotations[0]);
				
				nodeTransforms[0] 	= Matrix4x4.TRS(new Vector3(controlVertices[0].x, 0, controlVertices[0].y), Quaternion.Euler(-90, 90, nodeRotations[0]), Vector3.one);
			
				nodeRotations[nodeRotations.Count-1] 	= edgeRotations[edgeRotations.Count-1]+90;
				nodeTransforms[nodeRotations.Count-1] 	= Matrix4x4.TRS(new Vector3(controlVertices[controlVertices.Count-1].x, 0, controlVertices[controlVertices.Count-1].y), Quaternion.Euler(-90, -90, nodeRotations[nodeRotations.Count-1]), Vector3.one);
			}
			else
			{	// CLOSED
				derivedVertices.Add( controlVertices[0] );
			}
			
			length = running_distances[controlVertices.Count];
			
			
			
			//Debug.Log (">>>>>>>>>>> derivedVertices.Count="+ derivedVertices.Count);
			//for(int i=0; i<derivedVertices.Count; i++)
			//	Debug.Log ("derivedVertices["+i+"]= " +derivedVertices[i]);
			//for(int i=0; i<nodeRotations.Count; i++)
			//Debug.Log ("nodeRotations["+i+"]= " +nodeRotations[i]);
			
					
			// NORMALIZED U (RUNNING)		
			for (int i=0; i<controlVertices.Count; i++) 
				normalizedUvalues.Add( running_distances[i] / length );
				
				
			length = 0;
			int start = (shapeState == ShapeState.Closed) ? 0 : 1;;


			if (controlVertices.Count == 2)
			{
				length = segment_lengths[0];
			}
			else
			{
				int termin = (shapeState == ShapeState.Closed) ? 0 : 0;
				for (int i=start; i<controlVertices.Count - termin; i++)
				{
					length += segment_lengths[i];
					//Debug.Log("+ " + segment_lengths[i]);
				}
			}
			//Debug.Log("length="+length + ", shapeState="+shapeState);

		}











		/// <summary>
		/// Matrix at a certain length.
		/// </summary>
		/// <returns>The matrix that is positioned and rotated to reflect the spline at a certain length</returns>
		/// <param name="len">Length.</param>

		public Matrix4x4 matrixAtLength(float len) 
		{
			//Debug.Log("len=" + len + "/"+length);

			if (length == 0)
				initializeCurveMetrics(true);
			
			float cummulativeDistance = 0;

			// Note: The negative length needs to be treated differently in open and closed shapes.
			// In an open shape, the negative length extends out infinitely.
			// In a closed shape, step backwards until you find the len location.
			// To do this, the same algorithm has some adjustments 

			bool negCLosed = (isClosed && len < 0);

			// Start from first vert and work backwards or forwards until we find which segment has len.
			int gov = 0;
			int i = 0; 
			while (true)
			{
				if (gov++ > 1000)
					return Matrix4x4.identity;

				int prev_i = prevControlIndex(i);

				Vector2  p = thisPoint(i);
				Vector2 np = (isClosed && len < 0 ) ?  previousPoint(i) : nextPoint(i);

				Vector2 v2 	= negCLosed ? p-np : np - p;

				float tryer = cummulativeDistance+edgeLengths[i];
				//Debug.Log(cummulativeDistance + " ... " + edgeLengths[i] + " tryer="+tryer+" len="+len + " bool: "+ (tryer > len));

				bool tryerGreaterThanLen = negCLosed ? tryer > Mathf.Abs(len) : (tryer > len);

				if ( tryerGreaterThanLen || (!isClosed  &&  i == controlVertices.Count-2)  )
				{
					float remainder = negCLosed ?   Mathf.Abs(len) - cummulativeDistance: len - cummulativeDistance;
					float percentage = remainder/v2.magnitude;

					Vector2 pos2D;

					if (  ! isClosed  &&    ( (i==0 && len < 0)   || (i == controlVertices.Count-2 && len > tryer))     )
						pos2D = p + remainder * v2.normalized;
					else
						pos2D = Vector2.Lerp(p, np,  percentage);

                   
                    Vector3 pos3D = new Vector3(pos2D.x, 0, pos2D.y);

					float roty = negCLosed ? edgeRotations[prev_i] : edgeRotations[i];
					return Matrix4x4.TRS(pos3D, Quaternion.Euler(0, roty, 0), Vector3.one);
				}
				cummulativeDistance = tryer;

				if (negCLosed)
				{
					i--;
					if (i == -1)
						i=controlVertices.Count-1;
				}
				else
				{
					i++;
					if (i == controlVertices.Count)
						i=0;
				}
			}

			//return Matrix4x4.identity;

		}




















		
		public Spline getpOffsetVersion(float offset)
		{
			Spline s = new Spline();
			
			//Debug.Log ("OFFSET="+offset+" ++++++++++++++++++++++++++");

			for (int i=0; i<controlVertices.Count; i++) 
			{
				Matrix4x4 bevelTransform = this.nodeTransforms[i];
				
				if (shapeState == ShapeState.Open)
				{
					if (i==0)
						bevelTransform = this.begTransform;
					else if (i==this.controlVertices.Count-1)
						bevelTransform = this.endTransform;
						
				}	
				
				Vector3 vert3 = bevelTransform.MultiplyPoint( new Vector3(offset,  0, 0) ) ;
				s.controlVertices.Add (new Vector2(vert3.x, vert3.z));
			
				//Debug.Log(controlVertices[i] +" ===> " + new Vector2(vert3.x, vert3.z));
			}
			
			s.initializeCurveMetrics();
			
			
			
				
			return s;
		
		
		}

		

		public void shift(float x, float y) {
		
		for (int i = 0; i<controlVertices.Count; i++) {
				if (controlVertices[i].x == 999999 || controlVertices[i].x == 888888 )
				continue;
				controlVertices[i] = new Vector2(controlVertices[i].x + x, controlVertices[i].y + y);

		}
	}

		
		
		public bool isSharp(int index)
		{
		
			if (  Math.Round(breakingAngles[index]) >= Math.Round(breakAngleGeom) )
				return true;
			
			else
				return false;
		}
		
		public bool isSeam(int index)
		{
			if (index >= breakingAngles.Count)
				return false;

			if ( isSharp(index) || (index == 0 && shapeState == ShapeState.Closed) )
				return true;
			
			else
				return false;
		}
		public bool isBlend(int index)
		{
		//return false;
			//Debug.Log ("index="+index + ", "+bevelAngles[index]);
			if (index >= breakingAngles.Count)
				return false;

			
			if (Math.Round(breakingAngles[index]) <= Math.Round(breakAngleNormals)  )
				return true;
			
			else
				return false;
		}
		
		
		/***********************************
		 From a given vertex, get its previous vertex point.
		 If the given point is the first one, 
		 it will return  the last vertex;
		 ***********************************/
		public Vector2 previousPoint(int index) {		
			if (controlVertices.Count > 0)
				return controlVertices[( ((index-1)<0) ? (controlVertices.Count-1) : index-1)];
			return new Vector2();
		}
		public Vector2 thisPoint(int index) {	
			// useful when you want to be able to stray beyong the Count of the control vertices	
			if (controlVertices.Count > 0)
				return controlVertices[  (index>(controlVertices.Count-1)) ? 0 : index ];
			return new Vector2();
		}
		public Vector2 nextPoint(int index) {		
			if (controlVertices.Count > 0)
				return controlVertices[(index+1) % controlVertices.Count];
			return new Vector2();
		}
		

		// NEXT_I
		public int nextControlIndex(int i)
		{
			i++;
			if (i > controlVertices.Count-1)
				i = 0;
			return i;
		}

		// PREV_I
		public int prevControlIndex(int i)
		{
			i--;
			if (i < 0)
				i = controlVertices.Count-1;
			return i;
		}


		
		
		
		/*************************
	 	 * JOINT_IS_ACUTE
	 	 * See if the joint at an index is acute based on the break angle.
	 	 *************************/
		public bool jointIsAcute(int index) {
			if ( index == 0  ||  index >= (controlVertices.Count-1) ) // FIRST AND LAST ARE NEVER DOUBLED
				return false;	
			
			return (isSharp(index));
		}
		
		public bool bevelJointIsAcute(int index) {
			
			return (isSharp(index));
		}
		
		/*************************
	 	 * CLOSE_JOINT_IS_ACUTE
	 	 * Is joint at vertice[0] acute?
	 	 *************************/

	 	 /// <summary>
	 	 /// Is the close joint is acute?
	 	 /// </summary>
	 	 /// <returns><c>true</c>, if joint is acute was closed, <c>false</c> otherwise.</returns>
		public bool closeJointIsAcute() {
			return isSharp(0);
		}
		

		public void subdivideLongSegments(float desiredMaxLen)
		{
			List<Vector2> newControlVertices = new List<Vector2>();


			newControlVertices.Add(controlVertices[0]);

			for (int i = 1; i<controlVertices.Count; i++)
			{
				//Debug.Log("sub " + i + ", "+segment_lengths[i]);

				subdivideLongSegment(ref newControlVertices, i, desiredMaxLen); 

				//Debug.Log("..... * * * * ** * * * ** * * * ** * * * ** * * * ** * * * ** * * * ** * * * ** * * * *");


				newControlVertices.Add(controlVertices[i]);


			}

			// if open, do last to 0
			//Debug.Log(" * ! * !  * ! * !  * ! * !  * ! * !  * ! * !  * ! * !  * ! * !  * ! * !  * ! * !  * ! * ! shapeState=" +  shapeState);
			if (shapeState == ShapeState.Closed)
				subdivideLongSegment(ref newControlVertices, 0, desiredMaxLen); 


			//Debug.Log("* * * * ** * * * ** * * * ** * * * ** * * * ** * * * ** * * * ** * * * ** * * * *");

			controlVertices = newControlVertices;
			initializeCurveMetrics();

		}

		public void subdivideLongSegment(ref List<Vector2> newControlVertices, int i, float desiredMaxLen )
		{
			if (segment_lengths[i] > desiredMaxLen*1.3f)
			{
				// find the number of bays
			int nbays = Mathf.FloorToInt( segment_lengths[i] / desiredMaxLen );
			float actualBay = segment_lengths[i] / nbays;

			//Debug.Log("nbays="+nbays);
			for (int n = 1; n<nbays; n++)
			{
				// Add a lerped point
				float percentage = (actualBay * n) / segment_lengths[i];
				Vector2 newPoint = Vector2.Lerp(controlVertices[prevControlIndex(i)], controlVertices[i], percentage);

					//Debug.Log("new Point " + n + " " + newPoint);
					newControlVertices.Add(newPoint);
				}
			}
		}


		 /// <summary>
		 /// Divides the Spline into subslpines, where each subspline has no break angles in it.
		 /// This is useful for Spline repeater for placing nodes according to progressinve length rather than vertex positions
		 /// </summary>
		 /// <returns>The repeater spline segements.</returns>

		public List<SubsplineIndices> getSmoothSubsplineIndicies(float desiredbay = 0, float maxLength = 100)
		{
			// CREATE SUBSPLINES
			// use the breakAngleCorners to create sub splines that can be used in the PlanRepeater
			//1. Add indicies until you reacha corner vert, then begin a new 


			//Debug.Log("getSmoothSubsplineIndicies");


			breakIndices = new List<int>();

			max_segmentLength = maxLength;

			subdivideLongSegments(max_segmentLength);




			// break up straight aways by desiredBay


			subsplines = new List<SubsplineIndices>();



			//Debug.Log("================================== controlVertices.Count=" +  controlVertices.Count + " ::: bevelAngle0="+bevelAngle0);

			// FIND FIRST CURSOR VERTEX

			int cursor = 0; // essentially 0

			//Debug.Log("START " + bevelAngle0 + ", " + bevelAngles[cursor] + " " + breakAngleCorners/2);

			if (shapeState == ShapeState.Closed &&  bevelAngle0 <= breakAngleCorners/2)
			{
				cursor = controlVertices.Count-1;

				// back up to first long segment you find

				while (Mathf.Abs(bevelAngles[cursor]) < breakAngleCorners/2  )
				{
					//Debug.Log("["+cursor + "] " + bevelAngles[cursor] + " < " +breakAngleCorners/2);
					if (cursor == 0)
					{
						// if we made it all the way back to 0, then all the segments were too small.
						// just return this AXSpline


						SubsplineIndices ssi = new SubsplineIndices(this);
						ssi.setIndicesToAllSplinePoints();
						//ssi.splineIndices.Add(0);
						ssi.calcLength();

						subsplines.Add(ssi);

						/*
						foreach(SubsplineIndices sssi in subsplines)
						{
							//Debug.Log(" ****** SUBSPLINE");
							foreach (int i in sssi.splineIndices)
								Debug.Log(i);
						}
						*/

						//Debug.Log("RETURNING EARLY!!!!!!");

						return subsplines;
					}
					cursor--;
				}

			}


			// OK: Now we have our FIRST CURSOR starting point: cursor. 
			// Proceed forward from here with the grouping.



			SubsplineIndices rtss = new SubsplineIndices(this);

			//tmp_sp.controlVertices.Add(controlVertices[cursor]);
			rtss.beg_i = cursor;

			int startCursor = cursor;

			//Debug.Log("begin cursor="+cursor + " ::: " + controlVertices[cursor]);

			//Debug.Log("SplineIndices add ~~["+ cursor+"]~~");
			rtss.splineIndices.Add(cursor);

			//Debug.Log("*** BreakIndices add *["+ cursor+"]*");
			breakIndices.Add(cursor);

			// go around controlVerts once....

			for (int ii=0; ii< controlVertices.Count; ii++)
			{

				//Debug.Log(":::::::::::::::::::::::::::::::::::::> ii = " + ii + ", cursor="+cursor);
				cursor++;

				if (cursor == controlVertices.Count)
					cursor = 0;
				
				//Debug.Log(" cursor="+cursor+ " ... <<" + Mathf.Abs(bevelAnglesActual[cursor])+ ">> ::: " + controlVertices[cursor]);

				//float tmpBevAngle = (ii < controlVertices.Count-1)  ? bevelAngles[cursor] : bevelAngle0;
				float tmpBevAngle = bevelAnglesActual[cursor];

				if (cursor == nextControlIndex(cursor))
					tmpBevAngle = bevelAngle0;

				if (shapeState == ShapeState.Open  && (ii == 0 || ii == controlVertices.Count-1) )
				{
					// [ REMOVED 8/30/2017 ]
					//	 tmpBevAngle = 0;
				}


				//Debug.Log("<< ..... "+ii + " >> cursor="+cursor+ ", tmpBevAngle=" + tmpBevAngle + " -- " + prevControlIndex(startCursor));

				if (Mathf.Abs(tmpBevAngle) == 0 || Mathf.Abs(tmpBevAngle) >= breakAngleCorners/2  || (shapeState == ShapeState.Open && cursor == prevControlIndex(startCursor)) )
				{
					//Debug.Log("         >> >>> >>> >>> >>> >>> >>");
					//Debug.Log("   ......      *** BreakIndices add *["+ cursor+"]*");
					breakIndices.Add(cursor);
					rtss.end_i = cursor;

					//Debug.Log( "          SplineIndices add ~~["+ cursor+"]~~");
					rtss.splineIndices.Add(cursor);
					rtss.calcLength();

					//Debug.Log(" >>>>>>> !! >>>>>> !! >>>>>>> !! >>>>>>>> !! >>>>>> " + rtss.length);
					subsplines.Add(rtss);

					//Debug.Log("            NEW SplineIndices ===========================");

					rtss = new SubsplineIndices(this);
					rtss.beg_i = cursor;
					//Debug.Log("         >> >>> >>> >>> >>> >>> >>");

				} 
				//else 
				// Debug.Log("<< NOPE");
			

				//Debug.Log("SplineIndices add ~~["+ cursor+"]~~");
				rtss.splineIndices.Add(cursor);

			}

				//subsplines.Add(rtss);

			if (breakIndices[0] == breakIndices[breakIndices.Count-1])
				breakIndices.RemoveAt(breakIndices.Count-1);







			// PRINT

//			Debug.Log( " *************************************************************************************************");
//			Debug.Log( " *******************                                                      ***********************");
//			//foreach (int i in breakIndices)
//			//		Debug.Log(i);
//
//
//			foreach(SubsplineIndices ssi in subsplines)
//			{
//				
//
//				string println = "";
//				foreach (int i in ssi.splineIndices)
//					if (i == 0)
//						println += i;
//					else
//						println += (", "+i);
//
//				Debug.Log( " ****** SUBSPLINE: " + println);
//			}
//			Debug.Log( " *******************                                                      ***********************");
//			Debug.Log( " *************************************************************************************************");





			return subsplines;

		}















		// Create a list of groupedCornerNodes
		// Group consecutive breakAngle nodes that are closer together than minimum length

		// At the end call createInsetSplines (spanSplines),
		// which will create slines based on the groupedCornerNodes and insets

		// In a sense, we are possibly combining some of the subsplines.


		// If there is only one subSpline and the angle[o] is not break, 
		// then return early with no groupedCornerNodes


		public void createGroupedCornerNodes_NearBreakAngleVertices(float min_len)
		{
			// For each subspline, group points where the subspline length is smaller than min_segmentLength
			int[] groupedIndicies = new int[controlVertices.Count*2];

			int group_index = 0;
			groupedIndicies[group_index] = 0;

			// Add first suspline
			groupedCornerNodes 		=  new List<List<int>>();


			// If there is only one subSpline and the angle[o] is not break, 
			// then return early with no groupedCornerNodes

			//Debug.Log( " ?????? " + bevelAnglesActual[bevelAnglesActual.Length-1] + " .. "  + breakAngleCorners);

			if (subsplines.Count == 1 && bevelAnglesActual[bevelAnglesActual.Length-1] < breakAngleCorners)
			{
				getInsetCornerSplines(min_len/2);
				return;
				
			}

			List<int> tmpList = null;


			int subsplineCursor = 0;

			int startCursor = 0;
			int endCursor = subsplines.Count-1;

			float len = subsplines[subsplineCursor].length;


			//Debug.Log(".... endCursor = "+endCursor);

			//if (shapeState == ShapeState.Closed && subsplines[endCursor].length < min_len)
			if (shapeState == ShapeState.Closed && subsplines[endCursor].length < min_len)
			{

				//Debug.Log("..... Go backwards from first subsubline to find start...");

				// Go backwards from first subsubline to find start...
				for (int i=0; i<subsplines.Count; i++)
				{
					//Debug.Log("i="+i);
					subsplineCursor = prevSubsplineIndex(subsplineCursor);

					if (subsplineCursor == 0)
					{
						// we have gone all the way around and 
						// all of the lengths are small. 
						// Add all the points of this spline to the groupedCornerNodes.

						Spline cornerGroupSpline = new Spline();

						for(int ii=0; ii<controlVertices.Count; ii++)
							cornerGroupSpline.controlVertices.Add(controlVertices[ii]);

						insetSplines = new List<Spline>();
						cornerGroupSpline.shapeState = ShapeState.Closed;
						insetSplines.Add(cornerGroupSpline);



						insetSpanSplines = null;

						return;

					}

					len = subsplines[subsplineCursor].length;
					//Debug.Log(subsplineCursor + ": "+ subsplines[subsplineCursor].beg_i + "-" + subsplines[subsplineCursor].end_i + "  len="+len );

					if (len > min_len)
						break;
					else
						startCursor = subsplineCursor;
				}
			}

			subsplineCursor = startCursor;

			//Debug.Log(">>>>>>> Start at " + startCursor + ", next: " + nextSubsplineIndex(subsplineCursor));

			// startCursor is now our starting cursor... the first short subspline.


			tmpList = new List<int>();
			 
			// KICK IT OFF: BEG_I
			tmpList.Add(subsplines[startCursor].beg_i);

			//Debug.Log(" ??? " + subsplines[startCursor].length + " ... " + min_len);
			if (startCursor == 0 &&  subsplines[subsplineCursor].length > min_len)
			{
				//Debug.Log("new Group at Start");
				groupedCornerNodes.Add(tmpList);
				tmpList = new List<int>();

				tmpList.Add(subsplines[subsplineCursor].end_i);

					
			} 
			else
			{
				tmpList.Add(subsplines[startCursor].end_i);
			}

			endCursor = prevSubsplineIndex(startCursor);


			int governor = 0;
			while(( subsplineCursor=nextSubsplineIndex(subsplineCursor)) != startCursor && subsplineCursor != endCursor)
			{
				if (governor++ > 300)
				{
					Debug.Log("governor hit)");
					return;
				}	

				if (subsplines[subsplineCursor].length > min_len)
				{
					// begin new group
					groupedCornerNodes.Add(tmpList);
					tmpList = new List<int>();
				}

				// keep adding to group
				tmpList.Add(subsplines[subsplineCursor].end_i);
			}
			
			groupedCornerNodes.Add(tmpList);



			// PRINT
//			Debug.Log("!!! ==== groupedCornerNodes.Count = " + groupedCornerNodes.Count);
//			for (int i=0; i<groupedCornerNodes.Count; i++)
//			{
//				Debug.Log("================================================= Group "+i);
//				for (int j=0; j<groupedCornerNodes[i].Count; j++)
//					Debug.Log("============================> "+ groupedCornerNodes[i][j]);
//			}



			getInsetCornerSplines(min_len/2);

		}

		 








		// NEXT_SUBSPLINE_I
		public int nextSubsplineIndex(int i)
		{
			int next_i = i+1;
			if (next_i > subsplines.Count-1)
				next_i = 0;
			return next_i;
		}

		// PREV_SUBSPLINE_I
		public int prevSubsplineIndex(int i)
		{
			int prev_i = i-1;
			if (prev_i < 0)
				prev_i = subsplines.Count-1;
			return prev_i;
		}




		  
		// !!!!!!!! Check if the groupedCornerNodes.Count is 0, if so, then no inset and one spanSpline? !!!!!!



		// MANPAGE: http://www.archimatix.com/uncategorized/axspline-getinsetcornerspines
		public void getInsetCornerSplines(float inset)
		{
			// The subSplines already set are between breaks.

			// Now combine subSplines where the endpoints are closer than the 
			// The spanSlines are subSplines minus the Insets on either end.

			// There can't be more subSplines then there are vertices...
			// Each of these subSplines will have at least 3 points
			 
//			Debug.Log("** ** ** ** derivedVertices.Count="+derivedVertices.Count+" ----->>>> groupedCornerNodes.Count=" + groupedCornerNodes.Count);
//			foreach(List<int> ii in groupedCornerNodes)
//			{
//				Debug.Log("------ groupedCornerNodes");
//				foreach(int i in ii)
//				{
//					Debug.Log(i + ": "+ controlVertices[i]);
//				}
//			}


			if (groupedCornerNodes == null || groupedCornerNodes.Count == 0)
				inset = 0;

			//Debug.Log("** getInsetCornerSplines ** subsplines.Count=" + subsplines.Count);
			if (inset == 0)
			{
				// just transfer the subsplines to spanSplines

				insetSpanSplines 			=  new List<Spline>();

				for (int i=0; i<subsplines.Count; i++)
				{
					Spline s = new Spline();

					for (int k=0; k< subsplines[i].splineIndices.Count; k++)
						s.controlVertices.Add( controlVertices[subsplines[i].splineIndices[k]] );

					if (subsplines.Count == 1 && shapeState == ShapeState.Closed)
					{
						//Debug.Log(" * * * * * * *YUP " + s.controlVertices[0]);
						s.controlVertices.Add( s.controlVertices[0] );
					}


					s.shapeState = ShapeState.Open;
					s.initializeCurveMetrics();

					insetSpanSplines.Add(s);


					if (groupedCornerNodes.Count == 0)
						return;
					
				}

				return;


			}


			if (groupedCornerNodes.Count < 1)
			{
				//insetSplines = null;
				return;
			}


			insetSplines 			=  new List<Spline>();

			insetSpanSplines 			=  new List<Spline>();

			for (int i=0; i<groupedCornerNodes.Count; i++)
				insetSpanSplines.Add(new Spline());

			//Debug.Log("OKKK: "+ groupedCornerNodes.Count);

			if (groupedCornerNodes.Count == 1 && groupedCornerNodes[0].Count == controlVertices.Count)
			{
			//Debug.Log("..... yo");
				Spline cornerGroupSpline = new Spline();

				for(int i=0; i<controlVertices.Count; i++)
					cornerGroupSpline.controlVertices.Add(controlVertices[i]);

				cornerGroupSpline.shapeState = ShapeState.Open;
				insetSplines.Add(cornerGroupSpline);
				return;
			}




			
			// PLANSWEEPS AT CORNERS
			// Grouped corner nodes that are closer to ech other than the bay min.

			// First, go around and group verts that are closer
			// to  each other than min_sengmentLength
			min_segmentLength = 2*inset;





			// use groupedCornerNodes
			int lastIndexBeforeInsertedVert = 0;
			//int lastIndexBeforeLeftInsertedVert_0 = 0;





			// span0 is behind Group0 and must be calulated after last Group is done.

			int span0_toInnerControlPoint = 0;
			Vector2 span0_toDerivedPoint = Vector2.zero;





			for ( int i=0; i<groupedCornerNodes.Count; i++)
			{
				//Debug.Log("Group "+i+ " ========================================================================>>>>>>");

				// ADD LERPED INSET BEGIN POINT
				// Take into account that there may b curve points to follow allong until the inset distance is reached.
				// So, work backwards along the curve from the 0 point 
				Spline cornerGroupSpline = new Spline();

				List<int> groupedNodes = groupedCornerNodes[i];


//				Debug.Log("========================================================= Group "+i);
//				for (int j=0; j<groupedCornerNodes[i].Count; j++)
//					Debug.Log("====> "+ groupedCornerNodes[i][j]  + " ::: " + controlVertices[ groupedCornerNodes[i][j]] );



				// back up to where to insert the inset point.
				int controlVerts_cursor = groupedNodes[0];

				int groupCursor = 0;

				float percentage = 0;

				float remainingRunningDist = inset;
				int governor = 0;


				int terminalCursor = nextControlIndex(groupedNodes[groupedNodes.Count-1]);

				// the last regular index before the inserted righthand vert


				// INSET TO THE LEFT OF THE GROUP. WHILE running distance not used up
				//Debug.Log(" ************************************* group " + i + " shapeState="+shapeState);
				if (i > 0 || shapeState == ShapeState.Closed)
				{

					for (int control_i=0; control_i<controlVertices.Count; control_i++)
					{


						int this_index = controlVerts_cursor;
						int prev_index = prevControlIndex(controlVerts_cursor);

						//Debug.Log(control_i + ": this_index="+this_index+", prev_index="+prev_index +" ::: len = " + segment_lengths[controlVerts_cursor] + ", remainingRunningDist="+remainingRunningDist);

						if (segment_lengths[controlVerts_cursor] <= remainingRunningDist) {
						
							// add to runningDist and carry on...segment_lengths[controlVerts_cursor]
							remainingRunningDist -= segment_lengths[controlVerts_cursor];
						
						} else {
							
							// OK - FOUND LAST CONTROL VERT in the inset before this group.
							// Now create lerped start and then proceed foward until the last groupNode

							// Lerp between this_index and prev_index based on the running dist left

							percentage = 1-(remainingRunningDist/segment_lengths[controlVerts_cursor]);

							//Debug.Log("BEG-percentage: "+percentage + " remainingRunningDist=remainingRunningDist="+remainingRunningDist+", segment_lengths[controlVerts_cursor]=segment_lengths[controlVerts_cursor="+segment_lengths[controlVerts_cursor]);
							Vector2 endCorner_BegSpanVert = Vector2.Lerp (controlVertices[prev_index], controlVertices[this_index], percentage);
							//Debug.Log ("begVert=" + endCorner_BegSpanVert);

							// BEGIN CORNER SPLINE....
							cornerGroupSpline.controlVertices.Add(endCorner_BegSpanVert);



	
							// NOW SPANS

							// Add any span points between previous right side and this
							// Add insert point as end of previous span.
							int prevGroup_i = (i==0) ? groupedCornerNodes.Count-1 : i-1;
							int fromIndex = nextControlIndex(lastIndexBeforeInsertedVert);

							//Debug.Log("NOW SPANS: ["+i+"] fromIndex=" + fromIndex + ", prevGroup_i="+prevGroup_i +" of "+ (groupedCornerNodes.Count-1) + " :::: " + groupedCornerNodes[prevGroup_i][groupedCornerNodes[prevGroup_i].Count-1]);




							if (i == 0) {

								span0_toDerivedPoint = endCorner_BegSpanVert;
								span0_toInnerControlPoint = this_index;
							}
							else 
							{
								//Debug.Log(i + " FROM LAST.... lastIndexBeforeInsertedVert=" +fromIndex + " ...to... " + prev_index);
								int fillCursor = fromIndex;
								governor = 0;
								while (fillCursor != this_index)
								{
									if (governor++ > 500)
									{
										Debug.Log("governor hit)");
										return;
									}	
									//Debug.Log (" >>>>>>>>>>============ FILL CURSOR > " + fillCursor + " :: " + controlVertices[fillCursor]);
									insetSpanSplines[prevGroup_i].controlVertices.Add(controlVertices[fillCursor]);


									fillCursor = nextControlIndex(fillCursor);
								}
							

								// end the span with this begVert

								insetSpanSplines[prevGroup_i].controlVertices.Add(endCorner_BegSpanVert);


							}



							// add all verts between this  this_index and group's end...

							//int groupBegIndex =  groupedNodes[0];
							//int groupEndIndex = groupedNodes[groupedNodes.Count-1];


							groupCursor = this_index;

							terminalCursor = groupedNodes[0];
							 
							while (groupCursor != terminalCursor)
							{
								if (governor++ > 500)
								{
									Debug.Log("governor hit)");
									return;
								}	

								cornerGroupSpline.controlVertices.Add(controlVertices[groupCursor]);

								//if (i == 0)
								//	lastIndexBeforeLeftInsertedVert_0 = prev_index;

								groupCursor = nextControlIndex(groupCursor);
							}

							break;
						}
						controlVerts_cursor = prev_index;
					}
				}


				// MIDDLE - the points including and between this group's start and end.
				groupCursor = groupedNodes[0];
				terminalCursor = nextControlIndex(groupedNodes[groupedNodes.Count-1]);

				//	Debug.Log("middle: groupCursor=" + groupCursor + ", terminalCursor="+terminalCursor);
				governor = 0;
				bool mustDoFirst = true;
				while (groupCursor != terminalCursor || mustDoFirst)
				{
					if (governor++ > 500)
					{
						Debug.Log("governor hit)");
						return;
					}	
					mustDoFirst = false;

					cornerGroupSpline.controlVertices.Add(controlVertices[groupCursor]);
					groupCursor = nextControlIndex(groupCursor);
				}


				// END OF CORNER INSET: now add END lerp...

				if (  i < (groupedCornerNodes.Count-1)  || shapeState == ShapeState.Closed)
				{
					remainingRunningDist = inset;
					governor = 0;



					lastIndexBeforeInsertedVert = groupedNodes[groupedNodes.Count-1];
					//lastIndexBeforeInsertedVert = groupedNodes[0];

					groupCursor = nextControlIndex(groupedNodes[groupedNodes.Count-1]);
					//Debug.Log("groupedNodes.Count="+groupedNodes.Count+", groupCursor="+groupCursor+", segment_lengths[groupCursor]="+segment_lengths[groupCursor]+", groupedNodes[groupedNodes.Count-1]="+groupedNodes[groupedNodes.Count-1]);

					while (remainingRunningDist > segment_lengths[groupCursor])
					{
						if (governor++ > 500)
						{
							Debug.Log("governor hit)");
							return;
						}	

						remainingRunningDist -= segment_lengths[groupCursor];

						cornerGroupSpline.controlVertices.Add(controlVertices[groupCursor]);
						lastIndexBeforeInsertedVert = groupCursor;

						groupCursor = nextControlIndex(groupCursor);
					}
					percentage = remainingRunningDist/segment_lengths[groupCursor];

					//Debug.Log("END-percentage: "+percentage + " remainingRunningDist=remainingRunningDist="+remainingRunningDist+", groupCursor="+groupCursor+", segment_lengths[controlVerts_cursor]=segment_lengths[controlVerts_cursor="+segment_lengths[groupCursor]);


					Vector2 endVert = Vector2.Lerp (controlVertices[prevControlIndex(groupCursor)], controlVertices[groupCursor], percentage);

					//Debug.Log("[["+lastIndexBeforeInsertedVert+"]] Adding Endvert: " + endVert);

					cornerGroupSpline.controlVertices.Add(endVert);
					insetSpanSplines[i].controlVertices.Add(endVert);

					//Debug.Log ("ADD TO INSET SPLINE ["+i+"] " + endVert);

				}
				cornerGroupSpline.shapeState = ShapeState.Open;
				insetSplines.Add(cornerGroupSpline);
			}
			 

			// FINISH LAST INSET_SPAN BETWEEN GROUP LAST ANS GROUP )
			//Debug.Log("+++++++++++++++++++++)))))))))++++++++++++++++ lastIndexBeforeInsertedVert=" + lastIndexBeforeInsertedVert + ", span0_toDerivedPoint="+span0_toDerivedPoint+", span0_toInnerControlPoint=" + span0_toInnerControlPoint);

			int fillCursorLast = nextControlIndex (lastIndexBeforeInsertedVert);

			//Debug.Log ("fillCursorLast="+fillCursorLast + " :: span0_toInnerControlPoint=" + span0_toInnerControlPoint);

			int governor0 = 0;
			while (fillCursorLast  != span0_toInnerControlPoint ) {
				if (governor0++ > 300)
				{
					Debug.Log("governor hit)");
					return;
				}	


				insetSpanSplines.Last ().controlVertices.Add (controlVertices [fillCursorLast]);
				fillCursorLast = nextControlIndex (fillCursorLast);
			}

			// Derived inset point from Group0
			insetSpanSplines.Last ().controlVertices.Add (span0_toDerivedPoint);


			//int fillCursorLast = span_from_last;
//			int governorLast = 0;
//			while (fillCursorLast != span_to_last)
//			{
//				if (governorLast++ > 500)
//				{
//					Debug.Log("governor hit)");
//					return;
//				}	
//				Debug.Log (" >>>>>>>>>>============ FILL CURSOR > " + fillCursorLast + " :: " + controlVertices[fillCursorLast]);
//				insetSpanSplines[groupedCornerNodes.Count-1].controlVertices.Add(controlVertices[fillCursorLast]);
//
//
//				fillCursorLast = nextControlIndex(fillCursorLast);
//			}



 			// SPAN SPLINES

 			// For the span splines, use the groupedNodes for the indexes and use the inset splines for the inserted inset vertices.

 			for (int i = 0; i < insetSpanSplines.Count; i++) 
 			{
 				if (insetSpanSplines[i] != null)
 				{
					insetSpanSplines[i].shapeState = ShapeState.Open;
					insetSpanSplines[i].initializeCurveMetrics();

					//Debug.Log(spanSplines[i].toString());
				}
			}


			/*
			for ( int i=0; i<groupedCornerNodes.Count; i++)
			{
				Spline spanSpline = new Spline();

				int next_i = (i+1) % groupedCornerNodes.Count;

				int lastGroupedNodeIndex = groupedCornerNodes[i]		[groupedCornerNodes[i].Count-1];
				int nextGroupedNodeIndex = groupedCornerNodes[next_i]	[groupedCornerNodes[next_i].Count-1];

				int leftRegularIndex 	= nextControlIndex(lastGroupedNodeIndex);
				int rightRegularIndex 	= prevControlIndex(nextGroupedNodeIndex);

				Vector3 leftInsetVert 	= cornerSplines[i].controlVertices[cornerSplines[i].controlVertices.Count-1];
				Vector3 rightInsetVert 	= cornerSplines[next_i].controlVertices[0];

				Debug.Log("*******");
				Debug.Log(i + " :: " + next_i);
				Debug.Log(leftInsetVert + " :: " + rightInsetVert);
				Debug.Log(leftRegularIndex + " :: " + rightRegularIndex);

				spanSpline.controlVertices.Add(leftInsetVert);

				// add in between verts
				int cursor = leftRegularIndex;

				int governor = 0;

				Debug.Log("WHILE: "+cursor + " != " + nextGroupedNodeIndex);
				while(cursor != nextGroupedNodeIndex)
				{
					if (governor++ > 500) 
						break;
					Debug.Log("cursor="+cursor);

					cursor = nextControlIndex(cursor);
				}

				spanSpline.controlVertices.Add(leftInsetVert);

				//for(int ii=cornerSplines[i].Last; ii
			}
			*/
			

		}







		public void setRepeaterTransforms(int i, float inset, float bay)
		{
			// For a particular spanSpline[i]
			// Calculate the transforms for repeater nodes and cells
			// that occur at regular intervals along the spanSpline 
			// regardless of its vertices.
			// Save them in Lists, such as repeaterNodeTransforms, etc.
			// 
			// The first and last verts of the spanSpline will always be included.

			int 	nbays 		= 0;
			float 	actual_bay 	= 0;

			/*
			if (shapeState == ShapeState.Closed)
			{

				if (Vector2.Distance(controlVertices[0], controlVertices[controlVertices.Count-1])  > samePointTolerence && controlVertices.Count >1)
				{
					// add the first point to the last
					controlVertices.Add(controlVertices[0]);
				}

			}
			*/


			repeaterNodePositions = new List<Vector2>();
			repeaterCellPositions = new List<Vector2>();

			repeaterNodeRotations = new List<float>();
			repeaterCellRotations = new List<float>();


			repeaterNodeTransforms = new List<Matrix4x4>();
			repeaterCellTransforms = new List<Matrix4x4>();

			//Debug.Log("controlVertices.Count="+controlVertices.Count);


			if (length == 0)
			{

				initializeCurveMetrics(true);
			}

			//Debug.Log(" $$$$$ " + this.toString());

			//Debug.Log(".....length = " + length);

			if (length > 0)
			{
				nbays 		= Mathf.CeilToInt( length / bay );

				actual_bay 	= length / nbays;
			}
			else
				return;


			//Debug.Log("shapeState="+shapeState+", ** length="+length+", nbays = "+nbays+", actual_bay=" + actual_bay);

			float 	cummulativeNodeDistance 	= 0;

			float 	percentage 					= 0;

			Vector2 position2D		= Vector2.zero;
			Vector2 prevPosition2D	= Vector2.zero;

			Vector3 prevPosition3D 	= Vector3.zero;
			Vector3 position3D 		= Vector3.zero;

			float rot = 0;
			float prevRot = 0;

			float prev_running_distance 	= 0;
			float next_running_distance 	= 0; //segment_lengths[nextIndex(cursor)];

			//Debug.Log("LOOPER");



				


			for (i=0; i<controlVertices.Count-1; i++)
			{
				//if (controlVertices.Count>2) Debug.Log(" %%% "+i);

				//int prev_i = prevControlIndex(i);
				int next_i = nextControlIndex(i);



				next_running_distance += segment_lengths[next_i];


				int governor = 0;

			
				while ((next_running_distance*1.01f) >= cummulativeNodeDistance)
				{
					if (governor++ > 150)
					{
						Debug.Log("governor hit)");
						return;
					}	
 
					//Debug.Log("governor="+governor); 
					// Add a point for this

					// ** Position **

					// Get percentage for Lerp between this vert and the previous one.
					percentage = (cummulativeNodeDistance - prev_running_distance) / segment_lengths[next_i];

					//Debug.Log("cummulativeNodeDistance="+cummulativeNodeDistance+", prev_running_distance="+prev_running_distance+", segment_lengths["+next_i+"]="+segment_lengths[next_i]+", percentage="+ percentage);

					// do Lerp between points
					if (repeaterNodeTransforms.Count > 0)
					{
						prevPosition2D = position2D;
						prevPosition3D = position3D;
					}

					position2D = Vector2.Lerp(controlVertices[i], controlVertices[next_i], percentage);
					position3D = new Vector3(position2D.x, 0, position2D.y);


					//if (i == controlVertices.Count-1 && ! areSamePoints(position2D, controlVertices[0]) )
						repeaterNodePositions.Add(position2D);


					//Debug.Log(position3D);

					// ** Rotation **
					// Normal to line segment 

					prevRot = rot;

					rot = edgeRotations[next_i]-180;


					// Interpolate between bevel angles


					if (controlVertices.Count > 2)
					{
						if (i == 0)
							rot = edgeRotations[0];
						else if (i == nodeRotations.Count-2)
							rot = edgeRotations[nodeRotations.Count-2];
						else if ( Mathf.Abs(bevelAngles[i]) < 22 && Mathf.Abs(bevelAngles[next_i]) < 22)
							rot = Mathf.LerpAngle(nodeRotations[i]-90, nodeRotations[next_i]-90, percentage);
					}
					//if (controlVertices.Count>2) Debug.Log("bevelAngles["+i+"]="+Mathf.Abs(bevelAngles[i]) + ", bevelAngles["+next_i+"]="+Mathf.Abs(bevelAngles[next_i]) + " >>> " + rot);

					repeaterNodeRotations.Add(rot);

					repeaterNodeTransforms.Add(Matrix4x4.TRS(position3D, Quaternion.Euler(0, rot, 0), Vector3.one));

					//Debug.Log(spanNodeTransforms[spanNodeTransforms.Count-1]);

					if (repeaterNodeTransforms.Count > 1)
					{

						Vector2 cellCenterPosition2D = Vector3.Lerp(prevPosition2D, position2D, 0.5f);
						repeaterCellPositions.Add(cellCenterPosition2D);

						Vector3 cellCenterPosition = Vector3.Lerp(prevPosition3D, position3D, 0.5f);
						repeaterCellRotations.Add(Mathf.LerpAngle(prevRot, rot, 0.5f));

						repeaterCellTransforms.Add(Matrix4x4.TRS(cellCenterPosition, Quaternion.Euler(0,  Mathf.LerpAngle(prevRot, rot, 0.5f), 0), Vector3.one));
					}



					cummulativeNodeDistance += actual_bay;

					//Debug.Log((next_running_distance) + " >= " + cummulativeNodeDistance +" ??? ");

				}



				prev_running_distance = next_running_distance;
			}


		}
















		public bool areSamePoints(Vector2 a, Vector2 b)
		{

			if (Vector2.Distance(a, b)  < samePointTolerence)
				return true;

			return false;

		}


		/*************************
		 * TO_STRING
		 *************************/
		
		public string toString() {
			
			string ret = "Spline ("+controlVertices.Count+") [ ";
			for (int i=0; i<controlVertices.Count; i++) {
				if (i>0) ret += ",  ";
				ret += controlVertices[i].x.ToString("F4") + ", " + controlVertices[i].y.ToString("F4");	
			}
			ret += " ]";
			return ret;
		}
		
		












		
		public static void testSpline()
		{
			Debug.Log ("HERE");
			
			Path pathh = new Path();
			pathh.Add(new IntPoint(0, 0));
			pathh.Add(new IntPoint(2000, 0));
			pathh.Add(new IntPoint(3000, AXGeometry.Utilities.IntPointPrecision));
			pathh.Add(new IntPoint( 0,   AXGeometry.Utilities.IntPointPrecision));
			//pathh.Add(new IntPoint(0, 0));
			
			Spline spl = new Spline(pathh, false);
			
			//spl.setIsClosed(true);
			
			foreach(float d in spl.running_distances)
				Debug.Log (d);
			
			Debug.Log ("spl.splineLength="+spl.length);
			
			foreach(float d in spl.edgeLengths)
				Debug.Log (d);
			
			Debug.Log ("======= breaking angles ========");
			foreach(float a in spl.breakingAngles)
				Debug.Log (a);
			
			Debug.Log ("======= edge rots ============");
			foreach(float a in spl.edgeRotations)
				Debug.Log (a);
			
			Debug.Log ("======= nodeRots ==============");
			foreach(float a in spl.nodeRotations)
				Debug.Log (a);
			
			Debug.Log ("======= derived vertices ========");
			Debug.Log ("allocated="+spl.derivedVertices.Count);
			
			foreach(Vector2 dv in spl.derivedVertices)
				Debug.Log (dv);
			
			
			Debug.Log ("breakangle="+spl.breakAngleGeom);
		}
		
		
		
		public static Spline Circle(float radius, int segs, float breakP, float breakN)
		{
			List<Vector2> circle = new List<Vector2>();
				
			// circle spline creation...
			for (int i = 0; i<segs; i++) {
				float rads = Mathf.Deg2Rad*(i*360/(float)segs);
				Vector2 pt = new Vector2(radius*Mathf.Cos(rads),radius*Mathf.Sin(rads));
				circle.Add (pt);
			}
			Spline s = new Spline(circle, true, breakP, breakN);
			return s;
		}
		
		public static Spline Arc(float radius, float begAngle = 45, float endAngle = 135, int segs = 8, float breakP = 60, float breakN = 60)
		{
			List<Vector2> arcPoints = new List<Vector2>();

			float arcAngle = endAngle-begAngle;

			float deltaAngle = arcAngle / (float) segs;

			// circle spline creation...
			for (int i = 0; i<=segs; i++) {
				float rads = Mathf.Deg2Rad*(begAngle + i*deltaAngle);
				Vector2 pt = new Vector2(radius*Mathf.Cos(rads),radius*Mathf.Sin(rads));
				arcPoints.Add (pt);
			}

			bool isClosed = (deltaAngle < 360) ?  false : true;

			Spline s = new Spline(arcPoints, isClosed, breakP, breakN);
			return s;
		}
		

		
		
				
	} // Spline
	
	
	
} // AX
