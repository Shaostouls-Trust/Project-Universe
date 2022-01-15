using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectUniverse.Animation.Controllers;

namespace ProjectUniverse.Environment.Volumes
{
    public class VolumeNode : MonoBehaviour
    {
        [SerializeField] private GameObject linkedVolume;
        private GameObject globalVolume;
        [SerializeField] private GameObject originDoor;
        [SerializeField] private bool linkByImpact;

        public bool LinkByImpact
        {
            get { return linkByImpact; }
        }

        public GameObject VolumeLink
        {
            get { return linkedVolume; }
            set { linkedVolume = value; }
        }

        public GameObject GlobalLink
        {
            get { return globalVolume; }
            set { globalVolume = value; }
        }

        //public void SetVolumeLink(GameObject volume)
        //{
        //    linkedVolume = volume;
        //}

        //public void SetGlobalVolume(GameObject volume)
        //{
        //    globalVolume = volume;
        //}

        public GameObject GetDoor()
        {
            return originDoor;
        }

        //public GameObject GetVolumeLink()
        //{
        //    return linkedVolume;
        //}

        //public GameObject GetGlobalLink()
        //{
        //    return globalVolume;
        //}

        public bool DoorState()
        {
            return originDoor.gameObject.GetComponent<DoorAnimator>().OpenOrOpening();
        }
    }
}