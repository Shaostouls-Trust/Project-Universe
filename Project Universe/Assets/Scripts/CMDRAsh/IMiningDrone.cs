using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IMiningDrone : MonoBehaviour
{
    //private List<Consumable_Ore> oreInventory = new List<Consumable_Ore>();
    private List<ItemStack> oreInventory = new List<ItemStack>();

    public void AddOreInventory(ItemStack ore)
    {
        bool cont = true;
        foreach(ItemStack stack in oreInventory)
        {
            if(stack.CompareMetaData(ore))
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
        foreach(ItemStack stack in ores)
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

    public void EmptyInventory(GameObject player)
    {
        Debug.Log("Emptying Drone");
        IPlayer_Inventory playerInventory = player.GetComponent<IPlayer_Inventory>();
        for (int i = 0; i < oreInventory.Count; i++)
        {
            playerInventory.AddStackToPlayerInventory(oreInventory[i]);
        }
    }
}
