using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using AXClipperLib;
using Path 	= System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths 	= System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;

using AXPoly2Tri;
using PolygonPoints = System.Collections.Generic.List<AXPoly2Tri.PolygonPoint>;


using AXGeometry;

namespace AX
{

	
	public class AXPolygon {
	
	
		/* bridges to poly2tri 
		 * 
		 */
	
		
	
		public static Mesh triangulate(Path path, AXTexCoords tex, int seglenBigInt = 100000,bool useGrid = false)
		{
            //Debug.Log("A");
			/* Assume a single path with no holes 
			 * and return a mesh.
			 */
			if (path == null || path.Count < 3)
				return null;


			if (path[path.Count-1].X == path[0].X && path[path.Count-1].Y == path[0].Y)
				path.RemoveAt(path.Count-1);

			else if (AXGeometry.Utilities.IntPointsAreNear(path[0], path[path.Count-1]))
				path.RemoveAt(path.Count-1);



			//Paths tmpPaths = Clipper.SimplifyPolygon (path);

			 
			//CombineInstance[] combinator = new CombineInstance[tmpPaths.Count];

			//for (int i = 0; i < tmpPaths.Count; i++) {
			Mesh mesh = null;
			PolygonPoints _points;// = AXGeometry.Utilities.path2polygonPts (Pather.cleanPath(path));
			if (seglenBigInt > 0)
					_points = AXGeometry.Utilities.path2polygonPts(Pather.segmentPath(Clipper.CleanPolygon(path), seglenBigInt));
				else 
					_points = AXGeometry.Utilities.path2polygonPts(Clipper.CleanPolygon(path));



			Polygon _polygon = null;
			if (_points.Count >= 3) {
				_polygon = new Polygon (_points);
	
				
				if (_polygon != null) {
					try {

     
                        // STEINER POINTS

                        if (useGrid)
                        {
                            // GRID

                            Paths paths = new Paths();
                            paths.Add(path);
                            addSteinerPointsAsGrid(ref _polygon, paths, seglenBigInt);
                        }
                        else
                        {
                            // OFFSETS
                            ClipperOffset co = new ClipperOffset();
                            co.AddPath(path, AXClipperLib.JoinType.jtSquare, AXClipperLib.EndType.etClosedPolygon);
                            addSteinerPointsAtAllOffsets(ref _polygon, ref co, (float)seglenBigInt / ((float)AXGeometry.Utilities.IntPointPrecision), seglenBigInt);
                        }


                        P2T.Triangulate (_polygon);


					//foreach (DelaunayTriangle triangle in _polygon.Triangles)
						mesh = polygon2mesh (_polygon, tex);
					} catch {
						Debug.Log ("Can't triangulate: probably point on edge.");
					}
				}

					//combinator[i].mesh  		= mesh;
					//combinator [i].transform = Matrix4x4.identity;
					//return mesh;

				//}

			}


//			Mesh returnMesh = new Mesh();
//				returnMesh.CombineMeshes(combinator); 
//				return returnMesh;
			return mesh;

		}


		public static Mesh triangulate(Paths paths, AXTexCoords tex)
		{
			//Debug.Log ("B");
			List<Mesh> meshes = new List<Mesh>();
			
			foreach  (Path path in paths)
				meshes.Add (AXPolygon.triangulate(Clipper.CleanPolygon(path), tex));
			
			// combine
			CombineInstance[] combine = new CombineInstance[meshes.Count];
			for (int i = 0; i < meshes.Count; i++) {
				combine[i].mesh 		= meshes[i];
				combine[i].transform 	= Matrix4x4.identity;
			}
			
			Mesh mesh = new Mesh();
			mesh.CombineMeshes(combine);
			
			mesh.RecalculateNormals();
			
			return mesh;
			
		
		
		}
		
		
		


		
		
