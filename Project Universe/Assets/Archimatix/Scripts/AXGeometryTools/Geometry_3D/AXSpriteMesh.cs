using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using AXGeometry;


namespace AX
{



	public class AXSpriteMesh {

		public Mesh mesh;

		public float width;
		public float height;

		public float pixelsPerUnit;

		public float offsetX;
		public float offsetY;



		// Init
		public void init (float w = 5, float h = 3, float scale = 100, float offX = 0, float offY = 0)
		{

			mesh 				= new Mesh();

			width 				= w;
			height 				= h;
			pixelsPerUnit 		= scale;
			offsetX 			= offX;
			offsetY 			= offY;

			setVertexValues();
			setTrianglesIndices();
			setUVValues();

			mesh.RecalculateNormals();
			mesh.RecalculateTangents();


		}


		public void resize(float w, float h)
		{
			width = w;
			height = h;

			setVertexValues();
		}

		public void setTrianglesIndices()
		{
			int[] triangles 		= new int[6];
			
			triangles[0] = 0;
			triangles[1] = 2;
			triangles[2] = 1;
			
			triangles[3] = 0;
			triangles[4] = 3;
			triangles[5] = 2;

			mesh.triangles = triangles;
		}



		public void setVertexValues()
		{
			float sx = width/2;
			float sy = height/2;

			Vector3[] vertices = new Vector3[4];
			vertices[0] = new Vector3(-sx, -sy, 0);
			vertices[1] = new Vector3( sx, -sy, 0);
			vertices[2] = new Vector3( sx,  sy, 0);
			vertices[3] = new Vector3(-sx,  sy, 0);

			mesh.vertices = vertices;
			mesh.RecalculateBounds();
		}

		public void setUVValues()
		{
			float u0 = offsetX;
			float u1 = offsetX + width/pixelsPerUnit;
			float v0 = offsetY;
			float v1 = offsetY + height/pixelsPerUnit;

			Vector2[] uv = new Vector2[4];
				
			uv[0] = new Vector2( u0,   v0);
			uv[1] = new Vector2( u1,	  v0);
			uv[2] = new Vector2( u1,   v1);
			uv[3] = new Vector2( u0,   v1);

			mesh.uv = uv;

		}
	
		
	}


}
