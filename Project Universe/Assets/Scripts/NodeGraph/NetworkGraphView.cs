using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using System.Linq;
using ProjectUniverse;
using ProjectUniverse.PowerSystem;
using UnityEditor;
using static NetworkNode;

public class NetworkGraphView : MonoBehaviour
{
    public NetworkGraph Graph;

    [Header("Component Node Creation")]
    [SerializeField] private List<string> _targetComponentTypes = new();

    private VisualElement _graphContainer;
    private VisualElement _nodesContainer;
    private VisualElement _connectionsContainer;

    private Dictionary<string, NodeView> _nodeViews = new();
    private NodeView _selectedNode;
    private NodeView _connectionStartNode;
    private bool _isConnecting;
    private ConnectionView _previewConnection;

    // Room highlighting
    private HolographicRoom _highlightedRoom;
    private Color _originalRoomColor;
    private readonly Color _highlightColor = Color.yellow;

    // Input System references
    private PlayerControls _controls;
    private Vector2 _lastMousePosition;
    private bool _isPanning;
    private float _currentZoom = 1.0f;
    private const float ZOOM_SPEED = 0.1f;
    private const float MIN_ZOOM = 0.2f;
    private const float MAX_ZOOM = 3.0f;

    private int _connectionStartPortIndex;
    private bool _connectionStartIsOutput;

    // Add new fields for view modes
    public enum ViewMode
    {
        All,
        Transmission,
        Distribution,
        Branch
    }
    private ViewMode _currentViewMode = ViewMode.All;
    private VisualElement _tabContainer;
    private Dictionary<ViewMode, Button> _tabButtons = new();

    // Color schemes for different modes
    private readonly Color _allModeColor = new(0.7f, 0.7f, 0.7f);
    private readonly Color _transmissionColor = new(0.8f, 0.2f, 0.2f);
    private readonly Color _distributionColor = new(0.2f, 0.4f, 0.8f);
    private readonly Color _branchColor = new(0.2f, 0.7f, 0.3f);

    // Store connection information to properly track port-to-port connections
    [System.Serializable]
    public class ConnectionInfo
    {
        public string outputNodeId;
        public string inputNodeId;
        public int outputPortIndex;
        public int inputPortIndex;

        public ConnectionInfo(string outputId, string inputId, int outputPort, int inputPort)
        {
            outputNodeId = outputId;
            inputNodeId = inputId;
            outputPortIndex = outputPort;
            inputPortIndex = inputPort;
        }
    }
    private CableSize? _connectionFilter = null; // null means "All"
    private DropdownField _connectionFilterDropdown;

    private bool isSelectingPathSource = false;
    private bool isSelectingPathDestination = false;
    private Component pathSourceComponent;
    private Component pathDestinationComponent;
    private Button pathFindingButton;
    private Button clearPathButton;
    private int matrixLimit = 5;

    private void Awake()
    {
        _controls = new PlayerControls();
    }

    private void OnEnable()
    {
        _controls.Enable();

        var root = GetComponent<UIDocument>().rootVisualElement;

        // Set graph view to left half of screen
        root.Q<VisualElement>("root").style.width = new Length(50, LengthUnit.Percent);
        root.Q<VisualElement>("root").style.height = new Length(100, LengthUnit.Percent);

        _graphContainer = root.Q<VisualElement>("graph-container");
        _nodesContainer = root.Q<VisualElement>("nodes-container");
        _connectionsContainer = root.Q<VisualElement>("connections-container");

        // Create tab container before other toolbar elements
        CreateViewModeTabs(root);

        var deleteButton = root.Q<Button>("delete-node-button");
        deleteButton.clicked += OnDeleteNodeClicked;

        var scanButton = root.Q<Button>("scan-components-button");
        if (scanButton != null)
            scanButton.clicked += OnScanComponentsClicked;

        var scanRoomsButton = root.Q<Button>("scan-rooms-button");
        if (scanRoomsButton != null)
            scanRoomsButton.clicked += OnScanRoomsClicked;

        var connectBoundaryPortsButton = root.Q<Button>("connect-boundary-ports-button");
        if (connectBoundaryPortsButton != null)
            connectBoundaryPortsButton.clicked += OnConnectBoundaryPortsClicked;
        else
        {
            // If button doesn't exist in UI, create it
            var toolbar = root.Q<VisualElement>("toolbar");
            if (toolbar != null)
            {
                connectBoundaryPortsButton = new Button(() => OnConnectBoundaryPortsClicked())
                {
                    text = "Auto-Connect Networks",
                    name = "auto-connect-button"
                };
                connectBoundaryPortsButton.AddToClassList("toolbar-button");
                toolbar.Add(connectBoundaryPortsButton);
            }
        }
        /*_connectionFilterDropdown = root.Q<DropdownField>("connection-filter-dropdown");
        if (_connectionFilterDropdown != null)
        {
            _connectionFilterDropdown.choices = new List<string> { "All", "Transmission", "Distribution", "Branch" };
            _connectionFilterDropdown.value = "All";
            _connectionFilterDropdown.RegisterValueChangedCallback(OnConnectionFilterChanged);
        }
        else
        {
            // If button doesn't exist in UI, create it
            var toolbar = root.Q<VisualElement>("toolbar");
            if (toolbar != null)
            {
                _connectionFilterDropdown = new DropdownField();
                _connectionFilterDropdown.AddToClassList("cable-filter-dropdown");
                toolbar.Add(_connectionFilterDropdown);
                var choices = new List<string> { "All", "Transmission", "Distribution", "Branch" };
                _connectionFilterDropdown.choices = new List<string> { "All", "Transmission", "Distribution", "Branch" };
                _connectionFilterDropdown.value = "All";
                _connectionFilterDropdown.RegisterValueChangedCallback(OnConnectionFilterChanged);
            }
        }*/

        pathFindingButton = root.Q<Button>("path-finding-button");
        if (pathFindingButton == null)
        {
            var toolbar = root.Q<VisualElement>("toolbar");
            if (toolbar != null)
            {
                pathFindingButton = new Button()
                {
                    text = "Path Finding Mode",
                    name = "path-finding-button"
                };
                pathFindingButton.AddToClassList("toolbar-button");
                pathFindingButton.clicked += OnPathFindingButtonClicked;
                toolbar.Add(pathFindingButton);

                clearPathButton = new Button(() => OnClearPathFinding())
                {
                    text = "Clear Paths",
                    name = "clear-path-button"
                };
                clearPathButton.AddToClassList("toolbar-button");
                clearPathButton.SetEnabled(false);
                toolbar.Add(clearPathButton);
            }
        }

        // Register input callbacks
        _controls.Player.RightClick.started += OnRightClickStarted;
        _controls.Player.RightClick.canceled += OnRightClickCanceled;
        _controls.Player.ScrollWheel.performed += OnScrollWheelPerformed;

        // Register mouse move for preview connection
        _graphContainer.RegisterCallback<MouseMoveEvent>(OnGraphMouseMove);

        // Check if Graph is null before refreshing
        if (Graph != null)
        {
            // Make sure references are resolved before displaying
            RefreshView();
        }
    }

