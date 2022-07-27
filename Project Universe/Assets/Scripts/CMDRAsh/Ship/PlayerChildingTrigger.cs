using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerChildingTrigger : MonoBehaviour
{
    [SerializeField]
    public Transform Root;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (Root != null)
            {
                other.gameObject.transform.SetParent(Root);
            }
            else
            {
                other.gameObject.transform.SetParent(null);
            }
        }
        
    }

    /*private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.gameObject.transform.SetParent(null);
        }
    }*/
}
