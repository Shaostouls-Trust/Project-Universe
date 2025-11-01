using UnityEngine;
using ProjectUniverse.PowerSystem.CollisionDemo;

namespace ProjectUniverse.PowerSystem.Collision
{
    /// <summary>
    /// Enhanced projectile with ricochet capability and robust error handling
    /// </summary>
    public class BulletProjectile : MonoBehaviour
    {
        [Header("Movement")]
        public float speed = 20f;
        public float damageRadius = 0.5f;
        public float damage = 50f;

        [Header("Ricochet")]
        public int maxRicochets = 2;
        public LayerMask ricochetLayers = -1;
        public float ricochetSpeedLoss = 0.8f; // Speed multiplier after ricochet

        [Header("Lifetime")]
        public float maxLifetime = 10f;

        protected Vector3 velocity;
        protected ThreatManager threatManager;
        protected int threatId = -1;
        protected int currentRicochets = 0;
        protected float spawnTime;

        void Start()
        {
            spawnTime = Time.time;

            // Auto-find threat manager if not set
            if (threatManager == null)
            {
                threatManager = FindObjectOfType<ThreatManager>();
            }

            // Initialize with default forward direction if not already initialized
            if (velocity == Vector3.zero)
            {
                Initialize(transform.forward, threatManager);
            }
        }

        public void Initialize(Vector3 direction, ThreatManager manager)
        {
            velocity = direction.normalized * speed;
            threatManager = manager;
            spawnTime = Time.time;

            // Register as temporary threat
            if (threatManager != null)
            {
                threatId = threatManager.RegisterTemporaryThreat(this);
            }
            else
            {
                Debug.LogWarning("No ThreatManager found - projectile will not activate cable colliders");
            }
        }

        void FixedUpdate()
        {
            // Check lifetime
            if (Time.time - spawnTime > maxLifetime)
            {
                DestroyProjectile();
                return;
            }

            // Store old position for ricochet detection
            Vector3 oldPos = transform.position;
            Vector3 newPos = oldPos + velocity * Time.fixedDeltaTime;

            // Check for ricochet
            if (maxRicochets > 0 && currentRicochets < maxRicochets)
            {
                RaycastHit hit;
                if (Physics.Linecast(oldPos, newPos, out hit, ricochetLayers))
                {
                    HandleRicochet(hit);
                    return; // Ricochet handling updates position
                }
            }

            // Normal movement
            transform.position = newPos;

            // Update threat position
            if (threatManager != null && threatId >= 0)
            {
                threatManager.UpdateThreatPosition(threatId, transform.position);
            }
        }

        void HandleRicochet(RaycastHit hit)
        {
            // Calculate ricochet
            Vector3 incomingVector = velocity.normalized;
            Vector3 reflectedVector = Vector3.Reflect(incomingVector, hit.normal);

            // Update velocity with speed loss
            float newSpeed = velocity.magnitude * ricochetSpeedLoss;
            velocity = reflectedVector * newSpeed;

            // Position slightly away from collision point to prevent re-collision
            transform.position = hit.point + hit.normal * 0.1f;

            // Update rotation to match new direction
            if (velocity != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(velocity.normalized);
            }

            currentRicochets++;

            // Notify threat manager of trajectory change
            if (threatManager != null && threatId >= 0)
            {
                threatManager.OnProjectileRicochet(threatId, transform.position, velocity);
            }

            Debug.Log($"Projectile ricocheted ({currentRicochets}/{maxRicochets}), new speed: {newSpeed:F1}");
        }

        void OnTriggerEnter(Collider other)
        {
            // Hit something - apply damage and destroy
            if (threatManager != null && threatId >= 0)
            {
                threatManager.ExecuteThreatDamage(threatId, transform.position, damageRadius, damage);
            }

            Debug.Log($"Projectile hit {other.name}");
            DestroyProjectile();
        }

        void DestroyProjectile()
        {
            // Ensure we're unregistered from threat manager
            if (threatManager != null && threatId >= 0)
            {
                threatManager.UnregisterThreat(threatId);
                threatId = -1;
            }

            Destroy(gameObject);
        }

        void OnDestroy()
        {
            // Safety cleanup
            if (threatManager != null && threatId >= 0)
            {
                threatManager.UnregisterThreat(threatId);
            }
        }

        public Vector3 GetVelocity() => velocity;

        public float GetSpeed() => velocity.magnitude;

        public int GetRicochetCount() => currentRicochets;

        public float GetLifetime() => Time.time - spawnTime;

        void OnDrawGizmos()
        {
            // Draw velocity vector
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, velocity.normalized * 2f);

            // Draw damage radius
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Gizmos.DrawSphere(transform.position, damageRadius);

            // Draw ricochet count indicator
            if (currentRicochets > 0)
            {
                Gizmos.color = Color.yellow;
                for (int i = 0; i < currentRicochets; i++)
                {
                    Gizmos.DrawWireCube(transform.position + Vector3.up * (0.5f + i * 0.2f), Vector3.one * 0.1f);
                }
            }
        }
    }
}