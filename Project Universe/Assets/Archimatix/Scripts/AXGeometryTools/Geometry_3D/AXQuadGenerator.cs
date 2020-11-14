using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using AXGeometry;


namespace AX
{



	public class AXQuadGenerator {
		
		public float width;
		public float height;

		
		//bool useMeshCollider  = false;
		
		

		public bool top 		= true;
		public bool bottom 		= false;


		// GENERATE
		public void generate (ref Mesh mesh, float w = 5, float h = 3, AXTexCoords tex = null)
		{
			
			if (tex == null)
			{
			 	tex = new AXTexCoords();
			 	tex.scale = new Vector2(100,100); // pixels/unit
				tex.shift = Vector2.zero;

			}

				 
			// Combine the plan and sections to make the fabric
			float uShift = tex.shift.x;
			float vShift = tex.shift.y;
			float uScale = tex.scale.x;
			float vScale = tex.scale.y;

			float sx = width/2;
			float sy = height/2;


			float u0 = uShift/uScale;
			float u1 = (uShift + width)/uScale;


			float v0 = vShift/vScale;
			float v1 = (vShift + height)/vScale;


			int faces = (top?1:0) + (bottom?1:0);
			
			int totalVertCount 		= (top?4:0) + (bottom?4:0);
			int totalTriangCount 	= faces * 2;
			
			Vector3[] p = new Vector3[4]; 
			
			p[0] = new Vector3(-sx, -sy, 0);
			p[1] = new Vector3( sx, -sy, 0);
			p[2] = new Vector3( sx,  sy, 0);
			p[3] = new Vector3(-sx,  sy, 0);
			
					
			
			
			Vector3[] vt = new Vector3[totalVertCount];
			Vector2[] uv = new Vector2[totalVertCount];
			
			
			int vc = 0;
			
			if (top)
			{
				vt[vc++] = p[0];
				vt[vc++] = p[1];
				vt[vc++] = p[2];
				vt[vc++] = p[3];
				vc -= 4;
				uv[vc++] = new Vector2( u0,   v0);
				uv[vc++] = new Vector2( u1,	  v0);
				uv[vc++] = new Vector2( u1,   v1);
				uv[vc++] = new Vector2( u0,   v1);
			}
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
			for (int i=0; i<uv.Length; i++)
				uv[i] = new Vector2(uv[i].x/uScale+uShift, uv[i].y/vScale+vShift);
			
			int[] t 		= new int[totalTriangCount*3];
			
			int tc = 0;
			
			for (int f=0; f<faces; f++)
			{
				int b = f*4;
				t[tc++] = b+0;
				t[tc++] = b+2;
				t[tc++] = b+1;
				
				t[tc++] = b+0;
				t[tc++] = b+3;
				t[tc++] = b+2;
			}
			
			

			mesh.vertices = vt;
			mesh.triangles = t;
			mesh.uv = uv;
			mesh.RecalculateNormals();
			

		}
		
	}


}
