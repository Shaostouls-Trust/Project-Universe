using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;



/*
* When testing this conversion from javascript to c#, look out for the change in the test for the results of AXSegIntersectionResult
* since functions that return it, can't just return null. So the test for the result of segIntersection(A, B, C, D) and lineIntersection(A, B, C, D)
* need to test for if results.t in null, not results.
* 
* Also mind that previoustPoint() and nextPoint() now return zero vectors rather than null if wither fail. This should be ok because the 
* function should never fail such that the null value is returned. 
* 
*/


namespace AXGeometry
{

	[System.Serializable]
	public class AXSpline {

		// Persistent
		public string 		name				= "spline";
		public string 		key;
		public bool 		isControllable = false;

		public bool isHole;

		// clear each time
		private Vector2[] 	_verts;
		public 	Vector2[] 	verts {
			get  { return _verts;  }
			set  { _verts = value; }

		}


		private int 	_vertCount 			= 0;
		public 	int 	vertCount {
			get  { return _vertCount;  }
			set  { _vertCount = value; }
			
		}

		// Trying this out.
		// rather than use a vertex delimeter all the time
		// why not just maintain a list of subsplines? 
		// These sublines should always be considered as positive, or solids rather than voids.
		// The only time 8888888 is used as a vertex delimeter is when there is an assumption 
		// that the "void" or "hole" is always inside the verts in which it is listed.
		[System.NonSerialized]
		public List<AXSpline> subsplines;

		public static int 	builtinAllocBlock 	= 60;

		
		public float[] 		angles				= null;	
		public int 			sharpVertsInside_Count 		= 0;
		public float 		breakAngle 			= 60.0f;



		// each vert has a u-value for texture mapping based on wheter it is one
		// of the two verts making an acute angle or a blended vert at some point between acute verts
		// the uValues array will have as many items as (vertCount + sharpVertsCt)
		public float[] 		uValues				= null; 
		public float length = 0.0f;  // The overall length along the cure
		
		public bool isClosed = true;
		
		public bool isTriangle = false;

		public bool isDirty = false;
		
		// to support clipping
		AXPointList poly1;
		AXPointList poly2;
		
		public float minX =  1999999;
		public float maxX = -1999999;
		public float minY =  1999999;
		public float maxY = -1999999;
		
		public float cenX = 1999999;
		public float cenY = 1999999;
		
		public float width;
		public float height;
		
		public bool isProductOfBooleanOperation = false;

		// 
		public float[] segment_lengths;
		public float[] curve_distances;


		/* constructors */

		public AXSpline() {
			Clear();
		}

		public AXSpline(int count) {
			vertCount = 0;
			int newSize = (((count + 1) / builtinAllocBlock) + 1) * builtinAllocBlock;
			verts = new Vector2[newSize];
		}
		public AXSpline(Vector2[] _verts) {
			verts = _verts;
			vertCount = verts.Length;
		}

			

		public AXSpline(Vector2 pt0, Vector2 pt1, Vector2 pt2) {
			// triangle constructor -- 
			// IMPORTANT: assume that Push will not be used
			
			isTriangle = true;
			
			vertCount = 3;
			
			verts = new Vector2[3];
			
			verts[0] = pt0;
			verts[1] = pt1;
			verts[2] = pt2;
			
			
		}


		public AXSpline(List<AXSpline> splines) 
		{
			// Use a list of splines to make one spline
			// with that contain delimeter vertices such as 999999, 999999
			// The splines may have delimeters within them, but this is no matter
			vertCount = 0;
			foreach (AXSpline s in splines)
				vertCount += s.vertCount;

			// add space for delimeters
			vertCount += (splines.Count-1);

			verts = new Vector2[vertCount+10];

			int vc = 0;
			for (int i=0; i<splines.Count; i++)
			{
				if (i > 0)
					verts[vc++] = new Vector2(999999, 999999);

				for(int j=0; j<splines[i].vertCount; j++)
					verts[vc++] = splines[i].verts[j];
			}


		}
		

		/*************************
		 * CLEAR
		 *************************/
		public void Clear() {
			verts 			= new Vector2[builtinAllocBlock];
			vertCount 		= 0;
			sharpVertsInside_Count 	= 0;

			angles 			= null;
			uValues 		= null;
		
		
			minX 			=  1999999;
			maxX 			= -1999999;
			minY 			=  1999999;
			maxY 			= -1999999;
			
			cenX 			= 1999999;
			cenY 			= 1999999;
			

		
		}


		public void removeLastVertIfIdenticalToFirst()
		{
			if (Vector3.Distance (verts[0], verts[vertCount-1]) < .001)
				RemoveAt (vertCount-1);
		}


		/* getter methods 
		 * 
		 * 		getVertsAsVector2s()
		 * 		getVertsAsVector3s(axis: String)
		 * 		getVertsAsVector3s(string axis, float axisValue)
		 * 
		 */
			
		public Vector2[] getVertsAsVector2s() {
			// Makes sure that the array returned 
			// has no empty items at the end.
			
			int vc =  this.vertCount;

			Vector2[] tmpverts = new Vector2[vc];
			
			
			for(int i = 0; i<vc; i++) {
				tmpverts[i] = new Vector2(verts[i].x, verts[i].y);
			}
			return tmpverts;
			
		}
		public Vector2[] getVertsAsVector2sOpen() {
			// Makes sure that the array returned 
			// has no empty items at the end.
			
			int vc =  this.vertCount;
			if (Vector3.Distance (verts[0], verts[vc-1]) < .001)
				vc--;
			
			Vector2[] tmpverts = new Vector2[vc];
			
			
			for(int i = 0; i<vc; i++) {
				tmpverts[i] = new Vector2(verts[i].x, verts[i].y);
			}
			return tmpverts;
			
		}

		public Vector2[] getVertsAsVector2sReverse() {
			Vector2[] tmpverts = new Vector2[vertCount];
			
			for(int i = (vertCount-1); i>=0; i--) {
				tmpverts[i] = new Vector2(verts[i].x, verts[i].y);
			}
			return tmpverts;
			
		}
		public Vector2[] getVertsAsVector2sReverseOpen() {
			int vc =  this.vertCount;
			if (Vector3.Distance (verts[0], verts[vc-1]) < .001)
				vc--;
			
			Vector2[] tmpverts = new Vector2[vc];		

			for(int i = (vc-1); i>=0; i--) {
				tmpverts[i] = new Vector2(verts[i].x, verts[i].y);
			}
			return tmpverts;
			
		}

		public Vector3[]  getVertsAsVector3s(string axis) {
			return getVertsAsVector3s(axis, 0.0f, false);
		}
		public Vector3[]  getVertsAsVector3s(string axis, float axisValue) {
			return getVertsAsVector3s(axis, axisValue, false);
		}
		public Vector3[]  getVertsAsVector3s(string axis, bool asClosed) {
			return getVertsAsVector3s(axis, 0.0f, asClosed);
		}

		Vector3[] getVertsAsVector3s(string axis, float axisValue, bool asClosed) {
			//Debug.Log("WOW");
			int vcount = vertCount;
			if (asClosed)
			{
				vcount++;
			}


			Vector3[] tmpverts = new Vector3[vcount];

			int i = 0;
			for(i=0; i<this.vertCount; i++) {
				if (axis == "Z") {
					tmpverts[i] = new Vector3(verts[i].x, verts[i].y, axisValue);
				} else {
					tmpverts[i] = new Vector3(verts[i].x, axisValue, verts[i].y);
				}
			}
			if(asClosed)
				tmpverts[i] = new Vector3(tmpverts[0].x, tmpverts[0].y, tmpverts[0].z);

			return tmpverts;
			
		}
		




