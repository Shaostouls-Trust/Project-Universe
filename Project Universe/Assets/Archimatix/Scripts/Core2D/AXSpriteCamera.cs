using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AXSpriteCamera {

 
	// The camera is alwayspositioned relative to the
	// parametricObject.


	// The Z is important, with X and y values 
	// as offests from the x,y of the po.
	public Vector3 relativePosition;



	public bool isOrthographic = true;

	public float orthographicSize = 5.0f;
	public float fieldOfView = 60;

	public void init()
	{
		relativePosition = new Vector3(0, 0, -10);

	}




}
