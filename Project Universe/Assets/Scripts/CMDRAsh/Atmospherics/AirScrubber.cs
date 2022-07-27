using ProjectUniverse.Environment.Gas;
using ProjectUniverse.Environment.Volumes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirScrubber : MonoBehaviour
{
    [SerializeField] private float scrubRate_m3perSec = 1f;
    [SerializeField] VolumeAtmosphereController vacToScrub;
    public bool scrubToxic;
    public bool scrubCombustable;
    public bool scrubRadioactive;
    public bool scrubFlammable;
    public bool scrubNonOxygen;
    private float scrubRateLeft;

    // Update is called once per frame
    void Update()
    {
        scrubRateLeft = scrubRate_m3perSec;
        //for every gas in vacToScrub
        for (int i = 0; i < vacToScrub.RoomGasses.Count; i++)
        {
            if (scrubRate_m3perSec > 0f)
            {
                IGas gas = vacToScrub.RoomGasses[i];
                if (scrubToxic)
                {
                    if (gas.GetToxicity() > 0f)
                    {
                        ScrubGas(gas);
                    }
                }
                if (scrubCombustable)
                {
                    if (gas.GetCombustability() > 4f)
                    {
                        ScrubGas(gas);
                    }
                }
                if (scrubRadioactive)
                {
                    if (gas.GetNuclear())
                    {
                        ScrubGas(gas);
                    }
                }
                if (scrubFlammable)
                {
                    if (gas.GetFlamabitity() > 4f)
                    {
                        ScrubGas(gas);
                    }
                }
                if (scrubNonOxygen)
                {
                    if (gas.GetIDName() != "Oxygen")
                    {
                        ScrubGas(gas);
                    }
                }
            }
            else
            {
                break;
            }
        }
    }

    public void ScrubGas(IGas gas)
    {
        float conc = gas.GetConcentration() - scrubRateLeft * Time.deltaTime;

        scrubRateLeft -= gas.GetConcentration();
        if (scrubRateLeft < 0f)
        {
            scrubRateLeft = 0f;
        }
        //Debug.Log("Scrubbed " + gas.ToString());
        if (conc > 0f)
        {
            gas.SetConcentration(conc);
        }
        else
        {
            vacToScrub.RemoveRoomGas(gas);
        }
    }
}
