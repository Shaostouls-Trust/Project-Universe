using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using AXGeometry;


namespace AX
{


	/* FRUSTRUM_GENERATOR
	 *
	 *	An AXFrustrum is different than a Unity CubePrimitive
	 * it:
	 * 		1. Scales the uvs with size
	 * 		2. can turn any face off and on
	 * 		3. Set breakangle
	 *
	 *	return a reference to the new object
	 *
	 */
	public class AXFrustrumGenerator {
		
		public float 	h;
		public float 	r;
		public float 	R;

		public float 	bevel;


		public int 		segs = 8;
		public float 	breakAngle = 60;	
		
		//bool useMeshCollider  = false;
		
			
		// Texture controls
		public float uScale = 20f;
		public float vScale = 20f;
		public float uShift =  0f;
		public float vShift =  0f;


		// GENERATE
		public void generate (ref Mesh mesh,  AXTexCoords tex)
		{

			if (tex == null)
				 	return;
				 
			// Combine the plan and sections to make the fabric
//			float uShift = tex.shift.x;
//			float vShift = tex.shift.y;
//			float uScale = tex.scale.x;
//			float vScale = tex.scale.y;

			int secVertCount = (bevel > 0) ? 4 : 2;

			Vector3[] secVerts = new Vector3[secVertCount];



			if (bevel <= 0)
			{
				secVerts[0] =  new Vector3(R, 0, 0);
				secVerts[1] =  new Vector3(r, 0, 0);
			}
			else
			{
				float tanb = h / (R-r);
				float    a = (bevel*tanb) / (1+tanb);
				float 	 c = bevel - a;


				Debug.Log(R + ", " + bevel + " :: " +( R-bevel));
				Debug.Log("tanb="+tanb+", a="+a+", c="+c);

				secVerts[0] =  new Vector3((R-bevel), 0, 0);
				secVerts[1] =  new Vector3((R-c), a, 0);
				secVerts[2] =  new Vector3((r+c), (h-a), 0);
				secVerts[3] =  new Vector3((r-bevel), h, 0);

			}

			//bool smooth = (breakAngle > 90) ? true : false;



			float angle = 360.0f/segs;


			// Make Mesh
			int vertCount = secVerts.Length * 2*segs;

			Vector3[] vertices = new Vector3[vertCount];
			Vector2[] uvs = new Vector2[vertCount];

			int faceCount = (secVerts.Length-1) * segs;
			int[] 		triangles = new int[6*faceCount];



			Matrix4x4 localM = Matrix4x4.identity;

			// CREATE VERTICES
			for (int seg = 0; seg<segs; seg++)
			{	
				Quaternion rot = Quaternion.Euler(0, angle*seg, 0);

				localM = Matrix4x4.TRS(Vector3.zero, rot, Vector3.one);

				for (int j=0; j<secVerts.Length; j++)
				{
					vertices[(j+seg*j)] = localM.MultiplyPoint3x4(secVerts[j]);
				}

			}

			int tri = 0;
			// CREATE TRIANGLES
			for (int seg = 0; seg<segs-1; seg++)
			{	

				for (int j=0; j<secVerts.Length-1; j++)
				{
					vertices[(j+seg*j)] = localM.MultiplyPoint3x4(secVerts[j]);

					triangles[tri++] = (j+seg*j);
					triangles[tri++] = (j+(seg+1)*j);
					triangles[tri++] = ((j+1)+(seg+1)*j);


					triangles[tri++] = (j+seg*j);
					triangles[tri++] = ((j+1)+(seg+1)*j);
					triangles[tri++] = ((j+1)+(seg+1)*j);

				}

			}

			mesh.vertices = vertices;
			mesh.uv = uvs;
			mesh.triangles = triangles;





//			int faces = (front?1:0) + (back?1:0) + (left?1:0) + (right?1:0) + (top?1:0) + (bottom?1:0);

//			int totalVertCount 		= faces * 4;
//			int totalTriangCount 	= faces * 2;
//
//			Vector3[] p = new Vector3[8]; 
//
//			p[0] = new Vector3(-sx, 0, -sz);
//			p[1] = new Vector3( sx, 0, -sz);
//			p[2] = new Vector3( sx, 0,  sz);
//			p[3] = new Vector3(-sx, 0,  sz);
//
//			p[4] = new Vector3(-sx,  sy, -sz);
//			p[5] = new Vector3( sx,  sy, -sz);
//			p[6] = new Vector3( sx,  sy,  sz);
//			p[7] = new Vector3(-sx,  sy,  sz);
//			
//
//			 
//			// Y-AXIS JITTER
//			/*
//			if (false)
//			{
//				for (int i=4; i<8; i++)
//					p[i].y *= Mathf.PerlinNoise(p[i].x+100, p[i].z+100);
//			}
//			*/
//
//
//
//			Vector3[] vt 	= new Vector3[totalVertCount];
//			Vector2[] uv = new Vector2[totalVertCount];
//
//
//			int vc = 0;
//
//			if (front)
//			{
//				vt[vc++] = p[0];
//				vt[vc++] = p[1];
//				vt[vc++] = p[5];
//				vt[vc++] = p[4];
//
//				vc -= 4;
//				uv[vc++] = new Vector2(		0,		0);
//				uv[vc++] = new Vector2(side_x,		0);
//				uv[vc++] = new Vector2(side_x, side_y);
//				uv[vc++] = new Vector2(     0, side_y);
//
//
//			}
//			if (right)
//			{
//				vt[vc++] = p[1];
//				vt[vc++] = p[2];
//				vt[vc++] = p[6];
//				vt[vc++] = p[5];
//
//				vc -= 4;
//				uv[vc++] = new Vector2(side_x,		0);
//				uv[vc++] = new Vector2(side_x+side_z,		0);
//				uv[vc++] = new Vector2(side_x+side_z, side_y);
//				uv[vc++] = new Vector2(     side_x, side_y);
//			}
//			if (back)
//			{
//				vt[vc++] = p[2];
//				vt[vc++] = p[3];
//				vt[vc++] = p[7];
//				vt[vc++] = p[6];
//				vc -= 4;
//				uv[vc++] = new Vector2(side_x+side_z,		0);
//				uv[vc++] = new Vector2(side_x*2+side_z,		0);
//				uv[vc++] = new Vector2(side_x*2+side_z, side_y);
//				uv[vc++] = new Vector2(side_x+side_z,   side_y);
//			}
//			if (left)
//			{
//				vt[vc++] = p[3];
//				vt[vc++] = p[0];
//				vt[vc++] = p[4];
//				vt[vc++] = p[7];
//				vc -= 4;
//				uv[vc++] = new Vector2(-side_z,		   0);
//				uv[vc++] = new Vector2(  0,		   0);
//				uv[vc++] = new Vector2( 0,   side_y);
//				uv[vc++] = new Vector2(-side_z,   side_y);
//			}
//			if (top)
//			{
//				vt[vc++] = p[4];
//				vt[vc++] = p[5];
//				vt[vc++] = p[6];
//				vt[vc++] = p[7];
//				vc -= 4;
//				uv[vc++] = new Vector2(      0,   side_y);
//				uv[vc++] = new Vector2( side_x,	  side_y);
//				uv[vc++] = new Vector2( side_x,   side_y+side_z);
//				uv[vc++] = new Vector2(   	 0,   side_y+side_z);
//			}
//			if (bottom)
//			{
//				vt[vc++] = p[3];
//				vt[vc++] = p[2];
//				vt[vc++] = p[1];
//				vt[vc++] = p[0];
//				vc -= 4;
//				uv[vc++] = new Vector2(      0,   -side_z);
//				uv[vc++] = new Vector2( side_x,	  -side_z);
//				uv[vc++] = new Vector2( side_x,   0);
//				uv[vc++] = new Vector2(   	  0,  0);
//			}
//
//			for (int i=0; i<uv.Length; i++)
//				uv[i] = new Vector2(uv[i].x/uScale+uShift, uv[i].y/vScale+vShift);
//
//			int[] t 		= new int[totalTriangCount*3];
//
//			int tc = 0;
//
//			for (int f=0; f<faces; f++)
//			{
//				int b = f*4;
//				t[tc++] = b+0;
//				t[tc++] = b+2;
//				t[tc++] = b+1;
//
//				t[tc++] = b+0;
//				t[tc++] = b+3;
//				t[tc++] = b+2;
//			}
//			
//
//
//			mesh.vertices = vt;
//			mesh.triangles = t;
//			mesh.uv = uv;
//			mesh.RecalculateNormals();
//
//

			

		}

	}

}
