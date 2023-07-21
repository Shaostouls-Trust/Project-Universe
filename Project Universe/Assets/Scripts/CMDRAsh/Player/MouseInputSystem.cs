using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CMF;
using Unity.Netcode;

namespace ProjectUniverse.Player.PlayerController {
    public class MouseInputSystem : CameraInput
    {
        [SerializeField] private SupplementalController controller;
        private PlayerControls controls;

        //Invert input options;
        [SerializeField] private bool invertHorizontalInput = false;
        [SerializeField] private bool invertVerticalInput = false;
        [SerializeField] private Vector2 freelookHorizontalBounds;
        [SerializeField] private Vector2 freelookVerticalBounds;
        [SerializeField] private float mouseInputMultiplier =0.075f;

        private float horizontalInput = 0f;
        private float verticalInput = 0f;

        private void OnEnable()
        {
            try
            {
                controls.Player.Look.Enable();
            }
            catch (System.NullReferenceException)
            {
                Debug.Log("Controls not initialized.");
            }
        }

        private void OnDisable()
        {
            controls.Player.Look.Disable();
        }

        void Start()
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var networkedClient))
            {
                controls = networkedClient.PlayerObject.gameObject.GetComponent<SupplementalController>().PlayerController;
            }
            else
            {
                controls = new ProjectUniverse.PlayerControls();
            }
            controls.Player.Look.Enable();
            controls.Player.Look.performed += ctx =>
            {
                if (!controller.ShipMode)
                {
                    if (controller.CameraLocked == false)
                    {
                        horizontalInput = ctx.ReadValue<Vector2>().x * mouseInputMultiplier * Time.deltaTime * (1f / Time.unscaledDeltaTime);
                        verticalInput = ctx.ReadValue<Vector2>().y * mouseInputMultiplier * Time.deltaTime * (1f / Time.unscaledDeltaTime);
                    }
                    else
                    {
                        horizontalInput = 0f;
                        verticalInput = 0f;
                    }
                }
                // rotate ship
                else
                {
                    horizontalInput = 0f;
                    verticalInput = 0f;
                    // pass rotation info to ship controller
                    controller.RemoteLookAxis_Horizonal = ctx.ReadValue<Vector2>().x * 
                        mouseInputMultiplier * Time.deltaTime * (1f / Time.unscaledDeltaTime);
                    controller.RemoteLookAxis_Vertical = ctx.ReadValue<Vector2>().y * 
                        mouseInputMultiplier * Time.deltaTime * (1f / Time.unscaledDeltaTime);

                }
            };
            controls.Player.Look.canceled += ctx =>
            {
                horizontalInput = 0f;
                verticalInput = 0f;
            };
        }

        public override float GetHorizontalCameraInput()
        {
            if (!controller.CameraLocked)
            {
                float _input = horizontalInput;

                if (Time.timeScale > 0f && Time.deltaTime > 0)
                {
                    _input *= Time.timeScale;
                }
                else
                {
                    _input = 0f;
                }

                if (invertHorizontalInput) 
                { 
                    _input *= -1f; 
                }
                return _input;
            }
            else
            {
                return 0f;
            }
            
        }

        public override float GetVerticalCameraInput()
        {
            if (!controller.CameraLocked)
            {
                float _input = -verticalInput;
                if (Time.timeScale > 0f && Time.deltaTime > 0)
                {
                    _input *= Time.timeScale;
                }
                else
                {
                    _input = 0f;
                }
                if (invertVerticalInput)
                {
                    _input *= -1f;
                }
                return _input;
            }
            else
            {
                return 0f;
            }
                
        }

        
    }
}