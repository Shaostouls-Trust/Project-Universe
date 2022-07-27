using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DirectionalAccelerometer : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private TMP_Text up;
    [SerializeField] private TMP_Text forward;
    [SerializeField] private TMP_Text right;
    [SerializeField] private TMP_Text pitch;
    [SerializeField] private TMP_Text yaw;
    [SerializeField] private TMP_Text roll;

    private void OnGUI()
    {
        if (rb == null)
        {
            return;
        }
        Vector3 velocity = rb.transform.InverseTransformDirection(rb.velocity);
        Vector3 angularVelocity = rb.angularVelocity;

        up.text = Math.Round(velocity.y,1).ToString();
        forward.text = Math.Round(velocity.z,1).ToString();
        right.text = Math.Round(velocity.x,1).ToString();
        pitch.text = Math.Round(angularVelocity.x,2).ToString();
        yaw.text = Math.Round(angularVelocity.y,2).ToString();
        roll.text = Math.Round(angularVelocity.z,2).ToString();
    }
}
