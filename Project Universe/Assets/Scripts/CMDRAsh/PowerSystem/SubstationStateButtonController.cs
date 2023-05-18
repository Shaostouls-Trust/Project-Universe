using ProjectUniverse.PowerSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SubstationStateButtonController : MonoBehaviour
{
    [SerializeField] private IRoutingSubstation substation;
    public TMP_Text nameText;
    public TMP_Text lastInText;
    public TMP_Text lastReqText;

    public void SetData(IRoutingSubstation rout)
    {
        substation = rout;
        nameText.text = substation.stationName;
    }

    // Update is called once per frame
    void Update()
    {
        if (substation != null)
        {
            lastInText.text = Math.Round(substation.LastReceived, 2).ToString();
            lastReqText.text = Math.Round(substation.TotalRequiredPower, 2).ToString();
        }
    }

    public void ExternalInteractFunc(int param)
    {
        if (param == 0)
        {
            if (substation != null)
            {
                substation.CanRequestEnergy = true;
            }
        }
        else if (param == 1)
        {
            if (substation != null)
            {
                substation.CanRequestEnergy = false;
            }
        }
    }
}
