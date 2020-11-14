using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using AX;

using AXClipperLib;
using Path = System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;


namespace AXGeometry
{
	/// <summary>
	/// Curve2D
	/// Manages a collection of CurveControlPoint2D's and derived PathPoint2s
	/// </summary>
	/// 

	[System.Serializable]
	public class Curve2D
	{
		public List<CurveControlPoint2D> controlPoints = new List<CurveControlPoint2D>();

		// DERIVED PATH POINTS
		// The creation of derivedPoints is a product of which interpolation method is used; Catmull-Rom, Quadratic Bezier, Cubic Bezier, Piping, Ramping, etc.

		public float geomBreakingAngle = 60f;
		public bool isClosed = false;

		public Path path;

		public List<Vector2> derivedPointPositions = new List<Vector2>();
		public List<float> uDistances = new List<float>();


		public List<CurvePoint2> derivedCurvePoints = new List<CurvePoint2>();

		public List<int> lines = new List<int>();

		public List<List<int>> smoothSegments = new List<List<int>>();

		public float uDistance = 0;
		float uDistanceSinceLastSharpCorner = 0;

		public List<float> angles = new List<float>();

		// EDGE_ANGLES start at point 0 for the angle of the first edge.
		public List<float> edgeAngles = new List<float>();

		public bool endsPerpenducular = false;


        public Curve2D()
        {

        }


        // PATH CONSTRUCTOR
        // Constructor for Turtle-generated Path
        // - each Path point IS A control point.
        public Curve2D(Path _path, bool _isClosed = false, float _geomBreakAng = 60, bool _endsPerpenducular = false)
		{
			if (_path == null)
				return;

			isClosed = _isClosed;
			geomBreakingAngle = _geomBreakAng;

			endsPerpenducular = _endsPerpenducular;

			// CLipper.cleanPolygon seems to not like paths with two points....
			if (_path.Count > 2)
				path = Clipper.CleanPolygon(_path);
			else
				path = _path;


			for (int i = 0; i < path.Count; i++)
				controlPoints.Add(new CurveControlPoint2D(AXGeometry.Utilities.IntPt2Vec2(path[i])));

			DerivePointPositions();
			DeriveCurvePoints();
		}



		public void DerivePointPositions()
		{
			derivedPointPositions.Clear();

			// Use controlPoints to create derived way points
			for (int i = 0; i < controlPoints.Count; i++)
			{
				if (controlPoints[i].curvePointType == CurvePointType.Point)
					derivedPointPositions.Add(controlPoints[i].position);

				// !!! else OTHE TPES OF POINTS SUCH AS BEZIER....
			}

			// Remove last point if the same.

			if (derivedPointPositions != null && derivedPointPositions.Count > 0 && isSamePoint(derivedPointPositions[0], derivedPointPositions.Last()))
				derivedPointPositions.RemoveAt(derivedPointPositions.Count - 1);


			// Calculate angles / sharps
			if (derivedPointPositions.Count > 2)
			{
				calculateAnglesFromPositionPoints();


				// Shift array so a sharp corner is first?
				int firstSharpIndex = -1;
				for (int i = 0; i < angles.Count; i++)
				{
					if (isSharp(angles[i]))
					{
						firstSharpIndex = i;
						break;
					}
				}
				if (firstSharpIndex > 0)
				{
					derivedPointPositions = derivedPointPositions.ShiftLeft(firstSharpIndex);
					angles = angles.ShiftLeft(firstSharpIndex);
				}
			}

		}


		public bool isSamePoint(Vector2 a, Vector2 b, float toler = .0005f)
		{
			Vector2 d = a - b;
			if ((d.x * d.x + d.y * d.y) < toler * toler)
				return true;

			return false;
		}



