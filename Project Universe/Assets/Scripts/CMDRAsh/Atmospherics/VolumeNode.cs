using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectUniverse.Animation.Controllers;

namespace ProjectUniverse.Environment.Volumes
{
    public class VolumeNode : MonoBehaviour
    {
        private GameObject linkedVolume;
        private GameObject globalVolume;
        [SerializeField] private GameObject originDoor;

        public void SetVolumeLink(GameObject volume)
        {
            linkedVolume = volume;
        }

        public void SetGlobalVolume(GameObject volume)
        {
            globalVolume = volume;
        }

        public GameObject GetDoor()
        {
            return originDoor;
        }

        public GameObject GetVolumeLink()
        {
            return linkedVolume;
        }

        public GameObject GetGlobalLink()
        {
            return globalVolume;
        }

        public bool DoorState()
        {
            return originDoor.gameObject.GetComponent<DoorAnimator>().OpenOrOpening();
        }
    }
}