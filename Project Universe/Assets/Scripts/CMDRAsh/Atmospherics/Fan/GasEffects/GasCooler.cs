using UnityEngine;

/// <summary>
/// Cooler that reduces pressure/temperature
/// </summary>
public class GasCooler : GasEffectBase
{
    [SerializeField] private float coolingRate = 0.1f; // 10% reduction

    public override void OnPostTransfer(TransferContext context)
    {
        context.ReduceByPercentage(coolingRate, GetEffectName());
    }

    public override void OnPreTransfer(TransferContext context) { }

    public override string GetEffectName() => "GasCooler";
}
