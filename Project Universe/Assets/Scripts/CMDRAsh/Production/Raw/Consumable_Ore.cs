using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectUniverse.Player;
using ProjectUniverse.Data.Libraries.Definitions;
using ProjectUniverse.Data.Libraries;

namespace ProjectUniverse.Production.Resources
{
    /// <summary>
    /// Ore Quality is 0-7, with 0-1 being poor, 2-4 being normal, 5-6 being high, and 7 being very high.
    /// OreTypeSingle is the main resource in the ore
    /// OreInclusions is the 
    /// </summary>

    public class Consumable_Ore : MonoBehaviour
    {
        [SerializeField] private int OreQuality;
        [SerializeField] private int OreZone;
        [SerializeField] private string OreTypeSingle;
        private OreDefinition OreDef;
        private Dictionary<OreDefinition, float> oreInclusionDict;
        private Dictionary<MaterialDefinition, float> matInclusionDict;
        [SerializeField] private float OreMassKg;
        //private ItemStack thisItemStack;

        public Consumable_Ore(string type, int quality, int zone, float mass)
        {
            OreTypeSingle = type;//load in the ore definition
            OreQuality = quality;
            OreZone = zone;
            if (OreLibrary.OreDictionary.TryGetValue(type, out OreDef))
            {
                switch (zone)
                {
                    case 0:
                        InclusionLibrary.Zone0_Ores.TryGetValue(quality, out oreInclusionDict);
                        InclusionLibrary.Zone0_Mats.TryGetValue(quality, out matInclusionDict);
                        break;
                    case 1:
                        InclusionLibrary.Zone1_Ores.TryGetValue(quality, out oreInclusionDict);
                        InclusionLibrary.Zone1_Mats.TryGetValue(quality, out matInclusionDict);
                        break;
                    case 2:
                        InclusionLibrary.Zone2_Ores.TryGetValue(quality, out oreInclusionDict);
                        InclusionLibrary.Zone2_Mats.TryGetValue(quality, out matInclusionDict);
                        break;
                    case 3:
                        InclusionLibrary.Zone3_Ores.TryGetValue(quality, out oreInclusionDict);
                        InclusionLibrary.Zone3_Mats.TryGetValue(quality, out matInclusionDict);
                        break;
                    case 4:
                        InclusionLibrary.Zone4_Ores.TryGetValue(quality, out oreInclusionDict);
                        InclusionLibrary.Zone4_Mats.TryGetValue(quality, out matInclusionDict);
                        break;
                    case 5:
                        InclusionLibrary.Zone5_Ores.TryGetValue(quality, out oreInclusionDict);
                        InclusionLibrary.Zone5_Mats.TryGetValue(quality, out matInclusionDict);
                        break;
                    case 6:
                        InclusionLibrary.Zone6_Ores.TryGetValue(quality, out oreInclusionDict);
                        InclusionLibrary.Zone6_Mats.TryGetValue(quality, out matInclusionDict);
                        break;
                }
            }
            OreMassKg = mass;
        }

        override
        public string ToString()
        {
            return OreTypeSingle + "; Quality: " + OreQuality + "; Zone: " + OreZone + "; Mass: " + OreMassKg;
        }

        public void PickUpConsumable(GameObject player)
        {
            bool pickedup = player.GetComponent<IPlayer_Inventory>().AddToPlayerInventory<Consumable_Ore>(
                this.gameObject.GetComponent<Consumable_Ore>()
                );
            //the ore has been "picked up" so remove or hide it
            if (pickedup)
            {
                gameObject.SetActive(false);
            }
        }

        public bool CompareMetaData(Consumable_Ore comparee)
        {
            if (GetOreQuality() == comparee.GetOreQuality())
            {
                if (GetOreZone() == comparee.GetOreZone())
                {
                    return true;
                }
            }
            return false;
        }
        public float GetOreMass()
        {
            return OreMassKg;
        }
        public string GetOreType()
        {
            return OreTypeSingle;
        }
        public int GetOreQuality()
        {
            return OreQuality;
        }
        public Dictionary<OreDefinition, float> GetOreInclusions()
        {
            return oreInclusionDict;
        }
        public Dictionary<MaterialDefinition, float> GetMaterialInclusions()
        {
            return matInclusionDict;
        }
        public int GetOreZone()
        {
            return OreZone;
        }
        public OreDefinition GetOreDef()
        {
            return OreDef;
        }
        public float GetOreQuantity()
        {
            return OreMassKg;
        }
        public void RemoveOreAmount(float amount)
        {
            OreMassKg -= amount;
        }
    }
}