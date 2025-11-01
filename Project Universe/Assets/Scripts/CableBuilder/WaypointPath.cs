using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

namespace ProjectUniverse.PowerSystem
{
    [System.Serializable]
    public class WaypointPath
    {
        public string pathId;
        public ConnectionPoint entryPoint;
        public ConnectionPoint exitPoint;
        public List<ConnectionPoint> waypoints = new();
        public CableSize[] supportedCableSizes;
        private CableSize? _assignedCableSize = null; // Will be set when a cable is created

        public WaypointPath(string pathId, CableSize[] supportedSizes)
        {
            this.pathId = pathId;
            this.supportedCableSizes = supportedSizes;

            // Create default entry and exit points
            this.entryPoint = new ConnectionPoint(
                pathId + "_entry",
                Vector3.zero
            );

            this.exitPoint = new ConnectionPoint(
                pathId + "_exit",
                new Vector3(1, 0, 0)
            );
        }

        public void AddWaypoint(Vector3 position)
        {
            string waypointId = pathId + "_wp_" + waypoints.Count;
            ConnectionPoint waypoint = ConnectionPoint.CreateWaypoint(waypointId, position);
            waypoints.Add(waypoint);
        }

        public void MoveWaypointUp(int index)
        {
            if (index <= 0 || index >= waypoints.Count) return;

            (waypoints[index - 1], waypoints[index]) = (waypoints[index], waypoints[index - 1]);
        }

        public void MoveWaypointDown(int index)
        {
            if (index < 0 || index >= waypoints.Count - 1) return;

            (waypoints[index + 1], waypoints[index]) = (waypoints[index], waypoints[index + 1]);
        }

        public CableSize? assignedCableSize
        {
            get
            {
                if(_assignedCableSize == null)
                {
                    if (CableAssignmentManager.Instance.TryGetAssignedCableSize(pathId, out CableSize size))
                    {
                        _assignedCableSize = size;
                        return size;
                    }
                    return null;
                }
                return _assignedCableSize;
            }
        }
        public bool HasAssignedCableSize => _assignedCableSize.HasValue;
        public bool CanSupportCableSize(CableSize size)
        {
            var assignedSize = _assignedCableSize;
            if (assignedSize.HasValue)
                return assignedSize.Value == size;

            return System.Array.Exists(supportedCableSizes, s => s == size);
        }

        public void AssignCableSize(CableSize size)
        {
            if (CanSupportCableSize(size))
            {
                CableAssignmentManager.Instance.AssignCableSize(pathId, size);
                _assignedCableSize = size;
            }
        }

        public void UnassignCableSize()
        {
            CableAssignmentManager.Instance.UnassignCableSize(pathId);
            _assignedCableSize = null;
        }

        public ConnectionPoint GetEntryPoint()
        {
            return entryPoint;
        }

        public ConnectionPoint GetExitPoint()
        {
            return exitPoint;
        }

        public List<ConnectionPoint> GetAllPoints()
        {
            List<ConnectionPoint> allPoints = new()
            {
                entryPoint
            };
            allPoints.AddRange(waypoints);
            allPoints.Add(exitPoint);
            return allPoints;
        }

        public List<Vector3> GetPathPositions()
        {
            List<Vector3> positions = new()
            {
                entryPoint.position
            };

            foreach (var waypoint in waypoints)
            {
                positions.Add(waypoint.position);
            }

            positions.Add(exitPoint.position);
            return positions;
        }
    }
}