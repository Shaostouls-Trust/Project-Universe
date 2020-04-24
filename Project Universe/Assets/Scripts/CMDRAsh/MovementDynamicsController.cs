using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using System;

public class MovementDynamicsController : MonoBehaviour
{
    private Vector3 position;//duh
    private Vector3 oldPosition;//duh
    public Transform ObjTransform;
    [SerializeField]
    private VisualEffect visualEffect;
    private float movement;
    private Vector3 fConst;

    // Start is called before the first frame update
    void Start()
    {
        //This will grab the positonal data before first frame update, so the very first frame's particles will be the proper size.
        oldPosition = ObjTransform.position;
        //access the VFX graph parameter "Movement Dynamics"
        movement = visualEffect.GetFloat("MovementDynamics");
        //Parameter to adjust particle speed
        fConst = visualEffect.GetVector3("FallingVelocity");
    }

    void Update()
    {
        //get current position
        position = ObjTransform.position;
        if (position.y != oldPosition.y || position.x != oldPosition.x || position.z != oldPosition.z)
        {
            //vector of movement in 3 dimensions
            Vector3 heading = position - oldPosition;
            //magnitude of the movement vector
            var distance = heading.magnitude;
            //arbitrarally adjust the distance
            float adjustedDistance = distance / .1f;
            //add the distance to the movement dynamic 
            adjustedDistance += 1;
            movement = adjustedDistance;
            visualEffect.SetFloat("MovementDynamics", adjustedDistance);
            //speed up particles
            //multiply speed in x,y,z by distance
            float x = (fConst.x + 1) * (-1 * heading.x);
            float y = (fConst.y + 1) * (-1 * heading.y);
            float z = (fConst.z + 1) * (-1 * heading.z);
            visualEffect.SetVector3("FallingVelocity", new Vector3(x, y, z));
        }
        //next cycle update
        oldPosition = position;
    }

    void OnCollisionEnter(Collision other)
    {
        visualEffect.SetVector3("FallingVelocity", new Vector3(0, 0, 0));
    }
}