    private void CreateViewModeTabs(VisualElement root)
    {
        var toolbar = root.Q<VisualElement>("toolbar");
        if (toolbar == null) return;

        // Create tab container
        _tabContainer = new VisualElement
        {
            name = "view-mode-tabs"
        };
        _tabContainer.AddToClassList("tab-container");
        _tabContainer.style.flexDirection = FlexDirection.Row;
        _tabContainer.style.marginLeft = 10;
        _tabContainer.style.marginRight = 10;
        _tabContainer.style.flexShrink = 0;

        // Insert at the beginning of toolbar
        //toolbar.Insert(0, _tabContainer);

        // Create a new row above the existing toolbar for tabs
        var toolbarParent = toolbar.parent;
        var tabRow = new VisualElement
        {
            name = "tab-row"
        };
        tabRow.style.flexDirection = FlexDirection.Row;
        tabRow.style.height = 35;
        tabRow.style.paddingTop = 5;
        toolbarParent.Insert(toolbarParent.IndexOf(toolbar), tabRow);
        tabRow.Add(_tabContainer);

        // Create tabs
        CreateTab(ViewMode.All, "All", _allModeColor);
        CreateTab(ViewMode.Transmission, "Transmission", _transmissionColor);
        CreateTab(ViewMode.Distribution, "Distribution", _distributionColor);
        CreateTab(ViewMode.Branch, "Branch", _branchColor);

        // Set initial active tab
        SetActiveTab(ViewMode.All);
    }

    private void CreateTab(ViewMode mode, string label, Color color)
    {
        var tab = new Button(() => OnTabClicked(mode))
        {
            text = label,
            name = $"tab-{mode}"
        };

        tab.AddToClassList("view-tab");
        tab.style.backgroundColor = color * 0.5f; // Dimmed version when not active
        tab.style.borderTopLeftRadius = 5;
        tab.style.borderTopRightRadius = 5;
        tab.style.borderBottomLeftRadius = 0;
        tab.style.borderBottomRightRadius = 0;
        tab.style.paddingLeft = 15;
        tab.style.paddingRight = 15;
        tab.style.paddingTop = 5;
        tab.style.paddingBottom = 5;
        tab.style.marginRight = 2;

        _tabContainer.Add(tab);
        _tabButtons[mode] = tab;
    }

    private void SetActiveTab(ViewMode mode)
    {
        _currentViewMode = mode;

        // Update tab appearances
        foreach (var kvp in _tabButtons)
        {
            var isActive = kvp.Key == mode;
            var baseColor = GetModeColor(kvp.Key);

            if (isActive)
            {
                kvp.Value.style.backgroundColor = baseColor;
                kvp.Value.style.borderBottomColor = Color.clear;
                kvp.Value.style.unityFontStyleAndWeight = FontStyle.Bold;
            }
            else
            {
                kvp.Value.style.backgroundColor = baseColor * 0.5f;
                kvp.Value.style.borderBottomColor = baseColor * 0.3f;
                kvp.Value.style.unityFontStyleAndWeight = FontStyle.Normal;
            }
        }

        // Apply the view mode
        ApplyViewMode();
    }

    private Color GetModeColor(ViewMode mode)
    {
        return mode switch
        {
            ViewMode.All => _allModeColor,
            ViewMode.Transmission => _transmissionColor,
            ViewMode.Distribution => _distributionColor,
            ViewMode.Branch => _branchColor,
            _ => _allModeColor
        };
    }

    private void OnTabClicked(ViewMode mode)
    {
        SetActiveTab(mode);
    }

    private void ApplyViewMode()
    {
        var filterSize = GetCableSizeForMode(_currentViewMode);

        // Update node visibility and styling
        foreach (var kvp in _nodeViews)
        {
            var nodeView = kvp.Value;
            var shouldShow = ShouldShowNode(nodeView.Node);

            nodeView.style.display = shouldShow ? DisplayStyle.Flex : DisplayStyle.None;

            if (shouldShow)
            {
                // Update port styling
                //nodeView.UpdatePortStyling(filterSize);

                // For power system nodes, determine port cable sizes based on component type
                if (nodeView is PowerSystemNodeView && IsPowerSystemComponent(nodeView.Node.ComponentType))
                {
                    UpdatePowerSystemNodePortStyling(nodeView, filterSize);
                }
                else
                {
                    // For other nodes, use actual port cable sizes
                    nodeView.UpdatePortStyling(filterSize);
                }

                if (_currentViewMode != ViewMode.All)
                {
                    // Apply mode-specific styling
                    var modeColor = GetModeColor(_currentViewMode);
                    nodeView.style.borderBottomColor = modeColor;
                    nodeView.style.borderTopColor = modeColor;
                    nodeView.style.borderLeftColor = modeColor;
                    nodeView.style.borderRightColor = modeColor;
                    nodeView.style.borderBottomWidth = 2;
                    nodeView.style.borderTopWidth = 2;
                    nodeView.style.borderLeftWidth = 2;
                    nodeView.style.borderRightWidth = 2;
                }
                else
                {
                    // Reset to default styling
                    nodeView.style.borderBottomColor = _allModeColor;
                    nodeView.style.borderTopColor = _allModeColor;
                    nodeView.style.borderLeftColor = _allModeColor;
                    nodeView.style.borderRightColor = _allModeColor;
                    nodeView.style.borderBottomWidth = 1;
                    nodeView.style.borderTopWidth = 1;
                    nodeView.style.borderLeftWidth = 1;
                    nodeView.style.borderRightWidth = 1;
                }
            }
        }

        // Refresh connections with mode-specific styling
        RefreshConnections();
    }

    private void UpdatePowerSystemNodePortStyling(NodeView nodeView, CableSize? filterSize)
    {
        var node = nodeView.Node;

        // Determine which ports should be active based on component type and filter
        bool inputsActive = false;
        bool outputsActive = false;

        switch (node.ComponentType)
        {
            case "IGenerator":
                // All ports are transmission
                inputsActive = filterSize == null || filterSize == CableSize.Transmission;
                outputsActive = filterSize == null || filterSize == CableSize.Transmission;
                break;

            case "IRouter":
                // Input ports are transmission, output ports are distribution
                inputsActive = filterSize == null || filterSize == CableSize.Transmission;
                outputsActive = filterSize == null || filterSize == CableSize.Distribution;
                break;

            case "IRoutingSubstation":
                // Input ports are distribution, output ports are branch
                inputsActive = filterSize == null || filterSize == CableSize.Distribution;
                outputsActive = filterSize == null || filterSize == CableSize.Branch;
                break;

            case "IBreakerBox":
                // All ports are branch
                inputsActive = filterSize == null || filterSize == CableSize.Branch;
                outputsActive = filterSize == null || filterSize == CableSize.Branch;
                break;
        }

        // Apply the styling
        nodeView.UpdatePowerSystemPortStyling(inputsActive, outputsActive);
    }

