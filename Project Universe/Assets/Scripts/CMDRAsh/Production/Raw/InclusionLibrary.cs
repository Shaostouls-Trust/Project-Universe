using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.IO;
using UnityEngine;

/// <summary>
/// Load in inclusion definitions for game resource zones.
/// Inclusions in ores are split into 7 levels for each of the 7 zones, so 49 profiles in total.
/// 0{0{s:n;s:n};1{}}
/// 1{0{s:n;s:n}}
/// </summary>
public static class InclusionLibrary
{
    //Zone 0, Solar Inner Sector
    public static Dictionary<int, Dictionary<OreDefinition, float>> Zone0_Ores;
    public static Dictionary<int, Dictionary<MaterialDefinition, float>> Zone0_Mats;
    //Zone 1, Near-Solar Inner
    public static Dictionary<int, Dictionary<OreDefinition, float>> Zone1_Ores;
    public static Dictionary<int, Dictionary<MaterialDefinition, float>> Zone1_Mats;
    //Zone 2, Asteroid Middle Sector
    public static Dictionary<int, Dictionary<OreDefinition, float>> Zone2_Ores;
    public static Dictionary<int, Dictionary<MaterialDefinition, float>> Zone2_Mats;
    //Zone 3, Non-Nebulous Middle
    public static Dictionary<int, Dictionary<OreDefinition, float>> Zone3_Ores;
    public static Dictionary<int, Dictionary<MaterialDefinition, float>> Zone3_Mats;
    //Zone 4, Nebulous Inner Far
    public static Dictionary<int, Dictionary<OreDefinition, float>> Zone4_Ores;
    public static Dictionary<int, Dictionary<MaterialDefinition, float>> Zone4_Mats;
    //Zone 5, Nebulous Far Sector
    public static Dictionary<int, Dictionary<OreDefinition, float>> Zone5_Ores;
    public static Dictionary<int, Dictionary<MaterialDefinition, float>> Zone5_Mats;
    //Zone 6, Non-Nebulous Outer Far
    public static Dictionary<int, Dictionary<OreDefinition, float>> Zone6_Ores;
    public static Dictionary<int, Dictionary<MaterialDefinition, float>> Zone6_Mats;
    //Zone 7, Not Used
    public static Dictionary<int, Dictionary<OreDefinition, float>> Zone7_Ores;
    public static Dictionary<int, Dictionary<MaterialDefinition, float>> Zone7_Mats;

    private static Boolean isInitialized;

    public class InclusionDictionary
    {
        //will ensure this only runs once (at Awake()).
        public Boolean isInitialized = false;
        public Dictionary<int, Dictionary<OreDefinition, float>> IL_Zone0_Ores;
        public Dictionary<int, Dictionary<MaterialDefinition, float>> IL_Zone0_Mats;
        public Dictionary<int, Dictionary<OreDefinition, float>> IL_Zone1_Ores;
        public Dictionary<int, Dictionary<MaterialDefinition, float>> IL_Zone1_Mats;
        public Dictionary<int, Dictionary<OreDefinition, float>> IL_Zone2_Ores;
        public Dictionary<int, Dictionary<MaterialDefinition, float>> IL_Zone2_Mats;
        public Dictionary<int, Dictionary<OreDefinition, float>> IL_Zone3_Ores;
        public Dictionary<int, Dictionary<MaterialDefinition, float>> IL_Zone3_Mats;
        public Dictionary<int, Dictionary<OreDefinition, float>> IL_Zone4_Ores;
        public Dictionary<int, Dictionary<MaterialDefinition, float>> IL_Zone4_Mats;
        public Dictionary<int, Dictionary<OreDefinition, float>> IL_Zone5_Ores;
        public Dictionary<int, Dictionary<MaterialDefinition, float>> IL_Zone5_Mats;
        public Dictionary<int, Dictionary<OreDefinition, float>> IL_Zone6_Ores;
        public Dictionary<int, Dictionary<MaterialDefinition, float>> IL_Zone6_Mats;
        //7 not used
        public Dictionary<int, Dictionary<OreDefinition, float>> IL_Zone7_Ores;
        public Dictionary<int, Dictionary<MaterialDefinition, float>> IL_Zone7_Mats;

