using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class HolographicRoomManager : MonoBehaviour
{
    [Header("Room Management")]
    public List<HolographicRoom> rooms = new List<HolographicRoom>();

    [Header("Material Settings")]
    public Material baseMaterial;
    public Material templateMaterial;

    [Header("Visual Settings")]
    public GameObject labelPrefab;
    public float labelHeight = 0.2f;
    public Canvas labelCanvas;

    [Header("Color Palette")]
    public Color[] blueShades = new Color[]
    {
        new Color(0.1f, 0.4f, 0.9f, 0.3f),
        new Color(0.2f, 0.6f, 1f, 0.3f),
        new Color(0.3f, 0.7f, 0.9f, 0.3f),
        new Color(0.1f, 0.5f, 0.8f, 0.3f),
        new Color(0.4f, 0.8f, 1f, 0.3f)
    };

    void Start()
    {
        SetupRooms();
    }

    void SetupRooms()
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            // Assign unique color and base material
            rooms[i].roomColor = blueShades[i % blueShades.Length];
            rooms[i].baseMaterial = baseMaterial;
            rooms[i].templateMaterial = templateMaterial;
            rooms[i].SetupRoom();

            // Create label
            CreateRoomLabel(rooms[i]);
        }
    }

    void CreateRoomLabel(HolographicRoom room)
    {
        if (labelPrefab == null || room.volumeContainer == null) return;

        // Calculate center of all volumes
        Vector3 center = Vector3.zero;
        int count = 0;

        BoxCollider[] colliders = room.volumeContainer.GetComponents<BoxCollider>();
        foreach (var collider in colliders)
        {
            center += room.volumeContainer.transform.position + collider.center;
            count++;
        }

        if (count == 0) return;

        center /= count;

        // Create label above center
        GameObject label = Instantiate(labelPrefab, center + Vector3.up * labelHeight, Quaternion.identity, labelCanvas.transform);
        label.GetComponent<TMP_Text>().text = room.roomName;
        label.GetComponent<TMP_Text>().color = room.roomColor + new Color(0.3f, 0.3f, 0.3f, 0.7f);
        label.layer = LayerMask.NameToLayer("Hologram");
    }
}