using UnityEngine;

/// <summary>
/// Filter that removes specific gas types
/// </summary>
public class GasFilter : GasEffectBase
{
    [SerializeField] private string[] blockedGasTypes = { "CO2", "Dust" };

    public override void OnPreTransfer(TransferContext context)
    {
        foreach (var gasType in blockedGasTypes)
        {
            context.FilterGasType(gasType, GetEffectName());
        }
    }

    public override void OnPostTransfer(TransferContext context) { }

    public override string GetEffectName() => "GasFilter";
}
