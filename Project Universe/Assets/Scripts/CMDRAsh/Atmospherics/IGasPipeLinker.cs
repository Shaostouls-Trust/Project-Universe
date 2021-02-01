using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IGasPipeLinker : MonoBehaviour
{
    [SerializeField] private IGasPipe parentDuct;
    
    //detect the other duct collider
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("_GasPipeLink"))
        {
            //link the ducts
            parentDuct.AddNeighbor(collision.gameObject.GetComponent<IGasPipeLinker>().parentDuct);
            //collision.gameObject.GetComponent<IGasPipeLinker>().parentDuct.AddNeighbor(parentDuct);
            //disable collisions and rigidbodies
            //Debug.Log("Switching colliders for G/O "+collision.gameObject+" and "+gameObject);
            collision.gameObject.GetComponent<Rigidbody>().detectCollisions = false;
            collision.gameObject.GetComponent<BoxCollider>().enabled = false;
            gameObject.GetComponent<Rigidbody>().detectCollisions = false;
            gameObject.GetComponent<BoxCollider>().enabled = false;
        }       
    }
}
