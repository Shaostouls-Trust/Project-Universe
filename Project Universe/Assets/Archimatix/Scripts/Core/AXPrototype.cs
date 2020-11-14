using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;



using AX.Generators;
using AXGeometry;

using AX;

public class AXPrototype : MonoBehaviour {

	 

	// LIST of ParametricObjects that use this as a prototype
	[System.NonSerialized]
	private List<AXParametricObject> _parametricObjects;
	public  List<AXParametricObject>  parametricObjects
	{
		get 
		{
			if (_parametricObjects == null)
				_parametricObjects = new List<AXParametricObject>();

			return _parametricObjects;
		}

	}


}
