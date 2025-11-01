using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ProjectUniverse.PowerSystem;

[Serializable]
public class SerializableConnection
{
    public string outputNodeId;
    public string inputNodeId;
    public int outputPortIndex;
    public int inputPortIndex;

    public SerializableConnection(string outputId, string inputId, int outputPort, int inputPort)
    {
        outputNodeId = outputId;
        inputNodeId = inputId;
        outputPortIndex = outputPort;
        inputPortIndex = inputPort;
    }
}

[Serializable]
public class PowerNodeState
{
    public string nodeId;
    public int connectionCount;

    [Serializable]
    public class SerializableInternalRoute
    {
        public int inputIndex;
        public int outputIndex;
        public bool isConnected;

        public SerializableInternalRoute(int input, int output, bool connected)
        {
            inputIndex = input;
            outputIndex = output;
            isConnected = connected;
        }
    }

    public List<SerializableInternalRoute> internalRoutes = new List<SerializableInternalRoute>();

    [Serializable]
    public class SerializableNodeConnection
    {
        public string connectedPathId;
        public string connectedTemplateId;
        public bool isInput;
        public int pointIndex;

        public SerializableNodeConnection(string pathId, string templateId, bool input, int index)
        {
            connectedPathId = pathId;
            connectedTemplateId = templateId;
            isInput = input;
            pointIndex = index;
        }
    }

    public List<SerializableNodeConnection> activeConnections = new List<SerializableNodeConnection>();
}

[Serializable]
public class NetworkNode
{
    public string Id;
    public string Name;
    public Vector2 Position;

    // Serializable connection data - no longer direct references
    public List<string> InputConnectionIds = new List<string>();
    public List<string> OutputConnectionIds = new List<string>();
    public List<int> InputConnectionPortIndices = new List<int>();
    public List<int> OutputConnectionPortIndices = new List<int>();

    // Runtime connection references (rebuilt from serialized data)
    [System.NonSerialized]
    public List<NetworkNode> InputConnections = new List<NetworkNode>();
    [System.NonSerialized]
    public List<NetworkNode> OutputConnections = new List<NetworkNode>();

    // Persistent GameObject identification
    public string SourceGameObjectPath; // Scene hierarchy path
    public string SourceGameObjectName; // Fallback name
    public string ComponentType;
    public string SourceRoomPath; // Scene hierarchy path for room

    // Runtime references (rebuilt from paths)
    [System.NonSerialized]
    public GameObject SourceGameObject;
    [System.NonSerialized]
    public HolographicRoom SourceRoom;
    [System.NonSerialized]
    public RoomNetwork RoomNetwork;

    // PowerNode state serialization
    public PowerNodeState powerNodeState;

    // Cached port counts
    private int _cachedInputPortCount = 1;
    private int _cachedOutputPortCount = 1;

    public int TotalInputPortCount
    {
        get => _cachedInputPortCount + GetWaypointInputPorts().Count;
        set => _cachedInputPortCount = value;
    }
    public int TotalOutputPortCount
    {
        get => _cachedOutputPortCount + GetWaypointOutputPorts().Count;
        set => _cachedOutputPortCount = value;
    }
    public int InputPortCount
    {
        get => _cachedInputPortCount;
        set => _cachedInputPortCount = value;
    }

    public int OutputPortCount
    {
        get => _cachedOutputPortCount;
        set => _cachedOutputPortCount = value;
    }

    // Keep existing template and boundary port info classes
    [System.Serializable]
    public class TemplateInfo
    {
        public string templateId;
        public string templateName;
        public Template.TemplateType templateType;
        public List<CableSize> supportedSizes = new();
        public List<CableSize> assignedSizes = new();
        public int usedPaths;
        public int maxPaths;
        public float utilizationPercentage;
        public int maxCableCapacity;
        public int currentCableCount;
    }

    public List<TemplateInfo> templateInfos = new();

    [System.Serializable]
    public class BoundaryPortInfo
    {
        public string portId;
        public string boundaryName;
        public bool isConnected;
        public CableSize? assignedCableSize;
        public bool isInput;
        public Vector3 worldPosition;
    }

