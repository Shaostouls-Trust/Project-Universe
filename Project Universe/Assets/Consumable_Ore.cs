using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ore Quality is 0-3, with 0 being poor, 1 being normal, 2 being high, and 3 being very high.
/// OreTypeSingle is the main resource in the ore
/// OreInclusions is the 
/// </summary>

public class Consumable_Ore : MonoBehaviour
{
    [SerializeField] private int OreQuality;
    [SerializeField] private string OreTypeSingle;
    [SerializeField] private string[] OreInclusions;
    [SerializeField] public float OreMassKg;
    //private ItemStack thisItemStack;

    public Consumable_Ore(string type, int quality, string[] Inclusions, float mass)
    {
        OreTypeSingle = type;
        OreQuality = quality;
        OreInclusions = Inclusions;
        OreMassKg = mass;
    }

    public void PickUpConsumable(GameObject player)
    {
        bool pickedup = player.GetComponent<IPlayer_Inventory>().AddToPlayerInventory<Consumable_Ore>(this.gameObject);
        //the ore has been "picked up" so remove or hide it
        if (pickedup)
        {
            gameObject.SetActive(false);
        }
    }

    public string GetOreType()
    {
        return OreTypeSingle;
    }
    public int GetOreQuality()
    {
        return OreQuality;
    }
    public string[] GetOreInclusions()
    {
        return OreInclusions;
    }
}
