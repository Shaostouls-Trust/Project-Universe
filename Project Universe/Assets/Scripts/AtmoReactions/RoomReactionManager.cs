using System.Collections.Generic;
using UnityEngine;
using ProjectUniverse.Environment.Gas;
using ProjectUniverse.Environment.Fluids;


namespace ProjectUniverse.Environment.Chemistry
{
    /// <summary>
    /// Manages chemical reactions and phase changes within a room volume
    /// </summary>
    public class RoomReactionManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Environment.Volumes.VolumeAtmosphereController atmosphereController;

        [Header("Reaction Control")]
        [SerializeField] private bool enableReactions = true;
        [SerializeField] private float reactionCheckInterval = 0.5f;
        [SerializeField] private float reactionRateMultiplier = 0.5f; // Reduced for stability

        [Header("Phase Change Control")]
        [SerializeField] private bool enablePhaseChanges = true;
        [SerializeField] private float phaseChangeCheckInterval = 1.0f;

        [Header("Ignition Sources")]
        [SerializeField] private bool hasIgnitionSource = false;
        [SerializeField] private float ignitionSourceDuration = 0f;

        [Header("Debug")]
        [SerializeField] private bool debugMode = true;
        [SerializeField] private List<string> activeReactionIDs = new List<string>();
        [SerializeField] private List<string> currentGasComposition = new List<string>();

        private float timeSinceLastCheck = 0f;
        private float timeSincePhaseCheck = 0f;
        private float ignitionTimer = 0f;

        private void Start()
        {
            if (atmosphereController == null)
            {
                atmosphereController = GetComponent<Environment.Volumes.VolumeAtmosphereController>();
            }

            if (atmosphereController == null)
            {
                Debug.LogError("RoomReactionManager requires a VolumeAtmosphereController component!");
                enabled = false;
                return;
            }

            ChemistryDatabase.Initialize();
            ParticulateDatabase.Initialize();
        }

        private void Update()
        {
            UpdateGasComposition();

            // Update ignition source timer
            if (hasIgnitionSource && ignitionSourceDuration > 0f)
            {
                ignitionTimer += Time.deltaTime;
                if (ignitionTimer >= ignitionSourceDuration)
                {
                    hasIgnitionSource = false;
                    ignitionTimer = 0f;
                    if (debugMode) Debug.Log("Ignition source extinguished");
                }
            }

            // Check for phase changes
            if (enablePhaseChanges)
            {
                timeSincePhaseCheck += Time.deltaTime;
                if (timeSincePhaseCheck >= phaseChangeCheckInterval)
                {
                    timeSincePhaseCheck = 0f;
                    CheckPhaseChanges();
                }
            }

            // Check for reactions
            if (enableReactions)
            {
                timeSinceLastCheck += Time.deltaTime;
                if (timeSinceLastCheck >= reactionCheckInterval)
                {
                    timeSinceLastCheck = 0f;
                    CheckAndProcessReactions();
                }
            }
        }