        public InclusionDictionary()
        {
            IL_Zone0_Ores = new Dictionary<int, Dictionary<OreDefinition, float>>();
            IL_Zone0_Mats = new Dictionary<int, Dictionary<MaterialDefinition, float>>();
            IL_Zone1_Ores = new Dictionary<int, Dictionary<OreDefinition, float>>();
            IL_Zone1_Mats = new Dictionary<int, Dictionary<MaterialDefinition, float>>();
            IL_Zone2_Ores = new Dictionary<int, Dictionary<OreDefinition, float>>();
            IL_Zone2_Mats = new Dictionary<int, Dictionary<MaterialDefinition, float>>();
            IL_Zone3_Ores = new Dictionary<int, Dictionary<OreDefinition, float>>();
            IL_Zone3_Mats = new Dictionary<int, Dictionary<MaterialDefinition, float>>();
            IL_Zone4_Ores = new Dictionary<int, Dictionary<OreDefinition, float>>();
            IL_Zone4_Mats = new Dictionary<int, Dictionary<MaterialDefinition, float>>();
            IL_Zone5_Ores = new Dictionary<int, Dictionary<OreDefinition, float>>();
            IL_Zone5_Mats = new Dictionary<int, Dictionary<MaterialDefinition, float>>();
            IL_Zone6_Ores = new Dictionary<int, Dictionary<OreDefinition, float>>();
            IL_Zone6_Mats = new Dictionary<int, Dictionary<MaterialDefinition, float>>();
            //7 not used
            IL_Zone7_Ores = new Dictionary<int, Dictionary<OreDefinition, float>>();
            IL_Zone7_Mats = new Dictionary<int, Dictionary<MaterialDefinition, float>>();
        }

