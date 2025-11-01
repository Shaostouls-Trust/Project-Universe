using System.Linq;
using UnityEngine;

namespace ProjectUniverse.PowerSystem
{
    public class NetworkBoundaryPort : MonoBehaviour
    {
        [Header("Port Configuration")]
        public string portId; // Not the same as nodegraph port id
        public string boundaryName; // e.g., "North Wall", "Door A", etc.

        [Header("Connection Point")]
        public ConnectionPoint connectionPoint;

        [Header("Cable Configuration")]
        public CableSize? assignedCableSize; // Inherits from connected path
        // Machines should be able to specify this /\ and other code check aCS if machine connected here
        // so that NBPs can only connect if size same and sizes not defined by path should be okay.

        [System.Serializable]
        public class PortConnection
        {
            public WaypointPath connectedPath;
            public Template connectedTemplate;
            public bool isConnectedToEntry; // true if connected to path entry, false if exit

            public PortConnection(WaypointPath path, Template template, bool toEntry)
            {
                connectedPath = path;
                connectedTemplate = template;
                isConnectedToEntry = toEntry;
            }
        }

        [HideInInspector]
        public PortConnection activeConnection;

        [Header("Visualization")]
        public bool showGizmos = true;
        public Color portColor = Color.magenta;
        public Color connectedColor = Color.green;
        public float gizmoSize = 0.25f;

        private void Awake()
        {
            if (string.IsNullOrEmpty(portId))
            {
                portId = gameObject.name + "_" + System.Guid.NewGuid().ToString()[..8];
            }

            connectionPoint ??= new ConnectionPoint(
                    portId + "_connection",
                    Vector3.zero
                );
        }

        public void SetConnection(PortConnection connection)
        {
            activeConnection = connection;

            // Inherit cable size from connected path
            if (connection != null && connection.connectedPath != null)
            {
                assignedCableSize = connection.connectedPath.assignedCableSize;
            }
            else
            {
                assignedCableSize = null;
            }
        }

        public void ClearConnection()
        {
            activeConnection = null;
            assignedCableSize = null;
        }

        public Vector3 GetWorldPosition()
        {
            return transform.TransformPoint(connectionPoint.position);
        }

        public bool IsConnected()
        {
            return activeConnection != null;
        }

        public bool CanConnectTo(NetworkBoundaryPort otherPort)
        {
            // Both must be connected to paths
            if (!IsConnected() || !otherPort.IsConnected())
                return false;

            // Get the actual paths to check compatibility
            var thisPath = activeConnection.connectedPath;
            var otherPath = otherPort.activeConnection.connectedPath;

            if (thisPath == null || otherPath == null)
                return false;

            // Check if both paths have assigned cable sizes
            if (thisPath.assignedCableSize.HasValue && otherPath.assignedCableSize.HasValue)
            {
                return thisPath.assignedCableSize.Value == otherPath.assignedCableSize.Value;
            }

            // If one or both paths don't have assigned sizes, check if they share any supported sizes
            return thisPath.supportedCableSizes.Intersect(otherPath.supportedCableSizes).Any();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!showGizmos) return;

            Vector3 worldPos = GetWorldPosition();

            // Draw port
            Gizmos.color = IsConnected() ? connectedColor : portColor;
            Gizmos.DrawSphere(worldPos, gizmoSize);

            // Draw boundary indicator
            Gizmos.DrawWireCube(worldPos, 3 * gizmoSize * Vector3.one);

            // Draw connection line to port location
            if (connectionPoint.position != Vector3.zero)
            {
                Gizmos.DrawLine(transform.position, worldPos);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!showGizmos) return;

            Vector3 worldPos = GetWorldPosition();

            // Draw label
            UnityEditor.Handles.color = Color.white;
            UnityEditor.Handles.Label(worldPos + Vector3.up * 0.5f,
                $"Port: {boundaryName}\n" +
                $"Size: {(assignedCableSize.HasValue ? assignedCableSize.Value.ToString() : "None")}\n" +
                $"Connected: {IsConnected()}");
        }
#endif
    }
}