		// DERIVE CURVE POINTS
		// Based on breakAngle of derivedPointPositions, create derivedCurvePoints.
		// CurvePoints cary the meta data needed to create meshes, such as the angle, the bisector, etc.
		public void DeriveCurvePoints()
		{
			if (derivedPointPositions == null || derivedPointPositions.Count == 0)
				return;

			derivedCurvePoints.Clear();
			smoothSegments.Clear();
			int linePointCounter = 0;
			uDistance = 0;
			uDistanceSinceLastSharpCorner = 0;



			int final_i = (isClosed) ? derivedPointPositions.Count : derivedPointPositions.Count - 1;

			// Add the first point
			CurvePoint2 cp = new CurvePoint2(derivedPointPositions[0], 0);

			List<int> smoothSeg = new List<int>();

			smoothSeg.Add(linePointCounter);
			lines.Add(linePointCounter); // PT 1

			uDistances.Add(0);

			calculateCurvePoint(cp, 0);
			derivedCurvePoints.Add(cp);



			for (int i = 1; i <= final_i; i++)
			{
				int pIndex = (isClosed && i == final_i) ? 0 : i;

				// CREATE POINT
				cp = new CurvePoint2(derivedPointPositions[pIndex]);
				calculateCurvePoint(cp, i);
				derivedCurvePoints.Add(cp);

				linePointCounter++;
				lines.Add(linePointCounter); //PT 2

				smoothSeg.Add(linePointCounter);

				if (i == final_i)
				{
					smoothSegments.Add(smoothSeg); ;
					break;
				}



				// End current segment and start a new on
				// with a double of this CurvePoint?
				if (Mathf.Abs(cp.angle) > geomBreakingAngle)
				{
					//Debug.Log("cp.angle=" + cp.angle + " this_i=" + this_i + " final_i=" + final_i);
					cp = cp.clone();

					// Finish current smoothsegment
					smoothSegments.Add(smoothSeg);

					// Start a new smooth segment
					smoothSeg = new List<int>();

					// Reset the distance for the last sharp
					uDistanceSinceLastSharpCorner = 0;


					// ADD POINT: Add this point again
					cp.uDistanceSinceLastSharpCorner = uDistanceSinceLastSharpCorner;
					derivedCurvePoints.Add(cp);
					linePointCounter++;

					smoothSeg.Add(linePointCounter);
				}
				if (i != final_i)
					lines.Add(linePointCounter); // PT 1
			}


			if (!isClosed && derivedCurvePoints.Count >= 2)
			{
				CurvePoint2 p0 = derivedCurvePoints[0];
				CurvePoint2 p1 = derivedCurvePoints[1];

				CurvePoint2 pl = derivedCurvePoints.Last();
				CurvePoint2 pll = derivedCurvePoints[derivedCurvePoints.Count - 2];

				if (endsPerpenducular)
				{
					Vector2 L1 = p1.position - p0.position;
					Vector2 L1_P = new Vector3(L1.y, -L1.x);

					Vector2 LN = pl.position - pll.position;
					Vector2 LN_P = new Vector3(LN.y, -LN.x);

					p0.bisectorAngle = Vector2.SignedAngle(L1_P, Vector2.right);
					pl.bisectorAngle = Vector2.SignedAngle(LN_P, Vector2.right);

					p0.scaleX = 1;
					pl.scaleX = 1;
				}
				else
				{


					if (p0.bisectorAngle > p1.bisectorAngle)
					{
						p0.bisectorAngle = p1.bisectorAngle + p1.angle;
						pl.bisectorAngle = pll.bisectorAngle - pll.angle;

					}
					else
					{
						p0.bisectorAngle = p1.bisectorAngle - p1.angle;
						pl.bisectorAngle = pll.bisectorAngle + pll.angle;

					}

					p0.scaleX = p1.scaleX;
					pl.scaleX = pll.scaleX;
				}
				p0.matrix = Matrix4x4.TRS(new Vector3(p0.position.x, 0, p0.position.y), Quaternion.Euler(0, p0.bisectorAngle, 0), new Vector3(p0.scaleX, 1, 1));
				pl.matrix = Matrix4x4.TRS(new Vector3(pl.position.x, 0, pl.position.y), Quaternion.Euler(0, pl.bisectorAngle, 0), new Vector3(pl.scaleX, 1, 1));
			}




			setSharpUs();
		}




		public void calculateAnglesFromPositionPoints()
		{
			angles.Clear();


			Vector2 pp, p, np, v1, v2;

			for (int i = 0; i < derivedPointPositions.Count; i++)
			{
				pp = prevPositionPoint(i);
				p = thisPositionPoint(i);
				np = nextPositionPoint(i);

				v1 = p - pp;
				v2 = np - p;

				angles.Add(Vector2.Angle(v2, v1));
			}

			if (!isClosed)
			{
				angles[0] = angles[1];
				angles[angles.Count - 1] = angles[angles.Count - 2];
			}


		}

		public bool isSharp(float angle)
		{
			if (Mathf.Abs(angle) > geomBreakingAngle)
				return true;

			return false;
		}

		//public void addLastEdge

