using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Refil : MonoBehaviour
{



    public GameObject grenade;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {

            grenade.SendMessage("refil");
        }
    }
}
