using ProjectUniverse.Player.PlayerController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CMF
{
	//This character movement input class is an example of how to get input from a keyboard to control the character;
    public class CharacterKeyboardInput : CharacterInput
    {
		[SerializeField] private SupplementalController controller;
		public string horizontalInputAxis = "Horizontal";
		public string verticalInputAxis = "Vertical";
		public KeyCode jumpKey = KeyCode.Space;

		//If this is enabled, Unity's internal input smoothing is bypassed;
		public bool useRawInput = true;

        public override float GetHorizontalMovementInput()
		{
			if (!controller.ShipMode)
			{
				if (useRawInput)
					return Input.GetAxisRaw(horizontalInputAxis);
				else
					return Input.GetAxis(horizontalInputAxis);
            }
            else
            {
				if (useRawInput)
					controller.RemoteMoveAxis_Horizonal = Input.GetAxisRaw(horizontalInputAxis);
				else
					controller.RemoteMoveAxis_Horizonal = Input.GetAxis(horizontalInputAxis);
				return 0f;
            }
		}

		public override float GetVerticalMovementInput()
		{
			if (!controller.ShipMode)
			{
				if (useRawInput)
					return Input.GetAxisRaw(verticalInputAxis);
				else
					return Input.GetAxis(verticalInputAxis);
            }
            else
            {
				if (useRawInput)
					controller.RemoteMoveAxis_Vertical = Input.GetAxisRaw(verticalInputAxis);
				else
					controller.RemoteMoveAxis_Vertical = Input.GetAxis(verticalInputAxis);
				return 0f;
            }
		}

		public override bool IsJumpKeyPressed()
		{
			if (!controller.ShipMode) 
			{ 
				return Input.GetKey(jumpKey);
            }
            else
            {
                if (Input.GetKey(jumpKey))
                {
					controller.RemoteJump = 1f;
                }
                else
                {
					controller.RemoteJump = 0f;
				}
				return false;
            }
		}
    }
}
