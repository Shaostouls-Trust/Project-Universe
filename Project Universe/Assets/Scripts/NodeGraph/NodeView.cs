using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;
using ProjectUniverse.PowerSystem;

public class NodeView : VisualElement
{
    public NetworkNode Node { get; private set; }

    public event Action<NodeView> OnNodeSelected;
    public event Action<NodeView, bool, int> OnStartConnection;
    public event Action<NodeView, bool, int> OnEndConnection;
    public event Action<NodeView, Vector2> OnStartDrag;
    public event Action<NodeView, Vector2> OnDrag;
    public event Action<NodeView> OnEndDrag;

    protected VisualElement _body;
    private Label _title;
    protected VisualElement _mainContent;
    protected VisualElement _expandedContent;
    private Button _expandButton;
    protected List<VisualElement> _inputPorts = new();
    protected List<VisualElement> _outputPorts = new();

    private Vector2 _lastMousePos;
    private bool _isDragging;
    private bool _isExpanded = false;

    public NodeView(NetworkNode node)
    {
        Node = node;
        // Try to grab the RoomNetwork
        if(Node.SourceGameObject != null)
        {
            if(Node.SourceGameObject.TryGetComponent(out RoomNetwork rn)){
                Node.RoomNetwork = rn;
            }
        }

        // Refresh data before creating UI
        if (Node.RoomNetwork != null)
        {
            Node.RefreshRoomNetworkData();
        }

        AddToClassList("node");
        style.position = Position.Absolute;
        UpdatePosition();

        CreateNodeStructure();
        CreatePorts();
        CreateExpandedContent();
        RegisterCallbacks();
    }

    private Dictionary<PowerNode, HashSet<int>> _inputPortsByPowerNode = new();
    private Dictionary<PowerNode, HashSet<int>> _outputPortsByPowerNode = new();
    private List<int> _ungroupedInputPorts = new();
    private List<int> _ungroupedOutputPorts = new();

    private void CreateNodeStructure()
    {
        _body = new VisualElement();
        _body.AddToClassList("node-body");
        Add(_body);

        // Header with title and expand button
        var header = new VisualElement();
        header.AddToClassList("node-header");
        header.style.flexDirection = FlexDirection.Row;
        header.style.justifyContent = Justify.SpaceBetween;
        header.style.alignItems = Align.Center;
        _body.Add(header);

        _title = new Label(Node.Name);
        _title.AddToClassList("node-title");
        header.Add(_title);

        _expandButton = new Button(() => ToggleExpanded()) { text = "▼" };
        _expandButton.AddToClassList("expand-button");
        _expandButton.style.width = 20;
        _expandButton.style.height = 20;
        header.Add(_expandButton);

        _mainContent = new VisualElement();
        _mainContent.AddToClassList("main-content");
        _body.Add(_mainContent);

        // Utilization summary
        if (Node.RoomNetwork != null)
        {
            var utilizationLabel = new Label($"Utilization: {Node.GetOverallUtilization():F1}%");
            utilizationLabel.AddToClassList("utilization-label");
            _mainContent.Add(utilizationLabel);
        }
    }

    protected virtual void CreatePorts()
    {
        Debug.Log($"=== CreatePorts for {Node.Name} ({Node.ComponentType}) ===");
        Debug.Log($"InputPortCount: {Node.InputPortCount}, OutputPortCount: {Node.OutputPortCount}");
        Debug.Log($"Waypoint Inputs: {Node.GetWaypointInputPorts().Count}, Waypoint Outputs: {Node.GetWaypointOutputPorts().Count}");
        Debug.Log($"Total Inputs: {Node.TotalInputPortCount}, Total Outputs: {Node.TotalOutputPortCount}");
        Debug.Log($"Has RoomNetwork: {Node.RoomNetwork != null}");

        var portsContainer = new VisualElement();
        portsContainer.AddToClassList("ports-container");
        _mainContent.Add(portsContainer);

        // Clear existing ports
        _inputPorts.Clear();
        _outputPorts.Clear();

        // Initialize lists with nulls for all expected ports (including waypoint ports)
        for (int i = 0; i < Node.TotalInputPortCount; i++)
        {
            _inputPorts.Add(null);
        }
        for (int i = 0; i < Node.TotalOutputPortCount; i++)
        {
            _outputPorts.Add(null);
        }

        // Create input ports section
        var inputSection = new VisualElement();
        inputSection.AddToClassList("input-ports-section");
        portsContainer.Add(inputSection);

        // Create static waypoint input ports FIRST
        CreateWaypointInputPorts(inputSection);

        // Then create regular ports based on node type
        if (Node.RoomNetwork != null)
        {
            Debug.Log("Using RoomNetwork port creation path");
            DetectPortsWithPowerNodes();
            CreateGroupedInputPorts(inputSection);
            CreateConnectionMatrix(portsContainer);
        }
        else
        {
            Debug.Log("Using simple port creation path");
            CreateRegularInputPorts(inputSection);
        }

        // Create output ports section
        var outputSection = new VisualElement();
        outputSection.AddToClassList("output-ports-section");
        portsContainer.Add(outputSection);

        // Create static waypoint output ports FIRST
        CreateWaypointOutputPorts(outputSection);

        // Then create regular output ports
        if (Node.RoomNetwork != null)
        {
            CreateGroupedOutputPorts(outputSection);
        }
        else
        {
            CreateRegularOutputPorts(outputSection);
        }

        Debug.Log($"Final port counts - Input ports created: {_inputPorts.Count(p => p != null)}, Output ports created: {_outputPorts.Count(p => p != null)}");
        Debug.Log($"=== End CreatePorts for {Node.Name} ===");
    }