		/*************************
		 * PUSH
		 *************************/
		public void Push(Vector2 v) {
			
			if (isTriangle) {
				Debug.Log("Spline.Push - " + v + " -Can't add points to a Spline if it was initialized with the Triangle constructor");
				return;	
			}
			
			CheckToReallocBuiltinVector2();
			
			verts[vertCount] = v;
			vertCount++;

			isDirty = true;
			
		}
		
		void CheckToReallocBuiltinVector2() {
			//Check if we potentially hit the end of the array
			int sizeCheck = ((vertCount + 1) % builtinAllocBlock);
			// - Debug.Log("Size check - count: " + vertCount + " sizecalc: " + sizeCheck);
			if( sizeCheck  == 0 )
			{
				int newSize = (((vertCount + 1) / builtinAllocBlock) + 1) * builtinAllocBlock;
				// - Debug.Log("Reallocation of vert builtin array from size: " + vertCount + " to: " + newSize);
				//We need a more space.
				Vector2[] temp = new Vector2[newSize];
				
				//Crude copy
				for(int i = 0; i < vertCount; i++)
				{
					temp[i] = verts[i];
				}
				
				//Overwrite old
				verts = temp;
			}
		}

		/*************************
		 * POP
		 *************************/
		public Vector2 Pop() {
			
			Vector2 retv = verts[(vertCount-1)];
			
			// later optimze by checking if the size of the allocated verts could be reduced...
			
			vertCount--;

			isDirty = true;
			
			return retv;
		}
		
	/*

	0.		...0
	 -- 	...1
	1.		...2
	2.		...3

	*/

		
		/*************************ert
		 * INSERT_AT
		 *************************/
		public void InsertAfter(int index, Vector2 v) {
			Vector2[] nverts = new Vector2[verts.Length];
			
			if(vertCount < 0) 
				return;
			
			if (index > vertCount)
				index = vertCount;
				
			
			for (int i = 0; i<=vertCount; i++) {
				
				if (i <= index)
				{
					nverts[i] = verts[i];
					
				}
				else if (i == index+1)
				{
					nverts[i] = v;
				}
				else
				{
					nverts[i] = verts[i-1];
				}
				Debug.Log (i + ": " + nverts[i]);
			}
			verts = nverts;
			vertCount++;
			isDirty = true;
		}
		
		
		
		
		/*************************
		 * REMOVE_AT
		 *************************/
		public void RemoveAt(int index) {
			if(index >= vertCount || vertCount < 0) {
				return;
			}
			vertCount--;
			for (int i = index; i<vertCount; i++) {
				verts[i] = verts[i+1];
			}
			
			isDirty = true;
		}
		
		/*************************
		 * REVERSE
		 *************************/
		public AXSpline Reverse() {
			
			AXSpline s = new AXSpline();
			
			for(int i = (vertCount-1); i >= 0; i--) {
				s.Push(verts[i]);
			}
			verts = s.verts;

			isDirty = true;
			
			return s;
		}




		
		/*************************
		 * ATTACH
		 *************************/
		public AXSpline attach(AXSpline s) {
			AXSpline ret = new AXSpline( vertCount + s.vertCount + 1 );
			
			for (int i = 0; i<vertCount; i++) {
				ret.Push(verts[i]);
			} 
			
			ret.Push(new Vector2(999999,999999));
			
			for (int j = 0; j<s.vertCount; j++) {
				ret.Push(s.verts[j]);
			} 
			
			isDirty = true;
			
			return ret;	
		}
		
		
		
		/*************************
		 * ATTACH_HOLE
		 *************************/
		public AXSpline attachHole(AXSpline s) {

			AXSpline ret = new AXSpline( vertCount + s.vertCount + 1 );
			
			for (int i = 0; i<vertCount; i++) {
				ret.Push(verts[i]);
			} 
			
			ret.Push(new Vector2(888888,888888));
			
			for (int j = 0; j<s.vertCount; j++) {
				ret.Push(s.verts[j]);
			} 
			
			isDirty = true;
			
			return ret;	
		}
		
		


		
		/*************************
		 * SUB SPLINES
		 * 
		 * returns all the subslines (delimited by 999999)
		 * as a List
		 *************************/
		public List<AXSpline> getSubsplines()
		{
			if (subsplines == null && verts != null && verts.Length > 0)
			{
				subsplines = new List<AXSpline>();

				AXSpline s = new AXSpline();
				s.isClosed = isClosed;
				s.breakAngle = breakAngle;

				for (int i = 0; i<vertCount; i++)
				{
					//Debug.Log (verts[i].x + " in vert " + i);
					if (verts[i].x == 999999)
					{
						//Debug.Log("new sub");
						subsplines.Add(s);
						s = new AXSpline();
						s.breakAngle = breakAngle;
						s.isClosed = isClosed;
					} 
					else
						s.Push(verts[i]);
				}
				subsplines.Add(s);


				if (subsplines.Count == 1)
					subsplines[0].isClosed = isClosed;
			}
			//Debug.Log (subs.Count + " subs");
			return subsplines; 
		}

		/*************************
		 * GET_SOLID_AND_HOLES SPLINES
		 * 
		 * returns all the subslines (delimited by 999999)
		 * as a List
		 * 
		 * In the return List,
		 * the first Spline is the solid,
		 * while the following splines are holes
		 * totally enclosed in that spline
		 *************************/
		public List<AXSpline> getSolidAndHoles()
		{
			//Debug.Log ("getSolidAndHoles: " + verts.Length);
			if (verts != null && verts.Length > 0)
			{

				AXSpline s = new AXSpline();

				subsplines = new List<AXSpline>();
				for (int i = 0; i<vertCount; i++)
				{
					//Debug.Log (verts[i].x);
					if (verts[i].x == 888888)
					{
						//Debug.Log(" ========> new sub");
						subsplines.Add(s);
						s = new AXSpline();
						s.breakAngle = breakAngle;
						s.isClosed = isClosed;
					} 
					else
						s.Push(verts[i]);

					
				}
				subsplines.Add(s);

				
			}

			return subsplines; 

		}
		


		/*************************
		 * CLONE
		 *************************/
		public AXSpline clone() 
		{
			return clone (0, 0);
		}
		public AXSpline clone(float shft_x, float shft_y) 
		{
			AXSpline s = new AXSpline(vertCount);
			
			for (int i = 0; i<vertCount; i++) {
				s.Push(new Vector2((verts[i].x+shft_x), (verts[i].y+shft_y)));
			} 


			s.isClosed = isClosed;
			s.breakAngle = breakAngle;

			
			return s;	
		}
		public AXSpline clone(float shft_x, float shft_y, float rot_z) 
		{
			AXSpline s = clone();

			s.rotate(rot_z);
			s.shift (shft_x, shft_y);

			s.isClosed = isClosed;
			s.breakAngle = breakAngle;

			return s;	
		}




		
		
		/*************************
		 * IGNORE_GAPS
		 *************************/
		public AXSpline ignoreGaps() {
			AXSpline s = new AXSpline(vertCount);
			
			for (int i = 0; i<vertCount; i++) {
				if (verts[i].x == 999999 || verts[i].x == 888888)
					continue;
				s.Push(verts[i]);
			} 
			
			s.isClosed = isClosed;
			s.breakAngle = breakAngle;
			return s;	
		}
		
		

