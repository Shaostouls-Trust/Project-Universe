using CMF;
using MLAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectUniverse.Player.PlayerController
{
    public class KeyboardInputSystem : CharacterInput
    {
        private PlayerControls controls;
        private float movementX;
        private float movementY;
        private bool jumping;
        private bool onLadder = false;
        private bool onLadderMoving = false;
        private bool onLadderEnd = false;
        private bool coroutineRunning = false;
        private AdvancedWalkerController awc;
        private bool reverseLadderDir;
        private Vector3 ladderforward;

        public bool OnLadder
        {
            get { return onLadder; }
            set { onLadder = value; }
        }

        public bool OnLadderMoving
        {
            get { return onLadderMoving; }
            set { onLadderMoving = false; }
        }

        // Start is called before the first frame update
        void Start()
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var networkedClient))
            {
                controls = networkedClient.PlayerObject.gameObject.GetComponent<SupplementalController>().PlayerController;
                awc = GetComponent<AdvancedWalkerController>();
            }
            else
            {
                controls = new ProjectUniverse.PlayerControls();
            }
            controls.Player.Move.Enable();
            controls.Player.Jump.Enable();

            controls.Player.Jump.performed += ctx =>
            {
                jumping = true;
            };
            controls.Player.Jump.canceled += ctx =>
            {
                jumping = false;
            };

            controls.Player.Move.performed += ctx =>
            {
                //Debug.Log("Move Performed");
                if (onLadder)
                {
                    onLadderMoving = true;
                    Vector2 movementVector = ctx.ReadValue<Vector2>();
                    if (movementVector.y < 0f)
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
                        StartCoroutine("OnLadderMovement");
                    }
                }
            };
            controls.Player.Move.canceled += ctx =>
            {
                //Debug.Log("Move Canceled");
                if (onLadder)
                {
                    onLadderMoving = false;
                }
            };
        }

        public void OnMove(InputValue movementValue)
        {
            Vector2 movementVector = movementValue.Get<Vector2>();
            if (!onLadder)
            {
                movementX = movementVector.x;
                movementY = movementVector.y;
            }
        }

        public void EndOfLadder(Vector3 forward)
        {
            onLadderEnd = true;
            ladderforward = forward;
        }
        public void EndOfLadder()
        {
            OnLadder = false;
            OnLadderMoving = false;
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
                    awc.SetMomentum(new Vector3(0f, 0.6f, 0f) + (ladderforward*3f));//don't let the player move down
                    timer--;
                    if(timer <= 0f)
                    {
                        Debug.Log("End");
                        OnLadder = false;
                        OnLadderMoving = false;
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
                            awc.SetMomentum(new Vector3(0f, 3f, 0f));
                        }
                        else
                        {
                            awc.SetMomentum(new Vector3(0f, -3f, 0f));
                        }
                    }
                    else
                    {
                        if (angle >= 0f)
                        {
                            awc.SetMomentum(new Vector3(0f, -3f, 0f));
                        }
                        else
                        {
                            awc.SetMomentum(new Vector3(0f, 3f, 0f));
                        }
                    }
                    
                }
                else
                {
                    awc.SetMomentum(new Vector3(0f, 0.6f, 0f));//the player constantly moves down at abt this speed on ladder
                }
                yield return null;
            }
        }

        public void OnEnable()
        {
            if(controls != null)
            {
                controls.Player.Move.Enable();
                controls.Player.Jump.Enable();
            }
        }
        private void OnDisable()
        {
            controls.Player.Move.Disable();
            controls.Player.Jump.Disable();
        }

        public override float GetHorizontalMovementInput()
        {
            return movementX;
        }

        public override float GetVerticalMovementInput()
        {
            return movementY;
        }

        public override bool IsJumpKeyPressed()
        {
            return jumping;
        }
    }
}