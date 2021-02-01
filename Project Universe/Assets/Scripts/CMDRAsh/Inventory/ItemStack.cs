using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Item container
/// </summary>
public class ItemStack : MonoBehaviour
{
    private List<GameObject> ItemData;//maybe have a version that uses GameObject[] in cases where stacks are likely to just sit around.
    private string itemType;//Use strings for oreDict compat should we use one.
    private float itemCount;//be it quantity or mass
    private int maxCount;

    override
    public string ToString()
    {
        return "Itemstack: "+itemType+". Count: "+itemCount+" of "+maxCount;
    }

    public ItemStack(string myItemType, float myItemCount,int myMaxCount)
    {
        itemType = myItemType;
        maxCount = myMaxCount;
        itemCount = myItemCount;
    }
    
    public void AddItem(GameObject item)
    {
        itemCount++;
        ItemData.Add(item);
    }
    public void AddItems(GameObject[] items)
    {
        ItemData.AddRange(items);
        itemCount += items.Length;
    }
    public ItemStack AddItemStack(ItemStack itemstack)
    {
        float total = itemstack.Length() + itemCount;
        if (total > maxCount)
        {
            //Add the itemstack
            ItemData.AddRange(itemstack.GetItemData());
            //create a spillover itemstack
            ItemStack overflowStack = new ItemStack(itemstack.GetStackType(), total - maxCount, itemstack.GetMaxAmount());
            //set the overflow stack as the amount past the max cap
            List<GameObject> splitData = ItemData.GetRange(maxCount, ItemData.Count-1);
            overflowStack.AddItems(splitData.ToArray());
            //Remove the overflow from the list
            ItemData.RemoveRange(maxCount, ItemData.Count - 1);
            itemCount = ItemData.Count;
            //The overflow itemstack will be added to the containing inventory, or
            //this will be sent back to the inventory of player or machine if containing inventory is out of space
            return overflowStack;
        }
        else
        {
            ItemData.AddRange(itemstack.GetItemData());
            itemCount = ItemData.Count;
            return new ItemStack("null", 0, 0);
        }
        
    }

    public int Length()
    {
        return ItemData.Count;
    }
    public string GetStackType()
    {
        return itemType;
    }
    public int GetMaxAmount()
    {
        return maxCount;
    }
    public List<GameObject> GetItemData()
    {
        return ItemData;
    }
}
