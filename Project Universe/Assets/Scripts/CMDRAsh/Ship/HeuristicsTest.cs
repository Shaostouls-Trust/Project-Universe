using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeuristicsTest : MonoBehaviour
{
   
    public Vector3 rotationAxis;
    public float rotationSpeed = 0f;
    public Vector3 translations;

    // Update is called once per frame
    void FixedUpdate()
    {
        if(TryGetComponent(out Rigidbody rb))
        {
            rb.MoveRotation(rb.rotation * Quaternion.Euler(rotationAxis * (rotationSpeed * Time.deltaTime)));

            //rb.MovePosition(rb.position + (translations * Time.deltaTime));//does not affect velocity
            rb.velocity = translations;
        }
        else
        {
            transform.Rotate(rotationAxis, (rotationSpeed * Time.deltaTime));
            transform.position += translations * Time.deltaTime;
        }
        //Debug.Log("Angle: " + (transform.rotation.eulerAngles));
    }
}
