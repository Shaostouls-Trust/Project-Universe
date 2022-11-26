using ProjectUniverse.PowerSystem.Nuclear;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SteamGeneratorUIController : MonoBehaviour
{
    [SerializeField] private SteamGenerator steamGen;
    [SerializeField] private TMP_Text maxRate;
    [SerializeField] private TMP_Text currentRate;
    [SerializeField] private TMP_Text automatic;
    [SerializeField] private TMP_Text hottemp;
    [SerializeField] private TMP_Text cooltemp;
    [SerializeField] private TMP_Text coolantRes;
    [SerializeField] private TMP_Text waterRes;
    private float timer = 0.25f;

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        if(timer <= 0f)
        {
            timer = 0.25f;
            maxRate.text = ""+steamGen.ThresholdPumpRate;
            currentRate.text = ""+Mathf.Round(steamGen.CurrentPumpRate);
            if (steamGen.AutomaticControl)
            {
                automatic.text = "Yes";
            }
            else
            {
                automatic.text = "No";
            }
            hottemp.text = "" + Mathf.Round(steamGen.CoolantHotTemp);
            cooltemp.text = "" + Mathf.Round(steamGen.CoolantCoolTemp);
            coolantRes.text = "" + Mathf.Round(steamGen.CoolantReservoir);
            waterRes.text = "" + Mathf.Round(steamGen.WaterReservoir);
        }
    }
}
