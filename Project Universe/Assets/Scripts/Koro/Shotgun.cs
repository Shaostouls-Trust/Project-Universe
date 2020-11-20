
using JetBrains.Annotations;
using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.Video;

public class Shotgun : MonoBehaviour
{
    public float damage = 10f;
    public float range = 100f;
    public Camera fpsCam;
    public AudioSource gunSfx;
    public ParticleSystem muzzleflash;
    public GameObject impactEffect;
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
    private Vector3 hitscale;
    public float HitScaleMultiplier = 3f;
    public AudioSource reloadbullets;
    public float originalreloadtime = 1f;
    

    private void FixedUpdate()
    {
        if (numberofbulletsshot == magammount)
        {   
            canshoot = false;
            reloadtime -= Time.deltaTime;
            if (reloadtime <= 0)
            {
                if (!reloadbullets.isPlaying)
                {
                    reloadbullets.Play();
                }
                numberofbulletsshot = 0;
                    canshoot = true;
                reloadtime = originalreloadtime;
            }
        }
    }
 
    // Update is called once per frame
    void Update()
    {
        if (Input.GetButton("Fire1") && Time.time >= nexttimetofire && canshoot == true)
        {
            nexttimetofire = Time.time + 1f / fr;
            Shoot();
            gunSfx.Play();
            reloadsound.Play();

        }
    }

    void Shoot()
    {   numberofbulletsshot += 1;
        muzzleflash.Play();
        RaycastHit hit;
        
        rb.AddForce(transform.position * recoilforce);
      if  (Physics.SphereCast(fpsCam.transform.position, HitScaleMultiplier ,fpsCam.transform.forward, out hit , range))
        {
            hitscale = hit.normal;
            target target = hit.transform.GetComponent<target>();
            if(target != null)
            {

                target.TakeDamage(damage);
            }

            if(hit.rigidbody != null)
            {

               
                hit.rigidbody.AddForce(-hit.normal * forceonimpact);
                
            }
           GameObject impactGO = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(impactGO, 1.5f);
        }

    }
}
