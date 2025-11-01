using UnityEngine;
using ProjectUniverse.Environment.Gas;
using ProjectUniverse.Environment.Fluids;
using System.Collections.Generic;
using System.Linq;


namespace ProjectUniverse.Environment.Chemistry
{
    /// <summary>
    /// Test controller with enhanced debugging
    /// </summary>
    public class ReactionTestController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Environment.Volumes.VolumeAtmosphereController atmosphereController;
        [SerializeField] private RoomReactionManager reactionManager;

        [Header("Test Configuration")]
        [SerializeField] private bool autoSetupOnStart = true;
        [SerializeField] private ReactionTestType testType = ReactionTestType.MethaneSpark;

        [Header("Test Controls")]
        [SerializeField] private bool addTestGases = false;
        [SerializeField] private bool triggerSpark = false;
        [SerializeField] private bool setHighTemperature = false;
        [SerializeField] private bool resetRoom = false;

        [Header("Phase Change Tests")]
        [SerializeField] private bool addWaterLiquid = false;
        [SerializeField] private bool heatToBoiling = false;

        [Header("Custom Gas Addition")]
        [SerializeField] private string gasID = "CH4";
        [SerializeField] private float gasConcentration = 0.1f;
        [SerializeField] private float gasTemperature = 70f;

        [Header("Status Display")]
        [SerializeField] private string currentStatus = "";

        public enum ReactionTestType
        {
            MethaneAutoIgnition,
            MethaneSpark,
            HydrogenExplosion,
            NitrousDecomposition,
            AmmoniaOxidation,
            HydrogenNitrousOxide
        }

        private void Start()
        {
            if (atmosphereController == null)
            {
                atmosphereController = GetComponent<Environment.Volumes.VolumeAtmosphereController>();
            }

            if (reactionManager == null)
            {
                reactionManager = GetComponent<RoomReactionManager>();
            }

            if (autoSetupOnStart)
            {
                SetupTest(testType);
            }

            InvokeRepeating(nameof(UpdateStatus), 1f, 1f);
        }

        private void Update()
        {
            if (addTestGases)
            {
                addTestGases = false;
                SetupTest(testType);
            }

            if (triggerSpark)
            {
                triggerSpark = false;
                reactionManager.TriggerIgnition(5f); // 5 second spark
                PrintRoomStatus();
            }

            if (setHighTemperature)
            {
                setHighTemperature = false;
                reactionManager.SetRoomTemperature(700f);
                PrintRoomStatus();
            }

            if (resetRoom)
            {
                resetRoom = false;
                ResetRoom();
            }

            if (addWaterLiquid)
            {
                addWaterLiquid = false;
                AddLiquid("Water", 0.1f, 70f);
                Debug.Log("Added water liquid. Use 'Heat To Boiling' to vaporize it.");
            }

            if (heatToBoiling)
            {
                heatToBoiling = false;
                reactionManager.SetRoomTemperature(110f); // Above 100°C
                Debug.Log("Temperature set above boiling point of water");
            }
        }

        private void UpdateStatus()
        {
            float tempC = (atmosphereController.Temperature - 32f) * (5f / 9f);
            currentStatus = $"T: {tempC:F1}°C | P: {atmosphereController.RoomPressure:F2}atm | Gases: {atmosphereController.RoomGassesLegacy.Count}";

            if (reactionManager.HasIgnitionSource())
            {
                currentStatus += " | SPARK";
            }
        }

        private void SetupTest(ReactionTestType type)
        {
            ResetRoom();

            switch (type)
            {
                case ReactionTestType.MethaneAutoIgnition:
                    SetupMethaneAutoIgnition();
                    break;

                case ReactionTestType.MethaneSpark:
                    SetupMethaneSpark();
                    break;

                case ReactionTestType.HydrogenExplosion:
                    SetupHydrogenExplosion();
                    break;

                case ReactionTestType.NitrousDecomposition:
                    SetupNitrousDecomposition();
                    break;

                case ReactionTestType.AmmoniaOxidation:
                    SetupAmmoniaOxidation();
                    break;

                case ReactionTestType.HydrogenNitrousOxide:
                    SetupHydrogenNitrousOxide();
                    break;
            }

            Debug.Log($"Test setup complete: {type}");
            PrintRoomStatus();
        }

