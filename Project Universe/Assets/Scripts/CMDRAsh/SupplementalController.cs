using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupplementalController : MonoBehaviour
{
    public string crouchKey;
    public string proneKey;
    public bool crouchToggle;
    [SerializeField] bool crouching;
    [SerializeField] bool prone;
    [SerializeField] GameObject playerRoot;
    [SerializeField] float crouchHeight;
    [SerializeField] float proneHeight;
    [SerializeField] float shrinkerSize;
    [SerializeField] float defaultHeight;

    // Update is called once per frame
    void Update()
    {
        //crouch (hold c)
        if (!crouchToggle)
        {
            if (Input.GetKeyDown(crouchKey))
            {
                crouching = true;
                prone = false;
                playerRoot.transform.localScale = new Vector3(playerRoot.transform.localScale.x,
                    crouchHeight,playerRoot.transform.localScale.z);//.GetComponent<CapsuleCollider>()
            }
            if (Input.GetKeyUp(crouchKey))
            {
                crouching = false;
                prone = false;
                playerRoot.transform.localScale = new Vector3(playerRoot.transform.localScale.x,
                    defaultHeight, playerRoot.transform.localScale.z);
            }
        }
        //crouch (press c)
        else
        {
            if (Input.GetKeyDown(crouchKey))
            {
                if (crouching)
                {
                    crouching = false;
                    prone = false;
                    playerRoot.transform.localScale = new Vector3(1.0f, defaultHeight, 1.0f);
                }
                else
                {
                    crouching = true;
                    prone = false;
                    playerRoot.transform.localScale = new Vector3(1.0f, crouchHeight, 1.0f);
                }
            }
        }
        //prone (press z)
        if (Input.GetKeyDown(proneKey))
        {
            if (prone)
            {
                crouching = false;
                prone = false;
                playerRoot.transform.localScale = new Vector3(1.0f,
                defaultHeight, 1.0f);
                playerRoot.GetComponent<CharacterController>().height = defaultHeight;
                playerRoot.GetComponent<CharacterController>().radius = 0.31f;
                //playerRoot.GetComponent<CapsuleCollider>().height = defaultHeight;
                //playerRoot.GetComponent<CapsuleCollider>().radius = 0.31f;
            }
            else
            {
                crouching = false;
                prone = true;
                playerRoot.transform.localScale = new Vector3(1.0f, proneHeight, 1.0f);
                playerRoot.GetComponent<CharacterController>().height = proneHeight;
                playerRoot.GetComponent<CharacterController>().radius = shrinkerSize;
                //playerRoot.GetComponent<CapsuleCollider>().height = proneHeight;
                //playerRoot.GetComponent<CapsuleCollider>().radius = shrinkerSize;
            }
        }

    }
}
