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
        private IPlayer_Inventory player_Inventory;
        private CargoContainer container;
        [SerializeField] private InventoryUI playerUI;
        [SerializeField] private InventoryUI contUI;
        [SerializeField] private GameObject screen;
        [SerializeField] private Button closeButton;
        [SerializeField] private TMP_Text inventoryName;
        [SerializeField] private TMP_Text contName;

        public void Start()
        {
            closeButton.onClick.AddListener(delegate { CloseUI(); });
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var networkedClient))
            {
                player_Inventory = networkedClient.PlayerObject.gameObject.GetComponent<IPlayer_Inventory>();
            }
        }

        public void CloseUI()
        {
            player_Inventory.transform.gameObject.GetComponent<SupplementalController>().UnlockCursor();
            this.gameObject.SetActive(false);
            //canTransfer = false;
        }

        public void OpenUI()
        {
            screen.SetActive(true);
        }

        public void SetContName(string name)
        {
            contName.text = name;
        }

        public void SetPlayerName(string name)
        {
            inventoryName.text = name;
        }

        public void SetCargoContainer(CargoContainer cargo)
        {
            container = cargo;
        }

        public void ReloadDisplay()
        {
            //update both inventoryuis
            playerUI.TransferMode = true;
            contUI.TransferMode = true;
            playerUI.InventoryUIControllerExt = this;
            contUI.InventoryUIControllerExt = this;
            playerUI.PopulateInventoryScreen();
            contUI.SetContainer(container);
            contUI.PopulateInventoryScreen();
        }

        public void UpdateDisplay()
        {
            //update both inventoryuis
            //playerUI.RefreshInventoryScreen();
            playerUI.PopulateInventoryScreen();
            //contUI.RefreshInventoryScreen();
            contUI.PopulateInventoryScreen();
            //Update the display on the side of the cargo container
            container.GetCargoUIController().UpdateDisplay(container.GetInventory());
        }

        public void TransferToPlayer(ItemStack stack)
        {
            List<ItemStack> inventory = container.GetInventory();
            //
            ItemStack rtn;
            //moving one deletes all others of same type (but leaves the empty object to screw everything else up)
            bool valid = container.RemoveFromInventory(stack, out rtn);
            if (valid)
            {
                Debug.Log("Transfer To Player");
                player_Inventory.AddStackToPlayerInventory(rtn);
            }
            //container.GetInventoryUI().DisplayContainerInventory();
        }
        public void TransferToContainer(ItemStack stack)
        {
            //List<ItemStack> inventory = player_Inventory.GetPlayerInventory();
            //
            ItemStack rtn = player_Inventory.RemoveFromPlayerInventory(stack);//, (stack.LastIndex-1)
            Debug.Log("Transfer " + rtn + " To Cont");
            bool valid = container.AddToInventory(rtn);
            //player_Inventory.GetPlayerInventoryUI().UpdateDisplay();
        }
        public void LockScreenAndFreeCursor()
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var networkedClient))
            {
                networkedClient.PlayerObject.gameObject.GetComponent<SupplementalController>().LockAndFreeCursor();
            }
        }
    }
}