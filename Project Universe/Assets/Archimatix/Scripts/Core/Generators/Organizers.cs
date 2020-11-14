#pragma warning disable

using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;



/* Organizers combine are ParametricObjects that organize other oParametricObjects. 
 *
 * They tend not to create any splines or meshes of their own, 
 * though they can have summary Parameters and Handles
 * in order to simplify the controls rather than showing all the controls 
 * of the POs they organize.
 *
 *
 */


using AXGeometry;


namespace AX.Generators
{



	public class PrefabInstancer : AX.Generators.Generator3D
	{
		GameObject gameObject;



		// INIT_PARAMETRIC_OBJECT
		public override void init_parametricObject() 
		{
			base.init_parametricObject();


			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Output, "Output Mesh"));
		}



		// GROUPER::GENERATE
		public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool renderToOutputParameter)
		{
			//if (ArchimatixUtils.doDebug)
			//Debug.Log (parametricObject.Name + " generate +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");

			if (! parametricObject.isActive)
				return null;

			if (parametricObject.prefab == null)
				return null;

			preGenerate();


			// MESHES FROM PREFAB
			List<AXMesh> ax_meshes = new List<AXMesh>();




			Transform[] transforms = parametricObject.prefab.GetComponentsInChildren<Transform>();

			//Debug.Log("transforms: " + transforms.Length);
			for(int i=0; i<transforms.Length; i++)
			{
                //Debug.Log(transforms[i].gameObject.name);

                Mesh mesh = null;

				MeshFilter mf = transforms[i].gameObject.GetComponent<MeshFilter>();

                if (mf != null)
                {
                    mesh = mf.sharedMesh;
                }
                else
                {
                    // try Skinned Mesh Renderer
                    SkinnedMeshRenderer smr = transforms[i].gameObject.GetComponent<SkinnedMeshRenderer>();
                    if (smr != null)
                    {
                        //Debug.Log(transforms[i].gameObject.name + " smr.sharedMesh="+ smr.sharedMesh);
                        mesh = smr.sharedMesh;
                    }
                }

               


                if (mesh != null)
				{
                  //Debug.Log("mesh=" + mesh);

                    Material mater = AXModel.defaultMaterial;

					Renderer renderer = transforms[i].gameObject.GetComponent<Renderer>();
					if (renderer != null)
						 mater = renderer.sharedMaterial;

					ax_meshes.Add (new AXMesh (mesh, transforms[i].localToWorldMatrix, mater));

				}
			}

			//Debug.Log("ax_meshes: " + ax_meshes.Count);


			CombineInstance[] boundsCombinator = new CombineInstance[ax_meshes.Count];
			int boundingCursor = 0;

			for(int i=0; i<ax_meshes.Count; i++)
			{
				boundsCombinator[boundingCursor].mesh 		= ax_meshes[i].mesh;
				boundsCombinator[boundingCursor].transform 	= ax_meshes[i].drawMeshMatrix;// localPlacement_mx * tmpSrc_po.generator.localMatrixWithAxisRotationAndAlignment;
				boundingCursor++;
			}
			setBoundsWithCombinator(boundsCombinator);


            //Debug.Log("ax_meshes.Count="+ ax_meshes.Count);

            parametricObject.finishMultiAXMeshAndOutput(ax_meshes, renderToOutputParameter);


			// GAME_OBJECTS

			if (makeGameObjects)
			{	
				GameObject go = GameObject.Instantiate( parametricObject.prefab);

				Transform[] ntransforms = go.GetComponentsInChildren<Transform>();
				for(int i=0; i<ntransforms.Length; i++)
				{
					ArchimatixUtils.AddAXGameObjectTo(parametricObject, ntransforms[i].gameObject);
				}



				//GameObject go = ArchimatixUtils.createAXGameObjectFromPrefab(parametricObject, parametricObject.prefab);

				Matrix4x4 tmx = parametricObject.getLocalMatrix();

				//Debug.Log(tmx);
				go.transform.rotation 		= AXUtilities.QuaternionFromMatrix(tmx);
				go.transform.position 		= AXUtilities.GetPosition(tmx);
				go.transform.localScale 	= parametricObject.getLocalScaleAxisRotated();
				return go;
			}
			return null;

		}


	}

	public interface IOrganizer
	{

	}


	public class Organizer : AX.Generators.Generator3D
	{
		public override void initGUIColor ()
		{
			GUIColor 		= new Color(.99f,.93f,.65f, .8f);
			GUIColorPro 	= new Color( 1f,  .9f, .85f, .8f);
			ThumbnailColor  = new Color(.318f, .31f, .376f);
		}


	}




	public class PhysicsJoiner : Organizer, IOrganizer
	{

		//public override string GeneratorHandlerTypeName { get { return "GrouperHandler"; } }

	

		AXParameter P_FromMesh;
		AXParameter P_ToMesh;

		AXParameter P_BreakForce;


		// WORKING FIELDS (Updated every Generate)

		public AXParameter 			fromSrc_p;
		public AXParametricObject 	fromSrc_po;

		public AXParameter 			toSrc_p;
		public AXParametricObject 	toSrc_po;

		public float breakForce = 10;

		// INIT_PARAMETRIC_OBJECT
		public override void init_parametricObject() 
		{
			base.init_parametricObject();

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Input, "From Mesh"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Input, "To Mesh"));

			parametricObject.addParameter(AXParameter.DataType.Float, AXParameter.ParameterType.GeometryControl, "BreakForce", 10f, 0f, 5000f);

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Output,"Output Mesh"));

		}


		// POLL INPUTS (only on graph change())
		public override void pollInputParmetersAndSetUpLocalReferences()
		{
			//Debug.Log("Repeater::pollInputParmetersAndSetUpLocalReferences " + parametricObject.Name + " parametersHaveBeenPolled="+parametersHaveBeenPolled);

			base.pollInputParmetersAndSetUpLocalReferences();

			P_FromMesh 						= parametricObject.getParameter("From Mesh");
			P_ToMesh 						= parametricObject.getParameter("To Mesh");

			P_BreakForce					= parametricObject.getParameter("BreakForce");
		}

		// POLL CONTROLS (every model.generate())
		public override void pollControlValuesFromParmeters()
		{
			base.pollControlValuesFromParmeters();


			fromSrc_p 		= (P_FromMesh  	!= null && P_FromMesh.DependsOn != null)  	? P_FromMesh.DependsOn					: null;
			fromSrc_po 		= (fromSrc_p != null)  										? fromSrc_p.parametricObject			: null;

			toSrc_p 		= (P_ToMesh  	!= null && P_ToMesh.DependsOn != null)  	? P_ToMesh.DependsOn					: null;
			toSrc_po 		= (toSrc_p != null)  										? toSrc_p.parametricObject			: null;

			breakForce 		= (P_BreakForce != null) ? breakForce = P_BreakForce.FloatVal : 0;
			if (breakForce < 0)
			{
				breakForce = 0;
				P_BreakForce.FloatVal = 0;
			}
		}



		// GENERATE PHYSICS_JOINER 
		public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool isReplica)
		{
			if (toSrc_po == null || fromSrc_po == null)
				return null;

			if (parametricObject == null || ! parametricObject.isActive)
				return null;

			

			preGenerate();






		


			// FROM_MESH 
			GameObject 			fromPlugGO 		= null;
			if (fromSrc_p != null && fromSrc_po != null)
			{
				if (makeGameObjects && ! parametricObject.combineMeshes)
					fromPlugGO = fromSrc_po.generator.generate(true,  initiator_po,  isReplica);
			}	

			// TO_MESH 
			GameObject 			toPlugGO 		= null;
			if (toSrc_p != null && toSrc_po != null)
			{
				if (makeGameObjects && ! parametricObject.combineMeshes)
					toPlugGO = toSrc_po.generator.generate(true,  initiator_po,  isReplica);
			}	




			// LIVE MESHES
			List<AXMesh> 	ax_meshes 		= new List<AXMesh>(fromSrc_p.meshes);

			if (toSrc_p != null)
				ax_meshes.AddRange(toSrc_p.meshes);




			List<AXMesh> boundingMeshes 	= new List<AXMesh>();
			boundingMeshes.Add(new AXMesh(fromSrc_po.boundsMesh, fromSrc_po.generator.localMatrixWithAxisRotationAndAlignment));

			if (toSrc_po != null)
				boundingMeshes.Add(new AXMesh(toSrc_po.boundsMesh,  toSrc_po.generator.localMatrixWithAxisRotationAndAlignment));

			// FINISH BOUNDS

			CombineInstance[] boundsCombinator = new CombineInstance[boundingMeshes.Count];
			for(int bb=0; bb<boundsCombinator.Length; bb++)
			{
				boundsCombinator[bb].mesh 		= boundingMeshes[bb].mesh;
				boundsCombinator[bb].transform 	= boundingMeshes[bb].transMatrix;
			}
			setBoundsWithCombinator(boundsCombinator);


			GameObject go 		= null;

			if (toSrc_po != null && fromSrc_po != null)
			{

				if (makeGameObjects && ! parametricObject.combineMeshes)
				{
					go = ArchimatixUtils.createAXGameObject(parametricObject.Name, parametricObject);
					fromPlugGO.transform.parent = go.transform;

					toPlugGO.transform.parent = go.transform;


					// CONNECT RIGIDBODIES WITH FIXED_JOINT COMPONENT
					Rigidbody[] fromRBs = fromPlugGO.GetComponentsInChildren<Rigidbody>();
					Rigidbody[] toRBs 	= toPlugGO.GetComponentsInChildren<Rigidbody>();


					if (fromRBs != null && fromRBs.Length>0 && toRBs != null && toRBs.Length>0)
					{
						for(int i=0; i<toRBs.Length; i++)
						{
							FixedJoint fj = fromRBs[0].gameObject.AddComponent<FixedJoint>();
							fj.connectedBody = toRBs[i];
							fj.breakForce = breakForce;
						}
					}
				}
			}


			parametricObject.finishMultiAXMeshAndOutput(ax_meshes, isReplica);

			//GameObject.DestroyImmediate(fromPlugGO);

			return go;
		}

	}







}
