using System;
using UnityEngine;
using ProjectUniverse.Data.Libraries;
using ProjectUniverse.Data.Libraries.Definitions;

namespace ProjectUniverse.Environment.Fluids
{
    public enum FluidState
    {
        Liquid,
        Gas,
        Mixed // Partially vaporized (wet steam, etc.)
    }

    public class Fluid
    {
        // Constants
        private const float R_UNIVERSAL_J = 8.314f; // J/(mol·K)
        private const float R_UNIVERSAL_LATM = 0.08206f; // L·atm/(mol·K)
        private const float KELVIN_OFFSET = 273.15f;

        // Definition properties (from XML)
        private string idName;
        private int flammability;
        private int combustibility;
        private bool nuclear;
        private float toxicity;
        private float molarMass; // g/mol
        private float boilingPoint; // K at 1 atm
        private float specificHeatLiquid; // J/(kg·K)
        private float specificHeatGas; // J/(kg·K)
        private float enthalpyVaporization; // J/kg
        private float liquidDensity; // kg/m³ at STP

        // Instance properties
        private float mass; // kg (primary quantity)
        private float temperature; // K
        private float pressure; // atm
        private float quality; // 0-1, vapor fraction for mixed state
        private float gasVolume; // m^3 The volume of the container in which there is gas.
        private float liquidVolume; // m^3 The volume of liquid in the container.
        private FluidState currentState;
        private FluidDefinition definition;

        // Cached calculations
        private float cachedEnthalpy;
        private bool needsRecalculation = true;

        public Fluid(string fluidID, float massKg, float tempK, float containerVolume, float pressureAtm = 1f)
        {
            idName = fluidID;
            mass = massKg;
            temperature = tempK;
            pressure = pressureAtm;
            gasVolume = containerVolume;

            LoadDefinition();
            UpdateState();
            needsRecalculation = true;
            //Debug.Log($"M {idName}, {pressure}");
        }

        public Fluid(Fluid other)
        {
            idName = other.idName;
            mass = other.mass;
            temperature = other.temperature;
            pressure = other.pressure;
            quality = other.quality;
            currentState = other.currentState;
            gasVolume = other.gasVolume;

            // Copy definition properties
            flammability = other.flammability;
            combustibility = other.combustibility;
            nuclear = other.nuclear;
            toxicity = other.toxicity;
            molarMass = other.molarMass;
            boilingPoint = other.boilingPoint;
            specificHeatLiquid = other.specificHeatLiquid;
            specificHeatGas = other.specificHeatGas;
            enthalpyVaporization = other.enthalpyVaporization;
            liquidDensity = other.liquidDensity;
            definition = other.definition;

            needsRecalculation = true;
        }

        private void LoadDefinition()
        {
            // Try to load from unified FluidLibrary first
            if (FluidLibrary.FluidDictionary != null &&
                FluidLibrary.FluidDictionary.TryGetValue(idName, out definition))
            {
                flammability = definition.Flammability;
                combustibility = definition.Combustibility;
                nuclear = definition.IsNuclear;
                toxicity = definition.Toxicity;
                molarMass = definition.MolarMass;
                boilingPoint = definition.BoilingPoint;
                specificHeatLiquid = definition.SpecificHeatLiquid;
                specificHeatGas = definition.SpecificHeatGas;
                enthalpyVaporization = definition.EnthalpyVaporization;
                liquidDensity = definition.LiquidDensity;
            }
            else
            {
                // Default values for water if definition not found
                Debug.Log($"Fluid definition not found for {idName}, using water defaults");
                molarMass = 18.02f;
                boilingPoint = 373.15f;
                specificHeatLiquid = 4186f;
                specificHeatGas = 2010f;
                enthalpyVaporization = 2257000f;
                liquidDensity = 1000f;
            }
        }

        /// <summary>
        /// Update the current phase state based on temperature and pressure
        /// </summary>
        private void UpdateState()
        {
            float adjustedBoilingPoint = GetPressureAdjustedBoilingPoint();

            if (temperature < adjustedBoilingPoint - 0.1f)
            {
                currentState = FluidState.Liquid;
                quality = 0f;
            }
            else if (temperature > adjustedBoilingPoint + 0.1f)
            {
                currentState = FluidState.Gas;
                quality = 1f;
            }
            else
            {
                // At boiling point - state depends on energy content
                currentState = FluidState.Mixed;
                // Quality will be set by enthalpy calculations
            }

            // Update pressure here?

        }

        /// <summary>
        /// Get boiling point adjusted for current pressure using Clausius-Clapeyron approximation
        /// </summary>
        private float GetPressureAdjustedBoilingPoint()
        {
            // Simplified: BP changes ~10°C per doubling/halving of pressure
            float pressureRatio = pressure / 1f; // relative to 1 atm
            float tempAdjustment = 10f * Mathf.Log(pressureRatio, 2f);
            return boilingPoint + tempAdjustment;
        }

        /// <summary>
        /// All Fluids will be inside a container of some size, so if a Fluid has or is in a gas state
        /// it must have a volume <= the size of the container.
        /// </summary>
        /// <param name="contVolume"></param>
        public void SetContainerVolume(float contVolume)
        {
            gasVolume = contVolume;
        }

