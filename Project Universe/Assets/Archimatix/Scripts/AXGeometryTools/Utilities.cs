using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;

using AXClipperLib;
using Path 			= System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths 		= System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;

using AXPoly2Tri;
using PolygonPoints = System.Collections.Generic.List<AXPoly2Tri.PolygonPoint>;

using AX;

namespace AXGeometry
{
	public class Utilities
	{

		
		public static int IntPointPrecision = 1000;

		public static void setRealtimePrecisionFactor(PrecisionLevel l)
		{
			switch ((int) l) 
			{
			case 0:
				IntPointPrecision = 10000;
				break;
			case 1:
				IntPointPrecision = 1000;
				break;
			case 2:
				IntPointPrecision = 100;
				break;
			default:
				IntPointPrecision = 1000;
				break;

			}


		}

		public static void setBuildPrecisionFactor(PrecisionLevel l)
		{
			switch ((int) l) 
			{
			case 0:
				IntPointPrecision = 1000000;
				break;
			case 1:
				IntPointPrecision = 10000;
				break;
			case 2:
				IntPointPrecision = 1000;
				break;
			default:
				IntPointPrecision = 10000;
				break;

			}



		}


		public static bool PolygonPointsAreNear(PolygonPoint a, PolygonPoint b)
		{

			if ( Math.Abs(Math.Abs(a.X*a.X +a.Y*a.Y)-Math.Abs(b.X*b.X +b.Y*b.Y)) < .01f)
				return true;
			return false;
				
		}

		public static bool IntPointsAreNear(IntPoint a, IntPoint b)
		{
			if (Mathf.Abs(a.X-b.X) < 10 && Mathf.Abs(a.Y-b.Y) < 10)
				return true;
//		Debug.Log("a: " + a.X + ", " + a.Y + " - b: " + b.X + ", " + b.Y);
//			if ( Math.Abs(Math.Abs(a.X*a.X +a.Y*a.Y)-Math.Abs(b.X*b.X +b.Y*b.Y)) < 5)
//				return true;
			return false;
				
		}



		public static float cInt2f (int ci)
		{
			return ((float)ci)/IntPointPrecision;
		}

		public static Vector2 IntPt2Vec2 (IntPoint ip)
		{
			return new Vector2( ((float)ip.X)/IntPointPrecision, ((float)ip.Y)/IntPointPrecision);
		}

		public static Vector3 IntPt2Vec3 (IntPoint ip, float y=0)
		{
			return new Vector3( ((float)ip.X)/IntPointPrecision, y, ((float)ip.Y)/IntPointPrecision);
		}

		public static  IntPoint Vec2_2_IntPt(Vector2 v)
		{
			return new IntPoint((int)(IntPointPrecision*v.x), (int)(IntPointPrecision*v.y));
		}

		public static  IntPoint CurvePt_2_IntPt(CurveControlPoint2D c)
		{
			return new IntPoint((int)(IntPointPrecision*c.position.x), (int)(IntPointPrecision*c.position.y));
		} 




		public static float pathArea (Path path)
		{

			return Mathf.Abs(((float) Clipper.Area(path)) / Mathf.Pow(IntPointPrecision, 2) );

		}


		public static Path curve2Path(List<CurveControlPoint2D> curve)
		{ 
			Path path = new Path();

			for (int i=0; i<curve.Count; i++)
			{
				path.Add(CurvePt_2_IntPt(curve[i]));
			}
			return path;
		}
		public static AXSpline curve2Spline(List<CurveControlPoint2D> curve)
		{ 
			AXSpline s = new AXSpline();

			for (int i=0; i<curve.Count; i++)
			{
				s.Push(curve[i].position);
			}
			return s;
		}

		public static Path spline2Path(AXSpline s)
		{ 
			Path path = new Path();

			for (int i=0; i<s.vertCount; i++)
			{
				path.Add(Vec2_2_IntPt(s.verts[i]));
			}
			return path;
		}

		public static Path Vec2Array_2_Path(Vector2[] s)
		{ 
			Path path = new Path();

			for (int i=0; i<s.Length; i++)
			{
				path.Add(Vec2_2_IntPt(s[i]));
			}
			return path;
		}

