using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectUniverse.Environment.Volumes;
using ProjectUniverse.Util;
using UnityEngine.Rendering;

namespace ProjectUniverse.Environment.Fluids
{
    /// <summary>
    /// Unified pipe for transferring fluids in any state (liquid, gas, or mixed)
    /// Uses mass-based transfers with proper thermodynamic calculations
    /// </summary>
    public class FluidPipe : MonoBehaviour
    {
        [SerializeField] private List<Fluid> fluids = new();
        [SerializeField] private FluidPipe[] neighbors;
        [SerializeField] private float pipeTemperature; // K
        [SerializeField] private float[] tempTolerance = new float[2]; // [min, max] in K
        [SerializeField] private float maxPressure_bar = 216f;
        [SerializeField] private float currentPressure_bar = 1f;
        [SerializeField] private float volume_m3 = 0.4f; // standard duct volume
        [SerializeField] private float health = 100f;
        [SerializeField] private float leakRate = 0f; // 0-1, percentage lost per second
        [SerializeField] private GameObject[] bulletHoles;
        [SerializeField] private GameObject vent;
        private bool hasVent;
        [SerializeField] private Volume ductVolume;
        [SerializeField] private float insulationRating = 0.1f; // 0-1, heat transfer resistance
        [SerializeField] private bool burst = false;
        [Tooltip("If true, pipes will work outside of room volumes")]
        [SerializeField] private bool ignoreNeighborConstraint = false;
        [SerializeField] private float throughput_m3hr;
        [SerializeField] private float equivalentDiameterInner_m = 0.408f;
        [SerializeField] private float maxVelocity_ms = 120.5f;
        private float flowVelocity_ms = 0f;

        // Cached calculations
        private float totalMass_kg = 0f;
        private float totalVolume_m3 = 0f;
        private float averageTemperature_K = 293.15f;

        private AudioSource ventSFX;
        private bool hasVentSFX;
        private VolumeAtmosphereController roomVAC;

        // Properties
        public bool IsBurst
        {
            get { return burst; }
            set { burst = value; }
        }

        public List<Fluid> Fluids
        {
            get { return fluids; }
            set { fluids = value; RecalculateTotals(); }
        }

        public float Temperature
        {
            get { return pipeTemperature; }
            set { pipeTemperature = value; }
        }

        public float Volume
        {
            get { return volume_m3; }
            set { volume_m3 = value; }
        }

        public GameObject Vent
        {
            get { return vent; }
            set { vent = value; hasVent = (vent != null); }
        }

        public float Throughput
        {
            get { return throughput_m3hr; }
        }

        public float InnerDiameter
        {
            get { return equivalentDiameterInner_m; }
            set { equivalentDiameterInner_m = value; }
        }

        public float MaxVelocity
        {
            get { return maxVelocity_ms; }
        }

        public float FlowVelocity
        {
            get { return flowVelocity_ms; }
            set { flowVelocity_ms = value; }
        }

        public float GlobalPressure
        {
            get { return currentPressure_bar; }
        }

        public FluidPipe[] Neighbors
        {
            get { return neighbors; }
            set { neighbors = value; }
        }

        public bool HasVent
        {
            get { return hasVent; }
        }

        public bool HasSFX
        {
            get { return hasVentSFX; }
        }

        private void Start()
        {
            pipeTemperature = 293.15f; // Default 20°C

            if (Vent != null)
            {
                hasVent = true;
            }

            if (HasVent)
            {
                if (ventSFX == null)
                {
                    if (!Vent.TryGetComponent<AudioSource>(out ventSFX))
                    {
                        ventSFX = Vent.GetComponentInChildren<AudioSource>();
                    }
                }
            }

            hasVentSFX = (ventSFX != null);

            if (ductVolume != null)
            {
                ductVolume.TryGetComponent<VolumeAtmosphereController>(out roomVAC);
            }
        }

