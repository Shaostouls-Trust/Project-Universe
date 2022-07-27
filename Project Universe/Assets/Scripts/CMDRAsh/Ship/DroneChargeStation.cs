using ProjectUniverse.Ship;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneChargeStation : MonoBehaviour
{
    [SerializeField] private int chargeRate;

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Drone"))
        {
            ShipController sc = other.gameObject.GetComponent<ShipController>();
            if(sc.CurrentEnergy < sc.MaxEnergy)
            {
                sc.CurrentEnergy += (chargeRate * Time.deltaTime);
            }
        }
    }
}
