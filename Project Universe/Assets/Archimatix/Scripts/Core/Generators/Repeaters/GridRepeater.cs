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




	
	// GRID_REPEATER
	
	public class GridRepeater : Repeater, IRepeater
	{
		public override string GeneratorHandlerTypeName { get { return "GridRepeaterHandler"; } }
		
		 


		public override void pollInputParmetersAndSetUpLocalReferences()
		{
			base.pollInputParmetersAndSetUpLocalReferences();

		}



		
		// GENERATE GRID_REPEATER
		public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool isReplica)
		{
		//Debug.Log(parametricObject.Name + " Generate");
			if (parametricObject == null || ! parametricObject.isActive)
				return null;

			if (repeaterToolU == null || repeaterToolV == null)
				return null;

			preGenerate();


			// NODE_MESH
			AXParametricObject 	nodeSrc_po 		= null;
			GameObject 			nodePlugGO 		= null;

			if (nodeSrc_p != null)
			{
				nodeSrc_po 		= nodeSrc_p.parametricObject;
				if (makeGameObjects && ! parametricObject.combineMeshes)
					nodePlugGO = nodeSrc_po.generator.generate(true,  initiator_po,  isReplica);
			}	


			// CELL_MESH
			AXParametricObject 	cellSrc_po 		= null;
			GameObject 			cellPlugGO 		= null;
			if (cellSrc_p != null)
			{
				cellSrc_po 	= cellSrc_p.parametricObject;
				if (makeGameObjects && ! parametricObject.combineMeshes)
					cellPlugGO = cellSrc_po.generator.generate(true,  initiator_po,  isReplica);				
			}

			// BAY_SPAN_U			
			AXParametricObject 	spanUSrc_po 	= null;
			GameObject 			spanUPlugGO 	= null;
			if (spanUSrc_p != null)
			{
				spanUSrc_po 	= spanUSrc_p.parametricObject;
				if (makeGameObjects && ! parametricObject.combineMeshes)
					spanUPlugGO = spanUSrc_po.generator.generate(true,  initiator_po,  isReplica);				
			}
			
			// BAY_SPAN_V			
			AXParametricObject 	spanVSrc_po 	= null;
			GameObject 			spanVPlugGO 	= null;
			if (spanVSrc_p != null)
			{
				spanVSrc_po 	= spanVSrc_p.parametricObject;
				if (makeGameObjects && ! parametricObject.combineMeshes)
					spanVPlugGO = spanVSrc_po.generator.generate(true,  initiator_po,  isReplica);				
			}
			



			if (nodeSrc_po == null && cellSrc_po == null && spanUSrc_po == null && spanVSrc_po == null)
			{
				AXParameter output_p = getPreferredOutputParameter();
				if (output_p != null)
					output_p.meshes = null;
				
				return null;
			}


			GameObject go 		= null;
			if (makeGameObjects && ! parametricObject.combineMeshes)
				go = ArchimatixUtils.createAXGameObject(parametricObject.Name, parametricObject);



			// BOUNDING_SHAPE

			Paths boundingSolids = null;
			Paths boundingHoles = null;
		

			if (P_BoundingShape != null && P_BoundingShape.DependsOn != null)
			{
				
				AXParameter bounding_src_p 		= null;
				if (P_BoundingShape != null)
					bounding_src_p 		= P_BoundingShape.DependsOn;  // USING SINGLE SPLINE INPUT

				boundingShapeSrc_po = bounding_src_p.parametricObject;

				AXShape.thickenAndOffset(ref P_BoundingShape, bounding_src_p);
				// now boundin_p has offset and thickened polytree or paths.
		
				boundingSolids = P_BoundingShape.getTransformedSubjPaths();
				boundingHoles  = P_BoundingShape.getTransformedHolePaths();
			}


			 

			List<AXMesh> 	ax_meshes 		= new List<AXMesh>();

			Matrix4x4 localPlacement_mx = Matrix4x4.identity;

			// -----------------------------------

			int max_reps = 150;

			int 	cellsU 		= Mathf.Clamp(repeaterToolU.cells, 1, max_reps);
			float 	actualBayU 	= repeaterToolU.actualBay;

			int 	cellsV 		= Mathf.Clamp(repeaterToolV.cells, 1, max_reps);
			float 	actualBayV 	= repeaterToolV.actualBay;

			shiftU 		= -cellsU * actualBayU / 2;
			shiftV 		= -cellsV * actualBayV / 2;

						
			// BAY SPAN
			// Spanners are meshers that get replicated and sized to fit the bay...

			// prepare mesh to iterate in each direction

			//List<AXMesh> 	ax_meshes_X 		= new List<AXMesh>();
			//List<AXMesh> 	ax_meshes_Z 		= new List<AXMesh>();



			AXMesh tmpMesh;

			if (spanUSrc_p != null)
			{
				//ax_meshes_X = spanUSrc_p.meshes;
				//ax_meshes_Z = spanUSrc_p.meshes;
			}
			/* NEED TO INTEGRATE THIS BACK IN IN THE FUTRE...
			if (bay_span_source != null)
			{

				// 1. cache source object
				bay_span_source.cacheParameterValues();


				// Y_AXIS
				
				AXParameter p_bayv = parametricObject.getParameter("actual_bay_V");
				AXParameter p_bayv_client = null;
				if (p_bayv != null)
				{
					foreach (AXRelation rel in p_bayv.relations)
					{
						p_bayv_client = rel.getRelatedTo(p_bayv);
						p_bayv_client.intiateRipple_setFloatValueFromGUIChange(abayy);
					}
				}


				AXParameter p_bayu = parametricObject.getParameter("actual_bay_U");
				AXParameter p_bayu_client = null;

				// X-AXIS
				// For now, only set the boundaries. 
				// Perhaps later, may want to set other like controls as in Replicant

				// If there is a relation to actual_bay_U, propogate it
				if (p_bayu != null)
				{
					foreach (AXRelation rel in p_bayu.relations)
					{
						p_bayu_client = rel.getRelatedTo(p_bayu);
						p_bayu_client.intiateRipple_setFloatValueFromGUIChange(abayx);
					}
				}

				//bay_span_source.propagateParameterByBinding(1, bayx);
				//bay_span_source.propagateParameterByBinding(2, bayy);
				
				// 2. re_generate with temporary values set by this Replicant
				bay_span_source.generateOutputNow (makeGameObjects, parametricObject);
				
				// 3. Now that the bay_span_source has been regergrab the meshes from the input sources and add them here
				AXParameter output_p = bay_span_source.getParameter("Output Mesh");
				foreach (AXMesh amesh in output_p.meshes)
					ax_meshes_X.Add (amesh.Clone(amesh.transMatrix));
				
				

				// Z-AXIS
				// Use the bayz now to generate x

				if (p_bayu != null)
				{
					foreach (AXRelation rel in p_bayu.relations)
					{
						p_bayu_client = rel.getRelatedTo(p_bayu);
						p_bayu_client.intiateRipple_setFloatValueFromGUIChange(abayz);
					}
				}

				//bay_span_source.propagateParameterByBinding(1, bayz);


				// 2. re_generate with temporary values set by this Replicant
				bay_span_source.generateOutputNow (makeGameObjects, parametricObject);
				
				// 3. Now that the bay_span_source has been regergrab the meshes from the input sources and add them here
				foreach (AXMesh amesh in output_p.meshes)
					ax_meshes_Z.Add (amesh.Clone(amesh.transMatrix));


				// 4. restore source object; as though we were never here!
				bay_span_source.revertParametersFromCache();

				//Debug.Log ("HAVE BAY SPAN -- " + output_p.meshes.Count);


			}
			*/


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

			List<AXMesh> boundingMeshes = new List<AXMesh>();



			for (int i=0; i<=cellsU; i++) 
			{
				for (int k=0; k<=cellsV; k++) 
				{

					bool exclude = false;

					IntPoint ip = new IntPoint((i*actualBayU+shiftU)*AXGeometry.Utilities.IntPointPrecision,  (k*actualBayV+shiftV)*AXGeometry.Utilities.IntPointPrecision);

					if(boundingSolids != null)
					{

						exclude = true;

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
					}


					exclude = (boundingIsVoid) ? ! exclude : exclude;

					if (exclude) 
						continue;


					// NODES
					if (nodeSrc_po != null && nodeSrc_p.meshes != null && ( (i<=repeaterToolU.edgeCount || i>=cellsU-repeaterToolU.edgeCount) || (k<=repeaterToolV.edgeCount || k>=cellsV-repeaterToolV.edgeCount)))
					{
						string this_address = "node_"+i+"_"+k;

						// LOCAL PLACEMENT NODE
						localPlacement_mx = localNodeMatrixFromAddress(i, k);


						// AX_MESHES

						for (int mi = 0; mi < nodeSrc_p.meshes.Count; mi++) {
							AXMesh dep_amesh = nodeSrc_p.meshes [mi];
							tmpMesh = dep_amesh.Clone (localPlacement_mx * dep_amesh.transMatrix);
							tmpMesh.subItemAddress = this_address;
							ax_meshes.Add (tmpMesh);
						}



						// BOUNDING
						boundingMeshes.Add(new AXMesh(nodeSrc_po.boundsMesh, localPlacement_mx * nodeSrc_po.generator.localMatrix));


						// GAME_OBJECTS

						if (nodePlugGO != null && makeGameObjects && ! parametricObject.combineMeshes)
						{
							Matrix4x4 mx 				= localPlacement_mx * nodeSrc_po.generator.localMatrixWithAxisRotationAndAlignment;
							GameObject copyGO 			= (GameObject) GameObject.Instantiate(nodePlugGO, AXUtilities.GetPosition(mx), AXUtilities.QuaternionFromMatrix(mx));

							copyGO.transform.localScale	= new Vector3(copyGO.transform.localScale.x*jitterScale.x, copyGO.transform.localScale.y*jitterScale.y, copyGO.transform.localScale.z*jitterScale.z);

							#if UNITY_EDITOR
							//if (parametricObject.model.isSelected(nodeSrc_po) && nodeSrc_po.selectedConsumerAddress == this_address)
							//	Selection.activeGameObject = copyGO;
							#endif

							AXGameObject axgo = copyGO.GetComponent<AXGameObject>();
							if (axgo != null)
								axgo.consumerAddress = this_address;
							
							copyGO.name = copyGO.name+"_"+this_address;
							copyGO.transform.parent = go.transform;
						}
					} // \NODES
						
						
						

					// CELL CENTERS
					if (cellSrc_po != null && cellSrc_p.meshes != null    && i<cellsU &&  k<cellsV && ( (i < repeaterToolU.edgeCount || i > cellsU-repeaterToolU.edgeCount-1) || (k < repeaterToolV.edgeCount || k>cellsV-repeaterToolV.edgeCount-1) ) )
					{
						string this_address = "cell_"+i+"_"+k;

						// LOCAL PLACEMENT

						localPlacement_mx = localCellMatrixFromAddress(i, k);


						// AX_MESHES

						for (int mi = 0; mi < cellSrc_p.meshes.Count; mi++) {
							AXMesh dep_amesh = cellSrc_p.meshes [mi];
							tmpMesh = dep_amesh.Clone (localPlacement_mx * dep_amesh.transMatrix);
							tmpMesh.subItemAddress = this_address;
							ax_meshes.Add (tmpMesh);
						}


						// BOUNDING
						boundingMeshes.Add(new AXMesh(cellSrc_po.boundsMesh, localPlacement_mx * cellSrc_po.generator.localMatrix));



						// GAME_OBJECTS

						if (cellPlugGO != null && makeGameObjects && ! parametricObject.combineMeshes)
						{
							Matrix4x4 mx 				= localPlacement_mx * cellSrc_po.generator.localMatrixWithAxisRotationAndAlignment;
							GameObject copyGO 			= (GameObject) GameObject.Instantiate(cellPlugGO, AXUtilities.GetPosition(mx), AXUtilities.QuaternionFromMatrix(mx));

							copyGO.transform.localScale	= new Vector3(copyGO.transform.localScale.x*jitterScale.x, copyGO.transform.localScale.y*jitterScale.y, copyGO.transform.localScale.z*jitterScale.z);

							#if UNITY_EDITOR
							//if (parametricObject.model.isSelected(cellSrc_po) && cellSrc_po.selectedConsumerAddress == this_address)
								//Selection.activeGameObject = copyGO;
							#endif

							AXGameObject axgo = copyGO.GetComponent<AXGameObject>();
							if (axgo != null)
								axgo.consumerAddress = this_address;
							
							copyGO.name = copyGO.name+"_"+this_address;
							copyGO.transform.parent = go.transform;
						}
					}

						

					// SPAN_U
					if (spanUSrc_po != null && spanUSrc_p.meshes != null  && i<cellsU &&  k<=cellsV && ( (i < repeaterToolV.edgeCount || i > cellsU-repeaterToolU.edgeCount-1) || (k <= repeaterToolV.edgeCount || k>cellsV-repeaterToolV.edgeCount-1) ) )
					{
						string this_address = "spanU_"+i+"_"+k;

						// LOCAL PLACEMENT SPAN_U

						localPlacement_mx = localSpanUMatrixFromAddress(i, k);


						// AX_MESHES

						for (int mi = 0; mi < spanUSrc_p.meshes.Count; mi++) {
							AXMesh dep_amesh = spanUSrc_p.meshes [mi];
							tmpMesh = dep_amesh.Clone (localPlacement_mx * dep_amesh.transMatrix);
							tmpMesh.subItemAddress = this_address;
							ax_meshes.Add (tmpMesh);
						}


						// BOUNDING
						boundingMeshes.Add(new AXMesh(spanUSrc_po.boundsMesh, localPlacement_mx * spanUSrc_po.generator.localMatrix));



						// GAME_OBJECTS

						if (spanUSrc_po != null && makeGameObjects && ! parametricObject.combineMeshes)
						{
							Matrix4x4 mx 				= localPlacement_mx * spanUSrc_po.generator.localMatrixWithAxisRotationAndAlignment;
							GameObject copyGO 			= (GameObject) GameObject.Instantiate(spanUPlugGO, AXUtilities.GetPosition(mx), AXUtilities.QuaternionFromMatrix(mx));


							copyGO.transform.localScale	= new Vector3(copyGO.transform.localScale.x*jitterScale.x, copyGO.transform.localScale.y*jitterScale.y, copyGO.transform.localScale.z*jitterScale.z);

							#if UNITY_EDITOR
							//if (parametricObject.model.isSelected(spanUSrc_po) && spanUSrc_po.selectedConsumerAddress == this_address)
							//	Selection.activeGameObject = copyGO;
							#endif

							AXGameObject axgo = copyGO.GetComponent<AXGameObject>();
							if (axgo != null)
								axgo.consumerAddress = this_address;
							
							copyGO.name = copyGO.name+"_"+this_address;
							copyGO.transform.parent = go.transform;
						}
					}

					// SPAN_V
					if (spanVSrc_po != null && spanVSrc_p.meshes != null  && i<=cellsU &&  k<cellsV && ( (i <= repeaterToolU.edgeCount || i > cellsU-repeaterToolU.edgeCount-1) || (k < repeaterToolV.edgeCount || k>cellsV-repeaterToolV.edgeCount-1) ) )
					{
						string this_address = "spanV_"+i+"_"+k;

						// LOCAL PLACEMENT SPAN_U
						localPlacement_mx = localSpanVMatrixFromAddress(i, k);



						// AX_MESHES

						for (int mi = 0; mi < spanVSrc_p.meshes.Count; mi++) {
							AXMesh dep_amesh = spanVSrc_p.meshes [mi];
							tmpMesh = dep_amesh.Clone (localPlacement_mx * dep_amesh.transMatrix);
							tmpMesh.subItemAddress = this_address;
							ax_meshes.Add (tmpMesh);
						}



						// BOUNDING
						boundingMeshes.Add(new AXMesh(spanVSrc_po.boundsMesh, localPlacement_mx * spanVSrc_po.generator.localMatrix));



						// GAME_OBJECTS

						if (spanVSrc_po != null && makeGameObjects && ! parametricObject.combineMeshes)
						{
							Matrix4x4 mx 				= localPlacement_mx * spanVSrc_po.generator.localMatrixWithAxisRotationAndAlignment;
							GameObject copyGO 			= (GameObject) GameObject.Instantiate(spanVPlugGO, AXUtilities.GetPosition(mx), AXUtilities.QuaternionFromMatrix(mx));

						
							copyGO.transform.localScale	= new Vector3(copyGO.transform.localScale.x*jitterScale.x, copyGO.transform.localScale.y*jitterScale.y, copyGO.transform.localScale.z*jitterScale.z);

							#if UNITY_EDITOR
							//if (parametricObject.model.isSelected(spanVSrc_po) && spanVSrc_po.selectedConsumerAddress == this_address)
							//	Selection.activeGameObject = copyGO;
							#endif

							AXGameObject axgo = copyGO.GetComponent<AXGameObject>();
							if (axgo != null)
								axgo.consumerAddress = this_address;
							
							copyGO.name = copyGO.name+"_"+this_address;
							copyGO.transform.parent = go.transform;
						}
					}


				} // k
			} //i


			GameObject.DestroyImmediate(nodePlugGO);
			GameObject.DestroyImmediate(cellPlugGO);
			GameObject.DestroyImmediate(spanUPlugGO);
			GameObject.DestroyImmediate(spanVPlugGO);



			// FINISH AX_MESHES

			parametricObject.finishMultiAXMeshAndOutput(ax_meshes, isReplica);




			// FINISH BOUNDS

			CombineInstance[] boundsCombinator = new CombineInstance[boundingMeshes.Count];
			for(int bb=0; bb<boundsCombinator.Length; bb++)
			{
				boundsCombinator[bb].mesh 		= boundingMeshes[bb].mesh;
				boundsCombinator[bb].transform 	= boundingMeshes[bb].transMatrix;
			}
			setBoundsWithCombinator(boundsCombinator);



			// FINISH GAME_OBJECTS

			if (makeGameObjects)
			{
				if (parametricObject.combineMeshes)
					go = parametricObject.makeGameObjectsFromAXMeshes(ax_meshes, true);
				
				Matrix4x4 tmx = parametricObject.generator.localMatrixWithAxisRotationAndAlignment;

				go.transform.rotation 		= AXUtilities.QuaternionFromMatrix(tmx);
				go.transform.position 		= AXUtilities.GetPosition(tmx);
				go.transform.localScale 	= parametricObject.getLocalScaleAxisRotated();

				return go;
			}

			return null;			
		}


	}



}
