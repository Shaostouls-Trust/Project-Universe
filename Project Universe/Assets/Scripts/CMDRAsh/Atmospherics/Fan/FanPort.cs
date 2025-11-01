using UnityEngine;
using ProjectUniverse.Environment.Gas;
using System.Collections.Generic;

public class FanPort : MonoBehaviour
{
    [SerializeField] private float connectionSearchRadius = 1f;
    private IGasContainer connectedContainer;

    public void FindConnection()
    {
        Collider[] colliders = Physics.OverlapSphere(
            transform.position,
            connectionSearchRadius
        );

        foreach (Collider col in colliders)
        {
            if (col.TryGetComponent<IGasContainer>(out var container))
            {
                if (container.GetHashCode() != GetComponentInParent<Fan>()
                    .GetHashCode()) // Avoid self-connection
                {
                    connectedContainer = container;
                    return;
                }
            }
        }

        Debug.LogWarning($"No gas container found near {gameObject.name}");
    }

    public IGasContainer GetConnectedContainer() => connectedContainer;
    public bool IsConnected() => connectedContainer != null;
}