    private bool ShouldShowNode(NetworkNode node)
    {
        if (_currentViewMode == ViewMode.All)
            return true;

        // Special handling for power system components
        if (IsPowerSystemComponent(node.ComponentType))
        {
            return ShouldShowPowerSystemNode(node);
        }

        // Check if node has ports that match the current cable size filter
        var cableSize = GetCableSizeForMode(_currentViewMode);

        // Check input ports
        var inputPorts = node.GetInputPorts();
        foreach (var port in inputPorts)
        {
            if (port.assignedCableSize == cableSize)
                return true;
        }

        // Check output ports
        var outputPorts = node.GetOutputPorts();
        foreach (var port in outputPorts)
        {
            if (port.assignedCableSize == cableSize)
                return true;
        }

        return false;
    }
    private CableSize? GetCableSizeForMode(ViewMode mode)
    {
        return mode switch
        {
            ViewMode.Transmission => CableSize.Transmission,
            ViewMode.Distribution => CableSize.Distribution,
            ViewMode.Branch => CableSize.Branch,
            _ => null
        };
    }

    private bool ShouldShowPowerSystemNode(NetworkNode node)
    {
        switch (_currentViewMode)
        {
            case ViewMode.All:
                return true;

            case ViewMode.Transmission:
                // Show IGenerator and IRouter
                return node.ComponentType == "IGenerator" || node.ComponentType == "IRouter";

            case ViewMode.Distribution:
                // Show IRouter and IRoutingSubstation
                return node.ComponentType == "IRouter" || node.ComponentType == "IRoutingSubstation";

            case ViewMode.Branch:
                // Show IRoutingSubstation and IBreakerBox
                return node.ComponentType == "IRoutingSubstation" || node.ComponentType == "IBreakerBox";

            default:
                return false;
        }
    }

    private void OnDisable()
    {
        RestoreRoomColor();

        // Save all node states before disabling
        if (Graph != null)
        {
            Graph.SaveAllNodeStates();
        }

        _controls.Player.RightClick.started -= OnRightClickStarted;
        _controls.Player.RightClick.canceled -= OnRightClickCanceled;
        _controls.Player.ScrollWheel.performed -= OnScrollWheelPerformed;
        _controls.Disable();
    }

    private void OnPathFindingButtonClicked()
    {
        if (!isSelectingPathSource && !isSelectingPathDestination)
        {
            // Start source selection
            StartPathFindingSelection();
        }
        else
        {
            // Cancel selection
            CancelPathFindingSelection();
        }
    }

    private void StartPathFindingSelection()
    {
        isSelectingPathSource = true;
        isSelectingPathDestination = false;
        pathSourceComponent = null;
        pathDestinationComponent = null;
        pathFindingButton.text = "Cancel Selection";
        pathFindingButton.AddToClassList("selecting");

        Debug.Log("Path Finding Mode: Select source component");
    }

    private void CancelPathFindingSelection()
    {
        isSelectingPathSource = false;
        isSelectingPathDestination = false;
        pathSourceComponent = null;
        pathDestinationComponent = null;
        pathFindingButton.text = "Path Finding Mode";
        pathFindingButton.RemoveFromClassList("selecting");
    }

    private void OnClearPathFinding()
    {
        PowerSystemVisualizer.Instance.ExitPathFindingMode();
        clearPathButton.SetEnabled(false);
        CancelPathFindingSelection();

        // Clear any path finding highlights
        _nodeViews.Values.ToList().ForEach(nv => {
            nv.RemoveFromClassList("path-source");
            nv.RemoveFromClassList("path-destination");
        });
    }

    private void OnNodeSelected(NodeView nodeView)
    {
        // Handle path finding selection
        if (isSelectingPathSource || isSelectingPathDestination)
        {
            var component = GetPowerSystemComponent(nodeView.Node.SourceGameObject);

            if (component == null)
            {
                Debug.LogWarning($"Selected node '{nodeView.Node.Name}' is not a power system component. Please select a Generator, Router, Substation, or BreakerBox.");
                return;
            }

            if (isSelectingPathSource)
            {
                // Clear any previous highlights
                _nodeViews.Values.ToList().ForEach(nv => {
                    nv.RemoveFromClassList("path-source");
                    nv.RemoveFromClassList("path-destination");
                });

                pathSourceComponent = component;
                nodeView.AddToClassList("path-source");

                // Transition to destination selection
                isSelectingPathSource = false;
                isSelectingPathDestination = true;
                pathFindingButton.text = "Cancel Selection";

                Debug.Log($"Source selected: {component.name}. Now select destination component");
            }
            else if (isSelectingPathDestination)
            {
                if (component == pathSourceComponent)
                {
                    Debug.LogWarning("Cannot select the same component as both source and destination");
                    return;
                }

                pathDestinationComponent = component;
                nodeView.AddToClassList("path-destination");

                // Complete the selection
                isSelectingPathSource = false;
                isSelectingPathDestination = false;
                pathFindingButton.text = "Path Finding Mode";
                pathFindingButton.RemoveFromClassList("selecting");

                Debug.Log($"Destination selected: {component.name}. Finding paths...");

                // Set the matrix limit
                PowerSystemVisualizer.Instance.maxMatrixChanges = matrixLimit;

                // Start path finding visualization
                PowerSystemVisualizer.Instance.EnterPathFindingMode(pathSourceComponent, pathDestinationComponent);
                clearPathButton.SetEnabled(true);
            }

            return;
        }

        // Existing selection logic...
        _selectedNode?.SetSelected(false);
        RestoreRoomColor();
        _selectedNode = nodeView;
        _selectedNode.SetSelected(true);

        if (nodeView.Node.SourceRoom != null)
        {
            HighlightRoom(nodeView.Node.SourceRoom);
        }

#if UNITY_EDITOR
        if (nodeView.Node.SourceGameObject != null &&
            nodeView.Node.SourceGameObject.GetComponent<PowerNode>() != null)
        {
            Selection.activeGameObject = nodeView.Node.SourceGameObject;
        }
#endif
    }

    private void RefreshView()
    {
        if (Graph == null) return;

        // Ensure connections are restored
        Graph.RestoreConnections();

        _nodesContainer.Clear();
        _connectionsContainer.Clear();
        _nodeViews.Clear();

        foreach (var node in Graph.Nodes)
        {
            CreateNodeView(node);
        }

        RefreshConnections();
    }

    /*private void OnConnectionFilterChanged(ChangeEvent<string> evt)
    {
        switch (evt.newValue)
        {
            case "All":
                _connectionFilter = null;
                break;
            case "Transmission":
                _connectionFilter = CableSize.Transmission;
                break;
            case "Distribution":
                _connectionFilter = CableSize.Distribution;
                break;
            case "Branch":
                _connectionFilter = CableSize.Branch;
                break;
        }

        RefreshConnections();
    }*/

    private void OnDestroy()
    {
        // Ensure room colors are restored
        RestoreRoomColor();
    }

    private void Update()
    {
        // Handle panning with right-click drag
        if (_isPanning && IsMouseOverGraphView())
        {
            Vector2 lookDelta = _controls.Player.Look.ReadValue<Vector2>();
            if (lookDelta.magnitude > 0.01f)
            {
                PanGraph(lookDelta);
            }
        }
    }

