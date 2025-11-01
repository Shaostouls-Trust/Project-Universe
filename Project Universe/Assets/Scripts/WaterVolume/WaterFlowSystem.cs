using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine;
using ProjectUniverse.Environment.Fluids;
using System.Linq;

public class WaterFlowSystem : MonoBehaviour
{
    [Header("Flow Settings")]
    private float flowRate = 1f; // Base flow rate - Don't change, or water may not equalize. Has a root effect on flow rate.
    [SerializeField] private float minFlowRate = 0.002f; // Min flow rate so that every volume will equalize faster on the low end.
    public float gravityMultiplier = 9.81f;
    public float minWaterLevel = 0.001f; // Minimum water level before considering empty
    public int maxIterationsPerFrame = 1; // Prevent infinite loops
    public float defaultWaterTemperature = 70f; // Default water temperature in Celsius

    [Header("Debug")]
    public bool enableDebugLogs = false;

    private HashSet<VolumeWaterData> allWaterVolumes;
    private HashSet<Opening> allOpenings;
    private Dictionary<VolumeWaterData, List<Opening>> volumeConnections;
    //private Dictionary<VolumeWaterData, IFluid> volumeFluidMap;

    private void Awake()
    {
        //volumeFluidMap = new Dictionary<VolumeWaterData, IFluid>();
        InitializeSystem();
    }

    void Start()
    {
       //InitializeSystem();
    }

    void InitializeSystem()
    {
        // Find all water volumes and openings
        allWaterVolumes = new HashSet<VolumeWaterData>(FindObjectsByType<VolumeWaterData>(FindObjectsSortMode.None));
        allOpenings = new HashSet<Opening>(FindObjectsByType<Opening>(FindObjectsSortMode.None));

        // Initialize fluid for each volume
        //foreach (var volume in allWaterVolumes)
        //{
        //    // Create fluid instance for this volume with initial water volume
        //    IFluid waterFluid = new IFluid("Water", defaultWaterTemperature, volume.GetWaterVolume() / 1000f, 1.0f);
        //    volumeFluidMap[volume] = waterFluid;
        //}

        BuildConnectionMap();

        if (enableDebugLogs)
        {
            Debug.Log($"Water Flow System initialized with {allWaterVolumes.Count} volumes and {allOpenings.Count} openings");
        }
    }

    void BuildConnectionMap()
    {
        volumeConnections = new Dictionary<VolumeWaterData, List<Opening>>();

        foreach (var volume in allWaterVolumes)
        {
            volumeConnections[volume] = new List<Opening>();
        }

        foreach (var opening in allOpenings)
        {
            var (vol1, vol2) = opening.GetConnectedVolumes();
            if (vol1 != null && volumeConnections.ContainsKey(vol1))
            {
                volumeConnections[vol1].Add(opening);
            }
            if (vol2 != null && volumeConnections.ContainsKey(vol2))
            {
                volumeConnections[vol2].Add(opening);
            }
        }
    }

    void Update()
    {
        ProcessWaterFlow();
    }

    // Add this public method to register new openings
    public void RegisterNewOpening(Opening newOpening)
    {
        if (!allOpenings.Contains(newOpening))
        {
            allOpenings.Add(newOpening);
            BuildConnectionMap(); // Rebuild connections
        }
    }


    void ProcessWaterFlow()
    {
        int iterations = 0;
        bool waterMoved = true;

        while (waterMoved && iterations < maxIterationsPerFrame)
        {
            waterMoved = false;
            iterations++;

            foreach (var opening in allOpenings)
            {
                if (opening.CanWaterFlow())
                {
                    if (ProcessOpeningFlow(opening))
                    {
                        waterMoved = true;
                    }
                }
            }

            // Process direct connections last
            foreach (VolumeWaterData volume in allWaterVolumes)
            {
                if (volume.connectedVolumes.Count > 0)
                {
                    foreach (VolumeWaterData innerVol in volume.connectedVolumes)
                    {
                        if (ProcessDirectFlow(volume, innerVol))
                        {
                            waterMoved = true;
                        }
                    }
                }
            }
        }
    }

    // Add this method to WaterFlowSystem class
    bool ProcessOpeningFlow(Opening opening)
    {
        var (volume1, volume2) = opening.GetConnectedVolumes();

        if (volume1 == null || volume2 == null)
            return false;

        // Handle thick holes with special logic
        Hole hole = opening as Hole;
        if (hole != null && hole.isThick)
        {
            if (hole.isHorizontal)
            {
                // Horizontal thick hole (through a wall)
                return ProcessHorizontalThickHoleFlow(volume1, volume2, hole);
            }
            else
            {
                // Vertical thick hole (through floor/ceiling)
                return ProcessVerticalThickHoleFlow(volume1, volume2, hole);
            }
        }

        // Standard opening flow for doors and regular holes
        return ProcessStandardOpeningFlow(volume1, volume2, opening);
    }

