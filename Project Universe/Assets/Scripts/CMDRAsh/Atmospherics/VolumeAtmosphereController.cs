using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using PU_Gases;

public class VolumeAtmosphereController : MonoBehaviour
{
    private float roomPressure;
    [SerializeField] float pressureReport;
    public float roomTemp;
    public float roomOxygenation;
    private float roomVolume;
    public float humidity;
    public float toxicity;
    private List<IGas> roomGases = new List<IGas>();
    private List<IGas> gasesToEq = new List<IGas>();
    [SerializeField] private GameObject[] neighborEmpties;
    //[SerializeField] private GameObject[] roomVolumeDoors;
    private List<GameObject> connectedNeighbors = new List<GameObject>();
    [SerializeField] private int OxygenatedRoom_Priority = 10;
    [SerializeField] private int DeOxygenatedRoom_Priority = 9;

    private void Start()
    {
        roomVolume = (gameObject.GetComponent<BoxCollider>().size.x *
            gameObject.GetComponent<BoxCollider>().size.y *
            gameObject.GetComponent<BoxCollider>().size.z);
    }

    ///check doors
    ///if two doors are open
    ///check the two volumes
    ///go through equalization.
    void Update()
    {
        //combine all same gasses in the volume
        if (roomGases.Count > 1)
        {
            roomGases = CheckGasses(false,0.0f);
        }
        //check for the surround volumes
        //bool[] doorstates = DoorStates();
        for (int i = 0; i < neighborEmpties.Length; i++)//roomVolumeDoors.Length
        {
            GameObject door = neighborEmpties[i].GetComponent<VolumeNode>().GetDoor();
            //if the door (in this volume) is open
            if (door.GetComponent<DoorAnimator>().OpenOrOpening())//roomVolumeDoors
            {
                Vector3 back = door.transform.TransformDirection(Vector3.back);//is Vector3.back for all cases?
                //raycast to check neighbor door
                Debug.DrawRay(
                    new Vector3(door.transform.position.x,
                    door.transform.position.y + 0.025f,
                    door.transform.position.z),
                    back, Color.blue, 1.0f);
                //if a 1m raycast hits an object collider
                if (Physics.Raycast(
                    new Vector3(door.transform.position.x,
                    door.transform.position.y + 0.025f,
                    door.transform.position.z),
                    back, out RaycastHit hit, 1.0f))//roomVolumeDoors[i].transform.position
                {
                    //check if it's a door
                    //select the parent object via the DoorAnimator
                    bool clear = false;
                    Component myComponent = hit.collider.GetComponentInParent<DoorAnimator>();
                    Component myComponent2 = hit.collider.GetComponent<DoorAnimator>();
                    Component myComponent3 = hit.collider.GetComponentInChildren<DoorAnimator>();
                    try
                    {
                        GameObject myDoorGameobject = null;
                        if (myComponent != null)
                        {
                            myDoorGameobject = myComponent.gameObject;
                        }
                        else if(myComponent2 != null)
                        {
                            myDoorGameobject = myComponent2.gameObject;
                        }
                        else if(myComponent3 != null)
                        {
                            myDoorGameobject = myComponent3.gameObject;
                        }
                        bool isD2Open = myDoorGameobject.GetComponent<DoorAnimator>().OpenOrOpening();
                        clear = true;
                    }
                    catch (Exception e) 
                    {
                        Debug.Log(e);
                        Debug.Log("Case 1: " + myComponent);
                        Debug.Log("Case 2: " + myComponent2);
                        Debug.Log("Case 3: " + myComponent3);
                    }
                    
                    if (clear)
                    {
                        //Begin Equalization
                        GameObject localNeighbor = neighborEmpties[i].GetComponent<VolumeNode>().GetVolumeLink();
                        GameObject globalNeighbor = neighborEmpties[i].GetComponent<VolumeNode>().GetGlobalLink();
                        if (localNeighbor != null)
                        {
                            VolumeAtmosphereController iNeighborVolume = neighborEmpties[i].GetComponent<VolumeNode>().GetVolumeLink().GetComponent<VolumeAtmosphereController>();
                            LocalVolumeEqualizer(this, iNeighborVolume);
                        }
                        else if (globalNeighbor)
                        {
                            VolumeGlobalAtmosphereController iGlobalNeighbor = neighborEmpties[i].GetComponent<VolumeNode>().GetGlobalLink().GetComponent<VolumeGlobalAtmosphereController>();
                            GlobalVolumeEqualizer(this, iGlobalNeighbor);
                        }
                    }

                }
            }
        }
    }

