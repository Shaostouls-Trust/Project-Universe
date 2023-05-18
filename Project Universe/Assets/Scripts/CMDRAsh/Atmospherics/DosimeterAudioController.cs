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
        [SerializeField] private DroneVolumeController DVC;
        [SerializeField] private AudioSource speaker;
        [SerializeField] private AudioClip[] soundBlips;
        public AnimationCurve curve;
        private float cyclesToPlaySound = 0.1f;

        private void Start()
        {
            if (PVC != null)
            {
                PVC.SetMaxRadsDetectable(maxRads);
            }
            else
            {
                DVC.SetMaxRadsDetectable(maxRads);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if(PVC != null)
            {
                detectedRoentgen = PVC.GetRadiationExposureRate();
            }
            else
            {
                detectedRoentgen = DVC.GetRadiationExposureRate();
            }

            if (detectedRoentgen > 0.0f)
            {
                if (detectedRoentgen > maxRads)
                {
                    detectedRoentgen = maxRads;
                }
                float rate = 1f - curve.Evaluate(detectedRoentgen / maxRads);
                int[] range = {0, 3};
                if(rate <= 0.025f)
                {
                    range[0] = 4;
                    range[1] = 6;
                }
                else if(rate <= 0.1f)
                {
                    range[0] = 2;
                    range[1] = 6;
                }
                else if(rate <= 0.67f)
                {
                    range[0] = 1;
                    range[1] = 4;
                }

                cyclesToPlaySound -= Time.deltaTime;
                if (cyclesToPlaySound <= 0)
                {
                    if (!speaker.isPlaying)
                    {
                        speaker.panStereo = Random.Range(-1f, 1f);
                        speaker.PlayOneShot(soundBlips[Random.Range(range[0], range[1])]);
                        cyclesToPlaySound = rate;
                    }
                }
            }
        }
    }
}