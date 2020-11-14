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
	
	public interface IDeformer
	{
		
	}



	public class Deformer : AX.Generators.Generator3D, IDeformer
	{

		public override void initGUIColor ()
		{
			GUIColor 		= new Color(.87f,.77f,.43f,.8f);
			GUIColorPro 	= new Color(1f, 1f, .57f, .9f);
			ThumbnailColor  = new Color(.318f,.31f,.376f);
		}


	}
	
	/*	PERLIN DEFORMER
	 * 
	 */
	public class PerlinDeformer : AX.Generators.Generator3D, IDeformer
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

			float amount 	= parametricObject.floatValue("amount");
			
			List<AXMesh> 	ax_meshes 		= new List<AXMesh>();

			AXParameter 		input_p 	 = parametricObject.getParameter("Input Mesh");
			AXParametricObject 	input_parent = input_p.DependsOn.Parent;
			
			Matrix4x4 sourceLocalTrans 	= input_parent.getLocalTransformMatrix();
			Matrix4x4 sourceLocalAxis 	= input_parent.getAxisRotationMatrix();
			Matrix4x4 sourceLocalAlign 	= input_parent.generator.getLocalAlignMatrix();
			// just pass it through, but use grandparent's transform!
			
			
			for (int i = 0; i < input_p.DependsOn.meshes.Count; i++) {
				// Instance does not inhereit and build on the transform of its source object.
				AXMesh amesh = input_p.DependsOn.meshes [i];
				Mesh m = amesh.mesh;
				Vector3[] verts = m.vertices;
				for (int j = 0; j < verts.Length; j++) {
					float new_y = (verts [j].y < .01) ? verts [j].y : verts [j].y + amount * (Mathf.PerlinNoise (verts [j].x, verts [j].z) * .5f);
					verts [j] = new Vector3 (verts [j].x, new_y, verts [j].z);
				}
				m.vertices = verts;
				ax_meshes.Add (amesh.Clone (amesh.transMatrix * sourceLocalAlign.inverse * sourceLocalAxis.inverse * sourceLocalTrans.inverse));
			}
			
			parametricObject.bounds = input_parent.bounds;
			parametricObject.finishMultiAXMeshAndOutput(ax_meshes, isReplica);
			
			
			return null;
			
		}
	}
		



}
	
	