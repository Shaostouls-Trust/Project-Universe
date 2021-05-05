using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Item container
/// </summary>
public class ItemStack : MonoBehaviour
{
    //Probably need to accept lists of all pickipable items to avoid casting hell
    private string itemType;//Use strings for ore/mat Dict compat
    private float itemCount;//be it quantity or mass. Item count is not the array size.
    private int maxCount;
    private Type _originalType;
    private Array TArray;
    //private Consumable_Ore[] OreArray;
    private int lastIndex = 0;

    override
    public string ToString()
    {
        string assembly = "Itemstack: " + itemType + ". Count: " + itemCount + " of " + maxCount;
        if(_originalType == typeof(Consumable_Ore))
        {
            Consumable_Ore ore;
            for (int i = 0; i < lastIndex; i++)
            {
                ore = (Consumable_Ore)TArray.GetValue(i);
                assembly += "\n :" + ore.ToString();
            }
        }
        else if(_originalType == typeof(Consumable_Ingot))
        {
            Consumable_Ingot ingot;
            for(int i = 0; i < lastIndex; i++)
            {
                ingot = (Consumable_Ingot)TArray.GetValue(i);
                assembly += "\n"+i+": "+ ingot.ToString();
            }
        }
        else if (_originalType == typeof(Consumable_Material))
        {

        }
        else if (_originalType == typeof(Consumable_Component))
        {

        }
        return assembly;
    }

    public ItemStack(string myItemType, int myMaxCount, Type sourceType)//float myItemCount,
    {
        itemType = myItemType;
        maxCount = myMaxCount;
        //itemcount will be the number of items in TArray
        //itemCount = myItemCount;
        _originalType = sourceType;
        TArray = CreateArrayOfOriginalType(maxCount);
        //Debug.Log("Creating Itemstack P:3 Q:"+itemCount);
    }
    public ItemStack(string myItemType, int myMaxCount, Type sourceType, Array data)
    {
        itemType = myItemType;
        maxCount = myMaxCount;
        //itemcount will be the number of items in TArray
        //itemCount = myItemCount;
        _originalType = sourceType;
        TArray = data;
    }

    private Array CreateArrayOfOriginalType(int maxCap)
    {
        if (_originalType == typeof(Consumable_Ore))
        {
            return new Consumable_Ore[maxCap];
        }
        else if (_originalType == typeof(Consumable_Ingot))
        {
            return new Consumable_Ingot[maxCap];
        }
        else if(_originalType == typeof(Consumable_Material))
        {
            return new Consumable_Material[maxCap];
        }
        else if (_originalType == typeof(Consumable_Component))
        {
            return new Consumable_Component[maxCap];
        }
        else
        {
            return new GameObject[maxCap];
        }
    }

    public void AddItem(Consumable_Ore ore)
    {
        //Debug.Log("Adding ore...");
        itemCount += ore.GetOreQuantity();
        TArray.SetValue(ore, lastIndex++);
    }
    public void AddItem(Consumable_Ingot ingot)
    {
        //Debug.Log("Adding ingot...");
        itemCount += ingot.GetIngotMass();
        TArray.SetValue(ingot, lastIndex++);
    }
    public void AddItem(Consumable_Material material)
    {
        //Debug.Log("Adding mat...");
        itemCount += material.GetMaterialMass();
        TArray.SetValue(material, lastIndex++);
    }

    public void AddItem(Consumable_Component component)
    {
        Debug.Log("Adding comp...");
        itemCount++;
        //Debug.Log(component.ToString());
        //Debug.Log(lastIndex++);
        TArray.SetValue(component, lastIndex++);//cast exception on component addition
        
    }

    public void AddItems(Consumable_Ore[] ores)
    {
        foreach (Consumable_Ore ore in ores)
        {
            AddItem(ore);
        }
    }
    public void AddItems(Consumable_Ingot[] ingots)
    {
        foreach (Consumable_Ingot ingot in ingots)
        {
            AddItem(ingot);
        }
    }