    public void LocalVolumeEqualizer(VolumeAtmosphereController VACa, VolumeAtmosphereController VACb)
    {
        float pressureEq;
        float OxEq;
        float tempEq;
        float tempTox;

        float VACaVolume = (VACa.gameObject.GetComponent<BoxCollider>().size.x *
              VACa.gameObject.GetComponent<BoxCollider>().size.y *
              VACa.gameObject.GetComponent<BoxCollider>().size.z);

        float VACbVolume = (VACb.gameObject.GetComponent<BoxCollider>().size.x *
              VACb.gameObject.GetComponent<BoxCollider>().size.y *
              VACb.gameObject.GetComponent<BoxCollider>().size.z);
        float TotalVolume = VACaVolume + VACbVolume;
        //Debug.Log("VCAa volume: " + VACaVolume);
        //Debug.Log("VCAb volume: " + VACbVolume);

        //basic equalization of pressure and oxygen
        if (VACa.roomPressure != VACb.roomPressure)
        {
            //Peq = Vpt / Vt
            //Vpt = Vp1 + Vp2
            float Vp1 = VACaVolume * VACa.roomPressure;
            float Vp2 = VACbVolume * VACb.roomPressure;
            float Vpt = Vp1 + Vp2;
            pressureEq = (Vpt / TotalVolume);
            double presEq = Math.Round(pressureEq, 3);
            pressureEq = (float)presEq;
            VACa.roomPressure = pressureEq;
            VACb.roomPressure = pressureEq;
        }
        if(VACa.roomOxygenation != VACb.roomOxygenation)
        {
            float Voxy1 = VACaVolume * VACa.roomOxygenation;
            float Voxy2 = VACbVolume * VACb.roomOxygenation;
            float Vpt = Voxy1 + Voxy2;
            OxEq = (Vpt / TotalVolume);
            double oxyQ = Math.Round(OxEq, 3);
            OxEq = (float)oxyQ;
            VACa.roomOxygenation = OxEq;
            VACb.roomOxygenation = OxEq;
        }
        if(VACa.roomTemp != VACb.roomTemp)
        {
            float Vtemp1 = VACaVolume * VACa.roomTemp;
            float Vtemp2 = VACbVolume * VACb.roomTemp;
            float Vpt = Vtemp1 + Vtemp2;
            tempEq = (Vpt / TotalVolume);
            double TempQ = Math.Round(tempEq, 3);
            tempEq = (float)TempQ;
            VACa.roomTemp = tempEq;
            VACb.roomTemp = tempEq;
        }
        if (VACa.toxicity != VACb.toxicity)
        {
            float Vtox1 = VACaVolume * VACa.toxicity;
            float Vtox2 = VACbVolume * VACb.toxicity;
            float Vtt = Vtox1 + Vtox2;
            tempTox = (Vtt / TotalVolume);
            double TempQ = Math.Round(tempTox, 3);
            tempTox = (float)TempQ;
            VACa.roomTemp = tempTox;
            VACb.roomTemp = tempTox;
        }
        PostProcessVolumeUpdate();
    }

    public void GlobalVolumeEqualizer(VolumeAtmosphereController VAC, VolumeGlobalAtmosphereController VGAC)
    {
        if (VAC.roomPressure != VGAC.GetPressure())
        {
            VAC.roomPressure = VGAC.GetPressure();
        }
        if (VAC.roomOxygenation != VGAC.roomOxygenation)
        {
            VAC.roomOxygenation = VGAC.roomOxygenation;
        }
        if (VAC.roomTemp != VGAC.roomTemp)
        {
            VAC.roomTemp = VGAC.roomTemp;
        }
        if(VAC.toxicity != VGAC.toxicity)
        {
            VAC.toxicity = VGAC.toxicity;
        }
        PostProcessVolumeUpdate();
    }

