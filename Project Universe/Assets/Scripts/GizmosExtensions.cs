using UnityEngine;

public static class GizmosExtensions
{
    public static void DrawCone(Vector3 position, Vector3 direction, float radius, float height)
    {
        int segments = 8;
        Vector3 forward = direction.normalized;
        Vector3 up = Vector3.up;
        Vector3 right = Vector3.right;

        // Find perpendicular vectors
        if (Mathf.Abs(Vector3.Dot(forward, Vector3.up)) > 0.9f)
        {
            right = Vector3.Cross(forward, Vector3.forward).normalized;
        }
        else
        {
            right = Vector3.Cross(forward, Vector3.up).normalized;
        }
        up = Vector3.Cross(right, forward).normalized;

        Vector3 tip = position;
        Vector3 baseCenter = position - forward * height;

        for (int i = 0; i < segments; i++)
        {
            float angle1 = (float)i / segments * 2 * Mathf.PI;
            float angle2 = (float)(i + 1) / segments * 2 * Mathf.PI;

            Vector3 point1 = baseCenter + (right * Mathf.Cos(angle1) + up * Mathf.Sin(angle1)) * radius;
            Vector3 point2 = baseCenter + (right * Mathf.Cos(angle2) + up * Mathf.Sin(angle2)) * radius;

            // Draw base circle segment
            Gizmos.DrawLine(point1, point2);

            // Draw lines to tip
            Gizmos.DrawLine(point1, tip);
        }
    }
}
