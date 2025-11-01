using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ProjectUniverse.PowerSystem.Collision;

namespace ProjectUniverse.PowerSystem.CollisionDemo
{
    public class CollisionSystemManager : MonoBehaviour
    {
        [Header("Subsystems")]
        public CableSpatialIndexDemo spatialIndex;
        public ColliderPool colliderPool;
        //public PredictiveThreatManager projectileManager;
        public ThreatManager projectileManager;
        public EnvironmentalThreatManager environmentalManager;

        [Header("Performance Monitoring")]
        public bool enablePerformanceMonitoring = true;
        public float performanceCheckInterval = 1f;

        [Header("Safety Limits")]
        public int maxTotalThreats = 50;
        public int maxProjectileThreats = 30;
        public int maxEnvironmentalThreats = 10;
        public float maxSpatialQueryRadius = 100f; // Prevent accidentally querying entire world

        [Header("Integration")]
        public bool simulateCableDamage = true; // For demo, actually apply damage

        private PerformanceStats currentStats;
        private float lastPerformanceCheck;

        [System.Serializable]
        public class PerformanceStats
        {
            public int activeProjectiles;
            public int activeEnvironmental;
            public int totalActiveColliders;
            public int spatialCells;
            public float lastFrameTime;
            public int spatialQueriesPerSecond;
            public bool isHealthy = true;
            public List<string> warnings = new List<string>();
        }

        void Start()
        {
            ValidateSubsystems();

            if (enablePerformanceMonitoring)
            {
                InvokeRepeating(nameof(MonitorPerformance), performanceCheckInterval, performanceCheckInterval);
            }
        }

        void ValidateSubsystems()
        {
            if (spatialIndex == null)
                Debug.LogError("Spatial index is not assigned!");
            if (colliderPool == null)
                Debug.LogError("Collider pool is not assigned!");
            if (projectileManager == null)
                Debug.LogError("Projectile manager is not assigned!");
            if (environmentalManager == null)
                Debug.LogError("Environmental manager is not assigned!");

            // Ensure managers have correct references
            if (projectileManager != null)
            {
                projectileManager.spatialIndex = spatialIndex;
                projectileManager.colliderPool = colliderPool;
            }

            if (environmentalManager != null)
            {
                environmentalManager.spatialIndex = spatialIndex;
                environmentalManager.colliderPool = colliderPool;
            }
        }

        void MonitorPerformance()
        {
            currentStats = new PerformanceStats();
            currentStats.warnings.Clear();

            // Check threat counts
            if (projectileManager != null)
            {
                currentStats.activeProjectiles = projectileManager.GetActiveThreatCount();
                if (currentStats.activeProjectiles > maxProjectileThreats)
                {
                    currentStats.warnings.Add($"Too many projectile threats: {currentStats.activeProjectiles}/{maxProjectileThreats}");
                    currentStats.isHealthy = false;
                }
            }

            if (environmentalManager != null)
            {
                currentStats.activeEnvironmental = environmentalManager.GetActiveEnvironmentalThreatCount();
                if (currentStats.activeEnvironmental > maxEnvironmentalThreats)
                {
                    currentStats.warnings.Add($"Too many environmental threats: {currentStats.activeEnvironmental}/{maxEnvironmentalThreats}");
                    currentStats.isHealthy = false;
                }
            }

            // Check collider pool
            if (colliderPool != null)
            {
                var poolStats = colliderPool.GetStats();
                currentStats.totalActiveColliders = poolStats.activeColliders;

                if (poolStats.activeColliders > colliderPool.maxActiveColliders * 0.9f)
                {
                    currentStats.warnings.Add($"Collider pool near capacity: {poolStats.activeColliders}/{colliderPool.maxActiveColliders}");
                    currentStats.isHealthy = false;
                }
            }

            // Check frame time
            currentStats.lastFrameTime = Time.deltaTime;
            if (Time.deltaTime > 0.033f) // More than 33ms (30 FPS threshold)
            {
                currentStats.warnings.Add($"Frame time high: {Time.deltaTime * 1000f:F1}ms");
                currentStats.isHealthy = false;
            }

            // Log warnings
            if (!currentStats.isHealthy)
            {
                Debug.LogWarning($"Collision system performance issues detected: {string.Join(", ", currentStats.warnings)}");
            }
        }

        // Public API for cable damage with safety checks
        public void ApplyCableDamage(int cableId, int segmentIndex, float damage, DamageType type)
        {
            if (!simulateCableDamage) return;

            // In real implementation, this would interface with PowerSystemPathManager
            Debug.Log($"[DAMAGE] Cable {cableId} segment {segmentIndex}: {damage} {type} damage");

            // Simulate integration point
            var pathManager = PowerSystemPathManager.Instance;
            if (pathManager != null)
            {
                // pathManager.ApplyDamageToSegment(cableId, segmentIndex, damage);
            }
        }

        public void ApplyCableHeat(int cableId, int segmentIndex, float heatAmount)
        {
            if (!simulateCableDamage) return;

            Debug.Log($"[HEAT] Cable {cableId} segment {segmentIndex}: {heatAmount} heat");

            // Simulate integration point
            var pathManager = PowerSystemPathManager.Instance;
            if (pathManager != null)
            {
                // pathManager.ApplyHeatToSegment(cableId, segmentIndex, heatAmount);
            }
        }

        // Safe spatial query with radius limit
        public List<DemoCableSegment> SafeQueryRadius(Vector3 position, float radius)
        {
            if (radius > maxSpatialQueryRadius)
            {
                Debug.LogWarning($"Query radius {radius} exceeds maximum {maxSpatialQueryRadius}. Clamping.");
                radius = maxSpatialQueryRadius;
            }

            return spatialIndex.QueryRadiusAllRooms(position, radius);
        }

        // Emergency cleanup
        [ContextMenu("Emergency Cleanup")]
        public void EmergencyCleanup()
        {
            Debug.LogWarning("Performing emergency cleanup of collision system!");

            // Clear all active threats
            if (projectileManager != null)
            {
                var activeThreats = projectileManager.GetActiveThreats();
                foreach (var threat in activeThreats)
                {
                    projectileManager.UnregisterThreat(threat.id);
                }
            }

            if (environmentalManager != null)
            {
                var envThreats = environmentalManager.GetActiveThreats();
                foreach (var threat in envThreats)
                {
                    environmentalManager.UnregisterThreat(threat.id);
                }
            }

            // Force collider pool cleanup
            if (colliderPool != null)
            {
                colliderPool.CheckForOrphans();
            }

            Debug.Log("Emergency cleanup complete");
        }

        public PerformanceStats GetPerformanceStats() => currentStats;

        public enum DamageType
        {
            Projectile,
            Fire,
            Heat,
            Explosion,
            Environmental
        }
    }
}