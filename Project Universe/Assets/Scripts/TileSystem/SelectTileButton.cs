using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectTileButton : MonoBehaviour
{

    public int TileID;
    public GameObject Cube;
    private GameObject tile;
    private GameObject duplicate;
    private string path;

    public void Start()
    {
        Cube = GameObject.Find("TileGhost");
        Debug.Log(Cube);
    }

    public void WhenClicked(int a)
    {
        foreach (Transform child in Cube.transform)
            Destroy(child.gameObject);


        path = "Tile_Database/" + TileID.ToString();
        tile = GameObject.Find(path);
        duplicate = Instantiate<GameObject>(tile);
        foreach (Transform child in duplicate.transform)
        {
            child.tag = "PlacedTile";
        }
        duplicate.transform.rotation = Cube.transform.rotation;   //prevents from bar rotation orientation after selecting tile

        //tile.SetActive(true);

        duplicate.transform.SetParent(Cube.transform);
        duplicate.transform.localPosition = new Vector3(0, 0, 0);

        foreach (Transform child in duplicate.transform)
        {
            child.gameObject.layer = 0;
            foreach (Transform child2 in child.transform)
            {
                child2.gameObject.layer = 0;
            }
        }
    }

}
