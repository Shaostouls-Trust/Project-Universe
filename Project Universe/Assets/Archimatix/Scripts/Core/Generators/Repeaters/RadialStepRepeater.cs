using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;


using AXClipperLib;
using Path = System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;

using AXPoly2Tri;
using PolygonPoints = System.Collections.Generic.List<AXPoly2Tri.PolygonPoint>;

using AXGeometry;

using Axis = AXGeometry.Axis;

namespace AX.Generators
{





	// GRID_REPEATER

	public class RadialStepRepeater : RepeaterBase, IRepeater
	{
		public override string GeneratorHandlerTypeName { get { return "RadialStepRepeaterHandler"; } }


		// INPUTS

		public AXParameter P_Angle;
		public AXParameter P_Radius;
		public AXParameter P_Height;
		public AXParameter P_Riser;
		public AXParameter P_Steps;
		public AXParameter P_Actual_Riser;
		public AXParameter P_Actual_Step_Angle;

		public AXParameter P_TopStep;
		public AXParameter P_AlternateSteps;


		// OUTPUTS
		public AXParameter P_Stair_Profile;




		// WORKING FIELDS (Updated every Generate)

		public float angle;
		public float radius;
		public float height;
		public float riser;
		public float tread;
		public int 	 steps;
		public float actual_riser;
		public float actual_stepAngle;


		public bool topStep;
		public bool alternateSteps;


		public Path stairProfile;



		// INIT_PARAMETRIC_OBJECT
		public override void init_parametricObject() 
		{
			base.init_parametricObject();

			// INPUT MESH
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Input, "Node Mesh"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Input, "Cell Mesh"));

