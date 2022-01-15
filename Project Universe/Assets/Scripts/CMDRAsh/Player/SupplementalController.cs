using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectUniverse.Player;
using ProjectUniverse.Base;
using ProjectUniverse.Serialization.Handler;
using ProjectUniverse.Serialization;
//using UnityEditor;
using ProjectUniverse.Environment.Volumes;
using System;
using UnityEngine.SceneManagement;
using MLAPI;
using MLAPI.NetworkVariable;
using ProjectUniverse.Items.Weapons;
using MLAPI.Messaging;
using ProjectUniverse.Animation.Controllers;

namespace ProjectUniverse.Player.PlayerController
{
    [Serializable]
    public class SupplementalController : NetworkBehaviour
    {
        private Guid guid;
        public string crouchKey;
        public string proneKey;
        public bool crouchToggle;
        public bool crouching;
        public bool prone;
        public bool sprinting = false;
        [SerializeField] private PlayerControls controls;
        [SerializeField] GameObject playerRoot;
        [SerializeField] GameObject fleetBoy;
        [SerializeField] private float crouchHeight;
        [SerializeField] private float proneHeight;
        [SerializeField] private float standHeight;
        [SerializeField] private Light flashLight;
        [SerializeField] private GameObject firstPersonCameraRoot;
        [SerializeField] private GameObject thirdPersonCameraRoot;
        [SerializeField] private GameObject bodyTTTDRoot;
        [SerializeField] private GameObject handHeadEquipment;
        [SerializeField] private GameObject headTrackObject;
        [SerializeField] private GameObject[] uiElementsToHide;
        [SerializeField] private bool cameraLocked = false;
        [SerializeField] private bool cameraFirst = true;
        [SerializeField] private bool shift = false;
        [SerializeField] int CursorCase = 0;
        [SerializeField] private bool toggleCursorLock = false;
        [SerializeField] private int walkSpeed;
        [SerializeField] private int sprintSpeed;
        [SerializeField] private int crouchSpeed;
        [SerializeField] private int proneSpeed;
        //Player stats2
        private NetworkVariableFloat playerNetHealth = new NetworkVariableFloat(100f);
        [SerializeField] private float playerHealth = 100f;//Non-standard. Radiation, suffocation, etc.
        //[SerializeField] private MyLimb head;//0
        //[SerializeField] private MyLimb chest;//1
        [SerializeField] private float headHealth = 45f;
        [SerializeField] private float chestHealth = 225f;
        [SerializeField] private float lArmHealth = 110f;
        [SerializeField] private float rArmHealth = 110f;
        [SerializeField] private float lHandHealth = 25f;
        [SerializeField] private float rHandHealth = 25f;
        [SerializeField] private float lLegHealth = 125f;
        [SerializeField] private float rLegHealth = 125f;
        [SerializeField] private float lFootHealth = 25f;
        [SerializeField] private float rFootHealth = 25f;
        [SerializeField] private float playerHydration = 100f;
        [SerializeField] private float playerHappyStomach = 100f;
        private bool fleetBoyOut = false;
        private float lookClamp;

        private NetworkVariableBool netFlashlightState = new NetworkVariableBool(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.Everyone });

        public float HeadHealth
        {
            get { return headHealth; }
            set { headHealth = value; }
        }
        public float ChestHealth
        {
            get { return chestHealth; }
            set { chestHealth = value; }
        }
        public float LArmHealth
        {
            get { return lArmHealth; }
            set { lArmHealth = value; }
        }
        public float RArmHealth
        {
            get { return rArmHealth; }
            set { rArmHealth = value; }
        }
        public float LHandHealth
        {
            get { return lHandHealth; }
            set { lHandHealth = value; }
        }
        public float RHandHealth
        {
            get { return rHandHealth; }
            set { rHandHealth = value; }
        }
        public float LLegHealth
        {
            get { return lLegHealth; }
            set { lLegHealth = value; }
        }
        public float RLegHealth
        {
            get { return rLegHealth; }
            set { rLegHealth = value; }
        }
        public float LFootHealth
        {
            get { return lFootHealth; }
            set { lFootHealth = value; }
        }
        public float RFootHealth
        {
            get { return rFootHealth; }
            set { rFootHealth = value; }
        }
        public float PlayerHydration
        {
            get { return playerHydration; }
            set { playerHydration = value; }
        }
        public float PlayerHappyStomach
        {
            get { return playerHappyStomach; }
            set { playerHappyStomach = value; }
        }

