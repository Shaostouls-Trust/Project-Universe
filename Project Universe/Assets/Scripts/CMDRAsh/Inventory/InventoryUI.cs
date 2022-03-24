using ProjectUniverse.Base;
using ProjectUniverse.Player;
using ProjectUniverse.Production.Resources;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static FleetBoyItemButton;
using ProjectUniverse.Util;
using static ProjectUniverse.Base.ItemStack;
using UnityEngine.UI;
using System.Reflection;
using ProjectUniverse.Items.Containers;
using ProjectUniverse.UI;
using UnityEngine.InputSystem;
using MLAPI;
using ProjectUniverse.Player.PlayerController;
using ProjectUniverse.Items.Weapons;
using ProjectUniverse.Items;
using ProjectUniverse.Items.Tools;
using ProjectUniverse.Items.Consumable;

public class InventoryUI : MonoBehaviour
{
    private ProjectUniverse.PlayerControls controls;
    [SerializeField] private GameObject itemparent;
    [SerializeField] private GameObject itembuttonpref;
    [SerializeField] private CargoContainer container;//non-player1 inventory
    [SerializeField] private List<GameObject> fbibs = new List<GameObject>();
    [SerializeField] private TMP_Text description;
    [SerializeField] private TMP_Text statistics;
    [SerializeField] private IPlayer_Inventory playerInventory;
    [SerializeField] private Button wepButton;
    [SerializeField] private Button toolButton;
    [SerializeField] private Button gadButton;
    [SerializeField] private Button gearButton;
    [SerializeField] private Button consButton;
    [SerializeField] private Button ammoButton;
    [SerializeField] private Button rssButton;
    [SerializeField] private Button miscButton;
    [SerializeField] private Color32 enabledColor;
    [SerializeField] private Color32 disabledColor;
    private bool showWeps = true;
    private bool showTools = true;
    private bool showGadgets = true;
    private bool showGear = true;
    private bool showCons = true;
    private bool showAmmo = true;
    private bool showRss = true;
    private bool showMisc = true;
    private bool transfermode = false;
    private InventoryUIController invUICont;

    private FleetBoyItemButton selectedButton;

    public FleetBoyItemButton SelectedButton
    {
        get { return selectedButton; }
        set { selectedButton = value; }
    }

    public bool TransferMode
    {
        get { return transfermode; }
        set { transfermode = value; }
    }

