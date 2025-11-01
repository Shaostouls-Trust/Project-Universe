using UnityEngine;
using System.Collections.Generic;

namespace ProjectUniverse.PowerSystem.CollisionDemo
{
    public class FireHazard : MonoBehaviour
    {
        [Header("Fire Configuration")]
        public float damageRadius = 3f;
        public float damagePerSecond = 10f;
        public float lifetime = 30f; // Fire burns out after 30 seconds
        public bool isPermanent = false; // For fires that don't burn out

        [Header("Spread")]
        public bool canSpread = true;
        public float spreadCheckInterval = 5f;
        public float spreadChance = 0.3f;
        public float spreadRadius = 2f;

        private EnvironmentalThreatManager environmentalManager;
        private int threatId;
        private float creationTime;
        private float lastSpreadCheck;
        private float lastDamageTime;

        void Start()
        {
            creationTime = Time.time;
            lastSpreadCheck = Time.time;
            lastDamageTime = Time.time;

            // Auto-find manager if not set
            if (environmentalManager == null)
                environmentalManager = FindObjectOfType<EnvironmentalThreatManager>();

            if (environmentalManager != null)
            {
                //threatId = environmentalManager.RegisterEnvironmentalThreat(this);
            }
        }

        void Update()
        {
            // Check lifetime
            if (!isPermanent && Time.time - creationTime > lifetime)
            {
                ExtinguishFire();
                return;
            }

            // Apply damage at intervals (not every frame)
            if (Time.time - lastDamageTime > 1f)
            {
                ApplyFireDamage();
                lastDamageTime = Time.time;
            }

            // Check for spread
            if (canSpread && Time.time - lastSpreadCheck > spreadCheckInterval)
            {
                CheckFireSpread();
                lastSpreadCheck = Time.time;
            }
        }

        void ApplyFireDamage()
        {
            if (environmentalManager != null)
            {
                //environmentalManager.ApplyEnvironmentalDamage(threatId, transform.position, damageRadius, damagePerSecond);
            }
        }

        void CheckFireSpread()
        {
            if (Random.value < spreadChance)
            {
                // Pick random point nearby
                Vector2 randomCircle = Random.insideUnitCircle * spreadRadius;
                Vector3 spreadPoint = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

                // Check if there's something flammable there (simplified for demo)
                if (environmentalManager != null)
                {
                    //environmentalManager.TrySpawnFireAt(spreadPoint);
                }
            }
        }

        public void ExtinguishFire()
        {
            if (environmentalManager != null)
            {
                //environmentalManager.UnregisterEnvironmentalThreat(threatId);
            }
            Destroy(gameObject);
        }
    }
}