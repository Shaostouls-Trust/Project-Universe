using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePlayerController : MonoBehaviour
{
    public InventoryManager inventory;
    public float walkSpeed = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        PlayerMovement();
    }
    void PlayerMovement()
    {
        if (Input.GetAxis("Vertical") != 0)
        {
            gameObject.transform.position += Vector3.forward * walkSpeed * Input.GetAxis("Vertical");
        }
        if (Input.GetAxis("Horizontal") != 0)
        {
            gameObject.transform.position += Vector3.right * walkSpeed * Input.GetAxis("Horizontal");
        }
        if (Input.GetAxis("Mouse X") != 0)
        {
            gameObject.transform.localEulerAngles += Vector3.up * Input.GetAxis("Mouse X");
        }
    }
}
