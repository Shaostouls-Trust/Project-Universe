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

	
	// INSTANCE
	// The Instance only adds the transform in the chain based on its positioning parameter
	// It is akin to a null GameObject.

	public class Instance : AX.Generators.Generator3D, IRepeater, IReplica
	{
		public override string GeneratorHandlerTypeName { get { return "InstanceHandler"; } }
		
		// INIT_PARAMETRIC_OBJECT
		public override void init_parametricObject() 
		{
			base.init_parametricObject();

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Input, "Input Mesh"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Output,"Output Mesh"));
		}

		// POLL INPUTS (only on graph change())
		public override void pollInputParmetersAndSetUpLocalReferences()
		{
			//Debug.Log("Repeater::pollInputParmetersAndSetUpLocalReferences " + parametricObject.Name + " parametersHaveBeenPolled="+parametersHaveBeenPolled);

			base.pollInputParmetersAndSetUpLocalReferences();

			P_Input = parametricObject.getParameter("Input Mesh");
		}


		// GENERATE INSTANCE
		public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool isReplica)
		{
			if (parametricObject == null || ! parametricObject.isActive)
				return null;

			if (P_Input == null ||  P_Input.DependsOn == null || P_Input.DependsOn.meshes == null ||  P_Input.DependsOn.meshes.Count == 0)
				return null;

			if (inputSrc_po == null)
				inputSrc_po =  P_Input.DependsOn.parametricObject;
			preGenerate();

			GameObject go = null;


			if (makeGameObjects)
			{
				go = inputSrc_po.generator.generate(true,  initiator_po,  isReplica);
				if (go == null)
					return null;

				go.name = go.name + " (Inst)";
				AXGameObject axgo = go.GetComponent<AXGameObject>();
				if (axgo != null)
					axgo.makerPO_GUID = parametricObject.Guid;
				
			} 

			List<AXMesh> 	ax_meshes 		= new List<AXMesh>();
			
			Matrix4x4 srcLocalM = inputSrc_po.getLocalMatrix().inverse;
			// just pass it through, but use grandparent's transform!
			
			// Instance does not inhereit and build on the transform of its source object.
			// It only uses its mesh, textures, etc.
			for (int mi = 0; mi < P_Input.DependsOn.meshes.Count; mi++) {
				AXMesh amesh = P_Input.DependsOn.meshes [mi];
				ax_meshes.Add (amesh.Clone (srcLocalM * amesh.transMatrix));
			}

			parametricObject.bounds = inputSrc_po.bounds;
			parametricObject.boundsMesh = inputSrc_po.boundsMesh;

			parametricObject.finishMultiAXMeshAndOutput(ax_meshes, isReplica);
			
			// Turn ax_meshes into GameObjects
			if (makeGameObjects)
			{				
				Matrix4x4 tmx = parametricObject.getLocalMatrix();
				
				go.transform.localScale 	= AXUtilities.GetScale(tmx); //
				go.transform.rotation 		= AXUtilities.QuaternionFromMatrix(tmx);
				go.transform.position 		= AXUtilities.GetPosition(tmx);

				return go;
			}
			return null;
		}
	}


}