    [System.Serializable]
    public class WaypointPortInfo
    {
        public string waypointPathId;
        public string waypointName;
        public bool isComponentInput; // true if this connects to component's input
        public Vector3 waypointWorldPosition;
        public HolographicRoom connectedRoom;
        public string connectedRoomPath;

        // Runtime reference
        [System.NonSerialized]
        public WaypointPath waypointPath;
    }

    public List<WaypointPortInfo> waypointPortInfos = new();

    public List<BoundaryPortInfo> boundaryPortInfos = new();

    public NetworkNode()
    {
        Id = Guid.NewGuid().ToString();
    }

    public void SetSourceGameObject(GameObject gameObject)
    {
        SourceGameObject = gameObject;
        if (gameObject != null)
        {
            SourceGameObjectPath = GetGameObjectPath(gameObject);
            SourceGameObjectName = gameObject.name;
        }
    }

    public void SetSourceRoom(HolographicRoom room)
    {
        SourceRoom = room;
        if (room != null)
        {
            SourceRoomPath = GetGameObjectPath(room.gameObject);
        }
    }

    public void RestoreGameObjectReferences()
    {
        // Restore SourceGameObject
        if (!string.IsNullOrEmpty(SourceGameObjectPath))
        {
            SourceGameObject = GameObject.Find(SourceGameObjectPath);
            if (SourceGameObject == null && !string.IsNullOrEmpty(SourceGameObjectName))
            {
                SourceGameObject = GameObject.Find(SourceGameObjectName);
            }
        }

        // Restore SourceRoom
        if (!string.IsNullOrEmpty(SourceRoomPath))
        {
            var roomGameObject = GameObject.Find(SourceRoomPath);
            if (roomGameObject != null)
            {
                SourceRoom = roomGameObject.GetComponent<HolographicRoom>();
            }
        }

        // Restore RoomNetwork
        if (SourceGameObject != null)
        {
            RoomNetwork = SourceGameObject.GetComponent<RoomNetwork>();
        }
    }

    public void CapturePowerNodeState()
    {
        if (SourceGameObject == null) return;

        if (!SourceGameObject.TryGetComponent<PowerNode>(out var powerNode)) return;

        powerNodeState = new PowerNodeState
        {
            nodeId = powerNode.nodeId,
            connectionCount = powerNode.connectionCount
        };

        // Capture internal routes
        foreach (var route in powerNode.internalRoutes)
        {
            powerNodeState.internalRoutes.Add(new PowerNodeState.SerializableInternalRoute(
                route.inputIndex, route.outputIndex, route.isConnected));
        }

        // Capture active connections
        foreach (var connection in powerNode.activeConnections)
        {
            string pathId = connection.connectedPath?.pathId ?? "";
            string templateId = connection.connectedTemplate?.templateId ?? "";
            powerNodeState.activeConnections.Add(new PowerNodeState.SerializableNodeConnection(
                pathId, templateId, connection.isInput, connection.pointIndex));
        }
    }

    public void RestorePowerNodeState()
    {
        if (powerNodeState == null || SourceGameObject == null) return;

        if (!SourceGameObject.TryGetComponent<PowerNode>(out var powerNode)) return;

        powerNode.nodeId = powerNodeState.nodeId;
        powerNode.connectionCount = powerNodeState.connectionCount;

        // Restore internal routes
        powerNode.internalRoutes.Clear();
        foreach (var route in powerNodeState.internalRoutes)
        {
            powerNode.internalRoutes.Add(new PowerNode.InternalRoute(route.inputIndex, route.outputIndex)
            {
                isConnected = route.isConnected
            });
        }

        // Note: Active connections would need to be restored after all templates/paths are loaded
        // This might require a second pass or delayed restoration
    }
    
