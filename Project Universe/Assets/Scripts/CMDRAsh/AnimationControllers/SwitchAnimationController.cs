using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectUniverse.PowerSystem;

namespace ProjectUniverse.Animation.Controllers
{
    public class SwitchAnimationController : MonoBehaviour
    {
        private bool on = true;
        private bool turningon = false;
        private bool turningoff = false;
        [SerializeField]
        private GameObject greenObj;
        [SerializeField]
        private GameObject redObj;
        [SerializeField]
        private GameObject yellowObj;

        public GameObject GreenLED
        {
            get { return greenObj; }
        }
        public GameObject RedLED
        {
            get { return redObj; }
        }
        public GameObject YellowLED
        {
            get { return YellowLED; }
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
            Debug.Log("Server Switch Toggle");
            GetComponentInParent<IBreakerBox>().SwitchToggleServerRpc(numID);//, ref objs);
        }
    }
}