		public void calculateCurvePoint(CurvePoint2 cp, int i)
		{
			// **** !!!!
			// **** CURRENTLY CAN"T HANDLE CURVE WITH TWO POINTS
			// **** GIVES NAN IN MATRIX

			Vector2 pp = prevPositionPoint(i);
			Vector2 p = thisPositionPoint(i);
			Vector2 np = nextPositionPoint(i);

			Vector2 v1 = p - pp;
			Vector2 v2 = np - p;



			if (i != 0)
			{
				edgeAngles.Add(Vector2.SignedAngle(new Vector2(1, 0), v1));


				float magnitude = v1.magnitude;
				uDistance += magnitude;
				uDistances.Add(uDistance);

				cp.uDistance = uDistance;
				uDistanceSinceLastSharpCorner += magnitude;
				cp.uDistanceSinceLastSharpCorner = uDistanceSinceLastSharpCorner;
			}
			// ANGLE
			cp.angle = Vector2.Angle(v2, v1); ;

			if (Mathf.Abs(cp.angle) > geomBreakingAngle)
				cp.isSharp = true;

			// NODE ROTATION & TRANSFORM
			Vector2 v1PN = (new Vector2(v1.y, -v1.x)).normalized;
			Vector2 v2PN = (new Vector2(v2.y, -v2.x)).normalized;

			// -- BISECTOR: the addition of the normalized perpendicular vectors leads to a bisector
			Vector2 bisector = v1PN + v2PN;
			cp.bisector = bisector;

			cp.bisectorAngle = -Mathf.Atan2(bisector.y, bisector.x) * Mathf.Rad2Deg;
			//if (cp.bisectorAngle < 0)
			//    cp.bisectorAngle += 360;

			// -- SCALE: we can get the scaler from the dot product
			cp.scaleX = bisector.magnitude / Vector2.Dot(v1PN, bisector);


			cp.matrix = Matrix4x4.TRS(new Vector3(p.x, 0, p.y), Quaternion.Euler(0, cp.bisectorAngle, 0), new Vector3(cp.scaleX, 1, 1));

			return;
		}





		/// <summary>
		/// SET U SHARPS
		/// 
		/// SmoothSegment U's from 0 to 1
		/// Sets the u to 0 at start of the segment and 1 at the end.
		/// 
		/// Set the U of CurvePoints addressed by the SmoothSegment's indices
		/// 
		/// </summary>
		public void setSharpUs()
		{
			// EACH SEGMENT
			for (int i = 0; i < smoothSegments.Count; i++)
			{
				List<int> smoothSegIndices = smoothSegments[i];

				int index = smoothSegIndices[0];
				int lastIndex = smoothSegIndices.Last();


				// * FIRST U 
				//   =======  is 0
				CurvePoint2 cp = derivedCurvePoints[index];
				cp.uSharps = 0;


				// * TWEEN U's 
				//   =========  a percentage of local distance
				if (smoothSegIndices.Count > 2)
				{
					float lengthOfSmoothSeg = derivedCurvePoints[lastIndex].uDistanceSinceLastSharpCorner;

					for (int j = 1; j < smoothSegIndices.Count - 1; j++)
					{
						index = smoothSegIndices[j];
						cp = derivedCurvePoints[index];
						cp.uSharps = cp.uDistanceSinceLastSharpCorner / lengthOfSmoothSeg;
					}
				}


				// * LAST U  
				//   ======  is 1
				cp = derivedCurvePoints[lastIndex];

				if (cp.uDistanceSinceLastSharpCorner < .02f)
					cp.uSharps = .01f;
				else
					cp.uSharps = 1;
			}

		}


		public Matrix4x4 getTransformationMatrixForLengthOnCurve(float u)
		{

			if (derivedCurvePoints.Count >= 2)
			{
				Vector2 pp = derivedPointPositions[0];

				Vector2 p;


				int count = derivedPointPositions.Count - 1;
				if (isClosed)
					count++;

				for (int i = 1; i <= derivedPointPositions.Count; i++)
				{

					if (i == derivedPointPositions.Count)
						p = derivedPointPositions[0];
					else
						p = derivedPointPositions[i];

					//Debug.Log("["+i + "]: " + p.position + " :: " + u + " --- "  + p.uDistance);

					// skip double point
					//if (p.uDistance == pp.
					//    continue;

					//Debug.Log(i + " --> " + uDistances[i] + " > " + u);
					if (uDistances[i] > u)
					{
						// this is the point past the u of interest

						float distOnThisEdge = u - uDistances[i - 1];
						//Debug.Log("distOnThisEdge=" + distOnThisEdge);


						float edgeLength = uDistances[i] - uDistances[i - 1];
						float lerpFactor = distOnThisEdge / edgeLength;


						Vector2 pos = Vector2.Lerp(pp, p, lerpFactor);


						return Matrix4x4.TRS(new Vector3(pos.x, 0, pos.y), Quaternion.Euler(0, -edgeAngles[i - 1], 0), Vector3.one);

					}

					pp = p;
				}
			}



			return Matrix4x4.identity;
		}








