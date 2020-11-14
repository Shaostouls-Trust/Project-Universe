using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using AX;


// Attach this behavior to any UI GameOBject that you want to have Build the AXModel OnMOuseUp.

public class AXRuntimeUIElement : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
	public AXModel model;
	
	public void OnPointerDown (PointerEventData data)
	{
	}

	public void OnPointerUp (PointerEventData data)
	{
		if (model != null)
		{
			model.autobuild();
		}	
	}

}
