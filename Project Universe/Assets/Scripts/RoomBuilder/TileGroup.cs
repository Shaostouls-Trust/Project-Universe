using UnityEngine;
using System.Collections;
using UnityEditor;

[CreateAssetMenu(fileName = "TileGroup", menuName = "Scriptable Objects/TileGroup")]
public class TileGroup : ScriptableObject
{
    [SerializeField] private GameObject[] lowerFloors;
    [SerializeField] private GameObject[] baseStructs;
    [SerializeField] private GameObject[] floors;
    [SerializeField] private GameObject[] walls;
    [SerializeField] private GameObject[] wallStructs;
    [SerializeField] private GameObject[] doors;
    [SerializeField] private GameObject[] stairs;
    [SerializeField] private GameObject[] ductsMajorType; //exact replacement will be handled otherwise
    [SerializeField] private GameObject[] lights;
    [SerializeField] private GameObject[] overstructs;
    [SerializeField] private GameObject[] pipesMajorType; //exact replacement will be handled otherwise
    [SerializeField] private GameObject[] ceilings;

    public GameObject[] LowerFloors
    {
        get { return lowerFloors; }
    }
    public GameObject[] BaseStructs
    {
        get { return baseStructs; }
    }
    public GameObject[] Floors
    {
        get { return floors; }
    }
    public GameObject[] Walls
    {
        get { return walls; }
    }
    public GameObject[] WallStructs
    {
        get { return wallStructs; }
    }
    public GameObject[] Doors
    {
        get { return doors; }
    }
    public GameObject[] Stairs
    {
        get { return stairs; }
    }
    public GameObject[] DuctsMajorType
    {
        get { return ductsMajorType; }
    }
    public GameObject[] Lights
    {
        get { return lights; }
    }
    public GameObject[] Overstructs
    {
        get { return overstructs; }
    }
    public GameObject[] PipesMajorType
    {
        get { return pipesMajorType; }
    }
    public GameObject[] Ceilings
    {
        get { return ceilings; }
    }
}
