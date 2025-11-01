using ProjectUniverse.Animation.Controllers;
using ProjectUniverse.Environment.Gas;
using ProjectUniverse.Environment.Volumes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectUniverse.Environment.Hazards
{
    /// <summary>
    /// Manages room connections and pathfinding for mobile ignition sources
    /// </summary>
    public class RoomConnectionGraph : MonoBehaviour
    {
        private static RoomConnectionGraph instance;
        public static RoomConnectionGraph Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<RoomConnectionGraph>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("RoomConnectionGraph");
                        instance = go.AddComponent<RoomConnectionGraph>();
                    }
                }
                return instance;
            }
        }

        [Header("Configuration")]
        [SerializeField] private float connectionDiscoveryInterval = 5f;
        [SerializeField] private float pressureUpdateInterval = 2f;
        [SerializeField] private float maxConnectionWeight = 0.6f; // Cap at 60% for any single connection
        [SerializeField] private bool debugMode = true;

        // Room -> List of connections from that room
        private Dictionary<VolumeAtmosphereController, List<RoomConnection>> connectionGraph;
        private Dictionary<VolumeAtmosphereController, float> roomLastDiscoveryTime;
        private Dictionary<VolumeAtmosphereController, bool> roomsDirty;

        // Cache for active rooms
        private HashSet<VolumeAtmosphereController> activeRooms;
        private float lastPressureUpdate;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;

            connectionGraph = new Dictionary<VolumeAtmosphereController, List<RoomConnection>>();
            roomLastDiscoveryTime = new Dictionary<VolumeAtmosphereController, float>();
            roomsDirty = new Dictionary<VolumeAtmosphereController, bool>();
            activeRooms = new HashSet<VolumeAtmosphereController>();
        }

        /// <summary>
        /// Mark a room as active (contains fire, sources, or players)
        /// </summary>
        public void MarkRoomActive(VolumeAtmosphereController room, float duration = 30f)
        {
            if (!activeRooms.Contains(room))
            {
                activeRooms.Add(room);
                // Schedule removal after duration
                StartCoroutine(RemoveActiveRoomAfterDelay(room, duration));
            }
        }

        private System.Collections.IEnumerator RemoveActiveRoomAfterDelay(VolumeAtmosphereController room, float delay)
        {
            yield return new WaitForSeconds(delay);

            // Check if room should still be active (has fires, sources, etc.)
            if (!RoomHasActiveSources(room))
            {
                activeRooms.Remove(room);
            }
        }

        private bool RoomHasActiveSources(VolumeAtmosphereController room)
        {
            // Placeholder - will be implemented with HazardIntegrationManager
            // Check for: stationary sources, mobile sources, active fires
            return false;
        }

        /// <summary>
        /// Get all connections from a room, discovering them if needed
        /// </summary>
        public List<RoomConnection> GetRoomConnections(VolumeAtmosphereController room)
        {
            if (room == null) return new List<RoomConnection>();

            // Check if we need to discover/refresh connections
            if (!connectionGraph.ContainsKey(room) ||
                (roomsDirty.ContainsKey(room) && roomsDirty[room]) ||
                Time.time - roomLastDiscoveryTime.GetValueOrDefault(room, 0) > connectionDiscoveryInterval)
            {
                DiscoverRoomConnections(room);
            }

            // Update weights if needed
            if (Time.time - lastPressureUpdate > pressureUpdateInterval)
            {
                UpdateConnectionWeights(room);
                lastPressureUpdate = Time.time;
            }

            return connectionGraph.GetValueOrDefault(room, new List<RoomConnection>());
        }

        /// <summary>
        /// Discover all connections from a room
        /// </summary>
        private void DiscoverRoomConnections(VolumeAtmosphereController room)
        {
            List<RoomConnection> connections = new List<RoomConnection>();

            // 1. Discover door connections
            DiscoverDoorConnections(room, connections);

            // 2. Discover pipe/vent connections
            DiscoverPipeConnections(room, connections);

            // 3. Discover breach connections (placeholder for dynamic holes)
            DiscoverBreachConnections(room, connections);

            // Store and mark as clean
            connectionGraph[room] = connections;
            roomLastDiscoveryTime[room] = Time.time;
            roomsDirty[room] = false;

            if (debugMode)
            {
                Debug.Log($"Discovered {connections.Count} connections for room {room.gameObject.name}");
            }
        }

        private void DiscoverDoorConnections(VolumeAtmosphereController room, List<RoomConnection> connections)
        {
            var neighborEmpties = room.GetNeighborEmpties;
            if (neighborEmpties == null) return;

            // Track connections to same room to combine multiple doors
            Dictionary<VolumeAtmosphereController, RoomConnection> roomToConnection =
                new Dictionary<VolumeAtmosphereController, RoomConnection>();

            foreach (GameObject neighborEmpty in neighborEmpties)
            {
                if (neighborEmpty == null) continue;

                VolumeNode node = neighborEmpty.GetComponent<VolumeNode>();
                if (node == null || node.VolumeLink == null) continue;

                VolumeAtmosphereController targetRoom = node.VolumeLink.GetComponent<VolumeAtmosphereController>();
                if (targetRoom == null || targetRoom == room) continue;

                GameObject doorObj = node.GetDoor();
                if (doorObj == null) continue;

                DoorAnimator door = doorObj.GetComponent<DoorAnimator>();
                if (door == null) continue;

                // Check if we already have a connection to this room
                if (roomToConnection.ContainsKey(targetRoom))
                {
                    // Multiple doors to same room - increase weight
                    roomToConnection[targetRoom].BaseWeight += 0.2f;
                }
                else
                {
                    // New connection
                    RoomConnection connection = new RoomConnection(door, targetRoom, doorObj.transform.position);
                    connections.Add(connection);
                    roomToConnection[targetRoom] = connection;
                }
            }
        }

        private void DiscoverPipeConnections(VolumeAtmosphereController room, List<RoomConnection> connections)
        {
            var pipeSections = room.VolumeGasPipeSections;
            if (pipeSections == null || pipeSections.Count == 0) return;

            foreach (PipeSection section in pipeSections)
            {
                if (section == null) continue;

                // Check if this pipe section has broken pipes or active vents
                bool hasBrokenPipe = false;
                bool hasActiveVent = false;
                VolumeAtmosphereController connectedRoom = null;
                Vector3 connectionPoint = Vector3.zero;

                // Placeholder for pipe checking logic
                // This would check section.GetPipes() for burst pipes and active vents
                // And determine which room they connect to

                if (hasBrokenPipe || hasActiveVent)
                {
                    RoomConnection pipeConnection = new RoomConnection(section, connectedRoom, connectionPoint);
                    connections.Add(pipeConnection);
                }
            }
        }

        private void DiscoverBreachConnections(VolumeAtmosphereController room, List<RoomConnection> connections)
        {
            // Placeholder for dynamic breach detection
            // Would check for known breach locations and create connections
        }

        private void UpdateConnectionWeights(VolumeAtmosphereController room)
        {
            if (!connectionGraph.ContainsKey(room)) return;

            List<RoomConnection> connections = connectionGraph[room];
            float roomPressure = room.Pressure;

            // Update passability for doors
            foreach (var connection in connections)
            {
                connection.UpdatePassability();

                // Calculate pressure differential
                float targetPressure = connection.TargetRoom != null ? connection.TargetRoom.Pressure : 1.0f;
                float pressureDiff = roomPressure - targetPressure;

                connection.CalculateWeight(pressureDiff);
            }

            // Normalize weights and apply cap
            NormalizeAndCapWeights(connections);
        }

        private void NormalizeAndCapWeights(List<RoomConnection> connections)
        {
            if (connections.Count == 0) return;

            float totalWeight = connections.Sum(c => c.CurrentWeight);
            if (totalWeight <= 0) return;

            // Normalize weights
            foreach (var connection in connections)
            {
                connection.CurrentWeight /= totalWeight;

                // Apply cap
                if (connection.CurrentWeight > maxConnectionWeight)
                {
                    connection.CurrentWeight = maxConnectionWeight;
                }
            }

            // Re-normalize after capping
            totalWeight = connections.Sum(c => c.CurrentWeight);
            if (totalWeight > 0 && totalWeight != 1.0f)
            {
                foreach (var connection in connections)
                {
                    connection.CurrentWeight /= totalWeight;
                }
            }
        }

        /// <summary>
        /// Select a random connection based on weighted probability
        /// </summary>
        public RoomConnection SelectWeightedConnection(VolumeAtmosphereController fromRoom)
        {
            List<RoomConnection> connections = GetRoomConnections(fromRoom);
            if (connections.Count == 0) return null;

            // Filter to passable connections (or queued if door)
            var validConnections = connections.Where(c =>
                c.IsPassable || c.Type == RoomConnection.ConnectionType.Door).ToList();

            if (validConnections.Count == 0) return null;

            // Weighted random selection
            float random = UnityEngine.Random.value;
            float cumulative = 0f;

            foreach (var connection in validConnections)
            {
                cumulative += connection.CurrentWeight;
                if (random <= cumulative)
                {
                    return connection;
                }
            }

            // Fallback to last connection
            return validConnections[validConnections.Count - 1];
        }

        /// <summary>
        /// Mark a room's connections as dirty (needs refresh)
        /// </summary>
        public void MarkRoomDirty(VolumeAtmosphereController room)
        {
            roomsDirty[room] = true;
        }

        /// <summary>
        /// Handle door state change events
        /// </summary>
        public void OnDoorStateChanged(DoorAnimator door)
        {
            // Find all rooms that have this door as a connection and mark them dirty
            foreach (var kvp in connectionGraph)
            {
                foreach (var connection in kvp.Value)
                {
                    if (connection.AssociatedDoor == door)
                    {
                        MarkRoomDirty(kvp.Key);
                        break;
                    }
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (!debugMode || connectionGraph == null) return;

            foreach (var kvp in connectionGraph)
            {
                Vector3 roomCenter = kvp.Key.transform.position;

                foreach (var connection in kvp.Value)
                {
                    // Color based on connection type
                    switch (connection.Type)
                    {
                        case RoomConnection.ConnectionType.Door:
                            Gizmos.color = connection.IsPassable ? Color.green : Color.yellow;
                            break;
                        case RoomConnection.ConnectionType.PipeVent:
                            Gizmos.color = Color.cyan;
                            break;
                        case RoomConnection.ConnectionType.Breach:
                            Gizmos.color = Color.red;
                            break;
                    }

                    // Draw connection line
                    Gizmos.DrawLine(roomCenter, connection.ConnectionPoint);
                    Gizmos.DrawWireSphere(connection.ConnectionPoint, 0.5f);
                }
            }
        }
    }
}