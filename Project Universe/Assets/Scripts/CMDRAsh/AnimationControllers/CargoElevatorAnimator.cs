using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUniverse.Animation.Controllers
{
    public class CargoElevatorAnimator : MonoBehaviour
    {
        [SerializeField]
        private Animator anim;
        [SerializeField]
        private GameObject linkedRamp;
        private bool isUp;
        private bool isDown;
        //private bool isRising;
        //private bool isFalling;
        public bool startAtPosUp;


        //run the elevator
        public void ButtonResponse()
        {
            //No elevator directional interrupts
            if (isUp)
            {
                isDown = true;
                //isRising = false;
                //Debug.Log("Crawl Door Open");
                anim.Play("CargoElevatorA_Down");
                isDown = true;
                isUp = false;
            }
            else if (isDown)
            {
                //isFalling = false;
                //isRising = true;
                //Debug.Log("Crawl Door Close");
                anim.Play("CargoElevatorA_Up");
                isDown = false;
                isUp = true;
            }
        }

        public void LowerLinkedRamp()
        {
            linkedRamp.GetComponent<Animator>().Play("6x9DropRamp_Down");
        }

        public void RaiseLinkedRamp()
        {
            linkedRamp.GetComponent<Animator>().Play("6x9DropRamp_Up");
        }
        // Start is called before the first frame update
        void Start()
        {
            if (startAtPosUp)
            {
                isUp = true;
                isDown = false;
                //isRising = false;
                //isFalling = false;
            }
            else
            {
                isUp = false;
                isDown = true;
                //isRising = false;
                //isFalling = false;
            }

            //doorTF = anim.gameObject.transform;
        }

    }
}