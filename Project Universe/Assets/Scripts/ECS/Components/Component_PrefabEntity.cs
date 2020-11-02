using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[GenerateAuthoringComponent]//this makes this component a field
public struct Component_PrefabEntity : IComponentData
{
    public Entity prefabEntity;
}
