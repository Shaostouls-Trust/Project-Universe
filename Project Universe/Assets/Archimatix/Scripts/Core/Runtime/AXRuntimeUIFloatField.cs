using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AX;

public class AXRuntimeUIFloatField : MonoBehaviour {




	public AXModel model;

	public string P_GUID;

	[System.NonSerialized]
	public AXParameter P;


	public float P_value {
		get {
			if (model != null)
				return P.FloatVal;
			return 0;
		}
		set {
			if (P != null)
			{
				P.setValue(value);

				if (model != null)
					model.isAltered();
			}
		}
	}


	// Use this for initialization
	void Start () {

		if (! string.IsNullOrEmpty(P_GUID))
			P = model.getParameterByGUID (P_GUID);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
