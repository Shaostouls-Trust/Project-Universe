using UnityEngine;
using System.Collections.Generic;
using ProjectUniverse.Environment.Chemistry;
using ProjectUniverse.Environment.Volumes;

namespace ProjectUniverse.Environment.Hazards
{
    /// <summary>
    /// Scene-level manager bridging chemical reactions and fire hazards
    /// </summary>
    public class HazardIntegrationManager : MonoBehaviour
    {
        private static HazardIntegrationManager instance;
        public static HazardIntegrationManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<HazardIntegrationManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("HazardIntegrationManager");
                        instance = go.AddComponent<HazardIntegrationManager>();
                    }
                }
                return instance;
            }
        }

        [Header("Fire Spawning")]
        [SerializeField] private GameObject firePrefab;
        [SerializeField] private LayerMask surfaceLayerMask = -1;
        [SerializeField] private float maxSphereCastDistance = 50f;
        [SerializeField] private int maxFiresPerVolumeSection = 4;
        [SerializeField] private float fireSpawnChanceMultiplier = 0.7f;

        [Header("Ignition Sources")]
        [SerializeField] private float ignitionSourceInitialTemp = 800f; // Celsius
        [SerializeField] private float ignitionSourceCoolingRate = 5f; // °C per second
        [SerializeField] private float ignitionSourceDuration = 180f; // 3 minutes
        [SerializeField] private float ignitionSourceRadius = 2f;
        [SerializeField] private float fireSpawnCheckInterval = 1f; // Check every second

        [Header("Room Limits")]
        [SerializeField] private int maxFiresPerRoom = 4;
        [SerializeField] private int maxIgnitionSourcesPerRoom = 4;

        private Dictionary<int, IgnitionSource> activeIgnitionSources = new Dictionary<int, IgnitionSource>();
        private Dictionary<VolumeAtmosphereController, int> roomFireCounts = new Dictionary<VolumeAtmosphereController, int>();
        private Dictionary<VolumeAtmosphereController, int> roomIgnitionSourceCounts = new Dictionary<VolumeAtmosphereController, int>();
        private int nextIgnitionSourceId = 1;
        private float timeSinceFireCheck = 0f;

        [Header("Debug")]
        [SerializeField] private bool debugMode = true;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
        }

        private void Update()
        {
            timeSinceFireCheck += Time.deltaTime;
            UpdateIgnitionSources();
        }

        private void UpdateIgnitionSources()
        {
            var toRemove = new List<int>();

            foreach (var kvp in activeIgnitionSources)
            {
                IgnitionSource source = kvp.Value;
                source.Update(Time.deltaTime);

                if (!source.IsActive)
                {
                    toRemove.Add(kvp.Key);
                    continue;
                }

                if (timeSinceFireCheck >= fireSpawnCheckInterval && source.CanIgnite())
                {
                    AttemptFireCreation(source);
                }
            }

            if (timeSinceFireCheck >= fireSpawnCheckInterval)
            {
                timeSinceFireCheck = 0f;
            }

            foreach (int id in toRemove)
            {
                var source = activeIgnitionSources[id];
                if (source.AssignedRoom != null)
                {
                    DecrementRoomIgnitionSourceCount(source.AssignedRoom);
                }
                activeIgnitionSources.Remove(id);
                if (debugMode) Debug.Log($"IgnitionSource {id} removed (cooled or expired)");
            }
        }


        private void TriggerIgnitionInNearbyRooms(IgnitionSource source)
        {
            var rooms = FindObjectsOfType<VolumeAtmosphereController>();

            foreach (var room in rooms)
            {
                if (IsPointInRoom(source.Position, room))
                {
                    var reactionManager = room.GetComponent<Chemistry.RoomReactionManager>();
                    if (reactionManager != null)
                    {
                        reactionManager.TriggerIgnition(1f); // Continuous 1s pulses
                    }
                }
            }
        }

        private void AttemptFireCreation(IgnitionSource source)
        {
            if (firePrefab == null || source.AssignedRoom == null) return;

            // Check room fire limit
            if (GetRoomFireCount(source.AssignedRoom) >= maxFiresPerRoom)
            {
                if (debugMode) Debug.Log($"Room {source.AssignedRoom.gameObject.name} at fire limit ({maxFiresPerRoom})");
                return;
            }

            // Check for sufficient oxygen
            float oxygenConcentration = GetOxygenConcentration(source.AssignedRoom);
            if (oxygenConcentration < 0.15f) return;

            // Check for combustible gases or particulates
            bool hasCombustibles = HasCombustibleMaterials(source.AssignedRoom);
            if (!hasCombustibles && !source.AssignedRoom.HasCombustibleParticulates())
                return;

            // Spawn fire at ignition source location
            SpawnFireAtIgnitionSource(source);
        }

        private bool HasCombustibleMaterials(VolumeAtmosphereController room)
        {
            foreach (var gas in room.RoomGassesLegacy)
            {
                if (ChemistryDatabase.IsCombustible(gas.GetIDName()) && gas.GetConcentration() > 0.0001f)
                    return true;
            }
            return false;
        }

        private void SpawnFireAtIgnitionSource(IgnitionSource source)
        {
            Vector3 randomOffset = Random.insideUnitSphere * 0.5f;
            Vector3 spawnPos = source.Position + randomOffset;

            if (Physics.Raycast(spawnPos + Vector3.up, Vector3.down, out RaycastHit hit, 3f, surfaceLayerMask))
            {
                spawnPos = hit.point;
            }

            GameObject fireObj = Instantiate(firePrefab, spawnPos, Quaternion.identity);

            var fire = fireObj.GetComponent<PowerSystem.CollisionDemo.DemoFire>();
            if (fire != null)
            {
                fire.burnDuration = Random.Range(30f, 60f);
                fire.damagePerSecond = Random.Range(15f, 30f);
                fire.Initialize(source.AssignedRoom);

                // Increment room fire count
                IncrementRoomFireCount(source.AssignedRoom);
            }

            if (debugMode) Debug.Log($"Fire spawned from IgnitionSource {source.ID} in room {source.AssignedRoom.gameObject.name}");
        }

        private void SpawnFireNearIgnitionSource(IgnitionSource source, VolumeAtmosphereController room)
        {
            Vector3 randomOffset = Random.insideUnitSphere * source.IgnitionRadius;
            Vector3 spawnPos = source.Position + randomOffset;

            if (Physics.Raycast(spawnPos, Vector3.down, out RaycastHit hit, 10f, surfaceLayerMask))
            {
                GameObject fireObj = Instantiate(firePrefab, hit.point,
                    Quaternion.FromToRotation(Vector3.up, hit.normal));

                var fire = fireObj.GetComponent<PowerSystem.CollisionDemo.DemoFire>();
                if (fire != null)
                {
                    fire.burnDuration = Random.Range(30f, 60f);
                    fire.damagePerSecond = Random.Range(15f, 30f);
                    fire.Initialize(room);
                }

                if (debugMode) Debug.Log($"Fire spawned near IgnitionSource {source.ID}");
            }
        }

        private float GetOxygenConcentration(VolumeAtmosphereController room)
        {
            float totalGas = 0f;
            float oxygenAmount = 0f;

            foreach (var gas in room.RoomGassesLegacy)
            {
                float conc = gas.GetConcentration();
                totalGas += conc;
                if (gas.GetIDName() == "Oxygen")
                    oxygenAmount += conc;
            }

            return totalGas > 0f ? oxygenAmount / totalGas : 0f;
        }

        private bool IsPointInRoom(Vector3 point, VolumeAtmosphereController room)
        {
            foreach (var section in room.RoomVolumeSections)
            {
                if (section != null && section.bounds.Contains(point))
                    return true;
            }
            return false;
        }

        private void SpawnFireInVolumeSection(VolumeAtmosphereController room, BoxCollider volumeSection,
    float heatReleased, float explosionPotential)
        {
            Vector3 randomPoint = new Vector3(
                Random.Range(volumeSection.bounds.min.x, volumeSection.bounds.max.x),
                Random.Range(volumeSection.bounds.min.y, volumeSection.bounds.max.y),
                Random.Range(volumeSection.bounds.min.z, volumeSection.bounds.max.z)
            );

            Vector3 randomDirection = Random.onUnitSphere;

            if (Physics.SphereCast(randomPoint, 0.1f, randomDirection, out RaycastHit hit,
                maxSphereCastDistance, surfaceLayerMask))
            {
                IgnitionSource ignitionSource = new IgnitionSource(
                    nextIgnitionSourceId++,
                    IgnitionSourceType.Stationary,
                    hit.point,
                    ignitionSourceInitialTemp,
                    ignitionSourceCoolingRate,
                    ignitionSourceDuration,
                    ignitionSourceRadius
                );

                ignitionSource.AssignedRoom = room;
                activeIgnitionSources.Add(ignitionSource.ID, ignitionSource);

                // Increment room ignition source count
                IncrementRoomIgnitionSourceCount(room);

                if (debugMode)
                    Debug.Log($"Created IgnitionSource {ignitionSource.ID} at {hit.point} in room {room.gameObject.name}");
            }
        }

        public int CreateIgnitionSource(Vector3 position, IgnitionSourceType type = IgnitionSourceType.Stationary)
        {
            if (type == IgnitionSourceType.Mobile)
            {
                Debug.LogWarning("Mobile ignition sources are NYI");
                return -1;
            }

            IgnitionSource source = new IgnitionSource(
                nextIgnitionSourceId++,
                type,
                position,
                ignitionSourceInitialTemp,
                ignitionSourceCoolingRate,
                ignitionSourceDuration,
                ignitionSourceRadius
            );

            activeIgnitionSources.Add(source.ID, source);

            if (debugMode)
                Debug.Log($"Manually created IgnitionSource {source.ID} at {position}");

            return source.ID;
        }

        /// <summary>
        /// Spawn fires from an explosion based on heat release
        /// </summary>
        public void SpawnFiresFromExplosion(VolumeAtmosphereController sourceRoom, float heatReleased, float explosionPotential, Vector3 explosionCenter)
        {
            if (firePrefab == null || sourceRoom == null)
            {
                if (debugMode) Debug.LogWarning("Cannot spawn fires: missing firePrefab or sourceRoom");
                return;
            }

            var volumeSections = sourceRoom.RoomVolumeSections;
            if (volumeSections == null || volumeSections.Count == 0)
            {
                if (debugMode) Debug.LogWarning("No room volume sections found");
                return;
            }

            // Check room ignition source limit
            if (GetRoomIgnitionSourceCount(sourceRoom) >= maxIgnitionSourcesPerRoom)
            {
                if (debugMode) Debug.Log($"Room {sourceRoom.gameObject.name} at ignition source limit ({maxIgnitionSourcesPerRoom})");
                return;
            }

            float fireIntensity = heatReleased / 100000f;
            float explosionScale = explosionPotential / 10f;

            foreach (var volumeSection in volumeSections)
            {
                if (volumeSection == null) continue;

                Vector3 sectionCenter = volumeSection.bounds.center;
                float distanceToExplosion = Vector3.Distance(sectionCenter, explosionCenter);
                float maxRange = volumeSection.bounds.extents.magnitude * 3f;

                if (distanceToExplosion > maxRange) continue;

                float distanceFactor = 1f - (distanceToExplosion / maxRange);
                float sectionFireChance = fireIntensity * explosionScale * distanceFactor * fireSpawnChanceMultiplier;

                int numSources = 0;
                for (int i = 0; i < maxFiresPerVolumeSection; i++)
                {
                    if (Random.value < sectionFireChance)
                        numSources++;
                }

                if (debugMode && numSources > 0)
                    Debug.Log($"Spawning {numSources} ignition sources in volume section at {sectionCenter}");

                for (int i = 0; i < numSources; i++)
                {
                    // Check limit before spawning each source
                    if (GetRoomIgnitionSourceCount(sourceRoom) >= maxIgnitionSourcesPerRoom)
                    {
                        if (debugMode) Debug.Log($"Reached ignition source limit for room {sourceRoom.gameObject.name}");
                        break;
                    }

                    SpawnFireInVolumeSection(sourceRoom, volumeSection, heatReleased, explosionPotential);
                }
            }
        }

        /// <summary>
        /// Extinguish ignition sources in a volume that are below the water level
        /// </summary>
        public void ExtinguishIgnitionSourcesInVolumeByWater(
            VolumeAtmosphereController room,
            float waterHeight,
            Bounds volumeBounds)
        {
            if (room == null) return;

            var toRemove = new List<int>();

            foreach (var kvp in activeIgnitionSources)
            {
                IgnitionSource source = kvp.Value;

                // Check if this ignition source is in the specified room
                if (source.AssignedRoom != room) continue;

                // Check if position is within volume bounds
                if (!volumeBounds.Contains(source.Position)) continue;

                // Check if below water level
                if (source.Position.y <= waterHeight)
                {
                    source.ExtinguishByWater();
                    toRemove.Add(kvp.Key);

                    if (debugMode)
                    {
                        Debug.Log($"Water extinguished IgnitionSource {source.ID} at {source.Position} (water level: {waterHeight})");
                    }
                }
            }

            // Clean up extinguished sources
            foreach (int id in toRemove)
            {
                var source = activeIgnitionSources[id];
                if (source.AssignedRoom != null)
                {
                    DecrementRoomIgnitionSourceCount(source.AssignedRoom);
                }
                activeIgnitionSources.Remove(id);
            }
        }

        /// <summary>
        /// Check if a specific point is submerged in any volume with water
        /// Useful for fires to self-check if they should be extinguished
        /// </summary>
        public bool IsPointSubmergedInWater(Vector3 point, out VolumeWaterData submergingVolume)
        {
            submergingVolume = null;

            var allWaterVolumes = FindObjectsOfType<VolumeWaterData>();

            foreach (var volume in allWaterVolumes)
            {
                if (!volume.HasWater) continue;

                var collider = volume.GetComponent<BoxCollider>();
                if (collider == null) continue;

                // Check if point is within volume bounds
                if (!collider.bounds.Contains(point)) continue;

                // Check if point is below water level
                float waterHeight = volume.GetAbsoluteWaterHeight();
                if (point.y <= waterHeight)
                {
                    submergingVolume = volume;
                    return true;
                }
            }

            return false;
        }

        private int GetRoomFireCount(VolumeAtmosphereController room)
        {
            return roomFireCounts.ContainsKey(room) ? roomFireCounts[room] : 0;
        }

        private int GetRoomIgnitionSourceCount(VolumeAtmosphereController room)
        {
            return roomIgnitionSourceCounts.ContainsKey(room) ? roomIgnitionSourceCounts[room] : 0;
        }

        private void IncrementRoomFireCount(VolumeAtmosphereController room)
        {
            if (!roomFireCounts.ContainsKey(room))
                roomFireCounts[room] = 0;
            roomFireCounts[room]++;
        }

        private void IncrementRoomIgnitionSourceCount(VolumeAtmosphereController room)
        {
            if (!roomIgnitionSourceCounts.ContainsKey(room))
                roomIgnitionSourceCounts[room] = 0;
            roomIgnitionSourceCounts[room]++;
        }

        public void DecrementRoomFireCount(VolumeAtmosphereController room)
        {
            if (roomFireCounts.ContainsKey(room))
            {
                roomFireCounts[room] = Mathf.Max(0, roomFireCounts[room] - 1);
            }
        }

        private void DecrementRoomIgnitionSourceCount(VolumeAtmosphereController room)
        {
            if (roomIgnitionSourceCounts.ContainsKey(room))
            {
                roomIgnitionSourceCounts[room] = Mathf.Max(0, roomIgnitionSourceCounts[room] - 1);
            }
        }

        void OnDrawGizmos()
        {
            if (!debugMode) return;

            foreach (var source in activeIgnitionSources.Values)
            {
                float intensity = source.Temperature / source.InitialTemperature;
                Gizmos.color = Color.Lerp(Color.yellow, Color.red, intensity);
                Gizmos.DrawWireSphere(source.Position, source.IgnitionRadius);

                if (source.CanIgnite())
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(source.Position, 0.2f);
                }
            }
        }
    }
}