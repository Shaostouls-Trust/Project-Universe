using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using ProjectUniverse.PowerSystem.CollisionDemo;

namespace ProjectUniverse.PowerSystem.Collision
{
    /// <summary>
    /// Comprehensive threat management system with robustness and predictive capabilities
    /// </summary>
    public class ThreatManager : MonoBehaviour
    {
        [System.Serializable]
        public class ThreatInfo
        {
            public int id;
            public GameObject source;
            public Vector3 position;
            public Vector3 velocity;
            public float radius;
            public bool isActive;
            public ThreatType type;
            public float creationTime;
            public HashSet<(int cableId, int segmentIndex)> activatedSegments = new HashSet<(int, int)>();

            // Predictive data
            public List<Vector3> predictedPath = new List<Vector3>();
            public float lastPredictionTime;
        }

        [System.Serializable]
        public class SystemHealth
        {
            public int activeProjectiles;
            public int activeEnvironmentalThreats;
            public int activeColliders;
            public int orphanedColliders;
            public float memoryUsageMB;
            public float averageQueryTime;
        }

        public enum ThreatType
        {
            Projectile,
            Explosion,
            Environmental
        }

        [Header("References")]
        public CableSpatialIndexDemo spatialIndex;
        public ColliderPool colliderPool;

        [Header("Prediction Settings")]
        public float projectileLookaheadTime = 0.1f;
        public float colliderActivationRadius = 1f;
        public float predictionUpdateInterval = 0.05f;

        [Header("Robustness Settings")]
        public float colliderTimeoutSeconds = 10f;
        public int maxTotalActiveColliders = 500;
        public float maxQueryTimeWarning = 0.001f;
        public bool enableAutomaticCleanup = true;
        public bool validateStateConsistency = true;

        [Header("Performance Monitoring")]
        public SystemHealth currentHealth = new SystemHealth();
        public bool logPerformanceWarnings = true;

        [Header("Fallback Systems")]
        public bool enableBroadPhaseBackup = true;
        public float broadPhaseRadius = 50f;

        [Header("Debug")]
        public bool showThreatZones = true;
        public bool showPredictedPaths = true;

        public Dictionary<int, ThreatInfo> activeThreats = new Dictionary<int, ThreatInfo>();
        private int nextThreatId = 1;
        private Queue<int> threatsToRemove = new Queue<int>();
        private Queue<int> recentlyDestroyedThreats = new Queue<int>();
        private List<System.Exception> recentErrors = new List<System.Exception>();
        private Coroutine cleanupCoroutine;
        private Coroutine healthMonitorCoroutine;

        void Start()
        {
            if (enableAutomaticCleanup)
            {
                cleanupCoroutine = StartCoroutine(AutomaticCleanupLoop());
            }

            healthMonitorCoroutine = StartCoroutine(HealthMonitorLoop());
            Application.logMessageReceived += OnLogMessage;
        }

        void OnDestroy()
        {
            Application.logMessageReceived -= OnLogMessage;
            if (cleanupCoroutine != null) StopCoroutine(cleanupCoroutine);
            if (healthMonitorCoroutine != null) StopCoroutine(healthMonitorCoroutine);
        }

        public int RegisterTemporaryThreat(BulletProjectile projectile)
        {
            int id = nextThreatId++;

            var threat = new ThreatInfo
            {
                id = id,
                source = projectile.gameObject,
                position = projectile.transform.position,
                velocity = projectile.GetVelocity(),
                radius = projectile.damageRadius,
                isActive = true,
                type = ThreatType.Projectile,
                creationTime = Time.time
            };

            activeThreats[id] = threat;
            PredictAndActivateColliders(threat);

            Debug.Log($"Registered projectile threat {id} at {threat.position}");
            return id;
        }