    protected virtual void CreatePorts_()
    {
        var portsContainer = new VisualElement();
        portsContainer.AddToClassList("ports-container");
        _mainContent.Add(portsContainer);

        // Clear existing ports
        _inputPorts.Clear();
        _outputPorts.Clear();

        // Initialize lists with nulls for all expected ports (including waypoint ports)
        for (int i = 0; i < Node.InputPortCount; i++)
        {
            _inputPorts.Add(null);
        }
        for (int i = 0; i < Node.OutputPortCount; i++)
        {
            _outputPorts.Add(null);
        }

        // Create input ports section
        var inputSection = new VisualElement();
        inputSection.AddToClassList("input-ports-section");
        portsContainer.Add(inputSection);

        // Create static waypoint input ports FIRST
        CreateWaypointInputPorts(inputSection);

        // Then create regular ports based on node type
        if (Node.RoomNetwork != null)
        {
            DetectPortsWithPowerNodes();
            CreateGroupedInputPorts(inputSection);
            CreateConnectionMatrix(portsContainer);
        }
        else
        {
            CreateRegularInputPorts(inputSection);
        }

        // Create output ports section
        var outputSection = new VisualElement();
        outputSection.AddToClassList("output-ports-section");
        portsContainer.Add(outputSection);

        // Create static waypoint output ports FIRST
        CreateWaypointOutputPorts(outputSection);

        // Then create regular output ports
        if (Node.RoomNetwork != null)
        {
            CreateGroupedOutputPorts(outputSection);
        }
        else
        {
            CreateRegularOutputPorts(outputSection);
        }
    }

    private void CreateWaypointOutputPorts(VisualElement outputSection)
    {
        var waypointOutputs = Node.GetWaypointOutputPorts();

        if (waypointOutputs.Count > 0)
        {
            var waypointSection = new VisualElement();
            waypointSection.AddToClassList("waypoint-ports-section");
            waypointSection.AddToClassList("waypoint-output-section");
            outputSection.Add(waypointSection);

            var sectionHeader = new Label("Internal");
            sectionHeader.AddToClassList("waypoint-section-header");
            sectionHeader.style.color = new Color(0.8f, 0.4f, 0.8f); // Purple for waypoint ports
            sectionHeader.style.unityFontStyleAndWeight = FontStyle.Bold;
            waypointSection.Add(sectionHeader);

            for (int i = 0; i < waypointOutputs.Count; i++)
            {
                CreateWaypointOutputPort(waypointSection, i, waypointOutputs[i]);
            }
        }
    }

    private void CreateWaypointInputPort(VisualElement container, int waypointIndex, NetworkNode.WaypointPortInfo waypointInfo)
    {
        var inputPort = new VisualElement();
        inputPort.AddToClassList("port");
        inputPort.AddToClassList("input-port");
        inputPort.AddToClassList("waypoint-port"); // Special styling
        inputPort.style.backgroundColor = new Color(0.6f, 0.2f, 0.6f, 0.8f); // Purple background

        inputPort.tooltip = $"Waypoint: {waypointInfo.waypointName}\nRoom: {waypointInfo.connectedRoom?.roomName ?? "Unknown"}";

        container.Add(inputPort);

        // Waypoint ports use indices 0 to waypointCount-1
        _inputPorts[waypointIndex] = inputPort;

        inputPort.RegisterCallback<MouseDownEvent>(evt => OnInputPortMouseDown(evt, waypointIndex));
        inputPort.RegisterCallback<MouseUpEvent>(evt => OnInputPortMouseUp(evt, waypointIndex));
    }

    private void CreateWaypointOutputPort(VisualElement container, int waypointIndex, NetworkNode.WaypointPortInfo waypointInfo)
    {
        var outputPort = new VisualElement();
        outputPort.AddToClassList("port");
        outputPort.AddToClassList("output-port");
        outputPort.AddToClassList("waypoint-port"); // Special styling
        outputPort.style.backgroundColor = new Color(0.6f, 0.2f, 0.6f, 0.8f); // Purple background

        outputPort.tooltip = $"Waypoint: {waypointInfo.waypointName}\nRoom: {waypointInfo.connectedRoom?.roomName ?? "Unknown"}";

        container.Add(outputPort);

        // Waypoint ports use indices 0 to waypointCount-1
        _outputPorts[waypointIndex] = outputPort;

        outputPort.RegisterCallback<MouseDownEvent>(evt => OnOutputPortMouseDown(evt, waypointIndex));
        outputPort.RegisterCallback<MouseUpEvent>(evt => OnOutputPortMouseUp(evt, waypointIndex));
    }

    private void CreateRegularInputPorts(VisualElement inputSection)
    {
        var waypointInputCount = Node.GetWaypointInputPorts().Count;
        for (int i = 0; i < Node.InputPortCount; i++)
        {
            CreateInputPort(inputSection, waypointInputCount + i);
        }
    }

