using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassTriggerTo : MonoBehaviour
{
    [SerializeField] private GameObject target;
    [SerializeField] private string passTag;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(passTag))
        {
            target.SendMessage("ProxyTriggerEnter", other, SendMessageOptions.DontRequireReceiver);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag(passTag))
        {
            target.SendMessage("ProxyTriggerExit", other, SendMessageOptions.DontRequireReceiver);
        }
    }

    /*private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag(passTag))
        {
            target.SendMessage("ProxyTriggerStay", other, SendMessageOptions.DontRequireReceiver);
        }
    }*/
}
