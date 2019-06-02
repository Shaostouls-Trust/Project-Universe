using UnityEngine;

[CreateAssetMenu()]
public class Item : ScriptableObject
{
	#region Variables

	// Item types, for use in containers
	public enum ItemTypes {
		CROP,
		WEAPON,
		CONSUMABLE,
		AMMO,
		TOOL,
		DEV
	}

	// This items item type;
	public ItemTypes itemType;

	// The name of the item, for GUI purposes
	public string itemName;

	// The maximum amount of this item in 1 stack
	public int itemMaxStackSize;

	// The icon of this item.
	public Texture2D itemIcon;

	// The ingame model of this item to be rendered in the world
	public GameObject itemModel;

	// The current instance of this object, only used if this item is in the world
	public GameObject currentItemObject;
	#endregion
	
	#region Functions

	public Item(string name, int maxStackSize, ItemTypes type, Texture2D icon, GameObject model) {
		itemName = name;
		itemMaxStackSize = maxStackSize;
		itemType = type;
		itemIcon = icon;
		itemModel = model;
	}

	#endregion
}