		public static Mesh triangulate(PolyNode polynode, AXTexCoords tex, int seglen = 9999999, bool useGrid = false, int gridSeglen = 9999999)
		{
            if (polynode == null || polynode.Childs == null || polynode.Childs.Count == 0)
                return null;

			return triangulate(polynode.Childs, tex, seglen, useGrid, gridSeglen);
		}
	
	
	
	
	
	
	
	
		
		
		public static Mesh triangulate(List<PolyNode> childs, AXTexCoords tex, int seglenBigInt = 9999999, bool useGrid = false, int gridSeglen= 9999999)
		{
			//Debug.Log ("C " + seglenBigInt);
			Polygon _polygon = null;
			
			
			List<Mesh> meshes = new List<Mesh>();

			if (seglenBigInt < 10)
				seglenBigInt = 100000;



			//int count = 0;
			foreach(PolyNode node in childs)
			{
				// Contour is Solid
//				List<TriangulationPoint> tripoints = new List<TriangulationPoint>();
//
               

				PolygonPoints _points = null;

				if (seglenBigInt > 0 && seglenBigInt != 9999999)
					_points = AXGeometry.Utilities.path2polygonPts(Pather.segmentPath(node.Contour, seglenBigInt));
				else 
					_points = AXGeometry.Utilities.path2polygonPts(node.Contour);

				// POLYGON
				if (_points.Count >= 3)
					_polygon = new Polygon(_points);
				
				// ADD HOLES TO POLYGON
				foreach(PolyNode subnode in node.Childs)
				{

					PolygonPoints hpoints =  null;

					if (seglenBigInt > 0 && seglenBigInt != 9999999)
						hpoints = AXGeometry.Utilities.path2polygonPts(Pather.segmentPath(subnode.Contour, seglenBigInt));
					else
						hpoints = AXGeometry.Utilities.path2polygonPts(subnode.Contour);

					if (hpoints.Count >= 3)
						_polygon.AddHole(new Polygon(hpoints));
				}




                // STEINER POINTS
                try
                {
                   // Debug.Log("HERE "+useGrid+" : " + gridSeglen);
                    // STEINER POINTS

                    if (useGrid && gridSeglen < 9999999)
                    {
                        // GRID

                        
                       
                        Paths paths = new Paths();
                        paths.Add(node.Contour);
                        addSteinerPointsAsGrid(ref _polygon, paths, gridSeglen);
                    }
                    else
                    {
                        // OFFSETS
                        ClipperOffset co = new ClipperOffset();
                        co.AddPath(node.Contour, AXClipperLib.JoinType.jtSquare, AXClipperLib.EndType.etClosedPolygon);
                        addSteinerPointsAtAllOffsets(ref _polygon, ref co, (float)seglenBigInt / ((float)AXGeometry.Utilities.IntPointPrecision), seglenBigInt);
                    }


                    P2T.Triangulate(_polygon);
					meshes.Add (polygon2mesh(_polygon, tex));

				} catch {
					//Debug.Log ("Can't triangulate: probably point on edge.");
				}
				
				
				// Continue down the tree...
				foreach(PolyNode cnode in node.Childs)
				{
					Mesh submesh = triangulate(cnode, tex);
					if (submesh != null)
						meshes.Add(submesh);
				}
				
				
			}
			
			CombineInstance[] combine = new CombineInstance[meshes.Count];
			for (int i = 0; i < meshes.Count; i++) {
				combine[i].mesh 		= meshes[i];
				combine[i].transform 	= Matrix4x4.identity;
			}
			
			
			
			Mesh mesh = new Mesh();
			mesh.CombineMeshes(combine);
			
			mesh.RecalculateNormals();
			
			return mesh;
			
			
		}
		
		
		


		


		
		
		
		public static Mesh triangulatePolyNode(PolyNode node, AXTexCoords tex, int seglenBigInt = 1000000, bool useGrid = false)
		{

			//Debug.Log ("D " + seglenBigInt);
			Polygon _polygon = null;

			if (seglenBigInt < 10)
				seglenBigInt = 999999;

			
			List<Mesh> meshes = new List<Mesh>();
			
				// Contour is Solid

			PolygonPoints _points = null;

				if (seglenBigInt > 0 && seglenBigInt != 9999999)
					_points = AXGeometry.Utilities.path2polygonPts( Pather.segmentPath(Clipper.CleanPolygon(node.Contour), seglenBigInt));
				else 
					_points = AXGeometry.Utilities.path2polygonPts(Clipper.CleanPolygon(node.Contour));
			
			// POLYGON
			if (_points.Count >= 3)
				_polygon = new Polygon(_points);
			
			//Debug.Log ("_polygon="+_points.Count);
			// ADD HOLES TO POLYGON
			foreach(PolyNode subnode in node.Childs)
			{
				PolygonPoints hpoints =  null;

				if (seglenBigInt > 0 && seglenBigInt != 9999999)
					hpoints = AXGeometry.Utilities.path2polygonPts(Pather.segmentPath(Clipper.CleanPolygon(subnode.Contour), seglenBigInt));
				else
					hpoints = AXGeometry.Utilities.path2polygonPts(Clipper.CleanPolygon(subnode.Contour));



				if (hpoints.Count >= 3)
					_polygon.AddHole(new Polygon(hpoints));
			}

			try {

                // STEINER POINTS

                if (useGrid)
                {
                    // GRID

                    Paths paths = new Paths();
                    paths.Add(node.Contour);
                    addSteinerPointsAsGrid(ref _polygon, paths, seglenBigInt);
                }
                else
                {
                    // OFFSETS
                    ClipperOffset co = new ClipperOffset();
                    co.AddPath(node.Contour, AXClipperLib.JoinType.jtSquare, AXClipperLib.EndType.etClosedPolygon);
                    addSteinerPointsAtAllOffsets(ref _polygon, ref co, (float)seglenBigInt / ((float)AXGeometry.Utilities.IntPointPrecision), seglenBigInt);
                }


                P2T.Triangulate(_polygon);
				meshes.Add (polygon2mesh(_polygon, tex));

			} catch {
				//Debug.Log ("Can't triangulate: probably point on edge.");
			}
			
			
			// Continue down the tree...
			/*
			foreach(PolyNode cnode in node.Childs)
			{
				Mesh submesh = triangulatePolyNode(cnode, tex);
				if (submesh != null)
					meshes.Add(submesh);
			}
			*/
				
				
			
			
			CombineInstance[] combine = new CombineInstance[meshes.Count];
			for (int i = 0; i < meshes.Count; i++) {
				combine[i].mesh 		= meshes[i];
				combine[i].transform 	= Matrix4x4.identity;
			}
			
			
			
			Mesh mesh = new Mesh();
			mesh.CombineMeshes(combine);
			
			mesh.RecalculateNormals();
			
			return mesh;
			
			
		}


