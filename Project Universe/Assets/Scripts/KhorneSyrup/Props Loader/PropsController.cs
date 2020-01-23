using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PropsController : MonoBehaviour
{
    private GameObject Cmra;
    private Camera Cam;
    private GameObject prop;
    private GameObject Selectedprop;
    public GameObject propselectionInd; // need to assign this at runtime..
    private GameObject PlayerID;
    private Collider propCol; //For turning on and off the colliders when placing.
    private MeshFilter mesh;
    public bool AllowBuilding;
    private bool SnapToGrid;

    private Vector3 Coords;
    private Vector3 debugrayend;

    // private bool sw = false;

    public GameObject BuildingMenu;
    public GameObject Button;
    private GameObject temp;
    private string FilePath;

    private GameObject newButton;
    private SelectPropButton but;

    //GUI testing
    public Text SelectedpropText;



    //material var
    public Shader Shad;
    public Shader ScaleableShad;
    private Material propMat;
    private Material ScaleableMat;
    public Material SelectedMat;


    //  public PropsContainer propsContainer = PropsContainer.Load(Path.Combine(Application.dataPath, "props.xml"));

    // Start is called before the first frame update
    void Start()
    {
        Cmra = GameObject.Find("Main Camera");
        Cam = Cmra.GetComponent<Camera>();

        SnapToGrid = true;

        ReadXmlProps();

        /* SAVING XML FILE FOR TESTING - THIS WILL GO FOR EXTERIOR EDITOR
          if (sw == false)
         {
             SaveXMLprops();
             sw = true;
          }
        */

        prop = new GameObject();
        prop.name = "propGhost";
        prop.tag = "TilingGhost";

        PlayerID = new GameObject();
        PlayerID.name = "Placedprops";

    }


    // Update is called once per frame
    void Update()
    {
        //Send to GUI what prop is currently selected. will probably move to function. 
        if (Selectedprop != null)
        {
            SelectedpropText.text = Selectedprop.name + " selected";
            propselectionInd.transform.position = Selectedprop.GetComponentInChildren<Renderer>().bounds.center;
            propselectionInd.SetActive(true);

        }
        else
        {
            SelectedpropText.text = "none selected";
            propselectionInd.SetActive(false);
        }

        if (AllowBuilding)
        {
            prop.SetActive(true);
            if (prop.transform.childCount > 0)
            {
                //Probably need to find a more efficient way of doing this? Or maybe a better spot to put it..
                propCol = prop.GetComponentInChildren<Collider>(); //Get Collider from active prop and disable.
                if (propCol != null)
                {
                    propCol.enabled = false; //Get Collider from active prop and disable.
                }
                else { Debug.LogError("You need to add a collider to this mesh!   :  " + prop.GetComponentInChildren<MeshFilter>().mesh.name); }
            }

            RaycastHit hit;
            Ray ray = Cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                if (Physics.Raycast(ray))
                {
                    //this is for debug on Scene, draw line for raycasting
                    Debug.DrawLine(hit.point, debugrayend, Color.red);
                    //   Debug.Log(hit.point);

                    if (hit.transform.gameObject.tag != "TilingGhost" || hit.transform.gameObject.tag != "Player")  // prevents from raycasting prop

                        //prop snapping into 1x1x1 grid
                        if (SnapToGrid)
                        {
                            Coords.x = Mathf.Round(hit.point.x);
                            Coords.y = Mathf.Round(hit.point.y);
                            Coords.z = Mathf.Round(hit.point.z);
                        }

                    prop.transform.position = Vector3.Lerp(prop.transform.position, Coords, Time.deltaTime * 15f); //moving prop

                    if (Input.GetButtonDown("Rotate"))  //rotating prop 90 degree
                    {
                        prop.transform.rotation = prop.transform.rotation * Quaternion.Euler(0, 90 * Input.GetAxisRaw("Rotate"), 0);
                        //prop.transform.rotation = Quaternion.Lerp(prop.transform.rotation, prop.transform.rotation * Quaternion.Euler(0, 90 * Input.GetAxisRaw("Rotate"), 0), Time.deltaTime * 10f); WIP
                    }
                }
            }

            //simple building without restrictions
            if (Input.GetMouseButtonDown(0))
            {
                if (!EventSystem.current.IsPointerOverGameObject())//Check if the pointer is over any GUI elements.
                {
                    if (prop.transform.childCount > 0)
                    {
                        Placeprop();
                    }
                    if (hit.transform.gameObject.tag == "Placedprop") //Maybe rename this to something less close to "Placedprops" XD 
                    {  // prevents from raycasting prop ghost
                        Selectprop(hit.collider.transform.parent.gameObject);
                    }
                    if (hit.transform.gameObject.tag != "Placedprop" && Selectedprop != null && prop.transform.childCount == 0)
                    {
                        Selectprop(null);
                    }
                    Debug.Log(Selectedprop);
                }
            }
            //Clear the current selected prop from the propghost
            if (Input.GetMouseButtonDown(1))
            {
                if (Selectedprop != null && prop.transform.childCount == 0)
                {
                    GameObject.Destroy(Selectedprop);
                }
                foreach (Transform child in prop.transform)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }


        }
        else
        {
            prop.SetActive(false);
        }
    }


    private LODGroup group;
    private Texture colorMask;    //public for debug - should be private
    private Texture albedo;
    private Texture dirt;
    private Texture emissiveAO;
    private Texture metalSmooth;
    private Texture normalDM;
    private Color mainColor;
    private Color secColor;
    private Color tertColor;
    private Color quatColor;
    private Color detColor;
    private Color secDetColor;
    private Color emColor;
    private float EmMulti;

    private MaterialContainer matContainer;
    private bool matXml;

    //READING / SAVING XML FILE

    public void ReadXmlProps()
    {
        var propsContainer = PropsContainer.Load(Path.Combine(Application.dataPath, "props.xml"));  //Loading from XML

        //  Debug.Log(matContainer.material[0].ColorMask);


        var parentDatabase = new GameObject();       //Creating prop database gameobjects
        parentDatabase.name = "prop_Database";
        parentDatabase.layer = 10;
        for (int c = 0; c < propsContainer.props.Length; c++)
        {

            //##########################PROCEDURE FOR LOADED prop#############################################


            FilePath = propsContainer.props[c].model_path;   //reading model info 
                                                            //  Debug.Log(propsContainer.props[c].model_path);
                                                            //  FilePath = "Models/props/Floors/nukeguard/mesh1";
            GameObject model = Resources.Load<GameObject>(FilePath);
            GameObject obj = (GameObject)Instantiate(model);

            //  Debug.Log("searching for: " + propsContainer.props[c].model_path + ".xml");
            // Debug.Log(Application.dataPath +"/Resources/" + propsContainer.props[c].model_path);
            if (System.IO.File.Exists(Application.dataPath + "/Resources/" + propsContainer.props[c].model_path + ".xml"))
            {
                Debug.Log("found: " + propsContainer.props[c].model_path + ".xml");
                matContainer = MaterialContainer.Load(Path.Combine(Application.dataPath + "/Resources/" + propsContainer.props[c].model_path + ".xml"));  //Loading from XML
                matXml = true;

            }
            else
                matXml = false;

            //LOAD AND INSTANTIATE MODEL





            // --------------------------Creating and assigning prop metadata------------------------------------
            PropMetadata mt = obj.AddComponent(typeof(PropMetadata)) as PropMetadata;
            mt.type = propsContainer.props[c].type;
            mt.name = propsContainer.props[c].Name;
            mt.description = "Description to implement";

            //-------------------------------------------------------------------------------------


            //-------------------put in database as not renderable with all children--------------------------
            foreach (Transform child in obj.transform)
            {
                child.gameObject.layer = 10;
                foreach (Transform child2 in child.transform)
                    child2.gameObject.layer = 10;
            }

            obj.transform.SetParent(parentDatabase.transform);
            obj.name = c.ToString();    //Adding and assigning props into database, change name by prop ID position


            //-------------Create GUI with buttons-----------
            newButton = Instantiate(Button);
            but = newButton.GetComponent<SelectPropButton>();
            but.PropID = c;
            newButton.GetComponentInChildren<Text>().text = propsContainer.props[c].Name;
            newButton.transform.SetParent(BuildingMenu.transform);
            //-----------------------------------------------


            // TO REMOVE WHEN ALL props IS UNIFIED - Blender garbage hotfix ;)
            if (obj.transform.Find("Camera") != null)
            {
                obj.transform.Find("Camera").GetComponent<Camera>().enabled = false;
            }
            if (obj.transform.Find("hitbox") != null)
            {
                obj.transform.Find("hitbox").gameObject.AddComponent<MeshCollider>();
                if (obj.transform.Find("hitbox").GetComponent<MeshRenderer>() != null)
                    obj.transform.Find("hitbox").GetComponent<MeshRenderer>().enabled = false;   //disable render of hitbox
            }
            // END REMOVE


            //------------------------IF prop HAS SCALE ANIMATION BEHAVIOR--------------------------------
            // Debug.Log(obj.transform.childCount);
            //---------------------------------------------------------------------------------------


            //----------------MATERIAL INSTANTIATE---------


            // if (propMat != null)
            //   propMat.shader = Shad;
            if (matXml)
            {
                colorMask = Resources.Load<Texture2D>(matContainer.material[0].ColorMask);   //Loading textures from XML
                albedo = Resources.Load<Texture2D>(matContainer.material[0].Albedo);
                metalSmooth = Resources.Load<Texture2D>(matContainer.material[0].MetalSmooth);
                emissiveAO = Resources.Load<Texture2D>(matContainer.material[0].EmmisiveAO);
                normalDM = Resources.Load<Texture2D>(matContainer.material[0].NormalDM);
                dirt = Resources.Load<Texture2D>(matContainer.material[0].Dirt);


                if (colorMask != null)                                      //Setting textures into new material 
                    propMat = new Material(Shad);

                ColorUtility.TryParseHtmlString(matContainer.material[0].MainColor, out mainColor);    //convert hex into color
                ColorUtility.TryParseHtmlString(matContainer.material[0].SecColor, out secColor);
                ColorUtility.TryParseHtmlString(matContainer.material[0].TertColor, out tertColor);
                ColorUtility.TryParseHtmlString(matContainer.material[0].QuatColor, out quatColor);
                ColorUtility.TryParseHtmlString(matContainer.material[0].DetailColor, out detColor);
                ColorUtility.TryParseHtmlString(matContainer.material[0].SecDetailColor, out secDetColor);
                ColorUtility.TryParseHtmlString(matContainer.material[0].EmissionColor, out emColor);

                propMat.SetTexture("_CM", colorMask);     //assigning textures and colors into new material
                propMat.SetTexture("_AL", albedo);
                propMat.SetTexture("_DWM", dirt);
                propMat.SetTexture("_EMAO", emissiveAO);
                propMat.SetTexture("_MS", metalSmooth);
                propMat.SetTexture("_NMDM", normalDM);
                propMat.SetColor("_MC", mainColor);
                propMat.SetColor("_SC", secColor);
                propMat.SetColor("_TC", tertColor);
                propMat.SetColor("_QC", quatColor);
                propMat.SetColor("_DC", detColor);
                propMat.SetColor("SDC", secDetColor);
                propMat.SetColor("_EC", emColor);
                propMat.SetFloat("_EI", matContainer.material[0].EmissiveMulti);
                propMat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;    // realtime emissive flag


                //---------------------------------ASSIGNING MATERIALS--------------------------------
                if (obj.transform.Find("model") != null)                                //Assigning new instance of material to model
                {
                    for (int m = 0; m < obj.transform.Find("model").GetComponentInChildren<MeshRenderer>().materials.Length; m++)      //For every material in model
                    {
                        obj.transform.Find("model").GetComponentInChildren<MeshRenderer>().materials[m].CopyPropertiesFromMaterial(propMat);
                        foreach (Transform child in obj.transform.Find("model").transform)
                        {
                            child.GetComponentInChildren<MeshRenderer>().materials[m].CopyPropertiesFromMaterial(propMat);
                        }

                        obj.transform.Find("lod1").GetComponentInChildren<MeshRenderer>().materials[m].CopyPropertiesFromMaterial(propMat);
                        foreach (Transform child in obj.transform.Find("lod1").transform)
                        {
                            child.GetComponentInChildren<MeshRenderer>().materials[m].CopyPropertiesFromMaterial(propMat);
                        }

                        obj.transform.Find("lod2").GetComponentInChildren<MeshRenderer>().materials[m].CopyPropertiesFromMaterial(propMat);
                        foreach (Transform child in obj.transform.Find("lod2").transform)
                        {
                            child.GetComponentInChildren<MeshRenderer>().materials[m].CopyPropertiesFromMaterial(propMat);
                        }

                        //obj.transform.Find("lod3").GetComponentInChildren<MeshRenderer>().materials[m].CopyPropertiesFromMaterial(propMat);
                        obj.transform.Find("model_scale_back").GetComponentInChildren<SkinnedMeshRenderer>().materials[m].CopyPropertiesFromMaterial(propMat);
                        obj.transform.Find("model_scale_front").GetComponentInChildren<SkinnedMeshRenderer>().materials[m].CopyPropertiesFromMaterial(propMat);
                        obj.transform.Find("model_scale_left").GetComponentInChildren<SkinnedMeshRenderer>().materials[m].CopyPropertiesFromMaterial(propMat);
                        obj.transform.Find("model_scale_right").GetComponentInChildren<SkinnedMeshRenderer>().materials[m].CopyPropertiesFromMaterial(propMat);
                    }
                }

            }
            //---------------------------------------------


            //--------------------------------Building LOD groups------------------------------ 
            if (obj.transform.Find("model") != null)
            {
                group = obj.AddComponent<LODGroup>();

                // Add 4 LOD levels
                LOD[] lods = new LOD[4];

                for (int i = 0; i < 4; i++)
                {
                    GameObject primType = obj.transform.Find("model").gameObject;
                    switch (i)
                    {
                        case 1:
                            if (obj.transform.Find("lod1") != null)
                                primType = obj.transform.Find("lod1").gameObject;
                            break;
                        case 2:
                            if (obj.transform.Find("lod2") != null)
                                primType = obj.transform.Find("lod2").gameObject;
                            break;
                        case 3:
                            if (obj.transform.Find("lod3") != null)
                                primType = obj.transform.Find("lod3").gameObject;
                            break;
                    }

                    Renderer[] renderers = new Renderer[10];

                    renderers[0] = primType.GetComponent<Renderer>();



                    //-------------------------------------------------
                    if (primType.transform.childCount > 0)

                        for (int ch = 0; ch < primType.transform.childCount; ch++)
                        {
                            renderers[ch + 1] = primType.transform.GetChild(ch).gameObject.GetComponent<Renderer>();
                        }

                    lods[i] = new LOD(1.0F / (i + 3f), renderers); // i+1.2f
                }
                group.SetLODs(lods);
                group.RecalculateBounds();
            }
            //--------------------------------------------------------------------------------
        }
    }

    public void SaveXMLprops()
    {
        var propsContainer = PropsContainer.Load(Path.Combine(Application.dataPath, "props.xml"));
        propsContainer.Save(Path.Combine(Application.persistentDataPath, "props.xml"));
    }

    public void Placeprop()
    {
        if (AllowBuilding)
        {
            if (prop.transform.childCount > 0)
            {
                Debug.Log(prop.gameObject.transform.GetChild(0).gameObject);
                GameObject newpropChild = prop.gameObject.transform.GetChild(0).gameObject;
                GameObject newprop = (GameObject)Instantiate(newpropChild);
                Selectedprop = newprop;
                //Probably a more elegant way to do this.
                //Get the collider from the prop and enable it before placing.
                Collider HitBox = newprop.GetComponentInChildren<Collider>();
                HitBox.enabled = true;
                HitBox.tag = "Placedprop";
                newprop.tag = "Placedprop";

                newprop.transform.localPosition = Coords;
                newprop.transform.localRotation = prop.transform.rotation;
                newprop.transform.localScale = prop.transform.localScale;
                newprop.transform.SetParent(PlayerID.transform);

                //Set prop MetaData and add prop to selection
                newprop.name = newprop.GetComponent<PropMetadata>().name;
                newprop.GetComponent<PropMetadata>().buildBy = "PeterHammerman test";
                Selectprop(newprop);
            }
        }
    }

    public void Selectprop(GameObject Selected)
    {
        Selectedprop = Selected;

    }
    public void Removeprop()
    {

    }
}

