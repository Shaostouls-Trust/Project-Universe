using ProjectUniverse.Items;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectUniverse.Base;
using ProjectUniverse.Production.Resources;
using ProjectUniverse.Data.Libraries.Definitions;
using ProjectUniverse.UI;

namespace ProjectUniverse.Player
{
    /// <summary>
    /// This class is for the player's inventory, from equiped gear to carried items like ores, components, etc.
    /// </summary>
    public sealed class IPlayer_Inventory : MonoBehaviour
    {
        [Header("Player Equipment")]
        //[SerializeField] private IEquipable p_head;//helmet, gasmask, etc
        //[SerializeField] private IEquipable p_eyes;//goggles, glasses, etc
        //[SerializeField] private IEquipable p_ears;//radio, listening devices, etc
        //[SerializeField] private IEquipable p_mouth;//oxygen gear, mic, etc. Usually mutually exclusive/included with helmet.
        //[SerializeField] private IEquipable p_chest1;//main chestpiece
        //[SerializeField] private IEquipable p_chest2;//chest coverpiece. Pouches, grendoliers, ammo belts, armor, handheld equipment, etc
        //[SerializeField] private IEquipable p_waist;//more ammo, knives, grenades, medkits, handheld tools, etc
        //[SerializeField] private IEquipable p_legsUpperL;//sidearm, ammo, pouches, etc.
        //[SerializeField] private IEquipable p_legsUpperR;//same options as left leg
        //[SerializeField] private IEquipable p_legsLower;//padding, armor, water pouches, etc.
        //[SerializeField] private IEquipable p_feet;//boots, shoes, etc
        [Header("Inventory")]
        //Will be used with inventory to affect player weight in realtime
        private Rigidbody playerRigidbody;
        public float inventoryWeight;
        private List<ItemStack> p_inventory = new List<ItemStack>();
        [SerializeField] private InventoryUIController inventoryUI;

        void Start()
        {
            playerRigidbody = GetComponent<Rigidbody>();
            Consumable_Ingot ingot = new Consumable_Ingot("Ingot_Gold", 3, 10);
           
            Consumable_Ore Gore = new Consumable_Ore("Ore_Gold", 5, 5, 10);
            Consumable_Ore Iore = new Consumable_Ore("Ore_Iron", 5, 5, 25);
            Consumable_Ore Nore = new Consumable_Ore("Ore_Nickel", 5, 5, 25);
            Consumable_Ore Core = new Consumable_Ore("Ore_Copper", 5, 5, 25);

            /*
           Consumable_Component comp1 = new Consumable_Component("Component_ControlInterface", 99);
           Consumable_Component comp2 = new Consumable_Component("Component_ElectricalComponents", 99);
           Consumable_Component comp3 = new Consumable_Component("Component_ElectronicsComponents", 99);
           Consumable_Component comp4 = new Consumable_Component("Component_CopperComponents", 99);
           Consumable_Component comp5 = new Consumable_Component("Component_FiberglassInsulation", 99);
           Consumable_Component comp6 = new Consumable_Component("Component_NickelComponents", 99);
           Consumable_Component comp7 = new Consumable_Component("Component_PalladiumComponents", 99);
            */
            ItemStack devGOreStack = new ItemStack("Ore_Gold", 999, typeof(Consumable_Ore));
           devGOreStack.AddItem(Gore);
           ItemStack devIOreStack = new ItemStack("Ore_Iron", 999, typeof(Consumable_Ore));
           devIOreStack.AddItem(Iore);
           ItemStack devNOreStack = new ItemStack("Ore_Nickel", 999, typeof(Consumable_Ore));
           devNOreStack.AddItem(Nore);
           ItemStack devCOreStack = new ItemStack("Ore_Copper", 999, typeof(Consumable_Ore));
           devCOreStack.AddItem(Core);
          
            /*
            ItemStack devCompStack1 = new ItemStack("Component_ControlInterface", 999, typeof(Consumable_Component));
            devCompStack1.AddItem(comp1);
            ItemStack devCompStack2 = new ItemStack("Component_ElectricalComponents", 999, typeof(Consumable_Component));
            devCompStack2.AddItem(comp2);
            ItemStack devCompStack3 = new ItemStack("Component_ElectronicsComponents", 999, typeof(Consumable_Component));
            devCompStack3.AddItem(comp3);
            ItemStack devCompStack4 = new ItemStack("Component_CopperComponents", 999, typeof(Consumable_Component));
            devCompStack4.AddItem(comp4);
            ItemStack devCompStack5 = new ItemStack("Component_FiberglassInsulation", 999, typeof(Consumable_Component));
            devCompStack5.AddItem(comp5);
            ItemStack devCompStack6 = new ItemStack("Component_NickelComponents", 999, typeof(Consumable_Component));
            devCompStack6.AddItem(comp6);
            ItemStack devCompStack7 = new ItemStack("Component_PalladiumComponents", 999, typeof(Consumable_Component));
            devCompStack7.AddItem(comp7);
           */
            ItemStack devIngotStack = new ItemStack("Ingot_Gold", 999, typeof(Consumable_Ingot));
            int i = 0;
            while (i < 10)
            {
                devIngotStack.AddItem(ingot);
                i += 1;
            }
            p_inventory.Add(devIngotStack);
           
           p_inventory.Add(devGOreStack);
           p_inventory.Add(devIOreStack);
           p_inventory.Add(devNOreStack);
           p_inventory.Add(devCOreStack);
            /*
          p_inventory.Add(devCompStack1);
          p_inventory.Add(devCompStack2);
          p_inventory.Add(devCompStack3);
          p_inventory.Add(devCompStack4);
          p_inventory.Add(devCompStack5);
          p_inventory.Add(devCompStack6);
          p_inventory.Add(devCompStack7);*/
            Debug.Log("Start Length: " + p_inventory.Count);
        }