        //B
        private void UpdateGasComposition()
        {
            currentGasComposition.Clear();
            float totalGasVolume = 0f;

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
                if (fluid.GetState() == FluidState.Gas || fluid.GetState() == FluidState.Mixed)
                {
                    float gasVolume = fluid.GetVolume();
                    if (fluid.GetState() == FluidState.Mixed)
                    {
                        gasVolume *= fluid.GetQuality();
                    }
                    float percentage = totalGasVolume > 0
                        ? (gasVolume / totalGasVolume) * 100f
                        : 0f;
                    currentGasComposition.Add($"{fluid.GetIDName()}: {percentage:F2}%");
                }
            }
        }

        //A
        private void UpdateGasCompositionA()
        {
            currentGasComposition.Clear();
            float totalGasVolume = 0f;

            foreach (Fluid fluid in atmosphereController.RoomFluids)
            {
                if (fluid.GetState() == FluidState.Gas || fluid.GetState() == FluidState.Mixed)
                {
                    float gasVolume = fluid.GetVolume();
                    if (fluid.GetState() == FluidState.Mixed)
                        gasVolume *= fluid.GetQuality();
                    totalGasVolume += gasVolume;
                }
            }

            foreach (Fluid fluid in atmosphereController.RoomFluids)
            {
                if (fluid.GetState() == FluidState.Gas || fluid.GetState() == FluidState.Mixed)
                {
                    float gasVolume = fluid.GetVolume();
                    if (fluid.GetState() == FluidState.Mixed)
                        gasVolume *= fluid.GetQuality();

                    float percentage = totalGasVolume > 0 ? (gasVolume / totalGasVolume) * 100f : 0f;
                    currentGasComposition.Add($"{fluid.GetIDName()}: {percentage:F2}%");
                }
            }
        }

        /// <summary>
        /// Update the debug gas composition list
        /// </summary>
        private void UpdateGasComposition_()
        {
            currentGasComposition.Clear();
            float totalConcentration = 0f;

            foreach (IGas gas in atmosphereController.RoomGassesLegacy)
            {
                totalConcentration += gas.GetConcentration();
            }

            foreach (IGas gas in atmosphereController.RoomGassesLegacy)
            {
                float percentage = totalConcentration > 0
                    ? (gas.GetConcentration() / totalConcentration) * 100f
                    : 0f;
                currentGasComposition.Add($"{gas.GetIDName()}: {percentage:F2}%");
            }
        }
        //B
        private void CheckPhaseChanges()
        {
            float roomTempC = FahrenheitToCelsius(atmosphereController.Temperature);
            List<Fluid> fluidsToProcess = new List<Fluid>(atmosphereController.RoomFluids);

            foreach (Fluid fluid in fluidsToProcess)
            {
                CompoundData compound = ChemistryDatabase.GetCompound(fluid.GetIDName());
                if (compound == null) continue;

                // Only process liquids
                if (fluid.GetState() != FluidState.Liquid) continue;

                // Check if temperature is above boiling point
                if (roomTempC >= compound.BoilingPoint)
                {
                    VaporizeLiquid(fluid, compound);
                }
            }
        }

        //A
        private void CheckPhaseChangesA()
        {
            float roomTempC = FahrenheitToCelsius(atmosphereController.Temperature);
            List<Fluid> fluidsToProcess = new List<Fluid>(atmosphereController.RoomFluids);

            foreach (Fluid fluid in fluidsToProcess)
            {
                CompoundData compound = ChemistryDatabase.GetCompound(fluid.GetIDName());
                if (compound == null) continue;

                // The Fluid class now handles phase changes internally
                // Just update temperature and it will handle state transitions
                float tempK = roomTempC + 273.15f;
                fluid.SetTemperature(tempK);

                if (debugMode && fluid.GetState() == FluidState.Mixed)
                {
                    Debug.Log($"{compound.Name} is in mixed state with {fluid.GetQuality():P0} vapor quality");
                }
            }
        }

        /// <summary>
        /// Check for liquids that should boil into gases
        /// </summary>
        private void CheckPhaseChanges_()
        {
            float roomTempC = FahrenheitToCelsius(atmosphereController.Temperature);
            List<IFluid> fluidsToProcess = new List<IFluid>(atmosphereController.RoomFluidsLegacy);

            foreach (IFluid fluid in fluidsToProcess)
            {
                CompoundData compound = ChemistryDatabase.GetCompound(fluid.GetIDName());
                if (compound == null) continue;

                // Check if temperature is above boiling point
                if (roomTempC >= compound.BoilingPoint)
                {
                    //VaporizeLiquid(fluid, compound);
                }
            }
        }

        //B
        private void VaporizeLiquid(Fluid fluid, CompoundData compound)
        {
            float liquidMass = fluid.GetMass();
            if (liquidMass <= 0.0001f) return;

            // Calculate how much can vaporize this frame
            float vaporizeRate = 0.1f * phaseChangeCheckInterval; // 10% per second
            float massToVaporize = liquidMass * vaporizeRate;

            // Calculate heat required for vaporization
            float heatRequired = massToVaporize * compound.HeatOfVaporization;

            // Remove heat from room (endothermic process)
            atmosphereController.AddRoomHeat(-heatRequired, true);

            // Add energy to the fluid to vaporize it
            fluid.AddEnergy(heatRequired);

            if (debugMode)
            {
                Debug.Log($"Vaporized {massToVaporize:F4} kg of {compound.Name}, absorbing {heatRequired:F0} J");
            }
        }

        /// <summary>
        /// Convert liquid to gas (boiling)
        /// </summary>
        private void VaporizeLiquid_(IFluid fluid, CompoundData compound)
        {
            float liquidConcentration = fluid.GetConcentration();
            if (liquidConcentration <= 0.0001f) return;

            // Calculate how much can vaporize this frame
            float vaporizeRate = 0.1f * phaseChangeCheckInterval; // 10% per second
            float amountToVaporize = liquidConcentration * vaporizeRate;

            // Calculate heat required for vaporization
            // Q = n * ΔHvap
            float volumeL = amountToVaporize * 1000f; // m³ to L
            float massKg = (volumeL * compound.Density) / 1000f; // g to kg
            float moles = (massKg * 1000f) / compound.MolarMass; // kg to g, then to moles
            float heatRequired = moles * compound.HeatOfVaporization;

            // Remove heat from room (endothermic process)
            atmosphereController.AddRoomHeat(-heatRequired, true);

            // Remove liquid - bugged; fluid not properly removed
            IFluid fluidToRemove = new IFluid(fluid.GetIDName(), fluid.GetTemp(), amountToVaporize);
            //atmosphereController.RoomFluids.Remove(fluidToRemove); // removed while refactoring - add back with correct mass removal

            // Add gas
            IGas newGas = new IGas(
                compound.ID,
                atmosphereController.Temperature,
                amountToVaporize,
                atmosphereController.RoomPressure,
                atmosphereController.RoomVolume
            );
            atmosphereController.AddRoomGas(newGas);

            if (debugMode)
            {
                Debug.Log($"Vaporized {amountToVaporize:F4} m³ of {compound.Name}, absorbing {heatRequired:F0} J");
            }
        }

        /// <summary>
        /// Main reaction processing loop
        /// </summary>
        private void CheckAndProcessReactions()
        {
            activeReactionIDs.Clear();

            // Run based off what is in the room, not all possible combinations
            foreach (var reactionEntry in ChemistryDatabase.ReactionDatabase)
            {
                ReactionData reaction = reactionEntry.Value;

                if (CanReactionOccur(reaction))
                {
                    activeReactionIDs.Add(reaction.ID);
                    ExecuteReaction(reaction);
                }
            }
        }

        //A
        private bool CanReactionOccurA(ReactionData reaction)
        {
            // Check pressure range
            float roomPressure = atmosphereController.RoomPressure;
            if (roomPressure < reaction.Conditions.MinPressure || roomPressure > reaction.Conditions.MaxPressure)
            {
                return false;
            }

            // Check ignition requirement
            bool hasIgnition = hasIgnitionSource;
            bool hasAutoIgnition = false;

            if (reaction.Conditions.RequiresIgnition)
            {
                if (!hasIgnition)
                {
                    float roomTempC = FahrenheitToCelsius(atmosphereController.Temperature);
                    foreach (var reactant in reaction.Reactants)
                    {
                        CompoundData compound = ChemistryDatabase.GetCompound(reactant.Compound);
                        if (compound != null && compound.AutoIgnitionTemp > 0 && roomTempC >= compound.AutoIgnitionTemp)
                        {
                            hasAutoIgnition = true;
                            break;
                        }
                    }
                }

                if (!hasIgnition && !hasAutoIgnition)
                {
                    return false;
                }
            }

            if (!hasIgnition && !hasAutoIgnition)
            {
                float roomTempC = FahrenheitToCelsius(atmosphereController.Temperature);
                if (roomTempC < reaction.Conditions.MinTemperature || roomTempC > reaction.Conditions.MaxTemperature)
                {
                    return false;
                }
            }

            // Calculate total gas volume for percentage calculations
            float totalGasVolume = 0f;
            foreach (Fluid fluid in atmosphereController.RoomFluids)
            {
                if (fluid.GetState() == FluidState.Gas || fluid.GetState() == FluidState.Mixed)
                {
                    float gasVolume = fluid.GetVolume();
                    if (fluid.GetState() == FluidState.Mixed)
                        gasVolume *= fluid.GetQuality();
                    totalGasVolume += gasVolume;
                }
            }

            if (totalGasVolume <= 0f)
            {
                if (debugMode) Debug.Log($"Reaction {reaction.ID} no gases present");
                return false;
            }

            // Check for inhibitors
            if (reaction.Conditions.InhibitedBy != null && reaction.Conditions.InhibitedBy.Count > 0)
            {
                float totalInhibitorVolume = 0f;

                foreach (Fluid fluid in atmosphereController.RoomFluids)
                {
                    if (reaction.Conditions.InhibitedBy.Contains(fluid.GetIDName()))
                    {
                        if (fluid.GetState() == FluidState.Gas || fluid.GetState() == FluidState.Mixed)
                        {
                            float gasVolume = fluid.GetVolume();
                            if (fluid.GetState() == FluidState.Mixed)
                                gasVolume *= fluid.GetQuality();
                            totalInhibitorVolume += gasVolume;
                        }
                    }
                }

                float inhibitorPercentage = (totalInhibitorVolume / totalGasVolume) * 100f;
                if (inhibitorPercentage >= reaction.Conditions.InhibitorThreshold)
                {
                    if (debugMode) Debug.Log($"Reaction {reaction.ID} inhibited by {inhibitorPercentage:F1}% inhibitor gases");
                    return false;
                }
            }

            // Check that all reactants are present in sufficient quantities
            foreach (var reactant in reaction.Reactants)
            {
                Fluid gas = FindGasInRoom(reactant.Compound);
                if (gas == null)
                {
                    return false;
                }

                float gasVolume = gas.GetVolume();
                if (gas.GetState() == FluidState.Mixed)
                    gasVolume *= gas.GetQuality();

                float concentrationFraction = gasVolume / totalGasVolume;
                float concentrationPercentage = concentrationFraction * 100f;

                if (concentrationFraction < reactant.MinConcentration)
                {
                    if (debugMode) Debug.Log($"Reaction {reaction.ID} insufficient {reactant.Compound}: {concentrationPercentage:F2}% < {reactant.MinConcentration * 100f:F2}%");
                    return false;
                }
            }

            if (debugMode) Debug.Log($"All conditions met for reaction {reaction.ID} (ignition: {hasIgnition}, auto-ignition: {hasAutoIgnition})");
            return true;
        }

        //B
        private bool CanReactionOccur(ReactionData reaction)
        {
            // Check pressure range
            float roomPressure = atmosphereController.Pressure;
            if (roomPressure < reaction.Conditions.MinPressure || roomPressure > reaction.Conditions.MaxPressure)
            {
                //if (debugMode) Debug.Log($"Reaction {reaction.ID} pressure out of range: {roomPressure} atm (need {reaction.Conditions.MinPressure}-{reaction.Conditions.MaxPressure})");
                return false;
            }

            // Check ignition requirement
            bool hasIgnition = hasIgnitionSource;
            bool hasAutoIgnition = false;

            if (reaction.Conditions.RequiresIgnition)
            {
                if (!hasIgnition)
                {
                    // Check if any reactant has reached auto-ignition temperature
                    float roomTempC = FahrenheitToCelsius(atmosphereController.Temperature);
                    foreach (var reactant in reaction.Reactants)
                    {
                        CompoundData compound = ChemistryDatabase.GetCompound(reactant.Compound);
                        if (compound != null && compound.AutoIgnitionTemp > 0 && roomTempC >= compound.AutoIgnitionTemp)
                        {
                            hasAutoIgnition = true;
                            //if (debugMode) Debug.Log($"Auto-ignition reached for {compound.Name} at {roomTempC}°C");
                            break;
                        }
                    }
                }

                if (!hasIgnition && !hasAutoIgnition)
                {
                    //if (debugMode) Debug.Log($"Reaction {reaction.ID} requires ignition but none present (temp: {FahrenheitToCelsius(atmosphereController.Temperature):F1}°C, spark: {hasIgnitionSource})");
                    return false;
                }
            }

            // If we have ignition (spark), bypass temperature requirements
            // If no ignition, check temperature range normally
            if (!hasIgnition && !hasAutoIgnition)
            {
                float roomTempC = FahrenheitToCelsius(atmosphereController.Temperature);
                if (roomTempC < reaction.Conditions.MinTemperature || roomTempC > reaction.Conditions.MaxTemperature)
                {
                    //if (debugMode) Debug.Log($"Reaction {reaction.ID} temperature out of range: {roomTempC}°C (need {reaction.Conditions.MinTemperature}-{reaction.Conditions.MaxTemperature}°C)");
                    return false;
                }
            }

            float totalGasVolume = 0f;
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

            if (totalGasVolume <= 0f)
            {
                if (debugMode) Debug.Log($"Reaction {reaction.ID} no gases present");
                return false;
            }

            // Check for inhibitors
            if (reaction.Conditions.InhibitedBy != null && reaction.Conditions.InhibitedBy.Count > 0)
            {
                float totalInhibitorVolume = 0f;

                foreach (Fluid fluid in atmosphereController.RoomFluids)
                {
                    if (fluid.GetState() == FluidState.Gas || fluid.GetState() == FluidState.Mixed)
                    {
                        if (reaction.Conditions.InhibitedBy.Contains(fluid.GetIDName()))
                        {
                            float gasVolume = fluid.GetVolume();
                            if (fluid.GetState() == FluidState.Mixed)
                            {
                                gasVolume *= fluid.GetQuality();
                            }
                            totalInhibitorVolume += gasVolume;
                        }
                    }
                }

                float inhibitorPercentage = (totalInhibitorVolume / totalGasVolume) * 100f;
                if (inhibitorPercentage >= reaction.Conditions.InhibitorThreshold)
                {
                    if (debugMode) Debug.Log($"Reaction {reaction.ID} inhibited by {inhibitorPercentage:F1}% inhibitor gases");
                    return false;
                }
            }

            // Check that all reactants are present in sufficient quantities
            foreach (var reactant in reaction.Reactants)
            {
                Fluid gas = FindGasInRoom(reactant.Compound);
                if (gas == null)
                {
                    return false;
                }

                // Calculate concentration as percentage of total gases
                float gasVolume = gas.GetVolume();
                if (gas.GetState() == FluidState.Mixed)
                {
                    gasVolume *= gas.GetQuality();
                }
                float concentrationFraction = gasVolume / totalGasVolume;
                float concentrationPercentage = concentrationFraction * 100f;

                if (debugMode)
                {
                    Debug.Log($"Checking {reactant.Compound}: {concentrationPercentage:F2}% (need {reactant.MinConcentration * 100f:F2}%)");
                }

                if (concentrationFraction < reactant.MinConcentration)
                {
                    if (debugMode) Debug.Log($"Reaction {reaction.ID} insufficient {reactant.Compound}: {concentrationPercentage:F2}% < {reactant.MinConcentration * 100f:F2}%");
                    return false;
                }
            }

            if (debugMode) Debug.Log($"All conditions met for reaction {reaction.ID} (ignition: {hasIgnition}, auto-ignition: {hasAutoIgnition})");
            return true;
        }

        /// <summary>
        /// Check if all conditions are met for a reaction to occur
        /// </summary>
        private bool CanReactionOccur_(ReactionData reaction)
        {
            // Check pressure range
            float roomPressure = atmosphereController.Pressure;
            if (roomPressure < reaction.Conditions.MinPressure || roomPressure > reaction.Conditions.MaxPressure)
            {
                //if (debugMode) Debug.Log($"Reaction {reaction.ID} pressure out of range: {roomPressure} atm (need {reaction.Conditions.MinPressure}-{reaction.Conditions.MaxPressure})");
                return false;
            }

            // Check ignition requirement
            bool hasIgnition = hasIgnitionSource;
            bool hasAutoIgnition = false;

            if (reaction.Conditions.RequiresIgnition)
            {
                if (!hasIgnition)
                {
                    // Check if any reactant has reached auto-ignition temperature
                    float roomTempC = FahrenheitToCelsius(atmosphereController.Temperature);
                    foreach (var reactant in reaction.Reactants)
                    {
                        CompoundData compound = ChemistryDatabase.GetCompound(reactant.Compound);
                        if (compound != null && compound.AutoIgnitionTemp > 0 && roomTempC >= compound.AutoIgnitionTemp)
                        {
                            hasAutoIgnition = true;
                            //if (debugMode) Debug.Log($"Auto-ignition reached for {compound.Name} at {roomTempC}°C");
                            break;
                        }
                    }
                }

                if (!hasIgnition && !hasAutoIgnition)
                {
                    //if (debugMode) Debug.Log($"Reaction {reaction.ID} requires ignition but none present (temp: {FahrenheitToCelsius(atmosphereController.Temperature):F1}°C, spark: {hasIgnitionSource})");
                    return false;
                }
            }

            // If we have ignition (spark), bypass temperature requirements
            // If no ignition, check temperature range normally
            if (!hasIgnition && !hasAutoIgnition)
            {
                float roomTempC = FahrenheitToCelsius(atmosphereController.Temperature);
                if (roomTempC < reaction.Conditions.MinTemperature || roomTempC > reaction.Conditions.MaxTemperature)
                {
                    //if (debugMode) Debug.Log($"Reaction {reaction.ID} temperature out of range: {roomTempC}°C (need {reaction.Conditions.MinTemperature}-{reaction.Conditions.MaxTemperature}°C)");
                    return false;
                }
            }

            // Calculate total gas concentration for percentage calculations
            float totalGasConcentration = 0f;
            foreach (IGas gas in atmosphereController.RoomGassesLegacy)
            {
                totalGasConcentration += gas.GetConcentration();
            }

            if (totalGasConcentration <= 0f)
            {
                if (debugMode) Debug.Log($"Reaction {reaction.ID} no gases present");
                return false;
            }

            // Check for inhibitors
            if (reaction.Conditions.InhibitedBy != null && reaction.Conditions.InhibitedBy.Count > 0)
            {
                float totalInhibitorConcentration = 0f;

                foreach (IGas gas in atmosphereController.RoomGassesLegacy)
                {
                    if (reaction.Conditions.InhibitedBy.Contains(gas.GetIDName()))
                    {
                        totalInhibitorConcentration += gas.GetConcentration();
                    }
                }

                float inhibitorPercentage = (totalInhibitorConcentration / totalGasConcentration) * 100f;
                if (inhibitorPercentage >= reaction.Conditions.InhibitorThreshold)
                {
                    if (debugMode) Debug.Log($"Reaction {reaction.ID} inhibited by {inhibitorPercentage:F1}% inhibitor gases");
                    return false;
                }
            }

            // Check that all reactants are present in sufficient quantities
           /* foreach (var reactant in reaction.Reactants)
            {
                IGas gas = FindGasInRoom(reactant.Compound);
                if (gas == null)
                {
                    //if (debugMode) Debug.Log($"Reaction {reaction.ID} missing reactant: {reactant.Compound}");
                    return false;
                }

                // Calculate concentration as percentage of total gases
                float concentrationFraction = gas.GetConcentration() / totalGasConcentration;
                float concentrationPercentage = concentrationFraction * 100f;

                if (debugMode)
                {
                    Debug.Log($"Checking {reactant.Compound}: {concentrationPercentage:F2}% (need {reactant.MinConcentration * 100f:F2}%)");
                }

                // MinConcentration in XML is stored as fraction (0.05 = 5%)
                if (concentrationFraction < reactant.MinConcentration)
                {
                    if (debugMode) Debug.Log($"Reaction {reaction.ID} insufficient {reactant.Compound}: {concentrationPercentage:F2}% < {reactant.MinConcentration * 100f:F2}%");
                    return false;
                }
            }*/

            if (debugMode) Debug.Log($"All conditions met for reaction {reaction.ID} (ignition: {hasIgnition}, auto-ignition: {hasAutoIgnition})");
            return true;
        }

        //A2
        private void ExecuteReaction(ReactionData reaction)
        {
            if (debugMode) Debug.Log($"=== EXECUTING REACTION: {reaction.Name} ===");

            // Find limiting reagent based on moles present
            float limitingMoles = float.MaxValue;
            string limitingReagent = "";

            foreach (var reactant in reaction.Reactants)
            {
                Fluid gas = FindGasInRoom(reactant.Compound);
                if (gas == null) continue;

                CompoundData compound = ChemistryDatabase.GetCompound(reactant.Compound);
                if (compound == null) continue;

                // Get mass of gas in kg and convert to moles
                float massKg = gas.GetMass();
                float moles = (massKg * 1000f) / compound.MolarMass; // kg to g, then to moles

                // Clamp to prevent negative values
                moles = Mathf.Max(0f, moles);

                // How many moles of reaction can this support?
                float possibleReactionMoles = moles / reactant.Coefficient;

                if (possibleReactionMoles < limitingMoles)
                {
                    limitingMoles = possibleReactionMoles;
                    limitingReagent = compound.Name;
                }

                if (debugMode)
                {
                    Debug.Log($"Reactant {compound.Name}: {massKg:F6}kg = {moles:F6} moles, supports {possibleReactionMoles:F6} reaction moles");
                }
            }

            if (limitingMoles <= 0f || limitingMoles == float.MaxValue)
            {
                if (debugMode) Debug.Log("No limiting reagent found or zero moles available");
                return;
            }

            if (debugMode) Debug.Log($"Limiting reagent: {limitingReagent} ({limitingMoles:F6} moles)");

            // Apply reaction rate
            float reactionExtent = limitingMoles * reactionRateMultiplier * reactionCheckInterval;

            // Cap reaction to prevent instability
            reactionExtent = Mathf.Min(reactionExtent, limitingMoles * 0.3f);

            if (reactionExtent <= 0.0001f)
            {
                if (debugMode) Debug.Log("Reaction extent too small, skipping");
                return;
            }

            if (debugMode) Debug.Log($"Reaction extent: {reactionExtent:F4} moles");

            // Consume reactants by removing mass
            // Does not remove the proper mass, for some reason
            /*
            Added Methane: 10.0% of room volume (5.1123 kg)
            Reactant Methane: 5.112333kg = 318.724000 moles, supports 318.724000 reaction moles
            Consumed 95.617200 moles (1.533700kg) of Methane
            Reactant Methane: 1.475162kg = 91.967680 moles, supports 91.967680 reaction moles
            Consumed 27.590310 moles (0.442549kg) of Methane
            Reactant Methane: 2.103471kg = 131.139100 moles, supports 131.139100 reaction moles
            Consumed 39.341730 moles (0.631041kg) of Methane
            Reactant Methane: 1.032613kg = 64.377370 moles, supports 64.377370 reaction moles
             */
            foreach (var reactant in reaction.Reactants)
            {
                float molesToConsume = reactionExtent * reactant.Coefficient;
                CompoundData compound = ChemistryDatabase.GetCompound(reactant.Compound);
                if (compound == null) continue;

                float massToRemove = (molesToConsume * compound.MolarMass) / 1000f;

                // Create a fluid object representing what we're removing
                float tempK = FahrenheitToKelvin(atmosphereController.Temperature);
                Fluid gasToRemove = new Fluid(reactant.Compound, massToRemove, tempK, atmosphereController.RoomVolume, atmosphereController.Pressure);

                // Use the proper removal method
                atmosphereController.RemoveRoomGas(gasToRemove);

                if (debugMode)
                {
                    Debug.Log($"Consumed {molesToConsume:F6} moles ({massToRemove:F6}kg) of {reactant.Compound}");
                }
            }

            // Produce products
            float avgTemp = atmosphereController.Temperature;
            float avgPressure = atmosphereController.Pressure;

            foreach (var product in reaction.Products)
            {
                float molesToProduce = reactionExtent * product.Coefficient;
                ProduceGas(product.Compound, molesToProduce, avgTemp, avgPressure);
            }

            // Apply heat change (negative = exothermic = adds heat)
            float totalHeatReleased = -reactionExtent * reaction.Energetics.EnthalpyChange;
            atmosphereController.AddRoomHeat(totalHeatReleased, true);

            if (debugMode)
            {
                Debug.Log($"Heat released: {totalHeatReleased:F0} J ({(reaction.Energetics.EnthalpyChange < 0 ? "exothermic" : "endothermic")})");
            }

            // Apply contamination as specific particulates
            if (reaction.Effects.ContaminationPerMol > 0)
            {
                float contaminationAdded = reactionExtent * reaction.Effects.ContaminationPerMol;

                // Combustion reactions produce soot
                if (reaction.ID.Contains("combustion") || reaction.Name.Contains("Combustion"))
                {
                    atmosphereController.AddParticulate("soot", contaminationAdded);
                }
                // Explosions produce ash and carbon
                else if (reaction.Effects.ExplosionPotential > 5)
                {
                    atmosphereController.AddParticulate("ash", contaminationAdded * 0.6f);
                    atmosphereController.AddParticulate("carbon_black", contaminationAdded * 0.4f);

                    // Metal oxidation from explosions
                    if (reaction.Products.Exists(p => ChemistryDatabase.GetCompound(p.Compound)?.Name.Contains("Oxide") ?? false))
                    {
                        atmosphereController.AddParticulate("metal_oxide", contaminationAdded * 0.2f);
                    }
                }
                // Default to generic organic dust
                else
                {
                    atmosphereController.AddParticulate("organic_dust", contaminationAdded);
                }

                if (debugMode)
                {
                    Debug.Log($"Added {contaminationAdded:F2} ppmw of particulates");
                }
            }

            // Handle explosion potential
            if (reaction.Effects.ExplosionPotential > 5 && reactionExtent > 0.1f)
            {
                HandleExplosionEffect(reaction.Effects.ExplosionPotential, reactionExtent);
            }

            if (debugMode) Debug.Log("=== REACTION COMPLETE ===\n");
        }

        //B1
        private void ExecuteReaction_(ReactionData reaction)
        {
            if (debugMode) Debug.Log($"=== EXECUTING REACTION: {reaction.Name} ===");

            // Find limiting reagent based on moles present
            float limitingMoles = float.MaxValue;
            string limitingReagent = "";

            foreach (var reactant in reaction.Reactants)
            {
                Fluid gas = FindGasInRoom(reactant.Compound);
                if (gas == null) continue;

                CompoundData compound = ChemistryDatabase.GetCompound(reactant.Compound);
                if (compound == null) continue;

                // Get mass of gas in kg and convert to moles
                float massKg = gas.GetMass();
                float moles = (massKg * 1000f) / compound.MolarMass; // kg to g, then to moles

                // Clamp to prevent negative values
                moles = Mathf.Max(0f, moles);

                // How many moles of reaction can this support?
                float possibleReactionMoles = moles / reactant.Coefficient;

                if (possibleReactionMoles < limitingMoles)
                {
                    limitingMoles = possibleReactionMoles;
                    limitingReagent = compound.Name;
                }

                if (debugMode)
                {
                    Debug.Log($"Reactant {compound.Name}: {massKg:F6}kg = {moles:F6} moles, supports {possibleReactionMoles:F6} reaction moles");
                }
            }

            if (limitingMoles <= 0f || limitingMoles == float.MaxValue)
            {
                if (debugMode) Debug.Log("No limiting reagent found or zero moles available");
                return;
            }

            if (debugMode) Debug.Log($"Limiting reagent: {limitingReagent} ({limitingMoles:F6} moles)");

            // Apply reaction rate
            float reactionExtent = limitingMoles * reactionRateMultiplier * reactionCheckInterval;

            // Cap reaction to prevent instability
            reactionExtent = Mathf.Min(reactionExtent, limitingMoles * 0.3f);

            if (reactionExtent <= 0.0001f)
            {
                if (debugMode) Debug.Log("Reaction extent too small, skipping");
                return;
            }

            if (debugMode) Debug.Log($"Reaction extent: {reactionExtent:F4} moles");

            // Consume reactants by removing mass
            foreach (var reactant in reaction.Reactants)
            {
                Fluid gas = FindGasInRoom(reactant.Compound);
                if (gas == null) continue;

                float molesToConsume = reactionExtent * reactant.Coefficient;
                float massToRemove = (molesToConsume * gas.GetMolarMass()) / 1000f; // moles to kg

                gas.SetMass(gas.GetMass() - massToRemove);

                if (debugMode)
                {
                    Debug.Log($"Consumed {molesToConsume:F6} moles ({massToRemove:F6}kg) of {reactant.Compound}");
                }
            }

            // Produce products
            float avgTemp = atmosphereController.Temperature;
            float avgPressure = atmosphereController.Pressure;

            foreach (var product in reaction.Products)
            {
                float molesToProduce = reactionExtent * product.Coefficient;
                ProduceGas(product.Compound, molesToProduce, avgTemp, avgPressure);
            }

            // Apply heat change (negative = exothermic = adds heat)
            float totalHeatReleased = -reactionExtent * reaction.Energetics.EnthalpyChange;
            atmosphereController.AddRoomHeat(totalHeatReleased, true);

            if (debugMode)
            {
                Debug.Log($"Heat released: {totalHeatReleased:F0} J ({(reaction.Energetics.EnthalpyChange < 0 ? "exothermic" : "endothermic")})");
            }

            // Apply contamination as specific particulates
            if (reaction.Effects.ContaminationPerMol > 0)
            {
                float contaminationAdded = reactionExtent * reaction.Effects.ContaminationPerMol;

                // Combustion reactions produce soot
                if (reaction.ID.Contains("combustion") || reaction.Name.Contains("Combustion"))
                {
                    atmosphereController.AddParticulate("soot", contaminationAdded);
                }
                // Explosions produce ash and carbon
                else if (reaction.Effects.ExplosionPotential > 5)
                {
                    atmosphereController.AddParticulate("ash", contaminationAdded * 0.6f);
                    atmosphereController.AddParticulate("carbon_black", contaminationAdded * 0.4f);

                    // Metal oxidation from explosions
                    if (reaction.Products.Exists(p => ChemistryDatabase.GetCompound(p.Compound)?.Name.Contains("Oxide") ?? false))
                    {
                        atmosphereController.AddParticulate("metal_oxide", contaminationAdded * 0.2f);
                    }
                }
                // Default to generic organic dust
                else
                {
                    atmosphereController.AddParticulate("organic_dust", contaminationAdded);
                }

                if (debugMode)
                {
                    Debug.Log($"Added {contaminationAdded:F2} ppmw of particulates");
                }
            }

            // Handle explosion potential
            if (reaction.Effects.ExplosionPotential > 5 && reactionExtent > 0.1f)
            {
                HandleExplosionEffect(reaction.Effects.ExplosionPotential, reactionExtent);
            }

            if (debugMode) Debug.Log("=== REACTION COMPLETE ===\n");
        }

        private void ProduceGas(string compoundID, float moles, float temperature, float pressure)
        {
            CompoundData compound = ChemistryDatabase.GetCompound(compoundID);
            if (compound == null) return;

            // Convert moles to mass in kg
            float massKg = (moles * compound.MolarMass) / 1000f;

            // Convert temperature to Kelvin
            float tempK = FahrenheitToKelvin(temperature);

            // Create and add the gas
            Fluid newGas = new Fluid(compoundID, massKg, tempK, atmosphereController.RoomVolume, pressure);
            atmosphereController.AddRoomGas(newGas);

            if (debugMode) Debug.Log($"Produced {moles:F4} moles ({massKg:F4} kg) of {compound.Name}");
        }

        private Fluid FindGasInRoom(string compoundID)
        {
            foreach (Fluid fluid in atmosphereController.RoomFluids)
            {
                if ((fluid.GetState() == FluidState.Gas || fluid.GetState() == FluidState.Mixed)
                    && fluid.GetIDName() == compoundID)
                {
                    return fluid;
                }
            }
            return null;
        }

        //A
        private void ProduceGasA(string compoundID, float moles, float temperature, float pressure)
        {
            CompoundData compound = ChemistryDatabase.GetCompound(compoundID);
            if (compound == null) return;

            // Convert moles to mass
            float massKg = (moles * compound.MolarMass) / 1000f; // g to kg

            // Convert temperature to Kelvin
            float tempK = FahrenheitToKelvin(temperature);

            // Create and add the gas
            Fluid newGas = new Fluid(compoundID, massKg, tempK, atmosphereController.RoomVolume, pressure);
            atmosphereController.AddRoomGas(newGas);

            if (debugMode) Debug.Log($"Produced {moles:F4} moles ({massKg:F4} kg) of {compound.Name}");
        }

        /// <summary>
        /// Produce a gas and add it to the room
        /// </summary>
        private void ProduceGas_(string compoundID, float moles, float temperature, float pressure)
        {
            CompoundData compound = ChemistryDatabase.GetCompound(compoundID);
            if (compound == null) return;

            // Convert moles to volume: V = (n * R * T) / P
            float tempK = FahrenheitToKelvin(temperature);
            float volumeL = (moles * 0.0821f * tempK) / pressure;
            float volumeM3 = volumeL / 1000f;

            // Create and add the gas
            IGas newGas = new IGas(
                compoundID,
                temperature,
                volumeM3,
                pressure,
                atmosphereController.RoomVolume
            );

            atmosphereController.AddRoomGas(newGas);

            if (debugMode) Debug.Log($"Produced {moles:F4} moles ({volumeM3:F4} m³) of {compound.Name}");
        }

        //A
        // Replace the FindGasInRoom method:
        private Fluid FindGasInRoomA(string compoundID)
        {
            foreach (Fluid fluid in atmosphereController.RoomFluids)
            {
                if (fluid.GetIDName() == compoundID)
                {
                    // Only return if it's in a gaseous state (or partially gaseous)
                    if (fluid.GetState() == FluidState.Gas || fluid.GetState() == FluidState.Mixed)
                    {
                        return fluid;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Find a specific gas in the room
        /// </summary>
        private IGas FindGasInRoom_(string compoundID)
        {
            foreach (IGas gas in atmosphereController.RoomGassesLegacy)
            {
                if (gas.GetIDName() == compoundID)
                {
                    return gas;
                }
            }
            return null;
        }

        /// <summary>
        /// Handle explosion effects
        /// </summary>
        // Modified HandleExplosionEffect method
        private void HandleExplosionEffect(int explosionPotential, float reactionExtent)
        {
            if (debugMode)
            {
                Debug.LogWarning($"EXPLOSION! Potential: {explosionPotential}/10, Extent: {reactionExtent:F4} moles");
            }

            float pressureIncrease = explosionPotential * reactionExtent * 0.05f;
            Debug.Log(pressureIncrease);
            atmosphereController.AddPressure(pressureIncrease);

            float explosionHeat = explosionPotential * reactionExtent * 50000f;

            float particulateAmount = explosionPotential * reactionExtent * 10f;
            atmosphereController.AddParticulate("ash", particulateAmount * 0.5f);
            atmosphereController.AddParticulate("carbon_black", particulateAmount * 0.3f);
            atmosphereController.AddParticulate("metal_dust", particulateAmount * 0.2f);

            // Spawn fires from explosion via singleton manager
            if (Hazards.HazardIntegrationManager.Instance != null)
            {
                Hazards.HazardIntegrationManager.Instance.SpawnFiresFromExplosion(
                    atmosphereController, explosionHeat, explosionPotential, transform.position);
            }

            atmosphereController.CheckPressureRupture();
            atmosphereController.CheckExplosionPropagation(pressureIncrease, explosionHeat);
            StartCoroutine(ResetExplosionStateDelayed(1f));

            //TODO: Apply damage and vfx
        }

        private System.Collections.IEnumerator ResetExplosionStateDelayed(float delay)
        {
            yield return new WaitForSeconds(delay);
            atmosphereController.ResetExplosionState();
        }

        // Utility functions
        private float FahrenheitToCelsius(float fahrenheit)
        {
            return (fahrenheit - 32f) * (5f / 9f);
        }

        private float CelsiusToFahrenheit(float celsius)
        {
            return (celsius * 9f / 5f) + 32f;
        }

        private float FahrenheitToKelvin(float fahrenheit)
        {
            return FahrenheitToCelsius(fahrenheit) + 273.15f;
        }

        // Public methods for external control
        public void TriggerIgnition(float duration = 0f)
        {
            hasIgnitionSource = true;
            ignitionSourceDuration = duration;
            ignitionTimer = 0f;
        }

        public void ExtinguishIgnition()
        {
            hasIgnitionSource = false;
            ignitionTimer = 0f;
            if (debugMode) Debug.Log("Ignition source extinguished");
        }

        //A
        public void SetRoomTemperatureA(float temperatureCelsius)
        {
            float tempF = CelsiusToFahrenheit(temperatureCelsius);
            float tempK = temperatureCelsius + 273.15f;
            atmosphereController.Temperature = tempF;

            // Update all fluid temperatures
            foreach (Fluid fluid in atmosphereController.RoomFluids)
            {
                fluid.SetTemperature(tempK);
            }

            if (debugMode) Debug.Log($"Room temperature set to {temperatureCelsius}°C ({tempF:F1}°F)");
        }

        //Original, B
        public void SetRoomTemperature(float temperatureCelsius)
        {
            float tempF = CelsiusToFahrenheit(temperatureCelsius);
            atmosphereController.Temperature = tempF;

            // Update all gas temperatures
            foreach (IGas gas in atmosphereController.RoomGassesLegacy)
            {
                gas.SetTemp(tempF);
            }

            if (debugMode) Debug.Log($"Room temperature set to {temperatureCelsius}°C ({tempF:F1}°F)");
        }

        public bool HasIgnitionSource()
        {
            return hasIgnitionSource;
        }
    }
}