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
using ProjectUniverse.Items;
using static ProjectUniverse.Items.IEquipable;
using ProjectUniverse.UI;
using ProjectUniverse.Items.Tools;
using ProjectUniverse.Items.Consumable;
using ProjectUniverse.Ship;
using static ProjectUniverse.Items.Consumable.Consumable_Throwable;
using static Consumable_Applyable;

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
        [SerializeField] private GameObject[] uiMasterList;
        [SerializeField] private bool cameraLocked = false;
        [SerializeField] private bool cameraFirst = true;
        [SerializeField] private bool shift = false;
        [SerializeField] int CursorCase = 0;
        [SerializeField] private bool toggleCursorLock = false;
        private float movementSpeed;
        [SerializeField] private int walkSpeed;
        [SerializeField] private int sprintSpeed;
        [SerializeField] private int crouchSpeed;
        [SerializeField] private int proneSpeed;
        [SerializeField] private Vector2 moveAxis;
        private Vector3 localVelocity;
        //Player stats2
        private float playerHealthMax = 100f;
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
        [Space]
        [SerializeField] private GameObject tempRHBone;
        [SerializeField] private GameObject undersuitBone;
        [SerializeField] private GameObject headBone;
        [SerializeField] private GameObject torsoBone;
        [SerializeField] private GameObject rightShoulderBone;
        [SerializeField] private GameObject leftShoulderBone;
        [SerializeField] private GameObject rightArmBone;
        [SerializeField] private GameObject leftArmBone;
        [SerializeField] private GameObject rightForearmBone;
        [SerializeField] private GameObject leftForearmBone;
        [SerializeField] private GameObject rightHandBone;
        [SerializeField] private GameObject leftHandBone;
        [SerializeField] private GameObject waistBone;
        [SerializeField] private GameObject bottomBone;
        [SerializeField] private GameObject rightUpLegBone;
        [SerializeField] private GameObject leftUpLegBone;
        [SerializeField] private GameObject rightLoLegBone;
        [SerializeField] private GameObject leftLoLegBone;
        [SerializeField] private GameObject rightFootBone;
        [SerializeField] private GameObject leftFootBone;
        [Space]
        [Range(0f, 90f)]
        [SerializeField] private float upperVerticalLimit = 70f;//head rotation
        [Range(0f, -90f)]
        [SerializeField] private float lowerVerticalLimit = -70f;//head rotation
        //[SerializeField] private float cameraSpeed = 50f;
        private float lookClamp;
        //[SerializeField] private bool smoothCameraRotation = false;
        [SerializeField] private bool invertHorizontalInput = false;
        [SerializeField] private bool invertVerticalInput = false;
        [SerializeField] private float mouseInputMultiplier = 25f;

        private IEquipable[] equippedWeapons = new IEquipable[3];
        private IEquipable[] equippedTools = new IEquipable[2];
        private IEquipable[] equippedGadgets = new IEquipable[5];
        private IEquipable[] equippedConsumables = new IEquipable[3];//how will this work with having more than one consumable?
        private List<IEquipable> equippedGear = new List<IEquipable>();
        private IEquipable rightHand;
        private bool fleetBoyOut = false;
        private int selectedWeapon = 0;
        private bool canDrawWep = true;
        private bool toolmode = false;//whether the player has guns or tools out
        private int selectedCons = 0;
        [SerializeField] private GunAmmoUI ammoUI;

        private bool shipMode = false;
        //input axes for objects that override player look controls
        private float remoteLookAxis_Horiz = 0f;
        private float remoteLookAxis_Vert = 0f;
        private float remoteMoveAxis_Horiz = 0f;
        private float remoteMoveAxis_Vert = 0f;
        private float remoteJump = 0f;
        private float remoteShift = 0f;
        private float remoteRoll = 0f;
        private bool remoteFire = false;
        private ShipControlConsole controlConsole;
        private Rigidbody rigidbody;
        // position correction and rotation
        private bool grounded;
        private Transform floorTransform;
        private Vector3? floorNormal = null;
        private Vector3 gravityDirection;
        private Vector3 floorOldVelocity;
        private Vector3? floorOldWorldPosition = null;
        private Vector3? floorHitPosition = null;
        private Vector3? floorOldHitPosition;
        private Vector3 playerLocalOldPosition;
        private Rigidbody floorMasterRB;
        private Transform floorMasterTransform;
        private Vector3 shipLastRotationAxis;
        private Vector3 shipLastRotationAngles;
        /// NEED WAY TO TRACK MULTIPLE TRANFORM DIRECTIONS
        private Transform gravityTransform;

        //ladder movement
        private bool onLadder = false;
        private bool onLadderMoving = false;
        private bool onLadderEnd = false;
        private bool coroutineRunning = false;
        private bool reverseLadderDir;
        private Vector3 ladderforward;

        //Hi-Step up
        private bool hiStepReady = false;
        private float hiStepAmount = 0f;

        private bool jump;
        private Vector2 lookInput;
        private bool remoteLight;
        private bool remoteInventory;

        private NetworkVariableBool netFlashlightState = new NetworkVariableBool(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.Everyone });
        private NetworkVariableInt netSelectedWeapon = new NetworkVariableInt(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.Everyone }, 0);
        public CMF.AdvancedWalkerController awc;
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

        public float PlayerHealthMax
        {
            get { return playerHealthMax; }
            set { playerHealthMax = value; }
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

        public float SprintSpeed
        {
            get { return sprintSpeed; }
        }

        public float MovementSpeed
        {
            get { return movementSpeed; }
        }
        
        public Vector3 LocalVelocity
        {
            get { return localVelocity; }
            set { localVelocity = value; }
        }
        public bool FleetBoyOut
        {
            get { return fleetBoyOut; }
            set { fleetBoyOut = value; }
        }
        public bool ShipMode
        {
            get { return shipMode; }
            set { shipMode = value; }
        }
        public float RemoteLookAxis_Horizonal
        {
            get { return remoteLookAxis_Horiz; }
            set { 
                if (ShipMode)
                {
                    remoteLookAxis_Horiz = value;
                } 
            }
        }
        public float RemoteLookAxis_Vertical
        {
            get { return remoteLookAxis_Vert; }
            set {
                if (ShipMode)
                {
                    remoteLookAxis_Vert = value;
                }
            }
        }
        public float RemoteMoveAxis_Horizonal
        {
            get { return remoteMoveAxis_Horiz; }
            set
            {
                if (ShipMode)
                {
                    remoteMoveAxis_Horiz = value;
                }
            }
        }
        public float RemoteMoveAxis_Vertical
        {
            get { return remoteMoveAxis_Vert; }
            set
            {
                if (ShipMode)
                {
                    remoteMoveAxis_Vert = value;
                }
            }
        }
        public float RemoteJump
        {
            get { return remoteJump; }
            set
            {
                if (ShipMode)
                {
                    remoteJump = value;
                }
            }
        }
        public float RemoteShift
        {
            get { return remoteShift; }
            set
            {
                if (ShipMode)
                {
                    remoteShift = value;
                }
            }
        }
        public bool RemoteFire
        {
            get { return remoteFire; }
            set
            {
                if (ShipMode)
                {
                    remoteFire = value;
                }
            }
        }
        public float RemoteRoll
        {
            get { return remoteRoll; }
            set
            {
                if (ShipMode)
                {
                    remoteRoll = value;
                }
            }
        }

        public bool RemoteLight
        {
            get { return remoteLight; }
            set { remoteLight = value; }
        }

        public bool RemoteInventory
        {
            get { return remoteInventory; }
            set { remoteInventory = value; }
        }

        public Rigidbody PlayerRigidbody
        {
            get { return rigidbody; }
        }

        public ShipControlConsole ControlConsole
        {
            get { return controlConsole; }
            set { controlConsole = value; }
        }

        public Vector3 GravityDirection
        {
            get { return gravityDirection; }
            set { gravityDirection = value; }
        }

        public Vector3? FloorOldWorldPosition
        {
            get { return floorOldWorldPosition; }
            set { floorOldWorldPosition = value; }
        }

        public Vector3 FloorOldVelocity
        {
            get { return floorOldVelocity; }
            set { floorOldVelocity = value; }
        }

        public Rigidbody FloorMasterRB
        {
            get { return floorMasterRB; }
            set { floorMasterRB = value; }
        }

        public Transform FloorMasterTransform
        {
            get { return floorMasterTransform; }
            set { floorMasterTransform = value; }
        }

        public Vector3 ShipLastRotationAxis
        {
            get { return shipLastRotationAxis; }
            set { shipLastRotationAxis = value; }
        }

        public Vector3 ShipLastRotationAngles
        {
            get { return shipLastRotationAngles; }
            set { shipLastRotationAngles = value; }
        }
        public Transform GravityTransform
        {
            get { return gravityTransform; }
            set { gravityTransform = value; }
        }
        
        public bool OnLadder
        {
            get { return onLadder; }
            set { onLadder = value; }
        }

        public bool HiStepReady
        {
            get { return hiStepReady; }
            set { hiStepReady = value; }
        }
        public float HiStepAmount
        {
            get { return hiStepAmount; }
            set { hiStepAmount = value; }
        }

        public bool Jump
        {
            get { return jump; }
            set { jump = value; }
        }
        
        public Vector3 MoveAxis
        {
            get { return moveAxis; }
            set { moveAxis = value; }
        }

        public Vector2 LookInput
        {
            get { return lookInput; }
            set { lookInput = value; }
        }

        public GameObject ActiveCamera
        {
            get { 
                if(cameraFirst){
                    return firstPersonCameraRoot;//.GetComponentInChildren<Camera>(false);
                }
                else
                {
                    return thirdPersonCameraRoot;//.GetComponentInChildren<Camera>(false);
                }
            }
        }
        private void OnEnable()
        {
            controls.Player.Crouch.Enable();
            controls.Player.Prone.Enable();
            controls.Player.ShowInventory.Enable();
            controls.Player.Shift.Enable();

            controls.Player.Alt.Enable();
            controls.Player.Flashlight.Enable();

            controls.Player.Fire.Enable();
            controls.Player.Reload.Enable();
            controls.Player.Num1.Enable();
            controls.Player.Num2.Enable();
            controls.Player.Num3.Enable();
            controls.Player.Num4.Enable();
            controls.Player.ScrollWheel.Enable();

            controls.Player.Look.Enable();
            controls.Player.Move.Enable();
            controls.Player.Jump.Enable();

            controls.Player.LeanLeft.Enable();
            controls.Player.LeanRight.Enable();
        }
        private void OnDisable()
        {
            controls.Player.Crouch.Disable();
            controls.Player.Prone.Disable();
            controls.Player.ShowInventory.Disable();
            //controls.Player.Shift.Disable();

            controls.Player.Alt.Disable();
            controls.Player.Flashlight.Disable();

            controls.Player.Fire.Disable();
            controls.Player.Reload.Disable();
            controls.Player.Num1.Disable();
            controls.Player.Num2.Disable();
            controls.Player.Num3.Disable();
            controls.Player.Num4.Disable();
            controls.Player.ScrollWheel.Disable();

            controls.Player.Look.Disable();
            controls.Player.Move.Disable();
            controls.Player.Jump.Disable();

            controls.Player.LeanLeft.Disable();
            controls.Player.LeanRight.Disable();
        }

        private void Start()
        {
            NetworkListeners();
            //GetComponent<CMF.AdvancedWalkerController>().movementSpeed = walkSpeed;
            movementSpeed = WalkSpeed;
            //Message whatever is in the player's right hand to fire, or perform it's FIRE action
            controls.Player.Fire.performed += ctx =>
            {
                if (!ShipMode)
                {
                    if (!FleetBoyOut)
                    {
                        if (rightHand != null)//rightHandBone?
                        {
                            //get the active gun
                            //rightHand.SendMessage("Use", SendMessageOptions.DontRequireReceiver);
                        }
                        if (tempRHBone != null)
                        {
                            for (int a = 0; a < tempRHBone.transform.childCount; a++)
                            {
                                if (tempRHBone.transform.GetChild(a).gameObject.activeInHierarchy)
                                {
                                    tempRHBone.transform.GetChild(a).gameObject.SendMessage("Use", SendMessageOptions.DontRequireReceiver);
                                }
                            }

                        }
                    }
                }
                else // Ship LMB
                {
                    RemoteFire = true;
                }
            };
            controls.Player.Fire.canceled += ctx =>
            {
                if (!ShipMode)
                {
                    if (!FleetBoyOut)
                    {
                        if (rightHand != null)//rightHandBone?
                        {
                        }
                        if (tempRHBone != null)
                        {
                            for (int a = 0; a < tempRHBone.transform.childCount; a++)
                            {
                                if (tempRHBone.transform.GetChild(a).gameObject.activeInHierarchy)
                                {
                                    tempRHBone.transform.GetChild(a).gameObject.SendMessage("Stop", SendMessageOptions.DontRequireReceiver);
                                }
                            }

                        }
                    }
                }
                else
                {
                    RemoteFire = false;
                }
            };
            controls.Player.Reload.performed += ctx =>
            {
                if (!ShipMode)
                {
                    if (!FleetBoyOut)
                    {
                        if (rightHand != null)//rightHandBone?
                        {
                        }
                        if (tempRHBone != null)
                        {
                            for (int a = 0; a < tempRHBone.transform.childCount; a++)
                            {
                                if (tempRHBone.transform.GetChild(a).gameObject.activeInHierarchy)
                                {
                                    tempRHBone.transform.GetChild(a).gameObject.SendMessage("Reload", SendMessageOptions.DontRequireReceiver);
                                }
                            }

                        }
                    }
                }
            };
            controls.Player.Num1.performed += ctx =>
            {
                if (!ShipMode)
                {
                    if (!FleetBoyOut)
                    {
                        Debug.Log("qs throwables");
                        EquipFromQuickSelect(1);
                    }
                }
            };
            controls.Player.Num2.performed += ctx =>
            {
                if (!ShipMode)
                {
                    if (!FleetBoyOut)
                    {
                        Debug.Log("qs gadgets");
                        EquipFromQuickSelect(2);
                    }
                }
            };
            controls.Player.Num3.performed += ctx =>
            {
                if (!ShipMode)
                {
                    //Change firemode of equipped gun
                    if (!FleetBoyOut)
                    {
                        if (rightHand != null)//rightHandBone?
                        {
                        }
                        if (tempRHBone != null)
                        {
                            for (int a = 0; a < tempRHBone.transform.childCount; a++)
                            {
                                if (tempRHBone.transform.GetChild(a).gameObject.activeInHierarchy)
                                {
                                    Debug.Log("Mode Switch");
                                    tempRHBone.transform.GetChild(a).gameObject.SendMessage("ToogleMode", SendMessageOptions.DontRequireReceiver);
                                }
                            }

                        }
                    }
                }
            };
            controls.Player.Num4.performed += ctx =>
            {
                if (!ShipMode)
                {
                    if (!FleetBoyOut)
                    {
                        Debug.Log("qs tools");
                        EquipFromQuickSelect(3);
                    }
                }
            };

            controls.Player.ScrollWheel.performed += ctx =>
            {
                if (!ShipMode)
                {
                    if (canDrawWep && !FleetBoyOut)
                    {
                        canDrawWep = false;
                        if (IsLocalPlayer)
                        {
                            //Vector2 axisdelt = ctx.ReadValue<Vector2>();
                            //if (axisdelt.y < 0f)//down
                            //{
                            if (selectedWeapon == 0)
                            {
                                netSelectedWeapon.Value = 1;
                                selectedWeapon = netSelectedWeapon.Value;
                            }
                            //}
                            //else if (axisdelt.y > 0f)
                            //{
                            else if (selectedWeapon == 1)
                            {
                                netSelectedWeapon.Value = 0;
                                selectedWeapon = netSelectedWeapon.Value;
                            }
                            //}
                            SelectWeaponServerRpc();

                        }
                    }
                }
                else // change target
                {

                }
            };

            /*
            controls.Player.Crouch.performed += ctx =>
            {
                if (!ShipMode)
                {
                    if (!FleetBoyOut)
                    {
                        CapsuleCollider cap = GetComponent<CapsuleCollider>();
                        //Debug.Log("____");
                        if (crouchToggle)
                        {
                            if (crouching)
                            {
                                //Debug.Log("STAND");
                                //crouching = false;
                                //prone = false;
                                movementSpeed = WalkSpeed;
                                GetComponent<PlayerAnimationController>().OnExitCrouch();
                                cap.height = StandHeight;
                                cap.center = new Vector3(0f, 0.89f, 0f);
                                cap.radius = 0.3f;

                            }
                            else
                            {
                                //Debug.Log("CROUCH");
                                //crouching = true;
                                //prone = false;
                                movementSpeed = CrouchSpeed;
                                GetComponent<PlayerAnimationController>().OnPlayerCrouch();
                                cap.height = CrouchHeight;
                                cap.center = new Vector3(0f, 0.5f, 0f);
                                cap.radius = 0.3f;
                            }
                        }
                        else
                        {
                            Debug.Log("CROUCH HOLD");
                            //crouching = true;
                            //prone = false;
                            movementSpeed = CrouchSpeed;
                            GetComponent<PlayerAnimationController>().OnPlayerCrouch();
                            cap.height = CrouchHeight;
                            cap.center = new Vector3(0f, 0.5f, 0f);
                            cap.radius = 0.3f;
                        }
                    }
                }
            };

            controls.Player.Crouch.canceled += ctx =>
            {
                if (!ShipMode)
                {
                    CapsuleCollider cap = GetComponent<CapsuleCollider>();
                    if (!crouchToggle)
                    {
                        Debug.Log("CROUCH RELEASE");
                        //crouching = false;
                        //prone = false;
                        movementSpeed = WalkSpeed;
                        GetComponent<PlayerAnimationController>().OnExitCrouch();
                        cap.height = StandHeight;
                        cap.center = new Vector3(0f, 0.89f, 0f);
                        cap.radius = 0.3f;
                    }
                }
            };

            controls.Player.Prone.performed += ctx =>
            {
                if (!ShipMode)
                {
                    CapsuleCollider cap = GetComponent<CapsuleCollider>();
                    if (!FleetBoyOut)
                    {
                        if (prone)
                        {
                            //crouching = false;
                            //prone = false;
                            movementSpeed = WalkSpeed;
                            GetComponent<PlayerAnimationController>().OnExitProne();
                            cap.height = StandHeight;
                            cap.center = new Vector3(0f, 0.89f, 0f);
                            cap.radius = 0.3f;
                        }
                        else
                        {
                            //crouching = false;
                            //prone = true;
                            movementSpeed = ProneSpeed;
                            GetComponent<PlayerAnimationController>().OnPlayerProne();
                            cap.height = ProneHeight;
                            cap.center = new Vector3(0f, 0.25f, 0f);
                            cap.radius = 0.225f;
                        }
                    }
                }
            };
            */
            controls.Player.ShowInventory.performed += ctx =>
            {
                if (!ShipMode)
                {
                    ShowHideInventory();
                }
                else
                {
                    if (RemoteInventory)
                    {
                        RemoteInventory = false;
                    }
                    else
                    {
                        RemoteInventory = true;
                    }
                }
            };

            controls.Player.Shift.performed += ctx =>
            {
                if (!ShipMode)
                {
                    shift = true;
                    if (crouching)
                    {
                        movementSpeed += 1f;
                    }
                    else if (prone)
                    {
                        movementSpeed += 1f;
                    }
                    else
                    {
                        sprinting = true;
                        movementSpeed = sprintSpeed;
                    }
                }
                else
                {
                    RemoteShift = -1f;
                }
            };
            controls.Player.Shift.canceled += ctx =>
            {
                if (!ShipMode)
                {
                    shift = false;
                    if (crouching)
                    {
                        movementSpeed = CrouchSpeed;
                    }
                    else if (prone)
                    {
                        movementSpeed = ProneSpeed;
                    }
                    else
                    {
                        sprinting = false;
                        movementSpeed = WalkSpeed;
                    }
                }
                else
                {
                    RemoteShift = 0f;
                }
            };

            controls.Player.Alt.performed += ctx =>
            {
                if (!ShipMode)
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
                            //GetComponent<CMF.AdvancedWalkerController>().cameraTransform = thirdPersonCameraRoot.transform;
                            //enable the Turn To Transform Direction Script
                            bodyTTTDRoot.GetComponent<CMF.TurnTowardTransformDirection>().targetTransform = thirdPersonCameraRoot.transform;
                            handHeadEquipment.GetComponent<PoVRotationTracker>().CamTransformToTrack = thirdPersonCameraRoot.transform;
                            headTrackObject.GetComponent<PoVRotationTracker>().CamTransformToTrack = thirdPersonCameraRoot.transform;
                        }
                        // switch to 1st
                        else
                        {
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
                            //GetComponent<CMF.AdvancedWalkerController>().cameraTransform = firstPersonCameraRoot.transform;
                            //disable the Turn To Transform Direction Script
                            bodyTTTDRoot.GetComponent<CMF.TurnTowardTransformDirection>().targetTransform = firstPersonCameraRoot.transform;
                            handHeadEquipment.GetComponent<PoVRotationTracker>().CamTransformToTrack = firstPersonCameraRoot.transform;
                            headTrackObject.GetComponent<PoVRotationTracker>().CamTransformToTrack = firstPersonCameraRoot.transform;
                        }
                    }
                    else
                    {
                        //Throw consumable
                        GameObject oldEq = null;
                        if (RightHandEquipped != null)
                        {
                            oldEq = RightHandEquipped.gameObject;
                            DequipItem(oldEq);
                        }

                        if (EquippedConsumables[selectedCons] != null)
                        {

                            EquipItem(EquippedConsumables[selectedCons].gameObject, Slot.RightHand);
                            if (tempRHBone != null)
                            {
                                for (int a = 0; a < tempRHBone.transform.childCount; a++)
                                {
                                    if (tempRHBone.transform.GetChild(a).gameObject.activeInHierarchy)
                                    {
                                        tempRHBone.transform.GetChild(a).gameObject.SendMessage("Use", SendMessageOptions.DontRequireReceiver);
                                    }
                                }
                            }
                            DequipItem(EquippedConsumables[selectedCons].gameObject);
                        }

                        if (oldEq != null)
                        {
                            EquipItem(oldEq, Slot.RightHand);
                        } 
                    }
                }
                else // freelook during control of ship
                {

                }
            };

            controls.Player.Flashlight.performed += ctx =>
            {
                if (!ShipMode && !FleetBoyOut)
                {
                    FlashLightToggleServerRpc(!flashLight.enabled);
                }
                else
                {
                    if (RemoteLight)
                    {
                        RemoteLight = false;
                    }
                    else
                    {
                        RemoteLight = true;
                    }
                }
            };

            // Character Controller
            controls.Player.Move.performed += ctx =>
            {
                Vector2 input = ctx.ReadValue<Vector2>();
                if (!ShipMode)
                {
                    if (onLadder)
                    {
                        MoveAxis = Vector2.zero;
                        onLadderMoving = true;
                        if (input.y > 0f)//<
                        {
                            reverseLadderDir = true;
                        }
                        else
                        {
                            reverseLadderDir = false;
                        }
                        if (!coroutineRunning)
                        {
                            coroutineRunning = true;
                            StartCoroutine(OnLadderMovement());
                        }
                    }
                    else
                    {
                        MoveAxis = input;
                    }
                }
                else
                {
                    RemoteMoveAxis_Horizonal = input.x;
                    RemoteMoveAxis_Vertical = input.y;
                }
               
            };
            controls.Player.Move.canceled += ctx =>
            {
                if (!ShipMode)
                {
                    MoveAxis = Vector2.zero;
                }
                else
                {
                    RemoteMoveAxis_Horizonal = 0f;
                    RemoteMoveAxis_Vertical = 0f;
                }
            };

            controls.Player.Look.performed += ctx =>
            {
                Vector2 inputlook = mouseInputMultiplier * Time.deltaTime * ctx.ReadValue<Vector2>();
                inputlook.y *= -1f;

                // * (1f / Time.unscaledDeltaTime);
                if (invertHorizontalInput)
                {
                    inputlook.x *= -1f;
                }
                if (invertVerticalInput)
                {
                    inputlook.y *= -1f;
                }
                //Debug.Log("L: " + inputlook.x + " " + inputlook.y);
                if (!shipMode) { 
                    if (!cameraLocked)
                    {
                        //Debug.Log(inputlook.x + " : " + inputlook.y);
                        //RotateCamera(inputlook.y, inputlook.x);
                        lookInput = inputlook;
                    }
                    else
                    {
                        lookInput = Vector2.zero;
                    }
                }
                else
                {
                    //Debug.Log("Look: " + inputlook.x + " " + inputlook.y);
                    lookInput = Vector2.zero;
                    RemoteLookAxis_Horizonal = inputlook.x;
                    RemoteLookAxis_Vertical = inputlook.y;
                }
            };
            controls.Player.Look.canceled += ctx =>
            {
                lookInput = Vector2.zero;
                RemoteLookAxis_Horizonal = 0f;
                RemoteLookAxis_Vertical = 0f;
            };

            controls.Player.Jump.performed += ctx =>
            {
                if (!ShipMode)
                {
                    //Debug.Log(grounded);
                    //if grounded
                    if (grounded || awc.IsGrounded()) 
                    {
                        //if step-up trigger
                        //player 'steps up' onto surface (elevate player to level of surface)
                        if (HiStepReady)
                        {
                            rigidbody.MovePosition(new Vector3(0f, HiStepAmount, 0f) + transform.position);
                        }
                        else
                        {
                            Jump = true;
                            //Debug.Log("jump");
                            Vector3 force = new Vector3(0f, 50000f, 0f);
                            force = transform.TransformVector(force);
                            rigidbody.AddForce(force);
                        }
                        //if ledgegrab trigger
                        //player hauls up into surface (elevate player to level of surface)
                    }
                }
                else
                {
                    RemoteJump = 1f;
                }
            };
            controls.Player.Jump.canceled += ctx =>
            {
                Jump = false;
                RemoteJump = 0f;
            };

            controls.Player.LeanLeft.performed += ctx =>
            {
                //ksed
                if (!shipMode)
                {
                    //lean
                }
                else
                {
                    RemoteRoll = 1f;
                }
            };
            controls.Player.LeanLeft.canceled += ctx =>{
                if (!shipMode)
                {
                    //lean
                }
                else
                {
                    RemoteRoll = 0f;
                }
            };

            controls.Player.LeanRight.performed += ctx =>
            {
                //uikj
                if (!shipMode)
                {
                   
                }
                else
                {
                    RemoteRoll = -1f;
                }
            };
            controls.Player.LeanRight.canceled += ctx =>
            {
                if (!shipMode)
                {

                }
                else
                {
                    RemoteRoll = 0f;
                }
            };
        }

        public bool CameraLocked
        {
            get { return cameraLocked; }
        }

        public void SetCameraAngleHead(bool firstPerson, Vector3 newAngle)
        {
            //set head rotation to newAngle
            if (firstPerson)
            {
                Debug.Log("Set angle");
                //set camera not head
                headTrackObject.transform.localRotation = Quaternion.Euler(newAngle);
            }
        }

        private void Awake()
        {
            controls = new PlayerControls();
            rigidbody = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void Update()
        {
            playerHealth = playerNetHealth.Value;
        }

        public void EndOfLadder(Vector3 forward)
        {
            onLadderEnd = true;
            ladderforward = forward;
        }
        public void EndOfLadder()
        {
            OnLadder = false;
            onLadderMoving = false;
            onLadderEnd = false;
            coroutineRunning = false;
        }

        public IEnumerator OnLadderMovement()
        {
            Debug.Log("Start");
            float timer = 30;
            //turn off player gravity (store what the world gravity was. Ensure that changes in gravity don't reset this)
            while (OnLadder)
            {
                if (onLadderEnd)
                {
                    //move up
                    rigidbody.MovePosition(transform.position + transform.TransformDirection(new Vector3(0f, 3f, 0f)) * Time.deltaTime);
                    //transform.localPosition += transform.TransformDirection(new Vector3(0f, 3f, 0f)) * Time.deltaTime;//ladderforward * 3f
                    //awc.SetMomentum(new Vector3(0f, 0.6f, 0f) + (ladderforward * 3f));//don't let the player move down
                    timer--;
                    if (timer <= 0f)
                    {
                        //move forward 1m
                        rigidbody.MovePosition(transform.position + transform.TransformDirection(ladderforward));
                        Debug.Log("End");
                        OnLadder = false;
                        onLadderMoving = false;
                        onLadderEnd = false;
                        coroutineRunning = false;
                    }
                }
                else if (onLadderMoving)
                {
                    float angle = gameObject.GetComponent<SupplementalController>().GetCameraAngle() - 180f;
                    if (!reverseLadderDir)
                    {
                        if (angle >= 0f)
                        {
                            //localPosition
                            rigidbody.MovePosition(transform.position + transform.TransformDirection(new Vector3(0f, 3f, 0f)) * Time.deltaTime);
                            //transform.localPosition += transform.TransformDirection(new Vector3(0f, 3f, 0f)) * Time.deltaTime;
                            //awc.SetMomentum(new Vector3(0f, 3f, 0f));
                        }
                        else
                        {
                            //awc.SetMomentum(new Vector3(0f, -3f, 0f));
                            rigidbody.MovePosition(transform.position + transform.TransformDirection(new Vector3(0f, -3f, 0f)) * Time.deltaTime);
                            //transform.localPosition += transform.TransformDirection(new Vector3(0f, -3f, 0f)) * Time.deltaTime;
                        }
                    }
                    else
                    {
                        if (angle >= 0f)
                        {
                            //awc.SetMomentum(new Vector3(0f, -3f, 0f));
                            rigidbody.MovePosition(transform.position + transform.TransformDirection(new Vector3(0f, -3f, 0f)) * Time.deltaTime);                            
                            //transform.localPosition += transform.TransformDirection(new Vector3(0f, -3f, 0f)) * Time.deltaTime;
                        }
                        else
                        {
                            //awc.SetMomentum(new Vector3(0f, 3f, 0f));
                            //transform.localPosition += transform.TransformDirection(new Vector3(0f, 3f, 0f)) * Time.deltaTime;
                            rigidbody.MovePosition(transform.position + transform.TransformDirection(new Vector3(0f, 3f, 0f)) * Time.deltaTime);
                        }
                    }

                }
                else
                {
                    //awc.SetMomentum(new Vector3(0f, 0.6f, 0f));//the player constantly moves down at abt this speed on ladder
                }
                yield return null;
            }
        }

        
        private void FixedUpdate()
        {
            //raycast to visible render planes
            //get camera position
            GameObject camGo;
            if (cameraFirst)
            {
                camGo = firstPersonCameraRoot.GetComponentInChildren<Camera>().gameObject;
            }
            else
            {
                camGo = thirdPersonCameraRoot.GetComponentInChildren<Camera>().gameObject;
            }
            //Vector3 forward = camGo.transform.TransformDirection(0f, 0f, 1f);

            //RaycastHit[] hits;
            //hits = Physics.RaycastAll(camGo.transform.position, forward, 35f, 7, QueryTriggerInteraction.Ignore);
            //for(int i = 0; i < hits.Length; i++)
            //{

            //}

            /*
            grounded = GroundCheck();
            if (!grounded)
            {
                Debug.Log("Ground Fault");
            }
            MovePlayer(MoveAxis.x, MoveAxis.y);
            //rotate the players according to ship movement
            RotatePlayerAround(ShipLastRotationAngles);
            Gravity();

            if (floorTransform != null)
            {
                FloorOldWorldPosition = floorTransform.position;
            }
            else
            {
                Debug.Log("FOWP EXTRAP");
                FloorOldWorldPosition += floorOldVelocity;
            }
            //else floorOldWorldPosition does not change
            
            //only update relative velocity if player is touching the ground.
            if(floorMasterRB != null && grounded)
            {
                FloorOldVelocity = floorMasterRB.velocity;
            }
            else
            {
                //approximate velocity of the floor in the last frame
                if (floorOldWorldPosition != null && floorTransform != null)
                {
                    FloorOldVelocity = ((Vector3)FloorOldWorldPosition - floorTransform.position) / Time.deltaTime;
                }
                else
                {
                    Debug.Log("FOV Fault");
                }
            }
            floorOldHitPosition = floorHitPosition;
            
            playerLocalOldPosition = transform.localPosition;
            */

        }

        public IEquipable[] EquippedWeapons
        {
            get { return equippedWeapons; }
            set { equippedWeapons = value; }
        }
        public IEquipable[] EquippedTools
        {
            get { return equippedTools; }
            set { equippedTools = value; }
        }
        public List<IEquipable> EquippedGear
        {
            get { return equippedGear; }
            set { equippedGear = value; }
        }
        public IEquipable[] EquippedGadgets
        {
            get { return equippedGadgets; }
            set { equippedGadgets = value; }
        }
        public IEquipable[] EquippedConsumables
        {
            get { return equippedConsumables; }
            set { equippedConsumables = value; }
        }

        public IEquipable RightHandEquipped
        {
            get { return rightHand; }
            set { rightHand = value; }
        }

        public Rigidbody RB
        {
            get { return rigidbody; }
        }

        public void RotateCamera(float x, float y)
        {
            Vector3 rotation = new Vector3(x, y, 0f);

            /// use scaled time to try and get control over the camera during lag spikes. (Smooth cam rotation)
            rotation = Vector3.Lerp(new Vector3(0f, 0f, 0f), rotation, (1f / Time.captureFramerate) * 10f);

            float moux = rotation.x;
            lookClamp += rotation.x;

            if (lookClamp > upperVerticalLimit)
            {
                lookClamp = upperVerticalLimit;
                moux = upperVerticalLimit;
                ClampLookRotationToValue(upperVerticalLimit + 180f);
            }
            else if (lookClamp < lowerVerticalLimit)
            {
                lookClamp = lowerVerticalLimit;
                moux = lowerVerticalLimit;
                ClampLookRotationToValue(lowerVerticalLimit + 180f);
            }

            transform.Rotate(Vector3.up * rotation.y);
            if (cameraFirst)
            {
                //firstPersonCameraRoot.transform.Rotate(moux * Vector3.right);//up/down
                headTrackObject.transform.Rotate(moux * Vector3.up);
            }
            else
            {
                thirdPersonCameraRoot.transform.Rotate(moux * Vector3.right);
            }
        }

        private void ClampLookRotationToValue(float value)
        {
            Vector3 eulerRotation = transform.eulerAngles;
            eulerRotation.x = value * Time.deltaTime;
            firstPersonCameraRoot.transform.eulerAngles = eulerRotation;
        }

        /// <summary>
        /// Apply gravity to the player in it's direction of acceleration.
        /// </summary>
        public void Gravity()
        {
            Vector3 g = GravityDirection;
            //transform world direction to local
            if (GravityTransform != null)
            {
                g = gravityTransform.TransformDirection(g * Time.deltaTime * 50f);//60
            }
            else
            {
                g *= Time.deltaTime * 60f;
            }
            if (!grounded && !OnLadder)
            {
                //add gravity to velocity
                rigidbody.velocity += g;
            }

            //If gravity is along y
            if (GravityDirection.y != 0f)
            {
                //delta angle in x and z (world space?)
                //float angleZ = ((float)Math.Acos(x) * Mathf.Rad2Deg) - 90f;
                //float angleX = (float)Math.Asin(z) * Mathf.Rad2Deg;//*-1f?
                
                //Quadrant correction
                //if(GravityTransform.rotation.eulerAngles.z >= 90f && GravityTransform.rotation.eulerAngles.z < 180f)
                //{
                    //angleZ += 90f;
                //}
                //else if(GravityTransform.rotation.eulerAngles.z >= 180f && GravityTransform.rotation.eulerAngles.z < 270f)
                //{
                    //angleZ += 180f;
                //}else if (GravityTransform.rotation.eulerAngles.z >= 270f && GravityTransform.rotation.eulerAngles.z < 360f)
                //{
                    //angleZ += 270f;
                //}
                //wobble check
                //if (GravityTransform != null)
                //{
                    //if ((GravityTransform.rotation.eulerAngles.z - transform.rotation.eulerAngles.z) != 0f)//angleZ
                    //{
                        //float num = GravityTransform.rotation.eulerAngles.z - transform.rotation.eulerAngles.z;// angleZ;
                        //Debug.Log(num);
                        //transform.Rotate(new Vector3(0f, 0f, (float)Math.Round(num, 4)), Space.World);
                    //}
                //}

                ///screw this and just set the local rotations to 0f. 
                ///This causes all sorts of issues though and does not work completely. Also, the player struggles to move.
                ///
                if(transform.localRotation.eulerAngles.z != 0f)
                {
                    transform.Rotate(new Vector3(0f, 0f, -transform.localRotation.eulerAngles.z), Space.Self);
                }
                if(transform.localRotation.eulerAngles.x != 0f)
                {
                    transform.Rotate(new Vector3(-transform.localRotation.eulerAngles.x, 0f, 0f), Space.Self);
                }
                //Debug.Log(angleX + " " + angleZ);
                //transform.Rotate(new Vector3(angleX, 0f, angleZ), Space.World);

                //if (GravityTransform != null)
                //{
                //if (GravityTransform.rotation.eulerAngles.x >= 180 || GravityTransform.rotation.eulerAngles.z >= 90f)
                //{
                //angle Z 
                //}
                //    float zDiff = transform.rotation.eulerAngles.z-GravityTransform.rotation.eulerAngles.z;
                //    Debug.Log(-zDiff);

                //    transform.Rotate(new Vector3(0f,0f,-zDiff),Space.World);
                //}
            }

        }

        public void Grounded(bool state)
        {
            grounded = state;
        }

        /// <summary>
        /// Spherecast from the bottom of the player. Collision means player in grounded.
        /// </summary>
        public bool GroundCheck()
        {
            Vector3 pos = transform.position + new Vector3(0f, 0.10f, 0f);
            //Debug.DrawRay(pos, (-transform.up * .15f), Color.green,10f);
            
            // cast a shorting ray out the bottom
            //Physics.queriesHitTriggers = false;
            RaycastHit rayhit;
            /// raycast all?
            if (Physics.Raycast(pos, -transform.up, out rayhit, .125f))
            {
                if (TestRaycast(rayhit))
                {
                    return true;
                }
                else
                {
                    for (int r = 0; r <= 360; r += 10)
                    {
                        for (int az = -60; az <= 60; az += 10)
                        {
                            //Debug.Log(r + ";" + az);
                            if (az != 0f)
                            {
                                //adjust direction by the angles
                                Vector3 rot1Vec = Quaternion.AngleAxis(az + 90, transform.forward) * pos;//azimuth angles
                                //Debug.DrawRay(pos, (rot1Vec * .15f), Color.cyan, 0.1f);
                                Vector3 rot2Vec = Quaternion.AngleAxis(r, -transform.up) * rot1Vec;
                                //Debug.DrawRay(pos, (rot2Vec * .15f), Color.red, .1f);
                                if (Physics.Raycast(pos, rot2Vec, out rayhit, .125f))
                                {
                                    return TestRaycast(rayhit);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                ///
                /// Cast a ray every twenty degrees from 0 to 60 azimuth, and every 20 in a 360 circle.
                ///
                for(int r = 0; r <= 360; r += 20)
                {
                    for(int az = -60; az <= 60; az += 20)
                    {
                        if(az != 0f)
                        {
                            //adjust direction by the angles
                            Vector3 rot1Vec = Quaternion.AngleAxis(az+90, transform.forward) * pos;//azimuth angles
                            //Debug.DrawRay(pos, (rot1Vec * .15f), Color.cyan, 0.1f);
                            Vector3 rot2Vec = Quaternion.AngleAxis(r, -transform.up) * rot1Vec;
                            //Debug.DrawRay(pos, (rot2Vec * .15f), Color.red, .1f);
                            if (Physics.Raycast(pos, rot2Vec, out rayhit, .125f))
                            {
                                return TestRaycast(rayhit);
                            }
                        }
                    }
                }
            }
            floorTransform = null;
            floorNormal = null;
            floorHitPosition = null;
            return false;
        }

        private bool TestRaycast(RaycastHit rayhit)
        {
            if (!rayhit.transform.CompareTag("Player"))
            {
                // Get the surface normal
                floorTransform = rayhit.transform;
                floorNormal = rayhit.normal;
                floorHitPosition = rayhit.point;
                return true;
            }
            else
            {
                //Debug.Log("");
                floorTransform = null;
                floorNormal = null;
                floorHitPosition = null;
                return false;
            }
        }

        /// <summary>
        /// Set velocity of the player
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void MovePlayer(float x, float y)
        {
            //only move if grounded (or wearing certain equipment)


            // deltaTime and fixedDeltaTime are the same in a fixed update step
            Vector3 velocity = 50f * movementSpeed * Time.deltaTime * new Vector3(x, 0f, y);

            // This used to work...
            //velocity.y = rigidbody.velocity.y;

            // transform player movement directions to world axes. Does not account for angle of floor.
            velocity = transform.TransformDirection(velocity);

            //Debug.Log("1: "+velocity);
            //
            
            // project velocity onto the movement surface
            if (floorNormal != null)
            {
                //on slopes, movement is not along plane
                velocity = Vector3.ProjectOnPlane(velocity, (Vector3)floorNormal);
            }
            // when not touching the ground, directions are not aligned with gravity.
            // they need to be.
            else if(gravityTransform != null)
            {
                //movement along gravity normal (up)
                velocity = Vector3.ProjectOnPlane(velocity, gravityTransform.TransformDirection(gravityDirection));
                Debug.Log("FN Fault");
            }
            else
            {
                Debug.Log("GT Fault");
            }

            //Debug.Log("2: "+velocity);
                
            // set local velocity (for local movement direction logic)
            LocalVelocity = velocity;

            Vector3 floorVel;// = Vector3.zero;
            if (floorTransform != null)
            {
                // get velocity of floorTransform (with and without rigidbody?)
                // use the rigidbody found when trigger entered
                // else: Use the last ground coordinates to interpolate a velocity
                if (FloorMasterRB != null)
                {
                    //Debug.Log(FloorMasterRB.velocity);
                    floorVel = FloorMasterRB.velocity;
                }
                else
                {
                    if (FloorOldWorldPosition != null)
                    {
                        floorVel = ((Vector3)FloorOldWorldPosition - floorTransform.position) / Time.deltaTime;
                        //Debug.Log("FOWP");
                    }
                    else
                    {
                        floorVel = Vector3.zero;
                        Debug.Log("ZERO");
                        Debug.Log("----------------------------");
                    }
                }
                // get latest x,y coords of ground detection, find the difference between real and projected distances

                // if floorOldWorldPosition is not null, check false velocity
                if (FloorOldWorldPosition != null)
                {
                    if (floorVel.magnitude == 0 && ((Vector3)FloorOldWorldPosition - floorTransform.position).magnitude > 0f)
                    {
                        //Debug.Log(((Vector3)FloorOldWorldPosition).x + " " +
                        //    ((Vector3)FloorOldWorldPosition).y + " " + ((Vector3)FloorOldWorldPosition).z);
                        //Debug.Log(floorTransform.position.x + " " + floorTransform.position.y + " " + floorTransform.position.z);
                        floorVel = ((Vector3)FloorOldWorldPosition - floorTransform.position) / Time.deltaTime;
                    }
                }
                if(floorHitPosition != null && floorOldHitPosition != null)
                {
                    if (FloorOldVelocity.magnitude != 0f && floorVel.magnitude != 0f)
                    {
                        //get distance travelled by the player in last frame
                        Vector3 distance = (Vector3)floorOldHitPosition - (Vector3)floorHitPosition;
                        // remove the distance the player travelled due to local velocity
                        distance += velocity * Time.deltaTime;//+= b/c vel, dist are opposite.
                        // remove the distance the floor was supposed to travel
                        distance += (floorVel * Time.deltaTime);//+=
                        // adjust player velocity by the distance offset
                        velocity += (distance);
                        // move player to theoretical location -> (oldposition + oldvelocity) // - realposition
                        //Vector3 interpolatedPosition =
                        //    ((Vector3)floorOldHitPosition + (FloorOldVelocity * Time.deltaTime));//-((Vector3)floorHitPosition);
                        //Debug.Log("Old ideal velocity: " + (FloorOldVelocity * Time.deltaTime));
                        //Debug.Log("Old position X:" + ((Vector3)floorOldHitPosition).x + " Z:" + ((Vector3)floorOldHitPosition).z);
                        //Debug.Log("Real Location X: " + ((Vector3)floorHitPosition).x + " Z:" + ((Vector3)floorHitPosition).z);
                        //Debug.Log("theoretical location: " + interpolatedPosition);
                        //set the velocity?
                        //rigidbody.MovePosition(interpolatedPosition);

                    }
                }
            }
            else
            {
                // floor velocity persists
                floorVel = FloorOldVelocity;
                Debug.Log("FT-V Fault");
            }

            // velocity is having player try to clip/walk into things. Can we use .move somehow?
            //Debug.Log(floorVel);
            rigidbody.velocity = velocity + floorVel;
            //Debug.Log(rigidbody.velocity);
        }

        /// <summary>
        /// The passed the deltas will be separated into x,y,z comps and applied on local axes
        /// </summary>
        /// <param name="rate"></param>
        public void RotatePlayerAround(Vector3 angleDeltas)
        {
            if (FloorMasterTransform != null)
            {
                //Debug.Log("RotateAround " + angleDeltas);
                if (angleDeltas.x != 0f)//pitch
                {
                    transform.RotateAround(FloorMasterRB.transform.position, transform.right, angleDeltas.x * -1f);
                }
                if (angleDeltas.y != 0f)//yaw
                {
                    transform.RotateAround(FloorMasterRB.transform.position, transform.up, angleDeltas.y * -1f);
                }
                if (angleDeltas.z != 0f)//roll
                {
                    //handle with gravity?
                    //transform.RotateAround(FloorMasterRB.transform.position, transform.forward, angleDeltas.z*1f);
                }

                if (angleDeltas.magnitude > 0f) { 
                    Vector3 playerFloorDelta = playerLocalOldPosition - transform.localPosition;
                    // get the player's velocity
                    Vector3 playerVelocity = rigidbody.velocity;
                    //Debug.Log(playerLocalOldPosition.x +" "+ playerLocalOldPosition.y + " " + playerLocalOldPosition.z);//FloorMasterTransform.TransformPoint(playerLocalOldPosition)
                    //Debug.Log(transform.localPosition.x + " " + transform.localPosition.y + " " + transform.localPosition.z);
                    // if playerfloorDelta is greater than playerVelocity, move the player to the extrapolated velocity position
                    if (playerFloorDelta.magnitude > playerVelocity.magnitude)
                    {
                         //Debug.Log("move to: " + 
                         //    playerLocalOldPosition.x + " " + playerLocalOldPosition.y + " " + playerLocalOldPosition.z 
                         //    + " + "+ playerVelocity);
                        // move the player to the extrapolated position
                        //transform.localPosition = playerLocalOldPosition + playerVelocity;
                        rigidbody.MovePosition(FloorMasterTransform.TransformPoint(playerLocalOldPosition + playerVelocity));
                    }
                }
            }
        }

        public bool IsEquipped(IEquipable equipment)
        {
            if (EquippedGear.Contains(equipment))
            {
                return true;
            }
            foreach (IEquipable eq in EquippedWeapons)
            {
                if (equipment.Equals(eq)){
                    return true;
                }
            }
            foreach (IEquipable eqt in EquippedTools)
            {
                if (equipment.Equals(eqt))
                {
                    return true;
                }
            }
            foreach (IEquipable eqg in EquippedGadgets)
            {
                if (equipment.Equals(eqg))
                {
                    return true;
                }
            }
            return false;
        }

        public void EquipFromQuickSelect(int i)
        {
            // switch between throwables/consumables
            if (i == 1)
            {
                selectedCons++;
                if (selectedCons > EquippedConsumables.Length)
                {
                    selectedCons = 0;
                }
            }
            // switch between gadgets
            else if (i == 2)
            {

            }
            // toggle tools or weapons
            else if(i == 3)
            {
                if (toolmode)
                {
                    //pull out gunz
                    toolmode = false;
                    if (RightHandEquipped != null)
                    {
                        //dequip Right Hand
                        DequipItem(RightHandEquipped.gameObject);
                    }
                    if(EquippedWeapons[0] != null)
                    {
                        EquipItem(EquippedWeapons[0].gameObject, EquippedWeapons[0].EquipmentSlot);
                    }
                }
                else
                {
                    //pull out tools
                    toolmode = true;
                    if (RightHandEquipped != null)
                    {
                        //dequip Right Hand
                        DequipItem(RightHandEquipped.gameObject);
                    }
                    if (EquippedTools[0] != null)
                    {
                        EquipItem(EquippedTools[0].gameObject, EquippedTools[0].EquipmentSlot);
                    }
                }
            }
        }

        /// <summary>
        /// Remove one consumable from the appropriate stack
        /// Equip another of that consumable from the stack
        /// </summary>
        public void ConsumableCallback(ThrowableType throwableType)
        {
            IPlayer_Inventory inv = GetComponent<IPlayer_Inventory>();
            List<ItemStack> throwables = inv.GetPlayerInventory().FindAll(x => x.GetOriginalType() == typeof(Consumable_Throwable));
            ItemStack throwStack = throwables.Find(x => (x.GetItemArray().GetValue(0) as Consumable_Throwable).ThrowType == throwableType);
            throwStack.RemoveItemData(1);

            if(throwStack.GetItemArray().GetValue(0) != null)
            {
                //equip the next! yay!
                Consumable_Throwable obj = (throwStack.GetItemArray().GetValue(0) as Consumable_Throwable);
                EquipItem(obj.gameObject, obj.EquipmentSlot);
            }
            else
            {
                inv.RemoveFromPlayerInventory(throwStack);
            }
        }

        public void UsableCallback(ApplyableType applytype)
        {
            IPlayer_Inventory inv = GetComponent<IPlayer_Inventory>();
            List<ItemStack> applyables = inv.GetPlayerInventory().FindAll(x => x.GetOriginalType() == typeof(Consumable_Applyable));
            List<ItemStack> seedStacks = applyables.FindAll(x => (x.GetItemArray().GetValue(0) as Consumable_Applyable).ThisApplyableType == applytype);
            ItemStack stack = null;
            if (applytype == ApplyableType.Seed)
            {
                stack = seedStacks.Find(x => (x.GetItemArray().GetValue(0) as PlantSeed).Seeds == 0);
            }

            if( stack != null)
            {
                inv.RemoveFromPlayerInventory(stack);
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
                //Debug.Log("Show");
                //disable other controls like LMB, E, Enter, R, etc.
                controls.Player.Crouch.Disable();
                controls.Player.Prone.Disable();
                controls.Player.Alt.Disable();
                controls.Player.Shift.Disable();
                controls.Player.Flashlight.Disable();

                LockScreenAndFreeCursor();//playerRoot.GetComponent<SupplementalController>().
                fleetBoy.SetActive(true);
                fleetBoy.GetComponent<FleetBoy2000UIController>().Refresh();
                fleetBoyOut = true;
            }
            else
            {
                //Debug.Log("Hide");
                controls.Player.Crouch.Enable();
                controls.Player.Prone.Enable();
                controls.Player.Alt.Enable();
                controls.Player.Shift.Enable();
                controls.Player.Flashlight.Enable();
                FreeScreenAndLockCursor();//playerRoot.GetComponent<SupplementalController>().
                fleetBoy.SetActive(false);
                fleetBoyOut = false;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void InflictPlayerDamageServerRpc(float amount)
        {
            playerNetHealth.Value -= amount;
            if (playerNetHealth.Value <= 0)
            {
                playerNetHealth.Value = 0;
            }
        }

        public float PlayerHealth
        {
            get { return playerHealth; }
            set { playerHealth = value; }
        }

        public void EquipItem(GameObject objToEquip,Slot bone)
        {
            Debug.Log("Attempt to equip");
            switch (bone)
            {
                case Slot.Head:
                    break;
                case Slot.Hips:
                    break;
                case Slot.LeftFoot:
                    break;
                case Slot.LeftForearm:
                    break;
                case Slot.LeftHand:
                    break;
                case Slot.LeftLeg:
                    break;
                case Slot.LeftShoulder:
                    break;
                case Slot.LeftUpperArm:
                    break;
                case Slot.LeftUpperLeg:
                    break;
                case Slot.LowerBack:
                    break;
                case Slot.RightFoot:
                    break;
                case Slot.RightForearm:
                    break;
                case Slot.RightHand:
                    Debug.Log("RH");
                    IEquipable eq = objToEquip.GetComponent<IEquipable>();
                    rightHand = eq;
                    objToEquip.transform.SetParent(tempRHBone.transform);//rightHandBone
                    objToEquip.transform.localPosition = eq.ModelOffset;
                    objToEquip.transform.localRotation = Quaternion.Euler(eq.ModelRotation);
                    if(objToEquip.TryGetComponent<Weapon_Gun>(out Weapon_Gun g))
                    {
                        g.Base.PlayerRB = gameObject.GetComponent<Rigidbody>();
                        g.Base.AmmoUI = ammoUI;
                        g.Base.AmmoUI.UpdateGunID(g.ID.Split('_')[1]);
                        //g.Base.AmmoUI.UpdateAmmoTotal(g.Base.MagCap);
                    }
                    else if (objToEquip.TryGetComponent<MiningDrill>(out MiningDrill d))
                    {
                        d.Player = this.gameObject;
                    }
                    else if(objToEquip.TryGetComponent<Consumable_Throwable>(out Consumable_Throwable t))
                    {
                        t.SC = this;
                    }
                    objToEquip.GetComponent<Rigidbody>().detectCollisions = false;
                    objToEquip.GetComponent<Rigidbody>().isKinematic = true;
                    objToEquip.GetComponent<Rigidbody>().useGravity = false;
                    objToEquip.SetActive(true);
                    break;
                case Slot.RightLeg:
                    break;
                case Slot.RightShoulder:
                    break;
                case Slot.RightUpperArm:
                    break;
                case Slot.RightUpperLeg:
                    break;
                case Slot.Torso:
                    break;
                case Slot.Undersuit:
                    break;    
            }
        }

        public void DequipItem(GameObject objToDequip)//, Slot bone
        {
            Debug.Log("Attempt to dequip");
           /* switch (bone)
            {
                case Slot.Head:
                    break;
                case Slot.Hips:
                    break;
                case Slot.LeftFoot:
                    break;
                case Slot.LeftForearm:
                    break;
                case Slot.LeftHand:
                    break;
                case Slot.LeftLeg:
                    break;
                case Slot.LeftShoulder:
                    break;
                case Slot.LeftUpperArm:
                    break;
                case Slot.LeftUpperLeg:
                    break;
                case Slot.LowerBack:
                    break;
                case Slot.RightFoot:
                    break;
                case Slot.RightForearm:
                    break;
                case Slot.RightHand:*/
                    IEquipable eq = objToDequip.GetComponent<IEquipable>();
            rightHand = null;
            objToDequip.transform.SetParent(waistBone.transform);//rightHandBone
                    objToDequip.transform.localPosition = eq.ModelOffset;
                    objToDequip.transform.localRotation = Quaternion.Euler(eq.ModelRotation);
            objToDequip.GetComponent<Rigidbody>().useGravity = false;
            objToDequip.GetComponent<Rigidbody>().isKinematic = true;
            objToDequip.GetComponent<Rigidbody>().detectCollisions = false;
            objToDequip.SetActive(false);
             /*       break;
                case Slot.RightLeg:
                    break;
                case Slot.RightShoulder:
                    break;
                case Slot.RightUpperArm:
                    break;
                case Slot.RightUpperLeg:
                    break;
                case Slot.Torso:
                    break;
                case Slot.Undersuit:
                    break;
            }*/
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
                playerRoot.transform.SetPositionAndRotation(data.Position, Quaternion.Euler(0, dataRot.y, 0));
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
        public void HidePlayerUI()
        {
            foreach(GameObject obj in uiMasterList)
            {
                obj.SetActive(false);
            }
        }
        public void ShowPlayerUI()
        {
            foreach (GameObject obj in uiMasterList)
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

        [ServerRpc(RequireOwnership = false)]
        public void SelectWeaponServerRpc()
        {
            SelectWeaponClientRpc();
        }

        [ClientRpc]
        void SelectWeaponClientRpc()
        {
            if(netSelectedWeapon.Value == 0)
            {
                if(EquippedWeapons[1] != null)
                {
                    DequipItem(EquippedWeapons[1].gameObject);
                }
                if(EquippedWeapons[0] != null)
                {
                    EquipItem(EquippedWeapons[0].gameObject, Slot.RightHand);
                }
            }
            else
            {
                if (EquippedWeapons[0] != null)
                {
                    DequipItem(EquippedWeapons[0].gameObject);
                }
                if (EquippedWeapons[1] != null)
                {
                    EquipItem(EquippedWeapons[1].gameObject, Slot.RightHand);
                }
            }
            canDrawWep = true;
        }
    }
}