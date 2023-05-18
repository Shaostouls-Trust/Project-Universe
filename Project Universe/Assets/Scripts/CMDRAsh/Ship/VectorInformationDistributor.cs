using ProjectUniverse.Player.PlayerController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pass rigidbody and axial movement information to players
/// </summary>
public class VectorInformationDistributor : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    private Vector3 oldEulerAngles;

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out SupplementalController sc))
        {
            if(rb != null)
            {
                Debug.Log("Connecting to RB: " + sc.name);
                sc.FloorMasterRB = rb;
                sc.FloorMasterTransform = transform;
            }
            else
            {
                sc.FloorMasterTransform = transform;
            }
            oldEulerAngles = transform.rotation.eulerAngles;
        }
    }

    /*private void OnTriggerExit(Collider other)
    {
        if(other.TryGetComponent(out SupplementalController sc))
        {
            if (sc.FloorMasterRB = rb)
            {
                sc.FloorMasterRB = null;
                sc.FloorMasterTransform = null;
            }
        }
    }*/

    /// Test method for passing rotation onto players
    ///
    ///Causes mega lag for some reason
    /*
    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out SupplementalController sc))
        {
            //calculate the change in rotation in the last fixedframe
            Vector3 deltaEulers = oldEulerAngles - transform.rotation.eulerAngles;

            //Debug.Log(deltaEulers);
            sc.ShipLastRotationAngles = deltaEulers;
            //sc.ShipLastRotationAxis = new Vector3(1f, 1f, 1f);


            oldEulerAngles = transform.rotation.eulerAngles;
        }
    }*/
}
