using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using ProjectUniverse.PowerSystem.Collision;
using ProjectUniverse.Environment.Hazards;
using System.Linq;
using static UnityEngine.Rendering.LineRendering;

namespace ProjectUniverse.PowerSystem.CollisionDemo
{
    // Manages environmental threats with different update patterns
    public class EnvironmentalThreatManager : MonoBehaviour
    {
        [System.Serializable]
        public class EnvironmentalThreatInfo
        {
            public int id;
            public DemoFire fireSource;
            public Vector3 position;
            public float lastDamageTime;
            public float lastHeatTime;
            public float damageInterval = 1f; // Apply damage every second
            public float heatInterval = 0.5f; // Apply heat twice per second
            public bool isActive;

            // Persistent collider tracking
            public HashSet<(int cableId, int segmentIndex)> activeColliders = new HashSet<(int, int)>();
            public HashSet<(int cableId, int segmentIndex)> heatAffectedSegments = new HashSet<(int, int)>();
        }

        [Header("References")]
        public CableSpatialIndexDemo spatialIndex;
        public ColliderPool colliderPool;

        [Header("Machine Damage")]
        [SerializeField] private bool damageMachinesInThreatZone = true;
        [SerializeField] private float machineDamageMultiplier = 1.0f;
        private MachineSpatialGrid machineSpatialGrid;
        private Dictionary<int, HashSet<IEnvironmentalDamageReceiver>> threatMachines = new Dictionary<int, HashSet<IEnvironmentalDamageReceiver>>();

        [Header("Performance Settings")]
        public float colliderUpdateInterval = 2f; // Update colliders every 2 seconds
        public int maxEnvironmentalThreats = 10; // Limit for performance
        public float heatDamageMultiplier = 0.1f; // Heat causes less damage than direct fire

        [Header("Debug")]
        public bool showActiveZones = true;
        public bool debugMode = false;

        public Dictionary<int, EnvironmentalThreatInfo> environmentalThreats = new Dictionary<int, EnvironmentalThreatInfo>();
        private int nextEnvThreatId = 1000; // Start higher to distinguish from projectiles
        private Coroutine environmentalUpdateCoroutine;

        void Start()
        {
            // Initialize machine spatial grid
            machineSpatialGrid = GetComponent<MachineSpatialGrid>();
            if (machineSpatialGrid == null)
            {
                machineSpatialGrid = gameObject.AddComponent<MachineSpatialGrid>();
            }

            // Start the environmental update coroutine
            environmentalUpdateCoroutine = StartCoroutine(EnvironmentalUpdateLoop());
        }

        /// <summary>
        /// Register a machine to receive environmental damage
        /// </summary>
        public void RegisterMachine(IEnvironmentalDamageReceiver machine)
        {
            if (machineSpatialGrid != null)
            {
                machineSpatialGrid.RegisterMachine(machine);
                if (debugMode) Debug.Log($"Registered machine {(machine as MonoBehaviour)?.gameObject.name} for environmental damage");
            }
            else
            {
                machineSpatialGrid = GetComponent<MachineSpatialGrid>();
                if (machineSpatialGrid == null)
                {
                    machineSpatialGrid = gameObject.AddComponent<MachineSpatialGrid>();
                }
            }
        }

        /// <summary>
        /// Unregister a machine from receiving environmental damage
        /// </summary>
        public void UnregisterMachine(IEnvironmentalDamageReceiver machine)
        {
            if (machineSpatialGrid != null)
            {
                machineSpatialGrid.UnregisterMachine(machine);
                if (debugMode) Debug.Log($"Unregistered machine {(machine as MonoBehaviour)?.gameObject.name} from environmental damage");
            }
        }

        public void UpdateMachinePosition(IEnvironmentalDamageReceiver machine, Vector3 oldPosition, Vector3 newPosition)
        {
            if (machineSpatialGrid != null)
            {
                machineSpatialGrid.UpdateMachinePosition(machine, oldPosition, newPosition);
            }
        }

        public int RegisterEnvironmentalThreat(DemoFire fire)
        {
            if (environmentalThreats.Count >= maxEnvironmentalThreats)
            {
                Debug.LogWarning("Maximum environmental threats reached!");
                return -1;
            }

            int id = nextEnvThreatId++;

            var threat = new EnvironmentalThreatInfo
            {
                id = id,
                fireSource = fire,
                position = fire.transform.position,
                lastDamageTime = Time.time,
                lastHeatTime = Time.time,
                isActive = true
            };

            environmentalThreats[id] = threat;
            threatMachines[id] = new HashSet<IEnvironmentalDamageReceiver>();

            // Immediately activate initial colliders
            UpdateEnvironmentalThreatColliders(threat);

            Debug.Log($"Registered environmental threat (fire) {id} at {threat.position}");
            return id;
        }