    /// <summary>
    /// Remove items from the array starting at the back, and working in.
    /// The removed items will be added to a new itemstack and returned.
    /// The total amount will be taken from the rearmost first, working inwards.
    /// </summary>
    /// <param name="amountToRemove">The amount of items to remove</param> 
    /// <returns>The stack of removed items</returns>
    public ItemStack RemoveItemData(float amountToRemove)
    {
        if (amountToRemove < 0) { amountToRemove *= -1; }
        ItemStack returnStack;
        //itemCount is the total amount in all indicies.
        Debug.Log("amountToRemove: "+amountToRemove);
        if (itemCount - amountToRemove > 0)
        {
            itemCount -= amountToRemove;
            Debug.Log("remains: "+itemCount);
        }
        else
        {
            amountToRemove = itemCount;
            itemCount = 0;
            Debug.Log("all taken");
        }
        float runningAmount = 0.0f;
        returnStack = new ItemStack(itemType, 100, _originalType);//create a standard sized container
        for (int i = TArray.Length - 1; i >= 0; i--)
        {
            ///
            /// Remove Ore
            ///
            if (TArray.GetValue(i) != null && _originalType == typeof(Consumable_Ore))
            {
                Consumable_Ore tempOre = TArray.GetValue(i) as Consumable_Ore;//May throw null pointer
                //if the last ore index material has the amount we need to remove
                if (tempOre.GetOreQuantity() >= amountToRemove)
                {
                    float need = amountToRemove - runningAmount;
                    tempOre.RemoveOreAmount(amountToRemove);
                    Consumable_Ore newOre = new Consumable_Ore(tempOre.GetOreType(),
                        tempOre.GetOreQuality(), tempOre.GetOreZone(), amountToRemove);
                    returnStack.AddItem(newOre);
                    Debug.Log("Added: "+newOre.ToString());
                    runningAmount += need;
                }
                else
                {
                    float temp = runningAmount + tempOre.GetOreQuantity();
                    //if the next index has some of the amount we need, and would not be emptied.
                    if (temp > amountToRemove)
                    {
                        float need = amountToRemove - runningAmount;
                        //remove the need from the quantity and add to running
                        tempOre.RemoveOreAmount(amountToRemove);
                        Consumable_Ore newOre = new Consumable_Ore(tempOre.GetOreType(),
                            tempOre.GetOreQuality(), tempOre.GetOreZone(), amountToRemove);
                        returnStack.AddItem(newOre);
                        Debug.Log("Added: " + newOre.ToString());
                        runningAmount += need;
                    }
                    //if the next index has some of the amount we need, and would be empty completely.
                    else
                    {
                        runningAmount += tempOre.GetOreQuantity();
                        tempOre.RemoveOreAmount(amountToRemove);
                        Consumable_Ore newOre = new Consumable_Ore(tempOre.GetOreType(),
                            tempOre.GetOreQuality(), tempOre.GetOreZone(), amountToRemove);
                        returnStack.AddItem(newOre);
                        Debug.Log("Added: " + newOre.ToString());
                        TArray.SetValue(null, i);
                        lastIndex -= 1;
                    }
                }
            }
            ///
            /// Remove Ingots
            ///
            else if (TArray.GetValue(i) != null && _originalType == typeof(Consumable_Ingot))
            {
                if(amountToRemove > 0)
                {
                    //remove this ingot from TArray
                    Consumable_Ingot tempIng = TArray.GetValue(i) as Consumable_Ingot;//May throw null pointer
                    returnStack.AddItem(tempIng);
                    TArray.SetValue(null, i);//empty the TArray. May just need to create a new one.
                    lastIndex -= 1;
                    Debug.Log("Added: " + tempIng.ToString());
                    //subtract the mass of the ingot, because the max value is mass not quantity.
                    amountToRemove-=tempIng.GetIngotMass();
                }   
            }
            ///
            /// Remove Components
            ///
            if (TArray.GetValue(i) != null && _originalType == typeof(Consumable_Component))
            {
                Consumable_Component tempComp = TArray.GetValue(i) as Consumable_Component;//May throw null pointer
                //if the last ore index material has the amount we need to remove
                if (tempComp.GetQuantity() >= amountToRemove)
                {
                    float need = amountToRemove - runningAmount;
                    tempComp.RemoveComponentAmount((int)amountToRemove);
                    Consumable_Component newComp = new Consumable_Component(tempComp.GetComponentID(),
                        tempComp.GetQuantity(),tempComp.GetComponentDefinition());
                    returnStack.AddItem(newComp);
                    Debug.Log("Added: " + newComp.ToString());
                    runningAmount += need;
                }
                else
                {
                    float temp = runningAmount + tempComp.GetQuantity();
                    //if the next index has some of the amount we need, and would not be emptied.
                    if (temp > amountToRemove)
                    {
                        float need = amountToRemove - runningAmount;
                        //remove the need from the quantity and add to running
                        tempComp.RemoveComponentAmount((int)amountToRemove);
                        Consumable_Component newComp = new Consumable_Component(tempComp.GetComponentID(),
                            tempComp.GetQuantity(), tempComp.GetComponentDefinition());
                        returnStack.AddItem(newComp);
                        Debug.Log("Added: " + newComp.ToString());
                        runningAmount += need;
                    }
                    //if the next index has some of the amount we need, and would be empty completely.
                    else
                    {
                        runningAmount += tempComp.GetQuantity();
                        tempComp.RemoveComponentAmount((int)amountToRemove);
                        Consumable_Component newComp = new Consumable_Component(tempComp.GetComponentID(),
                            tempComp.GetQuantity(), tempComp.GetComponentDefinition());
                        returnStack.AddItem(newComp);
                        Debug.Log("Added: " + newComp.ToString());
                        TArray.SetValue(null, i);
                        lastIndex -= 1;
                    }
                }
            }
            //remove the objects from TArray
            //TArray.SetValue(null, i);//empty the TArray. May just need to create a new one.
            //lastIndex -= 1;
        }
        
        /*
        int amountToRemove_int = (int)Math.Round(amountToRemove);
        //Params will change based off of the above two cases
        returnStack = new ItemStack(itemType, amountToRemove_int, _originalType);//amountToRemove should be an int?
        for (int j = TArray.Length - 1; j >= (TArray.Length - amountToRemove_int); j--)//this treats all items as 1.0 units occupying 1 index
        {
            //Add the objects from the TArray to the return stack
            if (_originalType == typeof(Consumable_Ore))
            {
                returnStack.AddItem((Consumable_Ore)TArray.GetValue(j));
            }
            else if (_originalType == typeof(Consumable_Ingot))
            {
                returnStack.AddItem((Consumable_Ingot)TArray.GetValue(j));
            }
            //remove the objects from TArray
            TArray.SetValue(null, j);//empty the TArray. May just need to create a new one.
            lastIndex -= 1;
        }
        */
        return returnStack;
    }

