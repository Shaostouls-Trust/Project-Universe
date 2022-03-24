using ProjectUniverse.Items;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Items with multiple uses. Uses do not add up, regardless of amount of items in stack.
/// </summary>
public class Consumable_Applyable : IEquipable
{
    [SerializeField] private ApplyableType applyableType;

    public enum ApplyableType{
        Seed,
        Healthpack,
        Ammopack
    }

    public ApplyableType ThisApplyableType
    {
        get { return applyableType; }
    }

    override protected void Use()
    {
    }
}
