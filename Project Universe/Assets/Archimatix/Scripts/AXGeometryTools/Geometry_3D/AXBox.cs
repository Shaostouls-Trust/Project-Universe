using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using AXGeometry;


namespace AX
{


	public class AXPlaneGenerator {
		
		public float side_x;
		public float side_y;
		public float side_z;
		
		
		//bool useMeshCollider  = false;
		
		
		
		public bool top 		= true;
		public bool bottom 		= false;
		

		// GENERATE
		public void generate (ref Mesh mesh, AXTexCoords tex)
		{

			if (tex == null)
				 	return;
				 
			// Combine the plan and sections to make the fabric
			float uShift = tex.shift.x;
			float vShift = tex.shift.y;
			float uScale = tex.scale.x;
			float vScale = tex.scale.y;

			float sx = side_x/2;
			//float sy = side_y;
			float sz = side_z/2;
			
			int faces = (top?1:0) + (bottom?1:0);
			
			int totalVertCount 		= (top?4:0) + (bottom?4:0);
			int totalTriangCount 	= faces * 2;
			
			Vector3[] p = new Vector3[4]; 
			
			p[0] = new Vector3(-sx, 0, -sz);
			p[1] = new Vector3( sx, 0, -sz);
			p[2] = new Vector3( sx, 0,  sz);
			p[3] = new Vector3(-sx, 0,  sz);
			
					
			
			
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
				uv[vc++] = new Vector2(      0,   side_y);
				uv[vc++] = new Vector2( side_x,	  side_y);
				uv[vc++] = new Vector2( side_x,   side_y+side_z);
				uv[vc++] = new Vector2(   	 0,   side_y+side_z);
			}
			if (bottom)
			{
				vt[vc++] = p[3];
				vt[vc++] = p[2];
				vt[vc++] = p[1];
				vt[vc++] = p[0];
				vc -= 4;
				uv[vc++] = new Vector2(      0,   -side_z);
				uv[vc++] = new Vector2( side_x,	  -side_z);
				uv[vc++] = new Vector2( side_x,   0);
				uv[vc++] = new Vector2(   	  0,  0);
			}
			
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







	/* BOX_GENERATOR
	 *
	 *	An AXBox is different than a Unity CubePrimitive
	 * it:
	 * 		1. Scales the uvs with size
	 * 		2. can turn any face off and on
	 * 		3. Set breakangle
	 *
	 *	return a reference to the new object
	 *
	 */
	public class AXBoxGenerator {
		
		public float side_x;
		public float side_y;
		public float side_z;

		public float breakAngle = 60;	
		
		//bool useMeshCollider  = false;
		
		
		public bool front 		= true;
		public bool back 		= true;
		
		public bool left 		= true;
		public bool right 		= true;
		
		public bool top 		= true;
		public bool bottom 		= true;
		
		// Texture controls
		public float uScale = 20f;
		public float vScale = 20f;
		public float uShift =  0f;
		public float vShift =  0f;


        public int[] mappedFaces;

		public AXBoxGenerator() {

		}
		public AXBoxGenerator(float sx, float sy, float sz) {
			side_x = sx;
			side_y = sy;
			side_z = sz;
		}

