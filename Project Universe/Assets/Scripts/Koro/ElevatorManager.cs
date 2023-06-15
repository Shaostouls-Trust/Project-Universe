using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorManager : MonoBehaviour
{
    public int CurLevel;
    public Transform LevelDir;
    public Transform[] levels;
    public Transform elevatorPlatform;
    public float elevatorspeed;
    //public float elevatorOriginToFloorDistance;
    //public float elevatorOriginToWallInXDist;
    //public float elevatorOriginToWallInZDist;
    private Vector3 elevatorVelocity;//speed * unit direction
    private bool Move = false;
    [SerializeField] private bool X_;
    [SerializeField] private bool XY;
    [SerializeField] private bool XZ;
    [SerializeField] private bool Y_;
    [SerializeField] private bool YZ;
    [SerializeField] private bool Z_;
    [SerializeField] private AudioSource src;
    [SerializeField] private AudioClip[] clips;

    //hitting up when at top plays the up sound once,
    //and then trying to go back down always takes two pushes, which on the second moves down two levels
    public void Up()
    {
        if (!Move)
        {
            // this is for sending it up one level
            Move = true;
            CurLevel += 1;

            //if we aren't at the top
            if (CurLevel <= levels.Length - 1)
            {
                Debug.Log("received up call");
                LevelDir = levels[CurLevel];
                StartCoroutine(ElevatorMovement());
            }
            else
            {
                Debug.Log("at top");
                Move = false;
                CurLevel = levels.Length - 1;
            }
        }
    }
    public void SpecificLevel(int levelSpecified)
    {
        if (!Move)
        {
            //this is if you want to move the elevator to specified level like an actual elevator
            // just make sure you add the int in the call function in elevator btn script
            Debug.Log("received call to " + levelSpecified);
            Move = true;
            LevelDir = levels[levelSpecified];
            CurLevel = levelSpecified;
            StartCoroutine(ElevatorMovement());
        }
    }
    public void Down()
    {
        if (!Move)
        {
            //this is for sending it down one level
            Move = true;
            CurLevel -= 1;

            if (CurLevel >= 0)
            {
                Debug.Log("received down call");
                LevelDir = levels[CurLevel];
                StartCoroutine(ElevatorMovement());
            }
            else
            {
                Debug.Log("already at bottom");
                Move = false;
                CurLevel = 0;
            }
        }
    }

    //private void Update()
    //{
        //i made it in the update function so that you can animate it
        // you can also parent the player to the elevator or change the players position with the elevator so that you dont have to depend on the colliders
        //if (Move)
        //{
            //elevatorPlatform.position = LevelDir.position;
            //Move = false;
        //}
    //}

    IEnumerator ElevatorMovement()
    {
        src.PlayOneShot(clips[0]);
        //shaft positions need translated to world space
        Vector3 worldSpaceDest = LevelDir.TransformPoint(LevelDir.GetComponent<ElevatorPlatform>().ShaftCenterForLevel);
        float xD = worldSpaceDest.x - (float)Math.Round(elevatorPlatform.transform.position.x, 2);
        float yD = LevelDir.GetComponent<ElevatorPlatform>().ShaftCenterForLevel.y - (float)Math.Round(elevatorPlatform.transform.localPosition.y, 2);
        float zD = worldSpaceDest.z - (float)Math.Round(elevatorPlatform.transform.position.z, 2);
        //Debug.Log(LevelDir.GetComponent<ElevatorPlatform>().ShaftCenterForLevel.y + " - " + (float)Math.Round(elevatorPlatform.transform.localPosition.y, 2));
        //Debug.Log(yD);

        //if y etc
        if (Y_)
        {
            elevatorVelocity = new Vector3(0f, yD, 0f).normalized;
        }
        else if (XY)
        {
            elevatorVelocity = new Vector3(xD, yD, 0f).normalized;
        }
        else if (YZ)
        {
            elevatorVelocity = new Vector3(0f, yD, zD).normalized;
        }
        //if x position is not at the end
        else if (X_ || XZ)
        {
            elevatorVelocity = new Vector3(xD, 0f, 0f).normalized;
        }
        else if (XZ)
        {
            elevatorVelocity = new Vector3(xD, 0f, zD).normalized;
        }
        //if z etc
        else if (Z_)
        {
            elevatorVelocity = new Vector3(0f, 0f, zD).normalized;
        }
        //move elevator along velocity axis at elevator speed until elevator reaches target
        while (Move)
        {
            if (!src.isPlaying)
            {
                src.Play();
            }
            float elevY = elevatorPlatform.transform.localPosition.y;//global
            float elevX = elevatorPlatform.transform.localPosition.x;
            float elevZ = elevatorPlatform.transform.localPosition.z;
            float posYadj = LevelDir.GetComponent<ElevatorPlatform>().ShaftCenterForLevel.y; // - elevatorOriginToFloorDistance;
            float posXadj = LevelDir.GetComponent<ElevatorPlatform>().ShaftCenterForLevel.x; //- elevatorOriginToWallInXDist;
            float posZadj = LevelDir.GetComponent<ElevatorPlatform>().ShaftCenterForLevel.z; //- elevatorOriginToWallInZDist;
            //Debug.Log(elevY+" to "+ posYadj);
            if (Y_)
            {
                if(yD < 0f)//down
                {
                    //Debug.Log("152: " + elevY + " to " + posYadj);
                    if ((float)Math.Round(posYadj - elevY, 2) >= 0f)//<
                    { 
                        Move = false;
                    }
                }
                else //up
                {
                    if ((float)Math.Round(posYadj - elevY, 2) <= 0f)//>
                    {
                        //Debug.Log("136");
                        Move = false;
                    }
                }
            }
            else if (XY)
            {
                if (yD < 0f)//down
                {
                    if ((float)Math.Round(posYadj - elevY, 2) >= 0f)
                    {
                        Move = false;
                    }
                }
                else //up
                {
                    if ((float)Math.Round(posYadj - elevY, 2) <= 0f)
                    {
                        Move = false;
                    }
                }
            }
            else if (YZ)
            {
                if (yD < 0f)//down
                {
                    if ((float)Math.Round(posYadj - elevY, 2) >= 0f)
                    {
                        Move = false;
                    }
                }
                else //up
                {
                    if ((float)Math.Round(posYadj - elevY, 2) <= 0f)
                    {
                        Move = false;
                    }
                }
            }
            //if x position is not at the end
            else if (X_)
            {
                if (xD < 0f)//down
                {
                    if ((float)Math.Round(posXadj - elevX, 2) >= 0f)
                    {
                        Move = false;
                    }
                }
                else //up
                {
                    if ((float)Math.Round(posXadj - elevX, 2) <= 0f)
                    {
                        Move = false;
                    }
                }
            }
            else if (XZ)
            {
                if (xD < 0f)//down
                {
                    if ((float)Math.Round(posXadj - elevX, 2) >= 0f)
                    {
                        Move = false;
                    }
                }
                else //up
                {
                    if ((float)Math.Round(posXadj - elevX, 2) <= 0f)
                    {
                        Move = false;
                    }
                }
            }
            //if z etc
            else if (Z_)
            {
                if (zD < 0f)//down
                {
                    if ((float)Math.Round(posZadj - elevZ, 2) >= 0f)
                    {
                        Move = false;
                    }
                }
                else //up
                {
                    if ((float)Math.Round(posZadj - elevZ, 2) <= 0f)
                    {
                        Move = false;
                    }
                }
            }

            if (Move)
            {
                elevatorPlatform.Translate(elevatorVelocity * elevatorspeed * Time.deltaTime, Space.World);
            }
            yield return null;
        }
        if (src.isPlaying)
        {
            src.Stop();
        }
        src.PlayOneShot(clips[2]);
        Move = false;
    }

    public void ExternalInteractFunc(int i)
    {
        switch (i)
        {
            case -3:
                Up();
                break;
            case -2:
                Down();
                break;
            default:
                SpecificLevel(i);
                break;
        }
    }
}
