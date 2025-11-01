using ProjectUniverse.Environment.Gas;
using System.Collections.Generic;
using UnityEngine;

public interface IGasContainer
{
    List<IGas> Gases { get; }
    float Pressure { get; set; }
    //string ContainerName { get; }
}
