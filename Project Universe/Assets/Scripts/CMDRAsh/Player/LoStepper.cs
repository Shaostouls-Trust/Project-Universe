using ProjectUniverse.Player.PlayerController;
using ProjectUniverse.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Step onto stairs and other low-to-the-ground objects.
/// </summary>
public class LoStepper : MonoBehaviour
{
    [SerializeField] private SupplementalController controller;
    [SerializeField] private Vector3 rayOffset = new Vector3(0f, 0.175f, 0f);
    [SerializeField] private bool forwardOnly;
    [SerializeField] private float startYOffset = 0.15f;
    [SerializeField] private float endYOffset = 0.35f;
    [SerializeField] private int rayDensity = 5;
    [SerializeField] private bool stepUpInput;//require the spacebar be pressed to scale
    [SerializeField] private bool requireMotion;//require motion in the direction of scale
    private bool forward;
    private bool back;
    private bool left;
    private bool right;

    private bool forwardLo;
    private bool backLo;
    private bool leftLo;
    private bool rightLo;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            //Debug.Log("Raycast");
            Vector3 offset = rayOffset + transform.position;
            float distance = 0.35f;
            //check this stepper's directions
            Utils.RayCastCheckSide(transform, offset, distance, out forward, out back, out left, out right, forwardOnly);
            //Debug.Log(forward+" "+back+" "+left+" "+right);
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            Vector3 offset = rayOffset + transform.position;
            float distance = 0.35f;
            Utils.RayCastCheckSide(transform, offset, distance, out forward, out back, out left, out right, forwardOnly);
            //Debug.Log(forward + " " + back + " " + left + " " + right);
        }
    }

    private void FixedUpdate()
    {      
        if (!controller.crouching && !controller.prone)
        {
            Vector3 offset = rayOffset + transform.position;
            float distance = 0.35f;
            //check this stepper's directions
            //Utils.RayCastCheckSide(transform, offset, distance, out forward, out back, out left, out right, forwardOnly);
            
            controller.HiStepReady = false;
            //StepUpCheck();
            float stepDistance = 0f;
            
            /// PROJECT VECTORS ON FLOOR TO PREVENT RAMP ISSUES?
            /// 
            /// front
            Vector3 rayPos = transform.position + new Vector3(0f, 0.45f, -0.34f);//0.35f
            Debug.DrawRay(rayPos, -transform.up * 0.35f);
            RaycastHit[] hits = Physics.RaycastAll(rayPos, -transform.up * 0.34f);
            if (hits.Length > 0)
            {
                for (int i = 0; i < hits.Length; i++)
                {//!hits[i].transform.CompareTag("_VolumeArea") &&
                    if (!hits[i].collider.isTrigger && !hits[i].transform.CompareTag("Player"))
                    {
                        stepDistance = (hits[i].point.y - controller.transform.position.y);
                        stepDistance *= 1.1f;
                        if(stepDistance <= 0f)
                        {
                            stepDistance = 0f;
                        }
                        //else
                        //{
                            //Debug.Log("StepDistance: " + stepDistance);
                        //}

                    }
                }
                controller.Grounded(true);
                controller.RB.MovePosition(new Vector3(0f, stepDistance, 0f) + controller.transform.position);
            }
        }

    }

    public void StepUpCheck()
    {
        Vector3 localVel = controller.LocalVelocity;
        //forward movement check
        if (forward && !forwardLo)
        {
            if (requireMotion)
            {
                // if local velocity if +z
                if (localVel.z > 0f)
                {
                    //attempt to step up
                    StepUp(localVel.z, 1);
                }
            }
            else
            {
                StepUpNoVelocity();
            }
        }
        if (forwardOnly)
        {
            if (back && !backLo)
            {
                if (requireMotion)
                {
                    // local velocity -z
                    if (localVel.z < 0f)
                    {
                        //attempt to step up
                        StepUp(-localVel.z, 2);
                    }
                }
                else
                {
                    StepUpNoVelocity();
                }
            }
            if (left && !leftLo)
            {
                if (requireMotion)
                {
                    // local velocity -x
                    if (localVel.x < 0f)
                    {
                        //attempt to step up
                        StepUp(-localVel.x, 3);
                    }
                }
                else
                {
                    StepUpNoVelocity();
                }
            }
            if (right && !rightLo)
            {
                if (requireMotion)
                {
                    // local velocity +x
                    if (localVel.x > 0f)
                    {
                        //attempt to step up
                        StepUp(localVel.x, 4);
                    }
                }
                else
                {
                    StepUpNoVelocity();
                }
            }
        }
    }

    /// <summary>
    /// Move the player up as a function of their abs velocity.
    /// Determine the slope of stairs?
    /// </summary>
    public void StepUp(float relevantVelocity, int direction)
    {
        if (relevantVelocity != 0f)
        {
            Vector3 dir;
            switch (direction)
            {
                case 1:
                    dir = transform.forward;
                    break;
                case 2:
                    dir = -transform.forward;
                    break;
                case 3:
                    dir = -transform.right;
                    break;
                case 4:
                    dir = transform.right;
                    break;
                default:
                    dir = transform.forward;
                    break;
            }
            float stepDistance = 0f;
            ///singular ray
            // raycast downwards from the startPos and get the rayhit point
            Vector3 rayPos = transform.position + new Vector3(0f, 0.35f,0.35f);
            RaycastHit[] hits = Physics.RaycastAll(rayPos, -transform.up, 0.35f);
            for(int i = 0; i < hits.Length; i++)
            {
                if (!hits[i].collider.isTrigger)
                {
                    stepDistance = 0.35f - hits[i].point.y;
                }
            }

            ///incremental rays
            /*
            Vector3 startPos = transform.position;
            startPos.y += startYOffset;// 0.15f;
            Vector3 endPos = startPos;
            endPos.y += endYOffset;//0.35f;
            //trigger is .35 radius, .01 is safety factor
            stepDistance = Utils.GetNearestEdge(startPos, endPos, dir, rayDensity, .36f);
            */
            Debug.Log("+" + stepDistance);
            //move the player up by stepDistance + startPos difference (.1f) * a func of relevantVelocity
            //factor is % of relVel that is in direction of motion
            float factor = relevantVelocity / controller.LocalVelocity.magnitude;
            //controller.LocalVelocity.magnitude / relevantVelocity;
            float finalDistance = (stepDistance + 0.1f) * factor;//.075 might need to change?
            Debug.Log("=" + finalDistance);
            //if (!stepUpInput)
            //{
            //controller.HiStepReady = true;
            //}
            //else
            //{
            controller.RB.MovePosition(new Vector3(0f, finalDistance, 0f) + controller.transform.position);
            //}
        }
    }

    public void StepUpNoVelocity()
    {
        Vector3 dir = transform.forward;
        Vector3 startPos = transform.position;
        startPos.y += startYOffset;// 0.15f;
        Vector3 endPos = startPos;
        endPos.y += endYOffset;//0.35f;
                               //trigger is .35 radius, .01 is safety factor
        float stepDistance = Utils.GetNearestEdge(startPos, endPos, dir, rayDensity, .36f);
        Debug.Log("+" + stepDistance);
        float finalDistance = (stepDistance + 0.5f);
        //if (!stepUpInput)
        //{
        //    controller.HiStepReady = true;
        //    controller.HiStepAmount = finalDistance;
        //}
        //else
        //{
        //    controller.HiStepReady = false;
        //    controller.HiStepAmount = 0f;
            controller.RB.MovePosition(new Vector3(0f, finalDistance, 0f) + controller.transform.position);
        //}
    }

    public void Receiver(object[] bools)
    {
        forwardLo = (bool)bools[0];
        backLo = (bool)bools[1];
        leftLo = (bool)bools[2];
        rightLo = (bool)bools[3];
    }
}
