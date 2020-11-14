using UnityEngine;
using System.Collections;

public class AXSpline3D {

	public Vector3[] verts;
	public int vertCount = 0;
	
	public static int builtinAllocBlock = 30;
	
	 
	
	public AXSpline3D() {
		Clear();
	}
	
	public void Clear() {
		verts = new Vector3[builtinAllocBlock];
		vertCount = 0;
	}

	public Vector2[] getVertsAsVector2s() {
		// Makes sure that the array returned 
		// has no empty items at the end.
		Vector2[] tmpverts = new Vector2[vertCount];
		
		for(int i = 0; i<this.vertCount; i++) {
			tmpverts[i] = new Vector2(verts[i].x, verts[i].z);
		}
		return tmpverts;
		
	}

	
	public void CheckToReallocBuiltinVector3() {
		//Check if we potentially hit the end of the array
		int sizeCheck = ((vertCount + 1) % builtinAllocBlock);
		// - Debug.Log("Size check - count: " + vertCount + " sizecalc: " + sizeCheck);
		if( sizeCheck  == 0 )
		{
			int newSize = (((vertCount + 1) / builtinAllocBlock) + 1) * builtinAllocBlock;
			// - Debug.Log("Reallocation of vert builtin array from size: " + vertCount + " to: " + newSize);
			//We need a more space.
			Vector3[] temp  = new Vector3[newSize];
			
			//Crude copy
			for(int i = 0; i < vertCount; i++)
			{
				temp[i] = verts[i];
			}
			
			//Overwrite old
			verts = temp;
		}
	}
	
	/*************************
	 * PUSH
	 *************************/
	public void Push(Vector3 v) {
		
		bool samePoint = false;
		float range = .001f;
		
		if (vertCount > 0)
			samePoint = (v - verts[0]).sqrMagnitude < range*range;
		//Debug.Log (s.vertCount + " -- " + (vert - s.verts[0]).sqrMagnitude + " ... " + samePoint);
		
		if (! samePoint)
		{
			CheckToReallocBuiltinVector3();
			
			verts[vertCount] = v;
			vertCount++;
		}
		
	}











}
