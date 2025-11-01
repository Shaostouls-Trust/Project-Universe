using UnityEngine;

/// <summary>
/// Regulator that limits flow rate
/// </summary>
public class FlowRegulator : GasEffectBase
{
    [SerializeField] private float maxFlowRate = 5f;

    public override void OnPreTransfer(TransferContext context)
    {
        if (context.BaseFlowRate > maxFlowRate)
        {
            context.ModifyFlowRate(maxFlowRate, GetEffectName());
        }
    }

    public override void OnPostTransfer(TransferContext context) { }

    public override string GetEffectName() => "FlowRegulator";
}
