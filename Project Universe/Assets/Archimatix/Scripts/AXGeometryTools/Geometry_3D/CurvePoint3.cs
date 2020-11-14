using System;
using System.Collections;
using UnityEngine;

/* CURVE_POINT_3D
 *
 * For controlling a Bezier curve. Has Tangents.
 */

namespace AXGeometry
{


    /// <summary>
    /// PathPoint3.

    // A PathPoint3 has a poistion, tangent, velocity, and matrix for transorming section splines.
    /// </summary>
    [System.Serializable]
    public class CurvePoint3
    {
               
        public Vector3 position;
        public Quaternion rotation;

        public Vector3 tangent;
        public Vector3 normal;

        public float u = 0f;

        // Angle between segements that meet at this point.
        public float angle = 180f;
       
        public CurvePoint3()
        {
            position = Vector3.zero;
        }
        public CurvePoint3(Vector3 pos)
        {
            position = pos;
        }
        public CurvePoint3(Vector3 pos, Quaternion rot)
        {
            position = pos;
            rotation = rot;
        }



        public CurvePoint3(Vector3 pos, Vector3 tng, Vector3 up)
        {
            position = pos;

            tangent = tng;

            // NORMAL
            Vector3 binormal = Vector3.Cross(up, tangent).normalized;
            normal = Vector3.Cross(tangent, binormal).normalized;

            // ORIENTATION
            rotation = Quaternion.LookRotation(tangent, normal);

        }





        public Vector3 LocalToWorld(Vector3 point)
        { 
            return position + rotation * point;
        }

        public Vector3 WorldToLocal(Vector3 point)
        {
            return Quaternion.Inverse(rotation) * (point - position);
        }

        public Vector3 LocalToWorldDirection(Vector3 dir)
        {
            return rotation * dir;
        }







    }

}
