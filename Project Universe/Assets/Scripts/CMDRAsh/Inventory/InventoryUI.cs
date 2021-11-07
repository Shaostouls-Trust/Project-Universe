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

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject itemparent;
    [SerializeField] private GameObject itembuttonpref;
    [SerializeField] private GameObject container;
    [SerializeField] private List<GameObject> fbibs = new List<GameObject>();
    [SerializeField] private TMP_Text description;
    [SerializeField] private TMP_Text statistics;
    [SerializeField] private IPlayer_Inventory inventory;
    [SerializeField] private Button wepButton;
    [SerializeField] private Button gadButton;
    [SerializeField] private Button gearButton;
    [SerializeField] private Button consButton;
    [SerializeField] private Button ammoButton;
    [SerializeField] private Button rssButton;
    [SerializeField] private Button miscButton;
    [SerializeField] private Color32 enabledColor;
    [SerializeField] private Color32 disabledColor;
    private bool showWeps = true;
    private bool showGadgets = true;
    private bool showGear = true;
    private bool showCons = true;
    private bool showAmmo = true;
    private bool showRss = true;
    private bool showMisc = true;

    private FleetBoyItemButton selectedButton;

    public FleetBoyItemButton SelectedButton
    {
        get { return selectedButton; }
        set { selectedButton = value; }
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
        List<ItemStack> inventory = container.GetComponent<IPlayer_Inventory>().GetPlayerInventory();
        //sort inventory
        Debug.Log("Sorting Inventory");
        MethodInfo info = inventory[0].GetType().GetMethod("GetStackCategory");
        Utils.InsertionSort<int>(ref inventory, info,true);
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
                fbb.ItemName = name[1] + " " + name[0];
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
                //description.text = desc;
                //statistics.text = stats;
                fbb.ItemCategory = cat;
                fbb.Description = desc;
                fbb.Stats = stats;
                fbb.DescTMP = description;
                fbb.StatTxt = statistics;
                fbb.SelectedStack = inventory[t];
                fbb.UI = this;
            }
        }
        //Utils.CategorySort(ref fbibs);
    }

    /// Bind methods to buttons
    /// 
    void Start()
    {
        wepButton.onClick.AddListener(delegate { ButtonInputHandler(0); ToggleButtonStateTo(wepButton, showWeps); });
        gadButton.onClick.AddListener(delegate { ButtonInputHandler(1); ToggleButtonStateTo(gadButton, showGadgets); });
        gearButton.onClick.AddListener(delegate { ButtonInputHandler(2); ToggleButtonStateTo(gearButton, showGear); });
        consButton.onClick.AddListener(delegate { ButtonInputHandler(3); ToggleButtonStateTo(consButton, showCons); });
        ammoButton.onClick.AddListener(delegate { ButtonInputHandler(4); ToggleButtonStateTo(ammoButton, showAmmo); });
        rssButton.onClick.AddListener(delegate { ButtonInputHandler(5); ToggleButtonStateTo(rssButton, showRss); });
        miscButton.onClick.AddListener(delegate { ButtonInputHandler(6); ToggleButtonStateTo(miscButton, showMisc); });
    }

    ///Handle keyboard inputs
    ///
    void Update()
    {
        //if selected itemstack
        if(selectedButton != null)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                //use item. Equip to the right body slot, or use immediately
                if (selectedButton.ItemCategory == Category.Gear)
                {

                }
                else if (selectedButton.ItemCategory == Category.Weapon)
                {

                }
                else if (selectedButton.ItemCategory == Category.Gadget)
                {

                }
                else if (selectedButton.ItemCategory == Category.Consumable)
                {
                    //if medical or food, use immediately. Otherwise equip to hands. LMB or RMB equip to different hands.
                }
            }
            else if (Input.GetKeyDown(KeyCode.J))
            {
                Debug.Log("Drop Selected");
                //find item prefab
                Vector3 position;
                string prefix = "Prefabs\\Resources\\";
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

                //remove the item from inventory. Dropping an item will work based on itemstack index, not mass or itemcount.
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
                    ItemStack stk = inventory.RemoveFromPlayerInventory<Consumable_Ore>(selectedButton.SelectedStack,
                        (selectedButton.SelectedStack.LastIndex - 1));
                    Debug.Log(stk);
                    if(oreworldspace != null && stk != null)
                    {
                        oreworldspace.GetComponent<Consumable_Ore>().RegenerateOre(stk.GetItemArray().GetValue(0) as Consumable_Ore);
                    }
                    selectedButton.Count -= stk.GetRealLength();
                }
                else if(selectedButton.SelectedStack.GetOriginalType() == typeof(Consumable_Ingot))
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
                    ItemStack stk = inventory.RemoveFromPlayerInventory<Consumable_Ingot>(selectedButton.SelectedStack,
                        (selectedButton.SelectedStack.LastIndex - 1));
                    if (ingotworldspace != null && stk != null)
                    {
                        ingotworldspace.GetComponent<Consumable_Ingot>().RegenerateIngot(stk.GetItemArray().GetValue(0) as Consumable_Ingot);
                    }
                    selectedButton.Count -= stk.GetRealLength();
                }
                //refresh the inventory UI
                RefreshInventoryScreen();
            }
        }
        
    }

    /// <summary>
    /// Check buttons against inventory. Update, delete, and add as needed.
    /// </summary>
    public void RefreshInventoryScreen()
    {
        //get inventory
        List<ItemStack> inventory = container.GetComponent<IPlayer_Inventory>().GetPlayerInventory();
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
            //cb.highlightedColor = disabledColor;
            button.colors = cb;
        }
        else
        {
            cb = button.colors;
            cb.normalColor = disabledColor;
            //cb.highlightedColor = enabledColor;
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
        else if(t == 1)
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
        else if (t == 2)
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
        else if (t == 3)
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
        else if (t == 4)
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
        else if (t == 5)
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
        else if (t == 6)
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
