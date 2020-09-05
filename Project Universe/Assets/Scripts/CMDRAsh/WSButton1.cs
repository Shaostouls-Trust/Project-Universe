using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WSButton1 : MonoBehaviour
{

    private MeshRenderer renderer;
    [SerializeField]
    private GameObject scriptedObj;
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

            case "Func0001_Generator":
                func0001_Generator();
                break;
        }
        //Debug.Log("Handshake");
        
    }

    //close and lock door, or unlock if locked.
    public void doorButtonOverride()
    {
        scriptedObj.GetComponentInChildren<DoorAnimator>().buttonResponse();
    }

    public void shutterButton()
    {
        //Debug.Log("nya nya");
        scriptedObj.GetComponentInChildren<ShutterAnimator>().buttonResponse();
    }

    public void func0001_Generator()
    {
        scriptedObj.GetComponentInChildren<PixelMap_Interpreter>().ButtonResponse();
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
