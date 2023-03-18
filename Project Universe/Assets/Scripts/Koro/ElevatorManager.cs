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
    public void Up()
    {
        if (!Move)
        {
            // this is for sending it up one level
            Debug.Log("received up call");
            Move = true;
            CurLevel += 1;
            if(CurLevel > levels.Length - 1)
            {
                CurLevel = levels.Length - 1;
            }
            LevelDir = levels[CurLevel];
            StartCoroutine(ElevatorMovement());
        }
    }
    public void SpecificLevel(int levelSpecified)
    {
        //this is if you want to move the elevator to specified level like an actual elevator
        // just make sure you add the int in the call function in elevator btn script
        Debug.Log("received call to "+ levelSpecified);
        Move = true;
        LevelDir = levels[levelSpecified];
        CurLevel = levelSpecified;
        StartCoroutine(ElevatorMovement());
    }
    public void Down()
    {
        if (!Move)
        {
            //this is for sending it down one level
            Debug.Log("received down call");
            Move = true;
            CurLevel -= 1;
            if(CurLevel < 0)
            {
                CurLevel = 0;
            }
            LevelDir = levels[CurLevel];
            StartCoroutine(ElevatorMovement());
        }
    }
    public void RequestElevator()
    {
        //this is for requesting the elevator to the main level
        Debug.Log("received Request elevator");
        LevelDir = levels[0];
        Move = true;
    }
    private void Update()
    {
        //i made it in the update function so that you can animate it
        // you can also parent the player to the elevator or change the players position with the elevator so that you dont have to depend on the colliders
        //if (Move)
        //{
            //elevatorPlatform.position = LevelDir.position;
            //Move = false;
        //}
    }

    IEnumerator ElevatorMovement()
    {
        float xD = LevelDir.GetComponent<ElevatorPlatform>().ShaftCenterForLevel.x - (float)Math.Round(elevatorPlatform.transform.position.x, 2);
        float yD = LevelDir.GetComponent<ElevatorPlatform>().ShaftCenterForLevel.y - (float)Math.Round(elevatorPlatform.transform.position.y, 2);
        float zD = LevelDir.GetComponent<ElevatorPlatform>().ShaftCenterForLevel.z - (float)Math.Round(elevatorPlatform.transform.position.z, 2);
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
            float elevY = elevatorPlatform.transform.localPosition.y;//global
            float elevX = elevatorPlatform.transform.position.x;
            float elevZ = elevatorPlatform.transform.position.z;
            float posYadj = LevelDir.GetComponent<ElevatorPlatform>().ShaftCenterForLevel.y; // - elevatorOriginToFloorDistance;
            float posXadj = LevelDir.GetComponent<ElevatorPlatform>().ShaftCenterForLevel.x; //- elevatorOriginToWallInXDist;
            float posZadj = LevelDir.GetComponent<ElevatorPlatform>().ShaftCenterForLevel.z; //- elevatorOriginToWallInZDist;
            //Debug.Log(elevY+" to "+ posYadj);
            //Bug: Over or undershoots are probably causing occasional issues with determining floor arrival.
            if (Y_)
            {
                if(yD < 0f)//down
                {
                    if ((float)Math.Round(posYadj - elevY, 2) >= 0f)//<
                    {
                        //Debug.Log("128");
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
    }

    public void ExternalInteractFunc(int i)
    {
        if(i == -3)
        {
            //move up one level
            Up();
        }
        else if(i == -2)
        {
            //move up down level
            Down();
        }
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
