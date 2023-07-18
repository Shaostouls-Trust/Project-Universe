using ProjectUniverse.Items;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectUniverse.Base;
using ProjectUniverse.Production.Resources;
using ProjectUniverse.Data.Libraries.Definitions;
using ProjectUniverse.UI;
using ProjectUniverse.Items.Weapons;
using ProjectUniverse.Items.Tools;
using ProjectUniverse.Items.Consumable;

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

            Consumable_Ingot chrome = new Consumable_Ingot("Ingot_Chromium", 3, 10);
            Consumable_Ingot ironIng = new Consumable_Ingot("Ingot_Iron", 3, 10);
            Consumable_Ingot nickelIng = new Consumable_Ingot("Ingot_Nickel", 3, 10);
            Consumable_Material carbon = new Consumable_Material("Material_Carbon", 1);
           
            //Consumable_Ore Gore = new Consumable_Ore("Ore_Gold", 5, 5, 25);
            Consumable_Ore Iore = new Consumable_Ore("Ore_Iron", 3, 3, 10);
            Consumable_Ore Iore2 = new Consumable_Ore("Ore_Iron", 5, 5, 10);
            Consumable_Ore Nore = new Consumable_Ore("Ore_Nickel", 5, 5, 60);
            Consumable_Ore Core = new Consumable_Ore("Ore_Copper", 4, 5, 25);

            //create a gun
            //GameObject gun = Instantiate(Resources.Load<GameObject>("Prefabs/Resources/Equipable/Weapons/DevWeapon"));
            GameObject gun2 = Instantiate(Resources.Load<GameObject>("Prefabs/Resources/Equipable/Weapons/Scar"));
            //create a drill, welder
            GameObject drill = Instantiate(Resources.Load<GameObject>("Prefabs/Resources/Equipable/Tools/Drill"));
            GameObject welder = Instantiate(Resources.Load<GameObject>("Prefabs/Resources/Equipable/Tools/Welder"));
            //create a consumable
            GameObject throwgren = Instantiate(Resources.Load<GameObject>("Prefabs/Resources/Equipable/Consumables/Consumable_Grenade"));
            GameObject throwsmoke = Instantiate(Resources.Load<GameObject>("Prefabs/Resources/Equipable/Consumables/Consumable_SmokeGrenade"));
            GameObject cornseed = Instantiate(Resources.Load<GameObject>("Prefabs/Resources/Equipable/Consumables/Applyable_SeedCorn"));

            //gun.SetActive(false);
            gun2.SetActive(false);
            drill.SetActive(false);
            welder.SetActive(false);
            throwgren.SetActive(false);
            cornseed.SetActive(false);

            //Weapon_Gun wep_gun = gun.GetComponent<Weapon_Gun>();
            Weapon_Gun wep_gun2 = gun2.GetComponent<Weapon_Gun>();
            MiningDrill tool_drill = drill.GetComponent<MiningDrill>();
            IMachineWelder tool_welder = welder.GetComponent<IMachineWelder>();
            Consumable_Throwable cons_gren = throwgren.GetComponent<Consumable_Throwable>();
            Consumable_Throwable cons_smokegren = throwsmoke.GetComponent<Consumable_Throwable>();
            Consumable_Applyable apply_cornseed = cornseed.GetComponent<Consumable_Applyable>();

            //ItemStack gunstack = new ItemStack("Weapon_DevWeapon", 1, typeof(Weapon_Gun));
            ItemStack gunstack2 = new ItemStack("Weapon_Scar", 1, typeof(Weapon_Gun));
            ItemStack drillStack = new ItemStack("Tool_Drill", 1, typeof(MiningDrill));
            ItemStack welderStack = new ItemStack("Tool_Welder", 1 , typeof(IMachineWelder));
            ItemStack grenStack = new ItemStack("Consumable_Grenade", 4, typeof(Consumable_Throwable));
            ItemStack smokeStack = new ItemStack("Consumable_SmokeGrenade", 4, typeof(Consumable_Throwable));
            ItemStack seedStack = new ItemStack("Applyable_CornSeed", 9000, typeof(Consumable_Applyable));

            //gunstack.AddItem(wep_gun);
            gunstack2.AddItem(wep_gun2);
            drillStack.AddItem(tool_drill);
            welderStack.AddItem(tool_welder);
            grenStack.AddItem(cons_gren);
            smokeStack.AddItem(cons_smokegren);
            seedStack.AddItem(apply_cornseed);

            //p_inventory.Add(gunstack);
            p_inventory.Add(gunstack2);
            p_inventory.Add(drillStack);
            p_inventory.Add(welderStack);
            //p_inventory.Add(grenStack);
            //p_inventory.Add(smokeStack);
            //p_inventory.Add(seedStack);

            /*
            Consumable_Component comp1 = new Consumable_Component("Component_ControlInterface", 99);
            Consumable_Component comp2 = new Consumable_Component("Component_ElectricalComponents", 99);
            Consumable_Component comp3 = new Consumable_Component("Component_ElectronicsComponents", 99);
            Consumable_Component comp4 = new Consumable_Component("Component_CopperComponents", 99);
            Consumable_Component comp5 = new Consumable_Component("Component_FiberglassInsulation", 99);
            Consumable_Component comp6 = new Consumable_Component("Component_NickelComponents", 99);
            Consumable_Component comp7 = new Consumable_Component("Component_PalladiumComponents", 99);
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
            p_inventory.Add(devCompStack1);
            p_inventory.Add(devCompStack2);
            p_inventory.Add(devCompStack3);
            p_inventory.Add(devCompStack4);
            p_inventory.Add(devCompStack5);
            p_inventory.Add(devCompStack6);
            p_inventory.Add(devCompStack7);
            */
            Consumable_Component steelComps = Consumable_Component.ConstructComponent("Component_SteelComponents", 99);
            ItemStack devSteelCompStack = new ItemStack("Component_SteelComponents", 9000, typeof(Consumable_Component));
            devSteelCompStack.AddItem(steelComps);
            //p_inventory.Add(devSteelCompStack);

            Consumable_Component ironComps = Consumable_Component.ConstructComponent("Component_IronComponents", 99);
            ItemStack devIronCompStack = new ItemStack("Component_IronComponents", 9000, typeof(Consumable_Component));
            devIronCompStack.AddItem(ironComps);
            //p_inventory.Add(devIronCompStack);

            Consumable_Component alumComps = Consumable_Component.ConstructComponent("Component_AluminumComponents", 99);
            ItemStack devAlumCompStack = new ItemStack("Component_AluminumComponents", 9000, typeof(Consumable_Component));
            devAlumCompStack.AddItem(alumComps);
            //p_inventory.Add(devAlumCompStack);

            Consumable_Component tinComps = Consumable_Component.ConstructComponent("Component_TinComponents", 99);
            ItemStack devTinCompStack = new ItemStack("Component_TinComponents", 9000, typeof(Consumable_Component));
            devTinCompStack.AddItem(tinComps);
            //p_inventory.Add(devTinCompStack);

            Consumable_Component nickelComps = Consumable_Component.ConstructComponent("Component_NickelComponents", 99);
            ItemStack devNickelCompStack = new ItemStack("Component_NickelComponents", 9000, typeof(Consumable_Component));
            devNickelCompStack.AddItem(nickelComps);
            //p_inventory.Add(devNickelCompStack);

            ItemStack devIOreStack = new ItemStack("Ore_Iron", 9000, typeof(Consumable_Ore));
            devIOreStack.AddItem(Iore);
            devIOreStack.AddItem(Iore2);
            ItemStack devNOreStack = new ItemStack("Ore_Nickel", 9000, typeof(Consumable_Ore));
            devNOreStack.AddItem(Nore);
            ItemStack devCOreStack = new ItemStack("Ore_Copper", 9000, typeof(Consumable_Ore));
            devCOreStack.AddItem(Core);
            ItemStack carbonStack = new ItemStack("Material_Carbon", 9000, typeof(Consumable_Material));
            carbonStack.AddItem(carbon);
            ItemStack ironStack = new ItemStack("Ingot_Iron", 9000, typeof(Consumable_Ingot));
            ironStack.AddItem(ironIng);
            ItemStack nickelStack = new ItemStack("Ingot_Nickel", 9000, typeof(Consumable_Ingot));
            nickelStack.AddItem(nickelIng);
            ItemStack chromeStack = new ItemStack("Ingot_Chromium", 9000, typeof(Consumable_Ingot));
            chromeStack.AddItem(chrome);
            ItemStack devIngotStack = new ItemStack("Ingot_Gold", 9000, typeof(Consumable_Ingot));
            int i = 0;
            while (i < 10)
            {
                devIngotStack.AddItem(ingot);
                i += 1;
            }

            p_inventory.Add(devIngotStack);
            p_inventory.Add(devIOreStack);
            p_inventory.Add(devNOreStack);
            p_inventory.Add(devCOreStack);
            p_inventory.Add(carbonStack);
            p_inventory.Add(chromeStack);
            p_inventory.Add(ironStack);
            p_inventory.Add(nickelStack);
            Debug.Log("Start Length: " + p_inventory.Count);
        }

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
            ItemStack wrapper = new ItemStack(type, 9000, typeof(itemtype));//itemScript.GetType() is the same as itemtype
            wrapper.AddItem(itemScript);//This adds 45Kg to one index
            Debug.Log("Adding stack: " + wrapper.ToString());
            AddStackToPlayerInventory(wrapper);
            return true;
        }

        public bool AddToPlayerInventory<itemtype>(Consumable_Ingot item)
        {
            Consumable_Ingot itemScript = item.GetComponent<itemtype>() as Consumable_Ingot;
            string type = itemScript.GetIngotType();
            ItemStack wrapper = new ItemStack(type, 9000, typeof(itemtype));//itemScript.GetType() is the same as itemtype
            wrapper.AddItem(itemScript);
            Debug.Log("Adding stack: " + wrapper.ToString());
            AddStackToPlayerInventory(wrapper);
            return true;
        }

        public bool AddToPlayerInventory<itemtype>(Consumable_Material item)
        {
            Consumable_Material itemScript = item.GetComponent<itemtype>() as Consumable_Material;
            string type = itemScript.GetMaterialID();
            ItemStack wrapper = new ItemStack(type, 9000, typeof(itemtype));//itemScript.GetType() is the same as itemtype
            wrapper.AddItem(itemScript);
            Debug.Log("Adding stack: " + wrapper.ToString());
            AddStackToPlayerInventory(wrapper);
            return true;
        }

        public bool AddToPlayerInventory<itemtype>(IEquipable eq)
        {
            //gun.SetActive(false); done in ExternalInteract
            if (typeof(itemtype) == typeof(Weapon_Gun))
            {
                Weapon_Gun wep_gun = eq.gameObject.GetComponent<Weapon_Gun>();
                ItemStack gunstack = new ItemStack(wep_gun.ID, 1, typeof(Weapon_Gun));
                gunstack.AddItem(wep_gun);
                AddStackToPlayerInventory(gunstack);
            }
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