		public AXSpline mirrorXclone() {
			//Debug.Log("mirrorXclone :: " + vertCount);
			AXSpline s = new AXSpline();
			for (int i = 0; i<vertCount; i++) {
				// Push, but reversing x
				s.CheckToReallocBuiltinVector2();
				s.verts[i] = new Vector2(-verts[i].x, verts[i].y );
				s.vertCount++;
			}
			s = s.Reverse();
			s.isClosed = isClosed;
			s.breakAngle = breakAngle;
			return s;	
		}
		

		public AXSpline doubleInSymmetry(float separation)
		{

			float breaker = .01f;

			AXSpline s = clone ();


			s.shift(separation/2, 0);

			if(separation>0)
				s.Push (new Vector2(0, verts[vertCount-1].y+breaker ));

			for (int i=vertCount-1; i>=0; i--)
				s.Push (new Vector2(-verts[i].x-separation/2, verts[i].y));

			if(separation>0)
				s.Push (new Vector2(0, verts[0].y-breaker));

			s.breakAngle = breakAngle;
			s.isClosed = true;
			return s;
			
			
		}
		
		public AXSpline addBacking(float separation)
		{
			AXSpline s = clone ();
			s.shift(separation/2, 0);
			s.Push (new Vector2(0, verts[vertCount-1].y ));
			s.Push (new Vector2(0, verts[0].y));

			s.breakAngle = breakAngle;
			s.isClosed = true;
			return s;
			
			
		}
		
		
		public AXSpline mirrorAndAddBacking(float separation, float extra)
		{
			float breaker = .01f;

			AXSpline s = new AXSpline();
			
			s.Push (new Vector2(0, 			verts[vertCount-1].y+breaker ));
			s.Push (new Vector2(-separation/2-extra, 	verts[vertCount-1].y+breaker ));

			for(int i = (vertCount-1); i >= 0; i--) {
				s.Push(new Vector2(-verts[i].x-separation-extra, verts[i].y));
			}

			s.Push(new Vector2(-separation/2-extra, verts[0].y-breaker));
			s.Push(new Vector2(0, verts[0].y-breaker));


			s.breakAngle = breakAngle;
			s.isClosed = isClosed;

			return s;
			
			
		}
		

		/*************************
		 * Translate
		 *************************/
		public void shift(float x, float y) {
			
			for (int i = 0; i<vertCount; i++) {
				if (verts[i].x == 999999 || verts[i].x == 888888 )
					continue;
				verts[i].x += x;
				verts[i].y += y;
			}
		}
		/*************************
		 * Rotate
		 *************************/
		public void rotate(float degrees) {
			float tx;
			float ty;
			for (int i = 0; i<vertCount; i++) {
				if (verts[i].x == 999999 || verts[i].x == 888888)
					continue;

				float sin = Mathf.Sin(-degrees * Mathf.Deg2Rad);
				float cos = Mathf.Cos(-degrees * Mathf.Deg2Rad);

				tx = verts[i].x;
				ty = verts[i].y;

				verts[i].x = (cos * tx) - (sin * ty);
				verts[i].y = (sin * tx) + (cos * ty);
			}
		}


		/*************************
		 * Scale
		 *************************/
		public void scale(float s) {
			
			for (int i = 0; i<vertCount; i++) {
				if (verts[i].x == 999999 || verts[i].x == 888888)
					continue;
				verts[i].x *= s;
				verts[i].y *= s;
			}
		}

		/*************************
		 * turnRight
		 *************************/
		public AXSpline turnRight() {
			
			AXSpline s = new AXSpline();
			for (int i = 0; i<vertCount; i++) {
				if (verts[i].x == 999999 || verts[i].x == 888888)
					continue;
				s.Push(new Vector2(verts[i].y, -verts[i].x));
			}
			return s;
		}
		/*************************
		 * turnLeft
		 *************************/
		public AXSpline turnLeft() {
			
			AXSpline s = new AXSpline();
			for (var i = 0; i<vertCount; i++) {
				if (verts[i].x == 999999 || verts[i].x == 888888)
					continue;
				s.Push(new Vector2(-verts[i].y, verts[i].x));
				
			}
			return s;
		}
		
		/*************************
		 * turnUp
		 *************************/
		public Vector3[] turnUpAsVector3s() {
			
			Vector3[] v3s = new Vector3[vertCount];

			for (var i = 0; i<vertCount; i++) {
				if (verts[i].x == 999999 || verts[i].x == 888888)
					continue;
				v3s[i] = new Vector3(verts[i].x, verts[i].y, 0.0f);
				
			}
			return v3s;
		}
		

		
		/*************************
		 * TO_STRING
		 *************************/
		
		public string toString() {
			
			string ret = "Spline ("+vertCount+") [ ";
			for (int i=0; i<vertCount; i++) {
				if (i>0) ret += ",  ";
				ret += verts[i];	
			}
			ret += " ]";
			return ret;
		}
		
		
		
		/*************************
		 * GET_ANGLES_ARRAY
		 *************************/
		public float[] getAnglesArray()  {

			if (angles == null) {
				// class member
				angles  = new float[vertCount];
				
				// temporary variables
				Vector2 v1;
				Vector2 v2;
				float 	ang;

				sharpVertsInside_Count = 0;
				for (int i=0; i<vertCount; i++) {	

					v1 	= verts[i] - previousPoint(i);
					v2 	= nextPoint(i) - verts[i];
					ang = Vector3.Angle(v1, v2);

					if ((i > 0 && i < vertCount-1)  && ang > breakAngle) {
						// ACUTE ANGLE -- MAKE BREAK
						sharpVertsInside_Count++;
					}
					angles[i] = ang;		
				}


				getUValues();
			}

			
			return angles;
		}
		





		
		/*************************
		 * GET_UVALUES_ARRAY
		 *************************/
		public float[] getUValues() {

			//Debug.Log ("calc uValues");

			//if (vertCount > 0) {
			if ( (uValues == null || uValues.Length == 0 ) && vertCount > 0) {

				//Debug.Log(vertCount);
				// class member
				uValues = new float[vertCount];



				// temporary variables
				float segment_length = 0f;
				float curve_distance = 0f;

				segment_lengths = new float[vertCount];
				curve_distances = new float[vertCount];
				curve_distances[0] = 0;

				uValues[0] = 0;

				//int end = vertCount;

				for (int i=0; i<vertCount; i++) {
					segment_length 		= Vector3.Distance(previousPoint(i), verts[i]);
					segment_lengths[i] 	= segment_length;

					if (i > 0) 
						curve_distance 	   += segment_length;

					curve_distances[i] 	= curve_distance;

				}

				float length = curve_distances[vertCount-1];

				for (int i=1; i<vertCount; i++) {
					uValues[i] = curve_distances[i] / length;
				}
				

			}
			return uValues;
		}
		
		public float getLength() {


			length = 0;
			
			int start_i = 1;
			if (isClosed) {
				start_i = 0;
			}
			for (int i=start_i; i<vertCount; i++) {			
				length += Vector3.Distance(previousPoint(i), verts[i]);
			}	
			if (isClosed) {
				//length += Vector3.Distance(verts[vertCount-1], verts[0]);
			}
			return length;
		}
		
