using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Impact.Demo
{
    public class DemoButton : MonoBehaviour
    {
        [SerializeField]
        private UnityEvent onPressed;
        [SerializeField]
        private UnityEvent onHeld;
        [SerializeField]
        private AudioSource pressedAudio;

        public void Press()
        {
            onPressed.Invoke();
            pressedAudio.Play();
        }

        public void Hold()
        {
            onHeld.Invoke();
        }
    }
}

