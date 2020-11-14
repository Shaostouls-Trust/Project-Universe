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




	
	// LINEAR_REPEATER
	
	public class LinearRepeater : Repeater, IRepeater
	{
		public override string GeneratorHandlerTypeName { get { return "LinearRepeaterHandler"; } }


		public RepeaterTool repeaterTool;


		AXParameter P_DoFirstNode;
		AXParameter P_DoLastNode;


		public bool doFirstNode;
		public bool doLastNode;




		// INIT_PARAMETRIC_OBJECT
		public override void init_parametricObject() 
		{
			base.init_parametricObject();

			parametricObject.addParameter(AXParameter.DataType.Bool, 	"zAxis", 		false);
			parametricObject.addParameter(AXParameter.DataType.Bool, 	"Do First Node", 	true);
			parametricObject.addParameter(AXParameter.DataType.Bool, 	"Do Last Node", 	true);

			P_ProgressiveRotationX = parametricObject.addParameter(AXParameter.DataType.Float, 	"IncrRotX", 0f); 
			P_ProgressiveRotationY = parametricObject.addParameter(AXParameter.DataType.Float, 	"IncrRotY", 0f); 
			P_ProgressiveRotationZ = parametricObject.addParameter(AXParameter.DataType.Float, 	"IncrRotZ", 0f); 

		} 


		public override void initSpanParameters() 
		{
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Input, "Bay SpanU"));
		}

		public override void initRepeaterTools() 
		{
			P_RepeaterU = parametricObject.addParameter(new AXParameter(AXParameter.DataType.RepeaterTool, AXParameter.ParameterType.Input, "RepeaterU"));
			AXParametricObject repeaterTool =  parametricObject.model.createNode("RepeaterTool");
			repeaterTool.rect.x = parametricObject.rect.x-200;
			repeaterTool.isOpen = false;
			repeaterTool.intValue("Edge_Count", 100);
			P_RepeaterU.makeDependentOn(repeaterTool.getParameter("Output"));
		}

		 

		
		public override void pollInputParmetersAndSetUpLocalReferences()
		{
			//Debug.Log("LinearRepeater::pollInputParmetersAndSetUpLocalReferences " + parametricObject.Name + " parametersHaveBeenPolled="+parametersHaveBeenPolled);
			base.pollInputParmetersAndSetUpLocalReferences();

			P_DoFirstNode = parametricObject.getParameter("Do First Node");
			P_DoLastNode  = parametricObject.getParameter("Do Last Node");
		}
	
		// POLL CONTROLS (every model.generate())
		public override void pollControlValuesFromParmeters()
		{
			base.pollControlValuesFromParmeters();

			if (zAxis)
				repeaterToolV = repeaterToolU;


			doFirstNode = (P_DoFirstNode != null) ? P_DoFirstNode.boolval : true;
			doLastNode  = (P_DoLastNode  != null) ? P_DoLastNode.boolval  : true;
 		}
		   

				
		// GENERATE LINEAR_REPEATER
		public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool isReplica)
		{
			//Debug.Log("repeaterToolU="+repeaterToolU+", repeaterToolV="+repeaterToolV);
		
			//Debug.Log("LINEAR REPEATER: Gentrate");



			if (parametricObject == null || ! parametricObject.isActive)
				return null;

			if (repeaterToolU == null && repeaterToolV == null)
				return null;


			preGenerate();


			repeaterTool = (zAxis) ? repeaterToolV : repeaterToolU;
				
			repeaterTool = repeaterToolU;



			//Terrain terrain = Terrain.activeTerrain;
		

			// NODE_MESH
			AXParametricObject 	nodeSrc_po 		= null;
			GameObject 			nodePlugGO 		= null;
			if (nodeSrc_p != null)
			{
				nodeSrc_po 		= nodeSrc_p.parametricObject;
				if (makeGameObjects && ! parametricObject.combineMeshes)
					nodePlugGO 	= nodeSrc_po.generator.generate(true,  initiator_po,  isReplica);
			}	

			// CELL_MESH
			AXParametricObject 	cellSrc_po 	= null;
			GameObject 			cellPlugGO 	= null;
			if (cellSrc_p != null)
			{
				cellSrc_po 		= cellSrc_p.parametricObject;
				if (makeGameObjects && ! parametricObject.combineMeshes)
					cellPlugGO 	= cellSrc_po.generator.generate(true,  initiator_po,  isReplica);				
			}

			// BAY_SPAN			
			AXParametricObject 	spanSrc_po 	= null;
			GameObject 			spanPlugGO 	= null;

			if (spanUSrc_p != null)
			{
				spanSrc_po 		= spanUSrc_p.parametricObject;
				if (makeGameObjects && ! parametricObject.combineMeshes)
					spanPlugGO 	= spanSrc_po.generator.generate(true,  initiator_po,  isReplica);				
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



			Paths boundingSolids = null;
			Paths boundingHoles = null;
		
			
				
			

			List<AXMesh> 	ax_meshes 		= new List<AXMesh>();

			Matrix4x4 localPlacement_mx = Matrix4x4.identity;

			// -----------------------------------

			
			int max_reps = 150;

			int 	cellsU 		= Mathf.Clamp(repeaterTool.cells, 1, max_reps);
			float 	actualBayU 	= repeaterTool.actualBay;

			if (float.IsNaN(actualBayU))
				return null;

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

			if (nodeSrc_po != null && nodeSrc_p.meshes != null)
			{
				boundingObjectCount += cellsU+1;

				if (! doFirstNode)
					boundingObjectCount--;

				if (! doLastNode)
					boundingObjectCount--;
			}


			else if (cellSrc_p  != null && cellSrc_p.meshes != null)
				boundingObjectCount += cellsU;

			if (spanUSrc_p != null && spanUSrc_p.meshes != null)
				boundingObjectCount += cellsU;

			CombineInstance[] boundsCombinator = new CombineInstance[boundingObjectCount];


			// FOR EACH ADDRESS 

			for (int i=0; i<=cellsU; i++) 
			{
				//Debug.Log("["+i+"] i*actualBay="+i*actualBay+", perval="+perlval);

				if(boundingSolids != null)
				{
					IntPoint ip = new IntPoint((i*repeaterToolU.actualBay+shiftU)*AXGeometry.Utilities.IntPointPrecision, 0);

					bool exclude = true;
										
					if (boundingSolids != null)
					{
						foreach (Path path in boundingSolids)
						{
							if (Clipper.PointInPolygon(ip, path) == 1 && Clipper.Orientation(path))
							{
								exclude = false;
								break;
							}
						}
					}

					if (boundingHoles != null)
					{
						foreach (Path hole in boundingHoles)
						{
							if (Clipper.PointInPolygon(ip, hole) == 1)
							{
								exclude = true;
								break;
							}
						}
					}

					if (exclude) 
						continue;
				}




				//Debug.Log(" ** ** ** * " + nodeSrc_p.meshes);

				// NODES
				if (nodeSrc_po != null && nodeSrc_p.meshes != null)
				{
					// Debug.Log("nodeSrc_po.getLocalAlignMatrix()"+nodeSrc_po.getLocalAlignMatrix());
					if ( (i>0 && i<cellsU) || (i == 0  && doFirstNode) || (i == (cellsU) && doLastNode))
					{
						string this_address = "node_"+i;

						int ni = doFirstNode ? i : i-1;


						// LOCAL_PLACEMENT //

						localPlacement_mx = localNodeMatrixFromAddress(i);


						if (float.IsNaN(localPlacement_mx.m00))
							continue;


						// AX_MESHES

						for (int mi = 0; mi < nodeSrc_p.meshes.Count; mi++) {
							AXMesh dep_amesh = nodeSrc_p.meshes [mi];

							//tmpMesh = dep_amesh.CloneTransformed (localPlacement_mx * dep_amesh.transMatrix);
							tmpMesh = dep_amesh.Clone (localPlacement_mx * dep_amesh.transMatrix);
							tmpMesh.subItemAddress = this_address;
							ax_meshes.Add (tmpMesh);
						}



						// BOUNDING MESHES

						boundsCombinator[ni].mesh 		= nodeSrc_po.boundsMesh;
						boundsCombinator[ni].transform 	= localPlacement_mx * nodeSrc_po.generator.localMatrixWithAxisRotationAndAlignment;


						// GAME_OBJECTS

						if (nodePlugGO != null  && makeGameObjects && ! parametricObject.combineMeshes)
						{
							//Matrix4x4 mx = localPlacement_mx  * parametricObject.getTransMatrix() * source.getTransMatrix();

							//Debug.Log(nodeSrc_po.getLocalMatrix());
							Matrix4x4  mx 		=   localPlacement_mx * nodeSrc_po.generator.localMatrixWithAxisRotationAndAlignment;


							GameObject copyGO 	= (GameObject) GameObject.Instantiate(nodePlugGO, AXUtilities.GetPosition(mx), AXUtilities.QuaternionFromMatrix(mx));

							copyGO.transform.localScale	= new Vector3(copyGO.transform.localScale.x*jitterScale.x, copyGO.transform.localScale.y*jitterScale.y, copyGO.transform.localScale.z*jitterScale.z);

							AXGameObject axgo = copyGO.GetComponent<AXGameObject>();
							if (axgo != null)
								axgo.consumerAddress = this_address;

							copyGO.name = copyGO.name+"_" + this_address;
							copyGO.transform.parent = go.transform;
						}
					}
				} // \NODES
				



				// CELL CENTERS
				if (cellSrc_p != null && cellSrc_p.meshes != null    && i<cellsU )
				{
					string this_address = "cell_"+i;

					//Debug.Log("Here");
					// LOCAL_PLACEMENT
					localPlacement_mx = localCellMatrixFromAddress(i);


					// ACTUAL MESHES
					for (int mi = 0; mi < cellSrc_p.meshes.Count; mi++) {
						AXMesh dep_amesh = cellSrc_p.meshes [mi];
						tmpMesh = dep_amesh.Clone (localPlacement_mx * dep_amesh.transMatrix);
						tmpMesh.subItemAddress = this_address;
						ax_meshes.Add (tmpMesh);
					}

					 

					// BOUNDING MESHES

					boundsCombinator[i].mesh 		= cellSrc_po.boundsMesh;
					boundsCombinator[i].transform 	= localPlacement_mx * cellSrc_po.generator.localMatrixWithAxisRotationAndAlignment;;



					// GAME_OBJECTS

					if (cellPlugGO != null && makeGameObjects && ! parametricObject.combineMeshes)
					{
						//Matrix4x4 mx = localPlacement_mx  * parametricObject.getTransMatrix() * source.getTransMatrix();
						Matrix4x4 mx =      localPlacement_mx * cellSrc_po.generator.localMatrixWithAxisRotationAndAlignment;
						GameObject copyGO = (GameObject) GameObject.Instantiate(cellPlugGO, AXUtilities.GetPosition(mx), AXUtilities.QuaternionFromMatrix(mx));

						copyGO.transform.localScale	= new Vector3(copyGO.transform.localScale.x*jitterScale.x, copyGO.transform.localScale.y*jitterScale.y, copyGO.transform.localScale.z*jitterScale.z);

						AXGameObject axgo = copyGO.GetComponent<AXGameObject>();
						if (axgo != null)
							axgo.consumerAddress = this_address;
						
						copyGO.name = copyGO.name+"_" + this_address;
						copyGO.transform.parent = go.transform;

						//Debug.Log("LINEAR: " + axgo.consumerAddress);
					}
				} // \CELLS
	
					
					

				// SPANS
						
				if (spanUSrc_p != null && spanUSrc_p.meshes != null && i<cellsU)
				{
					string this_address = "spanU_"+i;
					// X-AXIS
					
					// LOCAL_PLACEMENT

					localPlacement_mx = localCellMatrixFromAddress(i);



					// AX_MESHES
																												// AX_MESHES								
					for (int mi = 0; mi < ax_meshes_X.Count; mi++) {
						AXMesh dep_amesh = ax_meshes_X [mi];
						tmpMesh = dep_amesh.Clone (localPlacement_mx * dep_amesh.transMatrix);
						tmpMesh.subItemAddress = this_address;
						ax_meshes.Add (tmpMesh);
					}



					// BOUNDING MESHES

					boundsCombinator[i].mesh 		= spanUSrc_po.boundsMesh;
					boundsCombinator[i].transform 	= localPlacement_mx * spanUSrc_po.generator.localMatrixWithAxisRotationAndAlignment;


					// GAME_OBJECTS
					
					if (spanPlugGO != null && makeGameObjects && ! parametricObject.combineMeshes)
					{
						Matrix4x4 mx =      localPlacement_mx * spanSrc_po.generator.localMatrixWithAxisRotationAndAlignment;
						GameObject copyGO = (GameObject) GameObject.Instantiate(spanPlugGO, AXUtilities.GetPosition(mx), AXUtilities.QuaternionFromMatrix(mx));

						copyGO.transform.localScale	= new Vector3(copyGO.transform.localScale.x*jitterScale.x, copyGO.transform.localScale.y*jitterScale.y, copyGO.transform.localScale.z*jitterScale.z);


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
				{
					go = parametricObject.makeGameObjectsFromAXMeshes(ax_meshes, true);


				}
				Matrix4x4 tmx = parametricObject.getLocalMatrix();

				go.transform.rotation 		= AXUtilities.QuaternionFromMatrix(tmx);
				go.transform.position 		= AXUtilities.GetPosition(tmx);
				go.transform.localScale 	= parametricObject.getLocalScaleAxisRotated();//AXUtilities.GetScale(tmx);

				return go;
			}
			

			return null;			
		}
		




	}






}