    // NEW: Separated standard opening flow logic
    private bool ProcessStandardOpeningFlow(VolumeWaterData volume1, VolumeWaterData volume2, Opening opening)
    {
        float waterHeight1 = volume1.GetAbsoluteWaterHeight();
        float waterHeight2 = volume2.GetAbsoluteWaterHeight();

        Bounds bounds1 = volume1.GetComponent<BoxCollider>().bounds;
        Bounds bounds2 = volume2.GetComponent<BoxCollider>().bounds;

        // Check if this is a vertical connection (floor/ceiling opening)
        bool isVerticalConnection = Mathf.Abs(bounds1.center.y - bounds2.center.y) >
                                    Mathf.Max(bounds1.extents.y, bounds2.extents.y);

        if (isVerticalConnection)
        {
            // Determine upper and lower volumes
            VolumeWaterData upperVolume = bounds1.center.y > bounds2.center.y ? volume1 : volume2;
            VolumeWaterData lowerVolume = bounds1.center.y > bounds2.center.y ? volume2 : volume1;

            return ProcessVerticalOpeningFlow(lowerVolume, upperVolume, opening);
        }

        // Horizontal connection (wall opening/door) - check if volumes are at compatible elevations
        if (!CanVolumesConnect_Opening(volume1, volume2, opening))
            return false;

        float openingBottom = opening.GetBottomElevation();

        // Determine flow direction and volumes
        VolumeWaterData sourceVolume, targetVolume;
        float sourceHeight, targetHeight;

        if (waterHeight1 > waterHeight2)
        {
            sourceVolume = volume1;
            targetVolume = volume2;
            sourceHeight = waterHeight1;
            targetHeight = waterHeight2;
        }
        else
        {
            sourceVolume = volume2;
            targetVolume = volume1;
            sourceHeight = waterHeight2;
            targetHeight = waterHeight1;
        }

        // Check if water can flow through opening
        if (sourceHeight <= openingBottom || !sourceVolume.HasWater)
            return false;

        // Get fluid data for source and target
        IFluid sourceFluid = sourceVolume.Fluid;
        IFluid targetFluid = targetVolume.Fluid;

        // Calculate effective flow height
        float effectiveFlowHeight = Mathf.Min(sourceHeight - openingBottom, opening.height);
        if (effectiveFlowHeight <= 0)
            return false;

        // Calculate flow rate using simplified fluid dynamics
        float heightDifference = sourceHeight - Mathf.Max(targetHeight, openingBottom);
        float flowVelocity = Mathf.Sqrt(2f * gravityMultiplier * heightDifference);
        float flowArea = opening.width * effectiveFlowHeight;
        float volumeFlowRate = flowVelocity * flowArea * opening.flowCoefficient;

        // Apply time-based flow
        float deltaVolume = volumeFlowRate * Time.deltaTime * flowRate;

        // Limit flow to available water and target capacity
        float availableWater = sourceVolume.GetWaterVolume();
        float targetCapacity = targetVolume.MaxWaterCapacity - targetVolume.GetWaterVolume();

        deltaVolume = Mathf.Min(deltaVolume, availableWater, targetCapacity);

        if (deltaVolume < minFlowRate)
        {
            deltaVolume = minFlowRate;
        }

        // Equilibrium check - stop flow when levels would be equal
        float finalSourceVolume = sourceVolume.GetWaterVolume() - deltaVolume;
        float finalTargetVolume = targetVolume.GetWaterVolume() + deltaVolume;

        float finalSourceHeight = sourceVolume.VolumeFloorHeight + (finalSourceVolume / sourceVolume.CrossSectionalArea);
        float finalTargetHeight = targetVolume.VolumeFloorHeight + (finalTargetVolume / targetVolume.CrossSectionalArea);

        // If flow would overshoot equilibrium, adjust to reach exact equilibrium
        if ((sourceHeight > targetHeight && finalSourceHeight < finalTargetHeight) ||
            Mathf.Abs(finalSourceHeight - finalTargetHeight) < 0.01f)
        {
            // Calculate exact equilibrium
            float totalVolume = sourceVolume.GetWaterVolume() + targetVolume.GetWaterVolume();
            float totalArea = sourceVolume.CrossSectionalArea + targetVolume.CrossSectionalArea;

            // Account for different floor heights
            float avgFloorHeight = (sourceVolume.VolumeFloorHeight * sourceVolume.CrossSectionalArea +
                                    targetVolume.VolumeFloorHeight * targetVolume.CrossSectionalArea) / totalArea;

            float equilibriumHeight = avgFloorHeight + (totalVolume / totalArea);

            float newSourceVolume = Mathf.Max(0, (equilibriumHeight - sourceVolume.VolumeFloorHeight) * sourceVolume.CrossSectionalArea);
            float newTargetVolume = Mathf.Max(0, (equilibriumHeight - targetVolume.VolumeFloorHeight) * targetVolume.CrossSectionalArea);

            sourceVolume.SetWaterVolume(newSourceVolume);
            targetVolume.SetWaterVolume(newTargetVolume);
        }
        else if (deltaVolume > minWaterLevel)
        {
            // Calculate temperature mixing when transferring water
            float sourceTemp = sourceFluid.GetTemp();
            float sourceVolumeBefore = sourceFluid.GetConcentration();
            float targetTemp = 0f;
            float targetVolumeBefore = 0f;

            if (targetFluid != null)
            {
                targetTemp = targetFluid.GetTemp();
                targetVolumeBefore = targetFluid.GetConcentration();
            }

            // Update volumes
            sourceVolume.SetWaterVolume(sourceVolume.GetWaterVolume() - deltaVolume);
            targetVolume.SetWaterVolume(targetVolume.GetWaterVolume() + deltaVolume);

            // Calculate mixed temperature in target
            if (targetVolumeBefore + deltaVolume > minWaterLevel)
            {
                float mixedTemp = ((targetVolumeBefore * targetTemp) + (deltaVolume * sourceTemp)) /
                                    (targetVolumeBefore + deltaVolume);
                targetVolume.Fluid.SetTemp(mixedTemp);
            }
        }

        if (enableDebugLogs && deltaVolume > minWaterLevel)
        {
            Debug.Log($"Water flowed: {deltaVolume:F3} from {sourceVolume.name} ({sourceFluid.GetTemp():F1}°C) to {targetVolume.name} ({targetFluid?.GetTemp():F1}°C)");
        }

        return deltaVolume > minWaterLevel;
    }