		public static Path Spline2Path(Spline s)
		{ 
			Path path = new Path();

			for (int i=0; i<s.controlVertices.Count; i++)
				path.Add(Vec2_2_IntPt(s.controlVertices[i]));
			
			return path;
		}

		public static Paths Splines2Paths(List<Spline> splines)
		{
			Paths retPaths = new Paths();
			for (int i=0; i<splines.Count; i++)
				retPaths.Add(Spline2Path(splines[i]));

			return retPaths;
		}

		public static IntPoint getCenter(Paths paths)
		{
			if (paths == null)
				return new IntPoint();

			
			IntRect ir = Clipper.GetBounds(paths);

			return new IntPoint( (ir.left + (ir.right - ir.left)/2), (ir.bottom + (ir.top - ir.bottom)/2) );
		}

		public static long[] getShifterToPathsCenter(Paths paths)
		{
			long[] shifter = new long[3];

			if (paths == null)
				return shifter;


			IntRect ir = Clipper.GetBounds(paths);

			shifter[0] 	= ir.left + (ir.right - ir.left)/2;
			shifter[1] 	= ir.bottom + (ir.top - ir.bottom)/2;	

			shifter[2]	= Math.Max((ir.right - ir.left), (ir.top - ir.bottom));

			return shifter;

		}

		public static Paths shiftPathsAsGroup(Paths paths, long[] shifter)
		{
			if (paths == null)
				return null;

			Paths retPaths = new Paths();

			foreach (Path path in paths)
			{
				Path tmp = new Path();
				foreach (IntPoint p in path)
					tmp.Add (new IntPoint(p.X-shifter[0], p.Y-shifter[1]));

				retPaths.Add (tmp);

			}

			return retPaths;


		}

		public static Paths centerPathsAsGroup(Paths paths)
		{
			if (paths == null)
				return null;

			return shiftPathsAsGroup(paths,  getShifterToPathsCenter(paths));

		}


		public static Rect getClipperBounds(Paths paths)
		{
			if (paths == null)
				return new Rect();
			/*
			Debug.Log ("get bounds from these paths");
			foreach(Path p in paths)
				Debug.Log (Archimatix.pathToString(p));
	*/
			IntRect ir = Clipper.GetBounds(paths);

			float x1 = ((float)ir.left)/IntPointPrecision;
			float y1 = ((float)ir.top)/IntPointPrecision;
			float x2 = ((float)ir.right)/IntPointPrecision;
			float y2 = ((float)ir.bottom)/IntPointPrecision;

			return new Rect(x1, y1, x2-x1, y2-y1);
		}



		/// <summary>
		/// Cleans the paths.
		/// </summary>
		/// <returns>Returns a set of Paths where all points that are closer than tolerance are merged.</returns>
		/// <param name="paths">Paths.</param>


		public static Paths cleanPaths(Paths paths, int tolerance = 10)
		{
			return Pather.cleanPaths(paths, tolerance);

			//return AXClipperLib.Clipper.CleanPolygons(paths, tolerance);
			//return paths;
//
//			// check the distance for between conscecutive IntPoints and if closer than tolerance replace the two points witha  median point.
//			Paths retPaths = new Paths();
//			foreach (Path path in paths)
//			{
//				
//
//
//			}
//			return retPaths;

		}
		public static Path cleanPath(Path path, int tolerance)
		{
			Path retPath = new Path();

//			foreach (IntPoint ip in path)
//			{
//				
//
//
//			}
			return retPath;

		}



		public static void printPath(Path path)
		{
			foreach(IntPoint ip in path)
				Debug.Log (ip.X+", "+ip.Y);
		}
		public static string pathToString(Path path) 
		{
			string ret = "";
			foreach (IntPoint ip in path)
				ret += " ("+ip.X+", "+ip.Y+")";
			return ret;
		}
		public static void printPaths(Paths paths)
		{
			if (paths == null) {
				Debug.Log("print paths: EMPTY");
				return;
			}
			Debug.Log (paths.Count + " paths ------- ");
			int c = 0;
			foreach(Path p in paths)
			{
				Debug.Log ("["+(c++)+"] " + pathToString(p));

			}
			Debug.Log ("end paths ------- ");

		}

