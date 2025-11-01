using UnityEngine;

public abstract class Opening : MonoBehaviour
{
    [Header("Opening Properties")]
    public float width = 1f;
    public float height = 2f;
    public float bottomHeight = 0f; // Height of bottom of opening from floor
    public VolumeWaterData connectedVolume1;
    public VolumeWaterData connectedVolume2;

    [Header("Flow Properties")]
    public float flowCoefficient = 1f; // Multiplier for flow rate

    public abstract bool CanWaterFlow();

    public float GetOpeningArea()
    {
        return width * height;
    }

    public virtual float GetBottomElevation()
    {
        return transform.position.y + bottomHeight;
    }

    public (VolumeWaterData, VolumeWaterData) GetConnectedVolumes()
    {
        return (connectedVolume1, connectedVolume2);
    }
}