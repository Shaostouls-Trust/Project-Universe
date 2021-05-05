using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IRadiationZone : MonoBehaviour
{
    [SerializeField] private AnimationCurve exposureFallOff;
    public SphereCollider radiationArea;
    [SerializeField] private float roentgen;
    private float rads;

    void OnTriggerEnter(Collider other)
    {
        PlayerVolumeController playerVC;
        if (other.gameObject.TryGetComponent<PlayerVolumeController>(out playerVC))
        {
            Transform playerTans = other.gameObject.transform;
            float deltaX = playerTans.position.x - transform.position.x;
            float deltaY = playerTans.position.y - transform.position.y;
            float deltaZ = playerTans.position.z - transform.position.z;
            float distance = (float)Mathf.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
            //Debug.Log("raw distance: "+distance);
            //ratio of player distance to center and the radius of the sphere to determine the 0 - 1.0 proximity
            //Debug.Log("Distance ratio: "+distance / radiationArea.radius);
            float level = exposureFallOff.Evaluate(distance/radiationArea.radius);
            //Debug.Log("exposure: " + rads * level);
            playerVC.SetRadiationExposureRate(roentgen * level);
        }
    }

    void OnTriggerStay(Collider other)
    {
        PlayerVolumeController playerVC;
        if (other.gameObject.TryGetComponent<PlayerVolumeController>(out playerVC))
        {
            Transform playerTans = other.gameObject.transform;
            float deltaX = playerTans.position.x - transform.position.x;
            float deltaY = playerTans.position.y - transform.position.y;
            float deltaZ = playerTans.position.z - transform.position.z;
            float distance = (float)Mathf.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
            //Debug.Log("raw distance: " + distance);
            //ratio of player distance to center and the radius of the sphere to determine the 0 - 1.0 proximity
            //Debug.Log("Distance ratio: " + distance / radiationArea.radius);
            float level = exposureFallOff.Evaluate((distance / radiationArea.radius));
            //Debug.Log("exposure: " + rads * level);
            playerVC.SetRadiationExposureRate(roentgen * level);
            playerVC.AddRadiationExposureTime(Time.deltaTime);
        }
    }

    void OnTriggerExit(Collider other)
    {
        PlayerVolumeController playerVC;
        if (other.gameObject.TryGetComponent<PlayerVolumeController>(out playerVC))
        {
            playerVC.SetRadiationExposureRate(0);
        }
    }
}
