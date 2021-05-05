using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
//using UnityEngine.Rendering.PostProcessing;

/// <summary>
/// This class for player reaction to local atmospheres
/// The player ought only be only be concerned about room pressure, temp, and oxygen/gas levels.
/// In other words, if you can breath, and if so, what are you breathing in?
/// </summary>
public class PlayerVolumeController : MonoBehaviour
{
    private float myRoomPressure;
    private float myLastRoomPress;
    private float myRoomTemp;
    private float myRoomOxygenation;
    private float myRoomToxicity;
    //radiation
    private float myRadExposureRateRaw=0;
    private float myRadExposureTime=0;
    private float myRadAbsorbtionRate = 1;
    private float myRadAbsorbed =0;
    private float myMaxRoentgenDetectable = 0;
    //Player stats:
    [SerializeField] private float playerHealth = 100f;
    [SerializeField] private float playerOxygen = 100;//hold breath %
    [SerializeField] private float playerTemp = 98.6f;
    [SerializeField] private float oxyUseRate = 3.2f;//1.6 is for 60 seconds hold breath
    private Volume[] volumesPlayerIsIn;

    // Start is called before the first frame update
    void Start()
    {
        myRoomPressure = 1f;
        myLastRoomPress = 1f;
        myRoomTemp = 68f;
        myRoomOxygenation = 100f;
        myRoomToxicity = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        /*
        //camera focussing
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
        if (playerOxygen <= 0)
        {
            //take suffocation damage
            InflictPlayerDamage(3.2f * Time.deltaTime);
            //playerHealth -= (3.2f * Time.deltaTime);//suffocate to death in 30 seconds
        }
        if (myRoomOxygenation < 0.5f)//if the air is too thin (shouldn't it be 50.0f?)
        {
            float oxyDefecit = 50.0f - myRoomOxygenation;
            //breath X from an air supply
            //if no air supply:
            ///1 minute of holding breath before taking damage at normal heart rate (1.6f)
            ///If partial air, subtract that from the rate
            ///scale down to 10 or so secs depending on heart rate
            playerOxygen -= ((oxyUseRate-(myRoomOxygenation/100))*Time.deltaTime);
        }
        else if (myRoomOxygenation > 0.5)
        {
            if(playerOxygen < 100)
            {
                playerOxygen += (50 * Time.deltaTime);//2 secs to recover from empty
            }
            if(playerOxygen > 100)
            {
                playerOxygen = 100;
            }
        }

        //inflict toxicity damage
        InflictPlayerDamage(Mathf.Lerp(0.0f, 3.0f, myRoomToxicity));

        if (myRoomPressure < 0.12f)//~121 millibar. 121 is lowest survivable w/ pure oxygen.
        {
            //if in pressure gear:
            //if not in pressure gear:
            //playerHealth -= (6.6f * Time.deltaTime);//0 atm kills in 15 secs
        }
        if(myLastRoomPress - myRoomPressure > 0.5f)
        {
            //if the air pressure changed too quickly, take damage or 'black out'
        }

        if (myRoomTemp != playerTemp)
        ///need a scalar to apply to playerTemp. Also will need some way to simulate sweating bring the body temp to a stable
        ///temp to ensure that 90*F doesn't cause your character to slowly die of heatstroke.
        ///We also need to ensure that when pressure is zero, you freeze more slowly. Esp in vacuum of space where it takes 12 hoursish for cold to kill you.
        {
            //slowly adjust player temp to match (use coroutine?)
        }
        if(playerTemp < 65f)//critical hypothermia
        {
            InflictPlayerDamage(0.5f * Time.deltaTime);
            //playerHealth -= (0.5f*Time.deltaTime);
        }
        //update last room pressure
        //myLastRoomPress = myRoomPressure;

        //player DEATH RAWR:
        if(playerHealth <= 0)
        {
            //you dead, punk!
        }
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
        myRadAbsorbed += (((myRadExposureRateRaw)*myRadAbsorbtionRate) * (Time.deltaTime/ 3600f)); //myRadExposureTime 
    }
    public float GetAbsorbedDose()
    {
        CalculateAbsorbedDose();
        return myRadAbsorbed;
    }
    public float GetRadiationExposureRate()
    {
        return myRadExposureRateRaw;
    }
    public float GetExposureTime()
    {
        return myRadExposureTime;
    }

    public void InflictPlayerDamage(float amount)
    {
        // \/ Naw. Let health be negative. Competative dying.
        //if (playerHealth <= 0)
        //{
        //    playerHealth = 0;
        //}
        playerHealth -= amount;
    }

    public void SetPlayerVolume(Volume[] playerVolume)
    {
        volumesPlayerIsIn = playerVolume;
    }

    public void OnVolumeEnter(float myVolPress, float myVolTemp, float myVolOx)
    {
        myRoomPressure = myVolPress;
        myRoomTemp = myVolTemp;
        myRoomOxygenation = myVolOx;
    }

    public void OnVolumeUpdate(float myVolPress, float myVolTemp, float myVolOx)
    {
        //myRoomPressure = myVolPress;
        myRoomTemp = myVolTemp;
        myRoomOxygenation = myVolOx;
        myLastRoomPress = myRoomPressure;
        myRoomPressure = myVolPress;
    }
}
