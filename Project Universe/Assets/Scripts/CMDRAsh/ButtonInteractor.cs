using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// raycast
/// when hits WSButton
/// on keygetinput
/// run func
/// </summary>
namespace ProjectUniverse.Environment.Interactable
{
    public class ButtonInteractor : MonoBehaviour
    {
        // Update is called once per frame
        void Update()
        {
            Vector3 forward = transform.TransformDirection(0f, 0f, 1f) * 1.5f;//1.5m reach
            Debug.DrawRay(transform.position, forward, Color.green);
            //if a 1m.5 raycast hits an object collider
            RaycastHit hit;
            if (Physics.Raycast(transform.position, forward, out hit, 1.5f))
            {
                //Debug.Log("Work, dang you!");
                //bad collision detection? Detection is unreliable.
                if (hit.collider.gameObject.tag == "Button3D")
                {
                    //Debug.Log("Work you piece of dung!");
                    if (Input.GetKeyDown("e"))
                    {
                        //handle the pressing of the button
                        //Debug.Log("External Call");
                        //Need to put on switch later
                        var script = hit.collider.gameObject.GetComponent<WSButton1>();
                        var script2 = hit.collider.gameObject.GetComponent<WSButton2>();
                        var script3 = hit.collider.gameObject.GetComponent<BreakerSwitch>();
                        if (script != null)
                        {
                            //Debug.Log("1");
                            script.externalInteractFunc();
                        }
                        else if (script3 != null)
                        {
                            //Debug.Log("2");
                            script3.externalInteractFunc();
                        }
                        else if(script2 != null)
                        {
                            script2.externalInteractFunc();
                        }
                    }
                }
                else if(hit.collider.gameObject.tag == "Plant")
                {
                    if (Input.GetKeyUp(KeyCode.E))
                    {
                        var scriptFruit = hit.collider.gameObject.GetComponent<FarmPlant>();
                        if(scriptFruit != null)
                        {
                            scriptFruit.PickupPlant();
                        }
                    }
                }
            }
        }
    }
}