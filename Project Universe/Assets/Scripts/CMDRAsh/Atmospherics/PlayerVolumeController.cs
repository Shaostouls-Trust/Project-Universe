using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using ProjectUniverse.Player;
using ProjectUniverse.Player.PlayerController;
using UnityEngine.UI;
using TMPro;
using ProjectUniverse.Util;
using ProjectUniverse.Environment.Gas;
//using UnityEngine.Rendering.PostProcessing;

/// <summary>
/// This class for player reaction to local atmospheres
/// The player ought only be only be concerned about room pressure, temp, and oxygen/gas levels.
/// In other words, if you can breath, and if so, what are you breathing in?
/// </summary>
namespace ProjectUniverse.Environment.Volumes
{
    //[Serializable]
    public sealed class PlayerVolumeController : MonoBehaviour
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
        private float myRadAbsorbtionRate = 0.92f;
        [SerializeField] private float myRadAbsorbed = 0;
        private float myMaxRoentgenDetectable = 0;
        //Player stats:
        //[SerializeField] private float playerHealth = 100f;
        [SerializeField] private float playerOxygen = 100;//hold breath %
        [SerializeField] private float playerTemp = 98.6f;
        [SerializeField] private float oxyUseRate = 3.2f;//1.6 is for 60 seconds hold breath
        private Volume[] volumesPlayerIsIn;
        private VolumeAtmosphereController playerVolume;
        public string CurrentVolumeOfPlayer;
        private SupplementalController playerControllerSup;
        [Space]
        private float co2PerBreath = 0.000025f;//25ml per breath at normal respiration (5% conversion)
        [SerializeField] private int breathsPerMinute = 15;//average breathing rate (1 every 4sec)
        private IGas breathInGas;
        private IGas breathOutGas;
        private float toNextBreath;
        [SerializeField] private AudioSource playerSFX;
        public AudioClip[] breathClips;

        // Start is called before the first frame update
        void Start()
        {
            playerControllerSup = GetComponent<SupplementalController>();
            myRoomPressure = 1f;
            myLastRoomPress = 1f;
            myRoomTemp = 68f;
            myRoomOxygenation = 100f;
            myRoomToxicity = 0f;
            breathInGas = new IGas("Oxygen", 70f, co2PerBreath,1.0f,0.006f);//6L in lungs
            breathOutGas = new IGas("CarbonDioxide", 98.6f, co2PerBreath,1.0f,0.006f);
            toNextBreath = 60f / breathsPerMinute;
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

            // Replace the below logic by breathing in what air from the volume you can.
            // may or may not be oxygen.
            BreatheAirFromVolume();

            if (playerControllerSup.ShipMode)
            {
                //Debug.Log("playerVolume.RSM: " + playerVolume.RSM);
                if(playerVolume != null && playerVolume.RSM != null)
                {
                    playerVolume.RSM.ExternalControllerState = true;
                }
            }
            else
            {
                if (playerVolume != null && playerVolume.RSM != null)
                {
                    playerVolume.RSM.ExternalControllerState = false;
                }
            }

            if (myRoomTemp != playerTemp)
            ///need a scalar to apply to playerTemp. Also will need some way to simulate sweating bring the body temp to a stable
            ///temp to ensure that 90*F doesn't cause your character to slowly die of heatstroke.
            ///We also need to ensure that when pressure is zero, you freeze more slowly. Esp in vacuum of space where it takes 12 hoursish for cold to kill you.
            {
                //slowly adjust player temp to match (use coroutine?)
            }
            if (playerTemp < 65f)//critical hypothermia
            {
                playerControllerSup.InflictPlayerDamageServerRpc(0.5f * Time.deltaTime);
                //playerHealth -= (0.5f*Time.deltaTime);
            }
            //update last room pressure
            //myLastRoomPress = myRoomPressure;

            //player DEATH RAWR:
            if (playerControllerSup.PlayerHealth <= 0)
            {
                //you dead, punk!
            }

            UpdateUI();
        }

