using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]


public class RoomController : MonoBehaviour
{
    [System.Serializable]
    public struct TileList
    {
        public GameObject[] Tiles;
    }
    // Basic Information.
    [SerializeField] private GameObject parent;
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private GameObject[] TileContainer;
    [SerializeField] private Vector3 roomScale = new Vector3( 1, 1, 1); // Set default as a room of 1x1x1.
    // Integers.
    [SerializeField] private int maxTiles; // Maximum tiles per floor.
    [SerializeField] private int currentTile = 0; // Current tile value.
    // Floats.
    [SerializeField] private float tileWidth = 0.10f; // Set the width of each tile. ( can be adjusted per tile for multi-block tiles.
    [SerializeField] private float floorHeight = 0.30f; // set the height of each floor. ( can be adjusted for larger dead space).
    // Arrays.
    [SerializeField] public TileList[] tileList; // Array of arrays. Used one per floor each with x amount of tiles.
    // Booleans.
    [SerializeField] private bool processed = false;
    [SerializeField] private bool clearingObjects = false;
    [SerializeField] private bool[] hideFloors;

    // Start is called before the first frame update
    void Start()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        // run Update function
        UpdateTiles();
    }
    void UpdateTiles()
    {
        // Set max tiles and array lengths for each floor.
        maxTiles = (int)roomScale.x * (int)roomScale.y;

        // Check if floors have been processed and if not, do so.
        if (processed == false && clearingObjects == false)
        {
            TileContainer = new GameObject[(int)roomScale.z];
            hideFloors = new bool[(int)roomScale.z];
            tileList = new TileList[(int)roomScale.z];
            for (int i = 0; i < (int)roomScale.z; i++)
            {
                tileList[i].Tiles = new GameObject[maxTiles];
            }
            // Set Max tiles and floor array.
            // Reset current selection;

            // For each level of floor do this. then index floor.
            for (int f = 0; f < tileList.Length ; f++)
            {
                if (TileContainer[f] == null)
                {
                    TileContainer[f] = Instantiate(new GameObject(), parent.transform);
                    TileContainer[f].name = "Tile Container: " + f;
                }
                else { TileContainer[f] = TileContainer[f]; }
                int currentTile = 0;
                // for each x Direction do this.
                for (int t = 0; t < (int)roomScale.x; t++)
                {
                    // for each y Direction do this
                    for (int y = 0; y < (int)roomScale.y; y++)
                    {
                        // Create each tile and then index. will be replaced with empty tile that will not be displayed in realworld.=====need to implement.=====
                        tileList[f].Tiles[currentTile] = Instantiate(tilePrefab, TileContainer[f].transform).gameObject;
                        tileList[f].Tiles[currentTile].transform.position = parent.transform.position + new Vector3(y*0.10f, f * 0.10f, t * 0.10f);
                        currentTile++;
                    }
                }

            }
            processed = true;
        }
        //Check if the room has already been generated and if so, clear room before generating new one in its place
        else if (processed == false && clearingObjects == true)
        {
            foreach (GameObject obj in TileContainer)
            {
                Destroy(obj);
                clearingObjects = false;
            }
            UpdateTiles();
        }
        VisibleFloors(hideFloors);

    }

    void VisibleFloors(bool[] floor)
    {
        for (int f = 0; f < tileList.Length; f++)
        {
            if (floor[f])
            {
                TileContainer[f].SetActive(false);
            }
            else { TileContainer[f].SetActive(true); }
        }
    }
}
