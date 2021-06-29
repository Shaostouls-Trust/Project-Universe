using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace ProjectUniverse.UI
{
    public class IInventoryIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        //private string itemName;
        private int index;
        private int inventoryType = 0;
        public Button DropItem;
        public Button UseItem;
        public TMP_Text ItemCount;
        public Image ItemIcon;
        private InventoryUIController contInvUI;
        private PlayerInventoryUIController playerUI;

        void Start()
        {
            DropItem.onClick.AddListener(delegate { DropItemEvent(); });
            UseItem.onClick.AddListener(delegate { UseItemEvent(); });
            //contInvUI = this.GetComponentInParent<InventoryUIController>();
        }

        public void SetRef_InventoryUIController(InventoryUIController uiController)
        {
            contInvUI = uiController;
        }
        public void SetRef_PlayerUIController(PlayerInventoryUIController pUIcontroller)
        {
            playerUI = pUIcontroller;
        }

        public void OnPointerEnter(PointerEventData data)
        {
            //display item's tooltip
            //if (Input.GetKey(KeyCode.LeftShift))
            //{
            if (inventoryType == 0)//player only
            {
                playerUI.DisplayItemTooltip(index);
            }
            else//player/cont
            {
                contInvUI.DisplayItemTooltip(index, inventoryType);
            }
            //}
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (inventoryType == 0)//player only
            {
                playerUI.HideItemTooltip();
            }
            else//player/cont
            {
                contInvUI.HideItemTooltip();
            }
        }

        public void UpdateCount(float newcount)
        {
            ItemCount.text = "" + newcount;
        }
        public void UpdateIcon(Sprite icon)
        {
            ItemIcon.sprite = icon;
        }
        public void DropItemEvent()
        {
            //spawn item in world and remove from inventory
        }
        public void UseItemEvent()
        {
            //Debug.Log("Icon: "+this.ToString());
            ///
            /// New approach. This one is broken and doesn't work.
            /// 
            //InventoryUIController contInvUI = this.GetComponentInParent<InventoryUIController>();
            //if shift_down: transfer item
            if (Input.GetKey(KeyCode.LeftShift))
            {
                if (contInvUI != null)
                {
                    switch (inventoryType)
                    {
                        case 0://icon in other inventory
                            break;
                        case 1://icon in player inv
                            Debug.Log("Trans to cont");
                            contInvUI.TransferToContainer(index);
                            break;
                        case 2://icon in cont inv
                            Debug.Log("Trans to player");
                            contInvUI.TransferToPlayer(index);
                            break;
                    }
                    //Debug.Log("cont move");
                    //contInvUI.TransferItem(index);
                }
            }

            //otherwise: try to call item's use func
        }
        public void SetInventoryType(int type)
        {
            inventoryType = type;
        }
        public void SetIndex(int ind)
        {
            index = ind;
        }
        public int GetIndex()
        {
            return index;
        }
    }
}