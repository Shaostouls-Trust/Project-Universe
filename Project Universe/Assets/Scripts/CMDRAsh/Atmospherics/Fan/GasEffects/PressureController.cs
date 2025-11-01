using UnityEngine;


/// <summary>
/// Controller that monitors and conditionally disables pump
/// </summary>
public class PressureController : GasEffectBase
{
    [SerializeField] private float maxPressure = 2f;

    public override void OnPreTransfer(TransferContext context)
    {
        if (parentFan.Pressure >= maxPressure)
        {
            parentFan.SetRunning(false);
            Debug.Log("Pressure limit reached, fan disabled");
        }
    }

    public override void OnPostTransfer(TransferContext context) { }

    public override string GetEffectName() => "PressureController";
}