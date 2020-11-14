#pragma warning disable 0114 // supress warning about hiding Start()

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using AX;


public class AXRuntimeControllerBase : MonoBehaviour {


	// In the inspector, drag your AXModel from the hiearchy window to 
	// this variable.
	public AXModel 		model;

	public static bool runtimeHandleIsDown;

	protected bool parameterWasAltered;


	#region AUTO_GENERATED_AX_BINDINGS


	// *** PARAMETER_REFERENCES_DEFINITION *** //



	// *** DYNAMIC_VARIABLES *** //



	// *** PARAMETER_REFERENCE_INIT *** //


	protected virtual void  InitializeParameterReferences()
	{

	}

	#endregion




	// Use this for initialization

	protected void InitializeController() {

		// Assume that this controller is attached to a GameObject 
		// under the AXModel's GameObject.

		if (model == null && transform.parent != null)
			model = transform.parent.gameObject.GetComponent<AXModel>();


		// Build the model to make sure its
		// starting form reflects all its internal parameters.
		if (model != null)
		{ 
			// BIND THE EXPOSED RUNTIME PARAMETERS
			// This way you don't have to do the model lookup with  
			// each seting or getting of the parameter value.
			InitializeParameterReferences();

			// Make sure the model is built with current parameter values.
			model.autobuild();


		}
	}



	// Use this whenever you want to rebuild the GameObjects for this model.
	public void buildGameObjects()
	{
		if (model != null)
			model.autobuild();
	}
	public void buildAllGameObjects()
	{
		if (model != null)
			model.buildAll();
	}









}
