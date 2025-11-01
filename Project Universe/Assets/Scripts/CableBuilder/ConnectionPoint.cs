using UnityEngine;
using System;

namespace ProjectUniverse.PowerSystem
{
    public enum CableSize
    {
        Transmission, // Large (500mm)
        Distribution, // Medium (250mm)
        Branch        // Small (150mm)
    }

    [Serializable]
    public class ConnectionPoint
    {
        public string id;
        public Vector3 position;

        // Constructor for entry/exit points
        public ConnectionPoint(string id, Vector3 position)
        {
            this.id = id;
            this.position = position;
        }

        // Constructor for waypoints
        public static ConnectionPoint CreateWaypoint(string id, Vector3 position)
        {
            return new ConnectionPoint(id, position);
        }
    }
}