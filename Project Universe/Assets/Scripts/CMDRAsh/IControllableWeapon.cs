using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using MLAPI;

public class IControllableWeapon : MonoBehaviour
{
    [SerializeField]
    private GameObject cannonBase;
    [SerializeField]
    private GameObject cannonTurret;
    [SerializeField]
    private GameObject[] cannonBarrels;
    private bool isControllerActive = false;
    private GameObject player;
    private Transform playerTrans;
    [SerializeField]
    private Camera gunSightCam;

    void Start()
    {
        // foreach (Transform child in transform)
        // {
        //GameObject.FindGameObjectWithTag("ControlCam").SetActive(false);
        // }
        gunSightCam.enabled = false;
    }

    public void GetPlayerCoords()
    {
        playerTrans = player.transform;
    }

    IEnumerator MyCannonControlUpdate()
    {
        //Debug.Log("Recurssive?");
        
        
        //player.transform.position = playerTrans.position;
        //cannonTurret.transform.rotation = Quaternion.AngleAxis(player.transform.rotation.eulerAngles.y,Vector3.up);// = ;
       // player.transform.rotation = playerTrans.rotation;
            
        yield return null;
        /// Loop Control
        /// Stop the current Coroutine, and Start a new one
        /// Do this until the player is not longer 'controlling' the turret
        /// Then reenable the player cam
        StopCoroutine(MyCannonControlUpdate());
        if (Input.GetKeyDown(KeyCode.F))//Press F to exit the controller
        {
            isControllerActive = false;
            GameObject.FindGameObjectWithTag("MainCamera").SetActive(true);

        }
        if (isControllerActive)
        {
            StartCoroutine(MyCannonControlUpdate());
        }
        
    }

    public void ExternalInteractFunc()
    {
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var networkedClient))
        {
            player = networkedClient.PlayerObject.gameObject;
        }
        else player = null;
        isControllerActive = true;

        //Disable the player camera
        GameObject.FindGameObjectWithTag("MainCamera").SetActive(false);
        Debug.Log("Rendering on Gunsight?");
        gunSightCam.enabled = true;
        //GameObject.FindGameObjectWithTag("ControlCam").SetActive(true);
        StartCoroutine(MyCannonControlUpdate());
    }
}
