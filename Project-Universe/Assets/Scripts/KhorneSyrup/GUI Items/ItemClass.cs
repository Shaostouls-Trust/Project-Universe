using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemClass : MonoBehaviour
{
    public string itemName, itemDescription;
    public RawImage icon;
    [SerializeField]private GameObject Panel;
    public enum itemType
    {
        Helmet,
        Chestplate,
        arms,
        legs,
        backpack,
        block,
        test
    }
    [SerializeField] private string trueItemName, trueItemDesc;

    // Start is called before the first frame update
    void Start()
    {
        itemName = trueItemName;
        itemDescription = trueItemDesc;
        GameObject gIcon = Instantiate(new GameObject(), Panel.transform);
        gIcon.AddComponent<RawImage>();
        icon = gIcon.GetComponent<RawImage>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
