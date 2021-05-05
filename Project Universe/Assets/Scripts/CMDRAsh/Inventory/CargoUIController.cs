using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class CargoUIController : MonoBehaviour
{
    [SerializeField] private CargoContainer container;
    public int maxItemsPerCol;
    //public Canvas cargoui;
    //3 panels of 8 slots each
    public GridLayoutGroup grid;
    public GameObject itemlabelprefab;
    public TMP_Text contName;


    // Start is called before the first frame update
    void Start()
    {
        contName.text = container.transform.gameObject.name;
        UpdateDisplay(container.GetInventory());
    }

    public void ChangeContainerName(string name)
    {
        contName.text = name;
    }
    public string GetContainerName()
    {
        return contName.text;
    }

    public void UpdateDisplay(List<ItemStack> cargo)
    {
        for(int i = 0; i < grid.transform.childCount; i++)
        {
            GameObject.Destroy(grid.transform.GetChild(i).transform.gameObject);
        }
        foreach(ItemStack stack in cargo)
        {
            GameObject instanceLabel = (GameObject)Instantiate(itemlabelprefab);
            instanceLabel.transform.SetParent(grid.transform);
            instanceLabel.transform.localScale = new Vector3(1, 1, 1);
            instanceLabel.transform.localPosition = new Vector3(0, 0, 0);
            instanceLabel.transform.localRotation = Quaternion.Euler(0, 0, 0);
            
            //change icon and text
            instanceLabel.transform.GetChild(0).GetComponent<TMP_Text>().text = stack.GetStackType();
            //instanceLabel.transform.GetChild(1).GetComponent<Image>().sprite = stack.GetSprite2D(); NYI
            instanceLabel.transform.GetChild(2).GetComponent<TMP_Text>().text = "x"+stack.Size();
            //Debug.Log("Creating ui item: "+stack.ToString());
            try
            {
                string path = "UI/Sprites/Inventory/" + "Icon_" + stack.GetStackType();
                Sprite nIcon = Resources.Load<Sprite>(path);
                instanceLabel.transform.GetChild(1).GetComponent<Image>().sprite = nIcon;
            }
            catch (Exception) {}
        }
    }
    
}