        /*
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                inventoryUI.ToggleDisplay();
                //Debug.Log("Length: " + p_inventory.Count);
                foreach(ItemStack itemstack in p_inventory)
                {
                    Debug.Log(itemstack.ToString());
                }
                inventoryUI.UpdateDisplay();
            }
        }
        */
        //public PlayerInventoryUIController GetPlayerInventoryUI()
        //{
        //    return inventoryUI;
        //}
        public InventoryUIController InventoryUI
        {
            get { return inventoryUI; }
            set { inventoryUI = value; }
        }

        public List<ItemStack> GetPlayerInventory()
        {
            return p_inventory;
        }
        public ItemStack RemoveFromPlayerInventory(ItemStack item)
        {
            ItemStack rtn = item;
            p_inventory.Remove(item);
            return rtn;
        }
        public ItemStack RemoveFromPlayerInventory(int atIndex)
        {
            ItemStack returnstack = p_inventory[atIndex];
            //p_inventory[atIndex].RemoveItemData(p_inventory[atIndex].Size());
            Debug.Log("index: " + atIndex);
            //Debug.Log("removing: "+p_inventory[atIndex]);
            p_inventory.RemoveAt(atIndex);
            //Debug.Log(p_inventory[0]);
            return returnstack;
        }
        public ItemStack RemoveFromPlayerInventory(int atIndex, float amount)//ItemStack item, 
        {
            //return p_inventory.Remove(item);
            //Debug.Log("Attempting to find and remove item from player inventory");
            return p_inventory[atIndex].RemoveItemData(amount);
            //return p_inventory.Find(x => x.GetStackType() == item.GetStackType()).RemoveItemData(amount);
        }
        /// <summary>
        /// Remove a certain index from a certain itemstack
        /// </summary>
        public ItemStack RemoveFromPlayerInventory<stacktype>(ItemStack removeFromStack, int atIndex)
        {
            int stackIndex = -1;
            for (int i = 0; i < p_inventory.Count; i++)
            {
                if(p_inventory[i] == removeFromStack)
                {
                    stackIndex = i;
                }
            }
            if(stackIndex != -1)
            {
                Debug.Log("Removing: " + p_inventory[stackIndex].GetItemArray().GetValue(atIndex));
                p_inventory[stackIndex].RemoveTArrayIndex<stacktype>(atIndex, out ItemStack stack);
                if(p_inventory[stackIndex].GetRealLength() <= 0f)
                {
                    p_inventory.RemoveAt(stackIndex);
                }
                return stack;
            }
            else
            {
                return null;
            }
        }

        //Called by pickupable to be picked up
        public bool AddToPlayerInventory<itemtype>(Consumable_Ore item)//THIS ADDS 45Kg OF ORE INTO ONE INDEX?
        {
            Consumable_Ore itemScript = item.GetComponent<itemtype>() as Consumable_Ore;
            string type = itemScript.GetOreType();
            ItemStack wrapper = new ItemStack(type, 999, typeof(itemtype));//itemScript.GetType() is the same as itemtype
            wrapper.AddItem(itemScript);//This adds 45Kg to one index
            Debug.Log("Adding stack: " + wrapper.ToString());
            AddStackToPlayerInventory(wrapper);
            return true;
        }

