using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RefilAmmo : MonoBehaviour
{



    public GameObject P;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {

            P.SendMessage("RefilHealth");
        }
    }
}
