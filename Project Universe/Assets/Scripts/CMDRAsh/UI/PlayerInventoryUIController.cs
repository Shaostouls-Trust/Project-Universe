using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class PlayerInventoryUIController : MonoBehaviour
{
    public float inventoryWeight;
    public float inventoryVolume;
    [SerializeField] private GameObject inventoryItems;
    [SerializeField] private IPlayer_Inventory player_Inventory;
    [SerializeField] private Button closeButton;
    private bool on;
    public GameObject ToolTipUI;
    //private bool canTransfer = false;

    private void Start()
    {
        closeButton.onClick.AddListener(delegate { CloseUI(); });
    }

    public void CloseUI()
    {
        this.gameObject.SetActive(false);
        on = false;
        player_Inventory.transform.gameObject.GetComponent<ProjectUniverseData.PlayerController.PlayerController>().UnlockCursor();
        //canTransfer = false;
    }

    public void DisplayOff()
    {
        on = false;
        this.transform.gameObject.SetActive(false);
    }
    public void ToggleDisplay()
    {
        if (on)
        {
            on = false;
            player_Inventory.transform.gameObject.GetComponent<ProjectUniverseData.PlayerController.PlayerController>().UnlockCursor();
        }
        else 
        { 
            on = true;
            LockScreenAndFreeCursor();
            UpdateDisplay();
        }
        this.transform.gameObject.SetActive(on);
    }
    public void LockScreenAndFreeCursor()
    {
        player_Inventory.transform.gameObject.GetComponent<ProjectUniverseData.PlayerController.PlayerController>().LockAndFreeCursor();
    }

    public void UpdateDisplay()
    {
        List<ItemStack> inventory = player_Inventory.GetPlayerInventory();
        for(int i = 0; i < inventory.Count; i++)
        {
            //if(i >= 30)
            //{
                //the inventory can only display 30 items. Player inventory not likely to hold even that many
            //    break;
            //}
            IInventoryIcon icon = inventoryItems.transform.GetChild(i).GetComponent<IInventoryIcon>();
            icon.SetRef_PlayerUIController(this);
            icon.transform.gameObject.SetActive(true);
            icon.UpdateCount(inventory[i].Size());
            icon.SetInventoryType(0);
            icon.SetIndex(i);
            //find Icon if has one
            Sprite nIcon;
            string path="";
            try
            {
                path = "UI/Sprites/Inventory/" + "Icon_" + inventory[i].GetStackType();
                nIcon = Resources.Load<Sprite>(path);
            }
            catch (Exception) { nIcon = null; }
            //Debug.Log(path);
            icon.UpdateIcon(nIcon);
        }
    }
    public void UseItem(int idx)
    {

    }

    public void DisplayItemTooltip(int index)
    {
        ToolTipUI.gameObject.SetActive(true);
        ToolTipUI.gameObject.transform.position = Input.mousePosition;
        ItemStack stack = player_Inventory.GetPlayerInventory()[index];
        ToolTipUI.GetComponentInChildren<TMP_Text>().text = stack.ToString();
    }
    public void HideItemTooltip()
    {
        ToolTipUI.gameObject.SetActive(false);
    }
}
