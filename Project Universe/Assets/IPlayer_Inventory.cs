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
    [SerializeField] private IEquipable p_eyes;//goggles, glasses, etc
    [SerializeField] private IEquipable p_ears;//radio, listening devices, etc
    [SerializeField] private IEquipable p_mouth;//oxygen gear, mic, etc. Usually mutually exclusive/included with helmet.
    [SerializeField] private IEquipable p_chest1;//main chestpiece
    [SerializeField] private IEquipable p_chest2;//chest coverpiece. Pouches, grendoliers, ammo belts, armor, handheld equipment, etc
    [SerializeField] private IEquipable p_waist;//more ammo, knives, grenades, medkits, handheld tools, etc
    [SerializeField] private IEquipable p_legsUpperL;//sidearm, ammo, pouches, etc.
    [SerializeField] private IEquipable p_legsUpperR;//same options as left leg
    [SerializeField] private IEquipable p_legsLower;//padding, armor, water pouches, etc.
    [SerializeField] private IEquipable p_feet;//boots, shoes, etc
    [Header("Inventory")]
    private List<ItemStack> p_inventory = new List<ItemStack>();
    public float inventoryWeight;

    void Start()
    {
        ItemStack devStack = new ItemStack("Ore_Iron", 1, 45);//we're saying one unit of iron is 1kg
        //p_inventory.Add(devStack);
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
    public bool AddStackToPlayerInventory(ItemStack item)
    {
        p_inventory.Add(item);
        return true;
    }

    public bool AddToPlayerInventory<itemtype>(GameObject item)
    {
        var itemScript = item.GetComponent<itemtype>() as Consumable_Ore;
        string type = itemScript.GetOreType();
        ItemStack wrapper = new ItemStack(type,itemScript.OreMassKg,99);//count should be mass or item count. Need to address this later
        p_inventory.Add(wrapper);
        return true;
    }
}
