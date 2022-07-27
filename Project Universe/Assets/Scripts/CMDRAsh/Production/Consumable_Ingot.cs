using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectUniverse.Data.Libraries.Definitions;
using ProjectUniverse.Data.Libraries;
using MLAPI;
using ProjectUniverse.Player;

namespace ProjectUniverse.Production.Resources
{
    public class Consumable_Ingot : MonoBehaviour
    {
        [SerializeField] private int ingotQuality;
        [SerializeField] private string typeSingle;
        private IngotDefinition ingotDef;
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
            IngotLibrary.IngotDictionary.TryGetValue(typeSingle, out ingotDef);
            InclusionLibrary.GetZone3Ores().TryGetValue(ingotQuality, out oreInclusionDict);
            InclusionLibrary.GetZone3Mats().TryGetValue(ingotQuality, out matInclusionDict);
            ingotMassKg = mass;
        }

        public void RegenerateIngot(Consumable_Ingot ingot)
        {
            typeSingle = ingot.GetIngotType();
            ingotQuality = ingot.ingotQuality;
            IngotDef = ingot.IngotDef;
            oreInclusionDict = ingot.oreInclusionDict;
            matInclusionDict = ingot.matInclusionDict;
            ingotMassKg = ingot.ingotMassKg;
        }

        override
        public string ToString()
        {
            return typeSingle + "; Quality: " + ingotQuality + "; Mass: " + ingotMassKg;
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

        public IngotDefinition IngotDef
        {
            get
            {
                return ingotDef;
            }

            set
            {
                ingotDef = value;
            }
        }

        public List<(OreDefinition, float)> GetOreInclusionsAsList()
        {
            List<(OreDefinition, float)> oreIncList = new List<(OreDefinition, float)>();
            foreach (var key in oreInclusionDict.Keys)
            {
                oreInclusionDict.TryGetValue(key, out float amt);
                oreIncList.Add((key,amt));
            }
            return oreIncList;
        }

        public List<(MaterialDefinition, float)> GetMatInclusionsAsList()
        {
            List<(MaterialDefinition, float)> matIncList = new List<(MaterialDefinition, float)>();
            foreach (var key in matInclusionDict.Keys)
            {
                matInclusionDict.TryGetValue(key, out float amt);
                matIncList.Add((key, amt));
            }
            return matIncList;
        }

        public bool CompareMetaData(Consumable_Ingot comparee)
        {
            if (GetIngotType() == comparee.GetIngotType())
            {
                if (Equals(GetIngotQuality(), comparee.GetIngotQuality()))
                {
                    return true;
                }
            }
            Debug.Log("METADATA ERROR: "+ GetIngotType() +" != "+ comparee.GetIngotType());
            return false;
        }

        public void ExternalInteractFunc()
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var networkedClient))
            {
                networkedClient.PlayerObject.gameObject.GetComponent<IPlayer_Inventory>().AddToPlayerInventory<Consumable_Ingot>(
                this.gameObject.GetComponent<Consumable_Ingot>()
                );
                Destroy(gameObject);
            }
        }
    }
}