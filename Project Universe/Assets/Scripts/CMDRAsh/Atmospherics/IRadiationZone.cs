using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IRadiationZone : MonoBehaviour
{
    [SerializeField] private AnimationCurve exposureFallOff;
    public SphereCollider radiationArea;
    [SerializeField] private float roentgen;
    private float rads;

    // Start is called before the first frame update
    void Start()
    {
        string comp = "";
        comp += "Rads at d=0: " + roentgen * exposureFallOff.Evaluate(0)+"\n";
        comp += "Rads at d=0.1: " + roentgen * exposureFallOff.Evaluate(0.1f) + "\n";
        comp += "Rads at d=0.2: " + roentgen * exposureFallOff.Evaluate(0.2f) + "\n";
        comp += "Rads at d=0.3: " + roentgen * exposureFallOff.Evaluate(0.3f) + "\n";
        comp += "Rads at d=0.4: " + roentgen * exposureFallOff.Evaluate(0.4f) + "\n";
        comp += "Rads at d=0.5: " + roentgen * exposureFallOff.Evaluate(0.5f) + "\n";
        comp += "Rads at d=0.6: " + roentgen * exposureFallOff.Evaluate(0.6f) + "\n";
        comp += "Rads at d=0.7: " + roentgen * exposureFallOff.Evaluate(0.7f) + "\n";
        comp += "Rads at d=0.8: " + roentgen * exposureFallOff.Evaluate(0.8f) + "\n";
        comp += "Rads at d=0.9: " + roentgen * exposureFallOff.Evaluate(0.9f) + "\n";
        comp += "Rads at d=1.0: " + roentgen * exposureFallOff.Evaluate(1) + "\n";

        Debug.Log(comp);
    }

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
