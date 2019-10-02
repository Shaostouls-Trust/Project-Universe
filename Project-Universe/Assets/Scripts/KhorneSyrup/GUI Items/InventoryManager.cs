using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    // Camera settings.
    [SerializeField] private Camera[] cameras;
    [SerializeField] private GameObject cameraPrefab;
    [SerializeField] private GameObject CameraBin;
    // Target settings.
    [SerializeField] private GameObject[] targets;
    [SerializeField] private GameObject targetParent;
    [SerializeField] private GameObject TargetBin;
    // Inventory variables.
    [SerializeField] private GameObject[] renderObjects;
    [SerializeField] private ItemClass[] objectInformation;
    [SerializeField] private int inventorySpace;
    [SerializeField] private GameObject inventoryItem;
    // GUI items.
    [SerializeField] private RawImage[] renderIcons;
    [SerializeField] private Texture2D[] renderIconsTex;
    [SerializeField] private CustomRenderTexture[] renderTextures;
    [SerializeField] private CustomRenderTexture[] targetTex;
    // integers.
    [SerializeField] private int invCount;
    [SerializeField] private int cameraCount;
    [SerializeField] private int currentItem;
    [SerializeField] private int currentCamera;
    // Booleans.
    [SerializeField] private bool processed = false;


    private static readonly int hide = 14; // Hide the currently selected Object by moving to "NonRenderableTexture" layer.
    private static readonly int show = 13; // Show the currently selected Object by moving to "RenderableTexture" layer.

    // Start is called before the first frame update.
    void Start()
    {
        // Create Camera bin.
        CameraBin = Instantiate(new GameObject(), transform);
        CameraBin.name = "Camera Bin";

        // Create Target bin.
        TargetBin = Instantiate(new GameObject(), targetParent.transform);
        TargetBin.name = "Target Bin";



    }
    // Process all objects in inventory.
    void ProcessGameObjects()
    {
        //Check if we've already acquired the inventory.
        if (processed == false) {

            int a = 0;
            currentItem = 0;
            currentCamera = 0;
            // Get all objects currently inside of inventory.
            foreach (Transform child in targetParent.transform)
            {
                // Check if this is an item or a target position.
                if (child.tag == "InventoryItem")
                {
                    a++;
                    if (a % 5 == 0)
                    {
                        cameraCount++;
                    }
                }
            }
            Debug.Log("test");
            // Set inventory array lengths.
            renderObjects = new GameObject[a];
            objectInformation = new ItemClass[a];
            renderIcons = new RawImage[a];
            renderTextures = new CustomRenderTexture[a];
            cameras = new Camera[cameraCount];
            targets = new GameObject[cameraCount];
            targets[0] = Instantiate(new GameObject(), TargetBin.transform);
            targets[0].name = "Target Point: " + 0;

            foreach (Transform child in targetParent.transform)
            {
                // Check if this is an item or a target position.
                if (child.tag == "InventoryItem")
                {
                    if (currentItem < renderObjects.Length)
                    {
                        // Check if we have more than 5 items in inventory and add to additional Target Point.
                        if (currentItem % 5 == 0)
                        {
                            currentCamera++;
                            renderObjects[currentItem] = child.gameObject;
                            renderObjects[currentItem].transform.parent = targets[currentCamera].transform;
                            IndexCamera();
                            currentItem++;
                        }
                        // Else, add to default target point.
                        else
                        {
                            renderObjects[currentItem] = child.gameObject;
                            renderObjects[currentItem].transform.parent = targets[currentCamera].transform;
                            objectInformation[currentItem] = child.gameObject.GetComponent<ItemClass>();
                            currentItem++;
                        }
                    }
                }
            }


            // Set processed to true after all is completed.
            processed = true;
        }
    }
    void IndexCamera()
    {
        cameras = new Camera[cameraCount];
        targets = new GameObject[cameraCount];
        targets[currentCamera] = Instantiate(new GameObject(), TargetBin.transform);
    }
    // Create and set Render Textures as required.
    void SetTextures(int count)
    {

    }
    // Refresh all cameras and targets.
    void RefreshCameras(int count)
    {

    }

    // Update is called once per frame.
    void Update()
    {
        ProcessGameObjects();
    }
}
