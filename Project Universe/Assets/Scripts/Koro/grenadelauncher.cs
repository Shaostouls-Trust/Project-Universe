using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class grenadelauncher : MonoBehaviour
{
    public GameObject guntip;
    public float shootforce = 40f;
    public GameObject grenadeprefab;
    private float NumberofGrenadesShot;
    public float MagAmount = 12f;
    public float reloadtime = 1f;
    private bool canshoot = true;
    public AudioSource reloadsound;
    public float originalreloadtime = 1f;



    private void Start()
    {
 
       
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && canshoot)
        {

            NumberofGrenadesShot += 1;

            ShootTheGrenade();
        }
    }
    void ShootTheGrenade()
    {
        GameObject grenade = Instantiate(grenadeprefab, transform.position, transform.rotation);
        Rigidbody rb = grenade.GetComponent<Rigidbody>();
        rb.AddForce(guntip.transform.forward * shootforce, ForceMode.VelocityChange);

    }

    private void FixedUpdate()
    {
        if (NumberofGrenadesShot == MagAmount)
        {
            canshoot = false;
            reloadtime -= Time.deltaTime;
            if (reloadtime <= 0)
            {
                reloadsound.Play();
                NumberofGrenadesShot = 0;
                canshoot = true;
                reloadtime = originalreloadtime;
            }

        }
    }
}
