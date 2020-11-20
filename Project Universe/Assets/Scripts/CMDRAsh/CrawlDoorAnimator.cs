using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrawlDoorAnimator : MonoBehaviour
{
    public float openBound;
    public float closeBound;
    [SerializeField]
    private Animator anim;
    private bool isClosed;
    private bool isOpen;
    private bool isOpening;
    private bool isClosing;

    private bool weldedClosed;
    
    private Transform doorTF;

    public void ButtonResponse()
    {
        //Debug.Log("Crawl Door");
        if (isClosed || isClosing)
        {
            isOpening = true;
            isClosing = false;
            //Debug.Log("Crawl Door Open");
            anim.Play("CrawlDoorOpen");
            isOpen = true;
            isClosed = false;
        }
        else if(isOpen || isOpening)
        {
            isOpening = false;
            isClosing = true;
            //Debug.Log("Crawl Door Close");
            anim.Play("CrawlDoorClose");
            isOpen = false;
            isClosed = true;
        }

    }

    void Start()
    {
        isClosed = true;
        isClosing = false;
        isOpen = false;
        isOpening = false;
        doorTF = anim.gameObject.transform;
    }

    /*
    void Update()
    {
        if(doorTF.localEulerAngles.y > closeBound && (isClosing || isClosed))
        {
            anim.enabled = false;
            doorTF.localRotation = Quaternion.Euler(doorTF.localEulerAngles.x,
                closeBound, doorTF.localEulerAngles.z);
        }
        else
        {
            anim.enabled = true;
        }
        if(doorTF.localEulerAngles.y < openBound && (isOpening || isOpen))
        {
            anim.enabled = false;
            doorTF.localRotation = Quaternion.Euler(doorTF.localEulerAngles.x,
                openBound, doorTF.localEulerAngles.z);
        }
        else
        {
            anim.enabled = true;
        }
    }
    */
}
