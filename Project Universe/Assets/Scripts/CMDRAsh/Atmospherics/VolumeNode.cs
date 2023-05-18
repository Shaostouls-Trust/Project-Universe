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

        private void Start()
        {
            if (originDoor != null && linkedVolume != null)
            {
                originDoor.GetComponent<DoorAnimator>().OthersideTextMesh.text = linkedVolume.name;
            }
           
        }

        public bool LinkByImpact
        {
            get { return linkByImpact; }
        }

        public GameObject VolumeLink
        {
            get
            {
                if(linkedVolume != null)
                {
                    if (linkedVolume.activeInHierarchy)
                    {
                        return linkedVolume;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
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

        public void SetDoor(GameObject door)
        {
            originDoor = door;
        }

        public void SetVolumeLink(GameObject room)
        {
            linkedVolume = room;
        }

        //public GameObject GetGlobalLink()
        //{
        //    return globalVolume;
        //}

        public bool DoorState()
        {
            DoorAnimator da = originDoor.gameObject.GetComponent<DoorAnimator>();
            return da.OpenOrOpening() || da.Closing;
        }
    }
}