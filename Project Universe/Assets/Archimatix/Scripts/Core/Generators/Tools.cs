using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
 


/* Organizers combine are ParametricObjects that organize other oParametricObjects. 
 *
 * They tend not to create any splines or meshes of their own, 
 * though they can have summary Parameters and Handles
 * in order to simplify the controls rather than showing all the controls 
 * of the POs they organize.
 *
 *
 */

using AXGeometry;
 


namespace AX.Generators
{


	public interface ITools
	{

	}
	 


	public class AXTool : AX.Generators.Generator, ITools
	{


		public AXTool()
		{
			Init();
		}



		public override void init_parametricObject() 
		{
			base.init_parametricObject();

		}
	}

	public class RepeaterTool : AXTool, ITools
	{

		public override string GeneratorHandlerTypeName { get { return "RepeaterToolHandler"; } }

		public string sizeName;
		public string bayName;
		public string cellCountName;
		public string actualBayName;
		public string edgeCountName;



		// INPUTS
		public AXParameter P_Size;
		public AXParameter P_Bay;
		public AXParameter P_Cells;
		public AXParameter P_ActualBay;
		public AXParameter P_EdgeCount;


		// POLLED MEMBERS
		public float 	size;
		public float 	bay;
		public int   	cells;
		public float 	actualBay;
		public int 		edgeCount;


		public RepeaterTool()
		{
			base.Init();

		}

		public virtual void initParameterNameStrings()
		{
			sizeName 		= "Size";
			bayName 		= "Bay";
			cellCountName 	= "Cells";
			actualBayName 	= "Actual_Bay";
			edgeCountName 	= "Edge_Count";
		}



