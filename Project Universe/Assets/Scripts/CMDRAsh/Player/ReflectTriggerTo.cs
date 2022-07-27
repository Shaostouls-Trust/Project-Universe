using ProjectUniverse.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReflectTriggerTo : MonoBehaviour
{
    public GameObject reflectTo;
    [SerializeField] private Vector3 rayOffset = new Vector3(0f, 0.4f, 0f);
    [SerializeField] private bool forwardOnly;    
    private bool forward;
    private bool back;
    private bool left;
    private bool right;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            //Debug.Log("Triggered");
            Vector3 offset = rayOffset + transform.position;
            float distance = 0.375f;
            Utils.RayCastCheckSide(transform, offset, distance, out forward, out back, out left, out right, forwardOnly);
            //Debug.Log(forward);
            object[] obj = { forward, back, left, right };
            reflectTo.SendMessage("Receiver", obj, SendMessageOptions.DontRequireReceiver);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            Vector3 offset = rayOffset + transform.position;
            float distance = 0.375f;
            Utils.RayCastCheckSide(transform, offset, distance, out forward, out back, out left, out right, forwardOnly);
            //Debug.Log(forward);
            object[] obj = { forward, back, left, right };
            reflectTo.SendMessage("Receiver", obj, SendMessageOptions.DontRequireReceiver);
        }
    }

}
