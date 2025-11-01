using UnityEngine;
using System.Collections;

public class RoverAudio : MonoBehaviour
{

    public AudioSource jetSound;
    private float jetPitch;
    private const float LowPitch = .5f;
    private const float HighPitch = 1.5f;
    public float SpeedToRevs = 0.1f;

    Rigidbody carRigidbody;

    void Awake()
    {
        carRigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        Vector3 myVelocity = carRigidbody.linearVelocity;
        float forwardSpeed = transform.InverseTransformDirection(carRigidbody.linearVelocity).z;
        float engineRevs = Mathf.Abs(forwardSpeed) * SpeedToRevs;
        jetSound.pitch = Mathf.Clamp(engineRevs, LowPitch, HighPitch);
    }

}