		public static AXSpline path2AXSpline(Path path)
		{
			AXSpline s = new AXSpline();
			for (int i=0; i< path.Count; i++)
				s.Push(IntPt2Vec2(path[i]));
			return s;
		}

        public static Vector2[] path2Vec2s(Path path)
        {
            Vector2[] verts = new Vector2[path.Count];

  
            for (int i = 0; i < path.Count; i++)
            {
                verts[i] = IntPt2Vec2(path[i]);
            }


            return verts;
        }


        public static Vector2[] path2Vec2s(Path path, float degs, float scale)
		{
			Vector2[] verts = new Vector2[path.Count];

			float sin = Mathf.Sin(-degs * Mathf.Deg2Rad);
			float cos = Mathf.Cos(-degs * Mathf.Deg2Rad);

			Vector2 tmp;
			float tx;
			float ty;

			for (int i=0; i< path.Count; i++)
			{
				tmp = IntPt2Vec2(path[i]);
				tx = tmp.x;
				ty = tmp.y;
				tmp.x = (cos * tx) - (sin * ty);
				tmp.y = (sin * tx) + (cos * ty);

				verts[i] = tmp * scale;
			}


			return verts;
		}


		public static Vector3[] path2Vec3s(Path path, Axis axis, float axisVal = 0f)
		{
			Vector3[] verts = new Vector3[path.Count];

			for (int i=0; i< path.Count; i++)
			{
				if (axis == Axis.Y)
					verts[i] = new Vector3( ((float)path[i].X)/IntPointPrecision, axisVal, ((float)path[i].Y)/IntPointPrecision);
				else
					verts[i] = new Vector3( ((float)path[i].X)/IntPointPrecision, ((float)path[i].Y)/IntPointPrecision, axisVal);
			}

			return verts;
		}

		public static Vector3[] path2Vec3s(Path path, Axis axis, bool isClosed)
		{
			bool addFirstPointToEnd = true;


			if (path.Count > 0 && path[0] == path[path.Count-1])
				addFirstPointToEnd = false; // don't add first point


			int count = (isClosed && addFirstPointToEnd) ? path.Count+1 : path.Count;


			Vector3[] verts = new Vector3[count];

			for (int i=0; i< path.Count; i++)
			{
				verts[i] = new Vector3( ((float)path[i].X)/IntPointPrecision, ((float)path[i].Y)/IntPointPrecision, 0);
			}
			if(isClosed && addFirstPointToEnd)
			{
				verts[verts.Length-1] = verts[0];
			}
			return verts;
		}




		public static PolygonPoints path2polygonPts(Path path)
		{
			PolygonPoints ppoints = new PolygonPoints();

			foreach(IntPoint ip in path)
				ppoints.Add (new PolygonPoint( ((double) ip.X)/IntPointPrecision, ((double) ip.Y)/IntPointPrecision  ));

			return ppoints;
		}

		public static Paths transformPaths(Paths paths, float xx, float yy, float degs)
		{
			if (paths == null)
				return new Paths();

			Paths clonePaths = new Paths();
			Path clonePath = null;

			IntPoint tmp_ip;

			//Vector2 tmp;
			long tx;
			long ty;


			float sin = Mathf.Sin(-degs * Mathf.Deg2Rad);
			float cos = Mathf.Cos(-degs * Mathf.Deg2Rad);


			foreach (Path path in paths)
			{
				clonePath = new Path();

				foreach (IntPoint ip in path)
				{
					// rotate
					tx = ip.X;
					ty = ip.Y;

					tmp_ip = new IntPoint((int)((cos * tx) - (sin * ty)) ,  (int)((sin * tx) + (cos * ty)) );
					tmp_ip.X += (int)(xx*IntPointPrecision);
					tmp_ip.Y += (int)(yy*IntPointPrecision);
					clonePath.Add(tmp_ip);
				}
				clonePaths.Add (clonePath);
			}

			return clonePaths;

		}








