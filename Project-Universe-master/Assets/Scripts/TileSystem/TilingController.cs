using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;



public class TilingController : MonoBehaviour
{

    public Camera Cam;
    public GameObject Tile;
    public bool AllowBuilding;
    private bool SnapToGrid;

    private Vector3 Coords;
    private Vector3 debugrayend;

    private bool sw = false;



    // Start is called before the first frame update
    void Start()
    {
       
        SnapToGrid = true;

        ReadXMLTiles();

      /* SAVING XML FILE FOR TESTING - THIS WILL GO FOR EXTERIOR EDITOR
        if (sw == false)
       {
           SaveXMLTiles();
           sw = true;
        }
      */
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
                    Debug.Log(hit.point);

                    if (hit.transform.gameObject.tag != "TilingGhost")  // prevents from raycasting tile

                        //tile snapping into 1x1x1 grid
                        if (SnapToGrid)
                        {
                            Coords.x = Mathf.Round(hit.point.x);
                            Coords.y = Mathf.Round(hit.point.y);
                            Coords.z = Mathf.Round(hit.point.z);
                        }
                    Tile.transform.position = Coords; //following tile
                }
            }
        }
        else
            Tile.SetActive(false);

    }


    //READING / SAVING XML FILE

    public void ReadXMLTiles()
    {
        var tileContainer = TileCollection.Load(Path.Combine(Application.dataPath, "Tiles.xml"));
       // var xmlData = @"<TileCollection><tiles><Tiles name=""a""><model_path></model_path>x<material_path>y</material_path></Tiles></tiles></TileCollection>";
       // var tileContainer = TileCollection.LoadFromText(xmlData);
        Debug.Log("Number of tiles in database: " + tileContainer.tiles.Length);
        Debug.Log(tileContainer.tiles[0].model_path);

    }

    public void SaveXMLTiles()
    {
        var tileContainer = TileCollection.Load(Path.Combine(Application.dataPath, "Tiles.xml"));
          tileContainer.Save(Path.Combine(Application.persistentDataPath, "Tiles.xml"));
    }
}

