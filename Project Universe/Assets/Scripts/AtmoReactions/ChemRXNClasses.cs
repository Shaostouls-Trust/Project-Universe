using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUniverse.Environment.Chemistry
{
    [Serializable]
    public class CompoundData
    {
        public string ID;
        public string Name;

        // Physical Properties
        public float MolarMass;
        public float BoilingPoint; // Celsius
        public float SpecificHeat; // J/(kg·K)
        public float Density; // g/L at STP

        // Thermodynamics
        public float HeatOfFormation; // J/mol
        public float HeatOfVaporization; // J/mol

        // Reactivity
        public float AutoIgnitionTemp; // Celsius
        public float FlammabilityMin; // % by volume
        public float FlammabilityMax; // % by volume
        public bool IsInhibitor;

        // Safety (legacy from IGas)
        public int Flamability; // 0-10
        public int Combustability; // 0-10
        public float Toxicity; // 0-10
        public bool IsNuclear;
    }

    [Serializable]
    public class ReactantData
    {
        public string Compound;
        public float Coefficient;
        public float MinConcentration; // Minimum concentration required (m³/m³ or fraction)
    }

    [Serializable]
    public class ProductData
    {
        public string Compound;
        public float Coefficient;
    }

    [Serializable]
    public class ReactionConditions
    {
        public float MinTemperature; // Celsius
        public float MaxTemperature; // Celsius
        public float MinPressure; // atm
        public float MaxPressure; // atm
        public bool RequiresIgnition;
        public List<string> InhibitedBy; // Compound IDs
        public float InhibitorThreshold; // % by volume
    }

    [Serializable]
    public class ReactionEnergetics
    {
        public float EnthalpyChange; // J/mol (negative = exothermic)
        public float ActivationEnergy; // J/mol
    }

    [Serializable]
    public class ReactionEffects
    {
        public float ContaminationPerMol; // ppm per mole reacted
        public int ExplosionPotential; // 0-10 scale
    }

    [Serializable]
    public class ReactionData
    {
        public string ID;
        public string Name;
        public string Type; // Combustion, Decomposition, Redox, Synthesis
        public string Description;

        public List<ReactantData> Reactants;
        public List<ProductData> Products;
        public ReactionConditions Conditions;
        public ReactionEnergetics Energetics;
        public ReactionEffects Effects;
    }
}