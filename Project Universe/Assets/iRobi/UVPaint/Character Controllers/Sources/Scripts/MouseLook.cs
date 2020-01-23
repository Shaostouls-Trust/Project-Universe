using UnityEngine;
using System.Collections;

/// MouseLook rotates the transform based on the mouse delta.
/// Minimum and Maximum values can be used to constrain the possible rotation

/// To make an FPS style character:
/// - Create a capsule.
/// - Add the MouseLook script to the capsule.
///   -> Set the mouse look to use LookX. (You want to only turn character but not tilt it)
/// - Add FPSInputController script to the capsule
///   -> A CharacterMotor and a CharacterController component will be automatically added.

/// - Create a camera. Make the camera a child of the capsule. Reset it's transform.
/// - Add a MouseLook script to the camera.
///   -> Set the mouse look to use LookY. (You want the camera to tilt up and down like a head. The character already turns.)
[AddComponentMenu("Camera-Control/Mouse Look")]
public class MouseLook : MonoBehaviour {

	public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
	public RotationAxes axes = RotationAxes.MouseXAndY;
	public float sensitivityX = 15F;
	public float sensitivityY = 15F;

	public float minimumX = -360F;
	public float maximumX = 360F;

	public float minimumY = -60F;
	public float maximumY = 60F;

	float rotationY = 0F;
	float smoothX=180,smoothY;

	float rotationX;
		Vector2 Delta;

	void Update ()
	{
		if (axes == RotationAxes.MouseXAndY)
				{
						smoothX += (Input.GetAxis("Mouse X")+Delta.x)* sensitivityX/3;
			rotationX = Mathf.Lerp(rotationX,smoothX,Time.deltaTime*25);
			
						smoothY += Input.GetAxis("Mouse Y") * sensitivityY+Delta.y;
						smoothY = Mathf.Clamp (smoothY, minimumY, maximumY);
			rotationY = Mathf.Lerp(rotationY,smoothY,Time.deltaTime*25);
			
			transform.localEulerAngles = new Vector3(-rotationY, rotationX+90, 0);
						Delta.x = Input.GetAxis ("Mouse X");
						Delta.y = Input.GetAxis ("Mouse Y");
		}
		else if (axes == RotationAxes.MouseX)
		{
						smoothX = Mathf.Lerp (smoothX, (Input.GetAxis ("Mouse X") + Delta.x) * sensitivityX,Time.deltaTime*25);
						transform.Rotate(0,smoothX, 0);
						Delta.x = Input.GetAxis ("Mouse X");
		}
		else
		{
			smoothY += Input.GetAxis("Mouse Y") * sensitivityY;
			rotationY = Mathf.Lerp(rotationY,smoothY,Time.deltaTime*25);
			rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);
			
			transform.localEulerAngles = new Vector3(-rotationY, transform.localEulerAngles.y, 0);
		}
	}
	
	void Start ()
	{
		// Make the rigid body not change rotation
		if (GetComponent<Rigidbody>())
			GetComponent<Rigidbody>().freezeRotation = true;
		Cursor.visible = false;
		Screen.lockCursor = true;
	}
}