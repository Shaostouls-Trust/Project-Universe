using UnityEngine;

public class WaterFlowTest : MonoBehaviour
{
    public WaterFlowSystem wfs;
    public VolumeWaterData vwd;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        wfs.AddWaterToVolume(vwd, vwd.VolumeSize.x * vwd.VolumeSize.y * vwd.VolumeSize.z);
    }


}