    // NEW: Handle vertical connections (floor/ceiling openings)
    private bool ProcessVerticalOpeningFlow(VolumeWaterData lowerVolume, VolumeWaterData upperVolume, Opening opening)
    {
        if (lowerVolume == null || upperVolume == null)
            return false;

        float lowerWaterHeight = lowerVolume.GetAbsoluteWaterHeight();
        float upperWaterHeight = upperVolume.GetAbsoluteWaterHeight();
        float openingElevation = opening.GetBottomElevation();

        // Determine flow direction
        bool flowUpward = lowerWaterHeight > upperWaterHeight;

        if (flowUpward)
        {
            // Water flows upward - lower volume must be full enough to reach the opening
            if (lowerWaterHeight <= openingElevation || !lowerVolume.HasWater)
                return false;

            // Lower volume must be at or near capacity to push water up
            float lowerFillRatio = lowerVolume.GetWaterVolume() / lowerVolume.MaxWaterCapacity;
            if (lowerFillRatio < 0.95f) // Must be at least 95% full to push water up
                return false;

            // Check if upper volume can receive water
            if (upperVolume.GetWaterVolume() >= upperVolume.MaxWaterCapacity)
                return false;
        }
        else
        {
            // Water flows downward - upper volume must have water above the opening
            if (upperWaterHeight <= openingElevation || !upperVolume.HasWater)
                return false;

            // Check if lower volume can receive water
            if (lowerVolume.GetWaterVolume() >= lowerVolume.MaxWaterCapacity)
                return false;
        }

        VolumeWaterData sourceVolume = flowUpward ? lowerVolume : upperVolume;
        VolumeWaterData targetVolume = flowUpward ? upperVolume : lowerVolume;
        float sourceHeight = flowUpward ? lowerWaterHeight : upperWaterHeight;
        float targetHeight = flowUpward ? upperWaterHeight : lowerWaterHeight;

        IFluid sourceFluid = sourceVolume.Fluid;
        IFluid targetFluid = targetVolume.Fluid;

        // Calculate effective flow
        float effectiveFlowHeight = Mathf.Min(sourceHeight - openingElevation, opening.height);
        if (effectiveFlowHeight <= 0)
            return false;

        float heightDifference = Mathf.Abs(sourceHeight - targetHeight);
        float flowVelocity = Mathf.Sqrt(2f * gravityMultiplier * heightDifference);
        float flowArea = opening.width * effectiveFlowHeight;

        // Reduce flow rate for upward flow
        float flowMultiplier = flowUpward ? 0.3f : 1.0f;
        float volumeFlowRate = flowVelocity * flowArea * opening.flowCoefficient * flowMultiplier;

        float deltaVolume = volumeFlowRate * Time.deltaTime * flowRate;

        // Limit flow
        float availableWater = sourceVolume.GetWaterVolume();
        float targetCapacity = targetVolume.MaxWaterCapacity - targetVolume.GetWaterVolume();
        deltaVolume = Mathf.Min(deltaVolume, availableWater, targetCapacity);

        if (deltaVolume < minFlowRate && availableWater > minFlowRate)
        {
            deltaVolume = minFlowRate;
        }

        if (deltaVolume > minWaterLevel)
        {
            // Transfer water with temperature mixing
            float sourceTemp = sourceFluid.GetTemp();
            float targetTemp = targetFluid?.GetTemp() ?? defaultWaterTemperature;
            float targetVolumeBefore = targetFluid?.GetConcentration() ?? 0f;

            sourceVolume.SetWaterVolume(sourceVolume.GetWaterVolume() - deltaVolume);
            targetVolume.SetWaterVolume(targetVolume.GetWaterVolume() + deltaVolume);

            if (targetVolumeBefore + deltaVolume > minWaterLevel)
            {
                float mixedTemp = ((targetVolumeBefore * targetTemp) + (deltaVolume * sourceTemp)) /
                                    (targetVolumeBefore + deltaVolume);
                targetVolume.Fluid.SetTemp(mixedTemp);
            }

            if (enableDebugLogs)
            {
                string direction = flowUpward ? "upward" : "downward";
                Debug.Log($"Vertical opening flow ({direction}): {deltaVolume:F3}L from {sourceVolume.name} to {targetVolume.name}");
            }

            return true;
        }

        return false;
    }


