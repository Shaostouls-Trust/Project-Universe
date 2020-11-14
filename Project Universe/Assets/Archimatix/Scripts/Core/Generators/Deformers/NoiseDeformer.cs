using UnityEngine;

using System;
using System.Collections.Generic;

using LibNoise.Unity;
using LibNoise.Unity.Generator;

using Perlin = LibNoise.Unity.Generator.Perlin;

namespace AX.Generators 
{
	/*	CustomNode
	 *  This is a template for an AX Generator
	 *  To generate Mesh output, it is best to subclass Generator3D
	 */
	public class NoiseDeformer : Deformer
	{

		// INPUTS
		// It is nice to have a local variable for parameters set at initialization
		// so that an expensive lookup for the parameter in a Dictionary does not have to be done every frame.
		public AXParameter	P_Octaves;
		public AXParameter	P_Frequency;
		public AXParameter	P_Persistence;
		public AXParameter	P_Lacunarity;
		public AXParameter	P_Amount;

		public AXParameter	P_CenX;
		public AXParameter	P_CenY;
		public AXParameter	P_CenZ;

		public AXParameter	P_ModifierCurve;

		public AXParameter	P_FromCenter;
		public AXParameter	P_VerticalOnly;


		// WORKING FIELDS (Updated every Generate)
		// As a best practice, each parameter value could have a local variable
		// That is set before the generate funtion is called.
		// This will allow Handles to acces the parameter values more efficiently.

		public int 			octaves 		= 2;
		public float 		frequency 		= 2f;
		public float 		persistence 	= .5f;
		public float 		lacunarity 		= 1.5f; // 1.5 to 3.5
		public float 		amount 			= 0;

		Vector3 center = Vector3.zero;

		public float 		cenX;
		public float 		cenY;
		public float 		cenZ;

		public AnimationCurve modifierCurve;
		public bool 		fromCenter;
		public bool 		verticalOnly;

		// INIT_PARAMETRIC_OBJECT
		// This initialization function is called when this Generator's AXParametric object 
		// is first created; for exampe, when its icon is clicked in the sidebar node menu.
		// It creates the default parameters that will appear in the node. 
		// Often there is at least one input and one output parameter. 
		// Parameters of type Float, Int and Bool that are created here will be available
		// to your generate() function. If no AXParameterType is specified, the type will be GeometryControl.

		public override void init_parametricObject() 
		{
			// Init parameters for the node
			base.init_parametricObject();

			// INPUT AND OUTPUT
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Input, "Input Mesh"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Output,"Output Mesh"));

			// GEOMETRY_CONTROLS
			parametricObject.addParameter(AXParameter.DataType.Float, 	"Amount", 5f);
			parametricObject.addParameter(AXParameter.DataType.Int, 	"Octaves", 2);
			parametricObject.addParameter(AXParameter.DataType.Float, 	"Frequency", 1f);
			parametricObject.addParameter(AXParameter.DataType.Float, 	"Persistence", .5f);
			parametricObject.addParameter(AXParameter.DataType.Float, 	"Lacunarity", 1.5f);

			parametricObject.addParameter(AXParameter.DataType.Bool, 	"FromCenter");
			parametricObject.addParameter(AXParameter.DataType.Float, 	"CenX");
			parametricObject.addParameter(AXParameter.DataType.Float, 	"CenY", 0);
			parametricObject.addParameter(AXParameter.DataType.Float, 	"CenZ");

			parametricObject.addParameter(AXParameter.DataType.Bool, 	"VerticalOnly");



			P_ModifierCurve = parametricObject.addParameter(AXParameter.DataType.AnimationCurve, 	"ModifierCurve");

			Keyframe[] keys = new Keyframe[2];
			keys[0] = new Keyframe(0, 1);
			keys[1] = new Keyframe(10, 1);