    /// <summary>
    /// Combine the two stacks and return the excess.
    /// </summary>
    /// <param name="itemstack"></param>
    /// <returns></returns>
    public ItemStack AddItemStack(ItemStack itemstack)
    {
        //check type to see if it can be added to the stack
        //if(CompareMetaData(itemstack))
        //{
            Debug.Log("ADDING ITEMSTACKS");
            //Add the two itemstack Lengths (amounts)
            int tempCap = maxCount;
            itemCount += itemstack.Size();
            //now combine their data
            //We'll need to expand the array for when it goes overcap
            if (itemCount > maxCount)
            {
                tempCap = (int)Math.Round(itemCount);
                //copy TArray into a new, larger array
                Array newArray = CreateArrayOfOriginalType(tempCap);
                TArray.CopyTo(newArray, 0);
                //set the larger array as TArray
                TArray = newArray;
            }
            //Debug.Log("itemstack Lmax: "+itemstack.Length());
            //Debug.Log("itemstack Lreal: "+ itemstack.GetRealLength());
            Debug.Log(TArray.GetHashCode()+" v "+itemstack.GetItemArray().GetHashCode());
            for (int j = 0; j < itemstack.Length(); j++)
            {
                //add every value in itemstack to TArray sequentially using lastIndex as the insertion point
                //Debug.Log(j +" "+lastIndex);
                //Debug.Log("Lreal: " + itemstack.GetRealLength());
                TArray.SetValue(itemstack.GetItemArray().GetValue(j), lastIndex++);
            }
            if (itemCount > maxCount)
            {
                //remove any values beyond the maxCount
                //create a spillover itemstack that is as large as the surcharge
                ItemStack overflowStack;// = new ItemStack(itemstack.GetStackType(), tempCap - maxCount, itemstack.GetOriginalType());
                Debug.Log("Remove and add the surcharge to overflow");
            //Remove the surcharge from the itemstack and put it into the overflow stack
            Debug.Log("tempCap - maxCount:"+ (tempCap - maxCount));
                overflowStack = RemoveItemData(tempCap - maxCount);
                Debug.Log("Returning overflow: " + overflowStack.ToString());
                return overflowStack;
            }
            else
            {
                Debug.Log("No overflow");
                return null;
            }
        //}
        //else
        //{
        //    Debug.Log("Non-Equivelant Stack");
       //     return null;
       // }
    }

