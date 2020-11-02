using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WSButton3 : MonoBehaviour
{

    [SerializeField]
    private GameObject scriptedObj;
    [SerializeField]
    private int numID;
    //private int time=30;

    void Start()
    {
        this.GetComponent<MeshRenderer>().enabled = false;
        //renderer.enabled = false;
    }

    /// <summary>
    /// Universal linking function to be present in all button classes. A universal backend.
    /// This button specifically for powersystem control switches.
    /// </summary>
    public void externalInteractFunc()
    {
        scriptedObj.GetComponent<SwitchAnimationController>().OnSwitch(numID);
    }

    void OnMouseOver()
    {
        if (!this.GetComponent<MeshRenderer>().enabled)
        {
            this.GetComponent<MeshRenderer>().enabled = true;
        }
    }

    void OnMouseExit()
    {
        if (this.GetComponent<MeshRenderer>().enabled)
        {
            this.GetComponent<MeshRenderer>().enabled = false;
        }
    }
}
