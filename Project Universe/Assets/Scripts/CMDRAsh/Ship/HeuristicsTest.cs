using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeuristicsTest : MonoBehaviour
{
   
    public Vector3 rotationAxis;
    public float rotationSpeed = 0f;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(rotationAxis, (rotationSpeed * Time.deltaTime));
    }
}
