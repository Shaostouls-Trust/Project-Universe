using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class doorAnimEventController : MonoBehaviour
{
    //object with material to be animated
    [SerializeField]
    private GameObject parentObject;

    public void emissionStateYellow()
    {
        parentObject.GetComponent<DoorAnimator>().animEventOpen();
    }

    public void emissionStateGreen()
    {
        parentObject.GetComponent<DoorAnimator>().animEventOpenDone();
    }
}
