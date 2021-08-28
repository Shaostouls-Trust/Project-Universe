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




	
	// RADIAL_REPEATER

	public class RadialRepeater : RepeaterBase, IRepeater
	{
		public override string GeneratorHandlerTypeName { get { return "RadialRepeaterHandler"; } }



		// INPUTS
		public AXParameter P_Rise;
		public AXParameter P_Riser;



		// WORKING FIELDS (Updated every Generate)

		public float rise;
		public float riser;

		 

		// INIT EXTRUDE GENERATOR

		public override void init_parametricObject() 
		{
			base.init_parametricObject();

			// INPUT MESH
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Input, "Node Mesh"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Input, "SpanU Mesh"));

			// MATERIAL
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.MaterialTool, AXParameter.ParameterType.Input, "Material"));

			// JITTER
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.JitterTool,   AXParameter.ParameterType.Input, "Jitter Translation"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.JitterTool,   AXParameter.ParameterType.Input, "Jitter Rotation"));
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.JitterTool,   AXParameter.ParameterType.Input, "Jitter Scale"));

			// REPEATER
			P_RepeaterU = parametricObject.addParameter(new AXParameter(AXParameter.DataType.RepeaterTool, AXParameter.ParameterType.Input, "RepeaterU"));



			P_Rise 	= parametricObject.addParameter(AXParameter.DataType.Float, "Rise", 0f, 0, 5000); 
			P_Riser = parametricObject.addParameter(AXParameter.DataType.Float, "Riser", 0f, .01f, 5000); 

			P_ProgressiveRotationX = parametricObject.addParameter(AXParameter.DataType.Float, 	"IncrRotX", 0f); 
			P_ProgressiveRotationY = parametricObject.addParameter(AXParameter.DataType.Float, 	"IncrRotY", 0f); 
			P_ProgressiveRotationZ = parametricObject.addParameter(AXParameter.DataType.Float, 	"IncrRotZ", 0f); 


			// OUTPUT
			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Mesh, AXParameter.ParameterType.Output,"Output Mesh"));


			// ADD A REPEATER TOOL
			AXParametricObject radialRepeaterTool =  parametricObject.model.createNode("RadialRepeaterTool");
			radialRepeaterTool.rect.x = parametricObject.rect.x-200;
			radialRepeaterTool.isOpen = false;
			P_RepeaterU.makeDependentOn(radialRepeaterTool.getParameter("Output"));
		}


		// POLL INPUTS (only on graph change())
		public override void pollInputParmetersAndSetUpLocalReferences()
		{
			//Debug.Log("Repeater::pollInputParmetersAndSetUpLocalReferences " + parametricObject.Name + " parametersHaveBeenPolled="+parametersHaveBeenPolled);

			base.pollInputParmetersAndSetUpLocalReferences();

			P_Rise = parametricObject.getParameter ("Rise");
			P_Riser = parametricObject.getParameter ("Riser");

		}

		public override void pollControlValuesFromParmeters()
		{

			base.pollControlValuesFromParmeters();
				
			rise =  (P_Rise != null) 	? 	P_Rise.FloatVal : 0;
			riser = (P_Riser != null) 	? 	P_Riser.FloatVal : 0;


		}

		public override bool shouldDoLastItem(RepeaterItem ri)
		{
			bool result = false;
			if (ri == RepeaterItem.Node && repeaterToolU != null && repeaterToolU.size > (360-repeaterToolU.actualBay/8))
					result = false;


			
			return result;

		}




		public override void initializeBays(string pName)
		{
			//Debug.Log ("RADIAL REPEATER initializeBays");
			if (parametricObject.isInitialized)
				return;
			parametricObject.isInitialized = true;


			RadialRepeaterTool gener = repeaterToolU as RadialRepeaterTool;

			//Debug.Log (pName);

			switch(pName)
			{


			case "Node Mesh":
				gener.radius = 3.5f * nodeSrc_p.parametricObject.bounds.size.x ;
				break;
			
			case "SpanU Mesh":
				gener.radius = 2.5f * spanUSrc_p.parametricObject.bounds.size.x ;
				break;
			}

			if (repeaterToolU != null)
				gener.P_Radius.initiatePARAMETER_Ripple_setFloatValueFromGUIChange(gener.radius);




		}







		public override Matrix4x4 localMatrixFromAddress(RepeaterItem ri, int i=0, int j=0, int k=0)
		{
			

			if (repeaterToolU == null)
				return Matrix4x4.identity;

			RadialRepeaterTool gener = repeaterToolU as RadialRepeaterTool;

		


			float perlinTranslation 	= 1;
			float perlinRot 			= 1;
			float perlinScale 			= 1;

			float actualSectorAngle = (repeaterToolU != null) ? repeaterToolU.actualBay : 0;

			float dang = (ri == RepeaterItem.Node) ? (i*actualSectorAngle) : (i*actualSectorAngle + actualSectorAngle/2);



			Matrix4x4 radialDispl = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler (0,-dang,0), Vector3.one) * Matrix4x4.TRS(new Vector3(gener.radius, 0, 0), Quaternion.Euler(0, -90, 0), Vector3.one);


			if (jitterTranslationTool != null)
				perlinTranslation 	=  Mathf.PerlinNoise( (i*actualSectorAngle + jitterTranslationTool.offset) * jitterTranslationTool.perlinScale,  	0);

			if (jitterRotationTool != null)
				perlinRot 			=  Mathf.PerlinNoise( (i*actualSectorAngle + jitterRotationTool.offset) * jitterRotationTool.perlinScale,  		0);

			if (jitterScaleTool != null)
				perlinScale 		=  Mathf.PerlinNoise( (i*actualSectorAngle + jitterScaleTool.offset) * jitterScaleTool.perlinScale,  		0);


			// TRANSLATION 	*********
			Vector3 translate = new Vector3(0, riser*i, 0);


			if (jitterTranslationTool != null)
				translate += new Vector3( perlinTranslation*jitterTranslationTool.x-jitterTranslationTool.x/2 , perlinTranslation*jitterTranslationTool.y-jitterTranslationTool.y/2, perlinTranslation*jitterTranslationTool.z-jitterTranslationTool.z/2);


	
			// ROTATION		*********
			Quaternion 	rotation = Quaternion.Euler (0, 0, 0);
			if (jitterRotationTool != null)
				rotation = Quaternion.Euler ((perlinRot*jitterRotationTool.x - jitterRotationTool.x/2)*1.5f, (perlinRot*jitterRotationTool.y - jitterRotationTool.y/2), (perlinRot*jitterRotationTool.z - jitterRotationTool.z/2)) * rotation;
			
				

			// SCALE		**********
			jitterScale = Vector3.zero;
			if (jitterScaleTool != null)
				jitterScale = new Vector3( perlinScale*jitterScaleTool.x-jitterScaleTool.x/2 , perlinScale*jitterScaleTool.y-jitterScaleTool.y/2, perlinScale*jitterScaleTool.z-jitterScaleTool.z/2);
			

			// ******** USE A FUNCTION HERE TO GET FOR HANDLES AS WELL
			Matrix4x4 dm =  Matrix4x4.TRS(translate, rotation, Vector3.one+jitterScale) * radialDispl * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(i * progressiveRotationX,  90 + i * progressiveRotationY, i * progressiveRotationZ), Vector3.one)  ;

			//Terrain terrain = Terrain.activeTerrain;
			//if (terrain != null)
			//	dm.m13 = translate.y + terrain.SampleHeight(parametricObject.model.gameObject.transform.TransformPoint(parametricObject.getLocalMatrix().MultiplyPoint(AXUtilities.GetPosition(dm))));
			

			return dm;

		}

		
		

	}






}
