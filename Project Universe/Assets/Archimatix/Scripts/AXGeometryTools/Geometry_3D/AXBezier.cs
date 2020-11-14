using UnityEngine;


namespace AXGeometry
{

    public static class AXBezier
    {

        public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            t = Mathf.Clamp01(t);
            float oneMinusT = 1f - t;
            return
                oneMinusT * oneMinusT * p0 +
                2f * oneMinusT * t * p1 +
                t * t * p2;
        }

        public static Vector3 GetFirstDerivative(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            return
                2f * (1f - t) * (p1 - p0) +
                2f * t * (p2 - p1);
        }

        public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            t = Mathf.Clamp01(t);
            float OneMinusT = 1f - t;
            return
                OneMinusT * OneMinusT * OneMinusT * p0 +
                3f * OneMinusT * OneMinusT * t * p1 +
                3f * OneMinusT * t * t * p2 +
                t * t * t * p3;
        }

        public static Vector3 GetFirstDerivative(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            t = Mathf.Clamp01(t);
            float oneMinusT = 1f - t;
            return
                3f * oneMinusT * oneMinusT * (p1 - p0) +
                6f * oneMinusT * t * (p2 - p1) +
                3f * t * t * (p3 - p2);
        }

        public static Vector3 GetTangent(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            float omt = 1f - t;
            float omt2 = omt * omt;
            float t2 = t * t;


            Vector3 tangent =
                p0 * (-omt2) +
                p1 * ( (3 * omt2) - (2 * omt) + (-3 * t2) + (2 * t)) +
                p2 * (t2);

            return tangent;
        }

        public static Vector3 GetTangent(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float omt = 1f - t;
            float omt2 = omt * omt;
            float t2 = t * t;


            Vector3 tangent =
                p0 * (-omt2) +
                p1 * (3 * omt2 - 2 * omt) +
                p2 * (-3 * t2 + 2 * t) +
                p3 * (t2);

            return tangent;
        }

        public static Vector3 GetNormal3D(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t, Vector3 up)
        {
            Vector3 tng = GetTangent(p0, p1, p2, p3, t);
            Vector3 binormal = Vector3.Cross(up, tng).normalized;

            return Vector3.Cross(tng, binormal).normalized;



        }

        public static Quaternion GetOrientation(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t, Vector3 up)
        {
            Vector3 tng = GetTangent(p0, p1, p2, p3, t);
            Vector3 normal = GetNormal3D(p0, p1, p2, p3, t, Vector3.up);

            return Quaternion.LookRotation(tng, normal);

        }

        public static CurvePoint3 GetPathPoint3(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t, Vector3 up)
        {

            // POSITION
            Vector3 position = GetPoint(p0, p1, p2, p3, t);

            // TANGENT
            Vector3 tangent = GetTangent(p0, p1, p2, p3, t);

            return new CurvePoint3(position, tangent, up);



        }
    }
}
