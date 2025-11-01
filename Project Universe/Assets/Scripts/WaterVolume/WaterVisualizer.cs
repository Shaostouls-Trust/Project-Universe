using System.Collections.Generic;
using UnityEngine;

public class WaterVisualizer : MonoBehaviour
{
    [Header("Visual Settings")]
    public Material waterMaterial;
    public GameObject waterPlanePrefab;
    public GameObject waterVisPrefab;

    private Dictionary<VolumeWaterData, GameObject> waterVisuals;

    void Start()
    {
        waterVisuals = new Dictionary<VolumeWaterData, GameObject>();
        InitializeVisuals();
    }

    void InitializeVisuals()
    {
        VolumeWaterData[] volumes = FindObjectsByType<VolumeWaterData>(FindObjectsSortMode.None);

        foreach (var volume in volumes)
        {
            if (waterVisPrefab != null) 
            {
                if (volume.WaterVisGO == null)
                {
                    volume.AssignNewWaterVisManager(waterVisPrefab, volume.GetComponent<BoxCollider>());
                } 
            }
            else if (waterPlanePrefab != null)
            {
                GameObject waterPlane = Instantiate(waterPlanePrefab, volume.transform);
                waterPlane.name = $"WaterVisual_{volume.name}";
                waterVisuals[volume] = waterPlane;

                if (waterMaterial != null)
                {
                    waterPlane.GetComponent<Renderer>().material = waterMaterial;
                }
            }
        }
    }

    void Update()
    {
        UpdateWaterVisuals();
    }

    void UpdateWaterVisuals()
    {
        foreach (var kvp in waterVisuals)
        {
            VolumeWaterData volume = kvp.Key;
            GameObject visual = kvp.Value;

            if (volume.HasWater)
            {
                visual.SetActive(true);

                // Position water plane at water level
                Vector3 pos = visual.transform.position;
                pos.y = volume.GetAbsoluteWaterHeight();
                visual.transform.position = pos;

                // Scale to fit volume
                Vector3 scale = visual.transform.localScale;
                scale.x = volume.VolumeSize.x/10f;
                scale.z = volume.VolumeSize.z/10f;
                visual.transform.localScale = scale;
            }
            else
            {
                visual.SetActive(false);
            }
        }
    }
}
