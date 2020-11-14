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
	
	public class FloorRepeater : RepeaterBase, IRepeater
	{
		public override string GeneratorHandlerTypeName { get { return "FloorRepeaterHandler"; } }


		// INPUTS
		public AXParameter	P_Floor;
		public AXParameter	P_Story;

		public AXParameter	P_Top;
		public AXParameter	P_Bottom;

		public AXParameter	P_Repeater;

		public AXParameter	P_TopFloor;
		public AXParameter	P_BottomFloor;

		public AXParameter	P_TopStory;
		public AXParameter	P_BottomStory;


		// WORKING FIELDS (Updated every Generate)

		public AXParameter 			floorSrc_p;
		public AXParametricObject 	floorSrc_po;

		public AXParameter 			storySrc_p;
		public AXParametricObject 	storySrc_po;

		public AXParameter 			topFloorSrc_p;
		public AXParametricObject 	topFloorSrc_po;

		public AXParameter 			bottomFloorSrc_p;
		public AXParametricObject 	bottomFloorSrc_po;



		public AXParametricObject 	repeaterToolSrc_po;
		public RepeaterTool 		repeaterTool;

		public bool topFloor;
		public bool bottomFloor;
		public bool topStory;
		public bool bottomStory;

		public override void init_parametricObject()
		{
			base.init_parametricObject ();

			// parameters
			parametricObject.addParameter (new AXParameter (AXParameter.DataType.Mesh, AXParameter.ParameterType.Input, "Floor Mesh"));
			parametricObject.addParameter (new AXParameter (AXParameter.DataType.Mesh, AXParameter.ParameterType.Input, "Story Mesh"));
			parametricObject.addParameter (new AXParameter (AXParameter.DataType.Mesh, AXParameter.ParameterType.Input, "Top Floor Mesh"));
			parametricObject.addParameter (new AXParameter (AXParameter.DataType.Mesh, AXParameter.ParameterType.Input, "Bottom Floor Mesh"));
		

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.JitterTool,   AXParameter.ParameterType.Input, "Jitter Translation"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.JitterTool,   AXParameter.ParameterType.Input, "Jitter Rotation"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.JitterTool,   AXParameter.ParameterType.Input, "Jitter Scale"));

			initRepeaterTools();


			  
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Output,"Output Mesh"));


			P_TopFloor = parametricObject.addParameter(AXParameter.DataType.Bool, 				"Top Floor", true);
			P_BottomFloor = parametricObject.addParameter(AXParameter.DataType.Bool, 			"Bottom Floor", true);
			P_TopStory = parametricObject.addParameter(AXParameter.DataType.Bool, 				"Top Story", true);
			P_BottomStory = parametricObject.addParameter(AXParameter.DataType.Bool, 			"Bottom Story", true);

			P_ProgressiveRotationX = parametricObject.addParameter(AXParameter.DataType.Float, 	"IncrRotX", 0f); 
			P_ProgressiveRotationY = parametricObject.addParameter(AXParameter.DataType.Float, 	"IncrRotY", 0f); 
			P_ProgressiveRotationZ = parametricObject.addParameter(AXParameter.DataType.Float, 	"IncrRotZ", 0f); 


		}



		public void initRepeaterTools() 
		{
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.RepeaterTool, AXParameter.ParameterType.Input, "Repeater"));
			AXParametricObject repeaterTool =  parametricObject.model.createNode("RepeaterTool");
			repeaterTool.rect.x = parametricObject.rect.x-200;
			repeaterTool.isOpen = false;
			repeaterTool.intValue("Edge_Count", 100);
			parametricObject.getParameter("Repeater").makeDependentOn(repeaterTool.getParameter("Output"));


		}


		// POLL INPUTS (only on graph change())
		public override void pollInputParmetersAndSetUpLocalReferences()
		{
			//Debug.Log("Repeater::pollInputParmetersAndSetUpLocalReferences " + parametricObject.Name + " parametersHaveBeenPolled="+parametersHaveBeenPolled);

			base.pollInputParmetersAndSetUpLocalReferences();

			P_Node 						= parametricObject.getParameter("Floor Mesh");
			P_Top 						= parametricObject.getParameter("Top Floor Mesh");
			P_Bottom 					= parametricObject.getParameter("Bottom Floor Mesh");
			P_Story 					= parametricObject.getParameter("Story Mesh");

			P_Repeater					= parametricObject.getParameter("Repeater");


			P_TopFloor 						= parametricObject.getParameter("Top Floor");
			P_BottomFloor 					= parametricObject.getParameter("Bottom Floor");
			P_TopStory 						= parametricObject.getParameter("Top Story");
			P_BottomStory 					= parametricObject.getParameter("Bottom Story");

		}



		// POLL CONTROLS (every model.generate())
		public override void pollControlValuesFromParmeters()
		{
			base.pollControlValuesFromParmeters();


			storySrc_p 			= (P_Story  	!= null && P_Story.DependsOn != null)  	? P_Story.DependsOn						: null;
			storySrc_po 		= (storySrc_p != null)  								? storySrc_p.parametricObject			: null;

			topFloorSrc_p 			= (P_Top  	!= null && P_Top.DependsOn != null)  		? P_Top.DependsOn						: null;
			topFloorSrc_po 			= (topFloorSrc_p != null)  									? topFloorSrc_p.parametricObject				: null;

			bottomFloorSrc_p 		= (P_Bottom  	!= null && P_Bottom.DependsOn != null)  ? P_Bottom.DependsOn					: null;
			bottomFloorSrc_po 		= (bottomFloorSrc_p != null)  								? bottomFloorSrc_p.parametricObject			: null;

			repeaterToolSrc_po 	= (P_Repeater != null && P_Repeater.DependsOn != null) 	? P_Repeater.DependsOn.parametricObject 	: null;
			repeaterTool		= (repeaterToolSrc_po != null) 							? (repeaterToolSrc_po.generator as RepeaterTool) : null;


			topFloor 			= (P_TopFloor != null) 		? P_TopFloor.boolval		: true;
			bottomFloor 		= (P_BottomFloor != null) 	? P_BottomFloor.boolval		: true;

			topStory 			= (P_TopStory != null) 		? P_TopStory.boolval		: true;
			bottomStory 		= (P_BottomStory != null) 	? P_BottomStory.boolval		: true;


		}










		public  override void initializeBays(string pName)
		{
			if (parametricObject.isInitialized)
				return;

			if (repeaterTool == null)
				return;

			parametricObject.isInitialized = true;

			/*
			switch(pName)
			{
			case "Floor Mesh":
				//Debug.Log(Mathf.Min(3f, nodeSrc_p.parametricObject.bounds.size.x*.7f));
				repeaterTool.P_Bay.intiateRipple_setFloatValueFromGUIChange( Mathf.Min(3f, nodeSrc_p.parametricObject.bounds.size.x*.7f));
				break;
			case "Cell Mesh":
				repeaterTool.P_Bay.intiateRipple_setFloatValueFromGUIChange( cellSrc_p.parametricObject.bounds.size.y*1.1f);
				break;
			case "Top Mesh":
				repeaterTool.P_Bay.intiateRipple_setFloatValueFromGUIChange( spanUSrc_p.parametricObject.bounds.size.y*1.1f);
				break;

			case "Bottom Mesh":
				repeaterTool.P_Bay.intiateRipple_setFloatValueFromGUIChange( spanUSrc_p.parametricObject.bounds.size.y*1.1f);
				break;
			}
			*/

		}






		// GENERATE LINEAR_REPEATER
		public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool isReplica)
		{
			//Debug.Log("repeaterToolU="+repeaterToolU+", repeaterToolV="+repeaterToolV);
			if (parametricObject == null || ! parametricObject.isActive)
				return null;

			if (repeaterTool == null)
				return null;
			

			preGenerate();






		


			// FLOOR_MESH (NODE_MESH)
		 	nodeSrc_po 		= null;
			GameObject 			nodePlugGO 		= null;

			//Debug.Log("Get source");
			if (nodeSrc_p != null)
			{
				nodeSrc_po 		= nodeSrc_p.parametricObject;
				if (makeGameObjects && ! parametricObject.combineMeshes)
					nodePlugGO = nodeSrc_po.generator.generate(true,  initiator_po,  isReplica);
			}	



			// STORY MESH (CELL_MESH)
		 	storySrc_po 	= null;
			GameObject 			storyPlugGO 	= null;
			if (storySrc_p != null)
			{
				storySrc_po 	= storySrc_p.parametricObject;
				if (makeGameObjects && ! parametricObject.combineMeshes)
					storyPlugGO = storySrc_po.generator.generate(true,  initiator_po,  isReplica);				
			}

			// TOP_FLOOR			
		 	topFloorSrc_po 	= null;
			GameObject topPlugGO 	= null;

			if (topFloorSrc_p != null)
			{
				topFloorSrc_po 	= topFloorSrc_p.parametricObject;
				if (makeGameObjects && ! parametricObject.combineMeshes)
					topPlugGO = topFloorSrc_po.generator.generate(true,  initiator_po,  isReplica);				
			}
			// BOTTOM_FLOOR			
		 	bottomFloorSrc_po 	= null;
			GameObject bottomPlugGO 	= null;

			if (bottomFloorSrc_p != null)
			{
				bottomFloorSrc_po 	= bottomFloorSrc_p.parametricObject;
				if (makeGameObjects && ! parametricObject.combineMeshes)
					bottomPlugGO = bottomFloorSrc_po.generator.generate(true,  initiator_po,  isReplica);				
			}




			if (nodeSrc_po == null && storySrc_po == null && topFloorSrc_po == null && bottomFloorSrc_po == null)
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


			int max_reps = 150;

			int 	cellsU 		= Mathf.Clamp(repeaterTool.cells, 1, max_reps);
			float 	actualBayU 	= repeaterTool.actualBay;

			shiftU = -cellsU * actualBayU / 2;

			AXMesh tmpMesh;



			// BAY SPAN
			// Spanners are meshers that get replicated and sized to fit the bay...

			// prepare mesh to iterate in each direction





			// BOUNDING

			int boundingObjectCount = 0;


			bool hasFloorMesh 		= (nodeSrc_po 			!= null && nodeSrc_p.meshes 		!= null);
			bool hasTopFloorMesh 	= (topFloorSrc_po 		!= null && topFloorSrc_p.meshes 	!= null);
			bool hasBottomFloorMesh = (bottomFloorSrc_po 	!= null && bottomFloorSrc_p.meshes 	!= null);

			bool hasStoryMesh = (storySrc_p != null && storySrc_p.meshes != null);

			// floors
			if (hasFloorMesh) {
				boundingObjectCount += cellsU + 1;

				if (!topFloor)
					boundingObjectCount--;
			 
				if (!bottomFloor)
					boundingObjectCount--;
				
			} else {
				if (hasTopFloorMesh && topFloor)
					boundingObjectCount++;

				if (hasBottomFloorMesh && bottomFloor)
					boundingObjectCount++;
			}


			// stories
			if (hasStoryMesh)
			{
				boundingObjectCount += cellsU;

				if (! topStory)
					boundingObjectCount--;

				if (! bottomStory)
					boundingObjectCount--;

			}

			//Debug.Log(cellsU + " :::::: " + boundingObjectCount);



			CombineInstance[] boundsCombinator = new CombineInstance[boundingObjectCount];
			int boundingCursor = 0;

			// LOOP
			for (int i=0; i<=cellsU; i++) 
			{
				


				// FLOORS (NODES)
				if ( (i==0 && bottomFloor) || (i==cellsU && topFloor) || (i>0 && i<cellsU))
				{
					AXParameter tmpSrc_p 			= nodeSrc_p;
					AXParametricObject tmpSrc_po 	= nodeSrc_po;
					GameObject 	tmpPlugGO 			= nodePlugGO;
					if (i == 0 && bottomFloorSrc_p != null ) {
						tmpSrc_p 	= bottomFloorSrc_p;
						tmpSrc_po 	= bottomFloorSrc_po;
						tmpPlugGO 	= bottomPlugGO;
					} else if (i == cellsU && topFloorSrc_p != null ) {
						tmpSrc_p 	= topFloorSrc_p;
						tmpSrc_po 	= topFloorSrc_po;
						tmpPlugGO 	= topPlugGO;
					}

					if (tmpSrc_po != null && tmpSrc_p.meshes != null)
					{
						string this_address = "node_"+i;





						localPlacement_mx = localNodeMatrixFromAddress(i);


						// AX_MESHES
					
						for (int j = 0; j < tmpSrc_p.meshes.Count; j++) {
							AXMesh dep_amesh = tmpSrc_p.meshes [j];

							tmpMesh = dep_amesh.Clone (localPlacement_mx * dep_amesh.transMatrix);
							tmpMesh.subItemAddress = this_address;
							ax_meshes.Add (tmpMesh);
						}



						// BOUNDING MESHES

						boundsCombinator[boundingCursor].mesh 		= tmpSrc_po.boundsMesh;
						boundsCombinator[boundingCursor].transform 	= localPlacement_mx * tmpSrc_po.generator.localMatrixWithAxisRotationAndAlignment;
						boundingCursor++;


						// GAME_OBJECTS

						if (tmpPlugGO != null  && makeGameObjects && ! parametricObject.combineMeshes)
						{
							//Matrix4x4 mx = localPlacement_mx  * parametricObject.getTransMatrix() * source.getTransMatrix();

							//Debug.Log(nodeSrc_po.getLocalMatrix());
							Matrix4x4  mx 		=  localPlacement_mx * tmpSrc_po.generator.localMatrixWithAxisRotationAndAlignment;
							GameObject copyGO	= (GameObject) GameObject.Instantiate(tmpPlugGO, AXUtilities.GetPosition(mx), AXUtilities.QuaternionFromMatrix(mx));

							copyGO.transform.localScale	= new Vector3(copyGO.transform.localScale.x*jitterScale.x, copyGO.transform.localScale.y*jitterScale.y, copyGO.transform.localScale.z*jitterScale.z);



							#if UNITY_EDITOR
							//if (parametricObject.model.isSelected(tmpSrc_po) && tmpSrc_po.selectedConsumerAddress == this_address)
							//	Selection.activeGameObject = copyGO;
							#endif


							AXGameObject axgo = copyGO.GetComponent<AXGameObject>();
							if (axgo != null)
								axgo.consumerAddress = this_address;

							copyGO.name = copyGO.name+"_" + this_address;
							copyGO.transform.parent = go.transform;
						}
					} // \NODES
				}



				// STORIES (CELLS)
				if ( (i==0 && bottomStory) || (i==cellsU-1 && topStory) || (i>0 && i<cellsU-1))
				{
					if (storySrc_p != null && storySrc_p.meshes != null    && i<cellsU )
					{
						string this_address = "cell_"+i;


						// LOCAL_PLACEMENT
						localPlacement_mx = localCellMatrixFromAddress(i);


						// AX_MESHES

						for (int j = 0; j < storySrc_p.meshes.Count; j++) {
							AXMesh dep_amesh = storySrc_p.meshes [j];
							tmpMesh = dep_amesh.Clone (localPlacement_mx * dep_amesh.transMatrix);
							tmpMesh.subItemAddress = this_address;
							ax_meshes.Add (tmpMesh);
						}



						// BOUNDING MESHES
						if (boundsCombinator.Length > boundingCursor)
						{
							boundsCombinator[boundingCursor].mesh 		= storySrc_po.boundsMesh;
							boundsCombinator[boundingCursor].transform 	= localPlacement_mx * storySrc_po.generator.localMatrixWithAxisRotationAndAlignment;
							boundingCursor++;
						}


						// GAME_OBJECTS

						if (storyPlugGO != null && makeGameObjects && ! parametricObject.combineMeshes)
						{
							//Matrix4x4 mx = localPlacement_mx  * parametricObject.getTransMatrix() * source.getTransMatrix();
							Matrix4x4 mx =      localPlacement_mx * storySrc_po.generator.localMatrixWithAxisRotationAndAlignment;
							GameObject copyGO = (GameObject) GameObject.Instantiate(storyPlugGO, AXUtilities.GetPosition(mx), AXUtilities.QuaternionFromMatrix(mx));

							AXGameObject axgo = copyGO.GetComponent<AXGameObject>();
							if (axgo != null)
								axgo.consumerAddress = this_address;

							copyGO.name = copyGO.name+"_" + this_address;
							copyGO.transform.parent = go.transform;
						}
					} // \ STORIES (CELLS)
				}







			} //i


			GameObject.DestroyImmediate(nodePlugGO);
			GameObject.DestroyImmediate(storyPlugGO);
			GameObject.DestroyImmediate(topPlugGO);
			GameObject.DestroyImmediate(bottomPlugGO);

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

			if (meshInput == "node" &&  nodeSrc_p != null && caller == nodeSrc_p.Parent)
				return localNodeMatrixFromAddress(indexU, indexV) * optionalLocalM;

			if (meshInput == "cell" &&  storySrc_p != null && caller == storySrc_p.Parent)
				return localCellMatrixFromAddress(indexU, indexV) * optionalLocalM;
			


			return Matrix4x4.identity;
		} 















	
		public  Matrix4x4 localNodeMatrixFromAddress(int i=0, int j=0, int k=0)
		{
			float perlinTranslation 	= 1;
			float perlinRot 			= 1;
			float perlinScale 			= 1;

			float actualBay = (repeaterTool != null) ? repeaterTool.actualBay : 0;

			float y = actualBay * i;




			if (jitterTranslationTool != null)
				perlinTranslation 	=  Mathf.PerlinNoise((y + jitterTranslationTool.offset) * jitterTranslationTool.perlinScale, 0);

			if (jitterRotationTool != null)
				perlinRot 			=  Mathf.PerlinNoise(jitterRotationTool.perlinScale * (y + jitterRotationTool.offset), jitterRotationTool.perlinScale * y);
			

			if (jitterScaleTool != null)
				perlinScale 		=  Mathf.PerlinNoise((y + jitterScaleTool.offset)* jitterScaleTool.perlinScale, 0);

			 
			// TRANSLATION 	*********
			Vector3 translate = new Vector3(0, y, 0);
			if (jitterTranslationTool != null)
				translate += new Vector3( perlinTranslation*jitterTranslationTool.x-jitterTranslationTool.x/2 , perlinTranslation*jitterTranslationTool.y-jitterTranslationTool.y/2, perlinTranslation*jitterTranslationTool.z-jitterTranslationTool.z/2);

			Terrain terrain = Terrain.activeTerrain;
			if (terrain != null)
				translate.y += terrain.SampleHeight(parametricObject.model.gameObject.transform.TransformPoint(translate));



			// ROTATION		*********
			Quaternion 	rotation = Quaternion.Euler (0, rotY, 0);
			if (jitterRotationTool != null)
				rotation = Quaternion.Euler ((perlinRot*jitterRotationTool.x - jitterRotationTool.x/2)*1.5f, (perlinRot*jitterRotationTool.y - jitterRotationTool.y/2) , (perlinRot*jitterRotationTool.z - jitterRotationTool.z/2)) * rotation;
			
				 

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

			return       Matrix4x4.TRS(translate, rotation, jitterScale) * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler( i * progressiveRotationX, i * progressiveRotationY,  i * progressiveRotationZ), Vector3.one);


		}

		


		public  Matrix4x4 localCellMatrixFromAddress(int i=0, int j=0, int k=0)
		{

			float perlinTranslation 	= 1;
			float perlinRot 			= 1;
			float perlinScale 			= 1;

			float actualBay = (repeaterTool != null) ? repeaterTool.actualBay : 0;


			// GENERATE PERLIN VALUES

			if (jitterTranslationTool != null)
				perlinTranslation 	=  Mathf.PerlinNoise( (i*actualBay + jitterTranslationTool.offset) * jitterTranslationTool.perlinScale,  0);

			if (jitterRotationTool != null)
				perlinRot 			=  Mathf.PerlinNoise( (i*actualBay + jitterRotationTool.offset) * jitterRotationTool.perlinScale,  0);

			if (jitterScaleTool != null)
				perlinScale 		=  Mathf.PerlinNoise( (i*actualBay + jitterScaleTool.offset) * jitterScaleTool.perlinScale,  0);


			// TRANSLATION 	*********

			//Vector3 translate = new Vector3((i*actualBayU+shiftU+actualBayU/2), 0, ((j>-1) ? (j*actualBayV+shiftV+actualBayV/2) : 0) );
			Vector3 translate = new Vector3(0, (i*actualBay), 0);

			// -- JITTER
			if (jitterTranslationTool != null)
				translate += new Vector3( perlinTranslation*jitterTranslationTool.x-jitterTranslationTool.x/2 , perlinTranslation*jitterTranslationTool.y-jitterTranslationTool.y/2, perlinTranslation*jitterTranslationTool.z-jitterTranslationTool.z/2);



			// ROTATION		*********
			Quaternion 	rotation = Quaternion.identity;// Quaternion.Euler (0, roty, 0);
			if (jitterRotationTool != null)
				rotation = Quaternion.Euler ((perlinRot*jitterRotationTool.x - jitterRotationTool.x/2)*1.5f, (perlinRot*jitterRotationTool.y - jitterRotationTool.y/2), (perlinRot*jitterRotationTool.z - jitterRotationTool.z/2)) * rotation;


			// SCALE		**********
			jitterScale = Vector3.zero;
			if (jitterScaleTool != null)
				jitterScale = new Vector3( perlinScale*jitterScaleTool.x-jitterScaleTool.x/2 , perlinScale*jitterScaleTool.y-jitterScaleTool.y/2, perlinScale*jitterScaleTool.z-jitterScaleTool.z/2);
			

			// ******** USE A FUNCTION HERE TO GET FOR HANDLES AS WELL
			return   Matrix4x4.TRS(translate, rotation, Vector3.one+jitterScale) * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(i * progressiveRotationX,  i * progressiveRotationY,  i * progressiveRotationZ), Vector3.one);
		}


		

	
	}






}