		public void calcStats() {
			if (cenX == 1999999) {
				minX = 1999999;
				for (int i=0; i<vertCount; i++) {
					if (verts[i].x == 999999 || verts[i].x == 888888)
						continue;

					if (verts[i].x < minX) minX = verts[i].x;
					if (verts[i].x > maxX) maxX = verts[i].x;
					if (verts[i].y < minY) minY = verts[i].y;
					if (verts[i].y > maxY) maxY = verts[i].y;
				}
				
				width  = Mathf.Abs(maxX-minX);	
				height = Mathf.Abs(maxY-minY);	
				
				cenX = minX + (maxX-minX)/2;
				cenY = minY + (maxY-minY)/2;
				
			}
			
		}
		
		public float getCenX() {
			calcStats();
			return cenX;
		}	
		public float getCenY() {
			calcStats();
			return cenY;
		}	
		
		public int getAllocateVertsCt() {
			
			getAnglesArray();


			Debug.Log ("vertCount = " + vertCount + ", sharpVertsInside_Count = " + sharpVertsInside_Count);
			
			int allocVertsCt = vertCount + sharpVertsInside_Count; // + 1; // add one for end vert

	/*
			// sharpVertsCt does not consider open or closed so it represents the max
			// remove some verts if open
			if (! isClosed )
			{
				if( angles[0] > breakAngle ) 
					allocVertsCt--;
				if(angles[vertCount-1] > breakAngle)
					allocVertsCt--;
			} 
			*/
			return  allocVertsCt;
		}
		

		public bool jointIsAcute(int i) {

			this.getAnglesArray();
			
			if (i == 0) {
				// FIRST NODE IS NEVER DOUBLED
				return false;	
			}

			if ((i == (vertCount-1)) && ! isClosed) {
				// LAST POINT IS DOUBLED ONLY IF CLOSED AND ACUTE WITH FIRST POINT 
				return false;
			}

			if (i == (vertCount)) {
				// LAST POINT IS DOUBLED ONLY IF CLOSED AND ACUTE WITH FIRST POINT 
				return false;
			}

			return (angles[i] > breakAngle);
		}
		
		public bool closeJointIsAcute() {

			this.getAnglesArray();
			return (angles != null && angles.Length > 0 && angles[0] > breakAngle);
		}
		
		
		
		/***********************************
		 From a given point, get its vertex index.
		 If the given point is not a polygon vertex, 
		 it will return -1 
		 ***********************************/
		public int vertexIndex(Vector2 vertex) {
			for (int i=0; i<vertCount; i++) {
				if (verts[i] == vertex) 
					return i;	
			}
			return -1;
		}
		
		
		/*************************
		 * AREA
		 *************************/
		public float area() {
			float dblArea = 0.0f;
			
			int j;
			for (int i=0; i<vertCount; i++) {
				j=(i+1) % vertCount;
				dblArea +=  verts[i].x * verts[j].y - (verts[i].y * verts[j].x);
			}
			
			dblArea=dblArea/2;
			return dblArea;
		}
		
		
		
		/***********************************
		 From a given vertex, get its previous vertex point.
		 If the given point is the first one, 
		 it will return  the last vertex;
		 ***********************************/
		public Vector2 previousPoint(int index) {		
			if (vertCount > 0)
				return verts[( ((index-1)<0) ? (vertCount-1) : index-1)];
			return new Vector2();
		}
		public Vector2 nextPoint(int index) {		
			if (vertCount > 0)
				return verts[(index+1) % vertCount];
			return new Vector2();
		}
		
		
		/*********************************************
	       To check whether a given point is a CPolygon Vertex
		**********************************************/
		public bool hasVertex(Vector2 pt) {
			int nIndex = vertexIndex(pt);
			if ((nIndex >= 0) && (nIndex < vertCount))
				return true;
			
			return false;
		}
		
		
		/*********************************************
	     REMOVE VERTEX
		**********************************************/
		public void removeVertex(Vector2 vert) {
			int index = vertexIndex(vert);
			if (index > -1) {			
				RemoveAt(index);
			}
		}	
		






		
		
		/***********************************************
			To check a vertex concave point or a convex point
			-----------------------------------------------------------
			The out polygon is in count clock-wise direction
		************************************************/
		public bool vertexIsConcave(Vector2 vertex) {
			return vertexIsConcave(vertexIndex(vertex));
			
		}
		public bool vertexIsConcave(int index) {
			
			if (index >= 0) {
				AXSpline tri = new AXSpline(previousPoint(index),verts[index], nextPoint(index));
				
				if (tri.area()>0) {
					return false;
				} else {
					return true;
				}
			}	
			return false;
		}
		
		
		
		
		
		
		
		/*******************************
		GET_CONVEX_VERSION
		
		*********************************/
		public AXSpline getConvexVersion() {
			AXSpline sp = this.clone();
			
			//bool checking = false;
			
			var i0 = 0;
			while (i0<sp.vertCount) {
				
				if (i0 >= sp.vertCount || sp.vertCount <= 3) break;
				
				//Debug.Log("["+i0+"]: sp.vertCount=" + sp.vertCount + " :: " + dAng);
				if ( ! sp.vertexIsConcave(i0)) {
					Debug.Log("....removing...." + sp.verts[i0]);
					sp.RemoveAt(i0);
					i0 = 0;
				} else {
					i0++;
				}
				
			}
			return sp;	
			
		}
		
		
		
		
		
		
		
		/*****************************************************
		 IS_COUNTER_CLOCKWISE
			based on:
			## Weiler-Atherton Clipping Applet
			## Author: Christopher Andrews
			## Date: May 2008
			http://people.cs.vt.edu/~cpa/clipping/
		******************************************************/
		public bool isCounterClockwise() {
			// take the dot product of the min point (thus a concave point) in any cardinal direction
			Vector2 A;
			Vector2 B;
			Vector2 C;
			if (vertCount == 3) {
				A = verts[0];
				B = verts[1];
				C = verts[2];
			} else {
				var indexOfMinX = 0;
				for (int i=1; i<vertCount; i++) 
					if (verts[i].x < verts[indexOfMinX].x) indexOfMinX = i;
				
				A = verts[((indexOfMinX-1) < 0) ? (vertCount-1) : (indexOfMinX-1)];
				B = verts[indexOfMinX];
				C = verts[((indexOfMinX+1) % vertCount)];
			}
			float z = ((A.x - B.x)*(C.y - B.y)) - ((A.y - B.y)*(C.x - B.x));
			
			return (z < 0);
		}





		
		
		/*****************************************************
		To reverse points to different direction (order) :
		******************************************************/
		public void reverseDirection() {
			Reverse();	
		}
		
		
		/*******************************
		IS_INSIDE
		
		*********************************/
		public bool isInside(Vector2 p) { 
			int j = vertCount-1; 
			bool inside = false; 
			for (int i = 0; i < vertCount; j = i++) { 
				if ( ((verts[i].y <= p.y && p.y < verts[j].y) || (verts[j].y <= p.y && p.y < verts[i].y)) && 
				    (p.x < (verts[j].x - verts[i].x) * (p.y - verts[i].y) / (verts[j].y - verts[i].y) + verts[i].x)) 
					inside = !inside; 
			} 
			return inside; 
		}	
		
		
		
		Vector2[] circle(float radius, int segs) {
			// A spline is an array of Vector2's
			Vector2[] retVerts = new Vector2[segs];
			
			// circle spline creation...
			for (int i = 0; i<segs; i++) {
				float rads = Mathf.Deg2Rad*(i*360/(float)segs);
				retVerts[i] = new Vector2(radius*Mathf.Cos(rads),radius*Mathf.Sin(rads));
			}
			return retVerts;
		}
		
