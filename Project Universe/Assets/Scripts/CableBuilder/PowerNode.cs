using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace ProjectUniverse.PowerSystem
{
    public class PowerNode : MonoBehaviour
    {
        [Header("Node Configuration")]
        public string nodeId;
        public int connectionCount = 2; // Number of input/output pairs

        [Header("Connection Points")]
        public List<ConnectionPoint> inputPoints = new();
        public List<ConnectionPoint> outputPoints = new();

        [System.Serializable]
        public class InternalRoute
        {
            public int inputIndex;
            public int outputIndex;
            public bool isConnected = true; // Can be disconnected by user

            public InternalRoute(int input, int output)
            {
                inputIndex = input;
                outputIndex = output;
                isConnected = true;
            }
        }

        public List<InternalRoute> internalRoutes = new();

        [Header("Visualization")]
        public bool showGizmos = true;
        public Color inputPointColor = Color.blue;
        public Color outputPointColor = new(1f, 0.5f, 0f); // Orange
        public Color nodeColor = Color.cyan;
        public Color routeColor = Color.yellow;
        public Color disconnectedRouteColor = Color.red;
        public float gizmoSize = 0.2f;

        // Track active connections
        [System.Serializable]
        public class NodeConnection
        {
            public WaypointPath connectedPath;
            public Template connectedTemplate;
            public ConnectionPoint nodePoint;
            public bool isInput; // true for input, false for output
            public int pointIndex; // Index in the input/output list

            public NodeConnection(WaypointPath path, Template template, ConnectionPoint point, bool input, int index)
            {
                connectedPath = path;
                connectedTemplate = template;
                nodePoint = point;
                isInput = input;
                pointIndex = index;
            }
        }

        [HideInInspector]
        public List<NodeConnection> activeConnections = new();

        private void Awake()
        {
            if (string.IsNullOrEmpty(nodeId))
            {
                nodeId = gameObject.name + "_" + System.Guid.NewGuid().ToString().Substring(0, 8);
            }

            // Initialize connection points if empty
            if (inputPoints.Count == 0 || outputPoints.Count == 0)
            {
                GenerateConnectionPoints();
            }

            // Initialize default routing if empty
            if (internalRoutes.Count == 0)
            {
                GenerateDefaultRouting();
            }
        }

        private void GenerateConnectionPoints()
        {
            inputPoints.Clear();
            outputPoints.Clear();

            float spacing = 1f;
            float startOffset = -(connectionCount - 1) * spacing * 0.5f;

            for (int i = 0; i < connectionCount; i++)
            {
                // Input points on the left
                Vector3 inputPosition = new(-1f, 0, startOffset + (i * spacing));
                ConnectionPoint inputPoint = new(
                    $"{nodeId}_input_{i}",
                    inputPosition
                );
                inputPoints.Add(inputPoint);

                // Output points on the right
                Vector3 outputPosition = new(1f, 0, startOffset + (i * spacing));
                ConnectionPoint outputPoint = new(
                    $"{nodeId}_output_{i}",
                    outputPosition
                );
                outputPoints.Add(outputPoint);
            }
        }

        private void GenerateDefaultRouting()
        {
            internalRoutes.Clear();

            // Default: connect each input to corresponding output
            for (int i = 0; i < connectionCount; i++)
            {
                internalRoutes.Add(new InternalRoute(i, i));
            }
        }

        public void RegenerateWithNewCount(int newCount)
        {
            connectionCount = newCount;
            GenerateConnectionPoints();
            GenerateDefaultRouting();
            activeConnections.Clear(); // Clear active connections when restructuring
        }

        public InternalRoute GetRouteFromInput(int inputIndex)
        {
            return internalRoutes.Find(r => r.inputIndex == inputIndex && r.isConnected);
        }

        public InternalRoute GetRouteToOutput(int outputIndex)
        {
            return internalRoutes.Find(r => r.outputIndex == outputIndex && r.isConnected);
        }

        public bool IsInputConnectedToOutput(int inputIndex, int outputIndex)
        {
            var route = internalRoutes.Find(r => r.inputIndex == inputIndex && r.outputIndex == outputIndex);
            return route != null && route.isConnected;
        }

        public void SetRoute(int inputIndex, int outputIndex, bool connected)
        {
            // Remove any existing route from this input
            internalRoutes.RemoveAll(r => r.inputIndex == inputIndex);

            // Remove any existing route to this output
            internalRoutes.RemoveAll(r => r.outputIndex == outputIndex);

            // Add new route if connected
            if (connected)
            {
                internalRoutes.Add(new InternalRoute(inputIndex, outputIndex));
            }
        }

        public void DisconnectRoute(int inputIndex)
        {
            var route = internalRoutes.Find(r => r.inputIndex == inputIndex);
            if (route != null)
            {
                route.isConnected = false;
            }
        }

        public void ReconnectRoute(int inputIndex)
        {
            var route = internalRoutes.Find(r => r.inputIndex == inputIndex);
            if (route != null)
            {
                route.isConnected = true;
            }
        }

        public WaypointPath GetConnectedOutputPath(WaypointPath inputPath)
        {
            // Find which input this path is connected to
            var inputConnection = activeConnections.Find(c => c.isInput && c.connectedPath == inputPath);
            if (inputConnection == null) return null;

            // Find the internal route from this input
            var route = GetRouteFromInput(inputConnection.pointIndex);
            if (route == null || !route.isConnected) return null;

            // Find the output connection at the routed output
            var outputConnection = activeConnections.Find(c => !c.isInput && c.pointIndex == route.outputIndex);
            return outputConnection?.connectedPath;
        }

        public void AddConnection(NodeConnection connection)
        {
            // Remove any existing connection at this point
            activeConnections.RemoveAll(c => c.isInput == connection.isInput && c.pointIndex == connection.pointIndex);

            activeConnections.Add(connection);
        }

        public void RemoveConnection(WaypointPath path)
        {
            activeConnections.RemoveAll(c => c.connectedPath == path);
        }

        public void ClearConnections()
        {
            activeConnections.Clear();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!showGizmos) return;

            // Draw node center
            Gizmos.color = nodeColor;
            Gizmos.DrawCube(transform.position, 2 * gizmoSize * Vector3.one);

            // Draw input points
            for (int i = 0; i < inputPoints.Count; i++)
            {
                Gizmos.color = inputPointColor;
                Vector3 worldPos = transform.TransformPoint(inputPoints[i].position);
                Gizmos.DrawSphere(worldPos, gizmoSize);

                // Draw line from node center to input point
                Gizmos.DrawLine(transform.position, worldPos);

                // Check if connected
                bool isConnected = activeConnections.Any(c => c.isInput && c.pointIndex == i);
                if (isConnected)
                {
                    Gizmos.DrawWireCube(worldPos, 2.5f * gizmoSize * Vector3.one);
                }
            }

            // Draw output points
            for (int i = 0; i < outputPoints.Count; i++)
            {
                Gizmos.color = outputPointColor;
                Vector3 worldPos = transform.TransformPoint(outputPoints[i].position);
                Gizmos.DrawSphere(worldPos, gizmoSize);

                // Draw line from node center to output point
                Gizmos.DrawLine(transform.position, worldPos);

                // Check if connected
                bool isConnected = activeConnections.Any(c => !c.isInput && c.pointIndex == i);
                if (isConnected)
                {
                    Gizmos.DrawWireCube(worldPos, 2.5f * gizmoSize * Vector3.one);
                }
            }

            // Draw internal routing
            foreach (var route in internalRoutes)
            {
                if (route.inputIndex < inputPoints.Count && route.outputIndex < outputPoints.Count)
                {
                    Gizmos.color = route.isConnected ? routeColor : disconnectedRouteColor;

                    Vector3 inputPos = transform.TransformPoint(inputPoints[route.inputIndex].position);
                    Vector3 outputPos = transform.TransformPoint(outputPoints[route.outputIndex].position);

                    // Draw curved line through node center
                    Vector3[] points = new Vector3[] { inputPos, transform.position, outputPos };
                    for (int i = 0; i < points.Length - 1; i++)
                    {
                        Gizmos.DrawLine(points[i], points[i + 1]);
                    }
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!showGizmos) return;

            // Draw labels for connection points
            UnityEditor.Handles.color = Color.white;

            for (int i = 0; i < inputPoints.Count; i++)
            {
                Vector3 worldPos = transform.TransformPoint(inputPoints[i].position);
                UnityEditor.Handles.Label(worldPos + Vector3.up * 0.3f, $"IN {i}");
            }

            for (int i = 0; i < outputPoints.Count; i++)
            {
                Vector3 worldPos = transform.TransformPoint(outputPoints[i].position);
                UnityEditor.Handles.Label(worldPos + Vector3.up * 0.3f, $"OUT {i}");
            }
        }
#endif
    }
}