        public PlayerControls PlayerController
        {
            get { return controls; }
        }

        public float CrouchHeight
        {
            get { return crouchHeight; }
        }
        public float CrouchSpeed
        {
            get { return crouchSpeed; }
        }
        public float ProneHeight
        {
            get { return proneHeight; }
        }
        public float ProneSpeed
        {
            get { return proneSpeed; }
        }
        public float StandHeight
        {
            get { return standHeight; }
        }
        public float WalkSpeed
        {
            get { return walkSpeed; }
        }

        private void OnEnable()
        {
            controls.Player.Crouch.Enable();
            controls.Player.Prone.Enable();
            controls.Player.ShowInventory.Enable();
            controls.Player.Shift.Enable();

            controls.Player.Alt.Enable();
            controls.Player.Sprint.Enable();
            controls.Player.Flashlight.Enable();
        }
        private void OnDisable()
        {
            controls.Player.Crouch.Disable();
            controls.Player.Prone.Disable();
            controls.Player.ShowInventory.Disable();
            //controls.Player.Shift.Disable();

            controls.Player.Alt.Disable();
            controls.Player.Sprint.Disable();
            controls.Player.Flashlight.Disable();
        }

        private void Start()
        {
            NetworkListeners();
            GetComponent<CMF.AdvancedWalkerController>().movementSpeed = walkSpeed;
            controls.Player.Crouch.performed += ctx =>
            {
                //Debug.Log("____");
                if (crouchToggle)
                {
                    if (crouching)
                    {
                        //Debug.Log("STAND");
                        //crouching = false;
                        //prone = false;
                        //GetComponent<CMF.AdvancedWalkerController>().movementSpeed = walkSpeed;
                        GetComponent<PlayerAnimationController>().OnExitCrouch();
                       
                    }
                    else
                    {
                        //Debug.Log("CROUCH");
                        //crouching = true;
                        //prone = false;
                        //GetComponent<CMF.AdvancedWalkerController>().movementSpeed = crouchSpeed;
                        GetComponent<PlayerAnimationController>().OnPlayerCrouch();
                        
                    }
                }
                else
                {
                    Debug.Log("CROUCH HOLD");
                    //crouching = true;
                    //prone = false;
                    //GetComponent<CMF.AdvancedWalkerController>().movementSpeed = crouchSpeed;
                    GetComponent<PlayerAnimationController>().OnPlayerCrouch();
                    
                }
            };

            controls.Player.Crouch.canceled += ctx =>
            {
                if (!crouchToggle)
                {
                    Debug.Log("CROUCH RELEASE");
                    //crouching = false;
                    //prone = false;
                    //GetComponent<CMF.AdvancedWalkerController>().movementSpeed = walkSpeed;
                    GetComponent<PlayerAnimationController>().OnExitCrouch();
                    
                }
            };

            controls.Player.Prone.performed += ctx =>
            {
                if (prone)
                {
                    //crouching = false;
                    //prone = false;
                    //GetComponent<CMF.AdvancedWalkerController>().movementSpeed = walkSpeed;
                    GetComponent<PlayerAnimationController>().OnExitProne();
                    
                }
                else
                {
                    //crouching = false;
                    //prone = true;
                    //GetComponent<CMF.AdvancedWalkerController>().movementSpeed = proneSpeed;
                    GetComponent<PlayerAnimationController>().OnPlayerProne();

                }
            };

            controls.Player.ShowInventory.performed += ctx =>
            {
                ShowHideInventory();
            };

            controls.Player.Shift.performed += ctx =>
            {
                shift = true;
            };
            controls.Player.Shift.canceled += ctx =>
            {
                shift = false;
            };

            controls.Player.Alt.performed += ctx =>
            {
                if (shift)
                {
                    Debug.Log("Switch VeiwPoint");
                    //switch to third person
                    if (cameraFirst)
                    {
                        //switch to 3rd
                        cameraFirst = false;

                        //set transform of thirdperson to firstperson
                        //Calculate up and forward direction;
                        Vector3 _forwardDirection = Vector3.ProjectOnPlane(firstPersonCameraRoot.transform.forward, thirdPersonCameraRoot.transform.up).normalized;
                        Vector3 _upDirection = thirdPersonCameraRoot.transform.up;
                        //Set rotation;
                        thirdPersonCameraRoot.transform.rotation = Quaternion.LookRotation(_forwardDirection, _upDirection);

                        firstPersonCameraRoot.SetActive(false);
                        thirdPersonCameraRoot.SetActive(true);
                        //set directional camera to Third person
                        GetComponent<CMF.AdvancedWalkerController>().cameraTransform = thirdPersonCameraRoot.transform;
                        //enable the Turn To Transform Direction Script
                        bodyTTTDRoot.GetComponent<CMF.TurnTowardTransformDirection>().targetTransform = thirdPersonCameraRoot.transform;
                        handHeadEquipment.GetComponent<PoVRotationTracker>().CamTransformToTrack = thirdPersonCameraRoot.transform;
                        headTrackObject.GetComponent<PoVRotationTracker>().CamTransformToTrack = thirdPersonCameraRoot.transform;
                    }
                    else
                    {
                        //switch to 1st
                        cameraFirst = true;

                        //set transform of firstperson to thirdperson
                        //Calculate up and forward direction;
                        Vector3 _forwardDirection = Vector3.ProjectOnPlane(thirdPersonCameraRoot.transform.forward, firstPersonCameraRoot.transform.up).normalized;
                        Vector3 _upDirection = firstPersonCameraRoot.transform.up;
                        //Set rotation;
                        firstPersonCameraRoot.transform.rotation = Quaternion.LookRotation(_forwardDirection, _upDirection);

                        thirdPersonCameraRoot.SetActive(false);
                        firstPersonCameraRoot.SetActive(true);
                        //set directional camera to First person
                        GetComponent<CMF.AdvancedWalkerController>().cameraTransform = firstPersonCameraRoot.transform;
                        //disable the Turn To Transform Direction Script
                        bodyTTTDRoot.GetComponent<CMF.TurnTowardTransformDirection>().targetTransform = firstPersonCameraRoot.transform;
                        handHeadEquipment.GetComponent<PoVRotationTracker>().CamTransformToTrack = firstPersonCameraRoot.transform;
                        headTrackObject.GetComponent<PoVRotationTracker>().CamTransformToTrack = firstPersonCameraRoot.transform;
                    }
                }
                else
                {
                    
                    if (CameraLocked)
                    {
                        Debug.Log("Free Screen and Lock Cursor");
                        FreeScreenAndLockCursor();
                        //UnlockCursor();
                    }
                    else
                    {
                        Debug.Log("Lock Screen and Free Cursor");
                        LockScreenAndFreeCursor();
                    }
                    cameraLocked = !cameraLocked;
                }
            };

            controls.Player.Sprint.performed += ctx =>
            {
                sprinting = true;
                if (crouching)
                {
                    GetComponent<CMF.AdvancedWalkerController>().movementSpeed += 1;
                }
                else if(prone)
                {
                    GetComponent<CMF.AdvancedWalkerController>().movementSpeed += 1;
                }
                else
                {
                    GetComponent<CMF.AdvancedWalkerController>().movementSpeed = sprintSpeed;
                }
            };

            controls.Player.Sprint.canceled += ctx =>
            {
                sprinting = false;
                if (crouching)
                {
                    GetComponent<CMF.AdvancedWalkerController>().movementSpeed = crouchSpeed;
                }
                else if (prone)
                {
                    GetComponent<CMF.AdvancedWalkerController>().movementSpeed = proneSpeed;
                }
                else
                {
                    GetComponent<CMF.AdvancedWalkerController>().movementSpeed = walkSpeed;
                }
            };

            controls.Player.Flashlight.performed += ctx =>
            {
                FlashLightToggleServerRpc(!flashLight.enabled);
            };

            
        }

