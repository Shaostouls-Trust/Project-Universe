using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectUniverse.PowerSystem;

namespace ProjectUniverse.Animation.Controllers
{
    public class SwitchAnimationController : MonoBehaviour
    {
        private bool on;
        private bool turningon;
        private bool turningoff;
        [SerializeField]
        private GameObject greenObj;
        [SerializeField]
        private GameObject redObj;
        [SerializeField]
        private GameObject yellowObj;

        void Update()
        {
            //rotation cannot exceed 180 or 0
            /*
            if (turningon || on)//turningon || on
            {
                if(transform.rotation.eulerAngles.y > 0.0)
                {
                    transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                }
            }

            else if (turningoff || !on)//turningoff || !on
            {
                if (transform.rotation.eulerAngles.y < 180)
                {
                    transform.localRotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);
                }
            }
            */
        }

        public void OnSwitch(int numID)
        {
            //play animation
            if (on || turningon)
            {
                turningoff = true;
                turningon = false;
                transform.GetComponent<Animator>().Play("TurnOff");
                //transform.GetComponent<Animator>().Play("TurnOn");
                on = false;
                turningoff = false;
            }
            else
            {
                turningoff = false;
                turningon = true;
                transform.GetComponent<Animator>().Play("TurnOn");
                //transform.GetComponent<Animator>().Play("TurnOff");
                on = true;
                turningon = false;
            }
            //pass in the emissive meshes

            GameObject[] objs = { greenObj, redObj, yellowObj };
            GetComponentInParent<IBreakerBox>().SwitchToggleServerRpc(numID, ref objs);
        }
    }
}