        public void UnregisterThreat(int threatId)
        {
            if (environmentalThreats.TryGetValue(threatId, out var threat))
            {
                // Deactivate all colliders
                foreach (var key in threat.activeColliders)
                {
                    colliderPool.DeactivateColliderForSegment(key.Item1, key.Item2, threatId);
                }

                // Clear machine references
                if (threatMachines.TryGetValue(threatId, out var machines))
                {
                    machines.Clear();
                    threatMachines.Remove(threatId);
                }

                environmentalThreats.Remove(threatId);
                Debug.Log($"Unregistered environmental threat {threatId}");
            }
        }

        // Coroutine-based update for performance
        IEnumerator EnvironmentalUpdateLoop()
        {
            while (true)
            {
                var threatsToRemove = new List<int>();

                // Create a copy of the values to avoid collection modification issues
                var threatsCopy = new List<EnvironmentalThreatInfo>(environmentalThreats.Values);

                foreach (var threat in threatsCopy)
                {
                    // Check if threat still exists in dictionary (may have been removed during yield)
                    if (!environmentalThreats.ContainsKey(threat.id))
                        continue;

                    if (!threat.isActive || threat.fireSource == null)
                    {
                        threatsToRemove.Add(threat.id);
                        continue;
                    }

                    // Update threat position //B
                    threat.position = threat.fireSource.transform.position;

                    // Update colliders periodically
                    UpdateEnvironmentalThreatColliders(threat);

                    // Apply damage
                    if (Time.time - threat.lastDamageTime >= threat.damageInterval)
                    {
                        ApplyFireDamage(threat);
                        threat.lastDamageTime = Time.time;
                    }

                    // Apply heat effects
                    if (Time.time - threat.lastHeatTime >= threat.heatInterval)
                    {
                        ApplyHeatEffects(threat);
                        threat.lastHeatTime = Time.time;
                    }

                    // Small delay between processing each threat
                    yield return new WaitForSeconds(0.1f);
                }

                // Clean up inactive threats
                foreach (var id in threatsToRemove)
                {
                    UnregisterThreat(id);
                }

                // Wait before next full cycle
                yield return new WaitForSeconds(colliderUpdateInterval);
            }
        }

        void UpdateEnvironmentalThreatColliders(EnvironmentalThreatInfo threat)
        {
            if (threat.fireSource == null) return;

            // Get current fire properties
            float damageRadius = threat.fireSource.GetCurrentDamageRadius();
            float heatRadius = threat.fireSource.GetCurrentHeatRadius();

            // Query for cables in damage radius (need colliders)
            var damageSegments = spatialIndex.QueryRadiusAllRooms(threat.position, damageRadius);
            var newActiveColliders = new HashSet<(int, int)>();

            foreach (var segment in damageSegments)
            {
                var key = (segment.cableId, segment.segmentIndex);
                newActiveColliders.Add(key);

                if (!threat.activeColliders.Contains(key))
                {
                    // New segment needs collider
                    colliderPool.ActivateColliderForSegment(segment, threat.id);
                    threat.activeColliders.Add(key);
                }
            }

            // Deactivate colliders no longer needed
            var toDeactivate = new List<(int, int)>();
            foreach (var key in threat.activeColliders)
            {
                if (!newActiveColliders.Contains(key))
                {
                    toDeactivate.Add(key);
                }
            }

            foreach (var key in toDeactivate)
            {
                colliderPool.DeactivateColliderForSegment(key.Item1, key.Item2, threat.id);
                threat.activeColliders.Remove(key);
            }

            // Update heat-affected segments (no colliders needed, just tracking)
            var heatSegments = spatialIndex.QueryRadiusAllRooms(threat.position, heatRadius);
            threat.heatAffectedSegments.Clear();
            foreach (var segment in heatSegments)
            {
                threat.heatAffectedSegments.Add((segment.cableId, segment.segmentIndex));
            }

            // Query for machines in damage radius
            if (damageMachinesInThreatZone && machineSpatialGrid != null)
            {
                var nearbyMachines = machineSpatialGrid.QueryRadius(threat.position, damageRadius);
                threatMachines[threat.id] = new HashSet<IEnvironmentalDamageReceiver>(nearbyMachines);
            }
        }

        /*void UpdateEnvironmentalThreatMachines(EnvironmentalThreatInfo threat, float damageRadius)
        {
            if (!threatMachines.TryGetValue(threat.id, out var currentMachines))
                return;

            var newMachines = new HashSet<IEnvironmentalDamageReceiver>();
            foreach (var machine in registeredMachines)
            {
                if (machine is MonoBehaviour mb && mb != null)
                {
                    float distance = Vector3.Distance(threat.position, mb.transform.position);
                    if (distance <= damageRadius)
                    {
                        Debug.Log("in range");
                        newMachines.Add(machine);
                    }
                }
            }

            // Update the threat's machine set
            threatMachines[threat.id] = newMachines;
        }*/

