using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using LibNoise.Unity;
using LibNoise.Unity.Generator;
using Perlin = LibNoise.Unity.Generator.Perlin;


using AXClipperLib;
using Path = System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;

using AXPoly2Tri;
using PolygonPoints = System.Collections.Generic.List<AXPoly2Tri.PolygonPoint>;

using AXGeometry;


namespace AX.Generators
{


	public interface IReplicator
	{
	}

	
	// REPLICATOR
	
	public class Replicator : Generator3D, IReplicator
	{
		public override string GeneratorHandlerTypeName { get { return "ReplicatorHandler"; } }


		// INPUTS
		public AXParameter 			P_Plan;
		public AXParameter 			planSrc_p;
		public AXParametricObject 	planSrc_po;


		// PROTOTYPE
		public AXParameter 			P_PrototypePlan;
		public AXParameter 			prototypePlanSrc_p;
		public AXParametricObject 	prototypePlanSrc_po;

		// NODE
		public AXParameter			P_Prototype;
		public AXParameter 			prototypeSrc_p;
		public AXParametricObject 	prototypeSrc_po;

		public Paths				planPaths;
		public Spline[]				planSplines;





		// INIT_PARAMETRIC_OBJECT
		public override void init_parametricObject() 
		{
			//SHAPES
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, 		AXParameter.ParameterType.Input, "Plan"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, 		AXParameter.ParameterType.Input, "PrototypePlan"));

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, 		AXParameter.ParameterType.Input, "Prototype"));

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Output,"Output Mesh"));



			base.init_parametricObject();
		}



		// POLL INPUTS (only on graph change())
		public override void pollInputParmetersAndSetUpLocalReferences()
		{
			base.pollInputParmetersAndSetUpLocalReferences();

			P_Plan 					= parametricObject.getParameter("Plan");

			P_PrototypePlan 				= parametricObject.getParameter("PrototypePlan");
			P_Prototype 					= parametricObject.getParameter("Prototype");

		}




		// POLL CONTROLS (every model.generate())
		public override void pollControlValuesFromParmeters()
		{
			base.pollControlValuesFromParmeters();

			prototypeSrc_p 		= (P_Prototype 		!= null) 	? getUpstreamSourceParameter(P_Prototype)		: null;
			prototypeSrc_po 		= (prototypeSrc_p	!= null) 	? prototypeSrc_p.parametricObject				: null;

		}



		public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool isReplica)
		{

			if (parametricObject == null || ! parametricObject.isActive)
				return null;

			if (P_PrototypePlan == null || P_Prototype == null)
				return null;

				 

			preGenerate();
			 
			 
			// PLAN			  
			planSrc_p		= P_Plan.DependsOn;// getUpstreamSourceParameter(P_Plan);
			planSrc_po 		= (planSrc_p != null) 								? planSrc_p.parametricObject 	: null;

			//Pather.printPaths(planSrc_p.getPaths());

			P_Plan.polyTree = null;
			AXShape.thickenAndOffset(ref P_Plan, planSrc_p);

			planPaths = P_Plan.getPaths();

			if (planPaths == null || planPaths.Count == 0)
				return null;
			


			

			prototypePlanSrc_p 	= P_PrototypePlan.DependsOn;
			prototypePlanSrc_po = (prototypePlanSrc_p != null) ? prototypePlanSrc_p.parametricObject 	: null;

			prototypeSrc_p 	= P_Prototype.DependsOn;
			prototypePlanSrc_po = (prototypeSrc_p != null) ? prototypeSrc_p.parametricObject 	: null;


			if (prototypePlanSrc_p == null || prototypePlanSrc_po == null || prototypeSrc_po == null)
				return null;

			AXParameter srcSrc_p = prototypePlanSrc_p;
			if (srcSrc_p == null)
				return null;



			// AX_MESHES
			List<AXMesh> 	ax_meshes 		= new List<AXMesh>();
			 

			GameObject go 			= null;
			if (makeGameObjects && ! parametricObject.combineMeshes)
				go = ArchimatixUtils.createAXGameObject(parametricObject.Name, parametricObject);


			Perlin perlin = new Perlin();
			perlin.OctaveCount = 1;
			perlin.Frequency = .05f;

			GameObject replicant = null;

			//Debug.Log("planPaths.Count="+ planPaths.Count);



			AXParameter P_Height = prototypeSrc_po.getParameter("Height", "SizeY");


			foreach (Path plan in planPaths)
			{
				

				// 1. cache source object
				//prototypeSrc_po.cacheParameterValues();

				Paths tmpPaths = new Paths();
				tmpPaths.Add(plan);

				IntPoint planCenter = AXGeometry.Utilities.getCenter(tmpPaths);
				Vector3 centerPt 	= AXGeometry.Utilities.IntPt2Vec3(planCenter);

				//Debug.Log("Center: " + centerPt);
				 
				srcSrc_p.paths = tmpPaths;



				float area = ((float) Clipper.Area(plan)) / 1000000000;

				//Debug.Log(area);
				float perlinVal =  (float) perlin.GetValue(centerPt);
				 


				float h = 3 + 100 * (float) Math.Exp(-(.1f*area)) + 5*perlinVal;// (float) perlin.GetValue(2,3,4);


				P_Height.initiatePARAMETER_Ripple_setFloatValueFromGUIChange( h );

				prototypeSrc_po.generator.pollControlValuesFromParmeters();
				prototypeSrc_po.isAltered = true;

				//Debug.Log("generate REP of "+prototypeSrc_po.Name);
				 
				foreach(AXParameter d in prototypePlanSrc_p.Dependents)
					d.parametricObject.setAltered();

				// ** GENERATE REPLICANT ** //
				replicant = prototypeSrc_po.generateOutputNow (makeGameObjects, parametricObject, false);

				//Debug.Log(replicant);

				if (replicant != null)
					replicant.transform.parent = go.transform;

				AXParameter output_p = prototypeSrc_po.getParameter("Output Mesh");
				foreach (AXMesh amesh in output_p.meshes)
					ax_meshes.Add (amesh.Clone(amesh.transMatrix));

			}


			// FINISH AX_MESHES

			parametricObject.finishMultiAXMeshAndOutput(ax_meshes, isReplica);


			if (makeGameObjects)
				return go;



			return null;
		}





		// GET_LOCAL_CONSUMER_MATRIX_PER_INPUT_SOCKET
		public override Matrix4x4 getLocalConsumerMatrixPerInputSocket(AXParametricObject input_po)
		{
			//Debug.Log ("input_po.Name=" + input_po.Name + " :: planSrc_po.Name=" + planSrc_po.Name);// + " -- " + endCapHandleTransform);
			// PLAN
			if (input_po == planSrc_po)
			{
				if (P_Plan.flipX)
						return Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90, 0, 0), new Vector3(-1,1,1));
					else
						return Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90, 0, 0), Vector3.one);

				
			}
							
			return Matrix4x4.identity ;
		}

	}
}
