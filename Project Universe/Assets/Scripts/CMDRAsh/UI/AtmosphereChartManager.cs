using UnityEngine;
using System.Collections.Generic;
using TMPro;
using ProjectUniverse.Environment.Gas;
using ProjectUniverse.Environment.Volumes;

public class AtmosphereChartManager : MonoBehaviour
{
    [SerializeField] private VolumeAtmosphereController vac;
    [SerializeField] private PieChartUI pieChart;
    [SerializeField] private BarChartUI barChart;
    [SerializeField] private float updateInterval = 1f;

    private float updateTimer = 0f;

    private void Update()
    {
        updateTimer += Time.deltaTime;
        if (updateTimer >= updateInterval)
        {
            updateTimer = 0f;
            RefreshCharts();
        }
    }

    private void RefreshCharts()
    {
        if (vac == null)
            return;

        // Prepare gas data for pie chart
        var gasData = new List<(string, float, string)>();
        float totalMoles = 0f;

        foreach (IGas gas in vac.RoomGassesLegacy)
        {
            totalMoles += gas.GetConcentration();
        }

        if (totalMoles > 0)
        {
            foreach (IGas gas in vac.RoomGassesLegacy)
            {
                float percentage = (gas.GetConcentration() / totalMoles) * 100f;
                if (percentage >= 0.1f) // Only show gases above 0.1%
                    gasData.Add((gas.GetIDName(), percentage, gas.GetIDName()));
            }
        }

        // Sort by percentage descending
        gasData.Sort((a, b) => b.Item2.CompareTo(a.Item2));
        pieChart.UpdatePieChart(gasData);

        // Prepare particulate data for bar chart
        var particulateData = new List<(string, float, string)>();
        List<ParticulateConcentration> particulates = vac.GetAllParticulates();

        foreach (var particulate in particulates)
        {
            if (particulate.ConcentrationPPMW > 0)
            {
                ParticulateType type = ParticulateDatabase.GetParticulate(particulate.ParticulateTypeID);
                string name = type?.Name ?? particulate.ParticulateTypeID;
                particulateData.Add((particulate.ParticulateTypeID, particulate.ConcentrationPPMW, name));
            }
        }

        // Sort by concentration descending
        particulateData.Sort((a, b) => b.Item2.CompareTo(a.Item2));
        barChart.UpdateBarChart(particulateData);
    }
}