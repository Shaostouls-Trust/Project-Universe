using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using AXClipperLib;
using Path = System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;

using AXPoly2Tri;

using AXGeometry;

/* EXTRUDE_GENERATOR
 *
 *	Generate extruded game objects that have procedural mesh's attached.
 *	These meshes will often take one or more Splines or Turtles as input.
 *
 *	return a reference to the new object
 *
 */
namespace AX
{
	
	public class AXWallExtrude {
		
		float height;
		
		bool useElev = false;
		private Plane  _elev;
		
		//bool useBase = false;
		//private Plane  _base;
		
		public float getHeight(float x, float z) {
			//Debug.Log ("here 2");
			if (useElev)
			{
				
				// find intersection with elevation plane
				Ray ray = new Ray (new Vector3(x,0,z), Vector3.up);
				
				float 	rayDistance = 0;
				Vector3 hitPoint;
				
				// If the ray makes contact with the ground plane then
				// position the marker at the distance along the ray where it
				// crosses the plane.
				if (_elev.Raycast(ray, out rayDistance)) 
				{
					hitPoint = ray.GetPoint(rayDistance);
					return hitPoint.y;
				}
			}
			return height;
		}
		
		public void setElev(Vector3 a, Vector3 b, Vector3 c)
		{
			useElev = true;
			_elev = new Plane(a, b, c);
		} 
		public void setElev(Plane p)
		{
			useElev = true;
			_elev = p;
		} 


		/*
		public void setBase(Vector3 a, Vector3 b, Vector3 c)
		{
			useBase = true;
			_base = new Plane(a, b, c);
		} 
		public void setBase(Plane p)
		{
			useElev = true;
			_base = p;
		} 
		*/
	
		public  Mesh generate(Spline planSpline, float hgt, AXTexCoords tex, LineType lineType) 
		{
			height = hgt;
			return generate(planSpline, tex, lineType);
		}
	
