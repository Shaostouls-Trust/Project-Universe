using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Hammer : MonoBehaviour
{
    public Animation animation;
    public Rigidbody rb;
    public float hitforce;
    private Vector3 collisionpostion;
    public float blastradius;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    private void OnCollisionEnter(Collision collision)
    {
        collisionpostion = collision.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButton("Fire1"))
        {

            animation.Play("HitAnim");
            rb.AddExplosionForce(hitforce, collisionpostion, blastradius);
              
        
        
        }


    }
}