        void Update()
        {
            if (Neighbors != null && Neighbors.Length > 0)
            {
                ignoreNeighborConstraint = true;
            }

            RecalculateTotals();

            // Temperature equilibration with pipe
            EquilibrateWithPipe();

            // Check for overpressure
            CheckPressureLimits();

            // Handle leaks
            if (leakRate > 0f && roomVAC != null)
            {
                HandleLeaks();
            }

            // Transfer between neighbors
            if (ignoreNeighborConstraint && neighbors != null && neighbors.Length > 0 && !burst)
            {
                TransferToNeighbors();
            }

            // Vent to room if applicable
            if (hasVent && ductVolume != null && roomVAC != null)
            {
                VentToVolume();
            }

            // Update throughput calculation
            UpdateThroughput();

            // Handle vent audio
            if (hasVent && hasVentSFX)
            {
                bool shouldPlay = fluids.Count > 0 && totalMass_kg > 0.01f;
                if (shouldPlay && !ventSFX.isPlaying)
                {
                    ventSFX.Play();
                }
                else if (!shouldPlay && ventSFX.isPlaying)
                {
                    ventSFX.Stop();
                }
            }
        }

        private void RecalculateTotals()
        {
            totalMass_kg = 0f;
            totalVolume_m3 = 0f;
            averageTemperature_K = 0f;
            currentPressure_bar = 0f;

            if (fluids.Count == 0) return;

            // Combine duplicate fluids
            fluids = Utils.CombineFluids(fluids);

            // Calculate totals
            float totalEnthalpy = 0f;
            foreach (var fluid in fluids)
            {
                totalMass_kg += fluid.GetMass();
                totalVolume_m3 += fluid.GetVolume();
                totalEnthalpy += fluid.GetEnthalpy();

                // Pressure contribution (partial pressure for gases)
                if (fluid.GetState() == FluidState.Gas || fluid.GetState() == FluidState.Mixed)
                {
                    float gasVolume = fluid.GetVolume();
                    if (fluid.GetState() == FluidState.Mixed)
                    {
                        // Only count gas portion for mixed state
                        gasVolume *= fluid.GetQuality();
                    }
                    // Add partial pressure contribution
                    currentPressure_bar += fluid.GetPressure() * (gasVolume / volume_m3);
                }
            }

            // Average temperature weighted by mass
            if (totalMass_kg > 0)
            {
                foreach (var fluid in fluids)
                {
                    averageTemperature_K += fluid.GetTemperature() * (fluid.GetMass() / totalMass_kg);
                }
            }
            else
            {
                averageTemperature_K = pipeTemperature;
            }

            // For liquids, add hydrostatic pressure (simplified)
            float liquidVolume = totalVolume_m3;
            foreach (var fluid in fluids)
            {
                if (fluid.GetState() == FluidState.Gas)
                {
                    liquidVolume -= fluid.GetVolume();
                }
                else if (fluid.GetState() == FluidState.Mixed)
                {
                    liquidVolume -= fluid.GetVolume() * fluid.GetQuality();
                }
            }

            if (liquidVolume > 0)
            {
                // Add 1 bar for liquid presence (simplified)
                currentPressure_bar += 1f;
            }
        }

        public void Receive(bool destructive, float inputVelocity, float inputPressure, List<Fluid> inputFluids, float avgTemp = 0f)
        {
            if (inputFluids == null || inputFluids.Count == 0) return;

            flowVelocity_ms = inputVelocity;

            if (destructive)
            {
                fluids.Clear();
            }

            // Add fluids and combine duplicates
            fluids.AddRange(inputFluids);
            fluids = Utils.CombineFluids(fluids);

            // Set pressure for gas fluids
            foreach (var fluid in fluids)
            {
                if (fluid.GetState() == FluidState.Gas || fluid.GetState() == FluidState.Mixed)
                {
                    fluid.SetPressure(inputPressure);
                }
            }

            RecalculateTotals();
        }

        public void Receive(bool destructive, float inputVelocity, float inputPressure, Fluid inputFluid, float avgTemp = 0f)
        {
            List<Fluid> fluidList = new List<Fluid> { inputFluid };
            Receive(destructive, inputVelocity, inputPressure, fluidList, avgTemp);
        }

        /// <summary>
        /// Extract fluids from this pipe up to the specified volume
        /// </summary>
        public List<Fluid> ExtractFluids(float maxVolume_m3)
        {
            if (fluids.Count == 0 || totalVolume_m3 == 0)
                return new List<Fluid>();

            // Limit extraction rate by throughput
            float maxExtractRate = (throughput_m3hr / 3600f) * Time.deltaTime;
            float extractVolume = Mathf.Min(maxVolume_m3, maxExtractRate, totalVolume_m3);

            // Extract proportionally by volume
            float extractRatio = extractVolume / totalVolume_m3;

            return ExtractByRatio(extractRatio);
        }

