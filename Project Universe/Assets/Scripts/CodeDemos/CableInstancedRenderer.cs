using UnityEngine;
using System.Collections.Generic;

public class CableInstancedRenderer : MonoBehaviour
{
    [Header("Rendering")]
    public Mesh cableMesh; // Cylinder mesh
    public Material cableMaterial; // Shader that supports instancing

    [Header("Demo Data")]
    public Transform[] demoPoints; // Drag some transforms in here for demo

    private Matrix4x4[] instanceMatrices;
    private Vector4[] instanceColors;
    private MaterialPropertyBlock propertyBlock;

    void Start()
    {
        propertyBlock = new MaterialPropertyBlock();
        GenerateDemoData();
    }

    void GenerateDemoData()
    {
        if (demoPoints.Length < 2) return;

        List<Matrix4x4> matrices = new List<Matrix4x4>();
        List<Vector4> colors = new List<Vector4>();

        // Create segments between consecutive points
        for (int i = 0; i < demoPoints.Length - 1; i++)
        {
            Vector3 start = demoPoints[i].position;
            Vector3 end = demoPoints[i + 1].position;

            // Calculate segment transform
            Vector3 direction = end - start;
            float length = direction.magnitude;
            Vector3 center = (start + end) * 0.5f;

            // Create transform matrix
            Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
            Vector3 scale = new Vector3(0.1f, length * 0.5f, 0.1f); // Cylinder is 2 units tall by default

            matrices.Add(Matrix4x4.TRS(center, rotation, scale));

            // Random colors for demo
            colors.Add(new Vector4(
                Random.Range(0.5f, 1f),
                Random.Range(0.2f, 0.8f),
                0.1f,
                1f
            ));
        }

        instanceMatrices = matrices.ToArray();
        instanceColors = colors.ToArray();
    }

    void Update()
    {
        if (instanceMatrices == null || instanceMatrices.Length == 0) return;

        // Set the color array in the material property block
        propertyBlock.SetVectorArray("_Colors", instanceColors);

        // Render all instances in a single draw call
        Graphics.DrawMeshInstanced(
            cableMesh,
            0, // submesh index
            cableMaterial,
            instanceMatrices,
            instanceMatrices.Length,
            propertyBlock
        );
    }

    // Demo: "Break" a random segment by changing its color
    [ContextMenu("Break Random Segment")]
    void BreakRandomSegment()
    {
        if (instanceColors == null || instanceColors.Length == 0) return;

        int randomIndex = Random.Range(0, instanceColors.Length);
        instanceColors[randomIndex] = new Vector4(0.1f, 0.1f, 0.1f, 0.3f); // Dark and transparent
    }

    // Demo: "Rebuild" by regenerating data (simulates room rebuild)
    [ContextMenu("Rebuild All Segments")]
    void RebuildSegments()
    {
        GenerateDemoData();
    }
}