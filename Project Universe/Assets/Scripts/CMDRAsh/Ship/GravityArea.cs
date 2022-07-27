using ProjectUniverse.Player.PlayerController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityArea : MonoBehaviour
{
    [SerializeField] private float gravity_m2;
    private List<SupplementalController> trackedControllers = new List<SupplementalController>();
    [SerializeField] private GravityDirections gravity_direction;

    public enum GravityDirections {
        Up_y,
        Right_x,
        Forward_z
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 dir;
        if(gravity_direction == GravityDirections.Up_y)
        {
            dir = new Vector3(0f, 1f, 0f);
        }
        else if (gravity_direction == GravityDirections.Right_x)
        {
            dir = new Vector3(1f, 0f, 0f);
        }
        else if(gravity_direction == GravityDirections.Forward_z)
        {
            dir = new Vector3(0f, 0f, 1f);
        }
        else
        {
            dir = new Vector3(0f, 0f, 0f);
        }
        foreach(SupplementalController sc in trackedControllers)
        {
            sc.GravityDirection = (gravity_m2 * dir);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out SupplementalController sc))
        {
            Vector3 dir;
            if (gravity_direction == GravityDirections.Up_y)
            {
                dir = new Vector3(0f, 1f, 0f);
            }
            else if (gravity_direction == GravityDirections.Right_x)
            {
                dir = new Vector3(1f, 0f, 0f);
            }
            else if (gravity_direction == GravityDirections.Forward_z)
            {
                dir = new Vector3(0f, 0f, 1f);
            }
            else
            {
                dir = new Vector3(0f, 0f, 0f);
            }

            trackedControllers.Add(sc);
            sc.GravityDirection = (gravity_m2 * dir);
            /// NEED WAY TO TRACK MULTIPLE TRANSFORM DIRECTIONS
            sc.GravityTransform = this.transform;
        }
    }

    /*private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out SupplementalController sc))
        {
            Vector3 dir;
            if (gravity_direction == GravityDirections.Up_y)
            {
                dir = new Vector3(0f, 1f, 0f);
            }
            else if (gravity_direction == GravityDirections.Right_x)
            {
                dir = new Vector3(1f, 0f, 0f);
            }
            else if (gravity_direction == GravityDirections.Forward_z)
            {
                dir = new Vector3(0f, 0f, 1f);
            }
            else
            {
                dir = new Vector3(0f, 0f, 0f);
            }

            trackedControllers.Remove(sc);
            sc.GravityDirection -= (gravity_m2 * dir);
            /// NEED WAY TO TRACK MULTIPLE TRANFORM DIRECTIONS
            sc.GravityTransform = null;
        }
    }*/
}
