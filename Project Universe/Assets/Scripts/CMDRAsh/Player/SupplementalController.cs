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
        private IEquipable[] equippedWeapons = new IEquipable[3];
        private IEquipable[] equippedTools = new IEquipable[2];
        private IEquipable[] equippedGadgets = new IEquipable[5];
        private IEquipable[] equippedConsumables = new IEquipable[3];//how will this work with having more than one consumable?
        private List<IEquipable> equippedGear = new List<IEquipable>();
        private IEquipable rightHand;
        private bool fleetBoyOut = false;
        private float lookClamp;
        private int selectedWeapon = 0;
        private bool canDrawWep = true;
        private bool toolmode = false;//whether the player has guns or tools out
        private int selectedCons = 0;
        [SerializeField] private GunAmmoUI ammoUI;

        private NetworkVariableBool netFlashlightState = new NetworkVariableBool(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.Everyone });
        private NetworkVariableInt netSelectedWeapon = new NetworkVariableInt(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.Everyone }, 0);

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
        public bool FleetBoyOut
        {
            get { return fleetBoyOut; }
            set { fleetBoyOut = value; }
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

            controls.Player.Fire.Enable();
            controls.Player.Reload.Enable();
            controls.Player.Num1.Enable();
            controls.Player.Num2.Enable();
            controls.Player.Num3.Enable();
            controls.Player.Num4.Enable();
            controls.Player.ScrollWheel.Enable();
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

            controls.Player.Fire.Disable();
            controls.Player.Reload.Disable();
            controls.Player.Num1.Disable();
            controls.Player.Num2.Disable();
            controls.Player.Num3.Disable();
            controls.Player.Num4.Disable();
            controls.Player.ScrollWheel.Disable();
        }

        private void Start()
        {
            NetworkListeners();
            GetComponent<CMF.AdvancedWalkerController>().movementSpeed = walkSpeed;

            //Message whatever is in the player's right hand to fire, or perform it's FIRE action
            controls.Player.Fire.performed += ctx =>
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
            };
            controls.Player.Fire.canceled += ctx =>
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
            };
            controls.Player.Reload.performed += ctx =>
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
            };
            controls.Player.Num1.performed += ctx =>
            {
                if (!FleetBoyOut)
                {
                    Debug.Log("qs throwables");
                    EquipFromQuickSelect(1);
                }
            };
            controls.Player.Num2.performed += ctx =>
            {
                if (!FleetBoyOut)
                {
                    Debug.Log("qs gadgets");
                    EquipFromQuickSelect(2);
                }
            };
            controls.Player.Num3.performed += ctx =>
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
            };
            controls.Player.Num4.performed += ctx =>
            {
                Debug.Log("qs tools");
                EquipFromQuickSelect(3);
            };

            controls.Player.ScrollWheel.performed += ctx =>
            {
                if (canDrawWep)
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
                
            };

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
                    //Throw consumable
                    GameObject oldEq = null;
                    if (RightHandEquipped != null)
                    {
                        oldEq = RightHandEquipped.gameObject;
                        DequipItem(oldEq);
                    }
                    
                    if(EquippedConsumables[selectedCons] != null)
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
            };

            controls.Player.Sprint.performed += ctx =>
            {
                sprinting = true;
                if (crouching)
                {
                    GetComponent<CMF.AdvancedWalkerController>().movementSpeed += 1;
                }
                else if (prone)
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
                controls.Player.Sprint.Disable();
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