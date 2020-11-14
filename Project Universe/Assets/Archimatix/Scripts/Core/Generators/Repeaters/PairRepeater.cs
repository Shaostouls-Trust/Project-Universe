using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using AXGeometry;

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
	public class PairRepeater : RepeaterBase, IRepeater
	{

		public override string GeneratorHandlerTypeName { get { return "PairRepeaterHandler"; } }

		// INPUTS
		public AXParameter	P_Seperation;
		public AXParameter	P_Symmetrical;


		// WORKING FIELDS (Updated every Generate)

		public float 	separation = 3;
		public bool 	symmetrical = true;

		// BASED ON zAxis or not
		Matrix4x4  		symmetryM;



		// INIT_PARAMETRIC_OBJECT
		public override void init_parametricObject() 
		{
			base.init_parametricObject();

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Input, "Node Mesh"));
			//parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Input, "Cell Mesh"));

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.JitterTool,   AXParameter.ParameterType.Input, "Jitter Translation"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.JitterTool,   AXParameter.ParameterType.Input, "Jitter Rotation"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.JitterTool,   AXParameter.ParameterType.Input, "Jitter Scale"));


			P_Seperation = parametricObject.addParameter(AXParameter.DataType.Float , 	"Separation", 		1f, 0f, 2000f);
			//p.sizeBindingAxis = Axis.X;

			parametricObject.addParameter(AXParameter.DataType.Bool, 	"zAxis", false);
			parametricObject.addParameter(AXParameter.DataType.Bool, 	"Symmetrical", true);

			P_ProgressiveRotationX = parametricObject.addParameter(AXParameter.DataType.Float, 	"IncrRotX", 0f); 
			P_ProgressiveRotationY = parametricObject.addParameter(AXParameter.DataType.Float, 	"IncrRotY", 0f); 
			P_ProgressiveRotationZ = parametricObject.addParameter(AXParameter.DataType.Float, 	"IncrRotZ", 0f); 

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Output,"Output Mesh"));
		}


		// POLL INPUTS (only on graph change())
		public override void pollInputParmetersAndSetUpLocalReferences()
		{
			//Debug.Log("Repeater::pollInputParmetersAndSetUpLocalReferences " + parametricObject.Name + " parametersHaveBeenPolled="+parametersHaveBeenPolled);

			base.pollInputParmetersAndSetUpLocalReferences();



			P_Seperation 			= parametricObject.getParameter("Separation", "separation");
			P_Symmetrical 			= parametricObject.getParameter("Symmetrical", "symetrical");

			pollControlValuesFromParmeters();
		}





		// POLL CONTROLS (every model.generate())
		public override void pollControlValuesFromParmeters()
		{
			base.pollControlValuesFromParmeters();

			cellSrc_p 		= (P_Cell  	!= null && P_Cell.DependsOn != null)  	? getUpstreamSourceParameter(P_Cell)	: null;
			cellSrc_po 		= (cellSrc_p != null)  								? cellSrc_p.parametricObject			: null;

			separation	= (P_Seperation != null) 	? P_Seperation.FloatVal : 3;
			symmetrical = (P_Symmetrical != null) 	? P_Symmetrical.boolval : true;


			// SYMMETRY_MATRIX
			Quaternion symRot 		= zAxis ? Quaternion.Euler(0, 0, 180) : Quaternion.Euler(180, 0, 0);
			symmetryM 	= Matrix4x4.TRS(Vector3.zero, symRot, -Vector3.one);


		}






		public override void connectionMadeWith(AXParameter to_p, AXParameter from_p)
		{
			if (from_p == null)// || from_p.DependsOn == null)
				return;

			AXParameter this_p = (to_p.parametricObject == parametricObject) ? to_p   : from_p;
			//AXParameter src_p  = (to_p.parametricObject == parametricObject) ? from_p : to_p;


			base.connectionMadeWith(to_p, from_p);


			if (parametricObject.is2D())
				return;

			switch(this_p.Name) { 
			case "Node Mesh":
				if (nodeSrc_po != null)
					P_Seperation.initiatePARAMETER_Ripple_setFloatValueFromGUIChange( nodeSrc_po.getBoundsAdjustedForAxis().size.x*1.1f);
				break;

			case "Cell Mesh":

				if (cellSrc_p != null)
					P_Seperation.initiatePARAMETER_Ripple_setFloatValueFromGUIChange( cellSrc_p.parametricObject.bounds.size.x*1.1f);
				break;
			}
		}


		// GENERATE PAIR_REPEATER
		public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool isReplica)
		{
		//Debug.Log ("PairRepeater::Generate ");
			if (parametricObject == null || ! parametricObject.isActive)
				return null;

			preGenerate();

			if ( (nodeSrc_p == null || nodeSrc_p.meshes == null || nodeSrc_p.meshes.Count == 0) && (nodeSrc_po != null &&  !(nodeSrc_po.generator is PrefabInstancer)))
				return null;


			if (P_Node == null || P_Node.DependsOn == null)
				return null;

			GameObject go = null;

			if (makeGameObjects && ! parametricObject.combineMeshes)
				go = ArchimatixUtils.createAXGameObject(parametricObject.Name, parametricObject);


			List<AXMesh> 	ax_meshes 		= new List<AXMesh>();

			GameObject plugGO = null;

			if (makeGameObjects && ! parametricObject.combineMeshes)
			{
				plugGO = nodeSrc_po.generator.generate(true,  initiator_po,  isReplica);


				if (plugGO == null)
					return null;
			}


           

            float separationX = zAxis ? 0 : separation;
			float separationZ = zAxis ? -separation : 0;


			CombineInstance[] boundsCombinator = new CombineInstance[2];



			// * RIGHT INSTANCE	*
			// --------------------------------------------------------------------


			// Right Instance is at Address 0...
			Matrix4x4 	localPlacement_mx = localNodeMatrixFromAddress(0);
			// use submeshes for right instance

			// AX_MESHES

			for (int mi = 0; mi < P_Node.DependsOn.meshes.Count; mi++) {
				AXMesh dep_amesh = P_Node.DependsOn.meshes [mi];
				ax_meshes.Add (dep_amesh.Clone (localPlacement_mx * dep_amesh.transMatrix));
			}


			// BOUNDING MESHES

			boundsCombinator[0].mesh 		= nodeSrc_po.boundsMesh;
			boundsCombinator[0].transform 	= localPlacement_mx * nodeSrc_po.generator.localMatrixWithAxisRotationAndAlignment;



			// GAME_OBJECTS

			if (plugGO != null && makeGameObjects && ! parametricObject.combineMeshes)
			{
				Matrix4x4 mx =  localPlacement_mx  * nodeSrc_po.generator.localMatrixWithAxisRotationAndAlignment;
				GameObject copyGO = (GameObject) GameObject.Instantiate(plugGO, AXUtilities.GetPosition(mx), AXUtilities.QuaternionFromMatrix(mx));
				copyGO.transform.localScale 	= nodeSrc_po.getLocalScaleAxisRotated();

				AXGameObject axgo = copyGO.GetComponent<AXGameObject>();
				if (axgo != null)
					axgo.consumerAddress = "node_0";


				copyGO.name = copyGO.name+"_node_right";
				copyGO.transform.parent = go.transform;
			}





			// * INVERSE (LEFT) INSTANCE 
			// --------------------------------------------------------------------

			// ***--- AX_MESHES - INVERSE (LEFT) ---***





			translate 			= new Vector3(-separationX/2, 0, -separationZ/2);

			// LOCAL PLACEMENT
			localPlacement_mx = localNodeMatrixFromAddress(1);

			// use submeshes for left instance
			for (int mi = 0; mi < P_Node.DependsOn.meshes.Count; mi++) {
				// CLONE
				AXMesh dep_amesh = P_Node.DependsOn.meshes [mi];
				AXMesh clone = dep_amesh.Clone ();
				// SYMETRICAL?
				if (symmetrical) {
					clone.mesh = AXMesh.freezeWithMatrix (dep_amesh.mesh, symmetryM * dep_amesh.transMatrix);
					clone.transMatrix = localPlacement_mx * symmetryM.inverse;
				}
				else
					clone = dep_amesh.Clone (localPlacement_mx * symmetryM.inverse * dep_amesh.transMatrix);
				// ADD TO AX_MESHES
				ax_meshes.Add (clone);
			}




			// BOUNDING MESHES

			boundsCombinator[1].mesh 		= nodeSrc_po.boundsMesh;
			boundsCombinator[1].transform 	= localPlacement_mx * nodeSrc_po.generator.localMatrixWithAxisRotationAndAlignment ;





			//   *** --- INVERSE (LEFT) - GAME_OBJECT ---**
			if (plugGO != null && makeGameObjects && ! parametricObject.combineMeshes)
			{

				// LOCAL PLACEMENT
				//Matrix4x4 mx = localPlacement_mx  * symmetryM * nodeSrc_po.generator.localMatrixWithAxisRotationAndAlignment;
				Matrix4x4 mx = localPlacement_mx   * nodeSrc_po.generator.localMatrixWithAxisRotationAndAlignment;






				// GAME_OBJECT
				GameObject copyGO = (GameObject) GameObject.Instantiate(plugGO, AXUtilities.GetPosition(mx), AXUtilities.QuaternionFromMatrix(mx));
				copyGO.transform.localScale 	= nodeSrc_po.getLocalScaleAxisRotated();

				// SYMETRICAL?
				if (symmetrical)
				{
					copyGO.transform.localScale =  copyGO.transform.localScale * -1;
					if(zAxis)
						copyGO.transform.Rotate(0, 180, 180);
					else
						copyGO.transform.Rotate(180, 0, 0);
				}


				// Force a refreshing of the colliders down the instatiatined hierachy
				// Based on a solution provided by pkamat here:http://forum.unity3d.com/threads/how-to-update-a-mesh-collider.32467/

				foreach(MeshCollider mc in copyGO.GetComponentsInChildren<MeshCollider>())
				{
					mc.sharedMesh = null;
					mc.sharedMesh = mc.gameObject.GetComponent<MeshFilter>().sharedMesh;
				}


				// ADD GAME_OBJECT
				AXGameObject axgo = copyGO.GetComponent<AXGameObject>();
				if (axgo != null)
					axgo.consumerAddress = "node_1";

				copyGO.name = copyGO.name+"_node_left";
				copyGO.transform.parent = go.transform;
			}







			GameObject.DestroyImmediate(plugGO);





			// COMBINE ALL THE MESHES
			CombineInstance[] combine = new CombineInstance[ax_meshes.Count];

			int combineCt = 0;
			for (int i = 0; i < ax_meshes.Count; i++) {
				AXMesh _amesh = ax_meshes [i];

                //_amesh.mesh.RecalculateNormals();
                //_amesh.mesh.RecalculateTangents();

                combine [combineCt].mesh = _amesh.mesh;
				combine [combineCt].transform = _amesh.transMatrix;
				combineCt++;
			}
			Mesh combinedMesh = new Mesh();
			combinedMesh.CombineMeshes(combine);
			combinedMesh.RecalculateBounds();

			// BOUNDARY - Use combined meshes for boundary
			setBoundsWithCombinator(boundsCombinator);

            /*
			// BOUNDS & CENTER

			Vector3 margin = new Vector3(source_po.bounds.size.x, source_po.bounds.size.y, source_po.bounds.size.z);
			Bounds b = new Bounds();
			b.size = new Vector3(separationX + margin.x, margin.y, separationZ + margin.z);
			b.extents = b.size/2;
			b.center = source_po.bounds.center;// + new Vector3(0, b.extents.y, 0);

			parametricObject.margin = margin;
			parametricObject.bounds = b;
			*/


            //Debug.Log("PairRepeater::Generate 2 " + ax_meshes.Count);
            // FINISH
            parametricObject.finishMultiAXMeshAndOutput(ax_meshes, isReplica);


			// Turn ax_meshes into GameObjects
			if (makeGameObjects)
			{
				if (parametricObject.combineMeshes)
				{
					go = parametricObject.makeGameObjectsFromAXMeshes(ax_meshes, true, false);

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

				//Matrix4x4 tmx = parametricObject.generator.localMatrix;
				Matrix4x4 tmx = parametricObject.getLocalMatrix();

				go.transform.rotation 		= AXUtilities.QuaternionFromMatrix(tmx);
				go.transform.position 		= AXUtilities.GetPosition(tmx);
				go.transform.localScale 	= parametricObject.getLocalScaleAxisRotated();



				return go;
			}


			return null;

		}







		/*
		public override void connectionMadeWith(AXParameter to_p, AXParameter from_p)
		{

			base.connectionMadeWith(to_p, from_p);

			AXParametricObject po = from_p.parametricObject;
			
			// use this po's bounds to init the separation parameters...
			if (! parametricObject.isInitialized)
			{
				parametricObject.isInitialized = true;

				if (zAxis)
					parametricObject.floatValue("Separation", 2f * parametricObject.bounds.size.z);
				else
					parametricObject.floatValue("Separation", 1.5f * parametricObject.bounds.size.x);
			}
		}
		*/

		public override void parameterWasModified(AXParameter p)
		{
			switch(p.Name)
			{
			case "zAxis":
				if (! parametricObject.isInitialized)
				{
					parametricObject.isInitialized = true;

					float sizex = nodeSrc_po.getBoundsAdjustedForAxis().size.x;
					float sizez = nodeSrc_po.getBoundsAdjustedForAxis().size.z;


					// zAxis hasn't changed yet, so...
					bool switchingToZAxis = ! zAxis;

					if (switchingToZAxis)
						P_Seperation.initiatePARAMETER_Ripple_setFloatValueFromGUIChange( sizex/sizez + sizez);
					else
						P_Seperation.initiatePARAMETER_Ripple_setFloatValueFromGUIChange(sizez/sizex + sizex);
				}
				break;
			}
		}















		public override void connectionBrokenWith(AXParameter p)
		{
			base.connectionBrokenWith(p);


			switch (p.Name)
			{

			case "Node Mesh":
				nodeSrc_po = null;
				P_Output.meshes = null;
				break;

			}

		}







		public  Matrix4x4 getLocalConsumerMatrixPerInputSocketOLD(AXParametricObject head)
		{
			if(zAxis)
				return Matrix4x4.TRS(new Vector3(0, 0, separation/2), Quaternion.identity, Vector3.one);

			return Matrix4x4.TRS(new Vector3(separation/2, 0, 0), Quaternion.identity, Vector3.one);
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


			//Debug.Log("indexU="+indexU+", indexV="+indexV);

			if (indexU == 1)
			{


			}
			// Find out which input this caller is fed into Node Mesh, Bay Span or Cell Mesh?

			if (meshInput == "node" && nodeSrc_p!= null && caller == nodeSrc_p.Parent)
				return localNodeMatrixFromAddress(indexU, indexV) * optionalLocalM;



			return Matrix4x4.identity;
		} 







		public  Matrix4x4 localNodeMatrixFromAddress(int i=0, int j=0)
		{
			float perlinTranslation 	= 1;
			float perlinRot 			= 1;
			float perlinScale 			= 1;





			float shiftUU = (zAxis) ? shiftV : shiftU;
			float shiftVV = (zAxis) ? shiftU : shiftV;

			float pShift = 2;

			float sep_x = (zAxis) ?  0 : (i==0) ? separation/2  : -separation/2 ;
			float sep_z = (zAxis) ?  ((i==0) ? separation/2  : -separation/2) : 0 ;

			float px = sep_x+pShift;

			if (jitterTranslationTool != null)
				perlinTranslation 	=  Mathf.PerlinNoise( px * jitterTranslationTool.perlinScale,  	 jitterTranslationTool.perlinScale);

			if (jitterRotationTool != null)
				perlinRot 			=  Mathf.PerlinNoise( px * jitterRotationTool.perlinScale,  	 jitterRotationTool.perlinScale);

			if (jitterScaleTool != null)
				perlinScale 		=  Mathf.PerlinNoise( px * jitterScaleTool.perlinScale,  		jitterScaleTool.perlinScale);



			// TRANSLATION 	*********
			Vector3 translate = new Vector3((sep_x+shiftUU), 0, (sep_z+shiftVV));


			if (jitterTranslationTool != null)
				translate += new Vector3( perlinTranslation*jitterTranslationTool.x-jitterTranslationTool.x/2 , perlinTranslation*jitterTranslationTool.y-jitterTranslationTool.y/2, perlinTranslation*jitterTranslationTool.z-jitterTranslationTool.z/2);



			// ROTATION		*********
			Quaternion 	rotation = Quaternion.identity;//Quaternion.Euler (0, rotY, 0);
			if (jitterRotationTool != null)
				rotation = Quaternion.Euler ((perlinRot*jitterRotationTool.x - jitterRotationTool.x/2)*1.5f, (perlinRot*jitterRotationTool.y - jitterRotationTool.y/2), (perlinRot*jitterRotationTool.z - jitterRotationTool.z/2)) * rotation;


			// SCALE		**********
			jitterScale = Vector3.zero;
			if (jitterScaleTool != null)
				jitterScale = new Vector3( perlinScale*jitterScaleTool.x-jitterScaleTool.x/2 , Mathf.Abs(perlinScale*jitterScaleTool.y), perlinScale*jitterScaleTool.z-jitterScaleTool.z/2);

			// ******** USE A FUNCTION HERE TO GET FOR HANDLES AS WELL


			Matrix4x4 retMX =  Matrix4x4.TRS(translate, rotation, Vector3.one+jitterScale) * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(i * progressiveRotationX, i * progressiveRotationY, i * progressiveRotationZ), Vector3.one);


			if (i == 1)
				retMX =  retMX * symmetryM;

			return retMX;
		}


	}







}