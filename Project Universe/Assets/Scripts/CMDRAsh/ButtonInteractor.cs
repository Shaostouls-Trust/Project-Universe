using Unity.Netcode;
using ProjectUniverse.Player.PlayerController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// raycast
/// when hits WSButton
/// on keygetinput
/// run func
/// </summary>
namespace ProjectUniverse.Environment.Interactable
{
    public class ButtonInteractor : MonoBehaviour
    {
        [SerializeField] private GameObject defaultpointer;
        [SerializeField] private GameObject interactpointer;
        [SerializeField] private float holdThreshold = 0.25f; // Time before considering it a hold //A

        private bool showingInteractPointer = false;
        private GameObject player;
        private bool triggered;
        [SerializeField] private SupplementalController controller;
        private PlayerControls controls;
        private GameObject target;

        //A
        // Hold operation variables
        private bool isHolding = false;
        private bool holdStarted = false;
        private float holdStartTime = 0f;
        private GameObject currentHoldTarget = null;
        private InteractionElement currentInteractionElement = null;

        void Start()
        {
            controls = controller.PlayerController;
            controls.Player.Interact.Enable();
            // Handle both press and release events
            controls.Player.Interact.started += OnInteractStarted;
            controls.Player.Interact.canceled += OnInteractCanceled;


        }

        //A start
        private void OnInteractStarted(InputAction.CallbackContext ctx)
        {
            if (target != null)
            {
                currentHoldTarget = target;
                currentInteractionElement = target.GetComponent<InteractionElement>();

                if (currentInteractionElement != null)
                {
                    // Check if this element supports hold operations
                    if (currentInteractionElement.SupportsHold)
                    {
                        holdStartTime = Time.time;
                        holdStarted = true;

                        // Start hold operation immediately or after threshold
                        if (currentInteractionElement.InstantHoldStart)
                        {
                            StartHoldOperation();
                        }
                    }
                    else
                    {
                        // Fire-and-forget interaction (original behavior)
                        currentInteractionElement.Interact();
                    }
                }
            }
        }

        private void OnInteractCanceled(InputAction.CallbackContext ctx)
        {
            if (isHolding && currentInteractionElement != null)
            {
                // End hold operation
                EndHoldOperation();
            }
            else if (holdStarted && currentInteractionElement != null)
            {
                // Handle quick tap for hold-enabled elements
                if (Time.time - holdStartTime < holdThreshold)
                {
                    if (currentInteractionElement.SupportsQuickTap)
                    {
                        currentInteractionElement.Interact();
                    }
                }
            }

            ResetHoldState();
        }
        private void StartHoldOperation()
        {
            if (!isHolding && currentInteractionElement != null)
            {
                isHolding = true;
                currentInteractionElement.StartHold();
            }
        }

        private void EndHoldOperation()
        {
            if (isHolding && currentInteractionElement != null)
            {
                currentInteractionElement.EndHold();
                isHolding = false;
            }
        }
        private void ResetHoldState()
        {
            holdStarted = false;
            isHolding = false;
            holdStartTime = 0f;
            currentHoldTarget = null;
            currentInteractionElement = null;
        }
        //A end

        private void OnEnable()
        {
            if (controls != null)
            {
                controls.Player.Interact.Enable();
            }

        }

        private void OnDisable()
        {
            // Clean up any active hold operations
            if (isHolding)
            {
                EndHoldOperation();
            }

            controls?.Player.Interact.Disable();
        }

        private void OnDestroy()
        {
            if (controls != null)
            {
                controls.Player.Interact.started -= OnInteractStarted;
                controls.Player.Interact.canceled -= OnInteractCanceled;
            }
        }

        void Update()
        {
            HandleRaycast();
            HandleHoldOperations();
        }

        private void HandleRaycast()
        {
            // Calculate reach distance
            float extensiondistance = Mathf.Lerp(0f, 0.75f, (transform.localRotation.eulerAngles.x / 90f));
            Vector3 forward = transform.TransformDirection(0f, 0f, 1f) * (1.5f + extensiondistance);
            Debug.DrawRay(transform.position, forward, Color.green);

            RaycastHit hit;
            Physics.queriesHitTriggers = true;

            if (Physics.Raycast(transform.position, forward, out hit, (1.5f + extensiondistance)))
            {
                HandleHit(hit);
            }
            else
            {
                HandleNoHit();
            }
        }

        private void HandleHit(RaycastHit hit)
        {
            var interactionElement = hit.collider.gameObject.GetComponent<InteractionElement>();

            if (interactionElement != null)
            {
                // Update UI
                if (!showingInteractPointer)
                {
                    defaultpointer.SetActive(false);
                    interactpointer.SetActive(true);
                    showingInteractPointer = true;
                }

                if (!triggered)
                {
                    // Handle PointerDetector
                    if (hit.collider.gameObject.TryGetComponent(out PointerDetector pd))
                    {
                        pd.ExternalInteractFunc();
                        triggered = true;
                    }

                    target = hit.collider.gameObject;
                }

                // Handle target change during hold
                if (isHolding && currentHoldTarget != hit.collider.gameObject)
                {
                    EndHoldOperation();
                    ResetHoldState();
                }
            }
            else
            {
                HandleTargetLoss();
            }
        }

        private void HandleNoHit()
        {
            HandleTargetLoss();
        }

        private void HandleTargetLoss()
        {
            target = null;

            // End hold operation if target is lost
            if (isHolding)
            {
                EndHoldOperation();
                ResetHoldState();
            }

            if (triggered)
            {
                ResetPlayerUI();
                triggered = false;
            }

            if (showingInteractPointer)
            {
                defaultpointer.SetActive(true);
                interactpointer.SetActive(false);
                showingInteractPointer = false;
            }
        }

        private void HandleHoldOperations()
        {
            // Check if we should start hold operation after threshold
            if (holdStarted && !isHolding && currentInteractionElement != null)
            {
                if (!currentInteractionElement.InstantHoldStart &&
                    Time.time - holdStartTime >= holdThreshold)
                {
                    StartHoldOperation();
                }
            }

            // Update hold operation
            if (isHolding && currentInteractionElement != null)
            {
                float holdDuration = Time.time - holdStartTime;
                currentInteractionElement.UpdateHold(holdDuration);
            }
        }

        private void ResetPlayerUI()
        {
            if (player == null)
            {
                player = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;
            }

            player.GetComponent<SupplementalController>().LockOnlyCursor();
            player.GetComponent<SupplementalController>().ShowCenterUI();
        }
    }
}