    private void CreateRegularOutputPorts(VisualElement outputSection)
    {
        var waypointOutputCount = Node.GetWaypointOutputPorts().Count;
        for (int i = 0; i < Node.OutputPortCount; i++)
        {
            CreateOutputPort(outputSection, waypointOutputCount + i);
        }
    }

    private void CreateWaypointInputPorts(VisualElement inputSection)
    {
        var waypointInputs = Node.GetWaypointInputPorts();

        if (waypointInputs.Count > 0)
        {
            var waypointSection = new VisualElement();
            waypointSection.AddToClassList("waypoint-ports-section");
            waypointSection.AddToClassList("waypoint-input-section");
            inputSection.Add(waypointSection);

            var sectionHeader = new Label("Internal");
            sectionHeader.AddToClassList("waypoint-section-header");
            sectionHeader.style.color = new Color(0.8f, 0.4f, 0.8f); // Purple for waypoint ports
            sectionHeader.style.unityFontStyleAndWeight = FontStyle.Bold;
            waypointSection.Add(sectionHeader);

            for (int i = 0; i < waypointInputs.Count; i++)
            {
                CreateWaypointInputPort(waypointSection, i, waypointInputs[i]);
            }
        }
    }

    protected virtual void CreateSimplePorts(VisualElement portsContainer)
    {
        // Create input ports section
        var inputSection = new VisualElement();
        inputSection.AddToClassList("input-ports-section");
        portsContainer.Add(inputSection);

        for (int i = 0; i < Node.InputPortCount; i++)
        {
            CreateInputPort(inputSection, i);
        }

        // Create output ports section
        var outputSection = new VisualElement();
        outputSection.AddToClassList("output-ports-section");
        portsContainer.Add(outputSection);

        for (int i = 0; i < Node.OutputPortCount; i++)
        {
            CreateOutputPort(outputSection, i);
        }
    }

    public void UpdatePortStyling(CableSize? filterSize)
    {
        var inputPorts = Node.GetInputPorts();
        for (int i = 0; i < _inputPorts.Count && i < inputPorts.Count; i++)
        {
            if (_inputPorts[i] != null)
            {
                var portInfo = inputPorts[i];
                bool isActive = filterSize == null || portInfo.assignedCableSize == filterSize;

                _inputPorts[i].style.opacity = isActive ? 1.0f : 0.3f;
                _inputPorts[i].SetEnabled(isActive);
            }
        }

        var outputPorts = Node.GetOutputPorts();
        for (int i = 0; i < _outputPorts.Count && i < outputPorts.Count; i++)
        {
            if (_outputPorts[i] != null)
            {
                var portInfo = outputPorts[i];
                bool isActive = filterSize == null || portInfo.assignedCableSize == filterSize;

                _outputPorts[i].style.opacity = isActive ? 1.0f : 0.3f;
                _outputPorts[i].SetEnabled(isActive);
            }
        }
    }

    public void UpdatePowerSystemPortStyling(bool inputsActive, bool outputsActive)
    {
        // Update all input ports
        foreach (var inputPort in _inputPorts)
        {
            if (inputPort != null)
            {
                inputPort.style.opacity = inputsActive ? 1.0f : 0.3f;
                inputPort.SetEnabled(inputsActive);
            }
        }

        // Update all output ports
        foreach (var outputPort in _outputPorts)
        {
            if (outputPort != null)
            {
                outputPort.style.opacity = outputsActive ? 1.0f : 0.3f;
                outputPort.SetEnabled(outputsActive);
            }
        }
    }

    private void DetectPortsWithPowerNodes()
    {
        if (Node.RoomNetwork == null) return;

        // Clear previous groupings
        _inputPortsByPowerNode.Clear();
        _outputPortsByPowerNode.Clear();
        _ungroupedInputPorts.Clear();
        _ungroupedOutputPorts.Clear();

        // Don't check the rooms that have no nodes
        if (Node.RoomNetwork.nodes.Count > 0)
        {
            // Get the actual input and output port lists
            var inputPortInfos = Node.GetInputPorts();
            var outputPortInfos = Node.GetOutputPorts();

            // Check input ports
            for (int i = 0; i < inputPortInfos.Count; i++)
            {
                var portInfo = inputPortInfos[i];
                var connectedPowerNode = GetConnectedPowerNode(portInfo);

                if (connectedPowerNode != null)
                {
                    if (!_inputPortsByPowerNode.ContainsKey(connectedPowerNode))
                        _inputPortsByPowerNode[connectedPowerNode] = new HashSet<int>();
                    _inputPortsByPowerNode[connectedPowerNode].Add(i);
                    Debug.Log($"Input port {i} ({portInfo.portId}) connects to PowerNode {connectedPowerNode.nodeId}");
                }
                else
                {
                    _ungroupedInputPorts.Add(i);
                }
            }

            // Check output ports
            for (int i = 0; i < outputPortInfos.Count; i++)
            {
                var portInfo = outputPortInfos[i];
                var connectedPowerNode = GetConnectedPowerNode(portInfo);

                if (connectedPowerNode != null)
                {
                    if (!_outputPortsByPowerNode.ContainsKey(connectedPowerNode))
                        _outputPortsByPowerNode[connectedPowerNode] = new HashSet<int>();
                    _outputPortsByPowerNode[connectedPowerNode].Add(i);
                    Debug.Log($"Output port {i} ({portInfo.portId}) connects to PowerNode {connectedPowerNode.nodeId}");
                }
                else
                {
                    _ungroupedOutputPorts.Add(i);
                }
            }

            // Debug output for grouped ports
            foreach (var kvp in _inputPortsByPowerNode)
            {
                Debug.Log($"Node {Node.Name} - PowerNode {kvp.Key.nodeId} input ports: [{string.Join(", ", kvp.Value)}]");
            }

            foreach (var kvp in _outputPortsByPowerNode)
            {
                Debug.Log($"Node {Node.Name} - PowerNode {kvp.Key.nodeId} output ports: [{string.Join(", ", kvp.Value)}]");
            }
        }
        else
        {
            // No PowerNodes, so all ports are ungrouped
            for (int i = 0; i < Node.InputPortCount; i++)
            {
                _ungroupedInputPorts.Add(i);
            }
            for (int i = 0; i < Node.OutputPortCount; i++)
            {
                _ungroupedOutputPorts.Add(i);
            }
        }
    }

