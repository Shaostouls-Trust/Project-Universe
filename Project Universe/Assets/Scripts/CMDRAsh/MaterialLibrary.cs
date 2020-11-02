using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialLibrary : MonoBehaviour
{
    public Material[] doorMaterials;
    public Material[] displayMaterials;
    public Material[] powerMaterials;

    //[SerializeField]
    private static Material[] doorStateMaterials;
    //[SerializeField]
    private static Material[] doorDisplayMaterials;
    private static Material[] powerStateMaterials;
    void Awake()
    {
        doorStateMaterials = doorMaterials;
        doorDisplayMaterials = displayMaterials;
        powerStateMaterials = powerMaterials;
    }

    public static Material[] GetDoorStateMaterials()
    {
        return doorStateMaterials;
    }
    public static Material GetDoorStateMaterials(int index)
    {
        return doorStateMaterials[index];
    }
    public static Material[] GetDoorDisplayMaterials()
    {
        return doorDisplayMaterials;
    }
    public static Material GetDoorDisplayMaterials(int index)
    {
        return doorDisplayMaterials[index];
    }
    public static Material[] GetPowerSystemStateMaterials()
    {
        return powerStateMaterials;
    }
    public static Material GetPowerSystemStateMaterials(int index)
    {
        return powerStateMaterials[index];
    }
}
