using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorAnimator : MonoBehaviour
{
    [SerializeField]
    private BoxCollider trigger;
    [SerializeField]
    private Animator[] anim;
    [SerializeField]
    private GameObject emissiveMesh;
    [SerializeField]
    private GameObject[] controlPanelScreens;
    //IMPORTANT
    //elem 0 is door L
    //elem 1 is door R
    public float leftBoundOpen;//1.8f
    public float leftBoundClosing;//1.0f
    public float rightBoundOpen;//-1.8f
    public float rightBoundClosing;//-1.0f

    private bool isLOpen;//door open
    private bool isLClosed;//door closed
    private bool isLOpening;//door in process of opening
    private bool isLClosing;//door in process of closing
    private bool isROpen;
    private bool isRClosed;
    private bool isROpening;
    private bool isRClosing;
    private bool hasEmissive;

    private bool locked;//whether or not the door can be opened
    private bool weldedClosed;//whether or not door has been welded shut
    private bool weldedOpen;//Allow welded open?

    private bool isPowered;
    private bool isRunning;
    //speed at which to run animation.
    private float animSpeed;

    private Transform doorL_TF;
    private Transform doorR_TF;
    //door emissive
    private Renderer emissRenderer;

    //panel emissive
    private Renderer[] panelRenderer;
    public bool doorAnimIsInX;

    void Start()
    {
        panelRenderer = new Renderer[controlPanelScreens.Length];
        doorL_TF = anim[0].gameObject.transform;
        doorR_TF = anim[1].gameObject.transform;
        if(emissiveMesh == null)
        {
            hasEmissive = false;
        }
        else
        {
            emissRenderer = emissiveMesh.GetComponent<Renderer>();
            hasEmissive = true;
        }
        isRunning = true;
        //grab all screen renderers
        for (int i = 0; i < controlPanelScreens.Length;i++)
        {
            panelRenderer[i] = controlPanelScreens[i].GetComponent<Renderer>();
        }
    }

    void Update()
    {
        //if door is powered, run the below checks
        if (isPowered && isRunning)
        {
            //check door positions and animation cycles
            //when the door is at open position, but the animation is not done opening.
            if (doorAnimIsInX)
            {
                if (doorL_TF.localPosition.x > leftBoundOpen && (isLOpening || isLOpen))
                {
                    anim[1].enabled = false;
                    //doorL_TF.localPosition = new Vector3(doorL_TF.localPosition.x, doorL_TF.localPosition.y, leftBoundOpen);
                    doorL_TF.localPosition = new Vector3(leftBoundOpen,doorL_TF.localPosition.y, doorL_TF.localPosition.z);
                }
                else
                {
                    anim[0].enabled = true;
                }
            }
            else
            {
                if (doorL_TF.localPosition.z > leftBoundOpen && (isLOpening || isLOpen))
                {
                    //stop the left door animation.
                    anim[0].enabled = false;
                    //return to final position
                    //doorL_TF.localPosition = new Vector3(leftBoundOpen, doorL_TF.localPosition.y, doorL_TF.localPosition.z);
                    doorL_TF.localPosition = new Vector3(doorL_TF.localPosition.x, doorL_TF.localPosition.y, leftBoundOpen);
                }
                else
                {
                    anim[0].enabled = true;
                }
            }

            //if closed but not done with closing animation
            if (doorAnimIsInX)
            {
                if (doorL_TF.localPosition.x < leftBoundClosing && (isLClosing || isLClosed))
                {
                    anim[0].enabled = false;
                    //doorL_TF.localPosition = new Vector3(doorL_TF.localPosition.x, doorL_TF.localPosition.y, leftBoundClosing);
                    doorL_TF.localPosition = new Vector3(leftBoundClosing, doorL_TF.localPosition.y, doorL_TF.localPosition.z);
                }
                else
                {
                    anim[0].enabled = true;
                }
            }
            else
            {
                if (doorL_TF.localPosition.z < leftBoundClosing && (isLClosing || isLClosed))
                {
                    //stop
                    anim[0].enabled = false;
                    //doorL_TF.localPosition = new Vector3(leftBoundClosing, doorL_TF.localPosition.y, doorL_TF.localPosition.z);
                    doorL_TF.localPosition = new Vector3(doorL_TF.localPosition.z, doorL_TF.localPosition.y, leftBoundClosing);
                }
                else
                {
                    anim[0].enabled = true;
                }
            } 

            //R Door open, but not done with animation
            if (doorAnimIsInX)
            {
                anim[1].enabled = false;
                if (doorR_TF.localPosition.x < rightBoundOpen && (isROpening || isROpen)) //open past -1.8Z
                {
                    doorR_TF.localPosition = new Vector3(rightBoundOpen, doorR_TF.localPosition.y, doorR_TF.localPosition.z);
                }
                else
                {
                    anim[1].enabled = true;
                }
            }
            else
            {
                if (doorR_TF.localPosition.z < rightBoundOpen && (isROpening || isROpen)) //open past -1.8Z
                {
                    anim[1].enabled = false;
                    doorR_TF.localPosition = new Vector3(doorR_TF.localPosition.x, doorR_TF.localPosition.y, rightBoundOpen);
                }
                else
                {
                    anim[1].enabled = true;
                }
            }

            //if closed but not done with anim
            if (doorAnimIsInX)
            {
                if (doorR_TF.localPosition.x > rightBoundClosing && (isRClosing || isRClosed)) //closed past -1.0f
                {
                    anim[1].enabled = false;
                    doorR_TF.localPosition = new Vector3(rightBoundClosing, doorR_TF.localPosition.y, doorR_TF.localPosition.z);
                }
                else
                {
                    anim[1].enabled = true;
                }
            }
            else
            {
                if (doorR_TF.localPosition.z > rightBoundClosing && (isRClosing || isRClosed)) //closed past -1.0f
                {
                    anim[1].enabled = false;
                    doorR_TF.localPosition = new Vector3(doorR_TF.localPosition.x, doorR_TF.localPosition.y, rightBoundClosing);
                }
                else
                {
                    anim[1].enabled = true;
                }
            } 
        }
    }

    void OnTriggerEnter()
    {
        if (isPowered && (!locked && isRunning))
        {
            if(!isLOpening && !isROpening)
            {
                //yellow blinking
                doorRIsOpening();
                doorLIsOpening();
                anim[0].Play("DoorLeftOpen");
                anim[1].Play("DoorRightOpen");
                doorLIsOpen();
                doorRIsOpen();
                //green
            }
        }
    }

    void OnTriggerStay()
    {
        if (isPowered && (!locked && isRunning))
        {
            //both doors are open (eval to false) or opening already.
            if ((!isLOpen && !isROpen)||(isROpening && isLOpening))
            {
                //yellow blinking
                doorRIsOpening();
                doorLIsOpening();
                anim[0].Play("DoorLeftOpen");
                anim[1].Play("DoorRightOpen");
                doorLIsOpen();
                doorRIsOpen();
                //green
            }
        }
    }

    void OnTriggerExit()
    {
        if (isPowered && (!locked && isRunning))
        {
            //yellow blinking
            doorRIsClosing();
            doorLIsClosing();
            anim[0].Play("DoorLeftClose");
            anim[1].Play("DoorRightClose");
            doorLIsClosed();
            doorRIsClosed();
            //green
        }
    }

    public void haltAllAnimations()
    {
        anim[0].enabled = false;
        anim[1].enabled = false;
    }

    public void lockDoor()
    {
        if(isRunning && isPowered)
        {
            //close door
            locked = true;
            if (isLOpen)
            {
                doorLIsClosing();
                anim[0].Play("DoorLeftClose");
                doorLIsClosed();
            }
            if (isROpen){
                doorRIsClosing();

                anim[1].Play("DoorRightClose");

                doorRIsClosed();
            }
        }
    }

    public void unlockDoor()
    {
        if (isRunning && isPowered)
        {
            locked = false;
            //green. Assumes no further issues. Will need logic later on.
            if (hasEmissive)
            {
                emissRenderer.material = MaterialLibrary.GetDoorStateMaterials(0);
            }
            for (int i = 0; i < panelRenderer.Length; i++)
            {
                panelRenderer[i].material = MaterialLibrary.GetDoorDisplayMaterials(0);
            }
        }
    }

    public void ButtonResponse()
    {
        if (!locked)
        {
            lockDoor();
        }
        else if (locked)
        {
            unlockDoor();
        }
    }

    //universal
    public void animEventOpen()
    {
            //if locked, change to flashing red
        if (locked)
        {
            if (hasEmissive)
            {
                emissRenderer.material = MaterialLibrary.GetDoorStateMaterials(3);
            }
            for (int i = 0; i < panelRenderer.Length; i++)
            {
                panelRenderer[i].material = MaterialLibrary.GetDoorDisplayMaterials(2);
            }
        }
        else
        {
            //change to blinking yellow on open
            if (hasEmissive)
            {
                emissRenderer.material = MaterialLibrary.GetDoorStateMaterials(1);
            }
            for (int i = 0; i < panelRenderer.Length; i++)
            {
                panelRenderer[i].material = MaterialLibrary.GetDoorDisplayMaterials(1);
            }
        }
    }

    //universal
    public void animEventOpenDone()
    {
        //if locked, change to solid red
        if (locked)
        {
            if (hasEmissive)
            {
                emissRenderer.material = MaterialLibrary.GetDoorStateMaterials(2);
            }
            for (int i = 0; i < panelRenderer.Length; i++)
            {
                panelRenderer[i].material = MaterialLibrary.GetDoorDisplayMaterials(2);
            }
        }
        else
        {
            //else change to green
            if (hasEmissive)
            {
                emissRenderer.material = MaterialLibrary.GetDoorStateMaterials(0);
            }
            for (int i = 0; i < panelRenderer.Length; i++)
            {
                panelRenderer[i].material = MaterialLibrary.GetDoorDisplayMaterials(0);
            }
        }
    }

    public void setPoweredState(bool value)
    {
        isPowered = value;
    }
    public void setRunningState(bool value)
    {
        isRunning = value;
    }

    public void setAnimSpeed(float speed)
    {
        anim[0].SetFloat("AnimSpeed", speed);
        anim[1].SetFloat("AnimSpeed", speed);
    }

    public void doorLIsOpening()
    {
        isLOpening = true;
        isLClosing = false;
        isLOpen = false;
        isLClosed = false;
    }

    public void doorLIsClosing()
    {
        isLOpening = false;
        isLClosing = true;
        isLOpen = false;
        isLClosed = false;
    }

    public void doorLIsClosed()
    {
        isLOpening = false;
        isLClosing = false;
        isLOpen = false;
        isLClosed = true;
    }

    public void doorLIsOpen()
    {
        isLOpening = false;
        isLClosing = false;
        isLOpen = true;
        isLClosed = false;
    }

    public void doorRIsOpening()
    {
        isROpening = true;
        isRClosing = false;
        isROpen = false;
        isRClosed = false;
    }

    public void doorRIsClosing()
    {
        isROpening = false;
        isRClosing = true;
        isROpen = false;
        isRClosed = false;
    }

    public void doorRIsClosed()
    {
        isROpening = false;
        isRClosing = false;
        isROpen = false;
        isRClosed = true;
    }

    public void doorRIsOpen()
    {
        isROpening = false;
        isRClosing = false;
        isROpen = true;
        isRClosed = false;
    }
}
