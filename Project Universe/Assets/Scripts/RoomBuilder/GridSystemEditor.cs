using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Transforms;
using Unity.Entities;
using ProjectUniverse.Environment.Gas;
using System.Collections;
using static ProjectUniverse.Environment.Gas.PipeUtilities;

#if (UNITY_EDITOR)
[CustomEditor(typeof(GridSystem))]
public class GridSystemEditor : Editor
{
    private const string PREF_PREFIX = "GridSystemEditor_";
    private const string UnderFloorCube = "UnderFloorCube";
    private const string BaseStructCube = "BaseStructCube";
    private const string FloorCube = "FloorCube";
    private const string WallCube = "WallCube";
    private const string WallStrutCube = "WallStrutCube";
    private const string DoorCube = "DoorCube";
    private const string StairCube = "StairCube";
    private const string OverStruct = "OverStruct";
    private const string DuctCube = "DuctCube";
    private const string PipeCube = "PipeCube";
    private const string LightCube = "LightCube";
    private const string CeilingCube = "CeilingCube";

    private GridSystem gridSystem;
    private Quaternion previewRotation = Quaternion.identity;
    private Dictionary<string, GameObject> blockTypes;
    private string[] blockTypeNames;
    private int selectedBlockTypeIndex = 0;
    private float maincellsize;
    private bool showReplacers = false;
    private int[] tileIndices = new int[12];
    private string[] lfNames, bsNames, flNames, wlNames, wsNames, drNames, stNames, dsNames, ltNames, ceNames, ostrNames;

    private List<GameObject> instantiatedPrefabs = new();
    private List<GameObject> disabledPlaceholders = new();
    private Dictionary<string, GameObject> placeholderToPrefabMap;
    private List<GameObject> instantiatedDucts = new();
    private List<GameObject> disabledDuctPlaceholders = new();

    private float originalRoomBottomY = 0f;
    private float interiorModeOffset = 0f;
    private bool isInInteriorMode = false;
    private bool showPipeDirectionArrows = true;

    private List<GameObject> instantiatedPipes = new();
    private List<GameObject> disabledPipePlaceholders = new();
    private List<GameObject> instantiatedPipeSections = new();
    private List<GameObject> instantiatedDuctSections = new();
    private List<GameObject> instantiatedDuctNodes = new();

    private int pipeTypeIndex = 0;
    private string[] pipeTypeNames;
    private float[] pipeDiameters = { 0.15f, 0.3f, 0.5f, 0.75f, 1.0f }; // in meters
    private string[] pipeDiameterNames = { "150mm", "300mm", "500mm", "750mm", "1000mm" };
    private int selectedPipeDiameter = 2; // Default to 500mm
    private bool placePipeCorners = false;
    private Dictionary<GameObject, int> pipePlaceholderDiameters = new();

    // Add this helper class for serialization
    [Serializable]
    private class IntList
    {
        public int[] values;
    }

    // Class to hold duct path information
    //private class DuctPathInfo
    //{
    //    public List<GameObject> DuctObjects { get; set; } = new List<GameObject>();
    //    public bool IsVertical { get; set; }
    //}

    /// <summary>
    /// Initializes the editor for the GridSystem, setting up block types and tile holders.
    /// </summary>
    /// <summary>
    /// Initializes the editor for the GridSystem, setting up block types and tile holders.
    /// </summary>
    private void OnEnable()
    {
        gridSystem = (GridSystem)target;
        if (gridSystem == null)
        {
            Debug.LogError("GridSystemEditor: gridSystem is null.");
            return;
        }

        // Initialize maincellsize with fallback
        maincellsize = maincellsize <= 0 ? 3f : maincellsize;
        gridSystem.GridSize = new Vector3(gridSystem.GridSize.x, gridSystem.staticSizeY, gridSystem.GridSize.z);

        // Initialize block types
        string[] blockTypeKeys = { UnderFloorCube, BaseStructCube, FloorCube, WallCube, WallStrutCube,
                                  DoorCube, StairCube, OverStruct, DuctCube, PipeCube, LightCube, CeilingCube };
        try
        {
            blockTypes = new Dictionary<string, GameObject>();
            for (int i = 0; i < blockTypeKeys.Length && i < gridSystem.BlockPrefabs.Length; i++)
            {
                blockTypes[blockTypeKeys[i]] = gridSystem.BlockPrefabs[i];
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error initializing block types: " + ex.Message);
        }
        blockTypeNames = blockTypes.Keys.ToArray();

        if (gridSystem.CurrentRoom != null && gridSystem.TileHolders == null)
        {
            IdentifyChildContainers();
            Debug.Log("Rebuild Containers List");
        }

        // Initialize tile names arrays
        try
        {
            var tileGroup = gridSystem.MyTileGroup;
            lfNames = tileGroup.LowerFloors.Select(go => go.name).ToArray();
            bsNames = tileGroup.BaseStructs.Select(go => go.name).ToArray();
            flNames = tileGroup.Floors.Select(go => go.name).ToArray();
            wlNames = tileGroup.Walls.Select(go => go.name).ToArray();
            wsNames = tileGroup.WallStructs.Select(go => go.name).ToArray();
            drNames = tileGroup.Doors.Select(go => go.name).ToArray();
            stNames = tileGroup.Stairs.Select(go => go.name).ToArray();
            dsNames = tileGroup.DuctsMajorType.Select(go => go.name).ToArray();
            ltNames = tileGroup.Lights.Select(go => go.name).ToArray();
            ceNames = tileGroup.Ceilings.Select(go => go.name).ToArray();
            ostrNames = tileGroup.Overstructs.Select(go => go.name).ToArray();
        }
        catch (Exception ex)
        {
            Debug.LogError("Error initializing tile names: " + ex.Message);
        }

        // Load preferences
        string[] prefKeys = { "selectedBlockTypeIndex", "lfIndex", "bsIndex", "flIndex", "wlIndex", "wsIndex",
                         "drIndex", "stIndex", "dsIndex", "ltIndex", "ceIndex", "ostrindex", "pipeTypeIndex", "pipeDiameter" };
        int[] defaultValues = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2 };

        selectedBlockTypeIndex = EditorPrefs.GetInt(PREF_PREFIX + prefKeys[0], defaultValues[0]);
        for (int i = 1; i < 11; i++)
        {
            tileIndices[i - 1] = EditorPrefs.GetInt(PREF_PREFIX + prefKeys[i], defaultValues[i]);
        }
        pipeTypeIndex = EditorPrefs.GetInt(PREF_PREFIX + prefKeys[11], defaultValues[11]);
        selectedPipeDiameter = EditorPrefs.GetInt(PREF_PREFIX + prefKeys[12], defaultValues[12]);

        // Initialize placeholder mapping
        GameObject[] prefabObjects = { gridSystem.LowerFloorGO, gridSystem.BaseStructGO, gridSystem.FloorGO,
                                  gridSystem.WallGO, gridSystem.WallStrutGO, gridSystem.DoorGO, gridSystem.StairGO,
                                  gridSystem.OverStructGO, gridSystem.DuctGO, gridSystem.PipeGO, gridSystem.LightGO, gridSystem.CeilingGO };

        placeholderToPrefabMap = new Dictionary<string, GameObject>();
        for (int i = 0; i < blockTypeKeys.Length && i < prefabObjects.Length; i++)
        {
            placeholderToPrefabMap[blockTypeKeys[i]] = prefabObjects[i];
        }

        RestorePipeReferences();
        RestoreDuctReferences();
    }


    /// <summary>
    /// Customizes the inspector GUI for the GridSystem.
    /// </summary>
    /// <summary>
    /// Customizes the inspector GUI for the GridSystem.
    /// </summary>
    public override void OnInspectorGUI()
    {
        if (gridSystem == null)
        {
            Debug.LogError("GridSystemEditor: gridSystem is null.");
            return;
        }

        DrawDefaultInspector();

        if (blockTypes == null || blockTypes.Count == 0)
        {
            EditorGUILayout.HelpBox("No block types available.", MessageType.Error);
        }
        else
        {
            selectedBlockTypeIndex = EditorGUILayout.Popup("Block Type", selectedBlockTypeIndex, blockTypeNames);
            EditorPrefs.SetInt(PREF_PREFIX + "selectedBlockTypeIndex", selectedBlockTypeIndex);
            if (blockTypes.TryGetValue(blockTypeNames[selectedBlockTypeIndex], out GameObject selectedPrefab))
            {
                gridSystem.blockPrefab = selectedPrefab;
            }
        }

        if (GUILayout.Button("Generate New Room"))
        {
            GenerateNewRoom();
        }

        GUILayout.Space(10f);
        HandleEditingSections();

        if (isInInteriorMode)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Interior Mode Y Adjustment", EditorStyles.boldLabel);

            float newOffset = EditorGUILayout.Slider("Y Offset", interiorModeOffset, -1f, gridSystem.staticSizeY);
            if (newOffset != interiorModeOffset)
            {
                interiorModeOffset = newOffset;
                UpdateInteriorModePosition();
            }

            if (GUILayout.Button("Reset Y Offset"))
            {
                interiorModeOffset = 0f;
                UpdateInteriorModePosition();
            }
        }

        GUILayout.Space(10f);
        HandleRotationButtons();

        showReplacers = EditorGUILayout.Foldout(showReplacers, "Tile Replacers");
        if (showReplacers && Selection.activeTransform && gridSystem.CurrentRoom != null)
        {
            HandleTileReplacers();
        }

        if (GUILayout.Button("Replace Tiles"))
        {
            RevertReplacements();
            ReplaceTiles();
        }

        if (GUILayout.Button("Replace Ducts"))
        {
            RevertDuctReplacements();
            ReplaceDucts();
        }

        showPipeDirectionArrows = EditorGUILayout.Toggle("Show Pipe Direction Arrows", showPipeDirectionArrows);
        placePipeCorners = EditorGUILayout.Toggle("Place Pipe Corners", placePipeCorners);

        if (GUILayout.Button("Replace Pipes"))
        {
            RevertPipeReplacements();
            ReplacePipes();
        }
        EditorGUILayout.Space();
        if (GUILayout.Button("Remove Placeholder Prefabs"))
        {
            RemovePlaceholderPrefabs();
        }

        if (GUILayout.Button("Revert Replacements"))
        {
            RevertReplacements();
        }

