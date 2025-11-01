using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using static ProjectUniverse.PowerSystem.GlobalRouteResolver;
using System.ComponentModel;
using UnityEditor.MemoryProfiler;
using Component = UnityEngine.Component;

namespace ProjectUniverse.PowerSystem
{
    [System.Serializable]
    public class PowerSystemComponentVisualization
    {
        public Vector3 size = Vector3.one;
        public Vector3 offset = Vector3.zero;
        public Color color = new(1f, 0.5f, 0f, 0.5f); // Orange with transparency
    }

    public class PowerSystemVisualizer : MonoBehaviour
    {
        private static PowerSystemVisualizer _instance;
        public static PowerSystemVisualizer Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<PowerSystemVisualizer>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("PowerSystemVisualizer");
                        _instance = go.AddComponent<PowerSystemVisualizer>();
                    }
                }
                return _instance;
            }
        }

        [Header("Visualization Settings")]
        public bool showPowerComponents = true;
        public bool showCableSegments = true;
        public bool useHologramMaterialInRuntime = false;

        [Header("Layer Settings")]
        public bool setLayerToHologram = true;
        public string visualizationLayer = "Hologram";

        [Header("Component Visualization")]
        public PowerSystemComponentVisualization generatorVisualization = new()
        {
            size = new Vector3(2f, 2f, 2f),
            color = new Color(1f, 1f, 0f, 0.5f) // Yellow
        };

        public PowerSystemComponentVisualization routerVisualization = new()
        {
            size = new Vector3(1.5f, 1.5f, 1.5f),
            color = new Color(0f, 1f, 0f, 0.5f) // Green
        };

        public PowerSystemComponentVisualization substationVisualization = new()
        {
            size = new Vector3(1.2f, 1.2f, 1.2f),
            color = new Color(0f, 0f, 1f, 0.5f) // Blue
        };

        public PowerSystemComponentVisualization breakerBoxVisualization = new()
        {
            size = new Vector3(0.8f, 1f, 0.3f),
            color = new Color(1f, 0f, 1f, 0.5f) // Magenta
        };

        public PowerSystemComponentVisualization machineVisualization = new()
        {
            size = new Vector3(1f, 1f, 1f),
            color = new Color(0.5f, 0.5f, 0.5f, 0.5f) // Gray
        };

        [Header("Cable Visualization")]
        public Color cableColor = new(1f, 0.5f, 0f, 0.8f); // Orange
        public Color connectorColor = new(0.8f, 0.4f, 0f, 0.9f); // Darker orange for connectors
        public float transmissionCableDiameter = 0.5f; // 500mm
        public float distributionCableDiameter = 0.25f; // 250mm
        public float branchCableDiameter = 0.15f; // 150mm
        public float standardSegmentLength = 3f; // 3 meter segments
        [SerializeField] private int frameDelayUpdate = 60;
        private int frameTracker = 0;

        [Header("Health Visualization")]
        public bool showCableHealth = false;
        public Color healthyColor = Color.green;
        public Color damagedColor = Color.yellow;
        public Color criticalColor = Color.red;
        public Color brokenColor = Color.black;

        [Header("Materials")]
        public Material cableMaterial;
        public Material connectorMaterial; // Different material for connectors
        public Material componentMaterial; // Hologram material

        [Header("Debug")]
        public bool debugLogging = false;

        private Dictionary<string, GameObject> cableSegmentObjects = new();
        private Dictionary<string, GameObject> connectorObjects = new();
        private Dictionary<Component, GameObject> componentVisualizationObjects = new();
        private Dictionary<string, GameObject> cableContainers = new();
        private bool wasPlayingLastFrame = false;

        // Add these fields to PowerSystemVisualizer class
        public bool showPathFindingMode = false;
        public bool includeAlternativeRoutings = true; // Show paths that require node changes
        private PowerConnection sourceConnection;
        private PowerConnection destinationConnection;
        private List<GlobalRouteResolver.AlternativePathSequence> currentPathResults = new();
        private Dictionary<string, Material> originalCableMaterials = new();
        private GlobalRouteResolver routeResolver;
        public int maxMatrixChanges;

        [System.Serializable]
        public class PathFinderResult
        {
            public List<PathCable> cables = new();
            public List<PowerNode> nodes = new();
            public List<PowerNode.InternalRoute> routeChanges = new();
            public Color pathColor;
            public int matrixChangesRequired;
            public float totalLength;
        }

        public void EnterPathFindingMode(Component source, Component destination)
        {
            showPathFindingMode = true;

            // Find the GlobalRouteResolver
            routeResolver = FindObjectOfType<GlobalRouteResolver>();
            if (routeResolver == null)
            {
                Debug.LogError("GlobalRouteResolver not found!");
                return;
            }

            // Test working method
            //routeResolver.ExploreGeneratorToRouterPaths(source, destination);

            // Clear previous results
            // /*
            ClearPathFindingVisualization();

            // Ensure connections are discovered
            routeResolver.DetectAndCreatePowerPaths();

            List<PowerConnection> sourceConnections = new();
            List<PowerConnection> destConnections = new();

            // Get all output/input ports on source/dest
            for (int i = 0; i < routeResolver.powerConnections.Count; i++)
            {
                PowerConnection pc = routeResolver.powerConnections[i];
                if (pc.source == source.gameObject)
                {
                    //if (pc.port.connectionType == PowerConnectionPoint.ConnectionType.Output || pc.port.connectionType == PowerConnectionPoint.ConnectionType.Both)
                    //{
                    //Debug.Log($"Found SCP on {source.gameObject}");
                    sourceConnections.Add(pc);
                    //}
                }
                else if (pc.source == destination.gameObject)
                {
                    //if (pc.port.connectionType == PowerConnectionPoint.ConnectionType.Input || pc.port.connectionType == PowerConnectionPoint.ConnectionType.Both)
                    //{
                    //Debug.Log($"Found DCP on {destination.gameObject}");
                    destConnections.Add(pc);
                    //}
                }
            }

            Debug.Log($"Source connections found: {sourceConnections.Count}");
            foreach (var sc in sourceConnections)
            {
                Debug.Log($"  - Port: {sc.port.GetHashCode()}, Type: {sc.port.connectionType}, Pos: {sc.port.localPosition}");
            }

            Debug.Log($"{sourceConnections.Count}, {destConnections.Count}");

            for (int j = 0; j < sourceConnections.Count; j++)
            {
                for (int k = 0; k < destConnections.Count; k++)
                {
                    // Setting to Output/Input assumes the source and destination are properly selected.
                    sourceConnection = sourceConnections[j];//FindPowerConnection(source, PowerConnectionPoint.ConnectionType.Output);
                    destinationConnection = destConnections[k];// FindPowerConnection(destination, PowerConnectionPoint.ConnectionType.Input);

                    Debug.Log($"{sourceConnection.source} to {destinationConnection.source}");

                    if (sourceConnection == null || destinationConnection == null)
                    {
                        Debug.LogError($"PowerConnection for {source?.name} or {destination?.name} is null!");
                        return;
                    }

                    // Find all possible paths
                    FindAllPossiblePaths();
                }
            }
            // */
            // Update visualization
            UpdatePathFindingVisualization();
        }

        public void ExitPathFindingMode()
        {
            showPathFindingMode = false;
            ClearPathFindingVisualization();
            sourceConnection = null;
            destinationConnection = null;
        }

        private void FindAllPossiblePaths()
        {
            // Because we are checking multiple ports, don't clear results
            //currentPathResults.Clear();

            if (routeResolver == null || sourceConnection == null || destinationConnection == null) return;

            // Determine cable size based on component types
            CableSize requiredSize = DetermineCableSize(sourceConnection.source, destinationConnection.source);

            Debug.Log(sourceConnection.port.name + " to " + destinationConnection.port.name);
            Debug.Log($"Dest data: {destinationConnection.source}, {destinationConnection.waypoint.pathId}, {destinationConnection.port},  {destinationConnection.port.localPosition}");

            // Get all possible paths including alternatives
            var results = routeResolver.ExploreAllPossiblePaths(
                sourceConnection,
                destinationConnection,
                requiredSize,
                includeAlternativeRoutings
            );

            Debug.Log($"Found {results.Count} possible paths from {sourceConnection.source.name}: {sourceConnection.port.name} to {destinationConnection.source.name}: {destinationConnection.port.name}");

            // The results are already sorted by GlobalRouteResolver (fewest changes first, then by distance)
            //currentPathResults = results;
            // Will need to check for duplicate paths
            currentPathResults.AddRange(results);
        }

        private CableSize DetermineCableSize(GameObject source, GameObject destination)
        {
            if (source.GetComponent<IGenerator>() != null && destination.GetComponent<IRouter>() != null)
                return CableSize.Transmission;
            if (source.GetComponent<IRouter>() != null && destination.GetComponent<IRoutingSubstation>() != null)
                return CableSize.Distribution;
            return CableSize.Branch;
        }

        private Color GetAlternativePathColor(int index)
        {
            Color[] alternativeColors = new Color[]
            {
                Color.cyan,
                Color.magenta,
                Color.yellow,
                new(0.5f, 0.5f, 1f), // Light blue
                new(1f, 0.5f, 1f), // Pink
                new(0.5f, 1f, 0.5f), // Light green
            };

            return alternativeColors[index % alternativeColors.Length];
        }

        private void UpdatePathFindingVisualization()
        {
            if (!showPathFindingMode || currentPathResults.Count == 0) return;

            // Store original materials
            foreach (var kvp in cableSegmentObjects)
            {
                var renderer = kvp.Value.GetComponent<MeshRenderer>();
                if (renderer != null && !originalCableMaterials.ContainsKey(kvp.Key))
                {
                    originalCableMaterials[kvp.Key] = renderer.material;
                }
            }

            // Visualize each path result
            for (int i = 0; i < currentPathResults.Count; i++)
            {
                var result = currentPathResults[i];
                Color pathColor;

                if (i == 0 && result.requiredNodeChanges.Count == 0)
                {
                    // Current active path (no changes required)
                    pathColor = new Color(1f, 0.5f, 0f); // Orange
                }
                else
                {
                    // Alternative paths
                    pathColor = GetAlternativePathColor(i);
                }

                VisualizePath(result, pathColor);
            }
        }

        private void VisualizePath(GlobalRouteResolver.AlternativePathSequence pathSequence, Color pathColor)
        {
            // Visualize each path segment in the sequence
            foreach (var pathInfo in pathSequence.pathSequence)
            {
                var path = pathInfo.path;
                var template = pathInfo.template;

                // Get all points in the path
                var positions = path.GetPathPositions();

                // Create segments
                for (int i = 0; i < positions.Count - 1; i++)
                {
                    Vector3 worldStart = template.transform.TransformPoint(positions[i]);
                    Vector3 worldEnd = template.transform.TransformPoint(positions[i + 1]);

                    string segmentKey = GenerateSegmentKey(worldStart, worldEnd);

                    if (cableSegmentObjects.TryGetValue(segmentKey, out GameObject segmentObj))
                    {
                        if (segmentObj.TryGetComponent<MeshRenderer>(out var renderer))
                        {
                            ApplyColorToRenderer(renderer, pathColor);

                            // Add additional visual indicator for paths requiring changes
                            if (pathSequence.requiredNodeChanges.Count > 0)
                            {
                                renderer.material.SetFloat("_Metallic", 0.8f);
                            }
                        }
                    }
                }
            }

            // Log the required changes for this path
            if (pathSequence.requiredNodeChanges.Count > 0)
            {
                Debug.Log($"Path requires {pathSequence.requiredNodeChanges.Count} node routing changes:");
                foreach (var change in pathSequence.requiredNodeChanges)
                {
                    Debug.Log($"  - {change.node.name}: Change input {change.fromInput} from output {change.currentOutput} to output {change.toOutput}");
                }
            }
        }

        private void ClearPathFindingVisualization()
        {
            // Restore original colors to cables
            foreach (var kvp in cableSegmentObjects)
            {
                if (kvp.Value != null)
                {
                    if (kvp.Value.TryGetComponent<MeshRenderer>(out var renderer))
                    {
                        ApplyColorToRenderer(renderer, cableColor);
                    }
                }
            }

            originalCableMaterials.Clear();
            currentPathResults.Clear();
        }

        // Not run-time usable, but might be useful later
        [ContextMenu("Apply Best Path Configuration")]
        public void ApplyBestPathConfiguration()
        {
            if (!showPathFindingMode || currentPathResults.Count == 0) return;

            // Get the best path (first one, already sorted)
            var bestPath = currentPathResults[0];

            if (bestPath.requiredNodeChanges.Count == 0)
            {
                Debug.Log("Current configuration is already optimal!");
                return;
            }

            // Apply the node routing changes
            foreach (var change in bestPath.requiredNodeChanges)
            {
                Debug.Log($"Applying routing change to {change.node.name}: Input {change.fromInput} -> Output {change.toOutput}");
                change.node.SetRoute(change.fromInput, change.toOutput, true);
            }

            // Refresh the power system
            if (routeResolver != null)
            {
                routeResolver.DetectAndCreatePowerPaths();
            }

            // Exit path finding mode and refresh visualization
            ExitPathFindingMode();
            UpdateVisualization();
        }

        public string GetCurrentPathInfo()
        {
            if (!showPathFindingMode || currentPathResults.Count == 0)
                return "No paths found";

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Found {currentPathResults.Count} possible paths:");

            for (int i = 0; i < currentPathResults.Count; i++)
            {
                var result = currentPathResults[i];
                sb.AppendLine($"\nPath {i + 1}:");
                sb.AppendLine($"  - Segments: {result.pathSequence.Count}");
                sb.AppendLine($"  - Distance: {result.totalDistance:F2}");
                sb.AppendLine($"  - Node changes required: {result.requiredNodeChanges.Count}");

                if (result.requiredNodeChanges.Count > 0)
                {
                    foreach (var change in result.requiredNodeChanges)
                    {
                        sb.AppendLine($"    * {change.node.name}: Input {change.fromInput} -> Output {change.toOutput}");
                    }
                }
            }

            return sb.ToString();
        }

        private string GenerateSegmentKey(Vector3 start, Vector3 end)
        {
            // Generate a consistent key regardless of direction
            Vector3 min = Vector3.Min(start, end);
            Vector3 max = Vector3.Max(start, end);
            return $"{min.x:F2},{min.y:F2},{min.z:F2}-{max.x:F2},{max.y:F2},{max.z:F2}";
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void Start()
        {
            // Create default materials if not assigned
            if (cableMaterial == null)
            {
                cableMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"))
                {
                    name = "Cable Material"
                };
            }

            if (connectorMaterial == null)
            {
                connectorMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"))
                {
                    name = "Connector Material"
                };
            }

            if (componentMaterial == null)
            {
                componentMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"))
                {
                    name = "Component Material"
                };
                componentMaterial.SetFloat("_Surface", 1); // Set to transparent
                componentMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                componentMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                componentMaterial.SetInt("_ZWrite", 0);
                componentMaterial.renderQueue = 3000;
            }

            // Apply colors to materials
            //UpdateMaterialColors();

            wasPlayingLastFrame = Application.isPlaying;
        }

        private void Update()
        {
            // Detect play mode changes and clear visualizations
            if (Application.isPlaying != wasPlayingLastFrame)
            {
                if (debugLogging) Debug.Log($"Play mode changed: {wasPlayingLastFrame} -> {Application.isPlaying}");
                ForceCleanupAllVisualizationObjects();
                wasPlayingLastFrame = Application.isPlaying;
                // Rebuild visualizations after clearing
                UpdateVisualization();
            }

            if (Application.isPlaying && showCableSegments && showCableHealth)
            {
                UpdateCableHealthVisualization();
            }
            else if(Application.isPlaying && showCableSegments)
            {
                frameTracker -= 1;
                if (frameTracker <= 0)
                {
                    UpdateVisualization();
                    frameTracker = frameDelayUpdate;
                }
            }
        }

        public void UpdateVisualization()
        {
            if (showPowerComponents)
                UpdateComponentVisualizations();
            else
                ClearComponentVisualizations();

            if (showCableSegments)
                UpdateCableVisualizations();
            else
                ClearCableVisualizations();
        }

        public void RefreshCables()
        {
            if (debugLogging) Debug.Log("Refreshing cable visualizations");
            ClearCableVisualizations();
            if (showCableSegments)
                UpdateCableVisualizations();
        }

        public void ClearAllVisualizations()
        {
            if (debugLogging) Debug.Log("Clearing all visualizations");
            ClearComponentVisualizations();
            ClearCableVisualizations();
        }

        private void UpdateComponentVisualizations()
        {
            // Find all power system components
            var generators = FindObjectsByType<IGenerator>(FindObjectsSortMode.None);
            var routers = FindObjectsByType<IRouter>(FindObjectsSortMode.None);
            var substations = FindObjectsByType<IRoutingSubstation>(FindObjectsSortMode.None);
            var breakerBoxes = FindObjectsByType<IBreakerBox>(FindObjectsSortMode.None);
            var machines = FindObjectsByType<IMachine>(FindObjectsSortMode.None);

            // Update generators
            foreach (var gen in generators)
            {
                UpdateComponentVisualization(gen as Component, generatorVisualization);
            }

            // Update routers
            foreach (var router in routers)
            {
                UpdateComponentVisualization(router as Component, routerVisualization);
            }

            // Update substations
            foreach (var substation in substations)
            {
                UpdateComponentVisualization(substation as Component, substationVisualization);
            }

            // Update breaker boxes
            foreach (var breaker in breakerBoxes)
            {
                UpdateComponentVisualization(breaker as Component, breakerBoxVisualization);
            }

            // Update machines
            foreach (var machine in machines)
            {
                UpdateComponentVisualization(machine as Component, machineVisualization);
            }
        }

        private void UpdateComponentVisualization(Component component, PowerSystemComponentVisualization visualization)
        {
            if (component == null) return;

            if (!componentVisualizationObjects.TryGetValue(component, out GameObject visObject))
            {
                visObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                visObject.name = $"{component.GetType().Name}_Visualization";
                visObject.transform.parent = component.transform;

                // Remove collider
#if UNITY_EDITOR
                if (Application.isEditor && !Application.isPlaying)
                    DestroyImmediate(visObject.GetComponent<Collider>());
                else
#endif
                    Destroy(visObject.GetComponent<Collider>());

                // Set material
                var renderer = visObject.GetComponent<MeshRenderer>();
                renderer.sharedMaterial = componentMaterial;

                // Set layer
                if (setLayerToHologram)
                {
                    int layer = LayerMask.NameToLayer(visualizationLayer);
                    if (layer != -1)
                        visObject.layer = layer;
                }

                componentVisualizationObjects[component] = visObject;
            }

            // Update transform
            visObject.transform.localPosition = visualization.offset;
            visObject.transform.localScale = visualization.size;
            visObject.transform.localRotation = Quaternion.identity;

            // Update color
            var meshRenderer = visObject.GetComponent<MeshRenderer>();
            ApplyColorToRenderer(meshRenderer, visualization.color);
        }

        private void UpdateCableVisualizations()
        {
            // Get all room networks
            var roomNetworks = FindObjectsOfType<RoomNetwork>();

            HashSet<string> activeCableIds = new();
            HashSet<string> activeConnectorIds = new();
            int totalPathsProcessed = 0;
            int totalSegmentsCreated = 0;

            foreach (var network in roomNetworks)
            {
                // Find or create container for this room
                GameObject roomContainer = GetOrCreateRoomContainer(network);

                foreach (var template in network.templates)
                {
                    if (template == null) continue;

                    foreach (var path in template.waypointPaths)
                    {
                        totalPathsProcessed++;
                        if (debugLogging) Debug.Log($"Processing path: {path.pathId} in template: {template.templateId}");

                        // Create cable segments for all paths
                        int segmentsCreated = CreateCableSegments(template, path, roomContainer, activeCableIds, activeConnectorIds);
                        totalSegmentsCreated += segmentsCreated;

                        if (debugLogging && segmentsCreated == 0)
                        {
                            Debug.LogWarning($"No segments created for path: {path.pathId}");
                        }
                    }
                }
            }

            if (debugLogging) Debug.Log($"Total paths processed: {totalPathsProcessed}, Total segments created: {totalSegmentsCreated}");

            // Remove old cable segments
            var keysToRemove = cableSegmentObjects.Keys.Where(k => !activeCableIds.Contains(k)).ToList();
            if (debugLogging && keysToRemove.Count > 0) Debug.Log($"Removing {keysToRemove.Count} old cable segments");

            foreach (var key in keysToRemove)
            {
                if (cableSegmentObjects[key] != null)
                {
#if UNITY_EDITOR
                    if (Application.isEditor && !Application.isPlaying)
                        DestroyImmediate(cableSegmentObjects[key]);
                    else
#endif
                        Destroy(cableSegmentObjects[key]);
                }
                cableSegmentObjects.Remove(key);
            }

            // Remove old connectors
            var connectorKeysToRemove = connectorObjects.Keys.Where(k => !activeConnectorIds.Contains(k)).ToList();
            foreach (var key in connectorKeysToRemove)
            {
                if (connectorObjects[key] != null)
                {
#if UNITY_EDITOR
                    if (Application.isEditor && !Application.isPlaying)
                        DestroyImmediate(connectorObjects[key]);
                    else
#endif
                        Destroy(connectorObjects[key]);
                }
                connectorObjects.Remove(key);
            }

            // Update colors based on current state
            if (showCableHealth && Application.isPlaying)
            {
                // Apply health colors
                UpdateCableHealthVisualization();
            }
            else
            {
                // Apply default colors only when not showing health
                UpdateAllCableColors();
            }
        }

        private void UpdateAllCableColors()
        {
            // Update cable colors
            foreach (var kvp in cableSegmentObjects)
            {
                if (kvp.Value != null)
                {
                    if (kvp.Value.TryGetComponent<MeshRenderer>(out var renderer))
                    {
                        ApplyColorToRenderer(renderer, cableColor);
                    }
                }
            }

            // Update connector colors
            foreach (var kvp in connectorObjects)
            {
                if (kvp.Value != null)
                {
                    if (kvp.Value.TryGetComponent<MeshRenderer>(out var renderer))
                    {
                        ApplyColorToRenderer(renderer, connectorColor);
                    }
                }
            }
        }

        private void ApplyColorToRenderer(MeshRenderer renderer, Color color)
        {
            if (renderer == null) return;

            MaterialPropertyBlock props = new();
            renderer.GetPropertyBlock(props);
            props.SetColor("_BaseColor", color);
            props.SetColor("_Color", color); // For built-in render pipeline compatibility
            renderer.SetPropertyBlock(props);
        }

        private void UpdateCableHealthVisualization()
        {
            // Get all path cables from PowerSystemPathManager
            var pathManager = PowerSystemPathManager.Instance;
            if (pathManager == null)
            {
                if (debugLogging) Debug.LogWarning("PowerSystemPathManager.Instance is null");
                return;
            }

            int cablesUpdated = 0;

            // Get all registered path cables for reverse lookup
            var allPathCables = pathManager.GetAllPathCables();

            // Update cable segment colors based on health
            foreach (var kvp in cableSegmentObjects)
            {
                if (kvp.Value == null) continue;

                if (!kvp.Value.TryGetComponent<MeshRenderer>(out var renderer)) continue;

                // Parse segment info from the ID
                // Format: {templateId}_{pathId}_segment_{i}_{subSegmentIndex}
                string[] parts = kvp.Key.Split('_');

                Color segmentColor = Color.gray; // Default color
                bool foundMatch = false;

                if (parts.Length >= 3)
                {
                    // Extract the pathId portion
                    int segmentIndex = -1;
                    for (int i = 0; i < parts.Length; i++)
                    {
                        if (parts[i] == "segment")
                        {
                            segmentIndex = i;
                            break;
                        }
                    }

                    if (segmentIndex > 1)
                    {
                        // PathId is everything between templateId and "segment"
                        string pathId = string.Join("_", parts, 1, segmentIndex - 1);

                        // First try direct lookup
                        var healthInfo = pathManager.GetCableHealthInfo(pathId);
                        if (healthInfo != null)
                        {
                            segmentColor = GetHealthColor(healthInfo.overallHealth, healthInfo.isOperational);
                            foundMatch = true;
                            cablesUpdated++;
                        }
                        else
                        {
                            // Check if this pathId is part of any multi-path cable
                            foreach (var cableEntry in allPathCables)
                            {
                                var cable = cableEntry.Value;
                                if (cable != null)
                                {
                                    var paths = cable.GetPaths();
                                    if (paths != null)
                                    {
                                        foreach (var path in paths)
                                        {
                                            if (path.pathId == pathId)
                                            {
                                                // Found the cable this path belongs to
                                                healthInfo = new PowerSystemPathManager.CableHealthInfo
                                                {
                                                    pathId = pathId,
                                                    overallHealth = cable.GetOverallHealth(),
                                                    maxHeat = cable.GetMaxHeat(),
                                                    isOperational = cable.IsOperational(),
                                                    totalSegments = cable.GetTotalSegments()
                                                };

                                                segmentColor = GetHealthColor(healthInfo.overallHealth, healthInfo.isOperational);
                                                foundMatch = true;
                                                cablesUpdated++;
                                                break;
                                            }
                                        }
                                    }
                                }
                                if (foundMatch) break;
                            }
                        }

                        if (!foundMatch && debugLogging)
                        {
                            Debug.Log($"No health info for pathId: {pathId}");
                        }
                    }
                }

                // Apply the color
                ApplyColorToRenderer(renderer, showCableHealth && foundMatch ? segmentColor : cableColor);
            }

            if (debugLogging) Debug.Log($"Updated health colors for {cablesUpdated} cable segments out of {cableSegmentObjects.Count} total segments");
        }
        
        private Color GetHealthColor(float healthPercentage, bool isOperational)
        {
            if (!isOperational) return brokenColor;
            if (healthPercentage > 0.75f) return healthyColor;
            if (healthPercentage > 0.5f) return damagedColor;
            return criticalColor;
        }

        private GameObject GetOrCreateRoomContainer(RoomNetwork network)
        {
            string containerId = $"CableContainer_{network.roomId}";

            if (!cableContainers.TryGetValue(containerId, out GameObject container))
            {
                container = new GameObject(containerId);
                container.transform.parent = network.transform;
                cableContainers[containerId] = container;
            }

            return container;
        }

        private GameObject GetOrCreatePathContainer(GameObject roomContainer, Template template, WaypointPath path)
        {
            string pathContainerId = $"Path_{template.templateId}_{path.pathId}";
            Transform existing = roomContainer.transform.Find(pathContainerId);

            if (existing != null)
                return existing.gameObject;

            GameObject pathContainer = new(pathContainerId);
            pathContainer.transform.parent = roomContainer.transform;
            return pathContainer;
        }

        private int CreateCableSegments(Template template, WaypointPath path, GameObject roomContainer,
                                      HashSet<string> activeCableIds, HashSet<string> activeConnectorIds)
        {
            List<Vector3> positions = new()
            {
                // Get all positions in world space
                template.transform.TransformPoint(path.entryPoint.position)
            };
            foreach (var waypoint in path.waypoints)
            {
                positions.Add(template.transform.TransformPoint(waypoint.position));
            }
            positions.Add(template.transform.TransformPoint(path.exitPoint.position));

            if (debugLogging) Debug.Log($"Path {path.pathId} has {positions.Count} positions");

            // Determine cable size to use
            CableSize cableSize = CableSize.Branch; // Default
            if (path.assignedCableSize.HasValue)
            {
                cableSize = path.assignedCableSize.Value;
            }
            else if (path.supportedCableSizes != null && path.supportedCableSizes.Length > 0)
            {
                // Use the smallest supported size as default
                cableSize = path.supportedCableSizes.OrderBy(s => (int)s).First();
            }

            // Get diameter for this cable size
            float diameter = GetCableDiameter(cableSize);

            // Create path container
            GameObject pathContainer = GetOrCreatePathContainer(roomContainer, template, path);

            int totalSegmentsCreated = 0;

            // Create connectors at each position
            for (int i = 0; i < positions.Count; i++)
            {
                string connectorId = $"{template.templateId}_{path.pathId}_connector_{i}";
                activeConnectorIds.Add(connectorId);
                CreateConnector(connectorId, positions[i], diameter, pathContainer);
            }

            // Create segments between consecutive points
            for (int i = 0; i < positions.Count - 1; i++)
            {
                // Check if positions are valid
                Vector3 startPos = positions[i];
                Vector3 endPos = positions[i + 1];
                float totalDistance = Vector3.Distance(startPos, endPos);

                if (totalDistance < 0.001f)
                {
                    if (debugLogging) Debug.LogWarning($"Skipping zero-length segment {i} in path {path.pathId}");
                    continue;
                }

                // Break into standard length segments
                Vector3 direction = (endPos - startPos).normalized;
                float remainingDistance = totalDistance;
                int subSegmentIndex = 0;
                Vector3 currentStart = startPos;

                while (remainingDistance > 0.001f)
                {
                    float segmentLength = Mathf.Min(standardSegmentLength, remainingDistance);
                    Vector3 currentEnd = currentStart + direction * segmentLength;

                    string segmentId = $"{template.templateId}_{path.pathId}_segment_{i}_{subSegmentIndex}";
                    activeCableIds.Add(segmentId);

                    CreateCableSegment(segmentId, currentStart, currentEnd, cableSize, pathContainer);
                    totalSegmentsCreated++;

                    currentStart = currentEnd;
                    remainingDistance -= segmentLength;
                    subSegmentIndex++;
                }
            }

            return totalSegmentsCreated;
        }

        private float GetCableDiameter(CableSize cableSize)
        {
            return cableSize switch
            {
                CableSize.Transmission => transmissionCableDiameter,
                CableSize.Distribution => distributionCableDiameter,
                CableSize.Branch => branchCableDiameter,
                _ => branchCableDiameter
            };
        }

        private void CreateConnector(string connectorId, Vector3 position, float diameter, GameObject parent)
        {
            if (!connectorObjects.TryGetValue(connectorId, out GameObject connectorObject))
            {
                connectorObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                connectorObject.name = $"Connector_{connectorId}";
                connectorObject.transform.parent = parent.transform;

                // Remove collider
#if UNITY_EDITOR
                if (Application.isEditor && !Application.isPlaying)
                    DestroyImmediate(connectorObject.GetComponent<Collider>());
                else
#endif
                    Destroy(connectorObject.GetComponent<Collider>());

                // Set initial material based on editor/runtime state
                var renderer = connectorObject.GetComponent<MeshRenderer>();
                renderer.sharedMaterial = GetCurrentConnectorMaterial();

                // Set layer
                if (setLayerToHologram)
                {
                    int layer = LayerMask.NameToLayer(visualizationLayer);
                    if (layer != -1)
                        connectorObject.layer = layer;
                }

                connectorObjects[connectorId] = connectorObject;
            }

            // Position and scale the connector
            connectorObject.transform.position = position;
            connectorObject.transform.localScale = Vector3.one * diameter;

            // Apply color
            if (connectorObject.TryGetComponent<MeshRenderer>(out var meshRenderer))
            {
                ApplyColorToRenderer(meshRenderer, connectorColor);
            }
        }

        private void CreateCableSegment(string segmentId, Vector3 startPos, Vector3 endPos, CableSize cableSize, GameObject parent)
        {

            if (!cableSegmentObjects.TryGetValue(segmentId, out GameObject segmentObject))
            {
                segmentObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                segmentObject.name = $"CableSegment_{segmentId}";
                segmentObject.transform.parent = parent.transform;

                // Remove collider
#if UNITY_EDITOR
                if (Application.isEditor && !Application.isPlaying)
                    DestroyImmediate(segmentObject.GetComponent<Collider>());
                else
#endif
                    Destroy(segmentObject.GetComponent<Collider>());

                // Set initial material based on editor/runtime state
                var renderer = segmentObject.GetComponent<MeshRenderer>();
                renderer.sharedMaterial = GetCurrentCableMaterial();

                // Set layer
                if (setLayerToHologram)
                {
                    int layer = LayerMask.NameToLayer(visualizationLayer);
                    if (layer != -1)
                        segmentObject.layer = layer;
                }

                cableSegmentObjects[segmentId] = segmentObject;

                if (debugLogging) Debug.Log($"Created cable segment: {segmentId}");
            }

            // Position and orient the cylinder
            Vector3 direction = endPos - startPos;
            float distance = direction.magnitude;

            if (distance < 0.001f)
            {
                if (debugLogging) Debug.LogWarning($"Zero-length cable segment: {segmentId}");
                return;
            }

            Vector3 midPoint = (startPos + endPos) / 2f;

            segmentObject.transform.SetPositionAndRotation(midPoint, Quaternion.FromToRotation(Vector3.up, direction.normalized));

            // Scale based on cable size
            float diameter = GetCableDiameter(cableSize);
            segmentObject.transform.localScale = new Vector3(diameter, distance / 2f, diameter);

            // Apply color
            if (segmentObject.TryGetComponent<MeshRenderer>(out var meshRenderer))
            {
                ApplyColorToRenderer(meshRenderer, cableColor);
            }
        }

        private Material GetCurrentCableMaterial()
        {
            if (Application.isPlaying && useHologramMaterialInRuntime)
            {
                return componentMaterial;
            }
            return cableMaterial;
        }

        private Material GetCurrentConnectorMaterial()
        {
            if (Application.isPlaying && useHologramMaterialInRuntime)
            {
                return componentMaterial;
            }
            return connectorMaterial;
        }

        private void ClearComponentVisualizations()
        {
            foreach (var kvp in componentVisualizationObjects)
            {
                if (kvp.Value != null)
                {
#if UNITY_EDITOR
                    if (Application.isEditor && !Application.isPlaying)
                        DestroyImmediate(kvp.Value);
                    else
#endif
                        Destroy(kvp.Value);
                }
            }
            componentVisualizationObjects.Clear();

            // Clean up any orphaned component visualization objects
            CleanupOrphanedVisualizationObjects();
        }

        private void CleanupOrphanedVisualizationObjects()
        {
            // Find all objects that match our visualization naming patterns
            var allObjects = FindObjectsOfType<GameObject>();
            List<GameObject> objectsToDestroy = new();

            for (int i = 0; i < allObjects.Length; i++)
            {
                var obj = allObjects[i];
                if (obj == null) continue;

                // Check for visualization objects
                if (obj.name.EndsWith("_Visualization") &&
                    !componentVisualizationObjects.ContainsValue(obj))
                {
                    if (debugLogging) Debug.Log($"Cleaning up orphaned visualization object: {obj.name}");
                    objectsToDestroy.Add(obj);
                }
            }

            // Destroy collected objects
            for (int i = 0; i < objectsToDestroy.Count; i++)
            {
                if (objectsToDestroy[i] != null)
                {
#if UNITY_EDITOR
                    if (Application.isEditor && !Application.isPlaying)
                        DestroyImmediate(objectsToDestroy[i]);
                    else
#endif
                        Destroy(objectsToDestroy[i]);
                }
            }
        }

        private void CleanupOrphanedCableObjects()
        {
            // Find all objects that match our cable naming patterns
            var allObjects = FindObjectsOfType<GameObject>();
            List<GameObject> objectsToDestroy = new();

            for (int i = 0; i < allObjects.Length; i++)
            {
                var obj = allObjects[i];
                if (obj == null) continue;

                // Check for cable segments
                if (obj.name.StartsWith("CableSegment_") &&
                    !cableSegmentObjects.ContainsValue(obj))
                {
                    if (debugLogging) Debug.Log($"Cleaning up orphaned cable segment: {obj.name}");
                    objectsToDestroy.Add(obj);
                }

                // Check for connectors
                if (obj.name.StartsWith("Connector_") &&
                    !connectorObjects.ContainsValue(obj))
                {
                    if (debugLogging) Debug.Log($"Cleaning up orphaned connector: {obj.name}");
                    objectsToDestroy.Add(obj);
                }

                // Check for cable containers
                if (obj.name.StartsWith("CableContainer_") &&
                    !cableContainers.ContainsValue(obj))
                {
                    if (debugLogging) Debug.Log($"Cleaning up orphaned cable container: {obj.name}");
                    objectsToDestroy.Add(obj);
                }

                // Check for path containers
                if (obj.name.StartsWith("Path_"))
                {
                    // Check if parent is a valid cable container
                    if (obj.transform.parent == null ||
                        !obj.transform.parent.name.StartsWith("CableContainer_"))
                    {
                        if (debugLogging) Debug.Log($"Cleaning up orphaned path container: {obj.name}");
                        objectsToDestroy.Add(obj);
                    }
                }
            }

            // Destroy collected objects
            for (int i = 0; i < objectsToDestroy.Count; i++)
            {
                if (objectsToDestroy[i] != null)
                {
#if UNITY_EDITOR
                    if (Application.isEditor && !Application.isPlaying)
                        DestroyImmediate(objectsToDestroy[i]);
                    else
#endif
                        Destroy(objectsToDestroy[i]);
                }
            }
        }

        private void ClearCableVisualizations()
        {
            // Clear cable segments
            foreach (var kvp in cableSegmentObjects)
            {
                if (kvp.Value != null)
                {
#if UNITY_EDITOR
                    if (Application.isEditor && !Application.isPlaying)
                        DestroyImmediate(kvp.Value);
                    else
#endif
                        Destroy(kvp.Value);
                }
            }
            cableSegmentObjects.Clear();

            // Clear connectors
            foreach (var kvp in connectorObjects)
            {
                if (kvp.Value != null)
                {
#if UNITY_EDITOR
                    if (Application.isEditor && !Application.isPlaying)
                        DestroyImmediate(kvp.Value);
                    else
#endif
                        Destroy(kvp.Value);
                }
            }
            connectorObjects.Clear();

            // Clear containers
            foreach (var kvp in cableContainers)
            {
                if (kvp.Value != null)
                {
#if UNITY_EDITOR
                    if (Application.isEditor && !Application.isPlaying)
                        DestroyImmediate(kvp.Value);
                    else
#endif
                        Destroy(kvp.Value);
                }
            }
            cableContainers.Clear();

            // Clean up any orphaned cable objects
            CleanupOrphanedCableObjects();
        }

        private void OnDestroy()
        {
            ForceCleanupAllVisualizationObjects();
        }

        // Editor context menu methods
#if UNITY_EDITOR
        [ContextMenu("Force Refresh")]
        private void ForceRefreshCables()
        {
            ClearAllVisualizations();
            UpdateVisualization();
        }

        [ContextMenu("Clear All Visualizations")]
        private void ForceClearAll()
        {
            ClearAllVisualizations();
        }

        // Force clear
#if UNITY_EDITOR
        [ContextMenu("Force Cleanup All Visualizations")]
        private void ForceCleanupAllVisualizationObjects()
        {
            Debug.Log("Force cleaning up all visualization objects...");

            ClearAllVisualizations();

            // Extra aggressive cleanup
            var allObjects = FindObjectsOfType<GameObject>();
            var objectsToDestroy = new List<GameObject>();

            for (int i = 0; i < allObjects.Length; i++)
            {
                var obj = allObjects[i];
                if (obj == null) continue;

                if (obj.name.StartsWith("CableSegment_") ||
                    obj.name.StartsWith("Connector_") ||
                    obj.name.StartsWith("CableContainer_") ||
                    obj.name.StartsWith("Path_") ||
                    obj.name.EndsWith("_Visualization"))
                {
                    objectsToDestroy.Add(obj);
                }
            }

            for (int i = objectsToDestroy.Count - 1; i >= 0; i--)
            {
                if (objectsToDestroy[i] != null)
                {
                    DestroyImmediate(objectsToDestroy[i]);
                }
            }
        }
#endif

        [ContextMenu("Toggle Debug Logging")]
        private void ToggleDebugLogging()
        {
            debugLogging = !debugLogging;
            Debug.Log($"Debug logging: {debugLogging}");
        }

        [ContextMenu("Update Material Colors")]
        private void ForceUpdateMaterialColors()
        {
            UpdateAllCableColors();

            // Update component colors
            foreach (var kvp in componentVisualizationObjects)
            {
                if (kvp.Value != null)
                {
                    if (kvp.Value.TryGetComponent<MeshRenderer>(out var renderer))
                    {
                        // Find the appropriate visualization settings
                        PowerSystemComponentVisualization vis = null;
                        if (kvp.Key is IGenerator) vis = generatorVisualization;
                        else if (kvp.Key is IRouter) vis = routerVisualization;
                        else if (kvp.Key is IRoutingSubstation) vis = substationVisualization;
                        else if (kvp.Key is IBreakerBox) vis = breakerBoxVisualization;
                        else if (kvp.Key is IMachine) vis = machineVisualization;

                        if (vis != null)
                        {
                            ApplyColorToRenderer(renderer, vis.color);
                        }
                    }
                }
            }
        }
#endif
    }
}