using System.Collections.Generic;
using UnityEngine;

public interface IGasEffect
{
    /// <summary>
    /// Called before gas transfer to allow pre-processing
    /// </summary>
    void OnPreTransfer(TransferContext context);

    /// <summary>
    /// Called after gas transfer to allow post-processing
    /// </summary>
    void OnPostTransfer(TransferContext context);

    /// <summary>
    /// Whether this effect is currently active
    /// </summary>
    bool IsActive();

    /// <summary>
    /// Display name for debugging
    /// </summary>
    string GetEffectName();

    /// <summary>
    /// Called when effect is added to fan
    /// </summary>
    void Initialize(Fan parentFan);

    /// <summary>
    /// Called when effect is removed or fan is destroyed
    /// </summary>
    void Cleanup();
}