		public AXSpline testshape() {
			// A spline is an array of Vector2's
			int i = 0;
			
			verts[i++] = new Vector2(  0,  0);
			verts[i++] = new Vector2( 20,  0);
			verts[i++] = new Vector2( 20, 10);
			verts[i++] = new Vector2( 30, 10);
			verts[i++] = new Vector2( 30,  0);
			verts[i++] = new Vector2( 40,  0);
			verts[i++] = new Vector2( 40, 30);
			verts[i++] = new Vector2(  0, 30);
			
			return this;
		}
		
		
		
		
		
		

		
		/* TRIM_TO_CONVEX
		 *
		 * This takes a spline shape and trims off what it needs to to make the 
		 * resulting spline be convex.  The resulting spline can be extruded and play well 
		 * in a physics simulation task where only conves meshes may work out well.
		 * 
		 * It returns the optimum shape based on that with the greatest area of the various attempts 
		 * made by slicing the shape at each concave vertex.
		 *
		 * CAVEAT:
		 * For now it supports a spline with only one concave squence. Later, adapt this to 
		 * shave from each concave sequence, in effect whittling 
		 * the shape down.
		 *
		 * FUTURE FUNCTIONALITY:
		 * Another way to do this would be to return a multiple splines 
		 * where all the original area is retained but the shape is split into multiple
		 * pieces that can later be extruded and amalgamated with physics joints.
		 *
		 * Alternatively (or additionally), this could also be used by the caller to make an invisible collider
		 * while the visible one uses the original curve.
		 *
		 */

		/*
		public AXSpline trimToConvex() {
			
			// add functionality for multiple concave sections of a single spline.
			
			bool thisVertIsConcave	= false;
			bool previousVertIsConcave	= false;
			
			bool traversingConcaveCurve	= true;
			
			AXSpline resSpline = null;
			AXSpline retSpline = null;
			
			float thisArea 		= 0;
			float greatestArea 	= 0;
			
			getAnglesArray();
			
			for (int i=0; i<vertCount; i++) {
				
				thisVertIsConcave = vertexIsConcave(i);
				
				if (thisVertIsConcave && angles[i] < 50 ) {
					//if (thisVertIsConcave) {
					traversingConcaveCurve 	= true;
					previousVertIsConcave 	= true;
					resSpline = getTrimmedSpline(i, false); 
				} else if (previousVertIsConcave) {
					traversingConcaveCurve 	= false;
					previousVertIsConcave 	= false;
					resSpline = getTrimmedSpline(i, true); 
				}
				
				// criteria for optimum
				if (resSpline != null) {
					if (retSpline == null) { 
						retSpline = resSpline;
						greatestArea = resSpline.area();
					} else {
						thisArea = resSpline.area();
						if (thisArea > greatestArea) {
							retSpline = resSpline;
							greatestArea = thisArea;
						}
					}
					resSpline = null;
				}
			}	
			if (retSpline == null) {
				retSpline = this;
			}
			return retSpline;
		}
		*/
		
		public AXSpline getTrimmedSpline(int trim_i, bool isLastOnConcaveCurve) {
			
			Vector2 A;
			Vector2 B;
			Vector2 C;
			Vector2 D;
			Vector2 d;
			Vector2 X;
			
			AXSegIntersectionResult results;
			
			float u;

			AXSpline retSpline = new AXSpline();
			
			
			bool onNewSpline 	= false;
			int cursor_i 		= trim_i+1;
			if (cursor_i == vertCount) {
				cursor_i = 0;
			}
			
			if (isLastOnConcaveCurve) {
				retSpline.Push(verts[trim_i]);
				retSpline.Push(verts[cursor_i]);
				cursor_i 	= cursor_i+1;
				if (cursor_i == vertCount) {
					cursor_i = 0;
				}
				onNewSpline = true;
			}
			
			
			// THIS SEGMENT
			A = previousPoint(trim_i);
			B = verts[trim_i];
			
			// AGAINST EACH OTHER SEGMENT
			
			while (trim_i != cursor_i) {			
				if (cursor_i > AXGeometry.Utilities.IntPointPrecision) {
					Debug.Log("Spline::getTrimmedSpline: GOVERNOR TERMINATES LOOP");
					break;
				}
				
				C 	= previousPoint(cursor_i);
				D 	= verts[cursor_i];
				d = D - C;
				
				results = lineIntersection(A, B, C, D);
				
				if (results.t != 999999 && results.t != 888888) {
					// we are concerned only with u (and not t) bcause it is the line outside of the 
					// intial segment that must have an intersection within the second (each other) segment.
					u = results.u;
					
					if (0<u && u<1) {
						// the line from the concave point intersects another segment in this spline.
						X = C + u*d;
						
						retSpline.Push(X);
						
						if (! onNewSpline) {
							// this intersection X signals the
							// START of a new curve
							onNewSpline = true;
							retSpline.Push(verts[cursor_i]);
						} else {
							// this intersection X signals one of the 
							// END conditions of the curve. The other is that it returns to the beginning.
							return retSpline;
						}
						
					} else {
						if (onNewSpline) {
							retSpline.Push(verts[cursor_i]);
						}
					}
					
				} else {
					if (onNewSpline) {
						retSpline.Push(verts[cursor_i]);
					}
				}
				
				cursor_i++;
				if (cursor_i == vertCount) {
					cursor_i = 0;
				}
			}
			
			// No end was encountered in the loop, so it must have ended back at the trim vertex passed as an argument.
			return retSpline;		
		}
		
		
		
		
		
		/*	CLIPING AND MERGING
		 *
		 *
		 *
		 */
		
