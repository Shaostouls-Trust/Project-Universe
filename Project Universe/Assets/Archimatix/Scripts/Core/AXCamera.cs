using UnityEngine;
using System.Collections;

[System.Serializable]
public class AXCamera  {

	public Vector3 targetPosition = new Vector3(10000, 10000, 10000);
	public Vector3 position;
	public Vector3 localPosition;
	
	
	// Spherical coordinates
	
	private float m_radius = 10;
	public  float radius
	{
		get { return m_radius; } 
		set { 
			m_radius = radiusAdjuster * value; 
			}
	} 
	
	public float radiusAdjuster = 1.0f;
	
	public float alpha 	= -65;
	public float beta 	= 45;
	
	
	public void rotate (float deltaDegs)
	{
		
	}
	public void setPosition ()
	{
		localPosition.y 	= radius  * (Mathf.Sin(beta * Mathf.Deg2Rad));

		float dist_xz 		= radius  * (Mathf.Cos(beta * Mathf.Deg2Rad));
		
		localPosition.x 	= dist_xz * (Mathf.Cos(alpha * Mathf.Deg2Rad));
		localPosition.z 	= dist_xz * (Mathf.Sin(alpha * Mathf.Deg2Rad));
		
		position = targetPosition + localPosition;		
	}
	
	
	public void tilt (float deltaDegs)
	{
		
	}
	public void setTilt (float deltaDegs)
	{
		
	}
	
	
	
}