    /// <summary>
    /// This method is responsable for adjusting the weight of the Deoxygenated VP and the Oxygenated VP. This will be Lerped between 0 and 1 using roomPressure as value.
    /// </summary>
    public void PostProcessVolumeUpdate()
    {
        Volume[] PPEVS = GetComponents<Volume>();
        for(int i = 0; i < PPEVS.Length; i++)
        {
            if (PPEVS[i].priority == OxygenatedRoom_Priority)
            {
                
                PPEVS[i].weight = roomPressure;//asserting that roomPressure is between 0 and 1
            }
            else if (PPEVS[i].priority == DeOxygenatedRoom_Priority)
            {
                PPEVS[i].weight = 1 - roomPressure;//assume that deoxy is inverse of room pressure
            }
        }
    }

    public List<GameObject> GetConnectedNeighbors()
    {
        return connectedNeighbors;
    }

    public void SetConnectedNeighbors(List<GameObject> newNeighbors)
    {
        connectedNeighbors = newNeighbors;
    }

    void OnTriggerEnter(Collider other)
    {
        //if (other.gameObject.CompareTag("_VolumeNode"))
        //{
        //    Debug.Log("VolumeNode detected with/in "+other.gameObject.name);
        //}
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerVolumeController player = other.GetComponent<PlayerVolumeController>();
            player.OnVolumeEnter(roomPressure,roomTemp,roomOxygenation);
            player.SetPlayerVolume(this.GetComponents<Volume>());
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("_VolumeNode"))
        {
            //Add to list to compare. Whatever exists in VAC is removed from VGAC
            if (!connectedNeighbors.Contains(other.gameObject))
            {
                //Debug.Log(this.name + " detected VolumeNode: " + other.gameObject.name);
                connectedNeighbors.Add(other.gameObject);
                other.GetComponent<VolumeNode>().SetVolumeLink(this.gameObject);
            }
        }
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerVolumeController player = other.GetComponent<PlayerVolumeController>();
            player.OnVolumeEnter(roomPressure, roomTemp, roomOxygenation);
            player.SetPlayerVolume(this.GetComponents<Volume>());
        }
    }

    public List<IGas> CheckGasses(bool setToLocalPressure,float localPressure)
    {
        if (roomGases.Count > 1)
        {
            //Debug.Log(roomGases.Count + " gasses in pipe.");
            List<IGas> newGassesList = roomGases;//new List<IGas>();
            //combine all same gasses
            for (int i = 0; i < newGassesList.Count; i++)
            {
                for (int j = 0; j < newGassesList.Count; j++)
                {
                    if (i != j)
                    {
                        if (newGassesList[i].GetIDName() == newGassesList[j].GetIDName())
                        {
                            IGas EQgas = CombineGases(roomGases[i], roomGases[j],localPressure,setToLocalPressure);
                            newGassesList.Remove(roomGases[i]);
                            newGassesList.Remove(roomGases[j - 1]);
                            newGassesList.Add(EQgas);
                        }
                    }
                }
            }
            foreach (IGas gas in newGassesList)
            {
                //Debug.Log("EQM result: "+gas.ToString());
            }
            return newGassesList;
        }
        else
        {
            //gasses is empty or only has one has in it
            return roomGases;
        }
    }
    public IGas CombineGases(IGas gasA, IGas gasB, float localPressure,bool setToLocalPressure)
    {
        float gasTemp;
        float gasConc;
        float gasPressure;

        float gasAt = gasA.GetTemp();
        float gasBt = gasB.GetTemp();
        gasTemp = (gasAt + gasBt) / 2;
        gasA.SetTemp(gasTemp);

        gasConc = gasA.GetConcentration() + gasB.GetConcentration();
        gasA.SetConcentration(gasConc);

        if (setToLocalPressure)
        {
            gasA.SetLocalPressure(localPressure);
        }
        else
        {
            float gasAp = gasA.GetLocalPressure();
            float gasBp = gasB.GetLocalPressure();
            gasPressure = (gasAp + gasBp);
            gasA.SetLocalPressure(gasPressure);
        }

        gasA.CalculateAtmosphericDensity();
        //Debug.Log("Volume Gas Combiner: "+gasPressure);
        return gasA;
    }

    /// <summary>
    /// Add the parameter gas to the room's gas array. Update Volume Atmosphere.
    /// </summary>
    /// <param name="gasToAdd"></param>
    public void AddRoomGas(IGas gasToAdd)
    {
        gasToAdd.SetLocalVolume(roomVolume);
        //gasesToEq.Add(gasToAdd);
        roomGases.Add(gasToAdd);
        //roomGases = CheckGasses();
        //update Volume Atmosphere
        float totalGas = CalculateRoomOxygenation();
        CalculateRoomTemp();
        CalculateRoomPressure(totalGas);
    }

    /// <summary>
    /// recalc room oxygenation. It is the ratio of oxygen (in m3) to room volume (m3).
    /// Gasses that are not oxygen do not count towards oxygenation.
    /// Returns: The total amount of gasses in the room in m3
    /// </summary>
    public float CalculateRoomOxygenation()
    {
        float oxygenation = 0.0f;
        float totalGasses = 0.0f;
        for(int i = 0;i < roomGases.Count; i++)//gasesToEq
        {
            totalGasses += roomGases[i].GetConcentration();//gasesToEq
            if (roomGases[i].GetIDName() == "Oxygen")
            {
                oxygenation += roomGases[i].GetConcentration();//gasesToEq
            }
        }
        //the above calcs are full oxygenation at 1.0, not 100.0f, so mult by 100
        roomOxygenation = (oxygenation /= roomVolume) * 100f;
        //float oxygenTemp = (oxygenation /= roomVolume)*100f;
        //roomOxygenation += oxygenTemp;
        return totalGasses;
    }

    /// <summary>
    /// Calculate room temp based on the gasses present in the room, and it's ambient heat.
    /// Returns the room temp before temp Eq.
    /// </summary>
    public void CalculateRoomTemp()
    {
        float temperature = 0.0f;
        for (int j = 0; j < roomGases.Count; j++)
        {
            temperature += roomGases[j].GetTemp();
        }
        //Equalized temp of the gases in the room
        roomTemp = temperature / (roomGases.Count);
    }

    /// <summary>
    /// Recalc room pressure based on the concentration of gas present in the volume, and it's temperature.
    /// </summary>
    public void CalculateRoomPressure(float totalRoomGasses_m3)
    {
        ///calculate pressure in the room according to the gas vars.
        ///IE The amount of gasses in the room where >x is 1.x atm and less than x is 0.x atm
        ///THEN adjust for temp

        float concPressure = totalRoomGasses_m3 / roomVolume;
        //t1 = originalTemp; t2 = roomTemp
        //p1 = roomPressure; p2 = X
        //v1 = roomVolume; v2 = roomVolume
        float p2 = 0.0f;
        foreach (IGas gas in roomGases)
        {
            float p1 = concPressure;
            float v1 = gas.GetLocalVolume();
            float t1 = ((gas.GetTemp() - 32f) * (5f / 9f)) + 273.15f;
            float v2 = gas.GetLocalVolume();
            float t2 = ((roomTemp - 32f) * (5f / 9f)) + 273.15f;
            //Debug.Log("p1: " +p1 + " v1: " +v1+ " t1: " +t1 + " v2: " +v2+ " t2: "+t2);
            //temp-adjusted pressure for one gas in the room
            p2 += (p1 * v1 * t2) / (t1 * v2);
        }
        roomPressure = p2 / roomGases.Count;
        pressureReport = roomPressure;
        //Debug.Log("Pressure recal: " + roomPressure);
        foreach (IGas setGases in roomGases)
        {
            setGases.SetLocalPressure(roomPressure);
            setGases.SetTemp(roomTemp);
            setGases.CalculateAtmosphericDensity();
        }
        roomGases = CheckGasses(true,roomPressure);
    }

    

    public float GetPressure()
    {
        return roomPressure;
    }
    public void SetPressure(float value)
    {
        roomPressure = value;
    }
    public float GetVolume()
    {
        return roomVolume;
    }
}
