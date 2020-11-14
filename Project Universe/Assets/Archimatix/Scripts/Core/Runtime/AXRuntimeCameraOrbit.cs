#pragma warning disable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AX
{
	public class AXRuntimeCameraOrbit : MonoBehaviour
	{

		public Transform targetRig;
		public Transform target;

		// The distance in the x-z plane to the target
		public float distance = 10.0f;
		public float distanceTarget = -10.0f;
		public float speed = .5f;
		// the height we want the camera to be above the target
		public float height = 5.0f;
		// How much we
		public float heightDamping = 1000;
		public float rotationDampingStart = .5f;
		public float rotationDamping = 2.5f;


		public float beta;
		public float betaTarget;

		public float alpha;
		public float alphaTarget;

		private bool isDraggingView = false;
		private float prevTouchPinchDistance;



		private Vector3 movingTarget_FromPoint;
		private float movingTarget_FromDistance;

		private Vector3 movingTarget_ToPoint;
		private float movingTarget_ToDistance;
		private float movingTarget_Duration;

		private float movingTarget_StartTime;
		private bool movingTarget_IsMoving;

		private float damping;
//		private float dampingTimer = 3000;

		public void OnEnable()
		{
			damping = rotationDampingStart;
		}

		// Update is called once per frame
		void Update ()
		{

			
				
			
			if (Input.GetMouseButtonDown (0) ) {

				
				//Debug.Log("Pressed left click. " +  Input.mousePosition);
				if (Input.mousePosition.y > 182 && Input.mousePosition.y < Screen.height - 65 && !AXRuntimeControllerBase.runtimeHandleIsDown) {
					//Debug.Log ("start dragging");
					isDraggingView = true;
				}

			} else if (Input.GetMouseButtonUp (0)) {
				isDraggingView = false;
			}
        

			float speed = .25f;
	

			if (isDraggingView ) {
		
				//Debug.Log("isDraggingView!");
				if (Input.touchCount > 0) {
					if (Input.GetTouch (0).phase == TouchPhase.Moved) {
					
						Touch touch1 = Input.GetTouch (0); 
						// iOS Input
						Vector2 touchDeltaPosition = touch1.deltaPosition;

						if (Input.touchCount == 1) {
		        	
							// Rotate camera
							target.transform.Rotate (0, touchDeltaPosition.x * speed, 0);
							height -= .5f * speed * touchDeltaPosition.y;
							prevTouchPinchDistance = 0;
		    	
		    	
		    	
		    	
		    	
		    	
						} else if (Input.touchCount == 2) {
		        	
							// PAN
							// by sliding 2 fingers

							// if more than 45 degs, just slide target in x & z
					
							if (touchDeltaPosition.x != 0 || touchDeltaPosition.y != 0) {
								target.transform.Translate (-touchDeltaPosition.x * speed, 0, -touchDeltaPosition.y * speed);
							}
							
							//target.transform.Translate (-speed*touchDeltaPosition.y*this.transform.up.normalized);
							//target.transform.Translate (-touchDeltaPosition.x * speed, 0, 0);
		    	
		    	
							// ZOOM 
							// by pinching
							Touch touch2 = Input.GetTouch (1); 
	
							float touchPinchDistance = Vector2.Distance (touch1.position, touch2.position);
							if (prevTouchPinchDistance > 0) {
								float pinch = touchPinchDistance - prevTouchPinchDistance;
								//print("pinch = " + pinch);
								if (Mathf.Abs (pinch) > 5) {
								
									distance -= pinch / 2.0f;
									if (distance < 5)
										distance = 5;
									height -= pinch / 2.0f;
								}
							}
							prevTouchPinchDistance = touchPinchDistance;
						}
					}
				} else {
					// Mouse Input
	    
					float h = 5 * Input.GetAxis ("Mouse X");
					float v = -10 * Input.GetAxis ("Mouse Y");

					if (h != 0 || v != 0)
					{
						damping = rotationDamping;
					}


					alphaTarget += h;
					betaTarget += v;

					// Rotate the target and the camera will follow

					//target.transform.Rotate(h, 0, 0);
					//height -= .2f*v;
		

					//height -= 1*scroll;
				}
//		if (height < -target.transform.position.y) {
//			height = -target.transform.position.y+1;
//		}
		
			}
	
			float scroll = Input.GetAxis ("Mouse ScrollWheel");
			distance -= 3 * scroll;

	
	
	
	
			speed = 60.0f;
			float rotationSpeed = 150.0f;
	
			float translationH = -Input.GetAxis ("Horizontal") * Time.deltaTime;
			float translationV = -Input.GetAxis ("Vertical") * Time.deltaTime;
	
	
	
			if (Input.GetKey (KeyCode.LeftAlt)) {
		
				// dolly in
				float tmpspeed = speed;
				if (translationV != 0) {
			
					//if (distance < 10) {
					// slow down
					tmpspeed = distance * distance / 20;
					//}
					if (tmpspeed > 100)
						tmpspeed = 200;
			
					float ratio = height / distance;
			
					distance += translationV * tmpspeed;
			
					if (distance < 1)
						distance = 1;
			
					height = ratio * distance;
			
			
			
				}
		
				if (translationH != 0) {
					target.transform.Rotate (0, translationH * -rotationSpeed, 0);
				}
		
				if (distance < 1) {
					distance = 1;
					height = 1;
			
				} 		
		
			} else {
				// divide into components to translate the target up or along z
//				if (translationH != 0 || translationV != 0) {
//					target.transform.Translate (-speed * translationH, 0, -speed * translationV);
//				}
			}

		}
		// UPDATE







		public void LateUpdate ()
		{

			//Early out if we don't have a target
			if (!target)
				return;
	
			// Calculate the current rotation angles





			float wantedHeight = target.position.y + height;
		
	
			float currentHeight = transform.position.y;



			// Damp the rotation around the y-axis
			alpha = Mathf.LerpAngle (alpha, alphaTarget, damping * Time.deltaTime);
			beta = Mathf.LerpAngle (beta, betaTarget, damping * Time.deltaTime);

			distance = Mathf.LerpAngle (distance, distanceTarget, speed * Time.deltaTime);
			// Damp the height
			currentHeight = Mathf.Lerp (currentHeight, wantedHeight, heightDamping * Time.deltaTime);

			var wantedTargetRigY = (beta > 180) ? (360 - beta) / 5 : -beta / 5;
			wantedTargetRigY /= 3;
			wantedTargetRigY -= 4;
			//Debug.Log(currentAlpha);
			float y = Mathf.Lerp (targetRig.position.y, wantedTargetRigY, heightDamping * Time.deltaTime);
			targetRig.position = new Vector3 (targetRig.position.x, y, targetRig.position.z);


			// Convert the angle into a rotation
			var currentRotation = Quaternion.Euler (beta, alpha, 0);
	
			// Set the position of the camera on the x-z plane to:
			// distance meters behind the target
			transform.position = target.position;
			transform.position -= currentRotation * Vector3.forward * distance;

			// Set the height of the camera
			//transform.position.y = currentHeight;
	
			// Always look at the target
			transform.LookAt (target);
	
			RenderSettings.fogDensity = .005f - (Mathf.Sqrt (distance) / 8500.0f);
			if (RenderSettings.fogDensity < .0007f)
				RenderSettings.fogDensity = .0007f;
			//Debug.Log(RenderSettings.fogDensity);
		}









	}
}
