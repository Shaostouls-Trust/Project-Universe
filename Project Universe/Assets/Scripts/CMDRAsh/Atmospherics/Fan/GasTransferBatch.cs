using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a batch of gas being transferred
/// </summary>
public class GasTransferBatch
{
    public string GasType { get; set; }
    public float Amount { get; set; }
    public Dictionary<string, float> Properties { get; set; } = new();

    public GasTransferBatch(string gasType, float amount)
    {
        GasType = gasType;
        Amount = amount;
    }

    public GasTransferBatch Clone()
    {
        return new GasTransferBatch(GasType, Amount)
        {
            Properties = new Dictionary<string, float>(Properties)
        };
    }
}
