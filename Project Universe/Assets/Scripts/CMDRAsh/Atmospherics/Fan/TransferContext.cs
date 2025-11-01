using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data passed to effects for safe modification
/// </summary>
public class TransferContext
{
    public IGasContainer Source { get; private set; }
    public IGasContainer Destination { get; private set; }

    // List of gas batches being transferred (can be modified)
    public List<GasTransferBatch> GasBatches { get; private set; }

    public float DeltaTime { get; private set; }
    public float BaseFlowRate { get; private set; }

    // Track modifications for debugging
    public List<string> AppliedEffects { get; private set; } = new();

    public TransferContext(
        IGasContainer source,
        IGasContainer destination,
        List<GasTransferBatch> gasBatches,
        float deltaTime,
        float baseFlowRate)
    {
        Source = source;
        Destination = destination;
        GasBatches = gasBatches;
        DeltaTime = deltaTime;
        BaseFlowRate = baseFlowRate;
    }

    /// <summary>
    /// Gets total gas amount across all batches
    /// </summary>
    //public float GetTotalAmount()
    //{
    //    return GasBatches.Sum(b => b.Amount);
    //}

    /// <summary>
    /// Safely modify flow rate with validation
    /// </summary>
    public void ModifyFlowRate(float newRate, string effectName)
    {
        if (newRate < 0)
        {
            Debug.LogWarning($"{effectName} tried to set negative flow rate");
            return;
        }

        BaseFlowRate = newRate;
        AppliedEffects.Add($"{effectName}: Flow={newRate:F2}");
    }

    /// <summary>
    /// Remove specific gas type from transfer
    /// </summary>
    public void FilterGasType(string gasType, string effectName)
    {
        GasBatches.RemoveAll(b => b.GasType == gasType);
        AppliedEffects.Add($"{effectName}: Filtered {gasType}");
    }

    /// <summary>
    /// Reduce gas amount by percentage
    /// </summary>
    public void ReduceByPercentage(float percentage, string effectName)
    {
        if (percentage < 0 || percentage > 1)
        {
            Debug.LogWarning($"{effectName} invalid percentage: {percentage}");
            return;
        }

        foreach (var batch in GasBatches)
        {
            batch.Amount *= (1 - percentage);
        }

        AppliedEffects.Add($"{effectName}: Reduced by {percentage * 100}%");
    }
}
