using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public static class DuctUtilities
{
    [Serializable]
    public class DuctPathInfo
    {
        public List<GameObject> DuctObjects { get; set; } = new List<GameObject>();
        public bool IsVertical { get; set; }
    }

    // Directions for checking duct connections
    public static readonly Vector3[] Directions = {
        Vector3.right, Vector3.left,
        Vector3.up, Vector3.down,
        Vector3.forward, Vector3.back
    };

    public static bool AreDuctsConnected(GameObject duct1, GameObject duct2)
    {
        Vector3 connectionVector = duct2.transform.position - duct1.transform.position;
        float distance = connectionVector.magnitude;

        // Ducts must be within max distance and along cardinal direction
        return distance <= 1.5f &&
               (Mathf.Abs(connectionVector.normalized.x) > 0.9f ||
                Mathf.Abs(connectionVector.normalized.y) > 0.9f ||
                Mathf.Abs(connectionVector.normalized.z) > 0.9f);
    }

    public static List<List<GameObject>> GroupDuctsIntoSegments(List<GameObject> ducts)
    {
        List<List<GameObject>> segments = new();
        HashSet<GameObject> processed = new();

        foreach (GameObject duct in ducts.Where(d => d != null && !processed.Contains(d)))
        {
            var segment = new List<GameObject>();
            var toProcess = new Queue<GameObject>();
            toProcess.Enqueue(duct);

            while (toProcess.Count > 0)
            {
                GameObject current = toProcess.Dequeue();
                if (processed.Contains(current)) continue;

                processed.Add(current);
                segment.Add(current);

                // Find connected ducts
                foreach (GameObject other in ducts.Where(d => d != null && !processed.Contains(d)))
                {
                    if (AreDuctsConnected(current, other))
                    {
                        toProcess.Enqueue(other);
                    }
                }
            }

            if (segment.Count > 0)
            {
                segments.Add(segment);
            }
        }

        return segments;
    }

    public static bool HasDuctInDirection(Vector3 position, Vector3 direction, List<GameObject> allDucts)
    {
        Vector3 checkPosition = position + direction;
        return allDucts.Any(duct => duct != null && Vector3.Distance(duct.transform.position, checkPosition) < 0.1f);
    }

    public static int GetDuctTypeIndex(bool[] connections)
    {
        int horizontalCount = (connections[0] ? 1 : 0) + (connections[1] ? 1 : 0) +
                              (connections[4] ? 1 : 0) + (connections[5] ? 1 : 0);
        bool hasUp = connections[2];
        bool hasDown = connections[3];

        // Vertical only
        if (horizontalCount == 0 && (hasUp || hasDown))
            return 4; // vertical

        // Vertical with horizontal
        if (horizontalCount > 0 && (hasUp || hasDown))
            return hasUp && !hasDown ? 6 : 5; // straight with closed bottom or vertical with straight

        // Horizontal only
        return horizontalCount switch
        {
            2 => (connections[0] && connections[1]) || (connections[4] && connections[5]) ? 0 : 1, // straight or corner
            3 => 2, // 3-way
            4 => 3, // 4-way
            _ => 0  // default to straight
        };
    }

    public static float GetDuctRotation(bool[] connections, int ductType)
    {
        return ductType switch
        {
            0 => connections[0] || connections[1] ? 90f : 0f, // straight
            1 => GetCornerDuctRotation(connections), // corner
            2 => Get3WayDuctRotation(connections), // 3-way
            6 => connections[0] || connections[1] ? 90f : 0f, // straight with closed bottom
            _ => 0f // 4-way, vertical, vertical with straight
        };
    }

    private static float GetCornerDuctRotation(bool[] connections)
    {
        if (connections[0] && connections[4]) return 0f;
        if (connections[0] && connections[5]) return 90f;
        if (connections[1] && connections[5]) return 180f;
        if (connections[1] && connections[4]) return 270f;
        return 0f;
    }

    private static float Get3WayDuctRotation(bool[] connections)
    {
        if (!connections[0]) return 90f;
        if (!connections[1]) return 270f;
        if (!connections[4]) return 180f;
        if (!connections[5]) return 0f;
        return 0f;
    }

    public static float GetDuctLength(GameObject duct)
    {
        //string ductName = duct.name.Replace("(Clone)", "").Trim();
        // All duct types default to 1m for now
        return 1f;
    }

    public static Vector3 GetNearestConnectionPoint(Vector3 position, GameObject[] gasPipeLinks, float maxDistance = 2f)
    {
        if (gasPipeLinks.Length == 0) return Vector3.zero;

        var nearest = gasPipeLinks
            .Where(link => link != null)
            .Select(link => (link.transform.position, Vector3.Distance(position, link.transform.position)))
            .OrderBy(tuple => tuple.Item2)
            .FirstOrDefault();

        return nearest.Item2 < maxDistance ? nearest.Item1 : Vector3.zero;
    }
}