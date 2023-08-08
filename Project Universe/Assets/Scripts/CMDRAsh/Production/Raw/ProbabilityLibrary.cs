using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.IO;
using UnityEngine;
using ProjectUniverse.Data.Libraries.Definitions;
using System.Globalization;

namespace ProjectUniverse.Data.Libraries
{
    public static class ProbabilityLibrary
    {
        //Zone 0, Solar Inner Sector
        public static Dictionary<OreDefinition, int> Zone0_Ores;
        //Zone 1, Near-Solar Inner
        public static Dictionary<OreDefinition, int> Zone1_Ores;
        //Zone 2, Asteroid Middle Sector
        public static Dictionary<OreDefinition, int> Zone2_Ores;
        //Zone 3, Non-Nebulous Middle
        public static Dictionary<OreDefinition, int> Zone3_Ores;
        //Zone 4, Nebulous Inner Far
        public static Dictionary<OreDefinition, int> Zone4_Ores;
        //Zone 5, Nebulous Far Sector
        public static Dictionary<OreDefinition, int> Zone5_Ores;
        //Zone 6, Non-Nebulous Outer Far
        public static Dictionary<OreDefinition, int> Zone6_Ores;
        private static Boolean isInitialized;

        public class ProbabilityDictionary
        {
            public Boolean isInitialized = false;
            public Dictionary<OreDefinition, int> IL_Zone0_Ores;
            public Dictionary<OreDefinition, int> IL_Zone1_Ores;
            public Dictionary<OreDefinition, int> IL_Zone2_Ores;
            public Dictionary<OreDefinition, int> IL_Zone3_Ores;
            public Dictionary<OreDefinition, int> IL_Zone4_Ores;
            public Dictionary<OreDefinition, int> IL_Zone5_Ores;
            public Dictionary<OreDefinition, int> IL_Zone6_Ores;

            public ProbabilityDictionary()
            {
                IL_Zone0_Ores = new Dictionary<OreDefinition, int>();
                IL_Zone1_Ores = new Dictionary<OreDefinition, int>();
                IL_Zone2_Ores = new Dictionary<OreDefinition, int>();
                IL_Zone3_Ores = new Dictionary<OreDefinition, int>();
                IL_Zone4_Ores = new Dictionary<OreDefinition, int>();
                IL_Zone5_Ores = new Dictionary<OreDefinition, int>();
                IL_Zone6_Ores = new Dictionary<OreDefinition, int>();
            }
            public void InitializeProbabilityDictionary()
            {
                int zoneID;
                string type;
                int ratio;

                if (!isInitialized)
                {

                    isInitialized = true;
                    Debug.Log("Probability Library Construction Initiated");
                    TextAsset _rawText = Resources.Load<TextAsset>("Data/Production/ZoneOreProbabilities");
                    XDocument xmlDoc = XDocument.Parse(_rawText.text, LoadOptions.PreserveWhitespace);
                    foreach (XElement oreDefs in xmlDoc.Descendants("ProbabilityDefinitions"))
                    {
                        //RSZone is the region of space in which these asteroids or whatnot may be.
                        foreach (XElement zone in oreDefs.Elements("Zone"))
                        {
                            //A sub-zone inside of each zone/region.
                            zoneID = int.Parse(zone.Element("ZoneID").Value);
                            Dictionary<OreDefinition, float> tempOreDic = new Dictionary<OreDefinition, float>();
                            OreDefinition tempOreDef;
                            foreach (XElement inclusionSet in zone.Elements("Probability"))
                            {
                                foreach (XElement ores in inclusionSet.Elements("Ore"))
                                {
                                    type = ores.Element("STR_ID").Attribute("Ore_Type").Value;
                                    //Debug.Log(type);
                                    ratio = int.Parse(ores.Element("Ratio").Attribute("Ratio").Value
                                        , CultureInfo.InvariantCulture);
                                    if (OreLibrary.OreDictionary.TryGetValue(type, out tempOreDef))
                                    {
                                        //Debug.Log(tempOreDef.GetOreType() + " is type " + type);
                                        //assign the above per-subzone compile to the appropriate zone dictionary
                                        switch (zoneID)
                                        {
                                            case 0:
                                                IL_Zone0_Ores.Add(tempOreDef, ratio);//add the ore id and probability to this zone
                                                break;
                                            case 1:
                                                IL_Zone1_Ores.Add(tempOreDef, ratio);
                                                break;
                                            case 2:
                                                IL_Zone2_Ores.Add(tempOreDef, ratio);
                                                break;
                                            case 3:
                                                IL_Zone3_Ores.Add(tempOreDef, ratio);
                                                break;
                                            case 4:
                                                IL_Zone4_Ores.Add(tempOreDef, ratio);
                                                break;
                                            case 5:
                                                IL_Zone5_Ores.Add(tempOreDef, ratio);
                                                break;
                                            case 6:
                                                IL_Zone6_Ores.Add(tempOreDef, ratio);
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        Debug.Log("Invalid Ore ID Detected.");
                                    }
                                }
                            }

                        }
                    }
                    Resources.UnloadAsset(_rawText);

                    Debug.Log("Probability Library Construction Finished");
                    Zone0_Ores = IL_Zone0_Ores;
                    Zone1_Ores = IL_Zone1_Ores;
                    Zone2_Ores = IL_Zone2_Ores;
                    Zone3_Ores = IL_Zone3_Ores;
                    Zone4_Ores = IL_Zone4_Ores;
                    Zone5_Ores = IL_Zone5_Ores;
                    Zone6_Ores = IL_Zone6_Ores;
                }
            }
        }

        public static void GetZoneOres(int zone, OreDefinition def, out int value)
        {
            value = 0;
            //Debug.Log(zone+", "+def.GetOreType());
            switch (zone)
            {
                case 0:
                    ProbabilityLibrary.Zone0_Ores.TryGetValue(def, out value);
                    break;
                case 1:
                    ProbabilityLibrary.Zone1_Ores.TryGetValue(def, out value);
                    break;
                case 2:
                    ProbabilityLibrary.Zone2_Ores.TryGetValue(def, out value);
                    break;
                case 3:
                    ProbabilityLibrary.Zone3_Ores.TryGetValue(def, out value);
                    break;
                case 4:
                    ProbabilityLibrary.Zone4_Ores.TryGetValue(def, out value);
                    break;
                case 5:
                    ProbabilityLibrary.Zone5_Ores.TryGetValue(def, out value);
                    break;
                case 6:
                    ProbabilityLibrary.Zone6_Ores.TryGetValue(def, out value);
                    break;
            }
        }

        public static Dictionary<OreDefinition, int> GetZone0Ores()
        {
            return Zone0_Ores;
        }

        public static Dictionary<OreDefinition, int> GetZone1Ores()
        {
            return Zone1_Ores;
        }

        public static Dictionary<OreDefinition, int> GetZone2Ores()
        {
            return Zone2_Ores;
        }

        public static Dictionary<OreDefinition, int> GetZone3Ores()
        {
            return Zone3_Ores;
        }

        public static Dictionary<OreDefinition, int> GetZone4Ores()
        {
            return Zone4_Ores;
        }

        public static Dictionary<OreDefinition, int> GetZone5Ores()
        {
            return Zone5_Ores;
        }

        public static Dictionary<OreDefinition, int> GetZone6Ores()
        {
            return Zone6_Ores;
        }
    }
}