    private void CreateGroupedInputPorts(VisualElement inputSection)
    {
        // Create PowerNode groups first
        foreach (var kvp in _inputPortsByPowerNode)
        {
            var powerNode = kvp.Key;
            var portIndices = kvp.Value.OrderBy(x => x).ToList();

            var powerNodeGroup = new VisualElement();
            powerNodeGroup.AddToClassList("powernode-port-group");
            powerNodeGroup.AddToClassList("input-powernode-group");
            inputSection.Add(powerNodeGroup);

            // Group header
            var groupHeader = new Label($"{powerNode.nodeId}");
            groupHeader.AddToClassList("powernode-group-header");
            powerNodeGroup.Add(groupHeader);

            // Ports container within the group
            var groupPortsContainer = new VisualElement();
            groupPortsContainer.AddToClassList("powernode-ports-container");
            powerNodeGroup.Add(groupPortsContainer);

            foreach (var portIndex in portIndices)
            {
                CreateInputPort(groupPortsContainer, portIndex);
            }
        }

        // Create ungrouped ports directly in the section (no separate container)
        foreach (var portIndex in _ungroupedInputPorts.OrderBy(x => x))
        {
            CreateInputPort(inputSection, portIndex);
        }
    }
    
    private void CreateGroupedOutputPorts(VisualElement outputSection)
    {
        // Create PowerNode groups first
        foreach (var kvp in _outputPortsByPowerNode)
        {
            var powerNode = kvp.Key;
            var portIndices = kvp.Value.OrderBy(x => x).ToList();

            var powerNodeGroup = new VisualElement();
            powerNodeGroup.AddToClassList("powernode-port-group");
            powerNodeGroup.AddToClassList("output-powernode-group");
            outputSection.Add(powerNodeGroup);

            // Group header
            var groupHeader = new Label($"{powerNode.nodeId}");
            groupHeader.AddToClassList("powernode-group-header");
            powerNodeGroup.Add(groupHeader);

            // Ports container within the group
            var groupPortsContainer = new VisualElement();
            groupPortsContainer.AddToClassList("powernode-ports-container");
            powerNodeGroup.Add(groupPortsContainer);

            foreach (var portIndex in portIndices)
            {
                CreateOutputPort(groupPortsContainer, portIndex);
            }
        }

        // Create ungrouped ports directly in the section (no separate container)
        foreach (var portIndex in _ungroupedOutputPorts.OrderBy(x => x))
        {
            CreateOutputPort(outputSection, portIndex);
        }
    }

    //A protected virtual
    protected virtual void CreateInputPort(VisualElement container, int portIndex)
    {
        var inputPort = new VisualElement();
        inputPort.AddToClassList("port");
        inputPort.AddToClassList("input-port");

        // Set tooltip if we have boundary port info
        var inputPorts = Node.GetInputPorts();
        if (portIndex < inputPorts.Count)
        {
            var portInfo = inputPorts[portIndex];
            inputPort.tooltip = $"{portInfo.boundaryName}\n{(portInfo.assignedCableSize?.ToString() ?? "Unassigned")}";
        }

        container.Add(inputPort);

        // Ensure the list is large enough and set the port at the correct index
        while (_inputPorts.Count <= portIndex)
        {
            _inputPorts.Add(null);
        }
        _inputPorts[portIndex] = inputPort;

        inputPort.RegisterCallback<MouseDownEvent>(evt => OnInputPortMouseDown(evt, portIndex));
        inputPort.RegisterCallback<MouseUpEvent>(evt => OnInputPortMouseUp(evt, portIndex));
    }

