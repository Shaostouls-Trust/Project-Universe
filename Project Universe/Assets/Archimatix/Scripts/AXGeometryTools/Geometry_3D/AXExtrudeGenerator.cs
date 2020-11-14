using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using AXPoly2Tri;

/* EXTRUDE_GENERATOR
 *
 *	Generate extruded game objects that have procedural mesh's attached.
 *	These meshes will often take one or more Splines or Turtles as input.
 *
 *	return a reference to the new object
 *
 */
 
using AXGeometry;


 namespace AX
 {
	 
	public class AXExtrudeGenerator {
	
		float 	height;
		
		//bool useElev = false;
		private Plane  _elev;
		
		//bool useBase = false;
		//private Plane  _base;
		
		//int 	segs  = 1;
		
	
		// Texture controls
	
		public float uScale = 20f;
		public float vScale = 20f;
		public float uShift =  0f;
		public float vShift =  0f;
	
	
		public Axis axis = Axis.Y;
	
	
		public bool rotSidesTex = true;
	
	
	
//		public float getHeight(float x, float z, int seg) {
////			if (useElev)
////			{
////				// find intersection with elevation plane
////				Ray ray = new Ray (new Vector3(x,0,z), Vector3.up);
////				
////				//Debug.Log ("here");
////				float 	rayDistance = 0;
////				Vector3 hitPoint;
////				
////				// If the ray makes contact with the ground plane then
////				// position the marker at the distance along the ray where it
////				// crosses the plane.
////				if (_elev.Raycast(ray, out rayDistance)) 
////				{
////					hitPoint = ray.GetPoint(rayDistance);
////					return hitPoint.y*seg/segs;
////				}
////			}
//			
//			return height*seg/segs;
//		}
//	
		public float getHeight(float x, float z, float height) {
//			if (useElev)
//			{
//				// find intersection with elevation plane
//				Ray ray = new Ray (new Vector3(x,0,z), Vector3.up);
//				
//				float 	rayDistance = 0;
//				Vector3 hitPoint;
//				
//				// If the ray makes contact with the ground plane then
//				// position the marker at the distance along the ray where it
//				// crosses the plane.
//				if (_elev.Raycast(ray, out rayDistance)) 
//				{
//					hitPoint = ray.GetPoint(rayDistance);
//					return hitPoint.y;
//				}
//			}
			return height;
		}

		/*
		public void setElev(Vector3 a, Vector3 b, Vector3 c)
		{
			_elev = new Plane(a, b, c);
		} 
		public void setBase(Vector3 a, Vector3 b, Vector3 c)
		{
			_base = new Plane(a, b, c);
		} 
		*/
		
		public static Mesh generate(AXSpline spline, float _height) {
	
			// If a GameObject is passed, just replace its mesh.
	
			Debug.Log ("--- Extrude: " + spline.toString());
	
			float height = _height;
			int segs = 1;
			bool rotSidesTex = false;
			Axis axis = Axis.Y;
			AXTexCoords tex = new AXTexCoords ();



			float[] angles 			= spline.getAnglesArray();
	
			int edgeLoopVertCt 		= spline.getAllocateVertsCt();

			Debug.Log ("edgeLoopVertCt 1 = " + edgeLoopVertCt);
			Debug.Log (spline.isClosed + " " + spline.closeJointIsAcute());
			if (spline.isClosed &&  spline.closeJointIsAcute()) // add a closing ribs
				edgeLoopVertCt += 2;
	
			Debug.Log ("edgeLoopVertCt 2 = " + edgeLoopVertCt);



			int totalVertCount 		= 0;
			int totalTriangCount 	= 0;
	
	
	
			int vcount = spline.vertCount;
			if (! spline.isClosed)
				vcount--;
	
			totalVertCount 		+= ((segs+1) * edgeLoopVertCt);
			totalTriangCount 	+= 3 * ( segs* (2 * vcount) );
	
			Debug.Log ("totalVertCount="+totalVertCount);
	
			spline.getAnglesArray();
			spline.getUValues();
	
			Vector3[] 	vertices 	= new Vector3[totalVertCount];
			Vector2[] 	uv 		 	= new Vector2[totalVertCount];
			int[] 		triangles	= new int[totalTriangCount*2];
			
			int index 				= 0;
			int vertCount 			= 0;
			int segThis 			= 0;
		
			// Create the Geometry
			// Run around the outline
			float x = 0;
			float y = 0;
			float z = 0;
			
			float u = 0;
			float v = 0;
			
			int i;
			float segHgt;
	
			int reps = spline.vertCount-1;
			if (spline.isClosed)
				reps++;
	
			//Debug.Log ("reps="+reps);
	
			// SIDE VERTICES
	
			for (int seg = 0; seg<=segs; seg++) {
	
				// each vert on spline
				for (i=0; i<=reps; i++) {
	
					segHgt = height*seg/segs; //getHeight(spline.verts[i].x, spline.verts[i].y, seg);
	
					if (i == spline.vertCount)
					{
						x =  spline.verts[0].x;
						y =  segHgt;
						z =  spline.verts[0].y;
					}
					else 
					{
						x =  spline.verts[i].x;
						y =  segHgt;
						z =  spline.verts[i].y;
					}
	
	
	
					if (rotSidesTex &&  axis != Axis.Y)
					{ 
						//Debug.Log ("use height of extrude for u, heigth of vert for v");
	
						u = height * (seg/segs) ;
						v = z;
					}
					else
					{ 
						// Y-Axis, use length along spline 
						if (i == 0) 
							u = 0;
						else if (i == spline.vertCount)
							u = spline.getLength() ;
						else
							u = spline.curve_distances[i] ;
						v = height * (seg/segs) ;
					}
	
	
	
					u /= tex.scale.x; //uScale;
					v /= tex.scale.y;
	
					u += tex.shift.x; /// uShift;
					v += tex.shift.y; /// vShift;
	
					vertices[vertCount] 	= new Vector3(x,y,z); // bottom
					uv[vertCount] 			= new Vector2 (u, v);
	
	
					vertCount++;
						 	 
					if (spline.jointIsAcute(i)) {
	
	
						vertices[vertCount] = new Vector3(x,y,z);
						uv[vertCount] 		= new Vector2(u,v);
	
						vertCount++;
					} 				
				} // end edge loop
	
	
				// if first vertex is acute, close loop by addeding vert0
	
				/*
				if (angles[0] > spline.breakAngle) {
					u = 1;
		
					vertices[vertCount] = vert0;
						  uv[vertCount] = new Vector2(u,v);
							 vertCount++;
				}
				*/
	
	
				// Added bottom, now jump to next horizontal....
				if (seg == 0) 
					continue;
	
				// skin between this edgeLoop and the previous
				segThis = seg;
	
				// TRIANGLES /////////////////////////////////////////////////////////////////////////////////////////
				// now make triangles
	
				var segPrev = segThis-1;
				
				
				int this_L;
				int this_U;
				int next_L = -1;
				int next_U = -1;
				
				var edgeloop_cursor = 0;
				
				for(i = 0; i<reps; i++) {
					if (angles[i] > spline.breakAngle && i>0) edgeloop_cursor++;
	
					this_L	= segPrev*edgeLoopVertCt +   edgeloop_cursor;
					next_L	= segPrev*edgeLoopVertCt +  ((edgeloop_cursor + 1) % edgeLoopVertCt);
					
					this_U 	= segThis*edgeLoopVertCt +   edgeloop_cursor;
					next_U 	= segThis*edgeLoopVertCt + ((edgeloop_cursor + 1) % edgeLoopVertCt);
	
					triangles[index++] = next_L;
					triangles[index++] = this_L;
					triangles[index++] = this_U;
					
					triangles[index++] = next_U;
					triangles[index++] = next_L;
					triangles[index++] = this_U;
	
					edgeloop_cursor++;
				}
			}
			 // end segs
		
	
	
			Mesh mesh = new Mesh();
	
	
			mesh.vertices 	= vertices;
			mesh.uv 		= uv;
			mesh.triangles 	= triangles;

			Debug.Log("NORMALS RECALCULATED");
			mesh.RecalculateNormals();
	
			/*
			// adjust normals for first and last vert
			if (! spline.closeJointIsAcute())
			{
				Vector3[] norms = mesh.normals;
				Quaternion rotation;
	
				// first rib
				rotation = Quaternion.Euler(0, spline.angles[0], 0);
				norms[0] 				= rotation * norms[0];
				norms[edgeLoopVertCt] 	= rotation * norms[edgeLoopVertCt];
	
				// last rib
				rotation = Quaternion.Euler(0, -spline.angles[0], 0);
				norms[spline.vertCount-1] = rotation * norms[spline.vertCount-1];
				norms[spline.vertCount-1+edgeLoopVertCt] = rotation * norms[spline.vertCount-1+edgeLoopVertCt];
	
				mesh.normals = norms;
			}
			*/
	
	
			return mesh;
	
		
		}
	
	
	
	
	
	}
}