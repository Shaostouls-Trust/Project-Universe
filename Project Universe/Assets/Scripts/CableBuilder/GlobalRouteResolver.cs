using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEditor.MemoryProfiler;
using UnityEngine.Rendering.VirtualTexturing;

namespace ProjectUniverse.PowerSystem
{
    public class GlobalRouteResolver : MonoBehaviour
    {
        private static GlobalRouteResolver _instance;
        public static GlobalRouteResolver Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<GlobalRouteResolver>();
                    if (_instance == null)
                    {
                        GameObject go = new("Global Route Resolver");
                        _instance = go.AddComponent<GlobalRouteResolver>();
                    }
                }
                return _instance;
            }
        }

        [Header("Configuration")]
        public float boundaryConnectionThreshold = 0.5f; // Distance threshold for boundary port connections

        [Header("Networks")]
        public List<RoomNetwork> roomNetworks = new();

        [Header("Power System Integration")]
        public bool detectPowerPaths = true;
        public float pathDetectionInterval = 5f; // How often to check for power paths
        private float lastPathDetectionTime;

        [System.Serializable]
        public class BoundaryConnection
        {
            public NetworkBoundaryPort portA;
            public RoomNetwork networkA;
            public NetworkBoundaryPort portB;
            public RoomNetwork networkB;
            public bool isSizeCompatible;

            public BoundaryConnection(NetworkBoundaryPort a, RoomNetwork netA,
                                    NetworkBoundaryPort b, RoomNetwork netB)
            {
                portA = a;
                networkA = netA;
                portB = b;
                networkB = netB;
                isSizeCompatible = a.CanConnectTo(b);
            }
        }

        [System.Serializable]
        public class PowerConnection
        {
            public PowerConnectionPoint port;
            public WaypointPath waypoint;
            public GameObject source;
            public PowerConnection(PowerConnectionPoint pcp, WaypointPath wp, GameObject go)
            {
                port = pcp;
                waypoint = wp;
                source = go;
            }

            public bool Equals(PowerConnection other)
            {
                if (other == null) return false;
                return port.name == other.port.name &&
                       port.localPosition == other.port.localPosition &&
                       waypoint == other.waypoint &&
                       source == other.source;
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as PowerConnection);
            }
            public override int GetHashCode()
            {
                return System.HashCode.Combine(port, waypoint, source);
            }
        }

        public List<BoundaryConnection> boundaryConnections = new();

        public List<PowerConnection> powerConnections = new();

        [Header("Power System")]
        [SerializeField] private IGenerator[] generators;
        [SerializeField] private IRouter[] routers;
        [SerializeField] private IRoutingSubstation[] substations;
        [SerializeField] private IBreakerBox[] breakers;
        [SerializeField] private IMachine[] machines;


        [Header("Visualization")]
        public bool showBoundaryConnections = true;
        public Color compatibleConnectionColor = Color.green;
        public Color incompatibleConnectionColor = Color.red;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
        }

        private void Update()
        {
            if (detectPowerPaths && Application.isPlaying && Time.time - lastPathDetectionTime > pathDetectionInterval)
            {
                DetectAndCreatePowerPaths();
                lastPathDetectionTime = Time.time;
            }
        }

        public void DiscoverPowerComponents()
        {
            generators = new IGenerator[0];
            routers = new IRouter[0];
            substations = new IRoutingSubstation[0];
            breakers = new IBreakerBox[0];
            machines = new IMachine[0];
            //Lists for easy
            foreach (var room in roomNetworks)
            {
                generators = room.gameObject.GetComponentsInChildren<IGenerator>(true).Concat(generators).ToArray();
                routers = room.gameObject.GetComponentsInChildren<IRouter>(true).Concat(routers).ToArray();
                substations = room.gameObject.GetComponentsInChildren<IRoutingSubstation>(true).Concat(substations).ToArray();
                breakers = room.gameObject.GetComponentsInChildren<IBreakerBox>(true).Concat(breakers).ToArray();
                machines = room.gameObject.GetComponentsInChildren<IMachine>(true).Concat(machines).ToArray();
            }
        }

        private void Start()
        {
            DetectAndCreatePowerPaths();
        }

        public void DetectAndCreatePowerPaths()
        {
            Debug.Log("Detecting power paths...");

            // First, ensure boundary connections are resolved
            ResolveBoundaryConnections();

            // Find all power system components (that are inside of rooms)
            DiscoverPowerComponents();

            Debug.Log($"Found: {generators.Length} generators, {routers.Length} routers, " +
                      $"{substations.Length} substations, {breakers.Length} breaker boxes, {machines.Length} machines");

            // Find Power connections
            FindPowerComponentConnections();

            List<PowerConnection> genConnections = new();
            List<PowerConnection> rouConnections = new();
            List<PowerConnection> subConnections = new();
            List<PowerConnection> brkConnections = new();
            List<PowerConnection> machConnections = new();

            // Grab the connection points for the power system components that are connected
            for (int i = 0; i < powerConnections.Count; i++)
            {
                GameObject obj = powerConnections[i].source;
                if (obj.TryGetComponent(out IMachine _))
                {
                    machConnections.Add(powerConnections[i]);
                }
                else if (obj.TryGetComponent(out IBreakerBox _))
                {
                    brkConnections.Add(powerConnections[i]);
                }
                else if (obj.TryGetComponent(out IRoutingSubstation _))
                {
                    subConnections.Add(powerConnections[i]);
                }
                else if (obj.TryGetComponent(out IRouter _))
                {
                    rouConnections.Add(powerConnections[i]);
                }
                else if (obj.TryGetComponent(out IGenerator _))
                {
                    genConnections.Add(powerConnections[i]);
                }
            }

            // Create paths between components
            CreateGeneratorToRouterPaths(genConnections, rouConnections);
            CreateRouterToSubstationPaths(rouConnections, subConnections);
            CreateSubstationToMachinePaths(subConnections, machConnections);
            CreateSubstationToBreakerBoxPaths(subConnections, brkConnections);

            // Print the info because I'm curious. (Keep this around for a while)
            /*Dictionary<Component, List<PathCable>> compConns = PowerSystemPathManager.Instance.ComponentConnections;
            foreach (Component comp in compConns.Keys)
            {
                if (compConns.TryGetValue(comp, out List<PathCable> pcl))
                {
                    Debug.Log("Component; " + comp);
                    foreach (var pc in pcl)
                    {
                        foreach (var wp in pc.GetPaths())
                        {
                            Debug.Log(wp.pathId);
                        }

                    }
                }

            }*/
            Debug.Log("Power path detection complete");
        }

        [ContextMenu("Detect Power Paths")]
        public void DetectPowerPathsEditor()
        {
            DetectAndCreatePowerPaths();
        }

        private void CreateGeneratorToRouterPaths(List<PowerConnection> generators, List<PowerConnection> routers)
        {
            foreach (PowerConnection gen in generators)
            {
                foreach (PowerConnection router in routers)
                {

                    var pathSequence = FindShortestPathSequence(gen, router, CableSize.Transmission);
                    if (pathSequence != null && pathSequence.Count > 0)
                    {
                        Debug.Log("Generator -> Router connection found.");
                        if (pathSequence.Count == 1)
                        {
                            PowerSystemPathManager.Instance.CreatePathConnection(gen.source.GetComponent<IGenerator>(),
                            router.source.GetComponent<IRouter>(), pathSequence[0].path, pathSequence[0].template);
                        }
                        else
                        {
                            PowerSystemPathManager.Instance.CreateMultiPathConnection(gen.source.GetComponent<IGenerator>(),
                                router.source.GetComponent<IRouter>(), pathSequence);
                        }
                    }
                }
            }
        }

        private void CreateRouterToSubstationPaths(List<PowerConnection> routers, List<PowerConnection> substations)
        {
            foreach (var router in routers)
            {
                foreach (var substation in substations)
                {
                    var pathSequence = FindShortestPathSequence(router, substation, CableSize.Distribution);
                    if (pathSequence != null && pathSequence.Count > 0)
                    {
                        Debug.Log("Router -> Substation connection found.");
                        if (pathSequence.Count == 1)
                        {
                            PowerSystemPathManager.Instance.CreatePathConnection(router.source.GetComponent<IRouter>(),
                            substation.source.GetComponent<IRoutingSubstation>(), pathSequence[0].path, pathSequence[0].template);
                        }
                        else
                        {
                            PowerSystemPathManager.Instance.CreateMultiPathConnection(router.source.GetComponent<IRouter>(),
                                substation.source.GetComponent<IRoutingSubstation>(), pathSequence);
                        }
                    }
                }
            }
        }

        private void CreateSubstationToMachinePaths(List<PowerConnection> substations, List<PowerConnection> machines)
        {
            foreach (var substation in substations)
            {
                foreach (var machine in machines)
                {
                    var pathSequence = FindShortestPathSequence(substation, machine, CableSize.Branch);
                    if (pathSequence != null && pathSequence.Count > 0)
                    {
                        Debug.Log("Substation -> Machine connection found.");
                        if (pathSequence.Count == 1)
                        {
                            PowerSystemPathManager.Instance.CreatePathConnection(substation.source.GetComponent<IRoutingSubstation>(),
                            machine.source.GetComponent<IMachine>(), pathSequence[0].path, pathSequence[0].template);
                        }
                        else
                        {
                            PowerSystemPathManager.Instance.CreateMultiPathConnection(substation.source.GetComponent<IRoutingSubstation>(),
                                machine.source.GetComponent<IMachine>(), pathSequence);
                        }
                    }
                }
            }
        }

        private void CreateSubstationToBreakerBoxPaths(List<PowerConnection> substations, List<PowerConnection> breakerBoxes)
        {
            foreach (var substation in substations)
            {
                foreach (var breaker in breakerBoxes)
                {
                    //Debug.Log(substation.source + " to " + breaker.source);
                    var pathSequence = FindShortestPathSequence(substation, breaker, CableSize.Branch);
                    if (pathSequence != null && pathSequence.Count > 0)
                    {
                        Debug.Log("Substation -> Breaker connection found.");
                        if (pathSequence.Count == 1)
                        {
                            PowerSystemPathManager.Instance.CreatePathConnection(substation.source.GetComponent<IRoutingSubstation>(),
                            breaker.source.GetComponent<IBreakerBox>(), pathSequence[0].path, pathSequence[0].template);
                        }
                        else
                        {
                            PowerSystemPathManager.Instance.CreateMultiPathConnection(substation.source.GetComponent<IRoutingSubstation>(),
                                breaker.source.GetComponent<IBreakerBox>(), pathSequence);
                        }
                    }
                }
            }
        }

        private List<PathInfo> FindShortestPathSequence(PowerConnection source, PowerConnection target, CableSize requiredSize)
        {
            // Get the waypoints connected to them
            WaypointPath sourceWP = source.waypoint;
            WaypointPath targetWP = target.waypoint;

            if (sourceWP == null || targetWP == null) return null;

            // Check if both connections are on the same path
            if (sourceWP.pathId == targetWP.pathId)
            {
                // Find the roomNetwork and template
                RoomNetwork room = source.source.gameObject.GetComponentInParent<RoomNetwork>();
                if (room != null)
                {
                    Template[] templates = room.gameObject.GetComponentsInChildren<Template>();
                    foreach (Template temp in templates)
                    {
                        foreach (WaypointPath wp in temp.waypointPaths)
                        {
                            if (sourceWP.pathId == wp.pathId)
                            {
                                List<PathInfo> pathInfList = new()
                        {
                            new PathInfo(sourceWP, temp, room)
                        };
                                return pathInfList;
                            }
                        }
                    }
                }
            }

            // Use new pathfinding algorithm for multi-path connections
            var pathFinder = new PowerConnectionPathFinder(this);
            return pathFinder.FindShortestPath(source, target, requiredSize);
        }

        public class PathInfo
        {
            public WaypointPath path;
            public Template template;
            public RoomNetwork room;

            public PathInfo(WaypointPath p, Template t, RoomNetwork r)
            {
                path = p;
                template = t;
                room = r;
            }
        }

        private class PowerConnectionPathFinder
        {
            private readonly GlobalRouteResolver resolver;
            //private Dictionary<PowerNode, int> nodeOutputSelections = new Dictionary<PowerNode, int>();

            public PowerConnectionPathFinder(GlobalRouteResolver res)
            {
                resolver = res;
            }

            public List<PathInfo> FindShortestPath(PowerConnection source, PowerConnection target, CableSize requiredSize)
            {
                // Special case: same waypoint
                if (source.waypoint.pathId == target.waypoint.pathId)
                {
                    RoomNetwork room = source.source.gameObject.GetComponentInParent<RoomNetwork>();
                    if (room != null)
                    {
                        Template[] templates = room.gameObject.GetComponentsInChildren<Template>();
                        foreach (Template temp in templates)
                        {
                            foreach (WaypointPath wp in temp.waypointPaths)
                            {
                                if (source.waypoint.pathId == wp.pathId)
                                {
                                    return new List<PathInfo> { new(source.waypoint, temp, room) };
                                }
                            }
                        }
                    }
                }

                // Build graph using shared method
                var graph = resolver.BuildWaypointGraph(requiredSize);

                // Find shortest path
                var path = resolver.FindShortestWaypointPath(source.waypoint, target.waypoint, graph);

                if (path == null || path.Count == 0) return null;

                // Convert to PathInfo list
                return resolver.ConvertToPathInfoList(path);
            }
        }

        private class WaypointConnection
        {
            public WaypointPath fromPath;
            public WaypointPath toPath;
            public Template fromTemplate;
            public Template toTemplate;
            public RoomNetwork room;
            public PowerNode throughNode;
            public int nodeInputIndex = -1;
            public int nodeOutputIndex = -1;
            public bool crossRoomConnection = false;
            public float distance;
        }
        private Dictionary<string, List<WaypointConnection>> BuildWaypointGraph(CableSize requiredSize, Dictionary<string, NodeRoutingOverride> nodeOverrides = null)
        {
            var graph = new Dictionary<string, List<WaypointConnection>>();

            // Add template-to-template connections
            foreach (var room in roomNetworks)
            {
                foreach (var conn in room.connections)
                {
                    if (!conn.isSizeCompatible) continue;
                    if (!conn.sourcePath.CanSupportCableSize(requiredSize)) continue;
                    if (!conn.targetPath.CanSupportCableSize(requiredSize)) continue;

                    string sourceId = conn.sourcePath.pathId;
                    if (!graph.ContainsKey(sourceId))
                        graph[sourceId] = new List<WaypointConnection>();

                    graph[sourceId].Add(new WaypointConnection
                    {
                        fromPath = conn.sourcePath,
                        toPath = conn.targetPath,
                        fromTemplate = conn.sourceTemplate,
                        toTemplate = conn.targetTemplate,
                        room = room,
                        distance = Vector3.Distance(
                            conn.GetSourceWorldPosition(),
                            conn.GetTargetWorldPosition()
                        )
                    });
                }

                // Add PowerNode connections
                foreach (var node in room.nodes)
                {
                    foreach (var inputConn in node.activeConnections.Where(c => c.isInput))
                    {
                        int outputIndex;

                        // Check if we have an override for this node
                        if (nodeOverrides != null && nodeOverrides.ContainsKey(node.nodeId))
                        {
                            var override_ = nodeOverrides[node.nodeId];
                            if (!override_.inputToOutputMap.TryGetValue(inputConn.pointIndex, out outputIndex))
                                continue;
                        }
                        else
                        {
                            // Use current routing
                            var route = node.GetRouteFromInput(inputConn.pointIndex);
                            if (route == null || !route.isConnected) continue;
                            outputIndex = route.outputIndex;
                        }

                        var outputConn = node.activeConnections.FirstOrDefault(c =>
                            !c.isInput && c.pointIndex == outputIndex);

                        if (outputConn != null)
                        {
                            string inputId = inputConn.connectedPath.pathId;
                            if (!graph.ContainsKey(inputId))
                                graph[inputId] = new List<WaypointConnection>();

                            graph[inputId].Add(new WaypointConnection
                            {
                                fromPath = inputConn.connectedPath,
                                toPath = outputConn.connectedPath,
                                fromTemplate = inputConn.connectedTemplate,
                                toTemplate = outputConn.connectedTemplate,
                                room = room,
                                throughNode = node,
                                nodeInputIndex = inputConn.pointIndex,
                                nodeOutputIndex = outputIndex,
                                distance = 2f
                            });
                        }
                    }
                }

                // Add boundary connections
                foreach (var port in room.boundaryPorts)
                {
                    if (!port.IsConnected()) continue;

                    foreach (var otherRoom in roomNetworks)
                    {
                        if (otherRoom == room) continue;

                        foreach (var otherPort in otherRoom.boundaryPorts)
                        {
                            if (!otherPort.IsConnected()) continue;

                            float distance = Vector3.Distance(port.GetWorldPosition(), otherPort.GetWorldPosition());
                            if (distance <= boundaryConnectionThreshold)
                            {
                                string fromId = port.activeConnection.connectedPath.pathId;
                                string toId = otherPort.activeConnection.connectedPath.pathId;

                                if (!graph.ContainsKey(fromId))
                                    graph[fromId] = new List<WaypointConnection>();

                                graph[fromId].Add(new WaypointConnection
                                {
                                    fromPath = port.activeConnection.connectedPath,
                                    toPath = otherPort.activeConnection.connectedPath,
                                    fromTemplate = port.activeConnection.connectedTemplate,
                                    toTemplate = otherPort.activeConnection.connectedTemplate,
                                    room = room,
                                    crossRoomConnection = true,
                                    distance = distance
                                });
                            }
                        }
                    }
                }
            }

            return graph;
        }

        private class NodeRoutingOverride
        {
            public Dictionary<int, int> inputToOutputMap = new();
        }

        public void DiscoverRoomNetworks()
        {
            roomNetworks.Clear();
            roomNetworks.AddRange(FindObjectsByType<RoomNetwork>(FindObjectsSortMode.None));
        }

        internal void FindPowerComponentConnections()
        {
            powerConnections.Clear();
            foreach (RoomNetwork room in roomNetworks)
            {
                float connectionThreshold = room.connectionThreshold;

                foreach (var template in room.templates)
                {
                    if (template == null) continue;

                    foreach (var path in template.waypointPaths)
                    {
                        // Check if path entry connects to comp output
                        ConnectionPoint pathEntry = path.GetEntryPoint();
                        Vector3 entryWorldPos = template.transform.TransformPoint(pathEntry.position);

                        // Check if path exit connects to node input
                        ConnectionPoint pathExit = path.GetExitPoint();
                        Vector3 exitWorldPos = template.transform.TransformPoint(pathExit.position);
                        //Debug.Log(entryWorldPos+" "+ exitWorldPos);

                        for (int a = 0; a < generators.Length; a++)
                        {
                            foreach (var gin in generators[a].ConnectionPoints)
                            {
                                Vector3 genInputWorldPos = generators[a].transform.TransformPoint(gin.localPosition);

                                if (Vector3.Distance(entryWorldPos, genInputWorldPos) <= connectionThreshold
                                    || Vector3.Distance(exitWorldPos, genInputWorldPos) <= connectionThreshold)
                                {
                                    //Debug.Log("Connected gen pComp to waypoint.");
                                    PowerConnection pc = new(gin, path, generators[a].gameObject);
                                    bool add = true;
                                    for(int x = 0; x < powerConnections.Count; x++)
                                    {
                                        if (powerConnections[x].Equals(pc))
                                        {
                                            add = false;
                                        }
                                    }
                                    if (add)
                                    {
                                        powerConnections.Add(pc);
                                        Debug.Log($"Creating PowerConnection - Port HashCode: {gin.GetHashCode()}, Source: {generators[a].gameObject.name}");
                                    }
                                    PowerSystemPathManager.Instance.RegisterComponentPathConnection(generators[a], path, template);
                                }
                            }
                        }

                        for (int a = 0; a < routers.Length; a++)
                        {
                            foreach (var gin in routers[a].ConnectionPoints)
                            {
                                Vector3 genInputWorldPos = routers[a].transform.TransformPoint(gin.localPosition);
                                //Debug.Log(genInputWorldPos);

                                if (Vector3.Distance(entryWorldPos, genInputWorldPos) <= connectionThreshold
                                        || Vector3.Distance(exitWorldPos, genInputWorldPos) <= connectionThreshold)
                                {
                                    //Debug.Log("Connected rou pComp to waypoint.");
                                    PowerConnection pc = new(gin, path, routers[a].gameObject);
                                    if (!powerConnections.Contains(pc))
                                    {
                                        powerConnections.Add(pc);
                                    }
                                    PowerSystemPathManager.Instance.RegisterComponentPathConnection(routers[a], path, template);
                                }
                            }
                        }

                        for (int a = 0; a < substations.Length; a++)
                        {
                            foreach (var gin in substations[a].ConnectionPoints)
                            {
                                Vector3 genInputWorldPos = substations[a].transform.TransformPoint(gin.localPosition);

                                if (Vector3.Distance(entryWorldPos, genInputWorldPos) <= connectionThreshold
                                    || Vector3.Distance(exitWorldPos, genInputWorldPos) <= connectionThreshold)
                                {
                                    //Debug.Log("Connected sub pComp to waypoint.");
                                    PowerConnection pc = new(gin, path, substations[a].gameObject);
                                    if (!powerConnections.Contains(pc))
                                    {
                                        powerConnections.Add(pc);
                                    }
                                    PowerSystemPathManager.Instance.RegisterComponentPathConnection(substations[a], path, template);
                                }
                            }
                        }

                        for (int a = 0; a < breakers.Length; a++)
                        {
                            Vector3 brkInputWorldPos = breakers[a].ConnectionPoint.GetWorldPosition();
                            if (Vector3.Distance(entryWorldPos, brkInputWorldPos) <= connectionThreshold
                                || Vector3.Distance(exitWorldPos, brkInputWorldPos) <= connectionThreshold)
                            {
                                //Debug.Log("Connected brk pComp to waypoint.");
                                PowerConnection pc = new(breakers[a].ConnectionPoint, path, breakers[a].gameObject);
                                if (!powerConnections.Contains(pc))
                                {
                                    powerConnections.Add(pc);
                                }
                                PowerSystemPathManager.Instance.RegisterComponentPathConnection(breakers[a], path, template);
                            }
                        }

                        for (int a = 0; a < machines.Length; a++)
                        {
                            Vector3 brkInputWorldPos = machines[a].connectionPoint.GetWorldPosition();
                            if (Vector3.Distance(entryWorldPos, brkInputWorldPos) <= connectionThreshold
                                || Vector3.Distance(exitWorldPos, brkInputWorldPos) <= connectionThreshold)
                            {
                                //Debug.Log("Connected m pComp to waypoint.");
                                PowerConnection pc = new(machines[a].connectionPoint, path, machines[a].gameObject);
                                if (!powerConnections.Contains(pc))
                                {
                                    powerConnections.Add(pc);
                                }
                                PowerSystemPathManager.Instance.RegisterComponentPathConnection(machines[a], path, template);
                            }
                        }
                    }
                }
            }
        }

        [System.Serializable]
        public class AlternativePathSequence
        {
            public List<PathInfo> pathSequence;
            public List<NodeRoutingChange> requiredNodeChanges;
            public float totalDistance;

            [System.NonSerialized]
            public Color pathColor = Color.white;

            [System.Serializable]
            public class NodeRoutingChange
            {
                public PowerNode node;
                public int fromInput;
                public int toOutput;
                public int currentOutput; // What it's currently connected to

                public NodeRoutingChange(PowerNode n, int input, int output, int current)
                {
                    node = n;
                    fromInput = input;
                    toOutput = output;
                    currentOutput = current;
                }
            }

            public AlternativePathSequence(List<PathInfo> paths)
            {
                pathSequence = new List<PathInfo>(paths);
                requiredNodeChanges = new List<NodeRoutingChange>();
                totalDistance = 0f;
            }
        }

        public List<AlternativePathSequence> ExploreAllPossiblePaths(PowerConnection source, PowerConnection target, CableSize requiredSize, bool includeCurrentRouting = true)
        {
            Debug.Log($"Exploring all possible paths from {source.source.name} to {target.source.name}");

            var allPaths = new List<AlternativePathSequence>();

            // Special case: same waypoint
            if (source.waypoint.pathId == target.waypoint.pathId)
            {
                var room = source.source.gameObject.GetComponentInParent<RoomNetwork>();
                if (room != null)
                {
                    var templates = room.gameObject.GetComponentsInChildren<Template>();
                    foreach (var temp in templates)
                    {
                        foreach (var wp in temp.waypointPaths)
                        {
                            if (source.waypoint.pathId == wp.pathId)
                            {
                                var sequence = new AlternativePathSequence(new List<PathInfo>
                        {
                            new(source.waypoint, temp, room)
                        });
                                allPaths.Add(sequence);
                                return allPaths;
                            }
                        }
                    }
                }
            }

            // First, try with current routing
            if (includeCurrentRouting)
            {
                var currentGraph = BuildWaypointGraph(requiredSize);
                var currentPath = FindShortestWaypointPath(source.waypoint, target.waypoint, currentGraph);

                if (currentPath != null && currentPath.Count > 0)
                {
                    var pathInfos = ConvertToPathInfoList(currentPath);
                    var sequence = new AlternativePathSequence(pathInfos)
                    {
                        totalDistance = currentPath.Sum(c => c.distance)
                    };
                    allPaths.Add(sequence);
                }
            }

            // Find nodes that are on potential paths
            var relevantNodes = FindRelevantNodes(source, target, requiredSize);

            // For each relevant node, try alternative routings
            foreach (var node in relevantNodes)
            {
                var alternatives = GenerateAlternativeRoutings(node, source, target, requiredSize);
                allPaths.AddRange(alternatives);
            }

            // Remove duplicates and sort
            allPaths = RemoveDuplicatePaths(allPaths);
            allPaths.Sort((a, b) =>
            {
                int changeComparison = a.requiredNodeChanges.Count.CompareTo(b.requiredNodeChanges.Count);
                if (changeComparison != 0) return changeComparison;
                return a.totalDistance.CompareTo(b.totalDistance);
            });

            // Log results
            Debug.Log($"Found {allPaths.Count} unique path configurations:");
            foreach (var path in allPaths)
            {
                Debug.Log($"  - Path with {path.pathSequence.Count} segments, {path.requiredNodeChanges.Count} node changes, distance: {path.totalDistance:F2}");
                foreach (var change in path.requiredNodeChanges)
                {
                    Debug.Log($"    * {change.node.name}: Change input {change.fromInput} from output {change.currentOutput} to output {change.toOutput}");
                }
            }

            return allPaths;
        }

        [ContextMenu("Explore Generator to Router Paths")]
        public void ExploreGeneratorToRouterPaths()
        {
            DetectAndCreatePowerPaths(); // Ensure we have current connections

            List<PowerConnection> genConnections = new();
            List<PowerConnection> rouConnections = new();

            foreach (var pc in powerConnections)
            {
                if (pc.source.TryGetComponent(out IRouter _))
                    genConnections.Add(pc);
                else if (pc.source.TryGetComponent(out IRoutingSubstation _))
                    rouConnections.Add(pc);
            }

            foreach (var gen in genConnections)
            {
                foreach (var router in rouConnections)
                {
                    Debug.Log(gen.port.name + " " + router.port.name);
                    var alternatives = ExploreAllPossiblePaths(gen, router, CableSize.Transmission);
                    if (alternatives.Count > 0)
                    {
                        Debug.Log($"=== {gen.source.name} to {router.source.name}: {alternatives.Count} possible configurations ===");
                    }
                }
            }
        }

        public void ExploreGeneratorToRouterPaths(Component source, Component destination)
        {
            DetectAndCreatePowerPaths(); // Ensure we have current connections (maybe hide?)(maybe the node ref to IRoutingSub is stale?)

            List<PowerConnection> sourceConnections = new();
            List<PowerConnection> destConnections = new();

            foreach (var pc in powerConnections)
            {
                if (pc.source.TryGetComponent(out IGenerator gen))
                {
                    Debug.Log("Gen:");
                    // Check if Components are equal
                    //Debug.Log((gen as Component).Equals(source));
                    if ((gen as Component).Equals(source))
                    {
                        sourceConnections.Add(pc);
                    }
                }

                else if (pc.source.TryGetComponent(out IRouter rou))
                {
                    Debug.Log("Router:");
                    Debug.Log(rou.gameObject + "=? " + destination.gameObject);
                    //Debug.Log((rou as Component).Equals(destination));
                    if ((rou as Component).Equals(destination))
                    {
                        destConnections.Add(pc);
                    }
                    else if ((rou as Component).Equals(source))
                    {
                        sourceConnections.Add(pc);
                    }
                }

                else if (pc.source.TryGetComponent(out IRoutingSubstation sub))
                {
                    Debug.Log("Substation:");
                    Debug.Log(sub.gameObject + "=? " + destination.gameObject);
                    //Debug.Log((sub as Component).Equals(destination));
                    if ((sub as Component).Equals(destination))
                    {
                        destConnections.Add(pc);
                    }
                    else if ((sub as Component).Equals(source))
                    {
                        sourceConnections.Add(pc);
                    }
                }

                else if (pc.source.TryGetComponent(out IBreakerBox bkr))
                {
                    Debug.Log("Breaker:");
                    Debug.Log(bkr.gameObject + "=? " + destination.gameObject);
                    //Debug.Log((sub as Component).Equals(destination));
                    if ((bkr as Component).Equals(destination))
                    {
                        destConnections.Add(pc);
                    }
                }

                else if (pc.source.TryGetComponent(out IMachine mac))
                {
                    Debug.Log("Machine:");
                    Debug.Log(mac.gameObject + "=? " + destination.gameObject);
                    //Debug.Log((sub as Component).Equals(destination));
                    if ((mac as Component).Equals(destination))
                    {
                        destConnections.Add(pc);
                    }
                }
            }

            foreach (var s in sourceConnections)
            {
                foreach (var d in destConnections)
                {
                    Debug.Log(s.port.name + " " + d.port.name);
                    var alternatives = ExploreAllPossiblePaths(s, d, CableSize.Transmission);
                    if (alternatives.Count > 0)
                    {
                        Debug.Log($"=== {s.source.name} to {d.source.name}: {alternatives.Count} possible configurations ===");
                    }
                }
            }
        }

        private List<AlternativePathSequence> GenerateAlternativeRoutings(PowerNode node, PowerConnection source, PowerConnection target, CableSize requiredSize)
        {
            var results = new List<AlternativePathSequence>();

            // Get current routing
            var currentRouting = new Dictionary<int, int>();
            foreach (var route in node.internalRoutes.Where(r => r.isConnected))
            {
                currentRouting[route.inputIndex] = route.outputIndex;
            }

            // Find which inputs and outputs are actually connected
            var connectedInputs = node.activeConnections.Where(c => c.isInput).Select(c => c.pointIndex).ToList();
            var connectedOutputs = node.activeConnections.Where(c => !c.isInput).Select(c => c.pointIndex).ToList();

            // Generate only meaningful permutations (connected inputs to connected outputs)
            foreach (var outputPermutation in GetPermutations(connectedOutputs, connectedInputs.Count))
            {
                var outputList = outputPermutation.ToList();
                var newRouting = new Dictionary<int, int>();

                for (int i = 0; i < connectedInputs.Count && i < outputList.Count; i++)
                {
                    newRouting[connectedInputs[i]] = outputList[i];
                }

                // Skip if this is the current routing
                bool isCurrent = true;
                foreach (var kvp in newRouting)
                {
                    if (!currentRouting.ContainsKey(kvp.Key) || currentRouting[kvp.Key] != kvp.Value)
                    {
                        isCurrent = false;
                        break;
                    }
                }
                if (isCurrent) continue;

                // Try path with this routing
                var overrides = new Dictionary<string, NodeRoutingOverride>
                {
                    [node.nodeId] = new NodeRoutingOverride { inputToOutputMap = newRouting }
                };

                var graph = BuildWaypointGraph(requiredSize, overrides);
                var path = FindShortestWaypointPath(source.waypoint, target.waypoint, graph);

                if (path != null && path.Count > 0)
                {
                    // Check if this path actually uses the node we modified
                    bool usesNode = path.Any(c => c.throughNode == node);
                    if (!usesNode) continue;

                    var pathInfos = ConvertToPathInfoList(path);
                    var sequence = new AlternativePathSequence(pathInfos)
                    {
                        totalDistance = path.Sum(c => c.distance)
                    };

                    // Add routing changes for this node
                    foreach (var conn in path.Where(c => c.throughNode == node))
                    {
                        var currentRoute = node.GetRouteFromInput(conn.nodeInputIndex);
                        sequence.requiredNodeChanges.Add(new AlternativePathSequence.NodeRoutingChange(
                            node,
                            conn.nodeInputIndex,
                            conn.nodeOutputIndex,
                            currentRoute?.outputIndex ?? -1
                        ));
                    }

                    results.Add(sequence);
                }
            }

            return results;
        }

        private List<AlternativePathSequence> RemoveDuplicatePaths(List<AlternativePathSequence> paths)
        {
            var unique = new List<AlternativePathSequence>();
            var seen = new HashSet<string>();

            foreach (var path in paths)
            {
                // Create a unique key for this path configuration
                var key = string.Join("|",
                    path.pathSequence.Select(p => p.path.pathId)) +
                    "||" +
                    string.Join("|", path.requiredNodeChanges.Select(c =>
                        $"{c.node.nodeId}:{c.fromInput}->{c.toOutput}"));

                if (!seen.Contains(key))
                {
                    seen.Add(key);
                    unique.Add(path);
                }
            }

            return unique;
        }


        private List<PowerNode> FindRelevantNodes(PowerConnection source, PowerConnection target, CableSize requiredSize)
        {
            var relevantNodes = new HashSet<PowerNode>();

            // Build a graph without any node restrictions to find all possible paths
            var unrestricted = BuildWaypointGraph(requiredSize, new Dictionary<string, NodeRoutingOverride>());

            // Do a BFS from source to find all nodes that could be on a path to target
            var visited = new HashSet<string>();
            var queue = new Queue<string>();
            queue.Enqueue(source.waypoint.pathId);
            visited.Add(source.waypoint.pathId);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                if (unrestricted.ContainsKey(current))
                {
                    foreach (var conn in unrestricted[current])
                    {
                        if (conn.throughNode != null)
                        {
                            relevantNodes.Add(conn.throughNode);
                        }

                        if (!visited.Contains(conn.toPath.pathId))
                        {
                            visited.Add(conn.toPath.pathId);
                            queue.Enqueue(conn.toPath.pathId);
                        }
                    }
                }
            }

            return relevantNodes.ToList();
        }

        private List<WaypointConnection> FindShortestWaypointPath(
            WaypointPath start, WaypointPath end,
            Dictionary<string, List<WaypointConnection>> graph)
        {
            var distances = new Dictionary<string, float>();
            var previous = new Dictionary<string, WaypointConnection>();
            var unvisited = new HashSet<string>();

            // Initialize
            foreach (var kvp in graph)
            {
                distances[kvp.Key] = float.MaxValue;
                unvisited.Add(kvp.Key);

                foreach (var conn in kvp.Value)
                {
                    if (!distances.ContainsKey(conn.toPath.pathId))
                    {
                        distances[conn.toPath.pathId] = float.MaxValue;
                        unvisited.Add(conn.toPath.pathId);
                    }
                }
            }

            if (distances.ContainsKey(start.pathId))
                distances[start.pathId] = 0;

            while (unvisited.Count > 0)
            {
                string current = null;
                float minDist = float.MaxValue;
                foreach (var pathId in unvisited)
                {
                    if (distances.ContainsKey(pathId) && distances[pathId] < minDist)
                    {
                        minDist = distances[pathId];
                        current = pathId;
                    }
                }

                if (current == null || current == end.pathId) break;

                unvisited.Remove(current);

                if (graph.ContainsKey(current))
                {
                    foreach (var connection in graph[current])
                    {
                        string neighbor = connection.toPath.pathId;
                        if (!unvisited.Contains(neighbor)) continue;

                        float alt = distances[current] + connection.distance;

                        if (alt < distances[neighbor])
                        {
                            distances[neighbor] = alt;
                            previous[neighbor] = connection;
                        }
                    }
                }
            }

            if (!previous.ContainsKey(end.pathId)) return null;

            var path = new List<WaypointConnection>();
            string currentPath = end.pathId;

            while (previous.ContainsKey(currentPath))
            {
                var connection = previous[currentPath];
                path.Insert(0, connection);
                currentPath = connection.fromPath.pathId;
            }

            return path;
        }

        private List<PathInfo> ConvertToPathInfoList(List<WaypointConnection> connections)
        {
            var result = new List<PathInfo>();
            var addedPaths = new HashSet<string>();

            foreach (var conn in connections)
            {
                if (!addedPaths.Contains(conn.fromPath.pathId))
                {
                    result.Add(new PathInfo(conn.fromPath, conn.fromTemplate, conn.room));
                    addedPaths.Add(conn.fromPath.pathId);
                }

                bool isLast = connections.IndexOf(conn) == connections.Count - 1;
                bool differentFromNext = !isLast &&
                    connections[connections.IndexOf(conn) + 1].fromPath.pathId != conn.toPath.pathId;

                if ((isLast || differentFromNext) && !addedPaths.Contains(conn.toPath.pathId))
                {
                    RoomNetwork destRoom = conn.room;
                    if (conn.crossRoomConnection)
                    {
                        foreach (var room in roomNetworks)
                        {
                            if (room.templates.Contains(conn.toTemplate))
                            {
                                destRoom = room;
                                break;
                            }
                        }
                    }

                    result.Add(new PathInfo(conn.toPath, conn.toTemplate, destRoom));
                    addedPaths.Add(conn.toPath.pathId);
                }
            }

            return result;
        }

        private IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> list, int length)
        {
            if (length == 1) return list.Select(t => new T[] { t });

            return GetPermutations(list, length - 1)
                .SelectMany(t => list.Where(e => !t.Contains(e)),
                    (t1, t2) => t1.Concat(new T[] { t2 }));
        }
        public void ResolveBoundaryConnections()
        {
            boundaryConnections.Clear();

            if (roomNetworks.Count < 2) return;

            // Check all network pairs
            for (int i = 0; i < roomNetworks.Count; i++)
            {
                for (int j = i + 1; j < roomNetworks.Count; j++)
                {
                    RoomNetwork networkA = roomNetworks[i];
                    RoomNetwork networkB = roomNetworks[j];

                    if (networkA == null || networkB == null) continue;

                    networkA.RefreshConnections();
                    networkB.RefreshConnections();

                    // Check all boundary port pairs
                    foreach (var portA in networkA.boundaryPorts)
                    {
                        if (portA == null || !portA.IsConnected()) continue;

                        foreach (var portB in networkB.boundaryPorts)
                        {
                            if (portB == null || !portB.IsConnected()) continue;

                            // Check if ports are close enough
                            float distance = Vector3.Distance(portA.GetWorldPosition(), portB.GetWorldPosition());

                            if (distance <= boundaryConnectionThreshold)
                            {
                                var connection = new BoundaryConnection(portA, networkA, portB, networkB);
                                boundaryConnections.Add(connection);
                            }
                        }
                    }
                }
            }
        }

        public List<BoundaryConnection> GetNetworkBoundaryConnections(RoomNetwork network)
        {
            return boundaryConnections.FindAll(c => c.networkA == network || c.networkB == network);
        }

        public List<BoundaryConnection> GetCompatibleConnections()
        {
            return boundaryConnections.FindAll(c => c.isSizeCompatible);
        }

        public bool AreNetworksConnected(RoomNetwork networkA, RoomNetwork networkB)
        {
            return boundaryConnections.Any(c =>
                (c.networkA == networkA && c.networkB == networkB && c.isSizeCompatible) ||
                (c.networkA == networkB && c.networkB == networkA && c.isSizeCompatible));
        }

        public List<RoomNetwork> GetConnectedNetworks(RoomNetwork network)
        {
            HashSet<RoomNetwork> connectedNetworks = new();

            foreach (var connection in boundaryConnections)
            {
                if (!connection.isSizeCompatible) continue;

                if (connection.networkA == network)
                    connectedNetworks.Add(connection.networkB);
                else if (connection.networkB == network)
                    connectedNetworks.Add(connection.networkA);
            }

            return connectedNetworks.ToList();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!showBoundaryConnections) return;

            foreach (var connection in boundaryConnections)
            {
                if (connection.portA == null || connection.portB == null) continue;

                Vector3 posA = connection.portA.GetWorldPosition();
                Vector3 posB = connection.portB.GetWorldPosition();

                Gizmos.color = connection.isSizeCompatible ?
                    compatibleConnectionColor : incompatibleConnectionColor;

                Gizmos.DrawLine(posA, posB);

                // Draw connection midpoint
                Vector3 midpoint = (posA + posB) * 0.5f;
                Gizmos.DrawSphere(midpoint, 0.1f);
            }
        }
#endif
        public IGenerator[] Generators
        {
            get { return generators; }
        }
        public IRouter[] Routers
        {
            get { return routers; }
        }
        public IRoutingSubstation[] Substations
        {
            get { return substations; }
        }
        public IMachine[] Machines
        {
            get { return machines; }
        }
        public IBreakerBox[] Breakers
        {
            get { return breakers; }
        }
        public Component[] GetAllPowerComponents()
        {
            List<Component> gol = new();
            foreach (var g in generators)
            {
                gol.Add(g);
            }
            foreach (var g in routers)
            {
                gol.Add(g);
            }
            foreach (var g in substations)
            {
                gol.Add(g);
            }
            foreach (var g in machines)
            {
                gol.Add(g);
            }
            foreach (var g in breakers)
            {
                gol.Add(g);
            }
            return gol.ToArray();
        }
    }
}