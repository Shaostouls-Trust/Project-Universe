using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using AX;

/* CURVE_POINT
 *
 * For controlling a Bezier curve. Has Tangents.
 */
 
namespace AXGeometry
{


    /// <summary>
    /// CurveControlPoint2D.

    // A CurveControlPoint2D is a point that has extra data associated with it.
    /// The common use is for a point on a Bezier curcve with handles.
    /// </summary>
    [System.Serializable]
	public class CurveControlPoint2D  {

		public string Guid;

		public CurvePointType curvePointType;

		public Vector2 position;

        public float angle;

        public bool isSharp = false;

        public string xExpression;
		public string yExpression;

		public Vector2 localHandleA;
		public Vector2 localHandleB;


		public float lastConvertTime;


		
		public CurveControlPoint2D (float _x, float _y)
		{
			position.x = _x;
			position.y = _y;

		}
		public CurveControlPoint2D (Vector2 p)
		{
			position = p;
			curvePointType = CurvePointType.Point;
			
		}
		public CurveControlPoint2D (Vector2 p, Vector2 a)
		{
			position = p;

			localHandleA = a - position;

			curvePointType = CurvePointType.BezierMirrored;
			localHandleB = -localHandleA;
		}
		public CurveControlPoint2D (Vector2 p, Vector2 a, Vector2 b)
		{
			position = p;

			localHandleA = a - position;
			localHandleB = b - position;

		}

		public bool isPoint()
		{
			if (curvePointType == CurvePointType.Point)
				return true;
			return false;
		}
		public bool isBezierPoint()
		{
			if (curvePointType == CurvePointType.BezierBroken || curvePointType == CurvePointType.BezierMirrored || curvePointType == CurvePointType.BezierUnified)
				return true;
			return false;
		}


		public void cycleConvertPoint(CurveControlPoint2D prev = null, CurveControlPoint2D next = null, float sinceLastClick = .5f )
		{
			
			if ((Time.time-lastConvertTime) > sinceLastClick)
			{
				
				switch(curvePointType)
				{
					case CurvePointType.Point:
						convertToBezier(prev, next);
						break;
					case CurvePointType.BezierMirrored:
						convertToBezierBroken(prev, next);;
						break;

					default:
						convertToPoint();
						break;

				}
			}

			lastConvertTime = Time.time;
		}

		public void convertToPoint()
		{
			curvePointType = CurvePointType.Point;
			localHandleA = Vector2.zero;
			localHandleB = Vector2.zero;

		}

		public void convertToBezier(CurveControlPoint2D prev = null, CurveControlPoint2D next = null)
		{
			
			curvePointType = CurvePointType.BezierMirrored;
			if (prev != null && next != null)
			{
				Vector2 tangent = (next.position-prev.position) * .25f;
				setHandleA(position-tangent);
				setHandleB(position+tangent);
			}

		}
		public void convertToBezierBroken(CurveControlPoint2D prev = null, CurveControlPoint2D next = null)
		{
			
			//curvePointType = CurvePointType.BezierMirrored;

			if (curvePointType != CurvePointType.BezierMirrored && prev != null && next != null)
			{
				Vector2 tangent = (next.position-prev.position) * .25f;
				setHandleA(position-tangent);
				setHandleB(position+tangent);
			}
			curvePointType = CurvePointType.BezierBroken;

		}




		public void setHandleA(Vector2 gloabalA)
		{
			localHandleA =  gloabalA - position;

			if (curvePointType == CurvePointType.BezierMirrored)
				localHandleB = -localHandleA; 
				
		}
		public void setHandleB(Vector2 gloabalB)
		{
			localHandleB = gloabalB - position ;

			if (curvePointType == CurvePointType.BezierMirrored)
				localHandleA = -localHandleB; 
				
		}




		public Vector3 asVec3()
		{
			return new Vector3(position.x, 0, position.y);
		}



		
		
	}
}
