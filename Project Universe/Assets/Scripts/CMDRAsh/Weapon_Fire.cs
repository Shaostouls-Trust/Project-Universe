using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon_Fire : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //fire
            Debug.Log("Fire!");
        }
    }
}
