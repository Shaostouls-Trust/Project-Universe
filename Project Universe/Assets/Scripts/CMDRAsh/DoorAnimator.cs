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
    [SerializeField]
    private Material[] stateMaterials; //replace with material library? Attaching all these materials every time seems terribly inefficient.
    [SerializeField]
    private Material[] displayMaterials; //same comment
    //IMPORTANT
    //elem 0 is door L
    //elem 1 is door R

    private bool isLOpen;//door open
    private bool isLClosed;//door closed
    private bool isLOpening;//door in process of opening
    private bool isLClosing;//door in process of closing
    private bool isROpen;
    private bool isRClosed;
    private bool isROpening;
    private bool isRClosing;

    private bool locked;//whether or not the door can be opened
    private bool weldedClosed;//whether or not door has been welded shut
    private bool weldedOpen;//Allow welded open?

    private bool isPowered;
    //speed at which to run animation. NYI
    private float animSpeed;

    private Transform doorL_TF;
    private Transform doorR_TF;
    //door emissive
    private Renderer emissRenderer;

    //panel emissive
    private Renderer[] panelRenderer;

    void Start()
    {
        panelRenderer = new Renderer[controlPanelScreens.Length];
        doorL_TF = anim[0].gameObject.transform;
        doorR_TF = anim[1].gameObject.transform;
        //emissMat = emissiveMesh.GetComponent<Renderer>().material;
        emissRenderer = emissiveMesh.GetComponent<Renderer>();
        //grab all screen renderers
        for (int i = 0; i < controlPanelScreens.Length;i++)
        {
            panelRenderer[i] = controlPanelScreens[i].GetComponent<Renderer>();
        }
        
    }

    void Update()
    {
        //check door positions and animation cycles
        //when the door is at open position, but the animation is not done opening.
        if (doorL_TF.localPosition.z > 1.8f && (isLOpening || isLOpen))
        {
            //stop the left door animation.
            anim[0].enabled = false;
            //return to final position
            doorL_TF.localPosition = new Vector3(doorL_TF.localPosition.x, doorL_TF.localPosition.y, 1.8f);
        }
        else
        {
            anim[0].enabled = true;
        }
        //if closed but not done with closing animation
        if (doorL_TF.localPosition.z < 1.0f && (isLClosing || isLClosed))
        {
            //stop
            anim[0].enabled = false;
            //return to start
            doorL_TF.localPosition = new Vector3(doorL_TF.localPosition.x, doorL_TF.localPosition.y,1.0f);
        }
        else
        {
            anim[0].enabled = true;
        }

        //R Door open, but not done with animation
        if (doorR_TF.localPosition.z < -1.8f) //open past -1.8Z
        {
            //stop the left door animation.
            anim[1].enabled = false;
            //return to final position
            doorR_TF.localPosition = new Vector3(doorR_TF.localPosition.x, doorR_TF.localPosition.y, -1.8f);
        }
        else
        {
            anim[1].enabled = true;
        }
        //if closed but not done with anim
        if (doorR_TF.localPosition.z > -1.0f) //closed past -1.0f
        {
            //stop the left door animation.
            anim[1].enabled = false;
            //return to final position
            doorR_TF.localPosition = new Vector3(doorR_TF.localPosition.x, doorR_TF.localPosition.y, -1.0f);
        }
        else
        {
            anim[1].enabled = true;
        }

        //power behavior
        //if (!isPowered)
       // {
        //    emissRenderer.material = stateMaterials[5];
       // }
    }

    void OnTriggerEnter()
    {
        //Debug.Log("Var IsPowered: " + isPowered);
        if (isPowered && !locked)
        {
            //yellow blinking
            //emissRenderer.material = stateMaterials[1];
            doorRIsOpening();
            doorLIsOpening();
            anim[0].Play("DoorLeftOpen");
            anim[1].Play("DoorRightOpen");
            doorLIsOpen();
            doorRIsOpen();
            //green
            //emissRenderer.material = stateMaterials[0];
        }
    }

    void OnTriggerStay()
    {
        if (isPowered && !locked)//can open
        { 
            //locked = false;
            if ((!isLOpen && !isROpen)||(isROpening && isLOpening))//both doors are open (eval to false) or opening already.
            {
                //yellow blinking
                //emissRenderer.material = stateMaterials[1];
                doorRIsOpening();
                doorLIsOpening();
                anim[0].Play("DoorLeftOpen");
                anim[1].Play("DoorRightOpen");
                doorLIsOpen();
                doorRIsOpen();
                //green
                //emissRenderer.material = stateMaterials[0];
            }
        }
    }

    void OnTriggerExit()
    {
        if (isPowered && !locked)
        {
            //yellow blinking
            //emissRenderer.material = stateMaterials[1];
            doorRIsClosing();
            doorLIsClosing();
            anim[0].Play("DoorLeftClose");
            anim[1].Play("DoorRightClose");
            doorLIsClosed();
            doorRIsClosed();
            //green
            //emissRenderer.material = stateMaterials[0];
        }
    }

    public void lockDoor()
    {
        //lock door (flip bool to prevent opening)
        locked = true;
        //close door
        doorRIsClosing();
        doorLIsClosing();
        anim[0].Play("DoorLeftClose");
        anim[1].Play("DoorRightClose");
        doorLIsClosed();
        doorRIsClosed();
    }

    public void unlockDoor()
    {
        locked = false;
        //green. Assumes no further issues. Will need logic later on.
        emissRenderer.material = stateMaterials[0];
        for(int i = 0; i < panelRenderer.Length; i++)
        {
            panelRenderer[i].material = displayMaterials[0];//MaterialReferences.display_Unlocked;
        }
    }

    public void buttonResponse()
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
            emissRenderer.material = stateMaterials[3];
            for (int i = 0; i < panelRenderer.Length; i++)
            {
                panelRenderer[i].material = displayMaterials[2];//MaterialReferences.display_Locked;
            }
        }
        else
        {
            //change to blinking yellow on open
            emissRenderer.material = stateMaterials[1];
            for (int i = 0; i < panelRenderer.Length; i++)
            {
                panelRenderer[i].material = displayMaterials[1];//MaterialReferences.display_Transit;
            }
        }
    }

    //universal
    public void animEventOpenDone()
    {
        //if locked, change to solid red
        if (locked)
        {
            emissRenderer.material = stateMaterials[2];
            for (int i = 0; i < panelRenderer.Length; i++)
            {
                panelRenderer[i].material = displayMaterials[2];//MaterialReferences.display_Locked;
            }
        }
        else
        {
            //else change to green
            emissRenderer.material = stateMaterials[0];
            for (int i = 0; i < panelRenderer.Length; i++)
            {
                panelRenderer[i].material = displayMaterials[0];//MaterialReferences.display_Unlocked;
            }
        }
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

    public void setPoweredState(bool value)
    {
        isPowered = value;
    }

    public void setAnimSpeed(float speed)
    {
        animSpeed = speed;
    }
}