        public bool AddToPlayerInventory<itemtype>(Consumable_Ingot item)
        {
            Consumable_Ingot itemScript = item.GetComponent<itemtype>() as Consumable_Ingot;
            string type = itemScript.GetIngotType();
            ItemStack wrapper = new ItemStack(type, 999, typeof(itemtype));//itemScript.GetType() is the same as itemtype
            wrapper.AddItem(itemScript);
            Debug.Log("Adding stack: " + wrapper.ToString());
            AddStackToPlayerInventory(wrapper);
            return true;
        }

        public bool AddToPlayerInventory<itemtype>(Consumable_Material item)
        {
            Consumable_Material itemScript = item.GetComponent<itemtype>() as Consumable_Material;
            string type = itemScript.GetMaterialID();
            ItemStack wrapper = new ItemStack(type, 999, typeof(itemtype));//itemScript.GetType() is the same as itemtype
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
                    if (i != j && p_inventory[j] != null && p_inventory[i] != null)
                    {
                        if (p_inventory[j].CompareMetaData(p_inventory[i]))
                        {
                            Debug.Log(p_inventory[j].GetRealLength() + "<" + p_inventory[j].GetMaxAmount() + ";" + p_inventory[i].GetRealLength() + "<" + p_inventory[i].GetMaxAmount());
                            //No point running this if one or both stacks are maxed. (GetRealLength was Length)
                            if (p_inventory[j].GetRealLength() < p_inventory[j].GetMaxAmount() && p_inventory[i].GetRealLength() < p_inventory[i].GetMaxAmount())
                            {
                                Debug.Log("Add item at index " + i + " with index " + j);
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
                                else if (overflowType == null)
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
            if (overflow.GetRealLength() > overflow.GetMaxAmount())//Length
            {
                float stacksToMake = overflow.GetRealLength() % overflow.GetMaxAmount();
                Debug.Log("Stacks remaining to be made: " + stacksToMake);
                //ItemStack newItemStack = new ItemStack(overflow.GetStackType(), overflow.Length(), overflow.GetMaxAmount());
                //ItemStack overflowItemStack = newItemStack.AddItemStack(overflow);
                //Debug.Log("Recurr Adding Item:" + newItemStack.ToString());
                //p_inventory.Add(newItemStack);//this adding causes an inf loop in the above for?
                //Debug.Log("Supposed to recur here");
                //SplitMaxedStacks(overflowItemStack);
            }
            else
            {
                if (overflow.Length() != 0 || overflow.GetStackType() != null)
                {
                    Debug.Log("Recurr end. (not) Adding Item:" + overflow.ToString());
                    //p_inventory.Add(overflow);//adds infinitely, because this adds overflow back into the stack
                    //which incr the length of loop above this, sending it into a second run, which then runs this again.
                }
            }
        }

        /// <summary>
        /// Returns X: the position of the comp in the player inventory
        ///         Y: the amount of said component present in the inventory
        /// </summary>
        /// <param name="comp"></param>
        /// <returns></returns>
        public Consumable_Component[] SearchInventoryForComponent(IComponentDefinition comp, out int lastIndex)
        {
            //int index = 0;
            //int amount = 0;
            lastIndex = -1;
            List<Consumable_Component> components = new List<Consumable_Component>();
            for (int i = 0; i < p_inventory.Count; i++)
            {
                if (p_inventory[i].GetOriginalType() == typeof(Consumable_Component))
                {
                    //Debug.Log(p_inventory[i].GetStackType() + " == " + comp.GetComponentType());
                    if (p_inventory[i].GetStackType() == comp.GetComponentType())
                    {
                        lastIndex = i;
                        Consumable_Component itemstackComponent = p_inventory[i].GetItemArray().GetValue(0) as Consumable_Component;
                        components.Add(itemstackComponent);
                    }
                }
            }
            return components.ToArray();
        }

        public float GetInventoryWeight()
        {
            return inventoryWeight;
        }
        public Rigidbody GetRigidbody()
        {
            return playerRigidbody;
        }
        public void OnLoad(params object[] data)
        {
            p_inventory = (List<ItemStack>)data[0];
            //playerRigidbody = (Rigidbody)data[1];
            inventoryWeight = (float)data[1];//2
        }

    }
}