        /// <summary>
        /// Calculate volume based on current state and conditions
        /// </summary>
        public float GetVolume()
        {
            float volume = 0f;

            switch (currentState)
            {
                case FluidState.Liquid:
                    // Incompressible liquid
                    liquidVolume = mass / liquidDensity; // m³
                    volume = liquidVolume;
                    //Debug.Log($"L {mass}");
                    break;

                case FluidState.Gas:
                    // Gas will expand to fill container
                    volume = gasVolume;
                    //Debug.Log($"Gas {volume}");
                    //Debug.Log($"G {moles}, {pressure}");
                    break;

                case FluidState.Mixed:
                    // Weighted average of liquid and gas volumes
                    float liquidMass = mass * (1f - quality);

                    liquidVolume = liquidMass / liquidDensity;
                    // gasVolume is the maximium size of the container - volume cannot be greater
                    // The gas will take up whatever volume the liquid does not
                    volume = gasVolume; //liquidVolume + gasVolume;
                    //Debug.Log($"Mixed {volume}");
                    //Debug.Log($"M {gasMoles}, {pressure}");
                    break;
            }

            needsRecalculation = false;
            return volume;
        }

        // The pressure should likely be calculated by the Fluid and set back to room/pipe.
        // That said, this function still has uses with pumps and liquid state fluids
        public void SetPressure(float newPressure)
        {
            pressure = newPressure;
            UpdateState();
            needsRecalculation = true;
        }

        /// <summary>
        /// Get the pressure of the fluid. 
        /// </summary>
        /// <returns></returns>
        public float GetPressure()
        {
            float Fpressure = 0f;
            switch (currentState)
            {
                case FluidState.Liquid:
                    // Incompressible liquid
                    Fpressure = pressure;
                    break;

                case FluidState.Gas:
                    // This calculation does not allow for pumps and pressurizers
                    // Ideal gas law: P = nRT/V
                    float moles = (mass * 1000f) / molarMass; // convert kg to g then to moles
                    Fpressure = (moles * R_UNIVERSAL_LATM * temperature) / gasVolume / 1000f; // L to m³
                    //Debug.Log($"G {moles}, {pressure}");
                    break;

                case FluidState.Mixed:

                    float gasMoles = (mass * 1000f) / molarMass; // convert kg to g then to moles
                    float gasPressure = (gasMoles * R_UNIVERSAL_LATM * temperature) / gasVolume / 1000f; // L to m³
                    // Weighted average of liquid and gas pressures. They should be close to eachother, except for in pumps.
                    Fpressure = (pressure * (1f - quality)) + gasPressure * quality;
                    //Debug.Log($"M {pressure}");
                    break;
            }
            return Fpressure;
        }

        /// <summary>
        /// Calculate total enthalpy of the fluid
        /// </summary>
        public float GetEnthalpy()
        {
            if (!needsRecalculation && cachedEnthalpy != 0)
                return cachedEnthalpy;

            float enthalpy = 0f;
            float referenceTemp = 273.15f; // 0°C reference

            switch (currentState)
            {
                case FluidState.Liquid:
                    // Sensible heat only
                    enthalpy = mass * specificHeatLiquid * (temperature - referenceTemp);
                    break;

                case FluidState.Gas:
                    // Sensible heat to boiling + latent heat + sensible heat as gas
                    enthalpy = mass * specificHeatLiquid * (boilingPoint - referenceTemp);
                    enthalpy += mass * enthalpyVaporization;
                    enthalpy += mass * specificHeatGas * (temperature - boilingPoint);
                    break;

                case FluidState.Mixed:
                    // Liquid portion + vaporized portion
                    float liquidEnthalpy = mass * specificHeatLiquid * (temperature - referenceTemp);
                    float vaporEnthalpy = mass * quality * enthalpyVaporization;
                    enthalpy = liquidEnthalpy + vaporEnthalpy;
                    break;
            }

            cachedEnthalpy = enthalpy;
            return enthalpy;
        }

        public float GetDensity()
        {
            switch (currentState)
            {
                case FluidState.Liquid:
                    return liquidDensity;

                case FluidState.Gas:
                    if (gasVolume != 0)
                    {
                        return mass / gasVolume;
                    }
                    else if (liquidVolume != 0)
                    {
                        return mass / liquidVolume;
                    }
                    else
                    {
                        return liquidDensity;
                    }
                case FluidState.Mixed:
                    //Average the Densities
                    float gasMass = mass * quality;
                    if (gasVolume != 0)
                    {
                        return (liquidDensity * (1f - quality)) + ((gasMass / gasVolume) * quality);
                    }
                    else if (liquidVolume != 0)
                    {
                        return (liquidDensity * (1f - quality)) + ((gasMass / liquidVolume) * quality);
                    }
                    else
                    {
                        return liquidDensity;
                    }
            }
            // This case will never be reached; fallback from/for switch just in case.
            return liquidDensity;
        }

