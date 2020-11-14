using UnityEngine;

using System;
using System.Collections.Generic;

using AXClipperLib;
using Path = System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;


using LibNoise.Unity;
using LibNoise.Unity.Generator;

using AXGeometry;
using AX;

namespace AX.Generators 
{
	/*	CustomNode
	 *  This is a template for an AX Generator
	 *  To generate Mesh output, it is best to subclass Generator3D
	 */
	public class PlanDeformer : Deformer
	{
		public override string GeneratorHandlerTypeName { get { return "PlanDeformerHandler"; } }


		// INPUTS
		public AXParameter 			P_Plan;
		public AXParameter 			planSrc_p;
		public AXParametricObject 	planSrc_po;




		// INPUTS
		// It is nice to have a local variable for parameters set at initialization
		// so that an expensive lookup for the parameter in a Dictionary does not have to be done every frame.
		public AXParameter	P_Turn;
		public AXParameter	P_Twist;
		public AXParameter	P_ModifierCurve;


		// WORKING FIELDS (Updated every Generate)
		// As a best practice, each parameter value could have a local variable
		// That is set before the generate funtion is called.
		// This will allow Handles to acces the parameter values more efficiently.
		public float 		turn;
		public float 		twist;
		public AnimationCurve modifierCurve;




		public Paths				planPaths;
		public Spline[]				planSplines;


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
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, 		AXParameter.ParameterType.Input, "Plan"));


			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Input, "Input Mesh"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Output,"Output Mesh"));

			// GEOMETRY_CONTROLS
			parametricObject.addParameter(AXParameter.DataType.Float, 	"Turn");
			parametricObject.addParameter(AXParameter.DataType.Float, 	"Twist");

			P_ModifierCurve = parametricObject.addParameter(AXParameter.DataType.AnimationCurve, 	"ModifierCurve");

			Keyframe[] keys = new Keyframe[2];
			keys[0] 		= new Keyframe(  0f,  0f);
			keys[1] 		= new Keyframe(100f,  0f);

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

			P_Plan 							= parametricObject.getParameter("Plan");


			P_Turn 							= parametricObject.getParameter("Turn");
			P_Twist 						= parametricObject.getParameter("Twist");
			P_ModifierCurve 				= parametricObject.getParameter("ModifierCurve");


		}


		// POLL CONTROLS (every model.generate())
		// It is helpful to set the values for parameter variables before generate().
		// These values will be available not only to generate() but also the Handle functions.
		public override void pollControlValuesFromParmeters()
		{
			base.pollControlValuesFromParmeters();

			turn 			= (P_Turn  != null)  	? P_Turn.FloatVal	: 0.0f;
			twist 			= (P_Twist  != null)  	? P_Twist.FloatVal	: 0.0f;

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



	
			planSrc_p		= P_Plan.DependsOn;// getUpstreamSourceParameter(P_Plan);
			planSrc_po 		= (planSrc_p != null) 								? planSrc_p.parametricObject 	: null;

			if (planSrc_p == null)
				return null;

			//Debug.Log("planSrc_po = " + planSrc_po.Name+"."+planSrc_p.Name + " ... " + planSrc_p.DependsOn.PType + " ..... " + planSrc_p.getPaths());

			P_Plan.polyTree = null;
			AXShape.thickenAndOffset(ref P_Plan, planSrc_p);
			if (P_Plan.reverse)
				P_Plan.doReverse();


			planPaths = P_Plan.getPaths();

			if (planPaths == null || planPaths.Count == 0)
				return null;
			 

			// ** CREATE PLAN_SPLINES **

			if (planPaths != null && planPaths.Count > 0)
			{
				planSplines = new Spline[planPaths.Count];

				if (planSplines != null)
				{
					for (int i=0; i<planSplines.Length; i++)
						planSplines[i] = new Spline(planPaths[i], (P_Plan.shapeState == ShapeState.Closed) ? true : false);
				}
			}




			// FOR EACH PATH
			for (int path_i=0; path_i<planPaths.Count; path_i++)
			{

				Spline planSpline = planSplines[path_i];

				planSpline.breakAngleCorners = 35;
				planSpline.shapeState = P_Plan.shapeState;


				//Matrix4x4 mm = planSpline.matrixAtLength(4.5f);

			}











			if (parametricObject == null  || ! parametricObject.hasInputMeshReady("Input Mesh"))
				return null;

			// At this point the amount variable has been set to the FloatValue of its amount parameter.
			preGenerate();



		

			//Vector3 center = Vector3.zero;

			// AXMeshes are the key product of a Generator3D
			List<AXMesh> 		ax_meshes 	= new List<AXMesh>();

			AXParameter 		inputSrc_p  = P_Input.DependsOn;
			AXParametricObject 	inputSrc_po = inputSrc_p.parametricObject;

			Spline planSpl = planSplines[0];


			if ( inputSrc_p != null && inputSrc_p.meshes != null) // Is there an input node connected to this node?
			{

				//Debug.Log("P_Input.DependsOn.meshes.Count="+P_Input.DependsOn.meshes.Count + ", "+ P_Input.DependsOn.meshes[0].GetType());
				AXMesh 		amesh = null;

				// EACH MESH
				for (int i = 0; i < P_Input.DependsOn.meshes.Count; i++) {

//					if (amount == 0)
//					{
//						amesh 	= P_Input.DependsOn.meshes [i];
//					}
//					else{
						// Instance does not inhereit and build on the transform of its source object.
						amesh 	= P_Input.DependsOn.meshes [i].Clone();
						Mesh 		m 		= amesh.mesh;
						Vector3[] 	verts 	= m.vertices; 


						// ---------- EACH VERTEX ----------------------
						for (int j = 0; j < verts.Length; j++) {

							Vector3 vert = new Vector3(verts[j].x, verts[j].y, verts[j].z);

							vert = P_Input.DependsOn.meshes[i].drawMeshMatrix.MultiplyPoint3x4(vert);


							// THE SPLINE POSITION MATRIX
							Matrix4x4 warperM = planSpl.matrixAtLength(vert.x);

							// SCALE
							float scale = modifierCurve.Evaluate( vert.x );

							// TWIST
							if (turn != 0 || twist != 0 || scale != 0)
								warperM *= Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(vert.x*twist+turn+scale, 0, 0), Vector3.one);

							vert =  warperM.MultiplyPoint3x4(new Vector3(0, vert.y, vert.z)); ;

							verts [j] = P_Input.DependsOn.meshes[i].drawMeshMatrix.inverse.MultiplyPoint3x4(vert);
						}
						// ---------- EACH VERTEX ----------------------


						m.vertices = verts;
					//}

					amesh.mesh.RecalculateNormals();
					ax_meshes.Add (amesh);
				}

				setBoundaryFromAXMeshes(ax_meshes);
				//parametricObject.bounds = inputSrc_po.bounds;
				parametricObject.finishMultiAXMeshAndOutput(ax_meshes, isReplica);


				if (makeGameObjects)
					return parametricObject.makeGameObjectsFromAXMeshes(ax_meshes);
			}

			return null;

		}







		public override Matrix4x4 getLocalConsumerMatrixPerInputSocket(AXParametricObject input_po, AXParameter input_P)
		{
			// use shift too
			// use caller address
			if (input_po == null)
				return Matrix4x4.identity;

			if (input_P != null)
			{
				if (input_P.Name == "Plan")
					return  Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90, 0, 0), Vector3.one);

			}


			// PLAN
			if (input_po == planSrc_po)
				return  Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90, 0, 0), Vector3.one);

			return Matrix4x4.identity;

		}


	}

}
