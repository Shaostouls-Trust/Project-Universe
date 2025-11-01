using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class HolographicRoom : MonoBehaviour
{
    [Header("Room Configuration")]
    public string roomName = "Room";
    public Color roomColor = new(0.2f, 0.6f, 1f, 0.3f);
    public Material baseMaterial; // Reference to your custom material
    public Material templateMaterial; // Material for template objects

    [Header("Room Volumes")]
    public GameObject volumeContainer; // Container object with BoxColliders

    private readonly List<GameObject> volumeObjects = new();
    private readonly List<GameObject> templateObjects = new();

    private readonly List<MeshRenderer> volumeRenderers = new();

    void Start()
    {
        if (baseMaterial != null && volumeContainer != null)
            SetupRoom();
    }

    public void SetupRoom()
    {
        // Clean up any previously created objects
        foreach (var obj in volumeObjects)
        {
            if (obj != null) Destroy(obj);
        }
        volumeObjects.Clear();

        foreach (var obj in templateObjects)
        {
            if (obj != null) Destroy(obj);
        }
        templateObjects.Clear();

        // Create volumes from BoxColliders
        if (volumeContainer != null)
        {
            BoxCollider[] colliders = volumeContainer.GetComponents<BoxCollider>();
            foreach (var collider in colliders)
            {
                GameObject volumeObj = CreateVolumeFromCollider(collider);
                volumeObjects.Add(volumeObj);

                // Store renderer reference for easy color changes
                if (volumeObj.TryGetComponent<MeshRenderer>(out var renderer))
                    volumeRenderers.Add(renderer);
            }
        }
        // Find and process templates
        Transform templatesContainer = FindTemplatesContainer();
        if (templatesContainer != null)
        {
            for (int i = 0; i < templatesContainer.childCount; i++)
            {
                Transform child = templatesContainer.GetChild(i);
                MeshFilter meshFilter = child.GetComponent<MeshFilter>();

                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    // Don't show template objects in scene view
                    GameObject templateObj = CreateTemplateObject(child);
                    templateObjects.Add(templateObj);
                }
            }
        }
    }

    // New method to update color without changing the stored color
    public void UpdateRoomColor(Color? overrideColor = null)
    {
        Color colorToApply = overrideColor ?? roomColor;

        foreach (var renderer in volumeRenderers)
        {
            if (renderer != null)
            {
                MaterialPropertyBlock props = new();
                renderer.GetPropertyBlock(props);
                props.SetColor("_BaseColor", colorToApply);
                renderer.SetPropertyBlock(props);
            }
        }
    }

    // Get current room color
    public Color GetRoomColor()
    {
        return roomColor;
    }

    // Get template count for port configuration
    public int GetTemplateCount()
    {
        Transform templatesContainer = FindTemplatesContainer();
        if (templatesContainer != null)
        {
            return templatesContainer.childCount;
        }
        return 0;
    }

    private GameObject CreateVolumeFromCollider(BoxCollider collider)
    {
        // Create a new game object for the volume
        GameObject volumeObj = new("Volume_" + volumeObjects.Count);
        volumeObj.transform.parent = transform;

        // Position and scale to match the collider
        volumeObj.transform.SetPositionAndRotation(volumeContainer.transform.position + collider.center, volumeContainer.transform.rotation);
        volumeObj.transform.localScale = collider.size;

        // Add mesh components
        MeshFilter meshFilter = volumeObj.AddComponent<MeshFilter>();
        meshFilter.mesh = CreateCubeMesh();

        MeshRenderer renderer = volumeObj.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = baseMaterial;

        // Set layer
        volumeObj.layer = LayerMask.NameToLayer("Hologram");

        // Apply material with property block for color override
        MaterialPropertyBlock props = new();
        renderer.GetPropertyBlock(props);
        props.SetColor("_BaseColor", roomColor);
        renderer.SetPropertyBlock(props);

        // Add hologram effect component
        volumeObj.AddComponent<HologramVolumeEffect>();

        return volumeObj;
    }

    private GameObject CreateTemplateObject(Transform templateTransform)
    {
        /*
        // Create a new game object for the template
        GameObject templateObj = new("TemplateHologram_" + templateObjects.Count);
        templateObj.transform.parent = transform;

        // Copy transform properties
        templateObj.transform.SetPositionAndRotation(templateTransform.position, templateTransform.rotation);
        templateObj.transform.localScale = templateTransform.localScale;

        // Copy mesh
        MeshFilter sourceMeshFilter = templateTransform.GetComponent<MeshFilter>();
        MeshFilter meshFilter = templateObj.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = sourceMeshFilter.sharedMesh;

        // Add renderer with template material
        MeshRenderer renderer = templateObj.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = templateMaterial;

        // Set layer
        templateObj.layer = LayerMask.NameToLayer("Hologram");

        // Add hologram effect component
        templateObj.AddComponent<HologramVolumeEffect>();

        return templateObj;*/
        //A
        // Return empty GameObject to maintain list consistency
        //GameObject emptyObj = new GameObject("TemplateHologram_Disabled_" + templateObjects.Count);
        //emptyObj.transform.parent = transform;
        //emptyObj.SetActive(false);
        //return emptyObj;
        //B
        return null;
    }

    private Transform FindTemplatesContainer()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.name.Contains("Templates"))
            {
                return child;
            }
        }
        return null;
    }

    private Mesh CreateCubeMesh()
    {
        GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Mesh mesh = temp.GetComponent<MeshFilter>().mesh;
        DestroyImmediate(temp);
        return mesh;
    }

    void OnDestroy()
    {
        // Ensure we clean up created objects
        foreach (var obj in volumeObjects)
        {
            if (obj != null) Destroy(obj);
        }

        foreach (var obj in templateObjects)
        {
            if (obj != null) Destroy(obj);
        }
    }

}