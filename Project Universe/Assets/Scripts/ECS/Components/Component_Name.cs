using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity;
using Unity.Entities;
using Unity.Collections;

public struct Component_Name : IComponentData
{
    public FixedString64 name; //GUID?
}
