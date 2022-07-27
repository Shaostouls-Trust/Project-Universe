using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyJointScript : MonoBehaviour
{
    [SerializeField] private GameObject playerObj;
    [SerializeField] private bool lockPos;
    public Vector3 playerPos = new Vector3(7.25f, -1.5f, 0f);

    public bool LockPos
    {
        set { lockPos = value; }
    }

    public GameObject PlayerObj
    {
        get { return playerObj; }
        set { playerObj = value; }
    }

    public void ResetPosition()
    {
        if (lockPos && playerObj != null)
        {
            //unlock y? Should allow free jump/ladder without micromanaging position.
            playerObj.transform.localPosition = playerPos;
        }
    }


    /// <summary>
    /// Set player rotation to the rotation of the Position object
    /// </summary>
    public void SetRotation(Transform trans)
    {
        if (playerObj != null)
        {
            playerObj.transform.localRotation = Quaternion.Euler(0f, playerObj.transform.localRotation.y, 0f);
        }
    }
}
