using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using ProjectUniverse.Base;
using ProjectUniverse.Production.Resources;
using ProjectUniverse.Player;
using static ProjectUniverse.Base.ItemStack;

/// <summary>
/// Change text color when the button is moused over and selected.
/// Pass activation parameters to use, equip, drop, etc this item.
/// </summary>
public class FleetBoyItemButton : MonoBehaviour, IDeselectHandler
{
    [SerializeField] private Category thisCat;
    [SerializeField] private float count;
    [SerializeField] private string itemname;
    [SerializeField] private TMP_Text countTxt;
    [SerializeField] private TMP_Text nameTxt;
    [SerializeField] private TMP_Text descTxt;
    [SerializeField] private TMP_Text statTxt;
    [SerializeField] private InventoryUI ui;
    [SerializeField] private ItemStack stack;

    //all inventory items have actions that can be performed while item is selected
    //use (E)
    //drop (J)
    //transfer (T) if first accessed a container

    // Start is called before the first frame update
    void Start()
    {
        countTxt.text = ""+count;
        nameTxt.text = itemname;
    }

    public float Count
    {
        get { return count; }
        set { count = value;
            countTxt.text = "" + count;
        }
    }
    public string ItemName
    {
        get { return itemname; }
        set { itemname = value; }
    }
    public Category ItemCategory
    {
        get { return thisCat; }
        set { thisCat = value; }
    }

    public InventoryUI UI
    {
        get { return ui; }
        set { ui = value; }
    }

    public string Description { get; set; }
    public string Stats { get; set; }

    public TMP_Text DescTMP
    {
        get { return descTxt; }
        set { descTxt = value; }
    }
    public TMP_Text StatTxt
    {
        get { return statTxt; }
        set { statTxt = value; }
    }

    public ItemStack SelectedStack
    {
        get { return stack; }
        set { stack = value; }
    }

    public void OnSelectButton()
    {
        DescTMP.text = Description;
        StatTxt.text = Stats;
        UI.SelectedButton = this;
        Debug.Log("UI selected button: "+this);
    }

    /// <summary>
    /// Release item from interaction control.
    /// </summary>
    /// <param name="eventData"></param>
    public void OnDeselect(BaseEventData eventData)
    {
        DescTMP.text = "";
        StatTxt.text = "";
        UI.SelectedButton = null;
        Debug.Log("UI selected button: null");
    }
}
