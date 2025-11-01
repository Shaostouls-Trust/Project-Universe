using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace ProjectUniverse.PowerSystem.CollisionDemo
{
    // Simplified cable segment for demo purposes
    [System.Serializable]
    public struct DemoCableSegment
    {
        public Vector3 start;
        public Vector3 end;
        public float radius;
        public int cableId;
        public int segmentIndex;

        public DemoCableSegment(Vector3 start, Vector3 end, float radius, int cableId, int segmentIndex)
        {
            this.start = start;
            this.end = end;
            this.radius = radius;
            this.cableId = cableId;
            this.segmentIndex = segmentIndex;
        }

        public Bounds GetBounds()
        {
            Vector3 min = Vector3.Min(start, end);
            Vector3 max = Vector3.Max(start, end);
            Vector3 size = max - min + Vector3.one * radius * 2f;
            Vector3 center = (min + max) * 0.5f;
            return new Bounds(center, size);
        }
    }

    // Spatial grid for a single room
    public class RoomSpatialGrid
    {
        private Dictionary<int, List<DemoCableSegment>> spatialCells;
        private Bounds roomBounds;
        private float cellSize;
        private Vector3Int gridDimensions;

        public RoomSpatialGrid(Bounds bounds, float cellSize = 5f)
        {
            this.roomBounds = bounds;
            this.cellSize = cellSize;
            this.spatialCells = new Dictionary<int, List<DemoCableSegment>>();

            // Calculate grid dimensions
            Vector3 size = bounds.size;
            gridDimensions = new Vector3Int(
                Mathf.CeilToInt(size.x / cellSize),
                Mathf.CeilToInt(size.y / cellSize),
                Mathf.CeilToInt(size.z / cellSize)
            );

            Debug.Log($"Created spatial grid: {gridDimensions} cells, cell size: {cellSize}");
        }

        public void AddCableSegment(DemoCableSegment segment)
        {
            // Find all cells this segment intersects
            var segmentBounds = segment.GetBounds();
            var cells = GetCellsIntersecting(segmentBounds);

            foreach (var cellKey in cells)
            {
                if (!spatialCells.ContainsKey(cellKey))
                    spatialCells[cellKey] = new List<DemoCableSegment>();

                spatialCells[cellKey].Add(segment);
            }
        }

        public void Clear()
        {
            spatialCells.Clear();
        }

        public List<DemoCableSegment> QueryRadius(Vector3 center, float radius)
        {
            var results = new List<DemoCableSegment>();
            var checkedSegments = new HashSet<(int, int)>(); // (cableId, segmentIndex)

            // Create query bounds
            Bounds queryBounds = new Bounds(center, Vector3.one * radius * 2f);
            var cells = GetCellsIntersecting(queryBounds);

            foreach (var cellKey in cells)
            {
                if (spatialCells.TryGetValue(cellKey, out var segments))
                {
                    foreach (var segment in segments)
                    {
                        var key = (segment.cableId, segment.segmentIndex);
                        if (checkedSegments.Contains(key)) continue;
                        checkedSegments.Add(key);

                        // Distance check to line segment
                        float distance = DistanceToLineSegment(center, segment.start, segment.end);
                        if (distance <= radius + segment.radius)
                        {
                            results.Add(segment);
                        }
                    }
                }
            }

            return results;
        }

        private HashSet<int> GetCellsIntersecting(Bounds bounds)
        {
            var cells = new HashSet<int>();

            // Convert bounds to grid coordinates
            Vector3 min = bounds.min - roomBounds.min;
            Vector3 max = bounds.max - roomBounds.min;

            Vector3Int minCell = new Vector3Int(
                Mathf.Max(0, Mathf.FloorToInt(min.x / cellSize)),
                Mathf.Max(0, Mathf.FloorToInt(min.y / cellSize)),
                Mathf.Max(0, Mathf.FloorToInt(min.z / cellSize))
            );

            Vector3Int maxCell = new Vector3Int(
                Mathf.Min(gridDimensions.x - 1, Mathf.FloorToInt(max.x / cellSize)),
                Mathf.Min(gridDimensions.y - 1, Mathf.FloorToInt(max.y / cellSize)),
                Mathf.Min(gridDimensions.z - 1, Mathf.FloorToInt(max.z / cellSize))
            );

            for (int x = minCell.x; x <= maxCell.x; x++)
            {
                for (int y = minCell.y; y <= maxCell.y; y++)
                {
                    for (int z = minCell.z; z <= maxCell.z; z++)
                    {
                        int cellKey = GetCellKey(x, y, z);
                        cells.Add(cellKey);
                    }
                }
            }

            return cells;
        }

        private int GetCellKey(int x, int y, int z)
        {
            return x + y * gridDimensions.x + z * gridDimensions.x * gridDimensions.y;
        }

        private float DistanceToLineSegment(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
        {
            Vector3 line = lineEnd - lineStart;
            float len = line.magnitude;
            if (len < 0.001f) return Vector3.Distance(point, lineStart);

            line.Normalize();
            Vector3 v = point - lineStart;
            float d = Vector3.Dot(v, line);
            d = Mathf.Clamp(d, 0f, len);

            return Vector3.Distance(point, lineStart + line * d);
        }

        public void DebugDrawCells()
        {
            foreach (var kvp in spatialCells)
            {
                if (kvp.Value.Count > 0)
                {
                    // Convert cell key back to coordinates for debug drawing
                    int cellKey = kvp.Key;
                    int z = cellKey / (gridDimensions.x * gridDimensions.y);
                    int y = (cellKey % (gridDimensions.x * gridDimensions.y)) / gridDimensions.x;
                    int x = cellKey % gridDimensions.x;

                    Vector3 cellCenter = roomBounds.min + new Vector3(
                        (x + 0.5f) * cellSize,
                        (y + 0.5f) * cellSize,
                        (z + 0.5f) * cellSize
                    );

                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireCube(cellCenter, Vector3.one * cellSize);
                }
            }
        }
    }
}