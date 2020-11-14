using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;


using AXClipperLib;
using Path = System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;

using AXPoly2Tri;
using PolygonPoints = System.Collections.Generic.List<AXPoly2Tri.PolygonPoint>;


namespace AX.Generators
{
/*	TERRAIN DEFORMER
	 * 
	 */
	public class TerrainDeformer : Deformer, IDeformer
	{

		
		// INIT_PARAMETRIC_OBJECT
		public override void init_parametricObject() 
		{
			// init parameters for palette
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Input, "Input Mesh"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Output,"Output Mesh"));
			parametricObject.addParameter(AXParameter.DataType.Float, 	"amount");
		}
		
		// GENERATE
		public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool isReplica)
		{			
			if (parametricObject == null  || ! parametricObject.hasInputMeshReady("Input Mesh"))
				return null;

			preGenerate();

			Terrain terrain = parametricObject.terrain;
			//float terrainY = 0;


			//float amount 	= parametricObject.floatValue("amount");
			
			List<AXMesh> 	ax_meshes 		= new List<AXMesh>();
			
			AXParameter 	input_p 		= parametricObject.getParameter("Input Mesh");
			AXParameter		inputSrc_p		= input_p.DependsOn;

			AXParametricObject 	inputSrc_po = inputSrc_p.parametricObject;
			
//			Matrix4x4 sourceLocalTrans 	= inputSrc_po.getLocalTransformMatrix();
//			Matrix4x4 sourceLocalAxis 	= inputSrc_po.getAxisRotationMatrix();
//			Matrix4x4 sourceLocalAlign 	= inputSrc_po.getLocalAlignMatrix();
			// just pass it through, but use grandparent's transform!

			//inputSrc_po.generator.generate(false, null, true);

			if (inputSrc_p != null && inputSrc_p.meshes != null)
			{


				for (int i = 0; i < inputSrc_p.meshes.Count; i++) {
					// Instance does not inhereit and build on the transform of its source object.

					AXMesh amesh = inputSrc_p.meshes [i].Clone();
					Mesh m = amesh.mesh;
					Vector3[] verts = m.vertices;
					for (int j = 0; j < verts.Length; j++) {
						//float new_y = (verts[i].y < .01) ? verts[i].y : verts[i].y+amount* (Mathf.PerlinNoise(verts[i].x,  verts[i].z) *  .5f);

						//float new_y = (terrain == null) ? verts [j].y : verts [j].y + terrain.SampleHeight (parametricObject.model.gameObject.transform.TransformPoint (parametricObject.localMatrix.MultiplyPoint (verts [j])));
						float new_y = (terrain == null) ? verts [j].y : verts [j].y + terrain.SampleHeight (inputSrc_p.parametricObject.localMatrix.MultiplyPoint( verts [j]));

						verts [j] = new Vector3 (verts [j].x, new_y, verts [j].z);
					}
					m.vertices = verts;
					ax_meshes.Add (amesh.Clone (amesh.transMatrix));// * sourceLocalAlign.inverse * sourceLocalAxis.inverse * sourceLocalTrans.inverse));
				}
				
				parametricObject.bounds = inputSrc_po.bounds;
				parametricObject.finishMultiAXMeshAndOutput(ax_meshes, isReplica);


				if (makeGameObjects)
					return parametricObject.makeGameObjectsFromAXMeshes(ax_meshes, true);
				

			}


			return null;
			
		}
		

		
	}
}