using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ProjectUniverse.Environment.Hazards;

namespace ProjectUniverse.PowerSystem.Collision
{
    /// <summary>
    /// Spatial grid for efficient querying of environmental damage receivers (machines, etc.)
    /// </summary>
    public class MachineSpatialGrid : MonoBehaviour
    {
        private class SpatialCell
        {
            public HashSet<IEnvironmentalDamageReceiver> machines = new HashSet<IEnvironmentalDamageReceiver>();
        }

        [SerializeField] private float cellSize = 5f;
        private Dictionary<Vector3Int, SpatialCell> grid = new Dictionary<Vector3Int, SpatialCell>();

        public void RegisterMachine(IEnvironmentalDamageReceiver machine)
        {
            if (machine is MonoBehaviour mb && mb != null)
            {
                Vector3Int cellKey = WorldToGridCoordinates(mb.transform.position);

                if (!grid.TryGetValue(cellKey, out var cell))
                {
                    cell = new SpatialCell();
                    grid[cellKey] = cell;
                }

                cell.machines.Add(machine);
            }
        }

        public void UnregisterMachine(IEnvironmentalDamageReceiver machine)
        {
            if (machine is MonoBehaviour mb && mb != null)
            {
                Vector3Int cellKey = WorldToGridCoordinates(mb.transform.position);

                if (grid.TryGetValue(cellKey, out var cell))
                {
                    cell.machines.Remove(machine);

                    if (cell.machines.Count == 0)
                    {
                        grid.Remove(cellKey);
                    }
                }
            }
        }

        public void UpdateMachinePosition(IEnvironmentalDamageReceiver machine, Vector3 oldPosition, Vector3 newPosition)
        {
            Vector3Int oldCell = WorldToGridCoordinates(oldPosition);
            Vector3Int newCell = WorldToGridCoordinates(newPosition);

            if (oldCell != newCell)
            {
                UnregisterMachine(machine);
                RegisterMachine(machine);
            }
        }

        public List<IEnvironmentalDamageReceiver> QueryRadius(Vector3 position, float radius)
        {
            var results = new List<IEnvironmentalDamageReceiver>();
            var checkedMachines = new HashSet<IEnvironmentalDamageReceiver>();

            float radiusSquared = radius * radius;
            Vector3Int centerCell = WorldToGridCoordinates(position);
            int cellRange = Mathf.CeilToInt(radius / cellSize);

            for (int x = centerCell.x - cellRange; x <= centerCell.x + cellRange; x++)
            {
                for (int y = centerCell.y - cellRange; y <= centerCell.y + cellRange; y++)
                {
                    for (int z = centerCell.z - cellRange; z <= centerCell.z + cellRange; z++)
                    {
                        Vector3Int key = new Vector3Int(x, y, z);

                        if (grid.TryGetValue(key, out var cell))
                        {
                            foreach (var machine in cell.machines)
                            {
                                if (machine is MonoBehaviour mb && mb != null && !checkedMachines.Contains(machine))
                                {
                                    float distanceSquared = (mb.transform.position - position).sqrMagnitude;

                                    if (distanceSquared <= radiusSquared)
                                    {
                                        results.Add(machine);
                                        checkedMachines.Add(machine);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return results;
        }

        private Vector3Int WorldToGridCoordinates(Vector3 worldPos)
        {
            return new Vector3Int(
                Mathf.FloorToInt(worldPos.x / cellSize),
                Mathf.FloorToInt(worldPos.y / cellSize),
                Mathf.FloorToInt(worldPos.z / cellSize)
            );
        }

        public void Clear()
        {
            grid.Clear();
        }
    }
}