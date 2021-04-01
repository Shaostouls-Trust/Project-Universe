using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is for the player's inventory, from equiped gear to carried items like ores, components, etc.
/// </summary>
public class IPlayer_Inventory : MonoBehaviour
{
    [Header("Player Equipment")]
    [SerializeField] private IEquipable p_head;//helmet, gasmask, etc
    //[SerializeField] private IEquipable p_eyes;//goggles, glasses, etc
    //[SerializeField] private IEquipable p_ears;//radio, listening devices, etc
    //[SerializeField] private IEquipable p_mouth;//oxygen gear, mic, etc. Usually mutually exclusive/included with helmet.
    //[SerializeField] private IEquipable p_chest1;//main chestpiece
    //[SerializeField] private IEquipable p_chest2;//chest coverpiece. Pouches, grendoliers, ammo belts, armor, handheld equipment, etc
    //[SerializeField] private IEquipable p_waist;//more ammo, knives, grenades, medkits, handheld tools, etc
    //[SerializeField] private IEquipable p_legsUpperL;//sidearm, ammo, pouches, etc.
    //[SerializeField] private IEquipable p_legsUpperR;//same options as left leg
    //[SerializeField] private IEquipable p_legsLower;//padding, armor, water pouches, etc.
    [SerializeField] private IEquipable p_feet;//boots, shoes, etc
    [Header("Inventory")]
    public float inventoryWeight;
    private List<ItemStack> p_inventory = new List<ItemStack>();

