using UnityEngine;
using System.Collections.Generic;

public static class ParticulateColorConfiguration
{
    private static Dictionary<string, Color> particulateColors = new Dictionary<string, Color>()
    {
        { "metal_oxide", new Color(1f, 1f, 1f) },      // White
        { "metal_dust", new Color(0.8f, 0.8f, 0.8f) }, // Light Grey
        { "soot", new Color(0.1f, 0.1f, 0.1f) },       // Black
        { "ash", new Color(0.3f, 0.3f, 0.3f) },        // Dark Grey
        { "carbon_black", new Color(0.15f, 0.15f, 0.15f) }, // Very Dark Grey
        { "radioactive_dust", new Color(0f, 1f, 0f) }, // Bright Green
        { "organic_dust", new Color(0.6f, 0.4f, 0.2f) }, // Brown
        { "dust", new Color(0.7f, 0.7f, 0.6f) },       // Beige
    };

    public static Color GetParticulateColor(string particulateId)
    {
        if (particulateColors.ContainsKey(particulateId))
            return particulateColors[particulateId];

        // Generate consistent random shade of grey/black for unknown particulates
        Random.InitState(particulateId.GetHashCode());
        float shade = Random.Range(0.1f, 0.8f);
        return new Color(shade, shade, shade);
    }

    public static void RegisterParticulateColor(string particulateId, Color color)
    {
        particulateColors[particulateId] = color;
    }
}