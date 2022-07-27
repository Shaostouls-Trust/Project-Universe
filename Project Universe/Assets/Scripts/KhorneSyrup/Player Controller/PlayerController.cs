using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectUniverse.Player.PlayerController
{
    public sealed class PlayerController : NetworkBehaviour
    {
        [Header("NetworkVariables")]
        //[SerializeField] private NetworkVariableVector3 Position = new NetworkVariableVector3(new NetworkVariableSettings
       // {
        //    WritePermission = NetworkVariablePermission.ServerOnly,
        //    ReadPermission = NetworkVariablePermission.Everyone
        //});
        //Begin Input list add to seperate client side script later.
        [Header("Input Manager")]
        [SerializeField] private float mouseXSensitivity, mouseYSensitivity;
        //Pointer lock and centering
        [SerializeField] int CursorCase = 0;
        [SerializeField] private bool toggleCursorLock = false;

        //End Input List
        [Header("AssetData")]
        [SerializeField] private Camera firstPersonCamera;
        [SerializeField] private bool cameraLocked = false;
        [SerializeField] private Transform playerRoot;
        [SerializeField] private CharacterController charController;
        [SerializeField] private Light flashLight;
        //[SerializeField] private PlayerGuiController guiController;
        private int activeFL = 0;
        //Movement Settings
        private float movementSpeed;
        [SerializeField] private float walkSpeed = 5.50f;
        [SerializeField] private float sprintSpeed = 8.00f;
        [SerializeField] private float walkSpeedRamp = 2.50f;

        [SerializeField] private float slopeDownForceMult;
        [SerializeField] private float slopeForceRayL;

        [SerializeField] private bool sprinting = false;

        //Jump Settings
        [SerializeField] private float jumpForce = 10.0f;
        [SerializeField] private float jumpMult = 1.0f;
        [SerializeField] private bool isJumping = false;
        [SerializeField] private AnimationCurve jumpFallOff;
        [SerializeField] private int currentJump = 0;
        [SerializeField] private int maxJump = 1;

        //Swim Settings
        [SerializeField] private bool isSwimming = false;
        [SerializeField] private float swimSpeed = 6.00f;
        [SerializeField] private float swimSprintSpeed = 7.50f;

        //Interaction stuff
        private GameObject Prop = null;
        private Rigidbody PropR = null;

        private float lookClamp;
        private float timeInAir = 0.0f;

        private NetworkVariableBool netFlashlightState = new NetworkVariableBool(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.Everyone });

        private ProjectUniverse.PlayerControls controls;

        // Start is called before the first frame update
        void Start()
        {
            //Lock Cursor to center by default
            //LockCursor();
            NetworkListeners();
            if (IsLocalPlayer)
            {
                charController = GetComponent<CharacterController>();
                charController.enabled = true;
                controls = gameObject.GetComponent<SupplementalController>().PlayerController;
            }
            else
            {
                charController.enabled = false;
                //turn off the camera and audio listener
                firstPersonCamera.enabled = false;
                firstPersonCamera.GetComponent<AudioListener>().enabled = false;
            }
            //controls.Player.Move.Enable(); Broken.
            controls.Player.Alt.Enable();
            controls.Player.Shift.Enable();
            controls.Player.Flashlight.Enable();
            //controls.Player.Jump.Enable(); Broken
            controls.Player.Look.Enable();

            controls.Player.Move.performed += ctx =>
            {
                float verticalInput = ctx.ReadValue<Vector2>().y;//Input.GetAxis(verticalInputName);
                Debug.Log(ctx.ReadValue<Vector2>().y+","+ ctx.ReadValue<Vector2>().x);
                float horizontalInput = ctx.ReadValue<Vector2>().x;//Input.GetAxis(horizontalInputName);

                Vector3 forwardMovement = playerRoot.transform.forward * verticalInput;
                Vector3 strafeMovement = playerRoot.transform.right * horizontalInput;

                charController.SimpleMove(Vector3.ClampMagnitude(forwardMovement + strafeMovement, 1.0f) * movementSpeed);

                if ((verticalInput != 0 || horizontalInput != 0) && OnSlope())
                {
                    charController.Move(Vector3.down * charController.height / 2 * slopeForceRayL * Time.deltaTime);
                }
                //Check if we're under water.
                if (!isSwimming)
                {
                    /*if (!isSwimming && currentJump < maxJump)
                    {
                        isJumping = true;
                        timeInAir = 0.0f;
                        StartCoroutine(JumpEvent());
                        currentJump++;
                    }*/
                }
                setMovementSpeed();
            };

            controls.Player.Alt.performed += ctx =>
            {
                Debug.Log("LeftAlt Pressed!");
                LockCursor();
                cameraLocked = !cameraLocked;
            };

            controls.Player.Shift.performed += ctx =>
            {
                sprinting = true;
            };
            controls.Player.Shift.canceled += ctx =>
            {
                sprinting = false;
            };

            controls.Player.Jump.performed += ctx =>
            {
                if (!isSwimming && currentJump < maxJump)
                {
                    isJumping = true;
                    timeInAir = 0.0f;
                    StartCoroutine(JumpEvent());
                    currentJump++;
                }
            };

            controls.Player.Flashlight.performed += ctx =>
            {
                FlashLightToggleServerRpc(!flashLight.enabled);
            };

            controls.Player.Look.performed += ctx =>
            {
                if (cameraLocked == false)
                {
                    float mouseX = ctx.ReadValue<Vector2>().x * mouseXSensitivity * Time.deltaTime;//Input.GetAxis(mouseXInputName)
                    float mouseY = ctx.ReadValue<Vector2>().y * mouseYSensitivity * Time.deltaTime;//Input.GetAxis(mouseYInputName)

                    lookClamp += mouseY;

                    if (lookClamp > 90.0f)
                    {
                        lookClamp = 90.0f;
                        mouseY = 90.0f;
                        ClampLookRotationToValue(270.0f);
                    }
                    else if (lookClamp < -90.0f)
                    {
                        lookClamp = -90.0f;
                        mouseY = -90.0f;
                        ClampLookRotationToValue(90);
                    }

                    firstPersonCamera.transform.Rotate(Vector3.left * mouseY);
                    playerRoot.Rotate(Vector3.up * mouseX);
                }
            };
        }

        public void NetworkListeners()
        {
            flashLight.enabled = false;
            netFlashlightState.Value = false;
            netFlashlightState.OnValueChanged += delegate { flashLight.enabled = netFlashlightState.Value; };
        }

        void Awake()
        {
            //LockCursor();
            //lookClamp = 0;
        }
        //Lock the cursor to the center of screen.
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

        // Update is called once per frame
        void Update()
        {
            if (IsLocalPlayer)
            {
                CameraControl();
                PlayerControl();
                //GuiUpdate();
            }
            //else
            //{
            //    charController.gameObject.transform.position = Position.Value;
            //}
        }

        public override void NetworkStart()
        {
            //Move();
        }

        ///Temp location - Technically belongs in a GUI controller
        ///Used with GUIs to free the cursor for GUI interaction
        public void LockAndFreeCursor()
        {
            LockCursor();//guiController.
            cameraLocked = !cameraLocked;
            Cursor.visible = true;
        }
        public void UnlockCursor()
        {
            LockCursor();
            cameraLocked = !cameraLocked;
            Cursor.visible = false;
        }
        /// 

        private void GuiUpdate()
        {
           
            /*
            if (Input.GetButtonDown(openInventoryInputName))
            {
                //guiController.OpenWindow(0);
            }
            if (Input.GetButtonDown(openMapInputName))
            {
                guiController.OpenWindow(1);
            }
            if (Input.GetButtonDown(openOptionsInputName))
            {
                guiController.OpenWindow(2);
            }
            else { }*/
        }

        private void PlayerControl()
        {
            PlayerMovement();
            //PlayerInteraction();
            FlashLight();
        }
        private void PlayerInteraction()
        {
            Ray ray = firstPersonCamera.ViewportPointToRay(Vector3.one / 3f);
            RaycastHit hit;
            Debug.DrawRay(ray.origin, ray.direction * 3f, Color.red);
           /* if (Input.GetButtonDown(interactInputName) && Prop == null)
            {
                if (Physics.Raycast(ray, out hit, 2f))
                {
                    //Button Case
                    if (hit.collider.gameObject.tag == "Button")
                    {
                        Interactable TargetScr = hit.collider.GetComponent<Interactable>();
                        TargetScr.Interact();
                        Debug.Log("You Used a button!!!");
                    }
                    //Prop Case
                    if (hit.collider.gameObject.tag == "Prop")
                    {
                        Interactable TargetScr = hit.collider.GetComponent<Interactable>();
                        TargetScr.Interact();
                        Prop = hit.collider.gameObject;
                        PropR = Prop.GetComponent<Rigidbody>();
                        PropR.useGravity = false;
                        PropR.isKinematic = true;
                        Prop.transform.parent = this.transform;
                    }
                    else { Debug.Log("Help, I'm blind!"); }
                }
            }
            else if (Input.GetButtonDown(interactInputName) && Prop != null)
            {
                DropProp(1);
            }
            else if (Input.GetMouseButtonDown(0) && Prop != null)
            {
                DropProp(2);
            }
            if (Prop != null)
            {
                Prop.transform.position = ray.GetPoint(2.0f);
            }*/
        }
        private void DropProp(int mode)
        {
            PropR.useGravity = true;
            PropR.isKinematic = false;
            if (mode == 1)
            {
            }
            else if (mode == 2)
            {

                PropR.AddForce(firstPersonCamera.transform.forward * 1000 / PropR.mass, ForceMode.Impulse);
            }
            Prop.transform.parent = null;
            Prop.transform.localScale = Vector3.one;
            Prop = null;
        }

        //Control the camera during regular movement.
        private void CameraControl()
        {
            //if (Input.GetButtonDown(lockCursorInputName))
            //{
            //    Debug.Log("LeftAlt Pressed!");
            //    LockCursor();
            //    cameraLocked = !cameraLocked;
            //}

           /* if (cameraLocked == false) { 
                float mouseX = Input.GetAxis(mouseXInputName) * mouseXSensitivity * Time.deltaTime;
                float mouseY = Input.GetAxis(mouseYInputName) * mouseYSensitivity * Time.deltaTime;

                lookClamp += mouseY;

                if (lookClamp > 90.0f)
                {
                    lookClamp = 90.0f;
                    mouseY = 90.0f;//shouldn't this be 90.0f not 0.0f?
                    ClampLookRotationToValue(270.0f);
                }
                else if (lookClamp < -90.0f)
                {
                    lookClamp = -90.0f;
                    mouseY = -90.0f;//was 0.0f
                    ClampLookRotationToValue(90);
                }

                firstPersonCamera.transform.Rotate(Vector3.left * mouseY);
                playerRoot.Rotate(Vector3.up * mouseX);
            }
            else {}*/
        }
        //Controls direction of player movement. (ObViOuSlY)
        private void PlayerMovement()
        {
            /*
            float verticalInput = Input.GetAxis(verticalInputName);
            float horizontalInput = Input.GetAxis(horizontalInputName);

            Vector3 forwardMovement = playerRoot.transform.forward * verticalInput;
            Vector3 strafeMovement = playerRoot.transform.right * horizontalInput;

            charController.SimpleMove(Vector3.ClampMagnitude(forwardMovement + strafeMovement, 1.0f) * movementSpeed);

            if ((verticalInput != 0 || horizontalInput != 0) && OnSlope())
            {
                charController.Move(Vector3.down * charController.height / 2 * slopeForceRayL * Time.deltaTime);
            }
            //Check if we're under water.
            if (!isSwimming)
            {
                jumpInput();
            }
            setMovementSpeed();*/

        }
        //slope movement stability.
        private bool OnSlope()
        {
            if (isJumping)
            {
                return false;
            }
            RaycastHit hit;

            if (Physics.Raycast(playerRoot.position, Vector3.down, out hit, charController.height / 2 * slopeForceRayL))
            {
                if (hit.normal != Vector3.up)
                {
                    return true;
                }
            }
            return false;
        }

        //Determine if the player is sprinting and set the players movement speed.
        private void setMovementSpeed()
        {

            if (sprinting && !isSwimming)
            {
                movementSpeed = Mathf.Lerp(movementSpeed, sprintSpeed, Time.deltaTime * walkSpeedRamp);
            }
            if (sprinting && isSwimming)
            {
                movementSpeed = Mathf.Lerp(movementSpeed, swimSprintSpeed, Time.deltaTime * walkSpeedRamp);
            }
            if (isSwimming)
            {
                movementSpeed = Mathf.Lerp(movementSpeed, swimSpeed, Time.deltaTime * walkSpeedRamp);
            }
            else
            {
                movementSpeed = Mathf.Lerp(movementSpeed, walkSpeed, Time.deltaTime * walkSpeedRamp);
            }
        }

        //Determine if the player has pressed the jump key, and if so, activate the jump event.
       /* private void jumpInput()
        {
            if (Input.GetButtonDown(jumpInputName) && !isSwimming && currentJump < maxJump)
            {
                isJumping = true;
                timeInAir = 0.0f;
                StartCoroutine(JumpEvent());
                currentJump++;
            }
        }*/

        private IEnumerator JumpEvent()
        {
            float jumpForce = jumpFallOff.Evaluate(timeInAir);
            charController.slopeLimit = 90.0f;
            do
            {
                timeInAir += Time.deltaTime;
                charController.Move(Vector3.up * jumpForce * jumpMult * Time.deltaTime);
                yield return null;
            } while (!charController.isGrounded && charController.collisionFlags != CollisionFlags.Above);

            isJumping = false;
            currentJump = 0;
            timeInAir = 0.0f;
            charController.slopeLimit = 47.0f;
        }

        private void FlashLight()
        {
            ///
            /// On Hold F and Mousewheel, allow flashlight OuterAngle to go from 10 to 120deg
            /// Decrease range as the angle broadens
            ///
            //if (Input.GetButtonDown(flashLightInputName))// && activeFL <= 0)
            //{
                //FlashLightToggleServerRpc(!flashLight.enabled);
                //flashLight.enabled = true;
                //activeFL = 1;
           // }
            //else if (Input.GetButtonDown(flashLightInputName) && activeFL >= 1)
            //{
            //    flashLight.enabled = false;
            //    activeFL = 0;
            //}
        //Do flashlight stuff
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
            firstPersonCamera.transform.eulerAngles = eulerRotation;
        }
    }
}