    //A protected virtual
    // Fixed CreateOutputPort method to handle list sizing properly
    protected virtual void CreateOutputPort(VisualElement container, int portIndex)
    {
        var outputPort = new VisualElement();
        outputPort.AddToClassList("port");
        outputPort.AddToClassList("output-port");

        // Set tooltip if we have boundary port info
        var outputPorts = Node.GetOutputPorts();
        if (portIndex < outputPorts.Count)
        {
            var portInfo = outputPorts[portIndex];
            outputPort.tooltip = $"{portInfo.boundaryName}\n{(portInfo.assignedCableSize?.ToString() ?? "Unassigned")}";
        }

        container.Add(outputPort);

        // Ensure the list is large enough and set the port at the correct index
        while (_outputPorts.Count <= portIndex)
        {
            _outputPorts.Add(null);
        }
        _outputPorts[portIndex] = outputPort;

        outputPort.RegisterCallback<MouseDownEvent>(evt => OnOutputPortMouseDown(evt, portIndex));
        outputPort.RegisterCallback<MouseUpEvent>(evt => OnOutputPortMouseUp(evt, portIndex));
    }

    // Add this method to NodeView class
    private void CreateConnectionMatrix(VisualElement portsContainer)
    {
        // Only add the matrix if we have PowerNode ports
        if (_inputPortsByPowerNode.Count == 0 && _outputPortsByPowerNode.Count == 0)
            return;

        // Create a container for the matrix
        var matrixSection = new VisualElement();
        matrixSection.AddToClassList("matrix-section");
        portsContainer.Add(matrixSection);

        // Create a header for the matrix
        var matrixHeader = new Label("PowerNode Routing");
        matrixHeader.AddToClassList("matrix-header");
        matrixSection.Add(matrixHeader);

        // Get all PowerNodes with either input or output ports
        var allPowerNodes = new HashSet<PowerNode>();
        foreach (var powerNode in _inputPortsByPowerNode.Keys)
            allPowerNodes.Add(powerNode);
        foreach (var powerNode in _outputPortsByPowerNode.Keys)
            allPowerNodes.Add(powerNode);

        // Create a matrix for each PowerNode
        foreach (var powerNode in allPowerNodes)
        {
            CreatePowerNodeMatrix(matrixSection, powerNode);
        }
    }

    private void CreatePowerNodeMatrix(VisualElement container, PowerNode powerNode)
    {
        if (powerNode == null) return;

        // Create a container for this PowerNode's matrix
        var powerNodeContainer = new VisualElement();
        powerNodeContainer.AddToClassList("powernode-matrix-container");
        container.Add(powerNodeContainer);

        // Add PowerNode identifier
        var nodeIdLabel = new Label($"Node: {powerNode.nodeId}");
        nodeIdLabel.AddToClassList("powernode-matrix-id");
        powerNodeContainer.Add(nodeIdLabel);

        // Create routing matrix visualization
        var routingMatrix = new VisualElement();
        routingMatrix.AddToClassList("routing-matrix");
        powerNodeContainer.Add(routingMatrix);

        // Create a grid of elements showing the routing configuration
        var matrixContainer = new VisualElement();
        matrixContainer.AddToClassList("matrix-grid");
        routingMatrix.Add(matrixContainer);

        // Header row with output labels
        var headerRow = new VisualElement();
        headerRow.AddToClassList("matrix-row");
        headerRow.AddToClassList("matrix-header-row");
        matrixContainer.Add(headerRow);

        // Empty corner cell
        var cornerCell = new VisualElement();
        cornerCell.AddToClassList("matrix-cell");
        cornerCell.AddToClassList("matrix-corner-cell");
        headerRow.Add(cornerCell);

        // Output labels
        for (int o = 0; o < powerNode.outputPoints.Count; o++)
        {
            var outputLabel = new Label($"Out {o}");
            outputLabel.AddToClassList("matrix-cell");
            outputLabel.AddToClassList("matrix-output-label");
            headerRow.Add(outputLabel);
        }

        // Create matrix rows
        for (int i = 0; i < powerNode.inputPoints.Count; i++)
        {
            var row = new VisualElement();
            row.AddToClassList("matrix-row");
            matrixContainer.Add(row);

            // Input label
            var inputLabel = new Label($"In {i}");
            inputLabel.AddToClassList("matrix-cell");
            inputLabel.AddToClassList("matrix-input-label");
            row.Add(inputLabel);

            // Connection buttons
            for (int o = 0; o < powerNode.outputPoints.Count; o++)
            {
                bool isConnected = powerNode.IsInputConnectedToOutput(i, o);

                // Capture indices for closure
                int inputIndex = i;
                int outputIndex = o;

                // Create a button for toggling the connection
                var routeButton = new Button(() => ToggleRoute(powerNode, inputIndex, outputIndex));
                routeButton.text = isConnected ? "●" : "○";
                routeButton.AddToClassList("matrix-cell");
                routeButton.AddToClassList("matrix-route-button");

                if (isConnected)
                    routeButton.AddToClassList("matrix-route-connected");

                row.Add(routeButton);
            }

            // Current route indicator
            var route = powerNode.GetRouteFromInput(i);
            if (route != null && route.isConnected)
            {
                var routeIndicator = new Label($"→ {route.outputIndex}");
                routeIndicator.AddToClassList("matrix-route-active");
                row.Add(routeIndicator);
            }
            else
            {
                var routeIndicator = new Label("(Disconnected)");
                routeIndicator.AddToClassList("matrix-route-inactive");
                row.Add(routeIndicator);
            }
        }

        // Add reset button
        var resetButton = new Button(() => ResetRouting(powerNode));
        resetButton.text = "Reset to Default";
        resetButton.AddToClassList("matrix-reset-button");
        routingMatrix.Add(resetButton);
    }

