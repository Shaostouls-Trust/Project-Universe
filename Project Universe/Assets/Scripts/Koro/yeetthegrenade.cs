using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class yeetthegrenade : MonoBehaviour
{
    public float throwforce = 40f;
    public GameObject grenadeprefab;
    public float grenadeamount = 3f;
    private float grenadethrown;
    private bool canyeet = true;
    private void FixedUpdate()
    {
        if (grenadethrown == grenadeamount)
        {

            canyeet = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G) && canyeet )
        {

            grenadethrown += 1f;
            throwgrenadepls();

        }
    }
    void throwgrenadepls()
    {

        GameObject grenade = Instantiate(grenadeprefab, transform.position, transform.rotation);
        Rigidbody rb = grenade.GetComponent<Rigidbody>();
        rb.AddForce(transform.forward * throwforce, ForceMode.VelocityChange);
        
    }
   public void refil()
    {

        grenadethrown = 0;
        canyeet = true;
    }
}
