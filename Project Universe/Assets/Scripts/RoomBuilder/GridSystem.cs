using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
#if (UNITY_EDITOR)
public class GridSystem : MonoBehaviour
{
    [SerializeField] private GameObject roomTemplate;
    public GameObject blockPrefab;  // The block prefab to place in the grid.
    [SerializeField] private GameObject[] blocksPrefabs;
    [SerializeField] private GameObject currentRoom;
    [SerializeField] private Vector3 gridSize = new(10, 1, 10);
    public float cellSize = 1f;
    public float roomBottomY = 0f;
    public float staticSizeY = 6f;
    public bool isFloorBlock = true;  // Whether we are placing floor blocks or not (restrict Y-axis placement).
    public bool drawYAxisLines = true;
    public bool drawGridLines = true;
    private EditingSection activesection = EditingSection.FloorBase;
    private Dictionary<string, GameObject> tileHolders;
    [SerializeField] private TileGroup tileGroup;
    private GameObject lofloGO;
    private GameObject ceilGO;
    private GameObject doorGO;
    private GameObject ductGO;
    private GameObject floorGO;
    private GameObject lightGO;
    private GameObject oStrGO;
    private GameObject pipeGO;
    private GameObject stairGO;
    private GameObject uflrGO;
    private GameObject wallGO;
    private GameObject wstrGO;
    private GameObject bsGO;

public enum EditingSection
    {
        FloorBase,
        MainRoom,
        Ceiling,
        Interior
    }

    public EditingSection ActiveSection
    {
        get { return activesection; }
        set { activesection = value; }
    }
    public GameObject[] BlockPrefabs
    {
        get { return blocksPrefabs; }
    }
    public GameObject RoomTemplate
    {
        get { return roomTemplate; }
    }
    public GameObject CurrentRoom
    {
        get { return currentRoom; }
        set { currentRoom = value; }
    }
    public Dictionary<string, GameObject> TileHolders
    {
        get { return tileHolders; }
        set { tileHolders = value; }
    }
    public TileGroup MyTileGroup
    {
        get { return tileGroup; }
    }

    public Vector3 GridSize
    {
        get { return gridSize; }
        set { gridSize = value; }
    }
    public GameObject LowerFloorGO
    {
        get { return lofloGO; }
        set { lofloGO = value; }
    }

    public GameObject BaseStructGO
    {
        get { return bsGO; }
        set { bsGO = value; }
    }

    public GameObject FloorGO
    {
        get { return floorGO; }
        set { floorGO = value; }
    }

    public GameObject WallGO
    {
        get { return wallGO; }
        set { wallGO = value; }
    }

    public GameObject WallStrutGO
    {
        get { return wstrGO; }
        set { wstrGO = value; }
    }

    public GameObject DoorGO
    {
        get { return doorGO; }
        set { doorGO = value; }
    }

    public GameObject StairGO
    {
        get { return stairGO; }
        set { stairGO = value; }
    }

    public GameObject DuctGO
    {
        get { return ductGO; }
        set { ductGO = value; }
    }

    public GameObject LightGO
    {
        get { return lightGO; }
        set { lightGO = value; }
    }

    public GameObject OverStructGO
    {
        get { return oStrGO; }
        set { oStrGO = value; }
    }

    public GameObject PipeGO
    {
        get { return pipeGO; }
        set { pipeGO = value; }
    }

    public GameObject CeilingGO
    {
        get { return ceilGO; }
        set { ceilGO = value; }
    }

    /// <summary>
    /// Places a block in the grid at the specified position and rotation.
    /// </summary>
    /// <param name="position">The position to place the block.</param>
    /// <param name="rotation">The rotation to apply to the block.</param>
    /// <param name="parent">The parent transform for the block.</param>
    /// <returns>The instantiated block GameObject.</returns>
    public GameObject PlaceBlockInGrid(Vector3 position, Quaternion rotation, Transform parent)
    {
        ///GameObject newBlock = Instantiate(blockPrefab, position, rotation, parent);
        GameObject newBlock = (GameObject)PrefabUtility.InstantiatePrefab(blockPrefab, parent);
        newBlock.transform.parent = parent;
        newBlock.transform.position = position;
        Vector3 defaultAngles = newBlock.transform.rotation.eulerAngles;
        newBlock.transform.rotation = Quaternion.Euler(rotation.eulerAngles + defaultAngles);

        TilePrefabSelector prefabSelector = newBlock.AddComponent<TilePrefabSelector>();
        prefabSelector.GridSystem = this;
        return newBlock;
    }

