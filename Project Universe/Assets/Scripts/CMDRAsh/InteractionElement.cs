using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Serve as a backend for interacting with world space objects. All objects the player can interact with will call from here.
/// </summary>
public class InteractionElement : MonoBehaviour
{
    [SerializeField] private GameObject scriptedObject;
    [SerializeField] private int parameter = -1;

    [Header("Hold Operation Settings")]
    [SerializeField] private bool supportsHold = false;
    [SerializeField] private bool instantHoldStart = false;
    [SerializeField] private bool supportsQuickTap = true;
    [SerializeField] private float holdUpdateInterval = 0.1f; // Throttle update calls

    private bool isHolding = false;
    private float currentHoldDuration = 0f;

    public bool SupportsHold => supportsHold;
    private bool isCurrentlyHolding = false;
    private float lastHoldUpdate = 0f;

    public bool InstantHoldStart => instantHoldStart;
    public bool SupportsQuickTap => supportsQuickTap;

    public int Parameter
    {
        get { return parameter; }
        set { parameter = value; }
    }

    //Fire-and-forget
    public void Interact()
    {
        if (parameter == -1)
        {
            scriptedObject.SendMessage("ExternalInteractFunc", null, SendMessageOptions.DontRequireReceiver);
        }
        else
        {
            scriptedObject.SendMessage("ExternalInteractFunc", parameter, SendMessageOptions.DontRequireReceiver);
        }
    }

    /// <summary>
    /// Start hold operation
    /// </summary>
    public void StartHold()
    {
        if (!supportsHold) return;

        isCurrentlyHolding = true;
        lastHoldUpdate = Time.time;

        SendInteractionMessage("ExternalHoldStart", parameter);
    }

    /// <summary>
    /// Update hold operation (called every frame during hold)
    /// </summary>
    public void UpdateHold(float holdDuration)
    {
        if (!supportsHold || !isCurrentlyHolding) return;

        // Throttle updates to avoid performance issues
        if (Time.time - lastHoldUpdate >= holdUpdateInterval)
        {
            var holdData = new HoldData
            {
                duration = holdDuration,
                parameter = parameter
            };

            SendInteractionMessage("ExternalHoldUpdate", holdData);
            lastHoldUpdate = Time.time;
        }
    }

    /// <summary>
    /// End hold operation
    /// </summary>
    public void EndHold()
    {
        if (!supportsHold || !isCurrentlyHolding) return;

        isCurrentlyHolding = false;
        SendInteractionMessage("ExternalHoldEnd", parameter);
    }

    private void SendInteractionMessage(string methodName, object param)
    {
        if (scriptedObject == null) return;

        if (param == null || (param is int && (int)param == -1))
        {
            scriptedObject.SendMessage(methodName, null, SendMessageOptions.DontRequireReceiver);
        }
        else
        {
            scriptedObject.SendMessage(methodName, param, SendMessageOptions.DontRequireReceiver);
        }
    }

    private void OnDisable()
    {
        // Clean up if disabled during hold
        if (isCurrentlyHolding)
        {
            isCurrentlyHolding = false;
            SendInteractionMessage("ExternalHoldEnd", parameter);
        }
    }
}

/// <summary>
/// Data structure for hold update information
/// </summary>
[System.Serializable]
public struct HoldData
{
    public float duration;
    public int parameter;
}