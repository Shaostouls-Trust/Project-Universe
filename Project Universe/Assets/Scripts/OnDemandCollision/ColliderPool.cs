using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ProjectUniverse.PowerSystem.CollisionDemo;

namespace ProjectUniverse.PowerSystem.Collision
{
    /// <summary>
    /// Unified collider pool with reference counting and robust error handling
    /// </summary>
    public class ColliderPool : MonoBehaviour
    {
        [System.Serializable]
        public class ColliderReference
        {
            public GameObject collider;
            public HashSet<int> threatIds = new HashSet<int>();
            public float lastAccessTime;
            public (int cableId, int segmentIndex) segmentKey;
            public bool isOrphaned;
        }

        [System.Serializable]
        public class ColliderPoolStats
        {
            public int activeColliders;
            public int pooledColliders;
            public int totalColliders;
            public int multiThreatColliders;
            public int orphanedColliders;
        }

        [Header("Pool Configuration")]
        public int initialPoolSize = 50;
        public int maxActiveColliders = 1000;

        [Header("Safety Settings")]
        public float orphanCheckInterval = 5f;
        public float colliderTimeout = 10f;

        [Header("Debug")]
        public bool logColliderOperations = false;

        private Dictionary<(int, int), ColliderReference> activeColliderRefs = new Dictionary<(int, int), ColliderReference>();
        private Queue<GameObject> colliderPool = new Queue<GameObject>();
        private Queue<GameObject> orphanedColliders = new Queue<GameObject>();

        void Start()
        {
            InitializePool();
            InvokeRepeating(nameof(CheckForOrphans), orphanCheckInterval, orphanCheckInterval);
        }

        void InitializePool()
        {
            for (int i = 0; i < initialPoolSize; i++)
            {
                CreatePooledCollider();
            }
        }

        GameObject CreatePooledCollider()
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            obj.name = "PooledCableCollider";
            obj.transform.localScale = new Vector3(0.2f, 0.5f, 0.2f);
            obj.SetActive(false);

            // Setup visual appearance
            var renderer = obj.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(0, 1, 0, 0.3f);
            }

            colliderPool.Enqueue(obj);
            return obj;
        }

        public void ActivateColliderForSegment(DemoCableSegment segment, int threatId)
        {
            var key = (segment.cableId, segment.segmentIndex);

            // Check hard limit
            if (activeColliderRefs.Count >= maxActiveColliders && !activeColliderRefs.ContainsKey(key))
            {
                Debug.LogWarning($"Maximum active colliders reached ({maxActiveColliders})! Skipping activation.");
                return;
            }

            if (activeColliderRefs.TryGetValue(key, out var reference))
            {
                // Add threat to existing collider
                reference.threatIds.Add(threatId);
                reference.lastAccessTime = Time.time;
                reference.isOrphaned = false;

                if (logColliderOperations)
                {
                    Debug.Log($"Added threat {threatId} to existing collider for segment {key}. Total threats: {reference.threatIds.Count}");
                }
            }
            else
            {
                // Create new collider
                GameObject collider = GetPooledCollider();
                if (collider == null)
                {
                    Debug.LogError("Failed to get pooled collider!");
                    return;
                }

                // Position collider along cable segment
                Vector3 center = (segment.start + segment.end) * 0.5f;
                Vector3 direction = (segment.end - segment.start).normalized;
                float length = Vector3.Distance(segment.start, segment.end);

                collider.transform.position = center;
                if (direction != Vector3.zero)
                {
                    collider.transform.rotation = Quaternion.LookRotation(direction);
                }

                // Adjust scale based on segment length
                var capsule = collider.GetComponent<CapsuleCollider>();
                if (capsule != null)
                {
                    capsule.height = Mathf.Max(length, 0.5f);
                    capsule.radius = segment.radius;
                }

                var newRef = new ColliderReference
                {
                    collider = collider,
                    segmentKey = key,
                    lastAccessTime = Time.time,
                    isOrphaned = false
                };
                newRef.threatIds.Add(threatId);

                activeColliderRefs[key] = newRef;

                if (logColliderOperations)
                {
                    Debug.Log($"Created new collider for segment {key} with threat {threatId}");
                }
            }
        }

        public void DeactivateColliderForSegment(int cableId, int segmentIndex, int threatId)
        {
            var key = (cableId, segmentIndex);

            if (activeColliderRefs.TryGetValue(key, out var reference))
            {
                reference.threatIds.Remove(threatId);
                reference.lastAccessTime = Time.time;

                if (reference.threatIds.Count == 0)
                {
                    // No more threats reference this collider
                    ReturnColliderToPool(reference.collider);
                    activeColliderRefs.Remove(key);

                    if (logColliderOperations)
                    {
                        Debug.Log($"Returned collider for segment {key} to pool (no more threats)");
                    }
                }
                else if (logColliderOperations)
                {
                    Debug.Log($"Removed threat {threatId} from collider for segment {key}. Remaining threats: {reference.threatIds.Count}");
                }
            }
        }

        GameObject GetPooledCollider()
        {
            if (colliderPool.Count > 0)
            {
                var obj = colliderPool.Dequeue();
                obj.SetActive(true);
                return obj;
            }
            else
            {
                // Pool is empty, create new one
                var obj = CreatePooledCollider();
                if (colliderPool.Count > 0)
                {
                    obj = colliderPool.Dequeue();
                    obj.SetActive(true);
                }
                return obj;
            }
        }

        void ReturnColliderToPool(GameObject collider)
        {
            if (collider != null)
            {
                collider.SetActive(false);
                colliderPool.Enqueue(collider);
            }
        }

        public void CheckForOrphans()
        {
            float currentTime = Time.time;
            var keysToRemove = new List<(int, int)>();

            foreach (var kvp in activeColliderRefs)
            {
                if (currentTime - kvp.Value.lastAccessTime > colliderTimeout)
                {
                    Debug.LogWarning($"Found orphaned collider for segment {kvp.Key} (timeout). Cleaning up.");
                    kvp.Value.isOrphaned = true;
                    orphanedColliders.Enqueue(kvp.Value.collider);
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                activeColliderRefs.Remove(key);
            }

            // Return orphaned colliders to pool
            while (orphanedColliders.Count > 0)
            {
                var collider = orphanedColliders.Dequeue();
                ReturnColliderToPool(collider);
            }
        }

        public ColliderPoolStats GetStats()
        {
            return new ColliderPoolStats
            {
                activeColliders = activeColliderRefs.Count,
                pooledColliders = colliderPool.Count,
                totalColliders = activeColliderRefs.Count + colliderPool.Count,
                multiThreatColliders = activeColliderRefs.Count(kvp => kvp.Value.threatIds.Count > 1),
                orphanedColliders = activeColliderRefs.Count(kvp => kvp.Value.isOrphaned)
            };
        }

        [ContextMenu("Force Cleanup All")]
        public void ForceCleanupAll()
        {
            foreach (var kvp in activeColliderRefs.ToList())
            {
                ReturnColliderToPool(kvp.Value.collider);
            }
            activeColliderRefs.Clear();
        }
    }
}