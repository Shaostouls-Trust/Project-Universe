using ProjectUniverse.Animation.Controllers;
using ProjectUniverse.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectUniverse.Environment.Interactable
{
    public class BreakerSwitch : MonoBehaviour
    {

        [SerializeField]
        private GameObject scriptedObj;
        [SerializeField]
        private int numID;
        [SerializeField] private Transform needle;
        [SerializeField] private Image dialBG;
        //private int time=30;

        void Start()
        {
            this.GetComponent<MeshRenderer>().enabled = false;
            if (needle != null && dialBG != null)
            {
                needle.localRotation = Quaternion.Euler(0f, 0f, -50f);
                dialBG.color = new Color(65f, 65f, 65f);
            }
            //renderer.enabled = false;
        }

        //public Transform Needle
        //{
        //    get { return needle; }
        //}

        public void SetPowerDisplay(float powerAmount)
        {
            if (dialBG != null && needle != null)
            {
                //if powerAmount is 0, set dialBG to (65f,65f,65f)
                if (powerAmount == 0)
                {
                    dialBG.color = new Color(65f, 65f, 65f);
                }
                else
                {
                    dialBG.color = new Color(255f, 255f, 255f);
                }
                if (powerAmount > 26f)
                {
                    powerAmount = 26f;
                }
                float deg = Utils.BreakerBoxB_DegToDial(powerAmount);
                //set needle localrotation to deg
                needle.localRotation = Quaternion.Euler(0f, 0f, deg);
            }
        }

        /// <summary>
        /// Universal linking function to be present in all button classes. A universal backend.
        /// This button specifically for powersystem control switches.
        /// </summary>
        public void ExternalInteractFunc()
        {
            scriptedObj.GetComponent<SwitchAnimationController>().OnSwitch(numID);
        }

        void OnMouseOver()
        {
            if (!this.GetComponent<MeshRenderer>().enabled)
            {
                this.GetComponent<MeshRenderer>().enabled = true;
            }
        }

        void OnMouseExit()
        {
            if (this.GetComponent<MeshRenderer>().enabled)
            {
                this.GetComponent<MeshRenderer>().enabled = false;
            }
        }
    }
}