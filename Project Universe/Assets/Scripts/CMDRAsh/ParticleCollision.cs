using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class ParticleCollision : MonoBehaviour
{
    [SerializeField]
    private VisualEffect visualEffect;
    //Box collider doing the detecting
    private BoxCollider box;
    [SerializeField]
    private GameObject colliderObject;//collider is now with parent.
    //V3 arraylist for points of collision
    private ArrayList collidePoints = new ArrayList();

    /*
     * Collision map generator for our particles. 
     * A box trigger gets the points of contact with each collider it detects.
     * It then calculates the midpoint from the first collider point and the last collider point. 
     * A 16-bit t3d map is created. We begin plotting points by color on the map.
     * The first point becomes true blue, the last becomes true red.
     * The midpoint values are true green. Everything else is a colorslide from green to red or blue, depending on location.
     * Once the map is made, save it as a t3dasset for next runs. At the very least, it needs passed to the vfx collider.
     */

    //invert the box collider's normals
    void Start()
    {
        box = colliderObject.GetComponent<BoxCollider>();
    }
    
    //Update with physics, as this is a physics-dependant component
    void FixedUpdate() 
    { 
        /*
    
        if(currentDims.x < targetDims.x)
        {
            currentDims.x += .1f;
        }
        if (currentDims.y < targetDims.y)
        {
            currentDims.y += .1f;
            center.y += .05f;
        }
        if (currentDims.z < targetDims.z)
        {
            currentDims.z += .1f;
        }
        box.size = currentDims;
        box.center = center;
        */
    }

    void OnCollisionEnter(Collision other)
    {
        Debug.Log("Git Some!");
    }

    //Collision event. We need to check all points detected and see if they are greater than 0y.
    //if they are, add them to the list of points.
    void OnTriggerEnter(Collider others)
    {
        Debug.Log("Ah! You hit "+others.name);
        //get the collision meshes

        //List<Collider> tempList = new List<Collider>(5);
        //Get the colliders of all the objects inside
        //get the contact points of the collision
        //int num = others.GetContacts(tempList);
        //Debug.Log(num);
        //pull out the V3 point data
        //    foreach (ContactPoint pt in tempList)
        //    {
        //get point data
        //        Vector3 vec = pt.point;
        //If we're not colliding with the floor
        //        if (vec.y > 0)
        //        {
        //            Debug.Log("We've collided with " + others.transform.name);
        //            collidePoints.Add(vec);
        //        }
        //    }
        //    tempList.Clear();
    }

    void OnTriggerStay(Collider others)
    {
        //Debug.Log("WEEEE!!");
    }

    void OnTriggerExit(Collider others)
    {
        //Debug.Log("Away!");
    }
}