        /// <summary>
        /// Extract fluids from this pipe up to the specified mass
        /// </summary>
        public List<Fluid> ExtractMass(float maxMass_kg)
        {
            if (fluids.Count == 0 || totalMass_kg == 0)
                return new List<Fluid>();

            float extractRatio = Mathf.Min(maxMass_kg / totalMass_kg, 1f);
            return ExtractByRatio(extractRatio);
        }

        private List<Fluid> ExtractByRatio(float ratio)
        {
            List<Fluid> extracted = new List<Fluid>();

            foreach (var fluid in fluids)
            {
                float extractMass = fluid.GetMass() * ratio;
                Fluid extractedFluid = fluid.Split(extractMass);
                extracted.Add(extractedFluid);
            }

            // Remove empty fluids
            fluids.RemoveAll(f => f.GetMass() <= 0.001f);
            RecalculateTotals();

            return extracted;
        }

        private void TransferToNeighbors()
        {
            if (totalMass_kg == 0) return;

            // Calculate average pressure across all connected pipes
            float totalPressure = currentPressure_bar;
            float totalCapacityUsed = totalVolume_m3 / volume_m3;
            int pipeCount = 1;

            foreach (var neighbor in neighbors)
            {
                if (neighbor != null && !neighbor.IsBurst)
                {
                    totalPressure += neighbor.currentPressure_bar;
                    totalCapacityUsed += neighbor.GetTotalVolume() / neighbor.Volume;
                    pipeCount++;
                }
            }

            float avgPressure = totalPressure / pipeCount;
            float avgCapacityUsed = totalCapacityUsed / pipeCount;

            // Transfer to equalize pressure and capacity usage
            foreach (var neighbor in neighbors)
            {
                if (neighbor == null || neighbor.IsBurst) continue;

                float neighborCapacityUsed = neighbor.GetTotalVolume() / neighbor.Volume;

                // Transfer if we have higher pressure or fuller pipe
                if (currentPressure_bar > neighbor.currentPressure_bar ||
                    (totalVolume_m3 / volume_m3) > neighborCapacityUsed)
                {
                    // Calculate transfer amount based on pressure difference
                    float pressureDiff = currentPressure_bar - neighbor.currentPressure_bar;
                    float transferRatio = Mathf.Clamp01(pressureDiff * 0.1f * Time.deltaTime);

                    // Also consider capacity difference
                    float capacityDiff = (totalVolume_m3 / volume_m3) - neighborCapacityUsed;
                    float capacityTransferRatio = Mathf.Clamp01(capacityDiff * 0.1f * Time.deltaTime);

                    transferRatio = Mathf.Max(transferRatio, capacityTransferRatio);

                    if (transferRatio > 0f)
                    {
                        List<Fluid> transferFluids = ExtractByRatio(transferRatio);
                        neighbor.Receive(false, flowVelocity_ms, avgPressure, transferFluids, averageTemperature_K);
                    }
                }
            }
        }

        private void VentToVolume()
        {
            if (roomVAC.Pressure >= 1.0f) return; // Room at pressure

            // Vent rate: 1 m³/s for gases, much less for liquids
            float ventRate = 1f;

            // Reduce vent rate for liquids
            float liquidFraction = 0f;
            foreach (var fluid in fluids)
            {
                if (fluid.GetState() == FluidState.Liquid)
                {
                    liquidFraction += fluid.GetMass() / totalMass_kg;
                }
                else if (fluid.GetState() == FluidState.Mixed)
                {
                    liquidFraction += (fluid.GetMass() / totalMass_kg) * (1f - fluid.GetQuality());
                }
            }

            ventRate *= (1f - liquidFraction * 0.9f); // Liquids vent 10x slower

            List<Fluid> ventedFluids = ExtractFluids(ventRate * Time.deltaTime);

            foreach (var fluid in ventedFluids)
            {
                // Convert liquids to gas when venting (atomization)
                if (fluid.GetState() == FluidState.Liquid)
                {
                    // Add energy to vaporize
                    float energyNeeded = fluid.GetMass() * 2257000f; // Assume water-like
                    fluid.AddEnergy(energyNeeded);
                }

                roomVAC.AddRoomFluid(fluid);
            }
        }