			P_ModifierCurve.animationCurve = new AnimationCurve(keys);

		}




		// POLL INPUTS (only on graph structure change())
		// This function is an optimization. It is costly to use getParameter(name) every frame,
		// so this function, which is called when scripts are loaded or when there has been a change in the
		// the graph structure. 
		// Input and Output parameters are set to P_Input and P_Output int eh base class call to this function.

		public override void pollInputParmetersAndSetUpLocalReferences()
		{
			base.pollInputParmetersAndSetUpLocalReferences();

			P_Octaves 						= parametricObject.getParameter("Octaves");
			P_Frequency 					= parametricObject.getParameter("Frequency");
			P_Persistence 					= parametricObject.getParameter("Persistence");
			P_Lacunarity 					= parametricObject.getParameter("Lacunarity");
			P_Amount 						= parametricObject.getParameter("Amount", "amount");
			P_FromCenter 					= parametricObject.getParameter("FromCenter", "amount");

			P_CenX 							= parametricObject.getParameter("CenX");
			P_CenY 							= parametricObject.getParameter("CenY");
			P_CenZ 							= parametricObject.getParameter("CenZ");



			P_ModifierCurve 				= parametricObject.getParameter("ModifierCurve");
			P_VerticalOnly 					= parametricObject.getParameter("VerticalOnly");

		}


		// POLL CONTROLS (every model.generate())
		// It is helpful to set the values for parameter variables before generate().
		// These values will be available not only to generate() but also the Handle functions.
		public override void pollControlValuesFromParmeters()
		{
			base.pollControlValuesFromParmeters();



			octaves 		= (P_Octaves  		!= null)  	? P_Octaves.IntVal		: 2;
			frequency 		= (P_Frequency  	!= null)  	? P_Frequency.FloatVal		: 2.0f;
			persistence 	= (P_Persistence  	!= null)  	? P_Persistence.FloatVal	: 0.5f;
			lacunarity 		= (P_Lacunarity  	!= null)  	? P_Lacunarity.FloatVal		: 1.5f;
			amount 			= (P_Amount  		!= null)  	? P_Amount.FloatVal			: 0.0f;

			cenX 			= (P_CenX  != null)  	? P_CenX.FloatVal	: 0.0f;
			cenY 			= (P_CenY  != null)  	? P_CenY.FloatVal	: 0.0f;
			cenZ 			= (P_CenZ  != null)  	? P_CenZ.FloatVal	: 0.0f;

			center.x = cenX;
			center.y = cenY;
			center.z = cenZ;



			if (P_ModifierCurve != null)
				modifierCurve = P_ModifierCurve.animationCurve;

			fromCenter 		= (P_FromCenter	!= null) 	? P_FromCenter.boolval	: false;
			verticalOnly 	= (P_VerticalOnly	!= null) 	? P_VerticalOnly.boolval	: false;
		}
		 

		// GENERATE
		// This is the main function for generating a Shape, Mesh, or any other output that a node may be tasked with generating.
		// You can do pre processing of Inputs here, but that might best be done in an override of pollControlValuesFromParmeters().
		// 
		// Often, you will be creating a list of AXMesh objects. AXMesh has a Mesh, a Matrix4x4 and a Material 
		// to be used when drawing the ouput to the scene before creating GameObjects. Your list of AXMeshes generated 
		// is pased to the model via parametricObject.finishMultiAXMeshAndOutput().
		//
		// When generate() is called by AX, it will be either with makeGameObjects set to true or false. 
		// makeGameObjects=False is passed when the user is dragging a parameter and the model is regenerateing multiple times per second. 
		// makeGameObjects=True is passed when the user has stopped editing (for example, OnMouseup from a Handle) and it is time to 
		// build a GameObject hierarchy.

		public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool isReplica)
		{	
				
			if (parametricObject == null  || ! parametricObject.hasInputMeshReady("Input Mesh"))
				return null;

			// At this point the amount variable has been set to the FloatValue of its amount parameter.
			preGenerate();


			// AXMeshes are the key product of a Generator3D
			List<AXMesh> 		ax_meshes 	= new List<AXMesh>();

			AXParameter 		inputSrc_p  = P_Input.DependsOn;
			AXParametricObject 	inputSrc_po = inputSrc_p.parametricObject;

			Perlin perlin = new Perlin();
			perlin.Frequency = frequency;
			perlin.OctaveCount = octaves;
			perlin.Persistence = persistence;
			perlin.Lacunarity = lacunarity;



			if ( inputSrc_p != null && inputSrc_p.meshes != null) // Is there an input node connected to this node?
			{

				//Debug.Log("P_Input.DependsOn.meshes.Count="+P_Input.DependsOn.meshes.Count + ", "+ P_Input.DependsOn.meshes[0].GetType());
				AXMesh 		amesh = null;
				for (int i = 0; i < P_Input.DependsOn.meshes.Count; i++) {

					if (amount == 0)
					{
						amesh 	= P_Input.DependsOn.meshes [i];
					}
					else
					{
						// Instance does not inhereit and build on the transform of its source object.
						amesh 	= P_Input.DependsOn.meshes [i].Clone();
						Mesh 		m 		= amesh.mesh;
						Vector3[] 	verts 	= m.vertices; 

						for (int j = 0; j < verts.Length; j++) {

							Vector3 vert = new Vector3(verts[j].x, verts[j].y, verts[j].z);


							Vector3 ray = (vert-center);

							float mag = ray.magnitude;

							vert = P_Input.DependsOn.meshes[i].drawMeshMatrix.MultiplyPoint3x4(vert);



							//float len = Mathf.Sqrt(vert.x*vert.x + vert.z*vert.z);

							float pval 		= (float) perlin.GetValue(vert);
							float curveScaleVal 	= (modifierCurve != null) ? modifierCurve.Evaluate( vert.y) : 1;

							float pang = pval*6*100 ;
							pang *= (modifierCurve != null) ? modifierCurve.Evaluate( vert.y) * curveScaleVal : 1;





							if (fromCenter)
							{
								vert = vert + (ray.normalized * pval * amount * .1f);//(amount*.5f));

							}
							else if (verticalOnly)
							{
								vert.y += pval * curveScaleVal * -amount;
							}
							else
							{
								Vector3 tilter = Quaternion.Euler(pang, pang, pang) * vert.normalized * amount/100;
								vert += tilter;
							}


							verts [j] = P_Input.DependsOn.meshes[i].drawMeshMatrix.inverse.MultiplyPoint3x4(vert);
						}
						m.vertices = verts;
					}
					ax_meshes.Add (amesh);
				}

				// Assume the noise is not so great that it significantly changes the bounds...
				parametricObject.boundsMesh		= inputSrc_po.boundsMesh;
				parametricObject.bounds 		= inputSrc_po.bounds;

				parametricObject.finishMultiAXMeshAndOutput(ax_meshes, isReplica);


				if (makeGameObjects)
					return parametricObject.makeGameObjectsFromAXMeshes(ax_meshes);
			}

			return null;

		}


	}

}
