using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;



using AXGeometry;


using AXClipperLib;
using Path = System.Collections.Generic.List<AXClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<AXClipperLib.IntPoint>>;

using Curve		= System.Collections.Generic.List<AXGeometry.CurveControlPoint2D>;


using LibNoise.Unity;
using LibNoise.Unity.Generator;

using Perlin = LibNoise.Unity.Generator.Perlin;



namespace AX.Generators
{
	public class ShapeNoiser  : AX.Generators.Generator2D, IShape
	{

		public AXParameter	P_Octaves;
		public AXParameter	P_Frequency;
		public AXParameter	P_Persistence;
		public AXParameter	P_Lacunarity;
		public AXParameter	P_Amount;

		public AXParameter	P_CenX;
		public AXParameter	P_CenY;




		// WORKING FIELDS (Updated every Generate)
		// As a best practice, each parameter value could have a local variable
		// That is set before the generate funtion is called.
		// This will allow Handles to acces the parameter values more efficiently.

		public int 			octaves 		= 2;
		public float 		frequency 		= 2f;
		public float 		persistence 	= .5f;
		public float 		lacunarity 		= 1.5f; // 1.5 to 3.5
		public float 		amount 			= 0;

		Vector2 center = Vector2.zero;


		// INITIALIZE
		public override void init_parametricObject() 
		{
			base.init_parametricObject();

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, AXParameter.ParameterType.Input, "Input Shape"));


			// GEOMETRY_CONTROLS
			parametricObject.addParameter(AXParameter.DataType.Float, 	"Amount", 5f);
			parametricObject.addParameter(AXParameter.DataType.Int, 	"Octaves", 2);
			parametricObject.addParameter(AXParameter.DataType.Float, 	"Frequency", 1f);
			parametricObject.addParameter(AXParameter.DataType.Float, 	"Persistence", .5f);
			parametricObject.addParameter(AXParameter.DataType.Float, 	"Lacunarity", 1.5f);

			parametricObject.addParameter(AXParameter.DataType.Float, 	"CenX");
			parametricObject.addParameter(AXParameter.DataType.Float, 	"CenY", 0);





			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Spline, AXParameter.ParameterType.Output, "Output Shape"));


		}



		// POLL INPUTS (only on graph change())
		public override void pollInputParmetersAndSetUpLocalReferences()
		{
			//Debug.Log("Repeater::pollInputParmetersAndSetUpLocalReferences " + parametricObject.Name + " parametersHaveBeenPolled="+parametersHaveBeenPolled);

			base.pollInputParmetersAndSetUpLocalReferences();

			P_Input 					= parametricObject.getParameter("Input Shape");

			P_Octaves 						= parametricObject.getParameter("Octaves");
			P_Frequency 					= parametricObject.getParameter("Frequency");
			P_Persistence 					= parametricObject.getParameter("Persistence");
			P_Lacunarity 					= parametricObject.getParameter("Lacunarity");
			P_Amount 						= parametricObject.getParameter("Amount", "amount");

			P_CenX 							= parametricObject.getParameter("CenX");
			P_CenY 							= parametricObject.getParameter("CenY");



			pollControlValuesFromParmeters();
			 
		}


		// POLL CONTROLS (every model.generate())
		// It is helpful to set the values for parameter variables before generate().
		// These values will be available not only to generate() but also the Handle functions.
		public override void pollControlValuesFromParmeters()
		{
			base.pollControlValuesFromParmeters();



			octaves 		= (P_Octaves  		!= null)  	? P_Octaves.IntVal		: 2;
			frequency 		= (P_Frequency  	!= null)  	? P_Frequency.FloatVal		: 2.0f;
			persistence 	= (P_Persistence  	!= null)  	? P_Persistence.FloatVal	: 0.5f;
			lacunarity 		= (P_Lacunarity  	!= null)  	? P_Lacunarity.FloatVal		: 1.5f;
			amount 			= (P_Amount  		!= null)  	? P_Amount.FloatVal			: 0.0f;


			center.x = (P_CenX  != null)  	? P_CenX.FloatVal	: 0.0f;;
			center.y = (P_CenY  != null)  	? P_CenY.FloatVal	: 0.0f;;
		}



		public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool isReplica)
		{
			//if (ArchimatixUtils.doDebug)
			//Debug.Log (parametricObject.Name + " generate +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");

			//Debug.Log("P_Output="+P_Output);

			if (! parametricObject.isActive)
				return null;

			if (P_Input == null ||  inputSrc_p == null )
				return null;

			if (P_Output == null )
				return null;

			preGenerate();


			Vector2 center = Vector2.zero;

			P_Input.polyTree = null;
			AXShape.thickenAndOffset(ref P_Input, inputSrc_p);

			//Pather.printPaths(P_Input.getPaths());

			P_Output.polyTree = null;

			AXShape.thickenAndOffset(ref P_Output, P_Input);


			P_Output.paths = P_Output.getPaths();

			P_Output.polyTree = null;



			bool planIsClosed 		= (P_Output.hasThickness || P_Output.shapeState == ShapeState.Closed) ? true : false;

			if (P_Output.paths != null && (P_Output.subdivision > 0 || P_Input.subdivision > 0))
			{
					int seglen = Pather.getSegLenBasedOnSubdivision(P_Output.paths, (int) ((P_Input.subdivision > 0) ? P_Input.subdivision : P_Output.subdivision));
				//Debug.Log ("seglen=" + seglen);
				P_Output.paths = Pather.segmentPaths(P_Output.paths,  seglen, planIsClosed);
			}


			Perlin perlin = new Perlin();
			perlin.Frequency = frequency;
			perlin.OctaveCount = octaves;
			perlin.Persistence = persistence;
			perlin.Lacunarity = lacunarity;

		

			for (int i = 0; i < P_Output.paths.Count; i++) {
				Path path = P_Output.paths[i];

				for (int j = 0; j < path.Count; j++) {
					IntPoint ip = path[j];

					Vector2 vert = Pather.IP2Vector2WithPrecision(ip);

					float pval 		= (float) perlin.GetValue(vert);

					Vector2 ray = (vert-center);

					vert = vert + (ray.normalized * pval * amount * .1f);

					path[j] = Pather.Vector2IPWithPrecision(vert);
				}
			}



			calculateBounds();


			return null;
		}


	}

}