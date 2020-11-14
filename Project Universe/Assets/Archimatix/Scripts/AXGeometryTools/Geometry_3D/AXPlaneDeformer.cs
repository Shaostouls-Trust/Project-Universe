using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using AXGeometry;


public class AXPlaneDeformer 
{
	// Take any mesh and flaten its Y-values against a plane...
	
	public static Mesh flatten(Mesh m, Plane p)
	{
		
		Vector3[] verts = new Vector3[m.vertices.Length] ;
		
		for (int i = 0; i < m.vertices.Length; i++) {
			Vector3 vert = m.vertices [i];
			
			Ray ray = new Ray (new Vector3(vert.x, 0, vert.z), Vector3.up);
			
			float 	rayDistance = 0;
			
			// If the ray makes contact with the ground plane then
			// position the marker at the distance along the ray where it
			// crosses the plane.
			if (p.Raycast(ray, out rayDistance)) 
				verts[i] = ray.GetPoint(rayDistance);
			else
				verts[i] = vert;
			
			
		}
		m.vertices = verts;
		return m;
		
	}
	

		
}
