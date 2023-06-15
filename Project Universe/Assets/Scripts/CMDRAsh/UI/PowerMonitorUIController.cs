using ProjectUniverse.PowerSystem;
using ProjectUniverse.PowerSystem.Nuclear;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class PowerMonitorUIController : MonoBehaviour
{
    [SerializeField] private SteamTurbine[] turbines;
    [SerializeField] private IRouter router1;
    [SerializeField] private IRouter router2;
    [SerializeField] private IRoutingSubstation[] router1subs;
    [SerializeField] private IRoutingSubstation[] router2subs;
    [SerializeField] private TMP_Text turbine1In;
    [SerializeField] private TMP_Text turbine2In;
    [SerializeField] private TMP_Text turbine3In;
    [SerializeField] private TMP_Text turbine4In;
    [SerializeField] private Image router1On;
    [SerializeField] private Image router1Off;
    [SerializeField] private TMP_Text router1In;
    [SerializeField] private TMP_Text router1Out;
    [SerializeField] private Image router2On;
    [SerializeField] private Image router2Off;
    [SerializeField] private TMP_Text router2In;
    [SerializeField] private TMP_Text router2Out;
    [SerializeField] private Image sub1On;
    [SerializeField] private Image sub1Off;
    [SerializeField] private TMP_Text sub1Buffer;
    [SerializeField] private TMP_Text sub1In;
    [SerializeField] private TMP_Text sub1Out;
    [SerializeField] private Image sub2On;
    [SerializeField] private Image sub2Off;
    [SerializeField] private TMP_Text sub2Buffer;
    [SerializeField] private TMP_Text sub2In;
    [SerializeField] private TMP_Text sub2Out;
    [SerializeField] private Image sub5On;
    [SerializeField] private Image sub5Off;
    [SerializeField] private TMP_Text sub5Buffer;
    [SerializeField] private TMP_Text sub5In;
    [SerializeField] private TMP_Text sub5Out;
    [SerializeField] private Image sub6On;
    [SerializeField] private Image sub6Off;
    [SerializeField] private TMP_Text sub6Buffer;
    [SerializeField] private TMP_Text sub6In;
    [SerializeField] private TMP_Text sub6Out;
    [SerializeField] private Color32 green;
    [SerializeField] private Color32 darkgreen;
    [SerializeField] private Color32 red;
    [SerializeField] private Color32 darkred;

    // Update is called once per frame
    void OnGUI()
    {
        //if null, then we're drawing power from a turbine
        if(!router1.UseGeneratorPower)
        {
            router1On.color = green;
            router1Off.color = darkred;
        }
        else
        {
            router1On.color = darkgreen;
            router1Off.color = red;
        }

        if (!router2.UseGeneratorPower)//ConnectedTurbine != null
        {
            router2On.color = green;
            router2Off.color = darkred;
        }
        else
        {
            router2On.color = darkgreen;
            router2Off.color = red;
        }

        if (router1subs[0].CanRequestEnergy)
        {
            sub1On.color = green;
            sub1Off.color = darkred;
        }
        else
        {
            sub1On.color = darkgreen;
            sub1Off.color = red;
        }

        if (router1subs[1].CanRequestEnergy)
        {
            sub2On.color = green;
            sub2Off.color = darkred;
        }
        else
        {
            sub2On.color = darkgreen;
            sub2Off.color = red;
        }

        if (router2subs[0].CanRequestEnergy)
        {
            sub5On.color = green;
            sub5Off.color = darkred;
        }
        else
        {
            sub5On.color = darkgreen;
            sub5Off.color = red;
        }

        if (router2subs[1].CanRequestEnergy)
        {
            sub6On.color = green;
            sub6Off.color = darkred;
        }
        else
        {
            sub6On.color = darkgreen;
            sub6Off.color = red;
        }

        turbine1In.text = Mathf.Round(turbines[0].PowerOutput) + " MWe";
        turbine2In.text = Mathf.Round(turbines[1].PowerOutput) + " MWe";
        turbine3In.text = Mathf.Round(turbines[2].PowerOutput) + " MWe";
        turbine4In.text = Mathf.Round(turbines[3].PowerOutput) + " MWe";

        if (router1.LastReceived > 10000f)
        {
            router1In.text = Mathf.Round(router1.LastReceived / 10000f) + " MWe";
        }
        else
        {
            router1In.text = Mathf.Round(router1.LastReceived) + " We";
        }
        if (router2.LastReceived > 10000f)
        {
            router2In.text = Mathf.Round(router1.LastReceived/10000f) + " MWe";
        }
        else
        {
            router2In.text = Mathf.Round(router1.LastReceived) + " We";
        }

        router1Out.text = Mathf.Round(router1.getTotalRequiredPower()) + " We";
        router2Out.text = Mathf.Round(router1.getTotalRequiredPower()) + " We";

        sub1Buffer.text = Math.Round(router1subs[0].BufferCurrent,1) + " We";
        sub2Buffer.text = Math.Round(router1subs[1].BufferCurrent,1) + " We";
        sub5Buffer.text = Math.Round(router2subs[0].BufferCurrent,1) + " We";
        sub6Buffer.text = Math.Round(router2subs[1].BufferCurrent,1) + " We";

        sub1In.text = Math.Round(router1subs[0].LastReceived,1) + " We";
        sub2In.text = Math.Round(router1subs[1].LastReceived,1) + " We";
        sub5In.text = Math.Round(router2subs[0].LastReceived,1) + " We";
        sub6In.text = Math.Round(router2subs[1].LastReceived,1) + " We";

        sub1Out.text = Math.Round(router1subs[0].TotalRequiredPower,1) + " We";
        sub2Out.text = Math.Round(router1subs[1].TotalRequiredPower,1) + " We";
        sub5Out.text = Math.Round(router2subs[0].TotalRequiredPower,1) + " We";
        sub6Out.text = Math.Round(router2subs[1].TotalRequiredPower,1) + " We";
    }
}