        private void EquilibrateWithPipe()
        {
            if (fluids.Count == 0) return;

            float heatTransferCoeff = 50f; // W/(m²·K)
            float pipeInnerSurfaceArea = Mathf.PI * equivalentDiameterInner_m * 1f; // per meter length

            foreach (var fluid in fluids)
            {
                float tempDiff = pipeTemperature - fluid.GetTemperature();
                float heatTransfer = heatTransferCoeff * pipeInnerSurfaceArea * tempDiff *
                                   (1f - insulationRating) * Time.deltaTime;

                if (Mathf.Abs(heatTransfer) > 0.1f)
                {
                    fluid.AddEnergy(heatTransfer);
                }
            }

            // Pipe temperature slowly moves toward fluid temperature
            float pipeHeatCapacity = 500f * 10f; // J/K (steel pipe, ~10kg)
            float pipeTempChange = -heatTransferCoeff * pipeInnerSurfaceArea *
                                 (pipeTemperature - averageTemperature_K) * Time.deltaTime / pipeHeatCapacity;
            pipeTemperature += pipeTempChange;
        }

        private void CheckPressureLimits()
        {
            if (burst) return;

            // Check pressure
            if (currentPressure_bar > maxPressure_bar)
            {
                burst = true;
                health = 0f;
                Debug.LogWarning($"Pipe burst due to overpressure: {currentPressure_bar} > {maxPressure_bar} bar");

                // Dump contents to room
                if (roomVAC != null)
                {
                    roomVAC.AddRoomFluid(fluids);
                    fluids.Clear();
                }
            }

            // Check temperature
            if (pipeTemperature < tempTolerance[0] || pipeTemperature > tempTolerance[1])
            {
                health -= Time.deltaTime * 10f;
                if (health <= 0f)
                {
                    burst = true;
                    Debug.LogWarning($"Pipe burst due to temperature: {pipeTemperature}K outside range [{tempTolerance[0]}, {tempTolerance[1]}]");
                }
            }
        }

        private void HandleLeaks()
        {
            if (leakRate <= 0f || roomVAC == null) return;

            float leakRatio = leakRate * Time.deltaTime;
            List<Fluid> leakedFluids = ExtractByRatio(leakRatio);

            foreach (var fluid in leakedFluids)
            {
                roomVAC.AddRoomFluid(fluid);
            }
        }

        private void UpdateThroughput()
        {
            // Calculate actual throughput based on what's flowing
            if (fluids.Count > 0 && flowVelocity_ms > 0)
            {
                // For mixed phase flow, use average density
                float avgDensity = totalMass_kg / totalVolume_m3;
                throughput_m3hr = Utils.CalculateFluidFlowThroughPipe(equivalentDiameterInner_m, flowVelocity_ms);

                // Reduce throughput for high liquid fraction (two-phase flow effects)
                float gasFraction = 0f;
                foreach (var fluid in fluids)
                {
                    if (fluid.GetState() == FluidState.Gas)
                        gasFraction += fluid.GetMass() / totalMass_kg;
                    else if (fluid.GetState() == FluidState.Mixed)
                        gasFraction += (fluid.GetMass() / totalMass_kg) * fluid.GetQuality();
                }

                // Two-phase flow multiplier (simplified)
                float twoPhaseMultiplier = gasFraction + (1f - gasFraction) * 0.3f;
                throughput_m3hr *= twoPhaseMultiplier;
            }
            else
            {
                throughput_m3hr = 0f;
            }
        }

        public void AddNeighbor(FluidPipe neighborPipe)
        {
            // Reset the neighbor list
            neighbors = new FluidPipe[1];
            neighbors[0] = neighborPipe;

            neighborPipe.Neighbors = new FluidPipe[1];
            neighborPipe.Neighbors[0] = this;
        }

        public float GetTotalVolume()
        {
            return totalVolume_m3;
        }

        public float GetTotalMass()
        {
            return totalMass_kg;
        }

        // Debug info
        public string GetContentsInfo()
        {
            string info = $"Pipe: {totalVolume_m3 / volume_m3:P1} full, {currentPressure_bar:F1} bar\n";
            info += $"Total: {totalMass_kg:F3}kg in {totalVolume_m3:F4}m³\n";
            info += $"Flow: {throughput_m3hr:F1} m³/hr at {flowVelocity_ms:F1} m/s\n";

            foreach (var fluid in fluids)
            {
                info += fluid.ToString() + "\n";
            }

            return info;
        }
    }
}