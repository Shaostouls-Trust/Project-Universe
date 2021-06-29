using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUniverse.Animation.Controllers
{
    public class ShutterAnimator : MonoBehaviour
    {
        [SerializeField]
        private Animator animator;
        [SerializeField]
        private GameObject[] controlPanelScreens;
        [SerializeField]
        private bool isPowered;
        private Transform[] shutterChilds;
        //speed at which to run animation. NYI
        private bool isOpening;
        private bool isOpen;
        private bool isClosing;
        private bool isClosed;
        public bool isLocked;

        // Start is called before the first frame update
        void Start()
        {
            //get transforms of child meshes
            //get child count for array
            int childNum = animator.gameObject.transform.childCount;
            shutterChilds = new Transform[childNum];
            //add children to a list
            for (int i = 0; i < childNum; ++i)
            {
                shutterChilds[i] = animator.gameObject.transform.GetChild(i);
            }
            //set default (starting) state
            ShutterIsOpen();
        }


        void FixedUpdate()
        {
            if (isPowered)
            {
                //itt through all childs, and if their position is greater than their specific max, set their position to the max.
                //set end bounds (in local space)
                float upbound = 2.25f;
                float[] downbounds = { 0f, -2.2f, -4.35f, -6.35f };

                if (isClosing || isClosed)
                {
                    for (int j = 0; j < shutterChilds.Length; j++)
                    {
                        //if we have exceeded the shutter's open position (Z is up for children)
                        if (shutterChilds[j].localPosition.z >= 2.25f)
                        {
                            shutterChilds[j].localPosition = new Vector3(shutterChilds[j].localPosition.x, shutterChilds[j].localPosition.y, upbound);
                        }
                    }
                }
                else if (isOpening || isOpen)
                {
                    for (int k = 0; k < shutterChilds.Length; k++)
                    {
                        //if we have exceeded the shutter's closing position (Z is up for children)
                        if (shutterChilds[k].localPosition.z >= downbounds[k])
                        {
                            shutterChilds[k].localPosition = new Vector3(shutterChilds[k].localPosition.x, shutterChilds[k].localPosition.y, downbounds[k]);
                        }
                    }
                }

            }
        }


        //open or close the shutters
        public void buttonResponse()
        {
            //Debug.Log("nya nya nya");
            if (isPowered && !isLocked)
            {
                if (isOpen || isOpening)
                {
                    Debug.Log("Closing");
                    ShutterIsClosing();
                    animator.Play("WindowShutter_MallBottom_Func001");//close
                    ShutterIsClosed();
                }
                else if (isClosed || isClosing)
                {
                    Debug.Log("Opening");
                    ShutterIsOpening();
                    animator.Play("WindowShutter_MallBottom_Func002");//open
                    ShutterIsOpen();
                }
            }
        }

        public void ShutterIsOpening()
        {
            isOpening = true;
            isClosing = false;
            isOpen = false;
            isClosed = false;
        }

        public void ShutterIsClosing()
        {
            isOpening = false;
            isClosing = true;
            isOpen = false;
            isClosed = false;
        }

        public void ShutterIsClosed()
        {
            isOpening = false;
            isClosing = false;
            isOpen = false;
            isClosed = true;
        }

        public void ShutterIsOpen()
        {
            isOpening = false;
            isClosing = false;
            isOpen = true;
            isClosed = false;
        }
    }
}