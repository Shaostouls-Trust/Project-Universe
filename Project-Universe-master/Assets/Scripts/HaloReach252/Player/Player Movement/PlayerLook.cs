﻿using UnityEngine;
using Mirror;

public class PlayerLook : NetworkBehaviour
{
	#region Variables
	[SyncVar]
	public float maxAngle;
	[SyncVar]
	public float minAngle;
	[SyncVar]
	public float horizLookSpeed;
	[SyncVar]
	public float vertLookSpeed;
	[SyncVar]
	public float mouseY;
	[SyncVar]
	private Transform playerCamera;
	#endregion

	#region Methods

	void Start()
	{
		if (!isLocalPlayer)
			return;

		playerCamera = GetComponent<Player>().playerCam.transform;
	}

	void Update()
    {
		if (!isLocalPlayer)
			return;

		CmdLook();
    }

	[Command]
	void CmdLook()
	{
		//Gets mouse rotation
		float mouseX = Input.GetAxis("Mouse X") * horizLookSpeed;
		mouseY += Input.GetAxis("Mouse Y") * vertLookSpeed;

		//Sets the PLAYER rotation horizontally
		transform.Rotate(new Vector3(0, mouseX, 0));

		//Clamps the vertical rotation, then rotates the CAMERA vertically
		mouseY = Mathf.Clamp(mouseY, minAngle, maxAngle);
		playerCamera.localEulerAngles = new Vector3(-mouseY, playerCamera.localEulerAngles.y, playerCamera.localEulerAngles.z);
	}
	
	#endregion
}
