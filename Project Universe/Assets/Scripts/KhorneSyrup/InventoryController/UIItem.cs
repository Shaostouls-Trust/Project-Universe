using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UIItem : MonoBehaviour
{
    public ItemClass itemInfo;
    public GameObject itemObject;
    public RawImage IconTexture;
    public TextMeshProUGUI nameTextField;
    public TextMeshProUGUI quantityTextField;
    public InventoryManager inventory;
    [SerializeField] private BoxCollider2D collisionCheck;
    public BoxCollider2D panelCollider;
    public Camera cam;
    public bool isVisible;
    // Start is called before the first frame update
    void Start()
    {
        collisionCheck = gameObject.GetComponent<BoxCollider2D>();
        nameTextField.text = itemInfo.ItemName;
    }

    private void Update()
    {
        cam.gameObject.SetActive(isVisible);
    }

    private void RemoveItemFromInventory()
    {
        StartCoroutine(inventory.ERemoveItemFromInventory(itemInfo.gameObject, itemInfo));
    }

    void OnTriggerEnter2D(Collider2D check)
    {
        if (check == panelCollider)
        {
            isVisible = true;
        }
    }
    private void OnTriggerExit2D(Collider2D check)
    {
        isVisible = false;
    }
}