        private void SetupMethaneAutoIgnition()
        {
            AddGas("Methane", 0.15f, 70f, 1f);
            AddGas("Oxygen", 0.4f, 70f, 1f);
            Debug.Log("Methane and oxygen added. Use 'Set High Temperature' to trigger auto-ignition (537°C)");
        }

        private void SetupMethaneSpark()
        {
            AddGas("Methane", 0.1f, 70f, 1f);
            AddGas("Oxygen", 0.25f, 70f, 1f);
            Debug.Log("Methane and oxygen added. Use 'Trigger Spark' to ignite");
        }

        private void SetupHydrogenExplosion()
        {
            AddGas("Hydrogen", 0.66f, 70f, 1f);
            AddGas("Oxygen", 0.34f, 70f, 1f);
            Debug.Log("Hydrogen and oxygen added (stoichiometric). Use 'Trigger Spark' for EXPLOSION!");
        }

        private void SetupNitrousDecomposition()
        {
            AddGas("N2O", 0.5f, 70f, 1f);
            Debug.Log("Nitrous oxide added. Heat to 450°C for decomposition");
        }

        private void SetupAmmoniaOxidation()
        {
            AddGas("NH3", 0.2f, 70f, 1f);
            AddGas("Oxygen", 0.3f, 70f, 1f);
            Debug.Log("Ammonia and oxygen added. Heat to 700°C and spark for oxidation");
        }

        private void SetupHydrogenNitrousOxide()
        {
            AddGas("Hydrogen", 0.15f, 70f, 1f);
            AddGas("N2O", 0.15f, 70f, 1f);
            Debug.Log("H2 + N2O added. Use 'Trigger Spark' for highly energetic reaction!");
        }

        private void AddGas(string gasID, float fraction, float temperature, float pressure)
        {
            float volumeM3 = fraction * atmosphereController.RoomVolume;

            // Convert volume and temperature to mass for Fluid class
            CompoundData compound = ChemistryDatabase.GetCompound(gasID);
            if (compound == null)
            {
                Debug.LogError($"Compound not found: {gasID}");
                return;
            }

            // Convert temperature from Fahrenheit to Kelvin
            float tempK = ((temperature - 32f) * 5f / 9f) + 273.15f;

            // Calculate mass using ideal gas law: PV = nRT, then n * M = mass
            // V in liters, P in atm, T in K, R = 0.0821 L⋅atm/(mol⋅K)
            float volumeL = volumeM3 * 1000f;
            float moles = (pressure * volumeL) / (0.0821f * tempK);
            float massKg = (moles * compound.MolarMass) / 1000f; // g to kg

            Fluid gas = new Fluid(gasID, massKg, tempK, atmosphereController.RoomVolume, pressure);
            gas.SetContainerVolume(volumeL);
            atmosphereController.AddRoomGas(gas);

            string name = compound.Name;
            Debug.Log($"Added {name}: {fraction * 100f:F1}% of room volume ({massKg:F4} kg)");
        }

        private void AddGas_(string gasID, float fraction, float temperature, float pressure)
        {
            float volumeM3 = fraction * atmosphereController.RoomVolume;
            IGas gas = new IGas(gasID, temperature, volumeM3, pressure, atmosphereController.RoomVolume);
            atmosphereController.AddRoomGas(gas);

            CompoundData compound = ChemistryDatabase.GetCompound(gasID);
            string name = compound != null ? compound.Name : gasID;
            Debug.Log($"Added {name}: {fraction * 100f:F1}% of room volume");
        }

