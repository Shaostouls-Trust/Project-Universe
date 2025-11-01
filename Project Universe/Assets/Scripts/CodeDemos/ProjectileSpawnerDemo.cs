using UnityEngine;
using System.Collections.Generic;
using ProjectUniverse.PowerSystem.Collision;

namespace ProjectUniverse.PowerSystem.CollisionDemo
{
    public class ProjectileSpawnerDemo : MonoBehaviour
    {
        public GameObject advancedProjectilePrefab;
        public ThreatManager threatManager;
        public Transform spawnPoint;
        public Transform target;

        [ContextMenu("Spawn Advanced Projectile")]
        void SpawnProjectile()
        {
            if (advancedProjectilePrefab && spawnPoint && target)
            {
                GameObject proj = Instantiate(advancedProjectilePrefab, spawnPoint.position, Quaternion.identity);
                var projectile = proj.GetComponent<BulletProjectile>();

                Vector3 direction = (target.position - spawnPoint.position).normalized;
                projectile.Initialize(direction, threatManager);
            }
        }
    }
}