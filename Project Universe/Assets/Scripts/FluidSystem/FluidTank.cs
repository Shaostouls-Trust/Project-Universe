using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectUniverse.Environment.Volumes;
using ProjectUniverse.Util;

namespace ProjectUniverse.Environment.Fluids
{
    public class FluidTank : MonoBehaviour
    {
        [SerializeField] private float capacity_m3; 
        [SerializeField] private float totalMass_kg; 
        [SerializeField] private float totalVolume_m3;
        public float flowRate_m3hr;
        private float flowVelocity_ms = 120f;
        [SerializeField] private float maxFlowVelocity_ms = 120f;
        [Tooltip("Pressure level of the outflow pump.")]
        [SerializeField] private float outputPressure = 200f;
        [SerializeField] private bool valveState = false;
        [SerializeField] private bool valveOperable = false;
        [SerializeField] private VolumeAtmosphereController roomVolume;
        [SerializeField] private FluidPipe inflowPipe;
        [SerializeField] private FluidPipe outflowPipe;

        private List<Fluid> fluids = new List<Fluid>();
        public bool autofill = false;
        [Tooltip("Allows tank to change output velocity to not cause overpressure in pipes.\n" +
            "Disable automatic control to set output rate via velocity.")]
        [SerializeField] private bool automaticControl;
        private float lastInlet = 0f;
        private float lastOutlet = 0f;
        public bool fixAt85 = false;

        // Tank ambient temperature (K)
        [SerializeField] private float tankTemperature = 293.15f; // 20°C

        public float TotalVolume
        {
            get { return totalVolume_m3; }
        }

        public float TotalMass
        {
            get { return totalMass_kg; }
        }

        public float Capacity
        {
            get { return capacity_m3; }
        }

        public float FillPercentage
        {
            get { return totalVolume_m3 / capacity_m3; }
        }

        public float InletRate
        {
            get { return lastInlet; }
        }

        public float OutletRate
        {
            get { return lastOutlet; }
        }

        public bool AutomaticMode
        {
            get { return automaticControl; }
            set { automaticControl = value; }
        }

        public bool ValveOperable
        {
            get { return valveOperable; }
        }

        public bool ValveState
        {
            get { return valveState; }
            set { valveState = value; }
        }

        public float FlowVelocity
        {
            get { return flowVelocity_ms; }
            set { flowVelocity_ms = value; }
        }

        public float FlowVelocityMax
        {
            get { return maxFlowVelocity_ms; }
        }

        public FluidPipe OutflowPipe
        {
            get { return outflowPipe; }
        }

        void Start()
        {
            if (autofill)
            {
                // Create water at 60°F (288.7K)
                float tempK = (60f - 32f) / 1.8f + 273.15f;
                float massKg = capacity_m3 * 1000f;

                Fluid water = new Fluid("Water", massKg, tempK, 1f);
                fluids.Add(water);
                RecalculateTotals();
            }
        }

        void Update()
        {
            lastInlet = 0f;
            lastOutlet = 0f;

            // Heat exchange with tank (simplified)
            EquilibrateTemperature();

            // Inflow handling
            if (totalVolume_m3 < capacity_m3 && inflowPipe != null)
            {
                float availableVolume = capacity_m3 - totalVolume_m3;
                List<Fluid> inflowFluids = inflowPipe.ExtractFluids(availableVolume);

                foreach (var fluid in inflowFluids)
                {
                    lastInlet += fluid.GetMass();
                    AddFluid(fluid);
                }
            }

            // Maintain 85% fill if requested
            if (fixAt85 && fluids.Count > 0)
            {
                float targetMass = capacity_m3 * 0.85f * 1000f; // Assuming water
                if (totalMass_kg < targetMass * 0.84f)
                {
                    fluids[0].SetMass(targetMass);
                    RecalculateTotals();
                }
            }

            // Outflow handling
            if (valveState && totalMass_kg > 0f)
            {
                if (outflowPipe != null)
                {
                    HandlePipeOutflow();
                }
                else if (roomVolume != null)
                {
                    HandleVolumeOutflow();
                }
            }
        }

