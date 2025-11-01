using UnityEngine;
using ProjectUniverse.Environment.Gas;
using System.Collections.Generic;

public class GasContainerAdapter : MonoBehaviour, IGasContainer
{
    [SerializeField] private List<IGas> gases = new();
    [SerializeField] private float pressure;

    public List<IGas> Gases => gases;
    public float Pressure
    {
        get => pressure;
        set => pressure = value;
    }
    //public string ContainerName => gameObject.name;
}

