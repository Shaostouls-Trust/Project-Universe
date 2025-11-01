using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.Networking.Transport;
using ProjectUniverse.PowerSystem.CollisionDemo;

namespace ProjectUniverse.PowerSystem.Collision
{
    public class CableSpatialIndexDemo : MonoBehaviour
    {
        [Header("Room Setup")]
        public Transform[] demoRooms; // Assign GameObjects with BoxColliders

        [Header("Demo Cables")]
        [SerializeField] private Transform[] cableStartPoints;
        [SerializeField] private Transform[] cableEndPoints;

        [Header("Auto Discovery")]
        public bool useAutoDiscovery = true;

        [Header("Query Test")]
        public Transform queryPoint;
        public float queryRadius = 2f;

        [Header("Boundary Settings")]
        public float boundaryOverlap = 0.5f; // How much to extend room bounds for boundary cables

        [Header("Debug")]
        public bool showSpatialCells = true;
        public bool showQueryResults = true;
        public bool showAutoDiscoveredCables = true;

        protected Dictionary<Transform, RoomSpatialGrid> roomGrids = new Dictionary<Transform, RoomSpatialGrid>();
        private List<DemoCableSegment> allSegments = new List<DemoCableSegment>();
        private List<DemoCableSegment> lastQueryResults = new List<DemoCableSegment>();

        // Store the discovered waypoint endpoints
        private List<Transform> discoveredStartPoints = new List<Transform>();
        private List<Transform> discoveredEndPoints = new List<Transform>();
        private int autoDiscoveredCableCount = 0;

        // Boundary cable tracking
        private Dictionary<(int, int), List<Transform>> segmentRoomMembership = new Dictionary<(int, int), List<Transform>>();

        void Start()
        {
            InitializeRoomGrids();

            if (useAutoDiscovery)
            {
                DiscoverWaypointCables();
            }

            AddDemoCables();
        }

        public Transform[] CableStartPoints { get { return cableStartPoints; } }
        public Transform[] CableEndPoints { get { return cableEndPoints; } }

        void InitializeRoomGrids()
        {
            foreach (var room in demoRooms)
            {
                var boxCollider = room.GetComponent<BoxCollider>();
                if (boxCollider != null)
                {
                    // Create bounds in world space
                    Bounds worldBounds = new Bounds(
                        room.TransformPoint(boxCollider.center),
                        Vector3.Scale(boxCollider.size, room.lossyScale)
                    );

                    roomGrids[room] = new RoomSpatialGrid(worldBounds);
                    Debug.Log($"Initialized grid for room: {room.name}, bounds: {worldBounds}");
                }
            }
        }

        /// <summary>
        /// NEW: Discover waypoint cables and convert them to start/end point pairs
        /// </summary>
        void DiscoverWaypointCables()
        {
            discoveredStartPoints.Clear();
            discoveredEndPoints.Clear();
            autoDiscoveredCableCount = 0;

            foreach (var room in demoRooms)
            {
                var roomNetwork = room.GetComponent<RoomNetwork>();
                if (roomNetwork == null) continue;

                // Get all templates in this room
                var templates = roomNetwork.templates;
                if (templates == null || templates.Count == 0)
                {
                    templates = room.GetComponentsInChildren<Template>().ToList();
                }

                Debug.Log($"Processing room {room.name} with {templates.Count} templates");

                foreach (var template in templates)
                {
                    if (template.waypointPaths == null) continue;

                    foreach (var waypointPath in template.waypointPaths)
                    {
                        ConvertWaypointPathToSegments(waypointPath, template);
                    }
                }

                // Process intermediate connections
                ConvertTemplateConnectionsToSegments(roomNetwork);

                // Process boundary connections
                ConvertBoundaryConnectionsToSegments(roomNetwork, room);
            }

            Debug.Log($"Auto-discovered {autoDiscoveredCableCount} cable segments from waypoint paths");
        }

        /// <summary>
        /// NEW: Convert waypoint path to start/end point pairs
        /// </summary>
        void ConvertWaypointPathToSegments(WaypointPath waypointPath, Template template)
        {
            var pathPositions = waypointPath.GetPathPositions();
            if (pathPositions.Count < 2) return;

            for (int i = 0; i < pathPositions.Count - 1; i++)
            {
                Vector3 localStart = pathPositions[i];
                Vector3 localEnd = pathPositions[i + 1];

                // Convert to world positions
                Vector3 worldStart = template.transform.TransformPoint(localStart);
                Vector3 worldEnd = template.transform.TransformPoint(localEnd);

                // Create transforms for start and end points
                GameObject startGO = new GameObject($"AutoStart_{autoDiscoveredCableCount}_{i}");
                startGO.transform.position = worldStart;
                startGO.hideFlags = HideFlags.HideInHierarchy;

                GameObject endGO = new GameObject($"AutoEnd_{autoDiscoveredCableCount}_{i}");
                endGO.transform.position = worldEnd;
                endGO.hideFlags = HideFlags.HideInHierarchy;

                discoveredStartPoints.Add(startGO.transform);
                discoveredEndPoints.Add(endGO.transform);

                Debug.Log($"Added waypoint segment: {worldStart} -> {worldEnd}");
            }

            autoDiscoveredCableCount++;
        }

