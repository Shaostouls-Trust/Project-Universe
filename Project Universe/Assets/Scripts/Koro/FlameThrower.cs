using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameThrower : MonoBehaviour
{

    public float damageamount = 0.1f;
    [HideInInspector]
    protected target trgt;

    private void Start()
    {
        trgt = FindObjectOfType<target>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy")
        {
            trgt.TakeDamage(damageamount);
            Debug.Log("AHHHHHHHHHHHH");
        }
        if (gameObject.activeSelf == true)
        {


        }
    }
}
