using UnityEngine;

public class Container : MonoBehaviour
{
	#region Variables
	public Container container;
	public string containerName;
	public int inventoryLength;
	public Item[] items;
	public Item.ItemTypes allowedItemTypes;
	#endregion
	
	#region Functions

	public Container(string name, int invLength, Item.ItemTypes allowedItemTypes) {
		items = new Item[invLength];
		containerName = name;
		this.allowedItemTypes = allowedItemTypes;
	}

    void Start() {
		container = this;

    }

    void Update() {
        
    }

	public void AddItem(Item item) {
		if(items.Length != inventoryLength) {

		} else {
			
		}
	}

	#endregion
}
