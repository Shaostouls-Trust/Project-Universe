using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using ProjectUniverse.Base;
using ProjectUniverse.Items.Containers;

namespace ProjectUniverse.UI
{
    public class CargoUIController : MonoBehaviour
    {
        [SerializeField] private CargoContainer container;
        [SerializeField] private GameObject itembuttonpref;
        [SerializeField] private GameObject buttonParent;
        private List<GameObject> buttons = new List<GameObject>();

        public void UpdateDisplay(List<ItemStack> cargo)
        {

            for (int i = buttonParent.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(buttonParent.transform.GetChild(i).gameObject);
            }
            for (int i = 0;i<=cargo.Count-1;i++)
            {
                if(i > 9)
                {
                    break;
                }
                if(cargo[i] != null)
                {
                    GameObject instanceButton = Instantiate(itembuttonpref, buttonParent.transform);
                    FleetBoyItemButton fbb = instanceButton.GetComponent<FleetBoyItemButton>();
                    buttons.Add(instanceButton);
                    //Debug.Log(cargo[i]);
                    string[] name = cargo[i].GetStackType().Split('_');
                    fbb.ItemName = name[1] + " " + name[0];
                    fbb.Count = cargo[i].GetRealLength();
                    instanceButton.GetComponent<Button>().interactable = false;
                }
            }
        }

    }
}