    private void ToggleRoute(PowerNode powerNode, int inputIndex, int outputIndex)
    {
        if (powerNode == null)
            return;

        bool isCurrentlyConnected = powerNode.IsInputConnectedToOutput(inputIndex, outputIndex);

        if (isCurrentlyConnected)
        {
            // Disconnect
            powerNode.SetRoute(inputIndex, outputIndex, false);
        }
        else
        {
            // Connect (will automatically disconnect any existing connections)
            powerNode.SetRoute(inputIndex, outputIndex, true);
        }

        // Refresh the UI
        RefreshPowerNodeMatrix();
    }

    private void ResetRouting(PowerNode powerNode)
    {
        if (powerNode == null)
            return;

        // Reset to default routing (each input to corresponding output)
        for (int i = 0; i < powerNode.connectionCount; i++)
        {
            powerNode.SetRoute(i, i, true);
        }

        // Refresh the UI
        RefreshPowerNodeMatrix();
    }

    private void RefreshPowerNodeMatrix()
    {
        // The simplest way to refresh is to recreate the ports section
        if (_mainContent != null)
        {
            // Find and remove the existing ports container
            var existingPortsContainer = _mainContent.Q<VisualElement>(className: "ports-container");
            if (existingPortsContainer != null)
            {
                _mainContent.Remove(existingPortsContainer);
            }

            // Recreate the ports
            CreatePorts();
        }
    }

    protected virtual void CreateExpandedContent()
    {
        _expandedContent = new VisualElement();
        _expandedContent.AddToClassList("expanded-content");
        _expandedContent.style.display = DisplayStyle.None;
        _body.Add(_expandedContent);

        // Always create basic structure, even if no RoomNetwork
        CreateStatsSection();
        CreateBoundaryPortsSection();
        CreateTemplatesSection();
    }

    private void CreateStatsSection()
    {
        var statsContainer = new VisualElement();
        statsContainer.AddToClassList("stats-container");
        _expandedContent.Add(statsContainer);

        if (Node.RoomNetwork != null)
        {
            var totalPathsLabel = new Label($"Paths: {Node.GetUsedPathCount()}/{Node.GetTotalPathCapacity()}");
            totalPathsLabel.AddToClassList("stats-label");
            statsContainer.Add(totalPathsLabel);

            var totalCablesLabel = new Label($"Cables: {Node.GetCurrentCableCount()}/{Node.GetTotalCableCapacity()}");
            totalCablesLabel.AddToClassList("stats-label");
            statsContainer.Add(totalCablesLabel);

            var overallUtilLabel = new Label($"Overall Utilization: {Node.GetOverallUtilization():F1}%");
            overallUtilLabel.AddToClassList("stats-label");
            statsContainer.Add(overallUtilLabel);
        }
        else
        {
            var noDataLabel = new Label("No RoomNetwork connected");
            noDataLabel.AddToClassList("stats-label");
            noDataLabel.style.color = Color.yellow;
            statsContainer.Add(noDataLabel);
        }
    }

    private void CreateBoundaryPortsSection()
    {
        var boundarySection = new VisualElement();
        boundarySection.AddToClassList("section");
        _expandedContent.Add(boundarySection);

        var boundaryHeader = new Label($"Boundary Ports ({Node.boundaryPortInfos.Count})");
        boundaryHeader.AddToClassList("section-header");
        boundarySection.Add(boundaryHeader);

        if (Node.boundaryPortInfos.Count > 0)
        {
            foreach (var portInfo in Node.boundaryPortInfos)
            {
                var portElement = new VisualElement();
                portElement.AddToClassList("port-info");
                portElement.style.flexDirection = FlexDirection.Row;
                portElement.style.justifyContent = Justify.SpaceBetween;
                boundarySection.Add(portElement);

                var portLabel = new Label($"{portInfo.boundaryName} ({(portInfo.isInput ? "IN" : "OUT")})");
                portLabel.AddToClassList("port-label");
                portElement.Add(portLabel);

                var portStatus = new Label(portInfo.isConnected ?
                    portInfo.assignedCableSize?.ToString() ?? "Connected" : "Disconnected");
                portStatus.AddToClassList("port-status");
                portStatus.style.color = portInfo.isConnected ? Color.green : Color.gray;
                portElement.Add(portStatus);
            }
        }
        else
        {
            var noBoundaryLabel = new Label("No boundary ports found");
            noBoundaryLabel.AddToClassList("port-label");
            noBoundaryLabel.style.color = Color.gray;
            boundarySection.Add(noBoundaryLabel);
        }
    }