        private void AddLiquid(string fluidID, float volumeM3, float temperature)
        {
            CompoundData compound = ChemistryDatabase.GetCompound(fluidID);
            if (compound == null)
            {
                Debug.LogError($"Compound not found: {fluidID}");
                return;
            }

            // Convert temperature from Fahrenheit to Kelvin
            float tempK = ((temperature - 32f) * 5f / 9f) + 273.15f;

            // Calculate mass from volume using liquid density
            float massKg = volumeM3 * compound.Density; // Density in kg/m³

            Fluid fluid = new Fluid(fluidID, massKg, tempK, atmosphereController.RoomVolume, 1f);
            atmosphereController.AddRoomFluid(fluid);

            string name = compound.Name;
            Debug.Log($"Added {name} liquid: {volumeM3:F3} m³ ({massKg:F4} kg)");
        }

        private void AddLiquid_(string fluidID, float volumeM3, float temperature)
        {
            IFluid fluid = new IFluid(fluidID, temperature, volumeM3);
            atmosphereController.RoomFluidsLegacy.Add(fluid);

            CompoundData compound = ChemistryDatabase.GetCompound(fluidID);
            string name = compound != null ? compound.Name : fluidID;
            Debug.Log($"Added {name} liquid: {volumeM3:F3} m³");
        }

        private void ResetRoom()
        {
            atmosphereController.RoomFluids.Clear();
            atmosphereController.Temperature = 70f;
            atmosphereController.RoomPressure = 0f;

            var fires = FindObjectsOfType<PowerSystem.CollisionDemo.DemoFire>();
            foreach (var fire in fires)
            {
                Destroy(fire.gameObject);
            }

            reactionManager.ExtinguishIgnition();

            Debug.Log("Room reset");
        }

        [ContextMenu("Print Room Status")]
        public void PrintRoomStatus()
        {
            float tempC = (atmosphereController.Temperature - 32f) * (5f / 9f);

            Debug.Log("═══════════════════════════════");
            Debug.Log($"Temperature: {tempC:F1}°C ({atmosphereController.Temperature:F1}°F)");
            Debug.Log($"Pressure: {atmosphereController.RoomPressure:F3} atm");
            Debug.Log($"Contamination: {atmosphereController.Contamination:F2} ppm");
            Debug.Log($"Ignition: {(reactionManager.HasIgnitionSource() ? "YES" : "NO")}");
            int gasCount = atmosphereController.RoomFluids.Count(f => f.GetState() == FluidState.Gas || f.GetState() == FluidState.Mixed);
            int liquidCount = atmosphereController.RoomFluids.Count(f => f.GetState() == FluidState.Liquid);
            Debug.Log($"Gas Count: {gasCount}");
            Debug.Log($"Liquid Count: {liquidCount}");

            if (atmosphereController.RoomFluids.Count > 0)
            {
                Debug.Log("--- Fluids ---");
                float totalGasVolume = 0f;

                // Calculate total gas volume
                foreach (Fluid fluid in atmosphereController.RoomFluids)
                {
                    if (fluid.GetState() == FluidState.Gas || fluid.GetState() == FluidState.Mixed)
                    {
                        float gasVolume = fluid.GetVolume();
                        if (fluid.GetState() == FluidState.Mixed)
                        {
                            gasVolume *= fluid.GetQuality();
                        }
                        totalGasVolume += gasVolume;
                    }
                }

                foreach (Fluid fluid in atmosphereController.RoomFluids)
                {
                    CompoundData compound = ChemistryDatabase.GetCompound(fluid.GetIDName());
                    string name = compound != null ? compound.Name : fluid.GetIDName();

                    if (fluid.GetState() == FluidState.Gas || fluid.GetState() == FluidState.Mixed)
                    {
                        float gasVolume = fluid.GetVolume();
                        if (fluid.GetState() == FluidState.Mixed)
                        {
                            gasVolume *= fluid.GetQuality();
                        }
                        float percentage = totalGasVolume > 0 ? (gasVolume / totalGasVolume) * 100f : 0f;
                        Debug.Log($"  {name} (Gas): {percentage:F2}% ({gasVolume:F4} m³, {fluid.GetMass():F4} kg)");
                    }
                    else if (fluid.GetState() == FluidState.Liquid)
                    {
                        Debug.Log($"  {name} (Liquid): {fluid.GetVolume():F4} m³ ({fluid.GetMass():F4} kg)");
                    }
                }
            }

            Debug.Log("═══════════════════════════════\n");
        }
    }
}