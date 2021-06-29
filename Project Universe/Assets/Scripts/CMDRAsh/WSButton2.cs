using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectUniverse.Generation;
using ProjectUniverse.Animation.Controllers;

public class WSButton2 : MonoBehaviour
{

    //private MeshRenderer renderer;
    [SerializeField]
    private GameObject scriptedObj;
    [SerializeField]
    private string type;

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
                Debug.Log("shutter");
                break;

            case "door":
                doorButtonOverride();
                Debug.Log("door");
                break;

            case "Func0001_Generator":
                func0001_Generator();
                break;

            case "Func0002_Generator":
                func0002_Generator();
                break;

            case "CrawlDoor":
                crawlDoorOpen();
                Debug.Log("crawlDoor");
                break;
            case "cargoElevator":
                cargoElevatorStart();
                Debug.Log("cargoElevator");
                break;
        }
    }

    //close and lock door, or unlock if locked.
    public void doorButtonOverride()
    {
        scriptedObj.GetComponentInChildren<DoorAnimator>().ButtonResponse();
    }

    public void crawlDoorOpen()
    {
        scriptedObj.GetComponent<CrawlDoorAnimator>().ButtonResponse();
    }

    public void shutterButton()
    {
        scriptedObj.GetComponentInChildren<ShutterAnimator>().buttonResponse();
    }

    public void cargoElevatorStart()
    {
        scriptedObj.GetComponent<CargoElevatorAnimator>().ButtonResponse();
    }

    public void func0001_Generator()
    {
        scriptedObj.GetComponentInChildren<PixelMap_Interpreter>().ButtonResponse();
    }

    public void func0002_Generator()
    {
        scriptedObj.GetComponentInChildren<PixelMap_ECSInterpreter>().ButtonResponse();
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