        public void UpdateThreatPosition(int threatId, Vector3 newPosition)
        {
            if (activeThreats.TryGetValue(threatId, out var threat))
            {
                threat.position = newPosition;

                // Re-predict collision path for projectiles
                if (threat.type == ThreatType.Projectile &&
                    Time.time - threat.lastPredictionTime > predictionUpdateInterval)
                {
                    PredictAndActivateColliders(threat);
                    threat.lastPredictionTime = Time.time;
                }
            }
        }

        public void OnProjectileRicochet(int threatId, Vector3 newPosition, Vector3 newVelocity)
        {
            if (activeThreats.TryGetValue(threatId, out var threat))
            {
                threat.position = newPosition;
                threat.velocity = newVelocity;

                // Clear old predictions and recalculate
                threat.predictedPath.Clear();
                PredictAndActivateColliders(threat);
            }
        }

        void PredictAndActivateColliders(ThreatInfo threat)
        {
            if (spatialIndex == null || colliderPool == null) return;

            List<DemoCableSegment> segments;

            try
            {
                // Calculate prediction bounds
                Vector3 currentPos = threat.position;
                Vector3 futurePos = currentPos + threat.velocity * projectileLookaheadTime;

                // Update predicted path
                threat.predictedPath.Clear();
                threat.predictedPath.Add(currentPos);
                threat.predictedPath.Add(futurePos);

                // Create swept bounds along predicted path
                Bounds sweptBounds = new Bounds(currentPos, Vector3.zero);
                sweptBounds.Encapsulate(futurePos);
                sweptBounds.Expand(threat.radius * 2f + colliderActivationRadius);

                // Query spatial index with error handling
                float queryRadius = sweptBounds.size.magnitude * 0.5f;
                Vector3 queryCenter = sweptBounds.center;

                segments = SafeQueryRadius(queryCenter, queryRadius, $"threat {threat.id}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error predicting colliders for threat {threat.id}: {e.Message}");
                return;
            }

            // Track newly found segments
            var newSegments = new HashSet<(int, int)>();

            foreach (var segment in segments)
            {
                var key = (segment.cableId, segment.segmentIndex);
                newSegments.Add(key);

                if (!threat.activatedSegments.Contains(key))
                {
                    // New segment - activate collider
                    colliderPool.ActivateColliderForSegment(segment, threat.id);
                    threat.activatedSegments.Add(key);
                }
            }

            // Deactivate colliders for segments no longer in path
            var toRemove = new List<(int, int)>();
            foreach (var key in threat.activatedSegments)
            {
                if (!newSegments.Contains(key))
                {
                    colliderPool.DeactivateColliderForSegment(key.Item1, key.Item2, threat.id);
                    toRemove.Add(key);
                }
            }

            foreach (var key in toRemove)
            {
                threat.activatedSegments.Remove(key);
            }
        }

        public void ExecuteThreatDamage(int threatId, Vector3 position, float radius, float damage)
        {
            if (!activeThreats.ContainsKey(threatId)) return;

            Debug.Log($"Executing damage for threat {threatId} at {position}, radius {radius}, damage {damage}");

            var affectedSegments = SafeQueryRadius(position, radius, $"damage from threat {threatId}");

            foreach (var segment in affectedSegments)
            {
                float distance = Vector3.Distance(position, (segment.start + segment.end) * 0.5f);
                float falloff = 1f - (distance / radius);
                float actualDamage = damage * Mathf.Max(0, falloff);

                if (actualDamage > 0)
                {
                    Debug.Log($"Would damage cable {segment.cableId} segment {segment.segmentIndex} for {actualDamage} damage");
                    // In real implementation: cableSystem.DamageSegment(segment.cableId, segment.segmentIndex, actualDamage);
                }
            }
        }

        public void UnregisterThreat(int threatId)
        {
            if (activeThreats.TryGetValue(threatId, out var threat))
            {
                // Deactivate all colliders for this threat
                foreach (var key in threat.activatedSegments)
                {
                    colliderPool.DeactivateColliderForSegment(key.Item1, key.Item2, threatId);
                }

                activeThreats.Remove(threatId);
                recentlyDestroyedThreats.Enqueue(threatId);
                Debug.Log($"Unregistered threat {threatId}");
            }
        }