    private void CreateNodeView(NetworkNode node)
    {
        NodeView nodeView;

        // Check if this is a PowerNode (deprecated)
        if (node.ComponentType == "PowerNode" && node.SourceGameObject != null &&
            node.SourceGameObject.GetComponent<PowerNode>() != null)
        {
            nodeView = new PowerNodeView(node);
            Debug.Log($"Created PowerNodeView for {node.Name}");
        }
        // Check if this is a power system component
        else if (IsPowerSystemComponent(node.ComponentType) && node.SourceGameObject != null)
        {
            nodeView = new PowerSystemNodeView(node);
            Debug.Log($"Created PowerSystemNodeView for {node.Name} ({node.ComponentType})");
        }
        else
        {
            nodeView = new NodeView(node);
        }

        _nodesContainer.Add(nodeView);
        _nodeViews[node.Id] = nodeView;

        nodeView.OnNodeSelected += OnNodeSelected;
        nodeView.OnStartConnection += OnStartConnection;
        nodeView.OnEndConnection += OnEndConnection;
        nodeView.OnStartDrag += OnNodeStartDrag;
        nodeView.OnDrag += OnNodeDrag;
        nodeView.OnEndDrag += OnNodeEndDrag;
    }

    private bool IsPowerSystemComponent(string componentType)
    {
        return componentType == "IGenerator" ||
               componentType == "IRouter" ||
               componentType == "IRoutingSubstation" ||
               componentType == "IBreakerBox";
    }

    private void RefreshConnections()
    {
        if (Graph == null) return;

        if (_previewConnection != null && _connectionsContainer.Contains(_previewConnection))
        {
            _connectionsContainer.Remove(_previewConnection);
        }

        _connectionsContainer.Clear();

        foreach (var connection in Graph.Connections)
        {
            if (_nodeViews.ContainsKey(connection.outputNodeId) &&
                _nodeViews.ContainsKey(connection.inputNodeId))
            {
                var startNodeView = _nodeViews[connection.outputNodeId];
                var endNodeView = _nodeViews[connection.inputNodeId];

                // Check if either node is hidden
                bool isHidden = startNodeView.style.display == DisplayStyle.None ||
                               endNodeView.style.display == DisplayStyle.None;

                // Determine cable size and visual properties for this connection
                var cableSize = GetConnectionCableSize(connection);
                var connectionColor = GetConnectionColorForMode(cableSize, isHidden);
                var connectionThickness = GetConnectionThickness(cableSize);

                // If connection leads to hidden node, make it invisible or very faint
                if (isHidden)
                {
                    connectionThickness = 1f;
                }

                var connectionView = new ConnectionView(startNodeView, endNodeView, false,
                    connection.outputPortIndex, connection.inputPortIndex, connectionColor, connectionThickness);
                _connectionsContainer.Add(connectionView);

                connectionView.schedule.Execute(() => connectionView.UpdatePosition());
            }
        }
    }

    private CableSize? GetConnectionCableSize(SerializableConnection connection)
    {
        var outputNode = Graph.Nodes.FirstOrDefault(n => n.Id == connection.outputNodeId);
        var inputNode = Graph.Nodes.FirstOrDefault(n => n.Id == connection.inputNodeId);

        if (outputNode == null || inputNode == null) return null;

        // Handle power system components
        if (IsPowerSystemComponent(outputNode.ComponentType) || IsPowerSystemComponent(inputNode.ComponentType))
        {
            return GetPowerSystemConnectionCableSize(outputNode, inputNode, connection);
        }

        // For room network connections, try to get cable size from boundary port info
        if (outputNode.ComponentType == "HolographicRoom" && inputNode.ComponentType == "HolographicRoom")
        {
            // Get the output port info
            var outputPorts = outputNode.GetOutputPorts();
            if (connection.outputPortIndex < outputPorts.Count)
            {
                var outputPortInfo = outputPorts[connection.outputPortIndex];
                if (outputPortInfo.assignedCableSize.HasValue)
                {
                    return outputPortInfo.assignedCableSize.Value;
                }
            }

            // Fallback to input port info
            var inputPorts = inputNode.GetInputPorts();
            if (connection.inputPortIndex < inputPorts.Count)
            {
                var inputPortInfo = inputPorts[connection.inputPortIndex];
                if (inputPortInfo.assignedCableSize.HasValue)
                {
                    return inputPortInfo.assignedCableSize.Value;
                }
            }
        }

        return null;
    }

    private CableSize? GetPowerSystemConnectionCableSize(NetworkNode outputNode, NetworkNode inputNode, SerializableConnection connection)
    {
        // Determine cable size based on the output node's component type and port
        switch (outputNode.ComponentType)
        {
            case "IGenerator":
                // All generator outputs are transmission
                return CableSize.Transmission;

            case "IRouter":
                // Router outputs are distribution
                return CableSize.Distribution;

            case "IRoutingSubstation":
                // Substation outputs are branch
                return CableSize.Branch;

            case "IBreakerBox":
                // Breaker box typically doesn't have outputs, but if it does, they're branch
                return CableSize.Branch;
        }

        // If output node isn't a power system component, check the input node
        switch (inputNode.ComponentType)
        {
            case "IRouter":
                // Router inputs are transmission
                return CableSize.Transmission;

            case "IRoutingSubstation":
                // Substation inputs are distribution
                return CableSize.Distribution;

            case "IBreakerBox":
                // Breaker box inputs are branch
                return CableSize.Branch;
        }

        return null;
    }

    private Color GetConnectionColorForMode(CableSize? cableSize, bool isHidden)
    {
        if (isHidden)
        {
            // Use the background color or a very faint color
            return new Color(0.1f, 0.1f, 0.1f, 0.1f); // Nearly invisible
        }

        if (_currentViewMode == ViewMode.All)
        {
            // In All mode, use default white/gray
            return Color.white;
        }

        // Get the cable size for current mode
        var modeCableSize = GetCableSizeForMode(_currentViewMode);

        if (cableSize.HasValue && cableSize.Value == modeCableSize)
        {
            // Connection matches current mode - use mode color
            return GetModeColor(_currentViewMode);
        }
        else
        {
            // Connection doesn't match - dim it significantly
            return new Color(0.3f, 0.3f, 0.3f, 0.3f);
        }
    }

    private Color GetConnectionColor(CableSize? cableSize)
    {
        // If no filter is set, all connections are white
        if (_connectionFilter == null)
            return Color.white;

        // If connection matches filter, highlight in orange
        if (cableSize.HasValue && cableSize.Value == _connectionFilter.Value)
            return new Color(1f, 0.5f, 0f); // Orange

        // If connection doesn't match filter, show in white but dimmed
        return new Color(0.7f, 0.7f, 0.7f, 0.5f); // Dimmed white
    }

    private float GetConnectionThickness(CableSize? cableSize)
    {
        if (!cableSize.HasValue)
            return 2f; // Default thickness

        return cableSize.Value switch
        {
            CableSize.Transmission => 6f,  // Thick
            CableSize.Distribution => 4f,  // Medium
            CableSize.Branch => 2f,        // Thin
            _ => 2f
        };
    }

