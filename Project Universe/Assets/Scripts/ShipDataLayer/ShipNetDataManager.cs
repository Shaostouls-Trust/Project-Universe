using UnityEngine;

// Example usage in networking layer
public class ShipNetDataManager : MonoBehaviour
{
    public void OnShipStateReceived(string jsonState)
    {
        ShipStateManager.Instance.DeserializeState(jsonState);
    }

    public void SendShipState()
    {
        string jsonState = ShipStateManager.Instance.SerializeState();
        // Send over network
    }
}