        private void HandlePipeOutflow()
        {
            float pipeVolume = outflowPipe.GetTotalVolume();
            if (pipeVolume >= outflowPipe.Volume) return;

            // Calculate flow rate
            float limVel = flowVelocity_ms;
            if (automaticControl && limVel > outflowPipe.MaxVelocity)
            {
                limVel = outflowPipe.MaxVelocity;
            }

            flowRate_m3hr = Utils.CalculateFluidFlowThroughPipe(outflowPipe.InnerDiameter, limVel);
            float flowRate_m3s = (flowRate_m3hr / 3600f) * Time.deltaTime;

            // Limit by available space in pipe
            float availableSpace = outflowPipe.Volume - pipeVolume;
            flowRate_m3s = Mathf.Min(flowRate_m3s, availableSpace);

            // Extract proportionally by mass
            float totalExtractMass = 0f;
            List<Fluid> outflowFluids = new List<Fluid>();

            // First pass: determine how much mass we can extract based on flow rate, ratio of vols
            foreach (var fluid in fluids)
            {
                float fluidVolume = fluid.GetVolume();
                float volumeRatio = fluidVolume / totalVolume_m3;
                float extractMass = (fluid.GetDensity() * flowRate_m3s) * volumeRatio;
                //Debug.Log($"{fluid.GetDensity()} * {flowRate_m3s} * {volumeRatio}: {extractMass} kg");
                totalExtractMass += extractMass;
            }

            // Second pass: actually extract the fluids
            foreach (var fluid in fluids)
            {
                float massRatio = fluid.GetMass() / totalMass_kg;
                float extractMass = (totalExtractMass * massRatio);
                //Debug.Log($"{fluid.GetDensity()} * {flowRate_m3s} * {massRatio} -> {extractMass} kg");
                if (extractMass <= 0.01)
                {
                    extractMass = fluid.GetMass();
                }

                Fluid extracted = fluid.Split(extractMass);
                outflowFluids.Add(extracted);
                lastOutlet += extractMass;
            }

            // Remove empty fluids
            fluids.RemoveAll(f => f.GetMass() <= 0f);

            // Send to pipe with tank pressure
            outflowPipe.Receive(false, limVel, outputPressure, outflowFluids);
            RecalculateTotals();
        }

        private void HandleVolumeOutflow()
        {
            float limVel = automaticControl ? 0f : maxFlowVelocity_ms;
            flowRate_m3hr = Utils.CalculateFluidFlowThroughPipe(0.408f, limVel);
            float flowRate_m3s = (flowRate_m3hr / 3600f) * Time.deltaTime;

            // Extract by mass proportionally
            float extractRatio = Mathf.Min(flowRate_m3s / totalVolume_m3, 1f);
            List<Fluid> spilledFluids = new List<Fluid>();

            foreach (var fluid in fluids)
            {
                float extractMass = fluid.GetMass() * extractRatio;
                Fluid spilled = fluid.Split(extractMass);
                spilledFluids.Add(spilled);
                lastOutlet += extractMass;
            }

            fluids.RemoveAll(f => f.GetMass() <= 0f);
            roomVolume.AddRoomFluid(spilledFluids);
            RecalculateTotals();
        }

        private void AddFluid(Fluid newFluid)
        {
            // Check if we already have this fluid type
            bool found = false;
            for (int i = 0; i < fluids.Count; i++)
            {
                if (fluids[i].GetIDName() == newFluid.GetIDName())
                {
                    fluids[i] = Fluid.Mix(fluids[i], newFluid);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                fluids.Add(newFluid);
            }

            RecalculateTotals();
        }

        private void RecalculateTotals()
        {
            totalMass_kg = 0f;
            totalVolume_m3 = 0f;

            foreach (var fluid in fluids)
            {
                totalMass_kg += fluid.GetMass();
                totalVolume_m3 += fluid.GetVolume();
            }
        }

        private void EquilibrateTemperature()
        {
            // Simple heat exchange with tank walls
            float heatTransferCoeff = 10f; // W/(m²·K)
            float tankSurfaceArea = 6f * Mathf.Pow(capacity_m3, 0.67f); // Approximate

            foreach (var fluid in fluids)
            {
                float tempDiff = tankTemperature - fluid.GetTemperature();
                float heatTransfer = heatTransferCoeff * tankSurfaceArea * tempDiff * Time.deltaTime;

                if (Mathf.Abs(heatTransfer) > 0.1f)
                {
                    fluid.AddEnergy(heatTransfer);
                }
            }
        }

        public void ExternalInteractFunc()
        {
            valveState = !valveState;
        }

        // Debug info
        public string GetContentsInfo()
        {
            string info = $"Tank: {FillPercentage:P1} full\n";
            info += $"Total: {totalMass_kg:F1}kg in {totalVolume_m3:F3}m³\n";

            foreach (var fluid in fluids)
            {
                info += fluid.ToString() + "\n";
            }

            return info;
        }
    }
}