    private void OnConnectBoundaryPortsClicked()
    {
        if (Graph == null) return;

        var globalResolver = FindFirstObjectByType<GlobalRouteResolver>();
        if (globalResolver == null)
        {
            Debug.LogWarning("No GlobalRouteResolver found in scene");
            return;
        }

        // Discover and resolve boundary connections
        globalResolver.DiscoverRoomNetworks();
        globalResolver.ResolveBoundaryConnections();

        // Refresh all room network data to ensure boundary port info is current
        RefreshAllRoomNetworkData();

        // Get all compatible boundary connections
        var compatibleConnections = globalResolver.GetCompatibleConnections();

        // Clear existing room network connections (but preserve PowerNode connections)
        ClearRoomNetworkConnections();

        // Create node graph connections based on boundary connections
        int connectionsCreated = 0;
        foreach (var boundaryConnection in compatibleConnections)
        {
            if (CreateNodeGraphConnection(boundaryConnection))
            {
                connectionsCreated++;
            }
        }

        RefreshConnections();
        Debug.Log($"Connected {connectionsCreated} boundary port pairs out of {compatibleConnections.Count} compatible connections");
    }

    private void RefreshAllRoomNetworkData()
    {
        foreach (var node in Graph.Nodes)
        {
            if (node.ComponentType == "HolographicRoom" && node.RoomNetwork != null)
            {
                node.RefreshRoomNetworkData();
            }
        }
    }
    
    private void ClearRoomNetworkConnections()
    {
        // Only clear connections between room network nodes, preserve PowerNode connections
        var connectionsToRemove = new List<SerializableConnection>();

        foreach (var connection in Graph.Connections)
        {
            var outputNode = Graph.Nodes.FirstOrDefault(n => n.Id == connection.outputNodeId);
            var inputNode = Graph.Nodes.FirstOrDefault(n => n.Id == connection.inputNodeId);

            if (outputNode != null && inputNode != null &&
                outputNode.ComponentType == "HolographicRoom" &&
                inputNode.ComponentType == "HolographicRoom")
            {
                connectionsToRemove.Add(connection);
            }
        }

        foreach (var connection in connectionsToRemove)
        {
            Graph.RemoveConnection(connection.outputNodeId, connection.outputPortIndex,
                                 connection.inputNodeId, connection.inputPortIndex);
        }
    }

    private bool CreateNodeGraphConnection(GlobalRouteResolver.BoundaryConnection boundaryConnection)
    {
        // Find the nodes corresponding to these room networks
        var nodeA = Graph.Nodes.FirstOrDefault(n => n.RoomNetwork == boundaryConnection.networkA);
        var nodeB = Graph.Nodes.FirstOrDefault(n => n.RoomNetwork == boundaryConnection.networkB);

        if (nodeA == null || nodeB == null)
        {
            Debug.LogWarning($"Could not find nodes for room networks in boundary connection");
            return false;
        }

        // Determine which port is input and which is output based on activeConnection
        bool portAIsInput = boundaryConnection.portA.activeConnection?.isConnectedToEntry ?? false;
        bool portBIsInput = boundaryConnection.portB.activeConnection?.isConnectedToEntry ?? false;

        // They should be opposite (one input, one output)
        if (portAIsInput == portBIsInput)
        {
            Debug.LogWarning($"Boundary ports {boundaryConnection.portA.portId} and {boundaryConnection.portB.portId} have same direction");
            return false;
        }

        NetworkNode outputNode, inputNode;
        NetworkBoundaryPort outputPort, inputPort;

        if (portAIsInput)
        {
            inputNode = nodeA;
            outputNode = nodeB;
            inputPort = boundaryConnection.portA;
            outputPort = boundaryConnection.portB;
        }
        else
        {
            inputNode = nodeB;
            outputNode = nodeA;
            inputPort = boundaryConnection.portB;
            outputPort = boundaryConnection.portA;
        }

        // Get port indices using consistent ordering
        int outputPortIndex = GetBoundaryPortIndex(outputNode, outputPort, false);
        int inputPortIndex = GetBoundaryPortIndex(inputNode, inputPort, true);

        if (outputPortIndex >= 0 && inputPortIndex >= 0)
        {
            Graph.Connect(outputNode, inputNode, outputPortIndex, inputPortIndex);
            Debug.Log($"Connected {outputNode.Name} port {outputPortIndex} to {inputNode.Name} port {inputPortIndex} via boundary ports {outputPort.portId} -> {inputPort.portId}");
            return true;
        }
        else
        {
            Debug.LogWarning($"Could not find port indices for boundary connection: output={outputPortIndex}, input={inputPortIndex}");
            return false;
        }
    }

    private int GetBoundaryPortIndex(NetworkNode node, NetworkBoundaryPort boundaryPort, bool isInput)
    {
        if (node.RoomNetwork == null) return -1;

        // Get all boundary ports of the specified type, ordered consistently by portId
        var boundaryPorts = isInput ?
            node.GetInputPorts().OrderBy(p => p.portId).ToList() :
            node.GetOutputPorts().OrderBy(p => p.portId).ToList();

        // Find the index of our specific port
        for (int i = 0; i < boundaryPorts.Count; i++)
        {
            if (boundaryPorts[i].portId == boundaryPort.portId)
            {
                return i;
            }
        }

        Debug.LogWarning($"Could not find boundary port {boundaryPort.portId} in node {node.Name} {(isInput ? "input" : "output")} ports");
        return -1;
    }

    private void OnDeleteNodeClicked()
    {
        if (_selectedNode != null && Graph != null)
        {
            Graph.DeleteNode(_selectedNode.Node);
            _nodesContainer.Remove(_selectedNode);
            _nodeViews.Remove(_selectedNode.Node.Id);
            RefreshConnections();
            _selectedNode = null;
            RestoreRoomColor();
        }
    }