		public static Matrix4x4 getEndTransformA(Path path)
		{
			if (path == null || path.Count == 0)
				return Matrix4x4.identity;

			// use the first two points
			Vector2 v = AXGeometry.Utilities.IntPt2Vec2(path[0]) - AXGeometry.Utilities.IntPt2Vec2(path[1]);
			Quaternion rot = Quaternion.identity;
			if (v != Vector2.zero)
				rot.SetLookRotation(new Vector3(v.x, 0, v.y), Vector3.down);
			return  Matrix4x4.TRS(IntPt2Vec3(path[0], 0), Quaternion.Euler (-90, rot.eulerAngles.y, 180), Vector3.one );
		}

		public static Matrix4x4 getEndTransformB2(Path path)
		{
			if (path == null || path.Count == 0)
				return Matrix4x4.identity;

			if (path[0] == path[path.Count-1])
				path.RemoveAt(path.Count-1);

			if (path.Count < 2)
				return Matrix4x4.identity;

			// use the  2 points
			Vector2 v = AXGeometry.Utilities.IntPt2Vec2(path[0]) - AXGeometry.Utilities.IntPt2Vec2(path[1]);

			Quaternion rot = Quaternion.identity;
			if (v != Vector2.zero)
				rot.SetLookRotation(new Vector3(v.x, 0, v.y), Vector3.down);
			return  Matrix4x4.TRS(IntPt2Vec3(path[1], 0), Quaternion.Euler (90, rot.eulerAngles.y, 0),  new Vector3(-1, -1, -1) );

		}

		public static Matrix4x4 getEndTransformB(Path path, bool isBeveled = false)
		{
			if (path == null || path.Count == 0)
				return Matrix4x4.identity;

			if (path[0] == path[path.Count-1])
				path.RemoveAt(path.Count-1);

			if (path.Count == 2)
				return getEndTransformB2(path);

			if (path.Count < 3)
				return Matrix4x4.identity;

			// use the last 2 points
			Vector2 v = AXGeometry.Utilities.IntPt2Vec2(path[path.Count-1]) - AXGeometry.Utilities.IntPt2Vec2(path[path.Count-2]);
			Vector2 vp = AXGeometry.Utilities.IntPt2Vec2(path[path.Count-2]) - AXGeometry.Utilities.IntPt2Vec2(path[path.Count-3]);

			Quaternion rot = Quaternion.identity;
			if (v != Vector2.zero)
				rot.SetLookRotation(new Vector3(v.x, 0, v.y), Vector3.down);

			Quaternion rotp = Quaternion.identity;
			if (vp != Vector2.zero)
				rotp.SetLookRotation(new Vector3(vp.x, 0, vp.y), Vector3.down);


			float ang = rot.eulerAngles.y;

			if (isBeveled)
				ang += (rot.eulerAngles.y-rotp.eulerAngles.y)/2;

			return Matrix4x4.TRS(IntPt2Vec3(path[path.Count-1], 0), Quaternion.Euler (90, ang, 180),  new Vector3(-1, -1, -1) );
		}


		public static List<Matrix4x4> getEndTransforms(Paths paths)
		{
			// Steps through a Paths and for each Path generates two transforms. 
			// Useful for the end caps of a plan sweep, among other things

			if (paths == null)
				return null;


			List<Matrix4x4> transforms = new List<Matrix4x4>();

			foreach ( Path path in paths )
			{

				if (path[0] == path[path.Count-1])
				{
					path.RemoveAt(path.Count-1);
					//isClosed = true;
				}
				if (path.Count < 2)
					continue;


				// use the first two points
				Vector2 v = IntPt2Vec2(path[0]) - IntPt2Vec2(path[1]);
				Quaternion rot = Quaternion.identity;
				if (v != Vector2.zero)
					rot.SetLookRotation(new Vector3(v.x, 0, v.y), Vector3.down);
				transforms.Add( Matrix4x4.TRS(IntPt2Vec3(path[0], 0), Quaternion.Euler (-90, rot.eulerAngles.y, 180), Vector3.one ) );

				// use the last 2 points
				v = IntPt2Vec2(path[path.Count-1]) - IntPt2Vec2(path[path.Count-2]);
				rot = Quaternion.identity;
				rot.SetLookRotation(new Vector3(v.x, 0, v.y), Vector3.down);
				transforms.Add( Matrix4x4.TRS(IntPt2Vec3(path[path.Count-1], 0), Quaternion.Euler (90, rot.eulerAngles.y, 180), -Vector3.one ) );

			}

			return transforms;

		}

