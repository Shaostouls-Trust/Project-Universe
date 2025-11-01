using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using static ProjectUniverse.PowerSystem.GlobalRouteResolver;

namespace ProjectUniverse.PowerSystem
{
    public class RoomNetwork : MonoBehaviour
    {
        [Header("Network Configuration")]
        public string roomId;
        public float connectionThreshold = 0.1f; // How close entry/exit points need to be to connect
        public bool autoDiscoverTemplates = true;

        [Header("Templates")]
        public List<Template> templates = new();

        [Header("Visualization")]
        public bool showConnectionGizmos = true;
        public Color compatibleConnectionColor = Color.green;
        public Color incompatibleConnectionColor = Color.yellow;
        public Color entryPointColor = Color.blue;
        public Color exitPointColor = new(1f, 0.5f, 0f); // Orange
        public Color selectedPathColor = Color.white;

        [Header("Nodes")]
        public List<PowerNode> nodes = new();

        [Header("Boundary Ports")]
        public List<NetworkBoundaryPort> boundaryPorts = new();

        // Connection structure
        [System.Serializable]
        public class TemplateConnection
        {
            public Template sourceTemplate;
            public WaypointPath sourcePath;
            public ConnectionPoint sourcePoint;
            public bool isSourceEntryPoint;
            public Template targetTemplate;
            public WaypointPath targetPath;
            public ConnectionPoint targetPoint;
            public bool isTargetEntryPoint;
            public bool isSizeCompatible;

            public TemplateConnection(Template source, WaypointPath sPath, ConnectionPoint sPoint, bool isSourceEntry,
                                    Template target, WaypointPath tPath, ConnectionPoint tPoint, bool isTargetEntry,
                                    bool compatible)
            {
                sourceTemplate = source;
                sourcePath = sPath;
                sourcePoint = sPoint;
                isSourceEntryPoint = isSourceEntry;
                targetTemplate = target;
                targetPath = tPath;
                targetPoint = tPoint;
                isTargetEntryPoint = isTargetEntry;
                isSizeCompatible = compatible;
            }

            public Vector3 GetSourceWorldPosition()
            {
                return sourceTemplate.transform.TransformPoint(sourcePoint.position);
            }

            public Vector3 GetTargetWorldPosition()
            {
                return targetTemplate.transform.TransformPoint(targetPoint.position);
            }
        }

        // Room endpoint structure
        [System.Serializable]
        public class RoomEndpoint
        {
            public Template template;
            public WaypointPath path;
            public ConnectionPoint point;
            public bool isEntryPoint;

            public RoomEndpoint(Template t, WaypointPath p, ConnectionPoint cp, bool isEntry)
            {
                template = t;
                path = p;
                point = cp;
                isEntryPoint = isEntry;
            }

            public Vector3 GetWorldPosition()
            {
                return template.transform.TransformPoint(point.position);
            }
        }

        // Room path structure
        [System.Serializable]
        public class RoomPath
        {
            public RoomEndpoint entry;
            public RoomEndpoint exit;
            public List<WaypointPath> pathSegments = new();
            public bool isSelected = false;

            public RoomPath(RoomEndpoint entryPoint, RoomEndpoint exitPoint)
            {
                entry = entryPoint;
                exit = exitPoint;
            }

            public void AddSegment(WaypointPath segment)
            {
                pathSegments.Add(segment);
            }