        if (GUILayout.Button("Revert Duct Replacements"))
        {
            RevertDuctReplacements();
        }
        if (GUILayout.Button("Revert Pipe Replacements"))
        {
            RevertPipeReplacements();
        }
    }

    private void GenerateNewRoom()
    {
        GameObject goBase = Instantiate(gridSystem.RoomTemplate, gridSystem.transform.position, Quaternion.identity);
        gridSystem.CurrentRoom = goBase;
        gridSystem.roomBottomY = gridSystem.transform.position.y;
        originalRoomBottomY = gridSystem.roomBottomY;
        IdentifyChildContainers();
    }

    private void HandleEditingSections()
    {
        if (gridSystem.ActiveSection == GridSystem.EditingSection.MainRoom)
        {
            //maincellsize = gridSystem.gridSize.y;
            if (gridSystem.GridSize.y > 0)
            {
                maincellsize = 3f;
            }
        }

        if (GUILayout.Button("Edit Floor Base"))
        {
            ExitInteriorMode();
            SetGridSettings(1f, 1f, false, GridSystem.EditingSection.FloorBase);
            gridSystem.transform.position = new Vector3(gridSystem.transform.position.x,
                gridSystem.roomBottomY,
                gridSystem.transform.position.z);
        }

        if (GUILayout.Button("Edit Main Room"))
        {
            ExitInteriorMode();
            // Ensure maincellsize is valid before using it
            if (maincellsize <= 0)
            {
                maincellsize = 3f; // Default fallback
            }
            SetGridSettings(3f, gridSystem.staticSizeY, true, GridSystem.EditingSection.MainRoom);
            gridSystem.transform.position = new Vector3(gridSystem.transform.position.x,
                gridSystem.roomBottomY + 1,
                gridSystem.transform.position.z);
        }

        if (GUILayout.Button("Edit Ceiling"))
        {
            ExitInteriorMode();
            gridSystem.transform.position = new Vector3(gridSystem.transform.position.x,
                gridSystem.roomBottomY + 1 + gridSystem.GridSize.y,
                gridSystem.transform.position.z);
            SetGridSettings(1f, 1f, false, GridSystem.EditingSection.Ceiling);
        }

        if (GUILayout.Button("Edit Interior"))
        {
            EnterInteriorMode();
            // Ensure maincellsize is valid before using it
            if (maincellsize <= 0)
            {
                maincellsize = 3f;
            }
            SetGridSettings(0.5f, gridSystem.staticSizeY, false, GridSystem.EditingSection.Interior);
            gridSystem.transform.position = new Vector3(gridSystem.transform.position.x,
                gridSystem.roomBottomY + 1,
                gridSystem.transform.position.z);
        }
    }

    /// <summary>
    /// Enters interior mode and saves the original room bottom Y
    /// </summary>
    private void EnterInteriorMode()
    {
        if (!isInInteriorMode)
        {
            isInInteriorMode = true;
            originalRoomBottomY = gridSystem.roomBottomY;
            interiorModeOffset = 0f;
        }
    }

    /// <summary>
    /// Exits interior mode and restores the original room bottom Y
    /// </summary>
    private void ExitInteriorMode()
    {
        if (isInInteriorMode)
        {
            isInInteriorMode = false;
            gridSystem.roomBottomY = originalRoomBottomY;
            interiorModeOffset = 0f;
        }
    }

    private void OnDisable()
    {
        ExitInteriorMode();
        // Serialize pipe references to EditorPrefs if needed
        SerializePipeReferences();
        SerializeDuctReferences();
    }
    // Add this method to serialize pipe references
    private void SerializePipeReferences()
    {
        SerializeGameObjectList(instantiatedPipes, "pipeInstanceIDs");
        SerializeGameObjectList(disabledPipePlaceholders, "placeholderInstanceIDs");
        SerializeGameObjectList(instantiatedPipeSections, "sectionInstanceIDs");
    }

    private void SerializeGameObjectList(List<GameObject> gameObjects, string key)
    {
        List<int> instanceIDs = gameObjects.Where(go => go != null).Select(go => go.GetInstanceID()).ToList();
        EditorPrefs.SetString(PREF_PREFIX + key, JsonUtility.ToJson(new IntList { values = instanceIDs.ToArray() }));
    }

    private void SerializeDuctReferences()
    {
        SerializeGameObjectList(instantiatedDucts, "ductInstanceIDs");
        SerializeGameObjectList(disabledDuctPlaceholders, "ductPlaceholderInstanceIDs");
        SerializeGameObjectList(instantiatedDuctSections, "ductSectionInstanceIDs");
        SerializeGameObjectList(instantiatedDuctNodes, "ductNodeInstanceIDs");
    }
    private void RestoreDuctReferences()
    {
        instantiatedDucts = RestoreGameObjectList("ductInstanceIDs");
        disabledDuctPlaceholders = RestoreGameObjectList("ductPlaceholderInstanceIDs");
        instantiatedDuctSections = RestoreGameObjectList("ductSectionInstanceIDs");
        instantiatedDuctNodes = RestoreGameObjectList("ductNodeInstanceIDs");
    }

    // Add this to OnEnable to restore references
    private void RestorePipeReferences()
    {
        instantiatedPipes = RestoreGameObjectList("pipeInstanceIDs");
        disabledPipePlaceholders = RestoreGameObjectList("placeholderInstanceIDs");
        instantiatedPipeSections = RestoreGameObjectList("sectionInstanceIDs");
    }
    private List<GameObject> RestoreGameObjectList(string key)
    {
        string json = EditorPrefs.GetString(PREF_PREFIX + key, "");
        if (string.IsNullOrEmpty(json)) return new List<GameObject>();

        IntList ids = JsonUtility.FromJson<IntList>(json);
        return ids.values.Select(id => EditorUtility.InstanceIDToObject(id) as GameObject)
                        .Where(go => go != null).ToList();
    }

    private void UpdateInteriorModePosition()
    {
        if (isInInteriorMode)
        {
            float newY = originalRoomBottomY + 1 + interiorModeOffset;
            // Ensure roomBottomY doesn't go below -1
            if (newY - 1 < -1)
            {
                newY = 0;
                interiorModeOffset = -originalRoomBottomY - 1;
            }

            gridSystem.roomBottomY = newY - 1; // Adjust roomBottomY to match the new position
            gridSystem.transform.position = new Vector3(
                gridSystem.transform.position.x,
                newY,
                gridSystem.transform.position.z
            );
        }
    }

    private void HandleRotationButtons()
    {
        if (GUILayout.Button("Rotate 90° X"))
        {
            previewRotation *= Quaternion.Euler(90, 0, 0);
        }

        if (GUILayout.Button("Rotate 90° Y"))
        {
            previewRotation *= Quaternion.Euler(0, 90, 0);
        }

        if (GUILayout.Button("Rotate 90° Z"))
        {
            previewRotation *= Quaternion.Euler(0, 0, 90);
        }

        if (GUILayout.Button("Reset Rotation"))
        {
            previewRotation = Quaternion.identity;
        }
    }

    private void HandleTileReplacers()
    {
        if (tileIndices[0] < gridSystem.MyTileGroup.LowerFloors.Length)
        {
            tileIndices[0] = EditorGUILayout.Popup("Lower Floor Type", tileIndices[0], lfNames);
            EditorPrefs.SetInt(PREF_PREFIX + "lfIndex", tileIndices[0]);
            gridSystem.LowerFloorGO = gridSystem.MyTileGroup.LowerFloors[tileIndices[0]];
        }
        else
        {
            Debug.LogError("Invalid Lower Floor index: " + tileIndices[0]);
        }

        if (tileIndices[1] < gridSystem.MyTileGroup.BaseStructs.Length)
        {
            tileIndices[1] = EditorGUILayout.Popup("Base Struct Type", tileIndices[1], bsNames);
            EditorPrefs.SetInt(PREF_PREFIX + "bsIndex", tileIndices[1]);
            gridSystem.BaseStructGO = gridSystem.MyTileGroup.BaseStructs[tileIndices[1]];
        }
        else
        {
            Debug.LogError("Invalid Base Struct index: " + tileIndices[1]);
        }

        if (tileIndices[2] < gridSystem.MyTileGroup.Floors.Length)
        {
            tileIndices[2] = EditorGUILayout.Popup("Floor Type", tileIndices[2], flNames);
            EditorPrefs.SetInt(PREF_PREFIX + "flIndex", tileIndices[2]);
            gridSystem.FloorGO = gridSystem.MyTileGroup.Floors[tileIndices[2]];
        }
        else
        {
            Debug.LogError("Invalid Floor index: " + tileIndices[2]);
        }

        if (tileIndices[3] < gridSystem.MyTileGroup.Walls.Length)
        {
            tileIndices[3] = EditorGUILayout.Popup("Wall Type", tileIndices[3], wlNames);
            EditorPrefs.SetInt(PREF_PREFIX + "wlIndex", tileIndices[3]);
            gridSystem.WallGO = gridSystem.MyTileGroup.Walls[tileIndices[3]];
        }
        else
        {
            Debug.LogError("Invalid Wall index: " + tileIndices[3]);
        }

        if (tileIndices[4] < gridSystem.MyTileGroup.WallStructs.Length)
        {
            tileIndices[4] = EditorGUILayout.Popup("Wall Struct Type", tileIndices[4], wsNames);
            EditorPrefs.SetInt(PREF_PREFIX + "wsIndex", tileIndices[4]);
            gridSystem.WallStrutGO = gridSystem.MyTileGroup.WallStructs[tileIndices[4]];
        }
        else
        {
            Debug.LogError("Invalid Wall Struct index: " + tileIndices[4]);
        }

        if (tileIndices[5] < gridSystem.MyTileGroup.Doors.Length)
        {
            tileIndices[5] = EditorGUILayout.Popup("Door Type", tileIndices[5], drNames);
            EditorPrefs.SetInt(PREF_PREFIX + "drIndex", tileIndices[5]);
            gridSystem.DoorGO = gridSystem.MyTileGroup.Doors[tileIndices[5]];
        }
        else
        {
            Debug.LogError("Invalid Door index: " + tileIndices[5]);
        }

        if (tileIndices[6] < gridSystem.MyTileGroup.Stairs.Length)
        {
            tileIndices[6] = EditorGUILayout.Popup("Stair Type", tileIndices[6], stNames);
            EditorPrefs.SetInt(PREF_PREFIX + "stIndex", tileIndices[6]);
            gridSystem.StairGO = gridSystem.MyTileGroup.Stairs[tileIndices[6]];
        }
        else
        {
            Debug.LogError("Invalid Stair index: " + tileIndices[6]);
        }

        if (tileIndices[7] < gridSystem.MyTileGroup.DuctsMajorType.Length)
        {
            tileIndices[7] = EditorGUILayout.Popup("Duct Type", tileIndices[7], dsNames);
            EditorPrefs.SetInt(PREF_PREFIX + "dsIndex", tileIndices[7]);
            gridSystem.DuctGO = gridSystem.MyTileGroup.DuctsMajorType[tileIndices[7]];
        }
        else
        {
            Debug.LogError("Invalid Duct index: " + tileIndices[7]);
        }

        if (tileIndices[8] < gridSystem.MyTileGroup.Lights.Length)
        {
            tileIndices[8] = EditorGUILayout.Popup("Light Type", tileIndices[8], ltNames);
            EditorPrefs.SetInt(PREF_PREFIX + "ltIndex", tileIndices[8]);
            gridSystem.LightGO = gridSystem.MyTileGroup.Lights[tileIndices[8]];
        }
        else
        {
            Debug.LogError("Invalid Light index: " + tileIndices[8]);
        }

        if (tileIndices[9] < gridSystem.MyTileGroup.Ceilings.Length)
        {
            tileIndices[9] = EditorGUILayout.Popup("Ceiling Type", tileIndices[9], ceNames);
            EditorPrefs.SetInt(PREF_PREFIX + "ceIndex", tileIndices[9]);
            gridSystem.CeilingGO = gridSystem.MyTileGroup.Ceilings[tileIndices[9]];
        }
        else
        {
            Debug.LogError("Invalid Ceiling index: " + tileIndices[9]);
        }

        if (tileIndices[10] < gridSystem.MyTileGroup.Overstructs.Length)
        {
            tileIndices[10] = EditorGUILayout.Popup("Ceiling Struct Type", tileIndices[10], ostrNames);
            EditorPrefs.SetInt(PREF_PREFIX + "ostrindex", tileIndices[10]);
            gridSystem.OverStructGO = gridSystem.MyTileGroup.Overstructs[tileIndices[10]];
        }
        else
        {
            Debug.LogError("Invalid Over Struct index: " + tileIndices[10]);
        }
        // Add pipe type and diameter selection at the end
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Pipe Settings", EditorStyles.boldLabel);

        if (pipeTypeNames != null && pipeTypeNames.Length > 0)
        {
            pipeTypeIndex = EditorGUILayout.Popup("Pipe Type", pipeTypeIndex, pipeTypeNames);
            EditorPrefs.SetInt(PREF_PREFIX + "pipeTypeIndex", pipeTypeIndex);
            gridSystem.PipeGO = gridSystem.MyTileGroup.PipesMajorType[pipeTypeIndex];
        }

        selectedPipeDiameter = EditorGUILayout.Popup("Pipe Diameter", selectedPipeDiameter, pipeDiameterNames);
        EditorPrefs.SetInt(PREF_PREFIX + "pipeDiameter", selectedPipeDiameter);

        //if (GUILayout.Button("Update All Pipe Sizes To Selected"))
        //{
        //    UpdatePipePlaceholderSizes();
        //}
    }

    private void UpdatePipePlaceholderVisualScale(GameObject pipePlaceholder, float diameter)
    {
        Vector3 newScale;// = pipePlaceholder.transform.localScale;

        if (placePipeCorners)
        {
            newScale = Vector3.one * diameter;
        }
        else
        {
            float rotationZ = pipePlaceholder.transform.localRotation.eulerAngles.z;
            bool isVertical = rotationZ == 90f || rotationZ == 270f;

            if (isVertical)
            {
                newScale = new Vector3(1f, diameter, diameter);
            }
            else
            {
                newScale = new Vector3(1f, diameter, diameter);
            }
        }
        pipePlaceholder.transform.localScale = newScale;
    }

    /// <summary>
    /// Replaces duct placeholders with appropriate duct types based on connections 
    /// </summary>
    private void ReplaceDucts()
    {
        if (instantiatedDuctSections.Count > 0) RevertDuctSections();
        if (gridSystem.CurrentRoom == null)
        {
            Debug.LogError("CurrentRoom is not assigned.");
            return;
        }

        // Find all duct placeholders
        var ductPlaceholders = FindDuctPlaceholders();
        if (ductPlaceholders.Count == 0) return;

        // Snap ducts to connection points
        var gasPipeLinks = GameObject.FindGameObjectsWithTag("_GasPipeLink");
        SnapDuctsToConnectionPoints(ductPlaceholders, gasPipeLinks);

        // Process each placeholder and create ducts
        var ductPaths = new List<DuctUtilities.DuctPathInfo>();

        foreach (var placeholder in ductPlaceholders.Where(p => p.activeSelf))
        {
            var newDuct = DetermineDuctTypeAndReplace(placeholder, ductPlaceholders);
            if (newDuct != null)
            {
                ductPaths.Add(new DuctUtilities.DuctPathInfo { DuctObjects = new List<GameObject> { newDuct } });
            }
        }

        // Group instantiated ducts into connected segments
        var connectedDuctGroups = DuctUtilities.GroupDuctsIntoSegments(instantiatedDucts);

        // Create sections based on connected groups
        ductPaths.Clear();
        foreach (var group in connectedDuctGroups)
        {
            ductPaths.Add(new DuctUtilities.DuctPathInfo { DuctObjects = group });
        }

        CreateDuctSections(ductPaths);
    }
    private List<GameObject> FindDuctPlaceholders()
    {
        var ductPlaceholders = new List<GameObject>();
        var roomTransform = gridSystem.CurrentRoom.transform;

        foreach (Transform tileHolder in roomTransform)
        {
            if (tileHolder.name.Equals("OverheadDucts") || tileHolder.name.Equals("Pipes"))
            {
                ductPlaceholders.AddRange(
                    tileHolder.Cast<Transform>()
                             .Where(duct => GetPrefabName(duct.gameObject) == DuctCube)
                             .Select(duct => duct.gameObject)
                );
            }
        }

        return ductPlaceholders;
    }
    private void SnapDuctsToConnectionPoints(List<GameObject> ductPlaceholders, GameObject[] gasPipeLinks)
    {
        foreach (var duct in ductPlaceholders)
        {
            var nearestPoint = DuctUtilities.GetNearestConnectionPoint(duct.transform.position, gasPipeLinks);
            if (nearestPoint != Vector3.zero)
            {
                duct.transform.position = nearestPoint;
            }
        }
    }
    private void PlaceNeighborDuctNode(GameObject ductObject, IGasPipe gasPipe)
    {
        // Load the NeighborDuctNode prefab
        GameObject nodePrefab = Resources.Load<GameObject>("Prefabs/Volumes/NeighborDuctNode 1");
        if (nodePrefab == null)
        {
            Debug.LogError("Failed to load NeighborDuctNode 1 prefab");
            return;
        }

        // Create the node instance
        GameObject nodeInstance = Instantiate(nodePrefab);
        nodeInstance.name = $"NeighborDuctNode_{instantiatedDuctNodes.Count + 1}";

        // Position the node at the duct with offset
        Vector3 offset = new Vector3(0, 0.26f, 0);
        nodeInstance.transform.position = ductObject.transform.position + offset;
        nodeInstance.transform.rotation = ductObject.transform.rotation;
        nodeInstance.transform.SetParent(gridSystem.CurrentRoom.transform);

        // Get and configure the IGasPipeLinker component
        if (nodeInstance.TryGetComponent<IGasPipeLinker>(out var linker))
        {
            linker.SetParentDuct(gasPipe);
        }
        else
        {
            Debug.LogError("IGasPipeLinker component not found on NeighborDuctNode prefab");
        }

        // Add to our tracking list
        instantiatedDuctNodes.Add(nodeInstance);
        Undo.RegisterCreatedObjectUndo(nodeInstance, "Create Neighbor Duct Node");
    }


    private void RevertDuctSections()
    {
        foreach (var section in instantiatedDuctSections.Where(s => s != null))
        {
            Undo.DestroyObjectImmediate(section);
        }
        instantiatedDuctSections.Clear();
    }
    private void RemoveDuctNodes()
    {
        foreach (var node in instantiatedDuctNodes.Where(n => n != null))
        {
            Undo.DestroyObjectImmediate(node);
        }
        instantiatedDuctNodes.Clear();
    }
    private void RevertDuctReplacements()
    {
        Debug.Log($"Starting duct revert. Instantiated ducts: {instantiatedDucts.Count}, Disabled placeholders: {disabledDuctPlaceholders.Count}");
        RevertDuctSections();
        RemoveDuctNodes();

        // Destroy instantiated ducts
        foreach (var duct in instantiatedDucts.Where(d => d != null))
        {
            Undo.DestroyObjectImmediate(duct);
        }
        instantiatedDucts.Clear();

        // Reactivate placeholders
        foreach (var placeholder in disabledDuctPlaceholders.Where(p => p != null))
        {
            placeholder.SetActive(true);
            Undo.RegisterFullObjectHierarchyUndo(placeholder, "Reactivate Duct Placeholder");
        }
        disabledDuctPlaceholders.Clear();
    }

    private GameObject DetermineDuctTypeAndReplace(GameObject placeholder, List<GameObject> allDucts)
    {
        placeholder.SetActive(false);
        disabledDuctPlaceholders.Add(placeholder);

        // Get connections in each direction
        bool[] connections = new bool[6];
        for (int i = 0; i < 6; i++)
        {
            connections[i] = DuctUtilities.HasDuctInDirection(placeholder.transform.position,
                                                           DuctUtilities.Directions[i], allDucts);
        }

        int ductTypeIndex = DuctUtilities.GetDuctTypeIndex(connections);
        float rotationY = DuctUtilities.GetDuctRotation(connections, ductTypeIndex);

        // Instantiate the appropriate duct
        if (ductTypeIndex >= 0 && ductTypeIndex < gridSystem.MyTileGroup.DuctsMajorType.Length)
        {
            GameObject ductPrefab = gridSystem.MyTileGroup.DuctsMajorType[ductTypeIndex];
            GameObject newDuct = Instantiate(ductPrefab, placeholder.transform.parent);
            newDuct.transform.SetPositionAndRotation(placeholder.transform.position, Quaternion.Euler(0, rotationY, 0));

            instantiatedDucts.Add(newDuct);
            Undo.RegisterCreatedObjectUndo(newDuct, "Replace Duct");
            return newDuct;
        }

        Debug.LogWarning($"Failed to create duct for placeholder at {placeholder.transform.position}");
        return null;
    }

    private void CreateDuctSections(List<DuctUtilities.DuctPathInfo> ductPaths)
    {
        // Clean up existing duct nodes first
        RemoveDuctNodes();

        for (int i = 0; i < ductPaths.Count; i++)
        {
            var pathInfo = ductPaths[i];
            if (pathInfo.DuctObjects.Count == 0) continue;

            // Create section GameObject
            GameObject sectionGO = new($"DuctSection_{i + 1}");
            sectionGO.transform.SetParent(gridSystem.CurrentRoom.transform);
            instantiatedDuctSections.Add(sectionGO);

            // Add PipeSection component
            PipeSection ductSection = sectionGO.AddComponent<PipeSection>();
            ductSection.GasPipesInSection = new List<IGasPipe>();
            ductSection.GasPipe = true;

            // Standard duct dimensions
            const float width = 0.6f;  // 600mm
            const float height = 0.4f; // 400mm
            const float hydraulicDiameter = 4 * (width * height) / (2 * (width + height));

            // Add IGasPipe components to each duct
            foreach (var ductObject in pathInfo.DuctObjects)
            {
                if (!ductObject.TryGetComponent(out IGasPipe gasPipe))
                {
                    gasPipe = ductObject.AddComponent<IGasPipe>();
                }

                float length = DuctUtilities.GetDuctLength(ductObject);
                float volume = width * height * length;

                gasPipe.InnerDiameter = hydraulicDiameter;
                gasPipe.Volume = volume;

                ductSection.GasPipesInSection.Add(gasPipe);

                
            }
            // Place neighbor duct nodes at first and last ducts (endpoints)
            if (pathInfo.DuctObjects.Count > 0)
            {
                var firstDuct = pathInfo.DuctObjects[0];
                var lastDuct = pathInfo.DuctObjects[^1];

                // Get the IGasPipe components for the endpoints
                firstDuct.TryGetComponent(out IGasPipe firstGasPipe);
                lastDuct.TryGetComponent(out IGasPipe lastGasPipe);

                PlaceNeighborDuctNode(firstDuct, firstGasPipe);

                // Only place second node if it's a different duct (avoid duplicate for single duct)
                if (firstDuct != lastDuct)
                {
                    PlaceNeighborDuctNode(lastDuct, lastGasPipe);
                }
            }
        }

        Debug.Log($"Created {ductPaths.Count} duct sections");
    }


    /// <summary>
    /// Replaces the tiles in the grid with the selected prefabs.
    /// </summary>
    private void ReplaceTiles()
    {
        if (gridSystem.CurrentRoom == null)
        {
            Debug.LogError("CurrentRoom is not assigned.");
            return;
        }

        Transform roomTransform = gridSystem.CurrentRoom.transform;

        foreach (Transform tileHolder in roomTransform)
        {
            GridSystem.EditingSection section = GridSystem.EditingSection.Interior;
            if (tileHolder.name.Equals("Pipes") || tileHolder.name.Equals("OverheadDucts"))
            {
                continue;
            }
            section = GridSystem.DetermineSectionForPlacement(tileHolder);

            foreach (Transform tile in tileHolder)
            {
                ReplaceTile(tile, section);
            }
        }
    }

    private void CreatePipeSections(List<PipeUtilities.PipePathInfo> pipePaths)
    {
        if (pipePaths == null || pipePaths.Count == 0) return;

        for (int i = 0; i < pipePaths.Count; i++)
        {
            var pathInfo = pipePaths[i];
            if (pathInfo.PipeObjects.Count == 0) continue;

            // Create section GameObject
            GameObject sectionGO = new($"PipeSection_{i + 1}");
            sectionGO.transform.SetParent(gridSystem.CurrentRoom.transform);
            instantiatedPipeSections.Add(sectionGO);

            // Add PipeSection component
            PipeSection pipeSection = sectionGO.AddComponent<PipeSection>();
            pipeSection.GasPipesInSection = new List<IGasPipe>(pathInfo.PipeObjects.Count);

            float diameter = pathInfo.Diameter;
            float totalVolume = 0f;

            // Process all pipes in batch
            foreach (GameObject pipeObject in pathInfo.PipeObjects)
            {
                if (!pipeObject.TryGetComponent(out IGasPipe gasPipe))
                {
                    gasPipe = pipeObject.AddComponent<IGasPipe>();
                }

                float length = PipeUtilities.GetPipeLength(pipeObject);
                float volume = Mathf.PI * (diameter * 0.5f) * (diameter * 0.5f) * length;

                gasPipe.InnerDiameter = diameter;
                gasPipe.Volume = volume;
                totalVolume += volume;

                pipeSection.GasPipesInSection.Add(gasPipe);
            }
        }
    }

    private void ReplacePipes()
    {
        // Early exit checks
        if (gridSystem.CurrentRoom == null)
        {
            Debug.LogError("CurrentRoom is not assigned.");
            return;
        }

        RevertPipeSections(); // Clean up existing pipe sections

        // Find all pipe placeholders in one pass
        List<GameObject> pipePlaceholders = FindAllPipePlaceholders();
        if (pipePlaceholders.Count == 0) return;

        // Group pipes by diameter using a single pass
        var pipeSegmentsByDiameter = GroupPipesByDiameter(pipePlaceholders);

        // Track placeholders that are part of segments
        HashSet<GameObject> usedPlaceholders = new(pipePlaceholders.Count);
        List<PipeUtilities.PipePathInfo> pipePaths = new();

        // Replace each segment with appropriate diameter
        foreach (var kvp in pipeSegmentsByDiameter)
        {
            int diameterIndex = kvp.Key;
            var segments = kvp.Value;

            foreach (var segment in segments)
            {
                if (segment.Count == 0) continue;

                PipeUtilities.PipePathInfo pathInfo = ReplacePipeSegmentWithDiameter(segment, diameterIndex);
                if (pathInfo != null)
                {
                    pipePaths.Add(pathInfo);
                    foreach (var pipe in segment) usedPlaceholders.Add(pipe);
                }
            }
        }

        // Reactivate any placeholders not used in segments
        foreach (GameObject placeholder in pipePlaceholders)
        {
            if (!usedPlaceholders.Contains(placeholder) && disabledPipePlaceholders.Contains(placeholder))
            {
                placeholder.SetActive(true);
                disabledPipePlaceholders.Remove(placeholder);
            }
        }

        CreatePipeSections(pipePaths);
    }
    // Helper method to group pipes by diameter
    private Dictionary<int, List<List<GameObject>>> GroupPipesByDiameter(List<GameObject> pipePlaceholders)
    {
        Dictionary<int, List<GameObject>> pipesByDiameter = new();

        // Group pipes by diameter in a single pass
        foreach (GameObject pipe in pipePlaceholders)
        {
            int diameterIndex = GetPipeDiameterIndex(pipe);

            if (!pipesByDiameter.TryGetValue(diameterIndex, out var pipes))
            {
                pipes = new List<GameObject>();
                pipesByDiameter[diameterIndex] = pipes;
            }

            pipes.Add(pipe);
        }

        // Convert to segments
        Dictionary<int, List<List<GameObject>>> result = new();

        foreach (var kvp in pipesByDiameter)
        {
            result[kvp.Key] = PipeUtilities.GroupPipesIntoSegments(kvp.Value);
        }

        return result;
    }
    // Helper method to find all pipe placeholders
    private List<GameObject> FindAllPipePlaceholders()
    {
        List<GameObject> placeholders = new();
        Transform pipesHolder = gridSystem.CurrentRoom.transform.Find("Pipes");

        if (pipesHolder != null)
        {
            foreach (Transform pipe in pipesHolder)
            {
                if (GetPrefabName(pipe.gameObject) == PipeCube)
                {
                    placeholders.Add(pipe.gameObject);
                }
            }
        }

        return placeholders;
    }

    private int GetPipeDiameterIndex(GameObject pipePlaceholder)
    {
        if (pipePlaceholderDiameters.ContainsKey(pipePlaceholder))
        {
            return pipePlaceholderDiameters[pipePlaceholder];
        }

        // Check if it's a corner (all scales equal)
        Vector3 scale = pipePlaceholder.transform.localScale;
        bool isCorner = Mathf.Abs(scale.x - scale.y) < 0.01f &&
                        Mathf.Abs(scale.y - scale.z) < 0.01f;

        // Try to determine from scale
        float checkScale = isCorner ? scale.x : Mathf.Max(scale.y, scale.z);

        for (int i = 0; i < pipeDiameters.Length; i++)
        {
            if (Mathf.Abs(checkScale - pipeDiameters[i]) < 0.05f)
            {
                return i;
            }
        }

        return 2; // Default to 500mm
    }

    // Consolidated pipe replacement logic
    private PipeUtilities.PipePathInfo ReplacePipeSegmentWithDiameter(List<GameObject> segment, int diameterIndex)
    {
        if (segment.Count == 0) return null;

        PipeUtilities.PipePathInfo pathInfo = new() { Diameter = pipeDiameters[diameterIndex] };

        // Disable all placeholders at once
        foreach (GameObject pipe in segment)
        {
            pipe.SetActive(false);
            disabledPipePlaceholders.Add(pipe);
        }

        // Single pipe case
        if (segment.Count == 1)
        {
            GameObject pipePrefab = gridSystem.MyTileGroup.PipesMajorType[diameterIndex * 3]; // 1m pipe
            GameObject newPipe = Instantiate(pipePrefab, segment[0].transform.parent);
            newPipe.transform.SetPositionAndRotation(segment[0].transform.position, segment[0].transform.rotation);
            instantiatedPipes.Add(newPipe);
            pathInfo.PipeObjects.Add(newPipe);
            Undo.RegisterCreatedObjectUndo(newPipe, "Replace Single Pipe");
            return pathInfo;
        }

        // Analyze segment topology
        var (endPoints, corners) = AnalyzePipeSegment(segment);

        // Choose replacement strategy based on segment type
        pathInfo.IsVertical = PipeUtilities.IsVerticalPipeSegment(segment);

        if (pathInfo.IsVertical)
        {
            pathInfo.PipeObjects = ReplaceVerticalPipeSegmentWithDiameter(segment, diameterIndex);
        }
        else if (corners.Count == 0)
        {
            GameObject startPipe = endPoints.Count > 0 ? endPoints[0] : segment[0];
            List<GameObject> orderedSegment = PipeUtilities.OrderPipeSegment(segment, startPipe);
            pathInfo.PipeObjects = ReplaceStraightPipeSegmentWithDiameter(orderedSegment, diameterIndex);
        }
        else
        {
            pathInfo.PipeObjects = ReplaceComplexPipeSegmentWithDiameter(segment, endPoints, corners, diameterIndex);
        }

        return pathInfo;
    }
    // Helper method to analyze pipe segment topology
    private (List<GameObject>, List<GameObject>) AnalyzePipeSegment(List<GameObject> segment)
    {
        List<GameObject> endPoints = new();
        List<GameObject> corners = new();

        foreach (GameObject pipe in segment)
        {
            int connectionCount = PipeUtilities.GetPipeConnectionCount(pipe, segment);

            if (connectionCount == 1)
            {
                endPoints.Add(pipe);
            }
            else if (connectionCount == 2 && PipeUtilities.IsPipeCorner(pipe, segment))
            {
                corners.Add(pipe);
            }
        }

        // Ensure we have at least one endpoint for path building
        if (endPoints.Count == 0 && segment.Count > 0)
        {
            endPoints.Add(segment[0]);
        }

        return (endPoints, corners);
    }
   
    private List<GameObject> ReplaceStraightPipeSegmentWithDiameter(List<GameObject> orderedSegment, int diameterIndex)
    {
        if (orderedSegment.Count < 2) return new List<GameObject>();

        List<GameObject> instantiatedPipesList = new();

        foreach (GameObject pipe in orderedSegment)
        {
            pipe.SetActive(false);
            disabledPipePlaceholders.Add(pipe);
        }

        GameObject start = orderedSegment[0];
        GameObject end = orderedSegment[^1];

        Vector3 overallDirection = (end.transform.position - start.transform.position).normalized;
        float rotation = PipeUtilities.GetPipeRotation(overallDirection);

        // Apply offset to start position based on placeholder extents
        Vector3 startOffset = -1f * PipeUtilities.GetPipeOffsetForPlaceholder(start, overallDirection);
        Vector3 endOffset = -1f * PipeUtilities.GetPipeOffsetForPlaceholder(end, -overallDirection);

        Vector3 adjustedStartPosition = start.transform.position + startOffset;
        Vector3 adjustedEndPosition = end.transform.position + endOffset;

        Vector3 currentPosition = adjustedStartPosition;
        float totalDistance = Vector3.Distance(adjustedStartPosition, adjustedEndPosition);
        float remainingDistance = totalDistance;

        int baseIndex = diameterIndex * 3;

        while (remainingDistance > 0.1f)
        {
            GameObject pipePrefab;
            float pipeLength;

            if (remainingDistance >= 3f && gridSystem.MyTileGroup.PipesMajorType.Length > baseIndex + 1)
            {
                pipePrefab = gridSystem.MyTileGroup.PipesMajorType[baseIndex + 1]; // 3m pipe
                pipeLength = 3f;
            }
            else
            {
                pipePrefab = gridSystem.MyTileGroup.PipesMajorType[baseIndex]; // 1m pipe
                pipeLength = 1f;
            }

            GameObject newPipe = Instantiate(pipePrefab, orderedSegment[0].transform.parent);
            newPipe.transform.SetPositionAndRotation(currentPosition, Quaternion.Euler(0, rotation, 0));
            instantiatedPipes.Add(newPipe);
            instantiatedPipesList.Add(newPipe);
            Undo.RegisterCreatedObjectUndo(newPipe, "Replace Pipe");

            currentPosition += overallDirection * pipeLength;
            remainingDistance -= pipeLength;
        }

        return instantiatedPipesList;
    }


    private List<GameObject> ReplaceComplexPipeSegmentWithDiameter(List<GameObject> segment, List<GameObject> endPoints,
    List<GameObject> corners, int diameterIndex)
    {
        List<GameObject> instantiatedPipesList = new();

        foreach (GameObject pipe in segment)
        {
            pipe.SetActive(false);
            disabledPipePlaceholders.Add(pipe);
        }

        List<GameObject> path = PipeUtilities.BuildPipePath(segment, endPoints, corners);

        for (int i = 0; i < path.Count; i++)
        {
            GameObject current = path[i];

            if (corners.Contains(current))
            {
                GameObject cornerPipe = PlaceCornerPipeWithDiameter(current, path, i, diameterIndex);
                if (cornerPipe != null)
                {
                    instantiatedPipesList.Add(cornerPipe);
                }
            }
            else
            {
                // For straight sections, we need to check if we should place pipes
                // This could be either to the next pipe (corner or straight) or as a standalone
                if (i < path.Count - 1)
                {
                    GameObject next = path[i + 1];
                    Vector3 direction = (next.transform.position - current.transform.position).normalized;
                    float distance = Vector3.Distance(current.transform.position, next.transform.position);

                    // Place straight pipes if there's enough distance
                    // Don't skip just because the next one is a corner
                    if (distance > 0.5f)
                    {
                        // Adjust the distance if the next is a corner (corners occupy some space)
                        float adjustedDistance = corners.Contains(next) ? distance - 0.5f : distance;

                        if (adjustedDistance > 0.1f)
                        {
                            List<GameObject> straightPipes = PlaceStraightPipesWithDiameter(
                                current.transform.position,
                                direction,
                                adjustedDistance,
                                diameterIndex);
                            instantiatedPipesList.AddRange(straightPipes);
                        }
                    }
                }
            }
        }

        return instantiatedPipesList;
    }

    private List<GameObject> ReplaceVerticalPipeSegmentWithDiameter(List<GameObject> segment, int diameterIndex)
    {
        if (segment.Count < 2) return new List<GameObject>();

        List<GameObject> instantiatedPipesList = new();

        // Sort pipes by Y position
        List<GameObject> orderedSegment = segment.OrderBy(p => p.transform.position.y).ToList();

        foreach (GameObject pipe in orderedSegment)
        {
            pipe.SetActive(false);
            disabledPipePlaceholders.Add(pipe);
        }

        GameObject start = orderedSegment[0];
        GameObject end = orderedSegment[^1];

        // Apply vertical offsets
        Vector3 startOffset = -1f * PipeUtilities.GetPipeOffsetForPlaceholder(start, Vector3.up);
        Vector3 endOffset = -1f * PipeUtilities.GetPipeOffsetForPlaceholder(end, Vector3.down);

        Vector3 adjustedStartPosition = start.transform.position + startOffset;
        Vector3 adjustedEndPosition = end.transform.position + endOffset;

        Vector3 currentPosition = adjustedStartPosition;
        float totalDistance = Mathf.Abs(adjustedEndPosition.y - adjustedStartPosition.y);
        float remainingDistance = totalDistance;

        int baseIndex = diameterIndex * 3;

        while (remainingDistance > 0.1f)
        {
            GameObject pipePrefab;
            float pipeLength;

            if (remainingDistance >= 3f && gridSystem.MyTileGroup.PipesMajorType.Length > baseIndex + 1)
            {
                pipePrefab = gridSystem.MyTileGroup.PipesMajorType[baseIndex + 1]; // 3m pipe
                pipeLength = 3f;
            }
            else
            {
                pipePrefab = gridSystem.MyTileGroup.PipesMajorType[baseIndex]; // 1m pipe
                pipeLength = 1f;
            }

            GameObject newPipe = Instantiate(pipePrefab, orderedSegment[0].transform.parent);
            newPipe.transform.SetPositionAndRotation(currentPosition, Quaternion.Euler(-90f, 0f, 0f));
            instantiatedPipes.Add(newPipe);
            instantiatedPipesList.Add(newPipe);
            Undo.RegisterCreatedObjectUndo(newPipe, "Replace Vertical Pipe");

            currentPosition += Vector3.up * pipeLength;
            remainingDistance -= pipeLength;
        }

        return instantiatedPipesList;
    }

    private GameObject PlaceCornerPipeWithDiameter(GameObject cornerPlaceholder, List<GameObject> path,
    int index, int diameterIndex)
    {
        Vector3 dirToPrev = Vector3.zero;
        Vector3 dirToNext = Vector3.zero;

        // For corner placeholders, find the two connected pipes
        if (PipeUtilities.IsCornerPlaceholder(cornerPlaceholder))
        {
            List<GameObject> connected = new();
            foreach (GameObject pipe in path)
            {
                if (pipe == cornerPlaceholder) continue;

                Vector3 diff = pipe.transform.position - cornerPlaceholder.transform.position;
                if (diff.magnitude < 1.5f && diff.magnitude > 0.1f)
                {
                    connected.Add(pipe);
                }
            }

            if (connected.Count >= 2)
            {
                dirToPrev = (connected[0].transform.position - cornerPlaceholder.transform.position).normalized;
                dirToNext = (connected[1].transform.position - cornerPlaceholder.transform.position).normalized;
            }
            else if (connected.Count == 1)
            {
                // Try to find direction from path index
                if (index > 0)
                    dirToPrev = (path[index - 1].transform.position - cornerPlaceholder.transform.position).normalized;
                if (index < path.Count - 1)
                    dirToNext = (path[index + 1].transform.position - cornerPlaceholder.transform.position).normalized;
            }
        }
        else
        {
            // Original logic for non-placeholder corners
            if (index > 0)
            {
                dirToPrev = (path[index - 1].transform.position - cornerPlaceholder.transform.position).normalized;
            }

            if (index < path.Count - 1)
            {
                dirToNext = (path[index + 1].transform.position - cornerPlaceholder.transform.position).normalized;
            }
        }

        float rotation = PipeUtilities.GetCornerPipeRotation(dirToPrev, dirToNext);

        int baseIndex = diameterIndex * 3;
        GameObject cornerPrefab = gridSystem.MyTileGroup.PipesMajorType[baseIndex + 2]; // Corner pipe
        GameObject newCorner = Instantiate(cornerPrefab, cornerPlaceholder.transform.parent);
        newCorner.transform.SetPositionAndRotation(cornerPlaceholder.transform.position, Quaternion.Euler(0, rotation, 0));
        instantiatedPipes.Add(newCorner);
        Undo.RegisterCreatedObjectUndo(newCorner, "Replace Corner Pipe");

        return newCorner;
    }

    private List<GameObject> PlaceStraightPipesWithDiameter(Vector3 startPosition, Vector3 direction, float distance, int diameterIndex)
    {
        List<GameObject> instantiatedPipesList = new();

        float rotation = PipeUtilities.GetPipeRotation(direction);

        // Find the placeholder at the start position to get proper offset
        GameObject startPlaceholder = disabledPipePlaceholders.FirstOrDefault(p =>
            Vector3.Distance(p.transform.position, startPosition) < 0.1f);

        Vector3 adjustedStartPosition = startPosition;
        if (startPlaceholder != null)
        {
            Vector3 offset = PipeUtilities.GetPipeOffsetForPlaceholder(startPlaceholder, direction);
            adjustedStartPosition += offset;
        }

        Vector3 currentPosition = adjustedStartPosition;
        float remainingDistance = distance;
        int baseIndex = diameterIndex * 3;

        while (remainingDistance > 0.1f)
        {
            GameObject pipePrefab;
            float pipeLength;

            if (remainingDistance >= 3f && gridSystem.MyTileGroup.PipesMajorType.Length > baseIndex + 1)
            {
                pipePrefab = gridSystem.MyTileGroup.PipesMajorType[baseIndex + 1]; // 3m pipe
                pipeLength = 3f;
            }
            else if (remainingDistance >= 1f)
            {
                pipePrefab = gridSystem.MyTileGroup.PipesMajorType[baseIndex]; // 1m pipe
                pipeLength = 1f;
            }
            else
            {
                break;
            }

            GameObject newPipe = Instantiate(pipePrefab, disabledPipePlaceholders[0].transform.parent);
            newPipe.transform.SetPositionAndRotation(currentPosition, Quaternion.Euler(0, rotation, 0));
            instantiatedPipes.Add(newPipe);
            instantiatedPipesList.Add(newPipe);
            Undo.RegisterCreatedObjectUndo(newPipe, "Replace Pipe");

            currentPosition += direction * pipeLength;
            remainingDistance -= pipeLength;
        }

        return instantiatedPipesList;
    }

    
    

    // For perp stacked pipe bugfix testing
    private bool ShouldPipesBeInSameSegment_(GameObject pipe1, GameObject pipe2, List<GameObject> currentSegment)
    {
        // If the segment already has pipes, check if adding this pipe makes sense
        if (currentSegment.Count > 1)
        {
            // Check if the new pipe would create a valid path
            Vector3 segmentDirection = GetSegmentDirection(currentSegment);
            Vector3 newPipeDirection = (pipe2.transform.position - pipe1.transform.position).normalized;

            // If the segment has a clear direction, new pipes should follow it
            if (segmentDirection != Vector3.zero)
            {
                float dot = Vector3.Dot(segmentDirection, newPipeDirection);

                // Allow connection if it continues in the same direction or creates a valid corner
                return Mathf.Abs(dot) > 0.9f || // Same direction
                       Mathf.Abs(dot) < 0.1f;   // Perpendicular (corner)
            }
        }

        return true; // Allow connection for small segments
    }

    // Get the general direction of a pipe segment
    private Vector3 GetSegmentDirection(List<GameObject> segment)
    {
        if (segment.Count < 2) return Vector3.zero;

        // Find the two most distant pipes to determine overall direction
        float maxDistance = 0;
        GameObject pipe1 = null, pipe2 = null;

        for (int i = 0; i < segment.Count; i++)
        {
            for (int j = i + 1; j < segment.Count; j++)
            {
                float dist = Vector3.Distance(segment[i].transform.position, segment[j].transform.position);
                if (dist > maxDistance)
                {
                    maxDistance = dist;
                    pipe1 = segment[i];
                    pipe2 = segment[j];
                }
            }
        }

        if (pipe1 != null && pipe2 != null)
        {
            return (pipe2.transform.position - pipe1.transform.position).normalized;
        }

        return Vector3.zero;
    }

    // For perp stacked pipe bugfix testing
    private bool ArePipesConnected_(GameObject pipe1, GameObject pipe2)
    {
        // Simple distance check - pipes must be adjacent
        Vector3 connectionVector = pipe2.transform.position - pipe1.transform.position;
        float distance = connectionVector.magnitude;

        // Check if one of them is a corner (corners can be slightly further)
        Vector3 scale1 = pipe1.transform.localScale;
        Vector3 scale2 = pipe2.transform.localScale;
        bool isCorner1 = Mathf.Abs(scale1.x - scale1.y) < 0.01f && Mathf.Abs(scale1.y - scale1.z) < 0.01f;
        bool isCorner2 = Mathf.Abs(scale2.x - scale2.y) < 0.01f && Mathf.Abs(scale2.y - scale2.z) < 0.01f;

        float maxDistance = (isCorner1 || isCorner2) ? 1.5f : 1.1f;

        // Pipes must be within the max distance to be considered connected
        if (distance > maxDistance) return false;

        // Check if the connection is along a cardinal direction
        Vector3 normalized = connectionVector.normalized;
        bool isCardinal =
            Mathf.Abs(normalized.x) > 0.9f ||
            Mathf.Abs(normalized.y) > 0.9f ||
            Mathf.Abs(normalized.z) > 0.9f;

        if (!isCardinal) return false;

        // If pipes are perpendicular and vertically separated, they shouldn't connect
        if (Mathf.Abs(normalized.y) > 0.9f) // Vertical connection
        {
            // Check if both pipes are vertical
            bool pipe1Vertical = IsVerticalPipe(pipe1);
            bool pipe2Vertical = IsVerticalPipe(pipe2);

            // If both are vertical, they can connect
            if (pipe1Vertical && pipe2Vertical)
            {
                return true;
            }

            // If neither is vertical, they're horizontal pipes stacked - don't connect
            if (!pipe1Vertical && !pipe2Vertical)
            {
                return false;
            }

            // One is vertical, one is horizontal - don't connect unless it's a corner
            return isCorner1 || isCorner2;
        }
        else // Horizontal connection
        {
            // Check if pipes are aligned in the same direction
            bool pipe1Vertical = IsVerticalPipe(pipe1);
            bool pipe2Vertical = IsVerticalPipe(pipe2);

            // Vertical pipes shouldn't connect horizontally
            if (pipe1Vertical || pipe2Vertical)
            {
                return isCorner1 || isCorner2;
            }

            // Both are horizontal - check if they're aligned
            // They should connect if they're in line with each other
            return ArePipesAligned(pipe1, pipe2, normalized);
        }
    }

    // Helper method to determine if a pipe is vertical based on its rotation
    private bool IsVerticalPipe(GameObject pipe)
    {
        Vector3 rotation = pipe.transform.rotation.eulerAngles;
        // Check for vertical rotation (Z rotation of 90 or 270, or X rotation of 90 or 270)
        return (Mathf.Abs(rotation.z - 90f) < 5f || Mathf.Abs(rotation.z - 270f) < 5f) ||
               (Mathf.Abs(rotation.x - 90f) < 5f || Mathf.Abs(rotation.x - 270f) < 5f);
    }

    // Helper method to get the primary direction of a pipe
    private Vector3 GetPipeDirection(GameObject pipe)
    {
        Vector3 rotation = pipe.transform.rotation.eulerAngles;
        Vector3 scale = pipe.transform.localScale;

        // Check if it's a corner (all scales equal)
        bool isCorner = Mathf.Abs(scale.x - scale.y) < 0.01f &&
                        Mathf.Abs(scale.y - scale.z) < 0.01f;

        if (isCorner)
        {
            return Vector3.zero; // Corners don't have a single direction
        }

        // For straight pipes, find the longest axis
        if (IsVerticalPipe(pipe))
        {
            return Vector3.up;
        }
        else
        {
            // Horizontal pipe - determine if X or Z aligned
            if (Mathf.Abs(rotation.y) < 45f || Mathf.Abs(rotation.y - 180f) < 45f)
            {
                return Vector3.forward; // Z-aligned
            }
            else
            {
                return Vector3.right; // X-aligned
            }
        }
    }

    // Helper method to check if two horizontal pipes are aligned
    private bool ArePipesAligned(GameObject pipe1, GameObject pipe2, Vector3 connectionDirection)
    {
        Vector3 pipe1Dir = GetPipeDirection(pipe1);
        Vector3 pipe2Dir = GetPipeDirection(pipe2);

        // If either is a corner, allow connection
        if (pipe1Dir == Vector3.zero || pipe2Dir == Vector3.zero)
        {
            return true;
        }

        // Check if both pipes are aligned with the connection direction
        float dot1 = Mathf.Abs(Vector3.Dot(pipe1Dir, connectionDirection));
        float dot2 = Mathf.Abs(Vector3.Dot(pipe2Dir, connectionDirection));

        // Both pipes should be aligned with the connection direction
        // OR perpendicular to it (for corners)
        return (dot1 > 0.9f && dot2 > 0.9f) || // Both aligned with connection
               (dot1 < 0.1f && dot2 < 0.1f);   // Both perpendicular (shouldn't happen for straight connection)
    }

    private void RevertPipeReplacements()
    {
        RevertPipeSections();

        // Destroy all instantiated pipes at once
        foreach (GameObject pipe in instantiatedPipes)
        {
            if (pipe != null) Undo.DestroyObjectImmediate(pipe);
        }
        instantiatedPipes.Clear();

        // Reactivate all placeholders at once
        foreach (GameObject placeholder in disabledPipePlaceholders)
        {
            if (placeholder != null)
            {
                placeholder.SetActive(true);
                Undo.RegisterFullObjectHierarchyUndo(placeholder, "Reactivate Pipe Placeholder");
            }
        }
        disabledPipePlaceholders.Clear();
    }

    private void RevertPipeSections()
    {
        foreach (var section in instantiatedPipeSections.Where(s => s != null))
        {
            Undo.DestroyObjectImmediate(section);
        }
        instantiatedPipeSections.Clear();
    }

    private void ReplaceTile(Transform tile, GridSystem.EditingSection section)
    {
        //Debug.Log(tile.localPosition + " " + tile.localRotation.eulerAngles.y);
        string prefabName = GetPrefabName(tile.gameObject);

        if (blockTypes.Keys.Contains(prefabName) && placeholderToPrefabMap.TryGetValue(prefabName, out GameObject objToInst))
        {
            tile.gameObject.SetActive(false);
            disabledPlaceholders.Add(tile.gameObject);

            if(objToInst == null)
            {
                HandleTileReplacers();
            }

            GameObject newBlock = Instantiate(objToInst, tile.parent);
            Vector3 position = gridSystem.SnapTileToGrid(newBlock, tile.localPosition, tile.localRotation.eulerAngles.y, section);
            Quaternion rotation = tile.rotation;

            instantiatedPrefabs.Add(newBlock);
            newBlock.transform.localPosition = position;
            Vector3 defaultAngles = newBlock.transform.localRotation.eulerAngles;
            newBlock.transform.localRotation = Quaternion.Euler(rotation.eulerAngles + defaultAngles);
            Undo.RegisterCreatedObjectUndo(newBlock, "Replace Tile");
        }
    }

    private string GetPrefabName(GameObject tile)
    {
        PrefabInstanceStatus instanceStatus = PrefabUtility.GetPrefabInstanceStatus(tile);
        if (instanceStatus == PrefabInstanceStatus.Connected)
        {
            string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(tile);
            return prefabPath.Split('.')[0].Split('/').Last();
        }
        else
        {
            return tile.name.Split("(Clone)")[0];
        }
    }

    /// <summary>
    /// Removes placeholder prefabs from the grid.
    /// </summary>
    private void RemovePlaceholderPrefabs()
    {
        if (gridSystem.CurrentRoom == null) return;

        Transform roomTransform = gridSystem.CurrentRoom.transform;
        GameObject thisgo;
        foreach (Transform tileHolder in roomTransform)
        {
            int cnt = tileHolder.childCount;
            for (int i = cnt - 1; i >= 0; i--)
            {
                thisgo = tileHolder.GetChild(i).gameObject;
                PrefabInstanceStatus instanceStatus = PrefabUtility.GetPrefabInstanceStatus(thisgo);
                string prefabName = null;

                if (instanceStatus == PrefabInstanceStatus.Connected)
                {
                    prefabName = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(thisgo);
                    prefabName = prefabName.Split('.')[0].Split('/').Last();
                }
                else
                {
                    prefabName = thisgo.name.Split("(Clone)")[0];
                }

                if (blockTypes.Keys.Contains(prefabName))
                {
                    Undo.DestroyObjectImmediate(thisgo);
                }
            }
        }
    }

    /// <summary>
    /// Reverts the replacements made to the tiles in the grid.
    /// </summary>
    private void RevertReplacements()
    {
        if (instantiatedPrefabs.Count > 0)
        {
            foreach (GameObject prefab in instantiatedPrefabs)
            {
                if(prefab != null)
                    Undo.DestroyObjectImmediate(prefab);
            }
            instantiatedPrefabs.Clear();
        }

        if (disabledPlaceholders.Count > 0)
        {
            foreach (GameObject placeholder in disabledPlaceholders)
            {
                if (placeholder != null)
                {
                    placeholder.SetActive(true);
                    Undo.RegisterFullObjectHierarchyUndo(placeholder, "Reactivate Placeholder");
                }
            }
            disabledPlaceholders.Clear();
        }
        else
        {
            //the editor window has been deselected before reversion
            Transform roomTransform = gridSystem.CurrentRoom.transform;
            GameObject thisgo;
            string prefabName;
            int cnt;
            foreach (Transform tileHolder in roomTransform)
            {
                cnt = tileHolder.childCount;
                for (int i = cnt - 1; i >= 0; i--)
                {
                    thisgo = tileHolder.GetChild(i).gameObject;
                    PrefabInstanceStatus instanceStatus = PrefabUtility.GetPrefabInstanceStatus(thisgo);

                    if (instanceStatus == PrefabInstanceStatus.NotAPrefab)
                    {
                        Undo.DestroyObjectImmediate(thisgo);
                    }
                    else
                    {
                        prefabName = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(thisgo);
                        prefabName = prefabName.Split('.')[0].Split('/').Last();
                        if (!blockTypes.Keys.Contains(prefabName))
                        {
                            Undo.DestroyObjectImmediate(thisgo);
                        }
                        else
                        {
                            thisgo.SetActive(true);
                            Undo.RegisterFullObjectHierarchyUndo(thisgo, "Reactivate Placeholder");
                        }
                    }
                }
            }
        }
        RevertPipeSections();
    }

    /// <summary>
    /// Sets the grid settings for the GridSystem.
    /// </summary>
    /// <param name="newCellSize">The new cell size for the grid.</param>
    /// <param name="newGridSizeY">The new Y size for the grid.</param>
    /// <param name="drawYAxis">Indicates if Y-axis lines should be drawn.</param>
    /// <param name="section">The section of the grid being edited.</param>
    private void SetGridSettings(float newCellSize, float newGridSizeY, bool drawYAxis, GridSystem.EditingSection section)
    {
        gridSystem.cellSize = newCellSize;
        gridSystem.GridSize = new Vector3(gridSystem.GridSize.x, newGridSizeY, gridSystem.GridSize.z);
        gridSystem.drawYAxisLines = drawYAxis;
        gridSystem.ActiveSection = section;
    }

    /// <summary>
    /// Gets the mouse position in the scene view.
    /// </summary>
    /// <returns>The position of the mouse in world coordinates.</returns>
    private Vector3 GetMousePositionInScene()
    {
        Ray ray;

        if (gridSystem.ActiveSection == GridSystem.EditingSection.MainRoom || gridSystem.ActiveSection == GridSystem.EditingSection.Interior)
        {
            ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.point.y >= gridSystem.transform.position.y)
                {
                    return hit.point;
                }
                // Otherwise the point is discarded
            }
        }
        // If the ray doesn't hit anything or if the builder is in a different mode
        ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        Plane gridPlane = new(Vector3.up, gridSystem.transform.position);

        if (gridPlane.Raycast(ray, out float enter))
        {
            // Return the point where the mouse ray intersects the grid plane.
            return ray.GetPoint(enter);
        }

        return Vector3.zero;
    }

    /// <summary>
    /// Draws the grid in the scene view and handles user interactions.
    /// </summary>
    private void OnSceneGUI()
    {
        // Ensure that we are in the correct context (inside the editor)
        if (gridSystem == null) return;
        DrawGrid();

        if (showPipeDirectionArrows)
        {
            DrawPipeDirectionArrows();
        }

        // Render the block preview at the mouse position
        Vector3 previewPosition = GetMousePositionInScene();
        Vector3 halfBlockSize = Vector3.zero;
        Matrix4x4 transPos = Matrix4x4.identity;
        if (previewPosition != Vector3.zero)
        {
            previewPosition = gridSystem.SnapPreviewToGrid(previewPosition, previewRotation.eulerAngles.y);

            // Special Condition for ceiling tiles to bypass minor bug in on-plane placement.
            if (gridSystem.ActiveSection == GridSystem.EditingSection.Ceiling)
            {
                if (gridSystem.blockPrefab.name == "CeilingCube")
                {
                    //Raise the ceiling tile up one.
                    previewPosition.y += 1;
                }
            }

            if (gridSystem.isFloorBlock)
            {
                previewPosition.y = gridSystem.transform.position.y;
            }

            if (gridSystem.blockPrefab.TryGetComponent<BoxCollider>(out var boxCollider))
            {
                Vector3 blockSize = boxCollider.size;
                Vector3 rawSize = gridSystem.blockPrefab.transform.localScale;

                blockSize.x *= rawSize.x;
                blockSize.y *= rawSize.y;
                blockSize.z *= rawSize.z;

                if (gridSystem.blockPrefab.name.Contains("Pipe"))
                {
                    float diameter = pipeDiameters[selectedPipeDiameter];
                    if (placePipeCorners)
                    {
                        blockSize.x = diameter;
                        blockSize.z = diameter;
                        blockSize.y = diameter;
                    }
                    else
                    {
                        float rotationZ = previewRotation.eulerAngles.z;
                        // Pipe runs along Y axis
                        if (rotationZ == 90f || rotationZ == 270f)
                        {
                            //Placeholder is X-forward
                            blockSize.x = 1f;
                            blockSize.z = diameter;
                            blockSize.y = diameter;
                        }
                        // Pipe runs along X axis
                        else
                        {
                            blockSize.y = diameter;
                            blockSize.z = diameter;
                            blockSize.x = 1f;
                        }
                    }
                }
                Vector3 gridStart = gridSystem.transform.position;
                Vector3 gridEnd = gridStart + new Vector3(gridSystem.GridSize.x, gridSystem.GridSize.y, gridSystem.GridSize.z);

                if (previewPosition.x >= gridStart.x && previewPosition.x <= gridEnd.x &&
                    previewPosition.y >= gridStart.y && previewPosition.y <= gridEnd.y &&
                    previewPosition.z >= gridStart.z && previewPosition.z <= gridEnd.z)
                {
                    // Draw the block preview with the same size as the prefab
                    Handles.color = Color.green;
                    transPos = Matrix4x4.TRS(previewPosition, previewRotation, Vector3.one);
                    Handles.matrix = transPos;
                    halfBlockSize = blockSize / 2f;
                    Handles.DrawWireCube(halfBlockSize, blockSize);
                    Handles.matrix = Matrix4x4.identity;

                    // Draw direction arrow for pipe preview
                    if (showPipeDirectionArrows && gridSystem.blockPrefab.name.Contains("Pipe"))
                    {
                        DrawPipeArrow(previewPosition, previewRotation.eulerAngles, Color.green, 0.5f, Vector3.right);
                        // Draw diameter indicator
                        DrawPipeDiameterIndicator(previewPosition, pipeDiameters[selectedPipeDiameter]);
                    }
                }
            }
        }

        // Allow interaction with the grid in the scene view
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)  // Left Mouse Button (LMB)
        {
            Vector3 gridStart = gridSystem.transform.position;
            Vector3 gridEnd = gridStart + new Vector3(gridSystem.GridSize.x, gridSystem.GridSize.y, gridSystem.GridSize.z);

            if (previewPosition.x >= gridStart.x && previewPosition.x <= gridEnd.x &&
                previewPosition.y >= gridStart.y && previewPosition.y <= gridEnd.y &&
                previewPosition.z >= gridStart.z && previewPosition.z <= gridEnd.z)
            {
                // Transform the offset
                halfBlockSize = transPos.MultiplyVector(halfBlockSize);

                GameObject newBlock = gridSystem.PlaceBlockInGrid(previewPosition + halfBlockSize,
                    previewRotation, DetermineParent());

                // If it's a pipe, store its diameter and update its scale
                if (newBlock.name.Contains("Pipe"))
                {
                    pipePlaceholderDiameters[newBlock] = selectedPipeDiameter;
                    UpdatePipePlaceholderVisualScale(newBlock, pipeDiameters[selectedPipeDiameter]);
                }

                Undo.RegisterCreatedObjectUndo(newBlock, "Place Block");  // Register the object for undo
                Undo.SetCurrentGroupName("Place Block");  // Group undo actions together
                // Consume the event so it doesn't propagate to other handlers
                Event.current.Use();
            }
        }

        // Handle Backspace for deleting block
        if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Backspace)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))  
            {
                GameObject parobj = null;
                try
                {
                    parobj = hit.collider.transform.parent.parent.gameObject;
                }
                catch (Exception) { }
                if (parobj != null && parobj == gridSystem.CurrentRoom)
                {
                    Undo.DestroyObjectImmediate(hit.collider.gameObject);  
                    Event.current.Use();
                }
            }
        }

        // Handle '\' key press to change prefab
        if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Backslash)
        {
            ChangeTilePrefabAtMousePosition();
            Event.current.Use(); 
        }

        HandleUtility.Repaint(); 
    }

    private void DrawPipeDiameterIndicator(Vector3 position, float scale)
    {
        Handles.color = new Color(0, 1, 0, 0.3f);
        // Draw diameter text
        GUIStyle style = new();
        style.normal.textColor = Color.green;
        style.fontSize = 12;
        Handles.Label(position + Vector3.up * 0.5f, (scale*100f).ToString()+" mm", style);
    }

    private void DrawPipeDirectionArrows()
    {
        if (!showPipeDirectionArrows || gridSystem.CurrentRoom == null) return;

        Transform pipesHolder = gridSystem.CurrentRoom.transform.Find("Pipes");

        if (pipesHolder != null)
        {
            foreach (Transform pipe in pipesHolder)
            {
                if (pipe.gameObject.activeSelf && GetPrefabName(pipe.gameObject) == PipeCube)
                {
                    DrawPipeArrow(pipe.position, pipe.rotation.eulerAngles, Color.yellow, 0.3f, Vector3.right);
                }
            }
        }

        // Draw arrows for instantiated pipes
        foreach (GameObject pipe in instantiatedPipes)
        {
            if (pipe != null)
            {
                DrawPipeArrow(pipe.transform.position, pipe.transform.rotation.eulerAngles, Color.cyan, 0.3f, Vector3.forward);
            }
        }
    }

    private void DrawPipeArrow(Vector3 position, Vector3 rotation, Color color, float size, Vector3 forwardDirection)
    {
        Handles.color = color;

        // Calculate direction based on rotation
        Vector3 direction;
        bool isVertical = Mathf.Abs(rotation.z - 90f) < 5f || Mathf.Abs(rotation.z - 270f) < 5f ||
                         Mathf.Abs(rotation.x - 90f) < 5f || Mathf.Abs(rotation.x - 270f) < 5f;

        if (isVertical)
        {
            direction = rotation.z < 180f ? Vector3.up : Vector3.down;
        }
        else
        {
            direction = Quaternion.Euler(0, rotation.y, 0) * forwardDirection;
        }

        // Draw arrow
        Vector3 arrowStart = position - 0.5f * size * direction;
        Vector3 arrowEnd = position + 0.5f * size * direction;

        Handles.DrawLine(arrowStart, arrowEnd);

        // Draw arrowhead
        Vector3 right = Vector3.Cross(direction, Vector3.up).normalized;
        if (right.magnitude < 0.1f) right = Vector3.Cross(direction, Vector3.forward).normalized;

        Vector3 arrowLeft = arrowEnd - 0.3f * size * direction - 0.2f * size * right;
        Vector3 arrowRight = arrowEnd - 0.3f * size * direction + 0.2f * size * right;

        Handles.DrawLine(arrowEnd, arrowLeft);
        Handles.DrawLine(arrowEnd, arrowRight);
        Handles.DrawWireDisc(position, direction, size * 0.1f);
    }

    /// <summary>
    /// Draws the grid lines in the scene view.
    /// </summary>
    private void DrawGrid()
    {
        if (gridSystem.drawGridLines)
        {
            // Draw grid lines in the scene
            Vector3 startPos = gridSystem.transform.position;
            Vector3 endPos = startPos + new Vector3(gridSystem.GridSize.x, 0, gridSystem.GridSize.z);

            // Calculate the number of cells in each direction based on grid size and cell size
            int numCellsX = Mathf.FloorToInt(gridSystem.GridSize.x / gridSystem.cellSize);
            int numCellsZ = Mathf.FloorToInt(gridSystem.GridSize.z / gridSystem.cellSize);

            // Draw X-axis grid lines
            for (int x = 0; x <= numCellsX; x++)
            {
                Vector3 start = new(startPos.x + x * gridSystem.cellSize, startPos.y, startPos.z);
                Vector3 end = new(startPos.x + x * gridSystem.cellSize, startPos.y, endPos.z);
                Handles.color = Color.blue;
                Handles.DrawLine(start, end);
            }

            // Draw Z-axis grid lines
            for (int z = 0; z <= numCellsZ; z++)
            {
                Vector3 start = new(startPos.x, startPos.y, startPos.z + z * gridSystem.cellSize);
                Vector3 end = new(endPos.x, startPos.y, startPos.z + z * gridSystem.cellSize);
                Handles.color = Color.red;
                Handles.DrawLine(start, end);
            }

            if (gridSystem.drawYAxisLines)
            {
                // Draw Y-axis
                int numCellsY = Mathf.FloorToInt(gridSystem.GridSize.y / gridSystem.cellSize);
                for (int y = 1; y <= numCellsY; y++)
                {
                    Vector3 start = new(startPos.x, startPos.y + y * gridSystem.cellSize, startPos.z);
                    Vector3 end = new(endPos.x, startPos.y + y * gridSystem.cellSize, startPos.z);
                    Handles.color = Color.green;
                    Handles.DrawLine(start, end);

                    start = new(endPos.x, startPos.y + y * gridSystem.cellSize, startPos.z);
                    end = new(endPos.x, startPos.y + y * gridSystem.cellSize, endPos.z);
                    Handles.DrawLine(start, end);

                    start = new(startPos.x, startPos.y + y * gridSystem.cellSize, startPos.z);
                    end = new(startPos.x, startPos.y + y * gridSystem.cellSize, endPos.z);
                    Handles.DrawLine(start, end);

                    start = new(startPos.x, startPos.y + y * gridSystem.cellSize, endPos.z);
                    end = new(endPos.x, startPos.y + y * gridSystem.cellSize, endPos.z);
                    Handles.DrawLine(start, end);
                }
            }
        }
    }

    /// <summary>
    /// Changes the prefab at the mouse position to the selected prefab.
    /// </summary>
    private void ChangeTilePrefabAtMousePosition()
    {
        // Perform a Raycast to check if there's a tile at the mouse position
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))  // Check for objects under the cursor
        {
            GameObject hitObject = hit.collider.gameObject;
            
            if (hitObject.TryGetComponent<TilePrefabSelector>(out var prefabSelector))
            {
                PrefabInstanceStatus instanceStatus = PrefabUtility.GetPrefabInstanceStatus(hitObject);
                string prefabName;

                if (instanceStatus == PrefabInstanceStatus.Connected)
                {
                    prefabName = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(hitObject);
                    prefabName = prefabName.Split('.')[0];
                    prefabName = prefabName.Split('/')[^1];
                }
                else
                {
                    prefabName = hitObject.name.Split("(Clone)")[0];
                }

                // Determine which type of prefab is under the mouse
                GameObject selectedPrefab = GetReplacementPrefab(prefabName);

                if (selectedPrefab != null)
                {
                    // Update the prefab in the TilePrefabSelector
                    prefabSelector.CurrentPrefab = selectedPrefab;
                    prefabSelector.UpdatePrefab();

                    Undo.RegisterCompleteObjectUndo(hitObject, "Change Tile Prefab");
                }
            }
        }
    }

    /// <summary>
    /// Maps the tile name to the corresponding selected prefab from the dropdown.
    /// </summary>
    /// <param name="tileName">The name of the tile to be replaced.</param>
    /// <returns>The replacement prefab if found; otherwise, null.</returns>
    private GameObject GetReplacementPrefab(string tileName)
    {
        // Map the tile name to the corresponding selected prefab from the dropdown
        if (placeholderToPrefabMap.TryGetValue(tileName, out GameObject prefab))
        {
            return prefab;
        }
        else
        {
            Debug.LogWarning("No replacement prefab found for: " + tileName);
            return null;
        }
    }

    /// <summary>
    /// Identifies child containers within the current room.
    /// </summary>
    public void IdentifyChildContainers()
    {
        Transform troom = gridSystem.CurrentRoom.transform;
        gridSystem.TileHolders = new Dictionary<string, GameObject>();
        for (int i = 0; i < troom.childCount; i++)
        {
            gridSystem.TileHolders.TryAdd(troom.GetChild(i).name, troom.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// Determines the parent transform for the block to be placed.
    /// </summary>
    /// <returns>The transform of the parent object for the block.</returns>
    private Transform DetermineParent()
    {
        // Lights can be placed in any mode.
        if (gridSystem.blockPrefab.name == "LightCube")
        {
            if (gridSystem.TileHolders.TryGetValue("Lights", out GameObject value))
            {
                return value.transform;
            }
        }

        if (gridSystem.blockPrefab.name == "DuctCube")
        {
            if (gridSystem.TileHolders.TryGetValue("OverheadDucts", out GameObject value))
            {
                return value.transform;
            }
        }

        else if (gridSystem.blockPrefab.name == "PipeCube")
        {
            if (gridSystem.TileHolders.TryGetValue("Pipes", out GameObject value))
            {
                return value.transform;
            }
        }

        else
        {
            if (gridSystem.ActiveSection == GridSystem.EditingSection.FloorBase)
            {
                if (gridSystem.TileHolders.TryGetValue("Lower", out GameObject value))
                {
                    return value.transform;
                }
            }

            else if (gridSystem.ActiveSection == GridSystem.EditingSection.MainRoom)
            {
                // Let's try to fix this hardcoding in the future
                if (gridSystem.blockPrefab.name == "FloorCube")
                {
                    if (gridSystem.TileHolders.TryGetValue("Floor", out GameObject value))
                    {
                        return value.transform;
                    }
                }

                else if (gridSystem.blockPrefab.name == "DoorCube")
                {
                    if (gridSystem.TileHolders.TryGetValue("Doors", out GameObject value))
                    {
                        return value.transform;
                    }
                }

                else if (gridSystem.blockPrefab.name == "StairCube")
                {
                    if (gridSystem.TileHolders.TryGetValue("Stairs", out GameObject value))
                    {
                        return value.transform;
                    }
                }

                else if (gridSystem.blockPrefab.name == "WallCube" || gridSystem.blockPrefab.name == "WallStrutCube")
                {
                    // Set Wall group by rotation, which will need to be cleaned up by hand
                    if (previewRotation.eulerAngles.y == 0)
                    {
                        if (gridSystem.TileHolders.TryGetValue("Wall0Z", out GameObject value))
                        {
                            return value.transform;
                        }
                    }

                    else if (previewRotation.eulerAngles.y == 90)
                    {
                        if (gridSystem.TileHolders.TryGetValue("Wall0X", out GameObject value))
                        {
                            return value.transform;
                        }
                    }

                    else if (previewRotation.eulerAngles.y == 180)
                    {
                        if (gridSystem.TileHolders.TryGetValue("WallPosZ", out GameObject value))
                        {
                            return value.transform;
                        }
                    }

                    else if (previewRotation.eulerAngles.y == 270)
                    {
                        if (gridSystem.TileHolders.TryGetValue("WallPosX", out GameObject value))
                        {
                            return value.transform;
                        }
                    }
                }
            }

            else if (gridSystem.ActiveSection == GridSystem.EditingSection.Ceiling)
            {
                if (gridSystem.blockPrefab.name == "OverStruct")
                {
                    if (gridSystem.TileHolders.TryGetValue("Upper", out GameObject value))
                    {
                        return value.transform;
                    }
                }

                else if (gridSystem.blockPrefab.name == "CeilingCube")
                {
                    if (gridSystem.TileHolders.TryGetValue("Ceiling", out GameObject value))
                    {
                        return value.transform;
                    }
                }

                else if (gridSystem.blockPrefab.name == "BaseStructCube")
                {
                    if (gridSystem.TileHolders.TryGetValue("Upper", out GameObject value))
                    {
                        return value.transform;
                    }
                }
            }

            else if (gridSystem.ActiveSection == GridSystem.EditingSection.Interior)
            {
                if (gridSystem.TileHolders.TryGetValue("InteriorObjects", out GameObject value))
                {
                    return value.transform;
                }
            }
        }

        return null;
    }
}
#endif