using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectUniverse.Base;
using ProjectUniverse.Player;
using MLAPI;

namespace ProjectUniverse.Production.Machines
{
    public class IMiningDrone : MonoBehaviour
    {
        //private List<Consumable_Ore> oreInventory = new List<Consumable_Ore>();
        [SerializeField] private int maxMass;
        //[SerializeField] private int maxVolume;
        private List<ItemStack> oreInventory = new List<ItemStack>();

        public void AddOreInventory(ItemStack ore)
        {
            bool cont = true;
            foreach (ItemStack stack in oreInventory)
            {
                if (stack.CompareMetaData(ore))
                {
                    cont = false;
                    stack.AddItemStack(ore);
                    break;
                }
            }
            if (cont)
            {
                oreInventory.Add(ore);
            }
        }
        public void AddOreInventory(List<ItemStack> ores)
        {
            foreach (ItemStack stack in ores)
            {
                AddOreInventory(stack);
            }
        }
        public void AddOreInventory(ItemStack[] ores)
        {
            foreach (ItemStack stack in ores)
            {
                AddOreInventory(stack);
            }
        }

        public void ExternalInteractFunc()
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var networkedClient))
            {
                Debug.Log("Emptying Drone");
                IPlayer_Inventory playerInventory = networkedClient.PlayerObject.gameObject.GetComponent<IPlayer_Inventory>();
                for (int i = 0; i < oreInventory.Count; i++)
                {
                    playerInventory.AddStackToPlayerInventory(oreInventory[i]);
                }
            }

        }
    }
}