    private void CreateTemplatesSection()
    {
        var templatesSection = new VisualElement();
        templatesSection.AddToClassList("section");
        _expandedContent.Add(templatesSection);

        var templatesHeader = new Label($"Templates ({Node.templateInfos.Count})");
        templatesHeader.AddToClassList("section-header");
        templatesSection.Add(templatesHeader);

        if (Node.templateInfos.Count > 0)
        {
            foreach (var templateInfo in Node.templateInfos)
            {
                var templateElement = new VisualElement();
                templateElement.AddToClassList("template-info");
                templatesSection.Add(templateElement);

                var templateHeader = new VisualElement();
                templateHeader.style.flexDirection = FlexDirection.Row;
                templateHeader.style.justifyContent = Justify.SpaceBetween;
                templateElement.Add(templateHeader);

                var templateName = new Label($"{templateInfo.templateName} ({templateInfo.templateType})");
                templateName.AddToClassList("template-name");
                templateHeader.Add(templateName);

                var templateUtilization = new Label($"{templateInfo.utilizationPercentage:F1}%");
                templateUtilization.AddToClassList("template-utilization");
                templateUtilization.style.color = templateInfo.utilizationPercentage > 80 ? Color.red :
                                                templateInfo.utilizationPercentage > 60 ? Color.yellow : Color.green;
                templateHeader.Add(templateUtilization);

                var templateDetails = new VisualElement();
                templateDetails.AddToClassList("template-details");
                templateElement.Add(templateDetails);

                var pathsLabel = new Label($"Paths: {templateInfo.usedPaths}/{templateInfo.maxPaths}");
                pathsLabel.AddToClassList("template-detail-label");
                templateDetails.Add(pathsLabel);

                var cablesLabel = new Label($"Cables: {templateInfo.currentCableCount}/{templateInfo.maxCableCapacity}");
                cablesLabel.AddToClassList("template-detail-label");
                templateDetails.Add(cablesLabel);

                if (templateInfo.supportedSizes.Count > 0)
                {
                    var supportedSizesLabel = new Label($"Supported: {string.Join(", ", templateInfo.supportedSizes)}");
                    supportedSizesLabel.AddToClassList("template-detail-label");
                    templateDetails.Add(supportedSizesLabel);
                }

                if (templateInfo.assignedSizes.Count > 0)
                {
                    var assignedSizesLabel = new Label($"Assigned: {string.Join(", ", templateInfo.assignedSizes)}");
                    assignedSizesLabel.AddToClassList("template-detail-label");
                    templateDetails.Add(assignedSizesLabel);
                }
            }
        }
        else
        {
            var noTemplatesLabel = new Label("No templates found");
            noTemplatesLabel.AddToClassList("template-name");
            noTemplatesLabel.style.color = Color.gray;
            templatesSection.Add(noTemplatesLabel);
        }
    }

    private PowerNode GetConnectedPowerNode(NetworkNode.BoundaryPortInfo portInfo)
    {
        if (Node.RoomNetwork?.boundaryPorts == null) return null;

        // Find the actual boundary port object
        var boundaryPort = Node.RoomNetwork.boundaryPorts.Find(bp => bp.portId == portInfo.portId);
        if (boundaryPort?.activeConnection == null)
        {
            Debug.Log($"No active connection found for port {portInfo.portId} in room {Node.Name}");
            return null;
        }

        var connection = boundaryPort.activeConnection;
        Debug.Log($"Checking port {portInfo.portId} in room {Node.Name} - connected to template {connection.connectedTemplate?.name}");

        // Only check if the connected waypoint path has PowerNode connections
        if (connection.connectedPath != null && connection.connectedTemplate != null)
        {
            Debug.Log($"Checking entry point: {connection.connectedPath.entryPoint.position}");
            var powerNode = GetPowerNodeFromConnectionPoint(connection.connectedPath.entryPoint);
            if (powerNode != null && IsTemplateConnectedToPowerNode(connection.connectedTemplate, powerNode))
            {
                Debug.Log($"Port {portInfo.portId} connects to PowerNode {powerNode.nodeId} via entry point");
                return powerNode;
            }

            Debug.Log($"Checking exit point: {connection.connectedPath.exitPoint.position}");
            powerNode = GetPowerNodeFromConnectionPoint(connection.connectedPath.exitPoint);
            if (powerNode != null && IsTemplateConnectedToPowerNode(connection.connectedTemplate, powerNode))
            {
                Debug.Log($"Port {portInfo.portId} connects to PowerNode {powerNode.nodeId} via exit point");
                return powerNode;
            }
        }

        Debug.Log($"No PowerNode connection found for port {portInfo.portId}");
        return null;
    }

    private bool IsTemplateConnectedToPowerNode(Template template, PowerNode powerNode)
    {
        if (template == null || powerNode == null) return false;

        Debug.Log($"Checking if template {template.name} is connected to PowerNode {powerNode.nodeId}");

        // Check if any of the PowerNode's active connections reference this template
        foreach (var activeConnection in powerNode.activeConnections)
        {
            if (activeConnection.connectedTemplate == template)
            {
                Debug.Log($"Template {template.name} IS connected to PowerNode {powerNode.nodeId}");
                return true;
            }
        }

        Debug.Log($"Template {template.name} is NOT connected to PowerNode {powerNode.nodeId}");
        return false;
    }
    
