using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class ParticulateType
{
    public string ID;
    public string Name;
    public float HealthRisk;        // 0-1 scale, where 1 is extremely hazardous
    public float Combustibility;    // Minimum ppmw for ignition (0 = non-combustible)
    public float Radioactivity;     // Source activity in Becquerels per gram

    public ParticulateType(string id, string name, float healthRisk = 0f, float combustibility = 0f, float radioactivity = 0f)
    {
        ID = id;
        Name = name;
        HealthRisk = healthRisk;
        Combustibility = combustibility;
        Radioactivity = radioactivity;
    }
}

[System.Serializable]
public class ParticulateConcentration
{
    public string ParticulateTypeID;
    public float ConcentrationPPMW; // Parts per million by weight

    public ParticulateConcentration(string typeID, float concentration)
    {
        ParticulateTypeID = typeID;
        ConcentrationPPMW = concentration;
    }
}

public static class ParticulateDatabase
{
    private static Dictionary<string, ParticulateType> particulateTypes;
    private static bool initialized = false;

    public static void Initialize()
    {
        if (initialized) return;

        particulateTypes = new Dictionary<string, ParticulateType>();

        // Define common particulate types
        RegisterParticulate(new ParticulateType("soot", "Soot", healthRisk: 0.2f, combustibility: 0f, radioactivity: 0f));
        RegisterParticulate(new ParticulateType("ash", "Ash", healthRisk: 0.1f, combustibility: 0f, radioactivity: 0f));
        RegisterParticulate(new ParticulateType("carbon_black", "Carbon Black", healthRisk: 0.3f, combustibility: 50f, radioactivity: 0f));
        RegisterParticulate(new ParticulateType("metal_dust", "Metal Dust", healthRisk: 0.4f, combustibility: 100f, radioactivity: 0f));
        RegisterParticulate(new ParticulateType("radioactive_dust", "Radioactive Dust", healthRisk: 0.9f, combustibility: 0f, radioactivity: 1000f));
        RegisterParticulate(new ParticulateType("organic_dust", "Organic Dust", healthRisk: 0.15f, combustibility: 200f, radioactivity: 0f));

        initialized = true;
    }

    public static void RegisterParticulate(ParticulateType type)
    {
        if (!particulateTypes.ContainsKey(type.ID))
        {
            particulateTypes.Add(type.ID, type);
        }
    }

    public static ParticulateType GetParticulate(string id)
    {
        return particulateTypes.ContainsKey(id) ? particulateTypes[id] : null;
    }

    public static Dictionary<string, ParticulateType> GetAllParticulates()
    {
        return new Dictionary<string, ParticulateType>(particulateTypes);
    }
}