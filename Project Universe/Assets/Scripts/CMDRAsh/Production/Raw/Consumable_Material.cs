using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectUniverse.Player;
using ProjectUniverse.Data.Libraries.Definitions;
using ProjectUniverse.Data.Libraries;

namespace ProjectUniverse.Production.Resources
{
    /// <summary>
    /// Material to be used in production. This object is what will appear in the world if some amount of this material is dropped.
    /// </summary>
    public class Consumable_Material : MonoBehaviour
    {
        [SerializeField] private string MaterialType;
        private MaterialDefinition MatDef;
        [SerializeField] private float MatMassKg;

        public Consumable_Material(string materialID, float mass)
        {
            MaterialType = materialID;
            OreLibrary.MaterialDictionary.TryGetValue(MaterialType, out MatDef);
            MatMassKg = mass;

        }

        override
        public string ToString()
        {
            return "" + MatMassKg + "Kg of " + MaterialType;
        }

        public void PickUpConsumable(GameObject player)
        {
            bool pickedup = player.GetComponent<IPlayer_Inventory>().AddToPlayerInventory<Consumable_Material>(
                this.gameObject.GetComponent<Consumable_Material>()
                );
            //the material has been "picked up" so remove or hide it
            if (pickedup)
            {
                gameObject.SetActive(false);
            }
        }

        public float GetMaterialMass()
        {
            return MatMassKg;
        }
        public bool CompareMetaData(Consumable_Material comparee)
        {
            if (GetMaterialID() == comparee.GetMaterialID())
            {
                return true;
            }
            return false;
        }

        public string GetMaterialID()
        {
            return MaterialType;
        }

        public MaterialDefinition GetMatDef()
        {
            return MatDef;
        }
        public float GetMatQuantity()
        {
            return MatMassKg;
        }
        public void RemoveMatAmount(float amount)
        {
            MatMassKg -= amount;
        }
    }
}