		public  Mesh generate(Spline planSpline, AXTexCoords tex, LineType lineType) {
							
			int planCount 	= (planSpline.isClosed ? planSpline.controlVertices.Count : planSpline.controlVertices.Count-1);
			int planlen 	= planSpline.derivedVertices.Count;
			
			//Debug.Log ("planlen="+planlen);
			//Debug.Log ("plan.isClosed="+planSpline.isClosed);
			
	
			List<Vector3> vertices = new List<Vector3>();
			List<Vector2>       uv = new List<Vector2>();
	
			List<int> triangles = new List<int>();
	
			List<Vector3> quadNormals = new List<Vector3>();
			List<Vector3> quadTangentsU = new List<Vector3>();
			List<Vector3> quadTangentsV = new List<Vector3>();
			
			//int prev_i;
			int next_i;
	
			float len;
			float u  = tex.shift.x/tex.scale.x;
			float v1 = tex.shift.y/tex.scale.y;
			float v2 = (tex.shift.y + height)/tex.scale.y;
	
			//bool isAcute;
	
			int LL = 0;
			int UL = 1;
			int LR = 2;
			int UR = 3;
	
			List<Vector2> verts = planSpline.controlVertices;
	
			int edgeVertCt = 0;
			
			for(int i=0; i<planCount; i++)
			{
			
				// add quad
				//prev_i = (i == 0) 					? 	(verts.Count-1) 	: 	i-1;
				next_i = (i == (verts.Count-1)) 	?  					0 	: 	i+1;
	
				// THIS VERT (only if 
				if (i== 0 || planSpline.isSharp(i))
				{
					// LOWER
					vertices.Add(new Vector3(verts[i].x, 0, verts[i].y ));
					
					if (tex.rotateSidesTex)
						uv.Add(new Vector2(v1, u));
					else
						uv.Add(new Vector2(u, v1));
					
					// upper
					vertices.Add(new Vector3(verts[i].x, getHeight( verts[i].x, verts[i].y), verts[i].y ));
					
					if (tex.rotateSidesTex)
						uv.Add(new Vector2(v2, u));
					else
						uv.Add(new Vector2(u, v2));
					//uv.Add(new Vector2(u, v2));
					
					if (i>0)
					{
						LL += 2;
						UL += 2;
						LR += 2;
						UR += 2;
					}
					edgeVertCt++;
	
				}
	
				// NEXT VERT
				len =  Vector3.Distance(verts[i], verts[next_i]);
				u += len/tex.scale.x;
	
				vertices.Add(new Vector3(verts[next_i].x, 0, verts[next_i].y));
				if (tex.rotateSidesTex)
					uv.Add(new Vector2(v1, u));
				else
					uv.Add(new Vector2(u, v1));
				
				vertices.Add(new Vector3(verts[next_i].x, getHeight( verts[next_i].x, verts[next_i].y), verts[next_i].y));
				if (tex.rotateSidesTex)
					uv.Add(new Vector2(v2, u));
				else
					uv.Add(new Vector2(u, v2));
				
				if (i>0)
				{
					LL += 2;
					UL += 2;
					LR += 2;
					UR += 2;
				}
				edgeVertCt++;
	
				triangles.Add (LL);
				triangles.Add (UL);
				triangles.Add (LR);
				
				triangles.Add (LR);
				triangles.Add (UL);
				triangles.Add (UR);
			
				// QUAD NORMAL: use quad to calculate its normal (lefthand rule in unity)
				Vector3 Tv = vertices[UL]-vertices[LL];
				Vector3 Tu = vertices[LR]-vertices[LL];
				
				quadNormals.Add (Vector3.Cross(Tv, Tu).normalized);
				quadTangentsV.Add (Tv);
				quadTangentsU.Add (Tu);
			}
	
	
	
			
			Mesh mesh = new Mesh();
			
			
			mesh.vertices 	= vertices.ToArray();
			mesh.uv 		= uv.ToArray();
	
	
	
			mesh.triangles 	= triangles.ToArray();
			
			mesh.RecalculateNormals();
			
			
			
			
			// NORMALS
			
			Vector3[] normals = new Vector3[mesh.vertices.Length];//mesh.normals;
			
			// PLAN
			int p_cur = 0;
			for (int i=0; i<planSpline.controlVertices.Count; i++)
			{
				int left_i 	= (i+planSpline.controlVertices.Count-1) % (planSpline.controlVertices.Count);
				int right_i = i;
				
				//Debug.Log(left_i + " ["+i+"] p_cur="+p_cur); 
				
				Vector3 normalLeft 		= Vector2.zero;
				Vector3 normalRight 	= Vector2.zero;
				Vector3 normalN			= Vector2.zero;
				
				if (i>0 || planSpline.isClosed)
					normalLeft		= quadNormals[left_i];
				
				if (i<planCount-1 || planSpline.isClosed)
					normalRight		= quadNormals[right_i];
				
				if ( (i>0 && i<planCount-1) || planSpline.isClosed)
					normalN			= (normalLeft  + normalRight).normalized;
				
								
				// VERTEX ADDRESSES (if not geom sharp, the left addresses will not be used...)
				int thisRight 	= p_cur*2;
				int thisRightU 	= thisRight + 1;
				
				int thisLeft  	= (i==0) ? (planlen*2-2)  : thisRight-2;
				int thisLeftU  	= thisLeft + 1;
				
				//Debug.Log ("["+i+"] isSharp="+ planSpline.isSharp(i)+" ... p_cur="+p_cur+ "::=>   " + thisLeft + ", " + thisLeftU + " <> "  + thisRight + ", " + thisRightU);
				
				
				
				// ASSIGN NORMALS
				
				if (i < planCount)
				{
					if ( ! planSpline.isBlend(i) || (i==0 && ! planSpline.isClosed) )
					{
						normals[thisRight] 	= normalRight; 
						normals[thisRightU] = normalRight; 
					}
					else 
					{
						normals[thisRight] 	= normalN;
						normals[thisRightU] = normalN;
					}
				}
				else if (planSpline.isClosed)
				{
					normals[thisRight] 	= normalRight; 
					normals[thisRightU] = normalRight; 
				}
				
				
				
				
				
				
				
				if (i > 0)
				{
					if (! planSpline.isBlend(i) || (i==(planSpline.controlVertices.Count-1) &&  ! planSpline.isClosed))
					{
						normals[thisLeft]  = normalLeft; 
						normals[thisLeftU] = normalLeft; 
					}
					else 
					{
						normals[thisLeft]  = normalN; 
						normals[thisLeftU] = normalN; 
					}
				}
				else if (planSpline.isClosed)
				{
					normals[thisLeft]  = normalN; 
					normals[thisLeftU] = normalN; 
					
				}
				
				
				p_cur++;
				next_i = (i<planSpline.controlVertices.Count-1) ? i+1 : 0;
				//Debug.Log("tris "+i+ ": "+ planSpline.isSharp(i));
				if (planSpline.isSharp(next_i))
					p_cur++;
				
				
				
			}
			
			mesh.normals = normals;
			
	
	
			return mesh;
			
			
		}
		
	
		
		
		
	}
}