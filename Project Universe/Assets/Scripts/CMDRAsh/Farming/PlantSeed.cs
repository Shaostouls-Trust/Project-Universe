using MLAPI;
using ProjectUniverse.Base;
using ProjectUniverse.Items;
using ProjectUniverse.Items.Consumable;
using ProjectUniverse.Player;
using ProjectUniverse.Player.PlayerController;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUniverse.Items.Consumable
{
    public class PlantSeed : Consumable_Applyable
    {
        [SerializeField] private PlantType plantType;
        [SerializeField] private GameObject PlantPrefab;
        public float range = 5f;
        [SerializeField] private int seeds = 50;
        private SupplementalController player;

        public SupplementalController Inventory
        {
            get { return player; }
            set { player = value; }
        }

        public enum PlantType
        {
            Corn = 0,
            Carrot = 1,
            Tomato = 2
        }

        public int Seeds
        {
            get { return seeds; }
        }

        public void RemoveSeedFromPlayerInventory()
        {
            Consumable_Produce produceSeeds = new Consumable_Produce(plantType.ToString() + "_Seeds", 1);
            ItemStack seedStack = new ItemStack(plantType.ToString() + "_Seeds", 9000, typeof(Consumable_Produce));
            seedStack.AddItem(produceSeeds);
            //playerInventory.RemoveFromPlayerInventory(seedStack);
            Debug.Log("Remove seed");
            //seedStack.RemoveItemData(1); <-- produces a NULL :(
        }

        override protected void Use()
        {
            if (seeds != 0)
            {
                RaycastHit hit;
                Vector3 forward = transform.TransformDirection(0f, 0f, 1f) * 5f;//5m reach
                if (Physics.Raycast(transform.position, forward, out hit, 5f))
                {
                    string tag = hit.transform.gameObject.tag;
                    if (tag == "Fertile environments" || tag == "Propitious environments"
                        || tag == "Hostile environments" || tag == "Difficult environments")
                    {
                        GameObject PlantSpawned = Instantiate(PlantPrefab, hit.point, Quaternion.Euler(ModelRotation));
                        seeds--;
                        //RemoveSeedFromPlayerInventory();
                    }
                }

            }
            if (seeds == 0)
            {
                //remove this item from the inventory
                player.UsableCallback(ThisApplyableType);
                Destroy(gameObject);
            }
        }

    }
}