		// INIT_PARAMETRIC_OBJECT
		public override void init_parametricObject() 
		{

			base.init_parametricObject();

			initParameterNameStrings ();


			// SIZE
			P_Size = parametricObject.addParameter(AXParameter.DataType.Float, 		sizeName, 9f);
			P_Size.expressions.Add (cellCountName+"="+sizeName+"/"+bayName);
			P_Size.expressions.Add (actualBayName+"="+sizeName+"/"+cellCountName);

			// BAY
			P_Bay = parametricObject.addParameter(AXParameter.DataType.Float, 		bayName, 3f, .1f, 1000f);
			P_Bay.expressions.Add (sizeName+"="+bayName+"*"+cellCountName);

			// CELLS
			P_Cells =parametricObject.addParameter(AXParameter.DataType.Int, 		cellCountName, 3, 1, 100);
			P_Cells.expressions.Add (actualBayName+"="+sizeName+"/"+cellCountName);
			P_Cells.expressions.Add (bayName+"="+actualBayName);

			// ACTUAL_BAY
			P_ActualBay = parametricObject.addParameter(AXParameter.DataType.Float,  actualBayName, 3f, .1f, 1000f);
			P_ActualBay.expressions.Add (sizeName+"="+actualBayName+"*"+cellCountName);


			P_EdgeCount = parametricObject.addParameter(AXParameter.DataType.Int,  	edgeCountName, 10);

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.RepeaterTool, AXParameter.ParameterType.Output, "Output"));
		}


		// POLL INPUTS (only on graph change())
		public override void pollInputParmetersAndSetUpLocalReferences()
		{
			initParameterNameStrings ();

			base.pollInputParmetersAndSetUpLocalReferences();

			P_Size 					= parametricObject.getParameter(sizeName);
			P_Bay 					= parametricObject.getParameter(bayName);
			P_Cells 				= parametricObject.getParameter(cellCountName);
			P_ActualBay 			= parametricObject.getParameter(actualBayName);
			P_EdgeCount 			= parametricObject.getParameter(edgeCountName);

		}







		// POLL CONTROLS (every model.generate())
		public override void pollControlValuesFromParmeters()
		{

			if (parametersHaveBeenPolled)
				return;

			base.pollControlValuesFromParmeters();

			if (P_Size != null) {
				P_Size.FloatVal = Math.Max (P_Size.FloatVal, .1f);
				size = P_Size.FloatVal;
			}


			if (P_Bay != null) {
				//P_Bay.FloatVal = Math.Max (P_Bay.FloatVal, .1f);
				bay = P_Bay.FloatVal;
			}


			if (P_Cells != null) {
				P_Cells.IntVal = (int)Math.Max (P_Cells.IntVal, 1);
				cells = P_Cells.IntVal;
			}

			if (P_ActualBay != null) {				
				//P_ActualBay.FloatVal = Math.Max (P_ActualBay.FloatVal, .1f);
				actualBay = P_ActualBay.FloatVal;
			}
			if (P_EdgeCount != null)
			{
				edgeCount = P_EdgeCount.IntVal;
				edgeCount--;
			}



			Bounds b = new Bounds();
			b.size = new Vector3 (size, .1f, .1f);
			b.center = Vector3.zero;
			b.extents = b.size/2;
			parametricObject.bounds = b;
		}


	}


	public class RadialRepeaterTool : RepeaterTool, ITools
	{

		public override string GeneratorHandlerTypeName { get { return "RadialRepeaterToolHandler"; } }


		// INPUTS
		public AXParameter	P_Radius;

		// WORKING FIELDS (Updated every Generate)
		public float 		radius;




		public RadialRepeaterTool()
		{
			base.Init();		
		}


		public override void initParameterNameStrings()
		{
			sizeName 		= "TotalAngle";
			bayName 		= "SectorAngle";
			cellCountName 	= "Sectors";
			actualBayName 	= "ActualSectorAngle";

		}

		// INIT_PARAMETRIC_OBJECT
		public override void init_parametricObject() 
		{
			base.init_parametricObject();


			P_Radius = parametricObject.addParameter(AXParameter.DataType.Float,  	"Radius", 12f, 0f, 5000 );;


			P_Size.FloatVal 		= 360;
			P_Size.min 				= 1;
			P_Size.max				= 360;


			P_Cells.IntVal 			= 12;
			P_Cells.intmin			= 1;
			P_Cells.intmax			= 100;



			P_Bay.FloatVal			= 30;
			P_Bay.min				= 1;
			P_Bay.max				= 360;

			P_ActualBay.FloatVal	= 30;

			parametricObject.parameters.Remove(P_EdgeCount);

		}



		public override void parameterWasModified(AXParameter p)
		{
			switch(p.Name)
			{
			case "TotalAngle":
				if (size > 360) {
					size = 360;
					P_Size.initiatePARAMETER_Ripple_setFloatValueFromGUIChange(size);
				}

				break;
			}
		}

		// POLL INPUTS (only on graph change())
		public override void pollInputParmetersAndSetUpLocalReferences()
		{
			initParameterNameStrings ();

			base.pollInputParmetersAndSetUpLocalReferences();

			P_Radius = parametricObject.getParameter ("Radius");

		}

		// POLL CONTROLS (every model.generate())
		public override void pollControlValuesFromParmeters()
		{
			base.pollControlValuesFromParmeters();

			radius = (P_Radius != null) ? P_Radius.FloatVal : 9;

			if (size > 360) 
			{
				size = 360;
				P_Size.initiatePARAMETER_Ripple_setFloatValueFromGUIChange(size);
			}
		}

	}







	/// <summary>
	/// Material tool.
	/// A MaterialTool can override a an AXParametricObject's  AXMaterial or AXTex, or both.

	/// </summary>

	public class MaterialTool : AXTool, ITools
	{
		public override string GeneratorHandlerTypeName { get { return "MaterialToolHandler"; } }


		// INPUTS
		public AXParameter P_Unified_Scaling;
		public AXParameter P_uScale;
		public AXParameter P_vScale;
		public AXParameter P_uShift;
		public AXParameter P_vShift;
		public AXParameter P_Running_U;
		public AXParameter P_Rot_Sides_Tex;
		public AXParameter P_Rot_Caps_Tex;


		// CONTROLS
		public bool unifiedScaling;





		// Generally speaking, This MaterialTool will defer to a downstream node, group node or model 
		// if the Material is null. Thus, often this node will be about scaling and shifting an inherited material.
		// The physics characteristics of an AX material wil also defer if null or if density is 0, etc.



		public int 				matTextureWidth = 1024;
		public float 			texelsPerUnit   = 256;

		// PHYSICS
		public bool 			physicsOpen; 

	



		// INIT_PARAMETRIC_OBJECT
		public override void init_parametricObject() 
		{
			base.init_parametricObject();


			parametricObject.addParameter(AXParameter.DataType.Bool,  	"Unified_Scaling",  true);
			parametricObject.addParameter(AXParameter.DataType.Float, 	"uScale", 			5f);
			parametricObject.addParameter(AXParameter.DataType.Float, 	"vScale", 			5f);
			parametricObject.addParameter(AXParameter.DataType.Float, 	"uShift", 			0f);
			parametricObject.addParameter(AXParameter.DataType.Float, 	"vShift", 			0f);
			parametricObject.addParameter(AXParameter.DataType.Bool,  	"Running_U",  		true);
			parametricObject.addParameter(AXParameter.DataType.Bool,  	"Rot_Sides_Tex", 	false);
			parametricObject.addParameter(AXParameter.DataType.Bool,  	"Rot_Caps_Tex", 	false);


			parametricObject.addParameter(new AXParameter(AXParameter.DataType.MaterialTool, AXParameter.ParameterType.Output, "Output"));

			// MaterialTool is the only generater whose ParametricObject has an original of axMat and axTex
			parametricObject.axMat = new AXMaterial();
			parametricObject.axTex = new AXTexCoords();
		}

		// POLL INPUTS (only on graph change())
		public override void pollInputParmetersAndSetUpLocalReferences()
		{
			//Debug.Log("Repeater::pollInputParmetersAndSetUpLocalReferences " + parametricObject.Name + " parametersHaveBeenPolled="+parametersHaveBeenPolled);

			base.pollInputParmetersAndSetUpLocalReferences();

			P_Unified_Scaling 	= parametricObject.getParameter("Unified_Scaling");
			P_uScale 			= parametricObject.getParameter("uScale");
			P_vScale 			= parametricObject.getParameter("vScale");
			P_uShift 			= parametricObject.getParameter("uShift");
			P_vShift 			= parametricObject.getParameter("vShift");
			P_Running_U 		= parametricObject.getParameter("Running_U");
			P_Rot_Sides_Tex 	= parametricObject.getParameter("Rot_Sides_Tex");
			P_Rot_Caps_Tex 		= parametricObject.getParameter("Rot_Caps_Tex");
		}
		 

		// INIT_VALUES_FROM_PARAMETRIC_OBJECT
		public override void pollControlValuesFromParmeters()
		{
			if (parametersHaveBeenPolled)
				return;

			base.pollControlValuesFromParmeters();
			 

			unifiedScaling = (P_Unified_Scaling != null) ? P_Unified_Scaling.boolval : true;


			if (parametricObject.axMat == null)
				parametricObject.axMat = new AXMaterial();

			// Is there a legacy material? If so, this is taken care of in AXModel.Enable()
			//Debug.Log(parametricObject.Name+" Legacy Material? " + ((parametricObject.Mat == null) ? "NULL": parametricObject.Mat.name) );

			//if (parametricObject.axMat.mat == null && parametricObject.Mat != null)
			//	parametricObject.axMat.mat = parametricObject.Mat;

			if (parametricObject.axTex == null)
				parametricObject.axTex = new AXTexCoords();

			parametricObject.axTex.scale = new Vector2(P_uScale.FloatVal, P_vScale.FloatVal);
			parametricObject.axTex.shift = new Vector2(P_uShift.FloatVal, P_vShift.FloatVal);


			parametricObject.axTex.runningU = P_Running_U.boolval;
			parametricObject.axTex.rotateSidesTex = P_Rot_Sides_Tex.boolval;
			parametricObject.axTex.rotateCapsTex = (P_Rot_Caps_Tex != null) ? P_Rot_Caps_Tex.boolval :  false;

			/*
			axTex.density = (P_Density != null) ? P_Density.FloatVal : 1;
			if (axTex.density < 0) 
			{
				axTex.density = 0;
				P_Density.FloatVal = axTex.density;
			}
			*/
			 
			setTexelsPerUnit();

		}  

		public void setTexelsPerUnit()
		{			


			if (parametricObject.axMat.mat != null && parametricObject.axMat.mat.HasProperty("_MainTex") && parametricObject.axMat.mat.mainTexture != null && parametricObject.axTex != null && parametricObject.axTex.scale.x != 0)
			{
				matTextureWidth = parametricObject.axMat.mat.mainTexture.width;
				texelsPerUnit = matTextureWidth / parametricObject.axTex.scale.x;
			}
			else 
				texelsPerUnit = 0;
		}
		 
		public override void parameterWasModified(AXParameter p)
		{
			switch(p.Name)
			{
			case "uScale":
				if(P_Unified_Scaling.boolval)
					parametricObject.floatValue("vScale", parametricObject.floatValue("uScale"));
				break;
			case "vScale":
				if(P_Unified_Scaling.boolval)
					parametricObject.floatValue("uScale", parametricObject.floatValue("vScale"));
				break;
			}

			if (parametricObject.axMat.mat != null)
			{
				//Debug.Log("Mat.mainTextureScale.x="+Mat.name + " : " + Mat.mainTextureScale);
				//tex.scale.x /= Mat.mainTextureScale.x;
				//tex.scale.y /= Mat.mainTextureScale.y;
			}
			 

		}

		public int textureSize ()
		{
			if (parametricObject.axMat.mat != null && parametricObject.axMat.mat.HasProperty("_MainTex") && parametricObject.axMat.mat.mainTexture != null)
				return parametricObject.axMat.mat.mainTexture.width;

			return 256;

		}

	}




	public class JitterTool : AXTool, ITools
	{

		// INPUTS
		public AXParameter P_PerlinScale;
		public AXParameter P_Offset;
		public AXParameter P_Jitter_X;
		public AXParameter P_Jitter_Y;
		public AXParameter P_Jitter_Z;

		public float 	perlinScale;
		public float 	offset;
		public float 	x;
		public float 	y;
		public float 	z;


		// INIT_PARAMETRIC_OBJECT
		public override void init_parametricObject() 
		{
			base.init_parametricObject();


			parametricObject.addParameter(AXParameter.DataType.Float, 	"PerlinScale", 1f);

			parametricObject.addParameter(AXParameter.DataType.Float, 	"Offset", 1f);

			parametricObject.addParameter(AXParameter.DataType.Float, 	"Jitter_X", .5f);
			parametricObject.addParameter(AXParameter.DataType.Float, 	"Jitter_Y", .5f);
			parametricObject.addParameter(AXParameter.DataType.Float, 	"Jitter_Z", .5f);

			parametricObject.addParameter(new AXParameter(AXParameter.DataType.JitterTool, AXParameter.ParameterType.Output, "Output"));
		}

		// POLL INPUTS (only on graph change())
		public override void pollInputParmetersAndSetUpLocalReferences()
		{
			if (parametersHaveBeenPolled)
				return;

			base.pollInputParmetersAndSetUpLocalReferences();

			P_PerlinScale 			= parametricObject.getParameter("PerlinScale");
			P_Offset 				= parametricObject.getParameter("Offset");
			P_Jitter_X 				= parametricObject.getParameter("Jitter_X");
			P_Jitter_Y 				= parametricObject.getParameter("Jitter_Y");
			P_Jitter_Z 				= parametricObject.getParameter("Jitter_Z");
		}



		// INIT_VALUES_FROM_PARAMETRIC_OBJECT
		public override void pollControlValuesFromParmeters()
		{

			base.pollControlValuesFromParmeters();

			perlinScale 	= P_PerlinScale.FloatVal;
			offset			= (P_Offset != null) ? P_Offset.FloatVal : 1;
			x 				= P_Jitter_X.FloatVal;
			y 				= P_Jitter_Y.FloatVal;
			z				= P_Jitter_Z.FloatVal;

			if (x < 0)
			{
				x = 0;
				P_Jitter_X.FloatVal = x;
			}
			if (y < 0)
			{
				y = 0;
				P_Jitter_Y.FloatVal = y;
			}
			if (z < 0)
			{
				z = 0;
				P_Jitter_Z.FloatVal = z;
			}

		}


	}




	/*	PlaneTool
	 * 
	 * 
	 */
	public class PlaneTool : AX.Generators.Generator2D, ITools
	{
		public override string GeneratorHandlerTypeName { get { return "PlaneToolHandler"; } }

		public Matrix4x4 matrix;

		// INIT_PARAMETRIC_OBJECT
		public override void init_parametricObject() 
		{
			base.init_parametricObject();


			parametricObject.hasCustomLogic = true;

			parametricObject.addParameter(AXParameter.DataType.Float,  "PointA_X", 0.0f);
			parametricObject.addParameter(AXParameter.DataType.Float,  "PointA_Y", 0.0f);
			parametricObject.addParameter(AXParameter.DataType.Float,  "PointA_Z", 0.0f);

			parametricObject.addParameter(AXParameter.DataType.Float,  "PointB_X", 10.0f);
			parametricObject.addParameter(AXParameter.DataType.Float,  "PointB_Y", 0.0f);
			parametricObject.addParameter(AXParameter.DataType.Float,  "PointB_Z", 0.0f);

			parametricObject.addParameter(AXParameter.DataType.Float,  "PointC_X", 0.0f);
			parametricObject.addParameter(AXParameter.DataType.Float,  "PointC_Y", 5.0f);
			parametricObject.addParameter(AXParameter.DataType.Float,  "PointC_Z", 10.0f);




			parametricObject.addParameter(new AXParameter(AXParameter.DataType.Plane, AXParameter.ParameterType.Output, "Output Plane"));


		}

		// GENERATE PLANE_TOOL
		public override GameObject generate(bool makeGameObjects, AXParametricObject initiator_po, bool isReplica)
		{
			AXParameter 		output 	= parametricObject.getParameter("Output Plane");

			Vector3 a = new Vector3(parametricObject.getParameter("PointA_X").val, parametricObject.getParameter("PointA_Y").val, parametricObject.getParameter("PointA_Z").val);
			Vector3 b = new Vector3(parametricObject.getParameter("PointB_X").val, parametricObject.getParameter("PointB_Y").val, parametricObject.getParameter("PointB_Z").val);
			Vector3 c = new Vector3(parametricObject.getParameter("PointC_X").val, parametricObject.getParameter("PointC_Y").val, parametricObject.getParameter("PointC_Z").val);

			output.plane = new Plane(a, b, c);

			matrix = AXUtilities.Plane2Matrix(output.plane, a);

			/*
			Debug.Log ("a="+a);
			Debug.Log ("b="+b);
			Debug.Log ("c="+c);
			
			Debug.Log ("output.plane "+output.plane.normal);
			*/

			return null;

		}	


	}






}