using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WSButton2 : MonoBehaviour
{

    private MeshRenderer renderer;
    [SerializeField]
    private GameObject[] scriptedObjs;
    [SerializeField]
    private string type;

    // Start is called before the first frame update
    void Start()
    {
        renderer = this.GetComponent<MeshRenderer>();
        renderer.enabled = false;
    }

    // Update is called once per frame
    //void Update()
    //{
    //   
    //}

    /// <summary>
    /// Universal linking function to be present in all button classes. A universal backend.
    /// </summary>
    public void externalInteractFunc()
    {
        switch (type)
        {
            case "shutter":
                shutterButton();
                Debug.Log("shutter");
                break;

            case "door": 
                doorButtonOverride(); 
                Debug.Log("door"); 
                break;
            
        }
    }

    //close and lock door, or unlock if locked.
    public void doorButtonOverride()
    {
        foreach(GameObject obj in scriptedObjs){
            obj.GetComponentInChildren<DoorAnimator>().buttonResponse();
        }
    }

    public void shutterButton()
    {
        foreach (GameObject obj in scriptedObjs)
        {
            obj.GetComponentInChildren<ShutterAnimator>().buttonResponse();
        }
    }

    void OnMouseOver()
    {
        if (!renderer.enabled)
        {
            //Debug.Log("enabled?");
            renderer.enabled = true;
        }
    }

    void OnMouseExit()
    {
        if (renderer.enabled)
        {
            //Debug.Log("disabled?");
            renderer.enabled = false;
        }
    }
}
