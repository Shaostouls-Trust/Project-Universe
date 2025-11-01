using UnityEngine;
using UnityEngine.UIElements;
using ProjectUniverse.PowerSystem;
using System.Linq;

public class PowerSystemNodeView : NodeView
{
    private IGenerator _generator;
    private IRouter _router;
    private IRoutingSubstation _substation;
    private IBreakerBox _breakerBox;

    public PowerSystemNodeView(NetworkNode node) : base(node)
    {
        // Add power system specific styling
        AddToClassList("power-system-node");

        // Get the power system component from the source GameObject
        if (node.SourceGameObject != null)
        {
            _generator = node.SourceGameObject.GetComponent<IGenerator>();
            _router = node.SourceGameObject.GetComponent<IRouter>();
            _substation = node.SourceGameObject.GetComponent<IRoutingSubstation>();
            _breakerBox = node.SourceGameObject.GetComponent<IBreakerBox>();
        }

        // Add component type specific styling
        switch (node.ComponentType)
        {
            case "IGenerator":
                AddToClassList("generator-node");
                break;
            case "IRouter":
                AddToClassList("router-node");
                break;
            case "IRoutingSubstation":
                AddToClassList("substation-node");
                break;
            case "IBreakerBox":
                AddToClassList("breaker-node");
                break;
        }

        // Override the content creation after base initialization
        schedule.Execute(() =>
        {
            ReplaceWithPowerSystemContent();
        });
    }

    private void ReplaceWithPowerSystemContent()
    {
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

        // Create power system specific content based on component type
        var powerSection = new VisualElement();
        powerSection.AddToClassList("section");
        expandedContent.Add(powerSection);

        var powerHeader = new Label($"Power System Info ({Node.ComponentType})");
        powerHeader.AddToClassList("section-header");
        powerSection.Add(powerHeader);

        if (Node.SourceGameObject == null) return;

        switch (Node.ComponentType)
        {
            case "IGenerator":
                CreateGeneratorInfo(powerSection);
                break;
            case "IRouter":
                CreateRouterInfo(powerSection);
                break;
            case "IRoutingSubstation":
                CreateSubstationInfo(powerSection);
                break;
            case "IBreakerBox":
                CreateBreakerInfo(powerSection);
                break;
        }

        // Add NetworkBoundaryPort information
        CreatePortInfo(expandedContent);

        // If inside a room, show that info too
        if (Node.SourceRoom != null)
        {
            CreateRoomInfo(expandedContent);
        }
    }

    private void CreateRoomInfo(VisualElement container)
    {
        var roomSection = new VisualElement();
        roomSection.AddToClassList("section");
        container.Add(roomSection);

        var roomHeader = new Label("Room Location");
        roomHeader.AddToClassList("section-header");
        roomSection.Add(roomHeader);

        var roomNameLabel = new Label($"Inside: {Node.SourceRoom.roomName ?? Node.SourceRoom.gameObject.name}");
        roomNameLabel.AddToClassList("power-info-label");
        roomSection.Add(roomNameLabel);
    }

    private void CreatePortInfo(VisualElement container)
    {
        var portsSection = new VisualElement();
        portsSection.AddToClassList("section");
        container.Add(portsSection);

        var portsHeader = new Label("Network Ports");
        portsHeader.AddToClassList("section-header");
        portsSection.Add(portsHeader);

        // Get input ports
        var inputsParent = Node.SourceGameObject.transform.Find("Inputs");
        if (inputsParent != null)
        {
            var inputPorts = inputsParent.GetComponentsInChildren<NetworkBoundaryPort>()
                .OrderBy(p => p.portId)
                .ToList();

            if (inputPorts.Count > 0)
            {
                var inputHeader = new Label($"Input Ports: {inputPorts.Count}");
                inputHeader.style.color = new Color(0.4f, 0.6f, 1f);
                inputHeader.style.unityFontStyleAndWeight = FontStyle.Bold;
                portsSection.Add(inputHeader);

                for (int i = 0; i < inputPorts.Count; i++)
                {
                    var port = inputPorts[i];
                    var portInfo = new Label($"IN {i}: {port.boundaryName} ({port.portId})");
                    portInfo.style.marginLeft = 10;
                    portInfo.style.color = port.IsConnected() ? Color.green : Color.gray;
                    portsSection.Add(portInfo);

                    if (port.assignedCableSize.HasValue)
                    {
                        var sizeInfo = new Label($"  Size: {port.assignedCableSize.Value}");
                        sizeInfo.style.marginLeft = 20;
                        sizeInfo.style.color = new Color(0.7f, 0.7f, 0.7f);
                        portsSection.Add(sizeInfo);
                    }
                }
            }
        }

        // Get output ports
        var outputsParent = Node.SourceGameObject.transform.Find("Outputs");
        if (outputsParent != null)
        {
            var outputPorts = outputsParent.GetComponentsInChildren<NetworkBoundaryPort>()
                .OrderBy(p => p.portId)
                .ToList();

            if (outputPorts.Count > 0)
            {
                var outputHeader = new Label($"Output Ports: {outputPorts.Count}");
                outputHeader.style.color = new Color(1f, 0.6f, 0.4f);
                outputHeader.style.unityFontStyleAndWeight = FontStyle.Bold;
                outputHeader.style.marginTop = 10;
                portsSection.Add(outputHeader);

                for (int i = 0; i < outputPorts.Count; i++)
                {
                    var port = outputPorts[i];
                    var portInfo = new Label($"OUT {i}: {port.boundaryName} ({port.portId})");
                    portInfo.style.marginLeft = 10;
                    portInfo.style.color = port.IsConnected() ? Color.green : Color.gray;
                    portsSection.Add(portInfo);

                    if (port.assignedCableSize.HasValue)
                    {
                        var sizeInfo = new Label($"  Size: {port.assignedCableSize.Value}");
                        sizeInfo.style.marginLeft = 20;
                        sizeInfo.style.color = new Color(0.7f, 0.7f, 0.7f);
                        portsSection.Add(sizeInfo);
                    }
                }
            }
        }
    }