    // NEW: Handle horizontal thick holes (through walls)
    private bool ProcessHorizontalThickHoleFlow(VolumeWaterData volume1, VolumeWaterData volume2, Hole hole)
    {
        if (volume1 == null || volume2 == null)
            return false;

        float waterHeight1 = volume1.GetAbsoluteWaterHeight();
        float waterHeight2 = volume2.GetAbsoluteWaterHeight();

        // For horizontal holes, water flows if it's above the hole's bottom
        float holeBottomElevation = hole.GetBottomElevation();

        // Determine source and target based on water height
        VolumeWaterData sourceVolume = waterHeight1 > waterHeight2 ? volume1 : volume2;
        VolumeWaterData targetVolume = waterHeight1 > waterHeight2 ? volume2 : volume1;
        float sourceHeight = Mathf.Max(waterHeight1, waterHeight2);
        float targetHeight = Mathf.Min(waterHeight1, waterHeight2);

        // Check if source has water above the hole bottom
        if (sourceHeight <= holeBottomElevation || !sourceVolume.HasWater)
            return false;

        // Check if target can receive water
        if (targetVolume.GetWaterVolume() >= targetVolume.MaxWaterCapacity)
            return false;

        IFluid sourceFluid = sourceVolume.Fluid;
        IFluid targetFluid = targetVolume.Fluid;

        // FIXED: For horizontal holes, effective flow height is just how much water is above the hole
        float effectiveFlowHeight = sourceHeight - holeBottomElevation;

        // Limit by the hole's actual dimension (width/diameter)
        effectiveFlowHeight = Mathf.Min(effectiveFlowHeight, hole.width);

        if (effectiveFlowHeight <= 0)
            return false;

        // Calculate flow through horizontal opening
        float heightDifference = sourceHeight - Mathf.Max(targetHeight, holeBottomElevation);
        float flowVelocity = Mathf.Sqrt(2f * gravityMultiplier * heightDifference);

        // Flow area for circular hole
        float flowArea = Mathf.PI * Mathf.Pow(hole.width / 2f, 2f);
        float volumeFlowRate = flowVelocity * flowArea * hole.flowCoefficient;

        // Apply time-based flow
        float deltaVolume = volumeFlowRate * Time.deltaTime * flowRate;

        // Limit flow
        float availableWater = sourceVolume.GetWaterVolume();
        float targetCapacity = targetVolume.MaxWaterCapacity - targetVolume.GetWaterVolume();
        deltaVolume = Mathf.Min(deltaVolume, availableWater, targetCapacity);

        if (deltaVolume < minFlowRate && availableWater > minFlowRate)
        {
            deltaVolume = minFlowRate;
        }

        if (deltaVolume > minWaterLevel)
        {
            // Transfer water with temperature mixing
            float sourceTemp = sourceFluid.GetTemp();
            float targetTemp = targetFluid?.GetTemp() ?? defaultWaterTemperature;
            float targetVolumeBefore = targetFluid?.GetConcentration() ?? 0f;

            sourceVolume.SetWaterVolume(sourceVolume.GetWaterVolume() - deltaVolume);
            targetVolume.SetWaterVolume(targetVolume.GetWaterVolume() + deltaVolume);

            if (targetVolumeBefore + deltaVolume > minWaterLevel)
            {
                float mixedTemp = ((targetVolumeBefore * targetTemp) + (deltaVolume * sourceTemp)) /
                                    (targetVolumeBefore + deltaVolume);
                targetVolume.Fluid.SetTemp(mixedTemp);
            }

            if (enableDebugLogs)
            {
                Debug.Log($"Horizontal thick hole flow: {deltaVolume:F3}L from {sourceVolume.name} to {targetVolume.name}");
            }

            return true;
        }

        return false;
    }

    // NEW: Handle ceiling openings (water flowing upward when pushed by pressure)
    private bool ProcessCeilingOpeningFlow(VolumeWaterData volume1, VolumeWaterData volume2, Opening opening)
    {
        if (volume1 == null || volume2 == null)
            return false;

        Bounds bounds1 = volume1.GetComponent<BoxCollider>().bounds;
        Bounds bounds2 = volume2.GetComponent<BoxCollider>().bounds;

        // Determine which volume is below (source) and which is above (target)
        VolumeWaterData lowerVolume = bounds1.center.y < bounds2.center.y ? volume1 : volume2;
        VolumeWaterData upperVolume = bounds1.center.y < bounds2.center.y ? volume2 : volume1;

        float lowerWaterHeight = lowerVolume.GetAbsoluteWaterHeight();
        float upperWaterHeight = upperVolume.GetAbsoluteWaterHeight();
        float ceilingElevation = opening.GetBottomElevation();

        // Water can only flow up through ceiling if lower volume water reaches the ceiling
        if (lowerWaterHeight <= ceilingElevation || !lowerVolume.HasWater)
            return false;

        // Check if upper volume can receive water
        if (upperVolume.GetWaterVolume() >= upperVolume.MaxWaterCapacity)
            return false;

        IFluid sourceFluid = lowerVolume.Fluid;
        IFluid targetFluid = upperVolume.Fluid;

        // Calculate pressure head - water must have enough pressure to push up
        float pressureHead = lowerWaterHeight - ceilingElevation;

        // Only flow if there's significant pressure (water significantly above ceiling)
        if (pressureHead < 0.01f)
            return false;

        // Calculate effective flow area (water above ceiling level)
        float effectiveFlowHeight = Mathf.Min(pressureHead, opening.height);
        float flowArea = opening.width * effectiveFlowHeight;

        // Flow velocity based on pressure difference
        // Account for the fact that water must overcome gravity to flow up
        float heightDifference = lowerWaterHeight - upperWaterHeight;

        // Only flow upward if lower water is higher than upper water
        if (heightDifference <= 0)
            return false;

        float flowVelocity = Mathf.Sqrt(2f * gravityMultiplier * heightDifference);
        float volumeFlowRate = flowVelocity * flowArea * opening.flowCoefficient * 0.5f; // Reduced for upward flow

        // Apply time-based flow
        float deltaVolume = volumeFlowRate * Time.deltaTime * flowRate;

        // Limit flow
        float availableWater = lowerVolume.GetWaterVolume();
        float targetCapacity = upperVolume.MaxWaterCapacity - upperVolume.GetWaterVolume();
        deltaVolume = Mathf.Min(deltaVolume, availableWater, targetCapacity);

        if (deltaVolume < minFlowRate && availableWater > minFlowRate)
        {
            deltaVolume = minFlowRate;
        }

        if (deltaVolume > minWaterLevel)
        {
            // Transfer water with temperature mixing
            float sourceTemp = sourceFluid.GetTemp();
            float targetTemp = targetFluid?.GetTemp() ?? defaultWaterTemperature;
            float targetVolumeBefore = targetFluid?.GetConcentration() ?? 0f;

            lowerVolume.SetWaterVolume(lowerVolume.GetWaterVolume() - deltaVolume);
            upperVolume.SetWaterVolume(upperVolume.GetWaterVolume() + deltaVolume);

            if (targetVolumeBefore + deltaVolume > minWaterLevel)
            {
                float mixedTemp = ((targetVolumeBefore * targetTemp) + (deltaVolume * sourceTemp)) /
                                    (targetVolumeBefore + deltaVolume);
                upperVolume.Fluid.SetTemp(mixedTemp);
            }

            if (enableDebugLogs)
            {
                Debug.Log($"Ceiling opening flow (upward): {deltaVolume:F3}L from {lowerVolume.name} to {upperVolume.name}");
            }

            return true;
        }

        return false;
    }

