using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LAVA : MonoBehaviour
{
    public Transform RespawnPoint;
    public Transform playertransform;
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {

            playertransform.position = RespawnPoint.position;

        }
    }
}
