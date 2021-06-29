using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectUniverse.Environment.Volumes;

namespace ProjectUniverse.Environment.Radiation
{
    public class DosimeterAudioController : MonoBehaviour
    {
        [SerializeField] private float maxRads;
        public float detectedRoentgen;
        [SerializeField] private PlayerVolumeController PVC;
        [SerializeField] private AudioSource speaker;
        public float SecondsPerAudioCycle;
        private float cyclesToPlaySound = 0;

        private void Start()
        {
            PVC.SetMaxRadsDetectable(maxRads);
        }

        // Update is called once per frame
        void Update()
        {
            detectedRoentgen = PVC.GetRadiationExposureRate();
            if (detectedRoentgen > 0.0f)
            {
                //play dosimeter sfx at 1 to 10 persec rate
                //get 0-1 lerp
                //Debug.Log(detectedRoentgen + " / " + maxRads +"/ 20000");
                //if(detectedRoentgen > maxRads)
                //{
                //    detectedRoentgen = maxRads;
                //}

                float rate = 0.0f;
                /// 0 - 250 roentgen: 1 - .375
                /// 250 - 500: .375 - .275
                /// 500 - 1000: .275 - .2
                /// 1000 - 1500: .2 - .175
                /// 1500 - 2500: .175 - .1
                /// 2500 - 5000: .1 - .075
                /// 5000 - 7500: .075 - .05
                /// 7500 - 10000: .05 - .025
                /// 10000 - 15000: .025 - .01
                /// 15000 - 20000: .01 - .005
                if (detectedRoentgen <= 250)
                {
                    rate = Mathf.Lerp(0.5f, 0.35f, (detectedRoentgen / 250f));//1f, 0.375f
                }
                else if (detectedRoentgen <= 500)
                {
                    rate = Mathf.Lerp(0.35f, 0.25f, (detectedRoentgen / 500f));//0.375f, 0.275f
                }
                else if (detectedRoentgen <= 1000)
                {
                    rate = Mathf.Lerp(0.25f, 0.175f, (detectedRoentgen / 1000f));//0.275f, 0.2f
                }
                else if (detectedRoentgen <= 1500)
                {
                    rate = Mathf.Lerp(0.175f, 0.1f, (detectedRoentgen / 1500f));//0.2f, 0.175f
                }
                else if (detectedRoentgen <= 2500)
                {
                    rate = Mathf.Lerp(0.1f, 0.075f, (detectedRoentgen / 2500f));//0.175f, 0.1f
                }
                else if (detectedRoentgen <= 5000)
                {
                    rate = Mathf.Lerp(0.075f, 0.05f, (detectedRoentgen / 5000f));//0.01f, 0.075f
                }
                else if (detectedRoentgen <= 7500)
                {
                    rate = Mathf.Lerp(0.05f, 0.025f, (detectedRoentgen / 7000f));//0.075f, 0.05f
                }
                else if (detectedRoentgen <= 10000)
                {
                    rate = Mathf.Lerp(0.025f, 0.0125f, (detectedRoentgen / 10000f));//0.05f, 0.025f
                }
                else if (detectedRoentgen <= 15000)
                {
                    rate = Mathf.Lerp(0.0125f, 0.01f, (detectedRoentgen / 15000f));//0.025f, 0.01f
                }
                else if (detectedRoentgen <= 20000)
                {
                    rate = Mathf.Lerp(0.01f, 0.005f, (detectedRoentgen / 20000f));//0.01f , 0.005f
                }
                else if (detectedRoentgen > 20000)
                {
                    rate = 0.0f;//SecondsPerAudioCycle
                }

                //float rate = Mathf.Lerp(1, SecondsPerAudioCycle, (detectedRoentgen / 20000f));//max for any docimeter is 20000
                //Debug.Log("rate: " + rate);
                cyclesToPlaySound -= Time.deltaTime;
                if (cyclesToPlaySound <= 0)
                {
                    speaker.Play();
                    cyclesToPlaySound = rate;
                }
            }
        }
    }
}