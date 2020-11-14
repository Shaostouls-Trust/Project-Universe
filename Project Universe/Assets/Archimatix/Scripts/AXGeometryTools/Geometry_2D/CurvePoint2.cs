using System;
using System.Collections;
using UnityEngine;

/* PATH_POINT_2D
 *
 * Frozen point. Has Tangents.
 */

namespace AXGeometry
{


    /// <summary>
    /// PathPoint2.

    // A PathPoint2 has a poistion, tangent, velocity, and matrix for transorming section splines.
    /// </summary>
    [System.Serializable]
    public class CurvePoint2
    {

        public Vector3 position;

        //public Vector2 normal;

        public float angle;
        public bool isSharp;

        public float uDistance;
        public float uDistanceSinceLastSharpCorner;

        public float u;
        public float uSharps;

        public Vector2 bisector;
        public float bisectorAngle;

        public Quaternion rotation;
        public float scaleX = 1;
        public Matrix4x4 matrix;


        public CurvePoint2(Vector3 pos)
        {
            position = pos;

            angle = 0;
            isSharp = false;

            uDistance = 0;
            uDistanceSinceLastSharpCorner = 0;

            u = 0;
            uSharps = 0;

            bisector = Vector2.zero;
            rotation = Quaternion.identity;

            matrix = Matrix4x4.identity;
        }
        public CurvePoint2(Vector3 pos, float uVal)
        {
            position = pos;


            angle = 0;
            isSharp = false;

            uDistance = uVal;
            uDistanceSinceLastSharpCorner = 0;

            u = 0;
            uSharps = 0;
            bisector = Vector2.zero;
            rotation = Quaternion.identity;
            matrix = Matrix4x4.identity;
        }
        public CurvePoint2(Vector3 pos, float ang, bool _isSharp)
        {


            position = pos;


            angle = ang;
            isSharp = _isSharp;

            uDistance = 0;
            uDistanceSinceLastSharpCorner = 0;
            u = 0;
            uSharps = 0;

            bisector = Vector2.zero;
            rotation = Quaternion.identity;
            matrix = Matrix4x4.identity;
        }


        public CurvePoint2 clone()
        {
            CurvePoint2 cp = new CurvePoint2(position);

            cp.angle = angle;
            cp.isSharp = isSharp;

            cp.uDistance = uDistance;
            cp.uDistanceSinceLastSharpCorner = uDistanceSinceLastSharpCorner;
            cp.u = u;
            cp.uSharps = uSharps;

            cp.bisector = bisector;
            cp.rotation = rotation;
            cp.matrix = matrix;

            return cp;
        }


        public Vector3 LocalToWorld(Vector3 point)
        {
            //return  new Vector3(position.x, 0, position.y) + rotation * point;

            //Debug.Log(new Vector3(position.x, 0, position.y) + " > ::::::::::::::::: "+point);
            //Debug.Log(matrix);

            //Debug.Log("::::::::::::::::: < " + matrix.MultiplyPoint3x4(point));
            return matrix.MultiplyPoint3x4(point);

        }

    }

}