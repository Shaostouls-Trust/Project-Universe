using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace ProjectUniverse.PowerSystem
{
    public class Template : MonoBehaviour
    {
        [Header("Template Configuration")]
        public string templateId;
        public TemplateType templateType;

        [Header("Waypoint Paths")]
        public List<WaypointPath> waypointPaths = new();

        [Header("Capacity Settings")]
        public int maxCableCapacity = 10;
        public int currentCableCount = 0;

        [Header("Visualization")]
        public bool showGizmos = true;
        public Color entryPointColor = Color.green;
        public Color exitPointColor = Color.red;
        public Color waypointColor = Color.yellow;
        public Color pathColor = Color.cyan;
        public Color assignedPathColor = new(0, 0.8f, 0.2f);
        [HideInInspector]
        public WaypointPath selectedPathInEditor;

        public enum TemplateType
        {
            DedicatedCableRoom,  // All cable sizes
            CeilingConduit,      // Distribution and Branch only
            WallConduit,         // Branch only
            FloorCrawlSpace      // Distribution and Branch with length restrictions
        }

        public void SetSelectedPath(WaypointPath path)
        {
            selectedPathInEditor = path;
        }

        public void ClearSelectedPath()
        {
            selectedPathInEditor = null;
        }

        public CableSize[] GetSupportedCableSizes()
        {
            return templateType switch
            {
                TemplateType.DedicatedCableRoom => new[] { CableSize.Transmission, CableSize.Distribution, CableSize.Branch },
                TemplateType.CeilingConduit or TemplateType.FloorCrawlSpace => new[] { CableSize.Distribution, CableSize.Branch },
                TemplateType.WallConduit => new[] { CableSize.Branch },
                _ => new CableSize[0],
            };
        }

        private void Awake()
        {
            // Ensure template has an ID
            if (string.IsNullOrEmpty(templateId))
            {
                templateId = gameObject.name + "_" + System.Guid.NewGuid().ToString()[..8];
            }
        }

        // Call this when template type changes to update all paths
        public void UpdatePathSupportedCableSizes()
        {
            CableSize[] supportedSizes = GetSupportedCableSizes();

            foreach (var path in waypointPaths)
            {
                // If a path has a cable size assigned, only update if that size is no longer supported
                //A
                if (path.HasAssignedCableSize)
                {
                    if (!supportedSizes.Contains(path.assignedCableSize.Value))
                    {
                        path.UnassignCableSize();
                        path.supportedCableSizes = supportedSizes;
                    }
                }
                // original
                else
                {
                    path.supportedCableSizes = supportedSizes;
                }
            }
        }

        public List<ConnectionPoint> GetAllConnectionPoints()
        {
            List<ConnectionPoint> allPoints = new();

            foreach (var path in waypointPaths)
            {
                allPoints.AddRange(path.GetAllPoints());
            }

            return allPoints;
        }

        public List<ConnectionPoint> GetEntryPoints()
        {
            List<ConnectionPoint> entryPoints = new();

            foreach (var path in waypointPaths)
            {
                entryPoints.Add(path.GetEntryPoint());
            }

            return entryPoints;
        }

        public List<ConnectionPoint> GetExitPoints()
        {
            List<ConnectionPoint> exitPoints = new();

            foreach (var path in waypointPaths)
            {
                exitPoints.Add(path.GetExitPoint());
            }

            return exitPoints;
        }

        public WaypointPath GetPathById(string pathId)
        {
            return waypointPaths.Find(p => p.pathId == pathId);
        }

        public bool CanAcceptCable(CableSize size)
        {
            if (currentCableCount >= maxCableCapacity)
                return false;

            return GetSupportedCableSizes().Contains(size);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!showGizmos) return;

            foreach (var path in waypointPaths)
            {
                bool isSelected = (selectedPathInEditor != null && selectedPathInEditor == path);
                Color pathGizmoColor = isSelected ? Color.white : (path.HasAssignedCableSize ? assignedPathColor : pathColor);//path.assignedCableSize.HasValue

                // Entry point
                Gizmos.color = entryPointColor;
                Vector3 entryWorldPos = transform.TransformPoint(path.entryPoint.position);
                Gizmos.DrawSphere(entryWorldPos, 0.1f);

                // Exit point
                Gizmos.color = exitPointColor;
                Vector3 exitWorldPos = transform.TransformPoint(path.exitPoint.position);
                Gizmos.DrawSphere(exitWorldPos, 0.1f);

                // Draw path
                Gizmos.color = pathGizmoColor;

                if (path.waypoints.Count == 0)
                {
                    // Direct line from entry to exit
                    Gizmos.DrawLine(entryWorldPos, exitWorldPos);
                }
                else
                {
                    // Draw from entry to first waypoint
                    Vector3 firstWaypointPos = transform.TransformPoint(path.waypoints[0].position);
                    Gizmos.DrawLine(entryWorldPos, firstWaypointPos);

                    // Draw between waypoints
                    for (int i = 0; i < path.waypoints.Count - 1; i++)
                    {
                        Vector3 currentPos = transform.TransformPoint(path.waypoints[i].position);
                        Vector3 nextPos = transform.TransformPoint(path.waypoints[i + 1].position);
                        Gizmos.DrawLine(currentPos, nextPos);
                    }

                    // Draw from last waypoint to exit
                    Vector3 lastWaypointPos = transform.TransformPoint(path.waypoints[^1].position);
                    Gizmos.DrawLine(lastWaypointPos, exitWorldPos);

                    // Draw waypoints
                    Gizmos.color = waypointColor;
                    foreach (var waypoint in path.waypoints)
                    {
                        Vector3 waypointWorldPos = transform.TransformPoint(waypoint.position);
                        Gizmos.DrawSphere(waypointWorldPos, 0.05f);
                    }
                }
            }
        }
#endif
    }
}