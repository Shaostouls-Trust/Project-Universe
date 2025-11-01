using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class BarChartUI : MonoBehaviour
{
    [SerializeField] private Transform barContainer;
    [SerializeField] private GameObject barPrefab;
    [SerializeField] private float barHeight = 30f;
    [SerializeField] private float maxConcentrationDisplay = 1000f; // PPMW scale

    public void UpdateBarChart(List<(string particulateId, float concentrationPPMW, string name)> particulateData)
    {
        // Clear previous bars
        foreach (Transform child in barContainer)
            Destroy(child.gameObject);

        float maxConcentration = particulateData.Count > 0
            ? Mathf.Max(maxConcentrationDisplay, particulateData[0].concentrationPPMW * 1.2f)
            : maxConcentrationDisplay;

        float yOffset = 0;

        foreach (var (particulateId, concentration, name) in particulateData)
        {
            if (concentration <= 0) continue;

            GameObject barObj = Instantiate(barPrefab, barContainer);
            RectTransform barRect = barObj.GetComponent<RectTransform>();

            if (barRect != null)
            {
                barRect.anchoredPosition = new Vector2(0, -yOffset);
                float width = (concentration / maxConcentration) * 250f;
                barRect.sizeDelta = new Vector2(width, barHeight);
                yOffset += barHeight + 5f;
            }

            Image barImage = barObj.GetComponent<Image>();
            if (barImage != null)
                barImage.color = ParticulateColorConfiguration.GetParticulateColor(particulateId);

            // Add label
            TextMeshProUGUI labelText = barObj.GetComponentInChildren<TextMeshProUGUI>();
            if (labelText != null)
                labelText.text = $"{name}: {concentration:F2} PPMW";
        }
    }
}