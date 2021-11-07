using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectUniverse.Player.PlayerController;
using ProjectUniverse.Base;
using ProjectUniverse.Player;
using ProjectUniverse.Items.Containers;
using MLAPI;

namespace ProjectUniverse.UI
{
    public class InventoryUIController : MonoBehaviour
    {
        //public float inventoryWeight;
        //public float inventoryVolume;
        //[SerializeField] private InventorySelectAndTransfer IST;
        [SerializeField] private GameObject cargoInventoryItems;
        [SerializeField] private GameObject playerInventoryItems;
        [SerializeField] private IPlayer_Inventory player_Inventory;
        [SerializeField] private CargoContainer container;
        [SerializeField] private Button closeButton;
        [SerializeField] private TMP_Text inventoryName;
        [SerializeField] private TMP_Text contMass;
        [SerializeField] private TMP_Text playerMass;
        public GameObject ToolTipUI;

        private bool on;
        //private bool canTransfer = false;

        void Start()
        {
            closeButton.onClick.AddListener(delegate { CloseUI(); });
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var networkedClient))
            {
                player_Inventory = networkedClient.PlayerObject.gameObject.GetComponent<IPlayer_Inventory>();
            }
        }

        public void CloseUI()
        {
            this.gameObject.SetActive(false);
            on = false;
            player_Inventory.transform.gameObject.GetComponent<PlayerController>().UnlockCursor();
            //canTransfer = false;
        }

        public void SetContName(string name)
        {
            inventoryName.text = name;
        }

        public void ResetDisplay()
        {
            //Debug.Log("RESET");
            foreach (IInventoryIcon icon in cargoInventoryItems.transform.GetComponentsInChildren<IInventoryIcon>())
            {
                icon.transform.gameObject.SetActive(false);
            }
            foreach (IInventoryIcon icon in playerInventoryItems.transform.GetComponentsInChildren<IInventoryIcon>())
            {
                icon.transform.gameObject.SetActive(false);
            }
        }

        public void SetCargoContainer(CargoContainer cargo)
        {
            container = cargo;
        }

        public void UpdateDisplay()
        {
            if (!on)
            {
                on = true;
                LockScreenAndFreeCursor();
                this.gameObject.SetActive(true);
            }
            ResetDisplay();
            //player inv
            List<ItemStack> playerInventory = player_Inventory.GetPlayerInventory();
            for (int i = 0; i < playerInventory.Count; i++)
            {
                //if (i >= 30)
                //{
                //the inventory can only display 50 items. Player inventory not likely to hold even that many
                //    break;
                //}
                IInventoryIcon icon = playerInventoryItems.transform.GetChild(i).GetComponent<IInventoryIcon>();
                //if (icon != null) { Debug.Log("GOOD"); }
                //if (!on){
                icon.transform.gameObject.SetActive(true);
                icon.SetInventoryType(1);
                icon.SetRef_InventoryUIController(this);
                //}
                icon.UpdateCount(playerInventory[i].Size());
                icon.SetIndex(i);
                Sprite nIcon;
                string path = "";
                //try
                //{
                path = "UI/Sprites/Inventory/" + "Icon_" + playerInventory[i].GetStackType();
                nIcon = Resources.Load<Sprite>(path);
                //}
                //catch (Exception) { nIcon = null; }
                icon.UpdateIcon(nIcon);
            }
            on = true;
            //container
            List<ItemStack> contInventory = container.GetInventory();
            for (int i = 0; i < contInventory.Count; i++)
            {
                //if (i >= 30)
                //{
                //the inventory can only display 30 items. Player inventory not likely to hold even that many
                //    break;
                //}
                IInventoryIcon icon = cargoInventoryItems.transform.GetChild(i).GetComponent<IInventoryIcon>();
                //if (!on)
                //{

                icon.transform.gameObject.SetActive(true);
                icon.SetInventoryType(2);
                icon.SetRef_InventoryUIController(this);
                //}
                icon.UpdateCount(contInventory[i].Size());
                icon.SetIndex(i);
                //find Icon if has one
                Sprite nIcon;
                string path = "";
                //try
                // {
                path = "UI/Sprites/Inventory/" + "Icon_" + contInventory[i].GetStackType();
                nIcon = Resources.Load<Sprite>(path);
                // }
                // catch (Exception) { nIcon = null; }
                icon.UpdateIcon(nIcon);
            }
            //on = true;
            //Update the display on the side of the cargo container
            container.GetCargoUIController().UpdateDisplay(container.GetInventory());
        }

        public void TransferToPlayer(int idx)
        {
            List<ItemStack> inventory = container.GetInventory();
            //
            ItemStack rtn;
            //moving one deletes all others of same type (but leaves the empty object to screw everything else up)
            Debug.Log("DONT CALL ME TWICE");
            bool valid = container.RemoveFromInventory(inventory[idx], out rtn);
            if (valid)
            {
                Debug.Log("Transfer To Player");
                player_Inventory.AddStackToPlayerInventory(rtn);
            }
            UpdateDisplay();
            //container.GetInventoryUI().DisplayContainerInventory();
        }
        public void TransferToContainer(int idx)
        {
            //List<ItemStack> inventory = player_Inventory.GetPlayerInventory();
            //
            ItemStack rtn = player_Inventory.RemoveFromPlayerInventory(idx);
            Debug.Log("Transfer " + rtn + " To Cont");
            //Debug.Log("REMAINING IN PLAYER INVENTORY");
            //foreach (ItemStack stack in player_Inventory.GetPlayerInventory())
            //{
            //    Debug.Log(stack);
            //}
            //Debug.Log("------------------");
            bool valid = container.AddToInventory(rtn);
            //if (valid)
            //{


            //}
            UpdateDisplay();
            //player_Inventory.GetPlayerInventoryUI().UpdateDisplay();
        }
        public void LockScreenAndFreeCursor()
        {
            player_Inventory.transform.gameObject.GetComponent<PlayerController>().LockAndFreeCursor();
        }

        public void UseItem(string itemName)
        {

        }

        public void DisplayItemTooltip(int index, int inventoryType)
        {
            ToolTipUI.gameObject.SetActive(true);
            ToolTipUI.gameObject.transform.position = Input.mousePosition + new Vector3(1, 1, 1);
            ItemStack stack;
            if (inventoryType == 1)
            {
                stack = player_Inventory.GetPlayerInventory()[index];
            }
            else
            {
                stack = container.GetInventory()[index];
            }
            ToolTipUI.GetComponentInChildren<TMP_Text>().text = stack.ToString();
        }
        public void HideItemTooltip()
        {
            ToolTipUI.gameObject.SetActive(false);
        }
    }
}