    private void CreateGeneratorInfo(VisualElement container)
    {
        if (_generator == null) return;

        var outputLabel = new Label($"Max Output: {_generator.OutputMax}");
        outputLabel.AddToClassList("power-info-label");
        container.Add(outputLabel);

        var lastOutputLabel = new Label($"Last Output: {_generator.LastOutput:F1}");
        lastOutputLabel.AddToClassList("power-info-label");
        container.Add(lastOutputLabel);

        var gridLabel = new Label($"Power Grid: {_generator.powerGrid ?? "None"}");
        gridLabel.AddToClassList("power-info-label");
        container.Add(gridLabel);

        if (_generator.Leaking)
        {
            var leakLabel = new Label("⚠️ LEAKING!");
            leakLabel.AddToClassList("power-info-label");
            leakLabel.style.color = Color.red;
            container.Add(leakLabel);
        }
    }

    private void CreateRouterInfo(VisualElement container)
    {
        if (_router == null) return;

        var bufferLabel = new Label($"Buffer: {_router.BufferCurrent:F1}/{_router.BufferMax}");
        bufferLabel.AddToClassList("power-info-label");
        container.Add(bufferLabel);

        var lastReceivedLabel = new Label($"Last Received: {_router.LastReceived:F1}");
        lastReceivedLabel.AddToClassList("power-info-label");
        container.Add(lastReceivedLabel);

        if (_router.ConnectedGenerator != null)
        {
            var genLabel = new Label($"Connected Gen: {_router.ConnectedGenerator.name}");
            genLabel.AddToClassList("power-info-label");
            container.Add(genLabel);
        }

        if (_router.ConnectedTurbine != null)
        {
            var turbineLabel = new Label($"Connected Turbine: {_router.ConnectedTurbine.name}");
            turbineLabel.AddToClassList("power-info-label");
            container.Add(turbineLabel);
        }

        var powerSourceLabel = new Label($"Using Generator: {(_router.UseGeneratorPower ? "Yes" : "No")}");
        powerSourceLabel.AddToClassList("power-info-label");
        container.Add(powerSourceLabel);
    }

    private void CreateSubstationInfo(VisualElement container)
    {
        if (_substation == null) return;

        var nameLabel = new Label($"Station: {_substation.stationName}");
        nameLabel.AddToClassList("power-info-label");
        container.Add(nameLabel);

        var bufferLabel = new Label($"Buffer: {_substation.BufferCurrent:F1}/{_substation.BufferMax}");
        bufferLabel.AddToClassList("power-info-label");
        container.Add(bufferLabel);

        var lastReceivedLabel = new Label($"Last Received: {_substation.LastReceived:F1}");
        lastReceivedLabel.AddToClassList("power-info-label");
        container.Add(lastReceivedLabel);

        var requiredLabel = new Label($"Total Required: {_substation.TotalRequiredPower:F1}");
        requiredLabel.AddToClassList("power-info-label");
        container.Add(requiredLabel);

        var canRequestLabel = new Label($"Can Request: {(_substation.CanRequestEnergy ? "Yes" : "No")}");
        canRequestLabel.AddToClassList("power-info-label");
        canRequestLabel.style.color = _substation.CanRequestEnergy ? Color.green : Color.red;
        container.Add(canRequestLabel);
    }

    private void CreateBreakerInfo(VisualElement container)
    {
        if (_breakerBox == null) return;

        var bufferLabel = new Label($"Buffer: {_breakerBox.BufferCurrent:F1}");
        bufferLabel.AddToClassList("power-info-label");
        container.Add(bufferLabel);

        var lastReceivedLabel = new Label($"Last Received: {_breakerBox.LastReceived:F1}");
        lastReceivedLabel.AddToClassList("power-info-label");
        container.Add(lastReceivedLabel);

        var runningLabel = new Label($"Running: {(_breakerBox.RunMachine ? "Yes" : "No")}");
        runningLabel.AddToClassList("power-info-label");
        runningLabel.style.color = _breakerBox.RunMachine ? Color.green : Color.red;
        container.Add(runningLabel);

        var requiredPowerLabel = new Label($"Total Required: {_breakerBox.GetTotalRequiredPower():F1}");
        requiredPowerLabel.AddToClassList("power-info-label");
        container.Add(requiredPowerLabel);
    }
}