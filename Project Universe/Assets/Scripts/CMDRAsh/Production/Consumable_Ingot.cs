using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Consumable_Ingot : MonoBehaviour
{
    [SerializeField] private int ingotQuality;
    [SerializeField] private string typeSingle;
    private IngotDefinition IngotDef;
    private Dictionary<OreDefinition, float> oreInclusionDict;
    private Dictionary<MaterialDefinition, float> matInclusionDict;
    [SerializeField] private float ingotMassKg;

    public Consumable_Ingot(string type, int quality, IngotDefinition ingot,
        Dictionary<OreDefinition, float> oreincs, Dictionary<MaterialDefinition, float> matincs, float mass)
    {
        typeSingle = type;
        ingotQuality = quality;
        IngotDef = ingot;
        oreInclusionDict = oreincs;
        matInclusionDict = matincs;
        ingotMassKg = mass;
    }

    public Consumable_Ingot(string type, int quality, float mass)
    {
        typeSingle = type;
        ingotQuality = quality;
        IngotLibrary.IngotDictionary.TryGetValue(typeSingle, out IngotDef);
        InclusionLibrary.GetZone3Ores().TryGetValue(ingotQuality, out oreInclusionDict);
        InclusionLibrary.GetZone3Mats().TryGetValue(ingotQuality, out matInclusionDict);
        ingotMassKg = mass;
    }

    override
    public string ToString()
    {
        return typeSingle + "; Quality: " + ingotQuality + "; Mass: "+ingotMassKg;
    }

    public float GetIngotMass()
    {
        return ingotMassKg;
    }

    public string GetIngotType()
    {
        return typeSingle;
    }

    public int GetIngotQuality()
    {
        return ingotQuality;
    }

    public bool CompareMetaData(Consumable_Ingot comparee)
    {
        if (GetIngotType() == comparee.GetIngotType())
        {
            if (GetIngotQuality() == comparee.GetIngotQuality())
            {
                return true;
            }
        }
        Debug.Log("METADATA ERROR");
        return false;
    }
}
