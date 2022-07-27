using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class EMUI : MonoBehaviour {
	public static bool UIClicked = false;
	public static bool UIHelpOverlay = false;

	private List<RaycastResult> UIRaycast() {
		PointerEventData pointer = new PointerEventData (EventSystem.current);
		pointer.position = Input.mousePosition;
		
		List<RaycastResult> raycastResults = new List<RaycastResult> ();
		EventSystem.current.RaycastAll (pointer, raycastResults);
		
		return raycastResults;
	}

	public bool CheckGUI(int mouseButtonIndex, ref bool UILockInstigator)
	{
		if (Input.GetMouseButton (mouseButtonIndex))
		{
			if (!UIHelpOverlay)
			{
				if (UIClicked && !UILockInstigator) {
					return false;
				}
				
				if (!UIClicked && !UILockInstigator) //first time click
				{
					List<RaycastResult> raycastResults = UIRaycast();
					if (raycastResults.Count > 0 && raycastResults [0].gameObject.layer == 5) //IF MOUSE IS OVER UI
						return false;
					else
					{
						UILockInstigator = true;
						UIClicked = true;
						return true;
					}
				}
				
				if (UIClicked && UILockInstigator) {
					return true;
				}
				
			}
			else
				return false;
		}

		if (Input.GetMouseButtonUp (mouseButtonIndex) && UILockInstigator) {
			UIClicked = false;
			UILockInstigator = false;
			return false;
		}
		return false;
	}
}