		// GENERATE
		public void generate (ref Mesh mesh, AXTexCoords tex)
		{

			if (tex == null)
				 	return;
				 
			// Combine the plan and sections to make the fabric
			float uShift = tex.shift.x;
			float vShift = tex.shift.y;
			float uScale = tex.scale.x;
			float vScale = tex.scale.y;


			//bool smooth = (breakAngle > 90) ? true : false;

			float sx = side_x/2;
			float sy = side_y;
			float sz = side_z/2;

			int faces = (front?1:0) + (back?1:0) + (left?1:0) + (right?1:0) + (top?1:0) + (bottom?1:0);

			int totalVertCount 		= faces * 4;
			int totalTriangCount 	= faces * 2;

			Vector3[] p = new Vector3[8]; 

			p[0] = new Vector3(-sx, 0, -sz);
			p[1] = new Vector3( sx, 0, -sz);
			p[2] = new Vector3( sx, 0,  sz);
			p[3] = new Vector3(-sx, 0,  sz);

			p[4] = new Vector3(-sx,  sy, -sz);
			p[5] = new Vector3( sx,  sy, -sz);
			p[6] = new Vector3( sx,  sy,  sz);
			p[7] = new Vector3(-sx,  sy,  sz);
			

			 
			// Y-AXIS JITTER
			/*
			if (false)
			{
				for (int i=4; i<8; i++)
					p[i].y *= Mathf.PerlinNoise(p[i].x+100, p[i].z+100);
			}
			*/



			Vector3[] vt 	= new Vector3[totalVertCount];
			Vector2[] uv = new Vector2[totalVertCount];


			int vc = 0;

			if (front)
			{
				vt[vc++] = p[0];
				vt[vc++] = p[1];
				vt[vc++] = p[5];
				vt[vc++] = p[4];

				vc -= 4;
				uv[vc++] = new Vector2(		0,		0);
				uv[vc++] = new Vector2(side_x,		0);
				uv[vc++] = new Vector2(side_x, side_y);
				uv[vc++] = new Vector2(     0, side_y);
  			}
			if (right)
			{
				vt[vc++] = p[1];
				vt[vc++] = p[2];
				vt[vc++] = p[6];
				vt[vc++] = p[5];

				vc -= 4;
				uv[vc++] = new Vector2(side_x,		0);
				uv[vc++] = new Vector2(side_x+side_z,		0);
				uv[vc++] = new Vector2(side_x+side_z, side_y);
				uv[vc++] = new Vector2(     side_x, side_y);
			}
			if (back)
			{
				vt[vc++] = p[2];
				vt[vc++] = p[3];
				vt[vc++] = p[7];
				vt[vc++] = p[6];
				vc -= 4;
				uv[vc++] = new Vector2(side_x+side_z,		0);
				uv[vc++] = new Vector2(side_x*2+side_z,		0);
				uv[vc++] = new Vector2(side_x*2+side_z, side_y);
				uv[vc++] = new Vector2(side_x+side_z,   side_y);
			}
			if (left)
			{
				vt[vc++] = p[3];
				vt[vc++] = p[0];
				vt[vc++] = p[4];
				vt[vc++] = p[7];
				vc -= 4;
				uv[vc++] = new Vector2(-side_z,		   0);
				uv[vc++] = new Vector2(  0,		   0);
				uv[vc++] = new Vector2( 0,   side_y);
				uv[vc++] = new Vector2(-side_z,   side_y);
			}
			if (top)
			{
				vt[vc++] = p[4];
				vt[vc++] = p[5];
				vt[vc++] = p[6];
				vt[vc++] = p[7];
				vc -= 4;
				uv[vc++] = new Vector2(      0,   side_y);
				uv[vc++] = new Vector2( side_x,	  side_y);
				uv[vc++] = new Vector2( side_x,   side_y+side_z);
				uv[vc++] = new Vector2(   	 0,   side_y+side_z);
			}
			if (bottom)
			{
				vt[vc++] = p[3];
				vt[vc++] = p[2];
				vt[vc++] = p[1];
				vt[vc++] = p[0];
				vc -= 4;
				uv[vc++] = new Vector2(      0,   -side_z);
				uv[vc++] = new Vector2( side_x,	  -side_z);
				uv[vc++] = new Vector2( side_x,   0);
				uv[vc++] = new Vector2(   	  0,  0);
			}

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







        public static Mesh GenerateMesh(Vector3 p0, Vector3 size, AXBlockSides sides)
        {
            Mesh mesh = new Mesh();
            //AXBoxGenerator boxer = new AXBoxGenerator(sx, sy, sz);
            //boxer.generate(ref mesh, tex);






            float u1 = 0;
            float u2 = 1f;
            float v1 = 0;
            float v2 = 1f;


            Vector3 p2 = p0 + size;

          
                 
            // Combine the plan and sections to make the fabric
            //float uShift = tex.shift.x;
            //float vShift = tex.shift.y;
            //float uScale = tex.scale.x;
            //float vScale = tex.scale.y;


            //bool smooth = (breakAngle > 90) ? true : false;
           


            int faces = (sides.Front?1:0) + (sides.Back?1:0) + (sides.Left?1:0) + (sides.Right?1:0) + (sides.Top?1:0) + (sides.Bottom?1:0);

            int totalVertCount      = faces * 4;
            int totalTriangCount    = faces * 2;

            Vector3[] p = new Vector3[8]; 

            p[0] = new Vector3(p0.x, p0.y, p0.z);
            p[1] = new Vector3(p2.x, p0.y, p0.z);
            p[2] = new Vector3(p2.x, p0.y, p2.z);
            p[3] = new Vector3(p0.x, p0.y, p2.z);

            p[4] = new Vector3(p0.x, p2.y, p0.z);
            p[5] = new Vector3(p2.x, p2.y, p0.z);
            p[6] = new Vector3(p2.x, p2.y, p2.z);
            p[7] = new Vector3(p0.x, p2.y, p2.z);
            

             
            // Y-AXIS JITTER
            /*
            if (false)
            {
                for (int i=4; i<8; i++)
                    p[i].y *= Mathf.PerlinNoise(p[i].x+100, p[i].z+100);
            }
            */



            Vector3[] vt    = new Vector3[totalVertCount];
            Vector2[] uv = new Vector2[totalVertCount];


            int vc = 0;

            if (sides.Front)
            {
                vt[vc++] = p[0];
                vt[vc++] = p[1];
                vt[vc++] = p[5];
                vt[vc++] = p[4];

                vc -= 4;
                uv[vc++] = new Vector2(u1, v1);
                uv[vc++] = new Vector2(u2, v1);
                uv[vc++] = new Vector2(u2, v2);
                uv[vc++] = new Vector2(u1, v2);
            }

            if (sides.Back)
            {
                vt[vc++] = p[2];
                vt[vc++] = p[3];
                vt[vc++] = p[7];
                vt[vc++] = p[6];
                vc -= 4;
                uv[vc++] = new Vector2(u1, v1);
                uv[vc++] = new Vector2(u2, v1);
                uv[vc++] = new Vector2(u2, v2);
                uv[vc++] = new Vector2(u1, v2);
            }
            if (sides.Left)
            {
                vt[vc++] = p[3];
                vt[vc++] = p[0];
                vt[vc++] = p[4];
                vt[vc++] = p[7];
                vc -= 4;
                uv[vc++] = new Vector2(u1, v1);
                uv[vc++] = new Vector2(u2, v1);
                uv[vc++] = new Vector2(u2, v2);
                uv[vc++] = new Vector2(u1, v2);
            }
            if (sides.Right)
            {
                vt[vc++] = p[1];
                vt[vc++] = p[2];
                vt[vc++] = p[6];
                vt[vc++] = p[5];

                vc -= 4;
                uv[vc++] = new Vector2(u1, v1);
                uv[vc++] = new Vector2(u2, v1);
                uv[vc++] = new Vector2(u2, v2);
                uv[vc++] = new Vector2(u1, v2);
            }

            if (sides.Top)
            {
                vt[vc++] = p[4];
                vt[vc++] = p[5];
                vt[vc++] = p[6];
                vt[vc++] = p[7];
                vc -= 4;
                uv[vc++] = new Vector2(u1, v1);
                uv[vc++] = new Vector2(u2, v1);
                uv[vc++] = new Vector2(u2, v2);
                uv[vc++] = new Vector2(u1, v2);
            }
            if (sides.Bottom)
            {
                vt[vc++] = p[3];
                vt[vc++] = p[2];
                vt[vc++] = p[1];
                vt[vc++] = p[0];
                vc -= 4;
                uv[vc++] = new Vector2(u1, v1);
                uv[vc++] = new Vector2(u2, v1);
                uv[vc++] = new Vector2(u2, v2);
                uv[vc++] = new Vector2(u1, v2);
            }

            //for (int i=0; i<uv.Length; i++)
                //uv[i] = new Vector2(uv[i].x/uScale+uShift, uv[i].y/vScale+vShift);

            int[] t         = new int[totalTriangCount*3];

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
            mesh.RecalculateBounds();






            return mesh;
        }













        public static Mesh GenerateCarton(Vector3 p0, Vector3 size, AXBlockSides sides)
        {
            Mesh mesh = new Mesh();
            //AXBoxGenerator boxer = new AXBoxGenerator(sx, sy, sz);
            //boxer.generate(ref mesh, tex);



            // UV DEFAULTS REFLECT FRONT PROJECTION ONLY

            // front
            float u0 = 0f; // always 0
            float u1 = 1;

            // right side
            float u2 = 1;
            float u3 = 1;

            // back
            float u4 = 0f;
            float u5 = 1;

            // left side
            float u6 = 0f;
            float u7 = 0f;


            // bottom
            float v0 = 0f;
            float v1 = 0f;

            // front
            float v2 = 0f;
            float v3 = 1;

            // top
            float v4 = 1;
            float v5 = 1;


            Vector3 p2 = p0 + size;



            // Combine the plan and sections to make the fabric
            //float uShift = tex.shift.x;
            //float vShift = tex.shift.y;
            //float uScale = tex.scale.x;
            //float vScale = tex.scale.y;


            //bool smooth = (breakAngle > 90) ? true : false;



            int faces = (sides.Front ? 1 : 0) + (sides.Back ? 1 : 0) + (sides.Left ? 1 : 0) + (sides.Right ? 1 : 0) + (sides.Top ? 1 : 0) + (sides.Bottom ? 1 : 0);

            int totalVertCount = faces * 4;
            int totalTriangCount = faces * 2;

            Vector3[] p = new Vector3[8];

            p[0] = new Vector3(p0.x, p0.y, p0.z);
            p[1] = new Vector3(p2.x, p0.y, p0.z);
            p[2] = new Vector3(p2.x, p0.y, p2.z);
            p[3] = new Vector3(p0.x, p0.y, p2.z);

            p[4] = new Vector3(p0.x, p2.y, p0.z);
            p[5] = new Vector3(p2.x, p2.y, p0.z);
            p[6] = new Vector3(p2.x, p2.y, p2.z);
            p[7] = new Vector3(p0.x, p2.y, p2.z);



            // Y-AXIS JITTER
            /*
            if (false)
            {
                for (int i=4; i<8; i++)
                    p[i].y *= Mathf.PerlinNoise(p[i].x+100, p[i].z+100);
            }
            */



            Vector3[] vt = new Vector3[totalVertCount];
            Vector2[] uv = new Vector2[totalVertCount];


            int vc = 0;

            if (sides.Front)
            {
                vt[vc++] = p[0];
                vt[vc++] = p[1];
                vt[vc++] = p[5];
                vt[vc++] = p[4];

                vc -= 4;
                uv[vc++] = new Vector2(u0, v2);
                uv[vc++] = new Vector2(u1, v2);
                uv[vc++] = new Vector2(u1, v3);
                uv[vc++] = new Vector2(u0, v3);
            }

            if (sides.Right)
            {
                vt[vc++] = p[1];
                vt[vc++] = p[2];
                vt[vc++] = p[6];
                vt[vc++] = p[5];

                vc -= 4;
                uv[vc++] = new Vector2(u2, v2);
                uv[vc++] = new Vector2(u3, v2);
                uv[vc++] = new Vector2(u3, v3);
                uv[vc++] = new Vector2(u2, v3);
            }

            if (sides.Back)
            {
                vt[vc++] = p[2];
                vt[vc++] = p[3];
                vt[vc++] = p[7];
                vt[vc++] = p[6];
                vc -= 4;
                uv[vc++] = new Vector2(u4, v2);
                uv[vc++] = new Vector2(u5, v2);
                uv[vc++] = new Vector2(u5, v3);
                uv[vc++] = new Vector2(u4, v3);
            }
            
            
            if (sides.Left)
            {
                vt[vc++] = p[3];
                vt[vc++] = p[0];
                vt[vc++] = p[4];
                vt[vc++] = p[7];
                vc -= 4;
                uv[vc++] = new Vector2(u6, v2);
                uv[vc++] = new Vector2(u7, v2);
                uv[vc++] = new Vector2(u7, v3);
                uv[vc++] = new Vector2(u6, v3);
            }

            if (sides.Top)
            {
                vt[vc++] = p[4];
                vt[vc++] = p[5];
                vt[vc++] = p[6];
                vt[vc++] = p[7];
                vc -= 4;
                uv[vc++] = new Vector2(u0, v4);
                uv[vc++] = new Vector2(u1, v4);
                uv[vc++] = new Vector2(u1, v5);
                uv[vc++] = new Vector2(u0, v5);
            }
            if (sides.Bottom)
            {
                vt[vc++] = p[3];
                vt[vc++] = p[2];
                vt[vc++] = p[1];
                vt[vc++] = p[0];
                vc -= 4;
                uv[vc++] = new Vector2(u0, v0);
                uv[vc++] = new Vector2(u1, v0);
                uv[vc++] = new Vector2(u1, v1);
                uv[vc++] = new Vector2(u0, v1);
            }

            //for (int i=0; i<uv.Length; i++)
            //uv[i] = new Vector2(uv[i].x/uScale+uShift, uv[i].y/vScale+vShift);

            int[] t = new int[totalTriangCount * 3];

            int tc = 0;

            for (int f = 0; f < faces; f++)
            {
                int b = f * 4;
                t[tc++] = b + 0;
                t[tc++] = b + 2;
                t[tc++] = b + 1;

                t[tc++] = b + 0;
                t[tc++] = b + 3;
                t[tc++] = b + 2;
            }



            mesh.vertices = vt;
            mesh.triangles = t;
            mesh.uv = uv;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();






            return mesh;
        }


    }

}
