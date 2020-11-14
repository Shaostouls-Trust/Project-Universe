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

using AX;


namespace AX.Generators
{



	/*	REPLICANT 
	 * 
	 * The Replicant is similar to the Instance,
	 * but rather than adding on transformation data to each AXMesh in the input,
	 * it sets controls on the source, regenerates it and then sets the controls back to where they were.
	 * 
	 * A Replicant can only manipulate its local handles, which are a copy of the handles of the 
	 * source input. This limit makes sense, otherwise, trying to get handles through the ancestry is too complex.
	 * 
	 * A typical scenario is for a Relicant to replicate a Module. The module has its "contract" with its inputs and
	 * the Replicant need only mimic the Parameters of the Module and the immediate handles.
	 * 
	 * Lets say the radius of a part of a Shape up the line was important to vary: then this should be linked to
	 * the Module controls so it is there at the artists fingertips and is easy for a Replicant to mimic.
	 * 
	 * A key question is: what happens once the hook up is made to the Replicant but then an edit is made to the source?
	 */
	public class Replicant : AX.Generators.Generator3D, IRepeater, IReplica, ICustomNode
	{

		
		// INIT_PARAMETRIC_OBJECT
		public override void init_parametricObject() 
		{
			Debug.Log ("Init Replicant");

			base.init_parametricObject();

			// parameters
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Input, "Input Mesh"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Output,"Output Mesh"));
			
			// handles
			
		}  

		public override void connectionMadeWith(AXParameter to_p, AXParameter from_p)
		{
			base.connectionMadeWith(to_p, from_p);

			AXParametricObject po = from_p.parametricObject;
			
			parametricObject.syncControlsOfPTypeWith(AXParameter.ParameterType.PositionControl, po);
			parametricObject.syncControlsOfPTypeWith(AXParameter.ParameterType.TextureControl,  po);
			parametricObject.syncControlsOfPTypeWith(AXParameter.ParameterType.BoundsControl, po);
			parametricObject.syncControlsOfPTypeWith(AXParameter.ParameterType.GeometryControl, po);
			parametricObject.syncHandlesWith(po);
		}
		
		
		// GENERATE REPLICANT
		public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool renderToOutputParameter)
		{
			//Debug.Log ("===> [" + parametricObject.Name + "] REPLICANT generate ... MAKE_GAME_OBJECTS="+makeGameObjects + " renderToOutputParameter="+renderToOutputParameter);

			if (parametricObject == null  || ! parametricObject.hasInputMeshReady("Input Mesh"))
				return null;

			if (! parametricObject.isActive)
				return null;

			preGenerate();




			AXParameter 		input_p 	 = parametricObject.getParameter("Input Mesh");
			if (parametricObject == null  || input_p == null || input_p.DependsOn == null)
				return null;
						
			AXParametricObject 	src_po 	= input_p.DependsOn.Parent;
			if (src_po == null)
				return null;



			//


			//AXParametricObject repro = AXEd






			Matrix4x4 srcLocalM = src_po.getLocalMatrix().inverse;

		
			// 1. cache source object
			List<AXParameter> geometryControlParameterList = parametricObject.getAllParametersOfPType(AXParameter.ParameterType.GeometryControl);
			src_po.cacheAndSetParameters(geometryControlParameterList);

			//Debug.Log("JETZ::: " +src_po.getParameter("Height").FloatVal);



			if (! makeGameObjects)
			{
				//Debug.Log(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
				// 3. re_generate source PO with temporary values set by this Replicant
				src_po.generateOutputNow (false, parametricObject, true, true);

				// 3. Now that the input_sourcePO has been regenerated, 
				//    grab the temporaryOutputMeshes meshes 
				//    from the input sources and add them here			
				List<AXMesh> 	ax_meshes 	= new List<AXMesh>();
				if (src_po.temporaryOutputMeshes != null)
				{
				Debug.Log("HAVE TEMPS");
					for (int mi = 0; mi < src_po.temporaryOutputMeshes.Count; mi++) {
						AXMesh amesh = src_po.temporaryOutputMeshes [mi];
						Debug.Log(parametricObject.getLocalMatrix());
						ax_meshes.Add (amesh.Clone (srcLocalM * amesh.transMatrix * parametricObject.getLocalMatrix()));
					}
				}
				parametricObject.finishMultiAXMeshAndOutput(ax_meshes, false);

				setBoundaryFromAXMeshes(ax_meshes);

			}
			// 4. Make GAME OBJECTS
			GameObject go = null;
			if (makeGameObjects)
			{
				//Debug.Log("YO: " + src_po.Name);


				go = src_po.generateOutputNow(true,  initiator_po,  true, true);

				if (go == null)
				{
					Debug.Log(src_po.Name + " Replicant GameObject not created....");
					return null;
				}
				go.name = src_po.Name + " Replicant";
				AXGameObject axgo = go.GetComponent<AXGameObject>();
				if (axgo != null)
					axgo.makerPO_GUID = parametricObject.Guid;
				
			} 
			


			

			//  5. restore source object; as though we were never here!
			src_po.revertParametersFromCache();
			
			
			// Turn ax_meshes into GameObjects
			if (makeGameObjects)
			{				
				Matrix4x4 tmx = parametricObject.getLocalMatrix();

				go.transform.rotation 		= AXUtilities.QuaternionFromMatrix(tmx);
				go.transform.position 		= AXUtilities.GetPosition(tmx);
				go.transform.localScale 	= parametricObject.getLocalScaleAxisRotated();

				return go;
			}
			
			return null;
			
		}


		
	}
	
}