    /// <summary>
    /// Clear all buttons, instantiate and add per object
    /// </summary>
    public void PopulateInventoryScreen()
    {
        //clear any existing buttons
        for (int i = itemparent.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(itemparent.transform.GetChild(i).gameObject);
        }
        fbibs = new List<GameObject>();
        //get inventory
        List<ItemStack> inventory;
        if (playerInventory != null)
        {
            inventory = playerInventory.GetPlayerInventory();
        }
        else
        {
            inventory = container.GetInventory();
        }
        //sort inventory
        //Debug.Log("Sorting Inventory");//[DISABLED] 
        if(inventory.Count > 0)
        {
            MethodInfo info = inventory[0].GetType().GetMethod("GetStackCategory");
            Utils.InsertionSort<int>(ref inventory, info, true);
        }
        Category cat;
        for (int t = 0; t < inventory.Count; t++)
        {
            cat = inventory[t].StackCategory;
            if (CheckSortState(cat))
            {
                //Debug.Log("Found Stack: "+inventory[t]);
                //instantiate some item buttons
                GameObject instanceButton = Instantiate(itembuttonpref, itemparent.transform);
                FleetBoyItemButton fbb = instanceButton.GetComponent<FleetBoyItemButton>();
                fbibs.Add(instanceButton);
                string[] name = inventory[t].GetStackType().Split('_');
                if(name[0] == "Weapon" || name[0] == "Tool")
                {
                    fbb.ItemName = name[1];
                }
                else
                {
                    if (name.Length == 2)
                    {
                        fbb.ItemName = name[0] + " " + name[1];
                    }
                    else 
                    {
                        fbb.ItemName = name[0];
                    }
                    
                }
                fbb.Count = inventory[t].GetRealLength();
                //using the itemtype, create some temp vars
                Type ty = inventory[t].GetOriginalType();
                //make a library? Temp?
                string desc = "";
                string stats = "";
                if (ty == typeof(Consumable_Ore))
                {
                    desc = "A metal-bearing rock that can be refined into ingots.";
                    Consumable_Ore ore = (Consumable_Ore)inventory[t].GetItemArray().GetValue(inventory[t].LastIndex - 1);
                    stats = "Quality: " + ore.GetOreQuality() + "\n"
                        + "Zone: " + ore.GetOreZone();
                }
                else if (ty == typeof(Consumable_Ingot))
                {
                    desc = "A standardized metal bar used for a variety of purposes.";
                    Consumable_Ingot ingot = (Consumable_Ingot)inventory[t].GetItemArray().GetValue(inventory[t].LastIndex - 1);
                    stats = "Quality: " + ingot.GetIngotQuality() + "\n"
                        + "Mass per ingot: " + ingot.GetIngotMass();
                }
                else if (ty == typeof(Consumable_Component))
                {
                    desc = "A component used in the construction, operation, and repair of a machine.";
                    Consumable_Component comp = (Consumable_Component)inventory[t].GetItemArray().GetValue(inventory[t].LastIndex - 1);
                    stats = "Durability: " + comp.RemainingHealth + "/" + comp.HealthValue + "\n"
                        + "Priority: " + comp.GetPriority();
                }
                else if(ty == typeof(Weapon_Gun))
                {
                    desc = "A gun. It shoots things. Don't point it at things you don't want dead!";
                    Weapon_Gun gun = (Weapon_Gun)inventory[t].GetItemArray().GetValue(inventory[t].LastIndex - 1);
                    stats = "Caliber: " + gun.Base.Caliber + "\n" + "Muzzle Velocity: " + gun.Base.MuzzleVelocity;
                }
                else if(ty == typeof(MiningDrill))
                {
                    desc = "A hand-held laser drill. Carves off materials when mining.";
                    MiningDrill drill = (MiningDrill)inventory[t].GetItemArray().GetValue(inventory[t].LastIndex - 1);
                    stats = "Mining Speed: "+drill.MineAmount;
                }
                else if(ty == typeof(Consumable_Throwable))
                {
                    desc = "Throw it and something happens. Try it on your friend!";
                    Consumable_Throwable c_throw = (Consumable_Throwable)inventory[t].GetItemArray().GetValue(inventory[t].LastIndex - 1);
                    stats = "" + c_throw.ID;
                }
                else if(ty == typeof(Consumable_Applyable))
                {
                    Consumable_Applyable c_throw = (Consumable_Applyable)inventory[t].GetItemArray().GetValue(inventory[t].LastIndex - 1);
                    if (c_throw.ThisApplyableType == Consumable_Applyable.ApplyableType.Seed)
                    {
                        desc = "Seeds. Plant them in soil.";
                        stats = "" + c_throw.ID;
                    }
                    else if(c_throw.ThisApplyableType == Consumable_Applyable.ApplyableType.Healthpack)
                    {
                        desc = "Healthpack. Used to heal wounds.";
                        stats = "" + c_throw.ID;
                    }
                    else if (c_throw.ThisApplyableType == Consumable_Applyable.ApplyableType.Ammopack)
                    {
                        desc = "Ammopack. Resupplies some ammo.";
                        stats = "" + c_throw.ID;
                    }
                }
                //description.text = desc;
                //statistics.text = stats;
                fbb.ItemCategory = cat;
                fbb.Description = desc;
                fbb.Stats = stats;
                fbb.DescTMP = description;
                fbb.StatTxt = statistics;
                fbb.SelectedStack = inventory[t];
                fbb.UI = this;
                //check if the item is equiped (DEFINIETLY A BETTER WAY TO DO THIS!)
                if(cat == Category.Gear || cat == Category.Weapon || cat == Category.Gadget || cat == Category.Tool)
                {
                    SupplementalController sc = playerInventory.gameObject.GetComponent<SupplementalController>();
                    if (playerInventory.gameObject.GetComponent<SupplementalController>().IsEquipped(inventory[t].GetItemArray().GetValue(0) as IEquipable))
                    {
                        //Highlight the fbb
                        IEquipable eq = fbb.SelectedStack.GetItemArray().GetValue(0) as IEquipable;
                        if (eq == sc.EquippedWeapons[0])
                        {
                            fbb.HighlightButton(0);
                        }
                        else if(eq == sc.EquippedWeapons[1])
                        {
                            fbb.HighlightButton(1);
                        }
                        if (eq == sc.EquippedTools[0])
                        {
                            fbb.HighlightButton(0);
                        }
                        else if (eq == sc.EquippedTools[1])
                        {
                            fbb.HighlightButton(1);
                        }
                    }
                    else
                    {
                        fbb.UnhighlightButton();
                    }
                }
            }
        }
    }