    void Start()
    {
        Consumable_Ingot ingot = new Consumable_Ingot("Ingot_Gold", 3, 50);
        ItemStack devIngotStack = new ItemStack("Ingot_Gold", 100, typeof(Consumable_Ingot));
        int i = 0;
        while (i<10)
        {
            devIngotStack.AddItem(ingot);
            i += 1;
        }
        p_inventory.Add(devIngotStack);
        Debug.Log("Start Length: " + p_inventory.Count);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log("Length: " + p_inventory.Count);
            foreach(ItemStack itemstack in p_inventory)
            {
                Debug.Log(itemstack.ToString());
            }
        }
    }

    public List<ItemStack> GetPlayerInventory()
    {
        return p_inventory;
    }
    public bool RemoveFromPlayerInventory(ItemStack item)
    {
        return p_inventory.Remove(item);
    }
    public ItemStack RemoveFromPlayerInventory(ItemStack item, int atIndex, float amount)
    {
        //return p_inventory.Remove(item);
        Debug.Log("Attempting to find and remove item from player inventory");
        return p_inventory[atIndex].RemoveItemData(amount);
        //return p_inventory.Find(x => x.GetStackType() == item.GetStackType()).RemoveItemData(amount);
    }

    //Called by pickupable to be picked up
    public bool AddToPlayerInventory<itemtype>(Consumable_Ore item)//THIS ADDS 45Kg OF ORE INTO ONE INDEX?
    {
        Consumable_Ore itemScript = item.GetComponent<itemtype>() as Consumable_Ore;
        string type = itemScript.GetOreType();
        ItemStack wrapper = new ItemStack(type, 100, typeof(itemtype));//itemScript.GetType() is the same as itemtype
        wrapper.AddItem(itemScript);//This adds 45Kg to one index
        Debug.Log("Adding stack: " + wrapper.ToString());
        AddStackToPlayerInventory(wrapper);
        return true;
    }

    public bool AddToPlayerInventory<itemtype>(Consumable_Ingot item)
    {
        Consumable_Ingot itemScript = item.GetComponent<itemtype>() as Consumable_Ingot;
        string type = itemScript.GetIngotType();
        ItemStack wrapper = new ItemStack(type, 100, typeof(itemtype));//itemScript.GetType() is the same as itemtype
        wrapper.AddItem(itemScript);
        Debug.Log("Adding stack: " + wrapper.ToString());
        AddStackToPlayerInventory(wrapper);
        return true;
    }

    public bool AddToPlayerInventory<itemtype>(Consumable_Material item)
    {
        Consumable_Material itemScript = item.GetComponent<itemtype>() as Consumable_Material;
        string type = itemScript.GetMaterialID();
        ItemStack wrapper = new ItemStack(type, 100, typeof(itemtype));//itemScript.GetType() is the same as itemtype
        wrapper.AddItem(itemScript);
        Debug.Log("Adding stack: " + wrapper.ToString());
        AddStackToPlayerInventory(wrapper);
        return true;
    }

    public bool AddStackToPlayerInventory(ItemStack item)
    {
        //Debug.Log("---------------------------------------------------------------------------------------------------------------------------");
        p_inventory.Add(item);
        
        //combine any two same itemstacks
        for (int i = 0; i < p_inventory.Count; i++)
        {
            //Debug.Log("i:"+i);
            for (int j = 0; j < p_inventory.Count; j++)
            {
                //Debug.Log("j:" + j);
                if (i != j)
                {
                    if (p_inventory[j].CompareMetaData(p_inventory[i]))
                    {
                        //No point running this if one or both stacks are maxed.
                        if (p_inventory[j].Length() < p_inventory[j].GetMaxAmount() && p_inventory[i].Length() < p_inventory[i].GetMaxAmount())
                        {
                            //Debug.Log("Add item at index "+i+" with index "+j);
                            //Add the two stacks and return the remainder. If there is no remainder, return null.
                //This part can be recursive
                //This part needs reworked
                            ItemStack overflow = p_inventory[i].AddItemStack(p_inventory[j]);//overflow will return null if there is no overflow.
                            string overflowType;
                            try
                            {
                                overflowType = overflow.GetStackType();
                            }
                            catch (Exception e) 
                            {
                                overflowType = null;
                            }
                            //Debug.Log("Overflow type: " + overflowType);
                            //if overflow, start the recursive loop.
                            if (overflowType != null)//checking "overflow != null" was not working, hence this work-around 
                            {
                            
                                //Debug.Log("Remove item at index:" + j + " which is: " + p_inventory[j].ToString());
                                p_inventory.RemoveAt(j);//b/c 90 > 100, overflow is null
                                p_inventory.Add(overflow);
                            }
                            else if(overflowType == null)
                            {
                                //Debug.Log("Removing j:" + p_inventory[j].ToString());
                                p_inventory.RemoveAt(j);
                                //have overflow return the new value?
                                //Debug.Log("AddItemStack returned null, itemstack is now: "+p_inventory[i].ToString());//need to set this to the new value
                            }
                            else { Debug.Log("AHHHHHHHHHHHHHHHHHHHHHHHH!!!"); }
                //To the above needs reworked
                        }
                        else
                        {
                            //Both stacks must be maxed, so add the 2nd to the inventory
                        }
                    }
                }
            }
        }
        return true;
    }

    public void SplitMaxedStacks(ItemStack overflow)
    {
        Debug.Log("Split Stack Recurr");
        //if the added stack is larger than the allowed cap
        if (overflow.Length() > overflow.GetMaxAmount())
        {
            float stacksToMake = overflow.Length() % overflow.GetMaxAmount();
            Debug.Log("Stacks remaining to be made: "+stacksToMake);
            //ItemStack newItemStack = new ItemStack(overflow.GetStackType(), overflow.Length(), overflow.GetMaxAmount());
            //ItemStack overflowItemStack = newItemStack.AddItemStack(overflow);
            //Debug.Log("Recurr Adding Item:" + newItemStack.ToString());
            //p_inventory.Add(newItemStack);//this adding causes an inf loop in the above for?
            //Debug.Log("Supposed to recur here");
            //SplitMaxedStacks(overflowItemStack);
        }
        else
        {
            if(overflow.Length() != 0 || overflow.GetStackType() != null)
            {
                Debug.Log("Recurr end. (not) Adding Item:" + overflow.ToString());
                //p_inventory.Add(overflow);//adds infinitely, because this adds overflow back into the stack
                //which incr the length of loop above this, sending it into a second run, which then runs this again.
            }
        }
    }
}
