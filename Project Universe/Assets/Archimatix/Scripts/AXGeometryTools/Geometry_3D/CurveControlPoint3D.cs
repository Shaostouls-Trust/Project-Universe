using UnityEngine;
using System.Collections;

/* CURVE_POINT_3D
 *
 * For controlling a Bezier curve. Has Tangents.
 */
 
namespace AXGeometry
{


	/// <summary>
	/// CurvePoint3D.

	// A CurvePoint is a point that has extra data associated with it.
	/// The common use is for a point on a Bezier curcve with handles.
	/// </summary>
	[System.Serializable]
	public class CurveControlPoint3D  {


		public CurvePointType curvePointType;

		public Vector3 		position;
		public Quaternion 	rotation;

        public bool isSharp = false;

        public Vector3 localHandleA;
        public Vector3 localHandleB;

        public CurveControlPoint3D()
        {
            position = Vector3.zero;
        }
        public CurveControlPoint3D(Vector3 pos)
        {
            position = pos;
        }
    }

}
