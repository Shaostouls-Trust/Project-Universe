using ProjectUniverse.Environment.Fluid;
using ProjectUniverse.Util;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FluidTankUIController : MonoBehaviour
{
    [SerializeField] private IFluidTank tank;
    [SerializeField] private Image valveImgOn;
    [SerializeField] private Image valveImgOff;
    [SerializeField] private RectTransform fluidTankScale;
    [SerializeField] private TMP_Text fluidRes;
    [SerializeField] private TMP_Text flowRate;
    [SerializeField] private Transform dial;
    [Space]
    [SerializeField] private Color32 GREENON = new Color32(0, 170, 50, 255);
    [SerializeField] private Color32 GREENOFF = new Color32(0, 50, 0, 255);
    [SerializeField] private Color32 REDON = new Color32(255, 0, 0, 255);
    [SerializeField] private Color32 REDOFF = new Color32(50, 0, 0, 255);
    [SerializeField] private Color32 YELLOWON = new Color32(255, 225, 0, 255);
    [SerializeField] private Color32 YELLOWOFF = new Color32(51, 43, 0, 255);
    [Space]
    [SerializeField] private Image warningA;
    [SerializeField] private Image warningB;
    [SerializeField] private Image warningC;
    [SerializeField] private Image warningD;
    [SerializeField] private Image warningE;
    [SerializeField] private Image warningF;
    [SerializeField] private Image warningG;
    private float timer = 0.25f;

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer = 0.25f;
            if (tank.ValveState)
            {
                valveImgOn.color = GREENON;
                valveImgOff.color = REDOFF;
            }
            else
            {
                valveImgOn.color = GREENOFF;
                valveImgOff.color = REDON;
            }
            float reservoirPer = tank.FluidLevel / tank.FluidCapacity;
            fluidRes.text = Mathf.Round(reservoirPer * 100f) + "%";
            fluidTankScale.localScale = new Vector3(1f, reservoirPer, 1f);
            flowRate.text = Mathf.Round(Utils.CalculateFluidFlowThroughPipe(tank.OutflowPipe.InnerDiameter
                , tank.FlowVelocity) / 3600f) + " m^3/s Max";
        }

        ///Warnings
        ///A - ValveFail: The valve is non-operable
        ///B - Tank Breach: Tank is leaking fluid.
        ///C - No Water: The fluid reservoir is empty.
        ///D - Manual Mode: Tank outflow pressure and velocity manually set
        ///E - No Inflow: No fluid is entering tank
        ///F - Water Low: fluid res is below 25%
        ///G - No Outflow: No fluid is leaving the tank
        ///

        if (!tank.ValveOperable)
        {
            warningA.color = REDON;
        }
        else
        {
            warningA.color = REDOFF;
        }

        //breach by damage
        warningB.color = REDOFF;

        if(tank.FluidLevel <= 0f)
        {
            warningC.color = REDON;
        }
        else
        {
            warningC.color = REDOFF;
        }

        if (!tank.AutomaticMode)
        {
            warningD.color = YELLOWON;
        }
        else
        {
            warningD.color = YELLOWOFF;
        }

        if(tank.InletRate <= 0f)
        {
            warningE.color = YELLOWON;
        }
        else
        {
            warningE.color = YELLOWOFF;
        }

        if(tank.FluidLevel <= (tank.FluidCapacity * 0.25f))
        {
            warningF.color = YELLOWON;
        }
        else
        {
            warningF.color = YELLOWOFF;
        }

        if (tank.Outletrate <= 0f)
        {
            warningG.color = YELLOWON;
        }
        else
        {
            warningG.color = YELLOWOFF;
        }
    }

    public void ExternalInteractFunc(int i)
    {
        if(i == 1)
        {
            tank.ValveState = !tank.ValveState;
        }
        else if(i==3)
        {
            //velocity up
            tank.FlowVelocity += 10;
            dial.localRotation = Quaternion.Euler(0f, dial.localRotation.eulerAngles.y + 10f, 0f);
            if (tank.FlowVelocity > tank.FlowVelocityMax)
            {
                tank.FlowVelocity = tank.FlowVelocityMax;
            }

        }
        else if (i == 2)
        {
            //velocity down
            tank.FlowVelocity -= 10;
            dial.localRotation = Quaternion.Euler(0f, dial.localRotation.eulerAngles.y - 10f, 0f);
            if (tank.FlowVelocity < 0f)
            {
                tank.FlowVelocity = 0f;
            }
        }
        else if (i == 4)
        {
            tank.AutomaticMode = !tank.AutomaticMode;
        }
    }


}