    // Fixed GetPowerNodeFromConnectionPoint method to be more precise
    private PowerNode GetPowerNodeFromConnectionPoint(ConnectionPoint connectionPoint)
    {
        if (connectionPoint == null) return null;

        // Find all PowerNodes in the scene and check if any connect to this point
        var powerNodes = UnityEngine.Object.FindObjectsByType<PowerNode>(FindObjectsSortMode.None);

        foreach (var powerNode in powerNodes)
        {
            // Check active connections first (more reliable than position matching)
            foreach (var activeConnection in powerNode.activeConnections)
            {
                if (activeConnection.connectedPath != null)
                {
                    // Check if this connection point is part of the connected path
                    var pathPoints = activeConnection.connectedPath.GetAllPoints();
                    foreach (var pathPoint in pathPoints)
                    {
                        if (Vector3.Distance(pathPoint.position, connectionPoint.position) < 0.01f)
                        {
                            Debug.Log($"Found PowerNode {powerNode.nodeId} connected via active connection to point at {connectionPoint.position}");
                            return powerNode;
                        }
                    }
                }
            }

            // Fallback to checking input/output points directly
            foreach (var inputPoint in powerNode.inputPoints)
            {
                if (Vector3.Distance(inputPoint.position, connectionPoint.position) < 0.01f)
                {
                    Debug.Log($"Found PowerNode {powerNode.nodeId} connected via input point to {connectionPoint.position}");
                    return powerNode;
                }
            }

            foreach (var outputPoint in powerNode.outputPoints)
            {
                if (Vector3.Distance(outputPoint.position, connectionPoint.position) < 0.01f)
                {
                    Debug.Log($"Found PowerNode {powerNode.nodeId} connected via output point to {connectionPoint.position}");
                    return powerNode;
                }
            }
        }

        return null;
    }

    private void ToggleExpanded()
    {
        _isExpanded = !_isExpanded;
        _expandedContent.style.display = _isExpanded ? DisplayStyle.Flex : DisplayStyle.None;
        _expandButton.text = _isExpanded ? "▲" : "▼";
    }

    public void UpdatePosition()
    {
        style.left = Node.Position.x;
        style.top = Node.Position.y;
    }

    public void SetSelected(bool selected)
    {
        if (selected)
            AddToClassList("selected");
        else
            RemoveFromClassList("selected");
    }

    public Vector2 GetInputPortPosition(int portIndex = 0)
    {
        if (portIndex < 0 || portIndex >= _inputPorts.Count)
        {
            if (_inputPorts.Count > 0)
                portIndex = 0;
            else
                return Vector2.zero;
        }

        // Check if the port element exists and is not null
        if (_inputPorts[portIndex] == null)
        {
            // Try to find the first non-null port
            for (int i = 0; i < _inputPorts.Count; i++)
            {
                if (_inputPorts[i] != null)
                {
                    return _inputPorts[i].worldBound.center;
                }
            }
            return Vector2.zero;
        }

        return _inputPorts[portIndex].worldBound.center;
    }

    public Vector2 GetOutputPortPosition(int portIndex = 0)
    {
        if (portIndex < 0 || portIndex >= _outputPorts.Count)
        {
            if (_outputPorts.Count > 0)
                portIndex = 0;
            else
                return Vector2.zero;
        }

        // Check if the port element exists and is not null
        if (_outputPorts[portIndex] == null)
        {
            // Try to find the first non-null port
            for (int i = 0; i < _outputPorts.Count; i++)
            {
                if (_outputPorts[i] != null)
                {
                    return _outputPorts[i].worldBound.center;
                }
            }
            return Vector2.zero;
        }

        return _outputPorts[portIndex].worldBound.center;
    }

    private void RegisterCallbacks()
    {
        this.RegisterCallback<MouseDownEvent>(OnMouseDown);
        this.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        this.RegisterCallback<MouseUpEvent>(OnMouseUp);
    }

    private void OnMouseDown(MouseDownEvent evt)
    {
        if (evt.button == 0) // Left click only
        {
            OnNodeSelected?.Invoke(this);

            // Start dragging
            _isDragging = true;
            _lastMousePos = evt.mousePosition;
            this.CaptureMouse();

            OnStartDrag?.Invoke(this, evt.mousePosition);
            evt.StopPropagation();
        }
    }

    private void OnMouseMove(MouseMoveEvent evt)
    {
        if (_isDragging)
        {
            Vector2 delta = evt.mousePosition - _lastMousePos;
            _lastMousePos = evt.mousePosition;

            OnDrag?.Invoke(this, delta);
            evt.StopPropagation();
        }
    }

    private void OnMouseUp(MouseUpEvent evt)
    {
        if (_isDragging && evt.button == 0)
        {
            _isDragging = false;
            this.ReleaseMouse();

            OnEndDrag?.Invoke(this);
            evt.StopPropagation();
        }
    }

    //A protected
    protected void OnInputPortMouseDown(MouseDownEvent evt, int portIndex)
    {
        if (evt.button == 0)
        {
            OnStartConnection?.Invoke(this, true, portIndex);
            evt.StopPropagation();
        }
    }

    //A protected
    protected void OnInputPortMouseUp(MouseUpEvent evt, int portIndex)
    {
        if (evt.button == 0)
        {
            OnEndConnection?.Invoke(this, true, portIndex);
            evt.StopPropagation();
        }
    }

    //A protected
    protected void OnOutputPortMouseDown(MouseDownEvent evt, int portIndex)
    {
        if (evt.button == 0)
        {
            OnStartConnection?.Invoke(this, false, portIndex);
            evt.StopPropagation();
        }
    }

    //A protected
    protected void OnOutputPortMouseUp(MouseUpEvent evt, int portIndex)
    {
        if (evt.button == 0)
        {
            OnEndConnection?.Invoke(this, false, portIndex);
            evt.StopPropagation();
        }
    }
}