using UnityEngine;
using System.Collections.Generic;

public static class GasColorConfiguration
{
    private static Dictionary<string, Color> gasColors = new Dictionary<string, Color>()
    {
        { "O2", new Color(0.2f, 0.7f, 1f) },           // Light Blue
        { "N2", new Color(0.8f, 0.8f, 0.9f) },         // Light Grey
        { "CO2", new Color(0.5f, 0.5f, 0.5f) },        // Dark Grey
        { "H2O", new Color(1f, 1f, 1f) },              // White
        { "Ar", new Color(1f, 0.8f, 0.2f) },           // Gold
        { "He", new Color(0.9f, 0.5f, 1f) },           // Light Purple
        { "Ne", new Color(1f, 0.6f, 0.8f) },           // Pink
        { "Kr", new Color(0.4f, 0.8f, 0.6f) },         // Teal
        { "Xe", new Color(0.6f, 0.4f, 0.9f) },         // Purple
        { "Rn", new Color(1f, 0.4f, 0.4f) },           // Red
        { "CH4", new Color(0.8f, 0.6f, 0.2f) },        // Brown
        { "NH3", new Color(0.5f, 1f, 0.5f) },          // Light Green
        { "NO", new Color(1f, 1f, 0.3f) },            // Yellow
        { "NO2", new Color(1f, 0.5f, 0.3f) },          // Orange
        { "CO", new Color(0.7f, 0.7f, 0.4f) },        // Olive
    };

    public static Color GetGasColor(string gasId)
    {
        if (gasColors.ContainsKey(gasId))
            return gasColors[gasId];

        // Generate consistent random color for unknown gases
        Random.InitState(gasId.GetHashCode());
        return new Color(Random.value, Random.value, Random.value);
    }

    public static void RegisterGasColor(string gasId, Color color)
    {
        gasColors[gasId] = color;
    }
}