		public static Matrix4x4 getFirstSegmentHandleMatrix(Paths paths)
		{



			if (paths != null && paths.Count > 0 && paths[0] != null)
			{
				Path firstPath = paths[0];


				//printPath(firstPath);

				if (firstPath.Count < 2)
					return Matrix4x4.identity;

				Vector3 p0 				= IntPt2Vec3( firstPath[0] );
				Vector3 p1 				= IntPt2Vec3( firstPath[1] );
				Vector3 secLocalOrigin 	= Vector3.Lerp(p1, p0, .75f);


				Vector3 v = p0 - p1;
				Quaternion rot = Quaternion.identity;
				//rot.SetFromToRotation(v, Vector3.down);
				if (v != Vector3.zero)
					rot.SetLookRotation(Vector3.up, v);

				//Debug.Log ("HANDLE: " + p0 + " :: " + p1 + ", " + v + ", rot = " + rot.eulerAngles.y);

				return Matrix4x4.TRS(secLocalOrigin, rot, Vector3.one );
			}
			return Matrix4x4.identity;
		}










		// CLONE_PATH
		public static Path clonePath(Path path)
		{
			if (path == null)
				return null;

			Path clonePath = new Path();

			for (int j=0; j<path.Count; j++)
				clonePath.Add(new IntPoint(path[j].X, path[j].Y));

			return clonePath;
		}

		// CLONE_PATHS
		public static Paths clonePaths(Paths paths)
		{
			if (paths == null)
				return null;

			Paths clonePaths 	= new Paths();

			for(int i=0; i<paths.Count; i++)
				clonePaths.Add (clonePath(paths[i]));

			return clonePaths;
		}



		public static void reversePaths(ref Paths paths)
		{
			for(int i=0; i<paths.Count; i++)
				paths[i].Reverse();

			
		}


		// TRANSFORM_PATH
		public static Path transformPath(Path path, Matrix4x4 m)
		{
			if (path == null)
				return null;

			Path clonePath = new Path();
			IntPoint tmp_ip;

			for (int j=0; j<path.Count; j++)
			{
				tmp_ip	= path[j];

				Vector3 pt = new Vector3( (float)tmp_ip.X/(float)IntPointPrecision, (float)tmp_ip.Y/(float)IntPointPrecision, 0);

				pt = m.MultiplyPoint3x4(pt);

				tmp_ip = new IntPoint((int)(pt.x*IntPointPrecision), (int)(pt.y*IntPointPrecision));

				clonePath.Add(tmp_ip);
			}

			return clonePath;
		}

		// TRANSFORM_PATHS
		public static Paths transformPaths(Paths paths, Matrix4x4 m)
		{
			if (paths == null)
				return null;

			Paths clonePaths = new Paths();
			Path clonePath = null;

			for(int i=0; i<paths.Count; i++)
			{
				clonePath = transformPath(paths[i], m);

				//if (Clipper.Orientation(clonePath) != Clipper.Orientation(paths[i]))
				//{
					//Debug.Log("transformPaths Reverse");
					//clonePath.Reverse();

				//}

				clonePaths.Add (clonePath);
			}
			return clonePaths;
		}


		// REVERSE POLYTREE
		public static void reversePolyTree(PolyTree polyTree)
		{
			if (polyTree == null)
				return;

			if (polyTree.Childs != null && polyTree.Childs.Count > 0)
				reversePolyTree(polyTree.Childs);
		}



		// TRANSFORM_POLY_NODE
		public static void reversePolyTree(List<PolyNode> childs)
		{
			if (childs == null || childs.Count == 0)
				return;

			for (int i = 0; i < childs.Count; i++) {
				PolyNode child = childs [i];

				child.Contour.Reverse();

				if (child.Childs != null)
					reversePolyTree (child.Childs);
			}

		}




