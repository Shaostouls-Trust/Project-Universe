// http://www.archimatix.com

using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

using AXClipperLib;
using Path = System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;

using AXPoly2Tri;
using PolygonPoints = System.Collections.Generic.List<AXPoly2Tri.PolygonPoint>;



namespace AX.Generators
{
	


	// LANDER GENERATOR
	public class Lander : Mesher, IMeshGenerator, ITerrainer
	{
		// This is really an extrude with no sides or depth.
		// later mak it individual and have an Extrude use it to differentiate the caps!
		public override string GeneratorHandlerTypeName { get { return "PolygonHandler"; } }



		public float h = .02f;


		// INPUTS

		// INIT
		public override void init_parametricObject() 
		{
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, AXParameter.ParameterType.Input, "Input Shape"));

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.MaterialTool, AXParameter.ParameterType.Input, "Material"));

			base.init_parametricObject();

			parametricObject.addParameter(AXParameter.DataType.Float,	"Height", 		30f, 0f, 1000f);



		}


		// POLL INPUTS (only on graph change())
		public override void pollInputParmetersAndSetUpLocalReferences()
		{
			base.pollInputParmetersAndSetUpLocalReferences();

			P_Plan = parametricObject.getParameter("Input Shape");
		}

		public override AXParameter getPreferredInputParameter()
		{			
			return P_Plan;
		}

		// POLL CONTROLS (every model.generate())
		public override void pollControlValuesFromParmeters()
		{
			base.pollControlValuesFromParmeters();

		}




		public override void connectionBrokenWith(AXParameter p)
		{
			base.connectionBrokenWith(p);


			switch (p.Name)
			{
				
			case "Input Shape":
				planSrc_po = null;
				P_Output.paths = null; 
				P_Output.polyTree = null;
				P_Output.meshes = null;
				break;

			}

		}

		





		// GENERATE POLYGON
		public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool renderToOutputParameter)
		{


			Terrain terrain = parametricObject.terrain;
			if (terrain != null)
			{


				carveTerrain();

			}


		/*
			if (parametricObject == null || ! parametricObject.isActive || planSrc_po == null)
				return null;

			//Debug.Log ("===> [" + parametricObject.Name + "] EXTRUDE generate ... MAKE_GAME_OBJECTS="+makeGameObjects);

			// RESULTING MESHES
			ax_meshes = new List<AXMesh>();



			preGenerate();

			P_Plan.polyTree = null;
			
			AXShape.thickenAndOffset(ref P_Plan, planSrc_p);



			GameObject retGO =   generateFirstPass(initiator_po, makeGameObjects, P_Plan, null, Matrix4x4.identity, renderToOutputParameter);


			// FINISH AX_MESHES

			parametricObject.finishMultiAXMeshAndOutput(ax_meshes, renderToOutputParameter);


			// FINISH BOUNDING

			setBoundaryFromAXMeshes(ax_meshes);


			return retGO;;

			*/
			return null;
		}





		public void memorizeTerrain()
		{

			Terrain terrain = parametricObject.terrain;

			if (terrain != null) 
			{
				int hmWidth 	= terrain.terrainData.heightmapResolution;
				int hmHeight 	= terrain.terrainData.heightmapResolution;

				float[,] temp =  terrain.terrainData.GetHeights (0, 0, hmWidth, hmHeight);

				parametricObject.heightsOrig = new float[hmWidth, hmHeight];


				for (int i = 0; i < hmWidth; i++)
				{
					for (int j = 0; j < hmHeight; j++) {
						parametricObject.heightsOrig[i,j] = temp[i,j];
					}
				}
			}
		}

		public void carveTerrain()
		{

			Terrain terrain = parametricObject.terrain;

			if (terrain != null) 
			{

				if (parametricObject.heightsOrig == null)
					memorizeTerrain();

					// BOUNDING_SHAPE
								

				if (P_Plan != null && P_Plan.DependsOn != null)
				{
					
					if (planSrc_po != null)
					{
						AXShape.thickenAndOffset(ref P_Plan, planSrc_p);
					// now boundin_p has offset and thickened polytree or paths.
			
						
					} 
				}




				int hmWidth 	= terrain.terrainData.heightmapResolution;
				int hmHeight 	= terrain.terrainData.heightmapResolution;


				float[,] heights = new float[hmWidth, hmHeight] ;


				// we set each sample of the terrain in the size to the desired height
				for (int i = 0; i < hmWidth; i++)
				{
					for (int j = 0; j < hmHeight; j++) 
					{
						// GET i, j in realworld coords
						float x = terrain.terrainData.size.x * i/hmWidth;
						float y = terrain.terrainData.size.z * j/hmHeight;


						if (isInside(y,x))
						{
							//Debug.Log(x+", "+y);
							heights [i, j] = h;
						}
						else
							heights [i, j] = parametricObject.heightsOrig[i,j];
						//print(heights[i,j]);
					}
				}
				// set the new height
				terrain.terrainData.SetHeights (0, 0, heights);

			}



		}

		public bool isInside(float i, float j)
		{
		 

			Paths boundingSolids = P_Plan.getTransformedSubjPaths();
			Paths boundingHoles  = P_Plan.getTransformedHolePaths();

			bool pointIsInside = false;

			IntPoint ip = new IntPoint(i*AXGeometry.Utilities.IntPointPrecision,  j*AXGeometry.Utilities.IntPointPrecision);

			if(boundingSolids != null)
			{

				

				if (boundingSolids != null)
				{
					foreach (Path path in boundingSolids)
					{
						if (Clipper.PointInPolygon(ip, path) == 1 && Clipper.Orientation(path))
						{
							pointIsInside = true;
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
							pointIsInside = false;
							break;
						}
					}
				}
			}


			//exclude = (boundingIsVoid) ? ! exclude : exclude;

			//if (pointIsInside) 
				//continue;

			return pointIsInside;

		}



		public override Matrix4x4 getLocalConsumerMatrixPerInputSocket(AXParametricObject input, AXParameter input_p=null)
		{
			if (planSrc_po != null)
			{
				if (input == planSrc_po)
					return Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90, 0, 0), Vector3.one);
				
				return Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(-90, 0, 0), Vector3.one);
			}
			return Matrix4x4.identity;
		}
	}
}