using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WSButton1 : MonoBehaviour
{

    //private MeshRenderer renderer;
    [SerializeField]
    private GameObject scriptedObj;
    [SerializeField]
    private string type;
    public GameObject player;

    void Start()
    {
        this.GetComponent<MeshRenderer>().enabled = false;
        //renderer.enabled = false;
    }

    /// <summary>
    /// Universal linking function to be present in all button classes. A universal backend.
    /// </summary>
    public void externalInteractFunc()
    {
        switch (type)
        {
            case "shutter":
                shutterButton();
                //Debug.Log("shutter");
                break;

            case "door": 
                doorButtonOverride(); 
                //Debug.Log("door"); 
                break;

            case "Func0001_Generator":
                func0001_Generator();
                break;

            case "Func0002_Generator":
                func0002_Generator();
                break;

            case "ICannonInteract":
                IMyCannonInteract(player);
                break;

            case "CrawlDoor":
                crawlDoorOpen();
                Debug.Log("crawlDoor");
                break;
        }
    }

    //close and lock door, or unlock if locked.
    public void doorButtonOverride()
    {
        scriptedObj.GetComponentInChildren<DoorAnimator>().ButtonResponse();
    }

    public void shutterButton()
    {
        scriptedObj.GetComponentInChildren<ShutterAnimator>().buttonResponse();
    }

    public void func0001_Generator()
    {
        scriptedObj.GetComponentInChildren<PixelMap_Interpreter>().ButtonResponse();
    }

    public void func0002_Generator()
    {
        scriptedObj.GetComponentInChildren<PixelMap_ECSInterpreter>().ButtonResponse();
    }

    public void IMyCannonInteract(GameObject player)
    {
        scriptedObj.GetComponent<IControllableWeapon>().ButtonResponse(player);
    }

    public void crawlDoorOpen()
    {
        scriptedObj.GetComponent<CrawlDoorAnimator>().ButtonResponse();
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