    private void OnScanRoomsClicked()
    {
        if (Graph == null) return;

        // Clear existing nodes
        Graph.Nodes.Clear();
        RefreshView();

        // Find all HolographicRooms in the scene
        var rooms = FindObjectsByType<HolographicRoom>(FindObjectsSortMode.None);

        var nodePosition = new Vector2(50, 50);
        float nodeSpacing = 200f;
        int nodesPerRow = 4;
        int currentNode = 0;

        foreach (var room in rooms)
        {
            // Calculate position in grid
            int row = currentNode / nodesPerRow;
            int col = currentNode % nodesPerRow;
            var position = new Vector2(
                nodePosition.x + col * nodeSpacing,
                nodePosition.y + row * nodeSpacing
            );

            // Get template count from HolographicRoom
            int templateCount = room.GetTemplateCount();

            // Create node with room info
            string nodeName = !string.IsNullOrEmpty(room.roomName) ? room.roomName : room.gameObject.name;
            var node = Graph.CreateNode(nodeName, position);
            node.SourceGameObject = room.gameObject;
            node.SourceRoom = room;
            node.ComponentType = "HolographicRoom";

            // Set port counts based on template objects
            node.InputPortCount = Mathf.Max(1, templateCount);
            node.OutputPortCount = Mathf.Max(1, templateCount);

            CreateNodeView(node);
            currentNode++;
        }

        Debug.Log($"Created {currentNode} room nodes from {rooms.Length} HolographicRooms");
    }
    private void DetectPowerNodeConnections()
    {
        // Find all PowerNode-based nodes
        var powerNodeIds = new List<string>();
        foreach (var node in Graph.Nodes)
        {
            if (node.SourceGameObject != null && node.SourceGameObject.GetComponent<PowerNode>() != null)
            {
                powerNodeIds.Add(node.Id);
            }
        }

        // For each PowerNode, check its connections
        foreach (var nodeId in powerNodeIds)
        {
            if (!_nodeViews.ContainsKey(nodeId)) continue;

            var nodeView = _nodeViews[nodeId];
            var powerNode = nodeView.Node.SourceGameObject.GetComponent<PowerNode>();

            // Check active connections in the PowerNode
            foreach (var connection in powerNode.activeConnections)
            {
                // Find the target PowerNode
                PowerNode targetPowerNode = null;
                bool isInput = connection.isInput;

                if (connection.connectedTemplate != null)
                {
                    // Try to find a node that connects to this template
                    foreach (var otherNodeId in powerNodeIds)
                    {
                        if (otherNodeId == nodeId) continue;

                        var otherPowerNode = _nodeViews[otherNodeId].Node.SourceGameObject.GetComponent<PowerNode>();
                        foreach (var otherConn in otherPowerNode.activeConnections)
                        {
                            if (otherConn.connectedTemplate == connection.connectedTemplate &&
                                otherConn.isInput != isInput)
                            {
                                targetPowerNode = otherPowerNode;
                                break;
                            }
                        }

                        if (targetPowerNode != null) break;
                    }
                }

                if (targetPowerNode != null)
                {
                    // Find the NetworkNode for the target PowerNode
                    NetworkNode targetNode = Graph.Nodes.Find(n =>
                        n.SourceGameObject != null && n.SourceGameObject.GetComponent<PowerNode>() == targetPowerNode);

                    if (targetNode != null)
                    {
                        // Connect in the appropriate direction
                        if (isInput)
                        {
                            // This node's input connects to target node's output
                            ConnectNodes(targetNode, nodeView.Node,
                                         connection.pointIndex, connection.pointIndex);
                        }
                        else
                        {
                            // This node's output connects to target node's input
                            ConnectNodes(nodeView.Node, targetNode,
                                         connection.pointIndex, connection.pointIndex);
                        }
                    }
                }
            }
        }

        RefreshConnections();
    }
    
    private void OnScanComponentsClicked()
    {
        if (Graph == null) return;

        var nodePosition = new Vector2(50, 50);
        float nodeSpacing = 200f;
        int nodesPerRow = 4;
        int currentNode = Graph.Nodes.Count;

        // Add power system types to scan list
        var powerSystemTypes = new List<string>
    {
        "IGenerator",
        "IRouter",
        "IRoutingSubstation",
        "IBreakerBox"
    };

        var typesToScan = new List<string>(_targetComponentTypes);
        foreach (var psType in powerSystemTypes)
        {
            if (!typesToScan.Contains(psType))
                typesToScan.Add(psType);
        }

        foreach (var typeName in typesToScan)
        {
            if (string.IsNullOrEmpty(typeName)) continue;

            Type componentType = Type.GetType(typeName) ??
                               Type.GetType($"UnityEngine.{typeName}") ??
                               Type.GetType($"{typeName}, Assembly-CSharp") ??
                               Type.GetType($"ProjectUniverse.PowerSystem.{typeName}, Assembly-CSharp");

            if (componentType != null)
            {
                var components = FindObjectsByType(componentType, FindObjectsSortMode.None);
                foreach (var component in components)
                {
                    Component component1 = (component as Component);
                    var gameObj = component1 != null ? component1.gameObject : null;
                    if (gameObj != null)
                    {
                        // Check if node already exists
                        var existingNode = Graph.Nodes.FirstOrDefault(n => n.SourceGameObjectPath == GetGameObjectPath(gameObj));
                        if (existingNode != null)
                        {
                            // Update existing node instead of skipping
                            existingNode.ComponentType = typeName;

                            // IMPORTANT: Reset port counts before setting new ones
                            existingNode.InputPortCount = 0;
                            existingNode.OutputPortCount = 0;

                            SetPowerSystemNodePorts(existingNode, component);

                            // Refresh the existing node view
                            if (_nodeViews.ContainsKey(existingNode.Id))
                            {
                                var oldView = _nodeViews[existingNode.Id];
                                _nodesContainer.Remove(oldView);
                                _nodeViews.Remove(existingNode.Id);
                                CreateNodeView(existingNode);
                            }
                            continue;
                        }

                        int row = currentNode / nodesPerRow;
                        int col = currentNode % nodesPerRow;
                        var position = new Vector2(
                            nodePosition.x + col * nodeSpacing,
                            nodePosition.y + row * nodeSpacing
                        );

                        string nodeName = gameObj.name;
                        var node = Graph.CreateNode(nodeName, position);
                        node.SetSourceGameObject(gameObj);
                        node.ComponentType = typeName;

                        // Set port counts based on component type
                        SetPowerSystemNodePorts(node, component);

                        CreateNodeView(node);
                        currentNode++;
                    }
                }
            }
        }

        // First ensure GlobalRouteResolver has discovered power connections
        var globalResolver = FindFirstObjectByType<GlobalRouteResolver>();
        if (globalResolver != null)
        {
            globalResolver.DiscoverRoomNetworks();
            globalResolver.DetectAndCreatePowerPaths();
        }

        // Now detect waypoint connections for all power system nodes
        DetectAllWaypointConnections();

        // Detect connections for both PowerNodes and Power System components
        DetectPowerNodeConnections();
        DetectPowerSystemConnections();

        Graph.SaveAllNodeStates();
        RefreshConnections();
    }
    
    private void DetectAllWaypointConnections()
    {
        Debug.Log("Detecting waypoint connections for all nodes...");

        foreach (var node in Graph.Nodes)
        {
            if (IsPowerSystemComponent(node.ComponentType))
            {
                node.DetectWaypointConnections();

                // If node has waypoint connections and is in a room, ensure room has corresponding ports
                if (node.waypointPortInfos.Count > 0 && node.SourceRoom != null)
                {
                    EnsureRoomWaypointPorts(node.SourceRoom, node);
                }
            }
        }

        // Refresh all node views to show new waypoint ports
        RefreshView();
    }
   
    private void EnsureRoomWaypointPorts(HolographicRoom room, NetworkNode componentNode)
    {
        // Find or create room node
        var roomNode = Graph.Nodes.FirstOrDefault(n => n.SourceRoom == room && n.ComponentType == "HolographicRoom");
        if (roomNode == null) return;

        // For each waypoint connection on the component, ensure room has corresponding port
        foreach (var waypointInfo in componentNode.waypointPortInfos)
        {
            // If component has waypoint input, room should have waypoint output (and vice versa)
            bool roomShouldHaveOutput = waypointInfo.isComponentInput;

            // Check if room node already has this waypoint port
            var existingWaypointPort = roomNode.waypointPortInfos
                .FirstOrDefault(w => w.waypointPathId == waypointInfo.waypointPathId);

            if (existingWaypointPort == null)
            {
                // Add corresponding waypoint port to room
                var roomWaypointInfo = new NetworkNode.WaypointPortInfo
                {
                    waypointPathId = waypointInfo.waypointPathId,
                    waypointName = waypointInfo.waypointName,
                    isComponentInput = !waypointInfo.isComponentInput, // Opposite direction
                    waypointWorldPosition = waypointInfo.waypointWorldPosition,
                    connectedRoom = room,
                    connectedRoomPath = waypointInfo.connectedRoomPath,
                    waypointPath = waypointInfo.waypointPath
                };

                roomNode.waypointPortInfos.Add(roomWaypointInfo);
                Debug.Log($"Added waypoint port to room {room.roomName} for waypoint {waypointInfo.waypointName}");
            }
        }
    }

