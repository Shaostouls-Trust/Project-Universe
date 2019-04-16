using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;



public class TilingController : MonoBehaviour
{
    private GameObject Cmra;
    private Camera Cam;
    private GameObject Tile;
    private GameObject PlayerID;
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
    private SelectTileButton but;

    //  public TileCollection tileContainer = TileCollection.Load(Path.Combine(Application.dataPath, "Tiles.xml"));

    // Start is called before the first frame update
    void Start()
    {
        Cmra = GameObject.Find("Main Camera");
        Cam = Cmra.GetComponent<Camera>();

        SnapToGrid = true;

        ReadXMLTiles();

        /* SAVING XML FILE FOR TESTING - THIS WILL GO FOR EXTERIOR EDITOR
          if (sw == false)
         {
             SaveXMLTiles();
             sw = true;
          }
        */

        Tile = new GameObject();
        Tile.name = "TileGhost";
        Tile.tag = "TilingGhost";

        PlayerID = new GameObject();
        PlayerID.name = "PlayerID";

    }


    // Update is called once per frame
    void Update()
    {
        if (AllowBuilding)
        {
            Tile.SetActive(true);

            RaycastHit hit;
            Ray ray = Cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                if (Physics.Raycast(ray))
                {
                    //this is for debug on Scene, draw line for raycasting
                    Debug.DrawLine(hit.point, debugrayend, Color.red);
                    //   Debug.Log(hit.point);

                    if (hit.transform.gameObject.tag != "TilingGhost")  // prevents from raycasting tile

                        //tile snapping into 1x1x1 grid
                        if (SnapToGrid)
                        {
                            Coords.x = Mathf.Round(hit.point.x);
                            Coords.y = Mathf.Round(hit.point.y);
                            Coords.z = Mathf.Round(hit.point.z);
                        }

                    Tile.transform.position = Vector3.Lerp(Tile.transform.position, Coords, Time.deltaTime * 15f); //moving tile

                    if (Input.GetButtonDown("Rotate"))  //rotating tile 90 degree
                    {
                        Tile.transform.rotation = Tile.transform.rotation * Quaternion.Euler(0, 90 * Input.GetAxisRaw("Rotate"), 0);
                        //Tile.transform.rotation = Quaternion.Lerp(Tile.transform.rotation, Tile.transform.rotation * Quaternion.Euler(0, 90 * Input.GetAxisRaw("Rotate"), 0), Time.deltaTime * 10f); WIP
                    }
                }
            }

            //simple building without restrictions
            if (Input.GetMouseButtonDown(0))
            {
                PlaceTile();
            }


        }
        else
            Tile.SetActive(false);



    }


    private LODGroup group;

    //READING / SAVING XML FILE

    public void ReadXMLTiles()
    {
        var tileContainer = TileCollection.Load(Path.Combine(Application.dataPath, "Tiles.xml"));  //Loading from XML
        // var xmlData = @"<TileCollection><tiles><Tiles name=""a""><model_path></model_path>x<material_path>y</material_path></Tiles></tiles></TileCollection>";
        // var tileContainer = TileCollection.LoadFromText(xmlData);
        // Debug.Log("Number of tiles in database: " + tileContainer.tiles.Length);
        //  Debug.Log(tileContainer.tiles[0].model_path);

        var parentDatabase = new GameObject();       //Creating tile database gameobjects
        parentDatabase.name = "Tile_Database";
        parentDatabase.layer = 10;
        for (int c = 0; c < tileContainer.tiles.Length; c++)
        {
            FilePath = tileContainer.tiles[c].model_path;   //reading model info 
            Debug.Log(tileContainer.tiles[c].model_path);
            //  FilePath = "Models/Tiles/Floors/nukeguard/mesh1";
            GameObject model = Resources.Load<GameObject>(FilePath);
            GameObject obj = (GameObject)Instantiate(model);

            // --------------------------Creating and assigning tile metadata------------------------------------
            TileMetadata mt = obj.AddComponent(typeof(TileMetadata)) as TileMetadata;
            mt.type = tileContainer.tiles[c].type;
            mt.name = tileContainer.tiles[c].Name;
            mt.description = "Description to implement";
            //-------------------------------------------------------------------------------------


            //-------------------put database as not renderable with all children--------------------------
            foreach (Transform child in obj.transform)
                child.gameObject.layer = 10;

            obj.transform.SetParent(parentDatabase.transform);
            obj.name = c.ToString();    //Adding and assigning tiles into database, change name by tile ID position


            //-------------Create GUI with buttons-----------
            newButton = Instantiate(Button);
            but = newButton.GetComponent<SelectTileButton>();
            but.TileID = c;
            newButton.GetComponentInChildren<Text>().text = tileContainer.tiles[c].Name;
            newButton.transform.SetParent(BuildingMenu.transform);
            //-----------------------------------------------


            // TO REMOVE WHEN ALL TILES IS UNIFIED - Blender garbage hotfix ;)
            if (obj.transform.Find("Camera") != null)
            {
                obj.transform.Find("Camera").GetComponent<Camera>().enabled = false;
            }
            // END REMOVE
       
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
                            primType = obj.transform.Find("lod1").gameObject;
                            break;
                        case 2:
                            primType = obj.transform.Find("lod2").gameObject;
                            break;
                        case 3:
                            primType = obj.transform.Find("lod3").gameObject;
                            break;
                    }
                    Renderer[] renderers = new Renderer[1];
                    renderers[0] = primType.GetComponentInChildren<Renderer>();
                    lods[i] = new LOD(1.0F / (i + 12f), renderers); // i+1.2f
                }
                group.SetLODs(lods);
                group.RecalculateBounds();
            }
            //--------------------------------------------------------------------------------
        }
    }

    public void SaveXMLTiles()
    {
        var tileContainer = TileCollection.Load(Path.Combine(Application.dataPath, "Tiles.xml"));
        tileContainer.Save(Path.Combine(Application.persistentDataPath, "Tiles.xml"));
    }

    public void PlaceTile()
    {

        if (Tile.transform.childCount > 0)
        {
            Debug.Log(Tile.gameObject.transform.GetChild(0).gameObject);
            GameObject newTileChild = Tile.gameObject.transform.GetChild(0).gameObject;
            GameObject newTile = (GameObject)Instantiate(newTileChild);
            newTile.transform.localPosition = Coords;
            newTile.transform.localRotation = Tile.transform.rotation;
            newTile.transform.localScale = Tile.transform.localScale;
            newTile.transform.SetParent(PlayerID.transform);
            newTile.GetComponent<TileMetadata>().buildBy = "PeterHammerman test";
        }
    }

    public void RemoveTile()
    {

    }
}

