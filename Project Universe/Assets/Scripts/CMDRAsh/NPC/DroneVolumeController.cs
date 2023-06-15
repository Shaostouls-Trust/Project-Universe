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
        
        // radiation damage
        // if radiation exposure rate is greater than 600, begin taking damage
        if(GetRadiationExposureRate() >= 600f)
        {
            radWarnUI.SetActive(true);
            if (!runningCo)
            {
                //StartCoroutine(UIFlash());
            }
            healthcur -= ((GetRadiationExposureRate() - 600f) / 10f) * Time.deltaTime;
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
            myRoomTemp = -328f;
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
        //Debug.Log("room: " + playerVolume);
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
