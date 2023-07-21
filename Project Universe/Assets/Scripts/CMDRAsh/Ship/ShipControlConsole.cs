using Unity.Netcode;
using ProjectUniverse.Player.PlayerController;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ProjectUniverse.Ship
{
    public class ShipControlConsole : MonoBehaviour
    {
        private SupplementalController playerSC;
        [SerializeField] private ShipController shipController;
        [SerializeField] private Vector3 seatPosition;
        private bool isRemoteDrone = false;
        [SerializeField] private GameObject shipUI;
        [SerializeField] private Camera shipCamera;
        [SerializeField] private GameObject spotlight;
        private bool lightOn;
        [SerializeField] private GameObject audsrc;
        [SerializeField] private GameObject invPanel;
        [SerializeField] private int connectionRange;
        [SerializeField] private TMP_Text rangeText;
        [SerializeField] private GameObject rangePanel;
        private bool remoteConnected;
        private float timer = 3f;
        private bool inventoryOut;
        private bool showInventory;
        private bool lockscreenandfreecursor = false;

        private bool firing = false;

        // Update is called once per frame
        void Update()
        {
            if(playerSC != null && playerSC.ShipMode)
            {
                if (!lockscreenandfreecursor)
                {
                    shipController.HorizMove = playerSC.RemoteMoveAxis_Horizonal;
                    shipController.VertMove = playerSC.RemoteMoveAxis_Vertical;
                    shipController.HorizLook = playerSC.RemoteLookAxis_Horizonal;
                    shipController.VertLook = playerSC.RemoteLookAxis_Vertical;
                    shipController.Roll = playerSC.RemoteRoll;
                    if (playerSC.RemoteJump == 0)
                    {
                        shipController.LatMove = playerSC.RemoteShift;
                    }
                    else
                    {
                        shipController.LatMove = playerSC.RemoteJump;
                    }
                
                    lightOn = playerSC.RemoteLight;
                
                    if (!isRemoteDrone)
                    {
                        // set player y rotation to the y rotation on the console (Z forward)
                        Vector3 rot = playerSC.transform.localRotation.eulerAngles;
                        playerSC.transform.localRotation = Quaternion.Euler(rot.x, transform.localRotation.eulerAngles.y, rot.z);

                        // set player pos to seat position (using joint)
                    }

                    // Fire
                    if (shipController.Tools != null)
                    {
                        if (playerSC.RemoteFire)
                        {
                            if (shipController.CurrentEnergy > 0f)
                            {
                                firing = true;
                                if(shipController.ToolSound != null && !shipController.ToolSound.isPlaying)
                                {
                                    shipController.ToolSound.Play();
                                }
                                shipController.Tools.gameObject.SendMessage("Use", SendMessageOptions.DontRequireReceiver);
                                shipController.DrawEnergy(2f);
                            }
                            else
                            {
                                if (firing)
                                {
                                    if (shipController.ToolSound != null && shipController.ToolSound.isPlaying)
                                    {
                                        shipController.ToolSound.Stop();
                                    }
                                    firing = false;
                                    shipController.Tools.gameObject.SendMessage("Stop", SendMessageOptions.DontRequireReceiver);
                                }
                            }
                        }
                        else
                        {
                            if (firing)
                            {
                                firing = false;
                                if (shipController.ToolSound != null && shipController.ToolSound.isPlaying)
                                {
                                    shipController.ToolSound.Stop();
                                }
                                shipController.Tools.gameObject.SendMessage("Stop", SendMessageOptions.DontRequireReceiver);
                            }
                        }
                    }
                
                    // Interact
                    if (remoteConnected)
                    {
                        if (Input.GetKey(KeyCode.F))
                        {
                            //Debug.Log("timer: "+ timer);
                            // subtract 1 from count every second
                            if (timer > 0f)
                            {
                                timer -= Time.deltaTime;
                            }
                            else 
                            {
                                UIExternalInteract();
                            }
                        }
                        else
                        {
                            timer = 3f;
                        }
                    }

                    //light
                    if (lightOn)
                    {
                        spotlight.SetActive(true);
                    }
                    else
                    {
                        spotlight.SetActive(false);
                    }
                }
                //inventory
                showInventory = playerSC.RemoteInventory;
                if (invPanel != null)
                {
                    if (showInventory)
                    {
                        if (!inventoryOut)
                        {
                            lockscreenandfreecursor = true;
                            LockScreenAndFreeCursor();
                            invPanel.SetActive(true);
                            invPanel.GetComponent<InventoryUI>().PopulateInventoryScreen();
                            //invPanel.GetComponent<InventoryUI>().RefreshInventoryScreen();
                            inventoryOut = true;
                        }
                    }
                    else
                    {
                        if (inventoryOut)
                        {
                            lockscreenandfreecursor = false;
                            FreeScreenAndLockCursor();
                            invPanel.SetActive(false);
                            inventoryOut = false;
                        }
                    }
                }

                //range
                if (isRemoteDrone && remoteConnected)
                {
                    float dist = Vector3.Distance(playerSC.transform.position, transform.position);
                    //Debug.Log(dist);
                    if (dist >= (connectionRange - 50f))
                    {
                        rangePanel.SetActive(true);
                        float remaining = connectionRange - dist;
                        rangeText.text = Math.Round(remaining, 1) + "m";
                    }
                    else
                    {
                        rangePanel.SetActive(false);
                    }
                    if (dist > connectionRange)
                    {
                        rangePanel.SetActive(false);
                        playerSC.ControlConsole = null;
                        ExitShipControlMode(playerSC);
                        shipUI.SetActive(false);
                        audsrc.SetActive(false);
                        playerSC.ActiveCamera.SetActive(true);
                        shipCamera.gameObject.SetActive(false);
                    }
                }
            }
        }

        public void ExternalInteractFunc()
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var networkedClient))
            {
                playerSC = networkedClient.PlayerObject.GetComponent<SupplementalController>();
                if (!playerSC.ShipMode)
                {
                    playerSC.ControlConsole = this;
                    EnterShipControlMode(playerSC);
                }
                else
                {
                    playerSC.ControlConsole = null;
                    ExitShipControlMode(playerSC);
                }
            }
        }

        public void UIExternalInteract()
        {
            if (playerSC == null)
            {
                if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var networkedClient))
                {
                    playerSC = networkedClient.PlayerObject.GetComponent<SupplementalController>();
                }
            }
            
            
            if (!playerSC.ShipMode)
            {
                isRemoteDrone = true;
                remoteConnected = true;
                Debug.Log("Enter ship mode");
                playerSC.ControlConsole = this;
                EnterShipControlMode(playerSC);
                shipUI.SetActive(true);
                spotlight.SetActive(true);
                audsrc.SetActive(true);
                playerSC.ActiveCamera.SetActive(false);
                shipCamera.gameObject.SetActive(true);
            }
            else
            {
                isRemoteDrone = false;
                remoteConnected = false;
                Debug.Log("Exit ship mode");
                playerSC.ControlConsole = null;
                ExitShipControlMode(playerSC);
                shipUI.SetActive(false);
                spotlight.SetActive(false);
                audsrc.SetActive(false);
                playerSC.ActiveCamera.SetActive(true);
                shipCamera.gameObject.SetActive(false);
            }
            
        }

        ///
        /// Enter a ship.
        /// Close all open UIs, dequip all hand-helds, set player view forward.
        ///
        private void EnterShipControlMode(SupplementalController player)
        {
            timer = 3f;
            if (player == null)
            {
                player = playerSC;
            }
            // disable movement and binds
            player.ShipMode = true;
            // close all open UIs
            if (player.FleetBoyOut)
            {
                player.ShowHideInventory();
            }
            player.HideCenterUI();
            player.HidePlayerUI();
            //set first person camera veiw to forward
            //player.SetCameraAngleHead(true, new Vector3(0f, -15f, 0f));
            //foreach(GameObject posO in shipController.PlayerPositions)
            //{
            //    RigidbodyJointScript rjs = posO.GetComponent<RigidbodyJointScript>();
            //    if (rjs.PlayerObj == player.gameObject)
            //    {
            //        rjs.LockPos = true;
            //        break;
            //    }
            //}
        }
        
        ///
        /// Exit ship.
        /// Restore UIs.
        ///
        private void ExitShipControlMode(SupplementalController player)
        {
            if (player == null)
            {
                player = playerSC;
            }
            player.ShipMode = false;
            player.ShowCenterUI();
            player.ShowPlayerUI();
            //foreach (GameObject posO in shipController.PlayerPositions)
            //{
           //     RigidbodyJointScript rjs = posO.GetComponent<RigidbodyJointScript>();
            //    if (rjs.PlayerObj == player)
            //    {
            //        rjs.LockPos = false;
            //        break;
            //    }
            //}
        }

        /// <summary>
        /// Lock the screen and free the cursor
        /// </summary>
        public void LockScreenAndFreeCursor()
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
        /// <summary>
        /// Unlock the screen and lock the cursor
        /// </summary>
        public void FreeScreenAndLockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}