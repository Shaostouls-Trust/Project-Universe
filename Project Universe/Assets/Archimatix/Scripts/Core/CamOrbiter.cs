using UnityEngine;
using System.Collections;

public class CamOrbiter : MonoBehaviour {
	/* 
	This Orbiting camera works with
	polor coordinates.
	
*/
	
	
	// The target we are following
	public Transform cameraTargetTransform;
	
	enum CameraProjection {Persp, Plan, Lateral, Transverse, Axonometric}
	
	public bool isDraggingView = false;
	
	
	public float radiusFromTarget			= 100;
	public float desiredRadiusFromTarget	= 80;
	public float minRadiusFromTarget		= .1f;
	public float maxRadiusFromTarget		= 500;
	
	public float desiredOrthographicSize	= 25;
	public float minOrthographicSize		= 3;
	public float maxOrthographicSize		= 100;
	
	public float alpha						= 15;
	public float desiredAlpha				= 15;
	
	public float beta						= 15;
	public float desiredBeta				= 15;
	
	public float deltaBeta					= 0;
	public float deltaBetaRange				= 45;
	
	public float betaRange					= 135;
	public float northPoleBeta				= 90;
	
	public float desiredX					= 0;
	public float desiredY					= 0;
	
	
	public float minHgtAboveGround 			= 50;
	
	float lerpFactorDolly					= 4;
	public bool autoDolly							= false;
	
	
	
	
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
		if(Input.GetMouseButtonDown(0)) 
		{
			isDraggingView = true;
		}
		if (Input.GetMouseButtonUp(0))
		{
			isDraggingView = false;
		}
		
		
		
		
		
		// ORBIT CONTROL
		float h;
		float v;
		
		if (isDraggingView) // not iOS device
		{
			h = (5000/Screen.width) * Input.GetAxis("Mouse X");
			v = (5000/Screen.width)  * Input.GetAxis("Mouse Y");
			
			desiredAlpha += h;
			desiredBeta -= v;
			
			desiredBeta = Mathf.Clamp(desiredBeta, -89, 89);			
			
		}
		
	}
	
	
	
	
	
	
	
	
	
	void FixedUpdate() {
		
		// LERP BETWEEN ACTUAL AND DESIRED VALUES

		// 1. Go to target, then back away
		transform.position = cameraTargetTransform.position;
		
		
		// 3. Reorient the camera to the axis of the orbit
		float lerpFactorRotate = 2;
		if (isDraggingView) {
			lerpFactorRotate = 6;
		}
		transform.rotation = Quaternion.identity;	
		alpha 	= Mathf.Lerp(alpha, desiredAlpha, lerpFactorRotate*Time.deltaTime);
		beta 	= Mathf.Lerp(beta, desiredBeta, lerpFactorRotate*Time.deltaTime);
		transform.Rotate(new Vector3(beta,alpha,0));
		
		// 4. Dolly out to radius or to collider
		lerpFactorDolly = 1;
		
		
		radiusFromTarget = Mathf.Lerp (radiusFromTarget, desiredRadiusFromTarget, lerpFactorDolly * Time.deltaTime );
		var distance = radiusFromTarget;
		transform.position -=  transform.rotation * Vector3.forward * distance;
		
		
	}
}