    public void DetectWaypointConnections()
    {
        waypointPortInfos.Clear();

        if (SourceGameObject == null) return;

        var globalResolver = GlobalRouteResolver.Instance; // Use singleton
        if (globalResolver == null)
        {
            Debug.LogWarning("GlobalRouteResolver.Instance is null");
            return;
        }

        // Ensure power connections are populated
        if (globalResolver.powerConnections.Count == 0)
        {
            Debug.LogWarning("No power connections found in GlobalRouteResolver");
            return;
        }

        // Find power connections for this component
        var componentConnections = globalResolver.powerConnections
            .Where(pc => pc.source == SourceGameObject)
            .ToList();

        Debug.Log($"Found {componentConnections.Count} power connections for {Name}");

        foreach (var connection in componentConnections)
        {
            var waypointInfo = new WaypointPortInfo
            {
                waypointPathId = connection.waypoint.pathId,
                waypointName = connection.waypoint.pathId, // WaypointPath doesn't have pathName
                waypointWorldPosition = connection.port.GetWorldPosition(), // Use port position
                waypointPath = connection.waypoint
            };

            // Determine if this connects to component's input or output
            waypointInfo.isComponentInput = DetermineConnectionDirection(connection);

            // Find which room contains this waypoint
            waypointInfo.connectedRoom = FindRoomContainingWaypoint(connection.waypoint);
            if (waypointInfo.connectedRoom != null)
            {
                waypointInfo.connectedRoomPath = GetGameObjectPath(waypointInfo.connectedRoom.gameObject);
            }

            waypointPortInfos.Add(waypointInfo);
        }

        Debug.Log($"Detected {waypointPortInfos.Count} waypoint connections for {Name}");
    }

    public void DetectWaypointConnectionsA2()
    {
        waypointPortInfos.Clear();

        if (SourceGameObject == null)
        {
            Debug.LogWarning($"SourceGameObject is null for node {Name}");
            return;
        }

        var globalResolver = GameObject.FindFirstObjectByType<GlobalRouteResolver>();
        if (globalResolver == null)
        {
            Debug.LogWarning("No GlobalRouteResolver found in scene");
            return;
        }

        Debug.Log($"Checking waypoint connections for {Name} ({ComponentType})");
        Debug.Log($"Total power connections in resolver: {globalResolver.powerConnections.Count}");

        // Find power connections for this component
        var componentConnections = new List<GlobalRouteResolver.PowerConnection>();

        foreach (var connection in globalResolver.powerConnections)
        {
            if (connection.source == SourceGameObject)
            {
                componentConnections.Add(connection);
                Debug.Log($"Found matching connection for {Name}: waypoint {connection.waypoint.pathId}");
            }
        }

        Debug.Log($"Found {componentConnections.Count} power connections for {Name}");

        foreach (var connection in componentConnections)
        {
            var waypointInfo = new WaypointPortInfo
            {
                waypointPathId = connection.waypoint.pathId,
                waypointName = connection.waypoint.pathId, // Use pathId as name for now
                waypointWorldPosition = GetWaypointWorldPosition(connection.waypoint),
                waypointPath = connection.waypoint
            };

            // Determine if this connects to component's input or output
            waypointInfo.isComponentInput = DetermineConnectionDirection(connection);

            // Find which room contains this waypoint
            waypointInfo.connectedRoom = FindRoomContainingWaypoint(connection.waypoint);
            if (waypointInfo.connectedRoom != null)
            {
                waypointInfo.connectedRoomPath = GetGameObjectPath(waypointInfo.connectedRoom.gameObject);
                Debug.Log($"Waypoint {waypointInfo.waypointName} connects {Name} to room {waypointInfo.connectedRoom.roomName}");
            }
            else
            {
                Debug.LogWarning($"Could not find room for waypoint {waypointInfo.waypointName}");
            }

            waypointPortInfos.Add(waypointInfo);
        }

        Debug.Log($"Added {waypointPortInfos.Count} waypoint port infos for {Name}");
    }

    private Vector3 GetWaypointWorldPosition(WaypointPath waypoint)
    {
        // Try to get world position from the waypoint path
        if (waypoint.entryPoint != null)
        {
            // Find the template containing this waypoint to get proper world position
            foreach (var room in GameObject.FindObjectsByType<RoomNetwork>(FindObjectsSortMode.None))
            {
                foreach (var template in room.templates)
                {
                    if (template.waypointPaths.Contains(waypoint))
                    {
                        return template.transform.TransformPoint(waypoint.entryPoint.position);
                    }
                }
            }
        }
        return Vector3.zero;
    }