    // Fixed vertical thick hole flow method
    private bool ProcessVerticalThickHoleFlow(VolumeWaterData volume1, VolumeWaterData volume2, Hole hole)
    {
        if (volume1 == null || volume2 == null)
            return false;

        Bounds bounds1 = volume1.GetComponent<BoxCollider>().bounds;
        Bounds bounds2 = volume2.GetComponent<BoxCollider>().bounds;

        // Determine which volume is upper and which is lower
        VolumeWaterData upperVolume = bounds1.center.y > bounds2.center.y ? volume1 : volume2;
        VolumeWaterData lowerVolume = bounds1.center.y > bounds2.center.y ? volume2 : volume1;

        float upperWaterHeight = upperVolume.GetAbsoluteWaterHeight();
        float lowerWaterHeight = lowerVolume.GetAbsoluteWaterHeight();

        float holeTopElevation = hole.GetTopElevation();
        float holeBottomElevation = hole.GetBottomElevation();

        // Determine flow direction based on water levels
        bool flowUpward = lowerWaterHeight > upperWaterHeight;

        if (flowUpward)
        {
            // Water flows upward through hole - lower volume must be full enough
            if (lowerWaterHeight <= holeBottomElevation || !lowerVolume.HasWater)
                return false;

            // Lower volume must be nearly full to push water up
            float lowerFillRatio = lowerVolume.GetWaterVolume() / lowerVolume.MaxWaterCapacity;
            if (lowerFillRatio < 0.95f)
                return false;

            if (upperVolume.GetWaterVolume() >= upperVolume.MaxWaterCapacity)
                return false;
        }
        else
        {
            // Water flows downward through hole - upper volume must have water
            if (upperWaterHeight <= holeTopElevation || !upperVolume.HasWater)
                return false;

            if (lowerVolume.GetWaterVolume() >= lowerVolume.MaxWaterCapacity)
                return false;
        }

        VolumeWaterData sourceVolume = flowUpward ? lowerVolume : upperVolume;
        VolumeWaterData targetVolume = flowUpward ? upperVolume : lowerVolume;
        IFluid sourceFluid = sourceVolume.Fluid;
        IFluid targetFluid = targetVolume.Fluid;

        // Calculate flow
        float sourceHeight = flowUpward ? lowerWaterHeight : upperWaterHeight;
        float targetHeight = flowUpward ? upperWaterHeight : lowerWaterHeight;

        float relevantElevation = flowUpward ? holeBottomElevation : holeTopElevation;

        if (sourceHeight <= relevantElevation)
            return false;

        float headHeight = sourceHeight - relevantElevation;
        float flowVelocity = Mathf.Sqrt(2f * gravityMultiplier * Mathf.Abs(sourceHeight - targetHeight));
        float flowArea = Mathf.PI * Mathf.Pow(hole.width / 2f, 2f);

        // Reduce flow for upward direction
        float flowMultiplier = flowUpward ? 0.3f : 1.0f;
        float volumeFlowRate = flowVelocity * flowArea * hole.flowCoefficient * flowMultiplier;

        float deltaVolume = volumeFlowRate * Time.deltaTime * flowRate;

        // Limit flow
        float availableWater = sourceVolume.GetWaterVolume();
        float targetCapacity = targetVolume.MaxWaterCapacity - targetVolume.GetWaterVolume();
        deltaVolume = Mathf.Min(deltaVolume, availableWater, targetCapacity);

        if (deltaVolume < minFlowRate && availableWater > minFlowRate)
        {
            deltaVolume = minFlowRate;
        }

        if (deltaVolume > minWaterLevel)
        {
            // Transfer water
            float sourceTemp = sourceFluid.GetTemp();
            float targetTemp = targetFluid?.GetTemp() ?? defaultWaterTemperature;
            float targetVolumeBefore = targetFluid?.GetConcentration() ?? 0f;

            sourceVolume.SetWaterVolume(sourceVolume.GetWaterVolume() - deltaVolume);
            targetVolume.SetWaterVolume(targetVolume.GetWaterVolume() + deltaVolume);

            // Mix temperatures
            if (targetVolumeBefore + deltaVolume > minWaterLevel)
            {
                float mixedTemp = ((targetVolumeBefore * targetTemp) + (deltaVolume * sourceTemp)) /
                                    (targetVolumeBefore + deltaVolume);
                targetVolume.Fluid.SetTemp(mixedTemp);
            }

            if (enableDebugLogs)
            {
                string direction = flowUpward ? "upward" : "downward";
                Debug.Log($"Vertical thick hole flow ({direction}): {deltaVolume:F3}L from {sourceVolume.name} to {targetVolume.name}");
            }

            return true;
        }

        return false;
    }

