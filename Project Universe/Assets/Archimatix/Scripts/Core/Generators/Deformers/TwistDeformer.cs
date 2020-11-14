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
	public class TwistDeformer : Deformer
	{

		// INPUTS
		// It is nice to have a local variable for parameters set at initialization
		// so that an expensive lookup for the parameter in a Dictionary does not have to be done every frame.
		public AXParameter	P_Twist_X;
		public AXParameter	P_Twist_Y;
		public AXParameter	P_Twist_Z;
		public AXParameter	P_Bend_X;
		public AXParameter	P_Bend_Y;
		public AXParameter	P_Bend_Z;

		public AXParameter	P_Amount;


		// WORKING FIELDS (Updated every Generate)
		// As a best practice, each parameter value could have a local variable
		// That is set before the generate funtion is called.
		// This will allow Handles to acces the parameter values more efficiently.
		public float 		twistX;
		public float 		twistY;
		public float 		twistZ;
		public float 		bendX;
		public float 		bendY;
		public float 		bendZ;
		public float 		amount = 1;


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
			parametricObject.addParameter(AXParameter.DataType.Float, 	"Twist_X");
			parametricObject.addParameter(AXParameter.DataType.Float, 	"Twist_Y");
			parametricObject.addParameter(AXParameter.DataType.Float, 	"Twist_Z");
			parametricObject.addParameter(AXParameter.DataType.Float, 	"Bend_X");
			parametricObject.addParameter(AXParameter.DataType.Float, 	"Bend_Y");
			parametricObject.addParameter(AXParameter.DataType.Float, 	"Bend_Z");
			parametricObject.addParameter(AXParameter.DataType.Float, 	"Amount", 1f);
		}




		// POLL INPUTS (only on graph structure change())
		// This function is an optimization. It is costly to use getParameter(name) every frame,
		// so this function, which is called when scripts are loaded or when there has been a change in the
		// the graph structure. 
		// Input and Output parameters are set to P_Input and P_Output int eh base class call to this function.

		public override void pollInputParmetersAndSetUpLocalReferences()
		{
			base.pollInputParmetersAndSetUpLocalReferences();

			P_Twist_X 						= parametricObject.getParameter("Twist_X");
			P_Twist_Y 						= parametricObject.getParameter("Twist_Y");
			P_Twist_Z 						= parametricObject.getParameter("Twist_Z");
			P_Bend_X 						= parametricObject.getParameter("Bend_X");
			P_Bend_Y 						= parametricObject.getParameter("Bend_Y");
			P_Bend_Z 						= parametricObject.getParameter("Bend_Z");
			P_Amount 						= parametricObject.getParameter("Amount", "amount");
		}


		// POLL CONTROLS (every model.generate())
		// It is helpful to set the values for parameter variables before generate().
		// These values will be available not only to generate() but also the Handle functions.
		public override void pollControlValuesFromParmeters()
		{
			base.pollControlValuesFromParmeters();

			twistX 			= (P_Twist_X  != null)  	? P_Twist_X.FloatVal	: 0.0f;
			twistY 			= (P_Twist_Y  != null)  	? P_Twist_Y.FloatVal	: 0.0f;
			twistZ 			= (P_Twist_Z  != null)  	? P_Twist_Z.FloatVal	: 0.0f;
			bendX 			= (P_Bend_X  != null)  		? P_Bend_X.FloatVal	: 0.0f;
			bendY 			= (P_Bend_Y  != null)  		? P_Bend_Y.FloatVal	: 0.0f;
			bendZ 			= (P_Bend_Z  != null)  		? P_Bend_Z.FloatVal	: 0.0f;
			amount 			= (P_Amount  != null)  		? P_Amount.FloatVal	: 1.0f;

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
				//Debug.Log("P_Input.DependsOn.meshes.Count="+P_Input.DependsOn.meshes.Count);


				AXMesh 		amesh = null;




				// EACH MESH
				for (int i = 0; i < P_Input.DependsOn.meshes.Count; i++) {

					if (amount == 0)
					{
						amesh 	= P_Input.DependsOn.meshes [i];
					}
					else{
						// Instance does not inhereit and build on the transform of its source object.
						amesh 	= P_Input.DependsOn.meshes [i].Clone();
						Mesh 		m 		= amesh.mesh;
						Vector3[] 	verts 	= m.vertices; 


						// ---------- EACH VERTEX ----------------------
						for (int j = 0; j < verts.Length; j++) {

							Vector3 vert = new Vector3(verts[j].x, verts[j].y, verts[j].z);

							vert = P_Input.DependsOn.meshes[i].drawMeshMatrix.MultiplyPoint3x4(vert);

							float pangx = vert.x * (1+amount);
							float pangy = vert.y * (1+amount);
							float pangz = vert.z * (1+amount);


							vert = Quaternion.Euler(bendX*pangy, twistY*pangy, bendZ*pangy) * Quaternion.Euler(twistX*pangx, bendY*pangx, bendZ*pangx) * Quaternion.Euler(bendX*pangz, bendY*pangz, twistZ*pangz) * vert;

//							float len = Mathf.Sqrt(vert.x*vert.x + vert.z*vert.z);
//
//							float new_x = vert.x;//+ pval;
//							float new_y = vert.y  -  factor*len*len*vert.y/5;// + pval;
//							float new_z = vert.z;//+ pval;
//
//							//Debug.Log(pval + " :::: " +  verts[j].x + " :: " + verts[j].z + " + " + Mathf.PerlinNoise (verts[j].x, verts[j].z));
//							verts [j] = new Vector3 (new_x, new_y, new_z);
							verts [j] = P_Input.DependsOn.meshes[i].drawMeshMatrix.inverse.MultiplyPoint3x4(vert);
						}
						// ---------- EACH VERTEX ----------------------


						m.vertices = verts;
					}
                    amesh.mesh.RecalculateNormals();
                    ax_meshes.Add (amesh);
				}

                //parametricObject.bounds = inputSrc_po.bounds;

                parametricObject.boundsMesh = ArchimatixUtils.meshClone(inputSrc_po.boundsMesh);

                if (parametricObject.boundsMesh != null)
                {
                    Vector3[] boundsVerts = parametricObject.boundsMesh.vertices;

                    // ---------- EACH VERTEX ----------------------
                    for (int j = 0; j < boundsVerts.Length; j++)
                    {

                        Vector3 vert = new Vector3(boundsVerts[j].x, boundsVerts[j].y, boundsVerts[j].z);

                        //vert = P_Input.DependsOn.meshes[i].drawMeshMatrix.MultiplyPoint3x4(vert);

                        float pangx = vert.x * (1 + amount);
                        float pangy = vert.y * (1 + amount);
                        float pangz = vert.z * (1 + amount);


                        vert = Quaternion.Euler(bendX * pangy, twistY * pangy, bendZ * pangy) * Quaternion.Euler(twistX * pangx, bendY * pangx, bendZ * pangx) * Quaternion.Euler(bendX * pangz, bendY * pangz, twistZ * pangz) * vert;

                        boundsVerts[j] = vert;

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
