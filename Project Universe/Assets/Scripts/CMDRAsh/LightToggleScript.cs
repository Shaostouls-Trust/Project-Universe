using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightToggleScript : MonoBehaviour
{
    //[SerializeField]
    //private GameObject[] lightsToToggle;
    [SerializeField]
    private GameObject[] lightsToKeep;
    //[SerializeField]
    private Light[] lightsToDisable;
    [SerializeField]
    private GameObject parentShip;
    //public bool allowOnlyVisible;
    //public bool toggleOnlyListed;

    public void OnTriggerEnter()
    {
        lightsToDisable = parentShip.GetComponentsInChildren<Light>();
        Debug.Log("lights!");
        //turn off all lights
        foreach (Light thisLight in lightsToDisable)
        {
            //thisLight.GetComponent<Light>().enabled = false;
            thisLight.enabled = false;
        }
        //turn on all visible lights
        foreach (GameObject light in lightsToKeep)
        {
            light.GetComponentInChildren<Light>().enabled = true;
        }
    }

    //void Update()
    //{
    // if (allowOnlyVisible)
    // {
    //     toggleOnlyListed = false;

    // }
    // if (toggleOnlyListed)
    // {
    //     allowOnlyVisible = false;
    // }
    // }

}