    private bool DetermineConnectionDirection(GlobalRouteResolver.PowerConnection connection)
    {
        // For power system components, determine based on component type and connection point
        switch (ComponentType)
        {
            case "IGenerator":
                // Generators only have outputs, so waypoint connections are outputs
                return false;

            case "IBreakerBox":
            case "IMachine":
                // These only have inputs, so waypoint connections are inputs  
                return true;

            case "IRouter":
            case "IRoutingSubstation":
                return InferDirectionFromConnectionPoint(connection.port);

            default:
                return true;
        }
    }

    private bool InferDirectionFromConnectionPoint(PowerConnectionPoint connectionPoint)
    {
        // If we can't infer from name
        if(connectionPoint.connectionType == PowerConnectionPoint.ConnectionType.Input)
        {
            return true;
        }
        else if (connectionPoint.connectionType == PowerConnectionPoint.ConnectionType.Output)
        {
            return false;
        }

        // Try to infer from the connection point name
        string name = connectionPoint.name.ToLower();

        if (name.Contains("input") || name.Contains("in"))
        {
            return true; // Input
        }
        else if (name.Contains("output") || name.Contains("out"))
        {
            return false; // Output
        }

        // Default to input if we can't determine
        Debug.LogWarning($"Could not determine direction for connection point {connectionPoint.name}, defaulting to input");
        return true;
    }

    private HolographicRoom FindRoomContainingWaypoint(WaypointPath waypoint)
    {
        // Find the room containing this waypoint
        foreach (RoomNetwork rn in GlobalRouteResolver.Instance.roomNetworks) {
            foreach (var rnTemplate in rn.templates)
            {
                foreach (var rnWaypoint in rnTemplate.waypointPaths)
                {
                    if (rnWaypoint.pathId == waypoint.pathId)
                    {
                        return rn.gameObject.GetComponent<HolographicRoom>();
                    }
                }
            } 
        }
        return null;
    }

    public List<WaypointPortInfo> GetWaypointInputPorts()
    {
        return waypointPortInfos.Where(w => w.isComponentInput).ToList();
    }

    public List<WaypointPortInfo> GetWaypointOutputPorts()
    {
        return waypointPortInfos.Where(w => !w.isComponentInput).ToList();
    }

    private string GetGameObjectPath(GameObject obj)
    {
        if (obj == null) return "";

        string path = obj.name;
        Transform parent = obj.transform.parent;

        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }

