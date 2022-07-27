using ProjectUniverse.Animation.Controllers;
using ProjectUniverse.Environment.Gas;
using ProjectUniverse.Environment.Volumes;
using ProjectUniverse.PowerSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AirlockAtmosphereUI : MonoBehaviour
{
    [SerializeField] private TMP_Text presTxt;
    [SerializeField] private TMP_Text oxTxt;
    [SerializeField] private TMP_Text extPresTxt;
    [SerializeField] private TMP_Text extOxTxt;
    [SerializeField] private Color32 GREEN;
    [SerializeField] private Color32 YELLOW;
    [SerializeField] private Color32 RED;
    [SerializeField] private VolumeAtmosphereController airlockVac;
    // VAC of the volume we are (de)pressurizing to
    private VolumeAtmosphereController extVac;//will equal null if to space
    [SerializeField] private VolumeNode door1Node;
    [SerializeField] private VolumeNode door2Node;
    private DoorAnimator door1;
    private DoorAnimator door2;
    private float timer = 0.5f;
    private bool adjustingPressure = false;
    private GameObject extDoor;//the door to the volume we are (de)pressurizing to

    private void Start()
    {
        door1 = door1Node.GetDoor().GetComponent<DoorAnimator>();
        door1.Locked = true;
        door2 = door2Node.GetDoor().GetComponent<DoorAnimator>();
        door2.Locked = true;
    }
    
    private void Update()
    {
        timer -= Time.deltaTime;
    }

    void OnGUI()
    {
        if (timer <= 0f)
        {
            // switch extVac around temp
            VolumeAtmosphereController temp = null;
            if (extDoor == door1Node.GetDoor())
            {
                //switch to door2 stuff
                if (door2Node.VolumeLink != null)
                {
                    temp = door2Node.VolumeLink.GetComponent<VolumeAtmosphereController>();                                                                    
                }
            }
            else
            {
                //switch to door1 stuff
                if (door1Node.VolumeLink != null)
                {
                    temp = door1Node.VolumeLink.GetComponent<VolumeAtmosphereController>();
                }
            }

            timer = 0.5f;
            float pressure = (float)Math.Round(airlockVac.Pressure, 2);
            presTxt.text = "" + pressure;
            if (pressure <= 0.5f && pressure > 0.25f)
            {
                presTxt.color = YELLOW;
            }
            else if (pressure <= 0.25f)
            {
                presTxt.color = RED;
            }
            else
            {
                presTxt.color = GREEN;
            }

            float ox = (float)Math.Round(airlockVac.Oxygenation, 2);
            oxTxt.text = "" + ox;
            if (ox <= 60.0f && ox > 30.0f)
            {
                oxTxt.color = YELLOW;
            }
            else if (ox <= 30f)
            {
                oxTxt.color = RED;
            }
            else
            {
                oxTxt.color = GREEN;
            }

            if (temp != null)
            {
                float extOx = (float)Math.Round(temp.Pressure, 2);
                extPresTxt.text = "" + extOx;
                if (extOx <= 0.5f && extOx > 0.25f)//<= 60.0f && extOx > 30.0f
                {
                    extPresTxt.color = YELLOW;
                }
                else if (extOx <= 0.25f)//<= 30.0f
                {
                    extPresTxt.color = RED;
                }
                else
                {
                    extPresTxt.color = GREEN;
                }

                float extPr = (float)Math.Round(temp.Oxygenation, 2);
                extOxTxt.text = "" + extPr;
                if (extPr <= 60.0 && extPr > 30.0f)//<= 0.5f && extPr > 0.25f
                {
                    extOxTxt.color = YELLOW;
                }
                else if (extPr <= 30.0f)// <= 0.25f
                {
                    extOxTxt.color = RED;
                }
                else
                {
                    extOxTxt.color = GREEN;
                }
            }
            else
            {
                extPresTxt.text = "0.0";
                extOxTxt.text = "0.0";
                extOxTxt.color = RED;
                extPresTxt.color = RED;
            }
        }
    }

    // cycle the airlock
    public void ExternalInteractFunc(int a)
    {
        //Debug.Log("Button");
        //cycle from inside
        if (a == 0)
        {
            // lock both doors
            if (door1.Open)
            {
                door1.Locked = false;
                door1.CloseDoor();
            }
            door1.Locked = true;
            if (door2.Open)
            {
                door2.Locked = false;
                door2.CloseDoor();
            }
            door2.Locked = true;
            //cycle pressure - extVac and extDoor should have already been set?
            if (extVac == null)
            {
                //Debug.Log("Pressure to 0f");
                StartCoroutine(PressurizeAirlock(0f));
            }
            else
            {
                //Debug.Log("Pressure to " + extVac.Pressure);
                StartCoroutine(PressurizeAirlock(extVac.Pressure));
            }
        }
        //cycle from door1
        // push button 1, close door 2, adjust pressure to side of door 1, open door 1
        else if (a == 1)
        {
            if (!adjustingPressure) 
            {
                //if door2 is open, close and lock it
                if (!door2.Locked)
                {
                    door2.CloseDoor();
                    door2.Locked = true;
                }
                else
                {
                    door2.Locked = false;
                    door2.CloseDoor();
                    door2.Locked = true;
                }
                //Debug.Log("Locked door 2");
                //pressure of the volume the player is entering from
                if (door1Node.VolumeLink != null)
                {
                    extVac = door1Node.VolumeLink.GetComponent<VolumeAtmosphereController>();
                    extDoor = door1Node.GetDoor();
                }
                else
                {
                    extVac = null;
                    extDoor = door1Node.GetDoor();
                }
                if (extVac == null)
                {
                    //Debug.Log("Pressure to 0f");
                    StartCoroutine(PressurizeAirlock(0f));
                    //if the airlock interior pressure is the pressure at the exterior of door 2
                    //adjust pressure to side of door 2, open door 2
                }
                else
                {
                    //Debug.Log("Pressure to "+extVac.Pressure);
                    StartCoroutine(PressurizeAirlock(extVac.Pressure));
                } 
            }
        }
        //cycle from door2
        // push button 2, close door 1, adjust pressure to side of door 2, open door 2
        else if (a == 2)
        {
            if (!adjustingPressure)
            {
                // if door1 is open, close it
                if (!door1.Locked)
                {
                    door1.CloseDoor();
                    door1.Locked = true;
                }
                else
                {
                    door1.Locked = false;
                    door1.CloseDoor();
                    door1.Locked = true;
                }
                //Debug.Log("Locked door 1");
                if(door2Node.VolumeLink != null)
                {
                    extVac = door2Node.VolumeLink.GetComponent<VolumeAtmosphereController>();
                    extDoor = door2Node.GetDoor();
                }
                else
                {
                    extVac = null;
                    extDoor = door2Node.GetDoor();
                }
                if (extVac == null)
                {
                    //Debug.Log("Pressure to 0f");
                    StartCoroutine(PressurizeAirlock(0f));
                }
                else
                {
                    //Debug.Log("Pressure to " + extVac.Pressure);
                    StartCoroutine(PressurizeAirlock(extVac.Pressure));
                }
            }
        }
    }

    // pressurize the airlock
    private IEnumerator PressurizeAirlock(float pressureTarget)
    {
        // switch extVac around to the vac of the other door
        
        if (extDoor == door1Node.GetDoor())
        {
            extDoor = door2Node.GetDoor();
            //switch to door2 stuff
            if (door2Node.VolumeLink != null)
            {
                extVac = door2Node.VolumeLink.GetComponent<VolumeAtmosphereController>();//2
                //Debug.Log(extVac.ToString());
            }
            else
            {
                extVac = null;
                //Debug.Log(extVac);
            }
        }
        else
        {
            //switch to door1 stuff
            extDoor = door1Node.GetDoor();
            if (door1Node.VolumeLink != null)
            {
                extVac = door1Node.VolumeLink.GetComponent<VolumeAtmosphereController>();
            }
            else
            {
                extVac = null;
            }
            //Debug.Log(extVac.ToString());
        }
        
        
        //Debug.Log("Beginning CoRtn");
        while (pressureTarget != airlockVac.Pressure)
        {
            adjustingPressure = true;

            //create new oxygengas (for now, later it will come from tanks or outside)
            float pres = (float)Math.Round((0.5f * Time.deltaTime), 3);
            IGas gasTest = new IGas("Oxygen", 70.0f, (float)Math.Round((2.5f * Time.deltaTime), 3), pres, .4f);
            gasTest.CalculateAtmosphericDensity();

            
            if (pressureTarget > airlockVac.Pressure)
            {
                //Debug.Log("Add gas");
                if(pres + pressureTarget > airlockVac.Pressure)
                {
                    gasTest.SetLocalPressure(pres - airlockVac.Pressure - pressureTarget);
                    airlockVac.AddRoomGas(gasTest);
                }
                else
                {
                    airlockVac.AddRoomGas(gasTest);
                }
            }
            else
            {
                Debug.Log("Pressure: " + airlockVac.Pressure + "/" + pressureTarget);
                //Debug.Log("Remove gas");
                //if we've more pressure than a single cycle of gas
                if (pres > airlockVac.Pressure)
                {
                    airlockVac.RemoveRoomGas(gasTest);
                }
                else //remove the remaining gas
                {
                    airlockVac.RemoveRoomGas(airlockVac.Pressure - pressureTarget);
                }
                
            }
            yield return null;
        }
        adjustingPressure = false;

        //pressure is adjusted, open the door
        // if extDoor is equal to door2 then open door 1
        //Debug.Log("Open exterior door");
        if (extDoor == door1Node.GetDoor())
        {
            door2.Locked = false;
            door2.OpenDoor();
        }
        else
        {
            door1.Locked = false;
            door1.OpenDoor();
        }

    }
}
