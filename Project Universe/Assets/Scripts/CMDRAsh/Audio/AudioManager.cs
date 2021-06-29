using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace ProjectUniverse.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public AudioMixer mixer;
        [Range(-80.0f, 20.0f)]
        public float masterVolume;
        [Range(-80.0f, 20.0f)]
        public float alarmMasterVolume;
        [Range(-80.0f, 20.0f)]
        public float priority_Volume;
        [Range(-80.0f, 20.0f)]
        public float generic_Volume;

        // Start is called before the first frame update
        void Start()
        {
            mixer.SetFloat("MasterVolume", masterVolume);
            mixer.SetFloat("AlarmMasterVolume", alarmMasterVolume);
            mixer.SetFloat("Generic_Volume", generic_Volume);
            mixer.SetFloat("Priority_Volume", priority_Volume);
        }

        // Update is called once per frame
        void Update()
        {
            //mixer.SetFloat("MasterVolume", masterVolume);
        }
    }
}