		bool makePolysForMerging(AXSpline spline_a, AXSpline spline_b) {
			/*
			  USE the Weiler-Atherton Clipping Algorithm
			  
			*/

			bool overlaps = false;

			int SUBJ = 1;
			int CLIP = 2;
			
			// head of linked-list [1]
			poly1 = new AXPointList(SUBJ, spline_a);
			poly2 = new AXPointList(CLIP, spline_b);
			
			AXPointNode 	nA = poly1.head;
			while (	nA  != null) {
				
				AXPointNode nB = nA.next[SUBJ];
				
				Vector2 A = nA.v;
				Vector2 B;
				
				if (nB != null) 	{	B = nB.v; } 
				else 	{	B = poly1.head.v; }
				
				
				
				AXPointNode 	nC = poly2.head;
				while(	nC	!= null ) {
					AXPointNode nD = nC.next[CLIP];
					Vector2 C = nC.v;
					Vector2 D;
					if (nD != null)  	{	D = nD.v; 			} 
					else 	{	D = poly2.head.v;	} 		
					
					// TEST FOR INTERSECTION OF SEGS: AB and CD
					AXSegIntersectionResult results = segIntersection(A, B, C, D);
					
					if (results.t != 999999 && results.t != 888888) {
						float t 	 = results.t;
						float u	 	 = results.u;
						bool inbound = results.inbound;
						
						Vector2 b = B - A;
						Vector2 X = A + b*t;				
						
						
						// INSERT nX INTO BOTH POLYS
						/*
						    This is looking for the intersection point between the line segments
					        AB and CD. We are using the parametric equation A-bt = C - du, where
					        b is the vector (B-A) and d is the vector (D-C). We want to solve for
					        t and u - if they are both between 0 and 1, we have a valid
					        intersection point.
					
					        For insertion, we assume that AB comes from the subject polygon and
					        that CD comes from the clipping region. This means that the
					        intersection point can be inserted directly between CD but that we need
					        to use the t value to find a valid position between A and B.
						*/
						
						AXPointNode nX;
						
						AXPointNode prev;
						AXPointNode next;
						
						
						if (0 < u && u < 1) {
							// we have a valid transverse intersection point

							overlaps = true;

							// SUBJ... POLY ///////////////////////////////////
							if 			(t == 0) {
								nX = nA;
							} else if 	(t == 1) {
								nX = nB;
							} else 				 {
								nX = new AXPointNode(X);
								nX.inbound = inbound;
								
								
								prev = nA;
								next = nA.next[SUBJ];
								while (next != null && next.t[SUBJ] < t) {
									prev = next;
									next = next.next[SUBJ];	
								}
								prev.next[SUBJ] = nX;
								nX.next[SUBJ] = next;
							}
							nX.t[SUBJ] = t;
							// SUBJ... POLY ///////////////////////////////////
							
							
							// CLIP... POLY ///////////////////////////////////
							//Debug.Log("Inserting into CLIP...");
							nX.t[CLIP] = u;
							prev = nC;
							next = nC.next[CLIP];
							//	while (next && next.t[CLIP] < u) {
							//		prev = next;
							//		next = next.next[CLIP];
							//	}
							prev.next[CLIP] = nX;
							nX.next[CLIP] = next;
							// CLIP... POLY ///////////////////////////////////
							
							
							
						} 
						
						
						else if ( (u == 0) && (0 < t && t < 1) ) {
							/* RORY: Dec 3, 2010
							 * 
							 * Don't use u == 1 as well because it inserts the same point twice into the SUBJ polygon.
							 * THis is because it means a point from the clipper falls right on an edge og the SUBJ 
							 * causing two intersections to be found in successive edge analyses where really there is only one.
							 *
							 * for more advanced implementation of Weiler Clipping, use something like:
							 * http://angusj.com/delphi/clipper.php#code
							 * (though that c# implementation uses  System.Collections.Generic  
							 * which is not supported on iOS unity currently
							 */
							
							// we can ignore them if the point it an endpoint in both
							// polygons - any following we do will just wrap around

							overlaps = true;
							
							if (u == 0) {
								nX = nC;
							} else {
								nX = nD;
							}
							nX.t[SUBJ] = t;
							nX.t[CLIP] = u;
							nX.inbound = inbound;
							
							
							// insert into AB
							prev = nA;
							next = nA.next[SUBJ];
							
							while(next != null && next.t[SUBJ]<t) {
								prev = next;
								next = next.next[SUBJ];
							}
							
							prev.next[SUBJ] = nX;
							nX.next[SUBJ] = next;
							
							
						}
						
						
						
					}
					nC = nD; // next in poly2.
				}
				nA = nB; // next in poly1.
			}

			//Debug.Log ("makePolysForMerging: poly1" + poly1.print ());
			//Debug.Log ("makePolysForMerging: poly2" + poly2.print ());

			return overlaps;
			
		}

		public AXSpline union(AXSpline s) {

			AXSpline retSpline = null;

			bool overlaps = false;

			try {
			
				overlaps = makePolysForMerging(this, s);
			} catch {

			}

			//Debug.Log ("OVERLAPS? " + overlaps + ", isInside(s.verts[0])=" + isInside(s.verts[0]));
			if (! overlaps )
			{
				if(! isInside(s.verts[0]))
				{
				// s is totally outside the other spline separated with 9999999
					retSpline = clone();
					return retSpline.attach(s);
				}
				else
				{
					// s is completely inside, just return this
					return this;
				}
			}

			// NOW we have two polys with extra points added for interssections.
			// make one splice from the 2 polys...
			
			

			// Find first inbound
			AXPointNode entering = poly1.getNextUnvisitedInbound();
			
			if (entering != null) {
				retSpline 	= new AXSpline();
				int currentPoly 	= 1;
				AXPointNode current = entering;
				
				while (current != null && ! current.visited) {
					retSpline.Push(current.v);
					current.visited = true;
					if (current.isIntersection(currentPoly) && current != entering) {
						currentPoly = ( currentPoly == 1 ? 2 : 1);
					}
					current = current.next[currentPoly];

					if (current == null) { // there is no next
						if (currentPoly == 1) {
							current = poly1.head;
						} else {
							current = poly2.head;
						}
					}

					
				}
				
				// THE TWO WERE OVERLAPPING AND SUCCESSFLLY UNIONED
				retSpline.isProductOfBooleanOperation = true;


				return retSpline;
			}
			
			// THE TWO DID NOT OVERLAP....
			// entering was false so no intersection. 
			// Either shape is completely inside or completely outside.
			
			if (isInside(s.verts[0]) ) {
				return this;
			} else if ( s.isInside(verts[0]) ) {
				return s;
			} else {
				// return the two together
				return this;
				
				// later:: return this.attach(s);
			}


		}
		
		
		
		
		
		
		public AXSpline intersection(AXSpline s) {
			
			makePolysForMerging(this, s);
			
			// NOW we have two polys with extra ponts added for interssections.
			// make one splice from the 2 polys...
			
			// Find first inbound
			AXPointNode entering = poly1.getNextUnvisitedInbound();
			
			int intersectionCount = 0;

			if (entering != null) {
				AXSpline retSpline 	= new AXSpline();
				int currentPoly 	= 2;
				AXPointNode current = entering;
				
				while (current != null && ! current.visited) {

					retSpline.Push(current.v);
					current.visited = true;
					if (current.isIntersection(currentPoly) && current != entering) {
						currentPoly = ( currentPoly == 1 ? 2 : 1);
						intersectionCount++;
					}
					current = current.next[currentPoly];
					
					if (current == null) { // there is no next
						if (currentPoly == 1) {
							current = poly1.head;
						} else {
							current = poly2.head;
						}
					}
					
				}
				if (! retSpline.isCounterClockwise()) {
					retSpline.Reverse();
				}
				
				// THE TWO WERE OVERLAPPING AND SUCCESSFLLY UNIONED
				if (intersectionCount > 0 ) { // this applies only for intersection request
					retSpline.isProductOfBooleanOperation = true;
					return retSpline;
				}
			}
			
			// THE TWO DID NOT OVERLAP....
			// entering was false so no intersection. 
			// Either shape is completely inside or completely outside.
			
			if (isInside(s.verts[0]) ) {
				return s;
			} else if ( s.isInside(verts[0]) ) {
				return this;
			} else {
				// return the two together
				//return this;
				
				// later:: return this.attach(s);
			}
			return null;
		}
		
		
		
		
		public AXSpline subtract(AXSpline s) {

			/*
			 * 1. Get all solids as list, ignore voids
			 * 2. For each solid, subtract each solid in s
			 * 3. If a solid is erased by a void, remove it from the list
			 * 3. If a multi solids are created, remove orig solid and add subsolids
			 * 
			 * 
			 */


			List<AXSpline> solids = this.getSubsplines();
			List<AXSpline>  voids =    s.getSubsplines();

			// for each solid go against each void.
			// after each void, use the new list of solids
			List<AXSpline> tmpRes = null;
			foreach (AXSpline _void in voids)
			{
				List<AXSpline> newSolids = new List<AXSpline>();

				foreach (AXSpline solid in solids)
				{
					tmpRes = doSubtract(solid, _void);
					if (tmpRes != null)
						newSolids.AddRange(tmpRes);
				}
				solids = newSolids;
			}

			// now serialize as verts... separated by 999999
			AXSpline result = new AXSpline(solids);

			return result;
		}

