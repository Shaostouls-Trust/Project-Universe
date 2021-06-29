using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUniverse.Items.Weapons
{
    public class ICasing : MonoBehaviour
    {
        //public AudioClip audClip;
        public AudioSource audSrc;

        private void OnCollisionEnter(Collision collision)
        {
            if (GetComponent<Rigidbody>().velocity.magnitude > 1f)
            {
                audSrc.Play(0);
            }
            ///
            /// Eventually sound will be randomized, etc
            ///
        }
    }
}