    bool ProcessDirectFlow(VolumeWaterData volume1, VolumeWaterData volume2)
    {
        if (volume1 == null || volume2 == null)
            return false;

        float waterHeight1 = volume1.GetAbsoluteWaterHeight();
        float waterHeight2 = volume2.GetAbsoluteWaterHeight();

        // Determine flow direction and volumes
        VolumeWaterData sourceVolume, targetVolume;
        float sourceHeight, targetHeight;

        if (waterHeight1 > waterHeight2)
        {
            sourceVolume = volume1;
            targetVolume = volume2;
            sourceHeight = waterHeight1;
            targetHeight = waterHeight2;
        }
        else
        {
            sourceVolume = volume2;
            targetVolume = volume1;
            sourceHeight = waterHeight2;
            targetHeight = waterHeight1;
        }

        // Check if water can flow through opening
        if (!sourceVolume.HasWater)
            return false;

        // Get fluid data for source and target
        IFluid sourceFluid = sourceVolume.Fluid;
        IFluid targetFluid = targetVolume.Fluid;

        // Calculate the exact overlapping bounds for flowrate
        float flowArea = VolumesConnectionArea(volume1, volume2);

        // Calculate flow rate using simplified fluid dynamics
        float heightDifference = sourceHeight - targetHeight;
        float flowVelocity = Mathf.Sqrt(2f * gravityMultiplier * heightDifference);

        float volumeFlowRate = flowVelocity * flowArea;

        // Apply time-based flow
        float deltaVolume = volumeFlowRate * Time.deltaTime * flowRate;

        // Limit flow to available water and target capacity
        float availableWater = sourceVolume.GetWaterVolume();
        float targetCapacity = targetVolume.MaxWaterCapacity - targetVolume.GetWaterVolume();

        deltaVolume = Mathf.Min(deltaVolume, availableWater, targetCapacity);

        // Equilibrium check - stop flow when levels would be equal
        float finalSourceVolume = sourceVolume.GetWaterVolume() - deltaVolume;
        float finalTargetVolume = targetVolume.GetWaterVolume() + deltaVolume;

        float finalSourceHeight = sourceVolume.VolumeFloorHeight + (finalSourceVolume / sourceVolume.CrossSectionalArea);
        float finalTargetHeight = targetVolume.VolumeFloorHeight + (finalTargetVolume / targetVolume.CrossSectionalArea);

        // If flow would overshoot equilibrium, adjust to reach exact equilibrium
        if ((sourceHeight > targetHeight && finalSourceHeight < finalTargetHeight) ||
            Mathf.Abs(finalSourceHeight - finalTargetHeight) < 0.01f)
        {
            // Calculate exact equilibrium
            float totalVolume = sourceVolume.GetWaterVolume() + targetVolume.GetWaterVolume();
            float totalArea = sourceVolume.CrossSectionalArea + targetVolume.CrossSectionalArea;

            // Account for different floor heights
            float avgFloorHeight = (sourceVolume.VolumeFloorHeight * sourceVolume.CrossSectionalArea +
                                    targetVolume.VolumeFloorHeight * targetVolume.CrossSectionalArea) / totalArea;

            float equilibriumHeight = avgFloorHeight + (totalVolume / totalArea);

            float newSourceVolume = Mathf.Max(0, (equilibriumHeight - sourceVolume.VolumeFloorHeight) * sourceVolume.CrossSectionalArea);
            float newTargetVolume = Mathf.Max(0, (equilibriumHeight - targetVolume.VolumeFloorHeight) * targetVolume.CrossSectionalArea);

            // Update fluid data
            //sourceFluid.SetConcentration(newSourceVolume / 1000f);
            //targetFluid.SetConcentration(newTargetVolume / 1000f);

            // Update volume water
            sourceVolume.SetWaterVolume(newSourceVolume);
            targetVolume.SetWaterVolume(newTargetVolume);

            // Calculate mixed temperature
            if (totalVolume > 0.001f)
            {
                float mixedTemp = ((sourceVolume.GetWaterVolume() * sourceFluid.GetTemp()) +
                                    (targetVolume.GetWaterVolume() * targetFluid.GetTemp())) / totalVolume;
                sourceFluid.SetTemp(mixedTemp);
                targetFluid.SetTemp(mixedTemp);
            }
        }
        else if (deltaVolume > minWaterLevel)
        {
            // Calculate temperature mixing when transferring water
            /*
            float sourceTemp = sourceFluid.GetTemp();
            float targetTemp = targetFluid.GetTemp();
            float sourceVolumeBefore = sourceFluid.GetConcentration() * 1000f;
            float targetVolumeBefore = targetFluid.GetConcentration() * 1000f;

            // Update volumes
            sourceVolume.SetWaterVolume(sourceVolume.GetWaterVolume() - deltaVolume);
            targetVolume.SetWaterVolume(targetVolume.GetWaterVolume() + deltaVolume);

            // Update fluid concentrations
            sourceFluid.SetConcentration(sourceVolume.GetWaterVolume() / 1000f);
            targetFluid.SetConcentration(targetVolume.GetWaterVolume() / 1000f);

            // Calculate mixed temperature in target
            if (targetVolumeBefore + deltaVolume > minWaterLevel)
            {
                float mixedTemp = ((targetVolumeBefore * targetTemp) + (deltaVolume * sourceTemp)) /
                                    (targetVolumeBefore + deltaVolume);
                targetFluid.SetTemp(mixedTemp);
            }
            */
            // Calculate temperature mixing when transferring water
            // /*
            float sourceTemp = sourceFluid.GetTemp();
            float sourceVolumeBefore = sourceFluid.GetConcentration();
            float targetTemp = 0f;
            float targetVolumeBefore = 0f;

            if (targetFluid != null)
            {
                targetTemp = targetFluid.GetTemp();
                targetVolumeBefore = targetFluid.GetConcentration();
            }

            // Update volumes
            sourceVolume.SetWaterVolume(sourceVolume.GetWaterVolume() - deltaVolume);
            targetVolume.SetWaterVolume(targetVolume.GetWaterVolume() + deltaVolume);

            // Calculate mixed temperature in target
            if (targetVolumeBefore + deltaVolume > minWaterLevel)
            {
                float mixedTemp = ((targetVolumeBefore * targetTemp) + (deltaVolume * sourceTemp)) /
                                    (targetVolumeBefore + deltaVolume);
                // Use targetVolume.Fluid in case targetFluid was null
                targetVolume.Fluid.SetTemp(mixedTemp);
            }//*/
        }

        if (enableDebugLogs && deltaVolume > minWaterLevel)
        {
            Debug.Log($"Water flowed: {deltaVolume:F3} from {sourceVolume.name} ({sourceFluid.GetTemp():F1}°C) to {targetVolume.name} ({targetFluid.GetTemp():F1}°C)");
        }

        return deltaVolume > minWaterLevel;
    }

