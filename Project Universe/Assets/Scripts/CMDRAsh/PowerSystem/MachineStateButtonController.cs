using ProjectUniverse.PowerSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MachineStateButtonController : MonoBehaviour
{
    [SerializeField] private IMachine machine;
    [SerializeField] private IBreakerBox breaker;
    public TMP_Text nameText;
    public TMP_Text lastInText;
    public TMP_Text lastReqText;

    public void SetData(IMachine mach)
    {
        machine = mach;
        nameText.text = machine.gameObject.name;
    }
    public void SetData(IBreakerBox brk)
    {
        breaker = brk;
        nameText.text = breaker.gameObject.name;
    }

    /// <summary>
    /// Get machine/breaker data and update UI
    /// </summary>
    private void Update()
    {
        if (machine != null)
        {
            //update lastInText with the machine's last received power
            lastInText.text = Math.Round(machine.LastReceived,2).ToString();
            lastReqText.text = Math.Round(machine.RequestedEnergyAmount(),2).ToString();
        }
        else if(breaker != null)
        {
            lastInText.text = Math.Round(breaker.LastReceived,2).ToString();
            lastReqText.text = Math.Round(breaker.GetTotalRequiredPower(),2).ToString();
        }
    }

    public void ExternalInteractFunc(int param)
    {
        if (param == 0)
        {
            if (machine != null)
            {
                machine.RunMachine = true;
            }
            else
            {
                breaker.RunMachine = true;
            }
        }
        else if (param == 1)
        {
            if (machine != null)
            {
                machine.RunMachine = false;
            }
            else
            {
                breaker.RunMachine = false;
            }
        }
    }
}
