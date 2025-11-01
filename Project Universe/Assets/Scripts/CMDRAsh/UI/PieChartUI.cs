using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class PieChartUI : MonoBehaviour
{
    [SerializeField] private Image pieChartImage;
    [SerializeField] private Transform labelContainer;
    [SerializeField] private GameObject labelPrefab;

    private Texture2D pieTexture;
    private const int TEXTURE_SIZE = 256;

    private void Start()
    {
        if (pieChartImage == null)
            pieChartImage = GetComponent<Image>();

        pieTexture = new Texture2D(TEXTURE_SIZE, TEXTURE_SIZE, TextureFormat.RGBA32, false);
        pieChartImage.sprite = Sprite.Create(pieTexture, new Rect(0, 0, TEXTURE_SIZE, TEXTURE_SIZE), Vector2.one * 0.5f);
    }

    public void UpdatePieChart(List<(string gasId, float percentage, string name)> gasData)
    {
        // Clear previous labels
        if (labelContainer != null)
        {
            foreach (Transform child in labelContainer)
                Destroy(child.gameObject);
        }

        // Clear texture
        Color[] pixels = pieTexture.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;
        pieTexture.SetPixels(pixels);

        float currentAngle = -90f; // Start from top
        int center = TEXTURE_SIZE / 2;
        int radius = TEXTURE_SIZE / 2 - 2;

        foreach (var (gasId, percentage, name) in gasData)
        {
            if (percentage <= 0) continue;

            Color gasColor = GasColorConfiguration.GetGasColor(gasId);
            float sliceAngle = (percentage / 100f) * 360f;

            // Draw pie slice
            DrawPieSlice(center, radius, currentAngle, sliceAngle, gasColor);

            // Add label
            if (labelContainer != null && labelPrefab != null)
            {
                GameObject labelObj = Instantiate(labelPrefab, labelContainer);
                TextMeshProUGUI labelText = labelObj.GetComponent<TextMeshProUGUI>();
                if (labelText != null)
                {
                    labelText.text = $"{name}: {percentage:F1}%";

                    Image labelImage = labelObj.GetComponent<Image>();
                    if (labelImage != null)
                        labelImage.color = gasColor;
                }
            }

            currentAngle += sliceAngle;
        }

        pieTexture.Apply();
    }

    private void DrawPieSlice(int centerX, int centerY, float startAngle, float sliceAngle, Color color)
    {
        for (float angle = startAngle; angle < startAngle + sliceAngle; angle += 1f)
        {
            float rad = angle * Mathf.Deg2Rad;

            for (float r = 0; r < TEXTURE_SIZE / 2; r += 1f)
            {
                int x = centerX + (int)(r * Mathf.Cos(rad));
                int y = centerY + (int)(r * Mathf.Sin(rad));

                if (x >= 0 && x < TEXTURE_SIZE && y >= 0 && y < TEXTURE_SIZE)
                {
                    pieTexture.SetPixel(x, y, color);
                }
            }
        }
    }
}