        // Robust query method with fallbacks
        public List<DemoCableSegment> SafeQueryRadius(Vector3 position, float radius, string context = "")
        {
            float startTime = Time.realtimeSinceStartup;
            List<DemoCableSegment> results = new List<DemoCableSegment>();

            try
            {
                if (spatialIndex != null)
                {
                    results = spatialIndex.QueryRadiusAllRooms(position, radius);
                }
                else if (enableBroadPhaseBackup)
                {
                    Debug.LogWarning("Spatial index unavailable, using broad-phase backup");
                    results = BroadPhaseQuery(position, radius);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Spatial query failed ({context}): {e.Message}");
                recentErrors.Add(e);

                if (enableBroadPhaseBackup)
                {
                    try
                    {
                        results = BroadPhaseQuery(position, radius);
                    }
                    catch (System.Exception e2)
                    {
                        Debug.LogError($"Backup query also failed: {e2.Message}");
                        results = new List<DemoCableSegment>();
                    }
                }
            }

            // Update performance metrics
            float queryTime = Time.realtimeSinceStartup - startTime;
            currentHealth.averageQueryTime = (currentHealth.averageQueryTime * 0.9f) + (queryTime * 0.1f);

            return results;
        }

        List<DemoCableSegment> BroadPhaseQuery(Vector3 position, float radius)
        {
            var results = new List<DemoCableSegment>();
            Debug.Log($"Using broad-phase query - this is expensive! Position: {position}, Radius: {radius}");
            return results;
        }

        IEnumerator AutomaticCleanupLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(5f);

                try
                {
                    CleanupDestroyedThreats();
                    ValidateSystemConsistency();
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error during automatic cleanup: {e.Message}");
                }
            }
        }

        void CleanupDestroyedThreats()
        {
            // Clean up recently destroyed threats
            while (recentlyDestroyedThreats.Count > 0)
            {
                recentlyDestroyedThreats.Dequeue(); // Just remove from queue
            }

            // Clean up inactive threats
            foreach (var kvp in activeThreats.ToList())
            {
                if (!kvp.Value.isActive || kvp.Value.source == null)
                {
                    UnregisterThreat(kvp.Key);
                }
            }
        }

        void ValidateSystemConsistency()
        {
            if (!validateStateConsistency) return;

            try
            {
                if (spatialIndex == null)
                {
                    Debug.LogError("Spatial index is null! System will fall back to broad-phase queries.");
                }

                if (colliderPool == null)
                {
                    Debug.LogError("Collider pool is null! No collision detection will work.");
                }

                var poolStats = colliderPool?.GetStats();
                if (poolStats != null && poolStats.activeColliders > maxTotalActiveColliders)
                {
                    Debug.LogWarning($"Too many active colliders: {poolStats.activeColliders}/{maxTotalActiveColliders}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error during consistency validation: {e.Message}");
            }
        }

        IEnumerator HealthMonitorLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);
                UpdateSystemHealth();

