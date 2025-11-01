using UnityEngine;

/// <summary>
/// Base class for common effect functionality
/// </summary>
public abstract class GasEffectBase : MonoBehaviour, IGasEffect
{
    protected Fan parentFan;
    [SerializeField] protected bool isActive = true;

    public virtual void Initialize(Fan fan)
    {
        parentFan = fan;
    }

    public virtual void Cleanup()
    {
        parentFan = null;
    }

    public virtual bool IsActive() => isActive && enabled;

    public abstract void OnPreTransfer(TransferContext context);
    public abstract void OnPostTransfer(TransferContext context);
    public abstract string GetEffectName();
}
