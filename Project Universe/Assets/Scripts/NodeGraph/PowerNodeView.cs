using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using ProjectUniverse.PowerSystem;

public class PowerNodeView : NodeView
{
    private PowerNode _powerNode;
    private VisualElement _routingMatrix;
    private Button _configureButton;
    private List<Button> _routeButtons = new();

    public PowerNodeView(NetworkNode node) : base(node)
    {
        // Try to get PowerNode component from the source GameObject
        if (node.SourceGameObject != null)
        {
            _powerNode = node.SourceGameObject.GetComponent<PowerNode>();
        }

        // Override the content creation after base initialization
        schedule.Execute(() => {
            ReplaceWithPowerNodeContent();
        });
    }

    private void ReplaceWithPowerNodeContent()
    {
        if (_powerNode == null)
            return;

        // Find and clear the expanded content section
        var expandedContent = this.Q<VisualElement>("expanded-content") ?? this.Q<VisualElement>(className: "expanded-content");
        if (expandedContent != null)
        {
            // Clear existing content that might be related to rooms
            expandedContent.Clear();
        }
        else
        {
            // Create expanded content if it doesn't exist
            var body = this.Q<VisualElement>(className: "node-body");
            if (body != null)
            {
                expandedContent = new VisualElement
                {
                    name = "expanded-content"
                };
                expandedContent.AddToClassList("expanded-content");
                expandedContent.style.display = DisplayStyle.None;
                body.Add(expandedContent);
            }
            else
            {
                return; // Can't proceed without proper structure
            }
        }

        // Create a header for the PowerNode section
        var powerNodeHeader = new Label("PowerNode Configuration");
        powerNodeHeader.AddToClassList("section-header");
        powerNodeHeader.style.marginBottom = 10;
        expandedContent.Add(powerNodeHeader);

        // Add node ID information
        var nodeIdContainer = new VisualElement();
        nodeIdContainer.style.marginBottom = 10;
        expandedContent.Add(nodeIdContainer);

        var nodeIdLabel = new Label($"Node ID: {_powerNode.nodeId}");
        nodeIdLabel.style.color = new Color(0.8f, 0.8f, 1f);
        nodeIdContainer.Add(nodeIdLabel);

        // Create routing section
        var routingSection = new VisualElement();
        routingSection.AddToClassList("section");
        expandedContent.Add(routingSection);

        var routingHeader = new Label("Internal Routing");
        routingHeader.AddToClassList("section-header");
        routingSection.Add(routingHeader);

        // Add configure button
        _configureButton = new Button(() => OpenNodeEditor()) { text = "Configure Routing" };
        _configureButton.style.marginTop = 5;
        _configureButton.style.marginBottom = 10;
        routingSection.Add(_configureButton);

        // Create routing matrix visualization
        _routingMatrix = new VisualElement();
        _routingMatrix.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
        _routingMatrix.style.borderBottomLeftRadius = 3;
        _routingMatrix.style.borderBottomRightRadius = 3;
        _routingMatrix.style.borderTopLeftRadius = 3;
        _routingMatrix.style.borderTopRightRadius = 3;
        _routingMatrix.style.paddingBottom = 2;
        _routingMatrix.style.paddingTop = 2;
        _routingMatrix.style.paddingLeft = 2;
        _routingMatrix.style.paddingRight = 2;
        routingSection.Add(_routingMatrix);

        RefreshRoutingMatrix();

        // Add connection points section
        var connectionsSection = new VisualElement();
        connectionsSection.AddToClassList("section");
        expandedContent.Add(connectionsSection);

        var connectionsHeader = new Label("Connection Points");
        connectionsHeader.AddToClassList("section-header");
        connectionsSection.Add(connectionsHeader);

        // Input points
        var inputPointsContainer = new VisualElement();
        inputPointsContainer.style.marginBottom = 10;
        connectionsSection.Add(inputPointsContainer);

        var inputHeader = new Label($"Input Points: {_powerNode.inputPoints.Count}");
        inputHeader.style.color = new Color(0.4f, 0.6f, 1f);
        inputHeader.style.unityFontStyleAndWeight = FontStyle.Bold;
        inputPointsContainer.Add(inputHeader);

        for (int i = 0; i < _powerNode.inputPoints.Count; i++)
        {
            var point = _powerNode.inputPoints[i];
            var pointInfo = new Label($"IN {i}: {point.id}");
            pointInfo.style.marginLeft = 10;
            pointInfo.style.color = new Color(0.7f, 0.7f, 0.7f);
            inputPointsContainer.Add(pointInfo);
        }

        // Output points
        var outputPointsContainer = new VisualElement();
        connectionsSection.Add(outputPointsContainer);

        var outputHeader = new Label($"Output Points: {_powerNode.outputPoints.Count}");
        outputHeader.style.color = new Color(1f, 0.6f, 0.4f);
        outputHeader.style.unityFontStyleAndWeight = FontStyle.Bold;
        outputPointsContainer.Add(outputHeader);

        for (int i = 0; i < _powerNode.outputPoints.Count; i++)
        {
            var point = _powerNode.outputPoints[i];
            var pointInfo = new Label($"OUT {i}: {point.id}");
            pointInfo.style.marginLeft = 10;
            pointInfo.style.color = new Color(0.7f, 0.7f, 0.7f);
            outputPointsContainer.Add(pointInfo);
        }

        // Add active connections section
        var activeConnectionsSection = new VisualElement();
        activeConnectionsSection.AddToClassList("section");
        expandedContent.Add(activeConnectionsSection);

        var activeConnectionsHeader = new Label($"Active Connections: {_powerNode.activeConnections.Count}");
        activeConnectionsHeader.AddToClassList("section-header");
        activeConnectionsSection.Add(activeConnectionsHeader);

        if (_powerNode.activeConnections.Count > 0)
        {
            foreach (var connection in _powerNode.activeConnections)
            {
                var connectionInfo = new VisualElement();
                connectionInfo.style.marginBottom = 5;
                activeConnectionsSection.Add(connectionInfo);

                string direction = connection.isInput ? "Input" : "Output";
                string templateName = connection.connectedTemplate != null ?
                    connection.connectedTemplate.name : "Unknown";
                string pathId = connection.connectedPath != null ?
                    connection.connectedPath.pathId : "Unknown";

                var connectionLabel = new Label($"{direction} {connection.pointIndex}: {templateName} - {pathId}");
                connectionLabel.style.color = connection.isInput ?
                    new Color(0.4f, 0.6f, 1f) : new Color(1f, 0.6f, 0.4f);
                connectionInfo.Add(connectionLabel);
            }
        }
        else
        {
            var noConnectionsLabel = new Label("No active connections");
            noConnectionsLabel.style.color = new Color(0.7f, 0.7f, 0.7f);
            noConnectionsLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            noConnectionsLabel.style.paddingTop = 5;
            noConnectionsLabel.style.paddingBottom = 5;
            activeConnectionsSection.Add(noConnectionsLabel);
        }
    }

