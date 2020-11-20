using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlasmaGun : MonoBehaviour
{
    public float damage = 10f;
    private LineRenderer lr;
    public float range = 100f;
    public Camera fpsCam;
    public AudioSource gunSfx;
    public ParticleSystem muzzleflash;
    public GameObject impactEffect;
    public Transform gunTip;
    public float forceonimpact = 30f;
    public float fr = 10f;
    private float nexttimetofire = 0f;
    public Rigidbody rb;
    public float recoilforce = 1f;
    private float numberofbulletsshot;
    public float magammount = 30f;
    public AudioSource reloadsound;
    public float reloadtime = 1f;
    private bool canshoot = true;
    public float originalreloadtime = 1f;
    public bool isshooting = false;
    public float laserlength = 3f;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
    }
    private void FixedUpdate()
    {
        if (numberofbulletsshot == magammount)
        {
            canshoot = false;
            reloadtime -= Time.deltaTime;
            if (reloadtime <= 0)
            {
                reloadsound.Play();
                numberofbulletsshot = 0;
                canshoot = true;
                isshooting = false;
                reloadtime = originalreloadtime;
            }
        }
    }


    // Update is called once per frame
    void Update()
    {

        if (isshooting == false)
        {
            lr.enabled = false;


        }
        if (isshooting == true)
        {
            lr.enabled = true;


        }

        if (Input.GetButton("Fire1") && Time.time >= nexttimetofire && canshoot == true)
        {
            isshooting = true;
            nexttimetofire = Time.time + 1f / fr;
            Shoot();
            gunSfx.Play();

        }
        if (Input.GetButtonUp("Fire1"))
        {
            isshooting = false;

        }
    }
    void Shoot()
    {
        Vector3 pos = gunTip.transform.position + gunTip.forward * laserlength; 

        numberofbulletsshot += 1;
        muzzleflash.Play();
        RaycastHit hit;
        RaycastHit plasmahit;

        rb.AddForce(transform.position * recoilforce);
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {




            target target = hit.transform.GetComponent<target>();
            if (target != null)
            {

                target.TakeDamage(damage);
            }

            if (hit.rigidbody != null)
            {


                hit.rigidbody.AddForce(-hit.normal * forceonimpact);

            }

            GameObject impactGO = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(impactGO, 1.5f);
        }
        if ( isshooting)
        {
          
            lr.positionCount = 2;
            lr.SetPosition(0, gunTip.position);
            lr.SetPosition(1, pos);
        }


    }
}
