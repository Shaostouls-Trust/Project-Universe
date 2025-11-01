using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.IO;
using UnityEngine;
using ProjectUniverse.Data.Libraries.Definitions;
using System.Globalization;

namespace ProjectUniverse.Data.Libraries
{
    public class FluidLibrary : MonoBehaviour
    {
        public static Dictionary<string, FluidDefinition> FluidDictionary;
        private static Boolean isInitialized;

        private void Awake()
        {
            FluidDefinitionLibrary FDL = new FluidDefinitionLibrary();
            FDL.InitializeFluidDictionary();
        }

        public class FluidDefinitionLibrary
        {
            public Dictionary<string, FluidDefinition> FL_FluidDictionary;

            public FluidDefinitionLibrary()
            {
                FL_FluidDictionary = new Dictionary<string, FluidDefinition>();
            }

            public void InitializeFluidDictionary()
            {
                Debug.Log("Fluid Library Construction Initiated");
                string fluidType;
                int flammability;
                int combustibility;
                float molarMass;
                bool isNuclear;
                float toxicity;
                float boilingPoint;
                float specificHeatLiquid;
                float specificHeatGas;
                float enthalpyVaporization;
                float liquidDensity;

                //will ensure this only runs once (at Awake()).
                if (!isInitialized)
                {
                    isInitialized = true;
                    TextAsset _rawText = Resources.Load<TextAsset>("Data/Production/MasterLibraries/FluidMasterList");
                    XDocument xmlDoc = XDocument.Parse(_rawText.text, LoadOptions.PreserveWhitespace);
                    Debug.Log("Fluid Master Found");

                    foreach (XElement fluidDefs in xmlDoc.Descendants("FluidDefinitions"))
                    {
                        foreach (XElement fluid in fluidDefs.Elements("Fluid"))
                        {
                            fluidType = fluid.Element("Fluid_Type").Attribute("STR_ID").Value;
                            flammability = int.Parse(fluid.Element("Flammability").Attribute("Value").Value
                                , CultureInfo.InvariantCulture);
                            combustibility = int.Parse(fluid.Element("Combustibility").Attribute("Value").Value
                                , CultureInfo.InvariantCulture);
                            molarMass = float.Parse(fluid.Element("MolarMass").Attribute("Value").Value
                                , CultureInfo.InvariantCulture);
                            isNuclear = bool.Parse(fluid.Element("IsNuclear").Attribute("BoolValue").Value);
                            toxicity = float.Parse(fluid.Element("Toxicity").Attribute("Value").Value
                                , CultureInfo.InvariantCulture);
                            boilingPoint = float.Parse(fluid.Element("BoilingPoint").Attribute("Value").Value
                                , CultureInfo.InvariantCulture);
                            specificHeatLiquid = float.Parse(fluid.Element("SpecificHeatLiquid").Attribute("Value").Value
                                , CultureInfo.InvariantCulture);
                            specificHeatGas = float.Parse(fluid.Element("SpecificHeatGas").Attribute("Value").Value
                                , CultureInfo.InvariantCulture);
                            enthalpyVaporization = float.Parse(fluid.Element("EnthalpyVaporization").Attribute("Value").Value
                                , CultureInfo.InvariantCulture);
                            liquidDensity = float.Parse(fluid.Element("LiquidDensity").Attribute("Value").Value
                                , CultureInfo.InvariantCulture);

                            FluidDefinition newFluidDef = new FluidDefinition(fluidType, flammability,
                                combustibility, molarMass, isNuclear, toxicity, boilingPoint,
                                specificHeatLiquid, specificHeatGas, enthalpyVaporization, liquidDensity);
                            Debug.Log($"Adding {fluidType} to Fluid Dictionary");
                            FL_FluidDictionary.Add(fluidType, newFluidDef);
                        }
                    }
                    Debug.Log("Fluid Library Construction Finished");
                    FluidDictionary = FL_FluidDictionary;
                    Resources.UnloadUnusedAssets();
                }
            }
        }
    }
}