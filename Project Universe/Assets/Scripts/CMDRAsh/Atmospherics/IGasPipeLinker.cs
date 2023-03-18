using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUniverse.Environment.Gas
{
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
                //Debug.Log("Collison Gameobject: "+collision.gameObject);
                collision.gameObject.GetComponent<Rigidbody>().detectCollisions = false;
                collision.gameObject.GetComponent<BoxCollider>().enabled = false;
                //Debug.Log("Origin Gameobject: " + gameObject);
                gameObject.GetComponent<Rigidbody>().detectCollisions = false;
                gameObject.GetComponent<BoxCollider>().enabled = false;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("_GasPipeLink"))
            {
                //link the ducts
                parentDuct.AddNeighbor(other.gameObject.GetComponent<IGasPipeLinker>().parentDuct);
                //disable collisions and rigidbodies
                //other.gameObject.GetComponent<Rigidbody>().detectCollisions = false;
                //other.gameObject.GetComponent<BoxCollider>().enabled = false;
                //gameObject.GetComponent<Rigidbody>().detectCollisions = false;
                //gameObject.GetComponent<BoxCollider>().enabled = false;

                //Disabling RB/Col not sufficient. Disable GO
                gameObject.SetActive(false);
            }
        }

        public void SetParentDuct(IGasPipe pipe)
        {
            parentDuct = pipe;
        }
    }
}