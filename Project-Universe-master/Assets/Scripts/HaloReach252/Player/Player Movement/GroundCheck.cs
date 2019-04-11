using UnityEngine;

public class GroundCheck : MonoBehaviour
{
	#region Variables
	public LayerMask layerMask;
	#endregion

	#region Methods

	public bool IsGrounded()
	{
		Collider[] hits = Physics.OverlapBox(transform.position, transform.localScale, transform.rotation, layerMask);
		for (int i = 0; i < hits.Length; i++)
		{
			if(hits[i].tag == "Ground")
				return true;
		}

		return false;
	}
	
	#endregion
}