        public bool CameraLocked
        {
            get { return cameraLocked; }
        }

        private void Awake()
        {
            controls = new PlayerControls();
        }

        // Update is called once per frame
        void Update()
        {
            playerHealth = playerNetHealth.Value;
            if (cameraFirst)
            {
            }
            else
            {
                //sync 1st person cam with rotation of 3rd person cam
                //1st person turns player to cam
                //firstPersonCameraRoot.transform.localRotation = Quaternion.Euler(thirdPersonCameraRoot.transform.localRotation.eulerAngles);
            }   
        }

        public float GetCameraAngle()
        {
            if (firstPersonCameraRoot.activeInHierarchy)
            {
                return firstPersonCameraRoot.transform.localEulerAngles.x;
            }
            else if(thirdPersonCameraRoot.activeInHierarchy)
            {
                return thirdPersonCameraRoot.transform.localEulerAngles.x;
            }
            else
            {
                return 0;
            }
        }

        public void NetworkListeners()
        {
            flashLight.enabled = false;
            netFlashlightState.Value = false;
            netFlashlightState.OnValueChanged += delegate { flashLight.enabled = netFlashlightState.Value; };
        }

        public void ShowHideInventory()
        {
            if (!fleetBoyOut)
            {
                Debug.Log("Show");
                //disable other controls like LMB, E, Enter, R, etc.
                controls.Player.Crouch.Disable();
                controls.Player.Prone.Disable();
                controls.Player.Alt.Disable();
                controls.Player.Sprint.Disable();
                controls.Player.Flashlight.Disable();

                LockScreenAndFreeCursor();//playerRoot.GetComponent<SupplementalController>().
                fleetBoy.SetActive(true);
                fleetBoy.GetComponent<FleetBoy2000UIController>().Refresh();
                fleetBoyOut = true;
            }
            else
            {
                Debug.Log("Hide");
                controls.Player.Crouch.Enable();
                controls.Player.Prone.Enable();
                controls.Player.Alt.Enable();
                controls.Player.Sprint.Enable();
                controls.Player.Flashlight.Enable();
                FreeScreenAndLockCursor();//playerRoot.GetComponent<SupplementalController>().
                fleetBoy.SetActive(false);
                fleetBoyOut = false;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void InflictPlayerDamageServerRpc(float amount)
        {

            // \/ Naw. Let health be negative. Competative dying.
            //if (playerHealth <= 0)
            //{
            //    playerHealth = 0;
            //}
            playerNetHealth.Value -= amount;
        }
        public float PlayerHealth
        {
            get { return playerHealth; }
            set { playerHealth = value; }
        }

        
        public void SavePlayer()
        {
            SceneDataHelper sdh = new SceneDataHelper(SceneManager.GetActiveScene().name, "Adrian Expanse Sector 1", DateTime.Now.ToString(), "CMDR Ash");
            PlayerData data = new PlayerData(guid, sdh, playerRoot.transform,firstPersonCameraRoot.transform.rotation.eulerAngles,//save FP and TP cam pos
                GetComponent<IPlayer_Inventory>(), GetComponent<PlayerVolumeController>(),this);
            SerializationHandler.SavePlayer("Player_current",data);
            Debug.Log("Saved");
        }

        public void LoadPlayer()
        {
            PlayerData data = SerializationHandler.Load("Player_current");
            if(data != null)
            {
                //Debug.Log("...Injecting");
                guid = data.GetGUID();
                //Debug.Log(data.Position);
                Vector3 dataRot = data.Rotation.eulerAngles;
                playerRoot.transform.position = data.Position;
                playerRoot.transform.rotation = Quaternion.Euler(0, dataRot.y, 0);
                firstPersonCameraRoot.transform.rotation = Quaternion.Euler(dataRot.x, 0, dataRot.z);//save FP and TP cam pos
                playerRoot.transform.localScale = data.Scale;
                object[] prams = 
                    { data.PlayerInventory, data.InventoryWeight };
                GetComponent<IPlayer_Inventory>().OnLoad(prams);
                prams = new object[]{
                data.GetPlayerVolumeController(),
                    };
                GetComponent<PlayerVolumeController>().OnLoad(prams);

                crouchToggle = data.LoadStatsSupplement.crouchToggle;
                crouching = data.LoadStatsSupplement.crouching;
                prone = data.LoadStatsSupplement.prone;
                PlayerHealth = data.LoadStatsSupplement.PlayerHealth;
                HeadHealth = data.LoadStatsSupplement.HeadHealth;
                ChestHealth = data.LoadStatsSupplement.ChestHealth;
                LArmHealth = data.LoadStatsSupplement.LArmHealth;
                RArmHealth = data.LoadStatsSupplement.RArmHealth;
                LHandHealth = data.LoadStatsSupplement.LHandHealth;
                RHandHealth = data.LoadStatsSupplement.RHandHealth;
                LLegHealth = data.LoadStatsSupplement.LLegHealth;
                RLegHealth = data.LoadStatsSupplement.RLegHealth;
                LFootHealth = data.LoadStatsSupplement.LFootHealth;
                RFootHealth = data.LoadStatsSupplement.RFootHealth;
                PlayerHydration = data.LoadStatsSupplement.PlayerHydration;
                PlayerHappyStomach = data.LoadStatsSupplement.PlayerHappyStomach;
            }
            else
            {
                Debug.LogError("Failed to Load");
            }
           
        }

        /// <summary>
        /// Toggle the lock on the cursor
        /// </summary>
        public void LockCursor()
        {
            if (toggleCursorLock == false)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = true;
                CursorCase = 1;
            }
            if (toggleCursorLock == true)
            {
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = false;
                CursorCase = 2;
            }
            toggleCursorLock = !toggleCursorLock;
        }

        public void LockOnlyCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = true;
        }
        /// <summary>
        /// Unlock the cursor
        /// </summary>
        public void UnlockOnlyCursor()
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }

        /// <summary>
        /// Lock the screen and free the cursor
        /// </summary>
        public void LockScreenAndFreeCursor()
        {
            LockCursor();
            cameraLocked = !cameraLocked;
            Cursor.visible = true;
        }
        /// <summary>
        /// Unlock the screen and lock the cursor
        /// </summary>
        public void FreeScreenAndLockCursor()
        {
            LockCursor();
            cameraLocked = !cameraLocked;
            Cursor.visible = false;
        }

        /// <summary>
        /// Hide all UI elements in the center of the screen
        /// </summary>
        public void HideCenterUI()
        {
            foreach(GameObject obj in uiElementsToHide)
            {
                obj.SetActive(false);
            }
        }
        public void ShowCenterUI()
        {
            foreach (GameObject obj in uiElementsToHide)
            {
                obj.SetActive(true);
            }
        }

        [ServerRpc]
        private void FlashLightToggleServerRpc(bool state)
        {
            FlashLightToggleClientRpc(state);
        }

        [ClientRpc]
        private void FlashLightToggleClientRpc(bool state)
        {
            netFlashlightState.Value = state;
        }

        //Kinda primitive, may adjust later.
        //Locks player camera so they can not look up and down in a 360 deg arc.
        private void ClampLookRotationToValue(float value)
        {
            Vector3 eulerRotation = transform.eulerAngles;
            eulerRotation.x = value * Time.deltaTime;//modified 11/17/20 by CMDRAsh. Added Time.deltatime to smooth out looking when fps not constant.
            firstPersonCameraRoot.transform.eulerAngles = eulerRotation;
        }
    }
}