    bool CanVolumesConnect_Opening(VolumeWaterData volume1, VolumeWaterData volume2, Opening opening)
    {
        // Check if volumes share a face at the opening location
        Bounds bounds1 = volume1.GetComponent<BoxCollider>().bounds;
        Bounds bounds2 = volume2.GetComponent<BoxCollider>().bounds;
        Vector3 openingPos = opening.transform.position;

        // Check if opening is at the boundary between volumes
        bool onBoundary = false;

        // Check X boundary
        if (Mathf.Abs(openingPos.x - bounds1.max.x) < 0.1f && Mathf.Abs(openingPos.x - bounds2.min.x) < 0.1f ||
            Mathf.Abs(openingPos.x - bounds1.min.x) < 0.1f && Mathf.Abs(openingPos.x - bounds2.max.x) < 0.1f)
        {
            onBoundary = true;
        }

        // Check Z boundary
        if (Mathf.Abs(openingPos.z - bounds1.max.z) < 0.1f && Mathf.Abs(openingPos.z - bounds2.min.z) < 0.1f ||
            Mathf.Abs(openingPos.z - bounds1.min.z) < 0.1f && Mathf.Abs(openingPos.z - bounds2.max.z) < 0.1f)
        {
            onBoundary = true;
        }

        return onBoundary;
    }