                if (logPerformanceWarnings)
                {
                    CheckPerformanceWarnings();
                }
            }
        }

        protected void Update()
        {
            // Clean up inactive threats
            foreach (var kvp in activeThreats)
            {
                if (!kvp.Value.isActive || kvp.Value.source == null)
                {
                    threatsToRemove.Enqueue(kvp.Key);
                }
            }

            while (threatsToRemove.Count > 0)
            {
                UnregisterThreat(threatsToRemove.Dequeue());
            }
        }


        void UpdateSystemHealth()
        {
            currentHealth.activeProjectiles = activeThreats.Count(kvp => kvp.Value.type == ThreatType.Projectile);
            currentHealth.activeEnvironmentalThreats = activeThreats.Count(kvp => kvp.Value.type == ThreatType.Environmental);

            var poolStats = colliderPool?.GetStats();
            if (poolStats != null)
            {
                currentHealth.activeColliders = poolStats.activeColliders;
                currentHealth.orphanedColliders = poolStats.orphanedColliders;
            }

            currentHealth.memoryUsageMB = (System.GC.GetTotalMemory(false) / 1024f / 1024f);
        }

        void CheckPerformanceWarnings()
        {
            if (currentHealth.averageQueryTime > maxQueryTimeWarning)
            {
                Debug.LogWarning($"Spatial queries taking too long: {currentHealth.averageQueryTime * 1000:F2}ms average");
            }

            if (recentErrors.Count > 5)
            {
                Debug.LogWarning($"High error rate detected: {recentErrors.Count} errors recently");
                recentErrors.RemoveRange(0, recentErrors.Count - 5); // Keep only recent errors
            }
        }

        void OnLogMessage(string logString, string stackTrace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception)
            {
                if (logString.Contains("CableCollision") || logString.Contains("ThreatManager"))
                {
                    try
                    {
                        recentErrors.Add(new System.Exception(logString));
                        if (recentErrors.Count > 10) recentErrors.RemoveAt(0);
                    }
                    catch
                    {
                        // Prevent infinite error loops
                    }
                }
            }
        }

        public List<ThreatInfo> GetActiveThreats()
        {
            return activeThreats.Values.Where(t => t.isActive).ToList();
        }

        public int GetActiveThreatCount()
        {
            return activeThreats.Count(kvp => kvp.Value.isActive);
        }

        public bool IsSystemHealthy()
        {
            return currentHealth.activeColliders < maxTotalActiveColliders &&
                   currentHealth.averageQueryTime < maxQueryTimeWarning * 2 &&
                   recentErrors.Count < 3;
        }

        [ContextMenu("Emergency System Reset")]
        public void EmergencyReset()
        {
            Debug.LogWarning("Performing emergency system reset!");

            try
            {
                // Clear all threats
                foreach (var kvp in activeThreats.ToList())
                {
                    UnregisterThreat(kvp.Key);
                }

                // Reset pools and queues
                recentErrors.Clear();
                recentlyDestroyedThreats.Clear();
                threatsToRemove.Clear();

                // Force collider pool cleanup
                colliderPool?.ForceCleanupAll();

                // Reset health metrics
                currentHealth = new SystemHealth();

                System.GC.Collect();
                Debug.Log("Emergency reset completed");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Emergency reset failed: {e.Message}");
            }
        }

        protected void OnDrawGizmos()
        {
            if (!showThreatZones) return;

            foreach (var threat in activeThreats.Values)
            {
                if (threat.type == ThreatType.Projectile)
                {
                    // Draw current position
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(threat.position, threat.radius);

                    // Draw predicted path
                    if (showPredictedPaths && threat.predictedPath.Count > 1)
                    {
                        Gizmos.color = Color.yellow;
                        for (int i = 0; i < threat.predictedPath.Count - 1; i++)
                        {
                            Gizmos.DrawLine(threat.predictedPath[i], threat.predictedPath[i + 1]);
                        }

                        Vector3 futurePos = threat.position + threat.velocity * projectileLookaheadTime;
                        Gizmos.DrawWireSphere(futurePos, threat.radius);
                    }
                }
            }

            // Draw system health indicator
            Vector3 healthPos = transform.position + Vector3.up * 5f;
            if (currentHealth.activeColliders > maxTotalActiveColliders * 0.8f)
            {
                Gizmos.color = Color.red;
            }
            else if (currentHealth.activeColliders > maxTotalActiveColliders * 0.5f)
            {
                Gizmos.color = Color.yellow;
            }
            else
            {
                Gizmos.color = Color.green;
            }
            Gizmos.DrawWireCube(healthPos, Vector3.one);
        }
    }
}