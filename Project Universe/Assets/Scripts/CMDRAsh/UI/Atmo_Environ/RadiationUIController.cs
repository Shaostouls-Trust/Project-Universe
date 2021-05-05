using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class RadiationUIController : MonoBehaviour
{
    public TMP_Text peakRateDis;
    public TMP_Text StayTimeDis;//seconds since entered rad zone, but displayed in hr,min,sec
    public Image rateBarParent;
    public Image doseMeter;
    public TMP_Text rateDis;
    public TMP_Text doseDis;
    public GameObject[] BodyHealthDis;
    /// 0 - Head
    /// 1 - Chest
    /// 2,3 - LegL, LegR
    /// 4,5 - ArmL, ArmR
    private float peakRate=0f;
    private float stayTime;
    private float rate;
    private float dose;
    //private float doscimeterMax;
    [SerializeField] private PlayerVolumeController PVC;

    // Update is called once per frame
    void Update()
    {
        //Rate
        rate = PVC.GetRadiationExposureRate();
        if(rate <= PVC.GetMaxRadsDetectable())
        {
            rateDis.text = Math.Round(rate, 1) + " R";
        }
        else if(rate <= PVC.GetMaxRadsDetectable()+5000) { rateDis.text = "5000 R"; }
        else { rateDis.text = "ERR"; }
        //rateDis.text = Math.Round(rate,1)+" R";
        Image rateBar = rateBarParent.transform.GetChild(0).GetComponent<Image>();
        if (rate <= 600) { rateDis.color = Color.green; rateBar.color = Color.green; }
        else if (rate < 2000) { rateDis.color = Color.yellow; rateBar.color = Color.yellow; }
        //else if (rate < 5000) { rateDis.color = Color.red; rateBar.color = Color.red; }
        else if (rate >= 2000) { rateDis.color = Color.red; rateBar.color = Color.red; }//5000
        float barmax = rateBarParent.rectTransform.rect.height;
        float y = 0;
        if (rate < 5000)
        {
            y = (rate / PVC.GetMaxRadsDetectable()) * barmax;
        }
        else { y = barmax;}
        rateBar.rectTransform.transform.localScale = new Vector3(1,y,1);
        //absorbed - meter and text
        dose = PVC.GetAbsorbedDose();
        doseDis.text = Math.Round(dose, 2) + " rads";
        Image doseBar = doseMeter.transform.GetChild(0).GetComponent<Image>();
        if (dose <= 600) { doseDis.color = Color.green; doseBar.color = Color.green; }
        else if (dose <= 2000) { doseDis.color = Color.yellow; doseBar.color = Color.yellow; }
        else if (dose > 2000) { doseDis.color = Color.red; doseBar.color = Color.red; }
        float dosebar = rateBarParent.rectTransform.rect.height;
        float y2 = 0;
        if(dose < 5000f) {
            y2 = (dose / 5000f) * dosebar;
        }
        else{ y2 = dosebar; }
        doseBar.rectTransform.transform.localScale = new Vector3(1, y2, 1);
        //absorbed - player ref
        Color[] colors;
        if(dose <= 300)
        {
            colors = new[] { Color.green, Color.green, Color.green, Color.green, Color.green, Color.green };
        }
        else if (dose <= 600)
        {
            colors = new[] { Color.green, Color.green, Color.yellow, Color.yellow, Color.yellow, Color.yellow };
        }
        else if(dose <= 1000)
        {
            colors = new[] { Color.yellow, Color.yellow, Color.yellow, Color.yellow, Color.yellow, Color.yellow};
        }
        else if (dose <= 2000)
        {
            colors = new[] { Color.yellow, Color.yellow, Color.red, Color.red, Color.red, Color.red };
        }
        else if(dose > 2000 && dose < 5000)
        {
            colors = new[] { Color.red, Color.red, Color.red, Color.red, Color.red, Color.red };
        }
        else{
            colors = new[] { Color.black, Color.black, Color.black, Color.black, Color.black, Color.black };
        }
        SetBodyColor(colors);
        //time
        stayTime = PVC.GetExposureTime();
        StayTimeDis.text = TimeSpan.FromSeconds(stayTime).ToString(@"hh\:mm\:ss");
        //highest dose rate
        if (rate > peakRate)
        {
            peakRate = rate;
        }
        peakRateDis.text = peakRate+"";
    }

    private void SetBodyColor(Color[] colors)
    {
        for(int i = 0;i < BodyHealthDis.Length; i++)
        {
            BodyHealthDis[i].GetComponent<Image>().color = colors[i];
        }
    }
}
