using UnityEngine;

using System;
using System.Collections.Generic;

using LibNoise.Unity;
using LibNoise.Unity.Generator;

namespace AX.Generators 
{
	/*	CustomNode
	 *  This is a template for an AX Generator
	 *  To generate Mesh output, it is best to subclass Generator3D
	 */
	public class ShearDeformer : Deformer
	{

		// INPUTS
		// It is nice to have a local variable for parameters set at initialization
		// so that an expensive lookup for the parameter in a Dictionary does not have to be done every frame.
		public AXParameter	P_Amount;

		public AXParameter	P_ShearX;
		public AXParameter	P_ShearZ;


		public AXParameter	P_ModifierCurve;


		// WORKING FIELDS (Updated every Generate)
		// As a best practice, each parameter value could have a local variable
		// That is set before the generate funtion is called.
		// This will allow Handles to acces the parameter values more efficiently.
		public float 		amount;
		public AnimationCurve modifierCurve;

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
			parametricObject.addParameter(AXParameter.DataType.Float, 	"Amount");

			P_ModifierCurve = parametricObject.addParameter(AXParameter.DataType.AnimationCurve, 	"ModifierCurve");

			Keyframe[] keys = new Keyframe[2];
			keys[0] = new Keyframe( 0f, 0f);
			keys[1] = new Keyframe(10f, 0f);

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



			P_Amount 						= parametricObject.getParameter("Amount", "amount");
			P_ModifierCurve 				= parametricObject.getParameter("ModifierCurve");


		}


		// POLL CONTROLS (every model.generate())
		// It is helpful to set the values for parameter variables before generate().
		// These values will be available not only to generate() but also the Handle functions.
		public override void pollControlValuesFromParmeters()
		{
			base.pollControlValuesFromParmeters();

			amount 			= (P_Amount  != null)  	? P_Amount.FloatVal	: 0.0f;

			if (P_ModifierCurve != null)
				modifierCurve = P_ModifierCurve.animationCurve;
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


			//Vector3 center = Vector3.zero;

			// AXMeshes are the key product of a Generator3D
			List<AXMesh> 		ax_meshes 	= new List<AXMesh>();

			AXParameter 		inputSrc_p  = P_Input.DependsOn;
			AXParametricObject 	inputSrc_po = inputSrc_p.parametricObject;



			if ( inputSrc_p != null && inputSrc_p.meshes != null) // Is there an input node connected to this node?
			{

				//Debug.Log("P_Input.DependsOn.meshes.Count="+P_Input.DependsOn.meshes.Count + ", "+ P_Input.DependsOn.meshes[0].GetType());
				AXMesh 		amesh = null;


				// BOUNDING

				//CombineInstance[] boundsCombinator = new CombineInstance[ P_Input.DependsOn.meshes.Count];



				// EACH MESH
				for (int i = 0; i < P_Input.DependsOn.meshes.Count; i++) {

					// Instance does not inhereit and build on the transform of its source object.
					amesh 	= P_Input.DependsOn.meshes [i].Clone();
					Mesh 		m 				= amesh.mesh;
					Vector3[] 	verts 			= m.vertices; 


					// ---------- EACH VERTEX ----------------------
					for (int j = 0; j < verts.Length; j++) {

						Vector3 vert = new Vector3(verts[j].x, verts[j].y, verts[j].z);

						vert = P_Input.DependsOn.meshes[i].drawMeshMatrix.MultiplyPoint3x4(vert);

						float scale = modifierCurve.Evaluate( vert.x);


						vert = new Vector3(vert.x, (vert.y+amount*vert.x)+scale, vert.z);
					
						verts [j] = P_Input.DependsOn.meshes[i].drawMeshMatrix.inverse.MultiplyPoint3x4(vert);
					}
					// ---------- EACH VERTEX ----------------------

					m.vertices = verts;

					amesh.mesh.RecalculateNormals();
					ax_meshes.Add (amesh);
					  

				}


				parametricObject.boundsMesh		=  ArchimatixUtils.meshClone( inputSrc_po.boundsMesh ) ;

                if (parametricObject.boundsMesh != null)
                {
                    Vector3[] boundsVerts = parametricObject.boundsMesh.vertices;

                    // ---------- EACH VERTEX ----------------------
                    for (int j = 0; j < boundsVerts.Length; j++)
                    {

                        Vector3 vert = new Vector3(boundsVerts[j].x, boundsVerts[j].y, boundsVerts[j].z);

                        //vert = P_Input.DependsOn.meshes[i].drawMeshMatrix.MultiplyPoint3x4(vert);

                        float scale = modifierCurve.Evaluate(vert.x);

                        boundsVerts[j] = new Vector3(vert.x, (vert.y + amount * vert.x) + scale, vert.z);

                        //boundsVerts [j] = P_Input.DependsOn.meshes[i].drawMeshMatrix.inverse.MultiplyPoint3x4(vert);
                    }
                    // ---------- EACH VERTEX ----------------------


                    parametricObject.boundsMesh.vertices = boundsVerts;
                    parametricObject.boundsMesh.RecalculateBounds();

                    //parametricObject.bounds = inputSrc_po.bounds;
                    parametricObject.bounds = parametricObject.boundsMesh.bounds;

                }

                parametricObject.finishMultiAXMeshAndOutput(ax_meshes, isReplica);


				if (makeGameObjects)
					return parametricObject.makeGameObjectsFromAXMeshes(ax_meshes);
			}

			return null;

		}


	}

}