		public List<AXSpline> doSubtract(AXSpline spline_a, AXSpline spline_b)
		{
			// assumes that a and b are monolithic splines
			// In other words, there is no 9999999 in them

			// Can return one or more subsplines if the subtractor cleaves the solod in multiple pieces.
			// This return, then, is an unserialized set of splines (would this be optimized as verts[]?

			// if results list Count is:
			// 1. [0]: the solid was completely inside the void
			// 2. [1]: the void was completely outside the solid
			// 3. [>1]: the void cut the solid into multiple pieces

			List<AXSpline> results = new List<AXSpline>();

			
			AXSpline cutter = spline_b.clone ();

			if (cutter.isCounterClockwise()) {
				cutter = cutter.Reverse();
			}
			
			bool overlaps = makePolysForMerging(spline_a, cutter);
			// NOW we have two polys with extra ponts added for interssections.
			// make one splice from the 2 polys...
			

			if (! overlaps) 
			{
				// THE TWO DID NOT OVERLAP....
				// Either shape is completely inside or completely outside.
				if (! spline_a.isInside(cutter.verts[0]))
					results.Add (spline_a);
				else
				{
					// hole
					//Debug.Log(" ============== Attaching hole "+spline_b.toString());
					spline_a = spline_a.attachHole(spline_b);
					//Debug.Log(spline_a.toString ());
					results.Add (spline_a);
				}
			} 
			else
			{
				AXPointNode leaving = null;

				// The merged polys are state machines
				// using the "visited" variable in each node of the lined list
				while ( (leaving = poly1.getNextUnvisitedInbound()) != null)
					results.Add (getNextSubtractionSpline(leaving));
			}

			return results;
		}


		public AXSpline getNextSubtractionSpline(AXPointNode leaving)
		{

			// Find first inbound

			bool begin = true;
			
			bool dotrim = false;
			
			if (leaving != null) {
				
				//Vector3 lastVert = new Vector3(leaving.v.x,leaving.v.y);
				
				AXSpline retSpline 	= new AXSpline();
				int currentPoly 		= 2;
				AXPointNode current 	= leaving;
				
				while (current != null && ! current.visited) {
					
					if (dotrim)
					{
						if(  !begin && (currentPoly == 1 || current.isIntersection(currentPoly))  )
							retSpline.Push(current.v);
					}
					else 
						retSpline.Push(current.v);
					
					begin = false;
					
					current.visited = true;
					if (current.isIntersection(currentPoly) && current != leaving) {
						
						if(currentPoly == 1)
						{
							//retSpline.Push(new Vector2(-999999,-999999));
						}
						currentPoly = ( currentPoly == 1 ? 2 : 1);
					}
					current = current.next[currentPoly];
					
					
					/* 2014_01_02 This did not make it through the translation to c#
					 * Not sure when it comes into play, but the test fails even when there is a current poly.
					 * 
					 * 2014_08_22 Fixed by reversing condition from != to ==
					 * */
					
					if (current == null) { // there is a next
						//Debug.Log ("no next");
						if (currentPoly == 1) 
							current = poly1.head;
						else 
							current = poly2.head;

					}
				}
				//retSpline.Push(lastVert);

				if (! retSpline.isCounterClockwise())
					retSpline.reverseDirection();

				// THE TWO WERE OVERLAPPING AND SUCCESSFLLY UNIONED
				retSpline.isProductOfBooleanOperation = true;
				
				if (! retSpline.isCounterClockwise())
					retSpline.reverseDirection();
		
				// how many point_nodes were unvisitied?
				
				
				return retSpline;
			}

			return null;


		}
		
		
		
		
		
		
		public AXSegIntersectionResult segIntersection(Vector2 A, Vector2 B, Vector2 C, Vector2 D) {
			/*
				This calculates the intersection point between two lines, AB and CD.
				The function is somewhat specialized for the purposes of the clipping. 
				
				The return value is None if the lines are parallel or a
				tuple of the form (t, u, inbound). inbound is a boolean value if
				we know that CD is a line that belongs to a polygon specified in CCW
				direction. The further optimization is that u and inbound are set to
				None if t is not within 0 and 1. All of the uses of this function
				in this application are only interested in line segments and this saves
				a couple of extra calculations.
			*/
			
			//Debug.Log("segIntersection: A=" + A + ", B=" + B + ", C=" + C + ", D=" + D);
			Vector2 b		 	= B-A;
			Vector2 d		 	= D-C;
			Vector2 d_perp	 	= new Vector2(-d.y, d.x);
			float denom 		= Vector2.Dot(d_perp, b);
			
			if (denom == 0) { return new AXSegIntersectionResult(); }// parallel: no intersection possible
			Vector2 c		= C-A;
			float numer	 	= Vector2.Dot(d_perp, c);
			float t		 	= numer / denom;
			
			
			if (0 <= t  && t <= 1) {
				//Debug.Log("t = " + t);
				
				Vector2 b_perp = new Vector2(-b.y, b.x);
				
				numer = Vector2.Dot(b_perp, c);
				//numer = b_perp.x*c.x + b_perp.y*c.y;
				
				float u = numer / denom;
				
				if (0<= u && u <=1) {
					bool inbound = (denom < 0);
					//Debug.Log(" ===>  u = " + u + ", inbound = " + inbound);
					return new AXSegIntersectionResult(t, u, inbound);
				}
			}
			// no intersection found
			return new AXSegIntersectionResult();
			
		}  
		AXSegIntersectionResult lineIntersection(Vector2 A, Vector2 B, Vector2 C, Vector2 D) {
			// similar to segIntersection, but it returns a value regardless of whether the segments intersect within their bounds. The calling 
			// function can decide what to do with the results.
			Vector2 b		 	= B-A;
			Vector2 d		 	= D-C;
			Vector2 d_perp	 	= new Vector2(-d.y, d.x);
			float 	denom	 	= Vector2.Dot(d_perp, b);
			
			if (denom == 0) { return new AXSegIntersectionResult(); }// parallel: no intersection possible
			Vector2 c		 	= C-A;
			float 	numer	 	= Vector2.Dot(d_perp, c);
			float 	t		 	= numer / denom;
			
			Vector2 b_perp = new Vector2(-b.y, b.x);
			
			numer = Vector2.Dot(b_perp, c);
			//numer = b_perp.x*c.x + b_perp.y*c.y;
			
			float u = numer / denom;
			
			bool inbound = (denom < 0);
			//Debug.Log(" ===>  u = " + u + ", inbound = " + inbound);
			return new AXSegIntersectionResult(t, u, inbound);
			
		}
		
		AXXIntersectionResult xIntersection(float y, float xa, float xb) {
			
			float x = -1;

			float x1;
			float x2;
			float y1;
			float y2;
			
			int ni;
			
			for(int i = 0; i<vertCount; i++) {
				
				if (i == vertCount-1) {
					ni = 0;
				} else {
					ni = i+1;
				}
				
				y1 = verts[i].y;
				y2 = verts[ni].y;
				
				
				if ( (y1 <= y && y <= y2) || (y2 <= y && y <= y1) ){
					//Debug.Log("Y="+y+" is between: "+verts[i] + " & " + verts[ni]);
					// we need to get x!
					// y = mx+b;
					x1 = verts[i].x;
					x2 = verts[ni].x;
					
					//float m = (y2-y1) / (x2-x1);
					float tmpx = (y-y1)*(x2-x1)/(y2-y1) + x1;
					
					//Debug.Log("tmpx="+tmpx);
					if (xa <= tmpx  && tmpx <= xb) {
						x = tmpx;
						
						Vector2 retP = (y2 > y1) ? verts[ni] : verts[i];
						
						
						return new AXXIntersectionResult(x, retP);	
					} 
					
					
				}	
			}
			
			return null;
			
		}