            public bool IsSizeCompatible()
            {
                if (pathSegments.Count == 0) return true;

                CableSize? firstSize = null;

                foreach (var segment in pathSegments)
                {
                    if (segment.assignedCableSize.HasValue)
                    {
                        if (!firstSize.HasValue)
                        {
                            firstSize = segment.assignedCableSize.Value;
                        }
                        else if (firstSize.Value != segment.assignedCableSize.Value)
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
        }

        // Connection data
        [HideInInspector] public List<TemplateConnection> connections = new();
        [HideInInspector] public List<RoomEndpoint> roomEndpoints = new();
        [HideInInspector] public List<RoomPath> roomPaths = new();

        public void DiscoverNodes()
        {
            nodes.Clear();
            nodes.AddRange(GetComponentsInChildren<PowerNode>());
        }

        public void DiscoverBoundaryPorts()
        {
            boundaryPorts.Clear();
            boundaryPorts.AddRange(GetComponentsInChildren<NetworkBoundaryPort>());
        }

        private void Start()
        {
            if (autoDiscoverTemplates)
            {
                DiscoverTemplates();
                DiscoverNodes();
            }

            RefreshConnections();

            // Update visualizations
            if (Application.isPlaying)
            {
                if (PowerSystemVisualizer.Instance != null)
                {
                    PowerSystemVisualizer.Instance.UpdateVisualization();
                }
            }
        }

        public void DiscoverTemplates()
        {
            // Store assigned cable sizes before clearing
            Dictionary<string, Dictionary<string, CableSize?>> templateCableSizes = new();

            foreach (var template in templates)
            {
                if (template == null) continue;

                Dictionary<string, CableSize?> pathSizes = new();
                foreach (var path in template.waypointPaths)
                {
                    pathSizes[path.pathId] = path.assignedCableSize;
                }
                templateCableSizes[template.templateId] = pathSizes;
            }

            templates.Clear();
            templates.AddRange(GetComponentsInChildren<Template>());

            // Restore assigned cable sizes
            foreach (var template in templates)
            {
                if (template == null) continue;

                if (templateCableSizes.ContainsKey(template.templateId))
                {
                    var pathSizes = templateCableSizes[template.templateId];
                    foreach (var path in template.waypointPaths)
                    {
                        if (pathSizes.ContainsKey(path.pathId) && pathSizes[path.pathId].HasValue)
                        {
                            path.AssignCableSize(pathSizes[path.pathId].Value);
                        }
                    }
                }
            }
        }

        public void RefreshConnections()
        {
            // Store assigned cable sizes before clearing
            Dictionary<string, Dictionary<string, CableSize?>> templateCableSizes = new();

            foreach (var template in templates)
            {
                if (template == null) continue;

                Dictionary<string, CableSize?> pathSizes = new();
                foreach (var path in template.waypointPaths)
                {
                    pathSizes[path.pathId] = path.assignedCableSize;
                }
                templateCableSizes[template.templateId] = pathSizes;
            }

            // Clear existing connection data only (not templates)
            connections.Clear();
            roomEndpoints.Clear();
            roomPaths.Clear();

            // Clear node connections
            foreach (var node in nodes)
            {
                if (node != null)
                    node.ClearConnections();
            }

            // Ensure we have templates and nodes
            if (templates.Count == 0 || templates.Any(t => t == null))
            {
                DiscoverTemplates();
            }

            if (nodes.Count == 0 || nodes.Any(n => n == null))
            {
                DiscoverNodes();
            }

            // Ensure we have boundary ports
            if (boundaryPorts.Count == 0 || boundaryPorts.Any(p => p == null))
            {
                DiscoverBoundaryPorts();
            }

            // Restore assigned cable sizes
            foreach (var template in templates)
            {
                if (template == null) continue;

                if (templateCableSizes.ContainsKey(template.templateId))
                {
                    var pathSizes = templateCableSizes[template.templateId];
                    foreach (var path in template.waypointPaths)
                    {
                        if (pathSizes.ContainsKey(path.pathId) && pathSizes[path.pathId].HasValue)
                        {
                            path.AssignCableSize(pathSizes[path.pathId].Value);
                        }
                    }
                }
            }

            // Find connections between templates
            FindTemplateConnections();

            // Find connections to nodes
            FindNodeConnections();

            // Identify room entry/exit points  
            IdentifyRoomEndpoints();

            // Build room paths
            BuildRoomPaths();

            // Find connections to boundary ports
            FindBoundaryPortConnections();

            if (Application.isPlaying)
            {
                // Update visualizations
                if (PowerSystemVisualizer.Instance != null)
                {
                    PowerSystemVisualizer.Instance.UpdateVisualization();
                }
            }
        }

        public void RefreshConnections_()
        {
            // Store assigned cable sizes before clearing
            Dictionary<string, Dictionary<string, CableSize?>> templateCableSizes = new();

            foreach (var template in templates)
            {
                if (template == null) continue;

                Dictionary<string, CableSize?> pathSizes = new();
                foreach (var path in template.waypointPaths)
                {
                    pathSizes[path.pathId] = path.assignedCableSize;
                }
                templateCableSizes[template.templateId] = pathSizes;
            }

            // Clear existing connection data only (not templates)
            connections.Clear();
            roomEndpoints.Clear();
            roomPaths.Clear();

            // Clear node connections
            foreach (var node in nodes)
            {
                if (node != null)
                    node.ClearConnections();
            }

            // Ensure we have templates and nodes
            if (templates.Count == 0 || templates.Any(t => t == null))
            {
                DiscoverTemplates();
            }

            if (nodes.Count == 0 || nodes.Any(n => n == null))
            {
                DiscoverNodes();
            }

            // Ensure we have boundary ports
            if (boundaryPorts.Count == 0 || boundaryPorts.Any(p => p == null))
            {
                DiscoverBoundaryPorts();
            }

            // Restore assigned cable sizes
            foreach (var template in templates)
            {
                if (template == null) continue;

                if (templateCableSizes.ContainsKey(template.templateId))
                {
                    var pathSizes = templateCableSizes[template.templateId];
                    foreach (var path in template.waypointPaths)
                    {
                        if (pathSizes.ContainsKey(path.pathId) && pathSizes[path.pathId].HasValue)
                        {
                            path.AssignCableSize(pathSizes[path.pathId].Value);
                        }
                    }
                }
            }

            // Find connections between templates
            FindTemplateConnections();

            // Find connections to nodes
            FindNodeConnections();

            // Identify room entry/exit points  
            IdentifyRoomEndpoints();

            // Build room paths
            BuildRoomPaths();

            // Find connections to boundary ports
            FindBoundaryPortConnections();
        }

        private void FindTemplateConnections()
        {
            HashSet<string> processedConnections = new();

            // Check every template against every other template
            for (int i = 0; i < templates.Count; i++)
            {
                Template sourceTemplate = templates[i];
                if (sourceTemplate == null) continue;

                foreach (var sourcePath in sourceTemplate.waypointPaths)
                {
                    // Check exit points
                    ConnectionPoint sourceExit = sourcePath.GetExitPoint();
                    Vector3 sourceExitWorldPos = sourceTemplate.transform.TransformPoint(sourceExit.position);

                    // Look for matching entry points in other templates
                    for (int j = 0; j < templates.Count; j++)
                    {
                        if (i == j) continue;

                        Template targetTemplate = templates[j];
                        if (targetTemplate == null) continue;

                        foreach (var targetPath in targetTemplate.waypointPaths)
                        {
                            ConnectionPoint targetEntry = targetPath.GetEntryPoint();
                            Vector3 targetEntryWorldPos = targetTemplate.transform.TransformPoint(targetEntry.position);

                            if (Vector3.Distance(sourceExitWorldPos, targetEntryWorldPos) <= connectionThreshold)
                            {
                                // Create unique connection identifier to prevent duplicates
                                string connectionId = $"{sourceTemplate.templateId}_{sourcePath.pathId}_exit_to_{targetTemplate.templateId}_{targetPath.pathId}_entry";

                                if (!processedConnections.Contains(connectionId))
                                {
                                    processedConnections.Add(connectionId);

                                    bool compatible = CheckCableSizeCompatibility(sourcePath, targetPath);
                                    connections.Add(new TemplateConnection(
                                        sourceTemplate, sourcePath, sourceExit, false,
                                        targetTemplate, targetPath, targetEntry, true,
                                        compatible
                                    ));
                                }
                            }
                        }
                    }
                }
            }
        }

        private void FindNodeConnections()
        {
            foreach (var node in nodes)
            {
                if (node == null) continue;

                // Check templates for connections to this node
                foreach (var template in templates)
                {
                    if (template == null) continue;

                    foreach (var path in template.waypointPaths)
                    {
                        // Check if path exit connects to node input
                        ConnectionPoint pathExit = path.GetExitPoint();
                        Vector3 exitWorldPos = template.transform.TransformPoint(pathExit.position);

                        for (int i = 0; i < node.inputPoints.Count; i++)
                        {
                            Vector3 nodeInputWorldPos = node.transform.TransformPoint(node.inputPoints[i].position);

                            if (Vector3.Distance(exitWorldPos, nodeInputWorldPos) <= connectionThreshold)
                            {
                                var connection = new PowerNode.NodeConnection(path, template, node.inputPoints[i], true, i);
                                node.AddConnection(connection);
                            }
                        }

                        // Check if path entry connects to node output
                        ConnectionPoint pathEntry = path.GetEntryPoint();
                        Vector3 entryWorldPos = template.transform.TransformPoint(pathEntry.position);

                        for (int i = 0; i < node.outputPoints.Count; i++)
                        {
                            Vector3 nodeOutputWorldPos = node.transform.TransformPoint(node.outputPoints[i].position);

                            if (Vector3.Distance(entryWorldPos, nodeOutputWorldPos) <= connectionThreshold)
                            {
                                var connection = new PowerNode.NodeConnection(path, template, node.outputPoints[i], false, i);
                                node.AddConnection(connection);
                            }
                        }
                    }
                }
            }
        }

        private bool CheckCableSizeCompatibility(WaypointPath path1, WaypointPath path2)
        {
            if (path1.assignedCableSize.HasValue && path2.assignedCableSize.HasValue)
            {
                return path1.assignedCableSize.Value == path2.assignedCableSize.Value;
            }

            // If one or both paths don't have assigned sizes, check if they share any supported sizes
            return path1.supportedCableSizes.Intersect(path2.supportedCableSizes).Any();
        }

        // Modify IdentifyRoomEndpoints to exclude boundary port connections:
        private void IdentifyRoomEndpoints()
        {
            // For each template
            foreach (var template in templates)
            {
                if (template == null) continue;

                // For each path
                foreach (var path in template.waypointPaths)
                {
                    // Check if entry point is connected as a target, to a node, or to a boundary port
                    ConnectionPoint entry = path.GetEntryPoint();
                    bool entryConnected = connections.Exists(c =>
                        (c.targetTemplate == template && c.targetPath == path && c.targetPoint == entry));

                    // Check if connected to any node output
                    bool entryConnectedToNode = false;
                    foreach (var node in nodes)
                    {
                        if (node.activeConnections.Any(nc => !nc.isInput && nc.connectedPath == path))
                        {
                            entryConnectedToNode = true;
                            break;
                        }
                    }

                    // Check if connected to any boundary port
                    bool entryConnectedToBoundary = boundaryPorts.Any(p =>
                        p.activeConnection != null &&
                        p.activeConnection.connectedPath == path &&
                        p.activeConnection.isConnectedToEntry);

                    if (!entryConnected && !entryConnectedToNode && !entryConnectedToBoundary)
                    {
                        roomEndpoints.Add(new RoomEndpoint(template, path, entry, true));
                    }

                    // Check if exit point is connected as a source, to a node, or to a boundary port
                    ConnectionPoint exit = path.GetExitPoint();
                    bool exitConnected = connections.Exists(c =>
                        (c.sourceTemplate == template && c.sourcePath == path && c.sourcePoint == exit));

                    // Check if connected to any node input
                    bool exitConnectedToNode = false;
                    foreach (var node in nodes)
                    {
                        if (node.activeConnections.Any(nc => nc.isInput && nc.connectedPath == path))
                        {
                            exitConnectedToNode = true;
                            break;
                        }
                    }

                    // Check if connected to any boundary port
                    bool exitConnectedToBoundary = boundaryPorts.Any(p =>
                        p.activeConnection != null &&
                        p.activeConnection.connectedPath == path &&
                        !p.activeConnection.isConnectedToEntry);

                    if (!exitConnected && !exitConnectedToNode && !exitConnectedToBoundary)
                    {
                        roomEndpoints.Add(new RoomEndpoint(template, path, exit, false));
                    }
                }
            }
        }

        private void FindPathsThroughNode(PowerNode node, WaypointPath inputPath,
                                         List<WaypointPath> currentPath, List<List<WaypointPath>> allPaths,
                                         HashSet<string> visitedInCurrentPath, int maxDepth)
        {
            if (currentPath.Count > maxDepth) return;

            // Get the output path connected through this node
            var outputPath = node.GetConnectedOutputPath(inputPath);

            if (outputPath == null)
            {
                // Path is disconnected in the node, this is a dead end
                return;
            }

            // Find the template containing this output path
            Template outputTemplate = null;
            foreach (var template in templates)
            {
                if (template.waypointPaths.Contains(outputPath))
                {
                    outputTemplate = template;
                    break;
                }
            }

            if (outputTemplate == null) return;

            string nextPathId = $"{outputTemplate.templateId}_{outputPath.pathId}";

            // Skip if already visited
            if (visitedInCurrentPath.Contains(nextPathId)) return;

            // Add to path
            List<WaypointPath> newPath = new(currentPath)
            {
                outputPath
            };

            HashSet<string> newVisited = new(visitedInCurrentPath)
            {
                nextPathId
            };

            // Continue path finding from this output
            FindPathsToExits(outputTemplate, outputPath, newPath, allPaths, newVisited, maxDepth);
        }

        // Modify FindPathsToExits to handle nodes:
        private void FindPathsToExits(Template currentTemplate, WaypointPath currentSegment,
                                     List<WaypointPath> currentPath, List<List<WaypointPath>> allPaths,
                                     HashSet<string> visitedInCurrentPath, int maxDepth = 10)
        {
            if (currentPath.Count > maxDepth) return;

            // Check if this path connects to a node
            PowerNode connectedNode = null;
            foreach (var node in nodes)
            {
                if (node.activeConnections.Any(c => c.isInput && c.connectedPath == currentSegment && c.connectedTemplate == currentTemplate))
                {
                    connectedNode = node;
                    break;
                }
            }

            if (connectedNode != null)
            {
                // Path goes through a node
                FindPathsThroughNode(connectedNode, currentSegment, currentPath, allPaths, visitedInCurrentPath, maxDepth);
                return;
            }

            // Find connections from this segment's exit
            List<TemplateConnection> outgoingConnections = connections.FindAll(c =>
                c.sourceTemplate == currentTemplate && c.sourcePath == currentSegment);

            if (outgoingConnections.Count == 0)
            {
                // This is an exit point, add the path if it's valid
                if (currentPath.Count > 0)
                {
                    allPaths.Add(new List<WaypointPath>(currentPath));
                }
                return;
            }

            // Follow each connection
            foreach (var connection in outgoingConnections)
            {
                string nextPathId = $"{connection.targetTemplate.templateId}_{connection.targetPath.pathId}";

                // Skip if we've already visited this path in the current traversal
                if (visitedInCurrentPath.Contains(nextPathId)) continue;

                // Create a new path including this connection
                List<WaypointPath> newPath = new(currentPath)
                {
                    connection.targetPath
                };

                // Create new visited set for this branch
                HashSet<string> newVisited = new(visitedInCurrentPath)
                {
                    nextPathId
                };

                // Continue recursively
                FindPathsToExits(connection.targetTemplate, connection.targetPath,
                                newPath, allPaths, newVisited, maxDepth);
            }
        }

        private void BuildRoomPaths()
        {
            HashSet<string> processedPaths = new();

            // Get all entry points
            List<RoomEndpoint> entries = roomEndpoints.FindAll(e => e.isEntryPoint);

            // For each entry point, find all possible paths to exit points
            foreach (var entry in entries)
            {
                List<List<WaypointPath>> allPaths = new();
                List<WaypointPath> currentPath = new();
                HashSet<string> visitedInCurrentPath = new();

                currentPath.Add(entry.path);
                visitedInCurrentPath.Add($"{entry.template.templateId}_{entry.path.pathId}");

                FindPathsToExits(entry.template, entry.path, currentPath, allPaths, visitedInCurrentPath);

                // Create RoomPath objects for each discovered path
                foreach (var pathSegments in allPaths)
                {
                    if (pathSegments.Count == 0) continue;

                    // Create path identifier
                    string pathId = string.Join("->", pathSegments.Select(p => p.pathId));

                    if (!processedPaths.Contains(pathId))
                    {
                        processedPaths.Add(pathId);

                        // Find the exit endpoint
                        WaypointPath lastSegment = pathSegments.Last();
                        Template lastTemplate = null;

                        // Find which template contains the last segment
                        foreach (var template in templates)
                        {
                            if (template.waypointPaths.Contains(lastSegment))
                            {
                                lastTemplate = template;
                                break;
                            }
                        }

                        if (lastTemplate != null)
                        {
                            RoomEndpoint exitPoint = roomEndpoints.Find(e =>
                                !e.isEntryPoint &&
                                e.template == lastTemplate &&
                                e.path == lastSegment);

                            if (exitPoint != null)
                            {
                                RoomPath roomPath = new(entry, exitPoint);
                                roomPath.pathSegments.AddRange(pathSegments);
                                roomPaths.Add(roomPath);
                            }
                        }
                    }
                }
            }
        }

        // Add new method for finding boundary port connections:
        private void FindBoundaryPortConnections()
        {
            foreach (var port in boundaryPorts)
            {
                if (port == null) continue;

                port.ClearConnection(); // Clear existing connection

                // Check templates for connections to this port
                foreach (var template in templates)
                {
                    if (template == null) continue;

                    foreach (var path in template.waypointPaths)
                    {
                        // Check if path exit connects to port
                        ConnectionPoint pathExit = path.GetExitPoint();
                        Vector3 exitWorldPos = template.transform.TransformPoint(pathExit.position);
                        Vector3 portWorldPos = port.GetWorldPosition();

                        if (Vector3.Distance(exitWorldPos, portWorldPos) <= connectionThreshold)
                        {
                            var connection = new NetworkBoundaryPort.PortConnection(path, template, false);
                            port.SetConnection(connection);
                            continue; // Only one connection per port
                        }

                        // Check if path entry connects to port
                        ConnectionPoint pathEntry = path.GetEntryPoint();
                        Vector3 entryWorldPos = template.transform.TransformPoint(pathEntry.position);

                        if (Vector3.Distance(entryWorldPos, portWorldPos) <= connectionThreshold)
                        {
                            var connection = new NetworkBoundaryPort.PortConnection(path, template, true);
                            port.SetConnection(connection);
                        }
                    }
                }
            }
        }

        // Add method to get connected boundary ports:
        public List<NetworkBoundaryPort> GetConnectedBoundaryPorts()
        {
            return boundaryPorts.FindAll(p => p.IsConnected());
        }

        // Public accessor methods
        public List<TemplateConnection> GetConnections()
        {
            return connections;
        }

        public List<RoomEndpoint> GetRoomEntries()
        {
            return roomEndpoints.FindAll(e => e.isEntryPoint);
        }

        public List<RoomEndpoint> GetRoomExits()
        {
            return roomEndpoints.FindAll(e => !e.isEntryPoint);
        }

        public List<RoomPath> GetCablePaths()
        {
            return roomPaths;
        }

        public Template GetTemplateById(string id)
        {
            return templates.Find(t => t.templateId == id);
        }

        public List<Template> GetTemplatesByType(Template.TemplateType type)
        {
            return templates.FindAll(t => t.templateType == type);
        }

        public List<RoomPath> GetPathsFromEntry(RoomEndpoint entry)
        {
            return roomPaths.FindAll(p => p.entry == entry);
        }

        public List<RoomPath> GetPathsToExit(RoomEndpoint exit)
        {
            return roomPaths.FindAll(p => p.exit == exit);
        }

        public void SelectPath(RoomPath path)
        {
            foreach (var p in roomPaths)
            {
                p.isSelected = (p == path);
            }
        }

        public void ClearSelection()
        {
            foreach (var p in roomPaths)
            {
                p.isSelected = false;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!showConnectionGizmos) return;

            // Draw connections
            foreach (var connection in connections)
            {
                if (connection.sourceTemplate == null || connection.targetTemplate == null) continue;

                Vector3 sourcePos = connection.GetSourceWorldPosition();
                Vector3 targetPos = connection.GetTargetWorldPosition();

                Gizmos.color = connection.isSizeCompatible ? compatibleConnectionColor : incompatibleConnectionColor;
                Gizmos.DrawLine(sourcePos, targetPos);

                // Draw a small sphere at the connection point
                float sphereSize = 0.05f;
                Gizmos.DrawSphere((sourcePos + targetPos) * 0.5f, sphereSize);
            }

            // Draw room endpoints
            foreach (var endpoint in roomEndpoints)
            {
                if (endpoint.template == null) continue;

                Vector3 pos = endpoint.GetWorldPosition();
                Gizmos.color = endpoint.isEntryPoint ? entryPointColor : exitPointColor;
                Gizmos.DrawSphere(pos, 0.15f);
            }

            // Draw selected paths
            foreach (var path in roomPaths)
            {
                if (!path.isSelected) continue;

                // Draw the entire path in white
                Gizmos.color = selectedPathColor;

                // Connect all segments
                Template prevTemplate = null;
                Vector3 prevPos = Vector3.zero;
                bool isFirst = true;

                foreach (var segment in path.pathSegments)
                {
                    // Find which template contains this segment
                    Template currentTemplate = templates.Find(t => t.waypointPaths.Contains(segment));
                    if (currentTemplate == null) continue;

                    // Draw the segment
                    List<Vector3> positions = segment.GetPathPositions();
                    for (int i = 0; i < positions.Count; i++)
                    {
                        Vector3 worldPos = currentTemplate.transform.TransformPoint(positions[i]);

                        if (i > 0 || isFirst)
                        {
                            if (!isFirst)
                            {
                                Gizmos.DrawLine(prevPos, worldPos);
                            }

                            prevPos = worldPos;
                        }
                    }

                    isFirst = false;
                    prevTemplate = currentTemplate;
                }
            }
        }
#endif
    }
}