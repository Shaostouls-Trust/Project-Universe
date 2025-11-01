using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace ProjectUniverse.PowerSystem
{
    public class PowerSystemPathManager : MonoBehaviour
    {
        private static bool isDirty = true;
        private static PowerSystemPathManager _instance;
        public static PowerSystemPathManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<PowerSystemPathManager>();
                    if (_instance == null)
                    {
                        GameObject go = new("PowerSystemPathManager");
                        _instance = go.AddComponent<PowerSystemPathManager>();
                    }
                }
                return _instance;
            }
        }

        [SerializeField] private List<SegmentDataEntry> savedSegmentData = new();
        private Dictionary<string, CableSegmentData> segmentDataLookup = new();


        [System.Serializable]
        public class CableHealthInfo
        {
            public string pathId;
            public float overallHealth;
            public float maxHeat;
            public bool isOperational;
            public int totalSegments;
        }
        [SerializeField] private GlobalRouteResolver resolver;
        private Dictionary<string, PathCable> pathCables = new();
        private Dictionary<Component, List<PathCable>> componentConnections = new();
        private Dictionary<string, CableSegmentData[]> persistentSegmentData = new();

        [System.Serializable]
        public class SegmentDataEntry
        {
            public string segmentKey; // Format: templateId_pathId_segmentIndex
            public CableSegmentData data;

            public SegmentDataEntry(string key, CableSegmentData data)
            {
                this.segmentKey = key;
                this.data = data;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            // Build lookup dictionary from saved data
            RebuildSegmentDataLookup();
        }

        private void RebuildSegmentDataLookup()
        {
            segmentDataLookup.Clear();
            foreach (var entry in savedSegmentData)
            {
                segmentDataLookup[entry.segmentKey] = entry.data;
            }
        }

        public void SaveSegmentData(string templateId, string pathId, int segmentIndex, CableSegmentData data)
        {
            string key = $"{templateId}_{pathId}_{segmentIndex}";

            // Update or add to saved data
            var existingIndex = savedSegmentData.FindIndex(e => e.segmentKey == key);
            if (existingIndex >= 0)
            {
                savedSegmentData[existingIndex].data = data;
            }
            else
            {
                savedSegmentData.Add(new SegmentDataEntry(key, data));
            }

            segmentDataLookup[key] = data;
        }
        
        public CableSegmentData GetSavedSegmentData(string templateId, string pathId, int segmentIndex, float defaultMaxHealth, float defaultMaxHeat)
        {
            string key = $"{templateId}_{pathId}_{segmentIndex}";

            if (segmentDataLookup.TryGetValue(key, out var saved))
            {
                return saved;
            }

            // Return new segment data if not found
            return new CableSegmentData(defaultMaxHealth, defaultMaxHeat);
        }

        public void SaveAllCableSegments()
        {
            // Save all current cable segments
            foreach (var kvp in pathCables)
            {
                var cable = kvp.Value;
                if (cable != null)
                {
                    cable.SaveAllSegments();
                }
            }
        }
        // Call this before destroying cables or on application pause/quit
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus) SaveAllCableSegments();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus) SaveAllCableSegments();
        }

        private void OnDestroy()
        {
            SaveAllCableSegments();
        }

        /// <summary>
        /// Registers a connection between a component and a waypoint path (without creating a cable)
        /// </summary>
        public void RegisterComponentPathConnection(Component component, WaypointPath path, Template template)
        {
            if (component == null || path == null || template == null) return;

            // Create a tracking entry even without a PathCable
            if (!componentConnections.ContainsKey(component))
            {
                componentConnections[component] = new List<PathCable>();
            }

            //Debug.Log($"Registered connection: {component.name} -> Path {path.pathId} in Template {template.name}");
        }
        
        /// <summary>
        /// Clears all connections
        /// </summary>
        public void ClearAllConnections()
        {
            isDirty = true;
            // Remove all cables from components
            foreach (var kvp in componentConnections)
            {
                if (kvp.Key is IGenerator gen)
                {
                    var genField = gen.GetType().GetField("iCableDLL",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (genField?.GetValue(gen) is LinkedList<ICable> genCables)
                    {
                        genCables.Clear();
                    }
                }
                else if (kvp.Key is IRouter router)
                {
                    var routerField = router.GetType().GetField("iCableDLL",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (routerField?.GetValue(router) is LinkedList<ICable> routerCables)
                    {
                        routerCables.Clear();
                    }
                }
                else if (kvp.Key is IRoutingSubstation sub)
                {
                    var subField = sub.GetType().GetField("iCableDLL",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (subField?.GetValue(sub) is LinkedList<ICable> subCables)
                    {
                        subCables.Clear();
                    }
                }
                else if (kvp.Key is IMachine mach)
                {
                    var machField = mach.GetType().GetField("iCableDLL",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (machField?.GetValue(mach) is LinkedList<ICable> machCables)
                    {
                        machCables.Clear();
                    }
                }
            }

            pathCables.Clear();
            componentConnections.Clear();
        }

        /// <summary>
        /// Creates a path-based cable connection using multiple waypoint paths
        /// </summary>
        public PathCable CreateMultiPathConnection(Component source, Component target, List<GlobalRouteResolver.PathInfo> pathSequence)
        {
            if (source == null || target == null || pathSequence == null || pathSequence.Count == 0)
                return null;

            isDirty = true;
            // Check if this exact path sequence already exists
            string sequenceKey = GenerateSequenceKey(pathSequence);
            if (pathCables.ContainsKey(sequenceKey))
            {
                Debug.LogWarning($"Path sequence {sequenceKey} already has a cable connection");
                return pathCables[sequenceKey];
            }

            PathCable cable;

            // Create appropriate cable type based on components
            if (source is IGenerator gen && target is IRouter router)
            {
                cable = new PathCable(gen, router, pathSequence);
            }
            else if (source is IRouter router1 && target is IRoutingSubstation substation)
            {
                cable = new PathCable(router1, substation, pathSequence);
            }
            else if (source is IRoutingSubstation substation1 && target is IMachine machine)
            {
                cable = new PathCable(substation1, machine, pathSequence);
            }
            else if (source is IRoutingSubstation substation2 && target is IBreakerBox breaker)
            {
                cable = new PathCable(substation2, breaker, pathSequence);
            }
            else if (source is IBreakerBox breaker1 && target is ISubMachine subMachine)
            {
                cable = new PathCable(breaker1, subMachine, pathSequence);
            }
            else
            {
                Debug.LogError($"Unsupported connection type: {source.GetType()} to {target.GetType()}");
                return null;
            }

            if (cable != null)
            {
                // Register the cable
                pathCables[sequenceKey] = cable;

                // Track connections for each component
                if (!componentConnections.ContainsKey(source))
                    componentConnections[source] = new List<PathCable>();
                componentConnections[source].Add(cable);

                if (!componentConnections.ContainsKey(target))
                    componentConnections[target] = new List<PathCable>();
                componentConnections[target].Add(cable);

                Debug.Log($"Created multi-path connection from {source.name} to {target.name} using {pathSequence.Count} paths");
            }

            return cable;
        }

        private string GenerateSequenceKey(List<GlobalRouteResolver.PathInfo> pathSequence)
        {
            return string.Join("->", pathSequence.Select(p => p.path.pathId));
        }

        public void SaveSegmentData(PathCable cable)
        {
            if (cable == null) return;

            var paths = cable.GetPaths();
            if (paths != null && paths.Count > 0)
            {
                string key = string.Join("->", paths.Select(p => p.pathId));
                persistentSegmentData[key] = cable.GetAllSegmentData(); // Need to add this method to PathCable
            }
        }

        public CableSegmentData[] GetSavedSegmentData(List<WaypointPath> paths)
        {
            if (paths == null || paths.Count == 0) return null;

            string key = string.Join("->", paths.Select(p => p.pathId));
            if (persistentSegmentData.TryGetValue(key, out var data))
            {
                return data;
            }
            return null;
        }

        /// <summary>
        /// Disconnects a multi-path cable
        /// </summary>
        public void DisconnectMultiPath(string sequenceKey)
        {
            if (pathCables.TryGetValue(sequenceKey, out var cable))
            {
                // Remove from component connections
                RemoveCableFromComponent(cable.gen);
                RemoveCableFromComponent(cable.route);
                RemoveCableFromComponent(cable.subst);
                RemoveCableFromComponent(cable.breaker);
                RemoveCableFromComponent(cable.mach);
                RemoveCableFromComponent(cable.subMach);

                pathCables.Remove(sequenceKey);
                isDirty = true;
                Debug.Log($"Disconnected multi-path {sequenceKey}");
            }
        }

        /// <summary>
        /// Creates a path-based cable connection between two power system components
        /// </summary>
        public PathCable CreatePathConnection(Component source, Component target, WaypointPath path, Template template)
        {
            if (source == null || target == null || path == null || template == null)
                return null;

            isDirty = true;
            // Check if path already has a cable
            if (pathCables.ContainsKey(path.pathId))
            {
                Debug.LogWarning($"Path {path.pathId} already has a cable connection");
                return null;
            }

            PathCable cable;

            // Create appropriate cable type based on components
            if (source is IGenerator gen && target is IRouter router)
            {
                cable = new PathCable(gen, router, path, template);
            }
            else if (source is IRouter router1 && target is IRoutingSubstation substation)
            {
                cable = new PathCable(router1, substation, path, template);
            }
            else if (source is IRoutingSubstation substation1 && target is IMachine machine)
            {
                cable = new PathCable(substation1, machine, path, template);
            }
            else if (source is IRoutingSubstation substation2 && target is IBreakerBox breaker)
            {
                cable = new PathCable(substation2, breaker, path, template);
            }
            else if (source is IBreakerBox breaker1 && target is ISubMachine subMachine)
            {
                cable = new PathCable(breaker1, subMachine, path, template);
            }
            else
            {
                Debug.LogError($"Unsupported connection type: {source.GetType()} to {target.GetType()}");
                return null;
            }

            if (cable != null)
            {
                // Register the cable
                pathCables[path.pathId] = cable;

                // Track connections for each component
                if (!componentConnections.ContainsKey(source))
                    componentConnections[source] = new List<PathCable>();
                componentConnections[source].Add(cable);

                if (!componentConnections.ContainsKey(target))
                    componentConnections[target] = new List<PathCable>();
                componentConnections[target].Add(cable);

                Debug.Log($"Created path connection from {source.name} to {target.name} via path {path.pathId}");
            }

            return cable;
        }

        public List<PathCable> GetComponentConnections(Component component)
        {
            // First check existing PathCable connections
            if (componentConnections.TryGetValue(component, out var cables))
            {
                return new List<PathCable>(cables);
            }
            return null;
        }

        /// <summary>
        /// Finds a machine connected through paths from a source component
        /// </summary>
        public T FindConnectedMachine<T>(Component source) where T : Component
        {
            var visited = new HashSet<Component>();
            var queue = new Queue<Component>();
            queue.Enqueue(source);
            visited.Add(source);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                // Check if this is the target type
                if (current is T target && current != source)
                    return target;

                // Get all connections from this component
                var connections = GetComponentConnections(current);
                foreach (var cable in connections)
                {
                    Component next = null;

                    // Determine the other end of the connection
                    if (cable.gen == current) next = cable.route;
                    else if (cable.route == current) next = cable.subst ?? (Component)cable.gen;
                    else if (cable.subst == current) next = cable.mach ?? cable.breaker ?? (Component)cable.route;
                    else if (cable.breaker == current) next = cable.subMach ?? (Component)cable.subst;
                    else if (cable.mach == current) next = cable.subst;
                    else if (cable.subMach == current) next = cable.breaker;

                    if (next != null && !visited.Contains(next))
                    {
                        visited.Add(next);
                        queue.Enqueue(next);
                    }
                }
            }

            return null;
        }

        private void RemoveCableFromComponent(Component component)
        {
            if (component != null && componentConnections.TryGetValue(component, out var cables))
            {
                cables.RemoveAll(c => !pathCables.ContainsValue(c));
                if (cables.Count == 0)
                    componentConnections.Remove(component);
                isDirty = true;
            }
        }
       
        /// <summary>
        /// Checks the health status of all cable connections
        /// </summary>
        public void CheckAllCableHealth()
        {
            var brokenCables = new List<string>();

            foreach (var kvp in pathCables)
            {
                if (!kvp.Value.IsOperational())
                {
                    brokenCables.Add(kvp.Key);
                }
            }

            // Log broken cables
            if (brokenCables.Count > 0)
            {
                Debug.LogWarning($"Found {brokenCables.Count} broken cable connections");
                foreach (var cableId in brokenCables)
                {
                    Debug.LogWarning($"Broken cable: {cableId}");
                }
            }
        }

        /// <summary>
        /// Gets cable health information for a specific path
        /// </summary>
        public CableHealthInfo GetCableHealthInfo_(string pathId)
        {
            if (pathCables.TryGetValue(pathId, out var cable))
            {
                return new CableHealthInfo
                {
                    pathId = pathId,
                    overallHealth = cable.GetOverallHealth(),
                    maxHeat = cable.GetMaxHeat(),
                    isOperational = cable.IsOperational(),
                    totalSegments = cable.GetTotalSegments()
                };
            }
            return null;
        }

        /// <summary>
        /// Gets cable health information for a specific path, creating a default if needed
        /// </summary>
        public CableHealthInfo GetCableHealthInfo(string pathId)
        {
            // First check if we have a PathCable registered
            if (pathCables.TryGetValue(pathId, out var cable))
            {
                return new CableHealthInfo
                {
                    pathId = pathId,
                    overallHealth = cable.GetOverallHealth(),
                    maxHeat = cable.GetMaxHeat(),
                    isOperational = cable.IsOperational(),
                    totalSegments = cable.GetTotalSegments()
                };
            }
            if (isDirty)
            {
                DebugListAllPathCables();
            }
            // If no PathCable exists, check if this is a valid path that just hasn't been connected yet
            // This prevents all unconnected paths from showing as gray
            return null;
        }

        /// <summary>
        /// Applies damage to cables within a radius (for explosions, etc)
        /// </summary>
        public void ApplyAreaDamage(Vector3 worldPosition, float damage, float radius)
        {
            foreach (var cable in pathCables.Values)
            {
                cable.DamageSegmentAtPosition(worldPosition, damage, radius);
            }
        }

        /// <summary>
        /// Gets all damaged cables that need repair
        /// </summary>
        public List<PathCable> GetDamagedCables(float healthThreshold = 0.5f)
        {
            return pathCables.Values
                .Where(cable => cable.GetOverallHealth() < healthThreshold)
                .ToList();
        }

        /// <summary>
        /// Gets the path cable for a specific path ID
        /// </summary>
        public PathCable GetPathCable(string pathId)
        {
            pathCables.TryGetValue(pathId, out var cable);
            return cable;
        }

        /// <summary>
        /// Checks if a path is available for connection
        /// </summary>
        public bool IsPathAvailable(string pathId)
        {
            return !pathCables.ContainsKey(pathId);
        }

        /// <summary>
        /// Debug method to list all registered path cables
        /// </summary>
        public void DebugListAllPathCables()
        {
            isDirty = false;
            Debug.Log($"Total registered PathCables: {pathCables.Count}");
            foreach (var kvp in pathCables)
            {
                Debug.Log($"  - PathId: {kvp.Key}, Cable: {kvp.Value}");
            }
        }

        /// <summary>
        /// Gets all registered path cables
        /// </summary>
        public Dictionary<string, PathCable> GetAllPathCables()
        {
            return new Dictionary<string, PathCable>(pathCables);
        }

        /// <summary>
        /// Gets cable health information for a specific path within any registered cable
        /// </summary>
        public CableHealthInfo GetCableHealthInfoByPath(string pathId)
        {
            // First check direct lookup
            if (pathCables.TryGetValue(pathId, out var cable))
            {
                return new CableHealthInfo
                {
                    pathId = pathId,
                    overallHealth = cable.GetOverallHealth(),
                    maxHeat = cable.GetMaxHeat(),
                    isOperational = cable.IsOperational(),
                    totalSegments = cable.GetTotalSegments()
                };
            }

            // Then check if this pathId is part of any multi-path cable
            foreach (var kvp in pathCables)
            {
                var pathCable = kvp.Value;
                if (pathCable != null)
                {
                    var paths = pathCable.GetPaths();
                    if (paths != null)
                    {
                        foreach (var path in paths)
                        {
                            if (path.pathId == pathId)
                            {
                                return new CableHealthInfo
                                {
                                    pathId = pathId,
                                    overallHealth = pathCable.GetOverallHealth(),
                                    maxHeat = pathCable.GetMaxHeat(),
                                    isOperational = pathCable.IsOperational(),
                                    totalSegments = pathCable.GetTotalSegments()
                                };
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}