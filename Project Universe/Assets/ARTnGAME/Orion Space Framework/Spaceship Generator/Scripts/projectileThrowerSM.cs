using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class projectileThrowerSM : MonoBehaviour
{
    public GameObject throwObj;

    // Start is called before the first frame update
    void Start()
    {
        
    }
    public float speed = 100;
    // Update is called once per frame
    void Update()
    {
        if(throwObj != null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                GameObject throwObj2 = Instantiate(throwObj, transform.position + transform.forward * 1.2f + transform.up * 1.2f, transform.rotation);
                throwObj2.GetComponent<Rigidbody>().useGravity = true;
                throwObj2.GetComponent<Rigidbody>().velocity = transform.forward * speed;
            }
        }
    }
}
