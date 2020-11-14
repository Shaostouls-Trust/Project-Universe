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
	
	public interface IRepeater
	{
	}
	
	public interface IReplica
	{
	}


	// REPEATER_BASE
	public class RepeaterBase : AX.Generators.Generator3D
	{

		// INPUTS
		public AXParameter	P_Node;
		public AXParameter	P_Cell;
		public AXParameter	P_SpanU;
		public AXParameter	P_SpanV;

		public AXParameter	P_Corner;


		public AXParameter	P_RepeaterU;
		public AXParameter	P_RepeaterV;

		public AXParameter	P_BoundingShape;


		public AXParameter P_BoundingIsVoid;


	
		public AXParameter 	P_zAxis;
		public AXParameter 	P_ProgressiveRotationX;
		public AXParameter 	P_ProgressiveRotationY;
		public AXParameter 	P_ProgressiveRotationZ;



		// WORKING FIELDS (Updated every Generate)
		public AXParameter 			nodeSrc_p;
		public AXParametricObject 	nodeSrc_po;

		public AXParameter 			cellSrc_p;
		public AXParametricObject 	cellSrc_po;

		public AXParameter 			spanUSrc_p;
		public AXParametricObject 	spanUSrc_po;

		public AXParameter 			spanVSrc_p;
		public AXParametricObject 	spanVSrc_po;

		public AXParameter 			cornerSrc_p;
		public AXParametricObject 	cornerSrc_po;

		public RepeaterTool 		repeaterToolU;
		public RepeaterTool 		repeaterToolV;

		public AXParameter			boundingShape_p;
		public AXParametricObject 	boundingShapeSrc_po;

		public bool					boundingIsVoid;

		public bool  zAxis;
		public float progressiveRotationX;
		public float progressiveRotationY;
		public float progressiveRotationZ;

		public Vector3 				translate;
		public Quaternion			rotation 		= Quaternion.identity;

		public float shiftU = 0;
		public float shiftV = 0;



		public override void initGUIColor ()
		{
			
			GUIColor 		= new Color(.6f, .67f, .9f, .8f);
			GUIColorPro 	= new Color(.80f,.80f,1f, .75f);
			ThumbnailColor  = new Color(.318f,.31f,.376f);

		}


		public override void init_parametricObject() 
		{
			base.init_parametricObject();


		}

		// POLL INPUTS (only on graph change())
		public override void pollInputParmetersAndSetUpLocalReferences()
		{
			//Debug.Log("Repeater::pollInputParmetersAndSetUpLocalReferences " + parametricObject.Name + " parametersHaveBeenPolled="+parametersHaveBeenPolled);

			base.pollInputParmetersAndSetUpLocalReferences();

			P_Node 					= parametricObject.getParameter("Node Mesh");
			P_Cell 					= parametricObject.getParameter("Cell Mesh");
			P_SpanU 				= parametricObject.getParameter("SpanU Mesh", "Bay SpanU");
			P_SpanV 				= parametricObject.getParameter("SpanV Mesh", "Bay SpanV");
			P_Corner 				= parametricObject.getParameter("Corner Mesh");



			P_RepeaterU 			=  parametricObject.getParameter("RepeaterU");
			P_RepeaterV 			=  parametricObject.getParameter("RepeaterV");

			P_zAxis					= parametricObject.getParameter("zAxis");
			P_ProgressiveRotationX 	= parametricObject.getParameter("IncrRotX");
			P_ProgressiveRotationY 	= parametricObject.getParameter("IncrRotY");
			P_ProgressiveRotationZ 	= parametricObject.getParameter("IncrRotZ");

		}

		public virtual bool shouldDoLastItem(RepeaterItem ri)
		{
			return true;

		}

		// POLL CONTROLS (every model.generate())
		public override void pollControlValuesFromParmeters()
		{
			base.pollControlValuesFromParmeters();

			nodeSrc_p 		= (P_Node		!= null)  	? getUpstreamSourceParameter(P_Node)	: null;
			nodeSrc_po 		= (nodeSrc_p	!= null) 	? nodeSrc_p.parametricObject			: null;

			cellSrc_p 		= (P_Cell		!= null)  	? getUpstreamSourceParameter(P_Cell)	: null;
			cellSrc_po 		= (cellSrc_p	!= null) 	? cellSrc_p.parametricObject			: null;

			spanUSrc_p 		= (P_SpanU		!= null) 	? getUpstreamSourceParameter(P_SpanU)	: null;
			spanUSrc_po 	= (spanUSrc_p	!= null ) 	? spanUSrc_p.parametricObject			: null;

			spanVSrc_p 		= (P_SpanV		!= null) 	? getUpstreamSourceParameter(P_SpanV)	: null;
			spanVSrc_po 	= (spanVSrc_p	!= null)	? spanVSrc_p.parametricObject			: null;
				
			cornerSrc_p 	= (P_Corner		!= null) 	? getUpstreamSourceParameter(P_Corner)	: null;
			cornerSrc_po 	= (cornerSrc_p	!= null) 	? cornerSrc_p.parametricObject			: null;





			AXParameter P_RepeaterU_Src_p = ( P_RepeaterU != null) 	?getUpstreamSourceParameter(P_RepeaterU)						: null;
			repeaterToolU = (P_RepeaterU_Src_p != null) 			? P_RepeaterU_Src_p.parametricObject.generator as RepeaterTool 	: null;

			AXParameter P_RepeaterV_Src_p = ( P_RepeaterV != null) 	?getUpstreamSourceParameter(P_RepeaterV)						: null;
			repeaterToolV = (P_RepeaterV_Src_p != null) 			? P_RepeaterV_Src_p.parametricObject.generator as RepeaterTool 	: null;

		
			zAxis 					= (P_zAxis != null) ? P_zAxis.boolval : false;
			progressiveRotationX 	= (P_ProgressiveRotationX != null) ? P_ProgressiveRotationX.FloatVal : 0;
			progressiveRotationY 	= (P_ProgressiveRotationY != null) ? P_ProgressiveRotationY.FloatVal : 0;
			progressiveRotationZ 	= (P_ProgressiveRotationZ != null) ? P_ProgressiveRotationZ.FloatVal : 0;

		}



		public override void connectionMadeWith(AXParameter to_p, AXParameter from_p)
		{
			
			if (from_p == null)// || from_p.DependsOn == null)
				return;

			AXParameter this_p = (to_p.parametricObject == parametricObject) ? to_p   : from_p;
			AXParameter src_p  = (to_p.parametricObject == parametricObject) ? from_p : to_p;
			//Debug.Log ("connectionMadeWith AAA "+ this_p.Name);

			//Repeater repeater = (to_p.parametricObject.generator is Repeater) ? to_p.parametricObject.generator as Repeater : from_p.parametricObject.generator as Repeater;

			base.connectionMadeWith(to_p, from_p);

			//if (repeater == null)
			//	return;

			//AXParametricObject po = repeater.parametricObject;


			if (parametricObject.is2D())
				return;

			//Debug.Log ("connectionMadeWith BBB "+ this_p.Name);

			switch(this_p.Name) { 
			case "Node Mesh":
				nodeSrc_p = src_p;
				if (! parametricObject.isInitialized)
					initializeBays(this_p.Name);
				break;
			case "Cell Mesh":
				cellSrc_p = src_p;
				if (! parametricObject.isInitialized)
					initializeBays(this_p.Name);
				break;
			case "Bay SpanU":
			case "SpanU Mesh":
				spanUSrc_p = src_p; 
				if (! parametricObject.isInitialized)
					initializeBays(this_p.Name);
				break;
			case "Bay SpanV":
			case "SpanV Mesh":
				spanUSrc_p = src_p;
				if (! parametricObject.isInitialized)
					initializeBays(this_p.Name);
				break;

			case "Jitter Translation":
				jitterTranslationTool = src_p.parametricObject.generator as JitterTool;
				break;

			case "Jitter Rotation":
				jitterRotationTool = src_p.parametricObject.generator as JitterTool;
				break;

			case "Jitter Scaling":
				jitterScaleTool = src_p.parametricObject.generator as JitterTool;
				break;

			case "RepeaterU":
				repeaterToolU = src_p.parametricObject.generator as RepeaterTool;
				break;
			case "RepeaterV":
				repeaterToolV = from_p.parametricObject.generator as RepeaterTool;
				break;
			}
		}

		public virtual void initializeBays(string pName)
		{
			//Debug.Log ("HERE");
			if (parametricObject.isInitialized)
				return;

			parametricObject.isInitialized = true;

			switch(pName)
			{
			case "Node Mesh":
				if (repeaterToolU != null)
					repeaterToolU.P_Bay.initiatePARAMETER_Ripple_setFloatValueFromGUIChange( nodeSrc_p.parametricObject.bounds.size.x*3.5f);
				if (repeaterToolV != null)
					repeaterToolV.P_Bay.initiatePARAMETER_Ripple_setFloatValueFromGUIChange( nodeSrc_p.parametricObject.bounds.size.z*3.5f);
				break;
			case "Cell Mesh":
				if (repeaterToolU != null)
					repeaterToolU.P_Bay.initiatePARAMETER_Ripple_setFloatValueFromGUIChange( cellSrc_p.parametricObject.bounds.size.x*3.1f);
				if (repeaterToolV != null)
					repeaterToolV.P_Bay.initiatePARAMETER_Ripple_setFloatValueFromGUIChange( cellSrc_p.parametricObject.bounds.size.z*3.1f);
				break;
			case "Bay SpanU":
				if (repeaterToolU != null)
					repeaterToolU.P_Bay.initiatePARAMETER_Ripple_setFloatValueFromGUIChange( spanUSrc_p.parametricObject.bounds.size.x*2.1f);
				if (repeaterToolV != null)
					repeaterToolV.P_Bay.initiatePARAMETER_Ripple_setFloatValueFromGUIChange( spanUSrc_p.parametricObject.bounds.size.z*2.1f);
				break;
			case "Bay SpanV":
				if (repeaterToolU != null)
					repeaterToolU.P_Bay.initiatePARAMETER_Ripple_setFloatValueFromGUIChange( spanVSrc_p.parametricObject.bounds.size.x*2.1f);
				if (repeaterToolV != null)
					repeaterToolV.P_Bay.initiatePARAMETER_Ripple_setFloatValueFromGUIChange( spanVSrc_p.parametricObject.bounds.size.z*2.1f);
				break;
			}
			 
		}

		public override void deleteRequested()
		{
			// CLEANUP REPEATER TOOLS
			if (repeaterToolU 			!= null && 	!repeaterToolU.hasMoreThanOneDependent())
				parametricObject.model.deletePO(repeaterToolU.parametricObject);

			if (repeaterToolV 			!= null && 	!repeaterToolV.hasMoreThanOneDependent())
				parametricObject.model.deletePO(repeaterToolV.parametricObject);

			// CLEANUP JITTER PO's
			if (jitterTranslationTool 	!= null && 	!jitterTranslationTool.hasMoreThanOneDependent())
				parametricObject.model.deletePO(jitterTranslationTool.parametricObject);

			if (jitterRotationTool 		!= null && 	!jitterRotationTool.hasMoreThanOneDependent())
				parametricObject.model.deletePO(jitterRotationTool.parametricObject);

			if (jitterScaleTool 		!= null && 	!jitterScaleTool.hasMoreThanOneDependent())
				parametricObject.model.deletePO(jitterScaleTool.parametricObject);

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
				return localMatrixFromAddress(RepeaterItem.Node, indexU, indexV) * optionalLocalM;

			if (meshInput == "spanU" && spanUSrc_p!= null && caller == spanUSrc_p.Parent)
				return localMatrixFromAddress(RepeaterItem.SpanU, indexU, indexV) * optionalLocalM;



			return Matrix4x4.identity;
		} 


		/*
		public virtual Matrix4x4 localMatrixFromAddress(RepeaterItem ri, int i=0, int j=0)
		{
			return Matrix4x4.identity;

		}
		*/
		public virtual Matrix4x4 localMatrixFromAddress(RepeaterItem ri, int i=0, int j=0, int k=0)
		{
			return Matrix4x4.identity;

		}




				
		// GENERATE REPEATER_BASE
		public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool isReplica)
		{
			//Debug.Log("repeaterToolU="+repeaterToolU+", repeaterToolV="+repeaterToolV);

			if (parametricObject == null)
				return null;

			if (! parametricObject.isActive)
				return null;

			if (repeaterToolU == null && repeaterToolV == null)
				return null;


			preGenerate();


			RepeaterTool repeaterTool = ((zAxis) ? repeaterToolV : repeaterToolU) as RepeaterTool;
				


			//Terrain terrain = parametricObject.terrain; ///Terrain.activeTerrain;
		

			// NODE_MESH
			AXParametricObject 	nodeSrc_po 		= null;
			GameObject nodePlugGO 					= null;

			if (nodeSrc_p != null)
			{
				nodeSrc_po 		= nodeSrc_p.parametricObject;
				if (makeGameObjects && ! parametricObject.combineMeshes)
					nodePlugGO = nodeSrc_po.generator.generate(true,  initiator_po,  isReplica);
			}	

			// CELL_MESH
			AXParametricObject 	cellSrc_po 	= null;
			GameObject cellPlugGO 	= null;
			if (cellSrc_p != null)
			{
				cellSrc_po 	= cellSrc_p.parametricObject;
				if (makeGameObjects && ! parametricObject.combineMeshes)
					cellPlugGO = cellSrc_po.generator.generate(true,  initiator_po,  isReplica);				
			}

			// SPAN_MESH		
			AXParametricObject 	spanSrc_po 	= null;
			GameObject spanPlugGO 	= null;

			if (spanUSrc_p != null)
			{
				spanSrc_po 	= spanUSrc_p.parametricObject;
				if (makeGameObjects && ! parametricObject.combineMeshes)
					spanPlugGO = spanSrc_po.generator.generate(true,  initiator_po,  isReplica);				
			}
			


			if (nodeSrc_po == null && spanSrc_po == null && cellSrc_po == null)
			{
				if (P_Output != null)
					P_Output.meshes = null;
				
				return null;
			}


			GameObject go 		= null;
			if (makeGameObjects && ! parametricObject.combineMeshes)
				go = ArchimatixUtils.createAXGameObject(parametricObject.Name, parametricObject);



			//AXParameter bounding_p = null;
			//Paths boundingSolids = null;
			//Paths boundingHoles = null;
		
			


			List<AXMesh> 	ax_meshes 		= new List<AXMesh>();

			Matrix4x4 localPlacement_mx = Matrix4x4.identity;

			// -----------------------------------

			
			int max_reps = 150;

			int 	cellsU 		= Mathf.Clamp(repeaterTool.cells, 1, max_reps);
			float 	actualBayU 	= repeaterTool.actualBay;

			shiftU = -cellsU * actualBayU / 2;

			AXMesh tmpMesh;


			
			// BAY SPAN
			// Spanners are meshers that get replicated and sized to fit the bay...

			// prepare mesh to iterate in each direction

			List<AXMesh> 	ax_meshes_X 		= new List<AXMesh>();



			if (spanUSrc_p != null)
			{
				ax_meshes_X = spanUSrc_p.meshes;
			}



			/* NEED TO INTEGRATE THIS BACK IN IN THE FUTRE...
			if (cell_center_source != null)
			{
				// Y-AXIS
				// For now, only set the boundaries. 
				// Perhaps later, may want to set other like controls as in Replicant
				// 1. cache source object
				cell_center_source.cacheParameterValues();



				//bay_center_source.propagateParameterByBinding(1, bayx);
				//bay_center_source.propagateParameterByBinding(3, bayz);
				
				// 2. re_generate with temporary values set by this Replicant
				cell_center_source.generateOutputNow (makeGameObjects, parametricObject);
				
				// 3. Now that the bay_span_source has been regenerted,  grab the meshes from the input sources and add them here
				AXParameter bc_output_p = cell_center_source.getParameter("Output Mesh");
				foreach (AXMesh amesh in bc_output_p.meshes)
					ax_meshes_Y.Add (amesh.Clone(amesh.transMatrix));
				
				// 4. restore source object; as though we were never here!
				cell_center_source.revertParametersFromCache();

			}
			*/



             
			// BOUNDING

			int boundingObjectCount = 0;

			if (nodeSrc_po != null && nodeSrc_p.meshes != null && nodeSrc_p.meshes.Count > 0)
			{
				//boundingObjectCount += cellsU+1;
				boundingObjectCount += cellsU;
			}
			if (cellSrc_p  != null && cellSrc_p.meshes != null && cellSrc_p.meshes.Count > 0)
				boundingObjectCount += cellsU;

			if (spanUSrc_p != null && spanUSrc_p.meshes != null && spanUSrc_p.meshes.Count > 0)
				boundingObjectCount += cellsU+1;

		
			CombineInstance[] boundsCombinator = new CombineInstance[boundingObjectCount];




            //Debug.Log("boundingObjectCount="+)


            for (int i=0; i<=cellsU; i++) 
			{

				//float terrainY = 0;


				// NODES
				if (nodeSrc_po != null && nodeSrc_p.meshes != null && (i<cellsU || shouldDoLastItem(RepeaterItem.Node)))
				{
					string this_address = "node_"+i;

					// LOCAL PLACEMENT

					localPlacement_mx = localMatrixFromAddress(RepeaterItem.Node, i);


					// AX_MESHES

					for (int mi = 0; mi < nodeSrc_p.meshes.Count; mi++) {
						AXMesh dep_amesh = nodeSrc_p.meshes [mi];
						tmpMesh = dep_amesh.Clone (localPlacement_mx * dep_amesh.transMatrix);
						tmpMesh.subItemAddress = this_address;
						ax_meshes.Add (tmpMesh);
					}



					// BOUNDING MESHES
					if (boundsCombinator.Length > i)
					{
						boundsCombinator[i].mesh 		= nodeSrc_po.boundsMesh;
						boundsCombinator[i].transform 	= localPlacement_mx * nodeSrc_po.generator.localMatrixWithAxisRotationAndAlignment;
					}




					// GAME_OBJECTS
					
					if (nodePlugGO != null  && makeGameObjects && ! parametricObject.combineMeshes)
					{
						Matrix4x4  mx 		=   localPlacement_mx * nodeSrc_po.generator.localMatrixWithAxisRotationAndAlignment;
						GameObject copyGO 	= (GameObject) GameObject.Instantiate(nodePlugGO, AXUtilities.GetPosition(mx), AXUtilities.QuaternionFromMatrix(mx));


						Vector3 newJitterScale = jitterScale + Vector3.one;
						copyGO.transform.localScale= new Vector3(copyGO.transform.localScale.x*newJitterScale.x, copyGO.transform.localScale.y*newJitterScale.y, copyGO.transform.localScale.z*newJitterScale.z);
						copyGO.transform.localScale += jitterScale;

						AXGameObject axgo = copyGO.GetComponent<AXGameObject>();
						if (axgo != null)
							axgo.consumerAddress = this_address;

						copyGO.name = copyGO.name+"_" + this_address;
						copyGO.transform.parent = go.transform;
					}
				} // \NODES
				



				// CELL CENTERS
				if (cellSrc_p != null && cellSrc_p.meshes != null    && i<cellsU )
				{
					string this_address = "cell_"+i;


					// LOCAL_PLACEMENT
					localPlacement_mx = localMatrixFromAddress(RepeaterItem.Cell, i);


					// ACTUAL MESHES

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

					if (cellPlugGO != null && makeGameObjects && ! parametricObject.combineMeshes)
					{
						//Matrix4x4 mx = localPlacement_mx  * parametricObject.getTransMatrix() * source.getTransMatrix();
						Matrix4x4 mx =      localPlacement_mx * cellSrc_po.generator.localMatrixWithAxisRotationAndAlignment;
						GameObject copyGO = (GameObject) GameObject.Instantiate(cellPlugGO, AXUtilities.GetPosition(mx), AXUtilities.QuaternionFromMatrix(mx));

						Vector3 newJitterScale = jitterScale + Vector3.one;
						copyGO.transform.localScale= new Vector3(copyGO.transform.localScale.x*newJitterScale.x, copyGO.transform.localScale.y*newJitterScale.y, copyGO.transform.localScale.z*newJitterScale.z);
						copyGO.transform.localScale += jitterScale;

						AXGameObject axgo = copyGO.GetComponent<AXGameObject>();
						if (axgo != null)
							axgo.consumerAddress = this_address;
						
						copyGO.name = copyGO.name+"_" + this_address;
						copyGO.transform.parent = go.transform;
					}
				} // \CELLS
	
					
					

				// SPANS
						
				if (spanUSrc_p != null && spanUSrc_p.meshes != null && i<cellsU)
				{
					string this_address = "spanU_"+i;
					// X-AXIS
					
					// LOCAL_PLACEMENT

					localPlacement_mx = localMatrixFromAddress(RepeaterItem.SpanU, i);



					// AX_MESHES
																																					
					for (int mi = 0; mi < ax_meshes_X.Count; mi++) {
						AXMesh dep_amesh = ax_meshes_X [mi];
						tmpMesh = dep_amesh.Clone (localPlacement_mx * dep_amesh.transMatrix);
						tmpMesh.subItemAddress = this_address;
						ax_meshes.Add (tmpMesh);
					}



                    // BOUNDING MESHES
                    if (boundsCombinator.Length > i)
                    {

                        boundsCombinator[i].mesh = spanUSrc_po.boundsMesh;
                        boundsCombinator[i].transform = localPlacement_mx * spanUSrc_po.generator.localMatrixWithAxisRotationAndAlignment;
                    }


					// GAME_OBJECTS

					if (spanPlugGO != null && makeGameObjects && ! parametricObject.combineMeshes)
					{
						Matrix4x4 mx =      localPlacement_mx * spanSrc_po.generator.localMatrixWithAxisRotationAndAlignment;
						GameObject copyGO = (GameObject) GameObject.Instantiate(spanPlugGO, AXUtilities.GetPosition(mx), AXUtilities.QuaternionFromMatrix(mx));

						Vector3 newJitterScale = jitterScale + Vector3.one;
						copyGO.transform.localScale= new Vector3(copyGO.transform.localScale.x*newJitterScale.x, copyGO.transform.localScale.y*newJitterScale.y, copyGO.transform.localScale.z*newJitterScale.z);
						copyGO.transform.localScale += jitterScale;


						AXGameObject axgo = copyGO.GetComponent<AXGameObject>();
						if (axgo != null)
							axgo.consumerAddress = this_address;
												
						copyGO.name = copyGO.name+"_" + this_address;

						copyGO.transform.parent = go.transform;
						
					}
					
				} // \SPANS





					


										

	

			
				
			} //i


			GameObject.DestroyImmediate(nodePlugGO);
			GameObject.DestroyImmediate(cellPlugGO);
			GameObject.DestroyImmediate(spanPlugGO);


			parametricObject.finishMultiAXMeshAndOutput(ax_meshes, isReplica);


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


	}













	// REPEATER
	public class Repeater : RepeaterBase
	{


		// INPUTS



		// WORKING FIELDS (Updated every Generate)




		public float rnd_rotY;
		


		public override void init_parametricObject() 
		{
			base.init_parametricObject();

			// parameters
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Input, "Node Mesh"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Input, "Cell Mesh"));
			initSpanParameters();

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.JitterTool,   AXParameter.ParameterType.Input, "Jitter Translation"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.JitterTool,   AXParameter.ParameterType.Input, "Jitter Rotation"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.JitterTool,   AXParameter.ParameterType.Input, "Jitter Scale"));

			initRepeaterTools();

			parametricObject.addSplineParameter("Bounding Shape");

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Bool, "Bounding Is Void"));

			P_ProgressiveRotationX = parametricObject.addParameter(AXParameter.DataType.Float, 	"IncrRotX", 0f); 
			P_ProgressiveRotationY = parametricObject.addParameter(AXParameter.DataType.Float, 	"IncrRotY", 0f); 
			P_ProgressiveRotationZ = parametricObject.addParameter(AXParameter.DataType.Float, 	"IncrRotZ", 0f); 


			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Output,"Output Mesh"));

			// handles

		}

		public virtual void initSpanParameters() 
		{
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Input, "SpanU Mesh"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Input, "SpanV Mesh"));

		}
		public virtual void initRepeaterTools() 
		{
			P_RepeaterU = parametricObject.addParameter(new AXParameter(AXParameter.DataType.RepeaterTool, AXParameter.ParameterType.Input, "RepeaterU"));
			AXParametricObject repeaterToolU =  parametricObject.model.createNode("RepeaterTool");
			repeaterToolU.rect.x = parametricObject.rect.x-200;
			repeaterToolU.isOpen = false;
			//repeaterToolU.intValue("Edge_Count", 2);
			P_RepeaterU.makeDependentOn(repeaterToolU.getParameter("Output"));

			P_RepeaterV = parametricObject.addParameter(new AXParameter(AXParameter.DataType.RepeaterTool, AXParameter.ParameterType.Input, "RepeaterV"));
			AXParametricObject repeaterToolV =  parametricObject.model.createNode("RepeaterTool");
			repeaterToolV.rect.x = parametricObject.rect.x-200;
			repeaterToolV.isOpen = false;
			//repeaterToolV.intValue("Edge_Count", 2);
			P_RepeaterV.makeDependentOn(repeaterToolV.getParameter("Output"));
		}



	
		// POLL INPUTS (only on graph change())
		public override void pollInputParmetersAndSetUpLocalReferences()
		{
			//Debug.Log("Repeater::pollInputParmetersAndSetUpLocalReferences " + parametricObject.Name + " parametersHaveBeenPolled="+parametersHaveBeenPolled);

			base.pollInputParmetersAndSetUpLocalReferences();


			P_BoundingShape = parametricObject.getParameter("Bounding Shape");
			P_BoundingIsVoid = parametricObject.getParameter("Bounding Is Void");

			setupRepeaters();
		}



		// POLL CONTROLS (every model.generate())
		public override void pollControlValuesFromParmeters()
		{
			base.pollControlValuesFromParmeters();

			boundingIsVoid = (P_BoundingIsVoid != null) ? P_BoundingIsVoid.boolval : false;

		}



		public override void parameterWasModified(AXParameter p)
		{
			switch(p.Name)
			{
				case "zAxis":
				zAxis = P_zAxis.boolval;
					setupRepeaters();
					break;
			}
		}


		public void setupRepeaters()
		{
			AXParameter input_pU = null;
			AXParameter input_pV = null;



			if (! parametricObject.boolValue("zAxis")) // Use boolValue() since the working variable zAxis has not been set yet...
			{
				input_pU = P_RepeaterU;
				input_pV = P_RepeaterV;
			}
			else
			{
				input_pV = P_RepeaterU;
				input_pU = P_RepeaterV;
			}


			if (input_pU != null)
				repeaterToolU = (input_pU != null && input_pU.DependsOn != null) ? input_pU.DependsOn.parametricObject.generator as RepeaterTool	: null;
			else 
				repeaterToolU = null;

			if (input_pV != null)
				repeaterToolV = (input_pV != null && input_pV.DependsOn != null) ? input_pV.DependsOn.parametricObject.generator as RepeaterTool	: null;
			else 
				repeaterToolV = null;

		}











		
		








		public override void connectionBrokenWith(AXParameter p)
		{
			base.connectionBrokenWith(p);

			switch (p.Name)
			{
				
			case "Node Mesh":
				nodeSrc_p = null;
				break;
			case "Cell Mesh":
				cellSrc_p = null;
				break;
			case "Bay Span":
				spanUSrc_p = null;
				break;

			case "Jitter Translation":
				jitterTranslationTool = null;
				break;
			case "Jitter Rotation":
				jitterRotationTool = null;
				break;
			case "Jitter Scale":
				jitterScaleTool = null;
				break;

			case "RepeaterU":
				repeaterToolU = null;
				break;
			case "RepeaterV":
				repeaterToolV = null;
				break;

			}
		}



		public virtual string getDefaultAddress()
		{
			return "0:0:0";
		}







		public override Matrix4x4 getLocalConsumerMatrixPerInputSocket(AXParametricObject caller)
		{
			// use shift too
			// use caller address
			if (caller == null)
				return Matrix4x4.identity;


			// PLAN
			if (caller == boundingShapeSrc_po)
				return  Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90, 0, 0), Vector3.one);



			Matrix4x4 optionalLocalM = Matrix4x4.identity;

			if (caller.selectedConsumer == null || caller.selectedConsumer != this.parametricObject || String.IsNullOrEmpty(caller.selectedConsumerAddress))
			{	

				caller.selectedConsumerAddress = "node_0_0";
				optionalLocalM = caller.localMatrix * caller.getAxisRotationMatrix().inverse;
			}

			string[] address = caller.selectedConsumerAddress.Split('_');
			 

			if (address.Length < 2) 
				return Matrix4x4.identity;

			string meshInput = address[0]; // e.g., "node", "cell", "spanU", "spanV"


			int indexU = int.Parse(address[1]);
			int indexV = 0;

			if (address.Length > 2)
				indexV = int.Parse(address[2]);
			
			if (repeaterToolU != null) 
			{
				if (indexU > repeaterToolU.cells)
					indexU = repeaterToolU.cells-1;
				if (indexU > repeaterToolU.edgeCount && indexU<repeaterToolU.cells-repeaterToolU.edgeCount)
					indexU = repeaterToolU.edgeCount;
			} 
			if (repeaterToolV != null)
			{
				if (indexV > repeaterToolV.cells)
					indexV = repeaterToolV.cells-1;
				if (indexV > repeaterToolV.edgeCount && indexV<repeaterToolV.cells-repeaterToolV.edgeCount)
					indexV = repeaterToolV.edgeCount;
			}

			if (repeaterToolU != null && repeaterToolV != null && indexU > repeaterToolU.edgeCount && indexU<repeaterToolU.cells-repeaterToolU.edgeCount  && indexV > repeaterToolV.edgeCount && indexV<repeaterToolV.cells-repeaterToolV.edgeCount)
			{
				if (repeaterToolV.cells > repeaterToolU.cells)
					indexU = repeaterToolU.edgeCount;
				else
					indexV = repeaterToolV.edgeCount; 
			}


			// Find out which input this caller is fed into Node Mesh, Bay Span or Cell Mesh?

			if (meshInput == "node" && nodeSrc_p != null && caller == nodeSrc_p.Parent)
				return localNodeMatrixFromAddress(indexU, indexV) * optionalLocalM;

			else if (meshInput == "cell" && cellSrc_p != null && caller == cellSrc_p.Parent) 
				return localCellMatrixFromAddress(indexU, indexV);

			else if (meshInput == "spanU" && spanUSrc_p != null && caller == spanUSrc_p.Parent)
				return localSpanUMatrixFromAddress(indexU, indexV);

			else if (meshInput == "spanV" && spanUSrc_p != null && caller == spanUSrc_p.Parent)
				return localSpanVMatrixFromAddress(indexU, indexV);

			return Matrix4x4.identity;
		} 



		public virtual Matrix4x4 localNodeMatrixFromAddress(int i=0, int j=0, int k=0)
		{
			float perlinTranslation 	= 1;
			float perlinRot 			= 1;
			float perlinScale 			= 1;

			float actualBayU = (repeaterToolU != null) ? repeaterToolU.actualBay : 0;
			float actualBayV = (repeaterToolV != null) ? repeaterToolV.actualBay : 0;

			if (zAxis)
			{
				int tmpi = i;
				i = j;
				j = tmpi;
			}
			float shiftUU = (zAxis) ? shiftV : shiftU;
			float shiftVV = (zAxis) ? shiftU : shiftV;

			float x = actualBayU * i;
			float y = actualBayV * j;



			if (jitterTranslationTool 	!= null)
				perlinTranslation 		=  Mathf.PerlinNoise( (x+jitterTranslationTool.offset) * jitterTranslationTool.perlinScale,  	(y+jitterTranslationTool.offset) * jitterTranslationTool.perlinScale);

			if (jitterRotationTool 		!= null)
				perlinRot 				=  Mathf.PerlinNoise( (x+jitterRotationTool.offset) * jitterRotationTool.perlinScale,  	(y+jitterRotationTool.offset) * jitterRotationTool.perlinScale);

			if (jitterScaleTool 		!= null) 
				perlinScale 			= Mathf.PerlinNoise ( (x+jitterScaleTool.offset) * jitterScaleTool.perlinScale, 		(y+jitterScaleTool.offset) * jitterScaleTool.perlinScale);

			// TRANSLATION 	*********
			Vector3 translate = new Vector3((x+shiftUU), 0, (y+shiftVV));

			if (jitterTranslationTool != null)
				translate += new Vector3( perlinTranslation*jitterTranslationTool.x-jitterTranslationTool.x/2 , perlinTranslation*jitterTranslationTool.y-jitterTranslationTool.y/2, perlinTranslation*jitterTranslationTool.z-jitterTranslationTool.z/2);

			Terrain terrain = parametricObject.terrain;
			if (terrain != null)
				translate.y += terrain.SampleHeight(parametricObject.model.gameObject.transform.TransformPoint(parametricObject.localMatrix.MultiplyPoint(translate)));
			 

			// ROTATION		*********
			Quaternion 	rotation = Quaternion.identity;// Quaternion.Euler (0, roty, 0);

			if (jitterRotationTool != null && ! float.IsNaN(perlinRot))
				rotation = Quaternion.Euler ((perlinRot*jitterRotationTool.x - jitterRotationTool.x/2)*1.5f, (perlinRot*jitterRotationTool.y - jitterRotationTool.y/2), (perlinRot*jitterRotationTool.z - jitterRotationTool.z/2)) * rotation;

			if (! quaternionIsValid(rotation))
				rotation = Quaternion.identity;
			//Debug.Log (jitterRotationTool.y);
			//Debug.Log (perlinRot);
			//Debug.Log (perlinRot*jitterRotationTool.y - jitterRotationTool.y/2);

			// SCALE		**********
			jitterScale = new Vector3 (scaleX, scaleY, scaleZ);

			if (jitterScaleTool != null)
			{
				// X
				if (jitterScaleTool.x < scaleX)
					jitterScale.x = jitterScale.x + perlinScale * jitterScaleTool.x - jitterScaleTool.x / 2;
				else
					jitterScale.x = jitterScale.x/2 +perlinScale * jitterScaleTool.x;

				// Y
				if (jitterScaleTool.y < scaleY)
					jitterScale.y = jitterScale.y + perlinScale * jitterScaleTool.y - jitterScaleTool.y / 2;
				else
					jitterScale.y = jitterScale.y/2 +perlinScale * jitterScaleTool.y;

				// Z
				if (jitterScaleTool.z < scaleZ)
					jitterScale.z = jitterScale.z + perlinScale * jitterScaleTool.z - jitterScaleTool.z / 2;
				else
					jitterScale.z = jitterScale.z/2 +perlinScale * jitterScaleTool.z;
			}




			// ******** USE A FUNCTION HERE TO GET FOR HANDLES AS WELL


				

			return   Matrix4x4.TRS(translate, rotation, jitterScale) * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler((i + j) * progressiveRotationX, (i + j) * progressiveRotationY, (i + j) * progressiveRotationZ), Vector3.one);

		}


		public virtual Matrix4x4 localCellMatrixFromAddress(int i=0, int j=0, int k=0)
		{
			float perlinTranslation 	= 1;
			float perlinRot 			= 1;
			float perlinScale 			= 1;

			float actualBayU = (repeaterToolU != null) ? repeaterToolU.actualBay : 0;
			float actualBayV = (repeaterToolV != null) ? repeaterToolV.actualBay : 0;

			if (zAxis)
			{
				int tmpi = i;
				i = j;
				j = tmpi;
			}
			float shiftUU = (zAxis) ? shiftV : shiftU;
			float shiftVV = (zAxis) ? shiftU : shiftV;


			float x = (i + .5f) * actualBayU;
			float y = (j + .5f) * actualBayV;


			// GENERATE PERLIN VALUES

			if (jitterTranslationTool != null)
				perlinTranslation 	=  Mathf.PerlinNoise( x * jitterTranslationTool.perlinScale,  	y  * jitterTranslationTool.perlinScale);

			if (jitterRotationTool != null)
				perlinRot 			=  Mathf.PerlinNoise( x * jitterRotationTool.perlinScale,  		y * jitterRotationTool.perlinScale);

			if (jitterScaleTool != null)
				perlinScale 		=  Mathf.PerlinNoise( x * jitterScaleTool.perlinScale,  		y * jitterScaleTool.perlinScale);


			// TRANSLATION 	*********

			Vector3 translate = new Vector3((x+shiftUU), 0, ((j>-1) ? (y+shiftVV) : 0) );

			// -- JITTER
			if (jitterTranslationTool != null)
				translate += new Vector3( perlinTranslation*jitterTranslationTool.x-jitterTranslationTool.x/2 , perlinTranslation*jitterTranslationTool.y-jitterTranslationTool.y/2, perlinTranslation*jitterTranslationTool.z-jitterTranslationTool.z/2);

			// -- TERRAIN
			Terrain terrain = parametricObject.terrain;//Terrain.activeTerrain;
			if (terrain != null)
				translate.y += terrain.SampleHeight(parametricObject.model.gameObject.transform.TransformPoint(parametricObject.localMatrix.MultiplyPoint(translate)));


			// ROTATION		*********
			Quaternion 	rotation = Quaternion.identity;// Quaternion.Euler (0, roty, 0);
			if (jitterRotationTool != null)
				rotation = Quaternion.Euler ((perlinRot*jitterRotationTool.x - jitterRotationTool.x/2)*1.5f, (perlinRot*jitterRotationTool.y - jitterRotationTool.y/2), (perlinRot*jitterRotationTool.z - jitterRotationTool.z/2)) * rotation;



			// SCALE		**********
			jitterScale = new Vector3 (scaleX, scaleY, scaleZ);

			if (jitterScaleTool != null)
			{
				// X
				if (jitterScaleTool.x < scaleX)
					jitterScale.x = jitterScale.x + perlinScale * jitterScaleTool.x - jitterScaleTool.x / 2;
				else
					jitterScale.x = jitterScale.x/2 +perlinScale * jitterScaleTool.x;

				// Y
				if (jitterScaleTool.y < scaleY)
					jitterScale.y = jitterScale.y + perlinScale * jitterScaleTool.y - jitterScaleTool.y / 2;
				else
					jitterScale.y = jitterScale.y/2 +perlinScale * jitterScaleTool.y;

				// Z
				if (jitterScaleTool.z < scaleZ)
					jitterScale.z = jitterScale.z + perlinScale * jitterScaleTool.z - jitterScaleTool.z / 2;
				else
					jitterScale.z = jitterScale.z/2 +perlinScale * jitterScaleTool.z;
			}

			// ******** USE A FUNCTION HERE TO GET FOR HANDLES AS WELL
			return   Matrix4x4.TRS(translate, rotation, jitterScale) * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler((i + j) * progressiveRotationX, (i + j) * progressiveRotationY, (i + j) * progressiveRotationZ), Vector3.one);
		}


		public virtual Matrix4x4 localSpanUMatrixFromAddress(int i=0, int j=0, int k=0)
		{
			float perlinTranslation 	= 1;
			float perlinRot 			= 1;
			float perlinScale 			= 1;

			float actualBayU = (repeaterToolU != null) ? repeaterToolU.actualBay : 0;
			float actualBayV = (repeaterToolV != null) ? repeaterToolV.actualBay : 0;

			if (zAxis)
			{
				int tmpi = i;
				i = j;
				j = tmpi;
			}
			float shiftUU = (zAxis) ? shiftV : shiftU;
			float shiftVV = (zAxis) ? shiftU : shiftV;


			float x = (i + .5f) * actualBayU;
			float y = (j ) * actualBayV;


			// GENERATE PERLIN VALUES

			if (jitterTranslationTool != null)
				perlinTranslation 	=  Mathf.PerlinNoise( x * jitterTranslationTool.perlinScale,  	y  * jitterTranslationTool.perlinScale);

			if (jitterRotationTool != null)
				perlinRot 			=  Mathf.PerlinNoise( x * jitterRotationTool.perlinScale,  		y * jitterRotationTool.perlinScale);

			if (jitterScaleTool != null)
				perlinScale 		=  Mathf.PerlinNoise( x * jitterScaleTool.perlinScale,  		y * jitterScaleTool.perlinScale);


			// TRANSLATION 	*********

			Vector3 translate = new Vector3(x+shiftUU, 0, ((j>-1) ? y+shiftVV : 0) );

			// -- JITTER
			if (jitterTranslationTool != null)
				translate += new Vector3( perlinTranslation*jitterTranslationTool.x-jitterTranslationTool.x/2 , perlinTranslation*jitterTranslationTool.y-jitterTranslationTool.y/2, perlinTranslation*jitterTranslationTool.z-jitterTranslationTool.z/2);

			// -- TERRAIN
			Terrain terrain = parametricObject.terrain;//Terrain.activeTerrain;
			if (terrain != null)
				translate.y += terrain.SampleHeight(parametricObject.model.gameObject.transform.TransformPoint(parametricObject.localMatrix.MultiplyPoint(translate)));


			// ROTATION		*********
			Quaternion 	rotation = Quaternion.identity;// Quaternion.Euler (0, roty, 0);
			if (jitterRotationTool != null)
				rotation = Quaternion.Euler ((perlinRot*jitterRotationTool.x - jitterRotationTool.x/2)*1.5f, (perlinRot*jitterRotationTool.y - jitterRotationTool.y/2), (perlinRot*jitterRotationTool.z - jitterRotationTool.z/2)) * rotation;



			// SCALE		**********
			jitterScale = new Vector3 (scaleX, scaleY, scaleZ);

			if (jitterScaleTool != null)
			{
				// X
				if (jitterScaleTool.x < scaleX)
					jitterScale.x = jitterScale.x + perlinScale * jitterScaleTool.x - jitterScaleTool.x / 2;
				else
					jitterScale.x = jitterScale.x/2 +perlinScale * jitterScaleTool.x;

				// Y
				if (jitterScaleTool.y < scaleY)
					jitterScale.y = jitterScale.y + perlinScale * jitterScaleTool.y - jitterScaleTool.y / 2;
				else
					jitterScale.y = jitterScale.y/2 +perlinScale * jitterScaleTool.y;

				// Z
				if (jitterScaleTool.z < scaleZ)
					jitterScale.z = jitterScale.z + perlinScale * jitterScaleTool.z - jitterScaleTool.z / 2;
				else
					jitterScale.z = jitterScale.z/2 +perlinScale * jitterScaleTool.z;
			}


			// ******** USE A FUNCTION HERE TO GET FOR HANDLES AS WELL
			return   Matrix4x4.TRS(translate, rotation, jitterScale) * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler((i + j) * progressiveRotationX, (i + j) * progressiveRotationY, (i + j) * progressiveRotationZ), Vector3.one);
		}

		public virtual Matrix4x4 localSpanVMatrixFromAddress(int i=0, int j=0, int k=0)
		{
			float perlinTranslation 	= 1;
			float perlinRot 			= 1;
			float perlinScale 			= 1;

			float actualBayU = (repeaterToolU != null) ? repeaterToolU.actualBay : 0;
			float actualBayV = (repeaterToolV != null) ? repeaterToolV.actualBay : 0;

			if (zAxis)
			{
				int tmpi = i;
				i = j;
				j = tmpi;
			}
			float shiftUU = (zAxis) ? shiftV : shiftU;
			float shiftVV = (zAxis) ? shiftU : shiftV;

			float x = (i ) * actualBayU;
			float y = (j+.5f) * actualBayV;

			// GENERATE PERLIN VALUES

			if (jitterTranslationTool != null)
				perlinTranslation 	=  Mathf.PerlinNoise( x * jitterTranslationTool.perlinScale,  	(y*actualBayV)  * jitterTranslationTool.perlinScale);
				 
			if (jitterRotationTool != null)
				perlinRot 			=  Mathf.PerlinNoise( x * actualBayU * jitterRotationTool.perlinScale,  		(y * actualBayV) * jitterRotationTool.perlinScale);

			if (jitterScaleTool != null)
				perlinScale 		=  Mathf.PerlinNoise( x * actualBayU * jitterScaleTool.perlinScale,  			(y * actualBayV) * jitterScaleTool.perlinScale);


			// TRANSLATION 	*********
			Vector3 translate = new Vector3(((i>-1) ? x+shiftUU : 0) , 0,  y+shiftVV);

			//Vector3 translate = new Vector3((i*actualBayU+shiftUU), 0, ((j>-1) ? (j*actualBayV+shiftVV+actualBayV/2) : 0) );

			// -- JITTER
			if (jitterTranslationTool != null)
				translate += new Vector3( perlinTranslation*jitterTranslationTool.x-jitterTranslationTool.x/2 , perlinTranslation*jitterTranslationTool.y-jitterTranslationTool.y/2, perlinTranslation*jitterTranslationTool.z-jitterTranslationTool.z/2);

			// -- TERRAIN
			Terrain terrain = parametricObject.terrain;//Terrain.activeTerrain;
			if (terrain != null)
				translate.y += terrain.SampleHeight(parametricObject.model.gameObject.transform.TransformPoint(parametricObject.localMatrix.MultiplyPoint(translate)));


			// ROTATION		*********
			Quaternion 	rotation = Quaternion.identity;// Quaternion.Euler (0, roty, 0);
			if (jitterRotationTool != null)
				rotation = Quaternion.Euler ((perlinRot*jitterRotationTool.x - jitterRotationTool.x/2)*1.5f, (perlinRot*jitterRotationTool.y - jitterRotationTool.y/2), (perlinRot*jitterRotationTool.z - jitterRotationTool.z/2)) * rotation;



			// SCALE		**********
			jitterScale = new Vector3 (scaleX, scaleY, scaleZ);

			if (jitterScaleTool != null)
			{
				// X
				if (jitterScaleTool.x < scaleX)
					jitterScale.x = jitterScale.x + perlinScale * jitterScaleTool.x - jitterScaleTool.x / 2;
				else
					jitterScale.x = jitterScale.x/2 +perlinScale * jitterScaleTool.x;

				// Y
				if (jitterScaleTool.y < scaleY)
					jitterScale.y = jitterScale.y + perlinScale * jitterScaleTool.y - jitterScaleTool.y / 2;
				else
					jitterScale.y = jitterScale.y/2 +perlinScale * jitterScaleTool.y;

				// Z
				if (jitterScaleTool.z < scaleZ)
					jitterScale.z = jitterScale.z + perlinScale * jitterScaleTool.z - jitterScaleTool.z / 2;
				else
					jitterScale.z = jitterScale.z/2 +perlinScale * jitterScaleTool.z;
			}


			// ******** USE A FUNCTION HERE TO GET FOR HANDLES AS WELL
			return   Matrix4x4.TRS(translate, rotation, jitterScale) * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler((i + j) * progressiveRotationX, 90 + (i + j) * progressiveRotationY, (i + j) * progressiveRotationZ), Vector3.one);
		}

	}




	
	
	
	

	
	
	











	// FRAME_REPEATER
	
	public class FrameRepeater : Repeater, IRepeater
	{

		// INIT
		public override void init_parametricObject() 
		{
			base.init_parametricObject();



			// parameters
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, AXParameter.ParameterType.Input, "Profile Spline"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, AXParameter.ParameterType.Input, "Plan Spline"));
			
			
			// template parameters
			parametricObject.addParameter(AXParameter.DataType.Bool, "isFramed", true);
			
			AXParameter p = null;
			p = parametricObject.addParameter(AXParameter.DataType.Float, AXParameter.ParameterType.BoundsControl, "len_X", 10f, 0f, 100f);
			p.sizeBindingAxis = Axis.X;
			p = parametricObject.addParameter(AXParameter.DataType.Float, AXParameter.ParameterType.BoundsControl, "len_Y", 0f, 0f, 100f);
			p.sizeBindingAxis = Axis.Y;
			p = parametricObject.addParameter(AXParameter.DataType.Float, AXParameter.ParameterType.BoundsControl, "len_Z", 10f, 0f, 100f);
			p.sizeBindingAxis = Axis.Z;

			parametricObject.addParameter(AXParameter.DataType.Float, "bay_X", 3, .1f, 10f);
			parametricObject.addParameter(AXParameter.DataType.Float, "bay_Y", 3, .1f, 10f);
			parametricObject.addParameter(AXParameter.DataType.Float, "bay_Z", 3, .1f, 10f);


			parametricObject.addParameter(AXParameter.DataType.Float, "margin", 0.05f, .01f, 1f);
			parametricObject.addParameter(AXParameter.DataType.Float, "thick", .1f, .01f, 1f);
			


			// handles
			
		}





		
		/*
		// GENERATE FRAME_REPEATER
		public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool isReplica)
		{
				
			AXParameter input_plan_p = parametricObject.getParameter("Plan Spline");
			AXParameter input_sec_p = parametricObject.getParameter("Profile Spline");

			
			if (input_sec_p.DependsOn == null)
				return null;

			preGenerate();

			AXParameter sp = input_sec_p.DependsOn;
			
			
			
			
			float moldThick 		= parametricObject.floatValue("thick");
			float margin 			= parametricObject.floatValue("margin");
			
			bool isFramed 			= parametricObject.boolValue("isFramed");
			if (! isFramed)
				margin 		= 0;


			AXSpline s				= sp.getParentShiftedSpline();
			AXSpline profile 		= s.doubleInSymmetry(moldThick); 
			
			Matrix4x4 tmpMatrix = Matrix4x4.identity;

			
			lenx = parametricObject.floatValue("len_X");
			leny = parametricObject.floatValue("len_Y");
			lenz = parametricObject.floatValue("len_Z");

			float framex = lenx - margin*2;
			float framez = lenz - margin*2;


			bayx = parametricObject.floatValue("bay_X");
			bayy = parametricObject.floatValue("bay_Y");
			bayz = parametricObject.floatValue("bay_Z");
			
			//calculateInternalDependencies();
			
			cells = nbay_getActual(framex, 	bayx);
			nby = nbay_getActual(leny, 	bayy);
			nbz = nbay_getActual(framez, 	bayz);


			bayx = bay_getActual(framex, 	bayx);
			bayy = bay_getActual(leny, 	bayy);
			bayz = bay_getActual(framez, 	bayz);


			shiftx = -cells*bayx/2;
			shiftz = -nbz*bayz/2;


			int periCt_x = parametricObject.intValue("perimeter_X");
			int periCt_z = parametricObject.intValue("perimeter_Z");

			int combineCount = cells-1+nbz-1;

			if(isFramed)
				combineCount++;





			CombineInstance[] combine = new CombineInstance[combineCount*2];
			
			float rotx = -90;
			
			Quaternion rotation 	= Quaternion.Euler(	rotx, 	0, 	0); 
			Quaternion rot180 		= Quaternion.Euler(	rotx, 	180, 0);
			//rotation = Quaternion.identity;
			Vector3 scaler 		 = Vector3.one;
			
			AXExtrudeGenerator e = new AXExtrudeGenerator();
			if (isFramed)
			{
				//e.topCap = false;
				//e.botCap = false;
			}
			e.uScale =  parametricObject.floatValue("uScale");
			e.vScale =  parametricObject.floatValue("vScale");

			e.rotSidesTex = true;
			
			int meshCur = 0;
			
			Vector3 translate = Vector3.zero;



			// X AXIS
			float peri_x = bayx * periCt_x;
			float peri_z = bayz * periCt_z;

			Mesh mesh_x 	= e.generate(profile, framex);
			Mesh mesh_xp 	= e.generate(profile, peri_x);
			
			Mesh mesh_z 	= e.generate(profile, framez);
			Mesh mesh_zp 	= e.generate(profile, peri_z);



			for (int i=1; i<cells; i++) 
			{
				if (meshCur >= combine.Length)
					break;

				if (i <= periCt_x || i > cells - periCt_x-1)
				{
					translate = new Vector3(i*bayx + shiftx, 0, (framez + shiftz) );
					tmpMatrix = Matrix4x4.TRS(translate, rotation, scaler);
					combine[meshCur].mesh = mesh_z;
					combine[meshCur].transform = tmpMatrix;
					meshCur++;
				} 
				else
				{
					translate = new Vector3(i*bayx + shiftx, 0, (framez + shiftz) );
					tmpMatrix = Matrix4x4.TRS(translate, rotation, scaler);
					combine[meshCur].mesh = mesh_zp;
					combine[meshCur].transform = tmpMatrix;
					meshCur++;

					// accross the courtyard
					translate = new Vector3(i*bayx + shiftx, 0, (framez + shiftz) - bayz*(nbz-periCt_z) );
					tmpMatrix = Matrix4x4.TRS(translate, rotation, scaler);
					combine[meshCur].mesh = mesh_zp;
					combine[meshCur].transform = tmpMatrix;
					meshCur++;
				}
				 





				
			}
			
			
			
			// Z AXIS
			
			rotation 	=     Quaternion.Euler(	rotx, 	-90, 	0); 
			rot180 = Quaternion.Euler(	rotx, 	90, 0);
			
			mesh_x = e.generate(profile, framex);
			for (int k=1; k<nbz; k++) 
			{



				if (k <= periCt_z || k > nbz - periCt_z-1)
				{
					translate = new Vector3(shiftx, 0, -k*bayz - shiftz);
					tmpMatrix = Matrix4x4.TRS(translate, rotation, scaler);
					combine[meshCur].mesh = mesh_x;
					combine[meshCur].transform = tmpMatrix;
					meshCur++;
				} 
				else
				{
					translate = new Vector3(shiftx, 0, -k*bayz - shiftz);
					tmpMatrix = Matrix4x4.TRS(translate, rotation, scaler);
					combine[meshCur].mesh = mesh_xp;
					combine[meshCur].transform = tmpMatrix;
					meshCur++;


					// accross the courtyard
					translate = new Vector3(shiftx  + bayx*(cells-periCt_x), 0, -k*bayz - shiftz);
					tmpMatrix = Matrix4x4.TRS(translate, rotation, scaler);
					combine[meshCur].mesh = mesh_xp;
					combine[meshCur].transform = tmpMatrix;
					meshCur++;


				}






				
			}
			
			//Debug.Log("added " + Parent.MeshCount);
			
			Mesh mesh = null;

			if(isFramed)
			{
				
				AXSpline boundaryProfile = sp.spline.mirrorAndAddBacking(moldThick, margin); 
				
				// Perimeter
				AXSplineExtrudeGenerator spex = new AXSplineExtrudeGenerator();
				spex.uScale =  parametricObject.floatValue("uScale");
				spex.vScale =  parametricObject.floatValue("vScale");
				
				AXTurtle perim = new AXTurtle();
				perim.mov (shiftx, shiftz);
				perim.dir (90);
				perim.right(	framex);
				perim.fwd(		framez);
				perim.left(		framex);
				perim.s.isClosed = true;
				
				
				//Debug.Log (perim.s.);
				
				 
				tmpMatrix = Matrix4x4.identity;
				//tmpMatrix.SetTRS(new Vector3(0,0,shiftz), Quaternion.identity, scaler);
				
				//boundaryProfile.isClosed = false;
				
				//mesh = spex.generate(perim.s, boundaryProfile);
				mesh = spex.generate(perim.s, profile);
				//Parent.addMesh(mesh, tmpMatrix);

				if (meshCur < combine.Length)
				{
					combine[meshCur].mesh = mesh;
					combine[meshCur].transform = tmpMatrix;
				
				}
			}
			
			
			
			mesh = new Mesh();
			mesh.CombineMeshes(combine);

			parametricObject.finishSingleMeshAndOutput (mesh, Matrix4x4.identity);
			

			return null;
			
		}

		*/
		
	}


			
}