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
	


	// TerrainStamper GENERATOR
	public class TerrainStamper : Mesher, IMeshGenerator, ITerrainer
	{
		// This is really an extrude with no sides or depth.
		// later mak it individual and have an Extrude use it to differentiate the caps!
		public override string GeneratorHandlerTypeName { get { return "PolygonHandler"; } }



		public float datum = .02f;

		//IntRect prevBounds;

		// INPUTS


		public AXParameter P_Height;
		public float height;

		// INIT
		public override void init_parametricObject ()
		{
			parametricObject.addParameter (new AXParameter (AXParameter.DataType.Spline, AXParameter.ParameterType.Input, "Input Shape"));

			parametricObject.addParameter (new AXParameter (AXParameter.DataType.MaterialTool, AXParameter.ParameterType.Input, "Material"));

			base.init_parametricObject ();

			parametricObject.addParameter (AXParameter.DataType.Float,	"Height", 5f, 0f, 1000f);



		}


		// POLL INPUTS (only on graph change())
		public override void pollInputParmetersAndSetUpLocalReferences ()
		{
			base.pollInputParmetersAndSetUpLocalReferences ();

			P_Plan = parametricObject.getParameter ("Input Shape");

			P_Height = parametricObject.getParameter ("Height");
		}

		public override AXParameter getPreferredInputParameter ()
		{			
			return P_Plan;
		}

		// POLL CONTROLS (every model.generate())
		public override void pollControlValuesFromParmeters ()
		{
			base.pollControlValuesFromParmeters ();


			height = (P_Height != null) ?  P_Height.FloatVal : 3;


		}




		public override void connectionBrokenWith (AXParameter p)
		{
			base.connectionBrokenWith (p);


			switch (p.Name) {
				
			case "Input Shape":
				planSrc_po = null;
				P_Output.paths = null; 
				P_Output.polyTree = null;
				P_Output.meshes = null;
				break;

			}

		}

		





		// GENERATE POLYGON
		public override GameObject generate (bool makeGameObjects, AXParametricObject initiator_po, bool renderToOutputParameter)
		{
			if (P_Plan == null && P_Plan.DependsOn == null)
				return null;

			Terrain terrain = parametricObject.terrain;
			if (terrain != null) {


				carveTerrain ();

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





		public void memorizeTerrain ()
		{

			Terrain terrain = parametricObject.terrain;

			if (terrain != null) {
				int hmWidth = terrain.terrainData.heightmapResolution;
				int hmHeight = terrain.terrainData.heightmapResolution;

				float[,] temp = terrain.terrainData.GetHeights (0, 0, hmWidth, hmHeight);

				parametricObject.heightsOrig = new float[hmWidth, hmHeight];


				for (int i = 0; i < hmWidth; i++) {
					for (int j = 0; j < hmHeight; j++) {
						parametricObject.heightsOrig [i, j] = temp [i, j];
					}
				}
			}
		}

		public void carveTerrain ()
		{

			Terrain terrain = parametricObject.terrain;

			if (terrain != null) {

				if (parametricObject.heightsOrig == null)
					memorizeTerrain ();

				// BOUNDING_SHAPE
								

				if (P_Plan != null && P_Plan.DependsOn != null) {
					
					if (planSrc_po != null) {
						AXShape.thickenAndOffset (ref P_Plan, planSrc_p);
						// now boundin_p has offset and thickened polytree or paths.
			
						
					} 
				}


				int hmWidth = terrain.terrainData.heightmapResolution;
				int hmHeight = terrain.terrainData.heightmapResolution;



				Paths subjPaths = P_Plan.getTransformedSubjPaths ();
				Paths holePaths = P_Plan.getTransformedHolePaths ();

				//Pather.printPaths(subjPaths);

				if (subjPaths == null)
					return;

				IntRect bounds = Clipper.GetBounds(subjPaths);

//				if (prevBounds.right > 0)
//				{
//					bounds.left 	= ( prevBounds.left < bounds.left ) 		? prevBounds.left 	: bounds.left;
//					bounds.right 	= ( prevBounds.right > bounds.right ) 		? prevBounds.right 	: bounds.right;
//
//
//					bounds.top 		= ( prevBounds.top < bounds.top ) 			? prevBounds.top : bounds.top;
//					bounds.bottom 	= ( prevBounds.bottom > bounds.bottom ) 	? prevBounds.bottom 	: bounds.bottom;
//				}

				float percentHgt = height / terrain.terrainData.size.y;

				//Debug.Log(bounds.left +"->"+bounds.right+" :: " + bounds.top+ "->"+bounds.bottom);
				int padding = 50000;

				bounds.left 	-= padding;
				bounds.right 	+= padding;
				bounds.top 		-= padding;
				bounds.bottom 	+= padding;

				int from_i 	= (int) ( (bounds.left/AXGeometry.Utilities.IntPointPrecision)  	* hmWidth / terrain.terrainData.size.x);
				int   to_i 	= (int) ( (bounds.right/AXGeometry.Utilities.IntPointPrecision) 	* hmWidth / terrain.terrainData.size.x );

				int from_j 	= (int) ( (bounds.top/AXGeometry.Utilities.IntPointPrecision)  	* hmHeight / terrain.terrainData.size.z );
				int   to_j 	= (int) ( (bounds.bottom/AXGeometry.Utilities.IntPointPrecision) 	* hmHeight / terrain.terrainData.size.z );

				//Debug.Log(from_i +"->"+to_i+" :: " + from_j+ "->"+to_j);

				from_i = Mathf.Max(0, from_i);
				  to_i = Mathf.Min(to_i, hmWidth);

				from_j = Mathf.Max(0, from_j);
				  to_j = Mathf.Min(to_j, hmHeight);

				//Debug.Log(from_i +"->"+to_i+" :: " + from_j+ "->"+to_j);

				// patch to be redrawn
				float[,] heights = new float[(to_j-from_j), (to_i-from_i)];

				//Debug.Log(heights.GetLength(0) +" -- " + heights.GetLength(1));
				if (from_i < to_i && from_j < to_j)
				{

					// we set each sample of the terrain in the size to the desired height
					for (int i = from_i; i < to_i; i++) {
						for (int j = from_j; j < to_j; j++) {
							// GET i, j in realworld coords
							float x = terrain.terrainData.size.x * i / hmWidth;
							float y = terrain.terrainData.size.z * j / hmHeight;

							//Debug.Log("[" + i + ", " + j + "] :: " + x +", " + y);
							if (isInside (x, y, subjPaths, holePaths)) 
								heights [j-from_j, i-from_i] = percentHgt;
							else
								heights [j-from_j, i-from_i] = parametricObject.heightsOrig [j, i];
						}
					}
					// set the new height
					terrain.terrainData.SetHeights (from_i, from_j, heights);
				}

				//prevBounds = bounds;
			}



		}

		public bool isInside (float i, float j, Paths boundingSolids, Paths boundingHoles)
		{
			bool pointIsInside = false;

			IntPoint ip = new IntPoint (i * AXGeometry.Utilities.IntPointPrecision, j * AXGeometry.Utilities.IntPointPrecision);

			//Debug.Log(ip.X+", "+ip.Y);

			if (boundingSolids != null) {

				if (boundingSolids != null) {
					
					foreach (Path path in boundingSolids) {
						//Pather.printPath(path);
						if (Clipper.PointInPolygon (ip, path) == 1 && Clipper.Orientation (path)) {
							pointIsInside = true;
							break;
						}
					}
				}

				if (boundingHoles != null) {
					foreach (Path hole in boundingHoles) {
						if (Clipper.PointInPolygon (ip, hole) == 1) {
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



		public override Matrix4x4 getLocalConsumerMatrixPerInputSocket (AXParametricObject input, AXParameter input_p = null)
		{
			if (planSrc_po != null) {
				if (input == planSrc_po)
					return Matrix4x4.TRS (Vector3.zero, Quaternion.Euler (90, 0, 0), Vector3.one);
				
				return Matrix4x4.TRS (Vector3.zero, Quaternion.Euler (-90, 0, 0), Vector3.one);
			}
			return Matrix4x4.identity;
		}
	}
}