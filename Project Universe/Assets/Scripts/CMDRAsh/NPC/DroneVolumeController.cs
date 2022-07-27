using ProjectUniverse.Environment.Volumes;
using ProjectUniverse.Ship;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public sealed class DroneVolumeController : MonoBehaviour
{
    [SerializeField] private TMP_Text pressureText;
    [SerializeField] private TMP_Text oxyText;
    [SerializeField] private TMP_Text tempText;
    [SerializeField] private TMP_Text toxText;
    private float myRoomPressure;
    private float myLastRoomPress;
    private float myRoomTemp;
    private float myRoomOxygenation;
    private float myRoomToxicity;
    //radiation
    private float myRadExposureRateRaw = 0;
    [SerializeField] private float myRadExposureTime = 0;
    private float myRadAbsorbtionRate = 0.9f;
    [SerializeField] private float myRadAbsorbed = 0;
    private float myMaxRoentgenDetectable = 0;
    //Player stats:
    //[SerializeField] private float playerHealth = 100f;
    [SerializeField] private float playerTemp = 98.6f;
    private Volume[] volumesPlayerIsIn;
    private VolumeAtmosphereController playerVolume;
    //public string CurrentVolumeOfPlayer;
    [SerializeField] private ShipControlConsole shipControlConsole;

    [SerializeField] private GameObject radWarnUI;
    [SerializeField] private Image[] radUIElements;
    [SerializeField] private Color32 BLACK;
    [SerializeField] private Color32 YELLOW;
    [SerializeField] private Sprite img1;
    [SerializeField] private Sprite img2;
    private bool c1 = false;
    private bool runningCo;
    [SerializeField] private RectTransform healthbar;
    [SerializeField] private float healthmax;
    private float healthcur;
    private bool canConnect;
    private bool connected;
    
    public bool CanConnect
    {
        get { return canConnect; }
        set { canConnect = value; }
    }
    public bool Connected
    {
        get { return connected; }
        set { connected = value; }
    }

    // Start is called before the first frame update
    void Start()
    {
        healthcur = healthmax;
        myRoomPressure = 1f;
        myLastRoomPress = 1f;
        myRoomTemp = 68f;
        myRoomOxygenation = 100f;
        myRoomToxicity = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        //update player vc to reflect the changes in the volume.
        OnVolumeUpdate();
        /*
        //camera focusing
        Camera maincamera = GetComponentInChildren<Camera>();
        //Debug.DrawRay(maincamera.transform.position, maincamera.transform.TransformVector(Vector3.forward),Color.cyan);
        if (Physics.Raycast(
            maincamera.transform.position,
            maincamera.transform.TransformVector(Vector3.forward),
            out RaycastHit hit, 100.0f))
        {
            Debug.Log(hit.distance);
            for (int i = 0; i < volumesPlayerIsIn.Length; i++)
            {
                if (volumesPlayerIsIn[i].profile.TryGet(out DepthOfField DofComponent)) 
                {
                    DofComponent.focusDistance.value = hit.distance;
                }
            }
        }
        */
        /*
        if (playerOxygen <= 0)
        {
            //take suffocation damage
            shipController.InflictPlayerDamageServerRpc((3.2f - ((myRoomOxygenation / 100f) * 3.2f)) * Time.deltaTime);
            //playerHealth -= (3.2f * Time.deltaTime);//suffocate to death in 30 seconds
        }
        if (myRoomOxygenation < 50.0f)//if the air is too thin
        {
            //float oxyDefecit = 100.0f - myRoomOxygenation;
            //breath X from an air supply
            //if no air supply:
            ///1 minute of holding breath before taking damage at normal heart rate (1.6f)
            ///If partial air, subtract that from the rate
            ///scale down to 10 or so secs depending on heart rate
            if (playerOxygen > 0f)
            {
                playerOxygen -= ((oxyUseRate - ((myRoomOxygenation / 100f)) * oxyUseRate) * Time.deltaTime);
            }
        }
        else if (myRoomOxygenation >= 50.0)
        {
            if (playerOxygen < 100)
            {
                playerOxygen += ((myRoomOxygenation / 100f) * 50 * Time.deltaTime);//2 secs to recover from empty at 100% O2
            }
            if (playerOxygen > 100)
            {
                playerOxygen = 100;
            }
        }*/

        //inflict toxicity damage
        //shipController.InflictPlayerDamageServerRpc(Mathf.Lerp(0.0f, 3.0f, myRoomToxicity));

        //if (myRoomPressure < 0.12f)//~121 millibar. 121 is lowest survivable w/ pure oxygen.
        //{
            //if in pressure gear:
            //if not in pressure gear:
            //playerHealth -= (6.6f * Time.deltaTime);//0 atm kills in 15 secs
        //}
        //if (myLastRoomPress - myRoomPressure > 0.5f)
        //{
            //if the air pressure changed too quickly, take damage or 'black out'
        //}

        //if (myRoomTemp != playerTemp)
        ///need a scalar to apply to playerTemp. Also will need some way to simulate sweating bring the body temp to a stable
        ///temp to ensure that 90*F doesn't cause your character to slowly die of heatstroke.
        ///We also need to ensure that when pressure is zero, you freeze more slowly. Esp in vacuum of space where it takes 12 hoursish for cold to kill you.
        //{
            //slowly adjust player temp to match (use coroutine?)
        //}
        //if (playerTemp < 65f)//critical hypothermia
        //{
        //    shipController.InflictPlayerDamageServerRpc(0.5f * Time.deltaTime);
            //playerHealth -= (0.5f*Time.deltaTime);
        //}
        //update last room pressure
        //myLastRoomPress = myRoomPressure;

        //player DEATH RAWR:
        //if (shipController.PlayerHealth <= 0)
        //{
            //you dead, punk!
        //}

        
        // radiation damage
        // if radiation exposure rate is greater than 700, begin taking damage
        if(GetRadiationExposureRate() >= 700f)
        {
            radWarnUI.SetActive(true);
            if (!runningCo)
            {
                //StartCoroutine(UIFlash());
            }
            healthcur -= ((GetRadiationExposureRate() - 700f) / 10f) * Time.deltaTime;
        }
        else
        {
            radWarnUI.SetActive(false);
        }
        
        if(healthcur >= 0f)
        {
            CanConnect = true;
        }
        else
        {
            CanConnect = false;
            if (Connected)
            {
                Connected = false;
                shipControlConsole.UIExternalInteract();
            }
        }
        
        UpdateUI();
    }

    public void AddRadiationExposureTime(float time)
    {
        myRadExposureTime += time;
    }
    public void SetMaxRadsDetectable(float rnt)
    {
        myMaxRoentgenDetectable = rnt;
    }
    public float GetMaxRadsDetectable()
    {
        return myMaxRoentgenDetectable;
    }
    public void SetRadiationExposureRate(float roentgen)
    {
        myRadExposureRateRaw = roentgen;
    }

    public void CalculateAbsorbedDose()
    {
        myRadAbsorbed += myRadAbsorbtionRate * (Time.deltaTime / 3600f);
        //myRadAbsorbed += (((myRadExposureRateRaw) * myRadAbsorbtionRate) * (Time.deltaTime / 3600f));
    }
    public float AbsorbedDose
    {
        get
        {
            return myRadAbsorbed;
        }
        set
        {
            myRadAbsorbed = value;
        }
    }

    public float AbsorbtionRate
    {
        get { return myRadAbsorbtionRate; }
        set { myRadAbsorbtionRate = value; }
    }
    public float GetRadiationExposureRate()
    {
        return myRadExposureRateRaw;
    }

    public float ExposureTime
    {
        get { return myRadExposureTime; }
        set { myRadExposureTime = value; }
    }

    public float PlayerTemp
    {
        get { return playerTemp; }
        set { playerTemp = value; }
    }

    public void SetPlayerVolume(Volume[] playerVolume)
    {
        volumesPlayerIsIn = playerVolume;
    }

    public void SetPlayerVolumeController(VolumeAtmosphereController vac)
    {
        playerVolume = vac;
    }
    public void ResetPlayerVolumeController(VolumeAtmosphereController vac)
    {
        if (playerVolume == vac)
        {
            playerVolume = null;
        }
    }

    public void OnVolumeEnter(float myVolPress, float myVolTemp, float myVolOx)
    {
        myRoomPressure = myVolPress;
        myRoomTemp = myVolTemp;
        myRoomOxygenation = myVolOx;
    }

    public void OnVolumeUpdate(float myVolPress, float myVolTemp, float myVolOx)
    {
        myRoomTemp = myVolTemp;
        myRoomOxygenation = myVolOx;
        myLastRoomPress = myRoomPressure;
        myRoomPressure = myVolPress;
    }
    public void OnVolumeUpdate()
    {
        if (playerVolume != null)
        {
            myRoomTemp = playerVolume.Temperature;
            myRoomOxygenation = playerVolume.Oxygenation;
            myLastRoomPress = myRoomPressure;
            myRoomPressure = playerVolume.Pressure;
        }
        else
        {
            myRoomTemp = 0f;
            myRoomOxygenation = 0f;
            myLastRoomPress = myRoomPressure;
            myRoomPressure = 0f;
        }

    }

    public void OnLoad(params object[] data)
    {
        //PlayerVolumeController pvc = (data[0] as PlayerVolumeController);
        //myRadExposureRateRaw = pvc.GetRadiationExposureRate();
        //myRadExposureTime = pvc.ExposureTime;
        //myRadAbsorbtionRate = pvc.GetAbsorbtionRate();
        //myRadAbsorbed = pvc.AbsorbedDose;
        //myMaxRoentgenDetectable = pvc.GetMaxRadsDetectable();
        //playerHealth = pvc.PlayerHealth;
        //playerOxygen = pvc.PlayerOxygen;
        //playerTemp = pvc.PlayerTemp;
        //oxyUseRate = pvc.OxygenUseRate;
    }

    public void UpdateUI()
    {
        pressureText.text = Math.Round(myRoomPressure, 2) + " atm";
        oxyText.text = Math.Round(myRoomOxygenation, 2) + " %";
        tempText.text = Math.Round(myRoomTemp, 2) + " F";
        toxText.text = Math.Round(myRoomToxicity, 2) + " %";

        //health
        float max = healthbar.parent.GetComponent<RectTransform>().rect.width;
        float scale = max / healthmax;
        healthbar.localScale = new Vector3(scale * healthcur,1f,1f);
    }

    public IEnumerator UIFlash()
    {
        float time = 0.5f;
        while(myRadAbsorbtionRate >= 700f)
        {
            runningCo = true;
            time -= Time.deltaTime;
            if(time <= 0f)
            {
                //cycle icons and colors
                if (c1)
                {
                    c1 = false;
                    radUIElements[0].color = YELLOW;
                    radUIElements[1].sprite = img1;
                    radUIElements[2].sprite = img1;
                }
                else
                {
                    c1 = true;
                    radUIElements[0].color = BLACK;
                    radUIElements[1].sprite = img2;
                    radUIElements[2].sprite = img2;
                }
                time = 0.5f;
            }
            yield return null;
        }
        runningCo = false;
    }
}