    private void RefreshRoutingMatrix()
    {
        if (_powerNode == null || _routingMatrix == null)
            return;

        _routingMatrix.Clear();
        _routeButtons.Clear();

        // Create a grid of buttons showing the routing configuration
        var matrixContainer = new VisualElement();
        matrixContainer.style.flexDirection = FlexDirection.Column;
        _routingMatrix.Add(matrixContainer);

        // Header row with output labels
        var headerRow = new VisualElement();
        headerRow.style.flexDirection = FlexDirection.Row;
        headerRow.style.marginBottom = 5;
        matrixContainer.Add(headerRow);

        // Empty corner cell
        var cornerCell = new VisualElement();
        cornerCell.style.width = 40;
        cornerCell.style.height = 20;
        headerRow.Add(cornerCell);

        // Output labels
        for (int o = 0; o < _powerNode.outputPoints.Count; o++)
        {
            var outputLabel = new Label($"Out {o}");
            outputLabel.style.width = 40;
            outputLabel.style.height = 20;
            outputLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            outputLabel.style.color = new Color(1f, 0.5f, 0f); // Orange
            headerRow.Add(outputLabel);
        }

        // Create matrix rows
        for (int i = 0; i < _powerNode.inputPoints.Count; i++)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.marginBottom = 2;
            matrixContainer.Add(row);

            // Input label
            var inputLabel = new Label($"In {i}");
            inputLabel.style.width = 40;
            inputLabel.style.height = 20;
            inputLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            inputLabel.style.color = Color.blue;
            row.Add(inputLabel);

            // Connection buttons
            for (int o = 0; o < _powerNode.outputPoints.Count; o++)
            {
                bool isConnected = _powerNode.IsInputConnectedToOutput(i, o);

                // Capture the indices for the closure
                int inputIndex = i;
                int outputIndex = o;

                var routeButton = new Button(() => ToggleRoute(inputIndex, outputIndex))
                {
                    text = isConnected ? "●" : "○"
                };
                routeButton.style.width = 40;
                routeButton.style.height = 20;
                routeButton.style.backgroundColor = isConnected ?
                    new Color(0, 0.7f, 0, 0.8f) : new Color(0.4f, 0.4f, 0.4f, 0.5f);
                routeButton.style.color = Color.white;
                routeButton.style.unityTextAlign = TextAnchor.MiddleCenter;

                row.Add(routeButton);
                _routeButtons.Add(routeButton);
            }

            // Current route indicator
            var route = _powerNode.GetRouteFromInput(i);
            if (route != null && route.isConnected)
            {
                var routeIndicator = new Label($"→ {route.outputIndex}");
                routeIndicator.style.marginLeft = 5;
                routeIndicator.style.color = new Color(0.8f, 0.8f, 0.2f);
                row.Add(routeIndicator);
            }
        }

        // Add reset button
        var resetButton = new Button(() => ResetRouting()) { text = "Reset to Default" };
        resetButton.style.marginTop = 10;
        _routingMatrix.Add(resetButton);
    }

    private void ToggleRoute(int inputIndex, int outputIndex)
    {
        if (_powerNode == null)
            return;

        bool isCurrentlyConnected = _powerNode.IsInputConnectedToOutput(inputIndex, outputIndex);

        if (isCurrentlyConnected)
        {
            // Disconnect
            _powerNode.SetRoute(inputIndex, outputIndex, false);
        }
        else
        {
            // Connect (will automatically disconnect any existing connections)
            _powerNode.SetRoute(inputIndex, outputIndex, true);
        }

        // Update UI
        RefreshRoutingMatrix();
    }

    private void ResetRouting()
    {
        if (_powerNode == null)
            return;

        // Reset to default routing (each input to corresponding output)
        for (int i = 0; i < _powerNode.connectionCount; i++)
        {
            _powerNode.SetRoute(i, i, true);
        }

        // Update UI
        RefreshRoutingMatrix();
    }

    private void OpenNodeEditor()
    {
        if (_powerNode == null)
            return;

#if UNITY_EDITOR
        // Open the Node Editor window
        UnityEditor.EditorApplication.ExecuteMenuItem("Tools/Cable System/Node Editor");

        // Select the PowerNode to edit
        UnityEditor.Selection.activeGameObject = _powerNode.gameObject;
#endif
    }
}