        return path;
    }

    // Keep existing methods
    public void RefreshRoomNetworkData()
    {
        if (RoomNetwork == null)
        {
            _cachedInputPortCount = 1;
            _cachedOutputPortCount = 1;
            templateInfos.Clear();
            boundaryPortInfos.Clear();
            return;
        }

        RoomNetwork.RefreshConnections();
        RefreshTemplateInfo();
        RefreshBoundaryPortInfo();

        _cachedInputPortCount = Math.Max(1, boundaryPortInfos.Count(p => p.isInput));
        _cachedOutputPortCount = Math.Max(1, boundaryPortInfos.Count(p => !p.isInput));
    }

    private void RefreshTemplateInfo()
    {
        templateInfos.Clear();
        if (RoomNetwork?.templates == null) return;

        foreach (var template in RoomNetwork.templates)
        {
            if (template == null) continue;

            var info = new TemplateInfo
            {
                templateId = template.templateId,
                templateName = !string.IsNullOrEmpty(template.name) ? template.name : template.templateId,
                templateType = template.templateType,
                usedPaths = template.waypointPaths?.Count(p => p.assignedCableSize.HasValue) ?? 0,
                maxPaths = template.waypointPaths?.Count ?? 0,
                maxCableCapacity = template.maxCableCapacity,
                currentCableCount = template.currentCableCount
            };

            var supportedSizes = template.GetSupportedCableSizes();
            if (supportedSizes != null)
            {
                info.supportedSizes = supportedSizes.ToList();
            }

            var assignedSizes = new HashSet<CableSize>();
            if (template.waypointPaths != null)
            {
                foreach (var path in template.waypointPaths)
                {
                    if (path?.assignedCableSize.HasValue == true)
                    {
                        assignedSizes.Add(path.assignedCableSize.Value);
                    }
                }
            }
            info.assignedSizes = assignedSizes.ToList();
            info.utilizationPercentage = info.maxPaths > 0 ? (float)info.usedPaths / info.maxPaths * 100f : 0f;

            templateInfos.Add(info);
        }
    }

    private void RefreshBoundaryPortInfo()
    {
        boundaryPortInfos.Clear();

        if (RoomNetwork?.boundaryPorts == null) return;

        // Get all CONNECTED boundary ports and sort them consistently by portId for deterministic ordering
        var sortedPorts = RoomNetwork.boundaryPorts
            .Where(port => port != null && port.IsConnected()) // Only include connected ports
            .OrderBy(port => port.portId)
            .ToList();

        Debug.Log($"Room {Name} has {sortedPorts.Count} connected boundary ports (out of {RoomNetwork.boundaryPorts.Count} total)");

        foreach (var port in sortedPorts)
        {
            var info = new BoundaryPortInfo
            {
                portId = port.portId,
                boundaryName = !string.IsNullOrEmpty(port.boundaryName) ? port.boundaryName : port.portId,
                isConnected = true, // We already filtered for connected ports
                assignedCableSize = port.assignedCableSize,
                worldPosition = port.GetWorldPosition()
            };

            // Determine if this is an input or output port based on its connection
            if (port.activeConnection != null)
            {
                // If connected to a path entry, it's an input to the room
                // If connected to a path exit, it's an output from the room
                info.isInput = port.activeConnection.isConnectedToEntry;
            }
            else
            {
                // This shouldn't happen since we filtered for connected ports
                info.isInput = true;
            }

            boundaryPortInfos.Add(info);
        }
    }
    
    private void RefreshBoundaryPortInfoA5()
    {
        boundaryPortInfos.Clear();

        if (RoomNetwork?.boundaryPorts == null) return;

        // Get all boundary ports and sort them consistently by portId for deterministic ordering
        var sortedPorts = RoomNetwork.boundaryPorts
            .Where(port => port != null)
            .OrderBy(port => port.portId)
            .ToList();

        foreach (var port in sortedPorts)
        {
            var info = new BoundaryPortInfo
            {
                portId = port.portId,
                boundaryName = !string.IsNullOrEmpty(port.boundaryName) ? port.boundaryName : port.portId,
                isConnected = port.IsConnected(),
                assignedCableSize = port.assignedCableSize,
                worldPosition = port.GetWorldPosition()
            };

            // Determine if this is an input or output port based on its connection
            if (port.IsConnected() && port.activeConnection != null)
            {
                // If connected to a path entry, it's an input to the room
                // If connected to a path exit, it's an output from the room
                info.isInput = port.activeConnection.isConnectedToEntry;
            }
            else
            {
                // For unconnected ports, we can't determine direction easily
                // Default to input for now
                info.isInput = true;
            }

            boundaryPortInfos.Add(info);
        }

        //Debug.Log($"RefreshBoundaryPortInfo: Found {boundaryPortInfos.Count} boundary ports for {Name}");
    }

    public List<BoundaryPortInfo> GetInputPorts()
    {
        return boundaryPortInfos.Where(p => p.isInput).ToList();
    }

    public List<BoundaryPortInfo> GetOutputPorts()
    {
        return boundaryPortInfos.Where(p => !p.isInput).ToList();
    }

    public int GetTotalPathCapacity()
    {
        return templateInfos.Sum(t => t.maxPaths);
    }

    public int GetUsedPathCount()
    {
        return templateInfos.Sum(t => t.usedPaths);
    }

    public float GetOverallUtilization()
    {
        int totalCapacity = GetTotalPathCapacity();
        return totalCapacity > 0 ? (float)GetUsedPathCount() / totalCapacity * 100f : 0f;
    }

    public int GetTotalCableCapacity()
    {
        return templateInfos.Sum(t => t.maxCableCapacity);
    }

    public int GetCurrentCableCount()
    {
        return templateInfos.Sum(t => t.currentCableCount);
    }

    public float GetCableUtilization()
    {
        int totalCapacity = GetTotalCableCapacity();
        return totalCapacity > 0 ? (float)GetCurrentCableCount() / totalCapacity * 100f : 0f;
    }
}