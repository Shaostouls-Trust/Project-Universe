using UnityEngine;

public class PlayerLook : MonoBehaviour
{
	#region Variables
	[SerializeField]
	private float maxAngle;
	public float MaxAngle { get; private set; }
	[SerializeField]
	private float minAngle;
	public float MinAngle { get; private set; }
	[SerializeField]
	private float horizLookSpeed;
	public float HorizLookSpeed { get; private set; }
	[SerializeField]
	private float vertLookSpeed;
	public float VertLookSpeed { get; private set; }

	private float mouseY;

	[SerializeField]
	private Transform playerCamera;
	#endregion

	#region Methods

	void Update()
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
