using ProjectUniverse;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class RoomSetupExample : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private PlayerControls controls;

    [Header("Materials")]
    [SerializeField] private Material hologramBaseMaterial; // Assign your custom material here
    [SerializeField] private Material templateMaterial; // Material for template objects

    public GameObject labelPrefab;
    public Canvas labelCanvas;

    [SerializeField] private HolographicViewController hvc;
    [SerializeField] private HolographicRoomManager manager;

    void Awake()
    {
        if (controls == null)
            controls = new PlayerControls();

        // Create template material if not assigned
        if (templateMaterial == null)
        {
            templateMaterial = new Material(hologramBaseMaterial);
            templateMaterial.SetColor("_BaseColor", new Color(1.0f, 0.5f, 0.0f, 0.5f)); // Orange color
        }
    }

    void OnEnable()
    {
        if (controls != null)
        {
            controls.Player.Look.Enable();
            controls.Player.Fire.Enable();
            controls.Player.RightClick.Enable();
            controls.Player.ScrollWheel.Enable();
        }
    }

    void OnDisable()
    {
        if (controls != null)
        {
            controls.Player.Look.Disable();
            controls.Player.Fire.Disable();
            controls.Player.RightClick.Disable();
            controls.Player.ScrollWheel.Disable();
        }
    }

    void Start()
    {
        SetupHolographicSystem();
    }

    void SetupHolographicSystem()
    {
        // 1. Create System Root
        //GameObject systemRoot = new GameObject("Holographic System");

        // 2. Create and configure Room Manager
        //GameObject managerObj = new GameObject("Holographic Room Manager");
        //managerObj.transform.parent = systemRoot.transform;
        //HolographicRoomManager manager = managerObj.AddComponent<HolographicRoomManager>();
        manager.baseMaterial = hologramBaseMaterial;
        manager.templateMaterial = templateMaterial;
        //manager.labelCanvas = labelCanvas;

        // Create label prefab
        //manager.labelPrefab = labelPrefab;

        // 3. Create Room 1 - Living Room
        /*GameObject room1Obj = new GameObject("Living Room");
        room1Obj.transform.parent = systemRoot.transform;
        HolographicRoom room1 = room1Obj.AddComponent<HolographicRoom>();
        room1.roomName = "Living Room";

        // Add BoxColliders for volumes
        BoxCollider volume1_1 = room1Obj.AddComponent<BoxCollider>();
        volume1_1.center = new Vector3(0, 1.5f, 0);
        volume1_1.size = new Vector3(6, 3, 8);

        BoxCollider volume1_2 = room1Obj.AddComponent<BoxCollider>();
        volume1_2.center = new Vector3(4, 1.5f, 0);
        volume1_2.size = new Vector3(2, 3, 4);

        // Set the volume container (self in this case)
        room1.volumeContainer = room1Obj;

        // Create a Templates container
        GameObject templatesContainer = new GameObject("Templates");
        templatesContainer.transform.parent = room1Obj.transform;

        // Add some template objects
        GameObject template1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        template1.transform.parent = templatesContainer.transform;
        template1.transform.position = new Vector3(0, 1, 0);
        template1.transform.localScale = new Vector3(1, 2, 1);
        DestroyImmediate(template1.GetComponent<Collider>());

        GameObject template2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        template2.transform.parent = templatesContainer.transform;
        template2.transform.position = new Vector3(3, 1, 0);
        template2.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        DestroyImmediate(template2.GetComponent<Collider>());

        // 4. Create Room 2 - Bedroom
        GameObject room2Obj = new GameObject("Bedroom");
        room2Obj.transform.parent = systemRoot.transform;
        HolographicRoom room2 = room2Obj.AddComponent<HolographicRoom>();
        room2.roomName = "Bedroom";

        // Add BoxCollider for volume
        BoxCollider volume2_1 = room2Obj.AddComponent<BoxCollider>();
        volume2_1.center = new Vector3(-5, 1.5f, 5);
        volume2_1.size = new Vector3(4, 3, 5);

        // Set the volume container
        room2.volumeContainer = room2Obj;

        // Create a Templates container
        GameObject templatesContainer2 = new GameObject("Templates");
        templatesContainer2.transform.parent = room2Obj.transform;

        // Add a template object
        GameObject template3 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        template3.transform.parent = templatesContainer2.transform;
        template3.transform.position = new Vector3(-5, 1, 5);
        template3.transform.localScale = new Vector3(1, 1.8f, 1);
        DestroyImmediate(template3.GetComponent<Collider>());

        // 5. Add rooms to manager
        manager.rooms.Add(room1);
        manager.rooms.Add(room2);*/

        // 6. Setup Camera
        /*GameObject cameraObj = new GameObject("Hologram Camera");
        Camera holoCam = cameraObj.AddComponent<Camera>();
        holoCam.transform.position = new Vector3(10, 8, -10);
        holoCam.transform.LookAt(Vector3.zero);
        holoCam.clearFlags = CameraClearFlags.SolidColor;
        holoCam.backgroundColor = new Color(0.02f, 0.02f, 0.05f);

        // Adjust viewport rect to only use right half of the screen
        holoCam.rect = new Rect(0.5f, 0, 0.5f, 1);

        HolographicViewController controller = cameraObj.AddComponent<HolographicViewController>();
        controller.hologramCamera = holoCam;*/

        // Setup input references
        if (controls != null)
        {
            hvc.lookAction = controls.Player.Look;
            hvc.clickAction = controls.Player.Fire;
            hvc.rightClickAction = controls.Player.RightClick;
            hvc.scrollAction = controls.Player.ScrollWheel;
        }
    }
}