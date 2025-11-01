using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace ProjectUniverse.PowerSystem
{
    /// <summary>
    /// Represents a cable connection through waypoint paths between power system components
    /// </summary>
    public class PathCable : ICable
    {
        private List<WaypointPath> paths;
        private List<Template> templates;
        private PowerNode startNode;
        private PowerNode endNode;
        private bool isActive = true;

        // Efficient segment health/heat storage
        private CableSegmentData[] segmentData;
        private int totalSegments;
        private float lastHealthCheck = -1f; // Cache for overall health
        private bool lastOperationalCheck = true; // Cache for operational status

        // Health/Heat configuration based on cable size
        private readonly Dictionary<CableSize, (float maxHealth, float maxHeat)> cableSizeConfigs = new()
        {
            { CableSize.Transmission, (500f, 288000f) }, // 500mm cable
            { CableSize.Distribution, (250f, 72000f) },  // 250mm cable
            { CableSize.Branch, (150f, 24000f) }         // 150mm cable
        };

        // Single path constructors (maintain compatibility)
        public PathCable(IGenerator generator, IRouter router, WaypointPath path, Template template) : base(generator, router)
        {
            this.paths = new List<WaypointPath> { path };
            this.templates = new List<Template> { template };
            InitializeSegmentData();
        }

        public PathCable(IRouter router, IRoutingSubstation substation, WaypointPath path, Template template) : base(router, substation)
        {
            this.paths = new List<WaypointPath> { path };
            this.templates = new List<Template> { template };
            InitializeSegmentData();
        }

        public PathCable(IRoutingSubstation substation, IMachine machine, WaypointPath path, Template template) : base(substation, machine)
        {
            this.paths = new List<WaypointPath> { path };
            this.templates = new List<Template> { template };
            InitializeSegmentData();
        }

        public PathCable(IRoutingSubstation substation, IBreakerBox breakerBox, WaypointPath path, Template template) : base(substation, breakerBox)
        {
            this.paths = new List<WaypointPath> { path };
            this.templates = new List<Template> { template };
            InitializeSegmentData();
        }

        public PathCable(IBreakerBox breakerBox, ISubMachine subMachine, WaypointPath path, Template template) : base(breakerBox, subMachine)
        {
            this.paths = new List<WaypointPath> { path };
            this.templates = new List<Template> { template };
            InitializeSegmentData();
        }

        // Multi-path constructors
        public PathCable(IGenerator generator, IRouter router, List<GlobalRouteResolver.PathInfo> pathSequence) : base(generator, router)
        {
            InitializeFromPathSequence(pathSequence);
            InitializeSegmentData();
        }

        public PathCable(IRouter router, IRoutingSubstation substation, List<GlobalRouteResolver.PathInfo> pathSequence) : base(router, substation)
        {
            InitializeFromPathSequence(pathSequence);
            InitializeSegmentData();
        }

        public PathCable(IRoutingSubstation substation, IMachine machine, List<GlobalRouteResolver.PathInfo> pathSequence) : base(substation, machine)
        {
            InitializeFromPathSequence(pathSequence);
            InitializeSegmentData();
        }

        public PathCable(IRoutingSubstation substation, IBreakerBox breakerBox, List<GlobalRouteResolver.PathInfo> pathSequence) : base(substation, breakerBox)
        {
            InitializeFromPathSequence(pathSequence);
            InitializeSegmentData();
        }

        public PathCable(IBreakerBox breakerBox, ISubMachine subMachine, List<GlobalRouteResolver.PathInfo> pathSequence) : base(breakerBox, subMachine)
        {
            InitializeFromPathSequence(pathSequence);
            InitializeSegmentData();
        }

        private void InitializeFromPathSequence(List<GlobalRouteResolver.PathInfo> pathSequence)
        {
            paths = new List<WaypointPath>();
            templates = new List<Template>();

            foreach (var pathInfo in pathSequence)
            {
                paths.Add(pathInfo.path);
                templates.Add(pathInfo.template);
            }
        }


        private void InitializeSegmentData()
        {
            // Calculate total segments needed
            totalSegments = 0;
            for (int i = 0; i < paths.Count; i++)
            {
                var positions = paths[i].GetPathPositions();
                totalSegments += positions.Count - 1; // segments between positions
            }

            // Initialize segment data array
            segmentData = new CableSegmentData[totalSegments];

            // Get cable size config
            CableSize cableSize = GetCableSize();
            var config = cableSizeConfigs[cableSize];

            // Initialize each segment, checking for saved data
            int segmentIndex = 0;
            var pathManager = PowerSystemPathManager.Instance;

            for (int pathIdx = 0; pathIdx < paths.Count; pathIdx++)
            {
                var path = paths[pathIdx];
                var template = templates[pathIdx];
                var positions = path.GetPathPositions();

                for (int i = 0; i < positions.Count - 1; i++)
                {
                    if (pathManager != null)
                    {
                        // Try to get saved data for this specific segment
                        segmentData[segmentIndex] = pathManager.GetSavedSegmentData(
                            template.templateId,
                            path.pathId,
                            i,
                            config.maxHealth,
                            config.maxHeat
                        );
                    }
                    else
                    {
                        // No path manager, create new segment
                        segmentData[segmentIndex] = new CableSegmentData(config.maxHealth, config.maxHeat);
                    }

                    segmentIndex++;
                }
            }
        }

        // Add method to get all segment data
        public CableSegmentData[] GetAllSegmentData()
        {
            return (CableSegmentData[])segmentData.Clone();
        }

        public void SaveAllSegments()
        {
            var pathManager = PowerSystemPathManager.Instance;
            if (pathManager == null) return;

            int segmentIndex = 0;

            for (int pathIdx = 0; pathIdx < paths.Count; pathIdx++)
            {
                var path = paths[pathIdx];
                var template = templates[pathIdx];
                var positions = path.GetPathPositions();

                for (int i = 0; i < positions.Count - 1; i++)
                {
                    pathManager.SaveSegmentData(
                        template.templateId,
                        path.pathId,
                        i,
                        segmentData[segmentIndex]
                    );

                    segmentIndex++;
                }
            }
        }

        private void InitializeSegmentData_()
        {
            // Calculate total segments needed
            totalSegments = 0;
            for (int i = 0; i < paths.Count; i++)
            {
                var positions = paths[i].GetPathPositions();
                totalSegments += positions.Count - 1; // segments between positions
            }

            // Initialize segment data array
            segmentData = new CableSegmentData[totalSegments];

            // Get cable size config
            CableSize cableSize = GetCableSize();
            var config = cableSizeConfigs[cableSize];

            // Initialize each segment
            for (int i = 0; i < totalSegments; i++)
            {
                segmentData[i] = new CableSegmentData(config.maxHealth, config.maxHeat);
            }
        }

        public CableSize GetCableSize()
        {
            // Determine cable size based on connection type
            if (gen != null && route != null) return CableSize.Transmission;
            if (route != null && subst != null) return CableSize.Distribution;
            return CableSize.Branch;
        }

        // Override TransferIn to handle heat generation
        public new void TransferIn(int legCount, float[] powerinPerLeg, int type)
        {
            base.TransferIn(legCount, powerinPerLeg, type);

            // Save segments if any were modified due to heat
            if (Time.frameCount % 60 == 0) // Save every 60 frames to avoid performance issues
            {
                SaveAllSegments();
            }
        }

        public float GetOverallHealth()
        {
            float totalHealth = 0f;
            for (int i = 0; i < totalSegments; i++)
            {
                totalHealth += segmentData[i].GetHealthPercentage();
            }
            return totalHealth / totalSegments;
        }

        public float GetMaxHeat()
        {
            float maxHeat = 0f;
            for (int i = 0; i < totalSegments; i++)
            {
                float heat = segmentData[i].GetHeatPercentage();
                if (heat > maxHeat) maxHeat = heat;
            }
            return maxHeat;
        }

        public void DamageSegment(int segmentIndex, float damage)
        {
            if (segmentIndex >= 0 && segmentIndex < totalSegments)
            {
                segmentData[segmentIndex].ApplyDamage(damage);
                lastHealthCheck = -1f; // Invalidate cache
            }
            if (Time.frameCount % 60 == 0) // Save every 60 frames to avoid performance issues
            {
                SaveAllSegments();
            }
        }

        public void DamageSegmentAtPosition(Vector3 worldPosition, float damage, float radius)
        {
            int segmentIndex = 0;

            for (int pathIdx = 0; pathIdx < paths.Count; pathIdx++)
            {
                var positions = paths[pathIdx].GetPathPositions();
                var template = templates[pathIdx];

                for (int i = 0; i < positions.Count - 1; i++)
                {
                    Vector3 startWorld = template.transform.TransformPoint(positions[i]);
                    Vector3 endWorld = template.transform.TransformPoint(positions[i + 1]);

                    // Check if segment is within damage radius
                    float distToSegment = DistanceToLineSegment(worldPosition, startWorld, endWorld);
                    if (distToSegment <= radius)
                    {
                        segmentData[segmentIndex].ApplyDamage(damage);
                    }

                    segmentIndex++;
                }
            }

            lastHealthCheck = -1f; // Invalidate cache
            if (Time.frameCount % 60 == 0) // Save every 60 frames to avoid performance issues
            {
                SaveAllSegments();
            }
        }

        private float DistanceToLineSegment(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
        {
            Vector3 line = lineEnd - lineStart;
            float len = line.magnitude;
            line.Normalize();

            Vector3 v = point - lineStart;
            float d = Vector3.Dot(v, line);
            d = Mathf.Clamp(d, 0f, len);

            return Vector3.Distance(point, lineStart + line * d);
        }

        public CableSegmentData GetSegmentData(int segmentIndex)
        {
            if (segmentIndex >= 0 && segmentIndex < totalSegments)
            {
                return segmentData[segmentIndex];
            }
            return new CableSegmentData(0, 0);
        }

        public int GetTotalSegments() => totalSegments;

        // Get specific segment info for visualization
        public void GetSegmentWorldPositions(int segmentIndex, out Vector3 start, out Vector3 end)
        {
            start = Vector3.zero;
            end = Vector3.zero;

            if (segmentIndex < 0 || segmentIndex >= totalSegments) return;

            int currentSegment = 0;

            for (int pathIdx = 0; pathIdx < paths.Count; pathIdx++)
            {
                var positions = paths[pathIdx].GetPathPositions();
                var template = templates[pathIdx];

                for (int i = 0; i < positions.Count - 1; i++)
                {
                    if (currentSegment == segmentIndex)
                    {
                        start = template.transform.TransformPoint(positions[i]);
                        end = template.transform.TransformPoint(positions[i + 1]);
                        return;
                    }
                    currentSegment++;
                }
            }
        }
        
        public WaypointPath GetPath() => paths?.FirstOrDefault();
        public List<WaypointPath> GetPaths() => paths;
        public Template GetTemplate() => templates?.FirstOrDefault();
        public List<Template> GetTemplates() => templates;
        public void SetActive(bool active) => isActive = active;
        public bool IsActive() => isActive;

        public void SetPowerNodes(PowerNode start, PowerNode end)
        {
            startNode = start;
            endNode = end;
        }

        public PowerNode GetStartNode() => startNode;
        public PowerNode GetEndNode() => endNode;

        public float GetTotalLength()
        {
            float totalLength = 0;
            for (int i = 0; i < paths.Count; i++)
            {
                var positions = paths[i].GetPathPositions();
                var template = templates[i];

                for (int j = 0; j < positions.Count - 1; j++)
                {
                    Vector3 worldPosA = template.transform.TransformPoint(positions[j]);
                    Vector3 worldPosB = template.transform.TransformPoint(positions[j + 1]);
                    totalLength += Vector3.Distance(worldPosA, worldPosB);
                }
            }
            return totalLength;
        }
        
        // Health and heat management methods
        public bool IsOperational()
        {
            // Check cached value first for efficiency
            if (Time.time - lastHealthCheck < 0.1f) // Cache for 0.1 seconds
            {
                return lastOperationalCheck;
            }

            lastHealthCheck = Time.time;

            // Check if any segment is broken
            for (int i = 0; i < totalSegments; i++)
            {
                if (!segmentData[i].IsOperational())
                {
                    lastOperationalCheck = false;
                    return false;
                }
            }

            lastOperationalCheck = true;
            return true;
        }
    }
}