		// MANPAGE: http://www.archimatix.com/uncategorized/axspline-getinsetcornerspines
		public AXSpline[] getInsetCornerSplines(float inset)
		{
			//There can't be more subsplines then there are vertices...
			// Each of these subslines will have at least 3 points


			AXSpline[] returnSplines; 


			// PLANSWEEPS AT CORNERS
			// First, go around and group verts that are closer
			// to  each other than min_sengmentLength
			float min_sengmentLength = 2*inset;



			// FIND FIRST CURSOR VERTEX

			int cursor = 0; // essentially 0

			if (segment_lengths[cursor] < min_sengmentLength )
			{
				cursor = vertCount-1;

				// back up to first long segment you find
				while (segment_lengths[cursor] < min_sengmentLength )
				{
					if (cursor == 0)
					{
						// if we mad it all the way back to 0, then all the segments were too small.
						// just return this AXSpline
						returnSplines = new AXSpline[1];
						returnSplines[0] = this;
						return returnSplines;
					}
					cursor--;
				}

			}

			
			// OK: Now we have our starting point: cursor. 
			// Proceed forward from here with the grouping.

			// Use a single array of ints with -88888 as seperators and -99999 as terminator.
			// Cant have more the 2*verCount entries (a seperator between each vert)

			int[] groupedIndicies = new int[2*vertCount*100];

			int index = 0;
			groupedIndicies[index++] = cursor++;


			int countOfSplines = 1;

			// GROUP VERTS THAT DEFINE THE SUBSPLINES
			while ( (cursor % vertCount) != groupedIndicies[0])
			{
				if (segment_lengths[cursor % vertCount] > min_sengmentLength)
				{
					countOfSplines++;
					groupedIndicies[index++] = -88888; // add break code
				}
				
				groupedIndicies[index++] = cursor % vertCount;

				// starting from cursor, add vertices to subspline

				cursor++;
			}

			// done... add terminator
			groupedIndicies[index++] = -99999;

			   

			// Take each group and add a beginning and ending vertex inset by margin.
			returnSplines = new AXSpline[countOfSplines];
			int splineCursor = 0;


			AXSpline spline = null;

			for(int j=0; j<groupedIndicies.Length; j++)
			{
				if (j==0 || groupedIndicies[j] == -88888 || groupedIndicies[j] == -99999)
				{
					// End a spline
					if (groupedIndicies[j] == -88888 || groupedIndicies[j] == -99999)
					{ 
						// Add end vert
						Vector2 endVert = Vector2.Lerp ( verts[groupedIndicies[j-1]], nextPoint(groupedIndicies[j-1]), .25f);
			
						spline.Push(endVert);
						returnSplines[splineCursor++] = spline;

						if (groupedIndicies[j] == -99999)
							break;
					}

					// Begin a spline
					if 	(j==0 || groupedIndicies[j] == -88888)
					{
						// skip over -88888
						if 	(groupedIndicies[j] == -88888) 
							j++;
						// start new AXSpline...
						spline = new AXSpline();

						Vector2 begVert = Vector2.Lerp (previousPoint(groupedIndicies[j]), verts[groupedIndicies[j]], .75f);
						spline.Push(begVert);
						spline.Push(verts[groupedIndicies[j]]);
					}
				}
				else
				{
					spline.Push(verts[groupedIndicies[j]]);
				}
			}

			/*
			Debug.Log("===========================");
			for(int j=0; j<groupedIndicies.Length; j++)
			{
					Debug.Log(groupedIndicies[j]);
				if (groupedIndicies[j] == -99999)
					break;
			}

			foreach(AXSpline s in returnSplines)
			{
				Debug.Log("----");
				Debug.Log(s.toString());
			}
			*/

			return returnSplines;

		}





	}











	/* SUPPORTING CLASSES
	 * 
	 */


	class AXXIntersectionResult {
		public float 	x;
		public Vector2 retP;

		public AXXIntersectionResult(float xx, Vector2 retPP) {
			x = xx;
			retP = retPP;
		}
		
	}


	// CLASS SEGINTERSECTION RESULT

	public class AXSegIntersectionResult {
		public float t = 999999;
		public float u;
		public bool  inbound;	

		public AXSegIntersectionResult() {
		
		}
		public AXSegIntersectionResult(float tt, float uu, bool ib) {
			t = tt;
			u = uu;
			inbound = ib;	
		}
	}



	// CLASS:  POINT_LIST ****************************
	[System.Serializable]
	public class AXPointList {
		
		// A LINKED LIST OF POINT NODES
		
		public int id;

		[System.NonSerialized]
		public AXPointNode head;
		
		public AXPointList(int _id, AXSpline s) {
			
			id   = _id;
			head = new AXPointNode(s.verts[0]);
			
			AXPointNode prevPoint = head;
			
			for (int i = 1; i<s.vertCount; i++) {
				
				AXPointNode pn = new AXPointNode(s.verts[i]);
				
				prevPoint.setNext(id, pn);
				
				prevPoint = pn;
			}
			
		}


		public AXPointNode getNextUnvisitedInbound() {
			AXPointNode cursor = head;
			
			//while (cursor != null && ! cursor.inbound && ! cursor.visited) {
			while (cursor != null) {
				if (cursor.isIntersection(id) && cursor.inbound && ! cursor.visited)
					return cursor;

				cursor = cursor.next[id];
			}
			return null;	
		}


		public AXPointNode getFirstOutbound() {
			AXPointNode cursor = head;
			
			//while (cursor && ! cursor.inbound && ! cursor.visited) {
			while (cursor != null && ! cursor.visited   && (! cursor.isAnIntersection || cursor.inbound) ) {
				cursor = cursor.next[id];
			}
			return cursor;	
		}
		
		public string print() {
			AXPointNode cursor = head;
			string ret = "";
			
			while(cursor != null) {
				ret += " [" +cursor.v.ToString();
				if (cursor.isIntersection(id)) {
					ret += " - intersec::" + cursor.inbound;
				}
				ret += "] (visited="+cursor.visited+") ...  ";
				cursor = cursor.next[id];	
			}
			return ret + "\n\r";
			
		}
	}






	// CLASS:  POINT_NODE ****************************
	public class AXPointNode {
		public Vector2 	v;

		[System.NonSerialized]
		public AXPointNode[] next = null;

		public float[] 	t;
		public bool 		inbound;
		
		public bool 		isAnIntersection = false; // if true and inbound-false, then this is an exiting
		
		public bool 		visited	;
		
		public AXPointNode(Vector2 _v) {
			v = _v;
			
			next 	= new AXPointNode[3];
			t 		= new float[3];
			t[1] 	= 1;
			t[2] 	= 1;
		}
		
		public void setNext(int polyID, AXPointNode pn) {
			next[polyID] = pn;
		}
		
		public bool isIntersection(int _id) {
			
			if (_id > 0) {
				return t[_id] < 1;
			} else {
				if (next[1] != null && next[2] != null) 
					return true;
			}
			
			
			return false;	
		}

		public string toString() {
			return v.ToString();
		}
		
		
	}	


}





