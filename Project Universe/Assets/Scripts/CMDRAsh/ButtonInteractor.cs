using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// raycast
/// when hits WSButton
/// on keygetinput
/// run func
/// </summary>

public class ButtonInteractor : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 forward = transform.TransformDirection(0f, 0f, 1f) * 10;
        Debug.DrawRay(transform.position, forward, Color.green,1.0f);
        //if a 1m raycast hits an object collider
        RaycastHit hit;
        if(Physics.Raycast(transform.position,forward,out hit, 1.0f))
        {
            //Debug.Log("Work, dang you!");
            if(hit.collider.gameObject.tag == "Button3D")
            {
                if (Input.GetKeyDown("e"))
                {
                    //handle the pressing of the button
                    Debug.Log("Pressed");
                    //Need to put on switch later
                    var script = hit.collider.gameObject.GetComponent<WSButton1>();
                    script.externalInteractFunc();
                }
            }
        }
    }
}