			// JITTER
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.JitterTool,   AXParameter.ParameterType.Input, "Jitter Translation"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.JitterTool,   AXParameter.ParameterType.Input, "Jitter Rotation"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.JitterTool,   AXParameter.ParameterType.Input, "Jitter Scale"));





			// ANGLE
			P_Angle = parametricObject.addParameter(AXParameter.DataType.Float, 			"Angle", 		270f);
			P_Angle.expressions.Add ("Actual_Step_Angle=Angle/Steps");

			// RADIUS
			P_Radius = parametricObject.addParameter(AXParameter.DataType.Float, 			"Radius", 			0f);


			// HEIGHT
			P_Height = parametricObject.addParameter(AXParameter.DataType.Float, 				"Height", 		3f);
			P_Height.expressions.Add ("Steps=Height/Riser");
			P_Height.expressions.Add ("Actual_Riser=Height/Steps");

			P_Height.min = .01f;

			// RISER
			P_Riser = parametricObject.addParameter(AXParameter.DataType.Float, 			"Riser", 		.25f, .01f, 100);
			//P_Riser.expressions.Add ("Rise=Riser*Steps");
			P_Riser.expressions.Add ("Steps=Height/Riser");
			P_Riser.expressions.Add ("Actual_Riser=Height/Steps");


			// STEPS
			P_Steps =parametricObject.addParameter(AXParameter.DataType.Int, 				"Steps", 		12);
			P_Steps.expressions.Add ("Actual_Riser=Height/Steps");
			P_Steps.expressions.Add ("Riser=Actual_Riser");
			P_Steps.expressions.Add ("Actual_Step_Angle=Angle/Steps");
			P_Steps.intmin = 1;

			// ACTUAL_RISER
			P_Actual_Riser = parametricObject.addParameter(AXParameter.DataType.Float,  		"Actual_Riser", .25f);
			P_Actual_Riser.expressions.Add ("Height=Actual_Riser*Steps");

			// ACTUAL_STEP_ANGLE
			P_Actual_Step_Angle = parametricObject.addParameter(AXParameter.DataType.Float,  	"Actual_Step_Angle", 22.5f);


			P_zAxis = parametricObject.addParameter(AXParameter.DataType.Bool, 					"zAxis", false);
			P_TopStep = parametricObject.addParameter(AXParameter.DataType.Bool, 				"Top Step", true);
			P_AlternateSteps = parametricObject.addParameter(AXParameter.DataType.Bool, 		"Alternate Steps", false);

			P_ProgressiveRotationX = parametricObject.addParameter(AXParameter.DataType.Float, 	"IncrRotX", 0f); 
			P_ProgressiveRotationY = parametricObject.addParameter(AXParameter.DataType.Float, 	"IncrRotY", 0f); 
			P_ProgressiveRotationZ = parametricObject.addParameter(AXParameter.DataType.Float, 	"IncrRotZ", 0f); 

			// OUTPUT
			P_Stair_Profile = parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, AXParameter.ParameterType.Output,"Stair Profile"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Output,"Output Mesh"));

		}

		// POLL INPUTS (only on graph change())
		public override void pollInputParmetersAndSetUpLocalReferences()
		{
			//Debug.Log("Repeater::pollInputParmetersAndSetUpLocalReferences " + parametricObject.Name + " parametersHaveBeenPolled="+parametersHaveBeenPolled);

			base.pollInputParmetersAndSetUpLocalReferences();


			P_Angle 				= parametricObject.getParameter("Angle");
			P_Radius 				= parametricObject.getParameter("Radius");
			P_Height 					= parametricObject.getParameter("Run"); 
			P_Riser 				= parametricObject.getParameter("Riser"); 
			P_Steps 				= parametricObject.getParameter("Steps"); 
			P_Actual_Riser 			= parametricObject.getParameter("Actual_Riser"); 
			P_Actual_Step_Angle 	= parametricObject.getParameter("Actual_Step_Angle"); 

			if (P_Height != null)
				if (P_Height.FloatVal < .01) P_Height.FloatVal = .01f;

			P_zAxis					= parametricObject.getParameter("zAxis");
			P_TopStep				= parametricObject.getParameter("Top Step");
			P_AlternateSteps		= parametricObject.getParameter("Alternate Steps");

			P_ProgressiveRotationX 	= parametricObject.getParameter("IncrRotX");
			P_ProgressiveRotationY 	= parametricObject.getParameter("IncrRotY");
			P_ProgressiveRotationZ 	= parametricObject.getParameter("IncrRotZ");


			P_Stair_Profile 		= parametricObject.getParameter("Stair Profile");
		}




		// POLL CONTROLS (every model.generate())
		public override void pollControlValuesFromParmeters()
		{
			base.pollControlValuesFromParmeters();

			height = (P_Height != null) ? P_Height.FloatVal : 3;

			if (P_Height != null) {
				P_Height.FloatVal = Math.Max (P_Height.FloatVal, .01f);
				height = P_Height.FloatVal;
			}




			angle 			= (P_Angle != null) ? P_Angle.FloatVal : 0;
			radius 			= (P_Radius != null) 	? P_Radius.FloatVal : height;

			riser 			= P_Riser.FloatVal;
			steps 			= P_Steps.IntVal;

			//actual_riser 	= P_Actual_Riser.FloatVal;

			actual_riser	= validateFloatValueWithMin(P_Actual_Riser, .01f);



			actual_stepAngle 	= (P_Actual_Step_Angle != null) ? P_Actual_Step_Angle.FloatVal : 10;
			actual_stepAngle    = validateFloatValueWithMin(P_Actual_Step_Angle, .01f);


			zAxis 				= P_zAxis.boolval;
			topStep				= P_TopStep.boolval;
			alternateSteps		= P_AlternateSteps.boolval;



			actual_stepAngle = (topStep) ? angle / steps : angle / (steps-1);
			if (P_Actual_Step_Angle != null)
				P_Actual_Step_Angle.FloatVal = actual_stepAngle;

			progressiveRotationX = (P_ProgressiveRotationX != null) ?  P_ProgressiveRotationX.FloatVal : 0;
			progressiveRotationY = (P_ProgressiveRotationX != null) ?  P_ProgressiveRotationY.FloatVal : 0;
			progressiveRotationZ = (P_ProgressiveRotationX != null) ?  P_ProgressiveRotationZ.FloatVal : 0;

			if (P_Stair_Profile != null)
				P_Stair_Profile.axis 	= (zAxis) ? Axis.X : Axis.NZ;


		}











		// GENERATE STEP_REPEATER
		public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool isReplica)
		{

			if (parametricObject == null || ! parametricObject.isActive)
				return null;

			preGenerate();




			// NODE_MESH
			GameObject nodePlugGO 					= null;

			if (nodeSrc_p != null)
			{
				nodeSrc_po 		= nodeSrc_p.parametricObject;
				if (makeGameObjects && ! parametricObject.combineMeshes)
					nodePlugGO = nodeSrc_po.generator.generate(true,  initiator_po,  isReplica);
			}	


			// CELL_MESH
			GameObject cellPlugGO 					= null;

			if (cellSrc_p != null)
			{
				cellSrc_po 		= cellSrc_p.parametricObject;
				if (makeGameObjects && ! parametricObject.combineMeshes)
					cellPlugGO = cellSrc_po.generator.generate(true,  initiator_po,  isReplica);
			}	









			if (nodeSrc_po == null && cellSrc_po == null)
			{
				if (P_Output != null)
					P_Output.meshes = null;

				return null;
			}





			GameObject go 		= null;
			if (makeGameObjects && ! parametricObject.combineMeshes)
				go = ArchimatixUtils.createAXGameObject(parametricObject.Name, parametricObject);




			List<AXMesh> 	ax_meshes 		= new List<AXMesh>();

			Matrix4x4 localPlacement_mx = Matrix4x4.identity;

			// -----------------------------------




			shiftU = 0;
			//shiftU = (topStep) ? (-steps * actual_tread / 2) : (-(steps-1) * actual_tread / 2);


			AXMesh tmpMesh;


			// BOUNDING

			CombineInstance[] boundsCombinator = new CombineInstance[steps];



			// LOOP

			//Debug.Log(nodeSrc_po.Name + ": " + nodeSrc_po.boundsMesh.vertices.Length);
			//AXGeometry.Utilities
			for (int i=0; i<steps; i++) 
			{
				//Debug.Log("["+i+"] i*actualBay="+i*actualBay+", perval="+perlval);


				if (i == steps-1 && ! topStep)
					break;




				// NODES
				if (nodeSrc_po != null && nodeSrc_p.meshes != null)
				{
					string this_address = "node_"+i;


					// LOCAL_PLACEMENT

					localPlacement_mx = localMatrixFromAddress(RepeaterItem.Node, i);




					// AX_MESHES

					for (int mi = 0; mi < nodeSrc_p.meshes.Count; mi++) {
						AXMesh dep_amesh = nodeSrc_p.meshes [mi];

						tmpMesh = dep_amesh.Clone (localPlacement_mx * dep_amesh.transMatrix);
						tmpMesh.subItemAddress = this_address;
						ax_meshes.Add (tmpMesh);
					}




					// BOUNDING MESHES

					boundsCombinator[i].mesh 		= nodeSrc_po.boundsMesh;
					boundsCombinator[i].transform 	= localPlacement_mx * nodeSrc_po.generator.localMatrixWithAxisRotationAndAlignment;



					// GAME_OBJECTS

					if (nodePlugGO != null  && makeGameObjects && ! parametricObject.combineMeshes)
					{
						//Matrix4x4 mx = localPlacement_mx  * parametricObject.getTransMatrix() * source.getTransMatrix();

						//Debug.Log(nodeSrc_po.getLocalMatrix());
						Matrix4x4 mx =   localPlacement_mx * nodeSrc_po.generator.localMatrixWithAxisRotationAndAlignment;
						GameObject copyGO = (GameObject) GameObject.Instantiate(nodePlugGO, AXUtilities.GetPosition(mx), AXUtilities.QuaternionFromMatrix(mx));

						Vector3 newJitterScale = jitterScale + Vector3.one;
						copyGO.transform.localScale= new Vector3(copyGO.transform.localScale.x*newJitterScale.x, copyGO.transform.localScale.y*newJitterScale.y, copyGO.transform.localScale.z*newJitterScale.z);
						copyGO.transform.localScale += jitterScale;


						#if UNITY_EDITOR
						//if (parametricObject.model.isSelected(nodeSrc_po) && nodeSrc_po.selectedConsumerAddress == this_address)
						//	Selection.activeGameObject = copyGO;
						#endif


						AXGameObject axgo = copyGO.GetComponent<AXGameObject>();
						if (axgo != null)
							axgo.consumerAddress = this_address;

						copyGO.name = copyGO.name+"_" + this_address;
						copyGO.transform.parent = go.transform;
					}
				} // \NODES





				// CELLS
				if (cellSrc_po != null && cellSrc_p.meshes != null)
				{
					string this_address = "cell_"+i;


					// LOCAL_PLACEMENT

					localPlacement_mx = localMatrixFromAddress(RepeaterItem.Cell, i);


					// AX_MESHES

					for (int mi = 0; mi < cellSrc_p.meshes.Count; mi++) {
						AXMesh dep_amesh = cellSrc_p.meshes [mi];

						tmpMesh = dep_amesh.Clone (localPlacement_mx * dep_amesh.transMatrix);
						tmpMesh.subItemAddress = this_address;
						ax_meshes.Add (tmpMesh);
					}




					// BOUNDING MESHES

					boundsCombinator[i].mesh 		= cellSrc_po.boundsMesh;
					boundsCombinator[i].transform 	= localPlacement_mx * cellSrc_po.generator.localMatrixWithAxisRotationAndAlignment;



					// GAME_OBJECTS

					if (cellPlugGO != null  && makeGameObjects && ! parametricObject.combineMeshes)
					{
						//Matrix4x4 mx = localPlacement_mx  * parametricObject.getTransMatrix() * source.getTransMatrix();

						//Debug.Log(cellSrc_po.getLocalMatrix());
						Matrix4x4 mx =   localPlacement_mx * cellSrc_po.generator.localMatrixWithAxisRotationAndAlignment;
						GameObject copyGO = (GameObject) GameObject.Instantiate(cellPlugGO, AXUtilities.GetPosition(mx), AXUtilities.QuaternionFromMatrix(mx));

						Vector3 newJitterScale = jitterScale + Vector3.one;
						copyGO.transform.localScale= new Vector3(copyGO.transform.localScale.x*newJitterScale.x, copyGO.transform.localScale.y*newJitterScale.y, copyGO.transform.localScale.z*newJitterScale.z);
						copyGO.transform.localScale += jitterScale;


						#if UNITY_EDITOR
						//if (parametricObject.model.isSelected(nodeSrc_po) && nodeSrc_po.selectedConsumerAddress == this_address)
						//	Selection.activeGameObject = copyGO;
						#endif


						AXGameObject axgo = copyGO.GetComponent<AXGameObject>();
						if (axgo != null)
							axgo.consumerAddress = this_address;

						copyGO.name = copyGO.name+"_" + this_address;
						copyGO.transform.parent = go.transform;
					}
				} // \CELLS




			} //i






			//Debug.Log(parametricObject.Name + " : " + parametricObject.bounds);



			GameObject.DestroyImmediate(nodePlugGO);
			GameObject.DestroyImmediate(cellPlugGO);


			// FINNISH AX_MESHES

			parametricObject.finishMultiAXMeshAndOutput(ax_meshes, isReplica);



			// FINISH BOUNDARIES

			setBoundsWithCombinator(boundsCombinator);




			// Turn ax_meshes into GameObjects
			if (makeGameObjects)
			{
				if (parametricObject.combineMeshes)
					go = parametricObject.makeGameObjectsFromAXMeshes(ax_meshes, true);

				Matrix4x4 tmx = parametricObject.getLocalMatrix();

				go.transform.rotation 		= AXUtilities.QuaternionFromMatrix(tmx);
				go.transform.position 		= AXUtilities.GetPosition(tmx);
				go.transform.localScale 	= parametricObject.getLocalScaleAxisRotated();//AXUtilities.GetScale(tmx);

				return go;
			}


			return null;			
		}












		public override Matrix4x4 localMatrixFromAddress(RepeaterItem ri, int i=0, int j=0, int k=0)
		{
			

			float perlinTranslation 	= 1;
			float perlinRot 			= 1;
			float perlinScale 			= 1;

			float actualTreadU = (zAxis) ? 0: actual_stepAngle;
			float actualTreadV = (zAxis) ? actual_stepAngle : 0;



			float dang = (ri == RepeaterItem.Node) ? (i*actual_stepAngle) : (i*actual_stepAngle + actual_stepAngle/2);

			Matrix4x4 radialDispl = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler (0,-dang,0), Vector3.one) * Matrix4x4.TRS(new Vector3(radius, 0, 0), Quaternion.Euler(0, -90, 0), Vector3.one);





			if (jitterTranslationTool != null)
				perlinTranslation 	=  Mathf.PerlinNoise( (i*actualTreadU + jitterTranslationTool.offset) * jitterTranslationTool.perlinScale,  	(j*actualTreadV) * jitterTranslationTool.perlinScale);

			if (jitterRotationTool != null)
				perlinRot 			=  Mathf.PerlinNoise( (i*actualTreadU + jitterRotationTool.offset) * jitterRotationTool.perlinScale,  	(j*actualTreadV) * jitterRotationTool.perlinScale);

			if (jitterScaleTool != null)
				perlinScale 		=  Mathf.PerlinNoise( (i*actualTreadU + jitterScaleTool.offset) * jitterScaleTool.perlinScale,  		(j*actualTreadV) * jitterScaleTool.perlinScale);



			// TRANSLATION 	*********
			Vector3 translate = new Vector3(0, (i+1)*actual_riser, 0);


			if (jitterTranslationTool != null)
				translate += new Vector3( perlinTranslation*jitterTranslationTool.x-jitterTranslationTool.x/2 , perlinTranslation*jitterTranslationTool.y-jitterTranslationTool.y/2, perlinTranslation*jitterTranslationTool.z-jitterTranslationTool.z/2);



			// ROTATION		*********
			Quaternion 	rotation = Quaternion.identity;//Quaternion.Euler (0, rotY, 0);
			if (jitterRotationTool != null && ! float.IsNaN(perlinRot))
				rotation = Quaternion.Euler ((perlinRot*jitterRotationTool.x - jitterRotationTool.x/2)*1.5f, (perlinRot*jitterRotationTool.y - jitterRotationTool.y/2), (perlinRot*jitterRotationTool.z - jitterRotationTool.z/2)) * rotation;


			// SCALE		**********
			jitterScale = Vector3.zero;
			if (jitterScaleTool != null)
				jitterScale = new Vector3( perlinScale*jitterScaleTool.x-jitterScaleTool.x/2 , perlinScale*jitterScaleTool.y-jitterScaleTool.y/2, perlinScale*jitterScaleTool.z-jitterScaleTool.z/2);


			// ******** USE A FUNCTION HERE TO GET FOR HANDLES AS WELL
			if (! quaternionIsValid(rotation))
				return Matrix4x4.identity;

			//return   Matrix4x4.TRS(translate, rotation, Vector3.one+jitterScale) * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, (((i + j) * progressiveRotation) + ( (alternateSteps && (i %2 ==0)) ? 180 : 0)), 0), Vector3.one);

			Matrix4x4 dm =  Matrix4x4.TRS(translate, rotation, Vector3.one+jitterScale) *radialDispl * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(i * progressiveRotationX,  90 + i * progressiveRotationY, i * progressiveRotationZ), Vector3.one)  ;

			return dm;

		}


	}
}