		// TRANSFORM_POLY_TREE
		public static void transformPolyTree(PolyTree polyTree, Matrix4x4 m)
		{
			if (polyTree == null)
				return;

			if (polyTree.Childs != null && polyTree.Childs.Count > 0)
				transformPolyNode(polyTree.Childs, m);
		}

		// TRANSFORM_POLY_NODE
		public static void transformPolyNode(List<PolyNode> childs, Matrix4x4 m)
		{
			if (childs == null || childs.Count == 0)
				return;

			foreach(PolyNode child in childs)
			{
				Path tmpPath = transformPath(child.Contour, m);
				for (int i = 0; i < tmpPath.Count; i++) 
					child.Contour[i] = tmpPath [i];

				if (child.Childs != null)
					transformPolyNode(child.Childs, m);
			}

		}




































		// DEPRICATED - now using mesh.RecalulateTangents() everywhere
		public static void calculateMeshTangents(ref Mesh mesh)
		{ 
			//mesh.RecalculateNormals();
			if (mesh == null)
				return;

			//speed up math by copying the mesh arrays
			int[] triangles = mesh.triangles;
			Vector3[] vertices = mesh.vertices;
			Vector2[] uv = mesh.uv;
			Vector3[] normals = mesh.normals;

			//variable definitions
			int triangleCount = triangles.Length;
			int vertexCount = vertices.Length;

			Vector3[] tan1 = new Vector3[vertexCount];
			Vector3[] tan2 = new Vector3[vertexCount];

			Vector4[] tangents = new Vector4[vertexCount];

			for (long a = 0; a < triangleCount; a += 3)
			{
				long i1 = triangles[a + 0];
				long i2 = triangles[a + 1];
				long i3 = triangles[a + 2];

				Vector3 v1 = vertices[i1];
				Vector3 v2 = vertices[i2];
				Vector3 v3 = vertices[i3];

				Vector2 w1 = uv[i1];
				Vector2 w2 = uv[i2];
				Vector2 w3 = uv[i3];

				float x1 = v2.x - v1.x;
				float x2 = v3.x - v1.x;
				float y1 = v2.y - v1.y;
				float y2 = v3.y - v1.y;
				float z1 = v2.z - v1.z;
				float z2 = v3.z - v1.z;

				float s1 = w2.x - w1.x;
				float s2 = w3.x - w1.x;
				float t1 = w2.y - w1.y;
				float t2 = w3.y - w1.y;

				float r = 1.0f / (s1 * t2 - s2 * t1);

				Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
				Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

				tan1[i1] += sdir;
				tan1[i2] += sdir;
				tan1[i3] += sdir;

				tan2[i1] += tdir;
				tan2[i2] += tdir;
				tan2[i3] += tdir;
			}


			for (long a = 0; a < vertexCount; ++a)
			{
				Vector3 n = normals[a];
				Vector3 t = tan1[a];

				//Vector3 tmp = (t - n * Vector3.Dot(n, t)).normalized;
				//tangents[a] = new Vector4(tmp.x, tmp.y, tmp.z);
				Vector3.OrthoNormalize(ref n, ref t);
				tangents[a].x = t.x;
				tangents[a].y = t.y;
				tangents[a].z = t.z;

				tangents[a].w = (Vector3.Dot(Vector3.Cross(n, t), tan2[a]) < 0.0f) ? -1.0f : 1.0f;
			}

			mesh.tangents = tangents;
		}



		// SNAPPING TOOLS
		public static float SnapToGrid(float val, float cellSize)
		{
			return cellSize * (int) Mathf.Round(val / cellSize);
		}
		public static Vector2 SnapToGrid(Vector2 point, float cellSize)
		{
			float x = cellSize * (int) Mathf.Round(point.x / cellSize);
			float y = cellSize * (int) Mathf.Round(point.y / cellSize);
		    return new Vector2(x, y);
		}

		public static Vector3 SnapToGrid(Vector3 point, float cellSize)
		{
			float x = cellSize * (int) Mathf.Round(point.x / cellSize);
			float y = cellSize * (int) Mathf.Round(point.y / cellSize);
			float z = cellSize * (int) Mathf.Round(point.z / cellSize);
		    return new Vector3(x, y, z);
		}



	} 



















}