    private void SetPowerSystemNodePorts(NetworkNode node, object component)
    {
        // First detect waypoint connections
        node.DetectWaypointConnections();

        // Check if this component is inside a room
        HolographicRoom parentRoom = node.SourceGameObject.GetComponentInParent<HolographicRoom>();
        if (parentRoom != null)
        {
            node.SetSourceRoom(parentRoom);
            Debug.Log($"{node.Name} is inside room: {parentRoom.roomName}");
        }

        // For power system components, don't create regular ports - only waypoint ports
        switch (component)
        {
            case IGenerator generator:
                node.InputPortCount = 0; // No regular ports
                node.OutputPortCount = 0; // Only waypoint ports
                Debug.Log($"Generator {node.Name} set to 0 regular ports, {node.GetWaypointOutputPorts().Count} waypoint outputs");
                break;

            case IRouter router:
                node.InputPortCount = 0; // No regular ports
                node.OutputPortCount = 0; // Only waypoint ports
                Debug.Log($"Router {node.Name} set to 0 regular ports, " +
                         $"{node.GetWaypointInputPorts().Count} waypoint inputs, {node.GetWaypointOutputPorts().Count} waypoint outputs");
                break;

            case IRoutingSubstation substation:
                node.InputPortCount = 0; // No regular ports
                node.OutputPortCount = 0; // Only waypoint ports
                break;

            case IBreakerBox breakerBox:
                node.InputPortCount = 0; // No regular ports
                node.OutputPortCount = 0; // Only waypoint ports
                Debug.Log($"BreakerBox {node.Name} set to 0 regular ports, {node.GetWaypointInputPorts().Count} waypoint inputs");
                break;

            case PowerNode powerNode:
                node.InputPortCount = powerNode.connectionCount;
                node.OutputPortCount = powerNode.connectionCount;
                node.CapturePowerNodeState();
                break;

            default:
                // For non-power system components, use the existing logic
                if (parentRoom != null)
                {
                    int templateCount = parentRoom.GetTemplateCount();
                    node.InputPortCount = Mathf.Max(1, templateCount);
                    node.OutputPortCount = Mathf.Max(1, templateCount);
                }
                else
                {
                    node.InputPortCount = 1;
                    node.OutputPortCount = 1;
                }
                break;
        }
    }
   