        /// <summary>
        /// Add thermal energy to the fluid
        /// </summary>
        public void AddEnergy(float joules)
        {
            float totalEnthalpy = GetEnthalpy() + joules;
            SetFromEnthalpy(totalEnthalpy);
        }

        /// <summary>
        /// Remove thermal energy from the fluid
        /// </summary>
        public void RemoveEnergy(float joules)
        {
            float totalEnthalpy = GetEnthalpy() - joules;
            SetFromEnthalpy(totalEnthalpy);
        }

        /// <summary>
        /// Set state from total enthalpy
        /// </summary>
        private void SetFromEnthalpy(float totalEnthalpy)
        {
            float referenceTemp = 273.15f;
            float adjustedBoilingPoint = GetPressureAdjustedBoilingPoint();

            // Energy to heat liquid to boiling
            float energyToBoiling = mass * specificHeatLiquid * (adjustedBoilingPoint - referenceTemp);

            // Energy to fully vaporize
            float energyToVaporize = mass * enthalpyVaporization;

            if (totalEnthalpy <= energyToBoiling)
            {
                // Pure liquid
                currentState = FluidState.Liquid;
                quality = 0f;
                temperature = referenceTemp + (totalEnthalpy / (mass * specificHeatLiquid));
            }
            else if (totalEnthalpy >= energyToBoiling + energyToVaporize)
            {
                // Pure gas
                currentState = FluidState.Gas;
                quality = 1f;
                float excessEnergy = totalEnthalpy - energyToBoiling - energyToVaporize;
                temperature = adjustedBoilingPoint + (excessEnergy / (mass * specificHeatGas));
            }
            else
            {
                // Mixed state
                currentState = FluidState.Mixed;
                temperature = adjustedBoilingPoint;
                float vaporizationProgress = totalEnthalpy - energyToBoiling;
                quality = vaporizationProgress / energyToVaporize;
            }

            cachedEnthalpy = totalEnthalpy;
            needsRecalculation = true;
        }

        /// <summary>
        /// Mix this fluid with another fluid of the same type
        /// </summary>
        public static Fluid Mix(Fluid fluid1, Fluid fluid2)
        {
            if (fluid1.idName != fluid2.idName)
            {
                Debug.LogError($"Cannot mix different fluids: {fluid1.idName} and {fluid2.idName}");
                return null;
            }

            // Conservation of mass
            float totalMass = fluid1.mass + fluid2.mass;

            // Conservation of energy
            float totalEnthalpy = fluid1.GetEnthalpy() + fluid2.GetEnthalpy();

            // Average pressure (simplified - in reality would need flow dynamics)
            float avgPressure = (fluid1.pressure * fluid1.mass + fluid2.pressure * fluid2.mass) / totalMass;

            // Calculate pressure
            float gasMoles = (fluid1.mass * 1000f) / fluid1.molarMass;
            float combVolume;
            combVolume = (gasMoles * R_UNIVERSAL_LATM * fluid1.temperature) / fluid1.pressure / 1000f; // L to m³
            combVolume += (gasMoles * R_UNIVERSAL_LATM * fluid2.temperature) / fluid2.pressure / 1000f;

            // Create result fluid
            Fluid result = new Fluid(fluid1.idName, totalMass, 300f, combVolume, avgPressure); // temp will be overwritten
            result.SetFromEnthalpy(totalEnthalpy);

            return result;
        }

        /// <summary>
        /// Split off a portion of this fluid by mass
        /// </summary>
        public Fluid Split(float extractMass)
        {
            if (extractMass > mass)
                extractMass = mass;

            // The extracted portion has same properties
            Fluid extracted = new Fluid(this);
            extracted.mass = extractMass;

            // Reduce this fluid's mass
            mass -= extractMass;

            // Both portions maintain same temperature, pressure, and quality
            return extracted;
        }

        // Property accessors
        public string GetIDName() => idName;
        public float GetMass() => mass;
        public float GetTemperature() => temperature;
        public float GetTemperatureF() => (temperature - KELVIN_OFFSET) * 1.8f + 32f;
        public FluidState GetState() => currentState;
        public float GetQuality() => quality;
        public int GetFlammability() => flammability;
        public int GetCombustibility() => combustibility;
        public bool IsNuclear() => nuclear;
        public float GetToxicity() => toxicity;
        public float GetMolarMass() => molarMass;

        // Setters that require recalculation
        public void SetMass(float newMass)
        {
            mass = newMass;
            needsRecalculation = true;
        }

        public void SetTemperature(float newTempK)
        {
            temperature = newTempK;
            UpdateState();
            needsRecalculation = true;
        }

        public void SetTemperatureF(float newTempF)
        {
            temperature = (newTempF - 32f) / 1.8f + KELVIN_OFFSET;
            UpdateState();
            needsRecalculation = true;
        }

        public override string ToString()
        {
            string stateStr = currentState.ToString();
            if (currentState == FluidState.Mixed)
                stateStr = $"Mixed ({quality:P0} vapor)";

            return $"{idName} [{stateStr}]: {mass:F2}kg at {GetTemperatureF():F1}°F, {pressure:F2}atm, Volume: {GetVolume():F3}m³";
        }
    }
}