    public bool CompareMetaData(ItemStack comparee)//Type typePram
    {
        Debug.Log(GetStackType() + " v " + comparee.GetStackType());
        if(GetStackType() == comparee.GetStackType())
        {
            if(_originalType == typeof(Consumable_Ore))//typePram
            {
                Debug.Log("comparing ore meta...");
                ///
                /// MIGHT MAKE IT SO THAT ORE ZONE AND QUALITY ARE 'HIDDEN' FROM META COMPS,
                /// TO AVOID NEEDING AN ITEMSTACK FOR EVERY DIFFERENT ZONE/QUALITY COMBO
                ///
                Consumable_Ore compOre = comparee.TArray.GetValue(0) as Consumable_Ore;
                Consumable_Ore oordOre = TArray.GetValue(0) as Consumable_Ore;
                //Debug.Log(compOre.ToString());
                if (oordOre.CompareMetaData(compOre))
                {
                    return true;
                }
                Debug.Log("ItemStacks not equal");
            }
            else if(_originalType == typeof(Consumable_Ingot))
            {
                Debug.Log("comparing ingot meta...");
                Consumable_Ingot compIng = comparee.TArray.GetValue(0) as Consumable_Ingot;
                Consumable_Ingot oordIng = TArray.GetValue(0) as Consumable_Ingot;
                //Only type and quality are checked. Mass and impurities are allowed to varry.
                if (oordIng.CompareMetaData(compIng))
                {
                    Debug.Log("Itemstacks ARE EQUAL");
                    return true;
                }
                Debug.Log("ItemStacks not equal");
            }
            else if(_originalType == typeof(Consumable_Material))
            {

            }
            else if(_originalType == typeof(Consumable_Component))
            {

            }
        }
        Debug.Log("Types not equal");
        return false;
    }
   
    public int Length()
    {
        float size = 0.0f;
        for (int i = 0; i < TArray.Length; i++)
        {
            if(TArray.GetValue(i) != null)
            {
                size = i+1;
            }
        }
        return (int)size;
        //return TArray.Length;
    }

    public int GetRealLength()
    {
        float size = 0;
        //itemData length is the summation of the counts of the itemData items
        for (int i = 0; i < TArray.Length; i++)
        {
            if(TArray.GetValue(i) != null)
            {
                float mass = 0f;
                if(_originalType == typeof(Consumable_Ingot)){
                    Consumable_Ingot ingot = TArray.GetValue(i) as Consumable_Ingot;
                    mass = ingot.GetIngotMass();
                }
                else if (_originalType == typeof(Consumable_Ore))
                {
                    Consumable_Ore ore = TArray.GetValue(i) as Consumable_Ore;
                    mass = ore.GetOreMass();
                }
                else if (_originalType == typeof(Consumable_Material))
                {
                    Consumable_Material mat = TArray.GetValue(i) as Consumable_Material;
                    mass = mat.GetMaterialMass();
                }
                else if (_originalType == typeof(Consumable_Component))
                {
                    Consumable_Component com = TArray.GetValue(i) as Consumable_Component;
                    mass = com.GetQuantity();
                }
                size += mass;
            }
        }
        return (int)size;
    }

    public float Size()
    {
        return itemCount;
    }

    public Type GetOriginalType()
    {
        return _originalType;
    }

    public string GetStackType()
    {
        return itemType;
    }
    public int GetMaxAmount()
    {
        return maxCount;
    }
    public Array GetItemArray()
    {
        return TArray;
    }
}