        public void InitializeInclusionDictionary()
        {
            int RSZone;
            int qualityZone;
            string type;
            float ratio;
            
            if (!isInitialized)
            {
                isInitialized = true;
                Debug.Log("Inclusion Library Construction Initiated");
                //find the xmls
                string xmlPath = "\\Assets\\Resources\\Data\\Production\\";
                string root = Directory.GetCurrentDirectory();
                string[] filesInDir = Directory.GetFiles(root + xmlPath, "*.xml", SearchOption.TopDirectoryOnly);

                foreach (string fileAndPath in filesInDir)
                {
                    //Debug.Log("FandP: " + fileAndPath);
                    XDocument doc = XDocument.Load(fileAndPath);
                    foreach (XElement oreDefs in doc.Descendants("ZoneDefinitions"))
                    {
                        //RSZone is the region of space in which these asteroids or whatnot may be.
                        //The above dictionaries are divided by zone
                        RSZone = int.Parse(oreDefs.Element("RSZone").Value);
                        //Debug.Log("---RSZone: "+RSZone);
                        foreach (XElement zone in oreDefs.Elements("Zone"))
                        {
                            //A sub-zone inside of each zone/region.
                            qualityZone = int.Parse(zone.Element("QualityZone").Value);
                            Dictionary<OreDefinition, float> tempOreDic = new Dictionary<OreDefinition, float>();
                            Dictionary<MaterialDefinition, float> tempMatDic = new Dictionary<MaterialDefinition, float>();
                            MaterialDefinition tempMatDef;
                            OreDefinition tempOreDef;
                            foreach (XElement inclusionSet in zone.Elements("Inclusions"))
                            {
                                foreach (XElement materials in inclusionSet.Elements("Materials"))
                                {
                                    //Debug.Log("Material: "+materials.Element("STR_ID").Attribute("Material_Type").Value);
                                    type = materials.Element("STR_ID").Attribute("Material_Type").Value;
                                    ratio = float.Parse(materials.Element("Ratio").Attribute("Ratio").Value);
                                    //use the material_type to get the MaterialDefinition from the MatDefLib
                                    if (OreLibrary.MaterialDictionary.TryGetValue(type, out tempMatDef))
                                    {
                                        tempMatDic.Add(tempMatDef, ratio);
                                    }
                                    else
                                    {
                                        Debug.Log("Invalid Material ID Detected.");
                                    }
                                }
                                //Debug.Log("------------------------------------------------------------------------");
                                foreach (XElement ores in inclusionSet.Elements("Ore"))
                                {
                                    //Debug.Log("Ore: "+ores.Element("STR_ID").Attribute("Ore_Type").Value);
                                    type = ores.Element("STR_ID").Attribute("Ore_Type").Value;
                                    ratio = float.Parse(ores.Element("Ratio").Attribute("Ratio").Value);
                                    if (OreLibrary.OreDictionary.TryGetValue(type, out tempOreDef))
                                    {
                                        tempOreDic.Add(tempOreDef, ratio);
                                    }
                                    else
                                    {
                                        Debug.Log("Invalid Ore ID Detected.");
                                    }
                                }
                            }
                            //assign the above per-subzone compile to the appropriate zone dictionary
                            //Debug.Log("Switch RSZ: " + RSZone);
                            switch (RSZone)
                            {
                                case 0:
                                    IL_Zone0_Ores.Add(qualityZone, tempOreDic);
                                    IL_Zone0_Mats.Add(qualityZone, tempMatDic);
                                    break;
                                case 1:
                                    IL_Zone1_Ores.Add(qualityZone, tempOreDic);
                                    IL_Zone1_Mats.Add(qualityZone, tempMatDic);
                                    break;
                                case 2:
                                    IL_Zone2_Ores.Add(qualityZone, tempOreDic);
                                    IL_Zone2_Mats.Add(qualityZone, tempMatDic);
                                    break;
                                case 3:
                                    IL_Zone3_Ores.Add(qualityZone, tempOreDic);
                                    IL_Zone3_Mats.Add(qualityZone, tempMatDic);
                                    break;
                                case 4:
                                    IL_Zone4_Ores.Add(qualityZone, tempOreDic);
                                    IL_Zone4_Mats.Add(qualityZone, tempMatDic);
                                    break;
                                case 5:
                                    IL_Zone5_Ores.Add(qualityZone, tempOreDic);
                                    IL_Zone5_Mats.Add(qualityZone, tempMatDic);
                                    break;
                                case 6:
                                    IL_Zone6_Ores.Add(qualityZone, tempOreDic);
                                    IL_Zone6_Mats.Add(qualityZone, tempMatDic);
                                    break;
                                case 7:
                                    //not used
                                    IL_Zone7_Ores.Add(qualityZone, tempOreDic);
                                    IL_Zone7_Mats.Add(qualityZone, tempMatDic);
                                    break;
                            }
                        }
                    }
                }
                Debug.Log("Inclusion Library Construction Finished");
                Zone0_Ores = IL_Zone0_Ores;
                Zone0_Mats = IL_Zone0_Mats;
                Zone1_Ores = IL_Zone1_Ores;
                Zone1_Mats = IL_Zone1_Mats;
                Zone2_Ores = IL_Zone2_Ores;
                Zone2_Mats = IL_Zone2_Mats;
                Zone3_Ores = IL_Zone3_Ores;
                Zone3_Mats = IL_Zone3_Mats;
                Zone4_Ores = IL_Zone4_Ores;
                Zone4_Mats = IL_Zone4_Mats;
                Zone5_Ores = IL_Zone5_Ores;
                Zone5_Mats = IL_Zone5_Mats;
                Zone6_Ores = IL_Zone6_Ores;
                Zone6_Mats = IL_Zone6_Mats;
                //not used
                Zone7_Ores = IL_Zone7_Ores;
                Zone7_Mats = IL_Zone7_Mats;
            }
        }
    }
    public static Dictionary<int, Dictionary<OreDefinition, float>> GetZone0Ores()
    {
        return Zone0_Ores;
    }
    public static Dictionary<int, Dictionary<MaterialDefinition, float>> GetZone0Mats()
    {
        return Zone0_Mats;
    }
    public static Dictionary<int, Dictionary<OreDefinition, float>> GetZone1Ores()
    {
        return Zone1_Ores;
    }
    public static Dictionary<int, Dictionary<MaterialDefinition, float>> GetZone1Mats()
    {
        return Zone1_Mats;
    }
    public static Dictionary<int, Dictionary<OreDefinition, float>> GetZone2Ores()
    {
        return Zone2_Ores;
    }
    public static Dictionary<int, Dictionary<MaterialDefinition, float>> GetZone2Mats()
    {
        return Zone2_Mats;
    }
    public static Dictionary<int, Dictionary<OreDefinition, float>> GetZone3Ores()
    {
        return Zone3_Ores;
    }
    public static Dictionary<int, Dictionary<MaterialDefinition, float>> GetZone3Mats()
    {
        return Zone3_Mats;
    }
    public static Dictionary<int, Dictionary<OreDefinition, float>> GetZone4Ores()
    {
        return Zone4_Ores;
    }
    public static Dictionary<int, Dictionary<MaterialDefinition, float>> GetZone4Mats()
    {
        return Zone4_Mats;
    }
    public static Dictionary<int, Dictionary<OreDefinition, float>> GetZone5Ores()
    {
        return Zone5_Ores;
    }
    public static Dictionary<int, Dictionary<MaterialDefinition, float>> GetZone5Mats()
    {
        return Zone5_Mats;
    }
    public static Dictionary<int, Dictionary<OreDefinition, float>> GetZone6Ores()
    {
        return Zone6_Ores;
    }
    public static Dictionary<int, Dictionary<MaterialDefinition, float>> GetZone6Mats()
    {
        return Zone6_Mats;
    }
    public static Dictionary<int, Dictionary<OreDefinition, float>> GetZone7Ores()
    {
        return Zone7_Ores;
    }
    public static Dictionary<int, Dictionary<MaterialDefinition, float>> GetZone7Mats()
    {
        return Zone7_Mats;
    }
}
