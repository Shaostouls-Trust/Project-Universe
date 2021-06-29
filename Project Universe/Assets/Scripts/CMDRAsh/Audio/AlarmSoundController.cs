using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace ProjectUniverse.Audio
{
    public class AlarmSoundController : MonoBehaviour
    {
        public AudioMixer mixer;
        [SerializeField]
        private AudioSource speaker;
        public AudioClip audioClip;
        public string outputGroup;
        public float dopplerLevel;
        public float spread;
        //public AudioRolloffMode rollOffMode;
        public int priority;
        public int minDistance;
        public int maxDistance;
        public float alarmVolume;
        //[Range(-10000.0f, 0.0f)]
        private AudioClip tempClip;

        public bool active;

        // Start is called before the first frame update
        void Start()
        {
            Debug.Log(audioClip.ToString());
            tempClip = audioClip;
            updateParameters();
        }

        public void updateParameters()
        {
            string[] value;
            //validate that sound file is in dictionary
            //Debug.Log(audioClip.ToString());
            if (SoundLibA.soundParameters.ContainsKey(audioClip.ToString()))
            {
                Debug.Log("GotPassed");
            }
            // if (SoundLibA.soundParameters.TryGetValue(audioClip.ToString(), out value))
            // {
            SoundLibA.soundParameters.TryGetValue(audioClip.ToString(), out value);
            Debug.Log("Passed");
            //grab parameters from the dictionary's value list.
            speaker.clip = audioClip;
            speaker.outputAudioMixerGroup = mixer.FindMatchingGroups(value[0])[0];
            speaker.volume = float.Parse(value[1]);
            speaker.dopplerLevel = float.Parse(value[2]);
            speaker.minDistance = int.Parse(value[3]);
            speaker.maxDistance = int.Parse(value[4]);
            speaker.spread = float.Parse(value[5]);
            speaker.priority = int.Parse(value[6]);
            tempClip = audioClip;
            //  }
            //configure speaker
            //speaker.outputAudioMixerGroup = mixer.FindMatchingGroups(outputGroup)[0];
            //speaker.volume = alarmVolume;
            //speaker.dopplerLevel = dopplerLevel;
            //speaker.minDistance = minDistance;
            //speaker.maxDistance = maxDistance;
            //speaker.spread = spread;
            //speaker.priority = priority;
            //speaker.rolloffMode = rollOffMode;
        }

        void Update()
        {
            //Debug.Log(audioClip.ToString());
            //Debug.Log(tempClip.ToString());
            //handle change in soundclip being played.
            if (audioClip.ToString() != tempClip.ToString())
            {
                updateParameters();
            }

            //Turn off or on the player
            if (active && !speaker.isPlaying)
            {
                speaker.Play();
            }
            else if (!active)
            {
                speaker.Stop();
            }
        }
    }
}