        /// <summary>
        /// NEW: Convert template connections to start/end point pairs
        /// </summary>
        void ConvertTemplateConnectionsToSegments(RoomNetwork roomNetwork)
        {
            if (roomNetwork.connections == null) return;

            foreach (var connection in roomNetwork.connections)
            {
                if (!connection.isSizeCompatible) continue;

                Vector3 sourceWorldPos = connection.GetSourceWorldPosition();
                Vector3 targetWorldPos = connection.GetTargetWorldPosition();

                // Create transforms for connection points
                GameObject startGO = new GameObject($"ConnStart_{autoDiscoveredCableCount}");
                startGO.transform.position = sourceWorldPos;
                startGO.hideFlags = HideFlags.HideInHierarchy;

                GameObject endGO = new GameObject($"ConnEnd_{autoDiscoveredCableCount}");
                endGO.transform.position = targetWorldPos;
                endGO.hideFlags = HideFlags.HideInHierarchy;

                discoveredStartPoints.Add(startGO.transform);
                discoveredEndPoints.Add(endGO.transform);

                autoDiscoveredCableCount++;
                Debug.Log($"Added template connection: {sourceWorldPos} -> {targetWorldPos}");
            }
        }

        /// <summary>
        /// NEW: Convert boundary connections to start/end point pairs
        /// </summary>
        void ConvertBoundaryConnectionsToSegments(RoomNetwork roomNetwork, Transform room)
        {
            if (roomNetwork.boundaryPorts == null) return;

            foreach (var boundaryPort in roomNetwork.boundaryPorts)
            {
                if (!boundaryPort.IsConnected()) continue;

                foreach (var otherRoom in demoRooms)
                {
                    if (otherRoom == room) continue;

                    var otherRoomNetwork = otherRoom.GetComponent<RoomNetwork>();
                    if (otherRoomNetwork?.boundaryPorts == null) continue;

                    foreach (var otherBoundaryPort in otherRoomNetwork.boundaryPorts)
                    {
                        if (!otherBoundaryPort.IsConnected()) continue;

                        float distance = Vector3.Distance(
                            boundaryPort.GetWorldPosition(),
                            otherBoundaryPort.GetWorldPosition()
                        );

                        if (distance <= boundaryOverlap)
                        {
                            Vector3 start = boundaryPort.GetWorldPosition();
                            Vector3 end = otherBoundaryPort.GetWorldPosition();

                            // Create transforms for boundary connection points
                            GameObject startGO = new GameObject($"BoundStart_{autoDiscoveredCableCount}");
                            startGO.transform.position = start;
                            startGO.hideFlags = HideFlags.HideInHierarchy;

                            GameObject endGO = new GameObject($"BoundEnd_{autoDiscoveredCableCount}");
                            endGO.transform.position = end;
                            endGO.hideFlags = HideFlags.HideInHierarchy;

                            discoveredStartPoints.Add(startGO.transform);
                            discoveredEndPoints.Add(endGO.transform);

                            autoDiscoveredCableCount++;
                            Debug.Log($"Added boundary connection: {start} -> {end}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// MODIFIED: Now includes discovered waypoint cables using the original logic
        /// </summary>
        void AddDemoCables()
        {
            int cableId = 0;

            // Combine manual cables with discovered cables
            var allStartPoints = new List<Transform>();
            var allEndPoints = new List<Transform>();

            // Add manual cables
            if (cableStartPoints != null) allStartPoints.AddRange(cableStartPoints);
            if (cableEndPoints != null) allEndPoints.AddRange(cableEndPoints);

            // Add discovered cables
            allStartPoints.AddRange(discoveredStartPoints);
            allEndPoints.AddRange(discoveredEndPoints);

            // Use the ORIGINAL logic that we know works
            for (int i = 0; i < Mathf.Min(allStartPoints.Count, allEndPoints.Count); i++)
            {
                Vector3 start = allStartPoints[i].position;
                Vector3 end = allEndPoints[i].position;

                var segment = new DemoCableSegment(start, end, 0.1f, cableId, 0);
                allSegments.Add(segment);

                // Add to appropriate room grids - ORIGINAL METHOD
                AddSegmentToRooms(segment);

                cableId++;
            }

            Debug.Log($"Added {allSegments.Count} total cable segments ({cableStartPoints?.Length ?? 0} manual + {autoDiscoveredCableCount} discovered)");
        }

        // ORIGINAL METHOD - unchanged
        void AddSegmentToRooms(DemoCableSegment segment)
        {
            foreach (var kvp in roomGrids)
            {
                var room = kvp.Key;
                var grid = kvp.Value;

                // Check if segment intersects room bounds
                var boxCollider = room.GetComponent<BoxCollider>();
                Bounds roomBounds = new Bounds(
                    room.TransformPoint(boxCollider.center),
                    Vector3.Scale(boxCollider.size, room.lossyScale)
                );

                if (DoesSegmentIntersectBounds(segment, roomBounds))
                {
                    grid.AddCableSegment(segment);
                    Debug.Log($"Added segment {segment.cableId} to room {room.name}");
                }
            }
        }

        protected bool DoesSegmentIntersectBounds(DemoCableSegment segment, Bounds bounds)
        {
            if (bounds.Contains(segment.start) || bounds.Contains(segment.end))
                return true;

            return bounds.Intersects(segment.GetBounds());
        }

        void Update()
        {
            if (queryPoint != null)
            {
                QueryAtPoint(queryPoint.position, queryRadius);
            }
        }

        [ContextMenu("Query At Test Point")]
        void QueryAtPoint(Vector3 position, float radius)
        {
            lastQueryResults.Clear();
            var seenSegments = new HashSet<(int, int)>(); // Track (cableId, segmentIndex) pairs

            foreach (var kvp in roomGrids)
            {
                var grid = kvp.Value;
                var results = grid.QueryRadius(position, radius);

                foreach (var segment in results)
                {
                    var key = (segment.cableId, segment.segmentIndex);
                    // Only add if we haven't seen this exact segment before
                    if (!seenSegments.Contains(key))
                    {
                        lastQueryResults.Add(segment);
                        seenSegments.Add(key);
                    }
                }
            }

            //Debug.Log($"Query found {lastQueryResults.Count} cable segments within {radius} units of {position}");
            //foreach (var result in lastQueryResults)
            //{
            //    Debug.Log($"Found segment, id: {result.cableId}-{result.segmentIndex}");
            //}
        }

        [ContextMenu("Rebuild All Grids")]
        void RebuildGrids()
        {
            foreach (var grid in roomGrids.Values)
            {
                grid.Clear();
            }

            allSegments.Clear();

            // Clean up old discovered transforms
            foreach (var t in discoveredStartPoints)
                if (t != null) DestroyImmediate(t.gameObject);
            foreach (var t in discoveredEndPoints)
                if (t != null) DestroyImmediate(t.gameObject);

            discoveredStartPoints.Clear();
            discoveredEndPoints.Clear();

            if (useAutoDiscovery)
            {
                DiscoverWaypointCables();
            }

            AddDemoCables();
        }

        public List<DemoCableSegment> QueryRadiusAllRooms(Vector3 position, float radius)
        {
            var results = new List<DemoCableSegment>();
            var checkedSegments = new HashSet<(int, int)>();

            foreach (var grid in roomGrids.Values)
            {
                var roomResults = grid.QueryRadius(position, radius);
                foreach (var segment in roomResults)
                {
                    var key = (segment.cableId, segment.segmentIndex);
                    if (!checkedSegments.Contains(key))
                    {
                        checkedSegments.Add(key);
                        results.Add(segment);
                    }
                }
            }

            return results;
        }

        void OnDrawGizmos()
        {
            if (showSpatialCells && roomGrids != null)
            {
                foreach (var grid in roomGrids.Values)
                {
                    grid.DebugDrawCells();
                }
            }

            if (showQueryResults && queryPoint != null)
            {
                // Draw query sphere
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(queryPoint.position, queryRadius);

                // Draw found segments
                Gizmos.color = Color.green;
                foreach (var segment in lastQueryResults)
                {
                    Gizmos.DrawLine(segment.start, segment.end);
                    Gizmos.DrawWireSphere(segment.start, segment.radius);
                    Gizmos.DrawWireSphere(segment.end, segment.radius);
                }
            }

            // Draw all demo cables
            Gizmos.color = Color.blue;
            foreach (var segment in allSegments)
            {
                Gizmos.DrawLine(segment.start, segment.end);
            }

            // Draw auto-discovered cables in different color
            if (showAutoDiscoveredCables)
            {
                Gizmos.color = Color.cyan;
                for (int i = 0; i < discoveredStartPoints.Count; i++)
                {
                    if (discoveredStartPoints[i] != null && discoveredEndPoints[i] != null)
                    {
                        Gizmos.DrawLine(discoveredStartPoints[i].position, discoveredEndPoints[i].position);
                    }
                }
            }
        }

    }
}