using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;



using AXGeometry;


namespace AX.Generators
{



	/*	GROUPER
	 * 
	 * The Grouper has a list of ParametricObjects under its management.
	 * 
	 * A List of ParamtericObjects
	 */
	public class Grouper : Organizer, IOrganizer
	{

		public override string GeneratorHandlerTypeName { get { return "GrouperHandler"; } }

		List<AXParameter> inputs;






		// INIT_PARAMETRIC_OBJECT
		public override void init_parametricObject()
		{
			base.init_parametricObject();

			parametricObject.useMeshInputs = true;
			parametricObject.meshInputs = new List<AXParameter>();

			// PLAN SHAPE

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.MaterialTool, AXParameter.ParameterType.Input, "Material"));


			AXParameter p = null;

			p = parametricObject.addParameter(AXParameter.DataType.Float, AXParameter.ParameterType.GeometryControl, "SizeX", 4f, 0f, 50f);
			p.sizeBindingAxis = Axis.X;

			p = parametricObject.addParameter(AXParameter.DataType.Float, AXParameter.ParameterType.GeometryControl, "SizeY", 4f, 0f, 50f);
			p.sizeBindingAxis = Axis.Y;

			p = parametricObject.addParameter(AXParameter.DataType.Float, AXParameter.ParameterType.GeometryControl, "SizeZ", 4f, 0f, 50f);
			p.sizeBindingAxis = Axis.Z;

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Output, "Output Mesh"));
		}



		// POLL CONTROLS (every model.generate())
		public override void pollControlValuesFromParmeters()
		{
			base.pollControlValuesFromParmeters();

			inputs = parametricObject.getAllInputMeshParameters();

		}


		// GROUPER::GENERATE
		public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool isReplica)
		{
			//if (ArchimatixUtils.doDebug)
			//Debug.Log (parametricObject.Name + " generate +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");

			if (!parametricObject.isActive)
				return null;

			if (parametricObject.Groupees == null)
				return null;


			preGenerate();


			parametricObject.useMeshInputs = true;

			if (parametricObject.meshInputs == null)
				parametricObject.meshInputs = new List<AXParameter>();


			// PROCESS INPUT SHAPES

			// pass Input Shape through...


			//Debug.Log( parametricObject.Name + " <><><><><> PROCESS INPUT SHAPES");
			List<AXParameter> inputShapes = parametricObject.getAllInputShapes();

			for (int i = 0; i < inputShapes.Count; i++)
			{
				AXParameter inputShape = inputShapes[i];

				//				if (inputShape != null) 
				//				{
				//					inputShape.polyTree = null;
				//					AXShape.thickenAndOffset(ref inputShape,  inputShape.DependsOn);
				//				}
				//


				//				// This causes external (unparented) objects to be made
				//				foreach(AXParameter d in inputShape.Dependents)
				//					//if (d.parametricObject.grouper != null && d.parametricObject.grouper.Guid == parametricObject.Guid)
				//						d.parametricObject.setAltered();

				if (inputShape != null)
				{
					inputShape.polyTree = null;
					if (!isReplica && inputShape.Dependents != null)
					{
						// Some Replicant has set the paths of this Plan_P manually

						AXShape.thickenAndOffset(ref inputShape, inputShape);
					}
					else
						AXShape.thickenAndOffset(ref inputShape, inputShape.DependsOn);
				}


			}



			GameObject go = null;

			if (makeGameObjects && !parametricObject.combineMeshes)
				go = ArchimatixUtils.createAXGameObject(parametricObject.Name, parametricObject);

			List<AXMesh> ax_meshes = new List<AXMesh>();


			List<AXMesh> boundingMeshes = new List<AXMesh>();

			// for each input


			//List<AXParameter> inputMeshes = parametricObject.getAllInputMeshParameters();





			// Reinstate the original functionality of the Grouper as simple combiner in addition to Groupees.
			if (inputs != null && inputs.Count > 0)
			{
				for (int i = 0; i < inputs.Count; i++)
				{
					if (inputs[i] != null && inputs[i].DependsOn != null)
					{
						if (inputs[i].Dependents != null || inputs[i].Dependents.Count == 0)
						{
							AXParameter src_p = inputs[i].DependsOn;
							AXParametricObject src_po = inputs[i].DependsOn.parametricObject;
							//if (! parametricObject.visited_pos.Contains (groupee))


							if (src_po.isAltered)
							{
								//Debug.Log("groupee.generateOutputNow: " + src_po.Name + " isAltered=" + src_po.isAltered);
								src_po.generateOutputNow(makeGameObjects, initiator_po);
								//Debug.Log("XXXXX: " + groupee.Output.meshes.Count);
								src_po.isAltered = false;
								parametricObject.model.AlteredPOs.Remove(src_po);
							}

							if (src_p != null && src_p.meshes != null)
							{
								for (int j = 0; j < src_p.meshes.Count; j++)
								{

									AXMesh dep_amesh = src_p.meshes[j];
									ax_meshes.Add(dep_amesh.Clone(dep_amesh.transMatrix));
								}
							}

							// BOUNDING MESHES
							//boundsCombinator[i].mesh 		= input_p.DependsOn.parametricObject.boundsMesh;
							//boundsCombinator[i].transform 	= input_p.DependsOn.parametricObject.generator.localMatrixWithAxisRotationAndAlignment;
							if (src_po.boundsMesh != null)
								boundingMeshes.Add(new AXMesh(src_po.boundsMesh, src_po.generator.localMatrixWithAxisRotationAndAlignment));



							// GAME_OBJECTS

							if (makeGameObjects && !parametricObject.combineMeshes)
							{

								GameObject plugGO = src_po.generator.generate(true, initiator_po, isReplica);
								if (plugGO != null)
									plugGO.transform.parent = go.transform;
							}
						}
					}
				}
			}








			// *** GROUPEES - Generate the groupees here 
			// so that all the inputs (thicknesses, etc.) have been processed first.
			//Debug.Log(" ?????????????? AteredPOs.Count="+parametricObject.model.AlteredPOs.Count);
			//List<AXParametricObject> visited_pos = new List<AXParametricObject>();
			if (parametricObject.Groupees != null && parametricObject.Groupees.Count > 0)
			{
				for (int i = 0; i < parametricObject.Groupees.Count; i++)
				{
					AXParametricObject groupee = parametricObject.Groupees[i];
					//if (! parametricObject.visited_pos.Contains (groupee))

					//Debug.Log("groupee.generateOutputNow: " + groupee.Name + " isAltered="+groupee.isAltered);
					if (groupee.isAltered)
					{
						//groupee.generateOutputNow (makeGameObjects, initiator_po);

						// CAREFUL: this last argument is force running this regardless of contains. Could make infinite loop
						groupee.generateOutputNow(makeGameObjects, initiator_po, true, true);



						//Debug.Log("XXXXX: " + groupee.Output.meshes.Count);
						groupee.isAltered = false;
						parametricObject.model.AlteredPOs.Remove(groupee);
					}
				}

			}





			// BOUNDING




			// Process
			for (int i = 0; i < parametricObject.Groupees.Count; i++)
			{

				AXParametricObject groupee = parametricObject.Groupees[i];


				//if (input_p != null && input_p.DependsOn != null && input_p.DependsOn.meshes != null && input_p.DependsOn.meshes.Count > 0) {
				//if (groupee != null && groupee.is3D() && ! groupee.hasDependents() && groupee.Output != null && groupee.Output.meshes != null) 
				if (groupee != null && groupee.is3D() && groupee.shouldRenderSelf(true))
				{
					// AX_MESHES
					//Debug.Log("(*) (*) (*) (*) groupee: " + groupee.Name + " " +groupee.Output.meshes.Count + " isAltered = " + groupee.isAltered);
					if (groupee.Output != null && groupee.Output.meshes != null)
					{
						for (int j = 0; j < groupee.Output.meshes.Count; j++)
						{

							AXMesh dep_amesh = groupee.Output.meshes[j];
							ax_meshes.Add(dep_amesh.Clone(dep_amesh.transMatrix));
						}
					}


					// BOUNDING MESHES
					//boundsCombinator[i].mesh 		= input_p.DependsOn.parametricObject.boundsMesh;
					//boundsCombinator[i].transform 	= input_p.DependsOn.parametricObject.generator.localMatrixWithAxisRotationAndAlignment;
					if (groupee.boundsMesh != null)
						boundingMeshes.Add(new AXMesh(groupee.boundsMesh, groupee.generator.localMatrixWithAxisRotationAndAlignment));



					// GAME_OBJECTS

					if (makeGameObjects && !parametricObject.combineMeshes)
					{

						GameObject plugGO = groupee.generator.generate(true, initiator_po, isReplica);
						if (plugGO != null)
							plugGO.transform.parent = go.transform;
					}

				}
			}



			// FINISH AX_MESHES


			//Debug.Log("ORG: " + ax_meshes.Count);
			parametricObject.finishMultiAXMeshAndOutput(ax_meshes, isReplica);





			// FINISH BOUNDS

			CombineInstance[] boundsCombinator = new CombineInstance[boundingMeshes.Count];
			for (int bb = 0; bb < boundsCombinator.Length; bb++)
			{
				boundsCombinator[bb].mesh = boundingMeshes[bb].mesh;
				boundsCombinator[bb].transform = boundingMeshes[bb].transMatrix;
			}
			setBoundsWithCombinator(boundsCombinator);


			if (P_BoundsX != null && !P_BoundsX.hasRelations() && !P_BoundsX.hasExpressions())
				P_BoundsX.FloatVal = parametricObject.bounds.size.x;

			if (P_BoundsY != null && !P_BoundsY.hasRelations() && !P_BoundsY.hasExpressions())
				P_BoundsY.FloatVal = parametricObject.bounds.size.y;

			if (P_BoundsZ != null && !P_BoundsZ.hasRelations() && !P_BoundsZ.hasExpressions())
				P_BoundsZ.FloatVal = parametricObject.bounds.size.z;




			//Debug.Log("*** *****> " + parametricObject.model.AlteredPOs.Count);


			// FINISH GAME_OBJECTS

			if (makeGameObjects)
			{
				if (parametricObject.combineMeshes)
				{
					go = parametricObject.makeGameObjectsFromAXMeshes(ax_meshes, true, false);


					// COMBINE ALL THE MESHES
					CombineInstance[] combine = new CombineInstance[ax_meshes.Count];

					int combineCt = 0;
					for (int i = 0; i < ax_meshes.Count; i++)
					{
						AXMesh _amesh = ax_meshes[i];
						combine[combineCt].mesh = _amesh.mesh;
						combine[combineCt].transform = _amesh.transMatrix;
						combineCt++;
					}

					Mesh combinedMesh = new Mesh();
					combinedMesh.CombineMeshes(combine);

					// If combine, use combined mesh as invisible collider
					MeshFilter mf = (MeshFilter)go.GetComponent(typeof(MeshFilter));

					if (mf == null)
						mf = (MeshFilter)go.AddComponent(typeof(MeshFilter));

					if (mf != null)
					{
						mf.sharedMesh = combinedMesh;
						parametricObject.addCollider(go);
					}
				}

				else
				{
					Matrix4x4 tmx = parametricObject.getLocalMatrix();

					go.transform.rotation = AXUtilities.QuaternionFromMatrix(tmx);
					go.transform.position = AXUtilities.GetPosition(tmx);
					go.transform.localScale = parametricObject.getLocalScaleAxisRotated();
				}
				return go;
			}


			//parametricObject.model.sw.milestone(parametricObject.Name + " generate");


			return null;

		}




		public void setControlSizeToSumOfBoundsMeshes()
		{




		}


		public override void connectionMadeWith(AXParameter to_p, AXParameter from_p)
		{

			if (from_p == null)// || from_p.DependsOn == null)
				return;

			//AXParameter this_p = (to_p.parametricObject == parametricObject) ? to_p   : from_p;
			//AXParameter src_p  = (to_p.parametricObject == parametricObject) ? from_p : to_p;

			base.connectionMadeWith(to_p, from_p);


			List<AXParameter> inputMeshes = parametricObject.getAllInputMeshParameters();

			if (inputMeshes.Count == 1 && inputMeshes[0] != null && inputMeshes[0].DependsOn != null)
			{
				Bounds b = inputMeshes[0].DependsOn.parametricObject.bounds;

				if (P_Size_X != null)
				{
					P_Size_X.FloatVal = b.size.x;
					P_Size_Y.FloatVal = b.size.y;
					P_Size_Z.FloatVal = b.size.z;


				}

			}

		}








	}



}