using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUniverse.Ship
{
    /// <summary>
    /// Doesn't exactly work. LOL.
    /// </summary>
    public class RigidbodyJointManager : MonoBehaviour
    {
        [SerializeField] private GameObject jointObj;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                /*Debug.Log("linking");
                FixedJoint joint = jointObj.GetComponent<FixedJoint>();//.AddComponent<FixedJoint>();
                joint.connectedBody = other.attachedRigidbody;
                joint.connectedMassScale = 10f;
                joint.anchor = new Vector3(0f, 0f, 0f);
                RigidbodyJointScript rjs = jointObj.GetComponent<RigidbodyJointScript>();
                rjs.PlayerObj = other.gameObject;*/
            }
        }

        /*
        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                ConfigurableJoint target = null;
                // find the joint connecting to this player and break it
                ConfigurableJoint[] joints = jointObj.GetComponents<ConfigurableJoint>();
                foreach(ConfigurableJoint joint in joints)
                {
                    if (joint.connectedBody == other.attachedRigidbody)
                    {
                        target = joint;
                        break;
                    }
                }

                if(target != null)
                {
                    Debug.Log("breaking");
                    Destroy(target);
                }
            }
        }*/
    }
}