    /// <summary>
    /// Snaps the given position to the grid.
    /// </summary>
    /// <param name="position">The position to snap to the grid.</param>
    /// <param name="ry">The rotation around the Y-axis.</param>
    /// <returns>The snapped position.</returns>
    public Vector3 SnapPreviewToGrid(Vector3 position, float ry)
    {
        float x = 0;
        float y = 0;
        float z = 0;
        // For standard orientation (0,0,0)
        float offset = 0f;
        if (ActiveSection == EditingSection.MainRoom)
        {
            offset = 1f;
        }
        if (ry == 0f)//good
        {
            x = Mathf.Floor(position.x / cellSize) * cellSize;
            y = (Mathf.Floor(position.y / cellSize) * cellSize) + offset;
            z = Mathf.Floor(position.z / cellSize) * cellSize;
        }
        else if (ry == 90f)
        {
            x = Mathf.Floor(position.x / cellSize) * cellSize;
            y = (Mathf.Floor(position.y / cellSize) * cellSize) + offset;
            z = Mathf.Ceil(position.z / cellSize) * cellSize;
        }
        else if (ry == 180f)
        {
            x = Mathf.Ceil(position.x / cellSize) * cellSize;
            y = (Mathf.Floor(position.y / cellSize) * cellSize) + offset;
            z = Mathf.Ceil(position.z / cellSize) * cellSize;
        }
        else if (ry == 270f)
        {
            x = Mathf.Ceil(position.x / cellSize) * cellSize;
            y = (Mathf.Floor(position.y / cellSize) * cellSize) + offset;
            z = Mathf.Floor(position.z / cellSize) * cellSize;
        }

        return new Vector3(x, y, z);
    }

    public Vector3 SnapTileToGrid(GameObject newObj, Vector3 position, float ry, EditingSection section)
    {
        //Debug.Log(newObj + " at " + ry + " in " + section);
        // For standard orientation (0,0,0) 
        float x = 0;
        float y = 0;
        float z = 0;
        float diff = 0;
        float diff2 = 0;

        // Doors should apply a base offset to allow different types of doors to be used
        if (newObj.name.Contains("Door"))
        {
            diff = newObj.transform.localPosition.x;
            diff2 = newObj.transform.localPosition.z;
        }

        // Replace really only seems to work when active section at time of build is MainRoom
        // Fake MainRoom size no matter the editingSection
        float fakeCellSize = 3f;

        float offset = 0f;
        if (section == EditingSection.MainRoom)
        {
            offset = 1f;
        }
        else if(section == EditingSection.Ceiling)
        {
            offset = roomBottomY + 1;
            if (newObj.name.Contains("Ceiling"))
            {
                offset += 1;
            }
        }
        else if(section == EditingSection.Interior)
        {
            fakeCellSize = 0.5f;
        }

        if (ry == 0f)
        {
            x = Mathf.Floor(position.x / fakeCellSize) * fakeCellSize + diff;
            y = (Mathf.Floor(position.y / fakeCellSize) * fakeCellSize) + offset;
            z = Mathf.Floor(position.z / fakeCellSize) * fakeCellSize + diff2;
        }
        else if (ry == 90f)
        {
            x = Mathf.Floor(position.x / fakeCellSize) * fakeCellSize + diff2;
            y = (Mathf.Floor(position.y / fakeCellSize) * fakeCellSize) + offset;
            z += fakeCellSize;
            z = Mathf.Ceil(position.z / fakeCellSize) * fakeCellSize - diff;
        }
        else if (ry == 180f)
        {
            x = Mathf.Ceil(position.x / fakeCellSize) * fakeCellSize - diff;
            y = (Mathf.Floor(position.y / fakeCellSize) * fakeCellSize) + offset;
            z = Mathf.Ceil(position.z / fakeCellSize) * fakeCellSize - diff2;
        }
        else if (ry == 270f)
        {
            x = Mathf.Ceil(position.x / fakeCellSize) * fakeCellSize - diff2;
            y = (Mathf.Floor(position.y / fakeCellSize) * fakeCellSize) + offset;
            z = Mathf.Floor(position.z / fakeCellSize) * fakeCellSize + diff;
        }

        return new Vector3(x, y, z);
    }

    public static EditingSection DetermineSectionForPlacement(Transform tileHolder)
    {
        EditingSection section = EditingSection.Interior;
        if (tileHolder.name.Equals("Lower"))
        {
            section = EditingSection.FloorBase;
        }
        else if (tileHolder.name.Equals("Upper") || tileHolder.name.Equals("Ceiling"))
        {
            section = EditingSection.Ceiling;
        }
        // replace with .Contains() check
        else if (tileHolder.name.Contains("Wall") || tileHolder.name.Equals("Floor") 
            || tileHolder.name.Equals("Doors") || tileHolder.name.Equals("Stairs"))
        {
            section = EditingSection.MainRoom;
        }
        else
        {
            section = EditingSection.Interior;
        }
        return section;
    }

    /// <summary>
    /// Gets the number of cells in the grid.
    /// </summary>
    /// <returns>A Vector3 representing the number of cells in each dimension of the grid.</returns>
    public Vector3 GetNumberOfCells()
    {
        return new Vector3(
            Mathf.Floor(gridSize.x / cellSize),
            Mathf.Floor(gridSize.y / cellSize),
            Mathf.Floor(gridSize.z / cellSize)
        );
    }
}
#endif