    private void DetectPowerSystemConnections()
    {
        // Find all power system component nodes
        var powerSystemNodes = Graph.Nodes.Where(n => IsPowerSystemComponent(n.ComponentType)).ToList();

        foreach (var node in powerSystemNodes)
        {
            if (node.SourceGameObject == null) continue;

            // Get NetworkBoundaryPort components from Inputs/Outputs
            var outputsParent = node.SourceGameObject.transform.Find("Outputs");

            if (outputsParent != null)
            {
                var outputPorts = outputsParent.GetComponentsInChildren<NetworkBoundaryPort>();

                for (int i = 0; i < outputPorts.Length; i++)
                {
                    var outputPort = outputPorts[i];
                    if (outputPort.IsConnected() && outputPort.activeConnection != null)
                    {
                        var connection = outputPort.activeConnection;
                        var connectedPath = connection.connectedPath;
                        var connectedTemplate = connection.connectedTemplate;

                        if (connectedPath != null && connectedTemplate != null)
                        {
                            // Find what's at the other end of this path
                            var targetNode = FindNodeConnectedToPath(connectedPath, node);

                            if (targetNode != null)
                            {
                                // Determine port indices
                                int targetPortIndex = GetNetworkBoundaryPortIndex(targetNode, connectedPath, true);

                                if (targetPortIndex >= 0)
                                {
                                    // Create the graph connection
                                    Graph.Connect(node, targetNode, i, targetPortIndex);

                                    // Create the path cable connection in the power system
                                    CreatePowerSystemPathConnection(node, targetNode, connectedPath, connectedTemplate);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void CreatePowerSystemPathConnection(NetworkNode sourceNode, NetworkNode targetNode, WaypointPath path, Template template)
    {
        var sourceComponent = GetPowerSystemComponent(sourceNode.SourceGameObject);
        var targetComponent = GetPowerSystemComponent(targetNode.SourceGameObject);

        if (sourceComponent != null && targetComponent != null)
        {
            PowerSystemPathManager.Instance.CreatePathConnection(sourceComponent, targetComponent, path, template);
        }
    }

    private Component GetPowerSystemComponent(GameObject obj)
    {
        //Debug.Log("Selected "+obj);
        if (obj == null) return null;

        return obj.GetComponent<IGenerator>() ??
               obj.GetComponent<IRouter>() ??
               obj.GetComponent<IRoutingSubstation>() ??
               obj.GetComponent<IBreakerBox>() ??
               (Component)obj.GetComponent<IMachine>();
    }

    private NetworkNode FindNodeConnectedToPath(WaypointPath path, NetworkNode excludeNode)
    {
        foreach (var node in Graph.Nodes)
        {
            if (node == excludeNode || node.SourceGameObject == null) continue;

            // Check input ports
            var inputsParent = node.SourceGameObject.transform.Find("Inputs");
            if (inputsParent != null)
            {
                var inputPorts = inputsParent.GetComponentsInChildren<NetworkBoundaryPort>();
                foreach (var port in inputPorts)
                {
                    if (port.IsConnected() && port.activeConnection?.connectedPath == path)
                        return node;
                }
            }
        }

        return null;
    }

    private int GetNetworkBoundaryPortIndex(NetworkNode node, WaypointPath path, bool checkInputs)
    {
        if (node.SourceGameObject == null) return -1;

        var parent = node.SourceGameObject.transform.Find(checkInputs ? "Inputs" : "Outputs");
        if (parent == null) return -1;

        var ports = parent.GetComponentsInChildren<NetworkBoundaryPort>()
            .OrderBy(p => p.portId)
            .ToList();

        for (int i = 0; i < ports.Count; i++)
        {
            if (ports[i].IsConnected() && ports[i].activeConnection?.connectedPath == path)
                return i;
        }

        return -1;
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

    private void HighlightRoom(HolographicRoom room)
    {
        if (room == null) return;

        // Store the room and its original color
        _highlightedRoom = room;
        _originalRoomColor = room.GetRoomColor();

        // Apply highlight color
        room.UpdateRoomColor(_highlightColor);
    }

    private void RestoreRoomColor()
    {
        if (_highlightedRoom == null) return;

        // Restore original color
        _highlightedRoom.UpdateRoomColor(_originalRoomColor);
        _highlightedRoom = null;
    }

    private void OnStartConnection(NodeView nodeView, bool isInput, int portIndex)
    {
        _connectionStartNode = nodeView;
        _isConnecting = true;
        _connectionStartPortIndex = portIndex;
        _connectionStartIsOutput = !isInput;

        // Create preview connection with correct port indices
        /*if (_connectionStartIsOutput)
        {
            _previewConnection = new ConnectionView(nodeView, null, true, portIndex, 0);
        }
        else
        {
            _previewConnection = new ConnectionView(null, nodeView, true, 0, portIndex);
        }*/
        // Create preview connection with default styling
    if (_connectionStartIsOutput)
    {
        _previewConnection = new ConnectionView(nodeView, null, true, portIndex, 0, 
            new Color(1f, 1f, 1f, 0.5f), 2f); // Semi-transparent white for preview
    }
    else
    {
        _previewConnection = new ConnectionView(null, nodeView, true, 0, portIndex, 
            new Color(1f, 1f, 1f, 0.5f), 2f); // Semi-transparent white for preview
    }

        _connectionsContainer.Add(_previewConnection);
    }

    private void OnEndConnection(NodeView nodeView, bool isInput, int portIndex)
    {
        if (_connectionStartNode != null && _connectionStartNode != nodeView)
        {
            // Determine which node is output and which is input
            NodeView outputNodeView, inputNodeView;
            int outputPortIndex, inputPortIndex;

            if (_connectionStartIsOutput && isInput)
            {
                // Start node is output, end node is input
                outputNodeView = _connectionStartNode;
                inputNodeView = nodeView;
                outputPortIndex = _connectionStartPortIndex;
                inputPortIndex = portIndex;
            }
            else if (!_connectionStartIsOutput && !isInput)
            {
                // Start node is input, end node is output
                outputNodeView = nodeView;
                inputNodeView = _connectionStartNode;
                outputPortIndex = portIndex;
                inputPortIndex = _connectionStartPortIndex;
            }
            else
            {
                // Invalid connection (output to output or input to input)
                CleanupConnection();
                return;
            }

            // Connect the nodes with specific port indices
            Graph.Connect(outputNodeView.Node, inputNodeView.Node, outputPortIndex, inputPortIndex);
            ConnectNodes(outputNodeView.Node, inputNodeView.Node, outputPortIndex, inputPortIndex);
        }

        CleanupConnection();
        RefreshConnections();
    }

    private void ConnectNodes(NetworkNode outputNode, NetworkNode inputNode, int outputPortIndex, int inputPortIndex)
    {
        Graph.Connect(outputNode, inputNode, outputPortIndex, inputPortIndex);
    }

    private void CleanupConnection()
    {
        // Clean up preview connection
        if (_previewConnection != null && _connectionsContainer.Contains(_previewConnection))
        {
            _connectionsContainer.Remove(_previewConnection);
            _previewConnection = null;
        }

        _connectionStartNode = null;
        _connectionStartPortIndex = 0;
        _connectionStartIsOutput = false;
        _isConnecting = false;
    }

    private void OnGraphMouseMove(MouseMoveEvent evt)
    {
        if (_isConnecting && _previewConnection != null)
        {
            // Convert mouse position to graph space
            Vector2 localPos = _connectionsContainer.WorldToLocal(evt.mousePosition);
            _previewConnection.UpdatePreviewEndPosition(localPos);
        }
    }

    // Node dragging callbacks
    private void OnNodeStartDrag(NodeView nodeView, Vector2 mousePosition)
    {
        OnNodeSelected(nodeView);
    }

    private void OnNodeDrag(NodeView nodeView, Vector2 delta)
    {
        nodeView.Node.Position += delta / _currentZoom;
        nodeView.UpdatePosition();

        // Update only connections related to this node
        _connectionsContainer.Query<ConnectionView>().ForEach(connection =>
        {
            connection.UpdatePosition();
        });
    }

    private void OnNodeEndDrag(NodeView nodeView)
    {
        // Nothing needed here for now
    }

    // Input System callbacks
    private void OnRightClickStarted(InputAction.CallbackContext context)
    {
        if (!IsMouseOverGraphView()) return;

        _isPanning = true;
        _lastMousePosition = Mouse.current.position.ReadValue();
        UpdateZoomDisplay();
    }

    private void OnRightClickCanceled(InputAction.CallbackContext context)
    {
        _isPanning = false;
        UpdateZoomDisplay();
    }

    private void OnScrollWheelPerformed(InputAction.CallbackContext context)
    {
        // Only handle zoom when mouse is over graph view
        if (!IsMouseOverGraphView())
            return;

        float scrollValue = context.ReadValue<Vector2>().y;
        float zoomDelta = -scrollValue * ZOOM_SPEED * 0.01f;

        ZoomGraph(zoomDelta);
    }

    private void PanGraph(Vector2 delta)
    {
        // Scale delta based on zoom level
        delta *= 2.0f / _currentZoom;

        foreach (var nodeView in _nodeViews.Values)
        {
            nodeView.Node.Position += delta;
            nodeView.UpdatePosition();
        }

        RefreshConnections();
    }

    private void ZoomGraph(float zoomDelta)
    {
        float newZoom = Mathf.Clamp(_currentZoom + zoomDelta, MIN_ZOOM, MAX_ZOOM);

        if (Mathf.Approximately(newZoom, _currentZoom))
            return;

        // Get mouse position in graph space for zoom origin
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector2 graphPos = _graphContainer.WorldToLocal(mousePos);

        float zoomFactor = newZoom / _currentZoom;

        foreach (var nodeView in _nodeViews.Values)
        {
            // Calculate position relative to mouse
            Vector2 relPos = nodeView.Node.Position - graphPos;

            // Scale position
            relPos *= zoomFactor;

            // Set new position
            nodeView.Node.Position = graphPos + relPos;
            nodeView.UpdatePosition();
        }

        _currentZoom = newZoom;

        // Apply zoom to nodes container
        _nodesContainer.style.scale = new StyleScale(new Scale(new Vector3(_currentZoom, _currentZoom, 1)));
        _connectionsContainer.style.scale = new StyleScale(new Scale(new Vector3(_currentZoom, _currentZoom, 1)));

        RefreshConnections();
        UpdateZoomDisplay();
    }

    private void UpdateZoomDisplay()
    {
        var zoomLabel = GetComponent<UIDocument>().rootVisualElement.Q<Label>("zoom-label");
        if (zoomLabel != null)
        {
            int zoomPercent = Mathf.RoundToInt(_currentZoom * 100);
            zoomLabel.text = $"{zoomPercent}%";
        }

        // Update cursor style
        if (_isPanning)
            _graphContainer.AddToClassList("panning");
        else
            _graphContainer.RemoveFromClassList("panning");
    }

    public bool IsMouseOverGraphView()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        // Get the graph container's screen bounds
        if (_graphContainer != null)
        {
            var bounds = _graphContainer.worldBound;
            return bounds.Contains(mousePos);
        }

        // Fallback to simple left half check
        return mousePos.x < Screen.width * 0.5f;
    }
}