        public static void addSteinerPointsAsGrid(ref Polygon _polygon, Paths paths, int seglen)
        {
           

  
            //long gridSizeLong = (long)(gridSize * AXGeometry.Utilities.IntPointPrecision);
            long gridSizeLong = (long)(seglen);

            IntRect bounds = Clipper.GetBounds(paths);
           
            long x = bounds.left;
            long y = bounds.top;
            while (x < bounds.right)
            {
                while (y < bounds.bottom)
                {
                    if (Clipper.PointInPolygon(new IntPoint(x, y), paths[0]) > 0)
                    {
                        _polygon.AddSteinerPoint(new TriangulationPoint((double)x / (double)AXGeometry.Utilities.IntPointPrecision, (double)y / (double)AXGeometry.Utilities.IntPointPrecision));
                    }

                    y += gridSizeLong;
                }
                y = bounds.top;
                x += gridSizeLong;
            }

        }
		
		public static void addSteinerPointsAtAllOffsets(ref Polygon _polygon, ref ClipperOffset co, float offset, int seglenBigInt)
		{
			float currOffset = 0;

			bool result = true;

			int gov = 0;

			while (result && gov++ != 256)
			{
				currOffset += offset;
				result = addSteinerPointsAtOffset(ref _polygon, ref co, currOffset, seglenBigInt);

			}

		}
		
		
		public static bool addSteinerPointsAtOffset(ref Polygon _polygon, ref ClipperOffset co, float offset, int seglenBigInt)
		{
			PolyTree resPolytree = new AXClipperLib.PolyTree();

			co.Execute (ref resPolytree, (double)(-offset * AXGeometry.Utilities.IntPointPrecision));
			Paths paths = Clipper.PolyTreeToPaths(resPolytree);

			if (paths != null && paths.Count > 0 && paths[0]!= null && paths[0].Count > 0)
			{

				foreach (Path path in paths)
				{
					if (path != null && path.Count > 0)
					{
						Path ppp = Pather.segmentPath( path, seglenBigInt);
						if (ppp != null && ppp.Count > 0)
							foreach(IntPoint ip in ppp)
							_polygon.AddSteinerPoint(new TriangulationPoint( (double)ip.X/(double)AXGeometry.Utilities.IntPointPrecision, (double)ip.Y/(double)AXGeometry.Utilities.IntPointPrecision ));
					}
				}


				return true;




			}
			return false;
		}

		
		
		
		
		
		
		
		
		
		
		
		public static Mesh triangulate(Spline s,  AXTexCoords tex)
		{
				Debug.Log ("E");
			if (s == null)
				return null;
			// poly has verts that are delimeted by 8888888 for holes
			
			
			// 1. transpose points to poly2tri structures
			// 2. create mesh
			Polygon _polygon = null;
			
			List<PolygonPoint> _points = new List<PolygonPoint>();
			
				
			for(int i=0; i<s.controlVertices.Count; i++)
				_points.Add(new PolygonPoint((double)s.controlVertices[i].x, (double)s.controlVertices[i].y));
			
			
			// POLYGON
			if (_points.Count >= 3)
			{
				_polygon = new Polygon(_points);
				
			}
			
			// populate the polygon triangles
			if (_polygon != null)
			{
				P2T.Triangulate(_polygon);
				return polygon2mesh(_polygon, tex);
			}
			return null;
			
		}
		
	
		public static Mesh triangulate(AXSpline poly, float height, AXTexCoords tex)
		{
			// poly has verts that are delimeted by 8888888 for holes
			
			//Debug.Log ("F");
			// 1. transpose points to poly2tri structures
			// 2. create mesh
			Polygon _polygon = null;
		
			List<AXSpline> parts = poly.getSolidAndHoles();
	
			if (parts == null || parts.Count == 0)
				return null;
	
			// CONTOUR
			AXSpline contour = parts[0];
			
			List<PolygonPoint> _points = new List<PolygonPoint>();
	
			for(int ii=0; ii<contour.vertCount; ii++)
				_points.Add(new PolygonPoint((double)contour.verts[ii].x, (double)contour.verts[ii].y));
	
	
			// POLYGON
			if (_points.Count >= 3)
			{
				_polygon = new Polygon(_points);
			
			
				// HOLES?
				if (parts.Count > 1)
				{
					for (int i=1; i<parts.Count; i++)
					{
						List<PolygonPoint> _holepoints = new List<PolygonPoint>();
						for(int ii=0; ii<parts[i].vertCount; ii++)
							_holepoints.Add(new PolygonPoint((double)parts[i].verts[ii].x, (double)parts[i].verts[ii].y));
						if (_holepoints.Count >= 3)
						{
							Polygon _hole = new Polygon(_holepoints);
							_polygon.AddHole(_hole);
						}
					}
				}
			}
	
			// populate the polygon triangles
			if (_polygon != null)
			{
				P2T.Triangulate(_polygon);
				
				return polygon2mesh(_polygon, tex);
			}
			return null;
	
		}
		
		
		
		
		
		
		
		
		
		
		
		
		
		

		
		
		
		

		
		
		
		
		
			// use trianges to construct a mesh
		public static Mesh polygon2mesh(Polygon poly, AXTexCoords tex)
		{

			Dictionary<TriangulationPoint, int> pointsDict = new  Dictionary<TriangulationPoint, int>();

			// GATHER UNIQUE POINTS INTO DICTIONARY
			int index = 0;
			foreach (DelaunayTriangle triangle in poly.Triangles)
			{

				if (! pointsDict.ContainsKey(triangle.Points[0]))
					  pointsDict[triangle.Points[0]] = index++;

				if (! pointsDict.ContainsKey(triangle.Points[1]))
					  pointsDict[triangle.Points[1]] = index++;
					  
				if (! pointsDict.ContainsKey(triangle.Points[2]))
					  pointsDict[triangle.Points[2]] = index++;
			}

			// CREAT VERTICES FROM UNIQUES
			Vector3[] vertices = new Vector3[pointsDict.Count];
			index = 0;
			foreach (TriangulationPoint pt in pointsDict.Keys)
				vertices[index++] = new Vector3 ((float)pt.X, 0, (float)pt.Y);

			if (tex == null)
				tex = new AXTexCoords();

			Mesh tmpMesh = new Mesh();


			int[] 		triangles 	= new 	int[3*poly.Triangles.Count];
			int cursor = 0;
			foreach (DelaunayTriangle triangle in poly.Triangles)
			{	
				triangles[cursor++] = pointsDict[triangle.Points[0]];//curv;
				triangles[cursor++] = pointsDict[triangle.Points[2]];//curv;
				triangles[cursor++] = pointsDict[triangle.Points[1]];//curv;
			}


			// SET OTHER MESH DATA
			Vector2[] 	uv 			= new Vector2[vertices.Length];
			Vector3[] 	normals 	= new Vector3[vertices.Length];
			Vector4[] 	tangents 	= new Vector4[vertices.Length];

			Vector4 tangent = new Vector4(1, 0, 0, -1);
			float u, v;
			for (int i=0; i<vertices.Length; i++)
			{

                if (tex.useRectMapping)
                {
                   
                    uv[i] = tex.uvFromRectMapping(new Vector2(vertices[i].x, vertices[i].z));
                   
                }
                else
                {
                    if (tex.scale.x != 0)
                    {
                        u = (vertices[i].x + tex.shift.x) / tex.scale.x;
                        v = (vertices[i].z + tex.shift.y) / tex.scale.y;
                    }
                    else
                    {
                        u = (vertices[i].x / .3f);
                        v = (vertices[i].z / .3f);
                    }
                    uv[i] = (tex.rotateCapsTex) ? new Vector2(v, u) : new Vector2(u, v);


                }


                normals[i]  = Vector3.up;
				tangents[i] = tangent;
			}

			// POPULATE MESH
			tmpMesh.vertices	= vertices;
			tmpMesh.uv 			= uv;
			tmpMesh.triangles 	= triangles;
			tmpMesh.normals 	= normals;
			tmpMesh.tangents	= tangents;

			return tmpMesh;
	
		}
	




