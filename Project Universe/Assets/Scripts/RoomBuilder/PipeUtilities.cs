using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ProjectUniverse.Environment.Gas
{
    /// <summary>
    /// Utility class for pipe-related operations and calculations
    /// </summary>
    public static class PipeUtilities
    {
        [Serializable]
        public class PipePathInfo
        {
            public List<GameObject> PipeObjects { get; set; } = new List<GameObject>();
            public float Diameter { get; set; }
            public bool IsVertical { get; set; }
        }

        public static Vector3 GetPipeOffsetForPlaceholder(GameObject placeholder, Vector3 direction)
        {
            // Get the bounds of the placeholder
            Bounds bounds = new Bounds();
            bool hasBounds = false;

            // Try to get bounds from renderer first
            if (placeholder.TryGetComponent<Renderer>(out var renderer))
            {
                bounds = renderer.bounds;
                hasBounds = true;
            }
            // Fallback to collider bounds
            else if (placeholder.TryGetComponent<Collider>(out var collider))
            {
                bounds = collider.bounds;
                hasBounds = true;
            }

            if (!hasBounds)
            {
                // If no bounds available, use transform scale as fallback
                Vector3 scale = placeholder.transform.localScale;
                bounds = new Bounds(placeholder.transform.position, scale);
            }

            // For horizontal pipes, we need to consider the placeholder's actual orientation
            // Get the placeholder's rotation to understand its local axes
            //Vector3 placeholderEuler = placeholder.transform.rotation.eulerAngles;

            // Calculate the maximum extent in the direction of the pipe
            Vector3 extents = bounds.size * 0.5f;
            Vector3 offset = Vector3.zero;

            // Normalize direction for comparison
            Vector3 normalizedDirection = direction.normalized;

            // Check if this is a vertical pipe (Y direction)
            if (Mathf.Abs(normalizedDirection.y) > 0.9f)
            {
                offset.y = extents.y * Mathf.Sign(normalizedDirection.y);
            }
            else
            {
                // Horizontal pipe - check X and Z directions
                if (Mathf.Abs(normalizedDirection.x) > 0.9f)
                {
                    // Pipe runs along X axis
                    offset.x = extents.x * Mathf.Sign(normalizedDirection.x);
                }
                else if (Mathf.Abs(normalizedDirection.z) > 0.9f)
                {
                    // Pipe runs along Z axis
                    offset.z = extents.z * Mathf.Sign(normalizedDirection.z);
                }
                else
                {
                    // Diagonal direction - use the dominant axis
                    if (Mathf.Abs(normalizedDirection.x) > Mathf.Abs(normalizedDirection.z))
                    {
                        offset.x = extents.x * Mathf.Sign(normalizedDirection.x);
                    }
                    else
                    {
                        offset.z = extents.z * Mathf.Sign(normalizedDirection.z);
                    }
                }
            }

            return offset;
        }

        public static float GetPipeLength(GameObject pipe)
        {
            // Try to determine pipe length from the prefab name
            string pipeName = pipe.name.Replace("(Clone)", "").Trim();

            if (pipeName.Contains("300mmPipe"))
            {
                return 3f;
            }
            else if (pipeName.Contains("Corner"))
            {
                return 1f; // Corners are typically 1m in effective length
            }
            else
            {
                return 1f; // Default to 1m for standard pipes
            }
        }

        public static bool IsCornerPlaceholder(GameObject pipePlaceholder)
        {
            Vector3 scale = pipePlaceholder.transform.localScale;
            return Mathf.Abs(scale.x - scale.y) < 0.01f &&
                   Mathf.Abs(scale.y - scale.z) < 0.01f &&
                   Mathf.Abs(scale.x - scale.z) < 0.01f;
        }

        public static bool IsVerticalPipe(GameObject pipe)
        {
            Vector3 rotation = pipe.transform.rotation.eulerAngles;
            // Check for vertical rotation (Z rotation of 90 or 270, or X rotation of 90 or 270)
            return (Mathf.Abs(rotation.z - 90f) < 5f || Mathf.Abs(rotation.z - 270f) < 5f) ||
                   (Mathf.Abs(rotation.x - 90f) < 5f || Mathf.Abs(rotation.x - 270f) < 5f);
        }

        public static bool IsVerticalPipeSegment(List<GameObject> segment)
        {
            if (segment.Count < 2) return false;

            // Get the first pipe's position
            Vector3 firstPos = segment[0].transform.position;

            // Check if all pipes in the segment are aligned vertically
            foreach (GameObject pipe in segment)
            {
                Vector3 pos = pipe.transform.position;
                // Allow small tolerance for x/z positions
                if (Mathf.Abs(pos.x - firstPos.x) > 0.1f || Mathf.Abs(pos.z - firstPos.z) > 0.1f)
                {
                    return false;
                }

                // Check if pipe is rotated for vertical orientation
                Vector3 rotation = pipe.transform.rotation.eulerAngles;
                if (!(Mathf.Abs(rotation.z - 90f) < 5f || Mathf.Abs(rotation.z - 270f) < 5f))
                {
                    return false;
                }
            }

            return true;
        }

        public static float GetPipeRotation(Vector3 direction)
        {
            // Normalize the direction
            direction = direction.normalized;

            // Calculate angle based on direction
            if (Mathf.Abs(direction.y) > 0.9f)
            {
                // Vertical pipe
                return 0f; // Rotation will be handled separately by setting z=90
            }
            else if (Mathf.Abs(direction.z) > Mathf.Abs(direction.x))
            {
                // Primarily Z direction
                if (direction.z > 0)
                    return 0f;   // Forward
                else
                    return 180f; // Backward
            }
            else
            {
                // Primarily X direction
                if (direction.x > 0)
                    return 90f;  // Right
                else
                    return 270f; // Left
            }
        }

        public static float GetCornerPipeRotation(Vector3 dir1, Vector3 dir2)
        {
            // Normalize directions
            dir1 = dir1.normalized;
            dir2 = dir2.normalized;

            // Handle vertical connections
            if (Mathf.Abs(dir1.y) > 0.9f || Mathf.Abs(dir2.y) > 0.9f)
            {
                // One direction is vertical, determine rotation based on the horizontal direction
                Vector3 horizontalDir = Mathf.Abs(dir1.y) > 0.9f ? dir2 : dir1;

                if (Mathf.Abs(horizontalDir.x) > 0.9f)
                {
                    return horizontalDir.x > 0 ? 90f : 270f;
                }
                else
                {
                    return horizontalDir.z > 0 ? 0f : 180f;
                }
            }

            // Both directions are horizontal
            if ((dir1.x > 0.9f && dir2.z > 0.9f) || (dir1.z > 0.9f && dir2.x > 0.9f))
            {
                return 0f;
            }
            else if ((dir1.x > 0.9f && dir2.z < -0.9f) || (dir1.z < -0.9f && dir2.x > 0.9f))
            {
                return 90f;
            }
            else if ((dir1.x < -0.9f && dir2.z < -0.9f) || (dir1.z < -0.9f && dir2.x < -0.9f))
            {
                return 180f;
            }
            else if ((dir1.x < -0.9f && dir2.z > 0.9f) || (dir1.z > 0.9f && dir2.x < -0.9f))
            {
                return 270f;
            }

            return 0f;
        }

        public static bool ArePipesConnected(GameObject pipe1, GameObject pipe2)
        {
            Vector3 connectionVector = pipe2.transform.position - pipe1.transform.position;
            float distance = connectionVector.magnitude;

            // Quick distance check before more complex checks
            if (distance > 1.5f) return false;

            bool isCorner1 = IsCornerPlaceholder(pipe1);
            bool isCorner2 = IsCornerPlaceholder(pipe2);

            // Fast path for corners - more permissive connections
            if (isCorner1 || isCorner2)
            {
                return distance < 1.5f && IsCardinalDirection(connectionVector);
            }

            // For straight pipes, be more restrictive
            if (distance > 1.1f) return false;

            if (!IsCardinalDirection(connectionVector)) return false;

            // Vertical connection check
            if (Mathf.Abs(connectionVector.normalized.y) > 0.9f)
            {
                return IsVerticalPipe(pipe1) && IsVerticalPipe(pipe2);
            }

            // Horizontal connection - don't connect vertical pipes horizontally
            return !IsVerticalPipe(pipe1) && !IsVerticalPipe(pipe2);
        }

        // Helper method to reduce redundancy
        private static bool IsCardinalDirection(Vector3 vector)
        {
            Vector3 normalized = vector.normalized;
            return Mathf.Abs(normalized.x) > 0.9f ||
                   Mathf.Abs(normalized.y) > 0.9f ||
                   Mathf.Abs(normalized.z) > 0.9f;
        }

        public static bool ShouldPipesBeInSameSegment(GameObject pipe1, GameObject pipe2, List<GameObject> currentSegment)
        {
            // Always allow corners to join segments
            if (IsCornerPlaceholder(pipe1) || IsCornerPlaceholder(pipe2))
            {
                return true;
            }

            // For small segments, allow connections
            if (currentSegment.Count <= 2)
            {
                return true;
            }

            // For larger segments, be more permissive to avoid breaking valid connections
            return true;
        }

        // Simplified group pipes method with early exits
        public static List<List<GameObject>> GroupPipesIntoSegments(List<GameObject> pipes)
        {
            if (pipes == null || pipes.Count == 0) return new List<List<GameObject>>();

            List<List<GameObject>> segments = new();
            HashSet<GameObject> processed = new(pipes.Count);

            foreach (GameObject pipe in pipes)
            {
                if (processed.Contains(pipe)) continue;

                List<GameObject> segment = new();
                Queue<GameObject> toProcess = new();
                toProcess.Enqueue(pipe);

                while (toProcess.Count > 0)
                {
                    GameObject current = toProcess.Dequeue();
                    if (processed.Contains(current)) continue;

                    processed.Add(current);
                    segment.Add(current);

                    // Use direct distance check for faster processing
                    foreach (GameObject other in pipes)
                    {
                        if (processed.Contains(other) || other == current) continue;

                        if (ArePipesConnected(current, other))
                        {
                            toProcess.Enqueue(other);
                        }
                    }
                }

                if (segment.Count > 0) segments.Add(segment);

                // Early exit if all pipes are processed
                if (processed.Count == pipes.Count) break;
            }

            return segments;
        }


        public static int GetPipeConnectionCount(GameObject pipe, List<GameObject> segment)
        {
            int count = 0;
            Vector3 pipePosition = pipe.transform.position;

            // Check in all six cardinal directions
            Vector3[] directions = { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back };

            foreach (Vector3 dir in directions)
            {
                Vector3 checkPosition = pipePosition + dir;

                foreach (GameObject other in segment)
                {
                    if (other == pipe) continue;

                    if (Vector3.Distance(other.transform.position, checkPosition) < 0.3f)
                    {
                        count++;
                        break;
                    }
                }
            }

            return count;
        }

        public static bool IsPipeCorner(GameObject pipe, List<GameObject> segment)
        {
            // Check if this is a corner placeholder (all scales are equal)
            Vector3 scale = pipe.transform.localScale;
            bool isCornerPlaceholder = Mathf.Abs(scale.x - scale.y) < 0.01f &&
                                       Mathf.Abs(scale.y - scale.z) < 0.01f &&
                                       Mathf.Abs(scale.x - scale.z) < 0.01f &&
                                       scale.x > 0.1f; // Make sure it's not just a tiny cube

            if (isCornerPlaceholder)
                return true;

            // Original corner detection logic as fallback
            Vector3 pipePosition = pipe.transform.position;

            // Find all connected pipes within 0.8f distance
            List<GameObject> nearbyPipes = new();
            List<Vector3> connectedDirections = new();

            foreach (GameObject other in segment)
            {
                if (other == pipe) continue;

                Vector3 connectionVector = other.transform.position - pipePosition;
                float distance = connectionVector.magnitude;

                // Only consider pipes within 0.8f for corner connections
                if (distance < 0.8f)
                {
                    nearbyPipes.Add(other);
                    connectedDirections.Add(connectionVector.normalized);
                }
            }

            // A corner must have exactly two connections
            if (nearbyPipes.Count != 2) return false;

            // For a corner, the connected directions must be perpendicular
            float dot = Vector3.Dot(connectedDirections[0], connectedDirections[1]);
            return Mathf.Abs(dot) < 0.1f; // Should be 0 for perpendicular
        }

        public static List<GameObject> BuildPipePath(List<GameObject> segment, List<GameObject> endPoints, List<GameObject> corners)
        {
            List<GameObject> path = new();

            if (endPoints.Count > 0)
            {
                // Start from an endpoint
                GameObject current = endPoints[0];
                HashSet<GameObject> visited = new();

                path.Add(current);
                visited.Add(current);

                while (true)
                {
                    GameObject next = GetNextPipeInPath(current, segment, visited);
                    if (next == null) break;

                    path.Add(next);
                    visited.Add(next);
                    current = next;
                }
            }

            return path;
        }

        public static GameObject GetNextPipeInPath(GameObject current, List<GameObject> segment, HashSet<GameObject> visited)
        {
            Vector3[] directions = { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back };

            foreach (Vector3 dir in directions)
            {
                Vector3 checkPosition = current.transform.position + dir;

                foreach (GameObject pipe in segment)
                {
                    if (!visited.Contains(pipe) &&
                        Vector3.Distance(pipe.transform.position, checkPosition) < 0.3f)
                    {
                        return pipe;
                    }
                }
            }

            return null;
        }

        public static List<GameObject> OrderPipeSegment(List<GameObject> segment, GameObject startPipe)
        {
            List<GameObject> ordered = new();
            HashSet<GameObject> visited = new();
            GameObject current = startPipe;

            while (current != null && !visited.Contains(current))
            {
                ordered.Add(current);
                visited.Add(current);

                // Find next connected pipe
                GameObject next = null;
                Vector3[] directions = { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back };

                foreach (Vector3 dir in directions)
                {
                    Vector3 checkPosition = current.transform.position + dir;

                    foreach (GameObject pipe in segment)
                    {
                        if (!visited.Contains(pipe) &&
                            Vector3.Distance(pipe.transform.position, checkPosition) < 0.3f)
                        {
                            next = pipe;
                            break;
                        }
                    }

                    if (next != null) break;
                }

                current = next;
            }

            return ordered;
        }
    }
}
