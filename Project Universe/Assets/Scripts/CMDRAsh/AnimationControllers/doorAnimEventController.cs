using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUniverse.Animation.Controllers
{
    public class doorAnimEventController : MonoBehaviour
    {
        //object with material to be animated
        [SerializeField]
        private GameObject parentObject;

        public void emissionStateYellow()
        {
            parentObject.GetComponent<DoorAnimator>().AnimEventOpenServerRpc();
        }

        public void emissionStateGreen()
        {
            parentObject.GetComponent<DoorAnimator>().AnimEventDoneServerRpc();
        }

        public void stopAllAnimations()
        {

            parentObject.GetComponent<DoorAnimator>().HaltAllAnimationsServerRpc();
        }
    }
}