        public void BreatheAirFromVolume()
        {
            toNextBreath -= Time.deltaTime;
            //oxygen is consumed from the volume
            //barring that, from the player's lungs
            if (playerVolume != null)
            {
                if (myRoomOxygenation < 50.0f)//if the air is too thin
                {
                    // if no air supply
                    ///1 minute of holding breath before taking damage at normal heart rate (1.6f)
                    ///scale down to 10 or so secs depending on heart rate
                    if (myRoomOxygenation > 10.0f)
                    {
                        //Breathe in what little there is.
                        if (toNextBreath <= 0f)
                        {
                            int clipIndex = UnityEngine.Random.Range(0, breathClips.Length);
                            playerSFX.PlayOneShot(breathClips[clipIndex]);
                            breathInGas.SetTemp(myRoomTemp);
                            breathInGas.SetLocalPressure(myRoomPressure);
                            playerVolume.RemoveRoomGas(breathInGas);
                            playerVolume.AddRoomGas(breathOutGas);
                        }
                        playerOxygen -= ((oxyUseRate - ((myRoomOxygenation / 100f)) * oxyUseRate) * Time.deltaTime);
                    }
                    else//hold breath
                    {
                        if (playerOxygen > 0f)
                        {
                            playerOxygen -= (oxyUseRate * Time.deltaTime);
                        }
                    }  
                }
                else if (myRoomOxygenation >= 50.0)
                {
                    if (toNextBreath <= 0f)
                    {
                        int clipIndex = UnityEngine.Random.Range(0, breathClips.Length);
                        playerSFX.PlayOneShot(breathClips[clipIndex]);
                        //Air conversion is every breath. Logic is constant
                        breathInGas.SetTemp(myRoomTemp);
                        breathInGas.SetLocalPressure(myRoomPressure);
                        playerVolume.RemoveRoomGas(breathInGas);
                        playerVolume.AddRoomGas(breathOutGas);
                    }
                    if (playerOxygen < 100)
                    {
                        playerOxygen += ((myRoomOxygenation / 100f) * 25f * Time.deltaTime);//4 secs to recover from empty at 100% O2
                    }
                    if (playerOxygen > 100)
                    {
                        playerOxygen = 100;
                    }
                }
                
                if (playerOxygen <= 0)
                {
                    //suffocate to death in 30 seconds
                    playerControllerSup.InflictPlayerDamageServerRpc((3.2f - ((myRoomOxygenation / 100f) * 3.2f)) * Time.deltaTime);
                }

                //Pressure damage
                if (myRoomPressure < 0.12f)//~121 millibar. 121 is lowest survivable w/ pure oxygen.
                {
                    //if in pressure gear:
                    //if not in pressure gear:
                    playerControllerSup.InflictPlayerDamageServerRpc(6.6f * Time.deltaTime);//0 atm kills in 15 secs
                }
                if (myLastRoomPress - myRoomPressure > 0.5f)
                {
                    //if the air pressure changed too quickly, take damage (or 'black out'?)
                    playerControllerSup.InflictPlayerDamageServerRpc(10f  * (myLastRoomPress - myRoomPressure));
                }

                //TOX
                //inflict toxicity damage
                if(myRoomToxicity > 0.1f)
                {
                    playerControllerSup.InflictPlayerDamageServerRpc(Mathf.Lerp(0.4f, 4.0f, (myRoomToxicity-0.2f))*Time.deltaTime);
                }
            }
            else
            {
                //we are in space (without survival gear)
                playerOxygen -= (oxyUseRate * Time.deltaTime);
                playerControllerSup.InflictPlayerDamageServerRpc(6.6f * Time.deltaTime);//0 atm kills in 15 secs
            }
            if(playerOxygen < 0f)
            {
                playerOxygen = 0f;
            }

            if (toNextBreath <= 0f)
            {
                toNextBreath = 60f / breathsPerMinute;
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
            myRadAbsorbed += myRadAbsorbtionRate * (Time.deltaTime / 3600f);
            //myRadAbsorbed += (((myRadExposureRateRaw) * myRadAbsorbtionRate) * (Time.deltaTime / 3600f));
        }
        public float AbsorbedDose {
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
        //public float GetExposureTime()
        //{
        //    return myRadExposureTime;
        //}

        public float PlayerOxygen
        {
            get { return playerOxygen; }
            set { playerOxygen = value; }
        }

        public float PlayerTemp
        {
            get { return playerTemp; }
            set { playerTemp = value; }
        }

        public float OxygenUseRate
        {
            get { return oxyUseRate; }
            set { oxyUseRate = value; }
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
            if(playerVolume == vac)
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
            PlayerVolumeController pvc = (data[0] as PlayerVolumeController);
            //myRadExposureRateRaw = pvc.GetRadiationExposureRate();
            myRadExposureTime = pvc.ExposureTime;
            //myRadAbsorbtionRate = pvc.GetAbsorbtionRate();
            myRadAbsorbed = pvc.AbsorbedDose;
            //myMaxRoentgenDetectable = pvc.GetMaxRadsDetectable();
            //playerHealth = pvc.PlayerHealth;
            playerOxygen = pvc.PlayerOxygen;
            playerTemp = pvc.PlayerTemp;
            oxyUseRate = pvc.OxygenUseRate;
        }

        public void UpdateUI()
        {
            pressureText.text = Math.Round(myRoomPressure,2) +" atm";
            oxyText.text = Math.Round(myRoomOxygenation, 2)+" %";
            tempText.text = Math.Round(myRoomTemp, 2) + " F";
            toxText.text = Math.Round(myRoomToxicity, 2) + " %";
        }
    }
}