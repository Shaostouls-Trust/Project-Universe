using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace ProjectUniverse.Environment.Chemistry
{
    public static class ChemistryDatabase
    {
        private static Dictionary<string, CompoundData> compoundDatabase;
        private static Dictionary<string, ReactionData> reactionDatabase;
        private static bool isInitialized = false;

        // Centralized lists of combustible and oxidizer compounds
        private static HashSet<string> combustibleGases;
        private static HashSet<string> oxidizerGases;

        public static Dictionary<string, CompoundData> CompoundDatabase => compoundDatabase;
        public static Dictionary<string, ReactionData> ReactionDatabase => reactionDatabase;

        public static void Initialize()
        {
            if (isInitialized) return;

            compoundDatabase = new Dictionary<string, CompoundData>();
            reactionDatabase = new Dictionary<string, ReactionData>();

            combustibleGases = new HashSet<string>();
            oxidizerGases = new HashSet<string>();
            InitializeCombustiblesAndOxidizers();

            LoadCompoundData();
            LoadReactionData();

            isInitialized = true;
            Debug.Log($"Chemistry Database Initialized: {compoundDatabase.Count} compounds, {reactionDatabase.Count} reactions");
        }

        private static void InitializeCombustiblesAndOxidizers()
        {
            // Combustible gases (fuels)
            combustibleGases.Add("Methane");   // CH4
            combustibleGases.Add("Hydrogen");    // H2
            combustibleGases.Add("C3H8");  // Propane
            combustibleGases.Add("C2H6");  // Ethane
            combustibleGases.Add("C4H10"); // Butane
            combustibleGases.Add("NH3");   // Ammonia
            combustibleGases.Add("CO");    // Carbon monoxide

            // Oxidizers
            oxidizerGases.Add("Oxygen");   // O2
            oxidizerGases.Add("N2O");  // Nitrous oxide
            oxidizerGases.Add("Chlorine");  // Cl2
            oxidizerGases.Add("Fluorine");   // F2
        }

        public static HashSet<string> GetCombustibleGases()
        {
            return new HashSet<string>(combustibleGases);
        }

        public static HashSet<string> GetOxidizerGases()
        {
            return new HashSet<string>(oxidizerGases);
        }

        public static bool IsCombustible(string compoundID)
        {
            return combustibleGases.Contains(compoundID);
        }

        public static bool IsOxidizer(string compoundID)
        {
            return oxidizerGases.Contains(compoundID);
        }

        private static void LoadCompoundData()
        {
            TextAsset xmlFile = Resources.Load<TextAsset>("CompoundData");
            if (xmlFile == null)
            {
                Debug.LogError("CompoundData.xml not found in Resources folder!");
                return;
            }

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlFile.text);

            XmlNodeList compoundNodes = xmlDoc.SelectNodes("//Compound");
            foreach (XmlNode node in compoundNodes)
            {
                CompoundData compound = new CompoundData
                {
                    ID = node.Attributes["id"].Value,
                    Name = node.Attributes["name"].Value,

                    // Physical Properties
                    MolarMass = float.Parse(node.SelectSingleNode("PhysicalProperties/MolarMass").InnerText),
                    BoilingPoint = float.Parse(node.SelectSingleNode("PhysicalProperties/BoilingPoint").InnerText),
                    SpecificHeat = float.Parse(node.SelectSingleNode("PhysicalProperties/SpecificHeat").InnerText),
                    Density = float.Parse(node.SelectSingleNode("PhysicalProperties/Density").InnerText),

                    // Thermodynamics
                    HeatOfFormation = float.Parse(node.SelectSingleNode("Thermodynamics/HeatOfFormation").InnerText),
                    HeatOfVaporization = float.Parse(node.SelectSingleNode("Thermodynamics/HeatOfVaporization").InnerText),

                    // Reactivity
                    AutoIgnitionTemp = float.Parse(node.SelectSingleNode("ReactivityProperties/AutoIgnitionTemp").InnerText),
                    FlammabilityMin = float.Parse(node.SelectSingleNode("ReactivityProperties/FlammabilityMin").InnerText),
                    FlammabilityMax = float.Parse(node.SelectSingleNode("ReactivityProperties/FlammabilityMax").InnerText),
                    IsInhibitor = bool.Parse(node.SelectSingleNode("ReactivityProperties/IsInhibitor").InnerText),

                    // Safety
                    Flamability = int.Parse(node.SelectSingleNode("SafetyProperties/Flamability").InnerText),
                    Combustability = int.Parse(node.SelectSingleNode("SafetyProperties/Combustability").InnerText),
                    Toxicity = float.Parse(node.SelectSingleNode("SafetyProperties/Toxicity").InnerText),
                    IsNuclear = bool.Parse(node.SelectSingleNode("SafetyProperties/IsNuclear").InnerText)
                };

                compoundDatabase[compound.ID] = compound;
            }
        }

        private static void LoadReactionData()
        {
            TextAsset xmlFile = Resources.Load<TextAsset>("ReactionDefinitions");
            if (xmlFile == null)
            {
                Debug.LogError("ReactionDefinitions.xml not found in Resources folder!");
                return;
            }

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlFile.text);

            XmlNodeList reactionNodes = xmlDoc.SelectNodes("//Reaction");
            foreach (XmlNode node in reactionNodes)
            {
                ReactionData reaction = new ReactionData
                {
                    ID = node.Attributes["id"].Value,
                    Type = node.Attributes["type"].Value,
                    Name = node.SelectSingleNode("Name").InnerText,
                    Description = node.SelectSingleNode("Description").InnerText,
                    Reactants = new List<ReactantData>(),
                    Products = new List<ProductData>()
                };

                // Load Reactants
                XmlNodeList reactantNodes = node.SelectNodes("Reactants/Reactant");
                foreach (XmlNode reactantNode in reactantNodes)
                {
                    reaction.Reactants.Add(new ReactantData
                    {
                        Compound = reactantNode.Attributes["compound"].Value,
                        Coefficient = float.Parse(reactantNode.Attributes["coefficient"].Value),
                        MinConcentration = float.Parse(reactantNode.Attributes["minConcentration"].Value)
                    });
                }

                // Load Products
                XmlNodeList productNodes = node.SelectNodes("Products/Product");
                foreach (XmlNode productNode in productNodes)
                {
                    reaction.Products.Add(new ProductData
                    {
                        Compound = productNode.Attributes["compound"].Value,
                        Coefficient = float.Parse(productNode.Attributes["coefficient"].Value)
                    });
                }

                // Load Conditions
                XmlNode conditionsNode = node.SelectSingleNode("Conditions");
                reaction.Conditions = new ReactionConditions
                {
                    MinTemperature = float.Parse(conditionsNode.SelectSingleNode("MinTemperature").InnerText),
                    MaxTemperature = float.Parse(conditionsNode.SelectSingleNode("MaxTemperature").InnerText),
                    MinPressure = float.Parse(conditionsNode.SelectSingleNode("MinPressure").InnerText),
                    MaxPressure = float.Parse(conditionsNode.SelectSingleNode("MaxPressure").InnerText),
                    RequiresIgnition = bool.Parse(conditionsNode.SelectSingleNode("RequiresIgnition").InnerText),
                    InhibitorThreshold = float.Parse(conditionsNode.SelectSingleNode("InhibitorThreshold").InnerText),
                    InhibitedBy = new List<string>()
                };

                string inhibitorList = conditionsNode.SelectSingleNode("InhibitedBy").InnerText;
                if (!string.IsNullOrEmpty(inhibitorList))
                {
                    string[] inhibitors = inhibitorList.Split(',');
                    foreach (string inhibitor in inhibitors)
                    {
                        reaction.Conditions.InhibitedBy.Add(inhibitor.Trim());
                    }
                }

                // Load Energetics
                XmlNode energeticsNode = node.SelectSingleNode("Energetics");
                reaction.Energetics = new ReactionEnergetics
                {
                    EnthalpyChange = float.Parse(energeticsNode.SelectSingleNode("EnthalpyChange").InnerText),
                    ActivationEnergy = float.Parse(energeticsNode.SelectSingleNode("ActivationEnergy").InnerText)
                };

                // Load Effects
                XmlNode effectsNode = node.SelectSingleNode("Effects");
                reaction.Effects = new ReactionEffects
                {
                    ContaminationPerMol = float.Parse(effectsNode.SelectSingleNode("ContaminationPerMol").InnerText),
                    ExplosionPotential = int.Parse(effectsNode.SelectSingleNode("ExplosionPotential").InnerText)
                };

                reactionDatabase[reaction.ID] = reaction;
            }
        }

        public static CompoundData GetCompound(string id)
        {
            if (!isInitialized) Initialize();

            if (compoundDatabase.TryGetValue(id, out CompoundData compound))
            {
                return compound;
            }

            Debug.LogWarning($"Compound {id} not found in database");
            return null;
        }

        public static ReactionData GetReaction(string id)
        {
            if (!isInitialized) Initialize();

            if (reactionDatabase.TryGetValue(id, out ReactionData reaction))
            {
                return reaction;
            }

            Debug.LogWarning($"Reaction {id} not found in database");
            return null;
        }
    }
}