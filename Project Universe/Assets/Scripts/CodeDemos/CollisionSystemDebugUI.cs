using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace ProjectUniverse.PowerSystem.CollisionDemo
{

    public class CollisionSystemDebugUI : MonoBehaviour
    {
        public CollisionSystemManager manager;

        void OnGUI()
        {
            if (manager == null) return;

            var stats = manager.GetPerformanceStats();
            if (stats == null) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 400));
            GUILayout.BeginVertical("box");

            GUILayout.Label($"=== Collision System Status ===");
            GUILayout.Label($"Health: {(stats.isHealthy ? "OK" : "WARNING")}");
            GUILayout.Label($"Frame Time: {stats.lastFrameTime * 1000f:F1}ms");
            GUILayout.Label($"Active Projectiles: {stats.activeProjectiles}");
            GUILayout.Label($"Active Environmental: {stats.activeEnvironmental}");
            GUILayout.Label($"Active Colliders: {stats.totalActiveColliders}");

            if (stats.warnings.Count > 0)
            {
                GUILayout.Label("Warnings:");
                foreach (var warning in stats.warnings)
                {
                    GUILayout.Label($"  - {warning}");
                }
            }

            if (GUILayout.Button("Emergency Cleanup"))
            {
                manager.EmergencyCleanup();
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}