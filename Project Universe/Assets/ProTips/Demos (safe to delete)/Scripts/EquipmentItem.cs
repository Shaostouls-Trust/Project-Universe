using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace ModelShark
{
    /// <summary>
    /// This script is an example of how you could implement an inventory system and have dynamic tooltips that show item stats, and
    /// comparisons equipped items.
    /// </summary>
    /// <remarks>
    /// Put this script on an inventory item prefab. Then you can manipulate the item's attributes and call ResetTooltip() to 
    /// refresh the tooltip values.
    /// </remarks>
    public class EquipmentItem : MonoBehaviour
    {
        public bool isEquipped;
        public ItemType itemType;
        public string itemName;
        public int damage;
        public int rateOfFire;
        public int armorRating;
        public int movePenalty;
        public int concealmentPenalty;
        public int value;
        public Rarity rarity;

        public TooltipTrigger TooltipTrigger { get; set; }
        public Image Image { get; set; }

        // Use this for initialization
        private void Start()
        {
            // Grab a reference to the TooltipTrigger (and Equipped Item section) and the Image component on this game object.
            TooltipTrigger = gameObject.GetComponent<TooltipTrigger>();
            Image = gameObject.GetComponent<Image>();

            ResetTooltip();
        }

        /// <summary>Resets the tooltip for this equipment item. Call this function anytime the stats of the item change.</summary>
        public void ResetTooltip()
        {
            // If this item is equipped, turn off the Equipped Item section of the tooltip.
            if (isEquipped)
                TooltipTrigger.TurnSectionOff("EquippedItem");
            else
            {
                TooltipTrigger.TurnSectionOn("EquippedItem");
                // Get a list of all the currently equipped items on the character. In reality, you would probably keep this list elsewhere,
                // but we're just querying the items here to keep it simple.
                EquipmentItem[] equipmentItems = FindObjectsOfType<EquipmentItem>();

                for (int i=0; i < equipmentItems.Length; i++)
                {
                    if (equipmentItems[i].isEquipped && equipmentItems[i].itemType == itemType)
                    {
                        TooltipTrigger.SetText("EquippedName", equipmentItems[i].itemName);
                        Image equippedImage = equipmentItems[i].GetComponent<Image>();
                        TooltipTrigger.SetImage("EquippedItemImage", equippedImage.sprite);
                        TooltipTrigger.SetText("EquippedStats", GetStatsText(equipmentItems[i]));
                        TooltipTrigger.SetText("EquippedValue", String.Format("<color=#888888>VALUE: </color> <size=14>{0}</size>", equipmentItems[i].value.ToString("##,000")));
                        break;
                    }
                }
            }

            // Set the Rarity colors
            string rarityColor = "bbbbbb"; // Default "Common" gray color.
            switch (rarity)
            {
                case Rarity.Rare:
                    rarityColor = "FFC924"; // Amber color for "Rare".
                    break;
                case Rarity.Uncommon:
                    rarityColor = "25ff00"; // Green color for "Uncommon".
                    break;
            }

            TooltipTrigger.SetText("TitleText", String.Format("{0} ({1})", itemName, itemType));
            TooltipTrigger.SetText("Stats", GetStatsText(this));
            TooltipTrigger.SetText("Description", "<b>NOTE:</b> This tooltip is dynamically populated at runtime with values from the item!");
            TooltipTrigger.SetText("Value", String.Format("<color=#888888>VALUE: </color> <size=16>{0}</size>", value.ToString("##,000")));
            TooltipTrigger.SetText("Rarity", String.Format("<color=#{0}>{1} {2}</color>", rarityColor, rarity, itemType));
            TooltipTrigger.SetImage("ItemImage", Image.sprite);
        }

        /// <summary>Generates formatted Stats text for an item, based on its Item Type.</summary>
        private static string GetStatsText(EquipmentItem item)
        {
            StringBuilder stats = new StringBuilder();

            switch (item.itemType)
            {
                case ItemType.Armor:
                    stats.AppendFormat("<color=#888888>ARMOR RATING: </color> <color=#00ff00>{0}{1}</color>\n", item.armorRating < 0 ? "-" : "+", Mathf.Abs(item.armorRating));
                    stats.AppendFormat("<color=#888888>MOVE PENALTY: </color> <color=#ff0000>{0}{1}</color>\n", "-", Mathf.Abs(item.movePenalty));
                    stats.AppendFormat("<color=#888888>CONCEALMENT:  </color> <color=#ff0000>{0}{1}</color>\n", "-", Mathf.Abs(item.concealmentPenalty));
                    break;
                case ItemType.Weapon:
                    stats.AppendFormat("<color=#888888>DAMAGE:       </color> <color=#00ff00>{0}</color> DPS\n", item.damage);
                    stats.AppendFormat("<color=#888888>FIRE RATE:    </color> <color=#00ff00>{0}</color> per sec\n", item.rateOfFire);
                    stats.AppendFormat("<color=#888888>CONCEALMENT:  </color> <color=#ff0000>{0}{1}</color>\n", "-", Mathf.Abs(item.concealmentPenalty));
                    break;
            }
            return stats.ToString();
        }
    }

    public enum ItemType
    {
        Weapon,
        Armor
    }

    public enum Rarity
    {
        Common,
        Uncommon,
        Rare
    }
}