    private void OnEnable()
    {
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var networkedClient))
        {
            controls = networkedClient.PlayerObject.gameObject.GetComponent<SupplementalController>().PlayerController;
        }
        else
        {
            controls = new ProjectUniverse.PlayerControls();
        }

        controls.Player.Inv_Drop.Enable();
        controls.Player.Inv_Transfer.Enable();
        controls.Player.Inv_Use.Enable();
        controls.Player.Num1.Enable();
        controls.Player.Num2.Enable();
    }

    private void OnDisable()
    {
        controls.Player.Inv_Drop.Disable();
        controls.Player.Inv_Transfer.Disable();
        controls.Player.Inv_Use.Disable();
        controls.Player.Num1.Disable();
        controls.Player.Num2.Disable();
    }

    /// Bind methods to buttons
    /// 
    void Start()
    {
        wepButton.onClick.AddListener(delegate { ButtonInputHandler(0); ToggleButtonStateTo(wepButton, showWeps); });
        toolButton.onClick.AddListener(delegate { ButtonInputHandler(1); ToggleButtonStateTo(toolButton, showTools); });
        gadButton.onClick.AddListener(delegate { ButtonInputHandler(2); ToggleButtonStateTo(gadButton, showGadgets); });
        gearButton.onClick.AddListener(delegate { ButtonInputHandler(3); ToggleButtonStateTo(gearButton, showGear); });
        consButton.onClick.AddListener(delegate { ButtonInputHandler(4); ToggleButtonStateTo(consButton, showCons); });
        ammoButton.onClick.AddListener(delegate { ButtonInputHandler(5); ToggleButtonStateTo(ammoButton, showAmmo); });
        rssButton.onClick.AddListener(delegate { ButtonInputHandler(6); ToggleButtonStateTo(rssButton, showRss); });
        miscButton.onClick.AddListener(delegate { ButtonInputHandler(7); ToggleButtonStateTo(miscButton, showMisc); });

        controls.Player.Inv_Drop.performed += ctx =>
        {
            DropSelected();
        };
        controls.Player.Inv_Transfer.performed += ctx =>
        {
            TransferSelected();
        };
        controls.Player.Inv_Use.performed += ctx =>
        {
            UseSelected();
        };
        controls.Player.Num1.performed += ctx =>
        {
            EquipToQuickSelect(1);
        };
        controls.Player.Num2.performed += ctx =>
        {
            EquipToQuickSelect(2);
        };
    }

    ///
    ///Handle keyboard inputs: Use, Drop, Transfer
    ///

    public void UseSelected()
    {
        Debug.Log("Attempt to use");
        SupplementalController controller = playerInventory.gameObject.GetComponent<SupplementalController>();
        //use item. Equip to the right body slot, or use immediately
        if (selectedButton.ItemCategory == Category.Gear)
        {

        }
        //Use IEquipable to get the slot. Grab the gun GO and parent the gun to that slot.
        else if (selectedButton.ItemCategory == Category.Weapon)
        {
            // clear the EquippedWeapons thing?
            for(int i = 0;i < controller.EquippedWeapons.Length; i++)
            {
                if(controller.EquippedWeapons[i] != null)
                {
                    //controller.DequipItem(controller.EquippedWeapons[i].gameObject);
                }
            }
            //dequip the other guns and tools
            if (controller.RightHandEquipped != null)
            {
                controller.DequipItem(controller.RightHandEquipped.gameObject);
            }
            //Equip the selected gun
            Weapon_Gun gun = selectedButton.SelectedStack.GetItemArray().GetValue(0) as Weapon_Gun;
            playerInventory.gameObject.GetComponent<SupplementalController>().EquipItem(gun.gameObject, gun.EquipmentSlot);
        }
        else if (selectedButton.ItemCategory == Category.Tool)
        {
            //dequip other tools and weapons
            if (controller.RightHandEquipped != null)
            {
                controller.DequipItem(controller.RightHandEquipped.gameObject);
            }
            //Equip the drill
            MiningDrill drill = selectedButton.SelectedStack.GetItemArray().GetValue(0) as MiningDrill;
            playerInventory.gameObject.GetComponent<SupplementalController>().EquipItem(drill.gameObject, drill.EquipmentSlot);
        }
        else if (selectedButton.ItemCategory == Category.Gadget)
        {

        }
        else if (selectedButton.ItemCategory == Category.Consumable)
        {
            //if medical or food, use immediately? Otherwise equip to hands. LMB or RMB equip to different hands.
            if (controller.RightHandEquipped != null)
            {
                controller.DequipItem(controller.RightHandEquipped.gameObject);
            }
            // check the type of consumable: Applyable or throwable
            if (selectedButton.SelectedStack.GetOriginalType() == typeof(Consumable_Throwable))
            {
                Consumable_Throwable cons = selectedButton.SelectedStack.GetItemArray().GetValue(0) as Consumable_Throwable;
                playerInventory.gameObject.GetComponent<SupplementalController>().EquipItem(cons.gameObject, cons.EquipmentSlot);
            }
            else if (selectedButton.SelectedStack.GetOriginalType() == typeof(Consumable_Applyable))
            {
                Consumable_Applyable cons = selectedButton.SelectedStack.GetItemArray().GetValue(0) as Consumable_Applyable;
                playerInventory.gameObject.GetComponent<SupplementalController>().EquipItem(cons.gameObject, cons.EquipmentSlot);
            }
            
        }
    }

    public void EquipToQuickSelect(int i)
    {
        SupplementalController controller = playerInventory.gameObject.GetComponent<SupplementalController>();
        if (SelectedButton.ItemCategory == Category.Weapon)
        {
            if (i == 1)
            {
                //clear the selection of the first weapon slot
                for(int a = 0; a < fbibs.Count; a++)
                {
                    if(fbibs[a].GetComponent<FleetBoyItemButton>().SelectedStack.GetItemArray().GetValue(0) as IEquipable == controller.EquippedWeapons[0])
                    {
                        fbibs[a].GetComponent<FleetBoyItemButton>().UnhighlightButton();
                        break;
                    }
                }
                //Set this item to the first weapon slot
                controller.EquippedWeapons[0] = (SelectedButton.SelectedStack.GetItemArray().GetValue(0) as IEquipable);
                SelectedButton.HighlightButton(0);
            }
            else
            {
                for (int a = 0; a < fbibs.Count; a++)
                {
                    if (fbibs[a].GetComponent<FleetBoyItemButton>().SelectedStack.GetItemArray().GetValue(0) as IEquipable == controller.EquippedWeapons[1])
                    {
                        fbibs[a].GetComponent<FleetBoyItemButton>().UnhighlightButton();
                        break;
                    }
                }
                //set item to second wep slot
                controller.EquippedWeapons[1] = (SelectedButton.SelectedStack.GetItemArray().GetValue(0) as IEquipable);
                SelectedButton.HighlightButton(1);
            }
        }
        else if(SelectedButton.ItemCategory == Category.Tool)
        {
            if (i == 1)
            {
                //Set this item to the first tool slot
                for (int a = 0; a < fbibs.Count; a++)
                {
                    if (fbibs[a].GetComponent<FleetBoyItemButton>().SelectedStack.GetItemArray().GetValue(0) as IEquipable == controller.EquippedTools[0])
                    {
                        fbibs[a].GetComponent<FleetBoyItemButton>().UnhighlightButton();
                        break;
                    }
                }
                //Set this item to the first weapon slot
                controller.EquippedTools[0] = (SelectedButton.SelectedStack.GetItemArray().GetValue(0) as IEquipable);
                SelectedButton.HighlightButton(0);
            }
            else
            {
                //set item to second tool slot
                for (int a = 0; a < fbibs.Count; a++)
                {
                    if (fbibs[a].GetComponent<FleetBoyItemButton>().SelectedStack.GetItemArray().GetValue(0) as IEquipable == controller.EquippedTools[1])
                    {
                        fbibs[a].GetComponent<FleetBoyItemButton>().UnhighlightButton();
                        break;
                    }
                }
                //Set this item to the first weapon slot
                controller.EquippedTools[1] = (SelectedButton.SelectedStack.GetItemArray().GetValue(0) as IEquipable);
                SelectedButton.HighlightButton(1);
            }
        }
        else if (SelectedButton.ItemCategory == Category.Consumable)
        {
            if (i == 1)
            {
                //Set this item to the first cons slot
                for (int a = 0; a < fbibs.Count; a++)
                {
                    if (fbibs[a].GetComponent<FleetBoyItemButton>().SelectedStack.GetItemArray().GetValue(0) as IEquipable == controller.EquippedConsumables[0])
                    {
                        fbibs[a].GetComponent<FleetBoyItemButton>().UnhighlightButton();
                        break;
                    }
                }
                controller.EquippedConsumables[0] = (SelectedButton.SelectedStack.GetItemArray().GetValue(0) as IEquipable);
                SelectedButton.HighlightButton(0);
            }
            else
            {
                //set item to second consum slot
                for (int a = 0; a < fbibs.Count; a++)
                {
                    if (fbibs[a].GetComponent<FleetBoyItemButton>().SelectedStack.GetItemArray().GetValue(0) as IEquipable == controller.EquippedConsumables[1])
                    {
                        fbibs[a].GetComponent<FleetBoyItemButton>().UnhighlightButton();
                        break;
                    }
                }
                //Set this item to the first weapon slot
                controller.EquippedConsumables[1] = (SelectedButton.SelectedStack.GetItemArray().GetValue(0) as IEquipable);
                SelectedButton.HighlightButton(1);
            }
        }
    }

    public void DropSelected()
    {
        Debug.Log("Drop Selected");
        Vector3 position;
        GameObject obj;
        RaycastHit hit;
        Vector3 forward = Camera.main.transform.TransformDirection(0f, 0f, 1f) * 1f;
        //Raycast along player vision axis until you either hit something, or reach 1m. If hit, offset by 0.25m
        if (Physics.Raycast(this.transform.position, forward, out hit, 1f))
        {
            position = hit.point + (forward * -0.25f);
        }
        else
        {
            position = this.transform.position + forward;
        }

        Type ss = selectedButton.SelectedStack.GetOriginalType();
        Debug.Log(ss.BaseType);
        if (ss.BaseType == typeof(IEquipable))
        {
            //Itemstack to be dropped
            ItemStack stk;
            ///
            /// Weapon_Gun
            ///
            if (ss == typeof(Weapon_Gun))
            {
                //move to position, activate GO and rb gravity
                Debug.Log("Drop Gun -- set position");
                Weapon_Gun gun = (selectedButton.SelectedStack.GetItemArray().GetValue(0) as Weapon_Gun);
                gun.gameObject.transform.position = position;
                //enable the gun to be picked up
                gun.gameObject.GetComponent<InteractionElement>().Parameter = 0;
                Rigidbody rb = gun.gameObject.GetComponent<Rigidbody>();
                rb.detectCollisions = true;
                rb.isKinematic = false;
                rb.useGravity = true;
                gun.gameObject.SetActive(true);
                //remove from inventory
                Debug.Log("Drop Gun -- remove from inventory");
                if (playerInventory != null)
                {
                    stk = playerInventory.RemoveFromPlayerInventory<Weapon_Gun>(selectedButton.SelectedStack,
                        (selectedButton.SelectedStack.LastIndex - 1));
                }
                else
                {
                    stk = container.RemoveFromInventory<Weapon_Gun>(selectedButton.SelectedStack, (selectedButton.SelectedStack.LastIndex - 1));
                }
                selectedButton.Count--;
                Debug.Log(stk);
            }
            
            
        }
        else
        {
            //find item prefab
            string prefix = "Prefabs\\Resources\\";
            //remove the item from inventory. Dropping an item will work based on itemstack index, not mass or itemcount.
            ///
            /// CONSUMABLE ORE
            ///
            if (selectedButton.SelectedStack.GetOriginalType() == typeof(Consumable_Ore))
            {
                prefix += "Ores\\";
                obj = Resources.Load(prefix + selectedButton.SelectedStack.GetStackType()) as GameObject;
                if (obj == null)
                {
                    obj = Resources.Load(prefix + "Ore_NoMat") as GameObject;
                }
                //Spawn the item in worldspace and pass parameters to it.
                GameObject oreworldspace = Instantiate(obj, position, Quaternion.identity);
                //drop item. 
                ItemStack stk;
                if (playerInventory != null)
                {
                    stk = playerInventory.RemoveFromPlayerInventory<Consumable_Ore>(selectedButton.SelectedStack,
                    (selectedButton.SelectedStack.LastIndex - 1));
                }
                else
                {
                    stk = container.RemoveFromInventory<Consumable_Ore>(selectedButton.SelectedStack, (selectedButton.SelectedStack.LastIndex - 1));
                }
                Debug.Log(stk);
                if (oreworldspace != null && stk != null)
                {
                    oreworldspace.GetComponent<Consumable_Ore>().RegenerateOre(stk.GetItemArray().GetValue(0) as Consumable_Ore);
                }
                selectedButton.Count -= stk.GetRealLength();
            }
            ///
            /// CONSUMABLE INGOT
            ///
            else if (selectedButton.SelectedStack.GetOriginalType() == typeof(Consumable_Ingot))
            {
                prefix += "Ingots\\";
                obj = Resources.Load(prefix + selectedButton.SelectedStack.GetStackType()) as GameObject;
                if (obj == null)
                {
                    obj = Resources.Load(prefix + "Ingot_Medium") as GameObject;
                }
                //Spawn the item in worldspace and pass parameters to it.
                GameObject ingotworldspace = Instantiate(obj, position, Quaternion.identity);
                //drop item. 
                ItemStack stk;
                if (playerInventory != null)
                {
                    stk = playerInventory.RemoveFromPlayerInventory<Consumable_Ingot>(selectedButton.SelectedStack,
                    (selectedButton.SelectedStack.LastIndex - 1));
                }
                else
                {
                    stk = container.RemoveFromInventory<Consumable_Ore>(selectedButton.SelectedStack, (selectedButton.SelectedStack.LastIndex - 1));
                }
                if (ingotworldspace != null && stk != null)
                {
                    ingotworldspace.GetComponent<Consumable_Ingot>().RegenerateIngot(stk.GetItemArray().GetValue(0) as Consumable_Ingot);
                }
                selectedButton.Count -= stk.GetRealLength();
            }
            ///
            /// Consumables
            ///
            else if (selectedButton.SelectedStack.GetOriginalType() == typeof(Consumable_Ingot))
            {
                prefix += "Consumables\\";
                obj = Resources.Load(prefix + selectedButton.SelectedStack.GetStackType()) as GameObject;
                if (obj == null)
                {
                    obj = Resources.Load(prefix + selectedButton.ItemName) as GameObject;
                }
                //Spawn the item in worldspace and pass parameters to it.
                GameObject ingotworldspace = Instantiate(obj, position, Quaternion.identity);

                //drop item. 
                ItemStack stk;
                if (playerInventory != null)
                {
                    stk = playerInventory.RemoveFromPlayerInventory<Consumable_Ingot>(selectedButton.SelectedStack,
                    (selectedButton.SelectedStack.LastIndex - 1));
                }
                else
                {
                    stk = container.RemoveFromInventory<Consumable_Ore>(selectedButton.SelectedStack, (selectedButton.SelectedStack.LastIndex - 1));
                }
                if (ingotworldspace != null && stk != null)
                {
                    ingotworldspace.GetComponent<Consumable_Ingot>().RegenerateIngot(stk.GetItemArray().GetValue(0) as Consumable_Ingot);
                }
                selectedButton.Count -= stk.GetRealLength();
            }

        }
        //refresh the inventory UI
        RefreshInventoryScreen();
    }

    public void TransferSelected()
    {
        if (TransferMode)
        {
            //transfer to other inventory
            if (InventoryUIControllerExt != null)
            {
                if (playerInventory != null)
                {
                    InventoryUIControllerExt.TransferToContainer(selectedButton.SelectedStack);

                }
                else
                {
                    InventoryUIControllerExt.TransferToPlayer(selectedButton.SelectedStack);

                }
                selectedButton.Count -= selectedButton.SelectedStack.GetRealLength();
                InventoryUIControllerExt.UpdateDisplay();
            }
        }
    }

    public InventoryUIController InventoryUIControllerExt
    {
        get { return invUICont; }
        set { invUICont = value; }
    }

    /// <summary>
    /// Check buttons against inventory. Update, delete, and add as needed.
    /// </summary>
    public void RefreshInventoryScreen()
    {
        //get inventory
        List<ItemStack> inventory;
        if (playerInventory != null)
        {
            inventory = playerInventory.GetPlayerInventory();
        }
        else
        {
            inventory = container.GetInventory();
        }
        FleetBoyItemButton fbib;
        if (inventory.Count == 0)
        {
            for (int b = fbibs.Count - 1; b >= 0; b--)
            {
                Destroy(fbibs[b].gameObject);
                fbibs.RemoveAt(b);
            }
        }
        //check every fbib against the inventory
        for(int b = fbibs.Count-1; b >= 0; b--)
        {
            fbib = fbibs[b].GetComponent<FleetBoyItemButton>();
            if (fbib.Count <= 0f)
            {
                Destroy(fbibs[b].gameObject);
                fbibs.RemoveAt(b);
                //break;
            }
        }
    }

    private void ToggleButtonStateTo(Button button, bool state)
    {
        ColorBlock cb;
        if (state)
        {
            cb = button.colors;
            cb.normalColor = enabledColor;
            cb.highlightedColor = enabledColor;
            cb.selectedColor = enabledColor;
            button.colors = cb;
        }
        else
        {
            cb = button.colors;
            cb.normalColor = disabledColor;
            cb.highlightedColor = disabledColor;
            cb.selectedColor = disabledColor;
            button.colors = cb;
        }
    }

    public void ButtonInputHandler(int t)
    {
        if(t == 0)
        {
            if (showWeps)
            {
                SortOutType(Category.Weapon);
            }
            else
            {
                SortInType(Category.Weapon);
            }
        }
        else if (t == 1)
        {
            if (showTools)
            {
                SortOutType(Category.Tool);
            }
            else
            {
                SortInType(Category.Tool);
            }
        }
        else if(t == 2)
        {
            if (showGadgets)
            {
                SortOutType(Category.Gadget);
            }
            else
            {
                SortInType(Category.Gadget);
            }
        }
        else if (t == 3)
        {
            if (showGear)
            {
                SortOutType(Category.Gear);
            }
            else
            {
                SortInType(Category.Gear);
            }
        }
        else if (t == 4)
        {
            if (showCons)
            {
                SortOutType(Category.Consumable);
            }
            else
            {
                SortInType(Category.Consumable);
            }
        }
        else if (t == 5)
        {
            if (showAmmo)
            {
                SortOutType(Category.Ammo);
            }
            else
            {
                SortInType(Category.Ammo);
            }
        }
        else if (t == 6)
        {
            if (showRss)
            {
                SortOutType(Category.Resource);
            }
            else
            {
                SortInType(Category.Resource);
            }
        }
        else if (t == 7)
        {
            if (showMisc)
            {
                SortOutType(Category.Misc);
            }
            else
            {
                SortInType(Category.Misc);
            }
        }
    }

    public void SortOutType(Category sortType)
    {
        switch (sortType)
        {
            case Category.Ammo:
                showAmmo = false;
                break;
            case Category.Tool:
                showTools = false;
                break;
            case Category.Consumable:
                showCons = false;
                break;
            case Category.Gadget:
                showGadgets = false;
                break;
            case Category.Gear:
                showGear = false;
                break;
            case Category.Misc:
                showMisc = false;
                break;
            case Category.Resource:
                showRss = false;
                break;
            case Category.Weapon:
                showWeps = false;
                break;
        }

        
        FleetBoyItemButton fbib;
        for (int b = fbibs.Count - 1; b >= 0; b--)
        {
            fbib = fbibs[b].GetComponent<FleetBoyItemButton>();
            if (!CheckSortState(fbib.ItemCategory))
            {
                Destroy(fbib.gameObject);
                fbibs.RemoveAt(b);
            }
        }
        //RefreshInventoryScreen();
    }

    public void SortInType(Category sortType)
    {
        switch (sortType)
        {
            case Category.Ammo:
                showAmmo = true;
                break;
            case Category.Tool:
                showTools = true;
                break;
            case Category.Consumable:
                showCons = true;
                break;
            case Category.Gadget:
                showGadgets = true;
                break;
            case Category.Gear:
                showGear = true;
                break;
            case Category.Misc:
                showMisc = true;
                break;
            case Category.Resource:
                showRss = true;
                break;
            case Category.Weapon:
                showWeps = true;
                break;
        }
        FilterType();
    }

    private bool CheckSortState(Category category)
    {
        switch (category)
        {
            case Category.Ammo:
                if (showAmmo) return true;
                break;
            case Category.Consumable:
                if(showCons) return true;
                break;
            case Category.Tool:
                if (showTools) return true;
                break;
            case Category.Gadget:
                if (showGadgets) return true;
                break;
            case Category.Gear:
                if (showGear) return true;
                break;
            case Category.Misc:
                if (showMisc) return true;
                break;
            case Category.Resource:
                if (showRss) return true;
                break;
            case Category.Weapon:
                if (showWeps) return true;
                break;
        }
        return false;
    }

    public void SetContainer(CargoContainer cont)
    {
        container = cont;
    }

    /// <summary>
    /// Stop a type of item from showing in the inventory. Delete or add items accordingly
    /// </summary>
    public void FilterType()
    {
        /// Remove disabled buttons
        ///
        /*
        FleetBoyItemButton fbib;
        for (int b = fbibs.Count - 1; b >= 0; b--)
        {
            fbib = fbibs[b].GetComponent<FleetBoyItemButton>();
            if (!CheckSortState(selectedButton.ItemCategory))
            {
                Destroy(fbibs[b].gameObject);
                fbibs.RemoveAt(b);
                break;
            }
        }*/
        /// Add back item included
        ///
        PopulateInventoryScreen();
    }
}
