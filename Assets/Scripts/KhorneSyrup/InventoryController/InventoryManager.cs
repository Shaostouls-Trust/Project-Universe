using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Jobs;

public class InventoryManager : MonoBehaviour
{
    // Define Camera Values.
    [SerializeField] private GameObject cameraPrefab; // Prefab for the icon cameras.
    [SerializeField] private List<GameObject> cameras = new List<GameObject>(); // Crrently created target
    [SerializeField] private GameObject cameraBin; // Stores all camera gameObjects.
    [SerializeField] private List<CustomRenderTexture> renderTex = new List<CustomRenderTexture>(); // Render textures for processing with camera.
    // Define Target Values.
    [SerializeField] private List<GameObject> targets = new List<GameObject>(); // All targets that have been created. may convert to list.
    [SerializeField] private GameObject currentTarget; // Currently cycled target.
    public GameObject targetBin; // Stores all the targetpoint gameObjects.
    [SerializeField] private int maxTargets = 12; // Max targets. currently is "maxInventory" / 5. will possibly change to some other less arbitrary value in the future. should likely be based off of hardware.
    // Define Inventory Values.
    [SerializeField] private GameObject IconPrefab;
    [SerializeField] private GameObject InventoryPanel;
    [SerializeField] public List<GameObject> stagedObjects = new List<GameObject>(); // Stores gameobjects pending transfer to target.
    [SerializeField] private List<GameObject> objects = new List<GameObject>(); // Stores gameobjects associated with players inventory. Should probably convert to list in the future.
    public GameObject inventoryContainer; // Stores all objects in the players inventory.
    [SerializeField] private int maxInventory = 60; // Max inventory size.. Currently arbitrary. will link to player carry capacity in the future.
    [SerializeField] private int itemCount = 0;
    [SerializeField] private List<GameObject> Icons = new List<GameObject>(); // created icons.
    [SerializeField] private List<CustomRenderTexture> RenderTextures = new List<CustomRenderTexture>();
    [SerializeField] private List<RawImage> IconTex = new List<RawImage>(); // Saved custom render textures to apply to icons.
    //[SerializeField] private ItemClass[] Items; // Stored Item information.
    [SerializeField] private List<ItemClass> Items = new List<ItemClass>();
    // Define Graphics settings/values.
    [SerializeField] private float rotationSpeed = 5.0f;// Rotation speed of objects in the inventory.
    [SerializeField] private int renderMult = 1; // Multiplier for rendertexture scale.
    public bool processed = false; // Check for if the inventory has been processed.
    [SerializeField] private BoxCollider2D panelCollider;
    private static int hideLayer = 18;
    private static int showLayer = 17;

    public int target = 0;
    int currentItem = 0;


    // Start is called before the first frame update
    void Start()
    {
        // Create Target bin.
        targetBin = new GameObject();
        targetBin.transform.parent = this.gameObject.transform;
        targetBin.name = "Target Bin";
        
        // Create Camera bin.
        cameraBin = new GameObject();
        cameraBin.transform.parent = this.gameObject.transform;
        cameraBin.name = "Camera Bin";
    }

    // Update is called once per frame
    void Update()
    {
        if (processed == false)
        {
            ProcessInventory();
        }
        else { CycleInventory(); }
    }

    void CycleInventory()
    {
        foreach (Transform obj in targetBin.transform)
        {
            bool isVisible = obj.GetComponent<ItemClass>().Icon.isVisible;
            obj.gameObject.SetActive(isVisible);
            if (isVisible)
                {
                obj.Rotate(rotationSpeed, rotationSpeed, 0);
            }
        }
    }
    
    void ProcessInventory()
    {
        foreach (Transform item in inventoryContainer.transform)
        {
            StartCoroutine(EAddItemToInventory(item.gameObject, item.GetComponent<ItemClass>()));
        }
        objects = stagedObjects;
        StartCoroutine(ECreateInventory());
    }
    IEnumerator ECreateInventory()
    {
        if (inventoryContainer.transform.childCount == 0)
        {
            processed = true;
        }
        yield return null;
    }
    public IEnumerator EAddItemToInventory(GameObject item, ItemClass info)
    {
            //StartCoroutine(EAddNewTarget(target));
        //Debug.Log(item.name);
        stagedObjects.Add(item);
        Items.Add(info);
        Icons.Add(info.gameObject);
        item.transform.position = targetBin.transform.position + new Vector3(currentItem * 3 ,0,0 );
        item.transform.parent = targetBin.transform;
        item.layer = showLayer;
        StartCoroutine(ECreateIcon(info, target, item));
        currentItem++;

        yield return null;
    }

     public IEnumerator ERemoveItemFromInventory(GameObject item, ItemClass info)
    {
        int i = 0;
        foreach (Transform obj in targetBin.transform)
        {
            if (obj == item.transform)
            {
                Icons.Remove(info.Icon.gameObject);
                Icons.Remove(Icons[i]);
                Items.Remove(info);
                IconTex.Remove(info.Icon.IconTexture);
                objects.Remove(obj.gameObject);
                cameras.Remove(cameras[i]);
                RenderTextures.Remove(RenderTextures[i]);
                Destroy(info.Icon.IconTexture);
                Destroy(info.Icon.gameObject);
                Destroy(item);
                Destroy(info.Icon.cam);
                currentItem--;
                i++;
            }

        }
        yield return null;
    }
    IEnumerator ECreateIcon(ItemClass info, int targetCamera, GameObject item)
    {
        GameObject cam = Instantiate(cameraPrefab, cameraBin.transform);
        cameras.Add(cam);
        cam.transform.position = item.transform.position + new Vector3(0, 0, -4.5f) ;
        GameObject icon = Instantiate(IconPrefab, InventoryPanel.transform);
        UIItem itemData = icon.GetComponent<UIItem>();
        CustomRenderTexture tex = new CustomRenderTexture(16 * renderMult, 16 * renderMult);
        tex.name = info.gameObject.name + " RenderTexture";
        cam.GetComponent<Camera>().targetTexture = tex;
        itemData.itemInfo = info;
        RenderTextures.Add(tex);
        itemData.panelCollider = panelCollider;
        itemData.IconTexture.texture = tex;
        itemData.cam = cam.GetComponent<Camera>();
        IconTex.Add(itemData.IconTexture);
        info.Icon = itemData;
        itemData.inventory = this;
        Collider col = item.GetComponent<BoxCollider>();
        Rigidbody rig = item.GetComponent<Rigidbody>();
        rig.isKinematic = true;
        col.enabled = false;

        yield return null;
    }
  
}
