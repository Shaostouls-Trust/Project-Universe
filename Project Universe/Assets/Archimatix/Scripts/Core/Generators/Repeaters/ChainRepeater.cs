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

namespace AX.Generators
{


	// REPEATER_BASE
	public class ChainRepeater : AX.Generators.Generator3D, IRepeater//, ICustomNode
	{

		// INPUTS
		public AXParameter	P_Node;
		public AXParameter	P_Cell;

		public AXParameter P_Size;
		public AXParameter P_Bay;
		public AXParameter P_Cells;
		public AXParameter P_ActualBay;


		// WORKING FIELDS (Updated every Generate)
		public AXParameter 			nodeSrc_p;
		public AXParametricObject 	nodeSrc_po;

		public AXParameter 			cellSrc_p;
		public AXParametricObject 	cellSrc_po;

		public float 	size;
		public float 	bay;
		public int   	cells;
		public float 	actualBay;



		public string sizeName;
		public string bayName;
		public string cellCountName;
		public string actualBayName;


		public virtual void initParameterNameStrings()
		{
			sizeName 		= "Size";
			bayName 		= "Bay";
			cellCountName 	= "Cells";
			actualBayName 	= "Actual_Bay";
		}

		public override void initGUIColor ()
		{

			GUIColor 		= new Color(.6f, .67f, .9f, .8f);
			GUIColorPro 	= new Color(.80f,.80f,1f, .75f);
			ThumbnailColor  = new Color(.318f,.31f,.376f);

		}