		// use trianges to construct a mesh
		public static Mesh polygon2meshOLD(Polygon poly, AXTexCoords tex)
		{
		Debug.Log(" OOOOOOOO " + poly.Triangles.ToString());
		for (int i = 0; i < poly.Points.Count; i++) {
			TriangulationPoint pt = poly.Points [i];
			Debug.Log ("pt [" + i + "] " + pt);
		}
			if (tex == null)
				tex = new AXTexCoords();
			Mesh tmpMesh = new Mesh();
			Vector3[] verts 	= new Vector3[3*poly.Triangles.Count];
			Vector2[] uv 		= new Vector2[3*poly.Triangles.Count];
			int[] triangles 	= new 	  int[3*poly.Triangles.Count];
			
			Vector3[] normals 	= new Vector3[3*poly.Triangles.Count];
			Vector3[] tangents 	= new Vector3[3*poly.Triangles.Count];
			
			int curv = 0;
	
			float u, v;
	
			Vector3 normal = Vector3.up;
			Vector3 tangent = Vector3.right;
			
			foreach (DelaunayTriangle triangle in poly.Triangles)
			{
				verts[curv] = new Vector3 ((float)triangle.Points[0].X, 0, (float)triangle.Points[0].Y);
				if (tex.scale.x != 0) {
					u = (verts[curv].x + tex.shift.x)/ tex.scale.x;
					v = (verts[curv].z + tex.shift.y)/ tex.scale.y;
				} else {
					u = (verts[curv].x / .3f);
					v = (verts[curv].z / .3f);
				}
				uv		[curv] = new Vector2 (u, v);	
				triangles[curv] = curv;
				normals[curv] 	= normal;
				tangents[curv]	= tangent;
				curv++;
	
				verts[curv] = new Vector3 ((float)triangle.Points[2].X, 0, (float)triangle.Points[2].Y);
				if (tex.scale.x != 0) {
					u = (verts[curv].x + tex.shift.x)/ tex.scale.x;
					v = (verts[curv].z + tex.shift.y)/ tex.scale.y;
				} else {
					u = (verts[curv].x / .3f);
					v = (verts[curv].z / .3f);
				}
				uv		[curv] = new Vector2 (u, v);
				//Debug.Log ("uv = " + u + ", " + v);
				triangles[curv] = curv;
				normals[curv] = normal;
				tangents[curv]	= tangent;
				curv++;
	
				verts[curv] = new Vector3 ((float)triangle.Points[1].X, 0, (float)triangle.Points[1].Y);
				if (tex.scale.x != 0) {
					u = (verts[curv].x + tex.shift.x)/ tex.scale.x;
					v = (verts[curv].z + tex.shift.y)/ tex.scale.y;
				} else {
					u = (verts[curv].x / .3f);
					v = (verts[curv].z / .3f);
				}
				uv		[curv] = new Vector2 (u, v);	
				triangles[curv] = curv;
				normals[curv] = normal;
				tangents[curv]	= tangent;
				curv++;
	
			}
			tmpMesh.vertices	= verts;
			tmpMesh.uv 			= uv;
			tmpMesh.triangles 	= triangles;
			tmpMesh.normals 	= normals;

			tmpMesh.RecalculateTangents();


			return tmpMesh;
	
		} 
	
	
	
	}

}
