using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(Time.time);
    }
}