    float VolumesConnectionArea(VolumeWaterData volume1, VolumeWaterData volume2)
    {
        // Check if volumes share a face at the opening location
        Bounds bounds1 = volume1.GetComponent<BoxCollider>().bounds;
        Bounds bounds2 = volume2.GetComponent<BoxCollider>().bounds;

        // Get X boundary Size
        // The edge of the sub room is behind the edge of the main
        float start = 0f;
        if (bounds1.min.x >= bounds2.min.x)
        {
            start = bounds1.min.x;
        }
        // The edge of the sub room is infront of the main room edge in X.
        else
        {
            start = bounds2.min.x;
        }
        float stop = 0f;
        // The far edge of the sub room is behind the far edge of the main
        if (bounds1.max.x >= bounds2.max.x)
        {
            stop = bounds2.max.x;
        }
        // The edge of the sub room is infront of the main room edge in X.
        else
        {
            stop = bounds1.max.x;
        }
        float x = start - stop;

        // Get Z boundary size
        // The edge of the sub room is behind the edge of the main
        start = 0f;
        if (bounds1.min.z >= bounds2.min.z)
        {
            start = bounds1.min.z;
        }
        // The edge of the sub room is infront of the main room edge in X.
        else
        {
            start = bounds2.min.z;
        }
        stop = 0f;
        // The far edge of the sub room is behind the far edge of the main
        if (bounds1.max.x >= bounds2.max.z)
        {
            stop = bounds2.max.z;
        }
        // The edge of the sub room is infront of the main room edge in X.
        else
        {
            stop = bounds1.max.z;
        }
        float z = start - stop;

        // Get Y boundary
        start = 0f;
        // The top of sub room is below the main room
        if (bounds1.max.y >= bounds2.max.y)
        {
            start = bounds2.max.y;
        }
        else
        {
            start = bounds1.max.y;
        }
        stop = 0f;
        // The bottom of sub room is below the main room
        if (bounds1.min.y >= bounds2.min.y)
        {
            stop = bounds1.min.y;
        }
        else
        {
            stop = bounds2.min.y;
        }
        float y = start - stop;

        //float area = 0f;
        // Check if the rooms share the XZ plane
        // For y, The min or max of one must be equal to the min or max of the other
        if (Mathf.Approximately(bounds1.max.y, bounds2.min.y) || Mathf.Approximately(bounds1.min.y, bounds2.max.y))
        {
            //if (enableDebugLogs)
            //{
            //    Debug.Log(Mathf.Abs(x * z) + " on XZ");
            //}
            return Mathf.Abs(x * z);
        }

        // Check if the rooms share the XY plane
        // For Z, the min or max of one must be equal to the min or max of the other
        if (Mathf.Approximately(bounds1.max.z, bounds2.min.z) || Mathf.Approximately(bounds1.min.z, bounds2.max.z))
        {
            //if (enableDebugLogs)
            //{
            //    Debug.Log(Mathf.Abs(x * y) + " on XY");
            //}
            return Mathf.Abs(x * y);
        }

        // Check if the rooms share the ZY plane
        // For X, same as above.
        if (Mathf.Approximately(bounds1.max.x, bounds2.min.x) || Mathf.Approximately(bounds1.min.x, bounds2.max.x))
        {
            //if (enableDebugLogs)
            //{
            //    Debug.Log(Mathf.Abs(z * y) + " on Zy");
            //}
            return Mathf.Abs(z * y);
        }

        //The rooms do not seem to connect; which might be an error
        Debug.Log(volume1.gameObject.name + " and " + volume2.gameObject.name + " do not seem to connect!");
        return 0f;
    }

    public void AddWaterToVolume(VolumeWaterData volume, IFluid fluid)
    {
        if (volume != null)
        {
            volume.AddFluid(fluid);
        }
    }

    // Public methods for external control
    public void AddWaterToVolume(VolumeWaterData volume, float amount, float temperature = 20f)
    {
        if (volume != null)
        {
            IFluid fluid = new IFluid("Water", temperature, amount);
            fluid.SetLocalPressure(1f);
            fluid.SetDensity(1000);
            volume.AddFluid(fluid);


            /*float currentVolume = volume.GetWaterVolume();
            volume.SetWaterVolume(currentVolume + amount);

            IFluid fluid;
            // Update fluid data with temperature mixing
            if (volumeFluidMap != null && volumeFluidMap.TryGetValue(volume, out fluid))
            {
                //fluid = volumeFluidMap[volume];
                float newVolume = currentVolume + amount;

                if (newVolume > 0.001f)
                {
                    float mixedTemp = ((currentVolume * fluid.GetTemp()) + (amount * temperature)) / newVolume;
                    fluid.SetTemp(mixedTemp);
                }
                else
                {
                    fluid.SetTemp(temperature);
                }

                fluid.SetConcentration(newVolume / 1000f);
            }
            else
            {
                fluid = new IFluid("Water", temperature, amount);
                volumeFluidMap.Add(volume, fluid);
            }*/
        }
    }

    public void DrainVolumeWater(VolumeWaterData volume, float amount)
    {
        if (volume != null)
        {
            float newVolume = Mathf.Max(0, volume.GetWaterVolume() - amount);
            volume.SetWaterVolume(newVolume);

            // Update fluid data
            IFluid fluid = volume.Fluid;
            fluid.SetConcentration(newVolume / 1000f);
        }
    }

    public float GetTotalWaterInSystem()
    {
        float total = 0f;
        foreach (var volume in allWaterVolumes)
        {
            total += volume.GetWaterVolume();
        }
        return total;
    }

    public float GetVolumeTemperature(VolumeWaterData volume)
    {
        if (volume != null)// && volumeFluidMap.ContainsKey(volume))
        {
            return volume.Fluid.GetTemp();
        }
        return defaultWaterTemperature;
    }

    public void SetVolumeTemperature(VolumeWaterData volume, float temperature)
    {
        if (volume != null)//&& volumeFluidMap.ContainsKey(volume))
        {
            volume.Fluid.SetTemp(temperature);
        }
    }

    public IFluid GetVolumeFluid(VolumeWaterData volume)
    {
        if (volume != null)// && volumeFluidMap.ContainsKey(volume))
        {
            return volume.Fluid;
        }
        return null;
    }

    public void HeatVolumeWater(VolumeWaterData volume, float energyInput)
    {
        if (volume != null)// && volumeFluidMap.ContainsKey(volume))
        {
            IFluid fluid = volume.Fluid;
            float waterMass = fluid.GetConcentration() * fluid.GetDensity();

            //if (waterMass > 0.001f)
            //{
                // Simple temperature change calculation
                // Using water's specific heat capacity of 4.186 J/g°C
                float specificHeatCapacity = 4.186f;
                float temperatureChange = energyInput / (waterMass * specificHeatCapacity);
                float newTemp = fluid.GetTemp() + temperatureChange;

                // Prevent unrealistic temperatures
                newTemp = Mathf.Clamp(newTemp, 0f, 100f);
                fluid.SetTemp(newTemp);
            //}
        }
    }
}
