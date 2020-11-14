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

	public class StepRepeater : RepeaterBase, IRepeater
	{
		public override string GeneratorHandlerTypeName { get { return "StepRepeaterHandler"; } }


		// INPUTS

		public AXParameter P_Start;
		public AXParameter P_End;
		public AXParameter P_Run;
		public AXParameter P_Rise;
		public AXParameter P_Riser;
		public AXParameter P_Steps;
		public AXParameter P_Actual_Riser;
		public AXParameter P_Actual_Tread;

		public AXParameter P_TopStep;
		public AXParameter P_AlternateSteps;


		// OUTPUTS
		public AXParameter P_Stair_Profile;




		// WORKING FIELDS (Updated every Generate)

		public float start;
		public float end;
		public float rise;
		public float run;
		public float riser;
		public float tread;
		public int 	 steps;
		public float actual_riser;
		public float actual_tread;


		public bool topStep;
		public bool alternateSteps;


		public Path stairProfile;



		// INIT_PARAMETRIC_OBJECT
		public override void init_parametricObject() 
		{
			base.init_parametricObject();

			// INPUT MESH
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Input, "Node Mesh"));

			// JITTER
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.JitterTool,   AXParameter.ParameterType.Input, "Jitter Translation"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.JitterTool,   AXParameter.ParameterType.Input, "Jitter Rotation"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.JitterTool,   AXParameter.ParameterType.Input, "Jitter Scale"));

			// START
			P_Start = parametricObject.addParameter(AXParameter.DataType.Float, 			"Start", 		0f);
			P_Start.expressions.Add ("Run=End-Start");

			// END
			P_End = parametricObject.addParameter(AXParameter.DataType.Float, 				"End", 			3.6f);
			P_End.expressions.Add ("Run=End-Start");

			// RUN
			P_Run = parametricObject.addParameter(AXParameter.DataType.Float, 				"Run", 			3.6f);
			P_Run.expressions.Add ("End=Start+Run");
			P_Run.expressions.Add ("Actual_Tread=Run/Steps");

			// RISE
			P_Rise = parametricObject.addParameter(AXParameter.DataType.Float, 				"Rise", 		3f);
			P_Rise.expressions.Add ("Steps=ceil(Rise/Riser)");
			P_Rise.expressions.Add ("Actual_Riser=Rise/Steps");
			P_Rise.expressions.Add ("Actual_Tread=Run/Steps");
			P_Rise.min = .01f;

			// RISER
			P_Riser = parametricObject.addParameter(AXParameter.DataType.Float, 			"Riser", 		.25f);
			//P_Riser.expressions.Add ("Rise=Riser*Steps");
			P_Riser.expressions.Add ("Steps=ceil(Rise/Riser)");
			P_Riser.expressions.Add ("Actual_Riser=Rise/Steps");


			// STEPS
			P_Steps =parametricObject.addParameter(AXParameter.DataType.Int, 				"Steps", 		12);
			P_Steps.expressions.Add ("Actual_Riser=Rise/Steps");
			P_Steps.expressions.Add ("Riser=Actual_Riser");
			P_Steps.expressions.Add ("Actual_Tread=Run/Steps");
			P_Steps.intmin = 1;

			// ACTUAL_RISER
			P_Actual_Riser = parametricObject.addParameter(AXParameter.DataType.Float,  		"Actual_Riser", .25f);
			P_Actual_Riser.expressions.Add ("Rise=Actual_Riser*Steps");

			// ACTUAL_TREAD
			P_Actual_Tread = parametricObject.addParameter(AXParameter.DataType.Float,  		"Actual_Tread", .3f);


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


			P_Start 				= parametricObject.getParameter("Start");
			P_End 					= parametricObject.getParameter("End");
			P_Rise 					= parametricObject.getParameter("Rise");
			P_Run 					= parametricObject.getParameter("Run"); 
			P_Riser 				= parametricObject.getParameter("Riser"); 
			P_Steps 				= parametricObject.getParameter("Steps"); 
			P_Actual_Riser 			= parametricObject.getParameter("Actual_Riser"); 
			P_Actual_Tread 			= parametricObject.getParameter("Actual_Tread"); 

			if (P_Run.FloatVal < .01) P_Run.FloatVal = .01f;

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

			rise = (P_Rise != null) ? P_Rise.FloatVal : 3;

			if (P_Rise != null) {
				P_Rise.FloatVal = Math.Max (P_Rise.FloatVal, .01f);
				rise = P_Rise.FloatVal;
			}


			run = P_Run.FloatVal;


			start 			= (P_Start != null) ? P_Start.FloatVal : 0;
			end 			= (P_End != null) 	? P_End.FloatVal : run;

			riser 			= P_Riser.FloatVal;
			steps 			= P_Steps.IntVal;

			//actual_riser 	= P_Actual_Riser.FloatVal;

			actual_riser	= validateFloatValueWithMin(P_Actual_Riser, .01f);



			actual_tread 	= P_Actual_Tread.FloatVal;
			actual_tread    = validateFloatValueWithMin(P_Actual_Tread, .01f);


			zAxis 				= P_zAxis.boolval;
			topStep				= P_TopStep.boolval;
			alternateSteps		= P_AlternateSteps.boolval;



			actual_tread = (topStep) ? run / steps : run / (steps-1);
			P_Actual_Tread.FloatVal = actual_tread;

			progressiveRotationX = (P_ProgressiveRotationX != null) ?  P_ProgressiveRotationX.FloatVal : 0;
			progressiveRotationY = (P_ProgressiveRotationX != null) ?  P_ProgressiveRotationY.FloatVal : 0;
			progressiveRotationZ = (P_ProgressiveRotationX != null) ?  P_ProgressiveRotationZ.FloatVal : 0;

			if (P_Stair_Profile != null)
				P_Stair_Profile.axis 	= (zAxis) ? Axis.X : Axis.NZ;


		}



		public void generateStairProfile()
		{
			AXTurtle t = new AXTurtle();
			t.dir(90);
			t.mov(start, 0);

			int loops = (topStep) ? steps : steps-1;
			for (int i = 0; i<loops; i++)
			{
				t.fwd(actual_riser);
				t.right(actual_tread);
			}
			t.back(rise); 
			t.path.Reverse();

			if (P_Stair_Profile != null)
			{
				P_Stair_Profile.paths = new Paths();
				P_Stair_Profile.paths.Add(t.path);
			}
		}









		// GENERATE STEP_REPEATER
		public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool isReplica)
		{

			if (parametricObject == null || ! parametricObject.isActive)
				return null;

			preGenerate();


			generateStairProfile();


			// NODE_MESH
			GameObject nodePlugGO 					= null;


			if (nodeSrc_p != null)
			{
				nodeSrc_po 		= nodeSrc_p.parametricObject;
				if (makeGameObjects && ! parametricObject.combineMeshes)
					nodePlugGO = nodeSrc_po.generator.generate(true,  initiator_po,  isReplica);
			}	

			if (nodeSrc_po == null)
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




			shiftU = start;
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


					// LOCAL_PLACEMENT //

					localPlacement_mx = localNodeMatrixFromAddress(i);


					 
					// AX_MESHES //

					for (int mi = 0; mi < nodeSrc_p.meshes.Count; mi++) {
						AXMesh dep_amesh = nodeSrc_p.meshes [mi];

						tmpMesh = dep_amesh.Clone (localPlacement_mx * dep_amesh.transMatrix);
						tmpMesh.subItemAddress = this_address;
						ax_meshes.Add (tmpMesh);
					}

					 

					// BOUNDING MESHES //

					boundsCombinator[i].mesh 		= nodeSrc_po.boundsMesh;
					boundsCombinator[i].transform 	= localPlacement_mx * nodeSrc_po.generator.localMatrixWithAxisRotationAndAlignment;



					// GAME_OBJECTS //

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




			} //i






			//Debug.Log(parametricObject.Name + " : " + parametricObject.bounds);



			GameObject.DestroyImmediate(nodePlugGO);


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











		public override Matrix4x4 getLocalConsumerMatrixPerInputSocket(AXParametricObject caller)
		{
			// use shift too
			// use caller address
			if (caller == null)
				return Matrix4x4.identity;

			Matrix4x4 optionalLocalM = Matrix4x4.identity;

			if (caller.selectedConsumer == null || caller.selectedConsumer != this.parametricObject || String.IsNullOrEmpty(caller.selectedConsumerAddress))
			{	
				caller.selectedConsumerAddress = "node_0";
				optionalLocalM = caller.getLocalMatrix() * caller.getAxisRotationMatrix().inverse;
			}

			string[] address = caller.selectedConsumerAddress.Split('_');


			if (address.Length < 2)
				return Matrix4x4.identity;

			string meshInput = address[0]; // e.g., "node", "cell", "spanU", "spanV"


			int indexU = int.Parse(address[1]);
			int indexV = 0;

			if (address.Length > 2)
				indexV = int.Parse(address[2]);


			// Find out which input this caller is fed into Node Mesh, Bay Span or Cell Mesh?

			if (meshInput == "node" && nodeSrc_p!= null && caller == nodeSrc_p.Parent)
				return localNodeMatrixFromAddress(indexU, indexV) * optionalLocalM;



			return Matrix4x4.identity;
		} 







		public  Matrix4x4 localNodeMatrixFromAddress(int i=0, int j=0)
		{
			float perlinTranslation 	= 1;
			float perlinRot 			= 1;
			float perlinScale 			= 1;

			float actualTreadU = (zAxis) ? 0: actual_tread;
			float actualTreadV = (zAxis) ? actual_tread : 0;




			float shiftUU = (zAxis) ? shiftV : shiftU;
			float shiftVV = (zAxis) ? shiftU : shiftV;



			if (jitterTranslationTool != null)
				perlinTranslation 	=  Mathf.PerlinNoise( (i*actualTreadU + jitterTranslationTool.offset) * jitterTranslationTool.perlinScale,  	(j*actualTreadV) * jitterTranslationTool.perlinScale);

			if (jitterRotationTool != null)
				perlinRot 			=  Mathf.PerlinNoise( (i*actualTreadU + jitterRotationTool.offset) * jitterRotationTool.perlinScale,  	(j*actualTreadV) * jitterRotationTool.perlinScale);

			if (jitterScaleTool != null)
				perlinScale 		=  Mathf.PerlinNoise( (i*actualTreadU + jitterScaleTool.offset) * jitterScaleTool.perlinScale,  		(j*actualTreadV) * jitterScaleTool.perlinScale);



			// TRANSLATION 	*********
			Vector3 translate = new Vector3((i*actualTreadU+shiftUU), (i+1)*actual_riser, (i*actualTreadV+shiftVV));


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

			return   Matrix4x4.TRS(translate, rotation, Vector3.one+jitterScale) * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler((i + j) * progressiveRotationX, (((i + j) * progressiveRotationY) + ( (alternateSteps && (i %2 ==0)) ? 180 : 0)), (i + j) * progressiveRotationZ), Vector3.one);

		}


	}
}