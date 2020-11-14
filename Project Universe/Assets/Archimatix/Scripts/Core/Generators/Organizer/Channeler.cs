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
	public class Channeler : Organizer, IOrganizer, ILogic
	{

		public override string GeneratorHandlerTypeName { get { return "ChannelerHandler"; } }

		public List<AXParameter> inputs;

		public AXParameter P_Channel;

		public int channel = 1;


		// INIT_PARAMETRIC_OBJECT
		public override void init_parametricObject() 
		{
			base.init_parametricObject();

			parametricObject.useMeshInputs = true;
			parametricObject.meshInputs = new List<AXParameter>();

			// PLAN SHAPE
			AXParameter p;

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.MaterialTool, AXParameter.ParameterType.Input, "Material"));
			p = parametricObject.addParameter(new AXParameter(AXParameter.DataType.CustomOption, "Channel"));
			p.optionLabels = new List<string>();
			p.optionLabels.Add("Item 1");
			p.optionLabels.Add("Item 2");
			p.optionLabels.Add("Item 3");
		

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Output, "Output Mesh"));
		}



		public override void pollInputParmetersAndSetUpLocalReferences()
		{
			base.pollInputParmetersAndSetUpLocalReferences();

			P_Channel =  parametricObject.getParameter("Channel");
		}

		// POLL CONTROLS (every model.generate())
		public override void pollControlValuesFromParmeters()
		{

			base.pollControlValuesFromParmeters();


			channel = (P_Channel  != null) 	? P_Channel.intval 	: 0;


			inputs = parametricObject.getAllInputMeshParameters();
			P_Channel.optionLabels = new List<string>();


			for (int i = 0; i < inputs.Count; i++) {
				AXParameter p = inputs [i];
				if (p.DependsOn != null)
					P_Channel.optionLabels.Add(p.DependsOn.parametricObject.Name);

			}

			P_Channel.optionLabels.Add("All");


		}

		// GROUPER::GENERATE
		public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool isReplica)
		{
			//if (ArchimatixUtils.doDebug)
			//Debug.Log (parametricObject.Name + " generate +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");


			if (! parametricObject.isActive)
				return null;

			

			preGenerate();


            if (parametricObject.code != null)
                parametricObject.executeCodeBloc(new List<string>(parametricObject.code.Split("\n"[0])), null);

            GameObject go 		= null;

			if (makeGameObjects && ! parametricObject.combineMeshes)
				go = ArchimatixUtils.createAXGameObject(parametricObject.Name, parametricObject);


			List<AXMesh> 	ax_meshes 		= new List<AXMesh>();


			// BOUNDING

			List<AXMesh> boundingMeshes = new List<AXMesh>();








			// Reinstate the original functionality of the Grouper as simple combiner in addition to Groupees.
			if (inputs != null && inputs.Count > 0)
			{


				// ALL
				if (channel == inputs.Count)
				{
					for(int i=0; i<inputs.Count; i++)
					{
						if (inputs[i] != null && inputs[i].DependsOn != null)
						{
							if (inputs[i].Dependents != null || inputs[i].Dependents.Count == 0)
							{
								AXParameter 			src_p  = inputs[i].DependsOn;
								AXParametricObject 		src_po = inputs[i].DependsOn.parametricObject;
								//if (! parametricObject.visited_pos.Contains (groupee))

								//Debug.Log("groupee.generateOutputNow: " + groupee.Name + " isAltered="+groupee.isAltered);
								if (src_po.isAltered)
								{
									src_po.generateOutputNow (makeGameObjects, initiator_po);
									//Debug.Log("XXXXX: " + groupee.Output.meshes.Count);
									src_po.isAltered = false;
									parametricObject.model.AlteredPOs.Remove(src_po);
								}

								if (src_p != null && src_p.meshes != null)
								{
									for (int j = 0; j < src_p.meshes.Count; j++) {

										AXMesh dep_amesh = src_p.meshes [j];
										ax_meshes.Add (dep_amesh.Clone (dep_amesh.transMatrix));
									}
								}

								// BOUNDING MESHES
								//boundsCombinator[i].mesh 		= input_p.DependsOn.parametricObject.boundsMesh;
								//boundsCombinator[i].transform 	= input_p.DependsOn.parametricObject.generator.localMatrixWithAxisRotationAndAlignment;
								if (src_po.boundsMesh != null)
									boundingMeshes.Add(new AXMesh(src_po.boundsMesh, src_po.generator.localMatrixWithAxisRotationAndAlignment));



								// GAME_OBJECTS

								if (makeGameObjects && !parametricObject.combineMeshes) {

									GameObject plugGO = src_po.generator.generate (true, initiator_po, isReplica);
									if (plugGO != null)
										plugGO.transform.parent = go.transform;
								}
							}
						}
					}
				}





				// JUST ONE CHANNEL
				else if (inputs.Count > channel && inputs[channel] != null)
				{





					AXParameter src_p = inputs[channel].DependsOn;

					if (src_p != null)
					{
						AXParametricObject src_po = src_p.parametricObject;

						if (src_po.is3D())
						{
							if (src_po.Output != null && src_po.Output.meshes != null)
							{
								for (int j = 0; j < src_po.Output.meshes.Count; j++) {

									AXMesh dep_amesh = src_po.Output.meshes [j];
									ax_meshes.Add (dep_amesh.Clone (dep_amesh.transMatrix));
								}
							}


							// BOUNDING MESHES
						//boundsCombinator[i].mesh 		= input_p.DependsOn.parametricObject.boundsMesh;
						//boundsCombinator[i].transform 	= input_p.DependsOn.parametricObject.generator.localMatrixWithAxisRotationAndAlignment;
							if (src_po.boundsMesh != null)
								boundingMeshes.Add(new AXMesh(src_po.boundsMesh, src_po.generator.localMatrixWithAxisRotationAndAlignment));


							// GAME_OBJECTS

							if (makeGameObjects && !parametricObject.combineMeshes) {

								GameObject plugGO = src_po.generator.generate (true, initiator_po, isReplica);
								if (plugGO != null)
									plugGO.transform.parent = go.transform;
							}


						}





						P_Output.meshes = src_p.meshes;






					}
				}


				// FINISH AX_MESHES


				//Debug.Log("ORG: " + ax_meshes.Count);
				parametricObject.finishMultiAXMeshAndOutput(ax_meshes, isReplica);



				// FINISH BOUNDS

				CombineInstance[] boundsCombinator = new CombineInstance[boundingMeshes.Count];
				for(int bb=0; bb<boundsCombinator.Length; bb++)
				{
					boundsCombinator[bb].mesh 		= boundingMeshes[bb].mesh;
					boundsCombinator[bb].transform 	= boundingMeshes[bb].transMatrix;
				}
				setBoundsWithCombinator(boundsCombinator);


				if (P_BoundsX != null && ! P_BoundsX.hasRelations()  &&	! P_BoundsX.hasExpressions())
					P_BoundsX.FloatVal = parametricObject.bounds.size.x;

				if (P_BoundsY != null && ! P_BoundsY.hasRelations()  &&	! P_BoundsY.hasExpressions())
					P_BoundsY.FloatVal = parametricObject.bounds.size.y;

				if (P_BoundsZ != null && ! P_BoundsZ.hasRelations()  &&	! P_BoundsZ.hasExpressions())
					P_BoundsZ.FloatVal = parametricObject.bounds.size.z;

			}



			// FINISH GAME_OBJECTS

			if (makeGameObjects)
			{
				if (parametricObject.combineMeshes)
				{
					go = parametricObject.makeGameObjectsFromAXMeshes(ax_meshes, true, false);


					// COMBINE ALL THE MESHES
					CombineInstance[] combine = new CombineInstance[ax_meshes.Count];

					int combineCt = 0;
					for (int i = 0; i < ax_meshes.Count; i++) {
						AXMesh _amesh = ax_meshes [i];
						combine [combineCt].mesh = _amesh.mesh;
						combine [combineCt].transform = _amesh.transMatrix;
						combineCt++;
					}

					Mesh combinedMesh = new Mesh();
					combinedMesh.CombineMeshes(combine);

					// If combine, use combined mesh as invisible collider
					MeshFilter mf = (MeshFilter) go.GetComponent(typeof(MeshFilter));

					if (mf == null)
						mf = (MeshFilter) go.AddComponent(typeof(MeshFilter));

					if (mf != null)
					{
						mf.sharedMesh = combinedMesh;
						parametricObject.addCollider(go);
					}
				}

				else
				{
					Matrix4x4 tmx = parametricObject.getLocalMatrix();

					go.transform.rotation 		= AXUtilities.QuaternionFromMatrix(tmx);
					go.transform.position 		= AXUtilities.GetPosition(tmx);
					go.transform.localScale 	= parametricObject.getLocalScaleAxisRotated();
				}
				return go;
			}




			return null;

		}

	}
}