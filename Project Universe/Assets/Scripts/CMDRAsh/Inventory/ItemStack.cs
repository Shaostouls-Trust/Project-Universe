using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectUniverse.Production.Resources;
using ProjectUniverse.Items.Consumable;
using ProjectUniverse.Items.Weapons;
using ProjectUniverse.Items.Tools;
using ProjectUniverse.Items;
/// <summary>
/// Item container
/// </summary>
namespace ProjectUniverse.Base
{
    [Serializable]
    public class ItemStack
    {
        private string itemType;//Use strings for ore/mat Dict compat
        private float itemCount = 0f;//be it quantity or mass. Item count is not the array size.
        private int maxCount;
        private Type _originalType;
        private Array TArray;
        private int lastIndex = 0;
        private Category stackCategory;

        override
        public string ToString()
        {
            string assembly = "Itemstack: " + itemType + ". Count: " + itemCount + " of " + maxCount;
            if (_originalType == typeof(Consumable_Ore))
            {
                Consumable_Ore ore;
                for (int i = 0; i < lastIndex; i++)
                {
                    ore = (Consumable_Ore)TArray.GetValue(i);
                    assembly += "\n :" + ore.ToString();
                }
            }
            else if (_originalType == typeof(Consumable_Ingot))
            {
                Consumable_Ingot ingot;
                for (int i = 0; i < lastIndex; i++)
                {
                    ingot = (Consumable_Ingot)TArray.GetValue(i);
                    assembly += "\n" + i + ": " + ingot.ToString();
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

        public enum Category
        {
            Weapon,
            Gadget,
            Gear,
            Consumable,
            Ammo,
            Resource,
            Misc
        }

        public ItemStack(ItemStack stack)
        {
            itemType = stack.GetStackType();
            maxCount = stack.GetMaxAmount();
            _originalType = stack.GetOriginalType();
            TArray = stack.GetItemArray();
            itemCount = stack.Size();
            lastIndex = stack.lastIndex;
            stackCategory = stack.stackCategory;
        }

        public ItemStack(string myItemType, int myMaxCount, Type sourceType)
        {
            itemType = myItemType;
            maxCount = myMaxCount;
            _originalType = sourceType;
            //Create an array with a starting length of 1
            TArray = CreateArrayOfOriginalType(1);//maxCount
            stackCategory = CalcSortCategory();
        }
        /// <summary>
        /// Set the ItemStack's TArray to the passed param. <br/>
        /// </summary>
        /// <param name="array"></param>
        public void SetTArray(Array array)//<T>
        {
            //if(typeof(T) == typeof(Consumable_Component))
            //{
            //     TArray = (Consumable_Component[])array;
            // }
            TArray = array;
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
            else if (_originalType == typeof(Consumable_Material))
            {
                return new Consumable_Material[maxCap];
            }
            else if (_originalType == typeof(Consumable_Component))
            {
                return new Consumable_Component[maxCap];
            }
            else if(_originalType == typeof(Consumable_Produce))
            {
                return new Consumable_Produce[maxCap];
            }
            else
            {
                return new GameObject[maxCap];
            }
        }

        public Array ExpandArrayBy(int expandAmount)
        {
            //copy TArray into a new, larger array
            Array newArray = CreateArrayOfOriginalType(TArray.Length+expandAmount);
            TArray.CopyTo(newArray, 0);
            return newArray;
        }

        public void AddItem(Consumable_Ore ore)
        {
            //Debug.Log("Adding ore...");
            itemCount += ore.GetOreQuantity();
            //check the length of TArray. Increase size if need be.
            if(TArray.Length <= lastIndex)
            {
                TArray = ExpandArrayBy(1);
            }
            TArray.SetValue(ore, lastIndex++);
        }
        public void AddItem(Consumable_Ingot ingot)
        {
            //Debug.Log("Adding ingot...");
            itemCount += ingot.GetIngotMass();
            //check the length of TArray. Increase size if need be.
            if (TArray.Length <= lastIndex)
            {
                TArray = ExpandArrayBy(1);
            }
            TArray.SetValue(ingot, lastIndex++);
        }
        public void AddItem(Consumable_Material material)
        {
            //Debug.Log("Adding mat...");
            itemCount += material.GetMaterialMass();
            //check the length of TArray. Increase size if need be.
            if (TArray.Length < lastIndex)
            {
                TArray = ExpandArrayBy(1);
            }
            TArray.SetValue(material, lastIndex++);
        }

        public void AddItem(Consumable_Component component)
        {
            //Debug.Log("Adding comp...");
            itemCount += component.GetQuantity();
            //check the length of TArray. Increase size if need be.
            if (TArray.Length < lastIndex)
            {
                TArray = ExpandArrayBy(1);
            }
            //Debug.Log(component.ToString());
            //Debug.Log(lastIndex++);
            TArray.SetValue(component, lastIndex++);//cast exception on component addition

        }

        public void AddItem(Consumable_Produce produce)
        {
            //Debug.Log("Adding comp...");
            itemCount += produce.GetProduceCount();
            //check the length of TArray. Increase size if need be.
            if (TArray.Length < lastIndex)
            {
                TArray = ExpandArrayBy(1);
            }
            //Debug.Log(component.ToString());
            //Debug.Log(lastIndex++);
            TArray.SetValue(produce, lastIndex++);//cast exception on component addition

        }

        public void RemoveTArrayIndex(int index)
        {
            TArray.SetValue(null, index);
            //contract the TArray around this index
            for (int i = index; i < TArray.Length - 1; i++)
            {
                TArray.SetValue(TArray.GetValue(i + 1), i);
            }
            LastIndex--;
            //Debug.Log(TArray.GetValue(0)+"->"+TArray.GetValue(LastIndex-1));
        }

        public void RemoveTArrayIndex<stacktype>(int index, out ItemStack returnstack)
        {
            if(typeof(stacktype) == typeof(Consumable_Ore))
            {
                Consumable_Ore ore = TArray.GetValue(index) as Consumable_Ore;
                returnstack = new ItemStack(ore.GetOreType(), 999, typeof(Consumable_Ore));
                returnstack.AddItem(ore);
                Debug.Log("return stack: " + returnstack);
            }
            else if (typeof(stacktype) == typeof(Consumable_Ingot))
            {
                Consumable_Ingot ingot = TArray.GetValue(index) as Consumable_Ingot;
                returnstack = new ItemStack(ingot.GetIngotType(), 99, typeof(Consumable_Ingot));
                returnstack.AddItem(ingot);
            }
            else if (typeof(stacktype) == typeof(Consumable_Material))
            {
                Consumable_Material mat = TArray.GetValue(index) as Consumable_Material;
                returnstack = new ItemStack(mat.GetMaterialID(), 99, typeof(Consumable_Material));
                returnstack.AddItem(mat);
            }
            else if (typeof(stacktype) == typeof(Consumable_Produce))
            {
                Consumable_Produce prod = TArray.GetValue(index) as Consumable_Produce;
                returnstack = new ItemStack(prod.ProduceType, 99, typeof(Consumable_Produce));
                returnstack.AddItem(prod);
            }
            else if (typeof(stacktype) == typeof(Consumable_Component))
            {
                Consumable_Component comp = TArray.GetValue(index) as Consumable_Component;
                returnstack = new ItemStack(comp.ComponentID, 99, typeof(Consumable_Component));
                returnstack.AddItem(comp);
            }
            else
            {
                returnstack = null;
            }
            TArray.SetValue(null, index);
            LastIndex--;
            //contract the TArray around this index
            for (int i = index; i < TArray.Length - 1; i++)
            {
                TArray.SetValue(TArray.GetValue(i + 1), i);
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
            //Debug.Log("");
            if (amountToRemove < 0) { amountToRemove *= -1; }
            ItemStack returnStack;
            //itemCount is the total amount in all indicies.
            if (itemCount - amountToRemove > 0)
            {
                itemCount -= amountToRemove;
                //Debug.Log("remains: "+itemCount);
            }
            else if(itemCount - amountToRemove == 0)
            {
                amountToRemove = itemCount;
                itemCount = 0;
                //Debug.Log("all taken");
            }
            else 
            { 
                return null;
            }
            float runningAmount = 0.0f;
            returnStack = new ItemStack(itemType, 999, _originalType);//create a standard sized container
            for (int i = TArray.Length - 1; i >= 0; i--)
            {
                if (runningAmount != amountToRemove)
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
                            tempOre.RemoveOreAmount(need);//amountToRemove
                            Consumable_Ore newOre = new Consumable_Ore(tempOre.GetOreType(),
                                tempOre.GetOreQuality(), tempOre.GetOreZone(), need);//amountToRemove
                            returnStack.AddItem(newOre);
                            //Debug.Log("Added: "+newOre.ToString());
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
                                tempOre.RemoveOreAmount(need);//amountToRemove
                                Consumable_Ore newOre = new Consumable_Ore(tempOre.GetOreType(),
                                    tempOre.GetOreQuality(), tempOre.GetOreZone(), need);//amountToRemove
                                returnStack.AddItem(newOre);
                                //Debug.Log("Added: " + newOre.ToString());
                                runningAmount += need;
                            }
                            //if the next index has some of the amount we need, and would be empty completely.
                            else
                            {
                                float need = tempOre.GetOreQuantity();
                                runningAmount += need;
                                tempOre.RemoveOreAmount(need);//amoutToRemove
                                Consumable_Ore newOre = new Consumable_Ore(tempOre.GetOreType(),
                                    tempOre.GetOreQuality(), tempOre.GetOreZone(), need);//amountToRemove
                                returnStack.AddItem(newOre);
                                //Debug.Log("Added: " + newOre.ToString());
                                TArray.SetValue(null, i);
                                lastIndex -= 1;
                            }
                        }
                        if (tempOre.GetOreQuantity() == 0)
                        {
                            TArray.SetValue(null, i);
                            lastIndex -= 1;
                        }
                    }
                    ///
                    /// Remove Ingots
                    ///
                    else if (TArray.GetValue(i) != null && _originalType == typeof(Consumable_Ingot))
                    {
                        Consumable_Ingot tempIng = TArray.GetValue(i) as Consumable_Ingot;//May throw null pointer
                        if (amountToRemove > 0)
                        {
                            //remove this ingot from TArray
                            returnStack.AddItem(tempIng);
                            TArray.SetValue(null, i);//empty the TArray. May just need to create a new one.
                            lastIndex -= 1;
                            //Debug.Log("Added: " + tempIng.ToString());
                            //subtract the mass of the ingot, because the max value is mass not quantity.
                            amountToRemove -= tempIng.GetIngotMass();
                        }
                        if (tempIng.GetIngotMass() == 0)
                        {
                            TArray.SetValue(null, i);
                            lastIndex -= 1;
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
                            Consumable_Component newComp = new Consumable_Component(tempComp.ComponentID,
                                (int)amountToRemove, tempComp.GetComponentDefinition());
                            returnStack.AddItem(newComp);
                            //Debug.Log("Added: " + newComp.ToString());
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
                                Consumable_Component newComp = new Consumable_Component(tempComp.ComponentID,
                                    (int)amountToRemove, tempComp.GetComponentDefinition());
                                returnStack.AddItem(newComp);
                                //Debug.Log("Added: " + newComp.ToString());
                                runningAmount += need;
                            }
                            //if the next index has some of the amount we need, and would be empty completely.
                            else
                            {
                                runningAmount += tempComp.GetQuantity();
                                tempComp.RemoveComponentAmount((int)amountToRemove);
                                Consumable_Component newComp = new Consumable_Component(tempComp.ComponentID,
                                    (int)amountToRemove, tempComp.GetComponentDefinition());
                                returnStack.AddItem(newComp);
                                //Debug.Log("Added: " + newComp.ToString());
                                TArray.SetValue(null, i);
                                lastIndex -= 1;
                            }
                        }
                        if (tempComp.GetQuantity() == 0)
                        {
                            TArray.SetValue(null, i);
                            lastIndex -= 1;
                        }
                    }
                    ///
                    /// Remove Produce
                    ///
                    else if (TArray.GetValue(i) != null && _originalType == typeof(Consumable_Produce))
                    {
                        Consumable_Produce tempProd = TArray.GetValue(i) as Consumable_Produce;//May throw null pointer
                        if (amountToRemove > 0)
                        {
                            //remove this ingot from TArray
                            returnStack.AddItem(tempProd);
                            TArray.SetValue(null, i);//empty the TArray. May just need to create a new one.
                            lastIndex -= 1;
                            //Debug.Log("Added: " + tempIng.ToString());
                            amountToRemove -= tempProd.GetProduceCount();
                        }
                        if (tempProd.GetProduceCount() == 0)
                        {
                            TArray.SetValue(null, i);
                            lastIndex -= 1;
                        }
                    }
                }
            }
            //check if this ItemStack realLength is 0
            //if(itemCount == 0)
            //{
            //    Debug.Log("ZERO");
            //    TArray.SetValue(null, 0);
            //}
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
            //Add the two itemstack Lengths (amounts)
            int tempCap = maxCount;
            itemCount += itemstack.Size();
            //now combine their data
            //We'll need to expand the array for when it goes overcap
            //Debug.Log("Len+item.Len: " + (Length() + itemstack.Length()));
            //Debug.Log("last+1: " + (lastIndex + 1));
            if ((Length()+itemstack.Length()) >= (lastIndex+1))//itemCount > maxCount
            {
                //Debug.Log("Adding " + itemstack.Length());
                TArray = ExpandArrayBy(itemstack.Length());
            }
            for (int j = 0; j < itemstack.Length(); j++)
            {
                //Debug.Log(j);
                if(itemstack.GetItemArray().GetValue(j) != null)
                {
                    //add every value in itemstack to TArray sequentially using lastIndex as the insertion point
                    //Debug.Log(j +": "+ itemstack.GetItemArray().GetValue(j));
                    //Debug.Log("ATarray.len: "+TArray.Length);
                    TArray.SetValue(itemstack.GetItemArray().GetValue(j), lastIndex++);
                }
            }
            if (itemCount > maxCount)
            {
                //remove any values beyond the maxCount
                //create a spillover itemstack that is as large as the surcharge
                ItemStack overflowStack;// = new ItemStack(itemstack.GetStackType(), tempCap - maxCount, itemstack.GetOriginalType());
                Debug.Log("Remove and add the surcharge to overflow");
                //Remove the surcharge from the itemstack and put it into the overflow stack
                Debug.Log("tempCap - maxCount:" + (tempCap - maxCount));
                overflowStack = RemoveItemData(tempCap - maxCount);
                Debug.Log("Returning overflow: " + overflowStack.ToString());
                return overflowStack;
            }
            else
            {
                //Debug.Log("No overflow");
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
            //Debug.Log(GetStackType() + " v " + comparee.GetStackType());
            if (GetStackType() == comparee.GetStackType())
            {
                if (_originalType == typeof(Consumable_Ore))//typePram
                {
                    //Debug.Log("comparing ore meta...");
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
                else if (_originalType == typeof(Consumable_Ingot))
                {
                    //Debug.Log("comparing ingot meta...");
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
                else if (_originalType == typeof(Consumable_Material))
                {

                }
                else if (_originalType == typeof(Consumable_Component))
                {
                    //Consumable_Component compCom = comparee.TArray.GetValue(0) as Consumable_Component;
                    //Consumable_Component oordCom = TArray.GetValue(0) as Consumable_Component;
                    //if(oordCom != null)
                    //{
                    //    if (oordCom.CompareMetaData(compCom))
                    //    {
                    //        Debug.Log("Itemstacks ARE EQUAL");
                            return true;
                    //    }
                    //    Debug.Log("ItemStacks not equal");
                    //}
                }
                else if(_originalType == typeof(Consumable_Produce))
                {
                    Consumable_Produce compProd = comparee.TArray.GetValue(0) as Consumable_Produce;
                    Consumable_Produce oordProd = TArray.GetValue(0) as Consumable_Produce;
                    //Only type and quality are checked. Mass and impurities are allowed to varry.
                    if (oordProd.CompareMetaData(compProd))
                    {
                        Debug.Log("Itemstacks ARE EQUAL");
                        return true;
                    }
                    Debug.Log("ItemStacks not equal");
                }
            }
            //Debug.Log("Types not equal");
            return false;
        }

        public int Length()
        {
            float size = 0.0f;
            for (int i = 0; i < TArray.Length; i++)
            {
                if (TArray.GetValue(i) != null)
                {
                    size = i + 1;
                }
            }
            return (int)size;
            //return TArray.Length;
        }

        public int LastIndex
        {
            get { return lastIndex; }
            set { lastIndex = value; }
        }

        /// <summary>
        /// Method that adds up and returns the quantities of all objects withing TArray
        /// </summary>
        /// <returns>The total number of items within each index of TArray</returns>
        public int GetRealLength()
        {
            float size = 0;
            //itemData length is the summation of the counts of the itemData items
            for (int i = 0; i < TArray.Length; i++)
            {
                if (TArray.GetValue(i) != null)
                {
                    float mass = 0f;
                    if (_originalType == typeof(Consumable_Ingot))
                    {
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
                    else if (_originalType == typeof(Consumable_Produce))
                    {
                        Consumable_Produce prod = TArray.GetValue(i) as Consumable_Produce;
                        mass = prod.GetProduceCount();
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
        public void SetItemCount(float cnt)
        {
            itemCount = cnt;
        }

        //used in reflection, insertion sorts
        public Category GetStackCategory()
        {
            return StackCategory;
        }

        public Category StackCategory
        {
            get { return stackCategory; }
            set { stackCategory = value; }
        }

        /// <summary>
		/// Use the type of the itemstack to determine the sorting inventory category
		/// </summary>
		private Category CalcSortCategory()
        {
            //weps
            if(_originalType == typeof(IGun_Customizable))
            {
                return Category.Weapon;
            }

            //gadgets
            else if(_originalType == typeof(IMachineWelder) || _originalType == typeof(MiningDrill))
            {
                return Category.Gadget;
            }

            //gear
            else if(_originalType == typeof(IEquipable))
            {
                return Category.Gear;
            }

            //consumables
            else if (_originalType == typeof(Consumable_Component) || _originalType == typeof(Consumable_Produce))
            {
                return Category.Consumable;
            }

            //ammo
            else if(_originalType == typeof(IBullet))
            {
                return Category.Ammo;
            }

            //rss
            if(_originalType == typeof(Consumable_Ingot) || _originalType == typeof(Consumable_Material) || _originalType == typeof(Consumable_Ore))
            {
                return Category.Resource;
            }

            //misc
            else
            {
                return Category.Misc;
            }
            
        }
    }
}