		// INIT_PARAMETRIC_OBJECT
		public override void init_parametricObject()
		{
			base.init_parametricObject ();

			initParameterNameStrings ();

			// INPUT MESH
			parametricObject.addParameter (new AXParameter (AXParameter.DataType.Mesh, AXParameter.ParameterType.Input, "Node Mesh"));



			// SIZE
			P_Size = parametricObject.addParameter(AXParameter.DataType.Float, 		sizeName, 9f);
			P_Size.expressions.Add (cellCountName+"="+sizeName+"/"+bayName);
			P_Size.expressions.Add (actualBayName+"="+sizeName+"/"+cellCountName);

			// BAY
			P_Bay = parametricObject.addParameter(AXParameter.DataType.Float, 		bayName, 3f, .1f, 1000f);
			P_Bay.expressions.Add (sizeName+"="+bayName+"*"+cellCountName);

			// CELLS
			P_Cells =parametricObject.addParameter(AXParameter.DataType.Int, 		cellCountName, 3, 1, 100);
			P_Cells.expressions.Add (actualBayName+"="+sizeName+"/"+cellCountName);
			P_Cells.expressions.Add (bayName+"="+actualBayName);

			// ACTUAL_BAY
			P_ActualBay = parametricObject.addParameter(AXParameter.DataType.Float,  actualBayName, 3f, .1f, 1000f);
			P_ActualBay.expressions.Add (sizeName+"="+actualBayName+"*"+cellCountName);

		
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.RepeaterTool, AXParameter.ParameterType.Output, "Output"));

		}



		// POLL INPUTS (only on graph change())
		public override void pollInputParmetersAndSetUpLocalReferences()
		{
			//Debug.Log("Repeater::pollInputParmetersAndSetUpLocalReferences " + parametricObject.Name + " parametersHaveBeenPolled="+parametersHaveBeenPolled);

			base.pollInputParmetersAndSetUpLocalReferences();

			initParameterNameStrings ();

			P_Node 					= parametricObject.getParameter("Node Mesh");
			P_Cell 					= parametricObject.getParameter("Cell Mesh");


			P_Size 					= parametricObject.getParameter(sizeName);
			P_Bay 					= parametricObject.getParameter(bayName);

			P_Cells 				= parametricObject.getParameter(cellCountName);

			P_ActualBay 			= parametricObject.getParameter(actualBayName);



		}



		// POLL CONTROLS (every model.generate())
		public override void pollControlValuesFromParmeters()
		{

			if (parametersHaveBeenPolled)
				return;

			base.pollControlValuesFromParmeters();


			nodeSrc_p 		= (P_Node		!= null)  	? getUpstreamSourceParameter(P_Node)	: null;
			nodeSrc_po 		= (nodeSrc_p	!= null) 	? nodeSrc_p.parametricObject			: null;

			cellSrc_p 		= (P_Cell		!= null)  	? getUpstreamSourceParameter(P_Cell)	: null;
			cellSrc_po 		= (cellSrc_p	!= null) 	? cellSrc_p.parametricObject			: null;



			if (P_Size != null) {
				P_Size.FloatVal = Math.Max (P_Size.FloatVal, .1f);
				size = P_Size.FloatVal;
			}


			if (P_Bay != null) {
				//P_Bay.FloatVal = Math.Max (P_Bay.FloatVal, .1f);
				bay = P_Bay.FloatVal;
			}

			if (P_Cells != null) {
				P_Cells.IntVal = (int)Math.Max (P_Cells.IntVal, 1);
				cells = P_Cells.IntVal;
			}

			if (P_ActualBay != null) {				
				//P_ActualBay.FloatVal = Math.Max (P_ActualBay.FloatVal, .1f);
				actualBay = P_ActualBay.FloatVal;
			}



			Bounds b = new Bounds();
			b.size = new Vector3 (size, .1f, .1f);
			b.center = Vector3.zero;
			b.extents = b.size/2;
			parametricObject.bounds = b;
		}






		// GENERATE STEP_REPEATER
		public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool isReplica)
		{

			if (parametricObject == null || !parametricObject.isActive)
				return null;

			preGenerate ();



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





			AXMesh tmpMesh;


			// BOUNDING

			CombineInstance[] boundsCombinator = new CombineInstance[cells];



			// LOOP

			//Debug.Log(nodeSrc_po.Name + ": " + nodeSrc_po.boundsMesh.vertices.Length);
			//AXGeometry.Utilities

			for (int i=0; i<cells; i++) 
			{
				//Debug.Log("["+i+"] i*actualBay="+i*actualBay+", perval="+perlval);


//				if (i == steps-1 && ! topStep)
//					break;

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
						Matrix4x4  mx 		=  localPlacement_mx * nodeSrc_po.generator.localMatrixWithAxisRotationAndAlignment;
						GameObject copyGO 	= (GameObject) GameObject.Instantiate(nodePlugGO, AXUtilities.GetPosition(mx), AXUtilities.QuaternionFromMatrix(mx));

//						Vector3 newJitterScale = jitterScale + Vector3.one;
//						copyGO.transform.localScale= new Vector3(copyGO.transform.localScale.x*newJitterScale.x, copyGO.transform.localScale.y*newJitterScale.y, copyGO.transform.localScale.z*newJitterScale.z);
//						copyGO.transform.localScale += jitterScale;


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

				// Daisy chain these
				Transform[] allChildren = go.transform.GetComponentsInChildren<Transform>();

				for (int j = allChildren.Length-1; j > 0 ; j--) {

					allChildren [j].parent = allChildren [j-1];
					CharacterJoint cj = allChildren [j].gameObject.AddComponent<CharacterJoint> ();
					cj.connectedBody = allChildren [j-1].gameObject.GetComponent<Rigidbody> ();

				}

				Matrix4x4 tmx = parametricObject.getLocalMatrix();

				go.transform.rotation 		= AXUtilities.QuaternionFromMatrix(tmx);
				go.transform.position 		= AXUtilities.GetPosition(tmx);
				go.transform.localScale 	= parametricObject.getLocalScaleAxisRotated();//AXUtilities.GetScale(tmx);

				return go;
			}


			return null;			


		}




		public  Matrix4x4 localNodeMatrixFromAddress(int i=0)
		{






			// TRANSLATION 	*********
			Vector3 translate = new Vector3(0, -(i+1)*actualBay, 0);


	
			return   Matrix4x4.TRS(translate, Quaternion.identity, Vector3.one);

		}







	}
}