        void ApplyFireDamage(EnvironmentalThreatInfo threat)
        {
            if (threat.fireSource == null) return;

            float damage = threat.fireSource.GetCurrentDamagePerSecond() * threat.damageInterval;
            float damageRadius = threat.fireSource.GetCurrentDamageRadius();

            var affectedSegments = spatialIndex.QueryRadiusAllRooms(threat.position, damageRadius);

            foreach (var segment in affectedSegments)
            {
                // Calculate distance-based damage falloff
                Vector3 segmentCenter = (segment.start + segment.end) * 0.5f;
                float distance = Vector3.Distance(threat.position, segmentCenter);
                float falloff = 1f - (distance / damageRadius);
                float actualDamage = damage * falloff;

                Debug.Log($"Fire {threat.id} would damage cable {segment.cableId} segment {segment.segmentIndex} for {actualDamage} fire damage");
                // In real implementation: cableSystem.DamageSegment(segment.cableId, segment.segmentIndex, actualDamage);
            }

            // Apply damage to machines
            if (damageMachinesInThreatZone && threatMachines.TryGetValue(threat.id, out var machines))
            {
                foreach (var machine in machines)
                {
                    if (machine is MonoBehaviour mb && mb != null)
                    {
                        float distance = Vector3.Distance(threat.position, mb.transform.position);
                        float falloff = Mathf.Max(0f, 1f - (distance / damageRadius));
                        float actualMachineDamage = damage * falloff * machineDamageMultiplier;

                        if (actualMachineDamage > 0f)
                        {
                            machine.ReceiveEnvironmentalDamage(actualMachineDamage, DamageType.Fire);
                            Debug.Log($"Fire {threat.id} damaging machine {mb.gameObject.name} for {actualMachineDamage} fire damage");
                        }
                    }
                }
            }
        }

        void ApplyHeatEffects(EnvironmentalThreatInfo threat)
        {
            if (threat.fireSource == null) return;

            float heatDamage = threat.fireSource.GetCurrentDamagePerSecond() * heatDamageMultiplier * threat.heatInterval;
            float heatRadius = threat.fireSource.GetCurrentHeatRadius();

            foreach (var segmentKey in threat.heatAffectedSegments)
            {
                // Find the segment to calculate distance
                var segments = spatialIndex.QueryRadiusAllRooms(threat.position, heatRadius);
                foreach (var segment in segments)
                {
                    if (segment.cableId == segmentKey.Item1 && segment.segmentIndex == segmentKey.Item2)
                    {
                        Vector3 segmentCenter = (segment.start + segment.end) * 0.5f;
                        float distance = Vector3.Distance(threat.position, segmentCenter);
                        float falloff = 1f - (distance / heatRadius);
                        float actualHeatDamage = heatDamage * falloff;

                        Debug.Log($"Fire {threat.id} applying heat to cable {segment.cableId} segment {segment.segmentIndex}: {actualHeatDamage} heat damage");
                        // In real implementation: cableSystem.ApplyHeat(segment.cableId, segment.segmentIndex, actualHeatDamage);
                        break;
                    }
                }
            }

            // Apply heat to machines
            if (damageMachinesInThreatZone && threatMachines.TryGetValue(threat.id, out var machines))
            {
                foreach (var machine in machines)
                {
                    if (machine is MonoBehaviour mb && mb != null)
                    {
                        float distance = Vector3.Distance(threat.position, mb.transform.position);
                        float falloff = Mathf.Max(0f, 1f - (distance / heatRadius));
                        float actualHeatDamage = heatDamage * falloff;

                        if (actualHeatDamage > 0f)
                        {
                            machine.ReceiveEnvironmentalDamage(actualHeatDamage, DamageType.Heat);
                            Debug.Log($"Fire {threat.id} applying heat to machine {mb.gameObject.name}: {actualHeatDamage} heat damage");
                        }
                    }
                }
            }
        }

        void OnDestroy()
        {
            if (environmentalUpdateCoroutine != null)
            {
                StopCoroutine(environmentalUpdateCoroutine);
            }
        }

        void OnDrawGizmos()
        {
            if (!showActiveZones) return;

            foreach (var threat in environmentalThreats.Values)
            {
                if (threat.fireSource == null) continue;

                // Draw active collider zones
                Gizmos.color = Color.red;
                foreach (var key in threat.activeColliders)
                {
                    // Visual indicator for active colliders
                    Gizmos.DrawWireCube(threat.position + Vector3.up * key.Item1 * 0.1f, Vector3.one * 0.2f);
                }

                // Draw heat zones
                Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f);
                foreach (var key in threat.heatAffectedSegments)
                {
                    // Visual indicator for heat-affected segments
                    Gizmos.DrawWireCube(threat.position + Vector3.up * key.Item1 * 0.05f, Vector3.one * 0.1f);
                }
            }
        }

        // Public method to manually trigger fire at position (for testing)
        [ContextMenu("Spawn Test Fire")]
        public void SpawnTestFire()
        {
            GameObject fireObj = new GameObject("TestFire");
            fireObj.transform.position = transform.position;
            fireObj.AddComponent<DemoFire>();
        }

        public int GetActiveEnvironmentalThreatCount() => environmentalThreats.Count;

        public List<EnvironmentalThreatInfo> GetActiveThreats()
        {
            return new List<EnvironmentalThreatInfo>(environmentalThreats.Values);
        }
    }
}