		public static Path GetPathFromCurve(List<CurveControlPoint2D> curve, ShapeState shapeState = ShapeState.Open, int segs = 3)
		{

			Path path = new Path();

			int msegs = Mathf.Max(1, Mathf.FloorToInt((float)segs));
			float timetick = 1 / (1f * msegs);

			for (int i = 0; i <= curve.Count - 1; i++)
			{
				if (i == curve.Count - 1 && shapeState == ShapeState.Open)
					break;

				CurveControlPoint2D a = curve[i];

				int next_i = (i == curve.Count - 1) ? 0 : i + 1;

				CurveControlPoint2D b = curve[next_i];

				if (a.isPoint() && b.isPoint())
				{
					if (i == 0)
						path.Add(AXGeometry.Utilities.Vec2_2_IntPt(a.position));

					path.Add(AXGeometry.Utilities.Vec2_2_IntPt(b.position));
				}
				else
				{

					int governor = 0;
					for (float t = 0; t <= (1 + .9f * timetick); t = t + timetick)
					{
						if (governor++ > 50)
						{
							Debug.Log("governor hit)");
							break;
						}

						if (i == 0 || t > 0)
						{

							Vector2 pt = bezierValue(curve[i], curve[next_i], t);
							path.Add(AXGeometry.Utilities.Vec2_2_IntPt(pt));
						}
					}
				}
			}
			return path;

		}

		public static Vector2 bezierValue(Vector2 pt0, Vector2 pt1, Vector2 pt2, Vector2 pt3, float t)
		{
			return Mathf.Pow((1 - t), 3) * pt0 + 3 * Mathf.Pow((1 - t), 2) * t * pt1 + 3 * (1 - t) * t * t * pt2 + Mathf.Pow(t, 3) * pt3;

		}
		public static Vector2 bezierValue(CurveControlPoint2D a, CurveControlPoint2D b, float t)
		{
			return Mathf.Pow((1 - t), 3) * a.position + 3 * Mathf.Pow((1 - t), 2) * t * (a.position + a.localHandleB) + 3 * (1 - t) * t * t * (b.position + b.localHandleA) + Mathf.Pow(t, 3) * b.position;

		}





		public void printSelf()
		{
			Debug.Log(" ----------------------------------------------------------");

			Debug.Log("derivedPointPositions.Count: " + derivedPointPositions.Count);
			int pt = 0;
			foreach (Vector2 cpp in derivedPointPositions)
				Debug.Log("[" + (pt++) + "] " + cpp.x + ", " + cpp.y);// + ", isSharp = " + cpp.isSharp + ", uSharp = " + cpp.uSharps);

			foreach (float a in angles)
				Debug.Log(a);

			Debug.Log(" ----------------------------------------------------------");
			Debug.Log("derivedCurvePoints.Count: " + derivedCurvePoints.Count);
			int ct = 0;
			foreach (CurvePoint2 cpp in derivedCurvePoints)
				Debug.Log("[" + (ct++) + "] " + cpp.position.x + ", " + cpp.position.y + ", " + cpp.position.z + ", isSharp = " + cpp.isSharp + ", uSharp = " + cpp.uSharps);


			string js = "";
			foreach (int j in lines)
			{
				js += " " + j;
			}
			Debug.Log(" >> -------------------------------------------------------------");
			Debug.Log("lines (in pairs): " + js);
			Debug.Log(" >> -------------------------------------------------------------");

			Debug.Log("smoothSegments: " + smoothSegments.Count);

			for (int i = 0; i < smoothSegments.Count; i++)
			{
				Debug.Log("** smooth segment[" + i + "]............................");
				List<int> smoothSegIndices = smoothSegments[i];
				for (int j = 0; j < smoothSegIndices.Count; j++)
				{
					Debug.Log("index[" + i + "_" + j + "]= " + smoothSegIndices[j] + " :: " + derivedCurvePoints[smoothSegIndices[j]].position + " ::: " + derivedCurvePoints[smoothSegIndices[j]].uSharps);
				}
			}
		}



		public Vector2 prevPositionPoint(int index)
		{
			if (derivedPointPositions.Count > 0)
			{
				//int backStep = ()
				return derivedPointPositions[(((index - 1) < 0) ? (derivedPointPositions.Count - 1) : index - 1)];

			}
			return Vector2.zero;
		}
		public Vector2 thisPositionPoint(int index)
		{
			// useful when you want to be able to stray beyond the Count of the positionPoints    
			if (derivedPointPositions.Count > 0)
				return derivedPointPositions[(index > (derivedPointPositions.Count - 1)) ? 0 : index];
			return Vector2.zero;
		}
		public Vector2 nextPositionPoint(int index)
		{
			if (derivedPointPositions.Count > 0)
				return derivedPointPositions[(index + 1) % derivedPointPositions.Count];
			return Vector2.zero;
		}



	}

}

