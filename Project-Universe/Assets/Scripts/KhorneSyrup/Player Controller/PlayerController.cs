using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUniverseData.PlayerController
{
    public class PlayerController : MonoBehaviour
    {
        //Begin Input list add to seperate client side script later.
        [Header("Input Manager")]
        [SerializeField] private string mouseXInputName;
        [SerializeField] private string mouseYInputName;
        [SerializeField] private string horizontalInputName;
        [SerializeField] private string verticalInputName;
        [SerializeField] private string jumpInputName;
        [SerializeField] private string crouchInputName;
        [SerializeField] private string interactInputName;
        [SerializeField] private string sprintInputName;
        [SerializeField] private string reloadInputName;
        [SerializeField] private string attackInputName;
        [SerializeField] private string secAttackInputName;
        [SerializeField] private string adsInputName;
        [SerializeField] private string offHandInputName;
        [SerializeField] private string flashLightInputName;
        [SerializeField] private float mouseXSensitivity, mouseYSensitivity;
        //End Input List
        [Header("AssetData")]
        [SerializeField] private Camera firstPersonCamera;
        [SerializeField] private Transform playerRoot;
        [SerializeField] private CharacterController charController;
        [SerializeField] private Light flashLight;
        private int activeFL = 0;
        //Movement Settings
        private float movementSpeed;
        [SerializeField] private float walkSpeed = 5.50f;
        [SerializeField] private float sprintSpeed = 8.00f;
        [SerializeField] private float walkSpeedRamp = 2.50f;

        [SerializeField] private float slopeDownForceMult;
        [SerializeField] private float slopeForceRayL;

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

        // Start is called before the first frame update
        void Start()
        {

        }

        void Awake()
        {
            //LockCursor();
            lookClamp = 0;
        }
        //Lock the cursor to the center of screen.
        void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        // Update is called once per frame
        void Update()
        {
            CameraControl();
            PlayerControl();
        }

        private void PlayerControl()
        {
            PlayerMovement();
            PlayerInteraction();
            FlashLight();
        }
        private void PlayerInteraction()
        {
            Ray ray = firstPersonCamera.ViewportPointToRay(Vector3.one / 2f);
            RaycastHit hit;
            Debug.DrawRay(ray.origin, ray.direction * 2f, Color.red);
            if (Input.GetButtonDown(interactInputName) && Prop == null)
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
                        //TargetScr.Interact();
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
            }
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
            float mouseX = Input.GetAxis(mouseXInputName) * mouseXSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis(mouseYInputName) * mouseYSensitivity * Time.deltaTime;

            lookClamp += mouseY;

            if (lookClamp > 90.0f)
            {
                lookClamp = 90.0f;
                mouseY = 0.0f;
                ClampLookRotationToValue(270.0f);
            }
            else if (lookClamp < -90.0f)
            {
                lookClamp = -90.0f;
                mouseY = 0.0f;
                ClampLookRotationToValue(90);
            }

            firstPersonCamera.transform.Rotate(Vector3.left * mouseY);
            playerRoot.Rotate(Vector3.up * mouseX);

        }
        //Controls direction of player movement. (ObViOuSlY)
        private void PlayerMovement()
        {
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

            setMovementSpeed();

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

            if (Input.GetButton(sprintInputName) && !isSwimming)
            {
                movementSpeed = Mathf.Lerp(movementSpeed, sprintSpeed, Time.deltaTime * walkSpeedRamp);
            }
            if (Input.GetButton(sprintInputName) && isSwimming)
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
        private void jumpInput()
        {
            if (Input.GetButtonDown(jumpInputName) && !isSwimming && currentJump < maxJump)
            {
                isJumping = true;
                timeInAir = 0.0f;
                StartCoroutine(JumpEvent());
                currentJump++;
            }
        }

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

            if (Input.GetButtonDown(flashLightInputName) && activeFL <= 0)
            {
                flashLight.enabled = true;
                activeFL = 1;
            }
            else if (Input.GetButtonDown(flashLightInputName) && activeFL >= 1)
            {
                flashLight.enabled = false;
                activeFL = 0;
            }
        //Do flashlight stuff
        }
        //Kinda primitive, may adjust later.
        //Locks player camera so they can not look up and down in a 360 deg arc.
        private void ClampLookRotationToValue(float value)
        {
            Vector3 eulerRotation = transform.eulerAngles;
            eulerRotation.x = value;
            firstPersonCamera.transform.eulerAngles = eulerRotation;
        }
    }
}