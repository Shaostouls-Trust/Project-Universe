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
                IMyCannonInteract();//player);
                break;

            case "CrawlDoor":
                crawlDoorOpen();
                Debug.Log("crawlDoor");
                break;
            case "pickupable":
                //determine the type of the object
                Consumable_Ore COre;
                if (scriptedObj.TryGetComponent(out COre))
                {
                    Pickup<Consumable_Ore>();
                }
                break;
            case "FillFurnace":
                Debug.Log("FillFurnace");
                FillFurnace();
                break;
            case "EmptyFurnace":
                EmptyFurnace();
                break;
            case "FillFactory":
                FillFactory();
                break;
            case "StartFactory":
                Debug.Log("Trying to start Factory");
                StartFactory();
                break;
            case "EmptyFactory":
                EmptyFactory();
                break;
            case "MiningDrone":
                MiningDrone();
                break;
            case "DisplayInventory":
                DisplayInventory();
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

    public void IMyCannonInteract()//GameObject player)
    {
        scriptedObj.GetComponent<IControllableWeapon>().ButtonResponse(player);
    }

    public void crawlDoorOpen()
    {
        scriptedObj.GetComponent<CrawlDoorAnimator>().ButtonResponse();
    }

    public void Pickup<pickupType>()//GameObject player)
    {
        //will eventually need to go by type.
        Debug.Log("Pickup type: "+typeof(pickupType));
        if(typeof(pickupType) == typeof(Consumable_Ore))
        {
            scriptedObj.GetComponentInParent<Consumable_Ore>().PickUpConsumable(player);
        }
    }

    public void FillFurnace()
    {
        scriptedObj.GetComponent<Mach_InductionFurnace>().InputFromPlayer(player);
    }
    public void EmptyFurnace()
    {
        scriptedObj.GetComponent<Mach_InductionFurnace>().OutputToPlayer(player);
    }
    public void FillFactory()
    {
        Debug.Log("FillFactory");
        scriptedObj.GetComponent<Mach_DevFactory>().InputFromPlayer(player);
    }
    public void EmptyFactory()
    {
        scriptedObj.GetComponent<Mach_DevFactory>().OutputToPlayer(player);
    }
    public void StartFactory()
    {
        scriptedObj.GetComponent<Mach_DevFactory>().StartFactory();
    }

    public void MiningDrone()
    {
        scriptedObj.GetComponent<IMiningDrone>().EmptyInventory(player);
    }

    public void DisplayInventory()
    {
        scriptedObj.GetComponent<CargoContainer>().DisplayInventory();
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
