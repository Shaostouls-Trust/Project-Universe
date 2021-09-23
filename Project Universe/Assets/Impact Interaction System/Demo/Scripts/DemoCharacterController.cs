using Impact.Triggers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Impact.Demo
{
    public class DemoCharacterController : MonoBehaviour
    {
        [SerializeField]
        private Rigidbody characterRigidbody;

        [Header("Movement")]
        [SerializeField]
        private float moveSpeed = 10;
        [SerializeField]
        private float jumpForce = 5;
        [SerializeField]
        private float sprintMultiplier = 2;
        [SerializeField]
        private float sneakMultiplier = 0.5f;
        [SerializeField]
        private float movementSmoothing = 0.1f;

        [Header("Camera")]
        [SerializeField]
        private Transform cameraTransform;
        [SerializeField]
        private float sensitivity = 1;
        [SerializeField]
        private float cameraSmoothing = 0.1f;

        [Header("Pick Up Objects")]
        [SerializeField]
        private float pickedUpDrag = 20f;
        [SerializeField]
        private float pickupHoldForce = 20f;
        [SerializeField]
        private float throwForce = 20f;

        [Header("Footsteps")]
        [SerializeField]
        private ImpactTag footstepLeftTag;
        [SerializeField]
        private ImpactTag footstepRightTag;
        [SerializeField]
        private float footstepInterval;
        [SerializeField]
        private Vector3 footOffset;

        [Header("Weapon")]
        [SerializeField]
        private ImpactTag bulletTag;
        [SerializeField]
        private float bulletForce;

        private Vector2 targetCharacterRotation = Vector2.zero;
        private Vector2 smoothedCharacterRotation, smoothedCharacterRotationV;

        private Vector3 targetMovement = Vector2.zero;
        private Vector3 smoothedMovement, smoothedMovementV;

        private bool isSneaking;
        private bool isGrounded;

        private Vector3 previousPosition;
        private float distanceTravelled;

        private Rigidbody pickedUpObject;
        private float pickupDistance;

        private int foot = 1;

        private void Awake()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        #region Updates

        private void Update()
        {
            targetMovement = getMovementInput();

            Vector2 mouseDelta = new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X")) * sensitivity;
            targetCharacterRotation += mouseDelta;

            smoothedCharacterRotation = Vector2.SmoothDamp(smoothedCharacterRotation, targetCharacterRotation, ref smoothedCharacterRotationV, cameraSmoothing);
            smoothedMovement = Vector3.SmoothDamp(smoothedMovement, targetMovement, ref smoothedMovementV, movementSmoothing);

            cameraTransform.localRotation = Quaternion.Euler(smoothedCharacterRotation.x, 0, 0);

            if (Input.GetKeyDown(KeyCode.Space))
                characterRigidbody.AddForce(Vector3.up * jumpForce);

            if (Input.GetMouseButtonDown(0))
            {
                if (pickedUpObject == null)
                    triggerWeapon();
                else
                    dropObject(throwForce);
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (pickedUpObject == null)
                {
                    if (!pickupObject())
                    {
                        pressButton(false);
                    }
                }

                else
                    dropObject(0);
            }

            if (Input.GetKey(KeyCode.E) && pickedUpObject == null)
            {
                pressButton(true);
            }
        }

        private void FixedUpdate()
        {
            characterRigidbody.MoveRotation(Quaternion.Euler(0, smoothedCharacterRotation.y, 0));
            characterRigidbody.velocity = new Vector3(smoothedMovement.x, characterRigidbody.velocity.y, smoothedMovement.z);

            updateGrounded();

            updatePickedUpObject();
        }

        private void LateUpdate()
        {
            Vector3 currentPosition = transform.position;
            float distanceTravelledThisFrame = Vector3.Distance(previousPosition, currentPosition);

            distanceTravelled += distanceTravelledThisFrame;

            if (isGrounded && distanceTravelled > footstepInterval)
            {
                distanceTravelled = 0;
                foot = -foot;

                triggerFootstep();
            }

            previousPosition = currentPosition;
        }

        #endregion

        #region Impact Footstep and Bullet Impact Integration

        private void triggerFootstep()
        {
            //Velocity is a 0-1 value used to scale the volume of the footstep sounds
            float velocity = isSneaking ? 0.25f : 1;

            RaycastHit hit;
            Ray r = new Ray(transform.position + transform.rotation * footOffset, Vector3.down);
            if (Physics.Raycast(r, out hit))
            {
                InteractionData data = new InteractionData()
                {
                    Velocity = Vector3.down * velocity,
                    CompositionValue = 1,
                    PriorityOverride = 100,
                    ThisObject = this.gameObject
                };

                if (foot > 0)
                    data.TagMask = footstepRightTag.GetTagMask();
                else if (foot < 0)
                    data.TagMask = footstepLeftTag.GetTagMask();

                ImpactRaycastTrigger.Trigger(data, hit, true);
            }
        }

        private void triggerWeapon()
        {
            RaycastHit hit;
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit))
            {
                InteractionData data = new InteractionData()
                {
                    Velocity = cameraTransform.forward * bulletForce,
                    CompositionValue = 1,
                    PriorityOverride = 100,
                    ThisObject = this.gameObject,
                    TagMask = bulletTag.GetTagMask()
                };

                ImpactRaycastTrigger.Trigger(data, hit, false);

                if (hit.rigidbody != null)
                    hit.rigidbody.AddForceAtPosition(cameraTransform.forward * bulletForce, hit.point);
            }
        }

        #endregion

        #region Objects and Buttons

        private bool pickupObject()
        {
            RaycastHit hit;
            if (Physics.Raycast(cameraTransform.position + cameraTransform.forward, cameraTransform.forward, out hit, 5))
            {
                if (hit.rigidbody != null)
                {
                    pickupDistance = Mathf.Max(3, hit.distance);
                    pickedUpObject = hit.rigidbody;
                    pickedUpObject.useGravity = false;
                    pickedUpObject.drag = pickedUpDrag;
                    pickedUpObject.angularDrag = pickedUpDrag;

                    return true;
                }
            }

            return false;
        }

        private void updatePickedUpObject()
        {
            if (pickedUpObject != null)
            {
                Vector3 target = cameraTransform.position + cameraTransform.forward * pickupDistance;
                Vector3 dir = target - pickedUpObject.position;

                pickedUpObject.AddForce(dir * pickupHoldForce, ForceMode.VelocityChange);
            }
        }

        private void dropObject(float force)
        {
            if (pickedUpObject != null)
            {
                pickedUpObject.useGravity = true;
                pickedUpObject.drag = 0;
                pickedUpObject.angularDrag = 0.05f;
                pickedUpObject.AddForce(cameraTransform.forward * force);
                pickedUpObject = null;
            }
        }

        private bool pressButton(bool hold)
        {
            RaycastHit hit;
            if (Physics.Raycast(cameraTransform.position + cameraTransform.forward, cameraTransform.forward, out hit, 5))
            {
                DemoButton button = hit.collider.GetComponentInParent<DemoButton>();

                if (button != null)
                {
                    if (hold)
                        button.Hold();
                    else
                        button.Press();
                }
            }

            return false;
        }
        #endregion

        #region Movement

        private Vector3 getMovementInput()
        {
            Vector3 movementInput = Vector3.zero;

            if (Input.GetKey(KeyCode.W))
                movementInput.z = 1;
            else if (Input.GetKey(KeyCode.S))
                movementInput.z = -1;

            if (Input.GetKey(KeyCode.A))
                movementInput.x = -1;
            else if (Input.GetKey(KeyCode.D))
                movementInput.x = 1;

            movementInput.Normalize();
            movementInput *= moveSpeed;

            if (Input.GetKey(KeyCode.LeftShift))
                movementInput *= sprintMultiplier;

            isSneaking = Input.GetKey(KeyCode.LeftControl);
            if (isSneaking)
                movementInput *= sneakMultiplier;

            return transform.rotation * movementInput;
        }

        private void updateGrounded()
        {
            isGrounded = Physics.Raycast(transform.position - new Vector3(0, 0.99f, 0), Vector3.down, 0.1f);
        }

        #endregion
    }
}

